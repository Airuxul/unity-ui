using UnityEngine;
using UnityEditor;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI生成器右键菜单支持
    /// </summary>
    public static class UIGeneratorContextMenu
    {
        /// <summary>
        /// 在Hierarchy中右键生成UI脚本
        /// </summary>
        [MenuItem("GameObject/UI/AirUI/Generate UI Script")]
        public static void GenerateUIScriptFromContext()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择一个GameObject", "确定");
                return;
            }
            
            // 打开生成器窗口并预填充选中的GameObject
            UIGeneratorWindow.ShowWindow();
            UIGeneratorWindow.SetTargetGameObject(selectedObject);
        }
    }
}
