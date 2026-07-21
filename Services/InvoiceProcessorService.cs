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
        private readonly EmisorService _emisorService = new();
        private readonly FieldBasedExtractor _fieldExtractor = new();

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
        /// Procesa un único PDF usando la estrategia unificada:
        /// 1. Detecta si tiene texto seleccionable o es escaneado
        /// 2. Identifica el emisor (XML primero, luego C#)
        /// 3. Intenta extracción por zonas (si existe plantilla)
        /// 4. Fallback a texto completo si no hay plantilla o falló
        /// 5. Parseo con el motor correspondiente (XML o C#)
        /// 
        /// Retorna una LISTA de facturas (puede ser múltiples si hay varias líneas de IVA).
        /// </summary>
        private List<Factura> ProcesarUnPdf(string rutaPdf)
        {
            // ── PASO 1: Detectar tipo de PDF ──────────────────────────────────
            string? textoRapido = _textExtractor.ExtraerTextoSeleccionable(rutaPdf,
                PdfTextExtractor.ModoExtraccion.Simple);

            bool esPdfSeleccionable = textoRapido != null;
            bool usarOcr = !esPdfSeleccionable;

            // ── PASO 2: Identificar emisor ─────────────────────────────────
            string textoIdentificacion;
            EmisorDefinicion? emisorXml = null;
            IInvoiceParser? parserCSharp = null;
            string nombreEmisor;

            if (esPdfSeleccionable)
                textoIdentificacion = textoRapido!;
            else
                textoIdentificacion = _ocrExtractor.ExtraerTextoIdentificacion(rutaPdf);

            // Intentar con emisores XML primero (misma lógica que PuedeParsar: ALL)
            var todosEmisoresXml = _emisorService.ObtenerTodos();
            emisorXml = todosEmisoresXml.FirstOrDefault(e =>
                e.Identificadores.All(id =>
                    textoIdentificacion.Contains(id, StringComparison.OrdinalIgnoreCase)));

            if (emisorXml != null)
            {
                nombreEmisor = emisorXml.Nombre;
                System.Diagnostics.Debug.WriteLine(
                    $"✓ Emisor detectado desde XML: {nombreEmisor} (NIF: {emisorXml.Nif})");
            }
            else
            {
                // Fallback a parsers C#
                parserCSharp = _parserFactory.ObtenerParser(textoIdentificacion);
                nombreEmisor = parserCSharp.Nombre;
                System.Diagnostics.Debug.WriteLine(
                    $"✓ Emisor detectado desde C#: {nombreEmisor}");
            }

            // ── PASO 3: Intentar extracción por zonas (si hay plantilla) ──────
            string textoExtraido = "";
            bool extraccionZonalExitosa = false;

            // Buscar plantilla OCR: primero en emisores XML, luego en plantillas_ocr.xml
            PlantillaOcr? plantilla = null;

            if (emisorXml?.ZonasOcr?.Zonas.Count > 0)
            {
                // Convertir zonas del XML al formato PlantillaOcr existente
                plantilla = emisorXml.ZonasOcr.APlantillaOcr(nombreEmisor);
                System.Diagnostics.Debug.WriteLine(
                    $"✓ Zonas OCR desde XML: {plantilla.Zonas.Count} zonas");
            }

            if (plantilla == null)
                plantilla = _plantillaService.ObtenerPorEmisor(nombreEmisor);

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
                    var modoExtraer = parserCSharp?.ModoExtraccion
                        ?? PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

                    textoExtraido = modoExtraer == PdfTextExtractor.ModoExtraccion.Simple
                        ? textoRapido!
                        : _textExtractor.ExtraerTextoSeleccionable(rutaPdf, modoExtraer)
                          ?? textoRapido!;
                }
                else
                {
                    textoExtraido = _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
                }
            }

            // ── PASO 5: Parsear el texto extraído ──────────────────────────────
            if (string.IsNullOrEmpty(textoExtraido))
                throw new InvalidOperationException("No se pudo extraer texto del PDF");

            List<Factura> facturasParseadas;

            if (emisorXml != null)
            {
                // Motor basado en XML
                var facturaUnica = _fieldExtractor.Extraer(emisorXml, textoExtraido, rutaPdf, usarOcr);
                facturasParseadas = new List<Factura> { facturaUnica };

                // Si el emisor XML define multi-factura, re-extraer con multi
                if (emisorXml.MultiFactura?.LineaIva != null)
                {
                    facturasParseadas = ProcesarMultipleDesdeXml(
                        emisorXml, textoExtraido, rutaPdf, usarOcr);
                }
            }
            else if (parserCSharp is BaseParser baseParser)
            {
                facturasParseadas = baseParser.ParsearMultiple(textoExtraido, rutaPdf, usarOcr);
            }
            else
            {
                facturasParseadas = new List<Factura>
                {
                    parserCSharp!.Parsear(textoExtraido, rutaPdf, usarOcr)
                };
            }

            // Marcar metadata sobre el método de extracción usado
            foreach (var factura in facturasParseadas)
            {
                factura.MensajeError ??= new List<string>();

                if (emisorXml != null)
                    factura.MensajeError.Add("Motor: XML (FieldBasedExtractor)");

                if (extraccionZonalExitosa)
                    factura.MensajeError.Add(
                        $"Extracción zonal utilizada ({(esPdfSeleccionable ? "coordenadas" : "OCR zonal")})");
            }

            return facturasParseadas;
        }

        /// <summary>
        /// Procesa multi-factura desde configuración XML.
        /// </summary>
        private List<Factura> ProcesarMultipleDesdeXml(
            EmisorDefinicion emisor, string texto, string rutaPdf, bool usarOcr)
        {
            var facturas = new List<Factura>();
            var config = emisor.MultiFactura!;
            var lineaIva = config.LineaIva!;

            var regex = new System.Text.RegularExpressions.Regex(lineaIva.Regex,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline |
                System.Text.RegularExpressions.RegexOptions.Compiled);
            var matches = regex.Matches(texto);

            // Extraer campos comunes (no IVA)
            var camposComunes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var campo in emisor.Campos)
            {
                bool estaEnLineaIva = lineaIva.Mapeo.Any(m => m.Campo == campo.Nombre);
                if (estaEnLineaIva || string.IsNullOrEmpty(campo.Regex))
                    continue;

                if (campo.Tipo == "Fijo" && !string.IsNullOrEmpty(campo.ValorFijo))
                {
                    camposComunes[campo.Nombre] = campo.ValorFijo;
                    continue;
                }

                var options = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                var match = System.Text.RegularExpressions.Regex.Match(texto, campo.Regex!, options);
                camposComunes[campo.Nombre] = match.Success && match.Groups.Count > campo.Grupo
                    ? match.Groups[campo.Grupo].Value.Trim() : string.Empty;
            }

            // Filtrar líneas de IVA válidas
            var lineasValidas = matches.Cast<System.Text.RegularExpressions.Match>();

            if (config.FiltrarBaseCero)
            {
                var mapeoBase = lineaIva.Mapeo.FirstOrDefault(m => m.Campo == "BaseImponible");
                if (mapeoBase != null)
                {
                    lineasValidas = lineasValidas.Where(m =>
                    {
                        string valor = m.Groups[mapeoBase.Grupo].Value
                            .Replace(".", "").Replace(",", ".");
                        return decimal.TryParse(valor,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out decimal baseImp) && baseImp != 0m;
                    });
                }
            }

            if (config.Deduplicar)
            {
                lineasValidas = lineasValidas.DistinctBy(m =>
                    string.Join("|", m.Groups.Cast<System.Text.RegularExpressions.Group>()
                        .Select(g => g.Value)));
            }

            decimal totalFactura = 0;
            if (camposComunes.TryGetValue("Total", out string? totalStr) && !string.IsNullOrEmpty(totalStr))
                totalFactura = ExtractorHelper.ParsearDecimal(totalStr);

            decimal subtotales = 0;

            foreach (var linea in lineasValidas)
            {
                var factura = new Factura
                {
                    RutaArchivo = rutaPdf,
                    ExtractedByOcr = usarOcr,
                    Emisor = new Proveedor { Nombre = emisor.Nombre, NIF = emisor.Nif },
                    Concepto = emisor.Concepto
                };

                // Aplicar campos comunes
                if (camposComunes.TryGetValue("NumeroFactura", out string? num))
                    factura.NumeroFactura = num;
                if (camposComunes.TryGetValue("Fecha", out string? fecha) && !string.IsNullOrEmpty(fecha))
                {
                    var campoFecha = emisor.Campos.FirstOrDefault(c => c.Nombre == "Fecha");
                    factura.Fecha = ExtractorHelper.ExtraerFecha(fecha, null,
                        campoFecha?.FormatosFecha.Count > 0 ? campoFecha.FormatosFecha : null,
                        campoFecha?.Cultura ?? "es-ES");
                }
                if (camposComunes.TryGetValue("ReceptorNombre", out string? nombre))
                    factura.Receptor.Nombre = nombre;
                if (camposComunes.TryGetValue("ReceptorNif", out string? nif) && !string.IsNullOrEmpty(nif))
                    factura.Receptor.NIF = nif;

                // Asignar campos de la línea de IVA
                foreach (var asignacion in lineaIva.Mapeo)
                {
                    string valor = linea.Groups[asignacion.Grupo].Value.Trim();
                    switch (asignacion.Campo)
                    {
                        case "BaseImponible":
                            factura.BaseImponible = ExtractorHelper.ParsearDecimal(valor); break;
                        case "PorcentajeIVA":
                            factura.PorcentajeIVA = ExtractorHelper.ParsearDecimal(valor); break;
                        case "CuotaIVA":
                            factura.CuotaIVA = ExtractorHelper.ParsearDecimal(valor); break;
                        case "Total":
                            factura.Total = ExtractorHelper.ParsearDecimal(valor); break;
                    }
                }

                // Total calculado si no se extrajo
                if (factura.Total == 0 && factura.BaseImponible != 0)
                    factura.Total = factura.BaseImponible + factura.CuotaIVA;

                // Post-procesamiento
                var postProcesador = new PostProcesamientoEngine();
                postProcesador.Aplicar(emisor.PostProcesamiento, factura, texto);

                factura.Estado = FacturaEstado.Determinar(factura);
                subtotales += factura.Total;
                facturas.Add(factura);
            }

            // Validar suma
            if (config.ValidarSuma && totalFactura != 0 && facturas.Count > 1)
            {
                if (subtotales != totalFactura)
                {
                    foreach (var factura in facturas)
                    {
                        factura.MensajeError.Add(
                            $"La suma de los sub-totales ({subtotales}) no coincide con el total de la factura ({totalFactura}).");
                        factura.Estado = EstadoFactura.Error;
                    }
                }
            }

            return facturas;
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

        /// <summary>
        /// Servicio de emisores XML (para CRUD desde UI).
        /// </summary>
        public EmisorService EmisorService => _emisorService;

        /// <summary>
        /// Motor de extracción basado en XML (para tester desde UI).
        /// </summary>
        public FieldBasedExtractor FieldExtractor => _fieldExtractor;

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

            // Buscar en XML primero
            var emisoresXml = _emisorService.ObtenerTodos();
            var emisorXml = emisoresXml.FirstOrDefault(e =>
                e.Identificadores.All(id =>
                    textoRapido.Contains(id, StringComparison.OrdinalIgnoreCase)));

            if (emisorXml != null)
            {
                sb.AppendLine($"Emisor detectado: {emisorXml.Nombre} (desde XML)");
                sb.AppendLine($"NIF: {emisorXml.Nif}");
                sb.AppendLine($"Campos definidos: {emisorXml.Campos.Count}");
                sb.AppendLine($"Post-procesamiento: {emisorXml.PostProcesamiento.Count} reglas");

                if (emisorXml.ZonasOcr?.Zonas.Count > 0)
                    sb.AppendLine($"Zonas OCR en XML: {emisorXml.ZonasOcr.Zonas.Count}");

                // Buscar también en plantillas_ocr.xml
                var plantillaLegacy = _plantillaService.ObtenerPorEmisor(emisorXml.Nombre);
                if (plantillaLegacy != null)
                    sb.AppendLine($"Plantilla OCR legacy: {plantillaLegacy.Zonas.Count} zonas");
            }
            else
            {
                var parser = _parserFactory.ObtenerParser(textoRapido);
                sb.AppendLine($"Emisor detectado: {parser.Nombre} (desde C#)");
                sb.AppendLine($"Tipo: {parser.GetType().Name}");

                var plantilla = _plantillaService.ObtenerPorEmisor(parser.Nombre);
                sb.AppendLine($"Plantilla de zonas disponible: {(plantilla != null ? $"SÍ ({plantilla.Zonas.Count} zonas)" : "NO")}");
            }

            return sb.ToString();
        }
    }
}