using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Persists the FPS overlay font scale used by the small-screen platforms on generated Demo Disc scene entities.
    /// </summary>
    public static class SmallScreenFpsComponentOverrideService {
        /// <summary>
        /// Editor service that writes platform-specific component values into entity save state.
        /// </summary>
        static readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

        /// <summary>
        /// Platforms whose frame buffers are too small for the shared two-times overlay: PSP at 480x272 and N64 at 320x240.
        /// </summary>
        static readonly string[] PlatformIds = ["psp", "n64"];

        /// <summary>
        /// Small-screen font scale, half of the shared two-times overlay. The N64 value was tuned against an emulator
        /// capture rather than derived: a quarter of the shared scale produced glyphs roughly four pixels tall, which
        /// the pass's one-bit alpha cutout reduced to unreadable smears, so N64 shares the PSP value.
        /// </summary>
        const float FontScale = 1f;

        /// <summary>
        /// Adds the persisted small-screen font-scale override to the FPS component owned by one generated entity.
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
            for (int index = 0; index < PlatformIds.Length; index++) {
                string platformId = PlatformIds[index];
                FPSComponent overrideComponent = (FPSComponent)PlatformEditingService.EnsurePlatformOverrideComponent(
                    fpsComponent,
                    saveComponent,
                    platformId);
                overrideComponent.FontScale = FontScale;
                PlatformEditingService.MarkPropertyOverride(
                    fpsComponent,
                    saveComponent,
                    platformId,
                    nameof(FPSComponent.FontScale));
                PlatformEditingService.PersistPlatformOverride(
                    fpsComponent,
                    overrideComponent,
                    saveComponent,
                    platformId);
            }
        }
    }
}
