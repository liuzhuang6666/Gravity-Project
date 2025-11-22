namespace MapGISPlugin3
{
    partial class Form_Gravity3DInversion
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBoxData = new System.Windows.Forms.GroupBox();
            this.treeViewLayers = new System.Windows.Forms.TreeView();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.btnBrowseTerrain = new System.Windows.Forms.Button();
            this.labelTerrain = new System.Windows.Forms.Label();
            this.textBoxTerrainPath = new System.Windows.Forms.TextBox();
            this.labelGDB = new System.Windows.Forms.Label();
            this.textBoxGdbDirectory = new System.Windows.Forms.TextBox();
            this.labelSave = new System.Windows.Forms.Label();
            this.textBoxSavePath = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBoxData.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1 (Title Bar)
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 30);
            this.panel1.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(10, 8);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(89, 12);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "重力三维反演";
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(770, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBoxData (Left Side)
            // 
            this.groupBoxData.Controls.Add(this.treeViewLayers);
            this.groupBoxData.Location = new System.Drawing.Point(12, 40);
            this.groupBoxData.Name = "groupBoxData";
            this.groupBoxData.Size = new System.Drawing.Size(250, 300);
            this.groupBoxData.TabIndex = 1;
            this.groupBoxData.TabStop = false;
            this.groupBoxData.Text = "选择重力异常图层";
            // 
            // treeViewLayers
            // 
            this.treeViewLayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLayers.Location = new System.Drawing.Point(3, 17);
            this.treeViewLayers.Name = "treeViewLayers";
            this.treeViewLayers.Size = new System.Drawing.Size(244, 280);
            this.treeViewLayers.TabIndex = 0;
            this.treeViewLayers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLayers_AfterSelect);
            // 
            // groupBoxSettings (Right Side)
            // 
            this.groupBoxSettings.Controls.Add(this.btnBrowseTerrain);
            this.groupBoxSettings.Controls.Add(this.labelTerrain);
            this.groupBoxSettings.Controls.Add(this.textBoxTerrainPath);
            this.groupBoxSettings.Controls.Add(this.labelGDB);
            this.groupBoxSettings.Controls.Add(this.textBoxGdbDirectory);
            this.groupBoxSettings.Controls.Add(this.labelSave);
            this.groupBoxSettings.Controls.Add(this.textBoxSavePath);
            this.groupBoxSettings.Location = new System.Drawing.Point(280, 40);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(500, 240);
            this.groupBoxSettings.TabIndex = 2;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "参数设置";
            // 
            // labelTerrain
            // 
            this.labelTerrain.AutoSize = true;
            this.labelTerrain.Location = new System.Drawing.Point(20, 40);
            this.labelTerrain.Name = "labelTerrain";
            this.labelTerrain.Size = new System.Drawing.Size(59, 12);
            this.labelTerrain.TabIndex = 0;
            this.labelTerrain.Text = "地形文件:";
            // 
            // textBoxTerrainPath
            // 
            this.textBoxTerrainPath.Location = new System.Drawing.Point(100, 37);
            this.textBoxTerrainPath.Name = "textBoxTerrainPath";
            this.textBoxTerrainPath.Size = new System.Drawing.Size(300, 21);
            this.textBoxTerrainPath.TabIndex = 1;
            // 
            // btnBrowseTerrain
            // 
            this.btnBrowseTerrain.Location = new System.Drawing.Point(410, 35);
            this.btnBrowseTerrain.Name = "btnBrowseTerrain";
            this.btnBrowseTerrain.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseTerrain.TabIndex = 2;
            this.btnBrowseTerrain.Text = "浏览...";
            this.btnBrowseTerrain.UseVisualStyleBackColor = true;
            this.btnBrowseTerrain.Click += new System.EventHandler(this.btnBrowseTerrain_Click);
            // 
            // labelGDB
            // 
            this.labelGDB.AutoSize = true;
            this.labelGDB.Location = new System.Drawing.Point(20, 90);
            this.labelGDB.Name = "labelGDB";
            this.labelGDB.Size = new System.Drawing.Size(53, 12);
            this.labelGDB.TabIndex = 3;
            this.labelGDB.Text = "GDB目录:";
            // 
            // textBoxGdbDirectory
            // 
            this.textBoxGdbDirectory.Location = new System.Drawing.Point(100, 87);
            this.textBoxGdbDirectory.Name = "textBoxGdbDirectory";
            this.textBoxGdbDirectory.Size = new System.Drawing.Size(300, 21);
            this.textBoxGdbDirectory.TabIndex = 4;
            // 
            // labelSave
            // 
            this.labelSave.AutoSize = true;
            this.labelSave.Location = new System.Drawing.Point(20, 140);
            this.labelSave.Name = "labelSave";
            this.labelSave.Size = new System.Drawing.Size(59, 12);
            this.labelSave.TabIndex = 5;
            this.labelSave.Text = "输出类名:";
            // 
            // textBoxSavePath
            // 
            this.textBoxSavePath.Location = new System.Drawing.Point(100, 137);
            this.textBoxSavePath.Name = "textBoxSavePath";
            this.textBoxSavePath.Size = new System.Drawing.Size(300, 21);
            this.textBoxSavePath.TabIndex = 6;
            this.textBoxSavePath.ReadOnly = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(380, 310);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(500, 310);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // Form_Gravity3DInversion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 360);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.groupBoxData);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "重力三维反演";
            this.Load += new System.EventHandler(this.Form_Gravity3DInversion_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBoxData.ResumeLayout(false);
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox groupBoxData;
        private System.Windows.Forms.TreeView treeViewLayers;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.Button btnBrowseTerrain;
        private System.Windows.Forms.Label labelTerrain;
        private System.Windows.Forms.TextBox textBoxTerrainPath;
        private System.Windows.Forms.Label labelGDB;
        private System.Windows.Forms.TextBox textBoxGdbDirectory;
        private System.Windows.Forms.Label labelSave;
        private System.Windows.Forms.TextBox textBoxSavePath;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}