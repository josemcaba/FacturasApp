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

        public string ExtraerTextoIdentificacion(string rutaPdf)
        {
            using var engine = CrearEngine();
            ConfigurarParametrosTesseract(engine);

            var bitmap = RenderizarPaginaReducida(rutaPdf, 0, 150);
            if (bitmap == null) return string.Empty;

            int altoTercio = bitmap.Height / 3;
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, altoTercio);
            var bitmapTercio = bitmap.Clone(rect, bitmap.PixelFormat);
            bitmap.Dispose();

            string texto;
            using (bitmapTercio)
            {
                texto = AplicarOcr(engine, bitmapTercio);
            }
            return texto.Trim();
        }


    }
}