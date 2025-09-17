/*----------------------------------------------------------------
// Copyright (C) 2019 中地数码科技有限公司
// 版权所有。
//
// 文件名：ProjectManage
// 文件功能描述：
//
//
// 创建标识：韩威 2019/3/14 15:37:02
//----------------------------------------------------------------*/

using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using System.Collections.Generic;

namespace MapGISPlugin3
{
    public delegate void OpenPrjChange(object openPrj);

    public delegate void ClosingProjectHandle(object sender, object prj, bool IsCancel);

    public delegate void ClosedProjectHandle(object sender);

    public delegate void OpeningProjectHandle(object sender, object prj, bool IsCancel);

    public delegate void OpenedProjectHandle(object sender, object prj);

    /// <summary>
    /// 工程管理类，本项目只有一个工程管理对象,一次只能打开一个工程
    /// </summary>
    public class ProjectManage
    {
        private static ProjectManage prjManage;
        private static readonly object syncRoot = new object();

        public event OpenPrjChange prjChange;

        private Dictionary<string, ProjectManageItem> projectManages = new Dictionary<string, ProjectManageItem>();

        public int ProjectManageItemCount
        {
            get { return projectManages.Count; }
        }

        public bool AddProjectManageItem(string key, ProjectManageItem item)
        {
            if (string.IsNullOrWhiteSpace(key) || projectManages.ContainsKey(key))
                return false;
            if (item == null)
                return false;
            projectManages.Add(key, item);

            item.ClosingProject += Item_ClosingProject;
            item.ClosedProject += Item_ClosedProject;
            item.OpeningProject += Item_OpeningProject;
            item.OpenedProject += Item_OpenedProject;
            return true;
        }

        public ProjectManageItem GetProjectManageItem(string key)
        {
            if (string.IsNullOrWhiteSpace(key) && projectManages.ContainsKey(key))
                return projectManages[key];
            else
                return null;
        }

        private void Item_OpenedProject(object sender, object prj)
        {
            int val = 1;
            if (InitPlugin.App != null && InitPlugin.App.Document != null)
            {
                foreach (ProjectManageItem item in projectManages.Values)
                {
                    if (item.IsOpen)
                    {
                        if (item.Doc != null && item.Doc.Handle == InitPlugin.App.Document.Handle)
                            val = 2;
                        break;
                    }
                }
            }
            if (prjChange != null)
                prjChange(OpenPrj);
        }

        private void Item_OpeningProject(object sender, object prj, bool IsCancel)
        {
            if (!IsCancel)
            {
                foreach (ProjectManageItem item in projectManages.Values)
                {
                    if (item != null && item != sender && item.IsOpen)
                    {
                        if (item.CloseProject() < 0)
                        {
                            IsCancel = true;
                            return;
                        }
                    }
                }
            }
        }

        private void Item_ClosedProject(object sender)
        {
            if (prjChange != null)
                prjChange(null);
        }

        private void Item_ClosingProject(object sender, object prj, bool IsCancel)
        {
        }

        public bool RemoveProjectManageItem(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            if (projectManages.ContainsKey(key))
                projectManages.Remove(key);
            return true;
        }

        public object OpenPrj
        {
            get
            {
                foreach (ProjectManageItem item in projectManages.Values)
                {
                    if (item.IsOpen)
                        return item.OpenPrj;
                }
                return null;
            }
        }

        public Document Doc
        {
            get
            {
                foreach (ProjectManageItem item in projectManages.Values)
                {
                    if (item.IsOpen)
                        return item.Doc;
                }
                if (InitPlugin.App != null)
                    return InitPlugin.App.Document;
                return null;
            }
        }

        public object GetOpenPrjExpandInfo(string key)
        {
            foreach (ProjectManageItem item in projectManages.Values)
            {
                if (item.IsOpen)
                {
                    return item.GetExpandInfo(key);
                }
            }
            return null;
        }

        public static ProjectManage GetInstance()
        {
            if (prjManage == null)
            {
                lock (syncRoot)
                {
                    if (prjManage == null)
                    {
                        prjManage = new ProjectManage();
                    }
                }
            }
            return prjManage;
        }

        public int OpenFile(string key, object prjParam)
        {
            if (projectManages.ContainsKey(key))
            {
                return projectManages[key].OpenProject(prjParam);
            }
            return 0;
        }

        public bool CloseProject()
        {
            bool rtn = false;

            bool existOpen = false;
            foreach (ProjectManageItem item in projectManages.Values)
            {
                if (item.IsOpen)
                {
                    existOpen = true;
                    rtn = item.CloseProject() > 0;
                    break;
                }
            }
            if (!existOpen && InitPlugin.App != null)
            {
                rtn = InitPlugin.App.Document.Close(false);
            }
            return rtn;
        }

        private ProjectManage()
        {
        }

        ~ProjectManage()
        {
            foreach (ProjectManageItem item in projectManages.Values)
            {
                if (item != null && item.IsOpen)
                {
                    item.CloseProject();
                }
            }
        }
    }

    public abstract class ProjectManageItem
    {
        public abstract bool IsOpen { get; }
        public abstract Document Doc { get; }
        public abstract object OpenPrj { get; }

        public abstract int OpenProject(object prjParam);

        public abstract int CloseProject();

        public abstract object GetExpandInfo(string key);

        public abstract event ClosingProjectHandle ClosingProject;

        public abstract event ClosedProjectHandle ClosedProject;

        public abstract event OpeningProjectHandle OpeningProject;

        public abstract event OpenedProjectHandle OpenedProject;

        //关闭文档之前，先清除选择集，不然会影响文件关闭效率
        protected void BeforeCloseDocment(Document document)
        {
            if (document == null)
                return;
            Maps maps = document.GetMaps();
            if (maps == null || maps.Count <= 0)
                return;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps.GetMap(i);
                if (map == null)
                    continue;
                SelectSet set = map.GetSelectSet();
                if (set != null && set.GetCount() > 0)
                    set.Clear();
                DataBase gdb = DataBase.OpenTempDB();
                LayerEnum layerEnum = map.LayerEnum;
                if (layerEnum != null && gdb != null)
                {
                    layerEnum.MoveToFirst();
                    MapLayer layer = null;
                    List<MapLayer> layers = new List<MapLayer>();
                    while ((layer = layerEnum.Next()) != null)
                    {
                        if (layer.GetData() != null && layer.GetData().GDataBase != null && gdb.Handle == layer.GetData().GDataBase.Handle)
                        {
                            layers.Add(layer);
                        }
                    }
                    if (layers != null && layers.Count > 0)
                    {
                        foreach (MapLayer lay in layers)
                        {
                            DocumentItem item = lay.Parent;
                            if (item is GroupLayer)
                                (item as GroupLayer).Remove(lay);
                            else
                                map.Remove(lay);
                        }
                    }
                }
                DataBase.FreeTempDB(gdb);
            }
        }
    }
}