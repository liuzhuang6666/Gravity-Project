// VisualizationCommand.cs
using MapGIS.PluginEngine;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.GeoMap;

namespace MapGISPlugin3
{
    public class VisualizationCommand : ICommand
    {
        private IApplication m_hook;

        public Bitmap Bitmap => null;
        public string Caption => "等值线可视化";
        public string Category => "重磁处理"; // 与位场转换在同一个分类下
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => "对指定的栅格图层进行等值线可视化";
        public string Name => "StandaloneVisualizationCommand";
        public string Tooltip => "等值线可视化";

        public void OnCreate(IApplication hook)
        {
            m_hook = hook;
        }

        public void OnClick()
        {
            if (m_hook == null || m_hook.Document == null)
            {
                MessageBox.Show("插件未能正确初始化或无法获取主文档。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 1. 弹出图层选择对话框
            using (var form = new VisualizationForm(m_hook))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RasterLayer selectedLayer = form.SelectedRasterLayer;
                    Map targetMap = form.SelectedMap;

                    // 【新增】从窗口获取用户输入的间隔值
                    decimal interval = form.ContourInterval;

                    if (selectedLayer == null || targetMap == null)
                    {
                        MessageBox.Show("未能获取有效的图层或地图。", "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        var helper = new VisualizationHelper();
                        // 【修改】调用 PerformVisualization 时，传入间隔值
                        helper.PerformVisualization(selectedLayer, targetMap, m_hook, (double)interval);

                        // 【已修正】使用正确的 MessageBoxButtons 和 MessageBoxIcon 枚举
                        MessageBox.Show($"图层 '{selectedLayer.Name}' 的可视化结果已成功添加到主地图。", "操作完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"可视化过程中发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}