namespace FacturasApp.UI;

partial class GestionEmisoresForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.ListBox lstEmisores;
    private System.Windows.Forms.TextBox txtBuscarEmisor;
    private System.Windows.Forms.Button btnNuevo;
    private System.Windows.Forms.Button btnEliminar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GestionEmisoresForm));
        lstEmisores = new ListBox();
        panelIzquierdo = new Panel();
        txtBuscarEmisor = new TextBox();
        btnEliminar = new Button();
        btnClonar = new Button();
        btnNuevo = new Button();
        tabs = new TabControl();
        tabGeneral = new TabPage();
        lblGeneralNombre = new Label();
        txtNombre = new TextBox();
        lblGeneralNif = new Label();
        txtNif = new TextBox();
        lblGeneralIds = new Label();
        lstIdentificadores = new ListBox();
        txtNuevoId = new TextBox();
        btnAddId = new Button();
        btnRemoveId = new Button();
        lblGeneralCultura = new Label();
        cmbCulturaFecha = new ComboBox();
        lblGeneralConceptoIngreso = new Label();
        txtConceptoIngreso = new TextBox();
        lblGeneralConceptoGasto = new Label();
        txtConceptoGasto = new TextBox();
        lblZonasTitulo = new Label();
        dgvZonas = new DataGridView();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
        tabCampos = new TabPage();
        lblGeneralModo = new Label();
        cmbModoExtraccion = new ComboBox();
        lblCamposLista = new Label();
        lstCampos = new ListBox();
        panelDetalle = new Panel();
        lblCampoNombre = new Label();
        cmbCampoNombre = new ComboBox();
        lblCampoTipo = new Label();
        cmbCampoTipo = new ComboBox();
        lblCampoRegex = new Label();
        txtCampoRegex = new TextBox();
        lblCampoValorFijo = new Label();
        txtCampoValorFijo = new TextBox();
        lblCampoFormato = new Label();
        txtCampoFormatoFecha = new TextBox();
        lblCampoSuma = new Label();
        txtCampoCamposSuma = new TextBox();
        btnCampoAdd = new Button();
        btnCampoRemove = new Button();
        lblRegexPattern = new Label();
        txtRegexPattern = new TextBox();
        lblRegexMatchCount = new Label();
        lblRegexResultados = new Label();
        dgvRegexMatches = new DataGridView();
        btnRegexApplyToField = new Button();
        tabMultiLinea = new TabPage();
        lblMultiLineas = new Label();
        lstMultiLineas = new ListBox();
        btnMultiLineaAdd = new Button();
        btnMultiLineaRemove = new Button();
        btnMultiLineaUp = new Button();
        btnMultiLineaDown = new Button();
        lblMultiLineaRegex = new Label();
        txtMultiLineaRegex = new TextBox();
        lblMultiLineaMapeo = new Label();
        dgvMultiLineaMapeo = new DataGridView();
        dataGridViewComboBoxColumn1 = new DataGridViewComboBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        lblSeparadorPostProc = new Label();
        lstPostProc = new ListBox();
        btnPostProcAdd = new Button();
        btnPostProcRemove = new Button();
        btnPostProcUp = new Button();
        btnPostProcDown = new Button();
        lblPostProcResumen = new Label();
        lblPostProcTipo = new Label();
        cmbPostProcTipo = new ComboBox();
        lblPostProcCond = new Label();
        txtPostProcCondicion = new TextBox();
        lblPostCondCampo = new Label();
        cmbPostCondCampo = new ComboBox();
        lblPostCondValor = new Label();
        txtPostCondValor = new TextBox();
        lblPostAccDestino = new Label();
        cmbPostAccDestino = new ComboBox();
        txtPostAccValor = new TextBox();
        lblDestinoA = new Label();
        cmbPostAccOrigen1 = new ComboBox();
        cmbPostAccOperador = new ComboBox();
        cmbPostAccOrigen2 = new ComboBox();
        lblSeparadorRegex = new Label();
        txtRegexSource = new TextBox();
        btnGuardar = new Button();
        btnCargarPdfMuestra = new Button();
        tabPaginas = new TabControl();
        picFactura = new PictureBox();
        panelDerecho = new Panel();
        panelCentral = new Panel();
        panelIzquierdo.SuspendLayout();
        tabs.SuspendLayout();
        tabGeneral.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvZonas).BeginInit();
        tabCampos.SuspendLayout();
        panelDetalle.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRegexMatches).BeginInit();
        tabMultiLinea.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMultiLineaMapeo).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picFactura).BeginInit();
        panelDerecho.SuspendLayout();
        panelCentral.SuspendLayout();
        SuspendLayout();
        // 
        // lstEmisores
        // 
        lstEmisores.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstEmisores.BorderStyle = BorderStyle.None;
        lstEmisores.DisplayMember = "DisplayText";
        lstEmisores.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        lstEmisores.Location = new Point(11, 30);
        lstEmisores.Name = "lstEmisores";
        lstEmisores.Size = new Size(324, 630);
        lstEmisores.TabIndex = 2;
        lstEmisores.SelectedIndexChanged += LstEmisores_SelectedIndexChanged;
        // 
        // panelIzquierdo
        // 
        panelIzquierdo.Controls.Add(txtBuscarEmisor);
        panelIzquierdo.Controls.Add(lstEmisores);
        panelIzquierdo.Controls.Add(btnEliminar);
        panelIzquierdo.Controls.Add(btnClonar);
        panelIzquierdo.Controls.Add(btnNuevo);
        panelIzquierdo.Dock = DockStyle.Left;
        panelIzquierdo.Location = new Point(0, 0);
        panelIzquierdo.Name = "panelIzquierdo";
        panelIzquierdo.Padding = new Padding(8);
        panelIzquierdo.Size = new Size(346, 711);
        panelIzquierdo.TabIndex = 1;
        // 
        // txtBuscarEmisor
        // 
        txtBuscarEmisor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtBuscarEmisor.Location = new Point(11, 0);
        txtBuscarEmisor.Name = "txtBuscarEmisor";
        txtBuscarEmisor.PlaceholderText = "Buscar por nombre o NIF...";
        txtBuscarEmisor.ShortcutsEnabled = false;
        txtBuscarEmisor.Size = new Size(324, 27);
        txtBuscarEmisor.TabIndex = 1;
        txtBuscarEmisor.TextChanged += TxtBuscarEmisor_TextChanged;
        // 
        // btnEliminar
        // 
        btnEliminar.Anchor = AnchorStyles.Bottom;
        btnEliminar.FlatStyle = FlatStyle.Flat;
        btnEliminar.Location = new Point(121, 673);
        btnEliminar.Name = "btnEliminar";
        btnEliminar.Size = new Size(104, 30);
        btnEliminar.TabIndex = 4;
        btnEliminar.Text = "− Eliminar";
        btnEliminar.UseVisualStyleBackColor = true;
        btnEliminar.Click += BtnEliminar_Click;
        // 
        // btnClonar
        // 
        btnClonar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClonar.FlatStyle = FlatStyle.Flat;
        btnClonar.Location = new Point(231, 673);
        btnClonar.Name = "btnClonar";
        btnClonar.Size = new Size(104, 30);
        btnClonar.TabIndex = 5;
        btnClonar.Text = "◎ Clonar";
        btnClonar.UseVisualStyleBackColor = true;
        btnClonar.Click += BtnClonar_Click;
        // 
        // btnNuevo
        // 
        btnNuevo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnNuevo.FlatStyle = FlatStyle.Flat;
        btnNuevo.Location = new Point(11, 673);
        btnNuevo.Name = "btnNuevo";
        btnNuevo.Size = new Size(104, 30);
        btnNuevo.TabIndex = 3;
        btnNuevo.Text = "+ Nuevo";
        btnNuevo.UseVisualStyleBackColor = true;
        btnNuevo.Click += BtnNuevo_Click;
        // 
        // tabs
        // 
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tabs.Controls.Add(tabGeneral);
        tabs.Controls.Add(tabCampos);
        tabs.Controls.Add(tabMultiLinea);
        tabs.Location = new Point(6, 3);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(753, 564);
        tabs.TabIndex = 0;
        // 
        // tabGeneral
        // 
        tabGeneral.Controls.Add(lblGeneralNombre);
        tabGeneral.Controls.Add(txtNombre);
        tabGeneral.Controls.Add(lblGeneralNif);
        tabGeneral.Controls.Add(txtNif);
        tabGeneral.Controls.Add(lblGeneralIds);
        tabGeneral.Controls.Add(lstIdentificadores);
        tabGeneral.Controls.Add(txtNuevoId);
        tabGeneral.Controls.Add(btnAddId);
        tabGeneral.Controls.Add(btnRemoveId);
        tabGeneral.Controls.Add(lblGeneralCultura);
        tabGeneral.Controls.Add(cmbCulturaFecha);
        tabGeneral.Controls.Add(lblGeneralConceptoIngreso);
        tabGeneral.Controls.Add(txtConceptoIngreso);
        tabGeneral.Controls.Add(lblGeneralConceptoGasto);
        tabGeneral.Controls.Add(txtConceptoGasto);
        tabGeneral.Controls.Add(lblZonasTitulo);
        tabGeneral.Controls.Add(dgvZonas);
        tabGeneral.Location = new Point(4, 29);
        tabGeneral.Name = "tabGeneral";
        tabGeneral.Size = new Size(745, 531);
        tabGeneral.TabIndex = 0;
        tabGeneral.Text = "General";
        // 
        // lblGeneralNombre
        // 
        lblGeneralNombre.AutoSize = true;
        lblGeneralNombre.Location = new Point(16, 16);
        lblGeneralNombre.Name = "lblGeneralNombre";
        lblGeneralNombre.Size = new Size(141, 20);
        lblGeneralNombre.TabIndex = 0;
        lblGeneralNombre.Text = "Nombre del emisor:";
        // 
        // txtNombre
        // 
        txtNombre.Location = new Point(16, 39);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(400, 27);
        txtNombre.TabIndex = 1;
        txtNombre.TextChanged += ControlModificado;
        // 
        // lblGeneralNif
        // 
        lblGeneralNif.AutoSize = true;
        lblGeneralNif.Location = new Point(422, 16);
        lblGeneralNif.Name = "lblGeneralNif";
        lblGeneralNif.Size = new Size(34, 20);
        lblGeneralNif.TabIndex = 2;
        lblGeneralNif.Text = "NIF:";
        // 
        // txtNif
        // 
        txtNif.Location = new Point(422, 38);
        txtNif.Name = "txtNif";
        txtNif.Size = new Size(200, 27);
        txtNif.TabIndex = 2;
        txtNif.TextChanged += ControlModificado;
        // 
        // lblGeneralIds
        // 
        lblGeneralIds.AutoSize = true;
        lblGeneralIds.Location = new Point(16, 84);
        lblGeneralIds.Name = "lblGeneralIds";
        lblGeneralIds.Size = new Size(323, 20);
        lblGeneralIds.TabIndex = 4;
        lblGeneralIds.Text = "Identificadores (textos para detectar el emisor):";
        // 
        // lstIdentificadores
        // 
        lstIdentificadores.Location = new Point(16, 143);
        lstIdentificadores.Name = "lstIdentificadores";
        lstIdentificadores.Size = new Size(400, 84);
        lstIdentificadores.TabIndex = 5;
        // 
        // txtNuevoId
        // 
        txtNuevoId.Location = new Point(16, 107);
        txtNuevoId.Name = "txtNuevoId";
        txtNuevoId.Size = new Size(316, 27);
        txtNuevoId.TabIndex = 3;
        // 
        // btnAddId
        // 
        btnAddId.FlatStyle = FlatStyle.Flat;
        btnAddId.Location = new Point(338, 107);
        btnAddId.Name = "btnAddId";
        btnAddId.Size = new Size(36, 30);
        btnAddId.TabIndex = 7;
        btnAddId.Text = "+";
        btnAddId.UseVisualStyleBackColor = true;
        btnAddId.Click += BtnAddId_Click;
        // 
        // btnRemoveId
        // 
        btnRemoveId.FlatStyle = FlatStyle.Flat;
        btnRemoveId.Location = new Point(380, 107);
        btnRemoveId.Name = "btnRemoveId";
        btnRemoveId.Size = new Size(36, 30);
        btnRemoveId.TabIndex = 8;
        btnRemoveId.Text = "−";
        btnRemoveId.UseVisualStyleBackColor = true;
        btnRemoveId.Click += BtnRemoveId_Click;
        // 
        // lblGeneralCultura
        // 
        lblGeneralCultura.AutoSize = true;
        lblGeneralCultura.Location = new Point(422, 136);
        lblGeneralCultura.Name = "lblGeneralCultura";
        lblGeneralCultura.Size = new Size(139, 20);
        lblGeneralCultura.TabIndex = 11;
        lblGeneralCultura.Text = "Cultura para fechas:";
        // 
        // cmbCulturaFecha
        // 
        cmbCulturaFecha.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCulturaFecha.Items.AddRange(new object[] { "es-ES", "en-US" });
        cmbCulturaFecha.Location = new Point(573, 133);
        cmbCulturaFecha.Name = "cmbCulturaFecha";
        cmbCulturaFecha.Size = new Size(96, 28);
        cmbCulturaFecha.TabIndex = 12;
        cmbCulturaFecha.SelectedIndexChanged += ControlModificado;
        // 
        // lblGeneralConceptoIngreso
        // 
        lblGeneralConceptoIngreso.AutoSize = true;
        lblGeneralConceptoIngreso.Location = new Point(422, 170);
        lblGeneralConceptoIngreso.Name = "lblGeneralConceptoIngreso";
        lblGeneralConceptoIngreso.Size = new Size(129, 20);
        lblGeneralConceptoIngreso.TabIndex = 13;
        lblGeneralConceptoIngreso.Text = "Concepto ingreso:";
        // 
        // txtConceptoIngreso
        // 
        txtConceptoIngreso.Location = new Point(573, 167);
        txtConceptoIngreso.Name = "txtConceptoIngreso";
        txtConceptoIngreso.Size = new Size(96, 27);
        txtConceptoIngreso.TabIndex = 14;
        txtConceptoIngreso.TextChanged += ControlModificado;
        // 
        // lblGeneralConceptoGasto
        // 
        lblGeneralConceptoGasto.AutoSize = true;
        lblGeneralConceptoGasto.Location = new Point(422, 203);
        lblGeneralConceptoGasto.Name = "lblGeneralConceptoGasto";
        lblGeneralConceptoGasto.Size = new Size(117, 20);
        lblGeneralConceptoGasto.TabIndex = 15;
        lblGeneralConceptoGasto.Text = "Concepto gasto:";
        // 
        // txtConceptoGasto
        // 
        txtConceptoGasto.Location = new Point(573, 200);
        txtConceptoGasto.Name = "txtConceptoGasto";
        txtConceptoGasto.Size = new Size(96, 27);
        txtConceptoGasto.TabIndex = 16;
        txtConceptoGasto.TextChanged += ControlModificado;
        // 
        // lblZonasTitulo
        // 
        lblZonasTitulo.AutoSize = true;
        lblZonasTitulo.Location = new Point(16, 249);
        lblZonasTitulo.Name = "lblZonasTitulo";
        lblZonasTitulo.Size = new Size(85, 20);
        lblZonasTitulo.TabIndex = 0;
        lblZonasTitulo.Text = "Zonas OCR:";
        // 
        // dgvZonas
        // 
        dgvZonas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvZonas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dgvZonas.BackgroundColor = Color.White;
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle1.BackColor = SystemColors.Control;
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
        dgvZonas.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        dgvZonas.ColumnHeadersHeight = 29;
        dgvZonas.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn7, dataGridViewTextBoxColumn8, dataGridViewTextBoxColumn9 });
        dgvZonas.Location = new Point(16, 272);
        dgvZonas.Name = "dgvZonas";
        dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle9.BackColor = SystemColors.Control;
        dataGridViewCellStyle9.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle9.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle9.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle9.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle9.WrapMode = DataGridViewTriState.True;
        dgvZonas.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
        dgvZonas.RowHeadersWidth = 51;
        dgvZonas.Size = new Size(721, 246);
        dgvZonas.TabIndex = 1;
        dgvZonas.CellValueChanged += DgvCellValueChanged;
        dgvZonas.UserAddedRow += DgvUserAddedRow;
        dgvZonas.UserDeletedRow += DgvUserDeletedRow;
        // 
        // dataGridViewTextBoxColumn3
        // 
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle2.NullValue = null;
        dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle2;
        dataGridViewTextBoxColumn3.HeaderText = "Zona";
        dataGridViewTextBoxColumn3.MinimumWidth = 6;
        dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        dataGridViewTextBoxColumn3.Width = 60;
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle3.NullValue = null;
        dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle3;
        dataGridViewTextBoxColumn4.HeaderText = "Pág";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        dataGridViewTextBoxColumn4.Width = 50;
        // 
        // dataGridViewTextBoxColumn5
        // 
        dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle4.Format = "N1";
        dataGridViewCellStyle4.NullValue = null;
        dataGridViewTextBoxColumn5.DefaultCellStyle = dataGridViewCellStyle4;
        dataGridViewTextBoxColumn5.HeaderText = "X%";
        dataGridViewTextBoxColumn5.MinimumWidth = 6;
        dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
        dataGridViewTextBoxColumn5.Width = 55;
        // 
        // dataGridViewTextBoxColumn6
        // 
        dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle5.Format = "N1";
        dataGridViewCellStyle5.NullValue = null;
        dataGridViewTextBoxColumn6.DefaultCellStyle = dataGridViewCellStyle5;
        dataGridViewTextBoxColumn6.HeaderText = "Y%";
        dataGridViewTextBoxColumn6.MinimumWidth = 6;
        dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
        dataGridViewTextBoxColumn6.Width = 55;
        // 
        // dataGridViewTextBoxColumn7
        // 
        dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle6.Format = "N1";
        dataGridViewCellStyle6.NullValue = null;
        dataGridViewTextBoxColumn7.DefaultCellStyle = dataGridViewCellStyle6;
        dataGridViewTextBoxColumn7.HeaderText = "W%";
        dataGridViewTextBoxColumn7.MinimumWidth = 6;
        dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
        dataGridViewTextBoxColumn7.Width = 55;
        // 
        // dataGridViewTextBoxColumn8
        // 
        dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.TopCenter;
        dataGridViewCellStyle7.Format = "N1";
        dataGridViewCellStyle7.NullValue = null;
        dataGridViewTextBoxColumn8.DefaultCellStyle = dataGridViewCellStyle7;
        dataGridViewTextBoxColumn8.HeaderText = "H%";
        dataGridViewTextBoxColumn8.MinimumWidth = 6;
        dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
        dataGridViewTextBoxColumn8.Width = 55;
        // 
        // dataGridViewTextBoxColumn9
        // 
        dataGridViewTextBoxColumn9.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewCellStyle8.WrapMode = DataGridViewTriState.True;
        dataGridViewTextBoxColumn9.DefaultCellStyle = dataGridViewCellStyle8;
        dataGridViewTextBoxColumn9.HeaderText = "Texto extraído";
        dataGridViewTextBoxColumn9.MinimumWidth = 6;
        dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
        dataGridViewTextBoxColumn9.ReadOnly = true;
        // 
        // tabCampos
        // 
        tabCampos.Controls.Add(lblGeneralModo);
        tabCampos.Controls.Add(cmbModoExtraccion);
        tabCampos.Controls.Add(lblCamposLista);
        tabCampos.Controls.Add(lstCampos);
        tabCampos.Controls.Add(panelDetalle);
        tabCampos.Controls.Add(btnCampoAdd);
        tabCampos.Controls.Add(btnCampoRemove);
        tabCampos.Controls.Add(lblRegexPattern);
        tabCampos.Controls.Add(txtRegexPattern);
        tabCampos.Controls.Add(lblRegexMatchCount);
        tabCampos.Controls.Add(lblRegexResultados);
        tabCampos.Controls.Add(dgvRegexMatches);
        tabCampos.Controls.Add(btnRegexApplyToField);
        tabCampos.Location = new Point(4, 29);
        tabCampos.Name = "tabCampos";
        tabCampos.Size = new Size(745, 531);
        tabCampos.TabIndex = 1;
        tabCampos.Text = "Campos";
        // 
        // lblGeneralModo
        // 
        lblGeneralModo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblGeneralModo.AutoSize = true;
        lblGeneralModo.Location = new Point(438, 254);
        lblGeneralModo.Name = "lblGeneralModo";
        lblGeneralModo.Size = new Size(145, 20);
        lblGeneralModo.TabIndex = 60;
        lblGeneralModo.Text = "Modo de extracción:";
        // 
        // cmbModoExtraccion
        // 
        cmbModoExtraccion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbModoExtraccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbModoExtraccion.Items.AddRange(new object[] { "Simple", "OrdenadoPosicion", "LayoutAnalysis" });
        cmbModoExtraccion.Location = new Point(589, 251);
        cmbModoExtraccion.Name = "cmbModoExtraccion";
        cmbModoExtraccion.Size = new Size(155, 28);
        cmbModoExtraccion.TabIndex = 59;
        cmbModoExtraccion.SelectedIndexChanged += CmbModoExtraccion_SelectedIndexChanged;
        // 
        // lblCamposLista
        // 
        lblCamposLista.AutoSize = true;
        lblCamposLista.Location = new Point(16, 16);
        lblCamposLista.Name = "lblCamposLista";
        lblCamposLista.Size = new Size(132, 20);
        lblCamposLista.TabIndex = 0;
        lblCamposLista.Text = "Campos definidos:";
        // 
        // lstCampos
        // 
        lstCampos.DisplayMember = "Nombre";
        lstCampos.Location = new Point(3, 38);
        lstCampos.Name = "lstCampos";
        lstCampos.Size = new Size(206, 164);
        lstCampos.TabIndex = 1;
        lstCampos.SelectedIndexChanged += LstCampos_SelectedIndexChanged;
        // 
        // panelDetalle
        // 
        panelDetalle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panelDetalle.BorderStyle = BorderStyle.FixedSingle;
        panelDetalle.Controls.Add(lblCampoNombre);
        panelDetalle.Controls.Add(cmbCampoNombre);
        panelDetalle.Controls.Add(lblCampoTipo);
        panelDetalle.Controls.Add(cmbCampoTipo);
        panelDetalle.Controls.Add(lblCampoRegex);
        panelDetalle.Controls.Add(txtCampoRegex);
        panelDetalle.Controls.Add(lblCampoValorFijo);
        panelDetalle.Controls.Add(txtCampoValorFijo);
        panelDetalle.Controls.Add(lblCampoFormato);
        panelDetalle.Controls.Add(txtCampoFormatoFecha);
        panelDetalle.Controls.Add(lblCampoSuma);
        panelDetalle.Controls.Add(txtCampoCamposSuma);
        panelDetalle.Location = new Point(215, 38);
        panelDetalle.Name = "panelDetalle";
        panelDetalle.Size = new Size(526, 198);
        panelDetalle.TabIndex = 2;
        // 
        // lblCampoNombre
        // 
        lblCampoNombre.AutoSize = true;
        lblCampoNombre.Location = new Point(12, 12);
        lblCampoNombre.Name = "lblCampoNombre";
        lblCampoNombre.Size = new Size(67, 20);
        lblCampoNombre.TabIndex = 0;
        lblCampoNombre.Text = "Nombre:";
        // 
        // cmbCampoNombre
        // 
        cmbCampoNombre.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCampoNombre.Items.AddRange(new object[] { "NumeroFactura", "Fecha", "ReceptorNombre", "ReceptorNif", "BaseImponible", "PorcentajeIVA", "CuotaIVA", "PorcentajeIRPF", "CuotaIRPF", "PorcentajeRE", "CuotaRE", "TotalFactura", "ConceptoIngreso", "ConceptoGasto" });
        cmbCampoNombre.Location = new Point(153, 9);
        cmbCampoNombre.Name = "cmbCampoNombre";
        cmbCampoNombre.Size = new Size(308, 28);
        cmbCampoNombre.TabIndex = 1;
        cmbCampoNombre.SelectedIndexChanged += CampoDetalle_Changed;
        cmbCampoNombre.TextChanged += CmbCampoNombre_TextChanged;
        // 
        // lblCampoTipo
        // 
        lblCampoTipo.AutoSize = true;
        lblCampoTipo.Location = new Point(12, 42);
        lblCampoTipo.Name = "lblCampoTipo";
        lblCampoTipo.Size = new Size(135, 20);
        lblCampoTipo.TabIndex = 2;
        lblCampoTipo.Text = "Tipo de extracción:";
        // 
        // cmbCampoTipo
        // 
        cmbCampoTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCampoTipo.Items.AddRange(new object[] { "Regex", "ValorFijo", "Suma", "RegexFechaGeneral", "RegexNifGeneral" });
        cmbCampoTipo.Location = new Point(153, 39);
        cmbCampoTipo.Name = "cmbCampoTipo";
        cmbCampoTipo.Size = new Size(308, 28);
        cmbCampoTipo.TabIndex = 3;
        cmbCampoTipo.SelectedIndexChanged += CampoDetalle_Changed;
        // 
        // lblCampoRegex
        // 
        lblCampoRegex.AutoSize = true;
        lblCampoRegex.Location = new Point(12, 72);
        lblCampoRegex.Name = "lblCampoRegex";
        lblCampoRegex.Size = new Size(53, 20);
        lblCampoRegex.TabIndex = 4;
        lblCampoRegex.Text = "Regex:";
        // 
        // txtCampoRegex
        // 
        txtCampoRegex.Location = new Point(153, 69);
        txtCampoRegex.Name = "txtCampoRegex";
        txtCampoRegex.Size = new Size(308, 27);
        txtCampoRegex.TabIndex = 5;
        txtCampoRegex.TextChanged += CampoDetalle_Changed;
        // 
        // lblCampoValorFijo
        // 
        lblCampoValorFijo.AutoSize = true;
        lblCampoValorFijo.Location = new Point(12, 102);
        lblCampoValorFijo.Name = "lblCampoValorFijo";
        lblCampoValorFijo.Size = new Size(72, 20);
        lblCampoValorFijo.TabIndex = 6;
        lblCampoValorFijo.Text = "Valor fijo:";
        // 
        // txtCampoValorFijo
        // 
        txtCampoValorFijo.Location = new Point(153, 99);
        txtCampoValorFijo.Name = "txtCampoValorFijo";
        txtCampoValorFijo.Size = new Size(308, 27);
        txtCampoValorFijo.TabIndex = 7;
        txtCampoValorFijo.TextChanged += CampoDetalle_Changed;
        // 
        // lblCampoFormato
        // 
        lblCampoFormato.AutoSize = true;
        lblCampoFormato.Location = new Point(12, 132);
        lblCampoFormato.Name = "lblCampoFormato";
        lblCampoFormato.Size = new Size(108, 20);
        lblCampoFormato.TabIndex = 8;
        lblCampoFormato.Text = "Formato fecha:";
        // 
        // txtCampoFormatoFecha
        // 
        txtCampoFormatoFecha.Location = new Point(153, 129);
        txtCampoFormatoFecha.Name = "txtCampoFormatoFecha";
        txtCampoFormatoFecha.PlaceholderText = "dd/MM/yyyy (opcional)";
        txtCampoFormatoFecha.Size = new Size(308, 27);
        txtCampoFormatoFecha.TabIndex = 9;
        txtCampoFormatoFecha.TextChanged += CampoDetalle_Changed;
        // 
        // lblCampoSuma
        // 
        lblCampoSuma.AutoSize = true;
        lblCampoSuma.Location = new Point(12, 162);
        lblCampoSuma.Name = "lblCampoSuma";
        lblCampoSuma.Size = new Size(156, 20);
        lblCampoSuma.TabIndex = 10;
        lblCampoSuma.Text = "Campos suma (coma):";
        // 
        // txtCampoCamposSuma
        // 
        txtCampoCamposSuma.Location = new Point(174, 159);
        txtCampoCamposSuma.Name = "txtCampoCamposSuma";
        txtCampoCamposSuma.PlaceholderText = "BaseImponible,CuotaIVA";
        txtCampoCamposSuma.Size = new Size(287, 27);
        txtCampoCamposSuma.TabIndex = 11;
        txtCampoCamposSuma.TextChanged += CampoDetalle_Changed;
        // 
        // btnCampoAdd
        // 
        btnCampoAdd.FlatStyle = FlatStyle.Flat;
        btnCampoAdd.Location = new Point(3, 206);
        btnCampoAdd.Name = "btnCampoAdd";
        btnCampoAdd.Size = new Size(100, 30);
        btnCampoAdd.TabIndex = 3;
        btnCampoAdd.Text = "+ Añadir";
        btnCampoAdd.UseVisualStyleBackColor = true;
        btnCampoAdd.Click += BtnCampoAdd_Click;
        // 
        // btnCampoRemove
        // 
        btnCampoRemove.FlatStyle = FlatStyle.Flat;
        btnCampoRemove.Location = new Point(109, 206);
        btnCampoRemove.Name = "btnCampoRemove";
        btnCampoRemove.Size = new Size(100, 30);
        btnCampoRemove.TabIndex = 4;
        btnCampoRemove.Text = "− Quitar";
        btnCampoRemove.UseVisualStyleBackColor = true;
        btnCampoRemove.Click += BtnCampoRemove_Click;
        // 
        // lblRegexPattern
        // 
        lblRegexPattern.AutoSize = true;
        lblRegexPattern.Location = new Point(3, 254);
        lblRegexPattern.Name = "lblRegexPattern";
        lblRegexPattern.Size = new Size(127, 20);
        lblRegexPattern.TabIndex = 52;
        lblRegexPattern.Text = "Expresión regular:";
        // 
        // txtRegexPattern
        // 
        txtRegexPattern.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtRegexPattern.Font = new Font("Consolas", 10F);
        txtRegexPattern.Location = new Point(3, 289);
        txtRegexPattern.Name = "txtRegexPattern";
        txtRegexPattern.Size = new Size(619, 27);
        txtRegexPattern.TabIndex = 53;
        txtRegexPattern.TextChanged += EjecutarRegex;
        // 
        // lblRegexMatchCount
        // 
        lblRegexMatchCount.AutoSize = true;
        lblRegexMatchCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblRegexMatchCount.Location = new Point(301, 321);
        lblRegexMatchCount.Name = "lblRegexMatchCount";
        lblRegexMatchCount.Size = new Size(0, 20);
        lblRegexMatchCount.TabIndex = 54;
        // 
        // lblRegexResultados
        // 
        lblRegexResultados.AutoSize = true;
        lblRegexResultados.Location = new Point(3, 321);
        lblRegexResultados.Name = "lblRegexResultados";
        lblRegexResultados.Size = new Size(289, 20);
        lblRegexResultados.TabIndex = 55;
        lblRegexResultados.Text = "Resultados (matches y grupos de captura):";
        // 
        // dgvRegexMatches
        // 
        dgvRegexMatches.AllowUserToAddRows = false;
        dgvRegexMatches.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvRegexMatches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvRegexMatches.ColumnHeadersHeight = 29;
        dgvRegexMatches.Location = new Point(3, 344);
        dgvRegexMatches.Name = "dgvRegexMatches";
        dgvRegexMatches.ReadOnly = true;
        dgvRegexMatches.RowHeadersWidth = 51;
        dgvRegexMatches.Size = new Size(738, 184);
        dgvRegexMatches.TabIndex = 56;
        // 
        // btnRegexApplyToField
        // 
        btnRegexApplyToField.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRegexApplyToField.BackColor = Color.FromArgb(46, 117, 182);
        btnRegexApplyToField.FlatStyle = FlatStyle.Flat;
        btnRegexApplyToField.ForeColor = Color.White;
        btnRegexApplyToField.Location = new Point(628, 285);
        btnRegexApplyToField.Name = "btnRegexApplyToField";
        btnRegexApplyToField.Size = new Size(116, 34);
        btnRegexApplyToField.TabIndex = 57;
        btnRegexApplyToField.Text = "← Asignar";
        btnRegexApplyToField.UseVisualStyleBackColor = false;
        btnRegexApplyToField.Click += BtnRegexApplyToField_Click;
        // 
        // tabMultiLinea
        // 
        tabMultiLinea.Controls.Add(lblMultiLineas);
        tabMultiLinea.Controls.Add(lstMultiLineas);
        tabMultiLinea.Controls.Add(btnMultiLineaAdd);
        tabMultiLinea.Controls.Add(btnMultiLineaRemove);
        tabMultiLinea.Controls.Add(btnMultiLineaUp);
        tabMultiLinea.Controls.Add(btnMultiLineaDown);
        tabMultiLinea.Controls.Add(lblMultiLineaRegex);
        tabMultiLinea.Controls.Add(txtMultiLineaRegex);
        tabMultiLinea.Controls.Add(lblMultiLineaMapeo);
        tabMultiLinea.Controls.Add(dgvMultiLineaMapeo);
        tabMultiLinea.Controls.Add(lblSeparadorPostProc);
        tabMultiLinea.Controls.Add(lstPostProc);
        tabMultiLinea.Controls.Add(btnPostProcAdd);
        tabMultiLinea.Controls.Add(btnPostProcRemove);
        tabMultiLinea.Controls.Add(btnPostProcUp);
        tabMultiLinea.Controls.Add(btnPostProcDown);
        tabMultiLinea.Controls.Add(lblPostProcResumen);
        tabMultiLinea.Controls.Add(lblPostProcTipo);
        tabMultiLinea.Controls.Add(cmbPostProcTipo);
        tabMultiLinea.Controls.Add(lblPostProcCond);
        tabMultiLinea.Controls.Add(txtPostProcCondicion);
        tabMultiLinea.Controls.Add(lblPostCondCampo);
        tabMultiLinea.Controls.Add(cmbPostCondCampo);
        tabMultiLinea.Controls.Add(lblPostCondValor);
        tabMultiLinea.Controls.Add(txtPostCondValor);
        tabMultiLinea.Controls.Add(lblPostAccDestino);
        tabMultiLinea.Controls.Add(cmbPostAccDestino);
        tabMultiLinea.Controls.Add(txtPostAccValor);
        tabMultiLinea.Controls.Add(lblDestinoA);
        tabMultiLinea.Controls.Add(cmbPostAccOrigen1);
        tabMultiLinea.Controls.Add(cmbPostAccOperador);
        tabMultiLinea.Controls.Add(cmbPostAccOrigen2);
        tabMultiLinea.Location = new Point(4, 29);
        tabMultiLinea.Name = "tabMultiLinea";
        tabMultiLinea.Size = new Size(745, 531);
        tabMultiLinea.TabIndex = 2;
        tabMultiLinea.Text = "Avanzado";
        // 
        // lblMultiLineas
        // 
        lblMultiLineas.AutoSize = true;
        lblMultiLineas.Location = new Point(16, 24);
        lblMultiLineas.Name = "lblMultiLineas";
        lblMultiLineas.Size = new Size(294, 20);
        lblMultiLineas.TabIndex = 1;
        lblMultiLineas.Text = "Regex de línea (cada match = una Factura):";
        // 
        // lstMultiLineas
        // 
        lstMultiLineas.DisplayMember = "Regex";
        lstMultiLineas.FormattingEnabled = true;
        lstMultiLineas.Location = new Point(16, 48);
        lstMultiLineas.Name = "lstMultiLineas";
        lstMultiLineas.Size = new Size(326, 184);
        lstMultiLineas.TabIndex = 2;
        lstMultiLineas.SelectedIndexChanged += LstMultiLineas_SelectedIndexChanged;
        // 
        // btnMultiLineaAdd
        // 
        btnMultiLineaAdd.FlatStyle = FlatStyle.Flat;
        btnMultiLineaAdd.Location = new Point(16, 238);
        btnMultiLineaAdd.Name = "btnMultiLineaAdd";
        btnMultiLineaAdd.Size = new Size(77, 30);
        btnMultiLineaAdd.TabIndex = 3;
        btnMultiLineaAdd.Text = "+ Añadir";
        btnMultiLineaAdd.UseVisualStyleBackColor = true;
        btnMultiLineaAdd.Click += BtnMultiLineaAdd_Click;
        // 
        // btnMultiLineaRemove
        // 
        btnMultiLineaRemove.FlatStyle = FlatStyle.Flat;
        btnMultiLineaRemove.Location = new Point(99, 238);
        btnMultiLineaRemove.Name = "btnMultiLineaRemove";
        btnMultiLineaRemove.Size = new Size(77, 30);
        btnMultiLineaRemove.TabIndex = 4;
        btnMultiLineaRemove.Text = "− Quitar";
        btnMultiLineaRemove.UseVisualStyleBackColor = true;
        btnMultiLineaRemove.Click += BtnMultiLineaRemove_Click;
        // 
        // btnMultiLineaUp
        // 
        btnMultiLineaUp.FlatStyle = FlatStyle.Flat;
        btnMultiLineaUp.Location = new Point(182, 238);
        btnMultiLineaUp.Name = "btnMultiLineaUp";
        btnMultiLineaUp.Size = new Size(77, 30);
        btnMultiLineaUp.TabIndex = 5;
        btnMultiLineaUp.Text = "↑ Subir";
        btnMultiLineaUp.UseVisualStyleBackColor = true;
        btnMultiLineaUp.Click += BtnMultiLineaUp_Click;
        // 
        // btnMultiLineaDown
        // 
        btnMultiLineaDown.FlatStyle = FlatStyle.Flat;
        btnMultiLineaDown.Location = new Point(265, 238);
        btnMultiLineaDown.Name = "btnMultiLineaDown";
        btnMultiLineaDown.Size = new Size(77, 30);
        btnMultiLineaDown.TabIndex = 6;
        btnMultiLineaDown.Text = "↓ Bajar";
        btnMultiLineaDown.UseVisualStyleBackColor = true;
        btnMultiLineaDown.Click += BtnMultiLineaDown_Click;
        // 
        // lblMultiLineaRegex
        // 
        lblMultiLineaRegex.AutoSize = true;
        lblMultiLineaRegex.Location = new Point(365, 24);
        lblMultiLineaRegex.Name = "lblMultiLineaRegex";
        lblMultiLineaRegex.Size = new Size(336, 20);
        lblMultiLineaRegex.TabIndex = 7;
        lblMultiLineaRegex.Text = "Regex de la línea seleccionada (match = Factura):";
        // 
        // txtMultiLineaRegex
        // 
        txtMultiLineaRegex.Location = new Point(365, 48);
        txtMultiLineaRegex.Multiline = true;
        txtMultiLineaRegex.Name = "txtMultiLineaRegex";
        txtMultiLineaRegex.Size = new Size(380, 41);
        txtMultiLineaRegex.TabIndex = 8;
        txtMultiLineaRegex.TextChanged += ControlModificado;
        // 
        // lblMultiLineaMapeo
        // 
        lblMultiLineaMapeo.AutoSize = true;
        lblMultiLineaMapeo.Location = new Point(365, 97);
        lblMultiLineaMapeo.Name = "lblMultiLineaMapeo";
        lblMultiLineaMapeo.Size = new Size(198, 20);
        lblMultiLineaMapeo.TabIndex = 9;
        lblMultiLineaMapeo.Text = "Mapeo de grupos a campos:";
        // 
        // dgvMultiLineaMapeo
        // 
        dgvMultiLineaMapeo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMultiLineaMapeo.ColumnHeadersHeight = 29;
        dgvMultiLineaMapeo.Columns.AddRange(new DataGridViewColumn[] { dataGridViewComboBoxColumn1, dataGridViewTextBoxColumn2 });
        dgvMultiLineaMapeo.Location = new Point(365, 121);
        dgvMultiLineaMapeo.Name = "dgvMultiLineaMapeo";
        dgvMultiLineaMapeo.RowHeadersWidth = 51;
        dgvMultiLineaMapeo.Size = new Size(380, 147);
        dgvMultiLineaMapeo.TabIndex = 10;
        dgvMultiLineaMapeo.CellValueChanged += DgvCellValueChanged;
        dgvMultiLineaMapeo.EditingControlShowing += DgvMultiLineaMapeo_EditingControlShowing;
        dgvMultiLineaMapeo.UserAddedRow += DgvUserAddedRow;
        dgvMultiLineaMapeo.UserDeletedRow += DgvUserDeletedRow;
        // 
        // dataGridViewComboBoxColumn1
        // 
        dataGridViewComboBoxColumn1.FlatStyle = FlatStyle.Flat;
        dataGridViewComboBoxColumn1.HeaderText = "Campo";
        dataGridViewComboBoxColumn1.Items.AddRange(new object[] { "BaseImponible", "PorcentajeIVA", "CuotaIVA", "PorcentajeIRPF", "CuotaIRPF", "PorcentajeRE", "CuotaRE", "SubTotal", "TotalFactura" });
        dataGridViewComboBoxColumn1.MinimumWidth = 6;
        dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.HeaderText = "Grupo";
        dataGridViewTextBoxColumn2.MinimumWidth = 6;
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        // 
        // lblSeparadorPostProc
        // 
        lblSeparadorPostProc.AutoSize = true;
        lblSeparadorPostProc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSeparadorPostProc.ForeColor = Color.Gray;
        lblSeparadorPostProc.Location = new Point(16, 315);
        lblSeparadorPostProc.Name = "lblSeparadorPostProc";
        lblSeparadorPostProc.Size = new Size(292, 20);
        lblSeparadorPostProc.TabIndex = 37;
        lblSeparadorPostProc.Text = "━━━━━━ POST-PROCESAMIENTO ━━━━━━";
        // 
        // lstPostProc
        // 
        lstPostProc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lstPostProc.Location = new Point(16, 338);
        lstPostProc.Name = "lstPostProc";
        lstPostProc.Size = new Size(366, 124);
        lstPostProc.TabIndex = 1;
        lstPostProc.SelectedIndexChanged += LstPostProc_SelectedIndexChanged;
        // 
        // btnPostProcAdd
        // 
        btnPostProcAdd.FlatStyle = FlatStyle.Flat;
        btnPostProcAdd.Location = new Point(16, 468);
        btnPostProcAdd.Name = "btnPostProcAdd";
        btnPostProcAdd.Size = new Size(85, 30);
        btnPostProcAdd.TabIndex = 8;
        btnPostProcAdd.Text = "+ Añadir";
        btnPostProcAdd.UseVisualStyleBackColor = true;
        btnPostProcAdd.Click += BtnPostProcAdd_Click;
        // 
        // btnPostProcRemove
        // 
        btnPostProcRemove.FlatStyle = FlatStyle.Flat;
        btnPostProcRemove.Location = new Point(107, 468);
        btnPostProcRemove.Name = "btnPostProcRemove";
        btnPostProcRemove.Size = new Size(85, 30);
        btnPostProcRemove.TabIndex = 9;
        btnPostProcRemove.Text = "− Quitar";
        btnPostProcRemove.UseVisualStyleBackColor = true;
        btnPostProcRemove.Click += BtnPostProcRemove_Click;
        // 
        // btnPostProcUp
        // 
        btnPostProcUp.FlatStyle = FlatStyle.Flat;
        btnPostProcUp.Location = new Point(198, 468);
        btnPostProcUp.Name = "btnPostProcUp";
        btnPostProcUp.Size = new Size(85, 30);
        btnPostProcUp.TabIndex = 20;
        btnPostProcUp.Text = "↑ Subir";
        btnPostProcUp.UseVisualStyleBackColor = true;
        btnPostProcUp.Click += BtnPostProcUp_Click;
        // 
        // btnPostProcDown
        // 
        btnPostProcDown.FlatStyle = FlatStyle.Flat;
        btnPostProcDown.Location = new Point(289, 468);
        btnPostProcDown.Name = "btnPostProcDown";
        btnPostProcDown.Size = new Size(85, 30);
        btnPostProcDown.TabIndex = 21;
        btnPostProcDown.Text = "↓ Bajar";
        btnPostProcDown.UseVisualStyleBackColor = true;
        btnPostProcDown.Click += BtnPostProcDown_Click;
        // 
        // lblPostProcResumen
        // 
        lblPostProcResumen.AutoSize = true;
        lblPostProcResumen.ForeColor = Color.FromArgb(70, 70, 70);
        lblPostProcResumen.Location = new Point(16, 501);
        lblPostProcResumen.Name = "lblPostProcResumen";
        lblPostProcResumen.Size = new Size(72, 20);
        lblPostProcResumen.TabIndex = 19;
        lblPostProcResumen.Text = "Resumen:";
        // 
        // lblPostProcTipo
        // 
        lblPostProcTipo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPostProcTipo.AutoSize = true;
        lblPostProcTipo.Location = new Point(388, 338);
        lblPostProcTipo.Name = "lblPostProcTipo";
        lblPostProcTipo.Size = new Size(42, 20);
        lblPostProcTipo.TabIndex = 2;
        lblPostProcTipo.Text = "Tipo:";
        // 
        // cmbPostProcTipo
        // 
        cmbPostProcTipo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbPostProcTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostProcTipo.Items.AddRange(new object[] { "Invertir Signo", "Establecer Valor", "Calcular" });
        cmbPostProcTipo.Location = new Point(436, 338);
        cmbPostProcTipo.Name = "cmbPostProcTipo";
        cmbPostProcTipo.Size = new Size(306, 28);
        cmbPostProcTipo.TabIndex = 3;
        cmbPostProcTipo.SelectedIndexChanged += CmbPostProcTipo_SelectedIndexChanged;
        // 
        // lblPostProcCond
        // 
        lblPostProcCond.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPostProcCond.AutoSize = true;
        lblPostProcCond.Location = new Point(388, 370);
        lblPostProcCond.Name = "lblPostProcCond";
        lblPostProcCond.Size = new Size(79, 20);
        lblPostProcCond.TabIndex = 4;
        lblPostProcCond.Text = "Condición:";
        // 
        // txtPostProcCondicion
        // 
        txtPostProcCondicion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtPostProcCondicion.Location = new Point(473, 370);
        txtPostProcCondicion.Name = "txtPostProcCondicion";
        txtPostProcCondicion.PlaceholderText = "Texto que debe aparecer en factura";
        txtPostProcCondicion.Size = new Size(269, 27);
        txtPostProcCondicion.TabIndex = 5;
        txtPostProcCondicion.TextChanged += PostProcControl_Changed;
        txtPostProcCondicion.Leave += TxtPostProcCondicion_Leave;
        // 
        // lblPostCondCampo
        // 
        lblPostCondCampo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPostCondCampo.AutoSize = true;
        lblPostCondCampo.Location = new Point(404, 404);
        lblPostCondCampo.Name = "lblPostCondCampo";
        lblPostCondCampo.Size = new Size(21, 20);
        lblPostCondCampo.TabIndex = 40;
        lblPostCondCampo.Text = "Si";
        // 
        // cmbPostCondCampo
        // 
        cmbPostCondCampo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbPostCondCampo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostCondCampo.Items.AddRange(new object[] { "PorcentajeIRPF" });
        cmbPostCondCampo.Location = new Point(431, 404);
        cmbPostCondCampo.Name = "cmbPostCondCampo";
        cmbPostCondCampo.Size = new Size(143, 28);
        cmbPostCondCampo.TabIndex = 6;
        cmbPostCondCampo.SelectedIndexChanged += PostProcControl_Changed;
        // 
        // lblPostCondValor
        // 
        lblPostCondValor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPostCondValor.AutoSize = true;
        lblPostCondValor.Location = new Point(580, 404);
        lblPostCondValor.Name = "lblPostCondValor";
        lblPostCondValor.Size = new Size(23, 20);
        lblPostCondValor.TabIndex = 41;
        lblPostCondValor.Text = "es";
        // 
        // txtPostCondValor
        // 
        txtPostCondValor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtPostCondValor.Location = new Point(609, 404);
        txtPostCondValor.Name = "txtPostCondValor";
        txtPostCondValor.PlaceholderText = "Ej: 0";
        txtPostCondValor.Size = new Size(80, 27);
        txtPostCondValor.TabIndex = 7;
        txtPostCondValor.TextChanged += PostProcControl_Changed;
        // 
        // lblPostAccDestino
        // 
        lblPostAccDestino.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPostAccDestino.AutoSize = true;
        lblPostAccDestino.Location = new Point(388, 436);
        lblPostAccDestino.Name = "lblPostAccDestino";
        lblPostAccDestino.Size = new Size(37, 20);
        lblPostAccDestino.TabIndex = 10;
        lblPostAccDestino.Text = "Fijar";
        // 
        // cmbPostAccDestino
        // 
        cmbPostAccDestino.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbPostAccDestino.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostAccDestino.Items.AddRange(new object[] { "ConceptoIngreso", "ConceptoGasto" });
        cmbPostAccDestino.Location = new Point(431, 434);
        cmbPostAccDestino.Name = "cmbPostAccDestino";
        cmbPostAccDestino.Size = new Size(143, 28);
        cmbPostAccDestino.TabIndex = 11;
        cmbPostAccDestino.SelectedIndexChanged += PostProcControl_Changed;
        // 
        // txtPostAccValor
        // 
        txtPostAccValor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtPostAccValor.Location = new Point(609, 434);
        txtPostAccValor.Name = "txtPostAccValor";
        txtPostAccValor.PlaceholderText = "123,45";
        txtPostAccValor.Size = new Size(80, 27);
        txtPostAccValor.TabIndex = 13;
        txtPostAccValor.TextChanged += PostProcControl_Changed;
        // 
        // lblDestinoA
        // 
        lblDestinoA.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblDestinoA.Location = new Point(580, 428);
        lblDestinoA.Name = "lblDestinoA";
        lblDestinoA.Size = new Size(23, 20);
        lblDestinoA.TabIndex = 42;
        lblDestinoA.Text = "a";
        lblDestinoA.TextAlign = ContentAlignment.TopCenter;
        // 
        // cmbPostAccOrigen1
        // 
        cmbPostAccOrigen1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbPostAccOrigen1.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostAccOrigen1.Location = new Point(431, 468);
        cmbPostAccOrigen1.Name = "cmbPostAccOrigen1";
        cmbPostAccOrigen1.Size = new Size(130, 28);
        cmbPostAccOrigen1.TabIndex = 15;
        cmbPostAccOrigen1.SelectedIndexChanged += PostProcControl_Changed;
        // 
        // cmbPostAccOperador
        // 
        cmbPostAccOperador.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbPostAccOperador.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostAccOperador.Items.AddRange(new object[] { "+", "-", "*", "/", "%" });
        cmbPostAccOperador.Location = new Point(567, 468);
        cmbPostAccOperador.Name = "cmbPostAccOperador";
        cmbPostAccOperador.Size = new Size(40, 28);
        cmbPostAccOperador.TabIndex = 16;
        cmbPostAccOperador.SelectedIndexChanged += PostProcControl_Changed;
        // 
        // cmbPostAccOrigen2
        // 
        cmbPostAccOrigen2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbPostAccOrigen2.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostAccOrigen2.Location = new Point(613, 468);
        cmbPostAccOrigen2.Name = "cmbPostAccOrigen2";
        cmbPostAccOrigen2.Size = new Size(130, 28);
        cmbPostAccOrigen2.TabIndex = 17;
        cmbPostAccOrigen2.SelectedIndexChanged += PostProcControl_Changed;
        // 
        // lblSeparadorRegex
        // 
        lblSeparadorRegex.AutoSize = true;
        lblSeparadorRegex.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSeparadorRegex.ForeColor = Color.Gray;
        lblSeparadorRegex.Location = new Point(10, 571);
        lblSeparadorRegex.Name = "lblSeparadorRegex";
        lblSeparadorRegex.Size = new Size(172, 20);
        lblSeparadorRegex.TabIndex = 50;
        lblSeparadorRegex.Text = "━━━ Texto Extraido ━━━";
        // 
        // txtRegexSource
        // 
        txtRegexSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtRegexSource.Font = new Font("Consolas", 9F);
        txtRegexSource.Location = new Point(13, 594);
        txtRegexSource.Multiline = true;
        txtRegexSource.Name = "txtRegexSource";
        txtRegexSource.ScrollBars = ScrollBars.Vertical;
        txtRegexSource.Size = new Size(738, 70);
        txtRegexSource.TabIndex = 51;
        txtRegexSource.TextChanged += EjecutarRegex;
        // 
        // btnGuardar
        // 
        btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnGuardar.BackColor = Color.FromArgb(46, 117, 182);
        btnGuardar.FlatStyle = FlatStyle.Flat;
        btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnGuardar.ForeColor = Color.White;
        btnGuardar.Location = new Point(13, 670);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(133, 33);
        btnGuardar.TabIndex = 1;
        btnGuardar.Text = "💾 Guardar";
        btnGuardar.UseVisualStyleBackColor = true;
        btnGuardar.Click += BtnGuardar_Click;
        // 
        // btnCargarPdfMuestra
        // 
        btnCargarPdfMuestra.Anchor = AnchorStyles.Top;
        btnCargarPdfMuestra.BackColor = Color.FromArgb(46, 117, 182);
        btnCargarPdfMuestra.FlatStyle = FlatStyle.Flat;
        btnCargarPdfMuestra.ForeColor = Color.White;
        btnCargarPdfMuestra.Location = new Point(8, 0);
        btnCargarPdfMuestra.Name = "btnCargarPdfMuestra";
        btnCargarPdfMuestra.Size = new Size(225, 32);
        btnCargarPdfMuestra.TabIndex = 0;
        btnCargarPdfMuestra.Text = "Cargar PDF de muestra";
        btnCargarPdfMuestra.UseVisualStyleBackColor = false;
        btnCargarPdfMuestra.Click += BtnCargarPdfMuestra_Click;
        // 
        // tabPaginas
        // 
        tabPaginas.Anchor = AnchorStyles.Top;
        tabPaginas.Location = new Point(8, 35);
        tabPaginas.Name = "tabPaginas";
        tabPaginas.SelectedIndex = 0;
        tabPaginas.Size = new Size(225, 28);
        tabPaginas.TabIndex = 1;
        tabPaginas.SelectedIndexChanged += TabPaginas_SelectedIndexChanged;
        // 
        // picFactura
        // 
        picFactura.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
        picFactura.BackColor = Color.LightGray;
        picFactura.BorderStyle = BorderStyle.FixedSingle;
        picFactura.Cursor = Cursors.Cross;
        picFactura.Location = new Point(8, 69);
        picFactura.Name = "picFactura";
        picFactura.Size = new Size(225, 634);
        picFactura.SizeMode = PictureBoxSizeMode.Zoom;
        picFactura.TabIndex = 2;
        picFactura.TabStop = false;
        picFactura.Paint += PicFactura_Paint;
        picFactura.MouseDown += PicFactura_MouseDown;
        picFactura.MouseMove += PicFactura_MouseMove;
        picFactura.MouseUp += PicFactura_MouseUp;
        // 
        // panelDerecho
        // 
        panelDerecho.Controls.Add(txtRegexSource);
        panelDerecho.Controls.Add(lblSeparadorRegex);
        panelDerecho.Controls.Add(tabs);
        panelDerecho.Controls.Add(btnGuardar);
        panelDerecho.Dock = DockStyle.Fill;
        panelDerecho.Location = new Point(585, 0);
        panelDerecho.Name = "panelDerecho";
        panelDerecho.Size = new Size(762, 711);
        panelDerecho.TabIndex = 6;
        // 
        // panelCentral
        // 
        panelCentral.Controls.Add(btnCargarPdfMuestra);
        panelCentral.Controls.Add(tabPaginas);
        panelCentral.Controls.Add(picFactura);
        panelCentral.Dock = DockStyle.Left;
        panelCentral.Location = new Point(346, 0);
        panelCentral.Name = "panelCentral";
        panelCentral.Size = new Size(239, 711);
        panelCentral.TabIndex = 2;
        // 
        // GestionEmisoresForm
        // 
        ClientSize = new Size(1347, 711);
        Controls.Add(panelDerecho);
        Controls.Add(panelCentral);
        Controls.Add(panelIzquierdo);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimizeBox = false;
        Name = "GestionEmisoresForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gestionar Emisores";
        WindowState = FormWindowState.Maximized;
        panelIzquierdo.ResumeLayout(false);
        panelIzquierdo.PerformLayout();
        tabs.ResumeLayout(false);
        tabGeneral.ResumeLayout(false);
        tabGeneral.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvZonas).EndInit();
        tabCampos.ResumeLayout(false);
        tabCampos.PerformLayout();
        panelDetalle.ResumeLayout(false);
        panelDetalle.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRegexMatches).EndInit();
        tabMultiLinea.ResumeLayout(false);
        tabMultiLinea.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMultiLineaMapeo).EndInit();
        ((System.ComponentModel.ISupportInitialize)picFactura).EndInit();
        panelDerecho.ResumeLayout(false);
        panelDerecho.PerformLayout();
        panelCentral.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel panelIzquierdo;
    private TabControl tabs;
    private TabPage tabGeneral;
    private Label lblGeneralNombre;
    private Button btnGuardar;
    private TextBox txtNombre;
    private Label lblGeneralNif;
    private TextBox txtNif;
    private Label lblGeneralIds;
    private ListBox lstIdentificadores;
    private TextBox txtNuevoId;
    private Button btnAddId;
    private Button btnRemoveId;
    private Label lblGeneralCultura;
    private ComboBox cmbCulturaFecha;
    private Label lblGeneralConceptoIngreso;
    private TextBox txtConceptoIngreso;
    private Label lblGeneralConceptoGasto;
    private TextBox txtConceptoGasto;
    private TabPage tabCampos;
    private Label lblCamposLista;
    private ListBox lstCampos;
    private Panel panelDetalle;
    private Label lblCampoNombre;
    private ComboBox cmbCampoNombre;
    private Label lblCampoTipo;
    private ComboBox cmbCampoTipo;
    private Label lblCampoRegex;
    private TextBox txtCampoRegex;
    private Label lblCampoValorFijo;
    private TextBox txtCampoValorFijo;
    private Label lblCampoFormato;
    private TextBox txtCampoFormatoFecha;
    private Label lblCampoSuma;
    private TextBox txtCampoCamposSuma;
    private Button btnCampoAdd;
    private Button btnCampoRemove;
    private TabPage tabMultiLinea;
    private Label lblMultiLineas;
    private ListBox lstMultiLineas;
    private Button btnMultiLineaAdd;
    private Button btnMultiLineaRemove;
    private Button btnMultiLineaUp;
    private Button btnMultiLineaDown;
    private Label lblMultiLineaRegex;
    private TextBox txtMultiLineaRegex;
    private Label lblMultiLineaMapeo;
    private DataGridView dgvMultiLineaMapeo;
    private DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private Label lblSeparadorPostProc;
    private ListBox lstPostProc;
    private Button btnPostProcUp;
    private Button btnPostProcDown;
    private Label lblPostProcTipo;
    private ComboBox cmbPostProcTipo;
    private Label lblPostProcCond;
    private TextBox txtPostProcCondicion;
    private Label lblPostCondCampo;
    private ComboBox cmbPostCondCampo;
    private Label lblPostCondValor;
    private TextBox txtPostCondValor;
    private Label lblPostAccDestino;
    private ComboBox cmbPostAccDestino;
    private TextBox txtPostAccValor;
    private ComboBox cmbPostAccOrigen1;
    private ComboBox cmbPostAccOperador;
    private ComboBox cmbPostAccOrigen2;
    private Label lblPostProcResumen;
    private Button btnPostProcAdd;
    private Button btnPostProcRemove;
    private Label lblZonasTitulo;
    private DataGridView dgvZonas;
    private Label lblSeparadorRegex;
    private TextBox txtRegexSource;
    private Label lblRegexPattern;
    private TextBox txtRegexPattern;
    private Label lblRegexMatchCount;
    private Label lblRegexResultados;
    private DataGridView dgvRegexMatches;
    private Button btnRegexApplyToField;
    protected Button btnClonar;
    private Panel panelDerecho;
    private Panel panelCentral;
    private Button btnCargarPdfMuestra;
    private TabControl tabPaginas;
    private PictureBox picFactura;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
    private Label lblGeneralModo;
    private ComboBox cmbModoExtraccion;
    private Label lblDestinoA;
}
