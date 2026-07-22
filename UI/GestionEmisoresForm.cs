using FacturasApp.Models;
using FacturasApp.Services;
using System.Drawing.Drawing2D;

namespace FacturasApp.UI
{
    public partial class GestionEmisoresForm : Form
    {
        private readonly EmisorService _emisorService = new();
        private readonly FieldBasedExtractor _fieldExtractor = new();
        private List<EmisorDefinicion> _emisores = new();
        private EmisorDefinicion? _emisorActual = null;
        private bool _cargandoSeleccion = false;

        // Listas temporales para edición
        private List<CampoExtraccion> _camposEditando = new();
        private List<ReglaPostProcesamiento> _reglasEditando = new();
        private List<ZonaOcrDefinicion> _zonasEditando = new();

        // ── Estado visual zonas OCR ──
        private readonly OcrZonalExtractor _ocrExtractor = new();
        private string _rutaPdfZonas = string.Empty;
        private readonly List<Bitmap> _imagenPaginasZonas = new();
        private int _paginaActualZonas = 0;
        private bool _dibujandoZonas = false;
        private Point _puntoInicioZonas;
        private Point _puntoActualZonas;
        private bool _rectanguloActivoZonas = false;

        public GestionEmisoresForm()
        {
            InitializeComponent();
            CargarEmisores();
            panelZonasIzq.Resize += (s, e) => AjustarPicFacturaZonas();
        }

        // ── Carga de datos ───────────────────────────────────────────────────

        private void CargarEmisores()
        {
            _emisores = _emisorService.ObtenerTodos();
            ActualizarLista();
        }

        private void ActualizarLista()
        {
            string filtro = txtBuscar.Text.Trim().ToLower();
            lstEmisores.Items.Clear();

            foreach (var emisor in _emisores)
            {
                if (!string.IsNullOrEmpty(filtro) &&
                    !emisor.Nombre.ToLower().Contains(filtro) &&
                    !emisor.Nif.ToLower().Contains(filtro))
                    continue;

                lstEmisores.Items.Add(new EmisorListItem(emisor));
            }
        }

        // ── Selección de emisor ──────────────────────────────────────────────

        private void LstEmisores_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cargandoSeleccion) return;

            if (lstEmisores.SelectedItem is EmisorListItem item)
            {
                _cargandoSeleccion = true;
                CargarEmisorEnEditor(item.Emisor);
                _cargandoSeleccion = false;
            }
        }

        private void CargarEmisorEnEditor(EmisorDefinicion emisor)
        {
            _emisorActual = emisor;
            _camposEditando = emisor.Campos.Select(c => new CampoExtraccion
            {
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                Regex = c.Regex,
                Grupo = c.Grupo,
                ValorFijo = c.ValorFijo,
                FormatoFecha = c.FormatoFecha,
                Cultura = c.Cultura,
                Opcional = c.Opcional,
                FormatosFecha = new List<string>(c.FormatosFecha)
            }).ToList();

            _reglasEditando = emisor.PostProcesamiento.Select(r => new ReglaPostProcesamiento
            {
                Nombre = r.Nombre,
                Condicion = r.Condicion != null ? new CondicionRegla
                {
                    Campo = r.Condicion.Campo,
                    Operador = r.Condicion.Operador,
                    Valor = r.Condicion.Valor,
                    TextoContiene = r.Condicion.TextoContiene
                } : null,
                Acciones = r.Acciones.Select(a => new AccionRegla
                {
                    Tipo = a.Tipo,
                    Campo = a.Campo,
                    Valor = a.Valor,
                    CampoFuente = a.CampoFuente
                }).ToList()
            }).ToList();

            _zonasEditando = emisor.ZonasOcr?.Zonas.Select(z => new ZonaOcrDefinicion
            {
                Campo = z.Campo,
                Pagina = z.Pagina,
                X = z.X,
                Y = z.Y,
                Ancho = z.Ancho,
                Alto = z.Alto,
                Regex = z.Regex,
                RegexRespaldo = z.RegexRespaldo,
                Opcional = z.Opcional
            }).ToList() ?? new List<ZonaOcrDefinicion>();

            // Datos básicos
            txtId.Text = emisor.Id;
            txtNombre.Text = emisor.Nombre;
            txtNif.Text = emisor.Nif;
            txtConcepto.Text = emisor.Concepto;
            txtIdentificadores.Text = string.Join("\n", emisor.Identificadores);
            cmbModoExtraccion.SelectedItem = emisor.ModoExtraccion;

            // Campos
            CargarCamposEnGrid();

            // Reglas
            CargarReglasEnGrid();

            // Zonas
            ActualizarZonas();

            lblEstado.Text = $"Emisor: {emisor.Nombre} (NIF: {emisor.Nif})";
            tabsEditor.Enabled = true;
        }

        // ── CRUD ─────────────────────────────────────────────────────────────

        private void BtnNuevo_Click(object? sender, EventArgs e)
        {
            var nuevo = new EmisorDefinicion
            {
                Id = "nuevo",
                Nombre = "Nuevo Emisor",
                Nif = "",
                Concepto = "600",
                Identificadores = new List<string>(),
                ModoExtraccion = "OrdenadoPosicion"
            };

            _emisorActual = nuevo;
            _camposEditando = new List<CampoExtraccion>();
            _reglasEditando = new List<ReglaPostProcesamiento>();
            _zonasEditando = new List<ZonaOcrDefinicion>();

            txtId.Text = nuevo.Id;
            txtNombre.Text = nuevo.Nombre;
            txtNif.Text = nuevo.Nif;
            txtConcepto.Text = nuevo.Concepto;
            txtIdentificadores.Text = "";
            cmbModoExtraccion.SelectedIndex = 0;

            CargarCamposEnGrid();
            CargarReglasEnGrid();
            ActualizarZonas();

            lblEstado.Text = "Nuevo emisor (sin guardar)";
            tabsEditor.Enabled = true;
            txtNombre.Focus();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (_emisorActual == null || string.IsNullOrEmpty(_emisorActual.Nif))
            {
                MessageBox.Show("Selecciona un emisor para eliminar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Eliminar el emisor '{_emisorActual.Nombre}' (NIF: {_emisorActual.Nif})?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                _emisorService.EliminarPorNif(_emisorActual.Nif);
                CargarEmisores();
                lblEstado.Text = "Emisor eliminado";
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (_emisorActual == null) return;

            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNif.Text))
            {
                MessageBox.Show("El NIF es obligatorio y actúa como clave única.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNif.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del emisor es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            // Verificar duplicado de NIF (solo si el NIF cambió)
            string nifNuevo = txtNif.Text.Trim().ToUpper();
            if (!nifNuevo.Equals(_emisorActual.Nif, StringComparison.OrdinalIgnoreCase))
            {
                if (_emisorService.ExisteNif(nifNuevo))
                {
                    MessageBox.Show(
                        $"Ya existe un emisor con el NIF '{nifNuevo}'.\nEl NIF es la clave única.",
                        "NIF duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNif.Focus();
                    return;
                }
            }

            // Aplicar cambios
            _emisorActual.Id = txtId.Text.Trim();
            _emisorActual.Nombre = txtNombre.Text.Trim();
            _emisorActual.Nif = nifNuevo;
            _emisorActual.Concepto = txtConcepto.Text.Trim();
            _emisorActual.Identificadores = txtIdentificadores.Text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();
            _emisorActual.ModoExtraccion = cmbModoExtraccion.SelectedItem?.ToString()
                ?? "OrdenadoPosicion";
            _emisorActual.Campos = _camposEditando;
            _emisorActual.PostProcesamiento = _reglasEditando;

            if (_zonasEditando.Count > 0)
            {
                _emisorActual.ZonasOcr ??= new ZonasOcrConfig();
                _emisorActual.ZonasOcr.Zonas = _zonasEditando;
            }
            else
            {
                _emisorActual.ZonasOcr = null;
            }

            _emisorService.GuardarEmisor(_emisorActual);
            CargarEmisores();
            lblEstado.Text = $"Guardado: {_emisorActual.Nombre}";
            lblEstado.ForeColor = Color.DarkGreen;
        }

        private void BtnCancelar_Click(object? sender, EventArgs e)
        {
            if (_emisorActual?.Nif != null)
            {
                var original = _emisorService.ObtenerPorNif(_emisorActual.Nif);
                if (original != null)
                    CargarEmisorEnEditor(original);
            }
        }

        // ── Campos ───────────────────────────────────────────────────────────

        private void CargarCamposEnGrid()
        {
            dgvCamposPruebas.Rows.Clear();
            foreach (var campo in _camposEditando)
            {
                dgvCamposPruebas.Rows.Add(campo.Nombre, campo.Tipo, campo.Regex ?? "",
                    campo.Grupo, campo.ValorFijo ?? "");
            }
        }

        private void BtnAgregarCampoPruebas_Click(object? sender, EventArgs e)
        {
            var nuevo = new CampoExtraccion { Nombre = "CampoNuevo", Tipo = "Texto" };
            _camposEditando.Add(nuevo);
            dgvCamposPruebas.Rows.Add(nuevo.Nombre, nuevo.Tipo, "", 1, "");
        }

        private void BtnEliminarCampoPruebas_Click(object? sender, EventArgs e)
        {
            if (dgvCamposPruebas.CurrentRow?.Index is int idx && idx >= 0 && idx < _camposEditando.Count)
            {
                _camposEditando.RemoveAt(idx);
                dgvCamposPruebas.Rows.RemoveAt(idx);
            }
        }

        // ── Reglas ───────────────────────────────────────────────────────────

        private void CargarReglasEnGrid()
        {
            dgvReglasPruebas.Rows.Clear();
            foreach (var regla in _reglasEditando)
            {
                string condicion = regla.Condicion != null
                    ? (regla.Condicion.TextoContiene != null
                        ? $"Texto contiene \"{regla.Condicion.TextoContiene}\""
                        : $"{regla.Condicion.Campo} {regla.Condicion.Operador} {regla.Condicion.Valor}")
                    : "(siempre)";

                string acciones = string.Join("; ",
                    regla.Acciones.Select(a => $"{a.Tipo}({a.Campo}, {a.Valor})"));

                dgvReglasPruebas.Rows.Add(regla.Nombre, condicion, acciones);
            }
        }

        private void BtnAgregarReglaPruebas_Click(object? sender, EventArgs e)
        {
            var nueva = new ReglaPostProcesamiento
            {
                Nombre = "Nueva regla",
                Condicion = new CondicionRegla { Operador = "Igual" },
                Acciones = new List<AccionRegla>()
            };
            _reglasEditando.Add(nueva);
            CargarReglasEnGrid();
        }

        private void BtnEliminarReglaPruebas_Click(object? sender, EventArgs e)
        {
            if (dgvReglasPruebas.CurrentRow?.Index is int idx && idx >= 0 && idx < _reglasEditando.Count)
            {
                _reglasEditando.RemoveAt(idx);
                CargarReglasEnGrid();
            }
        }

        // ── Zonas OCR ────────────────────────────────────────────────────────

        private void ActualizarZonas()
        {
            ActualizarListaZonasPagina();
            picFacturaZonas?.Invalidate();
        }

        private void BtnEliminarZonaLista_Click(object? sender, EventArgs e)
        {
            if (lstZonasPagina.SelectedIndex < 0) return;

            int numPag = _paginaActualZonas + 1;
            var zonasPagina = _zonasEditando
                .Where(z => z.Pagina == numPag)
                .ToList();

            if (lstZonasPagina.SelectedIndex >= zonasPagina.Count) return;

            var zonaAEliminar = zonasPagina[lstZonasPagina.SelectedIndex];
            _zonasEditando.Remove(zonaAEliminar);
            ActualizarZonas();
        }

        // ── Visual PDF zonas OCR ──────────────────────────────────────────────

        private void BtnCargarPdfZonas_Click(object? sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar PDF de muestra",
                Filter = "Archivos PDF (*.pdf)|*.pdf"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            _rutaPdfZonas = dialogo.FileName;
            LimpiarPaginasZonas();

            var bitmaps = _ocrExtractor.RenderizarPaginas(_rutaPdfZonas);
            if (bitmaps.Count == 0)
            {
                MessageBox.Show("No se pudo renderizar el PDF.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _imagenPaginasZonas.AddRange(bitmaps);

            tabPaginasZonas.TabPages.Clear();
            for (int i = 0; i < _imagenPaginasZonas.Count; i++)
            {
                var tab = new TabPage($"Página {i + 1}");
                tab.Tag = i;
                tabPaginasZonas.TabPages.Add(tab);
            }

            _paginaActualZonas = 0;
            if (tabPaginasZonas.TabCount > 0)
                tabPaginasZonas.SelectedIndex = 0;

            MostrarPaginaZonasActual();
            AjustarPicFacturaZonas();
        }

        private void TabPaginasZonas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabPaginasZonas.SelectedIndex < 0) return;
            _paginaActualZonas = tabPaginasZonas.SelectedIndex;
            MostrarPaginaZonasActual();
        }

        private void MostrarPaginaZonasActual()
        {
            if (_paginaActualZonas < 0 || _paginaActualZonas >= _imagenPaginasZonas.Count) return;

            picFacturaZonas.Image = _imagenPaginasZonas[_paginaActualZonas];
            _rectanguloActivoZonas = false;

            int total = _imagenPaginasZonas.Count;
            lblPaginasZonas.Text = total > 0
                ? $"Página {_paginaActualZonas + 1} de {total}"
                : string.Empty;

            ActualizarListaZonasPagina();
            picFacturaZonas.Invalidate();
            AjustarPicFacturaZonas();
        }

        private void ActualizarListaZonasPagina()
        {
            lstZonasPagina.Items.Clear();
            int numPag = _paginaActualZonas + 1;

            foreach (var zona in _zonasEditando.Where(z => z.Pagina == numPag))
            {
                lstZonasPagina.Items.Add(
                    $"{zona.Campo}  " +
                    $"[X:{zona.X:F1}% Y:{zona.Y:F1}% " +
                    $"W:{zona.Ancho:F1}% H:{zona.Alto:F1}%]");
            }

            int totalZonas = _zonasEditando.Count;
            int zonasEnPagina = _zonasEditando.Count(z => z.Pagina == numPag);
            lblZonasPagina.Text = $"Zonas en página {numPag}: {zonasEnPagina}" +
                                  (totalZonas > zonasEnPagina ? $" / {totalZonas} total" : "");
        }

        private void LstZonasPagina_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstZonasPagina.SelectedIndex < 0)
            {
                txtTextoZona.Text = string.Empty;
                return;
            }

            int numPag = _paginaActualZonas + 1;
            var zonasPagina = _zonasEditando
                .Where(z => z.Pagina == numPag)
                .ToList();

            if (lstZonasPagina.SelectedIndex < zonasPagina.Count)
            {
                var zonaSeleccionada = zonasPagina[lstZonasPagina.SelectedIndex];
                MostrarTextoZona(zonaSeleccionada);
            }
        }

        private void MostrarTextoZona(ZonaOcrDefinicion zonaDef)
        {
            if (string.IsNullOrEmpty(_rutaPdfZonas))
            {
                txtTextoZona.Text = "Carga un PDF primero";
                return;
            }

            try
            {
                txtTextoZona.Text = "Extrayendo...";
                Application.DoEvents();

                var zona = new ZonaOcr
                {
                    Campo = zonaDef.Campo,
                    NumPagina = zonaDef.Pagina,
                    X = zonaDef.X,
                    Y = zonaDef.Y,
                    Ancho = zonaDef.Ancho,
                    Alto = zonaDef.Alto,
                    RegexPersonalizada = zonaDef.Regex ?? string.Empty,
                    RegexRespaldo = zonaDef.RegexRespaldo,
                    Opcional = zonaDef.Opcional
                };

                var resultado = _ocrExtractor.ExtraerTextoZonalConMetadata(_rutaPdfZonas, zona);

                string prefijo = resultado.ObtenerPrefijo();
                string descripcion = resultado.ObtenerDescripcion();
                string contenido = resultado.EstaVacia
                    ? "(Sin texto detectado)"
                    : resultado.Texto.Replace("\n", "\r\n");

                txtTextoZona.Text = $"{prefijo} [{descripcion}] - Página {zona.NumPagina}\r\n" +
                                    $"{new string('-', 40)}\r\n{contenido}";

                txtTextoZona.ForeColor = resultado.ObtenerColor();
            }
            catch (Exception ex)
            {
                txtTextoZona.Text = $"Error: {ex.Message}";
                txtTextoZona.ForeColor = Color.Red;
            }
        }

        // ── Dibujo de zonas en PictureBox ─────────────────────────────────────

        private void PicFacturaZonas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_imagenPaginasZonas.Count == 0 || e.Button != MouseButtons.Left) return;
            _dibujandoZonas = true;
            _rectanguloActivoZonas = false;
            _puntoInicioZonas = e.Location;
            _puntoActualZonas = e.Location;
        }

        private void PicFacturaZonas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dibujandoZonas) return;
            _puntoActualZonas = e.Location;
            _rectanguloActivoZonas = true;
            picFacturaZonas.Invalidate();
        }

        private void PicFacturaZonas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_dibujandoZonas) return;
            _dibujandoZonas = false;

            var rect = ObtenerRectanguloNormalizado(_puntoInicioZonas, _puntoActualZonas);

            if (rect.Width < 10 || rect.Height < 10)
            {
                _rectanguloActivoZonas = false;
                picFacturaZonas.Invalidate();
                return;
            }

            var zonaOcr = ConvertirARectanglePorcentual(rect);
            zonaOcr.Pagina = _paginaActualZonas + 1;

            int numZonaEnPagina = _zonasEditando
                .Count(z => z.Pagina == zonaOcr.Pagina) + 1;
            zonaOcr.Campo = $"P{zonaOcr.Pagina}_Z{numZonaEnPagina}";

            _zonasEditando.Add(zonaOcr);
            ActualizarZonas();

            _rectanguloActivoZonas = false;
        }

        private void PicFacturaZonas_Paint(object? sender, PaintEventArgs e)
        {
            if (_imagenPaginasZonas.Count == 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int numPagActual = _paginaActualZonas + 1;

            foreach (var zona in _zonasEditando.Where(z => z.Pagina == numPagActual))
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

            if (_rectanguloActivoZonas)
            {
                var rect = ObtenerRectanguloNormalizado(_puntoInicioZonas, _puntoActualZonas);
                using var pen = new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash };
                using var brush = new SolidBrush(Color.FromArgb(40, 255, 0, 0));
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);
            }
        }

        // ── Coordenadas conversión ────────────────────────────────────────────

        private static Rectangle ObtenerRectanguloNormalizado(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        private ZonaOcrDefinicion ConvertirARectanglePorcentual(Rectangle rectPictureBox)
        {
            var areaImagen = CalcularAreaImagenEnPictureBox();

            double xReal = (rectPictureBox.X - areaImagen.X) / (double)areaImagen.Width;
            double yReal = (rectPictureBox.Y - areaImagen.Y) / (double)areaImagen.Height;
            double wReal = rectPictureBox.Width / (double)areaImagen.Width;
            double hReal = rectPictureBox.Height / (double)areaImagen.Height;

            return new ZonaOcrDefinicion
            {
                X = Math.Max(0, xReal * 100),
                Y = Math.Max(0, yReal * 100),
                Ancho = Math.Min(100, wReal * 100),
                Alto = Math.Min(100, hReal * 100)
            };
        }

        private Rectangle ConvertirAPixelesPictureBox(ZonaOcrDefinicion zona)
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
            if (picFacturaZonas.Image == null)
                return new Rectangle(0, 0, picFacturaZonas.Width, picFacturaZonas.Height);

            float escalaX = (float)picFacturaZonas.Width / picFacturaZonas.Image.Width;
            float escalaY = (float)picFacturaZonas.Height / picFacturaZonas.Image.Height;
            float escala = Math.Min(escalaX, escalaY);

            int anchoReal = (int)(picFacturaZonas.Image.Width * escala);
            int altoReal = (int)(picFacturaZonas.Image.Height * escala);
            int offsetX = (picFacturaZonas.Width - anchoReal) / 2;
            int offsetY = (picFacturaZonas.Height - altoReal) / 2;

            return new Rectangle(offsetX, offsetY, anchoReal, altoReal);
        }

        private void AjustarPicFacturaZonas()
        {
            const double proporcion = 0.7071;
            int headerH = 88;
            int anchoDisp = panelZonasIzq.ClientSize.Width;
            int altoDisp = panelZonasIzq.ClientSize.Height;
            if (anchoDisp < 10 || altoDisp < 10) return;

            int altoImg = altoDisp - headerH;
            int anchoImg = (int)(altoImg * proporcion);

            if (anchoImg > anchoDisp)
            {
                anchoImg = anchoDisp;
                altoImg = (int)(anchoDisp / proporcion);
                if (altoImg + headerH > altoDisp)
                    altoImg = altoDisp - headerH;
            }

            panelPdfContainer.Size = new Size(anchoImg, altoImg + headerH);
            panelPdfContainer.Location = new Point(0, 0);
        }

        private void LimpiarPaginasZonas()
        {
            tabPaginasZonas.TabPages.Clear();
            foreach (var img in _imagenPaginasZonas)
                img.Dispose();
            _imagenPaginasZonas.Clear();
            _paginaActualZonas = 0;
            picFacturaZonas.Image = null;
            lblPaginasZonas.Text = string.Empty;
        }

        // ── Tester (integrado en tabPruebas) ─────────────────────────────────

        private string ExtraerTextoPdfCargado()
        {
            if (string.IsNullOrEmpty(_rutaPdfZonas) || !File.Exists(_rutaPdfZonas))
                return string.Empty;

            var textExtractor = new PdfTextExtractor();
            string? texto = textExtractor.ExtraerTextoSeleccionable(
                _rutaPdfZonas, PdfTextExtractor.ModoExtraccion.OrdenadoPosicion);

            if (texto == null)
            {
                var ocr = new OcrExtractor();
                texto = ocr.ExtraerTextoConOcr(_rutaPdfZonas);
            }

            return texto ?? string.Empty;
        }

        private void BtnDetectarEmisorPruebas_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_rutaPdfZonas) || !File.Exists(_rutaPdfZonas))
            {
                MessageBox.Show("Carga un PDF en la zona de previsualización primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string texto = ExtraerTextoPdfCargado();
                if (string.IsNullOrEmpty(texto))
                {
                    MessageBox.Show("No se pudo extraer texto del PDF.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var todosLosEmisores = _emisorService.ObtenerTodos();
                var emisorDetectado = todosLosEmisores.FirstOrDefault(e =>
                    e.Identificadores.All(id =>
                        texto.Contains(id, StringComparison.OrdinalIgnoreCase)));

                if (emisorDetectado != null)
                {
                    for (int i = 0; i < lstEmisores.Items.Count; i++)
                    {
                        if (lstEmisores.Items[i] is EmisorListItem item &&
                            item.Emisor.Nif == emisorDetectado.Nif)
                        {
                            lstEmisores.SelectedIndex = i;
                            break;
                        }
                    }

                    lblEstado.Text = $"Detectado: {emisorDetectado.Nombre}";
                    lblEstado.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblEstado.Text = "No se detectó emisor conocido";
                    lblEstado.ForeColor = Color.DarkOrange;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnProbarExtraccionPruebas_Click(object? sender, EventArgs e)
        {
            if (_emisorActual == null)
            {
                MessageBox.Show("Selecciona o crea un emisor primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_rutaPdfZonas) || !File.Exists(_rutaPdfZonas))
            {
                MessageBox.Show("Carga un PDF en la zona de previsualización primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string texto = ExtraerTextoPdfCargado();
                if (string.IsNullOrEmpty(texto))
                {
                    MessageBox.Show("No se pudo extraer texto del PDF.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var resultado = _fieldExtractor.ExtraerCamposParaTest(
                    _emisorActual, texto);

                dgvResultados.Rows.Clear();

                foreach (var campo in _emisorActual.Campos)
                {
                    string valor = resultado.TryGetValue(campo.Nombre, out string? v) ? v : "";
                    bool ok = !string.IsNullOrEmpty(valor) || campo.Opcional;
                    string estado = ok ? "✓" : "✗";

                    var row = dgvResultados.Rows.Add(campo.Nombre, valor, estado);
                    var rowObj = dgvResultados.Rows[row];

                    if (!ok)
                        rowObj.DefaultCellStyle.ForeColor = Color.Red;
                    else
                        rowObj.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }

                lblEstado.Text = $"Extracción completada: {resultado.Count} campos procesados";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en extracción: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Búsqueda ─────────────────────────────────────────────────────────

        private void TxtBuscar_TextChanged(object? sender, EventArgs e)
        {
            ActualizarLista();
        }

        // ── Clase auxiliar para el ListBox ───────────────────────────────────

        private class EmisorListItem
        {
            public EmisorDefinicion Emisor { get; }
            public string DisplayText => $"{Emisor.Nombre} ({Emisor.Nif})";

            public EmisorListItem(EmisorDefinicion emisor)
            {
                Emisor = emisor;
            }

            public override string ToString() => DisplayText;
        }
    }
}
