using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class MoncayoParser : BaseParser
    {
        public override string Nombre => "ROSA MARIA MONCAYO";
        public override string Nif => "25042336M";
        public override string Concepto => "629"; // Concepto: OTROS SERVICIOS

        private static readonly string[] Identificadores =
            { "rosa maria moncayo", "25042336m"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"No.fact.:\s+(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"Fecha:\s+(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Nombre:\s+(.*)\s+Fecha",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"CIF\s+:\s+(.+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexBaseImponible = new(
            @"BASE IMPONIBLE.+[\r\n]+([\d,.]+)\s+",
            RegexOptions.Compiled);

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
            factura.Concepto = Concepto;
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);
            factura.Total = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }
    }
}
