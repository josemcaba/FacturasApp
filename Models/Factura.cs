namespace FacturasApp.Models
{
    public class Factura
    {
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public Empresa Emisor { get; set; } = new Proveedor();
        public Empresa Receptor { get; set; } = new Cliente();
        public string ConceptoIngreso { get; set; } = "700"; // Código contable por defecto
        public string ConceptoGasto { get; set; } = "600"; // Código contable por defecto
        public decimal BaseImponible { get; set; }
        public decimal PorcentajeIVA { get; set; } = 0m;
        public decimal CuotaIVA { get; set; }
        public decimal CuotaIVACalculado => Math.Round(BaseImponible * (PorcentajeIVA / 100m), 2, 
            MidpointRounding.AwayFromZero);
        public decimal PorcentajeIRPF { get; set; } = 0m;
        public decimal CuotaIRPF{ get; set; }
        public decimal CuotaIRPFCalculado => Math.Round(BaseImponible * (PorcentajeIRPF / 100m), 2,
            MidpointRounding.AwayFromZero);
        public decimal PorcentajeRE { get; set; } = 0m;
        public decimal CuotaRE { get; set; }
        public decimal CuotaRECalculado => Math.Round(BaseImponible * (PorcentajeRE / 100m), 2,
            MidpointRounding.AwayFromZero);
        public decimal Total { get; set; }
        public decimal TotalCalculado =>
            BaseImponible + CuotaIVACalculado - CuotaIRPFCalculado + CuotaRECalculado;
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
        public List<string> MensajeError { get; set; } = new();
    }
}