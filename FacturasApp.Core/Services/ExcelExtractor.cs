using ClosedXML.Excel;
using FacturasApp.Core.Models;
using System.Globalization;

namespace FacturasApp.Core.Services
{
    public class ExcelExtractor
    {
        private readonly ConfiguracionColumnasExcel _configuracion = new();

        // ── Método principal ─────────────────────────────────────────────────
        public List<Factura> ImportarDesdeExcel(string rutaExcel)
        {
            var config = _configuracion.Cargar();
            var mapa = ConfiguracionColumnasExcel.ToMapa(config);

            var facturas = new List<Factura>();

            using var workbook = new XLWorkbook(rutaExcel);
            var hoja = workbook.Worksheets.First();
            var mapaIndices = MapearColumnas(hoja, mapa);

            if (mapaIndices.Count == 0)
                throw new InvalidOperationException(
                    "No se reconoció ninguna columna del Excel. " +
                    "Verifica que la primera fila contiene cabeceras.");

            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

            for (int fila = 2; fila <= ultimaFila; fila++)
            {
                var row = hoja.Row(fila);
                if (row.IsEmpty()) continue;

                try
                {
                    var factura = ConstruirFactura(row, mapaIndices, rutaExcel, fila, mapa);
                    facturas.Add(factura);
                }
                catch (Exception ex)
                {
                    facturas.Add(new Factura
                    {
                        Estado = EstadoFactura.Error,
                        MensajeError = new List<string> { $"Error en fila {fila}: {ex.Message}" }
                    });
                }
            }

            return facturas;
        }

        // ── Mapeo de columnas ────────────────────────────────────────────────
        private Dictionary<string, int> MapearColumnas(IXLWorksheet hoja, Dictionary<string, string[]> mapa)
        {
            var resultado = new Dictionary<string, int>();
            var filaHeader = hoja.Row(1);
            int ultimaCol = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;

            for (int col = 1; col <= ultimaCol; col++)
            {
                string cabecera = filaHeader.Cell(col).GetString()
                    .Trim()
                    .ToLowerInvariant();

                if (string.IsNullOrEmpty(cabecera)) continue;

                foreach (var kvp in mapa)
                {
                    if (kvp.Value.Contains(cabecera, StringComparer.OrdinalIgnoreCase)
                        && !resultado.ContainsKey(kvp.Key))
                    {
                        resultado[kvp.Key] = col;
                        break;
                    }
                }
            }

            return resultado;
        }

        // ── Construcción de la factura ───────────────────────────────────────
        private Factura ConstruirFactura(IXLRow row,
            Dictionary<string, int> mapaIndices, string rutaExcel, int numFila,
            Dictionary<string, string[]> mapa)
        {
            var factura = new Factura
            {
                ExtractedByOcr = false,
                Emisor = new Empresa(),
                Receptor = new Empresa(),
                RutaArchivo = $"{rutaExcel} (fila {numFila})"
            };

            factura.NumeroFactura = LeerTexto(row, mapaIndices, "NumeroFactura");
            factura.Fecha = LeerFecha(row, mapaIndices);
            factura.Emisor.Nombre = LeerTexto(row, mapaIndices, "EmisorNombre");
            if (string.IsNullOrEmpty(factura.Emisor.Nombre))
                factura.Emisor.Nombre = "Emisor";
            factura.Emisor.NIF = LeerTexto(row, mapaIndices, "EmisorNIF");
            if (string.IsNullOrEmpty(factura.Emisor.NIF))
                factura.Emisor.NIF = "99999999R";
            factura.Receptor.Nombre = LeerTexto(row, mapaIndices, "ClienteNombre");
            factura.Receptor.NIF = LeerTexto(row, mapaIndices, "ClienteNIF");
            factura.BaseImponible = LeerDecimal(row, mapaIndices, "BaseImponible");
            factura.CuotaIVA = LeerDecimal(row, mapaIndices, "CuotaIVA");
            factura.PorcentajeIVA = LeerDecimal(row, mapaIndices, "PorcentajeIVA");
            if (factura.PorcentajeIVA == 0m && factura.CuotaIVA != 0m)
                factura.PorcentajeIVA = 21m;
            factura.PorcentajeIRPF = LeerDecimal(row, mapaIndices, "PorcentajeIRPF");
            factura.CuotaIRPF = LeerDecimal(row, mapaIndices, "CuotaIRPF");
            factura.PorcentajeRE = LeerDecimal(row, mapaIndices, "PorcentajeRE");
            factura.CuotaRE = LeerDecimal(row, mapaIndices, "CuotaRE");
            factura.TotalFactura = LeerDecimal(row, mapaIndices, "TotalFactura");

            if (string.IsNullOrEmpty(factura.NumeroFactura))
                factura.MensajeError.Add("Falta número de factura");
            if (factura.BaseImponible == 0m && factura.TotalFactura == 0m)
                factura.MensajeError.Add("Faltan base imponible y total");

            factura.Estado = FacturaEstado.Determinar(factura);
            return factura;
        }

        // ── Lectores de celda ────────────────────────────────────────────────
        private string LeerTexto(IXLRow row, Dictionary<string, int> mapa, string campo)
        {
            if (!mapa.TryGetValue(campo, out int col)) return string.Empty;
            return row.Cell(col).GetString().Trim();
        }

        private DateTime? LeerFecha(IXLRow row, Dictionary<string, int> mapa)
        {
            if (!mapa.TryGetValue("Fecha", out int col)) return null;
            var celda = row.Cell(col);
            if (celda.DataType == XLDataType.DateTime)
                return celda.GetDateTime();

            string texto = celda.GetString().Trim();
            if (string.IsNullOrEmpty(texto)) return null;
            if (DateTime.TryParse(texto, new CultureInfo("es-ES"), DateTimeStyles.None, out DateTime fecha))
                return fecha;
            return null;
        }

        private decimal LeerDecimal(IXLRow row, Dictionary<string, int> mapa, string campo)
        {
            if (!mapa.TryGetValue(campo, out int col)) return 0m;
            var celda = row.Cell(col);
            if (celda.DataType == XLDataType.Number)
                return (decimal)celda.GetDouble();

            string texto = celda.GetString().Trim().Replace("€", "").Replace("%", "").Trim();
            if (string.IsNullOrEmpty(texto)) return 0m;

            if (texto.Contains(',') && texto.Contains('.'))
                texto = texto.Replace(".", "").Replace(",", ".");
            else if (texto.Contains(','))
                texto = texto.Replace(",", ".");

            return decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : 0m;
        }

        // ── Utilidad: columnas no reconocidas ────────────────────────────────
        public List<string> ObtenerColumnasNoReconocidas(string rutaExcel)
        {
            var config = _configuracion.Cargar();
            var mapa = ConfiguracionColumnasExcel.ToMapa(config);
            return ConfiguracionColumnasExcel.ObtenerColumnasNoReconocidas(rutaExcel, mapa);
        }
    }
}