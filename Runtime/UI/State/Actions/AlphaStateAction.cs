using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// Alpha透明度状态动作
    /// 控制CanvasGroup的透明度
    /// </summary>
    [Serializable]
    public class AlphaStateAction : StateAction
    {
        [SerializeField, Range(0f, 1f)] private float targetAlpha = 1f;

        public float TargetAlpha
        {
            get => targetAlpha;
            set => targetAlpha = Mathf.Clamp01(value);
        }

        public AlphaStateAction()
        {
        }

        public AlphaStateAction(GameObject target, float alpha)
        {
            targetObject = target;
            targetAlpha = Mathf.Clamp01(alpha);
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[AlphaStateAction] 目标对象为空，无法应用透明度状态");
                return;
            }

            var canvasGroup = targetObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = targetAlpha;
            }
            else
            {
                Debug.LogWarning($"[AlphaStateAction] 目标对象 {targetObject.name} 上没有找到CanvasGroup组件");
            }
        }

        public override string GetActionTypeName()
        {
            return "透明度控制";
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;
            return targetObject.GetComponent<CanvasGroup>() != null;
        }
    }
}
