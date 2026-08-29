namespace city.game.tools {
    /// <summary>
    /// Writes one generated blueprint asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedBlueprintAssetWriteService {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        public SplitPlayGeneratedBlueprintAssetWriteService(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        public void WriteBlueprint(string relativePath, BlueprintAsset blueprintAsset) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative blueprint path must be provided.", nameof(relativePath));
            } else if (blueprintAsset == null) {
                throw new ArgumentNullException(nameof(blueprintAsset));
            }

            blueprintAsset.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath);
            blueprintAsset.FormerAuthoringAssetIds = Array.Empty<string>();
            AssetAuthoringService.WriteAsset(relativePath, blueprintAsset);
        }
    }
}
