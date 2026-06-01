using System.IO;
using Air.UI;
using UnityEditor;
using UnityEngine;

namespace Air.UI.Editor
{
    /// <summary>
    /// UIComponent的自定义编辑器
    /// </summary>
    [CustomEditor(typeof(UIComponent), true)]
    public class UIComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Regenerate Designer"))
            {
                var uiComponent = (UIComponent)target;
                string className = uiComponent.GetType().Name;
                
                // 查找Designer脚本路径
                string designerScriptPath = UIScriptGenerator.FindDesignerScriptPath(className);
                if (!string.IsNullOrEmpty(designerScriptPath))
                {
                    string outputFolder = Path.GetDirectoryName(designerScriptPath);
                    UIType uiType = target is UIPanel? UIType.Panel: UIType.Component;
                    UIScriptGenerator.GenerateUIScript(uiComponent.gameObject, uiComponent.GetType().Name, outputFolder, uiType);
                }
                else
                {
                    Debug.LogError($"Could not find Designer script for {className}. Please generate scripts first.");
                }
            }
            
            if (GUILayout.Button("Bind Fields"))
            {
                var uiComponent = (UIComponent)target;
                uiComponent.ClearUIComponentFields();
                uiComponent.BindUIComponent();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
