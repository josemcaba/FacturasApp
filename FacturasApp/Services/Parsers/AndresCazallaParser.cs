using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class AndresCazallaParser : BaseParser
    {
        public override string Nombre => "Andrés Cazalla";

        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

        private static readonly string[] Identificadores =
            { "semirueda", "Andres Cazalla" };

        public override bool PuedeParsar(string texto) =>
            Identificadores.Any(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"FACTURA\s+(20[\d]{5})\s+",
            RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"FACTURA\s+[\d]+\s+([\d/]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b([A-Z]?\d{7,8}[A-Z]?)\s+\d{6}\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"([\d,]+)\s[\d,]+\s.*\n[\d,]+\s[\d,]+\s[\d,]+\s[\d,]+",

            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"[\d,]+\s[\d,]+\s.*\n(\d+)\s[\d,]\s[\d,]+\s[\d,]+",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"[\d,]+\s[\d,]+\s.*\n\d+\s[\d,]\s[\d,]+\s([\d,]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                Emisor = new Proveedor { Nombre = "Andrés Cazalla" }
            };

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Emisor.NIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();
            factura.BaseImponible = ExtraerDecimal(RegexBase, texto, 1);
            factura.PorcentajeIVA = ExtraerPorcentajeIva(texto);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }

        private DateTime? ExtraerFecha(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;
            return DateTime.TryParse(
                m.Groups[1].Value,
                new System.Globalization.CultureInfo("es-ES"),
                System.Globalization.DateTimeStyles.None, out var f) ? f : null;
        }

        private decimal ExtraerPorcentajeIva(string texto)
        {
            var m = RegexIva.Match(texto);
            return m.Success && decimal.TryParse(
                m.Groups[1].Value, out var pct) ? pct : 10m;
        }
    }
}
