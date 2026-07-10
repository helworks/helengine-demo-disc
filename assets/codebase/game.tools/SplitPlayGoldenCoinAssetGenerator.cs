using CityGeneratedMaterialAssetDefinition = city.rendering.tools.GeneratedMaterialAssetDefinition;
using CityGeneratedMaterialAssetWriteService = city.rendering.tools.GeneratedMaterialAssetWriteService;
using CityGeneratedMaterialPlatformDefinition = city.rendering.tools.GeneratedMaterialPlatformDefinition;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Generates the reusable Split Play coin support assets.
    /// </summary>
    public sealed class SplitPlayGoldenCoinAssetGenerator {
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
        static readonly AutomaticScriptComponentPersistenceDescriptor AutomaticDescriptor =
            new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());

        readonly SplitPlayGeneratedModelAssetWriteService ModelWriteService;
        readonly SplitPlayGeneratedBlueprintAssetWriteService BlueprintWriteService;
        readonly CityGeneratedMaterialAssetWriteService MaterialWriteService;

        public SplitPlayGoldenCoinAssetGenerator() {
            ModelWriteService = new SplitPlayGeneratedModelAssetWriteService();
            BlueprintWriteService = new SplitPlayGeneratedBlueprintAssetWriteService();
            MaterialWriteService = new CityGeneratedMaterialAssetWriteService();
        }

        public void Generate(string projectRootPath) {
            ModelWriteService.WriteModel(
                projectRootPath,
                SplitPlayAssetCatalog.GoldenCoinCommonModelRelativePath,
                CreateCylinderModel(SplitPlayAssetCatalog.GoldenCoinCommonModelAssetId, 20));
            ModelWriteService.WriteModel(
                projectRootPath,
                SplitPlayAssetCatalog.GoldenCoinDsModelRelativePath,
                CreateCylinderModel(SplitPlayAssetCatalog.GoldenCoinDsModelAssetId, 10));
            MaterialWriteService.WriteMaterial(
                projectRootPath,
                SplitPlayAssetCatalog.GoldenCoinMaterialRelativePath,
                CreateMaterialDefinition());
            BlueprintWriteService.WriteBlueprint(
                projectRootPath,
                SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath,
                CreateBlueprintAsset());
        }

        BlueprintAsset CreateBlueprintAsset() {
            SceneAssetReference commonModelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(SplitPlayAssetCatalog.GoldenCoinCommonModelRelativePath);
            SceneAssetReference dsModelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(SplitPlayAssetCatalog.GoldenCoinDsModelRelativePath);
            SceneAssetReference materialReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(SplitPlayAssetCatalog.GoldenCoinMaterialRelativePath);

            MeshComponent meshComponent = new MeshComponent();
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference("Model", commonModelReference);
            saveState.SetAssetReference("Materials[0]", materialReference);

            EntityComponentPlatformOverrideState dsOverride = saveState.GetOrCreatePlatformOverride("ds");
            dsOverride.Payload = Array.Empty<byte>();
            dsOverride.SetAssetReference("Model", dsModelReference);

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneComponentAssetRecord baseRecord = registry.GetDescriptor(meshComponent).SerializeComponent(meshComponent, 0, saveState);
            SceneComponentAssetRecord meshRecord = new ComponentPlatformOverridePayloadService().Wrap(baseRecord, saveState);
            SceneComponentAssetRecord idleMotionRecord = AutomaticDescriptor.SerializeComponent(new city.game.SplitPlayIdleMotionComponent(), 1, null);

            return new BlueprintAsset {
                Id = SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath,
                RootEntity = new SceneEntityAsset {
                    Id = 1u,
                    Name = "GoldenCoin",
                    Enabled = true,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    LocalPosition = float3.Zero,
                    LocalScale = float3.One,
                    LocalOrientation = float4.Identity,
                    Components = [meshRecord, idleMotionRecord],
                    Children = Array.Empty<SceneEntityAsset>()
                },
                AssetReferences = [commonModelReference, dsModelReference, materialReference]
            };
        }

        CityGeneratedMaterialAssetDefinition CreateMaterialDefinition() {
            CityGeneratedMaterialAssetDefinition definition = new CityGeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = SplitPlayAssetCatalog.GoldenCoinMaterialAssetId,
                RenderState = new MaterialRenderState {
                    CullMode = MaterialCullMode.None
                },
                CastsShadows = true,
                ReceivesShadows = true
            };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"), 0.38f);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"));
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), 0.38f);
            ConfigureGameCubePlatform(definition.GetOrCreatePlatform("gamecube"));
            ConfigureDsPlatform(definition.GetOrCreatePlatform("ds"));
            return definition;
        }

        void ConfigureWindowsPlatform(CityGeneratedMaterialPlatformDefinition platformDefinition, float roughness) {
            platformDefinition.SchemaId = WindowsMaterialSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(RoughnessFieldId, roughness.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            platformDefinition.SetFieldValue(MetallicFieldId, "0.75");
            platformDefinition.SetFieldValue(SpecularFieldId, "0.5");
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "true");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFF0C62E");
        }

        void ConfigurePs2Platform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = Ps2MaterialSchemaId;
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(Ps2CastShadowsFieldId, "true");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFF0C62E");
        }

        void ConfigureGameCubePlatform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = GameCubeMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFF0C62E");
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        void ConfigureDsPlatform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = DsMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFF0C62E");
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        ModelAsset CreateCylinderModel(string assetId, int radialSteps) {
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            } else if (radialSteps < 3) {
                throw new ArgumentOutOfRangeException(nameof(radialSteps), "Cylinder generation requires at least three radial steps.");
            }

            const float radius = 0.5f;
            const float halfDepth = 0.08f;

            List<float3> positions = new List<float3>();
            List<float3> normals = new List<float3>();
            List<float2> texCoords = new List<float2>();
            List<ushort> indices = new List<ushort>();

            AppendCap(+halfDepth, +1f);
            AppendCap(-halfDepth, -1f);
            AppendSideBand();

            return new ModelAsset {
                Id = assetId,
                Positions = positions.ToArray(),
                Normals = normals.ToArray(),
                TexCoords = texCoords.ToArray(),
                Indices16 = indices.ToArray(),
                BoundsMin = new float3(-radius, -radius, -halfDepth),
                BoundsMax = new float3(radius, radius, halfDepth),
                Submeshes = [
                    new ModelSubmeshAsset {
                        MaterialSlotName = "DefaultMaterial",
                        IndexStart = 0,
                        IndexCount = indices.Count
                    }
                ]
            };

            void AppendCap(float z, float normalZ) {
                ushort centerIndex = (ushort)positions.Count;
                positions.Add(new float3(0f, 0f, z));
                normals.Add(new float3(0f, 0f, normalZ));
                texCoords.Add(new float2(0.5f, 0.5f));

                for (int step = 0; step < radialSteps; step++) {
                    float angle = (MathF.PI * 2f * step) / radialSteps;
                    float x = MathF.Cos(angle) * radius;
                    float y = MathF.Sin(angle) * radius;
                    positions.Add(new float3(x, y, z));
                    normals.Add(new float3(0f, 0f, normalZ));
                    texCoords.Add(new float2((x / radius + 1f) * 0.5f, (y / radius + 1f) * 0.5f));
                }

                for (int step = 0; step < radialSteps; step++) {
                    ushort ringA = (ushort)(centerIndex + 1 + step);
                    ushort ringB = (ushort)(centerIndex + 1 + ((step + 1) % radialSteps));
                    if (normalZ > 0f) {
                        indices.Add(centerIndex);
                        indices.Add(ringA);
                        indices.Add(ringB);
                    } else {
                        indices.Add(centerIndex);
                        indices.Add(ringB);
                        indices.Add(ringA);
                    }
                }
            }

            void AppendSideBand() {
                ushort sideStartIndex = (ushort)positions.Count;
                for (int step = 0; step < radialSteps; step++) {
                    float angle = (MathF.PI * 2f * step) / radialSteps;
                    float x = MathF.Cos(angle) * radius;
                    float y = MathF.Sin(angle) * radius;
                    float3 normal = new float3(MathF.Cos(angle), MathF.Sin(angle), 0f);
                    float u = (float)step / radialSteps;

                    positions.Add(new float3(x, y, halfDepth));
                    normals.Add(normal);
                    texCoords.Add(new float2(u, 0f));

                    positions.Add(new float3(x, y, -halfDepth));
                    normals.Add(normal);
                    texCoords.Add(new float2(u, 1f));
                }

                for (int step = 0; step < radialSteps; step++) {
                    int nextStep = (step + 1) % radialSteps;
                    ushort topA = (ushort)(sideStartIndex + step * 2);
                    ushort bottomA = (ushort)(sideStartIndex + step * 2 + 1);
                    ushort topB = (ushort)(sideStartIndex + nextStep * 2);
                    ushort bottomB = (ushort)(sideStartIndex + nextStep * 2 + 1);

                    indices.Add(topA);
                    indices.Add(bottomA);
                    indices.Add(topB);

                    indices.Add(topB);
                    indices.Add(bottomA);
                    indices.Add(bottomB);
                }
            }
        }
    }
}
