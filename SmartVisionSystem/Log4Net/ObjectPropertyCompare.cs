// ============================================================
// 文件：ObjectPropertyCompare.cs
// 模块：Log4Net（属性比较器）
// 说明：按指定属性比较两个对象（支持 IComparable 与字符串退化比较），
//       供 BindingCollection.ApplySortCore 排序时使用。
// ============================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVisionSystem.Log4Net
{
    /// <summary>
    /// 通用属性比较器：按指定属性名与排序方向比较两个对象，
    /// 配合可排序绑定集合实现 DataGridView 点击表头排序
    /// </summary>
    public class ObjectPropertyCompare<T> : IComparer<T>
    {
        /// <summary>
        /// 排序依据的属性描述符
        /// </summary>
        private readonly PropertyDescriptor property;
        /// <summary>
        /// 排序方向（升序/降序）
        /// </summary>
        private readonly ListSortDirection direction;

        /// <summary>
        /// 构造属性比较器
        /// </summary>
        /// <param name="property">排序依据属性</param>
        /// <param name="direction">排序方向</param>
        public ObjectPropertyCompare(PropertyDescriptor property, ListSortDirection direction)
        {
            this.property = property;
            this.direction = direction;
        }

        #region IComparer<T>

        /// <summary>
        /// 比较方法
        /// </summary>
        /// <param name="x">相对属性x</param>
        /// <param name="y">相对属性y</param>
        /// <returns></returns>
        public int Compare(T x, T y)
        {
            object xValue = x.GetType().GetProperty(property.Name).GetValue(x, null);
            object yValue = y.GetType().GetProperty(property.Name).GetValue(y, null);
            // 空值约定：null 视为最小（x 为 null 且 y 为 null 相等，否则 x 更小）
            int returnValue;
            if (xValue == null)
            {
                if (yValue == null)
                    returnValue = 0;
                else
                    returnValue = -1;
            }
            else
            {
                if (yValue == null)
                    returnValue = 1;
                else
                {
                    // 实现了 IComparable 的值类型直接比较，否则退化为字符串比较
                    if (xValue is IComparable)
                    {
                        returnValue = ((IComparable)xValue).CompareTo(yValue);
                    }
                    else if (xValue.Equals(yValue))
                    {
                        returnValue = 0;
                    }
                    else
                    {
                        returnValue = xValue.ToString().CompareTo(yValue.ToString());
                    }
                }
            }
            // 降序时反转比较结果
            if (direction == ListSortDirection.Ascending)
            {
                return returnValue;
            }
            else
            {
                return returnValue * -1;
            }
        }

        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        public bool Equals(T xWord, T yWord)
        {
            return xWord.Equals(yWord);
        }

        /// <summary>
        /// 获取对象哈希值
        /// </summary>
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
