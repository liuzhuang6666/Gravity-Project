using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry; // 包含 Dot, Dot3D, GeoPoints
using MapGIS.GeoMap;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MapGIS.UI.Controls; // GDBSelectFolderDialog
using MapGIS.PluginEngine;
using System.Drawing;

namespace MapGISPlugin3
{
    public partial class Form_CSAMTAddData : Form
    {
        private Point mousePoint = new Point();
        private IApplication _hook;
        private string selectedDataSource = "MapGisLocalPlus";
        private string selectedGdbDirectory = "/Temporary";

        public Form_CSAMTAddData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            UpdateGdbPathDisplay();
            // 初始化拖动和边框功能
            InitDragEvent();
        }

        #region --- 事件处理程序 ---

        // 选择观测数据文件
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择 CSAMT 观测数据文件 (.dat)";
                openFileDialog.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = openFileDialog.FileName;
                }
            }
        }

        // 【新增】选择发射源文件
        private void btnSelectTranFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择 CSAMT 发射源文件 (tran.dat)";
                openFileDialog.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTranFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnSelectGdbLocation_Click(object sender, EventArgs e)
        {
            using (GDBSelectFolderDialog gdbDialog = new GDBSelectFolderDialog())
            {
                gdbDialog.Title = "选择保存 GDB 位置";
                try { gdbDialog.SelectedPath = $"gdbp://{selectedDataSource}{selectedGdbDirectory}"; } catch { }
                if (gdbDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string fullSelectedPath = gdbDialog.SelectedPath;
                    Console.WriteLine($"GDB 返回路径: {fullSelectedPath}");
                    if (!string.IsNullOrEmpty(fullSelectedPath) && fullSelectedPath.StartsWith("gdbp://", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Uri uri = new Uri(fullSelectedPath);
                            string host = uri.Host; string path = uri.AbsolutePath;
                            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(path) && path != "/")
                            {
                                this.selectedDataSource = host; this.selectedGdbDirectory = path; UpdateGdbPathDisplay(); Console.WriteLine($"更新 GDB 位置: {selectedDataSource}:{selectedGdbDirectory}");
                            }
                            else { MessageBox.Show("请选择有效的数据库或目录。", "选择错误"); }
                        }
                        catch (Exception ex) { MessageBox.Show($"解析 GDB 路径出错: {ex.Message}", "错误"); }
                    }
                    else { MessageBox.Show("未获取有效 GDB 路径。", "错误"); }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnOK.Enabled = false; btnCancel.Enabled = false;

            string inputFile = txtInputFile.Text;
            string tranFile = txtTranFile.Text; // 【新增】发射源文件路径
            string dataSource = this.selectedDataSource;
            string gdbDirectory = this.selectedGdbDirectory;

            // 1. 验证输入
            if (string.IsNullOrWhiteSpace(inputFile) || !File.Exists(inputFile)) { MessageBox.Show("请选择有效的观测数据文件。", "错误"); ResetFormState(); return; }
            // 【新增】验证发射源文件
            if (string.IsNullOrWhiteSpace(tranFile) || !File.Exists(tranFile)) { MessageBox.Show("请选择有效的发射源文件。", "错误"); ResetFormState(); return; }
            if (string.IsNullOrWhiteSpace(dataSource) || string.IsNullOrWhiteSpace(gdbDirectory) || gdbDirectory == "/") { MessageBox.Show("请选择有效的 GDB 保存位置。", "错误"); ResetFormState(); return; }

            // 2. 生成名称和路径
            string baseName = Path.GetFileNameWithoutExtension(inputFile);
            string stationClassName = GenerateClassName(baseName, "测点");
            string transmitterClassName = GenerateClassName(baseName, "发射源"); // 【新增】
            string soundingClassName = GenerateClassName(baseName, "测深数据");

            string cleanGdbDirectory = "/" + gdbDirectory.Trim('/');
            string stationClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/sfcls/{stationClassName}";
            string transmitterClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/sfcls/{transmitterClassName}"; // 【新增】
            string soundingClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/ocls/{soundingClassName}";

            // 3. 数据容器
            var allDataRows = new List<CSAMTDataRow>();
            var uniqueStations = new Dictionary<string, StationInfo>();
            List<TxPointInfo> txPoints = new List<TxPointInfo>(); // 【新增】存储发射源点

            try
            {
                // --- 3.1 读取观测数据文件 (保持原有逻辑) ---
                Console.WriteLine("=== 开始读取观测数据文件 ===");
                using (StreamReader reader = new StreamReader(inputFile, Encoding.Default))
                {
                    string line; int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (lineNumber == 1 && IsHeaderLine(line)) { continue; }
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = Regex.Split(line.Trim(), @"\s+");
                        if (parts.Length < 7) { continue; }
                        try
                        {
                            CSAMTDataRow rowData = new CSAMTDataRow
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                Freq = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                Res = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                                Pha = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture)
                            };
                            allDataRows.Add(rowData);
                            string stationKey = $"{rowData.LineName}_{rowData.StationName}";
                            if (!uniqueStations.ContainsKey(stationKey)) { uniqueStations.Add(stationKey, new StationInfo { LineName = rowData.LineName, StationName = rowData.StationName, X = rowData.X, Y = rowData.Y }); }
                        }
                        catch { }
                    }
                }

                // --- 3.2 【新增】读取发射源文件 ---
                Console.WriteLine("=== 开始读取发射源文件 ===");
                using (StreamReader reader = new StreamReader(tranFile, Encoding.Default))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = Regex.Split(line.Trim(), @"\s+");
                        // 假设发射源文件每行是一个点：X Y [Z]，CSAMT通常有两个点(A, B)
                        if (parts.Length >= 2)
                        {
                            try
                            {
                                double x = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                                double y = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                                double z = 0.0;

                                // 【修正点】这里必须添加 NumberStyles.Any 参数
                                if (parts.Length > 2)
                                {
                                    double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out z);
                                }

                                txPoints.Add(new TxPointInfo { X = x, Y = y, Z = z });
                                // CSAMT 只需要两个点 (A, B)
                                if (txPoints.Count >= 2) break;
                            }
                            catch (Exception ex) { Console.WriteLine($"解析发射源行失败: {ex.Message}"); }
                        }
                    }
                }

                if (txPoints.Count < 2)
                {
                    MessageBox.Show("发射源文件格式错误或数据不足，未找到两个有效的坐标点(A, B)。", "数据错误");
                    ResetFormState();
                    return;
                }

                Console.WriteLine($"读取完成: {allDataRows.Count} 条观测记录, {uniqueStations.Count} 个测点, {txPoints.Count} 个发射源点。");
                if (allDataRows.Count == 0 || uniqueStations.Count == 0)
                {
                    MessageBox.Show("未读取到有效观测数据。", "错误");
                    ResetFormState();
                    return;
                }
            }
            catch (Exception ex) { MessageBox.Show($"读取文件失败: {ex.Message}", "错误"); Console.WriteLine($"文件读取异常: {ex}"); ResetFormState(); return; }

            // 4. 定义 COM 对象和状态标志
            SFeatureCls stationSfc = null;
            SFeatureCls transmitterSfc = null; // 【新增】
            ObjectCls soundingOcls = null;

            bool stationClassCreated = false;
            bool transmitterClassCreated = false; // 【新增】
            bool soundingClassCreated = false;

            bool stationAttached = false;
            bool transmitterAttached = false; // 【新增】
            bool tableAttached = false;

            try
            {
                // -- 5.1 创建 CSAMT 测点要素类 --
                Console.WriteLine($"创建测点类: {stationClassUrl}");
                stationSfc = new SFeatureCls();
                if (stationSfc.Create(stationClassUrl, GeomType.Pnt) <= 0) { throw new Exception($"创建测点要素类失败..."); }
                stationClassCreated = true;

                Fields stationFields = new Fields();
                stationFields.AppendField(new Field { FieldName = "测线号", FieldType = FieldType.FldString, MskLength = 50 });
                stationFields.AppendField(new Field { FieldName = "测点号", FieldType = FieldType.FldString, MskLength = 50 });
                stationFields.AppendField(new Field { FieldName = "X坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                stationFields.AppendField(new Field { FieldName = "Y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                if (stationSfc.UpdateFields(stationFields) <= 0) { throw new Exception("更新测点类字段失败。"); }

                Console.WriteLine("填充测点类...");
                stationSfc.BeginBatch(BatchType.Append);
                Record stationRecord = new Record { Fields = stationFields };
                foreach (var stationInfo in uniqueStations.Values)
                {
                    Dot3D pnt3D = new Dot3D(stationInfo.X, stationInfo.Y, 0.0);
                    stationRecord["测线号"] = stationInfo.LineName; stationRecord["测点号"] = stationInfo.StationName; stationRecord["X坐标"] = stationInfo.X; stationRecord["Y坐标"] = stationInfo.Y;
                    GeoPoints currentPnts = new GeoPoints();
                    currentPnts.Append(pnt3D);
                    stationSfc.Append(currentPnts, stationRecord, null);
                }
                stationSfc.EndBatch();

                // -- 5.2 【新增】创建 CSAMT 发射源要素类 (两个点) --
                Console.WriteLine($"创建发射源类: {transmitterClassUrl}");
                transmitterSfc = new SFeatureCls();
                if (transmitterSfc.Create(transmitterClassUrl, GeomType.Pnt) <= 0) { throw new Exception($"创建发射源要素类失败..."); }
                transmitterClassCreated = true;

                Fields tranFields = new Fields();
                tranFields.AppendField(new Field { FieldName = "点名", FieldType = FieldType.FldString, MskLength = 10 });
                tranFields.AppendField(new Field { FieldName = "X", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                tranFields.AppendField(new Field { FieldName = "Y", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                tranFields.AppendField(new Field { FieldName = "Z", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                if (transmitterSfc.UpdateFields(tranFields) <= 0) { throw new Exception("更新发射源类字段失败。"); }

                Console.WriteLine("填充发射源类...");
                transmitterSfc.BeginBatch(BatchType.Append);
                Record tranRecord = new Record { Fields = tranFields };

                string[] pointNames = { "A", "B" };
                for (int i = 0; i < Math.Min(txPoints.Count, 2); i++)
                {
                    var pt = txPoints[i];
                    Dot3D pnt3D = new Dot3D(pt.X, pt.Y, pt.Z);
                    tranRecord["点名"] = pointNames[i];
                    tranRecord["X"] = pt.X; tranRecord["Y"] = pt.Y; tranRecord["Z"] = pt.Z;
                    GeoPoints currentPnts = new GeoPoints();
                    currentPnts.Append(pnt3D);
                    transmitterSfc.Append(currentPnts, tranRecord, null);
                }
                transmitterSfc.EndBatch();

                // -- 5.3 创建 CSAMT 测深数据表 (ObjectCls) --
                Fields soundingFields = new Fields();
                soundingFields.AppendField(new Field { FieldName = "测线编号", FieldType = FieldType.FldString, MskLength = 50 });
                soundingFields.AppendField(new Field { FieldName = "测点编号", FieldType = FieldType.FldString, MskLength = 50 });
                soundingFields.AppendField(new Field { FieldName = "测点x坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                soundingFields.AppendField(new Field { FieldName = "测点y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                soundingFields.AppendField(new Field { FieldName = "频率", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingFields.AppendField(new Field { FieldName = "视电阻率", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingFields.AppendField(new Field { FieldName = "相位", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });

                soundingOcls = new ObjectCls();
                if (soundingOcls.Create(soundingClassUrl, soundingFields) <= 0) { throw new Exception($"创建测深数据表失败"); }
                soundingClassCreated = true;

                Console.WriteLine("填充数据表...");
                soundingOcls.BeginBatch(BatchType.Append);
                Record soundingRecord = new Record { Fields = soundingFields };
                foreach (var rowData in allDataRows)
                {
                    soundingRecord["测线编号"] = rowData.LineName;
                    soundingRecord["测点编号"] = rowData.StationName;
                    soundingRecord["测点x坐标"] = rowData.X;
                    soundingRecord["测点y坐标"] = rowData.Y;
                    soundingRecord["频率"] = rowData.Freq;
                    soundingRecord["视电阻率"] = rowData.Res;
                    soundingRecord["相位"] = rowData.Pha;
                    soundingOcls.Append(null, soundingRecord, null);
                }
                soundingOcls.EndBatch();

                // -- 5.5 加载图层 --
                Map targetMap = FindMapByName("电法数据");
                if (targetMap == null)
                {
                    targetMap = new Map(); targetMap.Name = "电法数据";
                    _hook.Document.GetMaps().Append(targetMap);
                }

                // 1. 加载 测点图层
                VectorLayer stationLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                stationAttached = stationLayer.AttachData(stationSfc);
                if (stationAttached)
                {
                    stationLayer.Name = stationClassName;
                    targetMap.Append(stationLayer);
                }

                // 2. 【新增】加载 发射源图层
                VectorLayer transmitterLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                transmitterAttached = transmitterLayer.AttachData(transmitterSfc);
                if (transmitterAttached)
                {
                    transmitterLayer.Name = transmitterClassName;
                    targetMap.Append(transmitterLayer);
                }

                // 3. 加载 测深表
                ObjectLayer soundingObjectLayer = new ObjectLayer();
                tableAttached = soundingObjectLayer.AttachData(soundingOcls);
                if (tableAttached)
                {
                    soundingObjectLayer.Name = soundingClassName;
                    soundingObjectLayer.URL = soundingClassUrl;
                    targetMap.Append(soundingObjectLayer);
                }

                // -- 成功 --
                MessageBox.Show($"导入成功！\n测点: {stationClassName}\n发射源: {transmitterClassName}\n数据: {soundingClassName}", "完成");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入出错: {ex.Message}", "错误");
                Console.WriteLine($"导入异常: {ex}");
                // 异常清理
                if (stationClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, stationClassName, XClsType.SFCls);
                if (transmitterClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, transmitterClassName, XClsType.SFCls);
                if (soundingClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, soundingClassName, XClsType.OCls);
            }
            finally
            {
                // 资源释放逻辑
                // 1. 测点 SFeatureCls
                if (!stationAttached && stationSfc != null) { try { if (stationSfc.HasOpen()) stationSfc.Close(); Marshal.ReleaseComObject(stationSfc); } catch { } }
                // 2. 发射源 SFeatureCls 【新增】
                if (!transmitterAttached && transmitterSfc != null) { try { if (transmitterSfc.HasOpen()) transmitterSfc.Close(); Marshal.ReleaseComObject(transmitterSfc); } catch { } }
                // 3. 测深 ObjectCls
                if (!tableAttached && soundingOcls != null) { try { if (soundingOcls.HasOpen()) soundingOcls.Close(); Marshal.ReleaseComObject(soundingOcls); } catch { } }

                ResetFormState();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        #endregion

        #region --- 辅助函数 ---
        private void UpdateGdbPathDisplay() { txtGdbPathDisplay.Text = $"{selectedDataSource}:{selectedGdbDirectory}"; }

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

            // 4. 关键：统一前缀 CSAMT_ + 文件名 + 类型 + 时间戳
            //    测点      → CSAMT_L100_测点_20250405143022
            //    发射源    → CSAMT_L100_发射源_20250405143022
            //    测深数据  → CSAMT_L100_测深数据_20250405143022
            string finalName = $"CSAMT_{cleanName}_{typeSuffix}_{timestamp}";

            // 5. MapGIS 类名最大长度 64（实测 64 没问题，保险起见留 1 位余量）
            if (finalName.Length > 64)
            {
                // 优先保留：CSAMT_ + 文件名 + 类型 + 部分时间戳
                int available = 64 - timestamp.Length + 1; // +1 是下划线
                string prefix = $"CSAMT_{cleanName}_{typeSuffix}_";
                if (prefix.Length > available)
                {
                    // 如果连前缀都放不下，就狠心截断文件名
                    string shortName = cleanName.Substring(0, Math.Max(5, available - prefix.Length + cleanName.Length));
                    prefix = $"CSAMT_{shortName}_{typeSuffix}_";
                }
                finalName = prefix + timestamp;
                if (finalName.Length > 64)
                    finalName = finalName.Substring(0, 64);
            }

            // 6. 去掉可能末尾残留的下划线（美观）
            finalName = finalName.TrimEnd('_');

            return finalName;
        }

        private void TryDeleteClassWithStaticRemove(string dataSourceName, string gdbDirPath, string className, XClsType clsType)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName) || string.IsNullOrWhiteSpace(gdbDirPath) || string.IsNullOrWhiteSpace(className)) return;
            string dbName = gdbDirPath.Trim('/').Split('/')[0]; string databaseUrl = $"gdbp://{dataSourceName}/{dbName}";
            DataBase db = null;
            try
            {
                db = DataBase.OpenByURL(databaseUrl);
                if (db == null) return;
                if (clsType == XClsType.SFCls) SFeatureCls.Remove(db, className);
                else if (clsType == XClsType.OCls) ObjectCls.Remove(db, className);
            }
            catch { }
            finally { if (db != null) { try { db.Close(); Marshal.ReleaseComObject(db); } catch { } } }
        }

        private bool IsHeaderLine(string line) { if (string.IsNullOrWhiteSpace(line)) return false; string firstWord = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(); if (firstWord == null) return false; return firstWord.Any(c => !char.IsDigit(c) && c != '.' && c != '-' && c != 'e' && c != 'E'); }

        private void ResetFormState() { if (this.InvokeRequired) { this.Invoke(new Action(ResetFormState)); } else { this.Cursor = Cursors.Default; btnOK.Enabled = true; btnCancel.Enabled = true; } }

        private Map FindMapByName(string mapName)
        {
            if (_hook == null || _hook.Document == null) return null;
            Maps maps = _hook.Document.GetMaps();
            for (int i = 0; i < maps.Count; i++) { Map currentMap = maps.GetMap(i); if (currentMap != null && currentMap.Name == mapName) return currentMap; }
            return null;
        }
        #endregion

        #region --- 辅助类 ---
        private class CSAMTDataRow { public string LineName { get; set; } public string StationName { get; set; } public double X { get; set; } public double Y { get; set; } public double Freq { get; set; } public double Res { get; set; } public double Pha { get; set; } }
        private class StationInfo { public string LineName { get; set; } public string StationName { get; set; } public double X { get; set; } public double Y { get; set; } }
        // 【新增】发射源点信息类
        private class TxPointInfo { public double X { get; set; } public double Y { get; set; } public double Z { get; set; } }
        #endregion

        #region --- 事件绑定 (Designer复用) ---
        private void btnSelectFile_Click_1(object sender, EventArgs e) { this.btnSelectFile_Click(sender, e); }
        private void btnSelectGdbLocation_Click_1(object sender, EventArgs e) { this.btnSelectGdbLocation_Click(sender, e); }
        private void btnOK_Click_1(object sender, EventArgs e) { this.btnOK_Click(sender, e); }
        private void btnCancel_Click_1(object sender, EventArgs e) { this.btnCancel_Click(sender, e); }
        private void button2_Click(object sender, EventArgs e) { this.Close(); }
        // 【新增】发射源按钮绑定
        private void btnSelectTranFile_Click_1(object sender, EventArgs e) { this.btnSelectTranFile_Click(sender, e); }
        #endregion

        #region --- 窗口拖动逻辑 ---
        private void InitDragEvent() { panel1.MouseDown += TitlePanel_MouseDown; panel1.MouseMove += TitlePanel_MouseMove; }
        private void TitlePanel_MouseDown(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { mousePoint.X = e.X; mousePoint.Y = e.Y; } }
        private void TitlePanel_MouseMove(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { this.Left = Control.MousePosition.X - mousePoint.X; this.Top = Control.MousePosition.Y - mousePoint.Y; } }
        #endregion
    }
}