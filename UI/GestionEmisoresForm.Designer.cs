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
            splitPrincipal = new SplitContainer();
            panelLista = new Panel();
            lstEmisores = new ListBox();
            txtBuscar = new TextBox();
            lblBuscar = new Label();
            panelBotonesLista = new FlowLayoutPanel();
            btnNuevo = new Button();
            btnEliminar = new Button();
            panelEditor = new Panel();
            tabsEditor = new TabControl();
            tabDatos = new TabPage();
            tabCampos = new TabPage();
            dgvCampos = new DataGridView();
            colCampoNombre = new DataGridViewTextBoxColumn();
            colCampoTipo = new DataGridViewComboBoxColumn();
            colCampoRegex = new DataGridViewTextBoxColumn();
            colCampoGrupo = new DataGridViewTextBoxColumn();
            colCampoValorFijo = new DataGridViewTextBoxColumn();
            panelBotonesCampos = new FlowLayoutPanel();
            btnAgregarCampo = new Button();
            btnEliminarCampo = new Button();
            tabReglas = new TabPage();
            dgvReglas = new DataGridView();
            colReglaNombre = new DataGridViewTextBoxColumn();
            colReglaCondicion = new DataGridViewTextBoxColumn();
            colReglaAccion = new DataGridViewTextBoxColumn();
            panelBotonesReglas = new FlowLayoutPanel();
            btnAgregarRegla = new Button();
            btnEliminarRegla = new Button();
            tabZonasOcr = new TabPage();
            dgvZonas = new DataGridView();
            colZonaCampo = new DataGridViewTextBoxColumn();
            colZonaPagina = new DataGridViewTextBoxColumn();
            colZonaX = new DataGridViewTextBoxColumn();
            colZonaY = new DataGridViewTextBoxColumn();
            colZonaAncho = new DataGridViewTextBoxColumn();
            colZonaAlto = new DataGridViewTextBoxColumn();
            panelBotonesZonas = new FlowLayoutPanel();
            btnAgregarZona = new Button();
            btnEliminarZona = new Button();
            tabTester = new TabPage();
            splitTester = new SplitContainer();
            panelTesterIzq = new Panel();
            txtTextoExtraido = new TextBox();
            lblTextoExtraido = new Label();
            btnProbarExtraccion = new Button();
            btnDetectarEmisor = new Button();
            panelRuta = new Panel();
            txtRutaPdf = new TextBox();
            btnSeleccionarPdf = new Button();
            lblRutaPdf = new Label();
            panelTesterDer = new Panel();
            dgvResultados = new DataGridView();
            colResultadoCampo = new DataGridViewTextBoxColumn();
            colResultadoValor = new DataGridViewTextBoxColumn();
            colResultadoEstado = new DataGridViewTextBoxColumn();
            lblResultados = new Label();
            panelBotonesPrincipales = new FlowLayoutPanel();
            btnCancelar = new Button();
            btnGuardar = new Button();
            lblEstado = new Label();
            lblNif = new Label();
            txtNif = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblId = new Label();
            txtId = new TextBox();
            lblConcepto = new Label();
            txtConcepto = new TextBox();
            lblIdentificadores = new Label();
            txtIdentificadores = new TextBox();
            lblModoExtraccion = new Label();
            cmbModoExtraccion = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)splitPrincipal).BeginInit();
            splitPrincipal.Panel1.SuspendLayout();
            splitPrincipal.Panel2.SuspendLayout();
            splitPrincipal.SuspendLayout();
            panelLista.SuspendLayout();
            panelBotonesLista.SuspendLayout();
            panelEditor.SuspendLayout();
            tabsEditor.SuspendLayout();
            tabCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCampos).BeginInit();
            panelBotonesCampos.SuspendLayout();
            tabReglas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReglas).BeginInit();
            panelBotonesReglas.SuspendLayout();
            tabZonasOcr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvZonas).BeginInit();
            panelBotonesZonas.SuspendLayout();
            tabTester.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitTester).BeginInit();
            splitTester.Panel1.SuspendLayout();
            splitTester.Panel2.SuspendLayout();
            splitTester.SuspendLayout();
            panelTesterIzq.SuspendLayout();
            panelRuta.SuspendLayout();
            panelTesterDer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            panelBotonesPrincipales.SuspendLayout();
            SuspendLayout();
            // 
            // splitPrincipal
            // 
            splitPrincipal.Dock = DockStyle.Fill;
            splitPrincipal.FixedPanel = FixedPanel.Panel1;
            splitPrincipal.Location = new Point(0, 0);
            splitPrincipal.Margin = new Padding(3, 4, 3, 4);
            splitPrincipal.Name = "splitPrincipal";
            // 
            // splitPrincipal.Panel1
            // 
            splitPrincipal.Panel1.Controls.Add(panelLista);
            // 
            // splitPrincipal.Panel2
            // 
            splitPrincipal.Panel2.Controls.Add(panelEditor);
            splitPrincipal.Size = new Size(1257, 933);
            splitPrincipal.SplitterDistance = 450;
            splitPrincipal.SplitterWidth = 5;
            splitPrincipal.TabIndex = 0;
            // 
            // panelLista
            // 
            panelLista.AutoSize = true;
            panelLista.Controls.Add(lstEmisores);
            panelLista.Controls.Add(txtBuscar);
            panelLista.Controls.Add(lblBuscar);
            panelLista.Controls.Add(panelBotonesLista);
            panelLista.Dock = DockStyle.Fill;
            panelLista.Location = new Point(0, 0);
            panelLista.Margin = new Padding(3, 4, 3, 4);
            panelLista.Name = "panelLista";
            panelLista.Padding = new Padding(9, 11, 9, 11);
            panelLista.Size = new Size(138, 933);
            panelLista.TabIndex = 0;
            // 
            // lstEmisores
            // 
            lstEmisores.DisplayMember = "DisplayText";
            lstEmisores.Dock = DockStyle.Fill;
            lstEmisores.Location = new Point(9, 65);
            lstEmisores.Margin = new Padding(3, 4, 3, 4);
            lstEmisores.Name = "lstEmisores";
            lstEmisores.Size = new Size(120, 809);
            lstEmisores.TabIndex = 0;
            lstEmisores.SelectedIndexChanged += LstEmisores_SelectedIndexChanged;
            // 
            // txtBuscar
            // 
            txtBuscar.Dock = DockStyle.Top;
            txtBuscar.Location = new Point(9, 38);
            txtBuscar.Margin = new Padding(3, 4, 3, 4);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Filtrar por nombre o NIF...";
            txtBuscar.Size = new Size(120, 27);
            txtBuscar.TabIndex = 1;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            // 
            // lblBuscar
            // 
            lblBuscar.Dock = DockStyle.Top;
            lblBuscar.Location = new Point(9, 11);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(120, 27);
            lblBuscar.TabIndex = 2;
            lblBuscar.Text = "Buscar:";
            // 
            // panelBotonesLista
            // 
            panelBotonesLista.Controls.Add(btnNuevo);
            panelBotonesLista.Controls.Add(btnEliminar);
            panelBotonesLista.Dock = DockStyle.Bottom;
            panelBotonesLista.Location = new Point(9, 874);
            panelBotonesLista.Margin = new Padding(3, 4, 3, 4);
            panelBotonesLista.Name = "panelBotonesLista";
            panelBotonesLista.Size = new Size(120, 48);
            panelBotonesLista.TabIndex = 3;
            // 
            // btnNuevo
            // 
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
            btnEliminar.Location = new Point(3, 52);
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
            panelEditor.Size = new Size(1114, 933);
            panelEditor.TabIndex = 0;
            // 
            // tabsEditor
            // 
            tabsEditor.Controls.Add(tabDatos);
            tabsEditor.Controls.Add(tabCampos);
            tabsEditor.Controls.Add(tabReglas);
            tabsEditor.Controls.Add(tabZonasOcr);
            tabsEditor.Controls.Add(tabTester);
            tabsEditor.Dock = DockStyle.Fill;
            tabsEditor.Location = new Point(0, 0);
            tabsEditor.Margin = new Padding(3, 4, 3, 4);
            tabsEditor.Name = "tabsEditor";
            tabsEditor.SelectedIndex = 0;
            tabsEditor.Size = new Size(1114, 874);
            tabsEditor.TabIndex = 0;
            // 
            // tabDatos
            // 
            tabDatos.AutoScroll = true;
            tabDatos.Location = new Point(4, 29);
            tabDatos.Margin = new Padding(3, 4, 3, 4);
            tabDatos.Name = "tabDatos";
            tabDatos.Padding = new Padding(14, 16, 14, 16);
            tabDatos.Size = new Size(1106, 841);
            tabDatos.TabIndex = 0;
            tabDatos.Text = "Datos";
            // 
            // tabCampos
            // 
            tabCampos.Controls.Add(dgvCampos);
            tabCampos.Controls.Add(panelBotonesCampos);
            tabCampos.Location = new Point(4, 29);
            tabCampos.Margin = new Padding(3, 4, 3, 4);
            tabCampos.Name = "tabCampos";
            tabCampos.Padding = new Padding(9, 11, 9, 11);
            tabCampos.Size = new Size(21, 42);
            tabCampos.TabIndex = 1;
            tabCampos.Text = "Campos";
            // 
            // dgvCampos
            // 
            dgvCampos.AllowUserToAddRows = false;
            dgvCampos.AllowUserToDeleteRows = false;
            dgvCampos.ColumnHeadersHeight = 29;
            dgvCampos.Columns.AddRange(new DataGridViewColumn[] { colCampoNombre, colCampoTipo, colCampoRegex, colCampoGrupo, colCampoValorFijo });
            dgvCampos.Dock = DockStyle.Fill;
            dgvCampos.Location = new Point(9, 11);
            dgvCampos.Margin = new Padding(3, 4, 3, 4);
            dgvCampos.Name = "dgvCampos";
            dgvCampos.RowHeadersVisible = false;
            dgvCampos.RowHeadersWidth = 51;
            dgvCampos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCampos.Size = new Size(3, 0);
            dgvCampos.TabIndex = 0;
            // 
            // colCampoNombre
            // 
            colCampoNombre.HeaderText = "Nombre";
            colCampoNombre.MinimumWidth = 6;
            colCampoNombre.Name = "colCampoNombre";
            colCampoNombre.Width = 120;
            // 
            // colCampoTipo
            // 
            colCampoTipo.HeaderText = "Tipo";
            colCampoTipo.Items.AddRange("Texto", "Decimal", "Fecha", "Nif", "Fijo", "Booleano", "Entero");
            colCampoTipo.MinimumWidth = 6;
            colCampoTipo.Name = "colCampoTipo";
            colCampoTipo.Width = 90;
            // 
            // colCampoRegex
            // 
            colCampoRegex.HeaderText = "Regex";
            colCampoRegex.MinimumWidth = 6;
            colCampoRegex.Name = "colCampoRegex";
            colCampoRegex.Width = 200;
            // 
            // colCampoGrupo
            // 
            colCampoGrupo.HeaderText = "Grupo";
            colCampoGrupo.MinimumWidth = 6;
            colCampoGrupo.Name = "colCampoGrupo";
            colCampoGrupo.Width = 60;
            // 
            // colCampoValorFijo
            // 
            colCampoValorFijo.HeaderText = "Valor Fijo";
            colCampoValorFijo.MinimumWidth = 6;
            colCampoValorFijo.Name = "colCampoValorFijo";
            colCampoValorFijo.Width = 120;
            // 
            // panelBotonesCampos
            // 
            panelBotonesCampos.Controls.Add(btnAgregarCampo);
            panelBotonesCampos.Controls.Add(btnEliminarCampo);
            panelBotonesCampos.Dock = DockStyle.Bottom;
            panelBotonesCampos.Location = new Point(9, -17);
            panelBotonesCampos.Margin = new Padding(3, 4, 3, 4);
            panelBotonesCampos.Name = "panelBotonesCampos";
            panelBotonesCampos.Size = new Size(3, 48);
            panelBotonesCampos.TabIndex = 1;
            // 
            // btnAgregarCampo
            // 
            btnAgregarCampo.Location = new Point(3, 4);
            btnAgregarCampo.Margin = new Padding(3, 4, 3, 4);
            btnAgregarCampo.Name = "btnAgregarCampo";
            btnAgregarCampo.Size = new Size(137, 40);
            btnAgregarCampo.TabIndex = 0;
            btnAgregarCampo.Text = "+ Agregar Campo";
            btnAgregarCampo.Click += BtnAgregarCampo_Click;
            // 
            // btnEliminarCampo
            // 
            btnEliminarCampo.Location = new Point(3, 52);
            btnEliminarCampo.Margin = new Padding(3, 4, 3, 4);
            btnEliminarCampo.Name = "btnEliminarCampo";
            btnEliminarCampo.Size = new Size(114, 40);
            btnEliminarCampo.TabIndex = 1;
            btnEliminarCampo.Text = "- Eliminar";
            btnEliminarCampo.Click += BtnEliminarCampo_Click;
            // 
            // tabReglas
            // 
            tabReglas.Controls.Add(dgvReglas);
            tabReglas.Controls.Add(panelBotonesReglas);
            tabReglas.Location = new Point(4, 29);
            tabReglas.Margin = new Padding(3, 4, 3, 4);
            tabReglas.Name = "tabReglas";
            tabReglas.Padding = new Padding(9, 11, 9, 11);
            tabReglas.Size = new Size(21, 42);
            tabReglas.TabIndex = 2;
            tabReglas.Text = "Reglas";
            // 
            // dgvReglas
            // 
            dgvReglas.AllowUserToAddRows = false;
            dgvReglas.AllowUserToDeleteRows = false;
            dgvReglas.ColumnHeadersHeight = 29;
            dgvReglas.Columns.AddRange(new DataGridViewColumn[] { colReglaNombre, colReglaCondicion, colReglaAccion });
            dgvReglas.Dock = DockStyle.Fill;
            dgvReglas.Location = new Point(9, 11);
            dgvReglas.Margin = new Padding(3, 4, 3, 4);
            dgvReglas.Name = "dgvReglas";
            dgvReglas.RowHeadersVisible = false;
            dgvReglas.RowHeadersWidth = 51;
            dgvReglas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReglas.Size = new Size(3, 0);
            dgvReglas.TabIndex = 0;
            // 
            // colReglaNombre
            // 
            colReglaNombre.HeaderText = "Nombre";
            colReglaNombre.MinimumWidth = 6;
            colReglaNombre.Name = "colReglaNombre";
            colReglaNombre.Width = 140;
            // 
            // colReglaCondicion
            // 
            colReglaCondicion.HeaderText = "Condición";
            colReglaCondicion.MinimumWidth = 6;
            colReglaCondicion.Name = "colReglaCondicion";
            colReglaCondicion.Width = 250;
            // 
            // colReglaAccion
            // 
            colReglaAccion.HeaderText = "Acción";
            colReglaAccion.MinimumWidth = 6;
            colReglaAccion.Name = "colReglaAccion";
            colReglaAccion.Width = 300;
            // 
            // panelBotonesReglas
            // 
            panelBotonesReglas.Controls.Add(btnAgregarRegla);
            panelBotonesReglas.Controls.Add(btnEliminarRegla);
            panelBotonesReglas.Dock = DockStyle.Bottom;
            panelBotonesReglas.Location = new Point(9, -17);
            panelBotonesReglas.Margin = new Padding(3, 4, 3, 4);
            panelBotonesReglas.Name = "panelBotonesReglas";
            panelBotonesReglas.Size = new Size(3, 48);
            panelBotonesReglas.TabIndex = 1;
            // 
            // btnAgregarRegla
            // 
            btnAgregarRegla.Location = new Point(3, 4);
            btnAgregarRegla.Margin = new Padding(3, 4, 3, 4);
            btnAgregarRegla.Name = "btnAgregarRegla";
            btnAgregarRegla.Size = new Size(137, 40);
            btnAgregarRegla.TabIndex = 0;
            btnAgregarRegla.Text = "+ Agregar Regla";
            btnAgregarRegla.Click += BtnAgregarRegla_Click;
            // 
            // btnEliminarRegla
            // 
            btnEliminarRegla.Location = new Point(3, 52);
            btnEliminarRegla.Margin = new Padding(3, 4, 3, 4);
            btnEliminarRegla.Name = "btnEliminarRegla";
            btnEliminarRegla.Size = new Size(114, 40);
            btnEliminarRegla.TabIndex = 1;
            btnEliminarRegla.Text = "- Eliminar";
            btnEliminarRegla.Click += BtnEliminarRegla_Click;
            // 
            // tabZonasOcr
            // 
            tabZonasOcr.Controls.Add(dgvZonas);
            tabZonasOcr.Controls.Add(panelBotonesZonas);
            tabZonasOcr.Location = new Point(4, 29);
            tabZonasOcr.Margin = new Padding(3, 4, 3, 4);
            tabZonasOcr.Name = "tabZonasOcr";
            tabZonasOcr.Padding = new Padding(9, 11, 9, 11);
            tabZonasOcr.Size = new Size(21, 42);
            tabZonasOcr.TabIndex = 3;
            tabZonasOcr.Text = "Zonas OCR";
            // 
            // dgvZonas
            // 
            dgvZonas.AllowUserToAddRows = false;
            dgvZonas.AllowUserToDeleteRows = false;
            dgvZonas.ColumnHeadersHeight = 29;
            dgvZonas.Columns.AddRange(new DataGridViewColumn[] { colZonaCampo, colZonaPagina, colZonaX, colZonaY, colZonaAncho, colZonaAlto });
            dgvZonas.Dock = DockStyle.Fill;
            dgvZonas.Location = new Point(9, 11);
            dgvZonas.Margin = new Padding(3, 4, 3, 4);
            dgvZonas.Name = "dgvZonas";
            dgvZonas.RowHeadersVisible = false;
            dgvZonas.RowHeadersWidth = 51;
            dgvZonas.Size = new Size(3, 0);
            dgvZonas.TabIndex = 0;
            // 
            // colZonaCampo
            // 
            colZonaCampo.HeaderText = "Campo";
            colZonaCampo.MinimumWidth = 6;
            colZonaCampo.Name = "colZonaCampo";
            // 
            // colZonaPagina
            // 
            colZonaPagina.HeaderText = "Pág";
            colZonaPagina.MinimumWidth = 6;
            colZonaPagina.Name = "colZonaPagina";
            colZonaPagina.Width = 50;
            // 
            // colZonaX
            // 
            colZonaX.HeaderText = "X";
            colZonaX.MinimumWidth = 6;
            colZonaX.Name = "colZonaX";
            colZonaX.Width = 80;
            // 
            // colZonaY
            // 
            colZonaY.HeaderText = "Y";
            colZonaY.MinimumWidth = 6;
            colZonaY.Name = "colZonaY";
            colZonaY.Width = 80;
            // 
            // colZonaAncho
            // 
            colZonaAncho.HeaderText = "Ancho";
            colZonaAncho.MinimumWidth = 6;
            colZonaAncho.Name = "colZonaAncho";
            colZonaAncho.Width = 80;
            // 
            // colZonaAlto
            // 
            colZonaAlto.HeaderText = "Alto";
            colZonaAlto.MinimumWidth = 6;
            colZonaAlto.Name = "colZonaAlto";
            colZonaAlto.Width = 80;
            // 
            // panelBotonesZonas
            // 
            panelBotonesZonas.Controls.Add(btnAgregarZona);
            panelBotonesZonas.Controls.Add(btnEliminarZona);
            panelBotonesZonas.Dock = DockStyle.Bottom;
            panelBotonesZonas.Location = new Point(9, -17);
            panelBotonesZonas.Margin = new Padding(3, 4, 3, 4);
            panelBotonesZonas.Name = "panelBotonesZonas";
            panelBotonesZonas.Size = new Size(3, 48);
            panelBotonesZonas.TabIndex = 1;
            // 
            // btnAgregarZona
            // 
            btnAgregarZona.Location = new Point(3, 4);
            btnAgregarZona.Margin = new Padding(3, 4, 3, 4);
            btnAgregarZona.Name = "btnAgregarZona";
            btnAgregarZona.Size = new Size(137, 40);
            btnAgregarZona.TabIndex = 0;
            btnAgregarZona.Text = "+ Agregar Zona";
            btnAgregarZona.Click += BtnAgregarZona_Click;
            // 
            // btnEliminarZona
            // 
            btnEliminarZona.Location = new Point(3, 52);
            btnEliminarZona.Margin = new Padding(3, 4, 3, 4);
            btnEliminarZona.Name = "btnEliminarZona";
            btnEliminarZona.Size = new Size(114, 40);
            btnEliminarZona.TabIndex = 1;
            btnEliminarZona.Text = "- Eliminar";
            btnEliminarZona.Click += BtnEliminarZona_Click;
            // 
            // tabTester
            // 
            tabTester.Controls.Add(splitTester);
            tabTester.Location = new Point(4, 29);
            tabTester.Margin = new Padding(3, 4, 3, 4);
            tabTester.Name = "tabTester";
            tabTester.Padding = new Padding(9, 11, 9, 11);
            tabTester.Size = new Size(21, 42);
            tabTester.TabIndex = 4;
            tabTester.Text = "Tester";
            // 
            // splitTester
            // 
            splitTester.Dock = DockStyle.Fill;
            splitTester.Location = new Point(9, 11);
            splitTester.Margin = new Padding(3, 4, 3, 4);
            splitTester.Name = "splitTester";
            // 
            // splitTester.Panel1
            // 
            splitTester.Panel1.Controls.Add(panelTesterIzq);
            // 
            // splitTester.Panel2
            // 
            splitTester.Panel2.Controls.Add(panelTesterDer);
            splitTester.Size = new Size(3, 20);
            splitTester.SplitterDistance = 169;
            splitTester.SplitterWidth = 5;
            splitTester.TabIndex = 0;
            // 
            // panelTesterIzq
            // 
            panelTesterIzq.Controls.Add(txtTextoExtraido);
            panelTesterIzq.Controls.Add(lblTextoExtraido);
            panelTesterIzq.Controls.Add(btnProbarExtraccion);
            panelTesterIzq.Controls.Add(btnDetectarEmisor);
            panelTesterIzq.Controls.Add(panelRuta);
            panelTesterIzq.Controls.Add(lblRutaPdf);
            panelTesterIzq.Dock = DockStyle.Fill;
            panelTesterIzq.Location = new Point(0, 0);
            panelTesterIzq.Margin = new Padding(3, 4, 3, 4);
            panelTesterIzq.Name = "panelTesterIzq";
            panelTesterIzq.Padding = new Padding(9, 11, 9, 11);
            panelTesterIzq.Size = new Size(169, 112);
            panelTesterIzq.TabIndex = 0;
            // 
            // txtTextoExtraido
            // 
            txtTextoExtraido.Dock = DockStyle.Fill;
            txtTextoExtraido.Font = new Font("Consolas", 9F);
            txtTextoExtraido.Location = new Point(9, 159);
            txtTextoExtraido.Margin = new Padding(3, 4, 3, 4);
            txtTextoExtraido.Multiline = true;
            txtTextoExtraido.Name = "txtTextoExtraido";
            txtTextoExtraido.ScrollBars = ScrollBars.Both;
            txtTextoExtraido.Size = new Size(151, 0);
            txtTextoExtraido.TabIndex = 0;
            txtTextoExtraido.WordWrap = false;
            // 
            // lblTextoExtraido
            // 
            lblTextoExtraido.Dock = DockStyle.Top;
            lblTextoExtraido.Location = new Point(9, 132);
            lblTextoExtraido.Name = "lblTextoExtraido";
            lblTextoExtraido.Size = new Size(151, 27);
            lblTextoExtraido.TabIndex = 1;
            lblTextoExtraido.Text = "Texto extraído del PDF:";
            // 
            // btnProbarExtraccion
            // 
            btnProbarExtraccion.Dock = DockStyle.Top;
            btnProbarExtraccion.Location = new Point(9, 85);
            btnProbarExtraccion.Margin = new Padding(3, 4, 3, 4);
            btnProbarExtraccion.Name = "btnProbarExtraccion";
            btnProbarExtraccion.Size = new Size(151, 47);
            btnProbarExtraccion.TabIndex = 2;
            btnProbarExtraccion.Text = "▶ Probar Extracción";
            btnProbarExtraccion.Click += BtnProbarExtraccion_Click;
            // 
            // btnDetectarEmisor
            // 
            btnDetectarEmisor.Dock = DockStyle.Top;
            btnDetectarEmisor.Location = new Point(9, 38);
            btnDetectarEmisor.Margin = new Padding(3, 4, 3, 4);
            btnDetectarEmisor.Name = "btnDetectarEmisor";
            btnDetectarEmisor.Size = new Size(151, 47);
            btnDetectarEmisor.TabIndex = 3;
            btnDetectarEmisor.Text = "🔍 Detectar Emisor";
            btnDetectarEmisor.Click += BtnDetectarEmisor_Click;
            // 
            // panelRuta
            // 
            panelRuta.Controls.Add(txtRutaPdf);
            panelRuta.Controls.Add(btnSeleccionarPdf);
            panelRuta.Location = new Point(0, 0);
            panelRuta.Margin = new Padding(3, 4, 3, 4);
            panelRuta.Name = "panelRuta";
            panelRuta.Size = new Size(229, 133);
            panelRuta.TabIndex = 4;
            // 
            // txtRutaPdf
            // 
            txtRutaPdf.Dock = DockStyle.Fill;
            txtRutaPdf.Location = new Point(0, 0);
            txtRutaPdf.Margin = new Padding(3, 4, 3, 4);
            txtRutaPdf.Name = "txtRutaPdf";
            txtRutaPdf.ReadOnly = true;
            txtRutaPdf.Size = new Size(183, 27);
            txtRutaPdf.TabIndex = 0;
            // 
            // btnSeleccionarPdf
            // 
            btnSeleccionarPdf.Dock = DockStyle.Right;
            btnSeleccionarPdf.Location = new Point(183, 0);
            btnSeleccionarPdf.Margin = new Padding(3, 4, 3, 4);
            btnSeleccionarPdf.Name = "btnSeleccionarPdf";
            btnSeleccionarPdf.Size = new Size(46, 133);
            btnSeleccionarPdf.TabIndex = 1;
            btnSeleccionarPdf.Text = "...";
            btnSeleccionarPdf.Click += BtnSeleccionarPdf_Click;
            // 
            // lblRutaPdf
            // 
            lblRutaPdf.Dock = DockStyle.Top;
            lblRutaPdf.Location = new Point(9, 11);
            lblRutaPdf.Name = "lblRutaPdf";
            lblRutaPdf.Size = new Size(151, 27);
            lblRutaPdf.TabIndex = 5;
            lblRutaPdf.Text = "Archivo PDF de prueba:";
            // 
            // panelTesterDer
            // 
            panelTesterDer.Controls.Add(dgvResultados);
            panelTesterDer.Controls.Add(lblResultados);
            panelTesterDer.Dock = DockStyle.Fill;
            panelTesterDer.Location = new Point(0, 0);
            panelTesterDer.Margin = new Padding(3, 4, 3, 4);
            panelTesterDer.Name = "panelTesterDer";
            panelTesterDer.Padding = new Padding(9, 11, 9, 11);
            panelTesterDer.Size = new Size(37, 112);
            panelTesterDer.TabIndex = 0;
            // 
            // dgvResultados
            // 
            dgvResultados.AllowUserToAddRows = false;
            dgvResultados.AllowUserToDeleteRows = false;
            dgvResultados.ColumnHeadersHeight = 29;
            dgvResultados.Columns.AddRange(new DataGridViewColumn[] { colResultadoCampo, colResultadoValor, colResultadoEstado });
            dgvResultados.Dock = DockStyle.Fill;
            dgvResultados.Location = new Point(9, 38);
            dgvResultados.Margin = new Padding(3, 4, 3, 4);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.ReadOnly = true;
            dgvResultados.RowHeadersVisible = false;
            dgvResultados.RowHeadersWidth = 51;
            dgvResultados.Size = new Size(19, 63);
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
            // lblResultados
            // 
            lblResultados.Dock = DockStyle.Top;
            lblResultados.Location = new Point(9, 11);
            lblResultados.Name = "lblResultados";
            lblResultados.Size = new Size(19, 27);
            lblResultados.TabIndex = 1;
            lblResultados.Text = "Resultado de la extracción:";
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
            panelBotonesPrincipales.Size = new Size(1114, 59);
            panelBotonesPrincipales.TabIndex = 1;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(979, 15);
            btnCancelar.Margin = new Padding(3, 4, 3, 4);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(114, 45);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(859, 15);
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
            lblEstado.Location = new Point(739, 11);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(114, 53);
            lblEstado.TabIndex = 2;
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblNif
            // 
            lblNif.Location = new Point(0, 0);
            lblNif.Name = "lblNif";
            lblNif.Size = new Size(100, 23);
            lblNif.TabIndex = 0;
            // 
            // txtNif
            // 
            txtNif.Location = new Point(0, 0);
            txtNif.Name = "txtNif";
            txtNif.Size = new Size(100, 27);
            txtNif.TabIndex = 0;
            // 
            // lblNombre
            // 
            lblNombre.Location = new Point(0, 0);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(100, 23);
            lblNombre.TabIndex = 0;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(0, 0);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(100, 27);
            txtNombre.TabIndex = 0;
            // 
            // lblId
            // 
            lblId.Location = new Point(0, 0);
            lblId.Name = "lblId";
            lblId.Size = new Size(100, 23);
            lblId.TabIndex = 0;
            // 
            // txtId
            // 
            txtId.Location = new Point(0, 0);
            txtId.Name = "txtId";
            txtId.Size = new Size(100, 27);
            txtId.TabIndex = 0;
            // 
            // lblConcepto
            // 
            lblConcepto.Location = new Point(0, 0);
            lblConcepto.Name = "lblConcepto";
            lblConcepto.Size = new Size(100, 23);
            lblConcepto.TabIndex = 0;
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(0, 0);
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(100, 27);
            txtConcepto.TabIndex = 0;
            // 
            // lblIdentificadores
            // 
            lblIdentificadores.Location = new Point(0, 0);
            lblIdentificadores.Name = "lblIdentificadores";
            lblIdentificadores.Size = new Size(100, 23);
            lblIdentificadores.TabIndex = 0;
            // 
            // txtIdentificadores
            // 
            txtIdentificadores.Location = new Point(0, 0);
            txtIdentificadores.Name = "txtIdentificadores";
            txtIdentificadores.Size = new Size(100, 27);
            txtIdentificadores.TabIndex = 0;
            // 
            // lblModoExtraccion
            // 
            lblModoExtraccion.Location = new Point(0, 0);
            lblModoExtraccion.Name = "lblModoExtraccion";
            lblModoExtraccion.Size = new Size(100, 23);
            lblModoExtraccion.TabIndex = 0;
            // 
            // cmbModoExtraccion
            // 
            cmbModoExtraccion.Location = new Point(0, 0);
            cmbModoExtraccion.Name = "cmbModoExtraccion";
            cmbModoExtraccion.Size = new Size(121, 28);
            cmbModoExtraccion.TabIndex = 0;
            // 
            // GestionEmisoresForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1257, 933);
            Controls.Add(splitPrincipal);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1026, 718);
            Name = "GestionEmisoresForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestión de Emisores";
            splitPrincipal.Panel1.ResumeLayout(false);
            splitPrincipal.Panel1.PerformLayout();
            splitPrincipal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPrincipal).EndInit();
            splitPrincipal.ResumeLayout(false);
            panelLista.ResumeLayout(false);
            panelLista.PerformLayout();
            panelBotonesLista.ResumeLayout(false);
            panelEditor.ResumeLayout(false);
            tabsEditor.ResumeLayout(false);
            tabCampos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCampos).EndInit();
            panelBotonesCampos.ResumeLayout(false);
            tabReglas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvReglas).EndInit();
            panelBotonesReglas.ResumeLayout(false);
            tabZonasOcr.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvZonas).EndInit();
            panelBotonesZonas.ResumeLayout(false);
            tabTester.ResumeLayout(false);
            splitTester.Panel1.ResumeLayout(false);
            splitTester.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitTester).EndInit();
            splitTester.ResumeLayout(false);
            panelTesterIzq.ResumeLayout(false);
            panelTesterIzq.PerformLayout();
            panelRuta.ResumeLayout(false);
            panelRuta.PerformLayout();
            panelTesterDer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            panelBotonesPrincipales.ResumeLayout(false);
            ResumeLayout(false);
        }

        // ── Helper methods ──

        private static void AddLabel(Control parent, string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(12, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            y += 22;
        }

        private static TextBox AddTextBox(Control parent, ref int y,
            bool readOnly = false, int width = 400)
        {
            var txt = new TextBox
            {
                Location = new Point(12, y),
                Width = width,
                ReadOnly = readOnly,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            parent.Controls.Add(txt);
            y += 28;
            return txt;
        }

        // ── Fields ──

        private SplitContainer splitPrincipal;
        private Panel panelLista;
        private TextBox txtBuscar;
        private Label lblBuscar;
        private ListBox lstEmisores;
        private FlowLayoutPanel panelBotonesLista;
        private Button btnNuevo;
        private Button btnEliminar;
        private Panel panelEditor;
        private TabControl tabsEditor;
        private TabPage tabDatos;
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
        private TabPage tabCampos;
        private DataGridView dgvCampos;
        private DataGridViewTextBoxColumn colCampoNombre;
        private DataGridViewComboBoxColumn colCampoTipo;
        private DataGridViewTextBoxColumn colCampoRegex;
        private DataGridViewTextBoxColumn colCampoGrupo;
        private DataGridViewTextBoxColumn colCampoValorFijo;
        private FlowLayoutPanel panelBotonesCampos;
        private Button btnAgregarCampo;
        private Button btnEliminarCampo;
        private TabPage tabReglas;
        private DataGridView dgvReglas;
        private DataGridViewTextBoxColumn colReglaNombre;
        private DataGridViewTextBoxColumn colReglaCondicion;
        private DataGridViewTextBoxColumn colReglaAccion;
        private FlowLayoutPanel panelBotonesReglas;
        private Button btnAgregarRegla;
        private Button btnEliminarRegla;
        private TabPage tabZonasOcr;
        private DataGridView dgvZonas;
        private DataGridViewTextBoxColumn colZonaCampo;
        private DataGridViewTextBoxColumn colZonaPagina;
        private DataGridViewTextBoxColumn colZonaX;
        private DataGridViewTextBoxColumn colZonaY;
        private DataGridViewTextBoxColumn colZonaAncho;
        private DataGridViewTextBoxColumn colZonaAlto;
        private FlowLayoutPanel panelBotonesZonas;
        private Button btnAgregarZona;
        private Button btnEliminarZona;
        private TabPage tabTester;
        private SplitContainer splitTester;
        private Panel panelTesterIzq;
        private Label lblRutaPdf;
        private TextBox txtRutaPdf;
        private Button btnSeleccionarPdf;
        private Button btnProbarExtraccion;
        private Button btnDetectarEmisor;
        private Panel panelTesterDer;
        private Label lblTextoExtraido;
        private TextBox txtTextoExtraido;
        private Label lblResultados;
        private DataGridView dgvResultados;
        private DataGridViewTextBoxColumn colResultadoCampo;
        private DataGridViewTextBoxColumn colResultadoValor;
        private DataGridViewTextBoxColumn colResultadoEstado;
        private FlowLayoutPanel panelBotonesPrincipales;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblEstado;
        private Panel panelRuta;
    }
}
