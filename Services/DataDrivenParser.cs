using System.Globalization;
using System.Text.RegularExpressions;
using FacturasApp.Models;
using FacturasApp.Services.Parsers;

namespace FacturasApp.Services
{
    public class DataDrivenParser : BaseParser
    {
        private readonly ProveedorConfig _config;

        public DataDrivenParser(ProveedorConfig config)
        {
            _config = config;
        }

        public override string Nombre => _config.Nombre;
        public override string Nif => _config.Nif;
        public override string Concepto => _config.Concepto;

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            _config.ModoExtraccion switch
            {
                ModoExtraccionTexto.Simple => PdfTextExtractor.ModoExtraccion.Simple,
                ModoExtraccionTexto.LayoutAnalysis => PdfTextExtractor.ModoExtraccion.LayoutAnalysis,
                _ => PdfTextExtractor.ModoExtraccion.OrdenadoPosicion
            };

        protected override string[] Identificadores => [.. _config.Identificadores];

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            return ParsearMultiple(texto, rutaArchivo, viaOcr).First();
        }

        public Dictionary<CampoFactura, string> ProbarExtraccion(string texto)
        {
            texto = AplicarPreprocesamiento(texto);
            return ExtraerCampos(texto);
        }

        public override List<Factura> ParsearMultiple(string texto, string rutaArchivo, bool viaOcr)
        {
            texto = AplicarPreprocesamiento(texto);

            if (_config.MultiLineaIva?.Lineas.Count > 0 && _config.MultiLineaIva.CrearFacturaPorLinea)
                return ProcesarMultiLineaIva(texto, rutaArchivo, viaOcr);

            var camposExtraidos = ExtraerCampos(texto);
            var factura = ConstruirFactura(camposExtraidos, rutaArchivo, viaOcr);

            if (factura.Fecha == null)
                factura.Fecha = ExtraerFecha(texto);
            if (string.IsNullOrEmpty(factura.Receptor.NIF))
                factura.Receptor.NIF = ExtraerNif(texto);

            AplicarPostprocesamiento(camposExtraidos, factura, texto);

            factura.Estado = FacturaEstado.Determinar(factura);
            return [factura];
        }

        // ── Preprocesamiento ──────────────────────────────────────────────

        private string AplicarPreprocesamiento(string texto)
        {
            if (_config.Preprocesamiento == null)
                return texto;

            foreach (var reemplazo in _config.Preprocesamiento.Reemplazos)
            {
                if (!string.IsNullOrEmpty(reemplazo.Pattern))
                    texto = texto.Replace(reemplazo.Pattern, reemplazo.Reemplazo);
            }

            foreach (var dup in _config.Preprocesamiento.EliminarDuplicados)
            {
                texto = dup.Tipo switch
                {
                    TipoEliminarDuplicados.Numericos => EliminarDuplicadosNumericos(texto),
                    _ => EliminarDuplicadosNoNumericos(texto)
                };
            }

            return texto;
        }

        // ── Extracción de campos ─────────────────────────────────────────

        private Dictionary<CampoFactura, string> ExtraerCampos(string texto)
        {
            var resultado = new Dictionary<CampoFactura, string>();

            foreach (var campo in _config.Campos)
            {
                if (!string.IsNullOrEmpty(campo.ValorFijo))
                {
                    resultado[campo.Nombre] = campo.ValorFijo;
                    continue;
                }

                if (string.IsNullOrEmpty(campo.Regex))
                    continue;

                if (campo.Nombre == CampoFactura.Fecha)
                {
                    var fechaStr = ExtraerFechaCruda(texto, campo);
                    if (fechaStr != null)
                        resultado[campo.Nombre] = fechaStr;
                    continue;
                }

                if (campo.Nombre == CampoFactura.ClienteNif)
                {
                    var nif = ExtraerNifCliente(texto, campo);
                    if (!string.IsNullOrEmpty(nif))
                        resultado[campo.Nombre] = nif;
                    continue;
                }

                var regex = new Regex(campo.Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var match = regex.Match(texto);
                if (match.Success)
                {
                    var valor = match.Groups.Count > campo.Grupo
                        ? match.Groups[campo.Grupo].Value.Trim()
                        : match.Value.Trim();
                    if (!string.IsNullOrEmpty(valor))
                    {
                        if (campo.DeduplicarNumericos)
                            valor = EliminarDuplicadosNumericos(valor);
                        resultado[campo.Nombre] = valor;
                    }
                }
                else if (!campo.Opcional)
                {
                    resultado[campo.Nombre] = string.Empty;
                }
            }

            return resultado;
        }

        private string? ExtraerFechaCruda(string texto, CampoConfig campo)
        {
            if (string.IsNullOrEmpty(campo.Regex))
                return null;

            var regex = new Regex(campo.Regex, RegexOptions.Compiled);
            var match = regex.Match(texto);
            if (!match.Success) return null;

            var fechaStr = match.Groups.Count > campo.Grupo
                ? match.Groups[campo.Grupo].Value.Trim()
                : match.Value.Trim();

            return string.IsNullOrEmpty(fechaStr) ? null : fechaStr;
        }

        private string ExtraerNifCliente(string texto, CampoConfig campo)
        {
            if (string.IsNullOrEmpty(campo.Regex))
                return string.Empty;

            var regex = new Regex(campo.Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var coincidencias = regex.Matches(texto);

            foreach (Match m in coincidencias)
            {
                string nif = m.Groups.Count > campo.Grupo
                    ? m.Groups[campo.Grupo].Value.Trim()
                    : m.Value.Trim();

                if (string.IsNullOrEmpty(nif)) continue;

                nif = nif.Replace(" ", "").Replace("-", "").Replace(".", "").Replace(",", "").Trim().ToUpper();
                if (nif.Length > 9) nif = nif[..9];

                if (_config.DebeOmitirNifEmisor && nif.Equals(_config.Nif, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (NifValidator.ValidarNif(nif))
                    return nif;
            }

            return string.Empty;
        }

        // ── Multi línea IVA ──────────────────────────────────────────────

        private List<Factura> ProcesarMultiLineaIva(string texto, string rutaArchivo, bool viaOcr)
        {
            var camposCabecera = ExtraerCampos(texto);
            var configMl = _config.MultiLineaIva!;
            var lineasAgrupadas = new List<Dictionary<string, string>>();

            foreach (var lineaConfig in configMl.Lineas)
            {
                var regex = new Regex(lineaConfig.Regex, RegexOptions.Compiled);
                var matches = regex.Matches(texto);
                var mapa = ParsearMapa(lineaConfig.Mapa);

                foreach (Match match in matches)
                {
                    var camposLinea = new Dictionary<string, string>();
                    foreach (var (campoStr, grupo) in mapa)
                    {
                        if (grupo < match.Groups.Count)
                            camposLinea[campoStr] = match.Groups[grupo].Value.Trim();
                    }
                    lineasAgrupadas.Add(camposLinea);
                }
            }

            // Deduplicar
            if (configMl.Deduplicar)
            {
                lineasAgrupadas = lineasAgrupadas
                    .DistinctBy(d => string.Join("|", d.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}")))
                    .ToList();
            }

            // Excluir base cero
            if (configMl.ExcluirBaseCero)
            {
                lineasAgrupadas = lineasAgrupadas
                    .Where(d => !d.TryGetValue("BaseImponible", out var b) || string.IsNullOrEmpty(b) ||
                                ParsearDecimal(b) != 0m)
                    .ToList();
            }

            decimal? totalFacturaDoc = null;
            if (configMl.TotalFactura != null && !string.IsNullOrEmpty(configMl.TotalFactura.Regex))
            {
                var regexTotal = new Regex(configMl.TotalFactura.Regex, RegexOptions.Compiled);
                var m = regexTotal.Match(texto);
                if (m.Success)
                {
                    var val = m.Groups.Count > configMl.TotalFactura.Grupo
                        ? m.Groups[configMl.TotalFactura.Grupo].Value
                        : m.Value;
                    totalFacturaDoc = ParsearDecimal(val);
                }
            }

            var facturas = new List<Factura>();

            foreach (var camposLinea in lineasAgrupadas)
            {
                var camposCombinados = new Dictionary<CampoFactura, string>(camposCabecera);
                foreach (var (campoStr, valor) in camposLinea)
                {
                    if (Enum.TryParse<CampoFactura>(campoStr, out var cf))
                        camposCombinados[cf] = valor;
                }

                var factura = ConstruirFactura(camposCombinados, rutaArchivo, viaOcr);
                AplicarPostprocesamiento(camposCombinados, factura, texto);

                if (factura.Fecha == null)
                    factura.Fecha = ExtraerFecha(texto);
                if (string.IsNullOrEmpty(factura.Receptor.NIF))
                    factura.Receptor.NIF = ExtraerNif(texto);

                facturas.Add(factura);
            }

            // Validar suma subtotales
            if (configMl.ValidarSumaSubtotales && totalFacturaDoc.HasValue)
            {
                var suma = facturas.Sum(f => f.Total);
                if (Math.Abs(suma - totalFacturaDoc.Value) > 0.01m)
                {
                    foreach (var f in facturas)
                    {
                        f.Estado = EstadoFactura.Error;
                        f.MensajeError.Add($"Suma subtotales ({suma:N2}) no coincide con total documento ({totalFacturaDoc.Value:N2})");
                    }
                }
            }

            // Determinar estado individual (si no está ya en error)
            foreach (var f in facturas.Where(f => f.Estado != EstadoFactura.Error))
                f.Estado = FacturaEstado.Determinar(f);

            return facturas;
        }

        private static Dictionary<string, int> ParsearMapa(string mapa)
        {
            var result = new Dictionary<string, int>();
            if (string.IsNullOrWhiteSpace(mapa))
                return result;

            foreach (var parte in mapa.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = parte.Split('=', StringSplitOptions.TrimEntries);
                if (kv.Length == 2 && int.TryParse(kv[1], out var grupo))
                    result[kv[0]] = grupo;
            }

            return result;
        }

        // ── Construcción de Factura ──────────────────────────────────────

        private Factura ConstruirFactura(Dictionary<CampoFactura, string> campos, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);
            factura.Concepto = _config.Concepto;

            foreach (var (campo, valorRaw) in campos)
            {
                if (string.IsNullOrEmpty(valorRaw)) continue;

                switch (campo)
                {
                    case CampoFactura.NumeroFactura:
                        factura.NumeroFactura = valorRaw;
                        break;

                    case CampoFactura.Fecha:
                        var campoFecha = _config.Campos.FirstOrDefault(c => c.Nombre == CampoFactura.Fecha);
                        factura.Fecha = ParsearFecha(valorRaw, campoFecha);
                        break;

                    case CampoFactura.ClienteNombre:
                        factura.Receptor.Nombre = valorRaw;
                        break;

                    case CampoFactura.ClienteNif:
                        factura.Receptor.NIF = valorRaw;
                        break;

                    case CampoFactura.BaseImponible:
                        factura.BaseImponible = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.PorcentajeIVA:
                        factura.PorcentajeIVA = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.CuotaIVA:
                        factura.CuotaIVA = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.PorcentajeIRPF:
                        factura.PorcentajeIRPF = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.CuotaIRPF:
                        factura.CuotaIRPF = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.PorcentajeRE:
                        factura.PorcentajeRE = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.CuotaRE:
                        factura.CuotaRE = ParsearDecimal(valorRaw);
                        break;

                    case CampoFactura.Total:
                        factura.Total = ParsearDecimal(valorRaw);
                        break;
                }
            }

            return factura;
        }

        private DateTime? ParsearFecha(string valorRaw, CampoConfig? campoFecha)
        {
            if (string.IsNullOrEmpty(valorRaw))
                return null;

            var cultura = campoFecha?.Cultura ?? "es-ES";
            var formato = campoFecha?.Formato ?? string.Empty;

            try
            {
                var culture = new CultureInfo(cultura);

                if (!string.IsNullOrEmpty(formato))
                {
                    if (DateTime.TryParseExact(valorRaw, formato, culture, DateTimeStyles.None, out var f1))
                        return f1;
                }

                if (DateTime.TryParse(valorRaw, culture, DateTimeStyles.None, out var f2))
                    return f2;
            }
            catch { }

            return null;
        }

        // ── Postprocesamiento ────────────────────────────────────────────

        private void AplicarPostprocesamiento(Dictionary<CampoFactura, string> campos, Factura factura, string textoOriginal)
        {
            var postProc = _config.Postprocesamiento;
            if (postProc == null || postProc.Condiciones.Count == 0)
                return;

            foreach (var cond in postProc.Condiciones)
            {
                if (!EvaluarCondicion(cond, campos, factura))
                    continue;

                foreach (var mover in cond.MoverCampos)
                    EjecutarMoverCampo(mover, campos, factura);

                foreach (var asignar in cond.AsignarValoresFijos)
                    EjecutarAsignarValorFijo(asignar, campos, factura);

                foreach (var copiar in cond.CopiarCampos)
                    EjecutarCopiarCampo(copiar, campos, factura);

                foreach (var sumar in cond.SumarCampos)
                    EjecutarSumarCampo(sumar, campos, factura);
            }

            ReconstruirFacturaDesdeCampos(campos, factura);
        }

        private bool EvaluarCondicion(CondicionConfig cond, Dictionary<CampoFactura, string> campos, Factura factura)
        {
            if (!Enum.TryParse<CampoFactura>(cond.Campo, out var campoEnum))
                return false;

            if (!campos.TryGetValue(campoEnum, out var valor))
                return false;

            var decimalCond = TryParseDecimal(cond.Valor);

            if (string.IsNullOrEmpty(valor) && decimalCond.HasValue && decimalCond.Value == 0m)
                valor = "0";

            var decimalValor = TryParseDecimal(valor);

            return cond.Operador switch
            {
                OperadorCondicion.Igual => decimalCond.HasValue && decimalValor.HasValue
                    ? Math.Abs(decimalValor.Value - decimalCond.Value) < 0.001m
                    : string.Equals(valor, cond.Valor, StringComparison.OrdinalIgnoreCase),
                OperadorCondicion.Distinto => !string.Equals(valor, cond.Valor, StringComparison.OrdinalIgnoreCase),
                OperadorCondicion.MayorQue => decimalValor.HasValue && decimalCond.HasValue && decimalValor.Value > decimalCond.Value,
                OperadorCondicion.MenorQue => decimalValor.HasValue && decimalCond.HasValue && decimalValor.Value < decimalCond.Value,
                OperadorCondicion.MayorOIgual => decimalValor.HasValue && decimalCond.HasValue && decimalValor.Value >= decimalCond.Value,
                OperadorCondicion.MenorOIgual => decimalValor.HasValue && decimalCond.HasValue && decimalValor.Value <= decimalCond.Value,
                _ => false
            };
        }

        private static void EjecutarMoverCampo(MoverCampoConfig mover, Dictionary<CampoFactura, string> campos, Factura factura)
        {
            if (!Enum.TryParse<CampoFactura>(mover.Origen, out var origen)) return;
            if (!Enum.TryParse<CampoFactura>(mover.Destino, out var destino)) return;
            if (!campos.TryGetValue(origen, out var valor)) return;
            campos[destino] = valor;
            campos[origen] = string.Empty;
        }

        private static void EjecutarAsignarValorFijo(AsignarValorFijoConfig asignar, Dictionary<CampoFactura, string> campos, Factura factura)
        {
            if (!Enum.TryParse<CampoFactura>(asignar.Campo, out var campo)) return;
            campos[campo] = asignar.Valor;
        }

        private static void EjecutarCopiarCampo(CopiarCampoConfig copiar, Dictionary<CampoFactura, string> campos, Factura factura)
        {
            if (!Enum.TryParse<CampoFactura>(copiar.Origen, out var origen)) return;
            if (!Enum.TryParse<CampoFactura>(copiar.Destino, out var destino)) return;
            if (!campos.TryGetValue(origen, out var valor)) return;
            campos[destino] = valor;
        }

        private static void EjecutarSumarCampo(SumarCampoConfig sumar, Dictionary<CampoFactura, string> campos, Factura factura)
        {
            if (!Enum.TryParse<CampoFactura>(sumar.Destino, out var destino)) return;
            if (!Enum.TryParse<CampoFactura>(sumar.Origen, out var origen)) return;

            if (!campos.TryGetValue(destino, out var valDestino)) return;
            if (!campos.TryGetValue(origen, out var valOrigen)) return;

            var d1 = TryParseDecimal(valDestino);
            var d2 = TryParseDecimal(valOrigen);
            if (d1.HasValue && d2.HasValue)
                campos[destino] = (d1.Value + d2.Value).ToString("F2", CultureInfo.InvariantCulture);
        }

        private void ReconstruirFacturaDesdeCampos(Dictionary<CampoFactura, string> campos, Factura factura)
        {
            foreach (var (campo, valor) in campos)
            {
                if (string.IsNullOrEmpty(valor)) continue;
                var dec = TryParseDecimal(valor);

                switch (campo)
                {
                    case CampoFactura.BaseImponible when dec.HasValue:
                        factura.BaseImponible = dec.Value;
                        break;
                    case CampoFactura.PorcentajeIVA when dec.HasValue:
                        factura.PorcentajeIVA = dec.Value;
                        break;
                    case CampoFactura.CuotaIVA when dec.HasValue:
                        factura.CuotaIVA = dec.Value;
                        break;
                    case CampoFactura.PorcentajeIRPF when dec.HasValue:
                        factura.PorcentajeIRPF = dec.Value;
                        break;
                    case CampoFactura.CuotaIRPF when dec.HasValue:
                        factura.CuotaIRPF = dec.Value;
                        break;
                    case CampoFactura.PorcentajeRE when dec.HasValue:
                        factura.PorcentajeRE = dec.Value;
                        break;
                    case CampoFactura.CuotaRE when dec.HasValue:
                        factura.CuotaRE = dec.Value;
                        break;
                    case CampoFactura.Total when dec.HasValue:
                        factura.Total = dec.Value;
                        break;
                    case CampoFactura.NumeroFactura:
                        factura.NumeroFactura = valor;
                        break;
                    case CampoFactura.ClienteNombre:
                        factura.Receptor.Nombre = valor;
                        break;
                    case CampoFactura.ClienteNif:
                        factura.Receptor.NIF = valor;
                        break;
                }
            }

            if (campos.TryGetValue(CampoFactura.Fecha, out var fechaStr) && !string.IsNullOrEmpty(fechaStr))
            {
                var campoFecha = _config.Campos.FirstOrDefault(c => c.Nombre == CampoFactura.Fecha);
                factura.Fecha = ParsearFecha(fechaStr, campoFecha);
            }
        }

        private static decimal? TryParseDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            try { return ParsearDecimal(valor); }
            catch { return null; }
        }
    }
}
