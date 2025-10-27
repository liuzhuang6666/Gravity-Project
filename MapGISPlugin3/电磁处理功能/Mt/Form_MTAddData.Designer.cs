
namespace MapGISPlugin3
{
    partial class Form_MTAddData
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
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(157, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "输入.dat文件";
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(320, 164);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(310, 28);
            this.txtInputFile.TabIndex = 1;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(581, 164);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(49, 29);
            this.btnSelectFile.TabIndex = 2;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(157, 220);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标 GDB 位置:";
            // 
            // txtGdbPathDisplay
            // 
            this.txtGdbPathDisplay.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.txtGdbPathDisplay.Location = new System.Drawing.Point(320, 210);
            this.txtGdbPathDisplay.Name = "txtGdbPathDisplay";
            this.txtGdbPathDisplay.ReadOnly = true;
            this.txtGdbPathDisplay.Size = new System.Drawing.Size(310, 28);
            this.txtGdbPathDisplay.TabIndex = 4;
            // 
            // btnSelectGdbLocation
            // 
            this.btnSelectGdbLocation.Location = new System.Drawing.Point(581, 210);
            this.btnSelectGdbLocation.Name = "btnSelectGdbLocation";
            this.btnSelectGdbLocation.Size = new System.Drawing.Size(49, 29);
            this.btnSelectGdbLocation.TabIndex = 5;
            this.btnSelectGdbLocation.Text = "...";
            this.btnSelectGdbLocation.UseVisualStyleBackColor = true;
            this.btnSelectGdbLocation.Click += new System.EventHandler(this.btnSelectGdbLocation_Click_1);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(334, 269);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 42);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click_1);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(491, 269);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 42);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click_1);
            // 
            // Form_MT_Import
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnSelectGdbLocation);
            this.Controls.Add(this.txtGdbPathDisplay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.txtInputFile);
            this.Controls.Add(this.label1);
            this.Name = "Form_MT_Import";
            this.Text = "Form_MTAddData";
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}