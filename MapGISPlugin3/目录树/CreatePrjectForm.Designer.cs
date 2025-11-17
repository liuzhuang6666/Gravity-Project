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
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions1 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.KeyShortcut keyShortcut1 = new DevExpress.Utils.KeyShortcut();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject2 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject3 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject4 = new DevExpress.Utils.SerializableAppearanceObject();
            this.labelControl_name = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.groupControl_info = new DevExpress.XtraEditors.GroupControl();
            this.buttonEdit_path = new DevExpress.XtraEditors.ButtonEdit();
            this.textEdit_name = new DevExpress.XtraEditors.TextEdit();
            this.simpleButton_cancel = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_ok = new DevExpress.XtraEditors.SimpleButton();
            this.panel_title = new DevExpress.XtraEditors.PanelControl();
            this.label_title = new DevExpress.XtraEditors.LabelControl();
            this.btn_close = new DevExpress.XtraEditors.SimpleButton();
            this.toolTipController1 = new DevExpress.Utils.ToolTipController(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_info)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_path.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_name.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panel_title)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl_name
            // 
            this.labelControl_name.Location = new System.Drawing.Point(69, 66);
            this.labelControl_name.Margin = new System.Windows.Forms.Padding(6);
            this.labelControl_name.Name = "labelControl_name";
            this.labelControl_name.Size = new System.Drawing.Size(90, 22);
            this.labelControl_name.TabIndex = 0;
            this.labelControl_name.Text = "工程名称：";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(69, 151);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(6);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(90, 22);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "保存路径：";
            // 
            // groupControl_info
            // 
            this.groupControl_info.Controls.Add(this.buttonEdit_path);
            this.groupControl_info.Controls.Add(this.textEdit_name);
            this.groupControl_info.Controls.Add(this.labelControl1);
            this.groupControl_info.Controls.Add(this.labelControl_name);
            this.groupControl_info.Location = new System.Drawing.Point(87, 60);
            this.groupControl_info.Margin = new System.Windows.Forms.Padding(6);
            this.groupControl_info.Name = "groupControl_info";
            this.groupControl_info.Size = new System.Drawing.Size(571, 239);
            this.groupControl_info.TabIndex = 2;
            this.groupControl_info.Text = "基本信息";
            // 
            // buttonEdit_path
            // 
            this.buttonEdit_path.Location = new System.Drawing.Point(187, 146);
            this.buttonEdit_path.Margin = new System.Windows.Forms.Padding(6);
            this.buttonEdit_path.Name = "buttonEdit_path";
            // 
            // 
            // 
            this.buttonEdit_path.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this.buttonEdit_path.Properties.Appearance.Options.UseBackColor = true;
            this.buttonEdit_path.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.buttonEdit_path.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "...", 20, true, true, false, editorButtonImageOptions1, keyShortcut1, serializableAppearanceObject1, serializableAppearanceObject2, serializableAppearanceObject3, serializableAppearanceObject4, "", null, null, DevExpress.Utils.ToolTipAnchor.Default)});
            this.buttonEdit_path.Properties.ReadOnly = true;
            this.buttonEdit_path.Size = new System.Drawing.Size(286, 31);
            this.buttonEdit_path.TabIndex = 4;
            this.buttonEdit_path.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.buttonEdit_path_ButtonClick);
            // 
            // textEdit_name
            // 
            this.textEdit_name.Location = new System.Drawing.Point(187, 60);
            this.textEdit_name.Margin = new System.Windows.Forms.Padding(6);
            this.textEdit_name.Name = "textEdit_name";
            // 
            // 
            // 
            this.textEdit_name.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this.textEdit_name.Properties.Appearance.Options.UseBackColor = true;
            this.textEdit_name.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.textEdit_name.Size = new System.Drawing.Size(286, 28);
            this.textEdit_name.TabIndex = 3;
            this.textEdit_name.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textEdit_name_KeyPress);
            // 
            // simpleButton_cancel
            // 
            this.simpleButton_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton_cancel.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.simpleButton_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.simpleButton_cancel.Location = new System.Drawing.Point(474, 346);
            this.simpleButton_cancel.Margin = new System.Windows.Forms.Padding(6);
            this.simpleButton_cancel.Name = "simpleButton_cancel";
            this.simpleButton_cancel.Size = new System.Drawing.Size(126, 42);
            this.simpleButton_cancel.TabIndex = 5;
            this.simpleButton_cancel.Text = "取消";
            this.simpleButton_cancel.Click += new System.EventHandler(this.simpleButton_cancel_Click);
            // 
            // simpleButton_ok
            // 
            this.simpleButton_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton_ok.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.simpleButton_ok.Location = new System.Drawing.Point(126, 346);
            this.simpleButton_ok.Margin = new System.Windows.Forms.Padding(6);
            this.simpleButton_ok.Name = "simpleButton_ok";
            this.simpleButton_ok.Size = new System.Drawing.Size(126, 42);
            this.simpleButton_ok.TabIndex = 6;
            this.simpleButton_ok.Text = "确定";
            this.simpleButton_ok.Click += new System.EventHandler(this.simpleButton_ok_Click);
            // 
            // panel_title
            // 
            this.panel_title.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this.panel_title.Appearance.Options.UseBackColor = true;
            this.panel_title.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panel_title.Controls.Add(this.label_title);
            this.panel_title.Controls.Add(this.btn_close);
            this.panel_title.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_title.Location = new System.Drawing.Point(0, 0);
            this.panel_title.Margin = new System.Windows.Forms.Padding(6);
            this.panel_title.Name = "panel_title";
            this.panel_title.Size = new System.Drawing.Size(753, 36);
            this.panel_title.TabIndex = 0;
            // 
            // label_title
            // 
            this.label_title.Appearance.Options.UseTextOptions = true;
            this.label_title.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.label_title.Dock = System.Windows.Forms.DockStyle.Left;
            this.label_title.Location = new System.Drawing.Point(0, 0);
            this.label_title.Margin = new System.Windows.Forms.Padding(6);
            this.label_title.Name = "label_title";
            this.label_title.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.label_title.Size = new System.Drawing.Size(72, 28);
            this.label_title.TabIndex = 0;
            this.label_title.Text = "创建项目";
            // 
            // btn_close
            // 
            this.btn_close.Appearance.Font = new System.Drawing.Font("宋体", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_close.Appearance.Options.UseFont = true;
            this.btn_close.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.btn_close.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_close.Location = new System.Drawing.Point(720, 0);
            this.btn_close.Margin = new System.Windows.Forms.Padding(6);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(33, 36);
            this.btn_close.TabIndex = 1;
            this.btn_close.Text = "X";
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // CreatePrjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 440);
            this.Controls.Add(this.panel_title);
            this.Controls.Add(this.simpleButton_ok);
            this.Controls.Add(this.simpleButton_cancel);
            this.Controls.Add(this.groupControl_info);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximumSize = new System.Drawing.Size(753, 440);
            this.MinimumSize = new System.Drawing.Size(753, 440);
            this.Name = "CreatePrjectForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CreatePorjectForm";
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_info)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_path.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_name.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panel_title)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl_name;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.GroupControl groupControl_info;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit_path;
        private DevExpress.XtraEditors.TextEdit textEdit_name;
        private DevExpress.XtraEditors.SimpleButton simpleButton_cancel;
        private DevExpress.XtraEditors.SimpleButton simpleButton_ok;
        private DevExpress.XtraEditors.PanelControl panel_title;
        private DevExpress.XtraEditors.LabelControl label_title;
        private DevExpress.XtraEditors.SimpleButton btn_close;
        private DevExpress.Utils.ToolTipController toolTipController1;
    }
}