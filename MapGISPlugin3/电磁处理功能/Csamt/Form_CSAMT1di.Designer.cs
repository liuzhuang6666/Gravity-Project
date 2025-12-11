using MapGIS.PlugUtility;

namespace MapGISPlugin3
{
    partial class Form_CSAMT1di
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
<<<<<<< HEAD
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
=======
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea9 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series9 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea10 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend8 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series10 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend7 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageCalculate = new System.Windows.Forms.TabPage();
            this.gridCalc = new System.Windows.Forms.DataGridView();
            this.chartProfileView = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBoxCalcParams = new System.Windows.Forms.GroupBox();
            this.txtActualError = new System.Windows.Forms.TextBox();
            this.labelActualError = new System.Windows.Forms.Label();
            this.txtAllowError = new System.Windows.Forms.TextBox();
            this.labelAllowError = new System.Windows.Forms.Label();
            this.chkJointInversion = new System.Windows.Forms.CheckBox();
            this.chkUseCurrentStation = new System.Windows.Forms.CheckBox();
            this.nudIterationCount = new System.Windows.Forms.NumericUpDown();
            this.labelIter = new System.Windows.Forms.Label();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.progressBarCalc = new System.Windows.Forms.ProgressBar();
            this.labelProgressPercent = new System.Windows.Forms.Label();
            this.tabPageLayout = new System.Windows.Forms.TabPage();
            this.chartLayout = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panelLayoutTop = new System.Windows.Forms.Panel();
            this.labelOffsetFromCenter = new System.Windows.Forms.Label();
            this.txtOffsetFromCenter = new System.Windows.Forms.TextBox();
            this.labelTxRxDistance = new System.Windows.Forms.Label();
            this.txtTxRxDistance = new System.Windows.Forms.TextBox();
            this.labelCurrentInversionStation = new System.Windows.Forms.Label();
            this.txtCurrentInversionStation = new System.Windows.Forms.TextBox();
            this.labelCurrentSelectedStation = new System.Windows.Forms.Label();
            this.txtCurrentSelectedStation = new System.Windows.Forms.TextBox();
            this.tabPageData = new System.Windows.Forms.TabPage();
            this.gridData = new System.Windows.Forms.DataGridView();
            this.tabPageModel = new System.Windows.Forms.TabPage();
            this.gridModelLayers = new System.Windows.Forms.DataGridView();
            this.groupBoxModelParams = new System.Windows.Forms.GroupBox();
            this.btnGenerateModel = new System.Windows.Forms.Button();
            this.nudInitialResistivity = new System.Windows.Forms.NumericUpDown();
            this.labelInitialResistivity = new System.Windows.Forms.Label();
            this.nudGrowthRate = new System.Windows.Forms.NumericUpDown();
            this.labelGrowthRate = new System.Windows.Forms.Label();
            this.nudInitialThickness = new System.Windows.Forms.NumericUpDown();
            this.labelInitialThickness = new System.Windows.Forms.Label();
            this.nudLayerCount = new System.Windows.Forms.NumericUpDown();
            this.labelLayerCount = new System.Windows.Forms.Label();
            this.labelStationLayer = new System.Windows.Forms.Label();
            this.labelLineName = new System.Windows.Forms.Label();
            this.cmbLineName = new System.Windows.Forms.ComboBox();
            this.cmbStationLayer = new System.Windows.Forms.ComboBox();
            this.splitContainerRightMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerRight = new System.Windows.Forms.SplitContainer();
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartPhase = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartResultSection = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panelResultControl = new System.Windows.Forms.Panel();
            this.nudMaxDepth = new System.Windows.Forms.NumericUpDown();
            this.labelDepthRange = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.openFileDialogTran = new System.Windows.Forms.OpenFileDialog();
            this.txtAz = new System.Windows.Forms.TextBox();
            this.txtAy = new System.Windows.Forms.TextBox();
            this.txtAx = new System.Windows.Forms.TextBox();
            this.txtBz = new System.Windows.Forms.TextBox();
            this.txtBy = new System.Windows.Forms.TextBox();
            this.txtBx = new System.Windows.Forms.TextBox();
            this.gridLayout = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageCalculate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCalc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).BeginInit();
            this.groupBoxCalcParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).BeginInit();
            this.tabPageLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLayout)).BeginInit();
            this.panelLayoutTop.SuspendLayout();
            this.tabPageData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).BeginInit();
            this.tabPageModel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridModelLayers)).BeginInit();
            this.groupBoxModelParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialResistivity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrowthRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLayerCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRightMain)).BeginInit();
            this.splitContainerRightMain.Panel1.SuspendLayout();
            this.splitContainerRightMain.Panel2.SuspendLayout();
            this.splitContainerRightMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
            this.splitContainerRight.Panel1.SuspendLayout();
            this.splitContainerRight.Panel2.SuspendLayout();
            this.splitContainerRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartResultSection)).BeginInit();
            this.panelResultControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth)).BeginInit();
            this.panelTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLayout)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 24);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.tabControl1);
            this.splitContainerMain.Panel1.Controls.Add(this.labelStationLayer);
            this.splitContainerMain.Panel1.Controls.Add(this.labelLineName);
            this.splitContainerMain.Panel1.Controls.Add(this.cmbLineName);
            this.splitContainerMain.Panel1.Controls.Add(this.cmbStationLayer);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerRightMain);
<<<<<<< HEAD
            this.splitContainerMain.Size = new System.Drawing.Size(1650, 939);
            this.splitContainerMain.SplitterDistance = 863;
=======
            this.splitContainerMain.Size = new System.Drawing.Size(1280, 676);
            this.splitContainerMain.SplitterDistance = 670;
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.splitContainerMain.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageCalculate);
            this.tabControl1.Controls.Add(this.tabPageLayout);
            this.tabControl1.Controls.Add(this.tabPageData);
            this.tabControl1.Controls.Add(this.tabPageModel);
            this.tabControl1.Location = new System.Drawing.Point(0, 50);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
<<<<<<< HEAD
            this.tabControl1.Size = new System.Drawing.Size(863, 889);
=======
            this.tabControl1.Size = new System.Drawing.Size(670, 626);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.tabControl1.TabIndex = 5;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPageCalculate
            // 
            this.tabPageCalculate.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageCalculate.Controls.Add(this.gridCalc);
            this.tabPageCalculate.Controls.Add(this.chartProfileView);
            this.tabPageCalculate.Controls.Add(this.groupBoxCalcParams);
            this.tabPageCalculate.Location = new System.Drawing.Point(4, 28);
            this.tabPageCalculate.Name = "tabPageCalculate";
            this.tabPageCalculate.Padding = new System.Windows.Forms.Padding(2);
<<<<<<< HEAD
            this.tabPageCalculate.Size = new System.Drawing.Size(855, 857);
=======
            this.tabPageCalculate.Size = new System.Drawing.Size(662, 600);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.tabPageCalculate.TabIndex = 0;
            this.tabPageCalculate.Text = "计算";
            // 
            // gridCalc
            // 
            this.gridCalc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridCalc.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridCalc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
<<<<<<< HEAD
            this.gridCalc.Location = new System.Drawing.Point(7, 398);
=======
            this.gridCalc.Location = new System.Drawing.Point(7, 335);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.gridCalc.Name = "gridCalc";
            this.gridCalc.RowHeadersWidth = 45;
            this.gridCalc.RowTemplate.Height = 23;
            this.gridCalc.Size = new System.Drawing.Size(841, 452);
            this.gridCalc.TabIndex = 3;
            // 
            // chartProfileView
            // 
            this.chartProfileView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
<<<<<<< HEAD
            chartArea1.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea1);
            this.chartProfileView.Location = new System.Drawing.Point(7, 110);
            this.chartProfileView.Name = "chartProfileView";
            series1.ChartArea = "ChartArea1";
            series1.Name = "Series1";
            this.chartProfileView.Series.Add(series1);
            this.chartProfileView.Size = new System.Drawing.Size(841, 284);
=======
            chartArea9.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea9);
            this.chartProfileView.Location = new System.Drawing.Point(7, 110);
            this.chartProfileView.Name = "chartProfileView";
            series9.ChartArea = "ChartArea1";
            series9.Name = "Series1";
            this.chartProfileView.Series.Add(series9);
            this.chartProfileView.Size = new System.Drawing.Size(648, 200);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartProfileView.TabIndex = 2;
            this.chartProfileView.Text = "chartProfileView";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // groupBoxCalcParams
            // 
            this.groupBoxCalcParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCalcParams.Controls.Add(this.txtActualError);
            this.groupBoxCalcParams.Controls.Add(this.labelActualError);
            this.groupBoxCalcParams.Controls.Add(this.txtAllowError);
            this.groupBoxCalcParams.Controls.Add(this.labelAllowError);
            this.groupBoxCalcParams.Controls.Add(this.chkJointInversion);
            this.groupBoxCalcParams.Controls.Add(this.chkUseCurrentStation);
            this.groupBoxCalcParams.Controls.Add(this.nudIterationCount);
            this.groupBoxCalcParams.Controls.Add(this.labelIter);
            this.groupBoxCalcParams.Controls.Add(this.btnCalculate);
            this.groupBoxCalcParams.Controls.Add(this.progressBarCalc);
            this.groupBoxCalcParams.Controls.Add(this.labelProgressPercent);
            this.groupBoxCalcParams.Location = new System.Drawing.Point(7, 7);
            this.groupBoxCalcParams.Name = "groupBoxCalcParams";
            this.groupBoxCalcParams.Size = new System.Drawing.Size(841, 97);
            this.groupBoxCalcParams.TabIndex = 1;
            this.groupBoxCalcParams.TabStop = false;
            this.groupBoxCalcParams.Text = "计算参数";
            // 
            // txtActualError
            // 
            this.txtActualError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtActualError.BackColor = System.Drawing.SystemColors.Control;
            this.txtActualError.Location = new System.Drawing.Point(308, 65);
            this.txtActualError.Name = "txtActualError";
            this.txtActualError.ReadOnly = true;
            this.txtActualError.Size = new System.Drawing.Size(100, 28);
            this.txtActualError.TabIndex = 9;
            // 
            // labelActualError
            // 
            this.labelActualError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelActualError.AutoSize = true;
            this.labelActualError.Location = new System.Drawing.Point(171, 68);
            this.labelActualError.Name = "labelActualError";
            this.labelActualError.Size = new System.Drawing.Size(134, 18);
            this.labelActualError.TabIndex = 8;
            this.labelActualError.Text = "实际迭代误差：";
            // 
            // txtAllowError
            // 
            this.txtAllowError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtAllowError.BackColor = System.Drawing.SystemColors.Control;
            this.txtAllowError.Location = new System.Drawing.Point(308, 38);
            this.txtAllowError.Name = "txtAllowError";
            this.txtAllowError.Size = new System.Drawing.Size(100, 28);
            this.txtAllowError.TabIndex = 7;
            // 
            // labelAllowError
            // 
            this.labelAllowError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAllowError.AutoSize = true;
            this.labelAllowError.Location = new System.Drawing.Point(171, 41);
            this.labelAllowError.Name = "labelAllowError";
            this.labelAllowError.Size = new System.Drawing.Size(134, 18);
            this.labelAllowError.TabIndex = 6;
            this.labelAllowError.Text = "允许迭代误差：";
            // 
            // chkJointInversion
            // 
            this.chkJointInversion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkJointInversion.AutoSize = true;
            this.chkJointInversion.Location = new System.Drawing.Point(17, 67);
            this.chkJointInversion.Name = "chkJointInversion";
            this.chkJointInversion.Size = new System.Drawing.Size(106, 22);
            this.chkJointInversion.TabIndex = 5;
            this.chkJointInversion.Text = "联合反演";
            this.chkJointInversion.UseVisualStyleBackColor = true;
            // 
            // chkUseCurrentStation
            // 
            this.chkUseCurrentStation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkUseCurrentStation.AutoSize = true;
            this.chkUseCurrentStation.Location = new System.Drawing.Point(17, 41);
            this.chkUseCurrentStation.Name = "chkUseCurrentStation";
            this.chkUseCurrentStation.Size = new System.Drawing.Size(124, 22);
            this.chkUseCurrentStation.TabIndex = 4;
            this.chkUseCurrentStation.Text = "仅当前测点";
            this.chkUseCurrentStation.UseVisualStyleBackColor = true;
            // 
            // nudIterationCount
            // 
            this.nudIterationCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.nudIterationCount.BackColor = System.Drawing.SystemColors.Control;
            this.nudIterationCount.Location = new System.Drawing.Point(308, 14);
            this.nudIterationCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudIterationCount.Name = "nudIterationCount";
            this.nudIterationCount.Size = new System.Drawing.Size(100, 28);
            this.nudIterationCount.TabIndex = 3;
            this.nudIterationCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // labelIter
            // 
            this.labelIter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelIter.AutoSize = true;
            this.labelIter.Location = new System.Drawing.Point(171, 17);
            this.labelIter.Name = "labelIter";
            this.labelIter.Size = new System.Drawing.Size(98, 18);
            this.labelIter.TabIndex = 2;
            this.labelIter.Text = "迭代次数：";
            // 
            // btnCalculate
            // 
            this.btnCalculate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(470, 23);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(67, 27);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // progressBarCalc
            // 
            this.progressBarCalc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBarCalc.Location = new System.Drawing.Point(470, 58);
            this.progressBarCalc.Name = "progressBarCalc";
            this.progressBarCalc.Size = new System.Drawing.Size(279, 23);
            this.progressBarCalc.TabIndex = 13;
            this.progressBarCalc.Visible = false;
            // 
            // labelProgressPercent
            // 
            this.labelProgressPercent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgressPercent.AutoSize = true;
            this.labelProgressPercent.Location = new System.Drawing.Point(755, 60);
            this.labelProgressPercent.Name = "labelProgressPercent";
            this.labelProgressPercent.Size = new System.Drawing.Size(26, 18);
            this.labelProgressPercent.TabIndex = 14;
            this.labelProgressPercent.Text = "0%";
            this.labelProgressPercent.Visible = false;
            // 
            // tabPageLayout
            // 
            this.tabPageLayout.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageLayout.Controls.Add(this.chartLayout);
            this.tabPageLayout.Controls.Add(this.panelLayoutTop);
            this.tabPageLayout.Location = new System.Drawing.Point(4, 28);
            this.tabPageLayout.Name = "tabPageLayout";
            this.tabPageLayout.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageLayout.Size = new System.Drawing.Size(855, 857);
            this.tabPageLayout.TabIndex = 1;
            this.tabPageLayout.Text = "布置图";
            // 
            // chartLayout
            // 
            this.chartLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.chartLayout.ChartAreas.Add(chartArea2);
            legend1.BackColor = System.Drawing.SystemColors.Control;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chartLayout.Legends.Add(legend1);
            this.chartLayout.Location = new System.Drawing.Point(7, 80);
            this.chartLayout.Name = "chartLayout";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartLayout.Series.Add(series2);
            this.chartLayout.Size = new System.Drawing.Size(841, 770);
            this.chartLayout.TabIndex = 1;
            this.chartLayout.Text = "chartLayout";
            this.chartLayout.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartLayout_MouseClick);
            // 
            // panelLayoutTop
            // 
            this.panelLayoutTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLayoutTop.Controls.Add(this.labelOffsetFromCenter);
            this.panelLayoutTop.Controls.Add(this.txtOffsetFromCenter);
            this.panelLayoutTop.Controls.Add(this.labelTxRxDistance);
            this.panelLayoutTop.Controls.Add(this.txtTxRxDistance);
            this.panelLayoutTop.Controls.Add(this.labelCurrentInversionStation);
            this.panelLayoutTop.Controls.Add(this.txtCurrentInversionStation);
            this.panelLayoutTop.Controls.Add(this.labelCurrentSelectedStation);
            this.panelLayoutTop.Controls.Add(this.txtCurrentSelectedStation);
            this.panelLayoutTop.Location = new System.Drawing.Point(7, 7);
            this.panelLayoutTop.Name = "panelLayoutTop";
            this.panelLayoutTop.Size = new System.Drawing.Size(841, 67);
            this.panelLayoutTop.TabIndex = 0;
            // 
            // labelOffsetFromCenter
            // 
            this.labelOffsetFromCenter.AutoSize = true;
            this.labelOffsetFromCenter.Location = new System.Drawing.Point(320, 42);
            this.labelOffsetFromCenter.Name = "labelOffsetFromCenter";
            this.labelOffsetFromCenter.Size = new System.Drawing.Size(188, 18);
            this.labelOffsetFromCenter.TabIndex = 8;
            this.labelOffsetFromCenter.Text = "偏移发射源中心距离：";
            this.labelOffsetFromCenter.Click += new System.EventHandler(this.labelOffsetFromCenter_Click);
            // 
            // txtOffsetFromCenter
            // 
            this.txtOffsetFromCenter.BackColor = System.Drawing.SystemColors.Control;
            this.txtOffsetFromCenter.Location = new System.Drawing.Point(507, 38);
            this.txtOffsetFromCenter.Name = "txtOffsetFromCenter";
            this.txtOffsetFromCenter.ReadOnly = true;
            this.txtOffsetFromCenter.Size = new System.Drawing.Size(126, 28);
            this.txtOffsetFromCenter.TabIndex = 7;
            // 
            // labelTxRxDistance
            // 
            this.labelTxRxDistance.AutoSize = true;
            this.labelTxRxDistance.Location = new System.Drawing.Point(320, 14);
            this.labelTxRxDistance.Name = "labelTxRxDistance";
            this.labelTxRxDistance.Size = new System.Drawing.Size(98, 18);
            this.labelTxRxDistance.TabIndex = 6;
            this.labelTxRxDistance.Text = "收发距离：";
            this.labelTxRxDistance.Click += new System.EventHandler(this.labelTxRxDistance_Click);
            // 
            // txtTxRxDistance
            // 
            this.txtTxRxDistance.BackColor = System.Drawing.SystemColors.Control;
            this.txtTxRxDistance.Location = new System.Drawing.Point(507, 10);
            this.txtTxRxDistance.Name = "txtTxRxDistance";
            this.txtTxRxDistance.ReadOnly = true;
            this.txtTxRxDistance.Size = new System.Drawing.Size(126, 28);
            this.txtTxRxDistance.TabIndex = 5;
            // 
            // labelCurrentInversionStation
            // 
            this.labelCurrentInversionStation.AutoSize = true;
            this.labelCurrentInversionStation.Location = new System.Drawing.Point(5, 42);
            this.labelCurrentInversionStation.Name = "labelCurrentInversionStation";
            this.labelCurrentInversionStation.Size = new System.Drawing.Size(134, 18);
            this.labelCurrentInversionStation.TabIndex = 4;
            this.labelCurrentInversionStation.Text = "当前反演测点：";
            // 
            // txtCurrentInversionStation
            // 
            this.txtCurrentInversionStation.BackColor = System.Drawing.SystemColors.Control;
            this.txtCurrentInversionStation.Location = new System.Drawing.Point(141, 38);
            this.txtCurrentInversionStation.Name = "txtCurrentInversionStation";
            this.txtCurrentInversionStation.ReadOnly = true;
            this.txtCurrentInversionStation.Size = new System.Drawing.Size(108, 28);
            this.txtCurrentInversionStation.TabIndex = 3;
            // 
            // labelCurrentSelectedStation
            // 
            this.labelCurrentSelectedStation.AutoSize = true;
            this.labelCurrentSelectedStation.Location = new System.Drawing.Point(5, 14);
            this.labelCurrentSelectedStation.Name = "labelCurrentSelectedStation";
            this.labelCurrentSelectedStation.Size = new System.Drawing.Size(134, 18);
            this.labelCurrentSelectedStation.TabIndex = 2;
            this.labelCurrentSelectedStation.Text = "当前选择测点：";
            // 
            // txtCurrentSelectedStation
            // 
            this.txtCurrentSelectedStation.BackColor = System.Drawing.SystemColors.Control;
            this.txtCurrentSelectedStation.Location = new System.Drawing.Point(141, 10);
            this.txtCurrentSelectedStation.Name = "txtCurrentSelectedStation";
            this.txtCurrentSelectedStation.ReadOnly = true;
            this.txtCurrentSelectedStation.Size = new System.Drawing.Size(108, 28);
            this.txtCurrentSelectedStation.TabIndex = 1;
            // 
            // tabPageData
            // 
            this.tabPageData.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageData.Controls.Add(this.gridData);
            this.tabPageData.Location = new System.Drawing.Point(4, 28);
            this.tabPageData.Name = "tabPageData";
            this.tabPageData.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageData.Size = new System.Drawing.Size(855, 857);
            this.tabPageData.TabIndex = 2;
            this.tabPageData.Text = "数据";
            // 
            // gridData
            // 
            this.gridData.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridData.Location = new System.Drawing.Point(2, 2);
            this.gridData.Name = "gridData";
            this.gridData.RowHeadersWidth = 45;
            this.gridData.RowTemplate.Height = 23;
            this.gridData.Size = new System.Drawing.Size(851, 853);
            this.gridData.TabIndex = 0;
            // 
            // tabPageModel
            // 
            this.tabPageModel.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageModel.Controls.Add(this.gridModelLayers);
            this.tabPageModel.Controls.Add(this.groupBoxModelParams);
            this.tabPageModel.Location = new System.Drawing.Point(4, 28);
            this.tabPageModel.Name = "tabPageModel";
            this.tabPageModel.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageModel.Size = new System.Drawing.Size(855, 857);
            this.tabPageModel.TabIndex = 3;
            this.tabPageModel.Text = "模型";
            // 
            // gridModelLayers
            // 
            this.gridModelLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridModelLayers.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridModelLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridModelLayers.Location = new System.Drawing.Point(7, 91);
            this.gridModelLayers.Name = "gridModelLayers";
            this.gridModelLayers.RowHeadersWidth = 45;
            this.gridModelLayers.RowTemplate.Height = 23;
            this.gridModelLayers.Size = new System.Drawing.Size(841, 759);
            this.gridModelLayers.TabIndex = 1;
            // 
            // groupBoxModelParams
            // 
            this.groupBoxModelParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxModelParams.Controls.Add(this.btnGenerateModel);
            this.groupBoxModelParams.Controls.Add(this.nudInitialResistivity);
            this.groupBoxModelParams.Controls.Add(this.labelInitialResistivity);
            this.groupBoxModelParams.Controls.Add(this.nudGrowthRate);
            this.groupBoxModelParams.Controls.Add(this.labelGrowthRate);
            this.groupBoxModelParams.Controls.Add(this.nudInitialThickness);
            this.groupBoxModelParams.Controls.Add(this.labelInitialThickness);
            this.groupBoxModelParams.Controls.Add(this.nudLayerCount);
            this.groupBoxModelParams.Controls.Add(this.labelLayerCount);
            this.groupBoxModelParams.Location = new System.Drawing.Point(7, 7);
            this.groupBoxModelParams.Name = "groupBoxModelParams";
            this.groupBoxModelParams.Size = new System.Drawing.Size(841, 80);
            this.groupBoxModelParams.TabIndex = 0;
            this.groupBoxModelParams.TabStop = false;
            this.groupBoxModelParams.Text = "模型参数";
            // 
            // btnGenerateModel
            // 
            this.btnGenerateModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerateModel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGenerateModel.Location = new System.Drawing.Point(763, 30);
            this.btnGenerateModel.Name = "btnGenerateModel";
            this.btnGenerateModel.Size = new System.Drawing.Size(67, 27);
            this.btnGenerateModel.TabIndex = 8;
            this.btnGenerateModel.Text = "生成模型";
            this.btnGenerateModel.UseVisualStyleBackColor = true;
            this.btnGenerateModel.Click += new System.EventHandler(this.btnGenerateModel_Click);
            // 
            // nudInitialResistivity
            // 
            this.nudInitialResistivity.DecimalPlaces = 1;
            this.nudInitialResistivity.Location = new System.Drawing.Point(458, 46);
            this.nudInitialResistivity.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudInitialResistivity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudInitialResistivity.Name = "nudInitialResistivity";
            this.nudInitialResistivity.Size = new System.Drawing.Size(100, 28);
            this.nudInitialResistivity.TabIndex = 7;
            this.nudInitialResistivity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelInitialResistivity
            // 
            this.labelInitialResistivity.AutoSize = true;
            this.labelInitialResistivity.Location = new System.Drawing.Point(281, 53);
            this.labelInitialResistivity.Name = "labelInitialResistivity";
            this.labelInitialResistivity.Size = new System.Drawing.Size(161, 18);
            this.labelInitialResistivity.TabIndex = 6;
            this.labelInitialResistivity.Text = "初始电阻率(Ω·m)";
            // 
            // nudGrowthRate
            // 
            this.nudGrowthRate.DecimalPlaces = 4;
            this.nudGrowthRate.Increment = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.nudGrowthRate.Location = new System.Drawing.Point(458, 19);
            this.nudGrowthRate.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudGrowthRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudGrowthRate.Name = "nudGrowthRate";
            this.nudGrowthRate.Size = new System.Drawing.Size(100, 28);
            this.nudGrowthRate.TabIndex = 5;
            this.nudGrowthRate.Value = new decimal(new int[] {
            12589,
            0,
            0,
            262144});
            // 
            // labelGrowthRate
            // 
            this.labelGrowthRate.AutoSize = true;
            this.labelGrowthRate.Location = new System.Drawing.Point(281, 26);
            this.labelGrowthRate.Name = "labelGrowthRate";
            this.labelGrowthRate.Size = new System.Drawing.Size(116, 18);
            this.labelGrowthRate.TabIndex = 4;
            this.labelGrowthRate.Text = "厚度增长倍: ";
            // 
            // nudInitialThickness
            // 
            this.nudInitialThickness.DecimalPlaces = 1;
            this.nudInitialThickness.Location = new System.Drawing.Point(140, 46);
            this.nudInitialThickness.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudInitialThickness.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudInitialThickness.Name = "nudInitialThickness";
            this.nudInitialThickness.Size = new System.Drawing.Size(100, 28);
            this.nudInitialThickness.TabIndex = 3;
            this.nudInitialThickness.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // labelInitialThickness
            // 
            this.labelInitialThickness.AutoSize = true;
            this.labelInitialThickness.Location = new System.Drawing.Point(11, 53);
            this.labelInitialThickness.Name = "labelInitialThickness";
            this.labelInitialThickness.Size = new System.Drawing.Size(107, 18);
            this.labelInitialThickness.TabIndex = 2;
            this.labelInitialThickness.Text = "首层厚度(m)";
            // 
            // nudLayerCount
            // 
            this.nudLayerCount.Location = new System.Drawing.Point(140, 19);
            this.nudLayerCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLayerCount.Name = "nudLayerCount";
            this.nudLayerCount.Size = new System.Drawing.Size(100, 28);
            this.nudLayerCount.TabIndex = 1;
            this.nudLayerCount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // labelLayerCount
            // 
            this.labelLayerCount.AutoSize = true;
            this.labelLayerCount.Location = new System.Drawing.Point(11, 26);
            this.labelLayerCount.Name = "labelLayerCount";
            this.labelLayerCount.Size = new System.Drawing.Size(80, 18);
            this.labelLayerCount.TabIndex = 0;
            this.labelLayerCount.Text = "层数(nL)";
            // 
            // labelStationLayer
            // 
            this.labelStationLayer.AutoSize = true;
            this.labelStationLayer.Location = new System.Drawing.Point(8, 18);
            this.labelStationLayer.Name = "labelStationLayer";
            this.labelStationLayer.Size = new System.Drawing.Size(125, 18);
            this.labelStationLayer.TabIndex = 4;
            this.labelStationLayer.Text = "选择测点图层:";
            // 
            // labelLineName
            // 
            this.labelLineName.AutoSize = true;
            this.labelLineName.Location = new System.Drawing.Point(338, 18);
            this.labelLineName.Name = "labelLineName";
            this.labelLineName.Size = new System.Drawing.Size(98, 18);
            this.labelLineName.TabIndex = 3;
            this.labelLineName.Text = "选择测线: ";
            // 
            // cmbLineName
            // 
            this.cmbLineName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLineName.BackColor = System.Drawing.SystemColors.Control;
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLineName.FormattingEnabled = true;
            this.cmbLineName.Location = new System.Drawing.Point(446, 15);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(406, 26);
            this.cmbLineName.TabIndex = 1;
            this.cmbLineName.SelectedIndexChanged += new System.EventHandler(this.cmbLineName_SelectedIndexChanged);
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.BackColor = System.Drawing.SystemColors.Control;
            this.cmbStationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStationLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbStationLayer.FormattingEnabled = true;
            this.cmbStationLayer.Location = new System.Drawing.Point(136, 14);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(196, 26);
            this.cmbStationLayer.TabIndex = 0;
            this.cmbStationLayer.SelectedIndexChanged += new System.EventHandler(this.cmbStationLayer_SelectedIndexChanged);
            // 
            // splitContainerRightMain
            // 
            this.splitContainerRightMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRightMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRightMain.Name = "splitContainerRightMain";
            this.splitContainerRightMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRightMain.Panel1
            // 
            this.splitContainerRightMain.Panel1.Controls.Add(this.splitContainerRight);
            // 
            // splitContainerRightMain.Panel2
            // 
            this.splitContainerRightMain.Panel2.Controls.Add(this.chartResultSection);
            this.splitContainerRightMain.Panel2.Controls.Add(this.panelResultControl);
<<<<<<< HEAD
            this.splitContainerRightMain.Size = new System.Drawing.Size(783, 939);
            this.splitContainerRightMain.SplitterDistance = 472;
=======
            this.splitContainerRightMain.Size = new System.Drawing.Size(606, 676);
            this.splitContainerRightMain.SplitterDistance = 340;
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.splitContainerRightMain.TabIndex = 0;
            // 
            // splitContainerRight
            // 
            this.splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRight.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRight.Name = "splitContainerRight";
            // 
            // splitContainerRight.Panel1
            // 
            this.splitContainerRight.Panel1.Controls.Add(this.chartResistivity);
            // 
            // splitContainerRight.Panel2
            // 
            this.splitContainerRight.Panel2.Controls.Add(this.chartPhase);
<<<<<<< HEAD
            this.splitContainerRight.Size = new System.Drawing.Size(783, 472);
            this.splitContainerRight.SplitterDistance = 391;
=======
            this.splitContainerRight.Size = new System.Drawing.Size(606, 340);
            this.splitContainerRight.SplitterDistance = 303;
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.splitContainerRight.TabIndex = 0;
            // 
            // chartResistivity
            // 
<<<<<<< HEAD
            chartArea3.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea3);
=======
            chartArea6.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea6);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartResistivity.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.BackColor = System.Drawing.SystemColors.Control;
            legend2.Name = "Legend1";
            this.chartResistivity.Legends.Add(legend2);
            this.chartResistivity.Location = new System.Drawing.Point(0, 0);
            this.chartResistivity.Name = "chartResistivity";
<<<<<<< HEAD
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chartResistivity.Series.Add(series3);
            this.chartResistivity.Size = new System.Drawing.Size(391, 472);
=======
            series6.ChartArea = "ChartArea1";
            series6.Legend = "Legend1";
            series6.Name = "Series1";
            this.chartResistivity.Series.Add(series6);
            this.chartResistivity.Size = new System.Drawing.Size(303, 340);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartResistivity.TabIndex = 1;
            this.chartResistivity.Text = "chartResistivity";
            this.chartResistivity.UseWaitCursor = true;
            // 
            // chartPhase
            // 
<<<<<<< HEAD
            chartArea4.Name = "ChartArea1";
            this.chartPhase.ChartAreas.Add(chartArea4);
=======
            chartArea7.Name = "ChartArea1";
            this.chartPhase.ChartAreas.Add(chartArea7);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartPhase.Dock = System.Windows.Forms.DockStyle.Fill;
            legend3.BackColor = System.Drawing.SystemColors.Control;
            legend3.Name = "Legend1";
            this.chartPhase.Legends.Add(legend3);
            this.chartPhase.Location = new System.Drawing.Point(0, 0);
            this.chartPhase.Name = "chartPhase";
<<<<<<< HEAD
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chartPhase.Series.Add(series4);
            this.chartPhase.Size = new System.Drawing.Size(388, 472);
=======
            series7.ChartArea = "ChartArea1";
            series7.Legend = "Legend1";
            series7.Name = "Series1";
            this.chartPhase.Series.Add(series7);
            this.chartPhase.Size = new System.Drawing.Size(299, 340);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartPhase.TabIndex = 2;
            this.chartPhase.Text = "chartPhase";
            // 
            // chartResultSection
            // 
            this.chartResultSection.BackColor = System.Drawing.SystemColors.Control;
<<<<<<< HEAD
            chartArea5.Name = "ChartArea1";
            this.chartResultSection.ChartAreas.Add(chartArea5);
=======
            chartArea8.Name = "ChartArea1";
            this.chartResultSection.ChartAreas.Add(chartArea8);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartResultSection.Dock = System.Windows.Forms.DockStyle.Fill;
            legend4.BackColor = System.Drawing.SystemColors.Control;
            legend4.Name = "Legend1";
            this.chartResultSection.Legends.Add(legend4);
            this.chartResultSection.Location = new System.Drawing.Point(0, 35);
            this.chartResultSection.Name = "chartResultSection";
<<<<<<< HEAD
            series5.ChartArea = "ChartArea1";
            series5.Legend = "Legend1";
            series5.Name = "Series1";
            this.chartResultSection.Series.Add(series5);
            this.chartResultSection.Size = new System.Drawing.Size(783, 428);
=======
            series8.ChartArea = "ChartArea1";
            series8.Legend = "Legend1";
            series8.Name = "Series1";
            this.chartResultSection.Series.Add(series8);
            this.chartResultSection.Size = new System.Drawing.Size(606, 297);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.chartResultSection.TabIndex = 0;
            this.chartResultSection.Text = "chartResultSection";
            // 
            // panelResultControl
            // 
            this.panelResultControl.Controls.Add(this.button1);
            this.panelResultControl.Controls.Add(this.nudMaxDepth);
            this.panelResultControl.Controls.Add(this.labelDepthRange);
            this.panelResultControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelResultControl.Location = new System.Drawing.Point(0, 0);
            this.panelResultControl.Name = "panelResultControl";
            this.panelResultControl.Size = new System.Drawing.Size(783, 35);
            this.panelResultControl.TabIndex = 1;
            // 
            // nudMaxDepth
            // 
            this.nudMaxDepth.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxDepth.Location = new System.Drawing.Point(168, 4);
            this.nudMaxDepth.Maximum = new decimal(new int[] {
            200000,
            0,
            0,
            0});
            this.nudMaxDepth.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxDepth.Name = "nudMaxDepth";
            this.nudMaxDepth.Size = new System.Drawing.Size(100, 28);
            this.nudMaxDepth.TabIndex = 1;
            this.nudMaxDepth.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxDepth.ValueChanged += new System.EventHandler(this.nudMaxDepth_ValueChanged);
            // 
            // labelDepthRange
            // 
            this.labelDepthRange.AutoSize = true;
            this.labelDepthRange.Location = new System.Drawing.Point(10, 10);
            this.labelDepthRange.Name = "labelDepthRange";
            this.labelDepthRange.Size = new System.Drawing.Size(152, 18);
            this.labelDepthRange.TabIndex = 0;
            this.labelDepthRange.Text = "显示最大深度(m):";
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.SystemColors.Control;
            this.panelTitle.Controls.Add(this.labelTitle);
            this.panelTitle.Controls.Add(this.btnClose);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Name = "panelTitle";
<<<<<<< HEAD
            this.panelTitle.Size = new System.Drawing.Size(1650, 36);
=======
            this.panelTitle.Size = new System.Drawing.Size(1280, 24);
>>>>>>> d166c7fefb1fb4d9fcda7470237e97968e645534
            this.panelTitle.TabIndex = 6;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(149, 24);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "CSAMT 一维反演";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("宋体", 6.75F);
            this.btnClose.Location = new System.Drawing.Point(1620, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(30, 24);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // openFileDialogTran
            // 
            this.openFileDialogTran.FileName = "tran.dat";
            this.openFileDialogTran.Filter = "发射源文件 (*. dat)|*.dat|所有文件 (*.*)|*.*";
            this.openFileDialogTran.Title = "请选择发射源文件";
            this.openFileDialogTran.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialogTran_FileOk);
            // 
            // txtAz
            // 
            this.txtAz.Location = new System.Drawing.Point(0, 0);
            this.txtAz.Name = "txtAz";
            this.txtAz.Size = new System.Drawing.Size(7, 28);
            this.txtAz.TabIndex = 0;
            this.txtAz.Visible = false;
            // 
            // txtAy
            // 
            this.txtAy.Location = new System.Drawing.Point(0, 0);
            this.txtAy.Name = "txtAy";
            this.txtAy.Size = new System.Drawing.Size(7, 28);
            this.txtAy.TabIndex = 0;
            this.txtAy.Visible = false;
            // 
            // txtAx
            // 
            this.txtAx.Location = new System.Drawing.Point(0, 0);
            this.txtAx.Name = "txtAx";
            this.txtAx.Size = new System.Drawing.Size(7, 28);
            this.txtAx.TabIndex = 0;
            this.txtAx.Visible = false;
            // 
            // txtBz
            // 
            this.txtBz.Location = new System.Drawing.Point(0, 0);
            this.txtBz.Name = "txtBz";
            this.txtBz.Size = new System.Drawing.Size(7, 28);
            this.txtBz.TabIndex = 0;
            this.txtBz.Visible = false;
            // 
            // txtBy
            // 
            this.txtBy.Location = new System.Drawing.Point(0, 0);
            this.txtBy.Name = "txtBy";
            this.txtBy.Size = new System.Drawing.Size(7, 28);
            this.txtBy.TabIndex = 0;
            this.txtBy.Visible = false;
            // 
            // txtBx
            // 
            this.txtBx.Location = new System.Drawing.Point(0, 0);
            this.txtBx.Name = "txtBx";
            this.txtBx.Size = new System.Drawing.Size(7, 28);
            this.txtBx.TabIndex = 0;
            this.txtBx.Visible = false;
            // 
            // gridLayout
            // 
            this.gridLayout.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLayout.Location = new System.Drawing.Point(0, 0);
            this.gridLayout.Name = "gridLayout";
            this.gridLayout.RowHeadersWidth = 45;
            this.gridLayout.RowTemplate.Height = 23;
            this.gridLayout.Size = new System.Drawing.Size(160, 100);
            this.gridLayout.TabIndex = 0;
            this.gridLayout.Visible = false;
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(513, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "一键成图";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form_CSAMT1di
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1650, 975);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.panelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form_CSAMT1di";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "可控源音频大地电磁法一维反演";
            this.Load += new System.EventHandler(this.Form_CSAMT1di_Load);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel1.PerformLayout();
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageCalculate.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridCalc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).EndInit();
            this.groupBoxCalcParams.ResumeLayout(false);
            this.groupBoxCalcParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).EndInit();
            this.tabPageLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartLayout)).EndInit();
            this.panelLayoutTop.ResumeLayout(false);
            this.panelLayoutTop.PerformLayout();
            this.tabPageData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).EndInit();
            this.tabPageModel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridModelLayers)).EndInit();
            this.groupBoxModelParams.ResumeLayout(false);
            this.groupBoxModelParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialResistivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrowthRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLayerCount)).EndInit();
            this.splitContainerRightMain.Panel1.ResumeLayout(false);
            this.splitContainerRightMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRightMain)).EndInit();
            this.splitContainerRightMain.ResumeLayout(false);
            this.splitContainerRight.Panel1.ResumeLayout(false);
            this.splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
            this.splitContainerRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartResultSection)).EndInit();
            this.panelResultControl.ResumeLayout(false);
            this.panelResultControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth)).EndInit();
            this.panelTitle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridLayout)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageCalculate;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProfileView;
        private System.Windows.Forms.TabPage tabPageLayout;
        private System.Windows.Forms.Label labelStationLayer;
        private System.Windows.Forms.Label labelLineName;
        private System.Windows.Forms.ComboBox cmbLineName;
        private System.Windows.Forms.ComboBox cmbStationLayer;
        private System.Windows.Forms.TabPage tabPageData;
        private System.Windows.Forms.DataGridView gridData;
        private System.Windows.Forms.TabPage tabPageModel;
        private System.Windows.Forms.DataGridView gridModelLayers;
        private System.Windows.Forms.GroupBox groupBoxModelParams;
        private System.Windows.Forms.Button btnGenerateModel;
        private System.Windows.Forms.NumericUpDown nudInitialResistivity;
        private System.Windows.Forms.Label labelInitialResistivity;
        private System.Windows.Forms.NumericUpDown nudGrowthRate;
        private System.Windows.Forms.Label labelGrowthRate;
        private System.Windows.Forms.NumericUpDown nudInitialThickness;
        private System.Windows.Forms.Label labelInitialThickness;
        private System.Windows.Forms.NumericUpDown nudLayerCount;
        private System.Windows.Forms.Label labelLayerCount;
        private System.Windows.Forms.GroupBox groupBoxCalcParams;
        private System.Windows.Forms.NumericUpDown nudIterationCount;
        private System.Windows.Forms.Label labelIter;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.CheckBox chkJointInversion;
        private System.Windows.Forms.CheckBox chkUseCurrentStation;
        private System.Windows.Forms.TextBox txtAllowError;
        private System.Windows.Forms.Label labelAllowError;
        private System.Windows.Forms.TextBox txtActualError;
        private System.Windows.Forms.Label labelActualError;
        private System.Windows.Forms.ProgressBar progressBarCalc;
        private System.Windows.Forms.Panel panelLayoutTop;
        private System.Windows.Forms.Label labelCurrentSelectedStation;
        private System.Windows.Forms.TextBox txtCurrentSelectedStation;
        private System.Windows.Forms.Label labelCurrentInversionStation;
        private System.Windows.Forms.TextBox txtCurrentInversionStation;
        private System.Windows.Forms.Label labelTxRxDistance;
        private System.Windows.Forms.TextBox txtTxRxDistance;
        private System.Windows.Forms.Label labelOffsetFromCenter;
        private System.Windows.Forms.TextBox txtOffsetFromCenter;
        private System.Windows.Forms.DataGridView gridCalc;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartLayout;
        private System.Windows.Forms.SplitContainer splitContainerRightMain;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResistivity;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPhase;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResultSection;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.OpenFileDialog openFileDialogTran;
        private System.Windows.Forms.TextBox txtAz;
        private System.Windows.Forms.TextBox txtAy;
        private System.Windows.Forms.TextBox txtAx;
        private System.Windows.Forms.TextBox txtBz;
        private System.Windows.Forms.TextBox txtBy;
        private System.Windows.Forms.TextBox txtBx;
        private System.Windows.Forms.DataGridView gridLayout;
        // 在类的字段声明区域添加（约在 gridLayout 声明附近）
        private System.Windows.Forms.Label labelDepthRange;
        private System.Windows.Forms.NumericUpDown nudMaxDepth;
        private System.Windows.Forms.Panel panelResultControl;
        private System.Windows.Forms.Label labelProgressPercent;
        private System.Windows.Forms.Button button1;
    }
}