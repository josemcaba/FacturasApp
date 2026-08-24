using FacturasApp.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class EsprinetParser : BaseParser
    {
        public override string Nombre => "Esprinet Ibérica, S.L.";
        public override string Nif => "B84443985";

        protected override string[] Identificadores =>
            ["B84443985", "Esprinet"];

        protected override Regex RegexFecha { get; } = new(
            @"COMERCIAL / SALES REP\s+(\d{1,2}/\d{2}/\d{2})",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"COMERCIAL / SALES REP.*?(V\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"FACTURADO A.*\n(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexImportes = new(
            @"Iva\s+(\d+)\s*%\s+([\d,.]+)\s+[\d,.]+\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL INVOICE\s+EUR\s+([\d,.]+)",
            RegexOptions.Compiled);

        private DateTime? ExtraerFechaCorta(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;

            return DateTime.TryParse(
                m.Groups[1].Value,
                new CultureInfo("es-ES"),
                DateTimeStyles.None,
                out var fecha) ? fecha : null;
        }

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFechaCorta(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexImportes, texto, 3);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
