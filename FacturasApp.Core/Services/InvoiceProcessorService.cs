using FacturasApp.Core.Models;
using FacturasApp.Core.Models.EmisoresConfig;
using FacturasApp.Core.Services.Parsers;

namespace FacturasApp.Core.Services
{
    public class InvoiceProcessorService
    {
        private readonly ITextExtractor _textExtractor;
        private readonly ParserFactory _parserFactory = new();
        private readonly ExcelExtractor _excelExtractor = new();

        // Configuración de tolerancia para extracción zonal
        public bool UsarZonasSiempre { get; set; } = true;
        public bool FallbackATextoCompleto { get; set; } = true;

        public InvoiceProcessorService(ITextExtractor textExtractor)
        {
            _textExtractor = textExtractor;
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
            IdentificarEmisor(rutaPdf, _textExtractor.ExtraerTextoSeleccionable(rutaPdf, ModoExtraccion.Simple));

        private IInvoiceParser IdentificarEmisor(string rutaPdf, string? textoRapido) =>
            textoRapido != null
                ? _parserFactory.ObtenerParser(textoRapido)
                : _parserFactory.ObtenerParser(_textExtractor.ExtraerTextoOcrIdentificacion(rutaPdf));

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
            string? textoRapido = _textExtractor.ExtraerTextoSeleccionable(rutaPdf, ModoExtraccion.Simple);

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
                    textoExtraido = _textExtractor.ExtraerTextoZonal(rutaPdf, plantilla,
                        parser.ModoExtraccion);
                    extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
                }
                else
                {
                    textoExtraido = _textExtractor.ExtraerTextoOcrZonal(rutaPdf, plantilla);
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
                    textoExtraido = parser.ModoExtraccion == ModoExtraccion.Simple
                        ? textoRapido!
                        : _textExtractor.ExtraerTextoSeleccionable(rutaPdf, parser.ModoExtraccion)
                          ?? textoRapido!;
                }
                else
                {
                    textoExtraido = _textExtractor.ExtraerTextoOcrCompleto(rutaPdf);
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

        /// <summary>
        /// Pasos 1-3 de la muestra: extrae el texto del PDF según la config del
        /// emisor (zonal si hay zonas, completo si no). Es lo único que puede
        /// necesitar OCR, de forma que su resultado se cachea en la interfaz y
        /// se reutiliza: recalcular valores o probar expresiones no vuelve a
        /// lanzar Tesseract.
        /// </summary>
        /// <param name="plantillaPrevia">
        /// Plantilla de zonas ya montada por el llamante (p. ej. con las zonas
        /// que se están dibujando en la interfaz, todavía sin guardarse en el
        /// XML). Si es null se usa la de la configuración del emisor.
        /// </param>
        public (string Texto, bool ViaOcr, bool Zonal) ExtraerTextoMuestra(
            EmisorConfig config, string rutaPdf, PlantillaOcr? plantillaPrevia = null)
        {
            var parser = new ConfigurableParserEngine(config);

            // ── PASO 1: Detectar tipo de PDF ──────────────────────────────────
            string? textoRapido = _textExtractor.ExtraerTextoSeleccionable(rutaPdf, ModoExtraccion.Simple);

            bool esPdfSeleccionable = textoRapido != null;
            bool usarOcr = !esPdfSeleccionable;

            // ── PASO 2: Extracción zonal (solo para el emisor indicado) ───────
            string textoExtraido = "";
            bool extraccionZonalExitosa = false;
            PlantillaOcr? plantilla = plantillaPrevia ?? ObtenerPlantilla(parser, parser.Nombre);

            if (UsarZonasSiempre && plantilla != null && plantilla.Zonas.Any())
            {
                if (esPdfSeleccionable)
                    textoExtraido = _textExtractor.ExtraerTextoZonal(rutaPdf, plantilla,
                        parser.ModoExtraccion);
                else
                    textoExtraido = _textExtractor.ExtraerTextoOcrZonal(rutaPdf, plantilla);

                extraccionZonalExitosa = !string.IsNullOrEmpty(textoExtraido);
            }

            // ── PASO 3: Fallback a texto completo ──────────────────────────────
            if (!extraccionZonalExitosa && FallbackATextoCompleto)
            {
                if (esPdfSeleccionable)
                {
                    textoExtraido = parser.ModoExtraccion == ModoExtraccion.Simple
                        ? textoRapido!
                        : _textExtractor.ExtraerTextoSeleccionable(rutaPdf, parser.ModoExtraccion)
                          ?? textoRapido!;
                }
                else
                {
                    textoExtraido = _textExtractor.ExtraerTextoOcrCompleto(rutaPdf);
                }
            }

            return (textoExtraido, usarOcr, extraccionZonalExitosa);
        }

        /// <summary>
        /// Pasos 4-5 de la muestra: parsea un texto ya extraído (barato, sólo
        /// regex) y añade el aviso de «Extracción zonal utilizada» si procede.
        /// </summary>
        public List<Factura> ParsearMuestra(EmisorConfig config, string texto,
            bool viaOcr, bool zonal, string rutaPdf)
        {
            if (string.IsNullOrEmpty(texto))
                return new List<Factura>();

            var parser = new ConfigurableParserEngine(config);
            var facturas = parser.ParsearMultiple(texto, rutaPdf, viaOcr);

            if (zonal)
            {
                foreach (var factura in facturas)
                {
                    factura.MensajeError ??= new List<string>();
                    factura.MensajeError.Add($"Extracción zonal utilizada ({(viaOcr ? "OCR zonal" : "coordenadas")})");
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

            return null;
        }

        // ── Helper: detección de duplicados ─────────────────────────────────────

        private void AddWithDuplicateDetection(List<Factura> acumuladas, IEnumerable<Factura> nuevas)
        {
            foreach (var nueva in nuevas)
            {
                var numero = (nueva.NumeroFactura ?? string.Empty).Trim();
                string rutaNuevaFull = RutaCompleta(nueva);

                if (!string.IsNullOrEmpty(numero))
                {
                    var existente = acumuladas.FirstOrDefault(f =>
                    {
                        var fNumero = (f.NumeroFactura ?? string.Empty).Trim();
                        if (!string.Equals(fNumero, numero, StringComparison.OrdinalIgnoreCase))
                            return false;

                        return !string.Equals(RutaCompleta(f), rutaNuevaFull, StringComparison.OrdinalIgnoreCase);
                    });

                    if (existente != null)
                    {
                        // El duplicado se marca sobre la factura ENTERA, no
                        // sólo sobre la línea con la que se detectó: el
                        // Estado (y sus mensajes) es único por factura. Se
                        // recorren todas las líneas que comparten documento y
                        // número, tanto en la copia nueva como en la antigua.
                        MarcarDuplicada(nueva,
                            $"Factura duplicada. Existe en: {existente.RutaArchivo}");

                        foreach (var linea in acumuladas.Where(f => MismaFactura(f, existente)))
                            MarcarDuplicada(linea,
                                $"Factura duplicada. Otra copia: {nueva.RutaArchivo}");

                        acumuladas.Add(nueva);
                        continue;
                    }
                }

                acumuladas.Add(nueva);
            }
        }

        /// <summary>
        /// Ruta absoluta en mayúsculas, o cadena vacía; para comparar el
        /// documento al que pertenece cada línea.
        /// </summary>
        private static string RutaCompleta(Factura f)
        {
            string ruta = (f.RutaArchivo ?? string.Empty).Trim();
            return string.IsNullOrEmpty(ruta) ? string.Empty : Path.GetFullPath(ruta).ToUpperInvariant();
        }

        /// <summary>
        /// ¿Son líneas de la misma factura? Mismo número y mismo documento
        /// (en facturas multilínea todas las líneas comparten ambos).
        /// </summary>
        private static bool MismaFactura(Factura a, Factura b) =>
            string.Equals((a.NumeroFactura ?? string.Empty).Trim(),
                (b.NumeroFactura ?? string.Empty).Trim(),
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(RutaCompleta(a), RutaCompleta(b), StringComparison.OrdinalIgnoreCase);

        private static void MarcarDuplicada(Factura factura, string mensaje)
        {
            if (factura.Estado == EstadoFactura.OK)
                factura.Estado = EstadoFactura.Duplicada;

            factura.MensajeError ??= new List<string>();
            if (!factura.MensajeError.Contains(mensaje))
                factura.MensajeError.Add(mensaje);
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
            var clavesVistas = new HashSet<string>(StringComparer.Ordinal);

            if (rutasPdf.Any())
            {
                var facturasPdf = ProcesarLote(rutasPdf, progreso);
                foreach (var f in facturasPdf)
                    clavesVistas.Add(GenerarClaveDeduplicacion(f));
                facturas.AddRange(facturasPdf);
            }

            if (!string.IsNullOrEmpty(rutaExcel) && File.Exists(rutaExcel))
            {
                var facturasExcel = ImportarDesdeExcel(rutaExcel);
                foreach (var f in facturasExcel)
                {
                    string clave = GenerarClaveDeduplicacion(f);
                    if (!clavesVistas.Add(clave) && f.Estado == EstadoFactura.OK)
                        f.Estado = EstadoFactura.Duplicada;
                }
                facturas.AddRange(facturasExcel);
            }

            return facturas;
        }

        private static string GenerarClaveDeduplicacion(Factura f) =>
            $"{f.NumeroFactura}|{f.Emisor.NIF}|{f.Fecha:yyyy-MM-dd}|{f.BaseImponible}|{f.SubTotal}";

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
                ? _textExtractor.ExtraerTextoSeleccionable(rutaPdf, ModoExtraccion.Simple) ?? ""
                : _textExtractor.ExtraerTextoOcrCompleto(rutaPdf);

            var parser = _parserFactory.ObtenerParser(textoRapido);
            sb.AppendLine($"Emisor detectado: {parser.Nombre}");

            PlantillaOcr? plantilla = ObtenerPlantilla(parser, parser.Nombre);
            sb.AppendLine($"Zonas OCR disponibles: {(plantilla != null ? $"SÍ ({plantilla.Zonas.Count} zonas)" : "NO")}");

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
