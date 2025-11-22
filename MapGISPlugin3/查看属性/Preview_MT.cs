using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MapGISPlugin3.查看属性
{
    public partial class Preview_MT : UserControl
    {
        private DataTable _dt; // 缓存总数据
        private int _lastHighlightedIndex = -1;

        public Preview_MT()
        {
            InitializeComponent();
            InitializeChart(); // 初始化图表配置
            InitializeGrid();  // 初始化表格配置
            InitializeTree();  // 初始化树形菜单
        }

        // ---------------------------------------------------------
        // 1. 初始化配置
        // ---------------------------------------------------------
        private void InitializeChart()
        {
            chartMT.Series.Clear();
            chartMT.ChartAreas.Clear();
            chartMT.Legends.Clear();

            ChartArea area = new ChartArea("MainArea");

            // --- 关键：初始状态全部设为线性，防止无数据时报错 ---
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;
            area.AxisY2.IsLogarithmic = false;

            // 美化网格
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY2.MajorGrid.Enabled = false;

            // 标题
            area.AxisX.Title = "X轴";
            area.AxisY.Title = "视电阻率 (Ω·m)";
            area.AxisY2.Title = "相位 (°)";

            chartMT.ChartAreas.Add(area);

            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Top;
            legend.Alignment = StringAlignment.Center;
            chartMT.Legends.Add(legend);
        }

        private void InitializeGrid()
        {
            gridMT.ReadOnly = true;
            gridMT.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridMT.AllowUserToAddRows = false;
            gridMT.RowHeadersVisible = false;
            gridMT.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridMT.SelectionChanged += GridMT_SelectionChanged;
        }

        private void InitializeTree()
        {
            tvData.AfterSelect += TvData_AfterSelect;
            tvData.HideSelection = false; // 失去焦点时仍显示选中项
        }

        // ---------------------------------------------------------
        // 📥 2. 数据加载 (构建 测线->测点 树)
        // ---------------------------------------------------------
        public void LoadData(DataTable dt)
        {
            _dt = dt;
            tvData.Nodes.Clear();
            ResetChartState(); // 清空图表并重置轴
            gridMT.DataSource = null;

            if (_dt == null || _dt.Rows.Count == 0) return;

            // 检查字段
            if (!_dt.Columns.Contains("测线编号") || !_dt.Columns.Contains("测点编号"))
            {
                MessageBox.Show("数据缺少 '测线编号' 或 '测点编号' 字段。");
                return;
            }

            try
            {
                tvData.BeginUpdate();

                // 1. 按测线分组
                var lineGroups = _dt.AsEnumerable()
                                    .GroupBy(r => r["测线编号"].ToString())
                                    .OrderBy(g => g.Key);

                foreach (var lineGroup in lineGroups)
                {
                    string lineName = lineGroup.Key;
                    TreeNode lineNode = new TreeNode($"测线: {lineName}");
                    lineNode.Tag = "LINE"; // 标记为父节点
                    lineNode.ImageIndex = 0;

                    // 2. 挂载测点
                    var stations = lineGroup.Select(r => r["测点编号"].ToString())
                                            .Distinct()
                                            .OrderBy(s => s);

                    foreach (var stationName in stations)
                    {
                        TreeNode stationNode = new TreeNode(stationName);
                        // 存入 [Line, Station] 以便唯一索引
                        stationNode.Tag = new string[] { lineName, stationName };
                        stationNode.ImageIndex = 1;
                        lineNode.Nodes.Add(stationNode);
                    }
                    tvData.Nodes.Add(lineNode);
                }

                tvData.EndUpdate();

                // 展开第一个节点
                if (tvData.Nodes.Count > 0)
                {
                    tvData.Nodes[0].Expand();
                    // 不自动选中子节点，避免误触
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("构建目录出错: " + ex.Message);
            }
        }

        // ---------------------------------------------------------
        // 🖱️ 3. 点击事件 (解决崩溃和连线问题的核心)
        // ---------------------------------------------------------
        private void TvData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null || node.Tag == null) return;

            // 判断点击的是 "测点" 还是 "测线"
            if (node.Tag is string[] tags)
            {
                // ---> 点击了具体的测点 (子节点)
                string lineName = tags[0];
                string stationName = tags[1];
                DrawStationData(lineName, stationName);
            }
            else
            {
                // ---> 点击了测线 (父节点) 或者折叠导致选中了父节点
                // 【关键修复 1】必须重置图表状态，包括关闭对数轴
                ResetChartState();
                gridMT.DataSource = null;
            }
        }

        private void DrawStationData(string lineName, string stationName)
        {
            try
            {
                // 1. 筛选数据
                DataView dv = new DataView(_dt);
                dv.RowFilter = $"测线编号 = '{lineName}' AND 测点编号 = '{stationName}'";

                // 【关键修复 2】强制排序，解决“首尾相连/乱连线”问题
                // Spline 图表是按点的添加顺序连线的，如果数据没排序，线条就会乱飞
                if (_dt.Columns.Contains("频率"))
                    dv.Sort = "频率 DESC"; // 频率通常从高到低
                else if (_dt.Columns.Contains("周期"))
                    dv.Sort = "周期 ASC";  // 周期通常从小到大

                // 2. 刷新表格
                gridMT.DataSource = dv;

                // 3. 刷新图表
                UpdateChart(dv);
            }
            catch (Exception ex)
            {
                Console.WriteLine("绘图出错: " + ex.Message);
            }
        }

        private void UpdateChart(DataView dv)
        {
            // A. 准备工作：先重置轴为线性，防止报错
            chartMT.Series.Clear();
            var area = chartMT.ChartAreas[0];
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;

            if (dv.Count == 0) return;

            _lastHighlightedIndex = -1;

            // B. 创建 Series
            Series sRes = chartMT.Series.Add("视电阻率");
            sRes.ChartType = SeriesChartType.Spline;
            sRes.MarkerStyle = MarkerStyle.Circle;
            sRes.MarkerSize = 6;
            sRes.Color = Color.Blue;
            sRes.YAxisType = AxisType.Primary;

            Series sPhs = chartMT.Series.Add("相位");
            sPhs.ChartType = SeriesChartType.Spline;
            sPhs.MarkerStyle = MarkerStyle.Diamond;
            sPhs.MarkerSize = 6;
            sPhs.Color = Color.Red;
            sPhs.YAxisType = AxisType.Secondary;

            // C. 填充数据
            List<double> xValues = new List<double>();
            List<double> yResValues = new List<double>();

            foreach (DataRowView row in dv)
            {
                // 智能识别 X 轴 (频率 或 周期)
                double XVal = -1;
                string xTitle = "X轴";

                if (row.DataView.Table.Columns.Contains("频率"))
                {
                    XVal = GetDouble(row, "频率");
                    xTitle = "频率 (Hz)";
                }
                else if (row.DataView.Table.Columns.Contains("周期"))
                {
                    XVal = GetDouble(row, "周期");
                    xTitle = "周期 (s)";
                }

                area.AxisX.Title = xTitle;

                // 智能识别 Y 轴
                double Rho = GetDouble(row, "视电阻率");
                if (Rho < 0) Rho = GetDouble(row, "视电阻率_TE"); // 兼容

                double Phs = GetDouble(row, "相位");
                if (Phs < 0) Phs = GetDouble(row, "相位_TE"); // 兼容

                // 【关键修复 3】严格过滤 <= 0 的数据
                if (XVal > 0 && Rho > 0)
                {
                    int idx = sRes.Points.AddXY(XVal, Rho);
                    sPhs.Points.AddXY(XVal, Phs);
                    sRes.Points[idx].Tag = XVal; // 存X值用于反向查找

                    xValues.Add(XVal);
                    yResValues.Add(Rho);
                }
            }

            // D. 安全开启对数轴
            if (xValues.Count > 0)
            {
                // 只有当所有数据都 > 0 时，才开启对数轴
                bool canLogX = xValues.All(v => v > 0);
                bool canLogY = yResValues.All(v => v > 0);

                area.AxisX.IsLogarithmic = canLogX;
                area.AxisY.IsLogarithmic = canLogY;

                // 如果是频率，习惯上反转坐标轴 (高频在左)
                if (area.AxisX.Title.Contains("频率"))
                    area.AxisX.IsReversed = true;
                else
                    area.AxisX.IsReversed = false;

                area.RecalculateAxesScale();
            }
        }

        // --- 辅助：重置图表状态 (防崩) ---
        private void ResetChartState()
        {
            chartMT.Series.Clear();
            if (chartMT.ChartAreas.Count > 0)
            {
                var area = chartMT.ChartAreas[0];
                area.AxisX.IsLogarithmic = false;
                area.AxisY.IsLogarithmic = false;
                // 清除标题或重置为默认
                area.AxisX.Title = "X轴";
            }
        }

        // ---------------------------------------------------------
        // 4. 表格点选高亮 (不变)
        // ---------------------------------------------------------
        private void GridMT_SelectionChanged(object sender, EventArgs e)
        {
            if (gridMT.CurrentRow == null || chartMT.Series.Count == 0) return;

            // 找 X 轴对应的列名
            string colName = "";
            if (gridMT.Columns.Contains("频率")) colName = "频率";
            else if (gridMT.Columns.Contains("周期")) colName = "周期";

            if (string.IsNullOrEmpty(colName)) return;

            object val = gridMT.CurrentRow.Cells[colName].Value;
            if (val == null || val == DBNull.Value) return;

            double selectedX;
            if (double.TryParse(val.ToString(), out selectedX))
            {
                HighlightChartPoint(selectedX);
            }
        }

        private void HighlightChartPoint(double targetX)
        {
            Series s = chartMT.Series[0];
            if (_lastHighlightedIndex >= 0 && _lastHighlightedIndex < s.Points.Count)
            {
                s.Points[_lastHighlightedIndex].MarkerSize = 6;
                s.Points[_lastHighlightedIndex].MarkerColor = Color.Blue;
            }

            for (int i = 0; i < s.Points.Count; i++)
            {
                double pointX = s.Points[i].XValue;
                // 浮点数比较容差
                if (Math.Abs(pointX - targetX) < 0.000001)
                {
                    s.Points[i].MarkerSize = 15;
                    s.Points[i].MarkerColor = Color.Yellow;
                    _lastHighlightedIndex = i;
                    break;
                }
            }
        }

        private double GetDouble(DataRowView row, string colName)
        {
            if (row.DataView.Table.Columns.Contains(colName) && row[colName] != DBNull.Value)
            {
                double v;
                if (double.TryParse(row[colName].ToString(), out v)) return v;
            }
            return -1.0;
        }
    }
}