using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Migrates the shared physics demo materials into the per-platform material settings flow required by Nintendo DS cooking.
    /// </summary>
    public sealed class MigratePhysicsDemoMaterialsCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.migrate-physics-demo-materials";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Migrate Physics Demo Materials";

        /// <summary>
        /// Rewrites the shared physics demo materials through the per-platform material settings service.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            PhysicsDemoMaterialMigrationService migrationService = new PhysicsDemoMaterialMigrationService();
            migrationService.Migrate(context.ProjectRootPath);
        }
    }
}
