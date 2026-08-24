using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services
{
    /// <summary>
    /// Interfaz para la extracción de texto de PDFs.
    /// Permite que WinForms y la app web implementen sus propias estrategias.
    /// </summary>
    public interface ITextExtractor
    {
        /// <summary>
        /// Extrae texto seleccionable nativo de un PDF.
        /// </summary>
        /// <param name="rutaPdf">Ruta al archivo PDF.</param>
        /// <param name="modo">Modo de extracción (Simple u Ordenado).</param>
        /// <returns>El texto extraído o null si no tiene texto seleccionable.</returns>
        string? ExtraerTextoSeleccionable(string rutaPdf, ModoExtraccion modo = ModoExtraccion.Simple);

        /// <summary>
        /// Extrae texto usando OCR (para PDFs escaneados).
        /// </summary>
        string ExtraerTextoOcrCompleto(string rutaPdf);

        /// <summary>
        /// Extrae texto de zonas específicas usando coordenadas (sin OCR).
        /// </summary>
        string ExtraerTextoZonal(string rutaPdf, PlantillaOcr plantilla);

        /// <summary>
        /// Extrae texto de zonas específicas usando OCR.
        /// </summary>
        string ExtraerTextoOcrZonal(string rutaPdf, PlantillaOcr plantilla);

        /// <summary>
        /// Extrae solo el texto necesario para identificar el emisor (top de la primera página).
        /// </summary>
        string ExtraerTextoOcrIdentificacion(string rutaPdf);

        /// <summary>
        /// Verifica si un PDF tiene texto seleccionable.
        /// </summary>
        bool EsSeleccionable(string rutaPdf);
    }
}
