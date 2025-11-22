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
                        if (att["X"] != null && att["X"] != DBNull.Value)
                            double.TryParse(att["X"].ToString(), out x);
                        if (att["Y"] != null && att["Y"] != DBNull.Value)
                            double.TryParse(att["Y"].ToString(), out y);
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
        /// 【新增】开始计算按钮事件（TEM 特有）
        /// 与 MT 的区别：
        /// 1. 导出两个文件（观测数据和发射源）
        /// 2. 调用 a.exe 时参数为：a.exe knowed.dat tran.dat workspace
        /// 3. 工作空间必须为 6 字符
        /// 【优化】添加计算流程日志
        /// </summary>
        /// 
        // 方法声明添加 async 关键字
        private async void btnCalculate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(m_CurrentSelectedStationName))
            {
                MessageBox.Show("请先在小地图上选择一个测点再计算。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 1. 基础数据校验
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

            // 初始化进度条
            progressBarCalculate.Visible = true;
            progressBarCalculate.Value = 0;
            _isCalculationCompleted = false;

            string tempKnowedFile = null;
            string tempTranFile = null;
            string exePath = null;
            string pluginDir = null;
            string workspaceName = null;
            string fullWorkspacePath = null;

            try
            {
                //MessageBox.Show("=== 开始 TEM 计算流程 ===", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 2. 路径初始化与校验
                pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(pluginDir))
                    throw new Exception("无法获取插件目录，请检查程序部署。");

                string algorithmDir = Path.Combine(pluginDir, "Algorithm", "TEM1di");
                exePath = Path.Combine(algorithmDir, "a.exe");
                if (!File.Exists(exePath))
                    throw new FileNotFoundException($"计算程序 'a.exe' 未找到！\n预期路径: {exePath}");

                // 3. 生成工作空间
                string selectedLine = cmbLineName.SelectedItem?.ToString() ?? "TEM";
                string linePrefix = selectedLine.Length >= 3 ? selectedLine.Substring(0, 3) : selectedLine.PadRight(3, '0');
                string randomPart = Path.GetRandomFileName().Replace(".", "").Substring(0, 3);
                workspaceName = $"{linePrefix}{randomPart}".ToUpper();
                fullWorkspacePath = Path.Combine(pluginDir, workspaceName);

                // 4. 生成临时文件并导出数据
                tempKnowedFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_knowed.dat");
                tempTranFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_tran.dat");
                ExportObservationDataToFile(tempKnowedFile);
                ExportTransmitterInfoToFile(tempTranFile);

                if (new FileInfo(tempKnowedFile).Length == 0)
                    throw new Exception($"观测数据文件导出为空：{tempKnowedFile}");
                if (new FileInfo(tempTranFile).Length == 0)
                    throw new Exception($"发射源文件导出为空：{tempTranFile}");

                // 5. 异步执行 a.exe（线程安全的进程追踪）
                string commandLine = $".\\a.exe \"{tempKnowedFile}\" \"{tempTranFile}\" {workspaceName}";
                //MessageBox.Show($"即将执行命令：\n{commandLine}", "执行命令", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 启动进度条定时器
                timerProgress.Start();

                // 异步等待 a.exe 完成（带线程安全的进程引用）
                bool isSuccess = await Task.Run(() =>
                {
                    using (Process process = new Process())
                    {
                        // 加锁赋值，确保主线程能获取到进程引用
                        lock (_processLock)
                        {
                            _aExeProcess = process;
                        }

                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = $"\"{tempKnowedFile}\" \"{tempTranFile}\" {workspaceName}",
                            WorkingDirectory = pluginDir,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        };

                        process.Start();
                        const int timeoutMs = 30 * 60 * 1000; // 30分钟超时
                        if (!process.WaitForExit(timeoutMs))
                        {
                            process.Kill();
                            throw new Exception($"a.exe 运行超时（超过 {timeoutMs / 1000} 秒），已强制终止。");
                        }

                        // 读取输出（UI线程显示）
                        string error = process.StandardError.ReadToEnd();
                        string output = process.StandardOutput.ReadToEnd();

                        this.Invoke((Action)(() =>
                        {
                            //MessageBox.Show($"程序输出：\n{output}", "计算输出", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (!string.IsNullOrEmpty(error))
                                MessageBox.Show($"错误信息：\n{error}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));

                        if (process.ExitCode != 0)
                            throw new Exception($"a.exe 运行失败（退出码：{process.ExitCode}）\n错误信息：\n{error}");

                        return true;
                    }
                });

                // 计算完成后更新状态
                _isCalculationCompleted = true;
                timerProgress.Stop();
                progressBarCalculate.Value = 100;

                if (isSuccess)
                {
                    if (!Directory.Exists(fullWorkspacePath))
                    {
                        MessageBox.Show($"计算成功，但未找到结果目录！\n预期路径：{fullWorkspacePath}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"计算成功！\n结果已保存到：\n{fullWorkspacePath}", "计算完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (MessageBox.Show("是否立即打开结果目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Process.Start("explorer.exe", fullWorkspacePath);
                        }
                    }
                }

                //MessageBox.Show("=== TEM 计算流程完成 ===", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                timerProgress.Stop();
                progressBarCalculate.Value = 0;
                MessageBox.Show($"计算失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 清理临时文件
                if (!string.IsNullOrEmpty(tempKnowedFile) && File.Exists(tempKnowedFile))
                {
                    try { File.Delete(tempKnowedFile); }
                    catch (Exception ex) { MessageBox.Show($"删除临时文件失败：{ex.Message}", "警告"); }
                }
                if (!string.IsNullOrEmpty(tempTranFile) && File.Exists(tempTranFile))
                {
                    try { File.Delete(tempTranFile); }
                    catch (Exception ex) { MessageBox.Show($"删除临时文件失败：{ex.Message}", "警告"); }
                }

                // 加锁清空进程引用（确保线程安全）
                lock (_processLock)
                {
                    _aExeProcess = null;
                }
            }
        }
        /// <summary>
        /// 定时器事件：逐步更新进度条
        /// </summary>
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            // 如果计算已完成，直接跳到100%
            if (_isCalculationCompleted)
            {
                progressBarCalculate.Value = 100;
                return;
            }

            // 进度条最多增长到90%，留10%等待计算完成
            const int maxProgress = 90;
            // 每次增长1%（速度可通过定时器间隔调整）
            if (progressBarCalculate.Value < maxProgress)
            {
                progressBarCalculate.Value += 1;
            }
        }

        #endregion

        #region --- 数据导出（新增） ---

        /// <summary>
        /// 【新增】导出观测数据到文件（TEM 特有）
        /// 格式：LineName StationName X Y SamplingTime_us EffectiveArea InducedVoltage_mV
        /// </summary>
        private void ExportObservationDataToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // 写入表头
                writer.WriteLine("PROFILE\tSTATION\tCOORX\tCOORY\tTIMES\tRAREA\tCHZ");

                // 只导出选中测点的数据（核心修改）
                foreach (DataRow row in m_CurrentLineData.Rows)
                {
                    string stationName = row["测点号"].ToString();
                    // 仅保留选中测点的数据
                    if (stationName == m_CurrentSelectedStationName)
                    {
                        string lineName = row["测线号"].ToString();
                        double x = Convert.ToDouble(row["X"]);
                        double y = Convert.ToDouble(row["Y"]);
                        double samplingTime = Convert.ToDouble(row["采样时间_us"]);
                        double area = Convert.ToDouble(row["有效面积"]);
                        double voltage = Convert.ToDouble(row["感应电压_mV"]);

                        writer.WriteLine($"{lineName} {stationName} {x} {y} {samplingTime} {area} {voltage}");
                    }
                }
            }
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
            gridData.Columns.Add("colX", "X");
            gridData.Columns.Add("colY", "Y");
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
            if (chartProfileView.Series != null) chartProfileView.Series.Clear();
            if (chartVoltage.Series != null) chartVoltage.Series.Clear();

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
    }
}