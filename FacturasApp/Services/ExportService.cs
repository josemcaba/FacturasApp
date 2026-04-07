using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using FacturasApp.Models;
using System.Globalization;

namespace FacturasApp.Services
{
    public class ExportService
    {
        // ── Excel ────────────────────────────────────────────────────────────

        public void ExportarAExcelIngresos(List<Factura> facturas, string rutaDestino)
        {
            using var workbook = new XLWorkbook();

            var correctas = facturas.Where(f => f.Estado == EstadoFactura.OK).ToList();
            var incorrectas = facturas.Where(f => f.Estado != EstadoFactura.OK).ToList();

            CrearHojaIngresos(workbook, correctas, "Ingresos Correctos");
            CrearHojaIngresos(workbook, incorrectas, "Ingresos Pendientes");

            workbook.SaveAs(rutaDestino);
        }

        public void ExportarAExcelGastos(List<Factura> facturas, string rutaDestino)
        {
            using var workbook = new XLWorkbook();

            var correctas = facturas.Where(f => f.Estado == EstadoFactura.OK).ToList();
            var incorrectas = facturas.Where(f => f.Estado != EstadoFactura.OK).ToList();

            CrearHojaGastos(workbook, correctas, "Gastos Correctos");
            CrearHojaGastos(workbook, incorrectas, "Gastos Pendientes");

            workbook.SaveAs(rutaDestino);
        }

        // ── Métodos privados de creación de hojas ────────────────────────────────────

        private void CrearHojaIngresos(XLWorkbook workbook,
            List<Factura> facturas, string nombreHoja)
        {
            var hoja = workbook.Worksheets.Add(nombreHoja);

            EscribirConceptosComunes(facturas, hoja);

            for (int i = 0; i < facturas.Count; i++)
            {
                var f = facturas[i];
                int fila = i + 2;

                hoja.Cell(fila, 4).Value = "700";
                hoja.Cell(fila, 14).Value = f.Receptor.NIF;
                hoja.Cell(fila, 15).Value = f.Receptor.Nombre;
            }

            hoja.Columns().AdjustToContents();
        }

        private void CrearHojaGastos(XLWorkbook workbook,
            List<Factura> facturas, string nombreHoja)
        {
            var hoja = workbook.Worksheets.Add(nombreHoja);

            EscribirConceptosComunes(facturas, hoja);

            for (int i = 0; i < facturas.Count; i++)
            {
                var f = facturas[i];
                int fila = i + 2;

                hoja.Cell(fila, 4).Value = !string.IsNullOrEmpty(f.Concepto) && f.Concepto != "0"
                                            ? f.Concepto : "600";
                hoja.Cell(fila, 14).Value = f.Emisor.NIF;
                hoja.Cell(fila, 15).Value = f.Emisor.Nombre;
            }

            hoja.Columns().AdjustToContents();
        }

        // ── Helpers compartidos ───────────────────────────────────────────────────────

        private int EscribirCabecera(IXLWorksheet hoja)
        {
            string[] columnas =
{
                "Número de factura", "Fecha de factura", "Fecha de operación",
                "Concepto", "Base IVA", "% IVA", "Cuota IVA",
                "Base IRPF", "% IRPF", "Cuota IRPF",
                "Base RE", "% RE", "Cuota RE",
                "NIF del Cliente", "Nombre del Cliente"
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
            IXLWorksheet hoja)
        {
            int columnasLength = EscribirCabecera(hoja);

            for (int i = 0; i < facturas.Count; i++)
            {
                var f = facturas[i];
                int fila = i + 2;
                hoja.Cell(fila, 1).Value = f.NumeroFactura;
                hoja.Cell(fila, 2).Value = f.Fecha?.ToString("dd/MM/yyyy") ?? string.Empty;
                hoja.Cell(fila, 3).Value = f.Fecha?.ToString("dd/MM/yyyy") ?? string.Empty;
                hoja.Cell(fila, 5).Value = f.BaseImponible;
                hoja.Cell(fila, 6).Value = f.PorcentajeIVA;
                hoja.Cell(fila, 7).Value = f.CuotaIVA;
                hoja.Cell(fila, 8).Value = f.BaseImponible;
                hoja.Cell(fila, 9).Value = f.PorcentajeIRPF;
                hoja.Cell(fila, 10).Value = f.CuotaIRPF;
                hoja.Cell(fila, 11).Value = f.BaseImponible;
                hoja.Cell(fila, 12).Value = f.PorcentajeRE;
                hoja.Cell(fila, 13).Value = f.CuotaRE;
                if (hoja.Name.Contains("Pendientes"))
                    hoja.Cell(fila, 16).Value = Path.GetFileName(f.RutaArchivo);

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

            foreach (int col in new[] { 5, 7, 8, 10, 11, 13 })
                hoja.Cell(fila, col).Style.NumberFormat.Format = fmtMoneda;

            foreach (int col in new[] { 6, 9, 12 })
                hoja.Cell(fila, col).Style.NumberFormat.Format = fmtNumero;

            hoja.Cell(fila, 4).Style.NumberFormat.Format = "@";
        }

        private void AplicarColorEstado(IXLWorksheet hoja,
            int fila, int numColumnas, EstadoFactura estado)
        {
            var color = estado switch
            {
                EstadoFactura.RevisionManual => XLColor.FromHtml("#FFF2CC"),
                EstadoFactura.Error => XLColor.FromHtml("#FCE4D6"),
                _ => XLColor.White
            };

            hoja.Range(fila, 1, fila, numColumnas)
                .Style.Fill.BackgroundColor = color;
        }
    }
}