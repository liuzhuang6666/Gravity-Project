using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTreeList.Nodes;
using MapGISPlugin3;
using System.IO;
using System.Xml;
using System.Drawing;
using DevExpress.Utils;
using DevExpress.XtraBars;
using MapGIS.PluginEngine;
using DevExpress.XtraTreeList;
using MapGISPlugin3.Properties;
using MapGIS.GeoObjects;
using MapGIS.GeoMap;
using MapGIS.GISControl;
using MapGIS.PlugUtility;

namespace MapGISPlugin3
{
    public partial class ProjectTreeControl : XtraUserControl
    {
        private const string NoData = "NoData";
        private Project project = null;
        private Dictionary<string, int> selectIndex = new Dictionary<string, int>();
        private Dictionary<string, string> selectValue = new Dictionary<string, string>();
        private IApplication app;
        private List<IRightMenuBar> menuManage = new List<IRightMenuBar>();
        private List<TreeItemInfo> selectItems = null;
        private bool isFirst = true;

        public ProjectTreeControl(IApplication hk)
        {
            InitializeComponent();
            this.app = hk;
            string configPath = StaticFunctions.GetConfigPath();
            OpenOrCreateMenuBar openOrCreate = new OpenOrCreateMenuBar();
            menuManage.Add(openOrCreate);
            ItemMenuBar item = new ItemMenuBar();
            menuManage.Add(item);
            this.treeList1.SelectionChanged += TreeList1_SelectionChanged;
        }

        public List<TreeItemInfo> SelectItems
        {
            get { return selectItems; }
        }

        private void TreeList1_SelectionChanged(object sender, EventArgs e)
        {
            selectItems = GetSelectItems();
            if (app != null && app.StateManager != null)
                app.StateManager.OnStateChanged(null, null);
        }

        private List<TreeItemInfo> GetSelectItems()
        {
            TreeListMultiSelection selNodes = treeList1.Selection;
            List<TreeItemInfo> rtn = new List<TreeItemInfo>();

            if (selNodes != null && selNodes.Count > 0)
            {
                for (int i = 0; i < selNodes.Count; i++)
                {
                    TreeListNode node = selNodes[i];
                    if (node != null)
                    {
                        TreeItemInfo item = null;
                        if (node.Tag is object[] && (node.Tag as object[]).Length == 2 && (node.Tag as object[])[0] is DataInfo)
                        {
                            item = ((node.Tag as object[])[0] as DataInfo).TreeInfo;
                        }
                        else if (node.Tag is TreeItemInfo)
                        {
                            item = (node.Tag as TreeItemInfo);
                        }
                        if (item != null)
                            rtn.Add(item);
                    }
                }
            }
            return rtn;
        }

       
      

        public void SetProject(Project Prj)
        {
            project = Prj;
            selectItems = null;
            LoadTree();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void LoadTree()
        {
            this.treeList1.SelectionChanged -= new System.EventHandler(this.treeList1_SelectionChanged);
            this.treeList1.Nodes.Clear();
            if (this.project == null || this.project.DataInfo == null)
                return;
            this.treeList1.BeginUpdate();

            MapGIS.GeoMap.Document doc = app.Document;
            if (doc.GetMaps() != null && doc.GetMaps().Count > 0)
            {
                Map Map = doc.GetMaps().GetMap(0);
                for (int i = 0; i < Map.LayerCount; i++)
                {
                    MapLayer layer = Map.get_Layer(i);
                    if (layer != null)
                    {
                        if (layer is MapLayer)
                        {
                            TreeListNode rootNode = this.treeList1.AppendNode(new object[] { this.project.DataInfo.DataName }, -1, 0, 0, 0, new object[] { this.project.DataInfo, layer });
                            if (this.project.DataInfo.TreeInfo != null && this.project.DataInfo.TreeInfo.ChildrenNode != null)
                            {
                                List<TreeItemInfo> treeInfos = this.project.DataInfo.TreeInfo.ChildrenNode;
                                for (int j = 0; j < treeInfos.Count; j++)
                                {
                                    TreeItemInfo info = treeInfos[j];
                                    if (info != null)
                                    {
                                        TreeListNode node = AddNode(rootNode, info);
                                    }
                                }
                            }
                            rootNode.ExpandAll();
                        }
                        else
                            this.treeList1.AppendNode(new object[] { layer.Name }, -1, 0, 0, 0, layer);
                    }
                }
            }
            this.treeList1.EndUpdate();
            this.treeList1.SelectionChanged += new System.EventHandler(this.treeList1_SelectionChanged);
        }

        /// <summary>
        /// 更新节点选择图标
        /// </summary>
        /// <param name="node"></param>
        public void UpdateSelectImage(TreeListNode node = null, bool updateChild = true)
        {
            if (node == null)
            {
                if (!updateChild)
                    return;
                TreeListNodes nodes = this.treeList1.Nodes;
                if (nodes != null)
                {
                    foreach (TreeListNode childNode in nodes)
                    {
                        UpdateSelectImage(childNode, updateChild);
                    }
                }
            }
            else
            {
                TreeItemInfo item = null;
                int imageIndex = -1;
                if (node.Tag is TreeItemInfo)
                {
                    item = (node.Tag as TreeItemInfo);
                    if (item != null)
                    {
                        int index = item.TileDataInfoIndex;
                        if (item.TileDataInfoList != null && item.TileDataInfoList.Count > index)
                        {
                            if (index >= 0 && item.TileDataInfoList[index] != null)
                            {
                                string key = item.TileDataInfoList[index].DataType.ToString();
                                if (selectIndex.ContainsKey(key))
                                {
                                    imageIndex = selectIndex[key];
                                }
                                else if (selectIndex.ContainsKey(NoData))
                                    imageIndex = selectIndex[NoData];
                            }
                            else if (selectIndex.ContainsKey(NoData))
                                imageIndex = selectIndex[NoData];
                        }
                        else if (selectIndex.ContainsKey(NoData))
                            imageIndex = selectIndex[NoData];
                    }
                }
                else if (node.Tag is object[])
                {
                    object[] objects = node.Tag as object[];
                    if (objects != null && objects.Length == 2 && objects[1] is MapLayer)
                    {
                        LayerState layerState = (objects[1] as MapLayer).State;
                        if (layerState == LayerState.UnVisible)
                            imageIndex = 0;
                        else if (layerState == LayerState.Visible)
                            imageIndex = 1;
                        else if (layerState == LayerState.Editable)
                            imageIndex = 2;
                        else if (layerState == LayerState.Active)
                            imageIndex = 3;
                    }
                }
                else if (node.Tag is MapLayer)
                {
                    LayerState layerState = (node.Tag as MapLayer).State;
                    if (layerState == LayerState.UnVisible)
                        imageIndex = 0;
                    else if (layerState == LayerState.Visible)
                        imageIndex = 1;
                    else if (layerState == LayerState.Editable)
                        imageIndex = 2;
                    else if (layerState == LayerState.Active)
                        imageIndex = 3;
                }
                if (imageIndex >= 0)
                {
                    node.ImageIndex = imageIndex;
                    node.SelectImageIndex = imageIndex;
                }
                if (updateChild)
                {
                    TreeListNodes nodes = node.Nodes;
                    if (nodes != null)
                    {
                        foreach (TreeListNode childNode in nodes)
                        {
                            UpdateSelectImage(childNode, updateChild);
                        }
                    }
                }
            }
            return;
        }


        public int UpDateNode()
        {
            TreeListNode node = this.treeList1.FocusedNode;
            if (node != null)
                return UpDateNode(node);
            return 0;
        }
        public int UpDateNode(TreeListNode node)
        {
            if (node != null && (((node.Tag is object[]) && (node.Tag as object[]).Length == 2 && (node.Tag as object[])[0] is DataInfo) || node.Tag is TreeItemInfo))
            {
                node.Nodes.Clear();
                if (node.Tag is object[] && (node.Tag as object[]).Length == 2 && (node.Tag as object[])[0] is DataInfo)
                {
                   DataInfo dataInfo = (node.Tag as object[])[0] as DataInfo;
                    if (dataInfo.TreeInfo != null && dataInfo.TreeInfo.ChildrenNode != null)
                    {
                        List<TreeItemInfo> treeInfos = dataInfo.TreeInfo.ChildrenNode;
                        for (int i = 0; i < treeInfos.Count; i++)
                        {
                            TreeItemInfo info = treeInfos[i];
                            if (info != null)
                            {
                                AddNode(node, info);
                            }
                        }
                    }
                }
                else
                {
                    node[0] = (node.Tag as TreeItemInfo).Name;
                    if ((node.Tag as TreeItemInfo).ChildrenNode != null)
                    {
                        List<TreeItemInfo> treeInfos = (node.Tag as TreeItemInfo).ChildrenNode;
                        for (int i = 0; i < treeInfos.Count; i++)
                        {
                            TreeItemInfo info = treeInfos[i];
                            if (info != null)
                            {
                                AddNode(node, info);
                            }
                        }
                    }
                }

    
            }
            return 1;
        }

        private TreeListNode AddNode(TreeListNode parentNode, TreeItemInfo info)
        {
            TreeListNode rtn = null;
            if (info != null && parentNode != null)
            {
                rtn = this.treeList1.AppendNode(new object[] { info.Name }, parentNode.Id, 0, 0, 0, info);
                if (rtn != null)
                {
                    if (info.ChildrenNode != null && info.ChildrenNode.Count > 0)
                    {
                        for (int i = 0; i < info.ChildrenNode.Count; i++)
                        {
                            TreeItemInfo item = info.ChildrenNode[i];
                            if (item != null)
                            {
                                AddNode(rtn, item);
                            }
                        }
                    }
                }
            }
            return rtn;
        }

        private void treeList1_MouseUp(object sender, MouseEventArgs e)
        {

        }

      

        private void ResetListItem(List<BarItem> listItem)
        {
            TreeListMultiSelection sel = this.treeList1.Selection;
            List<TreeListNode> nodes = new List<TreeListNode>();
            foreach (TreeListNode trNode in sel)
            {
                if (trNode != null)
                {
                    nodes.Add(trNode);
                }
            }
            for (int i = listItem.Count - 1; i >= 0; i--)
            {
                BarItem item = listItem[i];
                if (item is BarSubItem)
                {
                    BarSubItem subItem = (item as BarSubItem);
                    LinksInfo links = subItem.LinksPersistInfo;
                    List<BarItem> subitems = new List<BarItem>();
                    for (int j = 0; j < links.Count; j++)
                    {
                        LinkPersistInfo link = links[j];
                        BarItem barItem = link.Item;
                        subitems.Add(barItem);
                    }
                    if (subitems != null && subitems.Count > 0)
                    {
                        ResetListItem(subitems);
                    }
                    subItem.ClearLinks();
                    if (subitems == null || subitems.Count <= 0)
                        listItem.Remove(item);
                    else
                        subItem.AddItems(subitems.ToArray());
                }
                else if (item is BarButtonItem)
                {
                    object[] tag = item.Tag as object[];
                    if (tag != null && tag.Length == 2 && tag[0] != null)
                    {
                        if (tag[0] is IMenuCommand)
                        {
                            if (!(tag[0] as IMenuCommand).IsVisible(this, nodes.ToArray()))
                                listItem.Remove(item);
                            else if (!(tag[0] as IMenuCommand).IsEditable(this, nodes.ToArray()))
                                item.Enabled = false;
                            else
                                (item as BarButtonItem).ItemClick += ProjectTreeControl_ItemClick; ;
                        }
                    }
                    else if (tag != null && tag.Length == 3)
                    {
                        (item as BarButtonItem).ItemClick += ProjectTreeControl_ItemClick;
                    }
                }
            }
        }

        private void ProjectTreeControl_ItemClick(object sender, ItemClickEventArgs e)
        {
            TreeListMultiSelection sel = this.treeList1.Selection;
            List<TreeListNode> nodes = new List<TreeListNode>();
            foreach (TreeListNode trNode in sel)
            {
                if (trNode != null)
                {
                    nodes.Add(trNode);
                }
            }
            if (e.Item is BarButtonItem && (e.Item as BarButtonItem).Tag is object[])
            {
                object[] tag = (e.Item as BarButtonItem).Tag as object[];
                if (tag != null && tag.Length == 2 && tag[0] != null)
                {
                    if (tag[0] is IMenuCommand)
                    {
                        (tag[0] as IMenuCommand).OnClick(this, nodes.ToArray());
                    }
                    else if (tag[0] is ICommand)
                    {
                        (tag[0] as ICommand).OnClick();
                    }
                }
                else if (tag != null && tag.Length == 3)
                {
                    string name = "";
                    string type = "";
                    int index = -1;
                    if (tag != null && tag is object[] && (tag as object[]).Length == 3 && (tag as object[])[0] != null)
                    {
                        name = (tag as object[])[0].ToString();
                        type = (tag as object[])[1] == null ? null : (tag as object[])[1].ToString();
                        index = (tag as object[])[2] == null ? -1 : (int.TryParse((tag as object[])[2].ToString(), out index) ? index : -1);
                    }

                    if (sel.Count == 1)
                    {
                        TreeItemInfo itemInfo = null;
                        if (sel[0] != null && (sel[0].Tag is object[]) && (sel[0].Tag as object[]).Length == 2 && (sel[0].Tag as object[])[0] is DataInfo)
                            itemInfo = ((sel[0].Tag as object[])[0] as DataInfo).TreeInfo;
                        else if (sel[0] != null && sel[0].Tag is TreeItemInfo)
                            itemInfo = (sel[0].Tag as TreeItemInfo);
                        if (itemInfo != null)
                        {
                            itemInfo.TileDataInfoIndex = index;

                            //ThreeDimensionBridge.StaticFun.SetNodeShowTile(project.McjPath, Path.GetFullPath(itemInfo.Path), index);

                            string path = StaticFunctions.GetRelativePathBympjPath(Path.GetFullPath(itemInfo.Path), project.MpjPath);

                            if (path.IndexOf("node") > 0)
                            {

                                path = path.Substring(path.IndexOf("node") + 4);
                            }
                            while (path.StartsWith("\\"))
                                path = path.Remove(0, 1);
                            while (path.StartsWith("/"))
                                path = path.Remove(0, 1);
                            SetLayerShowTileIndex(path, index);


                            //MapControl MapControl = app.WorkSpaceEngine.GetMapControl();

                            //if (index < 0)
                            //{
                            //    MapControl.UpdateMap();
                            //}
                            //else
                            //{

                            //}


                            // M3dDocumentClass.UpdateTreeItemInfoFile((node.Tag as M3DDataInfo).TreeInfo);
                            UpdateSelectImage(sel[0], false);
                        }
                    }
                }
            }
        }

        private void SetLayerShowTileIndex(string path, int index)
        {
            if (app.ActiveContentsView is IMapContentsView && !string.IsNullOrWhiteSpace(path))
            {
                IMapContentsView controView = (app.ActiveContentsView as IMapContentsView);
                MapControl MapControl = controView.MapControl;
                if (MapControl != null && MapControl.ActiveMap!= null)
                {
                    Map Map = MapControl.ActiveMap;
                    int layerIndex = -1;
                    for (int i = 0; i < Map.LayerCount; i++)
                    {
                        MapLayer layer = Map.get_Layer(i);

                        if (layer != null && layer is MapLayer)
                        {
                           // layerIndex = layer.GetLayerRenderIndex();
                            break;
                        }

                    }
                    if (layerIndex >= 0)
                    {  //ThreeDimensionBridge.StaticFun.SetLayerShowTileIndex(MapControl, Map, layerIndex, path, index);


                    }
                }
            }
        }
        private void treeList1_BeforeExpand(object sender, DevExpress.XtraTreeList.BeforeExpandEventArgs e)
        {
        }
        //不同级节点不允许多选
        private void treeList1_SelectImageClick(object sender, DevExpress.XtraTreeList.NodeClickEventArgs e)
        {
        }
        private void treeList1_BeforeFocusNode(object sender, DevExpress.XtraTreeList.BeforeFocusNodeEventArgs e)
        {

        }

        /// <summary>
        /// 随树节点选择项改变图层属性框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_SelectionChanged(object sender, EventArgs e)
        {
            if (isFirst)
            {
                isFirst = false;
                try
                {
                    TreeListMultiSelection sel = this.treeList1.Selection;
                    List<TreeListNode> nodes = new List<TreeListNode>();
                    int minLevel = int.MaxValue;
                    foreach (TreeListNode trNode in sel)
                    {
                        if (trNode != null)
                        {
                            minLevel = Math.Min(minLevel, trNode.Level);
                        }
                    }
                    if (minLevel < int.MaxValue)
                    {
                        foreach (TreeListNode trNode in sel)
                        {
                            if (trNode != null && trNode.Level != minLevel)
                            {
                                nodes.Add(trNode);
                            }
                        }
                    }
                    if (nodes != null && nodes.Count > 0)
                    {
                        this.treeList1.Selection.UnselectNodes(nodes);
                    }

                    if (this.treeList1.Selection.Count == 0)
                    {
                        TreeListNode focNode = this.treeList1.FocusedNode;
                        if (focNode != null)
                        {
                            focNode.Selected = true;
                            return;
                        }
                    }
                }
                catch { }
                finally
                {
                    isFirst = true;
                }
            }
        }
    }
}