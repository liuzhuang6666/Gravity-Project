using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapGIS.PluginEngine;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GISControl;
using MapGIS.UI.Controls;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoDataBase.Config;
using MapGIS.GeoObjects;
namespace MapGISPlugin3
{
    public class DockWindow1 : IDockWindow
    {
       
        private Control control;
        private TreeView treeView1;     
        private DataBase gdb = null;

        private ContextMenuStrip xclsMenuStrip;
        private ToolStripMenuItem 预览ToolStripMenuItem;
        private ToolStripMenuItem 调阅ToolStripMenuItem;
        private ToolStripMenuItem 删除ToolStripMenuItem;
        private ToolStripMenuItem 导出ToolStripMenuItem;
        private ToolStripMenuItem 重命名ToolStripMenuItem;

        
        IApplication hook = null;
        //地图视图控件


        MapControl mapCtrl = null;// 预览
        Document doc = null;
        MapControl mapCtrl1 = null;//调阅
                                   //工作空间树
        MapWorkSpaceTree _Tree = null;
        //最顶层的树节点
        TreeNode TopNode = null;
        //数据库节点
        TreeNode gdbNode = null;
        //要素数据集节点
        TreeNode fdsNode = null;
        //与要素数据集同级的要素节点
        TreeNode xclsNode = null;
        //与要素数据集同级的简单简单要素类节点
        TreeNode sfclsNode = null;
        //与要素数据集同级的注记类节点
        TreeNode annNode = null;
        //与要素数据集同级的对象类节点
        TreeNode objNode = null;
        //要素数据集文件夹节点
        TreeNode fdsNode0 = null;
        //栅格数据集文件夹节点
        TreeNode rasdstNode = null;
        //栅格目录文件夹节点
        TreeNode rascatNode = null;
        //栅格数据集节点
        TreeNode rasdstNode0 = null;
        //栅格目录节点
        TreeNode rascatNode0 = null;
        //关系类文件夹节点
        TreeNode relationNode = null;
        //关系类节点
        TreeNode relationNode0 = null;
        //地图集文件夹节点
        TreeNode mapsetNode = null;
        // 地图集节点
        TreeNode mapsetNode0 = null;

        TreeNode databaseNode = null;
        //栅格目录节点下栅格数据集节点
        TreeNode rascatNode0_ras = null;
        //数据源管理对象
        SvcConfig svcConfig = new SvcConfig();

        //当前节点
        TreeNode CurrentNode = null;
        //数据源
        Server Svr = null;
        //数据库
        DataBase GDB = null;

        //数据库名称
        string gdbName = "";
        //要素数据集名称
        string fdsName = "";
        //要素类名称
        string xclsName = "";
        //栅格数据集名称
        string rasdstName0 = "";
        //栅格目录名称
        string rascatName0 = "";
        //关系类名称
        string relaclsName0 = "";
        //地图集名称
        string mapsetName0 = null;

        //简单要素类
        SFeatureCls sfCls = null;
        //注记类
        AnnotationCls anCls = null;
        RasterCatalog rascat = null;
        Map map = null;


        public DockWindow1()
        {
            this.control = new UserControl();

            this.treeView1 = new TreeView();
            this.treeView1.Dock = DockStyle.Fill;
            this.treeView1.ShowLines = true;
            this.treeView1.ShowPlusMinus = true;
            this.control.Controls.Add(this.treeView1);

            
            InitializeContextMenu();
            // xclsMenuStrip
            // 
            
        }
        //this.移动到数据集ToolStripMenuItem,
        //this.查看元数据信息ToolStripMenuItem});

        private void InitializeContextMenu()
        {
            xclsMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem 预览ToolStripMenuItem = new ToolStripMenuItem("预览");
            ToolStripMenuItem 调阅ToolStripMenuItem = new ToolStripMenuItem("调览");
            xclsMenuStrip.Items.Add(预览ToolStripMenuItem);
            xclsMenuStrip.Items.Add(调阅ToolStripMenuItem);

            
            // 设置TreeView的ContextMenuStrip属性
            treeView1.ContextMenuStrip = xclsMenuStrip;
        }


        #region IDockWindow成员

        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "分类数据空间"; }
        }

        public Control ChildHWND
        {
            get { return this.control; }
        }

        public DockingStyle DefaultDock
        {
            get { return DockingStyle.Left;
              
            }
        }

        public bool InitCreate
        {
            get { return true; }
        }

        public string Name
        {
            get { return "DockWindow1"; }
        }

        public void OnActive(bool isActive)
        {
        }

        public void OnCreate(IApplication hook)
        {
            this.hook = hook;
            treeView1.MouseDown += new MouseEventHandler(treeView1_MouseDown);
            this.mapCtrl = (this.hook.ActiveContentsView as IMapContentsView)?.MapControl;
            this.doc = hook.Document;
            //是否显示节点之间线
            treeView1.ShowLines = true;
            treeView1.ShowRootLines = true;
            treeView1.Indent = 16;
            treeView1.ItemHeight = 18;


            //在父节点显示+、-号
            treeView1.ShowNodeToolTips = true;
            treeView1.ShowPlusMinus = true;

            //初始化
            
            Init();
            //treeView1.SelectedNode.Expand();
            //treeView1.TopNode.ImageIndex = 0;

        }


        public int GetFdsID(DataBase db, string FdsName)
        {
            List<int> FdsIDs = null;
            FdsIDs = db.GetXclses(XClsType.Fds, 0);

            int id = 0;
            //通过要素数据集的名称来获取其ID
            for (int i = 0; i < FdsIDs.Count; i++)
            {
                if (db.GetXclsName(XClsType.Fds, FdsIDs[i]) == FdsName)
                {
                    id = FdsIDs[i];
                    break;
                }
            }

            return id;
        }
        public void Init()

        {

            string dsName = "MapGISLocal";
            //TopNode = this.treeView1.Nodes.Add(dsName);
            //treeView1.SelectedNode = TopNode;
            //TreeNode CurrentNode1 = TopNode;
            //实例化数据源
            Svr = new Server();
            //连接数据源

            Svr.Connect(dsName, " ", "");
            //获取当前数据源包含数据库的ID列表 
            List<int> ListStr = new List<int>();
            ListStr = Svr.GDBIDs;
            int count = ListStr.Count;
            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int id = ListStr[i];
                //根据数据库ID获取数据库名
                gdbName = Svr.GetDBName(id);
                //添加GDBcatalog节点
                //gdbNode = TopNode.Nodes.Add(gdbName); //添加数据库名到MapGISLocal下
                //gdbNode.ImageIndex = 1;
                //gdbNode.SelectedImageIndex = 1;
                sfclsNode = treeView1.Nodes.Add("sfcls", "MapGIS要素");//简单要素类
                                                                     //sfclsNode.ImageIndex = 2;
                                                                     //sfclsNode.SelectedImageIndex= 2;
                                                                     //annNode = gdbNode.Nodes.Add("ann", "注记类");
                                                                     //annNode.ImageIndex = 2;
                                                                     //annNode.SelectedImageIndex = 2;
                                                                     //objNode = gdbNode.Nodes.Add("obj", "对象类");
                                                                     //objNode.ImageIndex = 2;
                                                                     //objNode.SelectedImageIndex = 2;
                                                                     //relationNode = gdbNode.Nodes.Add("relacls","关系类");
                                                                     //relationNode.ImageIndex = 2;
                                                                     //relationNode.SelectedImageIndex = 2;

                //mapsetNode = gdbNode.Nodes.Add("mapset", "地图集");
                //mapsetNode.ImageIndex = 2;
                //mapsetNode.SelectedImageIndex = 2;

                //rascatNode = gdbNode.Nodes.Add("rascat", "栅格目录");
                //rascatNode.ImageIndex = 2;
                //rascatNode.SelectedImageIndex = 2;
                rasdstNode = treeView1.Nodes.Add("rasdst", "栅格数据");
                //rasdstNode.ImageIndex = 2;
                //rasdstNode.SelectedImageIndex = 2;

                //fdsNode0 = treeView1.Nodes.Add("fds", "要素数据集");
                //fdsNode0.ImageIndex = 2;
                //fdsNode0.SelectedImageIndex = 2;

                databaseNode = treeView1.Nodes.Add("database", "表格数据");
                // databaseNode.ImageIndex = 2;
                //databaseNode.SelectedImageIndex = 2;

                //根据数据库名打开数据库
                GDB = Svr.OpenGDB(gdbName);

                #region 读取栅格数据集和栅格目录
                List<int> rascatIDs = null;
                rascatIDs = GDB.GetXclses(XClsType.Rcat, 0);
                if (rascatIDs != null)
                {
                    for (int j = 0; j < rascatIDs.Count; j++)
                    {
                        rascatName0 = GDB.GetXclsName(XClsType.Rcat, rascatIDs[j]);
                        rascatNode0 = rascatNode.Nodes.Add(rascatName0);
                        rascat = new RasterCatalog(GDB);
                        rascat.Open(rascatName0);
                        int rasCount = rascat.GetItemNum();
                        string rasName = null;
                        for (int k = 1; k <= rasCount; k++)
                        {
                            rasName = rascat.GetItemName(k);
                            rascatNode0_ras = rascatNode0.Nodes.Add(rasName);
                            rascatNode0_ras.ImageIndex = 13;
                            rascatNode0_ras.SelectedImageIndex = 13;

                        }
                        rascatNode0.ImageIndex = 12;
                        rascatNode0.SelectedImageIndex = 12;

                    }

                }

                #endregion

                //                    #region  读取关系类
                //                    List<int> reclsIDs = null;
                //reclsIDs = GDB.GetXclses(XClsType.RCls, 0);
                //                    if(reclsIDs!=null)
                //                    {
                //                        for (int j = 0; j<reclsIDs.Count; j++)
                //                        {
                //                            relaclsName0 = GDB.GetXclsName(XClsType.RCls, reclsIDs[j]);
                //                            relationNode0 = relationNode.Nodes.Add(relaclsName0);
                //                            relationNode0.ImageIndex = 14;
                //                            relationNode0.SelectedImageIndex = 14;

                //                        }
                //                    }

                //                    #endregion

                //                    #region 读取地图集
                //                    List<int> mapsetIDs = null;
                //mapsetIDs = GDB.GetXclses(XClsType.MapSet, 0);
                //                    if (mapsetIDs != null)
                //                    {
                //                        for (int j = 0; j<mapsetIDs.Count; j++)
                //                        {
                //                            mapsetName0 = GDB.GetXclsName(XClsType.MapSet, mapsetIDs[j]);
                //                            mapsetNode0 = mapsetNode.Nodes.Add(mapsetName0);
                //                            mapsetNode0.ImageIndex = 15;
                //                            mapsetNode0.SelectedImageIndex = 15;

                //                        }
                //                    }

                //                    #endregion

                //获取ID列表
                List<int> dsIDs = null;
                FDsInfo fds = new FDsInfo();
                dsIDs = GDB.GetXclses(XClsType.Fds, 0);
                if (dsIDs != null)
                {
                    //获取要素数据集中简单要素类的个数
                    int cou = dsIDs.Count;
                    for (int j = 0; j < cou; j++)
                    {
                        //根据要素数据集ID取要素数据集信息
                        fdsName = GDB.GetXclsInfo(XClsType.Fds, dsIDs[j]).Name;
                        //取要素数据集名称
                        fdsNode = fdsNode0.Nodes.Add(fdsName);
                        fdsNode.ImageIndex = 3;
                        fdsNode.SelectedImageIndex = 3;
                        //取要素数据集内的简单要素类、注记类、对象类
                        xsfcls(XClsType.SFCls, dsIDs[j]);
                        xsfcls(XClsType.ACls, dsIDs[j]);
                        xsfcls(XClsType.OCls, dsIDs[j]);
                    }
                }
                //取要素数据集外的简单要素类、注记类、对象类 、栅格数据集
                xsfcls(XClsType.SFCls, 0);
                xsfcls(XClsType.ACls, 0);
                xsfcls(XClsType.OCls, 0);
                xsfcls(XClsType.Rds, 0);


                /*if (Svr.HasConnect)
                    连接数据源ToolStripMenuItem.Enabled = false;
                else
                    连接数据源ToolStripMenuItem.Enabled = true;  */

            }
        }
        public void xsfcls(XClsType type, int dsID)
        {

            List<int> xclsIDs = null;
            //获取要素类的ID列表
            xclsIDs = GDB.GetXclses(type, dsID);
            if (xclsIDs == null) return;

            //根据ID取要素类名    
            int cou = xclsIDs.Count;
            for (int i = 0; i < cou; i++)
            {
                //不属于要素数据集内的要素类          
                if (dsID == 0)
                {
                    switch (type)
                    {
                        case XClsType.SFCls:
                            xclsName = GDB.GetXclsName(XClsType.SFCls, xclsIDs[i]);
                            CurrentNode = sfclsNode.Nodes.Add(xclsName);
                            SFeatureCls sfcls = new SFeatureCls(GDB);
                            sfcls.Open(xclsName, 0);
                            //if (sfcls.GeomType == GeomType.Pnt)
                            //{
                            //    CurrentNode.ImageIndex = 4;
                            //    CurrentNode.SelectedImageIndex = 4;
                            //}
                            //else if (sfcls.GeomType == GeomType.Lin)
                            //{
                            //    CurrentNode.ImageIndex = 5;
                            //    CurrentNode.SelectedImageIndex = 5;
                            //}
                            //else if (sfcls.GeomType == GeomType.Reg)
                            //{
                            //    CurrentNode.ImageIndex = 6;
                            //    CurrentNode.SelectedImageIndex = 6;
                            //}
                            //else if (sfcls.GeomType == GeomType.Surface)
                            //{
                            //    CurrentNode.ImageIndex = 9;
                            //    CurrentNode.SelectedImageIndex = 9;
                            //}
                            break;
                        //case XClsType.ACls:
                        //    xclsName = GDB.GetXclsName(XClsType.ACls, xclsIDs[i]);
                        //    CurrentNode = annNode.Nodes.Add(xclsName);
                        //    CurrentNode.ImageIndex = 7;
                        //    //CurrentNode.SelectedImageIndex = 7;
                        //    break;
                        //case XClsType.OCls:
                        //    xclsName = GDB.GetXclsName(XClsType.OCls, xclsIDs[i]);
                        //    CurrentNode = objNode.Nodes.Add(xclsName);
                        //    //CurrentNode.ImageIndex = 8;
                        //    //CurrentNode.SelectedImageIndex = 8;
                        //    break;
                        case XClsType.Rds:
                            rasdstName0 = GDB.GetXclsName(XClsType.Rds, xclsIDs[i]);
                            rasdstNode0 = rasdstNode.Nodes.Add(rasdstName0);
                            //rasdstNode0.ImageIndex = 13;
                            //rasdstNode0.SelectedImageIndex = 13;
                            break;
                    }

                    CurrentNode.Tag = type;
                }
                else
                {
                    switch (type)
                    {
                        case XClsType.SFCls:
                            xclsName = GDB.GetXclsName(XClsType.SFCls, xclsIDs[i]);
                            CurrentNode = fdsNode.Nodes.Add(xclsName);
                            SFeatureCls sfcls = new SFeatureCls(GDB);
                            sfcls.Open(xclsName, 0);
                            //if (sfcls.GeomType == GeomType.Pnt)
                            //{
                            //    CurrentNode.ImageIndex = 4;
                            //    CurrentNode.SelectedImageIndex = 4;
                            //}
                            //else if (sfcls.GeomType == GeomType.Lin)
                            //{
                            //    CurrentNode.ImageIndex = 5;
                            //    CurrentNode.SelectedImageIndex = 5;
                            //}
                            //else if (sfcls.GeomType == GeomType.Reg)
                            //{
                            //    CurrentNode.ImageIndex = 6;
                            //    CurrentNode.SelectedImageIndex = 6;
                            //}
                            //else if (sfcls.GeomType == GeomType.Surface)
                            //{
                            //    CurrentNode.ImageIndex = 9;
                            //    CurrentNode.SelectedImageIndex = 9;
                            //}
                            break;
                        //case XClsType.ACls:
                        //    xclsName = GDB.GetXclsName(XClsType.ACls, xclsIDs[i]);
                        //    CurrentNode = fdsNode.Nodes.Add(xclsName);
                        //    //CurrentNode.ImageIndex = 7;
                        //    //CurrentNode.SelectedImageIndex = 7;
                        //    break;
                        //case XClsType.OCls:
                        //    xclsName = GDB.GetXclsName(XClsType.OCls, xclsIDs[i]);
                        //    CurrentNode = fdsNode.Nodes.Add(xclsName);
                        //    //CurrentNode.ImageIndex = 8;
                        //    //CurrentNode.SelectedImageIndex = 8;
                        //    break;
                    }


                    CurrentNode.Tag = type;
                }
            }
        }

        void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            //鼠标右键点击要素类名
            if (e.Button == MouseButtons.Right)
            {
                //记录鼠标点击坐标
                Point ClickPoint = new Point(e.X, e.Y);
                //根据坐标获取节点
                CurrentNode = treeView1.GetNodeAt(ClickPoint);
                xclsNode = treeView1.GetNodeAt(ClickPoint);

                //鼠标右击"MapGISLocal"时
                //if (CurrentNode.Level == 0)
                //{
                //    CurrentNode.ContextMenuStrip = dataSourceMS;
                //}

                ////右击数据库时
                //if (CurrentNode.Level == 1)
                //{

                //    //获取数据库的名字
                //    gdbName = CurrentNode.Text;
                //    CurrentNode.ContextMenuStrip = dataBaseMS;

                //}
                //if (CurrentNode.Level == 2)
                //{
                //    //获取当前点击节点的名（要素类名）
                //    xclsName = xclsNode.Text;
                //    //获取当前点击节点的父节点名（数据库名）
                //    gdbName = xclsNode.Parent.Text;
                //    //右击要素数据集
                //    if (CurrentNode.Name == "fds")
                //    {
                //        CurrentNode.ContextMenuStrip = fdsMS;
                //    }
                //    //右击简单要素类、注记类、对象类、栅格目录、栅格数据集
                //    if (CurrentNode.Name == "sfcls" || CurrentNode.Name == "ann" || CurrentNode.Name == "obj" || CurrentNode.Name == "rascat" || CurrentNode.Name == "rasdst")
                //    {

                //        if (CurrentNode.Name == "rascat" || CurrentNode.Name == "rasdst")
                //        {
                //            if (sfclsMS.Items.Count > 1)
                //            {
                //                sfclsMS.Items.RemoveAt(1);
                //                sfclsMS.Items.RemoveAt(1);
                //            }
                //        }
                //        else
                //        {
                //            sfclsMS.Items.Add("导入");
                //            sfclsMS.Items.Add("导出");
                //        }
                //        CurrentNode.ContextMenuStrip = sfclsMS;

                //    }
                //    if (CurrentNode.Name == "relacls" || CurrentNode.Name == "mapset")
                //    {
                //        CurrentNode.ContextMenuStrip = relacls_mapsetMS;
                //    }


                //}

                //右击要素类名
                if (CurrentNode.Level == 3)
                {
                    //获取当前点击节点的名（要素类名）
                    xclsName = xclsNode.Text;
                    //获取当前点击节点的父节点名（数据库名）
                    gdbName = xclsNode.Parent.Parent.Text;
                    if (CurrentNode.Parent.Name == "sfcls")
                    {

                        CurrentNode.ContextMenuStrip = xclsMenuStrip;
                    }
                    if (CurrentNode.Parent.Name == "ann")
                    {
                        CurrentNode.ContextMenuStrip = xclsMenuStrip;
                    }
                    //if (CurrentNode.Parent.Name == "fds")
                    //{
                    //    CurrentNode.ContextMenuStrip = sfclsMS;
                    //}
                    //if (CurrentNode.Parent.Name == "rasdst")
                    //{
                    //    CurrentNode.ContextMenuStrip = rdsMS;
                    //}

                }

                //右击要素数据集里的要素类名
                //if (CurrentNode.Level == 4)
                //{
                //    if (CurrentNode.Parent.Parent.Name == "rascat")
                //    {
                //        CurrentNode.ContextMenuStrip = rdsMS;
                //    }
                //    else
                //    {
                //        //获取当前点击节点的父节点名（数据库名）
                //        CurrentNode.ContextMenuStrip = xclsMenuStrip;
                //    }
                //    //获取当前点击节点的名（要素类名）
                //    xclsName = CurrentNode.Text;
                //    gdbName = CurrentNode.Parent.Parent.Parent.Text;
                //}
            }

        }

        private void 预览ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            VectorLayer layer = openXcls(CurrentNode.Tag.ToString());
            //显示要素类
            
            map.Append(layer);
            mapCtrl.ActiveMap = map;
            mapCtrl.Restore();
        }

        public VectorLayer openXcls(string type)
        {
            map = new Map();
            //根据数据库名打开数据库
            GDB = Svr.OpenGDB(gdbName);
            sfCls = new SFeatureCls(GDB);
            anCls = new AnnotationCls(GDB);
            VectorLayer vecLayer1 = null;
            switch (type)
            {
                case "SFCls":
                    //根据类名打开简单要素
                    sfCls.Open(xclsName, 0);
                    //实例化
                    vecLayer1 = new VectorLayer(VectorLayerType.SFclsLayer);
                    vecLayer1.URL = sfCls.URL;
                    bool rtn = vecLayer1.ConnectData();
                    sfCls.Close();
                    GDB.Close();
                    break;
                case "ACls":
                    anCls.Open(xclsName, 0);
                    vecLayer1 = new VectorLayer(VectorLayerType.AnnLayer);
                    bool r = vecLayer1.AttachData(anCls);
                    break;
            }
            return vecLayer1;
        }

        public void OnDestroy()
        {
        }

        #endregion
    }
}