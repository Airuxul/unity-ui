using System;
using UnityEngine;

namespace Air.UI.Trigger
{
    /// <summary>
    /// 触发动作抽象基类。支持执行完成后通过 onComplete 串联下一个动作或事件。
    /// </summary>
    [Serializable]
    public abstract class TriggerActionBase
    {
        /// <summary>
        /// 执行触发动作。完成后必须调用 onComplete，以支持串联下一个事件。
        /// </summary>
        /// <param name="onComplete">本动作执行完毕时的回调，用于继续触发下一个事件。</param>
        public abstract void Execute(Action onComplete);

        /// <summary>
        /// 获取动作类型名称（用于编辑器显示）。
        /// </summary>
        public abstract string GetActionTypeName();

        /// <summary>
        /// 验证动作是否有效、可执行。
        /// </summary>
        public virtual bool IsValid()
        {
            return true;
        }
    }
}
