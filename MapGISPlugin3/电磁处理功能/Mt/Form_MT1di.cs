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

namespace MapGISPlugin3
{
    public partial class Form_MT1di : Form
    {
        // --- 核心变量 ---
        private IApplication _hook;
        private List<MapLayer> m_allPointLayers;
        private List<MapLayer> m_allObjectLayers;
        private SFeatureCls m_SelectedStationLayer;
        private ObjectCls m_SelectedSoundingTable;
        private List<StationInfo> m_CurrentLineStations;
        private DataTable m_CurrentLineData;
        private string m_CurrentSelectedStationName;
        private const string ProfileLegendName = "ProfileLegend";
        private const string ResistivityLegendName = "ResistivityLegend";
        private const string PhaseLegendName = "PhaseLegend";

        public Form_MT1di(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;

            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();
        }

        #region 窗口拖动逻辑
        private Point mousePoint = new Point();

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
        #endregion

        private void LoadLayersFromMap()
        {
            m_allPointLayers.Clear();
            m_allObjectLayers.Clear();
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
                    layer = null;
                    try
                    {
                        layer = electroMap.get_Layer(i);
                        if (layer == null) continue;
                        ProcessLayerForComboBox_Debug(layer);
                    }
                    catch (COMException comEx)
                    {
                        MessageBox.Show($"遍历图层索引 {i} 时发生 COM 错误: {comEx.Message}", "LoadLayersFromMap - COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"遍历图层索引 {i} 时发生错误: {ex.Message}", "LoadLayersFromMap - 错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"查找地图或获取图层数COM错误: {comEx.Message}", "LoadLayersFromMap - 致命COM错误");
                return;
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
                MessageBox.Show("最终未能加载任何符合条件的测点图层到下拉框。", "LoadLayersFromMap - 结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    int subLayerCount = 0;
                    try { subLayerCount = groupLayer.Count; } catch { }
                    MapLayer subLayer = null;
                    for (int i = 0; i < subLayerCount; i++)
                    {
                        subLayer = null;
                        try
                        {
                            subLayer = groupLayer.get_Item(i);
                            ProcessLayerForComboBox_Debug(subLayer);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (layer is VectorLayer vectorLayer)
                {
                    GeomType geomType = GeomType.Unknown;
                    bool isPoint = false;
                    try
                    {
                        geomType = vectorLayer.GeometryType;
                        isPoint = (geomType == GeomType.Pnt);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    if (isPoint)
                    {
                        bool nameMatch = false;
                        if (layerName != null && layerName.Contains("测点"))
                        {
                            nameMatch = true;
                        }

                        if (nameMatch)
                        {
                            m_allPointLayers.Add(layer);
                            cmbStationLayer.Items.Add(layer);
                        }
                    }
                }
                else if (layer is ObjectLayer objectLayer)
                {
                    if (layerName != null && layerName.Contains("测深数据"))
                    {
                        m_allObjectLayers.Add(layer);
                    }
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"处理图层 '{layerName}' 时发生 COM 错误: {comEx.Message}", "ProcessLayer - COM 错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理图层 '{layerName}' 时发生错误: {ex.Message}", "ProcessLayer - 错误");
            }
        }

        #region --- 主控件事件 ---

        private void cmbStationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
            m_SelectedStationLayer = null;
            if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
            m_SelectedSoundingTable = null;

            cmbLineName.Items.Clear();
            ClearAllDisplays();

            if (cmbStationLayer.SelectedItem == null || !(cmbStationLayer.SelectedItem is MapLayer selectedLayer))
            {
                cmbLineName.Enabled = false;
                return;
            }

            Console.WriteLine($"用户选择了测点图层: {selectedLayer.Name}");

            try
            {
                m_SelectedStationLayer = selectedLayer.GetData() as SFeatureCls;
                if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
                {
                    MessageBox.Show($"无法从图层 '{selectedLayer.Name}' 获取有效的要素类数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmbLineName.Enabled = false;
                    m_SelectedStationLayer = null;
                    return;
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"获取图层数据时发生 COM 错误: {comEx.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取图层数据时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                return;
            }

            string stationLayerName = selectedLayer.Name;
            string expectedTableName = stationLayerName.Replace("测点", "测深数据");
            MapLayer soundingLayer = m_allObjectLayers.FirstOrDefault(layer => layer != null && layer.Name == expectedTableName);

            if (soundingLayer == null)
            {
                MessageBox.Show($"未找到与 '{stationLayerName}' 匹配的测深数据表 '{expectedTableName}'！", "关联失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                return;
            }

            try
            {
                m_SelectedSoundingTable = soundingLayer.GetData() as ObjectCls;
                if (m_SelectedSoundingTable == null || !m_SelectedSoundingTable.HasOpen())
                {
                    MessageBox.Show($"图层 '{expectedTableName}' 不是有效的对象类或无法打开！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_SelectedSoundingTable = null;
                    cmbLineName.Enabled = false;
                    if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                    m_SelectedStationLayer = null;
                    return;
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"获取测深表数据时发生 COM 错误: {comEx.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
                m_SelectedSoundingTable = null;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取测深表数据时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
                m_SelectedSoundingTable = null;
                return;
            }

            cmbLineName.Enabled = true;
            FillLineComboBox();

            if (cmbLineName.Items.Count > 0)
            {
                cmbLineName.Text = "请选择测线...";
            }
            else
            {
                MessageBox.Show($"在测点图层 '{stationLayerName}' 中未能查询到任何测线号。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbLineName.Enabled = false;
                ClearAllDisplays();
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
                UpdateDataGrids();

                if (m_CurrentLineStations.Count > 0)
                {
                    string firstStation = m_CurrentLineStations[0].StationName;
                    SelectStationAndRefreshCharts(firstStation);
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生 COM 错误: {comEx.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearAllDisplays();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearAllDisplays();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region --- 标签页控件事件 ---

        private void chartProfileView_MouseClick(object sender, MouseEventArgs e)
        {
            var hitTestResult = chartProfileView.HitTest(e.X, e.Y);
            if (hitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                DataPoint dataPoint = (DataPoint)hitTestResult.Object;
                string stationName = dataPoint.Tag?.ToString();
                if (string.IsNullOrEmpty(stationName))
                {
                    stationName = dataPoint.Label;
                }

                if (!string.IsNullOrEmpty(stationName))
                {
                    SelectStationAndRefreshCharts(stationName);
                }
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("没有加载任何测线数据，无法计算。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            int its = (int)nudIterationCount.Value;
            int iwd = rbInversionTE.Checked ? 0 : 1;

            string tempInputFile = Path.GetTempFileName();
            string exePath;
            string pluginDir;
            string workspaceName;
            string fullWorkspacePath;

            try
            {
                pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string algorithmDir = Path.Combine(pluginDir, "Algorithm", "MT1di");
                exePath = Path.Combine(algorithmDir, "a.exe");

                if (!File.Exists(exePath))
                {
                    throw new FileNotFoundException($"计算程序 'a.exe' 未找到。\n请确保它位于: {exePath}");
                }

                workspaceName = Path.GetRandomFileName().Substring(0, 6);
                fullWorkspacePath = Path.Combine(pluginDir, workspaceName);

                if (Directory.Exists(fullWorkspacePath))
                {
                    workspaceName = Path.GetRandomFileName().Substring(0, 6);
                    fullWorkspacePath = Path.Combine(pluginDir, workspaceName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查找 'a.exe' 或创建工作区失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Cursor = Cursors.Default;
                return;
            }

            try
            {
                Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);

                using (StreamWriter writer = new StreamWriter(tempInputFile))
                {
                    foreach (DataRow row in m_CurrentLineData.Rows)
                    {
                        string stationName = row["测点编号"].ToString();
                        StationInfo station;
                        if (!stationCoords.TryGetValue(stationName, out station)) { continue; }

                        double z_coord = 0.0;
                        double period = GetDoubleFromRow(row, "周期", 1.0);
                        if (period == 0) period = 1e-9;
                        double freq = 1.0 / period;

                        double rxy = GetDoubleFromRow(row, "视电阻率_TE", 0.0);
                        double pxy = GetDoubleFromRow(row, "相位_TE", 0.0);
                        double ryx = GetDoubleFromRow(row, "视电阻率_TM", 0.0);
                        double pyx = GetDoubleFromRow(row, "相位_TM", 0.0);

                        writer.WriteLine($"{stationName} {station.X} {station.Y} {z_coord} {freq} {rxy} {pxy} {ryx} {pyx}");
                    }
                }

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = exePath;
                startInfo.Arguments = $"\"{tempInputFile}\" \"{workspaceName}\" {iwd} {its}";
                startInfo.WorkingDirectory = pluginDir;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;

                string output = "";
                string error = "";

                using (Process process = Process.Start(startInfo))
                {
                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show($"计算成功！\n\n结果已保存到:\n{fullWorkspacePath}\n\n(提示: 结果文件为 'KNOW')\n程序输出:\n{output}",
                              "计算完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        throw new Exception($"a.exe 运行失败 (ExitCode: {process.ExitCode})。\n\n工作目录:\n{pluginDir}\n\n命令行:\n{startInfo.Arguments}\n\n错误信息:\n{error}\n\n输出信息:\n{output}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"计算过程中发生严重错误: \n{ex.Message}", "计算失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (File.Exists(tempInputFile))
                {
                    try { File.Delete(tempInputFile); }
                    catch (Exception ex) { Console.WriteLine($"删除临时文件 {tempInputFile} 失败: {ex.Message}"); }
                }
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region --- 核心辅助函数 ---

        private double GetDoubleFromRow(DataRow row, string columnName, double defaultValue)
        {
            try
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    return Convert.ToDouble(row[columnName]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDoubleFromRow 转换失败: {columnName}, 错误: {ex.Message}");
            }
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
                    if (rs == null) return;
                }

                try
                {
                    rs.MoveLast();
                    rs.MoveFirst();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"预加载记录集出错: {ex.Message}");
                    return;
                }

                int currentIndex = 0;
                int totalRecords = rs.Count;
                if (totalRecords == 0) return;

                do
                {
                    Record currentAtt = null;
                    try
                    {
                        currentIndex++;
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

                        if (currentIndex > rs.Count * 1.5 && rs.Count > 0) break;
                    }
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message}");
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
            catch (COMException comEx)
            {
                MessageBox.Show($"查询过程中发生 COM 错误: {comEx.Message}", "错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询过程中发生错误: {ex.Message}", "错误");
            }
            finally
            {
                if (rs != null) try { Marshal.ReleaseComObject(rs); } catch { }
                this.Cursor = Cursors.Default;
                cmbLineName.Enabled = (uniqueLines.Count > 0);
            }
        }

        private void ClearAllDisplays()
        {
            if (chartProfileView.Series != null) chartProfileView.Series.Clear();
            if (chartResistivity.Series != null) chartResistivity.Series.Clear();
            if (chartPhase.Series != null) chartPhase.Series.Clear();

            if (chartResistivity.ChartAreas.Count > 0)
            {
                chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartResistivity.ChartAreas[0].AxisY.IsLogarithmic = false;
            }
            if (chartPhase.ChartAreas.Count > 0)
            {
                chartPhase.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartPhase.ChartAreas[0].AxisY.IsLogarithmic = false;
            }
            if (chartProfileView.ChartAreas.Count > 0)
            {
                chartProfileView.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartProfileView.ChartAreas[0].AxisY.IsLogarithmic = false;
            }

            gridTE.DataSource = null;
            gridTM.DataSource = null;
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;

            if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = "周期-视电阻率";
            if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = "周期-相位";
        }

        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;

            chartProfileView.Series.Clear();

            // 确保图例存在
            if (chartProfileView.Legends[ProfileLegendName] == null)
                InitChartLegend(chartProfileView, ProfileLegendName);

            // 设置图表区域位置，为图例预留上方空间
            if (chartProfileView.ChartAreas.Count > 0)
            {
                var chartArea = chartProfileView.ChartAreas[0];

                // 图表区域从上方15%开始，为图例预留空间
                chartArea.Position = new ElementPosition(8, 5, 85, 90);

                // 绘图区内边距
                chartArea.InnerPlotPosition = new ElementPosition(10, 10, 85, 85);
            }

            // 创建测点系列
            Series stationSeries = chartProfileView.Series.Add("测点");
            stationSeries.ChartType = SeriesChartType.Point;
            stationSeries.MarkerStyle = MarkerStyle.Circle;
            stationSeries.MarkerSize = 8;
            stationSeries.MarkerColor = System.Drawing.Color.Blue;
            stationSeries.IsValueShownAsLabel = true;
            stationSeries.Legend = ProfileLegendName;
            stationSeries.LegendText = "测点";

            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                Console.WriteLine("UpdateProfileView: m_CurrentLineStations 为空，不绘制。");
                return;
            }

            // 计算坐标范围（添加边距避免点贴边）
            double minX = m_CurrentLineStations.Min(s => s.X);
            double maxX = m_CurrentLineStations.Max(s => s.X);
            double minY = m_CurrentLineStations.Min(s => s.Y);
            double maxY = m_CurrentLineStations.Max(s => s.Y);

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;

            double marginX = 0;
            double marginY = 0;

            // 如果只有一个点，或者所有点X坐标相同
            if (Math.Abs(rangeX) < 1e-6)
            {
                // 人为给一个宽度，比如前后各加 100 米，或者加 1.0
                marginX = 100.0;
            }
            else
            {
                marginX = rangeX * 0.1; // 正常的 10% 边距
            }

            // 如果只有一个点，或者所有点Y坐标相同
            if (Math.Abs(rangeY) < 1e-6)
            {
                // 人为给一个高度
                marginY = 100.0;
            }
            else
            {
                marginY = rangeY * 0.1;
            }

            // 添加测点数据
            foreach (var station in m_CurrentLineStations)
            {
                int index = stationSeries.Points.AddXY(station.X, station.Y);
                stationSeries.Points[index].Label = station.StationName;
                stationSeries.Points[index].Tag = station.StationName;
            }

            // 设置坐标轴范围和样式
            chartProfileView.ChartAreas[0].AxisX.Minimum = minX - marginX;
            chartProfileView.ChartAreas[0].AxisX.Maximum = maxX + marginX;
            chartProfileView.ChartAreas[0].AxisY.Minimum = minY - marginY;
            chartProfileView.ChartAreas[0].AxisY.Maximum = maxY + marginY;

            chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = true;
            chartPhase.ChartAreas[0].AxisX.IsLogarithmic = true;

            // 设置坐标轴标题
            chartProfileView.ChartAreas[0].AxisX.Title = "X坐标";
            chartProfileView.ChartAreas[0].AxisY.Title = "Y坐标";
            chartProfileView.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);
            chartProfileView.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);

            chartProfileView.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            chartProfileView.ChartAreas[0].AxisY.LabelStyle.Format = "F0";

            // 设置坐标轴标签样式
            chartProfileView.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            chartProfileView.ChartAreas[0].AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);

            // 启用边距
            chartProfileView.ChartAreas[0].AxisX.IsMarginVisible = true;
            chartProfileView.ChartAreas[0].AxisY.IsMarginVisible = true;

            // 应用美化设置
            BeautifyProfileViewAxes(chartProfileView.ChartAreas[0]);

            chartProfileView.ChartAreas[0].RecalculateAxesScale();
            CalibrateLegendSize(chartProfileView);

            // 强制刷新
            chartProfileView.Invalidate();
        }

        /// <summary>
        /// 专门为小地图美化的坐标轴设置
        /// </summary>
        private void BeautifyProfileViewAxes(ChartArea area)
        {
            if (area == null) return;

            // 横坐标设置
            area.AxisX.LabelStyle.Format = "F0";
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            area.AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);
            area.AxisX.TitleAlignment = StringAlignment.Center;

            // 纵坐标设置
            area.AxisY.LabelStyle.Format = "F0";
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            area.AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);
            area.AxisY.TitleAlignment = StringAlignment.Center;

            // 坐标轴间隔设置（小地图使用线性坐标）
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            // 网格线设置
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

            // 坐标轴线设置
            area.AxisX.LineWidth = 1;
            area.AxisX.LineColor = System.Drawing.Color.Black;
            area.AxisY.LineWidth = 1;
            area.AxisY.LineColor = System.Drawing.Color.Black;

            area.RecalculateAxesScale();
        }

        private void SelectStationAndRefreshCharts(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName)) return;

            m_CurrentSelectedStationName = stationName;

            // 高亮 chartProfileView 上的这个点
            try
            {
                if (chartProfileView.Series.Count > 0 && chartProfileView.Series["测点"] != null)
                {
                    foreach (var point in chartProfileView.Series["测点"].Points)
                    {
                        if (point.Tag?.ToString() == stationName)
                        {
                            point.MarkerSize = 12;
                            point.MarkerColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            point.MarkerSize = 8;
                            point.MarkerColor = System.Drawing.Color.Blue;
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"高亮小地图测点时出错: {ex.Message}"); }

            UpdateRightPanelCharts();
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_CurrentSelectedStationName))
            {
                UpdateRightPanelCharts();
            }
        }

        /// <summary>
        /// (辅助函数) 刷新右栏的两张曲线图
        /// 【修复横坐标标题被遮挡问题】
        /// </summary>
        private void UpdateRightPanelCharts()
        {
            // 先清除所有Series
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();

            // 确保图例存在
            if (chartResistivity.Legends[ResistivityLegendName] == null)
                InitChartLegend(chartResistivity, ResistivityLegendName);
            if (chartPhase.Legends[PhaseLegendName] == null)
                InitChartLegend(chartPhase, PhaseLegendName);

            // 选择TE/TM模式
            string resField = tabControl2.SelectedTab == tabPageDisplayTE ? "视电阻率_TE" : "视电阻率_TM";
            string phaseField = tabControl2.SelectedTab == tabPageDisplayTE ? "相位_TE" : "相位_TM";
            string seriesName = tabControl2.SelectedTab == tabPageDisplayTE ? "TE模式" : "TM模式";

            // 创建电阻率系列
            Series resSeries = chartResistivity.Series.Add("视电阻率");
            resSeries.ChartType = SeriesChartType.Spline;
            resSeries.MarkerStyle = MarkerStyle.Circle;
            resSeries.MarkerSize = 5;
            resSeries.BorderWidth = 2;
            resSeries.Legend = ResistivityLegendName;
            resSeries.LegendText = $"{seriesName} 视电阻率";

            // 创建相位系列
            Series phaseSeries = chartPhase.Series.Add("相位");
            phaseSeries.ChartType = SeriesChartType.Spline;
            phaseSeries.MarkerStyle = MarkerStyle.Circle;
            phaseSeries.MarkerSize = 5;
            phaseSeries.BorderWidth = 2;
            phaseSeries.Legend = PhaseLegendName;
            phaseSeries.LegendText = $"{seriesName} 相位";

            // 数据绑定
            if (m_CurrentLineData == null || string.IsNullOrEmpty(m_CurrentSelectedStationName))
                return;

            DataView dvStation = new DataView(m_CurrentLineData);
            dvStation.RowFilter = $"测点编号 = '{m_CurrentSelectedStationName}'";

            foreach (DataRowView row in dvStation)
            {
                if (row["周期"] == DBNull.Value || row[resField] == DBNull.Value || row[phaseField] == DBNull.Value)
                    continue;

                double period = Convert.ToDouble(row["周期"]);
                double res = Convert.ToDouble(row[resField]);
                double phase = Convert.ToDouble(row[phaseField]);

                resSeries.Points.AddXY(period, res);
                phaseSeries.Points.AddXY(period, phase);
            }

            // 设置坐标轴标题
            chartResistivity.ChartAreas[0].AxisX.Title = "周期(s)";
            chartResistivity.ChartAreas[0].AxisY.Title = "视电阻率";
            chartPhase.ChartAreas[0].AxisX.Title = "周期(s)";
            chartPhase.ChartAreas[0].AxisY.Title = "相位";

            // 应用统一的样式设置
            foreach (var chart in new[] { chartResistivity, chartPhase })
            {
                if (chart.ChartAreas.Count > 0)
                {
                    var chartArea = chart.ChartAreas[0];

                    // 设置图表区域位置，为图例和坐标轴标题预留空间
                    chartArea.Position = new ElementPosition(8, 15, 85, 75);

                    // 设置绘图区内边距
                    chartArea.InnerPlotPosition = new ElementPosition(10, 10, 85, 80);

                    // 横坐标设置
                    chartArea.AxisX.LabelStyle.Format = "0.###";
                    chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
                    chartArea.AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);
                    chartArea.AxisX.IsStartedFromZero = false;
                    chartArea.AxisX.IsMarginVisible = true;

                    // 纵坐标设置
                    chartArea.AxisY.LabelStyle.Format = "0.##";
                    chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
                    chartArea.AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);
                    chartArea.AxisY.IsStartedFromZero = false;
                    chartArea.AxisY.IsMarginVisible = true;

                    // 坐标轴自适应设置
                    chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
                    chartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

                    // 网格线设置
                    chartArea.AxisX.MajorGrid.LineWidth = 1;
                    chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                    chartArea.AxisY.MajorGrid.LineWidth = 1;
                    chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

                    chartArea.AxisX.MinorGrid.Enabled = true;
                    chartArea.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                    chartArea.AxisX.MinorGrid.LineColor = System.Drawing.Color.Gainsboro;

                    chartArea.AxisY.MinorGrid.Enabled = true;
                    chartArea.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                    chartArea.AxisY.MinorGrid.LineColor = System.Drawing.Color.Gainsboro;

                    // 坐标轴线设置
                    chartArea.AxisX.LineWidth = 1;
                    chartArea.AxisX.LineColor = System.Drawing.Color.Black;
                    chartArea.AxisY.LineWidth = 1;
                    chartArea.AxisY.LineColor = System.Drawing.Color.Black;
                }
            }

            // 强制重新计算坐标轴（实现按内容适应）
            chartResistivity.ChartAreas[0].RecalculateAxesScale();
            chartPhase.ChartAreas[0].RecalculateAxesScale();

            // 校准图例尺寸
            CalibrateLegendSize(chartResistivity);
            CalibrateLegendSize(chartPhase);

            // 强制刷新图表
            chartResistivity.Invalidate();
            chartPhase.Invalidate();
        }

        /// <summary>
        /// 美化图表坐标轴
        /// </summary>
        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;

            // 坐标轴标签格式
            area.AxisX.LabelStyle.Format = "0.###";
            area.AxisY.LabelStyle.Format = "0.##";

            // 坐标轴标签字体
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);

            // 坐标轴标题字体
            area.AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);
            area.AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 9f, FontStyle.Bold);

            // 网格线设置
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

            // 坐标轴线设置
            area.AxisX.LineWidth = 1;
            area.AxisX.LineColor = System.Drawing.Color.Black;
            area.AxisY.LineWidth = 1;
            area.AxisY.LineColor = System.Drawing.Color.Black;

            // 应用自适应配置
            ConfigureAutoScaleAxes(area);
        }

        /// <summary>
        /// 初始化图表图例（上方右侧样式，与图表区域分开）
        /// </summary>
        private void InitChartLegend(Chart chart, string legendName)
        {
            chart.Legends.Clear();

            Legend legend = new Legend(legendName)
            {
                IsDockedInsideChartArea = false, // 确保不在图表区域内
                Docking = Docking.Top,
                Alignment = StringAlignment.Far, // 右对齐
                LegendStyle = LegendStyle.Table,
                BorderColor = System.Drawing.Color.LightGray,
                BorderWidth = 1,
                BackColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微软雅黑", 8f),
                // 关键修改：将图例定位在图表区域之外的上方
                Position = new ElementPosition(65, 3, 30, 10) // 在图表上方，不与图表重叠
            };

            chart.Legends.Add(legend);
        }
        /// <summary>
        /// 配置坐标轴自适应
        /// </summary>
        private void ConfigureAutoScaleAxes(ChartArea chartArea)
        {
            if (chartArea == null) return;

            // 横坐标自适应
            chartArea.AxisX.IsStartedFromZero = false;
            chartArea.AxisX.IsMarginVisible = true;
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;

            // 纵坐标自适应  
            chartArea.AxisY.IsStartedFromZero = false;
            chartArea.AxisY.IsMarginVisible = true;
            chartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            // 强制重新计算坐标轴
            chartArea.RecalculateAxesScale();
        }

        /// <summary>
        /// 校准图例尺寸（针对不同图表使用不同尺寸）
        /// </summary>
        private void CalibrateLegendSize(Chart chart)
        {
            if (chart.Legends.Count == 0 || chart.Series.Count == 0) return;

            Legend legend = chart.Legends[0];
            int actualItemCount = chart.Series.Count;
            float singleItemHeight = legend.Font.Height + 4;

            float totalHeightPercent = (singleItemHeight / chart.Height * 100) * actualItemCount + 3;

            // 针对不同图表设置不同的图例尺寸
            if (chart == chartResistivity)
            {
                // chartResistivity 图例更大（内容多）
                legend.Position = new ElementPosition(
                    50,  // 从左边50%开始
                    2,   // 顶部
                    45,  // 宽度45%（更宽）
                    Math.Min(totalHeightPercent, 15) // 高度最大15%
                );
            }
            else if (chart == chartProfileView)
            {
                // chartProfileView 图例更小（内容少）
                legend.Position = new ElementPosition(
                    70,  // 从左边70%开始（更靠右）
                    2,   // 顶部
                    25,  // 宽度25%（更窄）
                    Math.Min(totalHeightPercent, 8)  // 高度最大8%（更矮）
                );
            }
            else
            {
                // 其他图表（chartPhase等）保持中等尺寸
                legend.Position = new ElementPosition(
                    60,  // 从左边60%开始
                    2,   // 顶部
                    35,  // 宽度35%
                    Math.Min(totalHeightPercent, 12) // 高度最大12%
                );
            }
        }

        // 其他辅助函数保持不变...
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
                // --- 1. 打印字段名，确认存在 ---
                Fields fields = m_SelectedStationLayer.Fields;


                // --- 2. 检查测线号字段是否存在 ---
                string lineField = "测线号"; // 你用的字段
                if (fields.IndexOf(lineField) < 0)
                {
                    MessageBox.Show($"严重错误：字段 '{lineField}' 不存在！\n请检查是否叫 '线号'、'LineNo'、'测线' 等");
                    return stations;
                }

                // --- 3. 构造查询 ---
                QueryDef query = new QueryDef
                {
                    Filter = $"{lineField} = '{lineName}'",
                    SubFields2 = "*"
                };

                rs = m_SelectedStationLayer.Select(query);

                // --- 4. 关键：检查 rs 是否为空 ---
                if (rs == null)
                {
                    MessageBox.Show($"查询失败：Select 返回 null\nFilter: {query.Filter}");
                    return stations;
                }

                if (rs.Count == 0)
                {
                    MessageBox.Show($"查询无结果！\n" +
                                   $"Filter: {query.Filter}\n" +
                                   $"lineName = '{lineName}' (长度: {lineName.Length})\n" +
                                   $"请检查测线号是否正确、是否有空格或大小写问题");
                    return stations;
                }


                // --- 5. 预加载 ---
                rs.MoveLast();
                rs.MoveFirst();

                // --- 6. 遍历 (V2.4 - 恢复正确的 while 循环) ---
                int totalRecords = rs.Count;
                int geomFailCount = 0;
                int attNullCount = 0;
                int nameNullCount = 0;
                int successCount = 0;
                string firstGeomType = "N/A";
                bool fieldNameError = false;
                int currentIndex = 0; // <-- 添加安全计数器

                // 【!! 关键修复 !!】 
                // 恢复为 MoveNext 和 IsEOF 检查，这是唯一可靠的模式
                if (totalRecords == 0)
                {
                    MessageBox.Show("遍历报告: totalRecords 为 0，循环前退出。");
                    return stations; // 确保空记录集时安全退出
                }


                do
                {
                    // 【!! 安全退出 !!】 (以防万一)
                    currentIndex++;
                    if (currentIndex > totalRecords * 1.5 && totalRecords > 0)
                    {
                        MessageBox.Show($"[QueryStationsForLine] 迭代超过预期 ({currentIndex} > {totalRecords})，强制停止循环。");
                        break;
                    }

                    IGeometry geomBase = null;
                    Record att = null;
                    double x = 0, y = 0;
                    bool geomSuccess = false;

                    try
                    {
                        geomBase = rs.Geometry;
                        if (geomBase == null)
                        {
                            geomFailCount++;
                            continue;
                        }

                        // --- 检查 1: Geometry ---
                        if (geomBase is GeoPoints geomPoints)
                        {
                            if (geomPoints.Count > 0)
                            {
                                Dot3D firstDot = null;
                                try
                                {
                                    firstDot = geomPoints.GetItem(0);
                                    if (firstDot != null)
                                    {
                                        x = firstDot.X;
                                        y = firstDot.Y;
                                        geomSuccess = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("获取 GeoPoints.GetItem(0) 出错: " + ex.Message);
                                }
                                finally
                                {
                                    if (firstDot != null) try { Marshal.ReleaseComObject(firstDot); } catch { }
                                }
                            }
                        }

                        if (!geomSuccess)
                        {
                            geomFailCount++;
                            if (geomFailCount == 1)
                            {
                                firstGeomType = geomBase.GetType().Name;
                            }
                            continue;
                        }

                        // --- 检查 2: Attribute ---
                        att = rs.Att;
                        if (att == null)
                        {
                            attNullCount++;
                            continue;
                        }

                        // --- 检查 3: Field Value ---
                        object val = null;
                        try
                        {
                            val = att["测点号"];
                        }
                        catch (Exception fieldEx)
                        {
                            MessageBox.Show($"字段名错误：在遍历时无法获取 '测点号' 字段！\n错误: {fieldEx.Message}\n请检查 '字段列表' 弹窗中的确切名称！", "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            fieldNameError = true;
                            break;
                        }

                        string stationName = val?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(stationName))
                        {
                            nameNullCount++;
                            continue;
                        }

                        // --- 成功 ---
                        stations.Add(new StationInfo
                        {
                            StationName = stationName,
                            X = x,
                            Y = y
                        });
                        successCount++;
                    }
                    finally
                    {
                        if (geomBase != null) try { Marshal.ReleaseComObject(geomBase); } catch { }
                        if (att != null) try { Marshal.ReleaseComObject(att); } catch { }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);
                // --- 循环结束 ---
            }
            catch (Exception ex)
            {
                MessageBox.Show($"异常：{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (rs != null)
                {
                    try { Marshal.ReleaseComObject(rs); }
                    catch (Exception ex)
                    {
                        // 吞掉释放时的异常，防止程序崩溃
                        Console.WriteLine($"释放 RecordSet (rs) 时出错: {ex.Message}");
                    }
                }
            }

            return stations.OrderBy(s => s.X).ToList();
        }


        /// <summary>
        /// (辅助函数) TODO 2: 查询指定测线的所有测深数据到 DataTable (V7.6.0.0 兼容版)
        /// 【!! 已修复 - 采用 FillLineComboBox 逻辑 !!】
        /// </summary>
        private DataTable QuerySoundingDataForLine(string lineName)
        {
            DataTable dataTable = new DataTable();
            if (m_SelectedSoundingTable == null || !m_SelectedSoundingTable.HasOpen())
            {
                Console.WriteLine("QuerySoundingDataForLine: m_SelectedSoundingTable 为 null 或未打开。");
                return dataTable;
            }

            RecordSet rs = null;
            Fields fields = null;
            try
            {
                QueryDef query = new QueryDef
                {
                    // 【!! 确认字段名 !!】
                    Filter = $"测线编号 = '{lineName}'",
                    SubFields2 = "*", // 获取所有字段
                    // 【!! 移除 CursorType !!】
                };

                // 【修复逻辑 1】Select
                rs = m_SelectedSoundingTable.Select(query);
                if (rs == null)
                {
                    MessageBox.Show("QuerySoundingDataForLine: Select(query) 返回 null，查询失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return dataTable;
                }

                // 【修复逻辑 2】预加载
                try
                {
                    rs.MoveLast();
                    rs.MoveFirst();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"QuerySoundingDataForLine: 预加载 (MoveLast) 失败: {ex.Message}。继续尝试迭代...", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


                // 动态创建 DataTable 的列结构
                fields = rs.Fields;
                if (fields == null) return dataTable;

                for (int i = 0; i < fields.Count; i++)
                {
                    Field fld = fields[i]; // GetField(index) 是 Fields 类的标准方法
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

                // 填充数据
                int currentIndex = 0;
                // 【修复逻辑 3】增加 !rs.IsEOF
                int totalRecords = rs.Count; // 【!! 关键修复 !!】获取总数用于安全检查

                // 【!! 关键修复 V2.5 !!】
                // 同样, MoveFirst() 后必须用 do-while
                if (totalRecords == 0)
                {
                    Console.WriteLine("QuerySoundingDataForLine: totalRecords 为 0，循环前退出。");
                    return dataTable;
                }

                do
                {
                    Record att = null;
                    try
                    {
                        // 【修复逻辑 4】安全退出
                        currentIndex++;
                        if (currentIndex > totalRecords * 1.5 && rs.Count > 0)
                        {
                            MessageBox.Show($"[调试] 迭代超过预期 ({currentIndex} > {rs.Count})，强制停止循环。", "QuerySoundingDataForLine - 安全退出");
                            break;
                        }

                        att = rs.Att;
                        if (att == null) continue;

                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < fields.Count; i++)
                        {
                            // Record.GetValue(index) 是标准方法
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
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message} (可能该记录损坏，跳过)", "QuerySoundingDataForLine - 迭代错误");
                    }
                    finally
                    {
                        // 在循环内部释放 Att，防止内存泄漏
                        if (att != null) { try { Marshal.ReleaseComObject(att); } catch { } }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);
                // 循环结束
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[调试] QuerySoundingDataForLine 捕获到异常: {ex.Message}", "QuerySoundingDataForLine - 致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (fields != null) { try { Marshal.ReleaseComObject(fields); } catch { } }
                if (rs != null) { try { Marshal.ReleaseComObject(rs); } catch { } }
            }

            return dataTable;
        }


        /// <summary>
        /// (辅助函数) TODO 4 & 5: 刷新 TE 和 TM 页的表格
        /// </summary>
        private void UpdateDataGrids()
        {
            if (m_CurrentLineData == null)
            {
                gridTE.DataSource = null;
                gridTM.DataSource = null;
                return;
            }

            try
            {
                // 1. 绑定 TE 数据 (DataView 用于选择列)
                DataView dvTE = new DataView(m_CurrentLineData);
                // 【!! 确认您的字段名 !!】
                gridTE.DataSource = dvTE.ToTable(false, // false = 不去重
                    "测点编号",
                    "周期",
                    "视电阻率_TE",
                    "相位_TE"
                // 添加您想在 TE 表格中显示的其他列
                );

                // 2. 绑定 TM 数据
                DataView dvTM = new DataView(m_CurrentLineData);
                // 【!! 确认您的字段名 !!】
                gridTM.DataSource = dvTM.ToTable(false,
                    "测点编号",
                    "周期",
                    "视电阻率_TM",
                    "相位_TM"
                // 添加您想在 TM 表格中显示的其他列
                );

                // 3. (可选) 调整列宽
                gridTE.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridTM.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"填充数据表格时出错: {ex.Message}\n\n请检查 '测点编号', '周期', '视电阻率_TE' 等字段名是否与数据表一致。", "表格错误");
            }
        }

        #endregion



        // 内部辅助类
        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private void Form_MT1di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            InitDragEvent();
            InitializeChartStyles(); // 初始化图表样式
        }

        /// <summary>
        /// 清除设计器自动生成的默认Series
        /// </summary>
        private void ClearDefaultSeries()
        {
            if (chartProfileView.Series.Count > 0)
                chartProfileView.Series.Clear();
            if (chartResistivity.Series.Count > 0)
                chartResistivity.Series.Clear();
            if (chartPhase.Series.Count > 0)
                chartPhase.Series.Clear();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 统一初始化图表样式（白色背景 + 上方右侧图例 + 坐标轴自适应）
        /// </summary>
        private void InitializeChartStyles()
        {
            // 为所有图表设置白色背景
            chartProfileView.BackColor = System.Drawing.Color.White;
            chartResistivity.BackColor = System.Drawing.Color.White;
            chartPhase.BackColor = System.Drawing.Color.White;

            // 清除设计器自动创建的默认Series
            ClearDefaultSeries();

            // 初始化所有图表的图例
            InitChartLegend(chartProfileView, ProfileLegendName);
            InitChartLegend(chartResistivity, ResistivityLegendName);
            InitChartLegend(chartPhase, PhaseLegendName);
        }

    }
}