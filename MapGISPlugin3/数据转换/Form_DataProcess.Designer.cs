namespace MapGISPlugin3
{
    partial class Form_Converter
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnRasToVec = new System.Windows.Forms.Button();
            this.btnRasToGrid = new System.Windows.Forms.Button();
            this.btnVecToRas = new System.Windows.Forms.Button();
            this.btnRasToPoint = new System.Windows.Forms.Button(); // 新增按钮声明
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();

            // panel1 (顶部标题栏)
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Height = 30;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);

            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(12, 9);
            this.labelTitle.Text = "MapGIS 数据转换工具 (RasTrans)";

            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.Text = "X";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // groupBox3 (功能区)
            this.groupBox3.Controls.Add(this.btnRasToVec);
            this.groupBox3.Controls.Add(this.btnRasToGrid);
            this.groupBox3.Controls.Add(this.btnVecToRas);
            this.groupBox3.Controls.Add(this.btnRasToPoint); // 添加新按钮到容器
            this.groupBox3.Location = new System.Drawing.Point(20, 50);
            this.groupBox3.Size = new System.Drawing.Size(935, 600);
            this.groupBox3.Text = "转换操作";

            // 按钮公共样式
            System.Drawing.Size commonSize = new System.Drawing.Size(220, 50);

            this.btnRasToVec.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRasToVec.Location = new System.Drawing.Point(40, 60);
            this.btnRasToVec.Size = commonSize;
            this.btnRasToVec.Text = "栅格转矢量 (Vector)";
            this.btnRasToVec.Click += new System.EventHandler(this.btnRasToVec_Click);

            this.btnRasToGrid.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRasToGrid.Location = new System.Drawing.Point(40, 130);
            this.btnRasToGrid.Size = commonSize;
            this.btnRasToGrid.Text = "栅格转网格 (Grid)";
            this.btnRasToGrid.Click += new System.EventHandler(this.btnRasToGrid_Click);

            this.btnVecToRas.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnVecToRas.Location = new System.Drawing.Point(40, 200);
            this.btnVecToRas.Size = commonSize;
            this.btnVecToRas.Text = "矢量转栅格 (Raster)";
            this.btnVecToRas.Click += new System.EventHandler(this.btnVecToRas_Click);

            // 新的 栅格转点 按钮
            this.btnRasToPoint.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRasToPoint.Location = new System.Drawing.Point(40, 270);
            this.btnRasToPoint.Size = commonSize;
            this.btnRasToPoint.Text = "栅格转点 (Point)";
            this.btnRasToPoint.Click += new System.EventHandler(this.btnRasToPoint_Click);

            // Form_Converter
            this.ClientSize = new System.Drawing.Size(975, 675);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form_Converter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnRasToVec;
        private System.Windows.Forms.Button btnRasToGrid;
        private System.Windows.Forms.Button btnVecToRas;
        private System.Windows.Forms.Button btnRasToPoint; // 声明新按钮组件
    }
}