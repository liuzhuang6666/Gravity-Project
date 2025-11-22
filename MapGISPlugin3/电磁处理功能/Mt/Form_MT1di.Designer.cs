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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBoxParams = new System.Windows.Forms.GroupBox();
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
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPageDisplayTE = new System.Windows.Forms.TabPage();
            this.tabPageDisplayTM = new System.Windows.Forms.TabPage();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.chartPhase = new System.Windows.Forms.DataVisualization.Charting.Chart();
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
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).BeginInit();
            this.tabControl2.SuspendLayout();
            this.panelTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).BeginInit();
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
            this.splitContainer1.Panel2.Controls.Add(this.chartPhase);
            this.splitContainer1.Panel2.Controls.Add(this.chartResistivity);
            this.splitContainer1.Panel2.Controls.Add(this.tabControl2);
            this.splitContainer1.Size = new System.Drawing.Size(1485, 927);
            this.splitContainer1.SplitterDistance = 849;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.Location = new System.Drawing.Point(0, 72);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(849, 855);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.groupBoxParams);
            this.tabPage1.Controls.Add(this.chartProfileView);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage1.Size = new System.Drawing.Size(841, 823);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "计算";
            // 
            // groupBoxParams
            // 
            this.groupBoxParams.Controls.Add(this.nudIterationCount);
            this.groupBoxParams.Controls.Add(this.label4);
            this.groupBoxParams.Controls.Add(this.label3);
            this.groupBoxParams.Controls.Add(this.rbInversionTM);
            this.groupBoxParams.Controls.Add(this.rbInversionTE);
            this.groupBoxParams.Controls.Add(this.btnCalculate);
            this.groupBoxParams.Location = new System.Drawing.Point(14, 15);
            this.groupBoxParams.Name = "groupBoxParams";
            this.groupBoxParams.Size = new System.Drawing.Size(778, 94);
            this.groupBoxParams.TabIndex = 1;
            this.groupBoxParams.TabStop = false;
            this.groupBoxParams.Text = "计算参数";
            // 
            // nudIterationCount
            // 
            this.nudIterationCount.BackColor = System.Drawing.SystemColors.Control;
            this.nudIterationCount.Location = new System.Drawing.Point(376, 38);
            this.nudIterationCount.Name = "nudIterationCount";
            this.nudIterationCount.Size = new System.Drawing.Size(198, 28);
            this.nudIterationCount.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(284, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 4;
            this.label4.Text = "迭代次数：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 18);
            this.label3.TabIndex = 3;
            this.label3.Text = "反演模式：";
            // 
            // rbInversionTM
            // 
            this.rbInversionTM.AutoSize = true;
            this.rbInversionTM.Location = new System.Drawing.Point(152, 56);
            this.rbInversionTM.Name = "rbInversionTM";
            this.rbInversionTM.Size = new System.Drawing.Size(51, 22);
            this.rbInversionTM.TabIndex = 2;
            this.rbInversionTM.Text = "TM";
            this.rbInversionTM.UseVisualStyleBackColor = true;
            // 
            // rbInversionTE
            // 
            this.rbInversionTE.AutoSize = true;
            this.rbInversionTE.Checked = true;
            this.rbInversionTE.Location = new System.Drawing.Point(152, 27);
            this.rbInversionTE.Name = "rbInversionTE";
            this.rbInversionTE.Size = new System.Drawing.Size(51, 22);
            this.rbInversionTE.TabIndex = 1;
            this.rbInversionTE.TabStop = true;
            this.rbInversionTE.Text = "TE";
            this.rbInversionTE.UseVisualStyleBackColor = true;
            // 
            // btnCalculate
            // 
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(634, 32);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(78, 40);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // chartProfileView
            // 
            chartArea6.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea6);
            this.chartProfileView.Location = new System.Drawing.Point(10, 115);
            this.chartProfileView.Name = "chartProfileView";
            this.chartProfileView.Size = new System.Drawing.Size(820, 712);
            this.chartProfileView.TabIndex = 0;
            this.chartProfileView.Text = "chart1";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gridTE);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage2.Size = new System.Drawing.Size(841, 823);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TE";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gridTE
            // 
            this.gridTE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTE.Location = new System.Drawing.Point(3, 3);
            this.gridTE.Name = "gridTE";
            this.gridTE.RowHeadersWidth = 62;
            this.gridTE.RowTemplate.Height = 30;
            this.gridTE.Size = new System.Drawing.Size(835, 817);
            this.gridTE.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.gridTM);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage3.Size = new System.Drawing.Size(841, 823);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "TM";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // gridTM
            // 
            this.gridTM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTM.Location = new System.Drawing.Point(3, 3);
            this.gridTM.Name = "gridTM";
            this.gridTM.RowHeadersWidth = 62;
            this.gridTM.RowTemplate.Height = 30;
            this.gridTM.Size = new System.Drawing.Size(835, 817);
            this.gridTM.TabIndex = 0;
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
            this.label2.Location = new System.Drawing.Point(446, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "选择测线:";
            // 
            // cmbLineName
            // 
            this.cmbLineName.BackColor = System.Drawing.SystemColors.Control;
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLineName.FormattingEnabled = true;
            this.cmbLineName.Location = new System.Drawing.Point(550, 20);
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
            // chartResistivity
            // 
            chartArea5.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea5);
            this.chartResistivity.Location = new System.Drawing.Point(10, 27);
            this.chartResistivity.Name = "chartResistivity";
            this.chartResistivity.Size = new System.Drawing.Size(606, 414);
            this.chartResistivity.TabIndex = 1;
            this.chartResistivity.Text = "chart2";
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.tabControl2.Controls.Add(this.tabPageDisplayTE);
            this.tabControl2.Controls.Add(this.tabPageDisplayTM);
            this.tabControl2.Location = new System.Drawing.Point(8, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(622, 28);
            this.tabControl2.TabIndex = 0;
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
            // 
            // tabPageDisplayTE
            // 
            this.tabPageDisplayTE.Location = new System.Drawing.Point(4, 28);
            this.tabPageDisplayTE.Name = "tabPageDisplayTE";
            this.tabPageDisplayTE.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPageDisplayTE.Size = new System.Drawing.Size(614, 0);
            this.tabPageDisplayTE.TabIndex = 0;
            this.tabPageDisplayTE.Text = "TE";
            this.tabPageDisplayTE.UseVisualStyleBackColor = true;
            // 
            // tabPageDisplayTM
            // 
            this.tabPageDisplayTM.Location = new System.Drawing.Point(4, 28);
            this.tabPageDisplayTM.Name = "tabPageDisplayTM";
            this.tabPageDisplayTM.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPageDisplayTM.Size = new System.Drawing.Size(624, 0);
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
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(1485, 48);
            this.panelTitle.TabIndex = 6;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(150, 48);
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
            this.btnClose.Location = new System.Drawing.Point(1441, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(44, 48);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // chartPhase
            // 
            this.chartPhase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chartArea4.Name = "ChartArea1";
            this.chartPhase.ChartAreas.Add(chartArea4);
            this.chartPhase.Location = new System.Drawing.Point(10, 438);
            this.chartPhase.Name = "chartPhase";
            this.chartPhase.Size = new System.Drawing.Size(606, 485);
            this.chartPhase.TabIndex = 2;
            this.chartPhase.Text = "chart3";
            // 
            // Form_MT1di
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1485, 975);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
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
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).EndInit();
            this.tabControl2.ResumeLayout(false);
            this.panelTitle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).EndInit();
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
    }
}