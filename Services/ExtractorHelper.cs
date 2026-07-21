using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FacturasApp.Services
{
    /// <summary>
    /// Métodos estáticos de extracción reutilizables desde BaseParser y FieldBasedExtractor.
    /// </summary>
    public static partial class ExtractorHelper
    {
        // ── Regex genéricas ────────────────────────────────────────────────────

        [GeneratedRegex(@"\b(\d{1,4}\s*[\/\.-]\s*(?:\d{1,2}|\D{3})\s*[\/\.-]\s*\d{1,4})\b",
            RegexOptions.Compiled)]
        public static partial Regex RegexFechaGenerica();

        [GeneratedRegex(@"\b(?:ES|)((?:(?:[A-Z]|\d)\d{7}(?:-|)[A-Z]|[A-Z](?:-|)\d{8}))\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        public static partial Regex RegexNifGenerica();

        [GeneratedRegex(@"\b(\d{1,4})\s*[\/\.-]\s*((?:\d{1,2}|\D{3}))\s*[\/\.-]\s*(\d{1,4})\b",
            RegexOptions.Compiled)]
        public static partial Regex RegexFechaFormateada();

        // ── Extracción de grupo ───────────────────────────────────────────────

        public static string ExtraerGrupo(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            return m.Success && m.Groups.Count > grupo
                ? m.Groups[grupo].Value.Trim()
                : string.Empty;
        }

        public static string ExtraerGrupo(string patronRegex, string texto, int grupo,
            RegexOptions options = RegexOptions.IgnoreCase)
        {
            var m = Regex.Match(texto, patronRegex, options);
            return m.Success && m.Groups.Count > grupo
                ? m.Groups[grupo].Value.Trim()
                : string.Empty;
        }

        // ── Extracción de decimal ─────────────────────────────────────────────

        public static decimal ExtraerDecimal(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            if (!m.Success) return 0m;
            return ParsearDecimal(m.Groups[grupo].Value);
        }

        public static decimal ExtraerDecimal(string patronRegex, string texto, int grupo,
            RegexOptions options = RegexOptions.IgnoreCase)
        {
            var m = Regex.Match(texto, patronRegex, options);
            if (!m.Success) return 0m;
            return ParsearDecimal(m.Groups[grupo].Value);
        }

        public static decimal ParsearDecimal(string valor)
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

        // ── Extracción de NIF ─────────────────────────────────────────────────

        public static string ExtraerNif(string texto, string nifEmisor,
            string? patronRegex = null)
        {
            Regex regex = patronRegex != null
                ? new Regex(patronRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled)
                : RegexNifGenerica();

            var coincidencias = regex.Matches(texto);

            foreach (Match m in coincidencias)
            {
                string nif = m.Groups.Count > 1
                    ? m.Groups[1].Value.Trim()
                    : m.Value.Trim();

                if (string.IsNullOrEmpty(nif)) continue;

                nif = nif.Replace(" ", "").Replace("-", "")
                         .Replace(".", "").Replace(",", "")
                         .Trim().ToUpper();

                if (nif.Length > 9)
                    nif = nif.Substring(0, 9);

                if (nif.Equals(nifEmisor, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (NifValidator.ValidarNif(nif))
                    return nif;
            }

            return string.Empty;
        }

        // ── Extracción de fecha ───────────────────────────────────────────────

        public static DateTime? ExtraerFecha(string texto, string? patronRegex = null,
            List<string>? formatosFecha = null, string cultura = "es-ES")
        {
            Regex regex = patronRegex != null
                ? new Regex(patronRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled)
                : RegexFechaGenerica();

            var matches = regex.Matches(texto);

            if (matches.Count == 0) return null;

            if (matches.Count > 1 && !matches.All(match => match.Value == matches[0].Value))
                return null; // Ambigüedad

            string valorFecha = matches[0].Value;

            // Intentar parsear con formatos personalizados primero
            if (formatosFecha != null && formatosFecha.Count > 0)
            {
                var cultureInfo = new CultureInfo(cultura);
                foreach (var formato in formatosFecha)
                {
                    if (DateTime.TryParseExact(valorFecha.Trim(), formato,
                        cultureInfo, DateTimeStyles.None, out var fecha))
                        return fecha;
                }
            }

            // Intentar con el formato genérico dd/MM/yyyy
            var m = RegexFechaFormateada().Matches(valorFecha);
            if (m.Count != 1) return null;

            string g1 = m[0].Groups[1].Value;
            string g2 = m[0].Groups[2].Value;
            string g3 = m[0].Groups[3].Value;

            string fechaParseo = g3.Length == 4
                ? $"{g3}/{g2}/{g1}"
                : $"{g1}/{g2}/{g3}";

            return DateTime.TryParse(fechaParseo,
                new CultureInfo(cultura),
                DateTimeStyles.None, out var f) ? f : null;
        }

        // ── Limpieza OCR ──────────────────────────────────────────────────────

        public static string EliminarDuplicadosNoNumericos(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            var resultado = new StringBuilder();
            char? ultimoCaracter = null;

            foreach (char c in texto)
            {
                bool esNumero = c >= '0' && c <= '9';
                bool esEspacio = c == ' ' || c == '\t' || c == '\n' || c == '\r';

                if (esNumero || esEspacio)
                {
                    // Números y espacios/saltos de línea se mantienen siempre
                    resultado.Append(c);
                    ultimoCaracter = null; // Reset para no colapsar con caracteres anteriores
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
            if (string.IsNullOrEmpty(texto)) return texto;

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
    }
}
