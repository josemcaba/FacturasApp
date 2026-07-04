using System.Drawing;

namespace FacturasApp.Models
{
    public static class EstadoFacturaExtensions
    {
        public static string ToDisplayText(this EstadoFactura estado) => estado switch
        {
            EstadoFactura.OK => "✔ Correcto",
            EstadoFactura.Revisar => "⚠ Revisar",
            EstadoFactura.Error => "✖ Error",
            _ => "Pendiente"
        };

        public static Color ToColor(this EstadoFactura estado) => estado switch
        {
            EstadoFactura.OK => Color.FromArgb(226, 239, 218),
            EstadoFactura.Revisar => Color.FromArgb(255, 242, 204),
            EstadoFactura.Error => Color.FromArgb(255, 228, 214),
            _ => Color.White
        };
    }
}
