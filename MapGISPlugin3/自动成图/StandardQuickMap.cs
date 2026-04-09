using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Container;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GeoObjects;
using MapGIS.GeoObjects.Att;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoObjects.Info;
using MapGIS.GeoObjects.SpatialRef;
using MapGIS.MapEditor;
using MapGIS.PluginEngine;
using MapGIS.PlugUtility;
using MapGIS.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace MapGISPlugin3
{
    public class StandardQuickMap : XtraForm
    {
        private CommonMapDecoration commonMapDecoration = new CommonMapDecoration();
        private MapDecoration mapDecoration;
        private RangeForm rangeForm;
        private Map map;
        private List<Dot> srcDataPolygon = new List<Dot>();
        private int srcDataMode = -1;
        private int scale;
        private SRefData srcSRS;
        private SRefData currentSRS;
        private SRefData dstMapSRS;
        private bool isProjectSrcSRS;
        private Rect dataRange = new Rect();
        private bool isInitUI;
        private Size imageSize = new Size(256, 256);
        private bool isMapOK;
        private Hashtable texts = new Hashtable();
        private IApplication app;
        private bool isWholeRange;
        private int checkData;
        private string oldLeftBottom = "";
        private bool isImageLayer;
        private IContainer components;

        // 【关键修复】声明为类成员变量
        private System.ComponentModel.ComponentResourceManager componentResourceManager;

        private LabelControl labelControl_Sacle;
        private GroupControl groupControl_DataRange;
        private TreeList treeList_Ann;
        private GroupControl groupControl_Text;
        private LabelControl labelControl6;
        private ButtonEdit buttonEdit_Template;
        private GroupControl groupControl_Styles;
        private SimpleButton simpleButton_OK;
        private SimpleButton simpleButton_Cancel;
        private TreeListColumn typeColumn;
        private TreeListColumn contentColumn;
        private RepositoryItemMemoEdit contentMemoEdit;
        private ComboBoxEdit comboBoxEdit_Scale;
        private ImageList largeImgList;
        private SplitContainerControl splitContainerControl1;
        private SplitContainerControl splitContainerControl2;
        private ListView listView_Style;
        private ColumnHeader nameColumnHeader;
        private ColumnHeader typeColumnHeader;
        private ColumnHeader descriptionColumnHeader;
        private LabelControl labelControl_WH;
        private Button button2;
        private Button button1;
        private PanelControl panelControl1;
        private List<LegendItemInfo> _dynamicLegends = new List<LegendItemInfo>();
        // 1. 【核心】复制 VisualizationHelper 中的颜色数组
        private readonly int[] _contourColors = {
    601,603,498,500,436,408,391,233,190,184,154,122,106,33,31,
    127,391,128,392,393,136,149,150,442,443,186,444,179,180,445,189,190
};

        public IApplication Application
        {
            set
            {
                app = value;
            }
        }
        // 【修改】通用测点结构 (不再叫 TEMStationInfo)
        private class StationPointInfo
        {
            public double X, Y;
            public string Name;
        }

        // 【保留】发射源结构 (依旧读属性坐标)
        // 【修改前】是固定的 ABCD 坐标
        // private class TransmitterInfo { public double PointA_X... }

        // 【修改后】改为动态列表，支持任意数量的点，且不强制闭合
        private class TransmitterInfo
        {
            // 用于存储所有有效的坐标点 (按 A->B->C->D 顺序)
            public List<Dot> Points = new List<Dot>();

            // 只要有2个或以上的点，就可以连成线，视为有效
            public bool IsValid
            {
                get { return Points != null && Points.Count >= 2; }
            }
        }
        // 用于保存图例信息的简单类
        public class LegendItemInfo
        {
            public string Name;      // 图例文字，如 "测点"
            public int SymID;        // 子图号
            public int Color;        // 颜色号
            public float Size;       // 大小
        }

        public StandardQuickMap(Map basemap, Rect rect)
        {
            InitializeComponent();
            InitDefaultStyle();
            map = basemap;
            if (rect == null)
            {
                if (map != null)
                {
                    rect = map.Range;
                }
                isWholeRange = true;
            }
            if (rect != null)
            {
                srcDataPolygon.Add(new Dot(rect.XMin, rect.YMin));
                srcDataPolygon.Add(new Dot(rect.XMax, rect.YMin));
                srcDataPolygon.Add(new Dot(rect.XMax, rect.YMax));
                srcDataPolygon.Add(new Dot(rect.XMin, rect.YMax));
                srcDataPolygon.Add(new Dot(rect.XMin, rect.YMin));
            }
            srcDataMode = 0;
        }
        /// <summary>
        /// 【双栏最终修复版】绘制等值线专用图例
        /// 修复了读取 "等值线间距" 失败的问题（统一为简体中文）
        /// </summary>
        private void DrawContourLegend(SFeatureCls lineCls, AnnotationCls annCls, Rect outRect, double unit)
        {
            // --- 1. 获取数据极值 ---
            double zMin, zMax;
            GetGridMinMax(out zMin, out zMax);

            if (zMin >= zMax) return;

            // =========================================================
            // ★★★ 核心修复：确保 Key 是简体中文 "等值线间距" ★★★
            // =========================================================
            double autoStep = 0;

            // 务必检查这里的字符串和 RefreshTextList 里的一模一样
            string keyName = "等值线间距";

            if (texts.ContainsKey(keyName))
            {
                string userInput = texts[keyName].ToString();
                if (double.TryParse(userInput, out double val) && val > 0.00001)
                {
                    autoStep = val;
                }
            }

            // 如果没读到（或者用户填了0），才自动计算
            if (autoStep <= 0.00001)
            {
                autoStep = CalculateNiceStep(zMin, zMax, 12);
                // [调试] 如果弹这个框，说明读取失败了，还是走了自动计算
                // MessageBox.Show("未能读取到输入值，正在使用自动计算步长: " + autoStep);
            }
            else
            {
                // [调试] 如果弹这个框，说明读取成功了
                // MessageBox.Show("成功读取步长: " + autoStep);
            }

            // --- 生成层级列表 ---
            double startZ = Math.Floor(zMin / autoStep) * autoStep;
            List<double> levels = new List<double>();
            int safetyLimit = 0;
            for (double z = startZ; z <= zMax + 0.0001; z += autoStep)
            {
                if (z < zMin - 0.0001) continue;
                levels.Add(z);
                safetyLimit++;
                if (safetyLimit > 500) break;
            }
            if (levels.Count > 0 && levels[levels.Count - 1] < zMax - 0.0001)
            {
                levels.Add(zMax);
            }
            levels.Reverse(); // 高值在上面

            if (levels.Count == 0) return;

            // =========================================================
            // ★★★ 双栏布局逻辑 (保持不变) ★★★
            // =========================================================

            // 1. 定义尺寸 (不压缩)
            double itemH = 6.0 * unit;       // 单行高度
            double colorBoxW = 12.0 * unit;  // 色块宽度
            double textBoxW = 28.0 * unit;   // 文字预留宽
            double colPadding = 8.0 * unit;  // 栏内左边距
            double textGap = 2.0 * unit;     // 文字间距
            double colGap = 10.0 * unit;     // 两栏间距
            double titleH = 15.0 * unit;     // 标题高

            // 2. 计算分栏
            int totalItems = levels.Count;
            int itemsInLeftCol = (int)Math.Ceiling(totalItems / 2.0);

            // 3. 计算总尺寸
            double contentH = itemsInLeftCol * itemH;
            double totalH = titleH + contentH + (2.0 * unit);
            double singleColW = colPadding + colorBoxW + textGap + textBoxW;
            double totalW = singleColW + colGap + singleColW;

            // 4. 定位
            double xLeft = outRect.XMin;
            double gapFromFrame = 15.0 * unit;
            double yTop = outRect.YMin - gapFromFrame;
            double yBottom = yTop - totalH;

            // --- 绘制外框 ---
            LinInfo framePen = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };
            DrawRect(lineCls, xLeft, yBottom, xLeft + totalW, yTop, framePen);

            TextAnnInfo txtInfo = new TextAnnInfo { Ovprnt = true };
            float titleSize = (float)(4.0 * unit);
            TextAnno title = new TextAnno { Text = "图  例", Height = titleSize, Width = titleSize };
            title.AnchorDot = new Dot(xLeft + totalW / 2.0 - (2.5 * titleSize) / 2.0, yTop - titleSize - (3.0 * unit));
            annCls.Append(title, null, txtInfo);

            // --- 循环绘制 ---
            float numSize = (float)(3.0 * unit);
            double listTopY = yTop - titleH;
            double leftColStartX = xLeft;
            double rightColStartX = xLeft + singleColW + colGap;

            for (int i = 0; i < totalItems; i++)
            {
                bool isLeftCol = i < itemsInLeftCol;
                int rowIndex = isLeftCol ? i : (i - itemsInLeftCol);
                double currentStartX = isLeftCol ? leftColStartX : rightColStartX;

                double rectTop = listTopY - (rowIndex * itemH);
                double rectBottom = rectTop - itemH;

                double val = levels[i];

                // 这里的 logicIndex 依然使用 autoStep 计算，保证颜色准确
                int logicIndex = (int)Math.Round((val - startZ) / autoStep);
                if (logicIndex < 0) logicIndex = 0;
                int colorId = _contourColors[logicIndex % _contourColors.Length];

                double cbX = currentStartX + colPadding;

                // 填充
                LinInfo fillPen = new LinInfo { LinStyID = 1, OutClr = new int[] { colorId }, OutPenW = new float[] { (float)(0.1 * unit) } };
                double fillStep = 0.15 * unit;
                for (double h = rectBottom + fillStep / 2; h < rectTop; h += fillStep)
                {
                    GeoVarLine hLine = new GeoVarLine();
                    hLine.Append(new Dot(cbX, h));
                    hLine.Append(new Dot(cbX + colorBoxW, h));
                    GeoLines gl = new GeoLines(); gl.Append(hLine);
                    lineCls.Append(gl, null, fillPen);
                }
                // 边框
                DrawRect(lineCls, cbX, rectBottom, cbX + colorBoxW, rectTop, framePen);

                // 文字
                string valText = val.ToString("0.##");
                TextAnno numAnn = new TextAnno { Text = valText, Height = numSize, Width = numSize * 0.7f };
                numAnn.AnchorDot = new Dot(cbX + colorBoxW + textGap, (rectTop + rectBottom) / 2.0 - numSize / 2.0);
                annCls.Append(numAnn, null, txtInfo);
            }
        }

        public StandardQuickMap(Map basemap, GeoPolygon geopolygon)
        {
            InitializeComponent();
            InitDefaultStyle();
            map = basemap;
            if (geopolygon != null)
            {
                Dots3D[] dots = geopolygon.Dots;
                if (dots != null)
                {
                    foreach (Dots3D dots3D in dots)
                    {
                        for (int j = 0; j < dots3D.Count; j++)
                        {
                            Dot3D item = dots3D.GetItem(j);
                            srcDataPolygon.Add(new Dot(item.X, item.Y));
                        }
                    }
                }
            }
            srcDataMode = 1;
        }

        public static bool IsDataOk(int flag)
        {
            bool flag2 = false;
            string text = "";
            switch (flag)
            {
                case 0:
                    text = "地图的空间参照系无效";
                    break;
                case -100:
                    text = "空间参照系范围无效";
                    break;
                case -200:
                    text = "纬度无效";
                    break;
                case -201:
                    text = "经度无效";
                    break;
                default:
                    flag2 = true;
                    break;
            }
            if (!flag2)
            {
                XMessageBox.Information(text);
            }
            return flag2;
        }
        private Rect GetPolygonRange(List<Dot> dots)
        {
            if (dots == null || dots.Count == 0) return new Rect();
            double minX = dots[0].X, maxX = dots[0].X;
            double minY = dots[0].Y, maxY = dots[0].Y;
            foreach (var d in dots)
            {
                if (d.X < minX) minX = d.X;
                if (d.X > maxX) maxX = d.X;
                if (d.Y < minY) minY = d.Y;
                if (d.Y > maxY) maxY = d.Y;
            }
            return new Rect(minX, minY, maxX, maxY);
        }

        public int InitUI()
        {
            if (map == null || map.SRSInfo == null || map.Range == null)
            {
                return 0;
            }
            isImageLayer = map.get_Layer(0).Type == LayerType.Image;
            isMapOK = true;
            largeImgList.ImageSize = imageSize;
            srcSRS = map.SRSInfo;
            if (srcSRS.Type == SRefType.JWD)
            {
                isProjectSrcSRS = false;
                currentSRS = new SRefData();
                scale = 1;
            }
            else if (srcSRS.Type == SRefType.PRJ)
            {
                isProjectSrcSRS = true;
                currentSRS = srcSRS;
                Rect targetRange = (srcDataPolygon.Count > 0) ? GetPolygonRange(srcDataPolygon) : map.Range;
                scale = CalculateSmartScale(targetRange);
            }
            else
            {
                isProjectSrcSRS = false;
                currentSRS = srcSRS;
                scale = 1000;
            }
            bool flag = false;
            switch (srcDataMode)
            {
                case 0:
                    flag = InitUI_Polygon();
                    break;
                case 1:
                    flag = InitUI_Polygon();
                    break;
            }

            string text = "未知";

            if (srcSRS.Type == SRefType.JWD || srcSRS.Type == SRefType.PRJ)
            {
                text = LanguageConvert.SRefLenUnitConvert(GetSrsUnit(srcSRS));
            }

         ((Control)(object)groupControl_DataRange).Text = "范围(单位:" + text;

            if (isProjectSrcSRS)
            {
                GroupControl val = groupControl_DataRange;
                ((Control)(object)val).Text = ((Control)(object)val).Text + "  " + "源比例尺" + "1:" + GetSrsRate(srcSRS);
            }
         ((Control)(object)groupControl_DataRange).Text += ")";
            if (isImageLayer)
            {
                if (srcSRS.Type == SRefType.JWD)
                {
                    scale = 1;
                }
                else if (srcSRS.Type == SRefType.PRJ)
                {
                    scale = 1000;
                }
             ((Control)(object)comboBoxEdit_Scale).Enabled = false;
            }
         ((Control)(object)comboBoxEdit_Scale).Text = scale.ToString();
            if (!IsCanPrj())
            {
                ((Control)(object)comboBoxEdit_Scale).Enabled = true;
            }
            int num = (flag ? 1 : 0);
            if (num == 0)
            {
                return num;
            }
            SaveUI();
            // 【修改】给刷新逻辑加上 try-catch，防止计算错误导致界面卡死或布局中断
            try
            {
                RefreshUIData();
            }
            catch (Exception ex)
            {
                // 即使出错也不要崩，保持界面可用
                Console.WriteLine("刷新界面数据失败: " + ex.Message);
            }
            return checkData;
        }

        private bool InitUI_Polygon()
        {
            if (srcDataPolygon.Count == 0)
            {
                rangeForm = new RangeForm(null, srcDataMode);
            }
            else
            {
                rangeForm = new RangeForm(srcDataPolygon, srcDataMode);
            }
            rangeForm.UpdateRange += WhenUpdateRange;
            ((Control)(object)rangeForm).Dock = DockStyle.Fill;
            ((Control)(object)groupControl_DataRange).Controls.Add((Control)(object)rangeForm);
            return true;
        }

        private bool SaveUI()
        {
            if (srcDataPolygon.Count == 0)
            {
                return false;
            }
            List<Dot> list = null;
            if (isProjectSrcSRS)
            {
                list = srcDataPolygon;
            }
            else
            {
                int num = 20;
                Transformation transformation = new Transformation();
                transformation.IsProjTrans = true;
                transformation.SetSourceSRef(srcSRS);
                transformation.SetTargetSRef(currentSRS);
                double num2 = 0.0;
                double num3 = 0.0;
                list = new List<Dot>();
                for (int i = 0; i < srcDataPolygon.Count - 1; i++)
                {
                    Dot dot = srcDataPolygon[i];
                    Dot dot2 = srcDataPolygon[i + 1];
                    double num4 = (dot2.X - dot.X) / (double)(num + 1);
                    double num5 = (dot2.Y - dot.Y) / (double)(num + 1);
                    for (int j = 0; j <= num; j++)
                    {
                        num2 = 0.0;
                        num3 = 0.0;
                        transformation.LpToMp(dot.X + (double)j * num4, dot.Y + (double)j * num5, ref num2, ref num3);
                        list.Add(new Dot(num2, num3));
                    }
                }
                list.Add(list[0].Clone());
            }
            if (list == null || list.Count == 0)
            {
                return false;
            }
            Rect rect = dataRange;
            double xMax = (dataRange.XMin = list[0].X);
            rect.XMax = xMax;
            Rect rect2 = dataRange;
            xMax = (dataRange.YMax = list[0].Y);
            rect2.YMin = xMax;
            for (int k = 0; k < list.Count; k++)
            {
                dataRange.XMin = ((dataRange.XMin > list[k].X) ? list[k].X : dataRange.XMin);
                dataRange.YMin = ((dataRange.YMin > list[k].Y) ? list[k].Y : dataRange.YMin);
                dataRange.XMax = ((dataRange.XMax < list[k].X) ? list[k].X : dataRange.XMax);
                dataRange.YMax = ((dataRange.YMax < list[k].Y) ? list[k].Y : dataRange.YMax);
            }
            commonMapDecoration.Init(currentSRS, dataRange, GetSrsUnit(currentSRS), scale);
            object property = commonMapDecoration.GetProperty("CheckAutoMapDecoration");
            if (property != null)
            {
                checkData = (int)property;
            }
            return true;
        }

        private void simpleButton_OK_Click(object sender, EventArgs e)
        {
            if (isMapOK)
            {
                Map map = null;
                map = MakeData();
                if (map != null && this.map.Parent is Document document)
                {
                    document.GetMaps().Append(map);
                    if (app != null)
                    {
                        app.WorkSpaceEngine.FireMenuItemClickEvent("MapGIS.WorkSpace.Style.PreviewMap", map);
                        app.WorkSpaceEngine.GetMapControl(map)?.Restore();
                    }
                }
            }
         ((Form)this).DialogResult = DialogResult.OK;
            ((Form)this).Close();
        }
        /// <summary>
        /// 【增强版】智能获取网格数据的极值 (Min/Max)
        /// 能够自动识别 ZMin, MinVal, MinValue, Min 等常见属性名
        /// </summary>
        private void GetGridMinMax(out double min, out double max)
        {
            // 1. 初始化
            min = 0;
            max = 100;
            bool found = false;

            if (map == null) return;

            List<MapLayer> layers = GetAllLeafLayers(map);

            foreach (MapLayer layer in layers)
            {
                // 跳过无关图层
                if (layer.Name.Contains("图框") || layer.Name.Contains("注记") || layer.Name.Contains("图例"))
                    continue;

                // 获取数据对象 (这通常是 RasterDataSet)
                object data = layer.GetData();
                if (data == null) continue;

                try
                {
                    // =============================================================
                    // ★★★ 核心修复：尝试获取“波段” (Band) ★★★
                    // =============================================================
                    object targetObject = data; // 默认查 data
                    Type t = data.GetType();

                    // 检查是否有 GetRasterBand 方法 (说明它是数据集)
                    var getBandMethod = t.GetMethod("GetRasterBand");
                    if (getBandMethod != null)
                    {
                        // 尝试获取第 1 波段 (索引可能是 1 或 0，VisualizationHelper里用的是1)
                        object band = null;
                        try
                        {
                            // 先试着拿 Band 1
                            band = getBandMethod.Invoke(data, new object[] { 1 });
                        }
                        catch { }

                        if (band == null)
                        {
                            try
                            {
                                // 如果失败，试着拿 Band 0
                                band = getBandMethod.Invoke(data, new object[] { 0 });
                            }
                            catch { }
                        }

                        // 如果成功拿到了波段，我们就查波段的属性，而不是数据集的属性
                        if (band != null)
                        {
                            targetObject = band;
                            // 更新类型信息，准备反射
                            t = targetObject.GetType();
                        }
                    }
                    // =============================================================

                    // 下面开始在 targetObject (可能是波段，也可能是数据集) 上找极值
                    string[] minPropNames = new string[] { "MinValue", "MinVal", "ZMin", "Min", "Minimum" };
                    string[] maxPropNames = new string[] { "MaxValue", "MaxVal", "ZMax", "Max", "Maximum" };

                    double? tempMin = null;
                    double? tempMax = null;

                    // 找最小值
                    foreach (string name in minPropNames)
                    {
                        var prop = t.GetProperty(name);
                        if (prop != null)
                        {
                            object val = prop.GetValue(targetObject, null);
                            if (val != null) { tempMin = Convert.ToDouble(val); break; }
                        }
                    }

                    // 找最大值
                    foreach (string name in maxPropNames)
                    {
                        var prop = t.GetProperty(name);
                        if (prop != null)
                        {
                            object val = prop.GetValue(targetObject, null);
                            if (val != null) { tempMax = Convert.ToDouble(val); break; }
                        }
                    }

                    // 验证结果
                    if (tempMin.HasValue && tempMax.HasValue)
                    {
                        min = tempMin.Value;
                        max = tempMax.Value;

                        // 防止 Min == Max 导致除零
                        if (Math.Abs(max - min) < 0.000001) max = min + 1.0;

                        // 成功了！弹窗提示一下 (调试通过后可注释掉)
                        // MessageBox.Show($"【成功】在图层 [{layer.Name}] 中读取到数据！\nMin = {min}\nMax = {max}\n(读取对象: {t.Name})");

                        found = true;
                        break; // 找到了就停止
                    }
                }
                catch (Exception ex)
                {
                    // 仅仅是调试用，正式发布可去掉
                    // MessageBox.Show($"读取图层 {layer.Name} 出错: " + ex.Message);
                }
            }

            if (!found)
            {
                // 依然没找到，说明属性名可能还是不对，或者波段也没取到
                // MessageBox.Show("【失败】已找到 gra.grd，但无法读取其 MinValue/MaxValue 属性。\n图例将使用默认范围 0-100。");
            }
        }

        private Map MakeData()
        {
            // [调试 Step 1]
            // MessageBox.Show("开始执行 MakeData 成图流程...");

            // 1. 基础初始化 (保持原逻辑)
            _dynamicLegends.Clear();
            if (srcDataPolygon == null || this.map == null)
            {
                //MessageBox.Show("【调试错误】源数据范围或底图为空，无法成图！");
                return null;
            }

            Map map = new Map();
            map.FromXML(this.map.ToXML());

            // 设置投影/比例尺 (保持原逻辑)
            if (scale > 1)
            {
                dstMapSRS = GetProjSrs();
                map.SetProjTrans(dstMapSRS);
                map.IsProjTrans = true;
            }
            else
            {
                dstMapSRS = srcSRS;
            }

            // =========================================================
            // ↓↓↓ 2. 核心修改：数据读取阶段 (读属性/几何) ↓↓↓
            // =========================================================

            // 定义数据容器
            TransmitterInfo tranInfo = null;
            List<StationPointInfo> allStationInfos = new List<StationPointInfo>();

            // 获取所有图层
            List<MapLayer> allLayers = GetAllLeafLayers(map);
            //MessageBox.Show($"【调试】底图中共检测到 {allLayers.Count} 个图层，开始遍历查找数据...");

            foreach (MapLayer layer in allLayers)
            {
                string layerName = layer.Name;
                // 尝试获取别名 (保持原逻辑)
                try
                {
                    var aliasProp = layer.GetType().GetProperty("AliasName");
                    if (aliasProp == null) aliasProp = layer.GetType().GetProperty("Alias");
                    if (aliasProp != null)
                    {
                        string alias = aliasProp.GetValue(layer, null) as string;
                        if (!string.IsNullOrEmpty(alias)) layerName += "|" + alias;
                    }
                }
                catch { }

                SFeatureCls sfCls = layer.GetData() as SFeatureCls;
                if (sfCls == null) continue;

                // --- A. 发射源处理 ---
                if (layerName.Contains("发射源"))
                {
                    // MessageBox.Show($"【调试】发现发射源图层：[{layerName}]\n正在尝试读取属性坐标...");

                    // ★ 调用专门的属性读取方法
                    tranInfo = GetTransmitterData(sfCls);

                    // 隐藏原始图层 (因为我们要重画)
                    TrySetEnumProperty(layer, "State", "Visible", "0");
                }
                // --- B. 测点处理 ---
                else if (layerName.Contains("测点"))
                {
                    // MessageBox.Show($"【调试】发现测点图层：[{layerName}]\n正在尝试读取几何坐标...");

                    // ★ 调用专门的几何读取方法
                    var stations = GetStationData(sfCls);

                    // 加入总列表 (支持多个测点图层)
                    if (stations.Count > 0)
                    {
                        allStationInfos.AddRange(stations);
                    }

                    // 隐藏原始图层
                    TrySetEnumProperty(layer, "State", "Visible", "0");
                }
            }

            // =========================================================
            // ↓↓↓ 3. 准备绘图图层 (GraphicsData) ↓↓↓
            // =========================================================

            if (map.GraphicsData == null)
            {
                try { ForceSet(map, "GraphicsData", new GraphicsData()); } catch { }
            }
            GraphicsData graphicsData = map.GraphicsData;

            // 确保线图层存在 (用于画图框、发射源框、测点圆)
            if (graphicsData != null && graphicsData.LinLayer == null)
            {
                MapLayer newLinLayer = (MapLayer)Activator.CreateInstance(typeof(MapLayer), true);
                TrySetEnumProperty(newLinLayer, "Type", "Line", "Lin");
                newLinLayer.Name = "图框线";
                TrySetEnumProperty(newLinLayer, "State", "Visible", "1");
                SFeatureCls newLinCls = new SFeatureCls();
                ForceSet(newLinCls, "ClsType", XClsType.SFCls);
                TrySetEnumProperty(newLinCls, "GeomType", "Line", "Lin");
                SetFrameXclsStru(newLinCls);
                if (!TrySetProperty(newLinLayer, "Data", newLinCls)) TryInvokeMethod(newLinLayer, "SetData", new object[] { newLinCls });
                AddLayerToMap(map, newLinLayer);
                ForceSet(graphicsData, "LinLayer", newLinLayer);
            }
            // 确保注记图层存在 (用于画测点名、图名)
            if (graphicsData != null && graphicsData.AnnLayer == null)
            {
                MapLayer newAnnLayer = (MapLayer)Activator.CreateInstance(typeof(MapLayer), true);
                TrySetEnumProperty(newAnnLayer, "Type", "Annotation", "Ann", "Label");
                newAnnLayer.Name = "图廓注记";
                TrySetEnumProperty(newAnnLayer, "State", "Visible", "1");
                AnnotationCls newAnnCls = new AnnotationCls();
                TrySetEnumProperty(newAnnCls, "ClsType", "AnnotationCls", "AnnCls", "Annotation");
                TrySetEnumProperty(newAnnCls, "GeomType", "Pnt", "Point");
                SetFrameXclsStru(newAnnCls);
                if (!TrySetProperty(newAnnLayer, "Data", newAnnCls)) TryInvokeMethod(newAnnLayer, "SetData", new object[] { newAnnCls });
                AddLayerToMap(map, newAnnLayer);
                ForceSet(graphicsData, "AnnLayer", newAnnLayer);
            }

            // 获取绘图对象的引用
            SFeatureCls drawLineCls = graphicsData.LinLayer.GetData() as SFeatureCls;
            AnnotationCls drawAnnCls = graphicsData.AnnLayer.GetData() as AnnotationCls;
            SetFrameXclsStru(drawLineCls);
            SetFrameXclsStru(drawAnnCls);

            // 处理覆盖区数据 (保持原逻辑)
            if (graphicsData.RegLayer != null) AddCoverData(graphicsData.RegLayer.GetData() as SFeatureCls, map.Range);

            // =========================================================
            // ↓↓↓ 4. 计算布局参数 scaleUnit (绘图尺寸基准) ↓↓↓
            // =========================================================

            // 【修改前】 使用全图范围，导致框选无效，永远显示全图
            // Rect innerRect = map.Range; 

            // 【修改后】 使用这一行！使用 SaveUI 中计算好的框选范围
            Rect innerRect = this.dataRange.Clone();

            // 如果 dataRange 为空（防御性编程），再回退到全图
            if (innerRect == null || (innerRect.XMax - innerRect.XMin) <= 0)
            {
                innerRect = map.Range;
            }

            // ---------------------------------------------------------
            // 下面的逻辑依然依赖 innerRect，现在它是框选范围了
            // ---------------------------------------------------------

            double mapWidth = innerRect.XMax - innerRect.XMin;
            double mapHeight = innerRect.YMax - innerRect.YMin;

            // 这里的 300 意味着假设图纸宽度大约 300mm
            // scaleUnit 决定了字体、线宽的相对大小
            double minSide = Math.Min(mapWidth, mapHeight);
            double scaleUnit = minSide / 300.0;

            // 增加一个上限限制，防止长条图导致字体过大
            if (scaleUnit > (mapWidth / 150.0))
            {
                scaleUnit = mapWidth / 150.0;
            }

            // 获取当前样式索引
            int selectedStyleIndex = 0;
            if (listView_Style != null && listView_Style.SelectedItems.Count > 0 && listView_Style.SelectedItems[0].Tag != null)
            {
                selectedStyleIndex = Convert.ToInt32(listView_Style.SelectedItems[0].Tag);
            }

            // 计算外框
            double marginBottom = 20.0 * scaleUnit;
            double marginNormal = 30.0 * scaleUnit;
            Rect outerRect = new Rect(innerRect.XMin - marginNormal, innerRect.YMin - marginBottom, innerRect.XMax + marginNormal, innerRect.YMax + marginNormal);

            // =========================================================
            // ↓↓↓ 5. 核心修改：绘制读取到的业务数据 ↓↓↓
            // =========================================================

            // --- 5.1 绘制发射源 (智能闭合逻辑) ---
            if (tranInfo != null && tranInfo.IsValid && drawLineCls != null)
            {
                // 构造线对象
                GeoVarLine tranLine = new GeoVarLine();

                // 1. 添加所有坐标点
                foreach (Dot dot in tranInfo.Points)
                {
                    tranLine.Append(new Dot(dot.X, dot.Y));
                }

                // 2. 【核心逻辑】根据点数决定是否闭合
                // 如果点数大于2 (例如三角形、矩形)，则强制把终点连回起点
                if (tranInfo.Points.Count > 2)
                {
                    // 获取第一个点的坐标
                    Dot firstPoint = tranInfo.Points[0];
                    // 添加到末尾，形成闭合回路
                    tranLine.Append(new Dot(firstPoint.X, firstPoint.Y));
                }

                GeoLines gl = new GeoLines();
                gl.Append(tranLine);

                // 设置样式：蓝色(5) 粗线
                LinInfo tranPen = new LinInfo();
                tranPen.LinStyID = 1;
                tranPen.OutClr = new int[] { 5 };
                tranPen.OutPenW = new float[] { (float)(0.5 * scaleUnit) };

                drawLineCls.Append(gl, null, tranPen);

                // 自动添加图例
                if (!_dynamicLegends.Exists(x => x.Name == "发射源"))
                    _dynamicLegends.Add(new LegendItemInfo { Name = "发射源", SymID = 0, Color = 5 });
            }

            // =========================================================
            // ↓↓↓ 5.2 绘制所有测点 (修改为：红色实心圆 + 注记) ↓↓↓
            // =========================================================
            if (allStationInfos.Count > 0 && drawLineCls != null)
            {
                //MessageBox.Show($"【调试】准备绘制 {allStationInfos.Count} 个测点...\n(样式：红色实心圆)");

                // 1. 定义填充用的画笔 (红色)
                LinInfo solidRedPen = new LinInfo();
                solidRedPen.LinStyID = 1;       // 实线
                solidRedPen.OutClr = new int[] { 6 }; // 红色 (MapGIS色号6)

                // 笔触设为 0.1mm，用于填充内部
                float fillPenWidth = (float)(0.1 * scaleUnit);
                solidRedPen.OutPenW = new float[] { fillPenWidth };

                // 2. 定义注记样式
                TextAnnInfo textInfo = new TextAnnInfo { Ovprnt = true };
                float labelSize = (float)(2.0 * scaleUnit); // 字高 2mm

                // 3. 循环绘制每一个点
                foreach (var st in allStationInfos)
                {
                    // --- 绘制实心圆 ---
                    double targetRadius = 0.8 * scaleUnit; // 目标半径 0.8mm (直径1.6mm)

                    // ★核心技巧：画同心圆填充★
                    // 步长设为笔宽的一半，保证线条重叠，没有空隙
                    double step = fillPenWidth * 0.5;

                    // 从极小的半径开始，一圈圈画到目标半径
                    for (double r = step; r <= targetRadius; r += step)
                    {
                        DrawCircle(drawLineCls, st.X, st.Y, r, solidRedPen);
                    }
                    // 最后补一刀最外圈，保证边缘圆滑
                    DrawCircle(drawLineCls, st.X, st.Y, targetRadius, solidRedPen);

                    // --- 绘制文字注记 ---
                    if (!string.IsNullOrEmpty(st.Name) && drawAnnCls != null)
                    {
                        TextAnno ann = new TextAnno();
                        ann.Text = st.Name;
                        ann.Height = labelSize;
                        ann.Width = labelSize * 0.7f;
                        // 标注位置：圆心右上方 (稍微避开实心圆)
                        ann.AnchorDot = new Dot(st.X + targetRadius * 1.2, st.Y + targetRadius * 1.2);
                        drawAnnCls.Append(ann, null, textInfo);
                    }
                }

                // 更新图例信息 (改为红色实心)
                if (!_dynamicLegends.Exists(x => x.Name == "测点"))
                    _dynamicLegends.Add(new LegendItemInfo { Name = "测点", SymID = 1, Color = 6 }); // 6=红
            }
            else
            {
                //MessageBox.Show("【调试警告】测点列表为空，未绘制任何测点！");
            }

            // =========================================================
            // ↓↓↓ 6. 绘制标准图框装饰 (保持原逻辑) ↓↓↓
            // =========================================================

            if (drawLineCls != null)
            {
                // 1. 绘制内图框 (细线)
                GeoLines innerBox = CreateBoxLine(innerRect);
                LinInfo innerInfo = new LinInfo();
                innerInfo.LinStyID = 1;
                innerInfo.OutClr = new int[] { 1 };
                innerInfo.OutPenW = new float[] { (float)(0.1 * scaleUnit) };
                drawLineCls.Append(innerBox, null, innerInfo);

                // 2. 绘制外图框 (粗线)
                GeoLines outerBox = CreateBoxLine(outerRect);
                LinInfo outerInfo = new LinInfo();
                outerInfo.LinStyID = 1;
                outerInfo.OutClr = new int[] { 1 };
                outerInfo.OutPenW = new float[] { (float)(0.8 * scaleUnit) };
                drawLineCls.Append(outerBox, null, outerInfo);

                // 根据样式绘制不同内容
                if (selectedStyleIndex == 200) // 测线成图样式
                {
                    DrawMapGrid(drawLineCls, innerRect, scaleUnit);
                    DrawGraphicScale(drawLineCls, drawAnnCls, outerRect, scaleUnit, scale);
                    DrawSurveyLineTable(drawLineCls, drawAnnCls, outerRect, scaleUnit);
                    DrawSurveyLineLegend(drawLineCls, drawAnnCls, outerRect, scaleUnit);
                }
                // 在 MakeData() 方法中

                else if (selectedStyleIndex == 300) // 重磁数据成图样式
                {
                    // MessageBox.Show("4. 命中样式 300！准备绘制等值线图例 (DrawContourLegend)...");
                    DrawMapGrid(drawLineCls, innerRect, scaleUnit);
                    DrawGraphicScale(drawLineCls, drawAnnCls, outerRect, scaleUnit, scale);
                    DrawSurveyLineTable(drawLineCls, drawAnnCls, outerRect, scaleUnit);

                    // ★★★ 使用新方法 ★★★
                    DrawContourLegend(drawLineCls, drawAnnCls, outerRect, scaleUnit);
                }
                else if (selectedStyleIndex == 0) // 标准分幅
                {
                    DrawJoinTable(drawLineCls, outerRect.XMin, outerRect.YMax, scaleUnit);
                    DrawMapGrid(drawLineCls, innerRect, scaleUnit);
                }
                else if (selectedStyleIndex == 100) // 实际材料图
                {
                    DrawMapGrid(drawLineCls, innerRect, scaleUnit);
                    DrawSurveyTableLines(drawLineCls, outerRect, scaleUnit);
                }
            }

            // 绘制注记 (标题、坐标等)
            if (drawAnnCls != null)
            {
                // =========================================================
                // ★★★ 核心修改：绘制主标题 ★★★
                // =========================================================
                if (texts.ContainsKey("主标题"))
                {
                    string titleText = texts["主标题"].ToString();
                    if (!string.IsNullOrEmpty(titleText))
                    {
                        TextAnno titleAnn = new TextAnno();
                        titleAnn.Text = titleText;

                        // 设置字号 (标题通常很大，例如 12mm - 15mm)
                        float titleH = (float)(15.0 * scaleUnit);
                        titleAnn.Height = titleH;
                        titleAnn.Width = titleH; // 正方字

                        // 字体设置 (黑体索引通常为2，根据MapGIS字库调整，这里使用默认或指定)
                        // titleAnn.FontID = 2; // 如果需要强制黑体可以取消注释

                        // 计算居中位置
                        double centerX = (outerRect.XMin + outerRect.XMax) / 2.0;
                        // 计算文字总长度 (简单估算：字数 * 字宽)
                        // 如果包含数字字母，宽度计算可以更精细，这里按全角字符估算居中
                        double textLen = 0;
                        foreach (char c in titleText) textLen += (c < 128 ? 0.5 : 1.0) * titleH;

                        // 定位：外框顶部上方 15mm 处
                        titleAnn.AnchorDot = new Dot(centerX - (textLen / 2.0), outerRect.YMax + (15.0 * scaleUnit));

                        drawAnnCls.Append(titleAnn, null, new TextAnnInfo { Ovprnt = true });
                    }
                }
                // =========================================================

                // 绘制坐标标注
                if (selectedStyleIndex == 200 || selectedStyleIndex == 300) // 样式200和300都画坐标注记
                {
                    DrawGridLabels(drawAnnCls, innerRect, scaleUnit);
                }
            }

            // =========================================================
            // ↓↓↓ 7. 设置最终范围并返回 ↓↓↓
            // =========================================================

            Rect finalRange = new Rect();
            finalRange.XMin = outerRect.XMin - (50 * scaleUnit);
            finalRange.XMax = outerRect.XMax + (50 * scaleUnit);
            finalRange.YMax = outerRect.YMax + (50 * scaleUnit);
            finalRange.YMin = outerRect.YMin - (100 * scaleUnit); // 底部预留多一点给表格

            map.IsCustomEntireRange = true;
            map.SetEntireRange(finalRange);
            map.Name = GetNewMapName();

            MessageBox.Show("MakeData 流程执行完毕，生成新图层成功！");
            return map;
        }
        /// <summary>
        /// 【修正版】读取所有测点（增加 COM 对象安全释放检查，防止报错）
        /// </summary>
        private List<StationPointInfo> GetStationData(SFeatureCls layer)
        {
            // MessageBox.Show("【调试】开始读取并裁剪测点..."); 
            List<StationPointInfo> stations = new List<StationPointInfo>();
            if (layer == null) return stations;

            RecordSet rs = null;
            try
            {
                rs = layer.Select(null);
                if (rs == null) return stations;
                rs.MoveFirst();

                while (!rs.IsEOF)
                {
                    IGeometry geom = null;
                    Record att = null;
                    try
                    {
                        geom = rs.Geometry;
                        att = rs.Att;

                        if (geom is GeoPoints pnts && pnts.Count > 0)
                        {
                            Dot3D dot = pnts.GetItem(0);

                            // =================================================
                            // ★★★ 核心修改：增加空间范围判断 ★★★
                            // =================================================
                            if (IsPointInClipRegion(dot.X, dot.Y))
                            {
                                string name = "";
                                if (att != null)
                                {
                                    object val = att["测点号"];
                                    if (val == null) val = att["点号"];
                                    if (val == null) val = att["Name"];
                                    if (val != null) name = val.ToString().Trim();
                                }

                                stations.Add(new StationPointInfo
                                {
                                    X = dot.X,
                                    Y = dot.Y,
                                    Name = name
                                });
                            }
                            // else { 也就是在多边形外面的点，直接丢弃，不添加到 list }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (geom != null && System.Runtime.InteropServices.Marshal.IsComObject(geom))
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(geom);
                        if (att != null && System.Runtime.InteropServices.Marshal.IsComObject(att))
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(att);
                    }
                    rs.MoveNext();
                }
            }
            finally
            {
                if (rs != null && System.Runtime.InteropServices.Marshal.IsComObject(rs))
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
            return stations;
        }


        /// <summary>
                /// 【新增辅助方法】递归获取地图中所有的子图层（穿透图层组）
                /// </summary>
        private List<MapLayer> GetAllLeafLayers(Map map)
        {
            List<MapLayer> list = new List<MapLayer>();
            if (map == null) return list;

            Stack<object> enumStack = new Stack<object>();

            object rootEnum = TryInvokeMethod(map, "GetLayerEnum", null);
            if (rootEnum == null)
            {
                var prop = map.GetType().GetProperty("LayerEnum");
                if (prop != null) rootEnum = prop.GetValue(map, null);
            }

            if (rootEnum != null) enumStack.Push(rootEnum);

            while (enumStack.Count > 0)
            {
                object curEnum = enumStack.Pop();
                try { TryInvokeMethod(curEnum, "MoveFirst", null); } catch { }

                while (true)
                {
                    MapLayer layer = null;
                    try
                    {
                        layer = TryInvokeMethod(curEnum, "Next", null) as MapLayer;
                    }
                    catch { }

                    if (layer == null) break;

                    string typeStr = layer.Type.ToString().ToLower();

                    // 如果是图层组，则获取其子枚举器并入栈
                    if (typeStr.Contains("group") || typeStr.Contains("grp"))
                    {
                        object childEnum = TryInvokeMethod(layer, "GetLayerEnum", null);
                        if (childEnum != null) enumStack.Push(childEnum);
                    }
                    else
                    {
                        list.Add(layer);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 【新增辅助方法】处理单个点图层：修改样式 + 记录图例
        /// </summary>
        private void ProcessPointLayer(SFeatureCls sfCls, string legendName, int symID, int colorID)
        {
            if (sfCls == null) return;

            // 1. 记录图例
            if (!_dynamicLegends.Exists(x => x.Name == legendName))
            {
                _dynamicLegends.Add(new LegendItemInfo
                {
                    Name = legendName,
                    SymID = symID,
                    Color = colorID,
                    Size = 9f
                });
            }

            // 2. 获取所有 ID 并修改样式
            long[] oids = null;
            object result = TryInvokeMethod(sfCls, "GetAllIDs", null);
            if (result == null) result = TryInvokeMethod(sfCls, "GetIDs", null);
            if (result == null) result = TryInvokeMethod(sfCls, "GetOIDs", null);

            if (result is long[]) oids = (long[])result;
            else if (result is int[]) oids = Array.ConvertAll((int[])result, id => (long)id);

            if (oids != null && oids.Length > 0)
            {
                // 计算合适的大小
                float targetSizeMM = 5.0f;
                float mapUnitSize = targetSizeMM;
                if (scale > 100)
                {
                    mapUnitSize = targetSizeMM * (scale / 1000.0f);
                }

                foreach (long id in oids)
                {
                    object geomObj = sfCls.GetGeometry(id);
                    IGeometry updateGeom = geomObj as IGeometry;

                    PntInfo pInfo = new PntInfo();
                    pInfo.LibID = 0;
                    pInfo.SymID = symID;
                    pInfo.OutClr = new int[] { colorID };
                    pInfo.Height = mapUnitSize;
                    pInfo.Width = mapUnitSize;

                    // 不设置 Scale，防止变小
                    // try { TrySetProperty(pInfo, "ScaleX", 1.0); ... }

                    if (updateGeom != null)
                    {
                        sfCls.Update(id, updateGeom, null, pInfo);
                    }
                }
            }
        }
        /// <summary>
        /// 【修正版】读取发射源数据（已修复 ReleaseComObject 报错）
        /// </summary>
        /// <summary>
        /// 【修正版】读取发射源数据 (支持非闭合、支持缺省)
        /// </summary>
        private TransmitterInfo GetTransmitterData(SFeatureCls layer)
        {
            TransmitterInfo info = new TransmitterInfo();
            if (layer == null) return info;

            RecordSet rs = null;
            // 使用字典临时存储，防止读取顺序乱序 (例如先读到C后读到A)
            Dictionary<string, Dot> tempDict = new Dictionary<string, Dot>();

            try
            {
                rs = layer.Select(null);
                if (rs == null) return info;
                rs.MoveFirst();

                while (!rs.IsEOF)
                {
                    Record att = null;
                    try
                    {
                        att = rs.Att;
                        if (att != null)
                        {
                            // 1. 读取点名
                            object objName = att["点名"];
                            if (objName == null) objName = att["Name"];
                            string name = objName?.ToString()?.Trim().ToUpper();

                            // 2. 读取坐标 (兼容 X/x/X坐标)
                            object objX = att["X坐标"];
                            if (objX == null) objX = att["X"];
                            if (objX == null) objX = att["x"];

                            object objY = att["Y坐标"];
                            if (objY == null) objY = att["Y"];
                            if (objY == null) objY = att["y"];

                            // 3. 解析数值
                            if (!string.IsNullOrEmpty(name) && objX != null && objY != null)
                            {
                                if (double.TryParse(objX.ToString(), out double x) &&
                                    double.TryParse(objY.ToString(), out double y))
                                {
                                    // 排除 (0,0) 的无效点 (可选，视你的数据情况而定)
                                    if (Math.Abs(x) > 0.0001 || Math.Abs(y) > 0.0001)
                                    {
                                        // 存入字典
                                        if (!tempDict.ContainsKey(name))
                                        {
                                            tempDict.Add(name, new Dot(x, y));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (att != null && System.Runtime.InteropServices.Marshal.IsComObject(att))
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(att);
                    }
                    rs.MoveNext();
                }

                // 4. 【关键步骤】按 A->B->C->D... 的顺序将存在的点加入列表
                // 这样即使缺了 D，列表里就只有 A,B,C，画出来就是折线，不会飞向原点
                string[] order = { "A", "B", "C", "D", "E", "F" }; // 可以按需扩充
                foreach (string key in order)
                {
                    if (tempDict.ContainsKey(key))
                    {
                        info.Points.Add(tempDict[key]);
                    }
                }
            }
            catch { }
            finally
            {
                if (rs != null && System.Runtime.InteropServices.Marshal.IsComObject(rs))
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }

            // 5. 校验：检查第一个点是否在裁剪范围内 (逻辑保持不变)
            if (info.Points.Count > 0)
            {
                Dot firstPoint = info.Points[0];
                // 如果第一个点不在范围内，视为整个数据无效 (过滤)
                if (!IsPointInClipRegion(firstPoint.X, firstPoint.Y))
                {
                    info.Points.Clear(); // 清空列表，标记为无效
                }
            }

            return info;
        }

        // [请将此方法添加到 StandardQuickMap 类中]

        /// <summary>
                        /// 【新增】绘制正下方的数字和图形比例尺
                        /// </summary>
                        /// <param name="lineCls">线要素类，用于画刻度线</param>
                        /// <param name="annCls">注记要素类，用于画文字</param>
                        /// <param name="outRect">图廓外框范围，用于定位</param>
                        /// <param name="unit">1毫米对应的逻辑坐标长度</param>
                        /// <param name="mapScale">当前地图比例尺分母</param>
        // 【完全替换这个方法】
        private void DrawGraphicScale(SFeatureCls lineCls, AnnotationCls annCls, Rect rect, double unit, int mapScale)
        {
            // 参数检查
            if (lineCls == null || annCls == null || rect == null) return;

            // ====================================================================
            // 1. 定位逻辑修改：从“居中”改为“靠左”
            // ====================================================================

            // 图例通常在左下角，宽度大约是 40.0 * unit。
            // 为了不重叠，我们将比例尺的起始点放在图例右侧，留出 10.0 * unit 的间隙。
            // 所以偏移量 = 40 (图例宽) + 10 (间隙) = 50.0 * unit
            double offsetFromLeft = 50.0 * unit;

            // 计算比例尺左起点的 X 坐标
            double startX = rect.XMin + offsetFromLeft;

            // Y轴定位 (保持不变)
            // 基准线在图框底边下方 35mm 处
            double baseY = rect.YMin - (35.0 * unit);
            // 文字显示在尺子上方 6mm 处
            double textY = baseY + (6.0 * unit);

            // ====================================================================
            // 2. 尺寸计算
            // ====================================================================
            int segmentCount = 4;
            double segmentLenMm = 10.0;
            double segmentLen = segmentLenMm * unit;
            double totalLen = segmentCount * segmentLen;
            double valuePerSegment = mapScale / 100.0;

            // 计算比例尺本身的中心点 X (用于上方数字文字居中)
            double barCenterX = startX + (totalLen / 2.0);

            // 样式定义
            LinInfo lineInfo = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };
            TextAnnInfo txtInfo = new TextAnnInfo { Ovprnt = true };

            float fontSizeNum = (float)(3.5 * unit);
            float fontSizeScale = (float)(3.0 * unit);

            // ====================================================================
            // 3. 绘制内容
            // ====================================================================

            // --- A. 数字比例尺 (例如 "1 : 10 000") ---
            string scaleText = $"1 : {mapScale.ToString("N0").Replace(",", " ")}";
            TextAnno numAnno = new TextAnno();
            numAnno.Text = scaleText;
            numAnno.Height = fontSizeNum;
            numAnno.Width = fontSizeNum;

            // 计算文字长度并居中于比例尺上方
            // 注意：这里使用 barCenterX 而不是全局 centerX
            double numTextLen = scaleText.Length * numAnno.Width; // 简单估算宽度
            numAnno.AnchorDot = new Dot(barCenterX - numTextLen / 2.0, textY);
            annCls.Append(numAnno, null, txtInfo);

            // --- B. 图形比例尺 (主横线) ---
            GeoVarLine mainLine = new GeoVarLine();
            mainLine.Append(new Dot(startX, baseY));
            mainLine.Append(new Dot(startX + totalLen, baseY));
            GeoLines glMain = new GeoLines(); glMain.Append(mainLine);
            lineCls.Append(glMain, null, lineInfo);

            // --- C. 刻度和数值 ---
            for (int i = 0; i <= segmentCount; i++)
            {
                double currentX = startX + i * segmentLen;
                double currentValue = i * valuePerSegment;

                // 大刻度 (向上画)
                GeoVarLine tick = new GeoVarLine();
                tick.Append(new Dot(currentX, baseY));
                tick.Append(new Dot(currentX, baseY + 1.5 * unit));
                GeoLines glTick = new GeoLines(); glTick.Append(tick);
                lineCls.Append(glTick, null, lineInfo);

                // 刻度数值
                string valText = currentValue.ToString("0.##");
                TextAnno valAnno = new TextAnno();
                valAnno.Text = valText;
                valAnno.Height = fontSizeScale;
                valAnno.Width = fontSizeScale;

                double valTextLen = valText.Length * valAnno.Width;
                valAnno.AnchorDot = new Dot(currentX - valTextLen / 2.0, baseY - fontSizeScale - (1.0 * unit));
                annCls.Append(valAnno, null, txtInfo);

                // 小刻度 (仅在第一段绘制)
                if (i == 0)
                {
                    int subSegments = 5;
                    double subLen = segmentLen / subSegments;
                    for (int j = 1; j < subSegments; j++)
                    {
                        double subX = currentX + j * subLen;
                        GeoVarLine subTick = new GeoVarLine();
                        subTick.Append(new Dot(subX, baseY));
                        subTick.Append(new Dot(subX, baseY + 1.0 * unit));
                        GeoLines glSubTick = new GeoLines(); glSubTick.Append(subTick);
                        lineCls.Append(glSubTick, null, lineInfo);
                    }
                }
            }

            // --- D. 单位 "m" ---
            TextAnno unitAnno = new TextAnno();
            unitAnno.Text = "m";
            unitAnno.Height = fontSizeScale;
            unitAnno.Width = fontSizeScale;
            unitAnno.AnchorDot = new Dot(startX + totalLen + 3.0 * unit, baseY - fontSizeScale / 2.0);
            annCls.Append(unitAnno, null, txtInfo);
        }

        /// <summary>
        /// 绘制测线成图的右下角表格 (仿照 image_689f99.png)
        /// </summary>
        private void DrawSurveyLineTable(SFeatureCls lineCls, AnnotationCls annCls, Rect outRect, double unit)
        {
            // --- 1. 尺寸定义 (总高 38mm) ---
            double colWidth1 = 25.0 * unit;
            double colWidth2 = 30.0 * unit;
            double colWidth3 = 25.0 * unit;
            double colWidth4 = 40.0 * unit;
            double tableW = colWidth1 + colWidth2 + colWidth3 + colWidth4;

            double rowH = 6.0 * unit;    // 行高 6mm
            double headerH = 8.0 * unit; // 头高 8mm
            double tableH = headerH + (5 * rowH); // 总高 = 38mm

            // --- 2. 绝对定位 (核心修改) ---
            // X轴：靠右对齐
            double xBase = outRect.XMax - tableW;

            // 【关键修改】Y轴：外图框底边 - 间隙 - 表格高度
            // 这样表格就跑到了图框的外面，而不是里面
            double gapFromFrame = 15.0 * unit; // 留出 15mm 给上面的比例尺
            double yBase = outRect.YMin - gapFromFrame - tableH;

            // --- 3. 绘制表格 --- (以下代码保持不变)
            LinInfo penFrame = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };
            float fSize = (float)(3.0 * unit);

            // 外边框
            DrawLine(lineCls, xBase, yBase, xBase + tableW, yBase, penFrame);            // 底
            DrawLine(lineCls, xBase, yBase + tableH, xBase + tableW, yBase + tableH, penFrame); // 顶
            DrawLine(lineCls, xBase, yBase, xBase, yBase + tableH, penFrame);            // 左
            DrawLine(lineCls, xBase + tableW, yBase, xBase + tableW, yBase + tableH, penFrame); // 右

            // 内部竖线
            DrawLine(lineCls, xBase + colWidth1, yBase, xBase + colWidth1, yBase + 5 * rowH, penFrame);
            DrawLine(lineCls, xBase + colWidth1 + colWidth2, yBase, xBase + colWidth1 + colWidth2, yBase + 5 * rowH, penFrame);
            DrawLine(lineCls, xBase + colWidth1 + colWidth2 + colWidth3, yBase, xBase + colWidth1 + colWidth2 + colWidth3, yBase + 5 * rowH, penFrame);

            // 内部横线
            for (int i = 1; i <= 5; i++)
            {
                double ly = yBase + i * rowH;
                DrawLine(lineCls, xBase, ly, xBase + tableW, ly, penFrame);
            }

            // --- 4. 填写文字 (代码保持不变) ---
            WriteLabelInRect(annCls, "制图单位", xBase, yBase + 5 * rowH, tableW, headerH, fSize, true); // 这个就是“工区磁测平面图”
            WriteLabelInRect(annCls, "编    图", xBase, yBase + 4 * rowH, colWidth1, rowH, fSize, false);
            WriteLabelInRect(annCls, "审    核", xBase, yBase + 3 * rowH, colWidth1, rowH, fSize, false);
            WriteLabelInRect(annCls, "数字制图", xBase, yBase + 2 * rowH, colWidth1, rowH, fSize, false);
            WriteLabelInRect(annCls, "技术负责", xBase, yBase + 1 * rowH, colWidth1, rowH, fSize, false);
            WriteLabelInRect(annCls, "单位负责人", xBase, yBase, colWidth1, rowH, fSize, false);

            WriteLabelInRect(annCls, "编    图", xBase + colWidth1, yBase + 4 * rowH, colWidth2, rowH, fSize, true);
            WriteLabelInRect(annCls, "审    核", xBase + colWidth1, yBase + 3 * rowH, colWidth2, rowH, fSize, true);
            WriteLabelInRect(annCls, "数字制图", xBase + colWidth1, yBase + 2 * rowH, colWidth2, rowH, fSize, true);
            WriteLabelInRect(annCls, "技术负责", xBase + colWidth1, yBase + 1 * rowH, colWidth2, rowH, fSize, true);
            WriteLabelInRect(annCls, "单位负责人", xBase + colWidth1, yBase, colWidth2, rowH, fSize, true);

            double xRight = xBase + colWidth1 + colWidth2;
            WriteLabelInRect(annCls, "顺序号", xRight, yBase + 4 * rowH, colWidth3, rowH, fSize, false);
            WriteLabelInRect(annCls, "图    号", xRight, yBase + 3 * rowH, colWidth3, rowH, fSize, false);
            WriteLabelInRect(annCls, "比例尺", xRight, yBase + 2 * rowH, colWidth3, rowH, fSize, false);
            WriteLabelInRect(annCls, "编图日期", xRight, yBase + 1 * rowH, colWidth3, rowH, fSize, false);
            WriteLabelInRect(annCls, "资料来源", xRight, yBase, colWidth3, rowH, fSize, false);

            double xRightContent = xRight + colWidth3;
            WriteLabelInRect(annCls, "顺序号", xRightContent, yBase + 4 * rowH, colWidth4, rowH, fSize, true);
            WriteLabelInRect(annCls, "图    号", xRightContent, yBase + 3 * rowH, colWidth4, rowH, fSize, true);
            WriteLabelInRect(annCls, "比例尺", xRightContent, yBase + 2 * rowH, colWidth4, rowH, fSize, true);
            WriteLabelInRect(annCls, "编图日期", xRightContent, yBase + 1 * rowH, colWidth4, rowH, fSize, true);
            WriteLabelInRect(annCls, "资料来源", xRightContent, yBase, colWidth4, rowH, fSize, true);
        }

        // 辅助：画线
        private void DrawLine(SFeatureCls cls, double x1, double y1, double x2, double y2, LinInfo info)
        {
            GeoVarLine line = new GeoVarLine();
            line.Append(new Dot(x1, y1));
            line.Append(new Dot(x2, y2));
            GeoLines lines = new GeoLines();
            lines.Append(line);
            cls.Append(lines, null, info);
        }
        /// <summary>
        /// 【新增算法】判断点 (x, y) 是否在 srcDataPolygon 多边形范围内
        /// </summary>
        private bool IsPointInClipRegion(double x, double y)
        {
            // 1. 如果是全图模式 (isWholeRange) 或者没有裁剪多边形，则默认全部保留
            if (isWholeRange || srcDataPolygon == null || srcDataPolygon.Count < 3)
            {
                return true;
            }

            // 2. 射线法判断点是否在多边形内
            int i, j = srcDataPolygon.Count - 1;
            bool oddNodes = false;

            for (i = 0; i < srcDataPolygon.Count; i++)
            {
                Dot pi = srcDataPolygon[i];
                Dot pj = srcDataPolygon[j];

                if ((pi.Y < y && pj.Y >= y || pj.Y < y && pi.Y >= y) &&
                    (pi.X <= x || pj.X <= x))
                {
                    if (pi.X + (y - pi.Y) / (pj.Y - pi.Y) * (pj.X - pi.X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }

        // 辅助：写字 (isKey=true表示从texts里取值，false表示直接显示label字符串)
        // 【完全替换此方法】辅助：写字 (自动居中修正版)
        private void WriteLabelInRect(AnnotationCls cls, string label, double x, double y, double w, double h, float fSize, bool isKey)
        {
            string txt = label;
            if (isKey)
            {
                if (texts.ContainsKey(label) && texts[label] != null)
                    txt = texts[label].ToString();
                else
                    return; // 没值就不画
            }

            if (string.IsNullOrEmpty(txt)) return;

            TextAnno ann = new TextAnno();
            ann.Text = txt;
            ann.Height = fSize;
            ann.Width = fSize; // 设置单个全角字符的标准宽高

            // --- 【核心修改：更精准的宽度计算】 ---
            // 遍历每个字符，汉字算1个宽度，数字字母算0.5个宽度
            double estimatedTotalWidth = 0;
            foreach (char c in txt)
            {
                // ASCII字符(数字、字母、半角符号)通常占半个字宽
                if (c < 128)
                    estimatedTotalWidth += 0.5 * fSize;
                else
                    estimatedTotalWidth += 1.0 * fSize;
            }

            // --- 计算居中坐标 ---
            // X轴居中：单元格中心 - 文字总宽的一半
            double cx = x + (w / 2.0) - (estimatedTotalWidth / 2.0);

            // Y轴居中：单元格中心 - 文字高度的一半
            // (注：MapGIS注记锚点通常在左下，所以这里减去一半高度让其视觉垂直居中)
            double cy = y + (h / 2.0) - (fSize / 2.0);

            ann.AnchorDot = new Dot(cx, cy);
            cls.Append(ann, null, new TextAnnInfo { Ovprnt = true });
        }

        /// <summary>
        /// 【修正版】绘制左下角图例 (已修改为：红色实心圆)
        /// </summary>
        private void DrawSurveyLineLegend(SFeatureCls lineCls, AnnotationCls annCls, Rect outRect, double unit)
        {
            // --- 1. 数据准备 ---
            if (_dynamicLegends == null || _dynamicLegends.Count == 0)
            {
                // 默认图例数据
                _dynamicLegends = new List<LegendItemInfo>();
                _dynamicLegends.Add(new LegendItemInfo { Name = "测点", SymID = 1, Color = 6 }); // 6=红
                _dynamicLegends.Add(new LegendItemInfo { Name = "发射源", SymID = 0, Color = 5 }); // 5=蓝
            }

            // --- 2. 计算尺寸和位置 ---
            double itemH = 8.0 * unit; // 每行高度
            double boxW = 40.0 * unit; // 框宽
            double boxH = (12.0 * unit) + (_dynamicLegends.Count * itemH); // 总高

            // 定位：左下角，与右侧表格顶部对齐
            double xLeft = outRect.XMin;
            double gapFromFrame = 15.0 * unit;
            double yTop = outRect.YMin - gapFromFrame;
            double yBottom = yTop - boxH;

            // --- 3. 绘制图例外框 ---
            LinInfo framePen = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };
            DrawRect(lineCls, xLeft, yBottom, xLeft + boxW, yTop, framePen);

            // 绘制标题 "图 例"
            TextAnnInfo txtInfo = new TextAnnInfo { Ovprnt = true };
            float titleSize = (float)(4.0 * unit);
            TextAnno title = new TextAnno();
            title.Text = "图   例";
            title.Height = titleSize;
            title.Width = titleSize;
            title.AnchorDot = new Dot(xLeft + boxW / 2.0 - (title.Text.Length * titleSize) / 2.0, yTop - titleSize - (2.0 * unit));
            annCls.Append(title, null, txtInfo);

            // --- 4. 循环绘制图例项 ---
            double startY = yTop - (10.0 * unit); // 第一行内容的基准Y

            for (int i = 0; i < _dynamicLegends.Count; i++)
            {
                LegendItemInfo item = _dynamicLegends[i];
                double currentY = startY - (i * itemH);
                double symbolX = xLeft + (10.0 * unit); // 符号中心 X 坐标

                // =========================================================
                // ↓↓↓ 核心修改：绘制符号 (红色实心圆) ↓↓↓
                // =========================================================

                if (item.Name.Contains("测点"))
                {
                    // 1. 设置颜色：红色 (MapGIS色号 6)
                    int colorId = 6;

                    // 2. 定义填充画笔 (笔触设为 0.1mm)
                    float penWidth = (float)(0.1 * unit);
                    LinInfo solidPen = new LinInfo
                    {
                        LinStyID = 1,
                        OutClr = new int[] { colorId },
                        OutPenW = new float[] { penWidth }
                    };

                    // 3. 定义半径 (图例里的符号通常比地图上稍微大一点点，这里设为 1.5mm)
                    double targetRadius = 1.5 * unit;

                    // 4. ★实心填充算法★：画密集的同心圆
                    double step = penWidth * 0.5; // 步长为笔宽的一半，确保无缝隙

                    for (double r = step; r < targetRadius; r += step)
                    {
                        DrawCircle(lineCls, symbolX, currentY, r, solidPen);
                    }
                    // 补上最外圈，边缘更光洁
                    DrawCircle(lineCls, symbolX, currentY, targetRadius, solidPen);
                }
                else if (item.Name.Contains("发射源"))
                {
                    // 发射源：蓝色空心矩形 (或者实心，看需求，这里保持蓝色框)
                    LinInfo bluePen = new LinInfo { LinStyID = 1, OutClr = new int[] { 5 }, OutPenW = new float[] { (float)(0.3 * unit) } }; // 5=蓝
                    double halfSize = 1.5 * unit;
                    DrawRect(lineCls, symbolX - halfSize, currentY - halfSize, symbolX + halfSize, currentY + halfSize, bluePen);
                }
                else
                {
                    // 其他默认：空心圆
                    LinInfo defaultPen = new LinInfo { LinStyID = 1, OutClr = new int[] { item.Color }, OutPenW = new float[] { (float)(0.1 * unit) } };
                    DrawCircle(lineCls, symbolX, currentY, 1.5 * unit, defaultPen);
                }

                // --- 绘制文字说明 ---
                float textSize = (float)(3.0 * unit);
                TextAnno text = new TextAnno();
                text.Text = item.Name;
                text.Height = textSize;
                text.Width = textSize * 0.7f; // 调整宽高比
                                              // 文字在符号右侧
                text.AnchorDot = new Dot(symbolX + (5.0 * unit), currentY - textSize / 2.0);
                annCls.Append(text, null, txtInfo);
            }
        }
        // 【新增辅助】画矩形框
        private void DrawRect(SFeatureCls lineCls, double xMin, double yMin, double xMax, double yMax, LinInfo info)
        {
            GeoVarLine box = new GeoVarLine();
            box.Append(new Dot(xMin, yMin));
            box.Append(new Dot(xMax, yMin));
            box.Append(new Dot(xMax, yMax));
            box.Append(new Dot(xMin, yMax));
            box.Append(new Dot(xMin, yMin)); // 闭合

            GeoLines lines = new GeoLines();
            lines.Append(box);
            lineCls.Append(lines, null, info);
        }

        // 【新增辅助】在线图层上画圆 (通过多段线拟合)
        private void DrawCircle(SFeatureCls lineCls, double centerX, double centerY, double radius, LinInfo info)
        {
            GeoVarLine circleLine = new GeoVarLine();
            int segments = 36; // 分36段，足够圆了

            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                circleLine.Append(new Dot(x, y));
            }

            GeoLines lines = new GeoLines();
            lines.Append(circleLine);
            lineCls.Append(lines, null, info);
        }
        // =================================================================
        //                 实际材料图样式 专用绘制函数
        // =================================================================

        // 1. 绘制右下角责任表的【线条】
        private void DrawSurveyTableLines(SFeatureCls lineCls, Rect outRect, double unit)
        {
            // 表格尺寸定义 (单位: mm/unit)
            double tableW = 80.0 * unit; // 表格宽 80mm
            double tableH = 40.0 * unit; // 表格高 40mm
            // 定位到右下角 (内缩一点)
            double x = outRect.XMax;
            double y = outRect.YMin; // 紧贴底框

            LinInfo thinPen = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };

            // 画外框
            GeoVarLine box = new GeoVarLine();
            box.Append(new Dot(x - tableW, y));
            box.Append(new Dot(x, y));
            box.Append(new Dot(x, y + tableH));
            box.Append(new Dot(x - tableW, y + tableH));
            box.Append(new Dot(x - tableW, y));
            GeoLines lines = new GeoLines(); lines.Append(box);
            lineCls.Append(lines, null, thinPen);

            // 画横线 (假设有 5 行)
            double rowH = tableH / 5.0;
            for (int i = 1; i < 5; i++)
            {
                GeoVarLine hLine = new GeoVarLine();
                hLine.Append(new Dot(x - tableW, y + i * rowH));
                hLine.Append(new Dot(x, y + i * rowH));
                GeoLines l = new GeoLines(); l.Append(hLine);
                lineCls.Append(l, null, thinPen);
            }

            // 画竖线 (分割"编图"、"审核"等标题栏和内容栏)
            // 第一列宽 20mm
            GeoVarLine vLine1 = new GeoVarLine();
            vLine1.Append(new Dot(x - tableW + 20 * unit, y));
            vLine1.Append(new Dot(x - tableW + 20 * unit, y + tableH - rowH)); // 最上面一行通常是总标题，不画竖线
            GeoLines l2 = new GeoLines(); l2.Append(vLine1);
            lineCls.Append(l2, null, thinPen);

            // 还可以画更多竖线分割右边的日期、比例尺等
        }
        /// <summary>
        /// 【核心算法】根据范围自动计算最合适的整洁步长
        /// </summary>
        /// <param name="min">最小坐标</param>
        /// <param name="max">最大坐标</param>
        /// <param name="targetDivisions">期望大概分几格(默认4-6格)</param>
        /// <returns>计算出的整洁步长(如100, 200, 500)</returns>
        private double CalculateNiceStep(double min, double max, int targetDivisions = 5)
        {
            double range = Math.Abs(max - min);
            if (range <= 0.000001) return 100.0; // 防止范围无效

            // 1. 算出原始的粗糙步长
            double rawStep = range / targetDivisions;

            // 2. 计算数量级 (比如 rawStep是 350，数量级就是 100)
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double residual = rawStep / magnitude;

            // 3. 归一化到 1, 2, 5 (制图学通用标准)
            double niceStep;
            if (residual < 1.5) niceStep = 1.0;       // 10, 100...
            else if (residual < 3.0) niceStep = 2.0;  // 20, 200... (也有人用2.5)
            else if (residual < 7.0) niceStep = 5.0;  // 50, 500...
            else niceStep = 10.0;                     // 100, 1000...

            return niceStep * magnitude;
        }

        // 2. 绘制右下角责任表的【文字】
        private void DrawSurveyTableText(AnnotationCls annCls, Rect outRect, double unit)
        {
            double tableW = 80.0 * unit;
            double tableH = 40.0 * unit;
            double xBase = outRect.XMax - tableW;
            double yBase = outRect.YMin;
            double rowH = tableH / 5.0;

            TextAnnInfo txtInfo = new TextAnnInfo { Ovprnt = true };
            float fontSize = (float)(3.0 * unit);

            // 辅助函数：写字
            void WriteCell(string txt, double offsetX, double offsetY)
            {
                TextAnno ann = new TextAnno();
                ann.Text = txt;
                ann.Height = fontSize; ann.Width = fontSize;
                ann.AnchorDot = new Dot(xBase + offsetX * unit, yBase + offsetY * unit + (rowH - fontSize) / 2);
                annCls.Append(ann, null, txtInfo);
            }

            // 填写内容 (参考你的图片结构)
            // 顶部标题
            WriteCell("制图单位: XX地质大队", 25, 4 * rowH / unit);
            // 实际材料图标题
            WriteCell("XX 工作实际材料图", 20, 3 * rowH / unit);

            // 左侧栏目
            WriteCell("编  图", 5, 2 * rowH / unit);
            WriteCell("审  核", 5, 1 * rowH / unit);
            WriteCell("总  工", 5, 0 * rowH / unit);

            // 内容栏 (预留)
            WriteCell("×××", 30, 2 * rowH / unit);
            WriteCell("×××", 30, 1 * rowH / unit);
            WriteCell("1:10000", 60, 1 * rowH / unit); // 比例尺
        }

        // 3. 绘制角点经纬度 (如 115°51′43″)
        private void DrawSurveyCornerLabels(AnnotationCls annCls, Rect rect, double unit)
        {
            float fSize = (float)(3.5 * unit);
            TextAnnInfo info = new TextAnnInfo { Ovprnt = true };

            // 模拟 DMS 格式字符串 (实际需要根据 srcSRS 做投影反算，这里仅作格式演示)
            // 如果你的 map 是投影坐标，你需要用 ProjToLonLat 函数反算
            string[] corners = new string[] {
    "115°51′43″", // 左上经度
                "28°15′00″",  // 左上纬度
                "115°54′43″", // 右下经度
                "28°12′00″"   // 右下纬度
            };

            // 左上角
            TextAnno tlLat = new TextAnno { Text = corners[1], Height = fSize, Width = fSize * 0.8f, AnchorDot = new Dot(rect.XMin - 20 * unit, rect.YMax) };
            TextAnno tlLon = new TextAnno { Text = corners[0], Height = fSize, Width = fSize * 0.8f, AnchorDot = new Dot(rect.XMin, rect.YMax + 2 * unit) };

            // 右上角
            TextAnno trLon = new TextAnno { Text = corners[2], Height = fSize, Width = fSize * 0.8f, AnchorDot = new Dot(rect.XMax - 20 * unit, rect.YMax + 2 * unit) };

            // 简单添加进去
            annCls.Append(tlLat, null, info);
            annCls.Append(tlLon, null, info);
            annCls.Append(trLon, null, info);
        }

        // 4. 绘制左下角图例
        private void DrawSurveyLegendText(AnnotationCls annCls, Rect outRect, double unit)
        {
            double x = outRect.XMin;
            double y = outRect.YMin + (30.0 * unit); // 稍微往上一点

            TextAnnInfo info = new TextAnnInfo { Ovprnt = true };
            float fSize = (float)(3.5 * unit);

            TextAnno title = new TextAnno { Text = "图例", Height = fSize * 1.2f, Width = fSize * 1.2f, AnchorDot = new Dot(x + 20 * unit, y + 10 * unit) };
            annCls.Append(title, null, info);

            string[] legends = new string[] { "磁测基点位置", "磁测点位及点线号", "磁测质量检查点" };

            for (int i = 0; i < legends.Length; i++)
            {
                // 这里只画文字，对应的符号（圆圈、方块）可以用 SFeatureCls 在对应位置画点或线
                TextAnno leg = new TextAnno();
                leg.Text = legends[i];
                leg.Height = fSize; leg.Width = fSize;
                leg.AnchorDot = new Dot(x + 15 * unit, y - (i * 8 * unit));
                annCls.Append(leg, null, info);
            }
        }

        // 5. 绘制图形比例尺 (线段尺)
        private void DrawGraphicScale(SFeatureCls lineCls, AnnotationCls annCls, Rect outRect, double unit)
        {
            // 位置：底部正中
            double centerX = (outRect.XMin + outRect.XMax) / 2.0;
            double y = outRect.YMin + (45.0 * unit); // 位于表格上方

            // 画线：0 -- 200 -- 400
            GeoVarLine scaleLine = new GeoVarLine();
            double segLen = 20.0 * unit; // 假设一段代表200米(根据比例尺实际算)
            scaleLine.Append(new Dot(centerX - 2 * segLen, y));
            scaleLine.Append(new Dot(centerX + 2 * segLen, y));

            LinInfo info = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };
            GeoLines gl = new GeoLines(); gl.Append(scaleLine);
            lineCls.Append(gl, null, info);

            // 画竖短线和文字
            TextAnnInfo tInfo = new TextAnnInfo { Ovprnt = true };
            float fSize = (float)(2.5 * unit);

            for (int i = -2; i <= 2; i++)
            {
                double ticketX = centerX + i * segLen;
                // 短线
                GeoVarLine tick = new GeoVarLine();
                tick.Append(new Dot(ticketX, y));
                tick.Append(new Dot(ticketX, y + 2 * unit));
                GeoLines glTick = new GeoLines(); glTick.Append(tick);
                lineCls.Append(glTick, null, info);

                // 文字 (0, 200, 400...)
                TextAnno num = new TextAnno();
                num.Text = (Math.Abs(i) * 200).ToString(); // 示例数值
                num.Height = fSize; num.Width = fSize * 0.8f;
                num.AnchorDot = new Dot(ticketX - 2 * unit, y + 3 * unit);
                annCls.Append(num, null, tInfo);
            }

            // 单位
            TextAnno unitTxt = new TextAnno { Text = "m", Height = fSize, Width = fSize, AnchorDot = new Dot(centerX + 2.5 * segLen, y + 3 * unit) };
            annCls.Append(unitTxt, null, tInfo);
        }

        // ================== 【必须补充】 通用反射辅助函数 (请务必复制到类中) ==================

        // 智能添加图层（自动查找 Append 或 LayerEnum.Append）
        private void AddLayerToMap(Map map, MapLayer layer)
        {
            try
            {
                // 1. 尝试直接调用 map.Append(layer)
                TryInvokeMethod(map, "Append", new object[] { layer });
            }
            catch
            {
                try
                {
                    // 2. 尝试调用 map.GetLayerEnum().Append(layer)
                    object enumObj = null;
                    var method = map.GetType().GetMethod("GetLayerEnum");
                    if (method != null) enumObj = method.Invoke(map, null);
                    else
                    {
                        // 3. 尝试属性 map.LayerEnum
                        var prop = map.GetType().GetProperty("LayerEnum");
                        if (prop != null) enumObj = prop.GetValue(map, null);
                    }

                    if (enumObj != null)
                    {
                        TryInvokeMethod(enumObj, "Append", new object[] { layer });
                    }
                }
                catch { } // 如果都失败，可能 map 是只读或版本差异过大
            }
        }

        // 智能设置枚举属性（尝试多个名字，如 Lin/Line, Ann/Annotation）
        private void TrySetEnumProperty(object obj, string propName, params string[] enumNames)
        {
            var prop = obj.GetType().GetProperty(propName);
            if (prop == null) return;

            Type enumType = prop.PropertyType;
            if (!enumType.IsEnum) return; // 不是枚举就不处理

            foreach (string name in enumNames)
            {
                try
                {
                    if (Enum.IsDefined(enumType, name))
                    {
                        object val = Enum.Parse(enumType, name);
                        ForceSet(obj, propName, val);
                        return; // 成功设置即退出
                    }
                }
                catch { }
            }
        }

        // (这 3 个基础反射函数保持不变，必须保留)
        private void ForceSet(object obj, string propName, object value)
        {
            var type = obj.GetType();
            var prop = type.GetProperty(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(obj, value, null); return; }
            var field = type.GetField($"<{propName}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) { field.SetValue(obj, value); return; }
            field = type.GetField(propName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(obj, value);
        }

        private bool TrySetProperty(object obj, string propName, object value)
        {
            var prop = obj.GetType().GetProperty(propName);
            if (prop != null && prop.CanWrite) { prop.SetValue(obj, value, null); return true; }
            return false;
        }

        // 【修正后】将返回类型改为 object，这样才能获取 GetAllIDs 等方法的返回值
        private object TryInvokeMethod(object obj, string methodName, object[] args)
        {
            if (obj == null) return null;

            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                // 返回调用的结果
                return method.Invoke(obj, args);
            }

            return null; // 如果找不到方法，返回 null
        }
        private void InitDefaultStyle()
        {
            // 1. 基础初始化
            if (texts == null) texts = new Hashtable();

            // 2. 清空列表
            listView_Style.Items.Clear();
            largeImgList.Images.Clear();

            // 3. 设置绘图尺寸
            int w = 250;
            int h = 250;
            if (largeImgList.ImageSize.Width != w) largeImgList.ImageSize = new Size(w, h);

            // ====================================================================
            // 样式 200: 测线成图样式 (原 image_689f99.png 风格)
            // ====================================================================
            Bitmap bmpLineMap = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmpLineMap))
            {
                g.Clear(System.Drawing.Color.White);
                // 开启抗锯齿
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // --- 布局参数 ---
                int marginTop = 30;
                int marginBottom = 70;
                int marginLeft = 20;
                int marginRight = 20;

                int mapW = w - marginLeft - marginRight;
                int mapH = h - marginTop - marginBottom;

                Rectangle innerRect = new Rectangle(marginLeft, marginTop, mapW, mapH);
                Rectangle outerRect = new Rectangle(marginLeft - 5, marginTop - 5, mapW + 10, mapH + 10);

                // --- 绘制网格 ---
                Pen gridPen = new Pen(System.Drawing.Color.LightGray, 1);
                gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                gridPen.DashPattern = new float[] { 5, 5 };

                for (int i = 1; i < 5; i++)
                {
                    int x = innerRect.X + (innerRect.Width / 5) * i;
                    g.DrawLine(gridPen, x, innerRect.Top, x, innerRect.Bottom);
                    g.DrawString((38400 + i).ToString(), new System.Drawing.Font("Arial", 6), Brushes.Gray, x - 10, innerRect.Bottom + 2);
                }
                for (int i = 1; i < 5; i++)
                {
                    int y = innerRect.Y + (innerRect.Height / 5) * i;
                    g.DrawLine(gridPen, innerRect.Left, y, innerRect.Right, y);
                    g.DrawString((2800 + i).ToString(), new System.Drawing.Font("Arial", 6), Brushes.Gray, innerRect.Left - 18, y - 5);
                }

                // --- 绘制图框 ---
                g.DrawRectangle(Pens.Black, innerRect); // 内框
                Pen thickPen = new Pen(System.Drawing.Color.Black, 2);
                g.DrawRectangle(thickPen, outerRect);   // 外框

                // --- 绘制标题 ---
                StringFormat sfCenter = new StringFormat() { Alignment = StringAlignment.Center };
                g.DrawString("工区磁测平面图", new System.Drawing.Font("黑体", 10, FontStyle.Bold), Brushes.Black, w / 2, 8, sfCenter);

                // --- 绘制右下角责任表 ---
                int tableW = 90;
                int tableH = 45;
                int tableX = w - tableW - 10;
                int tableY = h - tableH - 5;
                Pen tablePen = Pens.Black;
                System.Drawing.Font tableFont = new System.Drawing.Font("宋体", 6);

                g.DrawRectangle(tablePen, tableX, tableY, tableW, tableH);
                int rowH = tableH / 5;
                for (int i = 1; i < 5; i++) g.DrawLine(tablePen, tableX, tableY + i * rowH, tableX + tableW, tableY + i * rowH);
                g.DrawLine(tablePen, tableX + 25, tableY, tableX + 25, tableY + tableH);
                g.DrawLine(tablePen, tableX + 55, tableY, tableX + 55, tableY + tableH);

                g.DrawString("制图单位", tableFont, Brushes.Black, tableX + 2, tableY + 2);
                g.DrawString("XX地质队", tableFont, Brushes.Black, tableX + 30, tableY + 2);
                g.DrawString("1:10000", tableFont, Brushes.Black, tableX + 60, tableY + 18);

                // --- 绘制左下角图例 ---
                int legW = 60;
                int legH = 45;
                int legX = 10;
                int legY = h - legH - 5;

                g.DrawRectangle(tablePen, legX, legY, legW, legH);
                g.DrawString("图  例", tableFont, Brushes.Black, legX + 20, legY + 2);

                g.FillEllipse(Brushes.Red, legX + 5, legY + 15, 6, 6);
                g.DrawString("测点", tableFont, Brushes.Black, legX + 15, legY + 12);
                g.FillRectangle(Brushes.Blue, legX + 5, legY + 25, 6, 6);
                g.DrawString("发射源", tableFont, Brushes.Black, legX + 15, legY + 22);

                // --- 绘制中间比例尺 ---
                int scaleX = w / 2;
                int scaleY = h - 60;
                g.DrawLine(tablePen, scaleX - 20, scaleY, scaleX + 20, scaleY);
                g.DrawLine(tablePen, scaleX - 20, scaleY, scaleX - 20, scaleY - 3);
                g.DrawLine(tablePen, scaleX, scaleY, scaleX, scaleY - 3);
                g.DrawLine(tablePen, scaleX + 20, scaleY, scaleX + 20, scaleY - 3);
                g.DrawString("0     200     400 m", tableFont, Brushes.Black, scaleX - 35, scaleY - 10);
            }

            largeImgList.Images.Add(bmpLineMap);
            ListViewItem itemLineMap = new ListViewItem("测线成图样式", largeImgList.Images.Count - 1);
            itemLineMap.Tag = 200;
            listView_Style.Items.Add(itemLineMap);

            // ====================================================================
            // 样式 300: 重磁数据成图样式 (黑白渐变预览)
            // ====================================================================
            Bitmap bmpGravMag = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmpGravMag))
            {
                g.Clear(System.Drawing.Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int marginTop = 30;
                int marginBottom = 70;
                int marginLeft = 20;
                int marginRight = 20;
                int mapW = w - marginLeft - marginRight;
                int mapH = h - marginTop - marginBottom;
                Rectangle innerRect = new Rectangle(marginLeft, marginTop, mapW, mapH);
                Rectangle outerRect = new Rectangle(marginLeft - 5, marginTop - 5, mapW + 10, mapH + 10);

                Pen gridPen = new Pen(System.Drawing.Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom, DashPattern = new float[] { 5, 5 } };
                for (int i = 1; i < 5; i++) g.DrawLine(gridPen, innerRect.X + (innerRect.Width / 5) * i, innerRect.Top, innerRect.X + (innerRect.Width / 5) * i, innerRect.Bottom);
                for (int i = 1; i < 5; i++) g.DrawLine(gridPen, innerRect.Left, innerRect.Y + (innerRect.Height / 5) * i, innerRect.Right, innerRect.Y + (innerRect.Height / 5) * i);
                g.DrawRectangle(Pens.Black, innerRect);
                Pen thickPen = new Pen(System.Drawing.Color.Black, 2);
                g.DrawRectangle(thickPen, outerRect);

                StringFormat sfCenter = new StringFormat() { Alignment = StringAlignment.Center };
                g.DrawString("重磁异常平面图", new System.Drawing.Font("黑体", 10, FontStyle.Bold), Brushes.Black, w / 2, 8, sfCenter);

                int tableW = 90, tableH = 45;
                int tableX = w - tableW - 10;
                int tableY = h - tableH - 5;
                g.DrawRectangle(Pens.Black, tableX, tableY, tableW, tableH);
                g.DrawLine(Pens.Black, tableX, tableY + 10, tableX + tableW, tableY + 10);
                g.DrawString("责任表", new System.Drawing.Font("宋体", 6), Brushes.Black, tableX + 2, tableY + 2);

                // --- 绘制渐变图例 ---
                int legW = 60, legH = 45;
                int legX = 10;
                int legY = h - legH - 5;
                g.DrawRectangle(Pens.Black, legX, legY, legW, legH);
                g.DrawString("图  例", new System.Drawing.Font("宋体", 6), Brushes.Black, legX + 20, legY + 2);

                Rectangle gradientRect = new Rectangle(legX + 15, legY + 15, 10, 25);
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                  gradientRect, System.Drawing.Color.Black, System.Drawing.Color.White, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, gradientRect);
                }
                g.DrawRectangle(Pens.Black, gradientRect);
                System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 5);
                g.DrawString("高", smallFont, Brushes.Black, legX + 28, legY + 15);
                g.DrawString("低", smallFont, Brushes.Black, legX + 28, legY + 35);

                // --- 绘制比例尺 ---
                int scaleX = w / 2;
                int scaleY = innerRect.Bottom + 15;
                g.DrawLine(Pens.Black, scaleX - 20, scaleY, scaleX + 20, scaleY);
                g.DrawString("0      200 m", smallFont, Brushes.Black, scaleX - 22, scaleY - 10);
            }

            largeImgList.Images.Add(bmpGravMag);
            ListViewItem itemGravMag = new ListViewItem("重磁数据成图", largeImgList.Images.Count - 1);
            itemGravMag.Tag = 300;
            listView_Style.Items.Add(itemGravMag);

            // 4. 默认选中第一个 (即样式 200)
            if (listView_Style.Items.Count > 0)
            {
                listView_Style.Items[0].Selected = true;
            }

            // 5. 强制刷新左侧文本列表
            RefreshTextList();
        }

        private void AddCoverData(SFeatureCls regSfcls, Rect outrect)
        {
            if (isWholeRange || regSfcls == null || map == null || outrect == null || srcDataPolygon.Count == 0)
            {
                return;
            }
            Rect range = map.Range;
            if (range == null)
            {
                return;
            }
            GeoPolygon geoPolygon = new GeoPolygon();
            GeoLines geoLines = new GeoLines();
            GeoLines geoLines2 = new GeoLines();
            GeoVarLine geoVarLine = new GeoVarLine();
            GeoVarLine geoVarLine2 = new GeoVarLine();
            geoVarLine.Append(new Dot(outrect.XMin, outrect.YMin));
            geoVarLine.Append(new Dot(outrect.XMax, outrect.YMin));
            geoVarLine.Append(new Dot(outrect.XMax, outrect.YMax));
            geoVarLine.Append(new Dot(outrect.XMin, outrect.YMax));
            geoVarLine.Append(new Dot(outrect.XMin, outrect.YMin));
            geoLines2.Append(geoVarLine);
            geoPolygon.Append(geoLines2);
            int num = 0;
            if (!isProjectSrcSRS)
            {
                num = 20;
            }
            bool flag = IsSamePrjSrsButScale(srcSRS.Clone(), dstMapSRS.Clone());
            for (int i = 0; i < srcDataPolygon.Count - 1; i++)
            {
                Dot dot = srcDataPolygon[i];
                Dot dot2 = srcDataPolygon[i + 1];
                double num2 = (dot2.X - dot.X) / (double)(num + 1);
                double num3 = (dot2.Y - dot.Y) / (double)(num + 1);
                for (int j = 0; j <= num; j++)
                {
                    Dot dot3 = new Dot(dot.X + (double)j * num2, dot.Y + (double)j * num3);
                    dot3.X = ((dot3.X > range.XMax) ? range.XMax : dot3.X);
                    dot3.X = ((dot3.X < range.XMin) ? range.XMin : dot3.X);
                    dot3.Y = ((dot3.Y > range.YMax) ? range.YMax : dot3.Y);
                    dot3.Y = ((dot3.Y < range.YMin) ? range.YMin : dot3.Y);
                    if (flag && !isImageLayer)
                    {
                        TransIfSamePrjSrsButScale(dot3, srcSRS, dstMapSRS);
                    }
                    geoVarLine2.Append(dot3);
                }
            }
            geoVarLine2.Append(geoVarLine2.Get2D(0).Clone());
            if (dstMapSRS != null && !flag)
            {
                geoVarLine2 = geoVarLine2.TransSRS(srcSRS, dstMapSRS) as GeoVarLine;
            }
            geoLines.Append(geoVarLine2);
            geoPolygon.Append(geoLines);
            RegInfo regInfo = new RegInfo();
            regInfo.FillClr = 9;
            regSfcls.Append(geoPolygon, null, regInfo);
        }

        private bool IsSamePrjSrsButScale(SRefData srs1, SRefData srs2)
        {
            if (srs1 == null || srs2 == null || srs1.Type != SRefType.PRJ || srs2.Type != SRefType.PRJ)
            {
                return false;
            }
            return true;
        }

        private void TransIfSamePrjSrsButScale(Dot dot, SRefData srcSrs, SRefData dstSrs)
        {
            if (dot != null && srcSrs != null && dstSrs != null)
            {
                double num = srcSrs.Rate * srcSrs.UnitFactor;
                double num2 = dstSrs.Rate * dstSrs.UnitFactor;
                bool flag = true;
                double num3 = 1.0;
                if (num2 > num)
                {
                    flag = true;
                    num3 = num2 / num;
                }
                else
                {
                    flag = false;
                    num3 = num / num2;
                }
                if (!flag)
                {
                    dot.X *= num3;
                    dot.Y *= num3;
                }
                else
                {
                    dot.X /= num3;
                    dot.Y /= num3;
                }
            }
        }

        private SRefData GetProjSrs()
        {
            SRefData sRefData = currentSRS.Clone();
            sRefData.Rate = scale;
            sRefData.Unit = SRefLenUnit.MilliMeter;
            sRefData.UnitFactor = 0.001;
            string text = "";
            text = ((scale == 25000) ? "1:2.5万" : ((scale < 10000) ? ("1:" + scale) : ("1:" + scale / 10000 + "万")));
            text = ((sRefData.Spheroid != SRefEPType.Beijing54) ? (text + "_西安80_") : (text + "_北京54_"));
            if (sRefData.ProjType == SRefPrjType.GaussKruger)
            {
                text = text + sRefData.Zone + "带";
                text += ((sRefData.ZoneType == SRefZoneType.Degree3) ? 3 : 6);
                text += "_北";
            }
            else
            {
                text += sRefData.SRSName;
            }
            sRefData.SRSName = text;
            return sRefData;
        }

        private void SetFrameXclsStru(IVectorCls xcls)
        {
            if (xcls == null)
            {
                return;
            }
            Fields fields = new Fields();
            if (xcls.ClsType == XClsType.SFCls)
            {
                SFeatureCls sFeatureCls = xcls as SFeatureCls;
                if (sFeatureCls.GeomType == GeomType.Lin)
                {
                    Field field = new Field();
                    field.FieldName = "mpLength";
                    field.FieldType = MapGIS.GeoObjects.Att.FieldType.FldDouble;
                    field.MskLength = 15;
                    field.PointLength = 6;
                    field.Editable = 1;
                    fields.AppendField(field);
                }
                else if (sFeatureCls.GeomType == GeomType.Reg)
                {
                    Field field2 = new Field();
                    field2.FieldName = "mpArea";
                    field2.FieldType = MapGIS.GeoObjects.Att.FieldType.FldDouble;
                    field2.MskLength = 15;
                    field2.PointLength = 6;
                    field2.Editable = 1;
                    fields.AppendField(field2);
                    Field field3 = new Field();
                    field3.FieldName = "mpPerimeter";
                    field3.FieldType = MapGIS.GeoObjects.Att.FieldType.FldDouble;
                    field3.MskLength = 15;
                    field3.PointLength = 6;
                    field3.Editable = 1;
                    fields.AppendField(field3);
                }
            }
            Field field4 = new Field();
            field4.FieldName = "mpLayer";
            field4.FieldType = MapGIS.GeoObjects.Att.FieldType.FldLong;
            field4.MskLength = 10;
            field4.Editable = 1;
            fields.AppendField(field4);
            Field field5 = new Field();
            field5.FieldName = "type";
            field5.FieldType = MapGIS.GeoObjects.Att.FieldType.FldString;
            field5.MskLength = 25;
            field5.Editable = 1;
            fields.AppendField(field5);
            Field field6 = new Field();
            field6.FieldName = "name";
            field6.FieldType = MapGIS.GeoObjects.Att.FieldType.FldString;
            field6.MskLength = 25;
            field6.Editable = 1;
            fields.AppendField(field6);
            Field field7 = new Field();
            field7.FieldName = "alias";
            field7.FieldType = MapGIS.GeoObjects.Att.FieldType.FldString;
            field7.MskLength = 25;
            field7.Editable = 1;
            fields.AppendField(field7);
            Field field8 = new Field();
            field8.FieldName = "describe";
            field8.FieldType = MapGIS.GeoObjects.Att.FieldType.FldString;
            field8.MskLength = 50;
            field8.Editable = 1;
            fields.AppendField(field8);
            Field field9 = new Field();
            field9.FieldName = "tag";
            field9.FieldType = MapGIS.GeoObjects.Att.FieldType.FldString;
            field9.MskLength = 50;
            field9.Editable = 1;
            fields.AppendField(field9);
            Field field10 = new Field();
            field10.FieldName = "reserve";
            field10.FieldType = MapGIS.GeoObjects.Att.FieldType.FldString;
            field10.MskLength = 50;
            field10.Editable = 1;
            fields.AppendField(field10);
            xcls.Fields = fields;
        }

        private void simpleButton_Cancel_Click(object sender, EventArgs e)
        {
            ((Form)this).DialogResult = DialogResult.Cancel;
            ((Form)this).Close();
        }

        private void buttonEdit_Template_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择样式";
            openFileDialog.Filter = "样式文件" + "(.decsty)|*.decsty";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                if (fileName != null && File.Exists(fileName))
                {
                    ((Control)(object)buttonEdit_Template).Text = fileName;
                }
            }
        }

        private void buttonEdit_Template_EditValueChanged(object sender, EventArgs e)
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(((Control)(object)buttonEdit_Template).Text);
            }
            catch
            {
            }
            commonMapDecoration.FromXML(xmlDocument.InnerXml);
            isInitUI = true;
            RefreshTextList();
            isInitUI = false;
        }

        private void treeList_Ann_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
        {
            // 只有当修改的是“内容”这一列时才处理
            if (e.Column == contentColumn)
            {
                // 获取当前行的 Key（比如 "主标题"、"左下角附注"）
                // 注意：我们在 RefreshTextList 里把 key 存到了 Node 的 Tag 里，或者直接取第一列的值
                string key = e.Node.GetValue(typeColumn).ToString();

                if (!string.IsNullOrEmpty(key))
                {
                    // 更新哈希表，保存你的修改
                    texts[key] = e.Value;
                }
            }
        }

        private void comboBoxEdit_Scale_EditValueChanged(object sender, EventArgs e)
        {
            if (isMapOK)
            {
                scale = Convert.ToInt32(((Control)(object)comboBoxEdit_Scale).Text);
                if (scale <= 0)
                {
                    scale = 1;
                }
                if (!isProjectSrcSRS)
                {
                    RefreshCurrentSRS();
                }
                SaveUI();
                if (checkData > 0)
                {
                    RefreshUIData();
                }
            }
        }

        private void listView_Style_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView_Style_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isMapOK && listView_Style.FocusedItem != null)
            {
                int num = (int)listView_Style.FocusedItem.Tag;
                commonMapDecoration.SetProperty("SetAutoMapDecoration", num);

                // ========================================================
                // 【核心修改】切换样式时，强制清除已缓存的“主标题”
                // 这样 RefreshTextList 才会去调用 GetDefaultValueForKey 获取新标题
                // ========================================================
                if (texts.ContainsKey("主标题"))
                {
                    texts.Remove("主标题");
                }

                // 如果需要连带“制图单位”也刷新，可以把下面这行解开注释
                // if (texts.ContainsKey("制图单位")) texts.Remove("制图单位");

                RefreshTextList();
            }
        }

        private void comboBoxEdit_Scale_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (e.NewValue != null)
            {
                int num = 0;
                try
                {
                    num = Convert.ToInt32(e.NewValue.ToString());
                }
                catch
                {
                    ((CancelEventArgs)(object)e).Cancel = true;
                }
                if (num <= 0)
                {
                    ((CancelEventArgs)(object)e).Cancel = true;
                }
            }
            else
            {
                ((CancelEventArgs)(object)e).Cancel = true;
            }
        }

        private void RefreshTextList()
        {
            treeList_Ann.BeginUpdate();
            treeList_Ann.ClearNodes();
            isInitUI = true;

            // 获取当前选中的样式 ID
            int selectedStyleTag = 0;
            if (listView_Style.SelectedItems.Count > 0 && listView_Style.SelectedItems[0].Tag != null)
            {
                selectedStyleTag = (int)listView_Style.SelectedItems[0].Tag;
            }
            if (selectedStyleTag == 0)
            {
                selectedStyleTag = 200;
            }

            // 定义当前样式需要的字段列表
            string[] requiredKeys;

            if (selectedStyleTag == 200)
            {
                requiredKeys = new string[] {
          "主标题", "制图单位", "编    图", "审    核", "数字制图",
          "技术负责", "单位负责人", "顺序号", "图    号", "比例尺",
          "编图日期", "资料来源"
        };
            }
            else if (selectedStyleTag == 300) // 重磁数据成图
            {
                requiredKeys = new string[] {
            "主标题", "制图单位", "编    图", "审    核", "数字制图",
            "技术负责", "单位负责人", "顺序号", "图    号", "比例尺",
            "编图日期", "资料来源",
            // ★★★ 新增这一行：等值线间距 ★★★
            "等值线间距"
            };
            }
            else // 其他默认样式
            {
                requiredKeys = new string[] { "主标题", "左下角附注", "右下角附注" };
            }

            // 循环添加节点
            foreach (string key in requiredKeys)
            {
                string val = "";

                // 如果哈希表里已经存了（用户改过），就用改过的
                if (texts.ContainsKey(key))
                {
                    val = texts[key].ToString();
                }
                else
                {
                    // 否则使用默认值
                    val = GetDefaultValueForKey(key);
                    texts[key] = val;
                }

                treeList_Ann.AppendNode(new object[] { key, val }, -1, key);
            }

            isInitUI = false;
            treeList_Ann.EndUpdate();
        }
        /// <summary>
        /// 【修正版】绘制重磁数据成图样式的图例 (标准彩虹色标 + 真实极值)
        /// </summary>
        private void DrawGradientLegend(SFeatureCls lineCls, AnnotationCls annCls, Rect outRect, double unit)
        {
            // --- 1. 获取数据范围 ---
            double zMin, zMax;
            GetGridMinMax(out zMin, out zMax);

            // --- 2. 尺寸定义 ---
            double boxW = 40.0 * unit; // 图例框总宽
            double boxH = 45.0 * unit; // 图例框总高 (稍微加高一点以容纳文字)

            // --- 3. 定位 (绝对底部对齐) ---
            double xLeft = outRect.XMin;
            double gapFromFrame = 15.0 * unit;
            double yTop = outRect.YMin - gapFromFrame;
            double yBase = yTop - boxH;

            // --- 4. 绘制外边框 (图例的大框) ---
            LinInfo framePen = new LinInfo { LinStyID = 1, OutClr = new int[] { 1 }, OutPenW = new float[] { (float)(0.05 * unit) } };
            DrawRect(lineCls, xLeft, yBase, xLeft + boxW, yTop, framePen);

            // --- 5. 绘制标题 ---
            TextAnnInfo txtInfo = new TextAnnInfo { Ovprnt = true };
            float titleSize = (float)(4.0 * unit);
            TextAnno title = new TextAnno();
            title.Text = "图   例";
            title.Height = titleSize;
            title.Width = titleSize;
            // 居中显示标题
            title.AnchorDot = new Dot(xLeft + boxW / 2.0 - (title.Text.Length * titleSize) / 2.0, yTop - titleSize - (2.0 * unit));
            annCls.Append(title, null, txtInfo);

            // --- 6. 【核心】绘制彩色色标 (Rainbow Bar) ---
            // MapGIS 标准重磁色谱通常是: 蓝(低) -> 青 -> 绿 -> 黄 -> 红(高)
            // 对应的 MapGIS 颜色ID通常是: 5(蓝), 4(青), 3(绿), 2(黄), 6(红) 
            // *注：具体ID可能随库不同，这是最通用的标准库顺序*
            int[] colorIds = new int[] { 5, 4, 3, 2, 6 };

            double barX = xLeft + (10.0 * unit); // 色条左边缘
            double barW = 8.0 * unit;            // 色条宽度
            double barTotalH = 25.0 * unit;      // 色条总高度
            double barYTop = yTop - (10.0 * unit);
            double barYBottom = barYTop - barTotalH;

            // 计算每个色块的高度
            double segmentH = barTotalH / colorIds.Length;

            // 创建填充图层对象 (我们需要一个 RegLayer 来画填充，或者用线模拟填充)
            // *由于我们只有 lineCls (线层)，我们用密集的矩形线框或者粗线来模拟色块*

            for (int i = 0; i < colorIds.Length; i++)
            {
                int colorId = colorIds[i];

                // 从下往上画 (索引0是低值/蓝色，在最下面)
                double currentY = barYBottom + (i * segmentH);

                // 使用“粗线填充法”绘制色块
                // 也可以尝试获取 GraphicsData.RegLayer，但为了稳健性，我们在 LineLayer 上用粗笔触画实心矩形

                LinInfo fillPen = new LinInfo
                {
                    LinStyID = 1,
                    OutClr = new int[] { colorId }, // 设置颜色
                    OutPenW = new float[] { (float)(0.3 * unit) } // 粗笔触
                };

                // 内部填充循环
                double step = 0.15 * unit; // 步长
                for (double h = 0; h < segmentH; h += step)
                {
                    GeoVarLine hLine = new GeoVarLine();
                    hLine.Append(new Dot(barX, currentY + h));
                    hLine.Append(new Dot(barX + barW, currentY + h));
                    GeoLines glFill = new GeoLines();
                    glFill.Append(hLine);
                    lineCls.Append(glFill, null, fillPen);
                }

                // 补一个边框修正边缘
                DrawRect(lineCls, barX, currentY, barX + barW, currentY + segmentH, fillPen);
            }

            // 绘制色条的黑色轮廓线
            DrawRect(lineCls, barX, barYBottom, barX + barW, barYTop, framePen);

            // --- 7. 绘制刻度数值 ---
            float numSize = (float)(2.5 * unit);
            int stepCount = colorIds.Length;
            double valStep = (zMax - zMin) / stepCount;

            // 绘制刻度线和数字
            for (int i = 0; i <= stepCount; i++)
            {
                double yPos = barYBottom + (i * segmentH);

                // 刻度线
                GeoVarLine tick = new GeoVarLine();
                tick.Append(new Dot(barX + barW, yPos));
                tick.Append(new Dot(barX + barW + (1.5 * unit), yPos));
                GeoLines glTick = new GeoLines(); glTick.Append(tick);
                lineCls.Append(glTick, null, framePen);

                // 数值 (保留1位或2位小数)
                double val = zMin + (i * valStep);
                string valStr = val.ToString("0.##"); // 自动格式化
                if (Math.Abs(val) > 1000) valStr = val.ToString("0"); // 大数取整

                TextAnno numAnn = new TextAnno();
                numAnn.Text = valStr;
                numAnn.Height = numSize;
                numAnn.Width = numSize * 0.7f;
                // 文字位置：刻度线右侧
                numAnn.AnchorDot = new Dot(barX + barW + (2.0 * unit), yPos - numSize / 2.0);
                annCls.Append(numAnn, null, txtInfo);
            }

            // 顶部单位 (可选，例如 nT 或 mGal)
            TextAnno unitAnn = new TextAnno();
            unitAnn.Text = "(单位)"; // 如果知道具体单位，可以在这里改
            unitAnn.Height = numSize;
            unitAnn.Width = numSize * 0.7f;
            unitAnn.AnchorDot = new Dot(barX, barYTop + (1.0 * unit));
            annCls.Append(unitAnn, null, txtInfo);
        }

        // 【核心修改】根据选中的样式ID返回对应的默认标题
        // 【修改】根据当前选中的样式，返回不同的默认标题
        private string GetDefaultValueForKey(string key)
        {
            // 获取当前选中的样式索引
            int selectedStyleTag = 0;
            if (listView_Style.SelectedItems.Count > 0 && listView_Style.SelectedItems[0].Tag != null)
            {
                selectedStyleTag = (int)listView_Style.SelectedItems[0].Tag;
            }
            if (selectedStyleTag == 0) selectedStyleTag = 200;

            // ---------------------------------------------------------
            // ★★★ 核心修改：根据样式ID返回不同的主标题 ★★★
            // ---------------------------------------------------------
            if (key == "主标题")
            {
                if (selectedStyleTag == 200) return "工区磁测平面图";      // 样式200
                if (selectedStyleTag == 300) return "重磁异常平面图";      // 样式300
                return "标准分幅地图"; // 其他默认
            }

            if (key == "左下角附注") return "2025年12月航测";
            if (key == "右下角附注") return "制图单位: ***单位";

            // 表格内的字段
            if (key == "制图单位") return "XX地质大队";
            if (key == "编    图") return "×××";
            if (key == "审    核") return "×××";
            if (key == "数字制图") return "×××";
            if (key == "技术负责") return "×××";
            if (key == "单位负责人") return "×××";
            if (key == "顺序号") return "××";
            if (key == "图    号") return "××";
            if (key == "比例尺") return "1:" + scale.ToString();
            if (key == "编图日期") return DateTime.Now.ToString("yyyy年MM月");
            if (key == "资料来源") return "实测";
            if (key == "等值线间距") return "0.2";

            return "";
        }

        private void ReFilterStyle()
        {
            object property = commonMapDecoration.GetProperty("GetAutoMapDecorationNum");
            if (property != null)
            {
                int lCount = (int)property;
                DrawList(lCount);
            }
        }
        private void treeList_Ann_CustomNodeCellEdit(object sender, DevExpress.XtraTreeList.GetCustomNodeCellEditEventArgs e)
        {
            if (e.Column == contentColumn)
            {
                e.RepositoryItem = contentMemoEdit;
            }
        }

        private void DrawList(int lCount)
        {
            listView_Style.SelectedItems.Clear();
            listView_Style.Items.Clear();
            largeImgList.Images.Clear();
            listView_Style.BeginUpdate();
            long num = lCount;
            for (int i = 0; i < num; i++)
            {
                commonMapDecoration.SetProperty("SetAutoMapDecoration", i);
                Image value = DrawDecoration(commonMapDecoration, largeImgList.ImageSize);
                ListViewItem listViewItem = new ListViewItem(new string[1] { commonMapDecoration.GetProperty("GetAutoMapDecorationName") as string }, largeImgList.Images.Count);
                try
                {
                    largeImgList.Images.Add(value);
                }
                catch
                {
                }
                listViewItem.Tag = i;
                listView_Style.Items.Add(listViewItem);
            }
            listView_Style.EndUpdate();
            if (listView_Style.Items.Count > 0)
            {
                commonMapDecoration.SetProperty("SetAutoMapDecoration", 0);
                RefreshTextList();
            }
        }

        private void WhenUpdateRange(object sender, EventArgs e)
        {
            if (isMapOK)
            {
                SaveUI();
                RefreshUIData();
            }
        }

        public static Image DrawDecoration(CommonMapDecoration decoration, Size size)
        {
            if (decoration == null)
            {
                return null;
            }
            if (size.Width <= 0 || size.Height <= 0)
            {
                return null;
            }
            double num = 0.0;
            double num2 = 0.0;
            Rect rect = new Rect();
            decoration.SetProperty("GetRect", rect.Handle);
            num = rect.XMax - rect.XMin;
            num2 = rect.YMax - rect.YMin;
            GridImgInfo gridImgInfo = new GridImgInfo();
            gridImgInfo.ImgType = ImgType.PNG;
            gridImgInfo.Width = size.Width;
            gridImgInfo.Height = size.Height;
            gridImgInfo.BlackClr = 9;
            gridImgInfo.BufCapacity = size.Width * size.Height * 4;
            gridImgInfo.Count = 1;
            gridImgInfo.CachWid = size.Width;
            gridImgInfo.CachHei = size.Height;
            gridImgInfo.JpgRate = 95;
            gridImgInfo.GifTranFlag = true;
            gridImgInfo.ChgPal = true;
            gridImgInfo.QualityMode = 3;
            double num3 = num / (double)size.Width;
            double num4 = num2 / (double)size.Height;
            double num5 = ((num3 > num4) ? num3 : num4);
            double num6 = rect.XMin + num / 2.0 - num5 * (double)size.Width / 2.0;
            double num7 = rect.YMin + num2 / 2.0 - num5 * (double)size.Height / 2.0;
            double num8 = num6 + num5 * (double)size.Width;
            double num9 = num7 + num5 * (double)size.Height;
            num = num8 - num6;
            num2 = num9 - num7;
            Rect rect2 = new Rect(num6 - num / 20.0, num7 - num2 / 20.0, num8 + num / 20.0, num9 + num2 / 20.0);
            Rect rect3 = new Rect(0.0, 0.0, size.Width - 1, size.Height - 1);
            Display display = new Display();
            Transformation transformation = display.GetTransformation();
            transformation.SetClientRect(rect3);
            transformation.SetDeviceRect(rect3);
            transformation.SetDispRect(rect2);
            transformation.SetMapRange(rect2);
            display.SetGridImageDevice(gridImgInfo);
            display.ShowSymbol = true;
            display.Begin();
            display.SetBrush(9, 1, 1.0, 1, 1f);
            display.FixedAnnSize = true;
            display.SymbolScale = 1.0;
            display.UnitMode = UnitMode.Logic;
            display.SetPen(1.0, 1);
            DisplayCanvas displayCanvas = new DisplayCanvas(display);
            bool flag = decoration.Make(displayCanvas);
            display.End();
            displayCanvas.Dispose();
            display.Dispose();
            byte[] array = (flag ? gridImgInfo.BufList[0] : null);
            Image result = null;
            if (array != null && array.Length != 0)
            {
                try
                {
                    result = Image.FromStream(new MemoryStream(array));
                }
                catch
                {
                }
            }
            return result;
        }

        private void listView_Style_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void RefreshUIData()
        {
            double num = dataRange.XMax - dataRange.XMin;
            double num2 = dataRange.YMax - dataRange.YMin;
            if (srcSRS.Type == SRefType.PRJ || srcSRS.Type == SRefType.JWD)
            {
                double num3 = SRefData.UnitConvert(GetSrsUnit(currentSRS), SRefLenUnit.MilliMeter);
                num *= num3 * GetSrsRate(currentSRS);
                num2 *= num3 * GetSrsRate(currentSRS);
                num /= (double)scale;
                num2 /= (double)scale;
            }
         ((Control)(object)labelControl_WH).Text = "输出宽高:" + Math.Ceiling(num2) + "×" + Math.Ceiling(num) + " mm";
            //ReFilterStyle();
        }

        private string GetNewMapName()
        {
            if (texts.ContainsKey("主标题"))
            {
                return texts["主标题"].ToString();
            }
            return "即时成图";
        }

        private void RefreshCurrentSRS()
        {
            if (srcSRS.Type != SRefType.PRJ && srcSRS.Type != SRefType.JWD)
            {
                currentSRS = srcSRS;
                return;
            }
            double lon = 0.0;
            double lon2 = 0.0;
            int num = 1;
            string text = "";
            string text2 = "";
            bool flag = false;
            if (srcDataPolygon.Count > 0)
            {
                lon = (lon2 = srcDataPolygon[0].X);
                for (int i = 0; i < srcDataPolygon.Count; i++)
                {
                    lon = ((lon > srcDataPolygon[i].X) ? srcDataPolygon[i].X : lon);
                    lon2 = ((lon2 < srcDataPolygon[i].X) ? srcDataPolygon[i].X : lon2);
                }
                double num2 = SRefData.UnitConvert(srcSRS.AngUnit, SRefLenUnit.Degree);
                lon *= num2;
                lon2 *= num2;
                if (scale != 1)
                {
                    flag = Math.Abs(lon2 - lon) > 12.0;
                }
                lon = AngleConvert.DToDMS(lon);
                lon2 = AngleConvert.DToDMS(lon2);
            }
            DataBase dataBase = DataBase.OpenTempDB();
            if (dataBase != null)
            {
                if (scale >= 500000 || flag)
                {
                    text2 = ((srcSRS.Spheroid != SRefEPType.Beijing54) ? "1:100万_西安80_12带6_N20_24" : "1:100万_北京54_12带6_N20_24");
                    currentSRS = dataBase.SpatialRefMng.Get(dataBase.SpatialRefMng.GetID(text2));
                    if (currentSRS != null)
                    {
                        currentSRS.SRSName = $"兰伯特等角圆锥投影";
                        currentSRS.Rate = 1.0;
                        currentSRS.Unit = SRefLenUnit.Meter;
                        currentSRS.UnitFactor = 1.0;
                    }
                }
                else if (scale >= 50000)
                {
                    text = ((srcSRS.Spheroid != SRefEPType.Beijing54) ? "高斯大地坐标系_西安80_" : "高斯大地坐标系_北京54_");
                    text2 = text + "20带6_北";
                    num = (FrmNoUtility.CalZoneByLon(lon, is6Not3: true) + FrmNoUtility.CalZoneByLon(lon2, is6Not3: true)) / 2;
                    text = text + num + "带6_北";
                    currentSRS = dataBase.SpatialRefMng.Get(dataBase.SpatialRefMng.GetID(text));
                    if (currentSRS == null)
                    {
                        currentSRS = dataBase.SpatialRefMng.Get(dataBase.SpatialRefMng.GetID(text2));
                        if (currentSRS != null)
                        {
                            currentSRS.SRSName = text;
                            currentSRS.Zone = num;
                            currentSRS.Lon = FrmNoUtility.CalCenterByZone(num, is6Not3: true);
                        }
                    }
                }
                else if (scale > 1)
                {
                    text = ((srcSRS.Spheroid != SRefEPType.Beijing54) ? "高斯大地坐标系_西安80_" : "高斯大地坐标系_北京54_");
                    text2 = text + "40带3_北";
                    num = (FrmNoUtility.CalZoneByLon(lon, is6Not3: false) + FrmNoUtility.CalZoneByLon(lon2, is6Not3: false)) / 2;
                    text = text + num + "带3_北";
                    currentSRS = dataBase.SpatialRefMng.Get(dataBase.SpatialRefMng.GetID(text));
                    if (currentSRS == null)
                    {
                        currentSRS = dataBase.SpatialRefMng.Get(dataBase.SpatialRefMng.GetID(text2));
                        if (currentSRS != null)
                        {
                            currentSRS.SRSName = text;
                            currentSRS.Zone = num;
                            currentSRS.Lon = FrmNoUtility.CalCenterByZone(num, is6Not3: true);
                        }
                    }
                }
                else
                {
                    currentSRS = srcSRS;
                }
                DataBase.FreeTempDB(dataBase);
            }
            if (currentSRS == null)
            {
                currentSRS = srcSRS;
            }
        }

        private SRefLenUnit GetSrsUnit(SRefData srs)
        {
            if (srs == null)
            {
                return SRefLenUnit.Meter;
            }
            if (srs.Type == SRefType.JWD)
            {
                return srs.AngUnit;
            }
            if (srs.Type == SRefType.PRJ)
            {
                return srs.Unit;
            }
            return SRefLenUnit.MilliMeter;
        }

        private double GetSrsRate(SRefData srs)
        {
            if (srs == null)
            {
                return 1.0;
            }
            if (srs.Type == SRefType.PRJ)
            {
                return srs.Rate;
            }
            return 1.0;
        }

        private bool IsCanPrj()
        {
            if (isProjectSrcSRS)
            {
                return true;
            }
            if (srcDataPolygon.Count == 0)
            {
                return false;
            }
            if (srcSRS.Type != SRefType.JWD && srcSRS.Type != SRefType.PRJ)
            {
                return false;
            }
            double num = SRefData.UnitConvert(srcSRS.AngUnit, SRefLenUnit.Degree);
            Rect range = map.Range;
            if (range == null)
            {
                return false;
            }
            range.XMin *= num;
            range.XMax *= num;
            range.YMin *= num;
            range.YMax *= num;
            double num2 = range.XMax - range.XMin;
            double num3 = range.YMax - range.YMin;
            if (num2 > 90.0 || num3 > 45.0)
            {
                return false;
            }
            double num4 = 0.0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num7 = 0.0;
            num4 = (num6 = srcDataPolygon[0].X);
            num5 = (num7 = srcDataPolygon[0].Y);
            for (int i = 1; i < srcDataPolygon.Count; i++)
            {
                num4 = ((num4 > srcDataPolygon[i].X) ? srcDataPolygon[i].X : num4);
                num6 = ((num6 < srcDataPolygon[i].X) ? srcDataPolygon[i].X : num6);
                num5 = ((num5 > srcDataPolygon[i].Y) ? srcDataPolygon[i].Y : num5);
                num7 = ((num7 < srcDataPolygon[i].Y) ? srcDataPolygon[i].Y : num7);
            }
            num4 *= num;
            num6 *= num;
            num5 *= num;
            num7 *= num;
            if (num4 > num6 || num5 > num7)
            {
                return false;
            }
            bool flag = false;
            if (num4 >= -180.0 && num4 <= 180.0 && num6 >= -180.0 && num6 <= 180.0)
            {
                flag = true;
            }
            if (num4 >= 0.0 && num4 <= 360.0 && num6 >= 0.0 && num6 <= 360.0)
            {
                flag = true;
            }
            if (!flag)
            {
                return false;
            }
            if (num5 >= -80.0 && num5 <= 80.0 && num7 >= -80.0 && num7 <= 80.0)
            {
                return true;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.panelLeft = new DevExpress.XtraEditors.PanelControl();
            this.groupControl_Text = new DevExpress.XtraEditors.GroupControl();
            this.treeList_Ann = new DevExpress.XtraTreeList.TreeList();
            this.typeColumn = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.contentColumn = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.contentMemoEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.groupControl_DataRange = new DevExpress.XtraEditors.GroupControl();
            this.labelControl_WH = new DevExpress.XtraEditors.LabelControl();
            this.groupControl_Scale = new DevExpress.XtraEditors.GroupControl();
            this.comboBoxEdit_Scale = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl_Sacle = new DevExpress.XtraEditors.LabelControl();
            this.groupControl_Styles = new DevExpress.XtraEditors.GroupControl();
            this.listView_Style = new System.Windows.Forms.ListView();
            this.largeImgList = new System.Windows.Forms.ImageList(this.components);
            this.panelBottom = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.simpleButton_Cancel = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_OK = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_Text)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeList_Ann)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.contentMemoEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_DataRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_Scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit_Scale.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_Styles)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            // 
            // 
            // 
            this.splitContainerControl1.Panel1.Controls.Add(this.panelLeft);
            this.splitContainerControl1.Panel1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Panel1.Name = "";
            this.splitContainerControl1.Panel1.Size = new System.Drawing.Size(320, 650);
            this.splitContainerControl1.Panel1.TabIndex = 0;
            // 
            // 
            // 
            this.splitContainerControl1.Panel2.Controls.Add(this.groupControl_Styles);
            this.splitContainerControl1.Panel2.Location = new System.Drawing.Point(328, 0);
            this.splitContainerControl1.Panel2.Name = "";
            this.splitContainerControl1.Panel2.Size = new System.Drawing.Size(472, 650);
            this.splitContainerControl1.Panel2.TabIndex = 1;
            this.splitContainerControl1.Size = new System.Drawing.Size(800, 650);
            this.splitContainerControl1.SplitterPosition = 320;
            this.splitContainerControl1.TabIndex = 0;
            // 
            // panelLeft
            // 
            this.panelLeft.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelLeft.Controls.Add(this.groupControl_Text);
            this.panelLeft.Controls.Add(this.groupControl_DataRange);
            this.panelLeft.Controls.Add(this.groupControl_Scale);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(320, 650);
            this.panelLeft.TabIndex = 0;
            // 
            // groupControl_Text
            // 
            this.groupControl_Text.Controls.Add(this.treeList_Ann);
            this.groupControl_Text.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl_Text.Location = new System.Drawing.Point(0, 260);
            this.groupControl_Text.Name = "groupControl_Text";
            this.groupControl_Text.Size = new System.Drawing.Size(320, 390);
            this.groupControl_Text.TabIndex = 0;
            this.groupControl_Text.Text = "框外文本元素";
            // 
            // treeList_Ann
            // 
            this.treeList_Ann.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.typeColumn,
            this.contentColumn});
            this.treeList_Ann.DataSource = null;
            this.treeList_Ann.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeList_Ann.Location = new System.Drawing.Point(3, 33);
            this.treeList_Ann.Name = "treeList_Ann";
            this.treeList_Ann.OptionsView.ShowRoot = false;
            this.treeList_Ann.Size = new System.Drawing.Size(314, 354);
            this.treeList_Ann.TabIndex = 0;
            this.treeList_Ann.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeList_Ann_CellValueChanged);
            this.treeList_Ann.CustomNodeCellEdit += new DevExpress.XtraTreeList.GetCustomNodeCellEditEventHandler(this.treeList_Ann_CustomNodeCellEdit);


            
        
            // 
            // typeColumn
            // 
            this.typeColumn.Caption = "类别";
            this.typeColumn.FieldName = "Type";
            this.typeColumn.Name = "typeColumn";
            this.typeColumn.OptionsColumn.AllowEdit = false;
            this.typeColumn.Visible = true;
            this.typeColumn.VisibleIndex = 0;
            this.typeColumn.Width = 80;
            // 
            // contentColumn
            // 
            this.contentColumn.Caption = "内容";
            //this.contentColumn.ColumnEdit = this.contentMemoEdit;
            this.contentColumn.FieldName = "Content";
            this.contentColumn.Name = "contentColumn";
            this.contentColumn.Visible = true;
            this.contentColumn.VisibleIndex = 1;
            // 
            // contentMemoEdit
            // 
            this.contentMemoEdit.Name = "contentMemoEdit";
            // 
            // groupControl_DataRange
            // 
            this.groupControl_DataRange.Controls.Add(this.labelControl_WH);
            this.groupControl_DataRange.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControl_DataRange.Location = new System.Drawing.Point(0, 60);
            this.groupControl_DataRange.Name = "groupControl_DataRange";
            this.groupControl_DataRange.Size = new System.Drawing.Size(320, 200);
            this.groupControl_DataRange.TabIndex = 1;
            this.groupControl_DataRange.Text = "范围(单位:未知)";
            // 
            // labelControl_WH
            // 
            this.labelControl_WH.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelControl_WH.Location = new System.Drawing.Point(3, 165);
            this.labelControl_WH.Name = "labelControl_WH";
            this.labelControl_WH.Padding = new System.Windows.Forms.Padding(5);
            this.labelControl_WH.Size = new System.Drawing.Size(109, 32);
            this.labelControl_WH.TabIndex = 0;
            this.labelControl_WH.Text = "输出宽高: ...";
            // 
            // groupControl_Scale
            // 
            this.groupControl_Scale.Controls.Add(this.comboBoxEdit_Scale);
            this.groupControl_Scale.Controls.Add(this.labelControl_Sacle);
            this.groupControl_Scale.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControl_Scale.Location = new System.Drawing.Point(0, 0);
            this.groupControl_Scale.Name = "groupControl_Scale";
            this.groupControl_Scale.ShowCaption = false;
            this.groupControl_Scale.Size = new System.Drawing.Size(320, 60);
            this.groupControl_Scale.TabIndex = 2;
            this.groupControl_Scale.Text = "基本参数";
            // 
            // comboBoxEdit_Scale
            // 
            this.comboBoxEdit_Scale.Location = new System.Drawing.Point(80, 23);
            this.comboBoxEdit_Scale.Name = "comboBoxEdit_Scale";
            this.comboBoxEdit_Scale.Size = new System.Drawing.Size(100, 28);
            this.comboBoxEdit_Scale.TabIndex = 0;
            this.comboBoxEdit_Scale.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.comboBoxEdit_Scale_EditValueChanging);
            // 
            // labelControl_Sacle
            // 
            this.labelControl_Sacle.Location = new System.Drawing.Point(10, -4);
            this.labelControl_Sacle.Name = "labelControl_Sacle";
            this.labelControl_Sacle.Size = new System.Drawing.Size(60, 22);
            this.labelControl_Sacle.TabIndex = 1;
            this.labelControl_Sacle.Text = "比例尺:";
            // 
            // groupControl_Styles
            // 
            this.groupControl_Styles.Controls.Add(this.listView_Style);
            this.groupControl_Styles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl_Styles.Location = new System.Drawing.Point(0, 0);
            this.groupControl_Styles.Name = "groupControl_Styles";
            this.groupControl_Styles.Size = new System.Drawing.Size(472, 650);
            this.groupControl_Styles.TabIndex = 0;
            this.groupControl_Styles.Text = "图饰样式";
            // 
            // listView_Style
            // 
            this.listView_Style.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_Style.HideSelection = false;
            this.listView_Style.LargeImageList = this.largeImgList;
            this.listView_Style.Location = new System.Drawing.Point(3, 33);
            this.listView_Style.Name = "listView_Style";
            this.listView_Style.Size = new System.Drawing.Size(466, 614);
            this.listView_Style.TabIndex = 0;
            this.listView_Style.UseCompatibleStateImageBehavior = false;
            this.listView_Style.SelectedIndexChanged += new System.EventHandler(this.listView_Style_SelectedIndexChanged);
            // 
            // largeImgList
            // 
            this.largeImgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.largeImgList.ImageSize = new System.Drawing.Size(250, 250);
            this.largeImgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.SystemColors.Control;
            this.panelBottom.Controls.Add(this.button2);
            this.panelBottom.Controls.Add(this.button1);
            this.panelBottom.Controls.Add(this.simpleButton_Cancel);
            this.panelBottom.Controls.Add(this.simpleButton_OK);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 650);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(800, 50);
            this.panelBottom.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(680, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 30);
            this.button2.TabIndex = 3;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(578, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 30);
            this.button1.TabIndex = 2;
            this.button1.Text = "完成";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // simpleButton_Cancel
            // 
            this.simpleButton_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton_Cancel.Location = new System.Drawing.Point(1300, 10);
            this.simpleButton_Cancel.Name = "simpleButton_Cancel";
            this.simpleButton_Cancel.Size = new System.Drawing.Size(75, 30);
            this.simpleButton_Cancel.TabIndex = 0;
            this.simpleButton_Cancel.Text = "取消";
            this.simpleButton_Cancel.Click += new System.EventHandler(this.simpleButton_Cancel_Click);
            // 
            // simpleButton_OK
            // 
            this.simpleButton_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton_OK.Location = new System.Drawing.Point(1210, 10);
            this.simpleButton_OK.Name = "simpleButton_OK";
            this.simpleButton_OK.Size = new System.Drawing.Size(75, 30);
            this.simpleButton_OK.TabIndex = 1;
            this.simpleButton_OK.Text = "完成(F)";
            this.simpleButton_OK.Click += new System.EventHandler(this.simpleButton_OK_Click);
            // 
            // StandardQuickMap
            // 
            this.ClientSize = new System.Drawing.Size(800, 700);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.panelBottom);
            this.MinimizeBox = false;
            this.Name = "StandardQuickMap";
            this.ShowIcon = false;
            this.Text = "快速成图";
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_Text)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeList_Ann)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.contentMemoEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_DataRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_Scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit_Scale.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_Styles)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // 记得把变量类型改一下
        private System.Windows.Forms.Panel panelBottom; // 改为标准 Panel

        // 补充定义需要的新变量
        private DevExpress.XtraEditors.PanelControl panelLeft;
        private DevExpress.XtraEditors.GroupControl groupControl_Scale;


        private GeoLines CreateBoxLine(Rect r)
        {
            GeoLines lines = new GeoLines();
            GeoVarLine vLine = new GeoVarLine();
            vLine.Append(new Dot(r.XMin, r.YMin));
            vLine.Append(new Dot(r.XMax, r.YMin));
            vLine.Append(new Dot(r.XMax, r.YMax));
            vLine.Append(new Dot(r.XMin, r.YMax));
            vLine.Append(new Dot(r.XMin, r.YMin));
            lines.Append(vLine);
            return lines;
        }

        private void DrawJoinTable(SFeatureCls lineCls, double x, double y, double unit)
        {
            double cellSize = 5.0 * unit;
            double startY = y + (2.0 * unit);

            LinInfo defaultInfo = new LinInfo();
            defaultInfo.LinStyID = 1;
            defaultInfo.OutClr = new int[] { 1 };
            defaultInfo.OutPenW = new float[] { 0.05f };

            for (int i = 0; i <= 3; i++)
            {
                GeoVarLine hLine = new GeoVarLine();
                hLine.Append(new Dot(x, startY + i * cellSize));
                hLine.Append(new Dot(x + 3 * cellSize, startY + i * cellSize));
                GeoLines lines = new GeoLines();
                lines.Append(hLine);
                lineCls.Append(lines, null, defaultInfo);
            }

            for (int i = 0; i <= 3; i++)
            {
                GeoVarLine vLine = new GeoVarLine();
                vLine.Append(new Dot(x + i * cellSize, startY));
                vLine.Append(new Dot(x + i * cellSize, startY + 3 * cellSize));
                GeoLines lines = new GeoLines();
                lines.Append(vLine);
                lineCls.Append(lines, null, defaultInfo);
            }
        }
        // 【新增方法】用于绘制覆盖全图的公里格网/方里网
        // 【修正版】绘制正方形网格
        private void DrawMapGrid(SFeatureCls lineCls, Rect rect, double unit)
        {
            // 1. 分别计算建议步长
            double stepX = CalculateNiceStep(rect.XMin, rect.XMax);
            double stepY = CalculateNiceStep(rect.YMin, rect.YMax);

            // 2. 【核心修改】强制取一致，保证是正方形
            // Math.Min 会让网格密一点(500x500)，Math.Max 会让网格稀一点(1000x1000)
            // 这里建议用 Min，这样网格密一点，图看起来不那么空
            double step = Math.Min(stepX, stepY);

            LinInfo gridInfo = new LinInfo();
            gridInfo.LinStyID = 1;
            gridInfo.OutClr = new int[] { 1 };
            gridInfo.OutPenW = new float[] { (float)(0.02 * unit) };

            // 3. 绘制竖线 (使用统一的 step)
            double startX = Math.Ceiling(rect.XMin / step) * step;
            if (startX > rect.XMin && (startX - step) >= rect.XMin - 0.0001) startX -= step;

            for (double x = startX; x <= rect.XMax; x += step)
            {
                if (Math.Abs(x - rect.XMin) < 0.001 || Math.Abs(x - rect.XMax) < 0.001) continue;

                GeoVarLine vLine = new GeoVarLine();
                vLine.Append(new Dot(x, rect.YMin));
                vLine.Append(new Dot(x, rect.YMax));
                GeoLines lines = new GeoLines(); lines.Append(vLine);
                lineCls.Append(lines, null, gridInfo);
            }

            // 4. 绘制横线 (使用统一的 step)
            double startY = Math.Ceiling(rect.YMin / step) * step;
            for (double y = startY; y <= rect.YMax; y += step)
            {
                if (Math.Abs(y - rect.YMin) < 0.001 || Math.Abs(y - rect.YMax) < 0.001) continue;

                GeoVarLine hLine = new GeoVarLine();
                hLine.Append(new Dot(rect.XMin, y));
                hLine.Append(new Dot(rect.XMax, y));
                GeoLines lines = new GeoLines(); lines.Append(hLine);
                lineCls.Append(lines, null, gridInfo);
            }
        }
        /// <summary>
        /// 根据地图范围，自动计算一个适合 A3/A4 纸的整数比例尺
        /// </summary>
        private int CalculateSmartScale(Rect range)
        {
            if (range == null) return 1000;

            // 1. 获取地图实际宽度 (单位：米，假设是投影坐标系)
            double geoWidth = range.XMax - range.XMin;

            // 2. 设定目标图纸的绘图区域宽度 (单位：毫米)
            // A3纸宽约420mm，除去边框留350mm绘图区比较合适
            // A4纸宽约297mm，除去边框留250mm
            double paperWidthMm = 350.0;

            // 3. 计算原始比例尺
            // 公式：比例尺分母 = 地图实际距离(mm) / 图纸距离(mm)
            // 1米 = 1000毫米
            double rawScale = (geoWidth * 1000.0) / paperWidthMm;

            // 4. 将计算出的怪异数字(如 1234) 规整为标准比例尺 (500, 1000, 2000...)
            // 定义常用的标准比例尺列表
            int[] standardScales = new int[] {
  200, 500, 1000, 2000, 5000, 10000, 25000, 50000, 100000
 };

            // 找到第一个能包得住的比例尺（即比 rawScale 大的最小标准比例尺）
            // 这样能保证图能完全画在纸上
            foreach (int s in standardScales)
            {
                if (s >= rawScale) return s;
            }

            // 如果范围超级大，就取个整
            return (int)(Math.Ceiling(rawScale / 1000.0) * 1000);
        }
        // 【新增方法】绘制公里网/经纬网的坐标注记
        // 【修正版】绘制公里网/经纬网的坐标注记
        private void DrawGridLabels(AnnotationCls annCls, Rect rect, double unit)
        {
            if (annCls == null) return;

            double stepX = CalculateNiceStep(rect.XMin, rect.XMax);
            double stepY = CalculateNiceStep(rect.YMin, rect.YMax);

            // 【修改点1】字体参数统一
            // 表格里用的字号是 3.0mm，这里坐标稍微小一点点(2.5mm)或者一样大都可以
            float textHeight = (float)(3.5 * unit);

            // 【修改点2】宽度设为与高度一致 (1.0倍)，不再用 0.7f，这样字体就“正”了，和表格一样
            float textWidth = textHeight;

            double offset = 2.0 * unit;

            // 【修改点3】字体设置，保持和表格一致 (不设置Ifnt则默认)
            TextAnnInfo commonInfo = new TextAnnInfo();
            commonInfo.Ovprnt = true;
            // commonInfo.Ifnt = 0; // 如果需要强制宋体，可以加上这句，通常默认就是

            // --- 1. X轴坐标 ---
            double startX = Math.Ceiling(rect.XMin / stepX) * stepX;
            for (double x = startX; x <= rect.XMax; x += stepX)
            {
                if (Math.Abs(x - rect.XMin) < 0.001 || Math.Abs(x - rect.XMax) < 0.001) continue;

                string textVal = x.ToString("0.###");

                // =========================================================
                // ↓↓↓ 【修改这里】 下边注记 ↓↓↓
                // =========================================================
                TextAnno bottomAnn = new TextAnno();
                bottomAnn.Text = textVal;
                bottomAnn.Height = textHeight;
                bottomAnn.Width = textWidth;
                double halfLen = (textVal.Length * textWidth) / 2.0;

                // 原来是: rect.YMin - offset
                // 修改为: rect.YMin - offset - textHeight
                // 解释: 因为文字是向上画的，所以要多减去一个字高，让文字彻底“沉”到线下
                bottomAnn.AnchorDot = new Dot(x - halfLen, rect.YMin - offset - textHeight);

                annCls.Append(bottomAnn, null, commonInfo);

                // =========================================================
                // ↑↑↑ 修改结束 ↑↑↑
                // =========================================================

                // 上边
                TextAnno topAnn = new TextAnno();
                topAnn.Text = textVal;
                topAnn.Height = textHeight;
                topAnn.Width = textWidth;
                topAnn.AnchorDot = new Dot(x - halfLen, rect.YMax + offset);
                annCls.Append(topAnn, null, commonInfo);
            }

            // --- 2. Y轴坐标 ---
            double startY = Math.Ceiling(rect.YMin / stepY) * stepY;
            for (double y = startY; y <= rect.YMax; y += stepY)
            {
                if (Math.Abs(y - rect.YMin) < 0.001 || Math.Abs(y - rect.YMax) < 0.001) continue;

                string textVal = y.ToString("0.###");

                // 左边
                TextAnno leftAnn = new TextAnno();
                leftAnn.Text = textVal;
                leftAnn.Height = textHeight;
                leftAnn.Width = textWidth; // 使用正方比例
                double textLen = textVal.Length * textWidth;
                leftAnn.AnchorDot = new Dot(rect.XMin - offset - textLen, y - textHeight / 2.0);
                annCls.Append(leftAnn, null, commonInfo);

                // 右边
                TextAnno rightAnn = new TextAnno();
                rightAnn.Text = textVal;
                rightAnn.Height = textHeight;
                rightAnn.Width = textWidth;
                rightAnn.AnchorDot = new Dot(rect.XMax + offset, y - textHeight / 2.0);
                annCls.Append(rightAnn, null, commonInfo);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. 【关键】强制结束左下角表格的编辑状态，确保你最后输入的文字被保存
            if (treeList_Ann.ActiveEditor != null)
            {
                treeList_Ann.CloseEditor();
            }
            treeList_Ann.EndCurrentEdit();

            // 2. 开始成图逻辑
            if (isMapOK)
            {
                Map map = null;
                map = MakeData(); // 调用核心成图方法

                // 将生成的地图添加到当前文档中
                if (map != null && this.map.Parent is Document document)
                {
                    document.GetMaps().Append(map);

                    // 通知 MapGIS 刷新界面显示新图
                    if (app != null)
                    {
                        app.WorkSpaceEngine.FireMenuItemClickEvent("MapGIS.WorkSpace.Style.PreviewMap", map);
                        app.WorkSpaceEngine.GetMapControl(map)?.Restore();
                    }
                }
            }

            // 3. 关闭窗口并返回 OK 状态
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 直接关闭窗口，不保存任何操作
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
