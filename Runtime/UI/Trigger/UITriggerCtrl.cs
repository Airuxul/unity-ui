using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Air.UI.Trigger
{
    /// <summary>
    /// 单条触发绑定：由代码通过事件调用 Trigger 触发，按顺序执行多个动作，并可指定执行完后继续触发的下一个绑定。
    /// </summary>
    [Serializable]
    public class TriggerBinding
    {
        [SerializeField] private string bindingName = "New Binding";
        [SerializeReference] private List<TriggerActionBase> actions = new();
        [SerializeField] private string nextBindingName;

        /// <summary> 绑定名称，用于代码触发和串联时指定。 </summary>
        public string BindingName
        {
            get => bindingName;
            set => bindingName = value ?? string.Empty;
        }

        /// <summary> 按顺序执行的触发动作列表。 </summary>
        public List<TriggerActionBase> Actions => actions;

        /// <summary> 本绑定全部执行完后，若不为空则继续触发该名称的绑定。 </summary>
        public string NextBindingName
        {
            get => nextBindingName;
            set => nextBindingName = value;
        }

        /// <summary>
        /// 无参构造函数供 Unity 序列化使用。
        /// </summary>
        public TriggerBinding()
        {
        }

        public TriggerBinding(string name)
        {
            bindingName = name ?? string.Empty;
        }
    }

    /// <summary>
    /// UI 触发控制器。通过代码调用 Trigger 触发各类动作（事件、动画），支持触发后继续触发下一个绑定。
    /// </summary>
    [RequireComponent(typeof(UIComponent))]
    public class UITriggerCtrl : MonoBehaviour
    {
        [SerializeField] private List<TriggerBinding> bindings;

        private Dictionary<string, TriggerBinding> _bindingDict;
        private bool _initialized;

        /// <summary>
        /// 所有触发绑定（只读）。
        /// </summary>
        public IReadOnlyList<TriggerBinding> Bindings => bindings;

        private Dictionary<string, TriggerBinding> BindingDict
        {
            get
            {
                if (_bindingDict == null)
                {
                    BuildBindingDict();
                }

                return _bindingDict;
            }
        }

        private void Reset()
        {
            bindings = null;
            InitDefaultBindings();
        }

        private void BuildBindingDict()
        {
            InitDefaultBindings();
            RebindDefaultCallbacks();
            _bindingDict = new Dictionary<string, TriggerBinding>();
            foreach (var b in bindings)
            {
                if (b == null || string.IsNullOrEmpty(b.BindingName)) continue;
                if (!_bindingDict.TryAdd(b.BindingName, b))
                    Debug.LogWarning($"[UITriggerCtrl] 绑定名称重复: {b.BindingName}");
            }
        }

        public void InitDefaultBindings()
        {
            if (bindings != null) return;
            bindings = new List<TriggerBinding>();
            if (!TryGetComponent<UIPanel>(out var uiPanel))
            {
                return;
            }

            // 添加默认显示页面动画和事件逻辑
            var showUITriggerBinding = new TriggerBinding
            {
                BindingName = "ShowUI"
            };
            showUITriggerBinding.Actions.Add(
                new AnimationTriggerAction(
                    AnimationTriggerAction.AnimationSourceType.AnimatorTrigger,
                    gameObject,
                    "UIPanelShow",
                    true)
            );
            var panelShowAfterEvent = new UnityEvent();
            panelShowAfterEvent.AddListener(uiPanel.ShowAfter);
            showUITriggerBinding.Actions.Add(
                new EventTriggerAction(panelShowAfterEvent)
            );
            
            // 添加默认关闭页面动画和事件逻辑
            var closeUITriggerBinding = new TriggerBinding
            {
                BindingName = "HideUI"
            };
            closeUITriggerBinding.Actions.Add(
                new AnimationTriggerAction(
                    AnimationTriggerAction.AnimationSourceType.AnimatorTrigger,
                    gameObject,
                    "UIPanelHide",
                    true)
            );
            var panelCloseAfterEvent = new UnityEvent();
            panelCloseAfterEvent.AddListener(uiPanel.ShowAfter);
            closeUITriggerBinding.Actions.Add(
                new EventTriggerAction(panelCloseAfterEvent)
            );
            
            bindings.Add(showUITriggerBinding);
            bindings.Add(closeUITriggerBinding);
        }

        private void RebindDefaultCallbacks()
        {
            if (!TryGetComponent<UIPanel>(out var uiPanel) || bindings == null) return;
            RebindDefaultEvent("ShowUI", uiPanel.ShowAfter);
            RebindDefaultEvent("HideUI", uiPanel.ShowAfter);
        }

        private void RebindDefaultEvent(string bindingName, UnityAction callback)
        {
            if (callback == null) return;

            var binding = bindings.Find(b => b != null && b.BindingName == bindingName);
            if (binding == null) return;

            EventTriggerAction eventAction = null;
            foreach (var action in binding.Actions)
            {
                if (action is EventTriggerAction triggerAction)
                {
                    eventAction = triggerAction;
                    break;
                }
            }

            if (eventAction == null)
            {
                var unityEvent = new UnityEvent();
                unityEvent.AddListener(callback);
                binding.Actions.Add(new EventTriggerAction(unityEvent));
                return;
            }

            eventAction.OnTrigger.RemoveListener(callback);
            eventAction.OnTrigger.AddListener(callback);
        }

        /// <summary>
        /// 按绑定名称触发：依次执行该绑定的所有动作，然后若配置了“下一个绑定”则继续触发。
        /// </summary>
        /// <param name="bindingName">绑定名称。</param>
        public void Trigger(string bindingName)
        {
            if (string.IsNullOrEmpty(bindingName))
            {
                Debug.LogError("[UITriggerCtrl] 绑定名称为空");
                return;
            }

            if (!BindingDict.TryGetValue(bindingName, out var binding))
            {
                // Debug.LogError($"[UITriggerCtrl] 未找到绑定: {bindingName}");
                return;
            }

            ExecuteBinding(binding);
        }

        /// <summary>
        /// 执行一条绑定：按顺序执行动作，全部完成后若设置了 NextBindingName 则触发下一绑定。
        /// </summary>
        private void ExecuteBinding(TriggerBinding binding)
        {
            if (binding == null) return;

            var validActions = new List<TriggerActionBase>();
            foreach (var a in binding.Actions)
            {
                if (a != null && a.IsValid())
                    validActions.Add(a);
            }

            if (validActions.Count == 0)
            {
                TriggerNextIfSet(binding);
                return;
            }

            RunActionSequence(validActions, 0, () => TriggerNextIfSet(binding));
        }

        private void RunActionSequence(List<TriggerActionBase> list, int index, Action onAllComplete)
        {
            if (index >= list.Count)
            {
                onAllComplete?.Invoke();
                return;
            }

            list[index].Execute(() => RunActionSequence(list, index + 1, onAllComplete));
        }

        private void TriggerNextIfSet(TriggerBinding binding)
        {
            if (string.IsNullOrEmpty(binding.NextBindingName)) return;
            Trigger(binding.NextBindingName);
        }

        /// <summary>
        /// 获取指定名称的绑定。
        /// </summary>
        public TriggerBinding GetBinding(string bindingName)
        {
            BindingDict.TryGetValue(bindingName, out var binding);
            return binding;
        }

        public void TriggerUIPanelShow()
        {
            Trigger("ShowUI");
        }
    }
}
