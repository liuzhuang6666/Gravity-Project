namespace MapGISPlugin3
{
    partial class Form_TEMImport
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtKnowedFile = new System.Windows.Forms.TextBox();
            this.btnSelectKnowedFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGdbPathDisplay = new System.Windows.Forms.TextBox();
            this.btnSelectGdbLocation = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTranFile = new System.Windows.Forms.TextBox();
            this.btnSelectTranFile = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "观测数据文件：";
            // 
            // txtKnowedFile
            // 
            this.txtKnowedFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtKnowedFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtKnowedFile.Location = new System.Drawing.Point(242, 63);
            this.txtKnowedFile.Name = "txtKnowedFile";
            this.txtKnowedFile.Size = new System.Drawing.Size(311, 28);
            this.txtKnowedFile.TabIndex = 1;
            // 
            // btnSelectKnowedFile
            // 
            this.btnSelectKnowedFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectKnowedFile.Location = new System.Drawing.Point(560, 65);
            this.btnSelectKnowedFile.Name = "btnSelectKnowedFile";
            this.btnSelectKnowedFile.Size = new System.Drawing.Size(50, 28);
            this.btnSelectKnowedFile.TabIndex = 2;
            this.btnSelectKnowedFile.Text = "...";
            this.btnSelectKnowedFile.UseVisualStyleBackColor = true;
            this.btnSelectKnowedFile.Click += new System.EventHandler(this.btnSelectKnowedFile_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(80, 173);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标 GDB 位置：";
            // 
            // txtGdbPathDisplay
            // 
            this.txtGdbPathDisplay.BackColor = System.Drawing.SystemColors.Control;
            this.txtGdbPathDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGdbPathDisplay.Location = new System.Drawing.Point(242, 168);
            this.txtGdbPathDisplay.Name = "txtGdbPathDisplay";
            this.txtGdbPathDisplay.ReadOnly = true;
            this.txtGdbPathDisplay.Size = new System.Drawing.Size(311, 28);
            this.txtGdbPathDisplay.TabIndex = 4;
            // 
            // btnSelectGdbLocation
            // 
            this.btnSelectGdbLocation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectGdbLocation.Location = new System.Drawing.Point(560, 169);
            this.btnSelectGdbLocation.Name = "btnSelectGdbLocation";
            this.btnSelectGdbLocation.Size = new System.Drawing.Size(50, 28);
            this.btnSelectGdbLocation.TabIndex = 5;
            this.btnSelectGdbLocation.Text = "...";
            this.btnSelectGdbLocation.UseVisualStyleBackColor = true;
            this.btnSelectGdbLocation.Click += new System.EventHandler(this.btnSelectGdbLocation_Click);
            // 
            // btnOK
            // 
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(129, 378);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 42);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(550, 378);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 42);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtTranFile);
            this.groupBox1.Controls.Add(this.btnSelectTranFile);
            this.groupBox1.Controls.Add(this.btnSelectGdbLocation);
            this.groupBox1.Controls.Add(this.txtGdbPathDisplay);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnSelectKnowedFile);
            this.groupBox1.Controls.Add(this.txtKnowedFile);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(32, 87);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(690, 248);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "基本信息";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(80, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 18);
            this.label3.TabIndex = 8;
            this.label3.Text = "发射源文件：";
            // 
            // txtTranFile
            // 
            this.txtTranFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtTranFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTranFile.Location = new System.Drawing.Point(242, 115);
            this.txtTranFile.Name = "txtTranFile";
            this.txtTranFile.Size = new System.Drawing.Size(311, 28);
            this.txtTranFile.TabIndex = 9;
            // 
            // btnSelectTranFile
            // 
            this.btnSelectTranFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectTranFile.Location = new System.Drawing.Point(560, 117);
            this.btnSelectTranFile.Name = "btnSelectTranFile";
            this.btnSelectTranFile.Size = new System.Drawing.Size(50, 28);
            this.btnSelectTranFile.TabIndex = 10;
            this.btnSelectTranFile.Text = "...";
            this.btnSelectTranFile.UseVisualStyleBackColor = true;
            this.btnSelectTranFile.Click += new System.EventHandler(this.btnSelectTranFile_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(765, 48);
            this.panel1.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 48);
            this.label7.TabIndex = 1;
            this.label7.Text = "导入TEM数据";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Right;
            this.button2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(721, 0);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(44, 48);
            this.button2.TabIndex = 0;
            this.button2.Text = "X";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form_TEMImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 465);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(765, 465);
            this.MinimumSize = new System.Drawing.Size(765, 465);
            this.Name = "Form_TEMImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtKnowedFile;
        private System.Windows.Forms.Button btnSelectKnowedFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGdbPathDisplay;
        private System.Windows.Forms.Button btnSelectGdbLocation;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTranFile;
        private System.Windows.Forms.Button btnSelectTranFile;
    }
}