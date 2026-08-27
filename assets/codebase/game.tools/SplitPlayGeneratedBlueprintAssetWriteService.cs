namespace city.game.tools {
    /// <summary>
    /// Writes one generated blueprint asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedBlueprintAssetWriteService {
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        public SplitPlayGeneratedBlueprintAssetWriteService(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        public void WriteBlueprint(string relativePath, BlueprintAsset blueprintAsset) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative blueprint path must be provided.", nameof(relativePath));
            } else if (blueprintAsset == null) {
                throw new ArgumentNullException(nameof(blueprintAsset));
            }

            AssetAuthoringService.WriteNativeAsset(relativePath, blueprintAsset);
        }
    }
}
