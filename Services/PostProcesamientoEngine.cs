using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services
{
    /// <summary>
    /// Motor de reglas de post-procesamiento declarativas.
    /// Evalúa condiciones y ejecuta acciones sobre la Factura extraída.
    /// </summary>
    public class PostProcesamientoEngine
    {
        public void Aplicar(List<ReglaPostProcesamiento> reglas, Factura factura, string texto)
        {
            foreach (var regla in reglas)
            {
                if (EvaluarCondicion(regla.Condicion, factura, texto))
                {
                    foreach (var accion in regla.Acciones)
                        EjecutarAccion(accion, factura);
                }
            }
        }

        private bool EvaluarCondicion(CondicionRegla? condicion, Factura f, string texto)
        {
            if (condicion == null) return true; // Sin condición = siempre ejecutar

            // Condición por contenido de texto
            if (!string.IsNullOrEmpty(condicion.TextoContiene))
                return texto.Contains(condicion.TextoContiene, StringComparison.OrdinalIgnoreCase);

            // Condición por valor de campo
            if (string.IsNullOrEmpty(condicion.Campo)) return true;

            decimal valorCampo = ObtenerValorCampo(condicion.Campo, f);
            decimal valorComparacion = 0;

            if (!string.IsNullOrEmpty(condicion.Valor))
            {
                string valorStr = condicion.Valor.Replace(",", ".");
                if (!decimal.TryParse(valorStr,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out valorComparacion))
                    return false;
            }

            return condicion.Operador switch
            {
                "Igual" => valorCampo == valorComparacion,
                "Distinto" => valorCampo != valorComparacion,
                "MayorQue" => valorCampo > valorComparacion,
                "MenorQue" => valorCampo < valorComparacion,
                "MayorIgual" => valorCampo >= valorComparacion,
                "MenorIgual" => valorCampo <= valorComparacion,
                "NoVacio" => valorCampo != 0,
                _ => false
            };
        }

        private void EjecutarAccion(AccionRegla accion, Factura f)
        {
            switch (accion.Tipo)
            {
                case "SetValor":
                    AsignarValorCampo(accion.Campo, accion.Valor, f);
                    break;

                case "MultiplicarPor":
                    if (decimal.TryParse(accion.Valor.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal factor))
                    {
                        decimal actual = ObtenerValorCampo(accion.Campo, f);
                        AsignarValorCampo(accion.Campo, (actual * factor).ToString(
                            System.Globalization.CultureInfo.InvariantCulture), f);
                    }
                    break;

                case "SumarCampo":
                    if (!string.IsNullOrEmpty(accion.CampoFuente))
                    {
                        decimal sumando = ObtenerValorCampo(accion.CampoFuente, f);
                        decimal actual = ObtenerValorCampo(accion.Campo, f);
                        AsignarValorCampo(accion.Campo, (actual + sumando).ToString(
                            System.Globalization.CultureInfo.InvariantCulture), f);
                    }
                    break;

                case "UsarSiVacio":
                    if (!string.IsNullOrEmpty(accion.CampoFuente))
                    {
                        decimal actual = ObtenerValorCampo(accion.Campo, f);
                        if (actual == 0)
                        {
                            decimal fuente = ObtenerValorCampo(accion.CampoFuente, f);
                            AsignarValorCampo(accion.Campo, fuente.ToString(
                                System.Globalization.CultureInfo.InvariantCulture), f);
                        }
                    }
                    break;
            }
        }

        private static decimal ObtenerValorCampo(string campo, Factura f)
        {
            return campo switch
            {
                "BaseImponible" => f.BaseImponible,
                "PorcentajeIVA" => f.PorcentajeIVA,
                "CuotaIVA" => f.CuotaIVA,
                "PorcentajeIRPF" => f.PorcentajeIRPF,
                "CuotaIRPF" => f.CuotaIRPF,
                "PorcentajeRE" => f.PorcentajeRE,
                "CuotaRE" => f.CuotaRE,
                "Total" => f.Total,
                _ => 0m
            };
        }

        private static void AsignarValorCampo(string campo, string valorStr, Factura f)
        {
            if (!decimal.TryParse(valorStr.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal valor))
                return;

            switch (campo)
            {
                case "BaseImponible": f.BaseImponible = valor; break;
                case "PorcentajeIVA": f.PorcentajeIVA = valor; break;
                case "CuotaIVA": f.CuotaIVA = valor; break;
                case "PorcentajeIRPF": f.PorcentajeIRPF = valor; break;
                case "CuotaIRPF": f.CuotaIRPF = valor; break;
                case "PorcentajeRE": f.PorcentajeRE = valor; break;
                case "CuotaRE": f.CuotaRE = valor; break;
                case "Total": f.Total = valor; break;
                case "Concepto": f.Concepto = valorStr; break;
            }
        }
    }
}
