using System.Text.RegularExpressions;
using FacturasApp.Models.EmisoresConfig;
using FacturasApp.Services;

namespace FacturasApp.UI;

public partial class GestionEmisoresForm : Form
{
    private readonly ConfiguracionEmisores _configuracion = new();
    private EmisorConfig? _emisorActual;
    private bool _modificado;
    private bool _cargando;

    public GestionEmisoresForm()
    {
        InitializeComponent();
        CargarEmisores();
        FormClosing += (_, args) =>
        {
            if (_modificado)
            {
                var r = MessageBox.Show("Hay cambios sin guardar. ¿Salir sin guardar?",
                    "Cambios no guardados", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) args.Cancel = true;
            }
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

    private void CmbCampoNombre_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_emisorActual != null) MarcarModificado();
    }

    private void BtnRegexApplyToField_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtRegexPattern.Text)) return;
        tabs.SelectedIndex = 1;
        if (lstCampos.SelectedItem is CampoConfig campo)
            txtCampoRegex.Text = txtRegexPattern.Text;
    }

    private void DgvCellValueChanged(object? sender, DataGridViewCellEventArgs e) => MarcarModificado();
    private void DgvUserAddedRow(object? sender, DataGridViewRowEventArgs e) => MarcarModificado();
    private void DgvUserDeletedRow(object? sender, DataGridViewRowEventArgs e) => MarcarModificado();

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

    private void LstCampos_SelectedIndexChanged(object? sender, EventArgs e)
    {
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
    }

    private void BtnCampoAdd_Click(object? sender, EventArgs e)
    {
        if (_emisorActual == null) return;
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
