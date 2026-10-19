using UnityEngine;

namespace Air.UI
{
    public sealed class UIEscapeInput : MonoBehaviour
    {
        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || UIManager.Current == null) return;
            UIManager.Current.CloseTopPop();
        }
    }
}
