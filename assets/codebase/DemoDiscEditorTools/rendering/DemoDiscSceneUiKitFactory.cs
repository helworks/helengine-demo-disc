using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Builds the standard demo-disc scene UI root so every rendering and physics showcase scene shares one 2D overlay kit.
    /// </summary>
    public sealed class DemoDiscSceneUiKitFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Initializes the shared UI kit with the host-owned public asset authoring capability.
        /// </summary>
        public DemoDiscSceneUiKitFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        /// <summary>
        /// Creates one authored UI root carrying diagnostics, return-to-menu handling, light controls, and the optional debug label.
        /// </summary>
        /// <param name="entityName">Stable name for the generated UI root entity.</param>
        /// <param name="sceneLabel">Numbered scene label shown by debug-environment builds.</param>
        /// <param name="directionalLightEntities">Directional-light entities authored by this scene.</param>
        /// <returns>Live authored UI root entity.</returns>
        public Entity CreateStandardSceneUi(string entityName, string sceneLabel, Entity[] directionalLightEntities = null) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("UI root entity name must be provided.", nameof(entityName));
            }

            FontAsset font = ResolveRequiredEditorFont();
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create(entityName);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new FPSComponent { Font = font, FontScale = 2f });
            SmallScreenFpsComponentOverrideService.Apply(entity);
            entity.AddComponent(new DemoDisc.menu.DemoDiscReturnToMenuComponent { AllowPointerReturn = false });
            entity.AddComponent(new DemoDisc.rendering.DemoDiscLightToggleComponent {
                LightEntityReferences = CreateEntityReferences(directionalLightEntities)
            });
            new DemoDiscLightIndicatorOverlayFactory(AssetAuthoringService).AttachToSceneUi(entity, font);
            if (!string.IsNullOrWhiteSpace(sceneLabel)) {
                new DemoDiscSceneLabelOverlayFactory(AssetAuthoringService).AttachToSceneUi(entity, font, sceneLabel);
            }
            return entity;
        }

        /// <summary>
        /// Creates the dual-screen counterpart of the standard UI root for the bottom screen.
        /// </summary>
        /// <param name="entityName">Stable name for the generated UI root entity.</param>
        /// <param name="sceneLabel">Numbered scene label shown by debug-environment builds.</param>
        /// <returns>Live authored bottom-screen UI root entity.</returns>
        public Entity CreateHandheldSceneUi(string entityName, string sceneLabel) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("UI root entity name must be provided.", nameof(entityName));
            }

            FontAsset font = ResolveRequiredEditorFont();
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create(entityName);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new FPSComponent { Font = font, FontScale = HandheldFpsScale });
            if (!string.IsNullOrWhiteSpace(sceneLabel)) {
                new DemoDiscSceneLabelOverlayFactory(AssetAuthoringService).AttachToSceneUi(entity, font, sceneLabel);
            }
            return entity;
        }

        SceneEntityReference[] CreateEntityReferences(Entity[] entities) {
            if (entities == null || entities.Length == 0) {
                return Array.Empty<SceneEntityReference>();
            }

            SceneEntityReference[] references = new SceneEntityReference[entities.Length];
            for (int index = 0; index < entities.Length; index++) {
                EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entities[index]);
                if (saveComponent.EntityId == 0u) {
                    if (AssetAuthoringService.OwningCore is not EditorCore editorCore || editorCore.SceneEntityIdAllocator == null) {
                        throw new InvalidOperationException("Standard scene UI light references require an active editor scene-entity id allocator.");
                    }
                    saveComponent.EntityId = editorCore.SceneEntityIdAllocator.Allocate();
                }
                references[index] = new SceneEntityReference { EntityId = saveComponent.EntityId };
            }

            return references;
        }

        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException("Authored scene entities must expose initialized components.");
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }
            throw new InvalidOperationException("Authored scene entities require an EntitySaveComponent for references.");
        }

        /// <summary>
        /// Font scale used by bottom-screen diagnostics once the shared reference canvas resolves to a handheld screen.
        /// </summary>
        const float HandheldFpsScale = 1f;

        FontAsset ResolveRequiredEditorFont() {
            if (AssetAuthoringService.RendererResources.DefaultFontAsset == null) {
                throw new InvalidOperationException("A default editor font must be loaded before demo-disc scene UI can be generated.");
            }
            return AssetAuthoringService.RendererResources.DefaultFontAsset;
        }
    }
}