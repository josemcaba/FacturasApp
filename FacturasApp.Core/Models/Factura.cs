namespace FacturasApp.Core.Models
{
    public class Factura
    {
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public Empresa Emisor { get; set; } = new Empresa();
        public Empresa Receptor { get; set; } = new Empresa();
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

        // ── Total calculado — SIEMPRE ÚNICO por factura (documento) ─────────
        // · Factura única : Base + CuotaIVA − CuotaIRPF + CuotaRE.
        // · Factura multilínea: suma de los subtotales CALCULADOS de las líneas
        //   (todas las líneas de la misma factura muestran el mismo valor); la
        //   fija el motor con FijarTotalCalculado() al terminar de parsear.
        private decimal? _totalCalculadoMultilinea;
        public decimal TotalCalculado => _totalCalculadoMultilinea ??
            BaseImponible + CuotaIVACalculado - CuotaIRPFCalculado + CuotaRECalculado;
        public void FijarTotalCalculado(decimal valor) => _totalCalculadoMultilinea = valor;

        // En facturas multilínea la verificación por línea se hace sobre el
        // subtotal de la línea (lo usa el «Importe total», la columna Total del
        // grid y el campo obligatorio); la validación del documento es
        // TotalCalculado (único) contra TotalFactura extraído.
        public bool EsMultiLinea { get; set; }
        public decimal ImporteVerificacion => EsMultiLinea ? SubTotal : TotalFactura;

        // Comparación SIEMPRE igual: Total calculado (único de la factura)
        // contra TotalFactura extraído del documento.
        public decimal DiferenciaTotal =>
            Math.Abs(TotalCalculado - TotalFactura);

        // Tolerancia aceptable en la comparación de totales (0,05€)
        private const decimal ToleranciaTotal = 0.05m;

        public bool TotalesCoinciden =>
            DiferenciaTotal <= ToleranciaTotal;

        // ── Metadatos ────────────────────────────────────────────────────────
        public string RutaArchivo { get; set; } = string.Empty;
        public bool ExtractedByOcr { get; set; }
        public EstadoFactura Estado { get; set; } = EstadoFactura.Pendiente;
        public List<string> MensajeError { get; set; } = new();
    }
}