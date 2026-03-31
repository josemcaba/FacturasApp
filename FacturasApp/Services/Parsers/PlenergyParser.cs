using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class PlenergyParser : BaseParser
    {
        public override string Nombre => "PLENERGY GRUPO, S.L.";
        public override string Nif => "B93275394";

        private static readonly string[] Identificadores =
            { "PLENERGY GRUPO", "B93275394"};

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.LayoutAnalysis;

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"Nº FACTURA:\s+(.+)\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"FECHA:\s+(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"(.*)[\r\n]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"España[\r\n]+(.+)[\r\n]+Dirección",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            factura.Receptor.NIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Total = ExtraerDecimal(RegexImportes, texto, 3);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }
    }
}
