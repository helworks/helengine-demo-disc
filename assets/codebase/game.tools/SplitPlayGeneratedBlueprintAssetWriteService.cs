namespace city.game.tools {
    /// <summary>
    /// Writes one generated blueprint asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedBlueprintAssetWriteService {
        public void WriteBlueprint(string projectRootPath, string relativePath, BlueprintAsset blueprintAsset) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative blueprint path must be provided.", nameof(relativePath));
            } else if (blueprintAsset == null) {
                throw new ArgumentNullException(nameof(blueprintAsset));
            }

            string fullPath = Path.Combine(
                Path.GetFullPath(projectRootPath),
                "assets",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Blueprint directory could not be resolved."));

            using FileStream stream = File.Create(fullPath);
            global::helengine.editor.AssetSerializer.Serialize(stream, blueprintAsset);
        }
    }
}
