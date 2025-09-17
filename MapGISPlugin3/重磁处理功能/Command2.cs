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
    public class TestCmd : ICommand
    {
        #region ICommand成员
        IApplication hook = null;
        Document doc = null;
        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "测试"; }
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
            get { return "ceshi"; }
        }

        public string Name
        {
            get { return "位场转换处理"; }
        }

        public string Tooltip
        {
            get { return ""; }
        }

        public void OnClick()
        {
            Form1 form = new Form1(hook);
            form.Show();
            
        }

        public void OnCreate(IApplication hook)
        {
            this.hook = hook;
        }
       

        #endregion
    }
}
