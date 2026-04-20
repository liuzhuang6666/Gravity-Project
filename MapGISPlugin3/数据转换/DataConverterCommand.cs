using DevExpress.XtraTreeList.Nodes;
using MapGIS.GeoMap;
using MapGIS.PluginEngine;
using MapGIS.WorkSpaceEngine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    /// <summary>
    /// 数据转换工具命令类：用于调起 Form_Converter 窗口
    /// </summary>
    public class DataConverterCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员实现

        // 按钮显示的图标
        public Bitmap Bitmap
        {
            get
            {
                // 这里建议在 Resources 资源文件中找一个合适的图标
                // 如果没有特定的，可以暂用内置的转换图标
                return MapGIS.Desktop.Resources.Bmp_TreeFolderClosed_16;
            }
        }

        // 按钮显示的文字
        public string Caption
        {
            get { return "数据转换工具"; }
        }

        // 按钮所属的分组（在 MapGIS 界面上的分类）
        public string Category
        {
            get { return "地球物理数据处理"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        // 按钮是否可用：设为 true，并在 OnClick 中进行项目检查
        public bool Enabled
        {
            get { return true; }
        }

        // 状态栏提示文字
        public string Message
        {
            get { return "打开栅格与矢量数据相互转换工具"; }
        }

        public string Name
        {
            get { return "DataConverterCommand"; }
        }

        // 鼠标悬停提示
        public string Tooltip
        {
            get { return "数据转换 (Ras/Vec/Grid)"; }
        }

        /// <summary>
        /// 核心逻辑：点击按钮时触发
        /// </summary>
        public void OnClick()
        {
            // 1. 安全检查：确保程序已初始化且有文档打开
            if (this.app == null || this.app.Document == null)
            {
                MessageBox.Show("请先打开或新建一个地图工程项目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. 调起你的转换窗口
            // 传入 this.app (即 hook) 以便窗口内部可以使用 LayerSelectDialog
            using (Form_Converter form = new Form_Converter(this.app))
            {
                form.ShowDialog();
            }
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
        }

        #endregion

        #region 其他必要接口实现 (仿照 AddDataCommand)

        public bool BeginGroup
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public IApplication App
        {
            get { return app; }
            set { app = value; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }

        #endregion
    }
}