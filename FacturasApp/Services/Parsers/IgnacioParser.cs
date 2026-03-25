using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class IgnacioParser : BaseParser
    {
        public override string Nombre => "Ignacio";

        public override bool PuedeParsar(string texto) =>
            texto.Contains("ignacio", StringComparison.OrdinalIgnoreCase);

        private static readonly Regex RegexNumero = new(
            @"(?:factura|fra\.?|nº)[:\s#]*([A-Z0-9][-A-Z0-9/]{1,15})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"\b(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b([A-Z]?\d{7,8}[A-Z])\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"(?:base\s*imponible|base)[:\s]*([\d.,]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA\s*(\d{1,2})\s*%",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"(?:total\s+a\s+pagar|total\s+factura|total)[:\s€]*([\d.,]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                Emisor = new Proveedor
                {
                    Nombre = "Ignacio",
                    NIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper()
                }
            };

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
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
                m.Groups[1].Value, out var pct) ? pct : 21m;
        }
    }
}