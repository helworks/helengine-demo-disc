using helengine.editor;
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
        /// PSP keeps its own platform override; N64 and PS1 share the sd/msd group override.
        /// </summary>
        const string PspPlatformId = "psp";

        /// <summary>
        /// Small-screen font scale, half of the shared two-times overlay. The 240p value was tuned against
        /// an N64 emulator capture and applies to both platforms in sd/msd.
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
            saveComponent.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            EditorOverrideScope microSdScope = DemoDiscOverrideScopes.MicroSd;
            FPSComponent microSd = (FPSComponent)PlatformEditingService.EnsureScopeOverrideComponent(
                fpsComponent, saveComponent, microSdScope);
            microSd.FontScale = FontScale;
            microSd.Padding = new int2(8, 16);
            PlatformEditingService.MarkScopePropertyOverride(
                fpsComponent, saveComponent, microSdScope, nameof(FPSComponent.FontScale));
            PlatformEditingService.MarkScopePropertyOverride(
                fpsComponent, saveComponent, microSdScope, nameof(FPSComponent.Padding));
            PlatformEditingService.PersistScopeOverride(fpsComponent, microSd, saveComponent, microSdScope);

            EditorOverrideScope pspScope = DemoDiscOverrideScopes.Sd.Append(
                new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Platform, PspPlatformId));
            FPSComponent psp = (FPSComponent)PlatformEditingService.EnsureScopeOverrideComponent(
                fpsComponent, saveComponent, pspScope);
            psp.FontScale = FontScale;
            PlatformEditingService.MarkScopePropertyOverride(
                fpsComponent, saveComponent, pspScope, nameof(FPSComponent.FontScale));
            PlatformEditingService.PersistScopeOverride(fpsComponent, psp, saveComponent, pspScope);
        }
    }
}
