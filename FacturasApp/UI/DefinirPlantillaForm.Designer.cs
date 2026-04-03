using System.Drawing;
using System.Windows.Forms;

namespace FacturasApp.UI
{
    partial class DefinirPlantillaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent() // ← nombre correcto
        {
            picFactura = new PictureBox();
            lstZonas = new ListBox();
            cmbCampo = new ComboBox();
            btnCargarPdf = new Button();
            btnEliminarZona = new Button();
            btnGuardar = new Button();
            btnCerrar = new Button();
            lblInstrucciones = new Label();
            lblEmisor = new Label();
            txtEmisor = new TextBox();
            lblZonas = new Label();
            ((System.ComponentModel.ISupportInitialize)picFactura).BeginInit();
            SuspendLayout();
            // 
            // picFactura
            // 
            picFactura.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picFactura.BackColor = Color.LightGray;
            picFactura.BorderStyle = BorderStyle.FixedSingle;
            picFactura.Cursor = Cursors.Cross;
            picFactura.Location = new Point(12, 12);
            picFactura.Name = "picFactura";
            picFactura.Size = new Size(720, 696);
            picFactura.SizeMode = PictureBoxSizeMode.Zoom;
            picFactura.TabIndex = 0;
            picFactura.TabStop = false;
            picFactura.Paint += PicFactura_Paint;
            picFactura.MouseDown += PicFactura_MouseDown;
            picFactura.MouseMove += PicFactura_MouseMove;
            picFactura.MouseUp += PicFactura_MouseUp;
            // 
            // lstZonas
            // 
            lstZonas.Location = new Point(745, 305);
            lstZonas.Name = "lstZonas";
            lstZonas.Size = new Size(320, 184);
            lstZonas.TabIndex = 8;
            // 
            // cmbCampo
            // 
            cmbCampo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampo.Location = new Point(745, 245);
            cmbCampo.Name = "cmbCampo";
            cmbCampo.Size = new Size(320, 28);
            cmbCampo.TabIndex = 6;
            // Poblamos el desplegable con los campos disponibles
            cmbCampo.Items.AddRange(CamposDisponibles);
            cmbCampo.SelectedIndex = 0;
            // 
            // btnCargarPdf
            // 
            btnCargarPdf.BackColor = Color.FromArgb(46, 117, 182);
            btnCargarPdf.FlatStyle = FlatStyle.Flat;
            btnCargarPdf.ForeColor = Color.White;
            btnCargarPdf.Location = new Point(745, 65);
            btnCargarPdf.Name = "btnCargarPdf";
            btnCargarPdf.Size = new Size(210, 32);
            btnCargarPdf.TabIndex = 3;
            btnCargarPdf.Text = "📄 Cargar PDF de muestra";
            btnCargarPdf.UseVisualStyleBackColor = false;
            btnCargarPdf.Click += BtnCargarPdf_Click;
            // 
            // btnEliminarZona
            // 
            btnEliminarZona.FlatStyle = FlatStyle.Flat;
            btnEliminarZona.Location = new Point(745, 515);
            btnEliminarZona.Name = "btnEliminarZona";
            btnEliminarZona.Size = new Size(210, 30);
            btnEliminarZona.TabIndex = 9;
            btnEliminarZona.Text = "🗑 Eliminar zona seleccionada";
            btnEliminarZona.Click += BtnEliminarZona_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(33, 115, 70);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(745, 620);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(155, 34);
            btnGuardar.TabIndex = 10;
            btnGuardar.Text = "💾 Guardar plantilla";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // btnCerrar
            //
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Text = "Cerrar";
            btnCerrar.Location = new Point(910, 620);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(80, 34);
            btnCerrar.TabIndex = 11;
            btnCerrar.Click += (s, e) => Close();
            // 
            // lblInstrucciones
            // 
            lblInstrucciones.ForeColor = Color.DimGray;
            lblInstrucciones.Location = new Point(745, 110);
            lblInstrucciones.Name = "lblInstrucciones";
            lblInstrucciones.Size = new Size(320, 100);
            lblInstrucciones.TabIndex = 4;
            lblInstrucciones.Text = "1. Carga un PDF de muestra\n2. Selecciona el campo en el desplegable\n3. Dibuja un rectángulo sobre la zona\n4. Repite para cada campo\n5. Guarda la plantilla";
            // 
            // lblEmisor
            // 
            lblEmisor.AutoSize = true;
            lblEmisor.Location = new Point(745, 12);
            lblEmisor.Name = "lblEmisor";
            lblEmisor.Size = new Size(57, 20);
            lblEmisor.TabIndex = 1;
            lblEmisor.Text = "Emisor:";
            // 
            // txtEmisor
            // 
            txtEmisor.Location = new Point(745, 32);
            txtEmisor.Name = "txtEmisor";
            txtEmisor.PlaceholderText = "Nombre del emisor...";
            txtEmisor.Size = new Size(320, 27);
            txtEmisor.TabIndex = 2;
            // 
            // lblZonas
            // 
            lblZonas.AutoSize = true;
            lblZonas.Location = new Point(745, 285);
            lblZonas.Name = "lblZonas";
            lblZonas.Size = new Size(100, 23);
            lblZonas.TabIndex = 7;
            lblZonas.Text = "Zonas definidas:";
            // 
            // DefinirPlantillaForm
            // 
            ClientSize = new Size(1080, 720);
            Controls.Add(picFactura);
            Controls.Add(lblEmisor);
            Controls.Add(txtEmisor);
            Controls.Add(btnCargarPdf);
            Controls.Add(lblInstrucciones);
            Controls.Add(cmbCampo);
            Controls.Add(lblZonas);
            Controls.Add(lstZonas);
            Controls.Add(btnEliminarZona);
            Controls.Add(btnGuardar);
            Controls.Add(btnCerrar);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(900, 600);
            Name = "DefinirPlantillaForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Definir plantilla OCR por zonas";
            ((System.ComponentModel.ISupportInitialize)picFactura).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // ── Declaración de controles ──────────────────────────────────────────
        private PictureBox picFactura;
        private ListBox lstZonas;
        private ComboBox cmbCampo;
        private Button btnCargarPdf;
        private Button btnEliminarZona;
        private Button btnGuardar;
        private Button btnCerrar;
        private Label lblInstrucciones;
        private Label lblEmisor;
        private TextBox txtEmisor;
        private Label lblZonas;
    }
}
