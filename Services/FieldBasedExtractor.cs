using System.Globalization;
using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services
{
    /// <summary>
    /// Motor de extracción genérico basado en definiciones XML de emisores.
    /// Reemplaza a los parsers individuales para el ~90% de los casos.
    /// </summary>
    public class FieldBasedExtractor
    {
        private readonly PostProcesamientoEngine _postProcesador = new();

        public bool PuedeParsar(EmisorDefinicion emisor, string texto)
        {
            return emisor.Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));
        }

        public Factura Extraer(EmisorDefinicion emisor, string texto,
            string rutaArchivo, bool viaOcr)
        {
            // 1. Pre-procesamiento
            string textoProcesado = AplicarPreProcesamiento(emisor, texto);

            // 2. Limpiar OCR si se detectan duplicados
            if (viaOcr)
                textoProcesado = ExtractorHelper.EliminarDuplicadosNoNumericos(textoProcesado);

            // 3. Multi-factura
            if (emisor.MultiFactura?.LineaIva != null)
                return ExtraerMultiple(emisor, textoProcesado, rutaArchivo, viaOcr);

            // 4. Extraer campos individuales
            var campos = ExtraerCampos(emisor, textoProcesado);

            // 5. Construir Factura
            var factura = MapearAFactura(emisor, campos, textoProcesado, rutaArchivo, viaOcr);

            // 6. Post-procesamiento
            _postProcesador.Aplicar(emisor.PostProcesamiento, factura, textoProcesado);

            // 7. Estado
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }

        /// <summary>
        /// Realiza la extracción y retorna un diccionario de campos para el tester.
        /// No construye Factura ni aplica post-procesamiento.
        /// </summary>
        public Dictionary<string, string> ExtraerCamposParaTest(EmisorDefinicion emisor,
            string texto, bool viaOcr = false)
        {
            string textoProcesado = AplicarPreProcesamiento(emisor, texto);
            if (viaOcr)
                textoProcesado = ExtractorHelper.EliminarDuplicadosNoNumericos(textoProcesado);

            return ExtraerCampos(emisor, textoProcesado);
        }

        // ── Multi-factura ──────────────────────────────────────────────────────

        private Factura ExtraerMultiple(EmisorDefinicion emisor, string texto,
            string rutaArchivo, bool viaOcr)
        {
            var config = emisor.MultiFactura!;
            var lineaIva = config.LineaIva!;

            var regex = new Regex(lineaIva.Regex,
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            var matches = regex.Matches(texto);

            // Extraer campos comunes (los que no están en la línea de IVA)
            var camposComunes = ExtraerCamposComunes(emisor, texto);

            // Filtrar y deduplicar líneas de IVA
            var lineasValidas = matches.Cast<Match>();

            if (config.FiltrarBaseCero)
            {
                var mapeoBase = lineaIva.Mapeo.FirstOrDefault(m => m.Campo == "BaseImponible");
                if (mapeoBase != null)
                {
                    lineasValidas = lineasValidas.Where(m =>
                    {
                        string valor = m.Groups[mapeoBase.Grupo].Value
                            .Replace(".", "").Replace(",", ".");
                        return decimal.TryParse(valor, NumberStyles.Any,
                            CultureInfo.InvariantCulture, out decimal baseImp)
                            && baseImp != 0m;
                    });
                }
            }

            if (config.Deduplicar)
            {
                lineasValidas = lineasValidas.DistinctBy(m =>
                    string.Join("|", m.Groups.Cast<Group>().Select(g => g.Value)));
            }

            var facturas = new List<Factura>();
            decimal totalFactura = 0;

            if (!string.IsNullOrEmpty(camposComunes.ContainsKey("Total") ? camposComunes["Total"] : null))
            {
                totalFactura = ExtractorHelper.ParsearDecimal(camposComunes["Total"]);
            }

            decimal subtotales = 0;

            foreach (Match linea in lineasValidas)
            {
                var factura = new Factura
                {
                    RutaArchivo = rutaArchivo,
                    ExtractedByOcr = viaOcr
                };

                // Emisor
                factura.Emisor.Nombre = emisor.Nombre;
                factura.Emisor.NIF = emisor.Nif;

                // Campos comunes
                AplicarCamposComunes(factura, camposComunes, emisor);

                // Asignar campos de la línea de IVA
                foreach (var asignacion in lineaIva.Mapeo)
                {
                    string valor = linea.Groups[asignacion.Grupo].Value.Trim();
                    AsignarCampoFactura(factura, asignacion.Campo, valor);
                }

                // Total = Base + CuotaIVA si no se extrajo directamente
                if (factura.Total == 0 && factura.BaseImponible != 0)
                    factura.Total = factura.BaseImponible + factura.CuotaIVA;

                // Concepto
                if (!string.IsNullOrEmpty(emisor.Concepto) && emisor.Concepto != "600")
                    factura.Concepto = emisor.Concepto;

                // Post-procesamiento individual
                _postProcesador.Aplicar(emisor.PostProcesamiento, factura, texto);

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

            return facturas.Count > 0
                ? facturas[0]
                : CrearFacturaVacia(emisor, texto, rutaArchivo, viaOcr);
        }

        // ── Extracción de campos ───────────────────────────────────────────────

        private Dictionary<string, string> ExtraerCampos(EmisorDefinicion emisor, string texto)
        {
            var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var campo in emisor.Campos)
            {
                if (campo.Tipo == "Fijo" && !string.IsNullOrEmpty(campo.ValorFijo))
                {
                    campos[campo.Nombre] = campo.ValorFijo;
                    continue;
                }

                if (string.IsNullOrEmpty(campo.Regex))
                    continue;

                string valor = ExtraerCampoRegex(campo, texto);
                campos[campo.Nombre] = valor;
            }

            return campos;
        }

        private Dictionary<string, string> ExtraerCamposComunes(EmisorDefinicion emisor, string texto)
        {
            var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Solo extraer campos que NO están en la línea de IVA
            var camposEnLineaIva = emisor.MultiFactura?.LineaIva?.Mapeo
                .Select(m => m.Campo)
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();

            foreach (var campo in emisor.Campos)
            {
                if (camposEnLineaIva.Contains(campo.Nombre))
                    continue;

                if (campo.Tipo == "Fijo" && !string.IsNullOrEmpty(campo.ValorFijo))
                {
                    campos[campo.Nombre] = campo.ValorFijo;
                    continue;
                }

                if (string.IsNullOrEmpty(campo.Regex))
                    continue;

                string valor = ExtraerCampoRegex(campo, texto);
                campos[campo.Nombre] = valor;
            }

            return campos;
        }

        private static string ExtraerCampoRegex(CampoExtraccion campo, string texto)
        {
            // Multiline: ^ y $ matchean por línea (no por todo el texto)
            // Singleline: . matchea \n también
            var options = RegexOptions.IgnoreCase | RegexOptions.Multiline;

            // Si el regex contiene saltos de línea literales en el patrón,
            // también activar Singleline para que . matchee \n
            if (campo.Regex!.Contains('\n') || campo.Regex!.Contains('\r'))
                options |= RegexOptions.Singleline;

            var match = Regex.Match(texto, campo.Regex, options);

            if (!match.Success)
                return string.Empty;

            string resultado = match.Groups.Count > campo.Grupo
                ? match.Groups[campo.Grupo].Value.Trim()
                : match.Value.Trim();

            // Preservar saltos de línea en el valor extraído (no colapsar)
            return resultado;
        }

        // ── Mapeo a Factura ───────────────────────────────────────────────────

        private Factura MapearAFactura(EmisorDefinicion emisor,
            Dictionary<string, string> campos, string texto,
            string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr
            };

            // Emisor
            factura.Emisor.Nombre = emisor.Nombre;
            factura.Emisor.NIF = emisor.Nif;

            // Concepto por defecto
            if (!string.IsNullOrEmpty(emisor.Concepto))
                factura.Concepto = emisor.Concepto;

            // Mapear campos extraídos a propiedades de Factura
            AplicarCamposComunes(factura, campos, emisor);

            // NIF del receptor: si no se extrajo, usar el genérico
            if (string.IsNullOrEmpty(factura.Receptor.NIF))
                factura.Receptor.NIF = ExtractorHelper.ExtraerNif(texto, emisor.Nif);

            return factura;
        }

        private static void AplicarCamposComunes(Factura factura,
            Dictionary<string, string> campos, EmisorDefinicion emisor)
        {
            if (campos.TryGetValue("NumeroFactura", out string? num))
                factura.NumeroFactura = num;

            if (campos.TryGetValue("Fecha", out string? fecha) && !string.IsNullOrEmpty(fecha))
            {
                var campoFecha = emisor.Campos.FirstOrDefault(c => c.Nombre == "Fecha");
                factura.Fecha = ExtractorHelper.ExtraerFecha(fecha,
                    null, // ya tenemos el texto extraído
                    campoFecha?.FormatosFecha.Count > 0 ? campoFecha.FormatosFecha : null,
                    campoFecha?.Cultura ?? "es-ES");
            }

            if (campos.TryGetValue("ReceptorNombre", out string? nombre))
                factura.Receptor.Nombre = nombre;

            if (campos.TryGetValue("ReceptorNif", out string? nif))
                factura.Receptor.NIF = nif;

            if (campos.TryGetValue("BaseImponible", out string? baseImp))
                factura.BaseImponible = ExtractorHelper.ParsearDecimal(baseImp);

            if (campos.TryGetValue("PorcentajeIVA", out string? iva))
                factura.PorcentajeIVA = ExtractorHelper.ParsearDecimal(iva);

            if (campos.TryGetValue("CuotaIVA", out string? cuotaIva))
                factura.CuotaIVA = ExtractorHelper.ParsearDecimal(cuotaIva);

            if (campos.TryGetValue("PorcentajeIRPF", out string? irpf))
                factura.PorcentajeIRPF = ExtractorHelper.ParsearDecimal(irpf);

            if (campos.TryGetValue("CuotaIRPF", out string? cuotaIrpf))
                factura.CuotaIRPF = ExtractorHelper.ParsearDecimal(cuotaIrpf);

            if (campos.TryGetValue("PorcentajeRE", out string? re))
                factura.PorcentajeRE = ExtractorHelper.ParsearDecimal(re);

            if (campos.TryGetValue("CuotaRE", out string? cuotaRe))
                factura.CuotaRE = ExtractorHelper.ParsearDecimal(cuotaRe);

            if (campos.TryGetValue("Total", out string? total))
                factura.Total = ExtractorHelper.ParsearDecimal(total);

            if (campos.TryGetValue("Concepto", out string? concepto))
                factura.Concepto = concepto;
        }

        private static void AsignarCampoFactura(Factura factura, string campo, string valor)
        {
            switch (campo)
            {
                case "BaseImponible":
                    factura.BaseImponible = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "PorcentajeIVA":
                    factura.PorcentajeIVA = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "CuotaIVA":
                    factura.CuotaIVA = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "PorcentajeIRPF":
                    factura.PorcentajeIRPF = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "CuotaIRPF":
                    factura.CuotaIRPF = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "PorcentajeRE":
                    factura.PorcentajeRE = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "CuotaRE":
                    factura.CuotaRE = ExtractorHelper.ParsearDecimal(valor);
                    break;
                case "Total":
                    factura.Total = ExtractorHelper.ParsearDecimal(valor);
                    break;
            }
        }

        // ── Pre-procesamiento ──────────────────────────────────────────────────

        private static string AplicarPreProcesamiento(EmisorDefinicion emisor, string texto)
        {
            string resultado = texto;

            foreach (var regla in emisor.PreProcesamiento)
            {
                if (regla.EsRegex)
                    resultado = Regex.Replace(resultado, regla.Busco, regla.Por);
                else
                    resultado = resultado.Replace(regla.Busco, regla.Por);
            }

            return resultado;
        }

        // ── Factura vacía (fallback) ───────────────────────────────────────────

        private static Factura CrearFacturaVacia(EmisorDefinicion emisor,
            string texto, string rutaArchivo, bool viaOcr)
        {
            return new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                Emisor = new Proveedor
                {
                    Nombre = emisor.Nombre,
                    NIF = emisor.Nif
                },
                Estado = EstadoFactura.Error,
                MensajeError = new List<string> { "No se pudo extraer ninguna factura" }
            };
        }
    }
}
