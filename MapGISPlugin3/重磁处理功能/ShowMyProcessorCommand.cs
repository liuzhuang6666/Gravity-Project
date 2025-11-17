using MapGIS.PluginEngine;
using System.Windows.Forms;
using System.Drawing; // 需要添加这个 using 语句来使用 Bitmap

namespace MapGISPlugin3
{
    /// <summary>
    /// 这个类就是连接MapGIS按钮和你的Form1窗体的“桥梁”
    /// </summary>
    public class ShowMyProcessorCommand : ICommand
    {
        private IApplication m_hook;

        public void OnClick()
        {
            if (m_hook == null) return;

            FieldTransformForm myForm = new FieldTransformForm(m_hook);
            myForm.ShowDialog();
        }

        public void OnCreate(IApplication hook)
        {
            m_hook = hook;
        }

        public string Name
        {
            get { return "ShowMyProcessorCommand"; }
        }

        public string Caption
        {
            get { return "地球物理数据处理"; }
        }

        // 修正：将 ToolTip 改为 Tooltip
        public string Tooltip
        {
            get { return "打开地球物理数据处理与可视化窗口"; }
        }

        public string Category
        {
            get { return "数据处理工具"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public bool Checked
        {
            get { return false; }
        }

        // 新增：实现 Bitmap 属性
        /// <summary>
        /// 按钮上显示的图标。如果没有图标，返回 null。
        /// 你可以从项目资源中加载一个 16x16 的 bmp 或 png 图片。
        /// </summary>
        public Bitmap Bitmap
        {
            // 如果你没有为按钮准备图标，直接返回 null 即可
            get { return null; }
        }

        // 新增：实现 Message 属性
        /// <summary>
        /// 显示在状态栏的消息
        /// </summary>
        public string Message
        {
            get { return "启动地球物理数据处理工具..."; }
        }

        public string HelpFile
        {
            get { return ""; }
        }

        public int HelpContextID
        {
            get { return 0; }
        }
    }
}