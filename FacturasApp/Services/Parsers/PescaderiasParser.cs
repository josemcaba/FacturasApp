using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class PescaderiasParser : BaseParser
    {
        public override string Nombre => "Pescaderías";

        private static readonly string[] Identificadores =
            { "pescadería", "pescaderia", "pescados", "mariscos", "lonja" };

        public override bool PuedeParsar(string texto) =>
            Identificadores.Any(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"(?:factura|fra\.?|nº)[:\s#]*([A-Z0-9][-A-Z0-9/]{1,15})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"\b(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{2,4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombreEmisor = new(
            @"^([A-ZÁÉÍÓÚÑ][A-Za-záéíóúñ\s]{5,50})(?:\n|S\.L\.|S\.A\.)",
            RegexOptions.Multiline | RegexOptions.Compiled);

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
                    Nombre = ExtraerNombreEmisor(texto),
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

        private string ExtraerNombreEmisor(string texto)
        {
            var m = RegexNombreEmisor.Match(texto);
            return m.Success ? m.Groups[1].Value.Trim() : "Pescadería (desconocida)";
        }

        private DateTime? ExtraerFecha(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;
            string anyo = m.Groups[3].Value;
            if (anyo.Length == 2) anyo = "20" + anyo;
            return DateTime.TryParse(
                $"{m.Groups[1].Value}/{m.Groups[2].Value}/{anyo}",
                new System.Globalization.CultureInfo("es-ES"),
                System.Globalization.DateTimeStyles.None, out var f) ? f : null;
        }

        private decimal ExtraerPorcentajeIva(string texto)
        {
            var m = RegexIva.Match(texto);
            return m.Success && decimal.TryParse(
                m.Groups[1].Value, out var pct) ? pct : 4m;
        }
    }
}