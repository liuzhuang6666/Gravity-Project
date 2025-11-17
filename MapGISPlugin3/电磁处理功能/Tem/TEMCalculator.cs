using System;
using System.Diagnostics;
using System.IO;

namespace MapGISPlugin3
{
    /// <summary>
    /// TEM 计算工具
    /// 
    /// 【新增】独立的计算调度工具类
    /// 
    /// 职责：
    /// 1. 处理 a.exe 的调用
    /// 2. 参数验证
    /// 3. 进程管理
    /// 4. 结果处理
    /// </summary>
    public class TEMCalculator
    {
        private string _pluginDirectory;
        private string _exePath;

        public TEMCalculator(string pluginDirectory)
        {
            _pluginDirectory = pluginDirectory;
            _exePath = Path.Combine(pluginDirectory, "Algorithm", "TEM1di", "a.exe");
        }

        /// <summary>
        /// 执行 TEM 反演计算
        /// 
        /// 参数说明：
        /// knowed.dat - 观测数据文件路径
        /// tran.dat - 发射源信息文件路径
        /// workspace - 6 字符的工作空间名称
        /// 
        /// 调用格式：a.exe knowed.dat tran.dat workspace
        /// </summary>
        public TEMCalculationResult ExecuteCalculation(
            string knowedDataFile,
            string transmitterFile,
            string workspace)
        {
            // 验证输入
            ValidateInputs(knowedDataFile, transmitterFile, workspace);

            // 构造命令行
            var arguments = $@"""{knowedDataFile}"" ""{transmitterFile}"" ""{workspace}""";

            // 启动进程
            return ExecuteProcess(arguments, workspace);
        }

        /// <summary>
        /// 验证输入参数
        /// </summary>
        private void ValidateInputs(string knowedFile, string tranFile, string workspace)
        {
            if (!File.Exists(knowedFile))
                throw new FileNotFoundException($"观测数据文件不存在: {knowedFile}");

            if (!File.Exists(tranFile))
                throw new FileNotFoundException($"发射源文件不存在: {tranFile}");

            if (string.IsNullOrWhiteSpace(workspace))
                throw new ArgumentException("工作空间名称不能为空");

            if (workspace.Length != 6)
                throw new ArgumentException("工作空间名称必须为 6 字符");

            if (!File.Exists(_exePath))
                throw new FileNotFoundException($"计算程序不存在: {_exePath}");
        }

        /// <summary>
        /// 执行进程
        /// </summary>
        private TEMCalculationResult ExecuteProcess(string arguments, string workspace)
        {
            var result = new TEMCalculationResult();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _exePath,
                    Arguments = arguments,
                    WorkingDirectory = _pluginDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    result.ExitCode = process.ExitCode;
                    result.Output = output;
                    result.Error = error;
                    result.Success = (process.ExitCode == 0);
                    result.ResultPath = Path.Combine(_pluginDirectory, workspace);

                    if (!result.Success)
                    {
                        result.ErrorMessage = $"a.exe 运行失败 (ExitCode: {process.ExitCode})。\n\n错误信息:\n{error}";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }

    /// <summary>
    /// TEM 计算结果
    /// </summary>
    public class TEMCalculationResult
    {
        /// <summary>是否成功</summary>
        public bool Success { get; set; }

        /// <summary>结果路径</summary>
        public string ResultPath { get; set; }

        /// <summary>错误信息</summary>
        public string ErrorMessage { get; set; }

        /// <summary>进程退出码</summary>
        public int ExitCode { get; set; }

        /// <summary>标准输出</summary>
        public string Output { get; set; }

        /// <summary>标准错误</summary>
        public string Error { get; set; }
    }
}
