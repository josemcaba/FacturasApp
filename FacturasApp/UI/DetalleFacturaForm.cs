using FacturasApp.Models;

namespace FacturasApp.UI
{
    public class DetalleFacturaForm : Form
    {
        private readonly Factura _factura;

        // ── Controles ─────────────────────────────────────────────────────────
        private TextBox txtNumeroFactura = new();
        private TextBox txtFecha = new();
        private TextBox txtEstado = new();
        private CheckBox chkOcr = new();
        private TextBox txtEmisorNombre = new();
        private TextBox txtEmisorNif = new();
        private TextBox txtClienteNombre = new();
        private TextBox txtClienteNif = new();
        private TextBox txtBase = new();
        private TextBox txtPorcentajeIva = new();
        private TextBox txtCuotaIva = new();
        private TextBox txtPorcentajeIrpf = new();
        private TextBox txtCuotaIrpf = new();
        private TextBox txtPorcentajeRe = new();
        private TextBox txtCuotaRe = new();
        private TextBox txtTotalCalculado = new();
        private TextBox txtDiferencia = new();
        private TextBox txtTotal = new();
        private TextBox txtRutaArchivo = new();
        private Panel panelError = new();
        private TextBox txtError = new();
        private Button btnAbrirPdf = new();
        private Button btnCerrar = new();

        public DetalleFacturaForm(Factura factura)
        {
            _factura = factura;
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            Text = "Detalle de factura";
            Size = new Size(520, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);

            // ── Función auxiliar para crear filas etiqueta + campo ────────────
            int y = 12;
            Control AgregarFila(string etiqueta, Control control,
                int altura = 23)
            {
                var lbl = new Label
                {
                    Text = etiqueta,
                    Location = new Point(12, y + 3),
                    Size = new Size(130, 18),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                control.Location = new Point(150, y);
                control.Size = new Size(340, altura);
                Controls.Add(lbl);
                Controls.Add(control);
                y += altura + 8;
                return control;
            }

            void AgregarSeparador(string titulo)
            {
                y += 4;
                var lbl = new Label
                {
                    Text = titulo,
                    Location = new Point(12, y),
                    Size = new Size(480, 18),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 117, 182)
                };
                Controls.Add(lbl);
                y += 22;
            }

            // ── Identificación ────────────────────────────────────────────────
            AgregarSeparador("Identificación");
            txtNumeroFactura.ReadOnly = true;
            txtFecha.ReadOnly = true;
            txtEstado.ReadOnly = true;
            AgregarFila("Nº Factura:", txtNumeroFactura);
            AgregarFila("Fecha:", txtFecha);
            AgregarFila("Estado:", txtEstado);

            chkOcr.Text = "Extraído mediante OCR";
            chkOcr.Enabled = false;
            chkOcr.Location = new Point(150, y);
            Controls.Add(chkOcr);
            y += 30;

            // ── Emisor ────────────────────────────────────────────────────────
            AgregarSeparador("Emisor");
            txtEmisorNombre.ReadOnly = true;
            txtEmisorNif.ReadOnly = true;
            AgregarFila("Nombre:", txtEmisorNombre);
            AgregarFila("NIF:", txtEmisorNif);

            // ── Cliente ───────────────────────────────────────────────────────
            AgregarSeparador("Cliente");
            txtClienteNombre.ReadOnly = true;
            txtClienteNif.ReadOnly = true;
            AgregarFila("Nombre:", txtClienteNombre);
            AgregarFila("NIF:", txtClienteNif);

            // ── Importes ──────────────────────────────────────────────────────
            AgregarSeparador("Importes");
            txtBase.ReadOnly = true;
            txtPorcentajeIva.ReadOnly = true;
            txtCuotaIva.ReadOnly = true;
            txtPorcentajeIrpf.ReadOnly = true;
            txtCuotaIrpf.ReadOnly = true;
            txtPorcentajeRe.ReadOnly = true;
            txtCuotaRe.ReadOnly = true;
            txtTotalCalculado.ReadOnly = true;
            txtDiferencia.ReadOnly = true;
            txtTotal.ReadOnly = true;

            AgregarFila("Base imponible:", txtBase);
            AgregarFila("% IVA:", txtPorcentajeIva);
            AgregarFila("Cuota IVA:", txtCuotaIva);
            AgregarFila("% IRPF:", txtPorcentajeIrpf);
            AgregarFila("Cuota IRPF:", txtCuotaIrpf);
            AgregarFila("% RE:", txtPorcentajeRe);
            AgregarFila("Cuota RE:", txtCuotaRe);
            AgregarFila("Total factura:", txtTotal);
            AgregarFila("Total calculado:", txtTotalCalculado);
            AgregarFila("Diferencia:", txtDiferencia);
            AgregarFila("Total:", txtTotal);

            // ── Archivo ───────────────────────────────────────────────────────
            AgregarSeparador("Archivo");
            txtRutaArchivo.ReadOnly = true;
            AgregarFila("Ruta:", txtRutaArchivo);

            // ── Panel de error (oculto por defecto) ───────────────────────────
            panelError.Location = new Point(12, y);
            panelError.Size = new Size(480, 50);
            panelError.BackColor = Color.FromArgb(252, 228, 214);
            panelError.Visible = false;
            panelError.BorderStyle = BorderStyle.FixedSingle;

            txtError.Multiline = true;
            txtError.ReadOnly = true;
            txtError.BorderStyle = BorderStyle.None;
            txtError.BackColor = Color.FromArgb(252, 228, 214);
            txtError.Location = new Point(5, 5);
            txtError.Size = new Size(468, 38);
            panelError.Controls.Add(txtError);
            Controls.Add(panelError);
            y += 58;

            // ── Botones ───────────────────────────────────────────────────────
            btnAbrirPdf.Text = "📄 Abrir PDF";
            btnAbrirPdf.Location = new Point(12, y);
            btnAbrirPdf.Size = new Size(120, 32);
            btnAbrirPdf.FlatStyle = FlatStyle.Flat;
            btnAbrirPdf.BackColor = Color.FromArgb(46, 117, 182);
            btnAbrirPdf.ForeColor = Color.White;
            btnAbrirPdf.Click += btnAbrirPdf_Click;

            btnCerrar.Text = "Cerrar";
            btnCerrar.Location = new Point(410, y);
            btnCerrar.Size = new Size(80, 32);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Click += (s, e) => Close();

            Controls.Add(btnAbrirPdf);
            Controls.Add(btnCerrar);

            ClientSize = new Size(504, y + 50);
        }

        private void CargarDatos()
        {
            Text = $"Detalle — {_factura.NumeroFactura}";

            txtNumeroFactura.Text = _factura.NumeroFactura;
            txtFecha.Text = _factura.Fecha?.ToString("dd/MM/yyyy") ?? "—";
            txtEstado.Text = _factura.Estado.ToString();
            chkOcr.Checked = _factura.ExtractedByOcr;

            txtEmisorNombre.Text = _factura.Emisor.Nombre;
            txtEmisorNif.Text = _factura.Emisor.NIF;

            txtClienteNombre.Text = _factura.Receptor.Nombre;
            txtClienteNif.Text = _factura.Receptor.NIF;

            txtBase.Text = $"{_factura.BaseImponible:N2} €";
            txtPorcentajeIva.Text = $"{_factura.PorcentajeIVA} %";
            txtCuotaIva.Text = $"{_factura.CuotaIVA:N2} €";
            txtTotal.Text = $"{_factura.Total:N2} €";

            txtPorcentajeIrpf.Text = $"{_factura.PorcentajeIRPF} %";
            txtCuotaIrpf.Text = $"{_factura.CuotaIRPF:N2} €";
            txtPorcentajeRe.Text = $"{_factura.PorcentajeRE} %";
            txtCuotaRe.Text = $"{_factura.CuotaRE:N2} €";
            txtTotalCalculado.Text = $"{_factura.TotalCalculado:N2} €";
            txtDiferencia.Text = $"{_factura.DiferenciaTotal:N2} €";

            // Color rojo en diferencia si los totales no coinciden
            txtDiferencia.BackColor = _factura.TotalesCoinciden
                ? System.Drawing.Color.FromArgb(226, 239, 218)
                : System.Drawing.Color.FromArgb(252, 228, 214);

            txtRutaArchivo.Text = _factura.RutaArchivo;

            // Color del campo estado según valor
            txtEstado.BackColor = _factura.Estado switch
            {
                EstadoFactura.OK => Color.FromArgb(226, 239, 218),
                EstadoFactura.RevisionManual => Color.FromArgb(255, 242, 204),
                EstadoFactura.Error => Color.FromArgb(252, 228, 214),
                _ => SystemColors.Window
            };

            if (!string.IsNullOrEmpty(_factura.ErrorMensaje))
            {
                panelError.Visible = true;
                txtError.Text = _factura.ErrorMensaje;
            }

            // Desactivar botón PDF si el archivo viene de Excel
            btnAbrirPdf.Enabled = _factura.RutaArchivo.EndsWith(
                ".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeComponent()
        {

        }

        private void btnAbrirPdf_Click(object? sender, EventArgs e)
        {
            if (!File.Exists(_factura.RutaArchivo))
            {
                MessageBox.Show("No se encuentra el archivo PDF.",
                    "Archivo no encontrado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _factura.RutaArchivo,
                    UseShellExecute = true
                });
        }
    }
}