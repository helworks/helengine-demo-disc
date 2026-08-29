namespace city.game.tools {
    /// <summary>
    /// Regenerates only the Tilt Trial front-door scene through the city-owned authoring pipeline.
    /// </summary>
    public sealed class GenerateTiltTrialSceneCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier for targeted Tilt Trial scene regeneration.
        /// </summary>
        public string CommandId => "menu.generate-tilt-trial-scene";

        /// <summary>
        /// Gets the human-readable command label surfaced by the editor command catalog.
        /// </summary>
        public string DisplayName => "Generate Tilt Trial Scene";

        /// <summary>
        /// Rebuilds only the authored Tilt Trial front-door scene in the active project.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            GameSceneGenerator generator = new GameSceneGenerator(context.ScriptTypeResolver, context.Authoring);
            generator.GenerateTiltTrialScene(context.ProjectRootPath);
        }
    }
}
