
namespace MapGISPlugin3
{
    partial class Form_MT1di
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cmbStationLayer = new System.Windows.Forms.ComboBox();
            this.cmbLineName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chartProfileView = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBoxParams = new System.Windows.Forms.GroupBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.gridTE = new System.Windows.Forms.DataGridView();
            this.gridTM = new System.Windows.Forms.DataGridView();
            this.rbInversionTE = new System.Windows.Forms.RadioButton();
            this.rbInversionTM = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nudIterationCount = new System.Windows.Forms.NumericUpDown();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPageDisplayTM = new System.Windows.Forms.TabPage();
            this.tabPageDisplayTE = new System.Windows.Forms.TabPage();
            this.chartResistivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartPhase = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).BeginInit();
            this.groupBoxParams.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).BeginInit();
            this.tabControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(1434, 824);
            this.splitContainer1.SplitterDistance = 821;
            this.splitContainer1.TabIndex = 0;
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.FormattingEnabled = true;
            this.cmbStationLayer.Location = new System.Drawing.Point(147, 19);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(236, 26);
            this.cmbStationLayer.TabIndex = 0;
            // 
            // cmbLineName
            // 
            this.cmbLineName.FormattingEnabled = true;
            this.cmbLineName.Location = new System.Drawing.Point(551, 19);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(236, 26);
            this.cmbLineName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(446, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "选择测线:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 18);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择测点图层:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.Location = new System.Drawing.Point(0, 51);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(821, 773);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBoxParams);
            this.tabPage1.Controls.Add(this.chartProfileView);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(813, 741);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "计算";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gridTE);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(813, 741);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TE";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chartProfileView
            // 
            chartArea1.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartProfileView.Legends.Add(legend1);
            this.chartProfileView.Location = new System.Drawing.Point(11, 116);
            this.chartProfileView.Name = "chartProfileView";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartProfileView.Series.Add(series1);
            this.chartProfileView.Size = new System.Drawing.Size(782, 619);
            this.chartProfileView.TabIndex = 0;
            this.chartProfileView.Text = "chart1";
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
            this.groupBoxParams.Size = new System.Drawing.Size(778, 95);
            this.groupBoxParams.TabIndex = 1;
            this.groupBoxParams.TabStop = false;
            this.groupBoxParams.Text = "计算参数";
            
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.gridTM);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(813, 741);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "TM";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(635, 32);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(78, 40);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            // 
            // gridTE
            // 
            this.gridTE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTE.Location = new System.Drawing.Point(3, 3);
            this.gridTE.Name = "gridTE";
            this.gridTE.RowHeadersWidth = 62;
            this.gridTE.RowTemplate.Height = 30;
            this.gridTE.Size = new System.Drawing.Size(807, 735);
            this.gridTE.TabIndex = 0;
            // 
            // gridTM
            // 
            this.gridTM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTM.Location = new System.Drawing.Point(3, 3);
            this.gridTM.Name = "gridTM";
            this.gridTM.RowHeadersWidth = 62;
            this.gridTM.RowTemplate.Height = 30;
            this.gridTM.Size = new System.Drawing.Size(807, 735);
            this.gridTM.TabIndex = 0;
            // 
            // rbInversionTE
            // 
            this.rbInversionTE.AutoSize = true;
            this.rbInversionTE.Checked = true;
            this.rbInversionTE.Location = new System.Drawing.Point(151, 27);
            this.rbInversionTE.Name = "rbInversionTE";
            this.rbInversionTE.Size = new System.Drawing.Size(51, 22);
            this.rbInversionTE.TabIndex = 1;
            this.rbInversionTE.TabStop = true;
            this.rbInversionTE.Text = "TE";
            this.rbInversionTE.UseVisualStyleBackColor = true;
            // 
            // rbInversionTM
            // 
            this.rbInversionTM.AutoSize = true;
            this.rbInversionTM.Location = new System.Drawing.Point(151, 56);
            this.rbInversionTM.Name = "rbInversionTM";
            this.rbInversionTM.Size = new System.Drawing.Size(51, 22);
            this.rbInversionTM.TabIndex = 2;
            this.rbInversionTM.Text = "TM";
            this.rbInversionTM.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 18);
            this.label3.TabIndex = 3;
            this.label3.Text = "反演模式：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(284, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 4;
            this.label4.Text = "迭代次数：";
            // 
            // nudIterationCount
            // 
            this.nudIterationCount.Location = new System.Drawing.Point(377, 38);
            this.nudIterationCount.Name = "nudIterationCount";
            this.nudIterationCount.Size = new System.Drawing.Size(198, 28);
            this.nudIterationCount.TabIndex = 5;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPageDisplayTE);
            this.tabControl2.Controls.Add(this.tabPageDisplayTM);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(609, 28);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPageDisplayTM
            // 
            this.tabPageDisplayTM.Location = new System.Drawing.Point(4, 28);
            this.tabPageDisplayTM.Name = "tabPageDisplayTM";
            this.tabPageDisplayTM.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDisplayTM.Size = new System.Drawing.Size(601, 0);
            this.tabPageDisplayTM.TabIndex = 1;
            this.tabPageDisplayTM.Text = "TM";
            this.tabPageDisplayTM.UseVisualStyleBackColor = true;
            // 
            // tabPageDisplayTE
            // 
            this.tabPageDisplayTE.Location = new System.Drawing.Point(4, 28);
            this.tabPageDisplayTE.Name = "tabPageDisplayTE";
            this.tabPageDisplayTE.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDisplayTE.Size = new System.Drawing.Size(601, 34);
            this.tabPageDisplayTE.TabIndex = 0;
            this.tabPageDisplayTE.Text = "TE";
            this.tabPageDisplayTE.UseVisualStyleBackColor = true;
            // 
            // chartResistivity
            // 
            chartArea3.Name = "ChartArea1";
            this.chartResistivity.ChartAreas.Add(chartArea3);
            this.chartResistivity.Dock = System.Windows.Forms.DockStyle.Top;
            legend3.Name = "Legend1";
            this.chartResistivity.Legends.Add(legend3);
            this.chartResistivity.Location = new System.Drawing.Point(0, 28);
            this.chartResistivity.Name = "chartResistivity";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chartResistivity.Series.Add(series3);
            this.chartResistivity.Size = new System.Drawing.Size(609, 370);
            this.chartResistivity.TabIndex = 1;
            this.chartResistivity.Text = "chart2";
            // 
            // chartPhase
            // 
            chartArea2.Name = "ChartArea1";
            this.chartPhase.ChartAreas.Add(chartArea2);
            this.chartPhase.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.chartPhase.Legends.Add(legend2);
            this.chartPhase.Location = new System.Drawing.Point(0, 398);
            this.chartPhase.Name = "chartPhase";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartPhase.Series.Add(series2);
            this.chartPhase.Size = new System.Drawing.Size(609, 426);
            this.chartPhase.TabIndex = 2;
            this.chartPhase.Text = "chart3";
            // 
            // Form_MT1di
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1434, 824);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form_MT1di";
            this.Text = "Form_MT1di";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).EndInit();
            this.groupBoxParams.ResumeLayout(false);
            this.groupBoxParams.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIterationCount)).EndInit();
            this.tabControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartResistivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhase)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBoxParams;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProfileView;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbLineName;
        private System.Windows.Forms.ComboBox cmbStationLayer;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.NumericUpDown nudIterationCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rbInversionTM;
        private System.Windows.Forms.RadioButton rbInversionTE;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.DataGridView gridTE;
        private System.Windows.Forms.DataGridView gridTM;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPageDisplayTM;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPhase;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResistivity;
        private System.Windows.Forms.TabPage tabPageDisplayTE;
    }
}