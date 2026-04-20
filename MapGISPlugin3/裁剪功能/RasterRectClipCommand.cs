using DevExpress.XtraTreeList.Nodes;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GISControl;
using MapGIS.GISControl.IATool;
using MapGIS.PluginEngine;
using MapGIS.RasAnalysis;
using MapGIS.WorkSpaceEngine;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace MapGISPlugin3
{
    /// <summary>
    /// 栅格矩形框选裁剪命令类：点击即可在图上画矩形框裁剪
    /// </summary>
    public class RasterRectClipCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员实现
        public Bitmap Bitmap => null;
        public string Caption => "矩形框选栅格裁剪";
        public string Category => "地球物理数据处理";
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => "直接在地图上拖拽矩形框，对可见的栅格图层进行裁剪";
        public string Name => "RasterRectClipCommand";
        public string Tooltip => "矩形交互裁剪栅格";

        public void OnClick()
        {
            if (this.app == null || this.app.Document == null)
            {
                MessageBox.Show("应用未初始化或未打开任何文档工程！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MapControl mapControl = null;
            if (this.app.ActiveContentsView is IMapContentsView mapView)
            {
                mapControl = mapView.MapControl;
            }

            if (mapControl == null || mapControl.ActiveMap == null)
            {
                MessageBox.Show("未找到激活的二维地图视图！请确保已在工作空间内打开栅格底图。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 创建裁剪工具
            RasterRectClipInteractiveTool clipTool = new RasterRectClipInteractiveTool(mapControl);
            mapControl.SetBasTool(clipTool);
        }

        public void OnCreate(IApplication hook) { this.app = hook; }
        public bool BeginGroup => false;
        public bool Visible => true;
        public IApplication App { get => app; set => app = value; }
        public void OnClick(DocumentItem item) { OnClick(); }
        public void OnCreate(IWorkSpace ws) { this.ws = ws; }
        public void OnClick(object sender, params TreeListNode[] items) { OnClick(); }
        public bool IsVisible(object sender, params TreeListNode[] items) => true;
        public bool IsEditable(object sender, params TreeListNode[] items) => true;
        #endregion
    }

    /// <summary>
    /// 自定义的地图交互工具：负责画矩形 + 执行裁剪逻辑
    /// </summary>
    public class RasterRectClipInteractiveTool : GISBasTool
    {
        private MapControl _mapControl;
        private IARect _iaRectTool;
        private static int _clipCounter = 1;
        private bool isEscFlag = false;

        public RasterRectClipInteractiveTool(MapControl mapControl)
        {
            _mapControl = mapControl;

            // 完全仿照 StandardRectTool 初始化参数
            _iaRectTool = new IARect(mapControl, null, false, false, mapControl.Transformation, null, false);
            _iaRectTool.Finish += OnDrawFinished;

            base.Active += Tool_Active;
            base.Unactive += Tool_Unactive;
            base.Cancel += Tool_Cancel;
            base.PostRefresh += Tool_PostRefresh;
        }

        #region 工具生命周期
        private void Tool_Active(object sender, ToolEventArgs e) { _iaRectTool.Start(); }
        private void Tool_Unactive(object sender, ToolEventArgs e) { _iaRectTool.Pause(); }
        private void Tool_Cancel(object sender, ToolEventArgs e) { _iaRectTool.Cancel(); }
        private void Tool_PostRefresh(object sender, ToolEventArgs e) { _iaRectTool.PostRefresh(); }
        #endregion

        public override int OnMouseDown(object sender, MouseEventArgs e) 
        { 
            _iaRectTool.OnMouseDown(sender, e); 
            return 0; 
        }

        public override int OnMouseMove(object sender, MouseEventArgs e) 
        { 
            _iaRectTool.OnMouseMove(sender, e); 
            return 0; 
        }

        public override int OnMouseUp(object sender, MouseEventArgs e) 
        { 
            _iaRectTool.OnMouseUp(sender, e); 
            
            // 仿写：使用鼠标右键清空并退出工具
            if (e.Button == MouseButtons.Right && !isEscFlag)
            {
                _mapControl.SetBasTool(null);
            }
            return 0; 
        }

        public override int OnKeyDown(object sender, KeyEventArgs e) 
        { 
            _iaRectTool.OnKeyDown(sender, e); 
            return 0; 
        }

        /// <summary>
        /// 矩形绘制完成触发
        /// </summary>
        private void OnDrawFinished(object sender, ToolEventArgs e)
        {
            if (!isEscFlag && e.Geom != null && e.Geom is GeoRect geoRect)
            {
                // ★ 关键修复代码：按照底层逻辑，必须先散列点集才能获得真正的面积
                geoRect.DisperseToDots(0.2);
                Rect boundingBox = geoRect.CalRect();

                // 规范化坐标（处理左上至右下的反向拖拽）
                double xMin = Math.Min(boundingBox.XMin, boundingBox.XMax);
                double xMax = Math.Max(boundingBox.XMin, boundingBox.XMax);
                double yMin = Math.Min(boundingBox.YMin, boundingBox.YMax);
                double yMax = Math.Max(boundingBox.YMin, boundingBox.YMax);

                // 取小容差进行极小框选判断（防止地理坐标系下框选范围较小的情况被误拦截）
                if (Math.Abs(xMax - xMin) < 1e-8 || Math.Abs(yMax - yMin) < 1e-8)
                {
                    MessageBox.Show("框选无效！您可能只是点击了鼠标，请按走拖动来框选范围。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 寻底图并执行
                RasterLayer targetRasterLayer = GetFirstVisibleRasterLayer();
                if (targetRasterLayer == null)
                {
                    MessageBox.Show("未在地图中找到可见的栅格图层！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                GeoPolygon rectPolygon = ConvertRectToPolygon(xMin, yMin, xMax, yMax);
                string outPath = GetSavePath();
                if (string.IsNullOrEmpty(outPath)) return;

                // 强制绘图结束，清空画面虚影框
                _iaRectTool.Cancel();
                _mapControl.Refresh();

                ExecuteClip(targetRasterLayer, rectPolygon, outPath);

                // 裁剪完成后退出命令
                _mapControl.SetBasTool(null);
            }
        }

        /// <summary>
        /// 矩形转多边形
        /// </summary>
        private GeoPolygon ConvertRectToPolygon(double xMin, double yMin, double xMax, double yMax)
        {
            GeoVarLine ring = new GeoVarLine();
            ring.Append(new Dot3D(xMin, yMin, 0));
            ring.Append(new Dot3D(xMin, yMax, 0));
            ring.Append(new Dot3D(xMax, yMax, 0));
            ring.Append(new Dot3D(xMax, yMin, 0));
            ring.Append(new Dot3D(xMin, yMin, 0));

            GeoLines lines = new GeoLines();
            lines.Append(ring);

            GeoPolygon polygon = new GeoPolygon();
            polygon.Append(lines);
            return polygon;
        }

        /// <summary>
        /// 获取第一个可见栅格图层
        /// </summary>
        private RasterLayer GetFirstVisibleRasterLayer()
        {
            for (int i = 0; i < _mapControl.ActiveMap.LayerCount; i++)
            {
                MapLayer layer = _mapControl.ActiveMap.get_Layer(i);
                if (layer is RasterLayer && layer.State != LayerState.UnVisible)
                {
                    return layer as RasterLayer;
                }
            }
            return null;
        }

        /// <summary>
        /// 保存文件对话框
        /// </summary>
        private string GetSavePath()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "保存裁剪结果";
                sfd.Filter = "TIFF 图像(*.tif)|*.tif|IMG 图像(*.img)|*.img|MapGIS栅格(*.hdb)|*.hdb";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    return sfd.FileName;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 执行栅格裁剪
        /// </summary>
        private void ExecuteClip(RasterLayer rasterLayer, GeoPolygon polygon, string outputPath)
        {
            RasterDataSet rasterDs = rasterLayer.GetData() as RasterDataSet;
            if (rasterDs == null) return;

            // 范围异常拦截
            Rect rasRect = rasterDs.GetMapRange();
            Rect polyRect = polygon.CalRect();

            if (polyRect.XMin > rasRect.XMax || polyRect.XMax < rasRect.XMin ||
                polyRect.YMin > rasRect.YMax || polyRect.YMax < rasRect.YMin)
            {
                MessageBox.Show("裁剪范围完全在图像外部！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            System.Windows.Forms.Application.DoEvents();

            SFeatureCls tempPolyCls = null;
            try
            {
                RasImgSubset clipTool = new RasImgSubset();
                int[] bands = new int[rasterDs.BandCount];
                for (int i = 0; i < rasterDs.BandCount; i++) bands[i] = i + 1;

                clipTool.SetData(rasterDs, bands);
                clipTool.SetClipType(0);
                tempPolyCls = CreateTempRegion(polygon);

                int result = clipTool.RsClipImageBySFCls(tempPolyCls, 0, 3, outputPath);

                if (result > 0 || File.Exists(outputPath))
                {
                    // 添加结果图层
                    RasterLayer resLayer = new RasterLayer();
                    resLayer.URL = outputPath;
                    if (resLayer.ConnectData())
                    {
                        resLayer.Name = "RectClip_" + _clipCounter++;
                        _mapControl.ActiveMap.Append(resLayer);
                        _mapControl.Refresh();
                    }
                    MessageBox.Show("🎉 矩形裁剪成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("裁剪失败！请检查是否存在越界或非法框选区域。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"裁剪发生异常：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 释放资源
                if (tempPolyCls != null)
                {
                    if (tempPolyCls.HasOpen()) tempPolyCls.Close();
                    tempPolyCls.Dispose();
                }
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// 创建临时面要素类
        /// </summary>
        private SFeatureCls CreateTempRegion(GeoPolygon polygon)
        {
            DataBase tempDb = DataBase.OpenTempDB();
            SFeatureCls sfCls = new SFeatureCls(tempDb);
            string uniqueName = "RectClip_" + DateTime.Now.ToString("HHmmss_fff");
            sfCls.Create(uniqueName, GeomType.Reg, 0, 0, null);
            sfCls.Append(polygon, null, null);
            return sfCls;
        }
    }
}