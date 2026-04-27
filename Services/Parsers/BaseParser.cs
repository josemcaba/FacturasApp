using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public abstract class BaseParser : IInvoiceParser
    {
        public abstract string Nombre { get; }
        public abstract string Nif { get; }
        public virtual string Concepto => "600"; // Código contable por defecto
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

        protected string ExtraerNif(Regex regex, string texto, string nifEmisor)
        {
            // Matches() devuelve TODAS las coincidencias, no solo la primera
            var coincidencias = regex.Matches(texto);

            foreach (Match m in coincidencias)
            {
                string nif = m.Groups.Count > 1
                    ? m.Groups[1].Value.Trim()  // usamos grupo de captura si existe
                    : m.Value.Trim();           // si no, el match completo
                
                if (string.IsNullOrEmpty(nif)) continue;

                // Eliminamos espacios, guiones y puntos comunes en los NIFs
                nif = nif.Replace(" ", "")
                         .Replace("-", "")
                         .Replace(".", "")
                         .Replace(",", "")
                         .Trim()
                         .ToUpper();

                // Ignoramos el NIF del emisor
                if (nif.Equals(nifEmisor, StringComparison.OrdinalIgnoreCase))
                    continue;

                return nif; // Primer NIF que no es el del emisor
            }

            return string.Empty;
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

        public static readonly Regex RegexFecha = new(
            @"\b(\d{1,2})[\/\.-]((?:\d{1,2}|\D{3}))[\/\.-](\d{2,4})\b",
            RegexOptions.Compiled);

        protected DateTime? ExtraerFecha(Regex Regex, string texto)
        {
            var m = Regex.Match(texto);
            if ((!m.Success) | (m.Captures.Count != 1)) 
                return null;

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
                !string.IsNullOrEmpty(f.Receptor.NIF) &&
                f.Total != 0.0m;

            if (!camposObligatoriosOk)
                return EstadoFactura.RevisionManual;

            // Nombre del cliente (receptor) muy largo — si >40 caracteres → RevisiónManual
            if (f.Receptor.Nombre.Length > 40)
            { 
                f.ErrorMensaje = "Nombre del cliente demasiado largo";
                return EstadoFactura.RevisionManual;
            }

            // Verificación del NIF del emisor y del receptor — si no son válidos → Error
            if (!NifValidator.ValidarNif(f.Emisor.NIF) || !NifValidator.ValidarNif(f.Receptor.NIF))
            {
                f.ErrorMensaje = "NIF no válido";
                return EstadoFactura.Error;
            }

            // Verificación del total — si no coincide → Error
            if (!f.TotalesCoinciden)
                return EstadoFactura.Error;

            return EstadoFactura.OK;
        }
    }
}