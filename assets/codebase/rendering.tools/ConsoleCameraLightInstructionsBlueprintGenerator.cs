using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Generates the shared console camera/light instruction Blueprint.
    /// </summary>
    public sealed class ConsoleCameraLightInstructionsBlueprintGenerator {
        /// <summary>
        /// Host-owned capability used to author the current native Blueprint.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Initializes one console instruction Blueprint generator.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to save the current Blueprint.</param>
        public ConsoleCameraLightInstructionsBlueprintGenerator(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        /// <summary>
        /// Generates and serializes the shared console instruction root.
        /// </summary>
        /// <param name="projectRootPath">Project root used by the overlay factory to resolve generated icon assets.</param>
        /// <param name="overlayFactory">Factory used to author the temporary Blueprint root.</param>
        /// <param name="font">Font used by the Blueprint's camera and light labels.</param>
        public void Generate(string projectRootPath, DemoSceneInstructionOverlayFactory overlayFactory, FontAsset font) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (overlayFactory == null) {
                throw new ArgumentNullException(nameof(overlayFactory));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (AssetAuthoringService.OwningCore == null) {
                throw new InvalidOperationException("Console camera/light Blueprint generation requires an active editor core.");
            }

            Entity root = overlayFactory.CreateConsoleCameraLightInstructionsRoot(projectRootPath, font);
            try {
                AssetAuthoringService.WriteNativeBlueprint(
                    ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath,
                    GeneratedScenePersistenceRegistryFactory.Create(),
                    city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(
                        ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath));
            } finally {
                root.Dispose();
            }
        }
    }
}
