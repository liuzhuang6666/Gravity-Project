using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
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

        // --- 线程与计算控制 ---
        private CancellationTokenSource _progressCancellationTokenSource;
        private List<InversionResultPoint> m_LastInversionResults = null; // 缓存最后一次计算结果

        // --- 内部数据结构 ---
        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private class InversionResultPoint
        {
            public double Distance { get; set; }
            public double Depth { get; set; }
            public double Value { get; set; } // 电阻率
        }

        private class CalculationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public List<InversionResultPoint> Results { get; set; } = new List<InversionResultPoint>();
            public string WorkspacePath { get; set; }
            public string ActualError { get; set; }
        }

        private struct NiceScale { public double Min; public double Max; public double Interval; }

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

            // 绑定最大深度改变事件
            this.nudMaxDepth.ValueChanged += new System.EventHandler(this.nudMaxDepth_ValueChanged);
        }

        private void Form_MT1di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            InitDragEvent();
            InitializeChartStyles();

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
        // 核心计算逻辑 (Async/Await)
        // ====================================================================================

        private async void btnCalculate_Click(object sender, EventArgs e)
        {
            // 1. 基础校验
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("请先选择测线并加载数据！", "提示");
                return;
            }

            // 2. 准备参数
            int its = (int)nudIterationCount.Value;
            int iwd = rbInversionTE.Checked ? 0 : 1; // 0: TE, 1: TM

            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempRunDir = Path.Combine(Path.GetTempPath(), "MT1di_run_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            string workspaceName = "result";
            string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);

            // 3. UI 状态更新
            btnCalculate.Enabled = false;
            progressBar1.Value = 0;
            txtActualError.Text = "";
            // 4. 数据准备
            DataTable lineDataCopy = m_CurrentLineData.Copy();
            Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);

            // 5. 启动进度监控
            _progressCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _progressCancellationTokenSource.Token;

            var progressTask = MonitorProgressAsync(fullWorkspacePath, its, cancellationToken);

            try
            {
                // 6. 后台执行计算
                var calcResult = await Task.Run(() =>
                {
                    return RunMTCalculationAsync(pluginDir, tempRunDir, workspaceName, lineDataCopy, stationCoords, iwd, its);
                });

                // 7. 停止监控
                _progressCancellationTokenSource.Cancel();
                try { await progressTask; } catch (OperationCanceledException) { }

                // 8. 处理结果
                if (calcResult.Success)
                {
                    progressBar1.Value = 100;
                    if (!string.IsNullOrEmpty(calcResult.ActualError))
                    {
                        txtActualError.Text = calcResult.ActualError;
                    }

                    m_LastInversionResults = calcResult.Results;
                    DisplayInversionResults(calcResult.Results);

                    MessageBox.Show($"计算完成！\n结果保存在: {calcResult.WorkspacePath}", "成功");
                }
                else
                {
                    progressBar1.Value = 0;
                    DisplayInversionResults(new List<InversionResultPoint>());
                    MessageBox.Show(calcResult.ErrorMessage, "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生未处理的异常: {ex.Message}", "错误");
            }
            finally
            {
                _progressCancellationTokenSource?.Dispose();
                _progressCancellationTokenSource = null;
                btnCalculate.Enabled = true;
            }
        }

        private CalculationResult RunMTCalculationAsync(string pluginDir, string tempRunDir, string workspaceName, DataTable lineData, Dictionary<string, StationInfo> stationCoords, int iwd, int its)
        {
            var result = new CalculationResult();
            string exePath = Path.Combine(pluginDir, "Algorithm", "MT1di", "a.exe");

            if (!File.Exists(exePath))
            {
                result.Success = false;
                result.ErrorMessage = $"找不到计算程序: {exePath}";
                return result;
            }

            try
            {
                Directory.CreateDirectory(tempRunDir);
                string inputFileName = "input.dat";
                string fullInputPath = Path.Combine(tempRunDir, inputFileName);

                using (StreamWriter writer = new StreamWriter(fullInputPath))
                {
                    foreach (DataRow row in lineData.Rows)
                    {
                        string stationName = row["测点编号"].ToString();
                        if (!stationCoords.TryGetValue(stationName, out StationInfo station)) continue;

                        double period = GetDoubleFromRow(row, "周期", 0);
                        if (period <= 0) period = 1e-9;
                        double freq = 1.0 / period;

                        double r_te = GetDoubleFromRow(row, "视电阻率_TE", 0);
                        double p_te = GetDoubleFromRow(row, "相位_TE", 0);
                        double r_tm = GetDoubleFromRow(row, "视电阻率_TM", 0);
                        double p_tm = GetDoubleFromRow(row, "相位_TM", 0);
                        double z_coord = 0;

                        writer.WriteLine($"{stationName} {station.X} {station.Y} {z_coord} {freq:F6} {r_te:F4} {p_te:F4} {r_tm:F4} {p_tm:F4}");
                    }
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{inputFileName}\" \"{workspaceName}\" {iwd} {its}",
                    WorkingDirectory = tempRunDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string stdErr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);
                        result.WorkspacePath = fullWorkspacePath;
                        string resultFile = Path.Combine(fullWorkspacePath, "KNOW");
                        if (!File.Exists(resultFile))
                        {
                            var files = Directory.GetFiles(fullWorkspacePath, "KNOW*");
                            if (files.Length > 0) resultFile = files[0];
                        }

                        if (File.Exists(resultFile))
                        {
                            result.Results = ParseKnowFile(resultFile);
                            result.Success = true;

                            string recordFile = Path.Combine(fullWorkspacePath, "record");
                            if (File.Exists(recordFile))
                            {
                                try
                                {
                                    var lines = File.ReadAllLines(recordFile);
                                    if (lines.Length > 0)
                                    {
                                        string lastLine = lines.Last();
                                        var parts = lastLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (parts.Length > 2) result.ActualError = parts[2];
                                    }
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            result.Success = true;
                            result.ErrorMessage = "未生成结果文件(KNOW)。";
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = $"计算程序返回错误 (ExitCode: {process.ExitCode})\n{stdErr}";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"运行异常: {ex.Message}";
            }
            return result;
        }

        private async Task MonitorProgressAsync(string workspacePath, int totalIterations, CancellationToken cancellationToken)
        {
            string recordFile = Path.Combine(workspacePath, "record");
            await Task.Delay(500, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (File.Exists(recordFile))
                    {
                        using (var fs = new FileStream(recordFile, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var sr = new StreamReader(fs))
                        {
                            string content = await sr.ReadToEndAsync();
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                if (lines.Length > 0)
                                {
                                    string lastLine = lines.Last();
                                    var parts = lastLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length >= 2 && double.TryParse(parts[0], out double current) && double.TryParse(parts[1], out double total))
                                    {
                                        int percent = (int)((current / (total > 0 ? total : 1)) * 100);
                                        string rms = parts.Length > 2 ? parts[2] : "";
                                        this.BeginInvoke(new Action(() => {
                                            progressBar1.Value = Math.Min(percent, 100);
                                            if (!string.IsNullOrEmpty(rms))
                                            {
                                                txtActualError.Text = rms;
                                            }
                                        }));
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
                try { await Task.Delay(200, cancellationToken); } catch { break; }
            }
        }

        private List<InversionResultPoint> ParseKnowFile(string filePath)
        {
            var points = new List<InversionResultPoint>();
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    // 格式通常为: 距离(X)  高程/深度(Z)  电阻率(Rho)
                    if (parts.Length >= 3 &&
                        double.TryParse(parts[0], out double d) &&
                        double.TryParse(parts[1], out double z) &&
                        double.TryParse(parts[2], out double val))
                    {
                        // ================== 修正 ==================
                        // 直接读取原始值 (Z)，不取绝对值，不加负号
                        // 如果数据是正数(山上)，这里就是正数；如果是负数(地下)，这里就是负数
                        points.Add(new InversionResultPoint { Distance = d, Depth = z, Value = val });
                        // ==========================================
                    }
                }
            }
            catch { }
            return points;
        }
        // ====================================================================================
        // 结果展示 (自定义深度)
        // ====================================================================================

        private void nudMaxDepth_ValueChanged(object sender, EventArgs e)
        {
            if (m_LastInversionResults != null && m_LastInversionResults.Count > 0)
                DisplayInversionResults(m_LastInversionResults);
        }

        private void DisplayInversionResults(List<InversionResultPoint> points)
        {
            var chart = chartResultSection;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();

            if (points == null || points.Count == 0) return;

            // 1. 获取用户设定的“显示深度下限”（输入正数，转化为负坐标）
            // 例如输入 2000，代表想看到 -2000 米深的地方
            double deepLimitAbs = (double)nudMaxDepth.Value;
            if (deepLimitAbs <= 0) deepLimitAbs = 2000;
            double minZ_Limit = -deepLimitAbs;

            // 2. 筛选数据
            // 逻辑：保留 Value > 0 且 深度 >= -2000 的所有点
            // (这样既保留了 -2000 到 0 的地下数据，也保留了 > 0 的地形数据)
            var validPoints = points
                .Where(p => p.Value > 0 && p.Depth >= minZ_Limit)
                .ToList();

            if (validPoints.Count == 0)
            {
                chart.Titles.Add("当前范围内无有效数据");
                return;
            }

            chart.Titles.Add($"MT 反演电阻率断面 ( > {minZ_Limit}m )").Font = new Font("微软雅黑", 12f, FontStyle.Bold);

            // 3. 动态计算数据的真实边界
            double dataMinX = validPoints.Min(p => p.Distance);
            double dataMaxX = validPoints.Max(p => p.Distance);

            // Y轴上限：取数据中的最高点（比如山顶 +300m），如果没有正数则取0
            double maxDataZ = validPoints.Max(p => p.Depth);
            double dataMaxY = maxDataZ > 0 ? maxDataZ : 0;

            // 4. 计算美观刻度
            var niceX = CalculateStrictNiceScale(dataMinX, dataMaxX);
            // Y轴刻度范围：从 用户设定的最深处 到 数据的最高处
            var niceY = CalculateStrictNiceScale(minZ_Limit, dataMaxY);

            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "距离 (m)";
            ca.AxisY.Title = "高程 (m)"; // 改名为高程更准确

            // 设置 X 轴
            ca.AxisX.Minimum = niceX.Min - (niceX.Interval * 0.2);
            ca.AxisX.Maximum = niceX.Max + (niceX.Interval * 0.2);
            ca.AxisX.Interval = niceX.Interval;

            // 设置 Y 轴
            ca.AxisY.Minimum = minZ_Limit; // 固定底部，例如 -2000
                                           // 顶部稍微留白，让山顶不顶格
            double topMargin = (dataMaxY - minZ_Limit) * 0.05;
            ca.AxisY.Maximum = dataMaxY + topMargin;
            ca.AxisY.Interval = niceY.Interval;

            // 关键设置：正常坐标系（上正下负），不用反转
            ca.AxisY.IsReversed = false;

            // 强制显示 0 刻度线（如果它在范围内）
            ca.AxisY.Crossing = 0;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            // 如果想强调 0 线（地表大概位置），可以加一条粗线
            StripLine zeroLine = new StripLine();
            zeroLine.Interval = 0;
            zeroLine.StripWidth = 0; // 线宽由 BorderWidth 控制
            zeroLine.IntervalOffset = 0;
            zeroLine.BorderColor = Color.Black;
            zeroLine.BorderWidth = 1;
            zeroLine.BorderDashStyle = ChartDashStyle.Dash;
            ca.AxisY.StripLines.Add(zeroLine);

            // 强制显示末端标签
            ca.AxisY.LabelStyle.IsEndLabelVisible = true;

            // 色标处理 (保持不变)
            double minLog = Math.Log10(validPoints.Min(p => p.Value));
            double maxLog = Math.Log10(validPoints.Max(p => p.Value));

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

            // 绘图
            Series s = chart.Series.Add("Section");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.BorderWidth = 0;
            s.IsVisibleInLegend = false;

            // 动态点大小
            int dynamicSize = (validPoints.Count > 5000) ? 6 : (validPoints.Count > 1000 ? 10 : 15);
            s.MarkerSize = dynamicSize;

            foreach (var p in validPoints)
            {
                int idx = s.Points.AddXY(p.Distance, p.Depth);
                s.Points[idx].Color = GetColorForValue(Math.Log10(p.Value), minLog, maxLog);
                s.Points[idx].ToolTip = $"距离: {p.Distance:F0}\n高程: {p.Depth:F0}\n电阻率: {p.Value:F1}";
            }

            BeautifyChartAxes(ca);
        }


        // ====================================================================================
        // 辅助函数 (补全所有缺失的方法)
        // ====================================================================================

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
                // 确保列存在，防止异常
                List<string> teCols = new List<string> { "测点编号", "周期" };
                if (m_CurrentLineData.Columns.Contains("视电阻率_TE")) teCols.Add("视电阻率_TE");
                if (m_CurrentLineData.Columns.Contains("相位_TE")) teCols.Add("相位_TE");

                List<string> tmCols = new List<string> { "测点编号", "周期" };
                if (m_CurrentLineData.Columns.Contains("视电阻率_TM")) tmCols.Add("视电阻率_TM");
                if (m_CurrentLineData.Columns.Contains("相位_TM")) tmCols.Add("相位_TM");

                DataView dvTE = new DataView(m_CurrentLineData);
                gridTE.DataSource = dvTE.ToTable(false, teCols.ToArray());

                DataView dvTM = new DataView(m_CurrentLineData);
                gridTM.DataSource = dvTM.ToTable(false, tmCols.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新表格失败: {ex.Message}");
            }
        }

        private double GetDoubleFromRow(DataRow row, string colName, double defaultVal)
        {
            try { if (row.Table.Columns.Contains(colName) && row[colName] != DBNull.Value) return Convert.ToDouble(row[colName]); } catch { }
            return defaultVal;
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

        private NiceScale CalculateStrictNiceScale(double min, double max)
        {
            if (min >= max) return new NiceScale { Min = min - 10, Max = max + 10, Interval = 10 };
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

        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;
            area.AxisX.LabelStyle.Format = "F0";
            area.AxisY.LabelStyle.Format = "F0";
            area.AxisX.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisY.LabelStyle.Font = new Font("Arial", 8f);
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(64, Color.Gray);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(64, Color.Gray);
            area.BorderColor = Color.Black;
            area.BorderDashStyle = ChartDashStyle.Solid;
        }

        private void cmbStationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
            m_SelectedStationLayer = null;
            if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
            m_SelectedSoundingTable = null;
            cmbLineName.Items.Clear();
            ClearAllDisplays();
            if (cmbStationLayer.SelectedItem == null || !(cmbStationLayer.SelectedItem is MapLayer selectedLayer)) return;
            try
            {
                m_SelectedStationLayer = selectedLayer.GetData() as SFeatureCls;
                if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen()) return;
                string tableName = selectedLayer.Name.Replace("测点", "测深数据");
                MapLayer tableLayer = m_allObjectLayers.FirstOrDefault(l => l.Name == tableName);
                if (tableLayer != null)
                {
                    m_SelectedSoundingTable = tableLayer.GetData() as ObjectCls;
                    cmbLineName.Enabled = true;
                    FillLineComboBox();
                }
            }
            catch { }
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
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

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
                dv.Sort = "周期 ASC";
                foreach (DataRowView row in dv)
                {
                    if (row["周期"] == DBNull.Value) continue;
                    double T = Convert.ToDouble(row["周期"]);
                    if (row[resField] != DBNull.Value) sRes.Points.AddXY(T, Convert.ToDouble(row[resField]));
                    if (row[phaseField] != DBNull.Value) sPhase.Points.AddXY(T, Convert.ToDouble(row[phaseField]));
                }
            }
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

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_CurrentSelectedStationName))
                UpdateRightPanelCharts();
        }

        private void ClearAllDisplays()
        {
            chartProfileView.Series.Clear(); chartResistivity.Series.Clear(); chartPhase.Series.Clear();
            if (chartResultSection != null) chartResultSection.Series.Clear();
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;
        }

        // ====================================================================================
        // 空事件占位符 (防止设计器报错)
        // ====================================================================================

        // 这就是你报错 CS1061 缺失的方法，现在补上一个空的
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            // 计时器已废弃，此方法仅用于兼容旧的设计器代码
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void panelResultBox_Paint(object sender, PaintEventArgs e) { }
        private void progressBar2_Click(object sender, EventArgs e) { }
    }
}