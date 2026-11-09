// ============================================================
// 文件：SplashHandler.cs
// 模块：UIForm（启动闪屏）
// 说明：启动画面（SplashScreen）的线程管理器：在独立 STA 线程上
//       创建并运行闪屏窗体，主线程通过 Invoke 跨线程更新文字/置顶/关闭。
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartVisionSystem.UIForm
{
    /// <summary>
    /// 闪屏静态管理器：负责闪屏窗体的线程创建、进度文字更新、置顶与关闭
    /// </summary>
    public static class SplashHandler
    {
        /// <summary>
        /// 闪屏窗体实例（运行在独立线程上，跨线程访问需经 Invoke）
        /// </summary>
        static SplashScreenForm splash;

        /// <summary>
        /// 启动闪屏：新建后台线程创建并运行闪屏窗体（不阻塞主线程）
        /// </summary>
        public static void Start()
        {
            Thread splashThread = new Thread(new ThreadStart(StartSplash));
            splashThread.Start();
        }

        /// <summary>
        /// 闪屏线程入口：创建闪屏窗体并进入其消息循环
        /// </summary>
        static void StartSplash()
        {
            // Instance a splash form given the image names
            splash = new SplashScreenForm();
            // Run the form
            Application.Run(splash);
        }

        /// <summary>
        /// 关闭并释放闪屏窗体（通过 Invoke 切回闪屏线程执行关闭）
        /// </summary>
        public static void Close()
        {
            if (splash == null) return;

            // Shut down the splash screen
            splash.Invoke(new EventHandler(splash.KillMe));
            splash.Dispose();
            splash = null;
        }

        /// <summary>
        /// 更新闪屏上的进度提示文字（初始化各阶段调用）
        /// </summary>
        /// <param name="text">进度提示文本</param>
        public static void ChangeText(string text)
        {
            if (splash == null) return;
            splash.Invoke(new EventHandler(splash.ChangeText), text);
        }

        /// <summary>
        /// 将闪屏置顶并将鼠标指针置为等待状态
        /// </summary>
        public static void BringTop()
        {
            if (splash == null) return;
            splash.Invoke(new EventHandler(splash.BringTop));
        }
    }
}
