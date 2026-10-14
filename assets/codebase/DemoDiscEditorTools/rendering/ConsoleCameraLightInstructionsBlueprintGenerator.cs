using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Generates the shared console camera/light instruction Blueprint.
    /// </summary>
    public sealed class ConsoleCameraLightInstructionsBlueprintGenerator {
        /// <summary>
        /// Host-owned capability used to author the current native Blueprint.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Name of the instruction panel entity beneath the Blueprint's viewport root.
        /// </summary>
        const string InstructionPanelEntityName = "ConsoleCameraLightInstructionsPanel";

        /// <summary>
        /// Initializes one console instruction Blueprint generator.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to save the current Blueprint.</param>
        public ConsoleCameraLightInstructionsBlueprintGenerator(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
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
                ConfigureAndroidPresentation(root);
                AssetAuthoringService.WriteNativeBlueprint(
                    ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath,
                    GeneratedScenePersistenceRegistryFactory.Create(),
                    DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(
                        ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath),
                    Transaction);
            } finally {
                root.Dispose();
            }
        }

        /// <summary>
        /// Authors the Android landscape presentation of the shared instructions: density-scaled viewport, scalable
        /// glyphs, and the panel pinned to the bottom-left screen edges it sits against on the reference canvas.
        /// </summary>
        /// <param name="root">Generated Blueprint root whose viewport holds the instruction panel.</param>
        static void ConfigureAndroidPresentation(Entity root) {
            AndroidDensityPresentationAuthoringService android = new AndroidDensityPresentationAuthoringService();
            android.ConfigureDensitySubtree(root);
            for (int index = 0; index < root.Children.Count; index++) {
                if (root.Children[index] is EditorEntity panel && panel.Name == InstructionPanelEntityName) {
                    android.AnchorToLeftBottomEdges(panel);
                    return;
                }
            }

            throw new InvalidOperationException("The console instruction Blueprint is missing " + InstructionPanelEntityName + ".");
        }
    }
}
