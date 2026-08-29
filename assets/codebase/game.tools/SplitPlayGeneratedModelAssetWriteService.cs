namespace city.game.tools {
    /// <summary>
    /// Writes one generated raw model asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedModelAssetWriteService {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        public SplitPlayGeneratedModelAssetWriteService(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public void WriteModel(string relativePath, ModelAsset modelAsset) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative model path must be provided.", nameof(relativePath));
            } else if (modelAsset == null) {
                throw new ArgumentNullException(nameof(modelAsset));
            }

            modelAsset.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath);
            modelAsset.FormerAuthoringAssetIds = Array.Empty<string>();
            Transaction.WriteAsset(relativePath, modelAsset);
        }
    }
}
