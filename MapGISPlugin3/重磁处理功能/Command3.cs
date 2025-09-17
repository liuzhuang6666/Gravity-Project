using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapGIS.PluginEngine;
using System.Drawing;
using MapGIS.GeoMap;
using MapGIS.GISControl;
namespace MapGISPlugin3
{
    public class Command3 : ICommand
    {
        #region ICommand成员
        IApplication hock;
        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "位场曲面延拓处理"; }
        }

        public string Category
        {
            get { return "MapGISPlugin3"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Message
        {
            get { return ""; }
        }

        public string Name
        {
            get { return "Command3"; }
        }

        public string Tooltip
        {
            get { return ""; }
        }

        public void OnClick()
        {


        }

        public void OnCreate(IApplication hook)
        {

            /*if (hook != null)
            {
                this.hock = hook;
                this.hock.StateManager.StateChangedEvent += new StateChangedHandler(StateManager_StateChangedEvent);
            }
        }

             void StateManager_StateChangedEvent(object sender, StateEventArgs e)
        {
            this.hock.PluginContainer.PluginEnable(this, false);
            bool bEnable = false;
            if (this.hock.ActiveContentsView != null && this.hock.ActiveContentsView is IMapContentsView)
            {
                //当存在当前编辑状态的图层时，才可以进行查询
                MapControl ctr = (this.hock.ActiveContentsView as IMapContentsView).MapControl;
                if (ctr != null)
                {
                    Map map = ctr.ActiveMap;
                    if (map != null)
                    {
                        List<MapLayer> Activelayers = map.GetEditLayer(EditLayerType.Line | EditLayerType.Pnt | EditLayerType.Reg , SelectLayerControl.Editable);
                        if (Activelayers != null && Activelayers.Count > 0)
                            bEnable = true;
                    }
                }
            }
            //显示窗口
            IDockWindow dw = null;
            this.hock.PluginContainer.DockWindows.TryGetValue(typeof(DW_ShowQueryTurns).ToString(), out dw);

            if (dw != null && this.hock.PluginContainer.PluginIsVisible(dw))
            {
                this.hock.PluginContainer.PluginVisible(dw, bEnable);
            }
            this.hock.PluginContainer.PluginEnable(this, bEnable);


            return;
        }
         }*/

            #endregion
        }
    }
}