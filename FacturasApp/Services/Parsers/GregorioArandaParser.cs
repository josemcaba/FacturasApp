using CsvHelper;
using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class GregorioArandaParser : BaseParser
    {
        public override string Nombre => "Gregorio Aranda Garcia";
        public override string Nif => "25693621E";

        private static readonly string[] Identificadores =
            { "25693621E"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"Número de Factura.*[\n\r]+(?:Fact-)?(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"Fecha de Facturación.*[\n\r]+(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Proveedor[\n\r]+(.*?)\s+Gregorio Aranda",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b([A-Z]?\d{7,8}[A-Z]?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"otal.*?([\d,.]+)[\n\r]*IVA\s*\(([\d]+)%\)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Envío[\s\n\r]+(?:[\d,.\s\n\r]*)Total\s+([\d.,]+)",
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
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif).ToUpper();
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = DeterminarEstado(factura);
            
            return factura;
        }
    }
}
