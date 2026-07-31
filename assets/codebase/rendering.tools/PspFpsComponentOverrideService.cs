namespace city.rendering.tools {
    /// <summary>
    /// Persists the PSP-specific FPS overlay scale on generated Demo Disc scene entities.
    /// </summary>
    public static class PspFpsComponentOverrideService {
        /// <summary>
        /// Editor service that writes platform-specific component values into entity save state.
        /// </summary>
        static readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

        /// <summary>
        /// Platform identifier used by PSP scene overrides.
        /// </summary>
        const string PspPlatformId = "psp";

        /// <summary>
        /// PSP font scale that is half of the shared two-times FPS overlay scale.
        /// </summary>
        const float PspFpsFontScale = 1f;

        /// <summary>
        /// Adds the persisted PSP font-scale override to the FPS component owned by one generated entity.
        /// </summary>
        /// <param name="entity">Generated entity containing one FPS component and editor save state.</param>
        public static void Apply(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated FPS entities must expose initialized component collections.");
            }

            FPSComponent fpsComponent = null;
            EntitySaveComponent saveComponent = null;
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                Component component = entity.Components[componentIndex];
                if (component is FPSComponent currentFpsComponent) {
                    fpsComponent = currentFpsComponent;
                } else if (component is EntitySaveComponent currentSaveComponent) {
                    saveComponent = currentSaveComponent;
                }
            }

            if (fpsComponent == null) {
                throw new InvalidOperationException("Generated FPS entities must contain an FPS component.");
            } else if (saveComponent == null) {
                throw new InvalidOperationException("Generated FPS entities must contain an editor save component.");
            }

            FPSComponent overrideComponent = (FPSComponent)PlatformEditingService.EnsurePlatformOverrideComponent(
                fpsComponent,
                saveComponent,
                PspPlatformId);
            overrideComponent.FontScale = PspFpsFontScale;
            PlatformEditingService.MarkPropertyOverride(
                fpsComponent,
                saveComponent,
                PspPlatformId,
                nameof(FPSComponent.FontScale));
            PlatformEditingService.PersistPlatformOverride(
                fpsComponent,
                overrideComponent,
                saveComponent,
                PspPlatformId);
        }
    }
}
