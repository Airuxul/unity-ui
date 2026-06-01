using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// 状态动作抽象基类
    /// </summary>
    [Serializable]
    public abstract class StateAction
    {
        [SerializeField] protected GameObject targetObject;

        public GameObject TargetObject => targetObject;

        /// <summary>
        /// 应用状态动作
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// 获取动作类型名称（用于编辑器显示）
        /// </summary>
        public abstract string GetActionTypeName();

        /// <summary>
        /// 验证动作是否有效
        /// </summary>
        public virtual bool IsValid()
        {
            return targetObject != null;
        }
    }
}
