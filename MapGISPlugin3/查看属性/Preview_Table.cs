using System;
using System.Data;
using System.Windows.Forms;

// 【注意命名空间】确保和你 Preview_MT 的位置逻辑一致
namespace MapGISPlugin3.查看属性
{
    public partial class Preview_Table : UserControl
    {
        public Preview_Table()
        {
            InitializeComponent();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            // 基础样式设置
            // 既然 gridGeneric 是你刚拖进去的，确保名字对上了就不会红
            gridGeneric.ReadOnly = true;
            gridGeneric.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridGeneric.AllowUserToAddRows = false;
            gridGeneric.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells; // 通用表格列宽根据内容调整
        }

        // 对外接口：只负责显示数据
        public void LoadData(DataTable dt)
        {
            gridGeneric.DataSource = null;

            if (dt != null)
            {
                gridGeneric.DataSource = dt;
            }
            else
            {
                // 如果数据为空，可以显示个提示或者留白
                // MessageBox.Show("无数据"); 
            }
        }
    }
}