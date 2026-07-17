using helengine.editor;
using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Generates the console and DS/3DS gameplay presentation Blueprints without rewriting authored level scenes.
    /// </summary>
    public sealed class TiltTrialGameplayPresentationBlueprintGenerator {
        /// <summary>
        /// Stable project-relative path for the console gameplay presentation Blueprint.
        /// </summary>
        public const string ConsoleBlueprintRelativePath = "blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint";

        /// <summary>
        /// Stable project-relative path for the DS/3DS gameplay presentation Blueprint.
        /// </summary>
        public const string HandheldBlueprintRelativePath = "blueprints/games/tilt/TiltTrialHandheldPresentation.hblueprint";

        /// <summary>
        /// Generates both platform presentation Blueprints beneath the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets folder.</param>
        /// <param name="sceneFactory">Factory used to author presentation roots.</param>
        public void Generate(string projectRootPath, GameSceneFactory sceneFactory) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneFactory == null) {
                throw new ArgumentNullException(nameof(sceneFactory));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("Tilt Trial presentation generation requires an active editor core.");
            }

            WriteBlueprint(projectRootPath, ConsoleBlueprintRelativePath, sceneFactory.CreateTiltTrialConsolePresentationRoot());
            WriteBlueprint(projectRootPath, HandheldBlueprintRelativePath, sceneFactory.CreateTiltTrialHandheldPresentationRoot());
        }

        /// <summary>
        /// Saves one generated presentation root through the editor Blueprint serializer and disposes the temporary authoring hierarchy.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets folder.</param>
        /// <param name="relativePath">Project-relative Blueprint output path.</param>
        /// <param name="root">Temporary presentation root to serialize.</param>
        void WriteBlueprint(string projectRootPath, string relativePath, EditorEntity root) {
            if (root == null) {
                throw new ArgumentNullException(nameof(root));
            }

            try {
                string fullPath = Path.Combine(Path.GetFullPath(projectRootPath), "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
                BlueprintSaveService saveService = new BlueprintSaveService(
                    projectRootPath,
                    GeneratedScenePersistenceRegistryFactory.Create());
                saveService.Save(fullPath);
            } finally {
                root.Dispose();
            }
        }
    }
}
