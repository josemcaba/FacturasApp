using FacturasApp.Models;
using FacturasApp.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Linq;

namespace FacturasApp.UI
{
    public partial class MainForm : Form
    {
        private readonly InvoiceProcessorService _procesador = new();
        private readonly ExportService _exportador = new();
        private List<Factura> _facturas = new();

        public MainForm()
        {
            InitializeComponent();
            ConfigurarDataGridView();
            ConfigurarComboEstado();
        }

        // ── Selección de archivos PDF ─────────────────────────────────────────

        private void btnSeleccionarArchivos_Click(object sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar facturas PDF",
                Filter = "Archivos PDF (*.pdf)|*.pdf",
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
                Description = "Seleccionar carpeta con facturas PDF",
                UseDescriptionForTitle = true
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            var pdfs = Directory.GetFiles(
                dialogo.SelectedPath, "*.pdf", SearchOption.AllDirectories);

            foreach (string ruta in pdfs)
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
        }

        private void ConfigurarComboEstado()
        {
            cmbFiltroEstado.Items.AddRange(
                new object[] { "Todos", "OK", "RevisionManual", "Error" });
            cmbFiltroEstado.SelectedIndex = 0;
        }

        private void MostrarResultadosEnGrid(List<Factura> facturas)
        {
            var filas = facturas.Select(f => new FacturaGridRow(f)).ToList();
            dgvFacturas.DataSource = new BindingSource { DataSource = filas };
        }

        private void DgvFacturas_RowPrePaint(object? sender,
            DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // ← Obtenemos la factura directamente desde el DTO de la fila
            if (dgvFacturas.Rows[e.RowIndex].DataBoundItem
                is not FacturaGridRow fila) return;

            dgvFacturas.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                fila.FacturaOriginal.Estado switch
                {
                    EstadoFactura.OK             => Color.FromArgb(226, 239, 218),
                    EstadoFactura.RevisionManual => Color.FromArgb(255, 242, 204),
                    EstadoFactura.Error          => Color.FromArgb(255, 228, 214),
                    _                            => Color.White
                };
        }

        private void DgvFacturas_CellDoubleClick(object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // ← Obtenemos la factura directamente desde el DTO de la fila
            // Esto funciona correctamente tanto con filtro como sin él
            if (dgvFacturas.Rows[e.RowIndex].DataBoundItem
                is not FacturaGridRow fila) return;

            using var detalle = new DetalleFacturaForm(fila.FacturaOriginal);
            detalle.ShowDialog(this);
        }

        // ── Estadísticas ──────────────────────────────────────────────────────

        private void MostrarResumenEstadisticas(List<Factura> facturas)
        {
            int total = facturas.Count;
            int ok = facturas.Count(f => f.Estado == EstadoFactura.OK);
            int revision = facturas.Count(f => f.Estado ==
                                     EstadoFactura.RevisionManual);
            int errores = facturas.Count(f => f.Estado == EstadoFactura.Error);
            int porOcr = facturas.Count(f => f.ExtractedByOcr);
            decimal totalEuros = facturas
                .Where(f => f.Estado != EstadoFactura.Error)
                .Sum(f => f.Total);

            lblResumen.Text =
                $"Total: {total}  |  ✔ OK: {ok}  |  " +
                $"⚠ Revisión: {revision}  |  ✖ Errores: {errores}  |  " +
                $"OCR: {porOcr}  |  Importe total: {totalEuros:N2} €";
        }

        // ── Filtros ───────────────────────────────────────────────────────────

        private void txtFiltro_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_facturas.Count == 0) return;

            string textoBusqueda = txtFiltro.Text;
            string estadoFiltro = cmbFiltroEstado.SelectedItem?.ToString() ?? "Todos";

            var filtradas = _facturas.Where(f =>
            {
                bool coincideTexto =
                    string.IsNullOrEmpty(textoBusqueda) ||
                    f.NumeroFactura.Contains(textoBusqueda,
                        StringComparison.OrdinalIgnoreCase) ||
                    f.Emisor.Nombre.Contains(textoBusqueda,
                        StringComparison.OrdinalIgnoreCase) ||
                    f.Emisor.NIF.Contains(textoBusqueda,
                        StringComparison.OrdinalIgnoreCase);

                bool coincideEstado =
                    estadoFiltro == "Todos" ||
                    f.Estado.ToString() == estadoFiltro;

                return coincideTexto && coincideEstado;
            });

            dgvFacturas.DataSource = new BindingSource
            {
                DataSource = filtradas.Select(f => new FacturaGridRow(f)).ToList()
            };
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
                _exportador.ExportarAExcelIngresos(_facturas, dialogo.FileName);
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
                InitialDirectory = Environment.GetFolderPath(
                                       Environment.SpecialFolder.MyDocuments)
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Exportar todas las facturas sin alterarlas
                _exportador.ExportarAExcelGastos(_facturas, dialogo.FileName);
                MostrarExitoExportacion(dialogo.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Definir plantilla OCR para un emisor específico
        private void btnDefinirPlantilla_Click(object sender, EventArgs e)
        {
            using var ventana = new DefinirPlantillaForm();
            ventana.ShowDialog(this);
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