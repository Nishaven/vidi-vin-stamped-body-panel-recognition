// ============================================================
// 文件：VINCodeCamera1.cs
// 模块：VisionCore（VIN 码读码相机）
// 说明：A09/T58/A39 车型 VIN 码主读相机；读码失败时按车型级联触发备用相机
//       （A09 失败→VINCodeCamera2，T58 失败→VINCodeCamera4），A39 恒写 PLC，
//       A09/T58 仅读码成功才写 PLC。
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
using ViDi2;
using ViDi2.Runtime;
using ViDi2.VisionPro;
using static MvCamCtrl.NET.MyCamera;
using Image = ViDi2.VisionPro.Image;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// VIN 码读码相机 1（主读相机）：服务 A09/T58/A39 车型的 VIN 码深度学习读码；
    /// 读码失败时按车型级联触发备用相机（A09→mVINCodeCamera2、T58→mVINCodeCamera4）；
    /// A39 车型无论成败均写 PLC，A09/T58 仅读码成功才写 PLC；光源按车型使用 SA/SB/SC 通道。
    /// </summary>
    public class VINCodeCamera1 : CameraBase
    {
        /// <summary>VIN 码工位图像显示界面（车型下拉框、结果标签、ViDi 标注视图），由 UI 层注入</summary>
        public ImageViDiDisplay CurrentImageDisplay;
        /// <summary>光源控制串口（文本协议 "S&lt;通道&gt;&lt;4位亮度&gt;#"，本相机按车型使用 SA/SB/SC 通道）</summary>
        public SerialPort mSerialPort;
        // 按需存图的落盘目录（按 日期\相机名\OK|NG\车型 组织）
        private string SaveImagePath;
        /// <summary>级联备用相机引用：A09 车型读码失败时触发的 VIN 码相机 2（T09-M 工位）</summary>
        public VINCodeCamera2 mVINCodeCamera2;
        /// <summary>级联触发 VINCodeCamera2（T09-M 工位）时注入的曝光时间配方</summary>
        public double CameraExposure4;
        /// <summary>级联触发 VINCodeCamera2（T09-M 工位）时注入的增益配方</summary>
        public double CameraGain4;
        /// <summary>级联备用相机引用：T58 车型读码失败时触发的 VIN 码相机 4（T58-G 工位）</summary>
        public VINCodeCamera4 mVINCodeCamera4;
        /// <summary>级联触发 VINCodeCamera4（T58-G 工位）时注入的曝光时间配方</summary>
        public double CameraExposure6;
        /// <summary>级联触发 VINCodeCamera4（T58-G 工位）时注入的增益配方</summary>
        public double CameraGain6;
        
        /// <summary>最新一次读出的 VIN 码字符串（可能含 */? 噪声字符，校验通过后去除 *）</summary>
        public string VINCode = string.Empty;
        /// <summary>原始读码字符串（未经校验清洗的备份）</summary>
        public string Readcode = string.Empty;
        /// <summary>结果落库用的 SQLite 助手（写 ResultRecords 表），由外部注入</summary>
        public SQLiteHelper SQLiteHelperResult;
        /// <summary>读码结果队列，UI 结果表格的绑定数据源</summary>
        public ConcurrentQueue<VINCodeResult> VINCodeResultQueue;
        // SQLite 事务写入的 SQL 语句暂存列表（事务提交后即清空）
        private readonly ArrayList SQLiteResultDatas = new ArrayList();
        /// <summary>最近一次构造的读码结果记录（用于 UI 回写、PLC 输出与落库）</summary>
        private VINCodeResult mVINCodeResult;
        /// <summary>读码结果表格控件，由 UI 层注入</summary>
        public UIDataGridView ResultDataGridView;
        // ViDi 运行时流引用（旧版预留，当前主流程直接遍历工作区 Streams，未使用该字段）
        private ViDi2.Runtime.IStream mStream;
        /// <summary>ViDi Blue-Read 深度学习 OCR 读码工具引用</summary>
        private ViDi2.Runtime.ITool mBlueReadTool;
        /// <summary>UI 结果标签文本（OK/NG 及 VIN 码内容，绿色 OK / 红色 NG）</summary>
        private string LabelResult = string.Empty;
        /// <summary>
        /// 三菱PLC通信读写接口
        /// </summary>
        public IReadWriteNet mMelsecMcNet;
        //private int CurrentTriggerValue, BeforeTriggerValue;
        /// <summary>
        /// 构造函数：按序列号枚举并打开海康相机、设置曝光/增益，加载 ..\CameraVisionTool\{相机名}\*.vrws
        /// 的 ViDi 深度学习工作区，最后启动最高优先级的 STA 后台处理线程执行 RunCameraImage。
        /// </summary>
        /// <param name="systemConfigurations">系统配置（含 PLC 信号地址、校验开关、存图配置等）</param>
        /// <param name="cameraParameters">相机参数（序列号、曝光、增益、触发模式、存图开关等）</param>
        /// <param name="control">ViDi 运行时控制接口，用于加载深度学习工作区</param>
        public VINCodeCamera1(SystemConfigurations systemConfigurations, CameraParameters cameraParameters, ViDi2.Runtime.IControl control) : base(systemConfigurations, cameraParameters)
        {
            //bool res = VINCodeValidation("*LMGKT1L50R1231687*");
            //CameraListProductNmae
            // 保存相机参数与系统配置，供读码、PLC 写入、存图等流程使用
            MyCameraParameters = cameraParameters;
            mSystemConfigurations = systemConfigurations;
            Init(); 
            Log.Info($"{MyCameraParameters.CameraName}设置曝光时间返回值:{CurrentCamera.MV_CC_SetExposureTime_NET(((float)MyCameraParameters.CameraExposure) * 1000)}");
            Log.Info($"{MyCameraParameters.CameraName}设置增益返回值:{CurrentCamera.MV_CC_SetGain_NET((float)MyCameraParameters.CameraGain)}");
            // 加载本相机的 ViDi 深度学习工作区（..\CameraVisionTool\{相机名}\*.vrws，内含按车型划分的 Read 工具流）
            string CameraVisionToolBlockPath = Path.GetFullPath("..") + $"\\CameraVisionTool\\{MyCameraParameters.CameraName}";
            MyVisionTools = new VisionTools(CameraVisionToolBlockPath, MyCameraParameters.CameraName, control);
            // 启动 STA 后台最高优先级处理线程：循环执行 RunCameraImage（无图时阻塞在 mAutoResetEvent 上）
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
        /// 初始化相机：枚举海康设备并按序列号查找目标相机，然后创建句柄、注册取图回调并开流。
        /// </summary>
        public override void Init()
        {
            // 枚举设备列表并按序列号定位本工位相机
            SearchCamera();
            // 打开相机、设置触发模式并开始取图
            OpenCamera();
        }

        /// <summary>
        /// 相机 STA 处理线程主循环：从图像队列取图 → ViDi Blue-Read 深度学习读码 → VIN 码校验 →
        /// 按车型（A39/T58/A09）分发结果（UI 回写、级联备用相机触发、PLC 信号写入、SQLite 落库、存图）。
        /// </summary>
        public override void RunCameraImage()
        {
            while (true)
            {
                // 每轮循环新建 VIN 码校验器（主流程使用独立类 VINCodeValidation 做 17 位校验）
                VINCodeValidation mVINCodeValidation = new VINCodeValidation();
                // 队列中有图才处理，否则阻塞等待取图回调线程唤醒
                if (ImageQueue.Count > 0)
                {
                    try
                    {
                        // 统计本帧从出队到推理完成的处理耗时
                        Stopwatch mStopwatch = new Stopwatch();
                        mStopwatch.Start();
                        // 从取图回调线程填充的图像队列中取出一帧
                        ImageQueue.TryDequeue(out CurrentImage);
                        Log.Info($"VIN1相机图像队列移除处理时间：{mStopwatch.ElapsedMilliseconds}ms");
                        //ListGraphic.Clear();
                        //Log.Info($"VIN1相机显示清除时间：{mStopwatch.ElapsedMilliseconds}ms");
                        // 实时预览模式下不做推理，读码仅在采集（触发拍照）模式下执行
                        if (!StartLiveImage)
                        {

                            // 将 VisionPro 当前帧包装为 ViDi 推理所需的图像对象
                            using (var image = new Image(CurrentImage))
                            {
                                //mStream = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams[CarModel];
                                // 先用工作区第一个流预创建样本，作为后续 UI 渲染 ViDi 标注的兜底
                                ViDi2.ISample mSample = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams.First().CreateSample(image);
                                // 每帧读码前复位判定结果
                                Result = false;
                                // 遍历工作区全部 ViDi 流（一个工作区含多个车型的读码模型）
                                foreach (var stream in MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams)
                                {
                                    // 按流名前 2 字符与车型前 2 字符匹配，实现按车型选择读码模型
                                    if (stream.Name.Substring(0, 2) != CarModel.Substring(0, 2)) continue;
                                    // 用匹配车型的流创建样本（旧样本随之释放）
                                    using (mSample = stream.CreateSample(image))
                                    {
                                        // 取该流的 ViDi Blue-Read 深度学习 OCR 工具
                                        mBlueReadTool = stream.Tools["Read"];
                                        // 执行 Blue-Read 推理，生成字符匹配结果与标注视图
                                        mSample.Process(mBlueReadTool);
                                        // 遍历读码工具输出的视图（视图内含字符级匹配列表）
                                        foreach (ViDi2.IBlueView view in mSample.Markings[mBlueReadTool.Name].Views)
                                        {
                                            // 恰好 1 个匹配才认为读到完整 VIN 串（0 个或多个均判失败）
                                            if (view.Matches.Count == 1)
                                            {
                                                // 取匹配项的 FeatureString 作为 VIN 原始字符串（可能含 */? 噪声字符）
                                                foreach (ViDi2.IReadModelMatch match in view.Matches)
                                                {
                                                    VINCode = match.FeatureString;
                                                    // VIN 码校验（17 位 ISO 3779 / mod-11 校验）
                                                    if (mVINCodeValidation.Validation(VINCode))
                                                    {
                                                        // 校验通过：置 OK，并去除 * 噪声字符
                                                        Result = true;
                                                        LabelResult = $"VIN码:{VINCode}校验OK";
                                                        VINCode = VINCode.Replace("*", "");

                                                    }
                                                    else
                                                    {
                                                        // 校验失败：置 NG
                                                        Result = false;
                                                        LabelResult = $"VIN码:{VINCode}校验NG";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // 匹配数异常（0 个或多个）：判定读码失败
                                                Result = false;
                                                VINCode = string.Empty;
                                                LabelResult = "VIN码读取失败";
                                            }
                                        }
                                    }
                                    // 任一流读码成功即结束遍历，不再尝试其他车型的流
                                    if (Result) break;
                                }
                                // 记录本帧推理耗时与读码结果
                                mStopwatch.Stop();
                                Log.Info($"检测处理时间：{mStopwatch.ElapsedMilliseconds}ms,车型:{CarModel},VIN码:{VINCode}");
                               
                                // A39 车型分支：VIN1 为主读且无级联备用相机，无论成败均生成结果记录
                                if (CarModel == "A39")
                                {
                                    // 构造读码结果（附带车型识别相机的 FX 值）并入结果队列（UI 表格绑定源）
                                    mVINCodeResult = new VINCodeResult { RecordTime = DateTime.Now, CameraName = "VINCamera1", CarModel = CarModel, FX = CarIdentificationCamera.FX, VINCode = VINCode, Result = Result };
                                    VINCodeResultQueue.Enqueue(mVINCodeResult);
                                    // 跨线程回写 UI：刷新结果表格（超 200 条移除最旧）、结果标签（绿 OK/红 NG）与 ViDi 标注视图
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
                                // T58 车型分支：读码成功直接出结果；失败则级联触发备用相机 VINCodeCamera4（T58-G 工位）
                                else if (CarModel == "T58")
                                {
                                    // 读码成功：入队结果记录并回写 UI
                                    if (Result)
                                    {
                                        
                                        mVINCodeResult = new VINCodeResult { RecordTime = DateTime.Now, CameraName = "VINCamera1", CarModel = CarModel, FX = CarIdentificationCamera.FX, VINCode = VINCode, Result = Result };
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
                                    }
                                    else
                                    {
                                        
                                        // 读码失败：先记录一条“启用VINCamera4”的 NG 结果，提示已转入备用相机
                                        mVINCodeResult = new VINCodeResult { RecordTime = DateTime.Now, CameraName = "启用VINCamera4", CarModel = CarModel, FX = CarIdentificationCamera.FX, VINCode = VINCode, Result = Result };
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
                                        
                                        // 级联触发备用相机 VINCodeCamera4：注入 T58-G 车型与对应曝光/增益配方后软触发采集
                                        mVINCodeCamera4.CarModel = "T58-G";
                                        mVINCodeCamera4.Exposure = CameraExposure6;
                                        mVINCodeCamera4.Gain = CameraGain6;
                                        mVINCodeCamera4.SoftWareTrigger();
                                        Log.Info("VINCodeCamera4相机触发采集图片");
                                    }

                                }
                                // A09 车型分支：读码成功直接出结果；失败则级联触发备用相机 VINCodeCamera2（T09-M 工位）
                                else if (CarModel == "A09")
                                {
                                    // 读码成功：入队结果记录并回写 UI
                                    if (Result)
                                    {
                                        
                                        mVINCodeResult = new VINCodeResult { RecordTime = DateTime.Now, CameraName = "VINCamera1", CarModel = CarModel, FX = CarIdentificationCamera.FX, VINCode = VINCode, Result = Result };
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
                                    }
                                    else
                                    {
                                        
                                        // 读码失败：先记录一条“启用VINCamera2”的 NG 结果，提示已转入备用相机
                                        mVINCodeResult = new VINCodeResult { RecordTime = DateTime.Now, CameraName = "启用VINCamera2", CarModel = CarModel, FX = CarIdentificationCamera.FX, VINCode = VINCode, Result = Result };
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
                                      
                                        // 级联触发备用相机 VINCodeCamera2：注入 T09-M 车型与对应曝光/增益配方后软触发采集
                                        mVINCodeCamera2.CarModel = "T09-M";
                                        mVINCodeCamera2.Exposure = CameraExposure4;
                                        mVINCodeCamera2.Gain = CameraGain4;
                                        mVINCodeCamera2.SoftWareTrigger();
                                        Log.Info("VINCodeCamera2相机触发采集图片");
                                    }
                                }

                                
                                // PLC 写入与 SQLite 落库阶段（仅自动模式下执行）——A39：无论 OK/NG 均输出结果信号
                                if (CarModel == "A39")
                                {
                                    // 自动运行模式（联机生产）下才写 PLC 与落库
                                    if (IsAutoStatus)
                                    {
                                        // PLC 通信接口已注入才执行信号写入
                                        if (mMelsecMcNet != null)
                                        {
                                            // 读码成功：写 VIN 字符串 + OK 脉冲 + 作业完成脉冲
                                            if (Result)
                                            {
                                                Log.Info($"车型VIN码相机1输出VIN码，信号地址：{mSystemConfigurations.VINCodeCameraVINCodeAddress}写入值:{VINCode},写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraVINCodeAddress, VINCode).IsSuccess}");

                                                Log.Info($"车型VIN码相机1输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, true).IsSuccess}");
                                                Thread.Sleep(300);
                                                Log.Info($"车型VIN码相机1输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, false).IsSuccess}");


                                            }
                                            else
                                            {
                                                // 读码失败：输出 NG 脉冲信号（True→延时300ms→False）
                                                Log.Info($"车型VIN码相机1输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, true).IsSuccess}");
                                                Thread.Sleep(300);
                                                Log.Info($"车型VIN码相机1输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, false).IsSuccess}");
                                            }
                                            // 作业完成脉冲（True→延时300ms→False），通知 PLC 本次读码作业结束（A39 无论成败恒写）
                                            Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, true).IsSuccess}");
                                            Thread.Sleep(300);
                                            Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, false).IsSuccess}");

                                        }

                                        // A39 无论成败均落库：组装 ResultRecords 插入语句并事务写入 SQLite
                                        SQLiteResultDatas.Add($@"INSERT INTO ResultRecords (RecordTime,CameraName,CarModel,VINCode,Result) VALUES ('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','{mVINCodeResult.CameraName}','{mVINCodeResult.CarModel}','{mVINCodeResult.VINCode}','{mVINCodeResult.Result}')");
                                        SQLiteHelperResult.ExecuteSqlTran(SQLiteResultDatas);
                                        SQLiteResultDatas.Clear();
                                    }
                                }
                               
                                // PLC 写入与 SQLite 落库阶段（仅自动模式下执行）——T58：仅读码成功才写 PLC 与落库（失败由级联的 VINCamera4 负责）
                                else if (CarModel == "T58")
                                {
                                    // 自动运行模式（联机生产）下才写 PLC 与落库
                                    if (IsAutoStatus)
                                    {
                                        // PLC 通信接口已注入才执行信号写入
                                        if (mMelsecMcNet != null)
                                        {
                                            // 读码成功：写 VIN 字符串 + OK 脉冲 + 作业完成脉冲，并落库
                                            if (Result)
                                            {
                                                Log.Info($"车型VIN码相机1输出VIN码，信号地址：{mSystemConfigurations.VINCodeCameraVINCodeAddress}写入值:{VINCode},写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraVINCodeAddress, VINCode).IsSuccess}");

                                                // OK 脉冲信号（True→延时300ms→False）
                                                Log.Info($"车型VIN码相机1输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, true).IsSuccess}");
                                                Thread.Sleep(300);
                                                Log.Info($"车型VIN码相机1输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, false).IsSuccess}");
                                                //}
                                                //else
                                                //{
                                                //    Log.Info($"车型VIN码相机1输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, true).IsSuccess}");
                                                //    Thread.Sleep(300);
                                                //    Log.Info($"车型VIN码相机1输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, false).IsSuccess}");
                                                //}
                                                // 作业完成脉冲（True→延时300ms→False），通知 PLC 本次读码作业结束
                                                Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, true).IsSuccess}");
                                                Thread.Sleep(300);
                                                Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, false).IsSuccess}");

                                                // 组装 ResultRecords 插入语句并事务写入 SQLite（写完清空暂存列表）
                                                SQLiteResultDatas.Add($@"INSERT INTO ResultRecords (RecordTime,CameraName,CarModel,VINCode,Result) VALUES ('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','{mVINCodeResult.CameraName}','{mVINCodeResult.CarModel}','{mVINCodeResult.VINCode}','{mVINCodeResult.Result}')");
                                                SQLiteHelperResult.ExecuteSqlTran(SQLiteResultDatas);
                                                SQLiteResultDatas.Clear();
                                            }
                                        }

                                    }
                                }
                                
                                // PLC 写入与 SQLite 落库阶段（仅自动模式下执行）——A09：仅读码成功才写 PLC 与落库（失败由级联的 VINCamera2 负责）
                                else if (CarModel == "A09")
                                {
                                    // 自动运行模式（联机生产）下才写 PLC 与落库
                                    if (IsAutoStatus)
                                    {
                                        // PLC 通信接口已注入才执行信号写入
                                        if (mMelsecMcNet != null)
                                        {
                                            // 读码成功：写 VIN 字符串 + OK 脉冲 + 作业完成脉冲，并落库
                                            if (Result)
                                            {
                                                Log.Info($"车型VIN码相机1输出VIN码，信号地址：{mSystemConfigurations.VINCodeCameraVINCodeAddress}写入值:{VINCode},写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraVINCodeAddress, VINCode).IsSuccess}");

                                                // OK 脉冲信号（True→延时300ms→False）
                                                Log.Info($"车型VIN码相机1输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, true).IsSuccess}");
                                                Thread.Sleep(300);
                                                Log.Info($"车型VIN码相机1输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, false).IsSuccess}");
                                                //}
                                                //else
                                                //{
                                                //    Log.Info($"车型VIN码相机1输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, true).IsSuccess}");
                                                //    Thread.Sleep(300);
                                                //    Log.Info($"车型VIN码相机1输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, false).IsSuccess}");
                                                //}
                                                // 作业完成脉冲（True→延时300ms→False），通知 PLC 本次读码作业结束
                                                Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, true).IsSuccess}");
                                                Thread.Sleep(300);
                                                Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, false).IsSuccess}");

                                                // 组装 ResultRecords 插入语句并事务写入 SQLite（写完清空暂存列表）
                                                SQLiteResultDatas.Add($@"INSERT INTO ResultRecords (RecordTime,CameraName,CarModel,VINCode,Result) VALUES ('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','{mVINCodeResult.CameraName}','{mVINCodeResult.CarModel}','{mVINCodeResult.VINCode}','{mVINCodeResult.Result}')");
                                                SQLiteHelperResult.ExecuteSqlTran(SQLiteResultDatas);
                                                SQLiteResultDatas.Clear();
                                            }
                                        }

                                    }
                                }
                                //
                                #region 显示图像
                                #endregion

                                // 磁盘空间维护：剩余空间不足阈值（约 50GB）时清理最早一天的图片
                                CheckAndDelete();
                                #region 保存OK图像
                                // 配置允许且读码 OK 时保存 OK 图像（按 日期\相机名\OK\车型 目录，文件名含 VIN 码）
                                if (MyCameraParameters.IsSaveOKImage && Result)
                                {
                                    string time = DateTime.Now.ToString("HH-mm-ss");
                                    SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\OK\\" + $"{CarModel}\\";
                                    ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss") + "_" + VINCode.Replace("*", "") + ".bmp", (ICogImage)CurrentImage, 0);
                                }
                                // 配置允许且读码 NG 时保存 NG 图像（文件名追加 _NG，去除 * ? 噪声字符）
                                if (MyCameraParameters.IsSaveNGImage && !Result)
                                {
                                    string time = DateTime.Now.ToString("HH-mm-ss");
                                    SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\NG\\" + $"{CarModel}\\";
                                    ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss") + "_" + VINCode.Replace("*", "").Replace("?", "") + "_NG.bmp", (ICogImage)CurrentImage, 0);
                                }
                                #endregion
                            }
                        }
                    }
                    // 异常兜底：记录错误日志后阻塞等待下一次图像唤醒，避免线程空转
                    catch (System.Exception e)
                    {
                        Log.Error(e.Message + e.StackTrace);
                        mAutoResetEvent.WaitOne();
                    }
                }
                // 无待处理图像：阻塞等待取图回调线程唤醒
                else mAutoResetEvent.WaitOne();
            }
        }

        /// <summary>
        /// 旧版 VIN 码校验（17 位长度 + mod-11 校验位 + 年份位核对）：旧版预留，当前未被主流程调用
        /// （主流程使用独立类 VINCodeValidation 的 Validation 方法）。
        /// </summary>
        private bool VINCodeValidation(string mVINCode)
        {
            if (mVINCode.Replace("*", "").Replace("?", "").Length == 17)
            {
                int currentYear = DateTime.Now.Year;
                string yearString = "123456789ABCDEFGHJKLMNPRSTVWXY";
                int yearIndex = (currentYear - 2000) % 30;
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(mVINCode.Replace("*", ""));
                int total = 0;
                for (int i = 0; i < 17; i++)
                {
                    int temp = byteArray[i] - 48;
                    if (temp > 9) temp = temp - 16;
                    if (temp > 9) temp = temp - 9;
                    if (temp > 9) temp = temp - 8;
                    if (i == 8) temp = 0;
                    int site = 9 - (i + 1) % 10;
                    if (site == 1) site = 10;
                    total = total + site * temp;
                }
                int totalx = total % 11;
                int totals = byteArray[8] - 48;
                if (totals > 9) totals = 10;
                if (totalx == totals)
                {
                    if (mVINCode[10] == yearString[yearIndex + 1] | mVINCode[10] == yearString[yearIndex + 2] | mVINCode[10] == yearString[yearIndex] | mVINCode[10] == yearString[yearIndex - 1] | mVINCode[10] == yearString[yearIndex - 2] | mVINCode[10] == yearString[yearIndex - 3] | mVINCode[10] == yearString[yearIndex - 4])
                    {
                        if (mSystemConfigurations.ISPAOn2)
                            VINCode = mVINCode.Replace("*", "").Replace("?", "");
                        else VINCode = mVINCode;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
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
                    // 按车型点亮对应光源通道（SA=A09、SB=T58、SC=A39，亮度 255）
                    if (CarModel.Contains("A09")) mSerialPort.Write("SA0255#");
                    else if (CarModel.Contains("T58")) mSerialPort.Write("SB0255#");
                    else if (CarModel.Contains("A39")) mSerialPort.Write("SC0255#");
                    Log.Info("光源点亮成功");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($@"光源设置亮度为255出错，报错信息：{ex}");
            }
        }

        /// <summary>
        /// 关闭光源
        /// </summary>
        /// 

      
        public void CloseLight()
        {
            try
            {
                // 依次关闭 SA/SB/SC 三个光源通道（亮度置 0）
                mSerialPort.Write("SA0000#");
                Thread.Sleep(20);
                mSerialPort.Write("SB0000#");
                Thread.Sleep(20);
                mSerialPort.Write("SC0000#");
                Log.Info("光源关闭成功");
            }
            catch (System.Exception ex)
            {
                Log.Error($@"光源设置亮度为00出错，报错信息：{ex}");
            }
        }

        /// <summary>
        /// 软触发采集：点亮当前车型对应通道光源（SA/SB/SC），延时 200ms 待光照稳定后执行海康软触发拍照；
        /// 后续图像处理由取图回调与 STA 处理线程完成。
        /// </summary>
        public override void SoftWareTrigger()
        {
            try
            {
                if (CurrentCamera != null)
                {
                    // 先开光源并延时，保证成像光照稳定
                    OpenLight();
                    Thread.Sleep(200);
                    //int nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    int nRet1 = CurrentCamera.MV_CC_TriggerSoftwareExecute_NET();
                    Log.Info($"VINCamera1相机采集信息反馈：{nRet1}");
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

        /// <summary>海康 SDK 取图回调委托（静态持有，防止委托被 GC 回收导致回调失效）</summary>
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
        /// 海康 SDK 取图回调（在 SDK 回调线程执行）：按像素格式做转换（彩色→RGB8、黑白→Mono8），
        /// 生成 VisionPro 图像入队并唤醒 STA 处理线程执行读码，最后关闭光源。
        /// </summary>
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
            // 将转换后的数据包装为 VisionPro 图像并入队（Mono8 灰度 / RGB 三平面彩色）
            VisionProDisplay(pFrameInfo.nHeight, pFrameInfo.nWidth, pTemp, pixelType);

            // 队列有图则唤醒阻塞的 STA 处理线程执行读码推理
            if (ImageQueue.Count > 0)
            {
                mAutoResetEvent.Set();
                if (!StartLiveImage)
                    Log.Info($@"VIN码相机1收到触发拍照信号,图像队列数量：{ImageQueue.Count}");
            }
            // 采图完成立即关闭光源，防止持续点亮造成过曝与光源老化
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
