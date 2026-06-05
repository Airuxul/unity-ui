using Air.UnityConnector.Params;

namespace Air.UI.Editor.Cli
{
    public sealed class UiGenerateScriptParams
    {
        [CliParam("prefab_path", Description = "Prefab asset path, e.g. Assets/UI/MyPanel.prefab", Required = true,
            AlternateKeys = "prefab")]
        public string PrefabPath { get; set; }

        [CliParam("output_folder", Description = "Folder for .cs / .Designer.cs output", Required = true,
            AlternateKeys = "output")]
        public string OutputFolder { get; set; }

        [CliParam("class_name", Description = "C# class name (default: prefab root GameObject name)",
            AlternateKeys = "class")]
        public string ClassName { get; set; }

        [CliParam("ui_type", Description = "panel or component", AllowedValues = "panel|component")]
        public string UiType { get; set; } = "panel";

        [CliParam(Description = "Namespace for generated scripts (default Air.UI.Generated)")]
        public string Namespace { get; set; }

        [CliParam("root_name", Description = "Rename prefab root GameObject before generate")]
        public string RootName { get; set; }

        [CliParam("preserve_logic", Description = "Keep existing logic .cs when it contains preserve_marker or is not a stub")]
        public bool? PreserveLogic { get; set; }

        [CliParam("preserve_marker", Description = "Substring; when set, logic is kept only if file contains it")]
        public string PreserveMarker { get; set; }

        [CliParam("strip_panel", Description = "Remove UIPanel/UIComponent on prefab root before generate")]
        public bool? StripPanel { get; set; }

        [CliParam("refresh_designer", Description = "Delete .Designer.cs before regenerate")]
        public bool? RefreshDesigner { get; set; }
    }
}
