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
            btnCargarPdf = new Button();
            btnEliminarZona = new Button();
            btnGuardar = new Button();
            btnCerrar = new Button();
            lblEmisor = new Label();
            cmbEmisor = new ComboBox();
            lblZonas = new Label();
            txtTexto = new TextBox();
            ((System.ComponentModel.ISupportInitialize)picFactura).BeginInit();
            SuspendLayout();
            // 
            // picFactura
            // 
            picFactura.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picFactura.BackColor = Color.LightGray;
            picFactura.BorderStyle = BorderStyle.FixedSingle;
            picFactura.Cursor = Cursors.Cross;
            picFactura.Location = new Point(448, 12);
            picFactura.Name = "picFactura";
            picFactura.Size = new Size(420, 594);
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
            lstZonas.Location = new Point(12, 147);
            lstZonas.Name = "lstZonas";
            lstZonas.Size = new Size(430, 164);
            lstZonas.TabIndex = 8;
            // 
            // btnCargarPdf
            // 
            btnCargarPdf.BackColor = Color.FromArgb(46, 117, 182);
            btnCargarPdf.FlatStyle = FlatStyle.Flat;
            btnCargarPdf.ForeColor = Color.White;
            btnCargarPdf.Location = new Point(12, 69);
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
            btnEliminarZona.Location = new Point(12, 317);
            btnEliminarZona.Name = "btnEliminarZona";
            btnEliminarZona.Size = new Size(430, 30);
            btnEliminarZona.TabIndex = 9;
            btnEliminarZona.Text = "🗑 Eliminar zona seleccionada";
            btnEliminarZona.Click += BtnEliminarZona_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnGuardar.BackColor = Color.FromArgb(33, 115, 70);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(12, 572);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(155, 34);
            btnGuardar.TabIndex = 10;
            btnGuardar.Text = "💾 Guardar plantilla";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Location = new Point(362, 572);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(80, 34);
            btnCerrar.TabIndex = 11;
            btnCerrar.Text = "Cerrar";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // lblEmisor
            // 
            lblEmisor.AutoSize = true;
            lblEmisor.Location = new Point(12, 12);
            lblEmisor.Name = "lblEmisor";
            lblEmisor.Size = new Size(57, 20);
            lblEmisor.TabIndex = 1;
            lblEmisor.Text = "Emisor:";
            // 
            // cmbEmisor
            // 
            cmbEmisor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmisor.FormattingEnabled = true;
            cmbEmisor.Location = new Point(12, 35);
            cmbEmisor.Name = "cmbEmisor";
            cmbEmisor.Size = new Size(430, 28);
            cmbEmisor.TabIndex = 2;
            // 
            // lblZonas
            // 
            lblZonas.AutoSize = true;
            lblZonas.Location = new Point(12, 124);
            lblZonas.Name = "lblZonas";
            lblZonas.Size = new Size(117, 20);
            lblZonas.TabIndex = 7;
            lblZonas.Text = "Zonas definidas:";
            // 
            // txtTexto
            // 
            txtTexto.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtTexto.BackColor = SystemColors.Control;
            txtTexto.BorderStyle = BorderStyle.FixedSingle;
            txtTexto.Location = new Point(12, 353);
            txtTexto.Multiline = true;
            txtTexto.Name = "txtTexto";
            txtTexto.ReadOnly = true;
            txtTexto.ScrollBars = ScrollBars.Vertical;
            txtTexto.Size = new Size(430, 213);
            txtTexto.TabIndex = 13;
            txtTexto.WordWrap = false;
            // 
            // DefinirPlantillaForm
            // 
            ClientSize = new Size(880, 618);
            Controls.Add(txtTexto);
            Controls.Add(lblEmisor);
            Controls.Add(cmbEmisor);
            Controls.Add(btnCargarPdf);
            Controls.Add(lblZonas);
            Controls.Add(lstZonas);
            Controls.Add(btnEliminarZona);
            Controls.Add(btnGuardar);
            Controls.Add(btnCerrar);
            Controls.Add(picFactura);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(898, 665);
            Name = "DefinirPlantillaForm";
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Definir plantilla OCR por zonas";
            ((System.ComponentModel.ISupportInitialize)picFactura).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // ── Declaración de controles ──────────────────────────────────────────
        private PictureBox picFactura;
        private ListBox lstZonas;
        private Button btnCargarPdf;
        private Button btnEliminarZona;
        private Button btnGuardar;
        private Button btnCerrar;
        private Label lblEmisor;
        private ComboBox cmbEmisor;
        private Label lblZonas;
        private TextBox txtTexto;
    }
}
