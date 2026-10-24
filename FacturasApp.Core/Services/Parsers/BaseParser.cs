using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FacturasApp.Core.Models;
using System.Globalization;

namespace FacturasApp.Core.Services.Parsers
{
    public abstract class BaseParser : IInvoiceParser
    {
        public abstract string Nombre { get; }
        public abstract string Nif { get; }
        public abstract Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        public virtual ModoExtraccion ModoExtraccion =>
            ModoExtraccion.Ordenado;

        // Implementación base: devuelve lista con una sola factura
        // ConfigurableParserEngine la sobreescribe cuando la config declara <MultiLinea>
        public virtual List<Factura> ParsearMultiple(
            string texto, string rutaArchivo, bool viaOcr) =>
                [Parsear(texto, rutaArchivo, viaOcr)];

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
            @"\b(?:ES|)\s*((?:[A-Z][ -]{0,3}|)\d{1,2}(?:\.|)\d{3}(?:\.|)\d{3}(?:[ -]{0,3}[A-Z]|))\b",
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

        /// <summary>
        /// Extrae una fecha del texto usando la cultura indicada (por defecto es-ES).
        /// Si el patrón numérico no encuentra nada, se intenta fechas con nombre de mes
        /// generadas desde la propia cultura: "01 Ago 2026" / "1 de agosto de 2026"
        /// en es-ES, "01 Aug 2026" / "August 1, 2026" en en-US.
        /// La ambigüedad se resuelve sobre la FECHA resuelta, no sobre el texto: formas
        /// distintas de expresar el mismo día ("1/8/2026" y "01 Ago 2026") SÍ se aceptan;
        /// sólo se devuelve null si alguna no parsea o si hay 2+ días distintos
        /// (un teléfono "951.91.63.89" sigue rompiéndola → regex de fecha explícita).
        /// </summary>
        protected DateTime? ExtraerFecha(string texto, string? cultura = null)
        {
            return ExtraerFecha(RegexFecha, texto, cultura);
        }

        protected static DateTime? ExtraerFecha(Regex RegexF, string texto, string? cultura = null)
        {
            var cultureInfo = ObtenerCulturaFecha(cultura);

            var m = RegexF.Matches(texto);
            if (m.Count == 0)
                return ExtraerFechaConNombreMes(texto, cultureInfo);

            // Aplanamos todas las fechas candidatas: puede haber varias dentro de un
            // mismo match (p. ej. un regex externo que captura la línea entera).
            var candidatos = m
                .SelectMany(match => RegexFechaFormateada.Matches(match.Value))
                .ToList();

            if (candidatos.Count == 0)
                return null;

            return UnificarFechas(candidatos, match => ConstruirFechaNumerica(match, cultureInfo));
        }

        private static readonly Regex RegexFechaFormateada = new(
            @"\b(\d{1,4})\s*[\/\.-]\s*((?:\d{1,2}|\D{3}))\s*[\/\.-]\s*(\d{1,4})\b",
            RegexOptions.Compiled);

        /// <summary>
        /// Convierte un match de <see cref="RegexFechaFormateada"/> en DateTime.
        /// Si el año va de 4 dígitos al final se normaliza a aaaa/MM/dd; en cualquier
        /// otro caso se lee siempre como dd/MM/aaaa.
        /// </summary>
        private static DateTime? ConstruirFechaNumerica(Match m, CultureInfo cultura)
        {
            string g1 = m.Groups[1].Value;
            string g2 = m.Groups[2].Value;
            string g3 = m.Groups[3].Value;

            string fechaParseo = g3.Length == 4
                ? $"{g3}/{g2}/{g1}"
                : $"{g1}/{g2}/{g3}";

            return DateTime.TryParse(
                fechaParseo,
                cultura,
                DateTimeStyles.None, out var f) ? f : null;
        }

        /// <summary>
        /// Regla única de ambigüedad (numéricas y nombre de mes): parsea cada candidato
        /// y devuelve la fecha sólo si TODAS resuelven al mismo día. Si alguna no parsea
        /// o hay 2+ días distintos, devuelve null.
        /// </summary>
        private static DateTime? UnificarFechas(IEnumerable<Match> candidatos, Func<Match, DateTime?> parsear)
        {
            DateTime? fecha = null;

            foreach (var candidato in candidatos)
            {
                var f = parsear(candidato);
                if (f == null) return null;                       // alguna no parsea
                if (fecha == null) fecha = f;
                else if (fecha.Value != f.Value) return null;     // días distintos
            }

            return fecha;
        }

        // ── Fechas con nombre de mes (según la cultura del emisor) ───────────

        private static readonly ConcurrentDictionary<string, Regex> _regexesFechaMes = new();

        /// <summary>
        /// Cultura a usar para fechas: la indicada, o es-ES si está vacía o no existe.
        /// </summary>
        protected static CultureInfo ObtenerCulturaFecha(string? cultura)
        {
            if (string.IsNullOrWhiteSpace(cultura))
                return CultureInfo.GetCultureInfo("es-ES");

            try { return new CultureInfo(cultura); }
            catch (CultureNotFoundException) { return CultureInfo.GetCultureInfo("es-ES"); }
        }

        /// <summary>
        /// Regex para "01 Ago 2026" / "1 de agosto de 2026" (es-ES) y
        /// "01 Aug 2026" / "August 1, 2026" (en-US). Los nombres de mes se toman de la
        /// propia cultura (<see cref="DateTimeFormatInfo"/>), así que vale para
        /// cualquier cultura instalada; se cachea por nombre de cultura.
        /// El mes admite una extensión corta opcional de hasta 2 letras para cubrir
        /// "Sept" (en-US sólo conoce "Sep"), validada después en <see cref="MesANumero"/>.
        /// </summary>
        private static Regex RegexFechaConNombreMes(string nombreCultura) =>
            _regexesFechaMes.GetOrAdd(nombreCultura, nombre =>
            {
                var patronMes = string.Join("|",
                    MesesDeCultura(nombre).Select(Regex.Escape));
                return new Regex(
                    $@"\b(?:(?<dia>\d{{1,2}})\s+(?:de\s+)?(?<mes>{patronMes})[a-zA-Z]{{0,2}}?\.?(?:\s+de)?\s+(?<anio>\d{{4}})" +
                    $@"|(?<mes>{patronMes})[a-zA-Z]{{0,2}}?\.?\s+(?<dia>\d{{1,2}})(?:º|ª|st|nd|rd|th)?,?\s+(?<anio>\d{{4}}))\b",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            });

        private static List<string> MesesDeCultura(string nombreCultura)
        {
            var dtf = ObtenerCulturaFecha(nombreCultura).DateTimeFormat;
            return dtf.MonthNames
                .Concat(dtf.AbbreviatedMonthNames)
                .Where(mes => !string.IsNullOrWhiteSpace(mes))
                .Select(mes => mes.Trim().TrimEnd('.').Trim())
                .Where(mes => mes.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(mes => mes.Length) // "septiembre" antes que "sept"
                .ToList();
        }

        /// <summary>
        /// Busca fechas con nombre de mes en el texto. Misma regla de ambigüedad que
        /// con las numéricas (ver <see cref="UnificarFechas"/>): formas distintas del
        /// mismo día ("01 Ago 2026" y "1 de agosto de 2026") se aceptan; si alguna no
        /// parsea o hay 2+ días distintos, devuelve null.
        /// </summary>
        private static DateTime? ExtraerFechaConNombreMes(string texto, CultureInfo cultura)
        {
            var matches = RegexFechaConNombreMes(cultura.Name).Matches(texto);
            if (matches.Count == 0)
                return null;

            return UnificarFechas(matches, m => CrearFechaDesdeMes(m, cultura));
        }

        /// <summary>
        /// Convierte un match "dd MMM yyyy" o "MMM dd, yyyy" en DateTime, validando el
        /// día contra el mes/año. El nombre de mes se resuelve contra la cultura
        /// tolerando mayúsculas, puntos y extensiones cortas ("Sept" ≡ "Sep",
        /// que TryParse de en-US rechazaría).
        /// </summary>
        protected static DateTime? CrearFechaDesdeMes(Match m, CultureInfo cultura)
        {
            if (!int.TryParse(m.Groups["dia"].Value, out int dia) ||
                !int.TryParse(m.Groups["anio"].Value, out int anio) ||
                anio < 1)
                return null;

            int? mes = MesANumero(m.Groups["mes"].Value, cultura);
            if (mes == null)
                return null;

            if (dia < 1 || dia > DateTime.DaysInMonth(anio, mes.Value))
                return null;

            return new DateTime(anio, mes.Value, dia);
        }

        private static int? MesANumero(string mesCapturado, CultureInfo cultura)
        {
            var capturado = mesCapturado.Trim().TrimEnd('.').Trim();
            if (capturado.Length < 3) return null;

            var dtf = cultura.DateTimeFormat;
            for (int i = 0; i < 12; i++)
            {
                if (CoincideMes(capturado, dtf.MonthNames[i]) ||
                    CoincideMes(capturado, dtf.AbbreviatedMonthNames[i]))
                    return i + 1;
            }

            return null;
        }

        private static bool CoincideMes(string capturado, string? mesCultura)
        {
            if (string.IsNullOrWhiteSpace(mesCultura)) return false;

            var limpio = mesCultura.Trim().TrimEnd('.').Trim();
            if (capturado.Equals(limpio, StringComparison.OrdinalIgnoreCase))
                return true;

            // Extensiones cortas del mismo mes: "Sept" ≡ "Sep" (en-US), "Jul" ≡ "July"
            return limpio.Length >= 3 &&
                capturado.Length > limpio.Length &&
                capturado.Length <= limpio.Length + 2 &&
                capturado.StartsWith(limpio, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Intenta interpretar <paramref name="valor"/> como fecha a partir del nombre
        /// de mes de la cultura (p. ej. "01 Sept 2026" que TryParse de en-US no acepta).
        /// Devuelve null si no es una fecha válida.
        /// </summary>
        protected static DateTime? ParsearFechaConNombreMes(string valor, CultureInfo cultura)
        {
            var m = RegexFechaConNombreMes(cultura.Name).Match(valor);
            return m.Success ? CrearFechaDesdeMes(m, cultura) : null;
        }
    }
}