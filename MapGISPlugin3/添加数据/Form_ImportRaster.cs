using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GISControl;
using MapGIS.PluginEngine;
using MapGIS.UI.Controls;
using System;
using System.IO;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public partial class Form_ImportRaster : Form
    {
        private IApplication _app;
        private MapControl _mapControl;

        public Form_ImportRaster(IApplication app, MapControl mapControl)
        {
            InitializeComponent();
            _app = app;
            _mapControl = mapControl;

            this.Load += Form_ImportRaster_Load;
        }

        private void Form_ImportRaster_Load(object sender, EventArgs e)
        {
            // 初始化清空
            txtDataPath.Text = "";
        }

        /// <summary>
        /// 选择数据按钮（支持 GDB 数据库与本地文件）
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // 使用 GDBOpenFileDialog 可以自由在选择本地和数据库之间切换
            using (GDBOpenFileDialog openDlg = new GDBOpenFileDialog())
            {
                openDlg.Title = "选择要导入的栅格数据(本地/GDB均可)";
                openDlg.Filter = "栅格文件(*.img,*.tif,*.tiff,*.grd,*.hdb)|*.img;*.tif;*.tiff;*.grd;*.hdb|栅格数据集|rasdsp";
                openDlg.Multiselect = false;

                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    txtDataPath.Text = openDlg.FileName;
                }
            }
        }

        /// <summary>
        /// 确定导入按钮
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            string url = txtDataPath.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请先选择一条栅格数据路径！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                System.Windows.Forms.Application.DoEvents();

                // 1. 初始化栅格图层
                RasterLayer rstLayer = new RasterLayer();
                rstLayer.URL = url;

                // 2. 连接数据源以验证有效性
                if (rstLayer.ConnectData())
                {
                    // 3. 连接成功后，修改图层显示名称，防止为空或一长串路径
                    string pureName = Path.GetFileNameWithoutExtension(url);
                    // 简单处理：如果是 gdbp:// 开头的数据库图层，做特殊截取
                    if (url.StartsWith("gdbp://", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = url.Split('/');
                        if (parts.Length > 0) pureName = parts[parts.Length - 1];
                    }
                    rstLayer.Name = pureName;

                    // 4. 正式附加至当前活动地图 (此时MapGIS底层会自动通知树状目录去更新节点)
                    _mapControl.ActiveMap.Append(rstLayer);

                    // 5. 仅刷新视图的渲染面板
                    _mapControl.Refresh();
                    _mapControl.TryRefresh();

                    MessageBox.Show($"栅格图层 [{pureName}] 已成功导入并添加至地图！", "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("数据连接加载失败！请检查文件格式或是否被占用。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入栅格时发生异常：\n" + ex.Message, "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}