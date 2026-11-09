// ============================================================
// 文件：SystemConfigurations.cs
// 模块：VisionCore（系统配置）
// 说明：整机系统配置实体，对应 ..\Configurations\CameraConfigurations.json，
//       保存 PLC 地址、光源串口参数及 10 组"车型×VIN 相机"曝光/增益配方。
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// 系统配置实体：由 JSON 配置文件反序列化而来，程序启动时读取、
    /// 设置界面保存时写回（保存时自动备份旧配置）
    /// </summary>
    public class SystemConfigurations
    {
        /// <summary>
        /// PLC通信IP地址
        /// </summary>
        public string PLCCommunicationIP { get; set; }
        /// <summary>
        /// PLC通信端口号
        /// </summary>
        public int PLCCommunicationPort { get; set; }
        /// <summary>
        /// 光源控制器的COM口
        /// </summary>
        public string LightControlCOMName { get; set; }
        /// <summary>
        /// 光源控制器的COM2口
        /// </summary>
        public string LightControlCOMName2 { get; set; }
        /// <summary>
        /// AM8光源口
        /// </summary>
        public string LightIndex { get; set; }
        /// <summary>
        /// 发送PLCVIN码是否去除*号
        /// </summary>
        public bool ISPAOn2 { get; set; }
        /// <summary>
        /// 自动取图信号
        /// </summary>
        public bool ISAutoSaveImage { get; set; }
        /// <summary>
        /// 系统在线地址(心跳信号)
        /// </summary>
        public string SystemOnlineAddress { get; set; }
        /// <summary>
        /// PLC自动手动地址
        /// </summary>
        public string AutoModelAddress { get; set; }
        /// <summary>
        /// 系统在线地址
        /// </summary>
        public string AutoModelAddress2 { get; set; }
        /// <summary>
        /// 系统在线地址
        /// </summary>
        public string CameraTriggerAddress { get; set; }
        /// <summary>
        /// PLC暂停地址
        /// </summary>
        //public string PLCStopAddress { get; set; }
        /// <summary>
        /// 车型识别相机返修信号
        /// </summary>
        public string CarModelCameraFXAddress { get; set; }
        /// <summary>
        /// 车型识别相机OK信号
        /// </summary>
        public string CarModelCameraOKAddress { get; set; }
        /// <summary>
        /// 车型识别相机NG信号
        /// </summary>
        public string CarModelCameraNGAddress { get; set; }
        /// <summary>
        /// 车型识别相机车型码地址
        /// </summary>
        public string CarModelCameraCarModelAddress { get; set; }
        /// <summary>
        /// VIN码相机OK信号
        /// </summary>
        public string VINCodeCameraOKAddress { get; set; }
        /// <summary>
        /// VIN码相机NG信号
        /// </summary>
        public string VINCodeCameraNGAddress { get; set; }
        /// <summary>
        /// VIN码地址To PLC
        /// </summary>
        public string VINCodeCameraVINCodeAddress { get; set; }
        /// <summary>
        /// VIN检测完成信号地址
        /// </summary>
        public string JobRunDoneAddress { get; set; }
        /// <summary>
        /// 相机参数
        /// </summary>
        public List<CameraParameters> ListCameraParameters { get; set; } = new List<CameraParameters>();
    }
}
