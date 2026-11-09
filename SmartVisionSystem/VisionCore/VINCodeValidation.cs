// ============================================================
// 文件：VINCodeValidation.cs
// 模块：VisionCore（VIN 码校验）
// 说明：VIN 码合法性校验类。对 ViDi Blue-Read OCR 读出的 17 位完整 VIN 码执行
//       ISO 3779 加权校验（位置加权求和取模 11 对照第 9 位校验位，并检查车型年份码），
//       对 7 位短码执行 mod-11 校验，用于判定读码结果是否可信、是否需要复拍。
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ViDi2;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// VIN 码校验类：对 17 位完整 VIN 码做 ISO 3779 加权校验（校验位比对 + 车型年份码检查），
    /// 对 7 位短码做 mod-11 校验；校验通过说明 OCR 读码结果在数学上可信
    /// </summary>
    internal class VINCodeValidation
    {
        //readonly int currentYear = DateTime.Now.Year;
        // 车型年份代码表（30 年一循环）：数字 1~9 加 21 个字母（不含 VIN 禁用的 I/O/Q/U/Z），
        // 用于校验 VIN 年份位是否落在合理的生产年份区间内
        readonly string yearString = "123456789ABCDEFGHJKLMNPRSTVWXY";

        /// <summary>
        /// VIN 码校验入口：按有效长度分流——17 位走 ISO 3779 加权校验（先比对第 9 位校验位，
        /// 再检查年份位代码），7 位走 mod-11 短码校验，其它长度直接判为不合法
        /// </summary>
        /// <param name="vinCodeString">OCR 读出的 VIN 码字符串（可能含未识别占位符 *）</param>
        /// <returns>校验通过返回 true；长度不符、校验位或年份码不匹配返回 false</returns>
        public bool Validation(string vinCodeString)
        {
            // 当前年份，用于推算车型年份代码在 30 年循环表中的位置
            int currentYear = DateTime.Now.Year;
            // 剔除 OCR 未识别出的 * 占位符后统计有效长度
            int vinCodeStringLen = vinCodeString.Replace("*", "").Replace("?", "").Length;
            // 17 位完整 VIN 码：按 ISO 3779 加权算法校验
            if (vinCodeStringLen == 17)
            {
                // 当前年份对应的代码表索引（自 2000 年起 30 年一循环）
                int yearIndex = (currentYear - 2000) % 30;
                // 取有效字符的 ASCII 字节，逐位转换为数值并加权求和
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(vinCodeString.Replace("*", ""));
                int total = 0;
                for (int i = 0; i < vinCodeStringLen; i++)
                {
                    // 字符转数值：先减 '0'(48)，数字字符直接得到 0~9
                    int temp = byteArray[i] - 48;
                    // 字母经三级级联减法映射为 ISO 3779 字母数值表（A~H=1~8，J~R=1~9 跳过 I/O/Q，S~Z=2~9）
                    if (temp > 9) temp = temp - 16;
                    if (temp > 9) temp = temp - 9;
                    if (temp > 9) temp = temp - 8;
                    // 第 9 位（索引 8）是校验位本身，其数值清零，不参与加权和
                    if (i == 8) temp = 0;
                    // 位置权值：复现 ISO 3779 权重序列 8,7,6,5,4,3,2,10,0,9,8,7,6,5,4,3,2
                    int site = 9 - (i + 1) % 10;
                    // 第 8 位权值为 10（上式算出 1 时修正）
                    if (site == 1) site = 10;
                    total = total + site * temp;
                }
                //MessageBox.Show(vinCodeString[10].ToString());
                // 加权和取模 11，得到期望的校验值
                int totalx = total % 11;
                // 读取第 9 位实际校验位字符：数字按面值
                int totals = byteArray[8] - 48;
                // 校验位为字母（X）时按 10 处理
                if (totals > 9) totals = 10;
                if (totalx == totals)
                {
                    // 校验位匹配后，再检查索引 10 处的年份码：须命中当前年份前 4 年至后 2 年
                    // 对应的代码，避免误读或超龄车辆数据混入
                    if (vinCodeString[10] == GetYearChar(yearIndex + 1) | vinCodeString[10] == GetYearChar(yearIndex + 2) | vinCodeString[10] == GetYearChar(yearIndex) | vinCodeString[10] == GetYearChar(yearIndex - 1) | vinCodeString[10] == GetYearChar(yearIndex - 2) | vinCodeString[10] == GetYearChar(yearIndex - 3) | vinCodeString[10] == GetYearChar(yearIndex - 4))
                    {
                        string VINCode = vinCodeString;
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
            else if (vinCodeStringLen == 7)
            {
                //MessageBox.Show($"{vinCodeString}");
                // 7 位短码校验：第 1 位为校验位，后 6 位参与加权
                string temp = vinCodeString.Replace("*", "");
                int total = 0;
                // 第 2~7 位分别乘以权重 7~2 后求和
                for (int i = 1; i < vinCodeStringLen; i++)
                {
                    total = total + charToInt(temp[i]) * (8 - i);
                    //MessageBox.Show($"第{i}位的值{charToInt(temp[i])}");
                }

                // 加权和取模 11，得到期望的校验值
                int result = total % 11;
                //MessageBox.Show($"校验位值{result}");
                //MessageBox.Show($"校验结果{charToInt(temp[0]) == total}");
                // 余数为 10 时，校验位应为大写字母 X
                if (result == 10)
                {
                    if (temp[0].ToString() == "X")
                    {
                        return true;
                    }
                    else { return false; }
                }
                else
                {
                    if (charToInt(temp[0]) == result)
                    {
                        return true;
                    }
                    else { return false; }
                }
            }
            else
            {
                return false;
            }

        }
        //private int charToInt(char c)
        //{
        //    int temp = c;
        //    if ((temp >= 48 & temp <= 57) || (temp >= 65 & temp <= 90) & temp != 73 & temp!=79 & temp!=81)
        //    {
        //        temp = c - 48;
        //        if (temp > 9) temp = temp - 16;
        //        if (temp > 9) temp = temp - 1;
        //        if (temp > 9) temp = temp - 9;
        //        if (temp > 6) temp = temp - 1;
        //        if (temp > 7) temp = temp - 1;
        //        if (temp > 9) temp = temp - 9;
        //        return temp;
        //    }
        //    else
        //    {
        //        return 999;
        //    }


        //}
        /// <summary>
        /// 单字符转数值：数字字符按面值(0~9)返回；大写字母（排除 VIN 禁用的 I/O/Q）
        /// 经级联减法映射为 1~9 的数值；其它非法字符返回 999（调用方可据此判定短码不可信）
        /// </summary>
        /// <param name="c">待转换的字符</param>
        /// <returns>字符对应的数值；非法字符返回 999</returns>
        private int charToInt(char c)
        {
            // 取字符的 ASCII 码值
            int temp = c;
            // '0'~'9' 数字字符：减去 '0'(48) 后按面值返回
            if ((temp >= 48 && temp <= 57))
            {
                temp = c - 48;
                return temp;
            }
            // 'A'~'Z' 且排除 VIN 禁用字母 I/O/Q：经下方逐级减法映射为 1~9 的数值
            if ((temp >= 65 & temp <= 90) & temp != 73 & temp != 79 & temp != 81)
            {
                //temp = c - 48;
                //if (temp > 9) temp = temp - 16;
                //if (temp > 9) temp = temp - 1;
                //if (temp > 9) temp = temp - 9;
                //if (temp > 6) temp = temp - 1;
                //if (temp > 7) temp = temp - 1;
                //if (temp > 9) temp = temp - 9;
                //return temp;
                // 先减 '0'(48) 得到字母相对偏移量，再经下方逐级减法得到最终映射数值
                temp = c - 48;
                if (temp > 9)
                {
                    temp = temp - 16;
                }
                else
                {
                    return temp;
                }
                if (temp > 9)
                {
                    temp = temp - 1;
                }
                else
                {
                    return temp;
                }
                if (temp > 9)
                {
                    temp = temp - 9;
                }
                else
                {
                    return temp;
                }
                if (temp > 5)
                {
                    temp = temp - 1;
                }
                else
                {
                    return temp;
                }
                if (temp > 6)
                {
                    temp = temp - 1;
                }
                else
                {
                    return temp;
                }
                if (temp > 9)
                {
                    temp = temp - 9;
                    return temp;
                }
                else
                {
                    return temp;
                }
            }
            else
            {
                // 非法字符返回 999 作为异常标记（正常数值不可能达到）
                return 999;
            }


        }
        /// <summary>
        /// 按索引从 30 年循环的车型年份代码表中取对应字符；对索引做安全取模，
        /// 保证传入负数索引（年份早于表起点）或超界索引时也能正确环绕取值
        /// </summary>
        /// <param name="rawIndex">年份代码索引（允许为负数）</param>
        /// <returns>索引对应的年份代码字符</returns>
        private char GetYearChar(int rawIndex)
        {
            int len = yearString.Length;  // len = 30
            // 两次取模处理负索引，等价于数学意义上的取模，避免负数下标越界
            int safeIndex = ((rawIndex % len) + len) % len;
            return yearString[safeIndex];
        }
    }

}
