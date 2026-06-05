using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Air.UI;
using UnityEditor;
using UnityEngine;

namespace Air.UI.Editor
{
    /// <summary>Loads a UI prefab and runs <see cref="UIScriptGenerator"/> (shared by Editor window and CLI).</summary>
    public static class UiGenerateScriptService
    {
        const string DefaultGeneratedNamespace = "Air.UI.Generated";
        static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

        public static Dictionary<string, object> GenerateFromPrefab(
            string prefabAssetPath,
            string outputFolder,
            string className,
            UIType uiType,
            string rootName,
            bool stripPanelComponents,
            bool deleteDesignerFirst,
            UiGenerateScriptOptions options)
        {
            if (string.IsNullOrWhiteSpace(prefabAssetPath))
                throw new ArgumentException("prefab_path is required.");
            if (string.IsNullOrWhiteSpace(outputFolder))
                throw new ArgumentException("output_folder is required.");

            prefabAssetPath = prefabAssetPath.Replace('\\', '/');
            if (!prefabAssetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("prefab_path must be a .prefab asset path.");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath) == null)
                throw new FileNotFoundException($"Prefab not found: {prefabAssetPath}");

            className = string.IsNullOrWhiteSpace(className) ? null : className.Trim();
            options ??= new UiGenerateScriptOptions();

            var root = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            try
            {
                if (!string.IsNullOrEmpty(rootName) && root.name != rootName)
                    root.name = rootName;

                className ??= root.name;

                if (stripPanelComponents)
                    StripPanelComponents(root);

                if (deleteDesignerFirst)
                    DeleteDesignerScript(outputFolder, className);

                var logicPath = Path.Combine(outputFolder, className + ".cs");
                var preservedLogic = ReadPreservedLogic(logicPath, options);

                options.PrefabPipelineAttach = true;
                UIScriptGenerator.GenerateUIScript(
                    root,
                    className,
                    outputFolder,
                    uiType,
                    options);

                if (!string.IsNullOrEmpty(preservedLogic))
                    File.WriteAllText(logicPath, preservedLogic, Utf8WithBom);

                if (!UISerializer.TryAttachAndBindPrefabRoot(
                        root, className, options.NamespaceOverride ?? DefaultGeneratedNamespace))
                {
                    UISerializer.AddPendingPrefabAttachment(
                        prefabAssetPath, className, options.NamespaceOverride ?? DefaultGeneratedNamespace);
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabAssetPath);

                return new Dictionary<string, object>
                {
                    ["prefab_path"] = prefabAssetPath,
                    ["class_name"] = className,
                    ["output_folder"] = outputFolder,
                    ["logic_script"] = logicPath.Replace('\\', '/'),
                    ["designer_script"] = Path.Combine(outputFolder, className + ".Designer.cs").Replace('\\', '/'),
                    ["ui_type"] = uiType.ToString(),
                    ["namespace"] = options.NamespaceOverride ?? DefaultGeneratedNamespace,
                    ["preserved_logic"] = !string.IsNullOrEmpty(preservedLogic),
                };
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static string ReadPreservedLogic(string logicPath, UiGenerateScriptOptions options)
        {
            if (!options.PreserveLogicScript || !File.Exists(logicPath))
                return null;

            var text = File.ReadAllText(logicPath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(options.PreserveLogicMarker))
                return LooksLikeGeneratedStub(text) ? null : text;

            return text.Contains(options.PreserveLogicMarker, StringComparison.Ordinal)
                ? text
                : null;
        }

        static bool LooksLikeGeneratedStub(string text) =>
            text.Contains("子类可以重写此方法", StringComparison.Ordinal)
            || text.Contains("automatically generated", StringComparison.OrdinalIgnoreCase);

        static void StripPanelComponents(GameObject root)
        {
            foreach (var mb in root.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                if (mb is UIComponent || mb.GetType().Name.EndsWith("Panel", StringComparison.Ordinal))
                    UnityEngine.Object.DestroyImmediate(mb);
            }
        }

        static void DeleteDesignerScript(string outputFolder, string className)
        {
            var designer = Path.Combine(outputFolder, className + ".Designer.cs");
            if (File.Exists(designer))
                File.Delete(designer);
            var meta = designer + ".meta";
            if (File.Exists(meta))
                File.Delete(meta);
        }
    }
}
