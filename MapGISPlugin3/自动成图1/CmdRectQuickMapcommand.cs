using System.Drawing;
using MapGIS.GISControl;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;

// 【关键修改】加上统一的命名空间
namespace MapGISPlugin3
{
    public class CmdRectQuickMapcommand : ICommand
    {
        private IApplication hk;

        // 1. 修复资源报错：直接返回 null
        public Bitmap Bitmap => null;

        public string Caption => "框选成图";

        public string Category => "快速成图";
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => "框选成图";
        public string Name => "框选成图";
        public string Tooltip => "框选成图";

        public void OnClick()
        {
            if (hk.ActiveContentsView != null && hk.ActiveContentsView is IMapContentsView)
            {
                IContentsView activeContentsView = hk.ActiveContentsView;
                MapControl mapControl = ((IMapContentsView)((activeContentsView is IMapContentsView) ? activeContentsView : null)).MapControl;

                if (mapControl != null && mapControl.ActiveMap != null)
                {
                    // 2. 关键修改：使用 StandardRectTool
                    // 因为大家都在 namespace MapGISPlugin3 下，所以可以直接调用，不需要 using
                    StandardRectTool rectTool = new StandardRectTool(mapControl);

                    rectTool.Application = hk;

                    mapControl.SetBasTool((GISBasTool)(object)rectTool);
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