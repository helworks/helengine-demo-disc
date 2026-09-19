namespace city.rendering.tools {
    public sealed class DemoDiscSceneLabelOverlayFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        const string LabelEntityName = "DemoDiscSceneLabelText";
        const string FontReferenceName = "Font";
        const string NintendoDsPlatformId = "ds";
        const string Nintendo3DsPlatformId = "3ds";
        const string SceneLabelFontRelativePath = "Fonts/DemoDiscBody.ttf";
        const string ViewportEntityName = "DemoDiscSceneLabelViewport";
        const int SceneLabelCanvasWidth = 1280;
        const int SceneLabelCanvasHeight = 720;
        const float SceneLabelRight = 24f;
        const float SceneLabelTop = 72f;
        const int SceneLabelWidth = 656;
        const int SceneLabelHeight = 56;
        const float SceneLabelFontScale = 1.35f;
        const int SceneLabelRenderOrder = 7;
        public DemoDiscSceneLabelOverlayFactory(IEditorProjectAuthoringSession assetAuthoringService) {
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
            // The label is authored against the 1280-wide canvas the constants below subtract from, but it had
            // no viewport, so that canvas was an assumption rather than something the runtime enforced. On a
            // 320x240 frame buffer its 656-wide box therefore began past the right edge and the label never
            // appeared at all. Wrapping it in a reference-canvas viewport is what every other overlay in this
            // project does and is what makes the authored coordinates mean what they say on any screen size.
            Entity viewportEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(sceneUiEntity, ViewportEntityName);
            viewportEntity.LayerMask = overlayLayerMask;
            viewportEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                FixedSize = new int2(SceneLabelCanvasWidth, SceneLabelCanvasHeight)
            });
            Entity labelEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(viewportEntity, LabelEntityName);
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
                AssetAuthoringService.CreateFileReference(SceneLabelFontRelativePath, AssetEntryKind.Font));
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
