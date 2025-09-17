/*----------------------------------------------------------------
// Copyright (C) 2019 中地数码科技有限公司
// 版权所有。
//
// 文件名：MyCommand
// 文件功能描述：
//
//
// 创建标识：韩威 2019/3/12 16:29:35
//----------------------------------------------------------------*/

using DevExpress.XtraTreeList.Nodes;
using MapGIS.Desktop.UI.Controls;
using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GeoObjects.Geometry;
using MapGIS.PluginEngine;
using MapGIS.PlugUtility;
using MapGIS.Scene3D;
using MapGISPlugin3;
using MapGISPlugin3.Properties;
using MapGIS.WorkSpace.Style;
using MapGIS.WorkSpaceEngine;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System;

namespace MapGISPlugin3
{
    /// <summary>
    /// 针对目录树右键设计
    /// </summary>
    /// <param name="items"></param>
    public interface IMenuCommand : IPlugin
    {
        //
        // 摘要:
        //     命令按钮的图标
        Bitmap Bitmap { get; }

        //
        // 摘要:
        //     命令按钮的标题
        string Caption { get; }

        //
        // 摘要:
        //     命令按钮所属的类别
        string Category { get; }

        //
        // 摘要:
        //     鼠标移到命令按钮上时状态栏上显示的文本
        string Message { get; }

        //
        // 摘要:
        //     命令按钮的名称，无实际意义
        string Name { get; }

        //
        // 摘要:
        //     鼠标停留在命令按钮上时弹出的提示文本
        string Tooltip { get; }

        IApplication App
        {
            get;
            set;
        }

        void OnClick(object sender, params TreeListNode[] items);

        bool IsVisible(object sender, params TreeListNode[] items);

        bool IsEditable(object sender, params TreeListNode[] items);
    }

    /// <summary>
    /// 预览场景
    /// </summary>
    public class PreviewSceneCommand : IMenuCommand
    {
        public IApplication App
        {
            get;
            set;
        }

        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "预览场景"; }
        }

        public string Category
        {
            get { return "地球物理数据处理"; }
        }

        public string Message
        {
            get { return "预览场景"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "预览场景"; }
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            return true;
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            ProjectManage prjManage = ProjectManage.GetInstance();
            if (prjManage != null && prjManage.OpenPrj != null)
            {
                if (items != null && items.Length == 1 && items[0].Level == 0)
                    return true;
            }
            return false;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            ProjectManage prjManage = ProjectManage.GetInstance();
            if (App != null && prjManage != null && prjManage.OpenPrj != null)
            {
                Document doc = prjManage.Doc;
                if (doc != null)
                {
                    Map Map = null;
                    if (doc.GetScenes() != null && doc.GetScenes().Count > 0)
                        Map = doc.GetMaps().GetMap(0);
                    if (Map != null)
                    {
                        StaticFunctions.MapViewPreview(App, Map);
                    }
                }
            }
        }
    }

    //    /// <summary>
    //    /// 设置为当前显示范围
    //    /// </summary>
    //public class SetCurrrentDspRangeCommand : IMenuCommand
    //{
    //    public IApplication App
    //    {
    //        get;
    //        set;
    //    }

    //    public Bitmap Bitmap
    //    {
    //        get { return null; }
    //    }

    //    public string Caption
    //    {
    //        get { return "设置为当前显示范围"; }
    //    }

    //    public string Category
    //    {
    //        get { return "虚拟军事设施系统"; }
    //    }

    //    public string Message
    //    {
    //        get { return "设置为当前显示范围"; }
    //    }

    //    public string Name
    //    {
    //        get { return this.ToString(); }
    //    }

    //    public string Tooltip
    //    {
    //        get { return "设置为当前显示范围"; }
    //    }

    //    public bool IsEditable(object sender, params TreeListNode[] items)
    //    {
    //        return true;
    //    }

    //    public bool IsVisible(object sender, params TreeListNode[] items)
    //    {
    //        ProjectManage prjManage = ProjectManage.GetInstance();
    //        if (prjManage != null && prjManage.OpenPrj != null && prjManage.OpenPrj is M3DProject)
    //        {
    //            if (items != null && items.Length == 1 && (App.ActiveContentsView is ISceneContentsView) && (items[0].Tag is object[] || items[0].Tag is G3DLayer))
    //            {
    //                Scene3D.SceneControl control = (App.ActiveContentsView as ISceneContentsView).SceneControl;

    //                if (control != null)
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    public void OnClick(object sender, params TreeListNode[] items)
    //    {
    //        G3DLayer layer = null;
    //        if (items != null && items.Length == 1 && (items[0].Tag is object[] || items[0].Tag is G3DLayer))
    //        {
    //            if (items[0].Tag is object[] && (items[0].Tag as object[]).Length == 2 && (items[0].Tag as object[])[1] is G3DLayer)
    //                layer = (items[0].Tag as object[])[1] as G3DLayer;
    //            else if (items[0].Tag is G3DLayer)
    //                layer = items[0].Tag as G3DLayer;
    //        }
    //        if (layer != null)
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (App != null && App.ActiveContentsView is ISceneContentsView)
    //            {
    //                if (prjManage != null && prjManage.OpenPrj != null && prjManage.OpenPrj is M3DProject)
    //                {
    //                    Document doc = prjManage.Doc;
    //                    if (doc != null)
    //                    {
    //                        Scene scene = null;
    //                        if (doc.GetScenes() != null && doc.GetScenes().Count > 0)
    //                            scene = doc.GetScenes().GetScene(0);
    //                        if (scene != null)
    //                        {
    //                            if (scene.LayerCount > 0)
    //                            {
    //                                SetCurrrentDspRange dspRange = new SetCurrrentDspRange();
    //                                dspRange.OnCreate(App.WorkSpaceEngine);
    //                                dspRange.OnClick(layer);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //    /// <summary>
    //    /// 设置经纬度
    //    /// </summary>
    //    public class SetLocation : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "设置经纬度"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "设置经纬度"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "设置经纬度"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                return true;
    //            }
    //            return false;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage.OpenPrj is M3DProject)
    //            {
    //                string mcjPath = (prjManage.OpenPrj as M3DProject).McjPath;
    //                using (FmSetLocation fsl = new FmSetLocation())
    //                {
    //                    fsl.FormClosing += Form_FormClosing;
    //                    if (mcjPath != null && fsl.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //                    {
    //                        int rtn = 0;
    //                        rtn = ThreeDimensionBridge.StaticFun.SetLocationPoint(mcjPath, fsl.getX(), fsl.getY(), fsl.getZ());
    //                        if (rtn == 1)
    //                        {
    //                            XMessageBox.Information("设置经纬度成功！");
    //                        }
    //                        else
    //                        {
    //                            XMessageBox.Information("设置经纬度失败！");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        private void Form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
    //        {
    //            if (sender is FmSetLocation)
    //            {
    //                if ((sender as FmSetLocation).DialogResult == DialogResult.OK)
    //                {

    //                }
    //            }
    //        }
    //    }

    //    public class InputModeChildCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "导入数据"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "导入数据"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "导入数据"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && items[0].Tag is TreeItemInfo)
    //                return true;
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            if (items == null || items.Length != 1 || items[0].Tag is object[])
    //                return false;
    //            return true;
    //        }

    //        private TreeItemInfo itemInfo = null;

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && items[0].Tag is TreeItemInfo)
    //            {
    //                ProjectManage prjManage = ProjectManage.GetInstance();
    //                if (prjManage.OpenPrj is M3DProject)
    //                {
    //                    itemInfo = items[0].Tag as TreeItemInfo;
    //                    string mcjPath = (prjManage.OpenPrj as M3DProject).McjPath;
    //                    using (InputDataForm form = new InputDataForm())
    //                    {
    //                        form.FormClosing += Form_FormClosing;
    //                        if (mcjPath != null && form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //                        {
    //                            string dataType = form.PrjType;
    //                            string filePath = form.FilePath;
    //                            string dataName = form.DataName;
    //                            int rtn = 0;
    //                            if ("PointCloud".Equals(dataType))
    //                            {
    //                                rtn = ThreeDimensionBridge.StaticFun.ImportPointCloud(mcjPath, Path.GetFullPath(itemInfo.Path), filePath, dataName);
    //                            }
    //                            else
    //                            {
    //                                rtn = ThreeDimensionBridge.StaticFun.ImportSurface(mcjPath, Path.GetFullPath(itemInfo.Path), filePath, dataName, true);
    //                            }
    //                            if (rtn > 0)
    //                            {
    //                                Document doc = InitPlugin.App.Document;
    //                                if (doc != null && doc.GetScenes() != null && doc.GetScenes().Count > 0)
    //                                {
    //                                    Scene scene = doc.GetScenes().GetScene(0);
    //                                    if (scene != null && scene.LayerCount > 0)
    //                                    {
    //                                        M3DDataInfo info = (prjManage.OpenPrj as M3DProject).DataInfo;
    //                                        if (info != null && !string.IsNullOrWhiteSpace(info.DataName))
    //                                        {
    //                                            for (int i = 0; i < scene.LayerCount; i++)
    //                                            {
    //                                                G3DLayer layer = scene.GetLayer(i);
    //                                                if (layer != null && layer is ModelCacheLayer && info.DataName.Equals(layer.Name))
    //                                                {
    //                                                    ModelCacheLayer newLayer = new ModelCacheLayer();
    //                                                    newLayer.Name = layer.Name;
    //                                                    newLayer.URL = layer.URL;
    //                                                    scene.Insert(i, newLayer);
    //                                                    scene.Remove(layer);
    //                                                    break;
    //                                                }
    //                                            }
    //                                        }
    //                                    }
    //                                }
    //                                (prjManage.OpenPrj as M3DProject).UpDateTreeItem(itemInfo);
    //                                XMessageBox.Information("数据导入成功！");
    //                            }
    //                            else
    //                                XMessageBox.Information("数据导入失败！");
    //                        }
    //                    }
    //                }

    //            }
    //        }

    //        private void Form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
    //        {
    //            if (sender is InputDataForm && itemInfo != null)
    //            {
    //                if ((sender as InputDataForm).DialogResult == DialogResult.OK)
    //                {
    //                    string dataName = (sender as InputDataForm).DataName;
    //                    List<TileDataInfo> tileList = itemInfo.TileDataInfoList;
    //                    if (dataName != null && tileList != null && tileList.Count > 0)
    //                    {
    //                        bool isExist = false;
    //                        foreach (TileDataInfo info in tileList)
    //                        {
    //                            if (dataName.Equals(info.Nmae))
    //                            {
    //                                isExist = true;
    //                                break;
    //                            }
    //                        }
    //                        if (isExist)
    //                        {
    //                            XMessageBox.Information($"已存名称为“{dataName}”的数据，无法导入。");
    //                            e.Cancel = true;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public class AddModeChildCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "添加模型对象"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "添加模型对象"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "添加模型对象"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && (items[0].Tag is object[] || items[0].Tag is TreeItemInfo))
    //                return true;
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            //if (items != null && items.Length == 1 && items[0].Tag is M3DDataInfo)
    //            //{
    //            //    using (CreatePrjectForm form = new CreatePrjectForm(false))
    //            //    {
    //            //        form.Text = "选择模型";
    //            //        if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //            //        {
    //            //            string dirPath =  DataModeClass.GetModeDir(form.PrjType);
    //            //            if (M3dDemonstrateClass.AddNewChildNodeByMode(items[0].Tag as M3DDataInfo, dirPath, form.PrjName) > 0)
    //            //            {
    //            //                if (sender is ProjectTreeControl)
    //            //                {
    //            //                    ProjectTreeControl tree = (sender as ProjectTreeControl);
    //            //                    tree.UpDateNode(items[0]);
    //            //                    items[0].Expand();
    //            //                }
    //            //                M3dDocumentClass.UpdateTreeItemInfoFile((items[0].Tag as M3DDataInfo).TreeInfo);
    //            //            }
    //            //        }
    //            //    }

    //            //}
    //        }
    //    }

    //    public class AddChildCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "添加子节点"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "添加子节点"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "添加子节点"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && (items[0].Tag is object[] || items[0].Tag is TreeItemInfo))
    //            {
    //                if (items[0].Tag is TreeItemInfo)
    //                {
    //                    TreeItemInfo itemInfo = (items[0].Tag as TreeItemInfo);
    //                    if (itemInfo != null && ("水陆一体".Equals(itemInfo.SceneType) || "地下".Equals(itemInfo.SceneType)))
    //                        return false;
    //                }
    //                return true;
    //            }
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && (items[0].Tag is object[] || items[0].Tag is TreeItemInfo))
    //            {
    //                ProjectManage prjManage = ProjectManage.GetInstance();
    //                if (prjManage.OpenPrj is M3DProject)
    //                {
    //                    string path = (prjManage.OpenPrj as M3DProject).McjPath;
    //                    using (CreatePrjectForm form = new CreatePrjectForm(false, (items[0].Tag is TreeItemInfo) ? (items[0].Tag as TreeItemInfo).SceneType : null))
    //                    {
    //                        if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //                        {
    //                            TreeItemInfo treeInfo = null;

    //                            if (items[0].Tag is object[] && (items[0].Tag as object[]).Length == 2 && (items[0].Tag as object[])[0] is M3DDataInfo && ((items[0].Tag as object[])[0] as M3DDataInfo).TreeInfo != null)
    //                            {
    //                                treeInfo = ((items[0].Tag as object[])[0] as M3DDataInfo).TreeInfo;
    //                            }
    //                            else if (items[0].Tag is TreeItemInfo)
    //                            {
    //                                treeInfo = (items[0].Tag as TreeItemInfo);
    //                            }
    //                            if (treeInfo != null)
    //                            {
    //                                string outPath = null;
    //                                if (ThreeDimensionBridge.StaticFun.AddNewNode(path, form.PrjName, Path.GetFullPath(treeInfo.Path), form.SceneType, out outPath) > 0)
    //                                {
    //                                    (prjManage.OpenPrj as M3DProject).UpDateTreeItem(treeInfo);
    //                                    if (sender is ProjectTreeControl)
    //                                    {
    //                                        ProjectTreeControl tree = (sender as ProjectTreeControl);
    //                                        tree.UpDateNode(items[0]);
    //                                        items[0].Expand();
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public class ExportCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "导出节点"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "导出节点"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "导出节点"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && items[0].Tag is TreeItemInfo)
    //                return true;
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            if (items == null || items.Length != 1 || items[0].Tag is object[])
    //                return false;
    //            return true;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            //if (items != null && items.Length == 1 && items[0].Tag is M3DDataInfo)
    //            //{
    //            //    using (GDBSelectFolderDialog folderDlg = new GDBSelectFolderDialog())
    //            //    {
    //            //        folderDlg.Name = "选择保存路径";
    //            //        folderDlg.FolderType = FolderType.Windows_Folder;

    //            //        if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //            //        {
    //            //            string selPath = folderDlg.SelectedPath;
    //            //            string dirPath = Path.Combine(selPath, (items[0].Tag as M3DDataInfo).DataName);
    //            //            if (Directory.Exists(dirPath))
    //            //            {
    //            //                XMessageBox.Information($"存在名称为{(items[0].Tag as M3DDataInfo).DataName}的文件夹。");
    //            //                return;
    //            //            }
    //            //            Directory.CreateDirectory(dirPath);
    //            //            string oldPath = Path.GetDirectoryName((items[0].Tag as M3DDataInfo).Path) ;
    //            //            StaticFunctions.CopyDir(oldPath, dirPath);
    //            //            XMessageBox.Information("导出成功");
    //            //        }
    //            //    }
    //            //}
    //        }
    //    }

    //    public class DeleteChildCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "删除节点"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "删除节点"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "删除节点"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length > 0)
    //            {
    //                if (items.Length != 1)
    //                    return false;
    //                return true;
    //            }
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            if (items == null || items.Length != 1 || !(items[0].Tag is TreeItemInfo))
    //                return false;
    //            return true;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length > 0)
    //            {
    //                if (XMessageBox.Question("确定删除选中节点及子节点?") == System.Windows.Forms.DialogResult.OK)
    //                {
    //                    ProjectManage prjManage = ProjectManage.GetInstance();
    //                    if (prjManage.OpenPrj is M3DProject)
    //                    {
    //                        string path = (prjManage.OpenPrj as M3DProject).McjPath;

    //                        TreeListNode parNode = items[0].ParentNode;
    //                        int num = 0;
    //                        for (int i = 0; i < items.Length; i++)
    //                        {
    //                            TreeListNode node = items[i];
    //                            if (node != null && (node.Tag is TreeItemInfo))
    //                            {
    //                                try
    //                                {
    //                                    string filePath = (node.Tag as TreeItemInfo).Path;
    //                                    if (ThreeDimensionBridge.StaticFun.DeleteNode(path, Path.GetFullPath(filePath)) > 0)
    //                                    {
    //                                        num++;
    //                                        if ((prjManage.OpenPrj as M3DProject).RceManage != null)
    //                                            (prjManage.OpenPrj as M3DProject).RceManage.RemoveResourcePath(node.Tag as TreeItemInfo);
    //                                    }
    //                                }
    //                                catch { }
    //                            }
    //                        }

    //                        TreeItemInfo parInfo = null;
    //                        if (parNode.Tag is TreeItemInfo)
    //                            parInfo = (parNode.Tag as TreeItemInfo);
    //                        if (parInfo != null)
    //                        {
    //                            (prjManage.OpenPrj as M3DProject).UpDateTreeItem(parInfo);

    //                            if (sender is ProjectTreeControl)
    //                            {
    //                                ProjectTreeControl tree = (sender as ProjectTreeControl);
    //                                tree.UpDateNode(parNode);
    //                                items[0].Expand();
    //                            }
    //                        }
    //                        if (num == 0)
    //                            XMessageBox.Information("删除失败！");
    //                        else if (num < items.Length)
    //                            XMessageBox.Information("部分节点删除失败！");
    //                        else
    //                            XMessageBox.Information("删除成功！");
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 节点重命名
    //    /// </summary>
    //    public class RenameCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "重命名"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "重命名"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "重命名"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length > 0)
    //            {
    //                if (items.Length != 1)
    //                    return false;
    //                return true;
    //            }
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            if (items == null || items.Length != 1 || !(items[0].Tag is TreeItemInfo))
    //                return false;
    //            return true;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1)
    //            {
    //                ProjectManage prjManage = ProjectManage.GetInstance();
    //                if (prjManage.OpenPrj is M3DProject)
    //                {
    //                    string path = (prjManage.OpenPrj as M3DProject).McjPath;
    //                    TreeListNode node = items[0];


    //                    if (node != null && (node.Tag is TreeItemInfo))
    //                    {
    //                        try
    //                        {
    //                            string name = (node.Tag as TreeItemInfo).Name;
    //                            using (ReNameForm form = new ReNameForm(name))
    //                            {
    //                                if (form.ShowDialog() == DialogResult.OK)
    //                                {
    //                                    string nodePath = (node.Tag as TreeItemInfo).Path;
    //                                    if (ThreeDimensionBridge.StaticFun.RenameNode(path, Path.GetFullPath(nodePath), form.NewName) > 0)
    //                                    {

    //                                        TreeItemInfo info = (node.Tag as TreeItemInfo);
    //                                        if (info != null)
    //                                        {
    //                                            (prjManage.OpenPrj as M3DProject).UpDateTreeItem(info);
    //                                            if (sender is ProjectTreeControl)
    //                                            {
    //                                                ProjectTreeControl tree = (sender as ProjectTreeControl);
    //                                                tree.UpDateNode(node);
    //                                            }
    //                                        }

    //                                    }
    //                                }
    //                            }

    //                        }
    //                        catch { }
    //                    }

    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 文档属性
    //    /// </summary>
    //    public class PropertyCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "文档属性"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "文档属性"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "文档属性"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                if (items != null && items.Length == 1 && items[0].Tag is object[])
    //                {
    //                    return true;
    //                }
    //            }
    //            return false;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (App != null && App.ActiveContentsView is ISceneContentsView)
    //            {
    //                if (prjManage != null && prjManage.OpenPrj != null && prjManage.OpenPrj is M3DProject)
    //                {
    //                    Document doc = prjManage.Doc;
    //                    if (doc != null)
    //                    {
    //                        Scene scene = null;
    //                        if (doc.GetScenes() != null && doc.GetScenes().Count > 0)
    //                            scene = doc.GetScenes().GetScene(0);
    //                        if (scene != null)
    //                        {
    //                            bool isWorkSpace = false;
    //                            if (doc != null && this.App.Document.Handle == doc.Handle)
    //                                isWorkSpace = true;
    //                            IWorkSpace workSpace = null;
    //                            if (isWorkSpace)
    //                            {   //平台的工作目录在属性应用时会触发修改名称状态等逻辑，使用平台的弹出属性框可以屏蔽该问题
    //                                workSpace = this.App.WorkSpaceEngine;
    //                                if (workSpace != null)
    //                                    workSpace.FireMenuItemClickEvent("MapGIS.WorkSpace.Style.ItemProperty", scene);
    //                            }
    //                            else
    //                            {
    //                                DocumentItemProperties.ShowProperty(scene, true);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 图层属性
    //    /// </summary>
    //    public class LayerPropertyCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return Resources.属性; }
    //        }

    //        public string Caption
    //        {
    //            get { return "图层属性"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "图层属性"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "图层属性"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                if (items != null && items.Length == 1 && (items[0].Tag is object[] || items[0].Tag is G3DLayer))
    //                {
    //                    return true;
    //                }
    //            }
    //            return false;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && (items[0].Tag is object[] || items[0].Tag is G3DLayer))
    //            {
    //                G3DLayer layer = null;
    //                if (items[0].Tag is object[] && (items[0].Tag as object[]).Length == 2 && (items[0].Tag as object[])[1] is G3DLayer)
    //                {
    //                    layer = (items[0].Tag as object[])[1] as G3DLayer;
    //                }
    //                else if (items[0].Tag is G3DLayer)
    //                {
    //                    layer = items[0].Tag as G3DLayer;
    //                }
    //                if (layer != null)
    //                {
    //                    DocumentItemProperties.ShowProperty(layer, true);
    //                }
    //            }
    //        }
    //    }

    //    public class PropertyChildCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return null; }
    //        }

    //        public string Caption
    //        {
    //            get { return "节点参数"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "节点参数"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "节点参数"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && (items[0].Tag is TreeItemInfo))
    //                return true;
    //            return false;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1)
    //            {
    //                TreeItemInfo info = null;
    //                if (items[0].Tag is object[] && (items[0].Tag as object[]).Length == 2 && (items[0].Tag as object[])[0] is M3DDataInfo && ((items[0].Tag as object[])[0] as M3DDataInfo).TreeInfo != null)
    //                    info = ((items[0].Tag as object[])[0] as M3DDataInfo).TreeInfo;
    //                else if (items[0].Tag is TreeItemInfo)
    //                    info = (items[0].Tag as TreeItemInfo);
    //                if (info != null)
    //                {
    //                    using (AttributeForm form = new AttributeForm(info))
    //                    {
    //                        form.ShowDialog();
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public class ModelMoveCommand : ICommand, IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return MapGIS.PluginEngine.Application.IsRibbon ? Resources.Png_ModelMove_32 : Resources.Png_ModelMove_16; }
    //        }

    //        public string Caption
    //        {
    //            get { return "移动数据"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public bool Checked
    //        {
    //            get
    //            {
    //                return false;
    //            }
    //        }

    //        public bool Enabled
    //        {
    //            get
    //            {
    //                return true;
    //            }
    //        }

    //        public string Message
    //        {
    //            get { return "添加模型对象"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "移动数据"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && items[0].ParentNode != null && items[0].ParentNode.ParentNode != null && items[0].Tag is TreeItemInfo && (items[0].Tag as TreeItemInfo).ParentNode != null && ((items[0].Tag as TreeItemInfo).SceneType == StaticFunctions.OUTDOOR || (items[0].Tag as TreeItemInfo).SceneType == StaticFunctions.INDOOR))
    //                return true;
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public void OnClick()
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                IDockWindow dw = null;
    //                App.PluginContainer.DockWindows.TryGetValue(typeof(ProjectTreeDockWindow).ToString(), out dw);
    //                if (dw != null)
    //                {
    //                    if (App.ActiveContentsView is ISceneContentsView)
    //                    {
    //                        SceneControl scene = (App.ActiveContentsView as ISceneContentsView).SceneControl;
    //                        M3dNodeMoveEditTool tool = new M3dNodeMoveEditTool(scene, 1);
    //                        scene.SetActiveTool(tool);
    //                        tool.KeyQ += Tool_KeyQ;
    //                        tool.Edited += Tool_Edited;
    //                        tool.Start();
    //                    }
    //                }
    //            }
    //        }

    //        private string GetNodeJsonPath(string path)
    //        {
    //            string rtn = null;



    //            if (!string.IsNullOrWhiteSpace(path))
    //            {
    //                ProjectManage prjManage = ProjectManage.GetInstance();
    //                if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //                {
    //                    string mcjPath = (prjManage.OpenPrj as M3DProject).McjPath;
    //                    if (!string.IsNullOrWhiteSpace(mcjPath) && path.LastIndexOf('_') > 0)
    //                    {
    //                        string dirPath = Path.GetDirectoryName(mcjPath);
    //                        string filePath = path.Substring(path.LastIndexOf('_') + 1);
    //                        rtn = Path.Combine(dirPath, "node", filePath);
    //                    }
    //                }
    //            }
    //            return rtn;
    //        }

    //        private void Tool_Edited(string sender, Dot3D basPnt, Dot3D offPnt)
    //        {
    //            if (!string.IsNullOrWhiteSpace(sender))
    //            {
    //                string path = GetNodeJsonPath(sender);
    //                if (!string.IsNullOrWhiteSpace(path))
    //                {
    //                    ThreeDimensionBridge.StaticFun.MoveNodeData(Path.GetFullPath(path), offPnt.X, offPnt.Y, offPnt.Z);
    //                }
    //            }
    //        }

    //        private void Tool_KeyQ(List<string> sender, bool bValue)
    //        {
    //            if (sender != null && sender.Count > 0 && bValue)
    //            {
    //                List<string> paths = new List<string>();
    //                for (int i = 0; i < sender.Count; i++)
    //                {
    //                    string path = GetNodeJsonPath(sender[i]);
    //                    if (!string.IsNullOrWhiteSpace(path))
    //                        paths.Add(path);
    //                }
    //                if (paths.Count > 0)
    //                {
    //                    ModelMoveForm form = new ModelMoveForm();
    //                    if (form.ShowDialog() == DialogResult.OK)
    //                    {
    //                        for (int i = 0; i < paths.Count; i++)
    //                        {
    //                            ThreeDimensionBridge.StaticFun.MoveNodeData(Path.GetFullPath(paths[i]), form.DX, form.DY, form.DZ);
    //                        }
    //                    }
    //                }
    //            }
    //        }


    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            OnClick();
    //        }

    //        public void OnCreate(IApplication app)
    //        {
    //            this.App = app;
    //            if (this.App != null)
    //                this.App.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
    //        }

    //        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
    //        {
    //            bool isEnable = false;
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                isEnable = true;
    //            }
    //            App.PluginContainer.PluginEnable(this, isEnable);
    //        }
    //    }

    //    public class ModelRotateCommand : ICommand, IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return MapGIS.PluginEngine.Application.IsRibbon ? Resources.Png_ModelRotate_32 : Resources.Png_ModelRotate_16; }
    //        }

    //        public string Caption
    //        {
    //            get { return "旋转数据"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public bool Checked
    //        {
    //            get
    //            {
    //                return false;
    //            }
    //        }

    //        public bool Enabled
    //        {
    //            get
    //            {
    //                return true;
    //            }
    //        }

    //        public string Message
    //        {
    //            get { return "旋转数据"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "旋转数据"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length == 1 && items[0].ParentNode != null && items[0].ParentNode.ParentNode != null && items[0].Tag is TreeItemInfo && (items[0].Tag as TreeItemInfo).ParentNode != null && ((items[0].Tag as TreeItemInfo).SceneType == StaticFunctions.OUTDOOR || (items[0].Tag as TreeItemInfo).SceneType == StaticFunctions.INDOOR))
    //                return true;
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            return true;
    //        }

    //        public void OnClick()
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                IDockWindow dw = null;
    //                App.PluginContainer.DockWindows.TryGetValue(typeof(ProjectTreeDockWindow).ToString(), out dw);
    //                if (dw != null)
    //                {
    //                    List<TreeItemInfo> items = (dw as ProjectTreeDockWindow).GetSelectItems();
    //                    if (items != null && items.Count == 1 && items[0].ParentNode != null && (items[0].SceneType == StaticFunctions.OUTDOOR || items[0].SceneType == StaticFunctions.INDOOR))
    //                    {
    //                        TreeItemInfo info = items[0];
    //                        if (info.TileDataInfoList == null || info.TileDataInfoList.Count <= 0)
    //                        {
    //                            XMessageBox.Information("当前节点没有数据，无法旋转！");
    //                            return;
    //                        }
    //                        using (ModelRotateForm form = new ModelRotateForm())
    //                        {
    //                            if (form.ShowDialog() == DialogResult.OK)
    //                            {
    //                                if (ThreeDimensionBridge.StaticFun.RotateNodeData(Path.GetFullPath(info.Path), form.Angle) > 0)
    //                                {
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            OnClick();
    //        }

    //        public void OnCreate(IApplication app)
    //        {
    //            this.App = app;
    //            if (this.App != null)
    //                this.App.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
    //        }

    //        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
    //        {
    //            bool isEnable = false;
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                IDockWindow dw = null;
    //                App.PluginContainer.DockWindows.TryGetValue(typeof(ProjectTreeDockWindow).ToString(), out dw);
    //                if (dw != null)
    //                {
    //                    List<TreeItemInfo> items = (dw as ProjectTreeDockWindow).GetSelectItems();
    //                    if (items != null && items.Count == 1 && items[0].ParentNode != null && items[0].ParentNode.ParentNode != null && items[0].ParentNode.ParentNode != null && (items[0].SceneType == StaticFunctions.OUTDOOR || items[0].SceneType == StaticFunctions.INDOOR))
    //                        isEnable = true;
    //                }
    //            }
    //            App.PluginContainer.PluginEnable(this, isEnable);
    //        }
    //    }

    //    /// <summary>
    //    /// 添加部件层
    //    /// </summary>
    //    public class AddModelCommand : ICommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return MapGIS.PluginEngine.Application.IsRibbon ? Resources.Png_SfClsSurface_32 : Resources.Png_AreaLayer_16; }
    //        }

    //        public string Caption
    //        {
    //            get { return "添加部件层"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public bool Checked
    //        {
    //            get
    //            {
    //                return false;
    //            }
    //        }

    //        public bool Enabled
    //        {
    //            get
    //            {
    //                return true;
    //            }
    //        }

    //        public string Message
    //        {
    //            get { return "添加部件层"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "添加部件层"; }
    //        }

    //        public void OnClick()
    //        {
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                M3DProject project = prjManage.OpenPrj as M3DProject;
    //                if (project == null || project.DataInfo == null)
    //                    return;
    //                Document doc = App.Document;
    //                if (doc != null && doc.GetScenes() != null && doc.GetScenes().Count > 0)
    //                {
    //                    bool isExist = false;
    //                    Scene scene = doc.GetScenes().GetScene(0);
    //                    if (scene != null && scene.LayerCount > 0)
    //                    {
    //                        for (int i = 0; i < scene.LayerCount; i++)
    //                        {
    //                            G3DLayer layer = scene.GetLayer(i);
    //                            if (layer is ModelLayer && layer.Name == M3DProject.modelLayerName)
    //                                isExist = true;
    //                        }
    //                    }
    //                    if (!isExist)
    //                    {
    //                        string url = null;
    //                        if (!project.ExistModelCls(out url) || string.IsNullOrWhiteSpace(url))
    //                            isExist = project.AddModelCls(out url) > 0;
    //                        else
    //                            isExist = true;
    //                        if (isExist && !string.IsNullOrWhiteSpace(url))
    //                        {
    //                            ModelLayer layer = new ModelLayer();
    //                            layer.URL = url;
    //                            layer.State = LayerState.Active;
    //                            layer.Name = Path.GetFileNameWithoutExtension(url);
    //                            layer.ConnectData();
    //                            scene.Append(layer);

    //                            IDockWindow dw = null;
    //                            App.PluginContainer.DockWindows.TryGetValue(typeof(ProjectTreeDockWindow).ToString(), out dw);
    //                            if (dw != null && (dw as ProjectTreeDockWindow).TreeControl != null)
    //                            {
    //                                (dw as ProjectTreeDockWindow).TreeControl.LoadTree();
    //                            }
    //                        }
    //                    }
    //                    else
    //                        XMessageBox.Information("已存在部件层");
    //                }
    //            }
    //        }


    //        public void OnCreate(IApplication app)
    //        {
    //            this.App = app;
    //            if (this.App != null)
    //                this.App.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
    //        }

    //        private void StateManager_StateChangedEvent(object sender, StateEventArgs e)
    //        {
    //            bool isEnable = false;
    //            ProjectManage prjManage = ProjectManage.GetInstance();
    //            if (prjManage != null && prjManage.OpenPrj is M3DProject)
    //            {
    //                isEnable = true;
    //            }
    //            App.PluginContainer.PluginEnable(this, isEnable);
    //        }
    //    }

    //    /// <summary>
    //    /// 移除图层
    //    /// </summary>
    //    public class RemoveLayerCommand : IMenuCommand
    //    {
    //        public IApplication App
    //        {
    //            get;
    //            set;
    //        }

    //        public Bitmap Bitmap
    //        {
    //            get { return Resources.Png_NodeActive_16; }
    //        }

    //        public string Caption
    //        {
    //            get { return "移除图层"; }
    //        }

    //        public string Category
    //        {
    //            get { return "虚拟军事设施系统"; }
    //        }

    //        public string Message
    //        {
    //            get { return "移除图层"; }
    //        }

    //        public string Name
    //        {
    //            get { return this.ToString(); }
    //        }

    //        public string Tooltip
    //        {
    //            get { return "移除图层"; }
    //        }

    //        public bool IsEditable(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length > 0)
    //            {
    //                if (items.Length != 1)
    //                    return false;
    //                return true;
    //            }
    //            return false;
    //        }

    //        public bool IsVisible(object sender, params TreeListNode[] items)
    //        {
    //            if (items == null || items.Length != 1 || !(items[0].Tag is G3DLayer))
    //                return false;
    //            return true;
    //        }

    //        public void OnClick(object sender, params TreeListNode[] items)
    //        {
    //            if (items != null && items.Length > 0)
    //            {
    //                for (int i = 0; i < items.Length; i++)
    //                {
    //                    TreeListNode item = items[i];
    //                    if (item != null && item.Tag is G3DLayer)
    //                    {
    //                        G3DLayer layer = (item.Tag as G3DLayer);
    //                        if (layer.Parent != null)
    //                        {
    //                            if (layer.Parent is Scene)
    //                                (layer.Parent as Scene).Remove(layer);
    //                            else if (layer.Parent is Group3DLayer)
    //                                (layer.Parent as Group3DLayer).Remove(layer);
    //                        }
    //                    }
    //                }
    //                if (sender is ProjectTreeControl)
    //                {
    //                    ProjectTreeControl tree = (sender as ProjectTreeControl);
    //                    tree.LoadTree();
    //                }
    //            }
    //        }
    //    }

    public class NodeVisibleCommand : IMenuCommand
    {
        public IApplication App
        {
            get;
            set;
        }

        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "可见"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public string Message
        {
            get { return "可见"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "可见"; }
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                if (items.Length <= 0)
                    return false;
                return true;
            }
            return false;
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            if (items == null || items.Length <= 0 || (items[0].Tag is TreeItemInfo))
                return false;
            return true;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    TreeListNode item = items[i];
                    if (item != null && item.Tag is G3DLayer)
                    {
                        (item.Tag as G3DLayer).State = LayerState.Visible;
                    }
                    else if (item != null && item.Tag is object[] && (item.Tag as object[]).Length == 2 && (item.Tag as object[])[1] is G3DLayer)
                    {
                        ((item.Tag as object[])[1] as G3DLayer).State = LayerState.Visible;
                    }
                }
                if (sender is ProjectTreeControl)
                {
                    ProjectTreeControl tree = (sender as ProjectTreeControl);
                    tree.UpdateSelectImage();
                }
            }
        }
    }
    public class NodeUnVisibleCommand : IMenuCommand
    {
        public IApplication App
        {
            get;
            set;
        }

        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "不可见"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public string Message
        {
            get { return "不可见"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "不可见"; }
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                if (items.Length <= 0)
                    return false;
                return true;
            }
            return false;
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            if (items == null || items.Length <= 0 || (items[0].Tag is TreeItemInfo))
                return false;
            return true;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    TreeListNode item = items[i];
                    if (item != null && item.Tag is G3DLayer)
                    {
                        (item.Tag as G3DLayer).State = LayerState.UnVisible;
                    }
                    else if (item != null && item.Tag is object[] && (item.Tag as object[]).Length == 2 && (item.Tag as object[])[1] is G3DLayer)
                    {
                        ((item.Tag as object[])[1] as G3DLayer).State = LayerState.UnVisible;
                    }
                }
                if (sender is ProjectTreeControl)
                {
                    ProjectTreeControl tree = (sender as ProjectTreeControl);
                    tree.UpdateSelectImage();
                }
            }
        }
    }
    public class NodeEditableCommand : IMenuCommand
    {
        public IApplication App
        {
            get;
            set;
        }

        public Bitmap Bitmap
        {
            get { return null ; }
        }

        public string Caption
        {
            get { return "编辑"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public string Message
        {
            get { return "编辑"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "编辑"; }
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                if (items.Length <= 0)
                    return false;
                return true;
            }
            return false;
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            if (items == null || items.Length <= 0 || (items[0].Tag is TreeItemInfo))
                return false;
            return true;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    TreeListNode item = items[i];
                    if (item != null && item.Tag is G3DLayer)
                    {
                        (item.Tag as G3DLayer).State = LayerState.Editable;
                    }
                    else if (item != null && item.Tag is object[] && (item.Tag as object[]).Length == 2 && (item.Tag as object[])[1] is G3DLayer)
                    {
                        ((item.Tag as object[])[1] as G3DLayer).State = LayerState.Editable;
                    }
                }
                if (sender is ProjectTreeControl)
                {
                    ProjectTreeControl tree = (sender as ProjectTreeControl);
                    tree.UpdateSelectImage();
                }
            }
        }
    }
    public class NodeActiveCommand : IMenuCommand
    {
        public IApplication App
        {
            get;
            set;
        }

        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "当前编辑"; }
        }

        public string Category
        {
            get { return "虚拟军事设施系统"; }
        }

        public string Message
        {
            get { return "当前编辑"; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public string Tooltip
        {
            get { return "当前编辑"; }
        }

        public bool IsEditable(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                if (items.Length != 1)
                    return false;
                return true;
            }
            return false;
        }

        public bool IsVisible(object sender, params TreeListNode[] items)
        {
            if (items == null || items.Length != 1 || (items[0].Tag is TreeItemInfo))
                return false;
            return true;
        }

        public void OnClick(object sender, params TreeListNode[] items)
        {
            if (items != null && items.Length > 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    TreeListNode item = items[i];
                    if (item != null && item.Tag is G3DLayer)
                    {
                        (item.Tag as G3DLayer).State = LayerState.Active;
                    }
                    else if (item != null && item.Tag is object[] && (item.Tag as object[]).Length == 2 && (item.Tag as object[])[1] is G3DLayer)
                    {
                        ((item.Tag as object[])[1] as G3DLayer).State = LayerState.Active;
                    }
                }
                if (sender is ProjectTreeControl)
                {
                    ProjectTreeControl tree = (sender as ProjectTreeControl);
                    tree.UpdateSelectImage();
                }
            }
        }
    }

    //}
}