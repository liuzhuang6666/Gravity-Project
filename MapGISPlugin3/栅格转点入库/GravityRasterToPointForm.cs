using System;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.UI.Controls;

namespace MapGISPlugin3
{
    public partial class GravityRasterToPointForm : Form
    {
        private readonly IApplication app;
        private readonly GravityRasterToPointService service;
        private GravityRasterToPointRasterInfo currentInfo;
        private string selectedDataSource = "MapGisLocalPlus";
        private string selectedGdbDirectory = "/Temporary";

        public GravityRasterToPointForm(IApplication app)
        {
            this.app = app;
            service = new GravityRasterToPointService();
            InitializeComponent();
            UpdateGdbPathDisplay();
        }

        private void btnBrowseTiff_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "选择 TIFF 文件";
                dialog.Filter = "TIFF 文件|*.tif;*.tiff";
                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                txtTiffPath.Text = dialog.FileName;
                txtClassName.Text = service.BuildDefaultClassName(dialog.FileName);
                LoadRasterInfo(dialog.FileName);
            }
        }

        private void btnBrowseDatabase_Click(object sender, EventArgs e)
        {
            using (GDBSelectFolderDialog gdbDialog = new GDBSelectFolderDialog())
            {
                gdbDialog.Title = "选择保存 GDB 位置";
                try
                {
                    gdbDialog.SelectedPath = $"gdbp://{selectedDataSource}{selectedGdbDirectory}";
                }
                catch
                {
                }

                if (gdbDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string fullSelectedPath = gdbDialog.SelectedPath;
                if (string.IsNullOrWhiteSpace(fullSelectedPath) || !fullSelectedPath.StartsWith("gdbp://", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("未获取有效 GDB 路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    string host;
                    string path;
                    ParseGdbSelectedPath(fullSelectedPath, out host, out path);
                    if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(path) || path == "/")
                    {
                        MessageBox.Show("请选择有效的数据库或目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    selectedDataSource = host;
                    selectedGdbDirectory = path;
                    UpdateGdbPathDisplay();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("解析 GDB 路径出错：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            string tiffPath = (txtTiffPath.Text ?? string.Empty).Trim();
            string className = (txtClassName.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(tiffPath))
            {
                MessageBox.Show("请选择 TIFF 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTiffPath.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedDataSource) || string.IsNullOrWhiteSpace(selectedGdbDirectory) || selectedGdbDirectory == "/")
            {
                MessageBox.Show("请选择 MapGIS 数据库。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!service.IsValidClassName(className))
            {
                MessageBox.Show("输出点类名必须以字母开头，只能包含字母、数字和下划线。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtClassName.Focus();
                return;
            }

            try
            {
                if (currentInfo == null || !string.Equals(tiffPath, txtTiffPath.Tag as string, StringComparison.OrdinalIgnoreCase))
                {
                    LoadRasterInfo(tiffPath);
                }

                SetExecutingState(true);
                lblStatus.Text = "正在执行栅格转点，请稍候...";
                System.Windows.Forms.Application.DoEvents();

                GravityRasterToPointResult result = service.ImportToDatabase(tiffPath, selectedDataSource, selectedGdbDirectory, className);
                lblStatus.Text = "栅格转点完成。";

                MessageBox.Show(
                    "栅格转点完成。\r\n" +
                    "数据库：" + result.DatabasePath + "\r\n" +
                    "输出类：" + result.ClassName + "\r\n" +
                    "类路径：" + result.ClassUrl + "\r\n" +
                    "成功写入：" + result.SuccessCount + "\r\n" +
                    "跳过 NoData：" + result.SkipNoDataCount + "\r\n" +
                    "跳过无效像元：" + result.SkipInvalidCount,
                    "完成",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "执行失败。";
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetExecutingState(false);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtTiffPath_TextChanged(object sender, EventArgs e)
        {
            currentInfo = null;
            txtTiffPath.Tag = txtTiffPath.Text;
            ResetInfoLabels();
        }

        private void LoadRasterInfo(string tiffPath)
        {
            currentInfo = service.ReadRasterInfo(tiffPath);
            txtTiffPath.Tag = tiffPath;

            lblSizeValue.Text = currentInfo.Width + " × " + currentInfo.Height;
            lblBandValue.Text = currentInfo.BandCount.ToString();
            lblCellSizeValue.Text = currentInfo.CellSizeX.ToString("0.########") + ", " + currentInfo.CellSizeY.ToString("0.########");
            lblRangeValue.Text =
                currentInfo.MapRange.XMin.ToString("0.########") + ", " +
                currentInfo.MapRange.YMin.ToString("0.########") + " - " +
                currentInfo.MapRange.XMax.ToString("0.########") + ", " +
                currentInfo.MapRange.YMax.ToString("0.########");
            lblNoDataValue.Text = currentInfo.HasNoData ? currentInfo.NoDataValue.ToString("0.########") : "无";
            lblSrefValue.Text = string.IsNullOrWhiteSpace(currentInfo.SpatialReferenceName) ? "未知" : currentInfo.SpatialReferenceName;
            lblStatus.Text = "TIFF 信息读取完成，转点时默认使用第 1 波段。";

            if (string.IsNullOrWhiteSpace(txtClassName.Text))
            {
                txtClassName.Text = service.BuildDefaultClassName(tiffPath);
            }
        }

        private void ResetInfoLabels()
        {
            lblSizeValue.Text = "-";
            lblBandValue.Text = "-";
            lblCellSizeValue.Text = "-";
            lblRangeValue.Text = "-";
            lblNoDataValue.Text = "-";
            lblSrefValue.Text = "-";
            lblStatus.Text = "请选择 TIFF 文件和目标数据库。";
        }

        private void UpdateGdbPathDisplay()
        {
            txtDatabasePath.Text = selectedDataSource + ":" + selectedGdbDirectory;
        }

        private static void ParseGdbSelectedPath(string fullSelectedPath, out string host, out string path)
        {
            string raw = fullSelectedPath.Substring("gdbp://".Length);
            int slashIndex = raw.IndexOf('/');
            if (slashIndex < 0)
            {
                host = raw;
                path = "/";
                return;
            }

            host = raw.Substring(0, slashIndex);
            path = raw.Substring(slashIndex);
        }

        private void SetExecutingState(bool executing)
        {
            txtTiffPath.Enabled = !executing;
            btnBrowseTiff.Enabled = !executing;
            txtDatabasePath.Enabled = false;
            btnBrowseDatabase.Enabled = !executing;
            txtClassName.Enabled = !executing;
            btnExecute.Enabled = !executing;
            btnCancel.Enabled = !executing;
        }
    }
}
