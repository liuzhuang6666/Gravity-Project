using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class Gravity3DInversionCommand : ICommand
    {
        private IApplication _hk;

        public Bitmap Bitmap { get { return null; } }
        public string Caption { get { return "重力三维反演"; } }
        public string Category { get { return "地学算法"; } }
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "执行重力异常数据的三维反演"; } }
        public string Name { get { return "Gravity3DInversionCommand"; } }
        public string Tooltip { get { return "重力三维反演"; } }

        public void OnClick()
        {
            if (_hk != null)
            {
                Form_Gravity3DInversion form = new Form_Gravity3DInversion(_hk);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("未能获取MapGIS环境。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
        }
    }
}