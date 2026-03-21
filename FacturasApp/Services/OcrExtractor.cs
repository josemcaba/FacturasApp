using UglyToad.PdfPig;
using PdfPigPage = UglyToad.PdfPig.Content.Page;
using System.Drawing;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using Tesseract;

namespace FacturasApp.Services
{
    public class OcrExtractor
    {
        private readonly string _tessDataPath;
        private const string Idiomas = "spa+eng";

        public OcrExtractor(string tessDataPath = @"./tessdata")
        {
            _tessDataPath = tessDataPath;
        }

        public string ExtraerTextoConOcr(string rutaPdf)
        {
            var textoTotal = new System.Text.StringBuilder();

            using var engine = new TesseractEngine(
                _tessDataPath, Idiomas, EngineMode.Default);
            using var documento = PdfDocument.Open(rutaPdf);

            foreach (PdfPigPage pagina in documento.GetPages())
            {
                var imagenes = pagina.GetImages().ToList();
                if (imagenes.Count == 0) continue;

                var imagenPrincipal = imagenes
                    .OrderByDescending(i => i.WidthInSamples * i.HeightInSamples)
                    .First();

                try
                {
                    using var pix = ConvertirAPix(imagenPrincipal.RawBytes.ToArray());
                    if (pix == null) continue;

                    using var page = engine.Process(pix);
                    textoTotal.AppendLine(page.GetText());
                }
                catch
                {
                    // Si una página falla continuamos con la siguiente
                }
            }

            return textoTotal.ToString().Trim();
        }

        private Pix? ConvertirAPix(byte[] bytesImagen)
        {
            try
            {
                using var ms = new MemoryStream(bytesImagen);
                using var bitmap = new Bitmap(ms);

                using var msPng = new MemoryStream();
                bitmap.Save(msPng, DrawingImageFormat.Png); // ← alias aplicado aquí
                return Pix.LoadFromMemory(msPng.ToArray());
            }
            catch
            {
                return null;
            }
        }
    }
}
