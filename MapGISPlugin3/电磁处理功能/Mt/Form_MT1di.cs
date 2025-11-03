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

namespace MapGISPlugin3
{
    public partial class Form_MT1di : Form
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

        // 内部辅助类 (在文件底部定义)

        /// <summary>
        /// 构造函数
        /// </summary>
        public Form_MT1di(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;

            // 初始化列表
            m_allPointLayers = new List<MapLayer>();
            m_allObjectLayers = new List<MapLayer>();
            m_CurrentLineStations = new List<StationInfo>();
            m_CurrentLineData = new DataTable();
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

            cmbLineName.Items.Clear();      // 清空测线下拉框
            ClearAllDisplays();             // 清空所有图表、表格和小地图

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
            // --- [调试 1] ---
            MessageBox.Show($"[调试 1/7] 已选中测线: '{selectedLine}'。即将开始查询。", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Cursor = Cursors.WaitCursor; // 开始等待

            try
            {
                // 1. (TODO 1) 查询 SFeatureCls (点图层)，获取所有测点 (X, Y, 测点号)
                // --- [调试 2] ---
                MessageBox.Show("[调试 2/7] 即将调用 QueryStationsForLine (查询测点)...", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);

                m_CurrentLineStations = QueryStationsForLine(selectedLine);

                // --- [新调试 3] ---
                MessageBox.Show($"[调试 3/7] QueryStationsForLine 执行完毕。查询到 {m_CurrentLineStations.Count} 个测点。", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);


                // 2. (TODO 2) 查询 ObjectCls (测深表)，获取该测线所有数据
                // --- [新调试 4] ---
                MessageBox.Show("[调试 4/7] 即将调用 QuerySoundingDataForLine (查询测深数据)...", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                m_CurrentLineData = QuerySoundingDataForLine(selectedLine);
                // --- [新调试 5] ---
                MessageBox.Show($"[调试 5/7] QuerySoundingDataForLine 执行完毕。查询到 {m_CurrentLineData.Rows.Count} 条测深数据。", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);


                // 3. (TODO 3) 刷新"计算"页的小地图
                UpdateProfileView();

                // 4. (TODO 4 & 5) 刷新"TE" 和 "TM" 页的表格
                UpdateDataGrids();
                // --- [新调试 6] ---
                MessageBox.Show("[调试 6/7] 小地图 (ProfileView) 和数据表格 (DataGrids) 已刷新。", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);


                // 6. (TODO 6) 自动选中第一个点
                if (m_CurrentLineStations.Count > 0)
                {
                    string firstStation = m_CurrentLineStations[0].StationName;
                    // --- [新调试 7] ---
                    MessageBox.Show($"[调试 7/7] 即将自动选中第一个测点: '{firstStation}' 并刷新右侧图表。", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // 自动选中第一个点并刷新右侧图表
                    SelectStationAndRefreshCharts(firstStation);
                }
                else
                {
                    MessageBox.Show("[调试 7/7] 未查询到测点，跳过自动选中。", "cmbLineName_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        /// 事件: 点击 "开始计算" 按钮
        /// 【V-Final-2: 学习 MagCorrelationImaging 的相对路径逻辑】
        /// </summary>
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // --- 1. 检查数据 ---
            if (m_CurrentLineData == null || m_CurrentLineData.Rows.Count == 0)
            {
                MessageBox.Show("没有加载任何测线数据，无法计算。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            // --- 2. 获取参数 (根据文档) ---
            int its = (int)nudIterationCount.Value;
            int iwd = rbInversionTE.Checked ? 0 : 1; // 0=TE, 1=TM

            // --- 3. 定义路径 ---
            string tempInputFile = Path.GetTempFileName(); // 临时数据文件 (argv[1])
            string exePath;
            string pluginDir;
            string workspaceName;
            string fullWorkspacePath;

            try
            {
                // 【!! 学习 MagCorrelationImaging 的逻辑 !!】
                // 1. 获取插件的运行目录 (例如 ...\Program\)
                pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // 2. 组合出 a.exe 所在的子目录
                string algorithmDir = Path.Combine(pluginDir, "Algorithm", "MT1di");

                // 3. 获取 a.exe 的完整绝对路径
                exePath = Path.Combine(algorithmDir, "a.exe");
                // --- 【!! 学习结束 !!】 ---

                if (!File.Exists(exePath))
                {
                    // 这个错误信息现在是 100% 正确的
                    throw new FileNotFoundException($"计算程序 'a.exe' 未找到。\n请确保它位于: {exePath}\n\n(提示: 请执行本回复中的【第 2 步】来修复此问题。)");
                }

                // 根据文档: "工作空间为字符串类型，规定为字符串长度是6"
                workspaceName = Path.GetRandomFileName().Substring(0, 6); // (argv[2])
                // 组合出 "KNOW" 文件的完整保存路径 (也保存在插件目录 ...\Program\ 下)
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
                // --- 4. 数据转换 (写入临时文件) ---
                Dictionary<string, StationInfo> stationCoords = m_CurrentLineStations.ToDictionary(s => s.StationName);

                using (StreamWriter writer = new StreamWriter(tempInputFile))
                {
                    foreach (DataRow row in m_CurrentLineData.Rows)
                    {
                        string stationName = row["测点编号"].ToString();
                        StationInfo station;
                        if (!stationCoords.TryGetValue(stationName, out station)) { continue; }

                        double z_coord = 0.0; // Z坐标占位符
                        double period = GetDoubleFromRow(row, "周期", 1.0);
                        if (period == 0) period = 1e-9;
                        double freq = 1.0 / period; // C++ 需要的是频率

                        double rxy = GetDoubleFromRow(row, "视电阻率_TE", 0.0);
                        double pxy = GetDoubleFromRow(row, "相位_TE", 0.0);
                        double ryx = GetDoubleFromRow(row, "视电阻率_TM", 0.0);
                        double pyx = GetDoubleFromRow(row, "相位_TM", 0.0);

                        // 写入9列、空格分隔的行
                        writer.WriteLine($"{stationName} {station.X} {station.Y} {z_coord} {freq} {rxy} {pxy} {ryx} {pyx}");
                    }
                }

                // --- 5. 执行 a.exe ---
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = exePath;
                startInfo.Arguments = $"\"{tempInputFile}\" \"{workspaceName}\" {iwd} {its}";
                // 设置工作目录，a.exe 才能找到它的DLL(mt1di.dll等)并正确创建 workspaceName 文件夹
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

                    // --- 6. 反馈结果 ---
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
                // --- 7. 清理 ---
                if (File.Exists(tempInputFile))
                {
                    try { File.Delete(tempInputFile); }
                    catch (Exception ex) { Console.WriteLine($"删除临时文件 {tempInputFile} 失败: {ex.Message}"); }
                }
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region --- 3. 分析区事件 (右栏) ---


        #endregion

        #region --- 4. 核心辅助函数 (被事件调用) ---
        /// <summary>
        /// (辅助函数) 安全地从 DataRow 获取 double 值，处理 DBNull
        /// </summary>
        private double GetDoubleFromRow(DataRow row, string columnName, double defaultValue)
        {
            try
            {
                // 检查列是否存在且值不是 DBNull
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    return Convert.ToDouble(row[columnName]);
                }
            }
            catch (Exception ex)
            {
                // 仅在控制台输出错误，防止弹窗
                Console.WriteLine($"GetDoubleFromRow 转换失败: {columnName}, 错误: {ex.Message}");
            }
            // 转换失败或值为 DBNull 时返回默认值
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

                // 【修复逻辑 1】全查
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

                // 【修复逻辑 2】预加载
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
                int totalRecords = rs.Count; // 【V2.5 修复】获取总数

                // 【V2.5 修复】增加空记录集检查
                if (totalRecords == 0)
                {
                    Console.WriteLine("FillLineComboBox: totalRecords 为 0，循环前退出。");
                    return; // 会自动进入 finally 块，是安全的
                }

                // 【V2.5 修复】改为 do-while 循环
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
                                    validRecordCount++;
                                }
                            }
                        }

                        // 【修复逻辑 4】安全退出
                        if (currentIndex > rs.Count * 1.5 && rs.Count > 0) // rs.Count > 0 避免 0*1.5
                        {
                            MessageBox.Show($"迭代超过预期 ({currentIndex} > {rs.Count})，强制停止循环（可能 MoveNext Bug）");
                            break;
                        }
                    }
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message} (可能该记录损坏，跳过)");
                    }
                    finally
                    {
                        // 在每次循环结束时释放 Att 对象
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

            // 清空 Series
            if (chartProfileView.Series != null) chartProfileView.Series.Clear();
            if (chartResistivity.Series != null) chartResistivity.Series.Clear();
            if (chartPhase.Series != null) chartPhase.Series.Clear();

            // 重置轴为线性（关键：防止对数残留）
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

            // 清空数据源
            gridTE.DataSource = null;
            gridTM.DataSource = null;
            m_CurrentLineStations?.Clear();
            m_CurrentLineData?.Clear();
            m_CurrentSelectedStationName = null;

            // 重置标题
            if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = "周期-视电阻率";
            if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = "周期-相位";
        }




        /// <summary>
        /// (辅助函数) TODO 3: 刷新"计算"页的小地图 (chartProfileView)
        /// </summary>
        private void UpdateProfileView()
        {
            if (chartProfileView == null) return;

            chartProfileView.Series.Clear();

            Series s = chartProfileView.Series.Add("Stations");
            s.ChartType = SeriesChartType.Point; // 散点图
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 8;
            s.MarkerColor = System.Drawing.Color.Red;
            s.IsValueShownAsLabel = true; // 在点上显示标签

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
            }

            chartProfileView.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            chartProfileView.ChartAreas[0].AxisY.LabelStyle.Format = "F0";
            chartProfileView.ChartAreas[0].RecalculateAxesScale();
        }

        /// <summary>
        /// (辅助函数) TODO 6: 选中一个测点并刷新右栏图表
        /// </summary>
        private void SelectStationAndRefreshCharts(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName)) return;

            m_CurrentSelectedStationName = stationName;
            Console.WriteLine($"已选中测点: {stationName}");

            // (可选) 高亮 chartProfileView 上的这个点
            try
            {
                if (chartProfileView.Series.Count > 0)
                {
                    foreach (var point in chartProfileView.Series["Stations"].Points)
                    {
                        if (point.Tag?.ToString() == stationName)
                        {
                            point.MarkerColor = System.Drawing.Color.Blue;
                            point.MarkerSize = 12;
                        }
                        else
                        {
                            point.MarkerColor = System.Drawing.Color.Red;
                            point.MarkerSize = 8;
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"高亮小地图测点时出错: {ex.Message}"); }


            // 触发右栏刷新
            UpdateRightPanelCharts();
        }


        /// <summary>
        /// 事件: 当用户切换 右侧 的 "TE" / "TM" 图表标签页时 (tabControl2)
        /// </summary>
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 确保数据已加载 (m_CurrentSelectedStationName 不为空)
            // 这样在加载测线前或清空时切换标签不会出错
            if (!string.IsNullOrEmpty(m_CurrentSelectedStationName))
            {
                // 调用我们刚修改好的函数，它会读取 tabControl2 的状态
                UpdateRightPanelCharts();
            }
        }

        /// <summary>
        /// (辅助函数) 刷新右栏的两张曲线图
        /// 【V4 - 已修正为根据 tabControl2 状态刷新】
        /// </summary>
        private void UpdateRightPanelCharts()
        {
            chartResistivity.Series.Clear();
            chartPhase.Series.Clear();

            if (string.IsNullOrEmpty(m_CurrentSelectedStationName) || m_CurrentLineData == null)
            {
                if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = "周期-视电阻率";
                if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = "周期-相位";
                return;
            }

            string resField = "视电阻率_TM";
            string phaseField = "相位_TM";
            string seriesName = "TM 模式";

            if (tabControl2.SelectedTab == tabPageDisplayTE)
            {
                resField = "视电阻率_TE";
                phaseField = "相位_TE";
                seriesName = "TE 模式";
            }

            Console.WriteLine($"UpdateRightPanelCharts: resField={resField}, phaseField={phaseField}");

            if (string.IsNullOrEmpty(resField) || string.IsNullOrEmpty(phaseField))
            {
                MessageBox.Show("内部错误：resField 或 phaseField 未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 更新标题（如示例）
            if (chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = $"{m_CurrentSelectedStationName} - 视电阻率({seriesName})";
            if (chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = $"{m_CurrentSelectedStationName} - 相位({seriesName})";

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

            // 创建 Series（用 Spline 连接线，像示例平滑）
            var resSeries = chartResistivity.Series.Add(seriesName);
            var phaseSeries = chartPhase.Series.Add(seriesName);
            resSeries.ChartType = SeriesChartType.Spline; // 平滑线 + 点
            resSeries.MarkerStyle = MarkerStyle.Circle;
            resSeries.MarkerSize = 5; // 小点，像示例
            phaseSeries.ChartType = SeriesChartType.Spline;
            phaseSeries.MarkerStyle = MarkerStyle.Circle;
            phaseSeries.MarkerSize = 5;

            // 收集数据（用于轴检查）
            List<double> periods = new List<double>();
            List<double> resistivities = new List<double>();
            List<double> phases = new List<double>();

            foreach (DataRowView row in dvStation)
            {
                if (row["周期"] == DBNull.Value || row[resField] == DBNull.Value || row[phaseField] == DBNull.Value)
                    continue;

                double period = Convert.ToDouble(row["周期"]);
                double res = Convert.ToDouble(row[resField]);
                double phase = Convert.ToDouble(row[phaseField]);

                // 过滤无效（防止对数崩溃）
                if (period <= 0 || res <= 0) continue; // res >0 for log Y

                periods.Add(period);
                resistivities.Add(res);
                phases.Add(phase);

                resSeries.Points.AddXY(period, res);
                phaseSeries.Points.AddXY(period, phase);
            }

            if (periods.Count == 0)
            {
                Console.WriteLine("无有效数据点，跳过绘制。");
                return;
            }

            // 安全设置对数轴（恢复示例的对数）
            bool canLogX = periods.All(p => p > 0);
            bool canLogYRes = resistivities.All(r => r > 0);

            chartResistivity.ChartAreas[0].AxisX.IsLogarithmic = canLogX;
            chartResistivity.ChartAreas[0].AxisY.IsLogarithmic = canLogYRes;
            chartPhase.ChartAreas[0].AxisX.IsLogarithmic = canLogX;
            chartPhase.ChartAreas[0].AxisY.IsLogarithmic = false; // 相位始终线性

            if (!canLogX) Console.WriteLine("警告: 周期有 ≤0 值，使用线性轴。");
            if (!canLogYRes) Console.WriteLine("警告: 视电阻率有 ≤0 值，使用线性轴。");

            // 设置范围（像示例扩展一点）
            if (canLogX)
            {
                double minX = periods.Min();
                double maxX = periods.Max();
                chartResistivity.ChartAreas[0].AxisX.Minimum = Math.Pow(10, Math.Floor(Math.Log10(minX)));
                chartResistivity.ChartAreas[0].AxisX.Maximum = Math.Pow(10, Math.Ceiling(Math.Log10(maxX)));
                chartPhase.ChartAreas[0].AxisX.Minimum = chartResistivity.ChartAreas[0].AxisX.Minimum;
                chartPhase.ChartAreas[0].AxisX.Maximum = chartResistivity.ChartAreas[0].AxisX.Maximum;
            }

            chartResistivity.ChartAreas[0].RecalculateAxesScale();
            chartPhase.ChartAreas[0].RecalculateAxesScale();

            // 美化轴（接近示例：简洁标签、无旋转、浅网格）
            BeautifyChartAxes(chartResistivity.ChartAreas[0]);
            BeautifyChartAxes(chartPhase.ChartAreas[0]);
        }



        private void BeautifyChartAxes(ChartArea area)
        {
            if (area == null) return;

            // 标签格式：简洁，如0.001（示例中无多小数）
            area.AxisX.LabelStyle.Format = "0.###";
            area.AxisY.LabelStyle.Format = "0.##";

            // 字体：标准Arial，稍大
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 9f);
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 9f);

            // 无旋转（避免丑），用自动调整防止重叠
            area.AxisX.LabelStyle.Angle = 0;
            area.AxisX.LabelStyle.IsStaggered = false; // 示例中不交错

            // 间隔：对数每十年一标签，像示例0.001到1
            if (area.AxisX.IsLogarithmic)
            {
                area.AxisX.LogarithmBase = 10;
                area.AxisX.Interval = 1; // 每个10的幂
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

            // 网格：浅灰细线，像示例
            area.AxisX.MajorGrid.LineWidth = 1;
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            area.AxisY.MajorGrid.LineWidth = 1;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            // 轴线：黑粗
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
                // --- 1. 打印字段名，确认存在 ---
                Fields fields = m_SelectedStationLayer.Fields;
                MessageBox.Show($"字段总数: {fields.Count}\n字段列表:\n" +
                    string.Join("\n", Enumerable.Range(0, fields.Count).Select(i =>
                        $"{i}: {fields[i].FieldName} (类型: {fields[i].FieldType})"
                    )));

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

                MessageBox.Show($"查询成功！共 {rs.Count} 条记录，开始遍历...");

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

                // --- 显示调试报告 V2.2 ---
                if (!fieldNameError)
                {
                    MessageBox.Show($"遍历报告 (QueryStationsForLine V2.2):\n\n" +
                                    $"总记录 (rs.Count): {totalRecords}\n" +
                                    $"-------------------------------------\n" +
                                    $"成功添加测点: {successCount} (List.Count: {stations.Count})\n" +
                                    $"跳过 (几何错误): {geomFailCount}\n" +
                                    $"跳过 (属性为空): {attNullCount}\n" +
                                    $"跳过 (测点号为空): {nameNullCount}\n\n" +
                                    $"第一个错误几何类型: {firstGeomType}",
                                    "调试报告", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }






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

            MessageBox.Show($"最终返回 {stations.Count} 个测点");
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

        // (内部辅助类)
        private class StationInfo
        {
            public string StationName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }


        private void Form_MT1di_Load(object sender, EventArgs e)
        {
            LoadLayersFromMap();
        }

        

        
    } // End Class
} // End Namespace