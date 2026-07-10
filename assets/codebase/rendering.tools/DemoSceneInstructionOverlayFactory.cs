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
        /// Fixed desktop and console horizontal offset used for the primary camera icon slot.
        /// </summary>
        const float DesktopInstructionCameraPrimaryIconLeft = 20f;

        /// <summary>
        /// Fixed desktop and console horizontal offset used for the secondary camera icon slot.
        /// </summary>
        const float DesktopInstructionCameraSecondaryIconLeft = 72f;

        /// <summary>
        /// Fixed desktop and console horizontal offset used for the shared camera-row label.
        /// </summary>
        const float DesktopInstructionCameraTextLeft = 150f;

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
        /// Shared generated control-icon resolver used by desktop instruction rows.
        /// </summary>
        readonly GeneratedControlIconAssetResolver ControlIconResolver = new GeneratedControlIconAssetResolver();

        /// <summary>
        /// Editor API used to author per-platform sprite overrides on one shared icon entity.
        /// </summary>
        readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

        /// <summary>
        /// Raw generated control-icon binding plus the authored icon size used for one platform.
        /// </summary>
        readonly struct DesktopInstructionPlatformIconSpec {
            public DesktopInstructionPlatformIconSpec(string platformId, string controlId, int2 size) {
                PlatformId = platformId;
                ControlId = controlId;
                Size = size;
            }

            public string PlatformId { get; }
            public string ControlId { get; }
            public int2 Size { get; }
        }

        /// <summary>
        /// Raw generated control-icon binding plus the authored icon size and slot used for one camera row platform.
        /// </summary>
        readonly struct DesktopInstructionPlatformIconSlotSpec {
            public DesktopInstructionPlatformIconSlotSpec(string platformId, string controlId, int2 size, int slotIndex) {
                PlatformId = platformId;
                ControlId = controlId;
                Size = size;
                SlotIndex = slotIndex;
            }

            public string PlatformId { get; }
            public string ControlId { get; }
            public int2 Size { get; }
            public int SlotIndex { get; }
        }

        /// <summary>
        /// Shared camera-row icon bindings keyed by runtime platform and icon slot.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSlotSpec[] CameraIconSpecs = new[] {
            new DesktopInstructionPlatformIconSlotSpec("windows", "wasd", new int2(76, 52), 0),
            new DesktopInstructionPlatformIconSlotSpec("win32", "wasd", new int2(76, 52), 0),
            new DesktopInstructionPlatformIconSlotSpec("xbox360", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("xbox360", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("switch", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("switch", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("gamecube", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("gamecube", "control_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("wii", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("wii", "stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("ds", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("3ds", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("3ds", "circle_pad", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("psp", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("psp", "analog", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("ps2", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("ps2", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("psvita", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("psvita", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("n64", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("n64", "control_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("dreamcast", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("dreamcast", "analog", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("ps1", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("ps1", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("ps3", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("ps3", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("xbox", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("xbox", "left_stick", new int2(48, 48), 1),
            new DesktopInstructionPlatformIconSlotSpec("steamdeck", "dpad", new int2(48, 48), 0),
            new DesktopInstructionPlatformIconSlotSpec("steamdeck", "left_stick", new int2(48, 48), 1)
        };

        /// <summary>
        /// Shared light-toggle-row icon bindings keyed by runtime platform.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSpec[] LightIconSpecs = new[] {
            new DesktopInstructionPlatformIconSpec("windows", "key_l", new int2(46, 46)),
            new DesktopInstructionPlatformIconSpec("win32", "key_l", new int2(46, 46)),
            new DesktopInstructionPlatformIconSpec("xbox360", "rb", new int2(78, 45)),
            new DesktopInstructionPlatformIconSpec("switch", "r", new int2(89, 41)),
            new DesktopInstructionPlatformIconSpec("gamecube", "r", new int2(82, 43)),
            new DesktopInstructionPlatformIconSpec("wii", "b", new int2(58, 46)),
            new DesktopInstructionPlatformIconSpec("ds", "r", new int2(74, 42)),
            new DesktopInstructionPlatformIconSpec("3ds", "r", new int2(74, 42)),
            new DesktopInstructionPlatformIconSpec("psp", "r1", new int2(74, 42)),
            new DesktopInstructionPlatformIconSpec("ps2", "r1", new int2(65, 48)),
            new DesktopInstructionPlatformIconSpec("psvita", "r1", new int2(74, 42)),
            new DesktopInstructionPlatformIconSpec("n64", "r", new int2(70, 42)),
            new DesktopInstructionPlatformIconSpec("dreamcast", "r", new int2(70, 42)),
            new DesktopInstructionPlatformIconSpec("ps1", "r1", new int2(65, 48)),
            new DesktopInstructionPlatformIconSpec("ps3", "r1", new int2(65, 48)),
            new DesktopInstructionPlatformIconSpec("xbox", "rb", new int2(78, 45)),
            new DesktopInstructionPlatformIconSpec("steamdeck", "r1", new int2(78, 45))
        };

        /// <summary>
        /// Creates the shared desktop and console instruction overlay as one screen-bound root entity.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="font">Font used for the rendered instruction labels.</param>
        /// <returns>Screen-bound overlay root entity.</returns>
        public Entity CreateDesktopInstructionOverlayRoot(string projectRootPath, FontAsset font) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (font == null) {
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

            CreateDesktopInstructionCameraRow(panelEntity, projectRootPath, font, "Camera", DesktopInstructionFirstRowTop, DesktopInstructionRotateTextTopAdjustment);
            CreateDesktopInstructionRow(panelEntity, projectRootPath, font, "LightIcon", "Light", DesktopInstructionSecondRowTop, DesktopInstructionToggleTextTopAdjustment, LightIconSpecs);
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

            return Array.Empty<Entity>();
        }

        /// <summary>
        /// Creates the shared desktop or console camera row with one or two icon slots depending on the active platform.
        /// </summary>
        /// <param name="panelEntity">Instruction panel that should own the row.</param>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="font">Font used for the row label.</param>
        /// <param name="text">Row label text.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="textTopAdjustment">Desktop/shared vertical text adjustment for the row label.</param>
        void CreateDesktopInstructionCameraRow(
            Entity panelEntity,
            string projectRootPath,
            FontAsset font,
            string text,
            float topOffset,
            float textTopAdjustment) {
            if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(text)) {
                throw new ArgumentException("Instruction text must be provided.", nameof(text));
            }

            CreateInstructionIconEntity(projectRootPath, panelEntity, "CameraIconPrimary", DesktopInstructionCameraPrimaryIconLeft, topOffset, CameraIconSpecs, 0, 201);
            CreateInstructionIconEntity(projectRootPath, panelEntity, "CameraIconSecondary", DesktopInstructionCameraSecondaryIconLeft, topOffset, CameraIconSpecs, 1, 201);

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, "CameraText");
            textEntity.LocalPosition = new float3(DesktopInstructionCameraTextLeft, topOffset + textTopAdjustment, 0.1f);
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
        /// Creates one desktop or console instruction row with one shared icon entity and per-platform overrides.
        /// </summary>
        /// <param name="panelEntity">Instruction panel that should own the row.</param>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="font">Font used for the row label.</param>
        /// <param name="iconEntityName">Stable entity name assigned to the icon host.</param>
        /// <param name="text">Row label text.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="textTopAdjustment">Desktop/shared vertical text adjustment for the row label.</param>
        /// <param name="specs">Per-platform raw icon bindings used by the row.</param>
        void CreateDesktopInstructionRow(
            Entity panelEntity,
            string projectRootPath,
            FontAsset font,
            string iconEntityName,
            string text,
            float topOffset,
            float textTopAdjustment,
            DesktopInstructionPlatformIconSpec[] specs) {
            if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(iconEntityName)) {
                throw new ArgumentException("Icon entity name must be provided.", nameof(iconEntityName));
            } else if (string.IsNullOrWhiteSpace(text)) {
                throw new ArgumentException("Instruction text must be provided.", nameof(text));
            } else if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            }

            CreateInstructionIconEntity(projectRootPath, panelEntity, iconEntityName, DesktopInstructionIconLeft, topOffset, specs, 201);

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, iconEntityName + "Text");
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
        /// Creates the shared desktop instruction icon entity and persists per-platform sprite overrides.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="panelEntity">Instruction panel that should own the icon entity.</param>
        /// <param name="entityName">Stable entity name assigned to the icon entity.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="specs">Per-platform raw icon bindings used by the row.</param>
        /// <param name="renderOrder2D">Render order assigned to the icon sprite.</param>
        void CreateInstructionIconEntity(
            string projectRootPath,
            Entity panelEntity,
            string entityName,
            float leftOffset,
            float topOffset,
            DesktopInstructionPlatformIconSpec[] specs,
            byte renderOrder2D) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(panelEntity, entityName);
            entity.LocalPosition = new float3(leftOffset, topOffset, 0.1f);
            entity.LayerMask = DesktopOverlayLayerMask;
            DesktopInstructionPlatformIconSpec commonSpec = FindRequiredCommonSpec(specs, "windows");
            ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(projectRootPath, commonSpec.PlatformId, commonSpec.ControlId);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = commonIcon.FitDisplaySizeWithin(commonSpec.Size),
                SourceRect = commonIcon.SourceRect,
                RenderOrder2D = renderOrder2D,
                LayerMask = OverlayDrawableLayerMask
            };
            entity.AddComponent(spriteComponent);
            ApplyTextureReference(entity, spriteComponent, commonIcon.SourcePngRelativePath);

            for (int index = 0; index < specs.Length; index++) {
                DesktopInstructionPlatformIconSpec spec = specs[index];
                if (string.Equals(spec.PlatformId, commonSpec.PlatformId, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                ApplyPlatformSpriteOverride(projectRootPath, entity, spriteComponent, spec);
            }
        }

        /// <summary>
        /// Creates one shared desktop camera icon entity for the requested icon slot and persists per-platform sprite overrides.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="panelEntity">Instruction panel that should own the icon entity.</param>
        /// <param name="entityName">Stable entity name assigned to the icon entity.</param>
        /// <param name="leftOffset">Horizontal offset within the panel.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="specs">Per-platform raw icon bindings used by the row.</param>
        /// <param name="slotIndex">Requested camera icon slot.</param>
        /// <param name="renderOrder2D">Render order assigned to the icon sprite.</param>
        void CreateInstructionIconEntity(
            string projectRootPath,
            Entity panelEntity,
            string entityName,
            float leftOffset,
            float topOffset,
            DesktopInstructionPlatformIconSlotSpec[] specs,
            int slotIndex,
            byte renderOrder2D) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            } else if (slotIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(panelEntity, entityName);
            entity.LocalPosition = new float3(leftOffset, topOffset, 0.1f);
            entity.LayerMask = DesktopOverlayLayerMask;

            bool hasWindowsCommonSpec = TryFindPlatformSlotSpec(specs, "windows", slotIndex, out DesktopInstructionPlatformIconSlotSpec commonSpec);
            if (!hasWindowsCommonSpec) {
                commonSpec = FindRequiredSlotSpec(specs, slotIndex);
            }

            bool hideByDefault = !hasWindowsCommonSpec;
            ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(projectRootPath, commonSpec.PlatformId, commonSpec.ControlId);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = commonIcon.FitDisplaySizeWithin(commonSpec.Size),
                SourceRect = commonIcon.SourceRect,
                Color = hideByDefault ? new byte4(255, 255, 255, 0) : new byte4(255, 255, 255, 255),
                RenderOrder2D = renderOrder2D,
                LayerMask = OverlayDrawableLayerMask
            };
            entity.AddComponent(spriteComponent);
            ApplyTextureReference(entity, spriteComponent, commonIcon.SourcePngRelativePath);

            for (int index = 0; index < specs.Length; index++) {
                DesktopInstructionPlatformIconSlotSpec spec = specs[index];
                if (spec.SlotIndex != slotIndex) {
                    continue;
                }
                if (!hideByDefault && string.Equals(spec.PlatformId, commonSpec.PlatformId, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                ApplyPlatformSpriteOverride(projectRootPath, entity, spriteComponent, spec);
            }
        }

        /// <summary>
        /// Persists one platform-specific sprite override for the shared instruction icon entity.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="entity">Icon entity that owns the shared sprite component.</param>
        /// <param name="commonComponent">Common shared sprite component.</param>
        /// <param name="spec">Platform-specific raw icon binding plus authored size.</param>
        void ApplyPlatformSpriteOverride(string projectRootPath, Entity entity, SpriteComponent commonComponent, DesktopInstructionPlatformIconSpec spec) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (commonComponent == null) {
                throw new ArgumentNullException(nameof(commonComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            SpriteComponent overrideComponent = (SpriteComponent)PlatformEditingService.EnsurePlatformOverrideComponent(commonComponent, saveComponent, spec.PlatformId);
            ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(projectRootPath, spec.PlatformId, spec.ControlId);
            overrideComponent.Size = resolvedIcon.FitDisplaySizeWithin(spec.Size);
            PlatformEditingService.MarkPropertyOverride(commonComponent, saveComponent, spec.PlatformId, nameof(SpriteComponent.Size));
            overrideComponent.SourceRect = resolvedIcon.SourceRect;
            PlatformEditingService.MarkPropertyOverride(commonComponent, saveComponent, spec.PlatformId, nameof(SpriteComponent.SourceRect));
            PlatformEditingService.StoreAssetReference(
                commonComponent,
                overrideComponent,
                saveComponent,
                spec.PlatformId,
                TextureAssetScenePersistenceSupport.TextureReferenceName,
                BuildFileReference(resolvedIcon.SourcePngRelativePath));
            PlatformEditingService.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, spec.PlatformId);
        }

        /// <summary>
        /// Persists one platform-specific camera-slot sprite override for the shared instruction icon entity.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="entity">Icon entity that owns the shared sprite component.</param>
        /// <param name="commonComponent">Common shared sprite component.</param>
        /// <param name="spec">Platform-specific raw icon binding plus authored size.</param>
        void ApplyPlatformSpriteOverride(string projectRootPath, Entity entity, SpriteComponent commonComponent, DesktopInstructionPlatformIconSlotSpec spec) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (commonComponent == null) {
                throw new ArgumentNullException(nameof(commonComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            SpriteComponent overrideComponent = (SpriteComponent)PlatformEditingService.EnsurePlatformOverrideComponent(commonComponent, saveComponent, spec.PlatformId);
            ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(projectRootPath, spec.PlatformId, spec.ControlId);
            overrideComponent.Size = resolvedIcon.FitDisplaySizeWithin(spec.Size);
            PlatformEditingService.MarkPropertyOverride(commonComponent, saveComponent, spec.PlatformId, nameof(SpriteComponent.Size));
            overrideComponent.SourceRect = resolvedIcon.SourceRect;
            PlatformEditingService.MarkPropertyOverride(commonComponent, saveComponent, spec.PlatformId, nameof(SpriteComponent.SourceRect));
            overrideComponent.Color = new byte4(255, 255, 255, 255);
            PlatformEditingService.MarkPropertyOverride(commonComponent, saveComponent, spec.PlatformId, nameof(SpriteComponent.Color));
            PlatformEditingService.StoreAssetReference(
                commonComponent,
                overrideComponent,
                saveComponent,
                spec.PlatformId,
                TextureAssetScenePersistenceSupport.TextureReferenceName,
                BuildFileReference(resolvedIcon.SourcePngRelativePath));
            PlatformEditingService.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, spec.PlatformId);
        }

        /// <summary>
        /// Resolves the common authored icon binding used as the shared component baseline.
        /// </summary>
        /// <param name="specs">Per-platform row bindings.</param>
        /// <param name="platformId">Platform id that should supply the shared common baseline.</param>
        /// <returns>Matching platform icon spec.</returns>
        static DesktopInstructionPlatformIconSpec FindRequiredCommonSpec(DesktopInstructionPlatformIconSpec[] specs, string platformId) {
            if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            } else if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            for (int index = 0; index < specs.Length; index++) {
                if (string.Equals(specs[index].PlatformId, platformId, StringComparison.OrdinalIgnoreCase)) {
                    return specs[index];
                }
            }

            throw new InvalidOperationException($"Common prompt icon spec '{platformId}' was not found.");
        }

        /// <summary>
        /// Attempts to resolve one platform-specific camera icon slot spec.
        /// </summary>
        /// <param name="specs">Per-platform row bindings.</param>
        /// <param name="platformId">Platform id that should supply the common baseline.</param>
        /// <param name="slotIndex">Requested icon slot.</param>
        /// <param name="spec">Resolved slot spec when one exists.</param>
        /// <returns>True when a matching slot spec was found.</returns>
        static bool TryFindPlatformSlotSpec(DesktopInstructionPlatformIconSlotSpec[] specs, string platformId, int slotIndex, out DesktopInstructionPlatformIconSlotSpec spec) {
            if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            } else if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            } else if (slotIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            for (int index = 0; index < specs.Length; index++) {
                if (specs[index].SlotIndex == slotIndex &&
                    string.Equals(specs[index].PlatformId, platformId, StringComparison.OrdinalIgnoreCase)) {
                    spec = specs[index];
                    return true;
                }
            }

            spec = default;
            return false;
        }

        /// <summary>
        /// Resolves one required camera icon slot spec.
        /// </summary>
        /// <param name="specs">Per-platform row bindings.</param>
        /// <param name="slotIndex">Requested icon slot.</param>
        /// <returns>Matching slot spec.</returns>
        static DesktopInstructionPlatformIconSlotSpec FindRequiredSlotSpec(DesktopInstructionPlatformIconSlotSpec[] specs, int slotIndex) {
            if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            } else if (slotIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            for (int index = 0; index < specs.Length; index++) {
                if (specs[index].SlotIndex == slotIndex) {
                    return specs[index];
                }
            }

            throw new InvalidOperationException($"Common prompt icon slot '{slotIndex}' was not found.");
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
