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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
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
            pnlEstados = new FlowLayoutPanel();
            btnExportarExcelIngresos = new Button();
            btnExportarExcelGastos = new Button();
            lblFiltro = new Label();
            lblEstadoFiltro = new Label();
            btnGestionEmisores = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvFacturas).BeginInit();
            SuspendLayout();
            // 
            // btnSeleccionarArchivos
            // 
            btnSeleccionarArchivos.BackColor = Color.DarkCyan;
            btnSeleccionarArchivos.FlatStyle = FlatStyle.Flat;
            btnSeleccionarArchivos.ForeColor = Color.White;
            btnSeleccionarArchivos.Location = new Point(158, 12);
            btnSeleccionarArchivos.Name = "btnSeleccionarArchivos";
            btnSeleccionarArchivos.Size = new Size(140, 34);
            btnSeleccionarArchivos.TabIndex = 0;
            btnSeleccionarArchivos.Text = "📄 Facturas (PDF/imagen)";
            btnSeleccionarArchivos.UseVisualStyleBackColor = false;
            btnSeleccionarArchivos.Click += btnSeleccionarArchivos_Click;
            // 
            // btnSeleccionarCarpeta
            // 
            btnSeleccionarCarpeta.BackColor = Color.DarkBlue;
            btnSeleccionarCarpeta.FlatStyle = FlatStyle.Flat;
            btnSeleccionarCarpeta.ForeColor = Color.White;
            btnSeleccionarCarpeta.Location = new Point(12, 12);
            btnSeleccionarCarpeta.Name = "btnSeleccionarCarpeta";
            btnSeleccionarCarpeta.Size = new Size(140, 34);
            btnSeleccionarCarpeta.TabIndex = 1;
            btnSeleccionarCarpeta.Text = "📁 Carpeta facturas";
            btnSeleccionarCarpeta.UseVisualStyleBackColor = false;
            btnSeleccionarCarpeta.Click += btnSeleccionarCarpeta_Click;
            // 
            // btnSeleccionarExcel
            // 
            btnSeleccionarExcel.BackColor = Color.DarkViolet;
            btnSeleccionarExcel.FlatStyle = FlatStyle.Flat;
            btnSeleccionarExcel.ForeColor = Color.White;
            btnSeleccionarExcel.Location = new Point(304, 12);
            btnSeleccionarExcel.Name = "btnSeleccionarExcel";
            btnSeleccionarExcel.Size = new Size(140, 34);
            btnSeleccionarExcel.TabIndex = 2;
            btnSeleccionarExcel.Text = "📊 Excel facturas";
            btnSeleccionarExcel.UseVisualStyleBackColor = false;
            btnSeleccionarExcel.Click += btnSeleccionarExcel_Click;
            // 
            // btnLimpiarLista
            // 
            btnLimpiarLista.FlatStyle = FlatStyle.Flat;
            btnLimpiarLista.Location = new Point(450, 12);
            btnLimpiarLista.Name = "btnLimpiarLista";
            btnLimpiarLista.Size = new Size(140, 34);
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
            btnProcesar.Location = new Point(12, 216);
            btnProcesar.Name = "btnProcesar";
            btnProcesar.Size = new Size(140, 34);
            btnProcesar.TabIndex = 7;
            btnProcesar.Text = "▶  Procesar";
            btnProcesar.UseVisualStyleBackColor = false;
            btnProcesar.Click += btnProcesar_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(188, 221);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(784, 24);
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
            cmbFiltroEstado.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmbFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroEstado.Location = new Point(812, 529);
            cmbFiltroEstado.Name = "cmbFiltroEstado";
            cmbFiltroEstado.Size = new Size(158, 28);
            cmbFiltroEstado.TabIndex = 17;
            cmbFiltroEstado.SelectedIndexChanged += cmbFiltroEstado_SelectedIndexChanged;
            // 
            // dgvFacturas
            // 
            dgvFacturas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvFacturas.ColumnHeadersHeight = 29;
            dgvFacturas.Location = new Point(12, 318);
            dgvFacturas.Name = "dgvFacturas";
            dgvFacturas.RowHeadersWidth = 51;
            dgvFacturas.Size = new Size(958, 204);
            dgvFacturas.TabIndex = 15;
            // 
            // pnlEstados
            // 
            pnlEstados.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlEstados.AutoScroll = true;
            pnlEstados.FlowDirection = FlowDirection.LeftToRight;
            pnlEstados.Location = new Point(12, 528);
            pnlEstados.Name = "pnlEstados";
            pnlEstados.Padding = new Padding(0, 4, 0, 0);
            pnlEstados.Size = new Size(788, 64);
            pnlEstados.TabIndex = 16;
            pnlEstados.WrapContents = true;
            // 
            // btnExportarExcelIngresos
            // 
            btnExportarExcelIngresos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportarExcelIngresos.BackColor = Color.DarkGreen;
            btnExportarExcelIngresos.Enabled = false;
            btnExportarExcelIngresos.FlatStyle = FlatStyle.Flat;
            btnExportarExcelIngresos.ForeColor = Color.White;
            btnExportarExcelIngresos.Location = new Point(12, 598);
            btnExportarExcelIngresos.Name = "btnExportarExcelIngresos";
            btnExportarExcelIngresos.Size = new Size(161, 34);
            btnExportarExcelIngresos.TabIndex = 18;
            btnExportarExcelIngresos.Text = "📊 Exportar Ingresos";
            btnExportarExcelIngresos.UseVisualStyleBackColor = false;
            btnExportarExcelIngresos.Click += btnExportarExcelIngresos_Click;
            // 
            // btnExportarExcelGastos
            // 
            btnExportarExcelGastos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportarExcelGastos.BackColor = Color.DarkRed;
            btnExportarExcelGastos.Enabled = false;
            btnExportarExcelGastos.FlatStyle = FlatStyle.Flat;
            btnExportarExcelGastos.ForeColor = Color.White;
            btnExportarExcelGastos.Location = new Point(179, 598);
            btnExportarExcelGastos.Name = "btnExportarExcelGastos";
            btnExportarExcelGastos.Size = new Size(161, 34);
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
            // btnGestionEmisores
            // 
            btnGestionEmisores.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGestionEmisores.BackColor = Color.DarkOrange;
            btnGestionEmisores.FlatStyle = FlatStyle.Flat;
            btnGestionEmisores.ForeColor = Color.White;
            btnGestionEmisores.Location = new Point(832, 12);
            btnGestionEmisores.Name = "btnGestionEmisores";
            btnGestionEmisores.Size = new Size(140, 34);
            btnGestionEmisores.TabIndex = 21;
            btnGestionEmisores.Text = "⚙ Emisores";
            btnGestionEmisores.UseVisualStyleBackColor = false;
            btnGestionEmisores.Click += btnGestionEmisores_Click;
            // 
            // MainForm
            // 
            ClientSize = new Size(982, 640);
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
            Controls.Add(pnlEstados);
            Controls.Add(btnExportarExcelIngresos);
            Controls.Add(btnExportarExcelGastos);
            Controls.Add(btnGestionEmisores);
            Font = new Font("Segoe UI", 9F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 640);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gestión de Facturas";
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
        private Button btnExportarExcelIngresos;
        private Button btnExportarExcelGastos;
        private Button btnGestionEmisores;
        private ListBox lstArchivos;
        private TextBox txtRutaExcel;
        private ProgressBar progressBar;
        private Label lblContadorArchivos;
        private Label lblEstado;
        private Label lblPorcentaje;
        private FlowLayoutPanel pnlEstados;
        private TextBox txtFiltro;
        private ComboBox cmbFiltroEstado;
        private DataGridView dgvFacturas;
        private Label lblFiltro;
        private Label lblEstadoFiltro;
    }
}
