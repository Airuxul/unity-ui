using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// 交互状态动作
    /// 控制CanvasGroup的交互性和射线检测
    /// </summary>
    [Serializable]
    public class InteractableStateAction : StateAction
    {
        [SerializeField] private bool interactable = true;
        [SerializeField] private bool blockRaycasts = true;

        public bool Interactable
        {
            get => interactable;
            set => interactable = value;
        }

        public bool BlockRaycasts
        {
            get => blockRaycasts;
            set => blockRaycasts = value;
        }

        public InteractableStateAction()
        {
        }

        public InteractableStateAction(GameObject target, bool interactable, bool blockRaycasts = true)
        {
            targetObject = target;
            this.interactable = interactable;
            this.blockRaycasts = blockRaycasts;
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[InteractableStateAction] 目标对象为空，无法应用交互状态");
                return;
            }

            var canvasGroup = targetObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = blockRaycasts;
            }
            else
            {
                Debug.LogWarning($"[InteractableStateAction] 目标对象 {targetObject.name} 上没有找到CanvasGroup组件");
            }
        }

        public override string GetActionTypeName()
        {
            return "交互控制";
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;
            return targetObject.GetComponent<CanvasGroup>() != null;
        }
    }
}
