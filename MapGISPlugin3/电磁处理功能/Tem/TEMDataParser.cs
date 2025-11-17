using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 数据解析器
    /// 
    /// 【新增】独立的数据解析工具类
    /// 
    /// 职责：
    /// 1. 读取和解析 knowed.dat
    /// 2. 读取和解析 tran.dat
    /// 3. 验证数据格式
    /// 4. 数据类型转换
    ///
    /// 使用场景：
    /// 在 Form_TEMImport.btnOK_Click() 中调用
    /// 提高代码可维护性
    /// </summary>
    public class TEMDataParser
    {
        /// <summary>
        /// 解析观测数据文件
        /// </summary>
        public static List<TEMObservationData> ParseObservationFile(string filePath)
        {
            var result = new List<TEMObservationData>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    // 跳过标题行
                    if (lineNumber == 1 && IsHeaderLine(line))
                        continue;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var data = ParseObservationLine(line);
                        if (data != null)
                            result.Add(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"第 {lineNumber} 行解析失败: {ex.Message}");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 解析发射源文件
        /// </summary>
        public static TEMTransmitterInfo ParseTransmitterFile(string filePath)
        {
            var lines = new List<string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }

            if (lines.Count < 4)
                throw new Exception("发射源文件必须包含至少 4 行（ABCD 四个点）");

            try
            {
                var info = new TEMTransmitterInfo();

                var partsA = Regex.Split(lines[0].Trim(), @"\s+");
                var partsB = Regex.Split(lines[1].Trim(), @"\s+");
                var partsC = Regex.Split(lines[2].Trim(), @"\s+");
                var partsD = Regex.Split(lines[3].Trim(), @"\s+");

                if (partsA.Length >= 3 && partsB.Length >= 3 && partsC.Length >= 3 && partsD.Length >= 3)
                {
                    info.PointA_X = double.Parse(partsA[0], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointA_Y = double.Parse(partsA[1], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointA_Z = double.Parse(partsA[2], System.Globalization.CultureInfo.InvariantCulture);

                    info.PointB_X = double.Parse(partsB[0], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointB_Y = double.Parse(partsB[1], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointB_Z = double.Parse(partsB[2], System.Globalization.CultureInfo.InvariantCulture);

                    info.PointC_X = double.Parse(partsC[0], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointC_Y = double.Parse(partsC[1], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointC_Z = double.Parse(partsC[2], System.Globalization.CultureInfo.InvariantCulture);

                    info.PointD_X = double.Parse(partsD[0], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointD_Y = double.Parse(partsD[1], System.Globalization.CultureInfo.InvariantCulture);
                    info.PointD_Z = double.Parse(partsD[2], System.Globalization.CultureInfo.InvariantCulture);

                    return info;
                }
                else
                {
                    throw new Exception("发射源文件格式错误：每行必须有至少 3 列（X, Y, Z）");
                }
            }
            catch (FormatException ex)
            {
                throw new Exception($"发射源文件数值转换失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 解析观测数据的单行
        /// </summary>
        private static TEMObservationData ParseObservationLine(string line)
        {
            string[] parts = Regex.Split(line.Trim(), @"\s+");

            if (parts.Length < 7)
                return null;

            try
            {
                return new TEMObservationData
                {
                    LineName = parts[0],
                    StationName = parts[1],
                    X = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                    Y = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                    SamplingTime_us = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture),
                    EffectiveArea = double.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture),
                    InducedVoltage_mV = double.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture)
                };
            }
            catch (FormatException ex)
            {
                throw new Exception($"数值转换失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检测是否是标题行
        /// </summary>
        private static bool IsHeaderLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string firstWord = line.Trim()
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (firstWord == null)
                return false;

            // 如果第一个单词含有非数字字符（除了小数点和科学计数法），则判为标题
            return firstWord.Any(c => !char.IsDigit(c) && c != '.' && c != '-' && c != 'e' && c != 'E');
        }
    }
}