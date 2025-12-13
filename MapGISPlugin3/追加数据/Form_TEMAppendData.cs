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
    /// TEM 追加观测数据窗体
    /// 功能：向已有的测点要素类和观测数据表追加新的观测数据
    /// </summary>
    public partial class Form_TEMAppendData : Form
    {
        private Point mousePoint = new Point();
        private IApplication _hook;

        private string _selectedStationUrl = "";
        private string _selectedObservationUrl = "";
        // 在类的成员变量区域添加：
        private enum SelectionTarget { None, Station, Observation }
        private SelectionTarget _currentSelectionTarget = SelectionTarget.None;
        public Form_TEMAppendData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            InitDragEvent();

            // 添加文本框点击事件
            txtSelectedStation.Click += txtSelectedStation_Click;
            txtSelectedObservation.Click += txtSelectedObservation_Click;
        }

        // 添加两个新的事件处理方法：
        private void txtSelectedStation_Click(object sender, EventArgs e)
        {
            _currentSelectionTarget = SelectionTarget.Station;
            txtSelectedStation.BackColor = System.Drawing.Color.LightYellow;
            txtSelectedObservation.BackColor = System.Drawing.SystemColors.Control;
        }

        private void txtSelectedObservation_Click(object sender, EventArgs e)
        {
            _currentSelectionTarget = SelectionTarget.Observation;
            txtSelectedObservation.BackColor = System.Drawing.Color.LightYellow;
            txtSelectedStation.BackColor = System.Drawing.SystemColors.Control;
        }

        private void Form_TEMAppendData_Load(object sender, EventArgs e)
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
                    TreeNode layerNode = new TreeNode(layer.Name);
                    layerNode.Tag = layer;
                    layerNode.ForeColor = System.Drawing.Color.Blue;
                    parentNode.Nodes.Add(layerNode);
                }
                else if (layer is ObjectLayer)
                {
                    TreeNode layerNode = new TreeNode(layer.Name);
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
                openFileDialog.Title = "请选择 TEM 观测数据文件 (. dat)";
                openFileDialog.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = openFileDialog.FileName;
                }
            }
        }

        // 替换原有的 treeViewLayers_AfterSelect 方法：
        private void treeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            if (_currentSelectionTarget == SelectionTarget.Station)
            {
                if (e.Node.Tag is VectorLayer)
                {
                    VectorLayer vl = e.Node.Tag as VectorLayer;
                    txtSelectedStation.Text = vl.Name;
                    _selectedStationUrl = vl.URL;
                }
                else if (e.Node.Tag is ObjectLayer)
                {
                    MessageBox.Show("请选择矢量图层（蓝色）作为测点图层。", "提示");
                }
            }
            else if (_currentSelectionTarget == SelectionTarget.Observation)
            {
                if (e.Node.Tag is ObjectLayer)
                {
                    ObjectLayer ol = e.Node.Tag as ObjectLayer;
                    txtSelectedObservation.Text = ol.Name;
                    _selectedObservationUrl = ol.URL;
                }
                else if (e.Node.Tag is VectorLayer)
                {
                    MessageBox.Show("请选择对象图层（绿色）作为观测数据。", "提示");
                }
            }
            else
            {
                // 默认行为
                if (e.Node.Tag is VectorLayer)
                {
                    VectorLayer vl = e.Node.Tag as VectorLayer;
                    txtSelectedStation.Text = vl.Name;
                    _selectedStationUrl = vl.URL;
                }
                else if (e.Node.Tag is ObjectLayer)
                {
                    ObjectLayer ol = e.Node.Tag as ObjectLayer;
                    txtSelectedObservation.Text = ol.Name;
                    _selectedObservationUrl = ol.URL;
                }
            }
        }

        private void FindCorrespondingObservationTable(TreeNode parentNode, string stationName)
        {
            if (parentNode == null) return;
            string baseName = stationName.Replace("_测点", "").Split('_')[0];

            foreach (TreeNode node in parentNode.Nodes)
            {
                if (node.Tag is ObjectLayer)
                {
                    ObjectLayer ol = node.Tag as ObjectLayer;
                    if (ol.Name.Contains(baseName) && ol.Name.Contains("观测"))
                    {
                        txtSelectedObservation.Text = ol.Name;
                        _selectedObservationUrl = ol.URL;
                        break;
                    }
                }
            }
        }

        private void FindCorrespondingStationLayer(TreeNode parentNode, string observationName)
        {
            if (parentNode == null) return;
            string baseName = observationName.Replace("_观测数据", "").Split('_')[0];

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

                if (string.IsNullOrWhiteSpace(_selectedObservationUrl))
                {
                    MessageBox.Show("请在目录树中选择要追加数据的观测数据表。", "错误");
                    ResetFormState();
                    return;
                }

                // 读取TEM观测数据文件
                // TEM数据格式: 测线号 测点号 X Y 采样时间_us 有效面积 感应电压_mV
                var allDataRows = new List<TEMDataRow>();
                var uniqueStations = new Dictionary<string, StationInfo>();

                Console.WriteLine("=== 开始读取TEM观测数据文件 ===");
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
                        if (parts.Length < 7) continue;

                        try
                        {
                            TEMDataRow rowData = new TEMDataRow
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                SamplingTime_us = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                EffectiveArea = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                                InducedVoltage_mV = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture)
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
                bool observationAppendSuccess = AppendObservationData(allDataRows);

                if (stationAppendSuccess && observationAppendSuccess)
                {
                    MessageBox.Show($"追加成功！\n新增测点: {uniqueStations.Count} 个\n新增观测记录: {allDataRows.Count} 条", "完成");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    string errorMsg = "";
                    if (!stationAppendSuccess) errorMsg += "测点数据追加失败。\n";
                    if (!observationAppendSuccess) errorMsg += "观测数据追加失败。";
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
        /// TEM测点字段: 测线号, 测点号, X坐标, Y坐标
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

                Console.WriteLine("开始追加TEM测点数据...");
                stationSfc.BeginBatch(BatchType.Append);

                Record stationRecord = new Record { Fields = existingFields };
                int appendCount = 0;

                foreach (var stationInfo in uniqueStations.Values)
                {
                    Dot3D pnt3D = new Dot3D(stationInfo.X, stationInfo.Y, 0.0);
                    GeoPoints currentPnts = new GeoPoints();
                    currentPnts.Append(pnt3D);

                    // TEM 测点字段名（与Form_TEMImport一致）
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
                Console.WriteLine($"TEM测点追加完成: {appendCount}/{uniqueStations.Count}");
                return appendCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"追加TEM测点数据异常: {ex}");
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
        /// 追加观测数据到已有的 ObjectCls
        /// TEM观测数据字段: 测线号, 测点号, X, Y, 采样时间_us, 有效面积, 感应电压_mV
        /// </summary>
        private bool AppendObservationData(List<TEMDataRow> allDataRows)
        {
            ObjectCls observationOcls = null;
            try
            {
                observationOcls = new ObjectCls();
                if (!observationOcls.Open(_selectedObservationUrl))
                {
                    MessageBox.Show($"无法打开观测数据表: {_selectedObservationUrl}", "错误");
                    return false;
                }

                Fields existingFields = observationOcls.Fields;
                if (existingFields == null)
                {
                    MessageBox.Show("无法获取观测数据表的字段结构。", "错误");
                    return false;
                }

                Console.WriteLine("开始追加TEM观测数据...");
                observationOcls.BeginBatch(BatchType.Append);

                Record observationRecord = new Record { Fields = existingFields };
                int appendCount = 0;

                foreach (var rowData in allDataRows)
                {
                    // TEM 观测数据字段名（与Form_TEMImport一致）
                    observationRecord["测线号"] = rowData.LineName;
                    observationRecord["测点号"] = rowData.StationName;
                    observationRecord["X坐标"] = rowData.X;
                    observationRecord["Y坐标"] = rowData.Y;
                    observationRecord["采样时间_us"] = rowData.SamplingTime_us;
                    observationRecord["有效面积"] = rowData.EffectiveArea;
                    observationRecord["感应电压_mV"] = rowData.InducedVoltage_mV;

                    if (observationOcls.Append(null, observationRecord, null) > 0)
                    {
                        appendCount++;
                    }
                }

                observationOcls.EndBatch();
                Console.WriteLine($"TEM观测数据追加完成: {appendCount}/{allDataRows.Count}");
                return appendCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"追加TEM观测数据异常: {ex}");
                return false;
            }
            finally
            {
                if (observationOcls != null)
                {
                    try
                    {
                        if (observationOcls.HasOpen()) observationOcls.Close();
                        Marshal.ReleaseComObject(observationOcls);
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

        private class TEMDataRow
        {
            public string LineName { get; set; }
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double SamplingTime_us { get; set; }
            public double EffectiveArea { get; set; }
            public double InducedVoltage_mV { get; set; }
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