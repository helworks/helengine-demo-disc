namespace city.rendering.tools {
    /// <summary>
    /// Persists the N64-specific FPS overlay scale on generated Demo Disc scene entities.
    /// </summary>
    public static class Nintendo64FpsComponentOverrideService {
        /// <summary>
        /// Editor service that writes platform-specific component values into entity save state.
        /// </summary>
        static readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

        /// <summary>
        /// Platform identifier used by N64 scene overrides.
        /// </summary>
        const string Nintendo64PlatformId = "n64";

        /// <summary>
        /// N64 font scale. The shared overlay is authored at two times for desktop resolutions and PSP already
        /// halves it to one for a 480x272 screen; the N64 frame buffer is 320x240, so it takes a quarter of the
        /// shared scale. This is a starting value tuned against a capture in the final task, not a derived one,
        /// so it should not be treated as sacred.
        /// </summary>
        const float Nintendo64FpsFontScale = 0.5f;

        /// <summary>
        /// Adds the persisted N64 font-scale override to the FPS component owned by one generated entity.
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

            saveComponent.SetAssetReference(
                fpsComponent,
                "Font",
                DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());
            FPSComponent overrideComponent = (FPSComponent)PlatformEditingService.EnsurePlatformOverrideComponent(
                fpsComponent,
                saveComponent,
                Nintendo64PlatformId);
            overrideComponent.FontScale = Nintendo64FpsFontScale;
            PlatformEditingService.MarkPropertyOverride(
                fpsComponent,
                saveComponent,
                Nintendo64PlatformId,
                nameof(FPSComponent.FontScale));
            PlatformEditingService.PersistPlatformOverride(
                fpsComponent,
                overrideComponent,
                saveComponent,
                Nintendo64PlatformId);
        }
    }
}
