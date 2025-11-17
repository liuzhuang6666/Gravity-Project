using System;

namespace MapGISPlugin3
{
    #region --- TEM 数据结构 ---

    /// <summary>
    /// TEM 观测数据行
    /// 对应 knowed.dat 文件的单行记录
    /// 
    /// 字段说明（按文件列顺序）：
    /// 第 1 列：LineName (测线号)
    /// 第 2 列：StationName (测点号)
    /// 第 3 列：X (X 坐标，相对发射框中心)
    /// 第 4 列：Y (Y 坐标，相对发射框中心)
    /// 第 5 列：SamplingTime_us (采样时窗中心时间，单位：微秒)
    /// 第 6 列：EffectiveArea (接收框有效面积)
    /// 第 7 列：InducedVoltage_mV (Z 方向感应电压，单位：mV)
    /// </summary>
    public class TEMObservationData
    {
        /// <summary>测线号</summary>
        public string LineName { get; set; }

        /// <summary>测点号</summary>
        public string StationName { get; set; }

        /// <summary>X 坐标（相对发射框中心）</summary>
        public double X { get; set; }

        /// <summary>Y 坐标（相对发射框中心）</summary>
        public double Y { get; set; }

        /// <summary>采样时窗中心时间（单位：微秒）</summary>
        public double SamplingTime_us { get; set; }

        /// <summary>接收框有效面积</summary>
        public double EffectiveArea { get; set; }

        /// <summary>Z 方向感应电压（单位：mV）</summary>
        public double InducedVoltage_mV { get; set; }
    }

    /// <summary>
    /// TEM 测点信息
    /// 表示唯一的一个测点（去重后）
    /// 
    /// 与 TEMObservationData 的区别：
    /// - TEMObservationData：多行记录（每个采样时间一条）
    /// - TEMStationInfo：单行记录（唯一的测点）
    /// 
    /// 用途：
    /// 1. 绘制小地图上的点
    /// 2. 在下拉框中显示
    /// 3. 作为键来组织观测数据
    /// </summary>
    public class TEMStationInfo
    {
        /// <summary>测线号</summary>
        public string LineName { get; set; }

        /// <summary>测点号</summary>
        public string StationName { get; set; }

        /// <summary>X 坐标</summary>
        public double X { get; set; }

        /// <summary>Y 坐标</summary>
        public double Y { get; set; }
    }

    /// <summary>
    /// TEM 发射源信息
    /// 对应 tran.dat 文件的完整信息
    /// 
    /// 发射源定义：
    /// - 由 4 个点组成：A, B, C, D
    /// - 构成矩形框
    /// - A-B 为长边（平行 X 轴）
    /// - A-C 为短边（平行 Y 轴）
    /// - 矩形中心为坐标原点 (0, 0)
    ///
    /// 文件格式（tran.dat）：
    /// 第 1 行：A 点的 X, Y, Z
    /// 第 2 行：B 点的 X, Y, Z
    /// 第 3 行：C 点的 X, Y, Z
    /// 第 4 行：D 点的 X, Y, Z
    /// </summary>
    public class TEMTransmitterInfo
    {
        // 点 A 坐标
        public double PointA_X { get; set; }
        public double PointA_Y { get; set; }
        public double PointA_Z { get; set; }

        // 点 B 坐标
        public double PointB_X { get; set; }
        public double PointB_Y { get; set; }
        public double PointB_Z { get; set; }

        // 点 C 坐标
        public double PointC_X { get; set; }
        public double PointC_Y { get; set; }
        public double PointC_Z { get; set; }

        // 点 D 坐标
        public double PointD_X { get; set; }
        public double PointD_Y { get; set; }
        public double PointD_Z { get; set; }

        /// <summary>
        /// 计算矩形框中心
        /// 在标准 TEM 应用中，中心应该是 (0, 0)
        /// </summary>
        public (double centerX, double centerY) GetCenter()
        {
            double centerX = (PointA_X + PointB_X + PointC_X + PointD_X) / 4.0;
            double centerY = (PointA_Y + PointB_Y + PointC_Y + PointD_Y) / 4.0;
            return (centerX, centerY);
        }

        /// <summary>
        /// 计算矩形框的长边长度（A-B 的距离）
        /// </summary>
        public double GetLongSideLength()
        {
            double dx = PointB_X - PointA_X;
            double dy = PointB_Y - PointA_Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算矩形框的短边长度（A-C 的距离）
        /// </summary>
        public double GetShortSideLength()
        {
            double dx = PointC_X - PointA_X;
            double dy = PointC_Y - PointA_Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    #endregion
}