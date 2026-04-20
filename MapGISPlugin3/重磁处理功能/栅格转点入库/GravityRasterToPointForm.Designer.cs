namespace MapGISPlugin3
{
    partial class GravityRasterToPointForm
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
            this.lblTiffPath = new System.Windows.Forms.Label();
            this.txtTiffPath = new System.Windows.Forms.TextBox();
            this.btnBrowseTiff = new System.Windows.Forms.Button();
            this.lblDatabasePath = new System.Windows.Forms.Label();
            this.txtDatabasePath = new System.Windows.Forms.TextBox();
            this.btnBrowseDatabase = new System.Windows.Forms.Button();
            this.lblClassName = new System.Windows.Forms.Label();
            this.txtClassName = new System.Windows.Forms.TextBox();
            this.grpInfo = new System.Windows.Forms.GroupBox();
            this.lblSrefValue = new System.Windows.Forms.Label();
            this.lblSrefTitle = new System.Windows.Forms.Label();
            this.lblNoDataValue = new System.Windows.Forms.Label();
            this.lblNoDataTitle = new System.Windows.Forms.Label();
            this.lblRangeValue = new System.Windows.Forms.Label();
            this.lblRangeTitle = new System.Windows.Forms.Label();
            this.lblCellSizeValue = new System.Windows.Forms.Label();
            this.lblCellSizeTitle = new System.Windows.Forms.Label();
            this.lblBandValue = new System.Windows.Forms.Label();
            this.lblBandTitle = new System.Windows.Forms.Label();
            this.lblSizeValue = new System.Windows.Forms.Label();
            this.lblSizeTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTiffPath
            // 
            this.lblTiffPath.AutoSize = true;
            this.lblTiffPath.Location = new System.Drawing.Point(18, 20);
            this.lblTiffPath.Name = "lblTiffPath";
            this.lblTiffPath.Size = new System.Drawing.Size(59, 12);
            this.lblTiffPath.TabIndex = 0;
            this.lblTiffPath.Text = "TIFF路径";
            // 
            // txtTiffPath
            // 
            this.txtTiffPath.Location = new System.Drawing.Point(93, 16);
            this.txtTiffPath.Name = "txtTiffPath";
            this.txtTiffPath.Size = new System.Drawing.Size(382, 21);
            this.txtTiffPath.TabIndex = 1;
            this.txtTiffPath.TextChanged += new System.EventHandler(this.txtTiffPath_TextChanged);
            // 
            // btnBrowseTiff
            // 
            this.btnBrowseTiff.Location = new System.Drawing.Point(490, 15);
            this.btnBrowseTiff.Name = "btnBrowseTiff";
            this.btnBrowseTiff.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseTiff.TabIndex = 2;
            this.btnBrowseTiff.Text = "浏览";
            this.btnBrowseTiff.UseVisualStyleBackColor = true;
            this.btnBrowseTiff.Click += new System.EventHandler(this.btnBrowseTiff_Click);
            // 
            // lblDatabasePath
            // 
            this.lblDatabasePath.AutoSize = true;
            this.lblDatabasePath.Location = new System.Drawing.Point(18, 55);
            this.lblDatabasePath.Name = "lblDatabasePath";
            this.lblDatabasePath.Size = new System.Drawing.Size(53, 12);
            this.lblDatabasePath.TabIndex = 3;
            this.lblDatabasePath.Text = "数据库";
            // 
            // txtDatabasePath
            // 
            this.txtDatabasePath.Location = new System.Drawing.Point(93, 51);
            this.txtDatabasePath.Name = "txtDatabasePath";
            this.txtDatabasePath.ReadOnly = true;
            this.txtDatabasePath.Size = new System.Drawing.Size(382, 21);
            this.txtDatabasePath.TabIndex = 4;
            // 
            // btnBrowseDatabase
            // 
            this.btnBrowseDatabase.Location = new System.Drawing.Point(490, 50);
            this.btnBrowseDatabase.Name = "btnBrowseDatabase";
            this.btnBrowseDatabase.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseDatabase.TabIndex = 5;
            this.btnBrowseDatabase.Text = "选择";
            this.btnBrowseDatabase.UseVisualStyleBackColor = true;
            this.btnBrowseDatabase.Click += new System.EventHandler(this.btnBrowseDatabase_Click);
            // 
            // lblClassName
            // 
            this.lblClassName.AutoSize = true;
            this.lblClassName.Location = new System.Drawing.Point(18, 90);
            this.lblClassName.Name = "lblClassName";
            this.lblClassName.Size = new System.Drawing.Size(53, 12);
            this.lblClassName.TabIndex = 6;
            this.lblClassName.Text = "输出名";
            // 
            // txtClassName
            // 
            this.txtClassName.Location = new System.Drawing.Point(93, 86);
            this.txtClassName.Name = "txtClassName";
            this.txtClassName.Size = new System.Drawing.Size(382, 21);
            this.txtClassName.TabIndex = 7;
            // 
            // grpInfo
            // 
            this.grpInfo.Controls.Add(this.lblSrefValue);
            this.grpInfo.Controls.Add(this.lblSrefTitle);
            this.grpInfo.Controls.Add(this.lblNoDataValue);
            this.grpInfo.Controls.Add(this.lblNoDataTitle);
            this.grpInfo.Controls.Add(this.lblRangeValue);
            this.grpInfo.Controls.Add(this.lblRangeTitle);
            this.grpInfo.Controls.Add(this.lblCellSizeValue);
            this.grpInfo.Controls.Add(this.lblCellSizeTitle);
            this.grpInfo.Controls.Add(this.lblBandValue);
            this.grpInfo.Controls.Add(this.lblBandTitle);
            this.grpInfo.Controls.Add(this.lblSizeValue);
            this.grpInfo.Controls.Add(this.lblSizeTitle);
            this.grpInfo.Location = new System.Drawing.Point(20, 122);
            this.grpInfo.Name = "grpInfo";
            this.grpInfo.Size = new System.Drawing.Size(545, 136);
            this.grpInfo.TabIndex = 8;
            this.grpInfo.TabStop = false;
            this.grpInfo.Text = "TIFF信息";
            // 
            // lblSrefValue
            // 
            this.lblSrefValue.AutoSize = true;
            this.lblSrefValue.Location = new System.Drawing.Point(88, 110);
            this.lblSrefValue.Name = "lblSrefValue";
            this.lblSrefValue.Size = new System.Drawing.Size(11, 12);
            this.lblSrefValue.TabIndex = 11;
            this.lblSrefValue.Text = "-";
            // 
            // lblSrefTitle
            // 
            this.lblSrefTitle.AutoSize = true;
            this.lblSrefTitle.Location = new System.Drawing.Point(15, 110);
            this.lblSrefTitle.Name = "lblSrefTitle";
            this.lblSrefTitle.Size = new System.Drawing.Size(53, 12);
            this.lblSrefTitle.TabIndex = 10;
            this.lblSrefTitle.Text = "坐标系：";
            // 
            // lblNoDataValue
            // 
            this.lblNoDataValue.AutoSize = true;
            this.lblNoDataValue.Location = new System.Drawing.Point(331, 76);
            this.lblNoDataValue.Name = "lblNoDataValue";
            this.lblNoDataValue.Size = new System.Drawing.Size(11, 12);
            this.lblNoDataValue.TabIndex = 9;
            this.lblNoDataValue.Text = "-";
            // 
            // lblNoDataTitle
            // 
            this.lblNoDataTitle.AutoSize = true;
            this.lblNoDataTitle.Location = new System.Drawing.Point(258, 76);
            this.lblNoDataTitle.Name = "lblNoDataTitle";
            this.lblNoDataTitle.Size = new System.Drawing.Size(59, 12);
            this.lblNoDataTitle.TabIndex = 8;
            this.lblNoDataTitle.Text = "NoData：";
            // 
            // lblRangeValue
            // 
            this.lblRangeValue.AutoSize = true;
            this.lblRangeValue.Location = new System.Drawing.Point(88, 76);
            this.lblRangeValue.Name = "lblRangeValue";
            this.lblRangeValue.Size = new System.Drawing.Size(11, 12);
            this.lblRangeValue.TabIndex = 7;
            this.lblRangeValue.Text = "-";
            // 
            // lblRangeTitle
            // 
            this.lblRangeTitle.AutoSize = true;
            this.lblRangeTitle.Location = new System.Drawing.Point(15, 76);
            this.lblRangeTitle.Name = "lblRangeTitle";
            this.lblRangeTitle.Size = new System.Drawing.Size(53, 12);
            this.lblRangeTitle.TabIndex = 6;
            this.lblRangeTitle.Text = "范围值：";
            // 
            // lblCellSizeValue
            // 
            this.lblCellSizeValue.AutoSize = true;
            this.lblCellSizeValue.Location = new System.Drawing.Point(88, 76);
            this.lblCellSizeValue.Name = "lblCellSizeValue";
            this.lblCellSizeValue.Size = new System.Drawing.Size(11, 12);
            this.lblCellSizeValue.TabIndex = 5;
            this.lblCellSizeValue.Text = "-";
            // 
            // lblCellSizeTitle
            // 
            this.lblCellSizeTitle.AutoSize = true;
            this.lblCellSizeTitle.Location = new System.Drawing.Point(15, 76);
            this.lblCellSizeTitle.Name = "lblCellSizeTitle";
            this.lblCellSizeTitle.Size = new System.Drawing.Size(53, 12);
            this.lblCellSizeTitle.TabIndex = 4;
            this.lblCellSizeTitle.Text = "像元值：";
            // 
            // lblBandValue
            // 
            this.lblBandValue.AutoSize = true;
            this.lblBandValue.Location = new System.Drawing.Point(331, 39);
            this.lblBandValue.Name = "lblBandValue";
            this.lblBandValue.Size = new System.Drawing.Size(11, 12);
            this.lblBandValue.TabIndex = 3;
            this.lblBandValue.Text = "-";
            // 
            // lblBandTitle
            // 
            this.lblBandTitle.AutoSize = true;
            this.lblBandTitle.Location = new System.Drawing.Point(258, 39);
            this.lblBandTitle.Name = "lblBandTitle";
            this.lblBandTitle.Size = new System.Drawing.Size(53, 12);
            this.lblBandTitle.TabIndex = 2;
            this.lblBandTitle.Text = "波段数：";
            // 
            // lblSizeValue
            // 
            this.lblSizeValue.AutoSize = true;
            this.lblSizeValue.Location = new System.Drawing.Point(88, 39);
            this.lblSizeValue.Name = "lblSizeValue";
            this.lblSizeValue.Size = new System.Drawing.Size(11, 12);
            this.lblSizeValue.TabIndex = 1;
            this.lblSizeValue.Text = "-";
            // 
            // lblSizeTitle
            // 
            this.lblSizeTitle.AutoSize = true;
            this.lblSizeTitle.Location = new System.Drawing.Point(15, 39);
            this.lblSizeTitle.Name = "lblSizeTitle";
            this.lblSizeTitle.Size = new System.Drawing.Size(53, 12);
            this.lblSizeTitle.TabIndex = 0;
            this.lblSizeTitle.Text = "行列数：";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.Location = new System.Drawing.Point(20, 272);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(545, 36);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "请选择 TIFF 文件和目标数据库。";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(409, 320);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 27);
            this.btnExecute.TabIndex = 10;
            this.btnExecute.Text = "执行";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(490, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 27);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // GravityRasterToPointForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 361);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grpInfo);
            this.Controls.Add(this.txtClassName);
            this.Controls.Add(this.lblClassName);
            this.Controls.Add(this.btnBrowseDatabase);
            this.Controls.Add(this.txtDatabasePath);
            this.Controls.Add(this.lblDatabasePath);
            this.Controls.Add(this.btnBrowseTiff);
            this.Controls.Add(this.txtTiffPath);
            this.Controls.Add(this.lblTiffPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GravityRasterToPointForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "重力 TIFF 栅格转点入库";
            this.grpInfo.ResumeLayout(false);
            this.grpInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTiffPath;
        private System.Windows.Forms.TextBox txtTiffPath;
        private System.Windows.Forms.Button btnBrowseTiff;
        private System.Windows.Forms.Label lblDatabasePath;
        private System.Windows.Forms.TextBox txtDatabasePath;
        private System.Windows.Forms.Button btnBrowseDatabase;
        private System.Windows.Forms.Label lblClassName;
        private System.Windows.Forms.TextBox txtClassName;
        private System.Windows.Forms.GroupBox grpInfo;
        private System.Windows.Forms.Label lblSizeTitle;
        private System.Windows.Forms.Label lblSizeValue;
        private System.Windows.Forms.Label lblBandTitle;
        private System.Windows.Forms.Label lblBandValue;
        private System.Windows.Forms.Label lblCellSizeTitle;
        private System.Windows.Forms.Label lblCellSizeValue;
        private System.Windows.Forms.Label lblRangeTitle;
        private System.Windows.Forms.Label lblRangeValue;
        private System.Windows.Forms.Label lblNoDataTitle;
        private System.Windows.Forms.Label lblNoDataValue;
        private System.Windows.Forms.Label lblSrefTitle;
        private System.Windows.Forms.Label lblSrefValue;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnCancel;
    }
}
