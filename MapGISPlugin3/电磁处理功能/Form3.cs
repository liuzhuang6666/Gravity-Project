using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using MapGIS.GISControl;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Info;
using MapGIS.RasCommonObj;
using MapGIS.RasAnalysis;
using MapGIS.Desktop.UI.Controls;
using MapGIS.GeoObjects.Geometry;
using MapGIS.PlugUtility;
using MapGIS.UI.Controls;
using MapGIS.GeoObjects.Att;
using System.Text.RegularExpressions;

namespace MapGISPlugin3
{
    public partial class Form3 : Form
    {
        Maps maps = null;
        Map map = null;
        public Form3 (Document doc)
        {
            Document dc = doc;
            Maps maps = dc.GetMaps();
            map = maps.GetMap(0);
            InitializeComponent();
            
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.SelectedItem.ToString();



        }


        private void LoadData(string type)
        {
            switch (type)
            {
                case "MT":
               
                    Loadmt();
                    
                    break;
                default:
                    MessageBox.Show("未知数据类型");
                    break;
            }
        }
        
        private void Loadmt()
        {
            // 实现数据加载逻辑
                // 创建 OpenFileDialog 对象
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "请选择文件";
                openFileDialog.Filter = "DAT 文件|*.dat"; // 只显示 .dat 文件
                ObjectCls MT = new ObjectCls();


            // 显示文件选择对话框
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // 读取列标题
                using (StreamReader reader = new StreamReader(filePath, Encoding.GetEncoding("GBK")))
                {

                    string headerLine = reader.ReadLine();
                    string[] columnNames = headerLine.Split(new char[] { '\t', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < columnNames.Length; i++)
                    {
                        // 移除括号
                        columnNames[i] = Regex.Replace(columnNames[i], @"\(|\)", string.Empty);
                    }

                    // 创建Fields对象来存储字段信息
                    Fields fields = new Fields();
                    //foreach (string columnName in columnNames)
                    //{

                    //    Field fldArea = new Field();

                    //    fldArea.FieldName = columnName;
                    //    fldArea.FieldType = FieldType.FldDouble;
                    //    fldArea.MskLength = 32;
                    //    fldArea.Editable = 1;


                    //    bool rtn = fields.AppendField(fldArea) > 0;
                    //    if (rtn==false)
                    //        MessageBox.Show("Field added:" + columnName);
                    //}               
                    Field fld1 = new Field();
                    fld1.FieldName = "频率";
                    fld1.FieldType = FieldType.FldDouble;
                    fld1.MskLength = 15;
                    fld1.PointLength = 6;
                    fields.AppendField(fld1);
                    //bool rtn = fields.AppendField(fld1) > 0;
                    //if (rtn)
                    //    MessageBox.Show("频率加载 ");

                    Field fld2 = new Field();
                    fld2.FieldName = "视电阻率Rxy";
                    fld2.FieldType = FieldType.FldDouble;
                    fld2.MskLength = 15;
                    fld2.PointLength = 6;
                    fields.AppendField(fld2);

                    //bool rtn2 = fields.AppendField(fld2) > 0;
                    //if (rtn2)
                    //    MessageBox.Show("视电阻率(Rxy)加载 ");

                    Field fld3 = new Field();
                    fld3.FieldName = "视电阻率Ryx";
                    fld3.FieldType = FieldType.FldDouble;
                    fld3.MskLength = 15;
                    fld3.PointLength = 6;
                    fields.AppendField(fld3);

                    //bool rtn3 = fields.AppendField(fld3) > 0;
                    //if (rtn3)
                    //    MessageBox.Show("视电阻率(Ryx)加载 ");

                    Field fld4 = new Field();
                    fld4.FieldName = "相位Pxy";
                    fld4.FieldType = FieldType.FldDouble;
                    fld4.MskLength = 15;
                    fld4.PointLength = 6;
                    fields.AppendField(fld4);
                    //bool rtn4 = fields.AppendField(fld4) > 0;
                    //if (rtn4)
                    //    MessageBox.Show("相位(Pxy)加载 ");

                    Field fld5 = new Field();
                    fld5.FieldName = "相位Pyx";
                    fld5.FieldType = FieldType.FldDouble;
                    fld5.MskLength = 15;
                    fld5.PointLength = 6;
                    fields.AppendField(fld5);
                    //bool rtn5 = fields.AppendField(fld5) > 0;
                    //if (rtn5)
                    //    MessageBox.Show("相位(Pyx)加载 ");

                    Field fld6 = new Field();
                    fld6.FieldName = "倾子模";
                    fld6.FieldType = FieldType.FldDouble;
                    fld6.MskLength = 15;
                    fld6.PointLength = 6;
                    fields.AppendField(fld6);
                    //bool rtn6 = fields.AppendField(fld6) > 0;
                    //if (rtn6)
                    //    MessageBox.Show("倾子模加载 ");

                    Field fld7 = new Field();
                    fld7.FieldName = "倾子相位";
                    fld7.FieldType = FieldType.FldDouble;
                    fld7.MskLength = 15;
                    fld7.PointLength = 6;
                    fields.AppendField(fld7);
                    //bool rtn7 = fields.AppendField(fld7) > 0;
                    //if (rtn7)
                    //    MessageBox.Show("倾子相位加载 ");

                    Field fld8 = new Field();
                    fld8.FieldName = "主轴方位角";
                    fld8.FieldType = FieldType.FldDouble;
                    fld8.MskLength = 15;
                    fld8.PointLength = 6;
                    fields.AppendField(fld8);
                    //bool rtn8 = fields.AppendField(fld8) > 0;
                    //if (rtn8)
                    //    MessageBox.Show("主轴方位角加载 ");

                    if (MT.Create("gdbp://MapGisLocal/地球物理/ocls/"+ textBox1.Text , fields) > 0)
                        MessageBox.Show("创建成功！");

                    // 创建Record对象
                    Record record = new Record();
                    record.Fields = fields;

                    string line;
                    // 从第二行开始读取数据
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] columnValues = line.Split(new char[] { '\t', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        // 根据字段索引设置字段值
                        for (int i = 0; i < columnValues.Length; i++)
                        {
                            record.SetValue(columnNames[i], columnValues[i]);

                        }

                        MT.Append(null, record, null);

                            
                    }
                    
                }
                ObjectLayer mt = new ObjectLayer();
                mt.AttachData(MT);
                map.Append(mt);
            }


            else
            {
                Console.WriteLine("未选择文件");
            }

            MT.Close();
        }
       
    
    private void button1_Click(object sender, EventArgs e)
        {
            string selectedType = comboBox1.SelectedItem.ToString();

            LoadData(selectedType);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    
}


