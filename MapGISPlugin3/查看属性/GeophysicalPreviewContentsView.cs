using System;
using System.Drawing;
using System.Windows.Forms; // (重要) 确保此 using 存在，为了 'Control'
using MapGIS.PluginEngine;
using MapGIS.GeoMap;

namespace MapGISPlugin3
{
    /// <summary>
    /// (最终修正版) 严格按照您提供的 IContentsView API 实现
    /// </summary>
    public class GeophysicalPreviewContentsView : IContentsView
    {
        private IApplication m_app;
        private GeophysicalPreviewControl m_control; // 承载的自定义控件

        // --- 1. 自定义公共方法 (供 Command 调用) ---
        // (这个方法不变)
        public void UpdateContent(MapLayer layer)
        {
            if (m_control != null)
            {
                m_control.Initialize(layer);
            }
        }

        // --- 2. IPlugin 接口成员 ---
        // (IContentsView 继承自 IPlugin, IPlugin 的成员也必须实现)

        public string Name
        {
            // API: string Name { get; }
            // (这个 Name 必须和 .dcprj 设计器中注册的一致)
            get { return "MapGISPlugin3.GeophysicalPreviewContentsView"; }
        }

        public string Caption
        {
            // API: string Caption { get; }
            get { return "地球物理预览"; }
        }

        public void OnCreate(IApplication hook) // API: OnCreate(IApplication app)
        {
            m_app = hook;
            m_control = new GeophysicalPreviewControl(); // 创建您的 UserControl
        }

        // (IPlugin 接口通常还有一个 OnDestroy)
        public void OnDestroy()
        {
            if (m_control != null)
            {
                m_control.Dispose();
                m_control = null;
            }
            m_app = null;
        }

        public Bitmap Bitmap
        {
            // API: Bitmap Bitmap { get; }
            get { return null; } // 标签页图标 (可选)
        }

        // --- 3. IContentsView 接口成员 (严格匹配 API) ---

        /// <summary>
        /// (CS0535 修正) API: bool ControlBox { get; }
        /// </summary>
        public bool ControlBox
        {
            get { return true; } // true = 显示 'x' 关闭按钮
        }

        /// <summary>
        /// (CS0535 修正) API: bool InitCreate { get; }
        /// </summary>
        public bool InitCreate
        {
            get { return false; } // false = 启动时不加载
        }

        /// <summary>
        /// (CS0535 修正) API: Control ObjecthWnd { get; }
        /// (类型是 Control, 不是 IntPtr)
        /// </summary>
        public Control ObjecthWnd
        {
            get
            {
                // m_control (GeophysicalPreviewControl) 继承自 UserControl, 
                // UserControl 继承自 Control, 类型匹配。
                return m_control;
            }
        }

        /// <summary>
        /// (CS0535 修正) API: void OnActive(bool isActive)
        /// </summary>
        public void OnActive(bool isActive)
        {
            // 可以在这里添加激活/失活时的逻辑
        }

        /// <summary>
        /// (CS0535 修正) API: void OnClose()
        /// (返回类型是 void, 不是 bool)
        /// </summary>
        public void OnClose()
        {
            // 标签页关闭时触发的逻辑
            // OnDestroy 会在之后被调用
        }
    }
}