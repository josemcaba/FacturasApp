using System.Drawing;

namespace FacturasApp.Models
{
    public static class EstadoFacturaExtensions
    {
        public static string ToDisplayText(this EstadoFactura estado) => estado switch
        {
            EstadoFactura.OK => "✔ Correcto",
            EstadoFactura.Revisar => "⚠ Revisar",
            EstadoFactura.Duplicada => "⚠ Duplicada",
            EstadoFactura.Error => "✖ Error",
            _ => "Pendiente"
        };

        public static Color ToColor(this EstadoFactura estado) => estado switch
        {
            EstadoFactura.OK => Color.FromArgb(226, 239, 218),
            EstadoFactura.Revisar => Color.FromArgb(255, 242, 204),
            EstadoFactura.Duplicada => Color.FromArgb(230, 230, 250),
            EstadoFactura.Error => Color.FromArgb(252, 228, 214),
            _ => Color.White
        };
    }
}
