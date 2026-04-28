using FacturasApp.Models;
using PDFtoImage;
using SkiaSharp;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using Tesseract;

namespace FacturasApp.Services
{
    public class OcrZonalExtractor
    {
        private readonly string _tessDataPath;
        private const string Idiomas = "spa";
        private const int DpiRender = 300;

        public OcrZonalExtractor(string tessDataPath = @"./tessdata")
        {
            _tessDataPath = tessDataPath;
        }

        // ── Extracción zonal principal ────────────────────────────────────────

        public Dictionary<string, string> ExtraerZonas(
            string rutaPdf, PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();

            using var engine = new TesseractEngine(
                _tessDataPath, Idiomas, EngineMode.TesseractAndLstm);
            ConfigurarParametrosTesseract(engine);

            // Renderizamos la primera página del PDF
            using var paginaBitmap = RenderizarPagina(rutaPdf, 0);
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

        // ── Configuración de parámetros Tesseract ─────────────────────────────

        private void ConfigurarParametrosTesseract(TesseractEngine engine)
        {
            // PSM 6: Assume a single uniform block of text
            engine.SetVariable("tesseract_create_pdf", false);
            engine.SetVariable("tessedit_pageseg_mode", 6);

            // Caracteres a ignorar (blacklist)
            engine.SetVariable("tessedit_char_blacklist", "\\!|=@#$£&*{}[]:;");

            // Preservar espacios entre palabras
            engine.SetVariable("preserve_interword_spaces", 1);

            // Opcional: Mejorar precisión con español
            engine.SetVariable("language_model_penalty_non_dict_word", 0.1);
            engine.SetVariable("language_model_penalty_non_freq_dict_word", 0.1);
        }

        // ── Extracción de texto de una zona específica ───────────────────────

        /// <summary>
        /// Extrae el texto OCR de una zona específica del PDF.
        /// </summary>
        public string ExtraerTextoZonal(string rutaPdf, ZonaOcr zona)
        {
            if (zona == null)
                return string.Empty;

            using var engine = new TesseractEngine(
                _tessDataPath, Idiomas, EngineMode.Default);

            // Renderizamos la primera página del PDF
            using var paginaBitmap = RenderizarPagina(rutaPdf, 0);
            if (paginaBitmap == null)
                return string.Empty;

            try
            {
                // Convertimos coordenadas porcentuales a píxeles
                var rect = zona.ToRectangle(paginaBitmap.Width, paginaBitmap.Height);

                // Recortamos la zona de la imagen
                using var zonaImagen = RecortarZona(paginaBitmap, rect);
                if (zonaImagen == null)
                    return string.Empty;

                // Aplicamos OCR a la zona recortada
                string texto = AplicarOcr(engine, zonaImagen);
                return texto.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extrayendo zona: {ex.Message}");
                return string.Empty;
            }
        }

        // ── Renderizado de página ─────────────────────────────────────────────

        public Bitmap? RenderizarPagina(string rutaPdf, int numeroPagina)
        {
            try
            {
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);

                using var skBitmap = Conversion.ToImage(
                    pdfBytes,
                    page: new Index(numeroPagina),
                    password: null,
                    options: new RenderOptions(Dpi: DpiRender));

                return ConvertirSkBitmapABitmap(skBitmap);
            }
            catch
            {
                return null;
            }
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        private Bitmap? RecortarZona(Bitmap imagen, Rectangle rect)
        {
            // Ajustamos el rectángulo para que no salga fuera de la imagen
            rect = Rectangle.Intersect(rect,
                new Rectangle(0, 0, imagen.Width, imagen.Height));

            if (rect.Width <= 0 || rect.Height <= 0) return null;

            var zonaImagen = new Bitmap(rect.Width, rect.Height);
            using var g = Graphics.FromImage(zonaImagen);
            g.DrawImage(imagen, new Rectangle(0, 0, rect.Width, rect.Height),
                rect, GraphicsUnit.Pixel);

            return zonaImagen;
        }

        private string AplicarOcr(TesseractEngine engine, Bitmap imagen)
        {
            using var ms = new MemoryStream();
            imagen.Save(ms, DrawingImageFormat.Png); // ← DrawingImageFormat, no ImageFormat
            using var pix = Pix.LoadFromMemory(ms.ToArray());
            using var page = engine.Process(pix);
            return page.GetText();
        }

        private Bitmap? ConvertirSkBitmapABitmap(SKBitmap skBitmap)
        {
            try
            {
                using var skImage = SKImage.FromBitmap(skBitmap);
                using var skData = skImage.Encode(
                    SKEncodedImageFormat.Png, 100);
                using var ms = new MemoryStream(skData.ToArray());
                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }
    }
}