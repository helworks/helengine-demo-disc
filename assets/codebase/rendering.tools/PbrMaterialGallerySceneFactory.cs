using city.menu;
using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored PBR material gallery scene: a five by five sphere grid sweeping metallic and roughness under a three-light rig.
    /// </summary>
    public sealed class PbrMaterialGallerySceneFactory {
        /// <summary>
        /// Host-owned capability used to resolve generated control icons and fonts.
        /// </summary>
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        /// <summary>
        /// Initializes one PBR material gallery scene factory.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used by the shared instruction overlay.</param>
        public PbrMaterialGallerySceneFactory(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }
        /// <summary>
        /// Stable scene id used by the generated PBR material gallery asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/pbr_material_gallery.helen";

        /// <summary>
        /// World-space spacing between adjacent gallery spheres, in both grid axes.
        /// </summary>
        const float SphereSpacing = 2.4f;

        /// <summary>
        /// Local uniform scale applied to every gallery sphere.
        /// </summary>
        const float SphereScale = 1.6f;

        /// <summary>
        /// Local Y position every gallery sphere rests at, equal to its scaled radius.
        /// </summary>
        const float SphereRestY = SphereScale / 2f;

        /// <summary>
        /// Creates the canonical PBR material gallery live-authored scene definition.
        /// </summary>
        /// <param name="planeModel">Generated plane runtime model used by the ground mesh.</param>
        /// <param name="sphereModel">Generated sphere runtime model shared by every gallery sphere.</param>
        /// <param name="groundMaterial">Runtime material used by the ground mesh.</param>
        /// <param name="galleryMaterials">Twenty-five runtime materials ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</param>
        /// <returns>Live-authored scene definition for the PBR material gallery showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel planeModel, RuntimeModel sphereModel, RuntimeMaterial groundMaterial, RuntimeMaterial[] galleryMaterials) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (planeModel == null) {
                throw new ArgumentNullException(nameof(planeModel));
            } else if (sphereModel == null) {
                throw new ArgumentNullException(nameof(sphereModel));
            } else if (groundMaterial == null) {
                throw new ArgumentNullException(nameof(groundMaterial));
            } else if (galleryMaterials == null) {
                throw new ArgumentNullException(nameof(galleryMaterials));
            } else if (galleryMaterials.Length != PbrMaterialGalleryMaterialFactory.MetallicSteps * PbrMaterialGalleryMaterialFactory.RoughnessSteps) {
                throw new ArgumentException("PBR material gallery generation requires twenty-five runtime materials.", nameof(galleryMaterials));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory(AssetAuthoringService);
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
            ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new ConsoleCameraLightInstructionsSceneAttachmentService();
            consoleInstructionAttachmentService.ExcludeLegacyOverlayFromConsoles(projectRootPath, instructionOverlayEntity);
            Entity consoleInstructionBlueprintEntity = consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath, AssetAuthoringService);

            Entity[] sphereEntities = CreateSphereEntities(sphereModel, galleryMaterials);
            Entity[] rootEntities = new Entity[sphereEntities.Length + 8];
            rootEntities[0] = CreateCameraEntity();
            rootEntities[1] = CreateUiEntity();
            rootEntities[2] = CreateDirectionalLightEntity();
            rootEntities[3] = CreateDirectionalFillLightEntity();
            rootEntities[4] = CreateAmbientLightEntity();
            rootEntities[5] = instructionOverlayEntity;
            rootEntities[6] = consoleInstructionBlueprintEntity;
            rootEntities[7] = CreateGroundEntity(planeModel, groundMaterial);
            Array.Copy(sphereEntities, 0, rootEntities, 8, sphereEntities.Length);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = Array.Empty<Entity>()
                },
                RootEntities = rootEntities
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the PBR material gallery scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.42f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryCamera");
            entity.LocalPosition = new float3(0f, 10f, 16f);
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
                    ShadowDistance = 40f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = float3.Zero,
                AutoYawSpeedRadians = 0.08f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the PBR material gallery scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            return new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateStandardSceneUi("PbrMaterialGalleryUi", "13. PBR Gallery");
        }

        /// <summary>
        /// Creates the authored shadow-casting sun for the PBR material gallery scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.6f, -0.95f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGallerySun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 8f, 0f);
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.97f, 0.92f, 1f),
                Intensity = 1.15f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f,
                ShadowDistance = 40f
            });
            return entity;
        }

        /// <summary>
        /// Creates one weaker directional fill light that lifts every sphere's unlit hemisphere without adding a second shadow pass.
        /// </summary>
        /// <returns>Live authored fill-light entity.</returns>
        Entity CreateDirectionalFillLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(2.45f, -0.32f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryFill");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 6f, 0f);
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
        /// Creates one low-intensity ambient light so spheres facing away from the key lights do not collapse to flat black.
        /// </summary>
        /// <returns>Live authored ambient-light entity.</returns>
        Entity CreateAmbientLightEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryAmbient");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
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
        /// Creates the authored ground receiver mesh for the PBR material gallery scene.
        /// </summary>
        /// <param name="model">Runtime plane model used by the mesh.</param>
        /// <param name="material">Runtime material used by the mesh.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
            Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryGround");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = new float3(14f, 1f, 14f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Creates the twenty-five authored gallery sphere entities.
        /// </summary>
        /// <param name="sphereModel">Generated sphere runtime model shared by every gallery sphere.</param>
        /// <param name="galleryMaterials">Twenty-five runtime materials ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</param>
        /// <returns>Live authored sphere entities ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</returns>
        Entity[] CreateSphereEntities(RuntimeModel sphereModel, RuntimeMaterial[] galleryMaterials) {
            Entity[] sphereEntities = new Entity[galleryMaterials.Length];
            for (int metallicIndex = 0; metallicIndex < PbrMaterialGalleryMaterialFactory.MetallicSteps; metallicIndex++) {
                for (int roughnessIndex = 0; roughnessIndex < PbrMaterialGalleryMaterialFactory.RoughnessSteps; roughnessIndex++) {
                    int flatIndex = PbrMaterialGalleryMaterialFactory.ResolveIndex(metallicIndex, roughnessIndex);
                    float x = (roughnessIndex - 2) * SphereSpacing;
                    float z = (metallicIndex - 2) * SphereSpacing;
                    sphereEntities[flatIndex] = CreateSphereEntity(flatIndex, sphereModel, galleryMaterials[flatIndex], new float3(x, SphereRestY, z));
                }
            }

            return sphereEntities;
        }

        /// <summary>
        /// Creates one authored gallery sphere entity.
        /// </summary>
        /// <param name="flatIndex">Stable zero-based gallery index.</param>
        /// <param name="sphereModel">Generated sphere runtime model.</param>
        /// <param name="material">Runtime material assigned to the sphere.</param>
        /// <param name="localPosition">Authored local position for the sphere.</param>
        /// <returns>Live authored sphere entity.</returns>
        Entity CreateSphereEntity(int flatIndex, RuntimeModel sphereModel, RuntimeMaterial material, float3 localPosition) {
            Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGallerySphere" + flatIndex.ToString("00"));
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = localPosition;
            entity.LocalScale = new float3(SphereScale, SphereScale, SphereScale);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = sphereModel,
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
                throw new InvalidOperationException("A default editor font must be loaded before the PBR material gallery scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
