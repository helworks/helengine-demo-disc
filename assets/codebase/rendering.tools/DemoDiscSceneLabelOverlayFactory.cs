namespace city.rendering.tools {
    public sealed class DemoDiscSceneLabelOverlayFactory {
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;
        const string LabelEntityName = "DemoDiscSceneLabelText";
        const string FontReferenceName = "Font";
        const string NintendoDsPlatformId = "ds";
        const string Nintendo3DsPlatformId = "3ds";
        const string SceneLabelFontRelativePath = "Fonts/DemoDiscBody.ttf";
        const int SceneLabelCanvasWidth = 1280;
        const float SceneLabelRight = 24f;
        const float SceneLabelTop = 72f;
        const int SceneLabelWidth = 656;
        const int SceneLabelHeight = 56;
        const float SceneLabelFontScale = 1.35f;
        const int SceneLabelRenderOrder = 7;
        public DemoDiscSceneLabelOverlayFactory(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        public void AttachToSceneUi(Entity sceneUiEntity, FontAsset font, string labelText) {
            if (sceneUiEntity == null) {
                throw new ArgumentNullException(nameof(sceneUiEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(labelText)) {
                throw new ArgumentException("Scene label text must be provided.", nameof(labelText));
            }

            ushort overlayLayerMask = sceneUiEntity.LayerMask;
            Entity labelEntity = Core.Instance.EntityFactory.CreateChild(sceneUiEntity, LabelEntityName);
            labelEntity.LocalPosition = new float3(
                SceneLabelCanvasWidth - SceneLabelRight - SceneLabelWidth,
                SceneLabelTop,
                0.1f);
            labelEntity.Static = false;
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
                global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateFont(AssetAuthoringService, SceneLabelFontRelativePath));
            sceneUiEntity.AddComponent(new city.rendering.DemoDiscDebugSceneLabelComponent());
            saveComponent.GetOrCreateExistencePlatformOverride(NintendoDsPlatformId).Exists = false;
            saveComponent.GetOrCreateExistencePlatformOverride(Nintendo3DsPlatformId).Exists = false;
            labelEntity.Enabled = true;
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
