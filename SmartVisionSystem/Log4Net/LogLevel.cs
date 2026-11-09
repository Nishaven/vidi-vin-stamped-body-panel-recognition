// ============================================================
// 文件：LogLevel.cs
// 模块：Log4Net（日志等级枚举）
// 说明：项目自定义日志等级（Debug/Info/Warn/Error/Fatal），
//       各成员用 Description 特性标注中文显示名。
// ============================================================
using System.ComponentModel;

namespace SmartVisionSystem.Log4Net
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public enum LogLevel
    {
        [DescriptionAttribute("程序调试")]
        Debug = 1,
        [DescriptionAttribute("程序正常运行")]
        Info,
        [DescriptionAttribute("程序警告")]
        Warn,
        [DescriptionAttribute("程序错误")]
        Error,
        [DescriptionAttribute("程序致命问题")]
        Fatal
    }
}
