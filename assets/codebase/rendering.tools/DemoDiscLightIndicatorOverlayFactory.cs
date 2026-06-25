using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Authors the shared top-left light indicator row used by the rendering demo-disc scenes.
    /// </summary>
    public sealed class DemoDiscLightIndicatorOverlayFactory {
        /// <summary>
        /// Stable entity name used by the screen-bound viewport root that hosts the light indicator row.
        /// </summary>
        public const string IndicatorViewportEntityName = "DemoDiscLightIndicatorViewport";

        /// <summary>
        /// Stable entity name used by the text label rendered for the indicator row.
        /// </summary>
        public const string IndicatorLabelEntityName = "DemoDiscLightIndicatorLabel";

        /// <summary>
        /// Stable entity name used by the preview square whose fill color mirrors the current light state.
        /// </summary>
        public const string IndicatorSwatchEntityName = "DemoDiscLightIndicatorSwatch";

        /// <summary>
        /// Stable save-state slot name used for serialized font references.
        /// </summary>
        const string FontReferenceName = "Font";

        /// <summary>
        /// Fixed drawable layer mask used by the indicator label and preview square.
        /// </summary>
        const byte OverlayDrawableLayerMask = 0b00000001;

        /// <summary>
        /// Reference width used by the screen-bound indicator viewport.
        /// </summary>
        const int ReferenceViewportWidth = 1280;

        /// <summary>
        /// Reference height used by the screen-bound indicator viewport.
        /// </summary>
        const int ReferenceViewportHeight = 720;

        /// <summary>
        /// Left offset of the indicator label beneath the FPS overlay.
        /// </summary>
        const float IndicatorLabelLeft = 8f;

        /// <summary>
        /// Top offset of the indicator label beneath the FPS overlay.
        /// </summary>
        const float IndicatorLabelTop = 54f;

        /// <summary>
        /// Left offset of the preview square beside the indicator label.
        /// </summary>
        const float IndicatorSwatchLeft = 122f;

        /// <summary>
        /// Top offset of the preview square beside the indicator label.
        /// </summary>
        const float IndicatorSwatchTop = 73f;

        /// <summary>
        /// Width of the label text region.
        /// </summary>
        const int IndicatorLabelWidth = 52;

        /// <summary>
        /// Height of the label text region.
        /// </summary>
        const int IndicatorLabelHeight = 20;

        /// <summary>
        /// Width and height of the preview square.
        /// </summary>
        const int IndicatorSwatchSize = 32;

        /// <summary>
        /// Shared font scale used by the indicator label.
        /// </summary>
        const float IndicatorLabelFontScale = 1.5f;

        /// <summary>
        /// Shared two-dimensional render order used by the indicator label and preview square.
        /// </summary>
        const int IndicatorRenderOrder = 252;

        /// <summary>
        /// Shared rounded-corner radius used by the preview square.
        /// </summary>
        const float IndicatorSwatchRadius = 2f;

        /// <summary>
        /// Shared border thickness used by the preview square.
        /// </summary>
        const float IndicatorSwatchBorderThickness = 1f;

        /// <summary>
        /// Attaches the shared screen-bound light indicator row beneath the supplied scene UI root.
        /// </summary>
        /// <param name="sceneUiEntity">Scene UI entity that should own the indicator row.</param>
        /// <param name="font">Font assigned to the indicator label during live authoring.</param>
        public void AttachToSceneUi(Entity sceneUiEntity, FontAsset font) {
            if (sceneUiEntity == null) {
                throw new ArgumentNullException(nameof(sceneUiEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            }

            ushort overlayLayerMask = sceneUiEntity.LayerMask;
            Entity viewportEntity = Core.Instance.EntityFactory.CreateChild(sceneUiEntity, IndicatorViewportEntityName);
            viewportEntity.LayerMask = overlayLayerMask;
            viewportEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(ReferenceViewportWidth, ReferenceViewportHeight)
            });

            Entity labelEntity = Core.Instance.EntityFactory.CreateChild(viewportEntity, IndicatorLabelEntityName);
            labelEntity.LocalPosition = new float3(IndicatorLabelLeft, IndicatorLabelTop, 0.1f);
            labelEntity.LayerMask = overlayLayerMask;
            TextComponent labelComponent = new TextComponent {
                Text = "Light",
                Font = font,
                FontScale = IndicatorLabelFontScale,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(IndicatorLabelWidth, IndicatorLabelHeight),
                RenderOrder2D = IndicatorRenderOrder,
                LayerMask = OverlayDrawableLayerMask
            };
            labelEntity.AddComponent(labelComponent);
            ApplyEditorFontReference(labelEntity, labelComponent);

            Entity swatchEntity = Core.Instance.EntityFactory.CreateChild(viewportEntity, IndicatorSwatchEntityName);
            swatchEntity.LocalPosition = new float3(IndicatorSwatchLeft, IndicatorSwatchTop, 0.1f);
            swatchEntity.LayerMask = overlayLayerMask;
            swatchEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(IndicatorSwatchSize, IndicatorSwatchSize),
                Radius = IndicatorSwatchRadius,
                BorderThickness = IndicatorSwatchBorderThickness,
                FillColor = new byte4(255, 255, 255, 255),
                BorderColor = new byte4(30, 30, 30, 255),
                RenderOrder2D = IndicatorRenderOrder,
                LayerMask = OverlayDrawableLayerMask
            });
        }

        /// <summary>
        /// Stores the editor font reference on the generated scene save state for the supplied text component.
        /// </summary>
        /// <param name="entity">Entity that owns the text component.</param>
        /// <param name="component">Text component whose font reference should be stored.</param>
        void ApplyEditorFontReference(Entity entity, Component component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, FontReferenceName, DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
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
