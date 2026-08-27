using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Regenerates only the authored cube-test rendering scene inside the active city project.
    /// </summary>
    public sealed class GenerateCubeTestSceneCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-cube-test-scene";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Generate Cube Test Scene";

        /// <summary>
        /// Regenerates only the authored cube-test scene.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(context.AssetAuthoring);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(context.ProjectRootPath);
            CubeTestSceneFactory factory = new CubeTestSceneFactory(context.AssetAuthoring);
            GeneratedAuthoringSceneDefinition sceneDefinition = factory.CreateSceneDefinition(
                context.ProjectRootPath,
                assets.GeneratedCubeModel,
                assets.GeneratedCubeTestSolidMaterial);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(context.ScriptTypeResolver);
            sceneWriteService.WriteScene(context.ProjectRootPath, sceneDefinition);
        }
    }
}
