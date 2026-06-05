using System;
using Air.UnityConnector.Cli;
using Air.UnityConnector.Invoke;
using Air.UI.Editor;

namespace Air.UI.Editor.Cli
{
    /// <summary>unity-cmd: generate UIPanel/UIComponent scripts from a prefab (AirUI generator).</summary>
    public sealed class UiGenerateScriptCommand : CliCommand<UiGenerateScriptParams>
    {
        public const string CommandName = "ui.generate";

        public override InvokeDescriptor Descriptor { get; } = new InvokeDescriptor<UiGenerateScriptParams>(
            CommandName,
            CommandHostScope.Editor,
            "Generate UI panel/component scripts from a prefab (logic + Designer, UISerializer bind)");

        public override void Run(UiGenerateScriptParams p)
        {
            try
            {
                var uiType = ParseUiType(p.UiType);
                var options = new UiGenerateScriptOptions
                {
                    NamespaceOverride = string.IsNullOrWhiteSpace(p.Namespace) ? null : p.Namespace.Trim(),
                    PreserveLogicScript = p.PreserveLogic ?? true,
                    PreserveLogicMarker = string.IsNullOrWhiteSpace(p.PreserveMarker) ? null : p.PreserveMarker,
                };

                var data = UiGenerateScriptService.GenerateFromPrefab(
                    p.PrefabPath,
                    p.OutputFolder,
                    p.ClassName,
                    uiType,
                    p.RootName,
                    p.StripPanel ?? true,
                    p.RefreshDesigner ?? true,
                    options);

                CompleteSuccess(InvokeResult.Ok("ui.generate completed", data));
            }
            catch (Exception ex)
            {
                CompleteFail(ex.Message);
            }
        }

        static UIType ParseUiType(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("panel", StringComparison.OrdinalIgnoreCase))
                return UIType.Panel;
            if (value.Equals("component", StringComparison.OrdinalIgnoreCase))
                return UIType.Component;
            throw new ArgumentException($"ui_type must be panel or component, got '{value}'.");
        }
    }
}
