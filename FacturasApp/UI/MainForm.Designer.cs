using System.Drawing;
using System.Windows.Forms;

namespace FacturasApp.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnSeleccionarArchivos = new Button();
            btnSeleccionarCarpeta = new Button();
            btnSeleccionarExcel = new Button();
            btnLimpiarLista = new Button();
            lblContadorArchivos = new Label();
            lstArchivos = new ListBox();
            txtRutaExcel = new TextBox();
            btnProcesar = new Button();
            progressBar = new ProgressBar();
            lblPorcentaje = new Label();
            lblEstado = new Label();
            txtFiltro = new TextBox();
            cmbFiltroEstado = new ComboBox();
            dgvFacturas = new DataGridView();
            lblResumen = new Label();
            btnExportarExcel = new Button();
            btnExportarExcelIngresos = new Button();
            btnExportarExcelGastos = new Button();
            lblFiltro = new Label();
            lblEstadoFiltro = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvFacturas).BeginInit();
            SuspendLayout();
            // 
            // btnSeleccionarArchivos
            // 
            btnSeleccionarArchivos.BackColor = Color.FromArgb(46, 117, 182);
            btnSeleccionarArchivos.FlatStyle = FlatStyle.Flat;
            btnSeleccionarArchivos.ForeColor = Color.White;
            btnSeleccionarArchivos.Location = new Point(12, 12);
            btnSeleccionarArchivos.Name = "btnSeleccionarArchivos";
            btnSeleccionarArchivos.Size = new Size(140, 32);
            btnSeleccionarArchivos.TabIndex = 0;
            btnSeleccionarArchivos.Text = "📄 Archivos PDF";
            btnSeleccionarArchivos.UseVisualStyleBackColor = false;
            btnSeleccionarArchivos.Click += btnSeleccionarArchivos_Click;
            // 
            // btnSeleccionarCarpeta
            // 
            btnSeleccionarCarpeta.BackColor = Color.FromArgb(46, 117, 182);
            btnSeleccionarCarpeta.FlatStyle = FlatStyle.Flat;
            btnSeleccionarCarpeta.ForeColor = Color.White;
            btnSeleccionarCarpeta.Location = new Point(160, 12);
            btnSeleccionarCarpeta.Name = "btnSeleccionarCarpeta";
            btnSeleccionarCarpeta.Size = new Size(140, 32);
            btnSeleccionarCarpeta.TabIndex = 1;
            btnSeleccionarCarpeta.Text = "📁 Carpeta PDF";
            btnSeleccionarCarpeta.UseVisualStyleBackColor = false;
            btnSeleccionarCarpeta.Click += btnSeleccionarCarpeta_Click;
            // 
            // btnSeleccionarExcel
            // 
            btnSeleccionarExcel.BackColor = Color.FromArgb(33, 115, 70);
            btnSeleccionarExcel.FlatStyle = FlatStyle.Flat;
            btnSeleccionarExcel.ForeColor = Color.White;
            btnSeleccionarExcel.Location = new Point(308, 12);
            btnSeleccionarExcel.Name = "btnSeleccionarExcel";
            btnSeleccionarExcel.Size = new Size(140, 32);
            btnSeleccionarExcel.TabIndex = 2;
            btnSeleccionarExcel.Text = "📊 Excel facturas";
            btnSeleccionarExcel.UseVisualStyleBackColor = false;
            btnSeleccionarExcel.Click += btnSeleccionarExcel_Click;
            // 
            // btnLimpiarLista
            // 
            btnLimpiarLista.FlatStyle = FlatStyle.Flat;
            btnLimpiarLista.Location = new Point(456, 12);
            btnLimpiarLista.Name = "btnLimpiarLista";
            btnLimpiarLista.Size = new Size(90, 32);
            btnLimpiarLista.TabIndex = 3;
            btnLimpiarLista.Text = "✖ Limpiar";
            btnLimpiarLista.Click += btnLimpiarLista_Click;
            // 
            // lblContadorArchivos
            // 
            lblContadorArchivos.AutoSize = true;
            lblContadorArchivos.ForeColor = Color.Gray;
            lblContadorArchivos.Location = new Point(12, 52);
            lblContadorArchivos.Name = "lblContadorArchivos";
            lblContadorArchivos.Size = new Size(184, 20);
            lblContadorArchivos.TabIndex = 4;
            lblContadorArchivos.Text = "Sin archivos seleccionados";
            // 
            // lstArchivos
            // 
            lstArchivos.HorizontalScrollbar = true;
            lstArchivos.Location = new Point(12, 72);
            lstArchivos.Name = "lstArchivos";
            lstArchivos.Size = new Size(960, 84);
            lstArchivos.TabIndex = 5;
            // 
            // txtRutaExcel
            // 
            txtRutaExcel.BackColor = Color.FromArgb(240, 240, 240);
            txtRutaExcel.Location = new Point(12, 180);
            txtRutaExcel.Name = "txtRutaExcel";
            txtRutaExcel.PlaceholderText = "Ruta del Excel de facturas (opcional)...";
            txtRutaExcel.ReadOnly = true;
            txtRutaExcel.Size = new Size(960, 27);
            txtRutaExcel.TabIndex = 6;
            // 
            // btnProcesar
            // 
            btnProcesar.BackColor = Color.FromArgb(46, 117, 182);
            btnProcesar.Enabled = false;
            btnProcesar.FlatStyle = FlatStyle.Flat;
            btnProcesar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnProcesar.ForeColor = Color.White;
            btnProcesar.Location = new Point(12, 215);
            btnProcesar.Name = "btnProcesar";
            btnProcesar.Size = new Size(130, 36);
            btnProcesar.TabIndex = 7;
            btnProcesar.Text = "▶  Procesar";
            btnProcesar.UseVisualStyleBackColor = false;
            btnProcesar.Click += btnProcesar_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(152, 221);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(820, 24);
            progressBar.TabIndex = 8;
            progressBar.Visible = false;
            // 
            // lblPorcentaje
            // 
            lblPorcentaje.AutoSize = true;
            lblPorcentaje.Location = new Point(660, 224);
            lblPorcentaje.Name = "lblPorcentaje";
            lblPorcentaje.Size = new Size(0, 20);
            lblPorcentaje.TabIndex = 9;
            lblPorcentaje.Visible = false;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.ForeColor = Color.DimGray;
            lblEstado.Location = new Point(12, 260);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(40, 20);
            lblEstado.TabIndex = 10;
            lblEstado.Text = "Listo";
            // 
            // txtFiltro
            // 
            txtFiltro.Location = new Point(68, 285);
            txtFiltro.Name = "txtFiltro";
            txtFiltro.PlaceholderText = "Nombre, NIF, nº factura...";
            txtFiltro.Size = new Size(220, 27);
            txtFiltro.TabIndex = 12;
            txtFiltro.TextChanged += txtFiltro_TextChanged;
            // 
            // cmbFiltroEstado
            // 
            cmbFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroEstado.Location = new Point(352, 285);
            cmbFiltroEstado.Name = "cmbFiltroEstado";
            cmbFiltroEstado.Size = new Size(150, 28);
            cmbFiltroEstado.TabIndex = 14;
            cmbFiltroEstado.SelectedIndexChanged += cmbFiltroEstado_SelectedIndexChanged;
            // 
            // dgvFacturas
            // 
            dgvFacturas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvFacturas.ColumnHeadersHeight = 29;
            dgvFacturas.Location = new Point(12, 318);
            dgvFacturas.Name = "dgvFacturas";
            dgvFacturas.RowHeadersWidth = 51;
            dgvFacturas.Size = new Size(960, 330);
            dgvFacturas.TabIndex = 15;
            // 
            // lblResumen
            // 
            lblResumen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblResumen.AutoSize = true;
            lblResumen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblResumen.Location = new Point(12, 658);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(0, 20);
            lblResumen.TabIndex = 16;
            // 
            // btnExportarExcel
            // 
            btnExportarExcel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportarExcel.BackColor = Color.FromArgb(33, 115, 70);
            btnExportarExcel.Enabled = false;
            btnExportarExcel.FlatStyle = FlatStyle.Flat;
            btnExportarExcel.ForeColor = Color.White;
            btnExportarExcel.Location = new Point(12, 688);
            btnExportarExcel.Name = "btnExportarExcel";
            btnExportarExcel.Size = new Size(150, 34);
            btnExportarExcel.TabIndex = 17;
            btnExportarExcel.Text = "📊 Exportar Excel";
            btnExportarExcel.UseVisualStyleBackColor = false;
            btnExportarExcel.Click += btnExportarExcel_Click;
            // 
            // btnExportarExcelIngresos
            // 
            btnExportarExcelIngresos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportarExcelIngresos.BackColor = Color.FromArgb(33, 115, 70);
            btnExportarExcelIngresos.Enabled = false;
            btnExportarExcelIngresos.FlatStyle = FlatStyle.Flat;
            btnExportarExcelIngresos.ForeColor = Color.White;
            btnExportarExcelIngresos.Location = new Point(170, 688);
            btnExportarExcelIngresos.Name = "btnExportarExcelIngresos";
            btnExportarExcelIngresos.Size = new Size(150, 34);
            btnExportarExcelIngresos.TabIndex = 18;
            btnExportarExcelIngresos.Text = "📊 Exportar Ingresos";
            btnExportarExcelIngresos.UseVisualStyleBackColor = false;
            btnExportarExcelIngresos.Click += btnExportarExcelIngresos_Click;
            // 
            // btnExportarExcelGastos
            // 
            btnExportarExcelGastos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportarExcelGastos.BackColor = Color.FromArgb(100, 100, 100);
            btnExportarExcelGastos.Enabled = false;
            btnExportarExcelGastos.FlatStyle = FlatStyle.Flat;
            btnExportarExcelGastos.ForeColor = Color.White;
            btnExportarExcelGastos.Location = new Point(328, 688);
            btnExportarExcelGastos.Name = "btnExportarExcelGastos";
            btnExportarExcelGastos.Size = new Size(150, 34);
            btnExportarExcelGastos.TabIndex = 19;
            btnExportarExcelGastos.Text = "📊 Exportar Gastos";
            btnExportarExcelGastos.UseVisualStyleBackColor = false;
            btnExportarExcelGastos.Click += btnExportarExcelGastos_Click;
            // 
            // lblFiltro
            // 
            lblFiltro.Location = new Point(0, 0);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Size = new Size(100, 23);
            lblFiltro.TabIndex = 11;
            // 
            // lblEstadoFiltro
            // 
            lblEstadoFiltro.Location = new Point(0, 0);
            lblEstadoFiltro.Name = "lblEstadoFiltro";
            lblEstadoFiltro.Size = new Size(100, 23);
            lblEstadoFiltro.TabIndex = 13;
            // 
            // MainForm
            // 
            ClientSize = new Size(984, 736);
            Controls.Add(btnSeleccionarArchivos);
            Controls.Add(btnSeleccionarCarpeta);
            Controls.Add(btnSeleccionarExcel);
            Controls.Add(btnLimpiarLista);
            Controls.Add(lblContadorArchivos);
            Controls.Add(lstArchivos);
            Controls.Add(txtRutaExcel);
            Controls.Add(btnProcesar);
            Controls.Add(progressBar);
            Controls.Add(lblPorcentaje);
            Controls.Add(lblEstado);
            Controls.Add(lblFiltro);
            Controls.Add(txtFiltro);
            Controls.Add(lblEstadoFiltro);
            Controls.Add(cmbFiltroEstado);
            Controls.Add(dgvFacturas);
            Controls.Add(lblResumen);
            Controls.Add(btnExportarExcel);
            Controls.Add(btnExportarExcelIngresos);
            Controls.Add(btnExportarExcelGastos);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gestión de Facturas PDF";
            ((System.ComponentModel.ISupportInitialize)dgvFacturas).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // ── Declaración de campos ─────────────────────────────────────────────
        private Button btnSeleccionarArchivos;
        private Button btnSeleccionarCarpeta;
        private Button btnSeleccionarExcel;
        private Button btnLimpiarLista;
        private Button btnProcesar;
        private Button btnExportarExcel;
        private Button btnExportarExcelIngresos;
        private Button btnExportarExcelGastos;
        private ListBox lstArchivos;
        private TextBox txtRutaExcel;
        private ProgressBar progressBar;
        private Label lblContadorArchivos;
        private Label lblEstado;
        private Label lblPorcentaje;
        private Label lblResumen;
        private TextBox txtFiltro;
        private ComboBox cmbFiltroEstado;
        private DataGridView dgvFacturas;
        private Label lblFiltro;
        private Label lblEstadoFiltro;
    }
}
