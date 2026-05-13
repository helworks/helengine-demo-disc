using city.menu;
using gameplay.rendering;
using helengine.editor;

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
        /// Factory used to create authored scene entities for the active host.
        /// </summary>
        readonly IEntityFactory EntityFactory;

        /// <summary>
        /// Initializes one cube-test scene factory.
        /// </summary>
        public CubeTestSceneFactory()
            : this(ResolveEntityFactory()) {
        }

        /// <summary>
        /// Initializes one cube-test scene factory.
        /// </summary>
        /// <param name="entityFactory">Factory used to create authored scene entities for the active host.</param>
        public CubeTestSceneFactory(IEntityFactory entityFactory) {
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        }

        /// <summary>
        /// Resolves the host-owned authored entity factory from the active core instance.
        /// </summary>
        /// <returns>Host-owned authored entity factory.</returns>
        static IEntityFactory ResolveEntityFactory() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("Cube-test scene generation requires Core.Instance before resolving EntityFactory.");
            } else if (Core.Instance.EntityFactory == null) {
                throw new InvalidOperationException("Cube-test scene generation requires Core.Instance.EntityFactory.");
            }

            return Core.Instance.EntityFactory;
        }

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

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateCubeEntity(cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        EditorEntity CreateCameraEntity() {
            EditorEntity entity = CreateSceneRootEntity("CubeTestCamera", "cube-test-camera");
            entity.LocalPosition = new float3(0f, 0f, 6f);

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

            FPSComponent fpsComponent = new FPSComponent {
                Font = ResolveRequiredEditorFont()
            };
            entity.AddComponent(fpsComponent);

            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored directional-light entity for the cube-test scene.
        /// </summary>
        /// <returns>Live authored directional-light entity.</returns>
        EditorEntity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            EditorEntity entity = CreateSceneRootEntity("CubeTestSun", "cube-test-sun");
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
        /// Creates the authored cube mesh entity for the minimal rendering scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored cube entity.</returns>
        EditorEntity CreateCubeEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            EditorEntity entity = CreateSceneRootEntity("CubeTestCube", "cube-test-cube");
            entity.LocalScale = new float3(2f, 2f, 2f);

            MeshComponent meshComponent = new MeshComponent {
                Model = cubeModel,
                Material = standardMaterial,
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);

            entity.AddComponent(new AxisRotationComponent {
                Axis = new float3(0f, 1f, 0f),
                AngularSpeedRadiansPerSecond = (float)(Math.PI / 2.0)
            });
            return entity;
        }

        /// <summary>
        /// Creates one generated scene-root editor entity with the standard authored defaults used by the serializer.
        /// </summary>
        /// <param name="name">Display name assigned to the entity.</param>
        /// <param name="entityId">Stable entity id assigned to the hidden save component.</param>
        /// <returns>Configured scene-root editor entity.</returns>
        EditorEntity CreateSceneRootEntity(string name, string entityId) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Entity id must be provided.", nameof(entityId));
            }

            Entity entity = EntityFactory.Create(name);
            if (entity is not EditorEntity editorEntity) {
                throw new InvalidOperationException("Generated authored scene creation requires EditorEntity instances.");
            }

            GetSaveComponent(editorEntity).EntityId = entityId;
            return editorEntity;
        }

        /// <summary>
        /// Resolves the hidden save component attached to one editor entity.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Hidden save component attached to the entity.</returns>
        EntitySaveComponent GetSaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Editor entities must include a hidden save component.");
        }

        /// <summary>
        /// Resolves the editor font that should back the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Editor font asset required by the FPS component.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance == null || Core.Instance.DefaultFontAsset == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the cube-test scene can be generated.");
            }

            return Core.Instance.DefaultFontAsset;
        }

    }
}
