using System.Text;
using System.Text.RegularExpressions;
using FacturasApp.Models;
using System.Globalization;

namespace FacturasApp.Services.Parsers
{
    public abstract class BaseParser : IInvoiceParser
    {
        public abstract string Nombre { get; }
        public abstract string Nif { get; }
        public virtual string Concepto => "600"; // Código contable por defecto
        public abstract Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        public virtual PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;

        // Implementación base: devuelve lista con una sola factura
        // MercadonaParser (y cualquier otro que lo necesite) lo sobreescribe
        public virtual List<Factura> ParsearMultiple(
            string texto, string rutaArchivo, bool viaOcr) =>
                [Parsear(texto, rutaArchivo, viaOcr)];

        // ── PuedeParsar: template method ──────────────────────────────────

        protected virtual string[] Identificadores => [];

        public virtual bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        // ── Expresiones regulares genéricas (pueden ser sobrescritas) ────────

        /// <summary>
        /// Expresión regular genérica para extraer fechas.
        /// Puede ser sobrescrita si se necesita un patrón específico.
        /// </summary>
        protected virtual Regex RegexFecha { get; } = new(
            @"\b(\d{1,4}\s*[\/\.-]\s*(?:\d{1,2}|\D{3})\s*[\/\.-]\s*\d{1,4})\b",
            RegexOptions.Compiled);

        /// <summary>
        /// Expresión regular genérica para extraer NIFs.
        /// Puede ser sobrescrita si se necesita un patrón específico.
        /// </summary>
        protected virtual Regex RegexNif { get; } = new(
            @"\b(?:ES|)((?:(?:[A-Z]|\d)\d{7}(?:-|)[A-Z]|[A-Z](?:-|)\d{8}))\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // ── Helpers de extracción ────────────────────────────────────────────

        protected Factura CrearFacturaBase(string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };
            factura.Emisor.NIF = Nif;
            factura.Emisor.Nombre = Nombre;
            return factura;
        }

        public static string EliminarDuplicadosNoNumericos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var resultado = new StringBuilder();
            char? ultimoCaracter = null;

            foreach (char c in texto)
            {
                bool esNumero = c >= '0' && c <= '9';

                if (esNumero)
                {
                    resultado.Append(c);
                    ultimoCaracter = c;
                }
                else
                {
                    if (!ultimoCaracter.HasValue || c != ultimoCaracter.Value)
                    {
                        resultado.Append(c);
                        ultimoCaracter = c;
                    }
                }
            }

            return resultado.ToString();
        }

        public static string EliminarDuplicadosNumericos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var resultado = new StringBuilder();
            char? ultimoCaracter = null;

            foreach (char c in texto)
            {
                bool esNumero = c >= '0' && c <= '9';

                if (!esNumero)
                {
                    resultado.Append(c);
                    ultimoCaracter = c;
                }
                else
                {
                    if (!ultimoCaracter.HasValue || c != ultimoCaracter.Value)
                    {
                        resultado.Append(c);
                        ultimoCaracter = c;
                    }
                }
            }

            return resultado.ToString();
        }

        protected static string ExtraerGrupo(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            return m.Success ? m.Groups[grupo].Value.Trim() : string.Empty;
        }

        protected string ExtraerNif(string texto)
        {
            return ExtraerNif(RegexNif, texto, Nif);
        }

        protected static string ExtraerNif(Regex regex, string texto, string nifEmisor)
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

                // Tomamos solo los primeros 9 caracteres, que es la longitud estándar de un NIF
                if (nif.Length > 9)
                    nif = nif.Substring(0, 9);

                // Ignoramos el NIF del emisor
                if (nif.Equals(nifEmisor, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Comprobamos si el NIF es válido usando la clase NifValidator
                if (NifValidator.ValidarNif(nif))
                    return nif; // Primer NIF válido que no es el del emisor
            }

            return string.Empty;
        }

        protected static decimal ExtraerDecimal(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            if (!m.Success) return 0m;
            return ParsearDecimal(m.Groups[grupo].Value);
        }

        protected static decimal ParsearDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            string v = valor.Trim()
                .Replace("€", "")
                .Replace("%", "")
                .Replace(" ", "")
                .Trim();

            if (v.Contains(',') && v.Contains('.'))
                v = v.Replace(".", "").Replace(",", ".");
            else if (v.Contains(','))
                v = v.Replace(",", ".");

            return decimal.TryParse(v,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var r) ? r : 0m;
        }

        protected DateTime? ExtraerFecha(string texto)
        {
            return ExtraerFecha(RegexFecha, texto);
        }

        protected static DateTime? ExtraerFecha(Regex RegexF, string texto)
        {
            var m = RegexF.Matches(texto);
            if (m.Count != 1)
            {
                // Comprobamos si todas las coincidencias encontradas son iguales.
                // Si es así, continuamos. Si no, devolvemos null por ambigüedad.
                if (m.Count > 1 && !m.All(match => match.Value == m[0].Value))
                    return null;
            }

            Regex RegexFechaFormateada = new(
                @"\b(\d{1,4})\s*[\/\.-]\s*((?:\d{1,2}|\D{3}))\s*[\/\.-]\s*(\d{1,4})\b",
                RegexOptions.Compiled);

            m = RegexFechaFormateada.Matches(m[0].Value);
            if (m.Count != 1)
                return null;

            string g1 = m[0].Groups[1].Value;
            string g2 = m[0].Groups[2].Value;
            string g3 = m[0].Groups[3].Value;

            string fechaParseo = g3.Length == 4
                ? $"{g3}/{g2}/{g1}"
                : $"{g1}/{g2}/{g3}";

            return DateTime.TryParse(
                fechaParseo,
                new CultureInfo("es-ES"),
                DateTimeStyles.None, out var f) ? f : null;
        }
    }
}