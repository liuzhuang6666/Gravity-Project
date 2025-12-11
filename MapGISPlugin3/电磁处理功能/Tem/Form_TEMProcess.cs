using MapGIS.G3DWorkSpace;
using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.PluginEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using System.Linq;
using MapGIS.GeoObjects;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 数据处理窗体
    /// 【复用】整体结构参考 Form_MT1di
    /// 【修改】针对 TEM 的特有逻辑：处理两个输入文件、显示发射源框
    /// 【优化】添加 COM 资源释放调试日志、对象生命周期追踪
    /// </summary>
    public partial class Form_TEMProcess : Form
    {
        private IApplication _hook;
        private List<TEMStationInfo> m_CurrentLineStations;
        private DataTable m_CurrentLineData;
        private SFeatureCls m_SelectedStationLayer;
        private SFeatureCls m_SelectedTransmitterLayer;
        private ObjectCls m_SelectedObservationTable;
        private TEMTransmitterInfo m_TransmitterInfo;
        private string m_CurrentSelectedStationName;
        private bool _isCalculationCompleted = false;
        private Process _aExeProcess = null;
        private readonly object _processLock = new object(); // 锁对象，确保线程安全
        private Point _mouseDownPoint = new Point(); // 用于记录鼠标按下时的位置                                                 
        private Dictionary<string, double> _cachedCalcResults = new Dictionary<string, double>();// 存储拟合数据：Key = "测点名_时间索引", Value = 拟合Z值
        // 【新增】用来记住最后一次计算结果文件的路径
        private string _lastCalcResultPath = null;
        private List<InversionResultPoint> m_LastInversionResults = null;
        private string _runtimeRecordPath = null;

        // 替换原有测点进度追踪变量，适配record固定1行
        private int _totalStationsToCalc = 1;
        private int _currentStationIndex = 0; // 当前正在计算的测点索引（从0开始）
        private double _stationProgressRatio = 0.0; // 每个测点的进度占比（100%/总测点）
        private double _currentSubProgress = 0.0; // 当前测点的子进度（0→0.6）
        private string _lastRecordValue = ""; // 上一次读取的record值（用于判断是否更新）
        private bool _isCurrentStationCompleted = false; // 当前测点是否已完成
        public Form_TEMProcess(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            m_CurrentLineStations = new List<TEMStationInfo>();
            m_CurrentLineData = new DataTable();
            // 初始化拖动事件（绑定标题栏panelTitle）
            InitDragEvents();
        }
        /// <summary>
        /// 初始化标题栏拖动事件
        /// </summary>
        private void InitDragEvents()
        {
            // 鼠标按下时记录位置
            panel1.MouseDown += PanelTitle_MouseDown;
            // 鼠标移动时拖动窗口
            panel1.MouseMove += PanelTitle_MouseMove;
        }
        #region --- 初始化 ---

        /// <summary>
        /// 【复用】窗体加载事件，逻辑参考 Form_MT1di
        /// </summary>
        private void Form_TEMProcess_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
            timerProgress.Interval = 1000;
            // ================= 【新增这一行】 =================
            // 清除图例，腾出空间显示曲线
            chartZProfile.Legends.Clear();
            // =================================================
        }
        /// <summary>
        /// 标题栏鼠标按下：记录鼠标相对窗口的位置
        /// </summary>
        private void PanelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // 仅响应左键
            {
                // 记录鼠标在窗口内的坐标（相对于标题栏）
                _mouseDownPoint.X = e.X;
                _mouseDownPoint.Y = e.Y;
            }
        }

        /// <summary>
        /// 标题栏鼠标移动：计算窗口新位置
        /// </summary>
        private void PanelTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // 左键按住时才拖动
            {
                // 窗口新位置 = 鼠标当前屏幕位置 - 按下时的相对位置
                this.Left = Control.MousePosition.X - _mouseDownPoint.X;
                this.Top = Control.MousePosition.Y - _mouseDownPoint.Y;
            }
        }

        /// <summary>
        /// 【复用】从地图加载图层，逻辑参考 Form_MT1di.LoadLayersFromMap
        /// 【新增】额外加载发射源图层和发射源信息
        /// 【优化】添加图层加载日志
        /// </summary>
        private void LoadLayersFromMap()
        {
            Console.WriteLine("=== 开始加载地图图层 ===");
            try
            {
                Map temMap = FindMapByName("电法数据");
                if (temMap == null)
                {
                    MessageBox.Show("未找到 '电法数据' 地图");
                    return;
                }

                int layerCount = temMap.LayerCount;
                for (int i = 0; i < layerCount; i++)
                {
                    MapLayer layer = temMap.get_Layer(i);
                    if (layer == null) continue;

                    string layerName = layer.Name ?? "";
                    Console.WriteLine($"检测到图层: {layerName}");

                    if (layer is VectorLayer vectorLayer)
                    {
                        if (layerName.Contains("测点"))
                        {
                            m_SelectedStationLayer = vectorLayer.GetData() as SFeatureCls;
                            cmbStationLayer.Items.Add(layer);
                            Console.WriteLine("测点图层加载成功");
                        }
                        else if (layerName.Contains("发射源"))
                        {
                            // 【新增】加载发射源图层
                            m_SelectedTransmitterLayer = vectorLayer.GetData() as SFeatureCls;
                            if (m_SelectedTransmitterLayer != null)
                            {
                                LoadTransmitterInfo();
                                Console.WriteLine("发射源图层加载成功，开始读取发射源信息");
                            }
                            else
                            {
                                Console.WriteLine("发射源图层数据加载失败");
                            }
                        }
                    }
                    else if (layer is ObjectLayer objectLayer)
                    {
                        if (layerName.Contains("观测数据"))
                        {
                            m_SelectedObservationTable = objectLayer.GetData() as ObjectCls;
                            Console.WriteLine("观测数据表加载成功");
                        }
                    }
                }

                cmbStationLayer.DisplayMember = "Name";
                if (cmbStationLayer.Items.Count > 0)
                {
                    cmbStationLayer.SelectedIndex = 0;
                }
                Console.WriteLine("=== 地图图层加载完成 ===");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载地图图层时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"地图图层加载错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 【修复】从数据库加载发射源信息 - 静默模式（移除弹窗）
        /// </summary>
        private void LoadTransmitterInfo()
        {
            // 初始化发射源信息
            m_TransmitterInfo = new TEMTransmitterInfo();

            // 图层检查
            if (m_SelectedTransmitterLayer == null || !m_SelectedTransmitterLayer.HasOpen())
            {
                Console.WriteLine("LoadTransmitterInfo: 发射源图层未加载或未打开");
                return;
            }

            RecordSet rs = null;
            try
            {
                rs = m_SelectedTransmitterLayer.Select(null);
                if (rs == null)
                {
                    Console.WriteLine("LoadTransmitterInfo: RecordSet 为 null");
                    return;
                }

                int totalRecords = rs.Count;
                if (totalRecords <= 0)
                {
                    Console.WriteLine("LoadTransmitterInfo: 发射源图层无记录");
                    return;
                }

                // 移动到第一条记录
                rs.MoveFirst();
                int count = 0;

                do
                {
                    count++;
                    try
                    {
                        // 检查 RecordSet 状态
                        if (rs.IsEOF) break;

                        // 读取属性
                        Record att = rs.Att;
                        if (att == null) continue;

                        // 读取id
                        object nameObj = att["点名"];
                        string pointName = nameObj?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(pointName)) continue;

                        // 读取坐标
                        double x = 0, y = 0, z = 0;
                        if (att["X坐标"] != null && att["X坐标"] != DBNull.Value)
                            double.TryParse(att["X坐标"].ToString(), out x);
                        if (att["Y坐标"] != null && att["Y坐标"] != DBNull.Value)
                            double.TryParse(att["Y坐标"].ToString(), out y);
                        if (att["Z"] != null && att["Z"] != DBNull.Value)
                            double.TryParse(att["Z"].ToString(), out z);

                        // 匹配点名并赋值
                        switch (pointName.ToUpper())
                        {
                            case "A":
                                m_TransmitterInfo.PointA_X = x;
                                m_TransmitterInfo.PointA_Y = y;
                                m_TransmitterInfo.PointA_Z = z;
                                break;
                            case "B":
                                m_TransmitterInfo.PointB_X = x;
                                m_TransmitterInfo.PointB_Y = y;
                                m_TransmitterInfo.PointB_Z = z;
                                break;
                            case "C":
                                m_TransmitterInfo.PointC_X = x;
                                m_TransmitterInfo.PointC_Y = y;
                                m_TransmitterInfo.PointC_Z = z;
                                break;
                            case "D":
                                m_TransmitterInfo.PointD_X = x;
                                m_TransmitterInfo.PointD_Y = y;
                                m_TransmitterInfo.PointD_Z = z;
                                break;
                            default:
                                // 未知点名忽略
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"LoadTransmitterInfo 单条记录错误: {ex.Message}");
                    }

                } while (rs.MoveNext() && count < totalRecords * 2); // 防止无限循环
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadTransmitterInfo 总体异常: {ex.Message}");
            }
            finally
            {
                // 资源释放
                try
                {
                    if (rs != null)
                    {
                        Marshal.ReleaseComObject(rs);
                        rs = null;
                    }
                }
                catch { /* 忽略释放错误 */ }
            }
        }
        #endregion

        #region --- 事件处理 ---

        /// <summary>
        /// 【复用】测点图层选择事件，逻辑参考 Form_MT1di
        /// 【优化】添加图层选择日志
        /// </summary>
        private void cmbStationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStationLayer.SelectedItem is MapLayer selectedLayer)
            {
                m_SelectedStationLayer = selectedLayer.GetData() as SFeatureCls;
                FillLineComboBox();
                Console.WriteLine($"已选择测点图层：{selectedLayer.Name}");
            }
        }

        /// <summary>
        /// 【复用】测线选择事件，逻辑参考 Form_MT1di.cmbLineName_SelectedIndexChanged
        /// 【优化】添加测线选择日志、COM 资源释放追踪
        /// </summary>
        private void cmbLineName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllDisplays();
            if (cmbLineName.SelectedItem == null || m_SelectedStationLayer == null || m_SelectedObservationTable == null)
            {
                return;
            }

            string selectedLine = cmbLineName.SelectedItem.ToString();
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Console.WriteLine($"=== 开始加载测线 '{selectedLine}' 数据 ===");
                m_CurrentLineStations = QueryStationsForLine(selectedLine);
                m_CurrentLineData = QueryObservationDataForLine(selectedLine);

                UpdateProfileView();
                UpdateTransmitterGrid(); // 替换原 UpdateDataGrid()

                if (m_CurrentLineStations.Count > 0)
                {
                    string firstStation = m_CurrentLineStations[0].StationName;
                    SelectStationAndRefreshCharts(firstStation);
                    InitTimeSliderAndGrids();
                    UpdateZDataGrid(null);
                }
                Console.WriteLine($"=== 测线 '{selectedLine}' 数据加载完成 ===");
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生 COM 错误: {comEx.Message}", "错误");
                ClearAllDisplays();
                Console.WriteLine($"测线加载 COM 错误：{comEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生错误: {ex.Message}", "错误");
                ClearAllDisplays();
                Console.WriteLine($"测线加载错误：{ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 【复用】小地图点击事件，逻辑参考 Form_MT1di
        /// </summary>
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

        /// <summary>
        /// 【修改】开始计算按钮事件
        /// 1. 获取界面输入的5个反演参数
        /// 2. 导出观测数据和发射源数据
        /// 3. 调用 a.exe (带8个参数)
        /// 4. 计算完成后读取 record 文件并显示拟合误差
        /// </summary>
        private async void btnCalculate_Click(object sender, EventArgs e)
        {
            // 0. 基础前置检查
            if (string.IsNullOrEmpty(m_CurrentSelectedStationName))
            {
                MessageBox.Show("请先在小地图上选择一个测点再计算。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("没有加载任何测线数据，无法计算。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (m_TransmitterInfo == null)
            {
                MessageBox.Show("未加载发射源信息，请检查发射源图层。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ==================== 【新增：读取界面参数】 ====================
            // 根据你的描述：
            // 模型光滑因子(textBox1), 反演深度(textBox2), 模型层厚度递增因子(textBox4)
            // 模型初始电阻率(textBox3), 期望拟合误差(textBox6)

            double valSmooth, valDepth, valGrowth, valInitRes, valExpError;
            try
            {
                if (!double.TryParse(textBox1.Text, out valSmooth)) throw new Exception("模型光滑因子(textBox1)必须为数字");
                if (!double.TryParse(textBox2.Text, out valDepth)) throw new Exception("反演深度(textBox2)必须为数字");
                if (!double.TryParse(textBox4.Text, out valGrowth)) throw new Exception("模型层厚度递增因子(textBox4)必须为数字");
                if (!double.TryParse(textBox3.Text, out valInitRes)) throw new Exception("模型初始电阻率(textBox3)必须为数字");
                if (!double.TryParse(textBox6.Text, out valExpError)) throw new Exception("期望拟合误差(textBox6)必须为数字");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"参数输入格式错误：\n{ex.Message}", "参数检查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // ==============================================================

            // 初始化进度条
            progressBarCalculate.Visible = true;
            progressBarCalculate.Value = 0;
            _isCalculationCompleted = false;
            textBox5.Text = ""; // 清空之前的误差显示

            string tempKnowedFile = null;
            string tempTranFile = null;
            string exePath = null;
            string pluginDir = null;
            string workspaceName = null;
            string fullWorkspacePath = null;

            try
            {
                // 1. 路径初始化与校验
                pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(pluginDir))
                    throw new Exception("无法获取插件目录，请检查程序部署。");

                string algorithmDir = Path.Combine(pluginDir, "Algorithm", "TEM1di");
                exePath = Path.Combine(algorithmDir, "a.exe");
                if (!File.Exists(exePath))
                    throw new FileNotFoundException($"计算程序 'a.exe' 未找到！\n预期路径: {exePath}");

                // 2. 生成工作空间 (6字符)
                string selectedLine = cmbLineName.SelectedItem?.ToString() ?? "TEM";
                string linePrefix = selectedLine.Length >= 3 ? selectedLine.Substring(0, 3) : selectedLine.PadRight(3, '0');
                string randomPart = Path.GetRandomFileName().Replace(".", "").Substring(0, 3);
                workspaceName = $"{linePrefix}{randomPart}".ToUpper();

                

                // 3. 生成临时文件并导出数据
                tempKnowedFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_knowed.dat");
                tempTranFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_tran.dat");

                ExportObservationDataToFile(tempKnowedFile);
                ExportTransmitterInfoToFile(tempTranFile);

                if (new FileInfo(tempKnowedFile).Length == 0) throw new Exception($"观测数据文件导出为空");
                if (new FileInfo(tempTranFile).Length == 0) throw new Exception($"发射源文件导出为空");

                // 4. 【修改：构造命令行参数】
                // 格式: a.exe knowed tran workspace smooth depth growth initRes expError
                string arguments = $"\"{tempKnowedFile}\" \"{tempTranFile}\" \"{workspaceName}\" {valSmooth} {valDepth} {valGrowth} {valInitRes} {valExpError}";
                // ==================== 初始化进度状态（适配固定1行record） ====================
                if (m_CurrentLineStations != null && m_CurrentLineStations.Count > 0)
                {
                    _totalStationsToCalc = m_CurrentLineStations.Count;
                    _stationProgressRatio = 100.0 / _totalStationsToCalc; // 每个测点的进度占比
                    _currentStationIndex = 0; // 从第1个测点开始（索引0）
                    _currentSubProgress = 0.0; // 子进度重置为0
                    _lastRecordValue = ""; // 清空上一次record值
                    _isCurrentStationCompleted = false; // 当前测点未完成
                }
                else
                {
                    _totalStationsToCalc = 1;
                    _stationProgressRatio = 100.0;
                    _currentStationIndex = 0;
                    _currentSubProgress = 0.0;
                    _lastRecordValue = "";
                    _isCurrentStationCompleted = false;
                }

                // 2. 初始化进度条
                progressBarCalculate.Visible = true;
                progressBarCalculate.Value = 0; // 从0开始
                progressBarCalculate.Maximum = 100; //以此保持百分比显示

                // 3. 设置实时读取路径 (同上一步建议)
                fullWorkspacePath = Path.Combine(pluginDir, workspaceName);
                _runtimeRecordPath = Path.Combine(fullWorkspacePath, "record");

                // 4. 清空之前的误差显示
                textBox5.Text = "准备计算...";
                // 启动进度条定时器
                timerProgress.Start();
                
                // 5. 异步执行 a.exe
                bool isSuccess = await Task.Run(() =>
                {
                    using (Process process = new Process())
                    {
                        // 加锁赋值，确保 UI 线程能获取到进程引用(用于强制停止)
                        lock (_processLock)
                        {
                            _aExeProcess = process;
                        }

                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = arguments,
                            WorkingDirectory = pluginDir, // 工作目录设为插件目录
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        };

                        process.Start();
                        //const int timeoutMs = 30 * 60 * 1000; // 30分钟超时
                        //if (!process.WaitForExit(timeoutMs))
                        //{
                        //    process.Kill();
                        //    throw new Exception($"a.exe 运行超时（超过 {timeoutMs / 1000} 秒），已强制终止。");
                        //}

                        // 读取输出
                        string error = process.StandardError.ReadToEnd();

                        // 如果有标准错误输出，在主线程弹窗提示
                        if (!string.IsNullOrEmpty(error))
                        {
                            this.Invoke((Action)(() =>
                            {
                                MessageBox.Show($"警告信息：\n{error}", "计算警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }));
                        }

                        if (process.ExitCode != 0)
                            throw new Exception($"a.exe 运行失败（退出码：{process.ExitCode}）\n{error}");

                        return true;
                    }
                });

                // 6. 计算后处理
                _isCalculationCompleted = true;
                timerProgress.Stop();
                progressBarCalculate.Value = 100;

                if (isSuccess)
                {
                    // ==================== 【修改：读取 record 文件显示误差】 ====================
                    // 假设输出的 record 文件名为 "record" 且位于生成的工作空间内
                    string recordFilePath = Path.Combine(fullWorkspacePath, "record");
                    // 刷新数据列表（显示拟合值）
                    string calcResultPath = Path.Combine(fullWorkspacePath, "calc_z.dat");
                    // 【新增】把路径存下来，给点击事件用
                    _lastCalcResultPath = calcResultPath;
                    // --- 读取数据到缓存 ---
                    _cachedCalcResults.Clear();
                    if (File.Exists(calcResultPath))
                    {
                        var lines = File.ReadAllLines(calcResultPath);
                        foreach (var line in lines)
                        {
                            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 3)
                            {
                                // Key 格式: "S1_0" (测点S1的第0个时间道)
                                string key = $"{parts[0].Trim()}_{parts[1].Trim()}";
                                if (double.TryParse(parts[2], out double v)) _cachedCalcResults[key] = v;
                            }
                        }
                    }
                    // ---------------------

                    // 更新 Z 数据表
                    UpdateZDataGrid(calcResultPath);

                    // ---【开始修改成图逻辑】---
                    string knowResultPath = Path.Combine(fullWorkspacePath, "KNOW");

                    // 1. 解析结果文件到内存
                    m_LastInversionResults = ParseKnowFile(knowResultPath);

                    // 2. 调用新的成图方法
                    DisplayInversionResults(m_LastInversionResults);
                    // ---【修改结束】---

                    UpdateRightPanelCharts(); // 刷新一下曲线
                    TrackBarTime_Scroll(null, null); // 刷新一下Z剖面

                    if (File.Exists(recordFilePath))
                    {
                        try
                        {
                            // 1. 读取文件内容，例如 "1 1 0.154632"
                            string fileContent = File.ReadAllText(recordFilePath, Encoding.Default).Trim();

                            // 2. 按空格或Tab分割字符串
                            string[] parts = fileContent.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            // 3. 提取我们需要的值
                            // 如果格式固定是 "数字 数字 误差"，我们可以取最后一个值，或者取第3个值(索引2)
                            if (parts.Length >= 3)
                            {
                                // 取第3个值 (0.154632)
                                textBox5.Text = parts[2];
                            }
                            else if (parts.Length > 0)
                            {
                                // 如果格式不对，尝试取最后一个值作为兜底
                                textBox5.Text = parts[parts.Length - 1];
                            }
                            else
                            {
                                textBox5.Text = fileContent; // 分割失败，显示原始内容
                            }
                        }
                        catch (Exception readEx)
                        {
                            textBox5.Text = "读取失败";
                            Console.WriteLine($"读取 record 文件失败: {readEx.Message}");
                        }
                    }
                    else
                    {
                        textBox5.Text = "无结果";
                        Console.WriteLine($"未找到 record 文件: {recordFilePath}");
                    }
                    // ========================================================================

                    if (!Directory.Exists(fullWorkspacePath))
                    {
                        MessageBox.Show($"计算成功，但未找到结果目录！\n预期路径：{fullWorkspacePath}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"计算成功！\n结果已保存到：\n{fullWorkspacePath}", "计算完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 询问是否打开文件夹
                        if (MessageBox.Show("是否立即打开结果目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Process.Start("explorer.exe", fullWorkspacePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                timerProgress.Stop();
                progressBarCalculate.Value = 0;
                MessageBox.Show($"计算失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 7. 清理临时文件
                if (!string.IsNullOrEmpty(tempKnowedFile) && File.Exists(tempKnowedFile))
                {
                    try { File.Delete(tempKnowedFile); } catch { }
                }
                if (!string.IsNullOrEmpty(tempTranFile) && File.Exists(tempTranFile))
                {
                    try { File.Delete(tempTranFile); } catch { }
                }

                // 清空进程引用
                lock (_processLock)
                {
                    _aExeProcess = null;
                }
                _runtimeRecordPath = null;

                // 确保最后如果是异常退出，进度条归零；如果是正常完成，保持100
                if (!_isCalculationCompleted)
                {
                    progressBarCalculate.Value = 0;
                }
                _currentStationIndex = 0;
                _stationProgressRatio = 0.0;
                _currentSubProgress = 0.0;
                _lastRecordValue = "";
                _isCurrentStationCompleted = false;
            }
        }
        
        /// <summary>
        /// 定时器事件：逐步更新进度条
        /// </summary>
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            // 如果计算已经标记完成，直接拉满
            if (_isCalculationCompleted)
            {
                progressBarCalculate.Value = 100;
                return;
            }

            // 检查文件是否存在
            if (string.IsNullOrEmpty(_runtimeRecordPath) || !File.Exists(_runtimeRecordPath))
            {
                // 未生成record文件：推进当前测点的子进度（0→60%区间）
                UpdateSubProgress();
                // 计算当前显示进度
                double currentProgress = CalculateCurrentDisplayProgress();
                progressBarCalculate.Value = (int)Math.Min(100, Math.Max(0, currentProgress));
                return;
            }

            try
            {
                // 读取record文件（固定1行3个数）
                using (FileStream fs = new FileStream(_runtimeRecordPath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                {
                    string currentRecord = sr.ReadToEnd().Trim(); // 读取当前record值（如"1 1 0.154632"）

                    // --------------------- 核心：判断是否有新测点完成 ---------------------
                    if (!string.IsNullOrEmpty(currentRecord) && currentRecord != _lastRecordValue)
                    {
                        // record值变化 → 当前测点完成
                        _isCurrentStationCompleted = true;
                        _lastRecordValue = currentRecord; // 更新上一次record值

                        // 切换到下一个测点（如果还有未计算的）
                        if (_currentStationIndex < _totalStationsToCalc - 1)
                        {
                            _currentStationIndex++; // 下一个测点索引
                            _currentSubProgress = 0.0; // 重置子进度
                            _isCurrentStationCompleted = false; // 新测点未完成
                        }
                        else
                        {
                            // 所有测点完成（最后一个测点）
                            _isCalculationCompleted = true;
                        }
                    }
                    // --------------------------------------------------------------------

                    // 1. 推进子进度（当前测点未完成时）
                    if (!_isCurrentStationCompleted)
                    {
                        UpdateSubProgress();
                    }

                    // 2. 计算当前应显示的进度
                    double currentDisplayProgress = CalculateCurrentDisplayProgress();
                    progressBarCalculate.Value = (int)Math.Min(100, Math.Max(0, currentDisplayProgress));

                    // 3. 更新拟合误差（保持原有逻辑，取最后一个数）
                    if (!string.IsNullOrEmpty(currentRecord))
                    {
                        string[] parts = currentRecord.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            textBox5.Text = parts[2]; // 取第3个数作为误差
                        }
                        else if (parts.Length > 0)
                        {
                            textBox5.Text = parts[parts.Length - 1]; // 兜底取最后一个数
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 读取失败时，继续推进子进度，避免进度条停滞
                if (!_isCurrentStationCompleted)
                {
                    UpdateSubProgress();
                }
                double currentDisplayProgress = CalculateCurrentDisplayProgress();
                progressBarCalculate.Value = (int)Math.Min(100, Math.Max(0, currentDisplayProgress));
            }
        }

        // 新增：计算当前显示的总进度
        private double CalculateCurrentDisplayProgress()
        {
            // 已完成的测点总进度（每个完成的测点占满整个区间）
            double finishedProgress = _currentStationIndex * _stationProgressRatio;

            // 当前测点的进度（未完成→子进度60%；已完成→100%）
            double currentStationProgress = 0.0;
            if (_isCurrentStationCompleted)
            {
                // 当前测点已完成→占满整个区间
                currentStationProgress = _stationProgressRatio;
            }
            else
            {
                // 当前测点未完成→推进到区间的60%
                currentStationProgress = _stationProgressRatio * _currentSubProgress;
            }

            // 总进度 = 已完成测点进度 + 当前测点进度
            return finishedProgress + currentStationProgress;
        }

        // 保留之前的子进度推进方法（20分钟走到60%）
        private void UpdateSubProgress()
        {
            double totalDurationSec = 20 * 60; // 单个测点计算时长（20分钟=1200秒）
            double targetSubProgress = 0.6; // 目标：区间的60%
            double stepPerTick = targetSubProgress / totalDurationSec; // 每秒推进步长（0.6/1200=0.0005）

            // 子进度不超过0.6
            if (_currentSubProgress < targetSubProgress)
            {
                _currentSubProgress += stepPerTick;
                if (_currentSubProgress > targetSubProgress)
                {
                    _currentSubProgress = targetSubProgress;
                }
            }
        }


        #endregion

        #region --- 数据导出（新增） ---

        /// <summary>
                /// 【修正版】根据您的截图直接导出数据
                /// 修正点：
                /// 1. 直接读取表中的 "X" 和 "Y" 列
                /// 2. 修正了带有空格的列名 "采样时间 us" 和 "感应电压 mV"
                /// </summary>
        private void ExportObservationDataToFile(string filePath)
        {
            // 检查是否有数据
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("当前测线无数据，无法导出。", "提示");
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // 写入表头 (根据 a.exe 要求的格式)
                writer.WriteLine("PROFILE\tSTATION\tCOORX\tCOORY\tTIMES\tRAREA\tCHZ");

                foreach (DataRow row in m_CurrentLineData.Rows)
                {
                    try
                    {
                        // 1. 读取基础信息
                        string lineName = row["测线号"].ToString();
                        string stationName = row["测点号"].ToString();

                        // 2. 读取坐标 (根据截图，列名是 X 和 Y)
                        // 使用 GetRowValue 辅助方法防止列名对不上报错
                        double x = GetRowValue(row, "X", "X坐标");
                        double y = GetRowValue(row, "Y", "Y坐标");

                        // 3. 读取物理量 (根据截图，注意空格)
                        // 优先尝试 "采样时间 us"，如果失败尝试 "采样时间_us"
                        double samplingTime = GetRowValue(row, "采样时间 us", "采样时间_us");

                        // 优先尝试 "有效面积"
                        double area = GetRowValue(row, "有效面积", "有效面积");
                        if (area == 0) area = 1.0; // 防止面积为0导致计算错误

                        // 优先尝试 "感应电压 mV"
                        double voltage = GetRowValue(row, "感应电压 mV", "感应电压_mV");

                        // 4. 写入文件
                        writer.WriteLine($"{lineName} {stationName} {x} {y} {samplingTime} {area} {voltage}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"导出数据行异常: {ex.Message}");
                        // 可以选择 continue 跳过坏行，或者 throw 停止
                    }
                }
            }
        }

        /// <summary>
        /// 辅助函数：尝试获取列值，支持别名（防止空格/下划线差异）
        /// </summary>
        private double GetRowValue(DataRow row, string colName1, string colName2)
        {
            object val = null;
            if (row.Table.Columns.Contains(colName1))
            {
                val = row[colName1];
            }
            else if (row.Table.Columns.Contains(colName2))
            {
                val = row[colName2];
            }

            if (val != null && val != DBNull.Value)
            {
                if (double.TryParse(val.ToString(), out double result))
                {
                    return result;
                }
            }
            return 0.0;
        }

        /// <summary>
        /// 【新增】导出发射源信息到文件（TEM 特有）
        /// 格式：
        /// 第 1 行：A 点的 X, Y, Z
        /// 第 2 行：B 点的 X, Y, Z
        /// 第 3 行：C 点的 X, Y, Z
        /// 第 4 行：D 点的 X, Y, Z
        /// </summary>
        private void ExportTransmitterInfoToFile(string filePath)
        {
            if (m_TransmitterInfo == null) return;

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"{m_TransmitterInfo.PointA_X} {m_TransmitterInfo.PointA_Y} {m_TransmitterInfo.PointA_Z}");
                writer.WriteLine($"{m_TransmitterInfo.PointB_X} {m_TransmitterInfo.PointB_Y} {m_TransmitterInfo.PointB_Z}");
                writer.WriteLine($"{m_TransmitterInfo.PointC_X} {m_TransmitterInfo.PointC_Y} {m_TransmitterInfo.PointC_Z}");
                writer.WriteLine($"{m_TransmitterInfo.PointD_X} {m_TransmitterInfo.PointD_Y} {m_TransmitterInfo.PointD_Z}");
            }
        }

        #endregion

        #region --- 数据查询 ---

        /// <summary>
        /// 【复用】填充测线下拉框，逻辑参考 Form_MT1di.FillLineComboBox
        /// 【优化】添加下拉框填充日志、COM 资源释放追踪
        /// </summary>
        private void FillLineComboBox()
        {
            Console.WriteLine("=== 开始填充测线下拉框 ===");
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
            int validRecordCount = 0;

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
                    MessageBox.Show("Select(null) 返回 null。尝试用 QueryDef(Filter='')...");
                    QueryDef query = new QueryDef();
                    query.Filter = "";
                    query.WithSpatial = false;
                    rs = m_SelectedStationLayer.Select(query);
                    if (rs == null)
                    {
                        MessageBox.Show("Select(QueryDef) 也返回 null。检查数据源 URL 或 HasOpen()！");
                        return;
                    }
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

                if (totalRecords == 0)
                {
                    Console.WriteLine("FillLineComboBox: totalRecords 为 0，循环前退出。");
                    return;
                }

                do
                {
                    Record currentAtt = null;
                    try
                    {
                        currentIndex++;
                        currentAtt = rs.Att;
                        if (currentAtt != null)
                        {
                            object value = currentAtt["测线号"];
                            if (value != null && value != DBNull.Value)
                            {
                                string lineName = value.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(lineName))
                                {
                                    uniqueLines.Add(lineName);
                                    validRecordCount++;
                                }
                            }
                        }

                        if (currentIndex > rs.Count * 1.5 && rs.Count > 0)
                        {
                            MessageBox.Show($"迭代超过预期 ({currentIndex} > {rs.Count})，强制停止循环");
                            break;
                        }
                    }
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message}");
                    }
                    finally
                    {
                        if (currentAtt != null)
                        {
                            try { Marshal.ReleaseComObject(currentAtt); }
                            catch { Console.WriteLine("测线属性释放异常"); }
                        }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);

                if (uniqueLines.Count > 0)
                {
                    var sortedLines = uniqueLines.OrderBy(s => s).ToArray();
                    cmbLineName.Items.AddRange(sortedLines);
                }
                else
                {
                    MessageBox.Show($"查询未返回任何唯一值。（有效记录: {validRecordCount}）");
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
                if (rs != null)
                {
                    try { Marshal.ReleaseComObject(rs); }
                    catch { Console.WriteLine("测线 RecordSet 释放异常"); }
                }

                this.Cursor = Cursors.Default;
                cmbLineName.Enabled = (uniqueLines.Count > 0);
            }
            Console.WriteLine("=== 测线下拉框填充完成 ===");
        }

        /// <summary>
        /// 【复用】查询测线的测点，逻辑参考 Form_MT1di.QueryStationsForLine
        /// 【修改】返回类型为 List<TEMStationInfo>
        /// 【优化】添加测点查询日志、COM 资源释放追踪
        /// </summary>
        private List<TEMStationInfo> QueryStationsForLine(string lineName)
        {
            Console.WriteLine($"=== 开始查询测线 '{lineName}' 的测点 ===");
            var stations = new List<TEMStationInfo>();
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
                    MessageBox.Show($"查询失败：Select 返回 null");
                    return stations;
                }

                if (rs.Count == 0)
                {
                    MessageBox.Show($"查询无结果！\nFilter: {query.Filter}");
                    return stations;
                }

                rs.MoveLast();
                rs.MoveFirst();

                int totalRecords = rs.Count;
                int geomFailCount = 0;
                int currentIndex = 0;

                if (totalRecords == 0)
                {
                    return stations;
                }

                do
                {
                    currentIndex++;
                    if (currentIndex > totalRecords * 1.5 && totalRecords > 0)
                    {
                        MessageBox.Show($"迭代超过预期 ({currentIndex} > {totalRecords})，强制停止循环。");
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
                                    if (firstDot != null) try { Marshal.ReleaseComObject(firstDot); firstDot = null; } catch { }
                                    if (geomBase != null) try { Marshal.ReleaseComObject(geomBase); geomBase = null; } catch { }
                                    if (att != null) try { Marshal.ReleaseComObject(att); att = null; } catch { }
                                }
                            }
                        }

                        if (!geomSuccess)
                        {
                            geomFailCount++;
                            continue;
                        }

                        att = rs.Att;
                        if (att == null)
                        {
                            continue;
                        }

                        object val = null;
                        try
                        {
                            val = att["测点号"];
                        }
                        catch (Exception fieldEx)
                        {
                            MessageBox.Show($"字段名错误：在遍历时无法获取 '测点号' 字段！\n错误: {fieldEx.Message}", "致命错误");
                            break;
                        }

                        string stationName = val?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(stationName))
                        {
                            continue;
                        }

                        stations.Add(new TEMStationInfo
                        {
                            LineName = lineName,
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
                MessageBox.Show($"异常：{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (rs != null)
                {
                    try { Marshal.ReleaseComObject(rs); }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"释放 RecordSet (rs) 时出错: {ex.Message}");
                    }
                }
            }
            Console.WriteLine($"=== 测线 '{lineName}' 的测点查询完成，共 {stations.Count} 个测点 ===");
            return stations.OrderBy(s => s.X).ToList();
        }

        /// <summary>
        /// 【复用】查询观测数据，逻辑参考 Form_MT1di.QuerySoundingDataForLine
        /// 【修改】查询 TEM 观测表，字段名不同
        /// 【优化】添加观测数据查询日志、COM 资源释放追踪
        /// </summary>
        private DataTable QueryObservationDataForLine(string lineName)
        {
            Console.WriteLine($"=== 开始查询测线 '{lineName}' 的观测数据 ===");
            DataTable dataTable = new DataTable();
            if (m_SelectedObservationTable == null || !m_SelectedObservationTable.HasOpen())
            {
                Console.WriteLine("QueryObservationDataForLine: m_SelectedObservationTable 为 null 或未打开。");
                return dataTable;
            }

            RecordSet rs = null;
            Fields fields = null;
            try
            {
                QueryDef query = new QueryDef
                {
                    Filter = $"测线号 = '{lineName}'",
                    SubFields2 = "*"
                };

                rs = m_SelectedObservationTable.Select(query);
                if (rs == null)
                {
                    MessageBox.Show("QueryObservationDataForLine: Select(query) 返回 null，查询失败。", "错误");
                    return dataTable;
                }

                try
                {
                    rs.MoveLast();
                    rs.MoveFirst();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"QueryObservationDataForLine: 预加载 (MoveLast) 失败: {ex.Message}。", "警告");
                }

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

                int currentIndex = 0;
                int totalRecords = rs.Count;

                if (totalRecords == 0)
                {
                    Console.WriteLine("QueryObservationDataForLine: totalRecords 为 0，循环前退出。");
                    return dataTable;
                }

                do
                {
                    Record att = null;
                    try
                    {
                        currentIndex++;
                        if (currentIndex > totalRecords * 1.5 && rs.Count > 0)
                        {
                            MessageBox.Show($"迭代超过预期 ({currentIndex} > {rs.Count})，强制停止循环。");
                            break;
                        }

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
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message}");
                    }
                    finally
                    {
                        if (att != null) { try { Marshal.ReleaseComObject(att); } catch { } }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"QueryObservationDataForLine 捕获到异常: {ex.Message}", "致命错误");
            }
            finally
            {
                if (fields != null) { try { Marshal.ReleaseComObject(fields); } catch { } }
                if (rs != null) { try { Marshal.ReleaseComObject(rs); } catch { } }
            }
            Console.WriteLine($"=== 测线 '{lineName}' 的观测数据查询完成，共 {dataTable.Rows.Count} 条记录 ===");
            return dataTable;
        }

        #endregion

        #region --- 可视化 ---

        /// <summary>
        /// 【复用】更新小地图，逻辑参考 Form_MT1di.UpdateProfileView
        /// 【新增】绘制发射源矩形框（TEM 特有）
        /// 【优化】手动设置坐标轴范围，解决点挤成一坨的问题
        /// </summary>
        private void UpdateProfileView()
        {
            Console.WriteLine("=== 开始更新小地图 ===");
            if (chartProfileView == null) return;
            chartProfileView.Series.Clear();

            // 1. 绘制测点系列，并绑定图例
            Series stationSeries = chartProfileView.Series.Add("Stations");
            stationSeries.ChartType = SeriesChartType.Point;
            stationSeries.MarkerStyle = MarkerStyle.Circle;
            stationSeries.MarkerSize = 8;
            stationSeries.MarkerColor = System.Drawing.Color.Blue;
            stationSeries.IsValueShownAsLabel = true;
            stationSeries.LegendText = "测点";


            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                Console.WriteLine("UpdateProfileView: m_CurrentLineStations 为空，不绘制。");
                return;
            }

            // 计算测点和发射源的坐标范围（X、Y 轴的最小/最大值）
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            // 遍历测点，更新坐标范围
            foreach (var station in m_CurrentLineStations)
            {
                minX = Math.Min(minX, station.X);
                maxX = Math.Max(maxX, station.X);
                minY = Math.Min(minY, station.Y);
                maxY = Math.Max(maxY, station.Y);

                int pointIndex = stationSeries.Points.AddXY(station.X, station.Y);
                stationSeries.Points[pointIndex].Label = station.StationName;
                stationSeries.Points[pointIndex].Tag = station.StationName;
            }

            // 遍历发射源，更新坐标范围
            if (m_TransmitterInfo != null)
            {
                minX = Math.Min(minX, m_TransmitterInfo.PointA_X);
                maxX = Math.Max(maxX, m_TransmitterInfo.PointA_X);
                minY = Math.Min(minY, m_TransmitterInfo.PointA_Y);
                maxY = Math.Max(maxY, m_TransmitterInfo.PointA_Y);

                minX = Math.Min(minX, m_TransmitterInfo.PointB_X);
                maxX = Math.Max(maxX, m_TransmitterInfo.PointB_X);
                minY = Math.Min(minY, m_TransmitterInfo.PointB_Y);
                maxY = Math.Max(maxY, m_TransmitterInfo.PointB_Y);

                minX = Math.Min(minX, m_TransmitterInfo.PointC_X);
                maxX = Math.Max(maxX, m_TransmitterInfo.PointC_X);
                minY = Math.Min(minY, m_TransmitterInfo.PointC_Y);
                maxY = Math.Max(maxY, m_TransmitterInfo.PointC_Y);

                minX = Math.Min(minX, m_TransmitterInfo.PointD_X);
                maxX = Math.Max(maxX, m_TransmitterInfo.PointD_X);
                minY = Math.Min(minY, m_TransmitterInfo.PointD_Y);
                maxY = Math.Max(maxY, m_TransmitterInfo.PointD_Y);

                // 2. 绘制发射源系列，并绑定图例
                Series tranSeries = chartProfileView.Series.Add("Transmitter");
                tranSeries.ChartType = SeriesChartType.Line;
                tranSeries.MarkerStyle = MarkerStyle.Square;
                tranSeries.MarkerSize = 10;
                tranSeries.MarkerColor = System.Drawing.Color.Blue;
                tranSeries.Color = System.Drawing.Color.Blue;
                tranSeries.BorderWidth = 2;
                tranSeries.LegendText = "发射源框";

                // 绘制矩形：A-B-C-D-A
                tranSeries.Points.AddXY(m_TransmitterInfo.PointA_X, m_TransmitterInfo.PointA_Y);
                tranSeries.Points.AddXY(m_TransmitterInfo.PointB_X, m_TransmitterInfo.PointB_Y);
                tranSeries.Points.AddXY(m_TransmitterInfo.PointC_X, m_TransmitterInfo.PointC_Y);
                tranSeries.Points.AddXY(m_TransmitterInfo.PointD_X, m_TransmitterInfo.PointD_Y);
                tranSeries.Points.AddXY(m_TransmitterInfo.PointA_X, m_TransmitterInfo.PointA_Y);
            }

            // 手动设置坐标轴范围（给数据留一定的边距，避免紧贴图表边缘）
            double margin = (maxX - minX) * 0.1; // 边距为数据范围的 10%
            chartProfileView.ChartAreas[0].AxisX.Minimum = minX - margin;
            chartProfileView.ChartAreas[0].AxisX.Maximum = maxX + margin;
            chartProfileView.ChartAreas[0].AxisX.LabelStyle.Format = "F0";

            margin = (maxY - minY) * 0.1;
            chartProfileView.ChartAreas[0].AxisY.Minimum = minY - margin;
            chartProfileView.ChartAreas[0].AxisY.Maximum = maxY + margin;
            chartProfileView.ChartAreas[0].AxisY.LabelStyle.Format = "F0";

            chartProfileView.ChartAreas[0].RecalculateAxesScale();
            Console.WriteLine("=== 小地图更新完成 ===");
        }

        /// <summary>
        /// 【复用】更新数据表格，逻辑参考 Form_MT1di.UpdateDataGrids
        /// </summary>
        private void UpdateDataGrid()
        {
            if (m_CurrentLineData == null)
            {
                gridData.DataSource = null;
                return;
            }

            try
            {
                DataView dv = new DataView(m_CurrentLineData);
                gridData.DataSource = dv.ToTable(false, "测点号", "采样时间_us", "有效面积", "感应电压_mV");
                gridData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"填充数据表格时出错: {ex.Message}\n\n请检查字段名是否正确。", "表格错误");
            }
        }

        /// <summary>
        /// 【复用】选中测点并刷新图表，逻辑参考 Form_MT1di.SelectStationAndRefreshCharts
        /// 【优化】添加测点选择日志
        /// </summary>
        private void SelectStationAndRefreshCharts(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName)) return;

            m_CurrentSelectedStationName = stationName;
            Console.WriteLine($"已选中测点: {stationName}");

            try
            {
                if (chartProfileView.Series.Count > 0)
                {
                    foreach (var point in chartProfileView.Series["Stations"].Points)
                    {
                        if (point.Tag?.ToString() == stationName)
                        {
                            point.MarkerColor = System.Drawing.Color.Red;
                            point.MarkerSize = 12;
                        }
                        else
                        {
                            point.MarkerColor = System.Drawing.Color.Blue;
                            point.MarkerSize = 8;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"高亮小地图测点时出错: {ex.Message}");
            }

            UpdateRightPanelCharts();
            // 【新增】这行代码！更新右下角的表格
            // 如果 _lastCalcResultPath 是 null（还没计算），表格就只显示实测数据
            // 如果有路径（算过了），表格就会显示实测+拟合数据
            UpdateZDataGrid(_lastCalcResultPath);
        }

        /// <summary>
        /// 【修改】更新表格以显示发射源的 X、Y、Z 信息
        /// 【优化】内容完全自适应控件大小，无滚动条
        /// </summary>
        private void UpdateTransmitterGrid()
        {
            Console.WriteLine("=== 开始更新发射源表格 ===");
            if (gridData == null) return;

            // 1. 基础清理
            gridData.DataSource = null;
            gridData.Columns.Clear();
            gridData.Rows.Clear(); // 确保行也清空

            // 2. 设置表格样式：隐藏滚动条、列宽填充
            gridData.ScrollBars = ScrollBars.None; // 隐藏滚动条
            gridData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // 列宽自动填充整个控件宽度
            gridData.AllowUserToResizeRows = false; // 禁止用户调整行高
            gridData.AllowUserToResizeColumns = false; // 禁止用户调整列宽
            gridData.RowHeadersVisible = false; // 隐藏最左侧的行头（可选，为了更美观）

            // 3. 添加列
            gridData.Columns.Add("colPointName", "点名");
            gridData.Columns.Add("colX", "X坐标");
            gridData.Columns.Add("colY", "Y坐标");
            gridData.Columns.Add("colZ", "Z");

            if (m_TransmitterInfo == null)
            {
                gridData.Rows.Add("提示", "未加载数据", "", "");
                return;
            }

            // 4. 添加数据行
            gridData.Rows.Add("A",
        m_TransmitterInfo.PointA_X.ToString("F2"),
        m_TransmitterInfo.PointA_Y.ToString("F2"),
        m_TransmitterInfo.PointA_Z.ToString("F2"));
            gridData.Rows.Add("B",
              m_TransmitterInfo.PointB_X.ToString("F2"),
              m_TransmitterInfo.PointB_Y.ToString("F2"),
              m_TransmitterInfo.PointB_Z.ToString("F2"));
            gridData.Rows.Add("C",
              m_TransmitterInfo.PointC_X.ToString("F2"),
              m_TransmitterInfo.PointC_Y.ToString("F2"),
              m_TransmitterInfo.PointC_Z.ToString("F2"));
            gridData.Rows.Add("D",
              m_TransmitterInfo.PointD_X.ToString("F2"),
              m_TransmitterInfo.PointD_Y.ToString("F2"),
              m_TransmitterInfo.PointD_Z.ToString("F2"));

            // 5. 【核心】动态计算行高，使其填满垂直空间
            // 计算逻辑：(控件总高度 - 标题栏高度) / 行数
            int headerHeight = gridData.ColumnHeadersHeight;
            int clientHeight = gridData.ClientSize.Height; // 使用 ClientSize 获取除去边框后的内部高度
            int availableHeight = clientHeight - headerHeight;
            int rowCount = gridData.Rows.Count;

            if (rowCount > 0 && availableHeight > 0)
            {
                int rowHeight = availableHeight / rowCount;
                foreach (DataGridViewRow row in gridData.Rows)
                {
                    row.Height = rowHeight;
                }
            }

            // 移除之前的固定列宽代码
            // gridData.Columns["colPointName"].Width = 50; ... (已删除)

            Console.WriteLine("=== 发射源表格更新完成 ===");
        }

        // 辅助类：存储发射源点的属性
        public class TransmitterPoint
        {
            public string id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        /// <summary>
        /// 【新增】更新右栏曲线图（TEM 特有）
        /// 显示采样时间 vs 感应电压
        /// 【优化】添加曲线更新日志
        /// </summary>
        private void UpdateRightPanelCharts()
        {
            Console.WriteLine("=== 开始更新感应电压曲线 ===");
            chartVoltage.Series.Clear();



            if (string.IsNullOrEmpty(m_CurrentSelectedStationName) || m_CurrentLineData == null)
            {
                return;
            }

            DataView dvStation = new DataView(m_CurrentLineData);
            try
            {
                dvStation.RowFilter = $"测点号 = '{m_CurrentSelectedStationName}'";
                dvStation.Sort = "采样时间_us ASC";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"筛选测点出错: {ex.Message}", "错误");
                return;
            }

            if (dvStation.Count == 0)
            {
                Console.WriteLine($"未找到测点数据: {m_CurrentSelectedStationName}");
                return;
            }

            // 绘制感应电压系列，并绑定图例
            var voltageSeries = chartVoltage.Series.Add("感应电压");
            voltageSeries.ChartType = SeriesChartType.Spline;
            voltageSeries.MarkerStyle = MarkerStyle.Circle;
            voltageSeries.MarkerSize = 5;
            voltageSeries.BorderWidth = 2;


            List<double> times = new List<double>();
            List<double> voltages = new List<double>();

            foreach (DataRowView row in dvStation)
            {
                if (row["采样时间_us"] == DBNull.Value || row["感应电压_mV"] == DBNull.Value)
                    continue;

                double time = Convert.ToDouble(row["采样时间_us"]);
                double voltage = Convert.ToDouble(row["感应电压_mV"]);

                times.Add(time);
                voltages.Add(voltage);

                voltageSeries.Points.AddXY(time, voltage);
            }

            if (times.Count == 0)
            {
                Console.WriteLine("无有效数据点，跳过绘制。");
                return;
            }

            chartVoltage.ChartAreas[0].AxisX.Title = "采样时间 (μs)";
            chartVoltage.ChartAreas[0].AxisY.Title = "感应电压 (mV)";
            chartVoltage.ChartAreas[0].RecalculateAxesScale();
            Console.WriteLine("=== 感应电压曲线更新完成 ===");
        }

        /// <summary>
        /// 【复用】清空所有显示，逻辑参考 Form_MT1di.ClearAllDisplays
        /// 【优化】添加清空操作日志、COM 资源释放追踪
        /// </summary>
        private void ClearAllDisplays()
        {
            Console.WriteLine("=== 开始清空所有显示 ===");
            if (chartResistivity.Series != null) chartResistivity.Series.Clear();
            if (chartProfileView.Series != null) chartProfileView.Series.Clear();
            if (chartVoltage.Series != null) chartVoltage.Series.Clear();
            // 【新增这行】清空 Z 剖面图和颜色缓存
            if (chartZProfile.Series != null) chartZProfile.Series.Clear();
            _seriesOriginalColors.Clear();

            if (chartProfileView.ChartAreas.Count > 0)
            {
                chartProfileView.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartProfileView.ChartAreas[0].AxisY.IsLogarithmic = false;
            }
            if (chartVoltage.ChartAreas.Count > 0)
            {
                chartVoltage.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartVoltage.ChartAreas[0].AxisY.IsLogarithmic = false;
            }

            gridData.DataSource = null; // 清空发射源表格
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;
            Console.WriteLine("=== 所有显示清空完成 ===");
        }

        #endregion

        #region --- 辅助函数 ---

        /// <summary>
        /// 【复用】查找地图，逻辑参考 Form_MT1di
        /// 【优化】添加地图查找日志
        /// </summary>
        private Map FindMapByName(string mapName)
        {
            if (_hook == null || _hook.Document == null)
            {
                Console.WriteLine("FindMapByName: _hook 或 _hook.Document 为 null。");
                return null;
            }

            Maps maps = _hook.Document.GetMaps();
            for (int i = 0; i < maps.Count; i++)
            {
                Map currentMap = maps.GetMap(i);
                if (currentMap != null && currentMap.Name == mapName)
                {
                    Console.WriteLine($"找到地图: {mapName}");
                    return currentMap;
                }
            }

            Console.WriteLine($"FindMapByName: 未在项目中找到名为 '{mapName}' 的地图。");
            return null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process currentProcess = null;
            // 加锁获取当前进程引用，避免线程冲突
            lock (_processLock)
            {
                currentProcess = _aExeProcess;
            }

            // 检测进程是否正在运行
            if (currentProcess != null && !currentProcess.HasExited)
            {
                DialogResult result = MessageBox.Show(
                  "计算正在进行中，关闭窗口将终止计算。是否继续？",
                  "警告",
                  MessageBoxButtons.YesNo,
                  MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    return; // 不关闭窗口
                }

                // 强制终止进程
                try
                {
                    currentProcess.Kill();
                    // 等待进程终止（最多等2秒）
                    if (currentProcess.WaitForExit(2000))
                    {
                        MessageBox.Show("计算已终止。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("计算进程强制终止超时。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"终止计算失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 停止定时器并关闭窗口
            timerProgress.Stop();
            this.Close();
        }

        #endregion
        // 1. 初始化滑动条 (在 cmbLineName_SelectedIndexChanged 数据加载成功后调用)
        private void InitTimeSliderAndGrids()
        {
            if (m_CurrentLineStations.Count > 0 && m_CurrentLineData.Rows.Count > 0)
            {
                // 获取所有不重复的采样时间
                DataView view = new DataView(m_CurrentLineData);
                DataTable distinctTimes = view.ToTable(true, "采样时间_us");


                // 排序时间
                var times = distinctTimes.AsEnumerable()
                    .Select(r => r.Field<double>("采样时间_us"))
                    .OrderBy(t => t)
                    .ToList();

                if (times.Count > 0)
                {
                    trackBarTime.Minimum = 0;
                    trackBarTime.Maximum = times.Count - 1;
                    trackBarTime.Value = 0;
                    trackBarTime.Tag = times; // 将时间列表存入 Tag 方便调用

                    // 手动绑定事件（如果 Designer 没绑）
                    trackBarTime.Scroll -= TrackBarTime_Scroll;
                    trackBarTime.Scroll += TrackBarTime_Scroll;

                    // 触发一次初始绘制
                    TrackBarTime_Scroll(null, null);
                }
            }
        }

        // 2. 滑动条事件：更新 Z 剖面图
        private void TrackBarTime_Scroll(object sender, EventArgs e)
        {
            if (trackBarTime.Tag is List<double> times && times.Count > 0)
            {
                int index = trackBarTime.Value;
                double currentTime = times[index];
                lblTimeInfo.Text = $"采样延时时间: {currentTime} μs (第 {index + 1} 道)";

                UpdateZProfileChart(currentTime);
            }
        }

        // 3. 绘制剖面图核心逻辑 (多测道彩虹图 + 高亮当前道)
        // 辅助：存储原始颜色
        private Dictionary<string, System.Drawing.Color> _seriesOriginalColors = new Dictionary<string, System.Drawing.Color>();

        private void UpdateZProfileChart(double selectedTime)
        {
            // A. 初始化绘制所有线条
            if (chartZProfile.Series.Count == 0)
            {
                _seriesOriginalColors.Clear(); // 清空颜色缓存

                DataView view = new DataView(m_CurrentLineData);
                DataTable distinctTimes = view.ToTable(true, "采样时间_us");
                var timeList = distinctTimes.AsEnumerable()
                              .Select(r => r.Field<double>("采样时间_us"))
                              .OrderBy(t => t)
                              .ToList();

                // 准备计算 X 轴的范围，用于手动设置（可选）
                double minX = double.MaxValue;
                double maxX = double.MinValue;

                for (int i = 0; i < timeList.Count; i++)
                {
                    double t = timeList[i];
                    string seriesName = $"T_{t}";
                    Series series = chartZProfile.Series.Add(seriesName);
                    series.ChartType = SeriesChartType.Line;
                    series.Tag = t;

                    // 生成并保存颜色
                    System.Drawing.Color c = GetRainbowColor(i, timeList.Count);
                    series.Color = c;
                    _seriesOriginalColors[seriesName] = c; // 记住这个颜色

                    series.BorderWidth = 1;
                    series.MarkerStyle = MarkerStyle.None;

                    foreach (var station in m_CurrentLineStations)
                    {
                        var rows = m_CurrentLineData.Select($"测点号='{station.StationName}' AND 采样时间_us={t}");
                        if (rows.Length > 0)
                        {
                            double zVal = Convert.ToDouble(rows[0]["感应电压_mV"]);
                            // 这里添加的点：X 是测点坐标，Y 是感应电压
                            // 如果你想让横坐标变成“相对距离”，需要在这里转换 X

                            // === 【关键修改点 1：计算相对距离】 ===
                            // 假设第一个测点是起点 (0米)
                            double startX = m_CurrentLineStations[0].X;
                            double relativeDist = station.X - startX;

                            // 使用相对距离作为 X 轴
                            series.Points.AddXY(relativeDist, zVal);

                            // 更新最大最小值
                            if (relativeDist < minX) minX = relativeDist;
                            if (relativeDist > maxX) maxX = relativeDist;
                        }
                    }
                }

                // === 【关键修改点 2：设置 X 轴从 0 开始】 ===
                var area = chartZProfile.ChartAreas[0];
                area.AxisX.Title = "距离 (m)";
                area.AxisX.Minimum = 0; // 强制从 0 开始

                // 如果需要，可以设置最大值稍微大一点，留出边距
                if (maxX > minX)
                {
                    area.AxisX.Maximum = maxX * 1.05;
                }

                area.AxisY.Title = "感应电压 (mV)";
                area.RecalculateAxesScale();
            }

            // B. 高亮逻辑 (保持不变)
            double epsilon = 1e-9;
            foreach (Series s in chartZProfile.Series)
            {
                if (s.Tag != null && double.TryParse(s.Tag.ToString(), out double seriesTime))
                {
                    if (Math.Abs(seriesTime - selectedTime) < epsilon)
                    {
                        // 选中：变粗、变黑（或红）、带点
                        s.BorderWidth = 3;
                        s.Color = System.Drawing.Color.Black;
                        s.MarkerStyle = MarkerStyle.Circle;
                        s.MarkerSize = 7;
                    }
                    else
                    {
                        // 未选中：恢复原始颜色、变细
                        s.BorderWidth = 1;
                        if (_seriesOriginalColors.ContainsKey(s.Name))
                        {
                            s.Color = _seriesOriginalColors[s.Name];
                        }
                        s.MarkerStyle = MarkerStyle.None;
                    }
                }
            }
        }
        // 生成彩虹渐变色
        private System.Drawing.Color GetRainbowColor(int index, int total)
        {
            if (total <= 0) return System.Drawing.Color.Blue;

            // 简单的蓝->红渐变 (HSV模型更佳，但这里用RGB简单模拟)
            // 0 -> Blue, 0.5 -> Green, 1 -> Red
            double ratio = (double)index / (total - 1);
            int r = 0, g = 0, b = 0;

            if (ratio < 0.5)
            {
                // Blue to Green
                b = (int)(255 * (1 - 2 * ratio));
                g = (int)(255 * 2 * ratio);
            }
            else
            {
                // Green to Red
                g = (int)(255 * (2 - 2 * ratio));
                r = (int)(255 * (2 * ratio - 1));
            }

            // 限制范围 0-255
            r = Math.Min(255, Math.Max(0, r));
            g = Math.Min(255, Math.Max(0, g));
            b = Math.Min(255, Math.Max(0, b));

            return System.Drawing.Color.FromArgb(r, g, b);
        }
        private void UpdateZDataGrid(string calcFilePath = null)
        {
            // 基础清理
            gridZData.DataSource = null;
            gridZData.Rows.Clear();
            gridZData.Columns.Clear();

            // 自动选中逻辑
            if (string.IsNullOrEmpty(m_CurrentSelectedStationName))
            {
                if (m_CurrentLineStations != null && m_CurrentLineStations.Count > 0)
                    m_CurrentSelectedStationName = m_CurrentLineStations[0].StationName;
                else
                    return;
            }

            // 读取拟合结果 (如果路径存在)
            Dictionary<int, double> calcValues = new Dictionary<int, double>();
            if (!string.IsNullOrEmpty(calcFilePath) && File.Exists(calcFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(calcFilePath);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3 && parts[0].Trim() == m_CurrentSelectedStationName.Trim())
                        {
                            if (int.TryParse(parts[1], out int tIdx) && double.TryParse(parts[2], out double val))
                                calcValues[tIdx] = val;
                        }
                    }
                }
                catch { /* 忽略读取错误 */ }
            }

            // 准备实测数据
            DataRow[] rows = m_CurrentLineData.Select($"测点号='{m_CurrentSelectedStationName}'", "采样时间_us ASC");

            // 构建列
            gridZData.Columns.Add("colT", "采样延时(μs)");
            gridZData.Columns.Add("colZ", "实测感应电动势");
            gridZData.Columns.Add("colCalcZ", "拟合感应电动势");

            for (int i = 0; i < rows.Length; i++)
            {
                double t = Convert.ToDouble(rows[i]["采样时间_us"]);
                double z = Convert.ToDouble(rows[i]["感应电压_mV"]);

                string calcZStr = "-"; // 默认无数据
                if (calcValues.ContainsKey(i))
                    calcZStr = calcValues[i].ToString("G5");

                gridZData.Rows.Add(t, z, calcZStr);
            }
            gridZData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        /// <summary>
        /// 【最终完整版】绘制电阻率断面图（顶端强制 0 对齐）
        /// </summary>
        /// <param name="knowFilePath">KNOW 结果文件路径</param>
        private void UpdateResistivitySection(string knowFilePath)
        {
            // 1. 清理图表
            var chart = chartResistivity;
            chart.Series.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();
            chart.ChartAreas.Clear();

            // 添加标题
            Title title = new Title($"电阻率反演断面图");
            title.Font = new System.Drawing.Font("微软雅黑", 12, System.Drawing.FontStyle.Bold);
            title.ForeColor = System.Drawing.Color.Black;
            chart.Titles.Add(title);

            if (!File.Exists(knowFilePath)) return;

            // 2. 读取数据并计算相对距离
            List<ResultPoint> points = new List<ResultPoint>();
            try
            {
                string[] lines = File.ReadAllLines(knowFilePath);
                double startX = double.NaN;
                double startY = double.NaN;
                bool isFirstPoint = true;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 6)
                    {
                        double x = 0, y = 0, z = 0, res = 0;
                        // 尝试解析
                        bool parseSuccess = double.TryParse(parts[2], out x) &&
                                            double.TryParse(parts[3], out y) &&
                                            double.TryParse(parts[4], out z) &&
                                            double.TryParse(parts[5], out res);

                        if (parseSuccess)
                        {
                            if (res <= 0) continue; // 过滤无效电阻率

                            if (isFirstPoint)
                            {
                                startX = x;
                                startY = y;
                                isFirstPoint = false;
                            }

                            // 计算相对距离
                            double dist = Math.Sqrt(Math.Pow(x - startX, 2) + Math.Pow(y - startY, 2));

                            points.Add(new ResultPoint
                            {
                                Distance = dist,
                                Depth = z,
                                Value = res
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取结果文件失败: {ex.Message}");
                return;
            }

            if (points.Count == 0) return;

            // 3. 计算数据范围
            var validResPoints = points.Where(p => p.Value > 0).ToList();
            if (validResPoints.Count == 0) return;

            double minLog = Math.Log10(validResPoints.Min(p => p.Value));
            double maxLog = Math.Log10(validResPoints.Max(p => p.Value));

            // 4. 计算坐标轴范围 (NiceScale)
            double rawMinX = points.Min(p => p.Distance);
            double rawMaxX = points.Max(p => p.Distance);
            double rawMinY = points.Min(p => p.Depth);
            double rawMaxY = points.Max(p => p.Depth);

            var niceX = CalculateStrictNiceScale(rawMinX, rawMaxX);
            var niceY = CalculateStrictNiceScale(rawMinY, rawMaxY);

            // ---------------------------------------------------------
            // 5. 配置 ChartArea
            // ---------------------------------------------------------
            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "距离 (m)";
            ca.AxisY.Title = "深度/高程 (m)";

            // 字体设置
            ca.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            ca.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            // 【关键】强制显示最后一个标签，确保 "0" 一定会显示出来
            ca.AxisY.LabelStyle.IsEndLabelVisible = true;

            // 变量定义
            double padX = niceX.Interval * 0.2;
            // 底部留白依然需要，但顶部不需要了
            double padY_Bottom = niceY.Interval * 0.5;

            // =========================================================
            // 【X 轴逻辑】
            // =========================================================
            if (rawMinX >= 0 && niceX.Min < 0) ca.AxisX.Minimum = 0;
            else ca.AxisX.Minimum = niceX.Min - padX;

            ca.AxisX.Maximum = niceX.Max + padX;
            ca.AxisX.Interval = niceX.Interval;

            // =========================================================
            // 【Y 轴逻辑】(顶端对齐 0)
            // =========================================================
            bool isElevationMode = (rawMaxY <= 0);

            if (isElevationMode)
            {
                // === 高程模式 (负数) ===
                ca.AxisY.IsReversed = false;

                // 【核心修改 1】强制天花板为 0
                ca.AxisY.Maximum = 0;

                // 【核心修改 2】关闭自动边距，让数据紧贴边框
                ca.AxisY.IsMarginVisible = false;

                // 锁定步长
                double step = niceY.Interval;
                ca.AxisY.Interval = step;

                // 向下推算最小值 (为了底部好看，还是留一点空隙)
                double bottomLimit = Math.Floor(rawMinY / step) * step;
                if (bottomLimit > rawMinY) bottomLimit -= step;

                ca.AxisY.Minimum = bottomLimit - step; // 底部留一格

                // 让 X 轴沉底
                ca.AxisX.Crossing = ca.AxisY.Minimum;
            }
            else
            {
                // === 深度模式 (正数) ===
                ca.AxisY.IsReversed = true;
                ca.AxisY.IsMarginVisible = true; // 深度模式通常保留边距比较好看
                ca.AxisY.Minimum = niceX.Min;
                ca.AxisY.Maximum = niceY.Max + padY_Bottom;
                ca.AxisY.Interval = niceY.Interval;
                ca.AxisX.Crossing = niceY.Min;
            }

            // 应用通用美化
            BeautifyChartAxes(ca);

            // 6. 绘制数据点
            Series s = chart.Series.Add("Section");
            s.ChartType = SeriesChartType.Point; // 散点图
            s.MarkerStyle = MarkerStyle.Square;
            s.BorderWidth = 0;

            s.XValueType = ChartValueType.Double;
            s.YValueType = ChartValueType.Double;

            // 动态调整点的大小 (数据量大时点小一点)
            int dynamicSize = (points.Count > 5000) ? 4 : 8; // 稍微调小一点，方便看清
            s.MarkerSize = dynamicSize;
            double stepY = niceY.Interval;
            foreach (var p in validResPoints)
            {
                if (p.Distance < ca.AxisX.Minimum || p.Distance > ca.AxisX.Maximum) continue;

                // 【黑科技1】防止索引模式
                double fixX = p.Distance + (0.000001 * new Random().NextDouble());

                // 【黑科技2：视觉微调】
                // 如果点非常接近地表 (比如 -20m 以内)，强制将绘制坐标设为 0。
                // 这样方块的一半会被切掉，另一半紧贴顶端，消除缝隙。
                double drawDepth = p.Depth;
                // ====================================================================
                // 【修改开始】优化黑科技2：视觉微调
                // 原理：不要画在 0，而是画在 0 下方半个步长处，让方块的顶边刚好挨着 0
                // ====================================================================
                if (isElevationMode)
                {
                    // 判断条件：如果点非常接近地表（比如在一个步长范围内）
                    // 注意：这里用 niceY.Interval 更通用，代替硬编码的 -20
                    if (p.Depth > -stepY && p.Depth <= 0.1)
                    {
                        // 核心修改：向下移动半个步长
                        // 这样方块的中心在 -5m，如果方块够大，它的顶端刚好在 0m
                        drawDepth = -(stepY / 2.0);

                        // 如果你觉得移太多了，也可以只移一点点，比如：
                        // drawDepth = - (stepY * 0.2); 
                    }
                }
                // ====================================================================
                // 【修改结束】
                // ====================================================================

                int idx = s.Points.AddXY(fixX, drawDepth);

                double logVal = Math.Log10(p.Value);
                s.Points[idx].Color = GetColorForValue(logVal, minLog, maxLog);
                s.Points[idx].ToolTip = $"距离: {p.Distance:F1} m\n深度: {p.Depth:F1} m\n电阻率: {p.Value:F1}";
            }
            s.IsXValueIndexed = false;

            // 7. 添加颜色图例 (Legend)
            Legend legend = chart.Legends.Add("ColorScale");
            legend.Docking = Docking.Right;
            legend.Title = "lg(ρ)";
            legend.IsTextAutoFit = false;

            // 倒序生成图例，让红色(高阻)在上面，蓝色(低阻)在下面
            int levels = 10;
            for (int i = 0; i <= levels; i++)
            {
                double val = minLog + (maxLog - minLog) * i / levels;
                var item = new System.Windows.Forms.DataVisualization.Charting.LegendItem();
                item.Name = val.ToString("F1");
                item.Color = GetColorForValue(val, minLog, maxLog);
                item.MarkerStyle = MarkerStyle.Square;
                item.MarkerSize = 14;
                legend.CustomItems.Add(item);
            }
            // 反转列表使高值在上方
            legend.CustomItems.Reverse();
        }

        // 确保你有这个辅助类


        // 确保你有添加图例的方法


        // 辅助方法：添加图例 (保持代码整洁)
        private void AddColorLegend(double minLogRes, double maxLogRes)
        {
            Legend legend = new Legend("ColorBar");
            legend.Docking = Docking.Right;
            legend.Title = "lg(Res)";
            legend.IsTextAutoFit = false;

            int steps = 5;
            for (int i = 0; i <= steps; i++)
            {
                double val = maxLogRes - (maxLogRes - minLogRes) * ((double)i / steps);
                System.Drawing.Color c = GetColorForValue(val, minLogRes, maxLogRes);

                var item = new System.Windows.Forms.DataVisualization.Charting.LegendItem();
                item.Name = $"{val:F1}";
                item.Color = c;
                item.MarkerStyle = MarkerStyle.Square;
                legend.CustomItems.Add(item);
            }
            chartResistivity.Legends.Add(legend);
        }


        // 辅助类需要补充 Y 字段（如果你之前没有加的话）
        public class InversionPoint
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; } // 新增 Y
            public double Depth { get; set; }
            public double Res { get; set; }
            public double LogRes { get; set; }
        }


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // === 1. 定义基础数据 (S280 的原始深度和电阻率) ===
                double[] baseDepths = new double[] {
  -10, -22.6798, -28.3072, -34.4975, -41.3067, -48.7969, -57.0361, -66.0992,
  -76.0687, -87.0351, -99.0981, -112.367, -126.964, -143.02, -160.681,
  -180.109, -201.479, -224.986, -250.845, -279.288, -310.577, -344.994,
  -382.853, -424.498, -470.307, -520.697, -576.126, -637.099, -704.168,
  -777.944, -859.098, -948.368, -1495.13
};

                double[] baseResValues = new double[] {
  1.76905e+06, 1.75329e+06, 1.72141e+06, 1.6744e+06, 1.61339e+06, 1.53964e+06, 1.45454e+06,
  1.3597e+06, 1.25698e+06, 1.14853e+06, 1.03674e+06, 924136, 813270, 706512, 605885,
  512928, 428614, 353356, 287093, 229422, 179755, 137462, 101957, 72726.6,
  49285.2, 31117.8, 17649.4, 8305.2, 2645.2, 302.899, 49.8578, 438.492, 55.3218
};

                Random rnd = new Random();

                // 用于存储模拟数据的新列表
                List<InversionResultPoint> mockResults = new List<InversionResultPoint>();

                // === 2. 生成 10 个测点的数据 ===
                double startX = 461490;
                double startY = 4567050;
                int stationCount = 10;
                double spacing = 50.0; // 测点间距 50米

                // 为了计算 Distance，这里我们简单假设沿直线的 Distance 就是 i * spacing
                // 在真实数据中，Distance 是根据 X/Y 计算的

                for (int i = 0; i < stationCount; i++)
                {
                    double currentX = startX + (i * spacing);
                    double currentY = startY;
                    string stationName = $"S{280 + i * 10}";

                    // 当前测点沿测线的相对距离 (假设从 0 开始)
                    double relativeDist = i * spacing;

                    for (int j = 0; j < baseDepths.Length; j++)
                    {
                        double z = baseDepths[j]; // 负值
                        double originalRes = baseResValues[j];
                        double finalRes = originalRes;

                        // --- 模拟地质异常 (中间测点深部低阻) ---
                        bool isMiddleStation = (i >= 3 && i <= 6);
                        bool isDeep = (z < -200 && z > -800);

                        if (isMiddleStation && isDeep)
                        {
                            finalRes = originalRes * (0.01 + rnd.NextDouble() * 0.04); // 低阻
                        }
                        else
                        {
                            double noise = 0.9 + (rnd.NextDouble() * 0.2);
                            finalRes = originalRes * noise;
                        }

                        // 添加到列表
                        mockResults.Add(new InversionResultPoint
                        {
                            StationName = stationName,
                            X = currentX,
                            Y = currentY,
                            Distance = relativeDist, // 这一点很重要，绘图依赖这个
                            Depth = z,               // 保持负值
                            Resistivity = finalRes
                        });
                    }
                }

                // === 3. 更新缓存并绘图 ===
                // 将模拟数据存入缓存，这样调整深度旋钮时也能重绘模拟数据
                m_LastInversionResults = mockResults;

                // 设置一个合适的显示深度 (例如 1500m)，确保能看到全部模拟数据
                nudMaxDepth1.Value = 1500;

                // 调用新的绘图逻辑
                DisplayInversionResults(m_LastInversionResults);

                MessageBox.Show($"生成了 {stationCount} 个测点的数据并已绘制。\n\n" +
                  $"预期效果：\n" +
                  $"1. 使用新版“砌砖法”绘图。\n" +
                  $"2. 图表中间底部 (-200m ~ -800m) 应该会出现明显的【蓝色/冷色区域】。\n" +
                  $"3. 尝试调整右上角的深度旋钮，图表应能实时响应。", "测试成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"测试失败: {ex.Message}", "错误");
            }
        }
        
        // --- 【新增】复用 MT1di 的数据结构 ---
        private struct ResultPoint
        {
            public double Distance; // 沿测线相对距离
            public double Depth;    // 深度/高程
            public double Value;    // 电阻率
        }

        private struct NiceScale
        {
            public double Min;
            public double Max;
            public double Interval;
        }
        // --- 【新增】复用 MT1di 的优雅刻度计算算法 ---
        private NiceScale CalculateStrictNiceScale(double min, double max)
        {
            if (min == max) return new NiceScale { Min = min - 10, Max = max + 10, Interval = 10 };

            // 确保 min < max
            if (min > max) { double t = min; min = max; max = t; }

            double range = max - min;
            double roughInterval = range / 5.0; // 期望大概分5个刻度

            // 计算数量级 (例如 0.01, 1, 10, 100)
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
            double normalizedInterval = roughInterval / magnitude;

            // 归一化到 [1, 10) 区间，并规整为 1, 2, 5, 10
            double niceInterval;
            if (normalizedInterval < 1.5) niceInterval = 1.0;
            else if (normalizedInterval < 3.0) niceInterval = 2.0;
            else if (normalizedInterval < 7.0) niceInterval = 5.0;
            else niceInterval = 10.0;

            niceInterval *= magnitude;

            // 计算对齐后的最大最小值
            double niceMin = Math.Floor(min / niceInterval) * niceInterval;
            double niceMax = Math.Ceiling(max / niceInterval) * niceInterval;

            return new NiceScale { Min = niceMin, Max = niceMax, Interval = niceInterval };
        }

        // --- 【修改】颜色映射算法 (Jet色标，蓝-青-绿-黄-红) ---
        private System.Drawing.Color GetColorForValue(double value, double min, double max)
        {
            if (min >= max) return System.Drawing.Color.Black;

            // 归一化
            double normalized = (value - min) / (max - min);

            // Jet Colormap 逻辑
            if (normalized < 0.25)
                return System.Drawing.Color.FromArgb(0, (int)(255 * (normalized * 4)), 255); // 蓝 -> 青
            else if (normalized < 0.5)
                return System.Drawing.Color.FromArgb(0, 255, (int)(255 * (1 - (normalized - 0.25) * 4))); // 青 -> 绿
            else if (normalized < 0.75)
                return System.Drawing.Color.FromArgb((int)(255 * ((normalized - 0.5) * 4)), 255, 0); // 绿 -> 黄
            else
                return System.Drawing.Color.FromArgb(255, (int)(255 * (1 - (normalized - 0.75) * 4)), 0); // 黄 -> 红
        }

        private void nudMaxDepth1_ValueChanged(object sender, EventArgs e)
        {
            // 当用户调整深度时，如果有缓存数据，立即重绘
            if (m_LastInversionResults != null && m_LastInversionResults.Count > 0)
            {
                DisplayInversionResults(m_LastInversionResults);
            }

        }
        /// <summary>
        /// 1. 解析 KNOW 文件
        /// </summary>
        private List<InversionResultPoint> ParseKnowFile(string filePath)
        {
            var results = new List<InversionResultPoint>();
            if (!File.Exists(filePath)) return results;

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                double startX = double.NaN, startY = double.NaN;
                bool isFirst = true;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    // 格式: Line Station X Y Depth Res
                    if (parts.Length >= 6)
                    {
                        if (double.TryParse(parts[2], out double x) &&
                            double.TryParse(parts[3], out double y) &&
                            double.TryParse(parts[4], out double z) &&
                            double.TryParse(parts[5], out double res))
                        {
                            if (isFirst) { startX = x; startY = y; isFirst = false; }

                            // 计算沿测线距离
                            double dist = Math.Sqrt(Math.Pow(x - startX, 2) + Math.Pow(y - startY, 2));

                            results.Add(new InversionResultPoint
                            {
                                StationName = parts[1],
                                X = x,
                                Y = y,
                                Distance = dist,
                                Depth = z, // 通常 TEM 反演输出的深度为负值
                                Resistivity = res
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析 KNOW 文件出错: {ex.Message}");
            }
            return results;
        }

        /// <summary>
        /// 2. 绘制拼接拟断面图 (Stitched 1D Model)
        /// </summary>
        private void DisplayInversionResults(List<InversionResultPoint> results)
        {
            var chart = chartResistivity;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();
            chart.Images.Clear(); // 清除旧的背景图

            if (results == null || results.Count == 0) return;

            // 获取设计器上的控件值
            double userMaxDepth = (double)nudMaxDepth1.Value;

            chart.Titles.Add($"电阻率反演拟断面图 (0 ~ -{userMaxDepth:F0}m)");
            chart.Titles[0].Font = new System.Drawing.Font("微软雅黑", 12f, FontStyle.Bold);

            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "相对距离 (m)";
            ca.AxisY.Title = "深度 (m)";

            // 数据筛选 (假设深度是负数，我们要画 0 到 -MaxDepth)
            var validResults = results
                .Where(r => r.Resistivity > 0 && r.Depth >= -userMaxDepth && r.Depth <= 10)
                .ToList();

            if (validResults.Count == 0) return;

            // 颜色映射范围
            double minLogRes = Math.Log10(validResults.Min(r => r.Resistivity));
            double maxLogRes = Math.Log10(validResults.Max(r => r.Resistivity));

            // 按测点分组
            var stationGroups = validResults
                .GroupBy(r => r.StationName)
                .Select(g => new
                {
                    Name = g.Key,
                    Dist = g.First().Distance,
                    Layers = g.OrderByDescending(p => p.Depth).ToList() // 从浅(0)到深(-100)排序
                })
                .OrderBy(s => s.Dist)
                .ToList();

            if (stationGroups.Count == 0) return;

            // 计算绘图 X 范围
            double minX = stationGroups.Min(s => s.Dist);
            double maxX = stationGroups.Max(s => s.Dist);
            double dataRangeX = maxX - minX;
            if (dataRangeX <= 0) dataRangeX = 1;

            // 计算每个测点的柱状宽度
            double[] stationLeft = new double[stationGroups.Count];
            double[] stationWidth = new double[stationGroups.Count];

            for (int i = 0; i < stationGroups.Count; i++)
            {
                double current = stationGroups[i].Dist;
                double leftBound, rightBound;

                if (i == 0) leftBound = current;
                else leftBound = (stationGroups[i - 1].Dist + current) / 2.0;

                if (i == stationGroups.Count - 1) rightBound = current + (current - leftBound);
                else rightBound = (current + stationGroups[i + 1].Dist) / 2.0;

                stationLeft[i] = leftBound;
                stationWidth[i] = rightBound - leftBound;
            }

            // 单点修正
            if (stationGroups.Count == 1)
            {
                stationLeft[0] = minX - 10;
                stationWidth[0] = 20;
                minX = stationLeft[0];
                maxX = stationLeft[0] + 20;
            }

            // --- GDI+ 绘图 ---
            int bmpW = Math.Max(chart.Width, 100);
            int bmpH = Math.Max(chart.Height, 100);

            using (Bitmap bmp = new Bitmap(bmpW, bmpH))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.White);

                // 坐标映射参数
                // 世界坐标 X: minX ~ maxX (对应 stationLeft[0] ~ stationRight[last])
                double totalWorldWidth = (stationLeft[stationGroups.Count - 1] + stationWidth[stationGroups.Count - 1]) - stationLeft[0];
                double scaleX = bmpW / totalWorldWidth;
                double worldOffsetX = stationLeft[0];

                // 世界坐标 Y: 0 ~ -userMaxDepth (映射到屏幕 0 ~ bmpH)
                double scaleY = bmpH / userMaxDepth;

                for (int i = 0; i < stationGroups.Count; i++)
                {
                    var st = stationGroups[i];
                    var layers = st.Layers;

                    // 屏幕 X 坐标
                    float rectX = (float)((stationLeft[i] - worldOffsetX) * scaleX);
                    float rectW = (float)(stationWidth[i] * scaleX) + 1.0f; // +1 防止缝隙

                    for (int j = 0; j < layers.Count; j++)
                    {
                        var layer = layers[j];
                        double zCenter = Math.Abs(layer.Depth); // 转正值
                        double zTop, zBottom;

                        // 推算层顶底
                        if (j == 0)
                        {
                            zTop = 0;
                            if (layers.Count > 1) zBottom = (zCenter + Math.Abs(layers[j + 1].Depth)) / 2.0;
                            else zBottom = zCenter + 10;
                        }
                        else if (j == layers.Count - 1)
                        {
                            zTop = (Math.Abs(layers[j - 1].Depth) + zCenter) / 2.0;
                            zBottom = userMaxDepth;
                        }
                        else
                        {
                            zTop = (Math.Abs(layers[j - 1].Depth) + zCenter) / 2.0;
                            zBottom = (zCenter + Math.Abs(layers[j + 1].Depth)) / 2.0;
                        }

                        if (zTop > userMaxDepth) continue;
                        if (zBottom > userMaxDepth) zBottom = userMaxDepth;

                        // 屏幕 Y 坐标 (深度越大，Y越大)
                        float rectY = (float)(zTop * scaleY);
                        float rectH = (float)((zBottom - zTop) * scaleY);
                        if (rectH < 1) rectH = 1;

                        System.Drawing.Color c = GetColorForValueSmooth(Math.Log10(layer.Resistivity), minLogRes, maxLogRes);
                        using (SolidBrush brush = new SolidBrush(c))
                        {
                            g.FillRectangle(brush, rectX, rectY, rectW, rectH);
                        }
                    }
                }

                // 将图片设为背景
                string imgName = "SectionBkg_" + Guid.NewGuid().ToString();
                chart.Images.Add(new NamedImage(imgName, (Bitmap)bmp.Clone()));
                ca.BackImage = imgName;
                ca.BackImageWrapMode = ChartImageWrapMode.Scaled;
            }

            // 设置坐标轴以匹配图片
            ca.AxisX.Minimum = stationLeft[0];
            ca.AxisX.Maximum = stationLeft[stationGroups.Count - 1] + stationWidth[stationGroups.Count - 1];
            ca.AxisY.Minimum = -userMaxDepth;
            ca.AxisY.Maximum = 0;
            // 1. 清除原来的自动标签和自动间隔
            ca.AxisX.CustomLabels.Clear();
            ca.AxisX.Interval = 0;                  // 禁用自动间隔
            ca.AxisX.MajorGrid.Enabled = false;     // 不显示竖向网格线（因为不对齐）
            ca.AxisX.MajorTickMark.Enabled = true;  // 保留刻度线
            //2.循环添加真实测点位置的标签
            for (int i = 0; i < stationGroups.Count; i++)
            {
                var st = stationGroups[i];
                double pos = st.Dist; // 真实坐标，例如 150

                // 创建标签
                var label = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();

                // =========================================================
                // 【核心修复点】
                // 之前 range 只有 0.2，太窄导致文字竖排。
                // 现在我们将标签范围设置为整个柱子的宽度，空间非常充裕！
                // =========================================================
                label.FromPosition = stationLeft[i];
                label.ToPosition = stationLeft[i] + stationWidth[i];

                // 显示文本
                label.Text = pos.ToString("F0");

                // 强制显示在第一行
                label.RowIndex = 0;

                ca.AxisX.CustomLabels.Add(label);
            }

            // 美化

            ca.AxisY.LabelStyle.Format = "F0";
            
            ca.AxisY.Interval = CalculateNiceInterval(userMaxDepth, 10);
            BeautifyChartAxes(ca);
            // 强制关闭自动拟合，严格按照我们设置的 Interval 显示
            ca.AxisX.IsLabelAutoFit = false;
            ca.AxisY.IsLabelAutoFit = false;
            ca.AxisX.LabelStyle.Angle = 0;
            ca.AxisX.LabelStyle.IsStaggered = false; // 禁止标签上下错开


            // 确保从最小值开始计算间隔
            ca.AxisX.IntervalOffset = 0;
            ca.AxisY.IntervalOffset = 0;

            // 撑开坐标轴的透明点
            Series sDummy = chart.Series.Add("Dummy");
            sDummy.ChartType = SeriesChartType.Point;
            sDummy.Points.AddXY(ca.AxisX.Minimum, 0);
            sDummy.Points.AddXY(ca.AxisX.Maximum, -userMaxDepth);
            sDummy.Color = System.Drawing.Color.Transparent;
            sDummy.IsVisibleInLegend = false;

            // 添加色标
            AddColorBar(minLogRes, maxLogRes);
        }
        // --- 辅助函数 ---

        private void AddColorBar(double minLog, double maxLog)
        {
            Legend legend = chartResistivity.Legends.Add("ColorBar");
            legend.Docking = Docking.Right;
            legend.Title = "lg(ρ)";
            int steps = 10;
            for (int k = steps; k >= 0; k--)
            {
                double val = minLog + (maxLog - minLog) * k / steps;
                var item = new System.Windows.Forms.DataVisualization.Charting.LegendItem();
                item.Name = val.ToString("F1");
                item.Color = GetColorForValueSmooth(val, minLog, maxLog);
                item.MarkerStyle = MarkerStyle.Square;
                item.MarkerSize = 12;
                legend.CustomItems.Add(item);
            }
        }

        private System.Drawing.Color GetColorForValueSmooth(double val, double min, double max)
        {
            if (val < min) val = min;
            if (val > max) val = max;
            double ratio = (val - min) / (max - min);
            return GetJetColor(ratio);
        }

        // Jet 色标 (蓝 -> 绿 -> 红)
        private System.Drawing.Color GetJetColor(double v)
        {
            double r = 0, g = 0, b = 0;
            if (v < 0.125) { b = 0.5 + 4 * v; }
            else if (v < 0.375) { b = 1; g = 4 * (v - 0.125); }
            else if (v < 0.625) { b = 4 * (0.625 - v); g = 1; r = 4 * (v - 0.375); }
            else if (v < 0.875) { g = 4 * (0.875 - v); r = 1; }
            else { r = 1 - 4 * (v - 0.875); }

            if (v >= 0.875) { r = 1; g = 4 * (0.875 - v); if (g < 0) g = 0; b = 0; }

            // 简单转 RGB
            return System.Drawing.Color.FromArgb(
                (int)(Math.Min(1, Math.Max(0, r)) * 255),
                (int)(Math.Min(1, Math.Max(0, g)) * 255),
                (int)(Math.Min(1, Math.Max(0, b)) * 255)
            );
        }

        private double CalculateNiceInterval(double range, int targetSteps)
        {
            double rough = range / targetSteps;
            double mag = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            double norm = rough / mag;
            double nice;
            if (norm < 1.5) nice = 1;
            else if (norm < 3) nice = 2;
            else if (norm < 7) nice = 5;
            else nice = 10;
            return nice * mag;
        }

        private void BeautifyChartAxes(ChartArea area)
        {
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8f);
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(64, System.Drawing.Color.Gray);
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(64, System.Drawing.Color.Gray);
            area.BorderColor = System.Drawing.Color.Black;
            area.BorderDashStyle = ChartDashStyle.Solid;
        }

        // 数据结构
        public class InversionResultPoint
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Distance { get; set; }
            public double Depth { get; set; }
            public double Resistivity { get; set; }
        }
        /// <summary>
        /// 创建和“电法数据”同结构的Map（独立地图），仅用于展示图片（无需坐标处理）
        /// </summary>
        /// <param name="chart">要展示的电阻率断面图控件</param>
        private void CreateImageDisplayMap(Chart chart)
        {
            Map newMap = null;
            RasterLayer displayLayer = null;
            Document projectDoc = null; // 保留你的原始接口
            Maps projectMaps = null;    // 保留你的原始接口

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

            // 2. 校验图表是否已生成
            bool isChartValid = chart.Series.Count > 0 || !string.IsNullOrEmpty(chart.ChartAreas[0]?.BackImage);
            if (!isChartValid)
            {
                MessageBox.Show("请先生成电阻率断面图，再创建展示地图！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3. 生成临时图片（核心：添加白色背景+边框样式，不改动你的接口）
            string tempImagePath = Path.ChangeExtension(Path.GetTempFileName(), ".png");

            // 记录Chart原始样式（生成后恢复，不影响原控件显示）
            System.Drawing.Color originalChartBackColor = chart.BackColor;
            int originalChartBorderWidth = chart.BorderWidth;
            Dictionary<ChartArea, (System.Drawing.Color BackColor, int BorderWidth, System.Drawing.Color BorderColor)> originalChartAreaProps = new Dictionary<ChartArea, (System.Drawing.Color, int, System.Drawing.Color)>();
            Dictionary<Legend, (System.Drawing.Color BackColor, int BorderWidth, System.Drawing.Color BorderColor)> originalLegendProps = new Dictionary<Legend, (System.Drawing.Color, int, System.Drawing.Color)>();

            try
            {
                // 👉 步骤1：记录原始样式（避免影响原Chart显示）
                foreach (ChartArea ca in chart.ChartAreas)
                {
                    originalChartAreaProps[ca] = (ca.BackColor, ca.BorderWidth, ca.BorderColor);
                }
                foreach (Legend lg in chart.Legends)
                {
                    originalLegendProps[lg] = (lg.BackColor, lg.BorderWidth, lg.BorderColor);
                }

                // 👉 步骤2：设置Chart样式（白色背景+黑色边框，核心需求）
                chart.BackColor = System.Drawing.Color.White;          // 整体背景白色
                chart.BorderWidth = 0;                  // 取消整体默认边框（避免重复）

                // 断面图区域（ChartArea）：白色背景+黑色边框
                foreach (ChartArea chartArea in chart.ChartAreas)
                {
                    chartArea.BackColor = System.Drawing.Color.White;  // 图背景白色
                    chartArea.BorderWidth = 1;          // 边框宽度1px
                    chartArea.BorderColor = System.Drawing.Color.Black;// 边框黑色（醒目）
                                                        // 内边距：避免内容贴边框
                    chartArea.InnerPlotPosition.Auto = false;
                    chartArea.InnerPlotPosition.X = 5;
                    chartArea.InnerPlotPosition.Y = 5;
                    chartArea.InnerPlotPosition.Width = 90;
                    chartArea.InnerPlotPosition.Height = 90;
                }

                // 图例（Legend）：白色背景+黑色边框（和图统一风格）
                foreach (Legend legend in chart.Legends)
                {
                    legend.BackColor = System.Drawing.Color.White;     // 图例背景白色
                    legend.BorderWidth = 1;             // 边框宽度1px
                    System.Drawing.Color black = System.Drawing.Color.Black;
                    legend.BorderColor = black;   // 边框黑色
                    
                }

                // 👉 步骤3：生成带样式的图片（用你的原始逻辑，仅捕获样式后的内容）
                using (Bitmap chartBmp = new Bitmap(chartResistivity.ClientSize.Width, chartResistivity.ClientSize.Height))
                {
                    chartResistivity.Refresh(); // 刷新确保样式生效
                    chartResistivity.DrawToBitmap(chartBmp, chartResistivity.ClientRectangle);
                    chartBmp.Save(tempImagePath, System.Drawing.Imaging.ImageFormat.Png); // 无损保存
                }

                // 👉 步骤4：恢复Chart原始样式（不影响原控件显示）
                chart.BackColor = originalChartBackColor;
                chart.BorderWidth = originalChartBorderWidth;
                foreach (var kvp in originalChartAreaProps)
                {
                    kvp.Key.BackColor = kvp.Value.BackColor;
                    kvp.Key.BorderWidth = kvp.Value.BorderWidth;
                    kvp.Key.BorderColor = kvp.Value.BorderColor;
                }
                foreach (var kvp in originalLegendProps)
                {
                    kvp.Key.BackColor = kvp.Value.BackColor;
                    kvp.Key.BorderWidth = kvp.Value.BorderWidth;
                    kvp.Key.BorderColor = kvp.Value.BorderColor;
                    
                }

                // 4. 获取Maps集合（保留你的原始接口调用）
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

                // 5. 检查同名地图（保留你的GetMap(i)调用）
                for (int i = 0; i < projectMaps.Count; i++)
                {
                    Map existingMap = projectMaps.GetMap(i); // 你的原始接口，不改动
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

                // 6. 创建新Map（保留你的逻辑）
                newMap = new Map();
                newMap.Name = newMapName;

                // 7. 加载图片到RasterLayer（保留你的接口调用）
                displayLayer = new RasterLayer();
                displayLayer.URL = tempImagePath;
                displayLayer.Name = $"{newMapName}_展示图层";

                // 连接数据+验证有效性
                if (!displayLayer.ConnectData() || !displayLayer.IsValid)
                {
                    throw new Exception("图片图层加载失败！");
                }

                // 双重保障：无数据颜色设为白色（避免黑色残留）
                displayLayer.NoDataColor = System.Drawing.Color.White;

                // 设置图层范围（保留你的逻辑）
                using (Image img = Image.FromFile(tempImagePath))
                {
                    Rect imgRange = new Rect(0, 0, img.Width, img.Height);
                    displayLayer.set_Range(imgRange);
                }

                // 8. 图层添加到Map（保留你的Append方法）
                newMap.Append(displayLayer); // 你的原始接口，不改动

                // 9. 新Map添加到集合（保留你的Append方法）
                projectMaps.Append(newMap); // 你的原始接口，不改动

                // 10. 刷新视图+提示（保留你的逻辑）
                //_hook.ActiveView?.Refresh(); // 若你不需要自动刷新，可保持注释
                MessageBox.Show(
                    $"成功创建展示地图【{newMapName}】！\n" +
                    $"地图结构与'电法数据'一致，已设置白色背景+黑色边框（图+图例均带框）。",
                    "创建成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
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
                // 释放资源（保留你的逻辑，若你之前注释了Close/Release，可保持）
                if (displayLayer != null)
                {
                    try
                    {
                        //displayLayer.Close(); // 若你不需要关闭，可保持注释
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

        /// <summary>
        /// 按钮3点击事件：将电阻率断面图生成到新的MapGIS地图（与电法数据同级）
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            CreateImageDisplayMap(chartResistivity);
        }
    }
}
