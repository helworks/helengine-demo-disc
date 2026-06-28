using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Generates the authored city gameplay scene set inside the active project.
    /// </summary>
    public sealed class GameSceneGenerator {
        /// <summary>
        /// Writes the current authored city gameplay scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService();
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(projectRootPath);
            GameSceneFactory factory = new GameSceneFactory(assets);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService();
            sceneWriteService.WriteScene(projectRootPath, factory.CreateTiltTrialScene());
        }
    }
}
