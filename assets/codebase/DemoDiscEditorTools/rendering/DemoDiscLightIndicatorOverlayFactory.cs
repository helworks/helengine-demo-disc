using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Authors the light-state swatch that the rendering demo-disc scenes show on the light row of the instruction panel.
    /// </summary>
    public sealed class DemoDiscLightIndicatorOverlayFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Initializes the light indicator authoring factory with the host-owned public capability.
        /// </summary>
        public DemoDiscLightIndicatorOverlayFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }
        /// <summary>
        /// Stable entity name used by the screen-bound viewport root that hosts the light indicator row.
        /// </summary>
        public const string IndicatorViewportEntityName = "DemoDiscLightIndicatorViewport";

        /// <summary>
        /// Stable entity name used by the preview square whose fill color mirrors the current light state.
        /// </summary>
        public const string IndicatorSwatchEntityName = "DemoDiscLightIndicatorSwatch";

        /// <summary>
        /// Reference width used by the screen-bound indicator viewport, matching the instruction panel canvas.
        /// </summary>
        const int ReferenceViewportWidth = 1280;

        /// <summary>
        /// Reference height used by the screen-bound indicator viewport, matching the instruction panel canvas.
        /// </summary>
        const int ReferenceViewportHeight = 720;

        /// <summary>
        /// World depth of the preview square, above the instruction panel it sits on.
        /// </summary>
        const float IndicatorDepth = 252;

        /// <summary>
        /// Shared rounded-corner radius used by the preview square.
        /// </summary>
        const float IndicatorSwatchRadius = 2f;

        /// <summary>
        /// Shared border thickness used by the preview square.
        /// </summary>
        const float IndicatorSwatchBorderThickness = 1f;

        /// <summary>
        /// Creates a serialized reference to one generated overlay entity.
        /// </summary>
        /// <param name="entity">Generated entity that should receive a persisted scene id.</param>
        /// <returns>Serialized reference to the generated entity.</returns>
        SceneEntityReference CreateEntityReference(Entity entity) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            if (saveComponent.EntityId == 0u) {
                if (AssetAuthoringService.OwningCore is not EditorCore editorCore || editorCore.SceneEntityIdAllocator == null) {
                    throw new InvalidOperationException("Generated light-indicator references require an active editor scene-entity id allocator.");
                }

                saveComponent.EntityId = editorCore.SceneEntityIdAllocator.Allocate();
            }

            return new SceneEntityReference { EntityId = saveComponent.EntityId };
        }
        /// <summary>
        /// Attaches the light-state swatch beneath the supplied scene UI root, placed on the light row of the instruction panel.
        /// </summary>
        /// <remarks>
        /// The swatch stays a scene entity, not part of the instruction Blueprint, because the light toggle resolves it through a
        /// scene entity reference. It lives in its own screen-bound viewport with the panel's 1280x720 reference canvas, so the
        /// panel-space slot maps to the same screen position as the panel.
        /// </remarks>
        /// <param name="sceneUiEntity">Scene UI entity that should own the swatch.</param>
        public void AttachToSceneUi(Entity sceneUiEntity) {
            if (sceneUiEntity == null) {
                throw new ArgumentNullException(nameof(sceneUiEntity));
            }

            ushort overlayLayerMask = sceneUiEntity.LayerMask;
            Entity viewportEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(sceneUiEntity, IndicatorViewportEntityName);
            viewportEntity.LayerMask = overlayLayerMask;
            // ScalingMode is load-bearing: ViewportComponent.Update applies no scale unless the mode is ReferenceCanvasScalingMode,
            // and without it the swatch would keep its reference size on every screen instead of following the panel.
            viewportEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                FixedSize = new int2(ReferenceViewportWidth, ReferenceViewportHeight)
            });

            Entity swatchEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(viewportEntity, IndicatorSwatchEntityName);
            swatchEntity.LocalPosition = new float3(
                DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft + DemoSceneInstructionOverlayFactory.LightSwatchPanelLeft,
                DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop + DemoSceneInstructionOverlayFactory.LightSwatchPanelTop,
                0.1f);
            swatchEntity.LayerMask = overlayLayerMask;
            SceneUiDepth.SetWorldDepth(swatchEntity, IndicatorDepth);
            swatchEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(DemoSceneInstructionOverlayFactory.LightSwatchSize, DemoSceneInstructionOverlayFactory.LightSwatchSize),
                Radius = IndicatorSwatchRadius,
                BorderThickness = IndicatorSwatchBorderThickness,
                FillColor = new byte4(255, 255, 255, 255),
                BorderColor = new byte4(30, 30, 30, 255),
            });

            // The micro SD group moves the panel up and scales it about its top-left corner; follow it so the swatch stays on the light row.
            const float MicroSdScale = DemoSceneInstructionOverlayFactory.MicroSdInstructionPanelScale;
            EntitySaveComponent swatchSave = FindRequiredEntitySaveComponent(swatchEntity);
            swatchSave.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            SceneEntityPlatformTransformOverrideAsset microSdTransform = swatchSave.GetOrCreateTransformPlatformOverride(DemoDiscOverrideScopes.MicroSd);
            microSdTransform.HasLocalPositionOverride = true;
            microSdTransform.LocalPosition = new float3(
                DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft + DemoSceneInstructionOverlayFactory.LightSwatchPanelLeft * MicroSdScale,
                DemoSceneInstructionOverlayFactory.MicroSdInstructionPanelTop + DemoSceneInstructionOverlayFactory.LightSwatchPanelTop * MicroSdScale,
                0.1f);
            microSdTransform.HasLocalScaleOverride = true;
            microSdTransform.LocalScale = new float3(MicroSdScale, MicroSdScale, 1f);

            DemoDiscLightToggleComponent lightToggle = FindRequiredLightToggle(sceneUiEntity);
            lightToggle.IndicatorSwatchEntityReference = CreateEntityReference(swatchEntity);
        }

        DemoDiscLightToggleComponent FindRequiredLightToggle(Entity sceneUiEntity) {
            if (sceneUiEntity.Components == null) {
                throw new InvalidOperationException("Scene UI entities must expose initialized components.");
            }

            for (int componentIndex = 0; componentIndex < sceneUiEntity.Components.Count; componentIndex++) {
                if (sceneUiEntity.Components[componentIndex] is DemoDiscLightToggleComponent lightToggle) {
                    return lightToggle;
                }
            }

            throw new InvalidOperationException("Scene UI root must contain one light toggle component before creating the indicator.");
        }

        /// <summary>
        /// Resolves the hidden entity-save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity-save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entity is missing required save state.");
        }
    }
}
