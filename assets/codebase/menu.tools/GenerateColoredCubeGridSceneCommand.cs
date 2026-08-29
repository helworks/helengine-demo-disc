using city.rendering.tools;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Regenerates only the authored colored cube-grid rendering scene inside the active city project.
    /// </summary>
    public sealed class GenerateColoredCubeGridSceneCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-colored-cube-grid-scene";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Generate Colored Cube Grid Scene";

        /// <summary>
        /// Regenerates only the authored colored cube-grid scene and its file-backed material assets.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(context.Authoring, transaction);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare();
            ColoredCubeGridSceneFactory factory = new ColoredCubeGridSceneFactory(context.Authoring, transaction);
            factory.WriteMaterialAssets(context.ProjectRootPath);
            GeneratedAuthoringSceneDefinition sceneDefinition = factory.CreateSceneDefinition(
                context.ProjectRootPath,
                assets.GeneratedCubeModel,
                factory.CreateRuntimeMaterials());
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(context.ScriptTypeResolver, context.Authoring, transaction);
            sceneWriteService.WriteScene(sceneDefinition);
            transaction.Commit();
        }
    }
}
