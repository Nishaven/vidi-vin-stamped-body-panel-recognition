// ============================================================
// 文件：TOP-GPIO-1808.cs
// 模块：VisionCore（硬件 API）
// 说明：TOP-GPIO-1808 数字 IO 板卡 BMI.dll 的 P/Invoke 声明：
//       DO1~DO8（Pin 0~7）输出 / DI1~DI8（Pin 8~15）输入，
//       程序轮询 DI1（Pin 8）低电平作为相机触发信号。
// ============================================================
using System.Runtime.InteropServices;

namespace CTOPGPIO1808
{
    /// <summary>
    /// TOP-GPIO-1808 数字 IO 板卡 P/Invoke 封装（初始化/去初始化/读写引脚电平），
    /// 引脚映射：DO1~DO8 = Pin 0~7（输出），DI1~DI8 = Pin 8~15（输入）
    /// </summary>
    public static class TOPGPIO1808
    {
        /*
         * 函数功能
         * 	初始化GPIO
         * 
         * 参数
         * 	PinCount  -- 保存GPIO引脚数量的指针
         * 
         * 返回值
         * 	0    -- 成功
         *  非0  -- 失败
         */
        [DllImport("BMI.dll")]
        public static extern int TOPGPIO1808_Init(ref uint PinCount);

        /*
         * 函数功能
         * 	去初始化GPIO，在程序退出时调用以释放资源
         * 
         * 参数
         * 	无
         * 
         * 返回值
         * 	0    -- 成功
         *  非0  -- 失败
         */
        [DllImport("BMI.dll")]
        public static extern int TOPGPIO1808_Deinit();

        /*
         * 函数功能
         * 	读GPIO引脚电平，每次函数调用读取1个GPIO引脚的电平,此函数可以读回
         *	GPO的当前电平
         * 
         * 参数
         * 	Pin   -- GPIO引脚，0为GPIO0，1为GPIO1，依次类推
         *           注：对应TOP-GPIO-1808设备，8 ~ 15 对应DI1 ~ DI8
         *  Level  -- 保存GPIO引脚电平值的指针，指针指向的值为1时为高电平，
         *             0为低电平
         * 
         * 返回值
         *	0    -- 成功
         *  非0  -- 失败			
         */
       [DllImport("BMI.dll")]
        public static extern int TOPGPIO1808_ReadPin(uint Pin, ref uint Level);

       /*
        * 函数功能
        * 	写GPO引脚电平，每次函数调用写1个GPIO引脚的电平
        * 
        * 参数
        * 	Pin    -- GPO引脚，0为GPIO0，1为GPIO1，依次类推
        *            注：对于TOP-GPIO-1808设备， 0 ~ 7对应DO1 ~ DO8 
        *   Level  -- GPO引脚电平，1时为高电平，0为低电平
        * 
        * 返回值
        *	0    -- 成功
        *  非0  -- 失败			
        */
        [DllImport("BMI.dll")]
       public static extern int TOPGPIO1808_WritePin(uint Pin, uint Level);
    }
}
