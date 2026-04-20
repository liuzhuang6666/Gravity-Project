using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GISControl;
using MapGIS.RasAnalysis;
using System;
using System.IO;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public partial class Form_RasterClipByCoords : Form
    {
        private MapControl _mapControl;
        private RasterLayer _rasterLayer;
        private Rect _rasMaxRect; // 栅格真实的最大外包盒
        private static int _clipCounter = 1;

        public Form_RasterClipByCoords(MapControl mapControl, RasterLayer rasterLayer)
        {
            InitializeComponent();
            _mapControl = mapControl;
            _rasterLayer = rasterLayer;

            // 1. 获取并初始化原栅格的极值坐标
            RasterDataSet rasterDs = _rasterLayer.GetData() as RasterDataSet;
            if (rasterDs != null)
            {
                _rasMaxRect = rasterDs.GetMapRange();
                // 把最大允许的坐标默认填到文本框，防止用户两眼一黑不知怎么填
                txtXMin.Text = _rasMaxRect.XMin.ToString("F4");
                txtYMin.Text = _rasMaxRect.YMin.ToString("F4");
                txtXMax.Text = _rasMaxRect.XMax.ToString("F4");
                txtYMax.Text = _rasMaxRect.YMax.ToString("F4");
                
                lblTip.Text = $"当前栅格极限范围：\nX: [{_rasMaxRect.XMin:F2} ~ {_rasMaxRect.XMax:F2}]\nY: [{_rasMaxRect.YMin:F2} ~ {_rasMaxRect.YMax:F2}]";
            }

            // 2. 选择保存路径
            txtOutputPath.ButtonClick += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "TIFF图像(*.tif)|*.tif|IMG图像(*.img)|*.img|HDB栅格(*.hdb)|*.hdb";
                    if (sfd.ShowDialog() == DialogResult.OK)
                        txtOutputPath.Text = sfd.FileName;
                }
            };
        }

        private void btnClip_Click(object sender, EventArgs e)
        {
            // 校验路径
            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("请选择输出路径！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 解析坐标
            if (!double.TryParse(txtXMin.Text, out double inXMin) || !double.TryParse(txtYMin.Text, out double inYMin) ||
                !double.TryParse(txtXMax.Text, out double inXMax) || !double.TryParse(txtYMax.Text, out double inYMax))
            {
                MessageBox.Show("输入的坐标格式有误，请输入纯数字！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 逻辑校验
            if (inXMin >= inXMax || inYMin >= inYMax)
            {
                MessageBox.Show("坐标包围盒无效：最大值必须严格大于最小值！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 越界强监督拦截
            if (inXMin < _rasMaxRect.XMin || inXMax > _rasMaxRect.XMax ||
                inYMin < _rasMaxRect.YMin || inYMax > _rasMaxRect.YMax)
            {
                MessageBox.Show("裁剪坐标超出了当前栅格的最大数据范围！\n请参照上方提示的极值进行修正。", "越界错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 构造几何多边形并执行裁剪
            ExecuteClip(inXMin, inYMin, inXMax, inYMax);
        }

        private void ExecuteClip(double xMin, double yMin, double xMax, double yMax)
        {
            RasterDataSet rasterDs = _rasterLayer.GetData() as RasterDataSet;
            if (rasterDs == null) return;

            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents();

            SFeatureCls tempPolyCls = null;
            try
            {
                RasImgSubset clipTool = new RasImgSubset();
                int bandCount = rasterDs.BandCount;
                int[] bands = new int[bandCount];
                for (int i = 0; i < bandCount; i++) bands[i] = i + 1;

                clipTool.SetData(rasterDs, bands);
                clipTool.SetClipType(0); // 0 为内裁剪

                // 1. 先构造出多边形的边界环（变长线）
            GeoVarLine ring = new GeoVarLine();
            ring.Append(new Dot3D(xMin, yMin, 0));
            ring.Append(new Dot3D(xMin, yMax, 0));
            ring.Append(new Dot3D(xMax, yMax, 0));
            ring.Append(new Dot3D(xMax, yMin, 0));
            ring.Append(new Dot3D(xMin, yMin, 0)); // 闭合回起点

            // 2. 将边界环放入线集合
            GeoLines lines = new GeoLines();
            lines.Append(ring);

            // 3. 将线集合放入多边形中
            GeoPolygon targetPoly = new GeoPolygon();
            targetPoly.Append(lines);

                tempPolyCls = CreateTempRegion(targetPoly);
                int result = clipTool.RsClipImageBySFCls(tempPolyCls, 0, 3, txtOutputPath.Text);

                if (result > 0 || File.Exists(txtOutputPath.Text))
                {
                    RasterLayer resLayer = new RasterLayer();
                    resLayer.URL = txtOutputPath.Text;

                    if (resLayer.ConnectData())
                    {
                        resLayer.Name = "Coord_ClipArea_" + _clipCounter++;
                        _mapControl.ActiveMap.Append(resLayer);
                        _mapControl.Refresh();
                        _mapControl.TryRefresh();
                    }
                    MessageBox.Show("依据坐标裁剪成功并已加载到底图！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("裁剪计算失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"底层引擎计算异常：\n{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (tempPolyCls != null)
                {
                    if (tempPolyCls.HasOpen()) tempPolyCls.Close();
                    tempPolyCls.Dispose();
                }
                Cursor.Current = Cursors.Default;
            }
        }

        private SFeatureCls CreateTempRegion(GeoPolygon polygon)
        {
            DataBase tempDb = DataBase.OpenTempDB();
            SFeatureCls sfCls = new SFeatureCls(tempDb);
            string uniqueName = "CoordMask_" + DateTime.Now.ToString("HHmmss_fff");
            sfCls.Create(uniqueName, GeomType.Reg, 0, 0, null);
            sfCls.Append(polygon, null, null);
            return sfCls;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}