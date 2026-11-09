// ============================================================
// 文件：VINCodeCamera5.cs
// 模块：VisionCore(VIN 码读码相机)
// 说明：VIN 码读码相机 5，服务 V22/Type2 车型工位；由车型相机直接触发，
//       识别失败时级联触发 VINCodeCamera6 进行二次补拍识别。
// ============================================================
using Cognex.VisionPro;
using HslCommunication;
using HslCommunication.Core;
using MvCamCtrl.NET;
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.UIForm.Canvas;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ViDi2.Runtime;
using static MvCamCtrl.NET.MyCamera;
using Image = ViDi2.VisionPro.Image;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// VIN 码读码相机 5 类
    /// 职责：整车制造 VIN 码智能视觉读码，服务 V22/Type2 车型工位；
    ///       由车型相机直接触发，ViDi Blue-Read 深度学习 OCR 识别 VIN 码；
    ///       识别失败时级联触发 VINCodeCamera6 进行二次补拍。
    /// </summary>
    public class VINCodeCamera5 : CameraBase
    {
        /// <summary>
        /// ViDi 图像显示控件（绑定 SampleViewer 与结果标签）
        /// </summary>
        public ImageViDiDisplay CurrentImageDisplay;
        /// <summary>
        /// 光源控制器串口（SD0255# 亮度控制）
        /// </summary>
        public SerialPort mSerialPort;
        private string SaveImagePath;
        public string VINCode = string.Empty;
        public string Readcode = string.Empty;
        /// <summary>
        /// SQLite 数据库操作助手，写入 ResultRecords 识别结果表
        /// </summary>
        public SQLiteHelper SQLiteHelperResult;
        public ConcurrentQueue<VINCodeResult> VINCodeResultQueue;
        private readonly ArrayList SQLiteResultDatas = new ArrayList();
        private VINCodeResult mVINCodeResult;
        public UIDataGridView ResultDataGridView;
        private ViDi2.Runtime.IStream mStream;
        private ViDi2.Runtime.ITool mBlueReadTool;
        private string LabelResult = string.Empty;

        //2026.9 V22-G-s
        //相机5引用相机6、曝光、增益参数申明
        /// 级联备用 VINCodeCamera6 引用（本相机识别失败时触发其二次补拍）
        /// 本相机（VIN5）曝光时间配方值（微秒，由车型相机注入）
        /// 本相机（VIN5）增益配方值（由车型相机注入）
        /// 级联相机（VIN6）曝光时间配方值
        /// 级联相机（VIN6）增益配方值
        public VINCodeCamera6 mVINCodeCamera6;
        //public double CameraExposure5;
        //public double CameraGain5;
        public double CameraExposure6;
        public double CameraGain6;
        public double CameraExposure7;
        public double CameraGain7;
        //99为通用,无意义
        // 本相机（VIN5）曝光时间配方值（微秒，由车型相机注入）
        // 本相机（VIN5）增益配方值（由车型相机注入）
        public double CameraExposure99;
        public double CameraGain99;
        //2026.9 V22-G-e
        public IReadWriteNet mMelsecMcNet;
        //private int CurrentTriggerValue, BeforeTriggerValue;
        /// <summary>
        /// 构造函数：初始化相机参数、曝光增益、VisionTools 工作区，并启动 STA 图像处理线程
        /// </summary>
        /// <param name="systemConfigurations">系统配置（PLC地址、存图路径等）</param>
        /// <param name="cameraParameters">相机参数（序列号、曝光、增益、触发模式等）</param>
        /// <param name="control">ViDi 运行时控件</param>
        public VINCodeCamera5(SystemConfigurations systemConfigurations, CameraParameters cameraParameters, ViDi2.Runtime.IControl control) : base(systemConfigurations, cameraParameters)
        {

            //CameraListProductNmae

            MyCameraParameters = cameraParameters;
            mSystemConfigurations = systemConfigurations;
            Init();
            Log.Info($"{MyCameraParameters.CameraName}设置曝光时间返回值:{CurrentCamera.MV_CC_SetExposureTime_NET(((float)MyCameraParameters.CameraExposure) * 1000)}");
            Log.Info($"{MyCameraParameters.CameraName}设置增益返回值:{CurrentCamera.MV_CC_SetGain_NET((float)MyCameraParameters.CameraGain)}");
            string CameraVisionToolBlockPath = Path.GetFullPath("..") + $"\\CameraVisionTool\\{MyCameraParameters.CameraName}";
            MyVisionTools = new VisionTools(CameraVisionToolBlockPath, MyCameraParameters.CameraName, control);
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
        /// 初始化相机：枚举查找匹配序列号的相机设备 → 打开相机并开始取流
        /// </summary>
        public override void Init()
        {
            SearchCamera();
            OpenCamera();
        }

        /// <summary>
        /// 主图像处理循环（STA 线程入口）
        /// 流程：从图像队列取图 → ViDi Blue-Read 深度学习 OCR → VIN 码校验 →
        /// 结果入队 + UI 回写 → V22/Type2 车型 NG 时级联触发 Camera6 →
        /// 自动模式下写 PLC + 写 SQLite → 磁盘空间检查 → 按需存图
        /// </summary>
        public override void RunCameraImage()
        {


            // VIN 码校验器实例（独立类实现 ISO 3779 17位 + mod-11 校验）
            VINCodeValidation mVINCodeValidation = new VINCodeValidation();
            while (true)
            {
                if (ImageQueue.Count > 0)
                {
                    try
                    {
                        Stopwatch mStopwatch = new Stopwatch();
                        mStopwatch.Start();
                        ImageQueue.TryDequeue(out CurrentImage);
                        Log.Info($"VIN5相机图像队列移除处理时间：{mStopwatch.ElapsedMilliseconds}ms");
                        //ListGraphic.Clear();
                        //Log.Info($"VIN5相机显示清除时间：{mStopwatch.ElapsedMilliseconds}ms");
                        if (!StartLiveImage)
                        {

                            // 将 VisionPro 图像包装为 ViDi 可处理的 Image 对象
                            using (var image = new Image(CurrentImage))
                            {
                                //mStream = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams[CarModel];
                                ViDi2.ISample mSample = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams.First().CreateSample(image);
                                Result = false;
                                foreach (var stream in MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams)
                                {
                                    //if (stream.Name.Substring(0, 2) != CarModel.Substring(0, 2)) continue;
                                    using (mSample = stream.CreateSample(image))
                                    {
                                        mBlueReadTool = stream.Tools["Read"];
                                        mSample.Process(mBlueReadTool);
                                        foreach (ViDi2.IBlueView view in mSample.Markings[mBlueReadTool.Name].Views)
                                        {
                                            if (view.Matches.Count == 1)
                                            {
                                                foreach (ViDi2.IReadModelMatch match in view.Matches)
                                                {
                                                    VINCode = match.FeatureString;
                                                    if (mVINCodeValidation.Validation(VINCode))
                                                    {
                                                        Result = true;
                                                        LabelResult = $"VIN码:{VINCode}校验OK";
                                                        VINCode = VINCode.Replace("*", "");

                                                    }
                                                    else
                                                    {
                                                        // 校验未通过：标记失败
                                                        Result = false;
                                                        LabelResult = $"VIN码:{VINCode}校验NG";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // 匹配数量不为 1（0 或多个），读取失败
                                                Result = false;
                                                VINCode = string.Empty;
                                                LabelResult = "VIN码读取失败";
                                            }
                                        }
                                        // 当前 Stream 识别成功则跳出循环，不再尝试其他 Stream
                                        if (Result) break;
                                    }
                                }
                                mStopwatch.Stop();
                                Log.Info($"检测处理时间：{mStopwatch.ElapsedMilliseconds}ms,车型:{CarModel},VIN码:{VINCode}");

                                //2026.9 V22-G-s
                                //当结果为False时，切换相机6
                                // V22/Type2 车型分支：识别成功直接出结果，失败则级联触发 Camera6 二次补拍
                                if (CarModel == "V22" | CarModel == "Type2")
                                {
                                    if (Result)
                                    {
                                        //创建一个 VIN 码识别结果对象
                                        mVINCodeResult = new VINCodeResult 
                                        { 
                                            RecordTime = DateTime.Now, 
                                            CameraName = "VINCamera5",
                                            CarModel = CarModel,
                                            FX = CarIdentificationCamera.FX,
                                            VINCode = VINCode,
                                            Result = Result
                                        };
                                        VINCodeResultQueue.Enqueue(mVINCodeResult);     //将识别结果对象放入队列
                                        if (CurrentImageDisplay.InvokeRequired)
                                        {
                                            CurrentImageDisplay.BeginInvoke(new Action(() =>
                                            {   
                                                if (VINCodeResultQueue.Count > 200)
                                                {
                                                    VINCodeResultQueue.ToList().RemoveAt(0);
                                                }
                                                ResultDataGridView.DataSource = VINCodeResultQueue.ToList();
                                                ResultDataGridView.CurrentCell = ResultDataGridView.Rows[ResultDataGridView.Rows.Count - 1].Cells[0];
                                                CurrentImageDisplay.ComboBoxCarModel.SelectedItem = CarModel;
                                                CurrentImageDisplay.LabelResult.ForeColor = Result ? Color.Green : Color.Red;
                                                CurrentImageDisplay.LabelResult.Text = LabelResult;
                                                CurrentImageDisplay.mSampleViewer.ViewModel.ViewIndex = 0;
                                                CurrentImageDisplay.mSampleViewer.ViewModel.Sample = mSample;
                                                
                                            }));
                                        }
                                    }
                                    // 识别失败（NG）：级联触发 VINCodeCamera6 进行二次补拍识别
                                    else
                                    {

                                        // 记录 Camera5 失败结果（相机名标记为"启用VIN6相机"便于追溯级联）
                                        mVINCodeResult = new VINCodeResult 
                                        { 
                                            RecordTime = DateTime.Now, 
                                            CameraName = "启用VIN6相机", 
                                            CarModel = CarModel, 
                                            FX = CarIdentificationCamera.FX, 
                                            VINCode = VINCode, 
                                            Result = Result 
                                        };
                                        VINCodeResultQueue.Enqueue(mVINCodeResult);
                                        if (CurrentImageDisplay.InvokeRequired)
                                        {
                                            CurrentImageDisplay.BeginInvoke(new Action(() =>
                                            {
                                                if (VINCodeResultQueue.Count > 200)
                                                {
                                                    VINCodeResultQueue.ToList().RemoveAt(0);
                                                }
                                                ResultDataGridView.DataSource = VINCodeResultQueue.ToList();
                                                ResultDataGridView.CurrentCell = ResultDataGridView.Rows[ResultDataGridView.Rows.Count - 1].Cells[0];
                                                CurrentImageDisplay.ComboBoxCarModel.SelectedItem = CarModel;
                                                CurrentImageDisplay.LabelResult.ForeColor = Result ? Color.Green : Color.Red;
                                                CurrentImageDisplay.LabelResult.Text = LabelResult;
                                                CurrentImageDisplay.mSampleViewer.ViewModel.ViewIndex = 0;
                                                CurrentImageDisplay.mSampleViewer.ViewModel.Sample = mSample;
                                            }));
                                        }
                                        if (CarModel == "V22")
                                        {
                                            mVINCodeCamera6.CarModel = "V22-G";
                                            mVINCodeCamera6.Exposure = CameraExposure7;
                                            mVINCodeCamera6.Gain = CameraGain7;
                                        }
                                        else if (CarModel == "Type2")
                                        {
                                            //预留
                                        }
                                        //显存回收
                                        Thread.Sleep(500);
                                        mVINCodeCamera6.SoftWareTrigger();
                                        //记录日志
                                        Log.Info("VINCodeCamera6相机触发采集图片");
                                    }
                                }
                                //车型2从下面写入
                                // New2 车型预留分支（当前空实现）
                                else if (CarModel == "New2")
                                {
                                }
                                //2026.9 V22-e



                            }
                            if (IsAutoStatus)
                            {
                                if (mMelsecMcNet != null)
                                {
                                    // 识别成功：写 VIN 码字符串 + OK 脉冲
                                    if (Result)
                                    {
                                        // 写入 VIN 码字符串到 PLC 指定地址
                                        Log.Info($"车型VIN码相机5输出VIN码，信号地址：{mSystemConfigurations.VINCodeCameraVINCodeAddress}写入值:{VINCode},写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraVINCodeAddress, VINCode).IsSuccess}");
                                        
                                        // OK 脉冲：先写 True（上升沿）
                                        Log.Info($"车型VIN码相机5输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, true).IsSuccess}");
                                        // 保持 300ms 高电平
                                        Thread.Sleep(300);
                                        // 再写 False（下降沿），完成一次脉冲
                                        Log.Info($"车型VIN码相机5输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, false).IsSuccess}");
                                    }
                                    // 识别失败：写 NG 脉冲（V22识别NG时已级联相机6补拍，NG结论由相机6输出——对齐T58/A09模式）
                                    else
                                    {
                                        if (CarModel == "V22")
                                        {
                                            Log.Info("V22车型VIN码识别无结果，切换至6号相机拍摄V22-G");
                                        }
                                        if (CarModel != "V22")
                                        {
                                            // NG 脉冲：先写 True（上升沿）
                                            Log.Info($"VIN码相机5输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, true).IsSuccess}");
                                            Thread.Sleep(300);
                                            // 再写 False（下降沿），完成一次脉冲
                                            Log.Info($"VIN码相机5输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, false).IsSuccess}");
                                        }
                                    }
                                    if(!(CarModel=="V22" && !Result))
                                    {
                                    // 作业完成脉冲（无论 OK/NG 都输出），通知 PLC 本工位流程结束
                                    Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, true).IsSuccess}");
                                    Thread.Sleep(300);
                                    Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, false).IsSuccess}");
                                    }
                                    }

                                // 将识别结果写入 SQLite 数据库 ResultRecords 表（事务提交）
                                SQLiteResultDatas.Add($@"INSERT INTO ResultRecords (RecordTime,CameraName,CarModel,VINCode,Result) VALUES ('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','{mVINCodeResult.CameraName}','{mVINCodeResult.CarModel}','{mVINCodeResult.VINCode}','{mVINCodeResult.Result}')");
                                SQLiteHelperResult.ExecuteSqlTran(SQLiteResultDatas);
                                SQLiteResultDatas.Clear();
                            }
                            #region 显示图像
                            #endregion

                            // 磁盘空间检查：剩余空间不足 50GB 时删除最早一天的图片
                            CheckAndDelete();
                            #region 保存OK图像
                            // OK 图像保存：配置开启且识别成功时，按日期/相机/OK/车型目录保存图片
                            if (MyCameraParameters.IsSaveOKImage && Result)
                            {
                                string time = DateTime.Now.ToString("HH-mm-ss");
                                SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\OK\\" + $"{CarModel}\\";
                                ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss") + "_" + VINCode.Replace("*", "") + ".bmp", (ICogImage)CurrentImage, 0);
                            }
                            // NG 图像保存：配置开启且识别失败时，按日期/相机/NG/车型目录保存图片
                            if (MyCameraParameters.IsSaveNGImage && !Result)
                            {
                                string time = DateTime.Now.ToString("HH-mm-ss");
                                SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\NG\\" + $"{CarModel}\\";
                                ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss") + "_" + VINCode.Replace("*", "").Replace("?", "") + "_NG.bmp", (ICogImage)CurrentImage, 0);
                            }
                            #endregion
                        }
                    }
                    // 异常捕获：记录错误日志并阻塞等待下一张图像（避免线程空转）
                    catch (System.Exception e)
                    {
                        Log.Error(e.Message + e.StackTrace);
                        mAutoResetEvent.WaitOne();
                    }
                }
                // 队列为空时阻塞等待信号（由取图回调 Set 唤醒）
                else mAutoResetEvent.WaitOne();
            }
        }

        // 旧版预留 VIN 码校验方法，当前未被主流程调用（主流程使用独立 VINCodeValidation 类）
       

        /// <summary>
        /// 打开光源
        /// </summary>
        public void OpenLight()
        {
            try
            {
                if (mSerialPort != null)
                {
                    if (CarModel.Contains("V22") || CarModel.Contains("beiyong")) mSerialPort.Write("SD0255#");
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
                mSerialPort.Write("SD0000#");
                Log.Info("光源关闭成功");
            }
            catch (Exception ex)
            {
                Log.Error($@"光源设置亮度为00出错，报错信息：{ex}");
            }
        }

        /// <summary>
        /// 软件触发采集：开光源 → 延时 200ms → 执行软件触发命令
        /// </summary>
        public override void SoftWareTrigger()
        {
            try
            {
                if (CurrentCamera != null)
                {
                    // 打开光源（SD0255#）
                    OpenLight();
                    // 等待光源亮度稳定
                    Thread.Sleep(200);
                    //int nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    // 执行海康软件触发命令
                    int nRet1 = CurrentCamera.MV_CC_TriggerSoftwareExecute_NET();
                    //nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                    Log.Info($"VINCamera5相机采集信息反馈：{nRet1}");
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

        #region 相机SDK

        /// <summary>
        /// 海康相机取图回调委托
        /// </summary>
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

                if (CurrentDevice.nTLayerType == MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(CurrentDevice.SpecialInfo.stGigEInfo, 0);
                    MV_GIGE_DEVICE_INFO gigeInfo = (MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MV_GIGE_DEVICE_INFO));
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
            ImageBuffer = Marshal.AllocHGlobal((int)(ImageSize) * 3);

            // ch:设置触发模式为off || en:set trigger mode as off
            CurrentCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            CurrentCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            //CurrentCamera.MV_CC_SetTriggerDelay_NET(20);//设置触发延时100us
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
                if (enPixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    CogImage8Root cogImage8Root = new CogImage8Root();
                    cogImage8Root.Initialize((Int32)nWidth, (Int32)nHeight, pImageBuf, (Int32)nWidth, null);
                    CogImage8Grey cogImage8Grey = new CogImage8Grey();
                    cogImage8Grey.SetRoot(cogImage8Root);
                    ImageQueue.Enqueue(cogImage8Grey.ScaleImage((int)nWidth, (int)nHeight));
                }
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
        /// 海康相机取图回调函数
        /// 流程：判断像素格式（彩色/Mono）→ 格式转换（非标准格式转 RGB8/Mono8）→
        ///       VisionProDisplay 封装入队 → Set 事件唤醒处理线程 → 关闭光源
        /// </summary>
        /// <param name="pData">图像数据指针</param>
        /// <param name="pFrameInfo">帧输出信息（宽、高、像素格式等）</param>
        /// <param name="pUser">用户自定义指针</param>
        public void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            MvGvspPixelType pixelType = MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            IntPtr pTemp = IntPtr.Zero;
            if (IsColorPixelFormat(pFrameInfo.enPixelType))    // 彩色图像处理
            {
                if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
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
                pixelType = MvGvspPixelType.PixelType_Gvsp_Mono8;
            }
            VisionProDisplay(pFrameInfo.nHeight, pFrameInfo.nWidth, pTemp, pixelType);

            // 图像入队成功后，唤醒 STA 处理线程开始推理
            if (ImageQueue.Count > 0)
            {
                mAutoResetEvent.Set();
                if (!StartLiveImage)
                    Log.Info($@"VIN码相机5收到触发拍照信号,图像队列数量：{ImageQueue.Count}");
            }
            // 采集完成后关闭光源（节约寿命，下一次触发时再打开）
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
