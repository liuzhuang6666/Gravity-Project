// MagneticFieldTransformForm.Designer.cs
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    partial class MagneticFieldTransformForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.数据导入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelRight = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.txtOutPath = new System.Windows.Forms.TextBox();
            this.btnSelectOutPath = new System.Windows.Forms.Button();
            this.pnlPolar = new System.Windows.Forms.Panel();
            this.txtOd = new System.Windows.Forms.TextBox();
            this.lblOd = new System.Windows.Forms.Label();
            this.txtOi = new System.Windows.Forms.TextBox();
            this.lblOi = new System.Windows.Forms.Label();
            this.pnlDeriv = new System.Windows.Forms.Panel();
            this.txtOrd = new System.Windows.Forms.TextBox();
            this.lblOrd = new System.Windows.Forms.Label();
            this.txtOri = new System.Windows.Forms.TextBox();
            this.lblOri = new System.Windows.Forms.Label();
            this.pnlSecond = new System.Windows.Forms.Panel();
            this.txtIsm = new System.Windows.Forms.TextBox();
            this.lblIsm = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.pnlPolar.SuspendLayout();
            this.pnlDeriv.SuspendLayout();
            this.pnlSecond.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(201, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "实测数据";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(681, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "转换结果";
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1035, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "处理方法";
            //
            // comboBox1
            //
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(993, 119);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(159, 20);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            //
            // radioButton1
            //
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(884, 375);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(29, 16);
            this.radioButton1.TabIndex = 6;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "X";
            this.radioButton1.UseVisualStyleBackColor = true;
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(882, 360);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "文本选择";
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(882, 417);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "滤波参数";
            //
            // button1
            //
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(1007, 543);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 26);
            this.button1.TabIndex = 12;
            this.button1.Text = "计算";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // panelLeft
            //
            this.panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLeft.Location = new System.Drawing.Point(22, 119);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(450, 450);
            this.panelLeft.TabIndex = 15;
            //
            // menuStrip1
            //
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.数据导入ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 31);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1176, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            //
            // 数据导入ToolStripMenuItem
            //
            this.数据导入ToolStripMenuItem.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.数据导入ToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.数据导入ToolStripMenuItem.Name = "数据导入ToolStripMenuItem";
            this.数据导入ToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.数据导入ToolStripMenuItem.Text = "数据导入";
            this.数据导入ToolStripMenuItem.Click += new System.EventHandler(this.数据导入ToolStripMenuItem_Click);
            //
            // panelRight
            //
            this.panelRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRight.Location = new System.Drawing.Point(488, 119);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(450, 450);
            this.panelRight.TabIndex = 17;
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(948, 520);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 20;
            this.label6.Text = "结果输出";
            //
            // txtOutPath
            //
            this.txtOutPath.BackColor = System.Drawing.SystemColors.Control;
            this.txtOutPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOutPath.Location = new System.Drawing.Point(1007, 516);
            this.txtOutPath.Name = "txtOutPath";
            this.txtOutPath.Size = new System.Drawing.Size(114, 21);
            this.txtOutPath.TabIndex = 21;
            //
            // btnSelectOutPath
            //
            this.btnSelectOutPath.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectOutPath.Location = new System.Drawing.Point(1120, 516);
            this.btnSelectOutPath.Name = "btnSelectOutPath";
            this.btnSelectOutPath.Size = new System.Drawing.Size(32, 21);
            this.btnSelectOutPath.TabIndex = 27;
            this.btnSelectOutPath.Text = "...";
            this.btnSelectOutPath.UseVisualStyleBackColor = true;
            this.btnSelectOutPath.Click += new System.EventHandler(this.btnSelectOutPath_Click);
            //
            // pnlPolar
            //
            this.pnlPolar.Controls.Add(this.txtOd);
            this.pnlPolar.Controls.Add(this.lblOd);
            this.pnlPolar.Controls.Add(this.txtOi);
            this.pnlPolar.Controls.Add(this.lblOi);
            this.pnlPolar.Location = new System.Drawing.Point(993, 181);
            this.pnlPolar.Name = "pnlPolar";
            this.pnlPolar.Size = new System.Drawing.Size(170, 80);
            this.pnlPolar.TabIndex = 22;
            this.pnlPolar.Visible = false;
            //
            // txtOd
            //
            this.txtOd.Location = new System.Drawing.Point(65, 45);
            this.txtOd.Name = "txtOd";
            this.txtOd.Size = new System.Drawing.Size(100, 21);
            this.txtOd.TabIndex = 0;
            //
            // lblOd
            //
            this.lblOd.AutoSize = true;
            this.lblOd.Location = new System.Drawing.Point(10, 48);
            this.lblOd.Name = "lblOd";
            this.lblOd.Size = new System.Drawing.Size(41, 12);
            this.lblOd.TabIndex = 1;
            this.lblOd.Text = "磁偏角";
            //
            // txtOi
            //
            this.txtOi.Location = new System.Drawing.Point(65, 12);
            this.txtOi.Name = "txtOi";
            this.txtOi.Size = new System.Drawing.Size(100, 21);
            this.txtOi.TabIndex = 2;
            //
            // lblOi
            //
            this.lblOi.AutoSize = true;
            this.lblOi.Location = new System.Drawing.Point(10, 15);
            this.lblOi.Name = "lblOi";
            this.lblOi.Size = new System.Drawing.Size(41, 12);
            this.lblOi.TabIndex = 3;
            this.lblOi.Text = "磁倾角";
            //
            // pnlDeriv
            //
            this.pnlDeriv.Controls.Add(this.txtOrd);
            this.pnlDeriv.Controls.Add(this.lblOrd);
            this.pnlDeriv.Controls.Add(this.txtOri);
            this.pnlDeriv.Controls.Add(this.lblOri);
            this.pnlDeriv.Location = new System.Drawing.Point(993, 163);
            this.pnlDeriv.Name = "pnlDeriv";
            this.pnlDeriv.Size = new System.Drawing.Size(170, 80);
            this.pnlDeriv.TabIndex = 25;
            this.pnlDeriv.Visible = false;
            //
            // txtOrd
            //
            this.txtOrd.Location = new System.Drawing.Point(65, 45);
            this.txtOrd.Name = "txtOrd";
            this.txtOrd.Size = new System.Drawing.Size(100, 21);
            this.txtOrd.TabIndex = 0;
            //
            // lblOrd
            //
            this.lblOrd.AutoSize = true;
            this.lblOrd.Location = new System.Drawing.Point(3, 48);
            this.lblOrd.Name = "lblOrd";
            this.lblOrd.Size = new System.Drawing.Size(29, 12);
            this.lblOrd.TabIndex = 1;
            this.lblOrd.Text = "阶数";
            //
            // txtOri
            //
            this.txtOri.Location = new System.Drawing.Point(65, 12);
            this.txtOri.Name = "txtOri";
            this.txtOri.Size = new System.Drawing.Size(100, 21);
            this.txtOri.TabIndex = 2;
            //
            // lblOri
            //
            this.lblOri.AutoSize = true;
            this.lblOri.Location = new System.Drawing.Point(3, 15);
            this.lblOri.Name = "lblOri";
            this.lblOri.Size = new System.Drawing.Size(41, 12);
            this.lblOri.TabIndex = 3;
            this.lblOri.Text = "方位角";
            //
            // pnlSecond
            //
            this.pnlSecond.Controls.Add(this.txtIsm);
            this.pnlSecond.Controls.Add(this.lblIsm);
            this.pnlSecond.Location = new System.Drawing.Point(993, 163);
            this.pnlSecond.Name = "pnlSecond";
            this.pnlSecond.Size = new System.Drawing.Size(170, 50);
            this.pnlSecond.TabIndex = 26;
            this.pnlSecond.Visible = false;
            //
            // txtIsm
            //
            this.txtIsm.Location = new System.Drawing.Point(65, 12);
            this.txtIsm.Name = "txtIsm";
            this.txtIsm.Size = new System.Drawing.Size(100, 21);
            this.txtIsm.TabIndex = 0;
            //
            // lblIsm
            //
            this.lblIsm.AutoSize = true;
            this.lblIsm.Location = new System.Drawing.Point(3, 15);
            this.lblIsm.Name = "lblIsm";
            this.lblIsm.Size = new System.Drawing.Size(53, 12);
            this.lblIsm.TabIndex = 1;
            this.lblIsm.Text = "计算模式";
            //
            // panel1
            //
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1176, 31);
            this.panel1.TabIndex = 27;
            //
            // labelTitle
            //
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(141, 31);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "磁法位场转换";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnClose
            //
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClose.Location = new System.Drawing.Point(1132, 0);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(44, 31);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // MagneticFieldTransformForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 601);
            this.Controls.Add(this.pnlSecond);
            this.Controls.Add(this.pnlDeriv);
            this.Controls.Add(this.pnlPolar);
            this.Controls.Add(this.txtOutPath);
            this.Controls.Add(this.btnSelectOutPath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MagneticFieldTransformForm";
            this.Text = "磁法位场转换";
            this.Load += new System.EventHandler(this.MagneticFieldTransformForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pnlPolar.ResumeLayout(false);
            this.pnlPolar.PerformLayout();
            this.pnlDeriv.ResumeLayout(false);
            this.pnlDeriv.PerformLayout();
            this.pnlSecond.ResumeLayout(false);
            this.pnlSecond.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        // 替换 DevExpress PanelControl
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 数据导入ToolStripMenuItem;
        // 替换 DevExpress PanelControl
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Label label6;
        // 替换 DevExpress ButtonEdit 为 TextBox + Button
        private System.Windows.Forms.TextBox txtOutPath;
        private System.Windows.Forms.Button btnSelectOutPath;
        private IApplication hook;
        // --- 自定义标题栏控件 ---
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button btnClose;
        // --- 化极面板的控件 ---
        private System.Windows.Forms.Panel pnlPolar;
        private System.Windows.Forms.Label lblOi; // 磁倾角 标签
        private System.Windows.Forms.TextBox txtOi; // 磁倾角 输入框
        private System.Windows.Forms.Label lblOd; // 磁偏角 标签
        private System.Windows.Forms.TextBox txtOd; // 磁偏角 输入框
        // --- 方向导数面板的控件 ---
        private System.Windows.Forms.Panel pnlDeriv;
        private System.Windows.Forms.Label lblOri; // 方位角 标签
        private System.Windows.Forms.TextBox txtOri; // 方位角 输入框
        private System.Windows.Forms.Label lblOrd; // 阶数 标签
        private System.Windows.Forms.TextBox txtOrd; // 阶数 输入框
        // 【新增】二次导数面板的控件
        private System.Windows.Forms.Panel pnlSecond;
        private System.Windows.Forms.Label lblIsm; // 计算模式 标签
        private System.Windows.Forms.TextBox txtIsm; // 计算模式 输入框
    }
}