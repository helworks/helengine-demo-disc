namespace city.game.tools {
    /// <summary>
    /// Writes one generated raw model asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedModelAssetWriteService {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        public SplitPlayGeneratedModelAssetWriteService(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        public void WriteModel(string relativePath, ModelAsset modelAsset) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative model path must be provided.", nameof(relativePath));
            } else if (modelAsset == null) {
                throw new ArgumentNullException(nameof(modelAsset));
            }

            modelAsset.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath);
            modelAsset.FormerAuthoringAssetIds = Array.Empty<string>();
            // The session owns the transaction lifetime.  Calling its public
            // write boundary keeps detached test fixtures and host sessions
            // on the same path without constructing an editor transaction in
            // project code.
            AssetAuthoringService.WriteAsset(relativePath, modelAsset);
        }
    }
}
