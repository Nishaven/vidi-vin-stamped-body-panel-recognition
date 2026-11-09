// ============================================================
// 文件：BindingCollection.cs
// 模块：Log4Net（可排序绑定集合）
// 说明：支持点击表头排序的 BindingList 泛型集合，
//       用作 UI 日志表格与结果表格的数据源。
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
    /// 可排序的绑定集合：重写 BindingList 的排序内核属性与方法，
    /// 使 DataGridView 点击表头即可按该列排序
    /// </summary>
    public class BindingCollection<T> : BindingList<T>
    {
        /// <summary>
        /// 当前是否处于已排序状态
        /// </summary>
        private bool isSorted;
        /// <summary>
        /// 当前排序依据的属性描述符
        /// </summary>
        private PropertyDescriptor sortProperty;
        /// <summary>
        /// 当前排序方向（升序/降序）
        /// </summary>
        private ListSortDirection sortDirection;

        /// <summary>
        /// 是否已排序
        /// </summary>
        protected override bool IsSortedCore
        {
            get { return isSorted; }
        }

        /// <summary>
        /// 支持排序
        /// </summary>
        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        /// <summary>
        /// 当前排序方向
        /// </summary>
        protected override ListSortDirection SortDirectionCore
        {
            get { return sortDirection; }
        }

        /// <summary>
        /// 当前排序属性
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore
        {
            get { return sortProperty; }
        }

        /// <summary>
        /// 支持搜索
        /// </summary>
        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        /// <summary>
        /// 排序核心实现：用 ObjectPropertyCompare 比较器对内部列表排序，
        /// 完成后通知绑定控件整体刷新
        /// </summary>
        /// <param name="property">排序依据属性</param>
        /// <param name="direction">排序方向</param>
        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            if (this.Items is List<T> items)
            {
                ObjectPropertyCompare<T> pc = new ObjectPropertyCompare<T>(property, direction);
                items.Sort(pc);
                isSorted = true;
            }
            else
            {
                isSorted = false;
            }

            sortProperty = property;
            sortDirection = direction;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// 取消排序：清除已排序标志并通知绑定控件整体刷新
        /// </summary>
        protected override void RemoveSortCore()
        {
            isSorted = false;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        //排序
        public void Sort(PropertyDescriptor property, ListSortDirection direction)
        {
            this.ApplySortCore(property, direction);
        }
    }
}
