using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class GregorioArandaParser : BaseParser
    {
        public override string Nombre => "Gregorio Aranda Garcia";
        public override string Nif => "25693621E";

        protected override string[] Identificadores =>
            ["25693621E", "fecha de vencimiento", "datos proveedor"];

        private static readonly Regex RegexNumero = new(
            @"Número de Factura.*[\n\r]+(?:Fact-)?(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Datos Cliente[\n\r]+(.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"\b([A-Z]?\d{7,8}[A-Z]?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        private static readonly Regex RegexImportes = new(
            @"otal.*?([\d,.]+)[\n\r]*IVA\s*\(([\d]+)%\)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"\bFacturación[\s\r\n]+(.*?)\s",
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
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);
            
            return factura;
        }
    }
}
