using FacturasApp.Models;
using FacturasApp.Models.EmisoresConfig;
using FacturasApp.Services.Parsers;

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

        // Configuración de tolerancia para extracción zonal
        public bool UsarZonasSiempre { get; set; } = true;
        public bool FallbackATextoCompleto { get; set; } = true;

        public InvoiceProcessorService(string tessDataPath = @"./tessdata")
        {
            _ocrExtractor = new OcrExtractor(tessDataPath);
            _ocrZonalExtractor = new OcrZonalExtractor(tessDataPath);
        }

        // ── Procesado de PDFs por lotes (VERSIÓN UNIFICADA) ───────────────────

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
                    // ✅ Ahora recibe múltiples facturas
                    List<Factura> facturasDelPdf = ProcesarUnPdf(ruta);
                    if (facturasDelPdf.Any())
                        AddWithDuplicateDetection(facturas, facturasDelPdf);
                }
                catch (Exception ex)
                {
                    facturas.Add(new Factura
                    {
                        RutaArchivo = ruta,
                        Estado = EstadoFactura.Error,
                        MensajeError = new List<string> { ex.Message }
                    });
                }
            }

            return facturas;
        }

        /// <summary>
        /// Identifica el emisor de un PDF usando el mismo procedimiento que en el
        /// flujo normal: extrae texto rápido (u OCR si está escaneado) y delega
        /// en ParserFactory.ObtenerParser.
        /// </summary>
        public IInvoiceParser IdentificarEmisor(string rutaPdf) =>
            IdentificarEmisor(rutaPdf, _textExtractor.ExtraerTextoSeleccionable(rutaPdf,
                PdfTextExtractor.ModoExtraccion.Simple));

        private IInvoiceParser IdentificarEmisor(string rutaPdf, string? textoRapido) =>
            textoRapido != null
                ? _parserFactory.ObtenerParser(textoRapido)
                : _parserFactory.ObtenerParser(_ocrExtractor.ExtraerTextoIdentificacion(rutaPdf));

        /// <summary>
        /// Extrae el texto de un PDF usando el pipeline completo
        /// (detección, identificación de emisor, extracción zonal, fallback).
        /// Equivale a los pasos 1-4 de ProcesarUnPdf.
        /// </summary>
        public string ExtraerTexto(string rutaPdf)
        {
            var (texto, _, _, _, _) = ExtraerTextoCompleto(rutaPdf);
            return texto;
        }

        /// <summary>
        /// Pasos 1-4 del pipeline: extrae texto completo + metadatos del parser.
        /// </summary>
        private (string textoExtraido, IInvoiceParser parser, bool usarOcr, bool extraccionZonalExitosa, bool esPdfSeleccionable) ExtraerTextoCompleto(string rutaPdf)
        {
            // ── PASO 1: Detectar tipo de PDF ──────────────────────────────────
            string? textoRapido = _textExtractor.ExtraerTextoSeleccionable(rutaPdf,
                PdfTextExtractor.ModoExtraccion.Simple);

            bool esPdfSeleccionable = textoRapido != null;
            bool usarOcr = !esPdfSeleccionable;

            // ── PASO 2: Identificar emisor (con método rápido) ──────────────
            IInvoiceParser parser = IdentificarEmisor(rutaPdf, textoRapido);

            string nombreEmisor = parser.Nombre;

            // ── PASO 3: Intentar extracción por zonas (si hay plantilla) ──────
            string textoExtraido = "";
            bool extraccionZonalExitosa = false;
            PlantillaOcr? plantilla = ObtenerPlantilla(parser, nombreEmisor);

            if (UsarZonasSiempre && plantilla != null && plantilla.Zonas.Any())
            {
                if (esPdfSeleccionable)
                {
                    textoExtraido = ExtraerTextoZonalDesdePdf(rutaPdf, plantilla);
                    extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
                }
                else
                {
                    textoExtraido = ExtraerTextoOcrZonalConPlantilla(rutaPdf, plantilla);
                    extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
                }

                if (extraccionZonalExitosa)
                    System.Diagnostics.Debug.WriteLine($"✓ Extracción zonal exitosa para {nombreEmisor}");
                else
                    System.Diagnostics.Debug.WriteLine($"✗ Extracción zonal falló para {nombreEmisor}, usando fallback");
            }

            // ── PASO 4: Fallback a extracción completa si la zonal falló ──────
            if (!extraccionZonalExitosa && FallbackATextoCompleto)
            {
                if (esPdfSeleccionable)
                {
                    textoExtraido = parser.ModoExtraccion == PdfTextExtractor.ModoExtraccion.Simple
                        ? textoRapido!
                        : _textExtractor.ExtraerTextoSeleccionable(rutaPdf, parser.ModoExtraccion)
                          ?? textoRapido!;
                }
                else
                {
                    textoExtraido = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                }
            }

            return (textoExtraido, parser, usarOcr, extraccionZonalExitosa, esPdfSeleccionable);
        }

        /// <summary>
        /// Procesa un único PDF usando la estrategia unificada:
        /// 1. Detecta si tiene texto seleccionable o es escaneado
        /// 2. Identifica el emisor
        /// 3. Intenta extracción por zonas (si existe plantilla)
        /// 4. Fallback a texto completo si no hay plantilla o falló
        /// 5. Parseo con el parser correspondiente
        /// 
        /// Retorna una LISTA de facturas (puede ser múltiples si hay varias líneas de IVA).
        /// </summary>
        private List<Factura> ProcesarUnPdf(string rutaPdf)
        {
            var (textoExtraido, parser, usarOcr, extraccionZonalExitosa, esPdfSeleccionable) = ExtraerTextoCompleto(rutaPdf);

            // ── PASO 5: Parsear el texto extraído ──────────────────────────────
            if (string.IsNullOrEmpty(textoExtraido))
                throw new InvalidOperationException("No se pudo extraer texto del PDF");

            List<Factura> facturasParseadas = parser is BaseParser baseParser
                ? baseParser.ParsearMultiple(textoExtraido, rutaPdf, usarOcr)
                : new List<Factura> { parser.Parsear(textoExtraido, rutaPdf, usarOcr) };

            foreach (var factura in facturasParseadas)
            {
                if (extraccionZonalExitosa)
                {
                    factura.MensajeError ??= new List<string>();
                    factura.MensajeError.Add($"Extracción zonal utilizada ({(esPdfSeleccionable ? "coordenadas" : "OCR zonal")})");
                }
            }

            return facturasParseadas;
        }

        public List<Factura> ProcesarEmisorMuestra(EmisorConfig config, string rutaPdf)
        {
            var parser = new ConfigurableParserEngine(config);

            // ── PASO 1: Detectar tipo de PDF ──────────────────────────────────
            string? textoRapido = _textExtractor.ExtraerTextoSeleccionable(rutaPdf,
                PdfTextExtractor.ModoExtraccion.Simple);

            bool esPdfSeleccionable = textoRapido != null;
            bool usarOcr = !esPdfSeleccionable;

            // ── PASO 2: Extracción zonal (solo para el emisor indicado) ───────
            string textoExtraido = "";
            bool extraccionZonalExitosa = false;
            PlantillaOcr? plantilla = ObtenerPlantilla(parser, parser.Nombre);

            if (UsarZonasSiempre && plantilla != null && plantilla.Zonas.Any())
            {
                if (esPdfSeleccionable)
                    textoExtraido = ExtraerTextoZonalDesdePdf(rutaPdf, plantilla);
                else
                    textoExtraido = ExtraerTextoOcrZonalConPlantilla(rutaPdf, plantilla);

                extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
            }

            // ── PASO 3: Fallback a texto completo ──────────────────────────────
            if (!extraccionZonalExitosa && FallbackATextoCompleto)
            {
                if (esPdfSeleccionable)
                {
                    textoExtraido = parser.ModoExtraccion == PdfTextExtractor.ModoExtraccion.Simple
                        ? textoRapido!
                        : _textExtractor.ExtraerTextoSeleccionable(rutaPdf, parser.ModoExtraccion)
                          ?? textoRapido!;
                }
                else
                {
                    textoExtraido = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                }
            }

            // ── PASO 4: Parsear ────────────────────────────────────────────────
            if (string.IsNullOrEmpty(textoExtraido))
                return new List<Factura>();

            var facturas = parser.ParsearMultiple(textoExtraido, rutaPdf, usarOcr);

            foreach (var factura in facturas)
            {
                if (extraccionZonalExitosa)
                {
                    factura.MensajeError ??= new List<string>();
                    factura.MensajeError.Add($"Extracción zonal utilizada ({(esPdfSeleccionable ? "coordenadas" : "OCR zonal")})");
                }
            }

            return facturas;
        }

        // ── Selección de plantilla de zonas OCR ─────────────────────────────────

        private PlantillaOcr? ObtenerPlantilla(IInvoiceParser parser, string nombreEmisor)
        {
            if (parser is ConfigurableParserEngine xmlParser)
            {
                var zonas = xmlParser.Config.ZonasOcr;
                if (zonas == null || zonas.Count == 0)
                    return null;

                return new PlantillaOcr
                {
                    Emisor = nombreEmisor,
                    Zonas = zonas.Select(z => new ZonaOcr
                    {
                        Campo = z.Campo,
                        NumPagina = z.NumPagina,
                        X = z.X,
                        Y = z.Y,
                        Ancho = z.Ancho,
                        Alto = z.Alto,
                        Preprocesamiento = z.Preprocesamiento ?? new PreprocesamientoOcr()
                    }).ToList()
                };
            }

            return _plantillaService.ObtenerPorEmisor(nombreEmisor);
        }

        // ── Extracción zonal para PDFs con texto seleccionable ───────────────────

        /// <summary>
        /// Extrae texto de zonas específicas en un PDF con texto seleccionable
        /// usando coordenadas (sin OCR).
        /// </summary>
        private string ExtraerTextoZonalDesdePdf(string rutaPdf, PlantillaOcr plantilla)
        {
            try
            {
                var textosZonas = _textExtractor.ExtraerZonasTexto(rutaPdf, plantilla);

                var textoFinal = new System.Text.StringBuilder();
                foreach (var zona in plantilla.Zonas)
                {
                    if (textosZonas.TryGetValue(zona.Campo, out string? textoZona))
                    {
                        if (!string.IsNullOrWhiteSpace(textoZona))
                            textoFinal.AppendLine($"[{zona.Campo}]: {textoZona}");
                    }
                }

                string resultado = textoFinal.ToString().Trim();

                // Verificar si al menos una zona no vacía fue extraída
                if (resultado.Length < 10)  // Umbral mínimo de texto
                    return string.Empty;

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ExtraerTextoZonalDesdePdf: {ex.Message}");
                return string.Empty;
            }
        }

        // ── Extracción zonal para PDFs escaneados (OCR) ──────────────────────────

        /// <summary>
        /// Extrae texto de zonas específicas en un PDF escaneado usando OCR.
        /// Acepta la plantilla ya obtenida para evitar buscarla dos veces.
        /// </summary>
        private string ExtraerTextoOcrZonalConPlantilla(string rutaPdf, PlantillaOcr plantilla)
        {
            try
            {
                if (plantilla == null || plantilla.Zonas.Count == 0)
                    return string.Empty;

                // Extraer OCR zonal
                var textosZonas = _ocrZonalExtractor.ExtraerZonas(rutaPdf, plantilla);

                var textoFinal = new System.Text.StringBuilder();
                foreach (var zona in plantilla.Zonas)
                {
                    if (textosZonas.TryGetValue(zona.Campo, out string? textoZona))
                    {
                        if (!string.IsNullOrWhiteSpace(textoZona))
                            textoFinal.AppendLine($"[{zona.Campo}]: {textoZona}");
                    }
                }

                string resultado = textoFinal.ToString().Trim();

                if (resultado.Length < 10)
                    return string.Empty;

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ExtraerTextoOcrZonalConPlantilla: {ex.Message}");
                return string.Empty;
            }
        }

        // ── Helper: detección de duplicados ─────────────────────────────────────

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
                        nueva.MensajeError ??= new List<string>();
                        nueva.MensajeError.Add($"Factura duplicada. Existe en: {existente.RutaArchivo}");

                        if (nueva.Estado == EstadoFactura.OK)
                            nueva.Estado = EstadoFactura.Duplicada;

                        if (existente.Estado != EstadoFactura.Duplicada)
                        {
                            existente.MensajeError ??= new List<string>();
                            existente.MensajeError.Add($"Factura duplicada. Otra copia: {nueva.RutaArchivo}");

                            if (existente.Estado == EstadoFactura.OK)
                                existente.Estado = EstadoFactura.Duplicada;
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

        // ── Utilidades para debugging y diagnóstico ──────────────────────────

        /// <summary>
        /// Diagnóstico: muestra qué método se usaría para un PDF sin procesarlo
        /// </summary>
        public string DiagnosticarPdf(string rutaPdf)
        {
            var sb = new System.Text.StringBuilder();

            bool tieneTexto = _textExtractor.EsSeleccionable(rutaPdf);
            sb.AppendLine($"PDF: {Path.GetFileName(rutaPdf)}");
            sb.AppendLine($"Texto seleccionable: {(tieneTexto ? "SÍ" : "NO")}");

            string textoRapido = tieneTexto
                ? _textExtractor.ExtraerTextoSeleccionable(rutaPdf, PdfTextExtractor.ModoExtraccion.Simple) ?? ""
                : _ocrExtractor.ExtraerTextoConOcr(rutaPdf);

            var parser = _parserFactory.ObtenerParser(textoRapido);
            sb.AppendLine($"Emisor detectado: {parser.Nombre}");

            var plantilla = _plantillaService.ObtenerPorEmisor(parser.Nombre);
            sb.AppendLine($"Plantilla de zonas disponible: {(plantilla != null ? $"SÍ ({plantilla.Zonas.Count} zonas)" : "NO")}");

            if (UsarZonasSiempre && plantilla != null)
            {
                sb.AppendLine($"Se intentará extracción ZONAL ({(tieneTexto ? "coordenadas" : "OCR zonal")})");
            }
            else if (FallbackATextoCompleto)
            {
                sb.AppendLine($"Se usará extracción COMPLETA ({(tieneTexto ? "texto nativo" : "OCR completo")})");
            }

            return sb.ToString();
        }
    }
}