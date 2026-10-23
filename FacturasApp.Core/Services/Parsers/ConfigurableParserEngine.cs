using System.Text.RegularExpressions;
using System.Globalization;
using FacturasApp.Core.Models;
using FacturasApp.Core.Models.EmisoresConfig;

namespace FacturasApp.Core.Services.Parsers;

public class ConfigurableParserEngine : BaseParser
{
    private readonly EmisorConfig _config;

    public EmisorConfig Config => _config;

    public ConfigurableParserEngine(EmisorConfig config)
    {
        _config = config;
    }

    public override string Nombre => _config.Nombre;
    public override string Nif => _config.Nif;

    public override ModoExtraccion ModoExtraccion =>
        Enum.TryParse<ModoExtraccion>(_config.ModoExtraccion,
            ignoreCase: true, out var modo)
            ? modo
            : ModoExtraccion.Ordenado;

    public override bool PuedeParsar(string texto)
    {
        if (_config.Identificadores == null || _config.Identificadores.Count == 0)
            return false;
        return _config.Identificadores
            .All(id => texto.Contains(id, StringComparison.OrdinalIgnoreCase));
    }

    public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
    {
        if (TieneMultiLinea())
            return ParsearMultiple(texto, rutaArchivo, viaOcr).First();
        return ParsearUnica(texto, rutaArchivo, viaOcr);
    }

    public override List<Factura> ParsearMultiple(string texto, string rutaArchivo, bool viaOcr)
    {
        if (TieneMultiLinea())
            return ParsearMultiLinea(texto, rutaArchivo, viaOcr);
        return [ParsearUnica(texto, rutaArchivo, viaOcr)];
    }

    private bool TieneMultiLinea()
    {
        var multi = _config.MultiLinea;
        if (multi == null) return false;
        if (multi.Lineas.Count > 0) return true;
        return !string.IsNullOrEmpty(multi.RegexLinea);
    }

    // ── Factura única ────────────────────────────────────────────────────────

    private Factura ParsearUnica(string texto, string rutaArchivo, bool viaOcr)
    {
        var factura = CrearFacturaBase(rutaArchivo, viaOcr);
        factura.ConceptoGasto = string.IsNullOrEmpty(_config.ConceptoGasto) ? "600" : _config.ConceptoGasto;
        factura.ConceptoIngreso = string.IsNullOrEmpty(_config.ConceptoIngreso) ? "700" : _config.ConceptoIngreso;

        foreach (var campo in _config.Campos)
            ExtraerYAsignarCampo(factura, campo, texto);

        AplicarPostProcesamiento(factura, texto);
        factura.Estado = FacturaEstado.Determinar(factura);
        return factura;
    }

    // ── Multi-Línea ──────────────────────────────────────────────────────────

    private List<Factura> ParsearMultiLinea(string texto, string rutaArchivo, bool viaOcr)
    {
        var lineas = ObtenerLineasMultiLinea();
        if (lineas.Count == 0)
            return [ParsearUnica(texto, rutaArchivo, viaOcr)];

        var facturas = new List<Factura>();

        foreach (var linea in lineas)
        {
            var regex = new Regex(linea.Regex, RegexOptions.Compiled);
            foreach (Match match in regex.Matches(texto))
            {
                var factura = CrearFacturaBase(rutaArchivo, viaOcr);
                factura.EsMultiLinea = true;
                factura.ConceptoGasto = string.IsNullOrEmpty(_config.ConceptoGasto) ? "600" : _config.ConceptoGasto;
                factura.ConceptoIngreso = string.IsNullOrEmpty(_config.ConceptoIngreso) ? "700" : _config.ConceptoIngreso;

                var esCampoLinea = new HashSet<string>(
                    linea.MapeoCampos
                        .Select(m => m.Nombre),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var campo in _config.Campos)
                {
                    if (esCampoLinea.Contains(campo.Nombre))
                        continue;
                    // En multilínea el SubTotal NUNCA se extrae: se calcula
                    if (campo.Nombre == "SubTotal")
                        continue;

                    ExtraerYAsignarCampo(factura, campo, texto);
                }

                foreach (var mapeo in linea.MapeoCampos)
                {
                    // En multilínea el SubTotal NUNCA se extrae: se calcula
                    if (mapeo.Nombre == "SubTotal")
                        continue;
                    if (mapeo.Grupo >= match.Groups.Count) continue;
                    var valor = match.Groups[mapeo.Grupo].Value.Trim();
                    if (!string.IsNullOrEmpty(valor))
                        AsignarCampo(factura, mapeo.Nombre, valor);
                }

                AplicarPostProcesamiento(factura, texto);
                CalcularSubTotalMultiLinea(factura);

                // Líneas con base imponible 0 (regex demasiado suelto, línea
                // sin importe real…): se descartan, sea cual sea el número de
                // líneas que detecte el regex.
                if (factura.BaseImponible == 0m)
                    continue;

                facturas.Add(factura);
            }
        }

        // Ni matches ni líneas válidas (todas con base 0): se cae a factura
        // única, como cuando el regex no encuentra nada.
        if (facturas.Count == 0)
            return [ParsearUnica(texto, rutaArchivo, viaOcr)];

        // El Total Calculado es ÚNICO para toda la factura (todas las líneas
        // comparten el mismo valor): la suma de los subtotales calculados de
        // las líneas. Se fija ANTES de validar.
        var totalCalculado = facturas.Sum(f => f.SubTotal);
        foreach (var factura in facturas)
            factura.FijarTotalCalculado(totalCalculado);

        // Validación línea a línea: el paso 1 de FacturaEstado compara ese
        // TotalCalculado único con el TotalFactura extraído del documento.
        foreach (var factura in facturas)
            factura.Estado = FacturaEstado.Determinar(factura);

        // Sin el total del documento no hay nada que validar.
        MarcarTotalDocumentoNoExtraido(facturas);

        // El Estado también es ÚNICO para toda la factura, como el
        // TotalCalculado: se propaga el peor de las líneas y la unión de
        // mensajes a todas ellas.
        UnificarEstadoMultiLinea(facturas);

        return facturas;
    }

    /// <summary>
    /// Subtotal de una línea de factura multilínea: nunca se extrae del
    /// documento, se calcula con los porcentajes de la propia línea
    /// (Base + IVA − IRPF + RE), con el mismo redondeo que
    /// <see cref="Factura.CuotaIVACalculado"/>.
    /// </summary>
    private static void CalcularSubTotalMultiLinea(Factura factura)
    {
        static decimal Impuesto(decimal baseImponible, decimal porcentaje) =>
            Math.Round(baseImponible * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);

        factura.SubTotal = factura.BaseImponible
            + Impuesto(factura.BaseImponible, factura.PorcentajeIVA)
            - Impuesto(factura.BaseImponible, factura.PorcentajeIRPF)
            + Impuesto(factura.BaseImponible, factura.PorcentajeRE);
    }

    private List<LineaConfig> ObtenerLineasMultiLinea()
    {
        var multi = _config.MultiLinea;
        if (multi == null)
            return new List<LineaConfig>();

        // Compatibilidad: config antiguo con una única línea (RegexLinea + MapeoCampos)
        if (multi.Lineas.Count == 0 && !string.IsNullOrEmpty(multi.RegexLinea))
            return [new LineaConfig
            {
                Regex = multi.RegexLinea,
                MapeoCampos = multi.MapeoCampos
            }];

        return multi.Lineas;
    }

    /// <summary>
    /// Si el TotalFactura del documento no se pudo extraer (vale 0) no hay
    /// nada que validar: se explica en todas las líneas y se dejan en Error
    /// (antes se dejaban en Revisar; sin total la factura no está validada).
    /// </summary>
    private static void MarcarTotalDocumentoNoExtraido(List<Factura> facturas)
    {
        if (facturas[0].TotalFactura != 0m)
            return;

        foreach (var factura in facturas)
        {
            factura.MensajeError.Add(
                "No se pudo extraer el Total Factura del documento: no se puede validar la suma de subtotales calculados");
            factura.Estado = EstadoFactura.Error;
        }
    }

    /// <summary>
    /// El Estado de una factura multilínea es ÚNICO, como el TotalCalculado:
    /// todas sus líneas pasan a compartir el peor estado entre ellas y la
    /// unión de sus mensajes (sin repetidos, en el orden en que aparecen),
    /// de forma que el detalle de cualquier línea explica el estado de la
    /// factura entera.
    /// </summary>
    private static void UnificarEstadoMultiLinea(List<Factura> facturas)
    {
        var estadoUnico = facturas.Select(f => f.Estado).Aggregate(Peor);

        var mensajes = facturas
            .SelectMany(f => f.MensajeError)
            .Distinct()
            .ToList();

        foreach (var factura in facturas)
        {
            factura.Estado = estadoUnico;
            factura.MensajeError = new List<string>(mensajes);
        }
    }

    /// <summary>
    /// Jerarquía de gravedad de los estados: Error > Duplicada > Revisar >
    /// OK > Pendiente.
    /// </summary>
    private static EstadoFactura Peor(EstadoFactura a, EstadoFactura b)
    {
        if (a == EstadoFactura.Error || b == EstadoFactura.Error) return EstadoFactura.Error;
        if (a == EstadoFactura.Duplicada || b == EstadoFactura.Duplicada) return EstadoFactura.Duplicada;
        if (a == EstadoFactura.Revisar || b == EstadoFactura.Revisar) return EstadoFactura.Revisar;
        if (a == EstadoFactura.OK || b == EstadoFactura.OK) return EstadoFactura.OK;
        return EstadoFactura.Pendiente;
    }

    // ── Extracción y asignación ──────────────────────────────────────────────

    private void ExtraerYAsignarCampo(Factura factura, CampoConfig campo, string texto)
    {
        if (campo.UsarRegexFechaGeneral && campo.Nombre == "Fecha")
        {
            factura.Fecha = ExtraerFecha(texto, _config.CulturaFecha);
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

        string? rawValue = null;

        if (!string.IsNullOrEmpty(campo.ValorFijo))
        {
            rawValue = campo.ValorFijo;
        }
        else if (!string.IsNullOrEmpty(campo.Regex))
        {
            // Case-sensitive: la regex del XML se evalúa con la capitalización
            // exacta del patrón (igual que las regex de línea multilínea).
            // Si un campo necesita tolerancia a mayúsculas, se lleva dentro
            // del propio patrón con (?i) / (?i:…).
            var regex = new Regex(campo.Regex, RegexOptions.Compiled);
            var match = regex.Match(texto);
            if (match.Success && match.Groups.Count > 1)
                rawValue = match.Groups[1].Value.Trim();
        }

        if (rawValue != null)
            AsignarCampo(factura, campo.Nombre, rawValue);
    }

    private void AsignarCampo(Factura factura, string nombreCampo, string valorTexto)
    {
        if (string.IsNullOrEmpty(valorTexto)) return;

        switch (nombreCampo)
        {
            case "NumeroFactura":
                factura.NumeroFactura = valorTexto;
                break;

            case "Fecha":
                factura.Fecha = ParsearFecha(valorTexto);
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

    private DateTime? ParsearFecha(string valorTexto)
    {
        if (string.IsNullOrEmpty(valorTexto)) return null;

        var cultura = ObtenerCulturaFecha(_config.CulturaFecha);
        if (DateTime.TryParse(valorTexto, cultura, DateTimeStyles.None, out var fecha))
            return fecha;

        // Tolerancia a nombres de mes que TryParse rechaza ("01 Sept 2026" en en-US)
        return ParsearFechaConNombreMes(valorTexto, cultura);
    }

    // ── Post-procesamiento ───────────────────────────────────────────────────

    private void AplicarPostProcesamiento(Factura factura, string texto)
    {
        foreach (var regla in _config.PostProcesamiento)
        {
            var accion = regla.Accion;
            if (accion == null || string.IsNullOrEmpty(accion.Tipo)) continue;

            // En multilínea el SubTotal lo calcula el motor (nunca se extrae):
            // una regla que intentara reescribirlo se ignora para no pelearse
            // con él. «invertirsigno» no lleva CampoDestino, no se ve afectada.
            if (factura.EsMultiLinea && accion.CampoDestino == "SubTotal")
                continue;

            if (PostProcesamientoConfig.NormalizarTipo(accion.Tipo) == "invertirsigno" &&
                !string.IsNullOrEmpty(regla.CondicionTextoContiene) &&
                !texto.Contains(regla.CondicionTextoContiene, StringComparison.OrdinalIgnoreCase))
                continue;

            if (PostProcesamientoConfig.NormalizarTipo(accion.Tipo) == "establecervalor" &&
                !EvaluarCondicionCampo(factura, regla.CondicionCampo))
                continue;

            switch (PostProcesamientoConfig.NormalizarTipo(accion.Tipo))
            {
                case "invertirsigno":
                    InvertirSigno(factura);
                    break;

                case "establecervalor":
                    EstablecerValor(factura, accion);
                    break;

                case "calcular":
                    CalcularCampo(factura, accion);
                    break;

                case "sustituir":
                    SustituirTexto(factura, accion);
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

    private static bool EvaluarCondicionCampo(Factura factura, CondicionCampoPostProcesamiento? condicion)
    {
        if (condicion == null || string.IsNullOrEmpty(condicion.Campo))
            return false;

        if (EsCampoNumerico(condicion.Campo))
        {
            if (!decimal.TryParse(condicion.Valor, NumberStyles.Number,
                CultureInfo.GetCultureInfo("es-ES"), out var esperado))
                return false;
            return ObtenerValorDecimal(factura, condicion.Campo) == esperado;
        }

        var actual = ObtenerValorTexto(factura, condicion.Campo);
        return actual != null &&
            actual.Equals(condicion.Valor, StringComparison.OrdinalIgnoreCase);
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
            "%" => a * b / 100m,
            _ => a
        };
        AsignarDecimal(factura, accion.CampoDestino, resultado);
    }

    /// <summary>
    /// Sustituye todas las ocurrencias exactas de <see cref="AccionPostProcesamiento.Valor"/>
    /// por <see cref="AccionPostProcesamiento.Sustituto"/> en el campo destino
    /// (sólo campos de texto; "Sustituto" vacío borra la cadena buscada).
    /// </summary>
    private static void SustituirTexto(Factura factura, AccionPostProcesamiento accion)
    {
        var destino = accion.CampoDestino;
        if (string.IsNullOrEmpty(destino) || string.IsNullOrEmpty(accion.Valor)) return;
        if (!EsCampoTexto(destino)) return;

        var actual = ObtenerValorTexto(factura, destino);
        if (string.IsNullOrEmpty(actual) || !actual.Contains(accion.Valor, StringComparison.Ordinal)) return;

        AsignarTexto(factura, destino,
            actual.Replace(accion.Valor, accion.Sustituto, StringComparison.Ordinal));
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
        if (factura.CuotaIVACalculadoFijada)
            factura.FijarCuotaIVACalculado(-factura.CuotaIVACalculado);
        if (factura.CuotaIRPFCalculadoFijada)
            factura.FijarCuotaIRPFCalculado(-factura.CuotaIRPFCalculado);
        if (factura.CuotaRECalculadoFijada)
            factura.FijarCuotaRECalculado(-factura.CuotaRECalculado);
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
            case "CuotaIVA":
                factura.CuotaIVA = valor;
                factura.FijarCuotaIVACalculado(valor);
                break;
            case "CuotaIRPF":
                factura.CuotaIRPF = valor;
                factura.FijarCuotaIRPFCalculado(valor);
                break;
            case "CuotaRE":
                factura.CuotaRE = valor;
                factura.FijarCuotaRECalculado(valor);
                break;
            case "TotalFactura": factura.TotalFactura = valor; break;
            case "SubTotal": factura.SubTotal = valor; break;
            case "PorcentajeIVA": factura.PorcentajeIVA = valor; break;
            case "PorcentajeIRPF": factura.PorcentajeIRPF = valor; break;
            case "PorcentajeRE": factura.PorcentajeRE = valor; break;
        }
    }
}
