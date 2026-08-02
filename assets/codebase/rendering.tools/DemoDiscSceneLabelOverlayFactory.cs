using helengine.editor;

namespace city.rendering.tools {
    public sealed class DemoDiscSceneLabelOverlayFactory {
        public const string LabelViewportEntityName = "DemoDiscSceneLabelViewport";
        public const string LabelEntityName = "DemoDiscSceneLabelText";
        const string FontReferenceName = "Font";
        const int ReferenceViewportWidth = 1280;
        const int ReferenceViewportHeight = 720;
        const float SceneLabelRight = 24f;
        const float SceneLabelTop = 24f;
        const int SceneLabelWidth = 420;
        const int SceneLabelHeight = 32;
        const float SceneLabelFontScale = 1.5f;
        const int SceneLabelRenderOrder = 255;

        public void AttachToSceneUi(Entity sceneUiEntity, FontAsset font, string labelText) {
            if (sceneUiEntity == null) {
                throw new ArgumentNullException(nameof(sceneUiEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(labelText)) {
                throw new ArgumentException("Scene label text must be provided.", nameof(labelText));
            }

            ushort overlayLayerMask = sceneUiEntity.LayerMask;
            Entity viewportEntity = Core.Instance.EntityFactory.CreateChild(sceneUiEntity, LabelViewportEntityName);
            viewportEntity.LayerMask = overlayLayerMask;
            viewportEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(ReferenceViewportWidth, ReferenceViewportHeight)
            });

            Entity labelEntity = Core.Instance.EntityFactory.CreateChild(viewportEntity, LabelEntityName);
            labelEntity.LocalPosition = new float3(
                ReferenceViewportWidth - SceneLabelRight - SceneLabelWidth,
                SceneLabelTop,
                0.1f);
            labelEntity.LayerMask = overlayLayerMask;
            TextComponent labelComponent = new TextComponent {
                Text = labelText,
                Font = font,
                FontScale = SceneLabelFontScale,
                Alignment = TextAlignment.Right,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(SceneLabelWidth, SceneLabelHeight),
                RenderOrder2D = SceneLabelRenderOrder
            };
            labelEntity.AddComponent(labelComponent);
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(labelEntity);
            saveComponent.SetAssetReference(
                labelComponent,
                FontReferenceName,
                DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

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
