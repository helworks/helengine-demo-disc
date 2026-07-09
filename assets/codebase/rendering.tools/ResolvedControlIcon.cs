namespace city.rendering.tools {
    /// <summary>
    /// Describes one generated control icon resolved from the manifest and editor import pipeline.
    /// </summary>
    public sealed class ResolvedControlIcon {
        public string PlatformId { get; init; } = string.Empty;
        public string FamilyId { get; init; } = string.Empty;
        public string ControlId { get; init; } = string.Empty;
        public string SourcePngRelativePath { get; init; } = string.Empty;
        public string ImportedTextureAssetId { get; init; } = string.Empty;
    }
}
