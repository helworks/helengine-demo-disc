using helengine.editor;
using DemoDisc.menu;

namespace DemoDisc.EditorTools {
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
        /// Publishes the shared handheld bottom-screen chrome Blueprint, then generates the current
        /// city rendering showcase scenes that instance it.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            using (EditorAuthoringTransaction chromeTransaction = context.Authoring.BeginTransaction()) {
                new HandheldBottomScreenChromeBlueprintGenerator(context.Authoring, chromeTransaction).Generate();
                chromeTransaction.Commit();
            }

            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(context.Authoring, transaction);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare();
            RenderingSceneGenerator generator = new RenderingSceneGenerator(context.ScriptTypeResolver, context.Authoring, transaction);
            generator.Generate(context.ProjectRootPath, assets);
            transaction.Commit();
        }
    }
}
