using helengine.editor;

namespace city.scene.tools {
    /// <summary>
    /// Builds stable authored references through the public editor asset API.
    /// </summary>
    public static class DemoDiscEditorAssetReferenceFactory {
        /// <summary>
        /// Creates a canonical reference for one project-relative authored asset.
        /// </summary>
        /// <param name="assetAuthoring">Host-owned public project authoring capability.</param>
        /// <param name="relativePath">Path relative to the project assets directory.</param>
        /// <param name="expectedKind">Expected editor asset category.</param>
        /// <returns>Reference containing the asset id, path, and content hash.</returns>
        public static SceneAssetReference Create(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath, AssetEntryKind expectedKind) {
            if (assetAuthoring == null) {
                throw new ArgumentNullException(nameof(assetAuthoring));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Asset path must be provided.", nameof(relativePath));
            }

            return assetAuthoring.CreateFileReference(relativePath, expectedKind);
        }

        /// <summary>
        /// Creates a canonical model reference.
        /// </summary>
        public static SceneAssetReference CreateModel(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Model);
        }

        /// <summary>
        /// Creates a canonical material reference.
        /// </summary>
        public static SceneAssetReference CreateMaterial(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Material);
        }

        /// <summary>
        /// Creates a canonical image reference.
        /// </summary>
        public static SceneAssetReference CreateImage(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Image);
        }

        /// <summary>
        /// Creates a canonical font reference.
        /// </summary>
        public static SceneAssetReference CreateFont(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Font);
        }

        /// <summary>
        /// Creates a canonical audio reference.
        /// </summary>
        public static SceneAssetReference CreateAudio(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Audio);
        }

        /// <summary>
        /// Creates a canonical scene reference.
        /// </summary>
        public static SceneAssetReference CreateScene(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Scene);
        }

        /// <summary>
        /// Creates a canonical blueprint reference.
        /// </summary>
        public static SceneAssetReference CreateBlueprint(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.Blueprint);
        }

        /// <summary>
        /// Creates a canonical reference for an animation clip or other file-backed runtime asset.
        /// </summary>
        public static SceneAssetReference CreateFile(IEditorProjectAssetAuthoringService assetAuthoring, string relativePath) {
            return Create(assetAuthoring, relativePath, AssetEntryKind.File);
        }
    }
}
