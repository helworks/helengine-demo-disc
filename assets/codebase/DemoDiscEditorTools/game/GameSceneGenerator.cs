using helengine.editor;

namespace DemoDisc.EditorTools {
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
        /// Writes the reusable gameplay assets every authored gameplay scene references: the shared course
        /// props, the player sphere material, and the two platform presentation Blueprints. The scene pass
        /// references these by path, so they are published before it runs.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void GenerateReusableAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            TiltTrialPendulumHammerAssetGenerator pendulumHammerAssetGenerator = new TiltTrialPendulumHammerAssetGenerator(AssetAuthoringService, Transaction);
            pendulumHammerAssetGenerator.Generate(projectRootPath);

            TiltTrialRotatingPlatformAssetGenerator rotatingPlatformAssetGenerator = new TiltTrialRotatingPlatformAssetGenerator(AssetAuthoringService, Transaction);
            rotatingPlatformAssetGenerator.Generate(projectRootPath);

            SplitPlayGoalFlagAssetGenerator splitPlayGoalFlagAssetGenerator = new SplitPlayGoalFlagAssetGenerator(AssetAuthoringService, Transaction);
            splitPlayGoalFlagAssetGenerator.Generate(projectRootPath);

            SplitPlayGoldenCoinAssetGenerator splitPlayGoldenCoinAssetGenerator = new SplitPlayGoldenCoinAssetGenerator(AssetAuthoringService, Transaction);
            splitPlayGoldenCoinAssetGenerator.Generate(projectRootPath);

            TiltTrialPlayerSphereMarbleMaterialFactory materialFactory = new TiltTrialPlayerSphereMarbleMaterialFactory(AssetAuthoringService, Transaction);
            materialFactory.WriteMaterialAsset(projectRootPath, AssetAuthoringService);

            GameSceneFactory factory = CreateSceneFactory(projectRootPath);
            TiltTrialGameplayPresentationBlueprintGenerator presentationBlueprintGenerator = new TiltTrialGameplayPresentationBlueprintGenerator(AssetAuthoringService, Transaction);
            presentationBlueprintGenerator.Generate(factory);
        }

        /// <summary>
        /// Writes the current authored city gameplay scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            GameSceneFactory factory = CreateSceneFactory(projectRootPath);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue, AssetAuthoringService, Transaction);
            TiltTrialGameplayPresentationRootFactory presentationRootFactory = new TiltTrialGameplayPresentationRootFactory(ScriptTypeResolverValue, AssetAuthoringService);
            GeneratedAuthoringSceneDefinition tiltTrialLevelSelectScene = factory.CreateTiltTrialScene();
            sceneWriteService.WriteScene(tiltTrialLevelSelectScene);
            IReadOnlyList<GeneratedAuthoringSceneDefinition> tiltTrialLevelScenes = factory.CreateTiltTrialLevelScenes();
            for (int index = 0; index < tiltTrialLevelScenes.Count; index++) {
                GeneratedAuthoringSceneDefinition levelScene = tiltTrialLevelScenes[index];
                levelScene.RootEntities = presentationRootFactory.AppendPresentationRoots(levelScene.RootEntities);
                sceneWriteService.WriteScene(levelScene);
            }
            TiltTrialHandheldLevelSelectSceneFactory handheldLevelSelectSceneFactory = new TiltTrialHandheldLevelSelectSceneFactory();
            GeneratedAuthoringSceneDefinition handheldLevelSelectScene = handheldLevelSelectSceneFactory.Create(factory);
            sceneWriteService.WriteScene(handheldLevelSelectScene);

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

            GameSceneFactory factory = CreateSceneFactory(projectRootPath);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue, AssetAuthoringService, Transaction);
            GeneratedAuthoringSceneDefinition tiltTrialScene = factory.CreateTiltTrialScene();
            sceneWriteService.WriteScene(tiltTrialScene);
            TiltTrialHandheldLevelSelectSceneFactory handheldLevelSelectSceneFactory = new TiltTrialHandheldLevelSelectSceneFactory();
            GeneratedAuthoringSceneDefinition handheldLevelSelectScene = handheldLevelSelectSceneFactory.Create(factory);
            sceneWriteService.WriteScene(handheldLevelSelectScene);
        }

        /// <summary>
        /// Builds one gameplay scene factory over the shared rendering assets of the active project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Gameplay scene factory bound to this generator's transaction.</returns>
        GameSceneFactory CreateSceneFactory(string projectRootPath) {
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(AssetAuthoringService, Transaction);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare();
            return new GameSceneFactory(assets, projectRootPath, AssetAuthoringService, Transaction);
        }
    }
}
