#region 1. 必要的 using 引用
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoDataBase; // <--- 确保这一行存在
using MapGIS.GeoObjects.Geometry;
using System.Reflection;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.Scene3D;
using MapGIS.GISControl;
using System.Linq; // <--- 添加 Linq 命名空间
using System.Collections.Generic; // 为了使用 List<T> 等
using System.Runtime.InteropServices; // 为了释放 COM 对象
using MapGIS.G3DAttModeling; // 建模引擎
using MapGIS.Geologic.Model.VolumeModel; // GeoVolModelingParam
using MapGIS.GeoObjects.Geometry3D; // Dot3D, Rect3D
using MapGIS.G3DWorkSpace;

#endregion

namespace MapGISPlugin3
{
    public partial class Form_MagCorrelationImaging : Form
    {
        #region 2. 成员变量与公共属性
        private IApplication m_Hook;
        // 使用 RasterLayer 类型存储，避免重复转换
        public RasterLayer SelectedRasterLayer { get; private set; }
        public double Inclination { get; private set; }
        public double Declination { get; private set; }
        #endregion

        #region 3. 构造函数 与 窗体加载事件
        public Form_MagCorrelationImaging(IApplication hook)
        {
            InitializeComponent();
            m_Hook = hook;
        }
        private void Form_MagCorrelationImaging_Load(object sender, EventArgs e)
        {
            // 检查 MapGIS 环境
            if (m_Hook == null || m_Hook.Document == null)
            {
                ShowError("未能获取到当前地图文档！请先打开一个工程。", "环境错误");
                // 使用 BeginInvoke 确保 Load 事件结束后再关闭
                this.BeginInvoke(new MethodInvoker(Close));
                return;
            }

            // 填充图层树
            PopulateTreeView();

            // 设置 GDB 目录默认值并检查控件是否存在
            Control[] foundControls = this.Controls.Find("textBoxGdbDirectory", true);
            if (foundControls.Length > 0 && foundControls[0] is TextBox)
            {
                // TODO: 请务必确认 /Temporary 目录在 MapGisLocalPlus GDB 中存在！
                (foundControls[0] as TextBox).Text = "/Temporary";
                Console.WriteLine("[Form Load] 设置默认 GDB 目录为 /Temporary");
            }
            else
            {
                ShowError("错误：窗体缺少名为 'textBoxGdbDirectory' 的 TextBox 控件。\n请在设计器中添加。", "UI 配置错误");
                // 禁用确定按钮
                Control[] okButton = this.Controls.Find("btnOK", true);
                if (okButton.Length > 0) okButton[0].Enabled = false;
            }

            // (可选) 修改 textBoxSavePath 旁边 Label 的 Text
            Control[] savePathLabels = this.Controls.Find("labelSavePath", true); // 假设 Label 名为 labelSavePath
            if (savePathLabels.Length > 0 && savePathLabels[0] is System.Windows.Forms.Label)
            {
                (savePathLabels[0] as System.Windows.Forms.Label).Text = "输出类名:";
            }
        }
        #endregion

        #region 4. TreeView 填充逻辑 (增强了空检查和 COM 释放)
        private void PopulateTreeView()
        {
            treeViewLayers.Nodes.Clear();
            Document doc = m_Hook.Document;
            Maps maps = null; // 移到外部以便释放
            Map map = null;   // 移到外部以便释放

            if (doc == null) { ShowError("Document 对象为空，无法填充图层列表。", "环境错误"); return; }
            try
            {
                maps = doc.GetMaps();
                if (maps == null) { ShowWarning("未能获取地图列表 (Document.GetMaps 返回 null)。", "加载警告"); return; }

                Console.WriteLine($"[PopulateTreeView] 找到 {maps.Count} 个地图.");
                for (int i = 0; i < maps.Count; i++)
                {
                    map = maps.GetMap(i); // 获取 map 对象
                    if (map == null) { Console.WriteLine($"[PopulateTreeView] 警告: 第 {i} 个地图为 null."); continue; }
                    TreeNode mapNode = new TreeNode(map.Name);
                    treeViewLayers.Nodes.Add(mapNode);
                    Console.WriteLine($"[PopulateTreeView] 添加地图节点: {map.Name}");
                    AddLayersToNode(mapNode, map);
                    Marshal.ReleaseComObject(map); // 释放当前 map
                    map = null;
                }
                if (treeViewLayers.Nodes.Count > 0)
                {
                    treeViewLayers.ExpandAll();
                }
                else { ShowWarning("在当前文档中未找到任何地图或有效的栅格图层。", "数据缺失"); }
            }
            catch (Exception ex)
            {
                ShowError($"填充图层列表时出错: {ex.Message}", "列表加载失败");
                Console.WriteLine($"[PopulateTreeView] 错误: {ex.ToString()}");
            }
            finally
            {
                // 确保释放 COM 对象
                if (map != null) try { Marshal.ReleaseComObject(map); } catch { }
                if (maps != null) try { Marshal.ReleaseComObject(maps); } catch { }
            }
        }

        private void AddLayersToNode(TreeNode parentNode, Map map)
        {
            if (map == null || parentNode == null) return;
            Console.WriteLine($"[AddLayersToNode] 遍历地图 '{map.Name}' 下的 {map.LayerCount} 个图层...");
            MapLayer layer = null; // 移到外部以便释放
            try
            {
                for (int i = 0; i < map.LayerCount; i++)
                {
                    // 释放上一次循环的 layer
                    if (layer != null) { try { Marshal.ReleaseComObject(layer); } catch { } layer = null; }

                    layer = map.get_Layer(i);
                    if (layer != null)
                    {
                        ProcessLayer(parentNode, layer);
                    }
                    else { Console.WriteLine($"[AddLayersToNode] 地图 '{map.Name}' 第 {i} 层为 null。"); }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[AddLayersToNode] 遍历地图 '{map.Name}' 图层时出错: {ex.Message}"); }
            finally
            {
                if (layer != null) try { Marshal.ReleaseComObject(layer); } catch { }
            }
        }

        private void AddLayersToNode(TreeNode parentNode, GroupLayer groupLayer)
        {
            if (groupLayer == null || parentNode == null) return;
            Console.WriteLine($"[AddLayersToNode] 遍历组 '{groupLayer.Name}' 下 {groupLayer.Count} 层...");
            MapLayer layer = null; // 移到外部以便释放
            try
            {
                for (int i = 0; i < groupLayer.Count; i++)
                {
                    // 释放上一次循环的 layer
                    if (layer != null) { try { Marshal.ReleaseComObject(layer); } catch { } layer = null; }

                    layer = groupLayer.get_Item(i);
                    if (layer != null) { ProcessLayer(parentNode, layer); }
                    else { Console.WriteLine($"[AddLayersToNode] 组 '{groupLayer.Name}' 第 {i} 子层为 null。"); }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[AddLayersToNode] 遍历组 '{groupLayer.Name}' 图层时出错: {ex.Message}"); }
            finally
            {
                if (layer != null) try { Marshal.ReleaseComObject(layer); } catch { }
            }
        }

        private void ProcessLayer(TreeNode parentNode, MapLayer layer)
        {
            if (layer == null || parentNode == null) return;
            GroupLayer currentGroup = null; // 用于释放
            try
            {
                if (layer is GroupLayer)
                {
                    currentGroup = layer as GroupLayer;
                    Console.WriteLine($"[ProcessLayer] 处理组: {currentGroup.Name}");
                    if (!IsRecursiveGroup(parentNode, currentGroup))
                    { // 添加递归检查
                        TreeNode groupNode = new TreeNode(currentGroup.Name);
                        groupNode.Tag = currentGroup; // Tag group layer too
                        parentNode.Nodes.Add(groupNode);
                        AddLayersToNode(groupNode, currentGroup); // 递归
                    }
                    else { Console.WriteLine($"[ProcessLayer] 检测到递归组 '{currentGroup.Name}'。"); }
                }
                else if (layer is RasterLayer)
                {
                    Console.WriteLine($"[ProcessLayer] 添加栅格节点: {layer.Name}");
                    TreeNode layerNode = new TreeNode(layer.Name);
                    layerNode.Tag = layer; // 将图层对象存入 Tag
                    parentNode.Nodes.Add(layerNode);
                }
                else { /* Console.WriteLine($"[ProcessLayer] 跳过: {layer.Name} ..."); */ }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessLayer] 处理图层 '{layer?.Name ?? "null"}' 时出错: {ex.Message}");
            }
            finally
            {
                // 注意：存储在 Tag 中的 Layer 对象需要在 TreeNode 移除或窗体关闭时考虑释放
                // GroupLayer 对象也应在合适时机释放，但 ProcessLayer 本身不适合做这个操作
            }
        }

        /// <summary>
        /// 检查是否形成图层组递归。
        /// </summary>
        private bool IsRecursiveGroup(TreeNode parentNode, GroupLayer currentGroup)
        {
            TreeNode current = parentNode;
            int depth = 0; // 防止无限循环
            while (current != null && depth < 20) // 限制检查深度
            {
                if (object.ReferenceEquals(current.Tag, currentGroup)) return true;
                current = current.Parent;
                depth++;
            }
            return false;
        }
        #endregion

        #region 5. "确定"按钮事件 (最终版本 - 增强错误弹窗)
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            // ================== 步骤 1: 执行前预检查 ==================
            TreeNode selectedNode = treeViewLayers.SelectedNode;
            RasterLayer selectedRasterLayer = selectedNode?.Tag as RasterLayer;
            if (selectedRasterLayer == null)
            {
                ShowError("请在左侧树状图中选择一个有效的栅格图层节点！", "选择无效");
                this.Cursor = Cursors.Default;
                return;
            }

            double inclination, declination;
            if (!double.TryParse(textBoxInclination.Text, out inclination) || !double.TryParse(textBoxDeclination.Text, out declination))
            {
                ShowError("请输入有效的磁倾角和磁偏角数值！", "输入错误");
                this.Cursor = Cursors.Default;
                return;
            }

            string desiredClassName = textBoxSavePath.Text;
            if (string.IsNullOrWhiteSpace(desiredClassName))
            {
                ShowWarning("请先通过浏览按钮设置输出的类名！", "类名未设置");
                this.Cursor = Cursors.Default;
                return;
            }
            desiredClassName = System.Text.RegularExpressions.Regex.Replace(desiredClassName, @"[^\w]", "_");
            if (string.IsNullOrWhiteSpace(desiredClassName) || !char.IsLetter(desiredClassName[0]))
            {
                desiredClassName = "Result_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                ShowInfo($"清理后的类名无效或非字母开头，已生成默认名称: {desiredClassName}", "类名修正");
                Console.WriteLine($"[btnOK_Click] 警告: 已生成默认名称: {desiredClassName}");
                textBoxSavePath.Text = desiredClassName;
            }
            Console.WriteLine($"[btnOK_Click] 清理后的类名: {desiredClassName}");

            string gdbDirectory = "/Temporary";
            Control[] gdbDirBoxCtls = this.Controls.Find("textBoxGdbDirectory", true);
            TextBox gdbDirBox = (gdbDirBoxCtls.Length > 0 && gdbDirBoxCtls[0] is TextBox) ? (gdbDirBoxCtls[0] as TextBox) : null;
            if (gdbDirBox != null) { gdbDirectory = gdbDirBox.Text; }
            else { ShowWarning("未找到 GDB 目录输入框，将使用默认目录 /Temporary", "UI警告"); }

            if (string.IsNullOrWhiteSpace(gdbDirectory) || !gdbDirectory.StartsWith("/"))
            {
                ShowWarning("请输入有效的 GDB 目录路径 (必须以 '/' 开头)！\n例如: /Temporary 或 /MyData", "GDB 目录无效");
                if (gdbDirBox != null) gdbDirBox.Focus();
                this.Cursor = Cursors.Default;
                return;
            }
            gdbDirectory = gdbDirectory.Trim();

            this.SelectedRasterLayer = selectedRasterLayer;
            this.Inclination = inclination;
            this.Declination = declination;

            string tempGrdPath = "";
            string resultDatPath = "";
            string actualGdbPath = null;
            try
            {
                // ================== 步骤 2: 导出中间文件并执行算法 ==================
                tempGrdPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".grd");
                Console.WriteLine($"[btnOK_Click] 导出栅格到: {tempGrdPath}");
                if (!ExportRasterToGrd(this.SelectedRasterLayer, tempGrdPath)) { return; }
                Console.WriteLine($"[btnOK_Click] 导出 GRD 成功。");

                string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string algorithmDir = Path.Combine(pluginPath, "Algorithm", "MagCorrelationImaging");
                if (!Directory.Exists(algorithmDir)) { ShowError($"算法目录不存在！\n{algorithmDir}", "路径错误"); return; }
                string exePath = Path.Combine(algorithmDir, "a.exe");
                Console.WriteLine($"[btnOK_Click] 算法路径: {exePath}");
                if (!File.Exists(exePath)) { ShowError($"算法模块 (a.exe) 丢失！\n{exePath}", "文件丢失"); return; }

                Console.WriteLine($"[btnOK_Click] 执行算法...");
                ExecuteAlgorithm(exePath, tempGrdPath, this.Inclination, this.Declination);
                Console.WriteLine($"[btnOK_Click] 算法执行完毕。");

                // ================== 步骤 3: 强制转换DAT到SFCLS ==================
                resultDatPath = Path.Combine(Path.GetDirectoryName(exePath), "result.dat");
                Console.WriteLine($"[btnOK_Click] 检查 DAT 文件: {resultDatPath}");
                if (!File.Exists(resultDatPath)) { ShowError("算法执行完毕，但未找到结果文件 result.dat！", "结果文件丢失"); return; }
                FileInfo datInfo = new FileInfo(resultDatPath);
                if (datInfo.Length == 0) { ShowWarning("算法结果文件 result.dat 为空！", "结果文件为空"); return; }
                Console.WriteLine($"[btnOK_Click] 找到 DAT 文件，大小: {datInfo.Length} 字节。");

                Console.WriteLine($"[btnOK_Click] 调用 ConvertDatToSfclsInGdb...");
                actualGdbPath = ConvertDatToSfclsInGdb(resultDatPath, desiredClassName, gdbDirectory);

                if (string.IsNullOrEmpty(actualGdbPath)) { Console.WriteLine("[btnOK_Click] ConvertDatToSfclsInGdb 失败。"); return; }
                Console.WriteLine($"[btnOK_Click] ConvertDatToSfclsInGdb 成功，GDB 路径: {actualGdbPath}");

                // ================== 步骤 4: 加载图层 ==================
                Console.WriteLine($"[btnOK_Click] 加载图层: {actualGdbPath}");
                AddLayerToView(actualGdbPath);
                Console.WriteLine($"[btnOK_Click] 加载图层完成。");
                // ================== 步骤 4.5: 执行自动属性建模 ==================
                Console.WriteLine($"[btnOK_Click] 开始自动属性建模...");
                string dbName = gdbDirectory.Trim('/');
                string modelName = GetNameFromGdbPath(actualGdbPath);
                PerformAttributeModeling(actualGdbPath, dbName, modelName);
                // ===============================================================
                // ================== 步骤 5: 成功提示 ==================
                ShowInfo($"操作成功！\n点要素类已在 GDB 中创建并加载。\nGDB 路径: {actualGdbPath}", "操作完成");

                this.DialogResult = DialogResult.OK;

            }
            catch (TimeoutException timeEx) { Console.WriteLine($"[btnOK_Click] 超时异常: {timeEx.Message}"); this.DialogResult = DialogResult.Abort; }
            catch (Exception ex) { ShowError("处理过程中发生严重错误: " + ex.Message, "严重异常"); Console.WriteLine($"[btnOK_Click] 严重错误: {ex.ToString()}"); this.DialogResult = DialogResult.Abort; }
            finally
            {
                // 清理临时文件
                if (!string.IsNullOrEmpty(tempGrdPath) && File.Exists(tempGrdPath)) { try { File.Delete(tempGrdPath); } catch (Exception ex) { Console.WriteLine($"[btnOK_Click] 清理 GRD 文件失败: {ex.Message}"); } }
                this.Cursor = Cursors.Default;

                // 自动关闭处理
                if (this.Modal)
                {
                    // 模式对话框在 OK 或 Cancel 时会自动关闭
                    // 如果是 Abort，保留窗口让用户看到错误
                    if (this.DialogResult != DialogResult.OK && this.DialogResult != DialogResult.Cancel)
                    {
                        // 也许不需要做任何事，窗口会保持打开
                    }
                }
                else
                {
                    // 非模式对话框
                    if (this.DialogResult == DialogResult.OK || this.DialogResult == DialogResult.Cancel)
                        this.Close(); // 成功或取消时关闭
                }
            }
        }
        #endregion

        #region 6. 辅助函数 (ExportRasterToGrd, ExecuteAlgorithm, btnCancel, btnBrowse)

        /// <summary>
        /// 将 RasterLayer 导出为 Surfer 6 Text Grid (*.grd) 格式文件
        /// </summary>
        private bool ExportRasterToGrd(RasterLayer rasterLayer, string outputPath)
        {
            if (rasterLayer == null) { ShowError("内部错误：传递给 ExportRasterToGrd 的栅格图层为空！", "参数错误"); return false; }
            RasterDataSet rds = null; RasterBand band = null; StreamWriter writer = null;
            try
            {
                rds = rasterLayer.GetData() as RasterDataSet;
                if (rds == null) { ShowError("无法从选定的栅格图层获取栅格数据集！", "数据读取错误"); return false; }
                int width = rds.Width; int height = rds.Height;
                if (width <= 0 || height <= 0) { ShowError($"栅格数据的宽度({width})或高度({height})无效！", "数据错误"); return false; }
                Rect range = rds.GetMapRange();
                band = rds.GetRasterBand(1);
                if (band == null) { ShowError("无法获取栅格图层的第一个波段数据！", "数据读取错误"); return false; }
                double minVal = band.MinValue; double maxVal = band.MaxValue;
                if (minVal >= maxVal) { Console.WriteLine("[ExportRasterToGrd] 警告: Min/Max 值可能无效。"); }
                Console.WriteLine($"[ExportRasterToGrd] 栅格尺寸: {width}x{height}...");

                float[] pixelValues = new float[width * height];
                Console.WriteLine($"[ExportRasterToGrd] 读取像素值...");
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        try { pixelValues[y * width + x] = Convert.ToSingle(band.GetPixel(x, y)); }
                        catch (Exception pxEx) { Console.WriteLine($"[ExportRasterToGrd] 警告: 读取像素 ({x},{y}) 出错 - {pxEx.Message}. 设为 0."); pixelValues[y * width + x] = 0f; }
                    }
                    if (y % (height / 10.0) < 1 || y == height - 1) { Console.WriteLine($"[ExportRasterToGrd] ...已读取 {y + 1}/{height} 行"); } // 改进进度
                }
                Console.WriteLine($"[ExportRasterToGrd] 像素读取完成。");

                Console.WriteLine($"[ExportRasterToGrd] 写入 GRD: {outputPath}");
                writer = new StreamWriter(outputPath, false, Encoding.ASCII);
                writer.WriteLine("DSAA");
                writer.WriteLine($"{width} {height}");
                writer.WriteLine($"{range.XMin.ToString(System.Globalization.CultureInfo.InvariantCulture)} {range.XMax.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                writer.WriteLine($"{range.YMin.ToString(System.Globalization.CultureInfo.InvariantCulture)} {range.YMax.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                writer.WriteLine($"{minVal.ToString(System.Globalization.CultureInfo.InvariantCulture)} {maxVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                for (int y = 0; y < height; y++)
                {
                    StringBuilder sb = new StringBuilder();
                    int rowStartIndex = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        sb.Append(pixelValues[rowStartIndex + x].ToString("G", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    }
                    writer.WriteLine(sb.ToString().TrimEnd());
                    if (y % (height / 10.0) < 1 || y == height - 1) { Console.WriteLine($"[ExportRasterToGrd] ...已写入 {y + 1}/{height} 行"); } // 改进进度
                }
                writer.Flush();
                Console.WriteLine($"[ExportRasterToGrd] GRD 写入完成。");
                return true;
            }
            catch (COMException comEx) { ShowComError(comEx, "导出到 GRD 时发生 MapGIS COM 错误"); Console.WriteLine($"[ExportRasterToGrd] COM 错误: {comEx.ToString()}"); return false; }
            catch (IOException ioEx) { ShowError($"写入 GRD 文件失败: {ioEx.Message}", "文件写入错误"); Console.WriteLine($"[ExportRasterToGrd] IO 错误: {ioEx.ToString()}"); return false; }
            catch (Exception ex) { ShowError("导出到 GRD 文件时发生未知错误: " + ex.Message, "导出失败"); Console.WriteLine($"[ExportRasterToGrd] 未知错误: {ex.ToString()}"); return false; }
            finally
            {
                if (writer != null) { try { writer.Close(); writer.Dispose(); } catch { } } // 确保关闭和释放
                if (band != null) { try { Marshal.ReleaseComObject(band); } catch { } band = null; }
                if (rds != null) { try { Marshal.ReleaseComObject(rds); } catch { } rds = null; }
                Console.WriteLine("[ExportRasterToGrd] finally 块执行完毕。");
            }
        }

        /// <summary>
        /// 执行外部算法进程。
        /// </summary>
        private void ExecuteAlgorithm(string exePath, string grdFilePath, double inclination, double declination)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"\"{grdFilePath}\" {inclination} {declination}",
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            ShowInfo("即将开始执行外部关联反演算法...", "提示");
            Console.WriteLine($"[ExecuteAlgorithm] 执行: {exePath} {startInfo.Arguments}");
            Stopwatch stopwatch = Stopwatch.StartNew(); Process process = null;
            try
            {
                process = new Process { StartInfo = startInfo };
                StringBuilder output = new StringBuilder(); StringBuilder error = new StringBuilder();
                process.Start();
                output.Append(process.StandardOutput.ReadToEnd()); // 同步读取
                error.Append(process.StandardError.ReadToEnd()); // 同步读取
                bool exited = process.WaitForExit(60 * 60 * 1000); // 1 小时超时
                stopwatch.Stop(); Console.WriteLine($"[ExecuteAlgorithm] 耗时: {stopwatch.ElapsedMilliseconds} ms");

                if (exited)
                {
                    Console.WriteLine($"[ExecuteAlgorithm] 退出码: {process.ExitCode}");
                    if (output.Length > 0) Console.WriteLine($"[ExecuteAlgorithm] 标准输出:\n{output}"); else Console.WriteLine("[ExecuteAlgorithm] 标准输出为空。");
                    if (error.Length > 0)
                    {
                        Console.WriteLine($"[ExecuteAlgorithm] 标准错误:\n{error}");
                        ShowWarning("算法报告了信息或错误(详情请看日志):\n" + error.ToString().Substring(0, Math.Min(error.Length, 500)) + (error.Length > 500 ? "..." : ""), "算法输出");
                    }
                    if (process.ExitCode != 0)
                    {
                        string exitErrorMsg = $"外部算法执行失败，退出码: {process.ExitCode}。";
                        ShowError(exitErrorMsg + "\n详情请查看日志。", "算法执行失败");
                        throw new Exception(exitErrorMsg);
                    }
                }
                else
                {
                    Console.WriteLine($"[ExecuteAlgorithm] 错误: 超时！");
                    ShowError("算法执行超时（超过1小时），操作被中断。", "超时错误");
                    try { if (process != null && !process.HasExited) process.Kill(); } catch { }
                    throw new TimeoutException("外部算法执行超时（超过1小时），操作被中断。");
                }
            }
            catch (System.ComponentModel.Win32Exception winEx) { ShowError($"无法启动外部算法进程: {winEx.Message}", "启动失败"); throw; }
            catch (Exception ex) when (!(ex is TimeoutException)) { ShowError("执行外部算法时发生错误: " + ex.Message, "执行失败"); throw; }
            finally { if (process != null) process.Dispose(); Console.WriteLine("[ExecuteAlgorithm] finally 执行完毕。"); }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            if (!this.Modal) this.Close();
        }

        /// <summary>
        /// “浏览”按钮事件，用于设置输出的类名
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            Control[] savePathBoxCtls = this.Controls.Find("textBoxSavePath", true);
            TextBox savePathBox = (savePathBoxCtls.Length > 0 && savePathBoxCtls[0] is TextBox) ? (savePathBoxCtls[0] as TextBox) : null;

            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Title = "设置在GDB中输出的类名";
                saveDlg.Filter = "MapGIS Simple Feature Class (*.sfcls)|*.sfcls";
                saveDlg.DefaultExt = "sfcls";

                // --- 主要修改在这里 ---
                // 检查文本框中是否已有内容，如果没有，则生成一个带UID的默认名
                string defaultName;
                if (savePathBox != null && !string.IsNullOrWhiteSpace(savePathBox.Text))
                {
                    defaultName = savePathBox.Text; // 如果用户已经输入了名字，则使用用户的输入
                }
                else
                {
                    // 否则，生成一个带唯一ID的默认名称
                    string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                    defaultName = $"CorrelationImaging_Result_{uniqueId}";
                }
                // --- 修改结束 ---

                saveDlg.FileName = defaultName;

                if (saveDlg.ShowDialog(this) == DialogResult.OK)
                {
                    string className = Path.GetFileNameWithoutExtension(saveDlg.FileName);
                    if (savePathBox != null)
                    {
                        savePathBox.Text = className;
                        Console.WriteLine($"[btnBrowse_Click] 用户设置类名: '{className}'");
                    }
                    else
                    {
                        Console.WriteLine("[btnBrowse_Click] 警告: 未找到 textBoxSavePath 控件。");
                    }
                }
            }
        }

        #endregion

        #region 8. 属性建模 (新增)

        #region 8. 属性建模 (最终版)

        /// <summary>
        /// (建模辅助) 获取 MapGIS 格式的当前时间戳
        /// </summary>
        private GeoTIMESTAMP_STRU GetCurrentTimestamp()
        {
            DateTime now = DateTime.Now;
            GeoTIMESTAMP_STRU ts = new GeoTIMESTAMP_STRU();
            ts.Year = (short)now.Year;
            ts.Month = (short)now.Month;
            ts.Day = (short)now.Day;
            ts.Hour = (short)now.Hour;
            ts.Minute = (short)now.Minute;
            ts.Second = (short)now.Second;
            return ts;
        }

        /// <summary>
        /// 执行自动属性建模
        /// </summary>
        /// <param name="sfcGdbPath">输入的点要素类 GDBP 路径</param>
        /// <param name="dbName">GDB 数据库名称 (例如 "Temporary")</param>
        /// <param name="modelName">希望输出的模型名称</param>
        private void PerformAttributeModeling(string sfcGdbPath, string dbName, string modelName)
        {
            // 1. 准备建模引擎
            GeoGisVolDataModel dataModel = new GeoGisVolDataModel();

            // 2. 准备SFeatureCls和数据容器
            List<Dot3D> dotList = new List<Dot3D>();
            List<double> valueList = new List<double>();
            Rect3D dataRange = new Rect3D();

            // GeoGisGetVolDataByPntSfcls 需要的 'out' 参数
            int pWidth = 0, pHeight = 0;
            List<int> pImage = new List<int>();

            try
            {
                // === 3. 从 SFeatureCls 读取数据到内存 ===
                using (SFeatureCls sfc = new SFeatureCls())
                {
                    if (!sfc.Open(sfcGdbPath))
                    {
                        ShowError($"[建模] 无法打开刚创建的 SFeatureCls: {sfcGdbPath}", "建模失败");
                        return;
                    }

                    Console.WriteLine($"[建模] 正在从 {sfc.ClsName} 读取点数据...");

                    // 调用引擎的读取方法
                    int readResult = dataModel.GeoGisGetVolDataByPntSfcls(
                        sfc,         // 打开的 SFeatureCls
                        "Value",     // 我们硬编码的属性字段
                        ref dotList, // [ref] 输出的点列表
                        ref valueList, // [ref] 输出的值列表
                        ref dataRange, // [ref] 输出的数据范围
                        ref pWidth,    // [out] 虚拟参数 (我们不需要)
                        ref pHeight,   // [out] 虚拟参数 (我们不需要)
                        ref pImage     // [out] 虚拟参数 (我们不需要)
                    );

                    if (readResult <= 0 || dotList.Count == 0)
                    {
                        ShowError($"[建模] 从 SFeatureCls 读取数据失败。引擎返回代码: {readResult}", "建模失败");
                        return;
                    }
                    Console.WriteLine($"[建模] 数据读取完毕: {dotList.Count} 个点。");
                }

                // === 4. 准备建模参数 ===
                GeoVolModelingParam modelParams = new GeoVolModelingParam();
                modelParams.ModelName = modelName;
                modelParams.InterType = (GeoVolInterType)3; // 3 = InvDist (反距离加权)
                modelParams.SearchParam = new SearchRangeParam(); // 使用默认搜索参数
                modelParams.WorkArea = dataRange; // 使用刚读取到的真实范围
                modelParams.OrientDot = new Dot3D(dataRange.XMin, dataRange.YMin, dataRange.ZMin);

                // 【关键】设置时间戳，因为 API 需要它
                modelParams.CurrentTime = GetCurrentTimestamp();

                // 设置分辨率 (硬编码为 X/Y/Z 各 100 个单元格)
                int resolution = 3;
                double stepX = (dataRange.XMax - dataRange.XMin) / resolution;
                double stepY = (dataRange.YMax - dataRange.YMin) / resolution;
                double stepZ = (dataRange.ZMax - dataRange.ZMin) / resolution;
                if (stepX == 0) stepX = 1;
                if (stepY == 0) stepY = 1;
                if (stepZ == 0) stepZ = 1;
                modelParams.GridStep = new Dot3D(stepX, stepY, stepZ);

                // === 5. 准备输出数据库路径 ===
                string outputDbUrl = $"gdbp://MapGisLocalPlus/{dbName}";
                Console.WriteLine($"[建模] 建模参数准备完毕。输出到: {outputDbUrl}, 模型名: {modelName}");

                // === 6. 执行建模 ===
                string newLayerUrl = ""; // 接收新图层的 GDBP 路径
                Console.WriteLine("[建模] 调用 GeoGisVolDataToVoxelDataSet...");

                int modelResult = dataModel.GeoGisVolDataToVoxelDataSet(
                    dotList,       // 1. 点列表
                    valueList,     // 2. 值列表
                    outputDbUrl,   // 3. 数据库 URL
                    ref newLayerUrl, // 4. [ref] 新图层 URL
                    modelParams    // 5. 建模参数
                );

                // === 7. 处理结果 ===
                if (modelResult > 0 && !string.IsNullOrEmpty(newLayerUrl))
                {
                    Console.WriteLine($"[建模] 属性建模成功！新图层: {newLayerUrl}");
                    ShowInfo($"属性建模成功！\n已自动加载栅格元数据。", "建模完成");
                    // 自动加载新图层
                    AddLayerToView(newLayerUrl);
                }
                else
                {
                    ShowError($"属性建模失败！引擎返回代码: {modelResult}", "建模失败");
                    Console.WriteLine($"[建模] 属性建模失败！引擎返回代码: {modelResult}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"执行属性建模时发生严重错误: {ex.Message}", "建模异常");
                Console.WriteLine($"[建模] 严重错误: {ex.ToString()}");
            }
        }

        #endregion
        /// <summary>
        /// (建模辅助) 获取 MapGIS 格式的当前时间戳
        /// </summary>

        #region 7. 新增的核心功能函数 (ConvertDatToSfclsInGdb, AddLayerToView, GetNameFromGdbPath, ShowError, ShowWarning, ShowInfo, ShowComError)

        /// <summary>
        /// 将 DAT 文件转换为 GDB 中的 SFeatureCls。
        /// </summary>
        /// <summary>
        /// 将 DAT 文件转换为 GDB 中的 SFeatureCls (使用 SFeatureCls.Create(url) 方法)
        /// </summary>
        /// <param name="datPath">输入的DAT文件路径。</param>
        /// <param name="className">期望在 GDB 中创建的类名。</param>
        /// <param name="gdbDirectory">用户指定的 GDB 数据库/数据集路径 (例如 "/world" 或 "/Temporary")。</param>
        /// <returns>成功则返回创建的类的 GDB 路径，失败返回 null。</returns>
        private string ConvertDatToSfclsInGdb(string datPath, string className, string gdbDirectory)
        {
            // *** 移除了 Server srv = null; 和 DataBase db = null; ***
            SFeatureCls sfc = null; // 用于创建和写入的对象

            // GDB 内部相对路径，移除开头的 '/'
            string relativePath = $"{gdbDirectory.Trim('/')}/{className}";
            // 完整 GDBP 路径，格式：gdbp://(数据源名)/(数据库或数据集路径)/sfcls/(类名)
            string gdbPath = $"gdbp://MapGisLocalPlus/{relativePath}/sfcls/{className}";

            // *** 根据 SFeatureCls.Create(url, type) 示例，gdbPath 格式可能就是 gdbp://(数据源名)/(数据库名)/sfcls/(类名)
            // *** 我们需要重新确认用户输入的 gdbDirectory 到底代表什么。
            // *** 让我们采用 SFeatureCls.Create(url, type) 示例中的格式：
            // *** "gdbp://MapGISLocal/world/sfcls/" + LayerName
            // *** 这意味着 gdbDirectory 应该代表数据库名，例如 "world" 或 "示例数据" ***

            string dbName = gdbDirectory.Trim('/'); // 假设用户输入的是数据库名
            gdbPath = $"gdbp://MapGisLocalPlus/{dbName}/sfcls/{className}";


            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"[ConvertDatToSfclsInGdb] (新) 尝试使用 SFeatureCls.Create 直接创建...");
                Console.WriteLine($"[ConvertDatToSfclsInGdb] (新) 完整 GDBP 路径 (用于创建): {gdbPath}");

                // --- 移除了 Server/DataBase 连接逻辑 ---

                // --- 存在性检查和删除逻辑必须移除 (因为我们没有 db 对象) ---
                Console.WriteLine($"[ConvertDatToSfclsInGdb] ！！！注意：类存在性检查和删除逻辑已跳过 (使用 Create(url) 模式)。！！！");
                Console.WriteLine($"[ConvertDatToSfclsInGdb] ！！！如果目标类 '{className}' 在 '{dbName}/sfcls' 中已存在，创建将会失败。！！！");
                // --- 结束移除 ---

                // --- 核心创建逻辑 ---
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 创建新的 SFeatureCls() 实例...");
                sfc = new SFeatureCls(); // 1. 创建新实例

                Console.WriteLine($"[ConvertDatToSfclsInGdb] 调用 sfc.Create('{gdbPath}', GeomType.Pnt)...");
                int createResult = -1;
                try
                {
                    // 2. 在新实例上调用 Create，使用完整 GDBP 路径
                    createResult = sfc.Create(gdbPath, GeomType.Pnt);
                }
                catch (MissingMethodException mmEx) { ShowError($"致命错误：SFeatureCls 类没有 'Create(string, GeomType)' 方法...", "API 不匹配"); return null; }
                catch (COMException comEx) { ShowComError(comEx, $"在 GDB 中创建要素类时发生 COM 错误\n路径: {gdbPath}"); return null; }
                catch (Exception createEx) { ShowError($"调用 SFeatureCls.Create 时发生异常: {createEx.Message}\n请检查 GDBP 路径是否正确。", "GDB 创建异常"); return null; }

                if (createResult < 1)
                {
                    ShowError($"在 GDB 中创建简单要素类失败！\n(SFeatureCls.Create 返回 {createResult})\n尝试的路径: {gdbPath}\n\n可能原因:\n1. GDBP 路径格式错误 (数据库/sfcls/类名 结构是否正确？)。\n2. GDB 数据库 '{dbName}' (来自 GDB 目录框) 无效或不存在。\n3. 目标类 '{className}' 已存在。\n4. MapGIS 许可问题或权限不足。", "GDB 创建失败");
                    Console.WriteLine($"[ConvertDatToSfclsInGdb] 错误: sfc.Create('{gdbPath}') 失败，返回值: {createResult}");
                    return null;
                }
                Console.WriteLine($"[ConvertDatToSfclsInGdb] GDB 中创建 SFCLS 成功！Create 返回类 ID: {createResult}。");
                // --- 结束创建 ---

                if (sfc == null || !sfc.HasOpen()) { ShowError($"创建类后未能获取到有效的 SFeatureCls 对象！", "内部错误"); return null; }

                // --- 定义属性结构 ---
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 定义属性...");
                Fields fields = new Fields();
                Field valueField = new Field();
                valueField.FieldName = "Value"; valueField.FieldType = FieldType.FldDouble;
                valueField.MskLength = 15; valueField.PointLength = 6;
                fields.AppendField(valueField);
                Field xField = new Field();
                xField.FieldName = "X"; xField.FieldType = FieldType.FldDouble;
                xField.MskLength = 20; xField.PointLength = 8; // 为坐标提供更高精度
                fields.AppendField(xField);

                Field yField = new Field();
                yField.FieldName = "Y"; yField.FieldType = FieldType.FldDouble;
                yField.MskLength = 20; yField.PointLength = 8;
                fields.AppendField(yField);

                Field zField = new Field();
                zField.FieldName = "Z"; zField.FieldType = FieldType.FldDouble;
                zField.MskLength = 20; zField.PointLength = 8;
                fields.AppendField(zField);


                Console.WriteLine($"[ConvertDatToSfclsInGdb] 更新属性...");
                int updateFieldsResult = sfc.UpdateFields(fields);
                if (updateFieldsResult <= 0) { ShowError($"更新属性结构失败！返回值: {updateFieldsResult}", "属性更新失败"); return null; }
                Console.WriteLine($"[ConvertDatToSfclsInGdb] UpdateFields 完成。");

                // --- 开始批量写入 ---
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 开始批量写入..."); sfc.BeginBatch(BatchType.Append);
                int lineNum = 0; int successCount = 0; long skippedFormat = 0; long skippedParse = 0;
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 读取 DAT: {datPath}");
                using (StreamReader reader = new StreamReader(datPath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 4) { skippedFormat++; continue; }
                        double x, y, z, val;
                        if (!double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out x) ||
                            !double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out y) ||
                            !double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out z) ||
                            !double.TryParse(parts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                        { skippedParse++; continue; }

                        // *** 关键修正: 使用 GeoPoints ***
                        Dot3D dot = new Dot3D(x, y, z);
                        GeoPoints geoPoints = new GeoPoints();
                        geoPoints.Append(dot);
                        Record record = new Record();
                        record.Fields = fields; record["Value"] = val;
                        record["X"] = x;
                        record["Y"] = y;
                        record["Z"] = z;
                        sfc.Append(geoPoints, record, null); // 传入 GeoPoints
                        successCount++;
                    }
                }
                Console.WriteLine($"[ConvertDatToSfclsInGdb] DAT 读取完成。共{lineNum}行, 成功{successCount}行。");
                if (skippedFormat > 0) Console.WriteLine($"[ConvertDatToSfclsInGdb] 警告: 跳过格式错误 {skippedFormat} 行。");
                if (skippedParse > 0) Console.WriteLine($"[ConvertDatToSfclsInGdb] 警告: 跳过解析错误 {skippedParse} 行。");
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 结束批量写入..."); sfc.EndBatch();
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 关闭 SFCLS..."); sfc.Close(); sfc = null;
                sw.Stop(); Console.WriteLine($"[ConvertDatToSfclsInGdb] 转换成功！耗时: {sw.ElapsedMilliseconds} ms。");

                return gdbPath; // 返回创建成功的 GDB 路径
            }
            catch (Exception ex)
            {
                ShowError($"在 GDB 中转换创建时发生未处理异常: {ex.Message}", "GDB 转换异常");
                Console.WriteLine($"[ConvertDatToSfclsInGdb] 严重错误：{ex.ToString()}");
                return null;
            }
            finally
            {
                Console.WriteLine("[ConvertDatToSfclsInGdb] 开始执行 finally 块...");
                // *** 修正: 只释放 sfc ***
                if (sfc != null) { try { if (sfc.HasOpen()) sfc.Close(); Marshal.ReleaseComObject(sfc); Console.WriteLine("[Finally] Released sfc."); } catch { } }
                // *** 移除了 srv 和 db 的释放 ***
                Console.WriteLine("[ConvertDatToSfclsInGdb] finally 块执行完毕。");
            }
        }
        /// <summary>
        /// 使用 GMRunTime.AddLayer 将指定的图层添加到当前活动的三维场景 (Scene)。
        /// (最终版 - 采用官方示例逻辑)
        /// </summary>
        private void AddLayerToView(string layerGdbPath)
        {
            // 检查 MapGIS 环境是否有效
            if (m_Hook == null || m_Hook.Document == null) { ShowError("MapGIS 环境无效，无法加载图层。"); return; }
            Console.WriteLine($"[AddLayerToView] 使用 GMRunTime 添加图层到三维场景: {layerGdbPath}");
            // 检查传入的 GDB 路径是否为空
            if (string.IsNullOrEmpty(layerGdbPath)) { ShowWarning("传入的 GDB 路径为空，无法加载图层。"); return; }

            Scene scene = null;
            Scenes scenes = null;
            // 不再需要手动创建图层对象 (如 Vector3DLayer)

            try
            {
                // 获取文档中的所有三维场景
                scenes = m_Hook.Document.GetScenes();
                // 检查是否至少有一个场景存在
                if (scenes == null || scenes.Count == 0)
                {
                    ShowError("未找到任何三维场景。请先打开或创建一个三维场景以加载栅格体数据。", "加载错误");
                    Console.WriteLine("[AddLayerToView] 错误: 未找到三维场景。");
                    return; // 强制返回，不再尝试添加到二维地图
                }

                // 获取第一个三维场景
                scene = scenes.GetScene(0);
                if (scene == null)
                {
                    ShowError("获取第一个三维场景失败 (返回 null)。", "加载错误");
                    Console.WriteLine("[AddLayerToView] 错误: GetScene(0) 返回 null。");
                    return;
                }

                Console.WriteLine($"[AddLayerToView] 找到三维场景 '{scene.Name}'。正在调用 GMRunTime.AddLayer...");

                // --- 使用官方示例的方法 ---
                // GMRunTime 很可能是一个静态类。你需要确保包含了正确的 using 语句。
                // 根据 AttModelingForm.cs 的 using 语句，推测它在 MapGIS.PlugUtility 命名空间下。
                // 如果编译器找不到 GMRunTime，请添加对应的 using MapGIS.PlugUtility;
                try
                {
                    // 使用从示例代码中看到的神秘数字
                    GMRunTime.AddLayer(
                        (DocumentItem)(object)scene, // 将 Scene 对象作为父对象传递
                        3100, // 神秘数字，很可能指代 VoxelDataSet 数据类型
                        layerGdbPath, // 指向 VoxelDataSet 的 GDBP 路径 (URL)
                        3101, // 另一个神秘数字 (可能与分类或样式有关)
                        isVisible: true); // 设置图层可见

                    Console.WriteLine("[AddLayerToView] GMRunTime.AddLayer 调用成功。");

                    // 可选：刷新工作空间树 (这个操作通常比较安全)
                    try
                    {
                        m_Hook.WorkSpaceEngine.BeginUpdateTree();
                        m_Hook.WorkSpaceEngine.EndUpdateTree();
                        Console.WriteLine("[AddLayerToView] 工作空间树已刷新。");
                    }
                    catch (Exception treeEx) { Console.WriteLine($"[AddLayerToView] 刷新工作空间树时出错: {treeEx.Message}"); }

                    // 可选：刷新场景视图 (注意：这步仍有可能因为渲染大数据而变慢或卡顿)
                    SceneControl sceneCtrl = null;
                    try
                    {
                        sceneCtrl = m_Hook.WorkSpaceEngine.GetSceneControl(scene);
                        if (sceneCtrl != null)
                        {
                            Console.WriteLine("[AddLayerToView] 尝试刷新场景视图...");
                            // sceneCtrl.Reset();   // 缩放到全图范围 (可能会触发大量渲染，导致卡顿)
                            sceneCtrl.Refresh(); // 强制重绘 (也可能导致卡顿)
                            Console.WriteLine("[AddLayerToView] 场景视图刷新指令已发送。");
                        }
                        else
                        {
                            ShowWarning("未能获取 SceneControl 无法自动刷新三维视图。请手动刷新。", "刷新警告");
                        }
                    }
                    catch (COMException comEx)
                    {
                        ShowComError(comEx, "刷新三维场景视图时发生 COM 错误");
                    }
                    catch (Exception refreshEx)
                    {
                        ShowError($"刷新三维场景视图时发生错误: {refreshEx.Message}", "刷新错误");
                    }
                    finally
                    {
                        // 释放 SceneControl COM 对象
                        if (sceneCtrl != null) try { Marshal.ReleaseComObject(sceneCtrl); Console.WriteLine("[AddLayerToView] Released sceneCtrl."); } catch { }
                    }

                }
                catch (COMException comEx) // 捕获 GMRunTime.AddLayer 可能抛出的 COM 异常
                {
                    ShowComError(comEx, "调用 GMRunTime.AddLayer 添加图层时发生 COM 错误");
                    Console.WriteLine($"[AddLayerToView] GMRunTime.AddLayer() 抛出 COM 异常。");
                }
                catch (Exception ex) // 捕获 GMRunTime.AddLayer 可能抛出的其他异常
                {
                    ShowError($"调用 GMRunTime.AddLayer 添加图层时发生错误: {ex.Message}", "图层添加失败");
                    Console.WriteLine($"[AddLayerToView] GMRunTime.AddLayer() 抛出异常: {ex.ToString()}");
                }
            }
            catch (COMException comEx) // 捕获获取场景列表等准备步骤的 COM 错误
            {
                ShowComError(comEx, "在准备加载图层到场景时发生 MapGIS COM 错误");
            }
            catch (Exception ex) // 捕获其他意外错误
            {
                ShowError("自动加载图层到场景时发生失败: " + ex.Message, "加载错误");
            }
            finally
            {
                // 确保释放场景列表和场景 COM 对象
                if (scene != null) try { Marshal.ReleaseComObject(scene); Console.WriteLine("[AddLayerToView] Released scene."); } catch { }
                if (scenes != null) try { Marshal.ReleaseComObject(scenes); Console.WriteLine("[AddLayerToView] Released scenes."); } catch { }
                Console.WriteLine("[AddLayerToView] finally 块执行完毕。");
            }
        }
        /// <summary>
        /// 从 GDBP 路径中提取最后的类名。
        /// </summary>
        private string GetNameFromGdbPath(string gdbPath)
        {
            if (string.IsNullOrEmpty(gdbPath)) return "UnnamedLayer";
            try
            {
                Uri uri = new Uri(gdbPath);
                string segment = uri.Segments.LastOrDefault();
                if (!string.IsNullOrEmpty(segment)) { return segment.Trim('/'); }
            }
            catch { /* fallback */ }
            int lastSlash = gdbPath.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < gdbPath.Length - 1) { return gdbPath.Substring(lastSlash + 1); }
            try { return Path.GetFileNameWithoutExtension(gdbPath); } catch { }
            Console.WriteLine($"[GetNameFromGdbPath] 警告: 无法从 '{gdbPath}' 中解析出类名。");
            return "UnnamedLayer";
        }

        // *** 统一的弹窗辅助方法 ***
        /// <summary>
        /// 显示一个错误消息框 (Error Icon)
        /// </summary>
        private void ShowError(string message, string caption = "错误")
        {
            Console.WriteLine($"[Error] {caption}: {message}");
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        /// <summary>
        /// 显示一个警告消息框 (Warning Icon)
        /// </summary>
        private void ShowWarning(string message, string caption = "警告")
        {
            Console.WriteLine($"[Warning] {caption}: {message}");
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        /// <summary>
        /// 显示一个提示消息框 (Information Icon)
        /// </summary>
        private void ShowInfo(string message, string caption = "提示")
        {
            Console.WriteLine($"[Info] {caption}: {message}");
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /// <summary>
        /// 显示一个 MapGIS COM 错误消息框
        /// </summary>
        private void ShowComError(COMException comEx, string contextMessage = "发生 COM 错误")
        {
            string message = $"{contextMessage}:\n错误码: {comEx.ErrorCode}\n信息: {comEx.Message}";
            ShowError(message, "MapGIS COM 错误");
            Console.WriteLine($"[COM Error] Context: {contextMessage} - {comEx.ToString()}");
        }

        #endregion

        #region 8. 空的事件处理程序 (为 Designer.cs 保留)

        private void textBoxDeclination_TextChanged(object sender, EventArgs e)
        {
            // 为 Designer.cs 保留
        }

        private void treeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 为 Designer.cs 保留
            if (e.Node != null && e.Node.Tag is RasterLayer)
            {
                Console.WriteLine($"[Event] 用户选择了栅格图层: {e.Node.Text}");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // 为 Designer.cs 保留
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // 为 Designer.cs 保留
        }

        private void textBoxGdbDirectory_TextChanged(object sender, EventArgs e)
        {
            // 为 Designer.cs 保留
        }

        #endregion

        private void label4_Click(object sender, EventArgs e)
        {

        }
    } // End Class
}
#endregion