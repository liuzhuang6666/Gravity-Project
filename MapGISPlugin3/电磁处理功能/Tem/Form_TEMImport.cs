using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.PluginEngine;
using MapGIS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 数据导入窗体
    /// 【复用】整体结构参考 Form_MTAddData
    /// 【新增】针对 TEM 的特有逻辑：处理两个输入文件、创建三张表
    /// 【优化】添加 COM 组件检查、发射源图层管理、对象生命周期日志
    /// </summary>
    public partial class Form_TEMImport : Form
    {
        private IApplication _hook;
        private string selectedDataSource = "MapGisLocalPlus";
        private string selectedGdbDirectory = "/Temporary";
        private Point _mouseDownPoint = new Point(); // 用于记录鼠标按下时的位置

        public Form_TEMImport(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            UpdateGdbPathDisplay();
            // 【新增】COM 组件引用检查
            CheckCOMReferences();
            // 初始化拖动事件（绑定标题栏panelTitle）
            InitDragEvents();
        }
        /// <summary>
        /// 初始化标题栏拖动事件
        /// </summary>
        private void InitDragEvents()
        {
            // 鼠标按下时记录位置
            panel1.MouseDown += PanelTitle_MouseDown;
            // 鼠标移动时拖动窗口
            panel1.MouseMove += PanelTitle_MouseMove;
        }
        /// <summary>
        /// 标题栏鼠标按下：记录鼠标相对窗口的位置
        /// </summary>
        private void PanelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // 仅响应左键
            {
                // 记录鼠标在窗口内的坐标（相对于标题栏）
                _mouseDownPoint.X = e.X;
                _mouseDownPoint.Y = e.Y;
            }
        }

        /// <summary>
        /// 标题栏鼠标移动：计算窗口新位置
        /// </summary>
        private void PanelTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // 左键按住时才拖动
            {
                // 窗口新位置 = 鼠标当前屏幕位置 - 按下时的相对位置
                this.Left = Control.MousePosition.X - _mouseDownPoint.X;
                this.Top = Control.MousePosition.Y - _mouseDownPoint.Y;
            }
        }
        #region --- 新增：COM 组件引用检查 ---
        /// <summary>
        /// 检查 MapGIS 核心 COM 组件引用是否正常
        /// </summary>
        private void CheckCOMReferences()
        {
            try
            {
                // 1. 验证 SFeatureCls 组件
                var testSfc = new SFeatureCls();
                Console.WriteLine("SFeatureCls 组件引用正常。");

                // 2. 验证 RecordSet 组件（通过临时要素类的 Select 方法获取，避免无参构造）
                // 创建一个临时内存要素类（仅用于测试，不实际写入数据）
                string tempUrl = $"gdbp://MapGisLocalPlus/Temporary/sfcls/Test_ComCheck_{Guid.NewGuid()}";
                if (testSfc.Create(tempUrl, GeomType.Pnt) > 0)
                {
                    // 通过 Select 方法获取 RecordSet 实例（正确方式）
                    RecordSet testRs = testSfc.Select(null);
                    if (testRs != null)
                    {
                        Console.WriteLine("RecordSet 组件引用正常。");
                        Marshal.ReleaseComObject(testRs); // 及时释放
                    }
                    else
                    {
                        throw new Exception("RecordSet 实例获取失败。");
                    }
                    // 删除临时测试类
                    SFeatureCls.Remove(testSfc.GDataBase, testSfc.ClsName);
                }
                else
                {
                    throw new Exception("临时要素类创建失败，无法验证 RecordSet。");
                }

                Marshal.ReleaseComObject(testSfc); // 释放要素类
                Console.WriteLine("所有 MapGIS COM 组件引用验证通过。");
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region --- 事件处理 ---

        /// <summary>
        /// 【复用】选择文件的逻辑参考 MTImportCommand
        /// 【修改】选择 knowed.dat 文件
        /// </summary>
        private void btnSelectKnowedFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "请选择 TEM 观测数据文件 (knowed.dat)";
                ofd.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtKnowedFile.Text = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// 【新增】选择 tran.dat 文件（这是 TEM 特有的）
        /// </summary>
        private void btnSelectTranFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "请选择 TEM 发射源文件 (tran.dat)";
                ofd.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtTranFile.Text = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// 【复用】选择 GDB 位置，逻辑参考 Form_MTAddData
        /// </summary>
        private void btnSelectGdbLocation_Click(object sender, EventArgs e)
        {
            using (GDBSelectFolderDialog gdbDialog = new GDBSelectFolderDialog())
            {
                gdbDialog.Title = "选择保存 GDB 位置";
                try
                {
                    gdbDialog.SelectedPath = $"gdbp://{selectedDataSource}{selectedGdbDirectory}";
                }
                catch { }

                if (gdbDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string fullSelectedPath = gdbDialog.SelectedPath;
                    Console.WriteLine($"GDB 返回路径: {fullSelectedPath}");

                    if (!string.IsNullOrEmpty(fullSelectedPath) && fullSelectedPath.StartsWith("gdbp://", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Uri uri = new Uri(fullSelectedPath);
                            string host = uri.Host;
                            string path = uri.AbsolutePath;

                            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(path) && path != "/")
                            {
                                this.selectedDataSource = host;
                                this.selectedGdbDirectory = path;
                                UpdateGdbPathDisplay();
                                Console.WriteLine($"更新 GDB 位置: {selectedDataSource}:{selectedGdbDirectory}");
                            }
                            else
                            {
                                MessageBox.Show("请选择有效的数据库或目录。", "选择错误");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"解析 GDB 路径出错: {ex.Message}", "错误");
                        }
                    }
                    else
                    {
                        MessageBox.Show("未获取有效 GDB 路径。", "错误");
                    }
                }
            }
        }

        /// <summary>
        /// 【复用】确定按钮的整体框架参考 Form_MTAddData.btnOK_Click
        /// 【新增】处理两个输入文件、创建三张表的逻辑
        /// 【优化】添加对象生命周期日志、异常详细追踪
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnOK.Enabled = false;
            btnCancel.Enabled = false;

            string knowedFile = txtKnowedFile.Text;
            string tranFile = txtTranFile.Text;
            string dataSource = this.selectedDataSource;
            string gdbDirectory = this.selectedGdbDirectory;

            // 【复用】验证输入
            if (string.IsNullOrWhiteSpace(knowedFile) || !File.Exists(knowedFile))
            {
                MessageBox.Show("请选择有效的观测数据文件。", "错误");
                ResetFormState();
                return;
            }

            if (string.IsNullOrWhiteSpace(tranFile) || !File.Exists(tranFile))
            {
                MessageBox.Show("请选择有效的发射源文件。", "错误");
                ResetFormState();
                return;
            }

            if (string.IsNullOrWhiteSpace(dataSource) || string.IsNullOrWhiteSpace(gdbDirectory) || gdbDirectory == "/")
            {
                MessageBox.Show("请选择有效的 GDB 位置。", "错误");
                ResetFormState();
                return;
            }

            // 【复用】生成类名称
            string baseName = Path.GetFileNameWithoutExtension(knowedFile);

            // 生成三个统一风格的名字
            string stationClassName = GenerateClassName(baseName, "测点");
            string transmitterClassName = GenerateClassName(baseName, "发射源");
            string observationTableName = GenerateClassName(baseName, "观测数据");

            string cleanGdbDirectory = "/" + gdbDirectory.Trim('/');
            string stationClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/sfcls/{stationClassName}";
            string transmitterClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/sfcls/{transmitterClassName}";
            string observationTableUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/ocls/{observationTableName}";

            // 【新增】数据读取
            var allObservationRows = new List<TEMObservationData>();
            var uniqueStations = new Dictionary<string, TEMStationInfo>();
            TEMTransmitterInfo transmitterInfo = null;

            try
            {
                // 【新增】读取观测数据文件
                Console.WriteLine("=== 开始读取观测数据文件 ===");
                using (StreamReader reader = new StreamReader(knowedFile, Encoding.Default))
                {
                    string line;
                    int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (lineNumber == 1 && IsHeaderLine(line))
                        {
                            Console.WriteLine("跳过标题行。");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] parts = Regex.Split(line.Trim(), @"\s+");
                        if (parts.Length < 7)
                        {
                            Console.WriteLine($"警告: 行 {lineNumber} 列数不足 ({parts.Length}列)，已跳过。");
                            continue;
                        }

                        try
                        {
                            TEMObservationData rowData = new TEMObservationData
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                SamplingTime_us = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                EffectiveArea = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                                InducedVoltage_mV = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture)
                            };

                            allObservationRows.Add(rowData);

                            string stationKey = $"{rowData.LineName}_{rowData.StationName}";
                            if (!uniqueStations.ContainsKey(stationKey))
                            {
                                uniqueStations.Add(stationKey, new TEMStationInfo
                                {
                                    LineName = rowData.LineName,
                                    StationName = rowData.StationName,
                                    X = rowData.X,
                                    Y = rowData.Y
                                });
                            }
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine($"警告: 行 {lineNumber} 解析数字失败: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"警告: 行 {lineNumber} 处理出错: {ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"观测数据读取完成: {allObservationRows.Count} 条记录, {uniqueStations.Count} 个测点。");

                // 【新增】读取发射源文件
                Console.WriteLine("=== 开始读取发射源文件 ===");
                var tranLines = new List<string>();
                using (StreamReader reader = new StreamReader(tranFile, Encoding.Default))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            tranLines.Add(line);
                    }
                }

                if (tranLines.Count >= 4)
                {
                    try
                    {
                        string[] partsA = Regex.Split(tranLines[0].Trim(), @"\s+");
                        string[] partsB = Regex.Split(tranLines[1].Trim(), @"\s+");
                        string[] partsC = Regex.Split(tranLines[2].Trim(), @"\s+");
                        string[] partsD = Regex.Split(tranLines[3].Trim(), @"\s+");

                        if (partsA.Length >= 3 && partsB.Length >= 3 && partsC.Length >= 3 && partsD.Length >= 3)
                        {
                            transmitterInfo = new TEMTransmitterInfo
                            {
                                PointA_X = double.Parse(partsA[0], System.Globalization.CultureInfo.InvariantCulture),
                                PointA_Y = double.Parse(partsA[1], System.Globalization.CultureInfo.InvariantCulture),
                                PointA_Z = double.Parse(partsA[2], System.Globalization.CultureInfo.InvariantCulture),
                                PointB_X = double.Parse(partsB[0], System.Globalization.CultureInfo.InvariantCulture),
                                PointB_Y = double.Parse(partsB[1], System.Globalization.CultureInfo.InvariantCulture),
                                PointB_Z = double.Parse(partsB[2], System.Globalization.CultureInfo.InvariantCulture),
                                PointC_X = double.Parse(partsC[0], System.Globalization.CultureInfo.InvariantCulture),
                                PointC_Y = double.Parse(partsC[1], System.Globalization.CultureInfo.InvariantCulture),
                                PointC_Z = double.Parse(partsC[2], System.Globalization.CultureInfo.InvariantCulture),
                                PointD_X = double.Parse(partsD[0], System.Globalization.CultureInfo.InvariantCulture),
                                PointD_Y = double.Parse(partsD[1], System.Globalization.CultureInfo.InvariantCulture),
                                PointD_Z = double.Parse(partsD[2], System.Globalization.CultureInfo.InvariantCulture)
                            };
                            Console.WriteLine("发射源信息读取完成。");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解析发射源文件失败: {ex.Message}");
                    }
                }

                if (allObservationRows.Count == 0 || uniqueStations.Count == 0 || transmitterInfo == null)
                {
                    MessageBox.Show("未读取到有效数据。请检查文件内容和格式。", "错误");
                    ResetFormState();
                    return;
                }

                // 【新增】创建数据库表
                SFeatureCls stationSfc = null;
                SFeatureCls transmitterSfc = null;
                ObjectCls observationOcls = null;
                bool stationCreated = false;
                bool transmitterCreated = false;
                bool observationCreated = false;

                try
                {
                    // 【新增】创建测点要素类
                    Console.WriteLine($"=== 开始创建测点类: {stationClassUrl} ===");
                    stationSfc = new SFeatureCls();
                    if (stationSfc.Create(stationClassUrl, GeomType.Pnt) <= 0)
                    {
                        throw new Exception($"创建点要素类 '{stationClassName}' 失败");
                    }
                    stationCreated = true;
                    Console.WriteLine("测点要素类创建成功");

                    Fields stationFields = new Fields();
                    stationFields.AppendField(new Field { FieldName = "测线号", FieldType = FieldType.FldString, MskLength = 50 });
                    stationFields.AppendField(new Field { FieldName = "测点号", FieldType = FieldType.FldString, MskLength = 50 });
                    stationFields.AppendField(new Field { FieldName = "X坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                    stationFields.AppendField(new Field { FieldName = "Y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });

                    if (stationSfc.UpdateFields(stationFields) <= 0)
                    {
                        throw new Exception("更新测点类字段失败。");
                    }
                    Console.WriteLine("测点类字段更新成功");

                    Console.WriteLine("开始填充测点类...");
                    stationSfc.BeginBatch(BatchType.Append);
                    Record stationRecord = new Record { Fields = stationFields };

                    foreach (var stationInfo in uniqueStations.Values)
                    {
                        Dot3D pnt3D = new Dot3D(stationInfo.X, stationInfo.Y, 0.0);
                        GeoPoints currentPnts = new GeoPoints();
                        currentPnts.Append(pnt3D);

                        stationRecord["测线号"] = stationInfo.LineName;
                        stationRecord["测点号"] = stationInfo.StationName;
                        stationRecord["X坐标"] = stationInfo.X;
                        stationRecord["Y坐标"] = stationInfo.Y;

                        if (stationSfc.Append(currentPnts, stationRecord, null) <= 0)
                        {
                            Console.WriteLine($"警告: 追加测点 '{stationInfo.StationName}' 失败。");
                        }
                    }

                    stationSfc.EndBatch();
                    Console.WriteLine("填充测点类完成。");

                    // 【新增】创建发射源要素类
                    Console.WriteLine($"=== 开始创建发射源类: {transmitterClassUrl} ===");
                    transmitterSfc = new SFeatureCls();
                    if (transmitterSfc.Create(transmitterClassUrl, GeomType.Pnt) <= 0)
                    {
                        throw new Exception($"创建发射源要素类 '{transmitterClassName}' 失败");
                    }
                    transmitterCreated = true;
                    Console.WriteLine("发射源要素类创建成功");

                    Fields transmitterFields = new Fields();
                    transmitterFields.AppendField(new Field { FieldName = "点名", FieldType = FieldType.FldString, MskLength = 10 });
                    transmitterFields.AppendField(new Field { FieldName = "X坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                    transmitterFields.AppendField(new Field { FieldName = "Y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                    transmitterFields.AppendField(new Field { FieldName = "Z", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });

                    if (transmitterSfc.UpdateFields(transmitterFields) <= 0)
                    {
                        throw new Exception("更新发射源类字段失败。");
                    }
                    Console.WriteLine("发射源类字段更新成功");

                    Console.WriteLine("开始填充发射源类...");
                    transmitterSfc.BeginBatch(BatchType.Append);
                    Record transmitterRecord = new Record { Fields = transmitterFields };

                    var tranPoints = new[]
                    {
                        new { Name = "A", X = transmitterInfo.PointA_X, Y = transmitterInfo.PointA_Y, Z = transmitterInfo.PointA_Z },
                        new { Name = "B", X = transmitterInfo.PointB_X, Y = transmitterInfo.PointB_Y, Z = transmitterInfo.PointB_Z },
                        new { Name = "C", X = transmitterInfo.PointC_X, Y = transmitterInfo.PointC_Y, Z = transmitterInfo.PointC_Z },
                        new { Name = "D", X = transmitterInfo.PointD_X, Y = transmitterInfo.PointD_Y, Z = transmitterInfo.PointD_Z }
                    };
                    foreach (var point in tranPoints)
                    {
                        Dot3D pnt3D = null;
                        GeoPoints currentPnts = null;
                        try
                        {
                            pnt3D = new Dot3D(point.X, point.Y, point.Z);
                            currentPnts = new GeoPoints();
                            currentPnts.Append(pnt3D);
                            transmitterRecord["点名"] = point.Name;
                            transmitterRecord["X坐标"] = point.X;
                            transmitterRecord["Y坐标"] = point.Y;
                            transmitterRecord["Z"] = point.Z;

                            if (transmitterSfc.Append(currentPnts, transmitterRecord, null) <= 0)
                            {
                                Console.WriteLine($"警告: 追加发射源点 '{point.Name}' 失败。");
                            }
                        }
                        finally
                        {
                            // 补充释放次要对象
                            if (pnt3D != null) try { Marshal.ReleaseComObject(pnt3D); pnt3D = null; } catch { }
                            if (currentPnts != null) try { Marshal.ReleaseComObject(currentPnts); currentPnts = null; } catch { }
                        }
                    }
                    transmitterSfc.EndBatch();
                    Console.WriteLine("填充发射源类完成。");

                    // 【新增】创建观测数据表
                    Console.WriteLine($"=== 开始创建观测数据表: {observationTableUrl} ===");
                    Fields observationFields = new Fields();
                    observationFields.AppendField(new Field { FieldName = "测线号", FieldType = FieldType.FldString, MskLength = 50 });
                    observationFields.AppendField(new Field { FieldName = "测点号", FieldType = FieldType.FldString, MskLength = 50 });
                    observationFields.AppendField(new Field { FieldName = "X坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                    observationFields.AppendField(new Field { FieldName = "Y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                    observationFields.AppendField(new Field { FieldName = "采样时间_us", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                    observationFields.AppendField(new Field { FieldName = "有效面积", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                    observationFields.AppendField(new Field { FieldName = "感应电压_mV", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });

                    observationOcls = new ObjectCls();
                    int createResult = observationOcls.Create(observationTableUrl, observationFields);
                    if (createResult <= 0)
                    {
                        throw new Exception($"创建观测数据表失败，返回: {createResult}");
                    }
                    observationCreated = true;
                    Console.WriteLine("观测数据表创建成功");

                    Console.WriteLine("开始填充观测数据表...");
                    observationOcls.BeginBatch(BatchType.Append);
                    Record observationRecord = new Record { Fields = observationFields };
                    int appendCount = 0;

                    foreach (var rowData in allObservationRows)
                    {
                        observationRecord["测线号"] = rowData.LineName;
                        observationRecord["测点号"] = rowData.StationName;
                        observationRecord["X坐标"] = rowData.X;
                        observationRecord["Y坐标"] = rowData.Y;
                        observationRecord["采样时间_us"] = rowData.SamplingTime_us;
                        observationRecord["有效面积"] = rowData.EffectiveArea;
                        observationRecord["感应电压_mV"] = rowData.InducedVoltage_mV;

                        if (observationOcls.Append(null, observationRecord, null) > 0)
                            appendCount++;
                        else
                            Console.WriteLine($"警告: 追加记录失败 (测点: {rowData.StationName})。");
                    }
                    observationOcls.EndBatch();
                    Console.WriteLine($"填充观测数据表完成: {appendCount}/{allObservationRows.Count} 条。");

                    // 【复用】加载到地图
                    Map targetMap = FindMapByName("电法数据");
                    if (targetMap == null)
                    {
                        Console.WriteLine("创建 电法数据 地图...");
                        targetMap = new Map();
                        targetMap.Name = "电法数据";
                        _hook.Document.GetMaps().Append(targetMap);
                    }

                    Console.WriteLine("开始加载图层到地图...");

                    VectorLayer stationLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                    bool stationAttached = stationLayer.AttachData(stationSfc);
                    if (stationAttached)
                    {
                        stationLayer.Name = stationClassName;
                        targetMap.Append(stationLayer);
                        Console.WriteLine("测点图层加载成功。");
                    }
                    else
                    {
                        MessageBox.Show("测点图层附加失败。", "警告");
                    }

                    VectorLayer transmitterLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                    bool transmitterAttached = transmitterLayer.AttachData(transmitterSfc);
                    if (transmitterAttached)
                    {
                        transmitterLayer.Name = transmitterClassName;
                        targetMap.Append(transmitterLayer);
                        Console.WriteLine("发射源图层加载成功。");
                    }
                    else
                    {
                        MessageBox.Show("发射源图层附加失败。", "警告");
                    }

                    ObjectLayer observationLayer = new ObjectLayer();
                    bool observationAttached = observationLayer.AttachData(observationOcls);
                    if (observationAttached)
                    {
                        observationLayer.Name = observationTableName;
                        observationLayer.URL = observationTableUrl;
                        targetMap.Append(observationLayer);
                        Console.WriteLine("观测数据表加载成功。");
                    }
                    else
                    {
                        MessageBox.Show("观测数据表附加到地图失败。", "警告");
                    }

                    MessageBox.Show($"导入成功！\n图层: {stationClassName}\n发射源: {transmitterClassName}\n表: {observationTableName}\n位置: {dataSource}:{gdbDirectory}", "完成");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (COMException comEx)
                {
                    MessageBox.Show($"MapGIS 操作失败: {comEx.Message}", "GDB 错误");
                    Console.WriteLine($"=== GDB COM 异常详情 ===\n{comEx}");
                    if (stationCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, stationClassName, XClsType.SFCls);
                    if (transmitterCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, transmitterClassName, XClsType.SFCls);
                    if (observationCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, observationTableName, XClsType.OCls);
                    ResetFormState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入出错: {ex.Message}", "错误");
                    Console.WriteLine($"=== 导入异常详情 ===\n{ex}");
                    if (stationCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, stationClassName, XClsType.SFCls);
                    if (transmitterCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, transmitterClassName, XClsType.SFCls);
                    if (observationCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, observationTableName, XClsType.OCls);
                    ResetFormState();
                }
                finally
                {
                    // 【复用】资源释放逻辑参考 Form_MTAddData
                    // 【优化】添加释放日志
                    Console.WriteLine("=== 开始释放资源 ===");
                    if (stationSfc != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(stationSfc);
                            Console.WriteLine("测点要素类资源释放成功");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"测点要素类释放失败: {ex.Message}");
                        }
                    }
                    if (transmitterSfc != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(transmitterSfc);
                            Console.WriteLine("发射源要素类资源释放成功");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"发射源要素类释放失败: {ex.Message}");
                        }
                    }
                    if (observationOcls != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(observationOcls);
                            Console.WriteLine("观测数据表资源释放成功");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"观测数据表释放失败: {ex.Message}");
                        }
                    }
                    Console.WriteLine("=== 资源释放完成 ===");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入过程中发生异常: {ex.Message}", "错误");
                Console.WriteLine($"=== 导入异常详情 ===\n{ex}");
                ResetFormState();
            }
        }

        /// <summary>
        /// 【复用】取消按钮，逻辑参考 Form_MTAddData
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region --- 辅助函数 ---

        /// <summary>
        /// 【复用】更新 GDB 路径显示，逻辑参考 Form_MTAddData
        /// </summary>
        private void UpdateGdbPathDisplay()
        {
            txtGdbPathDisplay.Text = $"{selectedDataSource}:{selectedGdbDirectory}";
        }

        /// <summary>
        /// 【复用】生成类名称，逻辑参考 Form_MTAddData
        /// </summary>
        /// <summary>
        /// 统一命名函数：TEM_L100_测点_20250405143022
        /// </summary>
        private string GenerateClassName(string baseName, string typeSuffix)
        {
            // 1. 提取原始文件名（不带路径、不带扩展名）
            string fileName = Path.GetFileNameWithoutExtension(baseName ?? "Unknown");

            // 2. 清理非法字符，只保留字母、数字、下划线
            string cleanName = Regex.Replace(fileName, @"[^\w]", "_");
            cleanName = Regex.Replace(cleanName, @"_+", "_").Trim('_');

            if (string.IsNullOrWhiteSpace(cleanName))
                cleanName = "Data";

            // 3. 生成时间戳（精确到秒，足够区分）
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // 4. 关键：统一前缀 TEM_ + 文件名 + 类型 + 时间戳
            string finalName = $"TEM_{cleanName}_{typeSuffix}_{timestamp}";

            // 5. MapGIS 类名最大长度 64（实测 64 没问题，保险起见留 1 位余量）
            if (finalName.Length > 64)
            {
                // 优先保留：TEM_ + 文件名 + 类型 + 部分时间戳
                int available = 64 - timestamp.Length + 1; // +1 是下划线
                string prefix = $"TEM_{cleanName}_{typeSuffix}_";
                if (prefix.Length > available)
                {
                    // 如果连前缀都放不下，就狠心截断文件名
                    string shortName = cleanName.Substring(0, Math.Max(5, available - prefix.Length + cleanName.Length));
                    prefix = $"TEM_{shortName}_{typeSuffix}_";
                }
                finalName = prefix + timestamp;
                if (finalName.Length > 64)
                    finalName = finalName.Substring(0, 64);
            }

            // 6. 去掉可能末尾残留的下划线（美观）
            finalName = finalName.TrimEnd('_');

            return finalName;
        }
        /// <summary>
        /// 【复用】检测标题行，逻辑参考 Form_MTAddData
        /// </summary>
        private bool IsHeaderLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            string firstWord = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (firstWord == null) return false;
            return firstWord.Any(c => !char.IsDigit(c) && c != '.' && c != '-' && c != 'e' && c != 'E');
        }

        /// <summary>
        /// 【复用】删除出错的类，逻辑参考 Form_MTAddData
        /// 【优化】添加删除日志
        /// </summary>
        private void TryDeleteClassWithStaticRemove(string dataSourceName, string gdbDirPath, string className, XClsType clsType)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName) || string.IsNullOrWhiteSpace(gdbDirPath) || gdbDirPath == "/" || string.IsNullOrWhiteSpace(className))
            {
                Console.WriteLine("尝试删除类：参数不完整，跳过操作");
                return;
            }

            string dbName = gdbDirPath.Trim('/').Split('/')[0];
            string databaseUrl = $"gdbp://{dataSourceName}/{dbName}";
            Console.WriteLine($"=== 尝试清理类 '{className}' 从数据库: {databaseUrl} ===");

            DataBase db = null;
            try
            {
                db = DataBase.OpenByURL(databaseUrl);
                if (db == null)
                {
                    Console.WriteLine("无法打开数据库，跳过删除操作");
                    return;
                }

                bool removeResult = false;
                string typeName = clsType == XClsType.SFCls ? "要素类" : "对象类";
                Console.WriteLine($"尝试调用静态 Remove(db, \"{className}\") 删除 {typeName}...");

                if (clsType == XClsType.SFCls)
                {
                    removeResult = SFeatureCls.Remove(db, className);
                }
                else if (clsType == XClsType.OCls)
                {
                    removeResult = ObjectCls.Remove(db, className);
                }

                if (removeResult)
                {
                    Console.WriteLine($"成功删除类: {className}");
                }
                else
                {
                    Console.WriteLine($"删除类失败或类不存在: {className}");
                }
            }
            catch (COMException comEx)
            {
                Console.WriteLine($"删除类 COM 错误 (码: {comEx.ErrorCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除类错误: {ex.Message}");
            }
            finally
            {
                if (db != null)
                {
                    try
                    {
                        db.Close();
                        Marshal.ReleaseComObject(db);
                        Console.WriteLine("数据库连接已关闭");
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 【复用】重置表单状态，逻辑参考 Form_MTAddData
        /// </summary>
        private void ResetFormState()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ResetFormState));
            }
            else
            {
                this.Cursor = Cursors.Default;
                btnOK.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        /// <summary>
        /// 【复用】查找地图，逻辑参考 Form_MTAddData
        /// 【优化】添加查找日志
        /// </summary>
        private Map FindMapByName(string mapName)
        {
            if (_hook == null || _hook.Document == null)
            {
                Console.WriteLine("FindMapByName: _hook 或 _hook.Document 为 null。");
                return null;
            }

            Maps maps = _hook.Document.GetMaps();
            for (int i = 0; i < maps.Count; i++)
            {
                Map currentMap = maps.GetMap(i);
                if (currentMap != null && currentMap.Name == mapName)
                {
                    Console.WriteLine($"找到地图: {mapName}");
                    return currentMap;
                }
            }

            Console.WriteLine($"FindMapByName: 未在项目中找到名为 '{mapName}' 的地图。");
            return null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

    }
}