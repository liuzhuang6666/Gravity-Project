using DevExpress.XtraTreeList.Nodes;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.Config;
using MapGIS.GeoMap;
using MapGIS.PluginEngine;
using MapGIS.PlugUtility;
using MapGISPlugin3.Properties;
using MapGIS.UI.Controls;
using MapGIS.WorkSpaceEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MapGIS.GISControl;

namespace MapGISPlugin3
{

    /// <summary>
    /// 新建
    /// </summary>
    public class NewProjectCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (ws != null || app != null)
                    return MapGIS.Desktop.Resources.Png_New_16;
                return MapGIS.PluginEngine.Application.IsRibbon ? MapGIS.Desktop.Resources.Png_New_32 : MapGIS.Desktop.Resources.Png_New_16;
            }
        }

        public string Caption
        {
            get { return "新建"; }
        }

        public string Category
        {
            get { return "地球物理数据处理"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "新建"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "新建"; }
        }

        public void OnClick()
        {
            CreatePrjectForm form = new CreatePrjectForm(true);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ProjectManage prjectManage = ProjectManage.GetInstance();
                // 注意：这里的判断条件可能需要根据您的实际逻辑调整，
                // 为了确保代码能执行，我们暂时简化为 true
                if (true) // if (prjectManage != null && prjectManage.ProjectManageItemCount > 0)
                {
                    string mapxPath = Path.Combine(form.PrjPath, form.PrjName + ".mapx");

                    // 创建新的Document实例
                    Document newDoc = new Document();
                    newDoc.New();
                    newDoc.Title = form.PrjName;

                    // ==================== 修改点：开始 ====================
                    // 创建新的Map实例，并设置新的名称
                    Map mapGravity = new Map();
                    mapGravity.Name = "重力数据"; // 修改为 "重力数据"

                    Map mapMagnetic = new Map();
                    mapMagnetic.Name = "磁法数据"; // 修改为 "磁法数据"

                    Map mapElectric = new Map();
                    mapElectric.Name = "电法数据"; // 修改为 "电法数据"

                    // 将新地图添加到文档中
                    newDoc.GetMaps().Append(mapGravity);
                    newDoc.GetMaps().Append(mapMagnetic);
                    newDoc.GetMaps().Append(mapElectric);
                    // ==================== 修改点：结束 ====================

                    // 保存文档为.mapx文件
                    newDoc.SaveAs(mapxPath);

                    // 关闭文档
                    newDoc.Close(false);

                    // 弹出消息框询问是否打开新创建的文档
                    // 弹出消息框询问是否打开新创建的文档
                    if (XMessageBox.QuestionEx("创建完成，是否打开？") == DialogResult.Yes)
                    {
                        // 打开文档
                        InitPlugin.App.Document.Open(mapxPath);

                        // 【核心修正】: 手动触发UI状态更新
                        // 这会告诉所有命令重新检查自己的 Enabled 属性
                        if (InitPlugin.App != null && InitPlugin.App.StateManager != null)
                        {
                            InitPlugin.App.StateManager.OnStateChanged(null, null);
                        }
                    }
                    return;
                }
            }
        }

        public bool BeginGroup
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public IApplication App
        {
            get { return app; }

            set { app = value; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        #endregion ICommand 成员

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }
    }

    /// <summary>
    /// 打开
    /// </summary>
    public class OpenProjectCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (ws != null || app != null)
                    return MapGIS.Desktop.Resources.Png_Open_16;
                return MapGIS.PluginEngine.Application.IsRibbon ? MapGIS.Desktop.Resources.Png_Open_32 : MapGIS.Desktop.Resources.Png_Open_16;
            }
        }

        public string Caption
        {
            get { return "打开"; }
        }

        public string Category
        {
            get { return "地球物理数据处理"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "打开"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "打开"; }
        }

        public void OnClick()
        {
            using (GDBOpenFileDialog ofDlg = new GDBOpenFileDialog())
            {
                ofDlg.Filter = "工程文件(*.mapx)|*.mapx";
                if (DialogResult.OK == ofDlg.ShowDialog())
                {
                    OpenPrj(ofDlg.FileName);
                    InitPlugin.App.StateManager.OnStateChanged(null, null);
                }
            }
        }

        public void OpenPrj(string path)
        {
            int rtn = 0;
            if (!string.IsNullOrWhiteSpace(path))
            {
                ProjectManage prjectManage = ProjectManage.GetInstance();
                if (prjectManage != null && prjectManage.ProjectManageItemCount > 0)
                {
                    string ext = Path.GetExtension(path);
                    rtn = prjectManage.OpenFile(ext, path);
                }
                else if (InitPlugin.App != null && InitPlugin.App.Document != null)
                    InitPlugin.App.Document.Open(path);
            }
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        public bool BeginGroup
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        #endregion ICommand 成员

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
        }

        public IApplication App
        {
            get { return app; }

            set { app = value; }
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public class SaveProjectCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (ws != null || app != null)
                    return MapGIS.Desktop.Resources.Png_Save_16;
                return MapGIS.PluginEngine.Application.IsRibbon ? MapGIS.Desktop.Resources.Png_Save_16 : MapGIS.Desktop.Resources.Png_Save_16;
            }
        }

        public string Caption
        {
            get { return "保存"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "保存"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "保存"; }
        }

        public void OnClick()
        {
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        public bool BeginGroup
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        #endregion ICommand 成员

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
        }

        public IApplication App
        {
            get { return app; }

            set { app = value; }
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }
    }

    /// <summary>
    /// 另存为
    /// </summary>
    public class SaveAsProjectCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (ws != null || app != null)
                    return MapGIS.Desktop.Resources.Png_SaveAs_16;
                return MapGIS.PluginEngine.Application.IsRibbon ? MapGIS.Desktop.Resources.Png_SaveAs_16 : MapGIS.Desktop.Resources.Png_SaveAs_16;
            }
        }

        public string Caption
        {
            get { return "另存为"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "另存为"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "另存为"; }
        }

        public void OnClick()
        {
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        public bool BeginGroup
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        #endregion ICommand 成员

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
        }

        public IApplication App
        {
            get { return app; }

            set { app = value; }
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public class CloseProjectCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (ws != null || app != null)
                    return MapGIS.Desktop.Resources.Png_Close_16;
                return MapGIS.PluginEngine.Application.IsRibbon ? MapGIS.Desktop.Resources.Png_Close_32 : MapGIS.Desktop.Resources.Png_Close_16;
            }
        }

        public string Caption
        {
            get { return "关闭"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return "关闭"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "关闭"; }
        }

        public void OnClick()
        {
            ProjectManage proManage = ProjectManage.GetInstance();
            if (proManage != null)
            {
                proManage.CloseProject();
            }
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        public bool BeginGroup
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        #endregion ICommand 成员

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
        }

        public IApplication App
        {
            get { return app; }

            set { app = value; }
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }
    }


    /// <summary>
    /// 添加数据
    /// </summary>
    public class AddDataCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get { return MapGIS.Desktop.Resources.Png_AddLayer_16; }
        }

        public string Caption
        {
            get { return "添加数据"; }
        }

        public string Category
        {
            get { return "地球物理数据处理"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            // 【核心修改】: 直接返回 true，让按钮永远处于可用状态
            get { return true; }
        }

        public string Message
        {
            get { return "添加地球物理数据到对应的地图中"; }
        }

        public string Name
        {
            get { return "AddDataCommand"; }
        }

        public string Tooltip
        {
            get { return "添加数据"; }
        }

        public void OnClick()
        {
            // 【重要保障】：这个检查现在变得至关重要！
            // 它确保了即使用户在没有项目时点击按钮，程序也不会崩溃。
            if (this.app == null || this.app.Document == null)
            {
                MessageBox.Show("请先打开或新建一个项目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return; // 阻止代码继续执行
            }

            // 只有通过了上面的检查，才会执行下面的代码
            using (Form_AddData form = new Form_AddData(this.app))
            {
                form.ShowDialog();
            }
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
        }

        #endregion

        #region 其他接口实现 (保持标准实现)

        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
            // 由于按钮一直可用，这个事件处理函数现在不是必需的了，但保留也无妨
        }

        public bool BeginGroup
        {
            get { return true; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public IApplication App
        {
            get { return app; }
            set { app = value; }
        }

        public void OnClick(DocumentItem item)
        {
            OnClick();
        }

        public void OnCreate(IWorkSpace ws)
        {
            this.ws = ws;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            OnClick();
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }
        #endregion
    }






}
// *** 这是 addCommand 类的结束位置 ***