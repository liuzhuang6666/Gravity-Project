using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoObjects.SpatialRef;

namespace MapGISPlugin3
{
    public class GravityRasterToPointRasterInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BandCount { get; set; }
        public double CellSizeX { get; set; }
        public double CellSizeY { get; set; }
        public Rect MapRange { get; set; }
        public bool HasNoData { get; set; }
        public double NoDataValue { get; set; }
        public string SpatialReferenceName { get; set; }
    }

    public class GravityRasterToPointResult
    {
        public string ClassName { get; set; }
        public string ClassUrl { get; set; }
        public string DatabasePath { get; set; }
        public int SuccessCount { get; set; }
        public int SkipNoDataCount { get; set; }
        public int SkipInvalidCount { get; set; }
    }

    public class GravityRasterToPointService
    {
        private const int MaxClassNameLength = 64;

        public GravityRasterToPointRasterInfo ReadRasterInfo(string tiffPath)
        {
            ValidateTiffPath(tiffPath);

            RasterDataSet raster = null;
            SRefData sref = null;

            try
            {
                raster = new RasterDataSet();
                if (!raster.Open(tiffPath, RasAccessType.RasAccessType_ReadOnly))
                {
                    throw new InvalidOperationException("TIFF 打开失败。");
                }

                double cellSizeX = 0;
                double cellSizeY = 0;
                raster.GetCellSize(ref cellSizeX, ref cellSizeY);

                GravityRasterToPointRasterInfo info = new GravityRasterToPointRasterInfo
                {
                    Width = raster.GetPixelWidth(),
                    Height = raster.GetPixelHeight(),
                    BandCount = raster.GetBandNum(),
                    CellSizeX = cellSizeX,
                    CellSizeY = cellSizeY,
                    MapRange = raster.GetMapRange(),
                    HasNoData = raster.HasNullVal()
                };

                info.NoDataValue = info.HasNoData ? raster.GetNullVal() : 0;
                sref = raster.GetProj();
                info.SpatialReferenceName = GetSpatialReferenceName(sref);
                return info;
            }
            finally
            {
                ReleaseComObject(sref);
                CloseRaster(raster);
            }
        }

        public GravityRasterToPointResult ImportToDatabase(string tiffPath, string dataSource, string gdbDirectory, string className)
        {
            ValidateTiffPath(tiffPath);
            ValidateGdbLocation(dataSource, gdbDirectory);

            if (!IsValidClassName(className))
            {
                throw new ArgumentException("输出点类名必须以字母开头，只能包含字母、数字和下划线，且长度不能超过 64。", nameof(className));
            }

            DataBase gdb = null;
            RasterDataSet raster = null;
            RasterBand band = null;
            SRefData sref = null;
            SFeatureCls sfc = null;
            Fields fields = null;
            bool batchStarted = false;
            bool createdClass = false;
            string classUrl = BuildClassUrl(dataSource, gdbDirectory, className);
            GravityRasterToPointResult result = new GravityRasterToPointResult();

            try
            {
                gdb = OpenDatabase(dataSource, gdbDirectory);
                if (gdb == null)
                {
                    throw new InvalidOperationException("目标 MapGIS 数据库打开失败。");
                }

                if (gdb.XClsIsExist(XClsType.SFCls, className) > 0)
                {
                    throw new InvalidOperationException("目标点类已存在，请更换输出名称后重试。");
                }

                raster = new RasterDataSet();
                if (!raster.Open(tiffPath, RasAccessType.RasAccessType_ReadOnly))
                {
                    throw new InvalidOperationException("TIFF 打开失败。");
                }

                if (raster.GetBandNum() <= 0)
                {
                    throw new InvalidOperationException("TIFF 不包含可用波段。");
                }

                band = raster.GetRasterBand(1);
                if (band == null)
                {
                    throw new InvalidOperationException("无法获取第 1 波段。");
                }

                sref = raster.GetProj();
                int srId = ResolveSpatialReferenceId(gdb, sref);
                fields = BuildFields();

                sfc = new SFeatureCls();
                if (sfc.Create(classUrl, GeomType.Pnt) <= 0)
                {
                    throw new InvalidOperationException("创建点类失败。");
                }
                createdClass = true;

                if (sfc.UpdateFields(fields) <= 0)
                {
                    throw new InvalidOperationException("更新点类字段失败。");
                }

                if (!sfc.BeginBatch(BatchType.Append))
                {
                    throw new InvalidOperationException("启动批量入库失败。");
                }
                batchStarted = true;

                bool hasNoData = raster.HasNullVal();
                double noDataValue = hasNoData ? raster.GetNullVal() : 0;
                int width = raster.GetPixelWidth();
                int height = raster.GetPixelHeight();
                Record record = new Record { Fields = fields };

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        double value;
                        try
                        {
                            value = Convert.ToDouble(band.GetPixel(col, row));
                        }
                        catch
                        {
                            result.SkipInvalidCount++;
                            continue;
                        }

                        if (double.IsNaN(value) || double.IsInfinity(value))
                        {
                            result.SkipInvalidCount++;
                            continue;
                        }

                        if (hasNoData && AreEqual(value, noDataValue))
                        {
                            result.SkipNoDataCount++;
                            continue;
                        }

                        double x;
                        double y;
                        if (!raster.GetCoordinate(col + 0.5, row + 0.5, out x, out y, 1))
                        {
                            result.SkipInvalidCount++;
                            continue;
                        }

                        Dot3D dot = null;
                        GeoPoints points = null;

                        try
                        {
                            dot = new Dot3D(x, y, 0);
                            points = new GeoPoints();
                            points.Append(dot);
                            record["X"] = x;
                            record["Y"] = y;
                            record["Value"] = value;

                            if (sfc.Append(points, record, null) > 0)
                            {
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.SkipInvalidCount++;
                            }
                        }
                        finally
                        {
                            ReleaseComObject(points);
                            ReleaseComObject(dot);
                        }
                    }
                }

                if (result.SuccessCount <= 0)
                {
                    throw new InvalidOperationException("没有可写入的有效像元。");
                }

                if (!sfc.EndBatch())
                {
                    throw new InvalidOperationException("批量入库提交失败。");
                }
                batchStarted = false;

                result.ClassName = className;
                result.DatabasePath = dataSource + ":" + NormalizeDirectory(gdbDirectory);
                result.ClassUrl = classUrl;
                return result;
            }
            catch
            {
                if (batchStarted && sfc != null)
                {
                    try
                    {
                        sfc.EndBatch();
                    }
                    catch
                    {
                    }
                }

                if (createdClass && result.SuccessCount <= 0 && sfc != null)
                {
                    try
                    {
                        if (sfc.HasOpen())
                        {
                            sfc.Close();
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        SFeatureCls.Remove(gdb, className);
                    }
                    catch
                    {
                    }
                }

                throw;
            }
            finally
            {
                if (sfc != null)
                {
                    try
                    {
                        if (sfc.HasOpen())
                        {
                            sfc.Close();
                        }
                    }
                    catch
                    {
                    }
                }

                ReleaseComObject(fields);
                ReleaseComObject(sfc);
                ReleaseComObject(band);
                ReleaseComObject(sref);
                CloseRaster(raster);
                ReleaseComObject(gdb);
            }
        }

        public string BuildDefaultClassName(string tiffPath)
        {
            string baseName = Path.GetFileNameWithoutExtension(tiffPath) ?? string.Empty;
            baseName = Regex.Replace(baseName, @"[^\w]", "_");
            baseName = Regex.Replace(baseName, @"_+", "_").Trim('_');

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "GravityRaster";
            }

            if (!char.IsLetter(baseName[0]))
            {
                baseName = "G" + baseName;
            }

            string finalName = baseName + "_Point";
            if (finalName.Length > MaxClassNameLength)
            {
                finalName = finalName.Substring(0, MaxClassNameLength);
            }

            return finalName;
        }

        public bool IsValidClassName(string className)
        {
            if (string.IsNullOrWhiteSpace(className) || className.Length > MaxClassNameLength)
            {
                return false;
            }

            return Regex.IsMatch(className, "^[A-Za-z][A-Za-z0-9_]*$");
        }

        private static void ValidateTiffPath(string tiffPath)
        {
            if (string.IsNullOrWhiteSpace(tiffPath))
            {
                throw new ArgumentException("TIFF 路径不能为空。", nameof(tiffPath));
            }

            if (!File.Exists(tiffPath))
            {
                throw new FileNotFoundException("指定的 TIFF 文件不存在。", tiffPath);
            }

            string extension = Path.GetExtension(tiffPath);
            if (!string.Equals(extension, ".tif", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".tiff", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("只支持 TIFF 文件（.tif/.tiff）。", nameof(tiffPath));
            }
        }

        private static void ValidateGdbLocation(string dataSource, string gdbDirectory)
        {
            if (string.IsNullOrWhiteSpace(dataSource))
            {
                throw new ArgumentException("MapGIS 数据源不能为空。", nameof(dataSource));
            }

            if (string.IsNullOrWhiteSpace(gdbDirectory) || gdbDirectory == "/")
            {
                throw new ArgumentException("请选择有效的 MapGIS 数据库目录。", nameof(gdbDirectory));
            }
        }

        private static Fields BuildFields()
        {
            Fields fields = new Fields();
            fields.AppendField(new Field { FieldName = "X", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
            fields.AppendField(new Field { FieldName = "Y", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
            fields.AppendField(new Field { FieldName = "Value", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
            return fields;
        }

        private static DataBase OpenDatabase(string dataSource, string gdbDirectory)
        {
            string dbName = GetDatabaseName(gdbDirectory);
            string databaseUrl = $"gdbp://{dataSource}/{dbName}";
            DataBase gdb = DataBase.OpenByURL(databaseUrl);
            if (gdb == null || !gdb.HasOpened)
            {
                throw new InvalidOperationException("无法打开目标数据库：" + databaseUrl);
            }

            return gdb;
        }

        private static int ResolveSpatialReferenceId(DataBase gdb, SRefData sref)
        {
            if (gdb == null || sref == null || gdb.SpatialRefMng == null)
            {
                return 0;
            }

            string srefName = GetSpatialReferenceName(sref);
            if (!string.IsNullOrWhiteSpace(srefName) && srefName != "未知")
            {
                try
                {
                    int existingId = gdb.SpatialRefMng.GetID(srefName);
                    if (existingId > 0)
                    {
                        return existingId;
                    }
                }
                catch
                {
                }
            }

            try
            {
                int appendId = gdb.SpatialRefMng.Append(sref);
                if (appendId > 0)
                {
                    return appendId;
                }
            }
            catch
            {
            }

            return 0;
        }

        private static string GetSpatialReferenceName(SRefData sref)
        {
            if (sref == null)
            {
                return "未知";
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(sref.SRSName))
                {
                    return sref.SRSName;
                }
            }
            catch
            {
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(sref.PCSName))
                {
                    return sref.PCSName;
                }
            }
            catch
            {
            }

            return "未知";
        }

        private static string BuildClassUrl(string dataSource, string gdbDirectory, string className)
        {
            return $"gdbp://{dataSource}{NormalizeDirectory(gdbDirectory)}/sfcls/{className}";
        }

        private static string GetDatabaseName(string gdbDirectory)
        {
            string normalized = NormalizeDirectory(gdbDirectory).Trim('/');
            string[] segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 0)
            {
                throw new InvalidOperationException("无法解析数据库名称。");
            }

            return segments[0];
        }

        private static string NormalizeDirectory(string gdbDirectory)
        {
            if (string.IsNullOrWhiteSpace(gdbDirectory))
            {
                return "/";
            }

            return "/" + gdbDirectory.Trim('/');
        }

        private static bool AreEqual(double left, double right)
        {
            return Math.Abs(left - right) <= 1E-12;
        }

        private static void CloseRaster(RasterDataSet raster)
        {
            if (raster != null)
            {
                try
                {
                    raster.Close();
                }
                catch
                {
                }
            }

            ReleaseComObject(raster);
        }

        private static void ReleaseComObject(object comObject)
        {
            if (comObject == null)
            {
                return;
            }

            try
            {
                if (Marshal.IsComObject(comObject))
                {
                    Marshal.ReleaseComObject(comObject);
                }
            }
            catch
            {
            }
        }
    }
}
