// ============================================================
// 文件：DataGirdBaseAppender.cs
// 模块：Log4Net 日志输出（UI 表格附加器）
// 说明：自定义 log4net Appender，将日志事件渲染为文本后先入缓冲队列，
//       再由常驻后台任务每 300ms 批量跨线程追加到 UIDataGridView 日志表格，
//       表格行数超上限(200 行)时复用最旧行，避免高频日志刷新拖慢主线程。
// ============================================================
using log4net.Appender;
using log4net.Layout;
using SmartVisionSystem.UIForm;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmartVisionSystem.Log4Net
{
    /// <summary>
    /// 日志字符串广播事件委托：日志文本渲染后通过该事件对外发布，供外部订阅者(如日志文件写入)使用
    /// </summary>
    /// <param name="str">渲染后的日志文本</param>
    public delegate void LogString(string str);
    /// <summary>
    /// 自定义 log4net Appender：把日志事件渲染为文本，先缓存到内存队列，
    /// 再由常驻后台任务定时批量跨线程刷新到主界面的 UIDataGridView 日志表格；
    /// 表格超过 200 行时移除并复用最旧行对象，控制内存与刷新开销。
    /// </summary>
    public class DataGirdBaseAppender : AppenderSkeleton
    {
        public static event LogString LogString;//日志事件
        //将之前的 直接访问控件的方式修改为数据绑定  yoga  2019-8-26 09:41:25
        // 绑定的 UI 日志表格控件（改为数据绑定方式刷新，避免直接跨线程操作控件）
        UIDataGridView m_DataGrid;
        /// <summary>
        /// 获取或设置日志显示表格；赋值时将表格数据源绑定为日志行集合(BindingCollection)，
        /// 后续对集合的增删改会自动通知表格刷新
        /// </summary>
        public UIDataGridView DataGrid
        {
            get
            {
                return m_DataGrid;
            }
            set
            {
                m_DataGrid = value;
                m_DataGrid.DataSource = m_LogListShow;
            }
        }


        /// <summary>
        /// 静态构造函数（当前无初始化逻辑，预留）
        /// </summary>
        static DataGirdBaseAppender()
        {

        }
        /// <summary>
        /// log4net 日志事件入口：按 PatternLayout 将事件渲染为文本（异常对象追加堆栈信息），
        /// 未绑定表格或快速模式下直接丢弃，最后交由 Printf 异步刷入表格
        /// </summary>
        /// <param name="loggingEvent">log4net 传入的日志事件（级别、消息、异常等）</param>
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            // 未绑定 UI 表格时无处显示，直接返回
            if (DataGrid == null) return;

            // 快速模式下关闭界面日志，降低高频运行时的 UI 开销
            if (Log.IsFastModle)
            {
                return;
            }

            // 按配置的 PatternLayout 模板渲染日志文本
            PatternLayout patternLayout = (PatternLayout)this.Layout;
            string str;
            if (patternLayout != null)
            {
                str = patternLayout.Format(loggingEvent);

                // 日志事件携带异常时，把异常信息（含堆栈）追加到日志文本末尾
                if (loggingEvent.ExceptionObject != null)
                    str += loggingEvent.ExceptionObject.ToString() + Environment.NewLine;
            }
            // 未配置布局模板时，退化为“日志器名-消息”的简单格式
            else
                str = loggingEvent.LoggerName + "-" + loggingEvent.RenderedMessage + Environment.NewLine;

            // 打印
            Printf(str);
        }

        private bool m_Flag = false;//线程开启

        //private List<LogData> m_LogList = new List<LogData>();

        // 待显示日志文本缓冲队列：Append 只负责入队，UI 刷新由后台任务批量处理
        private readonly List<string> m_LogStrList = new List<string>();
        // 与表格绑定的日志行集合（实现 IBindingList，增删改可自动通知控件刷新）
        private readonly IBindingList m_LogListShow = new BindingCollection<LogData>();

        // 保护 m_LogStrList 的同步锁：日志可能来自相机取流、PLC 通信等多个线程
        private readonly Object m_LockObj = new Object();
        /// <summary>
        /// 异步输出日志：先将文本压入缓冲队列并立即返回；首次调用时启动常驻后台任务，
        /// 每 300ms 批量取出队列内容，解析为 LogData 后跨线程(Invoke)刷新到 UI 表格并滚动到最后一行
        /// </summary>
        /// <param name="str">渲染后的日志文本</param>
        private void Printf(string str)
        {
            // 加锁入队，保证多线程并发写日志时的线程安全
            lock (m_LockObj)
            {
                Debug.WriteLine(str);

                m_LogStrList.Add(str);
                //string[] strArr = Regex.Split(str, "--", RegexOptions.IgnoreCase);
                //LogData d = new LogData(strArr[0].Split(' ')[1], strArr[1], str.Replace($"{strArr[0]}--{strArr[1]}--", ""));
                //m_LogList.Add(d);
            }

            // 后台刷新任务尚未启动时置标志并启动（整个生命周期只启动一次）
            if (m_Flag == false)
            {
                m_Flag = true;
                Task.Run(
                () =>
                {
                    while (true)
                    {
                        Thread.Sleep(300);//日志刷新周期是300ms 避免 native刷新太块 抢占了主线程
                        if (m_LogStrList.Count == 0) continue;

                        // 加锁把缓冲队列一次性搬到临时列表，尽量缩短临界区，减少写日志阻塞
                        List<string> tempList = new List<string>();
                        lock (m_LockObj)
                        {
                            // 逐条触发日志广播事件（供外部订阅者处理），同时复制到临时列表等待刷表
                            for (int i = 0; i < m_LogStrList.Count; i++)
                            {
                                LogString?.Invoke(m_LogStrList[i]);
                                tempList.Add(m_LogStrList[i]);
                            }
                            m_LogStrList.Clear();
                            //tempList = CloneObject.DeepCopy<List<LogData>>(m_LogList);
                            //m_LogList.Clear();
                        }

                        if (tempList.Count > 0)
                        {
                            try
                            {
                                // 跨线程更新 UI：通过 Invoke 切换到表格所在线程执行刷新
                                DataGrid.Invoke(new Action(() =>
                                {
                                    try
                                    {
                                        foreach (var item in tempList)
                                        {

                                            // 按 "--" 分隔符拆分日志：[0]=时间+级别 [1]=日志器名 剩余部分=消息内容
                                            string[] strArr = Regex.Split(item, "--", RegexOptions.IgnoreCase);
                                            //如果小于200条直接new一个新对象
                                            if (m_LogListShow.Count < 200)
                                            {
                                                LogData d = new LogData(strArr[0].Split(' ')[1], strArr[1], item.Replace($"{strArr[0]}--{strArr[1]}--", ""));
                                                m_LogListShow.Add(d);
                                            }
                                            else//大于200条就将之前的重写后追加到最后
                                            {
                                                LogData d = m_LogListShow[0] as LogData;
                                                m_LogListShow.RemoveAt(0);
                                                d.RefushData(strArr[0].Split(' ')[1], strArr[1], item.Replace($"{strArr[0]}--{strArr[1]}--", ""));
                                                m_LogListShow.Add(d);
                                            }
                                        }
                                        // 选中最后一行，使日志自动滚动到最新
                                        DataGrid.CurrentCell = DataGrid.Rows[DataGrid.Rows.Count - 1].Cells[0];
                                        //DataGrid.FirstDisplayedScrollingRowIndex = DataGrid.Rows.Count - 1;
                                        //DataGrid.ScrollIntoView(DataGrid.Items[DataGrid.Items.Count - 1]);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }));
                            }
                            catch (TaskCanceledException)
                            {
                                //TaskCanceledException 异常是关闭时候可能出现 直接忽略
                            }
                            catch (Exception)
                            {
                                //throw;
                            }
                        }
                    }
                });
            }
        }
    }
}
