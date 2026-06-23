namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the minimal ground-and-cube probe scene.
    /// </summary>
    public sealed class GroundCubeProbeSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated ground-cube probe asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.GroundCubeProbeSceneId;

        /// <summary>
        /// Initializes one ground-cube probe scene factory.
        /// </summary>
        public GroundCubeProbeSceneFactory() { }

        /// <summary>
        /// Creates the canonical ground-cube probe live scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the authored meshes.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the authored meshes.</param>
        /// <returns>Live-authored ground-cube probe scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            }

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    SceneId = RenderingSceneGenerator.GroundCubeProbeNintendoDsSceneId,
                    UseDefaultBottomOverlay = true
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateGroundEntity(cubeModel, standardMaterial),
                    CreateCubeEntity(cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored fixed camera entity for the ground-cube probe scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.32f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("GroundCubeProbeCamera");
            entity.LocalPosition = new float3(0f, 10f, 28f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 96f,
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
            return entity;
        }

        /// <summary>
        /// Creates the authored directional-light entity for the ground-cube probe scene.
        /// </summary>
        /// <returns>Live authored directional-light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("GroundCubeProbeSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
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
        /// Creates the authored ground cube entity for the probe scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("GroundCubeProbeGround");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, -0.5f, 0f);
            entity.LocalScale = new float3(15f, 1f, 15f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Materials = new[] { standardMaterial },
                RenderOrder3D = 0
            });
            entity.AddComponent(CreateStaticRigidBodyComponent());
            entity.AddComponent(CreateBoxColliderComponent(new float3(15f, 1f, 15f)));
            return entity;
        }

        /// <summary>
        /// Creates the authored elevated unit cube entity for the probe scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored elevated cube entity.</returns>
        Entity CreateCubeEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("GroundCubeProbeCube");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 10f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Materials = new[] { standardMaterial },
                RenderOrder3D = 0
            });
            entity.AddComponent(CreateDynamicRigidBodyComponent());
            entity.AddComponent(CreateBoxColliderComponent(float3.One));
            return entity;
        }

        /// <summary>
        /// Creates the static rigid body used by the ground cube so the probe scene exposes one immovable support surface.
        /// </summary>
        /// <returns>Configured static rigid body component.</returns>
        RigidBody3DComponent CreateStaticRigidBodyComponent() {
            return new RigidBody3DComponent {
                BodyKind = BodyKind3D.Static,
                UseGravity = false,
                Mass = 1d
            };
        }

        /// <summary>
        /// Creates the dynamic rigid body used by the elevated probe cube so gravity can validate the minimal BEPU path.
        /// </summary>
        /// <returns>Configured dynamic rigid body component.</returns>
        RigidBody3DComponent CreateDynamicRigidBodyComponent() {
            return new RigidBody3DComponent {
                BodyKind = BodyKind3D.Dynamic,
                UseGravity = true,
                Mass = 1d
            };
        }

        /// <summary>
        /// Creates one box collider with the same dimensions as the authored cube mesh transform for minimal ground-and-cube contact tests.
        /// </summary>
        /// <param name="size">Full local box size used by the collider.</param>
        /// <returns>Configured box collider component.</returns>
        BoxCollider3DComponent CreateBoxColliderComponent(float3 size) {
            return new BoxCollider3DComponent {
                Size = size
            };
        }
    }
}
