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
        private readonly ParserFactory _parserFactory = new();
        private readonly ExcelExtractor _excelExtractor = new();
        private readonly ExcelParserFactory _excelParserFactory = new();

        public PdfTextExtractor.ModoExtraccion ModoExtraccion { get; set; } =
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

        public InvoiceProcessorService(string tessDataPath = @"./tessdata")
        {
            _ocrExtractor = new OcrExtractor(tessDataPath);
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
                        // PDF escaneado — usamos OCR directamente
                        string textoOcr = _ocrExtractor.ExtraerTextoConOcr(ruta);
                        IInvoiceParser parserOcr =
                            _parserFactory.ObtenerParser(textoOcr);

                        // ParsearMultiple también en flujo OCR
                        List<Factura> facturasOcr = parserOcr is BaseParser baseParserOcr
                            ? baseParserOcr.ParsearMultiple(textoOcr, ruta, true)
                            : new List<Factura> { parserOcr.Parsear(textoOcr, ruta, true) };

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

        // Helper: añade nuevas facturas a la lista comprobando duplicados por Número de factura
        // Se consideran duplicadas sólo si tienen el mismo número y pertenecen a archivos diferentes.
        private void AddWithDuplicateDetection(List<Factura> acumuladas, IEnumerable<Factura> nuevas)
        {
            foreach (var nueva in nuevas)
            {
                // Normalizamos el número de factura para comparación
                var numero = (nueva.NumeroFactura ?? string.Empty).Trim();
                // Normalizamos ruta (full path, uppercase) para comparación segura
                string rutaNueva = (nueva.RutaArchivo ?? string.Empty).Trim();
                string rutaNuevaFull = string.IsNullOrEmpty(rutaNueva)
                    ? string.Empty
                    : Path.GetFullPath(rutaNueva).ToUpperInvariant();

                if (!string.IsNullOrEmpty(numero))
                {
                    // Buscamos una factura existente con el mismo número pero que pertenezca a un archivo distinto
                    var existente = acumuladas.FirstOrDefault(f =>
                    {
                        var fNumero = (f.NumeroFactura ?? string.Empty).Trim();
                        if (!string.Equals(fNumero, numero, StringComparison.OrdinalIgnoreCase))
                            return false;

                        string rutaExistente = (f.RutaArchivo ?? string.Empty).Trim();
                        string rutaExistenteFull = string.IsNullOrEmpty(rutaExistente)
                            ? string.Empty
                            : Path.GetFullPath(rutaExistente).ToUpperInvariant();

                        // Consider duplicates only when file paths differ
                        return !string.Equals(rutaExistenteFull, rutaNuevaFull, StringComparison.OrdinalIgnoreCase);
                    });

                    if (existente != null)
                    {
                        // Marcar la nueva como duplicada y añadir un mensaje
                        nueva.Estado = EstadoFactura.Duplicada;
                        nueva.ErrorMensaje = $"Factura duplicada. Existe en: {existente.RutaArchivo}";

                        // También marcar la factura existente como duplicada si aún no lo está
                        if (existente.Estado != EstadoFactura.Duplicada)
                        {
                            existente.Estado = EstadoFactura.Duplicada;
                            existente.ErrorMensaje = $"Factura duplicada. Otra copia: {nueva.RutaArchivo}";
                        }

                        acumuladas.Add(nueva);
                        continue;
                    }
                }

                // Si no es duplicada (o no hay número), añadir normalmente
                acumuladas.Add(nueva);
            }
        }

        // ── Importación desde Excel ──────────────────────────────────────────

        public List<Factura> ImportarDesdeExcel(string rutaExcel)
        {
            var (esEspecifico, parserEspecifico) =
                _excelParserFactory.ObtenerParser(rutaExcel);

            if (esEspecifico && parserEspecifico != null)
                return parserEspecifico.Parsear(rutaExcel);

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

        public IReadOnlyList<string> ParsersExcelDisponibles =>
            _excelParserFactory.ParsersDisponibles;
    }
}