// VisualizationHelper.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Info;
using MapGIS.PluginEngine;
using MapGIS.RasAnalysis;
using MapGIS.RasCommonObj;
using MapGIS.UI.Controls;
using MapGIS.GeoObjects.Geometry;

namespace MapGISPlugin3
{
    public class VisualizationHelper
    {
        // 从 FieldTransformForm 中复制过来的成员变量
        private Rect m_Rect = null;
        private ContourParamStrcT_Stru m_ContourParamStrcT = null;
        private int m_BandNum = 1;
        private DataTable dataTable = new DataTable();
        private double m_Min = 0;
        private double m_Max = 0;
        private ContourNoteParam_Stru m_ContourNoteParam = null;
        private DataBase m_Tempdaba;
        private SFeatureCls m_Tempsfclslin;
        private SFeatureCls m_TempsfclsSlopelin;
        private SFeatureCls m_Tempsfclsreg;
        private AnnotationCls m_tempann;
        private double m_ScaleX = 0;
        private double m_ScaleY = 0;
        private bool m_ClipLine = true;
        private double m_LfZstep = 10;
        private float m_FSlopeYEps = 0;
        private float m_Length = 0;
        private SlopLinParam_Stru m_SlopLineParam = null;
        private string m_LinClsUrl;
        private string m_RegClsUrl;
        private string m_AnnClsUrl;
        private string m_SlopeLinClsUrl;
        private int[] Imc = {601,603,498,500,436,408,391,233,190,184,154,122,106,33,31,
               127,391,128,392,393,136,149,150,442,443,186,444,179,180,445,189,190};

        public VisualizationHelper()
        {
            // 初始化DataTable结构
            this.dataTable.Columns.Add("等值线层");
            this.dataTable.Columns.Add("线层");
            this.dataTable.Columns.Add("区层");
            this.dataTable.Columns.Add("注记层");
            this.dataTable.Columns.Add("线层数据", typeof(LinInfo));
            this.dataTable.Columns.Add("区层数据", typeof(RegInfo));
        }

        /// <summary>
        /// 公共入口方法，执行完整的可视化流程
        /// </summary>
        public void PerformVisualization(RasterLayer sourceLayer, Map targetMap, IApplication hook, double contourInterval)
        {
            // 1. 根据选择的图层初始化所有参数
            InitializeParameters(sourceLayer, contourInterval);

            // 2. 生成等值线、区、注记等临时要素类
            GenerateContourLayers(sourceLayer);

            // 3. 将生成的图层添加到主地图
            string baseName = Path.GetFileNameWithoutExtension(sourceLayer.Name);
            AddLayersToMainMap(targetMap, baseName, hook);
        }

        /// <summary>
        /// 步骤1：初始化所有可视化参数
        /// </summary>
        private void InitializeParameters(RasterLayer sourceLayer, double contourInterval)
        {
            // 【修改】使用传入的间隔值，而不是硬编码的 10
            this.m_LfZstep = contourInterval;
            // 正确的检查方式：如果图层为空，或者连接数据失败，则返回
            if (sourceLayer == null || !sourceLayer.ConnectData()) return; // <-- 修正后的行

            RasterDataSet rasdataset = sourceLayer.GetData() as RasterDataSet;
            m_Rect = rasdataset.GetMapRange();
            if (m_Rect == null) return;
            m_Min = rasdataset.GetRasterBand(m_BandNum).MinValue;
            m_Max = rasdataset.GetRasterBand(m_BandNum).MaxValue;
            if (m_Min.CompareTo(m_Max) == 0) return;

            m_ScaleX = (m_Rect.XMax - m_Rect.XMin) / 1000.0;
            m_ScaleY = (m_Rect.YMax - m_Rect.YMin) / 1000.0;

            // --- 以下代码完全复制自 FieldTransformForm 的 Init() 方法 ---
            #region 初始化等值线参数
            short a = 0;
            double fMapLength = RasCommonFunction.RsGetCriteriaNumb(m_Rect.XMax - m_Rect.XMin, ref a);
            double LengthScale = (long)((m_Rect.XMax - m_Rect.XMin) / fMapLength);
            double fMapHeight = RasCommonFunction.RsGetCriteriaNumb(m_Rect.YMax - m_Rect.YMin, ref a);
            double HeightScale = (long)((m_Rect.YMax - m_Rect.YMin) / fMapHeight);
            double Scale = Math.Max(LengthScale, HeightScale);
            if (Scale.CompareTo(0) != 0)
            {
                fMapLength = (m_Rect.XMax - m_Rect.XMin) / Scale;
                fMapHeight = (m_Rect.YMax - m_Rect.YMin) / Scale;
            }
            m_FSlopeYEps = Math.Min((int)(Math.Sqrt(fMapLength * fMapHeight)) / 10.0f, 10.0f);
            m_FSlopeYEps = Math.Max(m_FSlopeYEps, 0.01f);
            fMapLength = m_Rect.XMax - m_Rect.XMin;
            fMapHeight = m_Rect.YMax - m_Rect.YMin;

            m_ContourParamStrcT = new ContourParamStrcT_Stru();
            m_ContourParamStrcT.nNotDir = 1;
            m_ContourParamStrcT.FrmWidth = fMapLength;
            m_ContourParamStrcT.FrmHeight = fMapHeight;
            m_SlopLineParam = new SlopLinParam_Stru();
            m_ContourParamStrcT.pSlopLinParm = m_SlopLineParam;
            m_ContourParamStrcT.pSlopLinParm.nSltp = 16;
            m_ContourParamStrcT.pSlopLinParm.nSubSltp = 0;
            m_ContourParamStrcT.pSlopLinParm.fyScal = m_FSlopeYEps;
            m_ContourParamStrcT.pSlopLinParm.fxScal = 0.2f * m_FSlopeYEps;
            m_ClipLine = true;

            m_ContourNoteParam = new ContourNoteParam_Stru();
            m_Length = (float)Math.Min(fMapLength, fMapHeight);
            m_ContourNoteParam.MinDist = m_Length / 200;
            m_ContourNoteParam.MaxLayer = 1024;
            m_ContourNoteParam.LabelFmt.LogFlag = 0;
            m_ContourNoteParam.LabelFnt.FixSize = m_Length / 150;

            double dk, zdat;
            int k;
            do
            {
                dk = Math.Abs(m_Max - m_Min) / m_LfZstep;
                if ((10 <= dk) && (dk <= 30)) break;
                if (dk > 30) m_LfZstep = m_LfZstep * 2.0;
                if (dk < 10) m_LfZstep = m_LfZstep * 0.5;
            } while (true);

            zdat = 0; k = (int)dk;
            if (m_Min < 0)
            {
                do { zdat -= m_LfZstep; } while (zdat > m_Min);
                zdat += m_LfZstep;
            }
            else
            {
                do { zdat += m_LfZstep; } while (zdat < m_Min);
            }

            k = 0;
            List<double> lstfZdem = new List<double>();
            do
            {
                lstfZdem.Add(zdat);
                zdat += m_LfZstep;
                k++;
            } while (zdat < m_Max);
            lstfZdem.Add(m_Max);
            #endregion

            this.dataTable.Rows.Clear();
            for (int i = 0; i <= k; i++)
            {
                LinInfo linInfo = new LinInfo { LibID = 0, LinStyID = 1, OutClr = new int[] { 1, 4, 3 }, OutPenW = new float[] { 0.05F, 0.05F, 0.05F }, XScale = m_FSlopeYEps, YScale = m_FSlopeYEps };
                RegInfo reginfo = new RegInfo { OutPenW = 1, Ovprnt = true, PatClr = 3, PatHeight = 10, PatWidth = 10, FillClr = Imc[i % Imc.Length] };
                this.dataTable.Rows.Add(lstfZdem[i].ToString("F2"), "", "", (i % 3 == 0) ? "YES" : "NO", linInfo, reginfo);
            }
        }

        /// <summary>
        /// 步骤2：执行追踪，生成临时要素类
        /// </summary>
        private void GenerateContourLayers(RasterLayer sourceLayer)
        {
            if (m_ContourParamStrcT == null) return;

            RasterDataSet rasdataset = sourceLayer.GetData() as RasterDataSet;
            var traceContour = new RasTraceContour(rasdataset, m_BandNum);

            int n = this.dataTable.Rows.Count;
            ZVelStrcT_Stru[] arrayZVelStrcT = new ZVelStrcT_Stru[n];
            for (int i = 0; i < n; i++)
            {
                arrayZVelStrcT[i] = new ZVelStrcT_Stru
                {
                    linf = this.dataTable.Rows[i][4] as LinInfo,
                    rinf = this.dataTable.Rows[i][5] as RegInfo,
                    fZdem = (i == n - 1) ? m_Max : Convert.ToDouble(this.dataTable.Rows[i][0]),
                    mskOn = (sbyte)((this.dataTable.Rows[i][3] as string) == "YES" ? 1 : 0)
                };
            }
            m_ContourParamStrcT.SetZVelBuf(arrayZVelStrcT);
            m_ContourParamStrcT.pContourNoteParam = m_ContourNoteParam;

            m_Tempdaba = DataBase.OpenTempDB();
            if (m_Tempdaba == null) throw new Exception("打开临时数据库失败！");

            string baseName = GenerateClassName(Path.GetFileNameWithoutExtension(sourceLayer.Name), "Contour");

            // 创建各类要素
            m_Tempsfclslin = new SFeatureCls(m_Tempdaba);
            m_Tempsfclslin.Create(baseName + "_Line", GeomType.Lin, 0, 0, null);
            m_LinClsUrl = m_Tempsfclslin.URL;

            m_Tempsfclsreg = new SFeatureCls(m_Tempdaba);
            m_Tempsfclsreg.Create(baseName + "_Region", GeomType.Reg, 0, 0, null);
            m_RegClsUrl = m_Tempsfclsreg.URL;

            m_tempann = new AnnotationCls(m_Tempdaba);
            m_tempann.Create(baseName + "_Annotation", AnnType.Text, 0, 0, null);
            m_AnnClsUrl = m_tempann.URL;

            // 可选的坡度线
            // m_TempsfclsSlopelin = new SFeatureCls(m_Tempdaba); ...

            traceContour.ShowProgressBar(true);
            int rtn = traceContour.RsTraceContour(m_ContourParamStrcT, m_Tempsfclslin, null, m_Tempsfclsreg, m_tempann, 1024, false, m_ClipLine);
            traceContour.Dispose();

            if (rtn <= 0) throw new Exception("等值线追踪算法执行失败或没有生成任何结果。");
        }

        /// <summary>
        /// 步骤3：将要素类作为图层添加到主地图
        /// </summary>
        private void AddLayersToMainMap(Map targetMap, string baseName, IApplication hook)
        {
            if (targetMap == null) return;
            string layerBaseName = baseName + "_Contour";

            if (m_Tempsfclsreg != null && m_Tempsfclsreg.Count > 0)
            {
                var regLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                if (regLayer.AttachData(m_Tempsfclsreg))
                {
                    regLayer.Name = layerBaseName + "_区";
                    regLayer.URL = m_RegClsUrl;
                    targetMap.Append(regLayer);
                }
            }

            if (m_Tempsfclslin != null && m_Tempsfclslin.Count > 0)
            {
                var lineLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                if (lineLayer.AttachData(m_Tempsfclslin))
                {
                    lineLayer.Name = layerBaseName + "_线";
                    lineLayer.URL = m_LinClsUrl;
                    targetMap.Append(lineLayer);
                }
            }

            if (m_tempann != null && m_tempann.Count > 0)
            {
                var annLayer = new VectorLayer(VectorLayerType.AnnLayer);
                if (annLayer.AttachData(m_tempann))
                {
                    annLayer.Name = layerBaseName + "_注记";
                    annLayer.URL = m_AnnClsUrl;
                    targetMap.Append(annLayer);
                }
            }

            // 刷新主地图视图
            if (hook?.ActiveContentsView is IMapContentsView mapView)
            {
                mapView.MapControl.Refresh();
            }
        }

        // 从 FieldTransformForm 复制过来的辅助方法
        private string GenerateClassName(string baseName, string typeSuffix)
        {
            string sanitizedBase = Regex.Replace(baseName ?? "Import", @"[^\w]", "_");
            sanitizedBase = Regex.Replace(sanitizedBase, "_+", "_").Trim('_');
            if (string.IsNullOrWhiteSpace(sanitizedBase)) sanitizedBase = "Data";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string finalName = $"{sanitizedBase}_{typeSuffix}_{timestamp}";
            if (finalName.Length > 64)
            {
                string prefix = $"{sanitizedBase}_{typeSuffix}_".Substring(0, Math.Min($"{sanitizedBase}_{typeSuffix}_".Length, 64 - timestamp.Length - 1));
                finalName = prefix + timestamp;
                if (finalName.Length > 64) finalName = finalName.Substring(0, 64);
            }
            if (!char.IsLetter(finalName[0]))
            {
                finalName = "Lyr_" + finalName.Substring(0, Math.Min(finalName.Length, 60));
            }
            finalName = finalName.TrimEnd('_');
            return finalName;
        }
    }
}