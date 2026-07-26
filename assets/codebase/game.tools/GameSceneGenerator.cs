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

            SplitPlayGoalFlagAssetGenerator splitPlayGoalFlagAssetGenerator = new SplitPlayGoalFlagAssetGenerator();
            splitPlayGoalFlagAssetGenerator.Generate(projectRootPath);

            SplitPlayGoldenCoinAssetGenerator splitPlayGoldenCoinAssetGenerator = new SplitPlayGoldenCoinAssetGenerator();
            splitPlayGoldenCoinAssetGenerator.Generate(projectRootPath);

            TiltTrialPlayerSphereMarbleMaterialFactory materialFactory = new TiltTrialPlayerSphereMarbleMaterialFactory();
            materialFactory.WriteMaterialAsset(projectRootPath);
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService();
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(projectRootPath);
            GameSceneFactory factory = new GameSceneFactory(assets);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue);
            TiltTrialGameplayPresentationBlueprintGenerator presentationBlueprintGenerator = new TiltTrialGameplayPresentationBlueprintGenerator();
            presentationBlueprintGenerator.Generate(projectRootPath, factory);
            GeneratedAuthoringSceneDefinition tiltTrialLevelSelectScene = factory.CreateTiltTrialScene();
            sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);
            TiltTrialHandheldLevelSelectSceneFactory handheldLevelSelectSceneFactory = new TiltTrialHandheldLevelSelectSceneFactory();
            GeneratedAuthoringSceneDefinition handheldLevelSelectScene = handheldLevelSelectSceneFactory.Create(factory);
            sceneWriteService.WriteScene(projectRootPath, handheldLevelSelectScene);

            GeneratedAuthoringSceneDefinition tiltTrialLevel01RenderTestScene = factory.CreateTiltTrialLevel01RenderTestScene();
            sceneWriteService.WriteScene(projectRootPath, tiltTrialLevel01RenderTestScene);

            ZombislayerAssetPreparationService zombislayerAssetPreparationService = new ZombislayerAssetPreparationService();
            ZombislayerGenerationAssets zombislayerAssets = zombislayerAssetPreparationService.Prepare(projectRootPath);
            ZombislayerSceneFactory zombislayerSceneFactory = new ZombislayerSceneFactory(zombislayerAssets);
            GeneratedAuthoringSceneDefinition zombislayerScene = zombislayerSceneFactory.CreateGameplayScene();
            sceneWriteService.WriteScene(projectRootPath, zombislayerScene);
        }

        /// <summary>
        /// Regenerates only the Tilt Trial front-door scene without rewriting the other generated game scenes or shared rendering assets.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void GenerateTiltTrialScene(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService();
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(projectRootPath);
            GameSceneFactory factory = new GameSceneFactory(assets);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue);
            GeneratedAuthoringSceneDefinition tiltTrialScene = factory.CreateTiltTrialScene();
            sceneWriteService.WriteScene(projectRootPath, tiltTrialScene);
        }
    }
}
