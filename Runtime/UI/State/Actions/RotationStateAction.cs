using System;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// 旋转状态动作
    /// 控制GameObject的旋转
    /// </summary>
    [Serializable]
    public class RotationStateAction : StateAction
    {
        [SerializeField] private Vector3 targetRotation = Vector3.zero;

        public Vector3 TargetRotation
        {
            get => targetRotation;
            set => targetRotation = value;
        }

        public RotationStateAction()
        {
        }

        public RotationStateAction(GameObject target, Vector3 rotation)
        {
            targetObject = target;
            targetRotation = rotation;
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[RotationStateAction] 目标对象为空，无法应用旋转状态");
                return;
            }

            targetObject.transform.localEulerAngles = targetRotation;
        }

        public override string GetActionTypeName()
        {
            return "旋转控制";
        }
    }
}
