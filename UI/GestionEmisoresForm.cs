using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using FacturasApp.Models;
using FacturasApp.Models.EmisoresConfig;
using FacturasApp.Services;
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

    public GestionEmisoresForm()
    {
        InitializeComponent();
        panelCentral.Resize += PanelCentral_Resize;
        PanelCentral_Resize(null, EventArgs.Empty);
        CargarEmisores();
        FormClosing += (_, args) =>
        {
            if (_modificado)
            {
                var r = MessageBox.Show("Hay cambios sin guardar. ¿Salir sin guardar?",
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
        tabs.SelectedIndex = 1;
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
        int topY = btnCargarPdfMuestra.Bottom + 8;
        if (tabPaginas.Visible)
            topY = tabPaginas.Bottom + 8;
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

        LimpiarPaginas();
        _rutaPdf = dialogo.FileName;

        byte[] pdfBytes;
        try { pdfBytes = File.ReadAllBytes(_rutaPdf); }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al leer PDF:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int numPaginas;
        try { numPaginas = Conversion.GetPageCount(pdfBytes); }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al contar páginas:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
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
                MessageBox.Show($"Error al renderizar página {i + 1}:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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
        tabPaginas.Visible = true;
        PanelCentral_Resize(null, EventArgs.Empty);

        MostrarPaginaActual();
        picFactura.Invalidate();
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

    // ── Sincronización entre _zonasDibujo y dgvZonas ────────────────

    private void SincronizarDgvDesdeZonas()
    {
        _sincronizando = true;
        dgvZonas.Rows.Clear();
        foreach (var z in _zonasDibujo)
            dgvZonas.Rows.Add(z.Campo, z.NumPagina, z.X, z.Y, z.Ancho, z.Alto,
                z.RegexRespaldo ?? "", z.Opcional);
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
                X = double.TryParse(r.Cells[2].Value?.ToString(), out var x) ? x : 0,
                Y = double.TryParse(r.Cells[3].Value?.ToString(), out var y) ? y : 0,
                Ancho = double.TryParse(r.Cells[4].Value?.ToString(), out var w) ? w : 0,
                Alto = double.TryParse(r.Cells[5].Value?.ToString(), out var h) ? h : 0,
                RegexRespaldo = r.Cells[6].Value?.ToString(),
                Opcional = r.Cells[7].Value?.ToString() == "True"
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
        tabPaginas.Visible = false;
    }
    private void DgvCellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_sincronizando || _cargando) return;
        MarcarModificado();
        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
    }

    private void DgvUserAddedRow(object? sender, DataGridViewRowEventArgs e) => MarcarModificado();

    private void DgvUserDeletedRow(object? sender, DataGridViewRowEventArgs e)
    {
        if (_sincronizando || _cargando) return;
        MarcarModificado();
        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
    }
    // ── CARGA Y SELECCIÓN ──────────────────────────────────────────────────────

    private void CargarEmisores()
    {
        _cargando = true;
        lstEmisores.Items.Clear();
        var todos = _configuracion.CargarTodos();
        foreach (var kvp in todos.OrderBy(e => e.Key))
            lstEmisores.Items.Add(new EmisorListItem(kvp.Value));
        if (lstEmisores.Items.Count > 0)
            lstEmisores.SelectedIndex = 0;
        _cargando = false;
        FiltrarEmisores();
    }

    private void FiltrarEmisores()
    {
        lstEmisores.Refresh();
    }

    private void LstEmisores_SelectedIndexChanged(object? sender, EventArgs e)
    {
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
                dgvZonas.Rows.Add(z.Campo, z.NumPagina, z.X, z.Y, z.Ancho, z.Alto, z.RegexRespaldo ?? "", z.Opcional);
        _cargando = false;
        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
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

        if (lstCampos.SelectedItem is CampoConfig campoActual)
            ActualizarCampoDesdeDetalle(campoActual);
        _emisorActual.Campos = lstCampos.Items.Cast<CampoConfig>().ToList();
    }

    // ── CRUD ───────────────────────────────────────────────────────────────────

    private void NuevoEmisor()
    {
        var nuevo = new EmisorConfig
        {
            Nif = "NUEVO_NIF",
            Nombre = "Nuevo Emisor",
            ModoExtraccion = "OrdenadoPosicion",
            CulturaFecha = "es-ES",
            ConceptoIngreso = "700",
            ConceptoGasto = "600"
        };
        _configuracion.Guardar(nuevo);
        CargarEmisores();
        for (int i = 0; i < lstEmisores.Items.Count; i++)
            if (((EmisorListItem)lstEmisores.Items[i]).Config.Nif == "NUEVO_NIF")
                lstEmisores.SelectedIndex = i;
    }

    private void EliminarEmisor()
    {
        if (_emisorActual == null) return;
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
    }

    private void ClonarEmisor()
    {
        if (_emisorActual == null) return;
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
            PostProcesamiento = _emisorActual.PostProcesamiento.Select(p => new PostProcesamientoConfig
            {
                Tipo = p.Tipo,
                CondicionTextoContiene = p.CondicionTextoContiene,
                CamposAfectados = new List<string>(p.CamposAfectados)
            }).ToList(),
            ZonasOcr = _emisorActual.ZonasOcr?.Select(z => new ZonaOcrConfig
            {
                Campo = z.Campo, NumPagina = z.NumPagina,
                X = z.X, Y = z.Y, Ancho = z.Ancho, Alto = z.Alto,
                RegexRespaldo = z.RegexRespaldo, Opcional = z.Opcional
            }).ToList()
        };
        _configuracion.Guardar(clon);
        CargarEmisores();
    }

    private static CampoConfig CopiarCampo(CampoConfig c) => new()
    {
        Nombre = c.Nombre, Regex = c.Regex, Grupo = c.Grupo,
        ValorFijo = c.ValorFijo, UsarRegexFechaGeneral = c.UsarRegexFechaGeneral,
        UsarRegexNifGeneral = c.UsarRegexNifGeneral, EsSuma = c.EsSuma,
        CamposSuma = c.CamposSuma?.ToList(), FormatoFecha = c.FormatoFecha
    };

    // ── GUARDAR ────────────────────────────────────────────────────────────────

    private void GuardarCambios()
    {
        if (_emisorActual == null) return;
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
                Alto = double.TryParse(r.Cells[5].Value?.ToString(), out var h) ? h : 0,
                RegexRespaldo = r.Cells[6].Value?.ToString(),
                Opcional = r.Cells[7].Value?.ToString() == "True"
            }).ToList();

        try
        {
            _configuracion.Guardar(_emisorActual);
            _modificado = false;
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
        campo.Nombre = cmbCampoNombre.Text.Trim();
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

    private void CampoDetalle_Changed(object? sender, EventArgs e)
    {
        if (_cargandoCampo) return;
        if (lstCampos.SelectedItem is CampoConfig campo)
            ActualizarCampoDesdeDetalle(campo);
        MarcarModificado();
    }

    private void LstCampos_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _cargandoCampo = true;
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
        }
        _cargandoCampo = false;
    }

    private void BtnCampoAdd_Click(object? sender, EventArgs e)
    {
        if (_emisorActual == null) return;

        if (lstCampos.SelectedItem is CampoConfig campoActual)
            ActualizarCampoDesdeDetalle(campoActual);

        var nombre = cmbCampoNombre?.Text?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            MessageBox.Show("Selecciona o escribe un nombre de campo.",
                "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var campo = new CampoConfig { Nombre = nombre };
        lstCampos.Items.Add(campo);
        lstCampos.SelectedItem = campo;
        MarcarModificado();
    }

    private void BtnCampoRemove_Click(object? sender, EventArgs e)
    {
        if (lstCampos.SelectedItem is CampoConfig campo)
        {
            lstCampos.Items.Remove(campo);
            MarcarModificado();
        }
    }

    // ── EVENTOS POST-PROCESAMIENTO ─────────────────────────────────────────────

    private void LstPostProc_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstPostProc.SelectedItem is PostProcesamientoConfig regla)
        {
            cmbPostProcTipo.SelectedItem = regla.Tipo;
            txtPostProcCondicion.Text = regla.CondicionTextoContiene ?? "";
            txtPostProcCampos.Text = string.Join(",", regla.CamposAfectados);
        }
    }

    private void BtnPostProcAdd_Click(object? sender, EventArgs e)
    {
        var regla = new PostProcesamientoConfig
        {
            Tipo = cmbPostProcTipo.SelectedItem?.ToString() ?? "InvertirSigno",
            CondicionTextoContiene = txtPostProcCondicion.Text.Trim(),
            CamposAfectados = txtPostProcCampos.Text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList()
        };
        lstPostProc.Items.Add(regla);
        MarcarModificado();
    }

    private void BtnPostProcRemove_Click(object? sender, EventArgs e)
    {
        if (lstPostProc.SelectedItem is PostProcesamientoConfig regla)
        {
            lstPostProc.Items.Remove(regla);
            MarcarModificado();
        }
    }

    // ── EVENTOS ZONAS OCR ──────────────────────────────────────────────────────

    private void BtnAbrirEditorVisual_Click(object? sender, EventArgs e)
    {
        if (_emisorActual == null) return;
        using var editor = new DefinirPlantillaForm();
        editor.ShowDialog(this);

        var plantillaService = new PlantillaOcrService();
        var plantilla = plantillaService.ObtenerPorEmisor(_emisorActual.Nombre);
        dgvZonas.Rows.Clear();
        if (plantilla != null)
        {
            _emisorActual.ZonasOcr = plantilla.Zonas.Select(z => new ZonaOcrConfig
            {
                Campo = z.Campo, NumPagina = z.NumPagina,
                X = z.X, Y = z.Y, Ancho = z.Ancho, Alto = z.Alto,
                RegexRespaldo = z.RegexRespaldo, Opcional = z.Opcional
            }).ToList();
            foreach (var z in _emisorActual.ZonasOcr)
                dgvZonas.Rows.Add(z.Campo, z.NumPagina, z.X, z.Y, z.Ancho, z.Alto, z.RegexRespaldo ?? "", z.Opcional);
        }
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
        public string DisplayText => $"{Config.Nif} — {Config.Nombre}";
        public EmisorListItem(EmisorConfig config) => Config = config;
    }
}
