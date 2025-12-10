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
        private DataTable _dt; // 总数据
        private DataView _currentDv; // 【新增】当前正在展示的数据切片(某条测线某测点)
        private int _lastHighlightedIndex = -1;

        // 【新增】模式切换按钮
        private RadioButton _rbTE;
        private RadioButton _rbTM;

        public Preview_MT()
        {
            InitializeComponent();

            // 0. 【新增】先初始化顶部的切换按钮
            InitializeModeSwitcher();

            InitializeChart();
            InitializeGrid();
            InitializeTree();
        }

        // ---------------------------------------------------------
        // 0. 【新增】动态创建 TE/TM 切换按钮
        // ---------------------------------------------------------
        private void InitializeModeSwitcher()
        {
            // 1. 创建一个流式布局面板 (自动横向排列)
            FlowLayoutPanel panelTop = new FlowLayoutPanel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 35; //稍微高一点
            panelTop.Padding = new Padding(5); // 边距
            panelTop.AutoSize = false;
            panelTop.BackColor = Color.WhiteSmoke; // 给个背景色区分一下

            // 2. 创建 TE 按钮
            _rbTE = new RadioButton();
            _rbTE.Text = "TE 模式";
            _rbTE.AutoSize = true;
            _rbTE.Checked = true; // 默认选中 TE
            _rbTE.Margin = new Padding(10, 5, 20, 0); // 右边距设大点，把 TM 顶开

            // 3. 创建 TM 按钮 (确保这个被添加进去)
            _rbTM = new RadioButton();
            _rbTM.Text = "TM 模式";
            _rbTM.AutoSize = true;
            _rbTM.Margin = new Padding(0, 5, 0, 0);

            // 4. 绑定事件 (两个都要绑定！)
            // 为了防止逻辑混乱，我们统一用同一个事件处理函数
            EventHandler onModeChanged = (s, e) =>
            {
                // 只有当被选中的那个触发事件时才刷新 (防止取消选中也触发一次，导致闪烁)
                RadioButton rb = s as RadioButton;
                if (rb != null && rb.Checked)
                {
                    RefreshCurrentChart();
                }
            };

            _rbTE.CheckedChanged += onModeChanged;
            _rbTM.CheckedChanged += onModeChanged;

            // 5. 【重要】按顺序加入面板
            panelTop.Controls.Add(_rbTE);
            panelTop.Controls.Add(_rbTM);

            // 6. 加到主界面
            this.Controls.Add(panelTop);
            panelTop.BringToFront(); // 确保它在最上面
        }
        // ---------------------------------------------------------
        // 1. 初始化配置 (保持不变)
        // ---------------------------------------------------------
        private void InitializeChart()
        {
            chartMT.Series.Clear();
            chartMT.ChartAreas.Clear();
            chartMT.Legends.Clear();

            ChartArea area = new ChartArea("MainArea");
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;
            area.AxisY2.IsLogarithmic = false;

            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY2.MajorGrid.Enabled = false;

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
            tvData.HideSelection = false;
        }

        // ---------------------------------------------------------
        // 2. 数据加载 (构建树) - 保持不变
        // ---------------------------------------------------------
        public void LoadData(DataTable dt)
        {
            _dt = dt;
            _currentDv = null; // 清空当前状态
            tvData.Nodes.Clear();
            ResetChartState();
            gridMT.DataSource = null;

            if (_dt == null || _dt.Rows.Count == 0) return;
            if (!_dt.Columns.Contains("测线编号") || !_dt.Columns.Contains("测点编号"))
            {
                MessageBox.Show("数据缺少 '测线编号' 或 '测点编号' 字段。");
                return;
            }

            try
            {
                tvData.BeginUpdate();
                var lineGroups = _dt.AsEnumerable()
                                    .GroupBy(r => r["测线编号"].ToString())
                                    .OrderBy(g => g.Key);

                foreach (var lineGroup in lineGroups)
                {
                    string lineName = lineGroup.Key;
                    TreeNode lineNode = new TreeNode($"测线: {lineName}");
                    lineNode.Tag = "LINE";
                    lineNode.ImageIndex = 0;

                    var stations = lineGroup.Select(r => r["测点编号"].ToString())
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
                MessageBox.Show("构建目录出错: " + ex.Message);
            }
        }

        // ---------------------------------------------------------
        // 3. 点击事件与绘图逻辑
        // ---------------------------------------------------------
        private void TvData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null || node.Tag == null) return;

            if (node.Tag is string[] tags)
            {
                // 1. 准备数据
                PrepareStationData(tags[0], tags[1]);
                // 2. 刷新图表 (根据当前 TE/TM 按钮状态)
                RefreshCurrentChart();
            }
            else
            {
                ResetChartState();
                gridMT.DataSource = null;
                _currentDv = null;
            }
        }

        // 【新拆分的方法】只负责准备 DataView，不负责画图
        private void PrepareStationData(string lineName, string stationName)
        {
            DataView dv = new DataView(_dt);
            dv.RowFilter = $"测线编号 = '{lineName}' AND 测点编号 = '{stationName}'";

            if (_dt.Columns.Contains("频率"))
                dv.Sort = "频率 DESC";
            else if (_dt.Columns.Contains("周期"))
                dv.Sort = "周期 ASC";

            _currentDv = dv; // 保存到全局变量，供切换按钮使用
            gridMT.DataSource = dv; // 表格始终显示所有列，不用根据TE/TM变
        }

        // 【新拆分的方法】根据当前按钮状态画图
        private void RefreshCurrentChart()
        {
            if (_currentDv == null) return;
            UpdateChart(_currentDv);
        }

        private void UpdateChart(DataView dv)
        {
            chartMT.Series.Clear();
            var area = chartMT.ChartAreas[0];
            area.AxisX.IsLogarithmic = false;
            area.AxisY.IsLogarithmic = false;

            if (dv.Count == 0) return;
            _lastHighlightedIndex = -1;

            // ===========================================
            // A. 【核心修改】确定要画哪些列
            // ===========================================
            string modeName = _rbTE.Checked ? "TE模式" : "TM模式";
            string colRes = _rbTE.Checked ? "视电阻率_TE" : "视电阻率_TM";
            string colPhs = _rbTE.Checked ? "相位_TE" : "相位_TM";

            // 容错：如果找不到 _TE 列，尝试找不带后缀的 "视电阻率"
            if (!dv.Table.Columns.Contains(colRes)) colRes = "视电阻率";
            if (!dv.Table.Columns.Contains(colPhs)) colPhs = "相位";

            // B. 创建 Series
            Series sRes = chartMT.Series.Add(modeName + " - 视电阻率");
            sRes.ChartType = SeriesChartType.Spline;
            sRes.MarkerStyle = MarkerStyle.Circle;
            sRes.MarkerSize = 6;
            sRes.Color = _rbTE.Checked ? Color.Blue : Color.Green; // TE蓝, TM绿
            sRes.YAxisType = AxisType.Primary;

            Series sPhs = chartMT.Series.Add(modeName + " - 相位");
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
                // 智能识别 X 轴
                double XVal = -1;
                string xTitle = "X轴";

                if (dv.Table.Columns.Contains("频率"))
                {
                    XVal = GetDouble(row, "频率");
                    xTitle = "频率 (Hz)";
                }
                else if (dv.Table.Columns.Contains("周期"))
                {
                    XVal = GetDouble(row, "周期");
                    xTitle = "周期 (s)";
                }
                area.AxisX.Title = xTitle;

                // 【核心修改】直接取指定列的值
                double Rho = GetDouble(row, colRes);
                double Phs = GetDouble(row, colPhs);

                // 只有坐标和电阻率都 > 0 才画 (双对数要求)
                if (XVal > 0 && Rho > 0)
                {
                    int idx = sRes.Points.AddXY(XVal, Rho);

                    // 相位可能为负或者0，可以画，但不能参与对数轴计算(通常相位用线性轴)
                    sPhs.Points.AddXY(XVal, Phs);

                    sRes.Points[idx].Tag = XVal;
                    xValues.Add(XVal);
                    yResValues.Add(Rho);
                }
            }

            // D. 安全开启对数轴
            if (xValues.Count > 0)
            {
                bool canLogX = xValues.All(v => v > 0);
                bool canLogY = yResValues.All(v => v > 0);

                area.AxisX.IsLogarithmic = canLogX;
                area.AxisY.IsLogarithmic = canLogY;

                // 频率反转
                if (area.AxisX.Title.Contains("频率"))
                    area.AxisX.IsReversed = true;
                else
                    area.AxisX.IsReversed = false;

                area.RecalculateAxesScale();
            }
        }

        // ---------------------------------------------------------
        // 4. 辅助方法 (ResetChartState, Highlight, GetDouble) - 保持不变
        // ---------------------------------------------------------
        private void ResetChartState()
        {
            chartMT.Series.Clear();
            if (chartMT.ChartAreas.Count > 0)
            {
                var area = chartMT.ChartAreas[0];
                area.AxisX.IsLogarithmic = false;
                area.AxisY.IsLogarithmic = false;
                area.AxisX.Title = "X轴";
            }
        }

        private void GridMT_SelectionChanged(object sender, EventArgs e)
        {
            if (gridMT.CurrentRow == null || chartMT.Series.Count == 0) return;

            string colName = "";
            if (gridMT.Columns.Contains("频率")) colName = "频率";
            else if (gridMT.Columns.Contains("周期")) colName = "周期";

            if (string.IsNullOrEmpty(colName)) return;

            object val = gridMT.CurrentRow.Cells[colName].Value;
            if (val != null && val != DBNull.Value)
            {
                double selectedX;
                if (double.TryParse(val.ToString(), out selectedX))
                {
                    HighlightChartPoint(selectedX);
                }
            }
        }

        private void HighlightChartPoint(double targetX)
        {
            // 只高亮第一条曲线 (电阻率)
            if (chartMT.Series.Count == 0) return;
            Series s = chartMT.Series[0];

            if (_lastHighlightedIndex >= 0 && _lastHighlightedIndex < s.Points.Count)
            {
                s.Points[_lastHighlightedIndex].MarkerSize = 6;
                s.Points[_lastHighlightedIndex].MarkerColor = _rbTE.Checked ? Color.Blue : Color.Green;
            }

            for (int i = 0; i < s.Points.Count; i++)
            {
                double pointX = s.Points[i].XValue;
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
            // 返回 -1 代表无效值
            return -1.0;
        }
    }
}