using System.Drawing;
using FacturasApp.Core.Models;

namespace FacturasApp.UI;

public static class EstadoFacturaPresentacion
{
    public static string ToDisplayText(this EstadoFactura estado) => ObtenerFormato(estado).Texto;

    public static Color ToColor(this EstadoFactura estado) => ObtenerFormato(estado).Color;

    private static (string Texto, Color Color) ObtenerFormato(EstadoFactura estado) => estado switch
    {
        EstadoFactura.OK => ("✔ Correcto", Color.FromArgb(226, 239, 218)),
        EstadoFactura.Revisar => ("⚠ Revisar", Color.FromArgb(255, 242, 204)),
        EstadoFactura.Duplicada => ("⚠ Duplicada", Color.FromArgb(230, 230, 250)),
        EstadoFactura.Error => ("✖ Error", Color.FromArgb(252, 228, 214)),
        EstadoFactura.Pendiente => ("• Pendiente", Color.White),
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado,
            $"Estado de factura desconocido: {estado}")
    };
}