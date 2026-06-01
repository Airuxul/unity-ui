using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// 位置状态动作
    /// 控制RectTransform的锚点位置
    /// </summary>
    [Serializable]
    public class PositionStateAction : StateAction
    {
        [SerializeField] private Vector2 anchoredPosition = Vector2.zero;

        public Vector2 AnchoredPosition
        {
            get => anchoredPosition;
            set => anchoredPosition = value;
        }

        public PositionStateAction()
        {
        }

        public PositionStateAction(GameObject target, Vector2 position)
        {
            targetObject = target;
            anchoredPosition = position;
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[PositionStateAction] 目标对象为空，无法应用位置状态");
                return;
            }

            var rectTransform = targetObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = anchoredPosition;
            }
            else
            {
                Debug.LogWarning($"[PositionStateAction] 目标对象 {targetObject.name} 上没有找到RectTransform组件");
            }
        }

        public override string GetActionTypeName()
        {
            return "位置控制";
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;
            return targetObject.GetComponent<RectTransform>() != null;
        }
    }
}
