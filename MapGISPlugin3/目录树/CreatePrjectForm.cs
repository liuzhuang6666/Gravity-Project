using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using MapGIS.GeoDataBase;
using MapGIS.PlugUtility;
using MapGIS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        // 新增：拖动和拉伸相关变量
        private Point mousePoint = new Point();

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
            this.isNew = isNew;

            if (!isNew)
            {
                groupControl_info.Visible = false;
                this.Height = this.Height - groupControl_info.Height;
                this.Name = "添加子节点";
                parentType = type;
            }

            // 新增：初始化拖动和拉伸功能
            InitDragEvent();
        }

        /// <summary>
        /// 初始化标题栏拖动事件（绑定panel_title）
        /// </summary>
        private void InitDragEvent()
        {
            panel_title.MouseDown += TitlePanel_MouseDown;
            panel_title.MouseMove += TitlePanel_MouseMove;
        }

        /// <summary>
        /// 标题栏按下：记录鼠标相对位置
        /// </summary>
        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousePoint.X = e.X;
                mousePoint.Y = e.Y;
            }
        }

        /// <summary>
        /// 标题栏移动：计算窗口新位置
        /// </summary>
        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left = Control.MousePosition.X - mousePoint.X;
                this.Top = Control.MousePosition.Y - mousePoint.Y;
            }
        }

        #region 原有事件处理
        private void textEdit_name_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 1. 允许控制键（如退格键 Backspace），否则无法删除
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // 2. 获取操作系统定义的文件名非法字符 (包含 \ / : * ? " < > | 等)
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // 3. 检查输入的字符是否在非法字符列表中
            if (invalidChars.Contains(e.KeyChar))
            {
                e.Handled = true; // 阻止输入

                string errorMsg = "名称不能包含下列任何字符: \\ / : * ? \" < > |";
                toolTipController1.ShowHint(errorMsg, sender as Control, DevExpress.Utils.ToolTipLocation.BottomCenter);
            }
            else
            {
                // 如果输入合法，隐藏之前的提示
                toolTipController1.HideHint();
            }
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
                prjName = name;
                prjPath = path;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonEdit_path_ButtonClick(object sender, ButtonPressedEventArgs e)
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
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}