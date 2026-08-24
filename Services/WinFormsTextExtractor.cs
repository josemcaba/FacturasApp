using FacturasApp.Core.Models;
using FacturasApp.Core.Services;

namespace FacturasApp.Services
{
    /// <summary>
    /// Implementación de ITextExtractor para WinForms.
    /// Delega en PdfTextExtractor, OcrExtractor y OcrZonalExtractor existentes.
    /// </summary>
    public class WinFormsTextExtractor : ITextExtractor
    {
        private readonly PdfTextExtractor _pdfTextExtractor = new();
        private readonly OcrExtractor _ocrExtractor;
        private readonly OcrZonalExtractor _ocrZonalExtractor;

        public WinFormsTextExtractor(string tessDataPath = @"./tessdata")
        {
            _ocrExtractor = new OcrExtractor(tessDataPath);
            _ocrZonalExtractor = new OcrZonalExtractor(tessDataPath);
        }

        public string? ExtraerTextoSeleccionable(string rutaPdf, ModoExtraccion modo = ModoExtraccion.Simple)
        {
            var modoPdfium = modo == ModoExtraccion.Simple
                ? PdfTextExtractor.ModoExtraccion.Simple
                : PdfTextExtractor.ModoExtraccion.Ordenado;

            return _pdfTextExtractor.ExtraerTextoSeleccionable(rutaPdf, modoPdfium);
        }

        public string ExtraerTextoOcrCompleto(string rutaPdf)
        {
            return _ocrExtractor.ExtraerTextoConOcr(rutaPdf);
        }

        public string ExtraerTextoZonal(string rutaPdf, PlantillaOcr plantilla)
        {
            var textosZonas = _pdfTextExtractor.ExtraerZonasTexto(rutaPdf, plantilla);
            var sb = new System.Text.StringBuilder();
            foreach (var zona in plantilla.Zonas)
            {
                if (textosZonas.TryGetValue(zona.Campo, out string? texto) && !string.IsNullOrWhiteSpace(texto))
                    sb.AppendLine($"[{zona.Campo}]: {texto}");
            }
            string resultado = sb.ToString().Trim();
            return resultado.Length < 10 ? string.Empty : resultado;
        }

        public string ExtraerTextoOcrZonal(string rutaPdf, PlantillaOcr plantilla)
        {
            var textosZonas = _ocrZonalExtractor.ExtraerZonas(rutaPdf, plantilla);
            var sb = new System.Text.StringBuilder();
            foreach (var zona in plantilla.Zonas)
            {
                if (textosZonas.TryGetValue(zona.Campo, out string? texto) && !string.IsNullOrWhiteSpace(texto))
                    sb.AppendLine($"[{zona.Campo}]: {texto}");
            }
            string resultado = sb.ToString().Trim();
            return resultado.Length < 10 ? string.Empty : resultado;
        }

        public string ExtraerTextoOcrIdentificacion(string rutaPdf)
        {
            return _ocrExtractor.ExtraerTextoIdentificacion(rutaPdf);
        }

        public bool EsSeleccionable(string rutaPdf)
        {
            return _pdfTextExtractor.EsSeleccionable(rutaPdf);
        }
    }
}
