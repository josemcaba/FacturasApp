using FacturasApp.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Geometry;
using UglyToad.PdfPig.Util;
using PdfPigPage = UglyToad.PdfPig.Content.Page;

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

            using var documento = PdfDocument.Open(rutaPdf);

            foreach (PdfPigPage pagina in documento.GetPages())
            {
                string textoPagina = modo switch
                {
                    ModoExtraccion.Simple => ExtraerSimple(pagina),
                    ModoExtraccion.OrdenadoPosicion => ExtraerOrdenadoPorPosicion(pagina),
                    ModoExtraccion.LayoutAnalysis => ExtraerConLayoutAnalysis(pagina),
                    _ => ExtraerConLayoutAnalysis(pagina)
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

        private string ExtraerSimple(PdfPigPage pagina)
        {
            return pagina.Text;
        }

        private string ExtraerOrdenadoPorPosicion(PdfPigPage pagina)
        {
            const double toleranciaLinea = 3.0;

            var palabras = pagina.GetWords().ToList();
            if (palabras.Count == 0) return string.Empty;

            var lineas = palabras
                .GroupBy(p => Math.Round(p.BoundingBox.Bottom / toleranciaLinea)
                              * toleranciaLinea)
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ",
                    g.OrderBy(p => p.BoundingBox.Left)
                     .Select(p => p.Text)));

            return string.Join(Environment.NewLine, lineas);
        }

        private string ExtraerConLayoutAnalysis(PdfPigPage pagina)
        {
            try
            {
                return ContentOrderTextExtractor.GetText(pagina,
                    addDoubleNewline: true);
            }
            catch
            {
                return ExtraerOrdenadoPorPosicion(pagina);
            }
        }

        // ── NUEVA ESTRATEGIA: Extraer texto por zonas manteniendo el formato ───

        public Dictionary<string, string> ExtraerZonasTexto(
            string rutaPdf,
            PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();

            using var documento = PdfDocument.Open(rutaPdf);
            var pagina = documento.GetPages().First();

            double paginaWidth = pagina.Width;
            double paginaHeight = pagina.Height;

            string? textoCompleto = null;
            bool necesitaRespaldo = plantilla.Zonas.Any(z => !string.IsNullOrEmpty(z.RegexRespaldo));

            foreach (var zona in plantilla.Zonas)
            {
                try
                {
                    var rect = ConvertirZonaAPdfRectangle(zona, paginaWidth, paginaHeight);

                    // Extraer texto de la zona respetando el layout original
                    string textoDirecto = ExtraerTextoLayoutDesdeArea(pagina, rect);

                    if (string.IsNullOrEmpty(textoDirecto) && !string.IsNullOrEmpty(zona.RegexRespaldo))
                    {
                        if (necesitaRespaldo && textoCompleto == null)
                        {
                            textoCompleto = ContentOrderTextExtractor.GetText(pagina);
                        }
                        textoDirecto = zona.ExtraerConRespaldo(textoDirecto, textoCompleto);
                    }

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
        /// Extrae texto de un área específica respetando el layout original.
        /// Filtra las palabras por su posición y luego aplica el extractor de layout.
        /// </summary>
        private string ExtraerTextoLayoutDesdeArea(PdfPigPage pagina, PdfRectangle area)
        {
            // Filtrar palabras que están dentro del área
            var palabrasEnArea = pagina.GetWords()
                .Where(w => area.Contains(w.BoundingBox))
                .ToList();

            if (!palabrasEnArea.Any())
                return string.Empty;

            const double toleranciaLinea = 5.0; // puntos para considerar misma línea

            var lineas = palabrasEnArea
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom / toleranciaLinea))
                .OrderByDescending(g => g.Key)  // De arriba a abajo
                .Select(g => string.Join(" ",
                    g.OrderBy(w => w.BoundingBox.Left)  // De izquierda a derecha
                     .Select(w => w.Text)))
                .ToList();

            return string.Join(Environment.NewLine, lineas);
        }

        // ── Conversión de ZonaOcr a PdfRectangle ────────────────

        private PdfRectangle ConvertirZonaAPdfRectangle(ZonaOcr zona, double paginaWidth, double paginaHeight)
        {
            // Coordenadas en porcentaje (0-100) a puntos PDF
            double izquierda = (zona.X / 100.0) * paginaWidth;
            double ancho = (zona.Ancho / 100.0) * paginaWidth;
            double derecha = izquierda + ancho;

            // PdfPig usa Y desde la parte INFERIOR de la página (0 = abajo)
            // Nuestras coordenadas Y son desde la parte SUPERIOR (0 = arriba)
            double topPorcentaje = zona.Y;                      // Desde arriba
            double bottomPorcentaje = zona.Y + zona.Alto;       // Desde arriba

            double bottom = paginaHeight - (bottomPorcentaje / 100.0) * paginaHeight;
            double top = paginaHeight - (topPorcentaje / 100.0) * paginaHeight;

            return new PdfRectangle(izquierda, bottom, derecha, top);
        }
    }
}