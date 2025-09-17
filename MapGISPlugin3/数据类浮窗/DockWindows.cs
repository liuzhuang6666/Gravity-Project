using MapGIS.PluginEngine;
using MapGIS.PlugUtility;
using MapGISPlugin3;
using MapGISPlugin3.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    /// <summary>
    /// 场景树
    /// </summary>
    public class ProjectTreeDockWindow : IDockWindow
    {
        private ProjectTreeControl xUC;

        public Bitmap Bitmap
        {
            get { return null ; }
        }

        public string Caption
        {
            get { return "流程工作树"; }
        }

        public Control ChildHWND
        {
            get { return xUC; }
        }

        public DockingStyle DefaultDock
        {
            get { return DockingStyle.Left; }
        }

        public bool InitCreate
        {
            get { return true; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public ProjectTreeControl TreeControl
        {
            get { return xUC; }
        }

        public void OnActive(bool isActive)
        {
        }

        public List<TreeItemInfo> GetSelectItems()
        {
            if (xUC != null)
                return xUC.SelectItems;
            return null;
        }

        public void OnCreate(IApplication app)
        {
            xUC = new ProjectTreeControl(app);
            ProjectManage prjManage = ProjectManage.GetInstance();
            if (prjManage != null)
            {
                prjManage.prjChange += PrjManage_prjChange;
            }
        }

        private void PrjManage_prjChange(object openPrj)
        {
            if (openPrj != null && openPrj is Project)
            {
                xUC.SetProject(openPrj as Project);
            }
            else

                xUC.SetProject(null);
        }

        public void OnDestroy()
        {
        }
    }
}