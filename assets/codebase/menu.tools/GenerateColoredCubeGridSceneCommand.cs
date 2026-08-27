using city.rendering.tools;

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

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(context.AssetAuthoring);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(context.ProjectRootPath);
            ColoredCubeGridSceneFactory factory = new ColoredCubeGridSceneFactory(context.AssetAuthoring);
            factory.WriteMaterialAssets(context.ProjectRootPath);
            GeneratedAuthoringSceneDefinition sceneDefinition = factory.CreateSceneDefinition(
                context.ProjectRootPath,
                assets.GeneratedCubeModel,
                factory.CreateRuntimeMaterials());
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(context.ScriptTypeResolver);
            sceneWriteService.WriteScene(context.ProjectRootPath, sceneDefinition);
        }
    }
}
