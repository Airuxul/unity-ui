using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Air.UI
{
    /// <summary>
    /// 挂在带 Text 或 TMP 的物体上，序列化多语言 key。检视器字段画在文本组件底部。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [ExecuteAlways]
    public sealed class UILocaleBinding : MonoBehaviour
    {
        [SerializeField] string key;

        public string Key => key;

        public void SetKey(string value)
        {
            key = value ?? "";
        }

        void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            var value = UILocale.Lookup(key);
            if (value == null) return;
            var uiText = GetComponent<Text>();
            if (uiText != null && uiText.text != value) uiText.text = value;
            var tmp = GetComponent<TMP_Text>();
            if (tmp != null && tmp.text != value) tmp.text = value;
        }

        public static void ApplyTo(TMP_Text label, string key)
        {
            if (label == null) return;
            var binding = label.GetComponent<UILocaleBinding>();
            if (binding == null) return;
            binding.SetKey(key);
            binding.Apply();
        }
    }
}
