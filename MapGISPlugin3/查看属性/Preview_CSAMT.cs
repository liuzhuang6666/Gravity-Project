using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MapGISPlugin3.查看属性
{
    public partial class Preview_CSAMT : UserControl
    {
        private DataTable _dt;
        private int _lastHighlightedIndex = -1;

        public Preview_CSAMT()
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
            chartCSAMT.Series.Clear();
            chartCSAMT.ChartAreas.Clear();
            chartCSAMT.Legends.Clear();

            ChartArea area = new ChartArea("MainArea");

            // CSAMT 也是双对数坐标 (频率-电阻率)
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;
            area.AxisY2.IsLogarithmic = false;

            // 网格样式
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY2.MajorGrid.Enabled = false; // 相位通常不画网格

            // 标题
            area.AxisX.Title = "频率 (Hz)";
            area.AxisY.Title = "视电阻率 (Ω·m)";
            area.AxisY2.Title = "相位 (mrad 或 °)";

            chartCSAMT.ChartAreas.Add(area);

            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Top;
            legend.Alignment = StringAlignment.Center;
            chartCSAMT.Legends.Add(legend);
        }

        private void InitializeGrid()
        {
            gridCSAMT.ReadOnly = true;
            gridCSAMT.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCSAMT.AllowUserToAddRows = false;
            gridCSAMT.RowHeadersVisible = false;
            gridCSAMT.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCSAMT.SelectionChanged += GridCSAMT_SelectionChanged;
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
            gridCSAMT.DataSource = null;

            if (_dt == null || _dt.Rows.Count == 0) return;

            // --- 字段名适配 (兼容中文和英文) ---
            // 根据您的描述：PROFILE/测线编号, STATION/测点编号
            string colLine = FindColumn(_dt, "测线编号", "测线", "PROFILE", "Line");
            string colStation = FindColumn(_dt, "测点编号", "测点", "STATION", "Point");

            if (colLine == null || colStation == null)
            {
                MessageBox.Show("数据缺少测线或测点字段，请检查属性表头。");
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

                    // 获取该测线下的测点
                    var stations = lineGroup.Select(r => r[colStation].ToString())
                                            .Distinct()
                                            .OrderBy(s => s); // 字符串排序，如果是数字建议转int排

                    foreach (var stationName in stations)
                    {
                        TreeNode stationNode = new TreeNode(stationName);
                        // Tag 存数组: [测线名, 测点名]
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
                MessageBox.Show("构建目录出错: " + ex.Message);
            }
        }

        // ---------------------------------------------------------
        // 3. 点击与绘图
        // ---------------------------------------------------------
        private void TvData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null || node.Tag == null) return;

            if (node.Tag is string[] tags)
            {
                DrawStationData(tags[0], tags[1]);
            }
            else
            {
                ResetChartState();
                gridCSAMT.DataSource = null;
            }
        }

        private void DrawStationData(string lineName, string stationName)
        {
            // --- 字段适配 ---
            string colLine = FindColumn(_dt, "测线编号", "测线", "PROFILE");
            string colStation = FindColumn(_dt, "测点编号", "测点", "STATION");
            string colFreq = FindColumn(_dt, "频率", "FREQ", "Frequency");

            // 数据筛选
            DataView dv = new DataView(_dt);
            dv.RowFilter = $"{colLine} = '{lineName}' AND {colStation} = '{stationName}'";

            // CSAMT 按频率从高到低排序 (对应深度从浅到深)
            if (colFreq != null)
            {
                // 注意：如果频率列是字符串类型，排序可能会乱(100排在2前面)，建议在 LayerToDataTable 阶段转 double
                dv.Sort = $"{colFreq} DESC";
            }

            gridCSAMT.DataSource = dv;
            UpdateChart(dv);
        }

        private void UpdateChart(DataView dv)
        {
            chartCSAMT.Series.Clear();
            var area = chartCSAMT.ChartAreas[0];
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;

            if (dv.Count == 0) return;
            _lastHighlightedIndex = -1;

            // --- 字段适配 ---
            string colFreq = FindColumn(dv.Table, "频率", "FREQ");
            string colRes = FindColumn(dv.Table, "电阻率", "视电阻率", "RES", "Rho");
            string colPhs = FindColumn(dv.Table, "相位", "PHA", "Phase");

            if (colFreq == null || colRes == null) return; // 没频率或电阻率画不了

            // 1. 创建曲线
            Series sRes = chartCSAMT.Series.Add("视电阻率");
            sRes.ChartType = SeriesChartType.Spline;
            sRes.MarkerStyle = MarkerStyle.Circle;
            sRes.MarkerSize = 7;
            sRes.Color = Color.Blue;
            sRes.YAxisType = AxisType.Primary;

            Series sPhs = null;
            if (colPhs != null)
            {
                sPhs = chartCSAMT.Series.Add("相位");
                sPhs.ChartType = SeriesChartType.Spline;
                sPhs.MarkerStyle = MarkerStyle.Diamond;
                sPhs.MarkerSize = 7;
                sPhs.Color = Color.Red;
                sPhs.YAxisType = AxisType.Secondary;
            }

            // 2. 填充数据
            List<double> xValues = new List<double>();
            List<double> yValues = new List<double>();

            foreach (DataRowView row in dv)
            {
                double freq = GetDouble(row, colFreq);
                double res = GetDouble(row, colRes);

                // 双对数要求 > 0
                if (freq > 0 && res > 0)
                {
                    int idx = sRes.Points.AddXY(freq, res);
                    sRes.Points[idx].Tag = freq; // 用于高亮查找
                    xValues.Add(freq);
                    yValues.Add(res);

                    if (sPhs != null)
                    {
                        double phs = GetDouble(row, colPhs);
                        // 相位可以用线性轴画
                        sPhs.Points.AddXY(freq, phs);
                    }
                }
            }

            // 3. 设置对数轴
            if (xValues.Count > 0)
            {
                area.AxisX.IsLogarithmic = true; // 频率一定是 log
                area.AxisY.IsLogarithmic = true; // 电阻率一定是 log
                area.AxisX.IsReversed = true;    // 频率反转 (高频在左)
                area.RecalculateAxesScale();
            }
        }

        // ---------------------------------------------------------
        // 4. 辅助工具
        // ---------------------------------------------------------

        // 这是一个新加的工具，帮你自动找列名 (优先找第一个参数，找不到找第二个...)
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
            chartCSAMT.Series.Clear();
            if (chartCSAMT.ChartAreas.Count > 0)
            {
                chartCSAMT.ChartAreas[0].AxisX.IsLogarithmic = false;
                chartCSAMT.ChartAreas[0].AxisY.IsLogarithmic = false;
            }
        }

        // 表格高亮反查
        private void GridCSAMT_SelectionChanged(object sender, EventArgs e)
        {
            // 1. 基础检查
            if (gridCSAMT.CurrentRow == null || chartCSAMT.Series.Count == 0) return;
            if (gridCSAMT.DataSource == null) return;

            // 2. 【核心修复】安全获取 DataTable
            // 因为 DataSource 可能是 DataTable 也可能是 DataView，必须安全判断
            DataTable dt = null;
            if (gridCSAMT.DataSource is DataView dv)
            {
                dt = dv.Table; // 如果是视图，取它的源表
            }
            else if (gridCSAMT.DataSource is DataTable table)
            {
                dt = table;    // 如果本身就是表
            }

            if (dt == null) return;

            // 3. 找频率列 (使用修复后的 dt)
            string colFreq = FindColumn(dt, "频率", "FREQ", "Frequency");
            if (colFreq == null) return;

            // 4. 获取当前选中的值
            // DataGridView 虽然绑的是 DataView，但 CurrentRow.Cells 取值逻辑是一样的
            if (!gridCSAMT.Columns.Contains(colFreq)) return; // 双重保险

            object val = gridCSAMT.CurrentRow.Cells[colFreq].Value;
            double targetX;
            if (val != null && double.TryParse(val.ToString(), out targetX))
            {
                HighlightPoint(targetX);
            }
        }
        private void HighlightPoint(double targetX)
        {
            Series s = chartCSAMT.Series[0];
            // 还原上一个
            if (_lastHighlightedIndex >= 0 && _lastHighlightedIndex < s.Points.Count)
            {
                s.Points[_lastHighlightedIndex].MarkerSize = 7;
                s.Points[_lastHighlightedIndex].MarkerColor = Color.Blue;
            }

            // 找新的
            for (int i = 0; i < s.Points.Count; i++)
            {
                // 浮点数比较
                if (Math.Abs(s.Points[i].XValue - targetX) < 0.000001)
                {
                    s.Points[i].MarkerSize = 14;
                    s.Points[i].MarkerColor = Color.Yellow;
                    _lastHighlightedIndex = i;
                    break;
                }
            }
        }
    }
}