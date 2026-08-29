using city.physics.tools;

namespace city.menu.tools {
    /// <summary>
    /// Generates Nintendo DS companion scenes for the curated authored physics showcase scenes.
    /// </summary>
    public sealed class GeneratePhysicsNintendoDsScenesCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-physics-nintendo-ds-scenes";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Generate Physics DS Scenes";

        /// <summary>
        /// Generates the Nintendo DS companion scenes for the curated authored physics showcase scenes.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            PhysicsNintendoDsSceneGenerator generator = new PhysicsNintendoDsSceneGenerator(context.ScriptTypeResolver, context.Authoring);
            generator.Generate(context.ProjectRootPath);
        }
    }
}
