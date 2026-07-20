namespace FacturasApp.UI
{
    partial class GestionProveedoresForm
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
            splitContainer = new SplitContainer();
            panelIzquierdo = new Panel();
            lstProveedores = new ListBox();
            flowIzq = new FlowLayoutPanel();
            btnAdd = new Button();
            btnClone = new Button();
            btnDelete = new Button();
            tabControl = new TabControl();
            tabGeneral = new TabPage();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblNif = new Label();
            txtNif = new TextBox();
            lblConcepto = new Label();
            txtConcepto = new TextBox();
            lblModo = new Label();
            cmbModo = new ComboBox();
            lblOmitirNif = new Label();
            chkOmitirNif = new CheckBox();
            lblIds = new Label();
            txtIdentificadores = new TextBox();
            tabCampos = new TabPage();
            dgvCampos = new DataGridView();
            colCampo = new DataGridViewComboBoxColumn();
            colRegex = new DataGridViewTextBoxColumn();
            colGrupo = new DataGridViewTextBoxColumn();
            colValorFijo = new DataGridViewTextBoxColumn();
            colCultura = new DataGridViewComboBoxColumn();
            colFormato = new DataGridViewTextBoxColumn();
            colOpcional = new DataGridViewCheckBoxColumn();
            tabAvanzado = new TabPage();
            lblPre = new Label();
            dgvPreprocesamiento = new DataGridView();
            colPreTipo = new DataGridViewComboBoxColumn();
            colPrePattern = new DataGridViewTextBoxColumn();
            colPreReemplazo = new DataGridViewTextBoxColumn();
            chkMultiIva = new CheckBox();
            lblLineaRegex = new Label();
            txtLineaRegex = new TextBox();
            lblMapa = new Label();
            txtMapa = new TextBox();
            chkDedup = new CheckBox();
            chkExcluirCero = new CheckBox();
            chkValidarSuma = new CheckBox();
            lblTotalRegex = new Label();
            txtTotalRegex = new TextBox();
            lblTotalGrupo = new Label();
            txtTotalGrupo = new TextBox();
            lblPost = new Label();
            dgvCondiciones = new DataGridView();
            colAccion = new DataGridViewComboBoxColumn();
            colCondCampo = new DataGridViewTextBoxColumn();
            colOperador = new DataGridViewComboBoxColumn();
            colCondValor = new DataGridViewTextBoxColumn();
            colParametro = new DataGridViewTextBoxColumn();
            btnTestRegex = new Button();
            btnTestIdent = new Button();
            btnGuardar = new Button();
            btnCancelar = new Button();
            flowBottom = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelIzquierdo.SuspendLayout();
            flowIzq.SuspendLayout();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCampos).BeginInit();
            tabAvanzado.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPreprocesamiento).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCondiciones).BeginInit();
            flowBottom.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(panelIzquierdo);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(tabControl);
            splitContainer.Size = new Size(1281, 500);
            splitContainer.SplitterDistance = 430;
            splitContainer.TabIndex = 0;
            // 
            // panelIzquierdo
            // 
            panelIzquierdo.Controls.Add(lstProveedores);
            panelIzquierdo.Controls.Add(flowIzq);
            panelIzquierdo.Dock = DockStyle.Fill;
            panelIzquierdo.Location = new Point(0, 0);
            panelIzquierdo.Name = "panelIzquierdo";
            panelIzquierdo.Size = new Size(430, 500);
            panelIzquierdo.TabIndex = 0;
            // 
            // lstProveedores
            // 
            lstProveedores.Dock = DockStyle.Fill;
            lstProveedores.Location = new Point(0, 0);
            lstProveedores.Name = "lstProveedores";
            lstProveedores.Size = new Size(430, 415);
            lstProveedores.Sorted = true;
            lstProveedores.TabIndex = 0;
            lstProveedores.SelectedIndexChanged += LstProveedores_SelectedIndexChanged;
            // 
            // flowIzq
            // 
            flowIzq.Controls.Add(btnAdd);
            flowIzq.Controls.Add(btnClone);
            flowIzq.Controls.Add(btnDelete);
            flowIzq.Dock = DockStyle.Bottom;
            flowIzq.Location = new Point(0, 415);
            flowIzq.Name = "flowIzq";
            flowIzq.Padding = new Padding(3);
            flowIzq.Size = new Size(430, 85);
            flowIzq.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(6, 6);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(120, 34);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "+ Añadir";
            btnAdd.Click += BtnAdd_Click;
            // 
            // btnClone
            // 
            btnClone.Location = new Point(132, 6);
            btnClone.Name = "btnClone";
            btnClone.Size = new Size(120, 34);
            btnClone.TabIndex = 1;
            btnClone.Text = "Clonar";
            btnClone.Click += BtnClone_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(258, 6);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(120, 34);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "- Eliminar";
            btnDelete.Click += BtnDelete_Click;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabCampos);
            tabControl.Controls.Add(tabAvanzado);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(847, 500);
            tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(lblNombre);
            tabGeneral.Controls.Add(txtNombre);
            tabGeneral.Controls.Add(lblNif);
            tabGeneral.Controls.Add(txtNif);
            tabGeneral.Controls.Add(lblConcepto);
            tabGeneral.Controls.Add(txtConcepto);
            tabGeneral.Controls.Add(lblModo);
            tabGeneral.Controls.Add(cmbModo);
            tabGeneral.Controls.Add(lblOmitirNif);
            tabGeneral.Controls.Add(chkOmitirNif);
            tabGeneral.Controls.Add(lblIds);
            tabGeneral.Controls.Add(txtIdentificadores);
            tabGeneral.Location = new Point(4, 29);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Size = new Size(839, 467);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            // 
            // lblNombre
            // 
            lblNombre.Location = new Point(12, 15);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(80, 23);
            lblNombre.TabIndex = 0;
            lblNombre.Text = "Nombre:";
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(100, 12);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(300, 27);
            txtNombre.TabIndex = 1;
            // 
            // lblNif
            // 
            lblNif.Location = new Point(12, 45);
            lblNif.Name = "lblNif";
            lblNif.Size = new Size(80, 23);
            lblNif.TabIndex = 2;
            lblNif.Text = "NIF:";
            // 
            // txtNif
            // 
            txtNif.Location = new Point(100, 42);
            txtNif.Name = "txtNif";
            txtNif.Size = new Size(180, 27);
            txtNif.TabIndex = 3;
            // 
            // lblConcepto
            // 
            lblConcepto.Location = new Point(12, 75);
            lblConcepto.Name = "lblConcepto";
            lblConcepto.Size = new Size(80, 23);
            lblConcepto.TabIndex = 4;
            lblConcepto.Text = "Concepto:";
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(100, 72);
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(80, 27);
            txtConcepto.TabIndex = 5;
            txtConcepto.Text = "600";
            // 
            // lblModo
            // 
            lblModo.Location = new Point(12, 105);
            lblModo.Name = "lblModo";
            lblModo.Size = new Size(120, 23);
            lblModo.TabIndex = 6;
            lblModo.Text = "Modo Extracción:";
            // 
            // cmbModo
            // 
            cmbModo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModo.Items.AddRange(new object[] { "OrdenadoPosicion", "Simple", "LayoutAnalysis" });
            cmbModo.Location = new Point(130, 102);
            cmbModo.Name = "cmbModo";
            cmbModo.Size = new Size(180, 28);
            cmbModo.TabIndex = 7;
            // 
            // lblOmitirNif
            // 
            lblOmitirNif.Location = new Point(12, 135);
            lblOmitirNif.Name = "lblOmitirNif";
            lblOmitirNif.Size = new Size(120, 23);
            lblOmitirNif.TabIndex = 8;
            lblOmitirNif.Text = "Omitir NIF emisor:";
            // 
            // chkOmitirNif
            // 
            chkOmitirNif.Checked = true;
            chkOmitirNif.CheckState = CheckState.Checked;
            chkOmitirNif.Location = new Point(130, 132);
            chkOmitirNif.Name = "chkOmitirNif";
            chkOmitirNif.Size = new Size(20, 24);
            chkOmitirNif.TabIndex = 9;
            // 
            // lblIds
            // 
            lblIds.Location = new Point(12, 165);
            lblIds.Name = "lblIds";
            lblIds.Size = new Size(220, 23);
            lblIds.TabIndex = 10;
            lblIds.Text = "Identificadores (uno por línea)";
            // 
            // txtIdentificadores
            // 
            txtIdentificadores.AcceptsReturn = true;
            txtIdentificadores.Location = new Point(12, 188);
            txtIdentificadores.Multiline = true;
            txtIdentificadores.Name = "txtIdentificadores";
            txtIdentificadores.ScrollBars = ScrollBars.Vertical;
            txtIdentificadores.Size = new Size(400, 80);
            txtIdentificadores.TabIndex = 11;
            // 
            // tabCampos
            // 
            tabCampos.Controls.Add(dgvCampos);
            tabCampos.Location = new Point(4, 29);
            tabCampos.Name = "tabCampos";
            tabCampos.Size = new Size(839, 467);
            tabCampos.TabIndex = 1;
            tabCampos.Text = "Campos";
            // 
            // dgvCampos
            // 
            dgvCampos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCampos.ColumnHeadersHeight = 29;
            dgvCampos.Columns.AddRange(new DataGridViewColumn[] { colCampo, colRegex, colGrupo, colValorFijo, colCultura, colFormato, colOpcional });
            dgvCampos.Location = new Point(5, 5);
            dgvCampos.Name = "dgvCampos";
            dgvCampos.RowHeadersVisible = false;
            dgvCampos.RowHeadersWidth = 51;
            dgvCampos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCampos.Size = new Size(839, 467);
            dgvCampos.TabIndex = 0;
            dgvCampos.DefaultValuesNeeded += DgvCampos_DefaultValuesNeeded;
            // 
            // colCampo
            // 
            colCampo.FlatStyle = FlatStyle.Flat;
            colCampo.HeaderText = "Campo";
            colCampo.Items.AddRange(new object[] { "NumeroFactura", "Fecha", "ClienteNombre", "ClienteNif", "BaseImponible", "PorcentajeIVA", "CuotaIVA", "PorcentajeIRPF", "CuotaIRPF", "PorcentajeRE", "CuotaRE", "Total", "Descuento", "TotalParcial" });
            colCampo.MinimumWidth = 6;
            colCampo.Name = "colCampo";
            colCampo.Width = 125;
            // 
            // colRegex
            // 
            colRegex.HeaderText = "Regex";
            colRegex.MinimumWidth = 6;
            colRegex.Name = "colRegex";
            colRegex.Width = 200;
            // 
            // colGrupo
            // 
            colGrupo.HeaderText = "Grupo";
            colGrupo.MinimumWidth = 6;
            colGrupo.Name = "colGrupo";
            colGrupo.Width = 50;
            // 
            // colValorFijo
            // 
            colValorFijo.HeaderText = "Valor Fijo";
            colValorFijo.MinimumWidth = 6;
            colValorFijo.Name = "colValorFijo";
            colValorFijo.Width = 125;
            // 
            // colCultura
            // 
            colCultura.FlatStyle = FlatStyle.Flat;
            colCultura.HeaderText = "Cultura";
            colCultura.Items.AddRange(new object[] { "es-ES", "en-US", "en-GB", "de-DE", "fr-FR", "it-IT", "pt-PT" });
            colCultura.MinimumWidth = 6;
            colCultura.Name = "colCultura";
            colCultura.Width = 90;
            // 
            // colFormato
            // 
            colFormato.HeaderText = "Formato";
            colFormato.MinimumWidth = 6;
            colFormato.Name = "colFormato";
            colFormato.Width = 90;
            // 
            // colOpcional
            // 
            colOpcional.FlatStyle = FlatStyle.Flat;
            colOpcional.HeaderText = "Opcional";
            colOpcional.MinimumWidth = 6;
            colOpcional.Name = "colOpcional";
            colOpcional.Resizable = DataGridViewTriState.True;
            colOpcional.SortMode = DataGridViewColumnSortMode.Automatic;
            colOpcional.Width = 60;
            // 
            // tabAvanzado
            // 
            tabAvanzado.Controls.Add(lblPre);
            tabAvanzado.Controls.Add(dgvPreprocesamiento);
            tabAvanzado.Controls.Add(chkMultiIva);
            tabAvanzado.Controls.Add(lblLineaRegex);
            tabAvanzado.Controls.Add(txtLineaRegex);
            tabAvanzado.Controls.Add(lblMapa);
            tabAvanzado.Controls.Add(txtMapa);
            tabAvanzado.Controls.Add(chkDedup);
            tabAvanzado.Controls.Add(chkExcluirCero);
            tabAvanzado.Controls.Add(chkValidarSuma);
            tabAvanzado.Controls.Add(lblTotalRegex);
            tabAvanzado.Controls.Add(txtTotalRegex);
            tabAvanzado.Controls.Add(lblTotalGrupo);
            tabAvanzado.Controls.Add(txtTotalGrupo);
            tabAvanzado.Controls.Add(lblPost);
            tabAvanzado.Controls.Add(dgvCondiciones);
            tabAvanzado.Location = new Point(4, 29);
            tabAvanzado.Name = "tabAvanzado";
            tabAvanzado.Size = new Size(839, 467);
            tabAvanzado.TabIndex = 2;
            tabAvanzado.Text = "Avanzado";
            // 
            // lblPre
            // 
            lblPre.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPre.Location = new Point(12, 10);
            lblPre.Name = "lblPre";
            lblPre.Size = new Size(200, 23);
            lblPre.TabIndex = 0;
            lblPre.Text = "Preprocesamiento";
            // 
            // dgvPreprocesamiento
            // 
            dgvPreprocesamiento.ColumnHeadersHeight = 29;
            dgvPreprocesamiento.Columns.AddRange(new DataGridViewColumn[] { colPreTipo, colPrePattern, colPreReemplazo });
            dgvPreprocesamiento.Location = new Point(12, 30);
            dgvPreprocesamiento.Name = "dgvPreprocesamiento";
            dgvPreprocesamiento.RowHeadersVisible = false;
            dgvPreprocesamiento.RowHeadersWidth = 51;
            dgvPreprocesamiento.Size = new Size(818, 120);
            dgvPreprocesamiento.TabIndex = 1;
            dgvPreprocesamiento.DefaultValuesNeeded += DgvPreprocesamiento_DefaultValuesNeeded;
            // 
            // colPreTipo
            // 
            colPreTipo.FlatStyle = FlatStyle.Flat;
            colPreTipo.HeaderText = "Tipo";
            colPreTipo.Items.AddRange(new object[] { "Reemplazar", "EliminarDuplicados" });
            colPreTipo.MinimumWidth = 6;
            colPreTipo.Name = "colPreTipo";
            colPreTipo.Width = 130;
            // 
            // colPrePattern
            // 
            colPrePattern.HeaderText = "Pattern";
            colPrePattern.MinimumWidth = 6;
            colPrePattern.Name = "colPrePattern";
            colPrePattern.Width = 200;
            // 
            // colPreReemplazo
            // 
            colPreReemplazo.HeaderText = "Reemplazo";
            colPreReemplazo.MinimumWidth = 6;
            colPreReemplazo.Name = "colPreReemplazo";
            colPreReemplazo.Width = 200;
            // 
            // chkMultiIva
            // 
            chkMultiIva.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkMultiIva.Location = new Point(12, 160);
            chkMultiIva.Name = "chkMultiIva";
            chkMultiIva.Size = new Size(150, 24);
            chkMultiIva.TabIndex = 2;
            chkMultiIva.Text = "Multi línea IVA";
            chkMultiIva.CheckedChanged += ChkMultiIva_CheckedChanged;
            // 
            // lblLineaRegex
            // 
            lblLineaRegex.Location = new Point(24, 185);
            lblLineaRegex.Name = "lblLineaRegex";
            lblLineaRegex.Size = new Size(80, 23);
            lblLineaRegex.TabIndex = 3;
            lblLineaRegex.Text = "Linea Regex:";
            // 
            // txtLineaRegex
            // 
            txtLineaRegex.Enabled = false;
            txtLineaRegex.Location = new Point(110, 182);
            txtLineaRegex.Name = "txtLineaRegex";
            txtLineaRegex.Size = new Size(370, 27);
            txtLineaRegex.TabIndex = 4;
            // 
            // lblMapa
            // 
            lblMapa.Location = new Point(24, 212);
            lblMapa.Name = "lblMapa";
            lblMapa.Size = new Size(80, 23);
            lblMapa.TabIndex = 5;
            lblMapa.Text = "Mapa:";
            // 
            // txtMapa
            // 
            txtMapa.Enabled = false;
            txtMapa.Location = new Point(110, 209);
            txtMapa.Name = "txtMapa";
            txtMapa.Size = new Size(370, 27);
            txtMapa.TabIndex = 6;
            // 
            // chkDedup
            // 
            chkDedup.Enabled = false;
            chkDedup.Location = new Point(110, 236);
            chkDedup.Name = "chkDedup";
            chkDedup.Size = new Size(100, 24);
            chkDedup.TabIndex = 7;
            chkDedup.Text = "Deduplicar";
            // 
            // chkExcluirCero
            // 
            chkExcluirCero.Enabled = false;
            chkExcluirCero.Location = new Point(220, 236);
            chkExcluirCero.Name = "chkExcluirCero";
            chkExcluirCero.Size = new Size(130, 24);
            chkExcluirCero.TabIndex = 8;
            chkExcluirCero.Text = "Excluir base cero";
            // 
            // chkValidarSuma
            // 
            chkValidarSuma.Enabled = false;
            chkValidarSuma.Location = new Point(360, 236);
            chkValidarSuma.Name = "chkValidarSuma";
            chkValidarSuma.Size = new Size(160, 24);
            chkValidarSuma.TabIndex = 9;
            chkValidarSuma.Text = "Validar suma subtotales";
            // 
            // lblTotalRegex
            // 
            lblTotalRegex.Location = new Point(24, 262);
            lblTotalRegex.Name = "lblTotalRegex";
            lblTotalRegex.Size = new Size(80, 23);
            lblTotalRegex.TabIndex = 10;
            lblTotalRegex.Text = "Total Regex:";
            // 
            // txtTotalRegex
            // 
            txtTotalRegex.Enabled = false;
            txtTotalRegex.Location = new Point(110, 259);
            txtTotalRegex.Name = "txtTotalRegex";
            txtTotalRegex.Size = new Size(290, 27);
            txtTotalRegex.TabIndex = 11;
            // 
            // lblTotalGrupo
            // 
            lblTotalGrupo.Location = new Point(410, 262);
            lblTotalGrupo.Name = "lblTotalGrupo";
            lblTotalGrupo.Size = new Size(45, 23);
            lblTotalGrupo.TabIndex = 12;
            lblTotalGrupo.Text = "Grupo:";
            // 
            // txtTotalGrupo
            // 
            txtTotalGrupo.Enabled = false;
            txtTotalGrupo.Location = new Point(455, 259);
            txtTotalGrupo.Name = "txtTotalGrupo";
            txtTotalGrupo.Size = new Size(30, 27);
            txtTotalGrupo.TabIndex = 13;
            txtTotalGrupo.Text = "1";
            // 
            // lblPost
            // 
            lblPost.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPost.Location = new Point(12, 290);
            lblPost.Name = "lblPost";
            lblPost.Size = new Size(220, 23);
            lblPost.TabIndex = 14;
            lblPost.Text = "Postprocesamiento (condiciones)";
            // 
            // dgvCondiciones
            // 
            dgvCondiciones.ColumnHeadersHeight = 29;
            dgvCondiciones.Columns.AddRange(new DataGridViewColumn[] { colAccion, colCondCampo, colOperador, colCondValor, colParametro });
            dgvCondiciones.Location = new Point(12, 310);
            dgvCondiciones.Name = "dgvCondiciones";
            dgvCondiciones.RowHeadersVisible = false;
            dgvCondiciones.RowHeadersWidth = 51;
            dgvCondiciones.Size = new Size(818, 130);
            dgvCondiciones.TabIndex = 15;
            dgvCondiciones.DefaultValuesNeeded += DgvCondiciones_DefaultValuesNeeded;
            // 
            // colAccion
            // 
            colAccion.FlatStyle = FlatStyle.Flat;
            colAccion.HeaderText = "Acción";
            colAccion.Items.AddRange(new object[] { "Condicion", "Mover", "Asignar", "Copiar", "Sumar" });
            colAccion.MinimumWidth = 6;
            colAccion.Name = "colAccion";
            colAccion.Width = 70;
            // 
            // colCondCampo
            // 
            colCondCampo.HeaderText = "Campo";
            colCondCampo.MinimumWidth = 6;
            colCondCampo.Name = "colCondCampo";
            colCondCampo.Width = 70;
            // 
            // colOperador
            // 
            colOperador.FlatStyle = FlatStyle.Flat;
            colOperador.HeaderText = "Operador";
            colOperador.Items.AddRange(new object[] { "Igual", "Distinto", "MayorQue", "MenorQue", "MayorOIgual", "MenorOIgual" });
            colOperador.MinimumWidth = 6;
            colOperador.Name = "colOperador";
            colOperador.Width = 80;
            // 
            // colCondValor
            // 
            colCondValor.HeaderText = "Valor";
            colCondValor.MinimumWidth = 6;
            colCondValor.Name = "colCondValor";
            colCondValor.Width = 80;
            // 
            // colParametro
            // 
            colParametro.HeaderText = "Parámetro extra";
            colParametro.MinimumWidth = 6;
            colParametro.Name = "colParametro";
            colParametro.Width = 80;
            // 
            // btnTestRegex
            // 
            btnTestRegex.BackColor = Color.FromArgb(46, 117, 182);
            btnTestRegex.FlatStyle = FlatStyle.Flat;
            btnTestRegex.ForeColor = Color.White;
            btnTestRegex.Location = new Point(710, 8);
            btnTestRegex.Name = "btnTestRegex";
            btnTestRegex.Size = new Size(140, 34);
            btnTestRegex.TabIndex = 3;
            btnTestRegex.Text = "\U0001f9ea Probar Regex";
            btnTestRegex.UseVisualStyleBackColor = false;
            btnTestRegex.Click += BtnTestRegex_Click;
            // 
            // btnTestIdent
            // 
            btnTestIdent.BackColor = Color.FromArgb(46, 117, 182);
            btnTestIdent.FlatStyle = FlatStyle.Flat;
            btnTestIdent.ForeColor = Color.White;
            btnTestIdent.Location = new Point(856, 8);
            btnTestIdent.Name = "btnTestIdent";
            btnTestIdent.Size = new Size(160, 34);
            btnTestIdent.TabIndex = 2;
            btnTestIdent.Text = "🔍 Probar Identificación";
            btnTestIdent.UseVisualStyleBackColor = false;
            btnTestIdent.Click += BtnTestIdent_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.DarkGreen;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(1022, 8);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(120, 34);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text = "💾 Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.DimGray;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Location = new Point(1148, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(120, 34);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // flowBottom
            // 
            flowBottom.BackColor = Color.FromArgb(240, 240, 240);
            flowBottom.Controls.Add(btnCancelar);
            flowBottom.Controls.Add(btnGuardar);
            flowBottom.Controls.Add(btnTestIdent);
            flowBottom.Controls.Add(btnTestRegex);
            flowBottom.Dock = DockStyle.Bottom;
            flowBottom.FlowDirection = FlowDirection.RightToLeft;
            flowBottom.Location = new Point(0, 500);
            flowBottom.Name = "flowBottom";
            flowBottom.Padding = new Padding(5);
            flowBottom.Size = new Size(1281, 92);
            flowBottom.TabIndex = 1;
            // 
            // GestionProveedoresForm
            // 
            ClientSize = new Size(1281, 592);
            Controls.Add(splitContainer);
            Controls.Add(flowBottom);
            MinimumSize = new Size(800, 600);
            Name = "GestionProveedoresForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestión de Proveedores";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelIzquierdo.ResumeLayout(false);
            flowIzq.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            tabCampos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCampos).EndInit();
            tabAvanzado.ResumeLayout(false);
            tabAvanzado.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPreprocesamiento).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCondiciones).EndInit();
            flowBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        // ── Controles ─────────────────────────────────────────────────
        private SplitContainer splitContainer;
        private ListBox lstProveedores;
        private Button btnAdd, btnClone, btnDelete;

        private TabControl tabControl;
        private TextBox txtNombre, txtNif, txtConcepto, txtIdentificadores;
        private ComboBox cmbModo;
        private CheckBox chkOmitirNif;

        private DataGridView dgvCampos;
        private DataGridView dgvPreprocesamiento;

        private CheckBox chkMultiIva;
        private TextBox txtLineaRegex, txtMapa, txtTotalRegex, txtTotalGrupo;
        private CheckBox chkDedup, chkExcluirCero, chkValidarSuma;

        private DataGridView dgvCondiciones;

        private Button btnTestRegex, btnTestIdent, btnGuardar, btnCancelar;

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Panel panelIzquierdo;
        private FlowLayoutPanel flowIzq;
        private TabPage tabGeneral;
        private Label lblNombre;
        private Label lblNif;
        private Label lblConcepto;
        private Label lblModo;
        private Label lblOmitirNif;
        private Label lblIds;
        private TabPage tabCampos;
        private TabPage tabAvanzado;
        private Label lblPre;
        private Label lblLineaRegex;
        private Label lblMapa;
        private Label lblTotalRegex;
        private Label lblTotalGrupo;
        private Label lblPost;
        private DataGridViewComboBoxColumn colCampo;
        private DataGridViewTextBoxColumn colRegex;
        private DataGridViewTextBoxColumn colGrupo;
        private DataGridViewTextBoxColumn colValorFijo;
        private DataGridViewComboBoxColumn colCultura;
        private DataGridViewTextBoxColumn colFormato;
        private DataGridViewCheckBoxColumn colOpcional;
        private DataGridViewComboBoxColumn colPreTipo;
        private DataGridViewTextBoxColumn colPrePattern;
        private DataGridViewTextBoxColumn colPreReemplazo;
        private DataGridViewComboBoxColumn colAccion;
        private DataGridViewComboBoxColumn colOperador;
        private DataGridViewTextBoxColumn colParametro;
        private DataGridViewTextBoxColumn colCondCampo;
        private DataGridViewTextBoxColumn colCondValor;
        private FlowLayoutPanel flowBottom;
    }
}
