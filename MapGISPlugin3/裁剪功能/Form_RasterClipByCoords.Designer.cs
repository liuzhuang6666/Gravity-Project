namespace MapGISPlugin3
{
    partial class Form_RasterClipByCoords
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
            this.lblTip = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblXMin = new System.Windows.Forms.Label();
            this.txtXMin = new System.Windows.Forms.TextBox();
            this.lblYMin = new System.Windows.Forms.Label();
            this.txtYMin = new System.Windows.Forms.TextBox();
            this.lblXMax = new System.Windows.Forms.Label();
            this.txtXMax = new System.Windows.Forms.TextBox();
            this.lblYMax = new System.Windows.Forms.Label();
            this.txtYMax = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.txtOutputPath = new DevExpress.XtraEditors.ButtonEdit();
            this.btnClip = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtOutputPath.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTip
            // 
            this.lblTip.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblTip.Location = new System.Drawing.Point(15, 15);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(350, 40);
            this.lblTip.TabIndex = 0;
            this.lblTip.Text = "请等待读取栅格极值...";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtYMax);
            this.groupBox1.Controls.Add(this.lblYMax);
            this.groupBox1.Controls.Add(this.txtXMax);
            this.groupBox1.Controls.Add(this.lblXMax);
            this.groupBox1.Controls.Add(this.txtYMin);
            this.groupBox1.Controls.Add(this.lblYMin);
            this.groupBox1.Controls.Add(this.txtXMin);
            this.groupBox1.Controls.Add(this.lblXMin);
            this.groupBox1.Location = new System.Drawing.Point(17, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(350, 110);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入裁剪坐标";
            // 
            // lblXMin
            // 
            this.lblXMin.AutoSize = true;
            this.lblXMin.Location = new System.Drawing.Point(15, 30);
            this.lblXMin.Name = "lblXMin";
            this.lblXMin.Size = new System.Drawing.Size(35, 12);
            this.lblXMin.TabIndex = 0;
            this.lblXMin.Text = "X最小:";
            // 
            // txtXMin
            // 
            this.txtXMin.Location = new System.Drawing.Point(55, 27);
            this.txtXMin.Name = "txtXMin";
            this.txtXMin.Size = new System.Drawing.Size(100, 21);
            this.txtXMin.TabIndex = 1;
            // 
            // lblYMin
            // 
            this.lblYMin.AutoSize = true;
            this.lblYMin.Location = new System.Drawing.Point(180, 30);
            this.lblYMin.Name = "lblYMin";
            this.lblYMin.Size = new System.Drawing.Size(35, 12);
            this.lblYMin.TabIndex = 2;
            this.lblYMin.Text = "Y最小:";
            // 
            // txtYMin
            // 
            this.txtYMin.Location = new System.Drawing.Point(220, 27);
            this.txtYMin.Name = "txtYMin";
            this.txtYMin.Size = new System.Drawing.Size(100, 21);
            this.txtYMin.TabIndex = 3;
            // 
            // lblXMax
            // 
            this.lblXMax.AutoSize = true;
            this.lblXMax.Location = new System.Drawing.Point(15, 70);
            this.lblXMax.Name = "lblXMax";
            this.lblXMax.Size = new System.Drawing.Size(35, 12);
            this.lblXMax.TabIndex = 4;
            this.lblXMax.Text = "X最大:";
            // 
            // txtXMax
            // 
            this.txtXMax.Location = new System.Drawing.Point(55, 67);
            this.txtXMax.Name = "txtXMax";
            this.txtXMax.Size = new System.Drawing.Size(100, 21);
            this.txtXMax.TabIndex = 5;
            // 
            // lblYMax
            // 
            this.lblYMax.AutoSize = true;
            this.lblYMax.Location = new System.Drawing.Point(180, 70);
            this.lblYMax.Name = "lblYMax";
            this.lblYMax.Size = new System.Drawing.Size(35, 12);
            this.lblYMax.TabIndex = 6;
            this.lblYMax.Text = "Y最大:";
            // 
            // txtYMax
            // 
            this.txtYMax.Location = new System.Drawing.Point(220, 67);
            this.txtYMax.Name = "txtYMax";
            this.txtYMax.Size = new System.Drawing.Size(100, 21);
            this.txtYMax.TabIndex = 7;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(15, 195);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(59, 12);
            this.lblPath.TabIndex = 2;
            this.lblPath.Text = "输出位置:";
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Location = new System.Drawing.Point(80, 192);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.txtOutputPath.Size = new System.Drawing.Size(287, 20);
            this.txtOutputPath.TabIndex = 3;
            // 
            // btnClip
            // 
            this.btnClip.Location = new System.Drawing.Point(135, 235);
            this.btnClip.Name = "btnClip";
            this.btnClip.Size = new System.Drawing.Size(110, 32);
            this.btnClip.TabIndex = 4;
            this.btnClip.Text = "执行坐标裁剪";
            this.btnClip.Click += new System.EventHandler(this.btnClip_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(257, 235);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 32);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // Form_RasterClipByCoords
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 285);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnClip);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblTip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_RasterClipByCoords";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "指定坐标裁剪";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtOutputPath.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblXMin;
        private System.Windows.Forms.TextBox txtXMin;
        private System.Windows.Forms.Label lblYMin;
        private System.Windows.Forms.TextBox txtYMin;
        private System.Windows.Forms.Label lblXMax;
        private System.Windows.Forms.TextBox txtXMax;
        private System.Windows.Forms.Label lblYMax;
        private System.Windows.Forms.TextBox txtYMax;
        private System.Windows.Forms.Label lblPath;
        private DevExpress.XtraEditors.ButtonEdit txtOutputPath;
        private DevExpress.XtraEditors.SimpleButton btnClip;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}