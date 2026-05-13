using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Generates the city rendering showcase scene set inside the active project.
    /// </summary>
    public sealed class GenerateRenderingScenesCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-rendering-scenes";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Generate Rendering Scenes";

        /// <summary>
        /// Generates the current city rendering showcase scenes.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService();
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(context.ProjectRootPath);
            RenderingSceneGenerator generator = new RenderingSceneGenerator();
            generator.Generate(context.ProjectRootPath, assets);
        }
    }
}
