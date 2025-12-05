using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoMap;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MapGIS.PluginEngine;
using System.Drawing;

namespace MapGISPlugin3
{
    /// <summary>
    /// MT 追加观测数据窗体
    /// 功能：向已有的测点要素类和测深数据表追加新的观测数据
    /// </summary>
    public partial class Form_MTAppendData : Form
    {
        private Point mousePoint = new Point();
        private IApplication _hook;

        private string _selectedStationUrl = "";
        private string _selectedSoundingUrl = "";

        public Form_MTAppendData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            InitDragEvent();
        }

        private void Form_MTAppendData_Load(object sender, EventArgs e)
        {
            PopulateTreeView();
        }

        #region --- 目录树填充逻辑 ---

        private void PopulateTreeView()
        {
            treeViewLayers.Nodes.Clear();
            if (_hook == null || _hook.Document == null)
            {
                MessageBox.Show("未能获取到当前地图文档！", "环境错误");
                return;
            }

            try
            {
                Maps maps = _hook.Document.GetMaps();
                if (maps == null || maps.Count == 0)
                {
                    MessageBox.Show("当前文档中没有地图。", "提示");
                    return;
                }

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
            catch (Exception ex)
            {
                MessageBox.Show($"填充图层列表时出错: {ex.Message}", "错误");
            }
        }

        private void AddLayersToNode(TreeNode parentNode, Map map)
        {
            if (map == null || parentNode == null) return;
            for (int i = 0; i < map.LayerCount; i++)
            {
                MapLayer layer = map.get_Layer(i);
                if (layer != null) ProcessLayer(parentNode, layer);
            }
        }

        private void AddLayersToNode(TreeNode parentNode, GroupLayer groupLayer)
        {
            if (groupLayer == null || parentNode == null) return;
            for (int i = 0; i < groupLayer.Count; i++)
            {
                MapLayer layer = groupLayer.get_Item(i);
                if (layer != null) ProcessLayer(parentNode, layer);
            }
        }

        private void ProcessLayer(TreeNode parentNode, MapLayer layer)
        {
            if (layer == null || parentNode == null) return;

            try
            {
                if (layer is GroupLayer)
                {
                    GroupLayer currentGroup = layer as GroupLayer;
                    if (!IsRecursiveGroup(parentNode, currentGroup))
                    {
                        TreeNode groupNode = new TreeNode(currentGroup.Name);
                        groupNode.Tag = currentGroup;
                        parentNode.Nodes.Add(groupNode);
                        AddLayersToNode(groupNode, currentGroup);
                    }
                }
                else if (layer is VectorLayer)
                {
                    TreeNode layerNode = new TreeNode(layer.Name + " [测点]");
                    layerNode.Tag = layer;
                    layerNode.ForeColor = System.Drawing.Color.Blue;
                    parentNode.Nodes.Add(layerNode);
                }
                else if (layer is ObjectLayer)
                {
                    TreeNode layerNode = new TreeNode(layer.Name + " [测深数据]");
                    layerNode.Tag = layer;
                    layerNode.ForeColor = System.Drawing.Color.Green;
                    parentNode.Nodes.Add(layerNode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProcessLayer 出错: {ex.Message}");
            }
        }

        private bool IsRecursiveGroup(TreeNode parentNode, GroupLayer currentGroup)
        {
            TreeNode current = parentNode;
            int depth = 0;
            while (current != null && depth < 20)
            {
                if (object.ReferenceEquals(current.Tag, currentGroup)) return true;
                current = current.Parent;
                depth++;
            }
            return false;
        }

        #endregion

        #region --- 事件处理程序 ---

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择 MT 观测数据文件 (. dat)";
                openFileDialog.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void treeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            if (e.Node.Tag is VectorLayer)
            {
                VectorLayer vl = e.Node.Tag as VectorLayer;
                txtSelectedStation.Text = vl.Name;
                _selectedStationUrl = vl.URL;
                FindCorrespondingSoundingTable(e.Node.Parent, vl.Name);
            }
            else if (e.Node.Tag is ObjectLayer)
            {
                ObjectLayer ol = e.Node.Tag as ObjectLayer;
                txtSelectedSounding.Text = ol.Name;
                _selectedSoundingUrl = ol.URL;
                FindCorrespondingStationLayer(e.Node.Parent, ol.Name);
            }
        }

        private void FindCorrespondingSoundingTable(TreeNode parentNode, string stationName)
        {
            if (parentNode == null) return;
            string baseName = stationName.Replace("_测点_", "_").Split('_')[0];

            foreach (TreeNode node in parentNode.Nodes)
            {
                if (node.Tag is ObjectLayer)
                {
                    ObjectLayer ol = node.Tag as ObjectLayer;
                    if (ol.Name.Contains(baseName) && ol.Name.Contains("测深"))
                    {
                        txtSelectedSounding.Text = ol.Name;
                        _selectedSoundingUrl = ol.URL;
                        break;
                    }
                }
            }
        }

        private void FindCorrespondingStationLayer(TreeNode parentNode, string soundingName)
        {
            if (parentNode == null) return;
            string baseName = soundingName.Replace("_测深数据_", "_").Split('_')[0];

            foreach (TreeNode node in parentNode.Nodes)
            {
                if (node.Tag is VectorLayer)
                {
                    VectorLayer vl = node.Tag as VectorLayer;
                    if (vl.Name.Contains(baseName) && vl.Name.Contains("测点"))
                    {
                        txtSelectedStation.Text = vl.Name;
                        _selectedStationUrl = vl.URL;
                        break;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnOK.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                string inputFile = txtInputFile.Text;
                if (string.IsNullOrWhiteSpace(inputFile) || !File.Exists(inputFile))
                {
                    MessageBox.Show("请选择有效的观测数据文件。", "错误");
                    ResetFormState();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_selectedStationUrl))
                {
                    MessageBox.Show("请在目录树中选择要追加数据的测点图层。", "错误");
                    ResetFormState();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_selectedSoundingUrl))
                {
                    MessageBox.Show("请在目录树中选择要追加数据的测深数据表。", "错误");
                    ResetFormState();
                    return;
                }

                // 读取MT观测数据文件
                // MT数据格式: 测线号 测点号 X Y 周期 视电阻率_TE 相位_TE 视电阻率_TM 相位_TM
                var allDataRows = new List<MTDataRow>();
                var uniqueStations = new Dictionary<string, StationInfo>();

                Console.WriteLine("=== 开始读取MT观测数据文件 ===");
                using (StreamReader reader = new StreamReader(inputFile, Encoding.Default))
                {
                    string line;
                    int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (lineNumber == 1 && IsHeaderLine(line)) continue;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] parts = Regex.Split(line.Trim(), @"\s+");
                        if (parts.Length < 9) continue;

                        try
                        {
                            MTDataRow rowData = new MTDataRow
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                Period = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                Res_TE = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                                Phase_TE = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture),
                                Res_TM = double.Parse(parts[7], System.Globalization.CultureInfo.InvariantCulture),
                                Phase_TM = double.Parse(parts[8], System.Globalization.CultureInfo.InvariantCulture)
                            };
                            allDataRows.Add(rowData);

                            string stationKey = $"{rowData.LineName}_{rowData.StationName}";
                            if (!uniqueStations.ContainsKey(stationKey))
                            {
                                uniqueStations.Add(stationKey, new StationInfo
                                {
                                    LineName = rowData.LineName,
                                    StationName = rowData.StationName,
                                    X = rowData.X,
                                    Y = rowData.Y
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"警告: 行 {lineNumber} 解析失败: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine($"读取完成: {allDataRows.Count} 条观测记录, {uniqueStations.Count} 个测点。");

                if (allDataRows.Count == 0 || uniqueStations.Count == 0)
                {
                    MessageBox.Show("未读取到有效观测数据。", "错误");
                    ResetFormState();
                    return;
                }

                bool stationAppendSuccess = AppendStationData(uniqueStations);
                bool soundingAppendSuccess = AppendSoundingData(allDataRows);

                if (stationAppendSuccess && soundingAppendSuccess)
                {
                    MessageBox.Show($"追加成功！\n新增测点: {uniqueStations.Count} 个\n新增测深记录: {allDataRows.Count} 条", "完成");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    string errorMsg = "";
                    if (!stationAppendSuccess) errorMsg += "测点数据追加失败。\n";
                    if (!soundingAppendSuccess) errorMsg += "测深数据追加失败。";
                    MessageBox.Show(errorMsg, "部分失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"追加数据时出错: {ex.Message}", "错误");
                Console.WriteLine($"追加数据异常: {ex}");
            }
            finally
            {
                ResetFormState();
            }
        }

        /// <summary>
        /// 追加测点数据到已有的 SFeatureCls
        /// MT测点字段: 测线号, 测点号, X坐标, Y坐标
        /// </summary>
        private bool AppendStationData(Dictionary<string, StationInfo> uniqueStations)
        {
            SFeatureCls stationSfc = null;
            try
            {
                stationSfc = new SFeatureCls();
                if (!stationSfc.Open(_selectedStationUrl))
                {
                    MessageBox.Show($"无法打开测点要素类: {_selectedStationUrl}", "错误");
                    return false;
                }

                Fields existingFields = stationSfc.Fields;
                if (existingFields == null)
                {
                    MessageBox.Show("无法获取测点要素类的字段结构。", "错误");
                    return false;
                }

                Console.WriteLine("开始追加MT测点数据...");
                stationSfc.BeginBatch(BatchType.Append);

                Record stationRecord = new Record { Fields = existingFields };
                int appendCount = 0;

                foreach (var stationInfo in uniqueStations.Values)
                {
                    Dot3D pnt3D = new Dot3D(stationInfo.X, stationInfo.Y, 0.0);
                    GeoPoints currentPnts = new GeoPoints();
                    currentPnts.Append(pnt3D);

                    // MT 测点字段名（与Form_MTAddData一致）
                    stationRecord["测线号"] = stationInfo.LineName;
                    stationRecord["测点号"] = stationInfo.StationName;
                    stationRecord["X坐标"] = stationInfo.X;
                    stationRecord["Y坐标"] = stationInfo.Y;

                    if (stationSfc.Append(currentPnts, stationRecord, null) > 0)
                    {
                        appendCount++;
                    }
                }

                stationSfc.EndBatch();
                Console.WriteLine($"MT测点追加完成: {appendCount}/{uniqueStations.Count}");
                return appendCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"追加MT测点数据异常: {ex}");
                return false;
            }
            finally
            {
                if (stationSfc != null)
                {
                    try
                    {
                        if (stationSfc.HasOpen()) stationSfc.Close();
                        Marshal.ReleaseComObject(stationSfc);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 追加测深数据到已有的 ObjectCls
        /// MT测深字段: 测线编号, 测点编号, 测点x坐标, 测点y坐标, 周期, 视电阻率_TE, 相位_TE, 视电阻率_TM, 相位_TM
        /// </summary>
        private bool AppendSoundingData(List<MTDataRow> allDataRows)
        {
            ObjectCls soundingOcls = null;
            try
            {
                soundingOcls = new ObjectCls();
                if (!soundingOcls.Open(_selectedSoundingUrl))
                {
                    MessageBox.Show($"无法打开测深数据表: {_selectedSoundingUrl}", "错误");
                    return false;
                }

                Fields existingFields = soundingOcls.Fields;
                if (existingFields == null)
                {
                    MessageBox.Show("无法获取测深数据表的字段结构。", "错误");
                    return false;
                }

                Console.WriteLine("开始追加MT测深数据...");
                soundingOcls.BeginBatch(BatchType.Append);

                Record soundingRecord = new Record { Fields = existingFields };
                int appendCount = 0;

                foreach (var rowData in allDataRows)
                {
                    // MT 测深数据字段名（与Form_MTAddData一致）
                    soundingRecord["测线编号"] = rowData.LineName;
                    soundingRecord["测点编号"] = rowData.StationName;
                    soundingRecord["测点x坐标"] = rowData.X;
                    soundingRecord["测点y坐标"] = rowData.Y;
                    soundingRecord["周期"] = rowData.Period;
                    soundingRecord["视电阻率_TE"] = rowData.Res_TE;
                    soundingRecord["相位_TE"] = rowData.Phase_TE;
                    soundingRecord["视电阻率_TM"] = rowData.Res_TM;
                    soundingRecord["相位_TM"] = rowData.Phase_TM;

                    if (soundingOcls.Append(null, soundingRecord, null) > 0)
                    {
                        appendCount++;
                    }
                }

                soundingOcls.EndBatch();
                Console.WriteLine($"MT测深数据追加完成: {appendCount}/{allDataRows.Count}");
                return appendCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"追加MT测深数据异常: {ex}");
                return false;
            }
            finally
            {
                if (soundingOcls != null)
                {
                    try
                    {
                        if (soundingOcls.HasOpen()) soundingOcls.Close();
                        Marshal.ReleaseComObject(soundingOcls);
                    }
                    catch { }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region --- 辅助函数 ---

        private bool IsHeaderLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            string firstWord = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (firstWord == null) return false;
            return firstWord.Any(c => !char.IsDigit(c) && c != '.' && c != '-' && c != 'e' && c != 'E');
        }

        private void ResetFormState()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ResetFormState));
            }
            else
            {
                this.Cursor = Cursors.Default;
                btnOK.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        #endregion

        #region --- 辅助类 ---

        private class MTDataRow
        {
            public string LineName { get; set; }
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Period { get; set; }
            public double Res_TE { get; set; }
            public double Phase_TE { get; set; }
            public double Res_TM { get; set; }
            public double Phase_TM { get; set; }
        }

        private class StationInfo
        {
            public string LineName { get; set; }
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        #endregion

        #region --- 窗口拖动逻辑 ---

        private void InitDragEvent()
        {
            panel1.MouseDown += TitlePanel_MouseDown;
            panel1.MouseMove += TitlePanel_MouseMove;
        }

        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousePoint.X = e.X;
                mousePoint.Y = e.Y;
            }
        }

        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left = Control.MousePosition.X - mousePoint.X;
                this.Top = Control.MousePosition.Y - mousePoint.Y;
            }
        }

        #endregion
    }
}