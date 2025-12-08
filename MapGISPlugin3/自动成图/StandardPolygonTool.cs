using MapGIS.Desktop.UI.Controls;
using MapGIS.GeoMap;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GISControl;
using MapGIS.GISControl.IATool;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;
using MapGISPlugin3;
using System.Windows.Forms;


// 【修改点1】类名改为 StandardPolygonTool
public class StandardPolygonTool : GISBasTool
{
    private MapControl mapControl;
    private IAPolygon iaTool;
    private bool isEscFlag;
    private IApplication app;

    public IApplication Application
    {
        get { return app; }
        set { app = value; }
    }

    public override int SubType => 1;

    // 【修改点2】构造函数名改为 StandardPolygonTool
    public StandardPolygonTool(MapControl control)
    {
        mapControl = control;
        iaTool = new IAPolygon(mapControl, null, isfill: false, erasewhenfinish: false, mapControl.Transformation, null, canSnap: false);
        iaTool.Finish += Polygontool_Finish;
        base.Active += IAQueryPolygonTool_Active;
        base.Unactive += IAQueryPolygonTool_Unactive;
        base.Cancel += IAQueryPolygonTool_Cancel;
        base.PreRefresh += IAQueryPolygonTool_PreRefresh;
        base.PostRefresh += IAQueryPolygonTool_PostRefresh;
    }

    private void IAQueryPolygon(object obj)
    {
        if (obj != null)
        {
            GeoPolygon geopolygon = obj as GeoPolygon;

            // =========================================================
            // 【关键修改点3】这里调用你的新窗口 StandardQuickMap
            // =========================================================
            StandardQuickMap quickMap = new StandardQuickMap(mapControl.ActiveMap, geopolygon);
            quickMap.Application = app;

            // 调用 StandardQuickMap 里的静态检查方法
            if (StandardQuickMap.IsDataOk(quickMap.InitUI()))
            {
                ((Form)(object)quickMap).ShowDialog();
            }
            iaTool.PostRefresh();
        }
    }

    // 下面的代码保持原样
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

    private void IAQueryPolygonTool_PostRefresh(object sender, ToolEventArgs e)
    {
        iaTool.PostRefresh();
    }

    private void IAQueryPolygonTool_PreRefresh(object sender, ToolEventArgs e)
    {
    }

    private void IAQueryPolygonTool_Cancel(object sender, ToolEventArgs e)
    {
        iaTool.Cancel();
    }

    private void IAQueryPolygonTool_Unactive(object sender, ToolEventArgs e)
    {
        iaTool.Pause();
    }

    private void IAQueryPolygonTool_Active(object sender, ToolEventArgs e)
    {
        StressMapItem.ClearStress(mapControl);
        iaTool.Start();
    }

    private void Polygontool_Finish(object sender, ToolEventArgs e)
    {
        if (!isEscFlag)
        {
            IAQueryPolygon(e.Geom);
        }
    }
}