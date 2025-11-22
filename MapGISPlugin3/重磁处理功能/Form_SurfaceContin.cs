using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MapGIS.GISControl;
using MapGIS.GeoMap;
using MapGIS.GeoDataBase;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Info;
using MapGIS.RasCommonObj;
using MapGIS.RasAnalysis;
using MapGIS.GeoObjects.Geometry;
using System.IO;
using System.Runtime.InteropServices;
using MapGIS.PlugUtility;
using MapGIS.PluginEngine;
using System.Diagnostics;
using System.Reflection;

namespace MapGISPlugin3
{
    public partial class Form_SurfaceContin : Form
    {
        private Document m_tempDoc = new Document();
        private Map m_Map = new Map(); // 左侧临时地图
        private Map m_Map2 = new Map(); // 右侧临时地图
        private Document m_Maindoc;
        private Map m_SourceMap; // 源层所属的主地图
        private IApplication m_Hook;
        private bool m_ShowRasOrTin = true;
        private MapControl m_mtr = null;
        private MapControl m_mtr2 = null;
        private string _inputFilePath = null; // 存储导入的 .grd 文件路径

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
        private Point mousePoint = new Point();

        private int[] Imc = {601,603,498,500,436,408,391,233,190,184,154,122,106,33,31,
             127,391,128,392,393,136,149,150,442,443,186,444,179,180,445,189,190};

        public Form_SurfaceContin(IApplication hook)
        {
            InitializeComponent();
            // 新增：初始化拖动和边框功能
            InitDragEvent();
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

            m_tempDoc.GetMaps().Append(m_Map);
            m_tempDoc.GetMaps().Append(m_Map2);

            this.dataTable.Columns.Add("等值线层");
            this.dataTable.Columns.Add("线层");
            this.dataTable.Columns.Add("区层");
            this.dataTable.Columns.Add("注记层");
            this.dataTable.Columns.Add("线层数据", typeof(LinInfo));
            this.dataTable.Columns.Add("区层数据", typeof(RegInfo));
        }

        private void Form_SurfaceContin_Load(object sender, EventArgs e)
        {
            // 【修复1】将 buttonEdit1 替换为设计器中存在的 textBoxSavePath
            this.textBoxSavePath.ReadOnly = true;
            this.textBoxSavePath.BackColor = System.Drawing.SystemColors.ControlLight;

            this.m_mtr.ActiveMap = m_Map;
            this.m_mtr2.ActiveMap = m_Map2;
            this.m_mtr.ShowRuler = true;
            this.m_mtr2.ShowRuler = true;
        }

        // "数据导入" 菜单项的点击事件
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
                m_SourceMap = layerSelectDialog.SelectedMap;
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
                        m_Map.RemoveAll();
                        m_Map.Append(newLayerForPlugin);

                        try
                        {
                            _inputFilePath = new Uri(layerUrl).LocalPath;
                            string inputDirectory = Path.GetDirectoryName(_inputFilePath);
                            string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(_inputFilePath);
                            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string newFileName = $"{inputFileNameWithoutExt}_contin_{timestamp}.grd";
                            string newfilePath = Path.Combine(inputDirectory, newFileName);
                            // 【修复2】将 buttonEdit1 替换为 textBoxSavePath
                            this.textBoxSavePath.Text = newfilePath;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"自动生成输出路径时出错: {ex.Message}", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // 【修复3】将 buttonEdit1 替换为 textBoxSavePath
                            this.textBoxSavePath.Text = "";
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

        // "计算" 按钮的点击事件
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // 【修复4】将 buttonEdit1 替换为 textBoxSavePath
            string newfilePath = this.textBoxSavePath.Text.Trim();
            if (string.IsNullOrEmpty(newfilePath) || _inputFilePath == null || m_SourceMap == null)
            {
                MessageBox.Show("请先通过“数据导入”加载数据，并指定结果输出路径。", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double h;
            if (string.IsNullOrWhiteSpace(txtHeight.Text) || !double.TryParse(txtHeight.Text, out h))
            {
                MessageBox.Show("请输入有效的延拓高度 (h)！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHeight.Focus();
                return;
            }

            string resultDatPath;
            if (!ExecuteSurfaceContinAlgorithm(_inputFilePath, h, out resultDatPath))
            {
                return;
            }

            List<double> res = new List<double>();
            int nx = 0, ny = 0;
            double xllcorner = 0, yllcorner = 0, cellsize = 0;

            try
            {
                using (FileStream fs = new FileStream(_inputFilePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                {
                    sr.ReadLine();
                    string line = sr.ReadLine();
                    string[] words = line.Split(new[] { " ", "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Int32.TryParse(words[0], out nx);
                    Int32.TryParse(words[1], out ny);
                    line = sr.ReadLine();
                    words = line.Split(new[] { " ", "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    double.TryParse(words[0], out xllcorner);
                    double coorx2;
                    double.TryParse(words[1], out coorx2);
                    line = sr.ReadLine();
                    words = line.Split(new[] { " ", "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    double.TryParse(words[0], out yllcorner);
                    cellsize = (nx > 1) ? (coorx2 - xllcorner) / (nx - 1) : 100.0;
                }

                List<string> resultLines = new List<string>();
                using (FileStream fs = new FileStream(resultDatPath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        resultLines.Add(line);
                    }
                }

                for (int i = 1; i < resultLines.Count; i++)
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
                MessageBox.Show("读取输入 GRD 头文件或读取 result.dat 时出错: " + ex.Message);
                return;
            }

            double[,] grd = ConvertListToGrd(res, ny, nx);
            double nodataValue = -9999;
            GrdWriter.SaveToAsciiGrid(grd, newfilePath, xllcorner, yllcorner, cellsize, nodataValue);

            RasterLayer raslayerForMain = new RasterLayer();
            raslayerForMain.URL = "file:///" + newfilePath;
            if (raslayerForMain.ConnectData())
            {
                raslayerForMain.Name = Path.GetFileNameWithoutExtension(newfilePath);
                m_SourceMap.Append(raslayerForMain);

                if (m_Hook.ActiveContentsView is IMapContentsView mapView)
                {
                    mapView.MapControl.Refresh();
                }
            }
            else
            {
                MessageBox.Show("加载计算结果文件到主地图失败：\n" + newfilePath);
            }

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

        // 执行“曲面延拓” a.exe 的函数
        private bool ExecuteSurfaceContinAlgorithm(string inputGrdPath, double h, out string resultDatPath)
        {
            resultDatPath = null;
            try
            {
                string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string algorithmDir = Path.Combine(pluginPath, "Algorithm", "SurfaceContin");
                string exePath = Path.Combine(algorithmDir, "a.exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show($"算法模块 (a.exe) 丢失！\n请确保它位于：\n{exePath}", "文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (!File.Exists(inputGrdPath))
                {
                    MessageBox.Show($"输入文件 (grd) 丢失！\n路径：\n{inputGrdPath}", "文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{inputGrdPath}\" {h}",
                    WorkingDirectory = algorithmDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string errorOutput = process.StandardError.ReadToEnd();
                    bool exited = process.WaitForExit(5 * 60 * 1000);

                    if (!exited)
                    {
                        MessageBox.Show("算法执行超时（超过5分钟），操作被中断。", "超时错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (!process.HasExited) process.Kill();
                        return false;
                    }

                    if (process.ExitCode != 0)
                    {
                        string errorMessage = $"外部算法 a.exe 执行失败，退出码: {process.ExitCode}。";
                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            errorMessage += "\n\n错误信息:\n" + errorOutput;
                        }
                        MessageBox.Show(errorMessage, "算法执行失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                resultDatPath = Path.Combine(algorithmDir, "result.dat");
                if (!File.Exists(resultDatPath))
                {
                    MessageBox.Show("算法执行完毕，但未在预期位置找到结果文件 result.dat！\n请检查 a.exe 是否正确运行。", "结果文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        #region 保留的辅助函数
        // 浏览输出路径的事件处理方法（已与设计器控件匹配）
        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "选择输出文件路径";
                saveFileDialog.Filter = "Surfer 6 Grid File (*.grd)|*.grd|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.textBoxSavePath.Text = saveFileDialog.FileName;
                }
            }
        }

        // 数据导入按钮点击事件
        private void btnImportData_Click(object sender, EventArgs e)
        {
            数据导入ToolStripMenuItem_Click(sender, e);
        }

        // 关闭按钮事件
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 刷新左侧
        private void button4_Click(object sender, EventArgs e)
        {
            DengZhiXianKeShiHua(m_Map, m_mtr);
        }

        // 刷新右侧
        private void button3_Click(object sender, EventArgs e)
        {
            DengZhiXianKeShiHua(m_Map2, m_mtr2);
        }

        // 初始化等值线参数
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

            m_ScaleX = (m_Rect.XMax - m_Rect.XMin) / 1000.0;
            m_ScaleY = (m_Rect.YMax - m_Rect.YMin) / 1000.0;

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

        // 等值线可视化
        private void DengZhiXianKeShiHua(Map mapToUse, MapControl mtrToUse)
        {
            if (m_ContourParamStrcT == null) return;
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

        // 内部类：图层选择对话框
        private class LayerSelectDialog : Form
        {
            private TreeView treeViewLayers;
            private Button btnOK;
            private Button btnCancel;
            private IApplication m_Hook;
            public RasterLayer SelectedRasterLayer { get; private set; }
            public Map SelectedMap { get; private set; }

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
                        mapNode.Tag = map;
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

        // GRD文件写入工具类
        public class GrdWriter
        {
            public static void SaveToAsciiGrid(double[,] grid, string filePath, double xllcorner, double yllcorner, double cellsize, double nodataValue)
            {
                int rows = grid.GetLength(0);
                int cols = grid.GetLength(1);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("DSAA");
                    writer.WriteLine(cols + " " + rows);
                    writer.WriteLine(xllcorner + " " + (xllcorner + cellsize * (cols - 1)));
                    writer.WriteLine(yllcorner + " " + (yllcorner + cellsize * (rows - 1)));
                    writer.WriteLine(grid.Cast<double>().Min() + " " + grid.Cast<double>().Max());

                    for (int row = rows - 1; row >= 0; row--)
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

        // 列表转GRD数组
        public static double[,] ConvertListToGrd(List<double> dataList, int rows, int cols)
        {
            if (dataList.Count != rows * cols)
            {
                throw new ArgumentException("数据长度与指定的行列数不匹配");
            }

            double[,] grd = new double[rows, cols];
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    grd[row, col] = dataList[index];
                    index++;
                }
            }
            return grd;
        }

        // 资源释放
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                m_Tempsfclslin = null;
                m_TempsfclsSlopelin = null;
                m_Tempsfclsreg = null;
                m_tempann = null;
                m_Tempdaba = null;

                if (m_mtr != null) { m_mtr.Dispose(); m_mtr = null; }
                if (m_mtr2 != null) { m_mtr2.Dispose(); m_mtr2 = null; }
                if (m_tempDoc != null) { m_tempDoc.Dispose(); m_tempDoc = null; }

                m_Maindoc = null;
                m_SourceMap = null;
                m_Hook = null;
                m_Map = null;
                m_Map2 = null;
            }
            base.Dispose(disposing);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
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
    }
}