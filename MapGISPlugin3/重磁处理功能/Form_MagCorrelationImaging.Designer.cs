namespace MapGISPlugin3
{
    // **************************************************
    // 4. Designer 文件中的 partial class 名已修改
    // **************************************************
    partial class Form_MagCorrelationImaging
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
            this.textBoxInclination = new System.Windows.Forms.TextBox();
            this.textBoxDeclination = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.treeViewLayers = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSavePath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.textBoxGdbDirectory = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxInclination
            // 
            this.textBoxInclination.Location = new System.Drawing.Point(402, 215);
            this.textBoxInclination.Name = "textBoxInclination";
            this.textBoxInclination.Size = new System.Drawing.Size(97, 28);
            this.textBoxInclination.TabIndex = 1;
            // 
            // textBoxDeclination
            // 
            this.textBoxDeclination.Location = new System.Drawing.Point(614, 215);
            this.textBoxDeclination.Name = "textBoxDeclination";
            this.textBoxDeclination.Size = new System.Drawing.Size(97, 28);
            this.textBoxDeclination.TabIndex = 2;
            this.textBoxDeclination.TextChanged += new System.EventHandler(this.textBoxDeclination_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(402, 338);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(97, 34);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "确认";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(614, 338);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(97, 34);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // treeViewLayers
            // 
            this.treeViewLayers.Location = new System.Drawing.Point(67, 23);
            this.treeViewLayers.Name = "treeViewLayers";
            this.treeViewLayers.Size = new System.Drawing.Size(216, 349);
            this.treeViewLayers.TabIndex = 5;
            this.treeViewLayers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLayers_AfterSelect);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(334, 218);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 18);
            this.label1.TabIndex = 6;
            this.label1.Text = "磁倾角";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(546, 218);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "磁偏角";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // textBoxSavePath
            // 
            this.textBoxSavePath.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSavePath.ForeColor = System.Drawing.SystemColors.WindowText;
            this.textBoxSavePath.Location = new System.Drawing.Point(478, 271);
            this.textBoxSavePath.Name = "textBoxSavePath";
            this.textBoxSavePath.ReadOnly = true;
            this.textBoxSavePath.Size = new System.Drawing.Size(233, 28);
            this.textBoxSavePath.TabIndex = 8;
            this.textBoxSavePath.TextChanged += new System.EventHandler(this.textBoxGdbDirectory_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(334, 280);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "结果保存路径";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(680, 271);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(31, 28);
            this.btnBrowse.TabIndex = 10;
            this.btnBrowse.Text = "…";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // textBoxGdbDirectory
            // 
            this.textBoxGdbDirectory.Location = new System.Drawing.Point(468, 165);
            this.textBoxGdbDirectory.Name = "textBoxGdbDirectory";
            this.textBoxGdbDirectory.Size = new System.Drawing.Size(243, 28);
            this.textBoxGdbDirectory.TabIndex = 11;
            // 
            // Form_MagCorrelationImaging
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBoxGdbDirectory);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxSavePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeViewLayers);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.textBoxDeclination);
            this.Controls.Add(this.textBoxInclination);
            this.Name = "Form_MagCorrelationImaging";
            this.Text = "磁相关成像";
            this.Load += new System.EventHandler(this.Form_MagCorrelationImaging_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxInclination;
        private System.Windows.Forms.TextBox textBoxDeclination;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TreeView treeViewLayers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSavePath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox textBoxGdbDirectory;
    }
}