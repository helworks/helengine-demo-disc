namespace city.game.tools {
    /// <summary>
    /// Attaches the generated Tilt Trial presentation Blueprints to existing authored gameplay level scenes.
    /// </summary>
    public sealed class AttachTiltTrialGameplayPresentationBlueprintsCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.attach-tilt-trial-presentation-blueprints";

        /// <summary>
        /// Gets the human-readable editor command label.
        /// </summary>
        public string DisplayName => "Attach Tilt Trial Presentation Blueprints";

        /// <summary>
        /// Updates the existing authored Tilt Trial level files without generating replacement gameplay scenes.
        /// </summary>
        /// <param name="context">Editor command context supplied by the host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            TiltTrialGameplayPresentationAttachmentService service = new TiltTrialGameplayPresentationAttachmentService(context.ScriptTypeResolver, context.AssetAuthoring);
            service.AttachToAuthoredGameplayScenes(context.ProjectRootPath);
        }
    }
}
