namespace FacturasApp.Models
{
    public class Factura
    {
        // ── Identificación ──────────────────────────────────────
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }

        // ── Emisor (proveedor) ───────────────────────────────────
        public Proveedor Emisor { get; set; } = new Proveedor();

        // ── Receptor (cliente) ───────────────────────────────────
        public Cliente Receptor { get; set; } = new Cliente();

        // ── Importes ─────────────────────────────────────────────
        public decimal BaseImponible { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public decimal CuotaIVA => BaseImponible * (PorcentajeIVA / 100);
        public decimal Total { get; set; }

        // ── Metadatos del procesado ──────────────────────────────
        public string RutaArchivo { get; set; } = string.Empty;
        public bool ExtractedByOcr { get; set; }
        public EstadoFactura Estado { get; set; } = EstadoFactura.Pendiente;
        public string? ErrorMensaje { get; set; }
    }
}
