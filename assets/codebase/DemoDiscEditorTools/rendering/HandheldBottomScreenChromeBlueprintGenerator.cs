using DemoDisc.menu;
using DemoDisc.rendering;
using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Writes the shared dual-screen bottom-screen chrome as one Blueprint. The light and back buttons are
    /// identical in every handheld scene and reference nothing outside themselves, so they live in one asset
    /// instead of being rebuilt into each generated scene.
    /// </summary>
    public sealed class HandheldBottomScreenChromeBlueprintGenerator {
        /// <summary>
        /// Stable project-relative path of the shared bottom-screen chrome Blueprint.
        /// </summary>
        public const string BlueprintRelativePath = "blueprints/handheld/HandheldBottomScreenChrome.hblueprint";

        /// <summary>
        /// Stable name of the Blueprint root entity that owns the chrome.
        /// </summary>
        public const string ChromeRootEntityName = "DemoDiscBottomScreenChrome";

        /// <summary>
        /// Fixed font scale used by the dual-screen bottom overlay so button text matches the physics showcase sizing.
        /// </summary>
        const float NintendoDsBottomOverlayFontScale = 1f;

        /// <summary>
        /// Fixed font scale used by Nintendo 3DS button labels once the shared reference canvas resolves to the 3DS screen.
        /// </summary>
        const float Nintendo3DsBottomButtonLabelFontScale = 0.5f;

        /// <summary>
        /// Stable platform identifier used for the generated Nintendo 3DS button-label component override.
        /// </summary>
        const string Nintendo3DsPlatformId = "3ds";

        /// <summary>
        /// Runtime layer mask used by generated bottom-screen entities so the handheld cameras can render them.
        /// </summary>
        const ushort PersistedSceneLayerMask = EditorLayerMasks.SceneObjects;

        /// <summary>
        /// Fixed Nintendo DS screen width the shared bottom layout is authored against.
        /// </summary>
        const int ScreenWidth = 256;

        /// <summary>
        /// Fixed Nintendo DS screen height the shared bottom layout is authored against.
        /// </summary>
        const int ScreenHeight = 192;

        /// <summary>
        /// Fixed width used by the bottom-screen action button body.
        /// </summary>
        const int BackButtonWidth = 224;

        /// <summary>
        /// Fixed height used by the bottom-screen action button body.
        /// </summary>
        const int BackButtonHeight = 32;

        /// <summary>
        /// Fixed left offset that keeps the action buttons horizontally centered.
        /// </summary>
        const int BackButtonLeft = (ScreenWidth - BackButtonWidth) / 2;

        /// <summary>
        /// Fixed top offset that pins the back button near the bottom edge.
        /// </summary>
        const int BackButtonTop = ScreenHeight - BackButtonHeight - 6;

        /// <summary>
        /// Fixed horizontal inset used by the back button label.
        /// </summary>
        const int BackButtonLabelLeft = 80;

        /// <summary>
        /// Fixed vertical inset used by the action button labels.
        /// </summary>
        const int ButtonLabelTop = 6;

        /// <summary>
        /// Fixed width used by the back button label.
        /// </summary>
        const int BackButtonLabelWidth = 64;

        /// <summary>
        /// Fixed height used by the action button labels.
        /// </summary>
        const int ButtonLabelHeight = 20;

        /// <summary>
        /// Fixed top offset that stacks the light button above the back button.
        /// </summary>
        const int LightButtonTop = BackButtonTop - BackButtonHeight - 8;

        /// <summary>
        /// Fixed horizontal inset used by the light button label.
        /// </summary>
        const int LightButtonLabelLeft = 64;

        /// <summary>
        /// Fixed width used by the light button label.
        /// </summary>
        const int LightButtonLabelWidth = 80;

        /// <summary>
        /// Fixed left offset used by the light swatch.
        /// </summary>
        const int LightSwatchLeft = 148;

        /// <summary>
        /// Fixed top offset used by the light swatch.
        /// </summary>
        const int LightSwatchTop = 4;

        /// <summary>
        /// Fixed square size used by the light swatch.
        /// </summary>
        const int LightSwatchSize = 16;

        /// <summary>
        /// Render order used by the light swatch. It must sit at 220 or above: the DS bottom-screen renderer only
        /// promotes orders at or above 220 to the foreground OBJ priority, and at the base priority the
        /// earlier-drawn opaque button body wins the hardware tie and hides the swatch.
        /// </summary>
        const byte LightSwatchRenderOrder = 222;

        /// <summary>
        /// Render order used by the action button sprite body.
        /// </summary>
        const byte ButtonSpriteRenderOrder = 210;

        /// <summary>
        /// Render order used by the action button labels.
        /// </summary>
        const byte ButtonLabelRenderOrder = 221;

        /// <summary>
        /// Render order used by the transparent borders drawn over the action buttons.
        /// </summary>
        const byte ButtonBorderRenderOrder = 220;

        /// <summary>
        /// Border thickness used by the action buttons.
        /// </summary>
        const float ButtonBorderThickness = 2f;

        /// <summary>
        /// Host-owned capability used to author and persist the Blueprint.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Caller-owned transaction that publishes the Blueprint.
        /// </summary>
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Editor component override service used to persist the 3DS label scale without changing the shared baseline.
        /// </summary>
        readonly ComponentPlatformEditingService PlatformEditingServiceValue = new ComponentPlatformEditingService();

        /// <summary>
        /// Initializes one bottom-screen chrome Blueprint generator.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to author the Blueprint.</param>
        /// <param name="transaction">Caller-owned transaction that publishes the Blueprint.</param>
        public HandheldBottomScreenChromeBlueprintGenerator(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Writes the shared bottom-screen chrome Blueprint. The Blueprint serializer requires exactly one live
        /// editable root, so this runs before any scene factory creates authoring entities.
        /// </summary>
        public void Generate() {
            if (AssetAuthoringService.OwningCore == null) {
                throw new InvalidOperationException("Bottom-screen chrome generation requires an active editor core.");
            }

            Entity root = AssetAuthoringService.OwningCore.EntityFactory.Create(ChromeRootEntityName);
            try {
                root.LayerMask = PersistedSceneLayerMask;
                root.LocalPosition = float3.Zero;
                root.LocalScale = float3.One;
                root.LocalOrientation = float4.Identity;
                CreateLightButton(root);
                CreateBackButton(root);
                AssetAuthoringService.WriteNativeBlueprint(
                    BlueprintRelativePath,
                    GeneratedScenePersistenceRegistryFactory.Create(),
                    ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(BlueprintRelativePath),
                    Transaction);
            } finally {
                root.Dispose();
            }
        }

        /// <summary>
        /// Creates the light button that routes touch interaction and shoulder input through the shared handheld light cycle.
        /// </summary>
        /// <param name="chromeRoot">Chrome root that owns the button.</param>
        void CreateLightButton(Entity chromeRoot) {
            Entity lightButtonEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(chromeRoot, "DemoDiscBottomScreenLightButton");
            lightButtonEntity.LocalPosition = new float3(BackButtonLeft, LightButtonTop, 0f);
            lightButtonEntity.LayerMask = PersistedSceneLayerMask;
            lightButtonEntity.Static = true;

            CreateButtonBody(lightButtonEntity);
            CreateButtonBorder(lightButtonEntity);

            lightButtonEntity.AddComponent(new InteractableComponent {
                Size = new int2(BackButtonWidth, BackButtonHeight)
            });
            lightButtonEntity.AddComponent(new NintendoDsLightToggleOverlayComponent());

            Entity labelEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(lightButtonEntity, "DemoDiscBottomScreenLightButtonLabel");
            labelEntity.LocalPosition = new float3(LightButtonLabelLeft, ButtonLabelTop, 0f);
            labelEntity.LayerMask = PersistedSceneLayerMask;
            labelEntity.Static = true;
            CreateButtonLabel(labelEntity, "LIGHT", LightButtonLabelWidth);

            Entity lightSwatchEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(lightButtonEntity, "DemoDiscBottomScreenLightSwatch");
            lightSwatchEntity.LocalPosition = new float3(LightSwatchLeft, LightSwatchTop, 0.1f);
            lightSwatchEntity.LayerMask = PersistedSceneLayerMask;
            lightSwatchEntity.Static = true;
            lightSwatchEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(LightSwatchSize, LightSwatchSize),
                Radius = 2f,
                BorderThickness = 1f,
                FillColor = new byte4(255, 255, 255, 255),
                BorderColor = new byte4(30, 30, 30, 255),
                RenderOrder2D = LightSwatchRenderOrder,
            });
        }

        /// <summary>
        /// Creates the back button that routes touch interaction back to the demo-disc menu.
        /// </summary>
        /// <param name="chromeRoot">Chrome root that owns the button.</param>
        void CreateBackButton(Entity chromeRoot) {
            Entity backButtonEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(chromeRoot, "DemoDiscBottomScreenBackButton");
            backButtonEntity.LocalPosition = new float3(BackButtonLeft, BackButtonTop, 0f);
            backButtonEntity.LayerMask = PersistedSceneLayerMask;
            backButtonEntity.Static = true;

            CreateButtonBody(backButtonEntity);
            CreateButtonBorder(backButtonEntity);

            backButtonEntity.AddComponent(new InteractableComponent {
                Size = new int2(BackButtonWidth, BackButtonHeight)
            });
            backButtonEntity.AddComponent(new NintendoDsReturnOverlayComponent());

            Entity labelEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(backButtonEntity, "DemoDiscBottomScreenBackButtonLabel");
            labelEntity.LocalPosition = new float3(BackButtonLabelLeft, ButtonLabelTop, 0f);
            labelEntity.LayerMask = PersistedSceneLayerMask;
            labelEntity.Static = true;
            CreateButtonLabel(labelEntity, "BACK", BackButtonLabelWidth);
        }

        /// <summary>
        /// Adds one centered action-button label carrying the shared baseline scale and the 3DS override.
        /// </summary>
        /// <param name="labelEntity">Entity that owns the label.</param>
        /// <param name="text">Label text.</param>
        /// <param name="width">Label width.</param>
        void CreateButtonLabel(Entity labelEntity, string text, int width) {
            TextComponent labelComponent = new TextComponent {
                Text = text,
                Font = AssetAuthoringService.RendererResources.DefaultFontAsset,
                FontScale = NintendoDsBottomOverlayFontScale,
                Alignment = TextAlignment.Center,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(width, ButtonLabelHeight),
                RenderOrder2D = ButtonLabelRenderOrder,
            };
            labelEntity.AddComponent(labelComponent);
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(labelEntity);
            saveComponent.SetAssetReference(
                labelComponent,
                "Font",
                DemoDiscSceneComponentRecordFactory.CreateEditorFontReference(AssetAuthoringService));
            ApplyNintendo3DsLabelOverride(labelEntity, labelComponent);
        }

        /// <summary>
        /// Adds a palette-free solid body so the control stays visible after scene sprites consume OBJ palette banks.
        /// </summary>
        /// <param name="buttonEntity">Action-button entity receiving the body.</param>
        void CreateButtonBody(Entity buttonEntity) {
            buttonEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(BackButtonWidth, BackButtonHeight),
                Radius = 3f,
                BorderThickness = 0f,
                FillColor = new byte4(48, 29, 65, 255),
                BorderColor = new byte4(48, 29, 65, 255),
                RenderOrder2D = ButtonSpriteRenderOrder,
            });
        }

        /// <summary>
        /// Adds a transparent rounded border above one action-button sprite so its edge stays visible on every handheld renderer.
        /// </summary>
        /// <param name="buttonEntity">Action-button entity receiving the border overlay.</param>
        void CreateButtonBorder(Entity buttonEntity) {
            Entity borderEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(buttonEntity, "Border");
            borderEntity.LocalPosition = new float3(0f, 0f, 0.1f);
            borderEntity.LayerMask = PersistedSceneLayerMask;
            borderEntity.Static = true;
            borderEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(BackButtonWidth, BackButtonHeight),
                Radius = 3f,
                BorderThickness = ButtonBorderThickness,
                FillColor = new byte4(0, 0, 0, 0),
                BorderColor = new byte4(201, 147, 255, 255),
                RenderOrder2D = ButtonBorderRenderOrder,
            });
        }

        /// <summary>
        /// Persists the smaller Nintendo 3DS label scale while retaining one shared centered label definition.
        /// </summary>
        /// <param name="labelEntity">Generated button-label entity receiving the platform override.</param>
        /// <param name="commonLabelComponent">Shared label component used as the baseline.</param>
        void ApplyNintendo3DsLabelOverride(Entity labelEntity, TextComponent commonLabelComponent) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(labelEntity);
            TextComponent overrideComponent = (TextComponent)PlatformEditingServiceValue.EnsurePlatformOverrideComponent(
                commonLabelComponent,
                saveComponent,
                Nintendo3DsPlatformId);
            overrideComponent.FontScale = Nintendo3DsBottomButtonLabelFontScale;
            PlatformEditingServiceValue.MarkPropertyOverride(
                commonLabelComponent,
                saveComponent,
                Nintendo3DsPlatformId,
                nameof(TextComponent.FontScale));
            PlatformEditingServiceValue.PersistPlatformOverride(
                commonLabelComponent,
                overrideComponent,
                saveComponent,
                Nintendo3DsPlatformId);
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        static EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated bottom-screen entities must include EntitySaveComponent.");
        }
    }
}
