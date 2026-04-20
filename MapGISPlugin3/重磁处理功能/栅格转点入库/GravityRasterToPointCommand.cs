using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class GravityRasterToPointCommand : ICommand
    {
        private IApplication app;

        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "重力TIFF栅格转点入库"; }
        }

        public string Category
        {
            get { return "重磁处理"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "将 TIFF 栅格转为点并写入指定的 MapGIS 本地数据库。"; }
        }

        public string Name
        {
            get { return "GravityRasterToPointCommand"; }
        }

        public string Tooltip
        {
            get { return "重力TIFF栅格转点入库"; }
        }

        public void OnClick()
        {
            try
            {
                using (GravityRasterToPointForm form = new GravityRasterToPointForm(app))
                {
                    form.ShowDialog();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("打开栅格转点窗口失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnCreate(IApplication hook)
        {
            app = hook;
        }
    }
}
