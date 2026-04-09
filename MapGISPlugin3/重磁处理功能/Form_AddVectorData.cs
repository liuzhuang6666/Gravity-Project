using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase; // 矢量数据库命名空间

namespace MapGISPlugin3
{
    public partial class Form_AddVectorData : Form
    {
        // 窗口拖动所需字段
        private Point mousePoint = new Point();
        // 保存主程序引用
        private IApplication _hook = null;

        public Form_AddVectorData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            // 初始化自定义标题栏拖动功能
            InitTitleDrag();
        }

        private void Form_AddVectorData_Load(object sender, EventArgs e)
        {
            // 初始化代码（如有需要）
        }

        /// <summary>
        /// “...”浏览按钮：筛选 MapGIS 区文件 (.wp)
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择矢量区域数据 (区文件)";
                openFileDialog.Filter = "MapGIS 区文件|*.wp|所有文件|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxDataPath.Text = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// “确定”按钮：加载矢量数据逻辑
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            string dataPath = textBoxDataPath.Text.Trim();
            if (string.IsNullOrEmpty(dataPath) || !File.Exists(dataPath))
            {
                MessageBox.Show("请选择一个有效的区文件路径。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. 获取当前激活的地图，或者根据名称查找
                if (_hook == null || _hook.Document == null) return;

                // 这里默认添加到第一个地图，你也可以参考之前的 FindMapByName 逻辑
                Map targetMap = _hook.Document.GetMaps().GetMap(0);

                if (targetMap == null)
                {
                    MessageBox.Show("当前文档中没有可用的地图对象。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. 加载矢量数据
                LoadVectorToMap(targetMap, dataPath);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加矢量数据时发生错误: \n" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载 .wp 文件并添加到地图
        /// </summary>
        private void LoadVectorToMap(Map targetMap, string filePath)
        {
            // SFeatureCls 是 MapGIS 矢量要素类对象
            SFeatureCls sfCls = new SFeatureCls();

            // 打开区文件
            if (sfCls.Open(filePath))
            {
                // 创建矢量图层
                VectorLayer vecLayer = new VectorLayer(IntPtr.Zero, null);
                // 挂接数据源
                vecLayer.AttachData(sfCls);
                // 设置图层名
                vecLayer.Name = Path.GetFileNameWithoutExtension(filePath);

                // 添加到地图
                targetMap.Append(vecLayer);

                MessageBox.Show($"区域数据 '{vecLayer.Name}' 已成功添加！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                throw new Exception("打开 MapGIS 区文件失败。请检查文件格式是否正确。");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region --- 窗口拖动逻辑 ---

        private void InitTitleDrag()
        {
            panelTitle.MouseDown += TitlePanel_MouseDown;
            panelTitle.MouseMove += TitlePanel_MouseMove;
        }

        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousePoint.X = e.X;
                mousePoint.Y = e.Y;
            }
        }

        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left = Control.MousePosition.X - mousePoint.X;
                this.Top = Control.MousePosition.Y - mousePoint.Y;
            }
        }

        #endregion
    }
}