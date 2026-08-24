using ClosedXML.Excel;
using FacturasApp.Models;
using FacturasApp.UI;

namespace FacturasApp.Services
{
    public class ExportService
    {
        // ── Índices de columna (1-based) ──────────────────────────────────────
        private const int ColNumeroFactura = 1;
        private const int ColFecha = 2;
        private const int ColFechaOperacion = 3;
        private const int ColConcepto = 4;
        private const int ColBaseIVA = 5;
        private const int ColPorcentajeIVA = 6;
        private const int ColCuotaIVA = 7;
        private const int ColBaseIRPF = 8;
        private const int ColPorcentajeIRPF = 9;
        private const int ColCuotaIRPF = 10;
        private const int ColBaseRE = 11;
        private const int ColPorcentajeRE = 12;
        private const int ColCuotaRE = 13;
        private const int ColNifEntidad = 14;
        private const int ColNombreEntidad = 15;
        private const int ColArchivo = 16;

        // ── Excel ────────────────────────────────────────────────────────────

        public void ExportarAExcel(List<Factura> facturas, string rutaDestino, bool esGasto)
        {
            using var workbook = new XLWorkbook();

            var correctas = facturas.Where(f => f.Estado == EstadoFactura.OK).ToList();
            var incorrectas = facturas.Where(f => f.Estado != EstadoFactura.OK).ToList();

            string tipo = esGasto ? "Gastos" : "Ingresos";
            CrearHoja(workbook, correctas, $"{tipo} Correctos", esGasto);
            CrearHoja(workbook, incorrectas, $"{tipo} Pendientes", esGasto);

            workbook.SaveAs(rutaDestino);
        }

        // ── Métodos privados de creación de hojas ────────────────────────────────────

        private void CrearHoja(XLWorkbook workbook,
            List<Factura> facturas, string nombreHoja, bool esGasto)
        {
            var hoja = workbook.Worksheets.Add(nombreHoja);

            EscribirConceptosComunes(facturas, hoja, esGasto);

            for (int i = 0; i < facturas.Count; i++)
            {
                var f = facturas[i];
                int fila = i + 2;

                hoja.Cell(fila, ColConcepto).Value = esGasto ? f.ConceptoGasto : f.ConceptoIngreso;
                hoja.Cell(fila, ColNifEntidad).Value = esGasto ? f.Emisor.NIF : f.Receptor.NIF;
                hoja.Cell(fila, ColNombreEntidad).Value = esGasto ? f.Emisor.Nombre : f.Receptor.Nombre;
            }

            hoja.Columns().AdjustToContents();
        }

        // ── Helpers compartidos ───────────────────────────────────────────────────────

        private int EscribirCabecera(IXLWorksheet hoja, bool esGasto)
        {
            string entidad = esGasto ? "Emisor" : "Cliente";
            string[] columnas =
            {
                "Número de factura", "Fecha de factura", "Fecha de operación",
                "Concepto", "Base IVA", "% IVA", "Cuota IVA",
                "Base IRPF", "% IRPF", "Cuota IRPF",
                "Base RE", "% RE", "Cuota RE",
                $"NIF del {entidad}", $"Nombre del {entidad}"
            };
            if (hoja.Name.Contains("Pendientes"))
                columnas = columnas.Append("Archivo").ToArray();
            
            for (int i = 0; i < columnas.Length; i++)
            {
                var celda = hoja.Cell(1, i + 1);
                celda.Value = columnas[i];
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            return columnas.Length;
        }

        private void EscribirConceptosComunes(List<Factura> facturas,
            IXLWorksheet hoja, bool esGasto)
        {
            int columnasLength = EscribirCabecera(hoja, esGasto);

            for (int i = 0; i < facturas.Count; i++)
            {
                var f = facturas[i];
                int fila = i + 2;
                hoja.Cell(fila, ColNumeroFactura).Value = f.NumeroFactura;
                hoja.Cell(fila, ColFecha).Value = f.Fecha?.ToString("dd/MM/yyyy") ?? string.Empty;
                hoja.Cell(fila, ColFechaOperacion).Value = f.Fecha?.ToString("dd/MM/yyyy") ?? string.Empty;
                hoja.Cell(fila, ColBaseIVA).Value = f.BaseImponible;
                hoja.Cell(fila, ColPorcentajeIVA).Value = f.PorcentajeIVA;
                hoja.Cell(fila, ColCuotaIVA).Value = f.CuotaIVACalculado;
                hoja.Cell(fila, ColBaseIRPF).Value = f.BaseImponible;
                hoja.Cell(fila, ColPorcentajeIRPF).Value = f.PorcentajeIRPF;
                hoja.Cell(fila, ColCuotaIRPF).Value = f.CuotaIRPFCalculado;
                hoja.Cell(fila, ColBaseRE).Value = f.BaseImponible;
                hoja.Cell(fila, ColPorcentajeRE).Value = f.PorcentajeRE;
                hoja.Cell(fila, ColCuotaRE).Value = f.CuotaRECalculado;
                if (hoja.Name.Contains("Pendientes"))
                    hoja.Cell(fila, ColArchivo).Value = Path.GetFileName(f.RutaArchivo);

                AplicarFormatosIngresoGasto(hoja, fila);

                // Color según estado solo en la hoja de pendientes
                if (f.Estado != EstadoFactura.OK)
                    AplicarColorEstado(hoja, fila, columnasLength, f.Estado);
            }
        }

        private void AplicarFormatosIngresoGasto(IXLWorksheet hoja, int fila)
        {
            string fmtMoneda = "#,##0.00";
            string fmtNumero = "0.00";

            foreach (int col in new[] { ColBaseIVA, ColCuotaIVA, ColBaseIRPF, ColCuotaIRPF, ColBaseRE, ColCuotaRE })
                hoja.Cell(fila, col).Style.NumberFormat.Format = fmtMoneda;

            foreach (int col in new[] { ColPorcentajeIVA, ColPorcentajeIRPF, ColPorcentajeRE })
                hoja.Cell(fila, col).Style.NumberFormat.Format = fmtNumero;

            hoja.Cell(fila, ColConcepto).Style.NumberFormat.Format = "@";
        }

        private void AplicarColorEstado(IXLWorksheet hoja,
            int fila, int numColumnas, EstadoFactura estado)
        {
            hoja.Range(fila, 1, fila, numColumnas)
                .Style.Fill.BackgroundColor = XLColor.FromColor(estado.ToColor());
        }
    }
}