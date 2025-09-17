using MapGIS.GeoDataBase;
using MapGIS.GeoMap;
using MapGIS.GeoObjects.Geometry;
using MapGIS.PluginEngine;
using MapGIS.WorkSpace.Style;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MapGISPlugin3
{
    public class Project
    {
        private DataInfo dataInfo = null;
        private string mpjPath = "";
        private ResourceManage rceManage = null;
        private string mapPath = "";
        private string hdfPath = "";
        DataBase gdb = null;
        Server ser = null;
        //OTHER_LAYER

        /// <summary>
        /// m3d数据信息，考虑到内存大小与读取速率现在没有将所有数据读取出来，如果需要将代码放开即可
        /// </summary>
        public DataInfo DataInfo
        {
            get { return dataInfo; }

            set { dataInfo = value; }
        }

        public string MpjPath
        {
            get { return mpjPath; }
        }

        /// <summary>
        /// 资源管理
        /// </summary>
        public ResourceManage RceManage
        {
            get { return rceManage; }
        }

        public string MapPath
        {
            get
            {
                return mapPath;
            }
        }

        public string HdfPath
        {
            get
            {
                return hdfPath;
            }
        }

        //public Dictionary<string, string> LayerUrls
        //{
        //    get {  return layerUrls;  }
        //}

        public int Open(string mpjPath)
        {
            Close();
            this.mpjPath = mpjPath;
            dataInfo = ReadDataInfo(mpjPath);
            if (dataInfo != null)
            {
                string dirPath = Path.GetDirectoryName(mpjPath);
                string fileName = Path.GetFileNameWithoutExtension(mpjPath);
                if (!string.IsNullOrWhiteSpace(dirPath))
                {
                    hdfPath = Path.Combine(dirPath, "资源", fileName + ".hdf");
                    ser = new Server();
                    if (!string.IsNullOrWhiteSpace(hdfPath) && File.Exists(hdfPath) && ser.Connect("MapGISLocal", "", ""))
                    {
                        AttachGDB(fileName);
                    }
                    mapPath = Path.Combine(dirPath, fileName + ".mapx");
                    if (!File.Exists(mapPath))
                    {
                        Document doc = new Document();
                        Map Map = new Map();
                        Map.Name = Path.GetFileNameWithoutExtension(fileName);
                        Map.SetPropertyEx("InitOpenView", "true");
                        
                        RasterLayer rasterLayer = new RasterLayer();
                        rasterLayer.URL = "FILE:///" + mpjPath;
                        rasterLayer.Name = fileName;
                        try
                        {
                            if (rasterLayer.IsValid)
                            {
                                Map.Append(rasterLayer);
                            }
                        }
                        catch { }
                        doc.GetMaps().Append(Map);
                        doc.SaveAs(mapPath);
                        doc.Close(false);
                    }
                    else
                    {
                        Document doc = new Document();
                        if (doc.Open(mapPath) > 0)
                        {
                            if (doc.GetMaps() != null && doc.GetMaps().Count > 0)
                            {
                                Map Map = doc.GetMaps().GetMap(0);
                                if (Map.LayerCount > 0)
                                {
                                    for (int i = Map.LayerCount - 1; i >= 0; i--)
                                    {
                                        MapLayer layer = Map.get_Layer(i);
                                        if (layer is RasterLayer)
                                        {
                                            string name = Path.GetFileNameWithoutExtension(layer.URL);
                                            if (name.Equals(fileName))
                                            {
                                                Map.Remove(layer);
                                            }
                                        }
                                       
                                    }

                                }
                                RasterLayer cacheLayer = new RasterLayer();
                                cacheLayer.URL = "FILE:///" + mpjPath;
                                cacheLayer.Name = fileName;
                                try
                                {
                                    if (cacheLayer.IsValid)
                                    {
                                        if (Map.LayerCount > 0)
                                            Map.Insert(0, cacheLayer);
                                        else
                                            Map.Append(cacheLayer);
                                    }
                                }
                                catch { }
                            }
                            doc.Close(true);
                        }
                    }
                }
                rceManage = new ResourceManage(this.mpjPath, dataInfo);
                return 1;
            }
            this.mpjPath = null;
            return 0;
        }

        private DataBase AttachGDB(string fileName)
        {
            bool isAttach = false;
            string strHDF = hdfPath + "@GUID";
            int gdbId = ser.GetDBID(fileName);
            if (gdbId <= 0)
            {

                if (ser.AttachGDB(fileName, strHDF, null) > 0)
                    isAttach = true;
            }
            else
            {
                GDBSysInfo info = ser.GetGDBSysInfo(fileName);
                if (string.Compare(info.DataFilePath, hdfPath, true) == 0)
                {
                    isAttach = true;
                }
                else
                {
                    if (ser.DetachGDB(fileName))
                    {
                        if (ser.AttachGDB(fileName, strHDF, null) > 0)
                            isAttach = true;
                    }
                }
            }
            if (isAttach)
                gdb = ser.OpenGDB(fileName);
            return gdb;
        }
       
       
        public DataBase OpenGdb()
        {
            DataBase rtn = null;
            if (string.IsNullOrWhiteSpace(mpjPath) || !File.Exists(mpjPath))
                return rtn;
            string hdfName = Path.GetFileNameWithoutExtension(mpjPath);
            if (ser == null)
                ser = new Server();
            if (!ser.HasConnect)
            {
                if (!ser.Connect("MapGISLocal", "", ""))
                    return null;
            }
            if (string.IsNullOrWhiteSpace(hdfPath))
            {
                string dirPath = Path.GetDirectoryName(mpjPath);
                if (string.IsNullOrWhiteSpace(dirPath))
                    return null;
                hdfPath = Path.Combine(dirPath, "资源", hdfName + ".hdf");
            }
            if (File.Exists(hdfPath))
            {
                rtn = AttachGDB(hdfName);
                if (rtn == null)
                    return null;
            }
            else
                rtn = StaticFunctions.CreateDataBase(ser, hdfPath);
            return rtn;
        }


        public int UpDateTreeItem(TreeItemInfo treeInfo)
        {
            if (treeInfo == null)
                return 0;
            return GetTreeInfo(treeInfo.Path, ref treeInfo, treeInfo.ParentNode);
        }

        private DataInfo ReadDataInfo(string mcjPath)
        {
            if (string.IsNullOrWhiteSpace(mcjPath) || !File.Exists(mcjPath))
                return null;
            string info = "";

            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(mcjPath, System.IO.FileMode.Open);
                sr = new StreamReader(fs);
                info = sr.ReadToEnd();
            }
            catch { }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (fs != null)
                    fs.Close();
            }
            if (info != null && info.Length > 0)
            {
                JObject jo = JObject.Parse(info);
                IEnumerable<JProperty> properties = jo.Properties();
                DataInfo datainfo = new DataInfo();
                foreach (JProperty item in properties)
                {
                    switch (item.Name)
                    {
                        case "asset":
                            string value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                                datainfo.Asset = value;
                            break;

                        case "version":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                                datainfo.Version = value;

                            break;

                        case "dataName":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                                datainfo.DataName = value;
                            break;

                        case "guid":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                try
                                {
                                    datainfo.Guid = new Guid(value);
                                }
                                catch { }
                            }
                            break;

                        case "compressType":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                datainfo.CompressType = value;
                            }
                            break;

                        case "spatialReference":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                datainfo.SpatialReference = value;
                            }
                            break;

                        case "treeType":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                datainfo.TreeType = value;
                            }
                            break;

                        case "lodType":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                if ("Add".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                    datainfo.LodType = LodType.ADD;
                                else
                                    datainfo.LodType = LodType.REPLACE;
                            }
                            else
                                datainfo.LodType = LodType.REPLACE;
                            break;

                        

                        

                        case "rootNode":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                string url = GetUrl(mcjPath, value);
                                if (!string.IsNullOrWhiteSpace(url) && File.Exists(url))
                                {
                                    TreeItemInfo treeInfo = new TreeItemInfo();

                                    if (GetTreeInfo(url, ref treeInfo) > 0)
                                        datainfo.TreeInfo = treeInfo;
                                }
                            }
                            break;
                    }
                }
                return datainfo;
            }
            return null;
        }

        private int GetTreeInfo(string filePath, ref TreeItemInfo treeInfo, TreeItemInfo parItem = null)
        {
            if (treeInfo == null)
                return 0;

            treeInfo.Name = null;
            treeInfo.Path = null;
            treeInfo.ParentNode = null;
            treeInfo.ChildrenNode = null;
            treeInfo.TileDataInfoIndex = -1;
            if (string.IsNullOrWhiteSpace(filePath))
                return 0;
            string info = "";
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(filePath, System.IO.FileMode.Open);
                sr = new StreamReader(fs);
                info = sr.ReadToEnd();
            }
            catch { }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (fs != null)
                    fs.Close();
            }
            if (info != null && info.Length > 0)
            {
                JObject jo = JObject.Parse(info);
                IEnumerable<JProperty> properties = jo.Properties();
                treeInfo.TileDataInfoIndex = -1;
                treeInfo.Path = filePath;
                if (parItem != null)
                    treeInfo.ParentNode = parItem;
                foreach (JProperty item in properties)
                {
                    switch (item.Name)
                    {
                        case "name":
                            string value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                                treeInfo.Name = value;
                            break;

                        

                        case "lodMode":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                if ("pixel".Equals(value, StringComparison.InvariantCulture))
                                    treeInfo.LodMode = LodMode.pixel;
                                else
                                    treeInfo.LodMode = LodMode.distance;
                            }
                            break;

                        case "lodType":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                if ("REPLACE".Equals(value, StringComparison.InvariantCulture))
                                    treeInfo.LodType = LodType.REPLACE;
                                else
                                    treeInfo.LodType = LodType.ADD;
                            }
                            break;

                        case "lodError":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                float val = 0;
                                if (float.TryParse(value, out val))
                                    treeInfo.LodError = val;
                            }
                            break;

                        case "childrenNode":

                            if (item.Value is JArray)
                            {
                                JArray array = item.Value as JArray;
                                if (array != null && array.Count > 0)
                                {
                                    for (int i = 0; i < array.Count; i++)
                                    {
                                        JToken jToken = array[i];
                                        if (jToken != null)
                                        {
                                            value = GetValue(jToken);
                                            if (!string.IsNullOrWhiteSpace(value))
                                            {
                                                TreeItemInfo itemInfo = new TreeItemInfo();
                                                string url = GetUrl(filePath, value);
                                                if (!string.IsNullOrWhiteSpace(url) && File.Exists(url))
                                                {
                                                    if (GetTreeInfo(url, ref itemInfo, treeInfo) > 0)
                                                    {
                                                        if (treeInfo.ChildrenNode == null)
                                                            treeInfo.ChildrenNode = new List<TreeItemInfo>();
                                                        treeInfo.ChildrenNode.Add(itemInfo);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case "tileDataInfoIndex":
                            value = GetValue(item.Value);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                int val = 0;
                                if (int.TryParse(value, out val))
                                    treeInfo.LodError = val;
                                treeInfo.TileDataInfoIndex = val;
                            }
                            break;

                        case "tileDataInfoList":
                            if (item.Value is JArray)
                            {
                                JArray array = item.Value as JArray;
                                if (array != null && array.Count > 0)
                                {
                                    for (int i = 0; i < array.Count; i++)
                                    {
                                        JToken jToken = array[i];
                                        TileDataInfo tileInfo = null;
                                        if (jToken != null)
                                        {
                                            value = GetValue(jToken);
                                            if (!string.IsNullOrWhiteSpace(value))
                                            {
                                                tileInfo = GetTileDataInfo(filePath, value);
                                            }
                                        }
                                        if (tileInfo != null)
                                        {
                                            if (treeInfo.TileDataInfoList == null)
                                                treeInfo.TileDataInfoList = new List<TileDataInfo>();
                                            treeInfo.TileDataInfoList.Add(tileInfo);
                                        }
                                    }
                                }
                            }
                            break;

                        case "extend":

                            if (item.Value is JArray)
                            {
                                JArray array = item.Value as JArray;
                                if (array != null && array.Count > 0)
                                {
                                    for (int i = 0; i < array.Count; i++)
                                    {
                                        JToken jToken = array[i];
                                        if (jToken is JObject)
                                        {
                                            JObject extendJo = JObject.Parse(GetValue(jToken));
                                            IEnumerable<JProperty> exEn = extendJo.Properties();
                                            foreach (JProperty jp in exEn)
                                            {
                                                switch (jp.Name)
                                                {
                                                    case "MAP_TYPE":
                                                        string MapType = GetValue(jp.Value);
                                                        if (!string.IsNullOrWhiteSpace(MapType))
                                                            treeInfo.MapType = MapType;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                return 1;
            }
            return 0;
        }

        private TileDataInfo GetTileDataInfo(string filePath, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            JObject jo = JObject.Parse(value);
            IEnumerable<JProperty> properties = jo.Properties();
            TileDataInfo itemInfo = new TileDataInfo();
            foreach (JProperty item in properties)
            {
                switch (item.Name)
                {
                    case "tileData":
                        value = GetValue(item.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            string url = GetUrl(filePath, value);
                            itemInfo.Path = url;
                        }
                        break;

                    case "geometry":
                        value = GetValue(item.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            Geometry geometry = GetGeometryInfo(filePath, value);
                            if (geometry != null)
                                itemInfo.Geometry = geometry;
                        }
                        break;

                    case "texture":
                        value = GetValue(item.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            string url = GetUrl(filePath, value);
                            itemInfo.TexturePath = url;
                        }
                        break;

                    case "attribute":
                        value = GetValue(item.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            Attribute attribute = GetAttributeInfo(filePath, value);
                            if (attribute != null)
                                itemInfo.Attribute = attribute;
                        }
                        break;

                    case "dataType":
                        value = GetValue(item.Value);
                        itemInfo.DataType = value;
                        break;
                }
            }
            return itemInfo;
        }

        private string GetUrl(string filePath, string value)
        {
            JObject iobject = JObject.Parse(value);
            IEnumerable<JProperty> ipro = iobject.Properties();
            string url = "";
            foreach (JProperty pro in ipro)
            {
                switch (pro.Name)
                {
                    case "uri":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (Path.IsPathRooted(value))
                                url = value;
                            else
                            {
                                string dirPath = Path.GetDirectoryName(filePath);
                                url = Path.Combine(dirPath, value);
                            }
                        }
                        break;
                }
                if (!string.IsNullOrWhiteSpace(url))
                    break;
            }
            return url;
        }

        private Attribute GetAttributeInfo(string filePath, string value)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(value))
                return null;
            JObject jo = JObject.Parse(value);
            IEnumerable<JProperty> propertys = jo.Properties();
            Attribute rtn = new Attribute();
            foreach (JProperty pro in propertys)
            {
                switch (pro.Name)
                {
                    case "attType":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if ("bin".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.AttType = AttType.bin;
                            else
                                rtn.AttType = AttType.json;
                        }
                        break;

                    case "uri":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (Path.IsPathRooted(value))
                                rtn.Path = value;
                            else
                            {
                                string dirPath = Path.GetDirectoryName(filePath);
                                rtn.Path = Path.Combine(dirPath, value);
                            }
                        }
                        break;
                }
            }
            return rtn;
        }

        private Geometry GetGeometryInfo(string filePath, string value)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(value))
                return null;
            JObject jo = JObject.Parse(value);
            IEnumerable<JProperty> propertys = jo.Properties();
            Geometry rtn = new Geometry();
            foreach (JProperty pro in propertys)
            {
                switch (pro.Name)
                {
                    case "geometry":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if ("glb".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.BlobType = BlobType.glb;
                            else
                                rtn.BlobType = BlobType.glbx;
                        }
                        break;

                    case "uri":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (Path.IsPathRooted(value))
                                rtn.Path = value;
                            else
                            {
                                string dirPath = Path.GetDirectoryName(filePath);
                                rtn.Path = Path.Combine(dirPath, value);
                            }
                        }
                        break;

                    case "geometryType":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if ("Surface".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.GeometryType = GeometryType.Surface;
                            else if ("Point".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.GeometryType = GeometryType.Point;
                            else if ("Line".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.GeometryType = GeometryType.Line;
                            else if ("Polygon".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.GeometryType = GeometryType.Polygon;
                            else if ("Entity".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                                rtn.GeometryType = GeometryType.Entity;
                        }
                        break;

                    case "geoCompressType":
                        value = GetValue(pro.Value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            rtn.GeoCompressType = value;
                        }
                        break;
                }
            }
            return rtn;
        }

     

  
        private string GetValue(JToken jToken)
        {
            if (jToken == null)
                return null;
            if (jToken is JValue && (jToken as JValue).Value != null)
                return (jToken as JValue).Value.ToString();
            else if (jToken is JValue)
                return "";
            return jToken.ToString();
        }

        public int Close()
        {
            if (gdb != null)
                gdb.Close();
            if (!string.IsNullOrWhiteSpace(hdfPath) && ser != null && ser.HasConnect)
            {
                string name = Path.GetFileNameWithoutExtension(hdfPath);
                if (ser.GetDBID(name) > 0)
                {
                    ser.DetachGDB(name);
                }
                ser.DisConnect();
            }
            gdb = null;
            ser = null;
            dataInfo = null;
            mpjPath = "";
            rceManage = null;
            mapPath = "";
            hdfPath = "";
            return 1;
        }
    }

    public class M3DProjectMag : ProjectManageItem
    {
        private Document doc;
        private IApplication app;
        private Project project;
        private bool opening = false;
        private List<Map> previewMap;

        public override event ClosingProjectHandle ClosingProject;

        public override event ClosedProjectHandle ClosedProject;

        public override event OpeningProjectHandle OpeningProject;

        public override event OpenedProjectHandle OpenedProject;

        public override Document Doc
        {
            get { return doc; }
        }

        public override bool IsOpen
        {
            get { return project != null; }
        }

        public override object OpenPrj
        {
            get { return project; }
        }

        public M3DProjectMag(IApplication app)
        {
            if (app != null)
            {
                this.app = app;
                this.doc = app.Document;
                this.app.ApplicationClosingEvent += App_ApplicationClosingEvent;
            }
            if (this.doc != null)
            {
                doc.OpenedDocument -= Doc_OpenedDocument;
                doc.ClosingDocument -= Doc_ClosingDocument;
                doc.ClosedDocument -= Doc_ClosedDocument;
                doc.SavedDocument -= Doc_SavedDocument;
                doc.OpenedDocument += Doc_OpenedDocument;
                doc.ClosingDocument += Doc_ClosingDocument;
                doc.ClosedDocument += Doc_ClosedDocument;
                doc.SavedDocument += Doc_SavedDocument;
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(WaitDocument));
                thread.SetApartmentState(ApartmentState.STA);
                thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                thread.Start();
            }
        }

        public override int OpenProject(object prjParam)
        {
            int rtn = 0;
            if (prjParam != null && prjParam is string)
            {
                bool isClose = CloseProject() > 0;
                if (isClose)
                {
                    Project prj = new Project();
                    rtn = prj.Open(prjParam as string);
                    if (rtn > 0)
                    {
                        bool isCancel = false;
                        opening = true;
                        try
                        {
                            OpeningProject?.Invoke(this, prj, isCancel);
                            if (!isCancel)
                            {
                                project = prj;
                                rtn = OpenDocument(prj);
                                if (rtn <= 0)
                                {
                                    prj.Close();
                                    project = null;
                                }
                                else
                                {
                                    OpenedProject?.Invoke(this, project);
                                }
                            }
                            else
                            {
                                prj.Close();
                            }
                        }
                        catch { }
                        finally
                        {
                            opening = false;
                        }
                    }
                    else
                    {
                        project = null;
                    }
                }
            }
            return rtn;
        }

        public override int CloseProject()
        {
            bool rtn = true;
            if (IsOpen && doc != null && !opening)
            {
                bool isCancel = false;
                ClosingProject?.Invoke(this, project, isCancel);
                if (!isCancel)
                {
                    if (this.app.Document.Handle == doc.Handle)
                        this.app.WorkSpaceEngine.BeginUpdateTree();
                    if (doc != null)
                    {
                        if (!string.IsNullOrWhiteSpace(doc.GetFilePath()))
                            doc.Close(true);
                        else
                            doc.Close(false);
                    }
                    if (this.app.Document.Handle == doc.Handle)
                        this.app.WorkSpaceEngine.EndUpdateTree();
                    project.Close();
                    ClosedProject?.Invoke(this);
                    project = null;
                }
            }
            return rtn ? 1 : -1;
        }

        private void WaitDocument(object obj)
        {
            if (this.app != null)
            {
                while (this.app.Document == null)
                    Thread.Sleep(100);
                this.doc = app.Document;
                doc.OpenedDocument -= Doc_OpenedDocument;
                doc.ClosingDocument -= Doc_ClosingDocument;
                doc.ClosedDocument -= Doc_ClosedDocument;
                doc.SavedDocument -= Doc_SavedDocument;
                doc.OpenedDocument += Doc_OpenedDocument;
                doc.ClosingDocument += Doc_ClosingDocument;
                doc.ClosedDocument += Doc_ClosedDocument;
                doc.SavedDocument += Doc_SavedDocument;
            }
        }

        private void Doc_ClosedDocument(object sender, EventArgs e)
        {
        }

        private void Doc_ClosingDocument(object sender, ClosingDocumentEventArgs e)
        {
            if (IsOpen)
            {
                if (doc.IsDirty)
                    doc.ClearDirty();
            }
        }

        private void Doc_SavedDocument(object sender, EventArgs e)
        {
            if (IsOpen && doc != null)
            {
            }
        }

        private void Doc_OpenedDocument(object sender, EventArgs e)
        {
            if (!IsOpen)
                return;

            if (previewMap == null)
                previewMap = new List<Map>();
            previewMap.Clear();
            if (this.doc != null)
            {
                Maps Maps = this.doc.GetMaps();

                if (Maps != null)
                {
                    for (int i = 0; i < Maps.Count; i++)
                    {
                        Map Map = Maps.GetMap(i);
                        if (Map != null)
                        {
                            string value = Map.GetPropertyEx("InitOpenView");
                            if ((!string.IsNullOrEmpty(value) && string.Compare(value, "true", true) == 0))
                            {
                                previewMap.Add(Map);
                                Map.SetPropertyEx("InitOpenView", "false");
                            }
                            //map.SetPropertyEx("Expand", "false");
                        }
                    }
                    //保证初始默认打开视图
                    if (previewMap.Count == 0)
                    {
                        Map Map = Maps.GetMap(0);
                        if (Map != null)
                            previewMap.Add(Map);
                    }
                }
            }
            if (app != null)
            {
                IDockWindow dw = null;
                app.PluginContainer.DockWindows.TryGetValue(typeof(ProjectTreeDockWindow).ToString(), out dw);
                if (dw == null)
                    dw = app.PluginContainer.CreateDockWindow(typeof(ProjectTreeDockWindow).ToString());
                if (dw != null)
                    app.PluginContainer.ActiveDockWindow(dw);
            }
        }

        private void App_ApplicationClosingEvent(ApplicationClosingEventArgs args)
        {
            if (IsOpen && CloseProject() <= 0)
            {
                args.Cancel = true;
            }
        }

        private int OpenDocument(Project prj)
        {
            if (prj == null)
                return 0;
            string filePath = prj.MpjPath;
            int rtn = 0;
            if (doc != null && !String.IsNullOrWhiteSpace(prj.MapPath))
            {
                if (this.app.Document.Handle == doc.Handle)
                    this.app.WorkSpaceEngine.BeginUpdateTree();
                doc.Open(prj.MapPath);
                if (this.app.Document.Handle == doc.Handle)
                    this.app.WorkSpaceEngine.EndUpdateTree();

                if (previewMap != null && previewMap.Count > 0)
                {
                    if (previewMap.Count > 1)
                    {
                        for (int i = previewMap.Count - 1; i > 0; i--)
                        {
                            previewMap.RemoveAt(i);
                        }
                    }
                    StaticFunctions.MapViewPreview(app, previewMap[0]);
                }
                if (previewMap != null)
                    previewMap.Clear();
                rtn = 1;
            }
            return rtn;
        }
        public override object GetExpandInfo(string key)
        {
            return null;
        }
    }
}