using CityGeneratedMaterialAssetDefinition = city.rendering.tools.GeneratedMaterialAssetDefinition;
using CityGeneratedMaterialAssetWriteService = city.rendering.tools.GeneratedMaterialAssetWriteService;
using CityGeneratedMaterialPlatformDefinition = city.rendering.tools.GeneratedMaterialPlatformDefinition;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Generates the reusable Tilt Trial pendulum hammer support assets.
    /// </summary>
    public sealed class TiltTrialPendulumHammerAssetGenerator {
        /// <summary>
        /// Stable project-relative path for the shared pendulum hammer model.
        /// </summary>
        public const string PendulumHammerCommonModelRelativePath = "models/games/tilt/pendulum_hammer.hasset";

        /// <summary>
        /// Stable project-relative path for the low-poly DS pendulum hammer model.
        /// </summary>
        public const string PendulumHammerDsModelRelativePath = "models/games/tilt/pendulum_hammer_ds.hasset";

        /// <summary>
        /// Stable project-relative path for the wooden handle material.
        /// </summary>
        public const string PendulumHammerHandleMaterialRelativePath = "materials/games/tilt/PendulumHammerHandle.hasset";

        /// <summary>
        /// Stable project-relative path for the steel head material.
        /// </summary>
        public const string PendulumHammerHeadMaterialRelativePath = "materials/games/tilt/PendulumHammerHead.hasset";

        /// <summary>
        /// Stable project-relative path for the pendulum hammer Blueprint.
        /// </summary>
        public const string PendulumHammerBlueprintRelativePath = "blueprints/games/tilt/PendulumHammer.hblueprint";

        /// <summary>
        /// Stable asset id for the shared pendulum hammer model.
        /// </summary>
        public const string PendulumHammerCommonModelAssetId = "Models.games.tilt.pendulum_hammer";

        /// <summary>
        /// Stable asset id for the low-poly DS pendulum hammer model.
        /// </summary>
        public const string PendulumHammerDsModelAssetId = "Models.games.tilt.pendulum_hammer_ds";

        /// <summary>
        /// Stable asset id for the wooden handle material.
        /// </summary>
        public const string PendulumHammerHandleMaterialAssetId = "Materials.games.tilt.PendulumHammerHandle";

        /// <summary>
        /// Stable asset id for the steel head material.
        /// </summary>
        public const string PendulumHammerHeadMaterialAssetId = "Materials.games.tilt.PendulumHammerHead";

        const string WindowsMaterialSchemaId = "standard-shader";
        const string Ps2MaterialSchemaId = "ps2-simple-lit";
        const string GameCubeMaterialSchemaId = "gamecube-standard";
        const string DsMaterialSchemaId = "ds-standard-lit";
        const string StandardShaderAssetId = "ForwardStandardShader";

        const string UseCustomShaderFieldId = "use-custom-shader";
        const string ShaderAssetIdFieldId = "shader-asset-id";
        const string RoughnessFieldId = "roughness";
        const string MetallicFieldId = "metallic";
        const string SpecularFieldId = "specular";
        const string BaseColorFieldId = "base-color";
        const string CastsShadowFieldId = "casts-shadow";
        const string ReceivesShadowFieldId = "receives-shadow";
        const string AlphaModeFieldId = "alpha-mode";
        const string DoubleSidedFieldId = "double-sided";
        const string VertexColorModeFieldId = "vertex-color-mode";
        const string LightingModeFieldId = "lighting-mode";
        const string Ps2CastShadowsFieldId = "cast-shadows";

        const float HandleHalfWidth = 0.09f;
        const float HandleTopY = 0.05f;
        const float HandleBottomY = -2.05f;
        const float HeadCenterY = -2.2f;
        const float HeadRadius = 0.55f;
        const float HeadHalfLength = 0.6f;

        static readonly AutomaticScriptComponentPersistenceDescriptor AutomaticDescriptor =
            new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());

        readonly SplitPlayGeneratedModelAssetWriteService ModelWriteService;
        readonly SplitPlayGeneratedBlueprintAssetWriteService BlueprintWriteService;
        readonly CityGeneratedMaterialAssetWriteService MaterialWriteService;

        public TiltTrialPendulumHammerAssetGenerator() {
            ModelWriteService = new SplitPlayGeneratedModelAssetWriteService();
            BlueprintWriteService = new SplitPlayGeneratedBlueprintAssetWriteService();
            MaterialWriteService = new CityGeneratedMaterialAssetWriteService();
        }

        /// <summary>
        /// Generates the pendulum hammer models, materials, and Blueprint beneath the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets folder.</param>
        public void Generate(string projectRootPath) {
            ModelWriteService.WriteModel(
                projectRootPath,
                PendulumHammerCommonModelRelativePath,
                CreatePendulumHammerModel(PendulumHammerCommonModelAssetId, 18));
            ModelWriteService.WriteModel(
                projectRootPath,
                PendulumHammerDsModelRelativePath,
                CreatePendulumHammerModel(PendulumHammerDsModelAssetId, 8));
            MaterialWriteService.WriteMaterial(
                projectRootPath,
                PendulumHammerHandleMaterialRelativePath,
                CreateMaterialDefinition(PendulumHammerHandleMaterialAssetId, "#8A5A2BFF", 0.78f, 0.02f, false));
            MaterialWriteService.WriteMaterial(
                projectRootPath,
                PendulumHammerHeadMaterialRelativePath,
                CreateMaterialDefinition(PendulumHammerHeadMaterialAssetId, "#B7BDC9FF", 0.35f, 0.85f, false));
            BlueprintWriteService.WriteBlueprint(
                projectRootPath,
                PendulumHammerBlueprintRelativePath,
                CreateBlueprintAsset());
        }

        BlueprintAsset CreateBlueprintAsset() {
            SceneAssetReference commonModelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(PendulumHammerCommonModelRelativePath);
            SceneAssetReference dsModelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(PendulumHammerDsModelRelativePath);
            SceneAssetReference handleMaterialReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(PendulumHammerHandleMaterialRelativePath);
            SceneAssetReference headMaterialReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(PendulumHammerHeadMaterialRelativePath);

            MeshComponent meshComponent = new MeshComponent();
            meshComponent.Materials = new RuntimeMaterial[] { null, null };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference("Model", commonModelReference);
            saveState.SetAssetReference("Materials[0]", handleMaterialReference);
            saveState.SetAssetReference("Materials[1]", headMaterialReference);

            EntityComponentPlatformOverrideState dsOverride = saveState.GetOrCreatePlatformOverride("ds");
            dsOverride.Payload = Array.Empty<byte>();
            dsOverride.SetAssetReference("Model", dsModelReference);

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneComponentAssetRecord baseRecord = registry.GetDescriptor(meshComponent).SerializeComponent(meshComponent, 0, saveState);
            SceneComponentAssetRecord meshRecord = new ComponentPlatformOverridePayloadService().Wrap(baseRecord, saveState);
            SceneComponentAssetRecord swingRecord = AutomaticDescriptor.SerializeComponent(new city.game.TiltTrialPendulumHammerComponent(), 1, null);

            RigidBody3DComponent headRigidBody = new RigidBody3DComponent {
                BodyKind = BodyKind3D.Kinematic,
                UseGravity = false,
                Mass = 1d
            };
            BoxCollider3DComponent headCollider = new BoxCollider3DComponent {
                Size = new float3(HeadRadius * 2f, HeadRadius * 2f, HeadHalfLength * 2f)
            };
            SceneComponentAssetRecord headRigidBodyRecord = registry.GetDescriptor(headRigidBody).SerializeComponent(headRigidBody, 0, null);
            SceneComponentAssetRecord headColliderRecord = registry.GetDescriptor(headCollider).SerializeComponent(headCollider, 1, null);

            return new BlueprintAsset {
                Id = PendulumHammerBlueprintRelativePath,
                RootEntity = new SceneEntityAsset {
                    Id = 1u,
                    Name = "PendulumHammer",
                    Enabled = true,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    LocalPosition = float3.Zero,
                    LocalScale = float3.One,
                    LocalOrientation = float4.Identity,
                    Components = [meshRecord, swingRecord],
                    Children = [
                        new SceneEntityAsset {
                            Id = 2u,
                            Name = "HammerHead",
                            Enabled = true,
                            LayerMask = EditorLayerMasks.SceneObjects,
                            LocalPosition = new float3(0f, HeadCenterY, 0f),
                            LocalScale = float3.One,
                            LocalOrientation = float4.Identity,
                            Components = [headRigidBodyRecord, headColliderRecord],
                            Children = Array.Empty<SceneEntityAsset>()
                        }
                    ]
                },
                AssetReferences = [commonModelReference, dsModelReference, handleMaterialReference, headMaterialReference]
            };
        }

        CityGeneratedMaterialAssetDefinition CreateMaterialDefinition(string assetId, string baseColorHex, float roughness, float metallic, bool doubleSided) {
            CityGeneratedMaterialAssetDefinition definition = new CityGeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = assetId,
                RenderState = new MaterialRenderState {
                    CullMode = doubleSided ? MaterialCullMode.None : MaterialCullMode.Back
                },
                CastsShadows = true,
                ReceivesShadows = true
            };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"), baseColorHex, roughness, metallic, doubleSided);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"), baseColorHex, doubleSided);
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), baseColorHex, roughness, metallic, doubleSided);
            ConfigureGameCubePlatform(definition.GetOrCreatePlatform("gamecube"), baseColorHex, doubleSided);
            ConfigureDsPlatform(definition.GetOrCreatePlatform("ds"), baseColorHex, doubleSided);
            return definition;
        }

        void ConfigureWindowsPlatform(CityGeneratedMaterialPlatformDefinition platformDefinition, string baseColorHex, float roughness, float metallic, bool doubleSided) {
            platformDefinition.SchemaId = WindowsMaterialSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(RoughnessFieldId, roughness.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            platformDefinition.SetFieldValue(MetallicFieldId, metallic.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            platformDefinition.SetFieldValue(SpecularFieldId, "0.5");
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, doubleSided ? "true" : "false");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
            platformDefinition.SetFieldValue(BaseColorFieldId, baseColorHex);
        }

        void ConfigurePs2Platform(CityGeneratedMaterialPlatformDefinition platformDefinition, string baseColorHex, bool doubleSided) {
            platformDefinition.SchemaId = Ps2MaterialSchemaId;
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, doubleSided ? "true" : "false");
            platformDefinition.SetFieldValue(Ps2CastShadowsFieldId, "true");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, baseColorHex);
        }

        void ConfigureGameCubePlatform(CityGeneratedMaterialPlatformDefinition platformDefinition, string baseColorHex, bool doubleSided) {
            platformDefinition.SchemaId = GameCubeMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, doubleSided ? "true" : "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, baseColorHex);
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        void ConfigureDsPlatform(CityGeneratedMaterialPlatformDefinition platformDefinition, string baseColorHex, bool doubleSided) {
            platformDefinition.SchemaId = DsMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, doubleSided ? "true" : "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, baseColorHex);
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        ModelAsset CreatePendulumHammerModel(string assetId, int headRadialSteps) {
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            } else if (headRadialSteps < 3) {
                throw new ArgumentOutOfRangeException(nameof(headRadialSteps), "Pendulum hammer generation requires at least three radial head steps.");
            }

            List<float3> positions = new List<float3>();
            List<float3> normals = new List<float3>();
            List<float2> texCoords = new List<float2>();
            List<ushort> indices = new List<ushort>();
            List<ModelSubmeshAsset> submeshes = new List<ModelSubmeshAsset>();

            int handleIndexStart = indices.Count;
            ModelAsset handleBox = GeneratedPrimitiveMeshFactory.CreateBox(HandleHalfWidth * 2f, HandleTopY - HandleBottomY, HandleHalfWidth * 2f);
            GeneratedPrimitiveMeshFactory.Append(
                positions,
                normals,
                texCoords,
                indices,
                handleBox,
                position => new float3(position.X, position.Y + HandleBottomY, position.Z),
                normal => normal);
            submeshes.Add(new ModelSubmeshAsset {
                MaterialSlotName = "HandleMaterial",
                IndexStart = handleIndexStart,
                IndexCount = indices.Count - handleIndexStart
            });

            int headIndexStart = indices.Count;
            ModelAsset headCylinder = GeneratedPrimitiveMeshFactory.CreateSingleSidedCylinderY(HeadRadius, HeadHalfLength * 2f, headRadialSteps);
            // Rotates the engine's +Y cylinder axis onto +Z and centers it on the head pivot.
            GeneratedPrimitiveMeshFactory.Append(
                positions,
                normals,
                texCoords,
                indices,
                headCylinder,
                position => new float3(position.X, HeadCenterY - position.Z, position.Y - HeadHalfLength),
                normal => new float3(normal.X, -normal.Z, normal.Y));
            submeshes.Add(new ModelSubmeshAsset {
                MaterialSlotName = "HeadMaterial",
                IndexStart = headIndexStart,
                IndexCount = indices.Count - headIndexStart
            });

            return new ModelAsset {
                Id = assetId,
                Positions = positions.ToArray(),
                Normals = normals.ToArray(),
                TexCoords = texCoords.ToArray(),
                Indices16 = indices.ToArray(),
                BoundsMin = new float3(-HeadRadius, HeadCenterY - HeadRadius, -HeadHalfLength),
                BoundsMax = new float3(HeadRadius, HandleTopY, HeadHalfLength),
                Submeshes = submeshes.ToArray()
            };
        }
    }
}
