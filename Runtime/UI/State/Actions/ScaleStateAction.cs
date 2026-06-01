using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// 缩放状态动作
    /// 控制GameObject的缩放
    /// </summary>
    [Serializable]
    public class ScaleStateAction : StateAction
    {
        [SerializeField] private Vector3 targetScale = Vector3.one;

        public Vector3 TargetScale
        {
            get => targetScale;
            set => targetScale = value;
        }

        public ScaleStateAction()
        {
        }

        public ScaleStateAction(GameObject target, Vector3 scale)
        {
            targetObject = target;
            targetScale = scale;
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[ScaleStateAction] 目标对象为空，无法应用缩放状态");
                return;
            }

            targetObject.transform.localScale = targetScale;
        }

        public override string GetActionTypeName()
        {
            return "缩放控制";
        }
    }
}
