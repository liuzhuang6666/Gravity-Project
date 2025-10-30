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
        /// (辅助函数) 从 "电法数据" 地图加载图层列表 (带弹窗调试)
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

                MessageBox.Show($"成功找到 '电法数据' 地图 '{electroMap.Name}'，开始遍历 {layerCount} 个图层...", "LoadLayersFromMap - 调试信息 1", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

                        // 调用带有弹窗的图层处理函数
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
            // ... (catch 和 finally 块保持不变) ...
            catch (COMException comEx) { MessageBox.Show($"查找地图或获取图层数COM错误: {comEx.Message}", "LoadLayersFromMap - 致命COM错误"); /* ... 禁用控件 ... */ return; }
            catch (Exception ex) { MessageBox.Show($"加载图层列表意外错误: {ex.Message}", "LoadLayersFromMap - 致命错误"); /* ... 禁用控件 ... */ return; }
            finally { Console.WriteLine("[LoadLayersFromMap] finally 块执行完毕。"); }


            cmbStationLayer.DisplayMember = "Name";

            MessageBox.Show($"图层遍历完成。\n找到 {cmbStationLayer.Items.Count} 个测点图层。\n找到 {m_allObjectLayers.Count} 个测深数据表。", "LoadLayersFromMap - 调试信息 2", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
        /// (带弹窗调试的新增辅助函数) 处理单个图层，如果是组图层则递归处理
        /// </summary>
        private void ProcessLayerForComboBox_Debug(MapLayer layer)
        {
            if (layer == null) return;

            string layerName = "[无法获取名称]"; // 默认值
            string layerTypeName = "[无法获取类型]";
            try { layerName = layer.Name ?? "[名称为null]"; layerTypeName = layer.GetType().Name; } catch { /* 忽略获取名称/类型的错误 */ }

            MessageBox.Show($"开始处理图层: '{layerName}', 类型: {layerTypeName}", "ProcessLayer - 调试信息 3", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                // 检查是否是组图层
                if (layer is GroupLayer groupLayer)
                {
                    int subLayerCount = 0;
                    try { subLayerCount = groupLayer.Count; } catch { } // 安全获取数量
                    MessageBox.Show($"发现组图层: '{layerName}'，包含 {subLayerCount} 个子图层。准备递归...", "ProcessLayer - 调试信息 4", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    MapLayer subLayer = null;
                    for (int i = 0; i < subLayerCount; i++) // 使用安全获取的数量
                    {
                        subLayer = null;
                        try
                        {
                            subLayer = groupLayer.get_Item(i);
                            // 递归调用带弹窗的版本
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

                    MessageBox.Show($"图层 '{layerName}' 是 VectorLayer。几何类型: {geomType}。", "ProcessLayer - 调试信息 5", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (isPoint) // 几何类型是点
                    {
                        bool nameMatch = false;
                        if (layerName != null && layerName.Contains("测点"))
                        {
                            nameMatch = true;
                        }

                        if (nameMatch) // 名称也匹配
                        {
                            MessageBox.Show($"**成功!** 图层 '{layerName}' 符合所有条件 (点图层, 名称含'测点')。准备添加到下拉框。", "ProcessLayer - 调试信息 6", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            m_allPointLayers.Add(layer);
                            cmbStationLayer.Items.Add(layer);
                        }
                        else // 名称不匹配
                        {
                            MessageBox.Show($"图层 '{layerName}' 是点图层，但名称 **不包含 '测点'**，跳过添加。", "ProcessLayer - 调试信息 6a", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else // 几何类型不是点
                    {
                        MessageBox.Show($"图层 '{layerName}' 是 VectorLayer，但几何类型 **不是 Point** ({geomType})，跳过添加。", "ProcessLayer - 调试信息 6b", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                // 检查是否是 ObjectLayer
                else if (layer is ObjectLayer objectLayer)
                {
                    MessageBox.Show($"图层 '{layerName}' 是 ObjectLayer。", "ProcessLayer - 调试信息 7", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // 并且名称包含 "测深数据"
                    if (layerName != null && layerName.Contains("测深数据"))
                    {
                        MessageBox.Show($"找到测深数据表: '{layerName}'，添加到内部列表。", "ProcessLayer - 调试信息 8", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        m_allObjectLayers.Add(layer);
                    }
                    else
                    {
                        MessageBox.Show($"图层 '{layerName}' 是 ObjectLayer，但名称 **不包含 '测深数据'**，跳过。", "ProcessLayer - 调试信息 8a", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                // 其他类型的图层
                else
                {
                    MessageBox.Show($"图层 '{layerName}' 类型 ({layerTypeName}) **不符合要求**，跳过。", "ProcessLayer - 调试信息 9", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            // 使用您原始的变量名
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
                // 将获取的 SFeatureCls 赋值给 m_SelectedStationLayer
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
                // 将获取的 ObjectCls 赋值给 m_SelectedSoundingTable
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
            FillLineComboBox(); // 调用辅助函数填充测线列表 (此函数内部不依赖 m_SelectedStationLayerCls)

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
        /// 事件: 当用户选择一个新的 "测线"
        /// </summary>
        private void cmbLineName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: 实现 "主刷新"
            // 1. 查询 SFeatureCls (测点)，获取所有测点 (X, Y, 测点号) -> 存入 m_CurrentLineStations
            // 2. 查询 ObjectCls (测深)，获取该测线所有数据 -> 存入 m_CurrentLineData
            // 3. 调用 UpdateProfileView() (刷新"计算"页的小地图)
            // 4. 筛选 m_CurrentLineData，填充 gridTE (刷新"TE"页的表格)
            // 5. 筛选 m_CurrentLineData，填充 gridTM (刷新"TM"页的表格)
            // 6. (最后) 自动选中第一个点: 
            //    if (m_CurrentLineStations.Count > 0)
            //       SelectStationAndRefreshCharts(m_CurrentLineStations[0].StationName);
        }

        #endregion

        #region --- 2. 标签页控件事件 (左栏下方) ---

        /// <summary>
        /// 事件: 当用户在 "小地图" 上点击
        /// </summary>
        private void chartProfileView_MouseClick(object sender, MouseEventArgs e)
        {
            // TODO: 实现可视化点选
            // 1. 使用 chartProfileView.HitTest(e.X, e.Y)
            // 2. 找到被点击的数据点索引 pointIndex
            // 3. 从 m_CurrentLineStations[pointIndex] 获取 stationName
            // 4. 调用 SelectStationAndRefreshCharts(stationName)
        }

        /// <summary>
        /// 事件: 点击 "开始计算" 按钮
        /// </summary>
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // TODO: 实现第三步 - 计算
            // 1. 从 m_CurrentLineData (或重新查询) 中获取当前测线的全部数据
            // 2. 获取 groupBoxParams 里的所有参数
            // 3. 写入临时文件 input.txt
            // 4. 调用 System.Diagnostics.Process.Start("a.exe")
            // 5. 等待 a.exe 结束
            // 6. 加载 output.grd 结果到 "电法数据" 地图
        }

        #endregion

        #region --- 3. 分析区事件 (右栏) ---

        


        #endregion

        #region --- 4. 核心辅助函数 (被事件调用) ---

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
        /// (辅助函数) 填充测线下拉框 (cmbLineName) (【!! 最终诊断版 - 请告诉我你看到的最后一个数字 !!】)
        /// </summary>
        private void FillLineComboBox()
        {
            try
            {
                MessageBox.Show("1. FillLineComboBox 已开始。");
            }
            catch (Exception ex) { MessageBox.Show($"调试点1出错: {ex.Message}"); }

            cmbLineName.Items.Clear();
            if (m_SelectedStationLayer == null || !m_SelectedStationLayer.HasOpen())
            {
                MessageBox.Show("FillLineComboBox 错误: m_SelectedStationLayer 为 null 或未打开。");
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            cmbLineName.Enabled = false;

            RecordSet rs = null;
            HashSet<string> uniqueLines = new HashSet<string>(); // 使用 HashSet 进行唯一值提取（高效去重）
            int validRecordCount = 0; // 统计有效“测线号”记录数

            try
            {
                string queryField = "测线号";

                // *** 检查字段是否存在（使用 Fields 属性和 IndexOf） ***
                Fields fields = m_SelectedStationLayer.Fields; // 源代码有 Fields 属性
                if (fields == null || fields.IndexOf(queryField) < 0)
                {
                    MessageBox.Show($"字段 '{queryField}' 不存在或无法获取！");
                    return;
                }
                MessageBox.Show($"调试: 字段 '{queryField}' 存在（IndexOf = {fields.IndexOf(queryField)}）。");

                // *** 调试总要素数 ***
                int totalCount = m_SelectedStationLayer.Count;
                MessageBox.Show($"调试: 要素类总记录数 = {totalCount} (应为 429；如果不匹配，数据问题)");

                try
                {
                    MessageBox.Show("2. 即将调用 m_SelectedStationLayer.Select(null) 全查所有记录...");
                }
                catch (Exception ex) { MessageBox.Show($"调试点2出错: {ex.Message}"); }

                // 关键调用：全查
                rs = m_SelectedStationLayer.Select(null);

                try
                {
                    MessageBox.Show("3. Select() 已执行完毕。即将检查 rs 是否为 null...");
                }
                catch (Exception ex) { MessageBox.Show($"调试点3出错: {ex.Message}"); }

                if (rs == null)
                {
                    MessageBox.Show("Select(null) 返回 null。尝试用 QueryDef(Filter='')...");
                    QueryDef query = new QueryDef();
                    query.Filter = "";  // 空字符串，全查
                    query.WithSpatial = false;  // 不查空间，只属性
                    rs = m_SelectedStationLayer.Select(query);
                    if (rs == null)
                    {
                        MessageBox.Show("Select(QueryDef) 也返回 null。检查数据源 URL 或 HasOpen()！");
                        return;
                    }
                }

                // 调试：检查 rs.Count
                int debugCount = rs.Count;
                MessageBox.Show($"调试: rs.Count = {debugCount} (全查，应为 429；如果=429，继续预加载记录集...)");

                // *** 预加载记录集，避免延迟加载挂起 ***
                try
                {
                    rs.MoveLast();  // 移到最后，强制加载所有记录
                    rs.MoveFirst(); // 移回开头
                    MessageBox.Show("调试: 记录集预加载成功（MoveLast/MoveFirst）。即将开始迭代...");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"预加载记录集出错: {ex.Message} (可能数据损坏或 COM 问题)");
                    return;
                }

                // 迭代收集唯一值（加 IsEOF 检查、安全退出、进度）
                int currentIndex = 0;
                while (rs.MoveNext() && !rs.IsEOF)
                {
                    try
                    {
                        currentIndex++;
                        Record currentAtt = rs.Att;
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
                        // 进度调试
                        if (currentIndex % 100 == 0)
                        {
                            MessageBox.Show($"调试: 已迭代 {currentIndex} 条记录（有效: {validRecordCount}）。继续...");
                        }
                        // *** 安全退出：如果超过预期1.5倍，强制停止避免无限 ***
                        if (currentIndex > rs.Count * 1.5)
                        {
                            MessageBox.Show($"迭代超过预期 ({currentIndex} > {rs.Count * 1.5})，强制停止循环（可能 MoveNext Bug）");
                            break;
                        }
                    }
                    catch (Exception recEx)
                    {
                        MessageBox.Show($"迭代第 {currentIndex} 条记录出错: {recEx.Message} (可能该记录损坏，跳过)");
                    }
                }

                try
                {
                    MessageBox.Show($"4. while 循环已结束。有效'测线号'记录数: {validRecordCount}。即将填充下拉框...");
                }
                catch (Exception ex) { MessageBox.Show($"调试点4出错: {ex.Message}"); }

                if (uniqueLines.Count > 0)
                {
                    var sortedLines = uniqueLines.OrderBy(s => s).ToArray();
                    cmbLineName.Items.AddRange(sortedLines);
                    MessageBox.Show($"成功填充测线下拉框，共 {sortedLines.Length} 条唯一测线。（有效记录: {validRecordCount}）");
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
                    MessageBox.Show("RecordSet released in FillLineComboBox finally.");
                }

                this.Cursor = Cursors.Default;
                cmbLineName.Enabled = (uniqueLines.Count > 0);

                try
                {
                    MessageBox.Show("5. Finally 块已执行完毕。");
                }
                catch (Exception ex) { MessageBox.Show($"调试点5出错: {ex.Message}"); }
            }
        }






        // <summary>
        /// (辅助函数) 清空所有显示区域 (图表、表格、小地图)
        /// </summary>
        private void ClearAllDisplays()
        {
            Console.WriteLine("执行 ClearAllDisplays..."); // 调试信息
            // 清空左栏
            // cmbLineName.Items.Clear(); // 这个在 FillLineComboBox 前清空
            if (chartProfileView.Series != null) chartProfileView.Series.Clear(); // 安全检查
            gridTE.DataSource = null;
            gridTM.DataSource = null;
            m_CurrentLineStations?.Clear(); // 使用 ?. 安全调用
            m_CurrentLineData?.Clear(); // 使用 ?. 安全调用
            m_CurrentSelectedStationName = null;

            // 清空右栏
            if (chartResistivity.Series != null) chartResistivity.Series.Clear(); // 安全检查
            if (chartPhase.Series != null) chartPhase.Series.Clear(); // 安全检查
            // 恢复默认标题 (如果 Chart 控件有 Titles 集合且至少有一个 Title)
            if (chartResistivity.Titles != null && chartResistivity.Titles.Count > 0) chartResistivity.Titles[0].Text = "周期-视电阻率";
            if (chartPhase.Titles != null && chartPhase.Titles.Count > 0) chartPhase.Titles[0].Text = "周期-相位";
        }

        /// <summary>
        /// (辅助函数) 刷新"计算"页的小地图 (chartProfileView)
        /// </summary>
        private void UpdateProfileView()
        {
            // TODO:
            // 1. 清空 chartProfileView.Series
            // 2. 遍历 m_CurrentLineStations
            // 3. chartProfileView.Series[0].Points.AddXY(station.X, station.Y)
            // ... (设置图表样式为散点图)
        }

        /// <summary>
        /// (辅助函数) 选中一个测点并刷新右栏图表
        /// </summary>
        private void SelectStationAndRefreshCharts(string stationName)
        {
            m_CurrentSelectedStationName = stationName;

            // TODO: (可选) 高亮 chartProfileView 上的这个点

            // 触发右栏刷新
            UpdateRightPanelCharts();
        }

        /// <summary>
        /// (辅助函数) 刷新右栏的两张曲线图
        /// </summary>
        private void UpdateRightPanelCharts()
        {
            if (string.IsNullOrEmpty(m_CurrentSelectedStationName) || m_SelectedSoundingTable == null)
                return;

            // TODO:
            // 1. 查询 m_SelectedSoundingTable, 条件: 测点号 == m_CurrentSelectedStationName
            // 2. 清空 chartResistivity.Series 和 chartPhase.Series
            // 3. 检查 rbTE.Checked 还是 rbTM.Checked
            // 4. 遍历查询结果，将 (周期, 视电阻率) 添加到 chartResistivity
            // 5. 遍历查询结果，将 (周期, 相位) 添加到 chartPhase
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