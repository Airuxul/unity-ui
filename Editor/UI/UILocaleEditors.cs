using System;
using System.Collections;
using System.Reflection;
using Air.UI;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Air.UI.Editor
{
    [CustomEditor(typeof(Text), true)]
    [CanEditMultipleObjects]
    public class UILocaleTextEditor : UnityEditor.UI.TextEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UILocaleDrawer.Refresh(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UILocaleDrawer.Draw(targets);
        }
    }

    [CustomEditor(typeof(TextMeshProUGUI), true)]
    [CanEditMultipleObjects]
    public class UILocaleTmpUiEditor : TMP_EditorPanelUI
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UILocaleDrawer.Refresh(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UILocaleDrawer.Draw(targets);
        }
    }

    [CustomEditor(typeof(TextMeshPro), true)]
    [CanEditMultipleObjects]
    public class UILocaleTmpEditor : TMP_EditorPanel
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UILocaleDrawer.Refresh(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UILocaleDrawer.Draw(targets);
        }
    }

    static class UILocaleDrawer
    {
        static readonly GUIContent KeyLabel = new GUIContent("多语言 Key", "文案表中的编号，例如 i_123。");

        public static void Refresh(UnityEngine.Object[] targets)
        {
            for (int i = 0; i < targets.Length; i++)
                ApplyResolved(targets[i] as Component);
        }

        public static void Draw(UnityEngine.Object[] targets)
        {
            EditorGUILayout.Space();
            var shown = "";
            var mixed = false;
            var missing = false;
            for (int i = 0; i < targets.Length; i++)
            {
                var component = targets[i] as Component;
                var binding = component == null ? null : component.GetComponent<UILocaleBinding>();
                var value = binding == null ? "" : binding.Key;
                if (i == 0) shown = value;
                else if (shown != value) mixed = true;
                if (!string.IsNullOrEmpty(value) && UILocale.Lookup(value) == null) missing = true;
            }

            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            var next = EditorGUILayout.TextField(KeyLabel, shown);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; i++)
                    WriteKey(targets[i] as Component, next);
                missing = !string.IsNullOrEmpty(next) && UILocale.Lookup(next) == null;
            }

            if (missing)
                EditorGUILayout.HelpBox("文案表里没有这个 key。", MessageType.Warning);
        }

        static void WriteKey(Component component, string key)
        {
            if (component == null) return;
            var binding = component.GetComponent<UILocaleBinding>();
            if (string.IsNullOrEmpty(key))
            {
                if (binding != null) Undo.DestroyObjectImmediate(binding);
                return;
            }

            if (binding == null)
            {
                binding = Undo.AddComponent<UILocaleBinding>(component.gameObject);
                binding.hideFlags |= HideFlags.HideInInspector;
            }

            Undo.RecordObject(binding, "多语言 Key");
            binding.SetKey(key);
            EditorUtility.SetDirty(binding);
            ApplyResolved(component);
        }

        static void ApplyResolved(Component component)
        {
            if (component == null) return;
            var binding = component.GetComponent<UILocaleBinding>();
            if (binding == null || string.IsNullOrEmpty(binding.Key)) return;
            var value = UILocale.Lookup(binding.Key);
            if (value == null) return;

            if (component is Text uiText)
                WriteProperty(uiText, "m_Text", value);
            if (component is TMP_Text tmp)
            {
                WriteProperty(tmp, "m_text", value);
                if (tmp.text != value) tmp.text = value;
            }
        }

        static void WriteProperty(UnityEngine.Object target, string property, string value)
        {
            var serialized = new SerializedObject(target);
            var prop = serialized.FindProperty(property);
            if (prop == null || prop.stringValue == value) return;
            prop.stringValue = value;
            serialized.ApplyModifiedProperties();
        }
    }

    [InitializeOnLoad]
    static class UILocaleEditorPriority
    {
        static UILocaleEditorPriority()
        {
            Prefer(typeof(Text), typeof(UILocaleTextEditor));
            Prefer(typeof(TextMeshProUGUI), typeof(UILocaleTmpUiEditor));
            Prefer(typeof(TextMeshPro), typeof(UILocaleTmpEditor));
        }

        static void Prefer(Type inspected, Type editorType)
        {
            try
            {
                var attributes = typeof(CustomEditor).Assembly.GetType("UnityEditor.CustomEditorAttributes");
                if (attributes == null) return;
                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                MoveFirst(attributes.GetField("kSCustomEditors", flags), inspected, editorType);
                MoveFirst(attributes.GetField("kSCustomMultiEditors", flags), inspected, editorType);
                var cache = attributes.GetField("s_SearchCache", flags);
                (cache?.GetValue(null) as IList)?.Clear();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[UILocale] 未能挂上文本检视器。" + exception.Message);
            }
        }

        static void MoveFirst(FieldInfo field, Type inspected, Type editorType)
        {
            if (field == null) return;
            var dict = field.GetValue(null) as IDictionary;
            if (dict == null || !dict.Contains(inspected)) return;
            var list = dict[inspected] as IList;
            if (list == null) return;
            var inspectorField = (list.Count == 0 ? null : list[0].GetType().GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            if (inspectorField == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                if ((Type)inspectorField.GetValue(list[i]) != editorType) continue;
                if (i == 0) return;
                var item = list[i];
                list.RemoveAt(i);
                list.Insert(0, item);
                return;
            }
        }
    }
}
