using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
// MapGIS 引用
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Geometry;
using System.Windows.Forms.DataVisualization.Charting;

// 解决引用冲突
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace MapGISPlugin3
{
    public partial class Form_MT2di : Form
    {
        // ====================================================================================
        // 核心变量区
        // ====================================================================================
        private IApplication _hook;
        private List<MapLayer> m_allPointLayers;
        private List<MapLayer> m_allObjectLayers;
        private SFeatureCls m_SelectedStationLayer;
        private ObjectCls m_SelectedSoundingTable;
        private List<StationInfo> m_CurrentLineStations;
        private DataTable m_CurrentLineData;
        private string m_CurrentSelectedStationName;

        // 图例名称常量
        private const string ProfileLegendName = "ProfileLegend";
        private const string ResistivityLegendName = "ResistivityLegend";
        private const string PhaseLegendName = "PhaseLegend";

        // --- 计算与绘图状态变量 ---
        private string m_CurrentWorkspacePath;
        private bool m_IsCalculating = false;

        // --- 内部数据结构 ---
        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private struct ResultPoint
        {
            public double Distance;
            public double Depth;
            public double Value;
        }

        private struct NiceScale { public double Min; public double Max; public double Interval; }

        // ====================================================================================
        // 初始化与加载
        // ====================================================================================
        public Form_MT2di(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;

            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();
        }

        private void Form_MT2di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            InitDragEvent();
            InitializeChartStyles();

            if (timerProgress != null) timerProgress.Tick += timerProgress_Tick;
            if (chartResultSection != null) chartResultSection.Series.Clear();
        }

        private void InitializeChartStyles()
        {
            // 初始化上方图表的样式
            chartProfileView.BackColor = Color.White;
            chartResistivity.BackColor = Color.White;
            chartPhase.BackColor = Color.White;

            InitChartLegend(chartProfileView, ProfileLegendName);
            InitChartLegend(chartResistivity, ResistivityLegendName);
            InitChartLegend(chartPhase, PhaseLegendName);

            ClearDefaultSeries();
        }

        private void ClearDefaultSeries()
        {
            if (chartProfileView.Series.Count > 0) chartProfileView.Series.Clear();
            if (chartResistivity.Series.Count > 0) chartResistivity.Series.Clear();
            if (chartPhase.Series.Count > 0) chartPhase.Series.Clear();
        }

        private void ClearAllDisplays()
        {
            chartProfileView.Series.Clear();
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();
            if (chartResultSection != null) chartResultSection.Series.Clear();
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 窗口拖动
        private Point mousePoint = new Point();
        private void InitDragEvent()
        {
            if (panelTitle != null)
            {
                panelTitle.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) mousePoint = e.Location; };
                panelTitle.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) this.Location = new Point(Control.MousePosition.X - mousePoint.X, Control.MousePosition.Y - mousePoint.Y); };
            }
        }

        // ====================================================================================
        // 【回滚还原】上方图表逻辑 (保持原版 MT2di 代码逻辑，防止压盖)
        // ====================================================================================

        // 1. 初始化图例 (原版样式)
        private void InitChartLegend(Chart chart, string legendName)
        {
            chart.Legends.Clear();
            Legend legend = new Legend(legendName)
            {
                IsDockedInsideChartArea = false,
                Docking = Docking.Top,
                Alignment = StringAlignment.Far,
                LegendStyle = LegendStyle.Table,
                BorderColor = Color.LightGray,
                BorderWidth = 1,
                BackColor = Color.White,
                Font = new Font("微软雅黑", 8f),
                // 关键：原版特定的 Position
                Position = new ElementPosition(65, 3, 30, 10)
            };
            chart.Legends.Add(legend);
        }

        // 2. 校准图例尺寸 (原版逻辑，针对 Resistivity 做了特殊处理)
        private void CalibrateLegendSize(Chart chart)
        {
            if (chart.Legends.Count == 0 || chart.Series.Count == 0) return;

            Legend legend = chart.Legends[0];
            int actualItemCount = chart.Series.Count;
            // 计算高度百分比
            float singleItemHeight = legend.Font.Height + 4;
            float totalHeightPercent = (singleItemHeight / chart.Height * 100) * actualItemCount + 3;

            // 针对不同图表设置不同的图例尺寸 (完全还原)
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
                legend.Position = new ElementPosition(
                    70,
                    2,
                    25,
                    Math.Min(totalHeightPercent, 8)
                );
            }
            else
            {
                // 其他图表（chartPhase等）
                legend.Position = new ElementPosition(
                    60,
                    2,
                    35,
                    Math.Min(totalHeightPercent, 12)
                );
            }
        }

        // 3. 更新测线预览图 (原版逻辑)
        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;
            chartProfileView.Series.Clear();

            if (chartProfileView.Legends[ProfileLegendName] == null) InitChartLegend(chartProfileView, ProfileLegendName);

            // 设置图表区域位置 (完全还原)
            if (chartProfileView.ChartAreas.Count > 0)
            {
                var chartArea = chartProfileView.ChartAreas[0];
                chartArea.Position = new ElementPosition(8, 5, 85, 90);
                chartArea.InnerPlotPosition = new ElementPosition(10, 10, 85, 85);
            }

            Series stationSeries = chartProfileView.Series.Add("测点");
            stationSeries.ChartType = SeriesChartType.Point;
            stationSeries.MarkerStyle = MarkerStyle.Circle;
            stationSeries.MarkerSize = 8;
            stationSeries.Color = Color.Blue;
            stationSeries.IsValueShownAsLabel = true;
            stationSeries.Legend = ProfileLegendName;
            stationSeries.LegendText = "测点";

            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0) return;

            double minX = m_CurrentLineStations.Min(s => s.X);
            double maxX = m_CurrentLineStations.Max(s => s.X);
            double minY = m_CurrentLineStations.Min(s => s.Y);
            double maxY = m_CurrentLineStations.Max(s => s.Y);

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;
            double marginX = (Math.Abs(rangeX) < 1e-6) ? 100.0 : rangeX * 0.1;
            double marginY = (Math.Abs(rangeY) < 1e-6) ? 100.0 : rangeY * 0.1;

            foreach (var station in m_CurrentLineStations)
            {
                int index = stationSeries.Points.AddXY(station.X, station.Y);
                stationSeries.Points[index].Label = station.StationName;
                stationSeries.Points[index].Tag = station.StationName;
            }

            chartProfileView.ChartAreas[0].AxisX.Minimum = minX - marginX;
            chartProfileView.ChartAreas[0].AxisX.Maximum = maxX + marginX;
            chartProfileView.ChartAreas[0].AxisY.Minimum = minY - marginY;
            chartProfileView.ChartAreas[0].AxisY.Maximum = maxY + marginY;

            chartProfileView.ChartAreas[0].AxisX.Title = "X坐标";
            chartProfileView.ChartAreas[0].AxisY.Title = "Y坐标";

            BeautifyProfileViewAxes(chartProfileView.ChartAreas[0]);
            chartProfileView.ChartAreas[0].RecalculateAxesScale();
            CalibrateLegendSize(chartProfileView);
            chartProfileView.Invalidate();
        }

        // 4. 更新右侧曲线图 (原版逻辑)
        private void UpdateRightPanelCharts()
        {
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();

            // 图例只初始化一次
            if (chartResistivity.Legends[ResistivityLegendName] == null)
                InitChartLegend(chartResistivity, ResistivityLegendName);
            if (chartPhase.Legends[PhaseLegendName] == null)
                InitChartLegend(chartPhase, PhaseLegendName);

            string seriesName = tabControl2.SelectedTab == tabPageDisplayTE ? "TE模式" : "TM模式";
            string resField = tabControl2.SelectedTab == tabPageDisplayTE ? "视电阻率_TE" : "视电阻率_TM";
            string phaseField = tabControl2.SelectedTab == tabPageDisplayTE ? "相位_TE" : "相位_TM";

            Series sRes = chartResistivity.Series.Add("视电阻率");
            sRes.ChartType = SeriesChartType.Spline;
            sRes.MarkerStyle = MarkerStyle.Circle;
            sRes.MarkerSize = 5;
            sRes.BorderWidth =  2;
            sRes.Legend = ResistivityLegendName;
            sRes.LegendText = $"{seriesName} 视电阻率";

            Series sPhase = chartPhase.Series.Add("相位");
            sPhase.ChartType = SeriesChartType.Spline;
            sPhase.MarkerStyle = MarkerStyle.Circle;
            sPhase.MarkerSize = 5;
            sPhase.BorderWidth = 2;
            sPhase.Legend = PhaseLegendName;
            sPhase.LegendText = $"{seriesName} 相位";

            if (m_CurrentLineData != null && !string.IsNullOrEmpty(m_CurrentSelectedStationName))
            {
                DataView dv = new DataView(m_CurrentLineData);
                dv.RowFilter = $"测点编号 = '{m_CurrentSelectedStationName}'";
                dv.Sort = "周期 ASC";
                foreach (DataRowView row in dv)
                {
                    if (row["周期"] == DBNull.Value) continue;
                    double T = Convert.ToDouble(row["周期"]);

                    if (row[resField] != DBNull.Value)
                        sRes.Points.AddXY(T, Convert.ToDouble(row[resField]));

                    if (row[phaseField] != DBNull.Value)
                        sPhase.Points.AddXY(T, Convert.ToDouble(row[phaseField]));
                }
            }

            // ================== 关键：这里什么都不设置！==================
            // 不要写 Position
            // 不要写 InnerPlotPosition
            // 不要写 Alignment、Margin 之类乱七八糟的东西
            // 让 MS Chart 自己决定怎么画！

            chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = true;
            chartResistivity.ChartAreas[0].AxisY.IsLogarithmic = true;
            chartPhase.ChartAreas[0].AxisX.IsLogarithmic = true;

            chartResistivity.ChartAreas[0].AxisX.Title = "周期(s)";
            chartResistivity.ChartAreas[0].AxisY.Title = "视电阻率(Ω·m)";
            chartPhase.ChartAreas[0].AxisX.Title = "周期(s)";
            chartPhase.ChartAreas[0].AxisY.Title = "相位(°)";

            // 只做美化，不动布局
            BeautifyChartAxes(chartResistivity.ChartAreas[0]);
            BeautifyChartAxes(chartPhase.ChartAreas[0]);

            CalibrateLegendSize(chartResistivity);
            CalibrateLegendSize(chartPhase);

            // 最后强制刷新一次
            chartResistivity.Invalidate();
            chartPhase.Invalidate();
        }


        private void BeautifyProfileViewAxes(ChartArea area)
        {
            if (area == null) return;
            area.AxisX.LabelStyle.Format = "F0";
            area.AxisY.LabelStyle.Format = "F0";
            area.AxisX.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisY.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisX.TitleFont = new Font("微软雅黑", 9f, FontStyle.Bold);
            area.AxisY.TitleFont = new Font("微软雅黑", 9f, FontStyle.Bold);

            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisX.MinorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.MinorGrid.LineColor = Color.Gainsboro;
        }

        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;
            // 原版样式：斜体标签
            area.AxisX.LabelStyle.Format = "0.###";
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.LabelStyle.IsStaggered = true;
            area.AxisX.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisX.TitleFont = new Font("微软雅黑", 10f, FontStyle.Bold);

            area.AxisY.LabelStyle.Format = "0.##";
            area.AxisY.LabelStyle.Font = new Font("Arial", 9f);
            area.AxisY.TitleFont = new Font("微软雅黑", 10f, FontStyle.Bold);

            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisX.MinorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.MinorGrid.LineColor = Color.Gainsboro;
        }

        // ====================================================================================
        // 图层与数据加载 (保持不变)
        // ====================================================================================
        private void LoadLayersFromMap()
        {
            m_allPointLayers.Clear(); m_allObjectLayers.Clear(); cmbStationLayer.Items.Clear();
            Map electroMap = FindMapByName("电法数据");
            if (electroMap == null) return;
            for (int i = 0; i < electroMap.LayerCount; i++) ProcessLayer(electroMap.get_Layer(i));
            cmbStationLayer.DisplayMember = "Name";
            if (cmbStationLayer.Items.Count > 0) cmbStationLayer.Enabled = true;
        }

        private void ProcessLayer(MapLayer layer)
        {
            if (layer == null) return;
            if (layer is GroupLayer gl) { for (int i = 0; i < gl.Count; i++) ProcessLayer(gl.get_Item(i)); }
            else if (layer is VectorLayer vl && vl.GeometryType == GeomType.Pnt && layer.Name.Contains("测点"))
            {
                m_allPointLayers.Add(layer);
                cmbStationLayer.Items.Add(layer);
            }
            else if (layer is ObjectLayer ol && layer.Name.Contains("测深数据"))
            {
                m_allObjectLayers.Add(layer);
            }
        }

        private Map FindMapByName(string name)
        {
            if (_hook?.Document == null) return null;
            Maps maps = _hook.Document.GetMaps();
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps.GetMap(i).Name == name) return maps.GetMap(i);
            }
            return null;
        }

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

            try
            {
                m_SelectedStationLayer = selectedLayer.GetData() as SFeatureCls;
                if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen()) return;
                string tableName = selectedLayer.Name.Replace("测点", "测深数据");
                MapLayer tableLayer = m_allObjectLayers.FirstOrDefault(l => l.Name == tableName);
                if (tableLayer == null) return;
                m_SelectedSoundingTable = tableLayer.GetData() as ObjectCls;
                if (m_SelectedSoundingTable == null) return;
                cmbLineName.Enabled = true;
                FillLineComboBox();
                cmbLineName.Text = "请选择测线...";
            }
            catch (Exception ex) { MessageBox.Show($"加载错误: {ex.Message}"); }
        }

        private void FillLineComboBox()
        {
            cmbLineName.Items.Clear();
            if (m_SelectedStationLayer == null) return;
            RecordSet rs = m_SelectedStationLayer.Select(null);
            if (rs == null) return;
            HashSet<string> lines = new HashSet<string>();
            rs.MoveFirst();
            string lineField = "测线号";
            if (m_SelectedStationLayer.Fields.IndexOf(lineField) < 0) lineField = "线号";
            for (int i = 0; i < rs.Count; i++)
            {
                string val = rs.Att[lineField]?.ToString();
                if (!string.IsNullOrWhiteSpace(val)) lines.Add(val);
                rs.MoveNext();
            }
            try { Marshal.ReleaseComObject(rs); } catch { }
            cmbLineName.Items.AddRange(lines.OrderBy(s => s).ToArray());
        }

        private void cmbLineName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllDisplays();
            if (cmbLineName.SelectedItem == null) return;
            string lineName = cmbLineName.SelectedItem.ToString();
            this.Cursor = Cursors.WaitCursor;
            try
            {
                m_CurrentLineStations = QueryStationsForLine(lineName);
                m_CurrentLineData = QuerySoundingDataForLine(lineName);
                UpdateProfileView();
                UpdateDataGrids();
                if (m_CurrentLineStations.Count > 0) SelectStationAndRefreshCharts(m_CurrentLineStations[0].StationName);
            }
            catch (Exception ex) { MessageBox.Show($"加载数据失败: {ex.Message}"); }
            finally { this.Cursor = Cursors.Default; }
        }

        private List<StationInfo> QueryStationsForLine(string lineName)
        {
            var stations = new List<StationInfo>();
            if (m_SelectedStationLayer == null) return stations;
            string lineField = "测线号";
            if (m_SelectedStationLayer.Fields.IndexOf(lineField) < 0) lineField = "线号";
            QueryDef query = new QueryDef { Filter = $"{lineField} = '{lineName}'" };
            RecordSet rs = m_SelectedStationLayer.Select(query);
            if (rs == null) return stations;
            rs.MoveFirst();
            string stationField = "测点号";
            if (m_SelectedStationLayer.Fields.IndexOf(stationField) < 0) stationField = "点号";
            for (int i = 0; i < rs.Count; i++)
            {
                if (rs.Geometry is GeoPoints geomPoints && geomPoints.Count > 0)
                {
                    string name = rs.Att[stationField]?.ToString();
                    if (!string.IsNullOrEmpty(name)) stations.Add(new StationInfo { StationName = name, X = geomPoints.GetItem(0).X, Y = geomPoints.GetItem(0).Y });
                }
                rs.MoveNext();
            }
            try { Marshal.ReleaseComObject(rs); } catch { }
            return stations.OrderBy(s => s.X).ToList();
        }

        private DataTable QuerySoundingDataForLine(string lineName)
        {
            DataTable dt = new DataTable();
            if (m_SelectedSoundingTable == null) return dt;
            QueryDef query = new QueryDef { Filter = $"测线编号 = '{lineName}'", SubFields2 = "*" };
            RecordSet rs = m_SelectedSoundingTable.Select(query);
            if (rs == null) return dt;
            Fields fields = rs.Fields;
            for (int i = 0; i < fields.Count; i++) dt.Columns.Add(fields[i].FieldName);
            rs.MoveFirst();
            for (int i = 0; i < rs.Count; i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < fields.Count; j++) dr[j] = rs.Att.GetValue(j);
                dt.Rows.Add(dr);
                rs.MoveNext();
            }
            try { Marshal.ReleaseComObject(rs); } catch { }
            return dt;
        }

        private void chartProfileView_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = chartProfileView.HitTest(e.X, e.Y);
            if (hit.ChartElementType == ChartElementType.DataPoint)
            {
                DataPoint dp = (DataPoint)hit.Object;
                string name = dp.Tag?.ToString();
                SelectStationAndRefreshCharts(name);
            }
        }

        private void SelectStationAndRefreshCharts(string stationName)
        {
            if (string.IsNullOrEmpty(stationName)) return;
            m_CurrentSelectedStationName = stationName;
            if (chartProfileView.Series.Count > 0)
            {
                foreach (var p in chartProfileView.Series[0].Points)
                    p.Color = (p.Tag?.ToString() == stationName) ? Color.Red : Color.Blue;
            }
            UpdateRightPanelCharts();
        }

        private void UpdateDataGrids()
        {
            if (m_CurrentLineData == null) return;
            try
            {
                DataView dv = new DataView(m_CurrentLineData);
                gridTE.DataSource = dv.ToTable(false, "测点编号", "周期", "视电阻率_TE", "相位_TE");
                gridTM.DataSource = dv.ToTable(false, "测点编号", "周期", "视电阻率_TM", "相位_TM");
            }
            catch { }
        }

        // ====================================================================================
        // 计算逻辑 (异步 + 进度条)
        // ====================================================================================
        private int GetIWD_2D()
        {
            if (rbInversionTETM.Checked) return 0;
            else if (rbInversionTE.Checked) return 1;
            else if (rbInversionTM.Checked) return 2;
            else return 0;
        }

        private async void btnCalculate_Click(object sender, EventArgs e)
        {
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("请先选择测线并加载数据！", "提示");
                return;
            }

            int its = (int)nudIterationCount.Value;
            int iwd = GetIWD_2D();
            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string algorithmDir = Path.Combine(pluginDir, "Algorithm", "MT2di");
            string exePath = Path.Combine(algorithmDir, "a.exe");

            if (!File.Exists(exePath))
            {
                MessageBox.Show($"找不到计算程序: {exePath}");
                return;
            }

            string workspaceName = Guid.NewGuid().ToString("N").Substring(0, 6);
            m_CurrentWorkspacePath = Path.Combine(pluginDir, workspaceName);
            if (!Directory.Exists(m_CurrentWorkspacePath)) Directory.CreateDirectory(m_CurrentWorkspacePath);
            string tempInputFile = Path.Combine(pluginDir, "input.dat");

            // 生成输入文件
            try
            {
                Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);
                using (StreamWriter writer = new StreamWriter(tempInputFile))
                {
                    writer.WriteLine("PROFILE STATION COORX COORY FREQ RXY PXY RYX PYX");
                    foreach (DataRow row in m_CurrentLineData.Rows)
                    {
                        string lineName = row["测线编号"].ToString();
                        string stationName = row["测点编号"].ToString();
                        if (!stationCoords.TryGetValue(stationName, out StationInfo station)) continue;
                        double period = Convert.ToDouble(row["周期"]);
                        double freq = (period == 0) ? 1e-9 : 1.0 / period;
                        double rxy = row["视电阻率_TE"] != DBNull.Value ? Convert.ToDouble(row["视电阻率_TE"]) : 0;
                        double pxy = row["相位_TE"] != DBNull.Value ? Convert.ToDouble(row["相位_TE"]) : 0;
                        double ryx = row["视电阻率_TM"] != DBNull.Value ? Convert.ToDouble(row["视电阻率_TM"]) : 0;
                        double pyx = row["相位_TM"] != DBNull.Value ? Convert.ToDouble(row["相位_TM"]) : 0;
                        writer.WriteLine($"{lineName} {stationName} {station.X} {station.Y} {freq} {rxy} {pxy} {ryx} {pyx}");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"生成文件失败: {ex.Message}"); return; }

            // UI 锁定
            this.Cursor = Cursors.WaitCursor;
            btnCalculate.Enabled = false;
            progressBar1.Value = 0;
            if (lblRMS != null) lblRMS.Text = "实际迭代误差：--";
            lblStatus.Text = "计算中...";
            if (chartResultSection != null) chartResultSection.Series.Clear();

            m_IsCalculating = true;
            timerProgress.Start();

            // 异步执行
            bool success = await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = exePath;
                    startInfo.Arguments = $"\"{tempInputFile}\" \"{workspaceName}\" {iwd} {its}";
                    startInfo.WorkingDirectory = pluginDir;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
                catch { return false; }
            });

            timerProgress.Stop();
            m_IsCalculating = false;
            this.Cursor = Cursors.Default;
            btnCalculate.Enabled = true;
            progressBar1.Value = 100;
            try { if (File.Exists(tempInputFile)) File.Delete(tempInputFile); } catch { }

            if (success)
            {
                lblStatus.Text = "计算完成";
                string resultFile = FindBestResultFile(m_CurrentWorkspacePath);
                if (!string.IsNullOrEmpty(resultFile)) DrawInversionResultChart(resultFile);
                else MessageBox.Show("未找到结果文件 (KNOW_*)。");
            }
            else {
                lblStatus.Text = "计算失败";
                MessageBox.Show("计算程序执行失败。");
            }
            
        }

        private string FindBestResultFile(string folder)
        {
            if (!Directory.Exists(folder)) return null;
            var files = Directory.GetFiles(folder, "KNOW_*");
            if (files.Length == 0) return null;
            string bestFile = null;
            int maxIter = -1;
            foreach (var f in files)
            {
                string name = Path.GetFileName(f);
                string[] parts = name.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int iter))
                {
                    if (iter > maxIter) { maxIter = iter; bestFile = f; }
                }
            }
            return bestFile;
        }

        private void timerProgress_Tick(object sender, EventArgs e)
        {
            if (!m_IsCalculating || string.IsNullOrEmpty(m_CurrentWorkspacePath)) return;

            string recordPath = Path.Combine(m_CurrentWorkspacePath, "record");

            if (File.Exists(recordPath))
            {
                try
                {
                    // 使用 FileShare.ReadWrite 防止和 a.exe 文件锁冲突
                    using (FileStream fs = new FileStream(recordPath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // 分割字符串，处理多个空格的情况
                            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            // 格式是：Current Total RMS (例如：30 30 2.926)
                            if (parts.Length >= 3)
                            {
                                double current = double.Parse(parts[0]);
                                double total = double.Parse(parts[1]);
                                double rms = double.Parse(parts[2]); // 读取第3个数字

                                // 1. 更新进度条
                                if (total > 0)
                                {
                                    int percent = (int)((current / total) * 100);
                                    progressBar1.Value = Math.Min(percent, 100);
                                    lblStatus.Text = $"{percent}%";
                                }
                                    
                                // 2. 更新 RMS 标签 (F3 表示保留3位小数)
                                if (lblRMS != null)
                                    lblRMS.Text = $"实际迭代误差：{rms:F3}";
                            }
                            else if (parts.Length >= 2) // 兼容旧逻辑
                            {
                                double current = double.Parse(parts[0]);
                                double total = double.Parse(parts[1]);
                                if (total > 0)
                                    progressBar1.Value = Math.Min((int)((current / total) * 100), 100);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略文件被占用或格式错误的瞬间异常，等待下一次 Tick
                }
            }
        }



        // ====================================================================================
        // 【结果图绘制】完全照搬 MT1di 的实现 (严格取整 + Padding)
        // ====================================================================================
        private void DrawInversionResultChart(string filePath)
        {
            if (chartResultSection == null) return;

            // 1. 读取数据
            List<ResultPoint> points = new List<ResultPoint>();
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 &&
                        double.TryParse(parts[0], out double d) &&
                        double.TryParse(parts[1], out double z) &&
                        double.TryParse(parts[2], out double val))
                    {
                        points.Add(new ResultPoint { Distance = d, Depth = z, Value = val });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取结果文件失败: {ex.Message}");
                return;
            }

            if (points.Count == 0) return;

            // 2. 初始化图表
            var chart = chartResultSection;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();
            chart.Titles.Add("二维反演电阻率断面图").Font = new Font("微软雅黑", 12f, FontStyle.Bold);

            // 3. 获取原始数据范围
            double rawMinX = points.Min(p => p.Distance);
            double rawMaxX = points.Max(p => p.Distance);
            double rawMinY = points.Min(p => p.Depth);
            double rawMaxY = points.Max(p => p.Depth);

            // 计算合适的间隔 (Interval)，注意：这里只用它的 Interval 属性
            var niceX = CalculateStrictNiceScale(rawMinX, rawMaxX);
            var niceY = CalculateStrictNiceScale(rawMinY, rawMaxY);

            // 4. 配置 ChartArea
            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "距离 (m)";
            ca.AxisY.Title = "深度/高程 (m)";
            ca.AxisX.TitleAlignment = StringAlignment.Center;
            ca.AxisY.TitleAlignment = StringAlignment.Center;

            // 【关键修改 1】设置适度的留白 (5% 左右，即扩充一点点)
            double padX = niceX.Interval * 0.05;
            double padY = niceY.Interval * 0.05;

            // 【关键修改 2】坐标轴边界 = 原始边界 +/- 留白
            // 绝不使用 Floor/Ceiling 取整，防止从 0 突变到 -500
            double finalMinX = rawMinX - padX;
            double finalMaxX = rawMaxX + padX;
            double finalMinY = rawMinY - padY;
            double finalMaxY = rawMaxY + padY;

            ca.AxisX.Minimum = finalMinX;
            ca.AxisX.Maximum = finalMaxX;
            ca.AxisX.Interval = niceX.Interval; // 使用计算出的大间隔

            ca.AxisY.Minimum = finalMinY;
            ca.AxisY.Maximum = finalMaxY;
            ca.AxisY.Interval = niceY.Interval;

            // 【关键修改 3】计算偏移量，确保刻度线依然画在整数位置 (0, 500, 1000)
            // 算法：找到比 finalMin 大的第一个整数刻度，计算它与 finalMin 的距离
            ca.AxisX.IntervalOffset = (Math.Ceiling(finalMinX / niceX.Interval) * niceX.Interval) - finalMinX;
            ca.AxisY.IntervalOffset = (Math.Ceiling(finalMinY / niceY.Interval) * niceY.Interval) - finalMinY;

            // 隐藏边角的标签，防止只显示半个数字
            ca.AxisX.LabelStyle.IsEndLabelVisible = false;
            ca.AxisY.LabelStyle.IsEndLabelVisible = false;

            // 轴线贴边设置 (保持您觉得没问题的样式，不加 Crossing)
            ca.AxisY.IsReversed = false;

            // 5. 填充数据与图例
            var validPoints = points.Where(p => p.Value > 0).ToList();
            if (validPoints.Count == 0) return;

            double minLog = Math.Log10(validPoints.Min(p => p.Value));
            double maxLog = Math.Log10(validPoints.Max(p => p.Value));

            Legend legend = chart.Legends.Add("ColorScale");
            legend.Docking = Docking.Right;
            legend.Title = "lg(ρ)";
            legend.Font = new Font("Arial", 8F);

            int levels = 10;
            for (int i = 0; i <= levels; i++)
            {
                double val = minLog + (maxLog - minLog) * i / levels;
                legend.CustomItems.Add(new System.Windows.Forms.DataVisualization.Charting.LegendItem
                {
                    Name = val.ToString("F1"),
                    Color = GetColorForValue(val, minLog, maxLog),
                    MarkerStyle = MarkerStyle.Square,
                    MarkerSize = 14
                });
            }
            legend.CustomItems.Reverse();

            Series s = chart.Series.Add("Section");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.BorderWidth = 0;
            s.IsVisibleInLegend = false;

            int dynamicSize = (points.Count > 5000) ? 6 : 10;
            s.MarkerSize = dynamicSize;

            foreach (var p in validPoints)
            {
                // 剔除范围外数据
                if (p.Distance < finalMinX || p.Distance > finalMaxX || p.Depth < finalMinY || p.Depth > finalMaxY) continue;

                int idx = s.Points.AddXY(p.Distance, p.Depth);
                s.Points[idx].Color = GetColorForValue(Math.Log10(p.Value), minLog, maxLog);
                s.Points[idx].ToolTip = $"距离: {p.Distance:F0}\n深度: {p.Depth:F0}\n电阻率: {p.Value:F1}";
            }

            BeautifyResultChartAxes(ca);
        }









        // 结果图专用的坐标轴美化 (来自 MT1di)
        private void BeautifyResultChartAxes(ChartArea area)
        {
            if (area == null) return;
            area.AxisX.LabelStyle.Format = "F0";
            area.AxisY.LabelStyle.Format = "F0";
            area.AxisX.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisY.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
            area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(64, Color.Gray);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(64, Color.Gray);
            area.AxisX.MinorGrid.Enabled = false;
            area.AxisY.MinorGrid.Enabled = false;
            area.AxisX.MajorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
            area.AxisY.MajorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
            area.BorderColor = Color.Black;
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.AxisX.LineColor = Color.Black;
            area.AxisY.LineColor = Color.Black;
        }

        // --- 刻度计算辅助 ---
        private NiceScale CalculateStrictNiceScale(double min, double max)
        {
            if (min == max) return new NiceScale { Min = min - 10, Max = max + 10, Interval = 10 };
            double range = max - min;
            double roughInterval = range / 5.0;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
            double normalizedInterval = roughInterval / magnitude;
            double niceInterval;
            if (normalizedInterval < 1.5) niceInterval = 1.0;
            else if (normalizedInterval < 3.0) niceInterval = 2.0;
            else if (normalizedInterval < 7.0) niceInterval = 5.0;
            else niceInterval = 10.0;
            niceInterval *= magnitude;
            double niceMin = Math.Floor(min / niceInterval) * niceInterval;
            double niceMax = Math.Ceiling(max / niceInterval) * niceInterval;
            return new NiceScale { Min = niceMin, Max = niceMax, Interval = niceInterval };
        }

        private Color GetColorForValue(double value, double min, double max)
        {
            if (min >= max) return Color.Black;
            double normalized = (value - min) / (max - min);
            if (normalized < 0.25) return Color.FromArgb(0, (int)(255 * (normalized * 4)), 255);
            else if (normalized < 0.5) return Color.FromArgb(0, 255, (int)(255 * (1 - (normalized - 0.25) * 4)));
            else if (normalized < 0.75) return Color.FromArgb((int)(255 * ((normalized - 0.5) * 4)), 255, 0);
            else return Color.FromArgb(255, (int)(255 * (1 - (normalized - 0.75) * 4)), 0);
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void panelResultBox_Paint(object sender, PaintEventArgs e) { }
        private void rbInversionTE_CheckedChanged(object sender, EventArgs e) { }
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 切换 TE/TM 标签页时，重新绘制右侧曲线
            if (!string.IsNullOrEmpty(m_CurrentSelectedStationName))
                UpdateRightPanelCharts();
        }
    }
}