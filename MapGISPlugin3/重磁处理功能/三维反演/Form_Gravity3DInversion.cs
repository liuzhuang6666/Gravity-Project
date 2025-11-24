using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.Drawing;
using MapGIS.PluginEngine;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoObjects.Geometry3D;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.Scene3D;
using MapGIS.PlugUtility; // 用于 GMRunTime
using MapGIS.G3DWorkSpace;

namespace MapGISPlugin3
{
    public partial class Form_Gravity3DInversion : Form
    {
        private IApplication m_Hook;
        private Point mousePoint = new Point();

        public Form_Gravity3DInversion(IApplication hook)
        {
            InitializeComponent();
            m_Hook = hook;
            InitTitleDrag();
        }

        private void Form_Gravity3DInversion_Load(object sender, EventArgs e)
        {
            if (m_Hook == null || m_Hook.Document == null) { Close(); return; }
            PopulateTreeView();
            textBoxGdbDirectory.Text = "/Temporary";
        }

        private void btnBrowseTerrain_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择地形数据文件 (Grid)";
            dlg.Filter = "Surfer Grid (*.grd)|*.grd|所有文件 (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxTerrainPath.Text = dlg.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // 1. 验证重力数据
                if (treeViewLayers.SelectedNode == null || !(treeViewLayers.SelectedNode.Tag is RasterLayer selectedLayer))
                {
                    MessageBox.Show("请先选择一个重力异常栅格图层。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string originalGrdPath = GetLocalPath(selectedLayer.URL);
                if (!File.Exists(originalGrdPath)) { MessageBox.Show("无法找到原始栅格文件。", "错误"); return; }

                // 2. 验证地形数据
                string terrainPath = textBoxTerrainPath.Text;
                if (!File.Exists(terrainPath)) { MessageBox.Show("地形文件不存在。", "错误"); return; }

                // 3. 验证输出参数
                string className = textBoxSavePath.Text;
                if (string.IsNullOrWhiteSpace(className)) { MessageBox.Show("请输入输出类名。", "提示"); return; }
                string gdbDir = textBoxGdbDirectory.Text;

                // 4. 执行算法 (Method = 0 For Gravity)
                string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Algorithm", "3DInversion", "a.exe");
                if (!File.Exists(exePath)) { MessageBox.Show($"算法程序未找到: {exePath}", "错误"); return; }

                // 参数: <filename> <terrain> <method=0>
                string args = $"\"{originalGrdPath}\" \"{terrainPath}\" 0";
                ExecuteAlgorithm(exePath, args);

                // 5. 转换结果
                string resultPath = Path.Combine(Path.GetDirectoryName(exePath), "result.dat");
                if (!File.Exists(resultPath) || new FileInfo(resultPath).Length == 0)
                {
                    MessageBox.Show("算法执行失败，未生成有效的结果文件。", "错误");
                    return;
                }

                string finalGdbPath = ConvertDatToSfclsInGdb(resultPath, className, gdbDir);
                if (!string.IsNullOrEmpty(finalGdbPath))
                {
                    AddLayerToView(finalGdbPath);
                    MessageBox.Show("重力三维反演成功！结果已加载。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show($"执行出错: {ex.Message}", "错误"); }
            finally { this.Cursor = Cursors.Default; }
        }

        // --- 核心逻辑：转换 DAT 到 SFCLS ---
        // --- 核心逻辑：转换 DAT 到 SFCLS (修正版：修复属性字段丢失问题) ---
        private string ConvertDatToSfclsInGdb(string datPath, string className, string gdbDirectory)
        {
            SFeatureCls sfc = null;
            // 处理 GDB 路径，去除可能的首尾斜杠
            string dbName = gdbDirectory.Trim('/');
            // 拼接完整的 GDBP 路径
            string gdbPath = $"gdbp://MapGisLocalPlus/{dbName}/sfcls/{className}";

            try
            {
                // 1. 创建简单的点要素类
                sfc = new SFeatureCls();
                int createRes = sfc.Create(gdbPath, GeomType.Pnt);
                if (createRes < 1)
                {
                    MessageBox.Show($"在 GDB 中创建类失败。\n路径: {gdbPath}\n错误码: {createRes}\n请检查类名是否已存在或GDB目录是否正确。", "GDB 创建错误");
                    return null;
                }

                // 2. 定义属性结构 (*** 关键修正：必须设置 MskLength 和 PointLength ***)
                Fields fields = new Fields();

                // Value 字段
                Field valueField = new Field();
                valueField.FieldName = "Value";
                valueField.FieldType = FieldType.FldDouble;
                valueField.MskLength = 15;  // 总长度
                valueField.PointLength = 6; // 小数位
                fields.AppendField(valueField);

                // X 字段 (坐标建议精度高一点)
                Field xField = new Field();
                xField.FieldName = "X";
                xField.FieldType = FieldType.FldDouble;
                xField.MskLength = 20;
                xField.PointLength = 8;
                fields.AppendField(xField);

                // Y 字段
                Field yField = new Field();
                yField.FieldName = "Y";
                yField.FieldType = FieldType.FldDouble;
                yField.MskLength = 20;
                yField.PointLength = 8;
                fields.AppendField(yField);

                // Z 字段
                Field zField = new Field();
                zField.FieldName = "Z";
                zField.FieldType = FieldType.FldDouble;
                zField.MskLength = 20;
                zField.PointLength = 8;
                fields.AppendField(zField);

                // 3. 更新属性结构到 GDB
                int updateRes = sfc.UpdateFields(fields);
                if (updateRes <= 0)
                {
                    MessageBox.Show($"更新属性结构失败，错误码: {updateRes}", "属性定义错误");
                    return null;
                }

                // 4. 批量写入数据
                sfc.BeginBatch(BatchType.Append);
                using (StreamReader reader = new StreamReader(datPath))
                {
                    string line;
                    // 读取文件内容
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 跳过空行或标题行 (如果 result.dat 有标题 "x y z value")
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        // 简单的非数字字符检查，防止读取标题行报错
                        if (char.IsLetter(line.Trim()[0])) continue;

                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 4) continue;

                        double x, y, z, val;
                        // 解析数据
                        if (double.TryParse(parts[0], out x) &&
                            double.TryParse(parts[1], out y) &&
                            double.TryParse(parts[2], out z) &&
                            double.TryParse(parts[3], out val))
                        {
                            // 构建几何点
                            Dot3D dot = new Dot3D(x, y, z);
                            GeoPoints pts = new GeoPoints();
                            pts.Append(dot);

                            // 构建属性记录
                            Record rec = new Record();
                            rec.Fields = fields; // 必须关联定义好的 Fields 结构
                            rec["Value"] = val;
                            rec["X"] = x;
                            rec["Y"] = y;
                            rec["Z"] = z;

                            // 添加到要素类
                            sfc.Append(pts, rec, null);
                        }
                    }
                }
                sfc.EndBatch();

                // 显式关闭以保存更改
                sfc.Close();

                return gdbPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"转换数据过程中发生异常: {ex.Message}", "转换错误");
                Console.WriteLine(ex.ToString());
                return null;
            }
            finally
            {
                // 资源清理
                if (sfc != null)
                {
                    try
                    {
                        if (sfc.HasOpen()) sfc.Close();
                        Marshal.ReleaseComObject(sfc);
                    }
                    catch { }
                }
            }
        }

        // --- 辅助方法 ---
        private void ExecuteAlgorithm(string exePath, string args)
        {
            ProcessStartInfo info = new ProcessStartInfo(exePath, args)
            {
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            using (Process p = Process.Start(info))
            {
                p.WaitForExit(3600000); // 1 Hour
                if (p.ExitCode != 0) throw new Exception($"算法退出码 {p.ExitCode}: {p.StandardError.ReadToEnd()}");
            }
        }

        private void AddLayerToView(string gdbPath)
        {
            try
            {
                Scenes scenes = m_Hook.Document.GetScenes();
                if (scenes != null && scenes.Count > 0)
                {
                    Scene targetScene = null;

                    // --- 修改开始：遍历查找指定名称的场景 ---
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        Scene tempScene = scenes.GetScene(i);
                        // 这里的名称必须与你左侧树状列表中的名称完全一致
                        if (tempScene.Name == "重力数据三维场景")
                        {
                            targetScene = tempScene;
                            break;
                        }
                    }

                    // 如果没找到名为“重力数据三维场景”的节点，为了防止报错，可以选择默认添加到第1个，或者直接返回
                    if (targetScene == null)
                    {
                        // 也可以选择弹窗提示没找到场景，这里暂时默认回退到索引0
                        targetScene = scenes.GetScene(0);
                    }
                    // --- 修改结束 ---

                    // 使用找到的 targetScene 添加图层
                    GMRunTime.AddLayer((DocumentItem)(object)targetScene, 3100, gdbPath, 3101, true);

                    try
                    {
                        SceneControl sc = m_Hook.WorkSpaceEngine.GetSceneControl(targetScene);
                        if (sc != null) { sc.Refresh(); Marshal.ReleaseComObject(sc); }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                // 建议加上异常捕获日志，方便调试
                Console.WriteLine("AddLayerToView Error: " + ex.Message);
            }
        }

        private void PopulateTreeView()
        {
            treeViewLayers.Nodes.Clear();
            Document doc = m_Hook.Document;
            if (doc == null) return;
            Maps maps = doc.GetMaps();
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps.GetMap(i);
                TreeNode root = new TreeNode(map.Name);
                treeViewLayers.Nodes.Add(root);
                AddLayers(root, map);
            }
            treeViewLayers.ExpandAll();
        }

        private void AddLayers(TreeNode parent, Map map)
        {
            for (int i = 0; i < map.LayerCount; i++) ProcessLayer(parent, map.get_Layer(i));
        }

        private void ProcessLayer(TreeNode parent, MapLayer layer)
        {
            if (layer is RasterLayer)
            {
                TreeNode node = new TreeNode(layer.Name);
                node.Tag = layer;
                parent.Nodes.Add(node);
            }
            else if (layer is GroupLayer gl)
            {
                TreeNode group = new TreeNode(gl.Name);
                parent.Nodes.Add(group);
                for (int i = 0; i < gl.Count; i++) ProcessLayer(group, gl.get_Item(i));
            }
        }

        private string GetLocalPath(string url)
        {
            try { Uri uri = new Uri(url); if (uri.IsFile) return uri.LocalPath; } catch { }
            return null;
        }

        private void treeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 检查用户是否选中了一个栅格图层
            if (e.Node.Tag is RasterLayer layer)
            {
                // 1. 获取原始文件名 (不含扩展名)
                string sourceName = layer.Name; // 默认使用图层名
                try
                {
                    if (!string.IsNullOrEmpty(layer.URL))
                    {
                        Uri uri = new Uri(layer.URL);
                        if (uri.IsFile)
                        {
                            sourceName = Path.GetFileNameWithoutExtension(uri.LocalPath);
                        }
                    }
                }
                catch { /* 如果URL解析失败，保持使用图层名 */ }

                // 2. 清理非法字符 (将空格、特殊符号替换为下划线，防止类名报错)
                sourceName = System.Text.RegularExpressions.Regex.Replace(sourceName, @"[^\w]", "_");

                // 3. 生成时间戳
                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // 4. 组合类名: [文件名]_Gra_Inv_[时间戳]
                textBoxSavePath.Text = $"{sourceName}_Gra_Inv_{timeStamp}";
            }
            else
            {
                // 如果选中的不是栅格图层(比如选了地图节点)，清空输入框
                textBoxSavePath.Text = "";
            }
        }

        private void InitTitleDrag()
        {
            panel1.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) mousePoint = new Point(e.X, e.Y); };
            panel1.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) { Left = Control.MousePosition.X - mousePoint.X; Top = Control.MousePosition.Y - mousePoint.Y; } };
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
        private void btnCancel_Click(object sender, EventArgs e) => Close();
    }
}