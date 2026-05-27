using city.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the shared platform-aware instruction overlays used by the menu-visible rendering demo scenes.
    /// </summary>
    public sealed class DemoSceneInstructionOverlayFactory {
        /// <summary>
        /// Fixed runtime layer mask used by generated instruction overlay drawables.
        /// </summary>
        const byte RuntimeLayerMask = 0b00000001;

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
        /// Attaches the shared desktop and console instruction overlay beneath the supplied showcase camera.
        /// </summary>
        /// <param name="cameraEntity">Camera entity that should own the viewport-bound instruction overlay.</param>
        /// <param name="font">Font used for the rendered instruction labels.</param>
        public void AttachDesktopInstructionOverlay(Entity cameraEntity, FontAsset font) {
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            }

            Entity viewportRootEntity = Core.Instance.EntityFactory.CreateChild(cameraEntity, "DemoSceneInstructionViewport");
            viewportRootEntity.LayerMask = RuntimeLayerMask;
            viewportRootEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(DesktopViewportWidth, DesktopViewportHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = DesktopViewportWidth,
                ReferenceHeight = DesktopViewportHeight
            });

            Entity panelEntity = Core.Instance.EntityFactory.CreateChild(viewportRootEntity, "DemoSceneInstructionPanel");
            panelEntity.LocalPosition = new float3(24f, 614f, 0f);
            panelEntity.LayerMask = RuntimeLayerMask;
            panelEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(292, 76),
                Radius = 8f,
                BorderThickness = 2f,
                FillColor = new byte4(20, 24, 32, 224),
                BorderColor = new byte4(120, 140, 170, 255),
                RenderOrder2D = 200,
                LayerMask = RuntimeLayerMask
            });

            CreateDesktopInstructionRow(panelEntity, font, "RotateIconSet", "Rotate Camera", 10f, Xbox360DpadTexturePath, new int2(24, 24), Ps2DpadTexturePath, new int2(24, 24), SwitchDpadTexturePath, new int2(24, 24));
            CreateDesktopInstructionRow(panelEntity, font, "ToggleIconSet", "Toggle Light", 42f, Xbox360RightShoulderTexturePath, new int2(38, 22), Ps2RightShoulderTexturePath, new int2(32, 24), SwitchRightShoulderTexturePath, new int2(44, 20));
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
            panelEntity.LayerMask = RuntimeLayerMask;
            panelEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(NintendoDsScreenWidth - 16, 54),
                Radius = 0f,
                BorderThickness = 2f,
                FillColor = new byte4(20, 24, 32, 224),
                BorderColor = new byte4(120, 140, 170, 255),
                RenderOrder2D = 210,
                LayerMask = RuntimeLayerMask
            });

            CreateNintendoDsInstructionRow(panelEntity, font, "Rotate Camera", 8f, SwitchDpadTexturePath, new int2(18, 18));
            CreateNintendoDsInstructionRow(panelEntity, font, "Toggle Light", 30f, SwitchRightShoulderTexturePath, new int2(30, 14));
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
            iconSetEntity.LocalPosition = new float3(14f, topOffset, 0.1f);
            iconSetEntity.LayerMask = RuntimeLayerMask;
            iconSetEntity.AddComponent(new DemoScenePlatformInstructionIconSetComponent());
            CreatePlatformIconEntity(iconSetEntity, "Xbox360", xbox360TexturePath, xbox360Size, 201);
            CreatePlatformIconEntity(iconSetEntity, "Ps2", ps2TexturePath, ps2Size, 201);
            CreatePlatformIconEntity(iconSetEntity, "Switch", switchTexturePath, switchSize, 201);

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, iconSetEntityName + "Text");
            textEntity.LocalPosition = new float3(58f, topOffset - 1f, 0.1f);
            textEntity.LayerMask = RuntimeLayerMask;
            TextComponent textComponent = new TextComponent {
                Text = text,
                Font = font,
                FontScale = 1.2f,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(220, 20),
                RenderOrder2D = 202,
                LayerMask = RuntimeLayerMask
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
            iconEntity.LocalPosition = new float3(10f, topOffset, 0.1f);
            iconEntity.LayerMask = RuntimeLayerMask;
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = size,
                RenderOrder2D = 211,
                LayerMask = RuntimeLayerMask
            };
            iconEntity.AddComponent(spriteComponent);
            ApplyTextureReference(iconEntity, spriteComponent, texturePath);

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, text.Replace(" ", string.Empty) + "Text");
            textEntity.LocalPosition = new float3(48f, topOffset - 1f, 0.1f);
            textEntity.LayerMask = RuntimeLayerMask;
            TextComponent textComponent = new TextComponent {
                Text = text,
                Font = font,
                FontScale = 0.7f,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(170, 16),
                RenderOrder2D = 212,
                LayerMask = RuntimeLayerMask
            };
            textEntity.AddComponent(textComponent);
            ApplyFontReference(textEntity, textComponent);
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
            entity.LayerMask = RuntimeLayerMask;
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = size,
                RenderOrder2D = renderOrder2D,
                LayerMask = RuntimeLayerMask
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

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = relativePath
            };
        }
    }
}
