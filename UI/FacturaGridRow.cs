using FacturasApp.Models;

namespace FacturasApp.UI
{
    public class FacturaGridRow
    {
        public string NumeroFactura { get; }
        public string FechaFormateada { get; }
        public string EmisorNombre { get; }
        public string EmisorNif { get; }
        public string ClienteNombre { get; }
        public string ClienteNif { get; }
        public string BaseFormateada { get; }
        public decimal PorcentajeIVA { get; }
        public string CuotaIvaFmt { get; }
        public decimal PorcentajeIRPF { get; }
        public string CuotaIrpfFmt { get; }
        public decimal PorcentajeRE { get; }
        public string CuotaREFmt { get; }
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
            ClienteNombre = f.Receptor.Nombre;
            ClienteNif = f.Receptor.NIF;
            BaseFormateada = f.BaseImponible > 0
                                ? $"{f.BaseImponible:N2} €" : "0,00 €";
            PorcentajeIVA = f.PorcentajeIVA;
            CuotaIvaFmt = $"{f.CuotaIVA:N2} €";
            PorcentajeIRPF = f.PorcentajeIRPF;
            CuotaIrpfFmt = $"{f.CuotaIRPF:N2} €";
            PorcentajeRE = f.PorcentajeRE;
            CuotaREFmt = $"{f.CuotaRE:N2} €";
            TotalFormateado = $"{f.Total:N2} €";
            EstadoTexto = f.Estado switch
            {
                EstadoFactura.OK => "✔ Correcto",
                EstadoFactura.RevisionManual => "⚠ Revisar",
                EstadoFactura.Error => "✖ Error",
                _ => "Pendiente"
            };
            ExtractedByOcr = f.ExtractedByOcr;
            NombreArchivo = Path.GetFileName(f.RutaArchivo);
        }
    }
}