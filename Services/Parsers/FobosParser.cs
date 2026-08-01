using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class FobosParser : BaseParser
    {
        public override string Nombre => "ARNOLD CALLE TELECOM S.L.";
        public override string Nif => "B93013761";

        protected override string[] Identificadores =>
            ["B93013761", "FOBOS", "Telecom"];

        protected override Regex RegexFecha { get; } = new(
            @"Fecha emisión:\s*(\d{2})[\/\-](\d{2})[\/\-](\d{4})",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"Factura:\s*(.*?)\s",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Periodo:.*\n(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexBaseImponible = new(
            @"Base imponible:\s+([\d,.]+)\s*€",
            RegexOptions.Compiled);

        private static readonly Regex RegexPorcentajeIva = new(
            @"IVA\s+(\d+)\s*%",
            RegexOptions.Compiled);

        private static readonly Regex RegexCuotaIva = new(
            @"IVA\s+\d+\s*%\s*:\s*([\d,.]+)\s*€",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Importe:\s+([\d,.]+)\s*€",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.ConceptoGasto = "628";
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
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
