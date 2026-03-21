using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class MercadonaParser : BaseParser
    {
        public override string Nombre => "Mercadona";

        public override bool PuedeParsar(string texto) =>
            texto.Contains("mercadona", StringComparison.OrdinalIgnoreCase) ||
            texto.Contains("A-46103834");

        private static readonly Regex RegexNumero = new(
            @"FACTURA\s+SIMPLIFICADA\s+N[Ooº°]\s*(\d{4}-\d{6})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"\b(\d{2}/\d{2}/\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL[:\s€]*([\d.,]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexTipoIva = new(
            @"(\d{1,2})%\s*[\|]?\s*([\d.,]+)\s*[\|]?\s*([\d.,]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                Emisor = new Proveedor
                {
                    Nombre = "Mercadona S.A.",
                    NIF = "A-46103834"
                }
            };

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.TotalExtraido = ExtraerDecimal(RegexTotal, texto, 1);
            (factura.BaseImponible, factura.PorcentajeIVA) = ExtraerIva(texto);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }

        private DateTime? ExtraerFecha(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;
            return DateTime.TryParseExact(m.Groups[1].Value, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var f) ? f : null;
        }

        private (decimal baseImp, decimal pctIva) ExtraerIva(string texto)
        {
            decimal baseTotal = 0m;
            decimal pctPrincipal = 10m;

            foreach (Match m in RegexTipoIva.Matches(texto))
            {
                if (!decimal.TryParse(m.Groups[1].Value, out decimal pct)) continue;
                string baseStr = m.Groups[2].Value
                    .Replace(".", "").Replace(",", ".");
                if (decimal.TryParse(baseStr,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal baseIva))
                {
                    baseTotal += baseIva;
                    pctPrincipal = pct;
                }
            }

            return (baseTotal, pctPrincipal);
        }
    }
}