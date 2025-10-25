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
            this.textBoxGdbDirectory = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxInclination
            // 
            this.textBoxInclination.Location = new System.Drawing.Point(402, 214);
            this.textBoxInclination.Name = "textBoxInclination";
            this.textBoxInclination.Size = new System.Drawing.Size(64, 28);
            this.textBoxInclination.TabIndex = 1;
            this.textBoxInclination.TextChanged += new System.EventHandler(this.textBoxInclination_TextChanged);
            // 
            // textBoxDeclination
            // 
            this.textBoxDeclination.Location = new System.Drawing.Point(614, 214);
            this.textBoxDeclination.Name = "textBoxDeclination";
            this.textBoxDeclination.Size = new System.Drawing.Size(59, 28);
            this.textBoxDeclination.TabIndex = 2;
            this.textBoxDeclination.TextChanged += new System.EventHandler(this.textBoxDeclination_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(402, 338);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(98, 34);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "确认";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(614, 338);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(98, 34);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // treeViewLayers
            // 
            this.treeViewLayers.Location = new System.Drawing.Point(68, 22);
            this.treeViewLayers.Name = "treeViewLayers";
            this.treeViewLayers.Size = new System.Drawing.Size(216, 349);
            this.treeViewLayers.TabIndex = 5;
            this.treeViewLayers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLayers_AfterSelect);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(334, 219);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 18);
            this.label1.TabIndex = 6;
            this.label1.Text = "磁倾角";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(546, 219);
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
            this.textBoxSavePath.Location = new System.Drawing.Point(478, 272);
            this.textBoxSavePath.Name = "textBoxSavePath";
            this.textBoxSavePath.ReadOnly = true;
            this.textBoxSavePath.Size = new System.Drawing.Size(234, 28);
            this.textBoxSavePath.TabIndex = 8;
            this.textBoxSavePath.TextChanged += new System.EventHandler(this.textBoxGdbDirectory_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(334, 276);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "结果输出命名";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // textBoxGdbDirectory
            // 
            this.textBoxGdbDirectory.Location = new System.Drawing.Point(468, 165);
            this.textBoxGdbDirectory.Name = "textBoxGdbDirectory";
            this.textBoxGdbDirectory.Size = new System.Drawing.Size(242, 28);
            this.textBoxGdbDirectory.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(334, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 18);
            this.label4.TabIndex = 12;
            this.label4.Text = "GDB文件夹";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(472, 214);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 18);
            this.label5.TabIndex = 13;
            this.label5.Text = "°";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(679, 214);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(26, 18);
            this.label6.TabIndex = 14;
            this.label6.Text = "°";
            // 
            // Form_MagCorrelationImaging
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxGdbDirectory);
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
        private System.Windows.Forms.TextBox textBoxGdbDirectory;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}