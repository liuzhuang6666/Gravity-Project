using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraBars;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoDataBase.GeoMosaicRaster;
using MapGIS.GeoMap;
using MapGIS.GeoMap.EditUtility;
using MapGIS.GeoObjects.Geometry;
using MapGIS.GeoObjects.Att;
using MapGIS.GISControl;
using MapGIS.PlugUtility;
using MapGIS.GeoObjects.SpatialRef;


namespace MapGISPlugin3
{
    public partial class Form4 : Form
    {
        private DataTable dataTable;                    //数据表
        private MapControl m_MapCtr;                    //地图视图
        private RasterLayer m_RasterLayer;              //栅格图层
        private RasterDataSet m_Rasdataset;             //栅格数据集
        private RasCatalogLayer m_RasCatLayer;          //栅格目录图层
        private RasterCatalog m_RasCatalog;             //栅格目录
        private MosaicDatasetLayer m_MosaicDataSetLayer;//镶嵌数据集图层
        private MosaicDataSet m_MosaicDataSet;          //镶嵌数据集
        private bool m_CanShowForm = true;//return之前需设置该属性否则弹框进来就报错


        private Dot dot = new Dot();                    //全局dot
        private Rectangle rect;                         //全局rect
        private bool moving;                            //移动标志
        private List<Rect> m_LstRect = new List<Rect>();//栅格目录中栅格数据集范围列表

        private string m_TextEdit1Value;                //TextEdit1的值
        private string m_TextEdit2Value;                //TextEdit2的值
        private string m_TextEdit3Value;                //TextEdit3的值
        private string m_TextEdit4Value;                //TextEdit4的值

        private double m_XResolution;
        private double m_YResolution;

        private bool m_DataChangeFlag = false;
        /*public GetPixelForm(MapControl control)
        {
            InitializeComponent();
            m_MapCtr = control;
            List<MapLayer> lst = m_MapCtr.ActiveMap.GetEditLayer(EditLayerType.MOSAICRAS, SelectLayerControl.Active);
            if (lst.Count != 0)
            {
                m_MosaicDataSetLayer = lst[0] as MosaicDatasetLayer;
                // 修改说明：除错判断，当lst中第一个不是镶嵌数据集图层时，返回
                if (m_MosaicDataSetLayer == null)
                {
                    this.m_CanShowForm = false;
                    return;
                }
                // 修改说明：原因：对于非模态对话框，当从树上移除当前操作的图层后，再对该图层进行操作，程序崩溃。
                //          修改：监控树上图层移除事件，当发现移除的是当前操作图层，则关闭对话框。
                DocumentItem document = m_MosaicDataSetLayer.Parent;
                if (document is GroupLayer)
                {
                    GroupLayer group = document as GroupLayer;
                    group.RemoveLayer += new GroupLayer.RemoveLayerHandle(RemoveLayer);
                }
                else if (document is Map)
                {
                    Map map = document as Map;
                    map.RemoveLayer += new Map.RemoveLayerHandle(RemoveLayer);
                }
                m_MosaicDataSet = m_MosaicDataSetLayer.GetData() as MosaicDataSet;
                m_XResolution = m_MosaicDataSet.RasterInfo.cellSize.xsize;
                m_YResolution = m_MosaicDataSet.RasterInfo.cellSize.ysize;
            }
            else
            {
                lst = m_MapCtr.ActiveMap.GetEditLayer(EditLayerType.RasterDataset, SelectLayerControl.Active);
                if (lst.Count != 0)
                {
                    m_RasterLayer = lst[0] as RasterLayer;
                    // 修改说明：除错判断，当lst中第一个不是栅格图层时，返回
                    // 修改人：韩俊鹏 2014-04-16
                    if (m_RasterLayer == null)
                    {
                        this.m_CanShowForm = false;
                        return;
                    }
                    // 修改说明：原因：对于非模态对话框，当从树上移除当前操作的图层后，再对该图层进行操作，程序崩溃。
                    //          修改：监控树上图层移除事件，当发现移除的是当前操作图层，则关闭对话框。
                    // 修改人：韩俊鹏 2014-05-09
                    DocumentItem document = m_RasterLayer.Parent;
                    if (document is GroupLayer)
                    {
                        GroupLayer group = document as GroupLayer;
                        group.RemoveLayer += new GroupLayer.RemoveLayerHandle(RemoveLayer);
                    }
                    else if (document is Map)
                    {
                        Map map = document as Map;
                        map.RemoveLayer += new Map.RemoveLayerHandle(RemoveLayer);
                    }
                    m_Rasdataset = m_RasterLayer.GetData() as RasterDataSet;
                    if (m_Rasdataset == null)
                    {
                        this.m_CanShowForm = false;
                        return;
                    }
                    m_XResolution = m_Rasdataset.XResolution;
                    m_YResolution = m_Rasdataset.YResolution;
                }
                lst = m_MapCtr.ActiveMap.GetEditLayer(EditLayerType.RasterCat, SelectLayerControl.Active);
                if (lst.Count != 0)
                {
                    m_RasCatLayer = lst[0] as RasCatalogLayer;
                    // 修改说明：除错判断，当lst中第一个不是栅格目录图层时，返回
                    // 修改人：韩俊鹏 2014-04-16
                    if (m_RasCatLayer == null)
                    {
                        if (m_RasterLayer == null)
                        {
                            this.m_CanShowForm = false;
                        }
                        return;
                    }

                    // 修改说明：原因：对于非模态对话框，当从树上移除当前操作的图层后，再对该图层进行操作，程序崩溃。
                    //          修改：监控树上图层移除事件，当发现移除的是当前操作图层，则关闭对话框。
                    // 修改人：韩俊鹏 2014-05-09
                    DocumentItem document = m_RasCatLayer.Parent;
                    if (document is GroupLayer)
                    {
                        GroupLayer group = document as GroupLayer;
                        group.RemoveLayer += new GroupLayer.RemoveLayerHandle(RemoveLayer);
                    }
                    else if (document is Map)
                    {
                        Map map = document as Map;
                        map.RemoveLayer += new Map.RemoveLayerHandle(RemoveLayer);
                    }
                    m_RasCatalog = m_RasCatLayer.GetData() as RasterCatalog;
                }

                if (m_RasCatalog != null && m_Rasdataset != null)
                {
                    int indexraslayer = m_MapCtr.ActiveMap.IndexOf(m_RasterLayer);
                    int indexrascatlayer = m_MapCtr.ActiveMap.IndexOf(m_RasCatLayer);
                    if (indexrascatlayer > indexraslayer)
                    {
                        m_RasterLayer = null;
                        m_Rasdataset = null;
                    }
                    else
                    {
                        m_RasCatLayer = null;
                        m_RasCatalog = null;
                    }
                }
            }

            TransParams tp = null;
            if (m_MosaicDataSet != null)
            {
                tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_MosaicDataSetLayer);
            }
            else
            {
                if (m_RasCatalog == null)
                    tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasterLayer);
                else
                    tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasCatLayer);
            }
            double xmin = 0, ymin = 0, xmax = 0, ymax = 0;
            this.m_MapCtr.Transformation.MpToLp(this.m_MapCtr.DispRect.XMin, this.m_MapCtr.DispRect.YMin, ref xmin, ref ymin);
            this.m_MapCtr.Transformation.MpToLp(this.m_MapCtr.DispRect.XMax, this.m_MapCtr.DispRect.YMax, ref xmax, ref ymax);
            dot.X = (xmin + xmax) / 2;
            dot.Y = (ymin + ymax) / 2;
            OperTransformation.ResumeSourceSRef(tp);//回设参照系
            if (m_MosaicDataSet != null)
            {
                this.textEdit1.Properties.ReadOnly = false;
                this.textEdit2.Properties.ReadOnly = false;
            }
            else
            {
                if (m_RasCatalog != null)
                {
                    int n = m_RasCatalog.GetItemNum();
                    for (int i = n; i >= 1; i--)
                    {
                        RasterDataSet rasterdataset = m_RasCatalog.GetItem(i);
                        m_LstRect.Add(rasterdataset.GetMapRange());
                        rasterdataset.Close();
                    }
                }

                if (m_RasCatalog != null)
                {
                    this.textEdit1.Properties.ReadOnly = true;
                    this.textEdit2.Properties.ReadOnly = true;
                }
                else
                {
                    this.textEdit1.Properties.ReadOnly = false;
                    this.textEdit2.Properties.ReadOnly = false;
                }
            }

            m_Parent = IntPtr.Zero;

            this.dataTable = new DataTable();
            this.dataTable.Columns.Add(MapGIS.RasterEditor.Plugin.Properties.Resources.String_Band);
            this.dataTable.Columns.Add(MapGIS.RasterEditor.Plugin.Properties.Resources.String_ItemValue, typeof(string));
            this.gridControl1.DataSource = this.dataTable;
            //this.gridView1.Columns[0].Width = 65;
            this.gridView1.Columns[0].OptionsColumn.AllowEdit = false;
            this.gridView1.Columns[1].OptionsColumn.AllowEdit = true;
            this.gridView1.Columns[1].OptionsColumn.ReadOnly = true;
            this.gridView1.Columns[0].Width = 25;
            this.FormClosed += new FormClosedEventHandler(GetPixelForm_FormClosed);
            this.m_MapCtr.PreRefresh += new MapControl.PreRefreshEventHandle(m_MapCtr_PreRefresh);
            this.m_MapCtr.PostRefresh += new MapControl.PostRefreshEventHandle(mapCtr_PostRefresh);
            this.m_MapCtr.MouseDown += new MouseEventHandler(mapCtr_MouseDown);
            this.m_MapCtr.MouseMove += new MouseEventHandler(mapCtr_MouseMove);
            this.m_MapCtr.MouseUp += new MouseEventHandler(mapCtr_MouseUp);
            this.m_MapCtr.MouseDoubleClick += new MouseEventHandler(mapCtr_MouseDoubleClick);
        }

        private void GetPixelForm_Load(object sender, EventArgs e)
        {
            this.m_MapCtr.Cursor = Cursors.Default;

            DrawCross();
            ShowTextEdit();
            ShowPixlValue();//更新窗口
        }
        private void RemoveLayer(object sender, GroupLayerEventArgs e)
        {
            if (m_RasterLayer != null)
            {
                if (e.RemovedLayer.Handle == m_RasterLayer.Handle)
                    this.Close();
            }
            if (m_RasCatLayer != null)
            {
                if (e.RemovedLayer.Handle == m_RasCatLayer.Handle)
                    this.Close();
            }
            if (m_MosaicDataSetLayer != null)
            {
                if (e.RemovedLayer.Handle == m_MosaicDataSetLayer.Handle)
                    this.Close();
            }
        }
        private void DrawCross()
        {
            if ((int)this.m_MapCtr.ViewHandle > 0)
            {
                TransParam tp = null;
                if (m_MosaicDataSetLayer != null)
                {
                    tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_MosaicDataSetLayer);
                }
                else
                {
                    if (m_RasCatalog == null)
                        tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasterLayer);
                    else
                        tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasCatLayer);
                }
                EditXORDraw exr = new EditXORDraw(this.m_MapCtr.Display);
                exr.Begin(IntPtr.Zero);
                exr.SetUnitMode(UnitMode.Pixel);
                exr.SetPen(0.5, 6);
                double xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                this.m_MapCtr.Transformation.MpToLp(this.m_MapCtr.DispRect.XMin, this.m_MapCtr.DispRect.YMin, ref xmin, ref ymin);
                this.m_MapCtr.Transformation.MpToLp(this.m_MapCtr.DispRect.XMax, this.m_MapCtr.DispRect.YMax, ref xmax, ref ymax);
                exr.DrawLine(new Dot(xmin, dot.Y), new Dot(xmax, dot.Y), false);
                exr.DrawLine(new Dot(dot.X, ymin), new Dot(dot.X, ymax), false);
                exr.EndNoBlt();//结束绘制
                int x = 0, y = 0;
                this.m_MapCtr.Transformation.LpToWp(dot.X, dot.Y, ref x, ref y);//逻辑坐标转窗口坐标
                rect = new Rectangle(x - 5, y - 5, 10, 10);//计算全局rect
                OperTransformation.ResumeSourceSRef(tp);//回设参照系
            }
        }


        void mapCtr_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button && rect.Contains(e.Location))
            {
                this.moving = true;
                DrawCross();//擦去上一次
                #region 根据按下点计算全局dot
                double x = 0, y = 0;
                TransParams tp = null;
                if (m_MosaicDataSet != null)
                {
                    tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_MosaicDataSetLayer);
                }
                else
                {
                    if (m_RasCatalog == null)
                        tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasterLayer);
                    else
                        tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasCatLayer);
                }
                this.m_MapCtr.Transformation.WpToLp(e.Location.X, e.Location.Y, ref x, ref y);
                OperTransformation.ResumeSourceSRef(tp);//回设参照系
                dot.X = x;
                dot.Y = y;
                #endregion
                DrawCross();//绘制新十字
                ShowTextEdit();
                ShowPixlValue();//更新窗口
            }
        }

        void mapCtr_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button && this.moving)
            {
                DrawCross();//擦去上一次
                #region 根据移动点计算全局dot
                double x = 0, y = 0;
                TransParams tp = null;
                if (m_MosaicDataSet != null)
                {
                    tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_MosaicDataSetLayer);
                }
                else
                {
                    if (m_RasCatalog == null)
                        tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasterLayer);
                    else
                        tp = OperTransformation.SetSourceSRef(this.m_MapCtr.Transformation, m_RasCatLayer);
                }
                this.m_MapCtr.Transformation.WpToLp(e.Location.X, e.Location.Y, ref x, ref y);
                OperTransformation.ResumeSourceSRef(tp);//回设参照系
                dot.X = x;
                dot.Y = y;
                #endregion
                DrawCross();//绘制新十字
                ShowTextEdit();
                ShowPixlValue();//更新窗口
            }
        }

        void mapCtr_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                this.moving = false;
            }
        }
        public class GetPixelFormTool : MapGIS.Desktop.UI.Controls.AbsCommonFormTool
        {
            private GetPixelForm _GetPixelForm;

            public GetPixelFormTool(MapControl mapCtl)
            {
                _Form = _GetPixelForm = new GetPixelForm(mapCtl);
            }
        }
    }*/
    }
}
