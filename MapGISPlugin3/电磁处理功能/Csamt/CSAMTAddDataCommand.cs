using System;
using System.Drawing; // 用于 Bitmap
using System.Windows.Forms; // 用于 MessageBox
using MapGIS.PluginEngine; // 包含 ICommand, IApplication
using MapGIS.GeoMap; // 包含 Map, Maps, Document
using MapGIS.UI.Controls; // 可能需要，如果 Form_CSAMT_Import 放在这里
using System.Runtime.InteropServices;

namespace MapGISPlugin3 // 确保这个命名空间与你的项目一致
{
    /// <summary>
    /// 用于启动 CSAMT 数据导入窗体的插件入口命令。
    /// MapGIS 平台会将其识别为一个功能按钮或菜单项。
    /// </summary>
    public class CSAMTImportCommand : ICommand
    {
        // 用于存储从 MapGIS 主程序传递过来的 IApplication 钩子
        private IApplication _hk;

        #region ICommand 接口成员实现
        /// <summary>
        /// 按钮的图标（返回 null 表示使用默认图标或无图标）
        /// 你可以替换为 Resources.YourIcon 等
        /// </summary>
        public Bitmap Bitmap { get { return null; } }

        /// <summary>
        /// 功能的标题，会显示在按钮或菜单上
        /// </summary>
        public string Caption { get { return "CSAMT 数据导入"; } } // 修改为具体功能名称

        /// <summary>
        /// 功能所属的类别，用于在自定义界面时进行分组
        /// </summary>
        public string Category { get { return "地学数据处理"; } } // 可以自定义类别名称

        /// <summary>
        /// 对于可切换状态的按钮，表示是否被选中（固定为 false）
        /// </summary>
        public bool Checked { get { return false; } }

        /// <summary>
        /// 表示按钮是否可用（可以根据条件动态设置，这里简单设为 true）
        /// </summary>
        public bool Enabled
        {
            // 更好的做法是检查当前是否有打开的文档和地图
            // return _hk != null && _hk.Document != null && _hk.Document.GetMaps() != null && _hk.Document.GetMaps().Count > 0;
            get { return true; } // 暂时设为 true
        }

        /// <summary>
        /// 当鼠标悬停在功能上时，在状态栏显示的详细信息
        /// </summary>
        public string Message { get { return "导入控制源音频大地电磁测深数据 (.dat) 到 MapGIS 地理数据库"; } }

        /// <summary>
        /// 功能的唯一编程名称
        /// </summary>
        public string Name { get { return "CSAMTImportCommand"; } } // 保持唯一性

        /// <summary>
        /// 鼠标悬停时显示的工具提示
        /// </summary>
        public string Tooltip { get { return "导入 CSAMT 数据"; } }

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
                MessageBox.Show("请先打开一个地图文档。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Maps maps = null; // 不再需要在 finally 中释放，可以在 try 内部声明
            Map targetMap = null;

            try
            {
                Maps maps = currentDoc.GetMaps(); // 在 try 内部获取
                if (maps == null || maps.Count == 0)
                {
                    MessageBox.Show("当前文档中没有找到地图。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // 注意：这里没有获取到 maps，所以不需要释放
                    return;
                }

                targetMap = maps.GetMap(0); // 获取第一个地图
                if (targetMap == null)
                {
                    MessageBox.Show("获取地图对象失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // 注意：获取 targetMap 失败，但 maps 对象仍然存在，但我们不再手动释放它
                    return;
                }

                // --- 【移除】不再需要手动释放 maps ---
                // if (maps != null)
                // {
                // Marshal.ReleaseComObject(maps); // <--- 删除这一行
                // maps = null;
                // }
                // ------------------------------------

                // 创建并显示我们的导入窗体
                using (Form_CSAMTAddData importForm = new Form_CSAMTAddData(_hk))
                {
                    importForm.ShowDialog();
                }
            }
            catch (COMException comEx) // 优先捕获 COM 异常
            {
                MessageBox.Show($"MapGIS 操作时出错: {comEx.Message}\n错误码: {comEx.ErrorCode}", "COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"OnClick COM 异常: {comEx}");
            }
            catch (Exception ex) // 捕获其他 C# 异常
            {
                MessageBox.Show($"打开 CSAMT 数据导入窗口或获取地图时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"OnClick 异常: {ex}");
            }
            finally
            {
                // --- 【移除】不再需要在 finally 中释放 maps ---
                // if (maps != null)
                // {
                // try { Marshal.ReleaseComObject(maps); } catch { }
                // }
                // ---------------------------------------
                // targetMap 通常也不需要在这里释放，它的生命周期应由 Document 或 Form_CSAMT_Import 管理
                // if (targetMap != null) { try { Marshal.ReleaseComObject(targetMap); } catch { } }
            }
        }

        /// <summary>
        /// 当插件被 MapGIS 加载时，此方法会被调用
        /// </summary>
        /// <param name="hook">MapGIS主程序的IApplication接口实例</param>
        public void OnCreate(IApplication hook)
        {
            // 将传递进来的钩子保存到私有变量 _hk 中，以备 OnClick 时使用
            this._hk = hook;
            Console.WriteLine("CSAMTImportCommand 已创建并获取到 IApplication hook。"); // 添加日志
        }
        #endregion
    }
}