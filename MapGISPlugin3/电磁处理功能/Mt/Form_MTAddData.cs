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
using System.Drawing; // <-- 【关键】 必须引用

namespace MapGISPlugin3 // 确保命名空间正确
{
    public partial class Form_MTAddData : Form
    {
        private Point mousePoint = new Point();
        private IApplication _hook;
        private string selectedDataSource = "MapGisLocalPlus";
        private string selectedGdbDirectory = "/Temporary";

        public Form_MTAddData(IApplication hook)
        {
            InitializeComponent();
            _hook = hook;
            UpdateGdbPathDisplay();
            // 新增：初始化拖动和边框功能
            InitDragEvent();
        }

        #region --- 事件处理程序 ---

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "请选择 MT 数据文件 (.dat)";
                openFileDialog.Filter = "DAT 文件|*.dat|所有文件|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = openFileDialog.FileName;
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

            string inputFile = txtInputFile.Text; string dataSource = this.selectedDataSource; string gdbDirectory = this.selectedGdbDirectory;
            if (string.IsNullOrWhiteSpace(inputFile) || !File.Exists(inputFile)) {/*...*/ ResetFormState(); return; }
            if (string.IsNullOrWhiteSpace(dataSource) || string.IsNullOrWhiteSpace(gdbDirectory) || gdbDirectory == "/") {/*...*/ ResetFormState(); return; }
            string baseName = Path.GetFileNameWithoutExtension(inputFile); string stationClassName = GenerateClassName(baseName, "测点"); string soundingClassName = GenerateClassName(baseName, "测深数据");

            string cleanGdbDirectory = "/" + gdbDirectory.Trim('/');
            string stationClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/sfcls/{stationClassName}";
            string soundingClassUrl = $"gdbp://{dataSource}{cleanGdbDirectory}/ocls/{soundingClassName}";

            var allDataRows = new List<MTDataRow>(); var uniqueStations = new Dictionary<string, StationInfo>();
            try
            {
                // ... (文件读取逻辑，和您的一样，此处省略) ...
                using (StreamReader reader = new StreamReader(inputFile, Encoding.Default))
                {
                    string line; int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (lineNumber == 1 && IsHeaderLine(line)) { Console.WriteLine("跳过标题行。"); continue; }
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = Regex.Split(line.Trim(), @"\s+");
                        if (parts.Length < 9) { Console.WriteLine($"警告: 行 {lineNumber} 列数不足 ({parts.Length}列)，已跳过。"); continue; }
                        try
                        {
                            MTDataRow rowData = new MTDataRow
                            {
                                LineName = parts[0],
                                StationName = parts[1],
                                X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                                Period = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                                Res_TE = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                                Phase_TE = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture),
                                Res_TM = double.Parse(parts[7], System.Globalization.CultureInfo.InvariantCulture),
                                Phase_TM = double.Parse(parts[8], System.Globalization.CultureInfo.InvariantCulture)
                            };
                            allDataRows.Add(rowData);
                            string stationKey = $"{rowData.LineName}_{rowData.StationName}";
                            if (!uniqueStations.ContainsKey(stationKey)) { uniqueStations.Add(stationKey, new StationInfo { LineName = rowData.LineName, StationName = rowData.StationName, X = rowData.X, Y = rowData.Y }); }
                        }
                        catch (FormatException ex) { Console.WriteLine($"警告: 行 {lineNumber} 解析数字失败: {ex.Message}"); }
                        catch (Exception ex) { Console.WriteLine($"警告: 行 {lineNumber} 处理出错: {ex.Message}"); }
                    }
                }
                Console.WriteLine($"读取完成: {allDataRows.Count} 条记录, {uniqueStations.Count} 个测点。");
                if (allDataRows.Count == 0 || uniqueStations.Count == 0)
                {
                    MessageBox.Show("未读取到有效数据。请检查文件内容和格式。", "错误");
                    ResetFormState();
                    return;
                }
            }
            catch (Exception ex) { MessageBox.Show($"读取文件失败: {ex.Message}", "错误"); Console.WriteLine($"文件读取异常: {ex}"); ResetFormState(); return; }

            // 【!! 关键修改 1: 声明变量和布尔标志位 !!】
            // 我们需要这些变量在 'finally' 块中可见
            SFeatureCls stationSfc = null;
            ObjectCls soundingOcls = null;
            VectorLayer stationLayer = null;
            ObjectLayer soundingObjectLayer = null; // <-- 移到这里
            bool stationClassCreated = false;
            bool soundingClassCreated = false;

            bool attached = false; // <-- SFeatureCls 的附加状态
            bool tableAttached = false; // <-- ObjectCls 的附加状态

            try
            {
                // -- 5.1 创建 MT_Stations 点要素类 (SFeatureCls) --
                Console.WriteLine($"创建测点类: {stationClassUrl}");
                stationSfc = new SFeatureCls();
                if (stationSfc.Create(stationClassUrl, GeomType.Pnt) <= 0) { throw new Exception($"创建点要素类 '{stationClassName}' 失败..."); }
                stationClassCreated = true;

                // ... (省略 5.1 和 5.2 填充 SFeatureCls 字段和数据的代码, 与您的一致) ...
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
                    // 【改回】: 必须使用 Dot3D 和 GeoPoints 才能满足 IGeometry 接口
                    Dot3D pnt3D = new Dot3D(stationInfo.X, stationInfo.Y, 0.0);

                    stationRecord["测线号"] = stationInfo.LineName; stationRecord["测点号"] = stationInfo.StationName; stationRecord["X坐标"] = stationInfo.X; stationRecord["Y坐标"] = stationInfo.Y;

                    // 【改回】: 创建 GeoPoints 对象
                    GeoPoints currentPnts = new GeoPoints();
                    currentPnts.Append(pnt3D);

                    // 【改回】: 附加 GeoPoints 对象 (这能通过编译)
                    if (stationSfc.Append(currentPnts, stationRecord, null) <= 0)
                    { Console.WriteLine($"警告: 追加测点 '{stationInfo.StationName}' 失败。"); }
                }
                stationSfc.EndBatch();
                Console.WriteLine($"填充测点类完成。");


                // -- 5.3 创建 MT_Soundings 对象类 (ObjectCls) --
                // ... (省略 5.3 和 5.4 填充 ObjectCls 字段和数据的代码, 与您的一致) ...
                Fields soundingFields = new Fields();
                soundingFields.AppendField(new Field { FieldName = "测线编号", FieldType = FieldType.FldString, MskLength = 50 });
                soundingFields.AppendField(new Field { FieldName = "测点编号", FieldType = FieldType.FldString, MskLength = 50 });
                soundingFields.AppendField(new Field { FieldName = "测点x坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                soundingFields.AppendField(new Field { FieldName = "测点y坐标", FieldType = FieldType.FldDouble, MskLength = 20, PointLength = 8 });
                soundingFields.AppendField(new Field { FieldName = "周期", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingFields.AppendField(new Field { FieldName = "视电阻率_TE", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingFields.AppendField(new Field { FieldName = "相位_TE", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingFields.AppendField(new Field { FieldName = "视电阻率_TM", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingFields.AppendField(new Field { FieldName = "相位_TM", FieldType = FieldType.FldDouble, MskLength = 15, PointLength = 6 });
                soundingOcls = new ObjectCls();
                int createResult = soundingOcls.Create(soundingClassUrl, soundingFields);
                if (createResult <= 0) { throw new Exception($"创建对象类 '{soundingClassName}' 失败，返回: {createResult}"); }
                soundingClassCreated = true;
                Console.WriteLine("填充数据表...");
                soundingOcls.BeginBatch(BatchType.Append);
                Record soundingRecord = new Record { Fields = soundingFields };
                int soundingAppendCount = 0;
                foreach (var rowData in allDataRows)
                {
                    soundingRecord["测线编号"] = rowData.LineName;
                    soundingRecord["测点编号"] = rowData.StationName;
                    soundingRecord["测点x坐标"] = rowData.X;
                    soundingRecord["测点y坐标"] = rowData.Y;
                    soundingRecord["周期"] = rowData.Period;
                    soundingRecord["视电阻率_TE"] = rowData.Res_TE;
                    soundingRecord["相位_TE"] = rowData.Phase_TE;
                    soundingRecord["视电阻率_TM"] = rowData.Res_TM;
                    soundingRecord["相位_TM"] = rowData.Phase_TM;
                    if (soundingOcls.Append(null, soundingRecord, null) > 0) soundingAppendCount++;
                    else Console.WriteLine($"警告: 追加记录失败 (测点: {rowData.StationName})。");
                }
                soundingOcls.EndBatch();
                Console.WriteLine($"填充数据表完成: {soundingAppendCount}/{allDataRows.Count} 条。");


                // -- 5.5 加载图层 --
                Map targetMap = FindMapByName("电法数据");

                if (targetMap != null)
                {
                    Console.WriteLine("加载图层和表到 '电法数据' 地图...");

                    // 1. 加载 测点图层 (SFeatureCls) - 【保持对象附加，因为您说它工作正常】
                    stationLayer = new VectorLayer(VectorLayerType.SFclsLayer);
                    attached = stationLayer.AttachData(stationSfc); // <-- 记录附加状态
                    if (attached)
                    {
                        stationLayer.Name = stationClassName;
                        targetMap.Append(stationLayer);
                        Console.WriteLine("测点图层加载成功。");
                    }
                    else
                    {
                        MessageBox.Show("测点图层附加失败。", "警告");
                    }

                    // 2. 加载 测深表 (ObjectCls)
                    try
                    {
                        soundingObjectLayer = new ObjectLayer();
                        tableAttached = soundingObjectLayer.AttachData(soundingOcls); // <-- 记录附加状态
                        if (tableAttached)
                        {
                            soundingObjectLayer.Name = soundingClassName;

                            // -------------------------------------------------
                            // 【!! 关键修改 2: URL 补丁 !!】
                            // 这会强制 MapGIS 在保存 .mapx 时记住这个持久化路径
                            soundingObjectLayer.URL = soundingClassUrl;
                            // -------------------------------------------------

                            targetMap.Append(soundingObjectLayer);
                            Console.WriteLine("测深数据表加载成功。");
                        }
                        else
                        {
                            MessageBox.Show("测深数据表附加到地图失败。", "警告");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"添加测深表到地图时出错: {ex.Message}", "错误");
                    }
                }
                else
                {
                    MessageBox.Show("未在项目中找到名为 '电法数据' 的地图，图层和表未加载。\n(但数据已成功导入数据库)", "提示");
                }

                // -- 成功 --
                MessageBox.Show($"导入成功！\n图层: {stationClassName}\n表: {soundingClassName}\n位置: {dataSource}:{gdbDirectory}", "完成");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (COMException comEx) { MessageBox.Show($"MapGIS 操作失败: {comEx.Message}...", "GDB 错误"); Console.WriteLine($"GDB COM 异常: {comEx}"); if (stationClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, stationClassName, XClsType.SFCls); if (soundingClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, soundingClassName, XClsType.OCls); }
            catch (Exception ex) { MessageBox.Show($"导入出错: {ex.Message}", "错误"); Console.WriteLine($"导入异常: {ex}"); if (stationClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, stationClassName, XClsType.SFCls); if (soundingClassCreated) TryDeleteClassWithStaticRemove(dataSource, gdbDirectory, soundingClassName, XClsType.OCls); }
            finally
            {
                // 【!! 关键修改 3: 正确的 finally 释放逻辑 !!】
                // 我们使用 'attached' 和 'tableAttached' 标志位来决定是否释放 COM 对象

                // 1. 检查 SFeatureCls (stationSfc)
                // 只有当图层附加【失败】时 (attached == false)，我们才需要释放它
                if (!attached && stationSfc != null)
                {
                    try
                    {
                        Console.WriteLine("SFeatureCls [stationSfc] 未附加, 正在释放...");
                        if (stationSfc.HasOpen()) stationSfc.Close();
                        Marshal.ReleaseComObject(stationSfc);
                    }
                    catch (Exception ex) { Console.WriteLine($"释放 stationSfc (未附加) 出错: {ex.Message}"); }
                }
                else if (attached)
                {
                    Console.WriteLine("SFeatureCls [stationSfc] 已附加到图层, 将不释放。");
                }

                // 2. 检查 ObjectCls (soundingOcls)
                // 只有当图层附加【失败】时 (tableAttached == false)，我们才需要释放它
                if (!tableAttached && soundingOcls != null)
                {
                    try
                    {
                        Console.WriteLine("ObjectCls [soundingOcls] 未附加, 正在释放...");
                        if (soundingOcls.HasOpen()) soundingOcls.Close();
                        Marshal.ReleaseComObject(soundingOcls);
                    }
                    catch (Exception ex) { Console.WriteLine($"释放 soundingOcls (未附加) 出错: {ex.Message}"); }
                }
                else if (tableAttached)
                {
                    Console.WriteLine("ObjectCls [soundingOcls] 已附加到图层, 将不释放。");
                }

                Console.WriteLine("COM 对象已按需释放。");
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
            // ... (您的代码，不变) ...
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
                finalName = "MT_" + finalName.Substring(0, Math.Min(finalName.Length, 61));
            }
            finalName = finalName.TrimEnd('_');
            return finalName;
        }

        private bool IsValidClassName(string name) { if (string.IsNullOrWhiteSpace(name)) return false; return Regex.IsMatch(name, "^[a-zA-Z][a-zA-Z0-9_]*$"); }

        private void TryDeleteClassWithStaticRemove(string dataSourceName, string gdbDirPath, string className, XClsType clsType)
        {
            // ... (您的代码，不变) ...
            if (string.IsNullOrWhiteSpace(dataSourceName) || string.IsNullOrWhiteSpace(gdbDirPath) || gdbDirPath == "/" || string.IsNullOrWhiteSpace(className)) return;
            string dbName = gdbDirPath.Trim('/').Split('/')[0]; string databaseUrl = $"gdbp://{dataSourceName}/{dbName}"; Console.WriteLine($"尝试清理类 '{className}' 从数据库: {databaseUrl}"); DataBase db = null; try { db = DataBase.OpenByURL(databaseUrl); if (db == null) { Console.WriteLine("无法打开数据库"); return; } bool removeResult = false; string typeName = clsType == XClsType.SFCls ? "要素类" : "对象类"; Console.WriteLine($"尝试调用静态 Remove(db, \"{className}\") 删除 {typeName}..."); if (clsType == XClsType.SFCls) { removeResult = SFeatureCls.Remove(db, className); } else if (clsType == XClsType.OCls) { removeResult = ObjectCls.Remove(db, className); } if (removeResult) { Console.WriteLine($"成功删除类: {className}"); } else { Console.WriteLine($"删除类失败或类不存在: {className}"); } } catch (COMException comEx) { Console.WriteLine($"删除类 COM 错误 (码: {comEx.ErrorCode})"); } catch (Exception ex) { Console.WriteLine($"删除类错误: {ex.Message}"); } finally { if (db != null) { try { db.Close(); Marshal.ReleaseComObject(db); } catch { } } }
        }

        private bool IsHeaderLine(string line) { if (string.IsNullOrWhiteSpace(line)) return false; string firstWord = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(); if (firstWord == null) return false; return firstWord.Any(c => !char.IsDigit(c) && c != '.' && c != '-' && c != 'e' && c != 'E'); }

        private void ResetFormState() { if (this.InvokeRequired) { this.Invoke(new Action(ResetFormState)); } else { this.Cursor = Cursors.Default; btnOK.Enabled = true; btnCancel.Enabled = true; } }

        private Map FindMapByName(string mapName)
        {
            // ... (您的代码，不变) ...
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
                    return currentMap;
                }
            }
            Console.WriteLine($"FindMapByName: 未在项目中找到名为 '{mapName}' 的地图。");
            return null;
        }

        #endregion

        #region --- 辅助类 ---
        // (MTDataRow 和 StationInfo 类保持不变)
        private class MTDataRow { public string LineName { get; set; } public string StationName { get; set; } public double X { get; set; } public double Y { get; set; } public double Period { get; set; } public double Res_TE { get; set; } public double Phase_TE { get; set; } public double Res_TM { get; set; } public double Phase_TM { get; set; } }
        private class StationInfo { public string LineName { get; set; } public string StationName { get; set; } public double X { get; set; } public double Y { get; set; } }
        #endregion

        // 【保留】: 恢复您 Designer.cs 中需要的事件处理器
        private void btnSelectFile_Click_1(object sender, EventArgs e)
        {
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
        }

        private void btnSelectGdbLocation_Click_1(object sender, EventArgs e)
        {
            this.btnSelectGdbLocation.UseVisualStyleBackColor = true;
            this.btnSelectGdbLocation.Click += new System.EventHandler(this.btnSelectGdbLocation_Click);
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region --- 新增：窗口拖动与边框拉伸核心逻辑 ---

        /// <summary>
        /// 初始化标题栏拖动事件（绑定panel1）
        /// </summary>
        private void InitDragEvent()
        {
            panel1.MouseDown += TitlePanel_MouseDown;
            panel1.MouseMove += TitlePanel_MouseMove;
        }

        /// <summary>
        /// 标题栏按下：记录鼠标相对位置
        /// </summary>
        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousePoint.X = e.X;
                mousePoint.Y = e.Y;
            }
        }

        /// <summary>
        /// 标题栏移动：计算窗口新位置
        /// </summary>
        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left = Control.MousePosition.X - mousePoint.X;
                this.Top = Control.MousePosition.Y - mousePoint.Y;
            }
        }
        #endregion

    } // End Class Form_MT_Import
} // End namespace