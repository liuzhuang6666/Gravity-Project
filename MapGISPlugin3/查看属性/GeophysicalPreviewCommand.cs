using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.WorkSpaceEngine;
using DevExpress.XtraTreeList.Nodes;
using MapGIS.GISControl;

namespace MapGISPlugin3
{
    public class GeophysicalPreviewCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        // ---------------------------------------------------------
        // 🖱️ 1. 核心点击逻辑 (TOC 右键菜单触发)
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // 🖱️ 修复后的点击逻辑
        // ---------------------------------------------------------
        public void OnClick(DocumentItem item)
        {
            // 1. 只要是 MapLayer (包含矢量、栅格、属性表) 都放行
            if (!(item is MapLayer))
            {
                return;
            }

            // 2. 统一转换为 MapLayer
            MapLayer layer = item as MapLayer;

            // 获取应用程序实例
            IApplication app = InitPlugin.App;
            if (app == null) return;

            // --- 打开/激活预览窗口 ---
            string viewTypeName = "MapGISPlugin3.GeophysicalPreviewContentsView";
            string viewKey = "GeophysicalPreview_SingletonKey";

            IPluginContainer container = app.PluginContainer;
            IContentsView previewView = null;

            if (container.ContentsViews.ContainsKey(viewKey))
            {
                previewView = container.ContentsViews[viewKey];
            }
            else
            {
                previewView = container.CreateContentsView(viewTypeName, viewKey);
            }

            if (previewView == null) return;

            // 3. 传值
            if (previewView is GeophysicalPreviewContentsView)
            {
                // 直接传 layer 即可，不需要再区分 ObjectLayer 了
                (previewView as GeophysicalPreviewContentsView).UpdateContent(layer);
            }

            container.ActiveContentsView(previewView);
        }
        // ---------------------------------------------------------
        // 🚦 2. 状态控制
        // ---------------------------------------------------------
        public bool Enabled
        {
            get
            {
                // 永远可用，把判断逻辑留给 OnClick
                // 这样即使是不识别的图层，菜单也是亮的，点了之后我们可以提示“不支持”
                return true;
            }
        }

        // ---------------------------------------------------------
        // 📋 3. 菜单外观信息
        // ---------------------------------------------------------
        public string Caption { get { return "地球物理预览"; } }
        public string Name { get { return "GeophysicalPreviewCommand"; } }
        public string Category { get { return "地球物理工具"; } }
        public string Tooltip { get { return "预览地球物理数据(MT/重磁/表格)"; } }
        public string Message { get { return "打开地球物理数据预览视图"; } }
        public Bitmap Bitmap { get { return null; } } // 这里可以设置图标

        // ---------------------------------------------------------
        // ⚙️ 4. 接口的其他实现 (保持默认即可)
        // ---------------------------------------------------------
        public IApplication App { get { return InitPlugin.App; } set { } }

        public void OnCreate(IApplication hook) { }
        public void OnCreate(IWorkSpace ws) { }

        public bool Checked { get { return false; } }
        public bool BeginGroup { get { return true; } }
        public bool Visible { get { return true; } }

        // 工具栏点击 (我们主要用右键菜单，这里可以留空或提示)
        public void OnClick()
        {
            MessageBox.Show("请在图层树(TOC)中右键点击图层使用此功能。");
        }

        // 兼容 TreeListNode 的点击 (通常会转发给 OnClick(DocumentItem))
        public void OnClick(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0 && items[0].Tag is DocumentItem)
            {
                OnClick(items[0].Tag as DocumentItem);
            }
        }

        public bool IsVisible(object sender, params TreeListNode[] items) { return true; }
        public bool IsEditable(object sender, params TreeListNode[] items) { return true; }
    }
}