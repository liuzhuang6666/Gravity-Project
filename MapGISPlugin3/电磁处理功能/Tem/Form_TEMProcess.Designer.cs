using System.Windows.Forms.DataVisualization.Charting;

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
            this.timerProgress = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbLineName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbStationLayer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.progressBarCalculate = new System.Windows.Forms.ProgressBar();
            this.chartVoltage = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.gridData = new System.Windows.Forms.DataGridView();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.chartProfileView = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).BeginInit();
            this.SuspendLayout();
            // 
            // timerProgress
            // 
            this.timerProgress.Interval = 500;
            this.timerProgress.Tick += new System.EventHandler(this.timerProgress_Tick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1470, 48);
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
            this.button2.Location = new System.Drawing.Point(1426, 0);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(44, 48);
            this.button2.TabIndex = 0;
            this.button2.Text = "X";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbLineName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmbStationLayer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(18, 260);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(590, 104);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据选择";
            // 
            // cmbLineName
            // 
            this.cmbLineName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineName.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cmbLineName.Location = new System.Drawing.Point(315, 51);
            this.cmbLineName.Name = "cmbLineName";
            this.cmbLineName.Size = new System.Drawing.Size(246, 26);
            this.cmbLineName.TabIndex = 3;
            this.cmbLineName.SelectedIndexChanged += new System.EventHandler(this.cmbLineName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(310, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "测线：";
            // 
            // cmbStationLayer
            // 
            this.cmbStationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStationLayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cmbStationLayer.Location = new System.Drawing.Point(38, 51);
            this.cmbStationLayer.Name = "cmbStationLayer";
            this.cmbStationLayer.Size = new System.Drawing.Size(246, 26);
            this.cmbStationLayer.TabIndex = 1;
            this.cmbStationLayer.SelectedIndexChanged += new System.EventHandler(this.cmbStationLayer_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "测点图层：";
            // 
            // btnCalculate
            // 
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(18, 387);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(150, 45);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "开始计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // progressBarCalculate
            // 
            this.progressBarCalculate.Location = new System.Drawing.Point(183, 396);
            this.progressBarCalculate.Margin = new System.Windows.Forms.Padding(4);
            this.progressBarCalculate.Name = "progressBarCalculate";
            this.progressBarCalculate.Size = new System.Drawing.Size(424, 27);
            this.progressBarCalculate.TabIndex = 26;
            // 
            // chartVoltage
            // 
            chartArea1.Name = "ChartArea1";
            this.chartVoltage.ChartAreas.Add(chartArea1);
            legend1.BackColor = System.Drawing.SystemColors.Control;
            legend1.Name = "LegendVoltage";
            this.chartVoltage.Legends.Add(legend1);
            this.chartVoltage.Location = new System.Drawing.Point(692, 52);
            this.chartVoltage.Margin = new System.Windows.Forms.Padding(4);
            this.chartVoltage.Name = "chartVoltage";
            this.chartVoltage.Size = new System.Drawing.Size(751, 669);
            this.chartVoltage.TabIndex = 24;
            this.chartVoltage.Text = "chartVoltage";
            // 
            // gridData
            // 
            this.gridData.AllowUserToAddRows = false;
            this.gridData.AllowUserToDeleteRows = false;
            this.gridData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridData.Location = new System.Drawing.Point(18, 52);
            this.gridData.Margin = new System.Windows.Forms.Padding(4);
            this.gridData.Name = "gridData";
            this.gridData.ReadOnly = true;
            this.gridData.RowHeadersWidth = 62;
            this.gridData.RowTemplate.Height = 23;
            this.gridData.Size = new System.Drawing.Size(590, 198);
            this.gridData.TabIndex = 25;
            // 
            // chartProfileView
            // 
            chartArea2.Name = "ChartArea1";
            this.chartProfileView.ChartAreas.Add(chartArea2);
            this.chartProfileView.Location = new System.Drawing.Point(18, 448);
            this.chartProfileView.Margin = new System.Windows.Forms.Padding(4);
            this.chartProfileView.Name = "chartProfileView";
            this.chartProfileView.Size = new System.Drawing.Size(590, 273);
            this.chartProfileView.TabIndex = 23;
            this.chartProfileView.Text = "chartProfileView";
            this.chartProfileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartProfileView_MouseClick);
            // 
            // Form_TEMProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1470, 750);
            this.Controls.Add(this.progressBarCalculate);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.gridData);
            this.Controls.Add(this.chartVoltage);
            this.Controls.Add(this.chartProfileView);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(1470, 750);
            this.MinimumSize = new System.Drawing.Size(1470, 750);
            this.Name = "Form_TEMProcess";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.Load += new System.EventHandler(this.Form_TEMProcess_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartVoltage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProfileView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button2;
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
        // 新增：定时器控件声明
        private System.Windows.Forms.Timer timerProgress;
    }
}