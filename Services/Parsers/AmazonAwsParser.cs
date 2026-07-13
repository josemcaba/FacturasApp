using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class AmazonAwsParser : BaseParser
    {
        public override string Nombre => "AMAZON WEB SERVICES EMEA SARL, ESPAÑA";
        public override string Nif => "W0185696B";

        protected override string[] Identificadores =>
            ["W0185696B", "AMAZON WEB SERVICES"];

        private static readonly Regex RegexNumero = new(
            @"VAT Invoice Number:\s+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexFechaIngles = new(
            @"VAT Invoice Date:\s+(\w+ \d{1,2}, \d{4})",
            RegexOptions.Compiled);

        private static readonly Regex RegexClienteNombre = new(
            @"Address:[^\n]*\n([^\n]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexClienteNif = new(
            @"Tax Registration Number:\s+ES(\d{8}[A-Z])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexBaseImponible = new(
            @"Net Charges.*EUR\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexPorcentajeIva = new(
            @"VAT\s+-\s+(\d+)%",
            RegexOptions.Compiled);

        private static readonly Regex RegexCuotaIva = new(
            @"VAT\s+-\s+\d+%\s+EUR\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL AMOUNT EUR\s+([\d,.]+)",
            RegexOptions.Compiled);

        private DateTime? ExtraerFechaIngles(string texto)
        {
            var m = RegexFechaIngles.Match(texto);
            if (!m.Success) return null;

            string fechaStr = m.Groups[1].Value;
            string[] formatos = ["MMMM d, yyyy", "MMM d, yyyy"];

            return DateTime.TryParseExact(
                fechaStr, formatos,
                new CultureInfo("en-US"),
                DateTimeStyles.None,
                out var fecha) ? fecha : null;
        }

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFechaIngles(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexClienteNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(RegexClienteNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexPorcentajeIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexCuotaIva, texto, 1);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
