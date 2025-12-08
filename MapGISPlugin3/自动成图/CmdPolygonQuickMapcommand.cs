using System.Drawing;
using MapGIS.GISControl;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;
// using MapGISPlugin3; // 如果大家都在同一个 namespace 下，这行就不需要了

// 【关键修改】加上这个大括号，把类包起来
namespace MapGISPlugin3
{
    public class CmdPolygonQuickMapcommand : ICommand
    {
        private IApplication hk;

        public Bitmap Bitmap => null;

        public string Caption => "多边形成图";

        public string Category => "快速成图";

        public bool Checked => false;

        public bool Enabled => true;

        public string Message => "多边形成图";

        public string Name => "多边形成图";

        public string Tooltip => "多边形成图";

        public void OnClick()
        {
            if (hk.ActiveContentsView != null && hk.ActiveContentsView is IMapContentsView)
            {
                IContentsView activeContentsView = hk.ActiveContentsView;
                MapControl mapControl = ((IMapContentsView)((activeContentsView is IMapContentsView) ? activeContentsView : null)).MapControl;

                if (mapControl != null && mapControl.ActiveMap != null)
                {
                    // 3. 【关键修改】使用 StandardPolygonTool
                    StandardPolygonTool polygonQuickMapTool = new StandardPolygonTool(mapControl);
                    polygonQuickMapTool.Application = hk;

                    // 激活工具
                    mapControl.SetBasTool((GISBasTool)(object)polygonQuickMapTool);
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
} // 别忘了最后这个大括号