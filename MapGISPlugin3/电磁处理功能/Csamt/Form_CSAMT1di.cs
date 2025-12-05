using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;
using MapGIS.GeoObjects.Geometry;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Numerics;

namespace MapGISPlugin3
{
    public partial class Form_CSAMT1di : Form
    {
        private IApplication _hook;
        private List<MapLayer> m_allPointLayers;
        private List<MapLayer> m_allObjectLayers;
        private List<MapLayer> m_allTransmitterLayers;

        private SFeatureCls m_SelectedStationLayer;
        private ObjectCls m_SelectedSoundingTable;
        private SFeatureCls m_SelectedTransmitterLayer;

        private List<StationInfo> m_CurrentLineStations;
        private DataTable m_CurrentLineData;
        private string m_CurrentSelectedStationName;
        private Dictionary<string, (double Offset, double Distance)> m_StationDistances;

        private Point mousePoint = new Point();

        private double Ax = 0, Ay = 0, Az = 0;
        private double Bx = 0, By = 0, Bz = 0;

        public Form_CSAMT1di(IApplication hook)
        {
            InitializeComponent();
            InitDragEvent();

            _hook = hook;

            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_allTransmitterLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();
            m_StationDistances = new Dictionary<string, (double, double)>();

            // 不在计算页的左侧小地图上进行点选
            chartProfileView.MouseClick -= chartProfileView_MouseClick;
        }

        #region 图层加载

        private void LoadLayersFromMap()
        {
            m_allPointLayers.Clear();
            m_allObjectLayers.Clear();
            m_allTransmitterLayers.Clear();

            cmbStationLayer.Items.Clear();

            Map electroMap = null;
            try
            {
                electroMap = FindMapByName("电法数据");
                if (electroMap == null)
                {
                    MessageBox.Show("错误: 未在项目中找到名为 '电法数据' 的地图！", "LoadLayersFromMap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmbStationLayer.Enabled = false;
                    cmbLineName.Enabled = false;
                    return;
                }

                int layerCount = electroMap.LayerCount;
                MapLayer layer = null;
                for (int i = 0; i < layerCount; i++)
                {
                    layer = electroMap.get_Layer(i);
                    if (layer == null) continue;
                    ProcessLayerForComboBox_Debug(layer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图层列表意外错误: {ex.Message}", "LoadLayersFromMap - 致命错误");
                return;
            }

            cmbStationLayer.DisplayMember = "Name";

            if (cmbStationLayer.Items.Count > 0)
            {
                cmbStationLayer.Text = "请选择测点图层...";
                cmbStationLayer.Enabled = true;
                cmbLineName.Enabled = true;
            }
            else
            {
                MessageBox.Show("未能加载任何符合条件的测点图层到下拉框。", "LoadLayersFromMap - 结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbStationLayer.Enabled = false;
                cmbLineName.Enabled = false;
            }
        }

        private void ProcessLayerForComboBox_Debug(MapLayer layer)
        {
            if (layer == null) return;

            string layerName = "[无法获取名称]";
            try { layerName = layer.Name ?? "[名称为null]"; } catch { }

            try
            {
                if (layer is GroupLayer groupLayer)
                {
                    int subLayerCount = groupLayer.Count;
                    for (int i = 0; i < subLayerCount; i++)
                    {
                        MapLayer subLayer = groupLayer.get_Item(i);
                        ProcessLayerForComboBox_Debug(subLayer);
                    }
                }
                else if (layer is VectorLayer vectorLayer)
                {
                    GeomType geomType = vectorLayer.GeometryType;
                    bool isPoint = (geomType == GeomType.Pnt);
                    if (isPoint && layerName != null)
                    {
                        if (layerName.Contains("测点"))
                        {
                            m_allPointLayers.Add(layer);
                            cmbStationLayer.Items.Add(layer);
                        }
                        else if (layerName.Contains("发射源"))
                        {
                            m_allTransmitterLayers.Add(layer);
                        }
                    }
                }
                else if (layer is ObjectLayer)
                {
                    if (layerName != null && layerName.Contains("测深数据"))
                    {
                        m_allObjectLayers.Add(layer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理图层 '{layerName}' 时发生错误: {ex.Message}");
            }
        }

        #endregion

        #region 顶部下拉框事件

        private void cmbStationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
            m_SelectedStationLayer = null;

            if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
            m_SelectedSoundingTable = null;

            if (m_SelectedTransmitterLayer != null) try { Marshal.ReleaseComObject(m_SelectedTransmitterLayer); } catch { }
            m_SelectedTransmitterLayer = null;

            txtAx.Text = "0"; txtAy.Text = "0"; txtAz.Text = "0";
            txtBx.Text = "0"; txtBy.Text = "0"; txtBz.Text = "0";
            txtCurrentSelectedStation.Text = "";
            txtCurrentInversionStation.Text = "";
            txtTxRxDistance.Text = "";
            txtOffsetFromCenter.Text = "";

            cmbLineName.Items.Clear();
            ClearAllDisplays();

            if (cmbStationLayer.SelectedItem == null || !(cmbStationLayer.SelectedItem is MapLayer selectedLayer))
            {
                cmbLineName.Enabled = false;
                return;
            }

            try
            {
                m_SelectedStationLayer = selectedLayer.GetData() as SFeatureCls;
                if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
                {
                    MessageBox.Show($"无法从图层 '{selectedLayer.Name}' 获取有效要素类数据！", "错误");
                    cmbLineName.Enabled = false;
                    m_SelectedStationLayer = null;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取图层 '{selectedLayer.Name}' 数据时发生错误: {ex.Message}", "错误");
                cmbLineName.Enabled = false;
                m_SelectedStationLayer = null;
                return;
            }

            string stationLayerName = selectedLayer.Name;
            string expectedTableName = stationLayerName.Replace("测点", "测深数据");

            MapLayer soundingLayer = m_allObjectLayers.FirstOrDefault(layer => layer != null && layer.Name == expectedTableName);
            if (soundingLayer == null)
            {
                MessageBox.Show($"未在 '电法数据' 地图中找到与 '{stationLayerName}' 匹配的测深数据表 '{expectedTableName}'！", "关联失败");
                cmbLineName.Enabled = false;
                return;
            }

            try
            {
                m_SelectedSoundingTable = soundingLayer.GetData() as ObjectCls;
                if (m_SelectedSoundingTable == null || !m_SelectedSoundingTable.HasOpen())
                {
                    MessageBox.Show($"图层 '{expectedTableName}' 不是有效的对象类或无法打开！", "错误");
                    m_SelectedSoundingTable = null;
                    cmbLineName.Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取测深表数据时发生错误: {ex.Message}", "错误");
                m_SelectedSoundingTable = null;
                cmbLineName.Enabled = false;
                return;
            }

            string expectedTransmitterName = stationLayerName.Replace("测点", "发射源");
            MapLayer tranLayer = m_allTransmitterLayers.FirstOrDefault(layer => layer != null && layer.Name == expectedTransmitterName);
            if (tranLayer != null)
            {
                try
                {
                    m_SelectedTransmitterLayer = tranLayer.GetData() as SFeatureCls;
                    if (m_SelectedTransmitterLayer != null && m_SelectedTransmitterLayer.HasOpen())
                    {
                        LoadTransmitterCoordsFromLayer();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"自动加载发射源数据失败: {ex.Message}");
                }
            }

            cmbLineName.Enabled = true;
            FillLineComboBox();

            if (cmbLineName.Items.Count > 0)
            {
                cmbLineName.Text = "请选择测线...";
            }
            else
            {
                MessageBox.Show($"在测点图层 '{stationLayerName}' 中未能查询到任何测线号。");
                cmbLineName.Enabled = false;
                ClearAllDisplays();
            }
        }

        private void LoadTransmitterCoordsFromLayer()
        {
            if (m_SelectedTransmitterLayer == null) return;

            RecordSet rs = null;
            try
            {
                rs = m_SelectedTransmitterLayer.Select(null);
                if (rs == null || rs.Count == 0) return;

                rs.MoveFirst();

                int idxName = rs.Fields.IndexOf("点名");
                int idxX = rs.Fields.IndexOf("X");
                int idxY = rs.Fields.IndexOf("Y");
                int idxZ = rs.Fields.IndexOf("Z");
                bool useFields = (idxX >= 0 && idxY >= 0);

                long recordCount = rs.Count;

                for (int i = 0; i < recordCount; i++)
                {
                    Record att = null;
                    IGeometry geom = null;
                    Dot3D dotToRelease = null;

                    try
                    {
                        att = rs.Att;
                        if (att != null)
                        {
                            string pName = "";
                            double pX = 0, pY = 0, pZ = 0;

                            if (idxName >= 0)
                            {
                                object valName = att.GetValue(idxName);
                                if (valName != null) pName = valName.ToString().Trim().ToUpper();
                            }

                            if (useFields)
                            {
                                object oX = att.GetValue(idxX);
                                object oY = att.GetValue(idxY);
                                object oZ = (idxZ >= 0) ? att.GetValue(idxZ) : 0;
                                if (oX != null) double.TryParse(oX.ToString(), out pX);
                                if (oY != null) double.TryParse(oY.ToString(), out pY);
                                if (oZ != null) double.TryParse(oZ.ToString(), out pZ);
                            }
                            else
                            {
                                geom = rs.Geometry;
                                if (geom is Dot3D dot)
                                {
                                    pX = dot.X; pY = dot.Y; pZ = dot.Z;
                                }
                                else if (geom is GeoPoints pnts && pnts.Count > 0)
                                {
                                    dotToRelease = pnts.GetItem(0);
                                    if (dotToRelease != null)
                                    {
                                        pX = dotToRelease.X;
                                        pY = dotToRelease.Y;
                                        pZ = dotToRelease.Z;
                                    }
                                }
                            }

                            bool isA = (pName.Contains("A") || (string.IsNullOrEmpty(pName) && i == 0));
                            bool isB = (pName.Contains("B") || (string.IsNullOrEmpty(pName) && i == 1));

                            if (isA)
                            {
                                txtAx.Text = pX.ToString();
                                txtAy.Text = pY.ToString();
                                txtAz.Text = pZ.ToString();
                            }
                            else if (isB)
                            {
                                txtBx.Text = pX.ToString();
                                txtBy.Text = pY.ToString();
                                txtBz.Text = pZ.ToString();
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (dotToRelease != null && Marshal.IsComObject(dotToRelease))
                            Marshal.ReleaseComObject(dotToRelease);
                        if (geom != null && Marshal.IsComObject(geom))
                            Marshal.ReleaseComObject(geom);
                        if (att != null && Marshal.IsComObject(att))
                            Marshal.ReleaseComObject(att);
                    }

                    rs.MoveNext();
                }

                this.BeginInvoke(new Action(() =>
                {
                    UpdateDistances();
                    UpdateLayoutView();
                    UpdateSelectedStationInfo(m_CurrentSelectedStationName);
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadTransmitterCoordsFromLayer 致命错误: {ex.Message}");
            }
            finally
            {
                if (rs != null && Marshal.IsComObject(rs))
                    try { Marshal.ReleaseComObject(rs); } catch { }
            }
        }

        private void cmbLineName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllDisplays();

            if (cmbLineName.SelectedItem == null || m_SelectedStationLayer == null || m_SelectedSoundingTable == null)
            {
                return;
            }

            string selectedLine = cmbLineName.SelectedItem.ToString();
            this.Cursor = Cursors.WaitCursor;

            try
            {
                m_CurrentLineStations = QueryStationsForLine(selectedLine);
                m_CurrentLineData = QuerySoundingDataForLine(selectedLine);

                UpdateProfileView();
                UpdateLayoutView();
                UpdateDataGrids();               // 数据页全表
                UpdateCalcDataGridForCurrentStation(null); // 先清空计算页下方表

                if (m_CurrentLineStations.Count > 0)
                {
                    string firstStation = m_CurrentLineStations[0].StationName;
                    SelectStationAndRefreshCharts(firstStation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生错误: {ex.Message}", "错误");
                ClearAllDisplays();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region 计算按钮与 a.exe

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            bool allZero = true;
            double ax = 0, ay = 0, az = 0;
            double bx = 0, by = 0, bz = 0;
            try
            {
                ax = SafeParseDouble(txtAx.Text);
                ay = SafeParseDouble(txtAy.Text);
                az = SafeParseDouble(txtAz.Text);
                bx = SafeParseDouble(txtBx.Text);
                by = SafeParseDouble(txtBy.Text);
                bz = SafeParseDouble(txtBz.Text);
                allZero = (ax == 0 && ay == 0 && az == 0 && bx == 0 && by == 0 && bz == 0);
            }
            catch
            {
                allZero = true;
            }

            if (allZero)
            {
                var result = MessageBox.Show(
                    "发射源坐标未设置或全部为0，请在“布置图”页面设置发射源坐标后再进行计算。\n\n是否立即跳转到布置图页面？",
                    "发射源坐标未设置",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                    tabControl1.SelectedTab = tabPageLayout;

                return;
            }

            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("没有加载任何测线数据，无法计算。", "错误");
                return;
            }

            int its = (int)nudIterationCount.Value;
            int layerCount = (int)nudLayerCount.Value;
            double initialThickness = (double)nudInitialThickness.Value;
            double growthRate = (double)nudGrowthRate.Value;
            double initialResistivity = (double)nudInitialResistivity.Value;
            double allowError = SafeParseDouble(txtAllowError.Text);
            if (allowError <= 0) // 确保 eps 有效
            {
                MessageBox.Show("允许误差容忍度 (eps) 必须大于 0。", "输入错误");
                return;
            }
            this.Cursor = Cursors.WaitCursor;

            string tempRunDir = Path.Combine(Path.GetTempPath(), "CSAMT1di_run_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            try
            {
                Directory.CreateDirectory(tempRunDir);

                string exePath;
                try
                {
                    string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string algorithmDir = Path.Combine(pluginDir, "Algorithm", "CSAMT1di");
                    exePath = Path.Combine(algorithmDir, "a.exe");
                    if (!File.Exists(exePath))
                        throw new FileNotFoundException($"计算程序 'a.exe' 未找到。\n期望路径: {exePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"查找 'a.exe' 失败: {ex.Message}", "错误");
                    this.Cursor = Cursors.Default;
                    return;
                }

                string tempDataFile = Path.Combine(tempRunDir, "knowed.dat");
                string tempTranFile = Path.Combine(tempRunDir, "tran.dat");
                string workspaceName = "result";
                string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);

                Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);

                using (StreamWriter writer = new StreamWriter(tempDataFile, false, Encoding.UTF8))
                {
                    foreach (DataRow row in m_CurrentLineData.Rows)
                    {
                        string lineName = row["测线编号"].ToString();
                        string stationName = row["测点编号"].ToString();

                        if (!stationCoords.TryGetValue(stationName, out StationInfo station))
                            continue;

                        double freq = GetDoubleFromRow(row, "频率", 0.0);
                        double res = GetDoubleFromRow(row, "视电阻率", 0.0);
                        double pha = GetDoubleFromRow(row, "相位", 0.0);

                        string lineContent = $"{lineName} {stationName} {station.X} {station.Y} {freq} {res} {pha}";
                        writer.WriteLine(lineContent);
                    }
                }

                using (StreamWriter writer = new StreamWriter(tempTranFile, false, Encoding.UTF8))
                {
                    writer.WriteLine($"{ax} {ay} {az}");
                    writer.WriteLine($"{bx} {by} {bz}");
                }

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = exePath;


                string arguments = $"\"{tempDataFile}\" \"{tempTranFile}\" \"{workspaceName}\" {its} {layerCount} {initialThickness} {growthRate} {initialResistivity} {allowError}";

                // 【关键调试代码】—— 让你亲眼看到传给 a.exe 的完整命令行
                MessageBox.Show(
                    "即将执行的命令行：\n\n" +
                    exePath + "\n\n参数：\n" + arguments + "\n\n" +
                    "工作目录：\n" + tempRunDir,
                    "调试：a.exe 命令行参数",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                startInfo.Arguments = $"\"{tempDataFile}\" \"{tempTranFile}\" \"{workspaceName}\" {its} {layerCount} {initialThickness} {growthRate} {initialResistivity} {allowError}";

                startInfo.WorkingDirectory = tempRunDir;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                startInfo.StandardErrorEncoding = Encoding.GetEncoding("GBK");

                string output = "";
                string error = "";

                using (Process process = Process.Start(startInfo))
                {
                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        string knowFile = Path.Combine(fullWorkspacePath, "KNOW");
                        if (File.Exists(knowFile))
                        {
                            List<InversionResultPoint> results = ParseKnowFile(knowFile);

                            DisplayInversionResults(results);       // 数据页剖面图
                            ApplyInversionResultToDataTable(results); // 所有行写入“计算后电阻率”

                            // 更新计算页下方当前测点的表
                            UpdateCalcDataGridForCurrentStation(m_CurrentSelectedStationName);
                        }
                        else
                        {
                            DisplayInversionResults(new List<InversionResultPoint>());
                            MessageBox.Show($"计算程序返回成功，但未生成 KNOW 结果文件。\n\n程序输出:\n{output}", "提示");
                        }

                        string recordFile = Path.Combine(fullWorkspacePath, "record");
                        if (File.Exists(recordFile))
                        {
                            try
                            {
                                var lines = File.ReadAllLines(recordFile, Encoding.Default);
                                if (lines.Length > 0)
                                {
                                    string last = lines[lines.Length - 1];
                                    var parts = last.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length >= 3)
                                    {
                                        txtActualError.Text = parts[2];
                                    }
                                }
                            }
                            catch { }
                        }

                        MessageBox.Show($"计算完成！\n结果保存在: {fullWorkspacePath}", "成功");
                        UpdateDataGrids(); // 数据页刷新
                    }
                    else
                    {
                        throw new Exception($"a.exe 运行失败 (ExitCode: {process.ExitCode})。\n\n错误信息:\n{error}\n\n标准输出:\n{output}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"计算过程中发生错误: \n{ex.Message}", "计算失败");
            }
            finally
            {
                if (Directory.Exists(tempRunDir))
                {
                    try { Directory.Delete(tempRunDir, true); } catch { }
                }
                this.Cursor = Cursors.Default;
            }
        }

        private void ApplyInversionResultToDataTable(List<InversionResultPoint> results)
        {
            if (m_CurrentLineData == null || results == null || results.Count == 0) return;

            const string colName = "计算后电阻率";

            if (!m_CurrentLineData.Columns.Contains(colName))
            {
                m_CurrentLineData.Columns.Add(colName, typeof(double));
            }

            var grouped = results
                .GroupBy(r => r.StationName)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var shallow = g.OrderBy(p => Math.Abs(p.Depth)).First();
                        return shallow.Resistivity;
                    });

            foreach (DataRow row in m_CurrentLineData.Rows)
            {
                string sta = row["测点编号"].ToString();
                if (grouped.TryGetValue(sta, out double invRes))
                {
                    row[colName] = invRes;
                }
            }
        }

        private double SafeParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            if (double.TryParse(text, out double result))
                return result;
            return 0;
        }

        #endregion

        #region 公共辅助函数（数据与图表）

        private double GetDoubleFromRow(DataRow row, string columnName, double defaultValue)
        {
            try
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    return Convert.ToDouble(row[columnName]);
                }
            }
            catch { }
            return defaultValue;
        }

        private Map FindMapByName(string mapName)
        {
            if (_hook == null || _hook.Document == null) return null;
            Maps maps = _hook.Document.GetMaps();
            for (int i = 0; i < maps.Count; i++)
            {
                Map currentMap = maps.GetMap(i);
                if (currentMap != null && currentMap.Name == mapName)
                {
                    return currentMap;
                }
            }
            return null;
        }

        private void FillLineComboBox()
        {
            cmbLineName.Items.Clear();

            if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
            {
                MessageBox.Show("FillLineComboBox 错误: m_SelectedStationLayer 为 null 或未打开。");
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            cmbLineName.Enabled = false;

            RecordSet rs = null;
            HashSet<string> uniqueLines = new HashSet<string>();

            try
            {
                string queryField = "测线号";
                Fields fields = m_SelectedStationLayer.Fields;
                if (fields == null || fields.IndexOf(queryField) < 0)
                {
                    MessageBox.Show($"字段 '{queryField}' 不存在或无法获取！");
                    return;
                }

                rs = m_SelectedStationLayer.Select(null);
                if (rs == null)
                {
                    QueryDef query = new QueryDef();
                    query.Filter = "";
                    query.WithSpatial = false;
                    rs = m_SelectedStationLayer.Select(query);
                    if (rs == null)
                    {
                        MessageBox.Show("Select(QueryDef) 也返回 null。");
                        return;
                    }
                }

                rs.MoveLast();
                rs.MoveFirst();

                int totalRecords = rs.Count;
                int currentIndex = 0;

                if (totalRecords == 0)
                {
                    return;
                }

                do
                {
                    Record currentAtt = null;
                    try
                    {
                        currentIndex++;
                        if (currentIndex > rs.Count * 2 && rs.Count > 0)
                        {
                            break;
                        }

                        currentAtt = rs.Att;
                        if (currentAtt != null)
                        {
                            object value = currentAtt[queryField];
                            if (value != null && value != DBNull.Value)
                            {
                                string lineName = value.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(lineName))
                                {
                                    uniqueLines.Add(lineName);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (currentAtt != null) { try { Marshal.ReleaseComObject(currentAtt); } catch { } }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);

                if (uniqueLines.Count > 0)
                {
                    var sortedLines = uniqueLines.OrderBy(s => s).ToArray();
                    cmbLineName.Items.AddRange(sortedLines);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询过程中发生错误: {ex.Message}", "错误");
            }
            finally
            {
                if (rs != null)
                {
                    try { Marshal.ReleaseComObject(rs); } catch { }
                }
                this.Cursor = Cursors.Default;
                cmbLineName.Enabled = (uniqueLines.Count > 0);
            }
        }

        private void ClearAllDisplays()
        {
            if (chartProfileView.Series != null) chartProfileView.Series.Clear();
            if (chartLayout.Series != null) chartLayout.Series.Clear();
            if (chartResistivity.Series != null) chartResistivity.Series.Clear();
            if (chartPhase.Series != null) chartPhase.Series.Clear();

            DisplayInversionResults(new List<InversionResultPoint>());

            gridData.DataSource = null;
            gridCalc.DataSource = null;
            gridLayout.DataSource = null;

            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_StationDistances.Clear();
            m_CurrentSelectedStationName = null;

            txtCurrentSelectedStation.Text = "";
            txtCurrentInversionStation.Text = "";
            txtTxRxDistance.Text = "";
            txtOffsetFromCenter.Text = "";
        }

        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;

            chartProfileView.Series.Clear();
            chartProfileView.BackColor = System.Drawing.Color.White;

            Series s = chartProfileView.Series.Add("Stations");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 6;
            s.MarkerColor = System.Drawing.Color.Blue;
            s.IsValueShownAsLabel = true;
            s.LegendText = "测点";

            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                return;
            }

            foreach (var station in m_CurrentLineStations)
            {
                int pointIndex = s.Points.AddXY(station.X, station.Y);
                s.Points[pointIndex].Label = station.StationName;
                s.Points[pointIndex].Tag = station.StationName;
                s.Points[pointIndex].Color = System.Drawing.Color.Blue;
            }

            chartProfileView.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            chartProfileView.ChartAreas[0].AxisY.LabelStyle.Format = "F0";
            chartProfileView.ChartAreas[0].AxisX.IsStartedFromZero = false;
            chartProfileView.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chartProfileView.ChartAreas[0].RecalculateAxesScale();
        }

        private void UpdateLayoutView()
        {
            if (chartLayout == null) return;

            chartLayout.Series.Clear();
            chartLayout.BackColor = System.Drawing.Color.White;
            chartLayout.Legends.Clear();

            Legend layoutLegend = chartLayout.Legends.Add("LegendLayout");
            layoutLegend.Docking = Docking.Top;
            layoutLegend.Alignment = StringAlignment.Far;
            layoutLegend.Font = new System.Drawing.Font("微软雅黑", 8f);

            Series sStations = chartLayout.Series.Add("Stations");
            sStations.ChartType = SeriesChartType.Point;
            sStations.MarkerStyle = MarkerStyle.Circle;
            sStations.MarkerSize = 6;
            sStations.Color = System.Drawing.Color.Blue;
            sStations.LegendText = "测点";

            if (m_CurrentLineStations != null && m_CurrentLineStations.Count > 0)
            {
                foreach (var station in m_CurrentLineStations)
                {
                    int idx = sStations.Points.AddXY(station.X, station.Y);
                    sStations.Points[idx].Tag = station.StationName;
                    sStations.Points[idx].ToolTip = $"测点: {station.StationName}\nX: {station.X}\nY: {station.Y}";
                }
            }

            double ax = SafeParseDouble(txtAx.Text);
            double ay = SafeParseDouble(txtAy.Text);
            double bx = SafeParseDouble(txtBx.Text);
            double by = SafeParseDouble(txtBy.Text);

            if (!(ax == 0 && ay == 0 && bx == 0 && by == 0))
            {
                Series sTransmitter = chartLayout.Series.Add("Transmitter");
                sTransmitter.ChartType = SeriesChartType.Line;
                sTransmitter.Color = System.Drawing.Color.Red;
                sTransmitter.BorderWidth = 2;
                sTransmitter.MarkerStyle = MarkerStyle.Star5;
                sTransmitter.MarkerSize = 12;
                sTransmitter.MarkerColor = System.Drawing.Color.Red;
                sTransmitter.LegendText = "发射源 (AB)";

                int idxA = sTransmitter.Points.AddXY(ax, ay);
                sTransmitter.Points[idxA].Label = "A";
                sTransmitter.Points[idxA].LabelForeColor = System.Drawing.Color.Red;
                sTransmitter.Points[idxA].Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold);

                int idxB = sTransmitter.Points.AddXY(bx, by);
                sTransmitter.Points[idxB].Label = "B";
                sTransmitter.Points[idxB].LabelForeColor = System.Drawing.Color.Red;
                sTransmitter.Points[idxB].Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold);
            }

            chartLayout.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            chartLayout.ChartAreas[0].AxisY.LabelStyle.Format = "F0";
            chartLayout.ChartAreas[0].AxisX.IsStartedFromZero = false;
            chartLayout.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chartLayout.ChartAreas[0].AxisX.Title = "X 坐标";
            chartLayout.ChartAreas[0].AxisY.Title = "Y 坐标";

            chartLayout.ChartAreas[0].RecalculateAxesScale();
        }

        private void UpdateDistances()
        {
            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                return;
            }

            if (!double.TryParse(txtAx.Text, out Ax) || !double.TryParse(txtAy.Text, out Ay) ||
                !double.TryParse(txtBx.Text, out Bx) || !double.TryParse(txtBy.Text, out By))
            {
                return;
            }

            m_StationDistances.Clear();

            Complex tranA = new Complex(Ax, Ay);
            Complex tranB = new Complex(Bx, By);
            Complex tranAB = tranB - tranA;
            double ab = Complex.Abs(tranAB);

            if (ab == 0) return;

            Complex unit = tranAB / ab;

            foreach (var station in m_CurrentLineStations)
            {
                Complex measured = new Complex(station.X, station.Y);
                Complex ap = tranA - measured;
                double dot = ap.Real * unit.Real + ap.Imaginary * unit.Imaginary;
                Complex apn = dot * unit;

                double offset = Complex.Abs(apn + tranAB / 2.0);
                double distance = Complex.Abs(ap - apn);

                m_StationDistances[station.StationName] = (offset, distance);
            }
        }

        private void SelectStationAndRefreshCharts(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName)) return;

            m_CurrentSelectedStationName = stationName;

            txtCurrentSelectedStation.Text = stationName;
            txtCurrentInversionStation.Text = stationName;

            // 每次选点都重新计算距离并更新信息
            UpdateDistances();
            UpdateSelectedStationInfo(stationName);

            HighlightLayoutStation(stationName);
            UpdateRightPanelCharts();
            UpdateCalcDataGridForCurrentStation(stationName);
        }

        private void UpdateCalcDataGridForCurrentStation(string stationName)
        {
            if (m_CurrentLineData == null || string.IsNullOrEmpty(stationName))
            {
                gridCalc.DataSource = null;
                return;
            }

            if (!m_CurrentLineData.Columns.Contains("计算后电阻率"))
            {
                // 尚未计算，不显示该列
                DataView dv0 = new DataView(m_CurrentLineData)
                {
                    RowFilter = $"测点编号 = '{stationName}'"
                };
                gridCalc.DataSource = dv0.ToTable(false, "频率", "视电阻率", "相位");
            }
            else
            {
                DataView dv = new DataView(m_CurrentLineData)
                {
                    RowFilter = $"测点编号 = '{stationName}'"
                };
                gridCalc.DataSource = dv.ToTable(false, "频率", "视电阻率", "相位", "计算后电阻率");
            }

            gridCalc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void UpdateRightPanelCharts()
        {
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();

            chartResistivity.BackColor = System.Drawing.Color.White;
            chartPhase.BackColor = System.Drawing.Color.White;

            chartResistivity.Legends.Clear();
            chartPhase.Legends.Clear();

            Legend resLegend = chartResistivity.Legends.Add("LegendResistivity");
            resLegend.Docking = Docking.Top;
            resLegend.Alignment = StringAlignment.Far;
            resLegend.BackColor = System.Drawing.Color.White;
            resLegend.BorderColor = System.Drawing.Color.LightGray;
            resLegend.BorderWidth = 1;
            resLegend.Font = new System.Drawing.Font("微软雅黑", 8f);
            resLegend.LegendStyle = LegendStyle.Table;
            resLegend.IsEquallySpacedItems = false;
            resLegend.TableStyle = LegendTableStyle.Auto;

            Legend phaseLegend = chartPhase.Legends.Add("LegendPhase");
            phaseLegend.Docking = Docking.Top;
            phaseLegend.Alignment = StringAlignment.Far;
            phaseLegend.BackColor = System.Drawing.Color.White;
            phaseLegend.BorderColor = System.Drawing.Color.LightGray;
            phaseLegend.BorderWidth = 1;
            phaseLegend.Font = new System.Drawing.Font("微软雅黑", 8f);
            phaseLegend.LegendStyle = LegendStyle.Table;
            phaseLegend.IsEquallySpacedItems = false;
            phaseLegend.TableStyle = LegendTableStyle.Auto;

            if (string.IsNullOrEmpty(m_CurrentSelectedStationName) || m_CurrentLineData == null)
            {
                return;
            }

            string resField = "视电阻率";
            string phaseField = "相位";

            DataView dvStation = new DataView(m_CurrentLineData);
            dvStation.RowFilter = $"测点编号 = '{m_CurrentSelectedStationName}'";

            if (dvStation.Count == 0)
            {
                return;
            }

            var resSeries = chartResistivity.Series.Add("视电阻率");
            var phaseSeries = chartPhase.Series.Add("相位");

            resSeries.ChartType = SeriesChartType.Spline;
            resSeries.MarkerStyle = MarkerStyle.Circle;
            resSeries.MarkerSize = 5;
            resSeries.BorderWidth = 2;

            phaseSeries.ChartType = SeriesChartType.Spline;
            phaseSeries.MarkerStyle = MarkerStyle.Circle;
            phaseSeries.MarkerSize = 5;
            phaseSeries.BorderWidth = 2;

            List<double> freqs = new List<double>();
            List<double> resistivities = new List<double>();
            List<double> phases = new List<double>();

            foreach (DataRowView row in dvStation)
            {
                if (row["频率"] == DBNull.Value || row[resField] == DBNull.Value || row[phaseField] == DBNull.Value)
                    continue;

                double freq = Convert.ToDouble(row["频率"]);
                double res = Convert.ToDouble(row[resField]);
                double phase = Convert.ToDouble(row[phaseField]);

                if (freq <= 0 || res <= 0) continue;

                freqs.Add(freq);
                resistivities.Add(res);
                phases.Add(phase);

                resSeries.Points.AddXY(freq, res);
                phaseSeries.Points.AddXY(freq, phase);
            }

            if (freqs.Count == 0) return;

            bool canLogX = freqs.All(f => f > 0);
            bool canLogYRes = resistivities.All(r => r > 0);

            chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = canLogX;
            chartResistivity.ChartAreas[0].AxisY.IsLogarithmic = canLogYRes;
            chartPhase.ChartAreas[0].AxisX.IsLogarithmic = canLogX;
            chartPhase.ChartAreas[0].AxisY.IsLogarithmic = false;

            chartResistivity.ChartAreas[0].AxisX.IsReversed = true;
            chartPhase.ChartAreas[0].AxisX.IsReversed = true;

            if (canLogX && freqs.Count > 0)
            {
                double minX = freqs.Min();
                double maxX = freqs.Max();
                if (minX == maxX)
                {
                    minX = minX * 0.1;
                    maxX = maxX * 10;
                }
                chartResistivity.ChartAreas[0].AxisX.Minimum = Math.Pow(10, Math.Floor(Math.Log10(minX)));
                chartResistivity.ChartAreas[0].AxisX.Maximum = Math.Pow(10, Math.Ceiling(Math.Log10(maxX)));
                chartPhase.ChartAreas[0].AxisX.Minimum = chartResistivity.ChartAreas[0].AxisX.Minimum;
                chartPhase.ChartAreas[0].AxisX.Maximum = chartResistivity.ChartAreas[0].AxisX.Maximum;
            }

            BeautifyChartAxes(chartResistivity.ChartAreas[0]);
            BeautifyChartAxes(chartPhase.ChartAreas[0]);
        }

        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;

            area.BorderDashStyle = ChartDashStyle.Solid;
            area.BorderColor = System.Drawing.Color.Black;
            area.BorderWidth = 1;

            area.AxisY.TitleAlignment = StringAlignment.Near;

            area.AxisX.LabelStyle.Format = "0.###";
            area.AxisY.LabelStyle.Format = "0.##";
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 9f);
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 9f);
            area.AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 10f);
            area.AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 10f);
            area.AxisX.LabelStyle.Angle = 0;

            area.AxisX.MajorGrid.LineWidth = 1;
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            area.AxisY.MajorGrid.LineWidth = 1;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisX.MinorGrid.LineColor = System.Drawing.Color.Gainsboro;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.MinorGrid.LineColor = System.Drawing.Color.Gainsboro;

            area.AxisX.LineWidth = 1;
            area.AxisX.LineColor = System.Drawing.Color.Black;
            area.AxisY.LineWidth = 1;
            area.AxisY.LineColor = System.Drawing.Color.Black;
        }

        private List<StationInfo> QueryStationsForLine(string lineName)
        {
            var stations = new List<StationInfo>();
            if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
            {
                MessageBox.Show("错误：测点图层未打开！");
                return stations;
            }

            RecordSet rs = null;

            try
            {
                Fields fields = m_SelectedStationLayer.Fields;
                string lineField = "测线号";
                if (fields.IndexOf(lineField) < 0)
                {
                    MessageBox.Show($"严重错误：字段 '{lineField}' 不存在！");
                    return stations;
                }

                QueryDef query = new QueryDef
                {
                    Filter = $"{lineField} = '{lineName}'",
                    SubFields2 = "*"
                };

                rs = m_SelectedStationLayer.Select(query);
                if (rs == null)
                {
                    MessageBox.Show($"查询失败：Select 返回 null\nFilter: {query.Filter}");
                    return stations;
                }

                if (rs.Count == 0)
                {
                    MessageBox.Show($"查询无结果！Filter: {query.Filter}");
                    return stations;
                }

                rs.MoveLast();
                rs.MoveFirst();

                int totalRecords = rs.Count;

                do
                {
                    IGeometry geomBase = null;
                    Record att = null;
                    double x = 0, y = 0;
                    bool geomSuccess = false;

                    try
                    {
                        geomBase = rs.Geometry;
                        if (geomBase is GeoPoints geomPoints && geomPoints.Count > 0)
                        {
                            Dot3D firstDot = null;
                            try
                            {
                                firstDot = geomPoints.GetItem(0);
                                x = firstDot.X;
                                y = firstDot.Y;
                                geomSuccess = true;
                            }
                            finally
                            {
                                if (firstDot != null) try { Marshal.ReleaseComObject(firstDot); } catch { }
                            }
                        }

                        if (!geomSuccess)
                            continue;

                        att = rs.Att;
                        if (att == null)
                            continue;

                        object val = att["测点号"];
                        string stationName = val?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(stationName))
                            continue;

                        stations.Add(new StationInfo
                        {
                            StationName = stationName,
                            X = x,
                            Y = y
                        });
                    }
                    finally
                    {
                        if (geomBase != null) try { Marshal.ReleaseComObject(geomBase); } catch { }
                        if (att != null) try { Marshal.ReleaseComObject(att); } catch { }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"异常：{ex.Message}", "QueryStationsForLine");
            }
            finally
            {
                if (rs != null)
                {
                    try { Marshal.ReleaseComObject(rs); } catch { }
                }
            }

            return stations.OrderBy(s => s.X).ToList();
        }

        private DataTable QuerySoundingDataForLine(string lineName)
        {
            DataTable dataTable = new DataTable();

            if (m_SelectedSoundingTable == null || !m_SelectedSoundingTable.HasOpen())
            {
                return dataTable;
            }

            RecordSet rs = null;
            Fields fields = null;

            try
            {
                QueryDef query = new QueryDef
                {
                    Filter = $"测线编号 = '{lineName}'",
                    SubFields2 = "*",
                };

                rs = m_SelectedSoundingTable.Select(query);
                if (rs == null)
                {
                    MessageBox.Show("QuerySoundingDataForLine: Select(query) 返回 null。");
                    return dataTable;
                }

                rs.MoveLast();
                rs.MoveFirst();

                fields = rs.Fields;
                if (fields == null) return dataTable;

                for (int i = 0; i < fields.Count; i++)
                {
                    Field fld = fields[i];
                    if (fld != null)
                    {
                        Type colType = typeof(string);
                        switch (fld.FieldType)
                        {
                            case FieldType.FldDouble: colType = typeof(double); break;
                            case FieldType.FldFloat: colType = typeof(float); break;
                            case FieldType.FldShort: colType = typeof(short); break;
                            case FieldType.FldLong: colType = typeof(int); break;
                            case FieldType.FldInt64: colType = typeof(long); break;
                        }
                        dataTable.Columns.Add(fld.FieldName, colType);
                    }
                }

                do
                {
                    Record att = null;
                    try
                    {
                        att = rs.Att;
                        if (att == null) continue;

                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < fields.Count; i++)
                        {
                            object val = att.GetValue(i);
                            if (val != null && val != DBNull.Value)
                            {
                                dataRow[i] = val;
                            }
                            else
                            {
                                dataRow[i] = DBNull.Value;
                            }
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                    finally
                    {
                        if (att != null) { try { Marshal.ReleaseComObject(att); } catch { } }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"QuerySoundingDataForLine 捕获到异常: {ex.Message}", "错误");
            }
            finally
            {
                if (fields != null) { try { Marshal.ReleaseComObject(fields); } catch { } }
                if (rs != null) { try { Marshal.ReleaseComObject(rs); } catch { } }
            }
            return dataTable;
        }

        private void UpdateDataGrids()
        {
            if (m_CurrentLineData == null)
            {
                gridData.DataSource = null;
                return;
            }

            try
            {
                DataView dvData = new DataView(m_CurrentLineData);

                List<string> cols = new List<string> { "测点编号", "频率", "视电阻率", "相位" };
                if (m_CurrentLineData.Columns.Contains("计算后电阻率"))
                {
                    cols.Add("计算后电阻率");
                }

                gridData.DataSource = dvData.ToTable(false, cols.ToArray());
                gridData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"填充数据表格时出错: {ex.Message}", "表格错误");
            }
        }

        private List<InversionResultPoint> ParseKnowFile(string filePath)
        {
            var results = new List<InversionResultPoint>();
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"结果文件不存在: {filePath}", "错误");
                return results;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 6)
                    {
                        if (double.TryParse(parts[2], out double x) &&
                            double.TryParse(parts[3], out double y) &&
                            double.TryParse(parts[4], out double depth) &&
                            double.TryParse(parts[5], out double resistivity))
                        {
                            results.Add(new InversionResultPoint
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = x,
                                Y = y,
                                Depth = depth,
                                Resistivity = resistivity
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析 KNOW 文件时出错: {ex.Message}", "文件解析失败");
            }
            return results;
        }

        private System.Drawing.Color GetColorForValue(double value, double min, double max)
        {
            if (min >= max) return System.Drawing.Color.Black;

            double normalized = (value - min) / (max - min);

            if (normalized < 0.25)
                return System.Drawing.Color.FromArgb(0, (int)(255 * (normalized * 4)), 255);
            if (normalized < 0.5)
                return System.Drawing.Color.FromArgb(0, 255, (int)(255 * (1 - (normalized - 0.25) * 4)));
            if (normalized < 0.75)
                return System.Drawing.Color.FromArgb((int)(255 * ((normalized - 0.5) * 4)), 255, 0);

            return System.Drawing.Color.FromArgb(255, (int)(255 * (1 - (normalized - 0.75) * 4)), 0);
        }

        private void DisplayInversionResults(List<InversionResultPoint> results)
        {
            var chart = chartResultSection;

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();

            if (results == null || results.Count == 0) return;

            chart.Titles.Add("CSAMT 一维反演电阻率剖面图 (深部 > 1000m)");
            chart.Titles[0].Font = new System.Drawing.Font("微软雅黑", 12f, FontStyle.Bold);

            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "相对距离 (m)";
            ca.AxisY.Title = "深度 (m)";
            ca.AxisY.IsReversed = false;

            Series s = chart.Series.Add("ResistivitySection");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.MarkerSize = 12;
            s.BorderWidth = 0;
            s.IsVisibleInLegend = false;

            var validResults = results.Where(r => r.Resistivity > 0 && r.Depth < -1000).ToList();
            if (validResults.Count == 0)
            {
                MessageBox.Show("结果文件中没有深度超过 1000m 的有效数据。", "提示");
                return;
            }

            double minX = validResults.Min(r => r.X);
            double maxXRel = validResults.Max(r => r.X) - minX;
            double maxDepth = validResults.Max(r => Math.Abs(r.Depth));

            var logResistivities = validResults.Select(r => Math.Log10(r.Resistivity)).ToList();
            double minLogRes = logResistivities.Min();
            double maxLogRes = logResistivities.Max();

            Legend legend = chart.Legends.Add("ColorScale");
            legend.Docking = Docking.Right;
            legend.Alignment = StringAlignment.Center;
            legend.Title = "lg(ρ/Ω·m)";
            legend.TitleFont = new System.Drawing.Font("微软雅黑", 9F, FontStyle.Bold);

            int legendSteps = 7;
            for (int i = 0; i <= legendSteps; i++)
            {
                double val = minLogRes + (maxLogRes - minLogRes) * i / legendSteps;
                System.Windows.Forms.DataVisualization.Charting.LegendItem item = new System.Windows.Forms.DataVisualization.Charting.LegendItem
                {
                    Name = val.ToString("F1"),
                    Color = GetColorForValue(val, minLogRes, maxLogRes),
                    MarkerStyle = MarkerStyle.Square,
                    MarkerSize = 12
                };
                legend.CustomItems.Add(item);
            }

            int markerSize = s.MarkerSize;
            double halfMarkerPixels = markerSize / 2.0;
            double paddingPixels = halfMarkerPixels + 2;

            int chartWidth = chart.Width;
            int chartHeight = chart.Height;

            double plotAreaWidthRatio = 0.70;
            double plotAreaHeightRatio = 0.75;
            double plotAreaWidthPixels = chartWidth * plotAreaWidthRatio;
            double plotAreaHeightPixels = chartHeight * plotAreaHeightRatio;

            double dataRangeX = maxXRel;
            double dataRangeY = maxDepth - 1000;

            if (dataRangeX <= 0) dataRangeX = 1;
            if (dataRangeY <= 0) dataRangeY = 1;

            double pixelsPerUnitX = plotAreaWidthPixels / dataRangeX;
            double pixelsPerUnitY = plotAreaHeightPixels / dataRangeY;

            double offsetX = paddingPixels / pixelsPerUnitX;
            double offsetY = paddingPixels / pixelsPerUnitY;

            foreach (var point in validResults)
            {
                double logRes = Math.Log10(point.Resistivity);

                double relativeX = point.X - minX;
                double adjustedX = relativeX + offsetX;

                double positiveDepth = Math.Abs(point.Depth);
                double adjustedY = positiveDepth + offsetY;

                int pointIndex = s.Points.AddXY(adjustedX, adjustedY);
                s.Points[pointIndex].Color = GetColorForValue(logRes, minLogRes, maxLogRes);
                s.Points[pointIndex].ToolTip = $"X: {point.X:F1}\nRel X: {relativeX:F1}\nDepth: {positiveDepth:F0}m\nRes: {point.Resistivity:F1}";
            }

            ca.AxisX.IsMarginVisible = false;
            ca.AxisY.IsMarginVisible = false;

            ca.AxisX.Minimum = 0;
            ca.AxisX.Maximum = maxXRel + offsetX * 2;

            ca.AxisY.Minimum = 1000;
            ca.AxisY.Maximum = maxDepth + offsetY * 2;

            ca.BorderWidth = 0;

            ca.AxisX.LineWidth = 2;
            ca.AxisX.LineColor = System.Drawing.Color.Black;
            ca.AxisY.LineWidth = 2;
            ca.AxisY.LineColor = System.Drawing.Color.Black;

            ca.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray);
            ca.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray);
            ca.AxisX.MinorGrid.Enabled = false;
            ca.AxisY.MinorGrid.Enabled = false;

            ca.AxisX.MajorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
            ca.AxisY.MajorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;

            BeautifyChartAxes(ca);
        }

        #endregion

        #region 内部类与窗体事件等

        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private class InversionResultPoint
        {
            public string LineName { get; set; }
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Depth { get; set; }
            public double Resistivity { get; set; }
        }

        private void Form_CSAMT1di_Load(object sender, EventArgs e)
        {
            if (splitContainerMain.Orientation == Orientation.Vertical)
            {
                splitContainerMain.SplitterDistance = splitContainerMain.Width / 2;
            }
            else
            {
                splitContainerMain.SplitterDistance = splitContainerMain.Height / 2;
            }

            txtAllowError.Text = "0.1";
            txtActualError.Text = "";

            LoadLayersFromMap();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InitDragEvent()
        {
            panelTitle.MouseDown += TitlePanel_MouseDown;
            panelTitle.MouseMove += TitlePanel_MouseMove;
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

        private void chartLayout_MouseClick(object sender, MouseEventArgs e)
        {
            var hitTestResult = chartLayout.HitTest(e.X, e.Y);

            if (hitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                DataPoint dataPoint = (DataPoint)hitTestResult.Object;
                string stationName = dataPoint.Tag?.ToString();

                if (!string.IsNullOrEmpty(stationName))
                {
                    SelectStationAndRefreshCharts(stationName);
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void labelOffsetFromCenter_Click(object sender, EventArgs e)
        {

        }

        private void labelTxRxDistance_Click(object sender, EventArgs e)
        {

        }

        private void HighlightLayoutStation(string stationName)
        {
            try
            {
                if (chartLayout.Series.Count > 0 && chartLayout.Series.FindByName("Stations") != null)
                {
                    foreach (var point in chartLayout.Series["Stations"].Points)
                    {
                        if (point.Tag?.ToString() == stationName)
                        {
                            point.MarkerColor = System.Drawing.Color.Red;
                            point.MarkerSize = 10;
                        }
                        else
                        {
                            point.MarkerColor = System.Drawing.Color.Blue;
                            point.MarkerSize = 6;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Highlight layout station error: {ex.Message}");
            }
        }

        private void UpdateSelectedStationInfo(string stationName)
        {
            if (string.IsNullOrEmpty(stationName) || m_StationDistances == null)
            {
                txtTxRxDistance.Text = "";
                txtOffsetFromCenter.Text = "";
                return;
            }

            if (m_StationDistances.TryGetValue(stationName, out var distances))
            {
                txtTxRxDistance.Text = distances.Distance.ToString("F2");
                txtOffsetFromCenter.Text = distances.Offset.ToString("F2");
            }
            else
            {
                txtTxRxDistance.Text = "";
                txtOffsetFromCenter.Text = "";
            }
        }

        private void chartProfileView_MouseClick(object sender, MouseEventArgs e)
        {
            // 计算页不再用于点选
        }

        private void btnGenerateModel_Click(object sender, EventArgs e)
        {
            try
            {
                int layerCount = (int)nudLayerCount.Value;
                double initialThickness = (double)nudInitialThickness.Value;
                double growthRate = (double)nudGrowthRate.Value;
                double initialResistivity = (double)nudInitialResistivity.Value;

                DataTable dtModel = new DataTable();
                dtModel.Columns.Add("层号", typeof(int));
                dtModel.Columns.Add("厚度(m)", typeof(double));
                dtModel.Columns.Add("电阻率(Ω·m)", typeof(double));
                dtModel.Columns.Add("顶深(m)", typeof(double));
                dtModel.Columns.Add("底深(m)", typeof(double));

                double currentThickness = initialThickness;
                double currentTopDepth = 0;

                for (int i = 1; i <= layerCount; i++)
                {
                    double bottomDepth = currentTopDepth + currentThickness;

                    DataRow row = dtModel.NewRow();
                    row["层号"] = i;
                    row["厚度(m)"] = Math.Round(currentThickness, 2);
                    row["电阻率(Ω·m)"] = initialResistivity;
                    row["顶深(m)"] = Math.Round(currentTopDepth, 2);
                    row["底深(m)"] = Math.Round(bottomDepth, 2);
                    dtModel.Rows.Add(row);

                    currentTopDepth = bottomDepth;
                    currentThickness *= growthRate;
                }

                gridModelLayers.DataSource = dtModel;
                gridModelLayers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (gridModelLayers.Columns.Count > 0)
                {
                    gridModelLayers.Columns["层号"].ReadOnly = true;
                    gridModelLayers.Columns["顶深(m)"].ReadOnly = true;
                    gridModelLayers.Columns["底深(m)"].ReadOnly = true;
                }

                MessageBox.Show($"已生成 {layerCount} 层模型。", "模型生成成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成模型时出错: {ex.Message}", "错误");
            }
        }

        #endregion
    }
}