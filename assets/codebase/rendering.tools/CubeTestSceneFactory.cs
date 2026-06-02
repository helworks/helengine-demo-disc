using city.menu;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the minimal cube rendering test.
    /// </summary>
    public sealed class CubeTestSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated cube-test asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/cube_test.helen";

        /// <summary>
        /// Initializes one cube-test scene factory.
        /// </summary>
        public CubeTestSceneFactory() { }

        /// <summary>
        /// Creates the canonical cube-test live scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the authored mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the authored mesh.</param>
        /// <returns>Live-authored cube-test scene definition.</returns>
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
                    SceneId = RenderingSceneGenerator.CubeTestNintendoDsSceneId,
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = instructionOverlayFactory.CreateNintendoDsBottomInstructionRoots(instructionFont)
                },
                RootEntities = new[] {
                    cameraEntity,
                    instructionOverlayEntity,
                    CreateUiEntity(),
                    CreateDirectionalLightEntity(),
                    CreateGroundEntity(cubeModel, standardMaterial),
                    CreateCubeEntity(cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("CubeTestCamera");
            entity.LocalPosition = new float3(7f, 4.5f, 7f);
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
            return entity;
        }

        /// <summary>
        /// Creates the authored camera orientation for the minimal static ground-and-cube layout.
        /// </summary>
        /// <returns>Camera orientation that frames the elevated cube and ground.</returns>
        static float4 CreateCameraOrientation() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-2.35619449f, -0.27925268f, 0f, out orientation);
            return orientation;
        }

        /// <summary>
        /// Creates the authored UI root entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("CubeTestUi");
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            entity.AddComponent(new DemoDiscLightToggleComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored directional-light entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored directional-light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("CubeTestSun");
            entity.LocalPosition = new float3(0f, 4f, 0f);
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
        /// Creates the authored ground cube entity for the minimal rendering scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("CubeTestGround");
            entity.LocalPosition = new float3(0f, -0.5f, 0f);
            entity.LocalScale = new float3(14f, 1f, 14f);

            MeshComponent meshComponent = new MeshComponent {
                Model = cubeModel,
                Material = standardMaterial,
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            return entity;
        }

        /// <summary>
        /// Creates the authored cube mesh entity for the minimal rendering scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored cube entity.</returns>
        Entity CreateCubeEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("CubeTestCube");
            entity.LocalPosition = new float3(0f, 5f, 0f);
            entity.LocalScale = float3.One;

            MeshComponent meshComponent = new MeshComponent {
                Model = cubeModel,
                Material = standardMaterial,
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            return entity;
        }

        /// <summary>
        /// Resolves the optional font reference assigned to the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Optional font asset assigned to the FPS component.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the cube-test scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

    }
}



