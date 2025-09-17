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
                {
                    ProjectManage prjectManage = ProjectManage.GetInstance();
                    if (prjectManage != null && prjectManage.ProjectManageItemCount > 0)
                    {
                        string mapxPath = Path.Combine(form.PrjPath, form.PrjName + ".mapx");

                        // 创建新的Document实例
                        Document newDoc = new Document();
                        newDoc.New();
                        newDoc.Title = form.PrjName;
                        // 创建新的Map实例，并设置名称
                        Map newMap = new Map();
                        newMap.Name = "原始数据";
                        Map newMap2 = new Map();
                        newMap2.Name = "高程数据";
                        Map newMap3 = new Map();
                        newMap3.Name = "预处理数据";
                        Map newMap4 = new Map();
                        newMap4.Name = "反演解释";


                        // 将新地图添加到文档中
                        newDoc.GetMaps().Append(newMap);
                        newDoc.GetMaps().Append(newMap2);
                        newDoc.GetMaps().Append(newMap3);
                        newDoc.GetMaps().Append(newMap4);
                        // 保存文档为.mapx文件
                        newDoc.SaveAs(mapxPath);

                        // 关闭文档
                        newDoc.Close(false);

                        // 打开新创建的.mapx文件

                        // 弹出消息框询问是否打开新创建的文档
                        if (XMessageBox.QuestionEx("创建完成，是否打开？") == DialogResult.Yes)
                            InitPlugin.App.Document.Open(mapxPath);
                        return;

                    }
                }

                //else
                //XMessageBox.Information("创建失败！");
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


    public class addCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;
        private Map map = null;
        private Document maindc = null;


        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (ws != null || app != null)
                    return null;
                return null;
            }
        }

        public string Caption
        {
            get { return "重磁数据添加"; }
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
            get { return "添加重磁数据"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "添加重磁数据"; }
        }

        public void OnClick()
        {
            //m_Maindoc = app.Document; 
            
            AddDataForm adddataform = new AddDataForm(maindc);
            if (adddataform.ShowDialog() == DialogResult.OK)
            {
                string url = adddataform.Url;

                if (url == null || url == "")
                {
                    adddataform.Dispose();
                    return;
                }

                map.RemoveAll();//清空上一个map中的图层
                if (url.Contains("/sfcls/"))
                {
                    VectorLayer vectorlayer = new VectorLayer(VectorLayerType.SFclsLayer);
                    vectorlayer.URL = url;
                    if (vectorlayer.ConnectData())
                    {
                        // 修改说明：默认行为调整为如下方式，引导终端客户关注配图。解决bug12544
                        // 修改人：张凯俊 2019-07-8
                        vectorlayer.SymbolShow = true;
                        vectorlayer.FollowZoom = false;
                        map.Append(vectorlayer);

                    }
                }
                else
                {
                    RasterLayer raslayer = new RasterLayer();
                    if (url.Contains("/ras/"))
                        raslayer.URL = url;
                    else if (url.Contains("file:///"))
                        raslayer.URL = url;
                    else
                        raslayer.URL = "file:///" + url;
                    if (raslayer.ConnectData())
                    {
                        map.Append(raslayer);

                    }
                }
            }
            adddataform.Dispose();
        }

        public void OnCreate(IApplication hook)
        {
            this.app = hook;
            if (this.app != null)
                this.app.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);

            maindc = app.Document;
            Document dc = new Document();
            Maps maps = dc.GetMaps();
            map = maps.GetMap(0);
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
}