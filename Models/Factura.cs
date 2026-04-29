using FacturasApp.Services;

namespace FacturasApp.Models
{
    public class Factura
    {
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public Proveedor Emisor { get; set; } = new Proveedor();
        public Cliente Receptor { get; set; } = new Cliente();
        public string Concepto { get; set; } = string.Empty; // Código contable
        public decimal BaseImponible { get; set; }
        public decimal PorcentajeIVA { get; set; } = 0m;
        public decimal CuotaIVA => Math.Round(BaseImponible * (PorcentajeIVA / 100m), 2, 
            MidpointRounding.AwayFromZero);
        public decimal PorcentajeIRPF { get; set; } = 0m;
        public decimal CuotaIRPF => Math.Round(BaseImponible * (PorcentajeIRPF / 100m), 2,
            MidpointRounding.AwayFromZero);
        public decimal PorcentajeRE { get; set; } = 0m;
        public decimal CuotaRE => Math.Round(BaseImponible * (PorcentajeRE / 100m), 2,
            MidpointRounding.AwayFromZero);
        public decimal Total { get; set; }
        public decimal TotalCalculado =>
            BaseImponible + CuotaIVA - CuotaIRPF + CuotaRE;
        public decimal DiferenciaTotal =>
            Math.Abs(Total - TotalCalculado);

        // Tolerancia aceptable en la comparación de totales (0,01€)
        private const decimal ToleranciaTotal = 0.01m;

        public bool TotalesCoinciden =>
            DiferenciaTotal <= ToleranciaTotal;

        // ── Metadatos ────────────────────────────────────────────────────────
        public string RutaArchivo { get; set; } = string.Empty;
        public bool ExtractedByOcr { get; set; }
        public _Estado Estado { get; set; } = _Estado.Pendiente;
        public List<string> MensajeError { get; set; } = new();
    }
}