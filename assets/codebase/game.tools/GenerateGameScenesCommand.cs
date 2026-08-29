namespace city.game.tools {
    /// <summary>
    /// Regenerates the authored city gameplay scenes through the city-owned generated-scene pipeline.
    /// </summary>
    public sealed class GenerateGameScenesCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable command identifier used by headless and future in-editor command invocation paths.
        /// </summary>
        public string CommandId => "menu.generate-game-scenes";

        /// <summary>
        /// Gets the human-readable command label surfaced by the editor command catalog.
        /// </summary>
        public string DisplayName => "Generate Game Scenes";

        /// <summary>
        /// Rebuilds the authored city gameplay scenes using the current project gameplay definitions.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            GameSceneGenerator generator = new GameSceneGenerator(context.ScriptTypeResolver, context.Authoring);
            generator.Generate(context.ProjectRootPath);
        }
    }
}
