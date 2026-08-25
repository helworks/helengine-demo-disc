namespace city.game.tools {
    /// <summary>
    /// Writes one generated raw model asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedModelAssetWriteService {
        public void WriteModel(string projectRootPath, string relativePath, ModelAsset modelAsset) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative model path must be provided.", nameof(relativePath));
            } else if (modelAsset == null) {
                throw new ArgumentNullException(nameof(modelAsset));
            }

            string fullPath = Path.Combine(
                Path.GetFullPath(projectRootPath),
                "assets",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Model directory could not be resolved."));

            new global::helengine.editor.GeneratedAssetWriteService().WriteAsset(
                projectRootPath,
                relativePath,
                modelAsset);
        }
    }
}
