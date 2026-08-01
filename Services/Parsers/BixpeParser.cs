using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class BixpeParser : BaseParser
    {
        public override string Nombre => "Abbanza Research INT S.L.";
        public override string Nif => "B84596113";

        protected override string[] Identificadores =>
            ["B-84596113", "Abbanza Research INT"];

        private static readonly Regex RegexReceptorNombre = new(
            @"^([^\n(]+)\s+\(\d+\)\s+Fecha",
             RegexOptions.Compiled | RegexOptions.Multiline);
        protected override Regex RegexFecha { get; } = new(
            @"Fecha de la factura\s+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"N.*\s*de\s*factura\s+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBaseImponible = new(
            @"Subtotal\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexPorcentajeIva = new(
            @"IVA\s+\((\d+)%\)",
            RegexOptions.Compiled);

        private static readonly Regex RegexCuotaIva = new(
            @"IVA\s+\(\d+%\)\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total\s+([\d,.]+)€",
            RegexOptions.Compiled);

        private DateTime? ExtraerFechaEspañol(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;

            string fechaStr = m.Groups[1].Value.Trim();
            string[] formatos = ["d MMMM yyyy", "d MMM yyyy"];

            return DateTime.TryParseExact(
                fechaStr, formatos,
                new CultureInfo("es-ES"),
                DateTimeStyles.None,
                out var fecha) ? fecha : null;
        }

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFechaEspañol(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexPorcentajeIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexCuotaIva, texto, 1);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
