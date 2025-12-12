namespace MapGISPlugin3
{
    partial class Form_TEMProcess
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.panelDepthControl = new System.Windows.Forms.Panel();
            this.nudMaxDepth1 = new System.Windows.Forms.NumericUpDown();
            this.lblMaxDepth1 = new System.Windows.Forms.Label();
            this.nudMaxDepth = new System.Windows.Forms.NumericUpDown();
            this.lblMaxDepth = new System.Windows.Forms.Label();
            this.timerProgress = new System.Windows.Forms.Timer(this.components);
            this.lblTimeInfo = new System.Windows.Forms.Label();
            this.trackBarTime = new System.Windows.Forms.TrackBar();
            this.chartZProfile = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.gridZData = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tabControlLeft = new System.Windows.Forms.TabControl();
            this.tabPageLayout = new System.Windows.Forms.TabPage();
            this.gridData = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbLineName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbStationLayer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.progressBarCalculate = new System.Windows.Forms.ProgressBar();
            this.chartProfileView = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPageParams = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabPageCurve = new System.Windows.Forms.TabPage();
            this.tabPageDataList = new System.Windows.Forms.TabPage();
            this.chartVoltage = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.splitContainerRight = new System.Windows.Forms.SplitContainer();
            this.button3 = new System.Windows.Forms.Button();
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button1 = new System.Windows.Forms.Button();
            this.panelDepthControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartZProfile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridZData)).BeginInit();
            this.panel1.SuspendLayout();
            this.tabControlLeft.SuspendLayout();
            this.tabPageLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).BeginInit();
            this.tabPageParams.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPageCurve.SuspendLayout();
            this.tabPageDataList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
            this.splitContainerRight.Panel1.SuspendLayout();
            this.splitContainerRight.Panel2.SuspendLayout();
            this.splitContainerRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).BeginInit();
            this.SuspendLayout();
            // 
            // panelDepthControl
            // 
            this.panelDepthControl.BackColor = System.Drawing.SystemColors.Control;
            this.panelDepthControl.Controls.Add(this.nudMaxDepth1);
            this.panelDepthControl.Controls.Add(this.lblMaxDepth1);
            this.panelDepthControl.Controls.Add(this.nudMaxDepth);
            this.panelDepthControl.Controls.Add(this.lblMaxDepth);
            this.panelDepthControl.Location = new System.Drawing.Point(-2, -2);
            this.panelDepthControl.Name = "panelDepthControl";
            this.panelDepthControl.Size = new System.Drawing.Size(268, 28);
            this.panelDepthControl.TabIndex = 1;
            // 
            // nudMaxDepth1
            // 
            this.nudMaxDepth1.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudMaxDepth1.Location = new System.Drawing.Point(129, 0);
            this.nudMaxDepth1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudMaxDepth1.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMaxDepth1.Name = "nudMaxDepth1";
            this.nudMaxDepth1.Size = new System.Drawing.Size(135, 28);
            this.nudMaxDepth1.TabIndex = 0;
            this.nudMaxDepth1.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxDepth1.ValueChanged += new System.EventHandler(this.nudMaxDepth1_ValueChanged);
            // 
            // lblMaxDepth1
            // 
            this.lblMaxDepth1.AutoSize = true;
            this.lblMaxDepth1.BackColor = System.Drawing.SystemColors.Control;
            this.lblMaxDepth1.ForeColor = System.Drawing.Color.Black;
            this.lblMaxDepth1.Location = new System.Drawing.Point(4, 8);
            this.lblMaxDepth1.Name = "lblMaxDepth1";
            this.lblMaxDepth1.Size = new System.Drawing.Size(134, 18);
            this.lblMaxDepth1.TabIndex = 3;
            this.lblMaxDepth1.Text = "最大显示深度：";
            // 
            // nudMaxDepth
            // 
            this.nudMaxDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMaxDepth.BackColor = System.Drawing.Color.White;
            this.nudMaxDepth.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudMaxDepth.Location = new System.Drawing.Point(688, 6);
            this.nudMaxDepth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudMaxDepth.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMaxDepth.Name = "nudMaxDepth";
            this.nudMaxDepth.Size = new System.Drawing.Size(120, 28);
            this.nudMaxDepth.TabIndex = 2;
            this.nudMaxDepth.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // lblMaxDepth
            // 
            this.lblMaxDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaxDepth.BackColor = System.Drawing.Color.Transparent;
            this.lblMaxDepth.Location = new System.Drawing.Point(530, 6);
            this.lblMaxDepth.Name = "lblMaxDepth";
            this.lblMaxDepth.Size = new System.Drawing.Size(152, 28);
            this.lblMaxDepth.TabIndex = 1;
            this.lblMaxDepth.Text = "最大显示深度(m):";
            this.lblMaxDepth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // timerProgress
            // 
            this.timerProgress.Interval = 500;
            this.timerProgress.Tick += new System.EventHandler(this.timerProgress_Tick);
            // 
            // lblTimeInfo
            // 
            this.lblTimeInfo.AutoSize = true;
            this.lblTimeInfo.Location = new System.Drawing.Point(20, 82);
            this.lblTimeInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTimeInfo.Name = "lblTimeInfo";
            this.lblTimeInfo.Size = new System.Drawing.Size(188, 18);
            this.lblTimeInfo.TabIndex = 2;
            this.lblTimeInfo.Text = "当前采样时间: 未选择";
            // 
            // trackBarTime
            // 
            this.trackBarTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarTime.Location = new System.Drawing.Point(14, 14);
            this.trackBarTime.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarTime.Name = "trackBarTime";
            this.trackBarTime.Size = new System.Drawing.Size(650, 69);
            this.trackBarTime.TabIndex = 1;
            this.trackBarTime.TickStyle = System.Windows.Forms.TickStyle.Both;
            // 
            // chartZProfile
            // 
            this.chartZProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Title = "距离 (m)";
            chartArea1.AxisY.Title = "感应电动势 (mV)";
            chartArea1.Name = "ChartArea1";
            this.chartZProfile.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "Legend1";
            this.chartZProfile.Legends.Add(legend1);
            this.chartZProfile.Location = new System.Drawing.Point(0, 112);
            this.chartZProfile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chartZProfile.Name = "chartZProfile";
            this.chartZProfile.Size = new System.Drawing.Size(663, 748);
            this.chartZProfile.TabIndex = 0;
            this.chartZProfile.Text = "Z Profile Chart";
            // 
            // gridZData
            // 
            this.gridZData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridZData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridZData.Location = new System.Drawing.Point(0, 0);
            this.gridZData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gridZData.Name = "gridZData";
            this.gridZData.RowHeadersWidth = 62;
            this.gridZData.RowTemplate.Height = 23;
            this.gridZData.Size = new System.Drawing.Size(667, 866);
            this.gridZData.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1650, 48);
            this.panel1.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(132, 48);
            this.label7.TabIndex = 1;
            this.label7.Text = "TEM一维反演";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Right;
            this.button2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(1606, 0);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(44, 48);
            this.button2.TabIndex = 0;
            this.button2.Text = "X";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabControlLeft
            // 
            this.tabControlLeft.Controls.Add(this.tabPageLayout);
            this.tabControlLeft.Controls.Add(this.tabPageParams);
            this.tabControlLeft.Controls.Add(this.tabPageCurve);
            this.tabControlLeft.Controls.Add(this.tabPageDataList);
            this.tabControlLeft.Location = new System.Drawing.Point(8, 57);
            this.tabControlLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControlLeft.Name = "tabControlLeft";
            this.tabControlLeft.SelectedIndex = 0;
            this.tabControlLeft.Size = new System.Drawing.Size(675, 898);
            this.tabControlLeft.TabIndex = 0;
            // 
            // tabPageLayout
            // 
            this.tabPageLayout.Controls.Add(this.gridData);
            this.tabPageLayout.Controls.Add(this.groupBox1);
            this.tabPageLayout.Controls.Add(this.btnCalculate);
            this.tabPageLayout.Controls.Add(this.progressBarCalculate);
            this.tabPageLayout.Controls.Add(this.chartProfileView);
            this.tabPageLayout.Location = new System.Drawing.Point(4, 28);
            this.tabPageLayout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageLayout.Name = "tabPageLayout";
            this.tabPageLayout.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageLayout.Size = new System.Drawing.Size(667, 866);
            this.tabPageLayout.TabIndex = 0;
            this.tabPageLayout.Text = "布置图";
            this.tabPageLayout.UseVisualStyleBackColor = true;
            // 
            // gridData
            // 
            this.gridData.AllowUserToAddRows = false;
            this.gridData.AllowUserToDeleteRows = false;
            this.gridData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridData.Location = new System.Drawing.Point(9, 9);
            this.gridData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gridData.Name = "gridData";
            this.gridData.ReadOnly = true;
            this.gridData.RowHeadersWidth = 62;
            this.gridData.RowTemplate.Height = 23;
            this.gridData.Size = new System.Drawing.Size(658, 152);
            this.gridData.TabIndex = 25;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.cmbLineName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmbStationLayer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(9, 170);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(658, 104);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据选择";
            // 
            // cmbLineName
            // 
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cmbLineName.Location = new System.Drawing.Point(350, 51);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(246, 26);
            this.cmbLineName.TabIndex = 3;
            this.cmbLineName.SelectedIndexChanged += new System.EventHandler(this.cmbLineName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(345, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "测线：";
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStationLayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cmbStationLayer.Location = new System.Drawing.Point(39, 51);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(246, 26);
            this.cmbStationLayer.TabIndex = 1;
            this.cmbStationLayer.SelectedIndexChanged += new System.EventHandler(this.cmbStationLayer_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "测点图层：";
            // 
            // btnCalculate
            // 
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(48, 280);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(150, 45);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "开始计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // progressBarCalculate
            // 
            this.progressBarCalculate.Location = new System.Drawing.Point(214, 290);
            this.progressBarCalculate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBarCalculate.Name = "progressBarCalculate";
            this.progressBarCalculate.Size = new System.Drawing.Size(424, 27);
            this.progressBarCalculate.TabIndex = 26;
            // 
            // chartProfileView
            // 
            chartArea2.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea2);
            legend2.Alignment = System.Drawing.StringAlignment.Far;
            legend2.BackColor = System.Drawing.SystemColors.Control;
            legend2.BorderColor = System.Drawing.Color.LightGray;
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend2.Font = new System.Drawing.Font("微软雅黑", 8F);
            legend2.IsTextAutoFit = false;
            legend2.Name = "LegendProfile";
            this.chartProfileView.Legends.Add(legend2);
            this.chartProfileView.Location = new System.Drawing.Point(0, 339);
            this.chartProfileView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chartProfileView.Name = "chartProfileView";
            this.chartProfileView.Size = new System.Drawing.Size(668, 532);
            this.chartProfileView.TabIndex = 23;
            this.chartProfileView.Text = "chartProfileView";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // tabPageParams
            // 
            this.tabPageParams.Controls.Add(this.groupBox2);
            this.tabPageParams.Location = new System.Drawing.Point(4, 28);
            this.tabPageParams.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageParams.Name = "tabPageParams";
            this.tabPageParams.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageParams.Size = new System.Drawing.Size(667, 866);
            this.tabPageParams.TabIndex = 1;
            this.tabPageParams.Text = "模型与反演控制参数";
            this.tabPageParams.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBox5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBox6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBox4);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Location = new System.Drawing.Point(21, 45);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(622, 328);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "可选参数调整";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(140, 36);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(179, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "模型光滑因子(0.001)";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Control;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox5.Location = new System.Drawing.Point(338, 270);
            this.textBox5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(146, 28);
            this.textBox5.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(184, 80);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 18);
            this.label4.TabIndex = 1;
            this.label4.Text = "反演深度(1000)";
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Control;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox6.Location = new System.Drawing.Point(338, 220);
            this.textBox6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(146, 28);
            this.textBox6.TabIndex = 12;
            this.textBox6.Text = "0.05";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 123);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(215, 18);
            this.label5.TabIndex = 2;
            this.label5.Text = "模型层厚度递增因子(1.1)";
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox3.Location = new System.Drawing.Point(338, 171);
            this.textBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(146, 28);
            this.textBox3.TabIndex = 11;
            this.textBox3.Text = "500";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(142, 174);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(179, 18);
            this.label6.TabIndex = 3;
            this.label6.Text = "模型初始电阻率(500)";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox4.Location = new System.Drawing.Point(338, 120);
            this.textBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(146, 28);
            this.textBox4.TabIndex = 10;
            this.textBox4.Text = "1.1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(166, 273);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(152, 18);
            this.label9.TabIndex = 6;
            this.label9.Text = "当前测点拟合误差";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox2.Location = new System.Drawing.Point(338, 74);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(146, 28);
            this.textBox2.TabIndex = 9;
            this.textBox2.Text = "1000";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(148, 224);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(170, 18);
            this.label8.TabIndex = 7;
            this.label8.Text = "期望拟合误差(0.05)";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(338, 33);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(146, 28);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = "0.001";
            // 
            // tabPageCurve
            // 
            this.tabPageCurve.Controls.Add(this.lblTimeInfo);
            this.tabPageCurve.Controls.Add(this.trackBarTime);
            this.tabPageCurve.Controls.Add(this.chartZProfile);
            this.tabPageCurve.Location = new System.Drawing.Point(4, 28);
            this.tabPageCurve.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageCurve.Name = "tabPageCurve";
            this.tabPageCurve.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageCurve.Size = new System.Drawing.Size(667, 866);
            this.tabPageCurve.TabIndex = 2;
            this.tabPageCurve.Text = "感应电动势剖面曲线图";
            this.tabPageCurve.UseVisualStyleBackColor = true;
            // 
            // tabPageDataList
            // 
            this.tabPageDataList.Controls.Add(this.gridZData);
            this.tabPageDataList.Location = new System.Drawing.Point(4, 28);
            this.tabPageDataList.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageDataList.Name = "tabPageDataList";
            this.tabPageDataList.Size = new System.Drawing.Size(667, 866);
            this.tabPageDataList.TabIndex = 3;
            this.tabPageDataList.Text = "感应电动势数据列表";
            this.tabPageDataList.UseVisualStyleBackColor = true;
            // 
            // chartVoltage
            // 
            chartArea3.Name = "ChartArea1";
            this.chartVoltage.ChartAreas.Add(chartArea3);
            this.chartVoltage.Dock = System.Windows.Forms.DockStyle.Fill;
            legend3.Alignment = System.Drawing.StringAlignment.Far;
            legend3.BackColor = System.Drawing.SystemColors.Control;
            legend3.BorderColor = System.Drawing.Color.LightGray;
            legend3.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend3.Font = new System.Drawing.Font("微软雅黑", 8F);
            legend3.IsTextAutoFit = false;
            legend3.Name = "LegendVoltage";
            this.chartVoltage.Legends.Add(legend3);
            this.chartVoltage.Location = new System.Drawing.Point(0, 0);
            this.chartVoltage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chartVoltage.Name = "chartVoltage";
            this.chartVoltage.Size = new System.Drawing.Size(930, 437);
            this.chartVoltage.TabIndex = 24;
            this.chartVoltage.Text = "chartVoltage";
            // 
            // splitContainerRight
            // 
            this.splitContainerRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerRight.Location = new System.Drawing.Point(692, 52);
            this.splitContainerRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainerRight.Name = "splitContainerRight";
            this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            this.splitContainerRight.Panel1.Controls.Add(this.chartVoltage);
            // 
            // splitContainerRight.Panel2
            // 
            this.splitContainerRight.Panel2.Controls.Add(this.button3);
            this.splitContainerRight.Panel2.Controls.Add(this.panelDepthControl);
            this.splitContainerRight.Panel2.Controls.Add(this.chartResistivity);
            this.splitContainerRight.Size = new System.Drawing.Size(932, 898);
            this.splitContainerRight.SplitterDistance = 439;
            this.splitContainerRight.SplitterWidth = 6;
            this.splitContainerRight.TabIndex = 24;
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Location = new System.Drawing.Point(816, 0);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 30);
            this.button3.TabIndex = 2;
            this.button3.Text = "一键成图";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // chartResistivity
            // 
            this.chartResistivity.BackColor = System.Drawing.SystemColors.Control;
            this.chartResistivity.BorderlineColor = System.Drawing.SystemColors.Control;
            chartArea4.AxisX.Title = "距离 x(m)";
            chartArea4.AxisY.Title = "深度 z(m)";
            chartArea4.CursorX.IsUserEnabled = true;
            chartArea4.CursorX.IsUserSelectionEnabled = true;
            chartArea4.CursorY.IsUserEnabled = true;
            chartArea4.CursorY.IsUserSelectionEnabled = true;
            chartArea4.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea4);
            legend4.Enabled = false;
            legend4.Name = "Legend1";
            this.chartResistivity.Legends.Add(legend4);
            this.chartResistivity.Location = new System.Drawing.Point(-2, 27);
            this.chartResistivity.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chartResistivity.Name = "chartResistivity";
            this.chartResistivity.Size = new System.Drawing.Size(932, 424);
            this.chartResistivity.TabIndex = 0;
            this.chartResistivity.Text = "Resistivity Section";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(560, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 29);
            this.button1.TabIndex = 5;
            this.button1.Text = "测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form_TEMProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1650, 975);
            this.Controls.Add(this.splitContainerRight);
            this.Controls.Add(this.tabControlLeft);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(1650, 975);
            this.MinimumSize = new System.Drawing.Size(1650, 975);
            this.Name = "Form_TEMProcess";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.Load += new System.EventHandler(this.Form_TEMProcess_Load);
            this.panelDepthControl.ResumeLayout(false);
            this.panelDepthControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartZProfile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridZData)).EndInit();
            this.panel1.ResumeLayout(false);
            this.tabControlLeft.ResumeLayout(false);
            this.tabPageLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).EndInit();
            this.tabPageParams.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPageCurve.ResumeLayout(false);
            this.tabPageCurve.PerformLayout();
            this.tabPageDataList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltage)).EndInit();
            this.splitContainerRight.Panel1.ResumeLayout(false);
            this.splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
            this.splitContainerRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button2;

        // --- 新增变量 ---
        private System.Windows.Forms.TabControl tabControlLeft;
        private System.Windows.Forms.TabPage tabPageLayout;
        private System.Windows.Forms.TabPage tabPageParams;
        private System.Windows.Forms.TabPage tabPageCurve;
        private System.Windows.Forms.TabPage tabPageDataList;
        // ---------------

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbLineName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbStationLayer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.ProgressBar progressBarCalculate;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartVoltage;
        private System.Windows.Forms.DataGridView gridData;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProfileView;
        private System.Windows.Forms.Timer timerProgress;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTimeInfo;
        private System.Windows.Forms.TrackBar trackBarTime;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartZProfile;
        private System.Windows.Forms.DataGridView gridZData;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResistivity;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblMaxDepth;
        private System.Windows.Forms.NumericUpDown nudMaxDepth;
        private System.Windows.Forms.Panel panelDepthControl;
        private System.Windows.Forms.Label lblMaxDepth1;
        private System.Windows.Forms.NumericUpDown nudMaxDepth1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button1;
    }
}
