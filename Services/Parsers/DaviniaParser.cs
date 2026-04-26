using ClosedXML.Excel;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class DaviniaParser : IExcelInvoiceParser
    {
        public string Nombre => "Davinia Castillo";

        // Identifica el archivo por el nombre o por contenido de la cabecera
        public bool PuedeParsar(string rutaExcel)
        {
            // Primero comprobamos el nombre del archivo
            string nombreArchivo = Path.GetFileNameWithoutExtension(rutaExcel)
                .ToLowerInvariant();

            if (nombreArchivo.Contains("davinia") ||
                nombreArchivo.Contains("castillo"))
                return true;

            // Si el nombre no es suficiente, miramos dentro del Excel
            try
            {
                using var workbook = new XLWorkbook(rutaExcel);
                var hoja = workbook.Worksheets.First();

                // Buscamos "Davinia" o "Castillo" en las primeras filas
                for (int fila = 1; fila <= 3; fila++)
                {
                    string contenido = hoja.Row(fila)
                        .Cells()
                        .Select(c => c.GetString())
                        .Aggregate((a, b) => $"{a} {b}")
                        .ToLowerInvariant();

                    if (contenido.Contains("davinia") ||
                        contenido.Contains("castillo"))
                        return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // ── Mapa de columnas específico de Davinia ───────────────────────────
        // Ajusta los nombres según el Excel real que ella proporciona
        private static readonly Dictionary<string, string[]> MapaColumnas = new()
        {
            ["NumeroFactura"] = new[]
            {
                "número", "nº", "factura", "numero factura",
                "nº factura", "número factura"
            },
            ["Fecha"] = new[]
            {
                "fecha", "date", "fecha factura", "fecha emisión"
            },
            ["EmisorNombre"] = new[]
            {
                "emisor", "proveedor", "nombre", "nombre emisor",
                "davinia castillo", "razón social"
            },
            ["EmisorNIF"] = new[]
            {
                "nif", "cif", "nif emisor", "cif emisor"
            },
            ["ClienteNombre"] = new[]
            {
                "cliente", "nombre cliente", "receptor", "destinatario"
            },
            ["ClienteNIF"] = new[]
            {
                "nif / cif", "cif cliente", "nif receptor"
            },
            ["BaseImponible"] = new[]
            {
                "importe total sin iva", "base imponible", "importe base"
            },
            ["PorcentajeIVA"] = new[]
            {
                "iva %", "% iva", "tipo iva", "iva"
            },
            ["CuotaIVA"] = new[]
            {
                "cuota iva", "importe iva", "iva €"
            },
            ["Total"] = new[]
            {
                "total", "importe total", "total factura", "total €"
            }
        };

        public List<Factura> Parsear(string rutaExcel)
        {
            var facturas = new List<Factura>();

            using var workbook = new XLWorkbook(rutaExcel);
            var hoja = workbook.Worksheets.First();
            var mapaIndices = MapearColumnas(hoja);

            if (mapaIndices.Count == 0)
                throw new InvalidOperationException(
                    $"No se reconoció ninguna columna en el Excel de {Nombre}.");

            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

            for (int fila = 2; fila <= ultimaFila; fila++)
            {
                var row = hoja.Row(fila);
                if (row.IsEmpty()) continue;

                try
                {
                    var factura = ConstruirFactura(row, mapaIndices, rutaExcel);
                    facturas.Add(factura);
                }
                catch (Exception ex)
                {
                    facturas.Add(new Factura
                    {
                        RutaArchivo = rutaExcel,
                        Estado = EstadoFactura.Error,
                        ErrorMensaje = $"Fila {fila}: {ex.Message}"
                    });
                }
            }

            return facturas;
        }

        // ── Construcción de factura ──────────────────────────────────────────

        private Factura ConstruirFactura(IXLRow row,
            Dictionary<string, int> mapa, string rutaExcel)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaExcel,
                ExtractedByOcr = false,
                Emisor = new Proveedor
                {
                    // El emisor siempre es Davinia, aunque no esté en el Excel
                    Nombre = LeerTexto(row, mapa, "EmisorNombre")
                             is { Length: > 0 } nombre
                             ? nombre
                             : "Davinia Castillo",
                    NIF = LeerTexto(row, mapa, "EmisorNIF")
                },
                Receptor = new Cliente
                {
                    Nombre = LeerTexto(row, mapa, "ClienteNombre"),
                    NIF = LeerTexto(row, mapa, "ClienteNIF")
                }
            };

            factura.NumeroFactura = LeerTexto(row, mapa, "NumeroFactura");
            factura.Fecha = LeerFecha(row, mapa);
            factura.BaseImponible = LeerDecimal(row, mapa, "BaseImponible");
            factura.PorcentajeIVA = 21.0m;
            factura.Total = LeerDecimal(row, mapa, "Total");
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }

        // ── Mapeado de columnas ──────────────────────────────────────────────

        private Dictionary<string, int> MapearColumnas(IXLWorksheet hoja)
        {
            var resultado = new Dictionary<string, int>();
            int ultimaCol = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;

            for (int col = 1; col <= ultimaCol; col++)
            {
                string cabecera = hoja.Row(1).Cell(col).GetString()
                    .Trim()
                    .ToLowerInvariant();

                if (string.IsNullOrEmpty(cabecera)) continue;

                foreach (var kvp in MapaColumnas)
                {
                    if (kvp.Value.Contains(cabecera) &&
                        !resultado.ContainsKey(kvp.Key))
                    {
                        resultado[kvp.Key] = col;
                        break;
                    }
                }
            }

            return resultado;
        }

        // ── Lectores de celda ────────────────────────────────────────────────

        private string LeerTexto(IXLRow row,
            Dictionary<string, int> mapa, string campo)
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

            string[] formatos =
            {
                "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy",
                "d-M-yyyy",   "dd.MM.yyyy", "yyyy-MM-dd"
            };

            foreach (string formato in formatos)
            {
                if (DateTime.TryParseExact(texto, formato,
                    new System.Globalization.CultureInfo("es-ES"),
                    System.Globalization.DateTimeStyles.None,
                    out DateTime fecha))
                    return fecha;
            }

            return null;
        }

        private decimal LeerDecimal(IXLRow row,
            Dictionary<string, int> mapa, string campo)
        {
            if (!mapa.TryGetValue(campo, out int col)) return 0m;

            var celda = row.Cell(col);

            if (celda.DataType == XLDataType.Number)
                return (decimal)celda.GetDouble();

            string texto = celda.GetString()
                .Trim()
                .Replace("€", "")
                .Replace("%", "")
                .Trim();

            if (string.IsNullOrEmpty(texto)) return 0m;

            if (texto.Contains(',') && texto.Contains('.'))
                texto = texto.Replace(".", "").Replace(",", ".");
            else if (texto.Contains(','))
                texto = texto.Replace(",", ".");

            return decimal.TryParse(texto,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var r) ? r : 0m;
        }

        private EstadoFactura DeterminarEstado(Factura f) =>
            !string.IsNullOrEmpty(f.NumeroFactura) && f.Fecha.HasValue && f.Total > 0
                ? EstadoFactura.OK
                : EstadoFactura.RevisionManual;
    }
}
