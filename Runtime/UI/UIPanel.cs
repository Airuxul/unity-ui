using UnityEngine;

namespace Air.UI
{
    // todo 通过 CanvasGroup进行显示和隐藏
    // [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : UIComponent
    {
        public UIPanelConfig UIPanelConfig { get; private set; }

        public void Init(UIPanelConfig uiPanelConfig)
        {
            UIPanelConfig = uiPanelConfig;
            base.Init();
        }
    }
}
