using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FacturasApp.Core.Services;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using PdfiumViewer;
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
            // Imagen (JPG/PNG): no hay páginas que renderizar, la propia
            // imagen es la página 0 y su bitmap es el original.
            if (FormatosDocumento.EsImagen(rutaPdf))
                return numeroPagina == 0 ? CargarImagen(rutaPdf) : null;

            try
            {
                using var documento = PdfDocument.Load(rutaPdf);
                return RenderizarPagina(documento, numeroPagina, DpiRender);
            }
            catch
            {
                return null;
            }
        }

        public List<Bitmap> RenderizarPaginas(string rutaPdf)
        {
            var resultado = new List<Bitmap>();

            if (FormatosDocumento.EsImagen(rutaPdf))
            {
                var bitmap = CargarImagen(rutaPdf);
                if (bitmap != null)
                    resultado.Add(bitmap);
                return resultado;
            }

            try
            {
                using var documento = PdfDocument.Load(rutaPdf);
                for (int i = 0; i < documento.PageCount; i++)
                {
                    try
                    {
                        var bitmap = RenderizarPagina(documento, i, DpiRender);
                        if (bitmap != null)
                            resultado.Add(bitmap);
                    }
                    catch { }
                }
            }
            catch { }

            return resultado;
        }

        public Bitmap? RenderizarPaginaReducida(string rutaPdf, int numeroPagina, int dpi)
        {
            // En imágenes el DPI pedido no aplica: se devuelve la original
            // (EscaladaParaOcr ya se asegura de que sea lo bastante grande).
            if (FormatosDocumento.EsImagen(rutaPdf))
                return numeroPagina == 0 ? CargarImagen(rutaPdf) : null;

            try
            {
                using var documento = PdfDocument.Load(rutaPdf);
                return RenderizarPagina(documento, numeroPagina, dpi);
            }
            catch
            {
                return null;
            }
        }

        // ── Carga de imágenes (JPG/PNG) ──────────────────────────────────────

        /// <summary>
        /// Carga una imagen como Bitmap listo para Tesseract o para la
        /// vista previa. El Bitmap devuelto es responsabilidad del llamador
        /// (igual que con el renderizado de PDFs): NO se dispone aquí.
        /// </summary>
        public static Bitmap? CargarImagen(string ruta)
        {
            try
            {
                // Image.FromFile deja el fichero bloqueado mientras vive el
                // objeto: se pinta en un bitmap propio y se dispone la imagen
                // al instante para no retener el handle.
                using var imagen = System.Drawing.Image.FromFile(ruta);

                var bitmap = new Bitmap(imagen.Width, imagen.Height,
                    PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(bitmap))
                    g.DrawImage(imagen, 0, 0, imagen.Width, imagen.Height);

                return EscalarParaOcr(bitmap);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tesseract rinde hacia 300 DPI: las fotos de móvil y los escaneos
        /// suelen pasar de 2000 px de ancho, pero una captura pequeña no,
        /// así que se amplía para acercarse a esa resolución.
        /// </summary>
        private static Bitmap EscalarParaOcr(Bitmap bitmap)
        {
            const int anchoObjetivo = 2000;

            if (bitmap.Width >= anchoObjetivo)
                return bitmap;

            int nuevoAncho = anchoObjetivo;
            int nuevoAlto = (int)Math.Round(
                bitmap.Height * (double)nuevoAncho / bitmap.Width);

            var escalada = new Bitmap(nuevoAncho, nuevoAlto, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(escalada))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, nuevoAncho, nuevoAlto);
            }

            bitmap.Dispose();
            return escalada;
        }

        // ── Renderizado (PDFium) ──────────────────────────────────────────────

        private static Bitmap? RenderizarPagina(PdfDocument documento, int numeroPagina, int dpi)
        {
            try
            {
                // Tamaño de página en puntos PDF (72 ppp); se escala al DPI pedido.
                SizeF tamano = documento.PageSizes[numeroPagina];
                int ancho = (int)Math.Ceiling(tamano.Width * dpi / 72f);
                int alto = (int)Math.Ceiling(tamano.Height * dpi / 72f);

                // El Bitmap devuelto es la imagen real (System.Drawing) — NO se
                // dispone aquí: el llamador es responsable de liberarla.
                var imagen = documento.Render(
                    numeroPagina, ancho, alto, dpi, dpi, PdfRenderFlags.ForPrinting);
                return imagen as Bitmap;
            }
            catch
            {
                return null;
            }
        }

        // ── Conversiones ──────────────────────────────────────────────────────

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