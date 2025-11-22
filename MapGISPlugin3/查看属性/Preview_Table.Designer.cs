
namespace MapGISPlugin3.查看属性
{
    partial class Preview_Table
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.gridGeneric = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.gridGeneric)).BeginInit();
            this.SuspendLayout();
            // 
            // gridGeneric
            // 
            this.gridGeneric.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridGeneric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridGeneric.Location = new System.Drawing.Point(0, 0);
            this.gridGeneric.Name = "gridGeneric";
            this.gridGeneric.RowHeadersWidth = 62;
            this.gridGeneric.RowTemplate.Height = 30;
            this.gridGeneric.Size = new System.Drawing.Size(694, 654);
            this.gridGeneric.TabIndex = 0;
            // 
            // Preview_Table
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridGeneric);
            this.Name = "Preview_Table";
            this.Size = new System.Drawing.Size(694, 654);
            ((System.ComponentModel.ISupportInitialize)(this.gridGeneric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridGeneric;
    }
}
