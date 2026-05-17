using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Refreshes the generated textured cube-grid texture sources, import sidecars, cache entries, and material assets.
    /// </summary>
    public sealed class RefreshTexturedCubeGridAssetsCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.refresh-textured-cube-grid-assets";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Refresh Textured Cube Grid Assets";

        /// <summary>
        /// Regenerates the textured cube-grid authored texture and material asset set inside the active project.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            TexturedCubeGridSceneFactory factory = new TexturedCubeGridSceneFactory();
            factory.WriteAssets(context.ProjectRootPath);
        }
    }
}
