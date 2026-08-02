using System.Text;
using FacturasApp.Models;
using PdfiumViewer;

namespace FacturasApp.Services
{
    public class PdfTextExtractor
    {
        private const int MinCaracteresParaConsiderarSeleccionable = 30;

        public enum ModoExtraccion
        {
            Simple,
            OrdenadoPosicion,
            LayoutAnalysis
        }

        public string? ExtraerTextoSeleccionable(string rutaPdf,
            ModoExtraccion modo = ModoExtraccion.LayoutAnalysis)
        {
            var textoTotal = new System.Text.StringBuilder();

            using var documento = PdfDocument.Load(rutaPdf);

            for (int i = 0; i < documento.PageCount; i++)
            {
                string textoPagina = modo switch
                {
                    ModoExtraccion.Simple => ExtraerSimple(documento, i),
                    ModoExtraccion.OrdenadoPosicion => ExtraerOrdenadoPorPosicion(documento, i),
                    ModoExtraccion.LayoutAnalysis => ExtraerSimple(documento, i),
                    _ => ExtraerOrdenadoPorPosicion(documento, i)
                };

                textoTotal.AppendLine(textoPagina);
            }

            string resultado = textoTotal.ToString().Trim();

            return resultado.Length >= MinCaracteresParaConsiderarSeleccionable
                ? resultado
                : null;
        }

        public bool EsSeleccionable(string rutaPdf) =>
            ExtraerTextoSeleccionable(rutaPdf) != null;

        private static string ExtraerSimple(PdfDocument documento, int numPagina)
        {
            return documento.GetPdfText(numPagina) ?? string.Empty;
        }

        private static string ExtraerOrdenadoPorPosicion(PdfDocument documento, int numPagina)
        {
            const double toleranciaLinea = 4.0;

            var chars = documento.GetCharacterInformation(numPagina)
                .Where(c => !char.IsControl(c.Character))
                .ToList();
            if (chars.Count == 0) return string.Empty;

            // PDFium usa coordenadas PDF nativas: Y desde la parte INFERIOR (0 = abajo).
            // El bottom del glifo (Y + Height, con Height negativo) se alinea con la
            // baseline. Los chars de una misma línea tienen bottoms que pueden variar
            // unos puntos, así que se agrupan por anclas encadenadas (cada ancla agrupa
            // los bottoms que distan <= tolerancia del anterior, sin límites de bin).
            var anclas = new List<double>();
            foreach (var bottom in chars
                         .Select(c => c.Bounds.Y + c.Bounds.Height)
                         .OrderByDescending(b => b))
            {
                if (anclas.Count == 0 || anclas[^1] - bottom > toleranciaLinea)
                    anclas.Add(bottom);
            }

            var lineas = chars
                .GroupBy(c => anclas
                    .OrderBy(a => Math.Abs((c.Bounds.Y + c.Bounds.Height) - a))
                    .First())
                .OrderByDescending(g => g.Key)  // De arriba a abajo
                .Select(g => string.Concat(
                    g.OrderBy(c => c.Bounds.X)
                     .Select(c => c.Character)))
                .Select(l => l.Trim())
                .Where(l => l.Length > 0);

            return string.Join(Environment.NewLine, lineas);
        }

        // ── NUEVA ESTRATEGIA: Extraer texto por zonas manteniendo el formato ───

        public Dictionary<string, string> ExtraerZonasTexto(
            string rutaPdf,
            PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();

            using var documento = PdfDocument.Load(rutaPdf);

            // Precachear páginas necesarias por número de página (1-based)
            var paginasRequeridas = plantilla.Zonas
                .Select(z => z.NumPagina)
                .Distinct()
                .ToList();

            foreach (var zona in plantilla.Zonas)
            {
                try
                {
                    if (zona.NumPagina < 1 || zona.NumPagina > documento.PageCount)
                    {
                        resultado[zona.Campo] = string.Empty;
                        continue;
                    }

                    int indicePagina = zona.NumPagina - 1;
                    var tamanio = documento.PageSizes[indicePagina];

                    var rect = ConvertirZonaAPdfRectangle(zona, tamanio.Width, tamanio.Height);

                    string textoDirecto = ExtraerTextoLayoutDesdeArea(documento, indicePagina, rect);

                    resultado[zona.Campo] = textoDirecto;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error extrayendo zona {zona.Campo}: {ex.Message}");
                    resultado[zona.Campo] = string.Empty;
                }
            }

            return resultado;
        }

        /// <summary>
        /// Extrae texto de una zona específica del PDF si el PDF tiene texto seleccionable.
        /// Retorna null si no se puede extraer texto (PDF sin texto seleccionable).
        /// </summary>
        public string? ExtraerTextoZonal(string rutaPdf, ZonaOcr zona)
        {
            if (zona == null) return null;

            try
            {
                using var documento = PdfDocument.Load(rutaPdf);
                if (zona.NumPagina < 1 || zona.NumPagina > documento.PageCount)
                    return null;

                int indicePagina = zona.NumPagina - 1;
                var tamanio = documento.PageSizes[indicePagina];

                var rect = ConvertirZonaAPdfRectangle(zona, tamanio.Width, tamanio.Height);
                return ExtraerTextoLayoutDesdeArea(documento, indicePagina, rect);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extrayendo zona {zona.Campo}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extrae texto de un área específica respetando el layout original.
        /// Filtra los caracteres por su posición y los agrupa por líneas.
        /// PDFium usa coordenadas PDF nativas: Y desde la parte INFERIOR (0 = abajo).
        /// </summary>
        private static string ExtraerTextoLayoutDesdeArea(PdfDocument documento, int indicePagina,
            RectangleF rect)
        {
            var chars = documento.GetCharacterInformation(indicePagina);

            // Filtrar caracteres completamente dentro del área (coordenadas bottom-based:
            // rect.Top = borde inferior de la zona, rect.Bottom = borde superior)
            var charsEnArea = chars
                .Where(c => !char.IsControl(c.Character))
                .Where(c => c.Bounds.X >= rect.Left
                    && (c.Bounds.X + c.Bounds.Width) <= rect.Right
                    && c.Bounds.Y >= rect.Top
                    && (c.Bounds.Y + c.Bounds.Height) <= rect.Bottom)
                .ToList();

            if (charsEnArea.Count == 0)
                return string.Empty;

            // Misma agrupación por anclas encadenadas que en OrdenadoPosicion:
            // los bottoms de una misma línea varían unos puntos y los bins fijos
            // podían partir líneas.
            const double toleranciaLinea = 4.0;

            var anclas = new List<double>();
            foreach (var bottom in charsEnArea
                         .Select(c => c.Bounds.Y + c.Bounds.Height)
                         .OrderByDescending(b => b))
            {
                if (anclas.Count == 0 || anclas[^1] - bottom > toleranciaLinea)
                    anclas.Add(bottom);
            }

            var lineas = charsEnArea
                .GroupBy(c => anclas
                    .OrderBy(a => Math.Abs((c.Bounds.Y + c.Bounds.Height) - a))
                    .First())
                .OrderByDescending(g => g.Key)  // De arriba a abajo
                .Select(g => string.Concat(
                    g.OrderBy(c => c.Bounds.X)
                     .Select(c => c.Character)))
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            return string.Join(Environment.NewLine, lineas);
        }

        // ── Conversión de ZonaOcr a PdfRectangle ────────────────

        private RectangleF ConvertirZonaAPdfRectangle(ZonaOcr zona, float paginaWidth, float paginaHeight)
        {
            // Coordenadas en porcentaje (0-100) a puntos PDF.
            // PDFium usa Y desde la parte INFERIOR (0 = abajo).
            // Nuestras coordenadas Y son desde la parte SUPERIOR (0 = arriba).
            float izquierda = (float)(zona.X / 100.0) * paginaWidth;
            float ancho = (float)(zona.Ancho / 100.0) * paginaWidth;

            float topDesdeArriba = (float)(zona.Y / 100.0) * paginaHeight;    // Borde superior (desde arriba)
            float altoZona = (float)(zona.Alto / 100.0) * paginaHeight;       // Altura positiva

            // Y PDF desde abajo, con altura SIEMPRE positiva: el constructor de
            // RectangleF (desde .NET 8) normaliza alturas negativas y deja la recta
            // invertida, lo que descartaba todos los caracteres del filtro.
            float yPdf = paginaHeight - topDesdeArriba - altoZona;

            return new RectangleF(izquierda, yPdf, ancho, altoZona);
        }
    }
}
