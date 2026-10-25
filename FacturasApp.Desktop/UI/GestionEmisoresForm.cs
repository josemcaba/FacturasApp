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
    private bool _esNuevo;
    private bool _saltarCambioSeleccion;
    private bool _cargandoPostProc;
    private bool _refrescandoCuadroPostProc;
    private bool _cargandoLinea;
    // Pestaña «Columnas Excel»: mapeo de cabeceras → campos internos.
    private bool _columnasModificadas;
    private readonly ConfiguracionColumnasExcel _configColumnas = new();
    private DataGridView _dgvColumnasExcel = null!;
    private TabPage _tabColumnasExcel = null!;
    private Button _btnGuardarColumnas = null!;
    private Button _btnRestaurarColumnas = null!;
    private Label _lblEstadoColumnas = null!;
    // Última consulta VIES en memoria, claveada por el NIF exacto consultado:
    // evita repetir la llamada a la red al guardar tras pulsar "Verificar VIES".
    private string? _viesNifConsultado;
    private ViesResultado? _viesUltimoResultado;
    // La pestaña «Campos» trabaja en modo ALTA cuando el nombre elegido en la
    // combo todavía no está definido para el emisor, y en modo EDICIÓN cuando
    // ya lo está (la lista queda seleccionada con ese campo).
    private bool _campoEnAlta;
    // Texto extraído del PDF de muestra, tal y como lo ve el motor (zonal si
    // hay zonas, completo si no). El OCR sólo depende del PDF, del modo y de
    // las zonas — no de las expresiones—, así que se cachea con esa firma y se
    // reutiliza tanto en la consola del probador como en la tabla «Valores
    // extraídos»: iterar sobre una regex no vuelve a lanzar Tesseract.
    private (string Texto, bool ViaOcr, bool Zonal)? _textoMuestra;
    private string? _firmaTextoMuestra;
    // Refrescos pendientes que resuelve el mismo temporizador de 500 ms:
    // «previa» re-extrae el texto (puede correr OCR) y «tabla» sólo lo reparsea.
    private bool _pendientePrevia;
    private bool _pendienteTabla;
    // Aviso transitorio (se muestra en lblTipoAyuda) cuando el tipo de
    // extracción guardado o elegido no es válido para el nombre del campo.
    private string? _avisoTipoPendiente;

    public GestionEmisoresForm()
    {
        var textExtractor = new WinFormsTextExtractor();
        _invoiceService = new InvoiceProcessorService(textExtractor);

        InitializeComponent();

        InicializarPestanaColumnasExcel();

        // InitializeComponent puede disparar handlers de controles (p. ej.
        // CellValueChanged al fijar HeaderText de las columnas de dgvZonas) con
        // _cargando todavía a false; sin este reset el formulario se abriría
        // "modificado" y preguntaría por cambios pendientes sin haber entrado.
        _modificado = false;
        btnGuardar.Enabled = false;

        panelCentral.Resize += PanelCentral_Resize;
        CargarEmisores();
        // Al entrar no se selecciona ningún emisor: el usuario elige cuál abrir
        // de la lista o pulsa "+ Nuevo". El panel de detalle queda vacío (los
        // controles arrancan así en blanco) y el post-procesamiento en blanco.
        lstPostProc.Items.Clear();
        LimpiarPanelPostProc();
        Load += (_, _) => PanelCentral_Resize(null, EventArgs.Empty);
        FormClosing += (_, args) =>
        {
            if (_modificado || _columnasModificadas)
            {
                var msg = _columnasModificadas && !_modificado
                    ? "Hay cambios en las columnas de Excel sin guardar. ¿Salir sin guardar?"
                    : "Hay cambios sin guardar. ¿Salir sin guardar?";
                var r = MessageBox.Show(msg,
                    "Cambios no guardados", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) { args.Cancel = true; return; }
            }
            LimpiarPaginas();
        };
    }

    private void InicializarPestanaColumnasExcel()
    {
        var tab = new TabPage("Columnas Excel (importación)");
        _tabColumnasExcel = tab;
        tab.Padding = new Padding(6);

        _dgvColumnasExcel = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };

        var colCampo = new DataGridViewTextBoxColumn
        {
            Name = "Campo",
            HeaderText = "Campo interno",
            ReadOnly = true,
            FillWeight = 30
        };
        var colCabeceras = new DataGridViewTextBoxColumn
        {
            Name = "Cabeceras",
            HeaderText = "Cabeceras aceptadas (separadas por coma)",
            FillWeight = 70
        };
        _dgvColumnasExcel.Columns.Add(colCampo);
        _dgvColumnasExcel.Columns.Add(colCabeceras);

        var panelBotones = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        _btnGuardarColumnas = new Button
        {
            Text = "Guardar columnas",
            Location = new Point(6, 6),
            Size = new Size(150, 28)
        };
        _btnRestaurarColumnas = new Button
        {
            Text = "Restaurar por defecto",
            Location = new Point(162, 6),
            Size = new Size(170, 28)
        };
        _lblEstadoColumnas = new Label
        {
            Text = "Listo",
            Location = new Point(340, 10),
            AutoSize = true
        };
        panelBotones.Controls.Add(_btnGuardarColumnas);
        panelBotones.Controls.Add(_btnRestaurarColumnas);
        panelBotones.Controls.Add(_lblEstadoColumnas);

        tab.Controls.Add(_dgvColumnasExcel);
        tab.Controls.Add(panelBotones);

        tabs.TabPages.Add(tab);

        _dgvColumnasExcel.CellValueChanged += (_, _) =>
        {
            if (!_cargando) { _columnasModificadas = true; _btnGuardarColumnas.Enabled = true; }
        };
        _dgvColumnasExcel.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_dgvColumnasExcel.IsCurrentCellDirty)
                _dgvColumnasExcel.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };

        _btnGuardarColumnas.Click += BtnGuardarColumnas_Click;
        _btnRestaurarColumnas.Click += BtnRestaurarColumnas_Click;

        CargarColumnasEnGrid();
        _btnGuardarColumnas.Enabled = false;

        tabs.SelectedIndexChanged += (_, _) =>
        {
            if (tabs.SelectedTab == _tabColumnasExcel)
                LimpiarPaginas();
            ActualizarVersionXml();
        };
    }

    private void CargarColumnasEnGrid()
    {
        _cargando = true;
        try
        {
            var config = _configColumnas.Cargar();
            _dgvColumnasExcel.Rows.Clear();
            foreach (var col in config.Columnas)
            {
                _dgvColumnasExcel.Rows.Add(col.Campo, string.Join(", ", col.Cabeceras));
            }
            _columnasModificadas = false;
            _btnGuardarColumnas.Enabled = false;
            _lblEstadoColumnas.Text = $"Cargadas {config.Columnas.Count} columnas (v{config.Version})";
            if (tabs.SelectedTab == _tabColumnasExcel)
                ActualizarVersionXml();
        }
        finally
        {
            _cargando = false;
        }
    }

    private void BtnGuardarColumnas_Click(object? sender, EventArgs e)
    {
        try
        {
            var config = _configColumnas.Cargar();
            // Reconstruimos la lista a partir del grid, manteniendo el orden.
            var nuevas = new List<FacturasApp.Core.Models.ColumnaExcelConfig>();
            foreach (DataGridViewRow row in _dgvColumnasExcel.Rows)
            {
                var campo = row.Cells["Campo"].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(campo)) continue;
                var texto = row.Cells["Cabeceras"].Value?.ToString() ?? "";
                var cabeceras = texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.Trim().ToLowerInvariant())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                nuevas.Add(new FacturasApp.Core.Models.ColumnaExcelConfig
                {
                    Campo = campo,
                    Cabeceras = cabeceras
                });
            }
            config.Columnas = nuevas;
            _configColumnas.Guardar(config);

            // Validación: campos obligatorios sin variantes.
            var obligatorios = new[] { "NumeroFactura", "Fecha", "BaseImponible", "TotalFactura" };
            var sinVariantes = nuevas
                .Where(c => obligatorios.Contains(c.Campo, StringComparer.OrdinalIgnoreCase) && (c.Cabeceras == null || c.Cabeceras.Count == 0))
                .Select(c => c.Campo)
                .ToList();
            if (sinVariantes.Count > 0)
                MessageBox.Show($"Aviso: los siguientes campos obligatorios no tienen ninguna cabecera: {string.Join(", ", sinVariantes)}",
                    "Columnas Excel", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _columnasModificadas = false;
            _btnGuardarColumnas.Enabled = false;
            _lblEstadoColumnas.Text = $"Guardado (v{config.Version})";
            ActualizarVersionXml();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar: {ex.Message}", "Columnas Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnRestaurarColumnas_Click(object? sender, EventArgs e)
    {
        var r = MessageBox.Show("¿Restaurar las columnas por defecto? Se perderán los cambios no guardados.",
            "Columnas Excel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;
        try
        {
            // Borramos el archivo para que Cargar() lo resembrara con defaults.
            var ruta = _configColumnas.RutaArchivo;
            if (File.Exists(ruta)) File.Delete(ruta);
            var sentinel = Path.Combine(Path.GetDirectoryName(ruta)!, "ColumnasExcel.eliminado");
            if (File.Exists(sentinel)) File.Delete(sentinel);
            CargarColumnasEnGrid();
            _lblEstadoColumnas.Text = "Restaurado por defecto";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Columnas Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MarcarModificado()
    {
        // Sin emisor cargado no hay nada que guardar: evita que teclear en el
        // panel vacío deje la ventana "modificada" y dispare el aviso al cerrar.
        if (_cargando || _emisorActual == null) return;
        _modificado = true;
        btnGuardar.Enabled = true;
    }

    private void ControlModificado(object? sender, EventArgs e)
    {
        if (_cargandoLinea) return;
        MarcarModificado();
    }

    // El CharacterCasing.Upper del Designer convierte lo tecleado; este handler
    // cubre además el pegado y el arrastre de texto, que no pasan por esa
    // conversión. Se aplica también al cargar: si el XML tenía minúsculas, el
    // campo queda en mayúsculas y se persiste así al guardar.
    private bool _convirtiendoMayusculas;

    private void TxtMayusculas_TextChanged(object? sender, EventArgs e)
    {
        if (_convirtiendoMayusculas || sender is not TextBox tb) return;
        var texto = tb.Text;
        var mayusculas = texto.ToUpperInvariant();
        if (texto == mayusculas) return;
        // Asignar Text dispara de nuevo TextChanged: la bandera evita la recursión.
        _convirtiendoMayusculas = true;
        var caret = tb.SelectionStart;
        tb.Text = mayusculas;
        tb.SelectionStart = Math.Min(caret, tb.Text.Length);
        _convirtiendoMayusculas = false;
    }

    // Igual que TxtMayusculas_TextChanged pero específico del nombre del emisor:
    // además de mayúsculas quita las tildes de las vocales (no se admiten
    // vocales acentuadas), conservando la ñ y otros diacríticos no vocales.
    // El límite de 40 caracteres lo impone el MaxLength del Designer (bloquea
    // tecleo y pegado); aquí se recorta además por si acaso, porque asignar
    // Text por código no respeta MaxLength (cubre la carga de XMLs antiguos).
    private bool _normalizandoNombre;

    private void TxtNombre_TextChanged(object? sender, EventArgs e)
    {
        if (_normalizandoNombre || sender is not TextBox tb) return;
        var texto = QuitarTildesVocales(tb.Text.ToUpperInvariant());
        if (texto.Length > 40) texto = texto[..40];
        if (texto == tb.Text) return;
        // Asignar Text dispara de nuevo TextChanged: la bandera evita la recursión.
        _normalizandoNombre = true;
        var caret = tb.SelectionStart;
        tb.Text = texto;
        tb.SelectionStart = Math.Min(caret, tb.Text.Length);
        _normalizandoNombre = false;
    }

    private static string QuitarTildesVocales(string texto) => texto
        .Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I')
        .Replace('Ó', 'O').Replace('Ú', 'U').Replace('Ü', 'U')
        .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
        .Replace('ó', 'o').Replace('ú', 'u').Replace('ü', 'u');

    private void TxtBuscarEmisor_TextChanged(object? sender, EventArgs e) => FiltrarEmisores();
    private void BtnNuevo_Click(object? sender, EventArgs e) => NuevoEmisor();
    private void BtnEliminar_Click(object? sender, EventArgs e) => EliminarEmisor();
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

    /// <summary>
    /// Copia la expresión probada (txtRegexPattern) al campo que se está
    /// editando o definiendo. Es el único camino «probador → campo»: así se
    /// puede experimentar sin alterar el campo por tecleo accidental, y si no
    /// hay campo o el tipo no admite regex se avisa en lugar de no hacer nada.
    /// </summary>
    private void BtnRegexApplyToField_Click(object? sender, EventArgs e)
    {
        var patron = txtRegexPattern.Text;
        if (string.IsNullOrWhiteSpace(patron)) return;

        if (lstCampos.SelectedItem == null && !_campoEnAlta)
        {
            MessageBox.Show(
                "Elige primero un nombre en «Nombre»: podrás editar un campo " +
                "ya definido o dar de alta uno nuevo con ese nombre.",
                "Ningún campo seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            cmbCampoNombre.Focus();
            return;
        }

        if (cmbCampoTipo.SelectedItem?.ToString() != "Regex")
        {
            MessageBox.Show(
                "La expresión sólo puede asignarse a campos con tipo de extracción «Regex».\n\n" +
                "«RegexFechaGeneral» y «RegexNifGeneral» usan detectores predefinidos " +
                "y «ValorFijo» no usa expresión.",
                "Tipo de extracción no compatible", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // CampoDetalle_Changed aplica el valor al campo (o lo deja listo si es alta).
        txtCampoRegex.Text = patron;
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
            Title = "Seleccionar PDF o imagen de muestra",
            Filter = FormatosDocumento.FiltroDialogo
        };
        if (dialogo.ShowDialog() != DialogResult.OK) return;

        CargarPdfMuestra(dialogo.FileName, mostrarErrores: true);
    }

    private bool CargarPdfMuestra(string rutaPdf, bool mostrarErrores)
    {
        LimpiarPaginas();
        _rutaPdf = rutaPdf;

        if (FormatosDocumento.EsImagen(rutaPdf))
        {
            // JPG/PNG: la imagen es la única «página»; las zonas en % y la
            // caché de texto funcionan igual que con un PDF escaneado.
            var bitmap = OcrBase.CargarImagen(rutaPdf);
            if (bitmap == null)
            {
                if (mostrarErrores)
                    MessageBox.Show("No se pudo cargar la imagen de muestra.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _rutaPdf = null;
                ActualizarAyudasMuestra();
                ActualizarVistaPreviaZonal();
                ActualizarIndicadorIdentificadores();
                return false;
            }

            _imagenPaginas.Add(bitmap);
        }
        else
        {
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
                // Sin PDF de muestra utilizable: la vista previa y el probador
                // de regex quedan como al inicio (sin fuente de texto).
                _rutaPdf = null;
                ActualizarAyudasMuestra();
                ActualizarVistaPreviaZonal();
                ActualizarIndicadorIdentificadores();
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
                        if (imagen is Bitmap bitmapPagina)
                            _imagenPaginas.Add(bitmapPagina);
                        else
                            imagen.Dispose();
                    }
                    catch (Exception ex)
                    {
                        if (mostrarErrores)
                            MessageBox.Show($"Error al renderizar página {i + 1}:\n{ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _rutaPdf = null;
                        ActualizarAyudasMuestra();
                        ActualizarVistaPreviaZonal();
                        ActualizarIndicadorIdentificadores();
                        return false;
                    }
                }
            }
        }

        CrearPestanas();

        if (tabPaginas.TabCount > 0)
            tabPaginas.SelectedIndex = 0;
        PanelCentral_Resize(null, EventArgs.Empty);

        MostrarPaginaActual();
        picFactura.Invalidate();

        // El texto del PDF de muestra se extrae UNA sola vez y queda cacheado:
        // la consola del probador y la tabla «Valores extraídos» se alimentan
        // de esa caché, de forma que editar expresiones ya no vuelve a correr
        // el OCR (antes había dos pasadas de Tesseract por cada carga).
        ActualizarAyudasMuestra();

        ActualizarVistaPreviaZonal();
        ActualizarIndicadorIdentificadores();

        // Cargar un PDF de muestra cambia <RutaPdfMuestra>: es un cambio más a
        // guardar (si no se marcaba, al cambiar de emisor se perdía en silencio).
        // Las cargas automáticas al abrir/descartar un emisor no llegan aquí
        // marcadas: MarcarModificado no hace nada mientras _cargando está a true.
        MarcarModificado();

        return true;
    }

    /// <summary>
    /// Refresca la consola «Texto Extraido» (la fuente del probador) y la tabla
    /// «Valores extraídos», todo a partir del texto cacheado del motor. Al
    /// terminar la tabla ya está al día, así que no hace falta recalcularla
    /// aparte.
    /// </summary>
    private void ActualizarVistaPreviaZonal()
    {
        ActualizarCabeceraTextoExtraido();

        if (string.IsNullOrEmpty(_rutaPdf) || _emisorActual == null)
        {
            // Sin PDF de muestra el probador se queda sin fuente de texto
            // (antes podía seguir mostrando la del emisor anterior).
            _textoMuestra = null;
            _firmaTextoMuestra = null;
            txtRegexSource.Clear();
            ActualizarTablaValoresExtraidos();
            return;
        }

        string texto = "";
        try
        {
            texto = ObtenerTextoMuestra().Texto;
        }
        catch
        {
            // El motivo aparece en «Valores extraídos» al refrescar la tabla.
        }

        // La consola prueba exactamente contra el texto que va a parsear
        // ConfigurableParserEngine (mismos «[Campo]: valor»), de forma que lo
        // que salga aquí es lo que saldrá en la tabla.
        txtRegexSource.Text = NormalizarSaltoLinea(texto);
        ActualizarTablaValoresExtraidos();
    }

    /// <summary>
    /// Texto del PDF de muestra tal y como lo ve el motor (zonal si hay zonas,
    /// completo si no). Se cachea contra la firma de lo que lo determina —ruta,
    /// modo y zonas— para que sólo se vuelva a extraer (OCR incluido) cuando
    /// cambia alguno de esos datos.
    /// </summary>
    private (string Texto, bool ViaOcr, bool Zonal) ObtenerTextoMuestra()
    {
        if (_emisorActual == null || string.IsNullOrEmpty(_rutaPdf))
            return ("", false, false);

        var firma = FirmaTextoMuestra();
        if (_textoMuestra.HasValue && firma == _firmaTextoMuestra)
            return _textoMuestra.Value;

        var resultado = _invoiceService.ExtraerTextoMuestra(_emisorActual, _rutaPdf, PlantillaDeZonas());
        _textoMuestra = resultado;
        _firmaTextoMuestra = firma;
        return resultado;
    }

    private string FirmaTextoMuestra() =>
        // Modo: el de la config (lo que usa el motor) y el de la combo (lo que
        // se muestra); coinciden, pero cambiar cualquiera de los dos invalida.
        $"{_rutaPdf}|{_emisorActual?.ModoExtraccion}|{ModoSeleccionado()}|" +
        string.Join(";", _zonasDibujo.Select(z =>
            $"{z.Campo}|{z.NumPagina}|{z.X}|{z.Y}|{z.Ancho}|{z.Alto}"));

    /// <summary>
    /// Plantilla con las zonas que hay AHORA en la rejilla (todavía sin
    /// guardarse en el XML): así la extracción de la muestra ve las mismas zonas
    /// que se están dibujando, igual que la vista previa. Con la rejilla vacía
    /// devuelve una plantilla SIN zonas para que no se cuelen las del XML (p. ej.
    /// al borrar todas antes de guardar).
    /// </summary>
    private PlantillaOcr PlantillaDeZonas() => new()
    {
        Emisor = _emisorActual?.Nombre ?? "Previsualización",
        Zonas = _zonasDibujo.ToList()
    };

    /// <summary>
    /// Cabecera del cuadro «Texto Extraido»: indica si se muestran zonas o el
    /// texto completo y qué modo de extracción se ha aplicado.
    /// </summary>
    private void ActualizarCabeceraTextoExtraido()
    {
        if (lblSeparadorRegex == null) return;

        var modo = ModoSeleccionado();
        lblSeparadorRegex.Text = _zonasDibujo.Count > 0
            ? $"━━━ Texto Extraido · zonal ({_zonasDibujo.Count} zonas) · {modo} ━━━"
            : $"━━━ Texto Extraido · completo · {modo} ━━━";
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
            lblIndicadorEmisor.Text = "Cargue un PDF o imagen de muestra para verificar los identificadores";
            return;
        }

        var ids = ObtenerIdentificadores();
        if (ids.Count == 0)
        {
            lblIndicadorEmisor.ForeColor = Color.Gray;
            lblIndicadorEmisor.Text = "Sin identificadores (no detecta ningún documento)";
            return;
        }

        // Mismo procedimiento que el flujo normal: detectar emisor y ver si es el actual.
        // El texto se extrae UNA sola vez y sirve tanto para identificar (con la config
        // EN EDICIÓN: los identificadores tecleados en el textbox, quizá aún sin guardar)
        // como para detectar si OTRO XML distinto del actual identifica también el
        // documento de muestra (colisión de identificadores).
        IInvoiceParser? parser;
        IReadOnlyList<EmisorConfig> otrosQueIdentifican;
        try
        {
            EmisorConfig? configEditada = null;
            if (_emisorActual != null)
            {
                configEditada = CopiarConfig(_emisorActual);
                configEditada.Identificadores = ids;
            }

            var texto = _invoiceService.ExtraerTextoIdentificacion(_rutaPdf);
            parser = _invoiceService.IdentificarTexto(texto, configEditada);
            // Otros emisores = los que identifican el documento con NIF distinto
            // del actual (la copia ya guardada del propio emisor no cuenta).
            otrosQueIdentifican = _invoiceService.EmisoresQueIdentifican(texto)
                .Where(c => !string.Equals(c.Nif?.Trim(), _emisorActual?.Nif?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch
        {
            lblIndicadorEmisor.ForeColor = Color.FromArgb(192, 0, 0);
            lblIndicadorEmisor.Text = "✗ No se pudo leer el documento";
            return;
        }

        // Colisión: otro XML (anterior o posterior al actual en el directorio de
        // emisores) reclama el documento de muestra. Aviso con prioridad sobre el
        // ✓/✗, que se recupera en cuanto la colisión desaparece.
        if (otrosQueIdentifican.Count > 0)
        {
            var primero = otrosQueIdentifican[0];
            var etiqueta = !string.IsNullOrEmpty(primero.Nombre)
                ? (string.IsNullOrEmpty(primero.Nif) ? primero.Nombre : $"{primero.Nombre} ({primero.Nif})")
                : primero.Nif ?? "";
            var extra = otrosQueIdentifican.Count > 1
                ? $" … y {otrosQueIdentifican.Count - 1} más"
                : "";
            lblIndicadorEmisor.ForeColor = Color.FromArgb(192, 0, 0);
            lblIndicadorEmisor.Text =
                $"✗ Otro emisor identifica también este documento: {etiqueta}{extra}";
            return;
        }

        if (parser == null)
        {
            // Sin config XML que identifique el documento no hay extracción posible
            lblIndicadorEmisor.ForeColor = Color.FromArgb(192, 0, 0);
            lblIndicadorEmisor.Text = "✗ Ningún XML identifica el documento (sin config no se extrae)";
            return;
        }

        bool coincide = parser is ConfigurableParserEngine engine &&
            engine.Config.Nif != null &&
            string.Equals(engine.Config.Nif.Trim(), _emisorActual?.Nif?.Trim(),
                StringComparison.OrdinalIgnoreCase);

        lblIndicadorEmisor.ForeColor = coincide ? Color.FromArgb(0, 128, 0) : Color.FromArgb(192, 0, 0);
        lblIndicadorEmisor.Text = coincide
            ? "✓ El documento pertenece al emisor"
            : "✗ El documento no pertenece al emisor";
    }

    private void ActualizarTablaValoresExtraidos()
    {
        dgvValoresExtraidos.Rows.Clear();

        if (_emisorActual == null || string.IsNullOrEmpty(_rutaPdf) || !File.Exists(_rutaPdf))
            return;

        List<Factura> facturas;
        try
        {
            // Sólo se reparsea el texto cacheado (sin tocar el PDF ni correr
            // OCR): por eso el refresco puede ser automático también con
            // documentos escaneados.
            var (texto, viaOcr, zonal) = ObtenerTextoMuestra();
            facturas = _invoiceService.ParsearMuestra(_emisorActual, texto, viaOcr, zonal, _rutaPdf);
        }
        catch (Exception ex)
        {
            dgvValoresExtraidos.Rows.Clear();
            dgvValoresExtraidos.Rows.Add("Error", ex.Message);
            SinSeleccion();
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
        // En multilínea el Total Calculado es ÚNICO para la factura
        // (Σ de los subtotales de las líneas): se muestra una sola vez.
        if (facturas.Any(f => f.EsMultiLinea))
            AñadirComun("Total Calculado", f => f.TotalCalculado);
        else
            Añadir("Total Calculado", f => f.TotalCalculado);
        AñadirComun("Total Factura", f => f.TotalFactura);
        // En multilínea el Estado es ÚNICO para la factura (todas las líneas
        // comparten el peor estado y los mismos mensajes): un solo valor.
        if (facturas.Any(f => f.EsMultiLinea))
            AñadirComun("Estado", f => f.Estado);
        else
            Añadir("Estado", f => f.Estado);
        if (facturas.Any(f => f.MensajeError?.Count > 0))
            Añadir("Mensajes", f => string.Join("; ", f.MensajeError ?? new List<string>()));

        SinSeleccion();
    }

    /// <summary>
    /// Deja la tabla «Valores extraídos» sin ninguna fila marcada: al añadir
    /// filas la primera queda seleccionada y con celda actual, y el usuario
    /// quiere verla inicialmente limpia (no hay ninguna acción sobre la
    /// selección que dependa de esto).
    /// </summary>
    private void SinSeleccion()
    {
        dgvValoresExtraidos.ClearSelection();
        dgvValoresExtraidos.CurrentCell = null;
    }

    /// <summary>
    /// Modo de extracción elegido en cmbModoExtraccion (por defecto Ordenado).
    /// </summary>
    private ModoExtraccion ModoSeleccionado() =>
        Enum.TryParse<ModoExtraccion>(
            cmbModoExtraccion.SelectedItem?.ToString(), true, out var modo)
            ? modo
            : ModoExtraccion.Ordenado;

    private void CmbModoExtraccion_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargando || cmbModoExtraccion.SelectedIndex < 0) return;

        // El modo se aplica también con zonas: se refleja ya en la config en
        // memoria para que la tabla «Valores extraídos» lo tenga en cuenta
        // aunque todavía no se haya pulsado Guardar.
        if (_emisorActual != null)
            _emisorActual.ModoExtraccion = cmbModoExtraccion.SelectedItem?.ToString() ?? "Ordenado";

        MarcarModificado();
        // El modo entra en la firma de la caché: se vuelve a extraer con el modo
        // nuevo y ActualizarVistaPreviaZonal deja al día consola, zonas y tabla.
        ActualizarVistaPreviaZonal();
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
        // Igual que al editar la rejilla: la reextracción va en diferido.
        ProgramarRefrescoVistaPrevia();
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
        // Sin PDF no hay texto que cachear ni refrescos pendientes que resolver.
        _textoMuestra = null;
        _firmaTextoMuestra = null;
        _pendientePrevia = false;
        _pendienteTabla = false;
        timerRefrescoValores.Stop();
        picFactura.Image = null;
        txtRegexSource.Text = string.Empty;
        try { dgvValoresExtraidos.Rows.Clear(); } catch { }
        ActualizarAyudasMuestra();
        // tabPaginas remains visible (empty) to reserve layout space
    }
    private void DgvCellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        // RowIndex < 0 = celda de CABECERA (p. ej. asignar HeaderText en el
        // InitializeComponent, que ya trae las columnas añadidas a la rejilla):
        // no es una edición de datos y no debe marcar el emisor como modificado.
        if (_sincronizando || _cargando || e.RowIndex < 0) return;
        MarcarModificado();

        // El mapeo multilinea no toca zonas: no debe reextraer el texto.
        if (ReferenceEquals(sender, dgvMultiLineaMapeo)) return;

        SincronizarZonasDesdeDgv();
        picFactura.Invalidate();
        // Cambiar una zona obliga a reextraer el texto (OCR con escaneados):
        // se deja en diferido para no congelar la interfaz con cada celda.
        ProgramarRefrescoVistaPrevia();
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
        ProgramarRefrescoVistaPrevia();
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
        ActualizarMarcasEscaneados();
    }

    // ── DETECCIÓN DE PDF DE MUESTRA ESCANEADO ─────────────────────────────────

    /// <summary>
    /// Caché de la comprobación «¿este PDF carece de capa de texto?» por
    /// fichero (ruta + tamaño + fecha de modificación): abrir y parsear el
    /// documento cuesta, la lista de emisores se reconstruye con cada guardado
    /// y hay 43 muestras, muchas fuera del disco local. Vive toda la sesión.
    /// </summary>
    private static readonly Dictionary<string, (long Tam, long Mtime, bool Escaneado)> _cacheEscaneados = new();

    /// <summary>
    /// true si el PDF de muestra no tiene capa de texto seleccionable y el
    /// motor tendría que recurrir al OCR. Es el mismo criterio de la extracción
    /// (<see cref="PdfTextExtractor.EsSeleccionable"/>, umbral de 30 caracteres)
    /// y NO corre OCR. Sin fichero o ilegible → false: mejor sin marca que con
    /// una marca falsa.
    /// </summary>
    private static bool EsPdfEscaneado(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta)) return false;

        try
        {
            var info = new FileInfo(ruta);
            long mtime = info.LastWriteTimeUtc.Ticks;
            long tam = info.Length;

            lock (_cacheEscaneados)
            {
                if (_cacheEscaneados.TryGetValue(info.FullName, out var guardado) &&
                    guardado.Tam == tam && guardado.Mtime == mtime)
                    return guardado.Escaneado;
            }

            bool escaneado = !new PdfTextExtractor().EsSeleccionable(ruta);

            lock (_cacheEscaneados)
                _cacheEscaneados[info.FullName] = (tam, mtime, escaneado);

            return escaneado;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Rellena la marca «▨» (entre el NIF y el nombre) de los ítems de la
    /// lista. Los PDFs se
    /// abren FUERA del hilo de la UI (algunas muestras están en unidades
    /// lentas); los ya cacheados sólo cuestan un par de estadísticas. Si algo
    /// cambia, se vuelve a montar la lista cuando termina —el ListBox sólo
    /// pinta los ítems al insertarlos—, conservando la selección como hace
    /// FiltrarEmisores().
    /// </summary>
    private void ActualizarMarcasEscaneados()
    {
        var captura = _todosEmisores.ToList();

        _ = Task.Run(() =>
        {
            bool cambio = false;
            foreach (var item in captura)
            {
                bool escaneado = EsPdfEscaneado(item.Config.RutaPdfMuestra);
                if (item.Escaneado == escaneado) continue;
                item.Escaneado = escaneado;
                cambio = true;
            }

            if (!cambio || IsDisposed || !IsHandleCreated) return;

            try
            {
                BeginInvoke((Action)FiltrarEmisores);
            }
            catch (InvalidOperationException)
            {
                // Formulario cerrado entre la comprobación y el repintado
                // (ObjectDisposedException deriva de InvalidOperationException).
            }
        });
    }

    private void FiltrarEmisores()
    {
        var filtro = txtBuscarEmisor.Text.Trim();
        var nifSeleccionado = (lstEmisores.SelectedItem as EmisorListItem)?.Config.Nif;

        // Al vaciar/recargarse los ítems se dispara SelectedIndexChanged: la bandera
        // evita la confirmación de cambios pendientes mientras teclea el filtro.
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

    private async void LstEmisores_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargando || _saltarCambioSeleccion) return;

        // El destino se captura al entrar: tras guardar, GuardarCambiosAsync
        // recarga la lista y reselecciona el emisor guardado, moviendo la selección.
        var nifDestino = (lstEmisores.SelectedItem as EmisorListItem)?.Config.Nif;

        if (!await ConfirmarCambiosPendientesAsync("Cambiar de emisor"))
        {
            RestaurarSeleccionPrevia();
            return;
        }

        // Si tras guardar la selección ha vuelto al emisor recién guardado, se
        // fuerza el destino (con el mismo criterio que al guardar: si el filtro
        // activo lo oculta, se limpia la búsqueda para que vuelva a aparecer).
        var nifSeleccionado = (lstEmisores.SelectedItem as EmisorListItem)?.Config.Nif;
        if (nifDestino != null &&
            !string.Equals(nifSeleccionado, nifDestino, StringComparison.OrdinalIgnoreCase))
        {
            if (!SeleccionarEmisorPorNif(nifDestino))
            {
                txtBuscarEmisor.Clear();
                SeleccionarEmisorPorNif(nifDestino);
            }
            // SelectedIndexChanged se vuelve a disparar con el destino ya
            // seleccionado y es ese disparo el que lo carga en la UI.
            return;
        }

        if (lstEmisores.SelectedItem is EmisorListItem item)
            CargarEmisorEnUI(item.Config);
    }

    /// <summary>
    /// Vuelve a marcar en la lista el emisor cargado en la UI (o ninguno si es
    /// un emisor nuevo, que todavía no está en la lista). Se usa cuando el
    /// usuario cancela el cambio de selección: la selección ya se había movido
    /// al pulsar otro emisor. La bandera evita reentrar en el handler.
    /// </summary>
    private void RestaurarSeleccionPrevia()
    {
        _saltarCambioSeleccion = true;
        if (_esNuevo || _emisorActual == null || !SeleccionarEmisorPorNif(_emisorActual.Nif))
            lstEmisores.ClearSelected();
        _saltarCambioSeleccion = false;
    }

    private void CargarEmisorEnUI(EmisorConfig config)
    {
        _cargando = true;
        _modificado = false;
        // Se trabaja SIEMPRE sobre una copia: los objetos EmisorConfig viven en
        // una caché estática compartida con los parsers (ConfiguracionEmisores)
        // y mutarlos en sitio filtraría los cambios "sin guardar" al resto del
        // proceso, además de hacer imposible descartarlos de verdad.
        _emisorActual = CopiarConfig(config);
        btnGuardar.Enabled = false;
        // La verificación VIES en memoria es del emisor anterior: al cargar
        // otro se olvida para que no se herede su resultado.
        _viesNifConsultado = null;
        _viesUltimoResultado = null;

        txtNombre.Text = config.Nombre;
        txtNif.Text = config.Nif;
        txtIdentificadores.Text = string.Join(Environment.NewLine, config.Identificadores);
        cmbModoExtraccion.SelectedItem = config.ModoExtraccion;
        cmbCulturaFecha.SelectedItem = config.CulturaFecha;
        txtConceptoIngreso.Text = config.ConceptoIngreso;
        txtConceptoGasto.Text = config.ConceptoGasto;

        // Los campos se cargan desde la COPIA (_emisorActual) y no desde
        // `config`, que puede ser el objeto vivo de la caché: así lo que se
        // edite en la lista es exactamente lo que calcula la tabla «Valores
        // extraídos», lo que se guarda al final y lo que se descarta al cancelar.
        lstCampos.Items.Clear();
        foreach (var c in _emisorActual.Campos)
            lstCampos.Items.Add(c);
        ActualizarItemsCmbCampoNombre();
        // El primer campo queda seleccionado: el panel arranca en modo edición
        // con un ejemplo real en lugar de vacío y deshabilitado.
        if (lstCampos.Items.Count > 0)
            lstCampos.SelectedIndex = 0;

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

        // Sincroniza ANTES de cargar el PDF: así la extracción de la muestra ya
        // ve las zonas del emisor y no hace falta extraer dos veces (una sin
        // zonas y otra con ellas).
        SincronizarZonasDesdeDgv();

        var rutaPdf = config.RutaPdfMuestra;
        if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
        {
            LimpiarPaginas();
            // Con PDF el indicador lo refresca CargarPdfMuestra; sin él hay que
            // actualizarlo aquí para que no quede el resultado del emisor anterior.
            ActualizarIndicadorIdentificadores();
        }
        else
            CargarPdfMuestra(rutaPdf, mostrarErrores: false);

        _cargando = false;
        picFactura.Invalidate();
        ActualizarVistaPreviaZonal();
        ActualizarVersionXml();
        ActualizarColorBotonVerificar();
    }

    /// <summary>
    /// Muestra junto al botón Guardar la versión (&lt;Version&gt;, timestamp yyyyMMddHHmm)
    /// del XML del emisor que se está trabajando. Sin versión (1) o sin emisor → "—".
    /// </summary>
    private void ActualizarVersionXml()
    {
        long version = 1;
        if (tabs.SelectedTab == _tabColumnasExcel)
        {
            try { version = _configColumnas.Cargar().Version; } catch { version = 1; }
        }
        else
        {
            version = _emisorActual?.Version ?? 1;
        }
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

        SincronizarCamposDesdeUI();
        _emisorActual.Campos = lstCampos.Items.Cast<CampoConfig>().ToList();
        _emisorActual.PostProcesamiento = lstPostProc.Items.Cast<PostProcesamientoConfig>().ToList();
    }

    /// <summary>
    /// Vuelca en la lista del campo seleccionado lo que esté tecleado en el
    /// panel de detalle. Se ejecuta antes de validar y de guardar: si no, el
    /// tipo o la regex recién escritos no entrarían ni en la comprobación ni en
    /// el XML. Sólo mueve objetos de la lista (no cambia el estado de la UI).
    /// </summary>
    private void SincronizarCamposDesdeUI()
    {
        if (lstCampos.SelectedItem is not CampoConfig campoActual) return;

        var nombre = cmbCampoNombre.Text.Trim();
        if (!string.IsNullOrEmpty(nombre))
            campoActual.Nombre = nombre;
        ActualizarCampoDesdeDetalle(campoActual);
    }

    // ── CRUD ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Pregunta qué hacer con los cambios pendientes antes de una acción que
    /// perdería (cambiar de emisor o crear uno nuevo). Devuelve true si
    /// se puede continuar (cambios guardados o descartados) y false si el
    /// usuario cancela o el guardado no pasa la validación.
    /// </summary>
    private async Task<bool> ConfirmarCambiosPendientesAsync(string accion)
    {
        if (!_modificado) return true;

        var msg = _esNuevo
            ? $"Hay un emisor nuevo sin guardar. ¿{accion}?\n\n" +
              "Sí = guardarlo · No = descartarlo · Cancelar = seguir editando"
            : $"Hay cambios sin guardar en '{_emisorActual?.Nombre}'. ¿{accion}?\n\n" +
              "Sí = guardarlos · No = descartarlos · Cancelar = seguir editando";

        var r = MessageBox.Show(msg, "Cambios sin guardar",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        if (r == DialogResult.Yes)
        {
            // Si la validación falla, GuardarCambiosAsync ya ha mostrado el aviso
            // y el foco va al dato culpable: se sigue editando sin cambiar nada.
            return await GuardarCambiosAsync();
        }

        if (r == DialogResult.No)
        {
            DescartarCambiosPendientes();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Descarta las ediciones en memoria. La UI trabaja sobre una copia
    /// (ver CargarEmisorEnUI), así que el objeto de la caché intacto basta
    /// para restaurar el estado guardado; se recarga el emisor desde esa
    /// caché para que el descarte sea visible en la UI.
    /// </summary>
    private void DescartarCambiosPendientes()
    {
        var nifDescartado = _esNuevo ? null : _emisorActual?.Nif;
        var limpio = nifDescartado == null ? null : _configuracion.ObtenerPorNif(nifDescartado);

        _esNuevo = false;
        _modificado = false;
        btnGuardar.Enabled = false;

        if (limpio != null)
            CargarEmisorEnUI(limpio);
    }

    private async void NuevoEmisor()
    {
        if (!await ConfirmarCambiosPendientesAsync("Crear otro emisor nuevo")) return;

        var nuevo = new EmisorConfig
        {
            Nif = "NUEVO_NIF",
            Nombre = "Nuevo Emisor",
            ModoExtraccion = "Ordenado",
            CulturaFecha = "es-ES",
            ConceptoIngreso = "700",
            ConceptoGasto = "600",
            Campos = new List<CampoConfig>
            {
                new() { Nombre = "NumeroFactura", Regex = "FACTURA(.+)" },
                new() { Nombre = "Fecha", UsarRegexFechaGeneral = true },
                new() { Nombre = "ReceptorNombre", Regex = "CLIENTE(.+)" },
                new() { Nombre = "ReceptorNif", UsarRegexNifGeneral = true }
            }
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
        // Los cambios pendientes se descartan con el propio emisor: sin esto,
        // el salto a SelectedIndex = 0 dispararía la confirmación de cambios.
        _esNuevo = false;
        _modificado = false;
        btnGuardar.Enabled = false;
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

    /// <summary>
    /// Copia profunda de un emisor. La usa <see cref="CargarEmisorEnUI"/> para
    /// aislar la edición de la caché compartida con los parsers; los objetos
    /// anidados (que también se mutan en sitio desde la UI) los copian
    /// CopiarMapeos/CopiarCampo/CopiarPostProc/CopiarZona.
    /// </summary>
    private static EmisorConfig CopiarConfig(EmisorConfig o) => new()
    {
        Nif = o.Nif,
        Nombre = o.Nombre,
        Version = o.Version,
        ModoExtraccion = o.ModoExtraccion,
        ConceptoIngreso = o.ConceptoIngreso,
        ConceptoGasto = o.ConceptoGasto,
        CulturaFecha = o.CulturaFecha,
        RutaPdfMuestra = o.RutaPdfMuestra,
        Identificadores = new List<string>(o.Identificadores),
        Campos = o.Campos.Select(CopiarCampo).ToList(),
        MultiLinea = o.MultiLinea == null ? null : new MultiLineaConfig
        {
            RegexLinea = o.MultiLinea.RegexLinea,
            MapeoCampos = CopiarMapeos(o.MultiLinea.MapeoCampos),
            Lineas = o.MultiLinea.Lineas
                .Select(l => new LineaConfig
                {
                    Regex = l.Regex,
                    MapeoCampos = CopiarMapeos(l.MapeoCampos)
                }).ToList()
        },
        PostProcesamiento = o.PostProcesamiento.Select(CopiarPostProc).ToList(),
        ZonasOcr = o.ZonasOcr?.Select(CopiarZona).ToList(),
        NifEstado = o.NifEstado
    };

    private static List<MapeoCampoLinea> CopiarMapeos(List<MapeoCampoLinea> mapeos) =>
        mapeos.Select(m => new MapeoCampoLinea { Nombre = m.Nombre, Grupo = m.Grupo }).ToList();

    private static ZonaOcrConfig CopiarZona(ZonaOcrConfig z) => new()
    {
        Campo = z.Campo,
        NumPagina = z.NumPagina,
        X = z.X, Y = z.Y, Ancho = z.Ancho, Alto = z.Alto,
        Preprocesamiento = z.Preprocesamiento == null ? null : new PreprocesamientoOcr
        {
            EscalaGrises = z.Preprocesamiento.EscalaGrises,
            Binarizacion = z.Preprocesamiento.Binarizacion,
            UmbralBinarizacion = z.Preprocesamiento.UmbralBinarizacion,
            Redimensionar = z.Preprocesamiento.Redimensionar,
            FactorEscala = z.Preprocesamiento.FactorEscala,
            EliminarRuido = z.Preprocesamiento.EliminarRuido,
            InvertirColores = z.Preprocesamiento.InvertirColores,
            AutoRecortar = z.Preprocesamiento.AutoRecortar,
            PaddingAutoRecorte = z.Preprocesamiento.PaddingAutoRecorte
        }
    };

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

    // ── VERIFICACIÓN NIF (local + VIES) ───────────────────────────────────────

    private async void BtnVerificarNif_Click(object? sender, EventArgs e)
    {
        if (_emisorActual == null)
        {
            MessageBox.Show("Seleccione o cree un emisor antes de verificar su NIF.",
                "Verificar NIF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var nif = txtNif.Text.Trim();
        if (string.IsNullOrWhiteSpace(nif))
        {
            MessageBox.Show("Introduzca primero un NIF.", "Verificar NIF",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.Equals(nif, "General", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                "El emisor 'General' usa un NIF ficticio; no hay nada que verificar.",
                "Verificar NIF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        btnVerificarNif.Enabled = false;
        btnVerificarNif.Text = "Verificando...";
        try
        {
            string estado;
            string mensaje;
            if (NifValidator.ValidarNif(nif))
            {
                // NIF español: se resuelve en local, sin consultar la red.
                estado = "Valido";
                mensaje = $"VÁLIDO: {nif} es un NIF español válido (DNI/NIE/CIF)\n" +
                          "(verificación local, sin consulta a VIES).";
            }
            else if (ViesValidator.ObtenerPrefijoPais(nif) == "GB")
            {
                estado = "NoDisponible";
                mensaje = "Los NIF británicos (GB) dejaron de verificarse en VIES el " +
                          "1/1/2021 por el Brexit.\nDeben comprobarse en la HMRC británica.";
            }
            else if (ViesValidator.EsNifComunitario(nif))
            {
                var resultado = await ViesValidator.VerificarAsync(nif);
                GuardarResultadoVies(nif, resultado);
                if (IsDisposed) return;
                estado = resultado.Estado;
                mensaje = DescribirResultadoVies(resultado, nif);
            }
            else
            {
                estado = "Invalido";
                mensaje = $"NO VÁLIDO: {nif} no es un NIF español válido (DNI/NIE/CIF)\n" +
                          "ni un NIF-IVA comunitario con prefijo de estado miembro.";
            }

            // El estado se registra en el emisor como cualquier otra edición:
            // queda PENDIENTE de guardar y el XML sólo se escribe al pulsar
            // Guardar (con su correspondiente subida de <Version>).
            _emisorActual.NifEstado = estado;
            MarcarModificado();
            ActualizarColorBotonVerificar();
            mensaje += $"\n\nEstado '{estado}' registrado como pendiente de guardar.";

            MessageBox.Show(mensaje, "Verificar NIF", MessageBoxButtons.OK,
                estado == "Invalido" ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
        finally
        {
            btnVerificarNif.Enabled = true;
            btnVerificarNif.Text = "Verificar NIF";
        }
    }

    /// <summary>
    /// Pinta el botón «Verificar NIF» según el estado NIF del emisor cargado:
    /// verde claro = válido, rojo claro = inválido, amarillo claro = no
    /// disponible y color original si nunca se ha verificado. UseVisualStyleBackColor
    /// hay que apagarlo al pintar, o el botón ignora el BackColor.
    /// </summary>
    private void ActualizarColorBotonVerificar()
    {
        switch (_emisorActual?.NifEstado)
        {
            case "Valido":
                btnVerificarNif.UseVisualStyleBackColor = false;
                btnVerificarNif.BackColor = Color.LightGreen;
                break;
            case "Invalido":
                btnVerificarNif.UseVisualStyleBackColor = false;
                // Color.LightRed no existe en System.Drawing: rojo claro por RGB
                btnVerificarNif.BackColor = Color.FromArgb(255, 170, 170);
                break;
            case "NoDisponible":
                btnVerificarNif.UseVisualStyleBackColor = false;
                btnVerificarNif.BackColor = Color.LightYellow;
                break;
            default:
                btnVerificarNif.UseVisualStyleBackColor = true;
                break;
        }
    }

    /// <summary>Guarda en memoria el resultado de una consulta VIES para no
    /// repetirla al guardar. La caché se limpia al cargar otro emisor y sólo
    /// se usa si el NIF consultado sigue siendo el mismo.</summary>
    private void GuardarResultadoVies(string nif, ViesResultado resultado)
    {
        _viesNifConsultado = nif.Trim().ToUpperInvariant();
        _viesUltimoResultado = resultado;
    }

    private ViesResultado? ViesResultadoPara(string nif) =>
        _viesUltimoResultado != null &&
        string.Equals(_viesNifConsultado, nif.Trim().ToUpperInvariant(),
            StringComparison.OrdinalIgnoreCase)
            ? _viesUltimoResultado : null;

    /// <summary>Consulta VIES sin bloquear la UI, inhabilitando los controles
    /// que podrían reentrar en un guardado mientras la llamada está en vuelo.
    /// El resultado queda cacheado para el guardado posterior.</summary>
    private async Task<ViesResultado> ConsultarViesAsync(string nif)
    {
        btnGuardar.Enabled = false;
        lstEmisores.Enabled = false;
        btnNuevo.Enabled = false;
        try
        {
            var resultado = await ViesValidator.VerificarAsync(nif);
            GuardarResultadoVies(nif, resultado);
            return resultado;
        }
        finally
        {
            lstEmisores.Enabled = true;
            btnNuevo.Enabled = true;
            btnGuardar.Enabled = _modificado;
        }
    }

    private static string DescribirResultadoVies(ViesResultado r, string nif)
    {
        if (!r.Disponible)
        {
            return $"No se pudo consultar VIES para el NIF {nif}.\n" +
                   $"Motivo: {r.MensajeError}\n\n" +
                   "El servicio puede estar en mantenimiento (cada estado miembro " +
                   "puede caerse de forma independiente). Inténtelo más tarde.";
        }

        var texto = r.Valido
            ? $"VÁLIDO en VIES: el NIF-IVA {nif} está registrado como válido\n" +
              "para operaciones intracomunitarias."
            : $"NO VÁLIDO en VIES: el NIF-IVA {nif} no consta como válido\n" +
              "para operaciones intracomunitarias.";
        if (r.CodigoError != null)
            texto += $"\n({r.MensajeError})";
        if (r.Nombre != null)
            texto += $"\n\nTitular: {r.Nombre}";
        if (r.Direccion != null)
            texto += $"\nDirección: {r.Direccion}";
        texto += $"\nConsulta: {r.FechaConsulta:dd/MM/yyyy HH:mm}";
        return texto;
    }

    // ── GUARDAR ────────────────────────────────────────────────────────────────

    /// <summary>Guarda el emisor en disco. Devuelve false si la validación
    /// falla, el usuario cancela ante un NIF-IVA no válido en VIES o se
    /// produce un error (el aviso ya se ha mostrado).</summary>
    private async Task<bool> GuardarCambiosAsync()
    {
        if (_emisorActual == null) return false;

        var nifNuevo = txtNif.Text.Trim();
        if (string.IsNullOrWhiteSpace(nifNuevo))
        {
            MessageBox.Show("El NIF no puede estar vacío.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // Validate NIF uniqueness (exclude self for existing emitters)
        var nifAnterior = _emisorActual.Nif;
        var todos = _configuracion.CargarTodos();
        if (todos.ContainsKey(nifNuevo) &&
            (_esNuevo || !string.Equals(nifAnterior, nifNuevo, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Ya existe un emisor con NIF '{nifNuevo}'.\nEl NIF debe ser único.",
                "NIF duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var (errorPostProc, campoPostProc) = ValidarReglasPostProc();
        if (errorPostProc != null)
        {
            tabs.SelectedTab = tabMultiLinea;
            MessageBox.Show(errorPostProc,
                "Post-procesamiento incompleto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // Foco al dato que falta, una vez cerrado el aviso.
            campoPostProc?.Focus();
            return false;
        }

        // Campos: errores bloqueantes (expresión rota o tipo incompatível con el
        // nombre) ANTES de consultar VIES o tocar el disco. Salto a la pestaña
        // y foco al dato culpable, igual que con el post-procesamiento.
        var (errorCampos, focoCampo) = ValidarCampos();
        if (errorCampos != null)
        {
            tabs.SelectedTab = tabCampos;
            MessageBox.Show(errorCampos,
                "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focoCampo?.Focus();
            return false;
        }

        // VIES: sólo para NIF-IVA comunitarios (con prefijo de estado miembro)
        // y sólo si no hay ya un resultado en caché para ese mismo NIF. Un NIF
        // español sin prefijo no dispara ninguna llamada de red al guardar.
        if (ViesValidator.EsNifComunitario(nifNuevo))
        {
            var resultadoVies = ViesResultadoPara(nifNuevo)
                ?? await ConsultarViesAsync(nifNuevo);
            if (IsDisposed) return false;

            if (!resultadoVies.Disponible)
            {
                MessageBox.Show(
                    $"No se pudo consultar VIES para {nifNuevo}: " +
                    $"{resultadoVies.MensajeError}\n\n" +
                    "El emisor se guardará con estado de verificación 'NoDisponible'.",
                    "VIES no disponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (!resultadoVies.Valido)
            {
                var r = MessageBox.Show(
                    $"VIES no da por válido el NIF-IVA {nifNuevo}\n" +
                    "para operaciones intracomunitarias." +
                    (resultadoVies.CodigoError != null
                        ? $"\n({resultadoVies.MensajeError})" : "") +
                    "\n\n¿Guardar el emisor de todos modos?",
                    "NIF-IVA no válido en VIES",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                {
                    txtNif.Focus();
                    return false;
                }
            }

            // Los tres estados (válido, inválido o no disponible) quedan
            // registrados en el XML del emisor.
            _emisorActual.NifEstado = resultadoVies.Estado;
            ActualizarColorBotonVerificar();
        }
        // NIF sin prefijo comunitario: se valida el formato español aquí, porque
        // el procesamiento ya NO valida el NIF del emisor (se validó en el alta).
        // El emisor "General" es un caso especial con NIF ficticio.
        else if (!string.Equals(nifNuevo, "General", StringComparison.OrdinalIgnoreCase) &&
                 !NifValidator.ValidarNif(nifNuevo))
        {
            var r = MessageBox.Show(
                $"El NIF {nifNuevo} no tiene formato de NIF español válido (DNI/NIE/CIF).\n\n" +
                "¿Guardar el emisor de todos modos?",
                "NIF no válido", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes)
            {
                txtNif.Focus();
                return false;
            }
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

        // Avisos no bloqueantes: no impiden guardar, pero avisan de lo que hará
        // que las facturas salgan en «Revisar» o «Error».
        if (!ConfirmarAvisosCampos()) return false;

        try
        {
            _configuracion.Guardar(_emisorActual, _esNuevo ? null : nifAnterior);
            _esNuevo = false;
            _modificado = false;
            btnGuardar.Enabled = false;
            // Guardar deja en la caché LA MISMA instancia que edita la UI: sin
            // esta copia, las ediciones siguientes (todavía sin guardar y por
            // tanto descartables) se colarían en los parsers al vivir en la caché.
            _emisorActual = CopiarConfig(_emisorActual);
            RecargarListaCampos();
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
            // Refresca el indicador de identificadores: CargarEmisores() reconstruye
            // la lista con _saltarCambioSeleccion, sin llegar a CargarEmisorEnUI,
            // así que sin esta llamada el mensaje se quedaría desactualizado hasta
            // salir del emisor y volver a entrar.
            ActualizarIndicadorIdentificadores();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    /// <summary>
    /// Validación de la pestaña «Campos» antes de guardar: devuelve el mensaje
    /// del primer error bloqueante y el control a enfocar (null si todo va bien).
    /// Los avisos no bloqueantes están en <see cref="ConfirmarAvisosCampos"/>.
    /// </summary>
    private (string? Error, Control? Foco) ValidarCampos()
    {
        // Lo tecleado en el panel del campo seleccionado todavía no está en la
        // lista: sin sincronizar, se validaría lo anterior a lo tecleado.
        SincronizarCamposDesdeUI();

        var nombre = cmbCampoNombre.Text?.Trim() ?? "";
        if (!string.IsNullOrEmpty(nombre) && CampoDefinido(nombre) == null)
        {
            return ($"El campo «{nombre}» está tecleado en el panel pero no se ha " +
                    "añadido a la lista.\n\nPulsa «+ Añadir» para incluirlo, o vacía " +
                    "el nombre para descartarlo.", cmbCampoNombre);
        }

        foreach (var campo in lstCampos.Items.Cast<CampoConfig>())
        {
            // El tipo del campo seleccionado se lee del panel (es lo que está
            // tecleado); para el resto se deduce de lo guardado.
            var esSeleccionado = ReferenceEquals(lstCampos.SelectedItem, campo);
            var tipo = esSeleccionado
                ? cmbCampoTipo.SelectedItem?.ToString()
                : TipoDeducido(campo);

            switch (tipo)
            {
                case "RegexFechaGeneral":
                    if (!string.Equals(campo.Nombre, "Fecha", StringComparison.OrdinalIgnoreCase))
                    {
                        // Señalar el campo en la lista: al cargarlo en el panel se
                        // corrige el tipo y se deja el aviso en lblTipoAyuda.
                        if (!esSeleccionado) lstCampos.SelectedItem = campo;
                        return ($"El campo «{campo.Nombre}» usa «RegexFechaGeneral», que sólo " +
                                "funciona si el campo se llama «Fecha».\n\nSe ha vuelto a " +
                                "poner «Regex»: revisa su expresión.", cmbCampoTipo);
                    }
                    continue;

                case "RegexNifGeneral":
                    if (!string.Equals(campo.Nombre, "ReceptorNif", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(campo.Nombre, "EmisorNif", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!esSeleccionado) lstCampos.SelectedItem = campo;
                        return ($"El campo «{campo.Nombre}» usa «RegexNifGeneral», que sólo " +
                                "funciona para «ReceptorNif» o «EmisorNif».\n\nSe ha vuelto " +
                                "a poner «Regex»: revisa su expresión.", cmbCampoTipo);
                    }
                    continue;

                case "ValorFijo":
                    if (!string.IsNullOrWhiteSpace(campo.ValorFijo)) continue;
                    if (!esSeleccionado) lstCampos.SelectedItem = campo;
                    return ($"El campo «{campo.Nombre}» está como «ValorFijo» pero no " +
                            "tiene valor. Escribe el valor o cambia el tipo.", txtCampoValorFijo);

                case null:
                    if (!esSeleccionado) lstCampos.SelectedItem = campo;
                    return ($"El campo «{campo.Nombre}» no tiene tipo de extracción elegido.",
                        cmbCampoTipo);

                default:   // «Regex»: la expresión es obligatoria y tiene que capturar
                    var error = ErrorRegexCampo(campo.Regex);
                    if (error == null) continue;
                    if (!esSeleccionado) lstCampos.SelectedItem = campo;
                    return ($"Campo «{campo.Nombre}»: {error}", txtCampoRegex);
            }
        }

        return (null, null);
    }

    private static string TipoDeducido(CampoConfig campo) =>
        campo.UsarRegexFechaGeneral ? "RegexFechaGeneral"
        : campo.UsarRegexNifGeneral ? "RegexNifGeneral"
        : !string.IsNullOrEmpty(campo.ValorFijo) ? "ValorFijo"
        : "Regex";

    /// <summary>
    /// Avisos no bloqueantes sobre los campos: faltan obligatorios o, con el PDF
    /// de muestra cargado, no se extrae nada. Devuelve false si el usuario
    /// decide no guardar.
    /// </summary>
    private bool ConfirmarAvisosCampos()
    {
        var definidos = new HashSet<string>(
            _emisorActual!.Campos.Select(c => c.Nombre), StringComparer.OrdinalIgnoreCase);

        var obligatorios = new[]
        {
            "NumeroFactura", "Fecha", "ReceptorNombre", "ReceptorNif",
            "BaseImponible", "TotalFactura"
        };
        var faltan = obligatorios.Where(n => !definidos.Contains(n)).ToList();

        if (faltan.Count > 0)
        {
            var r = MessageBox.Show(
                "Faltan campos recomendados:\n• " + string.Join("\n• ", faltan) +
                "\n\nSin ellos las facturas quedarán en «Revisar» o «Error»." +
                "\n\n¿Guardar el emisor de todos modos?",
                "Campos recomendados", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return false;
        }

        if (MuestraNoExtraeNada())
        {
            var r = MessageBox.Show(
                "Con el documento de muestra cargado no se extrae ningún dato de los campos " +
                "definidos (ni número de factura, ni fecha, ni importes).\n\n" +
                "Revisa las expresiones antes de dar por bueno el emisor." +
                "\n\n¿Guardar el emisor de todos modos?",
                "La muestra no extrae nada", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return false;
        }

        return true;
    }

    /// <summary>
    /// true si, con el PDF de muestra actual, ningún campo extrae datos. Sólo
    /// reutiliza el texto cacheado: si aún no estaba extraído se extrae aquí
    /// (una única pasada al guardar, no al editar).
    /// </summary>
    private bool MuestraNoExtraeNada()
    {
        if (_emisorActual == null || string.IsNullOrEmpty(_rutaPdf) || !File.Exists(_rutaPdf))
            return false;

        try
        {
            var (texto, viaOcr, zonal) = ObtenerTextoMuestra();
            var facturas = _invoiceService.ParsearMuestra(_emisorActual, texto, viaOcr, zonal, _rutaPdf);
            if (facturas.Count == 0) return true;
            return facturas.All(f =>
                string.IsNullOrEmpty(f.NumeroFactura) &&
                !f.Fecha.HasValue &&
                string.IsNullOrEmpty(f.Receptor?.NIF) &&
                f.BaseImponible == 0 &&
                f.TotalFactura == 0);
        }
        catch
        {
            // Si la muestra no se puede procesar, no es ése el motivo para frenar
            // el guardado: el error real ya aparecería en «Valores extraídos».
            return false;
        }
    }

    /// <summary>
    /// Vuelve a poblar la lista de campos desde _emisorActual conservando la
    /// selección por nombre. Se usa tras guardar, cuando la caché se queda con
    /// la instancia guardada y la UI necesita seguir editando una copia propia.
    /// </summary>
    private void RecargarListaCampos()
    {
        if (_emisorActual == null) return;

        var seleccionado = cmbCampoNombre.Text?.Trim() ?? "";
        _campoAnterior = null;
        lstCampos.Items.Clear();
        foreach (var c in _emisorActual.Campos)
            lstCampos.Items.Add(c);
        ActualizarItemsCmbCampoNombre();

        var campo = string.IsNullOrEmpty(seleccionado) ? null : CampoDefinido(seleccionado);
        if (campo != null)
            lstCampos.SelectedItem = campo;
        RefrescarModoDetalleCampo();
    }

    private async void BtnGuardar_Click(object? sender, EventArgs e)
    {
        await GuardarCambiosAsync();
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
        // SelectedIndex = -1 y no Text = "": en una combo DropDownList el texto
        // sólo admite valores que estén en los ítems.
        cmbCampoNombre.SelectedIndex = -1;
        cmbCampoTipo.SelectedIndex = -1;
        txtCampoRegex.Text = "";
        txtCampoValorFijo.Text = "";
        // El probador refleja el detalle: sin campo no hay expresión que probar.
        txtRegexPattern.Text = "";
    }

    /// <summary>
    /// Campo ya definido para el emisor con ese nombre, o null si todavía no
    /// existe. Es la pieza que decide si el panel está en modo edición o de alta.
    /// </summary>
    private CampoConfig? CampoDefinido(string nombre) =>
        string.IsNullOrEmpty(nombre)
            ? null
            : lstCampos.Items.Cast<CampoConfig>()
                .FirstOrDefault(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Reconstruye la lista de tipos de extracción válidos para el nombre de
    /// campo indicado. El motor sólo aplica «RegexFechaGeneral» al campo
    /// «Fecha» y «RegexNifGeneral» a «ReceptorNif»/«EmisorNif»: no se ofrecen
    /// combinaciones que se ignorarían en silencio. Si <paramref name="tipoAConservar"/>
    /// ya no es válido para el nuevo nombre, se vuelve a «Regex» dejando aviso.
    /// </summary>
    private void ActualizarTiposDisponibles(string nombre, string? tipoAConservar)
    {
        cmbCampoTipo.Items.Clear();
        cmbCampoTipo.Items.Add("Regex");
        cmbCampoTipo.Items.Add("ValorFijo");
        if (string.Equals(nombre, "Fecha", StringComparison.OrdinalIgnoreCase))
            cmbCampoTipo.Items.Add("RegexFechaGeneral");
        if (string.Equals(nombre, "ReceptorNif", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(nombre, "EmisorNif", StringComparison.OrdinalIgnoreCase))
            cmbCampoTipo.Items.Add("RegexNifGeneral");

        if (string.IsNullOrEmpty(tipoAConservar)) return;

        if (cmbCampoTipo.Items.Contains(tipoAConservar))
        {
            cmbCampoTipo.SelectedItem = tipoAConservar;
        }
        else
        {
            cmbCampoTipo.SelectedItem = "Regex";
            _avisoTipoPendiente =
                $"«{tipoAConservar}» no vale para «{nombre}» → se usa «Regex».";
        }
    }

    /// <summary>
    /// Pone el panel en modo alta para un nombre que todavía no está definido:
    /// guarda los cambios pendientes del campo anterior, deselecciona la lista
    /// (sin tocar el nombre elegido), deja tipos válidos para ese nombre y
    /// borra los datos del campo anterior para que no se cuelen en el nuevo.
    /// </summary>
    private void PrepararAltaCampo(string nombre)
    {
        if (_campoAnterior != null)
            ActualizarCampoDesdeDetalle(_campoAnterior);   // no se pierde lo tecleado
        _campoAnterior = null;

        _cargandoCampo = true;
        lstCampos.SelectedItem = null;

        var tipoPrevio = cmbCampoTipo.SelectedItem?.ToString();
        ActualizarTiposDisponibles(nombre, tipoPrevio);
        if (cmbCampoTipo.SelectedIndex < 0)
            cmbCampoTipo.SelectedItem = "Regex";
        txtCampoRegex.Text = "";
        txtCampoValorFijo.Text = "";
        txtRegexPattern.Text = "";
        _cargandoCampo = false;
    }

    /// <summary>
    /// Estado visible del panel de detalle: modo EDICIÓN (nombre ya definido),
    /// modo ALTA (nombre por definir) o SIN SELECCIÓN. Habilita en consecuencia
    /// los controles y el botón «+ Añadir», y actualiza la ayuda del tipo.
    /// </summary>
    private void RefrescarModoDetalleCampo()
    {
        var nombre = cmbCampoNombre.Text?.Trim() ?? "";
        var existente = string.IsNullOrEmpty(nombre) ? null : CampoDefinido(nombre);
        _campoEnAlta = !string.IsNullOrEmpty(nombre) && existente == null;

        if (string.IsNullOrEmpty(nombre))
        {
            btnCampoAdd.Enabled = false;
            lblEstadoCampo.Text = "Sin selección: elige un nombre de campo";
            lblEstadoCampo.ForeColor = Color.Gray;
        }
        else if (existente != null)
        {
            btnCampoAdd.Enabled = false;
            lblEstadoCampo.Text = $"Editando «{existente.Nombre}»";
            lblEstadoCampo.ForeColor = Color.FromArgb(0, 128, 0);
        }
        else
        {
            btnCampoAdd.Enabled = true;
            lblEstadoCampo.Text = $"Nuevo campo «{nombre}» — define el tipo y pulsa «+ Añadir»";
            lblEstadoCampo.ForeColor = Color.FromArgb(180, 110, 0);
        }

        // Cada tipo usa un único dato: el resto queda deshabilitado para que no
        // se tecleen valores que el motor ignoraría.
        var tipo = cmbCampoTipo.SelectedItem?.ToString();
        var hayNombre = !string.IsNullOrEmpty(nombre);
        cmbCampoTipo.Enabled = hayNombre;
        txtCampoRegex.Enabled = hayNombre && tipo == "Regex";
        txtCampoValorFijo.Enabled = hayNombre && tipo == "ValorFijo";

        ActualizarAyudaTipoCampo();
    }

    /// <summary>
    /// Texto de ayuda bajo el panel de detalle: explica qué dato recoge cada
    /// tipo de extracción o, si hay uno pendiente, por qué se cambió el tipo.
    /// </summary>
    private void ActualizarAyudaTipoCampo()
    {
        if (lblTipoAyuda == null) return;

        if (_avisoTipoPendiente != null)
        {
            lblTipoAyuda.Text = _avisoTipoPendiente;
            lblTipoAyuda.ForeColor = Color.FromArgb(180, 110, 0);
            return;
        }

        var (texto, color) = (cmbCampoTipo.SelectedItem?.ToString()) switch
        {
            // «Regex» ya no lleva coletilla propia («El valor se toma del Grupo 1…»,
            // retirada a petición del usuario): cae en el caso por defecto de abajo.
            "ValorFijo" => ("Valor fijo para todas las facturas; la expresión se ignora.", Color.DimGray),
            "RegexFechaGeneral" => ("Detector general de fechas; sólo vale para «Fecha».", Color.DimGray),
            "RegexNifGeneral" => ("Detector general de NIF; sólo para ReceptorNif/EmisorNif.", Color.DimGray),
            _ => ("", Color.DimGray)
        };
        lblTipoAyuda.Text = texto;
        lblTipoAyuda.ForeColor = color;
    }

    private void CampoDetalle_Changed(object? sender, EventArgs e)
    {
        if (_cargandoCampo) return;

        // Al cambiar de tipo deja de aplicarse el aviso de tipo incompatible.
        if (sender == cmbCampoTipo) _avisoTipoPendiente = null;

        if (lstCampos.SelectedItem is CampoConfig campo)
        {
            ActualizarCampoDesdeDetalle(campo);
            // El probador sigue al campo al instante: lo que se teclee en
            // «Regex» se prueba solo contra el texto extraído de la muestra.
            if (sender == txtCampoRegex)
                txtRegexPattern.Text = txtCampoRegex.Text.Trim();
            MarcarModificado();
            ProgramarRefrescoValores();
        }
        // En modo alta no hay campo seleccionado: sólo se recalcula qué controles
        // están disponibles (nada se modifica todavía, así que no se marca).
        RefrescarModoDetalleCampo();
    }

    /// <summary>
    /// Un nombre elegido en la combo decide el modo del panel: si ya está
    /// definido para el emisor se selecciona en la lista para editarlo; si
    /// todavía no lo está, el panel queda en modo alta para crearlo.
    /// </summary>
    private void CmbCampoNombre_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargandoCampo) return;

        _avisoTipoPendiente = null;
        var nombre = cmbCampoNombre.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(nombre))
        {
            // Sin nombre no hay nada que editar: el panel queda inactivo.
            DeseleccionarCampo();
            RefrescarModoDetalleCampo();
            return;
        }

        var existente = CampoDefinido(nombre);
        if (existente != null)
        {
            // Nombre ya definido para este emisor → modo EDICIÓN.
            SeleccionarCampoEnLista(existente);
            return;
        }

        // Nombre todavía no definido → modo ALTA (el nombre elegido se conserva).
        PrepararAltaCampo(nombre);
        RefrescarModoDetalleCampo();
    }

    private void LstCampos_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Sólo el clic del usuario en la lista: los cambios programáticos de
        // selección (combo de nombres, alta/baja de campo, guardado) los gestiona
        // cada uno en su sitio a través de SeleccionarCampoEnLista.
        if (_cargandoCampo) return;

        if (lstCampos.SelectedItem is CampoConfig campo)
        {
            SeleccionarCampoEnLista(campo);
            return;
        }

        DeseleccionarCampo();
        RefrescarModoDetalleCampo();
    }

    /// <summary>
    /// Señala un campo en la lista y lo vuelca en el panel de detalle. Es el
    /// único camino «campo seleccionado», venga de la lista o de la combo de
    /// nombres, así ambos controles quedan siempre sincronizados.
    /// </summary>
    private void SeleccionarCampoEnLista(CampoConfig campo)
    {
        // Los cambios pendientes del campo anterior se guardan en él; el nuevo
        // no se toca, o se le colarían los valores del anterior.
        if (_campoAnterior != null && !ReferenceEquals(_campoAnterior, campo))
            ActualizarCampoDesdeDetalle(_campoAnterior);

        _cargandoCampo = true;
        _campoAnterior = campo;
        lstCampos.SelectedItem = campo;
        _cargandoCampo = false;

        CargarDetalleDeCampo(campo);
        RefrescarModoDetalleCampo();
    }

    /// <summary>
    /// Vuelca un campo en los controles del panel de detalle. Sólo toca datos
    /// (nunca la selección de la lista), por eso sirve desde cualquier origen.
    /// </summary>
    private void CargarDetalleDeCampo(CampoConfig campo)
    {
        _cargandoCampo = true;

        var tipoGuardado = campo.UsarRegexFechaGeneral ? "RegexFechaGeneral"
            : campo.UsarRegexNifGeneral ? "RegexNifGeneral"
            : !string.IsNullOrEmpty(campo.ValorFijo) ? "ValorFijo"
            : "Regex";
        // Los tipos dependen del nombre: reconstruir antes de restaurarlos.
        ActualizarTiposDisponibles(campo.Nombre, tipoGuardado);
        if (cmbCampoTipo.SelectedIndex < 0)
        {
            cmbCampoTipo.SelectedItem = "Regex";
            _avisoTipoPendiente =
                $"«{tipoGuardado}» guardado en el XML no vale para «{campo.Nombre}» → se usa «Regex».";
        }

        txtCampoRegex.Text = campo.Regex ?? "";
        txtCampoValorFijo.Text = campo.ValorFijo ?? "";
        SincronizarProbadorDesdeCampo(campo);
        // La combo refleja el campo en edición (no-op si el cambio vino de ahí).
        cmbCampoNombre.Text = campo.Nombre;

        _cargandoCampo = false;
    }

    /// <summary>
    /// Sin campo seleccionado: guarda los cambios pendientes del anterior y
    /// deja el panel inactivo. La lista y la combo quedan vacías a la par.
    /// </summary>
    private void DeseleccionarCampo()
    {
        if (_campoAnterior != null)
            ActualizarCampoDesdeDetalle(_campoAnterior);
        _campoAnterior = null;

        _cargandoCampo = true;
        lstCampos.SelectedItem = null;
        LimpiarDetalleCampo();
        _cargandoCampo = false;
    }

    /// <summary>
    /// El probador refleja el campo seleccionado: con «Regex» muestra su
    /// expresión para probarla al instante; el resto de tipos no usan
    /// expresión, así que queda vacío.
    /// </summary>
    private void SincronizarProbadorDesdeCampo(CampoConfig campo)
    {
        var usaRegex = !campo.UsarRegexFechaGeneral && !campo.UsarRegexNifGeneral
                       && string.IsNullOrEmpty(campo.ValorFijo);
        txtRegexPattern.Text = usaRegex ? campo.Regex ?? "" : "";
    }

    private void BtnCampoAdd_Click(object? sender, EventArgs e)
    {
        if (_emisorActual == null) return;

        var nombre = cmbCampoNombre.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(nombre))
        {
            MessageBox.Show("Elige un nombre de campo en la lista desplegable.",
                "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (CampoDefinido(nombre) != null)
        {
            MessageBox.Show($"El campo '{nombre}' ya está definido y se está editando en el panel.",
                "Ya existe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var tipo = cmbCampoTipo.SelectedItem?.ToString();
        if (tipo == null)
        {
            MessageBox.Show("Elige el tipo de extracción del campo.",
                "Tipo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbCampoTipo.Focus();
            return;
        }

        if (tipo == "Regex")
        {
            var error = ErrorRegexCampo(txtCampoRegex.Text);
            if (error != null)
            {
                MessageBox.Show(error, "Expresión regular", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCampoRegex.Focus();
                return;
            }
        }
        else if (tipo == "ValorFijo" && string.IsNullOrWhiteSpace(txtCampoValorFijo.Text))
        {
            MessageBox.Show("Escribe el valor fijo que se asignará al campo.",
                "Valor requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCampoValorFijo.Focus();
            return;
        }

        var nuevo = new CampoConfig { Nombre = nombre };
        ActualizarCampoDesdeDetalle(nuevo);
        nuevo.Nombre = nombre;

        lstCampos.Items.Add(nuevo);
        ActualizarItemsCmbCampoNombre();

        // El panel pasa a EDITAR el campo recién creado: la lista lo selecciona
        // y su handler rellena el detalle con lo que acaba de guardarse (no se
        // borra nada, así no se confunde con el alta de un campo siguiente).
        lstCampos.SelectedItem = nuevo;

        MarcarModificado();
        ProgramarRefrescoValores();
    }

    private void BtnCampoRemove_Click(object? sender, EventArgs e)
    {
        if (lstCampos.SelectedItem is not CampoConfig campo)
        {
            MessageBox.Show("Selecciona en la lista el campo que quieras quitar.",
                "Ningún campo seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirmar = MessageBox.Show(
            $"¿Quitar el campo '{campo.Nombre}' del emisor?",
            "Quitar campo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmar != DialogResult.Yes) return;

        lstCampos.Items.Remove(campo);
        ActualizarItemsCmbCampoNombre();

        // Al quitar el último seleccionado la lista puede señalar otro: en ese
        // caso el detalle ya lo ha cargado y _campoAnterior apunta a él (no se
        // anula, o se perderían sus cambios pendientes al cambiar de campo).
        if (lstCampos.SelectedItem == null)
        {
            _campoAnterior = null;
            _cargandoCampo = true;
            LimpiarDetalleCampo();
            _cargandoCampo = false;
        }

        MarcarModificado();
        RefrescarModoDetalleCampo();
        ProgramarRefrescoValores();
    }

    /// <summary>
    /// Comprueba una expresión de campo: debe existir y debe tener al menos un
    /// grupo de captura, porque el motor lee el Grupo 1 y sin paréntesis no se
    /// extraería ningún valor. Devuelve null si es válida.
    /// </summary>
    private static string? ErrorRegexCampo(string? regex)
    {
        if (string.IsNullOrWhiteSpace(regex))
            return "El campo no tiene expresión regular.";

        Regex probada;
        try
        {
            // Mismas opciones que el motor: case-sensitive, para que validar
            // en la GUI refleje exactamente lo que se extraerá al procesar.
            probada = new Regex(regex, RegexOptions.Compiled);
        }
        catch (RegexParseException ex)
        {
            return $"La expresión regular no es válida:\n{ex.Message}";
        }

        // GetGroupNumbers incluye el 0 (la coincidencia completa): hace falta
        // al menos un número más para que exista un grupo de captura.
        if (probada.GetGroupNumbers().Length <= 1)
            return "La expresión no tiene ningún grupo de captura: envuelve entre " +
                   "paréntesis lo que hay que extraer, porque el motor lee el Grupo 1.";

        return null;
    }

    // ── REFRESCO EN DIFERIDO DE «VALORES EXTRAÍDOS» Y CONSOLA ─────────────────

    /// <summary>
    /// Pide recalcular sólo la tabla «Valores extraídos», dejando pasar medio
    /// segundo de inactividad: al reparsear el texto cacheado no hay OCR, así
    /// que sirve igual con PDFs escaneados que con seleccionables.
    /// </summary>
    private void ProgramarRefrescoValores()
    {
        if (string.IsNullOrEmpty(_rutaPdf)) return;
        _pendienteTabla = true;
        ArrancarTemporizadorRefresco();
    }

    /// <summary>
    /// Pide reextraer el texto del PDF (puede correr OCR) y refrescar consola,
    /// zonas y tabla. Se deja en diferido para que editar una zona no congele
    /// la interfaz con cada celda que se teclea.
    /// </summary>
    private void ProgramarRefrescoVistaPrevia()
    {
        if (string.IsNullOrEmpty(_rutaPdf)) return;
        _pendientePrevia = true;
        ArrancarTemporizadorRefresco();
    }

    private void ArrancarTemporizadorRefresco()
    {
        timerRefrescoValores.Stop();
        timerRefrescoValores.Start();
    }

    private void TimerRefrescoValores_Tick(object? sender, EventArgs e)
    {
        timerRefrescoValores.Stop();

        bool previa = _pendientePrevia;
        bool tabla = _pendienteTabla;
        _pendientePrevia = false;
        _pendienteTabla = false;

        if (_emisorActual == null || string.IsNullOrEmpty(_rutaPdf)) return;

        // Si hay pendiente una reextracción, ésta ya deja la tabla al día.
        if (previa)
            ActualizarVistaPreviaZonal();
        else if (tabla)
            ActualizarTablaValoresExtraidos();
    }

    /// <summary>
    /// Ayuda que depende del PDF de muestra: el aviso «carga un PDF» de la
    /// pestaña Campos y la marca de muestra escaneada (misma comprobación —y
    /// caché— que la lista de emisores, sin pasar por el OCR).
    /// </summary>
    private void ActualizarAyudasMuestra()
    {
        var hayPdf = !string.IsNullOrEmpty(_rutaPdf) && File.Exists(_rutaPdf);

        if (lblSinPdf != null)
            lblSinPdf.Visible = !hayPdf;

        if (lblMuestraEscaneada != null)
            lblMuestraEscaneada.Visible = hayPdf && EsPdfEscaneado(_rutaPdf);
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
            // El tester usa EXACTAMENTE las opciones del motor (case-sensitive
            // y sin Multiline): lo que enseña aquí es lo que se extraerá al
            // procesar. Para tolerancia a mayúsculas o anclas por línea hay
            // que llevarlo en el patrón ((?i), (?m)).
            var regex = new Regex(patron, RegexOptions.Compiled);
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
        public EmisorListItem(EmisorConfig config) => Config = config;

        /// <summary>true si el PDF de muestra de este emisor carece de capa de
        /// texto y se extrae con OCR. Se precalcula en
        /// <see cref="ActualizarMarcasEscaneados"/>: el DisplayMember del
        /// ListBox pinta a menudo y no debe abrir ficheros.</summary>
        public bool Escaneado { get; set; }

        // La marca va ENTRE el NIF y el nombre («A17001231 - ▨ NOMBRE») para
        // que se vea siempre sin hacer scroll: al final de filas largas se
        // iría fuera de pantalla (el ListBox tiene scrollbar horizontal).
        // Ningún código parsea esta cadena: la selección siempre se resuelve
        // con SelectedItem → Config.Nif.
        public string DisplayText =>
            $"{Config.Nif} - " + (Escaneado ? "▨ " : "") + Config.Nombre;
    }
}
