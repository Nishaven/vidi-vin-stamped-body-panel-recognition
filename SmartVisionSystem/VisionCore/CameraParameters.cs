// ============================================================
// 文件：CameraParameters.cs
// 模块：VisionCore（相机参数配置）
// 说明：单台相机的参数实体（名称/序列号/曝光/增益/触发模式/存图开关），
//       与 SystemConfigurations 一同保存在 JSON 配置文件中。
// ============================================================
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// 相机参数配置实体：每台相机（车型识别相机 + 各 VIN 读码相机）各持有一份，
    /// 程序启动时从 JSON 配置反序列化，设置界面修改后写回
    /// </summary>
    public class CameraParameters
    {
        /// <summary>
        /// 相机名字
        /// </summary>
        public string CameraName { get; set; }
        /// <summary>
        /// 相机序列号
        /// </summary>
        public string CameraSerialNum { get; set; }
        /// <summary>
        /// 相机曝光时间
        /// </summary>
        public double CameraExposure { get; set; }
        /// <summary>
        /// 相机对比度
        /// </summary>
        public double CameraGain { get; set; }
        /// <summary>
        /// 相机序触发模式
        /// </summary>
        public string CameraTriggerMode { get; set; }
        /// <summary>
        /// 是否保存OK图像
        /// </summary>
        public bool IsSaveOKImage { get; set; }
        /// <summary>
        /// 是否保存NG图像
        /// </summary>
        public bool IsSaveNGImage { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string 备注 { get; set; }
    }
}
