using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Air.UI;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI组件类型管理器，用于识别基础UI组件和自定义UI组件
    /// </summary>
    public static class UIComponentTypes
    {
        /// <summary>
        /// 基础UI组件类型集合
        /// </summary>
        private static readonly HashSet<Type> BasicTypes = new()
        {
            typeof(Text),
            typeof(Image),
            typeof(Button),
            typeof(Toggle),
            typeof(Slider),
            typeof(InputField),
            typeof(Dropdown),
            typeof(ScrollRect),
            typeof(Scrollbar),
            typeof(CanvasGroup),
            typeof(UIComponent),
            typeof(TextMeshProUGUI)
        };

        /// <summary>
        /// 判断是否为基础UI组件类型
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果是基础UI组件类型返回true</returns>
        public static bool IsBasicType(Type type)
        {
            return BasicTypes.Contains(type);
        }

        /// <summary>
        /// 判断是否为自定义UI组件类型
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果是自定义UI组件类型返回true</returns>
        public static bool IsUIComponent(Type type)
        {
            return typeof(UIComponent).IsAssignableFrom(type) && type != typeof(UIComponent);
        }
    }
}
