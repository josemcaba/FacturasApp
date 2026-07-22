namespace FacturasApp.UI
{
    partial class GestionEmisoresForm
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
            tableLayout = new TableLayoutPanel();
            panelLista = new Panel();
            lblBuscar = new Label();
            txtBuscar = new TextBox();
            lstEmisores = new ListBox();
            panelBotonesLista = new FlowLayoutPanel();
            btnNuevo = new Button();
            btnEliminar = new Button();
            panelDatos = new Panel();
            lblId = new Label();
            txtId = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblNif = new Label();
            txtNif = new TextBox();
            lblConcepto = new Label();
            txtConcepto = new TextBox();
            lblIdentificadores = new Label();
            txtIdentificadores = new TextBox();
            lblModoExtraccion = new Label();
            cmbModoExtraccion = new ComboBox();
            panelEditor = new Panel();
            tabsEditor = new TabControl();
            tabPruebas = new TabPage();
            splitZonas = new SplitContainer();
            panelZonasIzq = new Panel();
            panelPdfContainer = new Panel();
            picFacturaZonas = new PictureBox();
            tabPaginasZonas = new TabControl();
            lblPaginasZonas = new Label();
            btnCargarPdfZonas = new Button();
            panelZonasDer = new Panel();
            splitPruebasDer = new SplitContainer();
            txtTextoZona = new TextBox();
            btnEliminarZonaLista = new Button();
            lstZonasPagina = new ListBox();
            lblZonasPagina = new Label();
            subTabsPruebas = new TabControl();
            subTabCampos = new TabPage();
            dgvCamposPruebas = new DataGridView();
            colCampoNombrePruebas = new DataGridViewTextBoxColumn();
            colCampoTipoPruebas = new DataGridViewComboBoxColumn();
            colCampoRegexPruebas = new DataGridViewTextBoxColumn();
            colCampoGrupoPruebas = new DataGridViewTextBoxColumn();
            colCampoValorFijoPruebas = new DataGridViewTextBoxColumn();
            panelBotonesCamposPruebas = new FlowLayoutPanel();
            btnAgregarCampo = new Button();
            btnEliminarCampo = new Button();
            subTabReglas = new TabPage();
            dgvReglasPruebas = new DataGridView();
            colReglaNombrePruebas = new DataGridViewTextBoxColumn();
            colReglaCondicionPruebas = new DataGridViewTextBoxColumn();
            colReglaAccionPruebas = new DataGridViewTextBoxColumn();
            panelBotonesReglasPruebas = new FlowLayoutPanel();
            btnAgregarRegla = new Button();
            btnEliminarRegla = new Button();
            subTabResultados = new TabPage();
            panelResultadosAcciones = new Panel();
            btnDetectarEmisor = new Button();
            btnProbarExtraccion = new Button();
            lblResultados = new Label();
            dgvResultados = new DataGridView();
            colResultadoCampo = new DataGridViewTextBoxColumn();
            colResultadoValor = new DataGridViewTextBoxColumn();
            colResultadoEstado = new DataGridViewTextBoxColumn();
            panelBotonesPrincipales = new FlowLayoutPanel();
            btnCancelar = new Button();
            btnGuardar = new Button();
            lblEstado = new Label();
            tableLayout.SuspendLayout();
            panelLista.SuspendLayout();
            panelBotonesLista.SuspendLayout();
            panelDatos.SuspendLayout();
            panelEditor.SuspendLayout();
            tabsEditor.SuspendLayout();
            tabPruebas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitZonas).BeginInit();
            splitZonas.Panel1.SuspendLayout();
            splitZonas.Panel2.SuspendLayout();
            splitZonas.SuspendLayout();
            panelZonasIzq.SuspendLayout();
            panelPdfContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picFacturaZonas).BeginInit();
            panelZonasDer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPruebasDer).BeginInit();
            splitPruebasDer.Panel1.SuspendLayout();
            splitPruebasDer.Panel2.SuspendLayout();
            splitPruebasDer.SuspendLayout();
            subTabsPruebas.SuspendLayout();
            subTabCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCamposPruebas).BeginInit();
            panelBotonesCamposPruebas.SuspendLayout();
            subTabReglas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReglasPruebas).BeginInit();
            panelBotonesReglasPruebas.SuspendLayout();
            subTabResultados.SuspendLayout();
            panelResultadosAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            panelBotonesPrincipales.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayout
            // 
            tableLayout.ColumnCount = 3;
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.Location = new Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.RowCount = 1;
            tableLayout.Size = new Size(1257, 933);
            tableLayout.TabIndex = 0;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.Controls.Add(panelLista, 0, 0);
            tableLayout.Controls.Add(panelDatos, 1, 0);
            tableLayout.Controls.Add(panelEditor, 2, 0);
            // 
            // panelLista
            // 
            panelLista.Controls.Add(lblBuscar);
            panelLista.Controls.Add(txtBuscar);
            panelLista.Controls.Add(lstEmisores);
            panelLista.Controls.Add(panelBotonesLista);
            panelLista.Dock = DockStyle.Fill;
            panelLista.Name = "panelLista";
            panelLista.Padding = new Padding(9, 11, 9, 11);
            panelLista.TabIndex = 0;
            // 
            // panelDatos
            // 
            panelDatos.AutoScroll = true;
            panelDatos.Controls.Add(lblId);
            panelDatos.Controls.Add(txtId);
            panelDatos.Controls.Add(lblNombre);
            panelDatos.Controls.Add(txtNombre);
            panelDatos.Controls.Add(lblNif);
            panelDatos.Controls.Add(txtNif);
            panelDatos.Controls.Add(lblConcepto);
            panelDatos.Controls.Add(txtConcepto);
            panelDatos.Controls.Add(lblIdentificadores);
            panelDatos.Controls.Add(txtIdentificadores);
            panelDatos.Controls.Add(lblModoExtraccion);
            panelDatos.Controls.Add(cmbModoExtraccion);
            panelDatos.Dock = DockStyle.Fill;
            panelDatos.Name = "panelDatos";
            panelDatos.TabIndex = 1;
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Location = new Point(9, 14);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(55, 20);
            lblBuscar.TabIndex = 2;
            lblBuscar.Text = "Buscar:";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(70, 11);
            txtBuscar.Margin = new Padding(3, 4, 3, 4);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Filtrar por nombre o NIF...";
            txtBuscar.Size = new Size(371, 27);
            txtBuscar.TabIndex = 1;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            // 
            // lstEmisores
            // 
            lstEmisores.DisplayMember = "DisplayText";
            lstEmisores.Location = new Point(9, 54);
            lstEmisores.Margin = new Padding(3, 4, 3, 4);
            lstEmisores.Name = "lstEmisores";
            lstEmisores.Size = new Size(432, 804);
            lstEmisores.TabIndex = 0;
            lstEmisores.SelectedIndexChanged += LstEmisores_SelectedIndexChanged;
            // 
            // panelBotonesLista
            // 
            panelBotonesLista.Controls.Add(btnNuevo);
            panelBotonesLista.Controls.Add(btnEliminar);
            panelBotonesLista.Dock = DockStyle.Bottom;
            panelBotonesLista.Location = new Point(9, 874);
            panelBotonesLista.Margin = new Padding(3, 4, 3, 4);
            panelBotonesLista.Name = "panelBotonesLista";
            panelBotonesLista.Size = new Size(432, 48);
            panelBotonesLista.TabIndex = 3;
            // 
            // btnNuevo
            // 
            btnNuevo.Anchor = AnchorStyles.None;
            btnNuevo.Location = new Point(3, 4);
            btnNuevo.Margin = new Padding(3, 4, 3, 4);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(91, 40);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.Click += BtnNuevo_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = AnchorStyles.None;
            btnEliminar.Location = new Point(100, 4);
            btnEliminar.Margin = new Padding(3, 4, 3, 4);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(91, 40);
            btnEliminar.TabIndex = 1;
            btnEliminar.Text = "Eliminar";
            btnEliminar.Click += BtnEliminar_Click;
            // 
            // panelEditor
            // 
            panelEditor.Controls.Add(tabsEditor);
            panelEditor.Controls.Add(panelBotonesPrincipales);
            panelEditor.Dock = DockStyle.Fill;
            panelEditor.Location = new Point(0, 0);
            panelEditor.Margin = new Padding(3, 4, 3, 4);
            panelEditor.Name = "panelEditor";
            panelEditor.Size = new Size(802, 933);
            panelEditor.TabIndex = 0;
            // 
            // tabsEditor
            // 
            tabsEditor.Controls.Add(tabPruebas);
            tabsEditor.Dock = DockStyle.Fill;
            tabsEditor.Location = new Point(0, 0);
            tabsEditor.Margin = new Padding(3, 4, 3, 4);
            tabsEditor.Name = "tabsEditor";
            tabsEditor.SelectedIndex = 0;
            tabsEditor.Size = new Size(802, 874);
            tabsEditor.TabIndex = 0;
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(12, 12);
            lblId.Name = "lblId";
            lblId.Size = new Size(125, 20);
            lblId.TabIndex = 0;
            lblId.Text = "ID (clave interna):";
            // 
            // txtId
            // 
            txtId.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtId.Location = new Point(12, 34);
            txtId.Name = "txtId";
            txtId.ReadOnly = true;
            txtId.Size = new Size(270, 27);
            txtId.TabIndex = 1;
            // 
            // lblNombre
            // 
            lblNombre.AutoSize = true;
            lblNombre.Location = new Point(12, 70);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(141, 20);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre del emisor:";
            // 
            // txtNombre
            // 
            txtNombre.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNombre.Location = new Point(12, 92);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(270, 27);
            txtNombre.TabIndex = 3;
            // 
            // lblNif
            // 
            lblNif.AutoSize = true;
            lblNif.Location = new Point(12, 128);
            lblNif.Name = "lblNif";
            lblNif.Size = new Size(121, 20);
            lblNif.TabIndex = 4;
            lblNif.Text = "NIF (clave única):";
            // 
            // txtNif
            // 
            txtNif.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNif.Location = new Point(12, 150);
            txtNif.Name = "txtNif";
            txtNif.Size = new Size(270, 27);
            txtNif.TabIndex = 5;
            // 
            // lblConcepto
            // 
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new Point(12, 186);
            lblConcepto.Name = "lblConcepto";
            lblConcepto.Size = new Size(138, 20);
            lblConcepto.TabIndex = 6;
            lblConcepto.Text = "Concepto contable:";
            // 
            // txtConcepto
            // 
            txtConcepto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConcepto.Location = new Point(12, 208);
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(270, 27);
            txtConcepto.TabIndex = 7;
            // 
            // lblIdentificadores
            // 
            lblIdentificadores.AutoSize = true;
            lblIdentificadores.Location = new Point(12, 244);
            lblIdentificadores.Name = "lblIdentificadores";
            lblIdentificadores.Size = new Size(213, 20);
            lblIdentificadores.TabIndex = 8;
            lblIdentificadores.Text = "Identificadores (uno por línea):";
            // 
            // txtIdentificadores
            // 
            txtIdentificadores.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIdentificadores.Location = new Point(12, 266);
            txtIdentificadores.Multiline = true;
            txtIdentificadores.Name = "txtIdentificadores";
            txtIdentificadores.Size = new Size(270, 80);
            txtIdentificadores.TabIndex = 9;
            // 
            // lblModoExtraccion
            // 
            lblModoExtraccion.AutoSize = true;
            lblModoExtraccion.Location = new Point(12, 354);
            lblModoExtraccion.Name = "lblModoExtraccion";
            lblModoExtraccion.Size = new Size(145, 20);
            lblModoExtraccion.TabIndex = 10;
            lblModoExtraccion.Text = "Modo de extracción:";
            // 
            // cmbModoExtraccion
            // 
            cmbModoExtraccion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModoExtraccion.Items.AddRange(new object[] { "OrdenadoPosicion", "Simple", "LayoutAnalysis" });
            cmbModoExtraccion.Location = new Point(12, 376);
            cmbModoExtraccion.Name = "cmbModoExtraccion";
            cmbModoExtraccion.Size = new Size(200, 28);
            cmbModoExtraccion.TabIndex = 11;
            // 
            // tabPruebas
            // 
            tabPruebas.Controls.Add(splitZonas);
            tabPruebas.Location = new Point(4, 29);
            tabPruebas.Margin = new Padding(3, 4, 3, 4);
            tabPruebas.Name = "tabPruebas";
            tabPruebas.Padding = new Padding(9, 11, 9, 11);
            tabPruebas.Size = new Size(794, 841);
            tabPruebas.TabIndex = 1;
            tabPruebas.Text = "Pruebas";
            // 
            // splitZonas
            // 
            splitZonas.Dock = DockStyle.Fill;
            splitZonas.Location = new Point(9, 11);
            splitZonas.Name = "splitZonas";
            // 
            // splitZonas.Panel1
            // 
            splitZonas.Panel1.Controls.Add(panelZonasIzq);
            // 
            // splitZonas.Panel2
            // 
            splitZonas.Panel2.Controls.Add(panelZonasDer);
            splitZonas.Size = new Size(776, 819);
            splitZonas.SplitterDistance = 427;
            splitZonas.SplitterWidth = 6;
            splitZonas.TabIndex = 0;
            // 
            // panelZonasIzq
            // 
            panelZonasIzq.Controls.Add(panelPdfContainer);
            panelZonasIzq.Dock = DockStyle.Fill;
            panelZonasIzq.Location = new Point(0, 0);
            panelZonasIzq.Name = "panelZonasIzq";
            panelZonasIzq.Size = new Size(427, 819);
            panelZonasIzq.TabIndex = 0;
            // 
            // panelPdfContainer
            // 
            panelPdfContainer.Controls.Add(picFacturaZonas);
            panelPdfContainer.Controls.Add(tabPaginasZonas);
            panelPdfContainer.Controls.Add(lblPaginasZonas);
            panelPdfContainer.Controls.Add(btnCargarPdfZonas);
            panelPdfContainer.Location = new Point(0, 0);
            panelPdfContainer.Name = "panelPdfContainer";
            panelPdfContainer.Size = new Size(427, 819);
            panelPdfContainer.TabIndex = 0;
            // 
            // picFacturaZonas
            // 
            picFacturaZonas.BackColor = Color.LightGray;
            picFacturaZonas.BorderStyle = BorderStyle.FixedSingle;
            picFacturaZonas.Cursor = Cursors.Cross;
            picFacturaZonas.Dock = DockStyle.Fill;
            picFacturaZonas.Location = new Point(0, 88);
            picFacturaZonas.Name = "picFacturaZonas";
            picFacturaZonas.Size = new Size(427, 731);
            picFacturaZonas.SizeMode = PictureBoxSizeMode.Zoom;
            picFacturaZonas.TabIndex = 3;
            picFacturaZonas.TabStop = false;
            picFacturaZonas.Paint += PicFacturaZonas_Paint;
            picFacturaZonas.MouseDown += PicFacturaZonas_MouseDown;
            picFacturaZonas.MouseMove += PicFacturaZonas_MouseMove;
            picFacturaZonas.MouseUp += PicFacturaZonas_MouseUp;
            // 
            // tabPaginasZonas
            // 
            tabPaginasZonas.Dock = DockStyle.Top;
            tabPaginasZonas.Location = new Point(0, 60);
            tabPaginasZonas.Name = "tabPaginasZonas";
            tabPaginasZonas.SelectedIndex = 0;
            tabPaginasZonas.Size = new Size(427, 28);
            tabPaginasZonas.TabIndex = 2;
            tabPaginasZonas.SelectedIndexChanged += TabPaginasZonas_SelectedIndexChanged;
            // 
            // lblPaginasZonas
            // 
            lblPaginasZonas.BackColor = Color.White;
            lblPaginasZonas.Dock = DockStyle.Top;
            lblPaginasZonas.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPaginasZonas.ForeColor = Color.FromArgb(46, 117, 182);
            lblPaginasZonas.Location = new Point(0, 35);
            lblPaginasZonas.Name = "lblPaginasZonas";
            lblPaginasZonas.Size = new Size(427, 25);
            lblPaginasZonas.TabIndex = 1;
            // 
            // btnCargarPdfZonas
            // 
            btnCargarPdfZonas.BackColor = Color.FromArgb(46, 117, 182);
            btnCargarPdfZonas.Dock = DockStyle.Top;
            btnCargarPdfZonas.FlatStyle = FlatStyle.Flat;
            btnCargarPdfZonas.ForeColor = Color.White;
            btnCargarPdfZonas.Location = new Point(0, 0);
            btnCargarPdfZonas.Name = "btnCargarPdfZonas";
            btnCargarPdfZonas.Size = new Size(427, 35);
            btnCargarPdfZonas.TabIndex = 0;
            btnCargarPdfZonas.Text = "Cargar PDF de muestra";
            btnCargarPdfZonas.UseVisualStyleBackColor = false;
            btnCargarPdfZonas.Click += BtnCargarPdfZonas_Click;
            // 
            // panelZonasDer
            // 
            panelZonasDer.Controls.Add(splitPruebasDer);
            panelZonasDer.Dock = DockStyle.Fill;
            panelZonasDer.Location = new Point(0, 0);
            panelZonasDer.Name = "panelZonasDer";
            panelZonasDer.Size = new Size(343, 819);
            panelZonasDer.TabIndex = 0;
            // 
            // splitPruebasDer
            // 
            splitPruebasDer.Dock = DockStyle.Fill;
            splitPruebasDer.Location = new Point(0, 0);
            splitPruebasDer.Name = "splitPruebasDer";
            splitPruebasDer.Orientation = Orientation.Horizontal;
            // 
            // splitPruebasDer.Panel1
            // 
            splitPruebasDer.Panel1.Controls.Add(txtTextoZona);
            splitPruebasDer.Panel1.Controls.Add(btnEliminarZonaLista);
            splitPruebasDer.Panel1.Controls.Add(lstZonasPagina);
            splitPruebasDer.Panel1.Controls.Add(lblZonasPagina);
            // 
            // splitPruebasDer.Panel2
            // 
            splitPruebasDer.Panel2.Controls.Add(subTabsPruebas);
            splitPruebasDer.Size = new Size(343, 819);
            splitPruebasDer.SplitterDistance = 320;
            splitPruebasDer.TabIndex = 0;
            // 
            // txtTextoZona
            // 
            txtTextoZona.BackColor = SystemColors.Control;
            txtTextoZona.BorderStyle = BorderStyle.FixedSingle;
            txtTextoZona.Dock = DockStyle.Bottom;
            txtTextoZona.Font = new Font("Consolas", 9F);
            txtTextoZona.Location = new Point(0, 125);
            txtTextoZona.Multiline = true;
            txtTextoZona.Name = "txtTextoZona";
            txtTextoZona.ReadOnly = true;
            txtTextoZona.ScrollBars = ScrollBars.Both;
            txtTextoZona.Size = new Size(343, 165);
            txtTextoZona.TabIndex = 3;
            txtTextoZona.WordWrap = false;
            // 
            // btnEliminarZonaLista
            // 
            btnEliminarZonaLista.Dock = DockStyle.Bottom;
            btnEliminarZonaLista.FlatStyle = FlatStyle.Flat;
            btnEliminarZonaLista.Location = new Point(0, 290);
            btnEliminarZonaLista.Name = "btnEliminarZonaLista";
            btnEliminarZonaLista.Size = new Size(343, 30);
            btnEliminarZonaLista.TabIndex = 2;
            btnEliminarZonaLista.Text = "Eliminar zona seleccionada";
            btnEliminarZonaLista.Click += BtnEliminarZonaLista_Click;
            // 
            // lstZonasPagina
            // 
            lstZonasPagina.Font = new Font("Consolas", 9F);
            lstZonasPagina.FormattingEnabled = true;
            lstZonasPagina.Location = new Point(0, 25);
            lstZonasPagina.Name = "lstZonasPagina";
            lstZonasPagina.Size = new Size(343, 94);
            lstZonasPagina.TabIndex = 1;
            lstZonasPagina.SelectedIndexChanged += LstZonasPagina_SelectedIndexChanged;
            // 
            // lblZonasPagina
            // 
            lblZonasPagina.Dock = DockStyle.Top;
            lblZonasPagina.Location = new Point(0, 0);
            lblZonasPagina.Name = "lblZonasPagina";
            lblZonasPagina.Size = new Size(343, 25);
            lblZonasPagina.TabIndex = 0;
            lblZonasPagina.Text = "Zonas en página:";
            // 
            // subTabsPruebas
            // 
            subTabsPruebas.Controls.Add(subTabCampos);
            subTabsPruebas.Controls.Add(subTabReglas);
            subTabsPruebas.Controls.Add(subTabResultados);
            subTabsPruebas.Dock = DockStyle.Fill;
            subTabsPruebas.Location = new Point(0, 0);
            subTabsPruebas.Name = "subTabsPruebas";
            subTabsPruebas.SelectedIndex = 0;
            subTabsPruebas.Size = new Size(343, 495);
            subTabsPruebas.TabIndex = 0;
            // 
            // subTabCampos
            // 
            subTabCampos.Controls.Add(dgvCamposPruebas);
            subTabCampos.Controls.Add(panelBotonesCamposPruebas);
            subTabCampos.Location = new Point(4, 29);
            subTabCampos.Name = "subTabCampos";
            subTabCampos.Padding = new Padding(3);
            subTabCampos.Size = new Size(335, 462);
            subTabCampos.TabIndex = 0;
            subTabCampos.Text = "Campos";
            // 
            // dgvCamposPruebas
            // 
            dgvCamposPruebas.AllowUserToAddRows = false;
            dgvCamposPruebas.AllowUserToDeleteRows = false;
            dgvCamposPruebas.ColumnHeadersHeight = 29;
            dgvCamposPruebas.Columns.AddRange(new DataGridViewColumn[] { colCampoNombrePruebas, colCampoTipoPruebas, colCampoRegexPruebas, colCampoGrupoPruebas, colCampoValorFijoPruebas });
            dgvCamposPruebas.Dock = DockStyle.Fill;
            dgvCamposPruebas.Location = new Point(3, 3);
            dgvCamposPruebas.Name = "dgvCamposPruebas";
            dgvCamposPruebas.RowHeadersVisible = false;
            dgvCamposPruebas.RowHeadersWidth = 51;
            dgvCamposPruebas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCamposPruebas.Size = new Size(329, 408);
            dgvCamposPruebas.TabIndex = 0;
            // 
            // colCampoNombrePruebas
            // 
            colCampoNombrePruebas.HeaderText = "Nombre";
            colCampoNombrePruebas.MinimumWidth = 6;
            colCampoNombrePruebas.Name = "colCampoNombrePruebas";
            colCampoNombrePruebas.Width = 120;
            // 
            // colCampoTipoPruebas
            // 
            colCampoTipoPruebas.HeaderText = "Tipo";
            colCampoTipoPruebas.Items.AddRange(new object[] { "Texto", "Decimal", "Fecha", "Nif", "Fijo", "Booleano", "Entero" });
            colCampoTipoPruebas.MinimumWidth = 6;
            colCampoTipoPruebas.Name = "colCampoTipoPruebas";
            colCampoTipoPruebas.Width = 90;
            // 
            // colCampoRegexPruebas
            // 
            colCampoRegexPruebas.HeaderText = "Regex";
            colCampoRegexPruebas.MinimumWidth = 6;
            colCampoRegexPruebas.Name = "colCampoRegexPruebas";
            colCampoRegexPruebas.Width = 200;
            // 
            // colCampoGrupoPruebas
            // 
            colCampoGrupoPruebas.HeaderText = "Grupo";
            colCampoGrupoPruebas.MinimumWidth = 6;
            colCampoGrupoPruebas.Name = "colCampoGrupoPruebas";
            colCampoGrupoPruebas.Width = 60;
            // 
            // colCampoValorFijoPruebas
            // 
            colCampoValorFijoPruebas.HeaderText = "Valor Fijo";
            colCampoValorFijoPruebas.MinimumWidth = 6;
            colCampoValorFijoPruebas.Name = "colCampoValorFijoPruebas";
            colCampoValorFijoPruebas.Width = 120;
            // 
            // panelBotonesCamposPruebas
            // 
            panelBotonesCamposPruebas.Controls.Add(btnAgregarCampo);
            panelBotonesCamposPruebas.Controls.Add(btnEliminarCampo);
            panelBotonesCamposPruebas.Dock = DockStyle.Bottom;
            panelBotonesCamposPruebas.Location = new Point(3, 411);
            panelBotonesCamposPruebas.Name = "panelBotonesCamposPruebas";
            panelBotonesCamposPruebas.Size = new Size(329, 48);
            panelBotonesCamposPruebas.TabIndex = 1;
            // 
            // btnAgregarCampo
            // 
            btnAgregarCampo.Location = new Point(3, 3);
            btnAgregarCampo.Name = "btnAgregarCampo";
            btnAgregarCampo.Size = new Size(137, 40);
            btnAgregarCampo.TabIndex = 0;
            btnAgregarCampo.Text = "+ Agregar Campo";
            btnAgregarCampo.Click += BtnAgregarCampoPruebas_Click;
            // 
            // btnEliminarCampo
            // 
            btnEliminarCampo.Location = new Point(146, 3);
            btnEliminarCampo.Name = "btnEliminarCampo";
            btnEliminarCampo.Size = new Size(114, 40);
            btnEliminarCampo.TabIndex = 1;
            btnEliminarCampo.Text = "- Eliminar";
            btnEliminarCampo.Click += BtnEliminarCampoPruebas_Click;
            // 
            // subTabReglas
            // 
            subTabReglas.Controls.Add(dgvReglasPruebas);
            subTabReglas.Controls.Add(panelBotonesReglasPruebas);
            subTabReglas.Location = new Point(4, 29);
            subTabReglas.Name = "subTabReglas";
            subTabReglas.Padding = new Padding(3);
            subTabReglas.Size = new Size(335, 462);
            subTabReglas.TabIndex = 1;
            subTabReglas.Text = "Reglas";
            // 
            // dgvReglasPruebas
            // 
            dgvReglasPruebas.AllowUserToAddRows = false;
            dgvReglasPruebas.AllowUserToDeleteRows = false;
            dgvReglasPruebas.ColumnHeadersHeight = 29;
            dgvReglasPruebas.Columns.AddRange(new DataGridViewColumn[] { colReglaNombrePruebas, colReglaCondicionPruebas, colReglaAccionPruebas });
            dgvReglasPruebas.Dock = DockStyle.Fill;
            dgvReglasPruebas.Location = new Point(3, 3);
            dgvReglasPruebas.Name = "dgvReglasPruebas";
            dgvReglasPruebas.RowHeadersVisible = false;
            dgvReglasPruebas.RowHeadersWidth = 51;
            dgvReglasPruebas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReglasPruebas.Size = new Size(329, 408);
            dgvReglasPruebas.TabIndex = 0;
            // 
            // colReglaNombrePruebas
            // 
            colReglaNombrePruebas.HeaderText = "Nombre";
            colReglaNombrePruebas.MinimumWidth = 6;
            colReglaNombrePruebas.Name = "colReglaNombrePruebas";
            colReglaNombrePruebas.Width = 140;
            // 
            // colReglaCondicionPruebas
            // 
            colReglaCondicionPruebas.HeaderText = "Condición";
            colReglaCondicionPruebas.MinimumWidth = 6;
            colReglaCondicionPruebas.Name = "colReglaCondicionPruebas";
            colReglaCondicionPruebas.Width = 250;
            // 
            // colReglaAccionPruebas
            // 
            colReglaAccionPruebas.HeaderText = "Acción";
            colReglaAccionPruebas.MinimumWidth = 6;
            colReglaAccionPruebas.Name = "colReglaAccionPruebas";
            colReglaAccionPruebas.Width = 300;
            // 
            // panelBotonesReglasPruebas
            // 
            panelBotonesReglasPruebas.Controls.Add(btnAgregarRegla);
            panelBotonesReglasPruebas.Controls.Add(btnEliminarRegla);
            panelBotonesReglasPruebas.Dock = DockStyle.Bottom;
            panelBotonesReglasPruebas.Location = new Point(3, 411);
            panelBotonesReglasPruebas.Name = "panelBotonesReglasPruebas";
            panelBotonesReglasPruebas.Size = new Size(329, 48);
            panelBotonesReglasPruebas.TabIndex = 1;
            // 
            // btnAgregarRegla
            // 
            btnAgregarRegla.Location = new Point(3, 3);
            btnAgregarRegla.Name = "btnAgregarRegla";
            btnAgregarRegla.Size = new Size(137, 40);
            btnAgregarRegla.TabIndex = 0;
            btnAgregarRegla.Text = "+ Agregar Regla";
            btnAgregarRegla.Click += BtnAgregarReglaPruebas_Click;
            // 
            // btnEliminarRegla
            // 
            btnEliminarRegla.Location = new Point(146, 3);
            btnEliminarRegla.Name = "btnEliminarRegla";
            btnEliminarRegla.Size = new Size(114, 40);
            btnEliminarRegla.TabIndex = 1;
            btnEliminarRegla.Text = "- Eliminar";
            btnEliminarRegla.Click += BtnEliminarReglaPruebas_Click;
            // 
            // subTabResultados
            // 
            subTabResultados.Controls.Add(panelResultadosAcciones);
            subTabResultados.Controls.Add(lblResultados);
            subTabResultados.Controls.Add(dgvResultados);
            subTabResultados.Location = new Point(4, 29);
            subTabResultados.Name = "subTabResultados";
            subTabResultados.Padding = new Padding(3);
            subTabResultados.Size = new Size(335, 462);
            subTabResultados.TabIndex = 2;
            subTabResultados.Text = "Resultados";
            // 
            // panelResultadosAcciones
            // 
            panelResultadosAcciones.Controls.Add(btnDetectarEmisor);
            panelResultadosAcciones.Controls.Add(btnProbarExtraccion);
            panelResultadosAcciones.Dock = DockStyle.Bottom;
            panelResultadosAcciones.Location = new Point(3, 402);
            panelResultadosAcciones.Name = "panelResultadosAcciones";
            panelResultadosAcciones.Size = new Size(329, 57);
            panelResultadosAcciones.TabIndex = 2;
            // 
            // btnDetectarEmisor
            // 
            btnDetectarEmisor.Location = new Point(173, 4);
            btnDetectarEmisor.Name = "btnDetectarEmisor";
            btnDetectarEmisor.Size = new Size(160, 47);
            btnDetectarEmisor.TabIndex = 0;
            btnDetectarEmisor.Text = "🔍 Detectar Emisor";
            btnDetectarEmisor.Click += BtnDetectarEmisorPruebas_Click;
            // 
            // btnProbarExtraccion
            // 
            btnProbarExtraccion.Location = new Point(3, 4);
            btnProbarExtraccion.Name = "btnProbarExtraccion";
            btnProbarExtraccion.Size = new Size(164, 47);
            btnProbarExtraccion.TabIndex = 1;
            btnProbarExtraccion.Text = "▶ Probar Extracción";
            btnProbarExtraccion.Click += BtnProbarExtraccionPruebas_Click;
            // 
            // lblResultados
            // 
            lblResultados.Dock = DockStyle.Top;
            lblResultados.Location = new Point(3, 3);
            lblResultados.Name = "lblResultados";
            lblResultados.Size = new Size(329, 27);
            lblResultados.TabIndex = 1;
            lblResultados.Text = "Resultado de la extracción:";
            lblResultados.Visible = false;
            // 
            // dgvResultados
            // 
            dgvResultados.AllowUserToAddRows = false;
            dgvResultados.AllowUserToDeleteRows = false;
            dgvResultados.ColumnHeadersHeight = 29;
            dgvResultados.Columns.AddRange(new DataGridViewColumn[] { colResultadoCampo, colResultadoValor, colResultadoEstado });
            dgvResultados.Dock = DockStyle.Fill;
            dgvResultados.Location = new Point(3, 3);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.ReadOnly = true;
            dgvResultados.RowHeadersVisible = false;
            dgvResultados.RowHeadersWidth = 51;
            dgvResultados.Size = new Size(329, 456);
            dgvResultados.TabIndex = 0;
            // 
            // colResultadoCampo
            // 
            colResultadoCampo.HeaderText = "Campo";
            colResultadoCampo.MinimumWidth = 6;
            colResultadoCampo.Name = "colResultadoCampo";
            colResultadoCampo.ReadOnly = true;
            colResultadoCampo.Width = 130;
            // 
            // colResultadoValor
            // 
            colResultadoValor.HeaderText = "Valor Extraído";
            colResultadoValor.MinimumWidth = 6;
            colResultadoValor.Name = "colResultadoValor";
            colResultadoValor.ReadOnly = true;
            colResultadoValor.Width = 200;
            // 
            // colResultadoEstado
            // 
            colResultadoEstado.HeaderText = "Estado";
            colResultadoEstado.MinimumWidth = 6;
            colResultadoEstado.Name = "colResultadoEstado";
            colResultadoEstado.ReadOnly = true;
            colResultadoEstado.Width = 80;
            // 
            // panelBotonesPrincipales
            // 
            panelBotonesPrincipales.Controls.Add(btnCancelar);
            panelBotonesPrincipales.Controls.Add(btnGuardar);
            panelBotonesPrincipales.Controls.Add(lblEstado);
            panelBotonesPrincipales.Dock = DockStyle.Bottom;
            panelBotonesPrincipales.FlowDirection = FlowDirection.RightToLeft;
            panelBotonesPrincipales.Location = new Point(0, 874);
            panelBotonesPrincipales.Margin = new Padding(3, 4, 3, 4);
            panelBotonesPrincipales.Name = "panelBotonesPrincipales";
            panelBotonesPrincipales.Padding = new Padding(9, 11, 9, 11);
            panelBotonesPrincipales.Size = new Size(802, 59);
            panelBotonesPrincipales.TabIndex = 1;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(667, 15);
            btnCancelar.Margin = new Padding(3, 4, 3, 4);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(114, 45);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(547, 15);
            btnGuardar.Margin = new Padding(3, 4, 3, 4);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(114, 45);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // lblEstado
            // 
            lblEstado.Dock = DockStyle.Fill;
            lblEstado.ForeColor = Color.DarkGreen;
            lblEstado.Location = new Point(427, 11);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(114, 53);
            lblEstado.TabIndex = 2;
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // GestionEmisoresForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1257, 933);
            Controls.Add(tableLayout);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1026, 718);
            Name = "GestionEmisoresForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestión de Emisores";
            tableLayout.ResumeLayout(false);
            panelLista.ResumeLayout(false);
            panelLista.PerformLayout();
            panelDatos.ResumeLayout(false);
            panelDatos.PerformLayout();
            panelBotonesLista.ResumeLayout(false);
            panelEditor.ResumeLayout(false);
            tabsEditor.ResumeLayout(false);
            tabPruebas.ResumeLayout(false);
            splitZonas.Panel1.ResumeLayout(false);
            splitZonas.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitZonas).EndInit();
            splitZonas.ResumeLayout(false);
            panelZonasIzq.ResumeLayout(false);
            panelPdfContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picFacturaZonas).EndInit();
            panelZonasDer.ResumeLayout(false);
            splitPruebasDer.Panel1.ResumeLayout(false);
            splitPruebasDer.Panel1.PerformLayout();
            splitPruebasDer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPruebasDer).EndInit();
            splitPruebasDer.ResumeLayout(false);
            subTabsPruebas.ResumeLayout(false);
            subTabCampos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCamposPruebas).EndInit();
            panelBotonesCamposPruebas.ResumeLayout(false);
            subTabReglas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvReglasPruebas).EndInit();
            panelBotonesReglasPruebas.ResumeLayout(false);
            subTabResultados.ResumeLayout(false);
            panelResultadosAcciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            panelBotonesPrincipales.ResumeLayout(false);
            ResumeLayout(false);
        }

        // ── Fields ──

        private TableLayoutPanel tableLayout;
        private Panel panelLista;
        private Panel panelDatos;
        private TextBox txtBuscar;
        private Label lblBuscar;
        private ListBox lstEmisores;
        private FlowLayoutPanel panelBotonesLista;
        private Button btnNuevo;
        private Button btnEliminar;
        private Panel panelEditor;
        private TabControl tabsEditor;
        private Label lblNif;
        private TextBox txtNif;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblId;
        private TextBox txtId;
        private Label lblConcepto;
        private TextBox txtConcepto;
        private Label lblIdentificadores;
        private TextBox txtIdentificadores;
        private Label lblModoExtraccion;
        private ComboBox cmbModoExtraccion;
        private Button btnAgregarCampo;
        private Button btnEliminarCampo;
        private Button btnAgregarRegla;
        private Button btnEliminarRegla;
        private Button btnProbarExtraccion;
        private Button btnDetectarEmisor;
        private Label lblResultados;
        private DataGridView dgvResultados;
        private DataGridViewTextBoxColumn colResultadoCampo;
        private DataGridViewTextBoxColumn colResultadoValor;
        private DataGridViewTextBoxColumn colResultadoEstado;
        private FlowLayoutPanel panelBotonesPrincipales;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblEstado;
        private SplitContainer splitZonas;
        private Panel panelZonasIzq;
        private Button btnCargarPdfZonas;
        private Label lblPaginasZonas;
        private TabControl tabPaginasZonas;
        private PictureBox picFacturaZonas;
        private Label lblZonasPagina;
        private ListBox lstZonasPagina;
        private Button btnEliminarZonaLista;
        private TextBox txtTextoZona;
        private Panel panelPdfContainer;
        private Panel panelZonasDer;
        private TabPage tabPruebas;
        private SplitContainer splitPruebasDer;
        private TabControl subTabsPruebas;
        private TabPage subTabCampos;
        private TabPage subTabReglas;
        private TabPage subTabResultados;
        private DataGridView dgvCamposPruebas;
        private DataGridViewTextBoxColumn colCampoNombrePruebas;
        private DataGridViewComboBoxColumn colCampoTipoPruebas;
        private DataGridViewTextBoxColumn colCampoRegexPruebas;
        private DataGridViewTextBoxColumn colCampoGrupoPruebas;
        private DataGridViewTextBoxColumn colCampoValorFijoPruebas;
        private FlowLayoutPanel panelBotonesCamposPruebas;
        private DataGridView dgvReglasPruebas;
        private DataGridViewTextBoxColumn colReglaNombrePruebas;
        private DataGridViewTextBoxColumn colReglaCondicionPruebas;
        private DataGridViewTextBoxColumn colReglaAccionPruebas;
        private FlowLayoutPanel panelBotonesReglasPruebas;
        private Panel panelResultadosAcciones;
    }
}
