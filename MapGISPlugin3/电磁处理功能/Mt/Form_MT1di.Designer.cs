using MapGIS.GeoMap;
using MapGIS.PluginEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace MapGISPlugin3
{

    partial class Form_MT1di
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBoxParams = new System.Windows.Forms.GroupBox();
            this.txtActualError = new System.Windows.Forms.TextBox();
            this.labelActualError = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.nudIterationCount = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rbInversionTM = new System.Windows.Forms.RadioButton();
            this.rbInversionTE = new System.Windows.Forms.RadioButton();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.chartProfileView = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gridTE = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.gridTM = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbLineName = new System.Windows.Forms.ComboBox();
            this.cmbStationLayer = new System.Windows.Forms.ComboBox();
            this.panelResultBox = new System.Windows.Forms.Panel();
            this.labelDepthRange = new System.Windows.Forms.Label();
            this.chartResultSection = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.nudMaxDepth = new System.Windows.Forms.NumericUpDown();
            this.chartPhase = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPageDisplayTE = new System.Windows.Forms.TabPage();
            this.tabPageDisplayTM = new System.Windows.Forms.TabPage();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBoxParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTE)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTM)).BeginInit();
            this.panelResultBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResultSection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).BeginInit();
            this.tabControl2.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
            this.splitContainer1.Panel2.Controls.Add(this.panelResultBox);
            this.splitContainer1.Panel2.Controls.Add(this.chartPhase);
            this.splitContainer1.Panel2.Controls.Add(this.chartResistivity);
            this.splitContainer1.Panel2.Controls.Add(this.tabControl2);
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(1100, 618);
            this.splitContainer1.SplitterDistance = 558;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.Location = new System.Drawing.Point(0, 48);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(558, 570);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.groupBoxParams);
            this.tabPage1.Controls.Add(this.chartProfileView);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(550, 544);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "计算";
            // 
            // groupBoxParams
            // 
            this.groupBoxParams.Controls.Add(this.txtActualError);
            this.groupBoxParams.Controls.Add(this.labelActualError);
            this.groupBoxParams.Controls.Add(this.progressBar1);
            this.groupBoxParams.Controls.Add(this.nudIterationCount);
            this.groupBoxParams.Controls.Add(this.label4);
            this.groupBoxParams.Controls.Add(this.label3);
            this.groupBoxParams.Controls.Add(this.rbInversionTM);
            this.groupBoxParams.Controls.Add(this.rbInversionTE);
            this.groupBoxParams.Controls.Add(this.btnCalculate);
            this.groupBoxParams.Location = new System.Drawing.Point(9, 10);
            this.groupBoxParams.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxParams.Name = "groupBoxParams";
            this.groupBoxParams.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxParams.Size = new System.Drawing.Size(544, 63);
            this.groupBoxParams.TabIndex = 1;
            this.groupBoxParams.TabStop = false;
            this.groupBoxParams.Text = "计算参数";
            // 
            // txtActualError
            // 
            this.txtActualError.BackColor = System.Drawing.SystemColors.Control;
            this.txtActualError.Location = new System.Drawing.Point(236, 41);
            this.txtActualError.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtActualError.Name = "txtActualError";
            this.txtActualError.ReadOnly = true;
            this.txtActualError.Size = new System.Drawing.Size(68, 21);
            this.txtActualError.TabIndex = 11;
            // 
            // labelActualError
            // 
            this.labelActualError.AutoSize = true;
            this.labelActualError.Location = new System.Drawing.Point(152, 43);
            this.labelActualError.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelActualError.Name = "labelActualError";
            this.labelActualError.Size = new System.Drawing.Size(89, 12);
            this.labelActualError.TabIndex = 10;
            this.labelActualError.Text = "实际迭代误差：";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(374, 25);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(158, 19);
            this.progressBar1.TabIndex = 9;
            // 
            // nudIterationCount
            // 
            this.nudIterationCount.BackColor = System.Drawing.SystemColors.Control;
            this.nudIterationCount.Location = new System.Drawing.Point(236, 17);
            this.nudIterationCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nudIterationCount.Name = "nudIterationCount";
            this.nudIterationCount.Size = new System.Drawing.Size(59, 21);
            this.nudIterationCount.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(150, 20);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "迭代次数：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 33);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "反演模式：";
            // 
            // rbInversionTM
            // 
            this.rbInversionTM.AutoSize = true;
            this.rbInversionTM.Location = new System.Drawing.Point(85, 41);
            this.rbInversionTM.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbInversionTM.Name = "rbInversionTM";
            this.rbInversionTM.Size = new System.Drawing.Size(42, 20);
            this.rbInversionTM.TabIndex = 2;
            this.rbInversionTM.Text = "TM";
            this.rbInversionTM.UseVisualStyleBackColor = true;
            // 
            // rbInversionTE
            // 
            this.rbInversionTE.AutoSize = true;
            this.rbInversionTE.Checked = true;
            this.rbInversionTE.Location = new System.Drawing.Point(85, 21);
            this.rbInversionTE.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbInversionTE.Name = "rbInversionTE";
            this.rbInversionTE.Size = new System.Drawing.Size(42, 20);
            this.rbInversionTE.TabIndex = 1;
            this.rbInversionTE.TabStop = true;
            this.rbInversionTE.Text = "TE";
            this.rbInversionTE.UseVisualStyleBackColor = true;
            // 
            // btnCalculate
            // 
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(316, 21);
            this.btnCalculate.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(52, 27);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // chartProfileView
            // 
            chartArea1.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea1);
            this.chartProfileView.Location = new System.Drawing.Point(7, 77);
            this.chartProfileView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chartProfileView.Name = "chartProfileView";
            this.chartProfileView.Size = new System.Drawing.Size(547, 467);
            this.chartProfileView.TabIndex = 0;
            this.chartProfileView.Text = "chart1";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gridTE);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(559, 544);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TE";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gridTE
            // 
            this.gridTE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTE.Location = new System.Drawing.Point(2, 2);
            this.gridTE.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gridTE.Name = "gridTE";
            this.gridTE.RowHeadersWidth = 62;
            this.gridTE.RowTemplate.Height = 30;
            this.gridTE.Size = new System.Drawing.Size(555, 540);
            this.gridTE.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.gridTM);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Size = new System.Drawing.Size(559, 544);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "TM";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // gridTM
            // 
            this.gridTM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTM.Location = new System.Drawing.Point(2, 2);
            this.gridTM.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gridTM.Name = "gridTM";
            this.gridTM.RowHeadersWidth = 62;
            this.gridTM.RowTemplate.Height = 30;
            this.gridTM.Size = new System.Drawing.Size(555, 540);
            this.gridTM.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择测点图层:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(297, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "选择测线:";
            // 
            // cmbLineName
            // 
            this.cmbLineName.BackColor = System.Drawing.SystemColors.Control;
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLineName.FormattingEnabled = true;
            this.cmbLineName.Location = new System.Drawing.Point(367, 13);
            this.cmbLineName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(159, 20);
            this.cmbLineName.TabIndex = 1;
            this.cmbLineName.SelectedIndexChanged += new System.EventHandler(this.cmbLineName_SelectedIndexChanged);
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.BackColor = System.Drawing.SystemColors.Control;
            this.cmbStationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStationLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbStationLayer.FormattingEnabled = true;
            this.cmbStationLayer.Location = new System.Drawing.Point(98, 13);
            this.cmbStationLayer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(159, 20);
            this.cmbStationLayer.TabIndex = 0;
            this.cmbStationLayer.SelectedIndexChanged += new System.EventHandler(this.cmbStationLayer_SelectedIndexChanged);
            // 
            // panelResultBox
            // 
            this.panelResultBox.Controls.Add(this.button1);
            this.panelResultBox.Controls.Add(this.labelDepthRange);
            this.panelResultBox.Controls.Add(this.chartResultSection);
            this.panelResultBox.Controls.Add(this.nudMaxDepth);
            this.panelResultBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelResultBox.Location = new System.Drawing.Point(0, 298);
            this.panelResultBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelResultBox.Name = "panelResultBox";
            this.panelResultBox.Size = new System.Drawing.Size(539, 320);
            this.panelResultBox.TabIndex = 4;
            this.panelResultBox.Paint += new System.Windows.Forms.PaintEventHandler(this.panelResultBox_Paint);
            // 
            // labelDepthRange
            // 
            this.labelDepthRange.AutoSize = true;
            this.labelDepthRange.Location = new System.Drawing.Point(3, 3);
            this.labelDepthRange.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDepthRange.Name = "labelDepthRange";
            this.labelDepthRange.Size = new System.Drawing.Size(101, 12);
            this.labelDepthRange.TabIndex = 9;
            this.labelDepthRange.Text = "显示最大深度(m):";
            // 
            // chartResultSection
            // 
            this.chartResultSection.BackColor = System.Drawing.SystemColors.Control;
            chartArea2.Name = "ChartArea1";
            this.chartResultSection.ChartAreas.Add(chartArea2);
            this.chartResultSection.Dock = System.Windows.Forms.DockStyle.Bottom;
            legend1.Name = "Legend1";
            this.chartResultSection.Legends.Add(legend1);
            this.chartResultSection.Location = new System.Drawing.Point(0, 20);
            this.chartResultSection.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chartResultSection.Name = "chartResultSection";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartResultSection.Series.Add(series1);
            this.chartResultSection.Size = new System.Drawing.Size(539, 300);
            this.chartResultSection.TabIndex = 2;
            this.chartResultSection.Text = "chart1";
            // 
            // nudMaxDepth
            // 
            this.nudMaxDepth.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxDepth.Location = new System.Drawing.Point(102, 0);
            this.nudMaxDepth.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nudMaxDepth.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudMaxDepth.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxDepth.Name = "nudMaxDepth";
            this.nudMaxDepth.Size = new System.Drawing.Size(82, 21);
            this.nudMaxDepth.TabIndex = 8;
            this.nudMaxDepth.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // chartPhase
            // 
            this.chartPhase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chartArea3.Name = "ChartArea1";
            this.chartPhase.ChartAreas.Add(chartArea3);
            this.chartPhase.Location = new System.Drawing.Point(263, 18);
            this.chartPhase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chartPhase.Name = "chartPhase";
            this.chartPhase.Size = new System.Drawing.Size(257, 276);
            this.chartPhase.TabIndex = 2;
            this.chartPhase.Text = "chart3";
            // 
            // chartResistivity
            // 
            chartArea4.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea4);
            this.chartResistivity.Location = new System.Drawing.Point(2, 18);
            this.chartResistivity.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chartResistivity.Name = "chartResistivity";
            this.chartResistivity.Size = new System.Drawing.Size(257, 276);
            this.chartResistivity.TabIndex = 1;
            this.chartResistivity.Text = "chart2";
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.tabControl2.Controls.Add(this.tabPageDisplayTE);
            this.tabControl2.Controls.Add(this.tabPageDisplayTM);
            this.tabControl2.Location = new System.Drawing.Point(6, 2);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(520, 19);
            this.tabControl2.TabIndex = 0;
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
            // 
            // tabPageDisplayTE
            // 
            this.tabPageDisplayTE.Location = new System.Drawing.Point(4, 22);
            this.tabPageDisplayTE.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageDisplayTE.Name = "tabPageDisplayTE";
            this.tabPageDisplayTE.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageDisplayTE.Size = new System.Drawing.Size(512, 0);
            this.tabPageDisplayTE.TabIndex = 0;
            this.tabPageDisplayTE.Text = "TE";
            this.tabPageDisplayTE.UseVisualStyleBackColor = true;
            // 
            // tabPageDisplayTM
            // 
            this.tabPageDisplayTM.Location = new System.Drawing.Point(4, 22);
            this.tabPageDisplayTM.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageDisplayTM.Name = "tabPageDisplayTM";
            this.tabPageDisplayTM.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageDisplayTM.Size = new System.Drawing.Size(512, 0);
            this.tabPageDisplayTM.TabIndex = 1;
            this.tabPageDisplayTM.Text = "TM";
            this.tabPageDisplayTM.UseVisualStyleBackColor = true;
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.SystemColors.Control;
            this.panelTitle.Controls.Add(this.labelTitle);
            this.panelTitle.Controls.Add(this.btnClose);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(1100, 32);
            this.panelTitle.TabIndex = 6;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(100, 32);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "MT一维反演";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("宋体", 6.75F);
            this.btnClose.Location = new System.Drawing.Point(1071, 0);
            this.btnClose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(29, 32);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(464, 1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "一键成图";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form_MT1di
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form_MT1di";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MT数据1D分析";
            this.Load += new System.EventHandler(this.Form_MT1di_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBoxParams.ResumeLayout(false);
            this.groupBoxParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTE)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTM)).EndInit();
            this.panelResultBox.ResumeLayout(false);
            this.panelResultBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResultSection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).EndInit();
            this.tabControl2.ResumeLayout(false);
            this.panelTitle.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        // 新增控件声明
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnClose;

        // 原有控件声明（保持不变）
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBoxParams;
        private System.Windows.Forms.NumericUpDown nudIterationCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rbInversionTM;
        private System.Windows.Forms.RadioButton rbInversionTE;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProfileView;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbLineName;
        private System.Windows.Forms.ComboBox cmbStationLayer;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView gridTE;
        private System.Windows.Forms.DataGridView gridTM;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPageDisplayTM;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResistivity;
        private System.Windows.Forms.TabPage tabPageDisplayTE;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPhase;
        private Panel panelResultBox;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResultSection;
        private NumericUpDown nudMaxDepth;
        private ProgressBar progressBar1;
        private Label labelDepthRange;
        private TextBox txtActualError;
        private Label labelActualError;
        private Button button1;
    }
}