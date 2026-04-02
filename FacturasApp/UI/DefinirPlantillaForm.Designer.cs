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
            // ── Inicialización de controles ───────────────────────────────────
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

            ((System.ComponentModel.ISupportInitialize)picFactura).BeginInit();
            SuspendLayout();

            // ── PictureBox ────────────────────────────────────────────────────
            picFactura.Location = new Point(12, 12);
            picFactura.Size = new Size(720, 688);
            picFactura.BorderStyle = BorderStyle.FixedSingle;
            picFactura.SizeMode = PictureBoxSizeMode.Zoom;
            picFactura.BackColor = Color.LightGray;
            picFactura.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                     AnchorStyles.Bottom | AnchorStyles.Right;
            picFactura.Cursor = Cursors.Cross;
            picFactura.MouseDown += PicFactura_MouseDown;
            picFactura.MouseMove += PicFactura_MouseMove;
            picFactura.MouseUp += PicFactura_MouseUp;
            picFactura.Paint += PicFactura_Paint;

            // ── Emisor ────────────────────────────────────────────────────────
            lblEmisor.Text = "Emisor:";
            lblEmisor.Location = new Point(745, 12);
            lblEmisor.AutoSize = true;

            txtEmisor.Location = new Point(745, 32);
            txtEmisor.Size = new Size(320, 23);
            txtEmisor.PlaceholderText = "Nombre del emisor...";

            // ── Cargar PDF ────────────────────────────────────────────────────
            btnCargarPdf.Text = "📄 Cargar PDF de muestra";
            btnCargarPdf.Location = new Point(745, 65);
            btnCargarPdf.Size = new Size(210, 32);
            btnCargarPdf.FlatStyle = FlatStyle.Flat;
            btnCargarPdf.BackColor = Color.FromArgb(46, 117, 182);
            btnCargarPdf.ForeColor = Color.White;
            btnCargarPdf.Click += BtnCargarPdf_Click;

            // ── Instrucciones ─────────────────────────────────────────────────
            lblInstrucciones.Text =
                "1. Carga un PDF de muestra\n" +
                "2. Selecciona el campo en el desplegable\n" +
                "3. Dibuja un rectángulo sobre la zona\n" +
                "4. Repite para cada campo\n" +
                "5. Guarda la plantilla";
            lblInstrucciones.Location = new Point(745, 110);
            lblInstrucciones.Size = new Size(320, 100);
            lblInstrucciones.ForeColor = Color.DimGray;

            // ── Campo a marcar ────────────────────────────────────────────────
            var lblCampo = new Label
            {
                Text = "Campo a marcar:",
                Location = new Point(745, 225),
                AutoSize = true
            };

            cmbCampo.Location = new Point(745, 245);
            cmbCampo.Size = new Size(320, 23);
            cmbCampo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampo.Items.AddRange(CamposDisponibles);
            cmbCampo.SelectedIndex = 0;

            // ── Zonas definidas ───────────────────────────────────────────────
            var lblZonas = new Label
            {
                Text = "Zonas definidas:",
                Location = new Point(745, 285),
                AutoSize = true
            };

            lstZonas.Location = new Point(745, 305);
            lstZonas.Size = new Size(320, 200);

            // ── Eliminar zona ─────────────────────────────────────────────────
            btnEliminarZona.Text = "🗑 Eliminar zona seleccionada";
            btnEliminarZona.Location = new Point(745, 515);
            btnEliminarZona.Size = new Size(210, 30);
            btnEliminarZona.FlatStyle = FlatStyle.Flat;
            btnEliminarZona.Click += BtnEliminarZona_Click;

            // ── Guardar / Cerrar ──────────────────────────────────────────────
            btnGuardar.Text = "💾 Guardar plantilla";
            btnGuardar.Location = new Point(745, 620);
            btnGuardar.Size = new Size(155, 34);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.BackColor = Color.FromArgb(33, 115, 70);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Click += BtnGuardar_Click;

            btnCerrar.Text = "Cerrar";
            btnCerrar.Location = new Point(910, 620);
            btnCerrar.Size = new Size(80, 34);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Click += (s, e) => Close();

            // ── Formulario ────────────────────────────────────────────────────
            Controls.AddRange(new Control[]
            {
                picFactura,
                lblEmisor, txtEmisor, btnCargarPdf,
                lblInstrucciones, lblCampo, cmbCampo,
                lblZonas, lstZonas, btnEliminarZona,
                btnGuardar, btnCerrar
            });

            Text = "Definir plantilla OCR por zonas";
            ClientSize = new Size(1080, 720);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);

            ((System.ComponentModel.ISupportInitialize)picFactura).EndInit();
            ResumeLayout(false);
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
    }
}
