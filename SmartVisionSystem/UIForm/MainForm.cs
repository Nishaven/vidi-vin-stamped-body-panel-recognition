// ============================================================
// 文件：MainForm.cs
// 模块：SmartVisionSystem.UIForm（主窗体）
// 说明：系统全局装配中心：加载 JSON 配置、创建 ViDi 运行时/PLC 连接/光源串口/SQLite/相机与页面，
//       注入相机级联引用与车型曝光增益配方，并维护 PLC 心跳、模式轮询、GPIO 触发与退出清理。
// ============================================================
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.UIForm.Canvas;
using SmartVisionSystem.UIForm.Pages;
using SmartVisionSystem.VisionCore;
using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Threading;
using CTOPGPIO1808;
using HslCommunication;
using HslCommunication.Profinet.Melsec;
using HslCommunication.Profinet;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Omron;

namespace SmartVisionSystem.UIForm
{
    /// <summary>
    /// 主窗体（SunnyUI 框架）：全局装配中心——创建 ViDi 运行时 Control、连接西门子 S7-1500 PLC、
    /// 打开两路光源串口、初始化 SQLite，构造车型识别相机与各 VIN 读码相机并互相注入级联引用和配方；
    /// 再通过定时器维护 PLC 心跳方波、自动/手动模式轮询、触发信号上升沿检测与键鼠空闲自动注销权限。
    /// </summary>
    public partial class MainForm : UIHeaderAsideMainFrame
    {
        // ViDi 深度学习本地运行时（单 GPU 模式），创建后注入各相机共享
        private readonly ViDi2.Runtime.Local.Control CameraControl = new ViDi2.Runtime.Local.Control(ViDi2.GpuMode.SingleDevicePerTool);
        // 相机页面窗体（各相机画面显示与结果表格）
        public readonly FormCamera mFormCamera;
        // 日志页面窗体
        private readonly FormLog mFormLog;
        // 系统设置页面窗体
        public readonly FormSystemSet mFormSystemSet;
        // 光源控制器串口1（车型识别相机使用，19200/N/8/1）
        public SerialPort mSerialPort;
        // 光源控制器串口2（各 VIN 读码相机共用）
        public SerialPort mSerialPort2;

        // 系统配置（从 JSON 反序列化：相机参数、PLC 信号地址、光源串口名等）
        public readonly SystemConfigurations MySystemConfigurations = new SystemConfigurations();
        // 相机配置文件路径（exe 上级目录 Configurations\CameraConfigurations.json）
        private readonly string CameraParameterConfigurationPath = Path.GetFullPath("..") + "\\Configurations\\CameraConfigurations.json";
        // 当前登录用户（默认"技术员"权限0；超级管理员可解锁系统设置页）
        public VisionCore.User mUser = new VisionCore.User("技术员", "", 0);
        //private MelsecMcAsciiNet MyMelsecMcNet;
        //public MelsecMcNet MyMelsecMcNet;
        // 西门子 S7-1500 PLC 通信客户端（变量名沿用历史 MelsecMcNet 叫法）
        public SiemensS7Net MyMelsecMcNet;
        // 车型识别相机（读码流程第一级）
        public readonly CarIdentificationCamera mCarIdentificationCamera;
        // VIN 读码相机1~5
        public readonly VINCodeCamera1 mVINCodeCamera1;
        public readonly VINCodeCamera2 mVINCodeCamera2;
        public readonly VINCodeCamera3 mVINCodeCamera3;
        public readonly VINCodeCamera4 mVINCodeCamera4;
        public readonly VINCodeCamera5 mVINCodeCamera5;

        //2026.9 V22-G-s
        // V22-G 新增的 VIN 读码相机6~8
        public readonly VINCodeCamera6 mVINCodeCamera6;
        public readonly VINCodeCamera7 mVINCodeCamera7;
        public readonly VINCodeCamera8 mVINCodeCamera8;
        //2026.9 V22-G-e

        // SQLite 数据库助手（VIN 读码结果落库）
        private readonly SQLiteHelper SQLiteHelperResult;
        // 读码结果并发队列（各相机写入，UI/落库消费）
        private readonly ConcurrentQueue<VINCodeResult> VINCodeResultQueue = new ConcurrentQueue<VINCodeResult>();
        // SQLite 数据库文件路径（exe 上级 VINDataRecord\DataRecord.db）
        private string ResultDataDBPath = Path.GetFullPath("..") + @"\VINDataRecord\DataRecord.db";
        // GPIO 板卡是否初始化成功
        private bool bIO_Init;
        // PLC 自动模式位与相机触发位的本次/上次读取结果（用于触发信号上升沿检测）
        private OperateResult<bool> AutoModel, CameraTriggerCurrent = new OperateResult<bool>(), CameraTriggerBefore = new OperateResult<bool>();
        // 心跳定时器（1s 方波写 PLC）与轮询定时器（300ms 模式/触发/空闲检测）
        private System.Timers.Timer HearBeattimer, HearBeattimer2;
        /// <summary>
        /// 主窗体构造：装配整个系统——初始化 ViDi/WPF 资源、打开结果数据库、加载 JSON 配置、
        /// 建立两路光源串口、创建全部相机与页面窗体，并注入相机间级联引用与各车型曝光/增益配方。
        /// </summary>
        public MainForm()
        {
            // 初始化 WPF Application 资源（ViDi 控件依赖默认样式，WinForms 宿主需手工加载）
            if (System.Windows.Application.Current == null)
            {
                Log.Info("程序开始加载资源文件。。。。。。");
                new System.Windows.Application();
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(new Uri("/ViDi.Net.UI;component/Styles/DefaultStyle.xaml", UriKind.Relative)) as System.Windows.ResourceDictionary);
                System.Windows.Application.Current.Resources = new System.Windows.ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/ViDi.NET.UI;component/Styles/DefaultStyle.xaml")
                };
                Log.Info("程序加载资源文件完成！");
            }

            // 初始化 ViDi UI 服务定位器（ViDi 显示控件依赖）
            if (!ViDi2.UI.ViDiSuiteServiceLocator.IsInitialized)
                ViDi2.UI.ViDiSuiteServiceLocator.Initialize();
            InitializeComponent();
            // 打开 VIN 结果数据库
            SQLiteHelperResult = new SQLiteHelper(ResultDataDBPath);
            SQLiteHelperResult.Open();
            mFormLog = new FormLog { SymbolSize = 24, Symbol = 61474 };
            // 反序列化相机配置 JSON 为系统配置对象
            MySystemConfigurations = Json.DeserializeFromFile<SystemConfigurations>(CameraParameterConfigurationPath, Encoding.UTF8);
            Log.Info("相机配置参数文件加载完成！");
            SplashHandler.ChangeText(">>> 加载相机配置参数文件...");
            // 光源串口1：配置的串口存在才打开（19200/N/8/1），供车型识别相机开/关灯
            if (SerialPort.GetPortNames().Any(p => p == MySystemConfigurations.LightControlCOMName))
            {
                mSerialPort = new SerialPort { PortName = MySystemConfigurations.LightControlCOMName, BaudRate = 19200, Parity = Parity.None, DataBits = 8, StopBits = StopBits.One };
                if (mSerialPort.IsOpen)
                {
                    mSerialPort.Close();
                }
                else
                {
                    Log.Warn($"光源控制器通信1 链接成功！");
                    mSerialPort.Open();
                }
                if (mSerialPort == null) Log.Warn($"光源控制器通信1 链接失败！");
            }
            else Log.Warn($"当前设备不能存在{MySystemConfigurations.LightControlCOMName},无法建立光源控制器通信！");

            // 光源串口2：各 VIN 读码相机共用
            if (SerialPort.GetPortNames().Any(p => p == MySystemConfigurations.LightControlCOMName2))
            {
                mSerialPort2 = new SerialPort { PortName = MySystemConfigurations.LightControlCOMName2, BaudRate = 19200, Parity = Parity.None, DataBits = 8, StopBits = StopBits.One };
                if (mSerialPort2.IsOpen)
                {
                    mSerialPort2.Close();
                }
                else
                {
                    Log.Warn($"光源控制器通信2 链接成功！");
                    mSerialPort2.Open();
                }
                if (mSerialPort2 == null) Log.Warn($"光源控制器通信2 链接失败！");
            }
            else Log.Warn($"当前设备不能存在{MySystemConfigurations.LightControlCOMName2},无法建立光源控制器2通信！");

            // 按配置创建车型识别相机与各 VIN 读码相机（共享同一个 ViDi Control）
            mCarIdentificationCamera = new CarIdentificationCamera(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera"), CameraControl);
            mVINCodeCamera1 = new VINCodeCamera1(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1"), CameraControl);
            mVINCodeCamera2 = new VINCodeCamera2(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2"), CameraControl);
            mVINCodeCamera3 = new VINCodeCamera3(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3"), CameraControl);
            mVINCodeCamera4 = new VINCodeCamera4(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4"), CameraControl);
            mVINCodeCamera5 = new VINCodeCamera5(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5"), CameraControl);
          
            // 回填各相机视觉工具的相机引用，便于工具界面反向操作相机
            mCarIdentificationCamera.MyVisionTools.MyCameraBase = mCarIdentificationCamera;
            mVINCodeCamera1.MyVisionTools.MyCameraBase = mVINCodeCamera1;
            mVINCodeCamera2.MyVisionTools.MyCameraBase = mVINCodeCamera2;
            mVINCodeCamera3.MyVisionTools.MyCameraBase = mVINCodeCamera3;
            mVINCodeCamera4.MyVisionTools.MyCameraBase = mVINCodeCamera4;
            mVINCodeCamera5.MyVisionTools.MyCameraBase = mVINCodeCamera5;

            //2026.9 V22-G-s
            mVINCodeCamera6 = new VINCodeCamera6(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6"), CameraControl);
            mVINCodeCamera7 = new VINCodeCamera7(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7"), CameraControl);
            mVINCodeCamera8 = new VINCodeCamera8(MySystemConfigurations, MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8"), CameraControl);
            mVINCodeCamera6.MyVisionTools.MyCameraBase = mVINCodeCamera6;
            mVINCodeCamera7.MyVisionTools.MyCameraBase = mVINCodeCamera7;
            mVINCodeCamera8.MyVisionTools.MyCameraBase = mVINCodeCamera8;
            //2026.9 V22-G-e

            //if (MySystemConfigurations.LightIndex != null) mVINCodeCamera3.LightIndex = MySystemConfigurations.LightIndex;
            // 创建各功能页面并挂载到左侧导航
            mFormCamera = new FormCamera(this) { SymbolSize = 24, Symbol = 62056 };
            mFormSystemSet = new FormSystemSet(MySystemConfigurations, this) { SymbolSize = 24, Symbol = 61459 };
            Aside.CreateNode(AddPage(mFormCamera, 10000));
            Aside.CreateNode(AddPage(mFormLog, 20000));
            Aside.CreateNode(AddPage(mFormSystemSet, 30000));
            Aside.SelectFirst();
            // 绑定车型识别相机的显示控件与光源串口1
            mCarIdentificationCamera.CurrentImageDisplay = mFormCamera.CarIdentificationCameraDisplay;
            mFormCamera.CarIdentificationCameraDisplay.mVisionTools = mCarIdentificationCamera.MyVisionTools;
            mCarIdentificationCamera.mSerialPort = mSerialPort;
            // 车型识别相机的各车型曝光/增益配方：取各 VIN 读码相机的配置，识别到车型后按编号下发
            mCarIdentificationCamera.CameraExposure1 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").CameraExposure;
            mCarIdentificationCamera.CameraExposure2 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").CameraExposure;
            mCarIdentificationCamera.CameraExposure3 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").CameraExposure;
            mCarIdentificationCamera.CameraExposure4 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraExposure;
            mCarIdentificationCamera.CameraExposure5 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraExposure;
            mCarIdentificationCamera.CameraExposure6 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraExposure;
            mCarIdentificationCamera.CameraExposure7 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraExposure;
            mCarIdentificationCamera.CameraExposure8 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").CameraExposure;
            mCarIdentificationCamera.CameraExposure9 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera9").CameraExposure;
            mCarIdentificationCamera.CameraExposure10 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera10").CameraExposure;
            mCarIdentificationCamera.CameraGain1 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").CameraGain;
            mCarIdentificationCamera.CameraGain2 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").CameraGain;
            mCarIdentificationCamera.CameraGain3 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").CameraGain;
            mCarIdentificationCamera.CameraGain4 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraGain;
            mCarIdentificationCamera.CameraGain5 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraGain;
            mCarIdentificationCamera.CameraGain6 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraGain;
            mCarIdentificationCamera.CameraGain7 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraGain;
            mCarIdentificationCamera.CameraGain8 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").CameraGain;
            mCarIdentificationCamera.CameraGain9 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera9").CameraGain;
            mCarIdentificationCamera.CameraGain10 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera10").CameraGain;

            //2026.9 V22-G-e
            mCarIdentificationCamera.CameraExposure11 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera11").CameraExposure;
            mCarIdentificationCamera.CameraExposure12 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera12").CameraExposure;
            mCarIdentificationCamera.CameraExposure13 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera13").CameraExposure;
            mCarIdentificationCamera.CameraExposure14 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera14").CameraExposure;
            mCarIdentificationCamera.CameraExposure15 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera15").CameraExposure;
            mCarIdentificationCamera.CameraExposure16 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera16").CameraExposure;
            mCarIdentificationCamera.CameraExposure99 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera99").CameraExposure;
            mCarIdentificationCamera.CameraGain11 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera11").CameraGain;
            mCarIdentificationCamera.CameraGain12 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera12").CameraGain;
            mCarIdentificationCamera.CameraGain13 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera13").CameraGain;
            mCarIdentificationCamera.CameraGain14 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera14").CameraGain;
            mCarIdentificationCamera.CameraGain15 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera15").CameraGain;
            mCarIdentificationCamera.CameraGain16 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera16").CameraGain;
            mCarIdentificationCamera.CameraGain99 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera99").CameraGain;
            //2026.9 V22-G-s


            // 为各 VIN 读码相机注入：显示控件 / 光源串口2 / SQLite / 结果队列 / 结果表格
            mVINCodeCamera1.CurrentImageDisplay = mFormCamera.VINCodeCamera1CameraDisplay;
            mFormCamera.VINCodeCamera1CameraDisplay.mVisionTools = mVINCodeCamera1.MyVisionTools;
            mVINCodeCamera1.mSerialPort = mSerialPort2;//不一样
            mVINCodeCamera1.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera1.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera1.ResultDataGridView = mFormCamera.ResultDataGridView;

            mVINCodeCamera2.CurrentImageDisplay = mFormCamera.VINCodeCamera2CameraDisplay;
            mFormCamera.VINCodeCamera2CameraDisplay.mVisionTools = mVINCodeCamera2.MyVisionTools;
            mVINCodeCamera2.mSerialPort = mSerialPort2;//不一样
            mVINCodeCamera2.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera2.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera2.ResultDataGridView = mFormCamera.ResultDataGridView;

            mVINCodeCamera3.CurrentImageDisplay = mFormCamera.VINCodeCamera3CameraDisplay;
            mFormCamera.VINCodeCamera3CameraDisplay.mVisionTools = mVINCodeCamera3.MyVisionTools;
            mVINCodeCamera3.mSerialPort = mSerialPort2;//不一样
            mVINCodeCamera3.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera3.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera3.ResultDataGridView = mFormCamera.ResultDataGridView;

            mVINCodeCamera4.CurrentImageDisplay = mFormCamera.VINCodeCamera4CameraDisplay;
            mVINCodeCamera5.CurrentImageDisplay = mFormCamera.VINCodeCamera5CameraDisplay;
            mFormCamera.VINCodeCamera4CameraDisplay.mVisionTools = mVINCodeCamera4.MyVisionTools;
            mFormCamera.VINCodeCamera5CameraDisplay.mVisionTools = mVINCodeCamera5.MyVisionTools;
            mVINCodeCamera4.mSerialPort = mSerialPort2;
            mVINCodeCamera5.mSerialPort = mSerialPort2;
            mVINCodeCamera4.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera5.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera4.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera5.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera4.ResultDataGridView = mFormCamera.ResultDataGridView;
            mVINCodeCamera5.ResultDataGridView = mFormCamera.ResultDataGridView;

            // 注入相机间级联引用：车型识别相机按车型触发对应读码相机
            mCarIdentificationCamera.mVINCodeCamera1 = mVINCodeCamera1;
            mCarIdentificationCamera.mVINCodeCamera2 = mVINCodeCamera2;
            mCarIdentificationCamera.mVINCodeCamera3 = mVINCodeCamera3;
            mCarIdentificationCamera.mVINCodeCamera4 = mVINCodeCamera4;
            mCarIdentificationCamera.mVINCodeCamera5 = mVINCodeCamera5; 
            // ===== 相机间引用 ===== 在CarIdentificationCamera中声明VINCodeCamera6-8
            mCarIdentificationCamera.mVINCodeCamera6 = mVINCodeCamera6;
            mCarIdentificationCamera.mVINCodeCamera7 = mVINCodeCamera7;
            mCarIdentificationCamera.mVINCodeCamera8 = mVINCodeCamera8;

            // 读码失败备用链：Camera2 失败 → Camera3
            mVINCodeCamera2.mVINCodeCamera3 = mVINCodeCamera3;
            mVINCodeCamera2.CameraExposure5 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraExposure;
            mVINCodeCamera2.CameraGain5 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraGain;
            // 备用链：Camera1 失败 → Camera2 / Camera4
            mVINCodeCamera1.mVINCodeCamera2 = mVINCodeCamera2;
            mVINCodeCamera1.CameraExposure4 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraExposure;
            mVINCodeCamera1.CameraGain4 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraGain;
            mVINCodeCamera1.mVINCodeCamera4 = mVINCodeCamera4;
            mVINCodeCamera1.CameraExposure6 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraExposure;
            mVINCodeCamera1.CameraGain6 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraGain;

            //2026.9 V22-G-s
            // ===== 通用配置：显示控件 / 串口 / SQLite / 结果队列 / 结果表格 =====
            //NO.6
            //显示控件
            mVINCodeCamera6.CurrentImageDisplay = mFormCamera.VINCodeCamera6CameraDisplay;
            mFormCamera.VINCodeCamera6CameraDisplay.mVisionTools = mVINCodeCamera6.MyVisionTools;
            //串口
            mVINCodeCamera6.mSerialPort = mSerialPort2;
            //SQLite
            mVINCodeCamera6.SQLiteHelperResult = SQLiteHelperResult;
            //结果队列
            mVINCodeCamera6.VINCodeResultQueue = VINCodeResultQueue;
            //结果表格
            mVINCodeCamera6.ResultDataGridView = mFormCamera.ResultDataGridView;
            //NO.7
            mVINCodeCamera7.CurrentImageDisplay = mFormCamera.VINCodeCamera7CameraDisplay;
            mFormCamera.VINCodeCamera7CameraDisplay.mVisionTools = mVINCodeCamera7.MyVisionTools;
            mVINCodeCamera7.mSerialPort = mSerialPort2;
            mVINCodeCamera7.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera7.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera7.ResultDataGridView = mFormCamera.ResultDataGridView;
            //NO.8
            mVINCodeCamera8.CurrentImageDisplay = mFormCamera.VINCodeCamera8CameraDisplay;
            mFormCamera.VINCodeCamera8CameraDisplay.mVisionTools = mVINCodeCamera8.MyVisionTools;
            mVINCodeCamera8.mSerialPort = mSerialPort2;
            mVINCodeCamera8.SQLiteHelperResult = SQLiteHelperResult;
            mVINCodeCamera8.VINCodeResultQueue = VINCodeResultQueue;
            mVINCodeCamera8.ResultDataGridView = mFormCamera.ResultDataGridView;


            //交叉引用关系，识别NG时切换相机在此声明
            //引用：2026.9 V22-G引用，在VINCodeCamera5中声明VINCodeCamera6，VINCodeCamera5调用VINCodeCamera6的曝光和增益
            //相机5调用相机6
            mVINCodeCamera5.mVINCodeCamera6 = mVINCodeCamera6;
            //相机5引用相机7的曝光和增益进6号相机
            mVINCodeCamera5.CameraExposure7 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraExposure;
            mVINCodeCamera5.CameraGain7 = MySystemConfigurations.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraGain;
            //2026.9 V22-G-e


        }

        /// <summary>
        /// 窗体加载：校验 SunnyUI 授权、初始化 GPIO 板卡并启动 DI 触发轮询线程、
        /// 连接西门子 S7-1500 PLC 并注入各相机，最后启动心跳与轮询两个定时器。
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // SunnyUI 授权码校验
            if (!Authorization.SetAuthorizationCode("3daa5a3b-defd-474d-b2d9-c4d2265dfe20"))
            {
                Log.Error("通信失败！");
            }

            bIO_Init = true;
            uint count = 0;
            int err= 0;
            // 初始化 GPIO 板卡：成功则启动引脚轮询线程（DI 触发检测）
            err = TOPGPIO1808.TOPGPIO1808_Init(ref count);
            if (err != 0)
            {
                Log.Error("IO通信失败！");
                bIO_Init = false;
            }
            else
            {
                Thread thread = new Thread(new ThreadStart(ReadPin));
                thread.Start();
            }

            //MyMelsecMcNet = new MelsecMcNet
            //{
            //    ConnectTimeOut = 1000,
            //    IpAddress = MySystemConfigurations.PLCCommunicationIP,
            //    Port = MySystemConfigurations.PLCCommunicationPort,
            //    ReceiveTimeOut = 1000,
            //};
            // 创建西门子 S7-1500 PLC 连接（连接/接收超时各 1 秒）
            MyMelsecMcNet = new SiemensS7Net(SiemensPLCS.S1500)
            {
                ConnectTimeOut = 1000,
                IpAddress = MySystemConfigurations.PLCCommunicationIP,
                Port = MySystemConfigurations.PLCCommunicationPort,
                //Slot = 1,
                ReceiveTimeOut = 1000,
            };

            // PLC 连接成功：把通信实例注入所有相机并启动心跳/轮询定时器
            if (MyMelsecMcNet.ConnectServer().IsSuccess)
            {
                Log.Info("PLC通信已连接");
                mCarIdentificationCamera.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera1.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera2.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera3.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera4.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera5.mMelsecMcNet = MyMelsecMcNet;

                //2026.9 V22-G-s
                mVINCodeCamera6.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera7.mMelsecMcNet = MyMelsecMcNet;
                mVINCodeCamera8.mMelsecMcNet = MyMelsecMcNet;
                //2026.9 V22-G-e


                // 心跳定时器：每 1s 向 PLC 写系统在线方波信号
                HearBeattimer = new System.Timers.Timer
                {
                    Interval = 1000
                };
                HearBeattimer.Elapsed += HearBeattimer_Elapsed;
                HearBeattimer.Start();
                // 轮询定时器：每 300ms 读自动模式/触发信号并做键鼠空闲检测
                HearBeattimer2 = new System.Timers.Timer
                {
                    Interval = 300
                };
                HearBeattimer2.Elapsed += HearBeattimer_Elapsed2;
                HearBeattimer2.Start();
                //if (mVINCodeCamera1.mMelsecMcNet != null) Log.Info($"切换至自动模式，暂停系统,信号地址：{MySystemConfigurations.PLCStopAddress}写入值False,写入状态：{MyMelsecMcNet.Write(MySystemConfigurations.PLCStopAddress, false).IsSuccess}");
            }
            else Log.Error("PLC通信连接失败，请检查PLC通信的硬件连接和软件设置");
            SplashHandler.Close();
        }

        // 自动模式标志（默认自动；实际运行模式以 PLC 读取结果为准）
        public bool bAutoModel = true;
        // 心跳方波当前电平（每次心跳后翻转）
        bool hearbeatflag = true;
        /// <summary>
        /// 心跳定时器回调：每秒向 PLC SystemOnlineAddress 写入方波电平（真/假交替），供 PLC 侧监控系统是否存活。
        /// </summary>
        private void HearBeattimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 写心跳电平并翻转，形成 1s 周期方波
            MyMelsecMcNet.Write(MySystemConfigurations.SystemOnlineAddress, hearbeatflag);
            hearbeatflag = !hearbeatflag;
        }

        /// <summary>
        /// 轮询定时器回调（300ms）：读 PLC 的自动/手动模式位与相机触发位，
        /// 把模式同步给所有相机并刷新界面模式指示；检测到触发信号上升沿则软触发车型识别相机；
        /// 另检测键鼠空闲超 3 分钟自动把权限降回"技术员"。
        /// </summary>
        private void HearBeattimer_Elapsed2(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 先停定时器避免回调重入，处理完再重启
            HearBeattimer2.Stop();
            // 读 PLC 的自动模式位与相机触发位
            AutoModel = MyMelsecMcNet.ReadBool(MySystemConfigurations.AutoModelAddress);
            CameraTriggerCurrent = MyMelsecMcNet.ReadBool(MySystemConfigurations.CameraTriggerAddress);
            if (AutoModel.IsSuccess)
            {
                // 把当前模式同步给所有相机（相机据此决定是否写 PLC 结果）
                mCarIdentificationCamera.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera1.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera2.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera3.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera4.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera5.IsAutoStatus = AutoModel.Content;

                //2026.9 V22-G-s
                mVINCodeCamera6.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera7.IsAutoStatus = AutoModel.Content;
                mVINCodeCamera8.IsAutoStatus = AutoModel.Content;
                //2026.9 V22-G-e

                // 跨线程刷新顶部模式指示（自动=绿色 / 手动=红色）
                if (AutoModel.Content)
                {
                    uiLabel2.BeginInvoke(new Action(() =>
                    {
                        uiLabel2.Text = "自动模式";
                        uiLabel2.ForeColor = System.Drawing.Color.Lime;
                    }));
                }
                else
                {
                    uiLabel2.BeginInvoke(new Action(() =>
                    {
                        uiLabel2.Text = "手动模式";
                        uiLabel2.ForeColor = System.Drawing.Color.Red;
                    }));
                }
            }
            if (CameraTriggerCurrent.IsSuccess)
            {
                // 触发信号上升沿（0→1）才触发一次车型识别采集，避免重复触发
                if (Convert.ToInt16(CameraTriggerCurrent.Content)- Convert.ToInt16(CameraTriggerBefore.Content)==1)
                {
                    mCarIdentificationCamera.SoftWareTrigger2();
                }
            }
            // 键鼠空闲超过 3 分钟：权限自动降级为技术员，防止高权限账号滞留在产线工位
            if (GetLastInputTime() > 3000 * 60)
            {
                mUser.Authority = 0;
                mUser.Name = "技术员";
                uiLabel1.BeginInvoke(new Action(() =>
                {
                    uiLabel1.Text = "当前为：技术员";
                }));
            }
            // 保存本次触发状态，供下一轮上升沿检测
            CameraTriggerBefore = CameraTriggerCurrent;
            HearBeattimer2.Start();
        }

        // GPIO 轮询上次读到的电平值（用于电平变化沿检测）
        static uint Oldtmp = 0;
        /// <summary>
        /// GPIO 引脚轮询线程：每 100ms 读 DI 引脚电平，检测到电平变为 0（触发沿）时
        /// 延时 1 秒后软触发车型识别相机采集。
        /// </summary>
        private void ReadPin()
        {
            //while (MySystemConfigurations.JopRunAddress != null && MySystemConfigurations.JopRunAddress != "")
            //{
            //    HslCommunication.OperateResult<bool> AutoModel = MyMelsecMcNet.ReadBool(MySystemConfigurations.JopRunAddress);
            //    if (AutoModel.IsSuccess && AutoModel.Content)
            //    {
            //        Thread.Sleep(1000);
            //        mCarIdentificationCamera.SoftWareTrigger2();
            //        MyMelsecMcNet.Write(MySystemConfigurations.JopRunAddress, false);
            //    }
            //    Thread.Sleep(100);
            //}
              // 板卡初始化成功时持续轮询 DI 引脚
              while (bIO_Init)
              {
                 uint tmp = 0;
                // 读 8 号输入引脚电平
                int err = TOPGPIO1808.TOPGPIO1808_ReadPin(8, ref tmp);

                // 引脚读到低电平且与上次不同（电平变化沿）：延时 1 秒避抖后触发车型识别采集
                if (err == 0 && tmp == 0 && Oldtmp != tmp)//高电平
                {
                    Thread.Sleep(1000);
                    mCarIdentificationCamera.SoftWareTrigger2();
                }
                Thread.Sleep(100);
                Oldtmp = tmp;
            }
            CameraTriggerBefore = CameraTriggerCurrent;
            HearBeattimer2.Start();
        }

        /// <summary>
        /// 登录按钮：弹出登录对话框，按账号切换当前用户与权限。
        /// </summary>
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            FormLogin mFormLogin = new FormLogin(this);
            mFormLogin.ShowDialog();
        }

        /// <summary>
        /// 窗体关闭：确认退出后按装配的逆序释放资源——反初始化 GPIO、把当前配置序列化回 JSON、
        /// 关闭异步存图队列与全部相机、释放各页面窗体，最后强制结束进程。
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ShowAskDialog("是否需要退出软件？"))
            {
                try
                {
                    // 反初始化 GPIO 板卡
                    if (bIO_Init) TOPGPIO1808.TOPGPIO1808_Deinit();
                    // 退出前把运行期配置写回 JSON（保留界面上的修改）
                    Json.SerializeToFile(MySystemConfigurations, CameraParameterConfigurationPath, Encoding.UTF8);
                    // 停止异步存图队列并等待后台存盘线程收尾
                    ImageSaveQueue.gShutDown();
                    mCarIdentificationCamera.CloseCamera();
                    mVINCodeCamera1.CloseCamera();
                    mVINCodeCamera2.CloseCamera();
                    mVINCodeCamera3.CloseCamera();
                    mVINCodeCamera4.CloseCamera();
                    mVINCodeCamera5.CloseCamera();

                    //2026.9 V22-G-s
                    mVINCodeCamera6.CloseCamera();
                    mVINCodeCamera7.CloseCamera();
                    mVINCodeCamera8.CloseCamera();
                    //2026.9 V22-G-e

                    mFormCamera.Dispose();
                    mFormLog.Dispose();
                    mFormSystemSet.Dispose();
                    mCarIdentificationCamera.mAutoResetEvent.Close();
                    this.Dispose();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Log.Info(ex.Message);
                    Environment.Exit(0);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 左侧导航切换回调：仅"系统设置"页做权限控制——超级管理员才能启用各配置分组，否则禁用。
        /// </summary>
        private void Aside_MenuItemClick(TreeNode node, NavMenuItem item, int pageIndex)
        {
            switch (node.Text)
            {
                case "系统设置":
                    if (mUser.Name == "超级管理员")
                    {
                        mFormSystemSet.GroupBoxStampingPLC.Enabled = true;
                        mFormSystemSet.uiGroupBox2.Enabled = true;
                        mFormSystemSet.uiGroupBox3.Enabled = true;
                    }
                    else
                    {
                        mFormSystemSet.GroupBoxStampingPLC.Enabled = false;
                        mFormSystemSet.uiGroupBox2.Enabled = false;
                        mFormSystemSet.uiGroupBox3.Enabled = false;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 模式指示标签点击事件（当前为空实现，保留占位）。
        /// </summary>
        private void uiLabel2_Click(object sender, EventArgs e)
        {

        }

        // 创建结构体用于返回捕获时间  
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            // 设置结构体块容量  
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            // 捕获的时间  
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }
        // 调用 user32.dll 获取系统最后一次输入事件的信息
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        //获取键盘和鼠标没有操作的时间
        private static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            // 捕获系统空闲时间（键盘及鼠标未操作时间）  
            if (!GetLastInputInfo(ref vLastInputInfo))
                return 0;
            else
                return Environment.TickCount - (long)vLastInputInfo.dwTime;//单位ms  
        }

        //    private void button1_Click(object sender, EventArgs e)
        //    {
        //        if (mUser.Name != "超级管理员")
        //        {
        //            UIMessageBox.ShowWarning("需要超级管理员才能点击！");
        //            return;
        //        }

        //        if (button1.Text == "非强制切换模式")
        //        {                
        //            button1.ForeColor = System.Drawing.Color.Red;
        //            button1.Text = "强制切换模式";
        //            //bAutoModel = true;
        //            //uiLabel2.Text = "手动模式";
        //            if (mVINCodeCamera1.mMelsecMcNet != null) Log.Info($"切换至手动模式，暂停系统,信号地址：{MySystemConfigurations.PLCStopAddress}写入值True,写入状态：{MyMelsecMcNet.Write(MySystemConfigurations.PLCStopAddress, true).IsSuccess}");
        //        }
        //        else
        //        {
        //            button1.ForeColor = System.Drawing.Color.Orange;
        //            button1.Text = "非强制切换模式";
        //            //bAutoModel = false;
        //            //uiLabel2.Text = "自动模式";
        //            if (mVINCodeCamera1.mMelsecMcNet != null) Log.Info($"切换至自动模式，暂停系统,信号地址：{MySystemConfigurations.PLCStopAddress}写入值False,写入状态：{MyMelsecMcNet.Write(MySystemConfigurations.PLCStopAddress, false).IsSuccess}");
        //        }
        //    //}
    }
}
