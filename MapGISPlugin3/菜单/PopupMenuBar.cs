/*----------------------------------------------------------------
// Copyright (C) 2019 中地数码科技有限公司
// 版权所有。
//
// 文件名：MenuBar
// 文件功能描述：
//
//
// 创建标识：韩威 2019/3/13 11:15:24
//----------------------------------------------------------------*/

using MapGIS.PluginEngine;
using MapGISPlugin3;
using System;
using System.Collections.Generic;

namespace MapGISPlugin3
{
    public interface IRightMenuBar : IPlugin
    {
        //
        // 摘要:
        //     菜单栏、工具条或者状态栏的标题
        string Caption { get; }

        //
        // 摘要:
        //     菜单栏、工具条或者状态栏的名称，无实际意义
        string Name { get; }

        List<IItem> ItemList
        {
            get;
            set;
        }
    }

    public class MenuItem : Item
    {
        private Type type;

        public MenuItem(string key, Type type) : base(key)
        {
            this.type = type;
        }

        public MenuItem(string key, bool group, Type type) : base(key, group)
        {
            this.type = type;
        }

        public MenuItem(string key, bool group, bool showLargeImage, Type type) : base(key, group, showLargeImage)
        {
            this.type = type;
        }

        public Type Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
    }

    /// <summary>
    /// 打开或创建菜单
    /// </summary>
    public class OpenOrCreateMenuBar : IRightMenuBar
    {
        private List<IItem> selectList = new List<IItem>();

        public OpenOrCreateMenuBar()
        {
            selectList.AddRange(new IItem[] {
                new MenuItem(typeof(NewProjectCommand).ToString(),Type.GetType(typeof(NewProjectCommand).ToString())),//制图线选择
                new MenuItem(typeof(OpenProjectCommand).ToString(),Type.GetType(typeof(OpenProjectCommand).ToString())),//制图点选择
                new MenuItem(typeof(CloseProjectCommand).ToString(),Type.GetType(typeof(CloseProjectCommand).ToString())),//制图点选择
           });
        }

        //选择

        public string Caption
        {
            get { return "右键菜单"; }
        }

        public List<IItem> ItemList
        {
            get { return selectList; }

            set { selectList = value; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }
    }

    /// <summary>
    /// 打开或创建菜单
    /// </summary>
    public class ItemMenuBar : IRightMenuBar
    {
        private List<IItem> selectList = new List<IItem>();

        public ItemMenuBar()
        {
            selectList.AddRange(new IItem[] {
                new MenuItem(typeof(PreviewSceneCommand).ToString(),Type.GetType(typeof(PreviewSceneCommand).ToString())),
                //new MenuItem(typeof(GlobalModeCommand).ToString(),Type.GetType(typeof(GlobalModeCommand).ToString())),
                //new MenuItem(typeof(PlaneModeeCommand).ToString(),Type.GetType(typeof(PlaneModeeCommand).ToString())),
                new MenuItem(typeof(NodeVisibleCommand).ToString(),Type.GetType(typeof(NodeVisibleCommand).ToString())),
                new MenuItem(typeof(NodeUnVisibleCommand).ToString(),Type.GetType(typeof(NodeUnVisibleCommand).ToString())),
                new MenuItem(typeof(NodeEditableCommand).ToString(),Type.GetType(typeof(NodeEditableCommand).ToString())),
                new MenuItem(typeof(NodeActiveCommand).ToString(),Type.GetType(typeof(NodeActiveCommand).ToString())),

               // new MenuItem(typeof(SetCurrrentDspRangeCommand).ToString(),Type.GetType(typeof(SetCurrrentDspRangeCommand).ToString())),
               // //new MenuItem(typeof(SetLocation).ToString(),Type.GetType(typeof(SetLocation).ToString())),
               // //new MenuItem(typeof(AddModeChildCommand).ToString(),Type.GetType(typeof(AddModeChildCommand).ToString())),
               // new MenuItem(typeof(AddChildCommand).ToString(),Type.GetType(typeof(AddChildCommand).ToString())),
               // new MenuItem(typeof(InputModeChildCommand).ToString(),Type.GetType(typeof(InputModeChildCommand).ToString())),
               //// new MenuItem(typeof(ExportCommand).ToString(),Type.GetType(typeof(ExportCommand).ToString())),
               ////new MenuItem(typeof(ModelMoveCommand).ToString(),Type.GetType(typeof(ModelMoveCommand).ToString())),
               ////new MenuItem(typeof(ModelRotateCommand).ToString(),Type.GetType(typeof(ModelRotateCommand).ToString())),
               // new MenuItem(typeof(DeleteChildCommand).ToString(),Type.GetType(typeof(DeleteChildCommand).ToString())),

               // new MenuItem(typeof(RenameCommand).ToString(),Type.GetType(typeof(RenameCommand).ToString())),
               // new MenuItem(typeof(RemoveLayerCommand).ToString(),Type.GetType(typeof(RemoveLayerCommand).ToString())),
               // new MenuItem(typeof(PropertyCommand).ToString(),Type.GetType(typeof(PropertyCommand).ToString())),
               // new MenuItem(typeof(LayerPropertyCommand).ToString(),Type.GetType(typeof(LayerPropertyCommand).ToString())),
               // new MenuItem(typeof(PropertyChildCommand).ToString(),Type.GetType(typeof(PropertyChildCommand).ToString())),
            });
        }

        //选择
        public string Caption
        {
            get { return "右键菜单"; }
        }

        public List<IItem> ItemList
        {
            get { return selectList; }

            set { selectList = value; }
        }

        public string Name
        {
            get { return this.ToString(); }
        }
    }
}