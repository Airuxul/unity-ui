using System.IO;
using UnityEditor;
using UnityEngine;
using Air.UI;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI脚本生成器窗口
    /// </summary>
    public class UIGeneratorWindow : EditorWindow
    {
        private const string BaseOutputFolder = "Assets/Scripts/Generated/UI";
        private GameObject _targetGameObject;
        private string _className;
        private string _outputFolder;
        private UIType _uiType = UIType.Panel;
        private UIComponent _cachedUIComponent;
        
        [MenuItem("Window/AirUI/UI Script Generator")]
        public static void ShowWindow()
        {
            GetWindow<UIGeneratorWindow>("UI Script Generator");
        }
        
        /// <summary>
        /// 设置目标GameObject（用于从右键菜单打开时预填充）
        /// </summary>
        /// <param name="gameObject">目标GameObject</param>
        public static void SetTargetGameObject(GameObject gameObject)
        {
            var window = GetWindow<UIGeneratorWindow>("UI Script Generator");
            window._targetGameObject = gameObject;
            window._className = gameObject.name;
            
            // 检测已有UIComponent并设置类型
            window._cachedUIComponent = gameObject.GetComponent<UIComponent>();
            if (window._cachedUIComponent != null)
            {
                System.Type componentType = window._cachedUIComponent.GetType();
                window._uiType = UITypeConfig.GetTypeByBaseClass(componentType);
                window._className = componentType.Name;
                
                // 更新输出文件夹
                string existingScriptPath = UIScriptGenerator.FindDesignerScriptPath(window._className);
                if (!string.IsNullOrEmpty(existingScriptPath))
                {
                    window._outputFolder = Path.GetDirectoryName(existingScriptPath);
                }
            }
            else
            {
                // 新组件，使用默认类型和文件夹
                window.UpdateOutputFolderByType();
            }
            
            window.Focus();
        }
        
        private void OnEnable()
        {
            minSize = new Vector2(400, 300);
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawMainSettings();
            DrawGenerateButton();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 18;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("AirUI Script Generator", titleStyle);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "拖拽GameObject到下方区域，设置类名和输出文件夹，然后生成UI脚本",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawMainSettings()
        {
            EditorGUILayout.LabelField("设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // GameObject拖拽区域
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("目标GameObject", EditorStyles.boldLabel);
            
            if (_targetGameObject == null)
            {
                EditorGUILayout.HelpBox("请拖拽一个GameObject到下方区域", MessageType.Info);
            }
            
            GameObject newTarget = EditorGUILayout.ObjectField("当前选择:", _targetGameObject, typeof(GameObject), true) as GameObject;
            
            // 如果目标GameObject改变，更新缓存
            if (newTarget != _targetGameObject)
            {
                _targetGameObject = newTarget;
                _cachedUIComponent = _targetGameObject != null ? _targetGameObject.GetComponent<UIComponent>() : null;
                
                // 如果检测到已有UIComponent，自动判断类型
                if (_cachedUIComponent != null)
                {
                    System.Type componentType = _cachedUIComponent.GetType();
                    _uiType = UITypeConfig.GetTypeByBaseClass(componentType);
                    _className = componentType.Name;
                    
                    // 更新输出文件夹
                    string existingScriptPath = UIScriptGenerator.FindDesignerScriptPath(_className);
                    if (!string.IsNullOrEmpty(existingScriptPath))
                    {
                        _outputFolder = Path.GetDirectoryName(existingScriptPath);
                    }
                }
            }
            
            // 检测是否已有UIComponent并显示提示
            if (_cachedUIComponent != null)
            {
                string componentTypeName = _cachedUIComponent.GetType().Name;
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    $"该GameObject已挂载 UIComponent: {componentTypeName}\n" +
                    "将只更新 Designer 脚本并重新绑定字段", 
                    MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // 类型选择
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("类型选择", EditorStyles.boldLabel);
            
            // 如果已有组件，禁用类型选择
            bool hasExistingComponent = _cachedUIComponent != null;
            GUI.enabled = !hasExistingComponent;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UI类型", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            
            // 动态生成类型选择按钮
            UIType[] allTypes = UITypeConfig.GetAllTypes();
            foreach (UIType type in allTypes)
            {
                UITypeInfo info = UITypeConfig.GetInfo(type);
                bool isSelected = GUILayout.Toggle(_uiType == type, info.DisplayName, "Button", GUILayout.Width(100));
                
                if (isSelected && _uiType != type)
                {
                    _uiType = type;
                    UpdateOutputFolderByType();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            
            // 显示当前类型信息
            UITypeInfo currentInfo = UITypeConfig.GetInfo(_uiType);
            if (hasExistingComponent)
            {
                EditorGUILayout.HelpBox($"类型由已有的UIComponent决定，当前类型：{currentInfo.DisplayName}", MessageType.Info);
            }
            else
            {
                string templateInfo = $"继承自 {currentInfo.BaseClassName}，命名空间 {currentInfo.Namespace}";
                EditorGUILayout.HelpBox($"当前类型：{currentInfo.DisplayName}\n{templateInfo}", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // 类名设置
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("脚本设置", EditorStyles.boldLabel);
            
            // 如果已有组件，禁用类名编辑
            GUI.enabled = !hasExistingComponent;
            _className = EditorGUILayout.TextField("类名", _className);
            GUI.enabled = true;
            
            if (hasExistingComponent)
            {
                EditorGUILayout.HelpBox("类名来自已有的UIComponent，无法修改", MessageType.Info);
            }
            else if (string.IsNullOrEmpty(_className))
            {
                EditorGUILayout.HelpBox("类名不能为空", MessageType.Warning);
            }
            
            // 输出文件夹
            EditorGUILayout.BeginHorizontal();
            
            // 如果已有组件，尝试自动填充输出文件夹
            if (hasExistingComponent && !string.IsNullOrEmpty(_className))
            {
                string existingScriptPath = UIScriptGenerator.FindDesignerScriptPath(_className);
                if (!string.IsNullOrEmpty(existingScriptPath))
                {
                    _outputFolder = Path.GetDirectoryName(existingScriptPath);
                }
            }
            
            // 如果已有组件，禁用文件夹选择
            GUI.enabled = !hasExistingComponent;
            _outputFolder = EditorGUILayout.TextField("输出文件夹", _outputFolder);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择输出文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 转换为相对路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        _outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        _outputFolder = selectedPath;
                    }
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            // 提示输出路径信息
            if (hasExistingComponent)
            {
                EditorGUILayout.HelpBox("输出文件夹来自现有Designer脚本位置", MessageType.Info);
            }
            else
            {
                UITypeInfo info = UITypeConfig.GetInfo(_uiType);
                EditorGUILayout.HelpBox($"当前输出到：{info.FolderName} 文件夹", MessageType.Info);
            }
            
            // 创建文件夹按钮
            if (!Directory.Exists(_outputFolder))
            {
                if (GUILayout.Button("创建输出文件夹", GUILayout.Height(25)))
                {
                    Directory.CreateDirectory(_outputFolder);
                    AssetDatabase.Refresh();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawGenerateButton()
        {
            EditorGUILayout.Space(20);
            
            // 根据是否已有UIComponent显示不同的按钮文本
            string buttonText = _cachedUIComponent != null ? "更新Designer脚本" : "生成UI脚本";
            
            // 生成按钮
            GUI.enabled = CanGenerateScript();
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                GenerateScript();
            }
            GUI.enabled = true;
        }
        
        private bool CanGenerateScript()
        {
            return _targetGameObject != null && 
                   !string.IsNullOrEmpty(_className) && 
                   !string.IsNullOrEmpty(_outputFolder);
        }
        
        private void GenerateScript()
        {
            if (!CanGenerateScript())
            {
                EditorUtility.DisplayDialog("错误", "请检查所有必填项", "确定");
                return;
            }
            
            try
            {
                // 使用新的接口生成脚本，传递类型参数
                UIScriptGenerator.GenerateUIScript(_targetGameObject, _className, _outputFolder, _uiType);
                
                // 刷新AssetDatabase
                AssetDatabase.Refresh();
                
                // 选中生成的脚本文件
                string logicScriptPath = Path.Combine(_outputFolder, _className + ".cs");
                Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(logicScriptPath);
                if (!File.Exists(logicScriptPath)) return;
                if (scriptAsset == null) return;
                Selection.activeObject = scriptAsset;
                EditorGUIUtility.PingObject(scriptAsset);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"生成脚本时发生错误:\n{e.Message}", "确定");
                Debug.LogError($"UI脚本生成失败: {e}");
            }
        }
        
        /// <summary>
        /// 根据选择的类型更新输出文件夹
        /// </summary>
        private void UpdateOutputFolderByType()
        {
            // 如果已有组件，不更新输出文件夹（由现有脚本位置决定）
            if (_cachedUIComponent != null)
            {
                return;
            }
            
            UITypeInfo info = UITypeConfig.GetInfo(_uiType);
            _outputFolder = Path.Combine(BaseOutputFolder, info.FolderName);
        }
    }
}
