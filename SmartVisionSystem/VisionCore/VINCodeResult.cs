// ============================================================
// 文件：VINCodeResult.cs
// 模块：VisionCore（检测结果实体）
// 说明：单次读码检测结果：生产时间/相机名/车型/返修标记/VIN 码/判定结果。
//       实例由各 VIN 相机推理后构造，入 VINCodeResultQueue 供 UI 表格绑定显示。
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// VIN 码读码检测结果实体：一条记录对应一次相机推理结果，
    /// 同时用于 UI 结果表格显示与 SQLite 结果表落库
    /// </summary>
    public class VINCodeResult
    {
        /// <summary>
        /// 生产时间
        /// </summary>
        public DateTime RecordTime { get; set; }
        /// <summary>
        /// 相机名称
        /// </summary>
        public string CameraName { get; set; }
        /// <summary>
        /// 识别车型
        /// </summary>
        public string CarModel { get; set; }
        // <summary>
        /// 返修结果
        /// </summary>
        public string FX{ get; set; }
        /// <summary>
        /// VIN码
        /// </summary>
        public string VINCode { get; set; }
        /// <summary>
        /// 判断结果
        /// </summary>
        public bool Result { get; set; }
    }
}
