// ContourVisualizer.cs
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Info;
using MapGIS.RasAnalysis;
using MapGIS.RasCommonObj;
using System;
using System.Collections.Generic;
using MapGIS.GeoObjects.Geometry;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.UI.Controls;
namespace MapGISPlugin3
{
    public class ContourVisualizer : IDisposable
    {
        // --- 模仿 Form1.cs 的成员变量 ---
        private RasterLayer _sourceLayer;
        private Map _targetMap;
        private IApplication _hook;
        private double _interval;
        private Rect _rect = null;
        private double _minVal = 0;
        private double _maxVal = 0;
        private ContourParamStrcT_Stru _contourParamStrcT = null;
        private ContourNoteParam_Stru _contourNoteParam = null;
        private ZVelStrcT_Stru[] _zVelArray = null;
        // 【关键】这些COM对象现在是成员变量，它们的生命周期被延长了
        private DataBase _db;
        private SFeatureCls _sfclsReg;
        private SFeatureCls _sfclsLin;
        private AnnotationCls _annCls;
        public ContourVisualizer(RasterLayer sourceLayer, Map targetMap, double interval, IApplication hook)
        {
            _sourceLayer = sourceLayer;
            _targetMap = targetMap;
            _interval = interval;
            _hook = hook;
        }
        // --- 模仿 Form1.cs 的 Init() 方法 ---
        private bool InitializeParameters()
        {
            RasterDataSet rasDataSet = _sourceLayer.GetData() as RasterDataSet;
            if (rasDataSet == null) return false;
            _rect = rasDataSet.GetMapRange();
            if (_rect == null) return false;
            _minVal = rasDataSet.GetRasterBand(1).MinValue;
            _maxVal = rasDataSet.GetRasterBand(1).MaxValue;
            if (_minVal.CompareTo(_maxVal) == 0) return false;
            // 1. 准备等值线参数
            _contourParamStrcT = new ContourParamStrcT_Stru { bMakeReg = true, bMapNote = true };
            _contourNoteParam = new ContourNoteParam_Stru();
            float length = (float)Math.Min(_rect.XMax - _rect.XMin, _rect.YMax - _rect.YMin);
            _contourNoteParam.MinDist = length / 200;
            _contourNoteParam.LabelFnt.FixSize = length / 250 > 0 ? length / 250 : 1.0f;
            _contourParamStrcT.pContourNoteParam = _contourNoteParam;
            // 2. 准备Z值和颜色
            List<double> zValues = new List<double>();
            double currentZ = Math.Floor(_minVal / _interval) * _interval;
            while (currentZ <= _maxVal)
            {
                if (currentZ >= _minVal) zValues.Add(currentZ);
                currentZ += _interval;
            }
            if (zValues.Count == 0 && _maxVal > _minVal) zValues.Add(_minVal);
            _zVelArray = new ZVelStrcT_Stru[zValues.Count];
            int[] colors = { 601, 603, 498, 500, 436, 408, 391, 233, 190, 184, 154, 122, 106, 33, 31, 127, 391, 128, 392, 393, 136, 149, 150, 442, 443, 186, 444, 179, 180, 445, 189, 190 };
            for (int i = 0; i < zValues.Count; i++)
            {
                _zVelArray[i] = new ZVelStrcT_Stru
                {
                    fZdem = zValues[i],
                    linf = new LinInfo { LinStyID = 1, OutClr = new[] { 1, 4, 3 }, OutPenW = new[] { 0.05f, 0.05f, 0.05f } },
                    rinf = new RegInfo { FillClr = colors[i % colors.Length], Ovprnt = true },
                    mskOn = (sbyte)(i % 3 == 0 ? 1 : 0)
                };
            }
            _contourParamStrcT.SetZVelBuf(_zVelArray);
            return true;
        }
        // --- 模仿 Form1.cs 的 DengZhiXianKeShiHua() 方法 ---
        public bool Execute()
        {
            if (!InitializeParameters())
            {
                MessageBox.Show("初始化等值线参数失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                // 1. 打开数据库
                _db = DataBase.OpenTempDB();
                if (_db == null) throw new Exception("无法打开临时数据库。");
                // 2. 在数据库上下文中创建所有类
                string baseName = System.IO.Path.GetFileNameWithoutExtension(_sourceLayer.Name);
                string regClassName = $"{baseName}_Region_{DateTime.Now:yyyyMMdd_HHmmss}";
                string linClassName = $"{baseName}_Line_{DateTime.Now:yyyyMMdd_HHmmss}";
                string annClassName = $"{baseName}_Anno_{DateTime.Now:yyyyMMdd_HHmmss}";
                _sfclsReg = new SFeatureCls(_db);
                if (_sfclsReg.Create(regClassName, GeomType.Reg, 0, 0, null) <= 0) throw new Exception($"创建区要素类 '{regClassName}' 失败。");
                _sfclsLin = new SFeatureCls(_db);
                if (_sfclsLin.Create(linClassName, GeomType.Lin, 0, 0, null) <= 0) throw new Exception($"创建线要素类 '{linClassName}' 失败。");
                _annCls = new AnnotationCls(_db);
                if (_annCls.Create(annClassName, AnnType.Text, 0, 0, null) <= 0) throw new Exception($"创建注记类 '{annClassName}' 失败。");
                // 3. 执行追踪
                /*                RasTraceContour traceContour = new RasTraceContour(_sourceLayer.GetData() as RasterDataSet, 1);
                                traceContour.ShowProgressBar(true);
                                int result = traceContour.RsTraceContour(_contourParamStrcT, _sfclsLin, null, _sfclsReg, _annCls, 1024, false, true);
                                traceContour.Dispose();
                                if (result <= 0) throw new Exception("等值线追踪算法执行失败。");*/
                // 4. 加载图层
                VectorLayer regLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                if (regLayer.AttachData(_sfclsReg))
                {
                    regLayer.Name = regClassName;
                    regLayer.State = LayerState.Visible;
                    _targetMap.Append(regLayer);
                }
                VectorLayer linLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                if (linLayer.AttachData(_sfclsLin))
                {
                    linLayer.Name = linClassName;
                    linLayer.State = LayerState.Visible;
                    _targetMap.Append(linLayer);
                }
                VectorLayer annLayer = new VectorLayer(VectorLayerType.AnnLayer);
                if (annLayer.AttachData(_annCls))
                {
                    annLayer.Name = annClassName;
                    annLayer.State = LayerState.Visible;
                    _targetMap.Append(annLayer);
                }
                if (_hook?.ActiveContentsView is IMapContentsView mapView)
                {
                    mapView.MapControl.Refresh();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成等值线时发生错误：\n{ex.Message}", "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public void Dispose()
        {
            _db = null;
            _sfclsReg = null;
            _sfclsLin = null;
            _annCls = null;
        }
    }
}