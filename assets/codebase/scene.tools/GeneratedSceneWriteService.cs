namespace city.scene.tools {
    /// <summary>
    /// Writes generated scene assets into the active city project beneath the assets tree.
    /// </summary>
    public sealed class GeneratedSceneWriteService {
        /// <summary>
        /// Writes one generated scene asset to its project-relative scene id using atomic replacement.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneId">Project-relative scene id, such as `scenes/rendering/directional_shadow_plaza.helen`.</param>
        /// <param name="sceneAsset">Fully-authored scene asset to serialize.</param>
        public void WriteScene(string projectRootPath, string sceneId, SceneAsset sceneAsset) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (sceneAsset == null) {
                throw new ArgumentNullException(nameof(sceneAsset));
            }

            string assetsRootPath = Path.GetFullPath(Path.Combine(projectRootPath, "assets"));
            if (!Directory.Exists(assetsRootPath)) {
                throw new InvalidOperationException($"Assets root was not found: {assetsRootPath}");
            }

            string relativeScenePath = sceneId.Replace('/', Path.DirectorySeparatorChar);
            string scenePath = Path.GetFullPath(Path.Combine(assetsRootPath, relativeScenePath));
            if (!IsInsideAssetsRoot(assetsRootPath, scenePath)) {
                throw new InvalidOperationException("Generated scenes must be written beneath the project assets folder.");
            }

            string directoryPath = Path.GetDirectoryName(scenePath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException("Generated scene path must contain a writable directory.");
            }

            Directory.CreateDirectory(directoryPath);
            string temporaryScenePath = scenePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try {
                using (FileStream stream = new FileStream(temporaryScenePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    helengine.editor.AssetSerializer.Serialize(stream, sceneAsset);
                }

                File.Move(temporaryScenePath, scenePath, true);
            } catch {
                if (File.Exists(temporaryScenePath)) {
                    File.Delete(temporaryScenePath);
                }

                throw;
            }
        }

        /// <summary>
        /// Determines whether one absolute path is stored inside the supplied assets root.
        /// </summary>
        /// <param name="assetsRootPath">Absolute assets root path.</param>
        /// <param name="scenePath">Absolute scene path to validate.</param>
        /// <returns>True when the path is inside the assets root.</returns>
        bool IsInsideAssetsRoot(string assetsRootPath, string scenePath) {
            if (string.IsNullOrWhiteSpace(assetsRootPath)) {
                throw new ArgumentException("Assets root path must be provided.", nameof(assetsRootPath));
            } else if (string.IsNullOrWhiteSpace(scenePath)) {
                throw new ArgumentException("Scene path must be provided.", nameof(scenePath));
            }

            string normalizedAssetsRootPath = assetsRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? assetsRootPath
                : assetsRootPath + Path.DirectorySeparatorChar;
            return scenePath.StartsWith(normalizedAssetsRootPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
