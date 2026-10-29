using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using FacturasApp.Core.Models;
using FacturasApp.Core.Services;
using FacturasApp.Services;

namespace FacturasApp.UI
{
    public partial class MainForm : Form
    {
        public static Icon DefaultIcon { get; } =
            (Icon)new ComponentResourceManager(typeof(MainForm)).GetObject("$this.Icon")!;

        private readonly InvoiceProcessorService _procesador;
        private readonly ExportService _exportador = new();
        private List<Factura> _facturas = new();

        // Propiedades de FacturaGridRow que corresponden a las columnas
        // visibles del grid (se resuelven una sola vez en
        // ConfigurarDataGridView) y caché del texto de búsqueda por factura,
        // para no repetir la reflexión en cada tecla que se pulsa en el filtro.
        private PropertyInfo[] _propiedadesFiltro = Array.Empty<PropertyInfo>();
        private readonly Dictionary<Factura, string> _cacheTextoBusqueda = new();

        public MainForm()
        {
            // Configurar el servicio ANTES de InitializeComponent()
            _procesador = CrearProcesadorConfigurado();

            InitializeComponent();

            this.Text = $"Gestión de Facturas V{VersionPublicada()}";

            // Make these controls adapt to the window width
            dgvFacturas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtRutaExcel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            lstArchivos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            ConfigurarDataGridView();
            ConfigurarComboEstado();
        }

        /// <summary>
        /// Versión de la publicación en sus cuatro niveles (p. ej. 3.2.1.16)
        /// para el título de la ventana. Application.ProductVersion es
        /// &lt;InformationalVersion&gt;: en un build normal viene «3.2.1+&lt;sha&gt;»
        /// (sólo tres niveles) y al publicar el csproj hornea la
        /// ApplicationVersion completa con la revisión ClickOnce; aquí sólo
        /// hace falta normalizar —quitar el «+», cortar a cuatro componentes y
        /// rellenar los que falten con 0— para que SIEMPRE salgan cuatro.
        /// </summary>
        private static string VersionPublicada()
        {
            var componentes = Application.ProductVersion.Split('+')[0].Split('.');

            var niveles = new string[4];
            for (int i = 0; i < niveles.Length; i++)
                niveles[i] = i < componentes.Length && componentes[i].Length > 0
                    ? componentes[i]
                    : "0";

            return string.Join('.', niveles);
        }

        // ── Configuración centralizada del procesador ─────────────────────────

        private InvoiceProcessorService CrearProcesadorConfigurado()
        {
            // Ruta a tessdata - carpeta en el directorio de ejecución
            string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            // Si la carpeta no existe, intentamos con ruta relativa
            if (!Directory.Exists(tessDataPath))
                tessDataPath = @"./tessdata";

            var textExtractor = new WinFormsTextExtractor(tessDataPath);
            var service = new InvoiceProcessorService(textExtractor);

            // Configuración óptima para usar zonas cuando sea posible
            service.UsarZonasSiempre = true;
            service.FallbackATextoCompleto = true;

            return service;
        }

        // ── Selección de archivos (PDF e imágenes) ───────────────────────────

        private void btnSeleccionarArchivos_Click(object sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar facturas (PDF o imagen)",
                Filter = FormatosDocumento.FiltroDialogo,
                Multiselect = true,
                InitialDirectory = Environment.GetFolderPath(
                                       Environment.SpecialFolder.MyDocuments)
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            foreach (string ruta in dialogo.FileNames)
                if (!lstArchivos.Items.Contains(ruta))
                    lstArchivos.Items.Add(ruta);

            ActualizarContadorArchivos();
            btnProcesar.Enabled = lstArchivos.Items.Count > 0;
        }

        private void btnSeleccionarCarpeta_Click(object sender, EventArgs e)
        {
            using var dialogo = new FolderBrowserDialog
            {
                Description = "Seleccionar carpeta con facturas (PDF o imagen)",
                UseDescriptionForTitle = true
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            var facturas = Directory
                .GetFiles(dialogo.SelectedPath, "*.*", SearchOption.AllDirectories)
                .Where(FormatosDocumento.EsFactura)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase);

            foreach (string ruta in facturas)
                if (!lstArchivos.Items.Contains(ruta))
                    lstArchivos.Items.Add(ruta);

            ActualizarContadorArchivos();
            btnProcesar.Enabled = lstArchivos.Items.Count > 0;
        }

        // ── Selección de Excel ────────────────────────────────────────────────

        private void btnSeleccionarExcel_Click(object sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar Excel con facturas",
                Filter = "Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
                InitialDirectory = Environment.GetFolderPath(
                                       Environment.SpecialFolder.MyDocuments)
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            txtRutaExcel.Text = dialogo.FileName;
            btnProcesar.Enabled = true;

            // Avisamos si hay columnas no reconocidas
            var noReconocidas = _procesador
                .ObtenerColumnasNoReconocidas(dialogo.FileName);

            if (noReconocidas.Count > 0)
                MessageBox.Show(
                    $"Las siguientes columnas no se importarán:\n\n" +
                    string.Join("\n", noReconocidas.Select(c => $"  • {c}")),
                    "Columnas no reconocidas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLimpiarLista_Click(object sender, EventArgs e)
        {
            lstArchivos.Items.Clear();
            txtRutaExcel.Text = string.Empty;
            dgvFacturas.DataSource = null;

            // Se vacían también las facturas y su caché de búsqueda: si no,
            // al teclear en el filtro se repoblaría la tabla con facturas
            // de un lote anterior
            _facturas.Clear();
            _cacheTextoBusqueda.Clear();

            // Y el panel de estadísticas, que sólo describe el lote actual
            LimpiarChips();
            pnlEstados.Visible = false;

            lblEstado.Text = "Listo";
            btnProcesar.Enabled = false;
            ActualizarContadorArchivos();
        }

        // ── Procesado ─────────────────────────────────────────────────────────

        private async void btnProcesar_Click(object sender, EventArgs e)
        {
            var rutasPdf = lstArchivos.Items.Cast<string>().ToList();
            var rutaExcel = txtRutaExcel.Text.Trim();

            if (rutasPdf.Count == 0 && string.IsNullOrEmpty(rutaExcel))
            {
                MessageBox.Show("Selecciona al menos un PDF o un archivo Excel.",
                    "Sin archivos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetUiEstadoProcesando(true);
            progressBar.Maximum = Math.Max(rutasPdf.Count, 1);
            progressBar.Value = 0;
            lblEstado.Text = "Iniciando procesado...";

            var progreso = new Progress<(int actual, int total, string archivo)>(p =>
            {
                progressBar.Value = Math.Min(p.actual, progressBar.Maximum);
                lblEstado.Text = $"Procesando {p.actual}/{p.total}: {p.archivo}";
                lblPorcentaje.Text = $"{(int)(p.actual * 100.0 / p.total)}%";
            });

            try
            {
                _facturas = await Task.Run(() =>
                    _procesador.ProcesarMixto(rutasPdf, rutaExcel, progreso));

                MostrarResultadosEnGrid(_facturas);
                MostrarResumenEstadisticas(_facturas);

                lblEstado.Text = $"✔ Completado — {_facturas.Count} facturas";
                // Habilitamos los botones de exportar Excel (Ingresos y Gastos)
                btnExportarExcelIngresos.Enabled = true;
                btnExportarExcelGastos.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante el procesado:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "Error en el procesado.";
            }
            finally
            {
                SetUiEstadoProcesando(false);
            }
        }

        // ── DataGridView ──────────────────────────────────────────────────────

        private void ConfigurarDataGridView()
        {
            dgvFacturas.AutoGenerateColumns = false;
            dgvFacturas.AllowUserToAddRows = false;
            dgvFacturas.ReadOnly = true;
            dgvFacturas.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvFacturas.RowHeadersVisible = false;
            dgvFacturas.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(245, 245, 245);

            dgvFacturas.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                    { Name = "colNumero",   HeaderText = "Nº Factura",
                      DataPropertyName = "NumeroFactura",   Width = 110 },
                new DataGridViewTextBoxColumn
                    { Name = "colFecha",    HeaderText = "Fecha",
                      DataPropertyName = "FechaFormateada", Width = 90  },
                new DataGridViewTextBoxColumn
                    { Name = "colEmisor",   HeaderText = "Emisor",
                      DataPropertyName = "EmisorNombre",    Width = 140 },
                new DataGridViewTextBoxColumn
                    { Name = "colNifEmisor", HeaderText = "NIF Emisor",
                      DataPropertyName = "EmisorNif",       Width = 95  },
                new DataGridViewTextBoxColumn
                    { Name = "colCliente",  HeaderText = "Cliente",
                      DataPropertyName = "ClienteNombre",   Width = 140 },
                new DataGridViewTextBoxColumn
                    { Name = "colNifCliente", HeaderText = "NIF Cliente",
                      DataPropertyName = "ClienteNif",      Width = 95  },
                new DataGridViewTextBoxColumn
                    { Name = "colBase",     HeaderText = "Base",
                      DataPropertyName = "BaseFormateada",  Width = 90,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colIvaPct",   HeaderText = "% IVA",
                      DataPropertyName = "PorcentajeIVA",   Width = 55,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colIvaCuota", HeaderText = "Cuota IVA",
                      DataPropertyName = "CuotaIvaFmt",     Width = 90,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colIrpfPct",  HeaderText = "% IRPF",
                      DataPropertyName = "PorcentajeIRPF",  Width = 60,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colIrpfCuota", HeaderText = "Cuota IRPF",
                      DataPropertyName = "CuotaIrpfFmt",    Width = 90,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colREPct",    HeaderText = "% RE",
                      DataPropertyName = "PorcentajeRE",    Width = 55,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colRECuota",  HeaderText = "Cuota RE",
                      DataPropertyName = "CuotaREFmt",      Width = 90,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colTotal",    HeaderText = "Total",
                      DataPropertyName = "TotalFormateado", Width = 100,
                      DefaultCellStyle  = { Alignment =
                          DataGridViewContentAlignment.MiddleRight }},
                new DataGridViewTextBoxColumn
                    { Name = "colEstado",   HeaderText = "Estado",
                      DataPropertyName = "EstadoTexto",     Width = 130 },
                new DataGridViewCheckBoxColumn
                    { Name = "colOcr",      HeaderText = "OCR",
                      DataPropertyName = "ExtractedByOcr",  Width = 45  },
                new DataGridViewTextBoxColumn
                    { Name = "colArchivo",  HeaderText = "Archivo",
                      DataPropertyName = "NombreArchivo",   Width = 180 },
                        });
            dgvFacturas.CellDoubleClick += DgvFacturas_CellDoubleClick;
            dgvFacturas.RowPrePaint += DgvFacturas_RowPrePaint;

            // El filtro de texto se aplica sobre TODAS las columnas visibles:
            // se toman sus DataPropertyName y se resuelven una sola vez contra
            // FacturaGridRow (así, si se añade o quita una columna, el filtro
            // la sigue sin tocar este método).
            var nombresColumnas = dgvFacturas.Columns
                .Cast<DataGridViewColumn>()
                .Where(c => c.Visible && !string.IsNullOrEmpty(c.DataPropertyName))
                .Select(c => c.DataPropertyName)
                .ToHashSet(StringComparer.Ordinal);

            _propiedadesFiltro = typeof(FacturaGridRow)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && nombresColumnas.Contains(p.Name))
                .ToArray();
        }

        private void ConfigurarComboEstado()
        {
            cmbFiltroEstado.Items.AddRange(
                new object[] { "Todos", "Pendiente", "OK", "Revisar", "Duplicada", "Error" });
            cmbFiltroEstado.SelectedIndex = 0;
        }

        private void MostrarResultadosEnGrid(List<Factura> facturas)
        {
            // Lote nuevo: los textos de búsqueda cacheados dejan de servir
            _facturas = facturas;
            _cacheTextoBusqueda.Clear();

            // Se reutiliza AplicarFiltros() para que un filtro de texto o de
            // estado ya escrito siga vigente con el nuevo lote (antes se
            // mostraban todas las facturas y se perdía el filtro)
            AplicarFiltros();
        }

        private void DgvFacturas_RowPrePaint(object? sender,
            DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // ← Obtenemos la factura directamente desde el DTO de la fila
            if (dgvFacturas.Rows[e.RowIndex].DataBoundItem
                is not FacturaGridRow fila) return;

            dgvFacturas.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                fila.FacturaOriginal.Estado.ToColor();
        }

        private void DgvFacturas_CellDoubleClick(object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // ← Obtenemos la factura directamente desde el DTO de la fila
            // Esto funciona correctamente tanto con filtro como sin él
            if (dgvFacturas.Rows[e.RowIndex].DataBoundItem
                is not FacturaGridRow fila) return;

            // using var detalle = new DetalleFacturaForm(fila.FacturaOriginal);
            // detalle.ShowDialog(this);

            using var detalle2 = new DetalleFacturaForm(fila.FacturaOriginal);
            detalle2.ShowDialog(this);
        }

        // ── Estadísticas ──────────────────────────────────────────────────────

        /// <summary>
        /// Fuente de las «chips» del panel de estados (una sola instancia para
        /// no crear una fuente por chip y por lote).
        /// </summary>
        private static readonly Font FuenteChips = new("Segoe UI", 8.25F);

        /// <summary>Fuente del chip cuyo estado filtra el listado ahora mismo.</summary>
        private static readonly Font FuenteChipsActiva =
            new("Segoe UI", 8.25F, FontStyle.Bold);

        /// <summary>Chip de cada estado, para marcar en negrita el filtro activo.</summary>
        private readonly Dictionary<EstadoFactura, Label> _chipsEstado = new();

        /// <summary>
        /// Pinta el resumen del lote COMPLETO (no del grid filtrado): una chip
        /// por estado —con recuento y porcentaje, en el color del grid y
        /// pulsable para filtrar— más documentos únicos, OCR e importe sin
        /// errores. Lo calcula <see cref="EstadisticasLote"/>.
        /// </summary>
        private void MostrarResumenEstadisticas(List<Factura> facturas)
        {
            var estadisticas = EstadisticasLote.Calcular(facturas);

            pnlEstados.SuspendLayout();
            LimpiarChips();

            foreach (var estado in estadisticas.Estados)
            {
                Label chip = CrearChip(
                    $"{estado.Estado.ToDisplayText()} {estado.Filas} " +
                    $"({estado.PorcentajeFilas:N1} %)",
                    estado.Estado.ToColor(), estado.Estado);

                _chipsEstado[estado.Estado] = chip;
                pnlEstados.Controls.Add(chip);
            }

            pnlEstados.Controls.Add(
                CrearChip($"Documentos: {estadisticas.TotalDocumentos}"));
            pnlEstados.Controls.Add(
                CrearChip($"OCR: {estadisticas.ExtraidasPorOcr}"));
            pnlEstados.Controls.Add(
                CrearChip($"Importe: {estadisticas.ImporteSinErrores:N2} €"));

            pnlEstados.ResumeLayout();
            pnlEstados.Visible = facturas.Count > 0;
            MarcarChipActivo();
        }

        /// <summary>
        /// Crea una chip del panel. Sólo las de estado son pulsables (cursiva
        /// de mano + clic para filtrar); el resto son informativas.
        /// </summary>
        private Label CrearChip(string texto, Color? color = null,
            EstadoFactura? estado = null)
        {
            var chip = new Label
            {
                AutoSize = true,
                BackColor = color ?? Color.FromArgb(238, 238, 238),
                BorderStyle = BorderStyle.FixedSingle,
                Font = FuenteChips,
                Margin = new Padding(0, 0, 8, 4),
                Padding = new Padding(6, 2, 6, 2),
                Text = texto
            };

            if (estado is EstadoFactura estadoChip)
            {
                chip.Cursor = Cursors.Hand;
                chip.Click += (_, _) => AlternarFiltroEstado(estadoChip);
            }

            return chip;
        }

        /// <summary>
        /// Chip pulsado: filtra el listado por ese estado; si ya era el filtro
        /// activo, lo quita y vuelve a «Todos».
        /// </summary>
        private void AlternarFiltroEstado(EstadoFactura estado)
        {
            string actual = cmbFiltroEstado.SelectedItem?.ToString() ?? "Todos";

            cmbFiltroEstado.SelectedItem = actual == estado.ToString()
                ? "Todos"
                : estado.ToString();
        }

        /// <summary>Marca en negrita la chip del estado que filtra el listado.</summary>
        private void MarcarChipActivo()
        {
            string actual = cmbFiltroEstado.SelectedItem?.ToString() ?? "Todos";

            foreach (var (estado, chip) in _chipsEstado)
                chip.Font = estado.ToString() == actual
                    ? FuenteChipsActiva
                    : FuenteChips;
        }

        /// <summary>
        /// Vacía el panel de chips liberando los controles (Controls.Clear no
        /// los dispone y cada uno tiene su handle GDI).
        /// </summary>
        private void LimpiarChips()
        {
            _chipsEstado.Clear();

            var chips = pnlEstados.Controls.Cast<Control>().ToList();
            pnlEstados.Controls.Clear();

            foreach (Control chip in chips)
                chip.Dispose();
        }

        // ── Filtros ───────────────────────────────────────────────────────────

        private void txtFiltro_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
            MarcarChipActivo();
        }

        private void AplicarFiltros()
        {
            if (_facturas.Count == 0)
            {
                dgvFacturas.DataSource = null;
                return;
            }

            string estadoFiltro = cmbFiltroEstado.SelectedItem?.ToString() ?? "Todos";

            // Búsqueda por palabras sueltas: TODAS deben aparecer en la fila,
            // en cualquiera de sus columnas
            var palabras = txtFiltro.Text.Split(' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

            var filtradas = _facturas
                // Primero el estado (sobre la factura, sin crear filas)
                .Where(f => estadoFiltro == "Todos" ||
                            f.Estado.ToString() == estadoFiltro)
                .Select(f => new FacturaGridRow(f))
                // Y después el texto, en cualquier columna de la fila
                .Where(fila =>
                {
                    if (palabras.Length == 0) return true;

                    string texto = TextoBusqueda(fila);
                    return palabras.All(p =>
                        texto.Contains(p, StringComparison.OrdinalIgnoreCase));
                })
                .ToList();

            dgvFacturas.DataSource = new BindingSource { DataSource = filtradas };
        }

        /// <summary>
        /// Texto de búsqueda de una fila: concatenación del valor de TODAS las
        /// columnas visibles del grid (exactamente las que se pintan), en su
        /// formato de visualización. Se cachea por factura para no repetir
        /// reflexión en cada tecla.
        /// </summary>
        private string TextoBusqueda(FacturaGridRow fila)
        {
            if (_cacheTextoBusqueda.TryGetValue(fila.FacturaOriginal,
                    out string? cacheado))
                return cacheado;

            var textos = new List<string>();

            foreach (PropertyInfo propiedad in _propiedadesFiltro)
            {
                object? valor = propiedad.GetValue(fila);
                if (valor is null) continue;

                // La columna OCR es booleana: se busca como «Sí»/«No»,
                // igual que se ve en la tabla
                string texto = valor is bool marcado
                    ? (marcado ? "Sí" : "No")
                    : Convert.ToString(valor, CultureInfo.CurrentCulture)
                        ?? string.Empty;

                if (texto.Length > 0) textos.Add(texto);
            }

            string resultado = string.Join(' ', textos);
            _cacheTextoBusqueda[fila.FacturaOriginal] = resultado;
            return resultado;
        }

        // ── Exportación ───────────────────────────────────────────────────────

        // Nuevo: Exportar Excel — Ingresos
        private void btnExportarExcelIngresos_Click(object sender, EventArgs e)
        {
            using var dialogo = new SaveFileDialog
            {
                Title = "Guardar como Excel (Ingresos)",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"Facturas_Ingresos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",

                // Usamos la carpeta de la primera factura selecciona para mayor comodidad
                InitialDirectory = _facturas.Count > 0
                    ? Path.GetDirectoryName(_facturas[0].RutaArchivo) ?? Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments)
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            try
            {
                _exportador.ExportarAExcel(_facturas, dialogo.FileName, esGasto: false);
                MostrarExitoExportacion(dialogo.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Nuevo: Exportar Excel — Gastos
        private void btnExportarExcelGastos_Click(object sender, EventArgs e)
        {
            using var dialogo = new SaveFileDialog
            {
                Title = "Guardar como Excel (Gastos)",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"Facturas_Gastos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                // Usamos la carpeta de la primera factura selecciona para mayor comodidad
                InitialDirectory = _facturas.Count > 0
                    ? Path.GetDirectoryName(_facturas[0].RutaArchivo) ?? Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments)
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Exportar todas las facturas sin alterarlas
                _exportador.ExportarAExcel(_facturas, dialogo.FileName, esGasto: true);
                MostrarExitoExportacion(dialogo.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Gestionar emisores (CRUD XML)
        private void btnGestionEmisores_Click(object sender, EventArgs e)
        {
            using var form = new GestionEmisoresForm();
            form.ShowDialog(this);
        }

        private void MostrarExitoExportacion(string rutaArchivo)
        {
            var resultado = MessageBox.Show(
                $"Archivo guardado:\n{rutaArchivo}\n\n¿Deseas abrirlo?",
                "Exportación completada",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (resultado == DialogResult.Yes)
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaArchivo,
                        UseShellExecute = true
                    });
        }

        // ── Helpers UI ────────────────────────────────────────────────────────

        private void SetUiEstadoProcesando(bool procesando)
        {
            btnSeleccionarArchivos.Enabled = !procesando;
            btnSeleccionarCarpeta.Enabled = !procesando;
            btnSeleccionarExcel.Enabled = !procesando;
            btnLimpiarLista.Enabled = !procesando;
            btnProcesar.Enabled = !procesando;
            // Nuevos botones de exportación Excel
            btnExportarExcelIngresos.Enabled = !procesando;
            btnExportarExcelGastos.Enabled = !procesando;
            btnGestionEmisores.Enabled = !procesando;
            progressBar.Visible = procesando;
            lblPorcentaje.Visible = procesando;
        }

        private void ActualizarContadorArchivos()
        {
            int n = lstArchivos.Items.Count;
            lblContadorArchivos.Text = n == 0
                ? "Sin archivos seleccionados"
                : $"{n} archivo{(n == 1 ? "" : "s")} seleccionado{(n == 1 ? "" : "s")}";
        }
    }
}