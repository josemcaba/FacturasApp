namespace FacturasApp.UI
{
    partial class DetalleFacturaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gboxFactura = new GroupBox();
            txtNumFactura = new TextBox();
            txtEstado = new TextBox();
            lblFecha = new Label();
            txtFecha = new TextBox();
            lblEstado = new Label();
            lblNumFactura = new Label();
            gboxEmisor = new GroupBox();
            txtNombreEmisor = new TextBox();
            txtNifEmisor = new TextBox();
            lblNombreEmisor = new Label();
            lblNifEmisor = new Label();
            gboxCliente = new GroupBox();
            txtNombreCliente = new TextBox();
            lblNombreCliente = new Label();
            txtNifCliente = new TextBox();
            lblNifCliente = new Label();
            gboxImportesExtraidos = new GroupBox();
            txtIvaExtraido = new TextBox();
            lblBaseExtraida = new Label();
            txtTotalExtraido = new TextBox();
            lblIvaExtraido = new Label();
            lblTotalFacturaExtraido = new Label();
            lblIrpfExtraido = new Label();
            txtBaseExtraida = new TextBox();
            lblReExtraido = new Label();
            txtCuotaReExtraido = new TextBox();
            txtIrpfExtraido = new TextBox();
            txtCuotaIrpfExtraido = new TextBox();
            txtReExtraido = new TextBox();
            txtCuotaIvaExtraido = new TextBox();
            gboxImportesCalculados = new GroupBox();
            txtIvaCalculado = new TextBox();
            lblBaseCalculada = new Label();
            txtTotalCalculado = new TextBox();
            lblIvaCalculado = new Label();
            lblTotalCalculado = new Label();
            lblIrpfCalculado = new Label();
            txtBaseCalculada = new TextBox();
            lblReCalculado = new Label();
            txtCuotaReCalculada = new TextBox();
            txtIrpfCalculado = new TextBox();
            txtCuotaIrpfCalculada = new TextBox();
            txtReCalculado = new TextBox();
            txtCuotaIvaCalculada = new TextBox();
            gboxArchivo = new GroupBox();
            txtRuta = new TextBox();
            lblRuta = new Label();
            btnAbrirArchivoPDF = new Button();
            txtMensajes = new TextBox();
            gboxFactura.SuspendLayout();
            gboxEmisor.SuspendLayout();
            gboxCliente.SuspendLayout();
            gboxImportesExtraidos.SuspendLayout();
            gboxImportesCalculados.SuspendLayout();
            gboxArchivo.SuspendLayout();
            SuspendLayout();
            // 
            // gboxFactura
            // 
            gboxFactura.Controls.Add(txtNumFactura);
            gboxFactura.Controls.Add(txtEstado);
            gboxFactura.Controls.Add(lblFecha);
            gboxFactura.Controls.Add(txtFecha);
            gboxFactura.Controls.Add(lblEstado);
            gboxFactura.Controls.Add(lblNumFactura);
            gboxFactura.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxFactura.ForeColor = SystemColors.MenuHighlight;
            gboxFactura.Location = new Point(12, 12);
            gboxFactura.Name = "gboxFactura";
            gboxFactura.Size = new Size(855, 90);
            gboxFactura.TabIndex = 11;
            gboxFactura.TabStop = false;
            gboxFactura.Text = "FACTURA";
            // 
            // txtNumFactura
            // 
            txtNumFactura.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNumFactura.Location = new Point(83, 36);
            txtNumFactura.Name = "txtNumFactura";
            txtNumFactura.ReadOnly = true;
            txtNumFactura.Size = new Size(304, 27);
            txtNumFactura.TabIndex = 7;
            // 
            // txtEstado
            // 
            txtEstado.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtEstado.Location = new Point(749, 36);
            txtEstado.Name = "txtEstado";
            txtEstado.ReadOnly = true;
            txtEstado.Size = new Size(100, 27);
            txtEstado.TabIndex = 9;
            txtEstado.TextAlign = HorizontalAlignment.Center;
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = true;
            lblFecha.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFecha.ForeColor = SystemColors.ControlText;
            lblFecha.Location = new Point(417, 39);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(53, 20);
            lblFecha.TabIndex = 1;
            lblFecha.Text = "Fecha:";
            // 
            // txtFecha
            // 
            txtFecha.Font = new Font("Segoe UI", 9F);
            txtFecha.Location = new Point(476, 36);
            txtFecha.Name = "txtFecha";
            txtFecha.ReadOnly = true;
            txtFecha.Size = new Size(100, 27);
            txtFecha.TabIndex = 8;
            txtFecha.TextAlign = HorizontalAlignment.Center;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblEstado.ForeColor = SystemColors.ControlText;
            lblEstado.Location = new Point(683, 39);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(60, 20);
            lblEstado.TabIndex = 2;
            lblEstado.Text = "Estado:";
            // 
            // lblNumFactura
            // 
            lblNumFactura.AutoSize = true;
            lblNumFactura.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNumFactura.ForeColor = SystemColors.ControlText;
            lblNumFactura.Location = new Point(6, 39);
            lblNumFactura.Name = "lblNumFactura";
            lblNumFactura.Size = new Size(71, 20);
            lblNumFactura.TabIndex = 0;
            lblNumFactura.Text = "Número:";
            // 
            // gboxEmisor
            // 
            gboxEmisor.Controls.Add(txtNombreEmisor);
            gboxEmisor.Controls.Add(txtNifEmisor);
            gboxEmisor.Controls.Add(lblNombreEmisor);
            gboxEmisor.Controls.Add(lblNifEmisor);
            gboxEmisor.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxEmisor.ForeColor = SystemColors.MenuHighlight;
            gboxEmisor.Location = new Point(12, 108);
            gboxEmisor.Name = "gboxEmisor";
            gboxEmisor.Size = new Size(425, 102);
            gboxEmisor.TabIndex = 5;
            gboxEmisor.TabStop = false;
            gboxEmisor.Text = "Emisor";
            // 
            // txtNombreEmisor
            // 
            txtNombreEmisor.Font = new Font("Segoe UI", 9F);
            txtNombreEmisor.Location = new Point(83, 60);
            txtNombreEmisor.Name = "txtNombreEmisor";
            txtNombreEmisor.ReadOnly = true;
            txtNombreEmisor.Size = new Size(336, 27);
            txtNombreEmisor.TabIndex = 10;
            // 
            // txtNifEmisor
            // 
            txtNifEmisor.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNifEmisor.Location = new Point(83, 27);
            txtNifEmisor.Name = "txtNifEmisor";
            txtNifEmisor.ReadOnly = true;
            txtNifEmisor.Size = new Size(336, 27);
            txtNifEmisor.TabIndex = 10;
            // 
            // lblNombreEmisor
            // 
            lblNombreEmisor.AutoSize = true;
            lblNombreEmisor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNombreEmisor.ForeColor = SystemColors.ControlText;
            lblNombreEmisor.Location = new Point(6, 60);
            lblNombreEmisor.Name = "lblNombreEmisor";
            lblNombreEmisor.Size = new Size(71, 20);
            lblNombreEmisor.TabIndex = 4;
            lblNombreEmisor.Text = "Nombre:";
            // 
            // lblNifEmisor
            // 
            lblNifEmisor.AutoSize = true;
            lblNifEmisor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNifEmisor.ForeColor = SystemColors.ControlText;
            lblNifEmisor.Location = new Point(6, 31);
            lblNifEmisor.Name = "lblNifEmisor";
            lblNifEmisor.Size = new Size(38, 20);
            lblNifEmisor.TabIndex = 3;
            lblNifEmisor.Text = "NIF:";
            // 
            // gboxCliente
            // 
            gboxCliente.Controls.Add(txtNombreCliente);
            gboxCliente.Controls.Add(lblNombreCliente);
            gboxCliente.Controls.Add(txtNifCliente);
            gboxCliente.Controls.Add(lblNifCliente);
            gboxCliente.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxCliente.ForeColor = SystemColors.MenuHighlight;
            gboxCliente.Location = new Point(443, 108);
            gboxCliente.Name = "gboxCliente";
            gboxCliente.Size = new Size(425, 102);
            gboxCliente.TabIndex = 6;
            gboxCliente.TabStop = false;
            gboxCliente.Text = "Cliente";
            // 
            // txtNombreCliente
            // 
            txtNombreCliente.Font = new Font("Segoe UI", 9F);
            txtNombreCliente.Location = new Point(83, 57);
            txtNombreCliente.Name = "txtNombreCliente";
            txtNombreCliente.ReadOnly = true;
            txtNombreCliente.Size = new Size(336, 27);
            txtNombreCliente.TabIndex = 11;
            // 
            // lblNombreCliente
            // 
            lblNombreCliente.AutoSize = true;
            lblNombreCliente.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNombreCliente.ForeColor = SystemColors.ControlText;
            lblNombreCliente.Location = new Point(6, 60);
            lblNombreCliente.Name = "lblNombreCliente";
            lblNombreCliente.Size = new Size(71, 20);
            lblNombreCliente.TabIndex = 4;
            lblNombreCliente.Text = "Nombre:";
            // 
            // txtNifCliente
            // 
            txtNifCliente.Font = new Font("Segoe UI", 9F);
            txtNifCliente.Location = new Point(83, 26);
            txtNifCliente.Name = "txtNifCliente";
            txtNifCliente.ReadOnly = true;
            txtNifCliente.Size = new Size(336, 27);
            txtNifCliente.TabIndex = 12;
            // 
            // lblNifCliente
            // 
            lblNifCliente.AutoSize = true;
            lblNifCliente.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNifCliente.ForeColor = SystemColors.ControlText;
            lblNifCliente.Location = new Point(6, 31);
            lblNifCliente.Name = "lblNifCliente";
            lblNifCliente.Size = new Size(38, 20);
            lblNifCliente.TabIndex = 3;
            lblNifCliente.Text = "NIF:";
            // 
            // gboxImportesExtraidos
            // 
            gboxImportesExtraidos.Controls.Add(txtIvaExtraido);
            gboxImportesExtraidos.Controls.Add(lblBaseExtraida);
            gboxImportesExtraidos.Controls.Add(txtTotalExtraido);
            gboxImportesExtraidos.Controls.Add(lblIvaExtraido);
            gboxImportesExtraidos.Controls.Add(lblTotalFacturaExtraido);
            gboxImportesExtraidos.Controls.Add(lblIrpfExtraido);
            gboxImportesExtraidos.Controls.Add(txtBaseExtraida);
            gboxImportesExtraidos.Controls.Add(lblReExtraido);
            gboxImportesExtraidos.Controls.Add(txtCuotaReExtraido);
            gboxImportesExtraidos.Controls.Add(txtIrpfExtraido);
            gboxImportesExtraidos.Controls.Add(txtCuotaIrpfExtraido);
            gboxImportesExtraidos.Controls.Add(txtReExtraido);
            gboxImportesExtraidos.Controls.Add(txtCuotaIvaExtraido);
            gboxImportesExtraidos.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxImportesExtraidos.ForeColor = SystemColors.MenuHighlight;
            gboxImportesExtraidos.Location = new Point(12, 216);
            gboxImportesExtraidos.Name = "gboxImportesExtraidos";
            gboxImportesExtraidos.Size = new Size(425, 227);
            gboxImportesExtraidos.TabIndex = 36;
            gboxImportesExtraidos.TabStop = false;
            gboxImportesExtraidos.Text = "Importes extraidos";
            // 
            // txtIvaExtraido
            // 
            txtIvaExtraido.Font = new Font("Segoe UI", 9F);
            txtIvaExtraido.Location = new Point(127, 74);
            txtIvaExtraido.Name = "txtIvaExtraido";
            txtIvaExtraido.ReadOnly = true;
            txtIvaExtraido.Size = new Size(77, 27);
            txtIvaExtraido.TabIndex = 23;
            txtIvaExtraido.TextAlign = HorizontalAlignment.Center;
            // 
            // lblBaseExtraida
            // 
            lblBaseExtraida.AutoSize = true;
            lblBaseExtraida.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBaseExtraida.ForeColor = SystemColors.ControlText;
            lblBaseExtraida.Location = new Point(83, 44);
            lblBaseExtraida.Name = "lblBaseExtraida";
            lblBaseExtraida.Size = new Size(121, 20);
            lblBaseExtraida.TabIndex = 24;
            lblBaseExtraida.Text = "Base Imponible:";
            // 
            // txtTotalExtraido
            // 
            txtTotalExtraido.Font = new Font("Segoe UI", 9F);
            txtTotalExtraido.Location = new Point(210, 173);
            txtTotalExtraido.Name = "txtTotalExtraido";
            txtTotalExtraido.ReadOnly = true;
            txtTotalExtraido.Size = new Size(129, 27);
            txtTotalExtraido.TabIndex = 35;
            txtTotalExtraido.TextAlign = HorizontalAlignment.Right;
            // 
            // lblIvaExtraido
            // 
            lblIvaExtraido.AutoSize = true;
            lblIvaExtraido.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIvaExtraido.ForeColor = SystemColors.ControlText;
            lblIvaExtraido.Location = new Point(83, 77);
            lblIvaExtraido.Name = "lblIvaExtraido";
            lblIvaExtraido.Size = new Size(38, 20);
            lblIvaExtraido.TabIndex = 25;
            lblIvaExtraido.Text = "IVA:";
            // 
            // lblTotalFacturaExtraido
            // 
            lblTotalFacturaExtraido.AutoSize = true;
            lblTotalFacturaExtraido.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalFacturaExtraido.ForeColor = SystemColors.ControlText;
            lblTotalFacturaExtraido.Location = new Point(83, 176);
            lblTotalFacturaExtraido.Name = "lblTotalFacturaExtraido";
            lblTotalFacturaExtraido.Size = new Size(104, 20);
            lblTotalFacturaExtraido.TabIndex = 34;
            lblTotalFacturaExtraido.Text = "Total Factura:";
            // 
            // lblIrpfExtraido
            // 
            lblIrpfExtraido.AutoSize = true;
            lblIrpfExtraido.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIrpfExtraido.ForeColor = SystemColors.ControlText;
            lblIrpfExtraido.Location = new Point(83, 110);
            lblIrpfExtraido.Name = "lblIrpfExtraido";
            lblIrpfExtraido.Size = new Size(45, 20);
            lblIrpfExtraido.TabIndex = 26;
            lblIrpfExtraido.Text = "IRPF:";
            // 
            // txtBaseExtraida
            // 
            txtBaseExtraida.Font = new Font("Segoe UI", 9F);
            txtBaseExtraida.Location = new Point(210, 41);
            txtBaseExtraida.Name = "txtBaseExtraida";
            txtBaseExtraida.ReadOnly = true;
            txtBaseExtraida.Size = new Size(129, 27);
            txtBaseExtraida.TabIndex = 33;
            txtBaseExtraida.TextAlign = HorizontalAlignment.Right;
            // 
            // lblReExtraido
            // 
            lblReExtraido.AutoSize = true;
            lblReExtraido.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblReExtraido.ForeColor = SystemColors.ControlText;
            lblReExtraido.Location = new Point(83, 143);
            lblReExtraido.Name = "lblReExtraido";
            lblReExtraido.Size = new Size(31, 20);
            lblReExtraido.TabIndex = 27;
            lblReExtraido.Text = "RE:";
            // 
            // txtCuotaReExtraido
            // 
            txtCuotaReExtraido.Font = new Font("Segoe UI", 9F);
            txtCuotaReExtraido.Location = new Point(210, 140);
            txtCuotaReExtraido.Name = "txtCuotaReExtraido";
            txtCuotaReExtraido.ReadOnly = true;
            txtCuotaReExtraido.Size = new Size(129, 27);
            txtCuotaReExtraido.TabIndex = 32;
            txtCuotaReExtraido.TextAlign = HorizontalAlignment.Right;
            // 
            // txtIrpfExtraido
            // 
            txtIrpfExtraido.Font = new Font("Segoe UI", 9F);
            txtIrpfExtraido.Location = new Point(127, 107);
            txtIrpfExtraido.Name = "txtIrpfExtraido";
            txtIrpfExtraido.ReadOnly = true;
            txtIrpfExtraido.Size = new Size(77, 27);
            txtIrpfExtraido.TabIndex = 28;
            txtIrpfExtraido.TextAlign = HorizontalAlignment.Center;
            // 
            // txtCuotaIrpfExtraido
            // 
            txtCuotaIrpfExtraido.Font = new Font("Segoe UI", 9F);
            txtCuotaIrpfExtraido.Location = new Point(210, 107);
            txtCuotaIrpfExtraido.Name = "txtCuotaIrpfExtraido";
            txtCuotaIrpfExtraido.ReadOnly = true;
            txtCuotaIrpfExtraido.Size = new Size(129, 27);
            txtCuotaIrpfExtraido.TabIndex = 31;
            txtCuotaIrpfExtraido.TextAlign = HorizontalAlignment.Right;
            // 
            // txtReExtraido
            // 
            txtReExtraido.Font = new Font("Segoe UI", 9F);
            txtReExtraido.Location = new Point(127, 140);
            txtReExtraido.Name = "txtReExtraido";
            txtReExtraido.ReadOnly = true;
            txtReExtraido.Size = new Size(77, 27);
            txtReExtraido.TabIndex = 29;
            txtReExtraido.TextAlign = HorizontalAlignment.Center;
            // 
            // txtCuotaIvaExtraido
            // 
            txtCuotaIvaExtraido.Font = new Font("Segoe UI", 9F);
            txtCuotaIvaExtraido.Location = new Point(210, 74);
            txtCuotaIvaExtraido.Name = "txtCuotaIvaExtraido";
            txtCuotaIvaExtraido.ReadOnly = true;
            txtCuotaIvaExtraido.Size = new Size(129, 27);
            txtCuotaIvaExtraido.TabIndex = 30;
            txtCuotaIvaExtraido.TextAlign = HorizontalAlignment.Right;
            // 
            // gboxImportesCalculados
            // 
            gboxImportesCalculados.Controls.Add(txtIvaCalculado);
            gboxImportesCalculados.Controls.Add(lblBaseCalculada);
            gboxImportesCalculados.Controls.Add(txtTotalCalculado);
            gboxImportesCalculados.Controls.Add(lblIvaCalculado);
            gboxImportesCalculados.Controls.Add(lblTotalCalculado);
            gboxImportesCalculados.Controls.Add(lblIrpfCalculado);
            gboxImportesCalculados.Controls.Add(txtBaseCalculada);
            gboxImportesCalculados.Controls.Add(lblReCalculado);
            gboxImportesCalculados.Controls.Add(txtCuotaReCalculada);
            gboxImportesCalculados.Controls.Add(txtIrpfCalculado);
            gboxImportesCalculados.Controls.Add(txtCuotaIrpfCalculada);
            gboxImportesCalculados.Controls.Add(txtReCalculado);
            gboxImportesCalculados.Controls.Add(txtCuotaIvaCalculada);
            gboxImportesCalculados.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxImportesCalculados.ForeColor = SystemColors.MenuHighlight;
            gboxImportesCalculados.Location = new Point(442, 216);
            gboxImportesCalculados.Name = "gboxImportesCalculados";
            gboxImportesCalculados.Size = new Size(425, 227);
            gboxImportesCalculados.TabIndex = 37;
            gboxImportesCalculados.TabStop = false;
            gboxImportesCalculados.Text = "Importes calculados";
            // 
            // txtIvaCalculado
            // 
            txtIvaCalculado.Font = new Font("Segoe UI", 9F);
            txtIvaCalculado.Location = new Point(127, 74);
            txtIvaCalculado.Name = "txtIvaCalculado";
            txtIvaCalculado.ReadOnly = true;
            txtIvaCalculado.Size = new Size(77, 27);
            txtIvaCalculado.TabIndex = 23;
            txtIvaCalculado.TextAlign = HorizontalAlignment.Center;
            // 
            // lblBaseCalculada
            // 
            lblBaseCalculada.AutoSize = true;
            lblBaseCalculada.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBaseCalculada.ForeColor = SystemColors.ControlText;
            lblBaseCalculada.Location = new Point(83, 44);
            lblBaseCalculada.Name = "lblBaseCalculada";
            lblBaseCalculada.Size = new Size(121, 20);
            lblBaseCalculada.TabIndex = 24;
            lblBaseCalculada.Text = "Base Imponible:";
            // 
            // txtTotalCalculado
            // 
            txtTotalCalculado.Font = new Font("Segoe UI", 9F);
            txtTotalCalculado.Location = new Point(210, 173);
            txtTotalCalculado.Name = "txtTotalCalculado";
            txtTotalCalculado.ReadOnly = true;
            txtTotalCalculado.Size = new Size(129, 27);
            txtTotalCalculado.TabIndex = 35;
            txtTotalCalculado.TextAlign = HorizontalAlignment.Right;
            // 
            // lblIvaCalculado
            // 
            lblIvaCalculado.AutoSize = true;
            lblIvaCalculado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIvaCalculado.ForeColor = SystemColors.ControlText;
            lblIvaCalculado.Location = new Point(83, 77);
            lblIvaCalculado.Name = "lblIvaCalculado";
            lblIvaCalculado.Size = new Size(38, 20);
            lblIvaCalculado.TabIndex = 25;
            lblIvaCalculado.Text = "IVA:";
            // 
            // lblTotalCalculado
            // 
            lblTotalCalculado.AutoSize = true;
            lblTotalCalculado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalCalculado.ForeColor = SystemColors.ControlText;
            lblTotalCalculado.Location = new Point(83, 176);
            lblTotalCalculado.Name = "lblTotalCalculado";
            lblTotalCalculado.Size = new Size(104, 20);
            lblTotalCalculado.TabIndex = 34;
            lblTotalCalculado.Text = "Total Factura:";
            // 
            // lblIrpfCalculado
            // 
            lblIrpfCalculado.AutoSize = true;
            lblIrpfCalculado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIrpfCalculado.ForeColor = SystemColors.ControlText;
            lblIrpfCalculado.Location = new Point(83, 110);
            lblIrpfCalculado.Name = "lblIrpfCalculado";
            lblIrpfCalculado.Size = new Size(45, 20);
            lblIrpfCalculado.TabIndex = 26;
            lblIrpfCalculado.Text = "IRPF:";
            // 
            // txtBaseCalculada
            // 
            txtBaseCalculada.Font = new Font("Segoe UI", 9F);
            txtBaseCalculada.Location = new Point(210, 41);
            txtBaseCalculada.Name = "txtBaseCalculada";
            txtBaseCalculada.ReadOnly = true;
            txtBaseCalculada.Size = new Size(129, 27);
            txtBaseCalculada.TabIndex = 33;
            txtBaseCalculada.TextAlign = HorizontalAlignment.Right;
            // 
            // lblReCalculado
            // 
            lblReCalculado.AutoSize = true;
            lblReCalculado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblReCalculado.ForeColor = SystemColors.ControlText;
            lblReCalculado.Location = new Point(83, 143);
            lblReCalculado.Name = "lblReCalculado";
            lblReCalculado.Size = new Size(31, 20);
            lblReCalculado.TabIndex = 27;
            lblReCalculado.Text = "RE:";
            // 
            // txtCuotaReCalculada
            // 
            txtCuotaReCalculada.Font = new Font("Segoe UI", 9F);
            txtCuotaReCalculada.Location = new Point(210, 140);
            txtCuotaReCalculada.Name = "txtCuotaReCalculada";
            txtCuotaReCalculada.ReadOnly = true;
            txtCuotaReCalculada.Size = new Size(129, 27);
            txtCuotaReCalculada.TabIndex = 32;
            txtCuotaReCalculada.TextAlign = HorizontalAlignment.Right;
            // 
            // txtIrpfCalculado
            // 
            txtIrpfCalculado.Font = new Font("Segoe UI", 9F);
            txtIrpfCalculado.Location = new Point(127, 107);
            txtIrpfCalculado.Name = "txtIrpfCalculado";
            txtIrpfCalculado.ReadOnly = true;
            txtIrpfCalculado.Size = new Size(77, 27);
            txtIrpfCalculado.TabIndex = 28;
            txtIrpfCalculado.TextAlign = HorizontalAlignment.Center;
            // 
            // txtCuotaIrpfCalculada
            // 
            txtCuotaIrpfCalculada.Font = new Font("Segoe UI", 9F);
            txtCuotaIrpfCalculada.Location = new Point(210, 107);
            txtCuotaIrpfCalculada.Name = "txtCuotaIrpfCalculada";
            txtCuotaIrpfCalculada.ReadOnly = true;
            txtCuotaIrpfCalculada.Size = new Size(129, 27);
            txtCuotaIrpfCalculada.TabIndex = 31;
            txtCuotaIrpfCalculada.TextAlign = HorizontalAlignment.Right;
            // 
            // txtReCalculado
            // 
            txtReCalculado.Font = new Font("Segoe UI", 9F);
            txtReCalculado.Location = new Point(127, 140);
            txtReCalculado.Name = "txtReCalculado";
            txtReCalculado.ReadOnly = true;
            txtReCalculado.Size = new Size(77, 27);
            txtReCalculado.TabIndex = 29;
            txtReCalculado.TextAlign = HorizontalAlignment.Center;
            // 
            // txtCuotaIvaCalculada
            // 
            txtCuotaIvaCalculada.Font = new Font("Segoe UI", 9F);
            txtCuotaIvaCalculada.Location = new Point(210, 74);
            txtCuotaIvaCalculada.Name = "txtCuotaIvaCalculada";
            txtCuotaIvaCalculada.ReadOnly = true;
            txtCuotaIvaCalculada.Size = new Size(129, 27);
            txtCuotaIvaCalculada.TabIndex = 30;
            txtCuotaIvaCalculada.TextAlign = HorizontalAlignment.Right;
            // 
            // gboxArchivo
            // 
            gboxArchivo.Controls.Add(txtRuta);
            gboxArchivo.Controls.Add(lblRuta);
            gboxArchivo.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxArchivo.ForeColor = SystemColors.MenuHighlight;
            gboxArchivo.Location = new Point(12, 449);
            gboxArchivo.Name = "gboxArchivo";
            gboxArchivo.Size = new Size(855, 74);
            gboxArchivo.TabIndex = 38;
            gboxArchivo.TabStop = false;
            gboxArchivo.Text = "Archivo";
            // 
            // txtRuta
            // 
            txtRuta.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtRuta.Location = new Point(58, 29);
            txtRuta.Name = "txtRuta";
            txtRuta.ReadOnly = true;
            txtRuta.Size = new Size(791, 27);
            txtRuta.TabIndex = 10;
            txtRuta.TextAlign = HorizontalAlignment.Right;
            // 
            // lblRuta
            // 
            lblRuta.AutoSize = true;
            lblRuta.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRuta.ForeColor = SystemColors.ControlText;
            lblRuta.Location = new Point(6, 32);
            lblRuta.Name = "lblRuta";
            lblRuta.Size = new Size(46, 20);
            lblRuta.TabIndex = 3;
            lblRuta.Text = "Ruta:";
            // 
            // btnAbrirArchivoPDF
            // 
            btnAbrirArchivoPDF.BackColor = SystemColors.MenuHighlight;
            btnAbrirArchivoPDF.Enabled = false;
            btnAbrirArchivoPDF.FlatStyle = FlatStyle.Flat;
            btnAbrirArchivoPDF.Location = new Point(717, 622);
            btnAbrirArchivoPDF.Name = "btnAbrirArchivoPDF";
            btnAbrirArchivoPDF.Size = new Size(150, 29);
            btnAbrirArchivoPDF.TabIndex = 1;
            btnAbrirArchivoPDF.Text = "Abrir Factura PDF";
            btnAbrirArchivoPDF.UseVisualStyleBackColor = false;
            btnAbrirArchivoPDF.Click += btnAbrirArchivoPDF_Click;
            // 
            // txtMensajes
            // 
            txtMensajes.BackColor = SystemColors.Control;
            txtMensajes.Location = new Point(12, 529);
            txtMensajes.Multiline = true;
            txtMensajes.Name = "txtMensajes";
            txtMensajes.ReadOnly = true;
            txtMensajes.Size = new Size(855, 87);
            txtMensajes.TabIndex = 42;
            // 
            // DetalleFacturaForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(879, 663);
            Controls.Add(txtMensajes);
            Controls.Add(btnAbrirArchivoPDF);
            Controls.Add(gboxArchivo);
            Controls.Add(gboxImportesCalculados);
            Controls.Add(gboxImportesExtraidos);
            Controls.Add(gboxCliente);
            Controls.Add(gboxEmisor);
            Controls.Add(gboxFactura);
            MaximizeBox = false;
            MaximumSize = new Size(897, 710);
            MinimizeBox = false;
            MinimumSize = new Size(897, 710);
            Name = "DetalleFacturaForm";
            Text = "Detalle";
            gboxFactura.ResumeLayout(false);
            gboxFactura.PerformLayout();
            gboxEmisor.ResumeLayout(false);
            gboxEmisor.PerformLayout();
            gboxCliente.ResumeLayout(false);
            gboxCliente.PerformLayout();
            gboxImportesExtraidos.ResumeLayout(false);
            gboxImportesExtraidos.PerformLayout();
            gboxImportesCalculados.ResumeLayout(false);
            gboxImportesCalculados.PerformLayout();
            gboxArchivo.ResumeLayout(false);
            gboxArchivo.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblNumFactura;
        private Label lblFecha;
        private Label lblEstado;
        private Label lblNifEmisor;
        private Label lblNombreEmisor;
        private GroupBox gboxEmisor;
        private GroupBox gboxCliente;
        private Label lblNombreCliente;
        private Label lblNifCliente;
        private TextBox txtNombreEmisor;
        private TextBox txtNifEmisor;
        private TextBox txtNumFactura;
        private TextBox txtFecha;
        private TextBox txtEstado;
        private TextBox txtNombreCliente;
        private TextBox txtNifCliente;
        private GroupBox gboxFactura;
        private GroupBox gboxImportesExtraidos;
        private TextBox txtIvaExtraido;
        private Label lblBaseExtraida;
        private TextBox txtTotalExtraido;
        private Label lblIvaExtraido;
        private Label lblTotalFacturaExtraido;
        private Label lblIrpfExtraido;
        private TextBox txtBaseExtraida;
        private Label lblReExtraido;
        private TextBox txtCuotaReExtraido;
        private TextBox txtIrpfExtraido;
        private TextBox txtCuotaIrpfExtraido;
        private TextBox txtReExtraido;
        private TextBox txtCuotaIvaExtraido;
        private GroupBox gboxImportesCalculados;
        private TextBox txtIvaCalculado;
        private Label lblBaseCalculada;
        private TextBox txtTotalCalculado;
        private Label lblIvaCalculado;
        private Label lblTotalCalculado;
        private Label lblIrpfCalculado;
        private TextBox txtBaseCalculada;
        private Label lblReCalculado;
        private TextBox txtCuotaReCalculada;
        private TextBox txtIrpfCalculado;
        private TextBox txtCuotaIrpfCalculada;
        private TextBox txtReCalculado;
        private TextBox txtCuotaIvaCalculada;
        private GroupBox gboxArchivo;
        private TextBox txtRuta;
        private Label lblRuta;
        private Button btnAbrirArchivoPDF;
        private TextBox txtMensajes;
    }
}