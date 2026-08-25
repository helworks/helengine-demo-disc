using CityGeneratedMaterialAssetDefinition = city.rendering.tools.GeneratedMaterialAssetDefinition;
using CityGeneratedMaterialAssetWriteService = city.rendering.tools.GeneratedMaterialAssetWriteService;
using CityGeneratedMaterialPlatformDefinition = city.rendering.tools.GeneratedMaterialPlatformDefinition;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Generates the reusable Tilt Trial rotating platform support assets.
    /// </summary>
    public sealed class TiltTrialRotatingPlatformAssetGenerator {
        /// <summary>
        /// Stable project-relative path for the shared rotating platform model.
        /// </summary>
        public const string RotatingPlatformModelRelativePath = "models/games/tilt/rotating_platform.hasset";

        /// <summary>
        /// Stable project-relative path for the platform deck material.
        /// </summary>
        public const string RotatingPlatformMaterialRelativePath = "materials/games/tilt/RotatingPlatform.hasset";

        /// <summary>
        /// Stable project-relative path for the rotating platform Blueprint.
        /// </summary>
        public const string RotatingPlatformBlueprintRelativePath = "blueprints/games/tilt/RotatingPlatform.hblueprint";

        /// <summary>
        /// Stable asset id for the shared rotating platform model.
        /// </summary>
        public const string RotatingPlatformModelAssetId = "Models.games.tilt.rotating_platform";

        /// <summary>
        /// Stable asset id for the platform deck material.
        /// </summary>
        public const string RotatingPlatformMaterialAssetId = "Materials.games.tilt.RotatingPlatform";

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

        const float PlatformHalfWidth = 2.0f;
        const float PlatformHalfHeight = 0.15f;
        const float PlatformHalfDepth = 0.8f;

        static readonly AutomaticScriptComponentPersistenceDescriptor AutomaticDescriptor =
            new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());

        readonly SplitPlayGeneratedModelAssetWriteService ModelWriteService;
        readonly SplitPlayGeneratedBlueprintAssetWriteService BlueprintWriteService;
        readonly CityGeneratedMaterialAssetWriteService MaterialWriteService;

        public TiltTrialRotatingPlatformAssetGenerator() {
            ModelWriteService = new SplitPlayGeneratedModelAssetWriteService();
            BlueprintWriteService = new SplitPlayGeneratedBlueprintAssetWriteService();
            MaterialWriteService = new CityGeneratedMaterialAssetWriteService();
        }

        /// <summary>
        /// Generates the rotating platform model, material, and Blueprint beneath the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets folder.</param>
        public void Generate(string projectRootPath) {
            ModelWriteService.WriteModel(
                projectRootPath,
                RotatingPlatformModelRelativePath,
                CreateRotatingPlatformModel(RotatingPlatformModelAssetId));
            MaterialWriteService.WriteMaterial(
                projectRootPath,
                RotatingPlatformMaterialRelativePath,
                CreateMaterialDefinition(RotatingPlatformMaterialAssetId, "#5C8DC9FF", 0.55f, 0.05f, false));
            BlueprintWriteService.WriteBlueprint(
                projectRootPath,
                RotatingPlatformBlueprintRelativePath,
                CreateBlueprintAsset(projectRootPath));
        }

        BlueprintAsset CreateBlueprintAsset(string projectRootPath) {
            SceneAssetReference modelReference = global::helengine.editor.EditorAssetReferenceFactory.CreateFileReference(projectRootPath, RotatingPlatformModelRelativePath, AssetEntryKind.Model);
            SceneAssetReference materialReference = global::helengine.editor.EditorAssetReferenceFactory.CreateFileReference(projectRootPath, RotatingPlatformMaterialRelativePath, AssetEntryKind.Material);

            MeshComponent meshComponent = new MeshComponent();
            meshComponent.Materials = new RuntimeMaterial[] { null };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference("Model", modelReference);
            saveState.SetAssetReference("Materials[0]", materialReference);

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneComponentAssetRecord baseRecord = registry.GetDescriptor(meshComponent).SerializeComponent(meshComponent, 0, saveState);
            SceneComponentAssetRecord meshRecord = new ComponentPlatformOverridePayloadService().Wrap(baseRecord, saveState);

            RigidBody3DComponent platformRigidBody = new RigidBody3DComponent {
                BodyKind = BodyKind3D.Kinematic,
                UseGravity = false,
                Mass = 1d
            };
            BoxCollider3DComponent platformCollider = new BoxCollider3DComponent {
                Size = new float3(PlatformHalfWidth * 2f, PlatformHalfHeight * 2f, PlatformHalfDepth * 2f)
            };
            SceneComponentAssetRecord rigidBodyRecord = registry.GetDescriptor(platformRigidBody).SerializeComponent(platformRigidBody, 1, null);
            SceneComponentAssetRecord colliderRecord = registry.GetDescriptor(platformCollider).SerializeComponent(platformCollider, 2, null);
            SceneComponentAssetRecord spinRecord = AutomaticDescriptor.SerializeComponent(new city.game.TiltTrialRotatingPlatformComponent(), 3, null);

            return new BlueprintAsset {
                Id = RotatingPlatformBlueprintRelativePath,
                RootEntity = new SceneEntityAsset {
                    Id = 1u,
                    Name = "RotatingPlatform",
                    Enabled = true,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    LocalPosition = float3.Zero,
                    LocalScale = float3.One,
                    LocalOrientation = float4.Identity,
                    Components = [meshRecord, rigidBodyRecord, colliderRecord, spinRecord],
                    Children = Array.Empty<SceneEntityAsset>()
                },
                AssetReferences = [modelReference, materialReference]
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

        ModelAsset CreateRotatingPlatformModel(string assetId) {
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            }

            List<float3> positions = new List<float3>();
            List<float3> normals = new List<float3>();
            List<float2> texCoords = new List<float2>();
            List<ushort> indices = new List<ushort>();

            ModelAsset deckBox = GeneratedPrimitiveMeshFactory.CreateBox(PlatformHalfWidth * 2f, PlatformHalfHeight * 2f, PlatformHalfDepth * 2f);
            GeneratedPrimitiveMeshFactory.Append(
                positions,
                normals,
                texCoords,
                indices,
                deckBox,
                position => new float3(position.X, position.Y - PlatformHalfHeight, position.Z),
                normal => normal);

            return new ModelAsset {
                Id = assetId,
                Positions = positions.ToArray(),
                Normals = normals.ToArray(),
                TexCoords = texCoords.ToArray(),
                Indices16 = indices.ToArray(),
                BoundsMin = new float3(-PlatformHalfWidth, -PlatformHalfHeight, -PlatformHalfDepth),
                BoundsMax = new float3(PlatformHalfWidth, PlatformHalfHeight, PlatformHalfDepth),
                Submeshes = [
                    new ModelSubmeshAsset {
                        MaterialSlotName = "PlatformMaterial",
                        IndexStart = 0,
                        IndexCount = indices.Count
                    }
                ]
            };
        }
    }
}
