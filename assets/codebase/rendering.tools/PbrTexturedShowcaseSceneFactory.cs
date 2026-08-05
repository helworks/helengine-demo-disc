using city.menu;
using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored PBR textured showcase scene: a scuffed-metal prop and a wood-plank prop lit by one shadow-casting sun.
    /// </summary>
    public sealed class PbrTexturedShowcaseSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated PBR textured showcase asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/pbr_textured_showcase.helen";

        /// <summary>
        /// Creates the canonical PBR textured showcase live-authored scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model used by both hero props.</param>
        /// <param name="planeModel">Generated plane runtime model used by the ground mesh.</param>
        /// <param name="groundMaterial">Runtime material used by the ground mesh.</param>
        /// <param name="metalMaterial">Runtime scuffed-metal material used by the metal prop.</param>
        /// <param name="woodMaterial">Runtime wood-plank material used by the wood prop.</param>
        /// <returns>Live-authored scene definition for the PBR textured showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel planeModel, RuntimeMaterial groundMaterial, RuntimeMaterial metalMaterial, RuntimeMaterial woodMaterial) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (planeModel == null) {
                throw new ArgumentNullException(nameof(planeModel));
            } else if (groundMaterial == null) {
                throw new ArgumentNullException(nameof(groundMaterial));
            } else if (metalMaterial == null) {
                throw new ArgumentNullException(nameof(metalMaterial));
            } else if (woodMaterial == null) {
                throw new ArgumentNullException(nameof(woodMaterial));
            }

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = Array.Empty<Entity>()
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateUiEntity(),
                    CreateDirectionalLightEntity(),
                    CreateGroundEntity(planeModel, groundMaterial),
                    CreatePropEntity("PbrTexturedShowcaseMetalProp", new float3(-2.6f, 1.2f, 0f), new float3(2.4f, 2.4f, 2.4f), cubeModel, metalMaterial),
                    CreatePropEntity("PbrTexturedShowcaseWoodProp", new float3(2.6f, 1.2f, 0f), new float3(2.4f, 2.4f, 2.4f), cubeModel, woodMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the PBR textured showcase scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.3f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseCamera");
            entity.LocalPosition = new float3(0f, 5f, 10f);
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
                    ShadowDistance = 30f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = new float3(0f, 1.2f, 0f),
                AutoYawSpeedRadians = 0.08f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the PBR textured showcase scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            PspFpsComponentOverrideService.Apply(entity);
            DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
            sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "14. PBR Textures");
            return entity;
        }

        /// <summary>
        /// Creates the authored shadow-casting sun for the PBR textured showcase scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.5f, -0.85f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 7f, 0f);
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.97f, 0.92f, 1f),
                Intensity = 1.1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.9f,
                ShadowDistance = 30f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored ground receiver mesh for the PBR textured showcase scene.
        /// </summary>
        /// <param name="model">Runtime plane model used by the mesh.</param>
        /// <param name="material">Runtime material used by the mesh.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
            Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseGround");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = new float3(12f, 1f, 12f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Creates one authored hero prop entity for the PBR textured showcase scene.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime material used by the mesh.</param>
        /// <returns>Live authored prop entity.</returns>
        Entity CreatePropEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font that should back the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Editor font asset required by the FPS component.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the PBR textured showcase scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
