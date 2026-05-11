using FacturasApp.Models;
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

        public PdfTextExtractor.ModoExtraccion ModoExtraccion { get; set; } =
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

        // Configuración de tolerancia para extracción zonal
        public bool UsarZonasSiempre { get; set; } = true;  // Si false, usa zonalsolo si hay plantilla
        public bool FallbackATextoCompleto { get; set; } = true;  // Si zona falla, usar texto completo

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
                    Factura? factura = ProcesarUnPdf(ruta);
                    if (factura != null)
                        AddWithDuplicateDetection(facturas, new List<Factura> { factura });
                }
                catch (Exception ex)
                {
                    facturas.Add(new Factura
                    {
                        RutaArchivo = ruta,
                        Estado = _Estado.Error,
                        MensajeError = new List<string> { ex.Message }
                    });
                }
            }

            return facturas;
        }

        /// <summary>
        /// Procesa un único PDF usando la estrategia unificada:
        /// 1. Detecta si tiene texto seleccionable o es escaneado
        /// 2. Identifica el emisor
        /// 3. Intenta extracción por zonas (si existe plantilla)
        /// 4. Fallback a texto completo si no hay plantilla o falló
        /// 5. Parseo con el parser correspondiente
        /// </summary>
        private Factura? ProcesarUnPdf(string rutaPdf)
        {
            // ── PASO 1: Detectar tipo de PDF ─────────────────────────────────────
            string? textoRapido = _textExtractor.ExtraerTextoSeleccionable(rutaPdf,
                PdfTextExtractor.ModoExtraccion.Simple);

            bool esPdfSeleccionable = textoRapido != null;
            bool usarOcr = !esPdfSeleccionable;

            // ── PASO 2: Identificar emisor (con método rápido) ───────────────────
            string textoIdentificacion;
            IInvoiceParser parser;
            string nombreEmisor;

            if (esPdfSeleccionable)
            {
                // PDF nativo: usamos texto rápido para identificar
                textoIdentificacion = textoRapido!;
                parser = _parserFactory.ObtenerParser(textoIdentificacion);
                nombreEmisor = parser.Nombre;
            }
            else
            {
                // PDF escaneado: OCR rápido solo para identificar emisor
                textoIdentificacion = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                parser = _parserFactory.ObtenerParser(textoIdentificacion);
                nombreEmisor = parser.Nombre;
            }

            // ── PASO 3: Intentar extracción por zonas (si hay plantilla) ─────────
            string textoExtraido = "";
            bool extraccionZonalExitosa = false;
            PlantillaOcr? plantilla = _plantillaService.ObtenerPorEmisor(nombreEmisor);

            if (UsarZonasSiempre && plantilla != null && plantilla.Zonas.Any())
            {
                if (esPdfSeleccionable)
                {
                    // PDF con texto nativo: extracción por coordenadas
                    textoExtraido = ExtraerTextoZonalDesdePdf(rutaPdf, plantilla);
                    extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
                }
                else
                {
                    // PDF escaneado: OCR zonal
                    textoExtraido = ExtraerTextoOcrZonalConPlantilla(rutaPdf, plantilla);
                    extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
                }

                // Log para debugging (opcional)
                if (extraccionZonalExitosa)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ Extracción zonal exitosa para {nombreEmisor}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Extracción zonal falló para {nombreEmisor}, usando fallback");
                }
            }

            // ── PASO 4: Fallback a extracción completa si la zonal falló ─────────
            if (!extraccionZonalExitosa && FallbackATextoCompleto)
            {
                if (esPdfSeleccionable)
                {
                    // Reextraer con el modo preferido del parser
                    textoExtraido = parser.ModoExtraccion == PdfTextExtractor.ModoExtraccion.Simple
                        ? textoRapido!
                        : _textExtractor.ExtraerTextoSeleccionable(rutaPdf, parser.ModoExtraccion)
                          ?? textoRapido!;
                }
                else
                {
                    // OCR completo
                    textoExtraido = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                }
            }

            // ── PASO 5: Parsear el texto extraído ─────────────────────────────────
            if (string.IsNullOrEmpty(textoExtraido))
                throw new InvalidOperationException("No se pudo extraer texto del PDF");

            List<Factura> facturasParseadas = parser is BaseParser baseParser
                ? baseParser.ParsearMultiple(textoExtraido, rutaPdf, usarOcr)
                : new List<Factura> { parser.Parsear(textoExtraido, rutaPdf, usarOcr) };

            // Marcar metadata sobre el método de extracción usado
            foreach (var factura in facturasParseadas)
            {
                if (extraccionZonalExitosa)
                {
                    factura.MensajeError ??= new List<string>();
                    factura.MensajeError.Add($"Extracción zonal utilizada ({(esPdfSeleccionable ? "coordenadas" : "OCR zonal")})");
                }
            }

            return facturasParseadas.FirstOrDefault();
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
                if (resultado.Length < 10)  // Umbleral mínimo de texto
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

        // ── Método legacy (mantener por compatibilidad, pero usar el nuevo flujo) ─

        /// <summary>
        /// Método legacy para compatibilidad. Usa ProcesarUnPdf internamente.
        /// </summary>
        [Obsolete("Usar ProcesarLote que unifica ambos flujos")]
        private string ExtraerTextoOcrZonal(string rutaPdf)
        {
            try
            {
                // Para compatibilidad, identificamos emisor y buscamos plantilla
                string textoIdentificacion = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                IInvoiceParser parser = _parserFactory.ObtenerParser(textoIdentificacion);
                string nombreEmisor = parser.Nombre;

                var plantilla = _plantillaService.ObtenerPorEmisor(nombreEmisor);
                if (plantilla == null || plantilla.Zonas.Count == 0)
                    return string.Empty;

                return ExtraerTextoOcrZonalConPlantilla(rutaPdf, plantilla);
            }
            catch
            {
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
                        nueva.Estado = _Estado.Duplicada;
                        nueva.MensajeError ??= new List<string>();
                        nueva.MensajeError.Add($"Factura duplicada. Existe en: {existente.RutaArchivo}");

                        if (existente.Estado != _Estado.Duplicada)
                        {
                            existente.Estado = _Estado.Duplicada;
                            existente.MensajeError ??= new List<string>();
                            existente.MensajeError.Add($"Factura duplicada. Otra copia: {nueva.RutaArchivo}");
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