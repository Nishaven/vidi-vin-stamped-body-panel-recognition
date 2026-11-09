// ============================================================
// 文件：LogData.cs
// 模块：Log4Net（日志数据实体）
// 说明：UI 日志表格的单行数据：时间/等级/内容 + 按等级计算的前景色，
//       由 DataGirdBaseAppender 在收到日志事件时构造。
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVisionSystem.Log4Net
{
    /// <summary>
    /// UI 日志表格行数据实体：一条日志记录及其显示颜色
    /// </summary>
    [Serializable]
    public class LogData
    {
        /// <summary>
        /// 构造日志行数据并计算显示颜色
        /// </summary>
        /// <param name="_DateTime">日志时间文本</param>
        /// <param name="_LogLevel">日志等级文本（INFO/WARN/ERROR/FATAL 等）</param>
        /// <param name="_LogText">日志内容</param>
        public LogData(string _DateTime, string _LogLevel, string _LogText)
        {
            RefushData(_DateTime, _LogLevel, _LogText);
        }

        /// <summary>
        /// 填充行数据并按日志等级/内容确定文字颜色：
        /// WARN=棕红、ERROR/FATAL=橙红、通信收发(接收/发送数据)=蓝色、其余=黑色
        /// </summary>
        /// <param name="_DateTime">日志时间文本</param>
        /// <param name="_LogLevel">日志等级文本</param>
        /// <param name="_LogText">日志内容</param>
        public void RefushData(string _DateTime, string _LogLevel, string _LogText)
        {
            DateTime = _DateTime;
            LogLevel = _LogLevel.Trim();
            LogText = _LogText.Trim();
            // 按日志等级映射表格前景色，通信收发类日志单独标蓝便于排查
            switch (LogLevel)
            {
                case "WARN":
                    TextColor = "#8B4513";
                    break;
                case "ERROR":
                    TextColor = "#FF4D27";
                    break;
                case "FATAL":
                    TextColor = "#FF4D27";
                    break;
                default:
                    if (LogText.Contains("接收数据") || LogText.Contains("发送数据"))
                    {
                        TextColor = "#0000FF";
                    }
                    else
                    {
                        TextColor = "#000000";
                    }
                    break;
            }
        }
        /// <summary>
        /// 日志时间
        /// </summary>
        public string DateTime { get; set; }
        /// <summary>
        /// 日志等级文本（INFO/WARN/ERROR/FATAL 等）
        /// </summary>
        public string LogLevel { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string LogText { get; set; }
        /// <summary>
        /// 该行日志的文字颜色（十六进制色值）
        /// </summary>
        public string TextColor { get; set; }
    }
}
