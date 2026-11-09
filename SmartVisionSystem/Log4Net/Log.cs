// ============================================================
// 文件：Log.cs
// 模块：Log4Net（日志门面）
// 说明：对 log4net 的静态门面封装：等级日志（Debug~Fatal）、参数修改
//       记录、UI 表格输出注册、日志清理与运行时等级调整。
//       程序加载期(s_IsStarting=true)的 Warn/Error/Fatal 额外收集到
//       s_ErrMsg 供启动完成后弹窗提示。
// ============================================================
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartVisionSystem.Log4Net
{
    /// <summary>
    /// 日志静态门面：全局各模块统一通过 Log.Info / Log.Error 等方法写日志，
    /// 输出同时落盘（log4net RollingFileAppender）与显示（DataGrid Appender）
    /// </summary>
    public static class Log
    {
        public static bool s_IsStarting = false;//程序正在加载中 此时的错误要弹窗提示!
        public static List<string> s_ErrMsg = new List<string>();//加载过程中是错误信息
        private static readonly ILog log4Net = LogManager.GetLogger("logLogger");//全局日志
        private static readonly ILog LogModify = LogManager.GetLogger("modifyLogger");//模块参数修改记录
        //private static Flyout s_Flyout;//自动关闭的提示窗口
        //private static TextBox s_ShowTipText;//文字显示
        //private static PackIcon s_ShowTipIcon;//图标
        //private static int s_TipCount;//提示文本显示次数 用于关闭tip使用
        /// <summary>
        /// 是否显示悬浮提示（预留开关）
        /// </summary>
        private static bool s_ShowTip;//是否显示tip
        //private static string s_OldTip;// 上一个显示的tip 如果相同就不要再显示
        public static bool IsFastModle = false;//快速模式下不显示日志 yoga 2019-6-9 23:09:16 
        /// <summary>
        /// 注册日志系统：加载 exe 目录下 log4net.config 并进入"程序加载中"状态
        /// （此阶段产生的 Warn/Error/Fatal 会被收集，待启动完成后弹窗提示）
        /// </summary>
        public static void RegisterLog()
        {
            s_IsStarting = true;
            //wpf需要代码显示配置 否则则所有的引用该dll的项目都要配置文件
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(Application.StartupPath + "\\log4net.config"));
        }
        /// <summary>
        /// 输出 Debug 等级日志
        /// </summary>
        /// <param name="str">日志内容</param>
        public static void Debug(string str)
        {
            log4Net.Debug(str);
        }

        /// <summary>
        /// 输出 Info 等级日志
        /// </summary>
        /// <param name="str">日志内容</param>
        public static void Info(string str)
        {
            log4Net.Info(str);
        }
        /// <summary>
        /// 输出 Warn 等级日志（程序加载期额外收集用于弹窗提示）
        /// </summary>
        /// <param name="str">日志内容</param>
        public static void Warn(string str)
        {
            if (s_IsStarting)
            {
                if (!s_ErrMsg.Contains(str)) s_ErrMsg.Add(str);//窗体加载过程中 需要弹窗提示
            }

            log4Net.Warn(str);
        }
        /// <summary>
        /// 输出 Error 等级日志（程序加载期额外收集用于弹窗提示）
        /// </summary>
        /// <param name="str">日志内容</param>
        public static void Error(string str) // error为关键字
        {
            if (s_IsStarting)
            {
                if (!s_ErrMsg.Contains(str)) s_ErrMsg.Add(str);//窗体加载过程中 需要弹窗提示
            }
            log4Net.Error(str);
        }
        /// <summary>
        /// 输出 Fatal 等级日志（程序加载期额外收集用于弹窗提示）
        /// </summary>
        /// <param name="str">日志内容</param>
        public static void Fatal(string str)
        {
            if (s_IsStarting)
            {
                if (!s_ErrMsg.Contains(str)) s_ErrMsg.Add(str);//窗体加载过程中 需要弹窗提示
            }
            log4Net.Fatal(str);
        }

        //记录模块参数变更记录
        public static void ModullParamModify(string str)
        {
            LogModify.Info(str);
        }

        /// <summary>
        /// 显示提示信息
        /// </summary>
        /// <param name="str"></param>
        public static void Tip(string str)
        {
            log4Net.Info(str);
        }
        /// <summary>
        /// 按等级分发输出日志（等级枚举转对应门面方法）
        /// </summary>
        /// <param name="LogLevel">日志等级</param>
        /// <param name="str">日志内容</param>
        public static void Print(LogLevel LogLevel, string str)
        {
            switch (LogLevel)
            {
                case LogLevel.Debug:
                    {
                        Log.Debug(str);
                        break;
                    }
                case LogLevel.Info:
                    {
                        Log.Info(str);
                        break;
                    }
                case LogLevel.Warn:
                    {
                        Log.Warn(str);
                        break;
                    }
                case LogLevel.Error:
                    {
                        Log.Error(str);
                        break;
                    }
                case LogLevel.Fatal:
                    {
                        Log.Fatal(str);
                        //MessageWindow.ShowError(str);//错误就直接弹窗 yoga  2019-8-15 09:35:54
                        break;
                    }
                default:
                    {
                        Log.Debug(str);
                        break;
                    }
            }
        }

        /// <summary>
        /// /只需要在窗口初始化的时候注册listview就可以了
        /// </summary>
        /// <param name="richTextBox_log"></param>
        public static void InitializeDataGrid(UIDataGridView dataGrid)
        {
            if (dataGrid == null) return;
            // 设置listview打印日志
            var logPattern = "%d{yyyy-MM-dd HH:mm:ss} --%-5p-- %m%n";
            var list_logAppender = new DataGirdBaseAppender
            {
                DataGrid = dataGrid,
                Layout = new PatternLayout(logPattern)
            };
            Logger log4NetLogger = log4Net.Logger as Logger;//单独配置appender
            log4NetLogger.AddAppender(list_logAppender);
            //log4net.Config.BasicConfigurator.Configure(list_logAppender); 是配置所有

        }

        /// <summary>
        /// 设置悬浮提示窗口可见性（预留开关）
        /// </summary>
        /// <param name="flag">是否可见</param>
        public static void SetTipWindowVisibility(bool flag)
        {
            s_ShowTip = flag;
        }

        /// <summary>
        /// 获取日志存储文件夹
        /// </summary>
        /// <returns></returns>
        private static string GetFolder()
        {
            Logger log4NetLogger = log4Net.Logger as Logger;//

            var appender = log4NetLogger.GetAppender("LogFile") as RollingFileAppender;
            return Path.GetDirectoryName(appender.File);
        }

        /// <summary>
        /// 异步清理过期日志文件：删除最后写入时间早于指定天数的日志文件
        /// </summary>
        /// <param name="dayNum">保留天数（超过该天数的日志将被删除）</param>
        public static void DeleteLog(int dayNum)
        {
            Task.Run(() =>
            {
                try
                {
                    // 删除制定日期前的日志
                    DateTime tempDate;
                    DirectoryInfo dir = new DirectoryInfo(GetFolder());
                    FileInfo[] fileInfo = dir.GetFiles();
                    // 遍历
                    foreach (FileInfo NextFile in fileInfo)
                    {
                        tempDate = NextFile.LastWriteTime;
                        int days = (DateTime.Now - tempDate).Days;
                        // 删除30天前
                        if (days > dayNum)
                            File.Delete(NextFile.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Error(ex.ToString());
                }
            });
        }

        /// <summary>
        /// 更新日志等级
        /// </summary>
        /// <param name="level"></param>
        public static void UpdateLogLevel(string str)
        {
            try
            {
                LogLevel logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), str);

                Level level = Level.Debug;//log4net 自带的类
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        {
                            level = Level.Debug;
                            break;
                        }
                    case LogLevel.Info:
                        {
                            level = Level.Info;
                            break;
                        }
                    case LogLevel.Warn:
                        {
                            level = Level.Warn;
                            break;
                        }
                    case LogLevel.Error:
                        {
                            level = Level.Error;
                            break;
                        }
                    case LogLevel.Fatal:
                        {
                            level = Level.Fatal;
                            break;
                        }
                    default:
                        {
                            level = Level.Debug;
                            break;
                        }
                }
                Logger log4NetLogger = log4Net.Logger as Logger;//
                log4NetLogger.Level = level;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
