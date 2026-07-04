using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class SarigaboParser : BaseParser
    {
        public override string Nombre => "Sarigabo, S.L.";
        public override string Nif => "B41256264";

        protected override string[] Identificadores =>
            ["sarigabo", "b41256264"];

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.Simple;

        [GeneratedRegex(@"\s(FVR\d+)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"\[Zona2\]:\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Base Imponible
        [GeneratedRegex(@"\bTotal Importe\s+([,\.0-9]+)€\s", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexImportes();

        // TOTAL FACTURA
        [GeneratedRegex(@"\bTotal Factura\s+([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalFactura();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre(), texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes(), texto, 1);
            factura.PorcentajeIVA = 10.0m;
            factura.Total = ExtraerDecimal(RegexTotalFactura(), texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}