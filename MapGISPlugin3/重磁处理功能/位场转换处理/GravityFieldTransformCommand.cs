// GravityFieldTransformCommand.cs
using System;
using System.Drawing;
using System.Windows.Forms; // 确保引用了 MessageBox
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class GravityFieldTransformCommand : ICommand
    {
        // 成员变量，用于保存从OnCreate接收的hook
        private IApplication m_hook;
        #region ICommand 成员
        public Bitmap Bitmap { get { return null; } }
        public string Caption { get { return "重力位场转换"; } }
        public string Category { get { return "重磁处理"; } } // 分类可以自定义
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "执行重力位场转换处理功能"; } }
        public string Name { get { return "GravityFieldTransformCommand"; } } // 建议使用英文名
        public string Tooltip { get { return "重力位场转换"; } }
        /// <summary>
        /// 当插件被MapGIS加载时调用，用于初始化
        /// </summary>
        public void OnCreate(IApplication hook)
        {
            // 保存主程序的钩子，以便OnClick时使用
            this.m_hook = hook;
        }
        /// <summary>
        /// 当用户点击按钮时执行
        /// </summary>
        public void OnClick()
        {
            // 检查钩子是否有效
            if (this.m_hook == null)
            {
                MessageBox.Show("插件未能正确初始化，无法与主程序通信！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 创建主窗体实例，并将钩子传递进去
            using (GravityFieldTransformForm form = new GravityFieldTransformForm(this.m_hook))
            {
                // 使用 ShowDialog() 以模态方式显示窗体
                // 这会暂停用户与MapGIS主界面的交互，直到此窗体关闭
                // 这是工具类窗体的标准做法
                form.ShowDialog();
            }
        }
        #endregion
    }
}