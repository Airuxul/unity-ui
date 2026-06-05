namespace Air.UI.Editor
{
    /// <summary>Options for <see cref="UIScriptGenerator"/> / <c>ui.generate</c> CLI.</summary>
    public sealed class UiGenerateScriptOptions
    {
        /// <summary>Replaces template namespace (default <c>Air.UI.Generated</c>).</summary>
        public string NamespaceOverride { get; set; }

        /// <summary>When true, skip overwriting existing logic .cs if it looks like user code.</summary>
        public bool PreserveLogicScript { get; set; } = true;

        /// <summary>
        /// If set, preserve logic only when the file contains this substring (project-specific marker).
        /// </summary>
        public string PreserveLogicMarker { get; set; }

        /// <summary>Drop redundant Image / button caption Text fields from Designer output.</summary>
        public bool PruneButtonDecorations { get; set; } = true;

        /// <summary>
        /// When true (CLI prefab pipeline), defer hierarchy-based pending attach; caller binds on prefab root before save.
        /// </summary>
        public bool PrefabPipelineAttach { get; set; }
    }
}
