using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;
using FacturasApp.Core.Models;
using FacturasApp.Core.Models.EmisoresConfig;
using FacturasApp.Core.Services;
using FacturasApp.Core.Services.Parsers;
using FacturasApp.Services;
using PdfiumViewer;

namespace FacturasApp.UI;

public partial class GestionEmisoresForm : Form
{
    private readonly ConfiguracionEmisores _configuracion = new();
    // Lista completa de emisores (sin filtrar); lstEmisores sólo muestra los que
    // cumplen el filtro de txtBuscarEmisor.
    private readonly List<EmisorListItem> _todosEmisores = new();
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
    private readonly InvoiceProcessorService _invoiceService;
    private readonly PdfTextExtractor _textExtractor = new();
    private readonly OcrZonalExtractor _ocrZonalExtractor = new();
    private bool _esNuevo;
    private bool _saltarCambioSeleccion;
    private bool _cargandoPostProc;
    private bool _refrescandoCuadroPostProc;
    private bool _cargandoLinea;

    public GestionEmisoresForm()
    {
        var textExtractor = new WinFormsTextExtractor();
        _invoiceService = new InvoiceProcessorService(textExtractor);

        InitializeComponent();

        panelCentral.Resize += PanelCentral_Resize;
        CargarEmisores();
        if (lstEmisores.Items.Count > 0)
            lstEmisores.SelectedIndex = 0;
        else
        {
            // Sin emisores: el panel de post-procesamiento arranca en blanco
            // (controles condicionales ocultos y deshabilitados).
            lstPostProc.Items.Clear();
            LimpiarPanelPostProc();
        }
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

    private void ControlModificado(object? sender, EventArgs e)
    {
        if (_cargandoLinea) return;
        MarcarModificado();
    }
    private void TxtBuscarEmisor_TextChanged(object? sender, EventArgs e) => FiltrarEmisores();
    private void BtnNuevo_Click(object? sender, EventArgs e) => NuevoEmisor();
    private void BtnEliminar_Click(object? sender, EventArgs e) => EliminarEmisor();
    private void BtnClonar_Click(object? sender, EventArgs e) => ClonarEmisor();
    private void BtnCancelar_Click(object? sender, EventArgs e) => Close();

    private void TxtIdentificadores_TextChanged(object? sender, EventArgs e)
    {
        if (_cargando) return;
        MarcarModificado();
        ActualizarIndicadorIdentificadores();
    }

    private List<string> ObtenerIdentificadores() =>
        txtIdentificadores.Lines
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

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

        int numPaginas;
        PdfDocument documento;
        try
        {
            documento = PdfDocument.Load(rutaPdf);
            numPaginas = documento.PageCount;
        }
        catch (Exception ex)
        {
            if (mostrarErrores)
                MessageBox.Show($"Error al contar páginas:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        using (documento)
        {
            for (int i = 0; i < numPaginas; i++)
            {
                try
                {
                    SizeF tamano = documento.PageSizes[i];
                    int ancho = (int)Math.Ceiling(tamano.Width * 300 / 72f);
                    int alto = (int)Math.Ceiling(tamano.Height * 300 / 72f);
                    var imagen = documento.Render(
                        i, ancho, alto, 300, 300, PdfRenderFlags.ForPrinting);
                    if (imagen is Bitmap bitmap)
                        _imagenPaginas.Add(bitmap);
                    else
                        imagen.Dispose();
                }
                catch (Exception ex)
                {
                    if (mostrarErrores)
                        MessageBox.Show($"Error al renderizar página {i + 1}:\n{ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        CrearPestanas();

        if (tabPaginas.TabCount > 0)
            tabPaginas.SelectedIndex = 0;
        PanelCentral_Resize(null, EventArgs.Empty);

        MostrarPaginaActual();
        picFactura.Invalidate();

        ActualizarVistaPreviaZonal();
        ActualizarTablaValoresExtraidos();

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
                ? NormalizarSaltoLinea(textoZona)
                : "";
        }
        _sincronizando = false;

        txtRegexSource.Text = NormalizarSaltoLinea(texto);
        ActualizarIndicadorIdentificadores();
    }

    private static string NormalizarSaltoLinea(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        texto = texto.Replace("\r\n", "\n");
        texto = Regex.Replace(texto, @"(\n[\s]*)+", "\n");
        return texto.Replace("\n", "\r\n");
    }

    private void ActualizarIndicadorIdentificadores()
    {
        if (lblIndicadorEmisor == null) return;

        if (string.IsNullOrEmpty(_rutaPdf) || !File.Exists(_rutaPdf))
        {
            lblIndicadorEmisor.ForeColor = Color.Gray;
            lblIndicadorEmisor.Text = "Cargue un PDF de muestra para verificar los identificadores";
            return;
        }

        var ids = ObtenerIdentificadores();
        if (ids.Count == 0)
        {
            lblIndicadorEmisor.ForeColor = Color.Gray;
            lblIndicadorEmisor.Text = "Sin identificadores (no detecta ningún PDF)";
            return;
        }

        // Mismo procedimiento que el flujo normal: detectar emisor y ver si es el actual
        IInvoiceParser parser;
        try
        {
            parser = _invoiceService.IdentificarEmisor(_rutaPdf);
        }
        catch
        {
            lblIndicadorEmisor.ForeColor = Color.FromArgb(192, 0, 0);
            lblIndicadorEmisor.Text = "✗ No se pudo leer el PDF";
            return;
        }

        bool coincide = parser is ConfigurableParserEngine engine &&
            engine.Config.Nif != null &&
            string.Equals(engine.Config.Nif.Trim(), _emisorActual?.Nif?.Trim(),
                StringComparison.OrdinalIgnoreCase);

        lblIndicadorEmisor.ForeColor = coincide ? Color.FromArgb(0, 128, 0) : Color.FromArgb(192, 0, 0);
        lblIndicadorEmisor.Text = coincide
            ? "✓ El PDF pertenece al emisor"
            : "✗ El PDF no pertenece al emisor";
    }

    private void ActualizarTablaValoresExtraidos()
    {
        dgvValoresExtraidos.Rows.Clear();

        if (_emisorActual == null || string.IsNullOrEmpty(_rutaPdf) || !File.Exists(_rutaPdf))
            return;

        List<Factura> facturas;
        try
        {
            facturas = _invoiceService.ProcesarEmisorMuestra(_emisorActual, _rutaPdf);
        }
        catch (Exception ex)
        {
            dgvValoresExtraidos.Rows.Clear();
            dgvValoresExtraidos.Rows.Add("Error", ex.Message);
            return;
        }

        if (facturas.Count == 0) return;

        string Formato(object? v) => v switch
        {
            null => "",
            DateTime dt => dt.ToString("dd/MM/yyyy"),
            decimal d => d.ToString("N2"),
            bool b => b ? "Sí" : "No",
            _ => v.ToString() ?? ""
        };

        string Joinar(Func<Factura, object?> selector) =>
            string.Join(" | ", facturas.Select(selector).Select(Formato));

        string Comunes(Func<Factura, object?> selector) =>
            Formato(selector(facturas.First()));

        void Añadir(string atributo, Func<Factura, object?> selector) =>
            dgvValoresExtraidos.Rows.Add(atributo, Joinar(selector));

        void AñadirComun(string atributo, Func<Factura, object?> selector) =>
            dgvValoresExtraidos.Rows.Add(atributo, Comunes(selector));

        AñadirComun("Nº Factura", f => f.NumeroFactura);
        AñadirComun("Fecha", f => f.Fecha);
        AñadirComun("Emisor", f => f.Emisor?.Nombre);
        AñadirComun("NIF Emisor", f => f.Emisor?.NIF);
        AñadirComun("Receptor", f => f.Receptor?.Nombre);
        AñadirComun("NIF Receptor", f => f.Receptor?.NIF);
        AñadirComun("Concepto Ingreso", f => f.ConceptoIngreso);
        AñadirComun("Concepto Gasto", f => f.ConceptoGasto);
        Añadir("Base Imponible", f => f.BaseImponible);
        Añadir("% IVA", f => f.PorcentajeIVA);
        Añadir("Cuota IVA", f => f.CuotaIVA+f.CuotaIVACalculado);
        Añadir("% IRPF", f => f.PorcentajeIRPF);
        Añadir("Cuota IRPF", f => f.CuotaIRPF+f.CuotaIRPFCalculado);
        Añadir("% RE", f => f.PorcentajeRE);
        Añadir("Cuota RE", f => f.CuotaRE+f.CuotaRECalculado);
        Añadir("SubTotal", f => f.SubTotal);
        Añadir("Total Calculado", f => f.TotalCalculado);
        AñadirComun("Total Factura", f => f.TotalFactura);
        Añadir("Estado", f => f.Estado);
        if (facturas.Any(f => f.MensajeError?.Count > 0))
            Añadir("Mensajes", f => string.Join("; ", f.MensajeError ?? new List<string>()));
    }

    private string ExtraerTextoConModoSeleccionado()
    {
        if (string.IsNullOrEmpty(_rutaPdf)) return "";
        var modo = Enum.TryParse<ModoExtraccion>(
            cmbModoExtraccion.SelectedItem?.ToString(), true, out var modoParsed)
            ? modoParsed
            : ModoExtraccion.Ordenado;

        var modoPdfium = modo == ModoExtraccion.Simple
            ? PdfTextExtractor.ModoExtraccion.Simple
            : PdfTextExtractor.ModoExtraccion.Ordenado;

        try
        {
            return _textExtractor.ExtraerTextoSeleccionable(_rutaPdf, modoPdfium)
                ?? _invoiceService.ExtraerTexto(_rutaPdf);
        }
        catch
        {
            return _invoiceService.ExtraerTexto(_rutaPdf);
        }
    }

    private void CmbModoExtraccion_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargando || cmbModoExtraccion.SelectedIndex < 0) return;
        MarcarModificado();
        if (string.IsNullOrEmpty(_rutaPdf)) return;
        txtRegexSource.Text = ExtraerTextoConModoSeleccionado();
        ActualizarIndicadorIdentificadores();
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
            dgvZonas.Rows.Add(z.Campo, z.NumPagina, z.X, z.Y, z.Ancho, z.Alto);
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

    private void DgvMultiLineaMapeo_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
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
        _todosEmisores.Clear();
        var todos = _configuracion.CargarTodos();
        foreach (var kvp in todos.OrderBy(e => e.Key))
            _todosEmisores.Add(new EmisorListItem(kvp.Value));
        _cargando = false;
        FiltrarEmisores();
    }

    private void FiltrarEmisores()
    {
        var filtro = txtBuscarEmisor.Text.Trim();
        var nifSeleccionado = (lstEmisores.SelectedItem as EmisorListItem)?.Config.Nif;

        // Al vaciar/recargarse los ítems se dispara SelectedIndexChanged: la bandera
        // evita el aviso "¿Descartar nuevo?" mientras el usuario teclea el filtro.
        _saltarCambioSeleccion = true;
        lstEmisores.SuspendLayout();
        lstEmisores.Items.Clear();
        foreach (var item in _todosEmisores)
        {
            if (string.IsNullOrEmpty(filtro) ||
                item.Config.Nif.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                item.Config.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            {
                lstEmisores.Items.Add(item);
            }
        }
        // Conserva la selección si el emisor sigue visible; si el filtro lo oculta,
        // el panel de detalle lo mantiene cargado para no perder ediciones.
        if (nifSeleccionado != null)
            SeleccionarEmisorPorNif(nifSeleccionado);
        lstEmisores.ResumeLayout();
        _saltarCambioSeleccion = false;
    }

    private bool SeleccionarEmisorPorNif(string? nif)
    {
        if (string.IsNullOrEmpty(nif)) return false;

        for (int i = 0; i < lstEmisores.Items.Count; i++)
        {
            if (string.Equals(((EmisorListItem)lstEmisores.Items[i]).Config.Nif, nif,
                    StringComparison.OrdinalIgnoreCase))
            {
                lstEmisores.SelectedIndex = i;
                return true;
            }
        }
        return false;
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
        txtIdentificadores.Text = string.Join(Environment.NewLine, config.Identificadores);
        cmbModoExtraccion.SelectedItem = config.ModoExtraccion;
        cmbCulturaFecha.SelectedItem = config.CulturaFecha;
        txtConceptoIngreso.Text = config.ConceptoIngreso;
        txtConceptoGasto.Text = config.ConceptoGasto;

        lstCampos.Items.Clear();
        foreach (var c in config.Campos)
            lstCampos.Items.Add(c);
        ActualizarItemsCmbCampoNombre();

        lstMultiLineas.Items.Clear();
        var multi = config.MultiLinea;
        if (multi != null)
        {
            if (multi.Lineas.Count > 0)
            {
                foreach (var l in multi.Lineas)
                    lstMultiLineas.Items.Add(l);
            }
            else if (!string.IsNullOrEmpty(multi.RegexLinea))
            {
                // Compatibilidad: config antiguo con una única línea
                lstMultiLineas.Items.Add(new LineaConfig
                {
                    Regex = multi.RegexLinea,
                    MapeoCampos = multi.MapeoCampos
                        .Select(m => new MapeoCampoLinea { Nombre = m.Nombre, Grupo = m.Grupo }).ToList()
                });
            }
        }
        if (lstMultiLineas.Items.Count > 0)
            lstMultiLineas.SelectedIndex = 0;
        else
            CargarLineaEnUI(null);

        lstPostProc.Items.Clear();
        foreach (var r in config.PostProcesamiento)
            lstPostProc.Items.Add(r);
        // Sin selección y panel en blanco: evita que queden valores del emisor anterior
        // (el evento no se dispara si no había selección previa).
        lstPostProc.SelectedIndex = -1;
        LimpiarPanelPostProc();

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
        ActualizarTablaValoresExtraidos();
        ActualizarIndicadorIdentificadores();
        ActualizarVersionXml();
    }

    /// <summary>
    /// Muestra junto al botón Guardar la versión (&lt;Version&gt;, timestamp yyyyMMddHHmm)
    /// del XML del emisor que se está trabajando. Sin versión (1) o sin emisor → "—".
    /// </summary>
    private void ActualizarVersionXml()
    {
        long version = _emisorActual?.Version ?? 1;
        if (version <= 1)
        {
            lblVersionXml.Text = "Versión XML: —";
            return;
        }

        var texto = version.ToString();
        lblVersionXml.Text = DateTime.TryParseExact(texto, "yyyyMMddHHmm",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
            ? $"Versión XML: {texto} ({fecha:dd/MM/yyyy HH:mm})"
            : $"Versión XML: {texto}";
    }

    private void ActualizarItemsCmbCampoNombre()
    {
        foreach (var c in lstCampos.Items.Cast<CampoConfig>())
            if (!cmbCampoNombre.Items.Contains(c.Nombre))
                cmbCampoNombre.Items.Add(c.Nombre);
    }

    // ── LÍNEAS MULTILÍNEA ────────────────────────────────────────────────────

    private void LstMultiLineas_SelectedIndexChanged(object? sender, EventArgs e)
    {
        CargarLineaEnUI(lstMultiLineas.SelectedItem as LineaConfig);
    }

    private void CargarLineaEnUI(LineaConfig? linea)
    {
        _cargandoLinea = true;
        txtMultiLineaRegex.Text = linea?.Regex ?? "";
        dgvMultiLineaMapeo.Rows.Clear();
        if (linea != null)
        {
            foreach (var m in linea.MapeoCampos)
                dgvMultiLineaMapeo.Rows.Add(m.Nombre, m.Grupo);
        }
        _cargandoLinea = false;
    }

    private void ActualizarLineaDesdeUI(LineaConfig linea)
    {
        linea.Regex = txtMultiLineaRegex.Text.Trim();
        linea.MapeoCampos = dgvMultiLineaMapeo.Rows.Cast<DataGridViewRow>()
            .Where(r => r.Cells[0].Value != null)
            .Select(r => new MapeoCampoLinea
            {
                Nombre = r.Cells[0].Value?.ToString() ?? "",
                Grupo = int.TryParse(r.Cells[1].Value?.ToString(), out var g) ? g : 1
            }).ToList();
    }

    private void BtnMultiLineaAdd_Click(object? sender, EventArgs e)
    {
        var linea = new LineaConfig { Regex = "Nueva línea (regex)" };
        lstMultiLineas.Items.Add(linea);
        lstMultiLineas.SelectedIndex = lstMultiLineas.Items.Count - 1;
        CargarLineaEnUI(linea);
        MarcarModificado();
    }

    private void BtnMultiLineaRemove_Click(object? sender, EventArgs e)
    {
        if (lstMultiLineas.SelectedIndex < 0) return;
        lstMultiLineas.Items.RemoveAt(lstMultiLineas.SelectedIndex);
        CargarLineaEnUI(lstMultiLineas.SelectedItem as LineaConfig);
        MarcarModificado();
    }

    private void BtnMultiLineaUp_Click(object? sender, EventArgs e) => MoverLinea(-1);

    private void BtnMultiLineaDown_Click(object? sender, EventArgs e) => MoverLinea(1);

    private void MoverLinea(int desplazamiento)
    {
        int idx = lstMultiLineas.SelectedIndex;
        int nuevo = idx + desplazamiento;
        if (idx < 0 || nuevo < 0 || nuevo >= lstMultiLineas.Items.Count) return;
        var linea = lstMultiLineas.Items[idx];
        lstMultiLineas.Items.RemoveAt(idx);
        lstMultiLineas.Items.Insert(nuevo, linea);
        lstMultiLineas.SelectedIndex = nuevo;
        MarcarModificado();
    }

    private void SincronizarUIaConfig()
    {
        if (_emisorActual == null) return;
        _emisorActual.Nombre = txtNombre.Text.Trim();
        _emisorActual.Nif = txtNif.Text.Trim();
        _emisorActual.Identificadores = ObtenerIdentificadores();
        _emisorActual.ModoExtraccion = cmbModoExtraccion.SelectedItem?.ToString() ?? "Ordenado";
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
            ModoExtraccion = "Ordenado",
            CulturaFecha = "es-ES",
            ConceptoIngreso = "700",
            ConceptoGasto = "600"
        };
        _esNuevo = true;
        _emisorActual = nuevo;
        CargarEmisorEnUI(nuevo);
        txtRegexSource.Clear();
        ActualizarIndicadorIdentificadores();
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
        ActualizarVersionXml();
        CargarEmisores();
        if (lstEmisores.Items.Count > 0)
            lstEmisores.SelectedIndex = 0;
        else
        {
            // Se eliminó el último emisor: la lista de reglas y el panel de detalle
            // no deben conservar los datos del emisor borrado.
            lstPostProc.Items.Clear();
            LimpiarPanelPostProc();
        }
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
            MultiLinea = _emisorActual.MultiLinea != null ? new MultiLineaConfig
            {
                RegexLinea = _emisorActual.MultiLinea.RegexLinea,
                MapeoCampos = _emisorActual.MultiLinea.MapeoCampos
                    .Select(m => new MapeoCampoLinea { Nombre = m.Nombre, Grupo = m.Grupo }).ToList(),
                Lineas = _emisorActual.MultiLinea.Lineas
                    .Select(l => new LineaConfig
                    {
                        Regex = l.Regex,
                        MapeoCampos = l.MapeoCampos
                            .Select(m => new MapeoCampoLinea { Nombre = m.Nombre, Grupo = m.Grupo }).ToList()
                    }).ToList()
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
        Nombre = c.Nombre, Regex = c.Regex,
        ValorFijo = c.ValorFijo, UsarRegexFechaGeneral = c.UsarRegexFechaGeneral,
        UsarRegexNifGeneral = c.UsarRegexNifGeneral
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
                Sustituto = p.Accion.Sustituto,
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

        var (errorPostProc, campoPostProc) = ValidarReglasPostProc();
        if (errorPostProc != null)
        {
            tabs.SelectedTab = tabMultiLinea;
            MessageBox.Show(errorPostProc,
                "Post-procesamiento incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // Foco al dato que falta, una vez cerrado el aviso.
            campoPostProc?.Focus();
            return;
        }

        SincronizarUIaConfig();

        _emisorActual.Identificadores = ObtenerIdentificadores();

        if (lstMultiLineas.SelectedItem is LineaConfig lineaActual)
            ActualizarLineaDesdeUI(lineaActual);

        _emisorActual.MultiLinea = lstMultiLineas.Items.Count > 0 ? new MultiLineaConfig
        {
            Lineas = lstMultiLineas.Items.Cast<LineaConfig>().ToList()
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
            // Selecciona el emisor guardado en la lista refrescada; si el filtro
            // activo lo oculta, se limpia la búsqueda para que vuelva a aparecer.
            if (!SeleccionarEmisorPorNif(_emisorActual.Nif))
            {
                txtBuscarEmisor.Clear();
                SeleccionarEmisorPorNif(_emisorActual.Nif);
            }
            ActualizarVersionXml();
            MessageBox.Show("Emisor guardado correctamente.",
                "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ActualizarTablaValoresExtraidos();
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
        campo.UsarRegexFechaGeneral = tipo == "RegexFechaGeneral";
        campo.UsarRegexNifGeneral = tipo == "RegexNifGeneral";
        campo.ValorFijo = tipo == "ValorFijo" ? txtCampoValorFijo.Text.Trim() : null;
        campo.Regex = tipo == "Regex" ? txtCampoRegex.Text.Trim() : null;
    }

    private void LimpiarDetalleCampo()
    {
        cmbCampoNombre.Text = "";
        cmbCampoTipo.SelectedIndex = -1;
        txtCampoRegex.Text = "";
        txtCampoValorFijo.Text = "";
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
                : !string.IsNullOrEmpty(campo.ValorFijo) ? "ValorFijo"
                : "Regex";
            txtCampoRegex.Text = campo.Regex ?? "";
            txtCampoValorFijo.Text = campo.ValorFijo ?? "";

            if (!campo.UsarRegexFechaGeneral && !campo.UsarRegexNifGeneral
                && string.IsNullOrEmpty(campo.ValorFijo)
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
        // Refresco del cuadro (volver a insertar los ítems): no tocar el panel,
        // que ya muestra los valores que acaban de escribirse en la regla.
        if (_refrescandoCuadroPostProc) return;

        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla)
        {
            // Sin selección: el panel queda en blanco y actúa como formulario
            // de alta de una nueva regla (campos activos).
            LimpiarPanelPostProc();
            return;
        }

        MostrarReglaEnPanel(regla);
    }

    /// <summary>
    /// Vuelca la regla en el panel de detalle.
    /// </summary>
    private void MostrarReglaEnPanel(PostProcesamientoConfig regla)
    {
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

    /// <summary>
    /// Pone el panel de detalle en blanco: es el formulario de alta de una regla
    /// (sin selección en la lista), con todos sus campos activos para cumplimentar.
    /// </summary>
    private void LimpiarPanelPostProc()
    {
        _cargandoPostProc = true;
        cmbPostProcTipo.SelectedIndex = -1;
        txtPostProcCondicion.Text = "";
        cmbPostCondCampo.SelectedIndex = -1;
        txtPostCondValor.Text = "";
        cmbPostAccDestino.SelectedIndex = -1;
        txtPostAccValor.Text = "";
        cmbPostAccOrigen1.SelectedIndex = -1;
        cmbPostAccOperador.SelectedIndex = -1;
        cmbPostAccOrigen2.SelectedIndex = -1;
        txtPostSustBuscar.Text = "";
        txtPostSustPor.Text = "";
        ActualizarControlesAccion();
        _cargandoPostProc = false;
        ActualizarResumenPostProc();
    }

    private void ActualizarControlesAccion()
    {
        var tipo = PostProcesamientoConfig.NormalizarTipo(cmbPostProcTipo.SelectedItem?.ToString() ?? "");

        // "Fijar" y el combo destino conservan SIEMPRE su posición del Designer
        // (sólo cambia el texto de la etiqueta en "Sustituir": "En <campo>").
        var esSustituir = tipo == "sustituir";
        var usarDestino = tipo is "establecervalor" or "calcular" or "sustituir";
        lblPostAccDestino.Visible = usarDestino;
        lblPostAccDestino.Text = esSustituir ? "En" : "Fijar";
        // "a" sólo enlaza destino con valor; en "Calcular" lo sustituye el "=" de la fórmula.
        lblDestinoA.Visible = tipo == "establecervalor";
        cmbPostAccDestino.Visible = usarDestino;

        // Parámetros propios de "Sustituir" (cadena a buscar y su sustituto).
        // Van en las filas 278 y 342, dejando la 310 para el combo destino.
        lblPostSustBuscar.Visible = esSustituir;
        txtPostSustBuscar.Visible = esSustituir;
        lblPostSustPor.Visible = esSustituir;
        txtPostSustPor.Visible = esSustituir;

        var usarValor = tipo == "establecervalor";
        txtPostAccValor.Visible = usarValor;

        var usarFormula = tipo == "calcular";
        lblPostAccFormula.Visible = usarFormula;
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
                // La sustitución sólo tiene sentido en campos de texto (el motor no
                // acepta campos numéricos ni personalizados para este tipo).
                "sustituir" => CamposDisponibles().Where(ConfigurableParserEngine.EsCampoTexto).ToList(),
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
            // Sin selección automática: si la regla no tiene origen, el combo queda
            // vacío; nunca se muestran (ni se escriben en el modelo) valores ajenos.
        }
    }

    private void ActualizarDetalleAccionDesdeRegla(PostProcesamientoConfig regla)
    {
        var accion = regla.Accion;
        if (accion == null) return;

        if (!string.IsNullOrEmpty(accion.CampoDestino) && cmbPostAccDestino.Items.Contains(accion.CampoDestino))
            cmbPostAccDestino.SelectedItem = accion.CampoDestino;
        else
            cmbPostAccDestino.SelectedIndex = -1;

        txtPostAccValor.Text = accion.Valor;

        if (!string.IsNullOrEmpty(accion.CampoOrigen1) && cmbPostAccOrigen1.Items.Contains(accion.CampoOrigen1))
            cmbPostAccOrigen1.SelectedItem = accion.CampoOrigen1;
        else
            cmbPostAccOrigen1.SelectedIndex = -1;

        if (!string.IsNullOrEmpty(accion.Operador) && cmbPostAccOperador.Items.Contains(accion.Operador))
            cmbPostAccOperador.SelectedItem = accion.Operador;
        else
            cmbPostAccOperador.SelectedIndex = -1;

        if (!string.IsNullOrEmpty(accion.CampoOrigen2) && cmbPostAccOrigen2.Items.Contains(accion.CampoOrigen2))
            cmbPostAccOrigen2.SelectedItem = accion.CampoOrigen2;
        else
            cmbPostAccOrigen2.SelectedIndex = -1;

        txtPostSustBuscar.Text = accion.Valor;
        txtPostSustPor.Text = accion.Sustituto;
    }

    private void ActualizarResumenPostProc()
    {
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla)
        {
            var total = lstPostProc.Items.Count;
            lblPostProcResumen.ForeColor = Color.FromArgb(70, 70, 70);
            lblPostProcResumen.Text = total == 0
                ? "Elige el tipo y pulsa «+ Añadir» para crear una regla"
                : $"{total} {(total == 1 ? "regla" : "reglas")} · elige el tipo para añadir otra" +
                  " · pendiente de guardar con 💾 Guardar";
            return;
        }

        var (error, _) = ErrorReglaPostProc(regla);
        if (error != null)
        {
            lblPostProcResumen.ForeColor = Color.FromArgb(192, 0, 0);
            lblPostProcResumen.Text = "⚠ Regla incompleta: " + error;
        }
        else
        {
            lblPostProcResumen.ForeColor = Color.FromArgb(70, 70, 70);
            lblPostProcResumen.Text = "Resumen: " + regla;
        }
    }

    /// <summary>
    /// Devuelve el motivo por el que la regla no puede guardarse (o null si es válida)
    /// y el control del panel donde falta ese dato, para poder llevarle el foco.
    /// </summary>
    private (string? Error, Control? Campo) ErrorReglaPostProc(PostProcesamientoConfig regla)
    {
        var tipo = PostProcesamientoConfig.NormalizarTipo(regla.Accion?.Tipo ?? "");
        var accion = regla.Accion;

        switch (tipo)
        {
            case "invertirsigno":
                return string.IsNullOrWhiteSpace(regla.CondicionTextoContiene)
                    ? ("falta la condición (texto que debe aparecer en la factura)", txtPostProcCondicion)
                    : (null, null);

            case "establecervalor":
                if (regla.CondicionCampo == null || string.IsNullOrEmpty(regla.CondicionCampo.Campo))
                    return ("falta la condición (campo y valor esperado)", cmbPostCondCampo);
                if (string.IsNullOrEmpty(regla.CondicionCampo.Valor))
                    return ("falta el valor esperado de la condición", txtPostCondValor);
                if (accion == null || string.IsNullOrEmpty(accion.CampoDestino))
                    return ("falta el campo destino", cmbPostAccDestino);
                if (string.IsNullOrEmpty(accion.Valor))
                    return ("falta el valor a fijar", txtPostAccValor);
                return (null, null);

            case "calcular":
                if (accion == null || string.IsNullOrEmpty(accion.CampoDestino))
                    return ("falta el campo destino", cmbPostAccDestino);
                if (string.IsNullOrEmpty(accion.CampoOrigen1))
                    return ("falta el primer campo de origen de la fórmula", cmbPostAccOrigen1);
                if (string.IsNullOrEmpty(accion.CampoOrigen2))
                    return ("falta el segundo campo de origen de la fórmula", cmbPostAccOrigen2);
                return (null, null);

            case "sustituir":
                if (accion == null || string.IsNullOrEmpty(accion.CampoDestino))
                    return ("falta el campo destino", cmbPostAccDestino);
                if (string.IsNullOrEmpty(accion.Valor))
                    return ("falta la cadena a buscar", txtPostSustBuscar);
                return (null, null);

            default:
                return ("tipo de acción desconocido", cmbPostProcTipo);
        }
    }

    /// <summary>
    /// Valida todas las reglas de la lista. Si hay alguna incompleta, deja
    /// seleccionada la primera y devuelve el mensaje y el campo a corregir
    /// (null/null si todo vale).
    /// </summary>
    private (string? Error, Control? Campo) ValidarReglasPostProc()
    {
        for (int i = 0; i < lstPostProc.Items.Count; i++)
        {
            if (lstPostProc.Items[i] is not PostProcesamientoConfig regla) continue;

            var (error, campo) = ErrorReglaPostProc(regla);
            if (error == null) continue;

            lstPostProc.SelectedIndex = i;
            ActualizarResumenPostProc();
            return ($"Post-procesamiento, regla {i + 1} de {lstPostProc.Items.Count}:\n• {error}", campo);
        }

        return (null, null);
    }

    private void CmbPostProcTipo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargandoPostProc) return;

        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla)
        {
            // Modo alta: sin regla seleccionada sólo se muestran los campos que
            // pedirá la regla nueva (activos, para cumplimentarlos).
            _cargandoPostProc = true;
            ActualizarControlesAccion();
            _cargandoPostProc = false;
            return;
        }

        var tipo = cmbPostProcTipo.SelectedItem?.ToString() ?? "InvertirSigno";
        regla.Accion ??= new AccionPostProcesamiento();
        regla.Accion.Tipo = tipo;

        // Reescribe la regla desde el panel: conserva sólo los campos del tipo nuevo
        // y descarta los que no aplican (evita condiciones heredadas ocultas).
        PostProcDesdePanel();
        // Y vuelca de nuevo la regla en el panel para que quede sincronizado
        // (p. ej. el textbox de valor, que queda oculto, pasa a vacío).
        MostrarReglaEnPanel(regla);

        ActualizarResumenPostProc();
        MarcarModificado();
    }

    private void PostProcControl_Changed(object? sender, EventArgs e)
    {
        if (_cargandoPostProc) return;
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig) return;

        PostProcDesdePanel();
        ActualizarResumenPostProc();
        MarcarModificado();
    }

    /// <summary>
    /// Escribe el panel de detalle en la regla seleccionada, tocando SOLO los campos
    /// del tipo activo y limpiando los que no aplican. Así no quedan condiciones
    /// heredadas ocultas ni atributos huérfanos en el XML, y sí se puede borrar
    /// una condición (null/vacío vale como ausencia).
    /// </summary>
    private void PostProcDesdePanel()
    {
        if (lstPostProc.SelectedItem is not PostProcesamientoConfig regla) return;

        PostProcARegla(regla);
        // La regla ha cambiado: el cuadro debe reflejarlo en el acto, sin esperar
        // a que se pulse "Guardar".
        ActualizarCuadroPostProc();
    }

    /// <summary>
    /// Vuelve a insertar los ítems del cuadro de reglas para que muestre los
    /// valores actuales de cada una. El ListBox conserva el texto de cada ítem
    /// en el momento en que se añade, así que con refrescar no basta: hay que
    /// volver a insertarlos para que se llame de nuevo a ToString(). Se mantiene
    /// la regla seleccionada y no se toca el panel de detalle.
    /// </summary>
    private void ActualizarCuadroPostProc()
    {
        if (lstPostProc.Items.Count == 0) return;

        var seleccion = lstPostProc.SelectedIndex;
        _refrescandoCuadroPostProc = true;
        try
        {
            var reglas = lstPostProc.Items.Cast<PostProcesamientoConfig>().ToList();

            lstPostProc.BeginUpdate();
            lstPostProc.Items.Clear();
            foreach (var regla in reglas)
                lstPostProc.Items.Add(regla);
            lstPostProc.EndUpdate();

            if (seleccion >= 0 && seleccion < lstPostProc.Items.Count)
                lstPostProc.SelectedIndex = seleccion;
        }
        finally
        {
            _refrescandoCuadroPostProc = false;
        }
    }

    /// <summary>
    /// Vuelca el panel de detalle en la regla indicada (la seleccionada, o la nueva
    /// que se está dando de alta). Sólo se tocan los campos del tipo activo.
    /// </summary>
    private void PostProcARegla(PostProcesamientoConfig regla)
    {
        var accion = regla.Accion ??= new AccionPostProcesamiento();

        var tipo = cmbPostProcTipo.SelectedItem?.ToString();
        if (!string.IsNullOrWhiteSpace(tipo))
            accion.Tipo = tipo;

        var tipoNorm = PostProcesamientoConfig.NormalizarTipo(accion.Tipo);

        // Condiciones: sólo las del tipo activo.
        regla.CondicionTextoContiene = tipoNorm == "invertirsigno"
            ? (string.IsNullOrWhiteSpace(txtPostProcCondicion.Text)
                ? null
                : txtPostProcCondicion.Text.Trim())
            : null;

        var campoCond = cmbPostCondCampo.SelectedItem?.ToString();
        regla.CondicionCampo = tipoNorm == "establecervalor" && !string.IsNullOrEmpty(campoCond)
            ? new CondicionCampoPostProcesamiento
            {
                Campo = campoCond,
                Valor = txtPostCondValor.Text.Trim()
            }
            : null;

        // Acción: sólo los campos del tipo activo.
        accion.CampoDestino = tipoNorm is "establecervalor" or "calcular" or "sustituir"
            ? cmbPostAccDestino.SelectedItem?.ToString() ?? ""
            : "";

        // "Sustituto" sólo existe en "Sustituir"; se limpia en el resto de tipos.
        // Sin Trim(): en una sustitución los espacios pueden ser parte de la cadena.
        accion.Sustituto = tipoNorm == "sustituir" ? txtPostSustPor.Text : "";

        switch (tipoNorm)
        {
            case "establecervalor":
                accion.Valor = txtPostAccValor.Text.Trim();
                accion.CampoOrigen1 = "";
                accion.Operador = "+";
                accion.CampoOrigen2 = "";
                break;

            case "calcular":
                accion.Valor = "";
                accion.CampoOrigen1 = cmbPostAccOrigen1.SelectedItem?.ToString() ?? "";
                accion.Operador = cmbPostAccOperador.SelectedItem?.ToString() ?? "+";
                accion.CampoOrigen2 = cmbPostAccOrigen2.SelectedItem?.ToString() ?? "";
                break;

            case "sustituir":
                accion.Valor = txtPostSustBuscar.Text;
                accion.CampoOrigen1 = "";
                accion.Operador = "+";
                accion.CampoOrigen2 = "";
                break;

            default:
                accion.Valor = "";
                accion.CampoOrigen1 = "";
                accion.Operador = "+";
                accion.CampoOrigen2 = "";
                break;
        }
    }

    private void BtnPostProcAdd_Click(object? sender, EventArgs e)
    {
        // Si hay una regla seleccionada, el panel es su editor y no el formulario de
        // alta: primero se vuelcan los parámetros sobre esa regla (por si el usuario
        // los hubiera cambiado sin que se hubieran escrito) y a continuación se
        // deselecciona para dejar el panel en blanco. Así no se duplica la regla que
        // se veía y el siguiente clic, ya en blanco, crea la regla nueva.
        if (lstPostProc.SelectedItem is not null)
        {
            PostProcDesdePanel();
            lstPostProc.SelectedIndex = -1;   // dispara LstPostProc… → LimpiarPanelPostProc()
            MarcarModificado();
            cmbPostProcTipo.Focus();
            return;
        }

        // El tipo es obligatorio para dar de alta una regla.
        var tipo = cmbPostProcTipo.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(tipo))
        {
            MessageBox.Show("Selecciona el tipo de post-procesamiento antes de añadir.",
                "Tipo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbPostProcTipo.Focus();
            return;
        }

        // Regla nueva construida con TODO lo cumplimentado en el panel (condición
        // y acción del tipo elegido), no sólo con el tipo.
        var regla = new PostProcesamientoConfig
        {
            Accion = new AccionPostProcesamiento { Tipo = tipo }
        };
        PostProcARegla(regla);

        // Se comprueba antes de registrarla: si falta algo, se avisa del dato
        // concreto y se lleva el foco a su campo.
        var (error, campo) = ErrorReglaPostProc(regla);
        if (error != null)
        {
            MessageBox.Show($"No se puede añadir la regla:\n• {error}",
                "Regla incompleta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            campo?.Focus();
            return;
        }

        lstPostProc.Items.Add(regla);
        MarcarModificado();

        // El panel vuelve a estar en blanco (también el tipo) para la siguiente alta;
        // la regla recién añadida queda pendiente de guardar con 💾 Guardar.
        LimpiarPanelPostProc();
        cmbPostProcTipo.Focus();
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
            var colMatch = new DataGridViewTextBoxColumn { Name = "colMatch", HeaderText = "#", Width = 50, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, MinimumWidth = 50 };
            dgvRegexMatches.Columns.Add(colMatch);
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
