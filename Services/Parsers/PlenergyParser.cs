using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class PlenergyParser : BaseParser
    {
        public override string Nombre => "PLENERGY GRUPO, S.L.";
        public override string Nif => "B93275394";

        protected override string[] Identificadores =>
            ["PLENERGY GRUPO", "B93275394"];

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.Simple;

        private static readonly Regex RegexNumero = new(
            @"Nº FACTURA:\s+(.+)\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"(.*)[\r\n]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"Málaga[\r\n]+(.+)[\r\n]+PLENERGY",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        protected override Regex RegexFecha { get; } = new(
            @"\bFECHA:\s*(.*?)[\s\n\r]+CLIENTE",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"Importe\s+TOTAL[\r\n]+([\d,.]+)\s+(\d+)\s*%\s+([\d,.]+)\s+[\d,.]+\s*€\s+([\d,.]+)\s*€",
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
            factura.ConceptoGasto = "624";
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.TotalFactura = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
