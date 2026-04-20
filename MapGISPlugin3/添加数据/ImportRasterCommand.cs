using DevExpress.XtraTreeList.Nodes;
using MapGIS.GeoMap;
using MapGIS.GISControl;
using MapGIS.PluginEngine;
using MapGIS.WorkSpaceEngine;
using System.Drawing;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public class ImportRasterCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;

        public Bitmap Bitmap => null; // 使用系统自带的添加数据图标
        public string Caption => "导入栅格数据";
        public string Category => "地球物理数据处理";
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => "将本地或数据库的栅格数据(TIF/IMG等)导入到当前地图";
        public string Name => "ImportRasterCommand";
        public string Tooltip => "导入栅格数据";

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
                MessageBox.Show("请先打开或新建一个二维地图视图！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Form_ImportRaster form = new Form_ImportRaster(app, mapControl))
            {
                form.ShowDialog();
            }
        }

        public void OnCreate(IApplication hook) { this.app = hook; }
        public bool BeginGroup => false;
        public bool Visible => true;
        public IApplication App { get => app; set => app = value; }
        public void OnClick(DocumentItem item) { OnClick(); }
        public void OnCreate(IWorkSpace ws) { }
        public void OnClick(object sender, params TreeListNode[] items) { OnClick(); }
        public bool IsVisible(object sender, params TreeListNode[] items) => true;
        public bool IsEditable(object sender, params TreeListNode[] items) => true;
    }
}