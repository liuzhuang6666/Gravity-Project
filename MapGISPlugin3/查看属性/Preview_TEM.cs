using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq; // 必须引用
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MapGISPlugin3.查看属性
{
    public partial class Preview_TEM : UserControl
    {
        private DataTable _dt;
        private int _lastHighlightedIndex = -1;

        public Preview_TEM()
        {
            InitializeComponent();
            InitializeChart();
            InitializeGrid();
            InitializeTree();
        }

        // ---------------------------------------------------------
        // 1. 初始化配置
        // ---------------------------------------------------------
        private void InitializeChart()
        {
            chartTEM.Series.Clear();
            chartTEM.ChartAreas.Clear();
            chartTEM.Legends.Clear();

            ChartArea area = new ChartArea("MainArea");

            // 【关键修改】初始化时必须是 False (线性)，防止空图表崩溃
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;

            // 网格美化
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineColor = Color.WhiteSmoke;

            area.AxisX.Title = "时间 (ms)";
            area.AxisY.Title = "感应电动势 (mV)";

            chartTEM.ChartAreas.Add(area);

            // ... Legend 代码不变 ...
            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Top;
            legend.Alignment = StringAlignment.Center;
            chartTEM.Legends.Add(legend);
        }

        private void InitializeGrid()
        {
            gridTEM.ReadOnly = true;
            gridTEM.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTEM.AllowUserToAddRows = false;
            gridTEM.RowHeadersVisible = false;
            gridTEM.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // 绑定修复后的安全选择事件
            gridTEM.SelectionChanged += GridTEM_SelectionChanged;
        }

        private void InitializeTree()
        {
            tvData.AfterSelect += TvData_AfterSelect;
            tvData.HideSelection = false;
        }

        // ---------------------------------------------------------
        // 2. 数据加载
        // ---------------------------------------------------------
        public void LoadData(DataTable dt)
        {
            _dt = dt;
            tvData.Nodes.Clear();
            ResetChartState();
            gridTEM.DataSource = null;

            if (_dt == null || _dt.Rows.Count == 0) return;

            // 字段匹配：PROFILE, STATION
            string colLine = FindColumn(_dt, "PROFILE", "测线号", "Line");
            string colStation = FindColumn(_dt, "STATION", "测点号", "Point");

            if (colLine == null || colStation == null)
            {
                MessageBox.Show("TEM 数据缺少 PROFILE(测线号) 或 STATION(测点号) 字段。");
                return;
            }

            try
            {
                tvData.BeginUpdate();

                // 按测线分组
                var lineGroups = _dt.AsEnumerable()
                                    .GroupBy(r => r[colLine].ToString())
                                    .OrderBy(g => g.Key);

                foreach (var lineGroup in lineGroups)
                {
                    string lineName = lineGroup.Key;
                    TreeNode lineNode = new TreeNode($"测线: {lineName}");
                    lineNode.Tag = "LINE";
                    lineNode.ImageIndex = 0;

                    var stations = lineGroup.Select(r => r[colStation].ToString())
                                            .Distinct()
                                            .OrderBy(s => s);

                    foreach (var stationName in stations)
                    {
                        TreeNode stationNode = new TreeNode(stationName);
                        stationNode.Tag = new string[] { lineName, stationName };
                        stationNode.ImageIndex = 1;
                        lineNode.Nodes.Add(stationNode);
                    }
                    tvData.Nodes.Add(lineNode);
                }
                tvData.EndUpdate();
                if (tvData.Nodes.Count > 0) tvData.Nodes[0].Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show("TEM 目录构建出错: " + ex.Message);
            }
        }

        // ---------------------------------------------------------
        // 3. 绘图逻辑
        // ---------------------------------------------------------
        private void TvData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is string[] tags)
            {
                DrawStationData(tags[0], tags[1]);
            }
            else
            {
                ResetChartState();
                gridTEM.DataSource = null;
            }
        }

        private void DrawStationData(string lineName, string stationName)
        {
            string colLine = FindColumn(_dt, "PROFILE", "测线号");
            string colStation = FindColumn(_dt, "STATION", "测点号");
            string colTime = FindColumn(_dt, "TIMES", "采样时间_us", "Time");

            DataView dv = new DataView(_dt);
            dv.RowFilter = $"{colLine} = '{lineName}' AND {colStation} = '{stationName}'";

            // TEM 必须按时间从小到大排序 (衰减过程)
            if (colTime != null)
            {
                dv.Sort = $"{colTime} ASC";
            }

            gridTEM.DataSource = dv;
            UpdateChart(dv);
        }

        private void UpdateChart(DataView dv)
        {
            ResetChartState();
            var area = chartTEM.ChartAreas[0];
            area.AxisX.Title = "时间 (ms)";
            area.AxisY.Title = "感应电动势 (mV)";
            if (dv.Count == 0) return;
            _lastHighlightedIndex = -1;

            // 字段匹配
            string colTime = FindColumn(dv.Table, "TIMES", "采样时间_us");
            string colVal = FindColumn(dv.Table, "CHZ", "感应电压_mV", "V", "EMF");

            if (colTime == null || colVal == null) return;

            Series sDecay = chartTEM.Series.Add("Z向感应电动势 (CHZ)");
            sDecay.ChartType = SeriesChartType.Spline; // 或者 Line
            sDecay.MarkerStyle = MarkerStyle.Square;
            sDecay.MarkerSize = 6;
            sDecay.Color = Color.Red;
            sDecay.BorderWidth = 2;

            List<double> xValues = new List<double>();
            List<double> yValues = new List<double>();

            foreach (DataRowView row in dv)
            {
                double t_micro = GetDouble(row, colTime);
                double val = GetDouble(row, colVal);

                // 【关键】对数坐标必须严格 > 0
                if (t_micro > 0 && val > 0)
                {
                    double t_ms = t_micro / 1000.0;
                    int idx = sDecay.Points.AddXY(t_ms, val);
                    sDecay.Points[idx].Tag = t_ms;
                    xValues.Add(t_ms);
                }
            }

            // 【关键修改】只有当确实加入了有效点后，才开启对数轴
            if (xValues.Count > 0)
            {
                // 只有这里才能设为 true
                area.AxisX.IsLogarithmic = true;
                area.AxisY.IsLogarithmic = true;

                // 显式重新计算刻度，防止缓存了 0 值
                area.RecalculateAxesScale();
            }
            else
            {
                // 如果数据全都是 0 或负数，保持 Linear，不然也会崩
                area.AxisX.IsLogarithmic = false;
                area.AxisY.IsLogarithmic = false;
            }
        }

        // ---------------------------------------------------------
        // 4. 辅助与反查 (已包含安全类型检查)
        // ---------------------------------------------------------
        private void GridTEM_SelectionChanged(object sender, EventArgs e)
        {
            if (gridTEM.CurrentRow == null || chartTEM.Series.Count == 0) return;
            if (gridTEM.DataSource == null) return;

            // 安全获取 DataTable
            DataTable dt = null;
            if (gridTEM.DataSource is DataView dv) dt = dv.Table;
            else if (gridTEM.DataSource is DataTable t) dt = t;
            if (dt == null) return;

            string colTime = FindColumn(dt, "TIMES", "采样时间_us");
            if (colTime == null) return;
            if (!gridTEM.Columns.Contains(colTime)) return;

            object val = gridTEM.CurrentRow.Cells[colTime].Value;
            double targetTimeUS; // 表格里是微秒

            if (val != null && double.TryParse(val.ToString(), out targetTimeUS))
            {
                double targetTimeMS = targetTimeUS / 1000.0; // 转成毫秒去图上找
                HighlightPoint(targetTimeMS);
            }
        }

        private void HighlightPoint(double targetX_MS)
        {
            Series s = chartTEM.Series[0];

            // 还原旧点
            if (_lastHighlightedIndex >= 0 && _lastHighlightedIndex < s.Points.Count)
            {
                s.Points[_lastHighlightedIndex].MarkerSize = 6;
                s.Points[_lastHighlightedIndex].MarkerColor = Color.Red;
            }

            // 找新点 (容差稍微大一点点，因为涉及浮点除法)
            for (int i = 0; i < s.Points.Count; i++)
            {
                if (Math.Abs(s.Points[i].XValue - targetX_MS) < 0.00001)
                {
                    s.Points[i].MarkerSize = 14;
                    s.Points[i].MarkerColor = Color.Blue; // 选中变蓝
                    _lastHighlightedIndex = i;
                    break;
                }
            }
        }

        private string FindColumn(DataTable dt, params string[] candidates)
        {
            foreach (string name in candidates)
            {
                if (dt.Columns.Contains(name)) return name;
            }
            return null;
        }

        private double GetDouble(DataRowView row, string colName)
        {
            if (string.IsNullOrEmpty(colName)) return -1;
            if (row[colName] != DBNull.Value)
            {
                double v;
                if (double.TryParse(row[colName].ToString(), out v)) return v;
            }
            return -1;
        }

        private void ResetChartState()
        {
            chartTEM.Series.Clear();
            if (chartTEM.ChartAreas.Count > 0)
            {
                // 【关键修改】清空数据前，先切回线性，保命
                chartTEM.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartTEM.ChartAreas[0].AxisY.IsLogarithmic = false;
                // 顺便重置标题
                chartTEM.ChartAreas[0].AxisX.Title = "时间 (ms)";
            }
        }
    }
}