using System.Text.RegularExpressions;
using System.Globalization;
using FacturasApp.Models;
using FacturasApp.Models.EmisoresConfig;

namespace FacturasApp.Services.Parsers;

public class ConfigurableParserEngine : BaseParser
{
    private readonly EmisorConfig _config;
    private readonly Lazy<Regex> _regexMultiLinea;

    public EmisorConfig Config => _config;

    public ConfigurableParserEngine(EmisorConfig config)
    {
        _config = config;
        _regexMultiLinea = new Lazy<Regex>(() =>
            !string.IsNullOrEmpty(_config.MultiLineaIVA?.RegexLinea)
                ? new Regex(_config.MultiLineaIVA.RegexLinea, RegexOptions.Compiled)
                : new Regex("^$"));
    }

    public override string Nombre => _config.Nombre;
    public override string Nif => _config.Nif;

    public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
        Enum.TryParse<PdfTextExtractor.ModoExtraccion>(_config.ModoExtraccion,
            ignoreCase: true, out var modo)
            ? modo
            : PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

    public override bool PuedeParsar(string texto)
    {
        if (_config.Identificadores == null || _config.Identificadores.Count == 0)
            return true;
        return _config.Identificadores
            .All(id => texto.Contains(id, StringComparison.OrdinalIgnoreCase));
    }

    public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
    {
        if (_config.MultiLineaIVA?.Habilitado == true)
            return ParsearMultiple(texto, rutaArchivo, viaOcr).First();
        return ParsearUnica(texto, rutaArchivo, viaOcr);
    }

    public override List<Factura> ParsearMultiple(string texto, string rutaArchivo, bool viaOcr)
    {
        if (_config.MultiLineaIVA?.Habilitado == true)
            return ParsearMultiLinea(texto, rutaArchivo, viaOcr);
        return [ParsearUnica(texto, rutaArchivo, viaOcr)];
    }

    // ── Factura única ────────────────────────────────────────────────────────

    private Factura ParsearUnica(string texto, string rutaArchivo, bool viaOcr)
    {
        var factura = CrearFacturaBase(rutaArchivo, viaOcr);
        factura.ConceptoGasto = string.IsNullOrEmpty(_config.ConceptoGasto) ? "600" : _config.ConceptoGasto;
        factura.ConceptoIngreso = string.IsNullOrEmpty(_config.ConceptoIngreso) ? "700" : _config.ConceptoIngreso;

        var camposSuma = new List<CampoConfig>();

        foreach (var campo in _config.Campos)
        {
            if (campo.EsSuma)
            {
                camposSuma.Add(campo);
                continue;
            }
            ExtraerYAsignarCampo(factura, campo, texto);
        }

        foreach (var campo in camposSuma)
            AsignarSuma(factura, campo);

        AplicarPostProcesamiento(factura, texto);
        factura.Estado = FacturaEstado.Determinar(factura);
        return factura;
    }

    // ── Multi-IVA ────────────────────────────────────────────────────────────

    private List<Factura> ParsearMultiLinea(string texto, string rutaArchivo, bool viaOcr)
    {
        var matches = _regexMultiLinea.Value.Matches(texto);
        if (matches.Count == 0)
            return [ParsearUnica(texto, rutaArchivo, viaOcr)];

        var facturas = new List<Factura>();

        foreach (Match match in matches)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);
            factura.ConceptoGasto = string.IsNullOrEmpty(_config.ConceptoGasto) ? "600" : _config.ConceptoGasto;
            factura.ConceptoIngreso = string.IsNullOrEmpty(_config.ConceptoIngreso) ? "700" : _config.ConceptoIngreso;

            var esCampoLinea = new HashSet<string>(
                _config.MultiLineaIVA!.MapeoCampos
                    .Select(m => m.Nombre),
                StringComparer.OrdinalIgnoreCase);

            var camposSuma = new List<CampoConfig>();

            foreach (var campo in _config.Campos)
            {
                if (campo.EsSuma)
                {
                    camposSuma.Add(campo);
                    continue;
                }

                if (esCampoLinea.Contains(campo.Nombre))
                    continue;

                if (campo.Nombre is "TotalFactura")
                    continue;

                ExtraerYAsignarCampo(factura, campo, texto);
            }

            foreach (var mapeo in _config.MultiLineaIVA.MapeoCampos)
            {
                if (mapeo.Grupo >= match.Groups.Count) continue;
                var valor = match.Groups[mapeo.Grupo].Value.Trim();
                if (!string.IsNullOrEmpty(valor))
                    AsignarCampo(factura, mapeo.Nombre, valor, campoFormatoFecha: null);
            }

            factura.TotalFactura = factura.SubTotal;

            foreach (var campo in camposSuma)
                AsignarSuma(factura, campo);

            AplicarPostProcesamiento(factura, texto);
            factura.Estado = FacturaEstado.Determinar(factura);
            facturas.Add(factura);
        }

        VerificarCoherenciaTotalMulti(facturas, texto);

        return facturas;
    }

    private void VerificarCoherenciaTotalMulti(List<Factura> facturas, string texto)
    {
        var campoTotal = _config.Campos.FirstOrDefault(c =>
            !c.EsSuma && !string.IsNullOrEmpty(c.Regex) &&
            (c.Nombre == "TotalFactura"));
        if (campoTotal == null) return;

        var regex = new Regex(campoTotal.Regex!, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var match = regex.Match(texto);
        if (!match.Success || campoTotal.Grupo >= match.Groups.Count) return;

        var totalDocumento = ParsearDecimal(match.Groups[campoTotal.Grupo].Value.Trim());
        var sumaSubtotales = facturas.Sum(f => f.SubTotal);

        if (Math.Abs(sumaSubtotales - totalDocumento) > 0.01m)
        {
            string mensaje = $"La suma de los subtotales Multi-IVA ({sumaSubtotales:N2} €) no coincide con el TotalFactura extraído ({totalDocumento:N2} €)";
            foreach (var factura in facturas)
            {
                factura.MensajeError.Add(mensaje);
                factura.Estado = EstadoFactura.Error;
            }
        }
    }

    // ── Extracción y asignación ──────────────────────────────────────────────

    private void ExtraerYAsignarCampo(Factura factura, CampoConfig campo, string texto)
    {
        if (campo.UsarRegexFechaGeneral && campo.Nombre == "Fecha")
        {
            factura.Fecha = ExtraerFecha(texto);
            return;
        }

        if (campo.UsarRegexNifGeneral)
        {
            var nif = ExtraerNif(texto);
            if (!string.IsNullOrEmpty(nif))
            {
                if (campo.Nombre == "ReceptorNif")
                    factura.Receptor.NIF = nif;
                else if (campo.Nombre == "EmisorNif")
                    factura.Emisor.NIF = nif;
            }
            return;
        }

        if (campo.EsSuma)
            return;

        string? rawValue = null;

        if (!string.IsNullOrEmpty(campo.ValorFijo))
        {
            rawValue = campo.ValorFijo;
        }
        else if (!string.IsNullOrEmpty(campo.Regex))
        {
            var regex = new Regex(campo.Regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var match = regex.Match(texto);
            if (match.Success && campo.Grupo < match.Groups.Count)
                rawValue = match.Groups[campo.Grupo].Value.Trim();
        }

        if (rawValue != null)
            AsignarCampo(factura, campo.Nombre, rawValue, campo.FormatoFecha);
    }

    private void AsignarCampo(Factura factura, string nombreCampo,
        string valorTexto, string? campoFormatoFecha)
    {
        if (string.IsNullOrEmpty(valorTexto)) return;

        switch (nombreCampo)
        {
            case "NumeroFactura":
                factura.NumeroFactura = valorTexto;
                break;

            case "Fecha":
                factura.Fecha = ParsearFecha(valorTexto, campoFormatoFecha);
                break;

            case "BaseImponible":
                factura.BaseImponible = ParsearDecimal(valorTexto);
                break;

            case "PorcentajeIVA":
                factura.PorcentajeIVA = ParsearDecimal(valorTexto);
                break;

            case "CuotaIVA":
                factura.CuotaIVA = ParsearDecimal(valorTexto);
                break;

            case "PorcentajeIRPF":
                factura.PorcentajeIRPF = ParsearDecimal(valorTexto);
                break;

            case "CuotaIRPF":
                factura.CuotaIRPF = ParsearDecimal(valorTexto);
                break;

            case "PorcentajeRE":
                factura.PorcentajeRE = ParsearDecimal(valorTexto);
                break;

            case "CuotaRE":
                factura.CuotaRE = ParsearDecimal(valorTexto);
                break;

            case "TotalFactura":
                factura.TotalFactura = ParsearDecimal(valorTexto);
                break;

            case "SubTotal":
                factura.SubTotal = ParsearDecimal(valorTexto);
                break;

            case "ReceptorNombre":
                factura.Receptor.Nombre = valorTexto;
                break;

            case "ReceptorNif":
                factura.Receptor.NIF = valorTexto;
                break;

            case "EmisorNif":
                factura.Emisor.NIF = valorTexto;
                break;

            case "ConceptoIngreso":
                factura.ConceptoIngreso = valorTexto;
                break;

            case "ConceptoGasto":
                factura.ConceptoGasto = valorTexto;
                break;
        }
    }

    private DateTime? ParsearFecha(string valorTexto, string? formatoFecha)
    {
        if (string.IsNullOrEmpty(formatoFecha))
        {
            var cultura = new CultureInfo(
                string.IsNullOrEmpty(_config.CulturaFecha) ? "es-ES" : _config.CulturaFecha);
            if (DateTime.TryParse(valorTexto, cultura, DateTimeStyles.None, out var fecha))
                return fecha;
            return null;
        }

        var culturaEspecifica = new CultureInfo(
            string.IsNullOrEmpty(_config.CulturaFecha) ? "es-ES" : _config.CulturaFecha);
        if (DateTime.TryParseExact(valorTexto, formatoFecha, culturaEspecifica,
            DateTimeStyles.None, out var fe))
            return fe;

        return null;
    }

    private void AsignarSuma(Factura factura, CampoConfig campo)
    {
        if (campo.CamposSuma == null || campo.CamposSuma.Count == 0) return;

        decimal suma = 0m;
        foreach (var nombre in campo.CamposSuma)
        {
            suma += ObtenerValorDecimal(factura, nombre);
        }

        AsignarDecimal(factura, campo.Nombre, suma);
    }

    // ── Post-procesamiento ───────────────────────────────────────────────────

    private void AplicarPostProcesamiento(Factura factura, string texto)
    {
        foreach (var regla in _config.PostProcesamiento)
        {
            var accion = regla.Accion;
            if (accion == null || string.IsNullOrEmpty(accion.Tipo)) continue;

            if (!string.IsNullOrEmpty(regla.CondicionTextoContiene) &&
                !texto.Contains(regla.CondicionTextoContiene, StringComparison.OrdinalIgnoreCase))
                continue;

            switch (PostProcesamientoConfig.NormalizarTipo(accion.Tipo))
            {
                case "invertirsigno":
                    InvertirSigno(factura);
                    break;

                case "mayusculas":
                    PonerMayusculas(factura, accion.CampoDestino);
                    break;

                case "establecervalor":
                    EstablecerValor(factura, accion);
                    break;

                case "calcular":
                    CalcularCampo(factura, accion);
                    break;

                case "truncar":
                    TruncarCampo(factura, accion);
                    break;
            }
        }
    }

    private static void EstablecerValor(Factura factura, AccionPostProcesamiento accion)
    {
        var destino = accion.CampoDestino;
        if (string.IsNullOrEmpty(destino)) return;

        if (EsCampoNumerico(destino))
        {
            if (decimal.TryParse(accion.Valor, NumberStyles.Number,
                CultureInfo.GetCultureInfo("es-ES"), out var valor))
                AsignarDecimal(factura, destino, valor);
        }
        else
        {
            AsignarTexto(factura, destino, accion.Valor);
        }
    }

    private static void CalcularCampo(Factura factura, AccionPostProcesamiento accion)
    {
        if (string.IsNullOrEmpty(accion.CampoDestino) || !EsCampoNumerico(accion.CampoDestino)) return;

        var a = ObtenerValorDecimal(factura, accion.CampoOrigen1);
        var b = ObtenerValorDecimal(factura, accion.CampoOrigen2);
        var resultado = accion.Operador switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b == 0m ? a : a / b,
            _ => a
        };
        AsignarDecimal(factura, accion.CampoDestino, resultado);
    }

    private static void TruncarCampo(Factura factura, AccionPostProcesamiento accion)
    {
        var destino = accion.CampoDestino;
        if (string.IsNullOrEmpty(destino)) return;
        if (!int.TryParse(accion.Valor, out var longitud) || longitud < 0) return;

        var texto = ObtenerValorTexto(factura, destino);
        if (texto == null) return;
        AsignarTexto(factura, destino, texto[..Math.Min(longitud, texto.Length)]);
    }

    private static string? ObtenerValorTexto(Factura factura, string nombreCampo)
    {
        return nombreCampo switch
        {
            "NumeroFactura" => factura.NumeroFactura,
            "ReceptorNombre" => factura.Receptor.Nombre,
            "ReceptorNif" => factura.Receptor.NIF,
            "EmisorNombre" => factura.Emisor.Nombre,
            "EmisorNif" => factura.Emisor.NIF,
            "ConceptoIngreso" => factura.ConceptoIngreso,
            "ConceptoGasto" => factura.ConceptoGasto,
            _ => null
        };
    }

    private static void AsignarTexto(Factura factura, string nombreCampo, string valor)
    {
        switch (nombreCampo)
        {
            case "NumeroFactura": factura.NumeroFactura = valor; break;
            case "ReceptorNombre": factura.Receptor.Nombre = valor; break;
            case "ReceptorNif": factura.Receptor.NIF = valor; break;
            case "EmisorNombre": factura.Emisor.Nombre = valor; break;
            case "EmisorNif": factura.Emisor.NIF = valor; break;
            case "ConceptoIngreso": factura.ConceptoIngreso = valor; break;
            case "ConceptoGasto": factura.ConceptoGasto = valor; break;
        }
    }

    public static bool EsCampoNumerico(string nombreCampo)
    {
        return nombreCampo is "BaseImponible" or "CuotaIVA" or "CuotaIRPF" or "CuotaRE"
            or "TotalFactura" or "SubTotal" or "PorcentajeIVA" or "PorcentajeIRPF" or "PorcentajeRE";
    }

    public static bool EsCampoTexto(string nombreCampo)
    {
        return nombreCampo is "NumeroFactura" or "ReceptorNombre" or "ReceptorNif"
            or "EmisorNombre" or "EmisorNif" or "ConceptoIngreso" or "ConceptoGasto";
    }

    private static void InvertirSigno(Factura factura)
    {
        factura.BaseImponible *= -1;
        factura.CuotaIVA *= -1;
        factura.CuotaIRPF *= -1;
        factura.CuotaRE *= -1;
        factura.TotalFactura *= -1;
        factura.SubTotal *= -1;
    }

    private static void PonerMayusculas(Factura factura, string nombreCampo)
    {
        switch (nombreCampo)
        {
            case "ReceptorNombre":
                factura.Receptor.Nombre = factura.Receptor.Nombre.ToUpper();
                break;
            case "EmisorNombre":
                factura.Emisor.Nombre = factura.Emisor.Nombre.ToUpper();
                break;
        }
    }

    // ── Helpers de lectura de campos ─────────────────────────────────────────

    private static decimal ObtenerValorDecimal(Factura factura, string nombreCampo)
    {
        return nombreCampo switch
        {
            "BaseImponible" => factura.BaseImponible,
            "CuotaIVA" => factura.CuotaIVA,
            "CuotaIRPF" => factura.CuotaIRPF,
            "CuotaRE" => factura.CuotaRE,
            "TotalFactura" => factura.TotalFactura,
            "SubTotal" => factura.SubTotal,
            "PorcentajeIVA" => factura.PorcentajeIVA,
            "PorcentajeIRPF" => factura.PorcentajeIRPF,
            "PorcentajeRE" => factura.PorcentajeRE,
            _ => 0m
        };
    }

    private static void AsignarDecimal(Factura factura, string nombreCampo, decimal valor)
    {
        switch (nombreCampo)
        {
            case "BaseImponible": factura.BaseImponible = valor; break;
            case "CuotaIVA": factura.CuotaIVA = valor; break;
            case "CuotaIRPF": factura.CuotaIRPF = valor; break;
            case "CuotaRE": factura.CuotaRE = valor; break;
            case "TotalFactura": factura.TotalFactura = valor; break;
            case "SubTotal": factura.SubTotal = valor; break;
            case "PorcentajeIVA": factura.PorcentajeIVA = valor; break;
            case "PorcentajeIRPF": factura.PorcentajeIRPF = valor; break;
            case "PorcentajeRE": factura.PorcentajeRE = valor; break;
        }
    }
}
