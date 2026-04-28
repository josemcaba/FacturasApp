using FacturasApp.Models;
using Tesseract;

namespace FacturasApp.Services
{
    public class OcrZonalExtractor : OcrBase
    {
        public OcrZonalExtractor(string tessDataPath = @"./tessdata")
            : base(tessDataPath) { }

        // ── Extracción de todas las zonas de una plantilla ────────────────────

        public Dictionary<string, string> ExtraerZonas(
            string rutaPdf, PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();

            using var engine = CrearEngine();
            using var paginaBitmap = RenderizarPagina(rutaPdf, 0);

            ConfigurarParametrosTesseract(engine);

            if (paginaBitmap == null) return resultado;

            foreach (var zona in plantilla.Zonas)
            {
                try
                {
                    var rect = zona.ToRectangle(
                        paginaBitmap.Width, paginaBitmap.Height);

                    using var zonaImagen = RecortarZona(paginaBitmap, rect);
                    if (zonaImagen == null) continue;

                    string texto = AplicarOcr(engine, zonaImagen);
                    resultado[zona.Campo] = texto.Trim();
                }
                catch
                {
                    resultado[zona.Campo] = string.Empty;
                }
            }

            return resultado;
        }

        // ── Extracción de una zona específica ─────────────────────────────────

        public string ExtraerTextoZonal(string rutaPdf, ZonaOcr zona)
        {
            if (zona == null) return string.Empty;

            using var engine = CrearEngine();
            using var paginaBitmap = RenderizarPagina(rutaPdf, 0);

            if (paginaBitmap == null) return string.Empty;

            try
            {
                var rect = zona.ToRectangle(
                    paginaBitmap.Width, paginaBitmap.Height);

                using var zonaImagen = RecortarZona(paginaBitmap, rect);
                if (zonaImagen == null) return string.Empty;

                return AplicarOcr(engine, zonaImagen).Trim();
            }
            catch
            {
                return string.Empty;
            }
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