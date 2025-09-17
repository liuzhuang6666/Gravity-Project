/*----------------------------------------------------------------
// Copyright (C) 2019 中地数码科技有限公司
// 版权所有。
//
// 文件名：InitPlugin
// 文件功能描述：
//
//
// 创建标识：韩威 2019/3/11 16:41:51
//----------------------------------------------------------------*/

using MapGIS.GeoMap;
using MapGIS.PluginEngine;
using MapGIS.GISControl;
using MapGISPlugin3;
using MapGIS.WorkSpaceEngine;
using System.Collections.Generic;

namespace MapGISPlugin3
{
    public class InitPlugin : IConnect
    {
        private static IApplication app;//应用程序框架对象
        private List<IMenuItem> appendMenuItems = new List<IMenuItem>();
        private DocumentItemPropertiesManage documentItemPropertiesManage;


        public static IApplication App
        {
            get { return app; }
        }

        public void OnConnection(IApplication app)
        {
            InitPlugin.app = app;
            this.InitMenuItems(InitPlugin.app.WorkSpaceEngine);
            app.ApplicationLoadedEvent += App_ApplicationLoadedEvent;
            app.PluginContainer.PluginLoadedEvent += new PluginLoadedHandler(PluginContainer_PluginLoadedEvent);
            ProjectManage prjectManage = ProjectManage.GetInstance();
            if (prjectManage != null)
            {
                prjectManage.AddProjectManageItem(".mpj", new M3DProjectMag(app));
            }
            documentItemPropertiesManage = new DocumentItemPropertiesManage();
        }


        private void PluginContainer_PluginLoadedEvent(IPlugin plugin)
        {


            if (plugin is IMapContentsView)
            {
                IMapContentsView controView = (plugin as IMapContentsView);
                MapControl mapControl = controView.MapControl;
                if (mapControl != null)
                {

                    //ThreeDimensionBridge.StaticFun.SetLayerShowTileIndex(sceneControl, null, 1, "", 1);


                    //SceneControl sc = this.workSpace.GetSceneControl(scene);
                    //if (sc != null && e.InsertedLayer != null)
                    //    sc.UpdateScene(scene, scene.IndexOf(e.InsertedLayer), LayerOperType.SetLayerAppend);

                }
            }

        }

        private void App_ApplicationLoadedEvent()
        {
            Document doc = InitPlugin.app.Document;
            if (doc != null)
                InitPlugin.app.Document.Close(false);
            SetCommandVisible(false);
            if (app.args != null && app.args.Length > 0)
            {
                for (int i = 0; i < app.args.Length; i++)
                {
                    string path = app.args[i].ToString();
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        ICommand cd = null;
                        app.PluginContainer.Commands.TryGetValue(typeof(OpenProjectCommand).ToString(), out cd);
                        if (cd != null)
                        {
                            cd.OnClick();
                        }
                    }
                }
            }

            doc.New();
            doc.Title = "新工程 ";
            Map scene = new Map();
            scene.Name = "原始数据";
            scene.SetPropertyEx("InitOpenView", "true");
            try
            {
                doc.GetMaps().Append(scene);
                InitPlugin.App.WorkSpaceEngine.FireMenuItemClickEvent("MapGIS.WorkSpace.Style.Previewmap", scene);
            }
            catch { }
        }

        private static void SetCommandVisible(bool isVisible)
        {
            IMenuExtander ime = app.WorkSpaceEngine.GetMenuExtand(typeof(GlobalNodeType));
            if (ime != null)
            {
                IMenuItem[] allMenuItems = ime.GetItems();
                if (allMenuItems != null)
                {
                    for (int i = 0; i < allMenuItems.Length; i++)
                    {
                        IMenuItem menuItem = allMenuItems[i];
                        if (menuItem != null)
                        {
                            switch (menuItem.GetType().ToString())
                            {
                                case "MapGIS.WorkSpace.Style.NewDocument":
                                case "MapGIS.WorkSpace.Style.OpenDocument":
                                case "MapGIS.WorkSpace.Style.SaveDocument":
                                case "MapGIS.WorkSpace.Style.CloseDocument":
                                    app.WorkSpaceEngine.SetMenuItemVisible(menuItem, isVisible);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void OnDisconnection()
        {
            if (documentItemPropertiesManage != null)
                documentItemPropertiesManage.Dispose();
            SetCommandVisible(true);
            this.UnInitMenuItems(InitPlugin.app.WorkSpaceEngine);
        }

        /// <summary>
        /// 删除此插件在工作空间中添加的右键菜单
        /// </summary>
        /// <param name="workSpace"></param>
        private void UnInitMenuItems(MapGIS.WorkSpaceEngine.IWorkSpace workSpace)
        {
            if (workSpace != null)
            {
                IMenuExtander ime = workSpace.GetMenuExtand(typeof(GlobalNodeType));
                if (ime != null)
                {
                    foreach (IMenuItem it in this.appendMenuItems)
                    {
                        ime.RemoveItem(it);
                    }
                }

                ime = workSpace.GetMenuExtand(typeof(MapGIS.GeoMap.Map));
                if (ime != null)
                {
                    foreach (IMenuItem it in this.appendMenuItems)
                    {
                        ime.RemoveItem(it);
                    }
                }

                ime = workSpace.GetMenuExtand(typeof(MapGIS.GeoMap.VectorLayer));
                if (ime != null)
                {
                    foreach (IMenuItem it in this.appendMenuItems)
                    {
                        ime.RemoveItem(it);
                    }
                }

                
            }
        }
        
        /// <summary>
        /// 初始化工作空间右键菜单
        /// </summary>
        /// <param name="workSpace"></param>
        private void InitMenuItems(MapGIS.WorkSpaceEngine.IWorkSpace workSpace)
        {
            if (workSpace != null)
            {
                IMenuExtander ime = workSpace.GetMenuExtand(typeof(GlobalNodeType));
                if (ime != null)
                {
                    //添加创建制图图层右键
                    NewProjectCommand item = new NewProjectCommand();
                    item.OnCreate(workSpace);
                    ime.AddItem(item);
                    appendMenuItems.Add(item);
                    OpenProjectCommand openItem = new OpenProjectCommand();
                    openItem.OnCreate(workSpace);
                    ime.AddItem(openItem);
                    appendMenuItems.Add(openItem);
                    SaveProjectCommand saveItem = new SaveProjectCommand();
                    saveItem.OnCreate(workSpace);
                    ime.AddItem(saveItem);
                    appendMenuItems.Add(saveItem);
                    CloseProjectCommand closeItem = new CloseProjectCommand();
                    closeItem.OnCreate(workSpace);
                    ime.AddItem(closeItem);
                    appendMenuItems.Add(closeItem);
                    addCommand addItem = new addCommand();
                    addItem.OnCreate(workSpace);
                    ime.AddItem(addItem);
                    appendMenuItems.Add(addItem);


                }
            }
        }
    }
}