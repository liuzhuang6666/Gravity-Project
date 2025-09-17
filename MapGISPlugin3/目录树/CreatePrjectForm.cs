using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using MapGIS.GeoDataBase;
using MapGIS.PlugUtility;
using MapGIS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MapGISPlugin3
{
    public partial class CreatePrjectForm : XtraForm
    {
        private string prjPath;
        private string prjName;
        private string MapType;
        private string parentType;
        private bool isNew;

        /// <summary>
        /// 保存路径
        /// </summary>
        public string PrjPath
        {
            get { return prjPath; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string PrjName
        {
            get { return prjName; }
        }

       

        public CreatePrjectForm(bool isNew, string type = null)
        {
            InitializeComponent();
            if (!isNew)
            {
                this.Height = this.Height - panelControl_path.Height;
                panelControl_path.Visible = false;
                this.Name = "添加子节点";
                parentType = type;
            }
            
            this.isNew = isNew;
        }

        

        private void textEdit_name_KeyPress(object sender, KeyPressEventArgs e)
        {
            string errorMsg = "";
            if (!e.Handled)
            {
                Server server = new Server();
                char[] invalidChars = server.GetInvalidChars(InvalidCharType.DataBaseName);
                if (invalidChars != null)
                {
                    //从数据源获取的数据库名非法字符中包含了Ctrl+C和Ctrl+V，导致无法粘贴。
                    List<char> invalidCharList = new List<char>(invalidChars);
                    invalidCharList.Remove((char)3);
                    invalidCharList.Remove((char)22);
                    invalidCharList.Remove((char)24);
                    invalidChars = invalidCharList.ToArray();
                }
                //hdf名称不能超过32字符,故此处限制工程名称不能超过32个字符.
                if (!MapGIS.Desktop.UI.Controls.KeyPressValidate.ValidateKeyChar(sender as DevExpress.XtraEditors.TextEdit, e.KeyChar, 32, out errorMsg, invalidChars))
                    e.Handled = true;
                server.Dispose();
            }
            if (!string.IsNullOrEmpty(errorMsg))
                this.toolTipController1.ShowHint(errorMsg, sender as Control, DevExpress.Utils.ToolTipLocation.BottomCenter);
        }

        private void simpleButton_ok_Click(object sender, EventArgs e)
        {
            string name = textEdit_name.Text;
            string path = buttonEdit_path.Text;
            if (string.IsNullOrWhiteSpace(name))
                XMessageBox.Information("请输入名称");
            else if (isNew && string.IsNullOrWhiteSpace(path))
                XMessageBox.Information("请选择保存路径");
            else
            {
                this.prjName = name;
                this.prjPath = path;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void buttonEdit_path_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (GDBSelectFolderDialog folderDlg = new GDBSelectFolderDialog())
            {
                folderDlg.FolderType = FolderType.Windows_Folder;
                folderDlg.SelectedPath = (sender as ButtonEdit).Text as string;
                if (DialogResult.OK == folderDlg.ShowDialog())
                {
                    (sender as ButtonEdit).Text = folderDlg.SelectedPath;
                }
            }
        }

        private void simpleButton_cancel_Click(object sender, EventArgs e)
        {

        }
    }
}