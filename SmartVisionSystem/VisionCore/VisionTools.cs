// ============================================================
// 文件：VisionTools.cs
// 模块：VisionCore（视觉工具加载）
// 说明：相机与 ViDi 深度学习运行时之间的桥梁：
//       扫描 ..\CameraVisionTool\{相机名}\ 目录并加载全部 .vrws 工作区。
// ============================================================
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro;
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.UIForm;
using System;
using System.IO;
using ViDi2.Runtime;

namespace SmartVisionSystem.VisionCore
{

    /// <summary>
    /// 运行时视觉工具
    /// </summary>
    //public class ToolBlockParameter
    //{
    //    public CogToolBlock ToolBlock;
    //    public string Path;
    //    public string Name;
    //}

    /// <summary>
    /// ViDi 运行时工作区参数实体：记录一台相机加载的一个 .vrws 工作区
    /// （工作区对象 / 文件路径 / 工作区名称）
    /// </summary>
    public class RuntimeWorkSpaceParameter
    {
        /// <summary>
        /// ViDi 运行时工作区对象（用于遍历 Streams/Tools 做深度学习推理）
        /// </summary>
        public IWorkspace CameraWorkspace;
        /// <summary>
        /// 工作区文件完整路径（.vrws 深度学习模型文件）
        /// </summary>
        public string Path;
        /// <summary>
        /// 工作区名称（取自 .vrws 文件名，不含扩展名）
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// 运行时视觉工具加载器：负责把相机目录下的 .vrws 深度学习工作区
    /// 加载进 ViDi 全局运行时，供相机处理线程推理调用
    /// </summary>
    public class VisionTools
    {
        /// <summary>
        /// 全局 ViDi 深度学习运行时控制接口（由 MainForm 创建并注入，管理 GPU 与工作区）
        /// </summary>
        public IControl CameraControl;
        //public ToolBlockParameter CameraToolBlock;
        /// <summary>
        /// 当前相机加载的 ViDi 运行时工作区参数（工作区对象/路径/名称）
        /// </summary>
        public RuntimeWorkSpaceParameter CameraRuntimeWork;
        /// <summary>
        /// 视觉工具配置目录路径（..\CameraVisionTool\{相机名}，内含 .vrws 工作区文件）
        /// </summary>
        public string mToolblockPath = string.Empty;
        /// <summary>
        /// 标定工具路径（预留字段，当前未使用）
        /// </summary>
        public string mCalibrationToolsPath;
        /// <summary>
        /// 相机名称（仅构造时赋值）
        /// </summary>
        private readonly string CameraName;
        /// <summary>
        /// 反向引用所属相机（由 MainForm 装配时注入，便于运行时回溯）
        /// </summary>
        public CameraBase MyCameraBase;
        /// <summary>
        /// 构造视觉工具加载器：保存参数后立即扫描目录并加载全部 ViDi 工作区
        /// </summary>
        /// <param name="toolblockPath">视觉工具配置目录（.vrws 所在目录）</param>
        /// <param name="cameraName">相机名称</param>
        /// <param name="control">ViDi 全局运行时控制接口</param>
        public VisionTools(string toolblockPath, string cameraName, IControl control)
        {
            CameraControl = control;
            CameraName = cameraName;
            mToolblockPath = toolblockPath;
            ToolsInitialization();
        }

        /// <summary>
        /// 相机任务视觉工具加载初始化：
        /// 1. 检查目录下是否存在配置文件，为空则记录错误并退出程序；
        /// 2. 遍历目录下全部 .vrws 工作区文件，逐个加载进 ViDi 运行时（Workspaces.Add）；
        /// 3. 加载过程中通过闪屏汇报进度，单个文件加载失败则记录日志并退出程序
        /// </summary>
        public void ToolsInitialization()
        {
            string[] allfile = Directory.GetFiles(mToolblockPath);
            //检测测量任务目录下的配置文件个数必须大于0
            if (allfile.Length == 0)
            {
                //MessageWindow.ShowError("没有视觉任务,请检查！\n 程序会退出！");
                Log.Error("没有视觉任务,请检查！\n 程序会退出！");
                Environment.Exit(Environment.ExitCode);
            }
            //加载各个相机的检测VPP
            //CameraToolBlock = new ToolBlockParameter();
            //加载相机下的所有ToolBlock
            string[] tbfiles = Directory.GetFiles(mToolblockPath, "*.vrws");
            foreach (var file in tbfiles)
            {
                try
                {
                    // 将 .vrws 工作区加载进 ViDi 运行时，并保存工作区参数（名称/路径/工作区对象）
                    CameraRuntimeWork = new RuntimeWorkSpaceParameter
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,
                        CameraWorkspace = CameraControl.Workspaces.Add(Path.GetFileNameWithoutExtension(file), file)
                    };
                    SplashHandler.ChangeText($@">>> 加载{Directory.GetParent(Path.GetDirectoryName(file)).Name}生产配方{Path.GetFileNameWithoutExtension(mToolblockPath)}视觉工具算法配置文件{Path.GetFileNameWithoutExtension(file)}...");
                    Log.Info($@"{Directory.GetParent(Path.GetDirectoryName(file)).Name}生产配方{Path.GetFileNameWithoutExtension(mToolblockPath)}视觉工具算法配置文件{Path.GetFileNameWithoutExtension(file)}加载完成");
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format($@"{Directory.GetParent(Path.GetDirectoryName(file)).Name}视觉任务文件{Path.GetFileName(file)}已存在，但无法成功加载，加载错误原因：{ex}"));
                    Environment.Exit(Environment.ExitCode);
                }
            }
        }
    }
}
