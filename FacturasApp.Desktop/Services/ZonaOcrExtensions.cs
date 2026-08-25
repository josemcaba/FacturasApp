using FacturasApp.Core.Models;

namespace FacturasApp.Services
{
    /// <summary>
    /// Métodos de extensión para ZonaOcr que dependen de System.Drawing.
    /// </summary>
    public static class ZonaOcrExtensions
    {
        /// <summary>
        /// Convierte las coordenadas porcentuales a píxeles según el tamaño real de la imagen.
        /// </summary>
        public static System.Drawing.Rectangle ToRectangle(this ZonaOcr zona, int imgAncho, int imgAlto)
        {
            return new System.Drawing.Rectangle(
                (int)(zona.X * imgAncho / 100.0),
                (int)(zona.Y * imgAlto / 100.0),
                (int)(zona.Ancho * imgAncho / 100.0),
                (int)(zona.Alto * imgAlto / 100.0));
        }
    }
}
