using System;
using System.Collections.Generic;

namespace Air.UI
{
    /// <summary>
    /// UI 展示参数标记接口，用于页面或组件的展示入参。
    /// </summary>
    public interface IUIShowParam
    {
        /// <summary>
        /// 将当前页面数据转换为指定组件参数类型（由 <see cref="ShowParamConverters"/> 统一分发）。
        /// </summary>
        bool TryTranslate<T>(out T componentParam) where T : struct, IUIShowParam;
    }

    /// <summary>
    /// 页面参数 → 组件参数的转换器注册表。页面实现 TryTranslate 时委托此处，新增组件只需注册一次。
    /// </summary>
    public static class ShowParamConverters
    {
        private static readonly Dictionary<(Type PageType, Type CompType), Delegate> Converters = new();

        /// <summary>
        /// 注册：从页面参数 TPage 转换为组件参数 TComp。
        /// 建议在页面参数类型的静态构造函数中调用。
        /// </summary>
        public static void Register<TShowParam1, TShowParam2>(Func<TShowParam1, TShowParam2> converter)
            where TShowParam1 : struct, IUIShowParam
            where TShowParam2 : struct, IUIShowParam
        {
            Converters[(typeof(TShowParam1), typeof(TShowParam2))] = converter;
        }

        /// <summary>
        /// 根据当前页面参数实例与目标组件类型，从注册表执行转换。
        /// </summary>
        public static bool TryConvert<TComp>(IUIShowParam page, out TComp componentParam)
            where TComp : struct, IUIShowParam
        {
            componentParam = default;
            if (page == null) return false;
            var key = (page.GetType(), typeof(TComp));
            if (!Converters.TryGetValue(key, out var del) || del == null) return false;
            try
            {
                componentParam = (TComp)del.DynamicInvoke(page);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
