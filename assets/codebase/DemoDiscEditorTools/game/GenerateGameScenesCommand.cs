
namespace DemoDisc.EditorTools {
    /// <summary>
    /// Regenerates every authored city gameplay asset and scene through the city-owned generated-scene pipeline.
    /// </summary>
    public sealed class GenerateGameScenesCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable command identifier used by headless and in-editor command invocation paths.
        /// </summary>
        public string CommandId => "menu.generate-game-scenes";

        /// <summary>
        /// Gets the human-readable command label surfaced by the editor command catalog.
        /// </summary>
        public string DisplayName => "Generate Game Scenes";

        /// <summary>
        /// Rebuilds the authored city gameplay content in two publications: the reusable props, materials and
        /// presentation Blueprints first, then the scenes that reference them. The scenes bind to the published
        /// Blueprints, so the two passes cannot share one publication.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            using (helengine.editor.EditorAuthoringTransaction assetTransaction = context.Authoring.BeginTransaction()) {
                GameSceneGenerator assetGenerator = new GameSceneGenerator(context.ScriptTypeResolver, context.Authoring, assetTransaction);
                assetGenerator.GenerateReusableAssets(context.ProjectRootPath);
                assetTransaction.Commit();
            }

            using (helengine.editor.EditorAuthoringTransaction sceneTransaction = context.Authoring.BeginTransaction()) {
                GameSceneGenerator sceneGenerator = new GameSceneGenerator(context.ScriptTypeResolver, context.Authoring, sceneTransaction);
                sceneGenerator.Generate(context.ProjectRootPath);
                sceneTransaction.Commit();
            }
        }
    }
}
