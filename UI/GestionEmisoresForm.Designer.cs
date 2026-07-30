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
        lblGeneralModo = new Label();
        cmbModoExtraccion = new ComboBox();
        lblGeneralCultura = new Label();
        cmbCulturaFecha = new ComboBox();
        lblGeneralConceptoIngreso = new Label();
        txtConceptoIngreso = new TextBox();
        lblGeneralConceptoGasto = new Label();
        txtConceptoGasto = new TextBox();
        tabZonas = new TabPage();
        lblZonasTitulo = new Label();
        dgvZonas = new DataGridView();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn10 = new DataGridViewTextBoxColumn();
        lblSeparadorZonas = new Label();
        txtZonasSource = new TextBox();
        tabCampos = new TabPage();
        lblCamposLista = new Label();
        lstCampos = new ListBox();
        panelDetalle = new Panel();
        lblCampoNombre = new Label();
        cmbCampoNombre = new ComboBox();
        lblCampoTipo = new Label();
        cmbCampoTipo = new ComboBox();
        lblCampoRegex = new Label();
        txtCampoRegex = new TextBox();
        lblCampoGrupo = new Label();
        txtCampoGrupo = new TextBox();
        lblCampoValorFijo = new Label();
        txtCampoValorFijo = new TextBox();
        lblCampoFormato = new Label();
        txtCampoFormatoFecha = new TextBox();
        lblCampoSuma = new Label();
        txtCampoCamposSuma = new TextBox();
        btnCampoAdd = new Button();
        btnCampoRemove = new Button();
        lblSeparadorRegex = new Label();
        txtRegexSource = new TextBox();
        lblRegexPattern = new Label();
        txtRegexPattern = new TextBox();
        lblRegexMatchCount = new Label();
        lblRegexResultados = new Label();
        dgvRegexMatches = new DataGridView();
        btnRegexApplyToField = new Button();
        tabMultiIVA = new TabPage();
        chkMultiIVA = new CheckBox();
        lblMultiIVARegex = new Label();
        txtMultiIVARegex = new TextBox();
        lblMultiIVAMapeo = new Label();
        dgvMultiIVAMapeo = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        tabPostProc = new TabPage();
        lblPostProcReglas = new Label();
        lstPostProc = new ListBox();
        lblPostProcTipo = new Label();
        cmbPostProcTipo = new ComboBox();
        lblPostProcCond = new Label();
        txtPostProcCondicion = new TextBox();
        lblPostProcCampos = new Label();
        txtPostProcCampos = new TextBox();
        btnPostProcAdd = new Button();
        btnPostProcRemove = new Button();
        btnGuardar = new Button();
        btnCargarPdfMuestra = new Button();
        tabPaginas = new TabControl();
        picFactura = new PictureBox();
        panelDerecho = new Panel();
        panelCentral = new Panel();
        panelIzquierdo.SuspendLayout();
        tabs.SuspendLayout();
        tabGeneral.SuspendLayout();
        tabZonas.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvZonas).BeginInit();
        tabCampos.SuspendLayout();
        panelDetalle.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRegexMatches).BeginInit();
        tabMultiIVA.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMultiIVAMapeo).BeginInit();
        tabPostProc.SuspendLayout();
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
        lstEmisores.Location = new Point(11, 30);
        lstEmisores.Name = "lstEmisores";
        lstEmisores.Size = new Size(324, 620);
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
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabs.Controls.Add(tabGeneral);
        tabs.Controls.Add(tabZonas);
        tabs.Controls.Add(tabCampos);
        tabs.Controls.Add(tabMultiIVA);
        tabs.Controls.Add(tabPostProc);
        tabs.Location = new Point(0, 0);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(744, 660);
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
        tabGeneral.Controls.Add(lblGeneralModo);
        tabGeneral.Controls.Add(cmbModoExtraccion);
        tabGeneral.Controls.Add(lblGeneralCultura);
        tabGeneral.Controls.Add(cmbCulturaFecha);
        tabGeneral.Controls.Add(lblGeneralConceptoIngreso);
        tabGeneral.Controls.Add(txtConceptoIngreso);
        tabGeneral.Controls.Add(lblGeneralConceptoGasto);
        tabGeneral.Controls.Add(txtConceptoGasto);
        tabGeneral.Location = new Point(4, 29);
        tabGeneral.Name = "tabGeneral";
        tabGeneral.Size = new Size(736, 627);
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
        txtNombre.Location = new Point(16, 38);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(400, 27);
        txtNombre.TabIndex = 1;
        txtNombre.TextChanged += ControlModificado;
        // 
        // lblGeneralNif
        // 
        lblGeneralNif.AutoSize = true;
        lblGeneralNif.Location = new Point(16, 68);
        lblGeneralNif.Name = "lblGeneralNif";
        lblGeneralNif.Size = new Size(268, 20);
        lblGeneralNif.TabIndex = 2;
        lblGeneralNif.Text = "NIF (clave única = nombre del archivo):";
        // 
        // txtNif
        // 
        txtNif.Location = new Point(16, 90);
        txtNif.Name = "txtNif";
        txtNif.Size = new Size(200, 27);
        txtNif.TabIndex = 3;
        txtNif.TextChanged += ControlModificado;
        // 
        // lblGeneralIds
        // 
        lblGeneralIds.AutoSize = true;
        lblGeneralIds.Location = new Point(16, 120);
        lblGeneralIds.Name = "lblGeneralIds";
        lblGeneralIds.Size = new Size(323, 20);
        lblGeneralIds.TabIndex = 4;
        lblGeneralIds.Text = "Identificadores (textos para detectar el emisor):";
        // 
        // lstIdentificadores
        // 
        lstIdentificadores.Location = new Point(16, 177);
        lstIdentificadores.Name = "lstIdentificadores";
        lstIdentificadores.Size = new Size(400, 84);
        lstIdentificadores.TabIndex = 5;
        // 
        // txtNuevoId
        // 
        txtNuevoId.Location = new Point(16, 143);
        txtNuevoId.Name = "txtNuevoId";
        txtNuevoId.Size = new Size(316, 27);
        txtNuevoId.TabIndex = 6;
        // 
        // btnAddId
        // 
        btnAddId.FlatStyle = FlatStyle.Flat;
        btnAddId.Location = new Point(338, 141);
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
        btnRemoveId.Location = new Point(380, 141);
        btnRemoveId.Name = "btnRemoveId";
        btnRemoveId.Size = new Size(36, 30);
        btnRemoveId.TabIndex = 8;
        btnRemoveId.Text = "−";
        btnRemoveId.UseVisualStyleBackColor = true;
        btnRemoveId.Click += BtnRemoveId_Click;
        // 
        // lblGeneralModo
        // 
        lblGeneralModo.AutoSize = true;
        lblGeneralModo.Location = new Point(16, 282);
        lblGeneralModo.Name = "lblGeneralModo";
        lblGeneralModo.Size = new Size(204, 20);
        lblGeneralModo.TabIndex = 9;
        lblGeneralModo.Text = "Modo de extracción de texto:";
        // 
        // cmbModoExtraccion
        // 
        cmbModoExtraccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbModoExtraccion.Items.AddRange(new object[] { "Simple", "OrdenadoPosicion", "LayoutAnalysis" });
        cmbModoExtraccion.Location = new Point(16, 304);
        cmbModoExtraccion.Name = "cmbModoExtraccion";
        cmbModoExtraccion.Size = new Size(200, 28);
        cmbModoExtraccion.TabIndex = 10;
        cmbModoExtraccion.SelectedIndexChanged += ControlModificado;
        // 
        // lblGeneralCultura
        // 
        lblGeneralCultura.AutoSize = true;
        lblGeneralCultura.Location = new Point(257, 281);
        lblGeneralCultura.Name = "lblGeneralCultura";
        lblGeneralCultura.Size = new Size(139, 20);
        lblGeneralCultura.TabIndex = 11;
        lblGeneralCultura.Text = "Cultura para fechas:";
        // 
        // cmbCulturaFecha
        // 
        cmbCulturaFecha.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCulturaFecha.Items.AddRange(new object[] { "es-ES", "en-US" });
        cmbCulturaFecha.Location = new Point(257, 304);
        cmbCulturaFecha.Name = "cmbCulturaFecha";
        cmbCulturaFecha.Size = new Size(159, 28);
        cmbCulturaFecha.TabIndex = 12;
        cmbCulturaFecha.SelectedIndexChanged += ControlModificado;
        // 
        // lblGeneralConceptoIngreso
        // 
        lblGeneralConceptoIngreso.AutoSize = true;
        lblGeneralConceptoIngreso.Location = new Point(16, 355);
        lblGeneralConceptoIngreso.Name = "lblGeneralConceptoIngreso";
        lblGeneralConceptoIngreso.Size = new Size(129, 20);
        lblGeneralConceptoIngreso.TabIndex = 13;
        lblGeneralConceptoIngreso.Text = "Concepto ingreso:";
        // 
        // txtConceptoIngreso
        // 
        txtConceptoIngreso.Location = new Point(16, 378);
        txtConceptoIngreso.Name = "txtConceptoIngreso";
        txtConceptoIngreso.Size = new Size(129, 27);
        txtConceptoIngreso.TabIndex = 14;
        txtConceptoIngreso.TextChanged += ControlModificado;
        // 
        // lblGeneralConceptoGasto
        // 
        lblGeneralConceptoGasto.AutoSize = true;
        lblGeneralConceptoGasto.Location = new Point(167, 355);
        lblGeneralConceptoGasto.Name = "lblGeneralConceptoGasto";
        lblGeneralConceptoGasto.Size = new Size(117, 20);
        lblGeneralConceptoGasto.TabIndex = 15;
        lblGeneralConceptoGasto.Text = "Concepto gasto:";
        // 
        // txtConceptoGasto
        // 
        txtConceptoGasto.Location = new Point(167, 378);
        txtConceptoGasto.Name = "txtConceptoGasto";
        txtConceptoGasto.Size = new Size(129, 27);
        txtConceptoGasto.TabIndex = 16;
        txtConceptoGasto.TextChanged += ControlModificado;
        // 
        // tabZonas
        // 
        tabZonas.Controls.Add(lblZonasTitulo);
        tabZonas.Controls.Add(dgvZonas);
        tabZonas.Controls.Add(lblSeparadorZonas);
        tabZonas.Controls.Add(txtZonasSource);
        tabZonas.Location = new Point(4, 29);
        tabZonas.Name = "tabZonas";
        tabZonas.Size = new Size(736, 627);
        tabZonas.TabIndex = 4;
        tabZonas.Text = "Zonas OCR";
        // 
        // lblZonasTitulo
        // 
        lblZonasTitulo.AutoSize = true;
        lblZonasTitulo.Location = new Point(3, 15);
        lblZonasTitulo.Name = "lblZonasTitulo";
        lblZonasTitulo.Size = new Size(126, 20);
        lblZonasTitulo.TabIndex = 0;
        lblZonasTitulo.Text = "Zonas del emisor:";
        // 
        // dgvZonas
        // 
        dgvZonas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        dgvZonas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvZonas.ColumnHeadersHeight = 29;
        dgvZonas.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn7, dataGridViewTextBoxColumn8, dataGridViewTextBoxColumn9, dataGridViewTextBoxColumn10 });
        dgvZonas.Location = new Point(3, 38);
        dgvZonas.Name = "dgvZonas";
        dgvZonas.RowHeadersWidth = 51;
        dgvZonas.Size = new Size(729, 260);
        dgvZonas.TabIndex = 1;
        dgvZonas.CellValueChanged += DgvCellValueChanged;
        dgvZonas.UserAddedRow += DgvUserAddedRow;
        dgvZonas.UserDeletedRow += DgvUserDeletedRow;
        // 
        // dataGridViewTextBoxColumn3
        // 
        dataGridViewTextBoxColumn3.HeaderText = "Campo";
        dataGridViewTextBoxColumn3.MinimumWidth = 6;
        dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewTextBoxColumn4.HeaderText = "Pág";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        // 
        // dataGridViewTextBoxColumn5
        // 
        dataGridViewTextBoxColumn5.HeaderText = "X%";
        dataGridViewTextBoxColumn5.MinimumWidth = 6;
        dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
        // 
        // dataGridViewTextBoxColumn6
        // 
        dataGridViewTextBoxColumn6.HeaderText = "Y%";
        dataGridViewTextBoxColumn6.MinimumWidth = 6;
        dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
        // 
        // dataGridViewTextBoxColumn7
        // 
        dataGridViewTextBoxColumn7.HeaderText = "Ancho%";
        dataGridViewTextBoxColumn7.MinimumWidth = 6;
        dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
        // 
        // dataGridViewTextBoxColumn8
        // 
        dataGridViewTextBoxColumn8.HeaderText = "Alto%";
        dataGridViewTextBoxColumn8.MinimumWidth = 6;
        dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
        // 
        // dataGridViewTextBoxColumn9
        // 
        dataGridViewTextBoxColumn9.HeaderText = "RegexRespaldo";
        dataGridViewTextBoxColumn9.MinimumWidth = 6;
        dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
        // 
        // dataGridViewTextBoxColumn10
        // 
        dataGridViewTextBoxColumn10.HeaderText = "Opcional";
        dataGridViewTextBoxColumn10.MinimumWidth = 6;
        dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
        // 
        // lblSeparadorZonas
        // 
        lblSeparadorZonas.AutoSize = true;
        lblSeparadorZonas.Font = new Font("Consolas", 10F, FontStyle.Bold);
        lblSeparadorZonas.ForeColor = Color.Gray;
        lblSeparadorZonas.Location = new Point(3, 307);
        lblSeparadorZonas.Name = "lblSeparadorZonas";
        lblSeparadorZonas.Size = new Size(273, 20);
        lblSeparadorZonas.TabIndex = 56;
        lblSeparadorZonas.Text = "━━━ Vista de la extracción ━━━";
        // 
        // txtZonasSource
        // 
        txtZonasSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtZonasSource.Font = new Font("Consolas", 9F);
        txtZonasSource.Location = new Point(3, 330);
        txtZonasSource.Multiline = true;
        txtZonasSource.Name = "txtZonasSource";
        txtZonasSource.Size = new Size(729, 294);
        txtZonasSource.TabIndex = 57;
        txtZonasSource.TextChanged += TxtZonasSource_TextChanged;
        // 
        // tabCampos
        // 
        tabCampos.Controls.Add(lblCamposLista);
        tabCampos.Controls.Add(lstCampos);
        tabCampos.Controls.Add(panelDetalle);
        tabCampos.Controls.Add(btnCampoAdd);
        tabCampos.Controls.Add(btnCampoRemove);
        tabCampos.Controls.Add(lblSeparadorRegex);
        tabCampos.Controls.Add(txtRegexSource);
        tabCampos.Controls.Add(lblRegexPattern);
        tabCampos.Controls.Add(txtRegexPattern);
        tabCampos.Controls.Add(lblRegexMatchCount);
        tabCampos.Controls.Add(lblRegexResultados);
        tabCampos.Controls.Add(dgvRegexMatches);
        tabCampos.Controls.Add(btnRegexApplyToField);
        tabCampos.Location = new Point(4, 29);
        tabCampos.Name = "tabCampos";
        tabCampos.Size = new Size(736, 627);
        tabCampos.TabIndex = 1;
        tabCampos.Text = "Campos";
        // 
        // lblCamposLista
        // 
        lblCamposLista.AutoSize = true;
        lblCamposLista.Location = new Point(3, 15);
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
        lstCampos.Size = new Size(206, 244);
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
        panelDetalle.Controls.Add(lblCampoGrupo);
        panelDetalle.Controls.Add(txtCampoGrupo);
        panelDetalle.Controls.Add(lblCampoValorFijo);
        panelDetalle.Controls.Add(txtCampoValorFijo);
        panelDetalle.Controls.Add(lblCampoFormato);
        panelDetalle.Controls.Add(txtCampoFormatoFecha);
        panelDetalle.Controls.Add(lblCampoSuma);
        panelDetalle.Controls.Add(txtCampoCamposSuma);
        panelDetalle.Location = new Point(215, 38);
        panelDetalle.Name = "panelDetalle";
        panelDetalle.Size = new Size(517, 280);
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
        cmbCampoNombre.Items.AddRange(new object[] { "NumeroFactura", "Fecha", "BaseImponible", "PorcentajeIVA", "CuotaIVA", "Total", "ReceptorNombre", "ReceptorNif", "EmisorNif", "PorcentajeIRPF", "CuotaIRPF", "PorcentajeRE", "CuotaRE", "ConceptoIngreso", "ConceptoGasto" });
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
        // lblCampoGrupo
        // 
        lblCampoGrupo.AutoSize = true;
        lblCampoGrupo.Location = new Point(12, 102);
        lblCampoGrupo.Name = "lblCampoGrupo";
        lblCampoGrupo.Size = new Size(53, 20);
        lblCampoGrupo.TabIndex = 6;
        lblCampoGrupo.Text = "Grupo:";
        // 
        // txtCampoGrupo
        // 
        txtCampoGrupo.Location = new Point(126, 99);
        txtCampoGrupo.Name = "txtCampoGrupo";
        txtCampoGrupo.Size = new Size(50, 27);
        txtCampoGrupo.TabIndex = 7;
        txtCampoGrupo.Text = "1";
        txtCampoGrupo.TextChanged += CampoDetalle_Changed;
        // 
        // lblCampoValorFijo
        // 
        lblCampoValorFijo.AutoSize = true;
        lblCampoValorFijo.Location = new Point(12, 132);
        lblCampoValorFijo.Name = "lblCampoValorFijo";
        lblCampoValorFijo.Size = new Size(72, 20);
        lblCampoValorFijo.TabIndex = 8;
        lblCampoValorFijo.Text = "Valor fijo:";
        // 
        // txtCampoValorFijo
        // 
        txtCampoValorFijo.Location = new Point(126, 129);
        txtCampoValorFijo.Name = "txtCampoValorFijo";
        txtCampoValorFijo.Size = new Size(335, 27);
        txtCampoValorFijo.TabIndex = 9;
        txtCampoValorFijo.TextChanged += CampoDetalle_Changed;
        // 
        // lblCampoFormato
        // 
        lblCampoFormato.AutoSize = true;
        lblCampoFormato.Location = new Point(12, 162);
        lblCampoFormato.Name = "lblCampoFormato";
        lblCampoFormato.Size = new Size(108, 20);
        lblCampoFormato.TabIndex = 10;
        lblCampoFormato.Text = "Formato fecha:";
        // 
        // txtCampoFormatoFecha
        // 
        txtCampoFormatoFecha.Location = new Point(126, 159);
        txtCampoFormatoFecha.Name = "txtCampoFormatoFecha";
        txtCampoFormatoFecha.PlaceholderText = "dd/MM/yyyy (opcional)";
        txtCampoFormatoFecha.Size = new Size(335, 27);
        txtCampoFormatoFecha.TabIndex = 11;
        txtCampoFormatoFecha.TextChanged += CampoDetalle_Changed;
        // 
        // lblCampoSuma
        // 
        lblCampoSuma.AutoSize = true;
        lblCampoSuma.Location = new Point(12, 192);
        lblCampoSuma.Name = "lblCampoSuma";
        lblCampoSuma.Size = new Size(156, 20);
        lblCampoSuma.TabIndex = 12;
        lblCampoSuma.Text = "Campos suma (coma):";
        // 
        // txtCampoCamposSuma
        // 
        txtCampoCamposSuma.Location = new Point(174, 189);
        txtCampoCamposSuma.Name = "txtCampoCamposSuma";
        txtCampoCamposSuma.PlaceholderText = "BaseImponible,CuotaIVA";
        txtCampoCamposSuma.Size = new Size(287, 27);
        txtCampoCamposSuma.TabIndex = 13;
        txtCampoCamposSuma.TextChanged += CampoDetalle_Changed;
        // 
        // btnCampoAdd
        // 
        btnCampoAdd.FlatStyle = FlatStyle.Flat;
        btnCampoAdd.Location = new Point(3, 288);
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
        btnCampoRemove.Location = new Point(109, 288);
        btnCampoRemove.Name = "btnCampoRemove";
        btnCampoRemove.Size = new Size(100, 30);
        btnCampoRemove.TabIndex = 4;
        btnCampoRemove.Text = "− Quitar";
        btnCampoRemove.UseVisualStyleBackColor = true;
        btnCampoRemove.Click += BtnCampoRemove_Click;
        // 
        // lblSeparadorRegex
        // 
        lblSeparadorRegex.AutoSize = true;
        lblSeparadorRegex.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSeparadorRegex.ForeColor = Color.Gray;
        lblSeparadorRegex.Location = new Point(3, 331);
        lblSeparadorRegex.Name = "lblSeparadorRegex";
        lblSeparadorRegex.Size = new Size(165, 20);
        lblSeparadorRegex.TabIndex = 50;
        lblSeparadorRegex.Text = "━━━ Probar Regex ━━━";
        // 
        // txtRegexSource
        // 
        txtRegexSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtRegexSource.Font = new Font("Consolas", 9F);
        txtRegexSource.Location = new Point(3, 354);
        txtRegexSource.Multiline = true;
        txtRegexSource.Name = "txtRegexSource";
        txtRegexSource.Size = new Size(729, 52);
        txtRegexSource.TabIndex = 51;
        txtRegexSource.TextChanged += EjecutarRegex;
        // 
        // lblRegexPattern
        // 
        lblRegexPattern.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblRegexPattern.AutoSize = true;
        lblRegexPattern.Location = new Point(3, 422);
        lblRegexPattern.Name = "lblRegexPattern";
        lblRegexPattern.Size = new Size(127, 20);
        lblRegexPattern.TabIndex = 52;
        lblRegexPattern.Text = "Expresión regular:";
        // 
        // txtRegexPattern
        // 
        txtRegexPattern.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtRegexPattern.Font = new Font("Consolas", 10F);
        txtRegexPattern.Location = new Point(3, 445);
        txtRegexPattern.Name = "txtRegexPattern";
        txtRegexPattern.Size = new Size(607, 27);
        txtRegexPattern.TabIndex = 53;
        txtRegexPattern.TextChanged += EjecutarRegex;
        // 
        // lblRegexMatchCount
        // 
        lblRegexMatchCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblRegexMatchCount.AutoSize = true;
        lblRegexMatchCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblRegexMatchCount.Location = new Point(298, 485);
        lblRegexMatchCount.Name = "lblRegexMatchCount";
        lblRegexMatchCount.Size = new Size(0, 20);
        lblRegexMatchCount.TabIndex = 54;
        // 
        // lblRegexResultados
        // 
        lblRegexResultados.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblRegexResultados.AutoSize = true;
        lblRegexResultados.Location = new Point(3, 485);
        lblRegexResultados.Name = "lblRegexResultados";
        lblRegexResultados.Size = new Size(289, 20);
        lblRegexResultados.TabIndex = 55;
        lblRegexResultados.Text = "Resultados (matches y grupos de captura):";
        // 
        // dgvRegexMatches
        // 
        dgvRegexMatches.AllowUserToAddRows = false;
        dgvRegexMatches.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvRegexMatches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvRegexMatches.ColumnHeadersHeight = 29;
        dgvRegexMatches.Location = new Point(3, 508);
        dgvRegexMatches.Name = "dgvRegexMatches";
        dgvRegexMatches.ReadOnly = true;
        dgvRegexMatches.RowHeadersWidth = 51;
        dgvRegexMatches.Size = new Size(729, 113);
        dgvRegexMatches.TabIndex = 56;
        // 
        // btnRegexApplyToField
        // 
        btnRegexApplyToField.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnRegexApplyToField.BackColor = Color.FromArgb(46, 117, 182);
        btnRegexApplyToField.FlatStyle = FlatStyle.Flat;
        btnRegexApplyToField.ForeColor = Color.White;
        btnRegexApplyToField.Location = new Point(616, 441);
        btnRegexApplyToField.Name = "btnRegexApplyToField";
        btnRegexApplyToField.Size = new Size(116, 34);
        btnRegexApplyToField.TabIndex = 57;
        btnRegexApplyToField.Text = "← Asignar";
        btnRegexApplyToField.UseVisualStyleBackColor = false;
        btnRegexApplyToField.Click += BtnRegexApplyToField_Click;
        // 
        // tabMultiIVA
        // 
        tabMultiIVA.Controls.Add(chkMultiIVA);
        tabMultiIVA.Controls.Add(lblMultiIVARegex);
        tabMultiIVA.Controls.Add(txtMultiIVARegex);
        tabMultiIVA.Controls.Add(lblMultiIVAMapeo);
        tabMultiIVA.Controls.Add(dgvMultiIVAMapeo);
        tabMultiIVA.Location = new Point(4, 29);
        tabMultiIVA.Name = "tabMultiIVA";
        tabMultiIVA.Size = new Size(736, 627);
        tabMultiIVA.TabIndex = 2;
        tabMultiIVA.Text = "Multi-IVA";
        // 
        // chkMultiIVA
        // 
        chkMultiIVA.AutoSize = true;
        chkMultiIVA.Location = new Point(16, 16);
        chkMultiIVA.Name = "chkMultiIVA";
        chkMultiIVA.Size = new Size(416, 24);
        chkMultiIVA.TabIndex = 0;
        chkMultiIVA.Text = "Habilitar modo multifactura (una Factura por línea de IVA)";
        chkMultiIVA.CheckedChanged += ControlModificado;
        // 
        // lblMultiIVARegex
        // 
        lblMultiIVARegex.AutoSize = true;
        lblMultiIVARegex.Location = new Point(16, 48);
        lblMultiIVARegex.Name = "lblMultiIVARegex";
        lblMultiIVARegex.Size = new Size(294, 20);
        lblMultiIVARegex.TabIndex = 1;
        lblMultiIVARegex.Text = "Regex de línea (cada match = una Factura):";
        // 
        // txtMultiIVARegex
        // 
        txtMultiIVARegex.Location = new Point(16, 72);
        txtMultiIVARegex.Multiline = true;
        txtMultiIVARegex.Name = "txtMultiIVARegex";
        txtMultiIVARegex.Size = new Size(600, 60);
        txtMultiIVARegex.TabIndex = 2;
        txtMultiIVARegex.TextChanged += ControlModificado;
        // 
        // lblMultiIVAMapeo
        // 
        lblMultiIVAMapeo.AutoSize = true;
        lblMultiIVAMapeo.Location = new Point(16, 140);
        lblMultiIVAMapeo.Name = "lblMultiIVAMapeo";
        lblMultiIVAMapeo.Size = new Size(198, 20);
        lblMultiIVAMapeo.TabIndex = 3;
        lblMultiIVAMapeo.Text = "Mapeo de grupos a campos:";
        // 
        // dgvMultiIVAMapeo
        // 
        dgvMultiIVAMapeo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMultiIVAMapeo.ColumnHeadersHeight = 29;
        dgvMultiIVAMapeo.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
        dgvMultiIVAMapeo.Location = new Point(16, 164);
        dgvMultiIVAMapeo.Name = "dgvMultiIVAMapeo";
        dgvMultiIVAMapeo.RowHeadersWidth = 51;
        dgvMultiIVAMapeo.Size = new Size(500, 200);
        dgvMultiIVAMapeo.TabIndex = 4;
        dgvMultiIVAMapeo.CellValueChanged += DgvCellValueChanged;
        dgvMultiIVAMapeo.UserAddedRow += DgvUserAddedRow;
        dgvMultiIVAMapeo.UserDeletedRow += DgvUserDeletedRow;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.HeaderText = "Campo";
        dataGridViewTextBoxColumn1.MinimumWidth = 6;
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.HeaderText = "Grupo";
        dataGridViewTextBoxColumn2.MinimumWidth = 6;
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        // 
        // tabPostProc
        // 
        tabPostProc.Controls.Add(lblPostProcReglas);
        tabPostProc.Controls.Add(lstPostProc);
        tabPostProc.Controls.Add(lblPostProcTipo);
        tabPostProc.Controls.Add(cmbPostProcTipo);
        tabPostProc.Controls.Add(lblPostProcCond);
        tabPostProc.Controls.Add(txtPostProcCondicion);
        tabPostProc.Controls.Add(lblPostProcCampos);
        tabPostProc.Controls.Add(txtPostProcCampos);
        tabPostProc.Controls.Add(btnPostProcAdd);
        tabPostProc.Controls.Add(btnPostProcRemove);
        tabPostProc.Location = new Point(4, 29);
        tabPostProc.Name = "tabPostProc";
        tabPostProc.Size = new Size(736, 627);
        tabPostProc.TabIndex = 3;
        tabPostProc.Text = "Post-Procesamiento";
        // 
        // lblPostProcReglas
        // 
        lblPostProcReglas.AutoSize = true;
        lblPostProcReglas.Location = new Point(16, 16);
        lblPostProcReglas.Name = "lblPostProcReglas";
        lblPostProcReglas.Size = new Size(56, 20);
        lblPostProcReglas.TabIndex = 0;
        lblPostProcReglas.Text = "Reglas:";
        // 
        // lstPostProc
        // 
        lstPostProc.Location = new Point(16, 38);
        lstPostProc.Name = "lstPostProc";
        lstPostProc.Size = new Size(220, 184);
        lstPostProc.TabIndex = 1;
        lstPostProc.SelectedIndexChanged += LstPostProc_SelectedIndexChanged;
        // 
        // lblPostProcTipo
        // 
        lblPostProcTipo.AutoSize = true;
        lblPostProcTipo.Location = new Point(250, 16);
        lblPostProcTipo.Name = "lblPostProcTipo";
        lblPostProcTipo.Size = new Size(42, 20);
        lblPostProcTipo.TabIndex = 2;
        lblPostProcTipo.Text = "Tipo:";
        // 
        // cmbPostProcTipo
        // 
        cmbPostProcTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPostProcTipo.Items.AddRange(new object[] { "InvertirSigno", "Mayusculas" });
        cmbPostProcTipo.Location = new Point(320, 13);
        cmbPostProcTipo.Name = "cmbPostProcTipo";
        cmbPostProcTipo.Size = new Size(200, 28);
        cmbPostProcTipo.TabIndex = 3;
        cmbPostProcTipo.SelectedIndexChanged += ControlModificado;
        // 
        // lblPostProcCond
        // 
        lblPostProcCond.AutoSize = true;
        lblPostProcCond.Location = new Point(250, 46);
        lblPostProcCond.Name = "lblPostProcCond";
        lblPostProcCond.Size = new Size(197, 20);
        lblPostProcCond.TabIndex = 4;
        lblPostProcCond.Text = "Condición (texto en factura):";
        // 
        // txtPostProcCondicion
        // 
        txtPostProcCondicion.Location = new Point(320, 43);
        txtPostProcCondicion.Name = "txtPostProcCondicion";
        txtPostProcCondicion.PlaceholderText = "Ej: ABONO (dejar vacío = siempre)";
        txtPostProcCondicion.Size = new Size(250, 27);
        txtPostProcCondicion.TabIndex = 5;
        txtPostProcCondicion.TextChanged += ControlModificado;
        // 
        // lblPostProcCampos
        // 
        lblPostProcCampos.AutoSize = true;
        lblPostProcCampos.Location = new Point(250, 76);
        lblPostProcCampos.Name = "lblPostProcCampos";
        lblPostProcCampos.Size = new Size(186, 20);
        lblPostProcCampos.TabIndex = 6;
        lblPostProcCampos.Text = "Campos afectados (coma):";
        // 
        // txtPostProcCampos
        // 
        txtPostProcCampos.Location = new Point(320, 73);
        txtPostProcCampos.Name = "txtPostProcCampos";
        txtPostProcCampos.PlaceholderText = "BaseImponible,Total";
        txtPostProcCampos.Size = new Size(250, 27);
        txtPostProcCampos.TabIndex = 7;
        txtPostProcCampos.TextChanged += ControlModificado;
        // 
        // btnPostProcAdd
        // 
        btnPostProcAdd.FlatStyle = FlatStyle.Flat;
        btnPostProcAdd.Location = new Point(16, 250);
        btnPostProcAdd.Name = "btnPostProcAdd";
        btnPostProcAdd.Size = new Size(120, 30);
        btnPostProcAdd.TabIndex = 8;
        btnPostProcAdd.Text = "+ Añadir Regla";
        btnPostProcAdd.UseVisualStyleBackColor = true;
        btnPostProcAdd.Click += BtnPostProcAdd_Click;
        // 
        // btnPostProcRemove
        // 
        btnPostProcRemove.FlatStyle = FlatStyle.Flat;
        btnPostProcRemove.Location = new Point(146, 250);
        btnPostProcRemove.Name = "btnPostProcRemove";
        btnPostProcRemove.Size = new Size(100, 30);
        btnPostProcRemove.TabIndex = 9;
        btnPostProcRemove.Text = "− Quitar";
        btnPostProcRemove.UseVisualStyleBackColor = true;
        btnPostProcRemove.Click += BtnPostProcRemove_Click;
        // 
        // btnGuardar
        // 
        btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnGuardar.BackColor = Color.FromArgb(46, 117, 182);
        btnGuardar.FlatStyle = FlatStyle.Flat;
        btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnGuardar.ForeColor = Color.White;
        btnGuardar.Location = new Point(637, 666);
        btnGuardar.Name = "btnGuardar";
        btnGuardar.Size = new Size(104, 33);
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
        btnCargarPdfMuestra.Size = new Size(223, 32);
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
        tabPaginas.Size = new Size(223, 28);
        tabPaginas.TabIndex = 1;
        tabPaginas.Visible = false;
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
        StartPosition = FormStartPosition.CenterParent;
        Text = "Gestionar Emisores";
        panelIzquierdo.ResumeLayout(false);
        panelIzquierdo.PerformLayout();
        tabs.ResumeLayout(false);
        tabGeneral.ResumeLayout(false);
        tabGeneral.PerformLayout();
        tabZonas.ResumeLayout(false);
        tabZonas.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvZonas).EndInit();
        tabCampos.ResumeLayout(false);
        tabCampos.PerformLayout();
        panelDetalle.ResumeLayout(false);
        panelDetalle.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRegexMatches).EndInit();
        tabMultiIVA.ResumeLayout(false);
        tabMultiIVA.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMultiIVAMapeo).EndInit();
        tabPostProc.ResumeLayout(false);
        tabPostProc.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picFactura).EndInit();
        panelDerecho.ResumeLayout(false);
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
    private Label lblGeneralModo;
    private ComboBox cmbModoExtraccion;
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
    private Label lblCampoGrupo;
    private TextBox txtCampoGrupo;
    private Label lblCampoValorFijo;
    private TextBox txtCampoValorFijo;
    private Label lblCampoFormato;
    private TextBox txtCampoFormatoFecha;
    private Label lblCampoSuma;
    private TextBox txtCampoCamposSuma;
    private Button btnCampoAdd;
    private Button btnCampoRemove;
    private TabPage tabMultiIVA;
    private CheckBox chkMultiIVA;
    private Label lblMultiIVARegex;
    private TextBox txtMultiIVARegex;
    private Label lblMultiIVAMapeo;
    private DataGridView dgvMultiIVAMapeo;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private TabPage tabPostProc;
    private Label lblPostProcReglas;
    private ListBox lstPostProc;
    private Label lblPostProcTipo;
    private ComboBox cmbPostProcTipo;
    private Label lblPostProcCond;
    private TextBox txtPostProcCondicion;
    private Label lblPostProcCampos;
    private TextBox txtPostProcCampos;
    private Button btnPostProcAdd;
    private Button btnPostProcRemove;
    private TabPage tabZonas;
    private Label lblZonasTitulo;
    private DataGridView dgvZonas;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
    private Label lblSeparadorRegex;
    private TextBox txtRegexSource;
    private Label lblRegexPattern;
    private TextBox txtRegexPattern;
    private Label lblRegexMatchCount;
    private Label lblRegexResultados;
    private DataGridView dgvRegexMatches;
    private Button btnRegexApplyToField;
    private Label lblSeparadorZonas = null!;
    private TextBox txtZonasSource = null!;
    protected Button btnClonar;
    private Panel panelDerecho;
    private Panel panelCentral;
    private Button btnCargarPdfMuestra;
    private TabControl tabPaginas;
    private PictureBox picFactura;
}
