using FacturasApp.Models;

namespace FacturasApp.UI
{
    public class FacturaGridRow
    {
        public string NumeroFactura { get; }
        public string FechaFormateada { get; }
        public string EmisorNombre { get; }
        public string EmisorNif { get; }
        public string BaseFormateada { get; }
        public decimal PorcentajeIVA { get; }
        public string TotalFormateado { get; }
        public string EstadoTexto { get; }
        public bool ExtractedByOcr { get; }
        public string NombreArchivo { get; }
        public Factura FacturaOriginal { get; }

        public FacturaGridRow(Factura f)
        {
            FacturaOriginal = f;
            NumeroFactura = f.NumeroFactura;
            FechaFormateada = f.Fecha?.ToString("dd/MM/yyyy") ?? "—";
            EmisorNombre = f.Emisor.Nombre;
            EmisorNif = f.Emisor.NIF;
            BaseFormateada = f.BaseImponible > 0
                                ? $"{f.BaseImponible:N2} €" : "—";
            PorcentajeIVA = f.PorcentajeIVA;
            TotalFormateado = f.Total > 0 ? $"{f.Total:N2} €" : "—";
            EstadoTexto = f.Estado switch
            {
                EstadoFactura.OK => "✔ Correcto",
                EstadoFactura.RevisionManual => "⚠ Revisión manual",
                EstadoFactura.Error => "✖ Error",
                _ => "Pendiente"
            };
            ExtractedByOcr = f.ExtractedByOcr;
            NombreArchivo = Path.GetFileName(f.RutaArchivo);
        }
    }
}
