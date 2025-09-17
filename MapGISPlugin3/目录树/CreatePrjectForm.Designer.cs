namespace MapGISPlugin3
{
    partial class CreatePrjectForm
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
            this.panelControl_path = new DevExpress.XtraEditors.PanelControl();
            this.buttonEdit_path = new DevExpress.XtraEditors.ButtonEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_ok = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_cancel = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl_name = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_name = new DevExpress.XtraEditors.TextEdit();
            this.toolTipController1 = new DevExpress.Utils.ToolTipController();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_path)).BeginInit();
            this.panelControl_path.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_path.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_name.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl_path
            // 
            this.panelControl_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl_path.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_path.Controls.Add(this.buttonEdit_path);
            this.panelControl_path.Location = new System.Drawing.Point(83, 58);
            this.panelControl_path.Name = "panelControl_path";
            this.panelControl_path.Size = new System.Drawing.Size(259, 32);
            this.panelControl_path.TabIndex = 17;
            // 
            // buttonEdit_path
            // 
            this.buttonEdit_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit_path.Location = new System.Drawing.Point(40, 9);
            this.buttonEdit_path.Name = "buttonEdit_path";
            this.buttonEdit_path.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.buttonEdit_path.Properties.ReadOnly = true;
            this.buttonEdit_path.Size = new System.Drawing.Size(216, 20);
            this.buttonEdit_path.TabIndex = 6;
            this.buttonEdit_path.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.buttonEdit_path_ButtonClick);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(27, 72);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(48, 14);
            this.labelControl1.TabIndex = 7;
            this.labelControl1.Text = "保存路径";
            // 
            // simpleButton_ok
            // 
            this.simpleButton_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton_ok.Location = new System.Drawing.Point(114, 123);
            this.simpleButton_ok.Name = "simpleButton_ok";
            this.simpleButton_ok.Size = new System.Drawing.Size(60, 23);
            this.simpleButton_ok.TabIndex = 16;
            this.simpleButton_ok.Text = "确定";
            this.simpleButton_ok.Click += new System.EventHandler(this.simpleButton_ok_Click);
            // 
            // simpleButton_cancel
            // 
            this.simpleButton_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.simpleButton_cancel.Location = new System.Drawing.Point(180, 122);
            this.simpleButton_cancel.Name = "simpleButton_cancel";
            this.simpleButton_cancel.Size = new System.Drawing.Size(60, 23);
            this.simpleButton_cancel.TabIndex = 15;
            this.simpleButton_cancel.Text = "取消";
            this.simpleButton_cancel.Click += new System.EventHandler(this.simpleButton_cancel_Click);
            // 
            // labelControl_name
            // 
            this.labelControl_name.Location = new System.Drawing.Point(27, 44);
            this.labelControl_name.Name = "labelControl_name";
            this.labelControl_name.Size = new System.Drawing.Size(48, 14);
            this.labelControl_name.TabIndex = 14;
            this.labelControl_name.Text = "工程名称";
            // 
            // textEdit_name
            // 
            this.textEdit_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textEdit_name.Location = new System.Drawing.Point(83, 41);
            this.textEdit_name.Name = "textEdit_name";
            this.textEdit_name.Size = new System.Drawing.Size(259, 20);
            this.textEdit_name.TabIndex = 13;
            // 
            // CreatePrjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 179);
            this.Controls.Add(this.panelControl_path);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.simpleButton_ok);
            this.Controls.Add(this.simpleButton_cancel);
            this.Controls.Add(this.labelControl_name);
            this.Controls.Add(this.textEdit_name);
            this.Name = "CreatePrjectForm";
            this.Text = "新建工程";
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_path)).EndInit();
            this.panelControl_path.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_path.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_name.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.PanelControl panelControl_path;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit_path;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton simpleButton_ok;
        private DevExpress.XtraEditors.SimpleButton simpleButton_cancel;
        private DevExpress.XtraEditors.LabelControl labelControl_name;
        private DevExpress.XtraEditors.TextEdit textEdit_name;
        private DevExpress.Utils.ToolTipController toolTipController1;
    }
}