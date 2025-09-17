using DevExpress.Utils;
using DevExpress.XtraBars;
using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoObjects.Geometry3D;
using MapGIS.GeoObjects.Info;
using MapGIS.PluginEngine;
using MapGIS.Scene3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public static class StaticFunctions
    {
        //public const string OUTDOOR = "室外";
        //public const string INDOOR = "室内";
        //public const string LAND_WATER = "水陆一体";
        //public const string UNDERGROUD = "地下";

        /// <summary>
        /// 增加最近打开的文件到最近打开组
        /// </summary>
        /// <param name="app">应用程序</param>
        /// <param name="recentFile">最近打开文件</param>
        public static void AddRecentDoc(IApplication app, string recentFile)
        {
            if (!string.IsNullOrEmpty(recentFile))
            {
                IRecentFileGroup rfg = null;
                rfg = app.RecentFileManager["RecentFile"];
                if (rfg != null)
                    rfg.Insert(0, recentFile);
            }
        }

        public static Document GetDocument(DocumentItem map)
        {
            Document doc = null;
            if (map != null)
            {
                DocumentItem item = map.Parent;
                if (item != null)
                {
                    if (item is Document)
                        return item as Document;
                    else
                        return GetDocument(item);
                }
            }
            return doc;
        }
        public static string GetMgisPath()
        {
            try
            {
                System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies != null && assemblies.Length > 0)
                {
                    for (int i = 0; i < assemblies.Length; i++)
                    {
                        if (assemblies[i] != null && assemblies[i].ManifestModule != null && ("MapGIS.ThreeDimension.Plugin.dll".Equals(assemblies[i].ManifestModule.Name, StringComparison.InvariantCultureIgnoreCase) || "MapGIS.ThreeDimension.Plugin.dll".Equals(assemblies[i].ManifestModule.ScopeName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            return Path.GetDirectoryName(assemblies[i].ManifestModule.FullyQualifiedName);
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }
        public static string GetConfigPath()
        {
            try
            {
                string dirPath = GetMgisPath();
                if (!string.IsNullOrWhiteSpace(dirPath))
                    return Path.Combine(dirPath, "Config");
            }
            catch
            {
            }
            return null;
        }

        
    #region 组织菜单

    public static void LoadBarManager(IApplication app, List<IRightMenuBar> menuManage, String info, List<BarItem> listItem, ImageCollection imageCollection)
        {
            if (menuManage != null && menuManage.Count > 0)
            {
                for (int i = 0; i < menuManage.Count; i++)
                {
                    IRightMenuBar menuBar = menuManage[i];
                    if (menuBar != null && menuBar.ToString().Equals(info))
                    {
                        LoadBarManager(app, menuBar, listItem, imageCollection);
                        break;
                    }
                }
            }
        }

        private static void LoadBarManager(IApplication app, IRightMenuBar menuBar, List<BarItem> listItem, ImageCollection imageCollection)
        {
            if (menuBar != null)
            {
                List<IItem> items = menuBar.ItemList;
                if (items != null && items.Count > 0)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        IItem item = items[j];
                        if (item == null)
                            continue;
                        BarItem barItem = GetBarItem(app, item, listItem, imageCollection);
                        if (barItem != null)
                        {
                            barItem.Id = listItem.Count;
                            listItem.Add(barItem);
                        }
                    }
                }
            }
        }

        private static BarItem GetBarItem(IApplication app, IItem item, List<BarItem> listItem, ImageCollection imageCollection)
        {
            BarItem rtn = null;
            if (item != null)
            {
                if (item is SubItem)
                {
                    IItem[] items = (item as SubItem).Items;
                    if (items.Length <= 0)
                        return rtn;
                    BarSubItem barSubItem = new BarSubItem();
                    barSubItem.Caption = (item as SubItem).Caption;
                    barSubItem.Name = (item as SubItem).ToString();
                    for (int i = 0; i < items.Length; i++)
                    {
                        BarItem bar = GetBarItem(app, items[i], listItem, imageCollection);
                        if (bar != null)
                        {
                            bar.Id = listItem.Count;
                            barSubItem.AddItem(bar);
                        }
                    }
                    if (barSubItem.LinksPersistInfo.Count > 0)
                    {
                        barSubItem.Tag = new object[] { item, (item as SubItem).Group };
                        rtn = barSubItem;
                    }
                }
                else if (item is Item)
                {
                    BarButtonItem bar = GetBarButtonItem(app, (item as Item), imageCollection);
                    if (bar != null)
                    {
                        rtn = bar;
                    }
                }
            }
            return rtn;
        }

        private static BarButtonItem GetBarButtonItem(IApplication app, Item item, ImageCollection imageCollection)
        {
            BarButtonItem rtn = null;
            IPlugin plugin = GetCommand(app, item);
            if (plugin == null)
                return rtn;
            if (plugin is IMenuCommand)
            {
                rtn = new BarButtonItem();
                rtn.Caption = (plugin as IMenuCommand).Caption;
                rtn.Name = (plugin as IMenuCommand).Name;
                Bitmap bimap = (plugin as IMenuCommand).Bitmap;
                if (imageCollection != null && bimap != null)
                {
                    int index = imageCollection.Images.Add(bimap);
                    rtn.ImageIndex = index;
                }
                rtn.Tag = new object[] { plugin, item.Group };
            }
            else if (plugin is ICommand)
            {
                rtn.Caption = (plugin as ICommand).Caption;
                rtn.Name = (plugin as ICommand).Name;
                Bitmap bimap = (plugin as ICommand).Bitmap;
                if (imageCollection != null && bimap != null)
                {
                    int index = imageCollection.Images.Add(bimap);
                    rtn.ImageIndex = index;
                }
                rtn.Tag = new object[] { plugin, item.Group };
            }
            return rtn;
        }

        private static IPlugin GetCommand(IApplication app, Item item)
        {
            IPlugin rtn = null;
            if (item == null)
                return rtn;
            string key = item.Key;
            if (string.IsNullOrWhiteSpace(key))
                return rtn;
            ICommand cd = null;
            if (app != null)
                app.PluginContainer.Commands.TryGetValue(key, out cd);
            if (cd != null)
                rtn = cd;
            if (rtn == null)
            {
                Type type = Type.GetType(key);
                if (type == null && item is MenuItem)
                {
                    type = (item as MenuItem).Type;
                }
                if (type == null)
                    return rtn;
                object instance = StaticFunctions.CreateInstance(type);
                if (instance is ICommand)
                {
                    (instance as ICommand).OnCreate(app);
                    app.PluginContainer.Commands.Add(key, (instance as ICommand));
                    rtn = (instance as IMenuCommand);
                }
                else if (instance is IMenuCommand)
                {
                    (instance as IMenuCommand).App = app;
                    rtn = (instance as IMenuCommand);
                }
            }
            return rtn;
        }

        public static void PopupMenuAddLinkPersist(PopupMenu popupMenu1, List<BarItem> listItem)
        {
            foreach (BarItem item in listItem)
            {
                if (item != null && item.Tag is object[])
                {
                    object[] tag = item.Tag as object[];
                    if (tag != null && tag.Length == 2 && tag[1] != null)
                    {
                        if (tag[1] is bool)
                        {
                            popupMenu1.LinksPersistInfo.Add(new DevExpress.XtraBars.LinkPersistInfo(item, (bool)tag[1]));
                            continue;
                        }
                    }
                }
                popupMenu1.LinksPersistInfo.Add(new DevExpress.XtraBars.LinkPersistInfo(item, false));
            }
        }

        public static void ManagerAddItem(BarManager barManager, List<BarItem> listItem)
        {
            if (barManager == null || listItem == null)
                return;
            foreach (BarItem item in listItem)
            {
                if (item is BarSubItem)
                {
                    BarSubItem subItem = (item as BarSubItem);
                    LinksInfo links = subItem.LinksPersistInfo;
                    List<BarItem> subitems = new List<BarItem>();
                    for (int i = 0; i < links.Count; i++)
                    {
                        LinkPersistInfo link = links[i];
                        BarItem barItem = link.Item;
                        subitems.Add(barItem);
                    }
                    if (subitems != null && subitems.Count > 0)
                    {
                        ManagerAddItem(barManager, subitems);
                        barManager.Items.Add(subItem);
                    }
                }
                else
                {
                    if (item is BarButtonItem)
                        barManager.Items.Add(item);
                }
            }
        }

        #endregion 组织菜单

        /// <summary>
        /// 创建Type的实例
        /// </summary>
        public static object CreateInstance(Type type)
        {
            object rtn = null;
            try
            {
                rtn = Activator.CreateInstance(type);
            }
            catch (System.Exception ex) { }
            return rtn;
        }

        public static void CopyDir(string oldPath, string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath) || string.IsNullOrWhiteSpace(oldPath))
                return;

            try
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                string[] files = Directory.GetFiles(oldPath);
                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            string filePath = files[i];
                            string name = Path.GetFileName(filePath);
                            string newPath = Path.Combine(dirPath, name);
                            File.Copy(filePath, newPath);
                        }
                        catch { }
                    }
                }
                string[] dirs = Directory.GetDirectories(oldPath);
                if (dirs != null && dirs.Length > 0)
                {
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        try
                        {
                            string filePath = dirs[i];
                            string name = Path.GetFileName(filePath);
                            string newPath = Path.Combine(dirPath, name);
                            CopyDir(filePath, newPath);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        public static void CreateResourceDir(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
                return;
            try
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                if (!Directory.Exists(Path.Combine(dirPath, "室外")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室外"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "RGBD")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "RGBD"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "RGBD", "image")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "RGBD", "image"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "RGBD", "depth")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "RGBD", "depth"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "视觉")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "视觉"));
                if (!Directory.Exists(Path.Combine(dirPath, "水陆")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "水陆"));
                if (!Directory.Exists(Path.Combine(dirPath, "地下")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "地下"));
            }
            catch { }
        }

        // 预览场景
        public static void MapViewPreview(IApplication app, Map Map)
        {
            if (app == null || Map == null)
                return;
            MapGIS.WorkSpaceEngine.IWorkSpace workSpace = app.WorkSpaceEngine;
            workSpace.FireMenuItemClickEvent("MapGIS.WorkSpace.Style.PreviewMap", Map);
            app.StateManager.OnStateChanged(null, new StateEventArgs());
        }
        public static string GetCameraPath()
        {
            {
                try
                {
                    string dirPath = GetConfigPath();
                    if (!string.IsNullOrWhiteSpace(dirPath))
                        return Path.Combine(dirPath, "sensor_width_camera_database.txt");
                }
                catch
                {
                }
                return null;
            }
        }


        public static string GetRelativePathBympjPath(string path, string mcjPath)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(mcjPath))
                return path;
            string dirPath = Path.GetDirectoryName(mcjPath);
            if (path.StartsWith(dirPath))
                return path.Remove(0, dirPath.Length);
            return path;
        }

        public static string GetRootedPathBymcjPath(string path, string mcjPath)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(mcjPath))
                return path;
            while (path.StartsWith("\\"))
                path = path.Remove(0, 1);
            while (path.StartsWith("/"))
                path = path.Remove(0, 1);
            if (Path.IsPathRooted(path))
                return path;
            string dirPath = Path.GetDirectoryName(mcjPath);
            return Path.Combine(dirPath, path);
        }
        /// <summary>
        /// 判断给定的选择集是否为空
        /// </summary>
        /// <param name="selSet">选择集</param>
        /// <returns>true—空，false-非空</returns>
        public static bool IsSelSetNullOrEmpty(Select3DSet selSet)
        {
            bool isEmpty = true;
            if (selSet != null)
            {
                List<Select3DSetItem> items = selSet.Get();
                isEmpty = items == null || items.Count == 0;
            }
            return isEmpty;
        }
        /// <summary>
        /// 获取控件中第一个场景
        /// </summary>
        /// <param name="ctr"></param>
        /// <returns></returns>
        //public static Scene GetScene(SceneControl ctr)
        //{
        //    Scene scene = null;
        //    if (ctr != null)
        //        scene = ctr.GetScene(0);
        //    return scene;
        //}

        /// <summary>
        /// 移动几何图形得到新的几何图形
        /// </summary>
        /// <param name="geom">原几何图形</param>
        /// <param name="dx">x方向移动距离</param>
        /// <param name="dy">y方向移动距离</param>
        /// <param name="dz">z方向移动距离</param>
        /// <returns>移动后的几何图形</returns>
        public static IGeometry3D MovingGeometry(IGeometry3D geom, double dx, double dy, double dz)
        {
            IGeometry3D rtnGeom = null;
            if (geom != null)
            {
                rtnGeom = geom.Clone() as IGeometry3D;
                if (rtnGeom is GeoAnySurface)
                    rtnGeom = (rtnGeom as GeoAnySurface).Move(dx, dy, dz);
                else if (rtnGeom is GeoAnyEntity)
                    rtnGeom = (rtnGeom as GeoAnyEntity).Move(dx, dy, dz);
                else if (rtnGeom is GeoMultiSurface)
                    rtnGeom = (rtnGeom as GeoMultiSurface).Move(dx, dy, dz);
                else if (rtnGeom is GeoMultiEntity)
                    rtnGeom = (rtnGeom as GeoMultiEntity).Move(dx, dy, dz);
            }
            return rtnGeom;
        }

        /// <summary>
        /// 移动选择集
        /// </summary>
        /// <param name="selSet">选择集</param>
        /// <param name="dx">X方向移动距离</param>
        /// <param name="dy">Y方向移动距离</param>
        /// <param name="dz">Z方向移动距离</param>
        //public static void MoveSelect3DSet(Select3DSet selSet, SceneControl sceneControl, double dx, double dy, double dz)
        //{
        //    Scene scene = GetScene(sceneControl);
        //    if (scene != null && !IsSelSetNullOrEmpty(selSet))
        //    {
        //        List<Select3DSetItem> selItems = selSet.Get();
        //        foreach (Select3DSetItem item in selItems)
        //        {
        //            ModelLayer layer = item.Layer as ModelLayer;
        //            List<long> idList = item.IDList;
        //            if (layer != null && idList != null && idList.Count > 0)
        //            {
        //                SFeatureCls sfCls = layer.GetData() as SFeatureCls;
        //                if (sfCls != null)
        //                {
        //                    int layerIndex = layer.GetLayerRenderIndex();
        //                    foreach (long id in idList)
        //                    {
        //                        IGeometry3D tempGeom = MovingGeometry(sfCls.GetGeometry(id) as IGeometry3D, dx, dy, dz);
        //                        if (tempGeom != null)
        //                        {
        //                            if (sfCls.Update(id, tempGeom, sfCls.GetAtt(id), sfCls.GetInfo(id)))
        //                            {
        //                                sfCls.CalRange();
        //                                sceneControl.UpdateScene(scene, layerIndex, (uint)id, ModelOperType.UpdateMdl);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 移动选择集
        /// </summary>
        /// <param name="selSet">选择集</param>
        /// <param name="dx">X方向移动距离</param>
        /// <param name="dy">Y方向移动距离</param>
        /// <param name="dz">Z方向移动距离</param>
        //public static void MoveSelect3DSet(Result3DSet selSet, SceneControl sceneControl, double dx, double dy, double dz)
        //{
        //    HistoryOperate b = HistoryOperate.GetInstance(sceneControl);
        //    b.Start("move");
        //    Scene scene = GetScene(sceneControl);
        //    if (scene != null && selSet != null && selSet.Num > 0)
        //    {
        //        for (int i = 0; i < selSet.Num; i++)
        //        {
        //            Result3DItem item = selSet.get_Item(i);
        //            if (item != null && item.Info != null)
        //            {
        //                ModelLayer layer = scene.GetLayerEx((int)item.Info.LayerIndex) as ModelLayer;
        //                uint oid = item.Info.ModelID;
        //                if (layer != null && oid > 0)
        //                {
        //                    int layerIndex = layer.GetLayerRenderIndex();
        //                    SFeatureCls sfCls = layer.GetData() as SFeatureCls;
        //                    if (sfCls != null)
        //                    {
        //                        b.AppendOperData(sfCls, (int)oid, GeoMap.OperationType.Modify);
        //                        IGeometry3D tempGeom = MovingGeometry(sfCls.GetGeometry(oid) as IGeometry3D, dx, dy, dz);
        //                        if (tempGeom != null)
        //                        {
        //                            if (sfCls.Update(oid, tempGeom, sfCls.GetAtt(oid), sfCls.GetInfo(oid)))
        //                            {
        //                                sfCls.CalRange();
        //                                sceneControl.UpdateScene(scene, layerIndex, oid, ModelOperType.UpdateMdl);

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    b.End();
        //}

        /// <summary>
        /// 移动简单要素类
        /// </summary>
        /// <param name="sfCls">简单要素类</param>
        /// <param name="dx">X方向移动距离</param>
        /// <param name="dy">Y方向移动距离</param>
        /// <param name="dz">Z方向移动距离</param>
        public static void MoveSFeatureCls(SFeatureCls sfCls, double dx, double dy, double dz)
        {
            if (sfCls != null)
            {
                QueryDef qd = new QueryDef();
                qd.CursorType = SetCursorType.ForwardOnly;
                RecordSet rs = sfCls.Select(qd);
                if (rs != null)
                {
                    rs.MoveFirst();
                    while (!rs.IsEOF)
                    {
                        IGeometry3D tempGeom = MovingGeometry(rs.Geometry as IGeometry3D, dx, dy, dz);
                        if (tempGeom != null)
                            sfCls.Update(rs.CurrentID, tempGeom, rs.Att, rs.Info);

                        rs.MoveNext();
                    }
                    rs.Dispose();
                    sfCls.CalRange();
                }
            }
        }


        /// <summary>
        /// 返回选择集个数
        /// </summary>
        /// <param name="selSet"></param>
        /// <returns></returns>
        public static int GetSelectCount(Select3DSet selSet)
        {
            int count = 0;
            if (selSet != null)
            {
                List<Select3DSetItem> lst = selSet.Get();
                if (lst != null && lst.Count > 0)
                {
                    foreach (Select3DSetItem item in lst)
                    {
                        count += item.IDList.Count;
                        item.Layer.Dispose();
                    }
                }
                lst.Clear();
                lst = null;
            }
            return count;
        }


        /// <summary>
        /// 获取指定状态图层列表
        /// </summary>
        /// <param name="ctr">场景视图控件</param>
        /// <param name="type">图层类型</param>
        /// <param name="slc">图层状态</param>
        /// <returns>图层列表</returns>
        //public static List<G3DLayer> GetEditLayers(SceneControl ctr, EditLayerType type, SelectLayerControl slc = SelectLayerControl.Editable)
        //{
        //    List<G3DLayer> layers = new List<G3DLayer>();
        //    if (ctr != null)
        //    {
        //        Scene scene = ctr.GetScene(0);
        //        if (scene != null)
        //        {
        //            layers = scene.GetEditLayer(type, slc);
        //            if (layers == null)
        //                layers = new List<G3DLayer>();
        //        }
        //    }
        //    return layers;
        //}


        /// <summary>
        /// 旋转选择集
        /// </summary>
        /// <param name="selSet">选择集</param>
        /// <param name="sceneControl">场景控件</param>
        /// <param name="center">旋转中心点</param>
        /// <param name="angleX">X方向旋转角度</param>
        /// <param name="angleY">Y方向旋转角度</param>
        /// <param name="angleZ">Z方向旋转角度</param>
        //public static void RotateResult3DSet(Result3DSet selSet, SceneControl sceneControl, Dot3D center, double angleX, double angleY, double angleZ)
        //{
        //    HistoryOperate historySingle = HistoryOperate.GetInstance(sceneControl);
        //    historySingle.Start("rotate");
        //    Scene scene = GetScene(sceneControl);
        //    if (scene != null && selSet != null && selSet.Num > 0)
        //    {
        //        for (int i = 0; i < selSet.Num; i++)
        //        {
        //            Result3DItem item = selSet.get_Item(i);
        //            if (item != null && item.Info != null)
        //            {
        //                ModelLayer layer = scene.GetLayerEx((int)item.Info.LayerIndex) as ModelLayer;
        //                uint oid = item.Info.ModelID;
        //                if (layer != null && oid > 0)
        //                {
        //                    int layerIndex = layer.GetLayerRenderIndex();
        //                    SFeatureCls sfCls = layer.GetData() as SFeatureCls;
        //                    if (sfCls != null)
        //                    {
        //                        historySingle.AppendOperData(sfCls, (int)oid, GeoMap.OperationType.Modify);
        //                        IGeometry3D tempGeom = RotatingGeometry(sfCls.GetGeometry(oid) as IGeometry3D, center, angleX, angleY, angleZ);
        //                        if (tempGeom != null)
        //                        {
        //                            if (sfCls.Update(oid, tempGeom, sfCls.GetAtt(oid), sfCls.GetInfo(oid)))
        //                            {
        //                                sfCls.CalRange();
        //                                sceneControl.UpdateScene(scene, layerIndex, oid, ModelOperType.UpdateMdl);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    historySingle.End();
        //}

        /// <summary>
        /// 旋转几何图形得到新的几何图形
        /// </summary>
        /// <param name="geom">原几何图形</param>
        /// <param name="center">旋转中心点</param>
        /// <param name="angleX">X方向旋转角度</param>
        /// <param name="angleY">Y方向旋转角度</param>
        /// <param name="angleZ">Z方向旋转角度</param>
        /// <returns>旋转后的几何图形</returns>
        public static IGeometry3D RotatingGeometry(IGeometry3D geom, Dot3D center, double angleX, double angleY, double angleZ)
        {
            IGeometry3D rtnGeom = null;
            if (geom != null)
            {
                rtnGeom = geom.Clone() as IGeometry3D;
                if (rtnGeom is GeoAnySurface)
                    rtnGeom = (rtnGeom as GeoAnySurface).Rotate(center, angleX, angleY, angleZ);
                else if (rtnGeom is GeoAnyEntity)
                    rtnGeom = (rtnGeom as GeoAnyEntity).Rotate(center, angleX, angleY, angleZ);
                else if (rtnGeom is GeoMultiSurface)
                    rtnGeom = (rtnGeom as GeoMultiSurface).Rotate(center, angleX, angleY, angleZ);
                else if (rtnGeom is GeoMultiEntity)
                    rtnGeom = (rtnGeom as GeoMultiEntity).Rotate(center, angleX, angleY, angleZ);
            }
            return rtnGeom;
        }
        /// <summary>
        /// 旋转选择集
        /// </summary>
        /// <param name="sfCls">简单要素类</param>
        /// <param name="center">旋转中心点</param>
        /// <param name="angleX">X方向旋转角度</param>
        /// <param name="angleY">Y方向旋转角度</param>
        /// <param name="angleZ">Z方向旋转角度</param>
        public static void RotateSFeatureCls(SFeatureCls sfCls, Dot3D center, double angleX, double angleY, double angleZ)
        {
            if (sfCls != null)
            {
                QueryDef qd = new QueryDef();
                qd.CursorType = SetCursorType.ForwardOnly;
                RecordSet rs = sfCls.Select(qd);
                if (rs != null)
                {
                    rs.MoveFirst();
                    while (!rs.IsEOF)
                    {
                        IGeometry3D tempGeom = RotatingGeometry(rs.Geometry as IGeometry3D, center, angleX, angleY, angleZ);
                        if (tempGeom != null)
                            sfCls.Update(rs.CurrentID, tempGeom, rs.Att, rs.Info);

                        rs.MoveNext();
                    }
                    rs.Dispose();
                    sfCls.CalRange();
                }
            }
        }
        /// <summary>
        /// 计算三维选择集的三维矩形范围
        /// </summary>
        /// <param name="selSet">三维选择集</param>
        /// <returns>三维选择集的三维矩形范围</returns>
        public static Rect3D CalRange(Select3DSet selSet)
        {
            Rect3D rtnRect = null;
            if (selSet != null)
            {
                List<Select3DSetItem> selItems = selSet.Get();
                foreach (Select3DSetItem item in selItems)
                {
                    ModelLayer layer = item.Layer as ModelLayer;
                    List<long> idList = item.IDList;
                    if (layer != null && idList != null && idList.Count > 0)
                    {
                        SFeatureCls sfCls = layer.GetData() as SFeatureCls;
                        if (sfCls != null)
                        {
                            foreach (long id in idList)
                            {
                                Rect3D rect = sfCls.GetRect3D(id);
                                if (rtnRect == null)
                                    rtnRect = rect.Clone();
                                else
                                {
                                    rtnRect.XMin = Math.Min(rect.XMin, rtnRect.XMin);
                                    rtnRect.YMin = Math.Min(rect.YMin, rtnRect.YMin);
                                    rtnRect.ZMin = Math.Min(rect.ZMin, rtnRect.ZMin);
                                    rtnRect.XMax = Math.Max(rect.XMax, rtnRect.XMax);
                                    rtnRect.YMax = Math.Max(rect.YMax, rtnRect.YMax);
                                    rtnRect.ZMax = Math.Max(rect.ZMax, rtnRect.ZMax);
                                }
                            }
                        }
                    }
                }
            }
            return rtnRect;
        }

        /// <summary>
        /// 计算三维选择集的三维矩形中心点
        /// </summary>
        /// <param name="selSet">三维选择集</param>
        /// <returns>计算三维选择集的三维矩形中心点</returns>
        public static Dot3D CalCenter(Select3DSet selSet)
        {
            Dot3D dot = null;
            Rect3D rect = CalRange(selSet);
            if (rect != null)
                dot = new Dot3D((rect.XMax + rect.XMin) / 2, (rect.YMax + rect.YMin) / 2, (rect.ZMax + rect.ZMin) / 2);
            return dot;
        }

        /// <summary>
        /// 根据路径创建hdf
        /// </summary>
        /// <param name="gdbPath">hdf路径</param>
        /// <returns>数据库</returns>
        public static DataBase CreateDataBase(Server svr, string gdbPath)
        {
            DataBase gdb = null;

            if (svr == null || string.IsNullOrEmpty(gdbPath))
                return null;
            if (!svr.HasConnect)
                return null;
            string gdbName = System.IO.Path.GetFileNameWithoutExtension(gdbPath);
            if (string.IsNullOrWhiteSpace(gdbName) || svr.GetDBID(gdbName) > 0)
                return null;

            LogEventReceiver LogER = new LogEventReceiver();  //日志事件触发器 
            GDBCreateParam createParam = new GDBCreateParam();//地理数据库创建信息对象
            DBFileInfo fileInfo = new DBFileInfo();           //数据库文件信息对象
            List<DBFileInfo> ListDB = new List<DBFileInfo>(); //数据库文件信息列表对象
            FileExtendInfo extendInfo = new FileExtendInfo(); //数据库文件扩展信息对象

            //设置数据库文件扩展信息  ***必须设置
            extendInfo.ExtendMode = FileExtendMode.Size;    //数据增长类型
            extendInfo.ExtendSize = 5;                      //数据文件增长大小
            extendInfo.ExtendUnit = FileExtendUnit.Mbyte;   //文件增长单位
            extendInfo.IsExtendable = true;                 //是否实现自增长
            extendInfo.MaxFileSize = 0;                     //数据库最大容量

            //设置数据库文件信息
            fileInfo.ExtendInfo = extendInfo;               //数据库文件扩展信息
            fileInfo.FilePath = gdbPath;                    //数据库路径
            fileInfo.InitSize = 15;                         //数据库初始大小

            //数据库文件信息列表
            ListDB.Add(fileInfo);


            createParam.GDBName = gdbName;                  //地理数据库的名称
            createParam.GDBOwner = "";                      //用户名称
            createParam.OwnerPsw = "";                      //用户密码
            createParam.DataFileInfos = ListDB;             //数据库文件信息列表
            createParam.IndexFileInfo = fileInfo;           //数据库文件信息         

            //创建数据库
            int rtn = svr.CreateGDB(createParam, LogER);
            if (rtn > 0)
                gdb = svr.OpenGDB(gdbName);
            return gdb;
        }

    }

    /// <summary>
    /// 内部光标
    /// </summary>
    public sealed class MyCursors
    {
        private static Cursor pickup;
        private static Cursor pickupDot;
        private static Cursor inputLine;
        private static Cursor cross;
        private static Cursor move;
        private static Cursor rotate;
        private static Cursor sizeTB;
        private static Cursor sizeLR;
        private static Cursor sizeRTLB;
        private static Cursor sizeLTRB;
        /// <summary>
        /// 拾取光标
        /// </summary>
        public static Cursor Pickup
        {
            get
            {
                if (pickup == null)
                {
                    pickup = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_Pickup));
                }
                return pickup;
            }
        }
        /// <summary>
        /// 拾取点光标
        /// </summary>
        public static Cursor PickupDot
        {
            get
            {
                if (pickupDot == null)
                {
                    pickupDot = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_PickupDot));
                }
                return pickupDot;
            }
        }
        /// <summary>
        /// 输入线
        /// </summary>
        public static Cursor InputLine
        {
            get
            {
                if (inputLine == null)
                {
                    inputLine = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_Cur00004));
                }
                return inputLine;
            }
        }
        /// <summary>
        /// 十字光标
        /// </summary>        
        public static Cursor Cross
        {
            get
            {
                if (cross == null)
                    cross = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_CursorC));
                return cross;
            }
        }
        /// <summary>
        /// 移动光标
        /// </summary>
        public static Cursor Move
        {
            get
            {
                if (move == null)
                    move = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_CursorA));
                return move;
            }
        }
        /// <summary>
        /// 旋转光标
        /// </summary>
        public static Cursor Rotate
        {
            get
            {
                if (rotate == null)
                {
                    rotate = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_HandMove));
                }
                return rotate;
            }
        }
        /// <summary>
        /// 上下调整Size的光标
        /// </summary>
        public static Cursor SizeTB
        {
            get
            {
                if (sizeTB == null)
                    sizeTB = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_Size4M));
                return sizeTB;
            }
        }
        /// <summary>
        /// 左右调整Size的光标
        /// </summary>
        public static Cursor SizeLR
        {
            get
            {
                if (sizeLR == null)
                    sizeLR = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_Size4M));
                return sizeLR;
            }
        }
        /// <summary>
        /// 右上—左下调整Size的光标
        /// </summary>
        public static Cursor SizeRTLB
        {
            get
            {
                if (sizeRTLB == null)
                    sizeRTLB = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_Size4M));
                return sizeRTLB;
            }
        }
        /// <summary>
        /// 左上—右下调整Size的光标
        /// </summary>
        public static Cursor SizeLTRB
        {
            get
            {
                if (sizeLTRB == null)
                    sizeLTRB = new Cursor(new MemoryStream(MapGIS.Desktop.Resources.Cur_Size4M));
                return sizeLTRB;
            }
        }
    }

    public class CommMethod
    {
        /// <summary>
        /// 创建矢量类
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="srcVcls">源矢量类对象</param>
        /// <param name="gdb">地理数据库对象</param>
        /// <param name="geomType">几何类型(创建简单要素类时所用)</param>
        /// <returns></returns>
        public static IVectorCls CreateVectCLS(string URL, IVectorCls srcVcls, out DataBase gdb, GeomType geomType = GeomType.Unknown)
        {
            gdb = null;
            IVectorCls rtnVectCls = null;

            int last1 = URL.LastIndexOf('/');
            if (last1 <= 0) return null;
            string name = URL.Substring(last1 + 1);

            string str = URL.Remove(last1);
            int last2 = str.LastIndexOf('/');
            string strType = str.Substring(last2 + 1);

            XClsType type;
            switch (strType.ToLower())
            {
                case "sfcls":
                    type = XClsType.SFCls;     //简单要素类
                    break;
                case "ocls":
                    type = XClsType.OCls;      //对象类
                    break;
                case "acls":
                    type = XClsType.ACls;      //注记类
                    break;
                default:
                    return null;
            }

            int p1 = URL.IndexOf('/');
            int p2 = URL.IndexOf('/', p1 + 1);
            int p3 = URL.IndexOf('/', p2 + 1);
            int p4 = URL.IndexOf('/', p3 + 1);
            if (p1 < 0 || p2 < 0 || p3 < 0 || p4 < 0) return null;
            string GdbURL = URL.Remove(p4);
            gdb = DataBase.OpenByURL(GdbURL);
            if (gdb == null) return null;

            int dsID = 0;
            // 修改说明：兼容fds标记（平一提供的对ds的可替换标记）
            // 修改人：华凯 2014-05-30
            string dsTag = "/ds/";
            if (URL.IndexOf("/fds/") >= 0)
                dsTag = "/fds/";
            int startDs = URL.IndexOf(dsTag);
            if (startDs != -1)//判断要素数据集
            {
                int endDs = URL.IndexOf("/", startDs + dsTag.Length);
                if (endDs != -1)
                {
                    string dsName = "";
                    endDs = endDs - startDs - dsTag.Length;
                    dsName = URL.Substring(startDs + dsTag.Length, endDs);
                    if (dsName != "")
                    {
                        dsID = gdb.XClsIsExist(XClsType.Fds, dsName);
                        if (dsID <= 0)//不存在，创建要素数据集
                        {
                            dsID = gdb.CreateFds(dsName, 0);
                            if (dsID <= 0)//创建失败
                            {
                                dsID = 0;
                            }
                        }
                    }
                }
            }

            IBasCls BasCls = gdb.GetXClass(type);
            IVectorCls vcls = BasCls as IVectorCls;
            if (vcls != null)
            {
                if (vcls is SFeatureCls)
                {
                    if (srcVcls is SFeatureCls)
                    {
                        SFeatureCls srcSfcls = srcVcls as SFeatureCls;
                        // 修改说明：默认取源数据集合类型，否则根据传入参数创建
                        // 修改人：易师盼 2014-04-18
                        if (geomType == GeomType.Unknown)
                            geomType = srcSfcls.GeomType;
                        // 修改说明：参照系与原数据保持一致
                        // 修改人：易师盼 2019-02-18
                        int srID = srcSfcls.SrID;
                        SFeatureCls sfcls = vcls as SFeatureCls;
                        if (srcSfcls.Fields != null && sfcls.Create(name, geomType, dsID, srID, srcSfcls.Fields.Clone()) > 0)
                        {
                            sfcls.ModelName = srcSfcls.ModelName;
                            rtnVectCls = sfcls;
                        }
                    }
                }
                if (vcls is ObjectCls)
                {
                    if (srcVcls != null)
                    {
                        ObjectCls sfcls = vcls as ObjectCls;
                        if (srcVcls.Fields != null && sfcls.Create(name, dsID, srcVcls.SrID, srcVcls.Fields.Clone()) > 0)
                        {
                            sfcls.ModelName = srcVcls.ModelName;
                            rtnVectCls = sfcls;
                        }
                    }
                }
                if (vcls is AnnotationCls)
                {
                    if (srcVcls != null)
                    {
                        AnnotationCls sfcls = vcls as AnnotationCls;
                        if (srcVcls.Fields != null && sfcls.Create(name, AnnType.Text, dsID, srcVcls.SrID, srcVcls.Fields.Clone()) > 0)
                        {
                            sfcls.ModelName = srcVcls.ModelName;
                            rtnVectCls = sfcls;
                        }
                    }
                }
            }
            return rtnVectCls;
        }
        /// <summary>
        /// 获取激活图层
        /// </summary>
        /// <param name="ctr">场景视图控件</param>
        /// <param name="type">图层类型</param>
        /// <returns>激活图层</returns>
        //public static G3DLayer GetActiveLayer(SceneControl ctr, EditLayerType type)
        //{
        //    G3DLayer layer = null;
        //    if (ctr != null)
        //    {
        //        Scene scene = ctr.GetScene(0);
        //        if (scene != null)
        //        {
        //            List<G3DLayer> layers = scene.GetEditLayer(type, SelectLayerControl.Active);
        //            if (layers != null && layers.Count > 0)
        //            {
        //                layer = layers[0];
        //            }
        //        }
        //    }
        //    return layer;
        //}
        /// <summary>
        /// 获取指定状态图层列表
        /// </summary>
        /// <param name="ctr">场景视图控件</param>
        /// <param name="type">图层类型</param>
        /// <param name="slc">图层状态</param>
        /// <returns>图层列表</returns>
        //public static List<G3DLayer> GetEditLayers(SceneControl ctr, EditLayerType type, SelectLayerControl slc = SelectLayerControl.Editable)
        //{
        //    List<G3DLayer> layers = new List<G3DLayer>();
        //    if (ctr != null)
        //    {
        //        Scene scene = ctr.GetScene(0);
        //        if (scene != null)
        //        {
        //            layers = scene.GetEditLayer(type, slc);
        //            if (layers == null)
        //                layers = new List<G3DLayer>();
        //        }
        //    }
        //    return layers;
        //}
        ///// <summary>
        ///// 清除地形分析工具
        ///// </summary>
        ///// <param name="ctr"></param>
        //public static void ClearTool(SceneControl ctr)
        //{
        //    if (ctr != null)
        //    {
        //        // 修改说明：清除时使用ctr.SetActiveTool(null)（可以进工具的cancel事件，cancel事件处理中实现关闭当前的对话框），代替调用CloseForm的方法
        //        // 修改人：易师盼 2014-05-08
        //        BasTool tool = ctr.GetActiveTool();
        //        if (tool != null)
        //        {
        //            if (tool is TerrainAnalyzeTool)
        //                (tool as TerrainAnalyzeTool).Clear();
        //            tool.Stop();
        //        }
        //        ctr.SetActiveTool(null);
        //        // 修改说明：停止高亮显示,bug12726
        //        // 修改人：易师盼 2019-11-08
        //        ctr.StopCustomDisplay();
        //    }
        //}
        ///// <summary>
        ///// 通用单选界面
        ///// </summary>
        ///// <param name="selSet"></param>
        ///// <param name="mapCtrl"></param>
        ///// <param name="layer"></param>
        ///// <param name="oid"></param>
        ///// <returns></returns>
        //public static bool SingleSelect(Select3DSet selSet, SceneControl sceneCtrl, bool bUpdateSelSet, out G3DLayer layer, out long oid)
        //{
        //    bool bRtn = false;
        //    layer = null;
        //    oid = -1;
        //    if (selSet != null)
        //    {
        //        if (CommMethod.GetSelectCount(selSet) > 1)
        //        {
        //            SingleSelectOidForm ssof = new SingleSelectOidForm(selSet, sceneCtrl);
        //            if (ssof.ShowDialog() == DialogResult.OK)
        //            {
        //                layer = ssof.Layer;
        //                oid = ssof.Oid;
        //                if (bUpdateSelSet)
        //                {
        //                    selSet.Clear();
        //                    List<long> ids = new List<long>();
        //                    ids.Add(oid);
        //                    selSet.Append(layer, ids);
        //                }
        //                bRtn = true;
        //            }
        //            else
        //            {
        //                if (bUpdateSelSet)
        //                {
        //                    selSet.Clear();
        //                }
        //            }
        //        }
        //        else if (CommMethod.GetSelectCount(selSet) == 1)
        //        {
        //            Select3DSetItem item = selSet.Get()[0];
        //            layer = item.Layer;
        //            if (item.IDList != null && item.IDList.Count == 1)
        //                oid = item.IDList[0];
        //            item.Dispose();
        //            bRtn = true;
        //        }
        //    }
        //    return bRtn;
        //}

        /// <summary>
        /// 返回选择集个数
        /// </summary>
        /// <param name="selSet"></param>
        /// <returns></returns>
        public static int GetSelectCount(Select3DSet selSet)
        {
            int count = 0;
            if (selSet != null)
            {
                List<Select3DSetItem> lst = selSet.Get();
                if (lst != null && lst.Count > 0)
                {
                    foreach (Select3DSetItem item in lst)
                    {
                        count += item.IDList.Count;
                        item.Layer.Dispose();
                    }
                }
                lst.Clear();
                lst = null;
            }
            return count;
        }
        /// <summary>
        /// 获取控件中第一个场景
        /// </summary>
        /// <param name="ctr"></param>
        /// <returns></returns>
        //public static Scene GetScene(SceneControl ctr)
        //{
        //    Scene scene = null;
        //    if (ctr != null)
        //        scene = ctr.GetScene(0);
        //    return scene;
        //}
        /// <summary>
        /// 按模型层来过滤选择集
        /// </summary>
        /// <param name="selSet"></param>
        /// <param name="bUpdateSelSet"></param>
        /// <returns></returns>
        public static Select3DSet FilterSelectSetByModel(Select3DSet selSet, bool bUpdateSelSet = true)
        {
            Select3DSet rtnSet = null;
            if (selSet != null)
            {
                rtnSet = new Select3DSet();
                List<Select3DSetItem> items = selSet.Get();
                if (items != null && items.Count > 0)
                {
                    List<Select3DSetItem> newItems = new List<Select3DSetItem>();
                    foreach (Select3DSetItem item in items)
                    {
                        if (item.Layer is ModelLayer)
                        {
                            newItems.Add(item);
                            rtnSet.Append(item.Layer, item.IDList);
                        }
                    }
                    if (bUpdateSelSet)
                    {
                        selSet.Clear();
                        foreach (Select3DSetItem item in newItems)
                        {
                            if (item.Layer is ModelLayer)
                                selSet.Append(item.Layer, item.IDList);
                        }
                    }
                }
            }
            return rtnSet;
        }
        /// <summary>
        /// 根据几何类型创建缺省参数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GeomInfo CreateDefaultInfo(GeomType type)
        {
            GeomInfo info = null;
            switch (type)
            {
                case GeomType.Pnt:
                    PntInfo ptInfo = new PntInfo();
                    ptInfo.SymID = 6;
                    ptInfo.Width = 20;
                    ptInfo.Height = 20;
                    ptInfo.Angle = 0;
                    ptInfo.OutClr = new int[] { 3, 4, 5 };
                    ptInfo.Ovprnt = true;
                    info = ptInfo;
                    break;
                case GeomType.Lin:
                    LinInfo lInfo = new LinInfo();
                    lInfo.LinStyID = 1;
                    lInfo.OutClr = new int[] { 3, 4, 5 };
                    lInfo.OutPenW = new float[] { 0.05F, 0.05F, 0.05F };
                    lInfo.XScale = 10;
                    lInfo.YScale = 10;
                    lInfo.Ovprnt = true;
                    info = lInfo;
                    break;
                case GeomType.Reg:
                    RegInfo rInfo = new RegInfo();
                    rInfo.PatID = 0;
                    rInfo.FillClr = 2;
                    rInfo.PatHeight = 20;
                    rInfo.PatWidth = 20;
                    rInfo.OutPenW = 1.0F;
                    rInfo.Angle = 0;
                    rInfo.PatClr = 3;
                    rInfo.FillMode = 0;
                    info = rInfo;
                    break;
                case GeomType.Ann:
                    TextAnnInfo aInfo = new TextAnnInfo();
                    aInfo.Color = 7;
                    aInfo.Height = 14;
                    aInfo.Width = 14;
                    aInfo.Ovprnt = true;
                    info = aInfo;
                    break;
                case GeomType.Surface:
                    SurfaceInfo surInfo = new SurfaceInfo();
                    // 修改说明：区分二三维无效符号标识，符号ID为-1表示三维的无效，0表示二维的无效
                    // 修改人：易师盼 2016-11-04
                    surInfo.PatID = -1;
                    //surInfo.PatID = 30000001;
                    surInfo.FillColor = 2;
                    surInfo.TransparentColor = 0;
                    surInfo.TextureScale = 1;
                    surInfo.TextureScaleY = 1;
                    info = surInfo;
                    break;
                case GeomType.Entity:
                    EntityInfo entityInfo = new EntityInfo();
                    // 修改说明：区分二三维无效符号标识，符号ID为-1表示三维的无效，0表示二维的无效
                    // 修改人：易师盼 2016-11-04
                    entityInfo.PatID = -1;
                    //entityInfo.PatID = 30000001;
                    entityInfo.FillColor = 2;
                    entityInfo.TransparentColor = 0;
                    entityInfo.TextureScale = 1;
                    entityInfo.TextureScaleY = 1;
                    info = entityInfo;
                    break;
            }
            return info;
        }
        /// <summary>
        /// 根据简单要素类所在的系统库查找rgb对应的颜色来创建图像参数
        /// </summary>
        /// <param name="sfcls"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GeomInfo CreateInfo(SFeatureCls sfcls, int r, int g, int b)
        {
            GeomInfo info = null;
            if (sfcls != null && sfcls.HasOpen())
            {
                info = CreateDefaultInfo(sfcls.GeomType);
                ResetInfoColor(sfcls, info, r, g, b);
            }
            return info;
        }
        /// <summary>
        /// 根据简单要素类所在的系统库查找rgb对应的颜色来创建图像参数
        /// </summary>
        /// <param name="sfcls"></param>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static GeomInfo CreateInfo(SFeatureCls sfcls, System.Drawing.Color rgb)
        {
            return CreateInfo(sfcls, rgb.R, rgb.G, rgb.B);
        }
        /// <summary>
        /// 将符号填充色设置为rgb对应的颜色
        /// </summary>
        /// <param name="sfcls">该图像参数对应的系统库</param>
        /// <param name="info"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns>true成功改变了填充色，其他为false</returns>
        public static bool ResetInfoColor(SFeatureCls sfcls, GeomInfo info, int r, int g, int b)
        {
            bool bRtn = false;
            if (sfcls != null && info != null)
            {
                SystemLibrary lib = GetSystemLib(sfcls);
                if (lib != null)
                {
                    int clr = FindColor(lib.GetColorLibrary(), r, g, b);
                    if (clr >= 0)
                    {
                        if (info is PntInfo)
                        {
                            int[] outClr = (info as PntInfo).OutClr;
                            if (outClr != null && outClr.Length >= 1)
                                outClr[0] = clr;
                            else
                                outClr = new int[] { clr, 4, 5 };
                            (info as PntInfo).OutClr = outClr;
                            bRtn = true;
                        }
                        if (info is LinInfo)
                        {
                            int[] outClr = (info as LinInfo).OutClr;
                            if (outClr != null && outClr.Length >= 1)
                                outClr[0] = clr;
                            else
                                outClr = new int[] { clr, 4, 5 };
                            (info as LinInfo).OutClr = outClr;
                            bRtn = true;
                        }
                        if (info is RegInfo)
                        {
                            (info as RegInfo).FillClr = clr;
                            bRtn = true;
                        }
                        if (info is TextAnnInfo)
                        {
                            (info as TextAnnInfo).Color = clr;
                            bRtn = true;
                        }
                        if (info is SurfaceInfo)
                        {
                            (info as SurfaceInfo).FillColor = clr;
                            bRtn = true;
                        }
                        if (info is EntityInfo)
                        {
                            (info as EntityInfo).FillColor = clr;
                            bRtn = true;
                        }
                    }
                }
            }
            return bRtn;
        }
        /// <summary>
        /// 将符号填充色设置为rgb对应的颜色
        /// </summary>
        /// <param name="sfcls"></param>
        /// <param name="info"></param>
        /// <param name="rgb"></param>
        /// <returns>true成功改变了填充色，其他为false</returns>
        public static bool ResetInfoColor(SFeatureCls sfcls, GeomInfo info, System.Drawing.Color rgb)
        {
            return ResetInfoColor(sfcls, info, rgb.R, rgb.G, rgb.B);
        }
        /// <summary>
        /// 获取简单要素类关联的系统库
        /// </summary>
        /// <param name="sfcls"></param>
        /// <returns></returns>
        public static SystemLibrary GetSystemLib(SFeatureCls sfcls)
        {
            SystemLibrary lib = null;
            if (sfcls != null && sfcls.HasOpen())
            {
                string name = sfcls.ModelName;
                Guid guid = Guid.Empty;
                if (!string.IsNullOrEmpty(name))
                {
                    try { guid = new Guid(name); }
                    catch { }
                }
                SystemLibrarys libs = SystemLibrarys.GetSystemLibrarys();
                if (libs != null)
                    lib = libs.GetSystemLibrary(guid);
            }
            return lib;
        }
        /// <summary>
        /// 在指定的颜色库中查找指定RGB对应的颜色号
        /// </summary>
        /// <param name="clrLib"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns>颜色库不存在则返回-1，其他的返回>=0</returns>
        public static int FindColor(ColorLibrary clrLib, int r, int g, int b)
        {
            int rtn = -1;
            if (clrLib != null)
                rtn = clrLib.FindNearColorNoByRgb((byte)r, (byte)g, (byte)b);
            return rtn;
        }
        /// <summary>
        /// 三维数据更新带有符号编号的图形参数前须检查修复纹理坐标,将三维数据中所有的UpdateInfo全部替换成UpdateInfoEx（给图元设置图形参数时，如果符号图案有效，须检查几何图形是否有纹理坐标，没有的话要进行修复，否则会报数据异常）
        /// </summary>
        /// <param name="sfcls"></param>
        /// <param name="oid"></param>
        /// <param name="info"></param>
        public static void UpdateInfoEx(IVectorCls vCls, long oid, GeomInfo info)
        {
            if (vCls != null && oid > 0)
            {
                bool flag1 = info is SurfaceInfo && (info as SurfaceInfo).PatID > 0;
                bool flag2 = info is EntityInfo && (info as EntityInfo).PatID > 0;
                if (flag1 || flag2)
                {
                    bool flag = false;
                    IGeometry3D geomtry = vCls.GetGeometry(oid) as IGeometry3D;
                    GeomInfo oldInfo = vCls.GetInfo(oid);
                    if (geomtry != null && oldInfo != null)
                    {
                        // 修改说明：当符号ID有效时，如果要素中的每个anySurface都是没有纹理坐标的，就进行纹理坐标的默认计算，同时symIndexList里面由-1设置为0
                        // 修改人：易师盼 2016-10-19
                        if (info is SurfaceInfo)
                        {
                            #region 面
                            bool invalidToValid = (oldInfo as SurfaceInfo).PatID < 1;
                            SurfaceInfo surInfo = info as SurfaceInfo;
                            if (geomtry is GeoAnySurface)
                            {
                                int symIndex = 0;
                                //InverseTextureCoorMap在数据计算纹理坐标前调用，与CalcTextureCoor成对出现
                                (geomtry as GeoAnySurface).InverseTextureCoorMap(oldInfo as SurfaceInfo, ref symIndex);
                                if (invalidToValid)
                                {
                                    flag = true;
                                    (geomtry as GeoAnySurface).CalcTextureCoor();
                                    if (symIndex == -1)
                                        symIndex = 0;
                                }
                                (geomtry as GeoAnySurface).CalcTextureCoorMap(symIndex, surInfo);
                            }
                            else if (geomtry is GeoMultiSurface)
                            {
                                GeoMultiSurface geoMultiSurface = geomtry as GeoMultiSurface;
                                List<int> symIndexList = new List<int>();
                                geoMultiSurface.InverseTextureCoorMap(oldInfo as SurfaceInfo, ref symIndexList);
                                if (invalidToValid)
                                {
                                    int num = geoMultiSurface.GetNum();
                                    for (int i = 0; i < num; i++)
                                    {
                                        GeoAnySurface geoAnySurface = geoMultiSurface.Get(i);
                                        flag = true;
                                        geoAnySurface.CalcTextureCoor();
                                        if (i < symIndexList.Count && symIndexList[i] == -1)
                                            symIndexList[i] = 0;
                                    }
                                }
                                geoMultiSurface.CalcTextureCoorMap(symIndexList, surInfo);
                            }
                            #endregion
                        }
                        else if (info is EntityInfo)
                        {
                            #region 体
                            bool invalidToValid = (oldInfo as EntityInfo).PatID < 1;
                            EntityInfo entityInfo = info as EntityInfo;
                            if (geomtry is GeoAnyEntity)
                            {
                                GeoAnyEntity geoAnyEntity = geomtry as GeoAnyEntity;
                                List<int> symIndexList = new List<int>();
                                geoAnyEntity.InverseTextureCoorMap(oldInfo as EntityInfo, ref symIndexList);
                                if (invalidToValid)
                                {
                                    int num = geoAnyEntity.GetSurfaceNum();
                                    for (int i = 0; i < num; i++)
                                    {
                                        GeoAnySurface geoAnySurface = geoAnyEntity.GetSurface(i);
                                        flag = true;
                                        geoAnySurface.CalcTextureCoor();
                                        if (i < symIndexList.Count && symIndexList[i] == -1)
                                            symIndexList[i] = 0;
                                    }
                                }
                                geoAnyEntity.CalcTextureCoorMap(symIndexList, entityInfo);
                            }
                            else if (geomtry is GeoMultiEntity)
                            {
                                GeoMultiEntity geoMultiEntity = geomtry as GeoMultiEntity;
                                List<int> symIndexList = new List<int>();
                                geoMultiEntity.InverseTextureCoorMap(oldInfo as EntityInfo, ref symIndexList);
                                if (invalidToValid)
                                {
                                    int num1 = geoMultiEntity.GetNum();
                                    int num = 0;
                                    for (int i = 0; i < num1; i++)
                                    {
                                        GeoAnyEntity geoAnyEntity = geoMultiEntity.Get(i);
                                        if (geoAnyEntity != null)
                                        {
                                            int num2 = geoAnyEntity.GetSurfaceNum();
                                            for (int j = 0; j < num2; j++)
                                            {
                                                GeoAnySurface geoAnySurface = geoAnyEntity.GetSurface(j);
                                                flag = true;
                                                geoAnySurface.CalcTextureCoor();
                                                if (num < symIndexList.Count && symIndexList[num] == -1)
                                                    symIndexList[num] = 0;
                                                num++;
                                            }
                                        }
                                    }
                                }
                                geoMultiEntity.CalcTextureCoorMap(symIndexList, entityInfo);
                            }
                            #endregion
                        }
                    }
                    if (flag)
                    {
                        //MapGIS.GeoObjects.Att.Record rcd = vCls.GetAtt(oid);
                        //vCls.Update(oid, geomtry, rcd, info);
                        vCls.Update(oid, geomtry, null, info);
                    }
                    else
                        vCls.UpdateInfo(oid, info);
                }
                else
                {
                    // 修改说明：当符号改成-1时，删掉纹理坐标
                    // 修改人：易师盼 2017-05-04
                    bool flag = false;
                    IGeometry3D geomtry = vCls.GetGeometry(oid) as IGeometry3D;
                    GeomInfo oldInfo = vCls.GetInfo(oid);
                    if (geomtry != null && oldInfo != null)
                    {
                        if (oldInfo is SurfaceInfo)
                        {
                            #region 面
                            bool invalidToValid = (oldInfo as SurfaceInfo).PatID > 0;
                            if (geomtry is GeoAnySurface)
                            {
                                if (invalidToValid)
                                {
                                    if ((geomtry as GeoAnySurface).HasTexturePosition() > 0)
                                    {
                                        (geomtry as GeoAnySurface).DelTexturePosition();
                                        flag = true;
                                    }
                                }
                            }
                            else if (geomtry is GeoMultiSurface)
                            {
                                GeoMultiSurface geoMultiSurface = geomtry as GeoMultiSurface;
                                int num = geoMultiSurface.GetNum();
                                if (invalidToValid)
                                {
                                    for (int i = 0; i < num; i++)
                                    {
                                        GeoAnySurface geoAnySurface = geoMultiSurface.Get(i);
                                        if (geoAnySurface.HasTexturePosition() > 0)
                                        {
                                            geoAnySurface.DelTexturePosition();
                                            flag = true;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (oldInfo is EntityInfo)
                        {
                            #region 体
                            bool invalidToValid = (oldInfo as EntityInfo).PatID > 1;
                            EntityInfo entityInfo = info as EntityInfo;
                            if (geomtry is GeoAnyEntity)
                            {
                                GeoAnyEntity geoAnyEntity = geomtry as GeoAnyEntity;
                                int num = geoAnyEntity.GetSurfaceNum();
                                if (invalidToValid)
                                {
                                    for (int i = 0; i < num; i++)
                                    {
                                        GeoAnySurface geoAnySurface = geoAnyEntity.GetSurface(i);
                                        if (geoAnySurface.HasTexturePosition() > 0)
                                        {
                                            geoAnySurface.DelTexturePosition();
                                            flag = true;
                                        }
                                    }
                                }
                            }
                            else if (geomtry is GeoMultiEntity)
                            {
                                GeoMultiEntity geoMultiEntity = geomtry as GeoMultiEntity;
                                int num1 = geoMultiEntity.GetNum();
                                if (invalidToValid)
                                {
                                    for (int i = 0; i < num1; i++)
                                    {
                                        GeoAnyEntity geoAnyEntity = geoMultiEntity.Get(i);
                                        if (geoAnyEntity != null)
                                        {
                                            int num2 = geoAnyEntity.GetSurfaceNum();
                                            for (int j = 0; j < num2; j++)
                                            {
                                                GeoAnySurface geoAnySurface = geoAnyEntity.GetSurface(j);
                                                if (geoAnySurface.HasTexturePosition() > 0)
                                                {
                                                    geoAnySurface.DelTexturePosition();
                                                    flag = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    if (flag)
                        vCls.Update(oid, geomtry, null, info);
                    else
                        vCls.UpdateInfo(oid, info);
                }
            }
        }
        /// <summary>
        /// 判断给定的选择集是否为空
        /// </summary>
        /// <param name="selSet">选择集</param>
        /// <returns>true—空，false-非空</returns>
        public static bool IsSelSetNullOrEmpty(Select3DSet selSet)
        {
            bool isEmpty = true;
            if (selSet != null)
            {
                List<Select3DSetItem> items = selSet.Get();
                isEmpty = items == null || items.Count == 0;
            }
            return isEmpty;
        }
        /// <summary>
        /// 计算三维选择集的三维矩形范围
        /// </summary>
        /// <param name="selSet">三维选择集</param>
        /// <returns>三维选择集的三维矩形范围</returns>
        public static Rect3D CalRange(Select3DSet selSet)
        {
            Rect3D rtnRect = null;
            if (selSet != null)
            {
                List<Select3DSetItem> selItems = selSet.Get();
                foreach (Select3DSetItem item in selItems)
                {
                    ModelLayer layer = item.Layer as ModelLayer;
                    List<long> idList = item.IDList;
                    if (layer != null && idList != null && idList.Count > 0)
                    {
                        SFeatureCls sfCls = layer.GetData() as SFeatureCls;
                        if (sfCls != null)
                        {
                            foreach (long id in idList)
                            {
                                Rect3D rect = sfCls.GetRect3D(id);
                                if (rtnRect == null)
                                    rtnRect = rect.Clone();
                                else
                                {
                                    rtnRect.XMin = Math.Min(rect.XMin, rtnRect.XMin);
                                    rtnRect.YMin = Math.Min(rect.YMin, rtnRect.YMin);
                                    rtnRect.ZMin = Math.Min(rect.ZMin, rtnRect.ZMin);
                                    rtnRect.XMax = Math.Max(rect.XMax, rtnRect.XMax);
                                    rtnRect.YMax = Math.Max(rect.YMax, rtnRect.YMax);
                                    rtnRect.ZMax = Math.Max(rect.ZMax, rtnRect.ZMax);
                                }
                            }
                        }
                    }
                }
            }
            return rtnRect;
        }
        /// <summary>
        /// 计算三维选择集的三维矩形中心点
        /// </summary>
        /// <param name="selSet">三维选择集</param>
        /// <returns>计算三维选择集的三维矩形中心点</returns>
        public static Dot3D CalCenter(Select3DSet selSet)
        {
            Dot3D dot = null;
            Rect3D rect = CommMethod.CalRange(selSet);
            if (rect != null)
                dot = new Dot3D((rect.XMax + rect.XMin) / 2, (rect.YMax + rect.YMin) / 2, (rect.ZMax + rect.ZMin) / 2);
            return dot;
        }
        /// <summary>
        /// 计算三维选择集的三维矩形中心点
        /// </summary>
        /// <param name="selSet">三维选择集</param>
        /// <returns>计算三维选择集的三维矩形中心点</returns>
        public static Dot3D CalCenter(SFeatureCls sfCls)
        {
            Dot3D dot = null;
            if (sfCls != null)
            {
                Rect3D rect = sfCls.GetRange3D();
                if (rect != null)
                    dot = new Dot3D((rect.XMax + rect.XMin) / 2, (rect.YMax + rect.YMin) / 2, (rect.ZMax + rect.ZMin) / 2);
            }
            return dot;
        }
        /// <summary>
        /// 计算点集中心点
        /// </summary>
        /// <param name="dts"></param>
        /// <returns></returns>
        public static Dot3D CalCenter(Dots3D dts)
        {
            Dot3D dt = null;
            if (dts != null && dts.Count > 0)
            {
                Dot3D tmpDt = dts.GetItem(0);
                if (tmpDt != null)
                {
                    double xmin = tmpDt.X;
                    double ymin = tmpDt.Y;
                    double zmin = tmpDt.Z;
                    double xmax = tmpDt.X;
                    double ymax = tmpDt.Y;
                    double zmax = tmpDt.Z;

                    #region 取外包范围
                    int count = dts.Count;
                    for (int i = 1; i < count; i++)
                    {
                        tmpDt = dts.GetItem(i);
                        if (xmin > tmpDt.X)
                        {
                            xmin = tmpDt.X;
                        }
                        else if (xmax < tmpDt.X)
                        {
                            xmin = tmpDt.X;
                        }
                        if (ymin > tmpDt.Y)
                        {
                            ymin = tmpDt.Y;
                        }
                        else if (ymax < tmpDt.Y)
                        {
                            ymax = tmpDt.Y;
                        }
                        if (zmin > tmpDt.Z)
                        {
                            zmin = tmpDt.Z;
                        }
                        else if (zmax < tmpDt.Z)
                        {
                            zmax = tmpDt.Z;
                        }
                    }
                    #endregion

                    dt = new Dot3D((xmin + xmax) / 2, (ymin + ymax) / 2, (zmin + zmax) / 2);
                }
            }
            return dt;
        }
        /// <summary>
        /// 比较两个字段是否相等（比较名称、类型、长度）
        /// </summary>
        /// <param name="fld1">第一个字段</param>
        /// <param name="fld2">第二个字段</param>
        /// <returns>true-相等，FALSE—不相等</returns>
        public static bool CompareFields(Field fld1, Field fld2)
        {
            bool isEqual = false;
            if (fld1 == null && fld2 == null)
                isEqual = true;
            else if (fld1 != null && fld2 != null)
                isEqual = (fld1.FieldName == fld2.FieldName && fld1.FieldType == fld2.FieldType && fld1.MskLength == fld2.MskLength);
            return isEqual;
        }
        /// <summary>
        /// 比较两个字段是否相等（比较名称、类型、长度）
        /// </summary>
        /// <param name="fld1">第一个字段</param>
        /// <param name="fld2">第二个字段</param>
        /// <returns>true-相等，FALSE—不相等</returns>
        public static bool FieldsContains(Fields flds, Field fld)
        {
            bool contains = false;
            if (flds != null && fld != null)
            {
                for (int i = 0; i < flds.Count; i++)
                {
                    Field tempFld = flds.GetItem(i);
                    if (CommMethod.CompareFields(tempFld, fld))
                    {
                        contains = true;
                        break;
                    }
                }
            }
            return contains;
        }
        /// <summary>
        /// 过滤出给定属性结构中的数值型字段
        /// </summary>
        /// <param name="flds">属性结构</param>
        /// <returns>数值型字段名称集合</returns>
        public static List<string> FilterNumericFields(Fields flds)
        {
            List<string> list = null;
            if (flds != null)
            {
                list = new List<string>();
                for (int i = 0; i < flds.Count; i++)
                {
                    Field fld = flds.GetItem(i);
                    if (fld != null)
                    {
                        switch (fld.FieldType)
                        {
                            case FieldType.FldByte:
                            case FieldType.FldShort:
                            case FieldType.FldLong:
                            case FieldType.FldInt64:
                            case FieldType.FldFloat:
                            case FieldType.FldDouble:
                            case FieldType.FldNumberic:
                                list.Add(fld.FieldName);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 过滤出给定属性结构中的字符串型字段
        /// </summary>
        /// <param name="flds">属性结构</param>
        /// <returns>字符串型字段名称集合</returns>
        public static List<string> FilterStringFields(Fields flds)
        {
            List<string> list = null;
            if (flds != null)
            {
                list = new List<string>();
                for (int i = 0; i < flds.Count; i++)
                {
                    Field fld = flds.GetItem(i);
                    if (fld != null)
                    {
                        switch (fld.FieldType)
                        {
                            case FieldType.FldString:
                                list.Add(fld.FieldName);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return list;
        }
        public static ModelCacheLayer HasModelCacheLayer(Scene scene, LayerState state = LayerState.Visible, ModelDataType type = ModelDataType.Oblique)
        {
            ModelCacheLayer layer = null;
            if (scene != null)
            {
                int count = scene.LayerCount;
                for (int i = 0; i < count; i++)
                {
                    G3DLayer glayer = scene.GetLayer(i);
                    if ((glayer is ModelCacheLayer) && ((glayer.State & state) == state) && (glayer as ModelCacheLayer).GetDataType() == type)
                        layer = glayer as ModelCacheLayer;
                    else if (glayer is Group3DLayer)
                        layer = GetModelCacheLayer(glayer as Group3DLayer);
                    if (layer != null)
                        break;
                }
            }
            return layer;
        }
        public static ModelCacheLayer GetModelCacheLayer(Group3DLayer group3DLayer, LayerState state = LayerState.Visible, ModelDataType type = ModelDataType.Oblique)
        {
            ModelCacheLayer layer = null;
            if (group3DLayer != null)
            {
                int count = group3DLayer.Count;
                for (int i = 0; i < count; i++)
                {
                    G3DLayer glayer = group3DLayer.GetLayer(i);
                    if ((glayer is ModelCacheLayer) && ((glayer.State & state) == state) && (glayer as ModelCacheLayer).GetDataType() == type)
                        layer = glayer as ModelCacheLayer;
                    else if (glayer is Group3DLayer)
                        layer = GetModelCacheLayer(glayer as Group3DLayer);
                    if (layer != null)
                        break;
                }
            }
            return layer;
        }

        /// <summary>
        /// 移动简单要素类
        /// </summary>
        /// <param name="sfCls">简单要素类</param>
        /// <param name="dx">X方向移动距离</param>
        /// <param name="dy">Y方向移动距离</param>
        /// <param name="dz">Z方向移动距离</param>
        //public static void MoveSFeatureCls(SFeatureCls sfCls, double dx, double dy, double dz)
        //{
        //    if (sfCls != null)
        //    {
        //        QueryDef qd = new QueryDef();
        //        qd.CursorType = SetCursorType.ForwardOnly;
        //        RecordSet rs = sfCls.Select(qd);
        //        if (rs != null)
        //        {
        //            rs.MoveFirst();
        //            while (!rs.IsEOF)
        //            {
        //                IGeometry3D tempGeom = ModelMoveTool.MovingGeometry(rs.Geometry as IGeometry3D, dx, dy, dz);
        //                if (tempGeom != null)
        //                    sfCls.Update(rs.CurrentID, tempGeom, rs.Att, rs.Info);

        //                rs.MoveNext();
        //            }
        //            rs.Dispose();
        //            sfCls.CalRange();
        //        }
        //    }
        //}
        ///// <summary>
        ///// 旋转选择集
        ///// </summary>
        ///// <param name="sfCls">简单要素类</param>
        ///// <param name="center">旋转中心点</param>
        ///// <param name="angleX">X方向旋转角度</param>
        ///// <param name="angleY">Y方向旋转角度</param>
        ///// <param name="angleZ">Z方向旋转角度</param>
        //public static void RotateSFeatureCls(SFeatureCls sfCls, Dot3D center, double angleX, double angleY, double angleZ)
        //{
        //    if (sfCls != null)
        //    {
        //        QueryDef qd = new QueryDef();
        //        qd.CursorType = SetCursorType.ForwardOnly;
        //        RecordSet rs = sfCls.Select(qd);
        //        if (rs != null)
        //        {
        //            rs.MoveFirst();
        //            while (!rs.IsEOF)
        //            {
        //                IGeometry3D tempGeom = ModelRotateTool.RotatingGeometry(rs.Geometry as IGeometry3D, center, angleX, angleY, angleZ);
        //                if (tempGeom != null)
        //                    sfCls.Update(rs.CurrentID, tempGeom, rs.Att, rs.Info);

        //                rs.MoveNext();
        //            }
        //            rs.Dispose();
        //            sfCls.CalRange();
        //        }
        //    }
        //}

        ///// <summary>
        ///// 三点构面
        ///// </summary>
        ///// <param name="dt1"></param>
        ///// <param name="dt2"></param>
        ///// <param name="dt3"></param>
        ///// <returns></returns>
        //public static GeoAnySurface GreateSurface(Dot3D dt1, Dot3D dt2, Dot3D dt3)
        //{
        //    GeoAnySurface surface = new GeoAnySurface();
        //    Dots3D dots = new Dots3D();
        //    dots.Append(dt1);
        //    dots.Append(dt2);
        //    dots.Append(dt3);

        //    uint[] tri = new uint[]{0,1,2};
        //    //tri[0] = 0;
        //    //tri[1] = 1;
        //    //tri[2] = 2;
        //    //tri[3] = 0;
        //    //tri[4] = 2;
        //    //tri[5] = 3;
        //    surface.Set(dots, tri, null, null, null, 0, null);
        //    return surface;
        //}
    }
}
