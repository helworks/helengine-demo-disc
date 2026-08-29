using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Generates the authored city gameplay scene set inside the active project.
    /// </summary>
    public sealed class GameSceneGenerator {
        /// <summary>
        /// Host-owned capability used to resolve imported assets and author current settings.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;
        /// <summary>
        /// Resolver used to restore project-authored components during temporary handheld clone loads.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolverValue;

        /// <summary>
        /// Initializes one gameplay scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to restore project-authored components during temporary handheld clone loads.</param>
        /// <param name="assetAuthoringService">Host-owned capability used by project generation services.</param>
        public GameSceneGenerator(
            IScriptTypeResolver scriptTypeResolver,
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            ScriptTypeResolverValue = scriptTypeResolver;
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Writes the current authored city gameplay scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            SplitPlayGoalFlagAssetGenerator splitPlayGoalFlagAssetGenerator = new SplitPlayGoalFlagAssetGenerator(AssetAuthoringService, Transaction);
            splitPlayGoalFlagAssetGenerator.Generate(projectRootPath);

            SplitPlayGoldenCoinAssetGenerator splitPlayGoldenCoinAssetGenerator = new SplitPlayGoldenCoinAssetGenerator(AssetAuthoringService, Transaction);
            splitPlayGoldenCoinAssetGenerator.Generate(projectRootPath);

            TiltTrialPlayerSphereMarbleMaterialFactory materialFactory = new TiltTrialPlayerSphereMarbleMaterialFactory(AssetAuthoringService, Transaction);
            materialFactory.WriteMaterialAsset(projectRootPath, AssetAuthoringService);
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(AssetAuthoringService, Transaction);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare();
            GameSceneFactory factory = new GameSceneFactory(assets, projectRootPath, AssetAuthoringService, Transaction);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue, AssetAuthoringService, Transaction);
            TiltTrialGameplayPresentationBlueprintGenerator presentationBlueprintGenerator = new TiltTrialGameplayPresentationBlueprintGenerator(AssetAuthoringService, Transaction);
            presentationBlueprintGenerator.Generate(factory);
            GeneratedAuthoringSceneDefinition tiltTrialLevelSelectScene = factory.CreateTiltTrialScene();
            sceneWriteService.WriteScene(tiltTrialLevelSelectScene);
            IReadOnlyList<GeneratedAuthoringSceneDefinition> tiltTrialLevelScenes = factory.CreateTiltTrialLevelScenes();
            for (int index = 0; index < tiltTrialLevelScenes.Count; index++) {
                sceneWriteService.WriteScene(tiltTrialLevelScenes[index]);
            }
            TiltTrialHandheldLevelSelectSceneFactory handheldLevelSelectSceneFactory = new TiltTrialHandheldLevelSelectSceneFactory();
            GeneratedAuthoringSceneDefinition handheldLevelSelectScene = handheldLevelSelectSceneFactory.Create(factory);
            sceneWriteService.WriteScene(handheldLevelSelectScene);

            GeneratedAuthoringSceneDefinition tiltTrialLevel01RenderTestScene = factory.CreateTiltTrialLevel01RenderTestScene();
            sceneWriteService.WriteScene(tiltTrialLevel01RenderTestScene);

            ZombislayerAssetPreparationService zombislayerAssetPreparationService = new ZombislayerAssetPreparationService(AssetAuthoringService);
            ZombislayerGenerationAssets zombislayerAssets = zombislayerAssetPreparationService.Prepare();
            ZombislayerSceneFactory zombislayerSceneFactory = new ZombislayerSceneFactory(zombislayerAssets, AssetAuthoringService);
            GeneratedAuthoringSceneDefinition zombislayerScene = zombislayerSceneFactory.CreateGameplayScene();
            sceneWriteService.WriteScene(zombislayerScene);
        }

        /// <summary>
        /// Regenerates the standard and handheld Tilt Trial selector scenes without rewriting gameplay levels or shared rendering assets.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void GenerateTiltTrialScene(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(AssetAuthoringService, Transaction);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare();
            GameSceneFactory factory = new GameSceneFactory(assets, projectRootPath, AssetAuthoringService, Transaction);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue, AssetAuthoringService, Transaction);
            GeneratedAuthoringSceneDefinition tiltTrialScene = factory.CreateTiltTrialScene();
            sceneWriteService.WriteScene(tiltTrialScene);
            TiltTrialHandheldLevelSelectSceneFactory handheldLevelSelectSceneFactory = new TiltTrialHandheldLevelSelectSceneFactory();
            GeneratedAuthoringSceneDefinition handheldLevelSelectScene = handheldLevelSelectSceneFactory.Create(factory);
            sceneWriteService.WriteScene(handheldLevelSelectScene);
        }
    }
}
