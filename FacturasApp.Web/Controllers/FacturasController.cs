using ClosedXML.Excel;
using FacturasApp.Core.Models;
using FacturasApp.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacturasApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturasController : ControllerBase
    {
        private readonly InvoiceProcessorService _processor;
        private readonly IWebHostEnvironment _env;

        // Almacén en memoria (solo usuario local)
        private static readonly List<Factura> _facturas = new();
        private static readonly object _lock = new();

        public FacturasController(InvoiceProcessorService processor, IWebHostEnvironment env)
        {
            _processor = processor;
            _env = env;
        }

        // ── Subir archivos (PDFs + Excel) ────────────────────────────────

        [HttpPost("upload")]
        public async Task<ActionResult<UploadResult>> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No se enviaron archivos");

            var rutasTemporales = new List<string>();
            string? rutaExcel = null;
            string directorioTemporal = Path.Combine(_env.ContentRootPath, "temp_uploads");
            Directory.CreateDirectory(directorioTemporal);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                bool esPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
                bool esExcel = file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                            || file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);

                if (!esPdf && !esExcel) continue;

                string extension = esExcel ? ".xlsx" : ".pdf";
                string ruta = Path.Combine(directorioTemporal, $"{Guid.NewGuid()}{extension}");
                using var stream = new FileStream(ruta, FileMode.Create);
                await file.CopyToAsync(stream);

                if (esExcel)
                    rutaExcel = ruta;
                else
                    rutasTemporales.Add(ruta);
            }

            return Ok(new UploadResult
            {
                ArchivosRecibidos = rutasTemporales.Count + (rutaExcel != null ? 1 : 0),
                Rutas = rutasTemporales,
                RutaExcel = rutaExcel
            });
        }

        // ── Procesar archivos ─────────────────────────────────────────────

        [HttpPost("process")]
        public ActionResult<List<FacturaDto>> Process([FromBody] ProcessRequest request)
        {
            if ((request.Rutas == null || request.Rutas.Count == 0)
                && string.IsNullOrEmpty(request.RutaExcel))
                return BadRequest("No hay archivos para procesar");

            var nuevas = _processor.ProcesarMixto(
                request.Rutas ?? new(),
                request.RutaExcel);

            lock (_lock)
            {
                foreach (var f in nuevas)
                {
                    var clave = $"{f.NumeroFactura}|{f.Emisor.NIF}|{f.Fecha:yyyy-MM-dd}|{f.BaseImponible}";
                    bool duplicada = _facturas.Any(ex =>
                        $"{ex.NumeroFactura}|{ex.Emisor.NIF}|{ex.Fecha:yyyy-MM-dd}|{ex.BaseImponible}" == clave
                        && ex.RutaArchivo != f.RutaArchivo);

                    if (duplicada && f.Estado == EstadoFactura.OK)
                        f.Estado = EstadoFactura.Duplicada;
                }

                _facturas.AddRange(nuevas);
            }

            // Limpiar archivos temporales
            foreach (var ruta in request.Rutas ?? Enumerable.Empty<string>())
            {
                try { if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta); }
                catch { }
            }
            if (!string.IsNullOrEmpty(request.RutaExcel))
            {
                try { if (System.IO.File.Exists(request.RutaExcel)) System.IO.File.Delete(request.RutaExcel); }
                catch { }
            }

            return Ok(nuevas.Select(FacturaDto.FromFactura).ToList());
        }

        // ── Listar facturas ───────────────────────────────────────────────

        [HttpGet]
        public ActionResult<List<FacturaDto>> GetAll([FromQuery] string? filtro, [FromQuery] string? estado)
        {
            lock (_lock)
            {
                IEnumerable<Factura> query = _facturas;

                if (!string.IsNullOrEmpty(filtro))
                {
                    string f = filtro.ToUpperInvariant();
                    query = query.Where(fac =>
                        (fac.NumeroFactura ?? "").ToUpper().Contains(f) ||
                        (fac.Emisor.Nombre ?? "").ToUpper().Contains(f) ||
                        (fac.Emisor.NIF ?? "").ToUpper().Contains(f) ||
                        (fac.Receptor.Nombre ?? "").ToUpper().Contains(f));
                }

                if (!string.IsNullOrEmpty(estado) && estado != "Todos")
                {
                    if (Enum.TryParse<EstadoFactura>(estado, true, out var est))
                        query = query.Where(fac => fac.Estado == est);
                }

                return Ok(query.Select(FacturaDto.FromFactura).ToList());
            }
        }

        // ── Resumen ───────────────────────────────────────────────────────

        [HttpGet("resumen")]
        public ActionResult<ResumenDto> GetResumen()
        {
            lock (_lock)
            {
                return Ok(new ResumenDto
                {
                    Total = _facturas.Count,
                    OK = _facturas.Count(f => f.Estado == EstadoFactura.OK),
                    Revisar = _facturas.Count(f => f.Estado == EstadoFactura.Revisar),
                    Error = _facturas.Count(f => f.Estado == EstadoFactura.Error),
                    Duplicada = _facturas.Count(f => f.Estado == EstadoFactura.Duplicada),
                    Pendiente = _facturas.Count(f => f.Estado == EstadoFactura.Pendiente),
                    TotalEuros = _facturas.Where(f => f.Estado == EstadoFactura.OK).Sum(f => f.TotalFactura),
                    OcrCount = _facturas.Count(f => f.ExtractedByOcr)
                });
            }
        }

        // ── Exportar Excel ────────────────────────────────────────────────

        [HttpGet("export/ingresos")]
        public IActionResult ExportarIngresos()
        {
            return ExportarExcel(false);
        }

        [HttpGet("export/gastos")]
        public IActionResult ExportarGastos()
        {
            return ExportarExcel(true);
        }

        private IActionResult ExportarExcel(bool esGasto)
        {
            List<Factura> copia;
            lock (_lock)
            {
                copia = _facturas.ToList();
            }

            if (copia.Count == 0)
                return BadRequest("No hay facturas para exportar");

            string nombreArchivo = esGasto ? "Gastos_FacturasApp.xlsx" : "Ingresos_FacturasApp.xlsx";
            string rutaTemporal = Path.Combine(_env.ContentRootPath, "temp_uploads", nombreArchivo);
            Directory.CreateDirectory(Path.GetDirectoryName(rutaTemporal)!);

            var exportService = new ExportService();
            exportService.ExportarAExcel(copia, rutaTemporal, esGasto);

            var bytes = System.IO.File.ReadAllBytes(rutaTemporal);
            try { System.IO.File.Delete(rutaTemporal); } catch { }

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }

        // ── Limpiar ───────────────────────────────────────────────────────

        [HttpDelete]
        public IActionResult Clear()
        {
            lock (_lock)
            {
                _facturas.Clear();
            }
            return Ok();
        }

        // ── Eliminar una factura ──────────────────────────────────────────

        [HttpDelete("{index:int}")]
        public IActionResult Delete(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _facturas.Count)
                    return NotFound();
                _facturas.RemoveAt(index);
            }
            return Ok();
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────

    public class UploadResult
    {
        public int ArchivosRecibidos { get; set; }
        public List<string> Rutas { get; set; } = new();
        public string? RutaExcel { get; set; }
    }

    public class ProcessRequest
    {
        public List<string> Rutas { get; set; } = new();
        public string? RutaExcel { get; set; }
    }

    public class FacturaDto
    {
        public int Index { get; set; }
        public string NumeroFactura { get; set; } = "";
        public string? Fecha { get; set; }
        public string EmisorNombre { get; set; } = "";
        public string EmisorNif { get; set; } = "";
        public string ReceptorNombre { get; set; } = "";
        public string ReceptorNif { get; set; } = "";
        public decimal BaseImponible { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public decimal CuotaIVA { get; set; }
        public decimal PorcentajeIRPF { get; set; }
        public decimal CuotaIRPF { get; set; }
        public decimal PorcentajeRE { get; set; }
        public decimal CuotaRE { get; set; }
        public decimal TotalFactura { get; set; }
        public decimal TotalCalculado { get; set; }
        public string Estado { get; set; } = "";
        public string EstadoDisplay { get; set; } = "";
        public bool ExtractedByOcr { get; set; }
        public string RutaArchivo { get; set; } = "";
        public List<string> MensajesError { get; set; } = new();

        public static FacturaDto FromFactura(Factura f)
        {
            string estadoDisplay = f.Estado switch
            {
                EstadoFactura.OK => "✔ Correcto",
                EstadoFactura.Revisar => "⚠ Revisar",
                EstadoFactura.Duplicada => "⚠ Duplicada",
                EstadoFactura.Error => "✖ Error",
                EstadoFactura.Pendiente => "• Pendiente",
                _ => f.Estado.ToString()
            };

            return new FacturaDto
            {
                Index = -1,
                NumeroFactura = f.NumeroFactura ?? "",
                Fecha = f.Fecha?.ToString("dd/MM/yyyy"),
                EmisorNombre = f.Emisor.Nombre ?? "",
                EmisorNif = f.Emisor.NIF ?? "",
                ReceptorNombre = f.Receptor.Nombre ?? "",
                ReceptorNif = f.Receptor.NIF ?? "",
                BaseImponible = f.BaseImponible,
                PorcentajeIVA = f.PorcentajeIVA,
                CuotaIVA = f.CuotaIVACalculado,
                PorcentajeIRPF = f.PorcentajeIRPF,
                CuotaIRPF = f.CuotaIRPFCalculado,
                PorcentajeRE = f.PorcentajeRE,
                CuotaRE = f.CuotaRECalculado,
                TotalFactura = f.TotalFactura,
                TotalCalculado = f.TotalCalculado,
                Estado = f.Estado.ToString(),
                EstadoDisplay = estadoDisplay,
                ExtractedByOcr = f.ExtractedByOcr,
                RutaArchivo = Path.GetFileName(f.RutaArchivo),
                MensajesError = f.MensajeError ?? new()
            };
        }
    }

    public class ResumenDto
    {
        public int Total { get; set; }
        public int OK { get; set; }
        public int Revisar { get; set; }
        public int Error { get; set; }
        public int Duplicada { get; set; }
        public int Pendiente { get; set; }
        public decimal TotalEuros { get; set; }
        public int OcrCount { get; set; }
    }
}
