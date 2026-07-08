using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Generates the authored city gameplay scene set inside the active project.
    /// </summary>
    public sealed class GameSceneGenerator {
        /// <summary>
        /// Resolver used to restore project-authored components during temporary handheld clone loads.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolverValue;

        /// <summary>
        /// Initializes one gameplay scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to restore project-authored components during temporary handheld clone loads.</param>
        public GameSceneGenerator(IScriptTypeResolver scriptTypeResolver = null) {
            ScriptTypeResolverValue = scriptTypeResolver;
        }

        /// <summary>
        /// Writes the current authored city gameplay scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            TiltTrialPlayerSphereMarbleMaterialFactory materialFactory = new TiltTrialPlayerSphereMarbleMaterialFactory();
            materialFactory.WriteMaterialAsset(projectRootPath);
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService();
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(projectRootPath);
            GameSceneFactory factory = new GameSceneFactory(assets);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue);
            GeneratedAuthoringSceneDefinition tiltTrialLevelSelectScene = factory.CreateTiltTrialLevelSelectScene();
            sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);

            IReadOnlyList<GeneratedAuthoringSceneDefinition> tiltTrialLevelScenes = factory.CreateTiltTrialLevelScenes();
            for (int index = 0; index < tiltTrialLevelScenes.Count; index++) {
                sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelScenes[index]);
            }
        }
    }
}
