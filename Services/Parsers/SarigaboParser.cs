using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public partial class SarigaboParser : BaseParser
    {
        public override string Nombre => "IMPULSO SARIGABO, SLU";
        public override string Nif => "B25880733";

        protected override string[] Identificadores =>
            ["sarigabo", "B25880733"];

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.Simple;

        [GeneratedRegex(@"\s(FVR\d+)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"([A-ZÁÉÍÓÚÜÑ ]+)\s*(?:\r?\n\s*)+\s*Cliente\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Base Imponible
        [GeneratedRegex(@"\b\bTotal .mporte[^\d]+(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexImportes();

        // TOTAL FACTURA
        [GeneratedRegex(@"\bTota.\s+Factura\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalFactura();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            texto = Regex.Replace(texto, @"74890980.", "74890980J", RegexOptions.Singleline);

            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre(), texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            if (factura.Receptor.NIF.StartsWith("74890980")) { factura.Receptor.NIF = "74890980J"; }
            factura.BaseImponible = ExtraerDecimal(RegexImportes(), texto, 1);
            factura.PorcentajeIVA = 10.0m;
            factura.Total = ExtraerDecimal(RegexTotalFactura(), texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}