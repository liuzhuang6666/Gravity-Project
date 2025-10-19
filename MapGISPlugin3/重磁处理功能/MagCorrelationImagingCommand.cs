using System;
using System.Drawing;
using System.Windows.Forms; // 确保引用了该命名空间
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    /// <summary>
    /// 这是“磁相关成像”功能的插件入口命令。
    /// MapGIS平台会识别这个类，并将其作为一个功能按钮或菜单项。
    /// </summary>
    public class MagCorrelationImagingCommand : ICommand
    {
        // 用于存储从MapGIS主程序传递过来的IApplication钩子
        private IApplication _hk;

        #region ICommand 接口成员实现

        /// <summary>
        /// 按钮的图标（这里暂时不提供，返回null）
        /// </summary>
        public Bitmap Bitmap { get { return null; } }

        /// <summary>
        /// 功能的标题，会显示在按钮或菜单上
        /// </summary>
        public string Caption { get { return "磁相关成像"; } }

        /// <summary>
        /// 功能所属的类别，用于在自定义界面时进行分组
        /// </summary>
        public string Category { get { return "地学算法"; } } // 您可以自定义，例如 "MapGISPlugin3"

        /// <summary>
        /// 对于可切换状态的按钮，表示是否被选中（这里固定为false）
        /// </summary>
        public bool Checked { get { return false; } }

        /// <summary>
        /// 表示按钮是否可用（这里固定为true）
        /// </summary>
        public bool Enabled { get { return true; } }

        /// <summary>
        /// 当鼠标悬停在功能上时，在状态栏显示的详细信息
        /// </summary>
        public string Message { get { return "执行磁化方向和地磁场方向的互相关成像算法"; } }

        /// <summary>
        /// 功能的唯一编程名称
        /// </summary>
        public string Name { get { return "MagCorrelationImagingCommand"; } }

        /// <summary>
        /// 鼠标悬停时显示的工具提示
        /// </summary>
        public string Tooltip { get { return "磁相关成像"; } }

        /// <summary>
        /// 【核心】当用户点击此功能按钮时执行的代码
        /// </summary>
        public void OnClick()
        {
            // 确保已经成功获取到了MapGIS应用程序的钩子
            if (_hk != null)
            {
                // 创建我们之前修改好的窗体实例
                // 并将钩子 _hk 传递给窗体的构造函数
                Form_MagCorrelationImaging form = new Form_MagCorrelationImaging(_hk);

                // 以模态对话框的形式显示窗体
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("未能获取到MapGIS应用程序实例！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 当插件被MapGIS加载时，此方法会被调用
        /// </summary>
        /// <param name="hook">MapGIS主程序的IApplication接口实例</param>
        public void OnCreate(IApplication hook)
        {
            // 将传递进来的钩子保存到私有变量 _hk 中，以备OnClick时使用
            this._hk = hook;
        }

        #endregion
    }
}