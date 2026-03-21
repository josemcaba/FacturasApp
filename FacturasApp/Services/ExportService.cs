using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using FacturasApp.Models;
using System.Globalization;

namespace FacturasApp.Services
{
    public class ExportService
    {
        // ── Excel ────────────────────────────────────────────────────────────

        public void ExportarAExcel(List<Factura> facturas, string rutaDestino)
        {
            using var workbook = new XLWorkbook();

            CrearHojaFacturas(workbook, facturas);
            CrearHojaResumenEmisor(workbook, facturas);
            CrearHojaIncidencias(workbook, facturas);

            workbook.SaveAs(rutaDestino);
        }

        private void CrearHojaFacturas(XLWorkbook workbook, List<Factura> facturas)
        {
            var hoja = workbook.Worksheets.Add("Facturas");

            string[] columnas =
            {
                "Nº Factura", "Fecha", "Emisor", "NIF Emisor",
                "Cliente", "NIF Cliente", "Base Imponible",
                "% IVA", "Cuota IVA", "Total", "Estado",
                "Via OCR", "Archivo"
            };

            // ── Cabecera ──────────────────────────────────────────────────────
            for (int i = 0; i < columnas.Length; i++)
            {
                var celda = hoja.Cell(1, i + 1);
                celda.Value = columnas[i];
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
            }

            // ── Datos ─────────────────────────────────────────────────────────
            for (int i = 0; i < facturas.Count; i++)
            {
                var f = facturas[i];
                int fila = i + 2;

                hoja.Cell(fila, 1).Value = f.NumeroFactura;
                hoja.Cell(fila, 2).Value = f.Fecha.HasValue
                    ? f.Fecha.Value.ToString("dd/MM/yyyy") : "—";
                hoja.Cell(fila, 3).Value = f.Emisor.Nombre;
                hoja.Cell(fila, 4).Value = f.Emisor.NIF;
                hoja.Cell(fila, 5).Value = f.Receptor.Nombre;
                hoja.Cell(fila, 6).Value = f.Receptor.NIF;
                hoja.Cell(fila, 7).Value = f.BaseImponible;
                hoja.Cell(fila, 8).Value = f.PorcentajeIVA / 100;
                hoja.Cell(fila, 9).Value = f.CuotaIVA;
                hoja.Cell(fila, 10).Value = f.Total;
                hoja.Cell(fila, 11).Value = f.Estado.ToString();
                hoja.Cell(fila, 12).Value = f.ExtractedByOcr ? "Sí" : "No";
                hoja.Cell(fila, 13).Value = Path.GetFileName(f.RutaArchivo);

                // Formato numérico
                string fmtMoneda = "#,##0.00 €";
                hoja.Cell(fila, 7).Style.NumberFormat.Format = fmtMoneda;
                hoja.Cell(fila, 9).Style.NumberFormat.Format = fmtMoneda;
                hoja.Cell(fila, 10).Style.NumberFormat.Format = fmtMoneda;
                hoja.Cell(fila, 8).Style.NumberFormat.Format = "0%";

                // Color de fila según estado
                var color = f.Estado switch
                {
                    EstadoFactura.OK => XLColor.FromHtml("#E2EFDA"),
                    EstadoFactura.RevisionManual => XLColor.FromHtml("#FFF2CC"),
                    EstadoFactura.Error => XLColor.FromHtml("#FCE4D6"),
                    _ => XLColor.White
                };

                hoja.Range(fila, 1, fila, columnas.Length)
                    .Style.Fill.BackgroundColor = color;
            }

            // ── Fila de totales ───────────────────────────────────────────────
            int filaTotal = facturas.Count + 2;
            hoja.Cell(filaTotal, 6).Value = "TOTALES";
            hoja.Cell(filaTotal, 6).Style.Font.Bold = true;

            hoja.Cell(filaTotal, 7).FormulaA1 = $"=SUM(G2:G{filaTotal - 1})";
            hoja.Cell(filaTotal, 9).FormulaA1 = $"=SUM(I2:I{filaTotal - 1})";
            hoja.Cell(filaTotal, 10).FormulaA1 = $"=SUM(J2:J{filaTotal - 1})";

            foreach (int col in new[] { 7, 9, 10 })
            {
                hoja.Cell(filaTotal, col).Style.NumberFormat.Format = "#,##0.00 €";
                hoja.Cell(filaTotal, col).Style.Font.Bold = true;
                hoja.Cell(filaTotal, col).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#BDD7EE");
            }

            hoja.Columns().AdjustToContents();
            hoja.RangeUsed()!.SetAutoFilter();
        }

        private void CrearHojaResumenEmisor(XLWorkbook workbook,
            List<Factura> facturas)
        {
            var hoja = workbook.Worksheets.Add("Resumen por Emisor");

            string[] columnas =
                { "Emisor", "NIF", "Nº Facturas", "Base Total", "IVA Total", "Total" };

            for (int i = 0; i < columnas.Length; i++)
            {
                var celda = hoja.Cell(1, i + 1);
                celda.Value = columnas[i];
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
                celda.Style.Font.FontColor = XLColor.White;
            }

            var resumen = facturas
                .Where(f => f.Estado != EstadoFactura.Error)
                .GroupBy(f => f.Emisor.Nombre)
                .Select(g => new
                {
                    Emisor = g.Key,
                    NIF = g.First().Emisor.NIF,
                    Cantidad = g.Count(),
                    BaseTotal = g.Sum(f => f.BaseImponible),
                    IvaTotal = g.Sum(f => f.CuotaIVA),
                    Total = g.Sum(f => f.Total)
                })
                .OrderByDescending(r => r.Total)
                .ToList();

            for (int i = 0; i < resumen.Count; i++)
            {
                int fila = i + 2;
                var r = resumen[i];

                hoja.Cell(fila, 1).Value = r.Emisor;
                hoja.Cell(fila, 2).Value = r.NIF;
                hoja.Cell(fila, 3).Value = r.Cantidad;
                hoja.Cell(fila, 4).Value = r.BaseTotal;
                hoja.Cell(fila, 5).Value = r.IvaTotal;
                hoja.Cell(fila, 6).Value = r.Total;

                string fmtMoneda = "#,##0.00 €";
                hoja.Cell(fila, 4).Style.NumberFormat.Format = fmtMoneda;
                hoja.Cell(fila, 5).Style.NumberFormat.Format = fmtMoneda;
                hoja.Cell(fila, 6).Style.NumberFormat.Format = fmtMoneda;
            }

            hoja.Columns().AdjustToContents();
        }

        private void CrearHojaIncidencias(XLWorkbook workbook,
            List<Factura> facturas)
        {
            var incidencias = facturas
                .Where(f => f.Estado is EstadoFactura.Error
                                     or EstadoFactura.RevisionManual)
                .ToList();

            if (incidencias.Count == 0) return;

            var hoja = workbook.Worksheets.Add("Incidencias");

            string[] columnas =
                { "Archivo", "Estado", "Nº Factura", "Fecha", "Emisor", "Motivo" };

            for (int i = 0; i < columnas.Length; i++)
            {
                var celda = hoja.Cell(1, i + 1);
                celda.Value = columnas[i];
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#C00000");
                celda.Style.Font.FontColor = XLColor.White;
            }

            for (int i = 0; i < incidencias.Count; i++)
            {
                var f = incidencias[i];
                int fila = i + 2;

                hoja.Cell(fila, 1).Value = Path.GetFileName(f.RutaArchivo);
                hoja.Cell(fila, 2).Value = f.Estado.ToString();
                hoja.Cell(fila, 3).Value = f.NumeroFactura;
                hoja.Cell(fila, 4).Value = f.Fecha?.ToString("dd/MM/yyyy") ?? "—";
                hoja.Cell(fila, 5).Value = f.Emisor.Nombre;
                hoja.Cell(fila, 6).Value = f.ErrorMensaje ?? "Campos incompletos";
            }

            hoja.Columns().AdjustToContents();
        }

        // ── CSV ──────────────────────────────────────────────────────────────

        public void ExportarACsv(List<Factura> facturas, string rutaDestino)
        {
            var config = new CsvConfiguration(new CultureInfo("es-ES"))
            {
                Delimiter = ";",
                Encoding = System.Text.Encoding.UTF8
            };

            using var writer = new StreamWriter(rutaDestino, false,
                System.Text.Encoding.UTF8);
            using var csv = new CsvWriter(writer, config);

            csv.Context.RegisterClassMap<FacturaCsvMap>();
            csv.WriteRecords(facturas);
        }
    }

    // ── Mapeado CSV ───────────────────────────────────────────────────────────

    public class FacturaCsvMap : ClassMap<Factura>
    {
        public FacturaCsvMap()
        {
            Map(f => f.NumeroFactura).Name("Nº Factura");
            Map(f => f.Fecha).Name("Fecha")
                .TypeConverterOption.Format("dd/MM/yyyy");
            Map(f => f.Emisor.Nombre).Name("Emisor");
            Map(f => f.Emisor.NIF).Name("NIF Emisor");
            Map(f => f.Receptor.Nombre).Name("Cliente");
            Map(f => f.Receptor.NIF).Name("NIF Cliente");
            Map(f => f.BaseImponible).Name("Base Imponible");
            Map(f => f.PorcentajeIVA).Name("% IVA");
            Map(f => f.CuotaIVA).Name("Cuota IVA");
            Map(f => f.Total).Name("Total");
            Map(f => f.Estado).Name("Estado");
            Map(f => f.ExtractedByOcr).Name("Via OCR");
            Map(f => f.RutaArchivo).Name("Archivo")
                .Convert(f => Path.GetFileName(f.Value.RutaArchivo));
        }
    }
}