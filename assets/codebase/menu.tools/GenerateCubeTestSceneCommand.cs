using city.rendering.tools;
using helengine.editor;

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

            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(context.Authoring, transaction);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare();
            CubeTestSceneFactory factory = new CubeTestSceneFactory(context.Authoring, transaction);
            GeneratedAuthoringSceneDefinition sceneDefinition = factory.CreateSceneDefinition(
                context.ProjectRootPath,
                assets.GeneratedCubeModel,
                assets.GeneratedCubeTestSolidMaterial);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(context.ScriptTypeResolver, context.Authoring, transaction);
            sceneWriteService.WriteScene(sceneDefinition);
            transaction.Commit();
        }
    }
}
