using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Regenerates the demo-disc startup scene as a temporary empty camera-only probe for PS2 leak isolation.
    /// </summary>
    public sealed class RegeneratePs2EmptyStartupProbeCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable command identifier used by headless editor-command invocation.
        /// </summary>
        public string CommandId => "menu.regenerate-ps2-empty-startup-probe";

        /// <summary>
        /// Gets the human-readable command label surfaced by editor command catalogs.
        /// </summary>
        public string DisplayName => "Regenerate PS2 Empty Startup Probe";

        /// <summary>
        /// Writes the temporary empty startup probe scene into the current city project.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            Ps2EmptyStartupProbeSceneFactory sceneFactory = new Ps2EmptyStartupProbeSceneFactory(context.Authoring);
            GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(context.Authoring);
            GeneratedAuthoringSceneDefinition sceneDefinition = sceneFactory.CreateSceneDefinition();
            sceneWriteService.WriteScene(context.ProjectRootPath, sceneDefinition);
        }
    }
}
