// Form1.cs (Modified)
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

namespace MapGISPlugin3
{
    public partial class Form1 : Form
    {
        //[DllImport("magdpless.dll", EntryPoint = "echo")]
        //public static extern void echo();

        //[DllImport("magdpless.dll", EntryPoint = "test0")]
        //public static extern void test0(int nx, int ny, double dx, double dy, double od, double oi);

        //[DllImport("magdpless.dll", EntryPoint = "test2")]
        //public static extern void test2(double[] value, int size);

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


        private Document m_doc = new Document();//地图文档
        private Document m_doc2 = new Document();
        private Map m_Map = new Map();//地图
        private Map m_Map2 = new Map();//地图
        private Map activeMap = new Map();//地图
        private Document m_Maindoc;//主框上的地图文档
        private IApplication m_Hook; // <--- 【新增】添加这个成员变量来保存整个应用程序钩子
        private bool m_ShowRasOrTin = true;//是否显示底图栅格
        private MapControl m_mtr = null;//视图控件
        private MapControl m_mtr2 = null;//视图控件2
        private MapControl mp = null;//主框上的地图
        private DataBase m_geodaba;//数据库


        public static string in_url = null;

        //private Raster m_ras = null;
        private Rect m_Rect = null;//数据范围
        private ContourParamStrcT_Stru m_ContourParamStrcT = null;//等值线追踪参数结构
        private int m_BandNum = 1;//波段号
        private DataTable dataTable = new DataTable();//等值层信息表
        private double m_Min = 0;//像元最大值
        private double m_Max = 0;//像元最小值
        private ContourNoteParam_Stru m_ContourNoteParam = null;//等值线注记参数结构
        private DataBase m_Tempdaba;//临时数据库
        private SFeatureCls m_Tempsfclslin;//临时线简单要素类
        private SFeatureCls m_TempsfclsSlopelin;//临时示坡线简单要素类
        private SFeatureCls m_Tempsfclsreg;//临时区简单要素类
        private AnnotationCls m_tempann;//临时注记
        private double m_ScaleX = 0;//X方向显示比
        private double m_ScaleY = 0;//Y方向显示比
        private bool m_ClipLine = true;//是否裁剪线
        private bool m_ShowReg = true;//是否显示区
        private bool m_ShowLine = true;//是否显示线
        private bool m_ShowSlopelin = false;//是否显示示坡线
        private bool m_ShowAnn = true;//是否显示注记
        private bool m_SymbolShow = true;//是否符号化显示
        private double m_LfZstep = 10;//等值线步长
        private float m_FSlopeYEps = 0;//示坡线宽
        private float m_Length = 0;
        private SlopLinParam_Stru m_SlopLineParam = null;//示坡线参数结构

        private int[] Imc ={601,603,498,500,436,408,391,233,190,184,154,122,106,33,31,
               127,391,128,392,393,136,149,150,442,443,186,444,179,180,445,189,190};//初始化区颜色
        private LayerSelectComboBox layerSelectComboBoxRas;




        public Form1(IApplication hook)
        {

            InitializeComponent();
            m_Hook = hook; // <--- 【修改】将传入的 hook 保存到成员变量中
            // 构造函数中可以尝试获取一次 Document，但这不是关键
            if (m_Hook != null)
            {
                m_Maindoc = m_Hook.Document;
            }
            //m_Maindoc = hook.Document;
            //IMapContentsView mainContentsView = hook.ActiveContentsView as IMapContentsView;

            //MapControl mp = mainContentsView.MapControl;
            //Map activeMap = mp.ActiveMap;

            this.m_mtr = new MapControl();
            this.m_mtr.Dock = DockStyle.Fill;
            this.m_mtr.ShowRuler = false;
            this.m_mtr.ShowScrollBar = false;
            this.panelControl1.Controls.Add(m_mtr);


            Document dc = hook.Document;
            Maps maps = dc.GetMaps();
            m_Map = maps.GetMap(0);
            m_Map2 = maps.GetMap(2);


            this.m_mtr2 = new MapControl();
            this.m_mtr2.Dock = DockStyle.Fill;
            this.m_mtr2.ShowRuler = false;
            this.m_mtr2.ShowScrollBar = false;
            this.panelControl2.Controls.Add(m_mtr2);


            m_doc2.GetMaps().Append(m_Map2);
            m_doc.GetMaps().Append(m_Map);

            this.dataTable.Columns.Add("等值线层");
            this.dataTable.Columns.Add("线层");
            this.dataTable.Columns.Add("区层");
            this.dataTable.Columns.Add("注记层");
            this.dataTable.Columns.Add("线层数据", typeof(LinInfo));
            this.dataTable.Columns.Add("区层数据", typeof(RegInfo));

            this.layerSelectComboBoxRas = new LayerSelectComboBox();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.m_mtr.ActiveMap = m_Map;
            this.m_mtr2.ActiveMap = m_Map2;
            this.m_mtr.ShowRuler = true;
            this.m_mtr2.ShowRuler = true;

            this.comboBox1.Items.Add("化极");
            this.comboBox1.Items.Add("三角");
            this.comboBox1.Items.Add("方向分量");
            this.comboBox1.Items.Add("导数");
            this.comboBox1.Items.Add("二次导数");
            this.comboBox1.Items.Add("分量");
            this.comboBox1.SelectedIndex = 0;
            return;
        }
        private void Init()
        {
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
            else
            {

            }

            m_ScaleX = (m_Rect.XMax - m_Rect.XMin) / 1000.0;
            m_ScaleY = (m_Rect.YMax - m_Rect.YMin) / 1000.0;

            #region 做计算,初始化等值线追踪结构体中参数

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
            if (m_Min < 0) // 确定最小等值线值...
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
            k = 0;  // 对每层设定高程值...
            List<double> lstfZdem = new List<double>();
            do
            {
                lstfZdem.Add(zdat);
                zdat += m_LfZstep;
                k++;
            } while (zdat < m_Max);
            lstfZdem.Add(m_Max);

            #endregion

            //初始化DataTable
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
            // --- 检查逻辑保持不变，非常棒 ---
            if (m_Hook == null)
            {
                MessageBox.Show("严重错误：插件未能正确初始化，无法与主程序通信。", "初始化失败", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            Document docToUse = m_Hook.Document;
            if (docToUse == null)
            {
                MessageBox.Show("操作失败：无法获取当前的地图文档。请确保您已在数据中心打开一个地图工程。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- 弹出选择对话框的逻辑保持不变 ---
            LayerSelectDialog layerSelectDialog = new LayerSelectDialog(m_Hook);
            if (layerSelectDialog.ShowDialog() == DialogResult.OK)
            {
                RasterLayer selectedLayerFromMainDoc = layerSelectDialog.SelectedRasterLayer;
                if (selectedLayerFromMainDoc != null)
                {
                    // 【关键修改点】
                    // 不要直接移动 selectedLayerFromMainDoc 对象。
                    // 而是获取它的数据源URL，然后创建一个新的图层对象。

                    // 1. 获取选中图层的数据源路径 (URL)
                    string layerUrl = selectedLayerFromMainDoc.URL;

                    // (调试) 可以加一个弹窗来确认URL是否正确获取
                    // MessageBox.Show("获取到的图层URL是: " + layerUrl);

                    if (string.IsNullOrEmpty(layerUrl))
                    {
                        MessageBox.Show("选中的图层没有有效的URL，无法加载。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 2. 创建一个全新的、属于本插件的 RasterLayer 对象
                    RasterLayer newLayerForPlugin = new RasterLayer();

                    // 3. 将新图层的URL指向同一个数据源
                    newLayerForPlugin.URL = layerUrl;

                    // 4. 连接数据，这是非常重要的一步！
                    if (newLayerForPlugin.ConnectData())
                    {
                        // 5. （可选但推荐）给新图层一个名字，可以和原来的一样
                        newLayerForPlugin.Name = selectedLayerFromMainDoc.Name;

                        // 6. 现在可以安全地操作插件内部的 m_Map 了
                        // 清空上一个map中的图层
                        m_Map.RemoveAll();

                        // 7. 将我们【新创建的】图层添加到插件的 m_Map 中
                        m_Map.Append(newLayerForPlugin);

                        // 后续逻辑保持不变，它们现在会作用于这个有效的新图层
                        if (this.m_mtr.ActiveMap.LayerCount != 0)
                            this.m_mtr.ActiveMap.get_Layer(0).State = m_ShowRasOrTin ? LayerState.Visible : LayerState.UnVisible;

                        this.m_mtr.Restore();

                        // 初始化
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
                        // 如果ConnectData失败，说明路径有问题或者文件损坏
                        MessageBox.Show("无法连接到栅格数据源，请检查文件是否有效。\n路径: " + layerUrl, "数据连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // 释放创建失败的对象
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(newLayerForPlugin);
                    }
                }
            }
            layerSelectDialog.Dispose();
        }

        // 新增的 LayerSelectDialog 类（作为内部类或单独文件，这里作为内部）
        private class LayerSelectDialog : Form
        {
            private TreeView treeViewLayers;
            private Button btnOK;
            private Button btnCancel;
            private IApplication m_Hook;
            public RasterLayer SelectedRasterLayer { get; private set; }

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
                finally
                {
                    // Removed: if (maps != null) Marshal.ReleaseComObject(maps);
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
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            private void BtnCancel_Click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void AddLayerToDoc(string savePath, RasterLayer rasLayer)
        {
            if (savePath.Contains("/ras/"))
            {//栅格数据集
                rasLayer.URL = savePath;
                string[] ss = savePath.Split('/');
                savePath = ss[ss.Length - 1];
            }
            else
            {//影像文件
                rasLayer.URL = "file:///" + savePath;
                string[] ss = savePath.Split('\\');
                savePath = ss[ss.Length - 1];
            }
            if (rasLayer.ConnectData())
            {
                // 修改说明：选择地图中的数据进行操作，结果生成到该地图中。
                // 修改人：韩俊鹏 2014-05-29
                rasLayer.Name = savePath;
                Map mp = MapGIS.DemAnalysis.Plugin.Util.GetMapByLayer(this.layerSelectComboBoxRas.SelectedDocumentItem);
                if (mp != null)
                    mp.Append(rasLayer);
                else
                {
                    if (m_doc.GetMaps().Count != 0)
                        m_doc.GetMaps().GetMap(0).Append(rasLayer);
                    else
                    {
                        Map map = new Map();
                        map.Name = this.Text;
                        map.Append(rasLayer);
                        m_doc.GetMaps().Append(map);
                    }
                }
            }
        }

        private void DengZhiXianKeShiHua(Map m_Map, MapControl m_mtr)
        {
            if (m_ContourParamStrcT == null) return;
            //清空这一个map中的图层，只留一个raslayer
            while (m_Map.LayerCount != 1)
            {
                m_Map.Remove(1);
            }
            RasTraceContour traceContour = null;
            if (m_mtr.ActiveMap.get_Layer(0) is RasterLayer)
            {
                RasterDataSet rasdataset = (m_mtr.ActiveMap.get_Layer(0) as RasterLayer).GetData() as RasterDataSet;
                traceContour = new RasTraceContour(rasdataset, m_BandNum);
            }
            else
            {
                return;
            }
            int n = this.dataTable.Rows.Count;
            //构造等值层信息结构体
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
            //添加等值层信息结构体到追踪参数结构体中
            m_ContourParamStrcT.SetZVelBuf(arrayZVelStrcT);
            m_ContourParamStrcT.pContourNoteParam = m_ContourNoteParam;
            //初始化临时简单要素类
            m_Tempsfclslin = null;
            m_TempsfclsSlopelin = null;
            m_Tempsfclsreg = null;
            m_tempann = null;
            if (m_Tempdaba == null)
                m_Tempdaba = DataBase.OpenTempDB();
            // 修改说明：OpenTempDB可能会不成功，返回null（Bug6696）
            // 修改人：陈容 2015-09-23
            if (m_Tempdaba == null)
                return;
            //创建临时简单要素类
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
            //追踪
            int rtn = traceContour.RsTraceContour(m_ContourParamStrcT, m_Tempsfclslin, m_TempsfclsSlopelin, m_Tempsfclsreg, m_tempann, 1024, false, m_ClipLine);
            //if (m_ContourParamStrcT.Colscl == 1)
            //{
            //    traceContour.RsMakeColScale(arrayZVelStrcT, m_ContourNoteParam, m_Tempsfclslin, m_Tempsfclsreg, m_tempann, m_Rect.XMin, m_Rect.YMin, m_Rect.XMax - m_Rect.XMin);
            //}
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
            //设置结果显示比
            m_Tempsfclslin.ScaleX = m_ScaleX;
            m_Tempsfclslin.ScaleY = m_ScaleY;
            m_TempsfclsSlopelin.ScaleX = m_ScaleX;
            m_TempsfclsSlopelin.ScaleY = m_ScaleY;
            m_Tempsfclsreg.ScaleX = m_ScaleX;
            m_Tempsfclsreg.ScaleY = m_ScaleY;
            //添加结果到视图
            if (rtn > 0)
            {
                VectorLayer vectorlayer1 = new VectorLayer(VectorLayerType.SFclsLayer);
                if (vectorlayer1.AttachData(m_Tempsfclsreg))
                    vectorlayer1.Name = "可视化区";
                m_mtr.ActiveMap.Append(vectorlayer1);

                VectorLayer vectorlayer2 = new VectorLayer(VectorLayerType.SFclsLayer);
                if (vectorlayer2.AttachData(m_Tempsfclslin))
                    vectorlayer2.Name = "可视化线";
                m_mtr.ActiveMap.Append(vectorlayer2);

                VectorLayer vectorlayer3 = new VectorLayer(VectorLayerType.SFclsLayer);
                if (vectorlayer3.AttachData(m_TempsfclsSlopelin))
                    vectorlayer3.Name = "可视化线";
                m_mtr.ActiveMap.Append(vectorlayer3);

                VectorLayer vectorlayer4 = new VectorLayer(VectorLayerType.AnnLayer);
                if (vectorlayer4.AttachData(m_tempann))
                    vectorlayer4.Name = "可视化注释";
                m_mtr.ActiveMap.Append(vectorlayer4);

                m_mtr.ActiveMap.get_Layer(1).State = m_ShowReg ? LayerState.Visible : LayerState.UnVisible;
                m_mtr.ActiveMap.get_Layer(2).State = m_ShowLine ? LayerState.Visible : LayerState.UnVisible;
                (m_mtr.ActiveMap.get_Layer(2) as VectorLayer).SymbolShow = m_SymbolShow;
                m_mtr.ActiveMap.get_Layer(3).State = m_ShowSlopelin ? LayerState.Visible : LayerState.UnVisible;
                (m_mtr.ActiveMap.get_Layer(3) as VectorLayer).SymbolShow = m_SymbolShow;
                m_mtr.ActiveMap.get_Layer(4).State = m_ShowAnn ? LayerState.Visible : LayerState.UnVisible;

                m_mtr.Restore();
            }


        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (this.buttonEdit1.Text.Trim() == "")
            {
                XMessageBox.Information("请填写路径");
                return;
            }
            string modeStr = comboBox1.SelectedItem.ToString();
            MapLayer x = this.m_mtr.ActiveMap.get_Layer(0);
            Uri uri = new Uri(x.URL);

            string filePath = uri.LocalPath;
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            string line;
            line = sr.ReadLine();

            if (line == "DSAA")
            {
                string[] separatingStrings = { " ", "\r\n", "\r", "\n" };

                int nx = 0;
                int ny = 0;
                double coorx0 = 0.0;
                double coorx2 = 0.0;
                double coory0 = 0.0;
                double coory2 = 0.0;
                double intervalx = 0.0;
                double intervaly = 0.0;
                double od = 10.0;
                double oi = 50.0;
                List<double> values = new List<double>();
                List<double> res = new List<double>();

                line = sr.ReadLine();
                string[] words = line.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(words[0], out nx);
                Int32.TryParse(words[1], out ny);

                line = sr.ReadLine();
                words = line.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                double.TryParse(words[0], out coorx0);
                double.TryParse(words[1], out coorx2);

                line = sr.ReadLine();
                words = line.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                double.TryParse(words[0], out coory0);
                double.TryParse(words[1], out coory2);

                intervalx = (nx > 1) ? (coorx2 - coorx0) / (nx - 1) : 100.0;
                intervaly = (ny > 1) ? (coory2 - coory0) / (ny - 1) : 100.0;

                sr.ReadLine();
                line = sr.ReadToEnd();
                words = line.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string each in words)
                {
                    double temp = 0.0;
                    double.TryParse(each, out temp);
                    values.Add(temp);
                }

                double[] buffer = new double[nx * ny];
                int index = 0;
                //foreach (double temp in values)
                //{
                //    buffer[index] = temp;
                //    index = index + 1;
                //}
                for (int j = 0; j < ny; j++)
                {
                    for (int i = 0; i < nx; i++)
                    {
                        buffer[j * nx + i] = values[index];
                        index++;
                    }
                }

                int size = Marshal.SizeOf(buffer[0]) * buffer.Length;
                //test0(nx, ny, intervalx, intervaly, od, oi);
                //test2(buffer, nx * ny);
                //var lst = new List<double>();
                //test3(lst.Add);
                //foreach (double each in lst)
                //{ 
                //    Console.WriteLine(each);
                //}
                if (modeStr == "化极")
                    polar(nx, ny, intervalx, intervaly, buffer, od, oi, res.Add);
                else if (modeStr == "三角")
                {
                    double elev = Convert.ToDouble(textBox1.Text);
                    contin(nx, ny, intervalx, intervaly, buffer, od, oi, elev, res.Add);
                }
                else if (modeStr == "方向分量")
                {
                    double nd = Convert.ToDouble(textBox2.Text);
                    double ni = Convert.ToDouble(textBox3.Text);
                    dircomp(nx, ny, intervalx, intervaly, buffer, od, oi, nd, ni, res.Add);
                }
                else if (modeStr == "导数")
                {
                    double nd = Convert.ToDouble(textBox2.Text);
                    double ni = Convert.ToDouble(textBox3.Text);
                    deriv(nx, ny, intervalx, intervaly, buffer, od, oi, nd, ni, res.Add);
                }
                else if (modeStr == "二次导数")
                {
                    int direction = 2;
                    second(nx, ny, intervalx, intervaly, buffer, od, oi, direction, res.Add);
                }
                else
                {
                    int direction = 2;
                    compon(nx, ny, intervalx, intervaly, buffer, od, oi, direction, res.Add);

                }


                double[,] grd = ConvertListToGrd(res, nx, ny);
                // 保存到.grd文件

                string newfilePath = this.buttonEdit1.Text.Trim();


                double xllcorner = 0.0;
                double yllcorner = 0.0;
                double cellsize = 1.0;
                double nodataValue = -9999;

                GrdWriter.SaveToAsciiGrid(grd, newfilePath, xllcorner, yllcorner, cellsize, nodataValue);

                /* RasterLayer rasterlayer = new RasterLayer();
                 rasterlayer.URL = newfilePath;

                 this.m_mtr2.ActiveMap.Append(rasterlayer);


                 foreach (double each in res)
                 {
                     listBox1.Items.Add(each);


                 }
                 this.m_mtr2.Restore();*/

                {
                    RasterLayer raslayer = new RasterLayer();

                    if (newfilePath.Contains("/ras/"))
                        raslayer.URL = newfilePath;
                    else if (newfilePath.Contains("file:///"))
                        raslayer.URL = newfilePath;
                    else
                        raslayer.URL = "file:///" + newfilePath;
                    if (raslayer.ConnectData())
                    {
                        m_Map2.Append(raslayer);


                        string fullPath = this.buttonEdit1.Text.Trim();
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
                        raslayer.Name = fileNameWithoutExtension;
                    }
                    else
                        XMessageBox.Information(newfilePath);

                }

                if (this.m_mtr2.ActiveMap.LayerCount != 0)
                    this.m_mtr2.ActiveMap.get_Layer(0).State = m_ShowRasOrTin ? LayerState.Visible : LayerState.UnVisible;
                this.m_mtr2.Restore();

                DengZhiXianKeShiHua(m_Map2, m_mtr2);





                sr.Close();
            }
        }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            // 使用标准的SaveFileDialog
            SaveFileDialog savefile = new SaveFileDialog();

            // 设置文件过滤器，让用户方便地保存为.grd文件
            savefile.Filter = "Surfer 6 Grid File (*.grd)|*.grd|All files (*.*)|*.*";
            savefile.FilterIndex = 1;
            savefile.RestoreDirectory = true; // 对话框记忆上次打开的目录
            savefile.Title = "请选择计算结果(.grd)的保存位置";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                // 获取用户选择的完整、标准的Windows文件路径
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
                    // 写入文件头
                    writer.WriteLine("ncols         " + cols);
                    writer.WriteLine("nrows         " + rows);
                    writer.WriteLine("xllcorner     " + xllcorner);
                    writer.WriteLine("yllcorner     " + yllcorner);
                    writer.WriteLine("cellsize      " + cellsize);
                    writer.WriteLine("nodata_value  " + nodataValue);

                    // 写入数据
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
            // 检查数据长度是否匹配
            if (dataList.Count != rows * cols)
            {
                throw new ArgumentException("数据长度与指定的行列数不匹配");
            }

            // 创建二维数组
            double[,] grd = new double[rows, cols];

            // 填充二维数组
            int index = 0;
            //for (int row = 0; row < rows; row++)
            //{
            //    for (int col = 0; col < cols; col++)
            //    {
            //        grd[row, col] = dataList[index];
            //        index++;
            //    }
            //}
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 释放第一个 MapControl
            if (m_mtr != null)
            {
                m_mtr.Dispose();
                m_mtr = null;
            }

            // 【添加】释放第二个 MapControl
            if (m_mtr2 != null)
            {
                m_mtr2.Dispose();
                m_mtr2 = null;
            }

            // （可选但推荐）在这里也可以添加对 m_Tempdaba 等其他长生命周期成员的清理
        }

    }
}