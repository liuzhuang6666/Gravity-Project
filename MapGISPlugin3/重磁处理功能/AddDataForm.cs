using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MapGIS.GeoDataBase.GeoRaster;
using MapGIS.GeoMap;
using MapGIS.Desktop.UI.Controls;
using MapGIS.UI.Controls;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using MapGIS.PlugUtility;

namespace MapGISPlugin3
{
    public partial class AddDataForm : DevExpress.XtraEditors.XtraForm
    {
        Map m_Map = null;
        private Point mousePoint = new Point();

        private string m_Url;// 数据URL

        /// <summary>
        /// 数据URL
        /// </summary>
        public string Url
        {
            get { return m_Url; }
        }
        
        /// <summary>
        /// 数据波段
        /// </summary>

        public AddDataForm(Document doc)
        {
            this.layerSelectComboBoxRas = new LayerSelectComboBox();
            InitializeComponent();
            InitTitleDrag();

            Maps maps = doc.GetMaps();
            m_Map = maps.GetMap(0);




            ((System.ComponentModel.ISupportInitialize)(this.layerSelectComboBoxRas.Properties)).BeginInit();
            this.layerSelectComboBoxRas.CanEdit = false;
            this.layerSelectComboBoxRas.Document = null;
            this.layerSelectComboBoxRas.Map = null;
            this.layerSelectComboBoxRas.Location = new Point(85, 45);
            this.layerSelectComboBoxRas.Size = new System.Drawing.Size(150, 30);


            this.layerSelectComboBoxRas.Properties.AutoHeight = false;     
    this.layerSelectComboBoxRas.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
    new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo),
    new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), null, "Tooltip Text", null, null),
    new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, false, true, false, DevExpress.XtraEditors.ImageLocation.MiddleRight, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), null, "Another Tooltip", null, null)
});

            this.Controls.Add(layerSelectComboBoxRas);

            this.layerSelectComboBoxRas.Properties.PopupFormSize = new System.Drawing.Size(259, 0);
            this.layerSelectComboBoxRas.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.layerSelectComboBoxRas.ShowOpenFileButton = true;
            this.layerSelectComboBoxRas.ShowTextEditImage = true;

            ((System.ComponentModel.ISupportInitialize)(this.layerSelectComboBoxRas.Properties)).EndInit();

            this.layerSelectComboBoxRas.Document = doc; 
            this.layerSelectComboBoxRas.Filter = CommonFunction.ReadInFilter();
            this.layerSelectComboBoxRas.Document = doc;
            this.layerSelectComboBoxRas.PreAddLayerEvent += new PreAddLayerHandler(layerSelectComboBoxRas_PreAddLayerEvent);
            this.layerSelectComboBoxRas.SelectedIndexChanged += new EventHandler(layerSelectComboBoxRas_SelectedIndexChanged);
            this.layerSelectComboBoxRas.SelectFirstItem();

            


        }
        /// <summary>
        /// 初始化标题栏拖动事件
        /// </summary>
        private void InitTitleDrag()
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

        private bool layerSelectComboBoxRas_PreAddLayerEvent(object sender, object layer)
        {
            if (layer is RasterDataSet)
            {
                return true;
            }
            else if (layer is string)
            {
                RasterDataSet rds = new RasterDataSet();
                if (rds.Open(layer as string, RasAccessType.RasAccessType_ReadOnly))
                {
                    rds.Close();
                    return true;
                }
                else
                {
                    bool isSucceed = MapGIS.Desktop.UI.Controls.MapGISErrorForm.ShowLastError();
                    
                    return false;
                }
            }
            return false;
        }
        private void layerSelectComboBoxRas_SelectedIndexChanged(object sender, EventArgs e)
        {
            string url = this.layerSelectComboBoxRas.SelectedItemUrl;
            
            RasterDataSet rasterdataset = new RasterDataSet();

            /*o = false;
            o = rasterdataset.Open(url, RasAccessType.RasAccessType_ReadOnly);
            if (o)
            {
                this.comboBoxEditBand.Properties.Items.Clear();
                int bandCount = rasterdataset.BandCount;
                for (int i = 0; i < bandCount; i++)
                {
                    string des = string.Format("b", i + 1);
                    this.comboBoxEditBand.Properties.Items.Add(des);
                }
                this.comboBoxEditBand.SelectedIndex = 0;
                rasterdataset.Close();
            }
            else
            {
                bool isSucceed = MapGIS.Desktop.UI.Controls.MapGISErrorForm.ShowLastError();
                if (!isSucceed)
                    XMessageBox.Information("G");
            }*/

        }
        private void simpleButtonOK_Click(object sender, EventArgs e)
        {
            m_Url = this.layerSelectComboBoxRas.SelectedItemUrl;
           
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public class CommonFunction
        {
            public static string ReadInFilter()
            {
                string filter = "栅格数据" + "|ras|MAPGISMSI (*.msi)|*.msi|Gtiff (*.tif)|*.tif|HFA (*.img)|*.img|*.grd" + "格式数据"+ "|*.*";
                return filter;
            }
            public static string SaveOutFilter()
            {
          string filter = "栅格数据" + "|ras|" + "MAPGISMSI (*.msi)" + "|*.msi|" + "GTiff (*.tif)" + "|*.tif|" + "HFA (*.img)" + "|*.img";
                return filter;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
