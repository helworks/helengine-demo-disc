using city.menu;
using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored PBR shadow theater scene: a metallic sphere cluster on a pedestal, lit by both a shadow-casting sun and a shadow-casting spotlight.
    /// </summary>
    public sealed class PbrShadowTheaterSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated PBR shadow theater asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/pbr_shadow_theater.helen";

        /// <summary>
        /// Creates the canonical PBR shadow theater live-authored scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model used by the pedestal.</param>
        /// <param name="sphereModel">Generated sphere runtime model shared by every cluster sphere.</param>
        /// <param name="pedestalMaterial">Runtime material used by the pedestal.</param>
        /// <param name="galleryMaterials">Twenty-five gallery runtime materials ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</param>
        /// <returns>Live-authored scene definition for the PBR shadow theater showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel sphereModel, RuntimeMaterial pedestalMaterial, RuntimeMaterial[] galleryMaterials) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (sphereModel == null) {
                throw new ArgumentNullException(nameof(sphereModel));
            } else if (pedestalMaterial == null) {
                throw new ArgumentNullException(nameof(pedestalMaterial));
            } else if (galleryMaterials == null) {
                throw new ArgumentNullException(nameof(galleryMaterials));
            } else if (galleryMaterials.Length != PbrMaterialGalleryMaterialFactory.MetallicSteps * PbrMaterialGalleryMaterialFactory.RoughnessSteps) {
                throw new ArgumentException("PBR shadow theater generation requires the full twenty-five element gallery material array.", nameof(galleryMaterials));
            }

            RuntimeMaterial lowRoughnessMetal = galleryMaterials[PbrMaterialGalleryMaterialFactory.ResolveIndex(4, 0)];
            RuntimeMaterial highRoughnessMetal = galleryMaterials[PbrMaterialGalleryMaterialFactory.ResolveIndex(4, 4)];
            RuntimeMaterial lowRoughnessDielectric = galleryMaterials[PbrMaterialGalleryMaterialFactory.ResolveIndex(0, 1)];

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
                    CreateSpotLightEntity(),
                    CreatePedestalEntity(cubeModel, pedestalMaterial),
                    CreateClusterSphereEntity("PbrShadowTheaterSphereLowRoughMetal", new float3(-1.3f, 2.1f, 0f), sphereModel, lowRoughnessMetal),
                    CreateClusterSphereEntity("PbrShadowTheaterSphereHighRoughMetal", new float3(1.3f, 2.1f, 0f), sphereModel, highRoughnessMetal),
                    CreateClusterSphereEntity("PbrShadowTheaterSphereDielectric", new float3(0f, 2.1f, 1.6f), sphereModel, lowRoughnessDielectric)
                }
            };
        }

        /// <summary>
        /// Creates the authored orbit camera entity for the PBR shadow theater scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.32f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterCamera");
            entity.LocalPosition = new float3(0f, 6f, 11f);
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
                    new float4(0.015f, 0.015f, 0.03f, 1f),
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
                OrbitCenter = new float3(0f, 1.6f, 0f),
                AutoYawSpeedRadians = 0.1f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the PBR shadow theater scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            PspFpsComponentOverrideService.Apply(entity);
            DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
            sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "15. PBR Shadow Theater");
            return entity;
        }

        /// <summary>
        /// Creates the authored shadow-casting sun for the PBR shadow theater scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.9f, -0.7f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 7f, 0f);
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.95f, 0.9f, 1f),
                Intensity = 0.85f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.9f,
                ShadowDistance = 30f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored shadow-casting spotlight for the PBR shadow theater scene.
        /// </summary>
        /// <returns>Live authored spotlight entity.</returns>
        Entity CreateSpotLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(1.9f, -0.95f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterSpotlight");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(-3.5f, 6f, 3f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new SpotLightComponent {
                Color = new float4(0.7f, 0.85f, 1f, 1f),
                Range = 20f,
                InnerConeAngleDegrees = 18f,
                OuterConeAngleDegrees = 30f,
                Intensity = 1.4f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored pedestal mesh for the PBR shadow theater scene.
        /// </summary>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime material used by the mesh.</param>
        /// <returns>Live authored pedestal entity.</returns>
        Entity CreatePedestalEntity(RuntimeModel model, RuntimeMaterial material) {
            Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterPedestal");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 0.5f, 0f);
            entity.LocalScale = new float3(6f, 1f, 6f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Creates one authored cluster sphere entity for the PBR shadow theater scene.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="model">Runtime sphere model used by the mesh.</param>
        /// <param name="material">Runtime material used by the mesh.</param>
        /// <returns>Live authored cluster sphere entity.</returns>
        Entity CreateClusterSphereEntity(string name, float3 localPosition, RuntimeModel model, RuntimeMaterial material) {
            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = localPosition;
            entity.LocalScale = new float3(1.6f, 1.6f, 1.6f);
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
                throw new InvalidOperationException("A default editor font must be loaded before the PBR shadow theater scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
