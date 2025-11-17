using MapGIS.PluginEngine;
namespace MapGISPlugin3
{
    partial class Form1
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
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panelControl1 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.数据导入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelControl2 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonEdit1 = new DevExpress.XtraEditors.ButtonEdit();
            this.pnlPolar = new System.Windows.Forms.Panel();
            this.txtOd = new System.Windows.Forms.TextBox();
            this.lblOd = new System.Windows.Forms.Label();
            this.txtOi = new System.Windows.Forms.TextBox();
            this.lblOi = new System.Windows.Forms.Label();
            this.pnlContin = new System.Windows.Forms.Panel();
            this.txtElev = new System.Windows.Forms.TextBox();
            this.lblElev = new System.Windows.Forms.Label();
            this.pnlDircomp = new System.Windows.Forms.Panel();
            this.txtNi = new System.Windows.Forms.TextBox();
            this.lblNi = new System.Windows.Forms.Label();
            this.txtNd = new System.Windows.Forms.TextBox();
            this.lblNd = new System.Windows.Forms.Label();
            this.pnlDeriv = new System.Windows.Forms.Panel();
            this.txtOrd = new System.Windows.Forms.TextBox();
            this.lblOrd = new System.Windows.Forms.Label();
            this.txtOri = new System.Windows.Forms.TextBox();
            this.lblOri = new System.Windows.Forms.Label();
            // 【新增】二次导数面板
            this.pnlSecond = new System.Windows.Forms.Panel();
            this.txtIsm = new System.Windows.Forms.TextBox();
            this.lblIsm = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit1.Properties)).BeginInit();
            this.pnlPolar.SuspendLayout();
            this.pnlContin.SuspendLayout();
            this.pnlDircomp.SuspendLayout();
            this.pnlDeriv.SuspendLayout();
            // 【新增】SuspendLayout for pnlSecond
            this.pnlSecond.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(302, 150);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "实测数据";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1022, 150);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "转换结果";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1552, 150);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 18);
            this.label3.TabIndex = 3;
            this.label3.Text = "处理方法";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(1490, 178);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(236, 26);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(1326, 562);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(42, 22);
            this.radioButton1.TabIndex = 6;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "X";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(1558, 724);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(42, 22);
            this.radioButton2.TabIndex = 7;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Y";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(1630, 724);
            this.radioButton3.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(42, 22);
            this.radioButton3.TabIndex = 8;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Z";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1323, 540);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 18);
            this.label4.TabIndex = 9;
            this.label4.Text = "文本选择";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1323, 626);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 18);
            this.label5.TabIndex = 10;
            this.label5.Text = "滤波参数";
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(1502, 810);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(242, 39);
            this.button1.TabIndex = 12;
            this.button1.Text = "计算";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panelControl1
            // 
            this.panelControl1.BackColor = System.Drawing.SystemColors.Control;
            this.panelControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControl1.Location = new System.Drawing.Point(33, 178);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(4);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(675, 675);
            this.panelControl1.TabIndex = 15;
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.数据导入ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 48);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1764, 28);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 数据导入ToolStripMenuItem
            // 
            this.数据导入ToolStripMenuItem.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.数据导入ToolStripMenuItem.Name = "数据导入ToolStripMenuItem";
            this.数据导入ToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.数据导入ToolStripMenuItem.Text = "数据导入";
            this.数据导入ToolStripMenuItem.Click += new System.EventHandler(this.数据导入ToolStripMenuItem_Click);
            // 
            // panelControl2
            // 
            this.panelControl2.BackColor = System.Drawing.SystemColors.Control;
            this.panelControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControl2.Location = new System.Drawing.Point(732, 178);
            this.panelControl2.Margin = new System.Windows.Forms.Padding(4);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(675, 675);
            this.panelControl2.TabIndex = 17;
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Location = new System.Drawing.Point(1295, 142);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 34);
            this.button3.TabIndex = 18;
            this.button3.Text = "可视化";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(1480, 724);
            this.radioButton4.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(42, 22);
            this.radioButton4.TabIndex = 19;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "X";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1414, 779);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 18);
            this.label6.TabIndex = 20;
            this.label6.Text = "结果输出";
            // 
            // buttonEdit1
            // 
            this.buttonEdit1.BackColor = System.Drawing.SystemColors.Control;
            this.buttonEdit1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.buttonEdit1.Location = new System.Drawing.Point(1502, 775);
            this.buttonEdit1.Margin = new System.Windows.Forms.Padding(4);
            this.buttonEdit1.Name = "buttonEdit1";
            // 
            // 
            // 
            this.buttonEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.buttonEdit1.Size = new System.Drawing.Size(145, 20);
            this.buttonEdit1.TabIndex = 21;
            // 
            // btnBrowse
            // 
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowse.Location = new System.Drawing.Point(1682, 775);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(62, 28);
            this.btnBrowse.TabIndex = 22;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
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
            // pnlContin
            // 
            this.pnlContin.Controls.Add(this.txtElev);
            this.pnlContin.Controls.Add(this.lblElev);
            this.pnlContin.Location = new System.Drawing.Point(993, 181);
            this.pnlContin.Name = "pnlContin";
            this.pnlContin.Size = new System.Drawing.Size(170, 50);
            this.pnlContin.TabIndex = 23;
            this.pnlContin.Visible = false;
            // 
            // txtElev
            // 
            this.txtElev.Location = new System.Drawing.Point(65, 12);
            this.txtElev.Name = "txtElev";
            this.txtElev.Size = new System.Drawing.Size(100, 21);
            this.txtElev.TabIndex = 0;
            // 
            // lblElev
            // 
            this.lblElev.AutoSize = true;
            this.lblElev.Location = new System.Drawing.Point(3, 15);
            this.lblElev.Name = "lblElev";
            this.lblElev.Size = new System.Drawing.Size(53, 12);
            this.lblElev.TabIndex = 1;
            this.lblElev.Text = "延拓高度";
            // 
            // pnlDircomp
            // 
            this.pnlDircomp.Controls.Add(this.txtNi);
            this.pnlDircomp.Controls.Add(this.lblNi);
            this.pnlDircomp.Controls.Add(this.txtNd);
            this.pnlDircomp.Controls.Add(this.lblNd);
            this.pnlDircomp.Location = new System.Drawing.Point(993, 163);
            this.pnlDircomp.Name = "pnlDircomp";
            this.pnlDircomp.Size = new System.Drawing.Size(170, 80);
            this.pnlDircomp.TabIndex = 24;
            this.pnlDircomp.Visible = false;
            // 
            // txtNi
            // 
            this.txtNi.Location = new System.Drawing.Point(65, 45);
            this.txtNi.Name = "txtNi";
            this.txtNi.Size = new System.Drawing.Size(100, 21);
            this.txtNi.TabIndex = 0;
            // 
            // lblNi
            // 
            this.lblNi.AutoSize = true;
            this.lblNi.Location = new System.Drawing.Point(3, 48);
            this.lblNi.Name = "lblNi";
            this.lblNi.Size = new System.Drawing.Size(53, 12);
            this.lblNi.TabIndex = 1;
            this.lblNi.Text = "方向偏角";
            // 
            // txtNd
            // 
            this.txtNd.Location = new System.Drawing.Point(65, 12);
            this.txtNd.Name = "txtNd";
            this.txtNd.Size = new System.Drawing.Size(100, 21);
            this.txtNd.TabIndex = 2;
            // 
            // lblNd
            // 
            this.lblNd.AutoSize = true;
            this.lblNd.Location = new System.Drawing.Point(3, 15);
            this.lblNd.Name = "lblNd";
            this.lblNd.Size = new System.Drawing.Size(53, 12);
            this.lblNd.TabIndex = 3;
            this.lblNd.Text = "方向倾角";
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
            this.lblOrd.Size = new System.Drawing.Size(53, 12);
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
            this.lblOri.Size = new System.Drawing.Size(53, 12);
            this.lblOri.TabIndex = 3;
            this.lblOri.Text = "方位角";
            // 
            // 【新增】pnlSecond
            // 
            this.pnlSecond.Controls.Add(this.txtIsm);
            this.pnlSecond.Controls.Add(this.lblIsm);
            this.pnlSecond.Location = new System.Drawing.Point(993, 163);  // 与pnlDeriv位置一致
            this.pnlSecond.Name = "pnlSecond";
            this.pnlSecond.Size = new System.Drawing.Size(170, 50);  // 只有一个参数，高度减小
            this.pnlSecond.TabIndex = 26;  // 递增TabIndex
            this.pnlSecond.Visible = false;
            // 
            // 【新增】txtIsm
            // 
            this.txtIsm.Location = new System.Drawing.Point(65, 12);
            this.txtIsm.Name = "txtIsm";
            this.txtIsm.Size = new System.Drawing.Size(100, 21);
            this.txtIsm.TabIndex = 0;
            // 
            // 【新增】lblIsm
            // 
            this.lblIsm.AutoSize = true;
            this.lblIsm.Location = new System.Drawing.Point(3, 15);
            this.lblIsm.Name = "lblIsm";
            this.lblIsm.Size = new System.Drawing.Size(53, 12);
            this.lblIsm.TabIndex = 1;
            this.lblIsm.Text = "计算模式";
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.SystemColors.Control;
            this.panelTitle.Controls.Add(this.labelTitle);
            this.panelTitle.Controls.Add(this.btnClose);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(1764, 48);
            this.panelTitle.TabIndex = 26;
            // 
            // labelTitle
            // 
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(150, 48);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "数据处理";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("宋体", 6.75F);
            this.btnClose.Location = new System.Drawing.Point(1720, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(44, 48);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 601);
            this.Controls.Add(this.pnlSecond);  // 【新增】添加pnlSecond到Form控件
            this.Controls.Add(this.pnlDeriv);
            this.Controls.Add(this.pnlDircomp);
            this.Controls.Add(this.pnlContin);
            this.Controls.Add(this.pnlPolar);
            this.Controls.Add(this.buttonEdit1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.radioButton4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.panelControl2);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据处理";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit1.Properties)).EndInit();
            this.pnlPolar.ResumeLayout(false);
            this.pnlPolar.PerformLayout();
            this.pnlContin.ResumeLayout(false);
            this.pnlContin.PerformLayout();
            this.pnlDircomp.ResumeLayout(false);
            this.pnlDircomp.PerformLayout();
            this.pnlDeriv.ResumeLayout(false);
            this.pnlDeriv.PerformLayout();
            // 【新增】ResumeLayout for pnlSecond
            this.pnlSecond.ResumeLayout(false);
            this.pnlSecond.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panelControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 数据导入ToolStripMenuItem;
        private System.Windows.Forms.Panel panelControl2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.Label label6;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit1;
        private IApplication hook;
        // --- 化极面板的控件 ---
        private System.Windows.Forms.Panel pnlPolar;
        private System.Windows.Forms.Label lblOi; // 磁倾角 标签
        private System.Windows.Forms.TextBox txtOi; // 磁倾角 输入框
        private System.Windows.Forms.Label lblOd; // 磁偏角 标签
        private System.Windows.Forms.TextBox txtOd; // 磁偏角 输入框
        // --- 三角(延拓)面板的控件 ---
        private System.Windows.Forms.Panel pnlContin;
        private System.Windows.Forms.Label lblElev; // 延拓高度 标签
        private System.Windows.Forms.TextBox txtElev; // 延拓高度 输入框
        // --- 方向分量面板的控件 ---
        private System.Windows.Forms.Panel pnlDircomp;
        private System.Windows.Forms.Label lblNd; // 方向倾角 标签
        private System.Windows.Forms.TextBox txtNd; // 方向倾角 输入框
        private System.Windows.Forms.Label lblNi; // 方向偏角 标签
        private System.Windows.Forms.TextBox txtNi; // 方向偏角 输入框
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