using System.Text.RegularExpressions;
using FacturasApp.Models;
using FacturasApp.Services;
using FacturasApp.Services.Parsers;

namespace FacturasApp.UI
{
    public partial class GestionProveedoresForm : Form
    {
        private readonly ProveedorConfigService _service = new();
        private bool _cargando = false;

        public GestionProveedoresForm()
        {
            InitializeComponent();
            CargarListaProveedores();
        }

        // ── Carga inicial ────────────────────────────────────────────────

        private void CargarListaProveedores()
        {
            _cargando = true;
            lstProveedores.Items.Clear();
            foreach (var nombre in _service.ObtenerNombresProveedores())
                lstProveedores.Items.Add(nombre);
            _cargando = false;
        }

        // ── Selección ────────────────────────────────────────────────────

        private void LstProveedores_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cargando) return;
            if (lstProveedores.SelectedItem is not string nombre || string.IsNullOrEmpty(nombre))
                return;

            var config = _service.ObtenerPorNombre(nombre);
            if (config != null)
                CargarConfigEnUI(config);
        }

        // ── UI → Config / Config → UI ──────────────────────────────────

        private void CargarConfigEnUI(ProveedorConfig config)
        {
            _cargando = true;

            txtNombre.Text = config.Nombre;
            txtNif.Text = config.Nif;
            txtConcepto.Text = config.Concepto;
            cmbModo.SelectedItem = config.ModoExtraccion.ToString();
            chkOmitirNif.Checked = config.DebeOmitirNifEmisor;
            txtIdentificadores.Text = string.Join("\r\n", config.Identificadores);

            // Campos - salir si el grid aún no tiene columnas
            if (dgvCampos.Columns.Count == 0)
            {
                _cargando = false;
                return;
            }
            dgvCampos.Rows.Clear();
            foreach (var campo in config.Campos)
            {
                int idx = dgvCampos.Rows.Add();
                var row = dgvCampos.Rows[idx];
                row.Cells["Campo"].Value = campo.Nombre.ToString();
                row.Cells["Regex"].Value = campo.Regex;
                row.Cells["Grupo"].Value = campo.Grupo;
                row.Cells["ValorFijo"].Value = campo.ValorFijo;
                row.Cells["Cultura"].Value = campo.Cultura;
                row.Cells["Formato"].Value = campo.Formato;
                row.Cells["Opcional"].Value = campo.Opcional;
            }

            // Preprocesamiento
            dgvPreprocesamiento.Rows.Clear();
            foreach (var r in config.Preprocesamiento.Reemplazos)
            {
                int idx = dgvPreprocesamiento.Rows.Add();
                dgvPreprocesamiento.Rows[idx].Cells["Tipo"].Value = "Reemplazar";
                dgvPreprocesamiento.Rows[idx].Cells["Pattern"].Value = r.Pattern;
                dgvPreprocesamiento.Rows[idx].Cells["Reemplazo"].Value = r.Reemplazo;
            }
            foreach (var d in config.Preprocesamiento.EliminarDuplicados)
            {
                int idx = dgvPreprocesamiento.Rows.Add();
                dgvPreprocesamiento.Rows[idx].Cells["Tipo"].Value = "EliminarDuplicados";
                dgvPreprocesamiento.Rows[idx].Cells["Pattern"].Value = d.Tipo.ToString();
            }

            // MultiLineaIva
            var ml = config.MultiLineaIva;
            chkMultiIva.Checked = ml != null;
            txtLineaRegex.Text = ml?.Lineas.FirstOrDefault()?.Regex ?? "";
            txtMapa.Text = ml?.Lineas.FirstOrDefault()?.Mapa ?? "";
            chkDedup.Checked = ml?.Deduplicar ?? false;
            chkExcluirCero.Checked = ml?.ExcluirBaseCero ?? false;
            chkValidarSuma.Checked = ml?.ValidarSumaSubtotales ?? false;
            txtTotalRegex.Text = ml?.TotalFactura?.Regex ?? "";
            txtTotalGrupo.Text = ml?.TotalFactura?.Grupo.ToString() ?? "1";

            // Postprocesamiento
            dgvCondiciones.Rows.Clear();
            if (config.Postprocesamiento != null)
            {
                foreach (var cond in config.Postprocesamiento.Condiciones)
                {
                    if (cond.MoverCampos.Count > 0)
                    {
                        foreach (var m in cond.MoverCampos)
                        {
                            int i = dgvCondiciones.Rows.Add();
                            dgvCondiciones.Rows[i].Cells["Accion"].Value = "Mover";
                            dgvCondiciones.Rows[i].Cells["Campo"].Value = cond.Campo;
                            dgvCondiciones.Rows[i].Cells["Operador"].Value = cond.Operador.ToString();
                            dgvCondiciones.Rows[i].Cells["Valor"].Value = cond.Valor;
                            dgvCondiciones.Rows[i].Cells["Parametro"].Value = $"{m.Origen}→{m.Destino}";
                        }
                    }
                    if (cond.AsignarValoresFijos.Count > 0)
                    {
                        foreach (var a in cond.AsignarValoresFijos)
                        {
                            int i = dgvCondiciones.Rows.Add();
                            dgvCondiciones.Rows[i].Cells["Accion"].Value = "Asignar";
                            dgvCondiciones.Rows[i].Cells["Campo"].Value = cond.Campo;
                            dgvCondiciones.Rows[i].Cells["Operador"].Value = cond.Operador.ToString();
                            dgvCondiciones.Rows[i].Cells["Valor"].Value = cond.Valor;
                            dgvCondiciones.Rows[i].Cells["Parametro"].Value = $"{a.Campo}={a.Valor}";
                        }
                    }
                    if (cond.CopiarCampos.Count > 0)
                    {
                        foreach (var c in cond.CopiarCampos)
                        {
                            int i = dgvCondiciones.Rows.Add();
                            dgvCondiciones.Rows[i].Cells["Accion"].Value = "Copiar";
                            dgvCondiciones.Rows[i].Cells["Campo"].Value = cond.Campo;
                            dgvCondiciones.Rows[i].Cells["Operador"].Value = cond.Operador.ToString();
                            dgvCondiciones.Rows[i].Cells["Valor"].Value = cond.Valor;
                            dgvCondiciones.Rows[i].Cells["Parametro"].Value = $"{c.Origen}→{c.Destino}";
                        }
                    }
                    if (cond.SumarCampos.Count > 0)
                    {
                        foreach (var s in cond.SumarCampos)
                        {
                            int i = dgvCondiciones.Rows.Add();
                            dgvCondiciones.Rows[i].Cells["Accion"].Value = "Sumar";
                            dgvCondiciones.Rows[i].Cells["Campo"].Value = cond.Campo;
                            dgvCondiciones.Rows[i].Cells["Operador"].Value = cond.Operador.ToString();
                            dgvCondiciones.Rows[i].Cells["Valor"].Value = cond.Valor;
                            dgvCondiciones.Rows[i].Cells["Parametro"].Value = $"{s.Destino}+={s.Origen}";
                        }
                    }
                    // Condición pura (sin acciones hijas, solo para mostrar)
                    if (cond.MoverCampos.Count == 0 && cond.AsignarValoresFijos.Count == 0
                        && cond.CopiarCampos.Count == 0 && cond.SumarCampos.Count == 0)
                    {
                        int i = dgvCondiciones.Rows.Add();
                        dgvCondiciones.Rows[i].Cells["Accion"].Value = "Condicion";
                        dgvCondiciones.Rows[i].Cells["Campo"].Value = cond.Campo;
                        dgvCondiciones.Rows[i].Cells["Operador"].Value = cond.Operador.ToString();
                        dgvCondiciones.Rows[i].Cells["Valor"].Value = cond.Valor;
                    }
                }
            }

            _cargando = false;
        }

        private ProveedorConfig LeerConfigDeUI()
        {
            var config = new ProveedorConfig
            {
                Nombre = txtNombre.Text.Trim(),
                Nif = txtNif.Text.Trim(),
                Concepto = txtConcepto.Text.Trim(),
                DebeOmitirNifEmisor = chkOmitirNif.Checked,
            };

            if (Enum.TryParse<ModoExtraccionTexto>(cmbModo.SelectedItem?.ToString(), out var modo))
                config.ModoExtraccion = modo;

            // Identificadores
            config.Identificadores = txtIdentificadores.Text
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            // Campos
            config.Campos = new List<CampoConfig>();
            foreach (DataGridViewRow row in dgvCampos.Rows)
            {
                if (row.IsNewRow) continue;
                var campo = row.Cells["Campo"].Value?.ToString();
                if (string.IsNullOrEmpty(campo)) continue;

                if (!Enum.TryParse<CampoFactura>(campo, out var campoEnum)) continue;

                var cc = new CampoConfig
                {
                    Nombre = campoEnum,
                    Regex = row.Cells["Regex"].Value?.ToString() ?? "",
                    Grupo = int.TryParse(row.Cells["Grupo"].Value?.ToString(), out var g) ? g : 1,
                    ValorFijo = row.Cells["ValorFijo"].Value?.ToString() ?? "",
                    Cultura = row.Cells["Cultura"].Value?.ToString() ?? "es-ES",
                    Formato = row.Cells["Formato"].Value?.ToString() ?? "",
                    Opcional = row.Cells["Opcional"].Value is bool b && b
                };
                config.Campos.Add(cc);
            }

            // Preprocesamiento
            config.Preprocesamiento = new PreprocesamientoConfig();
            foreach (DataGridViewRow row in dgvPreprocesamiento.Rows)
            {
                if (row.IsNewRow) continue;
                var tipo = row.Cells["Tipo"].Value?.ToString();
                if (tipo == "Reemplazar")
                {
                    config.Preprocesamiento.Reemplazos.Add(new ReemplazoConfig
                    {
                        Pattern = row.Cells["Pattern"].Value?.ToString() ?? "",
                        Reemplazo = row.Cells["Reemplazo"].Value?.ToString() ?? ""
                    });
                }
                else if (tipo == "EliminarDuplicados")
                {
                    if (Enum.TryParse<TipoEliminarDuplicados>(row.Cells["Pattern"].Value?.ToString(), out var t))
                        config.Preprocesamiento.EliminarDuplicados.Add(new EliminarDuplicadosConfig { Tipo = t });
                }
            }

            // MultiLineaIva
            if (chkMultiIva.Checked && !string.IsNullOrEmpty(txtLineaRegex.Text))
            {
                config.MultiLineaIva = new MultiLineaIvaConfig
                {
                    CrearFacturaPorLinea = true,
                    Deduplicar = chkDedup.Checked,
                    ExcluirBaseCero = chkExcluirCero.Checked,
                    ValidarSumaSubtotales = chkValidarSuma.Checked,
                };
                config.MultiLineaIva.Lineas.Add(new LineaIvaConfig
                {
                    Regex = txtLineaRegex.Text,
                    Mapa = txtMapa.Text
                });

                if (!string.IsNullOrEmpty(txtTotalRegex.Text))
                {
                    config.MultiLineaIva.TotalFactura = new TotalFacturaConfig
                    {
                        Regex = txtTotalRegex.Text,
                        Grupo = int.TryParse(txtTotalGrupo.Text, out var g) ? g : 1
                    };
                }
            }

            // Postprocesamiento
            config.Postprocesamiento = new PostprocesamientoConfig();
            foreach (DataGridViewRow row in dgvCondiciones.Rows)
            {
                if (row.IsNewRow) continue;
                var accion = row.Cells["Accion"].Value?.ToString();
                var campoCond = row.Cells["Campo"].Value?.ToString() ?? "";
                var operador = row.Cells["Operador"].Value?.ToString() ?? "Igual";
                var valor = row.Cells["Valor"].Value?.ToString() ?? "";
                var parametro = row.Cells["Parametro"].Value?.ToString() ?? "";

                Enum.TryParse<OperadorCondicion>(operador, out var op);

                // Buscar condición existente o crear nueva
                var cond = config.Postprocesamiento.Condiciones
                    .FirstOrDefault(c => c.Campo == campoCond && c.Operador == op && c.Valor == valor);
                if (cond == null)
                {
                    cond = new CondicionConfig
                    {
                        Campo = campoCond,
                        Operador = op,
                        Valor = valor
                    };
                    config.Postprocesamiento.Condiciones.Add(cond);
                }

                switch (accion)
                {
                    case "Mover":
                        var partesM = parametro.Split("→");
                        if (partesM.Length == 2)
                            cond.MoverCampos.Add(new MoverCampoConfig { Origen = partesM[0], Destino = partesM[1] });
                        break;
                    case "Asignar":
                        var partesA = parametro.Split("=");
                        if (partesA.Length == 2)
                            cond.AsignarValoresFijos.Add(new AsignarValorFijoConfig { Campo = partesA[0], Valor = partesA[1] });
                        break;
                    case "Copiar":
                        var partesC = parametro.Split("→");
                        if (partesC.Length == 2)
                            cond.CopiarCampos.Add(new CopiarCampoConfig { Origen = partesC[0], Destino = partesC[1] });
                        break;
                    case "Sumar":
                        var partesS = parametro.Split("+=");
                        if (partesS.Length == 2)
                            cond.SumarCampos.Add(new SumarCampoConfig { Destino = partesS[0].Trim(), Origen = partesS[1].Trim() });
                        break;
                }
            }

            return config;
        }

        // ── CRUD ───────────────────────────────────────────────────────

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            _cargando = true;
            txtNombre.Text = "";
            txtNif.Text = "";
            txtConcepto.Text = "600";
            cmbModo.SelectedIndex = 0;
            chkOmitirNif.Checked = true;
            txtIdentificadores.Text = "";
            dgvCampos.Rows.Clear();
            dgvPreprocesamiento.Rows.Clear();
            chkMultiIva.Checked = false;
            txtLineaRegex.Text = "";
            txtMapa.Text = "";
            chkDedup.Checked = false;
            chkExcluirCero.Checked = false;
            chkValidarSuma.Checked = false;
            txtTotalRegex.Text = "";
            txtTotalGrupo.Text = "1";
            dgvCondiciones.Rows.Clear();
            lstProveedores.SelectedIndex = -1;
            txtNombre.Focus();
            _cargando = false;
        }

        private void BtnClone_Click(object? sender, EventArgs e)
        {
            if (lstProveedores.SelectedItem is not string nombre || string.IsNullOrEmpty(nombre))
                return;

            var original = _service.ObtenerPorNombre(nombre);
            if (original == null) return;

            var copia = new ProveedorConfig
            {
                Nombre = original.Nombre + " (copia)",
                Nif = original.Nif,
                Concepto = original.Concepto,
                ModoExtraccion = original.ModoExtraccion,
                DebeOmitirNifEmisor = original.DebeOmitirNifEmisor,
                Identificadores = [.. original.Identificadores],
                Campos = original.Campos.Select(c => new CampoConfig
                {
                    Nombre = c.Nombre, Regex = c.Regex, Grupo = c.Grupo,
                    ValorFijo = c.ValorFijo, Cultura = c.Cultura, Formato = c.Formato,
                    Opcional = c.Opcional
                }).ToList(),
                Preprocesamiento = new PreprocesamientoConfig
                {
                    Reemplazos = [.. original.Preprocesamiento.Reemplazos.Select(r => new ReemplazoConfig { Pattern = r.Pattern, Reemplazo = r.Reemplazo })],
                    EliminarDuplicados = [.. original.Preprocesamiento.EliminarDuplicados.Select(d => new EliminarDuplicadosConfig { Tipo = d.Tipo })]
                },
                MultiLineaIva = original.MultiLineaIva != null ? new MultiLineaIvaConfig
                {
                    Lineas = [.. original.MultiLineaIva.Lineas.Select(l => new LineaIvaConfig { Regex = l.Regex, Mapa = l.Mapa })],
                    TotalFactura = original.MultiLineaIva.TotalFactura != null
                        ? new TotalFacturaConfig { Regex = original.MultiLineaIva.TotalFactura.Regex, Grupo = original.MultiLineaIva.TotalFactura.Grupo }
                        : null,
                    Deduplicar = original.MultiLineaIva.Deduplicar,
                    ExcluirBaseCero = original.MultiLineaIva.ExcluirBaseCero,
                    ValidarSumaSubtotales = original.MultiLineaIva.ValidarSumaSubtotales,
                    CrearFacturaPorLinea = original.MultiLineaIva.CrearFacturaPorLinea
                } : null,
                Postprocesamiento = new PostprocesamientoConfig
                {
                    Condiciones = [.. original.Postprocesamiento.Condiciones.Select(c => new CondicionConfig
                    {
                        Campo = c.Campo, Operador = c.Operador, Valor = c.Valor,
                        MoverCampos = [.. c.MoverCampos],
                        AsignarValoresFijos = [.. c.AsignarValoresFijos],
                        CopiarCampos = [.. c.CopiarCampos],
                        SumarCampos = [.. c.SumarCampos]
                    })]
                }
            };

            _service.GuardarProveedor(copia);
            CargarListaProveedores();
            lstProveedores.SelectedItem = copia.Nombre;
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lstProveedores.SelectedItem is not string nombre || string.IsNullOrEmpty(nombre))
                return;

            var result = MessageBox.Show(
                $"¿Eliminar el proveedor '{nombre}'?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            _service.EliminarProveedor(nombre);
            CargarListaProveedores();
            BtnAdd_Click(null, EventArgs.Empty);
        }

        // ── Guardar ────────────────────────────────────────────────────

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del proveedor es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNif.Text))
            {
                MessageBox.Show("El NIF del proveedor es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNif.Focus();
                return;
            }

            try
            {
                var config = LeerConfigDeUI();
                _service.GuardarProveedor(config);
                CargarListaProveedores();
                lstProveedores.SelectedItem = config.Nombre;
                MessageBox.Show($"Proveedor '{config.Nombre}' guardado correctamente.",
                    "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Tests ─────────────────────────────────────────────────────

        private void BtnTestRegex_Click(object? sender, EventArgs e)
        {
            var config = LeerConfigDeUI();
            if (config.Campos.Count == 0)
            {
                MessageBox.Show("Define al menos un campo con regex para probar.",
                    "Sin campos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var ofd = new OpenFileDialog
            {
                Filter = "Archivos PDF|*.pdf",
                Title = "Selecciona un PDF de prueba"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string texto;
            var extractor = new PdfTextExtractor();
            try
            {
                // Igual que ProcesarUnPdf: Simple para identificación, luego modo del proveedor
                var textoSimple = extractor.ExtraerTextoSeleccionable(ofd.FileName, PdfTextExtractor.ModoExtraccion.Simple);
                if (string.IsNullOrWhiteSpace(textoSimple))
                {
                    MessageBox.Show("No se pudo extraer texto del PDF.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var modo = config.ModoExtraccion switch
                {
                    ModoExtraccionTexto.Simple => PdfTextExtractor.ModoExtraccion.Simple,
                    ModoExtraccionTexto.LayoutAnalysis => PdfTextExtractor.ModoExtraccion.LayoutAnalysis,
                    _ => PdfTextExtractor.ModoExtraccion.OrdenadoPosicion
                };
                texto = extractor.ExtraerTextoSeleccionable(ofd.FileName, modo) ?? textoSimple;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al extraer texto:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var parser = new DataDrivenParser(config);
            var camposExtraidos = parser.ProbarExtraccion(texto);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Archivo: {Path.GetFileName(ofd.FileName)}");
            sb.AppendLine($"=== Prueba de extracción para '{config.Nombre}' ===\n");

            bool seIdentifica = parser.PuedeParsar(texto);
            sb.AppendLine($"Identificación: {(seIdentifica ? "✔ SÍ" : "✖ NO")}");
            sb.AppendLine();

            foreach (var campo in config.Campos)
            {
                if (!string.IsNullOrEmpty(campo.ValorFijo))
                {
                    sb.AppendLine($"[{campo.Nombre}] = (ValorFijo: {campo.ValorFijo})");
                    continue;
                }
                if (string.IsNullOrEmpty(campo.Regex))
                {
                    sb.AppendLine($"[{campo.Nombre}] = (sin regex)");
                    continue;
                }

                if (camposExtraidos.TryGetValue(campo.Nombre, out var val) && !string.IsNullOrEmpty(val))
                    sb.AppendLine($"[{campo.Nombre}] = {val}");
                else
                    sb.AppendLine($"[{campo.Nombre}] = (NO MATCH)");
            }

            using var formResultado = new Form
            {
                Text = $"Resultados - {Path.GetFileName(ofd.FileName)}",
                Size = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent,
                ShowIcon = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false
            };
            var txtResultado = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9), BackColor = Color.FromArgb(245, 245, 245),
                Dock = DockStyle.Fill, Text = sb.ToString()
            };
            formResultado.Controls.Add(txtResultado);
            formResultado.ShowDialog(this);
        }

        private void BtnTestIdent_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Archivos PDF|*.pdf",
                Title = "Selecciona un PDF para identificar el proveedor"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string texto;
            try
            {
                var extractor = new PdfTextExtractor();
                texto = extractor.ExtraerTextoSeleccionable(ofd.FileName, PdfTextExtractor.ModoExtraccion.Simple) ?? "";
                if (string.IsNullOrWhiteSpace(texto))
                {
                    MessageBox.Show("No se pudo extraer texto del PDF.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al extraer texto:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Archivo: {Path.GetFileName(ofd.FileName)}");
            sb.AppendLine("=== Resultados de identificación ===\n");

            // Buscar en XML config
            var xmlConfig = _service.ObtenerPorIdentificadores(texto);
            if (xmlConfig != null)
            {
                sb.AppendLine("✔ Configuración XML:");
                sb.AppendLine($"   Nombre: {xmlConfig.Nombre}");
                sb.AppendLine($"   NIF: {xmlConfig.Nif}");
                sb.AppendLine($"   Identificadores usados: {string.Join(", ", xmlConfig.Identificadores)}");
            }
            else
            {
                sb.AppendLine("✖ No coincide con ninguna configuración XML.");
            }

            sb.AppendLine();

            // Buscar en parsers code-behind
            var factory = new ParserFactory();
            var parser = factory.ObtenerParser(texto);
            sb.AppendLine($"Parser code-behind: {parser.Nombre}");
            sb.AppendLine($"   Match: {(parser.Nombre != "Parser Genérico" ? "✔ SÍ" : "✖ NO (se usará GenericParser)")}");

            using var formResultado = new Form
            {
                Text = $"Identificación - {Path.GetFileName(ofd.FileName)}",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent,
                ShowIcon = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false
            };
            var txtResultado = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill, Text = sb.ToString()
            };
            formResultado.Controls.Add(txtResultado);
            formResultado.ShowDialog(this);
        }

        // ── Eventos de UI ─────────────────────────────────────────────

        private void ChkMultiIva_CheckedChanged(object? sender, EventArgs e)
        {
            bool enabled = chkMultiIva.Checked;
            txtLineaRegex.Enabled = enabled;
            txtMapa.Enabled = enabled;
            chkDedup.Enabled = enabled;
            chkExcluirCero.Enabled = enabled;
            chkValidarSuma.Enabled = enabled;
            txtTotalRegex.Enabled = enabled;
            txtTotalGrupo.Enabled = enabled;
        }

        private void DgvCampos_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Grupo"].Value = 1;
            e.Row.Cells["Cultura"].Value = "es-ES";
            e.Row.Cells["Opcional"].Value = false;
        }

        private void DgvPreprocesamiento_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Tipo"].Value = "Reemplazar";
        }

        private void DgvCondiciones_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Accion"].Value = "Condicion";
            e.Row.Cells["Operador"].Value = "Igual";
        }
    }
}
