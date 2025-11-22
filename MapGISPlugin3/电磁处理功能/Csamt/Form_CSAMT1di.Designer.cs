using MapGIS.PlugUtility;

namespace MapGISPlugin3
{
    partial class Form_CSAMT1di
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageCalculate = new System.Windows.Forms.TabPage();
            this.groupBoxParams = new System.Windows.Forms.GroupBox();
            this.nudIterationCount = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.chartProfileView = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPageLayout = new System.Windows.Forms.TabPage();
            this.groupBoxTransmitter = new System.Windows.Forms.GroupBox();
            this.btnLoadTranFile = new System.Windows.Forms.Button();
            this.labelAz = new System.Windows.Forms.Label();
            this.txtAz = new System.Windows.Forms.TextBox();
            this.labelAy = new System.Windows.Forms.Label();
            this.txtAy = new System.Windows.Forms.TextBox();
            this.labelAx = new System.Windows.Forms.Label();
            this.txtAx = new System.Windows.Forms.TextBox();
            this.labelBz = new System.Windows.Forms.Label();
            this.txtBz = new System.Windows.Forms.TextBox();
            this.labelBy = new System.Windows.Forms.Label();
            this.txtBy = new System.Windows.Forms.TextBox();
            this.labelBx = new System.Windows.Forms.Label();
            this.txtBx = new System.Windows.Forms.TextBox();
            this.btnUpdateDistances = new System.Windows.Forms.Button();
            this.gridLayout = new System.Windows.Forms.DataGridView();
            this.chartLayout = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPageData = new System.Windows.Forms.TabPage();
            this.gridData = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbLineName = new System.Windows.Forms.ComboBox();
            this.cmbStationLayer = new System.Windows.Forms.ComboBox();
            this.chartResultSection = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartPhase = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.openFileDialogTran = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageCalculate.SuspendLayout();
            this.groupBoxParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).BeginInit();
            this.tabPageLayout.SuspendLayout();
            this.groupBoxTransmitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLayout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartLayout)).BeginInit();
            this.tabPageData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartResultSection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 48);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.cmbLineName);
            this.splitContainer1.Panel1.Controls.Add(this.cmbStationLayer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chartResultSection);
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1596, 810);
            this.splitContainer1.SplitterDistance = 750;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageCalculate);
            this.tabControl1.Controls.Add(this.tabPageLayout);
            this.tabControl1.Controls.Add(this.tabPageData);
            this.tabControl1.Location = new System.Drawing.Point(0, 66);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(750, 744);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPageCalculate
            // 
            this.tabPageCalculate.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageCalculate.Controls.Add(this.groupBoxParams);
            this.tabPageCalculate.Controls.Add(this.chartProfileView);
            this.tabPageCalculate.Location = new System.Drawing.Point(4, 28);
            this.tabPageCalculate.Name = "tabPageCalculate";
            this.tabPageCalculate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCalculate.Size = new System.Drawing.Size(742, 712);
            this.tabPageCalculate.TabIndex = 0;
            this.tabPageCalculate.Text = "计算";
            // 
            // groupBoxParams
            // 
            this.groupBoxParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxParams.Controls.Add(this.nudIterationCount);
            this.groupBoxParams.Controls.Add(this.label4);
            this.groupBoxParams.Controls.Add(this.btnCalculate);
            this.groupBoxParams.Location = new System.Drawing.Point(14, 15);
            this.groupBoxParams.Name = "groupBoxParams";
            this.groupBoxParams.Size = new System.Drawing.Size(714, 116);
            this.groupBoxParams.TabIndex = 1;
            this.groupBoxParams.TabStop = false;
            this.groupBoxParams.Text = "计算参数";
            // 
            // nudIterationCount
            // 
            this.nudIterationCount.BackColor = System.Drawing.SystemColors.Control;
            this.nudIterationCount.Location = new System.Drawing.Point(129, 52);
            this.nudIterationCount.Name = "nudIterationCount";
            this.nudIterationCount.Size = new System.Drawing.Size(198, 28);
            this.nudIterationCount.TabIndex = 5;
            this.nudIterationCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 4;
            this.label4.Text = "迭代次数：";
            // 
            // btnCalculate
            // 
            this.btnCalculate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(576, 46);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(100, 40);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // chartProfileView
            // 
            this.chartProfileView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea1);
            this.chartProfileView.Location = new System.Drawing.Point(10, 136);
            this.chartProfileView.Name = "chartProfileView";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartProfileView.Series.Add(series1);
            this.chartProfileView.Size = new System.Drawing.Size(717, 561);
            this.chartProfileView.TabIndex = 0;
            this.chartProfileView.Text = "chart1";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // tabPageLayout
            // 
            this.tabPageLayout.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageLayout.Controls.Add(this.groupBoxTransmitter);
            this.tabPageLayout.Controls.Add(this.gridLayout);
            this.tabPageLayout.Controls.Add(this.chartLayout);
            this.tabPageLayout.Location = new System.Drawing.Point(4, 28);
            this.tabPageLayout.Name = "tabPageLayout";
            this.tabPageLayout.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLayout.Size = new System.Drawing.Size(742, 727);
            this.tabPageLayout.TabIndex = 1;
            this.tabPageLayout.Text = "布置图";
            // 
            // groupBoxTransmitter
            // 
            this.groupBoxTransmitter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTransmitter.Controls.Add(this.btnLoadTranFile);
            this.groupBoxTransmitter.Controls.Add(this.labelAz);
            this.groupBoxTransmitter.Controls.Add(this.txtAz);
            this.groupBoxTransmitter.Controls.Add(this.labelAy);
            this.groupBoxTransmitter.Controls.Add(this.txtAy);
            this.groupBoxTransmitter.Controls.Add(this.labelAx);
            this.groupBoxTransmitter.Controls.Add(this.txtAx);
            this.groupBoxTransmitter.Controls.Add(this.labelBz);
            this.groupBoxTransmitter.Controls.Add(this.txtBz);
            this.groupBoxTransmitter.Controls.Add(this.labelBy);
            this.groupBoxTransmitter.Controls.Add(this.txtBy);
            this.groupBoxTransmitter.Controls.Add(this.labelBx);
            this.groupBoxTransmitter.Controls.Add(this.txtBx);
            this.groupBoxTransmitter.Controls.Add(this.btnUpdateDistances);
            this.groupBoxTransmitter.Location = new System.Drawing.Point(14, 15);
            this.groupBoxTransmitter.Name = "groupBoxTransmitter";
            this.groupBoxTransmitter.Size = new System.Drawing.Size(714, 116);
            this.groupBoxTransmitter.TabIndex = 2;
            this.groupBoxTransmitter.TabStop = false;
            this.groupBoxTransmitter.Text = "发射源位置";
            // 
            // btnLoadTranFile
            // 
            this.btnLoadTranFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadTranFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadTranFile.Location = new System.Drawing.Point(477, 46);
            this.btnLoadTranFile.Name = "btnLoadTranFile";
            this.btnLoadTranFile.Size = new System.Drawing.Size(124, 40);
            this.btnLoadTranFile.TabIndex = 13;
            this.btnLoadTranFile.Text = "上传发射源";
            this.btnLoadTranFile.UseVisualStyleBackColor = true;
            this.btnLoadTranFile.Click += new System.EventHandler(this.btnLoadTranFile_Click);
            // 
            // labelAz
            // 
            this.labelAz.AutoSize = true;
            this.labelAz.Location = new System.Drawing.Point(312, 86);
            this.labelAz.Name = "labelAz";
            this.labelAz.Size = new System.Drawing.Size(35, 18);
            this.labelAz.TabIndex = 12;
            this.labelAz.Text = "Az:";
            // 
            // txtAz
            // 
            this.txtAz.BackColor = System.Drawing.SystemColors.Control;
            this.txtAz.Location = new System.Drawing.Point(354, 78);
            this.txtAz.Name = "txtAz";
            this.txtAz.Size = new System.Drawing.Size(100, 28);
            this.txtAz.TabIndex = 11;
            this.txtAz.Text = "0";
            // 
            // labelAy
            // 
            this.labelAy.AutoSize = true;
            this.labelAy.Location = new System.Drawing.Point(164, 86);
            this.labelAy.Name = "labelAy";
            this.labelAy.Size = new System.Drawing.Size(35, 18);
            this.labelAy.TabIndex = 10;
            this.labelAy.Text = "Ay:";
            // 
            // txtAy
            // 
            this.txtAy.BackColor = System.Drawing.SystemColors.Control;
            this.txtAy.Location = new System.Drawing.Point(204, 78);
            this.txtAy.Name = "txtAy";
            this.txtAy.Size = new System.Drawing.Size(100, 28);
            this.txtAy.TabIndex = 9;
            this.txtAy.Text = "0";
            // 
            // labelAx
            // 
            this.labelAx.AutoSize = true;
            this.labelAx.Location = new System.Drawing.Point(16, 86);
            this.labelAx.Name = "labelAx";
            this.labelAx.Size = new System.Drawing.Size(35, 18);
            this.labelAx.TabIndex = 8;
            this.labelAx.Text = "Ax:";
            // 
            // txtAx
            // 
            this.txtAx.BackColor = System.Drawing.SystemColors.Control;
            this.txtAx.Location = new System.Drawing.Point(57, 78);
            this.txtAx.Name = "txtAx";
            this.txtAx.Size = new System.Drawing.Size(100, 28);
            this.txtAx.TabIndex = 7;
            this.txtAx.Text = "0";
            // 
            // labelBz
            // 
            this.labelBz.AutoSize = true;
            this.labelBz.Location = new System.Drawing.Point(312, 32);
            this.labelBz.Name = "labelBz";
            this.labelBz.Size = new System.Drawing.Size(35, 18);
            this.labelBz.TabIndex = 6;
            this.labelBz.Text = "Bz:";
            // 
            // txtBz
            // 
            this.txtBz.BackColor = System.Drawing.SystemColors.Control;
            this.txtBz.Location = new System.Drawing.Point(354, 27);
            this.txtBz.Name = "txtBz";
            this.txtBz.Size = new System.Drawing.Size(100, 28);
            this.txtBz.TabIndex = 5;
            this.txtBz.Text = "0";
            // 
            // labelBy
            // 
            this.labelBy.AutoSize = true;
            this.labelBy.Location = new System.Drawing.Point(164, 32);
            this.labelBy.Name = "labelBy";
            this.labelBy.Size = new System.Drawing.Size(35, 18);
            this.labelBy.TabIndex = 4;
            this.labelBy.Text = "By:";
            // 
            // txtBy
            // 
            this.txtBy.BackColor = System.Drawing.SystemColors.Control;
            this.txtBy.Location = new System.Drawing.Point(204, 27);
            this.txtBy.Name = "txtBy";
            this.txtBy.Size = new System.Drawing.Size(100, 28);
            this.txtBy.TabIndex = 3;
            this.txtBy.Text = "0";
            // 
            // labelBx
            // 
            this.labelBx.AutoSize = true;
            this.labelBx.Location = new System.Drawing.Point(16, 32);
            this.labelBx.Name = "labelBx";
            this.labelBx.Size = new System.Drawing.Size(35, 18);
            this.labelBx.TabIndex = 2;
            this.labelBx.Text = "Bx:";
            // 
            // txtBx
            // 
            this.txtBx.BackColor = System.Drawing.SystemColors.Control;
            this.txtBx.Location = new System.Drawing.Point(57, 27);
            this.txtBx.Name = "txtBx";
            this.txtBx.Size = new System.Drawing.Size(100, 28);
            this.txtBx.TabIndex = 1;
            this.txtBx.Text = "0";
            // 
            // btnUpdateDistances
            // 
            this.btnUpdateDistances.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateDistances.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnUpdateDistances.Location = new System.Drawing.Point(608, 46);
            this.btnUpdateDistances.Name = "btnUpdateDistances";
            this.btnUpdateDistances.Size = new System.Drawing.Size(100, 40);
            this.btnUpdateDistances.TabIndex = 0;
            this.btnUpdateDistances.Text = "更新距离";
            this.btnUpdateDistances.UseVisualStyleBackColor = true;
            this.btnUpdateDistances.Click += new System.EventHandler(this.btnUpdateDistances_Click);
            // 
            // gridLayout
            // 
            this.gridLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridLayout.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridLayout.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLayout.Location = new System.Drawing.Point(10, 136);
            this.gridLayout.Name = "gridLayout";
            this.gridLayout.RowHeadersWidth = 62;
            this.gridLayout.RowTemplate.Height = 30;
            this.gridLayout.Size = new System.Drawing.Size(717, 291);
            this.gridLayout.TabIndex = 1;
            // 
            // chartLayout
            // 
            this.chartLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.chartLayout.ChartAreas.Add(chartArea2);
            legend1.BackColor = System.Drawing.SystemColors.Control;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chartLayout.Legends.Add(legend1);
            this.chartLayout.Location = new System.Drawing.Point(10, 434);
            this.chartLayout.Name = "chartLayout";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartLayout.Series.Add(series2);
            this.chartLayout.Size = new System.Drawing.Size(717, 279);
            this.chartLayout.TabIndex = 0;
            this.chartLayout.Text = "chartLayout";
            // 
            // tabPageData
            // 
            this.tabPageData.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageData.Controls.Add(this.gridData);
            this.tabPageData.Location = new System.Drawing.Point(4, 28);
            this.tabPageData.Name = "tabPageData";
            this.tabPageData.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageData.Size = new System.Drawing.Size(742, 727);
            this.tabPageData.TabIndex = 2;
            this.tabPageData.Text = "数据";
            // 
            // gridData
            // 
            this.gridData.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridData.Location = new System.Drawing.Point(3, 3);
            this.gridData.Name = "gridData";
            this.gridData.RowHeadersWidth = 62;
            this.gridData.RowTemplate.Height = 30;
            this.gridData.Size = new System.Drawing.Size(736, 721);
            this.gridData.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 18);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择测点图层:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(400, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "选择测线:";
            // 
            // cmbLineName
            // 
            this.cmbLineName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLineName.BackColor = System.Drawing.SystemColors.Control;
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLineName.FormattingEnabled = true;
            this.cmbLineName.Location = new System.Drawing.Point(496, 20);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(236, 26);
            this.cmbLineName.TabIndex = 1;
            this.cmbLineName.SelectedIndexChanged += new System.EventHandler(this.cmbLineName_SelectedIndexChanged);
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.BackColor = System.Drawing.SystemColors.Control;
            this.cmbStationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStationLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbStationLayer.FormattingEnabled = true;
            this.cmbStationLayer.Location = new System.Drawing.Point(147, 20);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(236, 26);
            this.cmbStationLayer.TabIndex = 0;
            this.cmbStationLayer.SelectedIndexChanged += new System.EventHandler(this.cmbStationLayer_SelectedIndexChanged);
            // 
            // chartResultSection
            // 
            this.chartResultSection.BackColor = System.Drawing.SystemColors.Control;
            chartArea3.Name = "ChartArea1";
            this.chartResultSection.ChartAreas.Add(chartArea3);
            this.chartResultSection.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.BackColor = System.Drawing.SystemColors.Control;
            legend2.Name = "Legend1";
            this.chartResultSection.Legends.Add(legend2);
            this.chartResultSection.Location = new System.Drawing.Point(0, 420);
            this.chartResultSection.Margin = new System.Windows.Forms.Padding(4);
            this.chartResultSection.Name = "chartResultSection";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chartResultSection.Series.Add(series3);
            this.chartResultSection.Size = new System.Drawing.Size(842, 390);
            this.chartResultSection.TabIndex = 1;
            this.chartResultSection.Text = "chartResultSection";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.chartResistivity);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.chartPhase);
            this.splitContainer2.Size = new System.Drawing.Size(842, 420);
            this.splitContainer2.SplitterDistance = 418;
            this.splitContainer2.SplitterWidth = 6;
            this.splitContainer2.TabIndex = 0;
            // 
            // chartResistivity
            // 
            chartArea4.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea4);
            this.chartResistivity.Dock = System.Windows.Forms.DockStyle.Fill;
            legend3.BackColor = System.Drawing.SystemColors.Control;
            legend3.Name = "Legend1";
            this.chartResistivity.Legends.Add(legend3);
            this.chartResistivity.Location = new System.Drawing.Point(0, 0);
            this.chartResistivity.Name = "chartResistivity";
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chartResistivity.Series.Add(series4);
            this.chartResistivity.Size = new System.Drawing.Size(418, 420);
            this.chartResistivity.TabIndex = 1;
            this.chartResistivity.Text = "chart2";
            // 
            // chartPhase
            // 
            chartArea5.Name = "ChartArea1";
            this.chartPhase.ChartAreas.Add(chartArea5);
            this.chartPhase.Dock = System.Windows.Forms.DockStyle.Fill;
            legend4.BackColor = System.Drawing.SystemColors.Control;
            legend4.Name = "Legend1";
            this.chartPhase.Legends.Add(legend4);
            this.chartPhase.Location = new System.Drawing.Point(0, 0);
            this.chartPhase.Name = "chartPhase";
            series5.ChartArea = "ChartArea1";
            series5.Legend = "Legend1";
            series5.Name = "Series1";
            this.chartPhase.Series.Add(series5);
            this.chartPhase.Size = new System.Drawing.Size(418, 420);
            this.chartPhase.TabIndex = 2;
            this.chartPhase.Text = "chart3";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1596, 48);
            this.panel1.TabIndex = 6;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(150, 48);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "CSAMT一维反演";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("宋体", 6.75F);
            this.btnClose.Location = new System.Drawing.Point(1552, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(44, 48);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // openFileDialogTran
            // 
            this.openFileDialogTran.FileName = "tran.dat";
            this.openFileDialogTran.Filter = "发射源文件 (*.dat)|*.dat|所有文件 (*.*)|*.*";
            this.openFileDialogTran.Title = "请选择发射源文件";
            // 
            // Form_CSAMT1di
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1596, 858);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form_CSAMT1di";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "可控源音频大地电磁法一维反演";
            this.Load += new System.EventHandler(this.Form_CSAMT1di_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageCalculate.ResumeLayout(false);
            this.groupBoxParams.ResumeLayout(false);
            this.groupBoxParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).EndInit();
            this.tabPageLayout.ResumeLayout(false);
            this.groupBoxTransmitter.ResumeLayout(false);
            this.groupBoxTransmitter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLayout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartLayout)).EndInit();
            this.tabPageData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartResultSection)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageCalculate;
        private System.Windows.Forms.GroupBox groupBoxParams;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProfileView;
        private System.Windows.Forms.TabPage tabPageLayout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbLineName;
        private System.Windows.Forms.ComboBox cmbStationLayer;
        private System.Windows.Forms.TabPage tabPageData;
        private System.Windows.Forms.NumericUpDown nudIterationCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.DataGridView gridData;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPhase;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResistivity;
        private System.Windows.Forms.GroupBox groupBoxTransmitter;
        private System.Windows.Forms.Label labelAz;
        private System.Windows.Forms.TextBox txtAz;
        private System.Windows.Forms.Label labelAy;
        private System.Windows.Forms.TextBox txtAy;
        private System.Windows.Forms.Label labelAx;
        private System.Windows.Forms.TextBox txtAx;
        private System.Windows.Forms.Label labelBz;
        private System.Windows.Forms.TextBox txtBz;
        private System.Windows.Forms.Label labelBy;
        private System.Windows.Forms.TextBox txtBy;
        private System.Windows.Forms.Label labelBx;
        private System.Windows.Forms.TextBox txtBx;
        private System.Windows.Forms.Button btnUpdateDistances;
        private System.Windows.Forms.DataGridView gridLayout;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartLayout;
        private System.Windows.Forms.Button btnLoadTranFile;
        private System.Windows.Forms.OpenFileDialog openFileDialogTran;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResultSection;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnClose;
    }
}