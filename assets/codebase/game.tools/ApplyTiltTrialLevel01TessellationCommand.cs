namespace city.game.tools {
    /// <summary>
    /// Applies constrained-platform MeshComponent tessellation metadata to the existing authored Tilt Trial Level 01 scene.
    /// </summary>
    public sealed class ApplyTiltTrialLevel01TessellationCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable command identifier used by headless and in-editor command invocation paths.
        /// </summary>
        public string CommandId => "menu.apply-tilt-trial-level-01-tessellation";

        /// <summary>
        /// Gets the label presented by the editor command catalog.
        /// </summary>
        public string DisplayName => "Apply Tilt Trial Level 01 Tessellation";

        /// <summary>
        /// Writes PS2- and PSP-only tessellation metadata to the selected scalable Level 01 course MeshComponents.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            using helengine.editor.EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            TiltTrialLevel01TessellationAuthoringService service = new TiltTrialLevel01TessellationAuthoringService(context.Authoring, transaction);
            service.ApplyToAuthoredLevel01Scene();
            transaction.Commit();
        }
    }
}
