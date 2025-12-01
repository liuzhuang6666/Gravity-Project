namespace MapGISPlugin3
{
    partial class Form_CSAMTAddData
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
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
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
            this.label1.Location = new System.Drawing.Point(42, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "观测数据文件：";
            // 
            // txtInputFile
            // 
            this.txtInputFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtInputFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInputFile.Location = new System.Drawing.Point(150, 42);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(208, 21);
            this.txtInputFile.TabIndex = 1;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectFile.Location = new System.Drawing.Point(362, 43);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(33, 19);
            this.btnSelectFile.TabIndex = 2;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标 GDB 位置：";
            // 
            // txtGdbPathDisplay
            // 
            this.txtGdbPathDisplay.BackColor = System.Drawing.SystemColors.Control;
            this.txtGdbPathDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGdbPathDisplay.Location = new System.Drawing.Point(150, 113);
            this.txtGdbPathDisplay.Name = "txtGdbPathDisplay";
            this.txtGdbPathDisplay.ReadOnly = true;
            this.txtGdbPathDisplay.Size = new System.Drawing.Size(208, 21);
            this.txtGdbPathDisplay.TabIndex = 4;
            // 
            // btnSelectGdbLocation
            // 
            this.btnSelectGdbLocation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectGdbLocation.Location = new System.Drawing.Point(362, 114);
            this.btnSelectGdbLocation.Name = "btnSelectGdbLocation";
            this.btnSelectGdbLocation.Size = new System.Drawing.Size(33, 19);
            this.btnSelectGdbLocation.TabIndex = 5;
            this.btnSelectGdbLocation.Text = "...";
            this.btnSelectGdbLocation.UseVisualStyleBackColor = true;
            this.btnSelectGdbLocation.Click += new System.EventHandler(this.btnSelectGdbLocation_Click_1);
            // 
            // btnOK
            // 
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(86, 252);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(66, 28);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click_1);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(367, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 28);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click_1);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtTranFile);
            this.groupBox1.Controls.Add(this.btnSelectTranFile);
            this.groupBox1.Controls.Add(this.btnSelectGdbLocation);
            this.groupBox1.Controls.Add(this.txtGdbPathDisplay);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnSelectFile);
            this.groupBox1.Controls.Add(this.txtInputFile);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(21, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 165);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "基本信息";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "发射源文件：";
            // 
            // txtTranFile
            // 
            this.txtTranFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtTranFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTranFile.Location = new System.Drawing.Point(150, 77);
            this.txtTranFile.Name = "txtTranFile";
            this.txtTranFile.Size = new System.Drawing.Size(208, 21);
            this.txtTranFile.TabIndex = 9;
            // 
            // btnSelectTranFile
            // 
            this.btnSelectTranFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectTranFile.Location = new System.Drawing.Point(362, 79);
            this.btnSelectTranFile.Name = "btnSelectTranFile";
            this.btnSelectTranFile.Size = new System.Drawing.Size(33, 19);
            this.btnSelectTranFile.TabIndex = 10;
            this.btnSelectTranFile.Text = "...";
            this.btnSelectTranFile.UseVisualStyleBackColor = true;
            this.btnSelectTranFile.Click += new System.EventHandler(this.btnSelectTranFile_Click_1);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(510, 32);
            this.panel1.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 32);
            this.label7.TabIndex = 1;
            this.label7.Text = "添加CSAMT数据";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Right;
            this.button2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(481, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(29, 32);
            this.button2.TabIndex = 0;
            this.button2.Text = "X";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form_CSAMTAddData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 310);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(510, 310);
            this.MinimumSize = new System.Drawing.Size(510, 310);
            this.Name = "Form_CSAMTAddData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Button btnSelectFile;
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