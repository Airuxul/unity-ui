using UnityEngine;
using UnityEngine.UI;

namespace Air.UI.Extensions
{
    public static class ScrollRectExtensions
    {
        public static void Reflow(this ScrollRect scroll, float verticalNormalized)
        {
            if (scroll == null) return;
            Canvas.ForceUpdateCanvases();
            if (scroll.content != null) LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            scroll.verticalNormalizedPosition = verticalNormalized;
        }
    }
}
