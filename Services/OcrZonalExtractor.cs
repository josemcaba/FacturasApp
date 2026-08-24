using FacturasApp.Core.Models;
using Tesseract;

namespace FacturasApp.Services
{
    public class OcrZonalExtractor : OcrBase
    {
        private readonly PdfTextExtractor _pdfTextExtractor = new();

        public OcrZonalExtractor(string tessDataPath = @"./tessdata")
            : base(tessDataPath) { }

        // ── Extracción de todas las zonas de una plantilla ────────────────────

        public Dictionary<string, string> ExtraerZonas(string rutaPdf, PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();

            // 🔍 Verificar si el PDF tiene texto seleccionable
            bool esSeleccionable = _pdfTextExtractor.EsSeleccionable(rutaPdf);
            System.Diagnostics.Debug.WriteLine(
                $"📄 PDF {(esSeleccionable ? "CON" : "SIN")} texto seleccionable");

            if (esSeleccionable)
            {
                // ✅ Usar extracción de texto directo
                return _pdfTextExtractor.ExtraerZonasTexto(rutaPdf, plantilla);
            }

            // ❌ Fallback a OCR — agrupar zonas por página
            using var engine = CrearEngine();
            ConfigurarParametrosTesseract(engine);

            // Precachear páginas necesarias (una sola renderización por página)
            var paginasRequeridas = plantilla.Zonas
                .Select(z => z.NumPagina)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var cachePaginas = new Dictionary<int, Bitmap>();
            foreach (int numPagina in paginasRequeridas)
            {
                int indicePagina = numPagina - 1; // Convertir 1-based a 0-based
                var paginaBitmap = RenderizarPagina(rutaPdf, indicePagina);
                if (paginaBitmap != null)
                    cachePaginas[numPagina] = paginaBitmap;
            }

            try
            {
                foreach (var zona in plantilla.Zonas)
                {
                    try
                    {
                        if (!cachePaginas.TryGetValue(zona.NumPagina, out var paginaBitmap))
                        {
                            resultado[zona.Campo] = string.Empty;
                            continue;
                        }

                        var rect = zona.ToRectangle(paginaBitmap.Width, paginaBitmap.Height);

                        using var zonaImagen = RecortarZona(paginaBitmap, rect);
                        if (zonaImagen == null)
                        {
                            resultado[zona.Campo] = string.Empty;
                            continue;
                        }

                        string textoDirecto = AplicarOcr(engine, zonaImagen).Trim();

                        resultado[zona.Campo] = textoDirecto;
                    }
                    catch
                    {
                        resultado[zona.Campo] = string.Empty;
                    }
                }
            }
            finally
            {
                foreach (var bitmap in cachePaginas.Values)
                    bitmap.Dispose();
            }

            return resultado;
        }

        // ── Extracción de una zona específica ─────────────────────────────────

        /// <summary>
        /// Extrae texto de una zona específica.
        /// Retorna información sobre el método utilizado (Texto directo o OCR).
        /// </summary>
        public ResultadoExtraccionTexto ExtraerTextoZonalConMetadata(string rutaPdf, ZonaOcr zona)
        {
            if (zona == null)
                return new ResultadoExtraccionTexto(string.Empty, ResultadoExtraccionTexto.MetodoExtraccion.Ocr, estaVacia: true);

            int indicePagina = zona.NumPagina - 1; // Convertir 1-based a 0-based

            // 🔍 Verificar si el PDF tiene texto seleccionable
            bool esSeleccionable = _pdfTextExtractor.EsSeleccionable(rutaPdf);

            if (esSeleccionable)
            {
                // ✅ Intenta extraer texto directo primero
                var textoDirecto = _pdfTextExtractor.ExtraerTextoZonal(rutaPdf, zona);
                if (!string.IsNullOrEmpty(textoDirecto))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"✓ Texto extraído directamente: {textoDirecto.Substring(0, Math.Min(50, textoDirecto.Length))}...");
                    return new ResultadoExtraccionTexto(textoDirecto, ResultadoExtraccionTexto.MetodoExtraccion.TextoSeleccionable);
                }
                System.Diagnostics.Debug.WriteLine("⚠ Zona vacía en PDF, intentando OCR...");
            }

            // ❌ Fallback a OCR
            using var engine = CrearEngine();
            using var paginaBitmap = RenderizarPagina(rutaPdf, indicePagina);

            if (paginaBitmap == null)
                return new ResultadoExtraccionTexto(string.Empty, ResultadoExtraccionTexto.MetodoExtraccion.Ocr, estaVacia: true);

            try
            {
                var rect = zona.ToRectangle(
                    paginaBitmap.Width, paginaBitmap.Height);

                using var zonaImagen = RecortarZona(paginaBitmap, rect);
                if (zonaImagen == null)
                    return new ResultadoExtraccionTexto(string.Empty, ResultadoExtraccionTexto.MetodoExtraccion.Ocr, estaVacia: true);

                var textoOcr = AplicarOcr(engine, zonaImagen).Trim();
                System.Diagnostics.Debug.WriteLine(
                    $"🔤 Texto extraído con OCR: {textoOcr.Substring(0, Math.Min(50, textoOcr.Length))}...");

                bool estaVacia = string.IsNullOrEmpty(textoOcr);
                return new ResultadoExtraccionTexto(textoOcr, ResultadoExtraccionTexto.MetodoExtraccion.Ocr, estaVacia);
            }
            catch
            {
                return new ResultadoExtraccionTexto(string.Empty, ResultadoExtraccionTexto.MetodoExtraccion.Ocr, estaVacia: true);
            }
        }

        /// <summary>
        /// Método antiguo mantenido para compatibilidad.
        /// Retorna solo el texto sin información del método.
        /// </summary>
        public string ExtraerTextoZonal(string rutaPdf, ZonaOcr zona)
        {
            var resultado = ExtraerTextoZonalConMetadata(rutaPdf, zona);
            return resultado.Texto;
        }

        // ── Helper de recorte ─────────────────────────────────────────────────

        private Bitmap? RecortarZona(Bitmap imagen, Rectangle rect)
        {
            rect = Rectangle.Intersect(rect,
                new Rectangle(0, 0, imagen.Width, imagen.Height));

            if (rect.Width <= 0 || rect.Height <= 0) return null;

            var zonaImagen = new Bitmap(rect.Width, rect.Height);
            using var g = Graphics.FromImage(zonaImagen);
            g.DrawImage(imagen,
                new Rectangle(0, 0, rect.Width, rect.Height),
                rect, GraphicsUnit.Pixel);

            return zonaImagen;
        }
    }
}