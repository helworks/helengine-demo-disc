using city.menu;
using gameplay.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the minimal rotating cube rendering test.
    /// </summary>
    public sealed class CubeTestSceneFactory {
        /// <summary>
        /// Stable N64 target id used by the authored scene's explicit platform exclusions.
        /// </summary>
        const string Nintendo64PlatformId = "n64";

        /// <summary>
        /// Stable Dreamcast target id; its v0.2 runtime supports the same camera, cube and spin subset as N64.
        /// </summary>
        const string DreamcastPlatformId = "dc";

        /// <summary>
        /// Targets whose runtimes support neither the instruction overlay, the UI root nor the orbit camera.
        /// </summary>
        static readonly string[] MinimalRuntimeExcludedPlatformIds = [Nintendo64PlatformId, DreamcastPlatformId];

        /// <summary>
        /// Targets that cannot draw the shared console instruction Blueprint at all. The N64 runtime renders
        /// its authored sprite icons, so only Dreamcast still drops the whole root.
        /// </summary>
        static readonly string[] SpritelessRuntimeExcludedPlatformIds = [DreamcastPlatformId];

        /// <summary>
        /// Targets that cannot draw authored text. The N64 runtime now renders the shared 2D command list, so
        /// it keeps the UI root; only Dreamcast still drops it.
        /// </summary>
        static readonly string[] TextlessRuntimeExcludedPlatformIds = [DreamcastPlatformId];

        /// <summary>
        /// N64 alone. Used to strip authored controls that require input handling the N64 runtime does not
        /// provide, without also stripping them from Dreamcast, which already excludes the whole UI root at
        /// the textless tier above.
        /// </summary>
        static readonly string[] Nintendo64OnlyExcludedPlatformIds = [Nintendo64PlatformId];

        /// <summary>
        /// Existing editor platform authoring service used to persist N64 entity and component exclusions.
        /// </summary>
        readonly PlatformSceneAuthoringHelperService PlatformSceneAuthoringHelperServiceValue = new();

        /// <summary>
        /// Host-owned capability used to resolve generated control icons and fonts.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;
        /// <summary>
        /// Stable angular speed used by the rotating cube in radians per second.
        /// </summary>
        const float CubeAngularSpeedRadians = (float)(Math.PI / 2.0);

        /// <summary>
        /// Stable scene id used by the generated cube-test asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/cube_test.helen";

        /// <summary>
        /// Initializes one cube-test scene factory.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used by the shared instruction overlay.</param>
        public CubeTestSceneFactory(IEditorProjectAuthoringSession assetAuthoringService, EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Creates the canonical cube-test live scene definition.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated prompt icons.</param>
        /// <param name="cubeModel">Generated cube runtime model assigned to the authored mesh.</param>
        /// <param name="solidColorMaterial">Generated shared solid-color runtime material assigned to the authored mesh.</param>
        /// <returns>Live-authored cube-test scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel cubeModel, RuntimeMaterial solidColorMaterial) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (solidColorMaterial == null) {
                throw new ArgumentNullException(nameof(solidColorMaterial));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory(AssetAuthoringService, Transaction);
            Entity cameraEntity = CreateCameraEntity();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
            ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new ConsoleCameraLightInstructionsSceneAttachmentService();
            Entity consoleInstructionBlueprintEntity = consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath, AssetAuthoringService);
            Entity uiEntity = CreateUiEntity();
            Entity directionalLightEntity = CreateDirectionalLightEntity();

            ExcludeN64Root(instructionOverlayEntity, MinimalRuntimeExcludedPlatformIds);
            ExcludeN64Root(consoleInstructionBlueprintEntity, SpritelessRuntimeExcludedPlatformIds);
            ExcludeN64Root(uiEntity, TextlessRuntimeExcludedPlatformIds);
            ExcludeN64UiInputComponents(uiEntity, Nintendo64OnlyExcludedPlatformIds);
            ExcludeN64OrbitComponent(cameraEntity, MinimalRuntimeExcludedPlatformIds);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = Array.Empty<Entity>()
                },
                RootEntities = new[] {
                    cameraEntity,
                    instructionOverlayEntity,
                    consoleInstructionBlueprintEntity,
                    uiEntity,
                    directionalLightEntity,
                    CreateCubeEntity(cubeModel, solidColorMaterial)
                }
            };
        }

        /// <summary>
        /// Excludes one authored root subtree from the listed runtimes while retaining the same root on every other configured platform.
        /// </summary>
        /// <param name="rootEntity">Root subtree to exclude.</param>
        /// <param name="excludedPlatformIds">Platform ids that must not receive this root.</param>
        void ExcludeN64Root(Entity rootEntity, string[] excludedPlatformIds) {
            if (rootEntity is not EditorEntity editorRootEntity) {
                throw new InvalidOperationException("Cube-test N64 exclusions require editor entities.");
            }

            for (int index = 0; index < excludedPlatformIds.Length; index++) {
                PlatformSceneAuthoringHelperServiceValue.ExcludeEntitySubtreeFromScope(
                    editorRootEntity,
                    DemoDiscOverrideScopes.Platform(excludedPlatformIds[index]));
            }
        }

        /// <summary>
        /// Removes the authored orbit controller only on N64 while preserving the camera on all platforms.
        /// </summary>
        /// <param name="cameraEntity">Authored camera entity containing the orbit controller.</param>
        /// <param name="excludedPlatformIds">Platform ids that must not receive the orbit controller.</param>
        void ExcludeN64OrbitComponent(Entity cameraEntity, string[] excludedPlatformIds) {
            if (cameraEntity is not EditorEntity editorCameraEntity) {
                throw new InvalidOperationException("Cube-test N64 camera exclusions require an editor entity.");
            }

            city.rendering.DemoDiscOrbitCameraComponent orbitComponent = cameraEntity.Components
                .OfType<city.rendering.DemoDiscOrbitCameraComponent>()
                .Single();
            for (int index = 0; index < excludedPlatformIds.Length; index++) {
                PlatformSceneAuthoringHelperServiceValue.ExcludeComponentFromScope(
                    editorCameraEntity,
                    orbitComponent,
                    DemoDiscOverrideScopes.Platform(excludedPlatformIds[index]));
            }
        }

        /// <summary>
        /// Removes the authored return-to-menu and light-toggle controls only on N64 while preserving the rest
        /// of the UI root on every configured platform. Both components sit on the same entity as the FPS
        /// overlay, so excluding the entity is not an option; N64 has no input handling to drive either one.
        /// </summary>
        /// <param name="uiEntity">Authored UI root entity containing the return-to-menu and light-toggle controls.</param>
        /// <param name="excludedPlatformIds">Platform ids that must not receive the two input-driven components.</param>
        void ExcludeN64UiInputComponents(Entity uiEntity, string[] excludedPlatformIds) {
            if (uiEntity is not EditorEntity editorUiEntity) {
                throw new InvalidOperationException("Cube-test N64 UI input exclusions require an editor entity.");
            }

            city.menu.DemoDiscReturnToMenuComponent returnToMenuComponent = uiEntity.Components
                .OfType<city.menu.DemoDiscReturnToMenuComponent>()
                .Single();
            city.rendering.DemoDiscLightToggleComponent lightToggleComponent = uiEntity.Components
                .OfType<city.rendering.DemoDiscLightToggleComponent>()
                .Single();
            for (int index = 0; index < excludedPlatformIds.Length; index++) {
                EditorOverrideScope excludedScope = DemoDiscOverrideScopes.Platform(excludedPlatformIds[index]);
                PlatformSceneAuthoringHelperServiceValue.ExcludeComponentFromScope(editorUiEntity, returnToMenuComponent, excludedScope);
                PlatformSceneAuthoringHelperServiceValue.ExcludeComponentFromScope(editorUiEntity, lightToggleComponent, excludedScope);
            }
        }

        /// <summary>
        /// Creates the authored camera entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("CubeTestCamera");
            entity.LocalPosition = new float3(0f, 0f, 5f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = CreateCameraOrientation();

            CameraComponent cameraComponent = new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 64f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 24f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            };
            entity.AddComponent(cameraComponent);
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = float3.Zero,
                AutoYawSpeedRadians = 0f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored camera orientation for the rotating cube layout.
        /// </summary>
        /// <returns>Camera orientation that frames the cube at the origin.</returns>
        static float4 CreateCameraOrientation() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, 0f, 0f, out orientation);
            return orientation;
        }

        /// <summary>
        /// Creates the authored directional-light entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored directional-light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("CubeTestSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 4f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 24f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored UI root entity.</returns>
        Entity CreateUiEntity() {
            return new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateStandardSceneUi("CubeTestUi", "1. Cube Test");
        }

        /// <summary>
        /// Creates the authored cube mesh entity for the minimal rendering scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="solidColorMaterial">Generated shared solid-color runtime material assigned to the mesh.</param>
        /// <returns>Live authored cube entity.</returns>
        Entity CreateCubeEntity(RuntimeModel cubeModel, RuntimeMaterial solidColorMaterial) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("CubeTestCube");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 0f, 0f);
            entity.LocalScale = new float3(1f, 1f, 1f);
            entity.LocalOrientation = float4.Identity;

            MeshComponent meshComponent = new MeshComponent {
                Model = cubeModel,
                Materials = new[] { solidColorMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            entity.AddComponent(new city.rendering.CubeTestSpinComponent {
                BaseYawRadians = 0f,
                AngularSpeedRadians = CubeAngularSpeedRadians
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font used by the live instruction and UI entities.
        /// </summary>
        /// <returns>Loaded default editor font.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (AssetAuthoringService.RendererResources.DefaultFontAsset == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the cube-test scene can be generated.");
            }

            return AssetAuthoringService.RendererResources.DefaultFontAsset;
        }
    }
}
