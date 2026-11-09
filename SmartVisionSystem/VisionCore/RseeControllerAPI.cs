// ============================================================
// 文件：RseeControllerAPI.cs
// 模块：VisionCore（硬件 API）
// 说明：Rsee 光源控制器 DLL 的 P/Invoke 封装。
//       本类为预留代码，项目中未实际调用（当前光源控制走串口文本协议）。
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// Rsee 光源控制器 P/Invoke 封装（预留未使用）：
    /// 仅对外暴露打开串口封装方法，其余按通道亮度读写能力未在本项目调用
    /// </summary>
    public  class RseeControllerAPI
    {
        /// <summary>
        /// 打开 Rsee 控制器串口（P/Invoke 原始接口）
        /// </summary>
        [DllImport(@"RseeController.dll", EntryPoint = "RseeController_OpenCom", CallingConvention = CallingConvention.Cdecl)]
        extern static int RseeController_OpenCom(string szPort, int Baud_rate, bool overlapped);

        /// <summary>
        /// 关闭 Rsee 控制器串口（P/Invoke 原始接口）
        /// </summary>
        [DllImport(@"RseeController.dll", EntryPoint = "RseeController_CloseCom", CallingConvention = CallingConvention.Cdecl)]
        extern static bool RseeController_CloseCom(string szPort, int com);

        /// <summary>
        /// 设置 Rsee 控制器指定通道的亮度（P/Invoke 原始接口）
        /// </summary>
        [DllImport(@"RseeController.dll", EntryPoint = "RseeController_PM_D_BRTSetChannel", CallingConvention = CallingConvention.Cdecl)]
        extern static int RseeController_PM_D_BRTSetChannel(int com, int channel, int range);

        /// <summary>
        /// 读取 Rsee 控制器指定通道的亮度（P/Invoke 原始接口）
        /// </summary>
        [DllImport(@"RseeController.dll", EntryPoint = "RseeController_PM_D_BRTReadChannel", CallingConvention = CallingConvention.Cdecl)]
        extern static int RseeController_PM_D_BRTReadChannel(int com, int channel);

        /// <summary>
        /// 打开 Rsee 控制器串口的对外封装入口（返回 0 表示成功）
        /// </summary>
        /// <param name="szPort">串口名称，如 "COM3"</param>
        /// <param name="Baud_rate">波特率</param>
        /// <param name="overlapped">是否采用重叠（异步）IO 模式</param>
        /// <returns>0 表示成功，非 0 表示失败</returns>
        public static int RseeControllerOpenCom(string szPort, int Baud_rate, bool overlapped)
        { 
            return RseeController_OpenCom(szPort, Baud_rate, overlapped);
        }
    }
}
