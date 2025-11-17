using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.WorkSpaceEngine;
using DevExpress.XtraTreeList.Nodes;
using MapGIS.GISControl;

namespace MapGISPlugin3
{
    // (重要) 遵循您的 NewProjectCommand 模式
    public class GeophysicalPreviewCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        // 我们不再需要 m_app 或 m_workspace 成员变量
        // 我们将直接使用 InitPlugin.App

        /// <summary>
        /// (核心) 当右键菜单被点击时，框架会调用这个方法！
        /// </summary>
        /// <param name="item">这就是被右键点击的图层 (MapLayer)!</param>
        public void OnClick(DocumentItem item)
        {
            // 1. 检查传入的 item 是否是一个图层
            if (!(item is MapLayer))
            {
                return; // 不是图层，不执行
            }

            // 2. 将 item 转换为 MapLayer
            MapLayer layer = item as MapLayer;

            // 3. 【【【 最终修正：使用 InitPlugin.App 获取 IApplication 】】】
            IApplication app = InitPlugin.App;
            if (app == null)
            {
                MessageBox.Show("错误：无法从 InitPlugin.App 获取 IApplication 实例。");
                return;
            }

            // 4. 定义我们的标签页的 "类型" 和 "唯一Key"
            string viewTypeName = "MapGISPlugin3.GeophysicalPreviewContentsView";
            string viewKey = "GeophysicalPreview_SingletonKey"; // 唯一的Key，确保只打开一个

            IContentsView previewView = null;
            IPluginContainer container = app.PluginContainer; // <-- (修正点) 使用 app

            // 5. 尝试从已打开的标签页中查找
            if (container.ContentsViews.ContainsKey(viewKey))
            {
                previewView = container.ContentsViews[viewKey];
            }
            else
            {
                // 6. 如果没找到，就创建新的 (依赖 .dcprj 注册)
                previewView = container.CreateContentsView(viewTypeName, viewKey);
            }

            if (previewView == null)
            {
                MessageBox.Show("错误：无法创建或查找 " + viewTypeName +
                                "\n请确保它已在“数据中心设计器”的 <contentsviews> 节点中注册。",
                                "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 7. 将图层数据传递给标签页
            if (previewView is GeophysicalPreviewContentsView)
            {
                (previewView as GeophysicalPreviewContentsView).UpdateContent(layer);
            }

            // 8. 激活标签页
            container.ActiveContentsView(previewView);
        }

        #region 仿 NewProjectCommand 实现的其他接口

        /// <summary>
        /// (重要) Enabled 属性
        /// </summary>
        public bool Enabled
        {
            get
            {
                // 因为 Enabled 检查是在 OnClick(item) 之前，
                // 并且它没有 item 参数，我们无法在这里判断图层。
                // 您的 NewProjectCommand 设为 true，我们也设为 true。
                // 真正的检查在 OnClick(DocumentItem item) 内部进行。
                return true;
            }
        }

        // --- OnCreate (由 InitPlugin.cs 调用) ---
        // 我们不再需要在 OnCreate 中存储任何东西，
        // 因为我们将使用 InitPlugin.App
        public void OnCreate(IWorkSpace ws)
        {
            // 遵循 NewProjectCommand 的模式
        }

        // --- 其他接口的实现 ---

        // (当按钮在工具栏上被点击时调用的)
        public void OnClick()
        {
            // 检查 InitPlugin.App 和 ActiveContentsView
            IApplication app = InitPlugin.App;
            if (app == null) return;

            IContentsView activeView = app.ActiveContentsView;
            if (activeView is IMapContentsView)
            {
                // (这是一个有风险的假设，因为TOC可能没有选中图层)
                // 我们最好还是提示用户
                MessageBox.Show("请通过TOC右键菜单使用此功能。");
            }
        }

        public string Caption { get { return "地球物理预览"; } }
        public string Name { get { return "GeophysicalPreviewCommand"; } }
        public string Category { get { return "地球物理数据处理"; } }
        public string Tooltip { get { return "地球物理预览"; } }
        public Bitmap Bitmap { get { return null; } }
        public bool Checked { get { return false; } }
        public bool BeginGroup { get { return true; } }
        public bool Visible { get { return true; } }
        public string Message { get { return "显示地球物理预览"; } }

        public IApplication App
        {
            get { return InitPlugin.App; } // <-- (修正点) 总是返回静态实例
            set { /* 遵循 NewProjectCommand，什么都不做 */ }
        }

        // ICommand 的 OnCreate
        public void OnCreate(IApplication hook)
        {
            // 遵循 NewProjectCommand 的模式
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            // 检查 items 是否有效，并调用 OnClick(DocumentItem)
            if (items != null && items.Length > 0 && items[0].Tag is DocumentItem)
            {
                OnClick(items[0].Tag as DocumentItem);
            }
        }

        public bool IsVisible(object sender, params TreeListNode[] items) { return true; }
        public bool IsEditable(object sender, params TreeListNode[] items) { return true; }

        #endregion
    }
}