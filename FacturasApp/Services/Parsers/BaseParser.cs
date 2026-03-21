using System.Text.RegularExpressions;
using FacturasApp.Models;
using FacturasApp.Services;

namespace FacturasApp.Services.Parsers
{
    public abstract class BaseParser : IInvoiceParser
    {
        public abstract string Nombre { get; }
        public abstract bool PuedeParsar(string texto);
        public abstract Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        public virtual PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.LayoutAnalysis;

        // ── Regex comunes ────────────────────────────────────────────────────

        protected static readonly Regex RegexIRPF = new(
            @"IRPF\s*(\d{1,2})\s*%[:\s]*([\d.,]+)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected static readonly Regex RegexRE = new(
            @"(?:recargo\s+de\s+equivalencia|recargo\s+equiv\.?|R\.?E\.?)\s*" +
            @"(\d{1,2}[.,]\d{1,2}|\d{1,2})\s*%[:\s]*([\d.,]+)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // ── Helpers de extracción ────────────────────────────────────────────

        protected string ExtraerGrupo(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            return m.Success ? m.Groups[grupo].Value.Trim() : string.Empty;
        }

        protected decimal ExtraerDecimal(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            if (!m.Success) return 0m;
            return ParsearDecimal(m.Groups[grupo].Value);
        }

        protected decimal ParsearDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            string v = valor.Trim()
                .Replace("€", "")
                .Replace("%", "")
                .Trim();

            if (v.Contains(',') && v.Contains('.'))
                v = v.Replace(".", "").Replace(",", ".");
            else if (v.Contains(','))
                v = v.Replace(",", ".");

            return decimal.TryParse(v,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var r) ? r : 0m;
        }

        protected decimal ExtraerPorcentajeIRPF(string texto)
        {
            var m = RegexIRPF.Match(texto);
            return m.Success ? ParsearDecimal(m.Groups[1].Value) : 0m;
        }

        protected decimal ExtraerPorcentajeRE(string texto)
        {
            var m = RegexRE.Match(texto);
            return m.Success ? ParsearDecimal(m.Groups[1].Value) : 0m;
        }

        // ── Estado ───────────────────────────────────────────────────────────

        protected EstadoFactura DeterminarEstado(Factura f)
        {
            // Campos obligatorios — si falta alguno → RevisiónManual
            bool camposObligatoriosOk =
                !string.IsNullOrEmpty(f.NumeroFactura) &&
                f.Fecha.HasValue &&
                !string.IsNullOrEmpty(f.Emisor.Nombre) &&
                !string.IsNullOrEmpty(f.Emisor.NIF) &&
                !string.IsNullOrEmpty(f.Receptor.Nombre) &&
                !string.IsNullOrEmpty(f.Receptor.NIF);

            if (!camposObligatoriosOk)
                return EstadoFactura.RevisionManual;

            // Verificación del total — si no coincide → RevisiónManual
            if (!f.TotalesCoinciden)
                return EstadoFactura.RevisionManual;

            return EstadoFactura.OK;
        }
    }
}