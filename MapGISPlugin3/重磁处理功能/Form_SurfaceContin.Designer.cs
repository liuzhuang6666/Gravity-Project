namespace MapGISPlugin3
{
    partial class Form_SurfaceContin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelControl1 = new System.Windows.Forms.Panel();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panelControl2 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.pnlMainParams = new System.Windows.Forms.Panel();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.textBoxSavePath = new System.Windows.Forms.TextBox();
            this.labelControl1 = new System.Windows.Forms.Label();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.pnlMainParams.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(870, 30);
            this.panel1.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(-2, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(98, 30);
            this.label7.TabIndex = 1;
            this.label7.Text = "曲面延拓";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(840, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(30, 30);
            this.button1.TabIndex = 0;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.panelControl1);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Location = new System.Drawing.Point(13, 163);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(410, 316);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入数据可视化";
            // 
            // panelControl1
            // 
            this.panelControl1.BackColor = System.Drawing.SystemColors.Control;
            this.panelControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControl1.Location = new System.Drawing.Point(15, 30);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(375, 220);
            this.panelControl1.TabIndex = 0;
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Location = new System.Drawing.Point(155, 255);
            this.button4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(84, 34);
            this.button4.TabIndex = 18;
            this.button4.Text = "刷新";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.panelControl2);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Location = new System.Drawing.Point(442, 163);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(415, 316);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出结果可视化";
            // 
            // panelControl2
            // 
            this.panelControl2.BackColor = System.Drawing.SystemColors.Control;
            this.panelControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControl2.Location = new System.Drawing.Point(15, 30);
            this.panelControl2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(375, 220);
            this.panelControl2.TabIndex = 1;
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Location = new System.Drawing.Point(155, 255);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(84, 34);
            this.button3.TabIndex = 17;
            this.button3.Text = "刷新";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.pnlMainParams);
            this.groupBox3.Controls.Add(this.textBoxSavePath);
            this.groupBox3.Controls.Add(this.labelControl1);
            this.groupBox3.Controls.Add(this.btnBrowseOutput);
            this.groupBox3.Controls.Add(this.btnCalculate);
            this.groupBox3.Location = new System.Drawing.Point(13, 50);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(833, 86);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "计算参数";
            // 
            // pnlMainParams
            // 
            this.pnlMainParams.Controls.Add(this.txtHeight);
            this.pnlMainParams.Controls.Add(this.lblHeight);
            this.pnlMainParams.Location = new System.Drawing.Point(418, 23);
            this.pnlMainParams.Name = "pnlMainParams";
            this.pnlMainParams.Size = new System.Drawing.Size(250, 38);
            this.pnlMainParams.TabIndex = 20;
            // 
            // txtHeight
            // 
            this.txtHeight.BackColor = System.Drawing.SystemColors.Control;
            this.txtHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeight.Location = new System.Drawing.Point(129, 9);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(100, 28);
            this.txtHeight.TabIndex = 1;
            this.txtHeight.Text = "0.0";
            this.txtHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(14, 13);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(116, 18);
            this.lblHeight.TabIndex = 0;
            this.lblHeight.Text = "延拓高度(h):";
            // 
            // textBoxSavePath
            // 
            this.textBoxSavePath.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxSavePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxSavePath.Location = new System.Drawing.Point(125, 33);
            this.textBoxSavePath.Name = "textBoxSavePath";
            this.textBoxSavePath.ReadOnly = true;
            this.textBoxSavePath.Size = new System.Drawing.Size(221, 28);
            this.textBoxSavePath.TabIndex = 8;
            // 
            // labelControl1
            // 
            this.labelControl1.AutoSize = true;
            this.labelControl1.Location = new System.Drawing.Point(30, 38);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(89, 18);
            this.labelControl1.TabIndex = 9;
            this.labelControl1.Text = "输出路径:";
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowseOutput.Location = new System.Drawing.Point(341, 33);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(46, 28);
            this.btnBrowseOutput.TabIndex = 2;
            this.btnBrowseOutput.Text = "...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnCalculate
            // 
            this.btnCalculate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalculate.Location = new System.Drawing.Point(704, 32);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(98, 30);
            this.btnCalculate.TabIndex = 3;
            this.btnCalculate.Text = "计算";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // Form_SurfaceContin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(870, 520);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(870, 520);
            this.MinimumSize = new System.Drawing.Size(870, 520);
            this.Name = "Form_SurfaceContin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "曲面延拓";
            this.Load += new System.EventHandler(this.Form_SurfaceContin_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.pnlMainParams.ResumeLayout(false);
            this.pnlMainParams.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelControl1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panelControl2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel pnlMainParams;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.TextBox textBoxSavePath;
        private System.Windows.Forms.Label labelControl1;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnCalculate;
    }
}