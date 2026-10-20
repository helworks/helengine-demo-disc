using DemoDisc.rendering;
using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Builds the shared platform-aware instruction overlays used by the menu-visible rendering demo scenes.
    /// </summary>
    public sealed class DemoSceneInstructionOverlayFactory {
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
        /// Side of the square slot every instruction icon is fitted into, in reference pixels.
        /// </summary>
        public const int InstructionIconSize = 96;

        /// <summary>
        /// Inner margin between the panel border and its icons, rows and swatch.
        /// </summary>
        const int InstructionPanelPadding = 16;

        /// <summary>
        /// Vertical gap between the camera row and the light row.
        /// </summary>
        const int InstructionRowGap = 12;

        /// <summary>
        /// Horizontal gap between the two camera icons.
        /// </summary>
        const int InstructionIconGap = 8;

        /// <summary>
        /// Horizontal gap between the last icon of a row and its label.
        /// </summary>
        const int InstructionLabelGap = 16;

        /// <summary>
        /// Fixed desktop and console left offset used to anchor the shared instruction panel inside the reference viewport.
        /// </summary>
        public const float DesktopInstructionPanelLeft = 24f;

        /// <summary>
        /// Panel width: the label column plus the widest label ("Camera" spans up to 180 reference pixels on 240-line screens) and padding.
        /// </summary>
        const int DesktopInstructionPanelWidth = 440;

        /// <summary>
        /// Panel height: two icon rows, the gap between them and padding.
        /// </summary>
        const int DesktopInstructionPanelHeight = 2 * InstructionPanelPadding + 2 * InstructionIconSize + InstructionRowGap;

        /// <summary>
        /// Fixed desktop and console top offset; the panel bottom stays where the smaller panel used to end (678).
        /// </summary>
        public const float DesktopInstructionPanelTop = 678 - DesktopInstructionPanelHeight;

        /// <summary>
        /// Uniform scale the micro SD group applies to the instruction panel so it stays readable on 240-line screens.
        /// </summary>
        public const float MicroSdInstructionPanelScale = 1.5f;

        /// <summary>Reference-sized N64 instructions; native text already retains its readable pixel size.</summary>
        public const float Nintendo64InstructionPanelScale = 1f;

        /// <summary>Keeps the compact N64 panel at the existing Micro SD bottom margin.</summary>
        public const float Nintendo64InstructionPanelTop = DesktopViewportHeight - 24 - DesktopInstructionPanelHeight * Nintendo64InstructionPanelScale;

        const float Nintendo64InstructionLabelTopInRow = (InstructionIconSize / 2 * Nintendo64InstructionPanelScale - DemoDiscSceneUiFont.ReferenceCapCenter) / Nintendo64InstructionPanelScale;

        /// <summary>
        /// Micro SD panel top that keeps the scaled panel 24 reference pixels above the bottom of the screen.
        /// </summary>
        public const float MicroSdInstructionPanelTop = DesktopViewportHeight - 24 - DesktopInstructionPanelHeight * MicroSdInstructionPanelScale;

        /// <summary>
        /// Top offset of the camera row inside the panel.
        /// </summary>
        const float DesktopInstructionFirstRowTop = InstructionPanelPadding;

        /// <summary>
        /// Top offset of the light row inside the panel.
        /// </summary>
        const float DesktopInstructionSecondRowTop = InstructionPanelPadding + InstructionIconSize + InstructionRowGap;

        /// <summary>
        /// Horizontal offset of the single light icon, aligned with the first camera icon.
        /// </summary>
        const float DesktopInstructionIconLeft = InstructionPanelPadding;

        /// <summary>
        /// Horizontal offset of the primary camera icon slot.
        /// </summary>
        const float DesktopInstructionCameraPrimaryIconLeft = InstructionPanelPadding;

        /// <summary>
        /// Horizontal offset of the secondary camera icon slot.
        /// </summary>
        const float DesktopInstructionCameraSecondaryIconLeft = InstructionPanelPadding + InstructionIconSize + InstructionIconGap;

        /// <summary>
        /// Horizontal offset shared by both labels, so "Camera" and "Light" start in one column after the camera icons.
        /// </summary>
        const float DesktopInstructionLabelLeft = DesktopInstructionCameraSecondaryIconLeft + InstructionIconSize + InstructionLabelGap;

        /// <summary>
        /// Offset from a row top to its label box top that puts the middle of the capitals on the middle of the icon slot.
        /// </summary>
        const float DesktopInstructionLabelTopInRow = InstructionIconSize / 2 - DemoDiscSceneUiFont.ReferenceCapCenter;

        /// <summary>
        /// Micro SD counterpart of the label offset. Labels draw at native size, so the panel scale moves them but does not grow
        /// them; the box top is placed so the unscaled cap middle still lands on the scaled icon middle.
        /// </summary>
        const float MicroSdInstructionLabelTopInRow = (InstructionIconSize / 2 * MicroSdInstructionPanelScale - DemoDiscSceneUiFont.ReferenceCapCenter) / MicroSdInstructionPanelScale;

        /// <summary>
        /// Label box width, wide enough for "Camera" on every framebuffer and still inside the panel.
        /// </summary>
        const int DesktopInstructionTextWidth = DesktopInstructionPanelWidth - (int)DesktopInstructionLabelLeft - InstructionPanelPadding;

        /// <summary>
        /// Label box height, one scene-font text row.
        /// </summary>
        const int DesktopInstructionTextHeight = DemoDiscSceneUiFont.ReferenceRowHeight;

        /// <summary>
        /// Side of the light-state swatch that sits in the light row of the panel.
        /// </summary>
        public const int LightSwatchSize = 40;

        /// <summary>
        /// Panel-space left offset of the light-state swatch, right-aligned in the light row after the "Light" label.
        /// </summary>
        public const float LightSwatchPanelLeft = DesktopInstructionPanelWidth - InstructionPanelPadding - LightSwatchSize;

        /// <summary>
        /// Panel-space top offset of the light-state swatch, vertically centered on the light icon slot.
        /// </summary>
        public const float LightSwatchPanelTop = DesktopInstructionSecondRowTop + (InstructionIconSize - LightSwatchSize) / 2;

        /// <summary>
        /// Icon slot used by every camera and light icon binding.
        /// </summary>
        static readonly int2 InstructionIconSlot = new int2(InstructionIconSize, InstructionIconSize);

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
        /// Fixed Nintendo DS label height: one scene-font row on the 192-pixel-high DS screen (11 pixel font).
        /// </summary>
        const int NintendoDsInstructionTextHeight = 16;

        /// <summary>
        /// Shared generated control-icon resolver used by desktop instruction rows.
        /// </summary>
        readonly GeneratedControlIconAssetResolver ControlIconResolver = new GeneratedControlIconAssetResolver();

        /// <summary>
        /// Host-owned asset-authoring capability used to resolve generated control icons.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Initializes one instruction overlay factory with the host asset-authoring capability.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to resolve generated icon assets.</param>
        public DemoSceneInstructionOverlayFactory(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Editor API used to author per-platform sprite overrides on one shared icon entity.
        /// </summary>
        readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

        /// <summary>
        /// Raw generated control-icon binding plus the authored icon size used for one platform.
        /// </summary>
        readonly struct DesktopInstructionPlatformIconSpec {
            public DesktopInstructionPlatformIconSpec(string platformId, string controlId, int2 size, string sourcePlatformId = null) {
                PlatformId = platformId;
                ControlId = controlId;
                Size = size;
                SourcePlatformId = string.IsNullOrWhiteSpace(sourcePlatformId) ? platformId : sourcePlatformId;
            }

            public string PlatformId { get; }
            public string ControlId { get; }
            public int2 Size { get; }
            public string SourcePlatformId { get; }
        }

        /// <summary>
        /// Raw generated control-icon binding plus the authored icon size and slot used for one camera row platform.
        /// </summary>
        readonly struct DesktopInstructionPlatformIconSlotSpec {
            public DesktopInstructionPlatformIconSlotSpec(string platformId, string controlId, int2 size, int slotIndex, string sourcePlatformId = null) {
                PlatformId = platformId;
                ControlId = controlId;
                Size = size;
                SlotIndex = slotIndex;
                SourcePlatformId = string.IsNullOrWhiteSpace(sourcePlatformId) ? platformId : sourcePlatformId;
            }

            public string PlatformId { get; }
            public string ControlId { get; }
            public int2 Size { get; }
            public int SlotIndex { get; }
            public string SourcePlatformId { get; }
        }

        /// <summary>
        /// Shared camera-row icon bindings keyed by runtime platform and icon slot.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSlotSpec[] CameraIconSpecs = new[] {
            new DesktopInstructionPlatformIconSlotSpec("windows", "dpad", InstructionIconSlot, 0, "xbox360"),
            new DesktopInstructionPlatformIconSlotSpec("windows", "left_stick", InstructionIconSlot, 1, "xbox360"),
            new DesktopInstructionPlatformIconSlotSpec("win32", "dpad", InstructionIconSlot, 0, "xbox360"),
            new DesktopInstructionPlatformIconSlotSpec("win32", "left_stick", InstructionIconSlot, 1, "xbox360"),
            new DesktopInstructionPlatformIconSlotSpec("xbox360", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("xbox360", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("switch", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("switch", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("gamecube", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("gamecube", "control_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("wii", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("wii", "stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("ds", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("3ds", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("3ds", "circle_pad", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("psp", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("psp", "analog", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("ps2", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("ps2", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("psvita", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("psvita", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("n64", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("n64", "control_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("dreamcast", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("dreamcast", "analog", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("ps1", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("ps1", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("ps3", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("ps3", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("xbox", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("xbox", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("steamdeck", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("steamdeck", "left_stick", InstructionIconSlot, 1)
        };

        /// <summary>
        /// Shared light-toggle-row icon bindings keyed by runtime platform.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSpec[] LightIconSpecs = new[] {
            new DesktopInstructionPlatformIconSpec("windows", "y", InstructionIconSlot, "xbox360"),
            new DesktopInstructionPlatformIconSpec("win32", "y", InstructionIconSlot, "xbox360"),
            new DesktopInstructionPlatformIconSpec("xbox360", "y", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("switch", "x", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("gamecube", "y", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("wii", "2", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("ds", "x", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("3ds", "y", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("psp", "triangle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("ps2", "circle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("psvita", "triangle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("n64", "face_cluster", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("dreamcast", "y", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("ps1", "triangle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("ps3", "triangle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("xbox", "y", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("steamdeck", "y", InstructionIconSlot)
        };

        /// <summary>
        /// Console-only camera D-pad bindings used by the reusable camera/light Blueprint.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSpec[] ConsoleCameraIconSpecs = new[] {
            new DesktopInstructionPlatformIconSpec("ps2", "dpad", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("gamecube", "dpad", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("wii", "dpad", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("switch", "dpad", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("wiiu", "dpad", InstructionIconSlot)
        };

        /// <summary>
        /// Console camera icon bindings for both the directional pad and the primary camera stick.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSlotSpec[] ConsoleCameraIconSlotSpecs = new[] {
            new DesktopInstructionPlatformIconSlotSpec("ps2", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("ps2", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("n64", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("n64", "control_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("ps1", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("ps1", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("gamecube", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("gamecube", "control_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("wii", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("wii", "stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("switch", "dpad", InstructionIconSlot, 0),
            new DesktopInstructionPlatformIconSlotSpec("switch", "left_stick", InstructionIconSlot, 1),
            new DesktopInstructionPlatformIconSlotSpec("wiiu", "dpad", InstructionIconSlot, 0, "wii"),
            new DesktopInstructionPlatformIconSlotSpec("wiiu", "stick", InstructionIconSlot, 1, "wii")
        };

        /// <summary>
        /// Console-only light-toggle bindings used by the reusable camera/light Blueprint.
        /// </summary>
        static readonly DesktopInstructionPlatformIconSpec[] ConsoleLightIconSpecs = new[] {
            new DesktopInstructionPlatformIconSpec("ps2", "circle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("ps3", "triangle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("n64", "c_cluster", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("ps1", "triangle", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("gamecube", "y", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("wii", "2", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("switch", "x", InstructionIconSlot),
            new DesktopInstructionPlatformIconSpec("wiiu", "y", InstructionIconSlot, "xbox360")
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

            Entity viewportRootEntity = CreateInstructionViewportRoot("DemoSceneInstructionViewport", "DemoSceneInstructionPanel", out Entity panelEntity);
            // Micro SD uses the console Blueprint; do not draw a second panel over it.
            EntitySaveComponent viewportSave = FindRequiredEntitySaveComponent(viewportRootEntity);
            viewportSave.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            viewportSave.GetOrCreateExistencePlatformOverride(DemoDiscOverrideScopes.MicroSd).Exists = false;

            CreateDesktopInstructionCameraRow(panelEntity, projectRootPath, font, "Camera", DesktopInstructionFirstRowTop);
            CreateDesktopInstructionRow(panelEntity, projectRootPath, font, "LightIcon", "Light", DesktopInstructionSecondRowTop, LightIconSpecs);
            return viewportRootEntity;
        }

        /// <summary>
        /// Creates the console-only instruction panel that is serialized into the shared Blueprint.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="font">Font used for the rendered instruction labels.</param>
        /// <returns>Console camera/light instruction root entity.</returns>
        public Entity CreateConsoleCameraLightInstructionsRoot(string projectRootPath, FontAsset font) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            }

            Entity viewportRootEntity = CreateInstructionViewportRoot(
                "ConsoleCameraLightInstructions",
                "ConsoleCameraLightInstructionsPanel",
                out Entity panelEntity);
            CreateDesktopInstructionCameraRow(
                panelEntity,
                projectRootPath,
                font,
                "Camera",
                DesktopInstructionFirstRowTop,
                "ps2",
                ConsoleCameraIconSpecs,
                ConsoleCameraIconSlotSpecs);
            CreateDesktopInstructionRow(
                panelEntity,
                projectRootPath,
                font,
                "LightIcon",
                "Light",
                DesktopInstructionSecondRowTop,
                ConsoleLightIconSpecs,
                "ps2");
            return viewportRootEntity;
        }

        /// <summary>
        /// Creates a screen-bound reference viewport and its shared rounded instruction panel.
        /// </summary>
        /// <param name="viewportName">Stable viewport entity name.</param>
        /// <param name="panelName">Stable panel entity name.</param>
        /// <param name="panelEntity">Created panel entity.</param>
        /// <returns>Created viewport root entity.</returns>
        Entity CreateInstructionViewportRoot(
            string viewportName,
            string panelName,
            out Entity panelEntity) {
            if (string.IsNullOrWhiteSpace(viewportName)) {
                throw new ArgumentException("Viewport name must be provided.", nameof(viewportName));
            } else if (string.IsNullOrWhiteSpace(panelName)) {
                throw new ArgumentException("Panel name must be provided.", nameof(panelName));
            }

            Entity viewportRootEntity = AssetAuthoringService.OwningCore.EntityFactory.Create(viewportName);
            viewportRootEntity.LayerMask = DesktopOverlayLayerMask;
            viewportRootEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(DesktopViewportWidth, DesktopViewportHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = DesktopViewportWidth,
                ReferenceHeight = DesktopViewportHeight
            });

            panelEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(viewportRootEntity, panelName);
            panelEntity.LocalPosition = new float3(DesktopInstructionPanelLeft, DesktopInstructionPanelTop, 0f);
            panelEntity.LayerMask = DesktopOverlayLayerMask;
            EntitySaveComponent panelSave = FindRequiredEntitySaveComponent(panelEntity);
            panelSave.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            SceneEntityPlatformTransformOverrideAsset microSdPanel = panelSave.GetOrCreateTransformPlatformOverride(DemoDiscOverrideScopes.MicroSd);
            microSdPanel.HasLocalPositionOverride = true;
            microSdPanel.LocalPosition = new float3(DesktopInstructionPanelLeft, MicroSdInstructionPanelTop, 0f);
            microSdPanel.HasLocalScaleOverride = true;
            microSdPanel.LocalScale = new float3(MicroSdInstructionPanelScale, MicroSdInstructionPanelScale, 1f);
            SceneEntityPlatformTransformOverrideAsset nintendo64Panel = panelSave.GetOrCreateTransformPlatformOverride(DemoDiscOverrideScopes.Nintendo64);
            nintendo64Panel.HasLocalPositionOverride = true;
            nintendo64Panel.LocalPosition = new float3(DesktopInstructionPanelLeft, Nintendo64InstructionPanelTop, 0f);
            nintendo64Panel.HasLocalScaleOverride = true;
            nintendo64Panel.LocalScale = new float3(Nintendo64InstructionPanelScale, Nintendo64InstructionPanelScale, 1f);
            SceneUiDepth.SetWorldDepth(panelEntity, 200);
            panelEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(DesktopInstructionPanelWidth, DesktopInstructionPanelHeight),
                Radius = 8f,
                BorderThickness = 2f,
                FillColor = new byte4(20, 24, 32, 224),
                BorderColor = new byte4(120, 140, 170, 255),
            });

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
        /// <param name="commonPlatformId">Platform supplying the shared baseline icon components.</param>
        /// <param name="cameraIconSpecs">Optional single-slot platform icon bindings used by the desktop row.</param>
        /// <param name="cameraSlotSpecsOverride">Optional explicit platform and slot bindings used by console rows.</param>
        void CreateDesktopInstructionCameraRow(
            Entity panelEntity,
            string projectRootPath,
            FontAsset font,
            string text,
            float topOffset,
            string commonPlatformId = "windows",
            DesktopInstructionPlatformIconSpec[] cameraIconSpecs = null,
            DesktopInstructionPlatformIconSlotSpec[] cameraSlotSpecsOverride = null) {
            if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (string.IsNullOrWhiteSpace(text)) {
                throw new ArgumentException("Instruction text must be provided.", nameof(text));
            }

            DesktopInstructionPlatformIconSlotSpec[] cameraSlotSpecs = cameraSlotSpecsOverride
                ?? (cameraIconSpecs == null ? CameraIconSpecs : CreateSingleCameraSlotSpecs(cameraIconSpecs));

            CreateInstructionIconEntity(projectRootPath, panelEntity, "CameraIconPrimary", DesktopInstructionCameraPrimaryIconLeft, topOffset, cameraSlotSpecs, 0, 201, commonPlatformId);
            if (cameraIconSpecs == null || cameraSlotSpecsOverride != null) {
                CreateInstructionIconEntity(projectRootPath, panelEntity, "CameraIconSecondary", DesktopInstructionCameraSecondaryIconLeft, topOffset, cameraSlotSpecs, 1, 201, commonPlatformId);
            }

            CreateInstructionLabel(panelEntity, "CameraText", text, topOffset, font);
        }

        /// <summary>
        /// Converts one console camera icon per platform into the slot representation used by the shared sprite authoring helper.
        /// </summary>
        /// <param name="specs">Single-slot console camera icon specifications.</param>
        /// <returns>Slot specifications with every icon assigned to slot zero.</returns>
        static DesktopInstructionPlatformIconSlotSpec[] CreateSingleCameraSlotSpecs(DesktopInstructionPlatformIconSpec[] specs) {
            if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Console camera icon specs must be provided.", nameof(specs));
            }

            DesktopInstructionPlatformIconSlotSpec[] slotSpecs = new DesktopInstructionPlatformIconSlotSpec[specs.Length];
            for (int index = 0; index < specs.Length; index++) {
                slotSpecs[index] = new DesktopInstructionPlatformIconSlotSpec(
                    specs[index].PlatformId,
                    specs[index].ControlId,
                    specs[index].Size,
                    0);
            }

            return slotSpecs;
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
        /// <param name="specs">Per-platform raw icon bindings used by the row.</param>
        void CreateDesktopInstructionRow(
            Entity panelEntity,
            string projectRootPath,
            FontAsset font,
            string iconEntityName,
            string text,
            float topOffset,
            DesktopInstructionPlatformIconSpec[] specs,
            string commonPlatformId = "windows") {
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

            CreateInstructionIconEntity(projectRootPath, panelEntity, iconEntityName, DesktopInstructionIconLeft, topOffset, specs, 201, commonPlatformId);

            CreateInstructionLabel(panelEntity, iconEntityName + "Text", text, topOffset, font);
        }

        /// <summary>
        /// Creates one row label in the shared label column, with the middle of its capitals on the middle of the row's icon slot.
        /// </summary>
        /// <param name="panelEntity">Instruction panel that should own the label.</param>
        /// <param name="entityName">Stable entity name assigned to the label.</param>
        /// <param name="text">Label text.</param>
        /// <param name="rowTop">Top of the row's icon slot inside the panel.</param>
        /// <param name="font">Font assigned to the label during live authoring.</param>
        void CreateInstructionLabel(Entity panelEntity, string entityName, string text, float rowTop, FontAsset font) {
            Entity textEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(panelEntity, entityName);
            textEntity.LocalPosition = new float3(DesktopInstructionLabelLeft, rowTop + DesktopInstructionLabelTopInRow, 0.1f);
            textEntity.LayerMask = DesktopOverlayLayerMask;
            SceneUiDepth.SetWorldDepth(textEntity, 202);
            EntitySaveComponent textSave = FindRequiredEntitySaveComponent(textEntity);
            textSave.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            SceneEntityPlatformTransformOverrideAsset microSdText = textSave.GetOrCreateTransformPlatformOverride(DemoDiscOverrideScopes.MicroSd);
            microSdText.HasLocalPositionOverride = true;
            microSdText.LocalPosition = new float3(DesktopInstructionLabelLeft, rowTop + MicroSdInstructionLabelTopInRow, 0.1f);
            SceneEntityPlatformTransformOverrideAsset nintendo64Text = textSave.GetOrCreateTransformPlatformOverride(DemoDiscOverrideScopes.Nintendo64);
            nintendo64Text.HasLocalPositionOverride = true;
            nintendo64Text.LocalPosition = new float3(DesktopInstructionLabelLeft, rowTop + Nintendo64InstructionLabelTopInRow, 0.1f);
            TextComponent textComponent = new TextComponent {
                Text = text,
                Font = font,
                NativeFontPixels = DemoDiscSceneUiFont.Mode,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(DesktopInstructionTextWidth, DesktopInstructionTextHeight),
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

            Entity iconEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(panelEntity, text.Replace(" ", string.Empty) + "Icon");
            iconEntity.LocalPosition = new float3(NintendoDsInstructionIconLeft, topOffset, 0.1f);
            iconEntity.LayerMask = NintendoDsOverlayLayerMask;
            SceneUiDepth.SetWorldDepth(iconEntity, 211);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = size,
            };
            iconEntity.AddComponent(spriteComponent);
            ApplyTextureReference(iconEntity, spriteComponent, texturePath);

            Entity textEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(panelEntity, text.Replace(" ", string.Empty) + "Text");
            textEntity.LocalPosition = new float3(NintendoDsInstructionTextLeft, topOffset + NintendoDsInstructionTextTopAdjustment, 0.1f);
            textEntity.LayerMask = NintendoDsOverlayLayerMask;
            SceneUiDepth.SetWorldDepth(textEntity, 212);
            TextComponent textComponent = new TextComponent {
                Text = text,
                Font = font,
                NativeFontPixels = DemoDiscSceneUiFont.Mode,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(NintendoDsInstructionTextWidth, NintendoDsInstructionTextHeight),
            };
            textEntity.AddComponent(textComponent);
            ApplyFontReference(textEntity, textComponent, DemoDiscSceneUiFont.CreateReference(AssetAuthoringService));
        }

        /// <summary>
        /// Creates the shared desktop instruction icon entity and persists per-platform sprite overrides.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated icon assets.</param>
        /// <param name="panelEntity">Instruction panel that should own the icon entity.</param>
        /// <param name="entityName">Stable entity name assigned to the icon entity.</param>
        /// <param name="topOffset">Vertical offset within the panel.</param>
        /// <param name="specs">Per-platform raw icon bindings used by the row.</param>
        /// <param name="depth">World Z depth assigned to the icon entity.</param>
        void CreateInstructionIconEntity(
            string projectRootPath,
            Entity panelEntity,
            string entityName,
            float leftOffset,
            float topOffset,
            DesktopInstructionPlatformIconSpec[] specs,
            float depth,
            string commonPlatformId = "windows") {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (panelEntity == null) {
                throw new ArgumentNullException(nameof(panelEntity));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (specs == null || specs.Length == 0) {
                throw new ArgumentException("Desktop instruction icon specs must be provided.", nameof(specs));
            }

            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(panelEntity, entityName);
            entity.LocalPosition = new float3(leftOffset, topOffset, 0.1f);
            entity.LayerMask = DesktopOverlayLayerMask;
            DesktopInstructionPlatformIconSpec commonSpec = FindRequiredCommonSpec(specs, commonPlatformId);
            ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(projectRootPath, commonSpec.SourcePlatformId, commonSpec.ControlId, AssetAuthoringService, Transaction);
            SceneUiDepth.SetWorldDepth(entity, depth);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = commonIcon.FitDisplaySizeWithin(commonSpec.Size),
                SourceRect = commonIcon.SourceRect,
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
        /// <param name="depth">World Z depth assigned to the icon entity.</param>
        void CreateInstructionIconEntity(
            string projectRootPath,
            Entity panelEntity,
            string entityName,
            float leftOffset,
            float topOffset,
            DesktopInstructionPlatformIconSlotSpec[] specs,
            int slotIndex,
            float depth,
            string commonPlatformId = "windows") {
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

            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(panelEntity, entityName);
            entity.LocalPosition = new float3(leftOffset, topOffset, 0.1f);
            entity.LayerMask = DesktopOverlayLayerMask;

            bool hasCommonSpec = TryFindPlatformSlotSpec(specs, commonPlatformId, slotIndex, out DesktopInstructionPlatformIconSlotSpec commonSpec);
            if (!hasCommonSpec) {
                commonSpec = FindRequiredSlotSpec(specs, slotIndex);
            }

            bool hideByDefault = !hasCommonSpec;
            ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(projectRootPath, commonSpec.SourcePlatformId, commonSpec.ControlId, AssetAuthoringService, Transaction);
            SceneUiDepth.SetWorldDepth(entity, depth);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = commonIcon.FitDisplaySizeWithin(commonSpec.Size),
                SourceRect = commonIcon.SourceRect,
                Color = hideByDefault ? new byte4(255, 255, 255, 0) : new byte4(255, 255, 255, 255),
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
            ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(projectRootPath, spec.SourcePlatformId, spec.ControlId, AssetAuthoringService, Transaction);
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
            ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(projectRootPath, spec.SourcePlatformId, spec.ControlId, AssetAuthoringService, Transaction);
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
        /// Stores the shared scene UI font reference for an instruction label.
        /// </summary>
        void ApplyFontReference(Entity entity, Component component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", DemoDiscSceneUiFont.CreateReference(AssetAuthoringService));
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

            return ((IEditorProjectAuthoringSession)AssetAuthoringService).CreateFileReference(relativePath, AssetEntryKind.Image);
        }
    }
}
