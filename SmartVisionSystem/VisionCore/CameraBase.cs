// ============================================================
// 文件：CameraBase.cs
// 模块：VisionCore（相机抽象基类）
// 说明：全部 6 台业务相机（1 台车型识别 + 5 台 VIN 读码）的公共基类，
//       持有相机状态、海康 SDK 句柄、图像队列与 ViDi 工具引用；
//       同文件附带 User 用户权限实体。
// ============================================================
using Cognex.VisionPro;
using MvCamCtrl.NET;
using Sunny.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartVisionSystem.Log4Net;
using static MvCamCtrl.NET.MyCamera;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// 相机抽象基类：封装海康相机 SDK 句柄、图像队列、触发/曝光/增益属性
    /// （属性 setter 直连海康 SDK）与 ViDi 视觉工具引用；
    /// 派生类实现 Init / RunCameraImage / SoftWareTrigger 完成各自业务
    /// </summary>
    public abstract class CameraBase
    {
        /// <summary>
        /// 视觉工具
        /// </summary>
        public VisionTools MyVisionTools { get; set; }
        /// <summary>
        /// 当前图像
        /// </summary>
        public ICogImage CurrentImage;
        /// <summary>
        /// 处理结果
        /// </summary>
        public bool Result = false;

        /// <summary>
        /// 视觉结果图形标注集合（渲染叠加在检测图像上的标注图形）
        /// </summary>
        public CogGraphicCollection ListGraphic = new CogGraphicCollection();

        /// <summary>
        /// 该相机的参数配置（序列号/曝光/增益/触发模式/存图开关，来自 JSON 配置）
        /// </summary>
        public CameraParameters MyCameraParameters { get; set; }
        /// <summary>
        /// 系统级配置（PLC 地址、串口等，来自 JSON 配置）
        /// </summary>
        public SystemConfigurations mSystemConfigurations { get; set; }
        ///// <summary>
        ///// 当前电脑连接的相机框架组
        ///// </summary>
        //public CogFrameGrabbers FrameGrabbers = new CogFrameGrabbers();
        ///// <summary>
        ///// 相机取相类
        ///// </summary>
        //public ICogAcqFifo CurAcqFifo;
        /// <summary>
        /// 当前相机
        /// </summary>
        public MyCamera CurrentCamera;
        /// <summary>
        /// 当前相机信息
        /// </summary>
        public MV_CC_DEVICE_INFO CurrentDevice;
        /// <summary>
        /// 帧大小
        /// </summary>
        public UInt32 PayloadSize = 0;
        /// <summary>
        /// 相机实时取像标志位
        /// </summary>
        public bool StartLiveImage = false;
        /// <summary>
        /// 实际图像值
        /// </summary>
        public UInt32 ImageSize = 0;
        /// <summary>
        /// 离线调试模式标志位
        /// </summary>
        public bool IsAutoStatus = true;

        /// <summary>
        /// 实际图像buffer
        /// </summary>
        public IntPtr ImageBuffer = IntPtr.Zero;
        /// <summary>
        /// 图像队列
        /// </summary>
        public ConcurrentQueue<ICogImage> ImageQueue = new ConcurrentQueue<ICogImage>();
        /// <summary>
        /// 车型
        /// </summary>
        public string CarModel = string.Empty;
        //public string FX = string.Empty;
        /// <summary>
        /// 处理线程唤醒信号：海康 SDK 取图回调入队后 Set()，
        /// 相机处理线程（阻塞在 WaitOne）被唤醒后执行 ViDi 推理与结果分发
        /// </summary>
        public AutoResetEvent mAutoResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// 满盘删除图片
        /// </summary>
        public void CheckAndDelete()
        {
            // 获取 exe 上级目录所在磁盘信息
            DriveInfo driveinfo_ = new DriveInfo(Path.GetFullPath("..").Substring(0, 3));
            // 剩余空间低于 50GB 时触发图片清理
            if (driveinfo_.AvailableFreeSpace < (long)50 * (long)1024 * (long)1024 * (long)1024)
            {
                // 扫描 ..\Images\ 下按日期命名的图片归档文件夹
                DirectoryInfo Info_ = new DirectoryInfo(Path.GetFullPath("..") + "\\Images\\");
                DirectoryInfo [] Info_s = Info_.GetDirectories();
                if (Info_s.Length > 0)
                {
                    DirectoryInfo fsinfo_d = Info_s[0];
                    DateTime LastActionTime = DateTime.Parse(fsinfo_d.Name);  //设置默认时间
                    // 遍历找出日期最早（最旧）的文件夹作为本次清理对象
                    foreach (DirectoryInfo fsinfo in Info_s)
                    {
                        if (LastActionTime > DateTime.Parse(fsinfo.Name))
                        {
                            LastActionTime = DateTime.Parse(fsinfo.Name);
                            fsinfo_d = fsinfo;
                        }

                    }
                    // 记录清理日志并递归删除该日期文件夹（含其下全部图片）
                    Log.Info($"所在盘满50G，删除图片文件夹，删最早一天：{fsinfo_d.FullName}");
                    fsinfo_d.Delete(true);
                } 
            }
        }

        /// <summary>
        /// 触发模式私有字段（"Auto"=硬件触发 LINE0 / "Manual"=软件触发）
        /// </summary>
        string triggerMode;
        /// <summary>
        /// 相机出发模式属性
        /// </summary>
        public string TriggerMode
        {
            get
            {
                return triggerMode;
            }
            set
            {
                triggerMode = value;
                switch (value)
                {
                    case "Auto"://自动运行(硬件触发)
                        int nRet1 = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                        Log.Info($"相机采集信息反馈：{nRet1}");
                        break;
                    case "Manual"://手动触发
                        int nRet = CurrentCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                        Log.Info($"相机采集信息反馈：{nRet}");
                        break;
                }
            }
        }

        /// <summary>
        /// 曝光时间私有字段（界面单位 ms，SDK 单位 µs）
        /// </summary>
        double exposure;
        /// <summary>
        /// 相机曝光属性
        /// </summary>
        public double Exposure
        {
            get
            {
                return exposure;
            }
            set
            {
                exposure = value;
                if (CurrentCamera != null)
                {
                    Log.Info($"{MyCameraParameters.CameraName}设置曝光时间返回值:{CurrentCamera.MV_CC_SetExposureTime_NET(((float)exposure) * 1000)}");
                    //CurrentCamera.MV_CC_SetExposureTime_NET(((float)exposure) * 1000);
                    //CurAcqFifo.OwnedExposureParams.Exposure = value;
                    Thread.Sleep(5);
                    //CurAcqFifo.Flush();
                }
            }
        }

        /// <summary>
        /// 增益私有字段
        /// </summary>
        double gain;
        /// <summary>
        /// 相机增益属性
        /// </summary>
        public double Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                if (CurrentCamera != null)
                {
                    Log.Info($"{MyCameraParameters.CameraName}设置增益返回值:{ CurrentCamera.MV_CC_SetGain_NET((float)gain)}");
                    Thread.Sleep(5);
                }
            }
        }

        /// <summary>
        /// 基类空构造：参数由派生类在各自构造函数中保存到对应属性
        /// </summary>
        /// <param name="systemConfigurations">系统级配置</param>
        /// <param name="cameraParameters">该相机的参数配置</param>
        public CameraBase(SystemConfigurations systemConfigurations, CameraParameters cameraParameters) { }
        /// <summary>
        /// 相机初始化（枚举设备按序列号匹配并打开取流），由派生类实现
        /// </summary>
        public abstract void Init();
        //public abstract void SearchCamera();
        //public abstract void OpenCamera();
        /// <summary>
        /// 运行图像
        /// </summary>
        public abstract void RunCameraImage();

        /// <summary>
        /// 软件触发拍照（开光源 → 延时 → 相机软触发 → 触发源切回硬件 LINE0），由派生类实现
        /// </summary>
        public abstract void SoftWareTrigger();
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string ConfigPath;
        /// <summary>
        /// 相机的生产配方的集合
        /// </summary>
        //public BindingList<ProductRecipe> CameraListProductRecipe = new BindingList<ProductRecipe>();
        /// <summary>
        /// ComBox配方显示
        /// </summary>
        public BindingList<string> ListProductRecipe = new BindingList<string>();
        /// <summary>
        /// 自动运行状态标志（由 PLC 自动/手动模式信号同步）
        /// </summary>
        public bool AutoStatus = true;
        /// <summary>
        /// 是否发生换型（换车型后重新加载对应配方）
        /// </summary>
        public bool IsChangeProduct = true;
    }

    /// <summary>
    /// 用户权限实体：记录用户名、密码与权限等级
    /// （权限：0=技术员(一级)、2=二级、5=管理员(三级)）
    /// </summary>
    public class User
    {
        /// <summary>
        /// 构造用户实体
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="code">密码</param>
        /// <param name="authority">权限等级（0=技术员、2=二级、5=管理员）</param>
        public User(string name, string code, int authority)
        {
            _Name = name;
            _Code = code;
            _Authority = authority;
        }

        /// <summary>
        /// 用户名私有字段
        /// </summary>
        private string _Name = null;
        /// <summary>
        /// 密码私有字段
        /// </summary>
        private string _Code = null;
        /// <summary>
        /// 权限等级私有字段
        /// </summary>
        private int _Authority = 0;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }

        /// <summary>
        /// 权限等级（0=技术员、2=二级、5=管理员）
        /// </summary>
        public int Authority
        {
            get { return _Authority; }
            set { _Authority = value; }
        }
        //用户名和密码是否匹配
        public bool Equal(string name, string code)
        {
            if (_Name == name && _Code == code)
                return true;
            else
                return false;
        }
    }
}
