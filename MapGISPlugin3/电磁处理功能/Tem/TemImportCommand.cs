using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using System.Runtime.InteropServices;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 数据导入命令
    /// 【复用】整体逻辑参考 MTImportCommand
    /// 【修改】改为启动 Form_TEMImport，菜单分类改为"天然电磁处理"
    /// </summary>
    public class TEMImportCommand : ICommand
    {
        private IApplication _hk;

        #region ICommand 接口实现

        public Bitmap Bitmap { get { return null; } }

        public string Caption { get { return "TEM 数据导入"; } }

        public string Category { get { return "天然电磁处理"; } }

        public bool Checked { get { return false; } }

        public bool Enabled { get { return true; } }

        public string Message { get { return "导入瞬变电磁测深数据（观测数据和发射源信息）"; } }

        public string Name { get { return "TEMImportCommand"; } }

        public string Tooltip { get { return "导入 TEM 数据"; } }

        /// <summary>
        /// 【复用】OnClick 逻辑与 MTImportCommand 相同
        /// 【修改】启动 Form_TEMImport，创建 "TEM数据" 地图
        /// </summary>
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
                    MessageBox.Show("当前文档中没有找到地图。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Map targetMap = maps.GetMap(0);
                if (targetMap == null)
                {
                    MessageBox.Show("获取地图对象失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 【修改】启动 Form_TEMImport（而不是 Form_MTAddData）
                using (Form_TEMImport importForm = new Form_TEMImport(_hk))
                {
                    importForm.ShowDialog();
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"MapGIS 操作时出错: {comEx.Message}\n错误码: {comEx.ErrorCode}", "COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"TEMImportCommand OnClick COM 异常: {comEx}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开 TEM 数据导入窗口时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"TEMImportCommand OnClick 异常: {ex}");
            }
        }

        /// <summary>
        /// 【复用】插件初始化，保存 hook
        /// </summary>
        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
            Console.WriteLine("TEMImportCommand 已创建并获取到 IApplication hook。");
        }

        #endregion
    }
}