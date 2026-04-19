using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.GeoMap;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class CommandClickProbeCommand : ICommand
    {
        private IApplication _hk;

        public Bitmap Bitmap { get { return null; } }
        public string Caption { get { return "命令点击测试"; } }
        public string Category { get { return "重磁处理"; } }
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "用于测试按钮点击是否进入 ICommand.OnClick"; } }
        public string Name { get { return "CommandClickProbeCommand"; } }
        public string Tooltip { get { return "命令点击测试"; } }

        public void OnClick()
        {
            if (_hk == null)
            {
                MessageBox.Show("未能获取到 MapGIS 应用程序实例。", "测试命令", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Document currentDoc = _hk.Document;
            if (currentDoc == null)
            {
                MessageBox.Show("测试命令已进入 OnClick，但当前没有打开地图文档。", "测试命令", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                MessageBox.Show("测试命令已进入 OnClick。", "测试命令", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (CommandClickProbeForm form = new CommandClickProbeForm(_hk))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("测试窗口打开失败: " + ex.Message, "测试命令", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnCreate(IApplication hook)
        {
            _hk = hook;
        }
    }
}
