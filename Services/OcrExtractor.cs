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

        // Integración con plantillas zonales
        public string ExtraerTextoConOcrInteligente(
            string rutaPdf, string nombreEmisor)
        {
            var plantillaService = new PlantillaOcrService();
            var plantilla = plantillaService.ObtenerPorEmisor(nombreEmisor);

            if (plantilla == null || plantilla.Zonas.Count == 0)
                return ExtraerTextoConOcr(rutaPdf);

            var zonalExtractor = new OcrZonalExtractor(_tessDataPath);
            var textosPorCampo = zonalExtractor.ExtraerZonas(rutaPdf, plantilla);

            var sb = new System.Text.StringBuilder();
            foreach (var kvp in textosPorCampo)
                sb.AppendLine($"[{kvp.Key}]: {kvp.Value}");

            return sb.ToString();
        }
    }
}