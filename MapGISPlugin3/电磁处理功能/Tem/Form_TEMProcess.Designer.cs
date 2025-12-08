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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend7 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend8 = new System.Windows.Forms.DataVisualization.Charting.Legend();
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
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
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
            // timerProgress
            // 
            this.timerProgress.Interval = 500;
            this.timerProgress.Tick += new System.EventHandler(this.timerProgress_Tick);
            // 
            // lblTimeInfo
            // 
            this.lblTimeInfo.AutoSize = true;
            this.lblTimeInfo.Location = new System.Drawing.Point(13, 55);
            this.lblTimeInfo.Name = "lblTimeInfo";
            this.lblTimeInfo.Size = new System.Drawing.Size(125, 12);
            this.lblTimeInfo.TabIndex = 2;
            this.lblTimeInfo.Text = "当前采样时间: 未选择";
            // 
            // trackBarTime
            // 
            this.trackBarTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarTime.Location = new System.Drawing.Point(9, 9);
            this.trackBarTime.Name = "trackBarTime";
            this.trackBarTime.Size = new System.Drawing.Size(433, 45);
            this.trackBarTime.TabIndex = 1;
            this.trackBarTime.TickStyle = System.Windows.Forms.TickStyle.Both;
            // 
            // chartZProfile
            // 
            this.chartZProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea5.AxisX.Title = "距离 (m)";
            chartArea5.AxisY.Title = "感应电动势 (mV)";
            chartArea5.Name = "ChartArea1";
            this.chartZProfile.ChartAreas.Add(chartArea5);
            legend5.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend5.Name = "Legend1";
            this.chartZProfile.Legends.Add(legend5);
            this.chartZProfile.Location = new System.Drawing.Point(0, 75);
            this.chartZProfile.Name = "chartZProfile";
            this.chartZProfile.Size = new System.Drawing.Size(442, 449);
            this.chartZProfile.TabIndex = 0;
            this.chartZProfile.Text = "Z Profile Chart";
            // 
            // gridZData
            // 
            this.gridZData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridZData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridZData.Location = new System.Drawing.Point(0, 0);
            this.gridZData.Name = "gridZData";
            this.gridZData.RowHeadersWidth = 62;
            this.gridZData.RowTemplate.Height = 23;
            this.gridZData.Size = new System.Drawing.Size(442, 524);
            this.gridZData.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(980, 32);
            this.panel1.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 32);
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
            this.button2.Location = new System.Drawing.Point(951, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(29, 32);
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
            this.tabControlLeft.Location = new System.Drawing.Point(5, 38);
            this.tabControlLeft.Name = "tabControlLeft";
            this.tabControlLeft.SelectedIndex = 0;
            this.tabControlLeft.Size = new System.Drawing.Size(450, 550);
            this.tabControlLeft.TabIndex = 0;
            // 
            // tabPageLayout
            // 
            this.tabPageLayout.Controls.Add(this.gridData);
            this.tabPageLayout.Controls.Add(this.groupBox1);
            this.tabPageLayout.Controls.Add(this.btnCalculate);
            this.tabPageLayout.Controls.Add(this.progressBarCalculate);
            this.tabPageLayout.Controls.Add(this.chartProfileView);
            this.tabPageLayout.Location = new System.Drawing.Point(4, 22);
            this.tabPageLayout.Name = "tabPageLayout";
            this.tabPageLayout.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPageLayout.Size = new System.Drawing.Size(442, 524);
            this.tabPageLayout.TabIndex = 0;
            this.tabPageLayout.Text = "布置图";
            this.tabPageLayout.UseVisualStyleBackColor = true;
            // 
            // gridData
            // 
            this.gridData.AllowUserToAddRows = false;
            this.gridData.AllowUserToDeleteRows = false;
            this.gridData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridData.Location = new System.Drawing.Point(6, 6);
            this.gridData.Name = "gridData";
            this.gridData.ReadOnly = true;
            this.gridData.RowHeadersWidth = 62;
            this.gridData.RowTemplate.Height = 23;
            this.gridData.Size = new System.Drawing.Size(425, 101);
            this.gridData.TabIndex = 25;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbLineName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmbStationLayer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 113);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(425, 69);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据选择";
            // 
            // cmbLineName
            // 
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cmbLineName.Location = new System.Drawing.Point(233, 34);
            this.cmbLineName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(165, 20);
            this.cmbLineName.TabIndex = 3;
            this.cmbLineName.SelectedIndexChanged += new System.EventHandler(this.cmbLineName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(230, 17);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "测线：";
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStationLayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cmbStationLayer.Location = new System.Drawing.Point(26, 34);
            this.cmbStationLayer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(165, 20);
            this.cmbStationLayer.TabIndex = 1;
            this.cmbStationLayer.SelectedIndexChanged += new System.EventHandler(this.cmbStationLayer_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "测点图层：";
            // 
            // btnCalculate
            // 
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(6, 188);
            this.btnCalculate.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(100, 30);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "开始计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // progressBarCalculate
            // 
            this.progressBarCalculate.Location = new System.Drawing.Point(121, 194);
            this.progressBarCalculate.Name = "progressBarCalculate";
            this.progressBarCalculate.Size = new System.Drawing.Size(283, 18);
            this.progressBarCalculate.TabIndex = 26;
            // 
            // chartProfileView
            // 
            chartArea6.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea6);
            legend6.Alignment = System.Drawing.StringAlignment.Far;
            legend6.BackColor = System.Drawing.SystemColors.Control;
            legend6.BorderColor = System.Drawing.Color.LightGray;
            legend6.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend6.Font = new System.Drawing.Font("微软雅黑", 8F);
            legend6.IsTextAutoFit = false;
            legend6.Name = "LegendProfile";
            this.chartProfileView.Legends.Add(legend6);
            this.chartProfileView.Location = new System.Drawing.Point(6, 223);
            this.chartProfileView.Name = "chartProfileView";
            this.chartProfileView.Size = new System.Drawing.Size(425, 295);
            this.chartProfileView.TabIndex = 23;
            this.chartProfileView.Text = "chartProfileView";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // tabPageParams
            // 
            this.tabPageParams.Controls.Add(this.groupBox2);
            this.tabPageParams.Location = new System.Drawing.Point(4, 22);
            this.tabPageParams.Name = "tabPageParams";
            this.tabPageParams.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPageParams.Size = new System.Drawing.Size(442, 524);
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
            this.groupBox2.Location = new System.Drawing.Point(14, 30);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(415, 219);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "可选参数调整";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(93, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "模型光滑因子(0.001)";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Control;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox5.Location = new System.Drawing.Point(225, 180);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(98, 21);
            this.textBox5.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(123, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "反演深度(1000)";
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Control;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox6.Location = new System.Drawing.Point(225, 147);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(98, 21);
            this.textBox6.TabIndex = 12;
            this.textBox6.Text = "0.05";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(69, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(143, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "模型层厚度递增因子(1.1)";
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox3.Location = new System.Drawing.Point(225, 114);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(98, 21);
            this.textBox3.TabIndex = 11;
            this.textBox3.Text = "500";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(95, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "模型初始电阻率(500)";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox4.Location = new System.Drawing.Point(225, 80);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(98, 21);
            this.textBox4.TabIndex = 10;
            this.textBox4.Text = "1.1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(111, 182);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(101, 12);
            this.label9.TabIndex = 6;
            this.label9.Text = "当前测点拟合误差";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox2.Location = new System.Drawing.Point(225, 49);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(98, 21);
            this.textBox2.TabIndex = 9;
            this.textBox2.Text = "1000";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(99, 149);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 12);
            this.label8.TabIndex = 7;
            this.label8.Text = "期望拟合误差(0.05)";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(225, 22);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(98, 21);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = "0.001";
            // 
            // tabPageCurve
            // 
            this.tabPageCurve.Controls.Add(this.lblTimeInfo);
            this.tabPageCurve.Controls.Add(this.trackBarTime);
            this.tabPageCurve.Controls.Add(this.chartZProfile);
            this.tabPageCurve.Location = new System.Drawing.Point(4, 22);
            this.tabPageCurve.Name = "tabPageCurve";
            this.tabPageCurve.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPageCurve.Size = new System.Drawing.Size(442, 524);
            this.tabPageCurve.TabIndex = 2;
            this.tabPageCurve.Text = "感应电动势剖面曲线图";
            this.tabPageCurve.UseVisualStyleBackColor = true;
            // 
            // tabPageDataList
            // 
            this.tabPageDataList.Controls.Add(this.gridZData);
            this.tabPageDataList.Location = new System.Drawing.Point(4, 22);
            this.tabPageDataList.Name = "tabPageDataList";
            this.tabPageDataList.Size = new System.Drawing.Size(442, 524);
            this.tabPageDataList.TabIndex = 3;
            this.tabPageDataList.Text = "感应电动势数据列表";
            this.tabPageDataList.UseVisualStyleBackColor = true;
            // 
            // chartVoltage
            // 
            chartArea7.Name = "ChartArea1";
            this.chartVoltage.ChartAreas.Add(chartArea7);
            this.chartVoltage.Dock = System.Windows.Forms.DockStyle.Fill;
            legend7.Alignment = System.Drawing.StringAlignment.Far;
            legend7.BackColor = System.Drawing.SystemColors.Control;
            legend7.BorderColor = System.Drawing.Color.LightGray;
            legend7.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend7.Font = new System.Drawing.Font("微软雅黑", 8F);
            legend7.IsTextAutoFit = false;
            legend7.Name = "LegendVoltage";
            this.chartVoltage.Legends.Add(legend7);
            this.chartVoltage.Location = new System.Drawing.Point(0, 0);
            this.chartVoltage.Name = "chartVoltage";
            this.chartVoltage.Size = new System.Drawing.Size(499, 269);
            this.chartVoltage.TabIndex = 24;
            this.chartVoltage.Text = "chartVoltage";
            // 
            // splitContainerRight
            // 
            this.splitContainerRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerRight.Location = new System.Drawing.Point(461, 35);
            this.splitContainerRight.Name = "splitContainerRight";
            this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            this.splitContainerRight.Panel1.Controls.Add(this.chartVoltage);
            // 
            // splitContainerRight.Panel2
            // 
            this.splitContainerRight.Panel2.Controls.Add(this.chartResistivity);
            this.splitContainerRight.Size = new System.Drawing.Size(501, 549);
            this.splitContainerRight.SplitterDistance = 271;
            this.splitContainerRight.TabIndex = 24;
            // 
            // chartResistivity
            // 
            chartArea8.AxisX.Title = "距离 x(m)";
            chartArea8.AxisY.Title = "深度 z(m)";
            chartArea8.CursorX.IsUserEnabled = true;
            chartArea8.CursorX.IsUserSelectionEnabled = true;
            chartArea8.CursorY.IsUserEnabled = true;
            chartArea8.CursorY.IsUserSelectionEnabled = true;
            chartArea8.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea8);
            this.chartResistivity.Dock = System.Windows.Forms.DockStyle.Fill;
            legend8.Enabled = false;
            legend8.Name = "Legend1";
            this.chartResistivity.Legends.Add(legend8);
            this.chartResistivity.Location = new System.Drawing.Point(0, 0);
            this.chartResistivity.Name = "chartResistivity";
            this.chartResistivity.Size = new System.Drawing.Size(499, 272);
            this.chartResistivity.TabIndex = 0;
            this.chartResistivity.Text = "Resistivity Section";
            // 
            // Form_TEMProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 600);
            this.Controls.Add(this.splitContainerRight);
            this.Controls.Add(this.tabControlLeft);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximumSize = new System.Drawing.Size(980, 600);
            this.MinimumSize = new System.Drawing.Size(980, 600);
            this.Name = "Form_TEMProcess";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.Load += new System.EventHandler(this.Form_TEMProcess_Load);
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
    }
}