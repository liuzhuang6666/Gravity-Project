using System;
using System.Data;
using System.Windows.Forms;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Att;
// 引用刚才写的 UserControl 所在的命名空间
using MapGISPlugin3.查看属性;

namespace MapGISPlugin3
{
    public partial class GeophysicalPreviewControl : UserControl
    {
        public GeophysicalPreviewControl()
        {
            InitializeComponent();
        }

        public void Initialize(MapLayer layer)
        {
            this.Controls.Clear();
            if (layer == null) return;

            // 1. 获取图层名称并转大写，方便判断
            string layerName = layer.Name.Trim().ToUpper();
            // 假设命名格式：MT_测线1_2023... 或 CSAMT_工区B_...

            // 2. 转 DataTable (这个方法保持不变)
            DataTable dt = LayerToDataTable(layer);
            if (dt == null || dt.Rows.Count == 0)
            {
                ShowMessage($"图层 [{layer.Name}] 无数据。");
                return;
            }

            UserControl viewToLoad = null;

            // 3. 【核心修改】根据名字分发
            if (layerName.StartsWith("MT"))
            {
                // ---> MT (大地电磁)
                var view = new Preview_MT();
                view.LoadData(dt);
                viewToLoad = view;
            }
            else if (layerName.StartsWith("CSAMT"))
            {
                // ---> CSAMT (可控源音频大地电磁)
                var view = new Preview_CSAMT(); // 稍后创建这个类
                view.LoadData(dt);
                viewToLoad = view;
            }
            else if (layerName.StartsWith("TEM"))
            {
                // ---> TEM (瞬变电磁)
                var view = new Preview_TEM(); // 稍后创建这个类
                view.LoadData(dt);
                viewToLoad = view;
            }
            else
            {
                // ---> 其他数据 (普通表格)
                var view = new Preview_Table();
                view.LoadData(dt);
                viewToLoad = view;
            }

            // 4. 挂载
            if (viewToLoad != null)
            {
                viewToLoad.Dock = DockStyle.Fill;
                this.Controls.Add(viewToLoad);
            }
        }

        // --- 判断逻辑 ---
        private bool IsMTData(DataTable dt)
        {
            // 只要同时有 "周期" 和 "测点编号"，并且有电阻率数据，就认为是 MT
            bool hasPeriod = dt.Columns.Contains("周期");
            bool hasStation = dt.Columns.Contains("测点编号");
            bool hasRes = dt.Columns.Contains("视电阻率_TE") || dt.Columns.Contains("视电阻率_TM");

            return hasPeriod && hasStation && hasRes;
        }

        // ----------------------------------------------------------
        // 🛠️ 辅助工具：图层转 DataTable (带数据类型修复版)
        // ----------------------------------------------------------
        private DataTable LayerToDataTable(object layerObj)
        {
            DataTable dt = new DataTable();
            RecordSet rs = null;
            Fields fields = null;

            try
            {
                // 1. 获取底层数据类 (BasCls)
                IBasCls dataObj = null;
                if (layerObj is MapLayer mapLayer) dataObj = mapLayer.GetData();
                else if (layerObj is ObjectLayer objLayer) dataObj = objLayer.GetData();

                if (dataObj == null) return null;

                // 2. 获取记录集
                if (dataObj is IVectorCls vecCls)
                {
                    rs = vecCls.Select(null);
                    fields = vecCls.Fields;
                }
                else if (dataObj is ObjectCls objCls)
                {
                    rs = objCls.Select(null);
                    fields = objCls.Fields;
                }

                if (rs == null || fields == null) return null;

                // 3. 【核心修复】构建列结构时，指定正确的数据类型！
                for (int i = 0; i < fields.Count; i++)
                {
                    Field fld = fields.GetItem(i); // 获取字段定义
                    Type colType = typeof(string); // 默认是字符串

                    // 根据 MapGIS 的字段类型，映射到 C# 的类型
                    switch (fld.FieldType)
                    {
                        case FieldType.FldDouble:
                        case FieldType.FldFloat:
                            colType = typeof(double); // 浮点数 -> double
                            break;
                        case FieldType.FldLong:
                        case FieldType.FldShort:
                        case FieldType.FldInt64:
                            colType = typeof(double); // 整数也转 double，方便画图计算
                            break;
                            // 其他类型默认 string
                    }

                    dt.Columns.Add(fld.FieldName, colType);
                }

                // 4. 填充数据
                rs.MoveFirst();
                int maxRows = 1000000;
                int current = 0;

                while (!rs.IsEOF && current < maxRows)
                {
                    DataRow row = dt.NewRow();
                    Record att = rs.Att;
                    if (att != null)
                    {
                        for (int i = 0; i < fields.Count; i++)
                        {
                            object val = att.GetValue(i);

                            // 处理空值
                            if (val == null)
                            {
                                row[i] = DBNull.Value;
                            }
                            else
                            {
                                // 【安全转换】防止类型不匹配报错
                                // 比如数据库里是 float，DataTable 里是 double，直接赋可能会错，转一下
                                try
                                {
                                    if (dt.Columns[i].DataType == typeof(double))
                                        row[i] = Convert.ToDouble(val);
                                    else
                                        row[i] = val.ToString();
                                }
                                catch
                                {
                                    row[i] = DBNull.Value; // 转换失败当空值处理
                                }
                            }
                        }
                    }
                    dt.Rows.Add(row);
                    rs.MoveNext();
                    current++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取数据出错: {ex.Message}");
            }
            finally
            {
                // 释放资源...
            }

            return dt;
        }

        private void ShowMessage(string msg)
        {
            Label lbl = new Label();
            lbl.Text = msg;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(lbl);
        }
    }
}