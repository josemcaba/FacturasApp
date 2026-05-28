using FacturasApp.Models;
using FacturasApp.Services;
using FacturasApp.Services.Parsers;
using System.Drawing.Drawing2D;

namespace FacturasApp.UI
{
    public partial class DefinirPlantillaForm : Form
    {
        // ── Servicios ─────────────────────────────────────────────────────────
        private readonly PlantillaOcrService _plantillaService = new();
        private readonly OcrZonalExtractor _ocrExtractor = new();
        private readonly ParserFactory _parserFactory = new();

        // ── Estado ────────────────────────────────────────────────────────────
        private Bitmap? _imagenPagina;
        private string _rutaPdf = string.Empty;
        private string _nombreEmisor = string.Empty;
        private PlantillaOcr _plantilla = new();

        private bool _dibujando = false;
        private Point _puntoInicio;
        private Point _puntoActual;
        private bool _rectanguloActivo = false;

        public DefinirPlantillaForm()
        {
            InitializeComponent(); // ← llama al del Designer

            // 🔒 Bloquear UI de edición en producción
            if (!EnvironmentService.EsDesarrollo())
            {
                btnGuardar.Hide();

                // Mostrar indicador de modo de producción
                Text = "FacturasApp - 🔒 Modo Lectura (Cliente)";
            }
            else
            {
                Text = "FacturasApp - ✏️ Modo Edición (Desarrollo)";
            }

            // Poblamos el ComboBox de emisores con los parsers disponibles
            var emisoresDisponibles = _parserFactory.ParsersDisponibles.ToList();
            cmbEmisor.Items.AddRange(emisoresDisponibles.Cast<object>().ToArray());
            if (cmbEmisor.Items.Count > 0)
                cmbEmisor.SelectedIndex = 0;

            // Evento para mostrar texto cuando se selecciona una zona
            lstZonas.SelectedIndexChanged += LstZonas_SelectedIndexChanged;
        }

        // ── Carga del PDF ─────────────────────────────────────────────────────

        private void BtnCargarPdf_Click(object? sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar PDF de muestra",
                Filter = "Archivos PDF (*.pdf)|*.pdf"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            _rutaPdf = dialogo.FileName;

            if (string.IsNullOrEmpty(cmbEmisor.Text))
                cmbEmisor.Text = Path.GetFileNameWithoutExtension(_rutaPdf);

            _imagenPagina?.Dispose();
            _imagenPagina = _ocrExtractor.RenderizarPagina(_rutaPdf, 0);

            if (_imagenPagina == null)
            {
                MessageBox.Show("No se pudo renderizar el PDF.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            picFactura.Image = _imagenPagina;

            _nombreEmisor = cmbEmisor.Text.Trim();
            var existente = _plantillaService.ObtenerPorEmisor(_nombreEmisor);
            if (existente != null)
            {
                _plantilla = existente;
                ActualizarListaZonas();
                MessageBox.Show(
                    $"Se ha cargado la plantilla existente para '{_nombreEmisor}'",
                    "Plantilla cargada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _plantilla = new PlantillaOcr { Emisor = _nombreEmisor };
            }

            picFactura.Invalidate();
        }

        // ── Dibujo de rectángulos ─────────────────────────────────────────────

        private void PicFactura_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_imagenPagina == null) return;
            if (e.Button != MouseButtons.Left) return;

            _dibujando = true;
            _rectanguloActivo = false;
            _puntoInicio = e.Location;
            _puntoActual = e.Location;
        }

        private void PicFactura_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dibujando) return;
            _puntoActual = e.Location;
            _rectanguloActivo = true;
            picFactura.Invalidate();
        }

        private void PicFactura_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_dibujando) return;
            _dibujando = false;

            var rect = ObtenerRectanguloNormalizado(_puntoInicio, _puntoActual);

            if (rect.Width < 10 || rect.Height < 10)
            {
                _rectanguloActivo = false;
                picFactura.Invalidate();
                return;
            }

            var zonaOcr = ConvertirARectanglePorcentual(rect);

            // ← Generar nombre secuencial de zona: Zona1, Zona2, Zona3, etc.
            int numeroZona = _plantilla.Zonas.Count + 1;
            zonaOcr.Campo = $"Zona{numeroZona}";

            _plantilla.Zonas.Add(zonaOcr);
            ActualizarListaZonas();

            // ← Extraer y mostrar el texto OCR de la zona creada
            MostrarTextoZona(zonaOcr);

            _rectanguloActivo = false;
            picFactura.Invalidate();
        }

        private void PicFactura_Paint(object? sender, PaintEventArgs e)
        {
            if (_imagenPagina == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var zona in _plantilla.Zonas)
            {
                var rect = ConvertirAPixelesPictureBox(zona);
                using var pen = new Pen(Color.FromArgb(46, 117, 182), 2);
                using var brush = new SolidBrush(Color.FromArgb(40, 46, 117, 182));
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);

                using var font = new Font("Segoe UI", 7f, FontStyle.Bold);
                g.DrawString(zona.Campo, font,
                    Brushes.DarkBlue, rect.X + 2, rect.Y + 2);
            }

            if (_rectanguloActivo)
            {
                var rect = ObtenerRectanguloNormalizado(_puntoInicio, _puntoActual);
                using var pen = new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash };
                using var brush = new SolidBrush(Color.FromArgb(40, 255, 0, 0));
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);
            }
        }

        // ── Coordenadas ───────────────────────────────────────────────────────

        private Rectangle ObtenerRectanguloNormalizado(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        private ZonaOcr ConvertirARectanglePorcentual(Rectangle rectPictureBox)
        {
            var areaImagen = CalcularAreaImagenEnPictureBox();

            double xReal = (rectPictureBox.X - areaImagen.X) / (double)areaImagen.Width;
            double yReal = (rectPictureBox.Y - areaImagen.Y) / (double)areaImagen.Height;
            double wReal = rectPictureBox.Width / (double)areaImagen.Width;
            double hReal = rectPictureBox.Height / (double)areaImagen.Height;

            return new ZonaOcr
            {
                X = Math.Max(0, xReal * 100),
                Y = Math.Max(0, yReal * 100),
                Ancho = Math.Min(100, wReal * 100),
                Alto = Math.Min(100, hReal * 100)
            };
        }

        private Rectangle ConvertirAPixelesPictureBox(ZonaOcr zona)
        {
            var areaImagen = CalcularAreaImagenEnPictureBox();

            return new Rectangle(
                (int)(areaImagen.X + zona.X / 100.0 * areaImagen.Width),
                (int)(areaImagen.Y + zona.Y / 100.0 * areaImagen.Height),
                (int)(zona.Ancho / 100.0 * areaImagen.Width),
                (int)(zona.Alto / 100.0 * areaImagen.Height));
        }

        private Rectangle CalcularAreaImagenEnPictureBox()
        {
            if (_imagenPagina == null)
                return new Rectangle(0, 0, picFactura.Width, picFactura.Height);

            float escalaX = (float)picFactura.Width / _imagenPagina.Width;
            float escalaY = (float)picFactura.Height / _imagenPagina.Height;
            float escala = Math.Min(escalaX, escalaY);

            int anchoReal = (int)(_imagenPagina.Width * escala);
            int altoReal = (int)(_imagenPagina.Height * escala);
            int offsetX = (picFactura.Width - anchoReal) / 2;
            int offsetY = (picFactura.Height - altoReal) / 2;

            return new Rectangle(offsetX, offsetY, anchoReal, altoReal);
        }

        // ── Gestión de zonas y OCR ───────────────────────────────────────────

        private void ActualizarListaZonas()
        {
            lstZonas.Items.Clear();
            foreach (var zona in _plantilla.Zonas)
                lstZonas.Items.Add(
                    $"{zona.Campo}  " +
                    $"[X:{zona.X:F1}% Y:{zona.Y:F1}% " +
                    $"W:{zona.Ancho:F1}% H:{zona.Alto:F1}%]");
        }

        private void LstZonas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstZonas.SelectedIndex < 0)
            {
                txtTexto.Text = string.Empty;
                txtTexto.ForeColor = Color.Black;
                return;
            }

            var zonaSeleccionada = _plantilla.Zonas[lstZonas.SelectedIndex];
            MostrarTextoZona(zonaSeleccionada);
        }

        private void MostrarTextoZona(ZonaOcr zona)
        {
            if (string.IsNullOrEmpty(_rutaPdf))
            {
                txtTexto.Text = "Carga un PDF primero";
                txtTexto.ForeColor = Color.Black;
                return;
            }

            try
            {
                txtTexto.Text = "Extrayendo...";
                txtTexto.ForeColor = Color.DarkGray;
                Application.DoEvents();

                // Extraer texto con información del método utilizado
                var resultado = _ocrExtractor.ExtraerTextoZonalConMetadata(_rutaPdf, zona);

                // Mostrar indicador visual y descripción
                string prefijo = resultado.ObtenerPrefijo();
                string descripcion = resultado.ObtenerDescripcion();
                string contenido = resultado.EstaVacia ? "(Sin texto detectado)" : resultado.Texto.Replace("\n", "\r\n");

                // Mostrar en el cuadro de texto
                txtTexto.Text = $"{prefijo} [{descripcion}]\r\n{new string('-', 40)}\r\n{contenido}";

                // Cambiar color del indicador según el método
                txtTexto.ForeColor = resultado.ObtenerColor();

                System.Diagnostics.Debug.WriteLine(
                    $"Método: {descripcion}, Vacía: {resultado.EstaVacia}, Longitud: {resultado.Texto.Length}");
            }
            catch (Exception ex)
            {
                txtTexto.Text = $"❌ Error: {ex.Message}";
                txtTexto.ForeColor = Color.Red;
            }
        }

        private void BtnEliminarZona_Click(object? sender, EventArgs e)
        {
            if (lstZonas.SelectedIndex < 0) return;
            _plantilla.Zonas.RemoveAt(lstZonas.SelectedIndex);
            // ← Renumerar las zonas restantes para mantener consistencia
            for (int i = 0; i < _plantilla.Zonas.Count; i++)
                _plantilla.Zonas[i].Campo = $"Zona{i + 1}";
            ActualizarListaZonas();
            txtTexto.Text = string.Empty;
            txtTexto.ForeColor = Color.Black;
            picFactura.Invalidate();
        }

        // ── Guardar ───────────────────────────────────────────────────────────

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            _nombreEmisor = cmbEmisor.Text.Trim();

            if (string.IsNullOrEmpty(_nombreEmisor))
            {
                MessageBox.Show("Introduce el nombre del emisor.",
                    "Campo requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_plantilla.Zonas.Count == 0)
            {
                MessageBox.Show("Define al menos una zona antes de guardar.",
                    "Sin zonas",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _plantilla.Emisor = _nombreEmisor;
                _plantillaService.GuardarPlantilla(_plantilla);

                MessageBox.Show(
                    $"Plantilla guardada correctamente para '{_nombreEmisor}'.\n" +
                    $"Zonas definidas: {_plantilla.Zonas.Count}",
                    "Plantilla guardada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    $"❌ Operación bloqueada\n\n{ex.Message}",
                    "Acceso denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error guardando plantilla:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            _imagenPagina?.Dispose();
            base.Close();
        }
    }
}