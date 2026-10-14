using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Generates the console and DS/3DS gameplay presentation Blueprints without rewriting authored level scenes.
    /// </summary>
    public sealed class TiltTrialGameplayPresentationBlueprintGenerator {
        /// <summary>
        /// Host-owned capability used to author the current native Blueprints.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Stable project-relative path for the console gameplay presentation Blueprint.
        /// </summary>
        public const string ConsoleBlueprintRelativePath = "blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint";

        /// <summary>
        /// Stable project-relative path for the DS/3DS gameplay presentation Blueprint.
        /// </summary>
        public const string HandheldBlueprintRelativePath = "blueprints/games/tilt/TiltTrialHandheldPresentation.hblueprint";

        /// <summary>
        /// Initializes one gameplay presentation Blueprint generator.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to save current Blueprints.</param>
        public TiltTrialGameplayPresentationBlueprintGenerator(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Generates both platform presentation Blueprints beneath the supplied project.
        /// </summary>
        /// <param name="sceneFactory">Factory used to author presentation roots.</param>
        public void Generate(GameSceneFactory sceneFactory) {
            if (sceneFactory == null) {
                throw new ArgumentNullException(nameof(sceneFactory));
            } else if (AssetAuthoringService.OwningCore == null) {
                throw new InvalidOperationException("Tilt Trial presentation generation requires an active editor core.");
            }

            EditorEntity consoleRoot = sceneFactory.CreateTiltTrialConsolePresentationRoot();
            ExcludeCamerasFromMobile(consoleRoot);
            WriteBlueprint(ConsoleBlueprintRelativePath, consoleRoot);
            WriteBlueprint(HandheldBlueprintRelativePath, sceneFactory.CreateTiltTrialHandheldPresentationRoot());
        }

        /// <summary>
        /// Removes the console presentation's own cameras on mobile, where the level scene's camera, driven by the
        /// stage controller, presents the course and the Blueprint contributes only its HUD and lighting rig.
        /// </summary>
        /// <param name="entity">Console presentation subtree to search for camera entities.</param>
        static void ExcludeCamerasFromMobile(Entity entity) {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is CameraComponent) {
                    new MobilePresentationAuthoringService().Exclude(entity);
                    break;
                }
            }

            for (int index = 0; index < entity.Children.Count; index++) {
                ExcludeCamerasFromMobile(entity.Children[index]);
            }
        }

        /// <summary>
        /// Saves one generated presentation root through the editor Blueprint serializer and disposes the temporary authoring hierarchy.
        /// </summary>
        /// <param name="relativePath">Project-relative Blueprint output path.</param>
        /// <param name="root">Temporary presentation root to serialize.</param>
        void WriteBlueprint(string relativePath, EditorEntity root) {
            if (root == null) {
                throw new ArgumentNullException(nameof(root));
            }

            try {
                AssetAuthoringService.WriteNativeBlueprint(
                    relativePath,
                    GeneratedScenePersistenceRegistryFactory.Create(),
                    DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath),
                    Transaction);
            } finally {
                root.Dispose();
            }
        }
    }
}
