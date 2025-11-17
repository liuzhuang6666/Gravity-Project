// Form_ContourVisualization.cs
using System;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
namespace MapGISPlugin3
{
    public partial class Form_ContourVisualization : Form
    {
        private IApplication m_Hook;
        private RasterLayer _selectedRasterLayer;
        private Map _selectedMap;
        public Form_ContourVisualization(IApplication hook)
        {
            InitializeComponent();
            m_Hook = hook;
        }
        private void Form_ContourVisualization_Load(object sender, EventArgs e)
        {
            PopulateLayerTree();
            this.btnOK.Enabled = false;
        }
        private void PopulateLayerTree()
        {
            treeViewLayers.Nodes.Clear();
            Document doc = m_Hook.Document;
            if (doc == null) return;
            Maps maps = doc.GetMaps();
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps.GetMap(i);
                if (map == null) continue;
                TreeNode mapNode = new TreeNode(map.Name) { Tag = map };
                treeViewLayers.Nodes.Add(mapNode);
                for (int j = 0; j < map.LayerCount; j++)
                {
                    MapLayer layer = map.get_Layer(j);
                    if (layer is RasterLayer)
                    {
                        TreeNode layerNode = new TreeNode(layer.Name) { Tag = layer };
                        mapNode.Nodes.Add(layerNode);
                    }
                }
            }
            treeViewLayers.ExpandAll();
        }
        private void treeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is RasterLayer)
            {
                _selectedRasterLayer = e.Node.Tag as RasterLayer;
                _selectedMap = e.Node.Parent.Tag as Map;
                this.btnOK.Enabled = true;
            }
            else
            {
                _selectedRasterLayer = null;
                _selectedMap = null;
                this.btnOK.Enabled = false;
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_selectedRasterLayer == null || _selectedMap == null)
            {
                MessageBox.Show("请先在树状图中选择一个栅格图层。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            double interval = (double)this.numericUpDownInterval.Value;
            if (interval <= 0)
            {
                MessageBox.Show("等值线间距必须大于0。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            this.btnOK.Enabled = false;
            ContourVisualizer visualizer = null;
            try
            {
                visualizer = new ContourVisualizer(
                    _selectedRasterLayer,
                    _selectedMap,
                    interval,
                    m_Hook);
                if (visualizer.Execute())
                {
                    MessageBox.Show("等值线可视化成功！相关图层已添加到地图中。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("处理过程中发生未预料的错误：\n" + ex.Message, "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                visualizer?.Dispose();
                this.Cursor = Cursors.Default;
                this.btnOK.Enabled = true;
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}