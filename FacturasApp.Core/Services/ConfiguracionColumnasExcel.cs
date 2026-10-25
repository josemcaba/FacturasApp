using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services
{
    public class ConfiguracionColumnasExcel
    {
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FacturasApp", "Emisores");

        private const string NombreArchivo = "ColumnasExcel.xml";
        private const string NombreSentinel = "ColumnasExcel.eliminado";

        private readonly XmlSerializer _serializer = new(typeof(ColumnasExcelConfig));

        private static readonly string[] CamposFijos =
        {
            "NumeroFactura", "Fecha", "EmisorNombre", "EmisorNIF",
            "ClienteNombre", "ClienteNIF", "BaseImponible", "PorcentajeIVA",
            "CuotaIVA", "PorcentajeIRPF", "CuotaIRPF", "PorcentajeRE",
            "CuotaRE", "TotalFactura", "Concepto"
        };

        private static readonly Dictionary<string, string[]> Defaults = new()
        {
            ["NumeroFactura"] = new[] { "número factura", "número de factura", "numero factura", "numero de factura", "nº factura", "nº de factura", "n° factura" },
            ["Fecha"] = new[] { "fecha factura", "fecha de factura", "fecha" },
            ["EmisorNombre"] = new[] { "emisor", "nombre emisor", "nif del emisor", "nombre del emisor" },
            ["EmisorNIF"] = new[] { "cif emisor", "nif emisor", "cif del emisor" },
            ["ClienteNombre"] = new[] { "cliente", "nombre cliente", "nombre del cliente" },
            ["ClienteNIF"] = new[] { "cif", "nif / cif", "nif cliente", "cif cliente", "nif del cliente" },
            ["BaseImponible"] = new[] { "base", "base imponible", "base iva", "base irpf", "base re" },
            ["PorcentajeIVA"] = new[] { "tipo", "% iva", "iva %" },
            ["CuotaIVA"] = new[] { "iva", "cuota iva" },
            ["PorcentajeIRPF"] = new[] { "irpf %", "% irpf" },
            ["CuotaIRPF"] = new[] { "irpf", "cuota irpf" },
            ["PorcentajeRE"] = new[] { "re %", "% re" },
            ["CuotaRE"] = new[] { "re", "cuota re" },
            ["TotalFactura"] = new[] { "total", "importe total" },
            ["Concepto"] = new[] { "concepto" },
        };

        public string RutaArchivo => Path.Combine(RutaDirectorio, NombreArchivo);

        /// <summary>
        /// Asegura que el XML esté extraído (idempotente) y lo carga.
        /// Si falta o está corrupto, devuelve los valores por defecto.
        /// </summary>
        public ColumnasExcelConfig Cargar()
        {
            try
            {
                // Extrae el recurso embebido si procede (misma regla que los emisores).
                new ConfiguracionEmisores().Extraer(RutaDirectorio);
            }
            catch
            {
                // Si la extracción falla, seguimos intentando leer lo que haya.
            }

            Directory.CreateDirectory(RutaDirectorio);

            var ruta = RutaArchivo;
            if (!File.Exists(ruta))
            {
                var config = CrearPorDefecto();
                Guardar(config);
                return config;
            }

            try
            {
                using var stream = File.OpenRead(ruta);
                if (_serializer.Deserialize(stream) is ColumnasExcelConfig c)
                {
                    // Fusión: aseguramos que todos los campos fijos existan.
                    var existentes = new HashSet<string>(c.Columnas.Select(x => x.Campo), StringComparer.OrdinalIgnoreCase);
                    foreach (var campo in CamposFijos)
                    {
                        if (!existentes.Contains(campo))
                        {
                            c.Columnas.Add(new ColumnaExcelConfig
                            {
                                Campo = campo,
                                Cabeceras = Defaults.TryGetValue(campo, out var v)
                                    ? v.ToList()
                                    : new List<string>()
                            });
                        }
                    }
                    // Normalizamos las cabeceras.
                    foreach (var col in c.Columnas)
                    {
                        col.Cabeceras = col.Cabeceras
                            .Select(x => (x ?? "").Trim().ToLowerInvariant())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Distinct()
                            .ToList();
                    }
                    return c;
                }
            }
            catch
            {
                // XML corrupto: restablecemos defaults.
            }

            var porDefecto = CrearPorDefecto();
            Guardar(porDefecto);
            return porDefecto;
        }

        public void Guardar(ColumnasExcelConfig config)
        {
            Directory.CreateDirectory(RutaDirectorio);
            var ruta = RutaArchivo;

            var enDisco = File.Exists(ruta) ? File.ReadAllText(ruta) : null;
            var serializado = Serializar(config);
            var haCambiado = enDisco == null
                || ConfiguracionEmisores.ContenidoSinVersion(serializado)
                    != ConfiguracionEmisores.ContenidoSinVersion(enDisco);

            if (haCambiado)
            {
                var ahora = long.Parse(DateTime.Now.ToString("yyyyMMddHHmm"));
                config.Version = Math.Max(ahora, config.Version + 1);
                serializado = Serializar(config);
            }

            File.WriteAllText(ruta, serializado);
        }

        public static Dictionary<string, string[]> ToMapa(ColumnasExcelConfig config)
        {
            var resultado = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var col in config.Columnas)
            {
                if (string.IsNullOrWhiteSpace(col.Campo)) continue;
                resultado[col.Campo] = col.Cabeceras.ToArray();
            }
            return resultado;
        }

        public static List<string> ObtenerColumnasNoReconocidas(string rutaExcel, Dictionary<string, string[]> mapa)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook(rutaExcel);
            var hoja = workbook.Worksheets.First();
            var noReconocidas = new List<string>();
            int ultimaCol = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;
            var todasLasVariantes = mapa.Values
                .SelectMany(v => v)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int col = 1; col <= ultimaCol; col++)
            {
                string cabecera = hoja.Row(1).Cell(col).GetString()
                    .Trim()
                    .ToLowerInvariant();
                if (!string.IsNullOrEmpty(cabecera) && !todasLasVariantes.Contains(cabecera))
                    noReconocidas.Add(cabecera);
            }
            return noReconocidas;
        }

        private ColumnasExcelConfig CrearPorDefecto()
        {
            var config = new ColumnasExcelConfig { Version = 1 };
            foreach (var campo in CamposFijos)
            {
                config.Columnas.Add(new ColumnaExcelConfig
                {
                    Campo = campo,
                    Cabeceras = Defaults.TryGetValue(campo, out var v)
                        ? v.ToList()
                        : new List<string>()
                });
            }
            return config;
        }

        private string Serializar(ColumnasExcelConfig config)
        {
            using var memoria = new MemoryStream();
            _serializer.Serialize(memoria, config);
            return Encoding.UTF8.GetString(memoria.ToArray());
        }
    }
}