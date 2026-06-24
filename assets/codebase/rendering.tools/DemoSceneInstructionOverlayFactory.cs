using city.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the shared platform-aware instruction overlays used by the menu-visible rendering demo scenes.
    /// </summary>
    public sealed class DemoSceneInstructionOverlayFactory {
        /// <summary>
        /// Fixed font scale used by Nintendo DS instruction labels so the rendering demo bottom-screen guidance matches the compact physics-scene presentation.
        /// </summary>
        const float NintendoDsInstructionFontScale = 1.6f;

        /// <summary>
        /// Fixed desktop and console layer mask used by generated instruction overlays so showcase cameras can render them.
        /// </summary>
        const ushort DesktopOverlayLayerMask = EditorLayerMasks.SceneObjects;

        /// <summary>
        /// Fixed Nintendo DS runtime layer mask used by generated bottom-screen instruction overlays.
        /// </summary>
        const ushort NintendoDsOverlayLayerMask = 0b0000000000000001;

        /// <summary>
        /// Fixed drawable layer mask used by overlay render components across all platforms.
        /// </summary>
        const byte OverlayDrawableLayerMask = 0b00000001;

        /// <summary>
        /// Fixed desktop reference viewport width used for desktop and console instruction overlay layout.
        /// </summary>
        const int DesktopViewportWidth = 1280;

        /// <summary>
        /// Fixed desktop reference viewport height used for desktop and console instruction overlay layout.
        /// </summary>
        const int DesktopViewportHeight = 720;

        /// <summary>
        /// Fixed Nintendo DS bottom-screen width used for DS instruction overlay layout.
        /// </summary>
        const int NintendoDsScreenWidth = 256;

        /// <summary>
        /// Fixed Nintendo DS bottom-screen height used for DS instruction overlay layout.
        /// </summary>
        const int NintendoDsScreenHeight = 192;

        /// <summary>
        /// Fixed desktop and console left offset used to anchor the shared instruction panel inside the reference viewport.
        /// </summary>
        const float DesktopInstructionPanelLeft = 24f;

        /// <summary>
        /// Fixed desktop and console top offset used to keep the larger shared instruction panel inside the reference viewport.
        /// </summary>
        const float DesktopInstructionPanelTop = 528f;

        /// <summary>
        /// Fixed desktop and console panel width used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const int DesktopInstructionPanelWidth = 300;

        /// <summary>
        /// Fixed desktop and console panel height used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const int DesktopInstructionPanelHeight = 150;

        /// <summary>
        /// Fixed desktop and console first-row top offset used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const float DesktopInstructionFirstRowTop = 21f;

        /// <summary>
        /// Fixed desktop and console second-row top offset used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const float DesktopInstructionSecondRowTop = 90f;

        /// <summary>
        /// Fixed desktop and console label font scale used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const float DesktopInstructionLabelFontScale = 1.73f;

        /// <summary>
        /// Fixed desktop and console horizontal offset used for the shared icon host after the readability scale-up pass.
        /// </summary>
        const float DesktopInstructionIconLeft = 24f;

        /// <summary>
        /// Fixed desktop and console horizontal offset used for the shared instruction label after the readability scale-up pass.
        /// </summary>
        const float DesktopInstructionTextLeft = 112f;

        /// <summary>
        /// Fixed desktop and console vertical nudge used to visually center the larger labels against the shared icon rows.
        /// </summary>
        const float DesktopInstructionRotateTextTopAdjustment = -9f;

        /// <summary>
        /// Fixed desktop and console vertical nudge used to keep the toggle-light label aligned against the shoulder-button icon row.
        /// </summary>
        const float DesktopInstructionToggleTextTopAdjustment = -10f;

        /// <summary>
        /// Fixed desktop and console label width used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const int DesktopInstructionTextWidth = 140;

        /// <summary>
        /// Fixed desktop and console label height used by the shared instruction overlay after the readability scale-up pass.
        /// </summary>
        const int DesktopInstructionTextHeight = 28;

        /// <summary>
        /// Fixed desktop and console D-pad icon size used for the shared rotate-camera row after the readability scale-up pass.
        /// </summary>
        static readonly int2 DesktopInstructionDpadIconSize = new int2(48, 48);

        /// <summary>
        /// Fixed desktop and console Xbox 360 right-shoulder icon size used for the shared toggle-light row after the readability scale-up pass.
        /// </summary>
        static readonly int2 DesktopInstructionXbox360ShoulderIconSize = new int2(78, 45);

        /// <summary>
        /// Fixed desktop and console PS2 right-shoulder icon size used for the shared toggle-light row after the readability scale-up pass.
        /// </summary>
        static readonly int2 DesktopInstructionPs2ShoulderIconSize = new int2(65, 48);

        /// <summary>
        /// Fixed desktop and console Switch right-shoulder icon size used for the shared toggle-light row after the readability scale-up pass.
        /// </summary>
        static readonly int2 DesktopInstructionSwitchShoulderIconSize = new int2(89, 41);

        /// <summary>
        /// Fixed Nintendo DS bottom-screen panel height used after the readability scale-up pass.
        /// </summary>
        const int NintendoDsInstructionPanelHeight = 72;

        /// <summary>
        /// Fixed Nintendo DS icon left offset used after the readability scale-up pass.
        /// </summary>
        const float NintendoDsInstructionIconLeft = 12f;

        /// <summary>
        /// Fixed Nintendo DS text left offset used after the readability scale-up pass.
        /// </summary>
        const float NintendoDsInstructionTextLeft = 60f;

        /// <summary>
        /// Fixed Nintendo DS text top adjustment used to center the larger labels against their control icons.
        /// </summary>
        const float NintendoDsInstructionTextTopAdjustment = -2f;

        /// <summary>
        /// Fixed Nintendo DS label width used after the readability scale-up pass.
        /// </summary>
        const int NintendoDsInstructionTextWidth = 168;

        /// <summary>
        /// Fixed Nintendo DS label height used after the readability scale-up pass.
        /// </summary>
        const int NintendoDsInstructionTextHeight = 22;

        /// <summary>
        /// Stable project-relative texture path used for the Xbox 360 D-pad instruction icon.
        /// </summary>
        const string Xbox360DpadTexturePath = "Images/Instructions/Controls/xbox360_dpad.png";

        /// <summary>
        /// Stable project-relative texture path used for the Xbox 360 right-shoulder instruction icon.
        /// </summary>
        const string Xbox360RightShoulderTexturePath = "Images/Instructions/Controls/xbox360_rb.png";

        /// <summary>
        /// Stable project-relative texture path used for the PS2 D-pad instruction icon.
        /// </summary>
        const string Ps2DpadTexturePath = "Images/Instructions/Controls/ps2_dpad.png";

        /// <summary>
        /// Stable project-relative texture path used for the PS2 right-shoulder instruction icon.
        /// </summary>
        const string Ps2RightShoulderTexturePath = "Images/Instructions/Controls/ps2_r1.png";

        /// <summary>
        /// Stable project-relative texture path used for the Switch D-pad instruction icon.
        /// </summary>
        const string SwitchDpadTexturePath = "Images/Instructions/Controls/switch_dpad.png";

        /// <summary>
        /// Stable project-relative texture path used for the Switch right-shoulder instruction icon.
        /// </summary>
        const string SwitchRightShoulderTexturePath = "Images/Instructions/Controls/switch_r.png";

        /// <summary>
        /// Creates the shared desktop and console instruction overlay as one screen-bound root entity.
        /// </summary>
        /// <param name="font">Font used for the rendered instruction labels.</param>
        /// <returns>Screen-bound overlay root entity.</returns>
        public Entity CreateDesktopInstructionOverlayRoot(FontAsset font) {
            if (font == null) {
                throw new ArgumentNullException(nameof(font));
            }

            Entity viewportRootEntity = Core.Instance.EntityFactory.Create("DemoSceneInstructionViewport");
            viewportRootEntity.LayerMask = DesktopOverlayLayerMask;
            viewportRootEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(DesktopViewportWidth, DesktopViewportHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = DesktopViewportWidth,
                ReferenceHeight = DesktopViewportHeight
            });

            Entity panelEntity = Core.Instance.EntityFactory.CreateChild(viewportRootEntity, "DemoSceneInstructionPanel");
            panelEntity.LocalPosition = new float3(DesktopInstructionPanelLeft, DesktopInstructionPanelTop, 0f);
            panelEntity.LayerMask = DesktopOverlayLayerMask;
            panelEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(DesktopInstructionPanelWidth, DesktopInstructionPanelHeight),
                Radius = 8f,
                BorderThickness = 2f,
                FillColor = new byte4(20, 24, 32, 224),
                BorderColor = new byte4(120, 140, 170, 255),
                RenderOrder2D = 200,
                LayerMask = OverlayDrawableLayerMask
            });

            CreateDesktopInstructionRow(panelEntity, font, "RotateIconSet", "Rotate", DesktopInstructionFirstRowTop, DesktopInstructionRotateTextTopAdjustment, Xbox360DpadTexturePath, DesktopInstructionDpadIconSize, Ps2DpadTexturePath, DesktopInstructionDpadIconSize, SwitchDpadTexturePath, DesktopInstructionDpadIconSize);
            CreateDesktopInstructionRow(panelEntity, font, "ToggleIconSet", "Light", DesktopInstructionSecondRowTop, DesktopInstructionToggleTextTopAdjustment, Xbox360RightShoulderTexturePath, DesktopInstructionXbox360ShoulderIconSize, Ps2RightShoulderTexturePath, DesktopInstructionPs2ShoulderIconSize, SwitchRightShoulderTexturePath, DesktopInstructionSwitchShoulderIconSize);
            return viewportRootEntity;
        }

        /// <summary>
        /// Builds the shared Nintendo DS bottom-screen instruction roots that accompany the rendering demo companion scenes.
        /// </summary>
        /// <param name="font">Font used for the rendered instruction labels.</param>
        /// <returns>Bottom-screen roots that should be attached beneath the DS viewport scaffold.</returns>
        public Entity[] CreateNintendoDsBottomInstructionRoots(FontAsset font) {
            if (font == null) {
                throw new ArgumentNullException(nameof(font));
            }

            Entity panelEntity = Core.Instance.EntityFactory.Create("DemoSceneNintendoDsInstructionPanel");
            panelEntity.LocalPosition = new float3(8f, 76f, 0f);
            panelEntity.LayerMask = NintendoDsOverlayLayerMask;
            panelEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(NintendoDsScreenWidth - 16, NintendoDsInstructionPanelHeight),
                Radius = 0f,
                BorderThickness = 2f,
                FillColor = new byte4(20, 24, 32, 224),
                BorderColor = new byte4(120, 140, 170, 255),
                RenderOrder2D = 210,
                LayerMask = OverlayDrawableLayerMask
            });

            CreateNintendoDsInstructionRow(panelEntity, font, "Rotate Camera", 10f, SwitchDpadTexturePath, new int2(29, 29));
            CreateNintendoDsInstructionRow(panelEntity, font, "Toggle Light", 40f, SwitchRightShoulderTexturePath, new int2(48, 22));
            return [panelEntity];
        }

        /// <summary>
        /// Creates one desktop or console instruction row with platform-specific icon groups and one shared label.
        /// </summary>
        /// <param name="panelEntity">Instruction panel that should own the row.</param>
        /// <param name="font">Font used for the row label.</param>
        /// <param name="iconSetEntityName">Stable entity name assigned to the icon-set host.</param>
        /// <param name="text">Row label text.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="textTopAdjustment">Desktop/shared vertical text adjustment for the row label.</param>
        /// <param name="xbox360TexturePath">Xbox 360 icon texture path.</param>
        /// <param name="xbox360Size">Xbox 360 icon size.</param>
        /// <param name="ps2TexturePath">PS2 icon texture path.</param>
        /// <param name="ps2Size">PS2 icon size.</param>
        /// <param name="switchTexturePath">Switch icon texture path.</param>
        /// <param name="switchSize">Switch icon size.</param>
        void CreateDesktopInstructionRow(
            Entity panelEntity,
            FontAsset font,
            string iconSetEntityName,
            string text,
            float topOffset,
            float textTopAdjustment,
            string xbox360TexturePath,
            int2 xbox360Size,
            string ps2TexturePath,
            int2 ps2Size,
            string switchTexturePath,
            int2 switchSize) {
            if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(iconSetEntityName)) {
                throw new ArgumentException("Icon-set entity name must be provided.", nameof(iconSetEntityName));
            } else if (string.IsNullOrWhiteSpace(text)) {
                throw new ArgumentException("Instruction text must be provided.", nameof(text));
            }

            Entity iconSetEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, iconSetEntityName);
            iconSetEntity.LocalPosition = new float3(DesktopInstructionIconLeft, topOffset, 0.1f);
            iconSetEntity.LayerMask = DesktopOverlayLayerMask;
            iconSetEntity.AddComponent(new DemoScenePlatformInstructionIconSetComponent());
            CreatePlatformIconEntity(iconSetEntity, "Xbox360", xbox360TexturePath, xbox360Size, 201);
            CreatePlatformIconEntity(iconSetEntity, "Ps2", ps2TexturePath, ps2Size, 201);
            CreatePlatformIconEntity(iconSetEntity, "Switch", switchTexturePath, switchSize, 201);

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, iconSetEntityName + "Text");
            textEntity.LocalPosition = new float3(DesktopInstructionTextLeft, topOffset + textTopAdjustment, 0.1f);
            textEntity.LayerMask = DesktopOverlayLayerMask;
            TextComponent textComponent = new TextComponent {
                Text = text,
                Font = font,
                FontScale = DesktopInstructionLabelFontScale,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(DesktopInstructionTextWidth, DesktopInstructionTextHeight),
                RenderOrder2D = 202,
                LayerMask = OverlayDrawableLayerMask
            };
            textEntity.AddComponent(textComponent);
            ApplyFontReference(textEntity, textComponent);
        }

        /// <summary>
        /// Creates one compact Nintendo DS instruction row using the Switch icon set directly on the bottom screen.
        /// </summary>
        /// <param name="panelEntity">Instruction panel that should own the row.</param>
        /// <param name="font">Font used for the row label.</param>
        /// <param name="text">Row label text.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="texturePath">Switch icon texture path.</param>
        /// <param name="size">Switch icon size.</param>
        void CreateNintendoDsInstructionRow(Entity panelEntity, FontAsset font, string text, float topOffset, string texturePath, int2 size) {
            if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(text)) {
                throw new ArgumentException("Instruction text must be provided.", nameof(text));
            }

            Entity iconEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, text.Replace(" ", string.Empty) + "Icon");
            iconEntity.LocalPosition = new float3(NintendoDsInstructionIconLeft, topOffset, 0.1f);
            iconEntity.LayerMask = NintendoDsOverlayLayerMask;
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = size,
                RenderOrder2D = 211,
                LayerMask = OverlayDrawableLayerMask
            };
            iconEntity.AddComponent(spriteComponent);
            ApplyTextureReference(iconEntity, spriteComponent, texturePath);

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, text.Replace(" ", string.Empty) + "Text");
            textEntity.LocalPosition = new float3(NintendoDsInstructionTextLeft, topOffset + NintendoDsInstructionTextTopAdjustment, 0.1f);
            textEntity.LayerMask = NintendoDsOverlayLayerMask;
            TextComponent textComponent = new TextComponent {
                Text = text,
                Font = font,
                FontScale = NintendoDsInstructionFontScale,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(NintendoDsInstructionTextWidth, NintendoDsInstructionTextHeight),
                RenderOrder2D = 212,
                LayerMask = OverlayDrawableLayerMask
            };
            textEntity.AddComponent(textComponent);
            ApplyFontReference(textEntity, textComponent, DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

        /// <summary>
        /// Creates one platform-specific icon group entity that contains one instruction sprite.
        /// </summary>
        /// <param name="iconSetEntity">Icon-set host that should own the group.</param>
        /// <param name="entityName">Stable entity name assigned to the icon group.</param>
        /// <param name="texturePath">Project-relative icon texture path.</param>
        /// <param name="size">Rendered icon size.</param>
        /// <param name="renderOrder2D">Render order assigned to the icon sprite.</param>
        void CreatePlatformIconEntity(Entity iconSetEntity, string entityName, string texturePath, int2 size, byte renderOrder2D) {
            if (iconSetEntity == null) {
                throw new ArgumentNullException(nameof(iconSetEntity));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (string.IsNullOrWhiteSpace(texturePath)) {
                throw new ArgumentException("Texture path must be provided.", nameof(texturePath));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(iconSetEntity, entityName);
            entity.LayerMask = DesktopOverlayLayerMask;
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = size,
                RenderOrder2D = renderOrder2D,
                LayerMask = OverlayDrawableLayerMask
            };
            entity.AddComponent(spriteComponent);
            ApplyTextureReference(entity, spriteComponent, texturePath);
        }

        /// <summary>
        /// Stores the supplied editor font reference on the generated scene save state for the given text component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose font reference should be stored.</param>
        void ApplyFontReference(Entity entity, Component component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

        /// <summary>
        /// Stores the supplied generated Nintendo DS debug-font reference on the generated scene save state for the given text component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose font reference should be stored.</param>
        /// <param name="fontReference">Generated Nintendo DS debug-font reference.</param>
        void ApplyFontReference(Entity entity, Component component, SceneAssetReference fontReference) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (fontReference == null) {
                throw new ArgumentNullException(nameof(fontReference));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", fontReference);
        }

        /// <summary>
        /// Stores the supplied texture reference on the generated scene save state for the given sprite component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose texture reference should be stored.</param>
        /// <param name="texturePath">Project-relative texture path.</param>
        void ApplyTextureReference(Entity entity, Component component, string texturePath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(texturePath)) {
                throw new ArgumentException("Texture path must be provided.", nameof(texturePath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, TextureAssetScenePersistenceSupport.TextureReferenceName, BuildFileReference(texturePath));
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
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

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Builds one stable file-backed scene asset reference.
        /// </summary>
        /// <param name="relativePath">Project-relative asset path.</param>
        /// <returns>Stable file-backed scene asset reference.</returns>
        SceneAssetReference BuildFileReference(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            return global::helengine.SceneAssetReferenceFactory.CreateFileSystemTexture(relativePath);
        }
    }
}
