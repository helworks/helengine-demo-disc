using helengine.editor;

namespace city.scene.tools {
    /// <summary>
    /// Writes generated scene assets into the active city project beneath the assets tree.
    /// </summary>
    public sealed class GeneratedSceneWriteService {
        /// <summary>
        /// Writes one generated scene asset to its project-relative scene id using atomic replacement.
        /// </summary>
        /// <param name="sceneId">Project-relative scene id, such as `scenes/rendering/directional_shadow_plaza.helen`.</param>
        /// <param name="sceneAsset">Fully-authored scene asset to serialize.</param>
        /// <param name="assetAuthoringService">Host-owned capability used to write the current native scene.</param>
        public void WriteScene(string sceneId, SceneAsset sceneAsset, IEditorProjectAssetAuthoringService assetAuthoringService) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (sceneAsset == null) {
                throw new ArgumentNullException(nameof(sceneAsset));
            } else if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            }

            assetAuthoringService.WriteNativeAsset(
                sceneId,
                sceneAsset,
                ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(sceneId));
        }
    }
}
