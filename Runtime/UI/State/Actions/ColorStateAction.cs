using System;
using UnityEngine;
using UnityEngine.UI;

namespace Air.UI.State
{
    /// <summary>
    /// 颜色状态动作
    /// 控制UI组件的颜色（Image、Text、Graphic）
    /// </summary>
    [Serializable]
    public class ColorStateAction : StateAction
    {
        [SerializeField] private Color targetColor = Color.white;

        public Color TargetColor
        {
            get => targetColor;
            set => targetColor = value;
        }

        public ColorStateAction()
        {
        }

        public ColorStateAction(GameObject target, Color color)
        {
            targetObject = target;
            targetColor = color;
        }

        public override void Apply()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("[ColorStateAction] 目标对象为空，无法应用颜色状态");
                return;
            }

            // 尝试获取Image组件
            var image = targetObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = targetColor;
                return;
            }

            // 尝试获取Text组件
            var text = targetObject.GetComponent<Text>();
            if (text != null)
            {
                text.color = targetColor;
                return;
            }

            // 尝试获取Graphic组件（Image和Text的基类）
            var graphic = targetObject.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.color = targetColor;
                return;
            }

            Debug.LogWarning($"[ColorStateAction] 目标对象 {targetObject.name} 上没有找到Image、Text或Graphic组件");
        }

        public override string GetActionTypeName()
        {
            return "颜色控制";
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            // 检查目标对象是否有可以修改颜色的组件
            return targetObject.GetComponent<Graphic>() != null;
        }
    }
}
