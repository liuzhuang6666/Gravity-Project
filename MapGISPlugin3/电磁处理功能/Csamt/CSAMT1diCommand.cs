using System;
using System.Drawing; // 用于 Bitmap
using System.Windows.Forms; // 用于 MessageBox
using MapGIS.PluginEngine; // 包含 ICommand, IApplication
using MapGIS.GeoMap; // 包含 Map, Maps, Document
using System.Runtime.InteropServices; // 用于 COM 异常

namespace MapGISPlugin3 // 确保这个命名空间与你的项目一致
{
    /// <summary>
    /// 用于启动 CSAMT 一维反演处理窗体 (Form_CSAMT1di) 的命令类
    /// </summary>
    public class CSAMT1diCommand : ICommand // 直接实现 ICommand 接口
    {
        // 用于存储从 MapGIS 主程序传递过来的 IApplication 钩子
        private IApplication _hk;

        #region ICommand 接口成员实现
        /// <summary>
        /// 按钮的图标（返回 null 表示使用默认图标或无图标）
        /// </summary>
        public Bitmap Bitmap { get { return null; } }

        /// <summary>
        /// 功能的标题，会显示在按钮或菜单上
        /// </summary>
        public string Caption { get { return "CSAMT 一维反演"; } } // 修改为新功能的名称

        /// <summary>
        /// 功能所属的类别，用于在自定义界面时进行分组
        /// </summary>
        public string Category { get { return "电磁处理"; } } // 保持或修改类别

        /// <summary>
        /// 对于可切换状态的按钮，表示是否被选中（固定为 false）
        /// </summary>
        public bool Checked { get { return false; } }

        /// <summary>
        /// 表示按钮是否可用
        /// </summary>
        public bool Enabled
        {
            get
            {
                return true; // 暂时设为 true
            }
        }

        /// <summary>
        /// 当鼠标悬停在功能上时，在状态栏显示的详细信息
        /// </summary>
        public string Message { get { return "打开可控源音频大地电磁法一维反演处理界面"; } } // 修改描述

        /// <summary>
        /// 功能的唯一编程名称
        /// </summary>
        public string Name { get { return "CSAMT1diCommand"; } } // 新的唯一名称

        /// <summary>
        /// 鼠标悬停时显示的工具提示
        /// </summary>
        public string Tooltip { get { return "CSAMT 一维反演处理"; } } // 修改提示

        /// <summary>
        /// 【核心】当用户点击此功能按钮时执行的代码
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

            // 检查是否存在 "电法数据" 地图，这是 Form_CSAMT1di 运行的前提
            Map electroMap = FindMapByName("电法数据");
            if (electroMap == null)
            {
                MessageBox.Show("未在当前工程中找到名为 '电法数据' 的地图。\n请先使用 'MT 数据导入' 功能导入数据或检查地图名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // *** 关键：调用新的 Form_CSAMT1di 窗体 ***
                using (Form_CSAMT1di processForm = new Form_CSAMT1di(_hk))
                {
                    processForm.ShowDialog(); // 以模态方式显示窗体
                }
            }
            catch (COMException comEx) // 捕获 MapGIS 操作可能抛出的 COM 异常
            {
                MessageBox.Show($"MapGIS 操作时出错: {comEx.Message}\n错误码: {comEx.ErrorCode}", "COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"CSAMT1diCommand OnClick COM 异常: {comEx}");
            }
            catch (Exception ex) // 捕获其他 C# 异常
            {
                MessageBox.Show($"打开 CSAMT 一维反演处理窗口时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"CSAMT1diCommand OnClick 异常: {ex}");
            }
            finally
            {
                // if (electroMap != null) { try { Marshal.ReleaseComObject(electroMap); } catch { } }
            }
        }

        /// <summary>
        /// 当插件被 MapGIS 加载时，此方法会被调用
        /// </summary>
        /// <param name="hook">MapGIS主程序的IApplication接口实例</param>
        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
        }
        #endregion

        /// <summary>
        /// (辅助函数) 根据名称查找地图对象 - 复用逻辑
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
                            return currentMap; // 找到了，返回它
                        }
                    }
                    catch (COMException comEx)
                    {
                        Console.WriteLine($"FindMapByName - 获取地图 {i} 时发生 COM 错误: {comEx.Message}");
                    }
                    finally
                    {
                        // 不释放 currentMap
                    }
                }
            }
            catch (COMException comEx)
            {
                Console.WriteLine($"FindMapByName - 获取 Maps 集合时发生 COM 错误: {comEx.Message}");
            }
            finally
            {
                // 不释放 maps
            }

            return null; // 循环结束都没找到
        }
    } // End Class CSAMT1diCommand
} // End Namespace