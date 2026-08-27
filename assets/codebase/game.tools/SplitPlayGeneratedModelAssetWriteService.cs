namespace city.game.tools {
    /// <summary>
    /// Writes one generated raw model asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedModelAssetWriteService {
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        public SplitPlayGeneratedModelAssetWriteService(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        public void WriteModel(string relativePath, ModelAsset modelAsset) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative model path must be provided.", nameof(relativePath));
            } else if (modelAsset == null) {
                throw new ArgumentNullException(nameof(modelAsset));
            }

            AssetAuthoringService.WriteNativeAsset(
                relativePath,
                modelAsset,
                city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath));
        }
    }
}
