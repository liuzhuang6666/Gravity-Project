using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 追加观测数据命令
    /// </summary>
    public class TEMAppendDataCommand : ICommand
    {
        private IApplication _hk;

        #region ICommand 接口成员实现

        public Bitmap Bitmap { get { return null; } }
        public string Caption { get { return "TEM 追加观测数据"; } }
        public string Category { get { return "地学数据处理"; } }
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "向已有TEM数据追加新的观测数据"; } }
        public string Name { get { return "TEMAppendDataCommand"; } }
        public string Tooltip { get { return "追加 TEM 观测数据"; } }

        public void OnClick()
        {
            if (_hk == null)
            {
                MessageBox.Show("未能获取到 MapGIS 应用程序实例！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Document currentDoc = _hk.Document;
            if (currentDoc == null)
            {
                MessageBox.Show("请先打开一个地图文档。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Maps maps = currentDoc.GetMaps();
                if (maps == null || maps.Count == 0)
                {
                    MessageBox.Show("当前文档中没有地图，请先导入TEM数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (Form_TEMAppendData appendForm = new Form_TEMAppendData(_hk))
                {
                    appendForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开追加数据窗口时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"TEMAppendDataCommand 异常: {ex}");
            }
        }

        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
            Console.WriteLine("TEMAppendDataCommand 已创建。");
        }

        #endregion
    }
}