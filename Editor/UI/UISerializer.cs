using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Air.UI;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI序列化器，负责自动绑定UI组件字段和挂载脚本
    /// </summary>
    public static class UISerializer
    {
        public enum EBindStatus
        {
            Success,
            Failed,
            Skip
        }
        
        private const string UIComponentParnentFieldName = "parent";
        private const string PendingGoPathKey = "UISerializer_PendingGameObjectPath";
        private const string PendingClassNameKey = "UISerializer_PendingClassName";
        private const string PendingPrefabPathKey = "UISerializer_PendingPrefabPath";
        private const string PendingNamespaceKey = "UISerializer_PendingNamespace";
        
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            // 检查是否有待处理的任务
            if (HasPendingTask())
            {
                ProcessPendingTask();
            }
        }
        
        /// <summary>
        /// 添加待处理的附件任务
        /// </summary>
        /// <param name="gameObjectPath">GameObject路径</param>
        /// <param name="className">类名</param>
        public static void AddPendingAttachment(string gameObjectPath, string className)
        {
            EditorPrefs.DeleteKey(PendingPrefabPathKey);
            EditorPrefs.DeleteKey(PendingNamespaceKey);
            EditorPrefs.SetString(PendingGoPathKey, gameObjectPath);
            EditorPrefs.SetString(PendingClassNameKey, className);
        }

        /// <summary>Queue attach+bind after script compile for a Resources prefab asset.</summary>
        public static void AddPendingPrefabAttachment(
            string prefabAssetPath,
            string className,
            string namespaceOverride = null)
        {
            EditorPrefs.DeleteKey(PendingGoPathKey);
            EditorPrefs.SetString(PendingPrefabPathKey, prefabAssetPath);
            EditorPrefs.SetString(PendingClassNameKey, className);
            if (!string.IsNullOrWhiteSpace(namespaceOverride))
                EditorPrefs.SetString(PendingNamespaceKey, namespaceOverride);
            else
                EditorPrefs.DeleteKey(PendingNamespaceKey);
        }

        /// <summary>Attach panel script and bind SerializeField refs on a prefab root (call before SaveAsPrefabAsset).</summary>
        public static bool TryAttachAndBindPrefabRoot(
            GameObject root,
            string className,
            string namespaceOverride = null)
        {
            if (!root) return false;
            var scriptType = ResolveScriptType(className, namespaceOverride);
            if (scriptType == null)
                return false;

            PostGenerateProcess(root, className, namespaceOverride);
            return root.TryGetComponent(scriptType, out _);
        }
        
        /// <summary>
        /// 检查是否有待处理的任务
        /// </summary>
        /// <returns>如果有待处理任务返回true</returns>
        private static bool HasPendingTask() =>
            EditorPrefs.HasKey(PendingPrefabPathKey) || EditorPrefs.HasKey(PendingGoPathKey);

        private static void ProcessPendingTask()
        {
            var className = EditorPrefs.GetString(PendingClassNameKey);
            var namespaceOverride = EditorPrefs.HasKey(PendingNamespaceKey)
                ? EditorPrefs.GetString(PendingNamespaceKey)
                : null;

            if (EditorPrefs.HasKey(PendingPrefabPathKey))
            {
                var prefabPath = EditorPrefs.GetString(PendingPrefabPathKey);
                Debug.Log($"ProcessPendingPrefabTask: {prefabPath}, {className}");
                ProcessPendingPrefabAttachment(prefabPath, className, namespaceOverride);
            }
            else
            {
                var gameObjectPath = EditorPrefs.GetString(PendingGoPathKey);
                Debug.Log($"ProcessPendingHierarchyTask: {gameObjectPath}, {className}");
                var go = FindGameObjectByPath(gameObjectPath);
                if (go != null)
                    PostGenerateProcess(go, className, namespaceOverride);
                else
                    Debug.LogError($"Could not find GameObject at path: {gameObjectPath}");
            }

            ClearPendingTask();
        }

        static void ProcessPendingPrefabAttachment(
            string prefabAssetPath,
            string className,
            string namespaceOverride)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            try
            {
                if (!TryAttachAndBindPrefabRoot(root, className, namespaceOverride))
                {
                    Debug.LogError(
                        $"Failed to attach {className} on prefab {prefabAssetPath}. Re-run ui.generate after compile.");
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabAssetPath);
                AssetDatabase.ImportAsset(prefabAssetPath, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void ClearPendingTask()
        {
            EditorPrefs.DeleteKey(PendingGoPathKey);
            EditorPrefs.DeleteKey(PendingClassNameKey);
            EditorPrefs.DeleteKey(PendingPrefabPathKey);
            EditorPrefs.DeleteKey(PendingNamespaceKey);
        }
        
        /// <summary>
        /// 清空UI组件的所有序列化字段引用
        /// </summary>
        /// <param name="uiComponent">UI组件实例</param>
        public static void ClearUIComponentFields(this UIComponent uiComponent)
        {
            if (!uiComponent) return;
            
            var componentType = uiComponent.GetType();
            var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            int clearedCount = 0;
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<SerializeField>() == null) continue;
                
                // 清空Component类型字段
                if (typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(uiComponent, null);
                    clearedCount++;
                }
                // 清空List<UIComponent>类型字段
                else if (IsListOfUIComponent(field.FieldType))
                {
                    field.SetValue(uiComponent, null);
                    clearedCount++;
                }
            }
            
            Debug.Log($"Cleared {clearedCount} field references from {uiComponent.GetType().Name}");
            EditorUtility.SetDirty(uiComponent);
        }
        
        /// <summary>
        /// 绑定UI组件字段
        /// </summary>
        /// <param name="uiComponent">UI组件实例</param>
        public static void BindUIComponent(this UIComponent uiComponent)
        {
            if (!uiComponent) return;
            
            var componentType = uiComponent.GetType();
            var fieldInfos = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            int boundCount = 0;
            int failedCount = 0;
            
            foreach (var fieldInfo in fieldInfos)
            {
                var bindStatus = BindUIComponentField(uiComponent, fieldInfo);
                switch (bindStatus)
                {
                    case EBindStatus.Success:
                        boundCount++;
                        break;
                    case EBindStatus.Failed:
                        failedCount++;
                        break;
                }
            }

            // 绑定UIComponent基类的childs字段
            BindBaseChildsField(uiComponent);

            uiComponent.BindParentUIComponent();
            
            Debug.Log($"Binding complete for {componentType.Name}: {boundCount} bound, {failedCount} failed");
            EditorUtility.SetDirty(uiComponent);
        }
        
        /// <summary>
        /// 绑定UIComponent基类的childs字段
        /// </summary>
        /// <param name="uiComponent">UI组件实例</param>
        private static void BindBaseChildsField(UIComponent uiComponent)
        {
            // 获取基类UIComponent中的childs字段
            FieldInfo childsField = typeof(UIComponent).GetField("childs", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (childsField == null)
            {
                Debug.LogWarning("Could not find childs field in UIComponent base class");
                return;
            }
            
            // 收集所有直接子UIComponent
            List<UIComponent> childsList = new List<UIComponent>();
            Transform transform = uiComponent.transform;
            
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent<UIComponent>(out var childUIComponent))
                {
                    childsList.Add(childUIComponent);
                }
            }
            
            // 设置childs字段值
            childsField.SetValue(uiComponent, childsList);
            
            if (childsList.Count > 0)
            {
                Debug.Log($"Bound base childs field with {childsList.Count} child UIComponents");
            }
        }

        public static EBindStatus BindUIComponentField(this UIComponent uiComponent, FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<SerializeField>() == null)
            {
                return EBindStatus.Skip;
            }
                
            string fieldName = fieldInfo.Name;
            Type fieldType = fieldInfo.FieldType;
            
            // 跳过Childs列表字段，这些字段会在绑定UIComponent字段时自动处理
            if (fieldName.EndsWith("Childs") && IsListOfUIComponent(fieldType))
            {
                return EBindStatus.Skip;
            }
                    
            // 只处理Component类型的字段
            if (!typeof(Component).IsAssignableFrom(fieldType))
            {
                return EBindStatus.Skip;
            }
            
            if (fieldName == UIComponentParnentFieldName)
            {
                return EBindStatus.Skip;
            }
                
            Component foundComponent = FindComponentInChildren(uiComponent.transform, fieldType, fieldName);
                
            if (!foundComponent)
            {
                return EBindStatus.Failed;
            }
                
            fieldInfo.SetValue(uiComponent, foundComponent);
            Debug.Log($"Bound field {fieldName} ({fieldType.Name}) to {foundComponent.name}");
                            
            // 如果绑定的是UIComponent类型，设置其parent字段为当前组件，并处理Childs列表
            if (foundComponent is UIComponent childUIComponent)
            {
                SetParentField(childUIComponent, uiComponent);
                
                // 查找并绑定对应的Childs列表字段
                BindChildsListField(uiComponent, fieldName, childUIComponent);
            }

            return EBindStatus.Success;
        }
        
        /// <summary>
        /// 检查类型是否为List&lt;UIComponent&gt;
        /// </summary>
        private static bool IsListOfUIComponent(Type fieldType)
        {
            if (!fieldType.IsGenericType) return false;
            if (fieldType.GetGenericTypeDefinition() != typeof(List<>)) return false;
            
            Type genericArg = fieldType.GetGenericArguments()[0];
            return genericArg == typeof(UIComponent) || genericArg.IsSubclassOf(typeof(UIComponent));
        }
        
        /// <summary>
        /// 绑定UIComponent的Childs列表字段
        /// </summary>
        /// <param name="parentComponent">父组件</param>
        /// <param name="uiComponentFieldName">UIComponent字段名</param>
        /// <param name="childUIComponent">子UIComponent实例</param>
        private static void BindChildsListField(UIComponent parentComponent, string uiComponentFieldName, UIComponent childUIComponent)
        {
            // 查找对应的Childs字段（命名规则：uiComponentFieldName + "Childs"）
            string childsFieldName = uiComponentFieldName + "Childs";
            
            Type parentType = parentComponent.GetType();
            FieldInfo childsField = parentType.GetField(childsFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (childsField == null)
            {
                // 没有对应的Childs字段，跳过
                return;
            }
            
            if (!IsListOfUIComponent(childsField.FieldType))
            {
                Debug.LogWarning($"Field {childsFieldName} is not of type List<UIComponent>");
                return;
            }
            
            // 收集子UIComponent的所有直接子UIComponent
            List<UIComponent> childsList = new List<UIComponent>();
            Transform childTransform = childUIComponent.transform;
            
            for (int i = 0; i < childTransform.childCount; i++)
            {
                Transform grandChild = childTransform.GetChild(i);
                if (grandChild.TryGetComponent<UIComponent>(out var grandChildUIComponent))
                {
                    childsList.Add(grandChildUIComponent);
                }
            }
            
            // 设置Childs字段值
            childsField.SetValue(parentComponent, childsList);
            
            if (childsList.Count > 0)
            {
                Debug.Log($"Bound {childsFieldName} with {childsList.Count} child UIComponents");
            }
        }
        
        public static void BindParentUIComponent(this UIComponent uiComponent)
        {
            var parent = uiComponent.transform.parent;
            while (parent)
            {
                if (parent.TryGetComponent<UIComponent>(out var uiComponentParent))
                {
                    uiComponent.Parent = uiComponentParent;
                    break;
                }
                parent = parent.parent;
            }
        }
        
        /// <summary>
        /// 设置UIComponent的parent字段
        /// </summary>
        /// <param name="childComponent">子UIComponent</param>
        /// <param name="parentComponent">父UIComponent</param>
        private static void SetParentField(UIComponent childComponent, UIComponent parentComponent)
        {
            if (childComponent == null || parentComponent == null) return;
            
            // 获取基类UIComponent中的parent字段
            FieldInfo parentField = typeof(UIComponent).GetField(UIComponentParnentFieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (parentField != null)
            {
                parentField.SetValue(childComponent, parentComponent);
                Debug.Log($"Set parent of {childComponent.name} to {parentComponent.name}");
                EditorUtility.SetDirty(childComponent);
            }
            else
            {
                Debug.LogWarning("Could not find parent field in UIComponent base class");
            }
        }

        
        /// <summary>
        /// 在子物体中查找指定类型的组件
        /// </summary>
        /// <param name="parent">父Transform</param>
        /// <param name="componentType">组件类型</param>
        /// <param name="fieldName">字段名</param>
        /// <returns>找到的组件，如果没找到返回null</returns>
        private static Component FindComponentInChildren(Transform parent, Type componentType, string fieldName)
        {
            Component[] components = parent.GetComponentsInChildren(componentType, true);
            
            if (components.Length == 1)
            {
                return components[0];
            }
            
            // 尝试通过名称匹配
            foreach (Component component in components)
            {
                string componentName = component.name.Replace(" ", "").Replace("(", "").Replace(")", "");
                string expectedName = char.ToLower(componentName[0]) + componentName.Substring(1) + componentType.Name;
                
                if (expectedName == fieldName)
                {
                    return component;
                }
            }
            
            // 尝试通过字段名包含组件名的方式匹配
            foreach (Component component in components)
            {
                if (component.name.IndexOf(fieldName.Replace(componentType.Name, ""), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return component;
                }
            }
            
            return components.Length > 0 ? components[0] : null;
        }
        
        /// <summary>
        /// 生成后的处理流程
        /// </summary>
        /// <param name="targetGo">目标GameObject</param>
        /// <param name="className">类名</param>
        private static void PostGenerateProcess(
            GameObject targetGo,
            string className,
            string namespaceOverride = null)
        {
            if (!targetGo) return;

            AttachScriptToGameObject(targetGo, className, namespaceOverride);

            if (targetGo.TryGetComponent(out UIComponent uiComponent))
                BindUIComponent(uiComponent);
        }

        private static void AttachScriptToGameObject(
            GameObject targetGo,
            string className,
            string namespaceOverride = null)
        {
            var existingUIComponent = targetGo.GetComponent<UIComponent>();
            if (existingUIComponent)
                return;

            Type scriptType = ResolveScriptType(className, namespaceOverride);
            if (scriptType == null)
            {
                Debug.LogError($"Could not find script type {className}. Make sure the script compiled successfully.");
                return;
            }
            
            // 验证类型是否继承自 Component
            if (!typeof(Component).IsAssignableFrom(scriptType))
            {
                Debug.LogError($"Type {className} does not inherit from UnityEngine.Component");
                return;
            }
            
            try
            {
                Component addedComponent = targetGo.AddComponent(scriptType);
                if (addedComponent != null)
                {
                    EditorUtility.SetDirty(targetGo);
                    
                    // 如果是场景对象，标记场景为脏
                    if (!EditorUtility.IsPersistent(targetGo))
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetGo.scene);
                    }
                }
                else
                {
                    Debug.LogError($"AddComponent returned null for {className}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to attach {className}: {e.Message}\n{e.StackTrace}");
            }
        }
        
        static Type ResolveScriptType(string className, string namespaceOverride)
        {
            if (!string.IsNullOrWhiteSpace(namespaceOverride))
            {
                var fullName = $"{namespaceOverride}.{className}";
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType(fullName);
                    if (type != null)
                        return type;
                }
            }

            return GetTypeByName(className);
        }

        private static Type GetTypeByName(string typeName)
        {
            // 获取所有可能的命名空间
            UIType[] allTypes = UITypeConfig.GetAllTypes();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 尝试在所有配置的命名空间中查找
                foreach (UIType uiType in allTypes)
                {
                    UITypeInfo info = UITypeConfig.GetInfo(uiType);
                    string fullTypeName = $"{info.Namespace}.{typeName}";
                    var type = assembly.GetType(fullTypeName);
                    if (type == null) continue;
                    return type;
                }
                
                // 如果没找到，尝试不带命名空间的查找
                var typeWithoutNamespace = assembly.GetType(typeName);
                if (typeWithoutNamespace != null)
                    return typeWithoutNamespace;
            }
            
            Debug.LogError($"Could not find type {typeName} in any configured namespace");
            return null;
        }

        /// <summary>
        /// 通过路径查找GameObject
        /// </summary>
        /// <param name="path">GameObject路径</param>
        /// <returns>找到的GameObject，如果没找到返回null</returns>
        private static GameObject FindGameObjectByPath(string path)
        {
            string[] parts = path.Split('/');
            GameObject current = GameObject.Find(parts[0]);
            
            for (int i = 1; i < parts.Length && current != null; i++)
            {
                Transform child = current.transform.Find(parts[i]);
                current = child != null ? child.gameObject : null;
            }
            
            return current;
        }
    }
}
