using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using PdfPigPage = UglyToad.PdfPig.Content.Page;

namespace FacturasApp.Services
{
    public class PdfTextExtractor
    {
        private const int MinCaracteresParaConsiderarSeleccionable = 30;

        // ── Modo de extracción ────────────────────────────────────────────────
        public enum ModoExtraccion
        {
            Simple,       // Rápido pero desordenado
            OrdenadoPosicion, // Ordena por coordenadas Y/X
            LayoutAnalysis    // Usa el analizador de layout de PdfPig (mejor)
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
                    ModoExtraccion.Simple
                        => ExtraerSimple(pagina),
                    ModoExtraccion.OrdenadoPosicion
                        => ExtraerOrdenadoPorPosicion(pagina),
                    ModoExtraccion.LayoutAnalysis
                        => ExtraerConLayoutAnalysis(pagina),
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

        // ── Método 1: Simple (el que teníamos antes) ──────────────────────────

        private string ExtraerSimple(PdfPigPage pagina)
        {
            return pagina.Text;
        }

        // ── Método 2: Ordenado por posición ───────────────────────────────────
        // Agrupa las palabras por línea (coordenada Y similar)
        // y dentro de cada línea las ordena por X (izquierda a derecha)

        private string ExtraerOrdenadoPorPosicion(PdfPigPage pagina)
        {
            // Tolerancia en píxeles para considerar que dos palabras
            // están en la misma línea
            const double toleranciaLinea = 3.0;

            var palabras = pagina.GetWords().ToList();
            if (palabras.Count == 0) return string.Empty;

            // Agrupamos palabras por línea usando la coordenada Y del baseline
            var lineas = palabras
                .GroupBy(p => Math.Round(p.BoundingBox.Bottom / toleranciaLinea)
                              * toleranciaLinea)
                .OrderByDescending(g => g.Key) // Y mayor = arriba en PDF
                .Select(g => string.Join(" ",
                    g.OrderBy(p => p.BoundingBox.Left) // X menor = más a la izquierda
                     .Select(p => p.Text)));

            return string.Join(Environment.NewLine, lineas);
        }

        // ── Método 3: Layout Analysis (el más preciso) ────────────────────────
        // Usa el analizador de bloques de texto de PdfPig que detecta
        // columnas, párrafos y orden de lectura natural

        private string ExtraerConLayoutAnalysis(PdfPigPage pagina)
        {
            try
            {
                // ContentOrderTextExtractor respeta el orden natural de lectura
                return ContentOrderTextExtractor.GetText(pagina,
                    addDoubleNewline: true);
            }
            catch
            {
                // Si el análisis falla caemos al método de posición
                return ExtraerOrdenadoPorPosicion(pagina);
            }
        }
    }
}