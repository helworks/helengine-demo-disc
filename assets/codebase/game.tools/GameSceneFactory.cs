using city.menu;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Builds the generated authored gameplay scenes contributed by the city demo-disc project.
    /// </summary>
    public sealed class GameSceneFactory {
        /// <summary>
        /// Host-owned capability used to resolve generated control icons and fonts.
        /// </summary>
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;
        /// <summary>
        /// Stable authored material asset id required by the Tilt Trial player sphere.
        /// </summary>
        const string TiltTrialPlayerSphereMarbleMaterialAssetId = "Materials.rendering.tilt_trial.PlayerSphereMarble";

        /// <summary>
        /// Stable authored material settings path required by the Tilt Trial player sphere.
        /// </summary>
        const string TiltTrialPlayerSphereMarbleMaterialRelativePath = "materials/rendering/tilt_trial/PlayerSphereMarble.hasset";

        /// <summary>
        /// Stable authored material asset id required by the Tilt Trial course geometry.
        /// </summary>
        const string TiltTrialCourseMaterialAssetId = "Materials.rendering.tilt_trial.Course";

        /// <summary>
        /// Stable authored material settings path required by the Tilt Trial course geometry.
        /// </summary>
        const string TiltTrialCourseMaterialRelativePath = "materials/rendering/tilt_trial/Course.hasset";

        /// <summary>
        /// Stable authored source-font path used by the Tilt Trial speed HUD.
        /// </summary>
        const string TiltTrialSpeedHudFontRelativePath = "Fonts/Fredoka.ttf";

        /// <summary>
        /// Stable authored asset path used by the generated Tilt Trial level-select scene.
        /// </summary>
        const string TiltTrialLevelSelectSceneAssetRelativePath = "scenes/games/tilt/tilt_trial.helen";

        /// <summary>
        /// Stable authored asset path prefix used by the generated Tilt Trial gameplay scenes.
        /// </summary>
        const string TiltTrialGameplaySceneAssetDirectoryRelativePath = "scenes/games/tilt";

        /// <summary>
        /// Stable authored asset path used by the standalone Level 1 rendering validation scene.
        /// </summary>
        const string TiltTrialLevel01RenderTestSceneAssetRelativePath = "scenes/physics/test_scene_tilt_trial_level_01_render.helen";

        /// <summary>
        /// Stable mesh save-state slot used by the generated player sphere material reference.
        /// </summary>
        const string PlayerSphereMaterialReferenceName = "Materials[0]";

        /// <summary>
        /// Project-relative opaque PNG backdrop for the Tilt Trial title screen.
        /// </summary>
        const string TiltPlayTitleBackgroundTextureRelativePath = "images/ui/tilt_trial/title/background.png";

        /// <summary>
        /// Project-relative PNG for the normal primary title action.
        /// </summary>
        const string TiltPlayPrimaryButtonTextureRelativePath = "images/ui/tilt_trial/title/button_primary.png";

        /// <summary>
        /// Project-relative PNG for the focused primary title action.
        /// </summary>
        const string TiltPlayPrimaryButtonSelectedTextureRelativePath = "images/ui/tilt_trial/title/button_primary_selected.png";

        /// <summary>
        /// Project-relative PNG for the normal Options title action.
        /// </summary>
        const string TiltPlayOptionsButtonTextureRelativePath = "images/ui/tilt_trial/title/button_secondary_options.png";

        /// <summary>
        /// Project-relative PNG for the focused Options title action.
        /// </summary>
        const string TiltPlayOptionsButtonSelectedTextureRelativePath = "images/ui/tilt_trial/title/button_secondary_options_selected.png";

        /// <summary>
        /// Project-relative PNG for the normal Demo Disc return title action.
        /// </summary>
        const string TiltPlayDemoDiscButtonTextureRelativePath = "images/ui/tilt_trial/title/button_secondary_demo_disc.png";

        /// <summary>
        /// Project-relative PNG for the focused Demo Disc return title action.
        /// </summary>
        const string TiltPlayDemoDiscButtonSelectedTextureRelativePath = "images/ui/tilt_trial/title/button_secondary_demo_disc_selected.png";

        /// <summary>
        /// Shared generated cube model used by the authored Tilt Trial course geometry.
        /// </summary>
        readonly RuntimeModel GeneratedCubeModel;

        /// <summary>
        /// Shared generated sphere model used by the playable ball.
        /// </summary>
        readonly RuntimeModel GeneratedSphereModel;

        /// <summary>
        /// Stable mesh save-state slot used by the generated course material reference.
        /// </summary>
        const string CourseMaterialReferenceName = "Materials[0]";

        /// <summary>
        /// Dedicated authored marble material used only by the Tilt Trial player sphere.
        /// </summary>
        readonly RuntimeMaterial TiltTrialPlayerSphereMarbleMaterial;

        /// <summary>
        /// Dedicated authored course material used by the Tilt Trial stage pieces and catch floor.
        /// </summary>
        readonly RuntimeMaterial TiltTrialCourseMaterial;

        /// <summary>
        /// Authored six-colored-face model used exclusively by the render-only Tilt Trial clipping probe.
        /// </summary>
        readonly RuntimeModel TiltTrialClippingProbeModel;

        /// <summary>
        /// Authored textured material used exclusively by the render-only Tilt Trial clipping probe.
        /// </summary>
        readonly RuntimeMaterial TiltTrialClippingProbeMaterial;

        /// <summary>
        /// Authored golden-coin model used by the standalone render-test scene.
        /// </summary>
        readonly RuntimeModel GoldenCoinModel;

        /// <summary>
        /// Authored golden-coin material used by the standalone render-test scene.
        /// </summary>
        readonly RuntimeMaterial GoldenCoinMaterial;

        /// <summary>
        /// Authored goal-flag model used by the standalone render-test scene.
        /// </summary>
        readonly RuntimeModel GoalFlagModel;

        /// <summary>
        /// Authored goal-flag pole material used by the standalone render-test scene.
        /// </summary>
        readonly RuntimeMaterial GoalFlagPoleMaterial;

        /// <summary>
        /// Authored goal-flag banner material used by the standalone render-test scene.
        /// </summary>
        readonly RuntimeMaterial GoalFlagBannerMaterial;

        /// <summary>
        /// Editor service used to persist the 3DS-specific reference-canvas dimensions without changing the shared DS layout.
        /// </summary>
        readonly ComponentPlatformEditingService PlatformEditingServiceValue = new ComponentPlatformEditingService();

        /// <summary>
        /// Resolves generated platform-specific action icons for selector prompts.
        /// </summary>
        readonly GeneratedControlIconAssetResolver ControlIconResolver = new GeneratedControlIconAssetResolver();

        /// <summary>
        /// Absolute project root used to resolve generated selector action icons.
        /// </summary>
        readonly string ProjectRootPath;

        /// <summary>
        /// Editor service used to store current modifier-stack metadata for constrained target platforms.
        /// </summary>
        readonly MeshComponentModifierStackService MeshComponentModifierStackServiceValue = new MeshComponentModifierStackService();

        /// <summary>
        /// Stable platform identifier used for PlayStation 2-specific scene cooking.
        /// </summary>
        const string Ps2PlatformId = "ps2";

        /// <summary>
        /// Stable platform identifier used for PlayStation Portable-specific scene cooking.
        /// </summary>
        const string PspPlatformId = "psp";

        /// <summary>
        /// Maximum world-space edge length used to subdivide scaled Tilt Trial render-test course geometry on constrained platforms.
        /// </summary>
        const double TiltTrialRenderTestTessellationMaxEdgeLength = 1d;

        /// <summary>
        /// Stable platform identifier used by Nintendo DS-specific presentation overrides.
        /// </summary>
        const string NintendoDsPlatformId = "ds";

        /// <summary>
        /// Foreground OBJ render order used by the Nintendo DS start icon.
        /// </summary>
        const byte NintendoDsStartPromptIconRenderOrder = 220;

        /// <summary>
        /// Stable platform identifier used by the 3DS handheld viewport overrides.
        /// </summary>
        const string Nintendo3DsPlatformId = "3ds";

        /// <summary>
        /// Native Nintendo 3DS top-screen width used by the generated handheld presentation.
        /// </summary>
        const int Nintendo3DsTopScreenWidth = 400;

        /// <summary>
        /// Native Nintendo 3DS top-screen height used by the generated handheld presentation.
        /// </summary>
        const int Nintendo3DsTopScreenHeight = 240;

        /// <summary>
        /// Native Nintendo 3DS bottom-screen width used by the generated handheld presentation.
        /// </summary>
        const int Nintendo3DsBottomScreenWidth = 320;

        /// <summary>
        /// Native Nintendo 3DS bottom-screen height used by the generated handheld presentation.
        /// </summary>
        const int Nintendo3DsBottomScreenHeight = 240;

        /// <summary>
        /// Initializes one game-scene factory backed by the prepared generated runtime assets required by the authored gameplay scenes.
        /// </summary>
        /// <param name="assets">Prepared runtime assets consumed by the generated game scenes.</param>
        /// <param name="assetAuthoringService">Host-owned capability used by generated control-icon references.</param>
        public GameSceneFactory(RenderingSceneGenerationAssets assets, string projectRootPath, IEditorProjectAssetAuthoringService assetAuthoringService) {
            if (assets == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Game scene generation requires a project root path.", nameof(projectRootPath));
            } else if (assets.GeneratedCubeModel == null) {
                throw new ArgumentException("Game scene generation requires the generated cube runtime model.", nameof(assets));
            } else if (assets.GeneratedSphereModel == null) {
                throw new ArgumentException("Game scene generation requires the generated sphere runtime model.", nameof(assets));
            } else if (assets.TiltTrialPlayerSphereMarbleMaterial == null) {
                throw new ArgumentException($"Game scene generation requires authored runtime material '{TiltTrialPlayerSphereMarbleMaterialAssetId}'.", nameof(assets));
            } else if (assets.TiltTrialCourseMaterial == null) {
                throw new ArgumentException($"Game scene generation requires authored runtime material '{TiltTrialCourseMaterialAssetId}'.", nameof(assets));
            } else if (assets.TiltTrialClippingProbeModel == null || assets.TiltTrialClippingProbeMaterial == null) {
                throw new ArgumentException("Game scene generation requires the authored Tilt Trial clipping probe model and material.", nameof(assets));
            } else if (assets.GoldenCoinModel == null || assets.GoldenCoinMaterial == null) {
                throw new ArgumentException("Game scene generation requires the authored golden-coin model and material.", nameof(assets));
            } else if (assets.GoalFlagModel == null || assets.GoalFlagPoleMaterial == null || assets.GoalFlagBannerMaterial == null) {
                throw new ArgumentException("Game scene generation requires the authored goal-flag model and materials.", nameof(assets));
            }
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));

            GeneratedCubeModel = assets.GeneratedCubeModel;
            GeneratedSphereModel = assets.GeneratedSphereModel;
            TiltTrialPlayerSphereMarbleMaterial = assets.TiltTrialPlayerSphereMarbleMaterial;
            TiltTrialCourseMaterial = assets.TiltTrialCourseMaterial;
            TiltTrialClippingProbeModel = assets.TiltTrialClippingProbeModel;
            TiltTrialClippingProbeMaterial = assets.TiltTrialClippingProbeMaterial;
            GoldenCoinModel = assets.GoldenCoinModel;
            GoldenCoinMaterial = assets.GoldenCoinMaterial;
            GoalFlagModel = assets.GoalFlagModel;
            GoalFlagPoleMaterial = assets.GoalFlagPoleMaterial;
            GoalFlagBannerMaterial = assets.GoalFlagBannerMaterial;
            ProjectRootPath = Path.GetFullPath(projectRootPath);
        }

        /// <summary>
        /// Creates the generated authored Tilt Trial front-door scene.
        /// </summary>
        /// <returns>Generated authored scene definition for Tilt Trial.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialScene() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = GameSceneCatalog.TiltTrialSceneId,
                SceneAssetRelativePath = TiltTrialLevelSelectSceneAssetRelativePath,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = [
                    CreateLevelSelectCameraEntity(),
                    CreateTiltPlayViewportBackgroundEntity(),
                    CreateTiltPlayShellUiEntity()
                ]
            };
        }

        /// <summary>
        /// Creates a deterministic clipping probe containing one scaled cube, one light, a fixed-axis zoom camera, and the FPS overlay.
        /// </summary>
        /// <returns>Generated authored Level 1 render-test scene.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialLevel01RenderTestScene() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = GameSceneCatalog.TiltTrialLevel01RenderTestSceneId,
                SceneAssetRelativePath = TiltTrialLevel01RenderTestSceneAssetRelativePath,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = [
                    CreateLevel01RenderTestCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateLevel01RenderTestFpsEntity(),
                    CreateLevel01RenderOnlyStageRootEntity()
                ]
            };
        }

        /// <summary>
        /// Creates the dedicated generated authored Tilt Trial level-select scene.
        /// </summary>
        /// <returns>Generated authored scene definition for the Tilt Trial selector.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialLevelSelectScene() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = GameSceneCatalog.TiltTrialSceneId,
                SceneAssetRelativePath = TiltTrialLevelSelectSceneAssetRelativePath,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = [
                    CreateLevelSelectCameraEntity(),
                    CreateLevelSelectUiEntity(useOwnViewport: true)
                ]
            };
        }

        /// <summary>
        /// Creates the generated Tilt Play front-door shell containing title, placeholder options, and level-selector panels.
        /// </summary>
        /// <returns>Generated authoring root for the Tilt Play front-door UI.</returns>
        EditorEntity CreateTiltPlayShellUiEntity() {
            Entity shell = Core.Instance.EntityFactory.Create("TiltPlayShellUi");
            shell.LayerMask = EditorLayerMasks.SceneObjects;
            shell.AddComponent(new city.game.TiltPlayMenuComponent());
            shell.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(1280, 720)
            });
            shell.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = 1280,
                ReferenceHeight = 720
            });

            Entity titlePanel = Core.Instance.EntityFactory.CreateChild(shell, "TiltPlayTitlePanel");
            titlePanel.LayerMask = EditorLayerMasks.SceneObjects;
            titlePanel.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = "TiltPlayTitlePanel"
            });
            CreateTiltPlayActionButton(titlePanel, "TiltPlayPlayButton", new float3(380f, 398f, 0.1f), new int2(520, 72), city.game.TiltPlayMenuAction.Play, TiltPlayPrimaryButtonTextureRelativePath, TiltPlayPrimaryButtonSelectedTextureRelativePath);
            CreateTiltPlayActionButton(titlePanel, "TiltPlayOptionsButton", new float3(380f, 486f, 0.1f), new int2(250, 52), city.game.TiltPlayMenuAction.Options, TiltPlayOptionsButtonTextureRelativePath, TiltPlayOptionsButtonSelectedTextureRelativePath);
            CreateTiltPlayActionButton(titlePanel, "TiltPlayDemoDiscButton", new float3(650f, 486f, 0.1f), new int2(250, 52), city.game.TiltPlayMenuAction.BackToDemoDisc, TiltPlayDemoDiscButtonTextureRelativePath, TiltPlayDemoDiscButtonSelectedTextureRelativePath);

            Entity optionsPanel = CreateRoundedPanelEntity(shell, "TiltPlayOptionsPanel", new float3(0f, 0f, 0f), new int2(1280, 720), 0f, 0f, new byte4(18, 29, 45, 255), new byte4(18, 29, 45, 255), 1);
            optionsPanel.Enabled = false;
            CreateUiTextEntity(optionsPanel, "TiltPlayOptionsTitle", new float3(240f, 230f, 0.1f), "OPTIONS", new int2(800, 80), 3f, 3, new byte4(247, 248, 252, 255), TextAlignment.Center);
            CreateUiTextEntity(optionsPanel, "TiltPlayOptionsPlaceholder", new float3(240f, 330f, 0.1f), "Settings coming soon", new int2(800, 48), 1.5f, 3, new byte4(196, 210, 226, 255), TextAlignment.Center);
            CreateTiltPlayOptionsBackButton(optionsPanel);

            Entity levelSelectPanel = CreateLevelSelectUiEntity(useOwnViewport: false);
            levelSelectPanel.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = "TiltPlayLevelSelectPanel"
            });
            levelSelectPanel.Enabled = false;
            shell.AddChild(levelSelectPanel);

            if (shell is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Play shell generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the screen-bound title backdrop that fills the live viewport behind the fitted Tilt Play shell.
        /// </summary>
        /// <returns>Generated authoring root for the viewport-sized title backdrop.</returns>
        EditorEntity CreateTiltPlayViewportBackgroundEntity() {
            Entity backgroundRoot = Core.Instance.EntityFactory.Create("TiltPlayViewportBackground");
            backgroundRoot.LayerMask = EditorLayerMasks.SceneObjects;
            backgroundRoot.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(1280, 720)
            });

            Entity backgroundEntity = CreateTiltPlaySpriteEntity(
                backgroundRoot,
                "TiltPlayTitleBackground",
                new float3(0f, 0f, 0f),
                new int2(1280, 720),
                TiltPlayTitleBackgroundTextureRelativePath,
                0);
            LayoutComponent backgroundLayoutComponent = new LayoutComponent {
                LayoutSpace = LayoutComponent.CameraViewportLayoutSpace
            };
            backgroundLayoutComponent.SetAnchorDistances(left: 0f, top: 0f, right: 0f, bottom: 0f);
            backgroundEntity.AddComponent(backgroundLayoutComponent);

            if (backgroundRoot is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Play viewport background generation requires an editor-authored root.");
        }

        /// <summary>
        /// Creates the separate DS and 3DS level selector while leaving the authored gameplay levels shared.
        /// </summary>
        /// <returns>Generated handheld selector scene definition with a game-owned top screen and empty bottom screen.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialHandheldLevelSelectScene() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = global::city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId,
                SceneAssetRelativePath = "scenes/games/tilt/tilt_trial_ds.helen",
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = [],
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    RootEntities = CreateTiltTrialHandheldLevelSelectSceneRoots()
                }
            };
        }

        /// <summary>
        /// Creates the complete game-owned DS and 3DS level-selector root set without using the shared rendering showcase scaffold.
        /// </summary>
        /// <returns>Top camera, top-screen title root, and empty bottom-screen camera hierarchy.</returns>
        Entity[] CreateTiltTrialHandheldLevelSelectSceneRoots() {
            Entity topCameraEntity = CreateTiltTrialHandheldLevelSelectTopCameraEntity();
            return [
                topCameraEntity,
                CreateHandheldLevelSelectTopInfoEntity(topCameraEntity),
                CreateTiltTrialHandheldLevelSelectBottomScreenCameraEntity()
            ];
        }

        /// <summary>
        /// Creates the game-owned top-screen camera for the handheld level selector.
        /// </summary>
        /// <returns>Handheld selector top-screen camera.</returns>
        Entity CreateTiltTrialHandheldLevelSelectTopCameraEntity() {
            return CreateLevelSelectCameraEntity();
        }

        /// <summary>
        /// Creates the game-owned bottom-screen camera and selector viewport for the handheld selector.
        /// </summary>
        /// <returns>Bottom-screen camera containing the game-owned selector viewport.</returns>
        Entity CreateTiltTrialHandheldLevelSelectBottomScreenCameraEntity() {
            Entity cameraEntity = Core.Instance.EntityFactory.Create("TiltTrialHandheldLevelSelectBottomScreenCamera");
            cameraEntity.LayerMask = EditorLayerMasks.SceneObjects;
            cameraEntity.AddComponent(new CameraComponent {
                CameraDrawOrder = 1,
                LayerMask = 0b00000001,
                Viewport = new float4(0f, 1f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(18f / 255f, 27f / 255f, 43f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });

            Entity viewportRoot = Core.Instance.EntityFactory.CreateChild(cameraEntity, "TiltTrialHandheldLevelSelectBottomScreenRoot");
            viewportRoot.LayerMask = EditorLayerMasks.SceneObjects;
            ViewportComponent viewportComponent = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(256, 192),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = 256,
                ReferenceHeight = 192
            };
            viewportRoot.AddComponent(viewportComponent);
            ApplyNintendo3DsViewportOverride(viewportRoot, viewportComponent, Nintendo3DsBottomScreenWidth, Nintendo3DsBottomScreenHeight, 256, 192);
            viewportRoot.AddChild(CreateHandheldLevelSelectUiEntity());
            return cameraEntity;
        }

        /// <summary>
        /// Creates the isolated handheld menu top-screen title without adding the instruction text yet.
        /// </summary>
        /// <param name="parent">Top-screen camera that owns and routes the title subtree.</param>
        /// <returns>Top-screen title root.</returns>
        EditorEntity CreateHandheldLevelSelectTopInfoEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "TiltTrialHandheldLevelSelectTopInfo");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            ViewportComponent viewportComponent = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(256, 192)
            };
            entity.AddComponent(viewportComponent);
            ApplyNintendo3DsViewportOverride(entity, viewportComponent, Nintendo3DsTopScreenWidth, Nintendo3DsTopScreenHeight, 256, 192);
            ReferenceCanvasFitComponent referenceCanvasFitComponent = new ReferenceCanvasFitComponent {
                ReferenceWidth = 256,
                ReferenceHeight = 192
            };
            entity.AddComponent(referenceCanvasFitComponent);
            ApplyNintendo3DsReferenceCanvasOverride(entity, referenceCanvasFitComponent, 256, 192);
            CreateUiTextEntity(entity, "TiltTrialHandheldLevelSelectTitle", new float3(20f, 28f, 0.1f), "TILT TRIAL", new int2(216, 34), 1.35f, 2, new byte4(247, 248, 252, 255), TextAlignment.Center);
            Entity previewPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialHandheldLevelSelectPreviewPanel", new float3(72f, 67f, 0f), new int2(112, 112), 8f, 2f, new byte4(26, 40, 61, 255), new byte4(122, 147, 182, 255), 2);
            CreateUiTextEntity(previewPanelEntity, "TiltTrialHandheldLevelSelectPreviewText", new float3(8f, 42f, 0.1f), "Preview", new int2(96, 28), 0.85f, 3, new byte4(223, 230, 239, 255), TextAlignment.Center);
            return (EditorEntity)entity;
        }

        /// <summary>
        /// Creates the bottom-screen level selector using the existing selector component contract.
        /// </summary>
        /// <returns>Bottom-screen selector root.</returns>
        EditorEntity CreateHandheldLevelSelectUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialHandheldLevelSelectUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            entity.AddComponent(new city.game.TiltTrialLevelSelectComponent {
                UseDetailsStage = true
            });
            ViewportComponent viewportComponent = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(256, 192),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = 256,
                ReferenceHeight = 192
            };
            entity.AddComponent(viewportComponent);
            ApplyNintendo3DsViewportOverride(entity, viewportComponent, Nintendo3DsBottomScreenWidth, Nintendo3DsBottomScreenHeight, Nintendo3DsBottomScreenWidth, Nintendo3DsBottomScreenHeight);

            Entity listPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialLevelSelectListPanel", new float3(6f, 8f, 0f), new int2(244, 176), 5f, 0f, new byte4(0, 0, 0, 0), new byte4(0, 0, 0, 0), 1);
            Entity detailsPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialLevelSelectDetailsPanel", new float3(6f, 8f, 0f), new int2(244, 176), 5f, 0f, new byte4(0, 0, 0, 0), new byte4(0, 0, 0, 0), 1);
            detailsPanelEntity.Enabled = false;
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectName", new float3(12f, 10f, 0.1f), "Level 1", new int2(220, 22), 0.85f, 3, new byte4(247, 248, 252, 255), TextAlignment.Left);
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectTimer", new float3(12f, 40f, 0.1f), "MAX 99.00", new int2(220, 18), 0.65f, 3, new byte4(255, 214, 138, 255), TextAlignment.Left);
            Entity targetTimesEntity = CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectTargetTimes", new float3(12f, 64f, 0.1f), string.Empty, new int2(220, 18), 0.7f, 3, new byte4(223, 230, 239, 255), TextAlignment.Left);
            targetTimesEntity.Enabled = false;
            IReadOnlyList<global::city.game.TiltTrialLevelCatalogEntry> levelEntries = global::city.game.TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < levelEntries.Count; index++) {
                int oneBasedIndex = index + 1;
                CreateLevelSelectActionButton(
                    listPanelEntity,
                    $"TiltTrialLevelRow{oneBasedIndex:00}",
                    new float3(0f, 3f + (index * 32f), 0f), new int2(244, 30),
                    levelEntries[index].DisplayName,
                    city.game.TiltTrialLevelSelectAction.SelectStage,
                    index);
            }

            Entity backButtonEntity = CreateLevelSelectActionButton(detailsPanelEntity, "TiltTrialLevelSelectBackButton", new float3(6f, 116f, 0f), new int2(232, 28), "BACK", city.game.TiltTrialLevelSelectAction.BackToStages, -1);
            Entity playButtonEntity = CreateLevelSelectActionButton(detailsPanelEntity, "TiltTrialLevelSelectPlayButton", new float3(6f, 150f, 0f), new int2(232, 34), "PLAY", city.game.TiltTrialLevelSelectAction.PlaySelectedStage, -1);
            backButtonEntity.Enabled = false;
            playButtonEntity.Enabled = false;

            return (EditorEntity)entity;
        }

        /// <summary>
        /// Creates the scaffolded generated authored Tilt Trial gameplay level scenes.
        /// </summary>
        /// <returns>Ordered scaffolded gameplay scenes for all current Tilt Trial levels.</returns>
        public IReadOnlyList<GeneratedAuthoringSceneDefinition> CreateTiltTrialLevelScenes() {
            IReadOnlyList<global::city.game.TiltTrialLevelCatalogEntry> levelEntries = global::city.game.TiltTrialLevelCatalog.CreateEntries();
            GeneratedAuthoringSceneDefinition[] scenes = new GeneratedAuthoringSceneDefinition[levelEntries.Count];
            for (int index = 0; index < levelEntries.Count; index++) {
                scenes[index] = CreateTiltTrialGameplayScene(levelEntries[index]);
            }

            return scenes;
        }

        /// <summary>
        /// Creates the console presentation hierarchy that is saved into the reusable gameplay Blueprint.
        /// </summary>
        /// <returns>Single editor root containing the console camera, lighting, and HUD.</returns>
        public EditorEntity CreateTiltTrialConsolePresentationRoot() {
            global::city.game.TiltTrialLevelCatalogEntry levelEntry = global::city.game.TiltTrialLevelCatalog.CreateEntries()[0];
            EditorEntity root = CreatePresentationRoot("TiltTrialConsolePresentationRoot");
            root.AddChild(CreateCameraEntity());
            root.AddChild(CreateDirectionalLightEntity());
            root.AddChild(CreateDirectionalFillLightEntity());
            root.AddChild(CreateAmbientLightEntity());
            root.AddChild(CreateGameplayUiEntity(levelEntry));
            return root;
        }

        /// <summary>
        /// Creates the DS and 3DS presentation hierarchy that is saved into the reusable gameplay Blueprint.
        /// </summary>
        /// <returns>Single editor root containing the top-screen camera, lighting, and bottom-screen HUD.</returns>
        public EditorEntity CreateTiltTrialHandheldPresentationRoot() {
            EditorEntity root = CreatePresentationRoot("TiltTrialHandheldPresentationRoot");
            root.AddChild(CreateCameraEntity());
            root.AddChild(CreateDirectionalLightEntity());
            root.AddChild(CreateDirectionalFillLightEntity());
            root.AddChild(CreateAmbientLightEntity());
            global::city.game.TiltTrialLevelCatalogEntry levelEntry = global::city.game.TiltTrialLevelCatalog.CreateEntries()[0];
            root.AddChild(CreateHandheldGameplayControllerEntity(levelEntry));
            return root;
        }

        /// <summary>
        /// Creates a presentation Blueprint root that keeps generated child ids isolated from authored level entities.
        /// </summary>
        /// <param name="name">Stable presentation root name.</param>
        /// <returns>Empty editor presentation root.</returns>
        EditorEntity CreatePresentationRoot(string name) {
            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial presentation generation requires editor-authored roots.");
        }

        /// <summary>
        /// Creates the handheld gameplay controller and its game-owned bottom-screen presentation.
        /// </summary>
        /// <param name="levelEntry">Shared level metadata used to seed the bottom-screen HUD.</param>
        /// <returns>Gameplay controller containing the bottom-screen camera and HUD hierarchy.</returns>
        EditorEntity CreateHandheldGameplayControllerEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            Entity controllerEntity = Core.Instance.EntityFactory.Create("TiltTrialHandheldGameplayController");
            controllerEntity.LayerMask = EditorLayerMasks.SceneObjects;
            controllerEntity.AddComponent(new city.game.TiltTrialSessionComponent());
            controllerEntity.AddChild(CreateHandheldGameplayBottomScreenCameraEntity(levelEntry));
            if (controllerEntity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial handheld gameplay generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the game-owned DS and 3DS bottom-screen camera and HUD hierarchy for active gameplay.
        /// </summary>
        /// <param name="levelEntry">Shared level metadata used to seed the HUD values.</param>
        /// <returns>Bottom-screen camera containing the gameplay HUD.</returns>
        EditorEntity CreateHandheldGameplayBottomScreenCameraEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            Entity cameraEntity = Core.Instance.EntityFactory.Create("TiltTrialHandheldGameplayBottomScreenCamera");
            cameraEntity.LayerMask = EditorLayerMasks.SceneObjects;
            cameraEntity.AddComponent(new CameraComponent {
                CameraDrawOrder = 1,
                LayerMask = 0b00000001,
                Viewport = new float4(0f, 1f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(18f / 255f, 27f / 255f, 43f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });

            Entity viewportRoot = Core.Instance.EntityFactory.CreateChild(cameraEntity, "TiltTrialHandheldGameplayBottomScreenRoot");
            viewportRoot.LayerMask = EditorLayerMasks.SceneObjects;
            ViewportComponent viewportComponent = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(256, 192),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = 256,
                ReferenceHeight = 192
            };
            viewportRoot.AddComponent(viewportComponent);
            ApplyNintendo3DsViewportOverride(viewportRoot, viewportComponent, Nintendo3DsBottomScreenWidth, Nintendo3DsBottomScreenHeight, 256, 192);
            viewportRoot.AddChild(CreateHandheldGameplayUiEntity(levelEntry));

            if (cameraEntity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial handheld gameplay generation requires an editor-authored bottom-screen camera.");
        }

        /// <summary>
        /// Creates the compact bottom-screen HUD used during handheld Tilt Trial gameplay.
        /// </summary>
        /// <param name="levelEntry">Shared level metadata used to seed the HUD values.</param>
        /// <returns>Bottom-screen HUD root with the names consumed by the gameplay session.</returns>
        EditorEntity CreateHandheldGameplayUiEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialHandheldGameplayUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            ViewportComponent viewportComponent = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(256, 192),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = 256,
                ReferenceHeight = 192
            };
            entity.AddComponent(viewportComponent);
            ApplyNintendo3DsViewportOverride(entity, viewportComponent, Nintendo3DsBottomScreenWidth, Nintendo3DsBottomScreenHeight, Nintendo3DsBottomScreenWidth, Nintendo3DsBottomScreenHeight);

            Entity panelEntity = CreateRoundedPanelEntity(entity, "TiltTrialHandheldGameplayPanel", new float3(6f, 6f, 0f), new int2(244, 180), 6f, 2f, new byte4(26, 40, 61, 255), new byte4(96, 128, 168, 255), 1);
            panelEntity.Enabled = false;
            CreateUiTextEntity(panelEntity, "TiltTrialTimerText", new float3(12f, 10f, 0.1f), global::city.game.TiltTrialLevelSelectComponent.FormatTimerSeconds(levelEntry.StartTimeSeconds), new int2(104, 26), 0.8f, 2, new byte4(255, 246, 223, 255), TextAlignment.Left);
            CreateUiTextEntity(panelEntity, "TiltTrialCoinText", new float3(124f, 10f, 0.1f), "Coins 0/0", new int2(106, 26), 0.65f, 2, new byte4(255, 246, 223, 255), TextAlignment.Right);
            CreateUiTextEntity(panelEntity, "TiltTrialTargetTimesText", new float3(12f, 40f, 0.1f), "Targets G00.00 S00.00 B00.00", new int2(220, 18), 0.48f, 2, new byte4(223, 230, 239, 255), TextAlignment.Left);

            Entity speedTextEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, "TiltTrialSpeedText");
            speedTextEntity.LocalPosition = new float3(12f, 66f, 0.1f);
            speedTextEntity.Static = false;
            TextComponent speedTextComponent = new TextComponent {
                Text = "0\nkm/h",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(220, 58),
                FontScale = 0.9f,
                Alignment = TextAlignment.Center,
                RenderOrder2D = 2,
            };
            speedTextEntity.AddComponent(speedTextComponent);
            ApplyFontReference(speedTextEntity, speedTextComponent, TiltTrialSpeedHudFontRelativePath);
            speedTextEntity.AddComponent(new city.game.DemoTiltSpeedTextComponent {
                TargetEntityName = "PlayerSphere",
                TargetEntityRole = "PlayerSphere"
            });

            CreateUiTextEntity(panelEntity, "TiltTrialHandheldGameplayHint", new float3(12f, 145f, 0.1f), "D-PAD MOVE   L/R CAMERA", new int2(220, 20), 0.5f, 2, new byte4(196, 210, 226, 255), TextAlignment.Center);

            Entity startOverlayEntity = Core.Instance.EntityFactory.CreateChild(entity, "TiltTrialStartOverlay");
            startOverlayEntity.LocalPosition = new float3(16f, 58f, 0f);
            startOverlayEntity.Static = false;
            startOverlayEntity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = "TiltTrialStartOverlay"
            });
            CreateTiltTrialStartPrompt(startOverlayEntity, new float3(12f, 18f, 0.1f), new int2(200, 32), 0.72f, 5, new int2(32, 32));

            Entity resultsOverlayEntity = Core.Instance.EntityFactory.CreateChild(entity, "TiltTrialResultsOverlay");
            resultsOverlayEntity.LocalPosition = new float3(16f, 8f, 0f);
            resultsOverlayEntity.Static = false;
            resultsOverlayEntity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = "TiltTrialResultsOverlay"
            });
            resultsOverlayEntity.Enabled = false;
            CreateUiTextEntity(resultsOverlayEntity, "TiltTrialResultsBodyText", new float3(12f, 6f, 0.1f), "Time 00.00", new int2(200, 22), 0.72f, 5, new byte4(247, 248, 252, 255), TextAlignment.Center);
            CreateTiltTrialResultActionButton(resultsOverlayEntity, "TiltTrialResultNextButton", new float3(12f, 40f, 0.1f), new int2(200, 30), "NEXT", city.game.TiltTrialSessionAction.Next);
            CreateTiltTrialResultActionButton(resultsOverlayEntity, "TiltTrialResultRetryButton", new float3(12f, 78f, 0.1f), new int2(200, 30), "RETRY", city.game.TiltTrialSessionAction.Retry);
            CreateTiltTrialResultActionButton(resultsOverlayEntity, "TiltTrialResultExitButton", new float3(12f, 116f, 0.1f), new int2(200, 30), "BACK TO MENU", city.game.TiltTrialSessionAction.LevelSelect);

            Entity failOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialFailOverlay", new float3(16f, 26f, 0f), new int2(224, 140), 6f, 2f, new byte4(43, 23, 28, 245), new byte4(214, 112, 112, 255), 4);
            failOverlayEntity.Enabled = false;
            CreateUiTextEntity(failOverlayEntity, "TiltTrialFailTitleText", new float3(12f, 12f, 0.1f), "Time Up", new int2(200, 22), 0.9f, 5, new byte4(255, 223, 223, 255), TextAlignment.Center);
            CreateUiTextEntity(failOverlayEntity, "TiltTrialFailBodyText", new float3(12f, 48f, 0.1f), "> Retry\n  Level Select", new int2(200, 72), 0.65f, 5, new byte4(247, 248, 252, 255), TextAlignment.Center);

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial handheld gameplay generation requires an editor-authored HUD root.");
        }

        /// <summary>
        /// Creates the shared Tilt Trial start prompt from text siblings around a platform-specific Accept icon.
        /// </summary>
        /// <param name="parent">Start overlay that owns the prompt.</param>
        /// <param name="promptPosition">Top-left position of the prompt row.</param>
        /// <param name="promptSize">Authored prompt row size.</param>
        /// <param name="fontScale">Text font scale for the target presentation.</param>
        /// <param name="renderOrder2D">2D render order shared by the prompt parts.</param>
        /// <param name="iconBounds">Maximum display bounds for the generated control icon.</param>
        void CreateTiltTrialStartPrompt(Entity parent, float3 promptPosition, int2 promptSize, float fontScale, byte renderOrder2D, int2 iconBounds) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (promptSize.X <= 0 || promptSize.Y <= 0) {
                throw new ArgumentOutOfRangeException(nameof(promptSize), "Start prompt dimensions must be positive.");
            } else if (iconBounds.X <= 0 || iconBounds.Y <= 0) {
                throw new ArgumentOutOfRangeException(nameof(iconBounds), "Start prompt icon bounds must be positive.");
            }

            int gap = Math.Max(8, promptSize.X / 32);
            int iconLeft = (promptSize.X - iconBounds.X) / 2;
            int prefixWidth = iconLeft - gap;
            int suffixLeft = iconLeft + iconBounds.X + gap;
            int suffixWidth = promptSize.X - suffixLeft;
            if (prefixWidth <= 0 || suffixWidth <= 0) {
                throw new InvalidOperationException("Start prompt dimensions do not leave room for both text labels and the icon.");
            }

            byte4 promptColor = new byte4(255, 236, 196, 255);
            CreateUiTextEntity(
                parent,
                "TiltTrialStartPromptPrefixText",
                new float3(promptPosition.X, promptPosition.Y, promptPosition.Z),
                "Press",
                new int2(prefixWidth, promptSize.Y),
                fontScale,
                renderOrder2D,
                promptColor,
                TextAlignment.Right);

            Entity iconEntity = Core.Instance.EntityFactory.CreateChild(parent, "TiltTrialStartPromptIcon");
            iconEntity.LocalPosition = new float3(
                promptPosition.X + iconLeft,
                promptPosition.Y + Math.Max(0, (promptSize.Y - iconBounds.Y) / 2),
                promptPosition.Z);
            iconEntity.Static = false;
            ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(ProjectRootPath, "windows", "enter", AssetAuthoringService);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = commonIcon.FitDisplaySizeWithin(iconBounds),
                SourceRect = commonIcon.SourceRect,
                RenderOrder2D = renderOrder2D
            };
            iconEntity.AddComponent(spriteComponent);
            ApplyLevelSelectPromptTexture(iconEntity, spriteComponent, commonIcon.SourcePngRelativePath);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "gamecube", "gamecube", "a", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "switch", "switch", "a", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "wiiu", "wii", "a", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "wii", "wii", "a", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "ps2", "ps2", "cross", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "psp", "psp", "cross", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "psvita", "psvita", "cross", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, NintendoDsPlatformId, NintendoDsPlatformId, "a", iconBounds);
            CreateTiltTrialStartPromptPlatformOverride(iconEntity, spriteComponent, "3ds", "3ds", "a", iconBounds);

            CreateUiTextEntity(
                parent,
                "TiltTrialStartPromptSuffixText",
                new float3(promptPosition.X + suffixLeft, promptPosition.Y, promptPosition.Z),
                "to start",
                new int2(suffixWidth, promptSize.Y),
                fontScale,
                renderOrder2D,
                promptColor,
                TextAlignment.Left);
        }

        /// <summary>
        /// Persists one platform-specific Accept icon override for the Tilt Trial start prompt.
        /// </summary>
        /// <param name="entity">Icon entity owning the sprite.</param>
        /// <param name="commonComponent">Windows sprite component.</param>
        /// <param name="platformId">Target platform receiving the override.</param>
        /// <param name="sourcePlatformId">Icon family platform used to resolve the source asset.</param>
        /// <param name="controlId">Accept control id within the source icon family.</param>
        /// <param name="iconBounds">Maximum display size for the icon.</param>
        void CreateTiltTrialStartPromptPlatformOverride(Entity entity, SpriteComponent commonComponent, string platformId, string sourcePlatformId, string controlId, int2 iconBounds) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            SpriteComponent overrideComponent = (SpriteComponent)PlatformEditingServiceValue.EnsurePlatformOverrideComponent(commonComponent, saveComponent, platformId);
            ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(ProjectRootPath, sourcePlatformId, controlId, AssetAuthoringService);
            overrideComponent.Size = resolvedIcon.FitDisplaySizeWithin(iconBounds);
            overrideComponent.SourceRect = string.Equals(platformId, NintendoDsPlatformId, StringComparison.Ordinal)
                ? new float4(0f, 0f, 1f, 1f)
                : resolvedIcon.SourceRect;
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, platformId, nameof(SpriteComponent.Size));
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, platformId, nameof(SpriteComponent.SourceRect));
            if (string.Equals(platformId, NintendoDsPlatformId, StringComparison.Ordinal)) {
                overrideComponent.RenderOrder2D = NintendoDsStartPromptIconRenderOrder;
                PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, platformId, nameof(SpriteComponent.RenderOrder2D));
            }
            PlatformEditingServiceValue.StoreAssetReference(commonComponent, overrideComponent, saveComponent, platformId, TextureAssetScenePersistenceSupport.TextureReferenceName, global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateImage(AssetAuthoringService, resolvedIcon.SourcePngRelativePath));
            PlatformEditingServiceValue.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, platformId);
        }

        /// <summary>
        /// Creates one full-width handheld selector button and its semantic selector action bridge.
        /// </summary>
        /// <param name="parent">Selector panel that owns the button.</param>
        /// <param name="name">Stable button entity name.</param>
        /// <param name="position">Button position in the bottom-screen reference canvas.</param>
        /// <param name="size">Button dimensions.</param>
        /// <param name="label">Visible button label.</param>
        /// <param name="action">Semantic selector action emitted on release.</param>
        /// <param name="stageIndex">Zero-based stage index for stage selection, or -1 for non-stage actions.</param>
        Entity CreateLevelSelectActionButton(Entity parent, string name, float3 position, int2 size, string label, city.game.TiltTrialLevelSelectAction action, int stageIndex) {
            Entity buttonEntity = CreateRoundedPanelEntity(parent, name, position, size, 5f, 0f, new byte4(40, 58, 87, 255), new byte4(0, 0, 0, 0), 1);
            buttonEntity.AddComponent(new InteractableComponent {
                Size = size
            });
            buttonEntity.AddComponent(new city.game.TiltTrialLevelSelectActionComponent {
                Action = action,
                StageIndex = stageIndex
            });
            CreateUiTextEntity(buttonEntity, name + "Label", new float3(8f, 5f, 0.1f), label, new int2(size.X - 16, size.Y - 8), 0.7f, 3, new byte4(247, 248, 252, 255), TextAlignment.Center);
            return buttonEntity;
        }

        /// <summary>
        /// Creates one selector action prompt using the generated platform-specific control icon pack.
        /// </summary>
        /// <param name="parent">Selector view root that owns the prompt.</param>
        /// <param name="name">Stable prompt entity name.</param>
        /// <param name="position">Prompt position within the 1280 by 720 selector view.</param>
        /// <param name="controlId">Windows control id used by the common sprite.</param>
        /// <param name="label">Action label shown beside the icon.</param>
        /// <returns>Generated prompt entity.</returns>
        Entity CreateLevelSelectActionPrompt(Entity parent, string name, float3 position, string controlId, string label) {
            int2 iconBounds = new int2(56, 48);
            Entity promptEntity = Core.Instance.EntityFactory.CreateChild(parent, name);
            promptEntity.LocalPosition = position;
            promptEntity.LayerMask = EditorLayerMasks.SceneObjects;
            Entity iconEntity = Core.Instance.EntityFactory.CreateChild(promptEntity, name + "Icon");
            iconEntity.LocalPosition = new float3(10f, 7f, 0.1f);
            iconEntity.LayerMask = EditorLayerMasks.SceneObjects;
            ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(ProjectRootPath, "windows", controlId, AssetAuthoringService);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = commonIcon.FitDisplaySizeWithin(iconBounds),
                SourceRect = commonIcon.SourceRect,
                RenderOrder2D = 3
            };
            iconEntity.AddComponent(spriteComponent);
            ApplyLevelSelectPromptTexture(iconEntity, spriteComponent, commonIcon.SourcePngRelativePath);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "gamecube", "gamecube", controlId == "enter" ? "a" : "b", iconBounds);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "switch", "switch", controlId == "enter" ? "a" : "b", iconBounds);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "wiiu", "wii", controlId == "enter" ? "a" : "b", iconBounds);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "wii", "wii", controlId == "enter" ? "a" : "b", iconBounds);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "ps2", "ps2", controlId == "enter" ? "cross" : "circle", iconBounds);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "psp", "psp", controlId == "enter" ? "cross" : "circle", iconBounds);
            CreateLevelSelectPromptPlatformOverride(iconEntity, spriteComponent, "psvita", "psvita", controlId == "enter" ? "cross" : "circle", iconBounds);
            CreateUiTextEntity(promptEntity, name + "Label", new float3(72f, 7f, 0.1f), label, new int2(112, 48), 0.9f, 3, new byte4(247, 248, 252, 255), TextAlignment.Left);
            return promptEntity;
        }

        /// <summary>
        /// Applies one generated icon texture reference to a selector prompt sprite.
        /// </summary>
        /// <param name="entity">Icon entity owning the sprite.</param>
        /// <param name="component">Sprite component receiving the reference.</param>
        /// <param name="relativePath">Project-relative texture path.</param>
        void ApplyLevelSelectPromptTexture(Entity entity, SpriteComponent component, string relativePath) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, TextureAssetScenePersistenceSupport.TextureReferenceName, global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateImage(AssetAuthoringService, relativePath));
        }

        /// <summary>
        /// Persists one platform-specific selector prompt icon override.
        /// </summary>
        /// <param name="entity">Icon entity owning the shared sprite.</param>
        /// <param name="commonComponent">Windows sprite component.</param>
        /// <param name="platformId">Target platform receiving the override.</param>
        /// <param name="sourcePlatformId">Icon family platform used to resolve the source asset.</param>
        /// <param name="controlId">Control id within the source icon family.</param>
        /// <param name="iconBounds">Maximum display size for the icon.</param>
        void CreateLevelSelectPromptPlatformOverride(Entity entity, SpriteComponent commonComponent, string platformId, string sourcePlatformId, string controlId, int2 iconBounds) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            SpriteComponent overrideComponent = (SpriteComponent)PlatformEditingServiceValue.EnsurePlatformOverrideComponent(commonComponent, saveComponent, platformId);
            ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(ProjectRootPath, sourcePlatformId, controlId, AssetAuthoringService);
            overrideComponent.Size = resolvedIcon.FitDisplaySizeWithin(iconBounds);
            overrideComponent.SourceRect = resolvedIcon.SourceRect;
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, platformId, nameof(SpriteComponent.Size));
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, platformId, nameof(SpriteComponent.SourceRect));
            PlatformEditingServiceValue.StoreAssetReference(commonComponent, overrideComponent, saveComponent, platformId, TextureAssetScenePersistenceSupport.TextureReferenceName, global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateImage(AssetAuthoringService, resolvedIcon.SourcePngRelativePath));
            PlatformEditingServiceValue.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, platformId);
        }

        /// <summary>
        /// Creates one Tilt Play title-shell action button and its semantic pointer-action bridge.
        /// </summary>
        /// <param name="parent">Title-shell panel that owns the button.</param>
        /// <param name="name">Stable generated button role.</param>
        /// <param name="position">Button position in the shared reference canvas.</param>
        /// <param name="size">Button dimensions in authored pixels.</param>
        /// <param name="action">Semantic Tilt Play action emitted on release.</param>
        /// <param name="normalTextureRelativePath">Project-relative PNG used while the action is not selected.</param>
        /// <param name="selectedTextureRelativePath">Project-relative PNG used while the action is selected.</param>
        /// <returns>Generated action button entity.</returns>
        Entity CreateTiltPlayActionButton(Entity parent, string name, float3 position, int2 size, city.game.TiltPlayMenuAction action, string normalTextureRelativePath, string selectedTextureRelativePath) {
            Entity buttonEntity = CreateTiltPlaySpriteEntity(parent, name, position, size, normalTextureRelativePath, 4);
            buttonEntity.AddComponent(new InteractableComponent {
                Size = size
            });
            buttonEntity.AddComponent(new city.game.TiltPlayMenuActionComponent {
                Action = action
            });
            Entity selectedOverlayEntity = CreateTiltPlaySpriteEntity(buttonEntity, name + "SelectedOverlay", new float3(0f, 0f, 0.1f), size, selectedTextureRelativePath, 5);
            selectedOverlayEntity.Enabled = false;
            return buttonEntity;
        }

        /// <summary>
        /// Creates one title-screen sprite and persists its authored texture reference for platform-independent rendering.
        /// </summary>
        /// <param name="parent">Entity that owns the sprite.</param>
        /// <param name="name">Stable sprite entity role.</param>
        /// <param name="position">Sprite position in the reference canvas.</param>
        /// <param name="size">Authored pixel size of the sprite.</param>
        /// <param name="textureRelativePath">Project-relative PNG path for the sprite.</param>
        /// <param name="renderOrder">2D render order within the title screen.</param>
        /// <returns>Created sprite entity.</returns>
        Entity CreateTiltPlaySpriteEntity(Entity parent, string name, float3 position, int2 size, string textureRelativePath, byte renderOrder) {
            Entity spriteEntity = Core.Instance.EntityFactory.CreateChild(parent, name);
            spriteEntity.LocalPosition = position;
            spriteEntity.LayerMask = EditorLayerMasks.SceneObjects;
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = size,
                RenderOrder2D = renderOrder
            };
            spriteEntity.AddComponent(spriteComponent);
            spriteEntity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = name
            });
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(spriteEntity);
            saveComponent.SetAssetReference(spriteComponent, TextureAssetScenePersistenceSupport.TextureReferenceName, global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateImage(AssetAuthoringService, textureRelativePath));
            return spriteEntity;
        }

        /// <summary>
        /// Creates the temporary Options return button while that placeholder screen remains outside the title-screen sprite scope.
        /// </summary>
        /// <param name="parent">Options panel that owns the return action.</param>
        void CreateTiltPlayOptionsBackButton(Entity parent) {
            Entity buttonEntity = CreateRoundedPanelEntity(parent, "TiltPlayOptionsBackButton", new float3(420f, 520f, 0.1f), new int2(440, 48), 16f, 2f, new byte4(45, 36, 103, 255), new byte4(92, 239, 222, 255), 4);
            buttonEntity.AddComponent(new InteractableComponent {
                Size = new int2(440, 48)
            });
            buttonEntity.AddComponent(new city.game.TiltPlayMenuActionComponent {
                Action = city.game.TiltPlayMenuAction.Back
            });
            CreateUiTextEntity(buttonEntity, "TiltPlayOptionsBackButtonLabel", new float3(12f, 2f, 0.1f), "BACK", new int2(416, 44), 1f, 5, new byte4(247, 248, 252, 255), TextAlignment.Center);
        }

        /// <summary>
        /// Removes a handheld-only selector action entity from every non-handheld platform cook while retaining the authored DS and 3DS version.
        /// </summary>
        /// <param name="entity">Handheld-only selector action entity to exclude from non-handheld cooks.</param>
        void ExcludeHandheldOnlyEntityFromNonHandheldPlatforms(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            string[] nonHandheldPlatformIds = ["windows", "ps2", "psp", "psvita", "gamecube", "wii", "switch", "wiiu"];
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            for (int index = 0; index < nonHandheldPlatformIds.Length; index++) {
                saveComponent.GetOrCreateExistencePlatformOverride(nonHandheldPlatformIds[index]).Exists = false;
            }
        }

        /// <summary>
        /// Creates one handheld Tilt Trial result button backed by a presentation-independent session action.
        /// </summary>
        /// <param name="parent">Results panel that owns the button.</param>
        /// <param name="name">Stable result button presentation role.</param>
        /// <param name="position">Button position in the handheld reference canvas.</param>
        /// <param name="size">Button dimensions in authored pixels.</param>
        /// <param name="label">Visible button label.</param>
        /// <param name="action">Semantic action emitted by the button.</param>
        void CreateTiltTrialResultActionButton(Entity parent, string name, float3 position, int2 size, string label, city.game.TiltTrialSessionAction action) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Result button role must be provided.", nameof(name));
            } else if (string.IsNullOrWhiteSpace(label)) {
                throw new ArgumentException("Result button label must be provided.", nameof(label));
            }

            Entity buttonEntity = CreateRoundedPanelEntity(parent, name, position, size, 5f, 0f, new byte4(40, 58, 87, 255), new byte4(0, 0, 0, 0), 5);
            buttonEntity.AddComponent(new InteractableComponent {
                Size = size
            });
            buttonEntity.AddComponent(new city.game.TiltTrialPresentationActionComponent {
                Action = action
            });
            CreateUiTextEntity(buttonEntity, name + "Label", new float3(8f, 5f, 0.1f), label, new int2(size.X - 16, size.Y - 8), 0.7f, 6, new byte4(247, 248, 252, 255), TextAlignment.Center);
        }

        /// <summary>
        /// Creates one generated authored Tilt Trial gameplay scene from the supplied shared level metadata entry.
        /// </summary>
        /// <param name="levelEntry">Shared level entry defining scene id and timer metadata.</param>
        /// <returns>Generated authored gameplay scene.</returns>
        GeneratedAuthoringSceneDefinition CreateTiltTrialGameplayScene(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            EditorEntity cameraEntity = CreateCameraEntity();
            EditorEntity stageRootEntity = CreateStageRootEntity(levelEntry);
            EditorEntity playerSphereEntity = CreatePlayerSphereEntity();
            EditorEntity uiEntity = CreateGameplayUiEntity(levelEntry);
            Entity[] roots = new Entity[] {
                CreateLevelMetadataEntity(levelEntry),
                cameraEntity,
                CreateDirectionalLightEntity(),
                CreateDirectionalFillLightEntity(),
                CreateAmbientLightEntity(),
                CreatePhysicsBoundsDebugEntity(),
                uiEntity,
                stageRootEntity,
                CreateCatchFloorEntity(),
                playerSphereEntity
            };

            ConfigureTiltTrialCameraTarget(cameraEntity, playerSphereEntity);
            ConfigureTiltTrialSpeedTextTarget(uiEntity, playerSphereEntity);
            ConfigureTiltTrialGoalTarget(stageRootEntity, playerSphereEntity);
            ConfigureTiltTrialCoinTargets(stageRootEntity, playerSphereEntity);
            return new GeneratedAuthoringSceneDefinition {
                SceneId = levelEntry.SceneId,
                SceneAssetRelativePath = BuildTiltTrialGameplaySceneAssetRelativePath(levelEntry.SceneId),
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = roots
            };
        }

        /// <summary>
        /// Builds the authored project-relative scene path used by one Tilt Trial gameplay scene while preserving its runtime scene id.
        /// </summary>
        /// <param name="sceneId">Stable runtime scene id for the Tilt Trial gameplay scene.</param>
        /// <returns>Project-relative authored scene asset path.</returns>
        static string BuildTiltTrialGameplaySceneAssetRelativePath(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            return TiltTrialGameplaySceneAssetDirectoryRelativePath + "/" + sceneId + ".helen";
        }

        /// <summary>
        /// Creates the authored camera used by the dedicated Tilt Trial selector scene.
        /// </summary>
        /// <returns>Generated selector camera entity.</returns>
        Entity CreateLevelSelectCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevelSelectCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 20f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(18f / 255f, 27f / 255f, 43f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored third-person camera root used by Tilt Trial.
        /// </summary>
        /// <returns>Generated editor camera entity.</returns>
        EditorEntity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(MathF.PI, -0.42f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 2.74425f, -10.92f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 120f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 72f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.game.DemoTiltFollowCameraComponent {
                TargetEntityName = "PlayerSphere",
                TargetEntityRole = "PlayerSphere",
                TargetOffset = new float3(0f, 0.65f, 0f)
            });

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial camera generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the authored directional light used by Tilt Trial.
        /// </summary>
        /// <returns>Generated light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.6f, -0.95f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 8f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.97f, 0.92f, 1f),
                Intensity = 1.15f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f,
                ShadowDistance = 30f
            });
            return entity;
        }

        /// <summary>
        /// Creates one weaker directional fill light that lifts the sphere's unlit hemisphere without adding a second shadow pass.
        /// </summary>
        /// <returns>Generated fill-light entity.</returns>
        Entity CreateDirectionalFillLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(2.45f, -0.32f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialFill");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 6f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(0.78f, 0.84f, 1f, 1f),
                Intensity = 0.7f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Disabled,
                ShadowStrength = 0f,
                ShadowDistance = 0f
            });
            return entity;
        }

        /// <summary>
        /// Creates one low-intensity ambient light so small collectibles do not collapse to flat black or gray when they rotate away from the key lights.
        /// </summary>
        /// <returns>Generated ambient-light entity.</returns>
        Entity CreateAmbientLightEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialAmbient");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new AmbientLightComponent {
                Color = new float4(1f, 0.95f, 0.82f, 1f),
                Intensity = 0.18f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Disabled,
                ShadowStrength = 0f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root that drives one Tilt Trial level selector.
        /// </summary>
        /// <param name="useOwnViewport">Whether the selector is a standalone scene root that must fit itself to the live viewport.</param>
        /// <returns>Generated selector UI root entity.</returns>
        EditorEntity CreateLevelSelectUiEntity(bool useOwnViewport) {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevelSelectUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new city.game.TiltTrialLevelSelectComponent {
                UseDetailsStage = false
            });
            if (useOwnViewport) {
                entity.AddComponent(new ViewportComponent {
                    BindingMode = ViewportComponent.ScreenBindingMode,
                    FixedSize = new int2(1280, 720)
                });
                entity.AddComponent(new ReferenceCanvasFitComponent {
                    ReferenceWidth = 1280,
                    ReferenceHeight = 720
                });
            }

            Entity listPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialLevelSelectListPanel", new float3(40f, 32f, 0f), new int2(420, 656), 28f, 3f, new byte4(26, 40, 61, 255), new byte4(96, 128, 168, 255), 1);
            Entity detailsPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialLevelSelectDetailsPanel", new float3(500f, 32f, 0f), new int2(740, 576), 28f, 3f, new byte4(26, 40, 61, 255), new byte4(96, 128, 168, 255), 1);

            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectName", new float3(28f, 24f, 0.1f), "Level 1", new int2(420, 56), 2.2f, 3, new byte4(247, 248, 252, 255), TextAlignment.Left);
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectTimer", new float3(28f, 110f, 0.1f), "Limit 99.00", new int2(220, 36), 1.4f, 3, new byte4(255, 214, 138, 255), TextAlignment.Left);
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectMedals", new float3(28f, 156f, 0.1f), "Gold  18.00\nSilver 28.00\nBronze 40.00", new int2(260, 120), 1.2f, 3, new byte4(223, 230, 239, 255), TextAlignment.Left);
            Entity previewPanelEntity = CreateRoundedPanelEntity(
                detailsPanelEntity,
                "TiltTrialLevelSelectPreviewPanel",
                new float3(392f, 28f, 0f),
                new int2(320, 260),
                18f,
                2f,
                new byte4(18, 29, 45, 255),
                new byte4(109, 138, 170, 255),
                3);
            CreateUiTextEntity(
                previewPanelEntity,
                "TiltTrialLevelSelectPreviewText",
                new float3(20f, 108f, 0.1f),
                "Preview",
                new int2(280, 40),
                1.2f,
                4,
                new byte4(223, 230, 239, 255),
                TextAlignment.Center);

            IReadOnlyList<global::city.game.TiltTrialLevelCatalogEntry> levelEntries = global::city.game.TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < levelEntries.Count; index++) {
                float top = 22f + (index * 94f);
                int oneBasedIndex = index + 1;
                Entity rowEntity = CreateRoundedPanelEntity(listPanelEntity, $"TiltTrialLevelRow{oneBasedIndex:00}", new float3(24f, top, 0f), new int2(372, 76), 18f, 2f, new byte4(40, 58, 87, 255), new byte4(109, 138, 170, 255), 2);
                CreateUiTextEntity(rowEntity, $"TiltTrialLevelRow{oneBasedIndex:00}Label", new float3(20f, 18f, 0.1f), levelEntries[index].DisplayName, new int2(320, 40), 1.55f, 3, new byte4(247, 248, 252, 255), TextAlignment.Left);
            }

            Entity backButtonEntity = CreateLevelSelectActionButton(detailsPanelEntity, "TiltTrialLevelSelectBackButton", new float3(28f, 430f, 0f), new int2(320, 56), "BACK", city.game.TiltTrialLevelSelectAction.BackToStages, -1);
            Entity playButtonEntity = CreateLevelSelectActionButton(detailsPanelEntity, "TiltTrialLevelSelectPlayButton", new float3(28f, 500f, 0f), new int2(320, 56), "PLAY", city.game.TiltTrialLevelSelectAction.PlaySelectedStage, -1);
            CreateLevelSelectActionPrompt(entity, "TiltTrialLevelSelectPlayPrompt", new float3(848f, 638f, 0f), "enter", "PLAY");
            CreateLevelSelectActionPrompt(entity, "TiltTrialLevelSelectMenuPrompt", new float3(1056f, 638f, 0f), "escape", "MENU");
            ExcludeHandheldOnlyEntityFromNonHandheldPlatforms(backButtonEntity);
            ExcludeHandheldOnlyEntityFromNonHandheldPlatforms(playButtonEntity);

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.GetOrCreateExistencePlatformOverride("ds").Exists = false;
            saveComponent.GetOrCreateExistencePlatformOverride("3ds").Exists = false;

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial selector generation requires editor-authored entities.");
        }

        /// <summary>
        /// Persists the native 3DS viewport dimensions for one shared handheld viewport component.
        /// </summary>
        /// <param name="entity">Entity that owns the shared viewport component.</param>
        /// <param name="commonComponent">Shared DS-baseline viewport component.</param>
        /// <param name="width">Native 3DS screen width for the viewport.</param>
        /// <param name="height">Native 3DS screen height for the viewport.</param>
        /// <param name="referenceWidth">Authored reference width that should be mapped into the native viewport.</param>
        /// <param name="referenceHeight">Authored reference height that should be mapped into the native viewport.</param>
        void ApplyNintendo3DsViewportOverride(Entity entity, ViewportComponent commonComponent, int width, int height, int referenceWidth, int referenceHeight) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (commonComponent == null) {
                throw new ArgumentNullException(nameof(commonComponent));
            } else if (width <= 0 || height <= 0 || referenceWidth <= 0 || referenceHeight <= 0) {
                throw new ArgumentOutOfRangeException(nameof(width), "3DS viewport dimensions must be positive.");
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            ViewportComponent overrideComponent = (ViewportComponent)PlatformEditingServiceValue.EnsurePlatformOverrideComponent(
                commonComponent,
                saveComponent,
                Nintendo3DsPlatformId);
            overrideComponent.FixedSize = new int2(width, height);
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, Nintendo3DsPlatformId, nameof(ViewportComponent.FixedSize));
            overrideComponent.ReferenceWidth = referenceWidth;
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, Nintendo3DsPlatformId, nameof(ViewportComponent.ReferenceWidth));
            overrideComponent.ReferenceHeight = referenceHeight;
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, Nintendo3DsPlatformId, nameof(ViewportComponent.ReferenceHeight));
            PlatformEditingServiceValue.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, Nintendo3DsPlatformId);
        }

        /// <summary>
        /// Persists the native 3DS reference canvas for one shared top-screen fit component.
        /// </summary>
        /// <param name="entity">Entity that owns the shared reference-canvas component.</param>
        /// <param name="commonComponent">Shared DS-baseline reference-canvas component.</param>
        /// <param name="width">Native 3DS screen width for the reference canvas.</param>
        /// <param name="height">Native 3DS screen height for the reference canvas.</param>
        void ApplyNintendo3DsReferenceCanvasOverride(Entity entity, ReferenceCanvasFitComponent commonComponent, int width, int height) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (commonComponent == null) {
                throw new ArgumentNullException(nameof(commonComponent));
            } else if (width <= 0 || height <= 0) {
                throw new ArgumentOutOfRangeException(nameof(width), "3DS reference-canvas dimensions must be positive.");
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            ReferenceCanvasFitComponent overrideComponent = (ReferenceCanvasFitComponent)PlatformEditingServiceValue.EnsurePlatformOverrideComponent(
                commonComponent,
                saveComponent,
                Nintendo3DsPlatformId);
            overrideComponent.ReferenceWidth = width;
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, Nintendo3DsPlatformId, nameof(ReferenceCanvasFitComponent.ReferenceWidth));
            overrideComponent.ReferenceHeight = height;
            PlatformEditingServiceValue.MarkPropertyOverride(commonComponent, saveComponent, Nintendo3DsPlatformId, nameof(ReferenceCanvasFitComponent.ReferenceHeight));
            PlatformEditingServiceValue.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, Nintendo3DsPlatformId);
        }

        /// <summary>
        /// Creates one hidden metadata root entity that stores the shared authored level settings for a generated gameplay scene.
        /// </summary>
        /// <param name="levelEntry">Shared level entry defining scene id and timer metadata.</param>
        /// <returns>Generated metadata root entity.</returns>
        Entity CreateLevelMetadataEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevelMetadata");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new city.game.TiltTrialLevelSettingsComponent {
                LevelId = levelEntry.LevelId,
                DisplayName = levelEntry.DisplayName,
                SceneId = levelEntry.SceneId,
                StartTimeSeconds = levelEntry.StartTimeSeconds,
                GoldTimeSeconds = levelEntry.GoldTimeSeconds,
                SilverTimeSeconds = levelEntry.SilverTimeSeconds,
                BronzeTimeSeconds = levelEntry.BronzeTimeSeconds,
                PreviewTexturePath = levelEntry.PreviewTexturePath
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root that owns gameplay HUD and session overlay behavior while a Tilt Trial level is active.
        /// </summary>
        /// <returns>Generated UI root entity.</returns>
        EditorEntity CreateGameplayUiEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new city.game.TiltTrialSessionComponent());
            entity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(1280, 720)
            });
            entity.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = 1280,
                ReferenceHeight = 720
            });
            new DemoDiscSceneLabelOverlayFactory(AssetAuthoringService).AttachToSceneUi(
                entity,
                ResolveRequiredEditorFont(),
                levelEntry.DisplayName);

            CreateUiTextEntity(entity, "TiltTrialTimerText", new float3(530f, 16f, 0f), global::city.game.TiltTrialLevelSelectComponent.FormatTimerSeconds(levelEntry.StartTimeSeconds), new int2(220, 56), 2.2f, 1, new byte4(255, 246, 223, 255), TextAlignment.Center);

            Entity speedTextEntity = Core.Instance.EntityFactory.CreateChild(entity, "TiltTrialSpeedText");
            speedTextEntity.LocalPosition = new float3(16f, 600f, 0f);
            speedTextEntity.Static = false;
            TextComponent speedTextComponent = new TextComponent {
                Text = "0\nkm/h",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(320, 224),
                FontScale = 2.2f,
                Alignment = TextAlignment.Center,
                RenderOrder2D = 1,
            };
            speedTextEntity.AddComponent(speedTextComponent);
            LayoutComponent speedTextAnchorComponent = new LayoutComponent();
            speedTextAnchorComponent.LayoutSpace = LayoutComponent.CameraViewportLayoutSpace;
            speedTextAnchorComponent.SetAnchorDistances(left: 16f, bottom: 16f);
            speedTextEntity.AddComponent(speedTextAnchorComponent);
            ApplyFontReference(speedTextEntity, speedTextComponent, TiltTrialSpeedHudFontRelativePath);
            speedTextEntity.AddComponent(new city.game.DemoTiltSpeedTextComponent {
                TargetEntityName = "PlayerSphere",
                TargetEntityRole = "PlayerSphere"
            });

            Entity startOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialStartOverlay", new float3(320f, 260f, 0f), new int2(640, 150), 28f, 3f, new byte4(18, 27, 43, 238), new byte4(255, 214, 138, 255), 4);
            CreateTiltTrialStartPrompt(startOverlayEntity, new float3(36f, 48f, 0.1f), new int2(568, 48), 2f, 5, new int2(48, 48));

            Entity resultsOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialResultsOverlay", new float3(320f, 130f, 0f), new int2(640, 380), 28f, 3f, new byte4(18, 27, 43, 238), new byte4(255, 214, 138, 255), 4);
            resultsOverlayEntity.Enabled = false;
            CreateUiTextEntity(resultsOverlayEntity, "TiltTrialResultsTitleText", new float3(36f, 28f, 0.1f), "Clear", new int2(360, 42), 2f, 5, new byte4(255, 236, 196, 255), TextAlignment.Left);
            CreateUiTextEntity(resultsOverlayEntity, "TiltTrialResultsBodyText", new float3(36f, 86f, 0.1f), "Time 00.00", new int2(420, 220), 1.35f, 5, new byte4(247, 248, 252, 255), TextAlignment.Left);

            Entity failOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialFailOverlay", new float3(360f, 210f, 0f), new int2(560, 220), 28f, 3f, new byte4(43, 23, 28, 238), new byte4(214, 112, 112, 255), 4);
            failOverlayEntity.Enabled = false;
            CreateUiTextEntity(failOverlayEntity, "TiltTrialFailTitleText", new float3(36f, 28f, 0.1f), "Time Up", new int2(280, 42), 2f, 5, new byte4(255, 223, 223, 255), TextAlignment.Left);
            CreateUiTextEntity(failOverlayEntity, "TiltTrialFailBodyText", new float3(36f, 86f, 0.1f), "Retry", new int2(320, 96), 1.35f, 5, new byte4(247, 248, 252, 255), TextAlignment.Left);

            Entity coinTextEntity = Core.Instance.EntityFactory.CreateChild(entity, "TiltTrialCoinText");
            coinTextEntity.LocalPosition = new float3(16f, 16f, 0f);
            coinTextEntity.Static = false;
            TextComponent coinTextComponent = new TextComponent {
                Text = "Coins 0/0",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(255, 246, 223, 255),
                Size = new int2(280, 44),
                FontScale = 1.45f,
                Alignment = TextAlignment.Left,
                RenderOrder2D = 1,
            };
            coinTextEntity.AddComponent(coinTextComponent);
            LayoutComponent coinTextAnchorComponent = new LayoutComponent();
            coinTextAnchorComponent.LayoutSpace = LayoutComponent.CameraViewportLayoutSpace;
            coinTextAnchorComponent.SetAnchorDistances(left: 16f, top: 16f);
            coinTextEntity.AddComponent(coinTextAnchorComponent);
            ApplyFontReference(coinTextEntity, coinTextComponent, TiltTrialSpeedHudFontRelativePath);
            coinTextEntity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = "TiltTrialCoinText"
            });

            Entity physicsBoundsStatusTextEntity = Core.Instance.EntityFactory.CreateChild(entity, "TiltTrialPhysicsBoundsStatusText");
            physicsBoundsStatusTextEntity.LocalPosition = new float3(16f, 56f, 0f);
            physicsBoundsStatusTextEntity.Static = false;
            TextComponent physicsBoundsStatusTextComponent = new TextComponent {
                Text = "F3 Bounds Off",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(196, 210, 226, 255),
                Size = new int2(280, 36),
                FontScale = 1.1f,
                Alignment = TextAlignment.Left,
                RenderOrder2D = 1,
            };
            physicsBoundsStatusTextEntity.AddComponent(physicsBoundsStatusTextComponent);
            LayoutComponent physicsBoundsStatusAnchorComponent = new LayoutComponent();
            physicsBoundsStatusAnchorComponent.LayoutSpace = LayoutComponent.CameraViewportLayoutSpace;
            physicsBoundsStatusAnchorComponent.SetAnchorDistances(left: 16f, top: 56f);
            physicsBoundsStatusTextEntity.AddComponent(physicsBoundsStatusAnchorComponent);
            ApplyFontReference(physicsBoundsStatusTextEntity, physicsBoundsStatusTextComponent, TiltTrialSpeedHudFontRelativePath);
            physicsBoundsStatusTextEntity.AddComponent(new city.game.TiltTrialPhysicsBoundsStatusTextComponent());
            EntitySaveComponent physicsBoundsStatusTextEntitySaveComponent = FindRequiredEntitySaveComponent(physicsBoundsStatusTextEntity);
            string[] nonWindowsPlatformIds = ["ps2", "psp", "psvita", "gamecube", "wii", "wiiu", "switch", "ds", "3ds"];
            for (int platformIndex = 0; platformIndex < nonWindowsPlatformIds.Length; platformIndex++) {
                physicsBoundsStatusTextEntitySaveComponent.GetOrCreateExistencePlatformOverride(nonWindowsPlatformIds[platformIndex]).Exists = false;
            }
            physicsBoundsStatusTextEntitySaveComponent.GetOrCreateExistencePlatformOverride(new global::helengine.EditorOverrideScope("windows", "release")).Exists = false;

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial UI generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the Windows-only runtime physics-bounds debug host used by Tilt Trial gameplay scenes.
        /// </summary>
        /// <returns>Generated debug-root entity.</returns>
        Entity CreatePhysicsBoundsDebugEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialPhysicsBoundsDebug");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new global::city.game.TiltTrialPhysicsBoundsDebugDrawComponent());
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.GetOrCreateExistencePlatformOverride(new global::helengine.EditorOverrideScope("windows", "release")).Exists = false;
            return entity;
        }

        /// <summary>
        /// Creates the authored stage root that owns the runtime tilt controller and the kinematic support geometry it manipulates.
        /// </summary>
        /// <returns>Generated editor stage root.</returns>
        EditorEntity CreateStageRootEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            if (string.Equals(levelEntry.LevelId, "tilt-trial-01", StringComparison.Ordinal)) {
                return CreateTiltTrialLevel01StageRootEntity();
            } else if (string.Equals(levelEntry.LevelId, "tilt-trial-02", StringComparison.Ordinal)) {
                return CreateTiltTrialLevel02StageRootEntity();
            } else if (string.Equals(levelEntry.LevelId, "tilt-trial-03", StringComparison.Ordinal)) {
                return CreateTiltTrialLevel03StageRootEntity();
            } else if (string.Equals(levelEntry.LevelId, "tilt-trial-04", StringComparison.Ordinal)) {
                return CreateTiltTrialLevel04StageRootEntity();
            } else if (string.Equals(levelEntry.LevelId, "tilt-trial-05", StringComparison.Ordinal)) {
                return CreateTiltTrialLevel05StageRootEntity();
            }

            return CreateStageRootEntity();
        }

        /// <summary>
        /// Creates the default authored stage root used by non-specialized Tilt Trial levels.
        /// </summary>
        /// <returns>Generated editor stage root.</returns>
        EditorEntity CreateStageRootEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("StageRoot");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new city.game.DemoTiltStageComponent {
                MaximumPlanarSpeed = 11.25f,
                PlanarAccelerationUnitsPerSecond = 4.25f
            });
            entity.AddChild(CreateStartPadEntity());
            entity.AddChild(CreateRampEntity());
            entity.AddChild(CreateGoalFlagEntity(new float3(0f, -1.7f, 10.5f)));
            entity.AddChild(CreateLeftWallEntity());
            entity.AddChild(CreateRightWallEntity());

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial stage generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the dedicated beginner layout used by the first Tilt Trial level.
        /// </summary>
        /// <returns>Generated editor stage root for level 1.</returns>
        EditorEntity CreateTiltTrialLevel01StageRootEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("StageRoot");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new city.game.DemoTiltStageComponent {
                MaximumPlanarSpeed = 11.25f,
                PlanarAccelerationUnitsPerSecond = 4.25f
            });
            entity.AddChild(CreateLevel01StartPadEntity());
            entity.AddChild(CreateLevel01RampEntity());
            entity.AddChild(CreateLevel01BridgeEntity());
            entity.AddChild(CreateLevel01BlockerLeftEntity());
            entity.AddChild(CreateLevel01BlockerRightEntity());
            entity.AddChild(CreateLevel01FinalPlatformEntity());
            entity.AddChild(CreateGoalFlagEntity());
            entity.AddChild(CreateCollectibleCoinEntity("Coin01", new float3(0f, 1.35f, -2.2f)));
            entity.AddChild(CreateCollectibleCoinEntity("Coin02", new float3(-0.8f, 1.9f, 4.6f)));
            entity.AddChild(CreateCollectibleCoinEntity("Coin03", new float3(1.35f, 1.9f, 13.8f)));
            entity.AddChild(CreateLevel01LeftWallEntity());
            entity.AddChild(CreateLevel01RightWallEntity());
            entity.AddChild(CreateLevel01FinalLeftGuardEntity());
            entity.AddChild(CreateLevel01FinalRightGuardEntity());

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial stage generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the fixed-axis camera used to move directly toward and through the clipping probe cube.
        /// </summary>
        /// <returns>Generated render-test camera entity.</returns>
        EditorEntity CreateLevel01RenderTestCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0.6435011f, -0.3805064f, 0f, out orientation);
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevel01RenderTestCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(6f, 4f, 8f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 160f,
                ClearSettings = new CameraClearSettings(true, new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f), true, 1f, false, 0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 80f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = float3.Zero,
                AutoYawSpeedRadians = 0f,
                ManualYawSpeedRadians = 0f,
                ManualPitchSpeedRadians = 0f
            });
            return RequireEditorEntity(entity, "clipping-probe camera");
        }

        /// <summary>
        /// Creates the FPS-only diagnostic overlay for the Level 1 rendering validation scene.
        /// </summary>
        /// <returns>Generated FPS overlay entity.</returns>
        EditorEntity CreateLevel01RenderTestFpsEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevel01RenderTestFps");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            FPSComponent fpsComponent = new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            };
            entity.AddComponent(fpsComponent);
            ApplyEditorFontReference(entity, fpsComponent);
            return RequireEditorEntity(entity, "Level 1 render-test FPS overlay");
        }

        /// <summary>
        /// Creates the clipping probe root with exactly one authored six-colored-face 5-by-1-by-5 cube.
        /// </summary>
        /// <returns>Generated render-only stage root.</returns>
        EditorEntity CreateLevel01RenderOnlyStageRootEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("Ps2ClippingProbe");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddChild(CreateLevel01RenderOnlyCourseBoxEntity("ClipProbeCube", float3.Zero, new float3(5f, 1f, 5f), float4.Identity));
            return RequireEditorEntity(entity, "single-cube clipping probe");
        }

        /// <summary>
        /// Creates the visible Level 1 player sphere without physics, reset, or session behavior.
        /// </summary>
        /// <returns>Generated visual-only player sphere entity.</returns>
        Entity CreateLevel01RenderOnlyPlayerSphereEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("PlayerSphere");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 1.2f, -7f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedSphereModel,
                Materials = new[] { TiltTrialPlayerSphereMarbleMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialPlayerSphereMaterialReference(entity, meshComponent);
            return entity;
        }

        /// <summary>
        /// Creates one authored colored-face clipping probe cube for the render-only Level 1 scene.
        /// </summary>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Local position.</param>
        /// <param name="scale">Full box dimensions.</param>
        /// <param name="orientation">Local orientation.</param>
        /// <returns>Generated visual-only colored-face clipping probe cube.</returns>
        Entity CreateLevel01RenderOnlyCourseBoxEntity(string name, float3 position, float3 scale, float4 orientation) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Render-only course box names must be provided.", nameof(name));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            entity.LocalOrientation = orientation;
            MeshComponent meshComponent = new MeshComponent {
                Model = TiltTrialClippingProbeModel,
                Materials = new[] { TiltTrialClippingProbeMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialClippingProbeReferences(entity, meshComponent);
            return entity;
        }

        /// <summary>
        /// Stores the authored model and material references required to serialize the isolated colored-face clipping probe mesh.
        /// </summary>
        /// <param name="entity">Generated probe entity that owns the mesh component.</param>
        /// <param name="meshComponent">Probe mesh component assigned to the generated entity.</param>
        void ApplyTiltTrialClippingProbeReferences(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(meshComponent, "Model", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateModel(AssetAuthoringService, TiltTrialClippingProbeModelFactory.ModelRelativePath));
            saveComponent.SetAssetReference(meshComponent, "Materials[0]", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateMaterial(AssetAuthoringService, TiltTrialClippingProbeMaterialFactory.MaterialRelativePath));
        }

        /// <summary>
        /// Stores PS2- and PSP-only MeshComponent tessellation settings for one scaled Level 1 render-test course object.
        /// </summary>
        /// <param name="entity">Entity that owns the MeshComponent and its editor persistence metadata.</param>
        /// <param name="meshComponent">Course MeshComponent that should receive component-only tessellation settings.</param>
        void ApplyConstrainedPlatformTessellation(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            EntityComponentSaveState saveState = saveComponent.GetOrCreateComponentState(meshComponent);
            MeshComponentModifier modifier = new MeshComponentModifier(MeshComponentModifier.TessellateKind) {
                MaxEdgeLength = TiltTrialRenderTestTessellationMaxEdgeLength
            };
            MeshComponentModifierStackServiceValue.SetStack(saveState, Ps2PlatformId, new[] { modifier });
            MeshComponentModifierStackServiceValue.SetStack(saveState, PspPlatformId, new[] { modifier });
        }

        /// <summary>
        /// Creates a visual-only coin using the shared authored golden-coin blueprint.
        /// </summary>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Local position.</param>
        /// <returns>Generated visual-only coin entity.</returns>
        Entity CreateLevel01RenderOnlyCoinEntity(string name, float3 position) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Render-only coin names must be provided.", nameof(name));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = new float3(0.51f, 0.51f, 0.51f);
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GoldenCoinModel,
                Materials = new[] { GoldenCoinMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyRenderOnlyCoinReferences(entity, meshComponent);
            return entity;
        }

        /// <summary>
        /// Creates a visual-only finish flag using the shared authored flag blueprint.
        /// </summary>
        /// <param name="position">Local position.</param>
        /// <returns>Generated visual-only goal flag entity.</returns>
        Entity CreateLevel01RenderOnlyGoalFlagEntity(float3 position) {
            Entity entity = Core.Instance.EntityFactory.Create("GoalFlag");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = new float3(1.2f, 1.2f, 1.2f);
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GoalFlagModel,
                Materials = new[] { GoalFlagPoleMaterial, GoalFlagBannerMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyRenderOnlyGoalFlagReferences(entity, meshComponent);
            return entity;
        }

        /// <summary>
        /// Stores the file-backed model and material references for one standalone coin mesh.
        /// </summary>
        /// <param name="entity">Coin entity receiving the save metadata.</param>
        /// <param name="meshComponent">Coin mesh component receiving the references.</param>
        void ApplyRenderOnlyCoinReferences(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(meshComponent, "Model", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateModel(AssetAuthoringService, SplitPlayAssetCatalog.GoldenCoinCommonModelRelativePath));
            saveComponent.SetAssetReference(meshComponent, "Materials[0]", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateMaterial(AssetAuthoringService, SplitPlayAssetCatalog.GoldenCoinMaterialRelativePath));
        }

        /// <summary>
        /// Stores the file-backed model and material references for one standalone goal-flag mesh.
        /// </summary>
        /// <param name="entity">Goal-flag entity receiving the save metadata.</param>
        /// <param name="meshComponent">Goal-flag mesh component receiving the references.</param>
        void ApplyRenderOnlyGoalFlagReferences(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(meshComponent, "Model", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateModel(AssetAuthoringService, SplitPlayAssetCatalog.GoalFlagCommonModelRelativePath));
            saveComponent.SetAssetReference(meshComponent, "Materials[0]", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateMaterial(AssetAuthoringService, SplitPlayAssetCatalog.GoalFlagPoleMaterialRelativePath));
            saveComponent.SetAssetReference(meshComponent, "Materials[1]", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateMaterial(AssetAuthoringService, SplitPlayAssetCatalog.GoalFlagBannerMaterialRelativePath));
        }

        /// <summary>
        /// Creates the wider offset course used by the second Tilt Trial level.
        /// </summary>
        /// <returns>Generated editor stage root for level 2.</returns>
        EditorEntity CreateTiltTrialLevel02StageRootEntity() {
            Entity entity = CreateTiltTrialStageRoot("Level02StageRoot", 11.75f, 4.5f);
            entity.AddChild(CreateKinematicCourseBoxEntity("Level02StartPad", new float3(0f, 0f, -6.6f), new float3(7f, 1f, 9f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level02Straight", new float3(0f, 0.25f, 0.5f), new float3(5.2f, 0.8f, 7f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level02Offset", new float3(1.45f, 0.55f, 6.2f), new float3(4f, 0.8f, 5.5f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level02Finish", new float3(1.45f, 0.65f, 11.4f), new float3(6f, 0.9f, 5.2f), float4.Identity));
            entity.AddChild(CreateGoalFlagEntity(new float3(1.45f, 1f, 13f)));
            return RequireEditorEntity(entity, "level 2");
        }

        /// <summary>
        /// Creates the alternating narrow-platform course used by the third Tilt Trial level.
        /// </summary>
        /// <returns>Generated editor stage root for level 3.</returns>
        EditorEntity CreateTiltTrialLevel03StageRootEntity() {
            Entity entity = CreateTiltTrialStageRoot("Level03StageRoot", 12.25f, 4.5f);
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03StartPad", new float3(0f, 0f, -6.6f), new float3(6.5f, 1f, 8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03Platform01", new float3(-1.35f, 0.3f, 0.1f), new float3(3.7f, 0.8f, 5.4f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03Platform02", new float3(1.25f, 0.65f, 4.8f), new float3(3.4f, 0.8f, 4.8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03Platform03", new float3(-1.25f, 1f, 9.2f), new float3(3.2f, 0.8f, 4.8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03Finish", new float3(1.35f, 1.35f, 13.7f), new float3(4.6f, 0.9f, 5.2f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03Blocker01", new float3(-1.35f, 1.35f, 1.1f), new float3(0.8f, 1.6f, 0.9f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level03Blocker02", new float3(1.25f, 1.7f, 5.9f), new float3(0.8f, 1.6f, 0.9f), float4.Identity));
            entity.AddChild(CreateGoalFlagEntity(new float3(1.35f, 1.7f, 15.2f)));
            return RequireEditorEntity(entity, "level 3");
        }

        /// <summary>
        /// Creates the stepped turn-and-gap course used by the fourth Tilt Trial level.
        /// </summary>
        /// <returns>Generated editor stage root for level 4.</returns>
        EditorEntity CreateTiltTrialLevel04StageRootEntity() {
            Entity entity = CreateTiltTrialStageRoot("Level04StageRoot", 12.75f, 4.7f);
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04StartPad", new float3(0f, 0f, -6.6f), new float3(6f, 1f, 8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Platform01", new float3(1.5f, 0.35f, -0.1f), new float3(3.2f, 0.8f, 4.8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Platform02", new float3(-1.55f, 0.85f, 4.1f), new float3(3f, 0.8f, 4.2f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Platform03", new float3(1.45f, 1.35f, 8.3f), new float3(2.8f, 0.8f, 4.2f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Finish", new float3(-1.25f, 1.85f, 12.8f), new float3(4.2f, 0.9f, 4.8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Blocker01", new float3(1.5f, 1.45f, 0.5f), new float3(0.75f, 1.8f, 0.8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Blocker02", new float3(-1.55f, 1.95f, 4.7f), new float3(0.75f, 1.8f, 0.8f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level04Blocker03", new float3(1.45f, 2.45f, 8.9f), new float3(0.75f, 1.8f, 0.8f), float4.Identity));
            entity.AddChild(CreateGoalFlagEntity(new float3(-1.25f, 2.25f, 14.1f)));
            return RequireEditorEntity(entity, "level 4");
        }

        /// <summary>
        /// Creates the narrow rising zig-zag course used by the fifth Tilt Trial level.
        /// </summary>
        /// <returns>Generated editor stage root for level 5.</returns>
        EditorEntity CreateTiltTrialLevel05StageRootEntity() {
            Entity entity = CreateTiltTrialStageRoot("Level05StageRoot", 13.25f, 4.9f);
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05StartPad", new float3(0f, 0f, -6.6f), new float3(5.8f, 1f, 7.5f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Platform01", new float3(-1.55f, 0.4f, -0.1f), new float3(2.8f, 0.8f, 3.7f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Platform02", new float3(1.55f, 0.9f, 3.55f), new float3(2.7f, 0.8f, 3.5f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Platform03", new float3(-1.55f, 1.4f, 7.1f), new float3(2.6f, 0.8f, 3.5f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Platform04", new float3(1.55f, 1.9f, 10.65f), new float3(2.5f, 0.8f, 3.5f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Finish", new float3(-1.25f, 2.4f, 14.2f), new float3(3.8f, 0.9f, 4.4f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Blocker01", new float3(-1.55f, 1.5f, 0.2f), new float3(0.7f, 2f, 0.75f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Blocker02", new float3(1.55f, 2f, 3.9f), new float3(0.7f, 2f, 0.75f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Blocker03", new float3(-1.55f, 2.5f, 7.45f), new float3(0.7f, 2f, 0.75f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity("Level05Blocker04", new float3(1.55f, 3f, 11f), new float3(0.7f, 2f, 0.75f), float4.Identity));
            entity.AddChild(CreateGoalFlagEntity(new float3(-1.25f, 2.8f, 15.5f)));
            return RequireEditorEntity(entity, "level 5");
        }

        /// <summary>
        /// Creates common Tilt Trial stage behavior and guide walls for a generated difficulty layout.
        /// </summary>
        /// <param name="name">Stable stage-root entity name.</param>
        /// <param name="maximumPlanarSpeed">Maximum sphere speed for the layout.</param>
        /// <param name="wallX">Absolute guide-wall center offset.</param>
        /// <returns>Generated stage root.</returns>
        Entity CreateTiltTrialStageRoot(string name, float maximumPlanarSpeed, float wallX) {
            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new city.game.DemoTiltStageComponent {
                MaximumPlanarSpeed = maximumPlanarSpeed,
                PlanarAccelerationUnitsPerSecond = 4.25f
            });
            entity.AddChild(CreateKinematicCourseBoxEntity($"{name}LeftWall", new float3(-wallX, 1.3f, 3.5f), new float3(0.75f, 2.8f, 22f), float4.Identity));
            entity.AddChild(CreateKinematicCourseBoxEntity($"{name}RightWall", new float3(wallX, 1.3f, 3.5f), new float3(0.75f, 2.8f, 22f), float4.Identity));
            return entity;
        }

        /// <summary>
        /// Converts a generated stage entity into the editor-authored type required by scene serialization.
        /// </summary>
        /// <param name="entity">Generated stage entity.</param>
        /// <param name="levelDescription">Level description used in the failure message.</param>
        /// <returns>Editor-authored stage root.</returns>
        EditorEntity RequireEditorEntity(Entity entity, string levelDescription) {
            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException($"Tilt Trial {levelDescription} stage generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the authored playable ball entity.
        /// </summary>
        /// <returns>Generated editor sphere entity.</returns>
        EditorEntity CreatePlayerSphereEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("PlayerSphere");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 1.2f, -7f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedSphereModel,
                Materials = new[] { TiltTrialPlayerSphereMarbleMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            entity.AddComponent(new city.game.TiltTrialEntityRoleComponent {
                Role = "PlayerSphere"
            });
            ApplyTiltTrialPlayerSphereMaterialReference(entity, meshComponent);
            entity.AddComponent(new RigidBody3DComponent {
                BodyKind = BodyKind3D.Dynamic,
                UseGravity = true,
                Mass = 1d
            });
            entity.AddComponent(new SphereCollider3DComponent {
                Radius = 0.5f
            });
            entity.AddComponent(new global::city.game.TiltTrialPhysicsDebugSphereBoundsComponent {
                Radius = 0.5f
            });
            entity.AddComponent(new city.game.DemoTiltBallResetComponent {
                SpawnPosition = new float3(0f, 1.2f, -7f),
                SpawnOrientation = float4.Identity,
                ResetHeight = -12f
            });

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial player generation requires editor-authored entities.");
        }

        /// <summary>
        /// Stores the stable authored walnut material reference required by scene serialization on the generated player sphere mesh.
        /// </summary>
        /// <param name="entity">Generated player sphere entity that owns the mesh component.</param>
        /// <param name="meshComponent">Mesh component assigned to the generated player sphere.</param>
        void ApplyTiltTrialPlayerSphereMaterialReference(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                meshComponent,
                PlayerSphereMaterialReferenceName,
                global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateMaterial(AssetAuthoringService, TiltTrialPlayerSphereMarbleMaterialRelativePath));
        }

        /// <summary>
        /// Creates the visual-only floor that catches the eye while still allowing the playable sphere to fall through and reset.
        /// </summary>
        /// <returns>Generated floor entity.</returns>
        Entity CreateCatchFloorEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("CatchFloor");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, -14f, 0f);
            entity.LocalScale = new float3(24f, 1f, 24f);
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedCubeModel,
                Materials = new[] { TiltTrialCourseMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialCourseMaterialReference(entity, meshComponent);
            return entity;
        }

        /// <summary>
        /// Creates the starting support platform at the beginning of the Tilt Trial course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateStartPadEntity() {
            return CreateKinematicCourseBoxEntity("StartPad", new float3(0f, 0f, -7f), new float3(6f, 1f, 8f), float4.Identity);
        }

        /// <summary>
        /// Creates the larger beginner starting platform used by level 1.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01StartPadEntity() {
            return CreateKinematicCourseBoxEntity("StartPad", new float3(0f, 0f, -6.6f), new float3(7f, 1f, 7f), float4.Identity, true);
        }

        /// <summary>
        /// Creates the central ramp that accelerates the player sphere through the middle of the course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateRampEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.22f, 0f, out orientation);
            return CreateKinematicCourseBoxEntity("Ramp", new float3(0f, -0.2f, 1f), new float3(6f, 1f, 12f), orientation);
        }

        /// <summary>
        /// Creates the gentle tutorial ramp that introduces forward carry in level 1.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01RampEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.14f, 0f, out orientation);
            return CreateKinematicCourseBoxEntity("Ramp", new float3(0f, -0.05f, -0.1f), new float3(6f, 0.9f, 8f), orientation, true);
        }

        /// <summary>
        /// Creates the narrow bridge that gently introduces steering precision in level 1.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01BridgeEntity() {
            return CreateKinematicCourseBoxEntity("Bridge", new float3(0f, 0.5f, 5.8f), new float3(2.5f, 1f, 11.5f), float4.Identity, true);
        }

        /// <summary>
        /// Creates the first low blocker on the beginner bridge.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01BlockerLeftEntity() {
            return CreateKinematicCourseBoxEntity("BridgeBlockerLeft", new float3(-0.95f, 1.25f, 3.2f), new float3(1.1f, 1.5f, 1.1f), float4.Identity);
        }

        /// <summary>
        /// Creates the second low blocker on the beginner bridge.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01BlockerRightEntity() {
            return CreateKinematicCourseBoxEntity("BridgeBlockerRight", new float3(0.95f, 1.25f, 7.3f), new float3(1.1f, 1.5f, 1.1f), float4.Identity);
        }

        /// <summary>
        /// Creates the final wide landing platform used by level 1.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01FinalPlatformEntity() {
            return CreateKinematicCourseBoxEntity("FinalPlatform", new float3(1.35f, 0.2f, 13.8f), new float3(8.4f, 1f, 8.8f), float4.Identity, true);
        }

        /// <summary>
        /// Creates the finish platform at the far end of the Tilt Trial course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        /// <summary>
        /// Creates the left guide wall that keeps the player sphere on the authored course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLeftWallEntity() {
            return CreateKinematicCourseBoxEntity("LeftWall", new float3(-3.75f, 0.9f, 1.8f), new float3(1f, 2.5f, 24f), float4.Identity);
        }

        /// <summary>
        /// Creates the long left guide wall used by the first Tilt Trial level.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01LeftWallEntity() {
            return CreateKinematicCourseBoxEntity("LeftWall", new float3(-3.1f, 1.25f, 2.8f), new float3(0.8f, 2.8f, 19.8f), float4.Identity, true);
        }

        /// <summary>
        /// Creates the right guide wall that keeps the player sphere on the authored course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateRightWallEntity() {
            return CreateKinematicCourseBoxEntity("RightWall", new float3(3.75f, 0.9f, 1.8f), new float3(1f, 2.5f, 24f), float4.Identity);
        }

        /// <summary>
        /// Creates the long right guide wall used by the first Tilt Trial level.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01RightWallEntity() {
            return CreateKinematicCourseBoxEntity("RightWall", new float3(3.1f, 1.25f, 2.8f), new float3(0.8f, 2.8f, 19.8f), float4.Identity, true);
        }

        /// <summary>
        /// Creates the left-side final guard near the finish platform on level 1.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01FinalLeftGuardEntity() {
            return CreateKinematicCourseBoxEntity("FinalLeftGuard", new float3(-2.5f, 1.25f, 14.2f), new float3(0.8f, 2.8f, 7.4f), float4.Identity);
        }

        /// <summary>
        /// Creates the right-side final guard near the finish platform on level 1.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLevel01FinalRightGuardEntity() {
            return CreateKinematicCourseBoxEntity("FinalRightGuard", new float3(5.2f, 1.25f, 14.2f), new float3(0.8f, 2.8f, 7.4f), float4.Identity);
        }

        /// <summary>
        /// Creates one collectible coin blueprint instance with a trigger observer bound later to the player sphere.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="position">Local scene position.</param>
        /// <returns>Generated collectible coin entity.</returns>
        Entity CreateCollectibleCoinEntity(string name, float3 position) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Collectible coin name must be provided.", nameof(name));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = new float3(0.51f, 0.51f, 0.51f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new BlueprintInstanceComponent {
                BlueprintAssetReference = AssetAuthoringService.CreateFileReference(
                    SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath,
                    AssetEntryKind.Blueprint)
            });
            return entity;
        }

        /// <summary>
        /// Creates the visual finish flag blueprint instance used by the beginner first level.
        /// </summary>
        /// <returns>Generated finish flag entity.</returns>
        Entity CreateGoalFlagEntity() {
            return CreateGoalFlagEntity(new float3(1.35f, 0.65f, 16.6f));
        }

        /// <summary>
        /// Creates the visual finish flag blueprint instance at the supplied course location.
        /// </summary>
        /// <param name="position">Local position of the flag.</param>
        /// <returns>Generated finish flag entity.</returns>
        Entity CreateGoalFlagEntity(float3 position) {
            Entity entity = Core.Instance.EntityFactory.Create("GoalFlag");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = new float3(1.2f, 1.2f, 1.2f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new BlueprintInstanceComponent {
                BlueprintAssetReference = AssetAuthoringService.CreateFileReference(
                    SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath,
                    AssetEntryKind.Blueprint)
            });
            return entity;
        }

        /// <summary>
        /// Creates one mesh-backed static stage box used by the Tilt Trial course.
        /// </summary>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Local stage position.</param>
        /// <param name="scale">Full box size.</param>
        /// <param name="orientation">Local stage orientation.</param>
        /// <param name="enableConstrainedPlatformTessellation">Whether PS2 and PSP should generate a component-specific tessellated model variant.</param>
        /// <returns>Generated kinematic course entity.</returns>
        Entity CreateKinematicCourseBoxEntity(string name, float3 position, float3 scale, float4 orientation, bool enableConstrainedPlatformTessellation = false) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Course box names must be provided.", nameof(name));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            entity.LocalOrientation = orientation;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedCubeModel,
                Materials = new[] { TiltTrialCourseMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialCourseMaterialReference(entity, meshComponent);
            if (enableConstrainedPlatformTessellation) {
                ApplyConstrainedPlatformTessellation(entity, meshComponent);
            }
            entity.AddComponent(new RigidBody3DComponent {
                BodyKind = BodyKind3D.Static,
                UseGravity = false,
                Mass = 1d
            });
            entity.AddComponent(new BoxCollider3DComponent {
                // Collider sizes compose with entity scale, so the unit box tracks editor rescales.
                Size = float3.One
            });
            entity.AddComponent(new global::city.game.TiltTrialPhysicsDebugBoxBoundsComponent {
                Size = float3.One
            });
            return entity;
        }

        /// <summary>
        /// Stores the stable authored Tilt Trial course material reference required by scene serialization on one generated course mesh.
        /// </summary>
        /// <param name="entity">Generated course entity that owns the mesh component.</param>
        /// <param name="meshComponent">Mesh component assigned to the generated course entity.</param>
        void ApplyTiltTrialCourseMaterialReference(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                meshComponent,
                CourseMaterialReferenceName,
                global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateMaterial(AssetAuthoringService, TiltTrialCourseMaterialRelativePath));
        }

        /// <summary>
        /// Wires the generated third-person camera to the generated player sphere after fresh scene ids have been assigned.
        /// </summary>
        /// <param name="cameraEntity">Generated camera entity that owns the follow-camera component.</param>
        /// <param name="playerSphereEntity">Generated player sphere entity being followed.</param>
        void ConfigureTiltTrialCameraTarget(EditorEntity cameraEntity, EditorEntity playerSphereEntity) {
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            } else if (playerSphereEntity == null) {
                throw new ArgumentNullException(nameof(playerSphereEntity));
            }

            city.game.DemoTiltFollowCameraComponent followCameraComponent = FindRequiredFollowCameraComponent(cameraEntity);
            EntitySaveComponent playerSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            followCameraComponent.TargetEntityReference = new SceneEntityReference {
                EntityId = playerSaveComponent.EntityId
            };
        }

        /// <summary>
        /// Wires the generated speed HUD to the generated player sphere after fresh scene ids have been assigned.
        /// </summary>
        /// <param name="uiEntity">Generated UI root that owns the speed HUD text entity.</param>
        /// <param name="playerSphereEntity">Generated player sphere whose live speed should be displayed.</param>
        void ConfigureTiltTrialSpeedTextTarget(EditorEntity uiEntity, EditorEntity playerSphereEntity) {
            if (uiEntity == null) {
                throw new ArgumentNullException(nameof(uiEntity));
            } else if (playerSphereEntity == null) {
                throw new ArgumentNullException(nameof(playerSphereEntity));
            }

            city.game.DemoTiltSpeedTextComponent speedTextComponent = FindRequiredSpeedTextComponent(uiEntity);
            EntitySaveComponent playerSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            speedTextComponent.TargetEntityReference = new SceneEntityReference {
                EntityId = playerSaveComponent.EntityId
            };
        }

        /// <summary>
        /// Wires the generated goal trigger observer to the generated player sphere after fresh scene ids have been assigned.
        /// </summary>
        /// <param name="stageRootEntity">Generated stage root that owns the goal trigger entity.</param>
        /// <param name="playerSphereEntity">Generated player sphere whose overlap should complete the level.</param>
        void ConfigureTiltTrialGoalTarget(EditorEntity stageRootEntity, EditorEntity playerSphereEntity) {
            if (stageRootEntity == null) {
                throw new ArgumentNullException(nameof(stageRootEntity));
            } else if (playerSphereEntity == null) {
                throw new ArgumentNullException(nameof(playerSphereEntity));
            }

            EntitySaveComponent playerSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            if (stageRootEntity.Children == null) {
                throw new InvalidOperationException("Tilt Trial goal configuration requires stage children.");
            }

            for (int childIndex = 0; childIndex < stageRootEntity.Children.Count; childIndex++) {
                Entity child = stageRootEntity.Children[childIndex];
                if (!TryFindComponent<BlueprintInstanceComponent>(child, out BlueprintInstanceComponent blueprintInstance) ||
                    !string.Equals(blueprintInstance.BlueprintAssetReference?.RelativePath, SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath, StringComparison.Ordinal)) {
                    continue;
                }

                blueprintInstance.EntityReferenceOverrides = [
                    new BlueprintEntityReferenceOverrideAsset {
                        SourceEntityId = 1u,
                        ComponentKey = SplitPlayGoalFlagAssetGenerator.TriggerObserverComponentKey,
                        PropertyName = "TargetEntityReference",
                        TargetEntityId = playerSaveComponent.EntityId
                    }
                ];
                return;
            }

            throw new InvalidOperationException("Tilt Trial goal configuration requires one goal-flag blueprint instance.");
        }

        /// <summary>
        /// Wires all generated collectible-coin trigger observers to the generated player sphere after fresh scene ids have been assigned.
        /// </summary>
        /// <param name="stageRootEntity">Generated stage root that owns the collectible coin entities.</param>
        /// <param name="playerSphereEntity">Generated player sphere whose overlap should collect the coins.</param>
        void ConfigureTiltTrialCoinTargets(EditorEntity stageRootEntity, EditorEntity playerSphereEntity) {
            if (stageRootEntity == null) {
                throw new ArgumentNullException(nameof(stageRootEntity));
            } else if (playerSphereEntity == null) {
                throw new ArgumentNullException(nameof(playerSphereEntity));
            }

            EntitySaveComponent playerSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            if (stageRootEntity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < stageRootEntity.Children.Count; childIndex++) {
                Entity child = stageRootEntity.Children[childIndex];
                if (!TryFindComponent<BlueprintInstanceComponent>(child, out BlueprintInstanceComponent blueprintInstance) ||
                    !string.Equals(blueprintInstance.BlueprintAssetReference?.RelativePath, SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath, StringComparison.Ordinal)) {
                    continue;
                }

                blueprintInstance.EntityReferenceOverrides = [
                    new BlueprintEntityReferenceOverrideAsset {
                        SourceEntityId = 1u,
                        ComponentKey = SplitPlayGoldenCoinAssetGenerator.TriggerObserverComponentKey,
                        PropertyName = "TargetEntityReference",
                        TargetEntityId = playerSaveComponent.EntityId
                    }
                ];
            }
        }

        /// <summary>
        /// Resolves the generated Tilt Trial follow camera attached to the supplied camera root.
        /// </summary>
        /// <param name="cameraEntity">Generated camera entity whose follow-camera component should be returned.</param>
        /// <returns>Attached Tilt Trial follow camera component.</returns>
        city.game.DemoTiltFollowCameraComponent FindRequiredFollowCameraComponent(EditorEntity cameraEntity) {
            if (cameraEntity == null || cameraEntity.Components == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            }

            for (int componentIndex = 0; componentIndex < cameraEntity.Components.Count; componentIndex++) {
                if (cameraEntity.Components[componentIndex] is city.game.DemoTiltFollowCameraComponent followCameraComponent) {
                    return followCameraComponent;
                }
            }

            throw new InvalidOperationException("Tilt Trial camera generation requires a DemoTiltFollowCameraComponent.");
        }

        /// <summary>
        /// Resolves the generated Tilt Trial speed HUD updater nested beneath the supplied UI root.
        /// </summary>
        /// <param name="uiEntity">Generated UI root whose HUD updater should be returned.</param>
        /// <returns>Attached Tilt Trial speed HUD updater.</returns>
        city.game.DemoTiltSpeedTextComponent FindRequiredSpeedTextComponent(EditorEntity uiEntity) {
            if (uiEntity == null || uiEntity.Children == null) {
                throw new ArgumentNullException(nameof(uiEntity));
            }

            for (int childIndex = 0; childIndex < uiEntity.Children.Count; childIndex++) {
                Entity child = uiEntity.Children[childIndex];
                if (child.Components == null) {
                    continue;
                }

                for (int componentIndex = 0; componentIndex < child.Components.Count; componentIndex++) {
                    if (child.Components[componentIndex] is city.game.DemoTiltSpeedTextComponent speedTextComponent) {
                        return speedTextComponent;
                    }
                }
            }

            throw new InvalidOperationException("Tilt Trial UI generation requires a DemoTiltSpeedTextComponent.");
        }

        /// <summary>
        /// Finds one required direct child entity by authored name.
        /// </summary>
        /// <param name="parent">Parent entity whose direct children should be searched.</param>
        /// <param name="name">Required direct child entity name.</param>
        /// <returns>Matching direct child entity.</returns>
        EditorEntity FindRequiredChildEntityByName(EditorEntity parent, string name) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("A child entity name must be provided.", nameof(name));
            } else if (parent.Children == null) {
                throw new InvalidOperationException($"Tilt Trial generation requires child entity '{name}'.");
            }

            for (int childIndex = 0; childIndex < parent.Children.Count; childIndex++) {
                if (parent.Children[childIndex] is EditorEntity childEntity && string.Equals(childEntity.Name, name, StringComparison.Ordinal)) {
                    return childEntity;
                }
            }

            throw new InvalidOperationException($"Tilt Trial generation requires child entity '{name}'.");
        }

        /// <summary>
        /// Resolves the direct box collider attached to the supplied entity.
        /// </summary>
        /// <param name="entity">Entity whose box collider should be returned.</param>
        /// <returns>Attached box collider component.</returns>
        BoxCollider3DComponent FindRequiredBoxColliderComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is BoxCollider3DComponent boxCollider) {
                    return boxCollider;
                }
            }

            throw new InvalidOperationException("Tilt Trial generation requires a BoxCollider3DComponent.");
        }

        /// <summary>
        /// Resolves one component from the supplied entity when present.
        /// </summary>
        /// <typeparam name="TComponent">Requested component type.</typeparam>
        /// <param name="entity">Entity whose components should be searched.</param>
        /// <param name="component">Resolved component instance when present.</param>
        /// <returns>True when the component was found.</returns>
        bool TryFindComponent<TComponent>(Entity entity, out TComponent component) where TComponent : Component {
            component = null;
            if (entity == null || entity.Components == null) {
                return false;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TComponent typedComponent) {
                    component = typedComponent;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates one rounded 2D panel entity beneath the supplied parent.
        /// </summary>
        /// <param name="parent">Parent entity that should own the panel.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local entity position.</param>
        /// <param name="size">Panel size in authored pixels.</param>
        /// <param name="radius">Corner radius.</param>
        /// <param name="borderThickness">Border thickness in authored pixels.</param>
        /// <param name="fillColor">Panel fill color.</param>
        /// <param name="borderColor">Panel border color.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <returns>Created panel entity.</returns>
        Entity CreateRoundedPanelEntity(Entity parent, string entityName, float3 localPosition, int2 size, float radius, float borderThickness, byte4 fillColor, byte4 borderColor, byte renderOrder2D) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = false;
            entity.AddComponent(new RoundedRectComponent {
                Size = size,
                Radius = radius,
                BorderThickness = borderThickness,
                FillColor = fillColor,
                BorderColor = borderColor,
                RenderOrder2D = renderOrder2D,
            });
            entity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = entityName
            });
            return entity;
        }

        /// <summary>
        /// Creates one reusable UI text entity using the shared Tilt Trial HUD font.
        /// </summary>
        /// <param name="parent">Parent entity that should own the text.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local entity position.</param>
        /// <param name="text">Initial text content.</param>
        /// <param name="size">Text layout size in authored pixels.</param>
        /// <param name="fontScale">Uniform glyph scale.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <param name="color">Text color.</param>
        /// <param name="alignment">Requested text alignment.</param>
        /// <returns>Created text entity.</returns>
        Entity CreateUiTextEntity(Entity parent, string entityName, float3 localPosition, string text, int2 size, float fontScale, byte renderOrder2D, byte4 color, TextAlignment alignment) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = false;
            TextComponent textComponent = new TextComponent {
                Text = text ?? string.Empty,
                Font = ResolveRequiredEditorFont(),
                Color = color,
                Size = size,
                FontScale = fontScale,
                Alignment = alignment,
                RenderOrder2D = renderOrder2D,
            };
            entity.AddComponent(textComponent);
            ApplyFontReference(entity, textComponent, TiltTrialSpeedHudFontRelativePath);
            entity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {
                Role = entityName
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font used by the live Tilt Trial HUD entities.
        /// </summary>
        /// <returns>Loaded default editor font.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the Tilt Trial scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Stores the supplied file-backed font reference on the entity save state for the supplied text component.
        /// </summary>
        /// <param name="entity">Entity that owns the text component.</param>
        /// <param name="component">Text component whose font reference should be stored.</param>
        /// <param name="fontPath">Project-relative font path.</param>
        void ApplyFontReference(Entity entity, TextComponent component, string fontPath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(fontPath)) {
                throw new ArgumentException("Font path must be provided.", nameof(fontPath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", global::city.scene.tools.DemoDiscEditorAssetReferenceFactory.CreateFont(AssetAuthoringService, fontPath));
        }

        /// <summary>
        /// Stores the editor UI font reference on the generated FPS component used by the render-test scene.
        /// </summary>
        /// <param name="entity">Entity that owns the FPS component.</param>
        /// <param name="component">FPS component whose font reference should be stored.</param>
        void ApplyEditorFontReference(Entity entity, FPSComponent component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());
        }

        /// <summary>
        /// Resolves the hidden save component attached to one generated editor entity after ids have been assigned.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached entity save component with a non-zero scene id.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    if (saveComponent.EntityId == 0u) {
                        throw new InvalidOperationException("Generated editor entities must have a preassigned numeric scene entity id.");
                    }

                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated editor entities must include EntitySaveComponent.");
        }
    }
}
