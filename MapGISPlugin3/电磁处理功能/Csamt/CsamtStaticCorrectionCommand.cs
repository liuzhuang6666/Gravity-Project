// 这是通用模板，请复制到每个新文件中并修改
using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;

namespace MapGISPlugin3 // <--- 确保命名空间是这个！
{
    // 1. 修改类名
    public class CsamtStaticCorrectionCommand : ICommand
    {
        private IApplication _hk;

        #region ICommand 成员
        public Bitmap Bitmap => null;

        // 2. 修改Caption
        public string Caption => "滤波静校正";

        public string Category => "电磁数据处理";
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => $"执行 {Caption} 功能";

        // 3. 修改Name
        public string Name => "CsamtStaticCorrectionCommand";

        public string Tooltip => $"执行 {Caption} 功能";

        public void OnClick()
        {
            MessageBox.Show(Caption + " 功能待实现。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void OnCreate(IApplication hook)
        {
            this._hk = hook;
        }
        #endregion
    }
}