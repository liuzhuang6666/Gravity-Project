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
using System.Threading; // 必须引用
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

        // --- 线程与计算控制 ---
        private CancellationTokenSource _progressCancellationTokenSource;
        private List<InversionResultPoint> m_LastInversionResults = null; // 缓存结果
        private List<InversionResultPoint> m_LastInterpolatedResults = null; // 缓存插值后的网格数据
        private double m_LastInterpolatedDepth = -1;

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
            public double Depth { get; set; } // 原始值：正为山上，负为地下
            public double Value { get; set; } // 电阻率
        }

        // 结果封装类 (用于传递给UI线程)
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
        public Form_MT2di(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;

            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();

            // 绑定最大深度改变事件 (实现无需重新计算即可调整绘图)
            this.nudMaxDepth.ValueChanged += new System.EventHandler(this.nudMaxDepth_ValueChanged);
        }

        private void Form_MT2di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            InitDragEvent();
            InitializeChartStyles();
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
        // 核心计算逻辑 (Async/Await 多线程)
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
            // 1. 基础校验
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("请先选择测线并加载数据！", "提示");
                return;
            }

            // 2. 准备参数
            int its = (int)nudIterationCount.Value;
            int iwd = GetIWD_2D();

            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // 使用临时目录避免冲突
            string tempRunDir = Path.Combine(Path.GetTempPath(), "MT2di_run_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            string workspaceName = "result";
            string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);

            // 3. UI 状态更新
            btnCalculate.Enabled = false;
            progressBar1.Value = 0;
            txtActualError.Text = "";
            if (chartResultSection != null) chartResultSection.Series.Clear();

            // 4. 数据准备 (复制数据以防UI线程修改)
            DataTable lineDataCopy = m_CurrentLineData.Copy();
            Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);

            // 5. 启动进度监控
            _progressCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _progressCancellationTokenSource.Token;

            var progressTask = MonitorProgressAsync(fullWorkspacePath, cancellationToken);

            try
            {
                // 6. 后台执行计算
                var calcResult = await Task.Run(() =>
                {
                    return RunMT2DCalculationAsync(pluginDir, tempRunDir, workspaceName, lineDataCopy, stationCoords, iwd, its);
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

                    // 调用绘图逻辑
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

        private CalculationResult RunMT2DCalculationAsync(string pluginDir, string tempRunDir, string workspaceName, DataTable lineData, Dictionary<string, StationInfo> stationCoords, int iwd, int its)
        {
            var result = new CalculationResult();
            string algorithmDir = Path.Combine(pluginDir, "Algorithm", "MT2di");
            string exePath = Path.Combine(algorithmDir, "a.exe");

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
                    writer.WriteLine("PROFILE STATION COORX COORY FREQ RXY PXY RYX PYX");
                    foreach (DataRow row in lineData.Rows)
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
                    string stdErr = process.StandardError.ReadToEnd(); // 捕获错误输出
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);
                        result.WorkspacePath = fullWorkspacePath;

                        // 查找结果文件
                        string resultFile = FindBestResultFile(fullWorkspacePath);

                        if (!string.IsNullOrEmpty(resultFile) && File.Exists(resultFile))
                        {
                            result.Results = ParseKnowFile(resultFile);
                            result.Success = true;

                            // 读取 Final RMS
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
                            result.Success = false;
                            result.ErrorMessage = "计算成功执行，但未找到结果文件 (KNOW_*)。";
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = $"计算程序异常退出 (Code: {process.ExitCode})\n{stdErr}";
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
                if (parts.Length >= 2 && int.TryParse(parts[1], out int iter))
                {
                    if (iter > maxIter) { maxIter = iter; bestFile = f; }
                }
            }
            return bestFile ?? files[0];
        }

        private async Task MonitorProgressAsync(string workspacePath, CancellationToken cancellationToken)
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

        // ====================================================================================
        // 数据解析与绘图 (支持高程 + 深度控制)
        // ====================================================================================

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

                    if (parts.Length >= 3 &&
                        double.TryParse(parts[0], out double d) &&
                        double.TryParse(parts[1], out double z) &&
                        double.TryParse(parts[2], out double val))
                    {
                        // 直接读取原始值 (正值代表地形高程，负值代表地下)
                        // 不做 Math.Abs 处理
                        points.Add(new InversionResultPoint { Distance = d, Depth = z, Value = val });
                    }
                }
            }
            catch { }
            return points;
        }

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

            // 1. 获取用户输入的最大探测深度（正数 -> 负数坐标）
            double userDepthInput = (double)nudMaxDepth.Value;
            if (userDepthInput <= 0) userDepthInput = 2000;
            double minZ_Limit = -userDepthInput; // 例如 -2000

            // 2. 筛选数据：保留深度 >= -2000 的所有点 (包括地形点)
            var validPoints = points
                .Where(p => p.Value > 0 && p.Depth >= minZ_Limit)
                .ToList();

            if (validPoints.Count == 0)
            {
                chart.Titles.Add("当前深度范围内无有效数据");
                return;
            }

            // ===== 新增：IDW插值填充网格（带缓存优化）=====
            // 检查是否需要重新插值
            if (m_LastInterpolatedResults == null || Math.Abs(userDepthInput - m_LastInterpolatedDepth) > 0.1)
            {
                var interpolatedPoints = PerformIDWInterpolation(validPoints, minZ_Limit);
                if (interpolatedPoints.Count > 0)
                {
                    m_LastInterpolatedResults = interpolatedPoints;
                    m_LastInterpolatedDepth = userDepthInput;
                    validPoints = interpolatedPoints;
                }
            }
            else
            {
                // 使用缓存的插值结果，只需要过滤深度
                validPoints = m_LastInterpolatedResults
                    .Where(p => p.Depth >= minZ_Limit)
                    .ToList();
            }
            // ===== IDW插值结束 =====

            chart.Titles.Add($"MT 二维反演电阻率断面图 ( > {minZ_Limit}m )").Font = new Font("微软雅黑", 12f, FontStyle.Bold);

            // 3. 计算数据真实范围
            double dataMinX = validPoints.Min(p => p.Distance);
            double dataMaxX = validPoints.Max(p => p.Distance);
            double maxDataZ = validPoints.Max(p => p.Depth); // 地形最高点
            double dataMaxY = maxDataZ > 0 ? maxDataZ : 0;   // Y轴上限至少为0

            // 4. 计算刻度
            var niceX = CalculateStrictNiceScale(dataMinX, dataMaxX);
            var niceY = CalculateStrictNiceScale(minZ_Limit, dataMaxY);

            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "距离 (m)";
            ca.AxisY.Title = "高程 (m)";
            ca.AxisX.TitleAlignment = StringAlignment.Center;
            ca.AxisY.TitleAlignment = StringAlignment.Center;

            // 设置 X 轴
            ca.AxisX.Minimum = niceX.Min;
            ca.AxisX.Maximum = niceX.Max;
            ca.AxisX.Interval = niceX.Interval;

            // 设置 Y 轴
            ca.AxisY.Minimum = minZ_Limit; // 底部固定
            double topMargin = (dataMaxY - minZ_Limit) * 0.05;
            ca.AxisY.Maximum = dataMaxY + topMargin; // 顶部自适应
            ca.AxisY.Interval = niceY.Interval;

            // 关键：自然坐标系 (上正下负)，不反转
            ca.AxisY.IsReversed = false;

            // 强制显示 0 刻度线 (地面参考线)
            ca.AxisY.Crossing = 0;
            StripLine zeroLine = new StripLine();
            zeroLine.Interval = 0;
            zeroLine.StripWidth = 0;
            zeroLine.BorderColor = Color.Black;
            zeroLine.BorderWidth = 1;
            zeroLine.BorderDashStyle = ChartDashStyle.Dash;
            ca.AxisY.StripLines.Add(zeroLine);

            ca.AxisY.LabelStyle.IsEndLabelVisible = true;

            // 5. 色标
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

            // 6. 绘图
            Series s = chart.Series.Add("Section");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.BorderWidth = 0;
            s.IsVisibleInLegend = false;

            // 动态调整点大小（插值后点数会增加，所以用更小的点）
            int dynamicSize = (validPoints.Count > 10000) ? 3 : (validPoints.Count > 5000 ? 4 : 6);
            s.MarkerSize = dynamicSize;

            foreach (var p in validPoints)
            {
                int idx = s.Points.AddXY(p.Distance, p.Depth);
                s.Points[idx].Color = GetColorForValue(Math.Log10(p.Value), minLog, maxLog);
                s.Points[idx].ToolTip = $"距离: {p.Distance:F0}\n高程: {p.Depth:F0}\n电阻率: {p.Value:F1}";
            }

            BeautifyResultChartAxes(ca);
        }

        // 新增：快速绘制方法（只更新显示范围，不重新插值）
        private void DisplayInversionResultsFast(List<InversionResultPoint> interpolatedPoints)
        {
            var chart = chartResultSection;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();

            if (interpolatedPoints == null || interpolatedPoints.Count == 0) return;

            double userDepthInput = (double)nudMaxDepth.Value;
            if (userDepthInput <= 0) userDepthInput = 2000;
            double minZ_Limit = -userDepthInput;

            var validPoints = interpolatedPoints
                .Where(p => p.Depth >= minZ_Limit)
                .ToList();

            if (validPoints.Count == 0)
            {
                chart.Titles.Add("当前深度范围内无有效数据");
                return;
            }

            chart.Titles.Add($"MT 二维反演电阻率断面图 ( > {minZ_Limit}m )").Font = new Font("微软雅黑", 12f, FontStyle.Bold);

            double dataMinX = validPoints.Min(p => p.Distance);
            double dataMaxX = validPoints.Max(p => p.Distance);
            double maxDataZ = validPoints.Max(p => p.Depth);
            double dataMaxY = maxDataZ > 0 ? maxDataZ : 0;

            var niceX = CalculateStrictNiceScale(dataMinX, dataMaxX);
            var niceY = CalculateStrictNiceScale(minZ_Limit, dataMaxY);

            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "距离 (m)";
            ca.AxisY.Title = "高程 (m)";
            ca.AxisX.TitleAlignment = StringAlignment.Center;
            ca.AxisY.TitleAlignment = StringAlignment.Center;

            ca.AxisX.Minimum = niceX.Min;
            ca.AxisX.Maximum = niceX.Max;
            ca.AxisX.Interval = niceX.Interval;

            ca.AxisY.Minimum = minZ_Limit;
            double topMargin = (dataMaxY - minZ_Limit) * 0.05;
            ca.AxisY.Maximum = dataMaxY + topMargin;
            ca.AxisY.Interval = niceY.Interval;

            ca.AxisY.IsReversed = false;
            ca.AxisY.Crossing = 0;

            StripLine zeroLine = new StripLine();
            zeroLine.Interval = 0;
            zeroLine.StripWidth = 0;
            zeroLine.BorderColor = Color.Black;
            zeroLine.BorderWidth = 1;
            zeroLine.BorderDashStyle = ChartDashStyle.Dash;
            ca.AxisY.StripLines.Add(zeroLine);

            ca.AxisY.LabelStyle.IsEndLabelVisible = true;

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

            Series s = chart.Series.Add("Section");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.BorderWidth = 0;
            s.IsVisibleInLegend = false;

            int dynamicSize = (validPoints.Count > 10000) ? 3 : (validPoints.Count > 5000 ? 4 : 6);
            s.MarkerSize = dynamicSize;

            foreach (var p in validPoints)
            {
                int idx = s.Points.AddXY(p.Distance, p.Depth);
                s.Points[idx].Color = GetColorForValue(Math.Log10(p.Value), minLog, maxLog);
                s.Points[idx].ToolTip = $"距离: {p.Distance:F0}\n高程: {p.Depth:F0}\n电阻率: {p.Value:F1}";
            }

            BeautifyResultChartAxes(ca);
        }


        // 步骤2: 在 BeautifyResultChartAxes 方法之前，添加以下两个新方法

        /// <summary>
        /// IDW反距离加权插值 - 填充网格
        /// </summary>
        private List<InversionResultPoint> PerformIDWInterpolation(List<InversionResultPoint> sourcePoints, double minZ)
        {
            if (sourcePoints.Count == 0) return sourcePoints;

            // 计算数据边界
            double minX = sourcePoints.Min(p => p.Distance);
            double maxX = sourcePoints.Max(p => p.Distance);
            double maxZ = sourcePoints.Max(p => p.Depth);

            // 确保顶部至少到0
            if (maxZ < 0) maxZ = 0;

            // 动态计算网格密度（根据数据范围）
            double xRange = maxX - minX;
            double zRange = maxZ - minZ;

            // ===== 调整密度：纵向加密 =====
            // 横向：每15米一个点
            int xSteps = Math.Min(400, Math.Max(60, (int)(xRange / 15)));
            // 纵向：每10米一个点（原来是20米，现在加密一倍）
            int zSteps = Math.Min(400, Math.Max(50, (int)(zRange / 10)));
            // ===== 密度调整结束 =====

            double xStep = xRange / xSteps;
            double zStep = zRange / zSteps;

            var interpolatedPoints = new List<InversionResultPoint>();
            double power = 2.0; // IDW幂参数（2.0是标准值）
            double searchRadius = Math.Max(xStep, zStep) * 8; // 搜索半径

            // 生成规则网格并插值
            for (int i = 0; i <= xSteps; i++)
            {
                double x = minX + i * xStep;

                for (int j = 0; j <= zSteps; j++)
                {
                    double z = maxZ - j * zStep;

                    // 只插值在数据范围内的点
                    if (z < minZ) continue;

                    // IDW插值计算
                    double interpolatedValue = CalculateIDWValue(x, z, sourcePoints, power, searchRadius);

                    if (interpolatedValue > 0)
                    {
                        interpolatedPoints.Add(new InversionResultPoint
                        {
                            Distance = x,
                            Depth = z,
                            Value = interpolatedValue
                        });
                    }
                }
            }

            return interpolatedPoints.Count > 0 ? interpolatedPoints : sourcePoints;
        }

        /// <summary>
        /// 计算IDW插值值
        /// </summary>
        private double CalculateIDWValue(double x, double z, List<InversionResultPoint> sourcePoints, double power, double searchRadius)
        {
            double weightSum = 0;
            double valueSum = 0;
            int pointsUsed = 0;

            // 第一遍：检查是否恰好在某个源点上
            foreach (var point in sourcePoints)
            {
                double dist = Math.Sqrt(Math.Pow(x - point.Distance, 2) + Math.Pow(z - point.Depth, 2));

                if (dist < 1e-6) // 几乎重合
                {
                    return point.Value;
                }
            }

            // 第二遍：使用搜索半径内的所有点
            foreach (var point in sourcePoints)
            {
                double dist = Math.Sqrt(Math.Pow(x - point.Distance, 2) + Math.Pow(z - point.Depth, 2));

                if (dist > searchRadius) continue;

                double weight = 1.0 / Math.Pow(dist, power);
                weightSum += weight;
                valueSum += weight * point.Value;
                pointsUsed++;
            }

            // 如果搜索半径内没有足够的点，使用最近的N个点
            if (pointsUsed < 4)
            {
                var nearestPoints = sourcePoints
                    .Select(p => new {
                        Point = p,
                        Distance = Math.Sqrt(Math.Pow(x - p.Distance, 2) + Math.Pow(z - p.Depth, 2))
                    })
                    .OrderBy(p => p.Distance)
                    .Take(8) // 使用最近的8个点
                    .ToList();

                weightSum = 0;
                valueSum = 0;

                foreach (var np in nearestPoints)
                {
                    if (np.Distance < 1e-6) return np.Point.Value;

                    double weight = 1.0 / Math.Pow(np.Distance, power);
                    weightSum += weight;
                    valueSum += weight * np.Point.Value;
                }
            }

            return weightSum > 0 ? valueSum / weightSum : 0;
        }

        private void BeautifyResultChartAxes(ChartArea area)
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

        // ====================================================================================
        // 辅助函数 (保持原版逻辑)
        // ====================================================================================

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
            area.BorderColor = Color.Black;
            area.BorderDashStyle = ChartDashStyle.Solid;
        }

        // --- 基本UI交互与图层加载 ---
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
        private void timerProgress_Tick(object sender, EventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void panelResultBox_Paint(object sender, PaintEventArgs e) { }
        private void rbInversionTE_CheckedChanged(object sender, EventArgs e) { }
    }
}