using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using FacturasApp.Models;
using FacturasApp.Models.EmisoresConfig;
using FacturasApp.Services;
using FacturasApp.Services.Parsers;
using PDFtoImage;
using SkiaSharp;

namespace FacturasApp.UI;

public partial class GestionEmisoresForm : Form
{
    private readonly ConfiguracionEmisores _configuracion = new();
    private EmisorConfig? _emisorActual;
    private bool _modificado;
    private bool _cargando;
    private readonly List<Bitmap> _imagenPaginas = new();
    private int _paginaActual;
    private string? _rutaPdf;
    private readonly List<ZonaOcr> _zonasDibujo = new();
    private bool _dibujando;
    private Point _puntoInicio;
    private Point _puntoActual;
    private bool _rectanguloActivo;
    private bool _sincronizando;
    private bool _cargandoCampo;
    private CampoConfig? _campoAnterior;
    private readonly InvoiceProcessorService _invoiceService = new();
    private readonly PdfTextExtractor _textExtractor = new();
    private readonly OcrZonalExtractor _ocrZonalExtractor = new();
    private bool _esNuevo;
    private bool _saltarCambioSeleccion;
    private bool _cargandoPostProc;

    public GestionEmisoresForm()
    {
        InitializeComponent();

        panelCentral.Resize += PanelCentral_Resize;
        CargarEmisores();
        if (lstEmisores.Items.Count > 0)
            lstEmisores.SelectedIndex = 0;
        Load += (_, _) => PanelCentral_Resize(null, EventArgs.Empty);
        FormClosing += (_, args) =>
        {
            if (_modificado)
            {
                var msg = _esNuevo
                    ? "Hay un emisor nuevo sin guardar. ¿Salir sin guardar?"
                    : "Hay cambios sin guardar. ¿Salir sin guardar?";
                var r = MessageBox.Show(msg,
                    "Cambios no guardados", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) { args.Cancel = true; return; }
            }
            LimpiarPaginas();
        };
    }

    private void MarcarModificado()
    {
        if (_cargando) return;
        _modificado = true;
        if (_emisorActual != null)
            btnGuardar.Enabled = true;
    }

    private void ControlModificado(object? sender, EventArgs e) => MarcarModificado();
    private void TxtBuscarEmisor_TextChanged(object? sender, EventArgs e) => FiltrarEmisores();
    private void BtnNuevo_Click(object? sender, EventArgs e) => NuevoEmisor();
    private void BtnEliminar_Click(object? sender, EventArgs e) => EliminarEmisor();
    private void BtnClonar_Click(object? sender, EventArgs e) => ClonarEmisor();
    private void BtnCancelar_Click(object? sender, EventArgs e) => Close();

    private void BtnAddId_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtNuevoId.Text))
        {
            lstIdentificadores.Items.Add(txtNuevoId.Text.Trim());
            txtNuevoId.Clear();
            MarcarModificado();
        }
    }

    private void BtnRemoveId_Click(object? sender, EventArgs e)
    {
        if (lstIdentificadores.SelectedIndex >= 0)
        {
            lstIdentificadores.Items.RemoveAt(lstIdentificadores.SelectedIndex);
            MarcarModificado();
        }
    }

    private void BtnRegexApplyToField_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtRegexPattern.Text)) return;
        if (lstCampos.SelectedItem is CampoConfig campo)
            txtCampoRegex.Text = txtRegexPattern.Text;
    }

    private bool _ajustando;

    private void PanelCentral_Resize(object? sender, EventArgs e)
    {
        if (_ajustando) return;
        _ajustando = true;

        var panel = panelCentral;
        int panelH = panel.ClientSize.Height;
        int topY = tabPaginas.Bottom + 8;
        int availH = panelH - topY - 8;

        if (availH >= 10)
        {
            const double a4 = 0.707071;
            picFactura.Height = availH;
            picFactura.Width = (int)(availH * a4);

            panelCentral.Width = picFactura.Width + 16;

            picFactura.Left = 8;
            picFactura.Top = topY;

            btnCargarPdfMuestra.Left = 8;
            btnCargarPdfMuestra.Width = picFactura.Width;

            tabPaginas.Left = 8;
            tabPaginas.Width = picFactura.Width;
        }

        _ajustando = false;
    }

    private void BtnCargarPdfMuestra_Click(object? sender, EventArgs e)
    {
        using var dialogo = new OpenFileDialog
        {
            Title = "Seleccionar PDF de muestra",
            Filter = "Archivos PDF (*.pdf)|*.pdf"
        };
        if (dialogo.ShowDialog() != DialogResult.OK) return;

        CargarPdfMuestra(dialogo.FileName, mostrarErrores: true);
    }

    private bool CargarPdfMuestra(string rutaPdf, bool mostrarErrores)
    {
        LimpiarPaginas();
        _rutaPdf = rutaPdf;

        byte[] pdfBytes;
        try { pdfBytes = File.ReadAllBytes(rutaPdf); }
        catch (Exception ex)
        {
            if (mostrarErrores)
                MessageBox.Show($"Error al leer PDF:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        int numPaginas;
        try { numPaginas = Conversion.GetPageCount(pdfBytes); }
        catch (Exception ex)
        {
            if (mostrarErrores)
                MessageBox.Show($"Error al contar páginas:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        for (int i = 0; i < numPaginas; i++)
        {
            SKBitmap skBitmap;
            try
            {
                skBitmap = Conversion.ToImage(
                    pdfBytes,
                    page: new Index(i),
                    password: null,
                    options: new RenderOptions(Dpi: 300));
            }
            catch (Exception ex)
            {
                if (mostrarErrores)
                    MessageBox.Show($"Error al renderizar página {i + 1}:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using (skBitmap)
            {
                using var skImage = SKImage.FromBitmap(skBitmap);
                using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
                using var ms = new MemoryStream(skData.ToArray());
                _imagenPaginas.Add(new Bitmap(ms));
            }
        }

        CrearPestanas();

        if (tabPaginas.TabCount > 0)
            tabPaginas.SelectedIndex = 0;
        PanelCentral_Resize(null, EventArgs.Empty);

        MostrarPaginaActual();
        picFactura.Invalidate();

        ActualizarVistaPreviaZonal();

        return true;
    }

    private void ActualizarVistaPreviaZonal()
    {
        if (string.IsNullOrEmpty(_rutaPdf)) return;

        string texto;
        Dictionary<string, string>? resultados = null;
        try
        {
            if (_zonasDibujo.Count == 0)
            {
                texto = ExtraerTextoConModoSeleccionado();
            }
            else
            {
                var plantilla = new PlantillaOcr
                {
                    Emisor = "Previsualización",
                    Zonas = _zonasDibujo.ToList()
                };
                resultados = _ocrZonalExtractor.ExtraerZonas(_rutaPdf, plantilla);
                texto = string.Join(Environment.NewLine,
                    resultados.Select(kv => $"[{kv.Key}]: {kv.Value}"));
            }
        }
        catch
        {
            texto = _invoiceService.ExtraerTexto(_rutaPdf);
            resultados = null;
        }

        _sincronizando = true;
        foreach (DataGridViewRow r in dgvZonas.Rows)
        {
            if (r.Cells.Count < 7) continue;
            var campo = r.Cells[0].Value?.ToString() ?? "";
            r.Cells[6].Value = resultados != null && resultados.TryGetValue(campo, out var textoZona)
                ? textoZona
                : "";
        }
        _sincronizando = false;

        txtRegexSource.Text = texto;
    }

    private string ExtraerTextoConModoSeleccionado()
    {
        if (string.IsNullOrEmpty(_rutaPdf)) return "";
        var modo = Enum.TryParse<PdfTextExtractor.ModoExtraccion>(
            cmbModoExtraccion.SelectedItem?.ToString(), true, out var modoParsed)
            ? modoParsed
            : PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;
        try
        {
            return _textExtractor.ExtraerTextoSeleccionable(_rutaPdf, modo)
                ?? _invoiceService.ExtraerTexto(_rutaPdf);
        }
        catch
        {
            return _invoiceService.ExtraerTexto(_rutaPdf);
        }
    }

    private void CmbModoExtraccion_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargando || string.IsNullOrEmpty(_rutaPdf) || cmbModoExtraccion.SelectedIndex < 0) return;
        txtRegexSource.Text = ExtraerTextoConModoSeleccionado();
    }

    private void TabPaginas_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tabPaginas.SelectedIndex < 0) return;
        _paginaActual = tabPaginas.SelectedIndex;
        MostrarPaginaActual();
        picFactura.Invalidate();
    }

    // ── Dibujo de zonas (arrastrar rectángulos sobre picFactura) ────────

    private void PicFactura_Paint(object? sender, PaintEventArgs e)
    {
        if (_imagenPaginas.Count == 0) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int numPaginaActual = _paginaActual + 1;

        foreach (var zona in _zonasDibujo.Where(z => z.NumPagina == numPaginaActual))
        {
            var rect = ConvertirAPixelesPictureBox(zona);
            using var pen = new Pen(Color.FromArgb(46, 117, 182), 2);
            using var brush = new SolidBrush(Color.FromArgb(40, 46, 117, 182));
            g.FillRectangle(brush, rect);
            g.DrawRectangle(pen, rect);

            using var font = new Font("Segoe UI", 7f, FontStyle.Bold);
            g.DrawString(zona.Campo, font, Brushes.DarkBlue, rect.X + 2, rect.Y + 2);
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
        zonaOcr.NumPagina = _paginaActual + 1;

        int numZonaEnPagina = _zonasDibujo.Count(z => z.NumPagina == zonaOcr.NumPagina) + 1;
        zonaOcr.Campo = $"P{zonaOcr.NumPagina}_Z{numZonaEnPagina}";

        _zonasDibujo.Add(zonaOcr);
        SincronizarDgvDesdeZonas();

        _rectanguloActivo = false;
        picFactura.Invalidate();
        ActualizarVistaPreviaZonal();
    }

    // ── Coordenadas ──────────────────────────────────────────────────

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
            X = Math.Round(Math.Max(0, xReal * 100), 1),
            Y = Math.Round(Math.Max(0, yReal * 100), 1),
            Ancho = Math.Round(Math.Min(100, wReal * 100), 1),
            Alto = Math.Round(Math.Min(100, hReal * 100), 1)
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

    // ── Sincronización entre _zonasDibujo y dgvZonas ────────────────

    private void SincronizarDgvDesdeZonas()
    {
        _sincronizando = true;
        dgvZonas.Rows.Clear();
        foreach (var z in _zonasDibujo)
            dgvZonas.Rows.Add(z.Campo, z.NumPagina, z.X, z.Y, z.Ancho, z.Alto, "");
        _sincronizando = false;
        MarcarModificado();
    }

    private void SincronizarZonasDesdeDgv()
    {
        _zonasDibujo.Clear();
        foreach (DataGridViewRow r in dgvZonas.Rows)
        {
            if (r.Cells[0].Value == null) continue;
            _zonasDibujo.Add(new ZonaOcr
            {
                Campo = r.Cells[0].Value?.ToString() ?? "",
                NumPagina = int.TryParse(r.Cells[1].Value?.ToString(), out var p) ? p : 1,
                X = Math.Round(double.TryParse(r.Cells[2].Value?.ToString(), out var x) ? x : 0, 1),
                Y = Math.Round(double.TryParse(r.Cells[3].Value?.ToString(), out var y) ? y : 0, 1),
                Ancho = Math.Round(double.TryParse(r.Cells[4].Value?.ToString(), out var w) ? w : 0, 1),
                Alto = Math.Round(double.TryParse(r.Cells[5].Value?.ToString(), out var h) ? h : 0, 1),
            });
        }
    }

    private void CrearPestanas()
    {
        for (int i = 0; i < _imagenPaginas.Count; i++)
        {
            var tab = new TabPage($"Página {i + 1}");
            tab.Tag = i;
            tabPaginas.TabPages.Add(tab);
        }
    }

    private void MostrarPaginaActual()
    {
        if (_paginaActual < 0 || _paginaActual >= _imagenPaginas.Count) return;
        picFactura.Image = _imagenPaginas[_paginaActual];
        picFactura.Invalidate();
    }

    private void LimpiarPaginas()
    {
        tabPaginas.TabPages.Clear();
        foreach (var img in _imagenPaginas)
            img.Dispose();
        _imagenPaginas.Clear();
        _paginaActual = 0;
        _rutaPdf = null;
        picFactura.Image = null;
        _sincronizando = true;
        foreach (DataGridViewRow r in dgvZonas.Rows)
            if (r.Cells.Count >= 7)
                r.Cells[6].Value = "";
        _sincronizando = false;
        // tabPaginas remains visible (empty) to reserve layout space
    }
    private void DgvCellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_sincronizando || _cargando) return;
        MarcarModificado();
        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
        ActualizarVistaPreviaZonal();
    }

    private void DgvUserAddedRow(object? sender, DataGridViewRowEventArgs e) => MarcarModificado();

    private void DgvMultiIVAMapeo_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        if (e.Control is ComboBox combo)
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private void DgvUserDeletedRow(object? sender, DataGridViewRowEventArgs e)
    {
        if (_sincronizando || _cargando) return;
        MarcarModificado();
        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
        ActualizarVistaPreviaZonal();
    }
    // ── CARGA Y SELECCIÓN ──────────────────────────────────────────────────────

    private void CargarEmisores()
    {
        _cargando = true;
        lstEmisores.Items.Clear();
        var todos = _configuracion.CargarTodos();
        foreach (var kvp in todos.OrderBy(e => e.Key))
            lstEmisores.Items.Add(new EmisorListItem(kvp.Value));
        _cargando = false;
        FiltrarEmisores();
    }

    private void FiltrarEmisores()
    {
        lstEmisores.Refresh();
    }

    private void LstEmisores_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargando || _saltarCambioSeleccion) return;

        if (_esNuevo && _modificado)
        {
            var r = MessageBox.Show(
                "Hay un emisor nuevo sin guardar. ¿Descartarlo y cargar el seleccionado?",
                "Descartar nuevo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes)
            {
                _saltarCambioSeleccion = true;
                lstEmisores.SelectedIndex = -1;
                _saltarCambioSeleccion = false;
                return;
            }
            _esNuevo = false;
            _modificado = false;
        }

        if (lstEmisores.SelectedItem is EmisorListItem item)
            CargarEmisorEnUI(item.Config);
    }

    private void CargarEmisorEnUI(EmisorConfig config)
    {
        _cargando = true;
        _modificado = false;
        _emisorActual = config;
        btnGuardar.Enabled = false;

        txtNombre.Text = config.Nombre;
        txtNif.Text = config.Nif;
        lstIdentificadores.Items.Clear();
        foreach (var id in config.Identificadores)
            lstIdentificadores.Items.Add(id);
        cmbModoExtraccion.SelectedItem = config.ModoExtraccion;
        cmbCulturaFecha.SelectedItem = config.CulturaFecha;
        txtConceptoIngreso.Text = config.ConceptoIngreso;
        txtConceptoGasto.Text = config.ConceptoGasto;

        lstCampos.Items.Clear();
        foreach (var c in config.Campos)
            lstCampos.Items.Add(c);
        ActualizarItemsCmbCampoNombre();

        chkMultiIVA.Checked = config.MultiLineaIVA?.Habilitado ?? false;
        txtMultiIVARegex.Text = config.MultiLineaIVA?.RegexLinea ?? "";
        dgvMultiIVAMapeo.Rows.Clear();
        if (config.MultiLineaIVA?.MapeoCampos != null)
            foreach (var m in config.MultiLineaIVA.MapeoCampos)
                dgvMultiIVAMapeo.Rows.Add(m.Nombre, m.Grupo);

        lstPostProc.Items.Clear();
        foreach (var r in config.PostProcesamiento)
            lstPostProc.Items.Add(r);

        dgvZonas.Rows.Clear();
        if (config.ZonasOcr != null)
            foreach (var z in config.ZonasOcr)
                dgvZonas.Rows.Add(z.Campo, z.NumPagina, z.X, z.Y, z.Ancho, z.Alto);

        var rutaPdf = config.RutaPdfMuestra;
        if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
            LimpiarPaginas();
        else
            CargarPdfMuestra(rutaPdf, mostrarErrores: false);

        _cargando = false;
        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
        ActualizarVistaPreviaZonal();
    }

    private void ActualizarItemsCmbCampoNombre()
    {
        foreach (var c in lstCampos.Items.Cast<CampoConfig>())
            if (!cmbCampoNombre.Items.Contains(c.Nombre))
                cmbCampoNombre.Items.Add(c.Nombre);
    }

    private void SincronizarUIaConfig()
    {
        if (_emisorActual == null) return;
        _emisorActual.Nombre = txtNombre.Text.Trim();
        _emisorActual.Nif = txtNif.Text.Trim();
        _emisorActual.Identificadores = lstIdentificadores.Items.Cast<string>().ToList();
        _emisorActual.ModoExtraccion = cmbModoExtraccion.SelectedItem?.ToString() ?? "OrdenadoPosicion";
        _emisorActual.CulturaFecha = cmbCulturaFecha.SelectedItem?.ToString() ?? "es-ES";
        _emisorActual.ConceptoIngreso = txtConceptoIngreso.Text.Trim();
        _emisorActual.ConceptoGasto = txtConceptoGasto.Text.Trim();
        _emisorActual.RutaPdfMuestra = _rutaPdf ?? string.Empty;

        if (lstCampos.SelectedItem is CampoConfig campoActual)
        {
            var nombre = cmbCampoNombre.Text.Trim();
            if (!string.IsNullOrEmpty(nombre))
                campoActual.Nombre = nombre;
            ActualizarCampoDesdeDetalle(campoActual);
        }
        _emisorActual.Campos = lstCampos.Items.Cast<CampoConfig>().ToList();
        _emisorActual.PostProcesamiento = lstPostProc.Items.Cast<PostProcesamientoConfig>().ToList();
    }

    // ── CRUD ───────────────────────────────────────────────────────────────────

    private void NuevoEmisor()
    {
        if (_esNuevo && _modificado)
        {
            var r = MessageBox.Show(
                "Hay un emisor nuevo sin guardar. ¿Descartarlo y crear otro?",
                "Descartar nuevo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
        }

        var nuevo = new EmisorConfig
        {
            Nif = "NUEVO_NIF",
            Nombre = "Nuevo Emisor",
            ModoExtraccion = "OrdenadoPosicion",
            CulturaFecha = "es-ES",
            ConceptoIngreso = "700",
            ConceptoGasto = "600"
        };
        _esNuevo = true;
        _emisorActual = nuevo;
        CargarEmisorEnUI(nuevo);
        _modificado = true;
        btnGuardar.Enabled = true;
        _saltarCambioSeleccion = true;
        lstEmisores.ClearSelected();
        _saltarCambioSeleccion = false;
    }

    private void EliminarEmisor()
    {
        if (_emisorActual == null) return;

        if (_esNuevo)
        {
            MessageBox.Show("Guarda el emisor antes de eliminarlo.", "Operación no válida",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.Equals(_emisorActual.Nif, "General", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("No se puede eliminar el emisor genérico.", "Operación no permitida",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var confirm = MessageBox.Show(
            $"¿Eliminar '{_emisorActual.Nombre}' (NIF: {_emisorActual.Nif})?",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        _configuracion.Eliminar(_emisorActual.Nif);
        _emisorActual = null;
        CargarEmisores();
        if (lstEmisores.Items.Count > 0)
            lstEmisores.SelectedIndex = 0;
    }

    private void ClonarEmisor()
    {
        if (_emisorActual == null) return;

        if (_esNuevo && _modificado)
        {
            var r = MessageBox.Show(
                "Hay un emisor nuevo sin guardar. ¿Descartarlo y clonar el emisor actual?",
                "Descartar nuevo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
        }

        SincronizarUIaConfig();

        var clon = new EmisorConfig
        {
            Nif = _emisorActual.Nif + "_COPY",
            Nombre = _emisorActual.Nombre + " (Copia)",
            Identificadores = new List<string>(_emisorActual.Identificadores),
            ModoExtraccion = _emisorActual.ModoExtraccion,
            ConceptoIngreso = _emisorActual.ConceptoIngreso,
            ConceptoGasto = _emisorActual.ConceptoGasto,
            CulturaFecha = _emisorActual.CulturaFecha,
            Campos = _emisorActual.Campos.Select(CopiarCampo).ToList(),
            MultiLineaIVA = _emisorActual.MultiLineaIVA != null ? new MultiLineaIVAConfig
            {
                Habilitado = _emisorActual.MultiLineaIVA.Habilitado,
                RegexLinea = _emisorActual.MultiLineaIVA.RegexLinea,
                MapeoCampos = _emisorActual.MultiLineaIVA.MapeoCampos
                    .Select(m => new MapeoCampoMultiIVA { Nombre = m.Nombre, Grupo = m.Grupo }).ToList()
            } : null,
            PostProcesamiento = _emisorActual.PostProcesamiento.Select(CopiarPostProc).ToList(),
            ZonasOcr = _emisorActual.ZonasOcr?.Select(z => new ZonaOcrConfig
            {
                Campo = z.Campo, NumPagina = z.NumPagina,
                X = z.X, Y = z.Y, Ancho = z.Ancho, Alto = z.Alto
            }).ToList()
        };
        _esNuevo = true;
        _emisorActual = clon;
        CargarEmisorEnUI(clon);
        _modificado = true;
        btnGuardar.Enabled = true;
        _saltarCambioSeleccion = true;
        lstEmisores.ClearSelected();
        _saltarCambioSeleccion = false;
    }

    private static CampoConfig CopiarCampo(CampoConfig c) => new()
    {
        Nombre = c.Nombre, Regex = c.Regex, Grupo = c.Grupo,
        ValorFijo = c.ValorFijo, UsarRegexFechaGeneral = c.UsarRegexFechaGeneral,
        UsarRegexNifGeneral = c.UsarRegexNifGeneral, EsSuma = c.EsSuma,
        CamposSuma = c.CamposSuma?.ToList(), FormatoFecha = c.FormatoFecha
    };

    private static PostProcesamientoConfig CopiarPostProc(PostProcesamientoConfig p)
    {
        return new PostProcesamientoConfig
        {
            CondicionTextoContiene = p.CondicionTextoContiene,
            CondicionCampo = p.CondicionCampo == null ? null : new CondicionCampoPostProcesamiento
            {
                Campo = p.CondicionCampo.Campo,
                Valor = p.CondicionCampo.Valor
            },
            Accion = p.Accion == null ? null : new AccionPostProcesamiento
            {
                Tipo = p.Accion.Tipo,
                CampoDestino = p.Accion.CampoDestino,
                Valor = p.Accion.Valor,
                CampoOrigen1 = p.Accion.CampoOrigen1,
                Operador = p.Accion.Operador,
                CampoOrigen2 = p.Accion.CampoOrigen2
            }
        };
    }

    // ── GUARDAR ────────────────────────────────────────────────────────────────

    private void GuardarCambios()
    {
        if (_emisorActual == null) return;

        var nifNuevo = txtNif.Text.Trim();
        if (string.IsNullOrWhiteSpace(nifNuevo))
        {
            MessageBox.Show("El NIF no puede estar vacío.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Validate NIF uniqueness (exclude self for existing emitters)
        var nifAnterior = _emisorActual.Nif;
        var todos = _configuracion.CargarTodos();
        if (todos.ContainsKey(nifNuevo) &&
            (_esNuevo || !string.Equals(nifAnterior, nifNuevo, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Ya existe un emisor con NIF '{nifNuevo}'.\nEl NIF debe ser único.",
                "NIF duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SincronizarUIaConfig();

        _emisorActual.Identificadores = lstIdentificadores.Items.Cast<string>().ToList();

        _emisorActual.MultiLineaIVA = chkMultiIVA.Checked ? new MultiLineaIVAConfig
        {
            Habilitado = true,
            RegexLinea = txtMultiIVARegex.Text,
            MapeoCampos = dgvMultiIVAMapeo.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Cells[0].Value != null)
                .Select(r => new MapeoCampoMultiIVA
                {
                    Nombre = r.Cells[0].Value?.ToString() ?? "",
                    Grupo = int.TryParse(r.Cells[1].Value?.ToString(), out var g) ? g : 1
                }).ToList()
        } : null;

        _emisorActual.ZonasOcr = dgvZonas.Rows.Cast<DataGridViewRow>()
            .Where(r => r.Cells[0].Value != null)
            .Select(r => new ZonaOcrConfig
            {
                Campo = r.Cells[0].Value?.ToString() ?? "",
                NumPagina = int.TryParse(r.Cells[1].Value?.ToString(), out var p) ? p : 1,
                X = double.TryParse(r.Cells[2].Value?.ToString(), out var x) ? x : 0,
                Y = double.TryParse(r.Cells[3].Value?.ToString(), out var y) ? y : 0,
                Ancho = double.TryParse(r.Cells[4].Value?.ToString(), out var w) ? w : 0,
                Alto = double.TryParse(r.Cells[5].Value?.ToString(), out var h) ? h : 0
            }).ToList();

        try
        {
            _configuracion.Guardar(_emisorActual, _esNuevo ? null : nifAnterior);
            _esNuevo = false;
            _modificado = false;
            btnGuardar.Enabled = false;
            CargarEmisores();
            // Find and select the saved emitter in the refreshed list
            for (int i = 0; i < lstEmisores.Items.Count; i++)
            {
                if (((EmisorListItem)lstEmisores.Items[i]).Config.Nif == _emisorActual.Nif)
                {
                    lstEmisores.SelectedIndex = i;
                    break;
                }
            }
            MessageBox.Show("Emisor guardado correctamente.",
                "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        GuardarCambios();
    }

    // ── EVENTOS CAMPOS ─────────────────────────────────────────────────────────

    private void ActualizarCampoDesdeDetalle(CampoConfig campo)
    {
        var tipo = cmbCampoTipo.SelectedItem?.ToString() ?? "Regex";
        campo.EsSuma = tipo == "Suma";
        campo.UsarRegexFechaGeneral = tipo == "RegexFechaGeneral";
        campo.UsarRegexNifGeneral = tipo == "RegexNifGeneral";
        campo.ValorFijo = tipo == "ValorFijo" ? txtCampoValorFijo.Text.Trim() : null;
        campo.Regex = tipo == "Regex" ? txtCampoRegex.Text.Trim() : null;
        campo.Grupo = int.TryParse(txtCampoGrupo.Text, out var g) ? g : 1;
        campo.FormatoFecha = string.IsNullOrWhiteSpace(txtCampoFormatoFecha.Text)
            ? null : txtCampoFormatoFecha.Text.Trim();
        campo.CamposSuma = tipo == "Suma" && !string.IsNullOrWhiteSpace(txtCampoCamposSuma.Text)
            ? txtCampoCamposSuma.Text.Split(',').Select(s => s.Trim()).ToList()
            : null;
    }

    private void LimpiarDetalleCampo()
    {
        cmbCampoNombre.Text = "";
        cmbCampoTipo.SelectedIndex = -1;
        txtCampoRegex.Text = "";
        txtCampoGrupo.Text = "1";
        txtCampoValorFijo.Text = "";
        txtCampoFormatoFecha.Text = "";
        txtCampoCamposSuma.Text = "";
    }

    private void CampoDetalle_Changed(object? sender, EventArgs e)
    {
        if (_cargandoCampo) return;
        if (lstCampos.SelectedItem is CampoConfig campo)
            ActualizarCampoDesdeDetalle(campo);
        MarcarModificado();
    }

    private void CmbCampoNombre_TextChanged(object? sender, EventArgs e)
    {
        if (_cargandoCampo) return;

        var text = cmbCampoNombre.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(text))
        {
            if (lstCampos.SelectedItem != null)
            {
                if (_campoAnterior != null)
                    ActualizarCampoDesdeDetalle(_campoAnterior);
                _campoAnterior = null;
                _cargandoCampo = true;
                lstCampos.SelectedItem = null;
                _cargandoCampo = false;
            }
            return;
        }

        var match = lstCampos.Items.Cast<CampoConfig>()
            .FirstOrDefault(c => c.Nombre.Equals(text, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            if (lstCampos.SelectedItem != match)
                lstCampos.SelectedItem = match;
        }
        else
        {
            if (lstCampos.SelectedItem != null)
            {
                if (_campoAnterior != null)
                    ActualizarCampoDesdeDetalle(_campoAnterior);
                _campoAnterior = null;
                _cargandoCampo = true;
                lstCampos.SelectedItem = null;
                cmbCampoNombre.Text = text;
                _cargandoCampo = false;
            }
        }
    }

    private void LstCampos_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_campoAnterior != null)
            ActualizarCampoDesdeDetalle(_campoAnterior);

        _cargandoCampo = true;
        _campoAnterior = lstCampos.SelectedItem as CampoConfig;

        if (lstCampos.SelectedItem is CampoConfig campo)
        {
            if (cmbCampoNombre != null) cmbCampoNombre.Text = campo.Nombre;
            cmbCampoTipo.SelectedItem = campo.UsarRegexFechaGeneral ? "RegexFechaGeneral"
                : campo.UsarRegexNifGeneral ? "RegexNifGeneral"
                : campo.EsSuma ? "Suma"
                : !string.IsNullOrEmpty(campo.ValorFijo) ? "ValorFijo"
                : "Regex";
            txtCampoRegex.Text = campo.Regex ?? "";
            txtCampoGrupo.Text = campo.Grupo.ToString();
            txtCampoValorFijo.Text = campo.ValorFijo ?? "";
            txtCampoFormatoFecha.Text = campo.FormatoFecha ?? "";
            txtCampoCamposSuma.Text = campo.CamposSuma != null ? string.Join(",", campo.CamposSuma) : "";

            if (!campo.UsarRegexFechaGeneral && !campo.UsarRegexNifGeneral
                && !campo.EsSuma && string.IsNullOrEmpty(campo.ValorFijo)
                && !string.IsNullOrEmpty(txtCampoRegex.Text))
            {
                txtRegexPattern.Text = txtCampoRegex.Text;
            }
        }
        else
        {
            LimpiarDetalleCampo();
        }
        _cargandoCampo = false;
    }

    private void BtnCampoAdd_Click(object? sender, EventArgs e)
    {
        if (_emisorActual == null) return;

        var nombre = cmbCampoNombre?.Text?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            MessageBox.Show("Escribe un nombre de campo.",
                "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (lstCampos.Items.Cast<CampoConfig>().Any(c =>
            c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Ya existe un campo con el nombre '{nombre}'.",
                "Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var campo = new CampoConfig { Nombre = nombre };
        ActualizarCampoDesdeDetalle(campo);
        campo.Nombre = nombre;

        lstCampos.Items.Add(campo);
        lstCampos.SelectedItem = campo;
        ActualizarItemsCmbCampoNombre();

        _cargandoCampo = true;
        LimpiarDetalleCampo();
        if (cmbCampoNombre != null) cmbCampoNombre.Text = nombre;
        _cargandoCampo = false;

        MarcarModificado();
    }

    private void BtnCampoRemove_Click(object? sender, EventArgs e)
    {
        if (lstCampos.SelectedItem is CampoConfig campo)
        {
            lstCampos.Items.Remove(campo);
            ActualizarItemsCmbCampoNombre();
            MarcarModificado();
        }
    }

    // ── EVENTOS POST-PROCESAMIENTO ─────────────────────────────────────────────

    private List<string> CamposDisponibles()
    {
        var campos = new List<string>
        {
            "BaseImponible", "CuotaIVA", "CuotaIRPF", "CuotaRE",
            "TotalFactura", "SubTotal", "PorcentajeIVA", "PorcentajeIRPF", "PorcentajeRE",
            "NumeroFactura", "ReceptorNombre", "ReceptorNif", "EmisorNombre", "EmisorNif",
            "ConceptoIngreso", "ConceptoGasto"
        };
        foreach (var c in lstCampos.Items.Cast<CampoConfig>())
            if (!campos.Contains(c.Nombre, StringComparer.OrdinalIgnoreCase))
                campos.Add(c.Nombre);
        return campos;
    }

    private static void RellenarComboCampos(ComboBox combo, IEnumerable<string> campos)
    {
        combo.Items.Clear();
        foreach (var c in campos)
            combo.Items.Add(c);
    }

    private void LstPostProc_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla)
            return;

        _cargandoPostProc = true;

        var tipo = regla.Accion?.Tipo;
        cmbPostProcTipo.SelectedItem = tipo;
        if (cmbPostProcTipo.SelectedItem == null && !string.IsNullOrEmpty(tipo))
        {
            cmbPostProcTipo.SelectedItem = cmbPostProcTipo.Items.Cast<string>()
                .FirstOrDefault(i => PostProcesamientoConfig.NormalizarTipo(i) == PostProcesamientoConfig.NormalizarTipo(tipo));
        }
        ActualizarControlesAccion();

        txtPostProcCondicion.Text = regla.CondicionTextoContiene ?? "";

        var condCampo = regla.CondicionCampo;
        if (condCampo == null || string.IsNullOrEmpty(condCampo.Campo))
        {
            cmbPostCondCampo.SelectedIndex = -1;
            txtPostCondValor.Text = "";
        }
        else
        {
            if (!cmbPostCondCampo.Items.Contains(condCampo.Campo))
                cmbPostCondCampo.Items.Add(condCampo.Campo);
            cmbPostCondCampo.SelectedItem = condCampo.Campo;
            txtPostCondValor.Text = condCampo.Valor;
        }

        ActualizarDetalleAccionDesdeRegla(regla);
        ActualizarResumenPostProc();

        _cargandoPostProc = false;
    }

    private void ActualizarControlesAccion()
    {
        var tipo = PostProcesamientoConfig.NormalizarTipo(cmbPostProcTipo.SelectedItem?.ToString() ?? "");

        var usarDestino = tipo is "establecervalor" or "calcular";
        lblPostAccDestino.Visible = usarDestino;
        lblDestinoA.Visible = usarDestino;
        cmbPostAccDestino.Visible = usarDestino;

        var usarValor = tipo == "establecervalor";
        txtPostAccValor.Visible = usarValor;

        var usarFormula = tipo == "calcular";
        cmbPostAccOrigen1.Visible = usarFormula;
        cmbPostAccOperador.Visible = usarFormula;
        cmbPostAccOrigen2.Visible = usarFormula;

        var usarCondTexto = tipo == "invertirsigno";
        lblPostProcCond.Visible = usarCondTexto;
        txtPostProcCondicion.Visible = usarCondTexto;

        var usarCondCampo = tipo == "establecervalor";
        lblPostCondCampo.Visible = usarCondCampo;
        cmbPostCondCampo.Visible = usarCondCampo;
        lblPostCondValor.Visible = usarCondCampo;
        txtPostCondValor.Visible = usarCondCampo;
        if (usarCondCampo)
        {
            var actual = cmbPostCondCampo.SelectedItem?.ToString();
            RellenarComboCampos(cmbPostCondCampo, CamposDisponibles());
            if (actual != null && cmbPostCondCampo.Items.Contains(actual))
                cmbPostCondCampo.SelectedItem = actual;
        }

        if (usarDestino)
        {
            var actual = cmbPostAccDestino.SelectedItem?.ToString();
            var campos = tipo switch
            {
                "calcular" => CamposDisponibles().Where(ConfigurableParserEngine.EsCampoNumerico).ToList(),
                _ => CamposDisponibles()
            };
            RellenarComboCampos(cmbPostAccDestino, campos);
            if (actual != null && cmbPostAccDestino.Items.Contains(actual))
                cmbPostAccDestino.SelectedItem = actual;
        }

        if (usarFormula)
        {
            var camposNum = CamposDisponibles().Where(ConfigurableParserEngine.EsCampoNumerico).ToList();
            var a1 = cmbPostAccOrigen1.SelectedItem?.ToString();
            var a2 = cmbPostAccOrigen2.SelectedItem?.ToString();
            RellenarComboCampos(cmbPostAccOrigen1, camposNum);
            RellenarComboCampos(cmbPostAccOrigen2, camposNum);
            if (a1 != null && cmbPostAccOrigen1.Items.Contains(a1)) cmbPostAccOrigen1.SelectedItem = a1;
            if (a2 != null && cmbPostAccOrigen2.Items.Contains(a2)) cmbPostAccOrigen2.SelectedItem = a2;
            if (cmbPostAccOrigen1.Items.Count > 0 && cmbPostAccOrigen1.SelectedIndex < 0)
                cmbPostAccOrigen1.SelectedIndex = 0;
            if (cmbPostAccOrigen2.Items.Count > 0 && cmbPostAccOrigen2.SelectedIndex < 0)
                cmbPostAccOrigen2.SelectedIndex = 0;
        }
    }

    private void ActualizarDetalleAccionDesdeRegla(PostProcesamientoConfig regla)
    {
        var accion = regla.Accion;
        if (accion == null) return;

        if (!string.IsNullOrEmpty(accion.CampoDestino) && cmbPostAccDestino.Items.Contains(accion.CampoDestino))
            cmbPostAccDestino.SelectedItem = accion.CampoDestino;

        txtPostAccValor.Text = accion.Valor;

        if (!string.IsNullOrEmpty(accion.CampoOrigen1) && cmbPostAccOrigen1.Items.Contains(accion.CampoOrigen1))
            cmbPostAccOrigen1.SelectedItem = accion.CampoOrigen1;
        else if (cmbPostAccOrigen1.Items.Count > 0 && string.IsNullOrEmpty(accion.CampoOrigen1))
            cmbPostAccOrigen1.SelectedIndex = 0;

        if (cmbPostAccOperador.Items.Contains(accion.Operador))
            cmbPostAccOperador.SelectedItem = accion.Operador;

        if (!string.IsNullOrEmpty(accion.CampoOrigen2) && cmbPostAccOrigen2.Items.Contains(accion.CampoOrigen2))
            cmbPostAccOrigen2.SelectedItem = accion.CampoOrigen2;
        else if (cmbPostAccOrigen2.Items.Count > 0 && string.IsNullOrEmpty(accion.CampoOrigen2))
            cmbPostAccOrigen2.SelectedIndex = 0;
    }

    private void ActualizarResumenPostProc()
    {
        lblPostProcResumen.Text = lstPostProc.SelectedItem is PostProcesamientoConfig regla
            ? "Resumen: " + regla
            : "";
    }

    private void CmbPostProcTipo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargandoPostProc) return;
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla) return;

        var tipo = cmbPostProcTipo.SelectedItem?.ToString() ?? "InvertirSigno";
        regla.Accion ??= new AccionPostProcesamiento();
        regla.Accion.Tipo = tipo;

        _cargandoPostProc = true;
        ActualizarControlesAccion();
        _cargandoPostProc = false;

        lstPostProc.Refresh();
        ActualizarResumenPostProc();
        MarcarModificado();
    }

    private void PostProcControl_Changed(object? sender, EventArgs e)
    {
        if (_cargandoPostProc) return;
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla) return;

        var condicion = txtPostProcCondicion.Text.Trim();
        if (!string.IsNullOrWhiteSpace(condicion))
            regla.CondicionTextoContiene = condicion;

        var condCampo = cmbPostCondCampo.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(condCampo))
        {
            regla.CondicionCampo ??= new CondicionCampoPostProcesamiento();
            regla.CondicionCampo.Campo = condCampo;
            regla.CondicionCampo.Valor = txtPostCondValor.Text.Trim();
        }

        var accion = regla.Accion;
        if (accion != null)
        {
            accion.CampoDestino = cmbPostAccDestino.SelectedItem?.ToString() ?? accion.CampoDestino;
            accion.Valor = txtPostAccValor.Text.Trim();
            accion.CampoOrigen1 = cmbPostAccOrigen1.SelectedItem?.ToString() ?? "";
            accion.Operador = cmbPostAccOperador.SelectedItem?.ToString() ?? "+";
            accion.CampoOrigen2 = cmbPostAccOrigen2.SelectedItem?.ToString() ?? "";
        }

        lstPostProc.Refresh();
        ActualizarResumenPostProc();
        MarcarModificado();
    }

    private void TxtPostProcCondicion_Leave(object? sender, EventArgs e)
    {
        if (_cargandoPostProc) return;
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla) return;
        if (string.IsNullOrWhiteSpace(txtPostProcCondicion.Text))
            txtPostProcCondicion.Text = regla.CondicionTextoContiene ?? "";
    }

    private void BtnPostProcAdd_Click(object? sender, EventArgs e)
    {
        var tipoNorm = PostProcesamientoConfig.NormalizarTipo(cmbPostProcTipo.SelectedItem?.ToString() ?? "");
        if (tipoNorm == "invertirsigno")
        {
            if (string.IsNullOrWhiteSpace(txtPostProcCondicion.Text))
            {
                MessageBox.Show("Para 'Invertir Signo' la condición (texto en factura) es obligatoria.",
                    "Condición requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPostProcCondicion.Focus();
                return;
            }
        }
        else if (tipoNorm == "establecervalor")
        {
            if (cmbPostCondCampo.SelectedItem == null || string.IsNullOrWhiteSpace(txtPostCondValor.Text))
            {
                MessageBox.Show("Para 'Establecer Valor' debes indicar el campo de la condición y el valor esperado.",
                    "Condición requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPostCondCampo.Focus();
                return;
            }
        }

        var tipo = cmbPostProcTipo.SelectedItem?.ToString() ?? "InvertirSigno";
        var regla = new PostProcesamientoConfig
        {
            Accion = new AccionPostProcesamiento { Tipo = tipo }
        };
        if (tipoNorm == "invertirsigno")
            regla.CondicionTextoContiene = txtPostProcCondicion.Text.Trim();
        else if (tipoNorm == "establecervalor")
            regla.CondicionCampo = new CondicionCampoPostProcesamiento
            {
                Campo = cmbPostCondCampo.SelectedItem!.ToString()!,
                Valor = txtPostCondValor.Text.Trim()
            };

        lstPostProc.Items.Add(regla);
        lstPostProc.SelectedItem = regla;
        MarcarModificado();
    }

    private void BtnPostProcRemove_Click(object? sender, EventArgs e)
    {
        if (lstPostProc.SelectedItem is PostProcesamientoConfig regla)
        {
            var idx = lstPostProc.SelectedIndex;
            lstPostProc.Items.Remove(regla);
            if (lstPostProc.Items.Count > 0)
                lstPostProc.SelectedIndex = Math.Min(idx, lstPostProc.Items.Count - 1);
            MarcarModificado();
        }
    }

    private void BtnPostProcUp_Click(object? sender, EventArgs e)
    {
        var idx = lstPostProc.SelectedIndex;
        if (idx <= 0) return;
        var item = lstPostProc.Items[idx];
        lstPostProc.Items.RemoveAt(idx);
        lstPostProc.Items.Insert(idx - 1, item);
        lstPostProc.SelectedIndex = idx - 1;
        MarcarModificado();
    }

    private void BtnPostProcDown_Click(object? sender, EventArgs e)
    {
        var idx = lstPostProc.SelectedIndex;
        if (idx < 0 || idx >= lstPostProc.Items.Count - 1) return;
        var item = lstPostProc.Items[idx];
        lstPostProc.Items.RemoveAt(idx);
        lstPostProc.Items.Insert(idx + 1, item);
        lstPostProc.SelectedIndex = idx + 1;
        MarcarModificado();
    }

    // ── PROBAR REGEX ───────────────────────────────────────────────────────────

    private void EjecutarRegex(object? sender, EventArgs e)
    {
        var texto = txtRegexSource.Text;
        var patron = txtRegexPattern.Text;

        if (string.IsNullOrEmpty(patron) || string.IsNullOrEmpty(texto))
        {
            lblRegexMatchCount.Text = "";
            dgvRegexMatches.Columns.Clear();
            dgvRegexMatches.Rows.Clear();
            return;
        }

        try
        {
            var regex = new Regex(patron, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regex.Matches(texto);

            lblRegexMatchCount.Text = $"{matches.Count} match(es)";

            dgvRegexMatches.Columns.Clear();
            dgvRegexMatches.Columns.Add("colMatch", "#");
            if (matches.Count > 0)
            {
                for (int i = 0; i < matches[0].Groups.Count; i++)
                    dgvRegexMatches.Columns.Add($"colG{i}", i == 0 ? "Match completo" : $"Grupo {i}");
            }

            dgvRegexMatches.Rows.Clear();
            for (int m = 0; m < matches.Count; m++)
            {
                var row = new List<object> { m + 1 };
                for (int g = 0; g < matches[m].Groups.Count; g++)
                    row.Add(matches[m].Groups[g].Value);
                dgvRegexMatches.Rows.Add(row.ToArray());
            }
        }
        catch (RegexParseException)
        {
            lblRegexMatchCount.Text = "⚠ Regex inválida";
            dgvRegexMatches.Columns.Clear();
            dgvRegexMatches.Rows.Clear();
        }
    }

    // ── CLASE AUXILIAR ─────────────────────────────────────────────────────────

    private class EmisorListItem
    {
        public EmisorConfig Config { get; }
        public string DisplayText => $"{Config.Nif} - {Config.Nombre}";
        public EmisorListItem(EmisorConfig config) => Config = config;
    }
}
