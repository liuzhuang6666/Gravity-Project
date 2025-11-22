using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using System.Runtime.InteropServices;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 数据处理命令
    /// 【复用】整体逻辑参考 MT1diCommand
    /// 【修改】改为启动 Form_TEMProcess，查找 "TEM数据" 地图
    /// </summary>
    public class TEMProcessCommand : ICommand
    {
        private IApplication _hk;

        #region ICommand 接口实现

        public Bitmap Bitmap { get { return null; } }

        public string Caption { get { return "TEM 一维反演"; } }

        public string Category { get { return "天然电磁处理"; } }

        public bool Checked { get { return false; } }

        public bool Enabled { get { return true; } }

        public string Message { get { return "打开瞬变电磁反演处理界面"; } }

        public string Name { get { return "TEMProcessCommand"; } }

        public string Tooltip { get { return "TEM 一维反演"; } }

        /// <summary>
        /// 【复用】OnClick 逻辑与 MT1diCommand 相同
        /// 【修改】查找 "TEM数据" 地图，启动 Form_TEMProcess
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
                MessageBox.Show("请先打开或创建一个工程。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Map temMap = FindMapByName("电法数据");
            if (temMap == null)
            {
                MessageBox.Show("未在当前工程中找到名为 '电法数据' 的地图。\n请先使用 'TEM 数据导入' 功能导入数据或检查地图名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 【修改】启动 Form_TEMProcess（而不是 Form_MT1di）
                using (Form_TEMProcess processForm = new Form_TEMProcess(_hk))
                {
                    processForm.ShowDialog();
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"MapGIS 操作时出错: {comEx.Message}\n错误码: {comEx.ErrorCode}", "COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"TEMProcessCommand OnClick COM 异常: {comEx}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开 TEM 反演处理窗口时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"TEMProcessCommand OnClick 异常: {ex}");
            }
        }

        /// <summary>
        /// 【复用】插件初始化，保存 hook
        /// </summary>
        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
            Console.WriteLine("TEMProcessCommand 已创建并获取到 IApplication hook。");
        }

        #endregion

        /// <summary>
        /// 【复用】根据名称查找地图，逻辑与 MT1diCommand.FindMapByName 相同
        /// </summary>
        private Map FindMapByName(string mapName)
        {
            if (_hk == null || _hk.Document == null) return null;

            Maps maps = null;
            try
            {
                maps = _hk.Document.GetMaps();
                if (maps == null) return null;

                for (int i = 0; i < maps.Count; i++)
                {
                    Map currentMap = null;
                    try
                    {
                        currentMap = maps.GetMap(i);
                        if (currentMap != null && currentMap.Name == mapName)
                        {
                            return currentMap;
                        }
                    }
                    catch (COMException comEx)
                    {
                        Console.WriteLine($"FindMapByName - 获取地图 {i} 时发生 COM 错误: {comEx.Message}");
                    }
                }
            }
            catch (COMException comEx)
            {
                Console.WriteLine($"FindMapByName - 获取 Maps 集合时发生 COM 错误: {comEx.Message}");
            }

            return null;
        }
    }
}