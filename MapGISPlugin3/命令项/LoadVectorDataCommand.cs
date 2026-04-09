using DevExpress.XtraTreeList.Nodes;
using MapGIS.GeoMap;
using MapGIS.PluginEngine;
using MapGIS.WorkSpaceEngine;
using MapGISPlugin3;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    /// <summary>
    /// 加载矢量数据命令 (绑定 Form_AddVectorData 窗口)
    /// </summary>
    public class LoadVectorDataCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            // 使用 MapGIS 内置图标，或者替换为你自己的资源
            get { return MapGIS.Desktop.Resources.Png_Open_16; }
        }

        public string Caption
        {
            get { return "加载矢量数据"; }
        }

        public string Category
        {
            get { return "地球物理数据处理"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "加载 MapGIS 矢量区文件 (.wp) 到地图"; }
        }

        public string Name
        {
            get { return "LoadVectorDataCommand"; }
        }

        public string Tooltip
        {
            get { return "加载矢量数据"; }
        }

        public void OnClick()
        {
            // 安全检查
            if (this.app == null || this.app.Document == null)
            {
                MessageBox.Show("请先打开或新建一个项目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 【关键点】：实例化你刚才提供的 Form_AddVectorData 窗体
            // 传入 this.app 作为构造函数的参数
            using (Form_AddVectorData form = new Form_AddVectorData(this.app))
            {
                form.ShowDialog();
            }
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        #endregion

        #region 其他接口实现

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
        }

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

        public void OnClick(object sender, params DevExpress.XtraTreeList.Nodes.TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params DevExpress.XtraTreeList.Nodes.TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params DevExpress.XtraTreeList.Nodes.TreeListNode[] items)
        {
            return true;
        }
        #endregion
    }
}