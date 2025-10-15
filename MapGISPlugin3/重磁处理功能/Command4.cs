using System;
using System.Drawing;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class Command4 : ICommand
    {
        private IApplication _hk;

        #region ICommand成员
        public Bitmap Bitmap { get { return null; } }
        public string Caption { get { return "重磁数据导入"; } }
        public string Category { get { return "MapGISPlugin3"; } }
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "用于导入GRD格式的重磁栅格数据"; } }
        public string Name { get { return "ImportGravityMagneticData"; } }
        public string Tooltip { get { return "重磁数据导入"; } }

        public void OnClick()
        {
            if (_hk != null)
            {
                // *** 修正点 1 ***
                // 直接将 IApplication 接口 (_hk) 传递给窗体
                // 因为ActiveView和Document都在它里面
                new Form_GravityMagnetic(_hk).ShowDialog();
            }
        }

        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
        }
        #endregion
    }
}