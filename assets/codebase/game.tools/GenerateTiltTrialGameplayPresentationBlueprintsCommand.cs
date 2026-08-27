using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Exposes explicit editor generation for the two cook-time Tilt Trial gameplay presentation Blueprints.
    /// </summary>
    public sealed class GenerateTiltTrialGameplayPresentationBlueprintsCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-tilt-trial-presentation-blueprints";

        /// <summary>
        /// Gets the human-readable command label shown by editor command catalogs.
        /// </summary>
        public string DisplayName => "Generate Tilt Trial Presentation Blueprints";

        /// <summary>
        /// Generates the console and handheld presentation Blueprints for the active project.
        /// </summary>
        /// <param name="context">Editor command context supplied by the host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService(context.AssetAuthoring);
            RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(context.ProjectRootPath);
            GameSceneFactory sceneFactory = new GameSceneFactory(assets, context.ProjectRootPath, context.AssetAuthoring);
            TiltTrialGameplayPresentationBlueprintGenerator generator = new TiltTrialGameplayPresentationBlueprintGenerator(context.AssetAuthoring);
            generator.Generate(sceneFactory);
        }
    }
}
