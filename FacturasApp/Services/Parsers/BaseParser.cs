using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public abstract class BaseParser : IInvoiceParser
    {
        public abstract string Nombre { get; }
        public abstract string Nif { get; }
        public virtual string Concepto => "0"; // Código contable, por defecto 0 (sin asignar)
        public abstract bool PuedeParsar(string texto);
        public abstract Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        public virtual PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

        // Implementación base: devuelve lista con una sola factura
        // MercadonaParser (y cualquier otro que lo necesite) lo sobreescribe
        public virtual List<Factura> ParsearMultiple(
            string texto, string rutaArchivo, bool viaOcr) =>
            new() { Parsear(texto, rutaArchivo, viaOcr) };

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

        protected DateTime? ExtraerFecha(Regex Regex, string texto)
        {
            var m = Regex.Match(texto);
            if (!m.Success) return null;
            return DateTime.TryParse(
                $"{m.Groups[1].Value}/{m.Groups[2].Value}/{m.Groups[3].Value}",
                new System.Globalization.CultureInfo("es-ES"),
                System.Globalization.DateTimeStyles.None, out var f) ? f : null;
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
                return EstadoFactura.Error;

            return EstadoFactura.OK;
        }
    }
}