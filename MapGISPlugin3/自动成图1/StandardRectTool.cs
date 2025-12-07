using MapGIS.Desktop.UI.Controls;
using MapGIS.GeoMap;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GISControl;
using MapGIS.GISControl.IATool;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;
using System.Windows.Forms;

namespace MapGISPlugin3 {
    // 【修改点1】类名改为 StandardRectTool
    internal class StandardRectTool : GISBasTool
    {
        private MapControl mapControl;
        private IARect iaTool;
        private bool isEscFlag;
        private IApplication app;

        public IApplication Application
        {
            get { return app; }
            set { app = value; }
        }

        public override int SubType => 0;

        // 【修改点2】构造函数名改为 StandardRectTool
        public StandardRectTool(MapControl control)
        {
            mapControl = control;
            iaTool = new IARect(mapControl, null, isfill: false, erasewhenfinish: false, mapControl.Transformation, null, canSnap: false);
            iaTool.Finish += rectool_Finish;
            base.Active += IAQueryRectTool_Active;
            base.Unactive += IAQueryRectTool_Unactive;
            base.Cancel += IAQueryRectTool_Cancel;
            base.PreRefresh += IAQueryRectTool_PreRefresh;
            base.PostRefresh += IAQueryRectTool_PostRefresh;
        }

        private void IAQueryRect(object obj)
        {
            if (obj != null)
            {
                GeoRect geoRect = (GeoRect)obj;
                geoRect.DisperseToDots(0.2);

                // =========================================================
                // 【关键修改点3】这里调用你刚写好的 StandardQuickMap
                // =========================================================
                StandardQuickMap quickMap = new StandardQuickMap(mapControl.ActiveMap, geoRect.CalRect());
                quickMap.Application = app;

                // 调用 StandardQuickMap 里的静态检查方法
                if (StandardQuickMap.IsDataOk(quickMap.InitUI()))
                {
                    ((Form)(object)quickMap).ShowDialog();
                }
                iaTool.PostRefresh();
            }
        }

        // 下面的代码保持原样即可
        public override int OnMouseDown(object sender, MouseEventArgs e)
        {
            iaTool.OnMouseDown(sender, e);
            return 0;
        }

        public override int OnMouseMove(object sender, MouseEventArgs e)
        {
            iaTool.OnMouseMove(sender, e);
            return 0;
        }

        public override int OnMouseUp(object sender, MouseEventArgs e)
        {
            iaTool.OnMouseUp(sender, e);
            if (e.Button == MouseButtons.Right && !isEscFlag)
            {
                mapControl.SetBasTool(null);
            }
            return 0;
        }

        public override int OnKeyDown(object sender, KeyEventArgs e)
        {
            iaTool.OnKeyDown(sender, e);
            return 0;
        }

        private void IAQueryRectTool_PostRefresh(object sender, ToolEventArgs e)
        {
            iaTool.PostRefresh();
        }

        private void IAQueryRectTool_PreRefresh(object sender, ToolEventArgs e)
        {
        }

        private void IAQueryRectTool_Cancel(object sender, ToolEventArgs e)
        {
            iaTool.Cancel();
        }

        private void IAQueryRectTool_Unactive(object sender, ToolEventArgs e)
        {
            iaTool.Pause();
        }

        private void IAQueryRectTool_Active(object sender, ToolEventArgs e)
        {
            StressMapItem.ClearStress(mapControl);
            iaTool.Start();
        }

        private void rectool_Finish(object sender, ToolEventArgs e)
        {
            if (!isEscFlag)
            {
                IAQueryRect(e.Geom);
            }
        }
    }
}
