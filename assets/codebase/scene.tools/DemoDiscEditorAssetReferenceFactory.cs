using helengine.editor;

namespace city.scene.tools {
    /// <summary>
    /// Builds stable authored references through the public editor asset API.
    /// </summary>
    public static class DemoDiscEditorAssetReferenceFactory {
        /// <summary>
        /// Creates a canonical reference for one project-relative authored asset.
        /// </summary>
        /// <param name="relativePath">Path relative to the project assets directory.</param>
        /// <param name="expectedKind">Expected editor asset category.</param>
        /// <returns>Reference containing the asset id, path, and content hash.</returns>
        public static SceneAssetReference Create(string relativePath, AssetEntryKind expectedKind) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Asset path must be provided.", nameof(relativePath));
            }

            return EditorAssetReferenceFactory.CreateFileReference(
                EditorProjectPaths.ProjectRoot,
                relativePath,
                expectedKind);
        }

        /// <summary>
        /// Creates a canonical model reference.
        /// </summary>
        public static SceneAssetReference CreateModel(string relativePath) {
            return Create(relativePath, AssetEntryKind.Model);
        }

        /// <summary>
        /// Creates a canonical material reference.
        /// </summary>
        public static SceneAssetReference CreateMaterial(string relativePath) {
            return Create(relativePath, AssetEntryKind.Material);
        }

        /// <summary>
        /// Creates a canonical image reference.
        /// </summary>
        public static SceneAssetReference CreateImage(string relativePath) {
            return Create(relativePath, AssetEntryKind.Image);
        }

        /// <summary>
        /// Creates a canonical font reference.
        /// </summary>
        public static SceneAssetReference CreateFont(string relativePath) {
            return Create(relativePath, AssetEntryKind.Font);
        }

        /// <summary>
        /// Creates a canonical audio reference.
        /// </summary>
        public static SceneAssetReference CreateAudio(string relativePath) {
            return Create(relativePath, AssetEntryKind.Audio);
        }

        /// <summary>
        /// Creates a canonical scene reference.
        /// </summary>
        public static SceneAssetReference CreateScene(string relativePath) {
            return Create(relativePath, AssetEntryKind.Scene);
        }

        /// <summary>
        /// Creates a canonical blueprint reference.
        /// </summary>
        public static SceneAssetReference CreateBlueprint(string relativePath) {
            return Create(relativePath, AssetEntryKind.Blueprint);
        }

        /// <summary>
        /// Creates a canonical reference for an animation clip or other file-backed runtime asset.
        /// </summary>
        public static SceneAssetReference CreateFile(string relativePath) {
            return Create(relativePath, AssetEntryKind.File);
        }
    }
}
