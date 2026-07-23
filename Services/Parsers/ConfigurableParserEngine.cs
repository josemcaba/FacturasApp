using System.Text.RegularExpressions;
using System.Globalization;
using FacturasApp.Models;
using FacturasApp.Models.EmisoresConfig;

namespace FacturasApp.Services.Parsers;

public class ConfigurableParserEngine : BaseParser
{
    private readonly EmisorConfig _config;
    private readonly Lazy<Regex> _regexMultiLinea;

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

                ExtraerYAsignarCampo(factura, campo, texto);
            }

            foreach (var mapeo in _config.MultiLineaIVA.MapeoCampos)
            {
                if (mapeo.Grupo >= match.Groups.Count) continue;
                var valor = match.Groups[mapeo.Grupo].Value.Trim();
                if (!string.IsNullOrEmpty(valor))
                    AsignarCampo(factura, mapeo.Nombre, valor, campoFormatoFecha: null);
            }

            foreach (var campo in camposSuma)
                AsignarSuma(factura, campo);

            AplicarPostProcesamiento(factura, texto);
            factura.Estado = FacturaEstado.Determinar(factura);
            facturas.Add(factura);
        }

        return facturas;
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

            case "Total":
                factura.Total = ParsearDecimal(valorTexto);
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
            if (!string.IsNullOrEmpty(regla.CondicionTextoContiene) &&
                !texto.Contains(regla.CondicionTextoContiene, StringComparison.OrdinalIgnoreCase))
                continue;

            switch (regla.Tipo.ToLowerInvariant())
            {
                case "invertirsigno":
                    foreach (var c in regla.CamposAfectados)
                        InvertirSigno(factura, c);
                    break;

                case "mayusculas":
                    foreach (var c in regla.CamposAfectados)
                        PonerMayusculas(factura, c);
                    break;
            }
        }
    }

    private static void InvertirSigno(Factura factura, string nombreCampo)
    {
        switch (nombreCampo)
        {
            case "BaseImponible": factura.BaseImponible *= -1; break;
            case "CuotaIVA": factura.CuotaIVA *= -1; break;
            case "CuotaIRPF": factura.CuotaIRPF *= -1; break;
            case "CuotaRE": factura.CuotaRE *= -1; break;
            case "Total": factura.Total *= -1; break;
        }
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
            "Total" => factura.Total,
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
            case "Total": factura.Total = valor; break;
            case "PorcentajeIVA": factura.PorcentajeIVA = valor; break;
            case "PorcentajeIRPF": factura.PorcentajeIRPF = valor; break;
            case "PorcentajeRE": factura.PorcentajeRE = valor; break;
        }
    }
}
