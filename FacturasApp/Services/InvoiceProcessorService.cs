using CsvHelper;
using FacturasApp.Models;
using FacturasApp.Services.Parsers;
using System.Runtime.Intrinsics.X86;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
                        IInvoiceParser parserOcr = _parserFactory.ObtenerParser(textoOcr);
                        Factura facturaOcr = parserOcr.Parsear(textoOcr, ruta, true);

                        facturas.Add(facturaOcr);
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

                        Factura factura = parser.Parsear(textoFinal, ruta, false);

                        facturas.Add(factura);
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

        // ── Importación desde Excel ──────────────────────────────────────────

        public List<Factura> ImportarDesdeExcel(string rutaExcel)
        {
            // Primero comprobamos si hay un parser específico para este Excel
            var (esEspecifico, parserEspecifico) =
                _excelParserFactory.ObtenerParser(rutaExcel);

            if (esEspecifico && parserEspecifico != null)
                return parserEspecifico.Parsear(rutaExcel);

            // Si no, usamos el extractor genérico
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