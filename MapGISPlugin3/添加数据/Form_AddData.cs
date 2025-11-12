using System;
using System.IO;
using System.Windows.Forms;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.PluginEngine;
using MapGIS.RasCommonObj;
using System;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase.GeoRaster;
using System.Drawing;

namespace MapGISPlugin3
{
    public partial class Form_AddData : Form
    {
        // 新增：窗口拖动与拉伸所需字段
        private Point mousePoint = new Point();
        private int lastWidth = 0;
        private int lastHeight = 0;
        private Label[] borderLabels = new Label[4];
        // 保存从命令传递过来的主程序引用
        private IApplication _hook = null;

        // 构造函数，接收 IApplication 实例
        public Form_AddData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            // 新增：初始化拖动和边框拉伸功能
            InitCustomBorder();
            InitTitleDrag();

            // 在窗体加载时，执行初始化操作
            this.Load += new EventHandler(Form_AddData_Load);
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        private void Form_AddData_Load(object sender, EventArgs e)
        {
            // 1. 填充数据类型下拉框
            comboBoxDataType.Items.Clear();
            comboBoxDataType.Items.Add("重力");
            comboBoxDataType.Items.Add("磁法");
            comboBoxDataType.Items.Add("电法");
            // 默认选中第一项
            if (comboBoxDataType.Items.Count > 0)
            {
                comboBoxDataType.SelectedIndex = 0;
            }

            // 2. 绑定按钮的点击事件 (确保设计器中的事件也已关联)
            //this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            //this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        }

        /// <summary>
        /// “...”浏览按钮的点击事件
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择数据文件";
                openFileDialog.Filter = "GRD 栅格文件|*.grd|所有文件|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 将用户选择的文件路径显示在文本框中
                    textBoxDataPath.Text = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// “确定”按钮的点击事件
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            // --- 输入验证 ---
            if (comboBoxDataType.SelectedItem == null)
            {
                MessageBox.Show("请选择一个数据类型。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string dataPath = textBoxDataPath.Text.Trim();
            if (string.IsNullOrEmpty(dataPath) || !File.Exists(dataPath))
            {
                MessageBox.Show("请选择一个有效的数据文件路径。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- 核心逻辑 ---
            try
            {
                // 1. 根据下拉框选择，确定目标地图的名称
                string selectedType = comboBoxDataType.SelectedItem.ToString();
                string targetMapName = selectedType + "数据"; // 例如 "重力" -> "重力数据"

                // 2. 在当前文档中查找这个地图
                Map targetMap = FindMapByName(targetMapName);

                if (targetMap == null)
                {
                    MessageBox.Show($"在当前项目中未找到名为 '{targetMapName}' 的地图。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. 加载GRD数据并添加到找到的地图中
                LoadGrdToMap(targetMap, dataPath);

                // 4. 成功后关闭窗体
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加数据时发生未知错误: \n" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// “取消”按钮的点击事件
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 根据名称查找地图对象
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
                    return currentMap; // 找到了，返回它
                }
            }
            return null; // 循环结束都没找到
        }

        /// <summary>
        /// 将指定的GRD文件作为图层加载到目标地图中
        /// </summary>
        private void LoadGrdToMap(Map targetMap, string filePath)
        {
            RasterDataSet rstDataSet = null;
            try
            {
                rstDataSet = new RasterDataSet();
                if (rstDataSet.Open(filePath, RasAccessType.RasAccessType_ReadOnly))
                {
                    RasterLayer rstLayer = new RasterLayer();
                    rstLayer.AttachData(rstDataSet);
                    // 使用文件名作为默认图层名
                    rstLayer.Name = Path.GetFileNameWithoutExtension(filePath);

                    // 将图层添加到目标地图
                    targetMap.Append(rstLayer);

                    MessageBox.Show($"数据 '{rstLayer.Name}' 已成功添加到地图 '{targetMap.Name}' 中！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    throw new Exception("打开GRD文件失败。请检查文件是否有效或被占用。");
                }
            }
            catch (Exception ex)
            {
                // 如果发生异常，确保关闭数据集
                if (rstDataSet != null) rstDataSet.Close();
                // 重新抛出异常，让上层调用者（btnOK_Click）知道出错了
                throw ex;
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
    // 创建4个边框Label
    for (int i = 0; i < 4; i++)
    {
        borderLabels[i] = new Label();
        borderLabels[i].BackColor = System.Drawing.Color.FromArgb(188, 182, 211); // 统一边框颜色
        borderLabels[i].Size = new Size(2, 2);
        this.Controls.Add(borderLabels[i]);
        this.Controls.SetChildIndex(borderLabels[i], 0); // 边框置于底层
    }

    // 设置边框停靠和光标
    borderLabels[0].Dock = DockStyle.Left;     // 左边框
    borderLabels[1].Dock = DockStyle.Top;      // 上边框
    borderLabels[2].Dock = DockStyle.Right;    // 右边框
    borderLabels[3].Dock = DockStyle.Bottom;   // 下边框

    borderLabels[0].Cursor = Cursors.SizeWE;   // 左右拉伸光标
    borderLabels[2].Cursor = Cursors.SizeWE;
    borderLabels[1].Cursor = Cursors.SizeNS;   // 上下拉伸光标
    borderLabels[3].Cursor = Cursors.SizeNS;

    // 绑定边框事件
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
/// 初始化标题栏拖动事件
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
/// 边框按下：记录窗口初始尺寸和鼠标位置
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
/// 左边框拉伸（限制最小宽度）
/// </summary>
private void LeftBorder_MouseMove(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Left)
    {
        int newWidth = lastWidth - (Control.MousePosition.X - mousePoint.X);
        if (newWidth >= 400) // 确保表单控件不拥挤
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
        if (newWidth >= 400)
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
        if (newHeight >= 200) // 适配表单内容的最小高度
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
        if (newHeight >= 200)
        {
            this.Height = newHeight;
        }
    }
}
        #endregion

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
