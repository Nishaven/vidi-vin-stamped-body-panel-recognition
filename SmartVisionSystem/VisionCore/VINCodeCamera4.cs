// ============================================================
// 文件：VINCodeCamera4.cs
// 模块：VisionCore（VIN 码读码相机）
// 说明：T58-G1 工位 VIN 码备用读码相机（由 VINCodeCamera1 在 T58 车型主读失败时
//       级联软触发）；读到匹配且流名含 T58-G1 时将车型改记为 T58-G1；
//       无论读码成败均写 PLC（OK/NG 脉冲 + 作业完成）并落库；光源使用 SF 通道。
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
    /// VIN 码读码相机 4：T58-G1 备用工位（由 VINCodeCamera1 在 T58 车型主读失败时级联软触发）；
    /// 读到匹配且流名包含 T58-G1 时，将车型改记为 T58-G1；
    /// 无论读码成败均写 PLC（OK/NG 脉冲 + 作业完成信号）并落库 SQLite。
    /// </summary>
    public class VINCodeCamera4 : CameraBase
    {
        /// <summary>VIN 码工位图像显示界面（车型下拉框、结果标签、ViDi 标注视图），由 UI 层注入</summary>
        public ImageViDiDisplay CurrentImageDisplay;
        /// <summary>光源控制串口（文本协议 "S&lt;通道&gt;&lt;4位亮度&gt;#"，本相机统一使用 SF 通道）</summary>
        public SerialPort mSerialPort;
        // 按需存图的落盘目录（按 日期\相机名\OK|NG\车型 组织）
        private string SaveImagePath;
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
        public VINCodeCamera4(SystemConfigurations systemConfigurations, CameraParameters cameraParameters, ViDi2.Runtime.IControl control) : base(systemConfigurations, cameraParameters)
        {

            //CameraListProductNmae
            // 保存相机参数与系统配置，供读码、PLC 写入、存图等流程使用
            MyCameraParameters = cameraParameters;
            mSystemConfigurations = systemConfigurations;
            // 枚举设备并按序列号打开本工位相机
            Init();
            Log.Info($"{MyCameraParameters.CameraName}设置曝光时间返回值:{CurrentCamera.MV_CC_SetExposureTime_NET(((float)MyCameraParameters.CameraExposure) * 1000)}");
            Log.Info($"{MyCameraParameters.CameraName}设置增益返回值:{CurrentCamera.MV_CC_SetGain_NET((float)MyCameraParameters.CameraGain)}");
            // 加载本相机的 ViDi 深度学习工作区（..\CameraVisionTool\{相机名}\*.vrws，内含 Read 读码工具流）
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
        /// 结果分发（UI 回写、PLC 信号写入、SQLite 落库、存图）。
        /// 本相机为备用读码工位，无论读码成败均写 PLC 并落库。
        /// </summary>
        public override void RunCameraImage()
        {
            // 处理线程常驻主循环（后台线程，随进程退出而终止）
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
                        Log.Info($"VIN4相机图像队列移除处理时间：{mStopwatch.ElapsedMilliseconds}ms");
                        //ListGraphic.Clear();
                        //Log.Info($"VIN4相机显示清除时间：{mStopwatch.ElapsedMilliseconds}ms");
                        // 实时预览模式下不做推理，读码仅在采集（触发拍照）模式下执行
                        if (!StartLiveImage)
                        {

                            using (var image = new Image(CurrentImage))
                            {
                                //mStream = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams[CarModel];
                                // 先用工作区第一个流预创建样本，作为后续 UI 渲染 ViDi 标注的兜底
                                ViDi2.ISample mSample = MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams.First().CreateSample(image);
                                // 每帧读码前复位判定结果
                                Result = false;
                                // 遍历工作区全部 ViDi 流（本相机未按车型过滤流名，逐个流尝试直至读码成功）
                                foreach (var stream in MyVisionTools.CameraRuntimeWork.CameraWorkspace.Streams)
                                {
                                    //if (stream.Name.Substring(0, 2) != CarModel.Substring(0, 2)) continue;
                                    // 用当前流创建样本（旧样本随之释放）
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

                                                    // 读到匹配且流名含 T58-G1 时，车型改记为 T58-G1（备用工位车型映射）
                                                    if (stream.Name.Contains("T58-G1"))
                                                    {
                                                        CarModel = "T58-G1";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // 匹配数不为 1：读码失败
                                                Result = false;
                                                VINCode = string.Empty;
                                                LabelResult = "VIN码读取失败";
                                            }
                                        }
                                        // 读码成功即跳出流遍历，不再尝试后续流
                                        if (Result) break;
                                    }
                                }
                                // 停止计时，记录本帧推理耗时与读码结果
                                mStopwatch.Stop();
                                Log.Info($"检测处理时间：{mStopwatch.ElapsedMilliseconds}ms,车型:{CarModel},VIN码:{VINCode}");
                                // 构造本帧读码结果记录（FX 方向取自车型识别相机的最新值）并压入结果队列
                                mVINCodeResult = new VINCodeResult { RecordTime = DateTime.Now, CameraName = "VINCamera4", CarModel = CarModel, FX = CarIdentificationCamera.FX, VINCode = VINCode, Result = Result };
                                VINCodeResultQueue.Enqueue(mVINCodeResult);
                                // 跨线程回写 UI：结果表格、车型下拉框、结果标签与 ViDi 标注视图
                                if (CurrentImageDisplay.InvokeRequired)
                                {
                                    CurrentImageDisplay.BeginInvoke(new Action(() =>
                                    {
                                        // 结果队列超过 200 条时丢弃最早记录，防止结果表格无限增长
                                        if (VINCodeResultQueue.Count > 200)
                                        {
                                            VINCodeResultQueue.ToList().RemoveAt(0);
                                        }
                                        // 刷新结果表格并定位到最后一行
                                        ResultDataGridView.DataSource = VINCodeResultQueue.ToList();
                                        ResultDataGridView.CurrentCell = ResultDataGridView.Rows[ResultDataGridView.Rows.Count - 1].Cells[0];
                                        // 车型下拉框同步当前车型；结果标签按 OK/NG 显示绿色/红色
                                        CurrentImageDisplay.ComboBoxCarModel.SelectedItem = CarModel;
                                        CurrentImageDisplay.LabelResult.ForeColor = Result ? Color.Green : Color.Red;
                                        CurrentImageDisplay.LabelResult.Text = LabelResult;
                                        // 切换 ViDi 标注视图为本次读码样本，供界面查看字符匹配详情
                                        CurrentImageDisplay.mSampleViewer.ViewModel.ViewIndex = 0;
                                        CurrentImageDisplay.mSampleViewer.ViewModel.Sample = mSample;
                                    }));
                                }

                            }
                            // 自动运行模式下才做结果分发（写 PLC + 落库）
                            if (IsAutoStatus)
                            {
                                // PLC 连接就绪才输出信号
                                if (mMelsecMcNet != null)
                                {
                                    // 读码成功分支：写 VIN 码 + OK 脉冲
                                    if (Result)
                                    {
                                        // 先把读到的 VIN 码字符串写入 PLC 数据地址
                                        Log.Info($"车型VIN码相机4输出VIN码，信号地址：{mSystemConfigurations.VINCodeCameraVINCodeAddress}写入值:{VINCode},写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraVINCodeAddress, VINCode).IsSuccess}");
                                        
                                        // 再输出 OK 脉冲信号：True → 延时 300ms → False（脉宽约 300ms）
                                        Log.Info($"车型VIN码相机4输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, true).IsSuccess}");
                                        Thread.Sleep(300);
                                        Log.Info($"车型VIN码相机4输出OK信号，信号地址：{mSystemConfigurations.VINCodeCameraOKAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraOKAddress, false).IsSuccess}");
                                    }
                                    else
                                    {
                                        // 读码失败分支：输出 NG 脉冲信号（与相机 1 共用同一 NG 地址）
                                        Log.Info($"VIN码相机1和4输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, true).IsSuccess}");
                                        Thread.Sleep(300);
                                        Log.Info($"VIN码相机1和4输出NG信号，信号地址：{mSystemConfigurations.VINCodeCameraNGAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.VINCodeCameraNGAddress, false).IsSuccess}");
                                    }
                                    // 无论 OK/NG 均输出作业完成脉冲，通知 PLC 本工位节拍结束
                                    Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值True,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, true).IsSuccess}");
                                    Thread.Sleep(300);
                                    Log.Info($"作业完成，信号地址：{mSystemConfigurations.JobRunDoneAddress}写入值False,写入状态：{mMelsecMcNet.Write(mSystemConfigurations.JobRunDoneAddress, false).IsSuccess}");

                                }

                                // 无论读码成败均落库：拼接 INSERT 语句，以事务方式写入 ResultRecords 表后清空暂存列表
                                SQLiteResultDatas.Add($@"INSERT INTO ResultRecords (RecordTime,CameraName,CarModel,VINCode,Result) VALUES ('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','{mVINCodeResult.CameraName}','{mVINCodeResult.CarModel}','{mVINCodeResult.VINCode}','{mVINCodeResult.Result}')");
                                SQLiteHelperResult.ExecuteSqlTran(SQLiteResultDatas);
                                SQLiteResultDatas.Clear();
                            }
                            #region 显示图像
                            #endregion

                            // 按存图清理策略检查并删除过期图片
                            CheckAndDelete();
                            #region 保存OK图像
                            // 开启 OK 存图且读码成功时，按需保存 OK 图
                            if (MyCameraParameters.IsSaveOKImage && Result)
                            {
                                string time = DateTime.Now.ToString("HH-mm-ss");
                                // OK 图落盘路径：Images\日期\相机名\OK\车型\时分秒_VIN码.bmp，交由全局存图队列异步写盘
                                SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\OK\\" + $"{CarModel}\\";
                                ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss") + "_" + VINCode.Replace("*", "") + ".bmp", (ICogImage)CurrentImage, 0);
                            }
                            // 开启 NG 存图且读码失败时，按需保存 NG 图
                            if (MyCameraParameters.IsSaveNGImage && !Result)
                            {
                                string time = DateTime.Now.ToString("HH-mm-ss");
                                // NG 图落盘路径：Images\日期\相机名\NG\车型\时分秒_NG.bmp（文件名先去除 */? 噪声字符）
                                SaveImagePath = Path.GetFullPath("..") + "\\Images\\" + DateTime.Now.ToString("yyyy-MM-dd") + $"\\{MyCameraParameters.CameraName}\\NG\\" + $"{CarModel}\\";
                                ImageSaveQueue.gOnly.AddToQueue(SaveImagePath + DateTime.Now.ToString("HH-mm-ss") + "_" + VINCode.Replace("*", "").Replace("?", "") + "_NG.bmp", (ICogImage)CurrentImage, 0);
                            }
                            #endregion
                        }
                    }
                    catch (System.Exception e)
                    {
                        // 处理异常：记录错误日志后阻塞等待下一次唤醒，避免异常导致线程空转
                        Log.Error(e.Message + e.StackTrace);
                        mAutoResetEvent.WaitOne();
                    }
                }
                // 队列无图：阻塞等待取图回调线程入图后唤醒
                else mAutoResetEvent.WaitOne();
            }
        }

        /// <summary>
        /// VIN 码校验（本类旧版本地实现；主流程当前改用独立类 VINCodeValidation 的同名方法，
        /// 此方法作为预留保留）：先校验去除噪声字符后长度是否为 17 位，再做 ISO 3779 mod-11
        /// 加权校验位校验，最后校验第 11 位车型年份字符与当前年份的匹配（允许前后偏移）。
        /// </summary>
        /// <param name="mVINCode">待校验的 VIN 码字符串（可含 */? 噪声字符）</param>
        /// <returns>三项校验全部通过返回 true，否则返回 false</returns>
        private bool VINCodeValidation(string mVINCode)
        {
            // 去除 */? 噪声字符后长度必须恰为 17 位（ISO 3779 标准 VIN 长度），否则直接判失败
            if (mVINCode.Replace("*", "").Replace("?", "").Length == 17)
            {
                // 车型年份字符表：30 年一循环（数字后接字母，跳过易混淆的 I/O/Q/U/Z）
                int currentYear = DateTime.Now.Year;
                string yearString = "123456789ABCDEFGHJKLMNPRSTVWXY";
                // 当前年份在 30 年循环表中的索引
                int yearIndex = (currentYear - 2000) % 30;
                // 取去除 * 后的各字符字节，用于加权计算
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(mVINCode.Replace("*", ""));
                // mod-11 加权累加器
                int total = 0;
                for (int i = 0; i < 17; i++)
                {
                    // 字符转数值：'0'-'9' 直接取值，字母按 A=1、B=2… 分段折算映射
                    int temp = byteArray[i] - 48;
                    if (temp > 9) temp = temp - 16;
                    if (temp > 9) temp = temp - 9;
                    if (temp > 9) temp = temp - 8;
                    // 第 9 位（索引 8）是校验位本身，参与加权时按 0 处理
                    if (i == 8) temp = 0;
                    // 计算该位置的权重（位置加权，余 1 时权重记 10）
                    int site = 9 - (i + 1) % 10;
                    if (site == 1) site = 10;
                    total = total + site * temp;
                }
                // 加权和模 11 得到期望校验值
                int totalx = total % 11;
                // 实际校验位字符转数值（字母 X 记 10）
                int totals = byteArray[8] - 48;
                if (totals > 9) totals = 10;
                // 校验位比对：通过则继续做年份位校验
                if (totalx == totals)
                {
                    // 第 11 位（索引 10）为车型年份字符：允许落在当前年份索引前后共 7 年的范围内
                    if (mVINCode[10] == yearString[yearIndex + 1] | mVINCode[10] == yearString[yearIndex + 2] | mVINCode[10] == yearString[yearIndex] | mVINCode[10] == yearString[yearIndex - 1] | mVINCode[10] == yearString[yearIndex - 2] | mVINCode[10] == yearString[yearIndex - 3] | mVINCode[10] == yearString[yearIndex - 4])
                    {
                        // 校验全部通过：开启二次清洗开关时去除全部 */? 噪声字符，否则保留原始串
                        if (mSystemConfigurations.ISPAOn2) VINCode = mVINCode.Replace("*", "").Replace("?", "");
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
                    // T58 系列车型通过 SF 通道以亮度 255 点亮光源（文本协议 "S+通道+4位亮度+#"；
                    // 后一分支因字符串包含关系与前一分支等效，保留原始写法）
                    if (CarModel.Contains("T58")) mSerialPort.Write("SF0255#");
                    else if (CarModel.Contains("T58-G")) mSerialPort.Write("SF0255#");
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
                // SF 通道亮度归零，熄灭光源
                mSerialPort.Write("SF0000#");
                Log.Info("光源关闭成功");
            }
            catch (Exception ex)
            {
                Log.Error($@"光源设置亮度为00出错，报错信息：{ex}");
            }
        }

        /// <summary>
        /// 软触发拍照：点亮 SF 光源 → 延时 200ms 等光源稳定 → 执行一次软触发采集。
        /// 本方法供 VINCodeCamera1 在 T58 车型主读失败时级联调用（备用工位补拍）。
        /// </summary>
        public override void SoftWareTrigger()
        {
            try
            {
                if (CurrentCamera != null)
                {
                    // 点亮光源
                    OpenLight();
                    // 延时 200ms 等待光源亮度稳定后再曝光
                    Thread.Sleep(200);
                    //int nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    // 执行一次软触发采集（相机当前处于软触发模式）
                    int nRet1 = CurrentCamera.MV_CC_TriggerSoftwareExecute_NET();
                    //nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                    Log.Info($"VINCamera4相机采集信息反馈：{nRet1}");
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

        /// <summary>海康 SDK 取图回调委托（静态引用防止委托被 GC 回收导致回调失效）</summary>
        public static MyCamera.cbOutputExdelegate ImageCallback;
        /// <summary>
        /// 查找相机设备
        /// </summary>
        public void SearchCamera()
        {
            // 枚举局域网内的海康千兆网相机设备
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

                // 千兆网（GigE）相机分支：解析设备信息并按序列号匹配目标相机
                if (CurrentDevice.nTLayerType == MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(CurrentDevice.SpecialInfo.stGigEInfo, 0);
                    MV_GIGE_DEVICE_INFO gigeInfo = (MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MV_GIGE_DEVICE_INFO));
                    // 序列号匹配即为目标相机：记录设备信息并跳出遍历
                    if (gigeInfo.chSerialNumber == MyCameraParameters.CameraSerialNum)
                    {
                        CurrentDevice = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                        Log.Info($"找到{MyCameraParameters.CameraName},相机SN:{gigeInfo.chDeviceVersion},固件版本:{gigeInfo.chDeviceVersion}");
                        break;
                    }
                    // 遍历到最后仍未匹配到序列号，提示检查相机连接
                    if (!IsFind && i == m_pDeviceList.nDeviceNum - 1)
                    {
                        Log.Info($"找不到{MyCameraParameters.CameraName},相机SN:{gigeInfo.chDeviceVersion},请检查相机连接!");
                    }
                }
                // USB3 相机分支：解析设备信息并按序列号匹配目标相机
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
            // 创建相机对象并按 SearchCamera 定位到的设备句柄创建设备
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
            // 采集模式设为连续采集、触发模式设为开启（具体触发源由下方 TriggerSource 决定）
            CurrentCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            CurrentCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            //CurrentCamera.MV_CC_SetTriggerDelay_NET(20);//设置触发延时100us
            // 自动模式：触发源切换为硬件 Line0（由 PLC 硬触发信号拍照）
            if (MyCameraParameters.CameraTriggerMode == "Auto")
            {

                CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
            }
            // 手动模式：触发源切换为软触发（供级联补拍的 SoftWareTrigger 使用）
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
            // 设置 SDK 图像缓存节点数为 10，提高连续取图吞吐
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
                    // Mono8 分支：以 8 位灰度根图构造灰度图，缩放后入图像队列
                    CogImage8Root cogImage8Root = new CogImage8Root();
                    cogImage8Root.Initialize((Int32)nWidth, (Int32)nHeight, pImageBuf, (Int32)nWidth, null);
                    CogImage8Grey cogImage8Grey = new CogImage8Grey();
                    cogImage8Grey.SetRoot(cogImage8Root);
                    ImageQueue.Enqueue(cogImage8Grey.ScaleImage((int)nWidth, (int)nHeight));
                }
                else
                {
                    // 彩色分支：按 ImageSize 偏移拆出 R/G/B 三个平面，构造 24 位平面彩色图后入队
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
        /// 海康 SDK 取图回调（在 SDK 回调线程执行）：按像素格式做转换（彩色转 RGB8、
        /// 黑白转 Mono8），交给 VisionProDisplay 转为 VisionPro 图像并入队；
        /// 队列有图时唤醒 STA 处理线程，最后关闭光源。
        /// </summary>
        /// <param name="pData">SDK 返回的图像数据指针</param>
        /// <param name="pFrameInfo">帧信息（宽、高、像素格式、帧号等）</param>
        /// <param name="pUser">用户自定义参数（未使用）</param>
        public void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            // 默认按 RGB8 Packed 处理，Mono 分支中会改写为 Mono8
            MvGvspPixelType pixelType = MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            IntPtr pTemp = IntPtr.Zero;
            if (IsColorPixelFormat(pFrameInfo.enPixelType))    // 彩色图像处理
            {
                if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    // 已是 RGB8 Packed 格式，直接使用源数据指针
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
                // 标记当前帧像素类型为 Mono8
                pixelType = MvGvspPixelType.PixelType_Gvsp_Mono8;
            }
            // 转为 VisionPro 图像并入图像队列
            VisionProDisplay(pFrameInfo.nHeight, pFrameInfo.nWidth, pTemp, pixelType);

            // 队列有图则唤醒 STA 处理线程开始推理（非实时预览时打印触发日志）
            if (ImageQueue.Count > 0)
            {
                mAutoResetEvent.Set();
                if (!StartLiveImage)
                    Log.Info($@"VIN码相机4收到触发拍照信号,图像队列数量：{ImageQueue.Count}");
            }
            // 拍照完成即熄灭光源，避免光源常亮发热与寿命损耗
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
