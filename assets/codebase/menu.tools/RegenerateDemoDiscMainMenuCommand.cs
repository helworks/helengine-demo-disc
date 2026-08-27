namespace city.menu.tools {
    /// <summary>
    /// Regenerates the authored demo-disc main menu scene through the city-owned live scene generator.
    /// </summary>
    public sealed class RegenerateDemoDiscMainMenuCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable command identifier used by headless and future in-editor command invocation paths.
        /// </summary>
        public string CommandId => "menu.regenerate-demo-disc-main-menu";

        /// <summary>
        /// Gets the human-readable command label surfaced by the editor command catalog.
        /// </summary>
        public string DisplayName => "Regenerate Demo Disc Main Menu";

        /// <summary>
        /// Rebuilds the demo-disc main menu scene using the current gameplay menu definition provider.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            new DemoDiscLogoIdleAnimationGenerator().Generate(context.ProjectRootPath);
            DemoDiscSceneGenerator generator = new DemoDiscSceneGenerator(context.ScriptTypeResolver, context.AssetAuthoring);
            generator.Generate(context.ProjectRootPath);
        }
    }
}
