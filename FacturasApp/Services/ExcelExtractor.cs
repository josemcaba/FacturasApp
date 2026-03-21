using ClosedXML.Excel;
using FacturasApp.Models;

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
                "número de factura", "numero de factura", "nº factura",
                "n factura", "factura", "fra", "nº", "numero"
            },
            ["Fecha"] = new[]
            {
                "fecha", "fecha factura", "date", "fecha emisión", "fecha emision"
            },
            ["EmisorNombre"] = new[]
            {
                "nombre del emisor", "emisor", "proveedor", "nombre proveedor",
                "razón social", "razon social", "nombre emisor"
            },
            ["EmisorNIF"] = new[]
            {
                "nif del emisor", "nif emisor", "cif emisor", "nif proveedor",
                "cif proveedor", "nif/cif emisor"
            },
            ["ClienteNombre"] = new[]
            {
                "nombre del cliente", "cliente", "nombre cliente",
                "receptor", "nombre receptor"
            },
            ["ClienteNIF"] = new[]
            {
                "nif del cliente", "nif cliente", "cif cliente",
                "nif receptor", "nif/cif cliente"
            },
            ["BaseImponible"] = new[]
            {
                "base imponible", "base", "subtotal", "base imp",
                "importe base", "base imponible €"
            },
            ["PorcentajeIVA"] = new[]
            {
                "% iva", "iva %", "tipo iva", "porcentaje iva",
                "% impuesto", "iva porcentaje"
            },
            ["CuotaIVA"] = new[]
            {
                "cuota iva", "importe iva", "iva", "iva €",
                "cuota impuesto", "importe impuesto"
            },
            ["Total"] = new[]
            {
                "total", "total factura", "importe total", "total €",
                "total a pagar", "importe"
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
                        Estado = EstadoFactura.Error,
                        ErrorMensaje = $"Fila {fila}: {ex.Message}"
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

            factura.NumeroFactura = LeerTexto(row, mapaIndices, "NumeroFactura");
            factura.Fecha = LeerFecha(row, mapaIndices);
            factura.Emisor.Nombre = LeerTexto(row, mapaIndices, "EmisorNombre");
            factura.Emisor.NIF = LeerTexto(row, mapaIndices, "EmisorNIF");
            factura.Receptor.Nombre = LeerTexto(row, mapaIndices, "ClienteNombre");
            factura.Receptor.NIF = LeerTexto(row, mapaIndices, "ClienteNIF");
            factura.BaseImponible = LeerDecimal(row, mapaIndices, "BaseImponible");
            factura.PorcentajeIVA = LeerDecimal(row, mapaIndices, "PorcentajeIVA");
            factura.TotalExtraido = LeerDecimal(row, mapaIndices, "Total");

            // Si el Excel no trae CuotaIVA pero sí Base y %, la calculamos
            if (factura.CuotaIVA == 0 &&
                factura.BaseImponible > 0 && factura.PorcentajeIVA > 0)
            {
                // CuotaIVA es una propiedad calculada en el modelo,
                // no hace falta asignarla
            }

            factura.Estado = DeterminarEstado(factura);
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
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var r) ? r : 0m;
        }

        // ── Estado ───────────────────────────────────────────────────────────

        private EstadoFactura DeterminarEstado(Factura f)
        {
            bool camposCriticos = !string.IsNullOrEmpty(f.NumeroFactura)
                               && f.Fecha.HasValue
                               && f.TotalExtraido > 0;

            bool camposSecundarios = !string.IsNullOrEmpty(f.Emisor.NIF)
                                  && f.BaseImponible > 0;

            if (camposCriticos && camposSecundarios) return EstadoFactura.OK;
            if (camposCriticos) return EstadoFactura.RevisionManual;
            return EstadoFactura.RevisionManual;
        }

        // ── Utilidad: columnas no reconocidas ────────────────────────────────

        // Devuelve las cabeceras del Excel que no se han podido mapear
        // Útil para mostrar al usuario qué columnas se ignoraron
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
