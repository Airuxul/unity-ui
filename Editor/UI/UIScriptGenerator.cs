using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Air.UI;
using Transform = UnityEngine.Transform;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI脚本生成器，负责根据GameObject结构自动生成UI脚本代码
    /// </summary>
    public static class UIScriptGenerator
    {
        static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

        static void WriteUtf8(string path, string content) =>
            File.WriteAllText(path, content, Utf8WithBom);

        static string ReadUtf8(string path) =>
            File.ReadAllText(path, Encoding.UTF8);

        /// <summary>
        /// 生成新的脚本（首次生成）
        /// </summary>
        public static void GenerateUIScript(
            GameObject targetGo,
            string className,
            string outputFolder,
            UIType uiType) =>
            GenerateUIScript(targetGo, className, outputFolder, uiType, null);

        public static void GenerateUIScript(
            GameObject targetGo,
            string className,
            string outputFolder,
            UIType uiType,
            UiGenerateScriptOptions options)
        {
            options ??= new UiGenerateScriptOptions();
            var typeInfo = UITypeConfig.GetInfo(uiType);
            var namespaceName = string.IsNullOrWhiteSpace(options.NamespaceOverride)
                ? typeInfo.Namespace
                : options.NamespaceOverride;

            var existingUIComponent = targetGo.GetComponent<UIComponent>();
            if (existingUIComponent)
            {
                var existingClassName = existingUIComponent.GetType().Name;
                if (existingClassName != className)
                {
                    throw new Exception($"Could not find existing Designer script for {existingClassName}");
                }
            }

            // 收集字段
            List<ComponentField> fields = CollectComponentFields(targetGo);

            // 生成逻辑脚本
            var logicScriptPath = Path.Combine(outputFolder, className + ".cs");
            if (!existingUIComponent)
            {
                var logicScript = GenerateLogicScript(className, fields, uiType, namespaceName);
                if (string.IsNullOrEmpty(logicScript))
                {
                    throw new Exception("logicScript is null or empty");
                }

                WriteUtf8(logicScriptPath, logicScript);
                Debug.Log($"Generated logic script: {logicScriptPath}");
            }

            // 生成设计器脚本
            string designerScript = GenerateDesignerScript(className, fields, namespaceName);
            string designerScriptPath = Path.Combine(outputFolder, className + ".Designer.cs");
            WriteUtf8(designerScriptPath, designerScript);
            if (options.PruneButtonDecorations)
                PruneDesignerScriptFile(designerScriptPath);
            Debug.Log($"Generated designer script: {designerScriptPath}");

            Debug.Log($"Successfully generated {typeInfo.DisplayName} scripts for {className} to {outputFolder}");

            if (!options.PrefabPipelineAttach)
                UISerializer.AddPendingAttachment(GetGameObjectPath(targetGo), className);

            AssetDatabase.ImportAsset(logicScriptPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(designerScriptPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 查找Designer脚本的路径
        /// </summary>
        /// <param name="className">类名</param>
        /// <returns>Designer脚本路径，如果没找到返回null</returns>
        public static string FindDesignerScriptPath(string className)
        {
            string designerFileName = className + ".Designer.cs";
            string[] guids = AssetDatabase.FindAssets($"{className} t:Script");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(designerFileName))
                {
                    return assetPath;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取GameObject的完整路径
        /// </summary>
        /// <param name="go">目标GameObject</param>
        /// <returns>完整路径字符串</returns>
        private static string GetGameObjectPath(GameObject go)
        {
            if (!go) return "";

            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent)
            {
                string parentName = parent.name;
                if (!string.IsNullOrEmpty(parentName))
                {
                    path = parentName + "/" + path;
                }

                parent = parent.parent;
            }

            return path;
        }

        /// <summary>
        /// 生成逻辑脚本内容
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="fields">组件字段列表</param>
        /// <param name="uiType">脚本类型</param>
        /// <returns>逻辑脚本内容</returns>
        private static string GenerateLogicScript(
            string className,
            List<ComponentField> fields,
            UIType uiType,
            string namespaceName)
        {
            var templatePath = GetLogicTemplatePath(uiType);
            if (!File.Exists(templatePath))
            {
                throw new Exception($"Logic template file not found: {templatePath}");
            }

            return GenerateScriptFromTemplate(templatePath, className, fields, namespaceName);
        }

        /// <summary>
        /// 生成设计器脚本内容
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="fields">组件字段列表</param>
        /// <returns>设计器脚本内容</returns>
        private static string GenerateDesignerScript(
            string className,
            List<ComponentField> fields,
            string namespaceName)
        {
            var templatePath = GetDesignerTemplatePath();
            if (!File.Exists(templatePath))
            {
                throw new Exception($"Designer template file not found: {templatePath}");
            }

            return GenerateScriptFromTemplate(templatePath, className, fields, namespaceName);
        }

        internal static void PruneDesignerScriptFile(string designerScriptPath)
        {
            if (!File.Exists(designerScriptPath))
                return;

            var lines = File.ReadAllLines(designerScriptPath, Encoding.UTF8);
            var kept = new List<string>(lines.Length);
            foreach (var line in lines)
            {
                if (IsPrunedDecoration(line))
                    continue;
                kept.Add(line);
            }

            File.WriteAllLines(designerScriptPath, kept, Utf8WithBom);
        }

        /// <summary>
        /// 根据模板生成脚本内容
        /// </summary>
        /// <param name="templatePath">模板文件路径</param>
        /// <param name="className">类名</param>
        /// <param name="fields">组件字段列表</param>
        /// <returns>生成的脚本内容</returns>
        private static string GenerateScriptFromTemplate(
            string templatePath,
            string className,
            List<ComponentField> fields,
            string namespaceName)
        {
            string template = ReadUtf8(templatePath);

            // 替换类名
            template = template.Replace("#CLASSNAME#", className);
            template = template.Replace("namespace Air.UI.Generated", $"namespace {namespaceName}");

            // 替换字段
            if (template.Contains("#FIELDS#"))
            {
                var fieldsBuilder = new StringBuilder();
                foreach (ComponentField field in fields)
                    fieldsBuilder.AppendLine($"        [SerializeField] protected {field.fieldType} {field.fieldName};");

                fieldsBuilder.AppendLine();
                fieldsBuilder.AppendLine("        public override void Bind()");
                fieldsBuilder.AppendLine("        {");
                foreach (ComponentField field in fields)
                {
                    if (!field.childComponent)
                        continue;
                    fieldsBuilder.AppendLine($"            AddComponent({field.fieldName});");
                }

                fieldsBuilder.Append("        }");
                template = template.Replace("#FIELDS#", fieldsBuilder.ToString());
            }

            return template;
        }

        /// <summary>
        /// 获取逻辑脚本模板文件路径
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <returns>逻辑脚本模板文件路径</returns>
        private static string GetLogicTemplatePath(UIType uiType)
        {
            var info = UITypeConfig.GetInfo(uiType);
            return FindTemplateFile(info.TemplateFileName);
        }

        /// <summary>
        /// 获取设计器脚本模板文件路径
        /// </summary>
        /// <returns>设计器脚本模板文件路径</returns>
        private static string GetDesignerTemplatePath()
        {
            return FindTemplateFile("UIDesignerTemplate.txt");
        }

        /// <summary>
        /// 查找模板文件的实际路径
        /// </summary>
        /// <param name="templateFileName">模板文件名</param>
        /// <returns>模板文件的完整路径</returns>
        private static string FindTemplateFile(string templateFileName)
        {
            // 使用AssetDatabase查找模板文件
            string[] guids =
                AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(templateFileName) + " t:TextAsset");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains("Templates") && assetPath.EndsWith(templateFileName))
                {
                    return Path.GetFullPath(assetPath);
                }
            }

            throw new FileNotFoundException(
                $"Could not find template file: {templateFileName}. Please ensure the template exists in the Templates folder.");
        }

        /// <summary>
        /// 将GameObject名称转换为字段名（首字母小写）
        /// </summary>
        /// <param name="name">GameObject名称</param>
        /// <returns>字段名</returns>
        private static string ToFieldName(string name) => name;

        private static List<ComponentField> CollectComponentFields(GameObject targetGo)
        {
            List<ComponentField> fields = new List<ComponentField>();
            CollectComponentFieldsRecursive(targetGo.transform, fields, "");
            ProcessFieldsSameName(fields);
            return fields;
        }

        /// <summary>
        /// 处理同名字段
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private static void ProcessFieldsSameName(List<ComponentField> fields)
        {
            var seen = new HashSet<string>();
            foreach (var field in fields)
            {
                if (!seen.Add(field.fieldName))
                    throw new Exception($"Duplicate UI bind name: {field.fieldName}");
            }
        }

        /// <summary>
        /// 递归收集组件字段信息
        /// </summary>
        private static void CollectComponentFieldsRecursive(Transform transform, List<ComponentField> fields,
            string path)
        {
            var isFirst = string.IsNullOrEmpty(path);
            var currentPath = isFirst ? transform.name : $"{path}/{transform.name}";
            if (!isFirst && TryCollectPrefixed(transform, fields))
                return;

            for (var i = 0; i < transform.childCount; i++)
                CollectComponentFieldsRecursive(transform.GetChild(i), fields, currentPath);
        }

        static bool TryCollectPrefixed(Transform transform, List<ComponentField> fields)
        {
            var name = transform.name;
            if (!UIBindPrefixes.TryMatch(name, out var kind))
                return false;

            if (kind == UIBindKind.GameObject)
            {
                fields.Add(new ComponentField(name, nameof(GameObject)));
                return FindBlock(transform) != null;
            }

            if (kind == UIBindKind.View)
            {
                var block = FindBlock(transform);
                if (block == null)
                {
                    Debug.LogWarning($"UI bind skipped, {name} has no UIComponent.");
                    return false;
                }

                fields.Add(new ComponentField(name, block.GetType().Name, true));
                return true;
            }

            var component = FindBindComponent(transform, kind);
            if (component == null)
            {
                Debug.LogWarning($"UI bind skipped, {name} is missing {kind}.");
                return false;
            }

            fields.Add(new ComponentField(name, component.GetType().Name));
            return false;
        }

        static bool IsPrunedDecoration(string line)
        {
            if (line.Contains("img_", StringComparison.Ordinal) || line.Contains("txt_", StringComparison.Ordinal))
                return false;
            if (line.Contains("protected Image ", StringComparison.Ordinal))
                return true;
            return line.Contains("caption", StringComparison.OrdinalIgnoreCase)
                && line.Contains("protected Text ", StringComparison.Ordinal);
        }

        // comp_ / go_ 自己带了界面组件时，子节点属于那一块，父级字段不能绑进去。
        internal static bool HidesDescendants(Transform transform)
        {
            if (!UIBindPrefixes.TryMatch(transform.name, out var kind))
                return false;
            if (kind != UIBindKind.View && kind != UIBindKind.GameObject)
                return false;
            return FindBlock(transform) != null;
        }

        static UIComponent FindBlock(Transform transform)
        {
            UIComponent found = null;
            foreach (var component in transform.GetComponents<UIComponent>())
            {
                var type = component.GetType();
                if (type == typeof(UIComponent) || type == typeof(UIView))
                    continue;
                if (found == null || type.IsSubclassOf(found.GetType()))
                    found = component;
            }

            return found;
        }

        static Component FindBindComponent(Transform transform, UIBindKind kind)
        {
            var expected = UIBindPrefixes.ComponentType(kind);
            if (expected != null)
            {
                var component = transform.GetComponent(expected);
                if (component != null)
                    return component;
            }

            if (kind == UIBindKind.Text)
                return transform.GetComponent<TMP_Text>();
            if (kind == UIBindKind.Input)
                return transform.GetComponent<TMP_InputField>();
            if (kind == UIBindKind.Dropdown)
                return transform.GetComponent<TMP_Dropdown>();
            return null;
        }
    }

    public enum UIBindKind
    {
        Text,
        TextMesh,
        Image,
        Button,
        Toggle,
        Slider,
        Input,
        Dropdown,
        Scroll,
        Scrollbar,
        CanvasGroup,
        GameObject,
        View
    }

    public static class UIBindPrefixes
    {
        public static bool TryMatch(string nodeName, out UIBindKind kind)
        {
            kind = default;
            if (string.IsNullOrEmpty(nodeName))
                return false;
            if (nodeName.StartsWith("txt_", StringComparison.Ordinal)) { kind = UIBindKind.Text; return true; }
            if (nodeName.StartsWith("tmp_", StringComparison.Ordinal)) { kind = UIBindKind.TextMesh; return true; }
            if (nodeName.StartsWith("img_", StringComparison.Ordinal)) { kind = UIBindKind.Image; return true; }
            if (nodeName.StartsWith("btn_", StringComparison.Ordinal)) { kind = UIBindKind.Button; return true; }
            if (nodeName.StartsWith("tog_", StringComparison.Ordinal)) { kind = UIBindKind.Toggle; return true; }
            if (nodeName.StartsWith("sld_", StringComparison.Ordinal)) { kind = UIBindKind.Slider; return true; }
            if (nodeName.StartsWith("ipt_", StringComparison.Ordinal)) { kind = UIBindKind.Input; return true; }
            if (nodeName.StartsWith("drop_", StringComparison.Ordinal)) { kind = UIBindKind.Dropdown; return true; }
            if (nodeName.StartsWith("scr_", StringComparison.Ordinal)) { kind = UIBindKind.Scroll; return true; }
            if (nodeName.StartsWith("bar_", StringComparison.Ordinal)) { kind = UIBindKind.Scrollbar; return true; }
            if (nodeName.StartsWith("grp_", StringComparison.Ordinal)) { kind = UIBindKind.CanvasGroup; return true; }
            if (nodeName.StartsWith("go_", StringComparison.Ordinal)) { kind = UIBindKind.GameObject; return true; }
            if (nodeName.StartsWith("comp_", StringComparison.Ordinal)) { kind = UIBindKind.View; return true; }
            return false;
        }

        public static Type ComponentType(UIBindKind kind) => kind switch
        {
            UIBindKind.Text => typeof(Text),
            UIBindKind.TextMesh => typeof(TextMeshProUGUI),
            UIBindKind.Image => typeof(Image),
            UIBindKind.Button => typeof(Button),
            UIBindKind.Toggle => typeof(Toggle),
            UIBindKind.Slider => typeof(Slider),
            UIBindKind.Input => typeof(InputField),
            UIBindKind.Dropdown => typeof(Dropdown),
            UIBindKind.Scroll => typeof(ScrollRect),
            UIBindKind.Scrollbar => typeof(Scrollbar),
            UIBindKind.CanvasGroup => typeof(CanvasGroup),
            _ => null
        };
    }

    /// <summary>
    /// 组件字段信息
    /// </summary>
    [Serializable]
    public class ComponentField
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string fieldName;

        /// <summary>
        /// 字段类型
        /// </summary>
        public string fieldType;

        public bool childComponent;

        public ComponentField(string fieldName, string fieldType, bool childComponent = false)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.childComponent = childComponent;
        }
    }
}
