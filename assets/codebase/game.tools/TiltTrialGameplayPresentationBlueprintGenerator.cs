using helengine.editor;
using city.rendering.tools;

namespace city.game.tools {
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

            WriteBlueprint(ConsoleBlueprintRelativePath, sceneFactory.CreateTiltTrialConsolePresentationRoot());
            WriteBlueprint(HandheldBlueprintRelativePath, sceneFactory.CreateTiltTrialHandheldPresentationRoot());
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
                    city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath),
                    Transaction);
            } finally {
                root.Dispose();
            }
        }
    }
}
