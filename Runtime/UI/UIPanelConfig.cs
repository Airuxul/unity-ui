using System;
using System.Collections.Generic;

namespace Air.UI
{
    public enum EPanelLayer
    {
        Pop,
        Top
    }
    
    /// <summary>
    /// UI配置数据
    /// </summary>
    public class UIPanelConfig
    {
        public string UIPanelId;
        
        /// <summary>
        /// 显示顺序，数值越大越靠前
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Prefab资源路径
        /// </summary>
        public string PrefabPath { get; set; }

        /// <summary>
        /// UI类型（用于实例化后类型转换）
        /// </summary>
        public Type PanelType { get; set; }
        
        /// <summary>
        /// UI Show 参数类型
        /// </summary>
        public Type ShowParamType { get; set; }

        /// <summary>
        /// UI所在层级
        /// </summary>
        public EPanelLayer UILayer { get; set; }
    }
}
