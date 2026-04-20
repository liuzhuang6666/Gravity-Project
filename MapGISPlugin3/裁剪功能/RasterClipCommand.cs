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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    /// <summary>
    /// 栅格交互式裁剪命令类：点击即可在图上画多边形裁剪
    /// </summary>
    public class RasterClipCommand : ISingleMenuItem, ICommand, IMenuCommand
    {
        private IApplication app;
        private IWorkSpace ws = null;

        #region ICommand 成员实现
        public Bitmap Bitmap => null; 
        public string Caption => "交互式栅格裁剪";
        public string Category => "地球物理数据处理";
        public bool Checked => false;
        public bool Enabled => true;
        public string Message => "直接在地图上绘制多边形，对可见的栅格图层进行裁剪";
        public string Name => "RasterClipCommand";
        public string Tooltip => "交互裁剪栅格";

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

            // 激活定制画图裁剪工具（此时不再弹出窗口，直接在图上画）
            RasterClipInteractiveTool clipTool = new RasterClipInteractiveTool(mapControl);
            mapControl.SetBasTool((GISBasTool)clipTool);
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
    /// 自定义的地图交互工具：负责画多边形 + 执行裁剪逻辑 + 自动命名 + 清除红线
    /// </summary>
    public class RasterClipInteractiveTool : GISBasTool
    {
        private MapControl _mapControl;
        private IAPolygon _iaPolygon;
        
        // 静态计数器，用于生成依次递增的裁剪名称
        private static int _clipCounter = 1;

        public RasterClipInteractiveTool(MapControl mapControl)
        {
            _mapControl = mapControl;

            // 初始化 MapGIS 底层的多边形交互工具
            _iaPolygon = new IAPolygon(mapControl, null, false, false, mapControl.Transformation, null, true);
            _iaPolygon.Finish += OnDrawFinished;
            
            base.Active += Tool_Active;
            base.Unactive += Tool_Unactive;
            base.Cancel += Tool_Cancel;
            base.PostRefresh += Tool_PostRefresh;
        }

        #region 生命周期调度 (驱动画图引擎)
        private void Tool_Active(object sender, ToolEventArgs e) { _iaPolygon.Start(); }
        private void Tool_Unactive(object sender, ToolEventArgs e) { _iaPolygon.Pause(); }
        private void Tool_Cancel(object sender, ToolEventArgs e) { _iaPolygon.Cancel(); }
        private void Tool_PostRefresh(object sender, ToolEventArgs e) { _iaPolygon.PostRefresh(); }
        #endregion

        // 转发鼠标事件到底层画多边形引擎
        public override int OnMouseDown(object sender, MouseEventArgs e) { _iaPolygon.OnMouseDown(sender, e); return 0; }
        public override int OnMouseMove(object sender, MouseEventArgs e) { _iaPolygon.OnMouseMove(sender, e); return 0; }
        public override int OnMouseUp(object sender, MouseEventArgs e) { _iaPolygon.OnMouseUp(sender, e); return 0; }
        public override int OnKeyDown(object sender, KeyEventArgs e) { _iaPolygon.OnKeyDown(sender, e); return 0; }

        /// <summary>
        /// 鼠标右键结束绘制后触发
        /// </summary>
        private void OnDrawFinished(object sender, ToolEventArgs e)
        {
            if (e.Geom is GeoPolygon polygon)
            {
                // ==========================================
                // 1. 【解决红线不消失】强制取消绘画引擎并刷新视图
                // ==========================================
                _iaPolygon.Cancel();
                _mapControl.SetBasTool(null);
                _mapControl.Refresh();

                // 2. 寻找参与裁剪的可见栅格图层
                RasterLayer targetRasterLayer = GetFirstVisibleRasterLayer();
                if (targetRasterLayer == null)
                {
                    MessageBox.Show("裁剪中止：未在地图中找到可见的栅格图层！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. 询问用户将裁剪结果存到哪里
                string outPath = GetSavePath();
                if (string.IsNullOrEmpty(outPath)) return; // 用户取消保存则中止

                // 4. 执行实际的裁剪操作
                ExecuteClip(targetRasterLayer, polygon, outPath);
            }
        }

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
        /// 核心：执行 MapGIS 栅格裁切算法
        /// </summary>
        private void ExecuteClip(RasterLayer rasterLayer, GeoPolygon polygon, string outputPath)
        {
            RasterDataSet rasterDs = rasterLayer.GetData() as RasterDataSet;
            if (rasterDs == null) return;

            // 【越界拦截】 获取栅格原始范围与绘制边界的判断
            Rect rasRect = rasterDs.GetMapRange();
            Rect polyRect = polygon.CalRect();

            if (polyRect.XMin > rasRect.XMax || polyRect.XMax < rasRect.XMin || 
                polyRect.YMin > rasRect.YMax || polyRect.YMax < rasRect.YMin)
            {
                MessageBox.Show("您绘制的区域完全在栅格图像外部，无法进行裁剪！", "范围错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            System.Windows.Forms.Application.DoEvents();

            SFeatureCls tempPolyCls = null;
            try
            {
                RasImgSubset clipTool = new RasImgSubset();
                
                int bandCount = rasterDs.BandCount;
                int[] bands = new int[bandCount];
                for (int i = 0; i < bandCount; i++) bands[i] = i + 1;

                clipTool.SetData(rasterDs, bands);
                clipTool.SetClipType(0); // 0 为内裁剪

                // ==========================================
                // 【解决第二次报错】根据毫秒时间戳生成不同的临时名
                // ==========================================
                tempPolyCls = CreateTempRegion(polygon);

                // 执行基于区要素类的图片裁剪 (参数 3 代表保留背景)
                int result = clipTool.RsClipImageBySFCls(tempPolyCls, 0, 3, outputPath);

                if (result > 0 || File.Exists(outputPath))
                {
                    RasterLayer resLayer = new RasterLayer();
                    resLayer.URL = outputPath;
                    if (resLayer.ConnectData())
                    {
                        // ==========================================
                        // 【解决图层名为空】挂载前强行赋值编号名称
                        // ==========================================
                        resLayer.Name = "ClipArea_" + _clipCounter++;

                        _mapControl.ActiveMap.Append(resLayer);
                        
                        _mapControl.Refresh();
                        _mapControl.TryRefresh();
                    }
                    MessageBox.Show("🎉 裁剪成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("裁剪失败！底层算法无法处理当前的越界图形。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"裁剪发生异常：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // 动态加入时间戳后缀，绝不会在第二次裁剪时死锁崩溃！
            string uniqueName = "ClipMask_" + DateTime.Now.ToString("HHmmss_fff");
            sfCls.Create(uniqueName, GeomType.Reg, 0, 0, null);
            sfCls.Append(polygon, null, null);
            return sfCls;
        }
    }
}