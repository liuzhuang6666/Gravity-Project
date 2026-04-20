using DevExpress.XtraTreeList.Nodes;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.GISControl;
using MapGIS.PluginEngine;
using MapGIS.WorkSpaceEngine;
using System.Drawing;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public class RasterClipByCoordsCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        public Bitmap Bitmap => null; 
        public string Caption => "精准坐标裁剪";
        public string Category => "地球物理数据处理";
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => "输入指定坐标范围精准裁剪栅格图层";
        public string Name => "RasterClipByCoordsCommand";
        public string Tooltip => "坐标裁剪栅格";

        public void OnClick()
        {
            if (this.app == null || this.app.Document == null) return;

            MapControl mapControl = null;
            if (this.app.ActiveContentsView is IMapContentsView mapView)
            {
                mapControl = mapView.MapControl;
            }

            if (mapControl == null || mapControl.ActiveMap == null)
            {
                MessageBox.Show("未找到激活的二维地图视图！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 寻找最底层的当前可见栅格图层
            RasterLayer targetRasterLayer = null;
            for (int i = 0; i < mapControl.ActiveMap.LayerCount; i++)
            {
                MapLayer layer = mapControl.ActiveMap.get_Layer(i);
                if (layer is RasterLayer && layer.State != LayerState.UnVisible)
                {
                    targetRasterLayer = layer as RasterLayer;
                    break;
                }
            }

            if (targetRasterLayer == null)
            {
                MessageBox.Show("请确保地图中有可见的栅格底图！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 弹窗并把图层和地图控件传过去
            using (Form_RasterClipByCoords form = new Form_RasterClipByCoords(mapControl, targetRasterLayer))
            {
                form.ShowDialog();
            }
        }

        public void OnCreate(IApplication hook) { this.app = hook; }
        public bool BeginGroup => false;
        public bool Visible => true;
        public IApplication App { get => app; set => app = value; }
        public void OnClick(DocumentItem item) { OnClick(); }
        public void OnCreate(IWorkSpace ws) { this.ws = ws; }
        public void OnClick(object sender, params TreeListNode[] items) { OnClick(); }
        public bool IsVisible(object sender, params TreeListNode[] items) => true;
        public bool IsEditable(object sender, params TreeListNode[] items) => true;
    }
}