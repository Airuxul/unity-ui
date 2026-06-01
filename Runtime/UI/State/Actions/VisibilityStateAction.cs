using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// 显影状态动作
    /// 控制GameObject的显示/隐藏
    /// </summary>
    [Serializable]
    public class VisibilityStateAction : StateAction
    {
        [SerializeField] private bool isVisible = true;

        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        public VisibilityStateAction()
        {
        }

        public VisibilityStateAction(GameObject target, bool visible = true)
        {
            targetObject = target;
            isVisible = visible;
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[VisibilityStateAction] 目标对象为空，无法应用显影状态");
                return;
            }

            targetObject.SetActive(isVisible);
        }

        public override string GetActionTypeName()
        {
            return "显影控制";
        }
    }
}
