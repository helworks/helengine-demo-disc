using city.menu;
using gameplay.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the scaled-cube rendering test.
    /// </summary>
    public sealed class ScaledCubeSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated scaled-cube asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.ScaledCubeSceneId;

        /// <summary>
        /// Initializes one scaled-cube scene factory.
        /// </summary>
        public ScaledCubeSceneFactory() { }

        /// <summary>
        /// Creates the canonical scaled-cube live scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the authored mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the authored mesh.</param>
        /// <returns>Live-authored scaled-cube scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
            Entity cameraEntity = CreateCameraEntity();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(instructionFont);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    SceneId = RenderingSceneGenerator.ScaledCubeNintendoDsSceneId,
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = instructionOverlayFactory.CreateNintendoDsBottomInstructionRoots(instructionFont)
                },
                RootEntities = new[] {
                    cameraEntity,
                    instructionOverlayEntity,
                    CreateUiEntity(),
                    CreateDirectionalLightEntity(),
                    CreateCubeEntity(cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the scaled-cube scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.28f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("ScaledCubeCamera");
            entity.LocalPosition = new float3(0f, 18f, 48f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 128f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 48f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = new float3(0f, 10f, 0f),
                AutoYawSpeedRadians = 0.07f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the scaled-cube scene.
        /// </summary>
        /// <returns>Live authored UI root entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("ScaledCubeUi");
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            entity.AddComponent(new DemoDiscLightToggleComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored directional-light entity for the scaled-cube scene.
        /// </summary>
        /// <returns>Live authored directional-light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("ScaledCubeSun");
            entity.LocalPosition = new float3(0f, 8f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 48f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored scaled cube mesh entity.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored scaled cube entity.</returns>
        Entity CreateCubeEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("ScaledCube");
            entity.LocalPosition = new float3(0f, 10f, 0f);
            entity.LocalScale = new float3(5f, 20f, 10f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Material = standardMaterial,
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font assigned to the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Editor font asset used by the FPS overlay.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the scaled-cube scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
