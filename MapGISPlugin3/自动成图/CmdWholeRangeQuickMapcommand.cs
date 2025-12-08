using System.Drawing;
using System.Windows.Forms;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GISControl;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;

// 【关键修改】加上统一的命名空间
namespace MapGISPlugin3
{
    public class CmdWholeRangeQuickMapcommand : ICommand
    {
        private IApplication hk;

        // 1. 修复图片资源报错，暂时设为 null
        public Bitmap Bitmap => null;

        // 2. 修复 Application 报错，直接返回固定标题
        public string Caption => "全图成图";

        public string Category => "快速成图";

        public bool Checked => false;

        public bool Enabled => true;

        public string Message => "全图成图";

        public string Name => "全图成图";

        public string Tooltip => "全图成图";

        public void OnClick()
        {
            if (hk.ActiveContentsView == null || !(hk.ActiveContentsView is IMapContentsView))
            {
                return;
            }
            IContentsView activeContentsView = hk.ActiveContentsView;
            MapControl mapControl = ((IMapContentsView)((activeContentsView is IMapContentsView) ? activeContentsView : null)).MapControl;

            if (mapControl != null && mapControl.ActiveMap != null)
            {
                Rect rect = null; // 全图模式下范围传 null

                // 3. 【关键修改】使用你的新窗口 StandardQuickMap
                // 此时 StandardQuickMap 和本类都在 namespace MapGISPlugin3 下，直接调用即可
                StandardQuickMap quickMap = new StandardQuickMap(mapControl.ActiveMap, rect);
                quickMap.Application = hk;

                // 使用 StandardQuickMap 的静态检查方法
                if (StandardQuickMap.IsDataOk(quickMap.InitUI()))
                {
                    ((Form)(object)quickMap).ShowDialog();
                }
            }
        }

        public void OnCreate(IApplication hook)
        {
            if (hook != null)
            {
                hk = hook;
            }
        }
    }
}