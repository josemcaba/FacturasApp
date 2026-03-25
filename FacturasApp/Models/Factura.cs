namespace FacturasApp.Models
{
    public class Factura
    {
        // ── Identificación ───────────────────────────────────────────────────
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }

        // ── Emisor ───────────────────────────────────────────────────────────
        public Proveedor Emisor { get; set; } = new Proveedor();

        // ── Receptor ─────────────────────────────────────────────────────────
        public Cliente Receptor { get; set; } = new Cliente();

        // ── Importes ─────────────────────────────────────────────────────────
        public decimal BaseImponible { get; set; }

        public decimal PorcentajeIVA { get; set; } = 0m;
        public decimal CuotaIVA => BaseImponible * (PorcentajeIVA / 100m);

        public decimal PorcentajeIRPF { get; set; } = 0m;
        public decimal CuotaIRPF => BaseImponible * (PorcentajeIRPF / 100m);

        public decimal PorcentajeRE { get; set; } = 0m;
        public decimal CuotaRE => BaseImponible * (PorcentajeRE / 100m);

        // Total extraído directamente de la factura
        public decimal Total { get; set; }

        // Total calculado: Base + CuotaIVA - CuotaIRPF + CuotaRE
        public decimal TotalCalculado =>
            BaseImponible + CuotaIVA - CuotaIRPF + CuotaRE;

        // Diferencia entre total extraído y calculado
        public decimal DiferenciaTotal =>
            Math.Abs(Total - TotalCalculado);

        // Tolerancia aceptable en la comparación de totales (0,01€)
        private const decimal ToleranciaTotal = 0.01m;

        public bool TotalesCoinciden =>
            DiferenciaTotal <= ToleranciaTotal;

        // ── Metadatos ────────────────────────────────────────────────────────
        public string RutaArchivo { get; set; } = string.Empty;
        public bool ExtractedByOcr { get; set; }
        public EstadoFactura Estado { get; set; } = EstadoFactura.Pendiente;
        public string? ErrorMensaje { get; set; }
    }
}
