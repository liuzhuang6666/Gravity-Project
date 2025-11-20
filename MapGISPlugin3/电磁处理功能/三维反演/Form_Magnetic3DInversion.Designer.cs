namespace MapGISPlugin3
{
    partial class Form_Magnetic3DInversion
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
            this.labelInc = new System.Windows.Forms.Label();
            this.textBoxInc = new System.Windows.Forms.TextBox();
            this.labelDec = new System.Windows.Forms.Label();
            this.textBoxDec = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBoxData.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Size = new System.Drawing.Size(800, 30);
            this.panel1.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(10, 8);
            this.labelTitle.Text = "磁力三维反演";
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(770, 0);
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.Click += new System.EventHandler(this.button1_Click);
            this.btnClose.Text = "X";
            // 
            // groupBoxData
            // 
            this.groupBoxData.Controls.Add(this.treeViewLayers);
            this.groupBoxData.Location = new System.Drawing.Point(12, 40);
            this.groupBoxData.Size = new System.Drawing.Size(250, 300);
            this.groupBoxData.Text = "选择磁力异常图层";
            // 
            // treeViewLayers
            // 
            this.treeViewLayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLayers.Location = new System.Drawing.Point(3, 17);
            this.treeViewLayers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLayers_AfterSelect);
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Controls.Add(this.textBoxDec);
            this.groupBoxSettings.Controls.Add(this.labelDec);
            this.groupBoxSettings.Controls.Add(this.textBoxInc);
            this.groupBoxSettings.Controls.Add(this.labelInc);
            this.groupBoxSettings.Controls.Add(this.btnBrowseTerrain);
            this.groupBoxSettings.Controls.Add(this.labelTerrain);
            this.groupBoxSettings.Controls.Add(this.textBoxTerrainPath);
            this.groupBoxSettings.Controls.Add(this.labelGDB);
            this.groupBoxSettings.Controls.Add(this.textBoxGdbDirectory);
            this.groupBoxSettings.Controls.Add(this.labelSave);
            this.groupBoxSettings.Controls.Add(this.textBoxSavePath);
            this.groupBoxSettings.Location = new System.Drawing.Point(280, 40);
            this.groupBoxSettings.Size = new System.Drawing.Size(500, 240);
            this.groupBoxSettings.Text = "参数设置";
            // 
            // labelInc
            // 
            this.labelInc.AutoSize = true;
            this.labelInc.Location = new System.Drawing.Point(20, 30);
            this.labelInc.Text = "磁倾角:";
            // 
            // textBoxInc
            // 
            this.textBoxInc.Location = new System.Drawing.Point(80, 27);
            this.textBoxInc.Size = new System.Drawing.Size(80, 21);
            this.textBoxInc.Text = "50";
            // 
            // labelDec
            // 
            this.labelDec.AutoSize = true;
            this.labelDec.Location = new System.Drawing.Point(180, 30);
            this.labelDec.Text = "磁偏角:";
            // 
            // textBoxDec
            // 
            this.textBoxDec.Location = new System.Drawing.Point(240, 27);
            this.textBoxDec.Size = new System.Drawing.Size(80, 21);
            this.textBoxDec.Text = "20";
            // 
            // labelTerrain
            // 
            this.labelTerrain.AutoSize = true;
            this.labelTerrain.Location = new System.Drawing.Point(20, 70);
            this.labelTerrain.Text = "地形文件:";
            // 
            // textBoxTerrainPath
            // 
            this.textBoxTerrainPath.Location = new System.Drawing.Point(100, 67);
            this.textBoxTerrainPath.Size = new System.Drawing.Size(300, 21);
            // 
            // btnBrowseTerrain
            // 
            this.btnBrowseTerrain.Location = new System.Drawing.Point(410, 65);
            this.btnBrowseTerrain.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseTerrain.Click += new System.EventHandler(this.btnBrowseTerrain_Click);
            this.btnBrowseTerrain.Text = "浏览...";
            // 
            // labelGDB
            // 
            this.labelGDB.AutoSize = true;
            this.labelGDB.Location = new System.Drawing.Point(20, 110);
            this.labelGDB.Text = "GDB目录:";
            // 
            // textBoxGdbDirectory
            // 
            this.textBoxGdbDirectory.Location = new System.Drawing.Point(100, 107);
            this.textBoxGdbDirectory.Size = new System.Drawing.Size(300, 21);
            // 
            // labelSave
            // 
            this.labelSave.AutoSize = true;
            this.labelSave.Location = new System.Drawing.Point(20, 150);
            this.labelSave.Text = "输出类名:";
            // 
            // textBoxSavePath
            // 
            this.textBoxSavePath.Location = new System.Drawing.Point(100, 147);
            this.textBoxSavePath.Size = new System.Drawing.Size(300, 21);
            this.textBoxSavePath.ReadOnly = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(380, 310);
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            this.btnOK.Text = "确定";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(500, 310);
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.Text = "取消";
            // 
            // Form_Magnetic3DInversion
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
            this.Text = "磁力三维反演";
            this.Load += new System.EventHandler(this.Form_Magnetic3DInversion_Load);
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
        private System.Windows.Forms.Label labelInc;
        private System.Windows.Forms.TextBox textBoxInc;
        private System.Windows.Forms.Label labelDec;
        private System.Windows.Forms.TextBox textBoxDec;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}