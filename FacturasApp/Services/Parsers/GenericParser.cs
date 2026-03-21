using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class GenericParser : BaseParser
    {
        public override string Nombre => "Genérico";
        public override bool PuedeParsar(string texto) => true;

        private static readonly Regex RegexNumero = new(
            @"(?:factura|fra\.?|nº|n[uú]mero)[:\s#]*([A-Z0-9][-A-Z0-9/\\]{2,20})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"\b(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b([A-Z]?\d{7,8}[A-Z])\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"(?:base\s+imponible|subtotal|base)[:\s]*([\d.,]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA\s*(\d{1,2})\s*%",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"(?:total\s+factura|total\s+a\s+pagar|importe\s+total|total)[:\s]*([\d.,]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"^([A-ZÁÉÍÓÚÑ][A-Za-záéíóúñ\s,\.]{5,60})(?:\n|S\.L\.|S\.A\.|NIF|CIF)",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr
            };

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);

            var nifs = ExtraerTodosLosNifs(texto);
            factura.Emisor = new Proveedor
            {
                Nombre = ExtraerGrupo(RegexNombre, texto, 1),
                NIF = nifs.Count > 0 ? nifs[0] : string.Empty
            };
            factura.Receptor = new Cliente
            {
                NIF = nifs.Count > 1 ? nifs[1] : string.Empty
            };

            factura.BaseImponible = ExtraerDecimal(RegexBase, texto, 1);
            factura.PorcentajeIVA = ExtraerPorcentajeIva(texto);
            factura.PorcentajeIRPF = ExtraerPorcentajeIRPF(texto);
            factura.PorcentajeRE = ExtraerPorcentajeRE(texto);
            factura.TotalExtraido = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }

        private DateTime? ExtraerFecha(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;
            return DateTime.TryParse(
                $"{m.Groups[1].Value}/{m.Groups[2].Value}/{m.Groups[3].Value}",
                new System.Globalization.CultureInfo("es-ES"),
                System.Globalization.DateTimeStyles.None, out var f) ? f : null;
        }

        private decimal ExtraerPorcentajeIva(string texto)
        {
            var m = RegexIva.Match(texto);
            return m.Success && decimal.TryParse(
                m.Groups[1].Value, out var pct) ? pct : 0m;
        }

        private List<string> ExtraerTodosLosNifs(string texto)
        {
            return RegexNif.Matches(texto)
                .Select(m => m.Groups[1].Value.ToUpper())
                .Distinct()
                .ToList();
        }
    }
}