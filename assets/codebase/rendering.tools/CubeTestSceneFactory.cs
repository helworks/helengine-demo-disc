using city.menu;
using gameplay.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the minimal rotating cube rendering test.
    /// </summary>
    public sealed class CubeTestSceneFactory {
        /// <summary>
        /// Host-owned capability used to resolve generated control icons and fonts.
        /// </summary>
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;
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
        public CubeTestSceneFactory(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
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
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory(AssetAuthoringService);
            Entity cameraEntity = CreateCameraEntity();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
            ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new ConsoleCameraLightInstructionsSceneAttachmentService();
            Entity consoleInstructionBlueprintEntity = consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath, AssetAuthoringService);

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
                    CreateUiEntity(),
                    CreateDirectionalLightEntity(),
                    CreateCubeEntity(cubeModel, solidColorMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("CubeTestCamera");
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

            Entity entity = Core.Instance.EntityFactory.Create("CubeTestSun");
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
            Entity entity = Core.Instance.EntityFactory.Create("CubeTestCube");
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
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the cube-test scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
