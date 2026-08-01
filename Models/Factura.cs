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
        private decimal? _cuotaIVACalculadaFijada;
        public decimal CuotaIVACalculado => _cuotaIVACalculadaFijada ??
            Math.Round(BaseImponible * (PorcentajeIVA / 100m), 2,
                MidpointRounding.AwayFromZero);
        public void FijarCuotaIVACalculado(decimal valor) => _cuotaIVACalculadaFijada = valor;
        public bool CuotaIVACalculadoFijada => _cuotaIVACalculadaFijada.HasValue;
        public decimal PorcentajeIRPF { get; set; } = 0m;
        public decimal CuotaIRPF { get; set; }
        private decimal? _cuotaIRPFCalculadaFijada;
        public decimal CuotaIRPFCalculado => _cuotaIRPFCalculadaFijada ??
            Math.Round(BaseImponible * (PorcentajeIRPF / 100m), 2,
                MidpointRounding.AwayFromZero);
        public void FijarCuotaIRPFCalculado(decimal valor) => _cuotaIRPFCalculadaFijada = valor;
        public bool CuotaIRPFCalculadoFijada => _cuotaIRPFCalculadaFijada.HasValue;
        public decimal PorcentajeRE { get; set; } = 0m;
        public decimal CuotaRE { get; set; }
        private decimal? _cuotaRECalculadaFijada;
        public decimal CuotaRECalculado => _cuotaRECalculadaFijada ??
            Math.Round(BaseImponible * (PorcentajeRE / 100m), 2,
                MidpointRounding.AwayFromZero);
        public void FijarCuotaRECalculado(decimal valor) => _cuotaRECalculadaFijada = valor;
        public bool CuotaRECalculadoFijada => _cuotaRECalculadaFijada.HasValue;
        public decimal TotalFactura { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalCalculado =>
            BaseImponible + CuotaIVACalculado - CuotaIRPFCalculado + CuotaRECalculado;

        // En facturas multilínea la verificación se hace sobre el subtotal de la línea
        public bool EsMultiLinea { get; set; }
        public decimal ImporteVerificacion => EsMultiLinea ? SubTotal : TotalFactura;
        public decimal DiferenciaTotal =>
            Math.Abs(ImporteVerificacion - TotalCalculado);

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