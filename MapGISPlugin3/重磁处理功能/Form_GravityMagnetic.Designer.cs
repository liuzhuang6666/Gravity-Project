using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    partial class Form_GravityMagnetic
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
            this.方法类型 = new System.Windows.Forms.Label();
            this.comboBox_MethodType = new System.Windows.Forms.ComboBox();
            this.名称 = new System.Windows.Forms.Label();
            this.textBox_LayerName = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // 方法类型
            // 
            this.方法类型.AutoSize = true;
            this.方法类型.Location = new System.Drawing.Point(25, 20);
            this.方法类型.Name = "方法类型";
            this.方法类型.Size = new System.Drawing.Size(65, 12);
            this.方法类型.TabIndex = 0;
            this.方法类型.Text = "方法类型：";
            // 
            // comboBox_MethodType
            // 
            this.comboBox_MethodType.BackColor = System.Drawing.SystemColors.Control;
            this.comboBox_MethodType.FormattingEnabled = true;
            this.comboBox_MethodType.Location = new System.Drawing.Point(132, 20);
            this.comboBox_MethodType.Name = "comboBox_MethodType";
            this.comboBox_MethodType.Size = new System.Drawing.Size(121, 20);
            this.comboBox_MethodType.TabIndex = 1;
            // 
            // 名称
            // 
            this.名称.AutoSize = true;
            this.名称.Location = new System.Drawing.Point(25, 60);
            this.名称.Name = "名称";
            this.名称.Size = new System.Drawing.Size(41, 12);
            this.名称.TabIndex = 2;
            this.名称.Text = "名称：";
            // 
            // textBox_LayerName
            // 
            this.textBox_LayerName.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_LayerName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_LayerName.Location = new System.Drawing.Point(132, 60);
            this.textBox_LayerName.Name = "textBox_LayerName";
            this.textBox_LayerName.Size = new System.Drawing.Size(121, 21);
            this.textBox_LayerName.TabIndex = 3;
            // 
            // btn_OK
            // 
            this.btn_OK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_OK.Location = new System.Drawing.Point(65, 155);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 4;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_Cancel.Location = new System.Drawing.Point(236, 155);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 5;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_LayerName);
            this.groupBox1.Controls.Add(this.名称);
            this.groupBox1.Controls.Add(this.comboBox_MethodType);
            this.groupBox1.Controls.Add(this.方法类型);
            this.groupBox1.Location = new System.Drawing.Point(29, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(313, 89);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "基本信息";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(380, 20);
            this.panel1.TabIndex = 18;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 20);
            this.label7.TabIndex = 1;
            this.label7.Text = "重磁数据导入";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(360, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(20, 20);
            this.button1.TabIndex = 0;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form_GravityMagnetic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 200);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(380, 200);
            this.MinimumSize = new System.Drawing.Size(380, 200);
            this.Name = "Form_GravityMagnetic";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "重磁数据导入";
            this.Load += new System.EventHandler(this.Form_GravityMagnetic_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        
        #endregion

        private System.Windows.Forms.Label 方法类型;
        private System.Windows.Forms.ComboBox comboBox_MethodType;
        private System.Windows.Forms.Label 名称;
        private System.Windows.Forms.TextBox textBox_LayerName;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
    }
}
