using System.Windows.Forms;
using MapGIS.GeoMap; // 使用 MapGIS.GeoMap

namespace MapGISPlugin3
{
    public partial class GeophysicalPreviewControl : UserControl
    {
        private Label labelInfo;

        public GeophysicalPreviewControl()
        {
            InitializeComponent();

            this.labelInfo = new Label();
            this.labelInfo.Dock = DockStyle.Fill;
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelInfo.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.Controls.Add(this.labelInfo);
        }

        // 公共方法，用于从外部更新内容
        public void Initialize(MapLayer layer)
        {
            if (layer == null)
            {
                labelInfo.Text = "未选择图层";
                return;
            }

            labelInfo.Text = $"正在预览: {layer.Name}\n" +
                             $"图层类型: {layer.Type}";
        }
    }
}