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

        // 新增：拖动和拉伸相关变量
        private Point mousePoint = new Point();
        private int lastWidth = 0;
        private int lastHeight = 0;
        private Label[] borderLabels = new Label[4]; // 4个边框用于拉伸

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
            InitCustomBorder();
            InitDragEvent();
        }

        #region 新增：窗口拖动与拉伸核心逻辑
        /// <summary>
        /// 初始化自定义边框（左、上、右、下）
        /// </summary>
        private void InitCustomBorder()
        {
            // 创建4个边框Label（用于拉伸）
            for (int i = 0; i < 4; i++)
            {
                borderLabels[i] = new Label();
                borderLabels[i].BackColor = System.Drawing.Color.FromArgb(188, 182, 211); // 边框颜色，与标题栏协调
                borderLabels[i].Size = new Size(2, 2);
                this.Controls.Add(borderLabels[i]);
                this.Controls.SetChildIndex(borderLabels[i], 0); // 确保边框在最底层
            }

            // 设置边框停靠和光标
            borderLabels[0].Dock = DockStyle.Left;     // 左边框（左右拉伸）
            borderLabels[1].Dock = DockStyle.Top;      // 上边框（上下拉伸）
            borderLabels[2].Dock = DockStyle.Right;    // 右边框（左右拉伸）
            borderLabels[3].Dock = DockStyle.Bottom;   // 下边框（上下拉伸）

            borderLabels[0].Cursor = Cursors.SizeWE;
            borderLabels[2].Cursor = Cursors.SizeWE;
            borderLabels[1].Cursor = Cursors.SizeNS;
            borderLabels[3].Cursor = Cursors.SizeNS;

            // 绑定边框事件
            foreach (var label in borderLabels)
            {
                label.MouseDown += Border_MouseDown;
            }
            borderLabels[0].MouseMove += LeftBorder_MouseMove;
            borderLabels[1].MouseMove += TopBorder_MouseMove;
            borderLabels[2].MouseMove += RightBorder_MouseMove;
            borderLabels[3].MouseMove += BottomBorder_MouseMove;
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

        /// <summary>
        /// 边框按下：记录窗口初始尺寸和鼠标位置
        /// </summary>
        private void Border_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastWidth = this.Width;
                lastHeight = this.Height;
                mousePoint = Control.MousePosition;
            }
        }

        /// <summary>
        /// 左边框拉伸
        /// </summary>
        private void LeftBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newWidth = lastWidth - (Control.MousePosition.X - mousePoint.X);
                if (newWidth >= 400) // 限制最小宽度（适配表单内容）
                {
                    this.Width = newWidth;
                    this.Left = Control.MousePosition.X;
                }
            }
        }

        /// <summary>
        /// 右边框拉伸
        /// </summary>
        private void RightBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newWidth = lastWidth + (Control.MousePosition.X - mousePoint.X);
                if (newWidth >= 400)
                {
                    this.Width = newWidth;
                }
            }
        }

        /// <summary>
        /// 上边框拉伸
        /// </summary>
        private void TopBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newHeight = lastHeight - (Control.MousePosition.Y - mousePoint.Y);
                // 最小高度：根据是否显示groupControl_info动态调整
                int minHeight = isNew ? 250 : 150;
                if (newHeight >= minHeight)
                {
                    this.Height = newHeight;
                    this.Top = Control.MousePosition.Y;
                }
            }
        }

        /// <summary>
        /// 下边框拉伸
        /// </summary>
        private void BottomBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newHeight = lastHeight + (Control.MousePosition.Y - mousePoint.Y);
                int minHeight = isNew ? 250 : 150;
                if (newHeight >= minHeight)
                {
                    this.Height = newHeight;
                }
            }
        }
        #endregion

        #region 原有事件处理
        private void textEdit_name_KeyPress(object sender, KeyPressEventArgs e)
        {
            string errorMsg = "";
            if (!e.Handled)
            {
                Server server = new Server();
                char[] invalidChars = server.GetInvalidChars(InvalidCharType.DataBaseName);
                if (invalidChars != null)
                {
                    List<char> invalidCharList = new List<char>(invalidChars);
                    invalidCharList.Remove((char)3);
                    invalidCharList.Remove((char)22);
                    invalidCharList.Remove((char)24);
                    invalidChars = invalidCharList.ToArray();
                }
                if (!MapGIS.Desktop.UI.Controls.KeyPressValidate.ValidateKeyChar(sender as TextEdit, e.KeyChar, 32, out errorMsg, invalidChars))
                    e.Handled = true;
                server.Dispose();
            }
            if (!string.IsNullOrEmpty(errorMsg))
                toolTipController1.ShowHint(errorMsg, sender as Control, DevExpress.Utils.ToolTipLocation.BottomCenter);
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