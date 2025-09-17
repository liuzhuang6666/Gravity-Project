using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class Command1 : ICommand
    {
        #region ICommand成员
        public Bitmap Bitmap
        {
            get { return null; }
        }

        public string Caption
        {
            get { return "Command1"; }
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
            get { return "Command1"; }
        }

        public string Tooltip
        {
            get { return ""; }
        }

        public void OnClick()
        {
            new Form3(this._hk.Document).Show();
        }

        public void OnCreate(IApplication hook)
        {
            this._hk = hook;

        }
        private IApplication _hk;
        #endregion
    }
}
