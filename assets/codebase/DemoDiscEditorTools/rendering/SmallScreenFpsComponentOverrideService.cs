using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Persists the FPS overlay edge padding used by the 240p sd/msd group on generated Demo Disc scene entities.
    /// The overlay font itself needs no per-platform override because the shared scene UI font is cooked per platform.
    /// </summary>
    public static class SmallScreenFpsComponentOverrideService {
        /// <summary>
        /// Editor service that writes platform-specific component values into entity save state.
        /// </summary>
        static readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

        /// <summary>
        /// Adds the persisted 240p padding override to the FPS component owned by one generated entity. The wider vertical
        /// padding keeps the counter inside the visible area of a 240p television.
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

            saveComponent.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            EditorOverrideScope microSdScope = DemoDiscOverrideScopes.MicroSd;
            FPSComponent microSd = (FPSComponent)PlatformEditingService.EnsureScopeOverrideComponent(
                fpsComponent, saveComponent, microSdScope);
            microSd.Padding = new int2(8, 16);
            PlatformEditingService.MarkScopePropertyOverride(
                fpsComponent, saveComponent, microSdScope, nameof(FPSComponent.Padding));
            PlatformEditingService.PersistScopeOverride(fpsComponent, microSd, saveComponent, microSdScope);
        }
    }
}
