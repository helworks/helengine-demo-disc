namespace city.game.tools {
    /// <summary>
    /// Exposes explicit editor generation for the reusable Tilt Trial rotating platform assets.
    /// </summary>
    public sealed class GenerateTiltTrialRotatingPlatformCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-tilt-trial-rotating-platform";

        /// <summary>
        /// Gets the human-readable command label shown by editor command catalogs.
        /// </summary>
        public string DisplayName => "Generate Tilt Trial Rotating Platform";

        /// <summary>
        /// Generates the rotating platform model, material, and Blueprint for the active project.
        /// </summary>
        /// <param name="context">Editor command context supplied by the host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            TiltTrialRotatingPlatformAssetGenerator generator = new TiltTrialRotatingPlatformAssetGenerator();
            generator.Generate(context.ProjectRootPath);
        }
    }
}
