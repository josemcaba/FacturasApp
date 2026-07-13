using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using PDFtoImage;
using SkiaSharp;
using Tesseract;

namespace FacturasApp.Services
{
    public abstract class OcrBase
    {
        protected readonly string _tessDataPath;
        protected const string Idiomas = "spa";
        protected const int DpiRender = 300;

        protected OcrBase(string tessDataPath = @"./tessdata")
        {
            _tessDataPath = tessDataPath;
        }

        // ── Motor Tesseract ───────────────────────────────────────────────────

        protected TesseractEngine CrearEngine() =>
            new(_tessDataPath, Idiomas, EngineMode.TesseractAndLstm);

        protected void ConfigurarParametrosTesseract(TesseractEngine engine)
        {
            engine.SetVariable("tesseract_create_pdf", false);
            engine.SetVariable("tessedit_pageseg_mode", 6);
            engine.SetVariable("tessedit_char_blacklist", "\\!|=@#$£&*{}[]:;");
            engine.SetVariable("preserve_interword_spaces", 1);
            engine.SetVariable("language_model_penalty_non_dict_word", 0.1);
            engine.SetVariable("language_model_penalty_non_freq_dict_word", 0.1);
        }

        // ── Renderizado ───────────────────────────────────────────────────────

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

        public List<Bitmap> RenderizarPaginas(string rutaPdf)
        {
            var resultado = new List<Bitmap>();

            try
            {
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);
                int numPaginas = Conversion.GetPageCount(pdfBytes);

                for (int i = 0; i < numPaginas; i++)
                {
                    try
                    {
                        using var skBitmap = Conversion.ToImage(
                            pdfBytes,
                            page: new Index(i),
                            password: null,
                            options: new RenderOptions(Dpi: DpiRender));

                        var bitmap = ConvertirSkBitmapABitmap(skBitmap);
                        if (bitmap != null)
                            resultado.Add(bitmap);
                    }
                    catch { }
                }
            }
            catch { }

            return resultado;
        }

        // ── Conversiones ──────────────────────────────────────────────────────

        protected Bitmap? ConvertirSkBitmapABitmap(SKBitmap skBitmap)
        {
            try
            {
                using var skImage = SKImage.FromBitmap(skBitmap);
                using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
                using var ms = new MemoryStream(skData.ToArray());
                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }

        protected Pix? ConvertirAPix(byte[] bytesImagen)
        {
            try
            {
                return Pix.LoadFromMemory(bytesImagen);
            }
            catch
            {
                return null;
            }
        }

        protected Pix? ConvertirAPix(Bitmap bitmap)
        {
            try
            {
                using var ms = new MemoryStream();
                bitmap.Save(ms, DrawingImageFormat.Png);
                return Pix.LoadFromMemory(ms.ToArray());
            }
            catch
            {
                return null;
            }
        }

        // ── OCR ───────────────────────────────────────────────────────────────

        protected string AplicarOcr(TesseractEngine engine, Bitmap imagen)
        {
            using var pix = ConvertirAPix(imagen);
            if (pix == null) return string.Empty;
            using var page = engine.Process(pix);
            return page.GetText();
        }
    }
}