using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using Docnet.Core;
using Docnet.Core.Models;
using FacturasApp.Core.Models;
using FacturasApp.Core.Services;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using Tesseract;

namespace FacturasApp.Web.Services
{
    public class WebTextExtractor : ITextExtractor
    {
        private readonly string _tessDataPath;
        private const string Idiomas = "spa";
        private const int DpiRender = 300;
        private const int MinCaracteres = 30;

        // Tamaño de referencia para coordenadas (proporciones A4).
        // Las zonas porcentuales y el reensamblaje por posición necesitan
        // un espacio de coordenadas con valores significativos.
        private const int RefPageWidth = 1000;
        private const int RefPageHeight = 1414;

        public WebTextExtractor(string tessDataPath = @"./tessdata")
        {
            _tessDataPath = tessDataPath;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static IDocLib GetDocLib() => DocLib.Instance;

        private static (int PageWidth, int PageHeight) ObtenerTamanosPagina(string rutaPdf, int numPagina)
        {
            using var docReader = GetDocLib().GetDocReader(rutaPdf, new PageDimensions(RefPageWidth, RefPageHeight));
            using var pageReader = docReader.GetPageReader(numPagina);
            return (pageReader.GetPageWidth(), pageReader.GetPageHeight());
        }

        private static string ObtenerTextoPagina(string rutaPdf, int numPagina)
        {
            using var docReader = GetDocLib().GetDocReader(rutaPdf, new PageDimensions(1, 1));
            using var pageReader = docReader.GetPageReader(numPagina);
            return pageReader.GetText() ?? string.Empty;
        }

        private static List<Character> ObtenerCaracteresPagina(string rutaPdf, int numPagina)
        {
            using var docReader = GetDocLib().GetDocReader(rutaPdf, new PageDimensions(RefPageWidth, RefPageHeight));
            using var pageReader = docReader.GetPageReader(numPagina);
            return pageReader.GetCharacters().ToList();
        }

        private static Bitmap RenderizarPagina(string rutaPdf, int numPagina, int width, int height)
        {
            using var docReader = GetDocLib().GetDocReader(rutaPdf, new PageDimensions(width, height));
            using var pageReader = docReader.GetPageReader(numPagina);
            var rawBytes = pageReader.GetImage();
            int imgWidth = pageReader.GetPageWidth();
            int imgHeight = pageReader.GetPageHeight();
            return ConvertirABitmap(rawBytes, imgWidth, imgHeight);
        }

        private static Bitmap ConvertirABitmap(byte[] rawBytes, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(rawBytes, 0, bmpData.Scan0, rawBytes.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private int ObtenerPageCount(string rutaPdf)
        {
            using var docReader = GetDocLib().GetDocReader(rutaPdf, new PageDimensions(1, 1));
            return docReader.GetPageCount();
        }

        // ── Texto seleccionable ────────────────────────────────────────────

        public string? ExtraerTextoSeleccionable(string rutaPdf, ModoExtraccion modo = ModoExtraccion.Simple)
        {
            var textoTotal = new StringBuilder();
            int pageCount = ObtenerPageCount(rutaPdf);

            for (int i = 0; i < pageCount; i++)
            {
                string textoPagina = modo == ModoExtraccion.Simple
                    ? ObtenerTextoPagina(rutaPdf, i)
                    : ExtraerOrdenadoPorPosicion(rutaPdf, i);

                textoTotal.AppendLine(textoPagina);
            }

            string resultado = textoTotal.ToString().Trim();
            return resultado.Length >= MinCaracteres ? resultado : null;
        }

        public bool EsSeleccionable(string rutaPdf) =>
            ExtraerTextoSeleccionable(rutaPdf) != null;

        private static string ExtraerOrdenadoPorPosicion(string rutaPdf, int numPagina)
        {
            var chars = ObtenerCaracteresPagina(rutaPdf, numPagina)
                .Where(c => !char.IsControl(c.Char))
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
            int pageCount = ObtenerPageCount(rutaPdf);

            foreach (var zona in plantilla.Zonas)
            {
                try
                {
                    if (zona.NumPagina < 1 || zona.NumPagina > pageCount)
                    {
                        resultado[zona.Campo] = string.Empty;
                        continue;
                    }

                    int indicePagina = zona.NumPagina - 1;

                    // Obtener dimensiones de la página
                    var (paginaWidth, paginaHeight) = ObtenerTamanosPagina(rutaPdf, indicePagina);

                    // Docnet usa coordenadas de pantalla (Y desde arriba)
                    int izq = (int)(zona.X / 100.0 * paginaWidth);
                    int ancho = (int)(zona.Ancho / 100.0 * paginaWidth);
                    int top = (int)(zona.Y / 100.0 * paginaHeight);
                    int alto = (int)(zona.Alto / 100.0 * paginaHeight);
                    int der = izq + ancho;
                    int bot = top + alto;

                    // Obtener caracteres y filtrar por zona
                    var chars = ObtenerCaracteresPagina(rutaPdf, indicePagina)
                        .Where(c => !char.IsControl(c.Char))
                        .Where(c => c.Box.Left >= izq && c.Box.Right <= der
                                 && c.Box.Top >= top && c.Box.Bottom <= bot)
                        .ToList();

                    resultado[zona.Campo] = ReensamblarPorPosicion(chars);
                }
                catch
                {
                    resultado[zona.Campo] = string.Empty;
                }
            }

            return resultado;
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

            var paginasRequeridas = plantilla.Zonas
                .Select(z => z.NumPagina).Distinct().ToList();

            var bitmapsPaginas = new Dictionary<int, Bitmap>();
            try
            {
                int pageCount = ObtenerPageCount(rutaPdf);
                foreach (var numPag in paginasRequeridas)
                {
                    if (numPag >= 1 && numPag <= pageCount)
                    {
                        int idx = numPag - 1;
                        var (tamanoW, tamanoH) = ObtenerTamanosPagina(rutaPdf, idx);
                        int w = (int)Math.Ceiling(tamanoW * DpiRender / 72f);
                        int h = (int)Math.Ceiling(tamanoH * DpiRender / 72f);
                        var bmp = RenderizarPagina(rutaPdf, idx, w, h);
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

                    var rect = new Rectangle(x, y, w2, h2);
                    using var zonaBitmap = paginaBitmap.Clone(rect, paginaBitmap.PixelFormat);

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
                int pageCount = ObtenerPageCount(rutaPdf);
                for (int i = 0; i < pageCount; i++)
                {
                    try
                    {
                        var (tamanoW, tamanoH) = ObtenerTamanosPagina(rutaPdf, i);
                        int w = (int)Math.Ceiling(tamanoW * DpiRender / 72f);
                        int h = (int)Math.Ceiling(tamanoH * DpiRender / 72f);
                        using var bitmap = RenderizarPagina(rutaPdf, i, w, h);
                        var pix = ConvertirAPix(bitmap);
                        if (pix != null)
                        {
                            using var page = engine.Process(pix);
                            textoTotal.AppendLine(page.GetText());
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
                int pageCount = ObtenerPageCount(rutaPdf);
                if (pageCount == 0) return string.Empty;

                var (tamanoW, tamanoH) = ObtenerTamanosPagina(rutaPdf, 0);
                int w = (int)Math.Ceiling(tamanoW * DpiRender / 72f);
                int h = (int)Math.Ceiling(tamanoH * DpiRender / 72f);
                using var bitmap = RenderizarPagina(rutaPdf, 0, w, h);

                int altoTercio = bitmap.Height / 3;
                var rect = new Rectangle(0, 0, bitmap.Width, altoTercio);
                using var bitmapTercio = bitmap.Clone(rect, bitmap.PixelFormat);
                var pix = ConvertirAPix(bitmapTercio);
                if (pix != null)
                {
                    using var page = engine.Process(pix);
                    return page.GetText().Trim();
                }
            }
            catch { }

            return string.Empty;
        }

        // ── Helpers internos ───────────────────────────────────────────────

        private TesseractEngine CrearEngine() =>
            new(_tessDataPath, Idiomas, EngineMode.TesseractAndLstm);

        private static void Preprocesar(Bitmap bitmap, PreprocesamientoOcr config)
        {
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

                    bitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
        }

        private static string ReensamblarPorPosicion(List<Character> chars)
        {
            if (chars.Count == 0) return string.Empty;

            const double toleranciaLinea = 9.2;

            // Docnet BoundBox usa coordenadas de pantalla (Y desde arriba).
            // Top = borde superior del glifo (menor Y), Bottom = borde inferior (mayor Y).
            // Usamos Bottom como "ancla" de línea (borde inferior del glifo).

            var anclas = new List<double>();
            foreach (var bottom in chars
                         .Select(c => (double)c.Box.Bottom)
                         .OrderBy(b => b))
            {
                if (anclas.Count == 0 || bottom - anclas[^1] > toleranciaLinea)
                    anclas.Add(bottom);
            }

            var lineas = chars
                .GroupBy(c => anclas
                    .OrderBy(a => Math.Abs(c.Box.Bottom - a))
                    .First())
                .OrderBy(g => g.Key)
                .Select(g => string.Concat(
                    g.OrderBy(c => c.Box.Left)
                     .Select(c => c.Char)))
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
