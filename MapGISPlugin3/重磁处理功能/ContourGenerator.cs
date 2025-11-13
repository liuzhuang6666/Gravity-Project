// ContourGenerator.cs
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Info;
using MapGIS.RasAnalysis;
using MapGIS.RasCommonObj;
using System;
using System.Collections.Generic;
using System.IO;
using MapGIS.PluginEngine;
using MapGIS.UI.Controls;
using MapGIS.GeoObjects.Geometry;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MapGISPlugin3
{
    public static class ContourGenerator
    {
        public static bool GenerateAndAddToMap(RasterLayer sourceLayer, Map targetMap, double interval, IApplication hook)
        {
            if (sourceLayer == null || targetMap == null || interval <= 0) return false;

            RasterDataSet rasDataSet = sourceLayer.GetData() as RasterDataSet;
            if (rasDataSet == null) return false;

            #region 参数准备 (逻辑不变)
            int bandNum = 1;
            double minVal = rasDataSet.GetRasterBand(bandNum).MinValue;
            double maxVal = rasDataSet.GetRasterBand(bandNum).MaxValue;
            if (minVal.CompareTo(maxVal) == 0) return false;

            ContourParamStrcT_Stru contourParam = new ContourParamStrcT_Stru();
            contourParam.bMakeReg = true;
            contourParam.bMapNote = true;

            List<double> zValues = new List<double>();
            double currentZ = Math.Floor(minVal / interval) * interval;
            while (currentZ <= maxVal)
            {
                if (currentZ >= minVal) zValues.Add(currentZ);
                currentZ += interval;
            }
            if (zValues.Count == 0 && maxVal > minVal) zValues.Add(minVal);

            ZVelStrcT_Stru[] zVelArray = new ZVelStrcT_Stru[zValues.Count];
            int[] colors = { 601, 603, 498, 500, 436, 408, 391, 233, 190, 184, 154, 122, 106, 33, 31, 127, 391, 128, 392, 393, 136, 149, 150, 442, 443, 186, 444, 179, 180, 445, 189, 190 };

            for (int i = 0; i < zValues.Count; i++)
            {
                zVelArray[i] = new ZVelStrcT_Stru
                {
                    fZdem = zValues[i],
                    linf = new LinInfo { LinStyID = 1, OutClr = new[] { 1, 4, 3 }, OutPenW = new[] { 0.05f, 0.05f, 0.05f } },
                    rinf = new RegInfo { FillClr = colors[i % colors.Length], Ovprnt = true },
                    mskOn = (sbyte)(i % 3 == 0 ? 1 : 0)
                };
            }
            contourParam.SetZVelBuf(zVelArray);

            ContourNoteParam_Stru noteParam = new ContourNoteParam_Stru();
            Rect mapRange = rasDataSet.GetMapRange();
            float length = (float)Math.Min(mapRange.XMax - mapRange.XMin, mapRange.YMax - mapRange.YMin);
            noteParam.MinDist = length / 200;
            noteParam.LabelFnt.FixSize = length / 250 > 0 ? length / 250 : 1.0f;
            contourParam.pContourNoteParam = noteParam;
            #endregion

            string baseName = Path.GetFileNameWithoutExtension(sourceLayer.Name);
            string regClassName = GenerateClassName(baseName, "Region");
            string linClassName = GenerateClassName(baseName, "Line");
            string annClassName = GenerateClassName(baseName, "Anno");

            DataBase db = null;
            string dataSource = "";

            try
            {
                // 1. 打开数据库
                string[] dataSourcesToTry = { "MapGisLocal", "MapGisLocalPlus" };
                foreach (var ds in dataSourcesToTry)
                {
                    db = DataBase.OpenByURL($"gdbp://{ds}/Temporary");
                    if (db != null && db.HasOpened)
                    {
                        dataSource = ds;
                        break;
                    }
                }
                if (db == null) throw new Exception("无法打开 /Temporary 数据库。");

                // 2. 在数据库上下文中创建所有类
                SFeatureCls sfclsReg = new SFeatureCls(db);
                if (sfclsReg.Create(regClassName, GeomType.Reg, 0, 0, null) <= 0) throw new Exception($"创建区要素类 '{regClassName}' 失败。");

                SFeatureCls sfclsLin = new SFeatureCls(db);
                if (sfclsLin.Create(linClassName, GeomType.Lin, 0, 0, null) <= 0) throw new Exception($"创建线要素类 '{linClassName}' 失败。");

                AnnotationCls annCls = new AnnotationCls(db);
                if (annCls.Create(annClassName, AnnType.Text, 0, 0, null) <= 0) throw new Exception($"创建注记类 '{annClassName}' 失败。");

                // 3. 执行追踪
                RasTraceContour traceContour = new RasTraceContour(rasDataSet, bandNum);
                traceContour.ShowProgressBar(true);
                int result = traceContour.RsTraceContour(contourParam, sfclsLin, null, sfclsReg, annCls, 1024, false, true);
                traceContour.Dispose();

                if (result <= 0) throw new Exception("等值线追踪算法执行失败，没有生成任何结果。");

                // 4. 加载图层
                VectorLayer regLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                if (regLayer.AttachData(sfclsReg))
                {
                    regLayer.Name = regClassName;
                    targetMap.Append(regLayer);
                }

                VectorLayer linLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                if (linLayer.AttachData(sfclsLin))
                {
                    linLayer.Name = linClassName;
                    targetMap.Append(linLayer);
                }

                VectorLayer annLayer = new VectorLayer(VectorLayerType.AnnLayer);
                if (annLayer.AttachData(annCls))
                {
                    annLayer.Name = annClassName;
                    targetMap.Append(annLayer);
                }

                if (hook?.ActiveContentsView is IMapContentsView mapView)
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
            // --- 【最终核心修正：移除FINALLY块】 ---
            // 我们不再关闭db，因为新添加的图层依赖这个连接。
            // MapGIS应用程序将负责管理/Temporary数据库的生命周期。
        }

        private static string GenerateClassName(string baseName, string typeSuffix)
        {
            string sanitizedBase = Regex.Replace(baseName ?? "Layer", @"[^\w]", "_");
            sanitizedBase = Regex.Replace(sanitizedBase, "_+", "_").Trim('_');
            if (string.IsNullOrWhiteSpace(sanitizedBase)) sanitizedBase = "Data";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string finalName = $"{sanitizedBase}_{typeSuffix}_{timestamp}";
            if (!char.IsLetter(finalName[0])) finalName = "C_" + finalName;
            if (finalName.Length > 30) finalName = finalName.Substring(0, 30);
            return finalName;
        }
    }
}