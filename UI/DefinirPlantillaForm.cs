using FacturasApp.Models;
using FacturasApp.Services;
using FacturasApp.Services.Parsers;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

namespace FacturasApp.UI
{
    public partial class DefinirPlantillaForm : Form
    {
        // ── Servicios ─────────────────────────────────────────────────────────
        private readonly PlantillaOcrService _plantillaService = new();
        private readonly OcrZonalExtractor _ocrExtractor = new();
        private readonly ParserFactory _parserFactory = new();

        // ── Estado ────────────────────────────────────────────────────────────
        private string _rutaPdf = string.Empty;
        private string _nombreEmisor = string.Empty;
        private PlantillaOcr _plantilla = new();
        private string _plantillaOriginalXml = string.Empty;
        private readonly HashSet<string> _emisoresConPlantilla = new(StringComparer.OrdinalIgnoreCase);

        // Multi-página
        private readonly List<Bitmap> _imagenPaginas = new();
        private int _paginaActual = 0; // 0-based índice interno

        private bool _dibujando = false;
        private Point _puntoInicio;
        private Point _puntoActual;
        private bool _rectanguloActivo = false;

        public DefinirPlantillaForm()
        {
            InitializeComponent();

            if (!EnvironmentService.EsDesarrollo())
            {
                btnGuardar.Hide();
                btnEliminarPlantilla.Hide();
                Text = "FacturasApp - Modo Lectura (Cliente)";
            }
            else
            {
                Text = "FacturasApp - Modo Edición (Desarrollo)";
            }

            var emisoresDisponibles = _parserFactory.ParsersDisponibles.ToList();
            cmbEmisor.Items.AddRange(emisoresDisponibles.Cast<object>().ToArray());
            if (cmbEmisor.Items.Count > 0)
                cmbEmisor.SelectedIndex = 0;

            // Cargar emisores que tienen plantilla definida
            foreach (var emisor in _plantillaService.ObtenerEmisoresConPlantilla())
                _emisoresConPlantilla.Add(emisor);

            lstZonas.SelectedIndexChanged += LstZonas_SelectedIndexChanged;
            tabPaginas.SelectedIndexChanged += TabPaginas_SelectedIndexChanged;
            cmbEmisor.DrawItem += CmbEmisor_DrawItem;
            cmbEmisor.SelectedIndexChanged += CmbEmisor_SelectedIndexChanged;

            // Cargar plantilla del emisor seleccionado al iniciar
            CargarPlantillaEmisor();
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

            // Limpiar estado anterior
            LimpiarPaginas();

            // Renderizar todas las páginas
            var bitmaps = _ocrExtractor.RenderizarPaginas(_rutaPdf);
            if (bitmaps.Count == 0)
            {
                MessageBox.Show("No se pudo renderizar el PDF.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _imagenPaginas.AddRange(bitmaps);

            // Crear pestañas
            CrearPestanas();

            // Seleccionar primera pestaña
            if (tabPaginas.TabCount > 0)
                tabPaginas.SelectedIndex = 0;

            MostrarPaginaActual();
        }

        // ── Gestión de pestañas y páginas ────────────────────────────────────

        private void LimpiarPaginas()
        {
            tabPaginas.TabPages.Clear();
            foreach (var img in _imagenPaginas)
                img.Dispose();
            _imagenPaginas.Clear();
            _paginaActual = 0;
            picFactura.Image = null;
            lblPaginas.Text = string.Empty;
        }

        private void CrearPestanas()
        {
            for (int i = 0; i < _imagenPaginas.Count; i++)
            {
                var tab = new TabPage($"Página {i + 1}");
                tab.Tag = i;
                tabPaginas.TabPages.Add(tab);
            }

            ActualizarLabelPaginas();
        }

        private void TabPaginas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabPaginas.SelectedIndex < 0) return;
            _paginaActual = tabPaginas.SelectedIndex;
            MostrarPaginaActual();
        }

        // ── Helpers: detección de cambios sin guardar ────────────────────────

        private string SerializarPlantilla(PlantillaOcr plantilla)
        {
            using var sw = new StringWriter();
            var serializer = new XmlSerializer(typeof(PlantillaOcr));
            serializer.Serialize(sw, plantilla);
            return sw.ToString();
        }

        private bool HayCambiosSinGuardar()
        {
            string xmlActual = SerializarPlantilla(_plantilla);
            return xmlActual != _plantillaOriginalXml;
        }

        private bool ConfirmarSiHayCambiosSinGuardar(string accion)
        {
            if (!EnvironmentService.EsDesarrollo()) return true;
            if (!btnGuardar.Visible) return true;
            if (!HayCambiosSinGuardar()) return true;

            var resultado = MessageBox.Show(
                $"La plantilla de '{_nombreEmisor}' tiene cambios sin guardar.\n\n" +
                $"¿Deseas guardar los cambios antes de {accion}?",
                "Cambios sin guardar",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Cancel) return false;
            if (resultado == DialogResult.Yes)
            {
                _plantilla.Emisor = _nombreEmisor;
                _plantillaService.GuardarPlantilla(_plantilla);
                _plantillaOriginalXml = SerializarPlantilla(_plantilla);
                ActualizarHashSetEmisores();
            }
            return true;
        }

        private void ActualizarHashSetEmisores()
        {
            _emisoresConPlantilla.Clear();
            foreach (var emisor in _plantillaService.ObtenerEmisoresConPlantilla())
                _emisoresConPlantilla.Add(emisor);
            cmbEmisor.Invalidate();
        }

        private void CmbEmisor_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            string texto = cmbEmisor.Items[e.Index]?.ToString() ?? string.Empty;
            bool tienePlantilla = _emisoresConPlantilla.Contains(texto);

            Font baseFont = cmbEmisor.Font ?? this.Font;
            using var font = new Font(baseFont, tienePlantilla ? FontStyle.Bold : FontStyle.Regular);
            using var brush = new SolidBrush(e.ForeColor);

            Rectangle bounds = e.Bounds;
            Font itemFont = e.Font ?? this.Font;
            int y = bounds.Y + (bounds.Height - itemFont.Height) / 2;
            e.Graphics.DrawString(texto, font, brush, bounds.X + 3, y);
        }

        private void CmbEmisor_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!ConfirmarSiHayCambiosSinGuardar("cambiar de emisor")) return;
            CargarPlantillaEmisor();
        }

        private void CargarPlantillaEmisor()
        {
            _nombreEmisor = cmbEmisor.Text.Trim();
            if (string.IsNullOrEmpty(_nombreEmisor)) return;

            var existente = _plantillaService.ObtenerPorEmisor(_nombreEmisor);
            _plantilla = existente ?? new PlantillaOcr { Emisor = _nombreEmisor };
            _plantillaOriginalXml = SerializarPlantilla(_plantilla);

            ActualizarListaZonas();
            picFactura.Invalidate();
        }

        private void MostrarPaginaActual()
        {
            if (_paginaActual < 0 || _paginaActual >= _imagenPaginas.Count) return;

            picFactura.Image = _imagenPaginas[_paginaActual];
            _rectanguloActivo = false;

            ActualizarLabelPaginas();
            ActualizarListaZonas();
            picFactura.Invalidate();
        }

        private void ActualizarLabelPaginas()
        {
            if (_imagenPaginas.Count > 0)
                lblPaginas.Text = $"Página {_paginaActual + 1} de {_imagenPaginas.Count}";
            else
                lblPaginas.Text = string.Empty;
        }

        // ── Dibujo de rectángulos ─────────────────────────────────────────────

        private void PicFactura_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_imagenPaginas.Count == 0) return;
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

            // Asignar número de página actual (1-based)
            zonaOcr.NumPagina = _paginaActual + 1;

            // Generar nombre secuencial de zona por página
            int numZonaEnPagina = _plantilla.Zonas
                .Count(z => z.NumPagina == zonaOcr.NumPagina) + 1;
            zonaOcr.Campo = $"P{_paginaActual + 1}_Z{numZonaEnPagina}";

            _plantilla.Zonas.Add(zonaOcr);
            ActualizarListaZonas();

            MostrarTextoZona(zonaOcr);

            _rectanguloActivo = false;
            picFactura.Invalidate();
        }

        private void PicFactura_Paint(object? sender, PaintEventArgs e)
        {
            if (_imagenPaginas.Count == 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int numPaginaActual = _paginaActual + 1; // 1-based

            // Solo dibujar zonas de la página actual
            foreach (var zona in _plantilla.Zonas.Where(z => z.NumPagina == numPaginaActual))
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
            if (picFactura.Image == null)
                return new Rectangle(0, 0, picFactura.Width, picFactura.Height);

            float escalaX = (float)picFactura.Width / picFactura.Image.Width;
            float escalaY = (float)picFactura.Height / picFactura.Image.Height;
            float escala = Math.Min(escalaX, escalaY);

            int anchoReal = (int)(picFactura.Image.Width * escala);
            int altoReal = (int)(picFactura.Image.Height * escala);
            int offsetX = (picFactura.Width - anchoReal) / 2;
            int offsetY = (picFactura.Height - altoReal) / 2;

            return new Rectangle(offsetX, offsetY, anchoReal, altoReal);
        }

        // ── Gestión de zonas y OCR ───────────────────────────────────────────

        private void ActualizarListaZonas()
        {
            lstZonas.Items.Clear();
            int numPaginaActual = _paginaActual + 1;

            // Mostrar solo zonas de la página actual
            foreach (var zona in _plantilla.Zonas.Where(z => z.NumPagina == numPaginaActual))
            {
                lstZonas.Items.Add(
                    $"{zona.Campo}  " +
                    $"[X:{zona.X:F1}% Y:{zona.Y:F1}% " +
                    $"W:{zona.Ancho:F1}% H:{zona.Alto:F1}%]");
            }

            // Actualizar lblZonas con conteo por página
            int totalZonas = _plantilla.Zonas.Count;
            int zonasEnPagina = _plantilla.Zonas.Count(z => z.NumPagina == numPaginaActual);
            lblZonas.Text = $"Zonas definidas (Página {numPaginaActual}): {zonasEnPagina}" +
                            (totalZonas > zonasEnPagina ? $" / {totalZonas} total" : "");
        }

        private void LstZonas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstZonas.SelectedIndex < 0)
            {
                txtTexto.Text = string.Empty;
                txtTexto.ForeColor = Color.Black;
                return;
            }

            // Mapear índice del ListBox al índice real en la lista filtrada
            int numPaginaActual = _paginaActual + 1;
            var zonasPagina = _plantilla.Zonas
                .Where(z => z.NumPagina == numPaginaActual)
                .ToList();

            if (lstZonas.SelectedIndex < zonasPagina.Count)
            {
                var zonaSeleccionada = zonasPagina[lstZonas.SelectedIndex];
                MostrarTextoZona(zonaSeleccionada);
            }
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

                var resultado = _ocrExtractor.ExtraerTextoZonalConMetadata(_rutaPdf, zona);

                string prefijo = resultado.ObtenerPrefijo();
                string descripcion = resultado.ObtenerDescripcion();
                string contenido = resultado.EstaVacia ? "(Sin texto detectado)" : resultado.Texto.Replace("\n", "\r\n");

                txtTexto.Text = $"{prefijo} [{descripcion}] - Página {zona.NumPagina}\r\n" +
                                $"{new string('-', 40)}\r\n{contenido}";

                txtTexto.ForeColor = resultado.ObtenerColor();

                System.Diagnostics.Debug.WriteLine(
                    $"Método: {descripcion}, Página: {zona.NumPagina}, Vacía: {resultado.EstaVacia}, Longitud: {resultado.Texto.Length}");
            }
            catch (Exception ex)
            {
                txtTexto.Text = $"Error: {ex.Message}";
                txtTexto.ForeColor = Color.Red;
            }
        }

        private void BtnEliminarZona_Click(object? sender, EventArgs e)
        {
            if (lstZonas.SelectedIndex < 0) return;

            int numPaginaActual = _paginaActual + 1;
            var zonasPagina = _plantilla.Zonas
                .Where(z => z.NumPagina == numPaginaActual)
                .ToList();

            if (lstZonas.SelectedIndex >= zonasPagina.Count) return;

            var zonaAEliminar = zonasPagina[lstZonas.SelectedIndex];
            int indiceReal = _plantilla.Zonas.IndexOf(zonaAEliminar);
            if (indiceReal >= 0)
            {
                _plantilla.Zonas.RemoveAt(indiceReal);

                // Renumerar solo zonas de la misma página
                int contador = 1;
                foreach (var z in _plantilla.Zonas.Where(z => z.NumPagina == numPaginaActual))
                {
                    z.Campo = $"P{numPaginaActual}_Z{contador}";
                    contador++;
                }
            }

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
                _plantillaOriginalXml = SerializarPlantilla(_plantilla);
                ActualizarHashSetEmisores();

                int paginasConZonas = _plantilla.Zonas.Select(z => z.NumPagina).Distinct().Count();

                MessageBox.Show(
                    $"Plantilla guardada correctamente para '{_nombreEmisor}'.\n" +
                    $"Zonas definidas: {_plantilla.Zonas.Count} " +
                    $"en {paginasConZonas} página(s)",
                    "Plantilla guardada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    $"Operación bloqueada\n\n{ex.Message}",
                    "Acceso denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error guardando plantilla:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (!ConfirmarSiHayCambiosSinGuardar("cerrar")) return;
            LimpiarPaginas();
            base.Close();
        }

        private void BtnEliminarPlantilla_Click(object? sender, EventArgs e)
        {
            _nombreEmisor = cmbEmisor.Text.Trim();

            if (string.IsNullOrEmpty(_nombreEmisor))
            {
                MessageBox.Show("Selecciona un emisor primero.",
                    "Campo requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existente = _plantillaService.ObtenerPorEmisor(_nombreEmisor);
            if (existente == null)
            {
                MessageBox.Show($"No existe plantilla para '{_nombreEmisor}'.",
                    "Sin plantilla",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ConfirmarSiHayCambiosSinGuardar("eliminar la plantilla")) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar la plantilla completa del emisor?\n\n" +
                $"\"{_nombreEmisor}\"\n\n" +
                $"Se eliminarán {existente.Zonas.Count} zona(s) definida(s).",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes) return;

            try
            {
                _plantillaService.EliminarPlantilla(_nombreEmisor);
                _plantilla = new PlantillaOcr { Emisor = _nombreEmisor };
                _plantillaOriginalXml = SerializarPlantilla(_plantilla);
                ActualizarHashSetEmisores();

                LimpiarPaginas();
                ActualizarListaZonas();
                txtTexto.Text = string.Empty;
                txtTexto.ForeColor = Color.Black;

                MessageBox.Show(
                    $"Plantilla de '{_nombreEmisor}' eliminada correctamente.",
                    "Plantilla eliminada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error eliminando plantilla:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
