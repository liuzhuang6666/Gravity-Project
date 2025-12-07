namespace MapGISPlugin3.查看属性
{
    partial class Preview_CSAMT
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

        #region 组件设计器生成的代码

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvData = new System.Windows.Forms.TreeView(); // 树
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chartCSAMT = new System.Windows.Forms.DataVisualization.Charting.Chart(); // 改名了
            this.gridCSAMT = new System.Windows.Forms.DataGridView(); // 改名了

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartCSAMT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCSAMT)).BeginInit();
            this.SuspendLayout();

            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // Panel1 放树
            this.splitContainer1.Panel1.Controls.Add(this.tvData);
            // Panel2 放右侧图表区
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(929, 681);
            this.splitContainer1.SplitterDistance = 309;
            this.splitContainer1.TabIndex = 0;

            // 
            // tvData
            // 
            this.tvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvData.HideSelection = false;
            this.tvData.Location = new System.Drawing.Point(0, 0);
            this.tvData.Name = "tvData";
            this.tvData.Size = new System.Drawing.Size(309, 681);
            this.tvData.TabIndex = 0;

            // 
            // splitContainer2 (右侧上下分割)
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // Panel1 放图
            this.splitContainer2.Panel1.Controls.Add(this.chartCSAMT);
            // Panel2 放表
            this.splitContainer2.Panel2.Controls.Add(this.gridCSAMT);
            this.splitContainer2.Size = new System.Drawing.Size(616, 681);
            this.splitContainer2.SplitterDistance = 300; // 图表高度给大点
            this.splitContainer2.TabIndex = 0;

            // 
            // chartCSAMT
            // 
            chartArea1.Name = "ChartArea1";
            this.chartCSAMT.ChartAreas.Add(chartArea1);
            this.chartCSAMT.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chartCSAMT.Legends.Add(legend1);
            this.chartCSAMT.Location = new System.Drawing.Point(0, 0);
            this.chartCSAMT.Name = "chartCSAMT";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartCSAMT.Series.Add(series1);
            this.chartCSAMT.Size = new System.Drawing.Size(616, 300);
            this.chartCSAMT.TabIndex = 0;
            this.chartCSAMT.Text = "CSAMT Preview";

            // 
            // gridCSAMT
            // 
            this.gridCSAMT.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridCSAMT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCSAMT.Location = new System.Drawing.Point(0, 0);
            this.gridCSAMT.Name = "gridCSAMT";
            this.gridCSAMT.RowHeadersWidth = 50;
            this.gridCSAMT.RowTemplate.Height = 30;
            this.gridCSAMT.Size = new System.Drawing.Size(616, 377);
            this.gridCSAMT.TabIndex = 0;

            // 
            // Preview_CSAMT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Preview_CSAMT";
            this.Size = new System.Drawing.Size(929, 681);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartCSAMT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCSAMT)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCSAMT;
        private System.Windows.Forms.DataGridView gridCSAMT;
        private System.Windows.Forms.TreeView tvData;
    }
}