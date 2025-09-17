using MapGIS.GeoObjects.Geometry;
using System;
using System.Collections.Generic;

namespace MapGISPlugin3
{
    /// <summary>
    /// LOD类型
    /// </summary>
    public enum LodType
    {
        /// <summary>
        /// 添加层次细节
        /// </summary>
        ADD = 0,

        /// <summary>
        /// 替换层次细节
        /// </summary>
        REPLACE = 1,
    }

    /// <summary>
    /// LOD切换模式
    /// </summary>
    public enum LodMode
    {
        /// <summary>
        /// 距离切换模式
        /// </summary>
        distance = 0,

        /// <summary>
        /// 像素切换模式
        /// </summary>
        pixel = 1,
    }

    /// <summary>
    /// 几何数据类型
    /// </summary>
    public enum BlobType
    {
        /// <summary>
        /// glb
        /// </summary>
        glb = 0,

        /// <summary>
        /// glbx
        /// </summary>
        glbx = 1,
    }

    /// <summary>
    /// 几何类型
    /// </summary>
    public enum GeometryType
    {
        /// <summary>
        /// 点
        /// </summary>
        Point = 0,

        /// <summary>
        /// 线
        /// </summary>
        Line = 1,

        /// <summary>
        /// 区
        /// </summary>
        Polygon = 2,

        /// <summary>
        /// 面
        /// </summary>
        Surface = 3,

        /// <summary>
        /// 实体
        /// </summary>
        Entity = 4,
    }

    /// <summary>
    /// 属性数据类型
    /// </summary>
    public enum AttType
    {
        /// <summary>
        /// json
        /// </summary>
        json = 0,

        /// <summary>
        /// bin
        /// </summary>
        bin = 1,
    }

    /// <summary>
    /// 数据类型
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 矢量
        /// </summary>
        Vector = 0,

        /// <summary>
        /// 倾斜
        /// </summary>
        TiltPhotography = 1,

        /// <summary>
        /// 模型
        /// </summary>
        Model = 2,

        /// <summary>
        /// BIM
        /// </summary>
        BIM = 3,

        /// <summary>
        /// 点云
        /// </summary>
        PointCloud = 4,

        /// <summary>
        /// 管线
        /// </summary>
        PipeLine = 5,

        /// <summary>
        /// 地质体
        /// </summary>
        GeoModel = 6,

        /// <summary>
        /// 地质体网格
        /// </summary>
        GeoGrid = 7,

        /// <summary>
        /// 地质钻孔
        /// </summary>
        GeoDrill = 8,

        /// <summary>
        /// 地质剖面
        /// </summary>
        GeoSection = 9,

        /// <summary>
        /// OSGB
        /// </summary>
        OSGB = 10,
    }

    /// <summary>
    /// 外包
    /// </summary>
    public class Bounding
    {
        /// <summary>
        /// 数据外包盒左边界的经度值
        /// </summary>
        public double Left
        {
            get;
            set;
        }

        /// <summary>
        /// 数据外包盒上边界的纬度值
        /// </summary>
        public double Top
        {
            get;
            set;
        }

        /// <summary>
        /// 数据外包盒右边界的经度值
        /// </summary>
        public double Right
        {
            get;
            set;
        }

        /// <summary>
        /// 数据外包盒下边界的纬度值
        /// </summary>
        public double Bottom
        {
            get;
            set;
        }

        /// <summary>
        /// 数据外包盒高度的最小值
        /// </summary>
        public double MinHeight
        {
            get;
            set;
        }

        /// <summary>
        /// 数据外包盒高度的最大值
        /// </summary>
        public double MaxHeight
        {
            get;
            set;
        }

        /// <summary>
        /// 数据外包类型
        /// 0 数据外包盒
        /// 1 数据外包球
        /// </summary>
        public int Type
        {
            get;
            set;
        }

        /// <summary>
        /// 包围球中心点
        /// </summary>
        public Dot3D Center
        {
            get;
            set;
        }

        /// <summary>
        /// 包围球半径
        /// </summary>
        public double Radius
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 集合参数
    /// </summary>
    public class Geometry
    {
        /// <summary>
        /// 几何数据类型
        /// </summary>
        public BlobType BlobType
        {
            get;
            set;
        }

        /// <summary>
        /// 几何文件路径
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// 几何数据的压缩类型
        /// </summary>
        public string GeoCompressType
        {
            get;
            set;
        }

        /// <summary>
        /// 几何类型
        /// </summary>
        public GeometryType GeometryType
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 属性参数
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// 属性类型
        /// </summary>
        public AttType AttType
        {
            get;
            set;
        }

        /// <summary>
        /// 属性文件坐标
        /// </summary>
        public string Path
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 瓦片信息
    /// </summary>
    public class TileDataInfo
    {
        /// <summary>
        /// 瓦片文件路径
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        public string Nmae
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Path))
                    return "";
                else
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
            }
        }

        /// <summary>
        /// 几何信息
        /// </summary>
        public Geometry Geometry
        {
            get;
            set;
        }

        /// <summary>
        /// 属性信息
        /// </summary>
        public Attribute Attribute
        {
            get;
            set;
        }

        /// <summary>
        /// 纹理图片数据路径
        /// </summary>
        public string TexturePath
        {
            get;
            set;
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType
        {
            get;
            set;
        }
    }

    public class TreeItemInfo
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// lod切换模式
        /// </summary>
        public LodMode LodMode
        {
            get;
            set;
        }

        /// <summary>
        /// LOD类型
        /// </summary>
        public LodType LodType
        {
            get;
            set;
        }

        /// <summary>
        /// 外包
        /// </summary>
        public Bounding Bounding
        {
            get;
            set;
        }

        /// <summary>
        /// LOD切换误差值 其单位与切换模式对应，距离切换时单位为米，像素切换时单位为像素
        /// </summary>
        public float LodError
        {
            get;
            set;
        }

        /// <summary>
        /// 父节点
        /// </summary>
        public TreeItemInfo ParentNode
        {
            get;
            set;
        }


        /// <summary>
        /// 场景类型
        /// </summary>
        public string MapType
        {
            get { return MapType; }
            set { MapType = value; }
        }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<TreeItemInfo> ChildrenNode
        {
            get;
            set;
        }

        /// <summary>
        /// 当前显示模式
        /// </summary>
        public int TileDataInfoIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 瓦片信息
        /// </summary>
        public List<TileDataInfo> TileDataInfoList
        {
            get;
            set;
        }
    }

    public class DataInfo
    {
        /// <summary>
        /// 数据基本信息，如数据所有者
        /// </summary>
        public string Asset
        {
            get;
            set;
        }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// 数据名
        /// </summary>
        public string DataName
        {
            get;
            set;
        }

        /// <summary>
        /// 数据唯一标志符
        /// </summary>
        public Guid Guid
        {
            get;
            set;
        }

        /// <summary>
        /// 文件压缩类型
        /// </summary>
        public string CompressType
        {
            get;
            set;
        }

        /// <summary>
        /// 空间坐标参考系，wkt、wkid格式
        /// </summary>
        public string SpatialReference
        {
            get;
            set;
        }

        /// <summary>
        /// 树形组织结构类型，取值QuadTree|OCTree|K-DTree|RTree
        /// </summary>
        public string TreeType
        {
            get;
            set;
        }

        /// <summary>
        /// LOD类型，Add|Replace
        /// </summary>
        public LodType LodType
        {
            get;
            set;
        }

        /// <summary>
        /// 外包
        /// </summary>
        public Bounding BoundingBox
        {
            get;
            set;
        }

        /// <summary>
        /// 坐标点
        /// </summary>
        public Dot3D Position
        {
            get;
            set;
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// 树节点信息
        /// </summary>
        public TreeItemInfo TreeInfo
        {
            get;
            set;
        }
    }
}