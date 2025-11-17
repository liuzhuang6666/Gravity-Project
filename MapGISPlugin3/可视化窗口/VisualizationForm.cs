// VisualizationForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using MapGIS.GeoMap;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public partial class VisualizationForm : Form
    {
        private IApplication m_Hook;

        // 公共属性，用于从外部获取用户选择的结果
        public RasterLayer SelectedRasterLayer { get; private set; }
        public Map SelectedMap { get; private set; }

        // 【新增】一个公共属性，用于向外部传递用户设置的间隔值
        public decimal ContourInterval { get; private set; }

        public VisualizationForm(IApplication hook)
        {
            m_Hook = hook;
            InitializeComponent();
            PopulateTreeView();
        }

        // ... (此处省略了PopulateTreeView等方法的代码，因为它们与上一个回答中完全相同，为了简洁)
        // ... (请将上一个回答中 VisualizationForm.cs 的完整代码粘贴到这里)
        private void PopulateTreeView()
        {
            treeViewLayers.Nodes.Clear();
            Document doc = m_Hook.Document;
            if (doc == null)
            {
                MessageBox.Show("无法获取主文档对象。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Maps maps = doc.GetMaps();
            if (maps == null) return;

            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps.GetMap(i);
                if (map == null) continue;
                TreeNode mapNode = new TreeNode(map.Name);
                mapNode.Tag = map;
                treeViewLayers.Nodes.Add(mapNode);
                AddLayersToNode(mapNode, map);
            }
            if (treeViewLayers.Nodes.Count > 0)
            {
                treeViewLayers.ExpandAll();
            }
        }

        private void AddLayersToNode(TreeNode parentNode, Map map)
        {
            for (int i = 0; i < map.LayerCount; i++)
            {
                MapLayer layer = map.get_Layer(i);
                if (layer != null) ProcessLayer(parentNode, layer);
            }
        }

        private void AddLayersToNode(TreeNode parentNode, GroupLayer groupLayer)
        {
            for (int i = 0; i < groupLayer.Count; i++)
            {
                MapLayer layer = groupLayer.get_Item(i);
                if (layer != null) ProcessLayer(parentNode, layer);
            }
        }

        private void ProcessLayer(TreeNode parentNode, MapLayer layer)
        {
            if (layer is GroupLayer groupLayer)
            {
                TreeNode groupNode = new TreeNode(groupLayer.Name);
                groupNode.Tag = groupLayer;
                parentNode.Nodes.Add(groupNode);
                AddLayersToNode(groupNode, groupLayer);
            }
            else if (layer is RasterLayer)
            {
                TreeNode layerNode = new TreeNode(layer.Name);
                layerNode.Tag = layer;
                parentNode.Nodes.Add(layerNode);
            }
        }

        private void TreeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnOK.Enabled = (e.Node != null && e.Node.Tag is RasterLayer);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeViewLayers.SelectedNode;
            if (selectedNode != null && selectedNode.Tag is RasterLayer)
            {
                SelectedRasterLayer = selectedNode.Tag as RasterLayer;

                TreeNode node = selectedNode;
                while (node != null && !(node.Tag is Map))
                {
                    node = node.Parent;
                }
                if (node != null)
                {
                    SelectedMap = node.Tag as Map;
                }

                // 【新增】在关闭窗口前，记录下 NumericUpDown 的值
                this.ContourInterval = this.numericUpDownInterval.Value;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}