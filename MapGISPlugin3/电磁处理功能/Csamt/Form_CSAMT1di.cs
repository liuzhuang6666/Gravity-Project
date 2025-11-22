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
using System.Windows.Forms.DataVisualization.Charting; // <-- 必须引用
using System.Runtime.InteropServices;
using MapGIS.GeoObjects.Geometry;
using System.IO; // 用于文件读写 (Path, StreamWriter)
using System.Diagnostics; // 用于执行 a.exe (Process)
using System.Reflection; // 用于获取 a.exe 的路径 (Assembly)
using System.Numerics; // 用于 Complex

namespace MapGISPlugin3
{
    public partial class Form_CSAMT1di : Form
    {
        // --- 核心变量 ---
        private IApplication _hook;
        private List<MapLayer> m_allPointLayers; // 存储所有点图层
        private List<MapLayer> m_allObjectLayers; // 存储所有对象类
        private SFeatureCls m_SelectedStationLayer; // 当前选中的测点要素类
        private ObjectCls m_SelectedSoundingTable; // 自动关联的测深数据表
        private List<StationInfo> m_CurrentLineStations; // 当前测线上的所有测点 (用于小地图)
        private DataTable m_CurrentLineData; // 当前测线上的所有数据 (用于表格)
        private string m_CurrentSelectedStationName; // 当前选中的测点号
        private Dictionary<string, (double Offset, double Distance)> m_StationDistances; // 测点偏移和距离
        private Point mousePoint = new Point();

        // 发射源位置
        private double Ax = 0, Ay = 0, Az = 0;
        private double Bx = 0, By = 0, Bz = 0;

        // 内部辅助类 (在文件底部定义)
        /// <summary>
        /// 构造函数
        /// </summary>
        public Form_CSAMT1di(IApplication hook)
        {
            InitializeComponent();
            InitDragEvent();
            _hook = hook;
            // 初始化列表
            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();
            m_StationDistances = new Dictionary<string, (double, double)>();
        }

        /// <summary>
        /// (辅助函数) 从 "电法数据" 地图加载图层列表 (已移除弹窗调试)
        /// </summary>
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

                int layerCount = 0;
                try
                {
                    layerCount = electroMap.LayerCount; // 获取图层数量
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"获取 '电法数据' 地图图层数量时出错: {ex.Message}", "LoadLayersFromMap - 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmbStationLayer.Enabled = false;
                    cmbLineName.Enabled = false;
                    return;
                }

                MapLayer layer = null;
                for (int i = 0; i < layerCount; i++)
                {
                    layer = null;
                    try
                    {
                        layer = electroMap.get_Layer(i);
                        if (layer == null)
                        {
                            Console.WriteLine($"图层索引 {i} 为 null，跳过。"); // 这个信息不关键，用 Console 即可
                            continue;
                        }
                        // 调用（已移除弹窗的）图层处理函数
                        ProcessLayerForComboBox_Debug(layer);
                    }
                    catch (COMException comEx)
                    {
                        MessageBox.Show($"遍历图层索引 {i} (名称: {layer?.Name ?? "未知"}) 时发生 COM 错误: {comEx.Message} (Code: {comEx.ErrorCode})", "LoadLayersFromMap - COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"遍历图层索引 {i} (名称: {layer?.Name ?? "未知"}) 时发生错误: {ex.Message}", "LoadLayersFromMap - 错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        // 不释放 layer
                    }
                } // 遍历结束
            }
            catch (COMException comEx) { MessageBox.Show($"查找地图或获取图层数COM错误: {comEx.Message}", "LoadLayersFromMap - 致命COM错误"); /* ... 禁用控件 ... */ return; }
            catch (Exception ex) { MessageBox.Show($"加载图层列表意外错误: {ex.Message}", "LoadLayersFromMap - 致命错误"); /* ... 禁用控件 ... */ return; }
            finally { Console.WriteLine("[LoadLayersFromMap] finally 块执行完毕。"); }

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

        /// <summary>
        /// (已移除弹窗调试) 处理单个图层，如果是组图层则递归处理
        /// </summary>
        private void ProcessLayerForComboBox_Debug(MapLayer layer)
        {
            if (layer == null) return;

            string layerName = "[无法获取名称]"; // 默认值
            string layerTypeName = "[无法获取类型]";
            try { layerName = layer.Name ?? "[名称为null]"; layerTypeName = layer.GetType().Name; } catch { /* 忽略获取名称/类型的错误 */ }

            try
            {
                // 检查是否是组图层
                if (layer is GroupLayer groupLayer)
                {
                    int subLayerCount = 0;
                    try { subLayerCount = groupLayer.Count; } catch { } // 安全获取数量

                    MapLayer subLayer = null;
                    for (int i = 0; i < subLayerCount; i++) // 使用安全获取的数量
                    {
                        subLayer = null;
                        try
                        {
                            subLayer = groupLayer.get_Item(i);
                            // 递归调用（已移除弹窗的）版本
                            ProcessLayerForComboBox_Debug(subLayer);
                        }
                        catch (Exception ex) { MessageBox.Show($"处理组 '{layerName}' 的子图层 {i} 时出错: {ex.Message}", "ProcessLayer - 递归错误"); }
                        finally { /* 不释放 subLayer */ }
                    }
                }
                // 检查是否是 VectorLayer
                else if (layer is VectorLayer vectorLayer)
                {
                    GeomType geomType = GeomType.Unknown; // 默认值
                    bool isPoint = false;
                    try
                    {
                        geomType = vectorLayer.GeometryType;
                        isPoint = (geomType == GeomType.Pnt);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"获取图层 '{layerName}' 的几何类型时出错: {ex.Message}", "ProcessLayer - 错误");
                        return; // 获取类型失败，无法继续判断
                    }

                    if (isPoint) // 几何类型是点
                    {
                        bool nameMatch = false;
                        if (layerName != null && layerName.Contains("测点"))
                        {
                            nameMatch = true;
                        }

                        if (nameMatch) // 名称也匹配
                        {
                            m_allPointLayers.Add(layer);
                            cmbStationLayer.Items.Add(layer);
                        }
                    }
                }
                // 检查是否是 ObjectLayer
                else if (layer is ObjectLayer objectLayer)
                {
                    // 并且名称包含 "测深数据"
                    if (layerName != null && layerName.Contains("测深数据"))
                    {
                        m_allObjectLayers.Add(layer);
                    }
                }
            }
            catch (COMException comEx) // 捕获访问 layer 属性（如 Name, GeometryType）时可能发生的错误
            {
                MessageBox.Show($"处理图层 '{layerName}' 时发生 COM 错误: {comEx.Message} (Code: {comEx.ErrorCode})", "ProcessLayer - COM 错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理图层 '{layerName}' 时发生错误: {ex.Message}", "ProcessLayer - 错误");
            }
            // *** 不释放 layer ***
        }

        #region --- 1. 主控件事件 (左栏顶部) ---
        /// <summary>
        /// 事件: 当用户选择一个新的 "测点图层" (使用原始变量名)
        /// </summary>
        private void cmbStationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            // --- 1. 清理旧状态 ---
            if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
            m_SelectedStationLayer = null; // SFeatureCls
            if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
            m_SelectedSoundingTable = null; // ObjectCls
            cmbLineName.Items.Clear(); // 清空测线下拉框
            ClearAllDisplays(); // 清空所有图表、表格和小地图

            if (cmbStationLayer.SelectedItem == null || !(cmbStationLayer.SelectedItem is MapLayer selectedLayer))
            {
                cmbLineName.Enabled = false;
                Console.WriteLine("cmbStationLayer_SelectedIndexChanged: No valid MapLayer selected.");
                return;
            }

            Console.WriteLine($"用户选择了测点图层: {selectedLayer.Name}");

            // --- 2. 获取选中的测点图层和要素类 ---
            try
            {
                m_SelectedStationLayer = selectedLayer.GetData() as SFeatureCls;
                if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
                {
                    MessageBox.Show($"无法从图层 '{selectedLayer.Name}' 获取有效的要素类数据 (SFeatureCls)！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmbLineName.Enabled = false;
                    m_SelectedStationLayer = null; // 确保清空
                    return;
                }
                Console.WriteLine($"成功获取选中测点图层的 SFeatureCls: {selectedLayer.Name}");
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"获取图层 '{selectedLayer.Name}' 数据 (SFeatureCls) 时发生 COM 错误: {comEx.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error getting SFeatureCls for {selectedLayer.Name}: {comEx}");
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取图层 '{selectedLayer.Name}' 数据 (SFeatureCls) 时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error getting SFeatureCls for {selectedLayer.Name}: {ex}");
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                return;
            }

            // --- 3. 自动关联测深数据表 ---
            string stationLayerName = selectedLayer.Name;
            string expectedTableName = stationLayerName.Replace("测点", "测深数据");
            Console.WriteLine($"尝试查找匹配的测深数据表: {expectedTableName}");

            MapLayer soundingLayer = m_allObjectLayers.FirstOrDefault(layer => layer != null && layer.Name == expectedTableName);
            if (soundingLayer == null)
            {
                MessageBox.Show($"未在 '电法数据' 地图中找到与 '{stationLayerName}' 匹配的测深数据表 '{expectedTableName}'！\n请检查图层命名规范。", "关联失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                return;
            }

            Console.WriteLine($"找到可能的测深数据表图层: {soundingLayer.Name}");

            try
            {
                m_SelectedSoundingTable = soundingLayer.GetData() as ObjectCls;
                if (m_SelectedSoundingTable == null || !m_SelectedSoundingTable.HasOpen())
                {
                    MessageBox.Show($"图层 '{expectedTableName}' 不是有效的对象类 (ObjectCls) 或无法打开！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_SelectedSoundingTable = null; // 确保清空
                    cmbLineName.Enabled = false;
                    if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                    m_SelectedStationLayer = null;
                    return;
                }
                Console.WriteLine($"成功关联到测深数据表 ObjectCls: {soundingLayer.Name}");
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"获取测深表 '{soundingLayer.Name}' 数据 (ObjectCls) 时发生 COM 错误: {comEx.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error getting ObjectCls for {soundingLayer.Name}: {comEx}");
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
                m_SelectedSoundingTable = null;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取测深表 '{soundingLayer.Name}' 数据 (ObjectCls) 时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error getting ObjectCls for {soundingLayer.Name}: {ex}");
                cmbLineName.Enabled = false;
                if (m_SelectedStationLayer != null) try { Marshal.ReleaseComObject(m_SelectedStationLayer); } catch { }
                m_SelectedStationLayer = null;
                if (m_SelectedSoundingTable != null) try { Marshal.ReleaseComObject(m_SelectedSoundingTable); } catch { }
                m_SelectedSoundingTable = null;
                return;
            }

            // --- 4. (如果成功) 填充 cmbLineName (测线下拉框) ---
            cmbLineName.Enabled = true;
            FillLineComboBox(); // 调用辅助函数填充测线列表

            // --- 5. (如果成功且有测线) 自动选中第一条测线 ---
            if (cmbLineName.Items.Count > 0)
            {
                cmbLineName.Text = "请选择测线..."; // 给出提示
                Console.WriteLine("测线列表已填充，等待用户选择。");
            }
            else
            {
                MessageBox.Show($"在测点图层 '{stationLayerName}' 中未能查询到任何测线号。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbLineName.Enabled = false;
                ClearAllDisplays();
            }
        }

        /// <summary>
        /// 事件: 当用户选择一个新的 "测线" (主刷新函数) - 【!! 调试中 !!】
        /// </summary>
        private void cmbLineName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 0. 基本检查和清空
            ClearAllDisplays(); // 清空所有旧数据
            if (cmbLineName.SelectedItem == null || m_SelectedStationLayer == null || m_SelectedSoundingTable == null)
            {
                return;
            }

            string selectedLine = cmbLineName.SelectedItem.ToString();
            this.Cursor = Cursors.WaitCursor; // 开始等待

            try
            {
                // 1. 查询 SFeatureCls (点图层)，获取所有测点 (X, Y, 测点号)
                m_CurrentLineStations = QueryStationsForLine(selectedLine);

                // 2. 查询 ObjectCls (测深表)，获取该测线所有数据
                m_CurrentLineData = QuerySoundingDataForLine(selectedLine);

                // 3. 刷新"计算"页的小地图
                UpdateProfileView();

                // 4. 刷新"布置图"页的小地图
                UpdateLayoutView();

                // 5. 刷新"数据"页的表格
                UpdateDataGrids();

                // 6.自动选中第一个点
                if (m_CurrentLineStations.Count > 0)
                {
                    string firstStation = m_CurrentLineStations[0].StationName;
                    // 自动选中第一个点并刷新右侧图表
                    SelectStationAndRefreshCharts(firstStation);
                }
                else
                {
                    Console.WriteLine("cmbLineName_SelectedIndexChanged: 未查询到测点，跳过自动选中。");
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生 COM 错误: {comEx.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error in cmbLineName_SelectedIndexChanged: {comEx}");
                ClearAllDisplays(); // 出错时清空
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载测线 '{selectedLine}' 数据时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error in cmbLineName_SelectedIndexChanged: {ex}");
                ClearAllDisplays(); // 出错时清空
            }
            finally
            {
                this.Cursor = Cursors.Default; // 结束等待
            }
        }
        #endregion

        #region --- 2. 标签页控件事件 (左栏下方) ---
        /// <summary>
        /// 事件: 当用户在 "小地图" 上点击 (实现可视化点选)
        /// </summary>
        private void chartProfileView_MouseClick(object sender, MouseEventArgs e)
        {
            // 使用 HitTest 来查找点击位置的图表元素
            var hitTestResult = chartProfileView.HitTest(e.X, e.Y);

            // 检查是否点中了数据点 (DataPoint)
            if (hitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                // 从数据点中获取我们存储的测点信息
                DataPoint dataPoint = (DataPoint)hitTestResult.Object;
                // 在 UpdateProfileView 中, 我们把测点名存在了 Tag 里
                string stationName = dataPoint.Tag?.ToString();
                // (备用方案: 如果 Tag 为空, 尝试用 Label)
                if (string.IsNullOrEmpty(stationName))
                {
                    stationName = dataPoint.Label;
                }

                if (!string.IsNullOrEmpty(stationName))
                {
                    // 找到了测点名, 调用您现有的刷新函数
                    SelectStationAndRefreshCharts(stationName);
                }
            }
        }

        /// <summary>
        /// 事件: 点击 "更新距离" 按钮 (布置图)
        /// </summary>
        private void btnUpdateDistances_Click(object sender, EventArgs e)
        {
            UpdateDistances();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // --- 0. 检查发射源坐标 ---
            bool allZero = true;

            try
            {
                // 安全地检查每个坐标值
                double ax = SafeParseDouble(txtAx.Text);
                double ay = SafeParseDouble(txtAy.Text);
                double az = SafeParseDouble(txtAz.Text);
                double bx = SafeParseDouble(txtBx.Text);
                double by = SafeParseDouble(txtBy.Text);
                double bz = SafeParseDouble(txtBz.Text);

                // 检查是否所有坐标都是0
                allZero = (ax == 0 && ay == 0 && az == 0 && bx == 0 && by == 0 && bz == 0);
            }
            catch (Exception ex)
            {
                // 如果解析过程中出现异常，也认为是未设置
                allZero = true;
                Console.WriteLine($"解析坐标时出错: {ex.Message}");
            }

            if (allZero)
            {
                var result = MessageBox.Show("发射源坐标未设置或全部为0，请在布置图页面设置发射源坐标后再进行计算。\n\n是否立即跳转到布置图页面？",
                    "发射源坐标未设置",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // 切换到布置图标签页
                    tabControl1.SelectedTab = tabPageLayout;
                }
                return; // 停止计算
            }

            // --- 1. 检查数据 ---
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("没有加载任何测线数据，无法计算。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            string tempRunDir = Path.Combine(Path.GetTempPath(), "CSAMT1di_run_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            try
            {
                Directory.CreateDirectory(tempRunDir);

                int its = (int)nudIterationCount.Value;

                // --- 2. 定义路径 ---
                string exePath;
                string pluginDir;
                string algorithmDir;

                try
                {
                    pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    algorithmDir = Path.Combine(pluginDir, "Algorithm", "CSAMT1di");
                    exePath = Path.Combine(algorithmDir, "a.exe");

                    if (!File.Exists(exePath))
                    {
                        throw new FileNotFoundException($"计算程序 'a.exe' 未找到。\n请确保它位于: {exePath}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"查找 'a.exe' 失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return;
                }

                string tempDataFile = Path.Combine(tempRunDir, "knowed.dat");
                string tempTranFile = Path.Combine(tempRunDir, "tran.dat");
                string workspaceName = "results";
                string fullWorkspacePath = Path.Combine(tempRunDir, workspaceName);

                // --- 3. 数据转换 (写入临时文件) ---
                Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);
                using (StreamWriter writer = new StreamWriter(tempDataFile))
                {
                    foreach (DataRow row in m_CurrentLineData.Rows)
                    {
                        string lineName = row["测线编号"].ToString();
                        string stationName = row["测点编号"].ToString();
                        StationInfo station;
                        if (!stationCoords.TryGetValue(stationName, out station)) { continue; }

                        double freq = GetDoubleFromRow(row, "频率", 0.0);
                        double res = GetDoubleFromRow(row, "视电阻率", 0.0);
                        double pha = GetDoubleFromRow(row, "相位", 0.0);

                        writer.WriteLine($"{lineName} {stationName} {station.X} {station.Y} {freq} {res} {pha}");
                    }
                }

                using (StreamWriter writer = new StreamWriter(tempTranFile))
                {
                    writer.WriteLine($"{Ax} {Ay} {Az}");
                    writer.WriteLine($"{Bx} {By} {Bz}");
                }

                // --- 4. 执行 a.exe ---
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = exePath;
                startInfo.Arguments = $"\"{tempDataFile}\" \"{tempTranFile}\" \"{workspaceName}\" {its}";
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

                    // --- 5. 反馈结果 ---
                    if (process.ExitCode == 0)
                    {
                        // --- (核心修改) 读取 KNOW 文件并调用可视化函数 ---
                        string knowFile = Path.Combine(fullWorkspacePath, "KNOW");
                        if (File.Exists(knowFile))
                        {
                            // 调用解析和可视化函数
                            List<InversionResultPoint> results = ParseKnowFile(knowFile);
                            DisplayInversionResults(results);
                        }
                        else
                        {
                            // 如果文件不存在，清空结果图
                            DisplayInversionResults(new List<InversionResultPoint>());
                            MessageBox.Show("计算成功，但未找到 KNOW 结果文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        MessageBox.Show($"计算成功！\n\n反演结果剖面图已生成。\n结果文件位于:\n{fullWorkspacePath}",
                                        "计算完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        throw new Exception($"a.exe 运行失败 (ExitCode: {process.ExitCode})。\n\n工作目录:\n{startInfo.WorkingDirectory}\n\n命令行:\n{startInfo.FileName} {startInfo.Arguments}\n\n错误信息:\n{error}\n\n输出信息:\n{output}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"计算过程中发生严重错误: \n{ex.Message}", "计算失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // --- 6. 清理 ---
                if (Directory.Exists(tempRunDir))
                {
                    try
                    {
                        Directory.Delete(tempRunDir, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"删除临时目录 {tempRunDir} 失败: {ex.Message}");
                    }
                }
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 安全地将字符串转换为double，处理空值和非数字字符
        /// </summary>
        private double SafeParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            if (double.TryParse(text, out double result))
                return result;

            return 0;
        }


        /// <summary>
        /// 事件: 点击 "加载文件..." 按钮，从 tran.dat 文件加载发射源坐标
        /// </summary>
        private void btnLoadTranFile_Click(object sender, EventArgs e)
        {
            // 使用我们在 Designer 中添加的 openFileDialogTran 控件
            if (openFileDialogTran.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialogTran.FileName;
                try
                {
                    // 读取文件的所有行
                    string[] lines = File.ReadAllLines(filePath);

                    // 检查文件内容是否至少有两行
                    if (lines.Length < 2)
                    {
                        throw new FormatException("文件内容不足两行，无法解析发射源A点和B点。");
                    }

                    // --- 解析第一行 (A点坐标) ---
                    // 使用空格和制表符作为分隔符，并移除空条目
                    string[] partsA = lines[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partsA.Length < 3)
                    {
                        throw new FormatException("第一行坐标数据不足三项 (应为 X Y Z)。");
                    }
                    // 尝试将字符串转换为 double，以验证格式
                    double.Parse(partsA[0]);
                    double.Parse(partsA[1]);
                    double.Parse(partsA[2]);

                    // --- 解析第二行 (B点坐标) ---
                    string[] partsB = lines[1].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partsB.Length < 3)
                    {
                        throw new FormatException("第二行坐标数据不足三项 (应为 X Y Z)。");
                    }
                    double.Parse(partsB[0]);
                    double.Parse(partsB[1]);
                    double.Parse(partsB[2]);

                    // --- 如果解析成功，更新UI文本框 ---
                    txtAx.Text = partsA[0];
                    txtAy.Text = partsA[1];
                    txtAz.Text = partsA[2];

                    txtBx.Text = partsB[0];
                    txtBy.Text = partsB[1];
                    txtBz.Text = partsB[2];

                    MessageBox.Show("发射源坐标已成功从文件加载！", "加载成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // (可选，但推荐) 自动更新距离表格，提供即时反馈
                    UpdateDistances();
                }
                catch (FormatException fex)
                {
                    MessageBox.Show($"文件格式错误: {fex.Message}", "解析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取或解析文件时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
        #region --- 3. 分析区事件 (右栏) ---
        // (无修改)
        #endregion

        #region --- 4. 核心辅助函数 (被事件调用) ---
        /// <summary>
        /// (辅助函数) 安全地从 DataRow 获取 double 值，处理 DBNull
        /// </summary>
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

        /// <summary>
        /// (辅助函数) 查找地图
        /// </summary>
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

        /// <summary>
        /// (辅助函数) 填充测线下拉框 (cmbLineName) (已修复死循环)
        /// </summary>
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
                    MessageBox.Show($"预加载记录集出错: {ex.Message} (可能数据损坏或 COM 问题)");
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
                        if (currentIndex > rs.Count * 1.5 && rs.Count > 0)
                        {
                            MessageBox.Show($"迭代超过预期 ({currentIndex} > {rs.Count})，强制停止循环（可能 MoveNext Bug）");
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
                                    validRecordCount++;
                                }
                            }
                        }
                    }
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message} (可能该记录损坏，跳过)");
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
                else
                {
                    MessageBox.Show($"查询未返回任何唯一值。（有效记录: {validRecordCount}；所有'测线号'可能为空，用 MapGIS 桌面验证数据）");
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"查询过程中发生 COM 错误: {comEx.Message} (Code: {comEx.ErrorCode})", "错误");
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

        /// <summary>
        /// (辅助函数) 清空所有显示区域 (图表、表格、小地图)
        /// </summary>
        private void ClearAllDisplays()
        {
            Console.WriteLine("执行 ClearAllDisplays...");

            if (chartProfileView.Series != null) chartProfileView.Series.Clear();
            if (chartLayout.Series != null) chartLayout.Series.Clear();
            if (chartResistivity.Series != null) chartResistivity.Series.Clear();
            if (chartPhase.Series != null) chartPhase.Series.Clear();

            // --- (修改) 调用新的函数来清空结果图 ---
            DisplayInversionResults(new List<InversionResultPoint>());

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
            if (chartLayout.ChartAreas.Count > 0)
            {
                chartLayout.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartLayout.ChartAreas[0].AxisY.IsLogarithmic = false;
            }

            gridData.DataSource = null;
            gridLayout.DataSource = null;
            // --- (已删除) rtbResults.Text = ""; ---

            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_StationDistances.Clear();
            m_CurrentSelectedStationName = null;

            if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = "频率-视电阻率";
            if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = "频率-相位";
        }
        /// <summary>
        /// (辅助函数) 刷新"计算"页的小地图 (chartProfileView)
        /// </summary>
        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;

            chartProfileView.Series.Clear();
            chartProfileView.BackColor = System.Drawing.Color.White;

            Series s = chartProfileView.Series.Add("Stations");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 8;
            s.MarkerColor = System.Drawing.Color.Blue;
            s.IsValueShownAsLabel = true;
            s.LegendText = "测点";

            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                Console.WriteLine("UpdateProfileView: m_CurrentLineStations 为空，不绘制。");
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

        /// <summary>
        /// (辅助函数) 刷新"布置图"页的小地图 (chartLayout)
        /// </summary>
        private void UpdateLayoutView()
        {
            if (chartLayout == null) return;

            chartLayout.Series.Clear();
            chartLayout.BackColor = System.Drawing.Color.White;

            // --- 配置 chartLayout 图例 (上方右侧) ---
            chartLayout.Legends.Clear();
            Legend layoutLegend = chartLayout.Legends.Add("LegendLayout");
            layoutLegend.Docking = Docking.Top;
            layoutLegend.Alignment = StringAlignment.Far;
            layoutLegend.BackColor = System.Drawing.Color.White;
            layoutLegend.BorderColor = System.Drawing.Color.LightGray;
            layoutLegend.BorderWidth = 1;
            layoutLegend.Font = new System.Drawing.Font("微软雅黑", 8f);
            layoutLegend.LegendStyle = LegendStyle.Table;
            layoutLegend.IsEquallySpacedItems = false;
            layoutLegend.TableStyle = LegendTableStyle.Auto;

            Series s = chartLayout.Series.Add("Stations");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 8;
            s.MarkerColor = System.Drawing.Color.Red;
            s.IsValueShownAsLabel = true;
            s.LegendText = "测点";

            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                Console.WriteLine("UpdateLayoutView: m_CurrentLineStations 为空，不绘制。");
                return;
            }

            foreach (var station in m_CurrentLineStations)
            {
                int pointIndex = s.Points.AddXY(station.X, station.Y);
                s.Points[pointIndex].Label = station.StationName;
                s.Points[pointIndex].Tag = station.StationName;
                s.Points[pointIndex].Color = System.Drawing.Color.Blue;
            }

            chartLayout.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            chartLayout.ChartAreas[0].AxisY.LabelStyle.Format = "F0";
            chartLayout.ChartAreas[0].AxisX.IsStartedFromZero = false;
            chartLayout.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chartLayout.ChartAreas[0].RecalculateAxesScale();
        }

        /// <summary>
        /// (辅助函数) 更新测点偏移和距离
        /// </summary>
        private void UpdateDistances()
        {
            if (m_CurrentLineStations == null || m_CurrentLineStations.Count == 0)
            {
                return;
            }

            // 获取发射源位置 (从输入框)
            if (!double.TryParse(txtAx.Text, out Ax) || !double.TryParse(txtAy.Text, out Ay) || !double.TryParse(txtAz.Text, out Az) ||
                !double.TryParse(txtBx.Text, out Bx) || !double.TryParse(txtBy.Text, out By) || !double.TryParse(txtBz.Text, out Bz))
            {
                MessageBox.Show("发射源坐标输入无效，请检查数值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            m_StationDistances.Clear();

            Complex tranA = new Complex(Ax, Ay);
            Complex tranB = new Complex(Bx, By);
            Complex tranAB = tranB - tranA;
            double ab = Complex.Abs(tranAB);
            if (ab == 0)
            {
                MessageBox.Show("发射源 A 和 B 位置相同，无法计算。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
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

            // 更新 gridLayout
            DataTable dtLayout = new DataTable();
            dtLayout.Columns.Add("测点编号", typeof(string));
            dtLayout.Columns.Add("偏移距离", typeof(double));
            dtLayout.Columns.Add("收发距离", typeof(double));

            foreach (var kvp in m_StationDistances)
            {
                dtLayout.Rows.Add(kvp.Key, kvp.Value.Offset, kvp.Value.Distance);
            }

            gridLayout.DataSource = dtLayout;
            gridLayout.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        /// <summary>
        /// (辅助函数) 选中一个测点并刷新右栏图表
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
            catch (Exception ex) { Console.WriteLine($"高亮小地图测点时出错: {ex.Message}"); }

            UpdateRightPanelCharts();
        }

        /// <summary>
        /// (辅助函数) 刷新右栏的两张曲线图
        /// </summary>
        private void UpdateRightPanelCharts()
        {
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();

            // --- 为所有图表设置统一的白色背景 ---
            chartResistivity.BackColor = System.Drawing.Color.White;
            chartPhase.BackColor = System.Drawing.Color.White;
            chartProfileView.BackColor = System.Drawing.Color.White;

            // --- 清除现有图例并重新配置 ---
            chartResistivity.Legends.Clear();
            chartPhase.Legends.Clear();
            chartProfileView.Legends.Clear();

            // --- 配置 chartResistivity 图例 (上方右侧) ---
            Legend resLegend = chartResistivity.Legends.Add("LegendResistivity");
            resLegend.Docking = Docking.Top; // 顶部停靠
            resLegend.Alignment = StringAlignment.Far; // 右侧对齐
            resLegend.BackColor = System.Drawing.Color.White;
            resLegend.BorderColor = System.Drawing.Color.LightGray;
            resLegend.BorderWidth = 1;
            resLegend.Font = new System.Drawing.Font("微软雅黑", 8f);
            resLegend.LegendStyle = LegendStyle.Table;
            resLegend.IsEquallySpacedItems = false; // 允许不等间距
            resLegend.TableStyle = LegendTableStyle.Auto; // 自动表格样式

            // --- 配置 chartPhase 图例 (上方右侧) ---
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

            // --- 配置 chartProfileView 图例 (上方右侧) ---
            chartProfileView.Legends.Clear(); // 先清除所有图例
            Legend profileLegend = chartProfileView.Legends.Add("LegendProfile");
            profileLegend.Docking = Docking.Top;
            profileLegend.Alignment = StringAlignment.Far;
            profileLegend.BackColor = System.Drawing.Color.White;
            profileLegend.BorderColor = System.Drawing.Color.LightGray;
            profileLegend.BorderWidth = 1;
            profileLegend.Font = new System.Drawing.Font("微软雅黑", 8f);
            profileLegend.LegendStyle = LegendStyle.Table;
            profileLegend.IsEquallySpacedItems = false;
            profileLegend.TableStyle = LegendTableStyle.Auto;


            // --- (新增结束) ---

            if (string.IsNullOrEmpty(m_CurrentSelectedStationName) || m_CurrentLineData == null)
            {
                if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = "频率-视电阻率";
                if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = "频率-相位";
                chartResistivity.ChartAreas[0].AxisX.Title = "频率(Hz)";
                chartResistivity.ChartAreas[0].AxisY.Title = "视电阻率";
                chartPhase.ChartAreas[0].AxisX.Title = "频率(Hz)";
                chartPhase.ChartAreas[0].AxisY.Title = "相位";
                return;
            }

            string resField = "视电阻率";
            string phaseField = "相位";
            string resSeriesName = "视电阻率";
            string phaseSeriesName = "相位";

            Console.WriteLine($"UpdateRightPanelCharts: resField={resField}, phaseField={phaseField}");

            if (string.IsNullOrEmpty(resField) || string.IsNullOrEmpty(phaseField))
            {
                MessageBox.Show("内部错误：resField 或 phaseField 未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = $"{m_CurrentSelectedStationName} - {resSeriesName}";
            if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = $"{m_CurrentSelectedStationName} - {phaseSeriesName}";

            chartResistivity.ChartAreas[0].AxisX.Title = "频率(Hz)";
            chartResistivity.ChartAreas[0].AxisY.Title = "视电阻率";
            chartPhase.ChartAreas[0].AxisX.Title = "频率(Hz)";
            chartPhase.ChartAreas[0].AxisY.Title = "相位";

            DataView dvStation = new DataView(m_CurrentLineData);
            try
            {
                dvStation.RowFilter = $"测点编号 = '{m_CurrentSelectedStationName}'";
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

            var resSeries = chartResistivity.Series.Add(resSeriesName);
            var phaseSeries = chartPhase.Series.Add(phaseSeriesName);

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

            if (freqs.Count == 0)
            {
                Console.WriteLine("无有效数据点，跳过绘制。");
                return;
            }

            bool canLogX = freqs.All(f => f > 0);
            bool canLogYRes = resistivities.All(r => r > 0);

            chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = canLogX;
            chartResistivity.ChartAreas[0].AxisY.IsLogarithmic = canLogYRes;
            chartPhase.ChartAreas[0].AxisX.IsLogarithmic = canLogX;
            chartPhase.ChartAreas[0].AxisY.IsLogarithmic = false;

            chartResistivity.ChartAreas[0].AxisX.IsReversed = true;
            chartPhase.ChartAreas[0].AxisX.IsReversed = true;

            if (!canLogX) Console.WriteLine("警告: 频率有 ≤0 值，使用线性轴。");
            if (!canLogYRes) Console.WriteLine("警告: 视电阻率有 ≤0 值，使用线性轴。");

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

            chartResistivity.ChartAreas[0].RecalculateAxesScale();
            chartPhase.ChartAreas[0].RecalculateAxesScale();

            BeautifyChartAxes(chartResistivity.ChartAreas[0]);
            BeautifyChartAxes(chartPhase.ChartAreas[0]);
        }

        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;

            // --- (新增) 为图表区域添加黑色实线边框 ---
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.BorderColor = System.Drawing.Color.Black;
            area.BorderWidth = 1;

            // --- (新增) Y轴标题样式：水平放置在轴的顶部 ---
            area.AxisY.TitleAlignment = StringAlignment.Near; // 标题放在Y轴顶部
            //area.AxisY.TextOrientation = TextOrientation.Horizontal; // 标题水平显示

            // 原有代码
            area.AxisX.LabelStyle.Format = "0.###";
            area.AxisY.LabelStyle.Format = "0.##";
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 9f);
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 9f);
            area.AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 10f);
            area.AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 10f);
            area.AxisX.LabelStyle.Angle = 0;
            area.AxisX.LabelStyle.IsStaggered = false;

            if (area.AxisX.IsLogarithmic)
            {
                area.AxisX.LogarithmBase = 10;
                area.AxisX.Interval = 1;
            }
            else
            {
                area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            }

            if (area.AxisY.IsLogarithmic)
            {
                area.AxisY.LogarithmBase = 10;
                area.AxisY.Interval = 1;
            }
            else
            {
                area.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            }

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

            area.RecalculateAxesScale();
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
                    MessageBox.Show($"严重错误：字段 '{lineField}' 不存在！\n请检查是否叫 '线号'、'LineNo'、'测线' 等");
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
                    MessageBox.Show($"查询无结果！\n" +
                                    $"Filter: {query.Filter}\n" +
                                    $"lineName = '{lineName}' (长度: {lineName.Length})\n" +
                                    $"请检查测线号是否正确、是否有空格或大小写问题");
                    return stations;
                }

                rs.MoveLast();
                rs.MoveFirst();
                int totalRecords = rs.Count;
                int geomFailCount = 0;
                int attNullCount = 0;
                int nameNullCount = 0;
                int successCount = 0;
                string firstGeomType = "N/A";
                bool fieldNameError = false;
                int currentIndex = 0;

                if (totalRecords == 0)
                {
                    MessageBox.Show("遍历报告: totalRecords 为 0，循环前退出。");
                    return stations;
                }

                do
                {
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

                        att = rs.Att;
                        if (att == null)
                        {
                            attNullCount++;
                            continue;
                        }

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"异常：{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (rs != null)
                {
                    try { Marshal.ReleaseComObject(rs); } catch (Exception ex) { Console.WriteLine($"释放 RecordSet (rs) 时出错: {ex.Message}"); }
                }
            }

            return stations.OrderBy(s => s.X).ToList();
        }

        /// <summary>
        /// (辅助函数) 查询指定测线的所有测深数据到 DataTable (V7.6.0.0 兼容版)
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
                    Filter = $"测线编号 = '{lineName}'",
                    SubFields2 = "*",
                };

                rs = m_SelectedSoundingTable.Select(query);
                if (rs == null)
                {
                    MessageBox.Show("QuerySoundingDataForLine: Select(query) 返回 null，查询失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return dataTable;
                }

                try
                {
                    rs.MoveLast();
                    rs.MoveFirst();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"QuerySoundingDataForLine: 预加载 (MoveLast) 失败: {ex.Message}。继续尝试迭代...", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    Console.WriteLine("QuerySoundingDataForLine: totalRecords 为 0，循环前退出。");
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
                            MessageBox.Show($"[调试] 迭代超过预期 ({currentIndex} > {rs.Count})，强制停止循环。", "QuerySoundingDataForLine - 安全退出");
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
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message} (可能该记录损坏，跳过)", "QuerySoundingDataForLine - 迭代错误");
                    }
                    finally
                    {
                        if (att != null) { try { Marshal.ReleaseComObject(att); } catch { } }
                    }
                } while (rs.MoveNext() && !rs.IsEOF);
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
        /// (辅助函数) 刷新 数据 页的表格
        /// </summary>
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
                gridData.DataSource = dvData.ToTable(false,
                    "测点编号",
                    "频率",
                    "视电阻率",
                    "相位"
                );

                gridData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"填充数据表格时出错: {ex.Message}\n\n请检查 '测点编号', '频率', '视电阻率' 等字段名是否与数据表一致。", "表格错误");
            }
        }

        /// <summary>
        /// (新增辅助函数) 解析 KNOW 文件，返回结果数据列表
        /// </summary>
        private List<InversionResultPoint> ParseKnowFile(string filePath)
        {
            var results = new List<InversionResultPoint>();
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"结果文件不存在: {filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return results;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // 文件格式: “测线编号 测点编号 测点x坐标 测点y坐标 深度 电阻率值”
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
                MessageBox.Show($"解析 KNOW 文件时出错: {ex.Message}", "文件解析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return results;
        }

        /// <summary>
        /// (新增辅助函数) 根据数值和范围获取颜色，用于热力图
        /// </summary>
        /// <summary>
        /// (辅助函数) 根据数值和范围获取颜色，用于热力图
        /// </summary>
        // (修改) 在返回值类型前明确指定 System.Drawing.Color
        private System.Drawing.Color GetColorForValue(double value, double min, double max)
        {
            // (修改) 明确指定 System.Drawing.Color
            if (min >= max) return System.Drawing.Color.Black;

            // 归一化到 0-1 范围
            double normalized = (value - min) / (max - min);

            // 简单的彩虹色谱 (蓝 -> 青 -> 绿 -> 黄 -> 红)
            // (修改) 明确指定 System.Drawing.Color
            if (normalized < 0.25) // 蓝 -> 青
                return System.Drawing.Color.FromArgb(0, (int)(255 * (normalized * 4)), 255);
            if (normalized < 0.5) // 青 -> 绿
                return System.Drawing.Color.FromArgb(0, 255, (int)(255 * (1 - (normalized - 0.25) * 4)));
            if (normalized < 0.75) // 绿 -> 黄
                return System.Drawing.Color.FromArgb((int)(255 * ((normalized - 0.5) * 4)), 255, 0);

            // 黄 -> 红
            // (修改) 明确指定 System.Drawing.Color
            return System.Drawing.Color.FromArgb(255, (int)(255 * (1 - (normalized - 0.75) * 4)), 0);
        }

        /// <summary>
        /// (新增辅助函数) 将反演结果数据显示在剖面图上
        /// </summary>
        /// <summary>
        /// (辅助函数) 将反演结果数据显示在剖面图上
        /// </summary>
        private void DisplayInversionResults(List<InversionResultPoint> results)
        {
            var chart = chartResultSection;

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();

            if (results == null || results.Count == 0)
            {
                return;
            }

            chart.Titles.Add("一维反演电阻率剖面图");

            ChartArea ca = chart.ChartAreas.Add("ResultArea");
            ca.AxisX.Title = "测点坐标 (X)";
            ca.AxisY.Title = "深度 (m)";
            ca.AxisY.IsReversed = true;

            Series s = chart.Series.Add("ResistivitySection");
            s.ChartType = SeriesChartType.Point;
            s.MarkerStyle = MarkerStyle.Square;
            s.MarkerSize = 10;

            var validResults = results.Where(r => r.Resistivity > 0).ToList();
            if (validResults.Count == 0)
            {
                MessageBox.Show("结果文件中没有有效的（>0）电阻率值。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var logResistivities = validResults.Select(r => Math.Log10(r.Resistivity)).ToList();
            double minLogRes = logResistivities.Min();
            double maxLogRes = logResistivities.Max();

            Legend legend = chart.Legends.Add("ColorScale");
            legend.Docking = Docking.Right;
            legend.Alignment = StringAlignment.Center;
            legend.Title = "lg(ρ/Ω·m)";
            // (修改) 明确指定 System.Drawing.Font
            legend.TitleFont = new System.Drawing.Font("微软雅黑", 9F, FontStyle.Bold);

            int legendSteps = 7;
            for (int i = 0; i <= legendSteps; i++)
            {
                double val = minLogRes + (maxLogRes - minLogRes) * i / legendSteps;
                // (修改) 明确指定 System.Windows.Forms.DataVisualization.Charting.LegendItem
                System.Windows.Forms.DataVisualization.Charting.LegendItem item = new System.Windows.Forms.DataVisualization.Charting.LegendItem();
                item.Name = Math.Pow(10, val).ToString("0.#");
                item.Color = GetColorForValue(val, minLogRes, maxLogRes);
                item.MarkerStyle = MarkerStyle.Square;
                item.MarkerSize = 12;
                legend.CustomItems.Add(item);
            }

            foreach (var point in validResults)
            {
                double logRes = Math.Log10(point.Resistivity);
                int pointIndex = s.Points.AddXY(point.X, point.Depth);
                s.Points[pointIndex].Color = GetColorForValue(logRes, minLogRes, maxLogRes);
            }

            BeautifyChartAxes(ca);
            ca.AxisX.MinorGrid.Enabled = false;
            ca.AxisY.MinorGrid.Enabled = false;
        }
        #endregion

        // (内部辅助类)
        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        // --- (新增) 用于存储KNOW文件反演结果的数据结构 ---
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
            if (splitContainer2.Orientation == Orientation.Vertical)
            {
                // 如果是左右排列，取宽度的 50%
                splitContainer2.SplitterDistance = splitContainer2.Width / 2;
            }
            else
            {
                // 如果是上下排列，取高度的 50%
                splitContainer2.SplitterDistance = splitContainer2.Height / 2;
            }
            LoadLayersFromMap();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region --- 新增：窗口拖动与边框拉伸核心逻辑 ---

        /// <summary>
        /// 初始化标题栏拖动事件（绑定panel1）
        /// </summary>
        private void InitDragEvent()
        {
            panel1.MouseDown += TitlePanel_MouseDown;
            panel1.MouseMove += TitlePanel_MouseMove;
        }

        /// <summary>
        /// 标题栏按下：记录鼠标相对位置
        /// </summary>
        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousePoint.X = e.X;
                mousePoint.Y = e.Y;
            }
        }

        /// <summary>
        /// 标题栏移动：计算窗口新位置
        /// </summary>
        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left = Control.MousePosition.X - mousePoint.X;
                this.Top = Control.MousePosition.Y - mousePoint.Y;
            }
        }
        #endregion
    } // End Class
} // End Namespace