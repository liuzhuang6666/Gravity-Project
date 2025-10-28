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
        /// (辅助函数) 从 "电法数据" 地图加载图层列表 (已根据参考代码修正)
        /// </summary>
        private void LoadLayersFromMap()
        {
            m_allPointLayers.Clear();
            m_allObjectLayers.Clear();
            cmbStationLayer.Items.Clear(); // 清空下拉框

            Map electroMap = null; // 移到外部以便 finally 访问 (虽然我们不再手动释放它)
            try
            {
                electroMap = FindMapByName("电法数据"); // 使用辅助函数查找地图
                if (electroMap == null)
                {
                    MessageBox.Show("未在项目中找到名为 '电法数据' 的地图！请确保该地图存在。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // 禁用控件
                    cmbStationLayer.Enabled = false;
                    cmbLineName.Enabled = false;
                    return;
                }

                Console.WriteLine($"成功找到 '电法数据' 地图 '{electroMap.Name}'，开始遍历 {electroMap.LayerCount} 个图层..."); // 调试信息

                // --- 核心遍历逻辑 (仿照 AddLayersToNode) ---
                MapLayer layer = null; // 用于接收图层对象
                for (int i = 0; i < electroMap.LayerCount; i++) // 使用 LayerCount 属性
                {
                    layer = null; // 重置 layer 变量
                    try
                    {
                        layer = electroMap.get_Layer(i); // 使用 get_Layer 方法

                        if (layer == null)
                        {
                            Console.WriteLine($"图层索引 {i} 为 null，跳过。");
                            continue;
                        }

                        ProcessLayerForComboBox(layer); // 调用处理单个图层的函数

                    }
                    catch (COMException comEx)
                    {
                        // 处理获取单个图层时可能发生的 COM 异常
                        Console.WriteLine($"遍历图层索引 {i} (名称: {layer?.Name ?? "未知"}) 时发生 COM 错误: {comEx.Message} (Code: {comEx.ErrorCode})");
                    }
                    catch (Exception ex)
                    {
                        // 处理其他可能的异常
                        Console.WriteLine($"遍历图层索引 {i} (名称: {layer?.Name ?? "未知"}) 时发生错误: {ex.Message}");
                    }
                    finally
                    {
                        // *** 关键：不再释放 layer 对象 ***
                        // 它可能已经被添加到 ComboBox 或列表中，或者是一个 GroupLayer 需要进一步处理
                        // Map 对象会管理其子图层的生命周期
                    }
                }
                // --- 遍历结束 ---

            }
            catch (COMException comEx) // 捕获查找地图或获取 LayerCount 时可能发生的错误
            {
                MessageBox.Show($"查找 '电法数据' 地图或获取图层数量时出错: {comEx.Message}", "COM 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"LoadLayersFromMap - 查找地图或 LayerCount COM 异常: {comEx}");
                cmbStationLayer.Enabled = false;
                cmbLineName.Enabled = false;
                return; // 出错则终止加载
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图层列表时发生意外错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"LoadLayersFromMap - 加载图层列表异常: {ex}");
                cmbStationLayer.Enabled = false;
                cmbLineName.Enabled = false;
                return; // 出错则终止加载
            }
            finally
            {
                // *** 不再需要释放 electroMap ***
                // Map 对象由 Document 管理
                Console.WriteLine("[LoadLayersFromMap] finally 块执行完毕。");
            }


            // 设置 ComboBox 如何显示 MapLayer 对象 (显示其 Name 属性)
            cmbStationLayer.DisplayMember = "Name";

            Console.WriteLine($"加载完成: 找到 {cmbStationLayer.Items.Count} 个测点图层, {m_allObjectLayers.Count} 个测深数据表。"); // 调试信息

            // 检查是否找到了测点图层
            if (cmbStationLayer.Items.Count > 0)
            {
                cmbStationLayer.Text = "请选择测点图层..."; // 给个提示
                cmbStationLayer.Enabled = true; // 确保启用
                cmbLineName.Enabled = true; // 先启用，后续根据选择再决定是否禁用
            }
            else
            {
                MessageBox.Show("在 '电法数据' 地图中未找到符合命名规范的测点图层 (名称需包含 '测点')。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStationLayer.Enabled = false; // 没有可选图层，禁用下拉框
                cmbLineName.Enabled = false;
            }
        }

        /// <summary>
        /// (新增辅助函数) 处理单个图层，如果是组图层则递归处理 (仿照 ProcessLayer)
        /// </summary>
        private void ProcessLayerForComboBox(MapLayer layer)
        {
            if (layer == null) return;

            try
            {
                Console.WriteLine($"处理图层: {layer.Name}, 类型: {layer.GetType().Name}"); // 调试信息

                // 检查是否是组图层
                if (layer is GroupLayer groupLayer)
                {
                    Console.WriteLine($"发现组图层: {groupLayer.Name}，包含 {groupLayer.Count} 个子图层。开始递归...");
                    // 递归处理组图层中的每一个子图层
                    MapLayer subLayer = null;
                    for (int i = 0; i < groupLayer.Count; i++)
                    {
                        subLayer = null; // 重置
                        try
                        {
                            subLayer = groupLayer.get_Item(i); // 获取子图层
                            ProcessLayerForComboBox(subLayer); // 递归调用
                        }
                        catch (COMException comEx)
                        {
                            Console.WriteLine($"处理组 '{groupLayer.Name}' 的子图层 {i} 时发生 COM 错误: {comEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"处理组 '{groupLayer.Name}' 的子图层 {i} 时发生错误: {ex.Message}");
                        }
                        finally
                        {
                            // *** 不释放 subLayer ***
                        }
                    }
                }
                // 检查是否是符合条件的点图层
                else if (layer is VectorLayer vectorLayer && vectorLayer.GeometryType == GeomType.Pnt)
                {
                    // 并且名称包含 "测点"
                    if (layer.Name != null && layer.Name.Contains("测点"))
                    {
                        Console.WriteLine($"找到测点图层: {layer.Name}，添加到下拉框和列表。");
                        m_allPointLayers.Add(layer);
                        cmbStationLayer.Items.Add(layer); // 添加到 ComboBox
                    }
                    else
                    {
                        Console.WriteLine($"图层 '{layer.Name}' 是点图层但名称不含 '测点'，跳过。");
                    }
                }
                // 检查是否是符合条件的对象类图层
                else if (layer is ObjectLayer objectLayer)
                {
                    // 并且名称包含 "测深数据"
                    if (layer.Name != null && layer.Name.Contains("测深数据"))
                    {
                        Console.WriteLine($"找到测深数据表: {layer.Name}，添加到内部列表。");
                        m_allObjectLayers.Add(layer);
                    }
                    else
                    {
                        Console.WriteLine($"图层 '{layer.Name}' 是对象类但名称不含 '测深数据'，跳过。");
                    }
                }
                else
                {
                    Console.WriteLine($"图层 '{layer.Name}' 类型 ({layer.GetType().Name}) 不符合要求，跳过。");
                }
            }
            catch (COMException comEx) // 捕获访问 layer 属性（如 Name, GeometryType）时可能发生的错误
            {
                Console.WriteLine($"处理图层时发生 COM 错误: {comEx.Message} (Code: {comEx.ErrorCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理图层时发生错误: {ex.Message}");
            }
            // *** 不释放 layer ***
        }

        #region --- 1. 主控件事件 (左栏顶部) ---

        /// <summary>
        /// 事件: 当用户选择一个新的 "测点图层"
        /// </summary>
        private void cmbStationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: 实现自动关联逻辑
            // 1. 获取选中的 Layer
            // 2. 自动 .Replace("测点", "测深数据") 查找 m_allObjectLayers
            // 3. 锁定 m_SelectedStationLayer 和 m_SelectedSoundingTable
            // 4. (如果成功) 填充 cmbLineName
            // 5. (如果成功) 自动选中 cmbLineName.SelectedIndex = 0 (这将触发下一个事件)
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

        /// <summary>
        /// 事件: 当TE/TM模式切换时 (两个RadioButton都指向这个事件)
        /// </summary>
        private void rbMode_CheckedChanged(object sender, EventArgs e)
        {
            // 仅刷新右栏图表
            if (((RadioButton)sender).Checked)
            {
                UpdateRightPanelCharts();
            }
        }

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

    } // End Class
} // End Namespace