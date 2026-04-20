using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.Convert;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.Geologic.Model.GridModel;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;
using MapGIS.RasAnalysis;
using MapGIS.RasCommonObj;
using MapGIS.UI.Controls;
using System;
using System.Drawing;
using System.Runtime.Serialization.Formatters;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public partial class Form_Converter : Form
    {
        // 全局变量
        private IApplication m_Hook;
        private RasTrans m_rasTrans = new RasTrans();
        private Point m_MousePoint;

        // 构造函数
        public Form_Converter(IApplication hook)
        {
            InitializeComponent();
            m_Hook = hook;
        }

        #region 无边框窗口拖动 + 关闭功能
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            m_MousePoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(
                    this.Location.X + e.X - m_MousePoint.X,
                    this.Location.Y + e.Y - m_MousePoint.Y
                );
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion


        #region 栅格转点（标准 GDB 数据库导入逻辑 - 防内存溢出与死锁版）
        private void btnRasToPoint_Click(object sender, EventArgs e)
        {
            // 1. 选择输入栅格文件
            GDBOpenFileDialog openDlg = new GDBOpenFileDialog();
            openDlg.Title = "选择输入栅格";
            openDlg.Filter = "栅格文件(*.img,*.tif,*.tiff,*.hdb)|*.img;*.tif;*.tiff;*.hdb|栅格数据集|rasdsp";
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() != DialogResult.OK)
            {
                openDlg.Dispose();
                return;
            }

            RasterDataSet raster = new RasterDataSet();
            if (!raster.Open(openDlg.FileName, RasAccessType.RasAccessType_ReadOnly))
            {
                MessageBox.Show("栅格打开失败！请确认文件未被其他程序占用。", "错误");
                openDlg.Dispose();
                return;
            }

            // 2. 选择 GDB 输出位置
            GDBSelectFolderDialog gdbDialog = new GDBSelectFolderDialog();
            gdbDialog.Title = "选择保存 GDB 位置/要素集";
            
            if (gdbDialog.ShowDialog(this) != DialogResult.OK)
            {
                raster.Close();
                openDlg.Dispose();
                gdbDialog.Dispose();
                return;
            }

            string fullSelectedPath = gdbDialog.SelectedPath;
            if (string.IsNullOrEmpty(fullSelectedPath) || !fullSelectedPath.StartsWith("gdbp://", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("未获取有效 GDB 路径。", "错误");
                raster.Close();
                return;
            }

            // 3. 解析并组装最终目标 GDB 路径
            Uri uri = new Uri(fullSelectedPath);
            string host = uri.Host;
            string path = uri.AbsolutePath.TrimEnd('/');
            string outClsName = "栅格转点_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string finalGdbUrl = $"gdbp://{host}{path}/sfcls/{outClsName}";

            // 4. 读取栅格图像地图参数
            Rect mapRect = raster.GetMapRange();
            if (mapRect == null)
            {
                MessageBox.Show("未能获取到栅格地图范围 (缺少空间参照)。", "数据异常");
                raster.Close();
                return;
            }

            int rowCount = raster.Height;
            int colCount = raster.Width;
            double xMin = mapRect.XMin;
            double yMax = mapRect.YMax;
            double cellX = raster.XResolution;
            double cellY = raster.YResolution;
            double noDataVal = raster.GetNullVal();

            // 检测波段有效性与索引方向 (尝试波段1，若失败则尝试波段0)
            int targetBand = 1;
            
            // 【核心修复1】：必须提前分配内存！防止读取 TIFF 时底层COM不自动分配导致返回 null
            double[,] testPixel = new double[1, 1]; 
            raster.GetPixels(1, 0, 0, 0, 1, 1, ref testPixel);
            
            if (testPixel == null)
            {
                testPixel = new double[1, 1]; // 再次分配
                raster.GetPixels(0, 0, 0, 0, 1, 1, ref testPixel);
                if (testPixel != null) targetBand = 0;
            }

            if (testPixel == null)
            {
                MessageBox.Show("未能读取 TIFF 像素数据核心区！\n这通常是因为该 TIFF 文件带有 LZW 等特殊压缩，MapGIS底层接口无法直接解压为数值阵列。\n\n【解决办法】\n请在 MapGIS 软件中先将该 TIFF 导出/转换为原生的 .hdb 格式，再使用本工具！", "格式限制");
                raster.Close();
                return;
            }

            SFeatureCls pointCls = null;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                pointCls = new SFeatureCls();
                if (pointCls.Create(finalGdbUrl, GeomType.Pnt) <= 0)
                {
                    throw new Exception($"创建点要素类失败！\n路径: {finalGdbUrl}");
                }

                // 定义字段结构
                Fields pntFields = new Fields();
                pntFields.AppendField(new Field { FieldName = "X坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                pntFields.AppendField(new Field { FieldName = "Y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                pntFields.AppendField(new Field { FieldName = "Z像素值", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });

                if (pointCls.UpdateFields(pntFields) <= 0)
                {
                    throw new Exception("更新要素类字段结构失败。");
                }

                pointCls.BeginBatch(BatchType.Append);
                MapGIS.GeoObjects.Att.Record pntRecord = new MapGIS.GeoObjects.Att.Record { Fields = pntFields };

                int successCount = 0;

                // 【核心修改】采取逐行读取模式（Row-by-Row），防止超大TIFF文件一次性读取时导致 OutOfMemory 从而返回 Null
                for (int row = 0; row < rowCount; row++)
                {
                    // 【核心修复2】：每一行也要提前分配好准确的数组大小！(1行 x colCount列)
                    double[,] rowData = new double[1, colCount];
                    
                    // （波段, 金字塔0, X偏移0, Y偏移目前行, 宽度全列, 高度仅1行）
                    raster.GetPixels(targetBand, 0, 0, row, colCount, 1, ref rowData);
                    
                    if (rowData == null) continue; // 如果单行受损或无效，跳过不至于崩溃

                    for (int col = 0; col < colCount; col++)
                    {
                        // 始终读取索引 0，因为 rowData 高度只有 1 行
                        double cellVal = rowData[0, col];
                        if (cellVal == noDataVal || double.IsNaN(cellVal)) continue;

                        double x = xMin + (col + 0.5) * cellX;
                        double y = yMax - (row + 0.5) * cellY;

                        MapGIS.GeoObjects.Geometry.Dot3D pnt3D = new MapGIS.GeoObjects.Geometry.Dot3D(x, y, cellVal);
                        MapGIS.GeoObjects.Geometry.GeoPoints currentPnts = new MapGIS.GeoObjects.Geometry.GeoPoints();
                        currentPnts.Append(pnt3D);

                        pntRecord["X坐标"] = x;
                        pntRecord["Y坐标"] = y;
                        pntRecord["Z像素值"] = cellVal;

                        if (pointCls.Append(currentPnts, pntRecord, null) > 0)
                        {
                            successCount++;
                        }
                    }
                }

                pointCls.EndBatch();
                MessageBox.Show($"✅ 栅格转点 GDB 导入成功！\n生成点要素：{successCount} 个\n保存位置：{finalGdbUrl}", "完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理异常:\n{ex.Message}", "错误");
            }
            finally
            {
                // 释放所有底层COM资源，防止内存锁定
                if (pointCls != null)
                {
                    try
                    {
                        if (pointCls.HasOpen()) pointCls.Close();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pointCls);
                    }
                    catch { }
                }
                
                raster.Close();
                openDlg.Dispose();
                gdbDialog.Dispose();
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        #region 1. 栅格转矢量（标准 GDB 数据库导入逻辑）
        private void btnRasToVec_Click(object sender, EventArgs e)
        {
            // ===================== 1. 选择输入栅格 =====================
            GDBOpenFileDialog openDlg = new GDBOpenFileDialog();
            openDlg.Title = "选择输入栅格（本地/img/tif/tiff/hdb 或 GDB栅格）";
            openDlg.Filter = "栅格文件(*.img,*.tif,*.tiff,*.hdb)|*.img;*.tif;*.tiff;*.hdb|栅格数据集|rasdsp";
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() != DialogResult.OK)
            {
                openDlg.Dispose();
                return;
            }

            // ===================== 2. 打开输入栅格 =====================
            RasterDataSet raster = new RasterDataSet();
            bool isRasterOpen = raster.Open(openDlg.FileName, RasAccessType.RasAccessType_ReadOnly);
            if (!isRasterOpen)
            {
                MessageBox.Show("栅格文件打开失败！", "错误");
                openDlg.Dispose();
                return;
            }

            // ===================== 3. 选择输出GDB数据库位置 =====================
            GDBSelectFolderDialog gdbDialog = new GDBSelectFolderDialog();
            gdbDialog.Title = "选择保存 GDB 位置/要素集";

            if (gdbDialog.ShowDialog(this) != DialogResult.OK)
            {
                raster.Close();
                openDlg.Dispose();
                gdbDialog.Dispose();
                return;
            }

            string fullSelectedPath = gdbDialog.SelectedPath;
            if (string.IsNullOrEmpty(fullSelectedPath) || !fullSelectedPath.StartsWith("gdbp://", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("未获取有效 GDB 路径。", "错误");
                raster.Close();
                return;
            }

            // ===================== 4. 解析路径并创建出要素类 =====================
            Uri uri = new Uri(fullSelectedPath);
            string host = uri.Host;
            string path = uri.AbsolutePath.TrimEnd('/');
            string outClsName = "栅格转矢量_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fullOutUrl = $"gdbp://{host}{path}/sfcls/{outClsName}";

            SFeatureCls outCls = new SFeatureCls();
            int createResult = outCls.Create(fullOutUrl, MapGIS.GeoObjects.GeomType.Reg);

            if (createResult <= 0)
            {
                MessageBox.Show($"❌ 创建要素类最终失败！\n路径：{fullOutUrl}", "错误");
                outCls.Dispose();
                raster.Close();
                openDlg.Dispose();
                gdbDialog.Dispose();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            // ===================== 5. 执行转换 =====================
            // 注意：API 最后一个参数 null 代表不使用后台进度条/控制返回
            int convertResult = m_rasTrans.ConvertRasDataSetToVector(
                raster, 1, 10, true, 0.01, outCls, null
            );

            // ===================== 6. 释放资源 =====================
            outCls.Close();
            outCls.Dispose();
            raster.Close();
            openDlg.Dispose();
            gdbDialog.Dispose();
            this.Cursor = Cursors.Default;

            // ===================== 7. 结果 =====================
            if (convertResult > 0)
                MessageBox.Show($"🎉 转换成功！\n输出路径：{fullOutUrl}", "完成");
            else
                MessageBox.Show("❌ 转换失败！", "失败");
        }
        #endregion

        #region 2. 栅格转网格 按钮（支持本地+GDB数据库栅格）
        private void btnRasToGrid_Click(object sender, EventArgs e)
        {
            GDBOpenFileDialog gDBOpenFileDialog = new GDBOpenFileDialog();
            gDBOpenFileDialog.Title = "选择栅格文件";
            // ✅ 仅修改这一行：添加 tiff 后缀支持与 MapGIS数据库栅格关键字 rasdsp
            gDBOpenFileDialog.Filter = "栅格文件(*.img,*.tif,*.tiff,*.hdb)|*.img;*.tif;*.tiff;*.hdb|栅格数据集|rasdsp";
            gDBOpenFileDialog.Multiselect = false;

            if (gDBOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Surfer Grid数据|*.grd";
                    sfd.Title = "保存网格文件";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        int result = m_rasTrans.ConvertRasDataSetToGrd(gDBOpenFileDialog.FileName, sfd.FileName);
                        MessageBox.Show(result > 0 ? "栅格转网格 转换成功！" : "转换失败！");
                    }
                }
            }

            gDBOpenFileDialog.Close();
            gDBOpenFileDialog.Dispose();
        }
        #endregion

        #region 3. 矢量转栅格 按钮（支持本地+GDB数据库矢量）
        private void btnVecToRas_Click(object sender, EventArgs e)
        {
            // 标准文件选择弹窗（和你要求的格式完全一致）
            GDBOpenFileDialog gDBOpenFileDialog = new GDBOpenFileDialog();
            gDBOpenFileDialog.Title = "选择矢量文件（矢量转栅格）";
            // ✅ 仅修改这一行：添加MapGIS数据库矢量关键字 sfclsp
            gDBOpenFileDialog.Filter = "矢量文件(*.wp,*.wl,*.wt)|*.wp;*.wl;*.wt|简单要素类|sfclsp";
            gDBOpenFileDialog.Multiselect = false;

            if (gDBOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 修复核心：MapGIS原生打开矢量文件，删除错误的DataAccessMode参数！
                SFeatureCls sfc = new SFeatureCls();
                // ✅ 正确写法：仅传文件路径即可（MapGIS官方标准用法）
                if (sfc.Open(gDBOpenFileDialog.FileName))
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "栅格数据集(*.hdb)|*.hdb";
                        sfd.Title = "保存栅格文件";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            // ✅ 严格按照你提供的API文档赋值参数
                            VecToRasPARAM_Stru vrParam = new VecToRasPARAM_Stru();
                            vrParam.XStep = 10.0;   // 像元宽度（文档里的X步长）
                            vrParam.YStep = 10.0;   // 像元高度（文档里的Y步长）
                            vrParam.CellType = 0;   // 默认栅格类型（0=标准类型，兼容所有版本）
                            vrParam.BkCode = 0;     // 默认背景值

                            // ✅ 100%匹配你图片里的API参数顺序，无任何类型错误
                            int result = m_rasTrans.ConvertVectorToRasDataSet(
                                sfc,        // 1. 矢量对象
                                vrParam,    // 2. 转换参数
                                false,      // 3. 不使用查找表
                                null,       // 4. 查找表填充
                                null,       // 5. 查找表列表（修复CS1503）
                                sfd.FileName// 6. 输出路径
                            );

                            MessageBox.Show(result > 0 ? "✅ 矢量转栅格 转换完成！" : "❌ 转换失败！");
                        }
                    }
                    // 关闭释放
                    sfc.Close();
                    sfc.Dispose();
                }
                else
                {
                    MessageBox.Show("❌ 矢量文件打开失败！");
                }
            }

            gDBOpenFileDialog.Close();
            gDBOpenFileDialog.Dispose();
        }
        #endregion
    }
}