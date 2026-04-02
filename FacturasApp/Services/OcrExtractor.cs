using UglyToad.PdfPig;
using PdfPigPage = UglyToad.PdfPig.Content.Page;
using System.Drawing;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using Tesseract;
using PDFtoImage;
using SkiaSharp;
using FacturasApp.Models;

namespace FacturasApp.Services
{
    public class OcrExtractor
    {
        private readonly string _tessDataPath;
        private const string Idiomas = "spa+eng";

        // DPI de renderizado — mayor DPI = mejor OCR pero más lento
        // 300 es el estándar recomendado para OCR
        private const int DpiRenderizado = 300;

        public OcrExtractor(string tessDataPath = @"./tessdata")
        {
            _tessDataPath = tessDataPath;
        }

        public string ExtraerTextoConOcr(string rutaPdf)
        {
            var textoTotal = new System.Text.StringBuilder();

            using var engine = new TesseractEngine(
                _tessDataPath, Idiomas, EngineMode.Default);

            var paginas = RenderizarPaginas(rutaPdf);

            foreach (var bitmap in paginas)
            {
                try
                {
                    // ← Convertimos Bitmap a byte[] antes de pasarlo a ConvertirAPix
                    using var ms = new MemoryStream();
                    bitmap.Save(ms, DrawingImageFormat.Png);
                    byte[] bytes = ms.ToArray();

                    using var pix = ConvertirAPix(bytes);
                    if (pix == null) continue;

                    using var page = engine.Process(pix);
                    textoTotal.AppendLine(page.GetText());
                }
                catch
                {
                    // Si una página falla continuamos con la siguiente
                }
                finally
                {
                    bitmap.Dispose();
                }
            }

            return textoTotal.ToString().Trim();
        }
        // ── Renderizado de páginas ────────────────────────────────────────────

        private List<Bitmap> RenderizarPaginas(string rutaPdf)
        {
            var resultado = new List<Bitmap>();

            try
            {
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);
                int numPaginas = Conversion.GetPageCount(pdfBytes);

                for (int i = 0; i < numPaginas; i++)
                {
                    try
                    {
                        using var skBitmap = Conversion.ToImage(
                            pdfBytes,
                            page: new Index(i),
                            password: null,
                            options: new RenderOptions(Dpi: DpiRenderizado));

                        var bitmap = ConvertirSkBitmapABitmap(skBitmap);
                        if (bitmap != null)
                            resultado.Add(bitmap);
                    }
                    catch
                    {
                        // Si una página falla continuamos
                    }
                }
            }
            catch
            {
                
            }

            return resultado;
        }

        // ── Conversión SKBitmap → System.Drawing.Bitmap ───────────────────────

        private Bitmap? ConvertirSkBitmapABitmap(SkiaSharp.SKBitmap skBitmap)
        {
            try
            {
                // Codificamos el SKBitmap a PNG en memoria
                using var skImage = SkiaSharp.SKImage.FromBitmap(skBitmap);
                using var skData = skImage.Encode(
                    SkiaSharp.SKEncodedImageFormat.Png, 100);
                using var ms = new MemoryStream(skData.ToArray());

                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }

        // ── Conversión Bitmap → Pix de Tesseract ──────────────────────────────
        private Pix? ConvertirAPix(byte[] bytesImagen)
        {
            try
            {
                using var ms = new MemoryStream(bytesImagen);
                using var bitmap = new Bitmap(ms);

                using var msPng = new MemoryStream();
                bitmap.Save(msPng, DrawingImageFormat.Png); // ← alias aquí
                return Pix.LoadFromMemory(msPng.ToArray());
            }
            catch
            {
                return null;
            }
        }

        // ── Extracción con plantilla zonal ────────────────────────────────────────

        // Si existe plantilla para el emisor usa extracción zonal
        // Si no existe usa OCR completo de página
        public string ExtraerTextoConOcrInteligente(string rutaPdf, string nombreEmisor)
        {
            var plantillaService = new PlantillaOcrService();
            var plantilla = plantillaService.ObtenerPorEmisor(nombreEmisor);

            if (plantilla == null || plantilla.Zonas.Count == 0)
            {
                // Sin plantilla — OCR completo
                return ExtraerTextoConOcr(rutaPdf);
            }

            // Con plantilla — OCR zonal
            var zonalExtractor = new OcrZonalExtractor(_tessDataPath);
            var textosPorCampo = zonalExtractor.ExtraerZonas(rutaPdf, plantilla);

            // Construimos un texto estructurado con separadores
            // que los parsers pueden interpretar fácilmente
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in textosPorCampo)
                sb.AppendLine($"[{kvp.Key}]: {kvp.Value}");

            return sb.ToString();
        }
    }
}