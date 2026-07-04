using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class FiestaParser : BaseParser
    {
        public override string Nombre => "Fiesta Colombina, S.L.U.";
        public override string Nif => "B85905412";

        protected override string[] Identificadores =>
            ["fiesta colombina s.l.u", "n.i.f.:b85905412"];

        [GeneratedRegex(@"FACTURA:\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        protected override Regex RegexFecha { get; } = new(
            @"\bFECHA\s+(\d{1,2}[\/\.-](?:\d{1,2}|\D{3})[\/\.-]\d{2,4})\s+FECHA\b",
            RegexOptions.Compiled);

        [GeneratedRegex(@"NOMBRE:\s+(.+?\s+.+?\s+.+?)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Base Imponible
        [GeneratedRegex(@"BASE IMPONIBLE EUR 10% ([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexBaseImponible();

        // TOTAL FACTURA
        [GeneratedRegex(@"\bTOTAL EUR ([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalFactura();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre(), texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible(), texto, 1);
            factura.PorcentajeIVA = 10;
            factura.Total = ExtraerDecimal(RegexTotalFactura(), texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}