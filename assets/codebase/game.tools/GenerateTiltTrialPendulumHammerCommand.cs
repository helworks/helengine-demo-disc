namespace city.game.tools {
    /// <summary>
    /// Exposes explicit editor generation for the reusable Tilt Trial pendulum hammer assets.
    /// </summary>
    public sealed class GenerateTiltTrialPendulumHammerCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-tilt-trial-pendulum-hammer";

        /// <summary>
        /// Gets the human-readable command label shown by editor command catalogs.
        /// </summary>
        public string DisplayName => "Generate Tilt Trial Pendulum Hammer";

        /// <summary>
        /// Generates the pendulum hammer models, materials, and Blueprint for the active project.
        /// </summary>
        /// <param name="context">Editor command context supplied by the host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            TiltTrialPendulumHammerAssetGenerator generator = new TiltTrialPendulumHammerAssetGenerator(context.Authoring);
            generator.Generate(context.ProjectRootPath);
        }
    }
}
