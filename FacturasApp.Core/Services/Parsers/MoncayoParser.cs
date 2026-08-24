using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class MoncayoParser : BaseParser
    {
        public override string Nombre => "ROSA MARIA MONCAYO";
        public override string Nif => "25042336M";

        protected override string[] Identificadores =>
            ["rosa maria moncayo", "25042336m"];

        private static readonly Regex RegexNumero = new(
            @"No.fact.:\s+(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Nombre:\s+(.*)\s+Fecha",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"CIF\s+:\s+(.+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        private static readonly Regex RegexImportes = new(
            @"BASE IMPONIBLE.+IVA\s+(\d+)%.+[\r\n]+([\d,.]+)\s+",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };

            factura.Emisor.NIF = Nif;
            factura.Emisor.Nombre = Nombre;
            factura.ConceptoGasto = "629";
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);
            factura.TotalFactura = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
