using System;
using UnityEngine;
using UnityEngine.Events;

namespace Air.UI.Trigger
{
    /// <summary>
    /// 触发 Unity 事件的动作。执行时调用绑定的 UnityEvent，然后立即完成并触发下一个事件。
    /// </summary>
    [Serializable]
    public class EventTriggerAction : TriggerActionBase
    {
        [SerializeField] private UnityEvent onTrigger = new();

        /// <summary>
        /// 绑定的触发事件。在 Inspector 中可配置无参回调。
        /// </summary>
        public UnityEvent OnTrigger => onTrigger;

        public EventTriggerAction() {}

        public EventTriggerAction(UnityEvent triggerEvent)
        {
            onTrigger = triggerEvent;
        }
        
        public override void Execute(Action onComplete)
        {
            try
            {
                onTrigger?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventTriggerAction] 执行事件失败: {e.Message}");
            }

            onComplete?.Invoke();
        }

        public override string GetActionTypeName()
        {
            return "触发事件";
        }

        public override bool IsValid()
        {
            return onTrigger != null;
        }
    }
}
