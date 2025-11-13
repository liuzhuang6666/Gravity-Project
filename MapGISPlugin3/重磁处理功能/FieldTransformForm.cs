using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapGIS.GISControl;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Info;
using MapGIS.RasCommonObj;
using MapGIS.RasAnalysis;
using MapGIS.Desktop.UI.Controls;
using MapGIS.GeoObjects.Geometry;
using System.IO;
using System.Runtime.InteropServices;
using MapGIS.PlugUtility;
using MapGIS.UI.Controls;
using MapGIS.PluginEngine;
using MapGIS.GeoObjects.Geometry3D; // For Dot3D if needed
using MapGIS.GeoObjects.Att;
using System.Diagnostics;
using System.Reflection;

namespace MapGISPlugin3
{
    public partial class Form1 : Form
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AddAnsi([MarshalAs(UnmanagedType.R8)] double res);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "test3")]
        public static extern void test3(AddAnsi add);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "polar")]
        public static extern void polar(int nx, int ny, double dx, double dy, double[] values, double od, double oi, AddAnsi add);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "contin")]
        public static extern void contin(int nx, int ny, double dx, double dy, double[] values, double od, double oi, double elev, AddAnsi add);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "dircomp")]
        public static extern void dircomp(int nx, int ny, double dx, double dy, double[] values, double od, double oi, double nd, double ni, AddAnsi add);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "deriv")]
        public static extern void deriv(int nx, int ny, double dx, double dy, double[] values, double od, double oi, double nd, double ni, AddAnsi add);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "second")]
        public static extern void second(int nx, int ny, double dx, double dy, double[] values, double od, double oi, int direction, AddAnsi add);

        [DllImport(@"D:\APrj\地球物理数据处理\magdpless.dll", EntryPoint = "second")]
        public static extern void compon(int nx, int ny, double dx, double dy, double[] values, double od, double oi, int direction, AddAnsi add);

        // 【修改】移除未使用的成员：activeMap 和 m_geodaba（避免潜在释放问题）
        // private Map activeMap = new Map();
        // private DataBase m_geodaba;

        private Document m_tempDoc = new Document(); // 【修改】使用一个临时 Document 管理插件内部地图
        private Map m_Map = new Map(); // 【修改】左侧临时地图（用于源数据可视化）
        private Map m_Map2 = new Map(); // 【修改】右侧临时地图（用于结果可视化）
        private Document m_Maindoc; // 主文档（从 hook 获取，不要 Dispose）
        private Map m_SourceMap; // 【新增】记录源层所属的主地图，用于添加结果
        private IApplication m_Hook;
        private bool m_ShowRasOrTin = true;
        private MapControl m_mtr = null;
        private MapControl m_mtr2 = null;
        private string _inputFilePath = null;

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
        private bool m_ShowReg = true;
        private bool m_ShowLine = true;
        private bool m_ShowSlopelin = false;
        private bool m_ShowAnn = true;
        private bool m_SymbolShow = true;
        private double m_LfZstep = 10;
        private float m_FSlopeYEps = 0;
        private float m_Length = 0;
        private SlopLinParam_Stru m_SlopLineParam = null;

        private int[] Imc = {601,603,498,500,436,408,391,233,190,184,154,122,106,33,31,
               127,391,128,392,393,136,149,150,442,443,186,444,179,180,445,189,190};

        public Form1(IApplication hook)
        {
            InitializeComponent();
            m_Hook = hook;
            if (m_Hook != null)
            {
                m_Maindoc = m_Hook.Document;
            }

            this.m_mtr = new MapControl();
            this.m_mtr.Dock = DockStyle.Fill;
            this.m_mtr.ShowRuler = false;
            this.m_mtr.ShowScrollBar = false;
            this.panelControl1.Controls.Add(m_mtr);

            this.m_mtr2 = new MapControl();
            this.m_mtr2.Dock = DockStyle.Fill;
            this.m_mtr2.ShowRuler = false;
            this.m_mtr2.ShowScrollBar = false;
            this.panelControl2.Controls.Add(m_mtr2);

            // 【修改】将临时地图添加到临时文档
            m_tempDoc.GetMaps().Append(m_Map);
            m_tempDoc.GetMaps().Append(m_Map2);

            this.dataTable.Columns.Add("等值线层");
            this.dataTable.Columns.Add("线层");
            this.dataTable.Columns.Add("区层");
            this.dataTable.Columns.Add("注记层");
            this.dataTable.Columns.Add("线层数据", typeof(LinInfo));
            this.dataTable.Columns.Add("区层数据", typeof(RegInfo));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.buttonEdit1.Properties.ReadOnly = true;
            this.buttonEdit1.Properties.Buttons[0].Enabled = false;
            this.buttonEdit1.BackColor = System.Drawing.SystemColors.ControlLight;

            this.m_mtr.ActiveMap = m_Map;
            this.m_mtr2.ActiveMap = m_Map2;
            this.m_mtr.ShowRuler = true;
            this.m_mtr2.ShowRuler = true;

            this.comboBox1.Items.Add("化极");
            this.comboBox1.Items.Add("三角");
            this.comboBox1.Items.Add("方向分量");
            this.comboBox1.Items.Add("导数");
            this.comboBox1.Items.Add("方向导数"); // 【新增】
            this.comboBox1.Items.Add("二次导数");
            this.comboBox1.Items.Add("分量");
            this.comboBox1.SelectedIndex = 0;


            // 【新增】在加载时就更新一次UI
            UpdateParameterUI();
        }

        private void Init()
        {
            // 【修改】使用 m_mtr.ActiveMap（临时地图）代替固定 m_Map
            if (this.m_mtr.ActiveMap.LayerCount == 0) return;
            if (this.m_mtr.ActiveMap.get_Layer(0) is RasterLayer)
            {
                RasterDataSet rasdataset = (this.m_mtr.ActiveMap.get_Layer(0) as RasterLayer).GetData() as RasterDataSet;
                m_Rect = rasdataset.GetMapRange();
                if (m_Rect == null) return;
                m_Min = rasdataset.GetRasterBand(m_BandNum).MinValue;
                m_Max = rasdataset.GetRasterBand(m_BandNum).MaxValue;
                if (m_Min.CompareTo(m_Max) == 0) return;
            }

            m_ScaleX = (m_Rect.XMax - m_Rect.XMin) / 1000.0;
            m_ScaleY = (m_Rect.YMax - m_Rect.YMin) / 1000.0;

            #region 初始化等值线参数（保持不变）
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
                if ((10 <= dk) && (dk <= 30))
                    break;
                if (dk > 30) m_LfZstep = m_LfZstep * 2.0;
                if (dk < 10) m_LfZstep = m_LfZstep * 0.5;
            } while (1 > 0);
            zdat = 0; k = (int)dk;
            if (m_Min < 0)
            {
                do
                {
                    zdat -= m_LfZstep;
                } while (zdat > m_Min);
                zdat += m_LfZstep;
            }
            else
            {
                do
                {
                    zdat += m_LfZstep;
                } while (zdat < m_Min);
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
                LinInfo linInfo = new LinInfo();
                linInfo.LibID = 0; linInfo.LinStyID = 1; linInfo.OutClr = new int[] { 1, 4, 3 }; linInfo.OutPenW = new float[] { 0.05F, 0.05F, 0.05F }; linInfo.XScale = linInfo.YScale = m_FSlopeYEps;
                RegInfo reginfo = new RegInfo();
                reginfo.OutPenW = 1; reginfo.Ovprnt = true; reginfo.PatClr = 3; reginfo.PatHeight = 10; reginfo.PatWidth = 10; reginfo.FillClr = Imc[i];
                this.dataTable.Rows.Add(lstfZdem[i].ToString("F2"), "", "", (i % 3 == 0) ? "YES" : "NO", linInfo, reginfo);
            }
        }

        private void 数据导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_Hook == null)
            {
                MessageBox.Show("严重错误：插件未能正确初始化，无法与主程序通信。", "初始化失败", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (m_Maindoc == null)
            {
                MessageBox.Show("操作失败：无法获取当前的地图文档。请确保您已在数据中心打开一个地图工程。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LayerSelectDialog layerSelectDialog = new LayerSelectDialog(m_Hook);
            if (layerSelectDialog.ShowDialog() == DialogResult.OK)
            {
                RasterLayer selectedLayerFromMainDoc = layerSelectDialog.SelectedRasterLayer;
                m_SourceMap = layerSelectDialog.SelectedMap; // 【新增】记录源地图
                if (selectedLayerFromMainDoc != null && m_SourceMap != null)
                {
                    string layerUrl = selectedLayerFromMainDoc.URL;
                    if (string.IsNullOrEmpty(layerUrl))
                    {
                        MessageBox.Show("选中的图层没有有效的URL，无法加载。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    RasterLayer newLayerForPlugin = new RasterLayer();
                    newLayerForPlugin.URL = layerUrl;

                    if (newLayerForPlugin.ConnectData())
                    {
                        newLayerForPlugin.Name = selectedLayerFromMainDoc.Name;
                        m_Map.RemoveAll(); // 只清空临时地图
                        m_Map.Append(newLayerForPlugin);

                        try
                        {
                            _inputFilePath = new Uri(layerUrl).LocalPath;
                            string inputDirectory = Path.GetDirectoryName(_inputFilePath);
                            string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(_inputFilePath);
                            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string newFileName = $"{inputFileNameWithoutExt}_{timestamp}.grd";
                            string newfilePath = Path.Combine(inputDirectory, newFileName);
                            this.buttonEdit1.Text = newfilePath;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"自动生成输出路径时出错: {ex.Message}", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.buttonEdit1.Text = "";
                            _inputFilePath = null;
                        }

                        if (this.m_mtr.ActiveMap.LayerCount != 0)
                            this.m_mtr.ActiveMap.get_Layer(0).State = m_ShowRasOrTin ? LayerState.Visible : LayerState.UnVisible;

                        this.m_mtr.Restore();

                        m_Tempsfclslin = null;
                        m_TempsfclsSlopelin = null;
                        m_Tempsfclsreg = null;
                        m_tempann = null;
                        m_LfZstep = 10;
                        Init();
                        DengZhiXianKeShiHua(m_Map, m_mtr);
                    }
                    else
                    {
                        MessageBox.Show("无法连接到栅格数据源，请检查文件是否有效。\n路径: " + layerUrl, "数据连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            layerSelectDialog.Dispose();
        }

        // 【修改】LayerSelectDialog 添加 SelectedMap
        private class LayerSelectDialog : Form
        {
            private TreeView treeViewLayers;
            private Button btnOK;
            private Button btnCancel;
            private IApplication m_Hook;
            public RasterLayer SelectedRasterLayer { get; private set; }
            public Map SelectedMap { get; private set; } // 【新增】

            public LayerSelectDialog(IApplication hook)
            {
                m_Hook = hook;
                InitializeComponents();
                PopulateTreeView();
            }

            private void InitializeComponents()
            {
                this.Text = "选择栅格图层";
                this.Size = new Size(400, 600);
                this.StartPosition = FormStartPosition.CenterParent;

                treeViewLayers = new TreeView();
                treeViewLayers.Dock = DockStyle.Fill;
                treeViewLayers.AfterSelect += TreeViewLayers_AfterSelect;

                btnOK = new Button();
                btnOK.Text = "确定";
                btnOK.Dock = DockStyle.Right;
                btnOK.Click += BtnOK_Click;

                btnCancel = new Button();
                btnCancel.Text = "取消";
                btnCancel.Dock = DockStyle.Right;
                btnCancel.Click += BtnCancel_Click;

                Panel buttonPanel = new Panel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 40;
                buttonPanel.Controls.Add(btnOK);
                buttonPanel.Controls.Add(btnCancel);

                this.Controls.Add(treeViewLayers);
                this.Controls.Add(buttonPanel);
            }

            private void PopulateTreeView()
            {
                treeViewLayers.Nodes.Clear();
                Document doc = m_Hook.Document;
                Maps maps = null;

                if (doc == null) { MessageBox.Show("Document 对象为空，无法填充图层列表。", "环境错误"); return; }
                try
                {
                    maps = doc.GetMaps();
                    if (maps == null) { MessageBox.Show("未能获取地图列表 (Document.GetMaps 返回 null)。", "加载警告"); return; }

                    for (int i = 0; i < maps.Count; i++)
                    {
                        Map map = maps.GetMap(i);
                        if (map == null) continue;
                        TreeNode mapNode = new TreeNode(map.Name);
                        mapNode.Tag = map; // 【新增】设置 Tag 为 Map
                        treeViewLayers.Nodes.Add(mapNode);
                        AddLayersToNode(mapNode, map);
                    }
                    if (treeViewLayers.Nodes.Count > 0)
                    {
                        treeViewLayers.ExpandAll();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"填充图层列表时出错: {ex.Message}", "列表加载失败");
                }
            }

            private void AddLayersToNode(TreeNode parentNode, Map map)
            {
                if (map == null || parentNode == null) return;
                for (int i = 0; i < map.LayerCount; i++)
                {
                    MapLayer layer = map.get_Layer(i);
                    if (layer != null)
                    {
                        ProcessLayer(parentNode, layer);
                    }
                }
            }

            private void AddLayersToNode(TreeNode parentNode, GroupLayer groupLayer)
            {
                if (groupLayer == null || parentNode == null) return;
                for (int i = 0; i < groupLayer.Count; i++)
                {
                    MapLayer layer = groupLayer.get_Item(i);
                    if (layer != null)
                    {
                        ProcessLayer(parentNode, layer);
                    }
                }
            }

            private void ProcessLayer(TreeNode parentNode, MapLayer layer)
            {
                if (layer == null || parentNode == null) return;
                if (layer is GroupLayer)
                {
                    GroupLayer currentGroup = layer as GroupLayer;
                    if (!IsRecursiveGroup(parentNode, currentGroup))
                    {
                        TreeNode groupNode = new TreeNode(currentGroup.Name);
                        groupNode.Tag = currentGroup;
                        parentNode.Nodes.Add(groupNode);
                        AddLayersToNode(groupNode, currentGroup);
                    }
                }
                else if (layer is RasterLayer)
                {
                    TreeNode layerNode = new TreeNode(layer.Name);
                    layerNode.Tag = layer;
                    parentNode.Nodes.Add(layerNode);
                }
            }

            private bool IsRecursiveGroup(TreeNode parentNode, GroupLayer currentGroup)
            {
                TreeNode current = parentNode;
                int depth = 0;
                while (current != null && depth < 20)
                {
                    if (object.ReferenceEquals(current.Tag, currentGroup)) return true;
                    current = current.Parent;
                    depth++;
                }
                return false;
            }

            private void TreeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
            {
                if (e.Node != null && e.Node.Tag is RasterLayer)
                {
                    btnOK.Enabled = true;
                }
                else
                {
                    btnOK.Enabled = false;
                }
            }

            private void BtnOK_Click(object sender, EventArgs e)
            {
                TreeNode selectedNode = treeViewLayers.SelectedNode;
                SelectedRasterLayer = selectedNode?.Tag as RasterLayer;

                // 【新增】向上查找所属 Map
                TreeNode node = selectedNode;
                while (node != null && (node.Tag == null || !(node.Tag is Map)))
                {
                    node = node.Parent;
                }
                if (node != null)
                {
                    SelectedMap = node.Tag as Map;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            private void BtnCancel_Click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        /// <summary>
        /// 【带有调试弹窗的版本】执行外部的“化极”算法 a.exe
        /// </summary>
        private bool ExecuteReduceToPoleAlgorithm(string inputGrdPath, double oi, double od, out string resultDatPath)
        {
            resultDatPath = null;
            try
            {
                // 1. 定位 a.exe 及其工作目录
                string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string algorithmDir = Path.Combine(pluginPath, "Algorithm", "ReduceToPole");
                string exePath = Path.Combine(algorithmDir, "a.exe");

                // --- 【调试点 1: 验证路径和参数】 ---
                // 在执行前，弹窗显示所有即将使用的路径和参数，让你确认它们是否正确。
                string debug_msg1 = "[调试] 即将执行算法，请检查以下信息：\n\n" +
                                    $"EXE 路径:\n{exePath}\n\n" +
                                    $"工作目录:\n{algorithmDir}\n\n" +
                                    $"输入 GRD 文件:\n{inputGrdPath}\n\n" +
                                    $"磁倾角(oi): {oi}\n" +
                                    $"磁偏角(od): {od}";
                MessageBox.Show(debug_msg1, "调试点 1: 执行前检查");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show($"算法模块 (a.exe) 丢失！\n请确保它位于以下路径：\n{exePath}", "文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 2. 准备进程启动信息
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{inputGrdPath}\" {oi} {od}",
                    WorkingDirectory = algorithmDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                // 3. 执行进程并等待结果
                using (Process process = new Process { StartInfo = startInfo })
                {
                    StringBuilder errorOutput = new StringBuilder();
                    process.Start();

                    errorOutput.Append(process.StandardError.ReadToEnd());
                    bool exited = process.WaitForExit(5 * 60 * 1000);

                    // --- 【调试点 2: 检查 a.exe 执行结果】 ---
                    // 弹窗显示 a.exe 的退出码和任何错误信息。这是最关键的调试步骤！
                    string debug_msg2 = "[调试] a.exe 执行完毕。\n\n" +
                                        $"是否正常退出: {exited}\n" +
                                        $"退出码 (ExitCode): {(exited ? process.ExitCode.ToString() : "N/A (超时)")}\n\n" +
                                        $"标准错误流信息 (如果有):\n-----\n{errorOutput}\n-----";
                    MessageBox.Show(debug_msg2, "调试点 2: 进程执行结果");

                    if (exited)
                    {
                        if (process.ExitCode != 0)
                        {
                            // 这里的弹窗保持，因为它是有用的最终错误提示
                            string errorMessage = $"外部算法 a.exe 执行失败，退出码: {process.ExitCode}。";
                            if (errorOutput.Length > 0)
                            {
                                errorMessage += "\n\n错误信息:\n" + errorOutput.ToString();
                            }
                            MessageBox.Show(errorMessage, "算法执行失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("算法执行超时（超过5分钟），操作被中断。", "超时错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (!process.HasExited) process.Kill();
                        return false;
                    }
                }

                // 4. 验证结果文件
                resultDatPath = Path.Combine(algorithmDir, "result.dat");

                // --- 【调试点 3: 检查 result.dat 文件状态】 ---
                // 在读取文件之前，检查它是否存在以及大小是否大于0。
                string debug_msg3;
                if (File.Exists(resultDatPath))
                {
                    long fileSize = new FileInfo(resultDatPath).Length;
                    debug_msg3 = "[调试] 检查结果文件。\n\n" +
                                 $"文件路径:\n{resultDatPath}\n\n" +
                                 "状态: 文件存在。\n" +
                                 $"大小: {fileSize} 字节。";
                }
                else
                {
                    debug_msg3 = "[调试] 检查结果文件。\n\n" +
                                 $"文件路径:\n{resultDatPath}\n\n" +
                                 "状态: 文件不存在！";
                }
                MessageBox.Show(debug_msg3, "调试点 3: 结果文件检查");

                if (!File.Exists(resultDatPath))
                {
                    MessageBox.Show("算法执行完毕，但未在预期位置找到结果文件 result.dat！", "结果文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("执行外部算法时发生严重错误: " + ex.Message, "执行异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        // 【新增】执行外部的“方向导数”算法 a.exe
        private bool ExecuteDirectionalDerivativeAlgorithm(string inputGrdPath, double ori, double ord, out string resultDatPath)
        {
            resultDatPath = null;
            try
            {
                // 1. 定位 a.exe 及其工作目录
                string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string algorithmDir = Path.Combine(pluginPath, "Algorithm", "DirectionalDerivative");
                string exePath = Path.Combine(algorithmDir, "a.exe");
                // --- 【调试点 1: 验证路径和参数】 ---
                string debug_msg1 = "[调试] 即将执行方向导数算法，请检查以下信息：\n\n" +
                                    $"EXE 路径:\n{exePath}\n\n" +
                                    $"工作目录:\n{algorithmDir}\n\n" +
                                    $"输入 GRD 文件:\n{inputGrdPath}\n\n" +
                                    $"方位角(ori): {ori}\n" +
                                    $"阶数(ord): {ord}";
                MessageBox.Show(debug_msg1, "调试点 1: 执行前检查");
                if (!File.Exists(exePath))
                {
                    MessageBox.Show($"算法模块 (a.exe) 丢失！\n请确保它位于以下路径：\n{exePath}", "文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                // 2. 准备进程启动信息
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{inputGrdPath}\" {ori} {ord}",
                    WorkingDirectory = algorithmDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                // 3. 执行进程并等待结果
                using (Process process = new Process { StartInfo = startInfo })
                {
                    StringBuilder errorOutput = new StringBuilder();
                    process.Start();
                    errorOutput.Append(process.StandardError.ReadToEnd());
                    bool exited = process.WaitForExit(5 * 60 * 1000); // 5分钟超时
                    // --- 【调试点 2: 检查 a.exe 执行结果】 ---
                    string debug_msg2 = "[调试] a.exe 执行完毕。\n\n" +
                                        $"是否正常退出: {exited}\n" +
                                        $"退出码 (ExitCode): {(exited ? process.ExitCode.ToString() : "N/A (超时)")}\n\n" +
                                        $"标准错误流信息 (如果有):\n-----\n{errorOutput}\n-----";
                    MessageBox.Show(debug_msg2, "调试点 2: 进程执行结果");
                    if (exited)
                    {
                        if (process.ExitCode != 0)
                        {
                            string errorMessage = $"外部算法 a.exe 执行失败，退出码: {process.ExitCode}。";
                            if (errorOutput.Length > 0)
                            {
                                errorMessage += "\n\n错误信息:\n" + errorOutput.ToString();
                            }
                            MessageBox.Show(errorMessage, "算法执行失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("算法执行超时（超过5分钟），操作被中断。", "超时错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (!process.HasExited) process.Kill();
                        return false;
                    }
                }
                // 4. 验证结果文件
                resultDatPath = Path.Combine(algorithmDir, "result.dat");
                // --- 【调试点 3: 检查 result.dat 文件状态】 ---
                string debug_msg3;
                if (File.Exists(resultDatPath))
                {
                    long fileSize = new FileInfo(resultDatPath).Length;
                    debug_msg3 = "[调试] 检查结果文件。\n\n" +
                                 $"文件路径:\n{resultDatPath}\n\n" +
                                 "状态: 文件存在。\n" +
                                 $"大小: {fileSize} 字节。";
                }
                else
                {
                    debug_msg3 = "[调试] 检查结果文件。\n\n" +
                                 $"文件路径:\n{resultDatPath}\n\n" +
                                 "状态: 文件不存在！";
                }
                MessageBox.Show(debug_msg3, "调试点 3: 结果文件检查");
                if (!File.Exists(resultDatPath))
                {
                    MessageBox.Show("算法执行完毕，但未在预期位置找到结果文件 result.dat！", "结果文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("执行外部算法时发生严重错误: " + ex.Message, "执行异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void DengZhiXianKeShiHua(Map mapToUse, MapControl mtrToUse)
        {
            if (m_ContourParamStrcT == null) return;
            // 清空地图中的图层，只留一个 raster layer
            while (mapToUse.LayerCount != 1)
            {
                mapToUse.Remove(1);
            }
            RasTraceContour traceContour = null;
            if (mtrToUse.ActiveMap.get_Layer(0) is RasterLayer)
            {
                RasterDataSet rasdataset = (mtrToUse.ActiveMap.get_Layer(0) as RasterLayer).GetData() as RasterDataSet;
                traceContour = new RasTraceContour(rasdataset, m_BandNum);
            }
            else
            {
                return;
            }
            int n = this.dataTable.Rows.Count;
            ZVelStrcT_Stru[] arrayZVelStrcT = new ZVelStrcT_Stru[n];
            for (int i = 0; i < n; i++)
            {
                ZVelStrcT_Stru ZVelStrcT = new ZVelStrcT_Stru();
                ZVelStrcT.linf = this.dataTable.Rows[i][4] as LinInfo;
                ZVelStrcT.rinf = this.dataTable.Rows[i][5] as RegInfo;
                ZVelStrcT.fZdem = i == n - 1 ? m_Max : Convert.ToDouble(this.dataTable.Rows[i][0]);
                ZVelStrcT.mskOn = (sbyte)((this.dataTable.Rows[i][3] as string) == "YES" ? 1 : 0);
                arrayZVelStrcT[i] = ZVelStrcT;
            }
            m_ContourParamStrcT.SetZVelBuf(arrayZVelStrcT);
            m_ContourParamStrcT.pContourNoteParam = m_ContourNoteParam;
            m_Tempsfclslin = null;
            m_TempsfclsSlopelin = null;
            m_Tempsfclsreg = null;
            m_tempann = null;
            if (m_Tempdaba == null)
                m_Tempdaba = DataBase.OpenTempDB();
            if (m_Tempdaba == null)
                return;
            m_Tempsfclslin = new SFeatureCls(m_Tempdaba);
            if (m_Tempsfclslin.Create(Guid.NewGuid().ToString(), GeomType.Lin, 0, 0, null) <= 0)
                return;
            if (m_ContourParamStrcT.bShowSlin)
            {
                m_TempsfclsSlopelin = new SFeatureCls(m_Tempdaba);
                if (m_TempsfclsSlopelin.Create(Guid.NewGuid().ToString(), GeomType.Lin, 0, 0, null) <= 0)
                    return;
            }
            if (m_ContourParamStrcT.bMakeReg == false)
            {
                m_Tempsfclsreg = new SFeatureCls(m_Tempdaba);
                if (m_Tempsfclsreg.Create(Guid.NewGuid().ToString(), GeomType.Reg, 0, 0, null) <= 0)
                    return;
            }
            if (m_ContourParamStrcT.bMapNote)
            {
                m_tempann = new AnnotationCls(m_Tempdaba);
                if (m_tempann.Create(Guid.NewGuid().ToString(), AnnType.Text, 0, 0, null) <= 0)
                    return;
            }
            traceContour.ShowProgressBar(true);
            int rtn = traceContour.RsTraceContour(m_ContourParamStrcT, m_Tempsfclslin, m_TempsfclsSlopelin, m_Tempsfclsreg, m_tempann, 1024, false, m_ClipLine);
            traceContour.Dispose();
            if (m_Tempsfclsreg == null)
            {
                m_Tempsfclsreg = new SFeatureCls(m_Tempdaba);
                if (m_Tempsfclsreg.Create(Guid.NewGuid().ToString(), GeomType.Reg, 0, 0, null) <= 0)
                    return;
            }
            if (m_TempsfclsSlopelin == null)
            {
                m_TempsfclsSlopelin = new SFeatureCls(m_Tempdaba);
                if (m_TempsfclsSlopelin.Create(Guid.NewGuid().ToString(), GeomType.Lin, 0, 0, null) <= 0)
                    return;
            }
            if (m_tempann == null)
            {
                m_tempann = new AnnotationCls(m_Tempdaba);
                if (m_tempann.Create(Guid.NewGuid().ToString(), AnnType.Text, 0, 0, null) <= 0)
                    return;
            }
            m_Tempsfclslin.ScaleX = m_ScaleX;
            m_Tempsfclslin.ScaleY = m_ScaleY;
            m_TempsfclsSlopelin.ScaleX = m_ScaleX;
            m_TempsfclsSlopelin.ScaleY = m_ScaleY;
            m_Tempsfclsreg.ScaleX = m_ScaleX;
            m_Tempsfclsreg.ScaleY = m_ScaleY;
            if (rtn > 0)
            {
                VectorLayer vectorlayer1 = new VectorLayer(VectorLayerType.SFclsLayer);
                if (vectorlayer1.AttachData(m_Tempsfclsreg))
                    vectorlayer1.Name = "可视化区";
                mtrToUse.ActiveMap.Append(vectorlayer1);

                VectorLayer vectorlayer2 = new VectorLayer(VectorLayerType.SFclsLayer);
                if (vectorlayer2.AttachData(m_Tempsfclslin))
                    vectorlayer2.Name = "可视化线";
                mtrToUse.ActiveMap.Append(vectorlayer2);

                VectorLayer vectorlayer3 = new VectorLayer(VectorLayerType.SFclsLayer);
                if (vectorlayer3.AttachData(m_TempsfclsSlopelin))
                    vectorlayer3.Name = "可视化线";
                mtrToUse.ActiveMap.Append(vectorlayer3);

                VectorLayer vectorlayer4 = new VectorLayer(VectorLayerType.AnnLayer);
                if (vectorlayer4.AttachData(m_tempann))
                    vectorlayer4.Name = "可视化注释";
                mtrToUse.ActiveMap.Append(vectorlayer4);

                mtrToUse.ActiveMap.get_Layer(1).State = m_ShowReg ? LayerState.Visible : LayerState.UnVisible;
                mtrToUse.ActiveMap.get_Layer(2).State = m_ShowLine ? LayerState.Visible : LayerState.UnVisible;
                (mtrToUse.ActiveMap.get_Layer(2) as VectorLayer).SymbolShow = m_SymbolShow;
                mtrToUse.ActiveMap.get_Layer(3).State = m_ShowSlopelin ? LayerState.Visible : LayerState.UnVisible;
                (mtrToUse.ActiveMap.get_Layer(3) as VectorLayer).SymbolShow = m_SymbolShow;
                mtrToUse.ActiveMap.get_Layer(4).State = m_ShowAnn ? LayerState.Visible : LayerState.UnVisible;

                mtrToUse.Restore();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string newfilePath = this.buttonEdit1.Text.Trim();
            if (string.IsNullOrEmpty(newfilePath) || _inputFilePath == null || m_SourceMap == null)
            {
                MessageBox.Show("请先通过“数据导入”加载数据，并指定结果输出路径。", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择一个处理方法。", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string modeStr = comboBox1.SelectedItem.ToString();
            double od = 0, oi = 0, elev = 0, nd = 0, ni = 0;
            // 【新增】方向导数参数
            double ori = 0, ord = 0;

            #region 参数校验和读取
            // 根据当前选择的方法，校验并读取参数
            switch (modeStr)
            {
                case "化极":
                    if (string.IsNullOrWhiteSpace(txtOi.Text) || !double.TryParse(txtOi.Text, out oi))
                    {
                        MessageBox.Show("请输入有效的磁倾角！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtOi.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtOd.Text) || !double.TryParse(txtOd.Text, out od))
                    {
                        MessageBox.Show("请输入有效的磁偏角！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtOd.Focus();
                        return;
                    }
                    break;
                case "三角":
                    if (string.IsNullOrWhiteSpace(txtElev.Text) || !double.TryParse(txtElev.Text, out elev))
                    {
                        MessageBox.Show("请输入有效的延拓高度！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtElev.Focus();
                        return;
                    }
                    break;
                case "方向分量":
                case "导数":
                    if (string.IsNullOrWhiteSpace(txtNd.Text) || !double.TryParse(txtNd.Text, out nd))
                    {
                        MessageBox.Show("请输入有效的方向倾角！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtNd.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtNi.Text) || !double.TryParse(txtNi.Text, out ni))
                    {
                        MessageBox.Show("请输入有效的方向偏角！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtNi.Focus();
                        return;
                    }
                    break;
                // 【新增】方向导数参数校验
                case "方向导数":
                    if (string.IsNullOrWhiteSpace(txtOri.Text) || !double.TryParse(txtOri.Text, out ori))
                    {
                        MessageBox.Show("请输入有效的方位角！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtOri.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtOrd.Text) || !double.TryParse(txtOrd.Text, out ord))
                    {
                        MessageBox.Show("请输入有效的阶数！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtOrd.Focus();
                        return;
                    }
                    break;
            }
            #endregion

            // 【修改】using块中处理文件读取（对于a.exe方法，不再读取values到内存）
            using (FileStream fs = new FileStream(_inputFilePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs, Encoding.Default))
            {
                string line = sr.ReadLine();
                if (line == "DSAA")
                {
                    string[] separatingStrings = { " ", "\r\n", "\r", "\n" };
                    int nx = 0, ny = 0;
                    double coorx0 = 0.0, coorx2 = 0.0, coory0 = 0.0, coory2 = 0.0;
                    double intervalx = 0.0, intervaly = 0.0;
                    List<double> values = new List<double>();
                    List<double> res = new List<double>();
                    line = sr.ReadLine();
                    string[] words = line.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
                    Int32.TryParse(words[0], out nx);
                    Int32.TryParse(words[1], out ny);
                    line = sr.ReadLine();
                    words = line.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
                    double.TryParse(words[0], out coorx0);
                    double.TryParse(words[1], out coorx2);
                    line = sr.ReadLine();
                    words = line.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
                    double.TryParse(words[0], out coory0);
                    double.TryParse(words[1], out coory2);
                    intervalx = (nx > 1) ? (coorx2 - coorx0) / (nx - 1) : 100.0;
                    intervaly = (ny > 1) ? (coory2 - coory0) / (ny - 1) : 100.0;
                    sr.Close();
                    fs.Close();

                    // 【修改】这里的 buffer 数组不再需要，因为我们不再直接调用 DLL
                    // double[] buffer = new double[nx * ny];
                    // ... (填充 buffer 的代码被移除)

                    if (modeStr == "化极")
                    {
                        string resultDatPath;
                        if (ExecuteReduceToPoleAlgorithm(_inputFilePath, oi, od, out resultDatPath))
                        {
                            try
                            {
                                string[] resultLines = File.ReadAllLines(resultDatPath);
                                // 从第二行开始读取，跳过表头
                                for (int i = 1; i < resultLines.Length; i++)
                                {
                                    string currentLine = resultLines[i];
                                    if (string.IsNullOrWhiteSpace(currentLine)) continue;

                                    string[] parts = currentLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                    // 【修改】检查列数是否大于等于 3
                                    if (parts.Length >= 3)
                                    {
                                        double val;
                                        // 【修改】解析第 3 列 (索引为 2) 的值
                                        if (double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                                        {
                                            res.Add(val);
                                        }
                                    }
                                }

                                if (res.Count != nx * ny)
                                {
                                    MessageBox.Show($"从 result.dat 读取数据量 ({res.Count}) 与预期 ({nx * ny}) 不符！\n请检查算法输出。", "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("读取算法结果文件 result.dat 时出错: " + ex.Message, "文件读取失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (modeStr == "方向导数")
                    {
                        string resultDatPath;
                        if (ExecuteDirectionalDerivativeAlgorithm(_inputFilePath, ori, ord, out resultDatPath))
                        {
                            try
                            {
                                string[] resultLines = File.ReadAllLines(resultDatPath);
                                // 从第二行开始读取，跳过表头
                                for (int i = 1; i < resultLines.Length; i++)
                                {
                                    string currentLine = resultLines[i];
                                    if (string.IsNullOrWhiteSpace(currentLine)) continue;
                                    string[] parts = currentLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length >= 3)
                                    {
                                        double val;
                                        if (double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                                        {
                                            res.Add(val);
                                        }
                                    }
                                }
                                if (res.Count != nx * ny)
                                {
                                    MessageBox.Show($"从 result.dat 读取数据量 ({res.Count}) 与预期 ({nx * ny}) 不符！\n请检查算法输出。", "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("读取算法结果文件 result.dat 时出错: " + ex.Message, "文件读取失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        // ... (为其他方法读取原始数据的代码保持不变) ...
                        using (StreamReader sr_fallback = new StreamReader(_inputFilePath)) { /* ... */ }
                        double[] buffer = new double[nx * ny];
                        /* ... 填充 buffer ... */

                        // 【修改】使用从界面读取的参数
                        if (modeStr == "三角")
                        {
                            contin(nx, ny, intervalx, intervaly, buffer, 50, 10, elev, res.Add); // 注意：这里的oi,od还是硬编码，如果需要，也应从界面获取
                        }
                        else if (modeStr == "方向分量")
                        {
                            dircomp(nx, ny, intervalx, intervaly, buffer, 50, 10, nd, ni, res.Add);
                        }
                        else if (modeStr == "导数")
                        {
                            deriv(nx, ny, intervalx, intervaly, buffer, 50, 10, nd, ni, res.Add);
                        }
                        // ... (其他方法) ...
                    }

                    if (res.Count == 0) return;

                    double[,] grd = ConvertListToGrd(res, ny, nx);
                    double xllcorner = coorx0;
                    double yllcorner = coory0;
                    double cellsize = intervalx;
                    double nodataValue = -9999;

                    GrdWriter.SaveToAsciiGrid(grd, newfilePath, xllcorner, yllcorner, cellsize, nodataValue);

                    // ... (后续的加载图层、可视化等代码保持不变) ...
                    // 【修改】添加结果到源地图（主界面）
                    RasterLayer raslayerForMain = new RasterLayer();
                    raslayerForMain.URL = "file:///" + newfilePath;
                    if (raslayerForMain.ConnectData())
                    {
                        raslayerForMain.Name = Path.GetFileNameWithoutExtension(newfilePath);
                        m_SourceMap.Append(raslayerForMain);

                        // 刷新主视图
                        if (m_Hook.ActiveContentsView is IMapContentsView mapView)
                        {
                            mapView.MapControl.Refresh();
                        }
                    }
                    else
                    {
                        XMessageBox.Information("加载计算结果文件到主地图失败：\n" + newfilePath);
                    }

                    // 【修改】为插件右侧面板添加结果（使用临时地图）
                    RasterLayer raslayerForPlugin = new RasterLayer();
                    raslayerForPlugin.URL = "file:///" + newfilePath;
                    if (raslayerForPlugin.ConnectData())
                    {
                        m_Map2.RemoveAll();
                        m_Map2.Append(raslayerForPlugin);
                    }

                    if (this.m_mtr2.ActiveMap.LayerCount != 0)
                        this.m_mtr2.ActiveMap.get_Layer(0).State = m_ShowRasOrTin ? LayerState.Visible : LayerState.UnVisible;
                    this.m_mtr2.Restore();

                    DengZhiXianKeShiHua(m_Map2, m_mtr2);
                }
            } // using 结束，文件流自动关闭
        }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = "Surfer 6 Grid File (*.grd)|*.grd|All files (*.*)|*.*";
            savefile.FilterIndex = 1;
            savefile.RestoreDirectory = true;
            savefile.Title = "请选择计算结果(.grd)的保存位置";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                this.buttonEdit1.Text = savefile.FileName;
            }
            savefile.Dispose();
        }

        public class GrdWriter
        {
            public static void SaveToAsciiGrid(double[,] grid, string filePath, double xllcorner, double yllcorner, double cellsize, double nodataValue)
            {
                int rows = grid.GetLength(0);
                int cols = grid.GetLength(1);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("ncols         " + cols);
                    writer.WriteLine("nrows         " + rows);
                    writer.WriteLine("xllcorner     " + xllcorner);
                    writer.WriteLine("yllcorner     " + yllcorner);
                    writer.WriteLine("cellsize      " + cellsize);
                    writer.WriteLine("nodata_value  " + nodataValue);

                    for (int row = 0; row < rows; row++)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            double value = grid[row, col];
                            if (double.IsNaN(value)) value = nodataValue;
                            writer.Write(value + " ");
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        public static double[,] ConvertListToGrd(List<double> dataList, int rows, int cols)
        {
            if (dataList.Count != rows * cols)
            {
                throw new ArgumentException("数据长度与指定的行列数不匹配");
            }

            double[,] grd = new double[rows, cols];
            int index = 0;
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    grd[row, col] = dataList[index];
                    index++;
                }
            }
            return grd;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DengZhiXianKeShiHua(m_Map2, m_mtr2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DengZhiXianKeShiHua(m_Map, m_mtr);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // 【修改】移除所有 Marshal.ReleaseComObject 调用（避免对非 COM 对象的释放导致异常）
                // 只调用 Dispose（如果对象实现 IDisposable）
                // 临时简单要素类和注记：设为 null，让 GC 处理（假设无 Dispose 方法）
                m_Tempsfclslin = null;
                m_TempsfclsSlopelin = null;
                m_Tempsfclsreg = null;
                m_tempann = null;

                // 临时数据库：设为 null（如果有 Close 方法，可调用；这里假设无）
                m_Tempdaba = null;

                // MapControl：调用 Dispose
                if (m_mtr != null) { m_mtr.Dispose(); m_mtr = null; }
                if (m_mtr2 != null) { m_mtr2.Dispose(); m_mtr2 = null; }

                // 临时 Document：调用 Dispose（插件创建的）
                if (m_tempDoc != null) { m_tempDoc.Dispose(); m_tempDoc = null; }

                // 【修改】不要释放或 Dispose 从 hook 获取的对象（如 m_Maindoc, m_SourceMap）
                // 它们由主程序管理
                m_Maindoc = null;
                m_SourceMap = null;
                m_Hook = null;
                m_Map = null;
                m_Map2 = null;
            }
            base.Dispose(disposing);
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        // 创建一个新方法来管理UI的显示/隐藏
        // 创建一个新方法来管理UI的显示/隐藏
        private void UpdateParameterUI()
        {
            // 首先隐藏所有参数面板
            pnlPolar.Visible = false;
            pnlContin.Visible = false;
            pnlDircomp.Visible = false;
            // 【新增】隐藏方向导数面板
            pnlDeriv.Visible = false;
            // 如果您为其他方法也创建了面板，也在这里隐藏它们
            if (comboBox1.SelectedItem == null) return;
            string selectedMethod = comboBox1.SelectedItem.ToString();
            // 根据选择，只显示对应的面板
            switch (selectedMethod)
            {
                case "化极":
                    pnlPolar.Visible = true;
                    break;
                case "三角":
                    pnlContin.Visible = true;
                    break;
                case "方向分量":
                case "导数": // “方向分量”和“导数”使用相同的参数
                    pnlDircomp.Visible = true;
                    break;
                // 【新增】显示方向导数面板
                case "方向导数":
                    pnlDeriv.Visible = true;
                    break;
                // case "二次导数":
                // case "分量":
                // 如果这些方法需要参数，也在这里显示它们的面板
                // break;
                default:
                    // 对于没有参数的方法，不显示任何面板
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateParameterUI();
        }
    }
}