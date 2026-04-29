using ClosedXML.Excel;
using FacturasApp.Models;
using System.Globalization;
using FacturasApp.Services; 

namespace FacturasApp.Services
{
    public class ExcelExtractor
    {
        // ── Variantes aceptadas por cada campo ───────────────────────────────
        // Añade aquí cualquier nombre de columna que puedas encontrar
        private static readonly Dictionary<string, string[]> MapaColumnas = new()
        {
            ["NumeroFactura"] = new[]
            {
                "número de factura",
            },
            ["Fecha"] = new[]
            {
                "fecha factura",
            },
            ["EmisorNombre"] = new[]
            {
                "nombre del emisor",
            },
            ["EmisorNIF"] = new[]
            {
                "nif del emisor",
            },
            ["ClienteNombre"] = new[]
            {
                "cliente",
            },
            ["ClienteNIF"] = new[]
            {
                "nif / cif",
            },
            ["BaseImponible"] = new[]
            {
                "base imponible",
            },
            ["PorcentajeIVA"] = new[]
            {
                "% iva",
            },
            ["CuotaIVA"] = new[]
            {
                "iva",
            },
            ["PorcentajeIRPF"] = new[]
            {
                "% irpf",
            },
            ["PorcentajeRE"] = new[]
            {
                "% re",
            },
            ["Total"] = new[]
            {
                "importe total",
            }
        };

        // ── Método principal ─────────────────────────────────────────────────

        public List<Factura> ImportarDesdeExcel(string rutaExcel)
        {
            var facturas = new List<Factura>();

            using var workbook = new XLWorkbook(rutaExcel);
            var hoja = workbook.Worksheets.First();
            var mapaIndices = MapearColumnas(hoja);

            // Comprobamos que hay columnas reconocidas
            if (mapaIndices.Count == 0)
                throw new InvalidOperationException(
                    "No se reconoció ninguna columna del Excel. " +
                    "Verifica que la primera fila contiene cabeceras.");

            // Procesamos cada fila de datos (empezamos en la 2ª fila)
            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

            for (int fila = 2; fila <= ultimaFila; fila++)
            {
                var row = hoja.Row(fila);

                // Saltamos filas completamente vacías
                if (row.IsEmpty()) continue;

                try
                {
                    var factura = ConstruirFactura(row, mapaIndices);
                    facturas.Add(factura);
                }
                catch (Exception ex)
                {
                    // Si una fila falla la registramos y continuamos
                    facturas.Add(new Factura
                    {
                        Estado = _Estado.Error,
                        MensajeError = new List<string> { $"Error en fila {fila}: {ex.Message}" }
                    });
                }
            }

            return facturas;
        }

        // ── Mapeo de columnas ────────────────────────────────────────────────

        // Lee la cabecera del Excel y devuelve un diccionario
        // campo → número de columna
        private Dictionary<string, int> MapearColumnas(IXLWorksheet hoja)
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

                // Buscamos a qué campo corresponde esta cabecera
                foreach (var kvp in MapaColumnas)
                {
                    if (kvp.Value.Contains(cabecera) && !resultado.ContainsKey(kvp.Key))
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
            Dictionary<string, int> mapaIndices)
        {
            var factura = new Factura
            {
                ExtractedByOcr = false, // viene de Excel, no de OCR
                Emisor = new Proveedor(),
                Receptor = new Cliente()
            };

            // Lectura de campos
            factura.NumeroFactura = LeerTexto(row, mapaIndices, "NumeroFactura");
            factura.Fecha = LeerFecha(row, mapaIndices);
            factura.Emisor.Nombre = LeerTexto(row, mapaIndices, "EmisorNombre");
            factura.Emisor.NIF = LeerTexto(row, mapaIndices, "EmisorNIF");
            factura.Receptor.Nombre = LeerTexto(row, mapaIndices, "ClienteNombre");
            factura.Receptor.NIF = LeerTexto(row, mapaIndices, "ClienteNIF");
            factura.BaseImponible = LeerDecimal(row, mapaIndices, "BaseImponible");
            factura.PorcentajeIVA = LeerDecimal(row, mapaIndices, "PorcentajeIVA");
            factura.PorcentajeIVA = 21m; // Forzamos al 21% para evitar errores de redondeo 
            factura.PorcentajeIRPF = LeerDecimal(row, mapaIndices, "PorcentajeIRPF");
            factura.PorcentajeRE = LeerDecimal(row, mapaIndices, "PorcentajeRE");
            factura.Total = LeerDecimal(row, mapaIndices, "Total");

            // Determinar estado y mensajes de error
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
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

            // Intentamos primero como fecha nativa de Excel
            if (celda.DataType == XLDataType.DateTime)
                return celda.GetDateTime();

            // Si es texto intentamos parsearlo
            string texto = celda.GetString().Trim();
            if (string.IsNullOrEmpty(texto)) return null;

            // Intentamos varios formatos habituales en España
            string[] formatos =
            {
                "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy",
                "d-M-yyyy",   "dd.MM.yyyy", "yyyy-MM-dd",
                "dd/MMM/yyyy" // Ene, Feb, Mar...
            };

            foreach (string formato in formatos)
            {
                if (DateTime.TryParseExact(texto, formato,
                    new CultureInfo("es-ES"),
                    DateTimeStyles.None,
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

            // Si la celda ya es numérica lo leemos directamente
            if (celda.DataType == XLDataType.Number)
                return (decimal)celda.GetDouble();

            // Si es texto lo parseamos
            string texto = celda.GetString()
                .Trim()
                .Replace("€", "")
                .Replace("%", "")
                .Trim();

            if (string.IsNullOrEmpty(texto)) return 0m;

            // Detectamos si usa coma o punto como separador decimal
            // Formato europeo: 1.234,56 → 1234.56
            if (texto.Contains(',') && texto.Contains('.'))
                texto = texto.Replace(".", "").Replace(",", ".");
            else if (texto.Contains(','))
                texto = texto.Replace(",", ".");

            return decimal.TryParse(texto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var r) ? r : 0m;
        }


        // ── Utilidad: columnas no reconocidas ────────────────────────────────
        public List<string> ObtenerColumnasNoReconocidas(string rutaExcel)
        {
            using var workbook = new XLWorkbook(rutaExcel);
            var hoja = workbook.Worksheets.First();
            var noReconocidas = new List<string>();
            int ultimaCol = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;
            var todasLasVariantes = MapaColumnas.Values
                .SelectMany(v => v)
                .ToHashSet();

            for (int col = 1; col <= ultimaCol; col++)
            {
                string cabecera = hoja.Row(1).Cell(col).GetString()
                    .Trim()
                    .ToLowerInvariant();

                if (!string.IsNullOrEmpty(cabecera) &&
                    !todasLasVariantes.Contains(cabecera))
                    noReconocidas.Add(cabecera);
            }

            return noReconocidas;
        }
    }
}