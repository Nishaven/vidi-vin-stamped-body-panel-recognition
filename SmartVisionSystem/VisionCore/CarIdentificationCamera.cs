// ============================================================
// 文件：CarIdentificationCamera.cs
// 模块：SmartVisionSystem.VisionCore（视觉核心 - 车型识别相机）
// 说明：车型识别相机类，继承 CameraBase。负责海康相机取图、ViDi Blue-Locate 深度学习
//       车型与返修判定，并按车型级联触发对应 VIN 读码相机、把结果写回 PLC。
// ============================================================
using Cognex.VisionPro;
using HslCommunication.Core;
using MvCamCtrl.NET;
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.UIForm.Canvas;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using ViDi2.Runtime;
using ViDi2.VisionPro;
using static MvCamCtrl.NET.MyCamera;
using Image = ViDi2.VisionPro.Image;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// 车型识别相机：VIN 读码流程的第一级。收到触发后采集车身图像，
    /// 用 ViDi Blue-Locate 判定车型（A09/T58/A39/T68/A66/V22 等）与是否需要返修，
    /// 再按车型把曝光/增益配方下发并级联触发对应 VIN 读码相机，同时向 PLC 写返修/车型/OK/NG 信号。
    /// </summary>
    public class CarIdentificationCamera : CameraBase
    {
        /// <summary>相机页面上本相机的图像显示与识别结果标签控件</summary>
        public ImageViDiDisplay CurrentImageDisplay;
        // 光源控制器串口（文本协议 S<通道><亮度>#，19200/N/8/1，由 MainForm 注入）
        public SerialPort mSerialPort;
        // 本次结果图像的保存路径（按 日期/相机/OK或NG 分目录）
        private string SaveImagePath;
        // 级联VIN读码相机1：A09/T58/A39 车型触发（由 MainForm 注入）
        public VINCodeCamera1 mVINCodeCamera1;
        // 级联VIN读码相机2：T68/A66 车型触发（由 MainForm 注入）
        public VINCodeCamera2 mVINCodeCamera2;
        // 级联VIN读码相机3：Camera2 读码失败后的备用相机（由 MainForm 注入）
        public VINCodeCamera3 mVINCodeCamera3;
        // 级联VIN读码相机4：Camera1 读码失败后的备用相机（由 MainForm 注入）
        public VINCodeCamera4 mVINCodeCamera4;
        // 级联VIN读码相机5：V22 车型触发（由 MainForm 注入）
        public VINCodeCamera5 mVINCodeCamera5;
        //2026.9 V22-G
        // 级联VIN读码相机6/7/8：V22-G 新增工位相机（由 MainForm 注入）
        public VINCodeCamera6 mVINCodeCamera6;
        public VINCodeCamera7 mVINCodeCamera7;
        public VINCodeCamera8 mVINCodeCamera8;

        // ViDi "默认"流：车型识别（Blue-Locate 定位工具）
        private IStream mStream;
        // ViDi "1返修"流：返修判定（Blue-Locate 定位工具）
        private IStream mStreamFX;
        // 当前执行的 ViDi 工具（Locate，变量名沿用历史叫法）
        private ITool mBlueReadTool;
        // 车型识别结果文本（"车型:XXX" 或 "车型识别失败"）
        private string LabelResult = string.Empty;
        // 返修判定结果：true=不用返修，false=需要返修
        private bool ResultFX;
        // 返修特征名（"不用返修"或具体返修特征名），静态字段供外部读取
        public static string FX;
        //private ushort FXOK = 0;
        //private ushort FXNG = 1;
        // 按读码相机编号索引的曝光时间配方（CameraExposureN 对应 VINCodeCameraN 的配置，
        // 识别到车型后随触发下发给对应读码相机）
        public double CameraExposure1;
        public double CameraExposure2;
        public double CameraExposure3;
        public double CameraExposure4;
        public double CameraExposure5;
        public double CameraExposure6;
        public double CameraExposure7;
        public double CameraExposure8;
        public double CameraExposure9;
        public double CameraExposure10;

        // 按读码相机编号索引的增益配方（CameraGainN 对应 VINCodeCameraN 的配置）
        public double CameraGain1;
        public double CameraGain2;
        public double CameraGain3;
        public double CameraGain4;
        public double CameraGain5;
        public double CameraGain6;
        public double CameraGain7;
        public double CameraGain8;
        public double CameraGain9;
        public double CameraGain10;

        //2026.9 V22-G-s
        // V22-G 新增车型的曝光配方（对应 VINCodeCamera11~16 及 99 号配置）
        public double CameraExposure11;
        public double CameraExposure12;
        public double CameraExposure13;
        public double CameraExposure14;
        public double CameraExposure15;
        public double CameraExposure16;
        public double CameraExposure99;
        // V22-G 新增车型的增益配方（对应 VINCodeCamera11~16 及 99 号配置）
        public double CameraGain11;
        public double CameraGain12;
        public double CameraGain13;
        public double CameraGain14;
        public double CameraGain15;
        public double CameraGain16;
        public double CameraGain99;
        //2026.9 V22-G-e

        /// <summary>
        /// 三菱PLC通信读写接口
        /// </summary>
        public IReadWriteNet mMelsecMcNet;
        //private int CurrentTriggerValue, BeforeTriggerValue;
        /// <summary>
        /// 构造车型识别相机：查找并打开海康相机、设置初始曝光/增益，
        /// 加载 ViDi 视觉工具目录，并启动最高优先级的 STA 图像处理线程。
        /// </summary>
        /// <param name="systemConfigurations">系统配置（含 PLC 信号地址等）</param>
        /// <param name="cameraParameters">本相机参数（名称、序列号、曝光、增益、触发模式等）</param>
        /// <param name="control">ViDi 运行时控制接口（MainForm 创建后注入）</param>
        public CarIdentificationCamera(SystemConfigurations systemConfigurations, CameraParameters cameraParameters, IControl control) : base(systemConfigurations, cameraParameters)
        {
            //CameraListProductNmae
            MyCameraParameters = cameraParameters;
            mSystemConfigurations = systemConfigurations;
            Init();
            Log.Info($"{MyCameraParameters.CameraName}设置曝光时间返回值:{CurrentCamera.MV_CC_SetExposureTime_NET(((float)MyCameraParameters.CameraExposure) * 1000)}");
            Log.Info($"{MyCameraParameters.CameraName}设置增益返回值:{CurrentCamera.MV_CC_SetGain_NET((float)MyCameraParameters.CameraGain)}");
            // 拼接 exe 上级目录下本相机的 ViDi 工具包路径并加载
            string CameraVisionToolBlockPath = Path.GetFullPath("..") + $"\\CameraVisionTool\\{MyCameraParameters.CameraName}";
            MyVisionTools = new VisionTools(CameraVisionToolBlockPath, MyCameraParameters.CameraName, control);
            // 创建本相机的图像处理线程：后台 STA 线程（VisionPro 控件要求）、最高优先级，
            // 平时阻塞在 mAutoResetEvent，由取图回调唤醒
            Thread thread = new Thread(RunCameraImage)
            {
                IsBackground = true,
                Name = "ClassifyCameraThread",
                Priority = ThreadPriority.Highest
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// 初始化相机：按序列号枚举查找设备并打开（打开时注册取图回调并开始采集）。
        /// </summary>
        public override void Init()
        {
            SearchCamera();
            OpenCamera();
        }

        /// <summary>
        /// 相机图像处理主循环（STA 线程）：从队列取图后先跑"1返修"流判定是否返修，
        /// 再跑"默认"流识别车型并刷新 UI，随后按车型级联触发对应 VIN 读码相机、
        /// 向 PLC 写返修/车型/OK/NG 信号，最后把图像加入异步存档队列。
        /// </summary>
        public override void RunCameraImage()
        {
            // 常驻循环：有图像就处理，没有则阻塞在 mAutoResetEvent 等待取图回调唤醒
            while (true)
            {
                if (ImageQueue.Count > 0)
                {
                    try
                    {
                        // 取出一帧图像并清空叠加图层
                        ImageQueue.TryDequeue(out CurrentImage);
                        ListGraphic.Clear();
                        // 实时预览模式下只显示不判定，检测流程仅在非预览时执行
                        if (!StartLiveImage)
                        {
                            // 每帧先复位车型与返修状态，避免沿用上一帧结果
                            CarModel = "未识别";
                            FX = "不用返修";
                            // 统计单帧推理耗时
                            Stopwatch mStopwatch = new Stopwatch();
                            mStopwatch.Start();
                            // 把 VisionPro 图像包装为 ViDi 样本图像供深度学习推理
                            using (var image = new Image(CurrentImage))
                            {
                                //C2岗返修检测，不用返修true，返修false
                                {
                                    // 取"1返修"流，创建样本并执行 Locate 工具推理
                                    mStreamFX = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams["1返修"];
                                    using (ViDi2.ISample mSampleFX = mStreamFX.CreateSample(image))
                                    {
                                        mBlueReadTool = mStreamFX.Tools["Locate"];
                                        mSampleFX.Process(mBlueReadTool);
                                        //mStopwatch.Stop();
                                        // 先默认按需要返修处理，随后根据匹配结果修正
                                        ResultFX = false;
                                        // 遍历返修流的匹配视图：唯一匹配且命中 1 个特征名 → 该特征即返修类型；
                                        // 匹配数或特征数异常 → 按不用返修处理
                                        foreach (ViDi2.IBlueView view in mSampleFX.Markings[mBlueReadTool.Name].Views)
                                        {
                                            if (view.Matches.Count == 1)
                                            {
                                                foreach (ViDi2.ILayoutModelMatch match in view.Matches)
                                                {
                                                    if (match.Features.Count == 1)
                                                    {
                                                        foreach (ViDi2.IFeature feature in match.Features)
                                                        {
                                                            ResultFX = false;
                                                            FX = feature.Name;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ResultFX = true;
                                                        FX = "不用返修";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ResultFX = true;
                                                FX = "不用返修";
                                            }
                                        }
                                        // 兜底：未取到有效返修特征名时一律按不用返修处理
                                        if (FX == "不用返修" || FX == "") ResultFX = true;
                                    }
                                }
                               
                                {
                                    // 取"默认"流做车型识别：创建样本并执行 Locate 工具推理
                                    mStream = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams["默认"];
                                    using (ViDi2.ISample mSample = mStream.CreateSample(image))
                                    {
                                        mBlueReadTool = mStream.Tools["Locate"];
                                        mSample.Process(mBlueReadTool);
                                        mStopwatch.Stop();
                                        // 先默认识别失败，命中车型特征后再置成功
                                        Result = false;
                                        // 遍历车型流的匹配视图：唯一匹配且特征数为 1 时命中车型（特征名即车型名），
                                        // 否则判定车型识别失败
                                        foreach (ViDi2.IBlueView view in mSample.Markings[mBlueReadTool.Name].Views)
                                        {
                                            if (view.Matches.Count == 1)
                                            {
                                                foreach (ViDi2.ILayoutModelMatch match in view.Matches)
                                                {
                                                    if (match.Features.Count == 1)
                                                    {
                                                        foreach (ViDi2.IFeature feature in match.Features)
                                                        {
                                                            Result = true;
                                                            CarModel = feature.Name;
                                                            LabelResult = $"车型:{CarModel}";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Result = false;
                                                        LabelResult = "车型识别失败";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Result = false;
                                                LabelResult = "车型识别失败";
                                            }
                                        }
                                        // 车型名为异常值（error/未识别/空）时强制判定为失败
                                        if (CarModel == "error" || CarModel == "ERROR" || CarModel == "未识别" || CarModel == "") Result = false;
                                        // 跨线程回写 UI：异步刷新识别结果标签（车型+返修，通过绿/红显示）与 ViDi 样本视图
                                        if (CurrentImageDisplay.InvokeRequired)
                                        {
                                            CurrentImageDisplay.BeginInvoke(new Action(() =>
                                            {
                                                //加上返修结果显示
                                                CurrentImageDisplay.LabelResult.ForeColor = Result&&ResultFX ? Color.Green : Color.Red;
                                                CurrentImageDisplay.LabelResult.Text = LabelResult+ FX;
                                                CurrentImageDisplay.mSampleViewer.ViewModel.ViewIndex = 0;
                                                CurrentImageDisplay.mSampleViewer.ViewModel.Sample = mSample;
                                            }));
                                        }
                                    }
                                }
                            }
                            // 记录本帧检测耗时与两项识别结果日志，便于产线追溯
                            mStopwatch.Stop();
                            Log.Info($@"检测处理时间：{mStopwatch.ElapsedMilliseconds}ms");
                            //加上返修结果显示
                            Log.Info($"车型返修识别结果为:{ResultFX}，{FX}");
                            Log.Info($"车型识别结果为:{Result}，{CarModel}");
                            //if (CarModel == "A09" || CarModel == "A09_" || CarModel == "T58" || CarModel == "T58_" || CarModel == "A39" || CarModel == "A39_")
                            // 级联分支一：A09/T58/A39 车型 → 触发 VINCodeCamera1 读码
                            if (CarModel == "A09" || CarModel == "T58" || CarModel == "A39")
                            {
                                // 按车型给读码相机1下发对应的曝光/增益配方（A09→配方1）
                                if (CarModel == "A09")
                                {
                                    mVINCodeCamera1.CarModel = "A09";
                                    mVINCodeCamera1.Exposure = CameraExposure1;
                                    mVINCodeCamera1.Gain = CameraGain1;
                                }
                                //else if (CarModel == "A09_")
                                //{
                                //    mVINCodeCamera1.CarModel = "A09_";
                                //    mVINCodeCamera1.Exposure = CameraExposure6;
                                //    mVINCodeCamera1.Gain = CameraGain6;
                                //}
                                // T58 车型：配方2
                                else if (CarModel == "T58")
                                {
                                    mVINCodeCamera1.CarModel = "T58";
                                    mVINCodeCamera1.Exposure = CameraExposure2;
                                    mVINCodeCamera1.Gain = CameraGain2;
                                }
                                //else if (CarModel == "T58_")
                                //{
                                //    mVINCodeCamera1.CarModel = "T58_";
                                //    mVINCodeCamera1.Exposure = CameraExposure7;
                                //    mVINCodeCamera1.Gain = CameraGain7;
                                //}
                                // A39 车型：配方3
                                else if (CarModel == "A39")
                                {
                                    mVINCodeCamera1.CarModel = "A39";
                                    mVINCodeCamera1.Exposure = CameraExposure3;
                                    mVINCodeCamera1.Gain = CameraGain3;
                                }
                                //else if (CarModel == "A39_")
                                //{
                                //    mVINCodeCamera1.CarModel = "A39_";
                                //    mVINCodeCamera1.Exposure = CameraExposure8;
                                //    mVINCodeCamera1.Gain = CameraGain8;
                                //}
                                // 软件触发 VIN 码读码相机1 采集（开灯→软触发→切回硬触发源）
                                mVINCodeCamera1.SoftWareTrigger();
                                Log.Info("VINCodeCamera1相机触发采集图片");
                            }
                            // 级联分支二：T68/A66 车型（含带"_"的返修车）→ 触发 VINCodeCamera2 读码
                            else if (CarModel == "T68" || CarModel == "T68_" || CarModel == "A66" || CarModel == "A66_")
                            {
                                // 按车型给读码相机2下发配方：普通车配方4、返修车（带"_"）配方9
                                if (CarModel == "T68")
                                {
                                    mVINCodeCamera2.CarModel = "T68";
                                    mVINCodeCamera2.Exposure = CameraExposure4;
                                    mVINCodeCamera2.Gain = CameraGain4;
                                }
                                else if (CarModel == "T68_")
                                {
                                    mVINCodeCamera2.CarModel = "T68_";
                                    mVINCodeCamera2.Exposure = CameraExposure9;
                                    mVINCodeCamera2.Gain = CameraGain9;
                                }
                                else if (CarModel == "A66")
                                {
                                    mVINCodeCamera2.CarModel = "A66";
                                    mVINCodeCamera2.Exposure = CameraExposure4;
                                    mVINCodeCamera2.Gain = CameraGain4;
                                }
                                else if (CarModel == "A66_")
                                {
                                    mVINCodeCamera2.CarModel = "A66_";
                                    mVINCodeCamera2.Exposure = CameraExposure9;
                                    mVINCodeCamera2.Gain = CameraGain9;
                                }
                                // 软件触发 VIN 码读码相机2 采集
                                mVINCodeCamera2.SoftWareTrigger();
                                Log.Info("VINCodeCamera2相机触发采集图片");
                            }
                            // 级联分支三：V22 车型 → 触发 VINCodeCamera5 读码（配方10）
                            else if (CarModel == "V22")
                            {
                                mVINCodeCamera5.CarModel = "V22";
                                mVINCodeCamera5.Exposure = CameraExposure10;
                                mVINCodeCamera5.Gain = CameraGain10;
                                mVINCodeCamera5.SoftWareTrigger();
                                Log.Info("VINCodeCamera5相机触发采集V22图片");
                            }
                           
                            // 自动模式下才向 PLC 输出本工位结果（手动模式不写 PLC）
                            if (mMelsecMcNet != null && IsAutoStatus)
                            {
                                //新增返修结果输出给PLC，不用返修发false ushort FXOK=0 ，需要返修发 true ushort FXNG= 1.
                                if (ResultFX)
                                {
                                    Log.Info($"返修识别结果输出OK信号，信号地址：{mSystemConfigurations.CarModelCameraFXAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraFXAddress, false).IsSuccess}");
                                }
                                else
                                {
                                    Log.Info($"返修识别结果输出NG信号，信号地址：{mSystemConfigurations.CarModelCameraFXAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraFXAddress, true).IsSuccess}");
                                }
                                // 识别成功：写车型字符串 + OK 脉冲信号（True→保持300ms→False，供 PLC 捕获脉冲沿）
                                if (Result)
                                {
                                    Log.Info($"车型识别相机输出车型信号，信号地址：{mSystemConfigurations.CarModelCameraCarModelAddress}写入值:{CarModel},写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraCarModelAddress, CarModel).IsSuccess}");
                                    Log.Info($"车型识别相机输出OK信号，信号地址：{mSystemConfigurations.CarModelCameraOKAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraOKAddress, true).IsSuccess}");
                                    Thread.Sleep(300);
                                    Log.Info($"车型识别相机输出OK信号，信号地址：{mSystemConfigurations.CarModelCameraOKAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraOKAddress, false).IsSuccess}");
                                }
                                // 识别失败：写 NG 脉冲信号（True→保持300ms→False）
                                else
                                {
                                    Log.Info($"车型识别相机输出NG信号，信号地址：{mSystemConfigurations.CarModelCameraNGAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraNGAddress, true).IsSuccess}");
                                    Thread.Sleep(300);
                                    Log.Info($"车型识别相机输出NG信号，信号地址：{mSystemConfigurations.CarModelCameraNGAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CarModelCameraNGAddress, false).IsSuccess}");
                                }
                            }
                            #region 保存OK图像
                            // OK 图存档：按 日期/相机名/OK/车型 分目录，文件名带返修信息
                            if (MyCameraParameters.IsSaveOKImage && Result)
                            {
                                string time = DateTime.Now.ToString("HH-mm-ss");
                                SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\OK\\" + $"{CarModel}\\";
                                // 图像丢入异步存档队列，避免写盘阻塞检测线程
                                ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss")+ "-" + FX + ".bmp", (ICogImage)CurrentImage, 0);
                            }
                            // NG 图存档：按 日期/相机名/NG 分目录
                            if (MyCameraParameters.IsSaveNGImage && !Result)
                            {
                                string time = DateTime.Now.ToString("HH-mm-ss");
                                SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\NG\\";
                                // 图像丢入异步存档队列，避免写盘阻塞检测线程
                                ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss")+ "-" + FX + ".bmp", (ICogImage)CurrentImage, 0);
                            }
                            #endregion
                        }
                    }
                    catch (Exception e)
                    {
                        // 异常兜底：记录日志后阻塞等待下次取图唤醒，防止线程空转刷屏
                        Log.Error(e.Message + e.StackTrace);
                        mAutoResetEvent.WaitOne();
                    }
                }
                // 队列为空：阻塞等待取图回调 Set 唤醒
                else mAutoResetEvent.WaitOne();
            }
        }

        /// <summary>
        /// 打开光源
        /// </summary>
        public void OpenLight()
        {
            try
            {
                if (mSerialPort != null)
                {
                    mSerialPort.Write("SA0255#");
                    Log.Info("光源点亮成功");
                }
            }
            catch (Exception ex)
            {
                Log.Error($@"光源设置亮度为255出错，报错信息：{ex}");
            }
        }

        /// <summary>
        /// 关闭光源
        /// </summary>
        public void CloseLight()
        {
            try
            {
                if (mSerialPort != null)
                {
                    mSerialPort.Write("SA0000#");
                    Log.Info("光源关闭成功");
                }
            }
            catch (Exception ex)
            {
                Log.Error($@"光源设置亮度为00出错，报错信息：{ex}");
            }
        }

        /// <summary>
        /// 软件触发采集一帧：开灯 → 临时切换为软件触发源 → 执行一次软触发 → 切回硬件触发源(LINE0)。
        /// </summary>
        public override void SoftWareTrigger()
        {
            try
            {
                if (CurrentCamera != null)
                {
                    // 先点亮光源，稍等 10ms 让亮度稳定
                    OpenLight();
                    Thread.Sleep(10);
                    // 切到软件触发源执行一帧采集，完成后切回 LINE0 硬件触发源，保证 PLC/GPIO 仍可触发
                    int nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    nRet1 = CurrentCamera.MV_CC_TriggerSoftwareExecute_NET();
                    nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                    Log.Info($"车型识别相机采集信息反馈：{nRet1}");
                }
                else
                {
                    Log.Error("当前相机不存在，请检查相机供电和相机的网络连接!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 备用软触发采集：开灯后延时 200ms 等待亮度稳定，再直接执行软触发（不切换触发源）。
        /// </summary>
        public void SoftWareTrigger2()
        {
            try
            {
                if (CurrentCamera != null)
                {
                    // 点亮光源后延时 200ms，确保曝光时亮度已稳定
                    OpenLight();
                    Thread.Sleep(200);
                    //int nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    int nRet1 = CurrentCamera.MV_CC_TriggerSoftwareExecute_NET();
                    Log.Info($"车型识别 相机采集信息反馈：{nRet1}");
                }
                else
                {
                    Log.Error("当前相机不存在，请检查相机供电和相机的网络连接!");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        #region 相机SDK

        // 海康 SDK 取图回调委托（静态持有，防止委托被 GC 回收导致回调失效）
        public static MyCamera.cbOutputExdelegate ImageCallback;
        /// <summary>
        /// 查找相机设备
        /// </summary>
        public void SearchCamera()
        {
            MV_CC_DEVICE_INFO_LIST m_pDeviceList = new MV_CC_DEVICE_INFO_LIST();
            int nRet = MV_CC_EnumDevices_NET(MV_GIGE_DEVICE, ref m_pDeviceList);
            bool IsFind = false;
            if (MV_OK != nRet)
            {
                Log.Error("Enum Devices Fail");
                return;
            }
            // ch:在窗体列表中显示设备名 || Display the device'name on window's list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                CurrentDevice = (MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MV_CC_DEVICE_INFO));

                // GigE 设备：解析网卡设备信息结构
                if (CurrentDevice.nTLayerType == MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(CurrentDevice.SpecialInfo.stGigEInfo, 0);
                    MV_GIGE_DEVICE_INFO gigeInfo = (MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MV_GIGE_DEVICE_INFO));
                    // 用配置的相机序列号匹配本工位相机
                    if (gigeInfo.chSerialNumber == MyCameraParameters.CameraSerialNum)
                    {
                        CurrentDevice = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                        Log.Info($"找到{MyCameraParameters.CameraName},相机SN:{gigeInfo.chDeviceVersion},固件版本:{gigeInfo.chDeviceVersion}");
                        break;
                    }
                    if (!IsFind && i == m_pDeviceList.nDeviceNum - 1)
                    {
                        Log.Info($"找不到{MyCameraParameters.CameraName},相机SN:{gigeInfo.chDeviceVersion},请检查相机连接!");
                    }
                }
                // USB3 设备：解析 USB 设备信息结构后按序列号匹配
                else if (CurrentDevice.nTLayerType == MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(CurrentDevice.SpecialInfo.stUsb3VInfo, 0);
                    MV_USB3_DEVICE_INFO usbInfo = (MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MV_USB3_DEVICE_INFO));
                    if (usbInfo.chSerialNumber == MyCameraParameters.CameraSerialNum)
                    {
                        Log.Info($"找到{MyCameraParameters.CameraName},相机SN:{usbInfo.chDeviceVersion},固件版本:{usbInfo.chDeviceVersion}");
                        break;
                    }
                    else
                    {
                        Log.Info($"找不到{MyCameraParameters.CameraName},相机SN:{usbInfo.chDeviceVersion},请检查相机连接!");
                    }
                }
            }
        }
        /// <summary>
        /// 打开对应相机设备
        /// </summary>
        public void OpenCamera()
        {
            CurrentCamera = new MyCamera();

            if (MV_OK != CurrentCamera.MV_CC_CreateDevice_NET(ref CurrentDevice))
            {
                Log.Error($"创建{MyCameraParameters.CameraName}工站相机设备失败！");
                return;
            }
            // ch:打开设备 | en:Open device
            if (MV_OK != CurrentCamera.MV_CC_OpenDevice_NET())
            {
                CurrentCamera.MV_CC_DestroyDevice_NET();
                Log.Error($"打开{MyCameraParameters.CameraName}工站相机设备失败！");
                return;
            }

            // ch:获取包大小 || en: Get Payload Size
            MVCC_INTVALUE stParam = new MVCC_INTVALUE();
            if (MV_OK != CurrentCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam))
            {
                Log.Warn($"获取{MyCameraParameters.CameraName}包大小失败！");
                return;
            }
            PayloadSize = stParam.nCurValue;

            // ch:获取高 || en: Get Height
            if (MV_OK != CurrentCamera.MV_CC_GetIntValue_NET("Height", ref stParam))
            {
                Log.Warn($"获取{MyCameraParameters.CameraName}图像高失败！");
                return;
            }
            uint nHeight = stParam.nCurValue;

            // ch:获取宽 || en: Get Width
            if (MV_OK != CurrentCamera.MV_CC_GetIntValue_NET("Width", ref stParam))
            {
                Log.Warn($"获取{MyCameraParameters.CameraName}图像宽失败！");
                return;
            }
            uint nWidth = stParam.nCurValue;
            // 图像大小
            ImageSize = nWidth * nHeight;
            // 预分配非托管缓冲区（宽*高*3 字节），供取图回调中做像素格式转换
            ImageBuffer = Marshal.AllocHGlobal((int)(ImageSize) * 3);

            // ch:设置触发模式为off || en:set trigger mode as off
            CurrentCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            CurrentCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            //CurrentCamera.MV_CC_SetTriggerDelay_NET(20);//设置触发延时100us
            // 按配置选择触发源：自动模式用硬件 LINE0（PLC/GPIO 触发），手动模式用软件触发
            if (MyCameraParameters.CameraTriggerMode == "Auto")
            {

                CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
            }
            else if (MyCameraParameters.CameraTriggerMode == "Manual")
            {
                CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            }
            //if (MV_OK != CurrentCamera.MV_CC_SetIntValueEx_NET("LineDebouncerTime", 50))
            //{
            //    Log.Warn($"{MyCameraParameters.CameraName}防抖参数设置失败！");
            //    return;
            //}
            // ch:注册回调函数 | en:Register image callback
            ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
            int nRet = CurrentCamera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
            if (MyCamera.MV_OK != nRet)
            {
                Log.Warn($"{MyCameraParameters.CameraName}Register image callback failed！");
            }
            // ch:开启抓图 || en: start grab image
            CurrentCamera.MV_CC_SetImageNodeNum_NET(10);
            if (MV_OK != CurrentCamera.MV_CC_StartGrabbing_NET())
            {
                Log.Warn($"{MyCameraParameters.CameraName}开始采集图像宽失败！");
                return;
            }
        }
        /// <summary>
        /// 图像是否为Mono格式
        /// </summary>
        /// <param name="enType"></param>
        /// <returns></returns>
        private bool IsMonoPixelFormat(MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 图像是否为彩色
        /// </summary>
        /// <param name="enType"></param>
        /// <returns></returns>
        private bool IsColorPixelFormat(MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                case MvGvspPixelType.PixelType_Gvsp_RGBA8_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BGRA8_Packed:
                case MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 其他黑白格式转为Mono8
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pInData">输出图片数据</param>
        /// <param name="pOutData">输出图片数据</param>
        /// <param name="nHeight">高</param>
        /// <param name="nWidth">宽</param>
        /// <param name="nPixelType">像素格式</param>
        /// <returns></returns>
        public Int32 ConvertToMono8(object obj, IntPtr pInData, IntPtr pOutData, ushort nHeight, ushort nWidth, MyCamera.MvGvspPixelType nPixelType)
        {
            if (IntPtr.Zero == pInData || IntPtr.Zero == pOutData)
            {
                return MV_E_PARAMETER;
            }
            int nRet = MV_OK;
            MyCamera device = obj as MyCamera;
            MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MV_PIXEL_CONVERT_PARAM();
            stPixelConvertParam.pSrcData = pInData;//源数据
            if (IntPtr.Zero == stPixelConvertParam.pSrcData)
            {
                return -1;
            }
            stPixelConvertParam.nWidth = nWidth;//图像宽度
            stPixelConvertParam.nHeight = nHeight;//图像高度
            stPixelConvertParam.enSrcPixelType = nPixelType;//源数据的格式
            stPixelConvertParam.nSrcDataLen = (uint)(nWidth * nHeight * ((((uint)nPixelType) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * ((((uint)MvGvspPixelType.PixelType_Gvsp_RGB8_Packed) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.pDstBuffer = pOutData;//转换后的数据
            stPixelConvertParam.enDstPixelType = MvGvspPixelType.PixelType_Gvsp_Mono8;
            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * 3);
            nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
            if (MV_OK != nRet)
            {
                return -1;
            }
            return nRet;
        }
        /// <summary>
        /// 其他彩色格式转为RGB8
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pSrc"></param>
        /// <param name="nHeight"></param>
        /// <param name="nWidth"></param>
        /// <param name="nPixelType"></param>
        /// <param name="pDst"></param>
        /// <returns></returns>
        public Int32 ConvertToRGB(object obj, IntPtr pSrc, ushort nHeight, ushort nWidth, MyCamera.MvGvspPixelType nPixelType, IntPtr pDst)
        {
            if (IntPtr.Zero == pSrc || IntPtr.Zero == pDst)
            {
                return MV_E_PARAMETER;
            }
            int nRet = MV_OK;
            MyCamera device = obj as MyCamera;
            MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MV_PIXEL_CONVERT_PARAM();
            stPixelConvertParam.pSrcData = pSrc;//源数据
            if (IntPtr.Zero == stPixelConvertParam.pSrcData)
            {
                return -1;
            }
            stPixelConvertParam.nWidth = nWidth;//图像宽度
            stPixelConvertParam.nHeight = nHeight;//图像高度
            stPixelConvertParam.enSrcPixelType = nPixelType;//源数据的格式
            stPixelConvertParam.nSrcDataLen = (uint)(nWidth * nHeight * ((((uint)nPixelType) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * ((((uint)MvGvspPixelType.PixelType_Gvsp_RGB8_Packed) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.pDstBuffer = pDst;//转换后的数据
            stPixelConvertParam.enDstPixelType = MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            stPixelConvertParam.nDstBufferSize = (uint)nWidth * nHeight * 3;
            nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
            if (MV_OK != nRet)
            {
                return -1;
            }

            return MV_OK;
        }
        /// <summary>
        /// 显示图片
        /// </summary>
        /// <param name="nHeight">高</param>
        /// <param name="nWidth">宽</param>
        /// <param name="pImageBuf">图片数据</param>
        /// <param name="enPixelType">像素格式</param>
        public void VisionProDisplay(UInt32 nHeight, UInt32 nWidth, IntPtr pImageBuf, MvGvspPixelType enPixelType)
        {
            // ch: 显示 || display
            try
            {
                // Mono8 灰度图：直接用缓冲构造 CogImage8Grey，缩放后入队
                if (enPixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    CogImage8Root cogImage8Root = new CogImage8Root();
                    cogImage8Root.Initialize((Int32)nWidth, (Int32)nHeight, pImageBuf, (Int32)nWidth, null);
                    CogImage8Grey cogImage8Grey = new CogImage8Grey();
                    cogImage8Grey.SetRoot(cogImage8Root);
                    ImageQueue.Enqueue(cogImage8Grey.ScaleImage((int)nWidth, (int)nHeight));
                }
                // 彩色图：按 R/G/B 三个平面（各偏移一个 ImageSize）构造 24 位平面彩色图后入队
                else
                {
                    CogImage8Root image0 = new CogImage8Root();
                    IntPtr ptr0 = new IntPtr(pImageBuf.ToInt64());
                    image0.Initialize((int)nWidth, (int)nHeight, ptr0, (int)nWidth, null);

                    CogImage8Root image1 = new CogImage8Root();
                    IntPtr ptr1 = new IntPtr(pImageBuf.ToInt64() + ImageSize);
                    image1.Initialize((int)nWidth, (int)nHeight, ptr1, (int)nWidth, null);

                    CogImage8Root image2 = new CogImage8Root();
                    IntPtr ptr2 = new IntPtr(pImageBuf.ToInt64() + ImageSize * 2);
                    image2.Initialize((int)nWidth, (int)nHeight, ptr2, (int)nWidth, null);

                    CogImage24PlanarColor colorImage = new CogImage24PlanarColor();
                    colorImage.SetRoots(image0, image1, image2);
                    ImageQueue.Enqueue(colorImage.ScaleImage((int)nWidth, (int)nHeight));
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.ToString());
                //MessageBox.Show(ex.ToString());
                return;
            }
            return;
        }

        /// <summary>
        /// 海康 SDK 取图回调（在 SDK 线程执行）：先按像素格式转换（彩色→RGB8 / 黑白→Mono8），
        /// 再转成 VisionPro 图像入 ImageQueue 并唤醒处理线程，最后复位 PLC 触发信号并关灯。
        /// </summary>
        /// <param name="pData">SDK 返回的图像数据指针</param>
        /// <param name="pFrameInfo">帧信息（宽高、像素格式等）</param>
        /// <param name="pUser">用户自定义参数（未使用）</param>
        public void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            // 默认按彩色处理；pTemp 为转换后的数据指针（原始数据或 ImageBuffer）
            MvGvspPixelType pixelType = MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            IntPtr pTemp = IntPtr.Zero;
            if (IsColorPixelFormat(pFrameInfo.enPixelType))    // 彩色图像处理
            {
                if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    // 已是 RGB8：无需转换，直接使用原始数据
                    pTemp = pData;
                }
                else
                {
                    // 其他格式彩色图像转为RGB
                    int nRet = ConvertToRGB(CurrentCamera, pData, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType, ImageBuffer);
                    if (MV_OK != nRet)
                    {
                        return;
                    }
                    pTemp = ImageBuffer;
                }
                // Packed转Plane
                //unsafe
                //{
                //    byte* pBufForSaveImage = (byte*)pTemp;

                //    UInt32 nSupWidth = (pFrameInfo.nWidth + (UInt32)3) & 0xfffffffc;

                //    for (int nRow = 0; nRow < pFrameInfo.nHeight; nRow++)
                //    {
                //        for (int col = 0; col < pFrameInfo.nWidth; col++)
                //        {
                //            byteArrImageData[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col)];
                //            byteArrImageData[pFrameInfo.nWidth * pFrameInfo.nHeight + nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 1)];
                //            byteArrImageData[pFrameInfo.nWidth * pFrameInfo.nHeight * 2 + nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 2)];
                //        }
                //    }
                //    pTemp = Marshal.UnsafeAddrOfPinnedArrayElement(byteArrImageData, 0);
                //}
            }
            else if (IsMonoPixelFormat(pFrameInfo.enPixelType))    // Mono图像处理
            {
                if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    // 已是 Mono8：无需转换，直接使用原始数据
                    pTemp = pData;
                }
                else
                {
                    // 其他格式Mono转为Mono8
                    int nRet = ConvertToMono8(CurrentCamera, pData, ImageBuffer, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType);
                    if (MV_OK != nRet)
                    {
                        return;
                    }
                    pTemp = ImageBuffer;
                }
                // 黑白图统一标记为 Mono8，供显示环节选择灰度构造方式
                pixelType = MvGvspPixelType.PixelType_Gvsp_Mono8;
            }
            
            // 构造 VisionPro 图像并入队
            VisionProDisplay(pFrameInfo.nHeight, pFrameInfo.nWidth, pTemp, pixelType);

            // 唤醒本相机的处理线程开始检测
            if (ImageQueue.Count > 0)
            {
                mAutoResetEvent.Set();
                if (!StartLiveImage)
                    Log.Info($@"车型识别相机收到触发拍照信号,图像队列数量：{ImageQueue.Count}");
            }
            // 满足配置条件时向 PLC 写 False 复位相机触发信号，避免重复触发
            if (mMelsecMcNet != null && mSystemConfigurations.ISAutoSaveImage) Log.Info($"车型识别相机触发信号复位：{mSystemConfigurations.CameraTriggerAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.CameraTriggerAddress, false).IsSuccess}");
            // 采集完成立即关灯，降低光源损耗
            CloseLight();
        }
        /// <summary>
        /// 关闭相机设备
        /// </summary>
        public void CloseCamera()
        {
            StartLiveImage = false;
            // ch:停止抓图 || en:Stop grab image
            CurrentCamera.MV_CC_StopGrabbing_NET();
            Thread.Sleep(10);
            // ch:关闭设备 || en: Close device
            CurrentCamera.MV_CC_CloseDevice_NET();
            CurrentCamera.MV_CC_DestroyDevice_NET();
            Thread.Sleep(10);
        }
        #endregion
    }
}
