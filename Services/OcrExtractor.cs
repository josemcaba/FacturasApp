using Tesseract;

namespace FacturasApp.Services
{
    public class OcrExtractor : OcrBase
    {
        public OcrExtractor(string tessDataPath = @"./tessdata")
            : base(tessDataPath) { }

        public string ExtraerTextoConOcr(string rutaPdf)
        {
            var textoTotal = new System.Text.StringBuilder();

            using var engine = CrearEngine();
            ConfigurarParametrosTesseract(engine);

            var paginas = RenderizarPaginas(rutaPdf);

            foreach (var bitmap in paginas)
            {
                try
                {
                    textoTotal.AppendLine(AplicarOcr(engine, bitmap));
                }
                catch { }
                finally
                {
                    bitmap.Dispose();
                }
            }

            return textoTotal.ToString().Trim();
        }


    }
}