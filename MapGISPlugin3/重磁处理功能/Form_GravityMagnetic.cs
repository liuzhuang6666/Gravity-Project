using System;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.RasCommonObj;
using System.Drawing;

namespace MapGISPlugin3
{
    public partial class Form_GravityMagnetic : Form
    {
        // 新增：窗口拖动与边框拉伸所需字段
        private Point mousePoint = new Point();
        private int lastWidth = 0;
        private int lastHeight = 0;
        private Label[] borderLabels = new Label[4];
        private IApplication _hook = null;
        private Map _map = null;
        
       

        public Form_GravityMagnetic(IApplication hook)
        {
            // InitializeComponent() 必须是第一行，它会创建你在设计器里看到的所有控件
            InitializeComponent();
            // 新增：初始化拖动和边框拉伸功能
            InitCustomBorder();
            InitTitleDrag();

            // *** 修正点 1: 动态填充下拉框内容 ***
            // 清空所有可能的默认项，确保干净
            comboBox_MethodType.Items.Clear();
            // 添加你需要的选项
            comboBox_MethodType.Items.Add("概率成像方法");
            // 如果未来有更多方法，可以继续添加
            // comboBox_MethodType.Items.Add("其他方法A");
            // comboBox_MethodType.Items.Add("其他方法B");

            // *** 修正点 2: 动态绑定按钮的点击事件 ***
            // 这行代码将“确定”按钮的点击动作与我们已经写好的 btn_OK_Click 方法关联起来
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 这行代码将“取消”按钮的点击动作与我们已经写好的 btn_Cancel_Click 方法关联起来
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);

            // --- 以下是之前的代码，保持不变 ---
            _hook = hook;

            if (_hook != null && _hook.Document != null)
            {
                Maps maps = _hook.Document.GetMaps();
                if (maps != null && maps.Count > 0)
                {
                    _map = maps.GetMap(0);
                }
            }
        }

        #region UI Event Handlers (这部分代码已存在，无需修改)
        private void Form_GravityMagnetic_Load(object sender, EventArgs e)
        {
            // 当下拉框有内容后，默认选中第一项
            if (comboBox_MethodType.Items.Count > 0)
            {
                comboBox_MethodType.SelectedIndex = 0;
            }
            // 保持默认图层名
            textBox_LayerName.Text = "GravityMagnetic_Layer";
        }

        // “确定”按钮被点击时，会执行这个方法
        private void btn_OK_Click(object sender, EventArgs e)
        {
            // 检查下拉框是否选择了内容
            if (comboBox_MethodType.SelectedItem == null)
            {
                MessageBox.Show("请选择一个方法类型。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_map == null)
            {
                MessageBox.Show("未能获取到当前的地图对象，无法添加图层。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string layerName = textBox_LayerName.Text.Trim();
            if (string.IsNullOrEmpty(layerName))
            {
                MessageBox.Show("请输入图层的名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_LayerName.Focus();
                return;
            }
            LoadGrdData(layerName);
        }

        // “取消”按钮被点击时，会执行这个方法
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            // 关闭窗体
            this.Close();
        }
        #endregion

        private void LoadGrdData(string layerName)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "请选择重磁数据文件";
            openFileDialog.Filter = "GRD 栅格文件|*.grd|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                RasterDataSet rstDataSet = null;
                try
                {
                    rstDataSet = new RasterDataSet();
                    bool isOpenSuccess = rstDataSet.Open(filePath, RasAccessType.RasAccessType_ReadOnly);

                    if (isOpenSuccess)
                    {
                        RasterLayer rstLayer = new RasterLayer();
                        rstLayer.AttachData(rstDataSet);
                        rstLayer.Name = layerName;
                        _map.Append(rstLayer);

                        MessageBox.Show("数据加载成功！地图将在您操作后刷新。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("打开GRD文件失败。请检查文件是否有效或被占用。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (rstDataSet != null) rstDataSet.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("加载数据时发生异常: \n" + ex.ToString(), "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (rstDataSet != null) rstDataSet.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region --- 新增：窗口拖动与边框拉伸逻辑 ---
        /// <summary>
        /// 初始化自定义边框（左、上、右、下）
        /// </summary>
        private void InitCustomBorder()
        {
            // 1. 创建4个边框Label
            for (int i = 0; i < 4; i++)
            {
                borderLabels[i] = new Label();
                borderLabels[i].BackColor = System.Drawing.Color.FromArgb(188, 182, 211); // 与其他表单统一边框颜色
                borderLabels[i].Size = new Size(2, 2); // 边框宽度/高度
                this.Controls.Add(borderLabels[i]);
                this.Controls.SetChildIndex(borderLabels[i], 0); // 边框置于底层，不遮挡控件
            }

            // 2. 设置边框停靠与拉伸光标
            borderLabels[0].Dock = DockStyle.Left;     // 左边框
            borderLabels[1].Dock = DockStyle.Top;      // 上边框
            borderLabels[2].Dock = DockStyle.Right;    // 右边框
            borderLabels[3].Dock = DockStyle.Bottom;   // 下边框

            borderLabels[0].Cursor = Cursors.SizeWE;   // 左右拉伸光标
            borderLabels[2].Cursor = Cursors.SizeWE;
            borderLabels[1].Cursor = Cursors.SizeNS;   // 上下拉伸光标
            borderLabels[3].Cursor = Cursors.SizeNS;

            // 3. 绑定边框事件（按下+移动）
            foreach (var label in borderLabels)
            {
                label.MouseDown += Border_MouseDown;
            }
            borderLabels[0].MouseMove += LeftBorder_MouseMove;
            borderLabels[1].MouseMove += TopBorder_MouseMove;
            borderLabels[2].MouseMove += RightBorder_MouseMove;
            borderLabels[3].MouseMove += BottomBorder_MouseMove;
        }

        /// <summary>
        /// 初始化标题栏（panel1）拖动事件
        /// </summary>
        private void InitTitleDrag()
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

        /// <summary>
        /// 边框按下：记录窗口初始尺寸与鼠标位置
        /// </summary>
        private void Border_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastWidth = this.Width;
                lastHeight = this.Height;
                mousePoint = Control.MousePosition;
            }
        }

        /// <summary>
        /// 左边框拉伸（限制最小宽度，避免控件拥挤）
        /// </summary>
        private void LeftBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newWidth = lastWidth - (Control.MousePosition.X - mousePoint.X);
                if (newWidth >= 350) // 适配表单控件的最小宽度
                {
                    this.Width = newWidth;
                    this.Left = Control.MousePosition.X;
                }
            }
        }

        /// <summary>
        /// 右边框拉伸
        /// </summary>
        private void RightBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newWidth = lastWidth + (Control.MousePosition.X - mousePoint.X);
                if (newWidth >= 350)
                {
                    this.Width = newWidth;
                }
            }
        }

        /// <summary>
        /// 上边框拉伸（限制最小高度）
        /// </summary>
        private void TopBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newHeight = lastHeight - (Control.MousePosition.Y - mousePoint.Y);
                if (newHeight >= 180) // 适配表单内容的最小高度
                {
                    this.Height = newHeight;
                    this.Top = Control.MousePosition.Y;
                }
            }
        }

        /// <summary>
        /// 下边框拉伸
        /// </summary>
        private void BottomBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newHeight = lastHeight + (Control.MousePosition.Y - mousePoint.Y);
                if (newHeight >= 180)
                {
                    this.Height = newHeight;
                }
            }
        }
        #endregion
    }
}