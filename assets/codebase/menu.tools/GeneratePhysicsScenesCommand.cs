using city.physics.tools;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Generates the authored physics showcase scene set inside the active project.
    /// </summary>
    public sealed class GeneratePhysicsScenesCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-physics-scenes";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Generate Physics Scenes";

        /// <summary>
        /// Generates the current authored physics showcase scenes.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            PhysicsSceneGenerator generator = new PhysicsSceneGenerator(context.Authoring, transaction);
            generator.Generate(context.ProjectRootPath);
            transaction.Commit();
        }
    }
}
