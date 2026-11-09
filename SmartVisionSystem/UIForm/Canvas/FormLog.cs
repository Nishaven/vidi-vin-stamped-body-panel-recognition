// ============================================================
// 文件：FormLog.cs
// 模块：UIForm（日志页面）
// 说明：系统日志页面：构造时把页面内 UIDataGridView 注册为
//       log4net 的 UI 日志输出控件，实时滚动显示运行日志。
// ============================================================
using Sunny.UI;
using SmartVisionSystem.Log4Net;

namespace SmartVisionSystem.UIForm.Canvas
{
    /// <summary>
    /// 系统日志页面（UIPage）：将页面内表格挂载到 log4net 的
    /// DataGirdBaseAppender，实现文件 + UI 双通道日志输出
    /// </summary>
    public partial class FormLog : UIPage
    {
        /// <summary>
        /// 构造日志页面：初始化控件并将页面表格注册为 log4net UI 输出控件
        /// </summary>
        public FormLog()
        {
            InitializeComponent();
            Log.InitializeDataGrid(LogDataGridView);
        }
    }
}
