using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using FacturasApp.Core.Models;
using FacturasApp.Core.Services;
using PdfiumViewer;
using System.Drawing;
using System.Text;
using Tesseract;

namespace FacturasApp.Web.Services
{
    /// <summary>
    /// Implementación de ITextExtractor para ASP.NET Core.
    /// Reimplementa la lógica de PdfTextExtractor, OcrExtractor y OcrZonalExtractor
    /// usando las mismas librerías NuGet (PDFium, Tesseract).
    /// </summary>
    public class WebTextExtractor : ITextExtractor
    {
        private readonly string _tessDataPath;
        private const string Idiomas = "spa";
        private const int DpiRender = 300;
        private const int MinCaracteres = 30;

        public WebTextExtractor(string tessDataPath = @"./tessdata")
        {
            _tessDataPath = tessDataPath;
        }

        // ── Texto seleccionable ────────────────────────────────────────────

        public string? ExtraerTextoSeleccionable(string rutaPdf, ModoExtraccion modo = ModoExtraccion.Simple)
        {
            var textoTotal = new StringBuilder();

            using var documento = PdfDocument.Load(rutaPdf);

            for (int i = 0; i < documento.PageCount; i++)
            {
                string textoPagina = modo == ModoExtraccion.Simple
                    ? ExtraerSimple(documento, i)
                    : ExtraerOrdenadoPorPosicion(documento, i);

                textoTotal.AppendLine(textoPagina);
            }

            string resultado = textoTotal.ToString().Trim();
            return resultado.Length >= MinCaracteres ? resultado : null;
        }

        public bool EsSeleccionable(string rutaPdf) =>
            ExtraerTextoSeleccionable(rutaPdf) != null;

        private static string ExtraerSimple(PdfDocument documento, int numPagina) =>
            documento.GetPdfText(numPagina) ?? string.Empty;

        private static string ExtraerOrdenadoPorPosicion(PdfDocument documento, int numPagina)
        {
            var chars = documento.GetCharacterInformation(numPagina)
                .Where(c => !char.IsControl(c.Character))
                .ToList();

            return ReensamblarPorPosicion(chars);
        }

        // ── Extracción zonal (coordenadas, sin OCR) ────────────────────────

        public string ExtraerTextoZonal(string rutaPdf, PlantillaOcr plantilla)
        {
            var textosZonas = ExtraerZonasTexto(rutaPdf, plantilla);
            var sb = new StringBuilder();
            foreach (var zona in plantilla.Zonas)
            {
                if (textosZonas.TryGetValue(zona.Campo, out string? texto) && !string.IsNullOrWhiteSpace(texto))
                    sb.AppendLine($"[{zona.Campo}]: {texto}");
            }
            string resultado = sb.ToString().Trim();
            return resultado.Length < 10 ? string.Empty : resultado;
        }

        private Dictionary<string, string> ExtraerZonasTexto(string rutaPdf, PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();

            using var documento = PdfDocument.Load(rutaPdf);

            foreach (var zona in plantilla.Zonas)
            {
                try
                {
                    if (zona.NumPagina < 1 || zona.NumPagina > documento.PageCount)
                    {
                        resultado[zona.Campo] = string.Empty;
                        continue;
                    }

                    int indicePagina = zona.NumPagina - 1;
                    var tamanio = documento.PageSizes[indicePagina];
                    var rect = ConvertirZonaAPdfRectangle(zona, tamanio.Width, tamanio.Height);
                    string textoDirecto = ExtraerTextoLayoutDesdeArea(documento, indicePagina, rect);
                    resultado[zona.Campo] = textoDirecto;
                }
                catch
                {
                    resultado[zona.Campo] = string.Empty;
                }
            }

            return resultado;
        }

        private static string ExtraerTextoLayoutDesdeArea(PdfDocument documento, int indicePagina, RectangleF rect)
        {
            var chars = documento.GetCharacterInformation(indicePagina);

            var charsEnArea = chars
                .Where(c => !char.IsControl(c.Character))
                .Where(c => c.Bounds.X >= rect.Left
                    && (c.Bounds.X + c.Bounds.Width) <= rect.Right
                    && c.Bounds.Y >= rect.Top
                    && (c.Bounds.Y + c.Bounds.Height) <= rect.Bottom)
                .ToList();

            return ReensamblarPorPosicion(charsEnArea);
        }

        private static RectangleF ConvertirZonaAPdfRectangle(ZonaOcr zona, float paginaWidth, float paginaHeight)
        {
            float izquierda = (float)(zona.X / 100.0) * paginaWidth;
            float ancho = (float)(zona.Ancho / 100.0) * paginaWidth;
            float topDesdeArriba = (float)(zona.Y / 100.0) * paginaHeight;
            float altoZona = (float)(zona.Alto / 100.0) * paginaHeight;
            float yPdf = paginaHeight - topDesdeArriba - altoZona;

            return new RectangleF(izquierda, yPdf, ancho, altoZona);
        }

        // ── OCR zonal ──────────────────────────────────────────────────────

        public string ExtraerTextoOcrZonal(string rutaPdf, PlantillaOcr plantilla)
        {
            var textosZonas = ExtraerZonasOcr(rutaPdf, plantilla);
            var sb = new StringBuilder();
            foreach (var zona in plantilla.Zonas)
            {
                if (textosZonas.TryGetValue(zona.Campo, out string? texto) && !string.IsNullOrWhiteSpace(texto))
                    sb.AppendLine($"[{zona.Campo}]: {texto}");
            }
            string resultado = sb.ToString().Trim();
            return resultado.Length < 10 ? string.Empty : resultado;
        }

        private Dictionary<string, string> ExtraerZonasOcr(string rutaPdf, PlantillaOcr plantilla)
        {
            var resultado = new Dictionary<string, string>();
            if (plantilla.Zonas.Count == 0) return resultado;

            // Renderizar solo las páginas necesarias
            var paginasRequeridas = plantilla.Zonas
                .Select(z => z.NumPagina).Distinct().ToList();

            var bitmapsPaginas = new Dictionary<int, Bitmap>();
            try
            {
                using var doc = PdfDocument.Load(rutaPdf);
                foreach (var numPag in paginasRequeridas)
                {
                    if (numPag >= 1 && numPag <= doc.PageCount)
                    {
                        int idx = numPag - 1;
                        var tamano = doc.PageSizes[idx];
                        int w = (int)Math.Ceiling(tamano.Width * DpiRender / 72f);
                        int h = (int)Math.Ceiling(tamano.Height * DpiRender / 72f);
                        var img = doc.Render(idx, w, h, DpiRender, DpiRender, PdfRenderFlags.ForPrinting);
                        if (img is Bitmap bmp)
                            bitmapsPaginas[numPag] = bmp;
                    }
                }
            }
            catch { return resultado; }

            using var engine = CrearEngine();
            foreach (var zona in plantilla.Zonas)
            {
                try
                {
                    if (!bitmapsPaginas.TryGetValue(zona.NumPagina, out var paginaBitmap))
                    {
                        resultado[zona.Campo] = string.Empty;
                        continue;
                    }

                    int x = (int)(zona.X * paginaBitmap.Width / 100.0);
                    int y = (int)(zona.Y * paginaBitmap.Height / 100.0);
                    int w2 = (int)(zona.Ancho * paginaBitmap.Width / 100.0);
                    int h2 = (int)(zona.Alto * paginaBitmap.Height / 100.0);

                    var rect = new System.Drawing.Rectangle(x, y, w2, h2);
                    using var zonaBitmap = paginaBitmap.Clone(rect, paginaBitmap.PixelFormat);

                    // Preprocesamiento
                    if (zona.Preprocesamiento != null)
                        Preprocesar(zonaBitmap, zona.Preprocesamiento);

                    using var pix = ConvertirAPix(zonaBitmap);
                    if (pix != null)
                    {
                        using var page = engine.Process(pix);
                        resultado[zona.Campo] = page.GetText().Trim();
                    }
                    else
                        resultado[zona.Campo] = string.Empty;
                }
                catch
                {
                    resultado[zona.Campo] = string.Empty;
                }
            }

            foreach (var bmp in bitmapsPaginas.Values)
                bmp.Dispose();

            return resultado;
        }

        // ── OCR completo ───────────────────────────────────────────────────

        public string ExtraerTextoOcrCompleto(string rutaPdf)
        {
            var textoTotal = new StringBuilder();

            using var engine = CrearEngine();

            try
            {
                using var documento = PdfDocument.Load(rutaPdf);
                for (int i = 0; i < documento.PageCount; i++)
                {
                    try
                    {
                        var tamano = documento.PageSizes[i];
                        int w = (int)Math.Ceiling(tamano.Width * DpiRender / 72f);
                        int h = (int)Math.Ceiling(tamano.Height * DpiRender / 72f);
                        var img = documento.Render(i, w, h, DpiRender, DpiRender, PdfRenderFlags.ForPrinting);
                        if (img is Bitmap bitmap)
                        {
                            using (bitmap)
                            {
                                var pix = ConvertirAPix(bitmap);
                                if (pix != null)
                                {
                                    using var page = engine.Process(pix);
                                    textoTotal.AppendLine(page.GetText());
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return textoTotal.ToString().Trim();
        }

        public string ExtraerTextoOcrIdentificacion(string rutaPdf)
        {
            using var engine = CrearEngine();

            try
            {
                using var documento = PdfDocument.Load(rutaPdf);
                if (documento.PageCount == 0) return string.Empty;

                var tamano = documento.PageSizes[0];
                int w = (int)Math.Ceiling(tamano.Width * DpiRender / 72f);
                int h = (int)Math.Ceiling(tamano.Height * DpiRender / 72f);
                var img = documento.Render(0, w, h, DpiRender, DpiRender, PdfRenderFlags.ForPrinting);
                if (img is not Bitmap bitmap) return string.Empty;

                using (bitmap)
                {
                    int altoTercio = bitmap.Height / 3;
                    var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, altoTercio);
                    using var bitmapTercio = bitmap.Clone(rect, bitmap.PixelFormat);
                    var pix = ConvertirAPix(bitmapTercio);
                    if (pix != null)
                    {
                        using var page = engine.Process(pix);
                        return page.GetText().Trim();
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private TesseractEngine CrearEngine() =>
            new(_tessDataPath, Idiomas, EngineMode.TesseractAndLstm);

        private static void Preprocesar(Bitmap bitmap, PreprocesamientoOcr config)
        {
            // Simplificado: solo escala de grises y binarización básico
            // El preprocesamiento completo requiere System.Drawing.Imaging más avanzado
            if (!config.EscalaGrises) return;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    int gray = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                    if (config.Binarizacion && gray < config.UmbralBinarizacion)
                        gray = 0;
                    else if (config.Binarizacion)
                        gray = 255;

                    if (config.InvertirColores)
                        gray = 255 - gray;

                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(gray, gray, gray));
                }
            }
        }

        private static string ReensamblarPorPosicion(List<PdfCharacterInformation> chars)
        {
            if (chars.Count == 0) return string.Empty;

            const double toleranciaLinea = 9.2;

            var anclas = new List<double>();
            foreach (var bottom in chars
                         .Select(c => c.Bounds.Y + c.Bounds.Height)
                         .OrderByDescending(b => b))
            {
                if (anclas.Count == 0 || anclas[^1] - bottom > toleranciaLinea)
                    anclas.Add(bottom);
            }

            var lineas = chars
                .GroupBy(c => anclas
                    .OrderBy(a => Math.Abs((c.Bounds.Y + c.Bounds.Height) - a))
                    .First())
                .OrderByDescending(g => g.Key)
                .Select(g => string.Concat(
                    g.OrderBy(c => c.Bounds.X)
                     .Select(c => c.Character)))
                .Select(l => l.Trim())
                .Where(l => l.Length > 0);

            return string.Join(Environment.NewLine, lineas);
        }

        private static Pix? ConvertirAPix(Bitmap bitmap)
        {
            try
            {
                using var ms = new MemoryStream();
                bitmap.Save(ms, DrawingImageFormat.Png);
                return Pix.LoadFromMemory(ms.ToArray());
            }
            catch
            {
                return null;
            }
        }
    }
}
