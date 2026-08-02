using city.rendering.tools;

namespace city.physics.tools {
    /// <summary>
    /// Creates exportable scene assets for physics validation and demo playback.
    /// </summary>
    public sealed class PhysicsSceneFactory {
        /// <summary>
        /// Active project root used while authoring playable showcase overlays.
        /// </summary>
        string CurrentProjectRootPath = string.Empty;

        /// <summary>
        /// Stable generated provider identifier used for built-in primitive assets.
        /// </summary>
        const string GeneratedProviderId = EngineGeneratedAssetProvider.ProviderIdValue;

        /// <summary>
        /// Stable scene-asset source kind byte used for generated primitive references.
        /// </summary>
        const SceneAssetReferenceSourceKind GeneratedSourceKind = SceneAssetReferenceSourceKind.Generated;

        /// <summary>
        /// Stable render order assigned to generated debug geometry meshes.
        /// </summary>
        const byte DefaultMeshRenderOrder = 0;

        /// <summary>
        /// Stable tagged field name used for mesh model-reference persistence.
        /// </summary>
        const string MeshModelReferenceFieldName = "Model";

        /// <summary>
        /// Stable tagged field name used for mesh material-reference array persistence.
        /// </summary>
        const string MeshMaterialReferencesFieldName = "Materials";

        /// <summary>
        /// Stable tagged field name used for mesh render-order persistence.
        /// </summary>
        const string MeshRenderOrder3DFieldName = "RenderOrder3D";

        /// <summary>
        /// Stable camera draw order assigned to validation-scene cameras.
        /// </summary>
        const byte DefaultCameraDrawOrder = 0;

        /// <summary>
        /// Stable tagged field name used for camera draw-order persistence.
        /// </summary>
        const string CameraDrawOrderFieldName = "CameraDrawOrder";

        /// <summary>
        /// Stable tagged field name used for camera layer-mask persistence.
        /// </summary>
        const string CameraLayerMaskFieldName = "LayerMask";

        /// <summary>
        /// Stable tagged field name used for camera viewport persistence.
        /// </summary>
        const string CameraViewportFieldName = "Viewport";

        /// <summary>
        /// Stable tagged field name used for camera near clip-plane persistence.
        /// </summary>
        const string CameraNearPlaneDistanceFieldName = "NearPlaneDistance";

        /// <summary>
        /// Stable tagged field name used for camera far clip-plane persistence.
        /// </summary>
        const string CameraFarPlaneDistanceFieldName = "FarPlaneDistance";

        /// <summary>
        /// Stable tagged field name used for camera clear-settings persistence.
        /// </summary>
        const string CameraClearSettingsFieldName = "ClearSettings";

        /// <summary>
        /// Stable tagged field name used for camera render-settings persistence.
        /// </summary>
        const string CameraRenderSettingsFieldName = "RenderSettings";

        /// <summary>
        /// File-system scene-asset source kind used for authored file-backed assets.
        /// </summary>
        const SceneAssetReferenceSourceKind FileSystemSourceKind = SceneAssetReferenceSourceKind.FileSystem;

        /// <summary>
        /// Stable texture importer identifier stored on the generated sphere-tile texture sidecar.
        /// </summary>
        const string TextureImporterId = "gdi";

        /// <summary>
        /// Stable PS2 lit textured-material schema identifier used by the PS2 runtime path.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable GameCube textured-material schema identifier used by the GX runtime path.
        /// </summary>
        const string GameCubeMaterialSchemaId = "gamecube-standard-textured";

        /// <summary>
        /// Stable Nintendo DS textured-material schema identifier used by the DS runtime path.
        /// </summary>
        const string DsMaterialSchemaId = "ds-standard-textured";

        /// <summary>
        /// Stable standard shader asset identifier used by compatibility material payloads.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Relative project asset path for the neutral physics demo material.
        /// </summary>
        const string PhysicsDemoNeutralMaterialRelativePath = "Materials/physics/PhysicsDemoNeutral" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the non-shadow-casting ground physics demo material.
        /// </summary>
        const string PhysicsDemoGroundMaterialRelativePath = "Materials/physics/PhysicsDemoGround" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the blue physics demo material.
        /// </summary>
        const string PhysicsDemoBlueMaterialRelativePath = "Materials/physics/PhysicsDemoBlue" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the green physics demo material.
        /// </summary>
        const string PhysicsDemoGreenMaterialRelativePath = "Materials/physics/PhysicsDemoGreen" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the magenta physics demo material.
        /// </summary>
        const string PhysicsDemoMagentaMaterialRelativePath = "Materials/physics/PhysicsDemoMagenta" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the yellow physics demo material.
        /// </summary>
        const string PhysicsDemoYellowMaterialRelativePath = "Materials/physics/PhysicsDemoYellow" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the cyan physics demo material.
        /// </summary>
        const string PhysicsDemoCyanMaterialRelativePath = "Materials/physics/PhysicsDemoCyan" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the red physics demo material.
        /// </summary>
        const string PhysicsDemoRedMaterialRelativePath = "Materials/physics/PhysicsDemoRed" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the orange physics demo material.
        /// </summary>
        const string PhysicsDemoOrangeMaterialRelativePath = "Materials/physics/PhysicsDemoOrange" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the purple physics demo material.
        /// </summary>
        const string PhysicsDemoPurpleMaterialRelativePath = "Materials/physics/PhysicsDemoPurple" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the shared sphere-stack tile texture.
        /// </summary>
        const string PhysicsDemoSphereTileTextureRelativePath = "images/physics/PhysicsDemoSphereTile.bmp";

        /// <summary>
        /// Relative project asset path for the blue sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackBlueMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackBlue" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the green sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackGreenMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackGreen" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the magenta sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackMagentaMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackMagenta" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the yellow sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackYellowMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackYellow" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the cyan sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackCyanMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackCyan" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the red sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackRedMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackRed" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the orange sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackOrangeMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackOrange" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the purple sphere-stack material.
        /// </summary>
        const string PhysicsDemoSphereStackPurpleMaterialRelativePath = "Materials/physics/PhysicsDemoSphereStackPurple" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Material schema id used by the shared forward standard shader.
        /// </summary>
        const string StandardShaderSchemaId = "standard-shader";

        /// <summary>
        /// Material field id that disables custom shader overrides on standard materials.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

        /// <summary>
        /// Material field id that stores the authored standard-shader base color.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Material field id that stores the authored diffuse texture id.
        /// </summary>
        const string TextureAssetIdFieldId = "texture-id";

        /// <summary>
        /// Material field id that stores the compatibility shader asset identifier.
        /// </summary>
        const string ShaderAssetIdFieldId = "shader-asset-id";

        /// <summary>
        /// Material field id that controls shadow-map casting.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// PS2 material field id that controls shadow-map casting participation.
        /// </summary>
        const string Ps2CastShadowsFieldId = "cast-shadows";

        /// <summary>
        /// Material field id that controls shadow attenuation receiving.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// PS2 material field id that controls alpha test and blend mode selection.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Material field id that controls double-sided rasterization.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Material field id that controls whether vertex colors tint the final material.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Material field id that controls fixed-function lighting mode selection.
        /// </summary>
        const string LightingModeFieldId = "lighting-mode";

        /// <summary>
        /// GameCube material field id that stores the cooked imported texture path.
        /// </summary>
        const string GameCubeTextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Nintendo DS material field id that stores the cooked imported texture path.
        /// </summary>
        const string DsTextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Camera clear color used by physics validation scenes.
        /// </summary>
        static readonly float4 CornflowerBlueClearColor = new float4(0.39215687f, 0.58431375f, 0.92941177f, 1f);

        /// <summary>
        /// Width, in pixels, of the generated sphere-stack tile texture.
        /// </summary>
        const int PhysicsDemoSphereTileTextureWidth = 64;

        /// <summary>
        /// Height, in pixels, of the generated sphere-stack tile texture.
        /// </summary>
        const int PhysicsDemoSphereTileTextureHeight = 64;

        /// <summary>
        /// Tile size, in pixels, used by the generated sphere-stack tile texture.
        /// </summary>
        const int PhysicsDemoSphereTileTextureTileSize = 16;

        /// <summary>
        /// Grout thickness, in pixels, used between generated sphere-stack tiles.
        /// </summary>
        const int PhysicsDemoSphereTileTextureGroutThickness = 2;

        /// <summary>
        /// Shared generated bitmap bytes written to the authored sphere-stack tile texture source.
        /// </summary>
        static readonly byte[] PhysicsDemoSphereTileTextureBytes = BuildPhysicsDemoSphereTileTextureFileBytes();

        /// <summary>
        /// Imported texture asset identifier derived from the generated sphere-stack tile texture bytes.
        /// </summary>
        static readonly string PhysicsDemoSphereTileTextureAssetId = BuildImporterQualifiedAssetId(ComputeSourceChecksum(PhysicsDemoSphereTileTextureBytes), TextureImporterId);

        /// <summary>
        /// Current payload version for serialized rigid-body component scene records.
        /// </summary>
        const byte RigidBodyComponentPayloadVersion = 1;

        /// <summary>
        /// Current payload version for serialized box-collider component scene records.
        /// </summary>
        const byte BoxColliderComponentPayloadVersion = 1;

        /// <summary>
        /// Current payload version for serialized sphere-collider component scene records.
        /// </summary>
        const byte SphereColliderComponentPayloadVersion = 1;

        /// <summary>
        /// Current payload version for serialized kinematic-motion component scene records.
        /// </summary>
        const byte KinematicMotionComponentPayloadVersion = 1;

        /// <summary>
        /// Current payload version for serialized character-controller component scene records.
        /// </summary>
        const byte CharacterControllerComponentPayloadVersion = 1;

        /// <summary>
        /// Serialized rigid-body kind byte for static bodies.
        /// </summary>
        const byte StaticBodyKindCode = 0;

        /// <summary>
        /// Serialized rigid-body kind byte for dynamic bodies.
        /// </summary>
        const byte DynamicBodyKindCode = 2;

        /// <summary>
        /// Serialized rigid-body kind byte for kinematic bodies.
        /// </summary>
        const byte KinematicBodyKindCode = 1;

        /// <summary>
        /// Allocates numeric entity ids while one validation scene asset is being built.
        /// </summary>
        readonly SceneEntityAssetIdAllocator SceneEntityIdAllocator;

        /// <summary>
        /// Shared persistence registry used to serialize live editor-authored overlay entities into scene assets.
        /// </summary>
        readonly ComponentPersistenceRegistry PersistenceRegistry;

        /// <summary>
        /// Shared payload wrapper used to preserve any component-level override metadata while serializing live editor-authored overlay entities.
        /// </summary>
        readonly ComponentPlatformOverridePayloadService OverridePayloadService;

        /// <summary>
        /// Shared editor-authored scene writer used for the playable physics showcases so their instruction overlays follow the standard city save pipeline.
        /// </summary>
        readonly city.rendering.tools.GeneratedAuthoringSceneWriteService AuthoringSceneWriteService;

        /// <summary>
        /// Initializes the validation-scene factory with a fresh scene-local entity id allocator.
        /// </summary>
        public PhysicsSceneFactory() {
            SceneEntityIdAllocator = new SceneEntityAssetIdAllocator();
            PersistenceRegistry = city.rendering.tools.GeneratedScenePersistenceRegistryFactory.Create();
            OverridePayloadService = new ComponentPlatformOverridePayloadService();
            AuthoringSceneWriteService = new city.rendering.tools.GeneratedAuthoringSceneWriteService();
        }

        /// <summary>
        /// Creates one fully-authored physics validation scene asset for the requested scene id.
        /// </summary>
        /// <param name="sceneId">Stable relative scene id to author.</param>
        /// <returns>Generated scene asset ready for serialization.</returns>
        public SceneAsset CreateSceneAsset(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            SceneEntityIdAllocator.Reset();
            SceneAsset sceneAsset;
            if (string.Equals(sceneId, PhysicsSceneCatalog.CharacterSlopeSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateCharacterSlopeScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.CharacterStepsSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateCharacterStepsScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.CharacterMovingPlatformSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateCharacterMovingPlatformScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicStackBoxesSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateDynamicStackBoxesScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.SingleFallingCubeSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateSingleFallingCubeScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicSphereStackSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateDynamicSphereStackScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.StrictRotatedBoxCompareSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateStrictRotatedBoxCompareScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.RenderOnlySlopeSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateRenderOnlySlopeScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.MatrixRenderSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateMatrixRenderScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicMixedStackSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateDynamicMixedStackScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.KinematicPushSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateKinematicPushScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.MeshGroundStabilitySceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateMeshGroundStabilityScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshShowcaseSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateStaticMeshShowcaseScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshMinimalSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateStaticMeshMinimalScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.TriggerVolumeSceneId, StringComparison.Ordinal)) {
                sceneAsset = CreateTriggerVolumeScene();
            } else {
                throw new InvalidOperationException($"Unsupported physics validation scene id '{sceneId}'.");
            }

            return sceneAsset;
        }

        /// <summary>
        /// Writes every known validation scene into the target project assets folder.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        public void WriteScenes(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            CurrentProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(projectRootPath, "assets");
            if (!Directory.Exists(assetsRootPath)) {
                throw new DirectoryNotFoundException($"Physics validation scene export requires an assets directory at '{assetsRootPath}'.");
            }

            WriteSupportAssets(projectRootPath);

            string[] sceneIds = PhysicsSceneCatalog.GetSceneIds();
            for (int index = 0; index < sceneIds.Length; index++) {
                string sceneId = sceneIds[index];
                if (IsPlayablePhysicsShowcaseScene(sceneId)) {
                    WritePlayablePhysicsShowcaseScene(projectRootPath, sceneId);
                    continue;
                }

                SceneAsset sceneAsset = CreateSceneAsset(sceneId);
                string fullPath = GetSceneFullPath(projectRootPath, sceneId);
                string directoryPath = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrWhiteSpace(directoryPath)) {
                    throw new InvalidOperationException($"Could not resolve the directory path for scene '{sceneId}'.");
                }

                Directory.CreateDirectory(directoryPath);
                using FileStream stream = File.Create(fullPath);
                helengine.editor.AssetSerializer.Serialize(stream, sceneAsset);
            }
        }

        /// <summary>
        /// Creates the character slope validation scene.
        /// </summary>
        /// <returns>Authored slope validation scene asset.</returns>
        SceneAsset CreateCharacterSlopeScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "character_slope.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("character_slope.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(14f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("character_slope.ramp", "SlopeRamp", new float3(2.25f, 0.6f, 0f), new float3(5f, 0.6f, 3f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreateCharacterControllerBoxMeshEntity("character_slope.controller", "CharacterController", new float3(-4f, 0.75f, 0f), new float3(0.9f, 1.5f, 0.9f), float4.Identity, new float3(1f, 0f, 0f), 3d, 1d, 0.75d, 0.3d, CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
                    CreateMarkerEntity("character_slope.spawn", "ControllerSpawn", new float3(-4f, 0.75f, 0f)),
                    CreateMarkerEntity("character_slope.goal", "SlopeGoal", new float3(4.25f, 1.75f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("character_slope.camera", new float3(8.5f, 5f, 7.5f), CreateYawPitchRollDegrees(-135.0, -18.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.CharacterSlopeSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the character steps validation scene.
        /// </summary>
        /// <returns>Authored steps validation scene asset.</returns>
        SceneAsset CreateCharacterStepsScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "character_steps.scenario",
                new[] {
                    CreateCubeMeshEntity("character_steps.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 12f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreateCubeMeshEntity("character_steps.step01", "Step01", new float3(0.75f, 0.15f, 0f), new float3(1.5f, 0.3f, 3f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
                    CreateCubeMeshEntity("character_steps.step02", "Step02", new float3(2.25f, 0.45f, 0f), new float3(1.5f, 0.9f, 3f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreateCubeMeshEntity("character_steps.step03", "Step03", new float3(3.75f, 0.75f, 0f), new float3(1.5f, 1.5f, 3f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
                    CreateCubeMeshEntity("character_steps.step04", "Step04", new float3(5.25f, 1.05f, 0f), new float3(1.5f, 2.1f, 3f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
                    CreateMarkerEntity("character_steps.spawn", "ControllerSpawn", new float3(-4.5f, 0.75f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("character_steps.camera", new float3(9f, 5.5f, 7f), CreateYawPitchRollDegrees(-138.0, -20.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.CharacterStepsSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the character moving-platform validation scene.
        /// </summary>
        /// <returns>Authored moving-platform validation scene asset.</returns>
        SceneAsset CreateCharacterMovingPlatformScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "character_moving_platform.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("character_moving_platform.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(18f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("character_moving_platform.gap_a", "GapEdgeA", new float3(-1.75f, 0.25f, 0f), new float3(4f, 0.5f, 4f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("character_moving_platform.gap_b", "GapEdgeB", new float3(4.75f, 0.25f, 0f), new float3(4f, 0.5f, 4f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
                    CreateKinematicPhysicsBoxMeshEntity(
                        "character_moving_platform.platform",
                        "MovingPlatform",
                        new float3(-0.5f, 0.75f, 0f),
                        new float3(2.5f, 0.35f, 2.5f),
                        float4.Identity,
                        new float3(-0.5f, 0.75f, 0f),
                        new float3(3.5f, 0.75f, 0f),
                        2d,
                        true,
                        CreatePhysicsDemoMaterialReference(PhysicsDemoCyanMaterialRelativePath)),
                    CreateMarkerEntity("character_moving_platform.platform_start", "PlatformStart", new float3(-0.5f, 0.75f, 0f)),
                    CreateMarkerEntity("character_moving_platform.platform_end", "PlatformEnd", new float3(3.5f, 0.75f, 0f)),
                    CreateMarkerEntity("character_moving_platform.spawn", "ControllerSpawn", new float3(-5f, 0.75f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("character_moving_platform.camera", new float3(10f, 5.75f, 8f), CreateYawPitchRollDegrees(-140.0, -18.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.CharacterMovingPlatformSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the stacked dynamic-body validation scene.
        /// </summary>
        /// <returns>Authored stacked-box validation scene asset.</returns>
        SceneAsset CreateDynamicStackBoxesScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "dynamic_stack_boxes.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("dynamic_stack_boxes.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(14f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("dynamic_stack_boxes.box01", "StackBox01", new float3(0f, 0.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("dynamic_stack_boxes.box02", "StackBox02", new float3(0.5f, 1.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("dynamic_stack_boxes.box03", "StackBox03", new float3(1.0f, 2.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("dynamic_stack_boxes.box04", "StackBox04", new float3(1.5f, 3.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
                    CreateMarkerEntity("dynamic_stack_boxes.spawn", "DynamicSpawn", new float3(-2.5f, 1.5f, 0f))
                });
            SceneEntityAsset cameraEntity = CreatePhysicsShowcaseCameraEntity("dynamic_stack_boxes.camera", new float3(2.25f, 4.8f, 10.25f), CreateYawPitchRollDegrees(8.0, -16.0, 0.0), new float3(0.75f, 1.5f, 0f));
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.DynamicStackBoxesSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the minimal falling-cube validation scene.
        /// </summary>
        /// <returns>Authored ground-and-cube validation scene asset.</returns>
        SceneAsset CreateSingleFallingCubeScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "single_falling_cube.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("single_falling_cube.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(14f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("single_falling_cube.box01", "FallingCube", new float3(0f, 5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
                    CreateMarkerEntity("single_falling_cube.spawn", "DynamicSpawn", new float3(0f, 5f, 0f))
                });
            SceneEntityAsset cameraEntity = CreatePhysicsShowcaseCameraEntity("single_falling_cube.camera", new float3(7f, 4.5f, 7f), CreateYawPitchRollDegrees(-135.0, -16.0, 0.0), new float3(0f, 2.25f, 0f));
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.SingleFallingCubeSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the dynamic sphere-stack validation scene.
        /// </summary>
        /// <returns>Authored sphere-stack validation scene asset.</returns>
        SceneAsset CreateDynamicSphereStackScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "dynamic_sphere_stack.scenario",
                CreateDynamicSphereStackChildren());
            SceneEntityAsset cameraEntity = CreatePhysicsShowcaseCameraEntity("dynamic_sphere_stack.camera", new float3(9.5f, 6.75f, 9f), CreateYawPitchRollDegrees(45.0, -18.0, 0.0), float3.Zero);
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.DynamicSphereStackSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the strict rotated-box parity validation scene.
        /// </summary>
        /// <returns>Authored parity scene that compares one render-only rotated box against one physics-backed rotated box.</returns>
        SceneAsset CreateStrictRotatedBoxCompareScene() {
            float4 rampOrientation = CreateYawPitchRollDegrees(0.0, 0.0, 18.0);
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "strict_rotated_box_compare.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("strict_rotated_box_compare.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(24f, 1f, 18f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("strict_rotated_box_compare.flat_box", "PhysicsFlatBox", new float3(0f, 0.5f, -5.5f), new float3(6f, 1f, 4f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreateCubeMeshEntity("strict_rotated_box_compare.control_box", "ControlVisualBox", new float3(-5f, 1f, 1.5f), new float3(6f, 1f, 10f), rampOrientation, CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("strict_rotated_box_compare.physics_box", "PhysicsVisualBox", new float3(5f, 1f, 1.5f), new float3(6f, 1f, 10f), rampOrientation, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
                    CreatePhysicsSphereMeshEntity("strict_rotated_box_compare.physics_probe", "PhysicsProbe", new float3(5f, 4.5f, -1.25f), float4.Identity, DynamicBodyKindCode, true, CreateSphereStackMaterialReference(0)),
                    CreateMarkerEntity("strict_rotated_box_compare.control_marker", "ControlMarker", new float3(-5f, 2.5f, 1.5f)),
                    CreateMarkerEntity("strict_rotated_box_compare.physics_marker", "PhysicsMarker", new float3(5f, 2.5f, 1.5f))
                });
            SceneEntityAsset cameraEntity = CreatePhysicsShowcaseCameraEntity("strict_rotated_box_compare.camera", new float3(0f, 7.5f, 15.5f), CreateYawPitchRollDegrees(180.0, -20.0, 0.0), new float3(0f, 1.5f, 0f));
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.StrictRotatedBoxCompareSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the render-only slope validation scene.
        /// </summary>
        /// <returns>Authored slope scene that keeps the same rotated mesh shape while removing BEPU rigid bodies and colliders from the ramp.</returns>
        SceneAsset CreateRenderOnlySlopeScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "render_only_slope.scenario",
                new[] {
                    CreateCubeMeshEntity("render_only_slope.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(14f, 1f, 14f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreateCubeMeshEntity("render_only_slope.ramp", "SlopeRamp", new float3(2.25f, 0.6f, 0f), new float3(5f, 0.6f, 3f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreateMarkerEntity("render_only_slope.spawn", "SlopeStart", new float3(-4f, 0.75f, 0f)),
                    CreateMarkerEntity("render_only_slope.goal", "SlopeGoal", new float3(4.25f, 1.75f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("render_only_slope.camera", new float3(8.5f, 5f, 7.5f), CreateYawPitchRollDegrees(45.0, -18.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.RenderOnlySlopeSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the matrix render hero cube that animates through each transform combination.
        /// </summary>
        /// <returns>Authored hero cube entity with the dedicated matrix render component.</returns>
        SceneEntityAsset CreateMatrixRenderHeroEntity() {
            const string EntityId = "matrix_render.hero";

            if (string.IsNullOrWhiteSpace(EntityId)) {
                throw new InvalidOperationException("Matrix render hero entity id must be provided.");
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "HeroMotionCube",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = float3.Zero,
                LocalScale = new float3(2f, 2f, 2f),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(CreatePhysicsDemoMaterialReference(PhysicsDemoRedMaterialRelativePath)),
                    CreateAutomaticComponentRecord(new city.rendering.MatrixRenderComponent {
                        BaseLocalPosition = float3.Zero,
                        MotionOffset = new float3(0f, 0f, 5f),
                        BaseLocalScale = new float3(2f, 2f, 2f),
                        ScaledLocalScale = new float3(4f, 1f, 2f),
                        RotatedLocalOrientation = CreateYawPitchRollDegrees(0.0, 0.0, 18.0),
                        PhaseDurationSeconds = 1.5d
                    }, 1)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the orbit camera used by the matrix render scene so the animated cube can be inspected from every side.
        /// </summary>
        /// <returns>Authored orbit camera entity centered on the matrix render motion path.</returns>
        SceneEntityAsset CreateMatrixRenderCameraEntity() {
            float3 orbitCenter = new float3(0f, 0f, 2.5f);
            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "Camera",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = new float3(0f, 3.5f, 10.5f),
                LocalScale = float3.One,
                LocalOrientation = CreateYawPitchRollDegrees(0.0, -18.0, 0.0),
                Components = new[] {
                    CreateCameraComponentRecord(),
                    CreateAutomaticComponentRecord(new city.rendering.DemoDiscOrbitCameraComponent {
                        OrbitCenter = orbitCenter,
                        AutoYawSpeedRadians = 0.08f
                    }, 1)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates a matrix render scene that isolates one animated hero cube and an orbit camera for transform inspection.
        /// </summary>
        /// <returns>Authored matrix render scene.</returns>
        SceneAsset CreateMatrixRenderScene() {
            SceneEntityAsset scenarioEntity = CreateMatrixRenderScenarioRoot();
            SceneEntityAsset cameraEntity = CreateMatrixRenderCameraEntity();
            return CreateMatrixRenderSceneAsset(PhysicsSceneCatalog.MatrixRenderSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the Matrix Render scenario root with its hero cube and a scene-specific key light tuned for clearer shape readback.
        /// </summary>
        /// <returns>Authored Matrix Render scenario root.</returns>
        SceneEntityAsset CreateMatrixRenderScenarioRoot() {
            SceneEntityAsset[] sceneChildren = new[] {
                CreateMatrixRenderHeroEntity(),
                CreateMatrixRenderKeyLightEntity()
            };
            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "Scenario",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = float3.Zero,
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = Array.Empty<SceneComponentAssetRecord>(),
                Children = sceneChildren
            };
        }

        /// <summary>
        /// Creates the dedicated Matrix Render key light so the animated cube reads clearly from a front-left three-quarter angle.
        /// </summary>
        /// <returns>Directional light entity authored only for the Matrix Render scene.</returns>
        SceneEntityAsset CreateMatrixRenderKeyLightEntity() {
            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "KeyLight",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = new float3(-3f, 5f, 4f),
                LocalScale = float3.One,
                LocalOrientation = CreateYawPitchRollDegrees(26.0, -28.0, 0.0),
                Components = new[] { CreateDirectionalLightComponentRecord() },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the static floor and dynamic sphere tower used by the sphere-stack validation scene.
        /// </summary>
        /// <returns>Scenario children containing a ground body, stacked spheres, and one spawn marker.</returns>
        SceneEntityAsset[] CreateDynamicSphereStackChildren() {
            List<SceneEntityAsset> children = new List<SceneEntityAsset>(10);
            children.Add(CreatePhysicsBoxMeshEntity("dynamic_sphere_stack.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, CreateGeneratedStandardMaterialReference()));

            for (int sphereIndex = 0; sphereIndex < 8; sphereIndex++) {
                int sphereNumber = sphereIndex + 1;
                float staggerX = sphereIndex % 2 == 0 ? 0f : 0.08f;
                float staggerZ = sphereIndex % 3 == 0 ? -0.06f : 0.06f;
                children.Add(CreatePhysicsSphereMeshEntity(
                    "dynamic_sphere_stack.sphere" + sphereNumber.ToString("00"),
                    "StackSphere" + sphereNumber.ToString("00"),
                    new float3(staggerX, 0.5f + sphereIndex, staggerZ),
                    float4.Identity,
                    DynamicBodyKindCode,
                    true,
                    CreateSphereStackMaterialReference(sphereIndex)));
            }

            children.Add(CreateMarkerEntity("dynamic_sphere_stack.spawn", "SphereStackSpawn", new float3(0f, 0.5f, 0f)));
            return children.ToArray();
        }

        /// <summary>
        /// Creates the mixed dynamic box and sphere stack validation scene.
        /// </summary>
        /// <returns>Authored mixed primitive-stack validation scene asset.</returns>
        SceneAsset CreateDynamicMixedStackScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "dynamic_mixed_stack.scenario",
                CreateDynamicMixedStackChildren());
            SceneEntityAsset cameraEntity = CreatePhysicsShowcaseCameraEntity("dynamic_mixed_stack.camera", new float3(9.5f, 6.5f, 9f), CreateYawPitchRollDegrees(45.0, -18.0, 0.0), float3.Zero);
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.DynamicMixedStackSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates one alternating stack of dynamic cubes and spheres to expose box-sphere and sphere-box contacts in the same scene.
        /// </summary>
        /// <returns>Scenario children containing a ground body, mixed primitive stack, and one spawn marker.</returns>
        SceneEntityAsset[] CreateDynamicMixedStackChildren() {
            return new[] {
                CreatePhysicsBoxMeshEntity("dynamic_mixed_stack.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                CreatePhysicsBoxMeshEntity("dynamic_mixed_stack.box01", "StackBox01", new float3(0f, 0.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
                CreatePhysicsSphereMeshEntity("dynamic_mixed_stack.sphere01", "StackSphere01", new float3(0.08f, 1.5f, -0.04f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackGreenMaterialRelativePath)),
                CreatePhysicsBoxMeshEntity("dynamic_mixed_stack.box02", "StackBox02", new float3(-0.06f, 2.5f, 0.05f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
                CreatePhysicsSphereMeshEntity("dynamic_mixed_stack.sphere02", "StackSphere02", new float3(0.05f, 3.5f, 0.08f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackYellowMaterialRelativePath)),
                CreatePhysicsBoxMeshEntity("dynamic_mixed_stack.box03", "StackBox03", new float3(0.07f, 4.5f, -0.07f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoCyanMaterialRelativePath)),
                CreatePhysicsSphereMeshEntity("dynamic_mixed_stack.sphere03", "StackSphere03", new float3(-0.05f, 5.5f, 0.04f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackRedMaterialRelativePath)),
                CreatePhysicsBoxMeshEntity("dynamic_mixed_stack.box04", "StackBox04", new float3(0.03f, 6.5f, 0.06f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoOrangeMaterialRelativePath)),
                CreatePhysicsSphereMeshEntity("dynamic_mixed_stack.sphere04", "StackSphere04", new float3(-0.04f, 7.5f, -0.05f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackPurpleMaterialRelativePath)),
                CreateMarkerEntity("dynamic_mixed_stack.spawn", "MixedStackSpawn", new float3(0f, 0.5f, 0f))
            };
        }

        /// <summary>
        /// Creates the kinematic push validation scene.
        /// </summary>
        /// <returns>Authored kinematic push validation scene asset.</returns>
        SceneAsset CreateKinematicPushScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "kinematic_push.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("kinematic_push.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 12f), float4.Identity, StaticBodyKindCode, false, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreatePhysicsBoxMeshEntity("kinematic_push.block", "DynamicTarget", new float3(1.5f, 0.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
                    CreateKinematicPhysicsBoxMeshEntity(
                        "kinematic_push.pusher",
                        "KinematicPusher",
                        new float3(-2f, 0.5f, 0f),
                        new float3(1.5f, 1f, 1.5f),
                        float4.Identity,
                        new float3(-2f, 0.5f, 0f),
                        new float3(0.5f, 0.5f, 0f),
                        1d,
                        true,
                        CreatePhysicsDemoMaterialReference(PhysicsDemoCyanMaterialRelativePath)),
                    CreateMarkerEntity("kinematic_push.start", "PusherStart", new float3(-3.5f, 0.5f, 0f)),
                    CreateMarkerEntity("kinematic_push.end", "PusherEnd", new float3(0.5f, 0.5f, 0f)),
                    CreateMarkerEntity("kinematic_push.dynamic_spawn", "DynamicSpawn", new float3(1.5f, 0.5f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("kinematic_push.camera", new float3(8.5f, 4.75f, 7.25f), CreateYawPitchRollDegrees(-135.0, -16.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.KinematicPushSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the static-mesh ground stability validation scene.
        /// </summary>
        /// <returns>Authored static-ground stability validation scene asset.</returns>
        SceneAsset CreateMeshGroundStabilityScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "mesh_ground_stability.scenario",
                new[] {
                    CreateCubeMeshEntity("mesh_ground_stability.base", "GroundBase", new float3(0f, -0.5f, 0f), new float3(20f, 1f, 14f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreateCubeMeshEntity("mesh_ground_stability.section01", "StaticMeshGround01", new float3(-2.5f, 0.15f, 0f), new float3(3f, 0.3f, 4f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
                    CreateCubeMeshEntity("mesh_ground_stability.section02", "StaticMeshGround02", new float3(0.5f, 0.35f, 0f), new float3(3f, 0.7f, 4f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath)),
                    CreateCubeMeshEntity("mesh_ground_stability.section03", "StaticMeshGround03", new float3(3.5f, 0.2f, 0f), new float3(3f, 0.4f, 4f), CreateYawPitchRollDegrees(0.0, 0.0, -6.0), CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
                    CreateCubeMeshEntity("mesh_ground_stability.section04", "StaticMeshGround04", new float3(6.5f, 0.45f, 0f), new float3(3f, 0.9f, 4f), CreateYawPitchRollDegrees(0.0, 0.0, 5.0), CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
                    CreateMarkerEntity("mesh_ground_stability.spawn", "WalkerSpawn", new float3(-5.5f, 0.75f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("mesh_ground_stability.camera", new float3(11f, 6f, 8.5f), CreateYawPitchRollDegrees(-140.0, -18.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.MeshGroundStabilitySceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the playable static-mesh showcase validation scene.
        /// </summary>
        /// <returns>Authored static-mesh showcase scene asset.</returns>
        SceneAsset CreateStaticMeshShowcaseScene() {
            SceneEntityAsset[] scenarioChildren = CreateStaticMeshShowcaseChildren();
            SceneEntityAsset playerSphereEntity = FindRequiredSceneEntityAssetByName(scenarioChildren, "PlayerSphere");
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "static_mesh_showcase.scenario",
                scenarioChildren);
            SceneEntityAsset cameraEntity = CreateStaticMeshShowcaseCameraEntity(
                "static_mesh_showcase.camera",
                new float3(12f, 6.5f, 10f),
                CreateYawPitchRollDegrees(-132.0, -18.0, 0.0),
                playerSphereEntity.Id);
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.StaticMeshShowcaseSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the minimal playable static-mesh validation scene.
        /// </summary>
        /// <returns>Authored minimal static-mesh playable scene asset.</returns>
        SceneAsset CreateStaticMeshMinimalScene() {
            SceneEntityAsset[] scenarioChildren = new[] {
                CreatePhysicsBoxMeshEntity(
                    "static_mesh_minimal.ground",
                    "Ground",
                    new float3(0f, -0.5f, 0f),
                    new float3(18f, 1f, 18f),
                    float4.Identity,
                    StaticBodyKindCode,
                    false,
                    CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                CreatePhysicsSphereMeshEntity(
                    "static_mesh_minimal.player",
                    "PlayerSphere",
                    new float3(0f, 0.75f, 0f),
                    float4.Identity,
                    DynamicBodyKindCode,
                    true,
                    CreateSphereStackMaterialReference(0))
            };
            SceneEntityAsset playerSphereEntity = FindRequiredSceneEntityAssetByName(scenarioChildren, "PlayerSphere");
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "static_mesh_minimal.scenario",
                scenarioChildren);
            SceneEntityAsset cameraEntity = CreateStaticMeshShowcaseCameraEntity(
                "static_mesh_minimal.camera",
                new float3(8f, 5f, 8f),
                CreateYawPitchRollDegrees(-135.0, -18.0, 0.0),
                playerSphereEntity.Id);
            return CreatePhysicsShowcaseSceneAsset(PhysicsSceneCatalog.StaticMeshMinimalSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the trigger-volume validation scene.
        /// </summary>
        /// <returns>Authored trigger-volume validation scene asset.</returns>
        SceneAsset CreateTriggerVolumeScene() {
            SceneEntityAsset scenarioEntity = CreateScenarioRoot(
                "trigger_volume.scenario",
                new[] {
                    CreateCubeMeshEntity("trigger_volume.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(18f, 1f, 12f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
                    CreateCubeMeshEntity("trigger_volume.arch", "TriggerVolume", new float3(1.5f, 1.5f, 0f), new float3(2.5f, 3f, 2.5f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoCyanMaterialRelativePath)),
                    CreateMarkerEntity("trigger_volume.start", "PlayerPathStart", new float3(-5f, 0.75f, 0f)),
                    CreateMarkerEntity("trigger_volume.end", "PlayerPathEnd", new float3(5.5f, 0.75f, 0f))
                });
            SceneEntityAsset cameraEntity = CreateCameraEntity("trigger_volume.camera", new float3(9.5f, 5f, 7.5f), CreateYawPitchRollDegrees(-136.0, -18.0, 0.0));
            return CreateSceneAsset(PhysicsSceneCatalog.TriggerVolumeSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the final scene asset wrapper shared by every validation scenario.
        /// </summary>
        /// <param name="sceneId">Stable relative scene id.</param>
        /// <param name="cameraEntity">Root camera entity.</param>
        /// <param name="scenarioEntity">Root scenario entity.</param>
        /// <returns>Scene asset ready for serialization.</returns>
        SceneAsset CreateSceneAsset(
            string sceneId,
            SceneEntityAsset cameraEntity,
            SceneEntityAsset scenarioEntity) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            }
            if (scenarioEntity == null) {
                throw new ArgumentNullException(nameof(scenarioEntity));
            }

            return new SceneAsset {
                Id = sceneId,
                AssetReferences = CreateAssetReferences(),
                RootEntities = new[] { cameraEntity, scenarioEntity }
            };
        }

        /// <summary>
        /// Creates one playable physics showcase scene asset that includes orbit controls, a functional light-toggle updater, and desktop instruction overlay content.
        /// </summary>
        /// <param name="sceneId">Stable relative scene id.</param>
        /// <param name="cameraEntity">Root camera entity.</param>
        /// <param name="scenarioEntity">Root scenario entity.</param>
        /// <returns>Playable showcase scene asset ready for serialization.</returns>
        SceneAsset CreatePhysicsShowcaseSceneAsset(
            string sceneId,
            SceneEntityAsset cameraEntity,
            SceneEntityAsset scenarioEntity) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            }
            if (scenarioEntity == null) {
                throw new ArgumentNullException(nameof(scenarioEntity));
            }

            List<SceneAssetReference> assetReferences = CreateSceneAssetReferenceList();
            HashSet<string> assetReferenceKeys = CreateSceneAssetReferenceKeySet(assetReferences);
            List<SceneEntityAsset> rootEntities = new List<SceneEntityAsset> {
                cameraEntity
            };

            EditorEntity instructionOverlayRootEntity = CreatePhysicsShowcaseDesktopInstructionOverlayRoot();
            try {
                rootEntities.Add(SerializeGeneratedEditorEntity(instructionOverlayRootEntity, assetReferences, assetReferenceKeys));
            } finally {
                instructionOverlayRootEntity.Dispose();
            }

            EditorEntity physicsShowcaseUiEntity = CreateLivePhysicsShowcaseUiEntity(ResolveDemoDiscSceneLabel(sceneId));
            try {
                ReassignGeneratedEditorEntityIds(physicsShowcaseUiEntity);
                rootEntities.Add(SerializeGeneratedEditorEntity(physicsShowcaseUiEntity, assetReferences, assetReferenceKeys));
            } finally {
                physicsShowcaseUiEntity.Dispose();
            }
            rootEntities.Add(scenarioEntity);

            return new SceneAsset {
                Id = sceneId,
                AssetReferences = assetReferences.ToArray(),
                RootEntities = rootEntities.ToArray()
            };
        }

        /// <summary>
        /// Creates the matrix-render scene asset with one generated UI root that provides FPS diagnostics and return-to-menu support.
        /// </summary>
        /// <param name="sceneId">Stable relative scene id.</param>
        /// <param name="cameraEntity">Root camera entity.</param>
        /// <param name="scenarioEntity">Root scenario entity.</param>
        /// <returns>Matrix-render scene asset ready for serialization.</returns>
        SceneAsset CreateMatrixRenderSceneAsset(
            string sceneId,
            SceneEntityAsset cameraEntity,
            SceneEntityAsset scenarioEntity) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            }
            if (scenarioEntity == null) {
                throw new ArgumentNullException(nameof(scenarioEntity));
            }

            List<SceneAssetReference> assetReferences = CreateSceneAssetReferenceList();
            HashSet<string> assetReferenceKeys = CreateSceneAssetReferenceKeySet(assetReferences);
            List<SceneEntityAsset> rootEntities = new List<SceneEntityAsset> {
                cameraEntity
            };

            EditorEntity matrixRenderUiEntity = CreateLiveMatrixRenderUiEntity();
            try {
                ReassignGeneratedEditorEntityIds(matrixRenderUiEntity);
                rootEntities.Add(SerializeGeneratedEditorEntity(matrixRenderUiEntity, assetReferences, assetReferenceKeys));
            } finally {
                matrixRenderUiEntity.Dispose();
            }

            rootEntities.Add(scenarioEntity);
            return new SceneAsset {
                Id = sceneId,
                AssetReferences = assetReferences.ToArray(),
                RootEntities = rootEntities.ToArray()
            };
        }

        /// <summary>
        /// Creates the authored scenario children for the playable static-mesh showcase scene.
        /// </summary>
        /// <returns>Scenario children containing visible environment meshes, one hidden static-mesh collider, and one dynamic sphere probe.</returns>
        SceneEntityAsset[] CreateStaticMeshShowcaseChildren() {
            List<SceneEntityAsset> children = new List<SceneEntityAsset>();
            List<float3> collisionVertices = new List<float3>();
            List<int> collisionIndices = new List<int>();

            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.ground",
                "Ground",
                new float3(0f, -0.5f, 0f),
                new float3(24f, 1f, 18f),
                float4.Identity,
                CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.wall_left",
                "WallLeft",
                new float3(-11.5f, 1.5f, 0f),
                new float3(1f, 3f, 18f),
                float4.Identity,
                CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.wall_right",
                "WallRight",
                new float3(11.5f, 1.5f, 0f),
                new float3(1f, 3f, 18f),
                float4.Identity,
                CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.wall_back",
                "WallBack",
                new float3(0f, 1.5f, -8.5f),
                new float3(24f, 3f, 1f),
                float4.Identity,
                CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.wall_front",
                "WallFront",
                new float3(0f, 1.5f, 8.5f),
                new float3(24f, 3f, 1f),
                float4.Identity,
                CreatePhysicsDemoMaterialReference(PhysicsDemoCyanMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.ramp_left",
                "RampLeft",
                new float3(-4.5f, 0.45f, -1.5f),
                new float3(6f, 0.5f, 4f),
                CreateYawPitchRollDegrees(0.0, 0.0, 12.0),
                CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.platform_center",
                "PlatformCenter",
                new float3(2f, 1.25f, -1.5f),
                new float3(4f, 0.5f, 4f),
                float4.Identity,
                CreatePhysicsDemoMaterialReference(PhysicsDemoOrangeMaterialRelativePath));
            AppendStaticMeshShowcaseSection(
                children,
                collisionVertices,
                collisionIndices,
                "static_mesh_showcase.ramp_right",
                "RampRight",
                new float3(6.25f, 1.65f, 1.5f),
                new float3(5f, 0.5f, 4f),
                CreateYawPitchRollDegrees(0.0, 0.0, -10.0),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackPurpleMaterialRelativePath));

            children.Add(CreateStaticMeshColliderEntity(
                "static_mesh_showcase.static_mesh_collider",
                "StaticMeshCollider",
                new StaticMeshCollisionData3D(collisionVertices.ToArray(), collisionIndices.ToArray())));
            children.Add(CreatePhysicsSphereMeshEntity(
                "static_mesh_showcase.player_sphere",
                "PlayerSphere",
                new float3(-8f, 1.1f, 0f),
                float4.Identity,
                DynamicBodyKindCode,
                true,
                CreateSphereStackMaterialReference(0)));
            children.Add(CreateMarkerEntity("static_mesh_showcase.spawn", "PlayerSpawn", new float3(-8f, 1.1f, 0f)));
            children.Add(CreateMarkerEntity("static_mesh_showcase.goal", "ShowcaseGoal", new float3(8f, 2.2f, 1.5f)));
            return children.ToArray();
        }

        /// <summary>
        /// Creates the scenario root entity that owns the authored test geometry and markers.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="children">Authored scenario children.</param>
        /// <returns>Scenario root entity.</returns>
        SceneEntityAsset CreateScenarioRoot(string entityId, SceneEntityAsset[] children) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Scenario entity id must be provided.", nameof(entityId));
            }
            if (children == null) {
                throw new ArgumentNullException(nameof(children));
            }

            SceneEntityAsset[] sceneChildren = AppendKeyLight(children);

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "Scenario",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = float3.Zero,
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = Array.Empty<SceneComponentAssetRecord>(),
                Children = sceneChildren
            };
        }

        /// <summary>
        /// Creates one camera root entity for a validation scene.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="position">Camera position.</param>
        /// <param name="orientation">Camera orientation.</param>
        /// <returns>Camera entity with a serialized camera component.</returns>
        SceneEntityAsset CreateCameraEntity(string entityId, float3 position, float4 orientation) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Camera entity id must be provided.", nameof(entityId));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "Camera",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] { CreateCameraComponentRecord() },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one mesh-backed cube entity for the validation scene.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <returns>Mesh-backed entity.</returns>
        SceneEntityAsset CreateCubeMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            SceneAssetReference materialReference) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Mesh entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Mesh entity name must be provided.", nameof(name));
            }
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = scale,
                LocalOrientation = orientation,
                Components = new[] { CreateMeshComponentRecord(materialReference) },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one mesh-backed box entity that also carries serialized 3D physics records.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale and collider size.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="bodyKindCode">Rigid-body participation mode byte to serialize.</param>
        /// <param name="useGravity">True when the serialized rigid body should receive gravity.</param>
        /// <returns>Mesh-backed entity with serialized rigid-body and box-collider records.</returns>
        SceneEntityAsset CreatePhysicsBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            byte bodyKindCode,
            bool useGravity,
            SceneAssetReference materialReference) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Physics entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Physics entity name must be provided.", nameof(name));
            }
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = scale,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateMeshComponentRecord(materialReference),
                    CreateRigidBodyComponentRecord(bodyKindCode, useGravity, 1d, 1d, float3.Zero, 1),
                    CreateBoxColliderComponentRecord(scale, 2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Serializes one generated editor-authored entity subtree into a scene-asset entity while collecting any component asset references referenced by the subtree.
        /// </summary>
        /// <param name="entity">Generated editor entity to serialize.</param>
        /// <param name="assetReferences">Scene-level asset references being accumulated.</param>
        /// <param name="assetReferenceKeys">Deduplication keys for the accumulated scene-level asset references.</param>
        /// <returns>Serialized scene-asset entity.</returns>
        SceneEntityAsset SerializeGeneratedEditorEntity(
            EditorEntity entity,
            List<SceneAssetReference> assetReferences,
            HashSet<string> assetReferenceKeys) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (assetReferences == null) {
                throw new ArgumentNullException(nameof(assetReferences));
            } else if (assetReferenceKeys == null) {
                throw new ArgumentNullException(nameof(assetReferenceKeys));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            List<SceneComponentAssetRecord> componentRecords = new List<SceneComponentAssetRecord>();
            int persistedComponentIndex = 0;
            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    Component component = entity.Components[componentIndex];
                    if (component == null || component is IEditorHiddenComponent) {
                        continue;
                    }

                    EntityComponentSaveState saveState = null;
                    if (saveComponent.TryGetComponentState(component, out EntityComponentSaveState existingSaveState)) {
                        saveState = existingSaveState;
                        NormalizeGeneratedEditorFontReference(component, saveState);
                    }

                    IComponentPersistenceDescriptor descriptor = PersistenceRegistry.GetDescriptor(component);
                    SceneComponentAssetRecord baseRecord = descriptor.SerializeComponent(component, persistedComponentIndex, saveState);
                    componentRecords.Add(OverridePayloadService.Wrap(baseRecord, saveState));
                    AppendAssetReferences(saveState, assetReferences, assetReferenceKeys);
                    persistedComponentIndex++;
                }
            }

            List<SceneEntityAsset> childEntities = new List<SceneEntityAsset>();
            if (entity.Children != null) {
                for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                    if (entity.Children[childIndex] is not EditorEntity childEntity) {
                        continue;
                    } else if (childEntity.InternalEntity) {
                        continue;
                    } else if (!childEntity.IsSceneOwned) {
                        continue;
                    }

                    childEntities.Add(SerializeGeneratedEditorEntity(childEntity, assetReferences, assetReferenceKeys));
                }
            }

            return new SceneEntityAsset {
                Id = saveComponent.EntityId,
                Name = entity.Name,
                IsStatic = entity.Static,
                Enabled = entity.Enabled,
                LayerMask = entity.LayerMask,
                LocalPosition = entity.LocalPosition,
                LocalScale = entity.LocalScale,
                LocalOrientation = entity.LocalOrientation,
                Components = componentRecords.ToArray(),
                PlatformTransformOverrides = Array.Empty<SceneEntityPlatformTransformOverrideAsset>(),
                PlatformComponentOverrides = Array.Empty<SceneEntityPlatformComponentOverrideAsset>(),
                Children = childEntities.ToArray()
            };
        }

        /// <summary>
        /// Creates one playable physics showcase camera entity that includes manual orbit controls around the supplied orbit center.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="position">Camera position.</param>
        /// <param name="orientation">Camera orientation.</param>
        /// <param name="orbitCenter">World-space orbit center assigned to the camera controller.</param>
        /// <returns>Camera entity with serialized camera and orbit-controller components.</returns>
        SceneEntityAsset CreatePhysicsShowcaseCameraEntity(string entityId, float3 position, float4 orientation, float3 orbitCenter) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Camera entity id must be provided.", nameof(entityId));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "Camera",
                LocalPosition = position,
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    CreateAutomaticComponentRecord(new city.rendering.DemoDiscOrbitCameraComponent {
                        OrbitCenter = orbitCenter,
                        AutoYawSpeedRadians = 0f
                    }, 1),
                    city.rendering.tools.DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one playable static-mesh showcase camera entity that follows the serialized player sphere by scene-entity id.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="position">Camera position.</param>
        /// <param name="orientation">Camera orientation.</param>
        /// <param name="targetEntityId">Stable serialized scene-entity id of the followed player sphere.</param>
        /// <returns>Camera entity with serialized camera and follow-camera components.</returns>
        SceneEntityAsset CreateStaticMeshShowcaseCameraEntity(string entityId, float3 position, float4 orientation, uint targetEntityId) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Camera entity id must be provided.", nameof(entityId));
            } else if (targetEntityId == 0u) {
                throw new ArgumentOutOfRangeException(nameof(targetEntityId), "Static-mesh showcase cameras require a non-zero followed scene entity id.");
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "Camera",
                LocalPosition = position,
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    CreateAutomaticComponentRecord(new city.rendering.DemoFollowCameraComponent {
                        TargetEntityReference = new SceneEntityReference {
                            EntityId = targetEntityId
                        },
                        TargetOffset = new float3(0f, 1.4f, 0f)
                    }, 1),
                    city.rendering.tools.DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// <summary>
        /// Creates the shared desktop instruction overlay root used by the playable physics showcase scenes.
        /// </summary>
        /// <returns>Live editor-authored overlay root entity ready for serialization.</returns>
        EditorEntity CreatePhysicsShowcaseDesktopInstructionOverlayRoot() {
            if (Core.Instance == null || Core.Instance.EntityFactory == null) {
                throw new InvalidOperationException("Creating the physics showcase instruction overlay requires an active editor entity factory.");
            } else if (string.IsNullOrWhiteSpace(CurrentProjectRootPath)) {
                throw new InvalidOperationException("Creating the physics showcase instruction overlay requires an active project root path.");
            }

            city.rendering.tools.DemoSceneInstructionOverlayFactory instructionOverlayFactory = new city.rendering.tools.DemoSceneInstructionOverlayFactory();
            Entity overlayRootEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(CurrentProjectRootPath, ResolveRequiredEditorFont());
            if (overlayRootEntity is not EditorEntity editorOverlayRootEntity) {
                throw new InvalidOperationException("The physics showcase instruction overlay must be authored through editor entities.");
            }

            ReassignGeneratedEditorEntityIds(editorOverlayRootEntity);
            return editorOverlayRootEntity;
        }

        /// <summary>
        /// Creates one mesh-backed sphere entity that also carries serialized 3D physics records.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="bodyKindCode">Rigid-body participation mode byte to serialize.</param>
        /// <param name="useGravity">True when the serialized rigid body should receive gravity.</param>
        /// <param name="materialReference">Material reference used by the visible sphere mesh.</param>
        /// <returns>Mesh-backed entity with serialized rigid-body and sphere-collider records.</returns>
        SceneEntityAsset CreatePhysicsSphereMeshEntity(
            string entityId,
            string name,
            float3 position,
            float4 orientation,
            byte bodyKindCode,
            bool useGravity,
            SceneAssetReference materialReference) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Physics sphere entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Physics sphere entity name must be provided.", nameof(name));
            }
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateMeshComponentRecord(global::helengine.EngineSceneAssetReferenceFactory.CreateSphereModel(), materialReference),
                    CreateRigidBodyComponentRecord(bodyKindCode, useGravity, 1d, 1d, float3.Zero, 1),
                    CreateSphereColliderComponentRecord(0.5f, 2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one hidden entity that owns the combined static-mesh collider for the playable showcase environment.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="collisionData">Combined authored triangle soup used for static-mesh physics cooking.</param>
        /// <returns>Entity with one static rigid body and one static-mesh collider component.</returns>
        SceneEntityAsset CreateStaticMeshColliderEntity(
            string entityId,
            string name,
            StaticMeshCollisionData3D collisionData) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Static-mesh collider entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Static-mesh collider entity name must be provided.", nameof(name));
            }
            if (collisionData == null) {
                throw new ArgumentNullException(nameof(collisionData));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = float3.Zero,
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateRigidBodyComponentRecord(StaticBodyKindCode, false, 1d, 1d, float3.Zero, 1),
                    CreateStaticMeshColliderComponentRecord(collisionData, 2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one mesh-backed box entity that also carries serialized 3D kinematic-motion records.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale and collider size.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="startLocalPosition">Kinematic motion start position.</param>
        /// <param name="endLocalPosition">Kinematic motion end position.</param>
        /// <param name="travelDurationSeconds">One-way travel duration in seconds.</param>
        /// <param name="pingPong">True when the motion should reverse at the end.</param>
        /// <returns>Mesh-backed entity with serialized rigid-body, box-collider, and kinematic-motion records.</returns>
        SceneEntityAsset CreateKinematicPhysicsBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            float3 startLocalPosition,
            float3 endLocalPosition,
            double travelDurationSeconds,
            bool pingPong,
            SceneAssetReference materialReference) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Physics entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Physics entity name must be provided.", nameof(name));
            }
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = scale,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateMeshComponentRecord(materialReference),
                    CreateRigidBodyComponentRecord(KinematicBodyKindCode, false, 1d, 1d, float3.Zero, 1),
                    CreateBoxColliderComponentRecord(scale, 2),
                    CreateKinematicMotionComponentRecord(startLocalPosition, endLocalPosition, travelDurationSeconds, pingPong, 3)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one mesh-backed box entity that carries serialized 3D character-controller records.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale and collider size.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="desiredMoveDirection">Desired local move direction used by the controller.</param>
        /// <param name="moveSpeed">Horizontal move speed in world units per second.</param>
        /// <param name="gravityScale">Gravity multiplier used by the controller.</param>
        /// <param name="stepHeight">Maximum upward snap height used while climbing support surfaces.</param>
        /// <param name="groundSnapDistance">Maximum downward snap distance used to keep the controller grounded.</param>
        /// <returns>Mesh-backed entity with serialized box-collider and character-controller records.</returns>
        SceneEntityAsset CreateCharacterControllerBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            float3 desiredMoveDirection,
            double moveSpeed,
            double gravityScale,
            double stepHeight,
            double groundSnapDistance,
            SceneAssetReference materialReference) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Character controller entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Character controller entity name must be provided.", nameof(name));
            }
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = scale,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateMeshComponentRecord(materialReference),
                    CreateBoxColliderComponentRecord(scale, 1),
                    CreateCharacterControllerComponentRecord(desiredMoveDirection, moveSpeed, gravityScale, stepHeight, groundSnapDistance, 2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Adds one visible cuboid section to the static-mesh showcase and appends matching authored collision triangles to the shared static-mesh collider.
        /// </summary>
        /// <param name="children">Scenario child list receiving the visible mesh entity.</param>
        /// <param name="collisionVertices">Shared authored collision vertex list.</param>
        /// <param name="collisionIndices">Shared authored collision index list.</param>
        /// <param name="entityId">Stable serialized entity id for the visible section.</param>
        /// <param name="name">Authored entity name for the visible section.</param>
        /// <param name="position">World-space cuboid center.</param>
        /// <param name="scale">Full cuboid size.</param>
        /// <param name="orientation">World-space cuboid orientation.</param>
        /// <param name="materialReference">Visible material reference.</param>
        void AppendStaticMeshShowcaseSection(
            List<SceneEntityAsset> children,
            List<float3> collisionVertices,
            List<int> collisionIndices,
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            SceneAssetReference materialReference) {
            if (children == null) {
                throw new ArgumentNullException(nameof(children));
            } else if (collisionVertices == null) {
                throw new ArgumentNullException(nameof(collisionVertices));
            } else if (collisionIndices == null) {
                throw new ArgumentNullException(nameof(collisionIndices));
            }

            children.Add(CreateCubeMeshEntity(entityId, name, position, scale, orientation, materialReference));
            AppendCuboidCollisionData(collisionVertices, collisionIndices, position, scale, orientation);
        }

        /// <summary>
        /// Creates one empty marker entity used as a future spawn, target, or motion reference.
        /// </summary>
        /// <param name="entityId">Stable serialized entity id.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Marker position.</param>
        /// <returns>Marker entity without components.</returns>
        SceneEntityAsset CreateMarkerEntity(string entityId, string name, float3 position) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Marker entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Marker entity name must be provided.", nameof(name));
            }

            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = name,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = position,
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = Array.Empty<SceneComponentAssetRecord>(),
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the mutable asset-reference list used while building one playable physics showcase scene.
        /// </summary>
        /// <returns>Mutable scene-level asset-reference list initialized with the shared primitive and material dependencies.</returns>
        List<SceneAssetReference> CreateSceneAssetReferenceList() {
            return new List<SceneAssetReference>(CreateAssetReferences());
        }

        /// <summary>
        /// Creates one deduplication-key set preloaded from the supplied asset-reference list.
        /// </summary>
        /// <param name="assetReferences">Existing scene-level asset references that should seed the deduplication set.</param>
        /// <returns>Deduplication keys matching the supplied asset-reference list.</returns>
        HashSet<string> CreateSceneAssetReferenceKeySet(List<SceneAssetReference> assetReferences) {
            if (assetReferences == null) {
                throw new ArgumentNullException(nameof(assetReferences));
            }

            HashSet<string> assetReferenceKeys = new HashSet<string>(StringComparer.Ordinal);
            for (int referenceIndex = 0; referenceIndex < assetReferences.Count; referenceIndex++) {
                SceneAssetReference reference = assetReferences[referenceIndex];
                if (reference == null) {
                    continue;
                }

                assetReferenceKeys.Add(BuildAssetReferenceKey(reference));
            }

            return assetReferenceKeys;
        }

        /// <summary>
        /// Creates the shared generated-asset reference list used by validation scene mesh components.
        /// </summary>
        /// <returns>Stable generated asset reference list.</returns>
        static SceneAssetReference[] CreateAssetReferences() {
            return new[] {
                global::helengine.EngineSceneAssetReferenceFactory.CreateCubeModel(),
                global::helengine.EngineSceneAssetReferenceFactory.CreateSphereModel(),
                CreateGeneratedStandardMaterialReference(),
                CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoNeutralMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoGreenMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoCyanMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoRedMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoOrangeMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoPurpleMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackBlueMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackGreenMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackMagentaMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackYellowMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackCyanMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackRedMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackOrangeMaterialRelativePath),
                CreatePhysicsDemoMaterialReference(PhysicsDemoSphereStackPurpleMaterialRelativePath)
            };
        }

        /// <summary>
        /// Creates the file-backed material reference assigned to one dynamic sphere in the sphere-stack validation scene.
        /// </summary>
        /// <param name="sphereIndex">Zero-based sphere index.</param>
        /// <returns>Distinct colored material reference for the requested sphere.</returns>
        static SceneAssetReference CreateSphereStackMaterialReference(int sphereIndex) {
            if (sphereIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(sphereIndex), "Sphere index must be non-negative.");
            }

            string[] materialPaths = {
                PhysicsDemoSphereStackBlueMaterialRelativePath,
                PhysicsDemoSphereStackGreenMaterialRelativePath,
                PhysicsDemoSphereStackMagentaMaterialRelativePath,
                PhysicsDemoSphereStackYellowMaterialRelativePath,
                PhysicsDemoSphereStackCyanMaterialRelativePath,
                PhysicsDemoSphereStackRedMaterialRelativePath,
                PhysicsDemoSphereStackOrangeMaterialRelativePath,
                PhysicsDemoSphereStackPurpleMaterialRelativePath
            };
            return CreatePhysicsDemoMaterialReference(materialPaths[sphereIndex % materialPaths.Length]);
        }

        /// <summary>
        /// Creates one file-backed scene asset reference used for the exported physics demo materials.
        /// </summary>
        /// <param name="relativePath">Relative project asset path.</param>
        /// <returns>Scene asset reference targeting a file-backed asset.</returns>
        static SceneAssetReference CreatePhysicsDemoMaterialReference(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            return global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(relativePath);
        }

        /// <summary>
        /// Creates one generated reference to the platform-owned standard material.
        /// </summary>
        /// <returns>Generated standard material scene asset reference.</returns>
        static SceneAssetReference CreateGeneratedStandardMaterialReference() {
            return global::helengine.EngineSceneAssetReferenceFactory.CreateStandardMaterial();
        }

        /// <summary>
        /// Creates one serialized mesh component record that references the generated cube model and standard material.
        /// </summary>
        /// <returns>Serialized mesh component record.</returns>
        static SceneComponentAssetRecord CreateMeshComponentRecord(SceneAssetReference materialReference) {
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            SceneAssetReference modelReference = global::helengine.EngineSceneAssetReferenceFactory.CreateCubeModel();

            return CreateMeshComponentRecord(modelReference, materialReference);
        }

        /// <summary>
        /// Creates one serialized mesh component record that references the supplied generated model and material.
        /// </summary>
        /// <param name="modelReference">Generated model reference serialized into the mesh component payload.</param>
        /// <param name="materialReference">Material reference serialized into the mesh component payload.</param>
        /// <returns>Serialized mesh component record.</returns>
        static SceneComponentAssetRecord CreateMeshComponentRecord(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            }
            if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField(MeshModelReferenceFieldName, fieldWriter => SceneComponentBinaryFieldEncoding.WriteOptionalReference(fieldWriter, modelReference));
            writer.WriteField(MeshMaterialReferencesFieldName, fieldWriter => SceneComponentBinaryFieldEncoding.WriteOptionalReferenceArray(fieldWriter, new[] { materialReference }));
            writer.WriteField(MeshRenderOrder3DFieldName, fieldWriter => fieldWriter.WriteByte(DefaultMeshRenderOrder));

            return new SceneComponentAssetRecord {
                ComponentTypeId = "helengine.MeshComponent",
                ComponentIndex = 0,
                Payload = writer.BuildPayload()
            };
        }

        /// <summary>
        /// Creates one serialized camera component record using the shared reflected component serializer.
        /// </summary>
        /// <returns>Serialized camera component record.</returns>
        static SceneComponentAssetRecord CreateCameraComponentRecord() {
            CameraComponent component = new CameraComponent {
                CameraDrawOrder = DefaultCameraDrawOrder,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100f,
                ClearSettings = new CameraClearSettings(true, CornflowerBlueClearColor, true, 1f, false, 0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            };

            return CreateAutomaticComponentRecord(component, 0);
        }

        /// <summary>
        /// Appends the shared key light to the authored scenario children.
        /// </summary>
        /// <param name="children">Authored scenario children.</param>
        /// <returns>Copied child array with the shared key light appended at the end.</returns>
        SceneEntityAsset[] AppendKeyLight(SceneEntityAsset[] children) {
            if (children == null) {
                throw new ArgumentNullException(nameof(children));
            }

            SceneEntityAsset[] sceneChildren = new SceneEntityAsset[children.Length + 1];
            Array.Copy(children, sceneChildren, children.Length);
            sceneChildren[children.Length] = CreateKeyLightEntity();
            return sceneChildren;
        }

        /// <summary>
        /// Creates the shared directional light used to give the exported validation scenes stronger shape and visible shadows.
        /// </summary>
        /// <returns>Directional light entity appended to each scenario root.</returns>
        SceneEntityAsset CreateKeyLightEntity() {
            return new SceneEntityAsset {
                Id = AllocateSceneEntityId(),
                Name = "KeyLight",
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = new float3(0f, 6f, 0f),
                LocalScale = float3.One,
                LocalOrientation = CreateYawPitchRollDegrees(-48.0, -44.0, 0.0),
                Components = new[] { CreateDirectionalLightComponentRecord() },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one serialized directional light component record configured for shadowed validation-scene rendering.
        /// </summary>
        /// <returns>Directional light scene component payload.</returns>
        static SceneComponentAssetRecord CreateDirectionalLightComponentRecord() {
            DirectionalLightComponent lightComponent = new DirectionalLightComponent {
                Color = new float4(1.0f, 0.96f, 0.90f, 1.0f),
                Intensity = 1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f
            };
            return CreateAutomaticComponentRecord(lightComponent, 0);
        }

        /// <summary>
        /// Writes the shared texture and material assets consumed by the exported physics validation scenes.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        static void WriteSupportAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            DeleteObsoletePhysicsDemoShaderAsset(projectRootPath);
            WriteSphereTileTextureAssets(projectRootPath);
            WriteMaterialAsset(projectRootPath, PhysicsDemoGroundMaterialRelativePath, "PhysicsDemoGround", new float4(0.77f, 0.80f, 0.84f, 1.0f), false, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoNeutralMaterialRelativePath, "PhysicsDemoNeutral", new float4(0.77f, 0.80f, 0.84f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoBlueMaterialRelativePath, "PhysicsDemoBlue", new float4(0.33f, 0.56f, 0.90f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoGreenMaterialRelativePath, "PhysicsDemoGreen", new float4(0.38f, 0.76f, 0.49f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoMagentaMaterialRelativePath, "PhysicsDemoMagenta", new float4(0.84f, 0.42f, 0.73f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoYellowMaterialRelativePath, "PhysicsDemoYellow", new float4(0.92f, 0.79f, 0.33f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoCyanMaterialRelativePath, "PhysicsDemoCyan", new float4(0.31f, 0.79f, 0.82f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoRedMaterialRelativePath, "PhysicsDemoRed", new float4(0.90f, 0.32f, 0.29f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoOrangeMaterialRelativePath, "PhysicsDemoOrange", new float4(0.95f, 0.52f, 0.22f, 1.0f), true, true);
            WriteMaterialAsset(projectRootPath, PhysicsDemoPurpleMaterialRelativePath, "PhysicsDemoPurple", new float4(0.55f, 0.43f, 0.92f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackBlueMaterialRelativePath, "PhysicsDemoSphereStackBlue", new float4(0.33f, 0.56f, 0.90f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackGreenMaterialRelativePath, "PhysicsDemoSphereStackGreen", new float4(0.38f, 0.76f, 0.49f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackMagentaMaterialRelativePath, "PhysicsDemoSphereStackMagenta", new float4(0.84f, 0.42f, 0.73f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackYellowMaterialRelativePath, "PhysicsDemoSphereStackYellow", new float4(0.92f, 0.79f, 0.33f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackCyanMaterialRelativePath, "PhysicsDemoSphereStackCyan", new float4(0.31f, 0.79f, 0.82f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackRedMaterialRelativePath, "PhysicsDemoSphereStackRed", new float4(0.90f, 0.32f, 0.29f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackOrangeMaterialRelativePath, "PhysicsDemoSphereStackOrange", new float4(0.95f, 0.52f, 0.22f, 1.0f), true, true);
            WriteTexturedMaterialAsset(projectRootPath, PhysicsDemoSphereStackPurpleMaterialRelativePath, "PhysicsDemoSphereStackPurple", new float4(0.55f, 0.43f, 0.92f, 1.0f), true, true);
        }

        /// <summary>
        /// Deletes the obsolete custom shader generated by older physics demo material exports.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        static void DeleteObsoletePhysicsDemoShaderAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string shaderFullPath = Path.Combine(projectRootPath, "assets", "Shaders", "physics", "PhysicsDemoMesh.hlsl");
            string shaderSettingsFullPath = shaderFullPath + ".hasset";
            if (File.Exists(shaderFullPath)) {
                File.Delete(shaderFullPath);
            }
            if (File.Exists(shaderSettingsFullPath)) {
                File.Delete(shaderSettingsFullPath);
            }
        }

        /// <summary>
        /// Returns whether the supplied scene id belongs to a curated playable physics showcase scene that needs orbit controls and instruction overlays.
        /// </summary>
        /// <param name="sceneId">Stable scene id under evaluation.</param>
        /// <returns>True when the scene id belongs to a playable physics showcase scene.</returns>
        static bool IsPlayablePhysicsShowcaseScene(string sceneId) {
            return string.Equals(sceneId, PhysicsSceneCatalog.DynamicStackBoxesSceneId, StringComparison.Ordinal)
                || string.Equals(sceneId, PhysicsSceneCatalog.SingleFallingCubeSceneId, StringComparison.Ordinal)
                || string.Equals(sceneId, PhysicsSceneCatalog.DynamicSphereStackSceneId, StringComparison.Ordinal)
                || string.Equals(sceneId, PhysicsSceneCatalog.StrictRotatedBoxCompareSceneId, StringComparison.Ordinal)
                || string.Equals(sceneId, PhysicsSceneCatalog.DynamicMixedStackSceneId, StringComparison.Ordinal)
                || string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshShowcaseSceneId, StringComparison.Ordinal)
                || string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshMinimalSceneId, StringComparison.Ordinal);
        }

        static string ResolveDemoDiscSceneLabel(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicStackBoxesSceneId, StringComparison.Ordinal)) {
                return "8. Stacked Boxes";
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicSphereStackSceneId, StringComparison.Ordinal)) {
                return "9. Sphere Stack";
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicMixedStackSceneId, StringComparison.Ordinal)) {
                return "10. Mixed Stack";
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshShowcaseSceneId, StringComparison.Ordinal)) {
                return "11. Static Mesh";
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshMinimalSceneId, StringComparison.Ordinal)) {
                return "12. Simple Mesh";
            }
            return string.Empty;
        }

        /// <summary>
        /// Writes one playable physics showcase scene through the live authoring save pipeline so the desktop instruction overlay persists with the same metadata contract used by the rendering demo scenes.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        /// <param name="sceneId">Stable playable scene id to write.</param>
        void WritePlayablePhysicsShowcaseScene(string projectRootPath, string sceneId) {
            city.rendering.tools.GeneratedAuthoringSceneDefinition sceneDefinition = CreatePlayablePhysicsShowcaseSceneDefinition(projectRootPath, sceneId, true);
            AuthoringSceneWriteService.WriteScene(projectRootPath, sceneDefinition);
        }

        /// <summary>
        /// Builds one live authored playable physics showcase scene definition that can be written directly or reused by DS companion-scene generation without reloading the desktop `.helen` file.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the scene asset dependencies.</param>
        /// <param name="sceneId">Stable playable scene id to build.</param>
        /// <param name="includeDesktopInstructionOverlay">True when the desktop instruction overlay root should remain in the returned root-entity list.</param>
        /// <returns>Generated live-authored playable scene definition.</returns>
        public city.rendering.tools.GeneratedAuthoringSceneDefinition CreatePlayablePhysicsShowcaseSceneDefinition(
            string projectRootPath,
            string sceneId,
            bool includeDesktopInstructionOverlay) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (Core.Instance == null || Core.Instance.ContentManager == null) {
                throw new InvalidOperationException("Writing playable physics showcase scenes requires an active editor content manager.");
            }

            CurrentProjectRootPath = Path.GetFullPath(projectRootPath);
            string normalizedSceneId = NormalizePlayablePhysicsShowcaseSceneId(sceneId);
            SceneAsset authoredSceneAsset;
            Entity cameraEntity;
            if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.DynamicStackBoxesSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateDynamicStackBoxesScene();
                cameraEntity = CreateLivePhysicsShowcaseCameraEntity(
                    "DynamicStackBoxesCamera",
                    new float3(2.25f, 4.8f, 10.25f),
                    CreateYawPitchRollDegrees(8.0, -16.0, 0.0),
                    new float3(0.75f, 1.5f, 0f));
            } else if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.SingleFallingCubeSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateSingleFallingCubeScene();
                cameraEntity = CreateLivePhysicsShowcaseCameraEntity(
                    "SingleFallingCubeCamera",
                    new float3(7f, 4.5f, 7f),
                    CreateYawPitchRollDegrees(-135.0, -16.0, 0.0),
                    new float3(0f, 2.25f, 0f));
            } else if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.DynamicSphereStackSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateDynamicSphereStackScene();
                cameraEntity = CreateLivePhysicsShowcaseCameraEntity(
                    "DynamicSphereStackCamera",
                    new float3(7.75f, 4.75f, 7.5f),
                    CreateYawPitchRollDegrees(-135.0, -18.0, 0.0),
                    new float3(0f, 1.6f, 0f));
            } else if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.StrictRotatedBoxCompareSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateStrictRotatedBoxCompareScene();
                cameraEntity = CreateLivePhysicsShowcaseCameraEntity(
                    "StrictRotatedBoxCompareCamera",
                    new float3(0f, 7.5f, 15.5f),
                    CreateYawPitchRollDegrees(180.0, -20.0, 0.0),
                    new float3(0f, 1.5f, 0f));
            } else if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.DynamicMixedStackSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateDynamicMixedStackScene();
                cameraEntity = CreateLivePhysicsShowcaseCameraEntity(
                    "DynamicMixedStackCamera",
                    new float3(8.5f, 5f, 8.25f),
                    CreateYawPitchRollDegrees(-136.0, -18.0, 0.0),
                    new float3(0f, 1.4f, 0f));
            } else if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.StaticMeshShowcaseSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateStaticMeshShowcaseScene();
                SceneEntityAsset staticMeshScenarioRoot = ResolveRequiredPlayablePhysicsShowcaseScenarioRoot(authoredSceneAsset);
                SceneEntityAsset playerSphereEntity = FindRequiredSceneEntityAssetByName(staticMeshScenarioRoot.Children, "PlayerSphere");
                cameraEntity = CreateLiveStaticMeshShowcaseCameraEntity(
                    "StaticMeshShowcaseCamera",
                    new float3(12f, 6.5f, 10f),
                    CreateYawPitchRollDegrees(-132.0, -18.0, 0.0),
                    playerSphereEntity.Id);
            } else if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.StaticMeshMinimalSceneId, StringComparison.Ordinal)) {
                authoredSceneAsset = CreateStaticMeshMinimalScene();
                SceneEntityAsset staticMeshScenarioRoot = ResolveRequiredPlayablePhysicsShowcaseScenarioRoot(authoredSceneAsset);
                SceneEntityAsset playerSphereEntity = FindRequiredSceneEntityAssetByName(staticMeshScenarioRoot.Children, "PlayerSphere");
                cameraEntity = CreateLiveStaticMeshShowcaseCameraEntity(
                    "StaticMeshMinimalCamera",
                    new float3(8f, 5f, 8f),
                    CreateYawPitchRollDegrees(-135.0, -18.0, 0.0),
                    playerSphereEntity.Id);
            } else {
                throw new InvalidOperationException($"Scene '{sceneId}' is not one of the playable physics showcases.");
            }

            List<Entity> rootEntities = new List<Entity> {
                cameraEntity,
                CreateLivePhysicsShowcaseUiEntity(ResolveDemoDiscSceneLabel(normalizedSceneId))
            };
            if (includeDesktopInstructionOverlay) {
                city.rendering.tools.DemoSceneInstructionOverlayFactory instructionOverlayFactory = new city.rendering.tools.DemoSceneInstructionOverlayFactory();
                FontAsset instructionFont = ResolveRequiredEditorFont();
                Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
                city.rendering.tools.ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new city.rendering.tools.ConsoleCameraLightInstructionsSceneAttachmentService();
                consoleInstructionAttachmentService.ExcludeLegacyOverlayFromConsoles(projectRootPath, instructionOverlayEntity);
                rootEntities.Insert(1, instructionOverlayEntity);
                rootEntities.Insert(2, consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath));
            }

            IReadOnlyList<EditorEntity> scenarioRoots = LoadPlayablePhysicsShowcaseScenarioRoots(projectRootPath, authoredSceneAsset);
            for (int index = 0; index < scenarioRoots.Count; index++) {
                rootEntities.Add(scenarioRoots[index]);
            }
            for (int index = 0; index < rootEntities.Count; index++) {
                if (rootEntities[index] is not EditorEntity editorRootEntity) {
                    throw new InvalidOperationException("Playable physics showcase roots must be editor entities before they can be saved.");
                }

                AssignFreshGeneratedEditorEntityIds(editorRootEntity);
            }
            if (string.Equals(normalizedSceneId, PhysicsSceneCatalog.StaticMeshShowcaseSceneId, StringComparison.Ordinal)
                || string.Equals(normalizedSceneId, PhysicsSceneCatalog.StaticMeshMinimalSceneId, StringComparison.Ordinal)) {
                RebindStaticMeshShowcaseCameraTarget(cameraEntity, scenarioRoots);
            }

            return new city.rendering.tools.GeneratedAuthoringSceneDefinition {
                SceneId = normalizedSceneId,
                SceneSettings = authoredSceneAsset.SceneSettings,
                RootEntities = rootEntities.ToArray()
            };
        }

        /// <summary>
        /// Normalizes playable showcase scene identifiers so callers may use either authored asset ids or the shorter logical ids exposed by the demo-disc menu catalog.
        /// </summary>
        /// <param name="sceneId">Playable scene identifier supplied by the caller.</param>
        /// <returns>Normalized authored asset scene id.</returns>
        static string NormalizePlayablePhysicsShowcaseSceneId(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            if (string.Equals(sceneId, "test_scene_dynamic_stack_boxes", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.DynamicStackBoxesSceneId;
            } else if (string.Equals(sceneId, "test_scene_single_falling_cube", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.SingleFallingCubeSceneId;
            } else if (string.Equals(sceneId, "test_scene_dynamic_sphere_stack", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.DynamicSphereStackSceneId;
            } else if (string.Equals(sceneId, "test_scene_strict_rotated_box_compare", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.StrictRotatedBoxCompareSceneId;
            } else if (string.Equals(sceneId, "test_scene_dynamic_mixed_stack", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.DynamicMixedStackSceneId;
            } else if (string.Equals(sceneId, "test_scene_static_mesh_showcase", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.StaticMeshShowcaseSceneId;
            } else if (string.Equals(sceneId, "test_scene_static_mesh_minimal", StringComparison.Ordinal)) {
                return PhysicsSceneCatalog.StaticMeshMinimalSceneId;
            }

            return sceneId;
        }

        /// <summary>
        /// Loads the serialized scenario root from one playable physics showcase scene into live editor entities so the generated desktop overlay can be saved through the standard authoring pipeline.
        /// </summary>
        /// <param name="projectRootPath">Absolute city project root path that owns the scene asset dependencies.</param>
        /// <param name="authoredSceneAsset">Playable showcase scene asset whose scenario subtree should be materialized.</param>
        /// <returns>Live editor entities that represent the serialized scenario subtree.</returns>
        IReadOnlyList<EditorEntity> LoadPlayablePhysicsShowcaseScenarioRoots(string projectRootPath, SceneAsset authoredSceneAsset) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (authoredSceneAsset == null) {
                throw new ArgumentNullException(nameof(authoredSceneAsset));
            }

            SceneEntityAsset scenarioRootEntity = ResolveRequiredPlayablePhysicsShowcaseScenarioRoot(authoredSceneAsset);
            ComponentPersistenceRegistry persistenceRegistry = city.rendering.tools.GeneratedScenePersistenceRegistryFactory.Create();
            ContentManager assetContentManager = new ContentManager(new HostFileSystemContentStreamSource(Path.Combine(projectRootPath, "assets")));
            EditorContentManagerConfiguration.ConfigureSharedAssetContentManager(assetContentManager);
            EditorSceneAssetReferenceResolver referenceResolver = new EditorSceneAssetReferenceResolver(assetContentManager, projectRootPath);
            SceneLoadService sceneLoadService = new SceneLoadService(persistenceRegistry, referenceResolver);
            SceneAsset scenarioSceneAsset = new SceneAsset {
                Id = authoredSceneAsset.Id,
                SceneSettings = authoredSceneAsset.SceneSettings,
                AssetReferences = authoredSceneAsset.AssetReferences,
                RootEntities = new[] {
                    scenarioRootEntity
                }
            };
            return sceneLoadService.Load(scenarioSceneAsset);
        }

        /// <summary>
        /// Resolves the serialized scenario root from one playable physics showcase scene asset.
        /// </summary>
        /// <param name="authoredSceneAsset">Playable showcase scene asset whose scenario root should be extracted.</param>
        /// <returns>Serialized scenario root entity.</returns>
        static SceneEntityAsset ResolveRequiredPlayablePhysicsShowcaseScenarioRoot(SceneAsset authoredSceneAsset) {
            if (authoredSceneAsset == null) {
                throw new ArgumentNullException(nameof(authoredSceneAsset));
            }

            SceneEntityAsset[] rootEntities = authoredSceneAsset.RootEntities;
            if (rootEntities == null || rootEntities.Length == 0) {
                throw new InvalidOperationException("Playable physics showcase scenes must define at least one root entity.");
            }

            SceneEntityAsset scenarioRootEntity = rootEntities[rootEntities.Length - 1];
            if (scenarioRootEntity == null) {
                throw new InvalidOperationException("Playable physics showcase scenes must end with a scenario root entity.");
            }

            return scenarioRootEntity;
        }

        /// <summary>
        /// Creates one live authored camera entity for a playable physics showcase scene.
        /// </summary>
        /// <param name="entityName">Human-readable camera entity name.</param>
        /// <param name="position">Initial camera position.</param>
        /// <param name="orientation">Initial camera orientation.</param>
        /// <param name="orbitCenter">Point orbited by manual showcase controls.</param>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateLivePhysicsShowcaseCameraEntity(string entityName, float3 position, float4 orientation, float3 orbitCenter) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Camera entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.Create(entityName);
            entity.LocalPosition = position;
            entity.LocalOrientation = orientation;

            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = DefaultCameraDrawOrder,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100f,
                ClearSettings = new CameraClearSettings(true, CornflowerBlueClearColor, true, 1f, false, 0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = orbitCenter,
                AutoYawSpeedRadians = 0f
            });
            entity.AddComponent(new city.menu.DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates one live authored camera entity for the static-mesh showcase that follows the serialized player sphere by scene-entity id.
        /// </summary>
        /// <param name="entityName">Human-readable camera entity name.</param>
        /// <param name="position">Initial camera position.</param>
        /// <param name="orientation">Initial camera orientation.</param>
        /// <param name="targetEntityId">Stable serialized scene-entity id of the followed player sphere.</param>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateLiveStaticMeshShowcaseCameraEntity(string entityName, float3 position, float4 orientation, uint targetEntityId) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Camera entity name must be provided.", nameof(entityName));
            } else if (targetEntityId == 0u) {
                throw new ArgumentOutOfRangeException(nameof(targetEntityId), "Static-mesh showcase cameras require a non-zero followed scene entity id.");
            }

            Entity entity = Core.Instance.EntityFactory.Create(entityName);
            entity.LocalPosition = position;
            entity.LocalOrientation = orientation;

            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = DefaultCameraDrawOrder,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100f,
                ClearSettings = new CameraClearSettings(true, CornflowerBlueClearColor, true, 1f, false, 0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoFollowCameraComponent {
                TargetEntityReference = new SceneEntityReference {
                    EntityId = targetEntityId
                },
                TargetOffset = new float3(0f, 1.4f, 0f)
            });
            entity.AddComponent(new city.menu.DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Rebinds the static-mesh showcase follow camera so its serialized target reference matches the freshly assigned live player-sphere save id.
        /// </summary>
        /// <param name="cameraEntity">Generated camera entity whose follow target should be updated.</param>
        /// <param name="scenarioRoots">Generated showcase scenario roots that contain the player sphere.</param>
        void RebindStaticMeshShowcaseCameraTarget(Entity cameraEntity, IReadOnlyList<EditorEntity> scenarioRoots) {
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            } else if (scenarioRoots == null) {
                throw new ArgumentNullException(nameof(scenarioRoots));
            }

            city.rendering.DemoFollowCameraComponent followCameraComponent = FindRequiredDemoFollowCameraComponent(cameraEntity);
            EditorEntity playerSphereEntity = FindRequiredEditorEntityByName(scenarioRoots, "PlayerSphere");
            EntitySaveComponent playerSphereSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            followCameraComponent.TargetEntityReference = new SceneEntityReference {
                EntityId = playerSphereSaveComponent.EntityId
            };
        }

        /// <summary>
        /// Creates one live authored UI root that shows FPS diagnostics and owns the playable showcase light-toggle updater.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        EditorEntity CreateLivePhysicsShowcaseUiEntity(string sceneLabel) {
            Entity entity = Core.Instance.EntityFactory.Create("ShowcaseUi");
            FPSComponent fpsComponent = new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            };
            entity.AddComponent(fpsComponent);
            city.rendering.tools.PspFpsComponentOverrideService.Apply(entity);
            ApplyEditorFontReference(entity, fpsComponent);
            entity.AddComponent(new city.rendering.DemoDiscLightToggleComponent());
            DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory();
            lightIndicatorOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont());
            if (!string.IsNullOrWhiteSpace(sceneLabel)) {
                city.rendering.tools.DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new city.rendering.tools.DemoDiscSceneLabelOverlayFactory();
                sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), sceneLabel);
            }
            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("The physics showcase UI root must be authored through editor entities.");
        }

        /// <summary>
        /// Creates one live authored UI root for the render-motion-probe scene so desktop and DS builds share FPS and return-to-menu behavior.
        /// </summary>
        /// <returns>Live authored UI entity for the render-motion-probe scene.</returns>
        EditorEntity CreateLiveMatrixRenderUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("MatrixRenderUi");
            FPSComponent fpsComponent = new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            };
            entity.AddComponent(fpsComponent);
            city.rendering.tools.PspFpsComponentOverrideService.Apply(entity);
            ApplyEditorFontReference(entity, fpsComponent);
            entity.AddComponent(new city.menu.DemoDiscReturnToMenuComponent());
            Entity phaseStatusEntity = Core.Instance.EntityFactory.CreateChild(entity, "MatrixRenderPhaseStatus");
            phaseStatusEntity.LocalPosition = new float3(16f, 392f, 0f);
            phaseStatusEntity.Static = false;
            TextComponent phaseStatusTextComponent = new TextComponent {
                Text = "Operation: Translation",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(1024, 56),
                FontScale = 1.5f,
                RenderOrder2D = 1,
            };
            phaseStatusEntity.AddComponent(phaseStatusTextComponent);
            ApplyEditorFontReference(phaseStatusEntity, phaseStatusTextComponent);
            phaseStatusEntity.AddComponent(new city.rendering.MatrixRenderPhaseStatusTextComponent());
            city.rendering.tools.DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new city.rendering.tools.DemoDiscSceneLabelOverlayFactory();
            sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "6. Matrix Render");
            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("The render-motion-probe UI root must be authored through editor entities.");
        }

        /// <summary>
        /// Finds one serialized scene entity by display name inside the supplied subtree.
        /// </summary>
        /// <param name="entities">Serialized scene entities to inspect.</param>
        /// <param name="entityName">Display name to resolve.</param>
        /// <returns>Matching serialized scene entity.</returns>
        SceneEntityAsset FindRequiredSceneEntityAssetByName(SceneEntityAsset[] entities, string entityName) {
            if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                SceneEntityAsset entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }
                if (string.Equals(entity.Name, entityName, StringComparison.Ordinal)) {
                    return entity;
                }

                SceneEntityAsset nestedMatch = FindSceneEntityAssetByNameOrNull(entity.Children, entityName);
                if (nestedMatch != null) {
                    return nestedMatch;
                }
            }

            throw new InvalidOperationException($"Expected one serialized scene entity named '{entityName}'.");
        }

        /// <summary>
        /// Finds one serialized scene entity by display name inside the supplied subtree when present.
        /// </summary>
        /// <param name="entities">Serialized scene entities to inspect.</param>
        /// <param name="entityName">Display name to resolve.</param>
        /// <returns>Matching serialized scene entity when present; otherwise <c>null</c>.</returns>
        SceneEntityAsset FindSceneEntityAssetByNameOrNull(SceneEntityAsset[] entities, string entityName) {
            if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                SceneEntityAsset entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }
                if (string.Equals(entity.Name, entityName, StringComparison.Ordinal)) {
                    return entity;
                }

                SceneEntityAsset nestedMatch = FindSceneEntityAssetByNameOrNull(entity.Children, entityName);
                if (nestedMatch != null) {
                    return nestedMatch;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds one generated live editor entity by display name inside the supplied roots.
        /// </summary>
        /// <param name="entities">Generated live editor roots to inspect.</param>
        /// <param name="entityName">Display name to resolve.</param>
        /// <returns>Matching generated live editor entity.</returns>
        EditorEntity FindRequiredEditorEntityByName(IReadOnlyList<EditorEntity> entities, string entityName) {
            if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                EditorEntity entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }
                if (string.Equals(entity.Name, entityName, StringComparison.Ordinal)) {
                    return entity;
                }

                EditorEntity nestedMatch = FindEditorEntityByNameOrNull(entity.Children, entityName);
                if (nestedMatch != null) {
                    return nestedMatch;
                }
            }

            throw new InvalidOperationException($"Expected one generated editor entity named '{entityName}'.");
        }

        /// <summary>
        /// Finds one generated live editor entity by display name inside the supplied subtree when present.
        /// </summary>
        /// <param name="entities">Generated live editor entities to inspect.</param>
        /// <param name="entityName">Display name to resolve.</param>
        /// <returns>Matching generated live editor entity when present; otherwise <c>null</c>.</returns>
        EditorEntity FindEditorEntityByNameOrNull(List<Entity> entities, string entityName) {
            if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                if (entities[entityIndex] is not EditorEntity entity) {
                    continue;
                }
                if (string.Equals(entity.Name, entityName, StringComparison.Ordinal)) {
                    return entity;
                }

                EditorEntity nestedMatch = FindEditorEntityByNameOrNull(entity.Children, entityName);
                if (nestedMatch != null) {
                    return nestedMatch;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the generated follow-camera component attached to one camera entity.
        /// </summary>
        /// <param name="entity">Camera entity whose follow-camera component should be returned.</param>
        /// <returns>Attached generated follow-camera component.</returns>
        city.rendering.DemoFollowCameraComponent FindRequiredDemoFollowCameraComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated camera entities must expose initialized component collections.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is city.rendering.DemoFollowCameraComponent followCameraComponent) {
                    return followCameraComponent;
                }
            }

            throw new InvalidOperationException("Generated static-mesh showcase cameras must include DemoFollowCameraComponent.");
        }
        /// <summary>
        /// Stores the shared generated UI-font reference on the entity save state for the supplied FPS component.
        /// </summary>
        /// <param name="entity">Entity that owns the FPS component.</param>
        /// <param name="component">FPS component whose font reference should be stored.</param>
        void ApplyEditorFontReference(Entity entity, FPSComponent component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());
        }

        /// <summary>
        /// Stores the shared authored body-font reference on the entity save state for the supplied text component.
        /// </summary>
        /// <param name="entity">Entity that owns the text component.</param>
        /// <param name="component">Text component whose font reference should be stored.</param>
        void ApplyEditorFontReference(Entity entity, TextComponent component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

        /// <summary>
        /// Rewrites any stale generated-overlay font save reference before one generated overlay component is serialized through the manual physics showcase scene path.
        /// </summary>
        /// <param name="component">Generated overlay component currently being serialized.</param>
        /// <param name="saveState">Save metadata that should carry the normalized font reference.</param>
        void NormalizeGeneratedEditorFontReference(Component component, EntityComponentSaveState saveState) {
            if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (saveState == null) {
                throw new ArgumentNullException(nameof(saveState));
            }

            if (component is FPSComponent fpsComponent) {
                NormalizeGeneratedEditorFontReference(fpsComponent.Font, saveState, true);
            } else if (component is DebugComponent debugComponent) {
                NormalizeGeneratedEditorFontReference(debugComponent.Font, saveState, false);
            } else if (component is TextComponent textComponent) {
                NormalizeGeneratedEditorFontReference(textComponent.Font, saveState, false);
            }
        }

        /// <summary>
        /// Stores the shared authored body-font reference when one generated overlay component uses the active editor font instance.
        /// </summary>
        /// <param name="font">Runtime font assigned to the generated overlay component.</param>
        /// <param name="saveState">Save metadata that should carry the normalized font reference.</param>
        void NormalizeGeneratedEditorFontReference(FontAsset font, EntityComponentSaveState saveState, bool useEditorUiFont) {
            if (saveState == null) {
                throw new ArgumentNullException(nameof(saveState));
            } else if (font == null) {
                return;
            } else if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                return;
            } else if (!ReferenceEquals(font, editorCore.DefaultFontAssetForEditor)) {
                return;
            }

            saveState.SetAssetReference(
                "Font",
                useEditorUiFont
                    ? DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference()
                    : DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Writes the generated authored source texture, import sidecar, and cached texture asset used by the sphere-stack materials.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        static void WriteSphereTileTextureAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            WriteSphereTileTextureSource(projectRootPath);
            WriteSphereTileTextureCacheAsset(projectRootPath);
        }

        /// <summary>
        /// Writes the generated sphere-stack tile texture source bitmap and its import-settings sidecar.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        static void WriteSphereTileTextureSource(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullPath = Path.Combine(projectRootPath, "assets", PhysicsDemoSphereTileTextureRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a texture directory for '{PhysicsDemoSphereTileTextureRelativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(fullPath, PhysicsDemoSphereTileTextureBytes);

            using FileStream stream = File.Create(fullPath + ".hasset");
            AssetImportSettingsBinarySerializer.Serialize(stream, CreateSphereTileTextureImportSettings(PhysicsDemoSphereTileTextureBytes));
        }

        /// <summary>
        /// Writes the cached runtime texture asset paired with the generated sphere-stack tile texture source.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `cache` directory.</param>
        static void WriteSphereTileTextureCacheAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string cachePath = Path.Combine(projectRootPath, "cache", PhysicsDemoSphereTileTextureAssetId);
            string directoryPath = Path.GetDirectoryName(cachePath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a texture cache directory for '{PhysicsDemoSphereTileTextureAssetId}'.");
            }

            Directory.CreateDirectory(directoryPath);
            using FileStream stream = File.Create(cachePath);
            global::helengine.editor.AssetSerializer.Serialize(stream, CreateSphereTileTextureAsset());
        }

        /// <summary>
        /// Creates the import-settings payload paired with the generated sphere-stack tile texture source.
        /// </summary>
        /// <param name="textureBytes">Generated authored texture bytes whose checksum should be persisted into the sidecar.</param>
        /// <returns>Import settings that match the generated sphere-stack tile texture source.</returns>
        static AssetImportSettings CreateSphereTileTextureImportSettings(byte[] textureBytes) {
            if (textureBytes == null) {
                throw new ArgumentNullException(nameof(textureBytes));
            }

            AssetImportSettings settings = new AssetImportSettings();
            settings.Importer.ImporterId = TextureImporterId;
            settings.Importer.SourceChecksum = ComputeSourceChecksum(textureBytes);
            settings.Importer.AssetId = PhysicsDemoSphereTileTextureAssetId;
            return settings;
        }

        /// <summary>
        /// Creates the cached runtime texture asset that matches the generated sphere-stack tile texture bitmap.
        /// </summary>
        /// <returns>Cached runtime texture asset for the sphere-stack materials.</returns>
        static TextureAsset CreateSphereTileTextureAsset() {
            return new TextureAsset {
                Width = PhysicsDemoSphereTileTextureWidth,
                Height = PhysicsDemoSphereTileTextureHeight,
                Colors = BuildPhysicsDemoSphereTileTextureAssetColors()
            };
        }

        /// <summary>
        /// Writes one file-backed textured material asset used by the exported sphere-stack validation scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        /// <param name="relativePath">Relative project asset path for the material file.</param>
        /// <param name="assetId">Serialized material asset identifier.</param>
        /// <param name="surfaceColor">Authored standard material base color.</param>
        /// <param name="castsShadows">True when the material should cast dynamic shadows where supported.</param>
        /// <param name="receivesShadows">True when the material should receive dynamic shadows where supported.</param>
        static void WriteTexturedMaterialAsset(string projectRootPath, string relativePath, string assetId, float4 surfaceColor, bool castsShadows, bool receivesShadows) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            }

            string baseColor = ConvertColorToHtml(surfaceColor);
            string castsShadowValue = castsShadows ? "true" : "false";
            string receivesShadowValue = receivesShadows ? "true" : "false";

            city.rendering.tools.GeneratedMaterialAssetDefinition definition = new city.rendering.tools.GeneratedMaterialAssetDefinition {
                MaterialAsset = new ShaderMaterialAsset {
                    Id = assetId,
                    RenderState = new MaterialRenderState(),
                    CastsShadows = castsShadows,
                    ReceivesShadows = receivesShadows
                }
            };

            city.rendering.tools.GeneratedMaterialPlatformDefinition windowsSettings = definition.GetOrCreatePlatform("windows");
            windowsSettings.SchemaId = StandardShaderSchemaId;
            windowsSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            windowsSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            windowsSettings.SetFieldValue(TextureAssetIdFieldId, PhysicsDemoSphereTileTextureAssetId);
            windowsSettings.SetFieldValue(CastsShadowFieldId, castsShadowValue);
            windowsSettings.SetFieldValue(ReceivesShadowFieldId, receivesShadowValue);
            windowsSettings.SetFieldValue(BaseColorFieldId, baseColor);

            city.rendering.tools.GeneratedMaterialPlatformDefinition ps2Settings = definition.GetOrCreatePlatform("ps2");
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.SetFieldValue(TextureAssetIdFieldId, PhysicsDemoSphereTileTextureAssetId);
            ps2Settings.SetFieldValue(AlphaModeFieldId, "opaque");
            ps2Settings.SetFieldValue(DoubleSidedFieldId, "false");
            ps2Settings.SetFieldValue(Ps2CastShadowsFieldId, castsShadowValue);
            ps2Settings.SetFieldValue(VertexColorModeFieldId, "ignore");
            ps2Settings.SetFieldValue(BaseColorFieldId, baseColor);

            city.rendering.tools.GeneratedMaterialPlatformDefinition pspSettings = definition.GetOrCreatePlatform("psp");
            pspSettings.SchemaId = StandardShaderSchemaId;
            pspSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            pspSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            pspSettings.SetFieldValue(TextureAssetIdFieldId, PhysicsDemoSphereTileTextureAssetId);
            pspSettings.SetFieldValue(CastsShadowFieldId, castsShadowValue);
            pspSettings.SetFieldValue(ReceivesShadowFieldId, receivesShadowValue);
            pspSettings.SetFieldValue(BaseColorFieldId, baseColor);

            city.rendering.tools.GeneratedMaterialPlatformDefinition gameCubeSettings = definition.GetOrCreatePlatform("gamecube");
            gameCubeSettings.SchemaId = GameCubeMaterialSchemaId;
            gameCubeSettings.SetFieldValue(TextureAssetIdFieldId, PhysicsDemoSphereTileTextureAssetId);
            gameCubeSettings.SetFieldValue(GameCubeTextureRelativePathFieldId, "cooked/imported/" + PhysicsDemoSphereTileTextureAssetId);
            gameCubeSettings.SetFieldValue(DoubleSidedFieldId, "false");
            gameCubeSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            gameCubeSettings.SetFieldValue(BaseColorFieldId, baseColor);
            gameCubeSettings.SetFieldValue(LightingModeFieldId, "lit");

            city.rendering.tools.GeneratedMaterialPlatformDefinition dsSettings = definition.GetOrCreatePlatform("ds");
            dsSettings.SchemaId = DsMaterialSchemaId;
            dsSettings.SetFieldValue(TextureAssetIdFieldId, PhysicsDemoSphereTileTextureAssetId);
            dsSettings.SetFieldValue(DsTextureRelativePathFieldId, "cooked/imported/" + PhysicsDemoSphereTileTextureAssetId);
            dsSettings.SetFieldValue(DoubleSidedFieldId, "false");
            dsSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            dsSettings.SetFieldValue(BaseColorFieldId, baseColor);
            dsSettings.SetFieldValue(LightingModeFieldId, "lit");

            city.rendering.tools.GeneratedMaterialAssetWriteService writeService = new city.rendering.tools.GeneratedMaterialAssetWriteService();
            writeService.WriteMaterial(projectRootPath, relativePath, definition);
        }

        /// <summary>
        /// Builds the BMP file bytes written to the generated sphere-stack tile texture source file.
        /// </summary>
        /// <returns>24-bit BMP bytes for the shared sphere-stack tile texture.</returns>
        static byte[] BuildPhysicsDemoSphereTileTextureFileBytes() {
            int rowStride = ((PhysicsDemoSphereTileTextureWidth * 3) + 3) & ~3;
            int pixelDataLength = rowStride * PhysicsDemoSphereTileTextureHeight;
            int pixelDataOffset = 14 + 40;
            int fileLength = pixelDataOffset + pixelDataLength;
            byte[] fileBytes = new byte[fileLength];

            fileBytes[0] = (byte)'B';
            fileBytes[1] = (byte)'M';
            WriteInt32(fileBytes, 2, fileLength);
            WriteInt32(fileBytes, 10, pixelDataOffset);
            WriteInt32(fileBytes, 14, 40);
            WriteInt32(fileBytes, 18, PhysicsDemoSphereTileTextureWidth);
            WriteInt32(fileBytes, 22, PhysicsDemoSphereTileTextureHeight);
            WriteInt16(fileBytes, 26, 1);
            WriteInt16(fileBytes, 28, 24);
            WriteInt32(fileBytes, 34, pixelDataLength);

            for (int y = 0; y < PhysicsDemoSphereTileTextureHeight; y++) {
                int rowOffset = pixelDataOffset + ((PhysicsDemoSphereTileTextureHeight - 1 - y) * rowStride);
                for (int x = 0; x < PhysicsDemoSphereTileTextureWidth; x++) {
                    ResolvePhysicsDemoSphereTilePixelColor(x, y, out byte red, out byte green, out byte blue, out _);
                    int pixelOffset = rowOffset + (x * 3);
                    fileBytes[pixelOffset + 0] = blue;
                    fileBytes[pixelOffset + 1] = green;
                    fileBytes[pixelOffset + 2] = red;
                }
            }

            return fileBytes;
        }

        /// <summary>
        /// Builds the runtime RGBA pixel payload paired with the generated sphere-stack tile texture bitmap.
        /// </summary>
        /// <returns>Top-down row-major RGBA pixel bytes.</returns>
        static byte[] BuildPhysicsDemoSphereTileTextureAssetColors() {
            byte[] colors = new byte[PhysicsDemoSphereTileTextureWidth * PhysicsDemoSphereTileTextureHeight * 4];

            for (int y = 0; y < PhysicsDemoSphereTileTextureHeight; y++) {
                for (int x = 0; x < PhysicsDemoSphereTileTextureWidth; x++) {
                    ResolvePhysicsDemoSphereTilePixelColor(x, y, out byte red, out byte green, out byte blue, out byte alpha);
                    int pixelOffset = ((y * PhysicsDemoSphereTileTextureWidth) + x) * 4;
                    colors[pixelOffset + 0] = red;
                    colors[pixelOffset + 1] = green;
                    colors[pixelOffset + 2] = blue;
                    colors[pixelOffset + 3] = alpha;
                }
            }

            return colors;
        }

        /// <summary>
        /// Resolves one grayscale tile pixel for the shared sphere-stack rotation texture.
        /// </summary>
        /// <param name="x">Zero-based pixel column.</param>
        /// <param name="y">Zero-based pixel row.</param>
        /// <param name="red">Resolved red channel.</param>
        /// <param name="green">Resolved green channel.</param>
        /// <param name="blue">Resolved blue channel.</param>
        /// <param name="alpha">Resolved alpha channel.</param>
        static void ResolvePhysicsDemoSphereTilePixelColor(int x, int y, out byte red, out byte green, out byte blue, out byte alpha) {
            int tileX = x / PhysicsDemoSphereTileTextureTileSize;
            int tileY = y / PhysicsDemoSphereTileTextureTileSize;
            int localX = x % PhysicsDemoSphereTileTextureTileSize;
            int localY = y % PhysicsDemoSphereTileTextureTileSize;
            bool isGrout = localX < PhysicsDemoSphereTileTextureGroutThickness
                || localY < PhysicsDemoSphereTileTextureGroutThickness
                || localX >= PhysicsDemoSphereTileTextureTileSize - PhysicsDemoSphereTileTextureGroutThickness
                || localY >= PhysicsDemoSphereTileTextureTileSize - PhysicsDemoSphereTileTextureGroutThickness;
            bool isAccent = !isGrout
                && localX >= 3
                && localX <= 7
                && localY >= 3
                && localY <= 7;

            byte luminance;
            if (isGrout) {
                luminance = 72;
            } else if (isAccent) {
                luminance = 252;
            } else if (((tileX + tileY) & 1) == 0) {
                luminance = 214;
            } else {
                luminance = 168;
            }

            red = luminance;
            green = luminance;
            blue = luminance;
            alpha = byte.MaxValue;
        }

        /// <summary>
        /// Computes the stable lowercase SHA-256 checksum string stored in the generated sphere-stack texture sidecar.
        /// </summary>
        /// <param name="sourceBytes">Texture source bytes to hash.</param>
        /// <returns>Lowercase hexadecimal SHA-256 checksum string.</returns>
        static string ComputeSourceChecksum(byte[] sourceBytes) {
            if (sourceBytes == null) {
                throw new ArgumentNullException(nameof(sourceBytes));
            }

            byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(sourceBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Builds one importer-qualified texture asset id using the same identity scheme as the editor import pipeline.
        /// </summary>
        /// <param name="sourceChecksum">Lowercase hexadecimal source checksum.</param>
        /// <param name="importerId">Registered texture importer identifier.</param>
        /// <returns>Importer-qualified lowercase asset identifier.</returns>
        static string BuildImporterQualifiedAssetId(string sourceChecksum, string importerId) {
            if (string.IsNullOrWhiteSpace(sourceChecksum)) {
                throw new ArgumentException("Source checksum must be provided.", nameof(sourceChecksum));
            } else if (string.IsNullOrWhiteSpace(importerId)) {
                throw new ArgumentException("Importer id must be provided.", nameof(importerId));
            }

            string identity = string.Concat("importer", "\n", sourceChecksum, "\n", importerId);
            byte[] identityBytes = System.Text.Encoding.UTF8.GetBytes(identity);
            byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(identityBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Writes one 32-bit little-endian integer into the supplied buffer.
        /// </summary>
        /// <param name="buffer">Buffer receiving the encoded integer.</param>
        /// <param name="offset">Destination byte offset.</param>
        /// <param name="value">Value to encode.</param>
        static void WriteInt32(byte[] buffer, int offset, int value) {
            if (buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }

            byte[] encodedBytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(encodedBytes, 0, buffer, offset, 4);
        }

        /// <summary>
        /// Writes one 16-bit little-endian integer into the supplied buffer.
        /// </summary>
        /// <param name="buffer">Buffer receiving the encoded integer.</param>
        /// <param name="offset">Destination byte offset.</param>
        /// <param name="value">Value to encode.</param>
        static void WriteInt16(byte[] buffer, int offset, short value) {
            if (buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }

            byte[] encodedBytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(encodedBytes, 0, buffer, offset, 2);
        }

        /// <summary>
        /// Writes one file-backed material asset used by the exported physics validation scenes.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        /// <param name="relativePath">Relative project asset path for the material file.</param>
        /// <param name="assetId">Serialized material asset identifier.</param>
        /// <param name="surfaceColor">Authored standard material base color.</param>
        static void WriteMaterialAsset(string projectRootPath, string relativePath, string assetId, float4 surfaceColor, bool castsShadows, bool receivesShadows) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            }

            city.rendering.tools.GeneratedMaterialAssetDefinition definition = new city.rendering.tools.GeneratedMaterialAssetDefinition {
                MaterialAsset = new ShaderMaterialAsset {
                    Id = assetId,
                    RenderState = new MaterialRenderState(),
                    CastsShadows = castsShadows,
                    ReceivesShadows = receivesShadows
                }
            };

            IReadOnlyList<string> supportedPlatforms = new EditorProjectPlatformsService(projectRootPath).Load().SupportedPlatforms;
            for (int platformIndex = 0; platformIndex < supportedPlatforms.Count; platformIndex++) {
                city.rendering.tools.GeneratedMaterialPlatformDefinition platformDefinition = definition.GetOrCreatePlatform(supportedPlatforms[platformIndex]);
                platformDefinition.SchemaId = StandardShaderSchemaId;
                platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
                platformDefinition.SetFieldValue(TextureAssetIdFieldId, string.Empty);
                platformDefinition.SetFieldValue(CastsShadowFieldId, castsShadows ? "true" : "false");
                platformDefinition.SetFieldValue(ReceivesShadowFieldId, receivesShadows ? "true" : "false");
                platformDefinition.SetFieldValue(BaseColorFieldId, ConvertColorToHtml(surfaceColor));
            }

            city.rendering.tools.GeneratedMaterialAssetWriteService writeService = new city.rendering.tools.GeneratedMaterialAssetWriteService();
            writeService.WriteMaterial(projectRootPath, relativePath, definition);
        }

        /// <summary>
        /// Converts one normalized color into the material settings HTML color format.
        /// </summary>
        /// <param name="color">Normalized color value to serialize.</param>
        /// <returns>HTML color string in #RRGGBBAA format.</returns>
        static string ConvertColorToHtml(float4 color) {
            return string.Concat(
                "#",
                ConvertColorChannelToByte(color.X).ToString("X2"),
                ConvertColorChannelToByte(color.Y).ToString("X2"),
                ConvertColorChannelToByte(color.Z).ToString("X2"),
                ConvertColorChannelToByte(color.W).ToString("X2"));
        }

        /// <summary>
        /// Converts one normalized color channel into an 8-bit channel value.
        /// </summary>
        /// <param name="value">Normalized channel value.</param>
        /// <returns>Clamped byte channel.</returns>
        static byte ConvertColorChannelToByte(float value) {
            double scaledValue = Math.Round(Math.Clamp((double)value, 0d, 1d) * 255d, MidpointRounding.AwayFromZero);
            return (byte)scaledValue;
        }

        /// <summary>
        /// Creates one serialized rigid-body component record.
        /// </summary>
        /// <param name="bodyKindCode">Rigid-body participation mode byte to serialize.</param>
        /// <param name="useGravity">True when gravity should be enabled.</param>
        /// <param name="mass">Serialized authored mass value.</param>
        /// <param name="gravityScale">Serialized authored gravity scale.</param>
        /// <param name="linearVelocity">Serialized authored linear velocity.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized rigid-body component record.</returns>
        static SceneComponentAssetRecord CreateRigidBodyComponentRecord(
            byte bodyKindCode,
            bool useGravity,
            double mass,
            double gravityScale,
            float3 linearVelocity,
            int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            RigidBody3DComponent component = new RigidBody3DComponent {
                AngularVelocity = float3.Zero,
                BodyKind = ResolveBodyKind(bodyKindCode),
                GravityScale = gravityScale,
                LinearVelocity = linearVelocity,
                Mass = mass,
                UseGravity = useGravity
            };
            return CreateAutomaticComponentRecord(component, componentIndex);
        }

        /// <summary>
        /// Creates one serialized box-collider component record.
        /// </summary>
        /// <param name="size">Serialized authored full collider size.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized box-collider component record.</returns>
        static SceneComponentAssetRecord CreateBoxColliderComponentRecord(float3 size, int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            BoxCollider3DComponent component = new BoxCollider3DComponent {
                CollisionLayer = 1,
                CollisionMask = ushort.MaxValue,
                DynamicFriction = 0.4d,
                IsTrigger = false,
                Restitution = 0d,
                Size = size,
                StaticFriction = 0.6d
            };
            return CreateAutomaticComponentRecord(component, componentIndex);
        }

        /// <summary>
        /// Creates one serialized sphere-collider component record.
        /// </summary>
        /// <param name="radius">Serialized authored sphere radius.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized sphere-collider component record.</returns>
        static SceneComponentAssetRecord CreateSphereColliderComponentRecord(float radius, int componentIndex) {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(radius), "Sphere radius must be a finite value greater than zero.");
            }
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            SphereCollider3DComponent component = new SphereCollider3DComponent {
                CollisionLayer = 1,
                CollisionMask = ushort.MaxValue,
                DynamicFriction = 0.4d,
                IsTrigger = false,
                Radius = radius,
                Restitution = 0d,
                StaticFriction = 0.6d
            };
            return CreateAutomaticComponentRecord(component, componentIndex);
        }

        /// <summary>
        /// Creates one serialized static-mesh collider component record.
        /// </summary>
        /// <param name="collisionData">Authored triangle soup stored on the static-mesh collider component.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized static-mesh collider component record.</returns>
        static SceneComponentAssetRecord CreateStaticMeshColliderComponentRecord(StaticMeshCollisionData3D collisionData, int componentIndex) {
            if (collisionData == null) {
                throw new ArgumentNullException(nameof(collisionData));
            }
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            StaticMeshCollider3DComponent component = new StaticMeshCollider3DComponent {
                CollisionData = collisionData,
                CollisionLayer = 1,
                CollisionMask = ushort.MaxValue,
                DynamicFriction = 0.4d,
                IsTrigger = false,
                Restitution = 0d,
                StaticFriction = 0.6d
            };
            return CreateAutomaticComponentRecord(component, componentIndex);
        }

        /// <summary>
        /// Appends one oriented cuboid as authored triangle soup to the supplied static-mesh collision buffers.
        /// </summary>
        /// <param name="vertices">Shared authored collision vertex list.</param>
        /// <param name="indices">Shared authored collision index list.</param>
        /// <param name="position">World-space cuboid center.</param>
        /// <param name="scale">Full cuboid size.</param>
        /// <param name="orientation">World-space cuboid orientation.</param>
        static void AppendCuboidCollisionData(
            List<float3> vertices,
            List<int> indices,
            float3 position,
            float3 scale,
            float4 orientation) {
            if (vertices == null) {
                throw new ArgumentNullException(nameof(vertices));
            } else if (indices == null) {
                throw new ArgumentNullException(nameof(indices));
            }

            int vertexStartIndex = vertices.Count;
            float halfX = scale.X * 0.5f;
            float halfY = scale.Y * 0.5f;
            float halfZ = scale.Z * 0.5f;
            float3[] localVertices = new[] {
                new float3(-halfX, -halfY, -halfZ),
                new float3(halfX, -halfY, -halfZ),
                new float3(halfX, halfY, -halfZ),
                new float3(-halfX, halfY, -halfZ),
                new float3(-halfX, -halfY, halfZ),
                new float3(halfX, -halfY, halfZ),
                new float3(halfX, halfY, halfZ),
                new float3(-halfX, halfY, halfZ)
            };
            int[] localIndices = new[] {
                0, 2, 1,
                0, 3, 2,
                4, 5, 6,
                4, 6, 7,
                0, 1, 5,
                0, 5, 4,
                1, 2, 6,
                1, 6, 5,
                2, 3, 7,
                2, 7, 6,
                3, 0, 4,
                3, 4, 7
            };

            for (int index = 0; index < localVertices.Length; index++) {
                vertices.Add(TransformCollisionVertex(localVertices[index], position, orientation));
            }

            for (int index = 0; index < localIndices.Length; index++) {
                indices.Add(vertexStartIndex + localIndices[index]);
            }
        }

        /// <summary>
        /// Transforms one authored local collision vertex into world space.
        /// </summary>
        /// <param name="localVertex">Local collision vertex relative to the cuboid origin.</param>
        /// <param name="position">World-space cuboid center.</param>
        /// <param name="orientation">World-space cuboid orientation.</param>
        /// <returns>World-space collision vertex.</returns>
        static float3 TransformCollisionVertex(float3 localVertex, float3 position, float4 orientation) {
            System.Numerics.Vector3 source = new System.Numerics.Vector3(localVertex.X, localVertex.Y, localVertex.Z);
            System.Numerics.Quaternion rotation = new System.Numerics.Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
            System.Numerics.Vector3 rotated = System.Numerics.Vector3.Transform(source, rotation);
            return new float3(rotated.X + position.X, rotated.Y + position.Y, rotated.Z + position.Z);
        }

        /// <summary>
        /// Serializes one generated component through the same reflected editor payload path used by authored scene components.
        /// </summary>
        /// <param name="component">Component instance to serialize.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized scene component record.</returns>
        static SceneComponentAssetRecord CreateAutomaticComponentRecord(Component component, int componentIndex) {
            if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());
            return descriptor.SerializeComponent(component, componentIndex, null);
        }

        /// <summary>
        /// Appends the asset references stored on one component save state into the scene-level dependency list.
        /// </summary>
        /// <param name="saveState">Component save state that may contain asset references.</param>
        /// <param name="assetReferences">Scene-level dependency list being populated.</param>
        /// <param name="assetReferenceKeys">Deduplication keys for the dependency list.</param>
        void AppendAssetReferences(
            EntityComponentSaveState saveState,
            List<SceneAssetReference> assetReferences,
            HashSet<string> assetReferenceKeys) {
            if (saveState == null) {
                return;
            } else if (assetReferences == null) {
                throw new ArgumentNullException(nameof(assetReferences));
            } else if (assetReferenceKeys == null) {
                throw new ArgumentNullException(nameof(assetReferenceKeys));
            }

            foreach (SceneAssetReference reference in saveState.EnumerateAssetReferences()) {
                if (reference == null) {
                    continue;
                }

                string referenceKey = BuildAssetReferenceKey(reference);
                if (assetReferenceKeys.Add(referenceKey)) {
                    assetReferences.Add(reference);
                }
            }
        }

        /// <summary>
        /// Ensures one generated live editor subtree owns hidden save components and fresh scene entity ids before it is persisted through the standard authoring pipeline.
        /// </summary>
        /// <param name="entity">Generated live editor subtree root that should receive fresh scene entity ids.</param>
        void AssignFreshGeneratedEditorEntityIds(EditorEntity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            EntitySaveComponent saveComponent = EnsureEntitySaveComponent(entity);
            saveComponent.EntityId = AllocateSceneEntityId();
            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (entity.Children[childIndex] is not EditorEntity childEntity) {
                    continue;
                }

                AssignFreshGeneratedEditorEntityIds(childEntity);
            }
        }

        /// <summary>
        /// Resolves the hidden save component attached to one generated editor-authored entity.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(EditorEntity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    if (saveComponent.EntityId == 0u) {
                        throw new InvalidOperationException("Generated editor entities must have a preassigned numeric scene entity id.");
                    }

                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated editor entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Resolves the hidden save component attached to one live editor entity, creating it when a freshly generated subtree has not received one yet.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached save component.</returns>
        static EntitySaveComponent EnsureEntitySaveComponent(EditorEntity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            EntitySaveComponent createdSaveComponent = new EntitySaveComponent();
            entity.AddComponent(createdSaveComponent);
            return createdSaveComponent;
        }

        /// <summary>
        /// Builds the stable deduplication key used for one scene asset reference.
        /// </summary>
        /// <param name="reference">Scene asset reference being keyed.</param>
        /// <returns>Stable deduplication key.</returns>
        static string BuildAssetReferenceKey(SceneAssetReference reference) {
            if (reference == null) {
                throw new ArgumentNullException(nameof(reference));
            }

            return string.Concat(
                reference.SourceKind.ToString(),
                "|",
                reference.RelativePath ?? string.Empty,
                "|",
                reference.ProviderId ?? string.Empty,
                "|",
                reference.AssetId ?? string.Empty);
        }

        /// <summary>
        /// Resolves the editor font required by the shared playable physics showcase instruction overlay.
        /// </summary>
        /// <returns>Loaded editor font asset.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the physics showcase scenes can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Reassigns one generated editor-authored subtree into the current scene-local entity-id allocator so mixed manual and live-generated roots remain collision free.
        /// </summary>
        /// <param name="entity">Generated editor subtree root whose save-component ids should be reassigned.</param>
        void ReassignGeneratedEditorEntityIds(EditorEntity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.EntityId = AllocateSceneEntityId();
            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (entity.Children[childIndex] is not EditorEntity childEntity) {
                    continue;
                }

                ReassignGeneratedEditorEntityIds(childEntity);
            }
        }

        /// <summary>
        /// Converts the compact generated body-kind code into the runtime enum used by reflected persistence.
        /// </summary>
        /// <param name="bodyKindCode">Serialized generated body-kind code.</param>
        /// <returns>Runtime body-kind enum value.</returns>
        static BodyKind3D ResolveBodyKind(byte bodyKindCode) {
            if (bodyKindCode == StaticBodyKindCode) {
                return BodyKind3D.Static;
            }
            if (bodyKindCode == KinematicBodyKindCode) {
                return BodyKind3D.Kinematic;
            }
            if (bodyKindCode == DynamicBodyKindCode) {
                return BodyKind3D.Dynamic;
            }

            throw new ArgumentOutOfRangeException(nameof(bodyKindCode), "Unsupported generated rigid-body kind code.");
        }

        /// <summary>
        /// Creates one serialized kinematic-motion component record.
        /// </summary>
        /// <param name="startLocalPosition">Motion path start position.</param>
        /// <param name="endLocalPosition">Motion path end position.</param>
        /// <param name="travelDurationSeconds">One-way travel duration in seconds.</param>
        /// <param name="pingPong">True when the motion should reverse at the end.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized kinematic-motion component record.</returns>
        static SceneComponentAssetRecord CreateKinematicMotionComponentRecord(
            float3 startLocalPosition,
            float3 endLocalPosition,
            double travelDurationSeconds,
            bool pingPong,
            int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }
            if (double.IsNaN(travelDurationSeconds) || double.IsInfinity(travelDurationSeconds) || travelDurationSeconds <= 0d) {
                throw new ArgumentOutOfRangeException(nameof(travelDurationSeconds), "Travel duration must be a finite value greater than zero.");
            }

            using MemoryStream stream = new MemoryStream();
            using EngineBinaryWriter writer = EngineBinaryWriter.Create(stream, EngineBinaryEndianness.LittleEndian);
            writer.WriteByte(KinematicMotionComponentPayloadVersion);
            writer.WriteFloat3(startLocalPosition);
            writer.WriteFloat3(endLocalPosition);
            writer.WriteInt64(BitConverter.DoubleToInt64Bits(travelDurationSeconds));
            writer.WriteByte(pingPong ? (byte)1 : (byte)0);

            return new SceneComponentAssetRecord {
                ComponentTypeId = "helengine.KinematicMotion3DComponent",
                ComponentIndex = componentIndex,
                Payload = stream.ToArray()
            };
        }

        /// <summary>
        /// Creates one serialized character-controller component record.
        /// </summary>
        /// <param name="desiredMoveDirection">Desired planar move direction.</param>
        /// <param name="moveSpeed">Horizontal move speed in world units per second.</param>
        /// <param name="gravityScale">Gravity multiplier used by the controller.</param>
        /// <param name="stepHeight">Maximum upward snap height used while climbing support surfaces.</param>
        /// <param name="groundSnapDistance">Maximum downward snap distance used to keep the controller grounded.</param>
        /// <param name="componentIndex">Entity-local component order index.</param>
        /// <returns>Serialized character-controller component record.</returns>
        static SceneComponentAssetRecord CreateCharacterControllerComponentRecord(
            float3 desiredMoveDirection,
            double moveSpeed,
            double gravityScale,
            double stepHeight,
            double groundSnapDistance,
            int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }
            if (double.IsNaN(moveSpeed) || double.IsInfinity(moveSpeed) || moveSpeed < 0d) {
                throw new ArgumentOutOfRangeException(nameof(moveSpeed), "Move speed must be a finite value greater than or equal to zero.");
            }
            if (double.IsNaN(gravityScale) || double.IsInfinity(gravityScale) || gravityScale < 0d) {
                throw new ArgumentOutOfRangeException(nameof(gravityScale), "Gravity scale must be a finite value greater than or equal to zero.");
            }
            if (double.IsNaN(stepHeight) || double.IsInfinity(stepHeight) || stepHeight < 0d) {
                throw new ArgumentOutOfRangeException(nameof(stepHeight), "Step height must be a finite value greater than or equal to zero.");
            }
            if (double.IsNaN(groundSnapDistance) || double.IsInfinity(groundSnapDistance) || groundSnapDistance < 0d) {
                throw new ArgumentOutOfRangeException(nameof(groundSnapDistance), "Ground snap distance must be a finite value greater than or equal to zero.");
            }

            using MemoryStream stream = new MemoryStream();
            using EngineBinaryWriter writer = EngineBinaryWriter.Create(stream, EngineBinaryEndianness.LittleEndian);
            writer.WriteByte(CharacterControllerComponentPayloadVersion);
            writer.WriteFloat3(desiredMoveDirection);
            writer.WriteInt64(BitConverter.DoubleToInt64Bits(moveSpeed));
            writer.WriteInt64(BitConverter.DoubleToInt64Bits(gravityScale));
            writer.WriteInt64(BitConverter.DoubleToInt64Bits(stepHeight));
            writer.WriteInt64(BitConverter.DoubleToInt64Bits(groundSnapDistance));

            return new SceneComponentAssetRecord {
                ComponentTypeId = "helengine.CharacterController3DComponent",
                ComponentIndex = componentIndex,
                Payload = stream.ToArray()
            };
        }

        /// <summary>
        /// Writes one optional scene asset reference into a component payload.
        /// </summary>
        /// <param name="writer">Destination writer receiving the payload.</param>
        /// <param name="reference">Reference to serialize.</param>
        static void WriteOptionalReference(EngineBinaryWriter writer, SceneAssetReference reference) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }
            if (reference == null) {
                throw new ArgumentNullException(nameof(reference));
            }

            writer.WriteByte(1);
            writer.WriteInt32((int)reference.SourceKind);
            writer.WriteString(reference.RelativePath);
            writer.WriteString(reference.ProviderId);
            writer.WriteString(reference.AssetId);
        }

        /// <summary>
        /// Writes one `float4` payload into a binary component stream.
        /// </summary>
        /// <param name="writer">Destination writer receiving the payload.</param>
        /// <param name="value">Vector value to write.</param>
        static void WriteFloat4(EngineBinaryWriter writer, float4 value) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteSingle(value.X);
            writer.WriteSingle(value.Y);
            writer.WriteSingle(value.Z);
            writer.WriteSingle(value.W);
        }

        /// <summary>
        /// Creates one quaternion from yaw, pitch, and roll angles expressed in degrees.
        /// </summary>
        /// <param name="yawDegrees">Yaw around the Y axis in degrees.</param>
        /// <param name="pitchDegrees">Pitch around the X axis in degrees.</param>
        /// <param name="rollDegrees">Roll around the Z axis in degrees.</param>
        /// <returns>Converted quaternion.</returns>
        static float4 CreateYawPitchRollDegrees(double yawDegrees, double pitchDegrees, double rollDegrees) {
            float4.CreateFromYawPitchRoll(
                (float)(yawDegrees * Math.PI / 180.0),
                (float)(pitchDegrees * Math.PI / 180.0),
                (float)(rollDegrees * Math.PI / 180.0),
                out float4 result);
            return result;
        }

        /// <summary>
        /// Resolves the absolute output path for one relative physics validation scene id.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path.</param>
        /// <param name="sceneId">Relative scene id stored in the asset.</param>
        /// <returns>Absolute output file path.</returns>
        static string GetSceneFullPath(string projectRootPath, string sceneId) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            string relativePath = sceneId.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRootPath, "assets", relativePath);
        }

        /// <summary>
        /// Allocates the next scene-local entity id for the validation scene currently being built.
        /// </summary>
        /// <returns>Next non-zero scene-local entity id.</returns>
        uint AllocateSceneEntityId() {
            return SceneEntityIdAllocator.Allocate();
        }
    }
}
