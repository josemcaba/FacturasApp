using FacturasApp.Models;

namespace FacturasApp.UI
{
    public partial class DetalleFacturaForm : Form
    {
        private readonly Factura _factura;
        public DetalleFacturaForm(Factura factura)
        {
            _factura = factura;
            InitializeComponent();
            CargarDatos();
            // Habilitar el botón para abrir el archivo PDF
            // solo si la ruta del archivo no está vacía y
            // se trata de un PDF
            btnAbrirArchivoPDF.Enabled = !string.IsNullOrEmpty(_factura.RutaArchivo) && 
                                          Path.GetExtension(_factura.RutaArchivo).Equals(".pdf", 
                                          StringComparison.OrdinalIgnoreCase);
        }

        private void CargarDatos()
        {
            txtNumFactura.Text = _factura.NumeroFactura;
            txtFecha.Text = _factura.Fecha?.ToString("dd/MM/yyyy");
            txtEstado.Text = _factura.Estado.ToString();
            txtNifEmisor.Text = _factura.Emisor.NIF;
            txtNombreEmisor.Text = _factura.Emisor.Nombre;
            txtNifCliente.Text = _factura.Receptor.NIF;
            txtNombreCliente.Text = _factura.Receptor.Nombre;

            txtBaseExtraida.Text = $"{_factura.BaseImponible.ToString("N2")} €";
            txtIvaExtraido.Text = $"{_factura.PorcentajeIVA.ToString("N0")} %";
            txtCuotaIvaCalculada.Text = _factura.CuotaIVACalculado.ToString("N2");

            if (_factura.PorcentajeIRPF > 0)
            {
                txtIrpfExtraido.Text = $"{_factura.PorcentajeIRPF.ToString("N0")} %";
                txtCuotaIrpfCalculada.Text = _factura.CuotaIRPFCalculado.ToString("N2");
            }
            if (_factura.PorcentajeRE > 0)
            {
                txtReExtraido.Text = $"{_factura.PorcentajeRE.ToString("N1")} %";
                txtCuotaReCalculada.Text = _factura.CuotaRECalculado.ToString("N2");
            }
            txtTotalCalculado.Text = $"{_factura.TotalCalculado.ToString("N2")} €";
            txtTotalExtraido.Text = $"{_factura.TotalFactura.ToString("N2")} €";

            if (!_factura.TotalesCoinciden)
            {
                txtEstado.BackColor = Color.Red;
                txtTotalCalculado.BackColor = Color.Red;
                txtTotalExtraido.BackColor = Color.Red;
            }

            txtRuta.Text = _factura.RutaArchivo;
            txtMensajes.Text = string.Join(Environment.NewLine, _factura.MensajeError);
        }

        private void btnAbrirArchivoPDF_Click(object sender, EventArgs e)
        {
            if (!File.Exists(_factura.RutaArchivo))
            {
                MessageBox.Show("No se encuentra el archivo PDF.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = _factura.RutaArchivo,
                UseShellExecute = true
            });
        }

        // Sobreescribe OnShown para quitar el foco de cualquier control al mostrar el formulario:
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ActiveControl = null;
        }
    }
}