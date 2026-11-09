// ============================================================
// 文件：SplashScreenForm.cs
// 模块：UIForm（启动闪屏窗体）
// 说明：程序启动时的进度画面窗体，显示启动进度文字；
//       由 SplashHandler 在独立线程上创建并驱动。
// ============================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartVisionSystem.UIForm
{
    /// <summary>
    /// 启动闪屏窗体：承载启动图片与进度提示文字，方法均由
    /// SplashHandler 通过 Invoke 跨线程调度执行
    /// </summary>
    public partial class SplashScreenForm : Form
    {
        /// <summary>
        /// 初始化闪屏窗体
        /// </summary>
        public SplashScreenForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 关闭自身并恢复默认鼠标指针（供 Invoke 调用的关闭入口）
        /// </summary>
        public void KillMe(object o, EventArgs e)
        {
            this.Close();
            this.Cursor = Cursors.Default; ;
        }

        /// <summary>
        /// 置顶显示闪屏并将鼠标指针置为等待状态（供 Invoke 调用）
        /// </summary>
        public void BringTop(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            //this.TopMost = true;
        }

        /// <summary>
        /// 更新闪屏进度提示文字（供 Invoke 调用，文本经 sender 传入）
        /// </summary>
        public void ChangeText(object sender, EventArgs e)
        {
            LabelStatusInfor.Text = sender as String;
        }
    }
}
