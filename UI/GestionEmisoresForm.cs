using FacturasApp.Models;
using FacturasApp.Services;

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

        public GestionEmisoresForm()
        {
            InitializeComponent();
            ConstruirTabDatos();
            CargarEmisores();
        }

        /// <summary>
        /// Construye el contenido dinámico del tab "Datos" que el Designer no puede procesar.
        /// Debe llamarse después de InitializeComponent().
        /// </summary>
        private void ConstruirTabDatos()
        {
            int y = 12;
            AddLabel(tabDatos, "ID (clave interna):", ref y);
            txtId = AddTextBox(tabDatos, ref y, true);
            y += 8;

            AddLabel(tabDatos, "Nombre del emisor:", ref y);
            txtNombre = AddTextBox(tabDatos, ref y);
            y += 8;

            AddLabel(tabDatos, "NIF (clave única):", ref y);
            txtNif = AddTextBox(tabDatos, ref y);
            y += 8;

            AddLabel(tabDatos, "Concepto contable:", ref y);
            txtConcepto = AddTextBox(tabDatos, ref y, false, 80);
            y += 8;

            AddLabel(tabDatos, "Identificadores (uno por línea):", ref y);
            txtIdentificadores = new TextBox
            {
                Location = new Point(12, y),
                Width = 400,
                Height = 80,
                Multiline = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            tabDatos.Controls.Add(txtIdentificadores);
            y += 88;

            AddLabel(tabDatos, "Modo de extracción:", ref y);
            cmbModoExtraccion = new ComboBox
            {
                Location = new Point(12, y),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbModoExtraccion.Items.AddRange(new object[]
                { "OrdenadoPosicion", "Simple", "LayoutAnalysis" });
            cmbModoExtraccion.SelectedIndex = 0;
            tabDatos.Controls.Add(cmbModoExtraccion);
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
            CargarZonasEnGrid();

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
            CargarZonasEnGrid();

            lblEstado.Text = "Nuevo emisor (sin guardar)";
            tabsEditor.Enabled = true;
            tabsEditor.SelectedTab = tabDatos;
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
                tabsEditor.SelectedTab = tabDatos;
                txtNif.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del emisor es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabsEditor.SelectedTab = tabDatos;
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
                    tabsEditor.SelectedTab = tabDatos;
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
            dgvCampos.Rows.Clear();
            foreach (var campo in _camposEditando)
            {
                dgvCampos.Rows.Add(campo.Nombre, campo.Tipo, campo.Regex ?? "",
                    campo.Grupo, campo.ValorFijo ?? "");
            }
        }

        private void BtnAgregarCampo_Click(object? sender, EventArgs e)
        {
            var nuevo = new CampoExtraccion { Nombre = "CampoNuevo", Tipo = "Texto" };
            _camposEditando.Add(nuevo);
            dgvCampos.Rows.Add(nuevo.Nombre, nuevo.Tipo, "", 1, "");
        }

        private void BtnEliminarCampo_Click(object? sender, EventArgs e)
        {
            if (dgvCampos.CurrentRow?.Index is int idx && idx >= 0 && idx < _camposEditando.Count)
            {
                _camposEditando.RemoveAt(idx);
                dgvCampos.Rows.RemoveAt(idx);
            }
        }

        // ── Reglas ───────────────────────────────────────────────────────────

        private void CargarReglasEnGrid()
        {
            dgvReglas.Rows.Clear();
            foreach (var regla in _reglasEditando)
            {
                string condicion = regla.Condicion != null
                    ? (regla.Condicion.TextoContiene != null
                        ? $"Texto contiene \"{regla.Condicion.TextoContiene}\""
                        : $"{regla.Condicion.Campo} {regla.Condicion.Operador} {regla.Condicion.Valor}")
                    : "(siempre)";

                string acciones = string.Join("; ",
                    regla.Acciones.Select(a => $"{a.Tipo}({a.Campo}, {a.Valor})"));

                dgvReglas.Rows.Add(regla.Nombre, condicion, acciones);
            }
        }

        private void BtnAgregarRegla_Click(object? sender, EventArgs e)
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

        private void BtnEliminarRegla_Click(object? sender, EventArgs e)
        {
            if (dgvReglas.CurrentRow?.Index is int idx && idx >= 0 && idx < _reglasEditando.Count)
            {
                _reglasEditando.RemoveAt(idx);
                CargarReglasEnGrid();
            }
        }

        // ── Zonas OCR ────────────────────────────────────────────────────────

        private void CargarZonasEnGrid()
        {
            dgvZonas.Rows.Clear();
            foreach (var zona in _zonasEditando)
            {
                dgvZonas.Rows.Add(zona.Campo, zona.Pagina,
                    zona.X, zona.Y, zona.Ancho, zona.Alto);
            }
        }

        private void BtnAgregarZona_Click(object? sender, EventArgs e)
        {
            var nueva = new ZonaOcrDefinicion
            {
                Campo = $"P1_Z{_zonasEditando.Count + 1}",
                Pagina = 1
            };
            _zonasEditando.Add(nueva);
            CargarZonasEnGrid();
        }

        private void BtnEliminarZona_Click(object? sender, EventArgs e)
        {
            if (dgvZonas.CurrentRow?.Index is int idx && idx >= 0 && idx < _zonasEditando.Count)
            {
                _zonasEditando.RemoveAt(idx);
                CargarZonasEnGrid();
            }
        }

        // ── Tester ───────────────────────────────────────────────────────────

        private void BtnSeleccionarPdf_Click(object? sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar PDF de prueba",
                Filter = "Archivos PDF (*.pdf)|*.pdf"
            };

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                txtRutaPdf.Text = dialogo.FileName;
            }
        }

        private void BtnDetectarEmisor_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtRutaPdf.Text) || !File.Exists(txtRutaPdf.Text))
            {
                MessageBox.Show("Selecciona un PDF válido primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var textExtractor = new PdfTextExtractor();
                string? texto = textExtractor.ExtraerTextoSeleccionable(
                    txtRutaPdf.Text, PdfTextExtractor.ModoExtraccion.OrdenadoPosicion);

                if (texto == null)
                {
                    var ocr = new OcrExtractor();
                    texto = ocr.ExtraerTextoConOcr(txtRutaPdf.Text);
                }

                txtTextoExtraido.Text = texto?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);

                // Buscar emisor que matchee
                var todosLosEmisores = _emisorService.ObtenerTodos();
                var emisorDetectado = todosLosEmisores.FirstOrDefault(e =>
                    e.Identificadores.All(id =>
                        texto.Contains(id, StringComparison.OrdinalIgnoreCase)));

                if (emisorDetectado != null)
                {
                    // Seleccionar en la lista
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

        private void BtnProbarExtraccion_Click(object? sender, EventArgs e)
        {
            if (_emisorActual == null)
            {
                MessageBox.Show("Selecciona o crea un emisor primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtTextoExtraido.Text))
            {
                MessageBox.Show("Primero carga un PDF y extrae su texto (botón Detectar Emisor).",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var resultado = _fieldExtractor.ExtraerCamposParaTest(
                    _emisorActual, txtTextoExtraido.Text);

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
