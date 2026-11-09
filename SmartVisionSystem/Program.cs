// ============================================================
// 文件：Program.cs
// 模块：程序入口
// 说明：应用程序主入口：单实例互斥检查 → 全局资源初始化
//       （异步存图队列 / 日志 / 启动闪屏）→ 启动主窗体 MainForm。
// ============================================================
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.UIForm;
using SmartVisionSystem.VisionCore;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartVisionSystem
{
    /// <summary>
    /// 应用程序入口类：负责单实例互斥、全局资源初始化（存图队列/日志/闪屏）
    /// 并启动主窗体（主窗体构造函数内完成相机/PLC/串口/数据库等全部装配）
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 单实例互斥：已存在同名互斥体说明软件已在运行，
            // 弹窗提示后直接退出，防止重复开启导致设备误触发/故障
            Mutex mutex = new Mutex(true, "SmartVisionSystem");
            if (!mutex.WaitOne(0, false))
            {
                UIMessageBox.ShowWarning("亲，软件已经在运行中，请勿重复开启软件导致设备故障哦！");
                Application.Exit();
                return;
            }

            // 初始化全局异步存图队列单例（后台 worker 线程消费落盘）
            ImageSaveQueue.gInit();

            // 在独立线程上启动启动画面（闪屏），避免阻塞主流程初始化
            SplashHandler.Start();

            // 等待闪屏窗体创建完成，确保后续置顶与进度文字更新有效
            Thread.Sleep(100);

            // 将闪屏窗口置顶显示
            SplashHandler.BringTop();

            // 更新闪屏进度提示（后续各阶段初始化也会持续更新该文字）
            SplashHandler.ChangeText(">>> 正在启动程序，加载应用程序配置...");

            // 加载 log4net.config，完成日志系统注册（文件 + UI 双通道）
            Log.RegisterLog();

            // 启动主窗体并进入消息循环：
            // MainForm 构造函数内依次完成 WPF 资源初始化、SQLite 打开、
            // 配置反序列化、光源串口打开、6 台相机初始化（海康开流 +
            // ViDi 工作区加载 + 处理线程启动）与页面装配
            Application.Run(new MainForm());
        }
    }
}
