using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using System.Runtime.InteropServices;

namespace MapGISPlugin3
{
    /// <summary>
    /// 用于启动“曲面延拓”窗体的命令
    /// </summary>
    public class SurfaceContinCommand : ICommand
    {
        private IApplication _hk;

        public Bitmap Bitmap { get { return null; } }
        public string Caption { get { return "曲面延拓"; } }
        public string Category { get { return "地磁处理"; } }
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "使用外部 a.exe 执行曲面延拓"; } }
        public string Name { get { return "SurfaceContinCommand"; } }
        public string Tooltip { get { return "曲面延拓"; } }

        public void OnClick()
        {
            if (_hk == null || _hk.Document == null)
            {
                MessageBox.Show("请先打开一个地图文档。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 创建并显示新的窗体
                using (Form_SurfaceContin continForm = new Form_SurfaceContin(_hk))
                {
                    continForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开窗体时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
        }
    }
}