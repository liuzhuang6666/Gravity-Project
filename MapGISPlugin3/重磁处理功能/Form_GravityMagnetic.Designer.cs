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
            this.SuspendLayout();
            // 
            // 方法类型
            // 
            this.方法类型.AutoSize = true;
            this.方法类型.Location = new System.Drawing.Point(56, 64);
            this.方法类型.Name = "方法类型";
            this.方法类型.Size = new System.Drawing.Size(53, 12);
            this.方法类型.TabIndex = 0;
            this.方法类型.Text = "方法类型";
            // 
            // comboBox_MethodType
            // 
            this.comboBox_MethodType.FormattingEnabled = true;
            this.comboBox_MethodType.Location = new System.Drawing.Point(163, 64);
            this.comboBox_MethodType.Name = "comboBox_MethodType";
            this.comboBox_MethodType.Size = new System.Drawing.Size(121, 20);
            this.comboBox_MethodType.TabIndex = 1;
            // 
            // 名称
            // 
            this.名称.AutoSize = true;
            this.名称.Location = new System.Drawing.Point(56, 104);
            this.名称.Name = "名称";
            this.名称.Size = new System.Drawing.Size(29, 12);
            this.名称.TabIndex = 2;
            this.名称.Text = "名称";
            // 
            // textBox_LayerName
            // 
            this.textBox_LayerName.Location = new System.Drawing.Point(163, 104);
            this.textBox_LayerName.Name = "textBox_LayerName";
            this.textBox_LayerName.Size = new System.Drawing.Size(121, 21);
            this.textBox_LayerName.TabIndex = 3;
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(58, 144);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 4;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(209, 144);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 5;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // Form_GravityMagnetic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 220);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.textBox_LayerName);
            this.Controls.Add(this.名称);
            this.Controls.Add(this.comboBox_MethodType);
            this.Controls.Add(this.方法类型);
            this.Name = "Form_GravityMagnetic";
            this.Text = "重磁数据导入";
            this.Load += new System.EventHandler(this.Form_GravityMagnetic_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label 方法类型;
        private System.Windows.Forms.ComboBox comboBox_MethodType;
        private System.Windows.Forms.Label 名称;
        private System.Windows.Forms.TextBox textBox_LayerName;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
    }
}