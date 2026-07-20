using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the standalone PS2 textured VU renderer probe scene using one isolated ground cube.
    /// </summary>
    public sealed class Ps2VuRendererProbeSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated PS2 textured VU renderer probe.
        /// </summary>
        public const string SceneId = "scenes/rendering/ps2_vu_renderer_probe.helen";

        /// <summary>
        /// Creates one textured ground cube using the Tilt Trial opening ground transform.
        /// </summary>
        /// <param name="cubeModel">Cooked cube model assigned to the four boxes.</param>
        /// <param name="coinModel">Cooked sphere model flattened into the coin shape.</param>
        /// <param name="sharedMaterial">One cooked textured material shared by every probe object.</param>
        /// <returns>Generated renderer probe scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(
            RuntimeModel cubeModel,
            RuntimeModel coinModel,
            RuntimeMaterial sharedMaterial) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (coinModel == null) {
                throw new ArgumentNullException(nameof(coinModel));
            } else if (sharedMaterial == null) {
                throw new ArgumentNullException(nameof(sharedMaterial));
            }

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = false,
                    BottomScreenRootEntities = Array.Empty<Entity>()
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateMeshEntity("Ps2VuProbeGround", cubeModel, sharedMaterial, new float3(0f, 0f, 0f), new float3(2f, 2f, 2f), float4.Identity)
                }
            };
        }

        /// <summary>
        /// Creates the Tilt Trial opening camera used by the renderer probe.
        /// </summary>
        /// <returns>Configured renderer probe camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("Ps2VuProbeCamera");
            entity.LocalPosition = new float3(0f, 0f, 5f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 64f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(26f / 255f, 38f / 255f, 64f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 32f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = float3.Zero
            });
            return entity;
        }

        /// <summary>
        /// Creates the fixed directional light used to reveal box faces and the coin rim.
        /// </summary>
        /// <returns>Configured renderer probe directional-light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            Entity entity = Core.Instance.EntityFactory.Create("Ps2VuProbeSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 6f, 0f);
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1.2f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 32f
            });
            return entity;
        }

        /// <summary>
        /// Creates one mesh entity with the shared probe material.
        /// </summary>
        /// <param name="name">Stable authored entity name.</param>
        /// <param name="model">Cooked model assigned to the entity.</param>
        /// <param name="material">Shared cooked textured material assigned to the entity.</param>
        /// <param name="position">Fixed authored position.</param>
        /// <param name="scale">Fixed authored scale.</param>
        /// <param name="orientation">Fixed authored orientation.</param>
        /// <returns>Configured mesh entity.</returns>
        Entity CreateMeshEntity(
            string name,
            RuntimeModel model,
            RuntimeMaterial material,
            float3 position,
            float3 scale,
            float4 orientation) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (model == null) {
                throw new ArgumentNullException(nameof(model));
            } else if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Creates the fixed rotation that turns the generated sphere into a coin face.
        /// </summary>
        /// <returns>Coin orientation around the authored horizontal axis.</returns>
        static float4 CreateCoinOrientation() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, (float)(Math.PI / 2.0), 0f, out orientation);
            return orientation;
        }
    }
}
