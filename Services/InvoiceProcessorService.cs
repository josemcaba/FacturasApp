using FacturasApp.Models;
using FacturasApp.Services.Parsers;
using System.IO;
using System.Linq;

namespace FacturasApp.Services
{
    public class InvoiceProcessorService
    {
        private readonly PdfTextExtractor _textExtractor = new();
        private readonly OcrExtractor _ocrExtractor;
        private readonly OcrZonalExtractor _ocrZonalExtractor;
        private readonly ParserFactory _parserFactory = new();
        private readonly ExcelExtractor _excelExtractor = new();
        private readonly PlantillaOcrService _plantillaService = new();

        public PdfTextExtractor.ModoExtraccion ModoExtraccion { get; set; } =
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

        public InvoiceProcessorService(string tessDataPath = @"./tessdata")
        {
            _ocrExtractor = new OcrExtractor(tessDataPath);
            _ocrZonalExtractor = new OcrZonalExtractor(tessDataPath);
        }

        // ── Procesado de PDFs por lotes ──────────────────────────────────────

        public List<Factura> ProcesarLote(
            IEnumerable<string> rutasPdf,
            IProgress<(int actual, int total, string archivo)>? progreso = null)
        {
            var rutas = rutasPdf.ToList();
            var facturas = new List<Factura>();

            for (int i = 0; i < rutas.Count; i++)
            {
                string ruta = rutas[i];
                progreso?.Report((i + 1, rutas.Count, Path.GetFileName(ruta)));

                try
                {
                    // Primera extracción en modo Simple para identificar el emisor
                    string? textoIdentificacion =
                        _textExtractor.ExtraerTextoSeleccionable(ruta,
                            PdfTextExtractor.ModoExtraccion.Simple);

                    bool usaOcr = textoIdentificacion == null;

                    if (usaOcr)
                    {
                        // PDF escaneado — intentamos OCR zonal primero
                        string textoOcrZonal = ExtraerTextoOcrZonal(ruta);
                        
                        // Si no hay plantilla definida, usamos OCR completo
                        if (string.IsNullOrEmpty(textoOcrZonal))
                            textoOcrZonal = _ocrExtractor.ExtraerTextoConOcr(ruta);

                        IInvoiceParser parserOcr =
                            _parserFactory.ObtenerParser(textoOcrZonal);

                        // ParsearMultiple también en flujo OCR
                        List<Factura> facturasOcr = parserOcr is BaseParser baseParserOcr
                            ? baseParserOcr.ParsearMultiple(textoOcrZonal, ruta, true)
                            : new List<Factura> { parserOcr.Parsear(textoOcrZonal, ruta, true) };

                        AddWithDuplicateDetection(facturas, facturasOcr);
                    }
                    else
                    {
                        // PDF seleccionable — identificamos el emisor primero
                        IInvoiceParser parser =
                            _parserFactory.ObtenerParser(textoIdentificacion!);

                        // Reextracción con el modo preferido del parser
                        string textoFinal = parser.ModoExtraccion ==
                            PdfTextExtractor.ModoExtraccion.Simple
                            ? textoIdentificacion!
                            : _textExtractor.ExtraerTextoSeleccionable(ruta,
                                  parser.ModoExtraccion) ?? textoIdentificacion!;

                        // ParsearMultiple devuelve 1 o N facturas según el parser
                        List<Factura> nuevasFacturas = parser is BaseParser baseParser
                            ? baseParser.ParsearMultiple(textoFinal, ruta, false)
                            : new List<Factura> { parser.Parsear(textoFinal, ruta, false) };

                        AddWithDuplicateDetection(facturas, nuevasFacturas);
                    }
                }
                catch (Exception ex)
                {
                    facturas.Add(new Factura
                    {
                        RutaArchivo = ruta,
                        Estado = EstadoFactura.Error,
                        ErrorMensaje = ex.Message
                    });
                }
            }

            return facturas;
        }

        // ── OCR Zonal: extrae texto de las zonas definidas en la plantilla ────

        /// <summary>
        /// Intenta extraer texto usando OCR zonal basado en las plantillas definidas.
        /// Retorna el texto concatenado de todas las zonas, o string.Empty si no hay plantilla.
        /// </summary>
        private string ExtraerTextoOcrZonal(string rutaPdf)
        {
            try
            {
                // Intentamos identificar el emisor primero
                // Para esto, podrías usar OCR completo rápido o un patrón simple
                string textoIdentificacion = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                IInvoiceParser parser = _parserFactory.ObtenerParser(textoIdentificacion);
                string nombreEmisor = parser.Nombre;

                // Buscamos la plantilla definida para este emisor
                var plantilla = _plantillaService.ObtenerPorEmisor(nombreEmisor);
                if (plantilla == null || plantilla.Zonas.Count == 0)
                    return string.Empty;

                // Extraemos OCR zonal
                var textosZonas = _ocrZonalExtractor.ExtraerZonas(rutaPdf, plantilla);

                // Concatenamos los textos de todas las zonas
                var textoFinal = new System.Text.StringBuilder();
                foreach (var zona in plantilla.Zonas)
                {
                    if (textosZonas.TryGetValue(zona.Campo, out string? textoZona))
                        textoFinal.AppendLine(textoZona);
                }

                return textoFinal.ToString().Trim();
            }
            catch
            {
                // Si algo falla en OCR zonal, retornamos vacío para fallback a OCR completo
                return string.Empty;
            }
        }

        // Helper: añade nuevas facturas a la lista comprobando duplicados por Número de factura
        private void AddWithDuplicateDetection(List<Factura> acumuladas, IEnumerable<Factura> nuevas)
        {
            foreach (var nueva in nuevas)
            {
                var numero = (nueva.NumeroFactura ?? string.Empty).Trim();
                string rutaNueva = (nueva.RutaArchivo ?? string.Empty).Trim();
                string rutaNuevaFull = string.IsNullOrEmpty(rutaNueva)
                    ? string.Empty
                    : Path.GetFullPath(rutaNueva).ToUpperInvariant();

                if (!string.IsNullOrEmpty(numero))
                {
                    var existente = acumuladas.FirstOrDefault(f =>
                    {
                        var fNumero = (f.NumeroFactura ?? string.Empty).Trim();
                        if (!string.Equals(fNumero, numero, StringComparison.OrdinalIgnoreCase))
                            return false;

                        string rutaExistente = (f.RutaArchivo ?? string.Empty).Trim();
                        string rutaExistenteFull = string.IsNullOrEmpty(rutaExistente)
                            ? string.Empty
                            : Path.GetFullPath(rutaExistente).ToUpperInvariant();

                        return !string.Equals(rutaExistenteFull, rutaNuevaFull, StringComparison.OrdinalIgnoreCase);
                    });

                    if (existente != null)
                    {
                        nueva.Estado = EstadoFactura.Duplicada;
                        nueva.ErrorMensaje = $"Factura duplicada. Existe en: {existente.RutaArchivo}";

                        if (existente.Estado != EstadoFactura.Duplicada)
                        {
                            existente.Estado = EstadoFactura.Duplicada;
                            existente.ErrorMensaje = $"Factura duplicada. Otra copia: {nueva.RutaArchivo}";
                        }

                        acumuladas.Add(nueva);
                        continue;
                    }
                }

                acumuladas.Add(nueva);
            }
        }

        // ── Importación desde Excel ──────────────────────────────────────────

        public List<Factura> ImportarDesdeExcel(string rutaExcel)
        {
            return _excelExtractor.ImportarDesdeExcel(rutaExcel);
        }

        public List<string> ObtenerColumnasNoReconocidas(string rutaExcel)
        {
            return _excelExtractor.ObtenerColumnasNoReconocidas(rutaExcel);
        }

        // ── Procesado mixto (PDFs + Excel juntos) ────────────────────────────

        public List<Factura> ProcesarMixto(
            IEnumerable<string> rutasPdf,
            string? rutaExcel = null,
            IProgress<(int actual, int total, string archivo)>? progreso = null)
        {
            var facturas = new List<Factura>();

            if (rutasPdf.Any())
                facturas.AddRange(ProcesarLote(rutasPdf, progreso));

            if (!string.IsNullOrEmpty(rutaExcel) && File.Exists(rutaExcel))
                facturas.AddRange(ImportarDesdeExcel(rutaExcel));

            return facturas;
        }

        // ── Información de parsers disponibles ───────────────────────────────

        public IReadOnlyList<string> ParsersDisponibles =>
            _parserFactory.ParsersDisponibles;

    }
}