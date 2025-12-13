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
    /// CSAMT 追加观测数据窗体
    /// 功能：向已有的测点要素类和测深数据表追加新的观测数据
    /// </summary>
    public partial class Form_CSAMTAppendData : Form
    {
        private Point mousePoint = new Point();
        private IApplication _hook;

        // 存储用户选择的目标类
        private SFeatureCls _targetStationCls = null;
        private ObjectCls _targetSoundingCls = null;
        private string _selectedStationUrl = "";
        private string _selectedSoundingUrl = "";
        // 在类的成员变量区域（约第25行附近）添加：
        private enum SelectionTarget { None, Station, Sounding }
        private SelectionTarget _currentSelectionTarget = SelectionTarget.None;
        public Form_CSAMTAppendData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            InitDragEvent();

            // 添加文本框点击事件
            txtSelectedStation.Click += txtSelectedStation_Click;
            txtSelectedSounding.Click += txtSelectedSounding_Click;
        }

        private void txtSelectedStation_Click(object sender, EventArgs e)
        {
            _currentSelectionTarget = SelectionTarget.Station;
            // 高亮显示当前激活的文本框
            txtSelectedStation.BackColor = System.Drawing.Color.LightYellow;
            txtSelectedSounding.BackColor = System.Drawing.SystemColors.Control;
        }

        private void txtSelectedSounding_Click(object sender, EventArgs e)
        {
            _currentSelectionTarget = SelectionTarget.Sounding;
            // 高亮显示当前激活的文本框
            txtSelectedSounding.BackColor = System.Drawing.Color.LightYellow;
            txtSelectedStation.BackColor = System.Drawing.SystemColors.Control;
        }

        private void Form_CSAMTAppendData_Load(object sender, EventArgs e)
        {
            PopulateTreeView();
        }

        #region --- 目录树填充逻辑 ---

        /// <summary>
        /// 填充目录树，显示所有地图中的图层
        /// </summary>
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
                    mapNode.ImageIndex = 0;
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
                Console.WriteLine($"[PopulateTreeView] 错误: {ex}");
            }
        }

        private void AddLayersToNode(TreeNode parentNode, Map map)
        {
            if (map == null || parentNode == null) return;

            for (int i = 0; i < map.LayerCount; i++)
            {
                MapLayer layer = map.get_Layer(i);
                if (layer != null)
                {
                    ProcessLayer(parentNode, layer);
                }
            }
        }

        private void AddLayersToNode(TreeNode parentNode, GroupLayer groupLayer)
        {
            if (groupLayer == null || parentNode == null) return;

            for (int i = 0; i < groupLayer.Count; i++)
            {
                MapLayer layer = groupLayer.get_Item(i);
                if (layer != null)
                {
                    ProcessLayer(parentNode, layer);
                }
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
                    // 显示矢量图层（测点）
                    TreeNode layerNode = new TreeNode(layer.Name);
                    layerNode.Tag = layer;
                    layerNode.ForeColor = System.Drawing.Color.Blue;
                    parentNode.Nodes.Add(layerNode);
                }
                else if (layer is ObjectLayer)
                {
                    // 显示对象图层（测深数据表）
                    TreeNode layerNode = new TreeNode(layer.Name);
                    layerNode.Tag = layer;
                    layerNode.ForeColor = System.Drawing.Color.Green;
                    parentNode.Nodes.Add(layerNode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessLayer] 处理图层 '{layer?.Name}' 时出错: {ex.Message}");
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

        /// <summary>
        /// 选择观测数据文件
        /// </summary>
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择 CSAMT 观测数据文件 (. dat)";
                openFileDialog.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// 目录树节点选择变更
        /// </summary>
        private void treeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            if (_currentSelectionTarget == SelectionTarget.Station)
            {
                // 用户想要选择测点图层
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
            else if (_currentSelectionTarget == SelectionTarget.Sounding)
            {
                // 用户想要选择测深数据
                if (e.Node.Tag is ObjectLayer)
                {
                    ObjectLayer ol = e.Node.Tag as ObjectLayer;
                    txtSelectedSounding.Text = ol.Name;
                    _selectedSoundingUrl = ol.URL;
                }
                else if (e.Node.Tag is VectorLayer)
                {
                    MessageBox.Show("请选择对象图层（绿色）作为测深数据。", "提示");
                }
            }
            else
            {
                // 没有激活任何文本框时的默认行为
                if (e.Node.Tag is VectorLayer)
                {
                    VectorLayer vl = e.Node.Tag as VectorLayer;
                    txtSelectedStation.Text = vl.Name;
                    _selectedStationUrl = vl.URL;
                }
                else if (e.Node.Tag is ObjectLayer)
                {
                    ObjectLayer ol = e.Node.Tag as ObjectLayer;
                    txtSelectedSounding.Text = ol.Name;
                    _selectedSoundingUrl = ol.URL;
                }
            }
        }



        /// <summary>
        /// 确定按钮 - 执行追加操作
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnOK.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                // 1. 验证输入
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

                // 2.  读取观测数据文件
                var allDataRows = new List<CSAMTDataRow>();
                var uniqueStations = new Dictionary<string, StationInfo>();

                Console.WriteLine("=== 开始读取观测数据文件 ===");
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
                            CSAMTDataRow rowData = new CSAMTDataRow
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                Freq = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                Res = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                                Pha = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture)
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

                // 3. 追加测点数据到 SFeatureCls
                bool stationAppendSuccess = AppendStationData(uniqueStations);

                // 4. 追加测深数据到 ObjectCls
                bool soundingAppendSuccess = AppendSoundingData(allDataRows);

                // 5. 显示结果
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
        /// CSAMT测点字段: 测线号, 测点号, X坐标, Y坐标
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

                // 获取已有字段结构
                Fields existingFields = stationSfc.Fields;
                if (existingFields == null)
                {
                    MessageBox.Show("无法获取测点要素类的字段结构。", "错误");
                    return false;
                }

                Console.WriteLine("开始追加测点数据...");
                stationSfc.BeginBatch(BatchType.Append);

                Record stationRecord = new Record { Fields = existingFields };
                int appendCount = 0;

                foreach (var stationInfo in uniqueStations.Values)
                {
                    Dot3D pnt3D = new Dot3D(stationInfo.X, stationInfo.Y, 0.0);
                    GeoPoints currentPnts = new GeoPoints();
                    currentPnts.Append(pnt3D);

                    // CSAMT 测点字段名
                    stationRecord["测线号"] = stationInfo.LineName;
                    stationRecord["测点号"] = stationInfo.StationName;
                    stationRecord["X坐标"] = stationInfo.X;
                    stationRecord["Y坐标"] = stationInfo.Y;

                    if (stationSfc.Append(currentPnts, stationRecord, null) > 0)
                    {
                        appendCount++;
                    }
                    else
                    {
                        Console.WriteLine($"警告: 追加测点 '{stationInfo.StationName}' 失败。");
                    }
                }

                stationSfc.EndBatch();
                Console.WriteLine($"测点追加完成: {appendCount}/{uniqueStations.Count}");
                return appendCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"追加测点数据异常: {ex}");
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
        /// CSAMT测深字段: 测线编号, 测点编号, 测点x坐标, 测点y坐标, 频率, 视电阻率, 相位
        /// </summary>
        private bool AppendSoundingData(List<CSAMTDataRow> allDataRows)
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

                // 获取已有字段结构
                Fields existingFields = soundingOcls.Fields;
                if (existingFields == null)
                {
                    MessageBox.Show("无法获取测深数据表的字段结构。", "错误");
                    return false;
                }

                Console.WriteLine("开始追加测深数据...");
                soundingOcls.BeginBatch(BatchType.Append);

                Record soundingRecord = new Record { Fields = existingFields };
                int appendCount = 0;

                foreach (var rowData in allDataRows)
                {
                    // CSAMT 测深数据字段名
                    soundingRecord["测线编号"] = rowData.LineName;
                    soundingRecord["测点编号"] = rowData.StationName;
                    soundingRecord["测点x坐标"] = rowData.X;
                    soundingRecord["测点y坐标"] = rowData.Y;
                    soundingRecord["频率"] = rowData.Freq;
                    soundingRecord["视电阻率"] = rowData.Res;
                    soundingRecord["相位"] = rowData.Pha;

                    if (soundingOcls.Append(null, soundingRecord, null) > 0)
                    {
                        appendCount++;
                    }
                    else
                    {
                        Console.WriteLine($"警告: 追加测深记录失败 (测点: {rowData.StationName})。");
                    }
                }

                soundingOcls.EndBatch();
                Console.WriteLine($"测深数据追加完成: {appendCount}/{allDataRows.Count}");
                return appendCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"追加测深数据异常: {ex}");
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

        private class CSAMTDataRow
        {
            public string LineName { get; set; }
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Freq { get; set; }
            public double Res { get; set; }
            public double Pha { get; set; }
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