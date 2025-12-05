namespace MapGISPlugin3.查看属性
{
    partial class Preview_TEM
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvData = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chartTEM = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.gridTEM = new System.Windows.Forms.DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartTEM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTEM)).BeginInit();
            this.SuspendLayout();

            // splitContainer1
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Panel1.Controls.Add(this.tvData);
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(900, 600);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 0;

            // tvData
            this.tvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvData.HideSelection = false;
            this.tvData.Location = new System.Drawing.Point(0, 0);
            this.tvData.Name = "tvData";
            this.tvData.Size = new System.Drawing.Size(250, 600);
            this.tvData.TabIndex = 0;

            // splitContainer2
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer2.Panel1.Controls.Add(this.chartTEM);
            this.splitContainer2.Panel2.Controls.Add(this.gridTEM);
            this.splitContainer2.Size = new System.Drawing.Size(646, 600);
            this.splitContainer2.SplitterDistance = 350; // 图表占大一点
            this.splitContainer2.TabIndex = 0;

            // chartTEM
            chartArea1.Name = "ChartArea1";
            this.chartTEM.ChartAreas.Add(chartArea1);
            this.chartTEM.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chartTEM.Legends.Add(legend1);
            this.chartTEM.Location = new System.Drawing.Point(0, 0);
            this.chartTEM.Name = "chartTEM";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartTEM.Series.Add(series1);
            this.chartTEM.Size = new System.Drawing.Size(646, 350);
            this.chartTEM.TabIndex = 0;
            this.chartTEM.Text = "chartTEM";

            // gridTEM
            this.gridTEM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTEM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTEM.Location = new System.Drawing.Point(0, 0);
            this.gridTEM.Name = "gridTEM";
            this.gridTEM.RowTemplate.Height = 23;
            this.gridTEM.Size = new System.Drawing.Size(646, 246);
            this.gridTEM.TabIndex = 0;

            // Preview_TEM
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Preview_TEM";
            this.Size = new System.Drawing.Size(900, 600);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartTEM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTEM)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvData;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartTEM;
        private System.Windows.Forms.DataGridView gridTEM;
    }
}