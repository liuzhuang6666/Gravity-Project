using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing; // 基础绘图
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
// MapGIS 引用
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Geometry;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;
// 解决冲突
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
namespace MapGISPlugin3
{
    public partial class Form_MT1di : Form
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
        // --- 计算状态变量 ---
        private string m_CurrentWorkspacePath;
        private bool m_IsCalculating = false;
        // --- 绘图极值缓存 ---
        private double m_MinValLog = 0;
        private double m_MaxValLog = 3;
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
        // ====================================================================================
        // 初始化与加载
        // ====================================================================================
        public Form_MT1di(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();
        }
        private void Form_MT1di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            InitDragEvent();
            InitializeChartStyles();
            // 删除了 panelLegend 相关代码
            // 删除了 picResult 相关代码（因为现在用 Chart 了）
            if (timerProgress != null) timerProgress.Tick += timerProgress_Tick;
            // 如果你想在启动时清空新图表，可以加这句：
            if (chartResultSection != null) chartResultSection.Series.Clear();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // 窗口拖动逻辑
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
        // UI 事件处理
        // ====================================================================================
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
                if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
                {
                    MessageBox.Show("无效的测点图层数据。");
                    return;
                }
                string tableName = selectedLayer.Name.Replace("测点", "测深数据");
                MapLayer tableLayer = m_allObjectLayers.FirstOrDefault(l => l.Name == tableName);
                if (tableLayer == null)
                {
                    MessageBox.Show($"未找到关联的测深数据表: {tableName}");
                    return;
                }
                m_SelectedSoundingTable = tableLayer.GetData() as ObjectCls;
                if (m_SelectedSoundingTable == null) return;
                cmbLineName.Enabled = true;
                FillLineComboBox();
                cmbLineName.Text = "请选择测线...";
            }
            catch (Exception ex) { MessageBox.Show($"加载图层数据出错: {ex.Message}"); }
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
                if (m_CurrentLineStations.Count > 0)
                    SelectStationAndRefreshCharts(m_CurrentLineStations[0].StationName);
            }
            catch (Exception ex) { MessageBox.Show($"加载测线数据失败: {ex.Message}"); }
            finally { this.Cursor = Cursors.Default; }
        }
        private void chartProfileView_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = chartProfileView.HitTest(e.X, e.Y);
            if (hit.ChartElementType == ChartElementType.DataPoint)
            {
                DataPoint dp = (DataPoint)hit.Object;
                string name = dp.Tag?.ToString();
                if (string.IsNullOrEmpty(name)) name = dp.Label;
                SelectStationAndRefreshCharts(name);
            }
        }
        // 【修复】这个函数就是你报错 CS1061 缺失的那个
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_CurrentSelectedStationName))
                UpdateRightPanelCharts();
        }
        // ====================================================================================
        // 核心计算逻辑
        // ====================================================================================
        private async void btnCalculate_Click(object sender, EventArgs e)
        {
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("请先选择测线并加载数据！");
                return;
            }
            int its = (int)nudIterationCount.Value;
            int iwd = rbInversionTE.Checked ? 0 : 1;
            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string exePath = Path.Combine(pluginDir, "Algorithm", "MT1di", "a.exe");
            if (!File.Exists(exePath)) { MessageBox.Show($"找不到计算程序: {exePath}"); return; }
            string workspaceName = Guid.NewGuid().ToString("N").Substring(0, 6);
            m_CurrentWorkspacePath = Path.Combine(pluginDir, workspaceName);
            string inputFileName = "input.dat";
            string fullInputPath = Path.Combine(pluginDir, inputFileName);
            // UI 锁定
            this.Cursor = Cursors.WaitCursor;
            btnCalculate.Enabled = false;
            progressBar1.Value = 0;
            lblStatus.Text = "计算中...";
            lblResultResError.Text = "RMS: 计算中...";
            lblResultPhaseError.Text = "";
            if (chartResultSection != null) chartResultSection.Series.Clear();
            m_IsCalculating = true;
            timerProgress.Start();
            // 异步执行
            string stdOut = "";
            bool success = await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = exePath;
                    startInfo.Arguments = $"\"{inputFileName}\" \"{workspaceName}\" {iwd} {its}";
                    startInfo.WorkingDirectory = pluginDir;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    using (Process process = Process.Start(startInfo))
                    {
                        stdOut = process.StandardOutput.ReadToEnd();
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
            if (success)
            {
                lblStatus.Text = "计算完成";
                lblResultResError.Text = "RMS: " + (string.IsNullOrWhiteSpace(stdOut) ? "--" : "查看日志");
                string resultFile = Path.Combine(m_CurrentWorkspacePath, "KNOW");
                if (File.Exists(resultFile)) DrawInversionResultChart(resultFile);
                else MessageBox.Show($"未找到结果文件 KNOW");
            }
            else
            {
                lblStatus.Text = "计算失败";
                MessageBox.Show("计算程序返回错误。");
            }
        }
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            if (!m_IsCalculating || string.IsNullOrEmpty(m_CurrentWorkspacePath)) return;
            string recordPath = Path.Combine(m_CurrentWorkspacePath, "record");
            if (File.Exists(recordPath))
            {
                try
                {
                    using (FileStream fs = new FileStream(recordPath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2)
                            {
                                double current = double.Parse(parts[0]);
                                double total = double.Parse(parts[1]);
                                if (total > 0)
                                {
                                    int percent = (int)((current / total) * 100);
                                    progressBar1.Value = Math.Min(percent, 100);
                                    lblStatus.Text = $"{percent}%";
                                    if (parts.Length > 2) lblResultResError.Text = "RMS: " + parts[2];
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
        private Color GetJetColor(double v, double vmin, double vmax)
        {
            double dv = vmax - vmin;
            if (dv == 0) return Color.Blue;
            double ratio = (v - vmin) / dv;
            double r = 0, g = 0, b = 0;
            if (ratio < 0) ratio = 0; if (ratio > 1) ratio = 1;
            if (ratio < 0.25) { r = 0; g = 4 * ratio; b = 1; }
            else if (ratio < 0.5) { r = 0; g = 1; b = 1 - 4 * (ratio - 0.25); }
            else if (ratio < 0.75) { r = 4 * (ratio - 0.5); g = 1; b = 0; }
            else { r = 1; g = 1 - 4 * (ratio - 0.75); b = 0; }
            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
        // ====================================================================================
        // 新版结果图绘制 (MS Chart)
        // ====================================================================================
        /// <summary>
        /// 颜色映射算法 (Jet Colormap)
        /// </summary>
        private Color GetColorForValue(double value, double min, double max)
        {
            if (min >= max) return Color.Black;
            // 归一化到 0-1
            double normalized = (value - min) / (max - min);
            // Jet Color Map 逻辑
            if (normalized < 0.25)
                return Color.FromArgb(0, (int)(255 * (normalized * 4)), 255);
            else if (normalized < 0.5)
                return Color.FromArgb(0, 255, (int)(255 * (1 - (normalized - 0.25) * 4)));
            else if (normalized < 0.75)
                return Color.FromArgb((int)(255 * ((normalized - 0.5) * 4)), 255, 0);
            else
                return Color.FromArgb(255, (int)(255 * (1 - (normalized - 0.75) * 4)), 0);
        }
        /// <summary>
        /// 核心绘制函数：读取文件并绘制伪断面图
        /// </summary>
        /// <summary>
        /// 核心绘制函数：读取文件并绘制伪断面图 (MS Chart 版)
        /// </summary>
        // ====================================================================================
        // 最终修复版绘图逻辑
        // ====================================================================================
        /// <summary>
        /// 核心绘制函数
        /// </summary>
        private void DrawInversionResultChart(string filePath)
        {
            // -------------------------------------------------------------
            // 1. 读取数据
            // -------------------------------------------------------------
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
            catch { return; }
            if (points.Count == 0) return;
            // -------------------------------------------------------------
            // 2. 初始化图表
            // -------------------------------------------------------------
            var chart = chartResultSection;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();
            chart.Titles.Add("MT 一维反演电阻率断面图").Font = new Font("微软雅黑", 12f, FontStyle.Bold);
            // -------------------------------------------------------------
            // 3. 计算完美的整数刻度 (修复：拒绝乱扩充)
            // -------------------------------------------------------------
            double rawMinX = points.Min(p => p.Distance);
            double rawMaxX = points.Max(p => p.Distance);
            double rawMinY = points.Min(p => p.Depth);
            double rawMaxY = points.Max(p => p.Depth);
            // 使用严格取整算法
            var niceX = CalculateStrictNiceScale(rawMinX, rawMaxX);
            var niceY = CalculateStrictNiceScale(rawMinY, rawMaxY);
            // -------------------------------------------------------------
            // 4. 强制配置坐标轴 (解决压盖 + 隐藏多余边界标签 + 刻度对齐)
            // -------------------------------------------------------------
            ChartArea ca = chart.ChartAreas.Add("ResultArea");

            ca.AxisX.Title = "距离 (m)";
            ca.AxisY.Title = "深度/高程 (m)";
            ca.AxisX.TitleAlignment = StringAlignment.Center;
            ca.AxisY.TitleAlignment = StringAlignment.Center;

            // 1. 计算留白量 (0.6 个刻度间隔)
            double padX = niceX.Interval * 0.2;
            double padY = niceY.Interval * 0.4;

            // 2. 重新计算带留白的范围 (往外扩)
            double newMinX = niceX.Min - padX;
            double newMaxX = niceX.Max + padX;
            double newMinY = niceY.Min - padY;
            double newMaxY = niceY.Max + padY;

            // 3. 填入扩充后的范围
            ca.AxisX.Minimum = newMinX;
            ca.AxisX.Maximum = newMaxX;
            ca.AxisX.Interval = niceX.Interval;

            ca.AxisY.Minimum = newMinY;
            ca.AxisY.Maximum = newMaxY;
            ca.AxisY.Interval = niceY.Interval;

            // =========================================================
            // 【关键修改】设置刻度偏移量 (IntervalOffset)
            // =========================================================
            // 原理：算出"扩充边界"到"原本完美起点"的距离，让刻度线从完美起点开始画。
            // 这样既保留了边缘空隙，又不会把边缘的乱七八糟数字画出来。

            // 计算 X 轴偏移 (取模防止超过一个间隔)
            ca.AxisX.IntervalOffset = (niceX.Min - newMinX) % niceX.Interval;

            // 计算 Y 轴偏移
            ca.AxisY.IntervalOffset = (niceY.Min - newMinY) % niceY.Interval;

            // =========================================================

            // 4. 隐藏首尾标签 (双重保险，防止正好碰上整数倍时显示出来)
            ca.AxisX.LabelStyle.IsEndLabelVisible = false;
            ca.AxisY.LabelStyle.IsEndLabelVisible = false;

            // 5. 调整轴线位置 (Crossing) - 把轴线画在扩充后的边缘
            bool isDataNegative = (rawMinY < 0 && rawMaxY <= 0);

            if (isDataNegative)
            {
                // 高程模式
                ca.AxisY.IsReversed = false;
                ca.AxisX.Crossing = newMinY; // X轴在最底端
            }
            else
            {
                // 深度模式
                ca.AxisY.IsReversed = true;
                ca.AxisX.Crossing = newMaxY; // X轴在最底端(深度最大处)
            }

            // Y轴在最左端
            ca.AxisY.Crossing = newMinX;

            // 6. 基础样式
            ca.AxisX.IsMarksNextToAxis = false;
            ca.AxisY.IsMarksNextToAxis = false;
            ca.AxisX.Enabled = AxisEnabled.True;
            ca.AxisY.Enabled = AxisEnabled.True;
            // -------------------------------------------------------------
            // 5. 填充数据与图例 (保持不变)
            // -------------------------------------------------------------
            var validPoints = points.Where(p => p.Value > 0).ToList();
            if (validPoints.Count == 0) return;
            double minLog = Math.Log10(validPoints.Min(p => p.Value));
            double maxLog = Math.Log10(validPoints.Max(p => p.Value));
            // 图例
            Legend legend = chart.Legends.Add("ColorScale");
            legend.Docking = Docking.Right;
            legend.Title = "lg(ρ/Ω·m)";
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
            // 数据点
            Series s = chart.Series.Add("Section");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.BorderWidth = 0;
            s.IsVisibleInLegend = false;
            // 动态调整点大小，防止缝隙
            // 简单粗暴逻辑：如果数据少，点大一点；数据多，点小一点
            int dynamicSize = (points.Count > 5000) ? 10 : 18;
            s.MarkerSize = dynamicSize;
            foreach (var p in validPoints)
            {
                // 严格剔除范围外数据，防止压盖
                if (p.Distance < niceX.Min || p.Distance > niceX.Max || p.Depth < niceY.Min || p.Depth > niceY.Max) continue;
                int idx = s.Points.AddXY(p.Distance, p.Depth);
                s.Points[idx].Color = GetColorForValue(Math.Log10(p.Value), minLog, maxLog);
                s.Points[idx].ToolTip = $"距离: {p.Distance:F0}\n深度: {p.Depth:F0}\n电阻率: {p.Value:F1}";
            }
            BeautifyChartAxes(ca);
        }





        private struct NiceScale { public double Min; public double Max; public double Interval; }
        /// <summary>
        /// 严格整数刻度计算，只对齐，不扩充
        /// </summary>
        private NiceScale CalculateStrictNiceScale(double min, double max)
        {
            if (min == max) return new NiceScale { Min = min - 10, Max = max + 10, Interval = 10 };
            double range = max - min;
            // 目标分段数：5段左右最舒服
            double roughInterval = range / 5.0;
            // 计算数量级 (10, 100, 1000...)
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
            double normalizedInterval = roughInterval / magnitude;
            // 归一化到 1, 2, 5
            double niceInterval;
            if (normalizedInterval < 1.5) niceInterval = 1.0;
            else if (normalizedInterval < 3.0) niceInterval = 2.0;
            else if (normalizedInterval < 7.0) niceInterval = 5.0;
            else niceInterval = 10.0;
            niceInterval *= magnitude;
            // 向内/向外取整
            // 使用 Floor/Ceiling 保证正好包住数据，或者正好在整数上
            double niceMin = Math.Floor(min / niceInterval) * niceInterval;
            double niceMax = Math.Ceiling(max / niceInterval) * niceInterval;
            return new NiceScale { Min = niceMin, Max = niceMax, Interval = niceInterval };
        }
        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;
            // 强制整数格式
            area.AxisX.LabelStyle.Format = "F0";
            area.AxisY.LabelStyle.Format = "F0";
            area.AxisX.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisY.LabelStyle.Font = new Font("Arial", 8f);
            // 禁用自动调整，防止标签乱跳
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
            area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
            // 灰色网格线
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(64, Color.Gray);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(64, Color.Gray);
            // 禁用小网格
            area.AxisX.MinorGrid.Enabled = false;
            area.AxisY.MinorGrid.Enabled = false;
            // 刻度线朝外 (专业绘图风格)
            area.AxisX.MajorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
            area.AxisY.MajorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
            // 黑色实线边框
            area.BorderColor = Color.Black;
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.AxisX.LineColor = Color.Black;
            area.AxisY.LineColor = Color.Black;
        }
        // ====================================================================================
        // 辅助函数
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
            else if (layer is VectorLayer vl && vl.GeometryType == GeomType.Pnt && layer.Name.Contains("测点")) { m_allPointLayers.Add(layer); cmbStationLayer.Items.Add(layer); }
            else if (layer is ObjectLayer ol && layer.Name.Contains("测深数据")) { m_allObjectLayers.Add(layer); }
        }
        private Map FindMapByName(string name)
        {
            if (_hook?.Document == null) return null;
            Maps maps = _hook.Document.GetMaps();
            for (int i = 0; i < maps.Count; i++) if (maps.GetMap(i).Name == name) return maps.GetMap(i);
            return null;
        }
        private void FillLineComboBox()
        {
            cmbLineName.Items.Clear();
            if (m_SelectedStationLayer == null) return;
            RecordSet rs = m_SelectedStationLayer.Select(null);
            if (rs == null) return;
            HashSet<string> lines = new HashSet<string>();
            rs.MoveFirst();
            for (int i = 0; i < rs.Count; i++) { string val = rs.Att["测线号"]?.ToString(); if (!string.IsNullOrWhiteSpace(val)) lines.Add(val); rs.MoveNext(); }
            try { Marshal.ReleaseComObject(rs); } catch { }
            cmbLineName.Items.AddRange(lines.OrderBy(s => s).ToArray());
            if (cmbLineName.Items.Count > 0) cmbLineName.SelectedIndex = 0;
        }
        private List<StationInfo> QueryStationsForLine(string lineName)
        {
            var list = new List<StationInfo>();
            if (m_SelectedStationLayer == null) return list;
            QueryDef q = new QueryDef { Filter = $"测线号='{lineName}'" };
            RecordSet rs = m_SelectedStationLayer.Select(q);
            if (rs == null) return list;
            rs.MoveFirst();
            for (int i = 0; i < rs.Count; i++)
            {
                if (rs.Geometry is GeoPoints pnts && pnts.Count > 0)
                {
                    string name = rs.Att["测点号"]?.ToString();
                    if (!string.IsNullOrEmpty(name)) list.Add(new StationInfo { StationName = name, X = pnts.GetItem(0).X, Y = pnts.GetItem(0).Y });
                }
                rs.MoveNext();
            }
            try { Marshal.ReleaseComObject(rs); } catch { }
            return list.OrderBy(s => s.X).ToList();
        }
        private DataTable QuerySoundingDataForLine(string lineName)
        {
            DataTable dt = new DataTable();
            if (m_SelectedSoundingTable == null) return dt;
            QueryDef q = new QueryDef { Filter = $"测线编号='{lineName}'", SubFields2 = "*" };
            RecordSet rs = m_SelectedSoundingTable.Select(q);
            if (rs == null) return dt;
            Fields flds = rs.Fields;
            for (int i = 0; i < flds.Count; i++) dt.Columns.Add(flds[i].FieldName);
            rs.MoveFirst();
            for (int i = 0; i < rs.Count; i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < flds.Count; j++) dr[j] = rs.Att.GetValue(j);
                dt.Rows.Add(dr); rs.MoveNext();
            }
            try { Marshal.ReleaseComObject(rs); } catch { }
            return dt;
        }
        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;
            chartProfileView.Series.Clear();
            if (chartProfileView.Legends[ProfileLegendName] == null) InitChartLegend(chartProfileView, ProfileLegendName);
            // 恢复图表布局
            if (chartProfileView.ChartAreas.Count > 0)
            {
                var chartArea = chartProfileView.ChartAreas[0];
                chartArea.Position = new ElementPosition(8, 5, 85, 90);
                chartArea.InnerPlotPosition = new ElementPosition(10, 10, 85, 85);
            }
            Series s = chartProfileView.Series.Add("测点");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 8;
            s.Color = Color.Blue;
            s.Legend = ProfileLegendName;
            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0) return;
            double minX = m_CurrentLineStations.Min(x => x.X), maxX = m_CurrentLineStations.Max(x => x.X);
            double minY = m_CurrentLineStations.Min(x => x.Y), maxY = m_CurrentLineStations.Max(x => x.Y);
            double rangeX = maxX - minX, rangeY = maxY - minY;
            double marginX = (rangeX == 0) ? 100 : rangeX * 0.1;
            double marginY = (rangeY == 0) ? 100 : rangeY * 0.1;
            foreach (var sta in m_CurrentLineStations)
            {
                int idx = s.Points.AddXY(sta.X, sta.Y);
                s.Points[idx].Label = sta.StationName;
                s.Points[idx].Tag = sta.StationName;
            }
            chartProfileView.ChartAreas[0].AxisX.Minimum = minX - marginX;
            chartProfileView.ChartAreas[0].AxisX.Maximum = maxX + marginX;
            chartProfileView.ChartAreas[0].AxisY.Minimum = minY - marginY;
            chartProfileView.ChartAreas[0].AxisY.Maximum = maxY + marginY;
            BeautifyProfileViewAxes(chartProfileView.ChartAreas[0]);
            chartProfileView.ChartAreas[0].RecalculateAxesScale();
            CalibrateLegendSize(chartProfileView);
        }
        private void UpdateDataGrids()
        {
            if (m_CurrentLineData == null) return;
            DataView dvTE = new DataView(m_CurrentLineData);
            gridTE.DataSource = dvTE.ToTable(false, "测点编号", "周期", "视电阻率_TE", "相位_TE");
            DataView dvTM = new DataView(m_CurrentLineData);
            gridTM.DataSource = dvTM.ToTable(false, "测点编号", "周期", "视电阻率_TM", "相位_TM");
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
        private void UpdateRightPanelCharts()
        {
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();
            if (chartResistivity.Legends[ResistivityLegendName] == null) InitChartLegend(chartResistivity, ResistivityLegendName);
            if (chartPhase.Legends[PhaseLegendName] == null) InitChartLegend(chartPhase, PhaseLegendName);
            string seriesName = tabControl2.SelectedTab == tabPageDisplayTE ? "TE模式" : "TM模式";
            string resField = tabControl2.SelectedTab == tabPageDisplayTE ? "视电阻率_TE" : "视电阻率_TM";
            string phaseField = tabControl2.SelectedTab == tabPageDisplayTE ? "相位_TE" : "相位_TM";
            Series sRes = chartResistivity.Series.Add("视电阻率");
            sRes.ChartType = SeriesChartType.Spline;
            sRes.MarkerStyle = MarkerStyle.Circle;
            sRes.MarkerSize = 5;
            sRes.BorderWidth = 2;
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
                foreach (DataRowView row in dv)
                {
                    if (row["周期"] == DBNull.Value) continue;
                    double T = Convert.ToDouble(row["周期"]);
                    if (row[resField] != DBNull.Value) sRes.Points.AddXY(T, Convert.ToDouble(row[resField]));
                    if (row[phaseField] != DBNull.Value) sPhase.Points.AddXY(T, Convert.ToDouble(row[phaseField]));
                }
            }
            // 对数坐标
            chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = true;
            chartResistivity.ChartAreas[0].AxisY.IsLogarithmic = true;
            chartPhase.ChartAreas[0].AxisX.IsLogarithmic = true;
            chartResistivity.ChartAreas[0].AxisX.Title = "周期(s)";
            chartResistivity.ChartAreas[0].AxisY.Title = "视电阻率";
            chartPhase.ChartAreas[0].AxisX.Title = "周期(s)";
            chartPhase.ChartAreas[0].AxisY.Title = "相位";
            BeautifyChartAxes(chartResistivity.ChartAreas[0]);
            BeautifyChartAxes(chartPhase.ChartAreas[0]);
            CalibrateLegendSize(chartResistivity);
            CalibrateLegendSize(chartPhase);
        }
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
                Position = new ElementPosition(65, 3, 30, 10)
            };
            chart.Legends.Add(legend);
        }
        // 【修复】你之前报错 InitializeChartStyles 不存在，其实是这里，我加上了
        private void InitializeChartStyles()
        {
            ClearDefaultSeries();
            InitChartLegend(chartProfileView, ProfileLegendName);
            InitChartLegend(chartResistivity, ResistivityLegendName);
            InitChartLegend(chartPhase, PhaseLegendName);
            chartProfileView.BackColor = Color.White;
            chartResistivity.BackColor = Color.White;
            chartPhase.BackColor = Color.White;
        }
        // 【修复】这个函数是我上次不小心漏掉的
        private void ClearDefaultSeries()
        {
            if (chartProfileView.Series.Count > 0) chartProfileView.Series.Clear();
            if (chartResistivity.Series.Count > 0) chartResistivity.Series.Clear();
            if (chartPhase.Series.Count > 0) chartPhase.Series.Clear();
        }
        private void CalibrateLegendSize(Chart chart)
        {
            if (chart.Legends.Count == 0 || chart.Series.Count == 0) return;
            Legend legend = chart.Legends[0];
            int actualItemCount = chart.Series.Count;
            float totalHeightPercent = (legend.Font.Height + 4) / (float)chart.Height * 100 * actualItemCount + 3;
            if (chart == chartResistivity) legend.Position = new ElementPosition(50, 2, 45, Math.Min(totalHeightPercent, 15));
            else if (chart == chartProfileView) legend.Position = new ElementPosition(70, 2, 25, Math.Min(totalHeightPercent, 8));
            else legend.Position = new ElementPosition(60, 2, 35, Math.Min(totalHeightPercent, 12));
        }
        private void BeautifyProfileViewAxes(ChartArea area)
        {
            if (area == null) return;
            area.AxisX.LabelStyle.Format = "F0";
            area.AxisX.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisX.TitleFont = new Font("微软雅黑", 9f, FontStyle.Bold);
            area.AxisY.LabelStyle.Format = "F0";
            area.AxisY.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisY.TitleFont = new Font("微软雅黑", 9f, FontStyle.Bold);
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true; area.AxisX.MinorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MinorGrid.Enabled = true; area.AxisY.MinorGrid.LineColor = Color.Gainsboro;
        }

        private void ConfigureAutoScaleAxes(ChartArea chartArea)
        {
            if (chartArea == null) return;
            chartArea.AxisX.IsStartedFromZero = false;
            chartArea.AxisX.IsMarginVisible = true;
            chartArea.AxisY.IsStartedFromZero = false;
            chartArea.AxisY.IsMarginVisible = true;
            chartArea.RecalculateAxesScale();
        }
        private void ClearAllDisplays()
        {
            chartProfileView.Series.Clear(); chartResistivity.Series.Clear(); chartPhase.Series.Clear();
            if (chartResultSection != null) chartResultSection.Series.Clear();
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;
        }
        // 空事件 (防止设计器报错)
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void panelResultBox_Paint(object sender, PaintEventArgs e) { }
        private void progressBar2_Click(object sender, EventArgs e) { }
    }
}
