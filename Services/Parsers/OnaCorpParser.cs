using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public class OnaCorpParser : BaseParser
    {
        public override string Nombre => "ONA CORP, S.L.U.";
        public override string Nif => "B85002764";

        protected override string[] Identificadores =>
            ["ona corp", "ESB85002764", "+34915079385"];

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.LayoutAnalysis;

        private static readonly Regex RegexNumero = new(
            @"FACTURA(?:| RECTIFICATIVA)[\r\n]+(.+)[\r\n]+FECHA",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Dirección de Facturación[\r\n]+.*[\r\n]+(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"\bFECHA[\s\n\r]+(.*?)[\s\n\r]+N",
            RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"España[\r\n]+(.+)[\r\n]+Dirección",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        private static readonly Regex RegexImportes = new(
            @"Base Imponible ([\d,.]+)€[\r\n]+IVA Total \((\d+)%\) [\d,.]+€[\r\n]+TOTAL FACTURA ([\d,.]+)€",
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
            factura.Total = ExtraerDecimal(RegexImportes, texto, 3);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
