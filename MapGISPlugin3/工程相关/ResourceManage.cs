using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MapGISPlugin3
{
    public class ResourceManage
    {
        private string mcjPath;
        private DataInfo dataInfo = null;
        private Dictionary<string, string> dicPath = new Dictionary<string, string>();
        private string rootPath = "";
        private string rceRootPath = "";

        public string RceRootPath
        {
            get
            {
                return rceRootPath;
            }
        }

        public ResourceManage(string mcjPath, DataInfo info)
        {
            this.mcjPath = mcjPath;
            this.dataInfo = info;
            if (!string.IsNullOrWhiteSpace(mcjPath) && File.Exists(mcjPath))
            {
                rootPath = Path.GetDirectoryName(mcjPath);
                if (!string.IsNullOrWhiteSpace(rootPath))
                    rceRootPath = Path.Combine(rootPath, "资源");
            }
            ReadInfo();
        }

        /// <summary>
        /// 获取节点资源信息
        /// </summary>
        /// <param name="TreeInfo"></param>
        /// <returns></returns>
        public string GetResourcePath(TreeItemInfo treeInfo)
        {
            string key = GetKey(treeInfo);
            if (string.IsNullOrWhiteSpace(key))
                return "";
            if (dicPath.ContainsKey(key))
                return Path.Combine(rceRootPath, dicPath[key]);
            else
            {
                string guid = Guid.NewGuid().ToString();
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    string rtn = Path.Combine(rceRootPath, guid);
                    CreateResourceDir(rtn);
                    dicPath.Add(key, guid);
                    WriteInfo();
                    return rtn;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取节点资源信息
        /// </summary>
        /// <param name="TreeInfo"></param>
        /// <returns></returns>
        //public string GetResourcePath(TreeItemInfo treeInfo, string SceneType)
        //{
        //    string dirPath = GetResourcePath(treeInfo);
        //    if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
        //        return null;
        //    if (SceneType == StaticFunctions.OUTDOOR)
        //        return Path.Combine(dirPath, "室外");
        //    else if (SceneType == StaticFunctions.INDOOR)
        //        return Path.Combine(dirPath, "室内");
        //    else if (SceneType == StaticFunctions.LAND_WATER)
        //        return Path.Combine(dirPath, "水陆");
        //    else if (SceneType == StaticFunctions.UNDERGROUD)
        //        return Path.Combine(dirPath, "地下");

        //    return null;
        //}

        /// <summary>
        /// 获取节点资源信息
        /// </summary>
        /// <param name="TreeInfo"></param>
        /// <returns></returns>
        //public string GetParamFilePath(TreeItemInfo treeInfo, string SceneType)
        //{
        //    string dirPath = GetResourcePath(treeInfo);
        //    if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
        //        return null;
        //    if (SceneType == StaticFunctions.OUTDOOR)
        //        return Path.Combine(dirPath, "室外", "param.xml");
        //    else if (SceneType == StaticFunctions.INDOOR)
        //        return Path.Combine(dirPath, "室内", "param.xml");
        //    //else if (SceneType == StaticFunctions.LAND_WATER)
        //    //    return Path.Combine(dirPath, "水陆", "param.xml");
        //    else if (SceneType == StaticFunctions.UNDERGROUD)
        //        return Path.Combine(dirPath, "地下", "param.xml");
        //    return null;
        //}



        public void RemoveResourcePath(TreeItemInfo treeInfo)
        {
            string key = GetKey(treeInfo);
            if (string.IsNullOrWhiteSpace(key))
                return;
            if (dicPath.ContainsKey(key))
            {
                string dirPath = Path.Combine(rceRootPath, dicPath[key]);
                DeleteResourceDir(dirPath);
                dicPath.Remove(key);
                WriteInfo();
            }
        }

        private void DeleteResourceDir(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
                return;
            try
            {
                if (!Directory.Exists(dirPath))
                    Directory.Delete(dirPath, true);
            }
            catch { }
        }

        private void CreateResourceDir(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
                return;
            try
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                if (!Directory.Exists(Path.Combine(dirPath, "室外")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室外"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "RGBD")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "RGBD"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "RGBD", "image")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "RGBD", "image"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "RGBD", "depth")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "RGBD", "depth"));
                if (!Directory.Exists(Path.Combine(dirPath, "室内", "视觉")))
                    Directory.CreateDirectory(Path.Combine(dirPath, "室内", "视觉"));
                //if (!Directory.Exists(Path.Combine(dirPath, "水陆")))
                //    Directory.CreateDirectory(Path.Combine(dirPath, "水陆"));
                //if (!Directory.Exists(Path.Combine(dirPath, "地下")))
                //    Directory.CreateDirectory(Path.Combine(dirPath, "地下"));
            }
            catch { }
        }

        private string GetKey(TreeItemInfo treeInfo)
        {
            if (treeInfo == null)
                return "";
            string filePath = treeInfo.Path;
            if (string.IsNullOrWhiteSpace(filePath))
                return "";
            if (filePath.StartsWith(rootPath, StringComparison.InvariantCultureIgnoreCase))
            {
                string key = filePath.Substring(rootPath.Length + 1);
                if (!string.IsNullOrWhiteSpace(key))
                    return key.ToLower();
            }
            return filePath.ToLower();
        }

        private void ReadInfo()
        {
            if (dicPath == null)
                dicPath = new Dictionary<string, string>();
            dicPath.Clear();
            if (string.IsNullOrWhiteSpace(rceRootPath))
                return;
            string filePath = Path.Combine(rceRootPath, "ResourcesList.xml");
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(filePath);
            }
            catch { doc = null; }
            if (doc == null)
                return;
            XmlNodeList itemNodes = doc.SelectNodes("root/item");
            if (itemNodes == null || itemNodes.Count <= 0)
                return;

            for (int i = 0; i < itemNodes.Count; i++)
            {
                XmlNode node = itemNodes[i];
                if (node != null && node is XmlElement)
                {
                    XmlElement xmlEle = node as XmlElement;
                    string key = xmlEle.GetAttribute("key");
                    string value = xmlEle.GetAttribute("value");

                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                        continue;
                    if (dicPath.ContainsKey(key))
                        continue;
                    dicPath.Add(key, value);
                }
            }
        }

        private void WriteInfo()
        {
            if (string.IsNullOrWhiteSpace(rceRootPath))
                return;
            string filePath = Path.Combine(rceRootPath, "ResourcesList.xml");

            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                string xmlContent = "<?xml version=\"1.0\" encoding=\"gb2312\" ?><root></root>";
                doc.LoadXml(xmlContent);
            }
            catch { doc = null; }
            if (doc == null)
                return;
            if (dicPath != null && dicPath.Count > 0)
            {
                XmlNode rootNode = doc.SelectSingleNode("root");
                foreach (string key in dicPath.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(dicPath[key]))
                        continue;
                    //增加方案节点
                    XmlElement xEle = doc.CreateElement("item");
                    xEle.SetAttribute("key", key);
                    xEle.SetAttribute("value", dicPath[key]);
                    rootNode.AppendChild(xEle);
                }
            }
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                doc.Save(filePath);
            }
            catch { }
        }
    }
}