// ContourVisualizationCommand.cs
using System;
using MapGIS.PluginEngine;
using System.Drawing;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public class ContourVisualizationCommand : ICommand
    {
        private IApplication m_hook;

        #region ICommand 成员

        public Bitmap Bitmap { get { return null; } } // 您可以为按钮指定一个图标
        public string Caption { get { return "等值线可视化"; } }
        public string Category { get { return "重磁处理"; } } // 与之前的插件放在同一分类下
        public bool Checked { get { return false; } }
        public bool Enabled { get { return true; } }
        public string Message { get { return "对栅格图层进行等值线可视化分析"; } }
        public string Name { get { return "ContourVisualizationCommand"; } }
        public string Tooltip { get { return "等值线可视化"; } }

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
            if (this.m_hook == null || this.m_hook.Document == null)
            {
                MessageBox.Show("插件未能正确初始化或当前无打开的地图文档！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 创建新的功能窗体实例，并将钩子传递进去
            using (Form_ContourVisualization form = new Form_ContourVisualization(this.m_hook))
            {
                // 以模态方式显示窗体
                form.ShowDialog();
            }
        }

        #endregion
    }
}