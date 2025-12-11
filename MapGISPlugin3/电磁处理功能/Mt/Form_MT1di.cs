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

        // 图例名称常量 (保持代码一原样)
        private const string ProfileLegendName = "ProfileLegend";
        private const string ResistivityLegendName = "ResistivityLegend";
        private const string PhaseLegendName = "PhaseLegend";

        // --- 线程与计算控制 ---
        private CancellationTokenSource _progressCancellationTokenSource;
        private List<InversionResultPoint> m_LastInversionResults = null;

        // 窗口拖动逻辑变量
        private Point mousePoint = new Point();

        // --- 内部数据结构 ---
        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        // 修改：根据 KNOW 文件格式定义结果点
        private class InversionResultPoint
        {
            public double Distance { get; set; } // 第一列
            public double Depth { get; set; }    // 第二列 (负值)
            public double Value { get; set; }    // 第三列 (电阻率)
        }

        private class CalculationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public List<InversionResultPoint> Results { get; set; } = new List<InversionResultPoint>();
            public string WorkspacePath { get; set; }
            public string ActualError { get; set; }
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

            // 绑定最大深度改变事件
            this.nudMaxDepth.ValueChanged += new System.EventHandler(this.nudMaxDepth_ValueChanged);
        }

        private void Form_MT1di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            InitDragEvent();
            InitializeChartStyles(); // 恢复代码一的样式初始化
            chartResultSection.Visible = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("请先选择测线并加载数据！", "提示");
                return;
            }

            int its = (int)nudIterationCount.Value;
            int iwd = rbInversionTE.Checked ? 0 : 1;

            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempRunDir = Path.Combine(Path.GetTempPath(), "MT1di_run_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            string workspaceName = "result";
            string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);

            btnCalculate.Enabled = false;
            progressBar1.Value = 0;
            txtActualError.Text = "计算中...";

            DataTable lineDataCopy = m_CurrentLineData.Copy();
            Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);

            _progressCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _progressCancellationTokenSource.Token;

            var progressTask = MonitorProgressAsync(fullWorkspacePath, its, cancellationToken);

            try
            {
                var calcResult = await Task.Run(() =>
                {
                    return RunMTCalculationAsync(pluginDir, tempRunDir, workspaceName, lineDataCopy, stationCoords, iwd, its);
                });

                _progressCancellationTokenSource.Cancel();
                try { await progressTask; } catch (OperationCanceledException) { }

                if (calcResult.Success)
                {
                    progressBar1.Value = 100;
                    if (!string.IsNullOrEmpty(calcResult.ActualError))
                        txtActualError.Text = calcResult.ActualError;

                    m_LastInversionResults = calcResult.Results;

                    // 使用新的绘图逻辑
                    DisplayInversionResults(calcResult.Results);

                    MessageBox.Show($"计算完成！\n结果保存在: {calcResult.WorkspacePath}", "成功");
                }
                else
                {
                    progressBar1.Value = 0;
                    // 清空显示
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

                    // 格式验证：距离(X)  深度(Z,负值)  电阻率(Rho)
                    if (parts.Length >= 3 &&
                        double.TryParse(parts[0], out double d) &&
                        double.TryParse(parts[1], out double z) &&
                        double.TryParse(parts[2], out double val))
                    {
                        // 直接读取文件中的深度值 (通常为负，表示地下)
                        points.Add(new InversionResultPoint { Distance = d, Depth = z, Value = val });
                    }
                }
            }
            catch { }
            return points;
        }

        // ====================================================================================
        // 【核心绘图】结果展示 (位图渲染 + 柱状显示 + 代码一的容器)
        // ====================================================================================

        private void nudMaxDepth_ValueChanged(object sender, EventArgs e)
        {
            if (m_LastInversionResults != null && m_LastInversionResults.Count > 0)
                DisplayInversionResults(m_LastInversionResults);
        }

        private void DisplayInversionResults(List<InversionResultPoint> points)
        {
            // 1. 基础校验
            if (points == null || points.Count == 0)
            {
                // 确实没数据时，才清空显示
                chartResultSection.Series.Clear();
                chartResultSection.ChartAreas.Clear();
                chartResultSection.Images.Clear();
                chartResultSection.Titles.Clear();
                chartResultSection.Visible = false;
                return;
            }

            // 获取控件引用，但暂时不清空它，保持旧画面直到新画面就绪
            var chart = chartResultSection;

            // =========================================================
            // 第一阶段：数据准备 (在内存中进行，不操作 UI)
            // =========================================================

            // 获取设置参数
            double userDepthInput = (double)nudMaxDepth.Value;
            if (userDepthInput <= 0) userDepthInput = 2000;

            double minZ_Display = -userDepthInput;
            double maxZ_Display = 0;

            // 数据过滤
            var validPoints = points.Where(p => p.Value > 0 && p.Depth >= minZ_Display && p.Depth <= 0.1).ToList();

            if (validPoints.Count == 0)
            {
                // 如果筛选后没数据，这时再清空并提示
                chart.Series.Clear();
                chart.ChartAreas.Clear();
                chart.Images.Clear();
                chart.Titles.Clear();
                chart.Titles.Add("当前深度范围内无有效数据");
                return;
            }

            // 数据分组与排序
            var stationGroups = validPoints
                .GroupBy(p => p.Distance)
                .Select(g => new
                {
                    Distance = g.Key,
                    Layers = g.OrderByDescending(p => p.Depth).ToList()
                })
                .OrderBy(s => s.Distance)
                .ToList();

            if (stationGroups.Count == 0) return;

            // 计算色标范围
            double minLog = Math.Log10(validPoints.Min(p => p.Value));
            double maxLog = Math.Log10(validPoints.Max(p => p.Value));

            // 计算几何参数
            double minX = stationGroups.Min(s => s.Distance);
            double maxX = stationGroups.Max(s => s.Distance);
            double rangeX = maxX - minX;
            if (rangeX <= 0) rangeX = 100;

            double[] stationWidths = new double[stationGroups.Count];
            double[] stationLeftEdges = new double[stationGroups.Count];

            for (int i = 0; i < stationGroups.Count; i++)
            {
                double leftBound, rightBound;
                double currentX = stationGroups[i].Distance;

                if (i == 0)
                {
                    leftBound = currentX - minX;
                    if (stationGroups.Count > 1)
                        rightBound = (currentX + stationGroups[i + 1].Distance) / 2 - minX;
                    else
                        rightBound = rangeX;
                }
                else if (i == stationGroups.Count - 1)
                {
                    leftBound = (stationGroups[i - 1].Distance + currentX) / 2 - minX;
                    rightBound = rangeX;
                }
                else
                {
                    leftBound = (stationGroups[i - 1].Distance + currentX) / 2 - minX;
                    rightBound = (currentX + stationGroups[i + 1].Distance) / 2 - minX;
                }
                stationLeftEdges[i] = leftBound;
                stationWidths[i] = rightBound - leftBound;
            }

            // =========================================================
            // 第二阶段：后台绘图 (最耗时的步骤)
            // =========================================================

            // 预先生成 Bitmap，此时还没有贴到 Chart 上，界面不会白屏
            int bmpWidth = Math.Max(chart.Width - 100, 800);
            int bmpHeight = Math.Max(chart.Height - 100, 400);

            // 使用临时变量存储生成的图片
            Bitmap readyBitmap = new Bitmap(bmpWidth, bmpHeight);

            using (Graphics g = Graphics.FromImage(readyBitmap))
            {
                g.Clear(Color.White);

                double xScale = bmpWidth / rangeX;
                double yScale = bmpHeight / Math.Abs(minZ_Display);

                for (int i = 0; i < stationGroups.Count; i++)
                {
                    var group = stationGroups[i];
                    var layers = group.Layers;

                    double xPixel = stationLeftEdges[i] * xScale;
                    double wPixel = stationWidths[i] * xScale;
                    if (wPixel < 1) wPixel = 1;

                    for (int j = 0; j < layers.Count; j++)
                    {
                        var layer = layers[j];
                        double zTop, zBottom;

                        if (j == 0)
                        {
                            zTop = maxZ_Display;
                            if (layers.Count > 1) zBottom = (layer.Depth + layers[j + 1].Depth) / 2;
                            else zBottom = layer.Depth;
                        }
                        else if (j == layers.Count - 1)
                        {
                            zTop = (layers[j - 1].Depth + layer.Depth) / 2;
                            zBottom = minZ_Display;
                        }
                        else
                        {
                            zTop = (layers[j - 1].Depth + layer.Depth) / 2;
                            zBottom = (layer.Depth + layers[j + 1].Depth) / 2;
                        }

                        zTop = Math.Min(maxZ_Display, Math.Max(minZ_Display, zTop));
                        zBottom = Math.Min(maxZ_Display, Math.Max(minZ_Display, zBottom));

                        if (zBottom >= zTop) continue;

                        float yPixel = (float)(Math.Abs(zTop) * yScale);
                        float hPixel = (float)(Math.Abs(zBottom - zTop) * yScale);

                        if (hPixel > 0)
                        {
                            Color color = GetColorForValueSmooth(Math.Log10(layer.Value), minLog, maxLog);
                            using (SolidBrush brush = new SolidBrush(color))
                            {
                                g.FillRectangle(brush, (float)xPixel, yPixel, (float)wPixel + 0.5f, hPixel + 0.5f);
                            }
                        }
                    }
                }
            } // Graphics 释放，但 readyBitmap 还活着

            // =========================================================
            // 第三阶段：UI 瞬间替换 (毫秒级)
            // =========================================================

            // 挂起 UI 逻辑，防止刷新闪烁
            chart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(chart)).BeginInit();

            try
            {
                // 1. 此时才清空旧内容
                chart.Series.Clear();
                chart.ChartAreas.Clear();
                chart.Legends.Clear();
                chart.Titles.Clear();
                chart.Images.Clear(); // 移除旧背景图

                // 2. 添加标题
                chart.Titles.Add($"MT 一维反演电阻率断面图 (0 ~ {minZ_Display}m)").Font = new Font("微软雅黑", 12f, FontStyle.Bold);

                // 3. 设置背景图
                ChartArea ca = chart.ChartAreas.Add("ResultArea");
                string imgName = "BackImage_" + Guid.NewGuid().ToString();

                // 将准备好的 Bitmap 放入 NamedImage
                // 注意：这里不需要再Clone了，因为 readyBitmap 是我们刚生成的
                chart.Images.Add(new NamedImage(imgName, readyBitmap));

                ca.BackImage = imgName;
                ca.BackImageWrapMode = ChartImageWrapMode.Scaled;

                // 4. 设置轴范围
                ca.AxisX.Minimum = minX;
                ca.AxisX.Maximum = maxX;
                ca.AxisY.Minimum = minZ_Display;
                ca.AxisY.Maximum = maxZ_Display;

                // 5. 设置细分刻度 (修复坐标轴过大的问题)
                double xInterval = CalculateNiceInterval(rangeX, 10);
                double yInterval = CalculateNiceInterval(Math.Abs(minZ_Display - maxZ_Display), 8);

                ca.AxisX.Interval = xInterval;
                ca.AxisY.Interval = yInterval;

                ca.AxisX.MinorGrid.Enabled = true;
                ca.AxisX.MinorGrid.Interval = xInterval / 2;
                ca.AxisX.MinorGrid.LineColor = Color.FromArgb(20, Color.Gray);

                ca.AxisY.MinorGrid.Enabled = true;
                ca.AxisY.MinorGrid.Interval = yInterval / 2;
                ca.AxisY.MinorGrid.LineColor = Color.FromArgb(20, Color.Gray);

                BeautifyChartAxes(ca);

                // 6. 重新添加图例
                Legend legend = chart.Legends.Add("ColorScale");
                legend.Docking = Docking.Right;
                legend.Title = "lg(ρ/Ω·m)";
                legend.Font = new Font("Arial", 8F);

                int legendSteps = 10;
                for (int i = legendSteps; i >= 0; i--)
                {
                    double val = minLog + (maxLog - minLog) * i / legendSteps;
                    legend.CustomItems.Add(new System.Windows.Forms.DataVisualization.Charting.LegendItem
                    {
                        Name = val.ToString("F1"),
                        Color = GetColorForValueSmooth(val, minLog, maxLog),
                        MarkerStyle = MarkerStyle.Square,
                        MarkerSize = 14
                    });
                }

                // 7. 哑序列撑开坐标轴
                Series sDummy = chart.Series.Add("Dummy");
                sDummy.ChartType = SeriesChartType.Point;
                sDummy.Points.AddXY(minX, maxZ_Display);
                sDummy.Points.AddXY(maxX, minZ_Display);
                sDummy.Color = Color.Transparent;
                sDummy.IsVisibleInLegend = false;
                chart.Visible = true;
            }
            finally
            {
                // 恢复 UI 刷新
                ((System.ComponentModel.ISupportInitialize)(chart)).EndInit();
                chart.ResumeLayout();
            }
        }
        // ====================================================================================
        // 辅助函数：颜色与美化 (移植自 CSAMT 代码)
        // ====================================================================================
        /// <summary>
        /// 计算美观的刻度间隔（1, 2, 5 的倍数），确保坐标轴有足够的细分
        /// </summary>
        private double CalculateNiceInterval(double range, int targetTicks)
        {
            if (range <= 0 || targetTicks <= 0) return 1;

            double roughInterval = range / targetTicks;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
            double residual = roughInterval / magnitude;

            double niceInterval;
            if (residual <= 1.5)
                niceInterval = 1 * magnitude;
            else if (residual <= 3)
                niceInterval = 2 * magnitude;
            else if (residual <= 7)
                niceInterval = 5 * magnitude;
            else
                niceInterval = 10 * magnitude;

            return niceInterval;
        }
        private Color GetColorForValueSmooth(double value, double min, double max)
        {
            if (min >= max) return Color.Gray;
            double normalized = Math.Max(0, Math.Min(1, (value - min) / (max - min)));
            return GetJetColor(normalized);
        }

        private Color GetJetColor(double v)
        {
            double r, g, b;
            if (v < 0.125) { r = 0; g = 0; b = 0.5 + v * 4; }
            else if (v < 0.375) { r = 0; g = (v - 0.125) * 4; b = 1; }
            else if (v < 0.625) { r = (v - 0.375) * 4; g = 1; b = 1 - (v - 0.375) * 4; }
            else if (v < 0.875) { r = 1; g = 1 - (v - 0.625) * 4; b = 0; }
            else { r = 1 - (v - 0.875) * 4; g = 0; b = 0; }
            return Color.FromArgb((int)(Math.Max(0, Math.Min(1, r)) * 255),
                                  (int)(Math.Max(0, Math.Min(1, g)) * 255),
                                  (int)(Math.Max(0, Math.Min(1, b)) * 255));
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
            area.AxisX.Title = "距离 (m)";
            area.AxisY.Title = "深度 (m)";
            area.BorderColor = Color.Black;
            area.BorderDashStyle = ChartDashStyle.Solid;
        }

        // ====================================================================================
        // 【完全恢复】代码一 原有的图表逻辑 (Profile, Resistivity, Phase)
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

        // --- 恢复 chartProfileView 逻辑 ---
        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;
            chartProfileView.Series.Clear();
            if (chartProfileView.Legends[ProfileLegendName] == null) InitChartLegend(chartProfileView, ProfileLegendName);

            // 恢复 ChartArea 设置
            if (chartProfileView.ChartAreas.Count > 0)
            {
                var chartArea = chartProfileView.ChartAreas[0];
                chartArea.Position = new ElementPosition(8, 5, 85, 90);
                chartArea.InnerPlotPosition = new ElementPosition(10, 10, 85, 85);
                BeautifyProfileViewAxes(chartArea);
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

        // --- 恢复 chartResistivity/Phase 逻辑 ---
        private void UpdateRightPanelCharts()
        {
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();
            if (chartResistivity.Legends[ResistivityLegendName] == null) InitChartLegend(chartResistivity, ResistivityLegendName);
            if (chartPhase.Legends[PhaseLegendName] == null) InitChartLegend(chartPhase, PhaseLegendName);
            AdjustChartAreaLayout(chartResistivity.ChartAreas[0]);
            AdjustChartAreaLayout(chartPhase.ChartAreas[0]);
            string resField = tabControl2.SelectedTab == tabPageDisplayTE ? "视电阻率_TE" : "视电阻率_TM";
            string phaseField = tabControl2.SelectedTab == tabPageDisplayTE ? "相位_TE" : "相位_TM";

            Series sRes = chartResistivity.Series.Add("视电阻率");
            sRes.ChartType = SeriesChartType.Spline;
            sRes.MarkerStyle = MarkerStyle.Circle;
            sRes.MarkerSize = 5;
            sRes.BorderWidth = 2;
            sRes.Legend = ResistivityLegendName;
            sRes.LegendText = $"视电阻率";

            Series sPhase = chartPhase.Series.Add("相位");
            sPhase.ChartType = SeriesChartType.Spline;
            sPhase.MarkerStyle = MarkerStyle.Circle;
            sPhase.MarkerSize = 5;
            sPhase.BorderWidth = 2;
            sPhase.Legend = PhaseLegendName;
            sPhase.LegendText = $"相位";

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


            BeautifyChartAxes(chartResistivity.ChartAreas[0]); // 使用通用的 Beautify
            BeautifyChartAxes(chartPhase.ChartAreas[0]);
            chartResistivity.ChartAreas[0].AxisX.Title = "周期(s)";
            chartResistivity.ChartAreas[0].AxisY.Title = "视电阻率";
            chartPhase.ChartAreas[0].AxisX.Title = "周期(s)";
            chartPhase.ChartAreas[0].AxisY.Title = "相位";
            CalibrateLegendSize(chartResistivity);
            CalibrateLegendSize(chartPhase);
        }
        private void AdjustChartAreaLayout(ChartArea area)
        {
            if (area == null) return;

            // 1. 设置 ChartArea 在整个控件中的位置 (通常占满)
            area.Position.Auto = false;
            area.Position.X = 0;
            area.Position.Y = 0;
            area.Position.Width = 90;
            area.Position.Height = 80;

            // 2. 设置 InnerPlotPosition (实际画线和格网的区域，不含轴标签)
            // Auto = false 禁止自动调整，防止被图例挤压或反之
            area.InnerPlotPosition.Auto = false;

            // X=10: 左侧留 10% 给 Y 轴数值标签
            // Y=15: 顶部留 15% 给图例 (这是你要的下移效果)
            // Width=85: 宽度
            // Height=78: 高度 (下方留些空间给 X 轴标签)
            area.InnerPlotPosition = new ElementPosition(22, 20, 85, 90);
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

        // 恢复 CalibrateLegendSize
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

        // 恢复 BeautifyProfileViewAxes
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
            if (!string.IsNullOrEmpty(m_CurrentSelectedStationName)) UpdateRightPanelCharts();
        }

        private void ClearAllDisplays()
        {
            chartProfileView.Series.Clear(); chartResistivity.Series.Clear(); chartPhase.Series.Clear();
            if (chartResultSection != null)
            {
                chartResultSection.Series.Clear();
                chartResultSection.Images.Clear();
                chartResultSection.Visible = false;
            }
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;
        }

        // 空事件占位符
        private void timerProgress_Tick(object sender, EventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void panelResultBox_Paint(object sender, PaintEventArgs e) { }
        private void progressBar2_Click(object sender, EventArgs e) { }
        private void CreateImageDisplayMap(Chart chart)
        {
            Map newMap = null;
            RasterLayer displayLayer = null;
            Document projectDoc = null;
            Maps projectMaps = null;

            // 前置校验：确保图表和图片有效
            if (chart == null)
            {
                MessageBox.Show("断面图控件未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 1. 获取新Map名称（和Chart标题一致）
            string newMapName = string.Empty;
            if (chart.Titles.Count > 0)
            {
                newMapName = chart.Titles[0].Text?.Trim();
            }
            if (string.IsNullOrWhiteSpace(newMapName))
            {
                newMapName = $"电阻率断面图_展示图_{DateTime.Now:yyyyMMddHHmmss}";
            }

            // 2. 校验图表是否已生成（适配反演图和视电阻率图）
            bool isChartValid = chart.Series.Count > 0 || (chart.ChartAreas.Count > 0 && !string.IsNullOrEmpty(chart.ChartAreas[0]?.BackImage));
            if (!isChartValid)
            {
                MessageBox.Show("请先生成对应图表（反演结果/视电阻率），再创建展示地图！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3. 生成临时图片（核心：保留样式+不压缩绘图区域）
            string tempImagePath = Path.ChangeExtension(Path.GetTempFileName(), ".png");

            // 记录Chart原始样式（生成后恢复，不影响原控件显示）
            System.Drawing.Color originalChartBackColor = chart.BackColor;
            int originalChartBorderWidth = chart.BorderWidth;
            Dictionary<ChartArea, (System.Drawing.Color BackColor, int BorderWidth, System.Drawing.Color BorderColor, bool InnerPlotAuto)> originalChartAreaProps = new Dictionary<ChartArea, (System.Drawing.Color, int, System.Drawing.Color, bool)>();
            Dictionary<Legend, (System.Drawing.Color BackColor, int BorderWidth, System.Drawing.Color BorderColor)> originalLegendProps = new Dictionary<Legend, (System.Drawing.Color, int, System.Drawing.Color)>();

            try
            {
                // 👉 步骤1：记录原始样式（新增记录InnerPlotPosition.Auto状态）
                foreach (ChartArea ca in chart.ChartAreas)
                {
                    originalChartAreaProps[ca] = (ca.BackColor, ca.BorderWidth, ca.BorderColor, ca.InnerPlotPosition.Auto);
                }
                foreach (Legend lg in chart.Legends)
                {
                    originalLegendProps[lg] = (lg.BackColor, lg.BorderWidth, lg.BorderColor);
                }

                // 👉 步骤2：设置Chart样式（保留需求样式，删除压缩绘图区域的代码）
                chart.BackColor = System.Drawing.Color.White;          // 保留：整体白色背景
                chart.BorderWidth = 0;                                 // 保留：取消整体默认边框（避免重复）

                // 断面图区域（ChartArea）：保留背景+边框，删除InnerPlotPosition固定值（不压缩绘图区域）
                foreach (ChartArea chartArea in chart.ChartAreas)
                {
                    chartArea.BackColor = System.Drawing.Color.White;  // 保留：图背景白色
                    chartArea.BorderWidth = 1;                          // 保留：1px黑色边框
                    chartArea.BorderColor = System.Drawing.Color.Black;
                    chartArea.InnerPlotPosition.Auto = true;           // 关键：恢复自动绘图区域，不压缩（避免坐标被挡）
                                                                       //  删除：之前的固定InnerPlotPosition设置（X=5、Y=5等）
                }

                // 图例（Legend）：保留白色背景+黑色边框（不修改其他属性）
                foreach (Legend legend in chart.Legends)
                {
                    legend.BackColor = System.Drawing.Color.White;     // 保留：图例白色背景
                    legend.BorderWidth = 1;                             // 保留：1px黑色边框
                    legend.BorderColor = System.Drawing.Color.Black;
                }

                // 👉 步骤3：生成带样式的图片（用原图完整大小，不裁剪）
                using (Bitmap chartBmp = new Bitmap(chart.ClientSize.Width, chart.ClientSize.Height))
                {
                    chart.Refresh(); // 刷新确保样式生效
                    chart.DrawToBitmap(chartBmp, chart.ClientRectangle); // 捕获原图完整区域（包括坐标）
                    chartBmp.Save(tempImagePath, System.Drawing.Imaging.ImageFormat.Png); // 无损保存
                }

                // 👉 步骤4：恢复Chart原始样式（包括InnerPlotPosition.Auto状态）
                chart.BackColor = originalChartBackColor;
                chart.BorderWidth = originalChartBorderWidth;
                foreach (var kvp in originalChartAreaProps)
                {
                    kvp.Key.BackColor = kvp.Value.BackColor;
                    kvp.Key.BorderWidth = kvp.Value.BorderWidth;
                    kvp.Key.BorderColor = kvp.Value.BorderColor;
                    kvp.Key.InnerPlotPosition.Auto = kvp.Value.InnerPlotAuto; // 恢复绘图区域自动分配
                }
                foreach (var kvp in originalLegendProps)
                {
                    kvp.Key.BackColor = kvp.Value.BackColor;
                    kvp.Key.BorderWidth = kvp.Value.BorderWidth;
                    kvp.Key.BorderColor = kvp.Value.BorderColor;
                }

                // 4. MapGIS相关操作（保持你的原始接口，不改动）
                projectDoc = _hook.Document;
                if (projectDoc == null)
                {
                    throw new Exception("未打开MapGIS项目，请先打开项目！");
                }
                projectMaps = projectDoc.GetMaps();
                if (projectMaps == null)
                {
                    throw new Exception("项目地图集合为空！");
                }

                // 5. 检查同名地图（保持不变）
                for (int i = 0; i < projectMaps.Count; i++)
                {
                    Map existingMap = projectMaps.GetMap(i);
                    if (existingMap != null && existingMap.Name == newMapName)
                    {
                        DialogResult result = MessageBox.Show(
                            $"已存在名为【{newMapName}】的地图，是否覆盖？",
                            "提示",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );
                        if (result == DialogResult.Yes)
                        {
                            projectMaps.Remove(i);
                            Marshal.ReleaseComObject(existingMap);
                        }
                        else
                        {
                            MessageBox.Show("取消创建展示地图！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        break;
                    }
                }

                // 6. 创建新Map并加载图层（保持不变）
                newMap = new Map();
                newMap.Name = newMapName;

                displayLayer = new RasterLayer();
                displayLayer.URL = tempImagePath;
                displayLayer.Name = $"{newMapName}_展示图层";

                if (!displayLayer.ConnectData() || !displayLayer.IsValid)
                {
                    throw new Exception("图片图层加载失败！");
                }

                displayLayer.NoDataColor = System.Drawing.Color.White;

                // 设置图层范围（和图片尺寸一致，确保显示完整）
                using (Image img = Image.FromFile(tempImagePath))
                {
                    Rect imgRange = new Rect(0, 0, img.Width, img.Height);
                    displayLayer.set_Range(imgRange);
                }

                // 7. 添加图层和地图（保持你的原始接口）
                newMap.Append(displayLayer);
                projectMaps.Append(newMap);

                MessageBox.Show("创建成功");
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"MapGIS组件错误：{comEx.Message}\n错误码：{comEx.ErrorCode}", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建展示地图失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 释放资源（保持不变）
                if (displayLayer != null)
                {
                    try
                    {
                        //displayLayer.Close();
                        //Marshal.ReleaseComObject(displayLayer);
                    }
                    catch { }
                }
                if (newMap != null)
                {
                    try { Marshal.ReleaseComObject(newMap); } catch { }
                }
                if (projectMaps != null)
                {
                    try { Marshal.ReleaseComObject(projectMaps); } catch { }
                }
                if (projectDoc != null)
                {
                    try { Marshal.ReleaseComObject(projectDoc); } catch { }
                }

                // 删除临时图片
                if (File.Exists(tempImagePath))
                {
                    try { File.Delete(tempImagePath); }
                    catch (Exception ex) { Console.WriteLine($"删除临时文件失败：{ex.Message}"); }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // 前置校验：确保反演结果图已生成且有效
            if (chartResultSection == null)
            {
                MessageBox.Show("反演结果图表未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 校验图表是否已生成（反演结果图用 ChartArea.BackImage 判断，和 CreateImageDisplayMap 逻辑一致）
            bool isChartValid = chartResultSection.ChartAreas.Count > 0 && !string.IsNullOrEmpty(chartResultSection.ChartAreas[0]?.BackImage);
            if (!isChartValid)
            {
                MessageBox.Show("请先完成反演计算，生成反演结果剖面图后再操作！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 调用核心方法：生成反演结果图的展示地图（复用之前固化的逻辑）
            CreateImageDisplayMap(chartResultSection);
        }
    }
}