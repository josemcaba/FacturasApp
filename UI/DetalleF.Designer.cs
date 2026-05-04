namespace FacturasApp.UI
{
    partial class DetalleF
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
            lblNumFactura = new Label();
            lblFecha = new Label();
            lblEstado = new Label();
            lblNifEmisor = new Label();
            lblNombreEmisor = new Label();
            gboxEmisor = new GroupBox();
            txtNombreEmisor = new TextBox();
            txtNifEmisor = new TextBox();
            gboxCliente = new GroupBox();
            txtNombreCliente = new TextBox();
            label1 = new Label();
            txtNifCliente = new TextBox();
            label2 = new Label();
            txtNumFactura = new TextBox();
            txtFecha = new TextBox();
            txtEstado = new TextBox();
            lblBaseImponible = new Label();
            gboxFactura = new GroupBox();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            gboxEmisor.SuspendLayout();
            gboxCliente.SuspendLayout();
            gboxFactura.SuspendLayout();
            SuspendLayout();
            // 
            // lblNumFactura
            // 
            lblNumFactura.AutoSize = true;
            lblNumFactura.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNumFactura.ForeColor = SystemColors.ControlText;
            lblNumFactura.Location = new Point(6, 41);
            lblNumFactura.Name = "lblNumFactura";
            lblNumFactura.Size = new Size(71, 20);
            lblNumFactura.TabIndex = 0;
            lblNumFactura.Text = "Número:";
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = true;
            lblFecha.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFecha.ForeColor = SystemColors.ControlText;
            lblFecha.Location = new Point(339, 41);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(53, 20);
            lblFecha.TabIndex = 1;
            lblFecha.Text = "Fecha:";
            // 
            // lblEstado
            // 
            lblEstado.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblEstado.ForeColor = SystemColors.ControlText;
            lblEstado.Location = new Point(684, 41);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(60, 20);
            lblEstado.TabIndex = 2;
            lblEstado.Text = "Estado:";
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
            // gboxEmisor
            // 
            gboxEmisor.Controls.Add(txtNombreEmisor);
            gboxEmisor.Controls.Add(txtNifEmisor);
            gboxEmisor.Controls.Add(lblNombreEmisor);
            gboxEmisor.Controls.Add(lblNifEmisor);
            gboxEmisor.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxEmisor.ForeColor = SystemColors.MenuHighlight;
            gboxEmisor.Location = new Point(12, 114);
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
            txtNombreEmisor.Size = new Size(336, 27);
            txtNombreEmisor.TabIndex = 10;
            txtNombreEmisor.Text = "8888888888000000000088888888880000000000\r\n";
            // 
            // txtNifEmisor
            // 
            txtNifEmisor.Location = new Point(83, 26);
            txtNifEmisor.Name = "txtNifEmisor";
            txtNifEmisor.Size = new Size(336, 31);
            txtNifEmisor.TabIndex = 10;
            // 
            // gboxCliente
            // 
            gboxCliente.Controls.Add(txtNombreCliente);
            gboxCliente.Controls.Add(label1);
            gboxCliente.Controls.Add(txtNifCliente);
            gboxCliente.Controls.Add(label2);
            gboxCliente.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxCliente.ForeColor = SystemColors.MenuHighlight;
            gboxCliente.Location = new Point(443, 114);
            gboxCliente.Name = "gboxCliente";
            gboxCliente.Size = new Size(425, 102);
            gboxCliente.TabIndex = 6;
            gboxCliente.TabStop = false;
            gboxCliente.Text = "Cliente";
            // 
            // txtNombreCliente
            // 
            txtNombreCliente.Location = new Point(83, 60);
            txtNombreCliente.Name = "txtNombreCliente";
            txtNombreCliente.Size = new Size(336, 31);
            txtNombreCliente.TabIndex = 11;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(6, 60);
            label1.Name = "label1";
            label1.Size = new Size(71, 20);
            label1.TabIndex = 4;
            label1.Text = "Nombre:";
            // 
            // txtNifCliente
            // 
            txtNifCliente.Location = new Point(83, 26);
            txtNifCliente.Name = "txtNifCliente";
            txtNifCliente.Size = new Size(336, 31);
            txtNifCliente.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.ForeColor = SystemColors.ControlText;
            label2.Location = new Point(6, 31);
            label2.Name = "label2";
            label2.Size = new Size(38, 20);
            label2.TabIndex = 3;
            label2.Text = "NIF:";
            // 
            // txtNumFactura
            // 
            txtNumFactura.Location = new Point(83, 38);
            txtNumFactura.Name = "txtNumFactura";
            txtNumFactura.Size = new Size(250, 31);
            txtNumFactura.TabIndex = 7;
            // 
            // txtFecha
            // 
            txtFecha.Font = new Font("Segoe UI", 9F);
            txtFecha.Location = new Point(398, 38);
            txtFecha.Name = "txtFecha";
            txtFecha.Size = new Size(100, 27);
            txtFecha.TabIndex = 8;
            txtFecha.Text = "04/05/2026";
            txtFecha.TextAlign = HorizontalAlignment.Center;
            // 
            // txtEstado
            // 
            txtEstado.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtEstado.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtEstado.Location = new Point(750, 38);
            txtEstado.Name = "txtEstado";
            txtEstado.Size = new Size(100, 27);
            txtEstado.TabIndex = 9;
            txtEstado.Text = "Pendiente";
            txtEstado.TextAlign = HorizontalAlignment.Center;
            // 
            // lblBaseImponible
            // 
            lblBaseImponible.AutoSize = true;
            lblBaseImponible.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBaseImponible.Location = new Point(11, 246);
            lblBaseImponible.Name = "lblBaseImponible";
            lblBaseImponible.Size = new Size(121, 20);
            lblBaseImponible.TabIndex = 10;
            lblBaseImponible.Text = "Base Imponible:";
            // 
            // gboxFactura
            // 
            gboxFactura.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gboxFactura.Controls.Add(txtNumFactura);
            gboxFactura.Controls.Add(txtEstado);
            gboxFactura.Controls.Add(lblNumFactura);
            gboxFactura.Controls.Add(txtFecha);
            gboxFactura.Controls.Add(lblFecha);
            gboxFactura.Controls.Add(lblEstado);
            gboxFactura.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gboxFactura.ForeColor = SystemColors.MenuHighlight;
            gboxFactura.Location = new Point(12, 12);
            gboxFactura.Name = "gboxFactura";
            gboxFactura.Size = new Size(856, 90);
            gboxFactura.TabIndex = 11;
            gboxFactura.TabStop = false;
            gboxFactura.Text = "FACTURA";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(12, 277);
            label3.Name = "label3";
            label3.Size = new Size(38, 20);
            label3.TabIndex = 12;
            label3.Text = "IVA:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(11, 307);
            label4.Name = "label4";
            label4.Size = new Size(45, 20);
            label4.TabIndex = 13;
            label4.Text = "IRPF:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label5.Location = new Point(11, 337);
            label5.Name = "label5";
            label5.Size = new Size(31, 20);
            label5.TabIndex = 14;
            label5.Text = "RE:";
            // 
            // DetalleF
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(879, 450);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(lblBaseImponible);
            Controls.Add(gboxCliente);
            Controls.Add(gboxEmisor);
            Controls.Add(gboxFactura);
            Name = "DetalleF";
            Text = "Form1";
            gboxEmisor.ResumeLayout(false);
            gboxEmisor.PerformLayout();
            gboxCliente.ResumeLayout(false);
            gboxCliente.PerformLayout();
            gboxFactura.ResumeLayout(false);
            gboxFactura.PerformLayout();
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
        private Label label1;
        private Label label2;
        private TextBox txtNombreEmisor;
        private TextBox txtNifEmisor;
        private TextBox txtNumFactura;
        private TextBox txtFecha;
        private TextBox txtEstado;
        private TextBox txtNombreCliente;
        private TextBox txtNifCliente;
        private Label lblBaseImponible;
        private GroupBox gboxFactura;
        private Label label3;
        private Label label4;
        private Label label5;
    }
}