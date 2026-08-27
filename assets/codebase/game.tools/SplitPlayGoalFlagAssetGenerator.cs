using CityGeneratedMaterialAssetDefinition = city.rendering.tools.GeneratedMaterialAssetDefinition;
using CityGeneratedMaterialAssetWriteService = city.rendering.tools.GeneratedMaterialAssetWriteService;
using CityGeneratedMaterialPlatformDefinition = city.rendering.tools.GeneratedMaterialPlatformDefinition;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Generates the reusable Split Play goal-flag support assets.
    /// </summary>
    public sealed class SplitPlayGoalFlagAssetGenerator {
        const string WindowsMaterialSchemaId = "standard-shader";
        const string Ps2MaterialSchemaId = "ps2-simple-lit";
        const string GameCubeMaterialSchemaId = "gamecube-standard";
        const string DsMaterialSchemaId = "ds-standard-lit";
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable component key for the blueprint goal component so scene overrides survive regeneration.
        /// </summary>
        public const string GoalComponentKey = "5a1f0c9e8d474b6c8f2a3b4c5d6e7f01";

        /// <summary>
        /// Stable component key for the blueprint trigger observer so scene entity-reference overrides survive regeneration.
        /// </summary>
        public const string TriggerObserverComponentKey = "9c8b7a6d5e4f43210fedcba987654321";

        /// <summary>
        /// Stable component key for the blueprint kinematic rigid body so scene overrides survive regeneration.
        /// </summary>
        public const string RigidBodyComponentKey = "3e2d1c0b4a5968778695a4b3c2d1e0f4";

        /// <summary>
        /// Stable component key for the blueprint trigger box collider so scene overrides survive regeneration.
        /// </summary>
        public const string BoxColliderComponentKey = "7f6e5d4c3b2a19080706050403020100";

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

        readonly SplitPlayGeneratedModelAssetWriteService ModelWriteService;
        readonly SplitPlayGeneratedBlueprintAssetWriteService BlueprintWriteService;
        readonly CityGeneratedMaterialAssetWriteService MaterialWriteService;
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        public SplitPlayGoalFlagAssetGenerator(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            ModelWriteService = new SplitPlayGeneratedModelAssetWriteService(AssetAuthoringService);
            BlueprintWriteService = new SplitPlayGeneratedBlueprintAssetWriteService(AssetAuthoringService);
            MaterialWriteService = new CityGeneratedMaterialAssetWriteService(AssetAuthoringService);
        }

        public void Generate(string projectRootPath) {
            ModelWriteService.WriteModel(
                SplitPlayAssetCatalog.GoalFlagCommonModelRelativePath,
                CreateGoalFlagModel(SplitPlayAssetCatalog.GoalFlagCommonModelAssetId, 16));
            ModelWriteService.WriteModel(
                SplitPlayAssetCatalog.GoalFlagDsModelRelativePath,
                CreateGoalFlagModel(SplitPlayAssetCatalog.GoalFlagDsModelAssetId, 6));
            MaterialWriteService.WriteMaterial(
                SplitPlayAssetCatalog.GoalFlagPoleMaterialRelativePath,
                CreateMaterialDefinition(SplitPlayAssetCatalog.GoalFlagPoleMaterialAssetId, "#FFE5E7EB", 0.42f, 0.60f, false));
            MaterialWriteService.WriteMaterial(
                SplitPlayAssetCatalog.GoalFlagBannerMaterialRelativePath,
                CreateMaterialDefinition(SplitPlayAssetCatalog.GoalFlagBannerMaterialAssetId, "#FFFF5A5A", 0.62f, 0.05f, false));
            BlueprintWriteService.WriteBlueprint(
                SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath,
                CreateBlueprintAsset(projectRootPath));
        }

        BlueprintAsset CreateBlueprintAsset(string projectRootPath) {
            SceneAssetReference commonModelReference = AssetAuthoringService.CreateFileReference(SplitPlayAssetCatalog.GoalFlagCommonModelRelativePath, AssetEntryKind.Model);
            SceneAssetReference dsModelReference = AssetAuthoringService.CreateFileReference(SplitPlayAssetCatalog.GoalFlagDsModelRelativePath, AssetEntryKind.Model);
            SceneAssetReference poleMaterialReference = AssetAuthoringService.CreateFileReference(SplitPlayAssetCatalog.GoalFlagPoleMaterialRelativePath, AssetEntryKind.Material);
            SceneAssetReference bannerMaterialReference = AssetAuthoringService.CreateFileReference(SplitPlayAssetCatalog.GoalFlagBannerMaterialRelativePath, AssetEntryKind.Material);

            MeshComponent meshComponent = new MeshComponent();
            meshComponent.Materials = new RuntimeMaterial[] { null, null };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference("Model", commonModelReference);
            saveState.SetAssetReference("Materials[0]", poleMaterialReference);
            saveState.SetAssetReference("Materials[1]", bannerMaterialReference);

            EntityComponentPlatformOverrideState dsOverride = saveState.GetOrCreatePlatformOverride("ds");
            dsOverride.Payload = Array.Empty<byte>();
            dsOverride.SetAssetReference("Model", dsModelReference);

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneComponentAssetRecord baseRecord = registry.GetDescriptor(meshComponent).SerializeComponent(meshComponent, 0, saveState);
            SceneComponentAssetRecord meshRecord = new ComponentPlatformOverridePayloadService().Wrap(baseRecord, saveState);

            SceneComponentAssetRecord goalRecord = SerializeWithStableKey(registry, new global::city.game.TiltTrialGoalComponent(), 1, GoalComponentKey);
            SceneComponentAssetRecord observerRecord = SerializeWithStableKey(registry, new global::helengine.SceneEntityTriggerObserverComponent(), 2, TriggerObserverComponentKey);
            SceneComponentAssetRecord rigidBodyRecord = SerializeWithStableKey(registry, new RigidBody3DComponent {
                BodyKind = BodyKind3D.Kinematic,
                UseGravity = false,
                Mass = 1d
            }, 3, RigidBodyComponentKey);
            BoxCollider3DComponent triggerCollider = new BoxCollider3DComponent {
                Size = new float3(1f, 2f, 1f)
            };
            triggerCollider.IsTrigger = true;
            SceneComponentAssetRecord colliderRecord = SerializeWithStableKey(registry, triggerCollider, 4, BoxColliderComponentKey);

            return new BlueprintAsset {
                Id = SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath,
                RootEntity = new SceneEntityAsset {
                    Id = 1u,
                    Name = "GoalFlag",
                    Enabled = true,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    LocalPosition = float3.Zero,
                    LocalScale = float3.One,
                    LocalOrientation = float4.Identity,
                    Components = [meshRecord, goalRecord, observerRecord, rigidBodyRecord, colliderRecord],
                    Children = Array.Empty<SceneEntityAsset>()
                },
                AssetReferences = [commonModelReference, dsModelReference, poleMaterialReference, bannerMaterialReference]
            };
        }

        /// <summary>
        /// Serializes one blueprint component with a stable component key so scene-owned overrides survive regeneration.
        /// </summary>
        /// <param name="registry">Persistence registry used for serialization.</param>
        /// <param name="component">Component instance to serialize.</param>
        /// <param name="componentIndex">Zero-based component slot on the blueprint root.</param>
        /// <param name="componentKey">Stable component key persisted with the record.</param>
        /// <returns>Serialized component record carrying the stable key.</returns>
        static SceneComponentAssetRecord SerializeWithStableKey(ComponentPersistenceRegistry registry, Component component, int componentIndex, string componentKey) {
            SceneComponentAssetRecord record = registry.GetDescriptor(component).SerializeComponent(component, componentIndex, null);
            return new SceneComponentAssetRecord {
                ComponentIndex = record.ComponentIndex,
                ComponentKey = componentKey,
                ComponentTypeId = record.ComponentTypeId,
                Payload = record.Payload
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

        ModelAsset CreateGoalFlagModel(string assetId, int poleRadialSteps) {
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            } else if (poleRadialSteps < 3) {
                throw new ArgumentOutOfRangeException(nameof(poleRadialSteps), "Goal flag generation requires at least three radial pole steps.");
            }

            const float poleRadius = 0.05f;
            const float poleHeight = 2.0f;
            const float flagThickness = 0.03f;

            List<float3> positions = new List<float3>();
            List<float3> normals = new List<float3>();
            List<float2> texCoords = new List<float2>();
            List<ushort> indices = new List<ushort>();
            List<ModelSubmeshAsset> submeshes = new List<ModelSubmeshAsset>();

            int poleIndexStart = indices.Count;
            AppendCylinderY(new float3(0f, 0f, 0f), poleRadius, poleHeight, poleRadialSteps);
            submeshes.Add(new ModelSubmeshAsset {
                MaterialSlotName = "PoleMaterial",
                IndexStart = poleIndexStart,
                IndexCount = indices.Count - poleIndexStart
            });

            int flagIndexStart = indices.Count;
            AppendPennant(new float3(0f, 1.96f, 0f), 0.72f, 0.38f, flagThickness * 0.5f);
            submeshes.Add(new ModelSubmeshAsset {
                MaterialSlotName = "BannerMaterial",
                IndexStart = flagIndexStart,
                IndexCount = indices.Count - flagIndexStart
            });

            return new ModelAsset {
                Id = assetId,
                Positions = positions.ToArray(),
                Normals = normals.ToArray(),
                TexCoords = texCoords.ToArray(),
                Indices16 = indices.ToArray(),
                BoundsMin = new float3(-poleRadius, 0f, -Math.Max(poleRadius, flagThickness * 0.5f)),
                BoundsMax = new float3(0.72f, poleHeight, Math.Max(poleRadius, flagThickness * 0.5f)),
                Submeshes = submeshes.ToArray()
            };

            void AppendCylinderY(float3 baseCenter, float radius, float height, int radialSteps) {
                float minY = baseCenter.Y;
                float maxY = baseCenter.Y + height;

                ushort bottomCenterIndex = (ushort)positions.Count;
                positions.Add(new float3(baseCenter.X, minY, baseCenter.Z));
                normals.Add(new float3(0f, -1f, 0f));
                texCoords.Add(new float2(0.5f, 0.5f));

                for (int step = 0; step < radialSteps; step++) {
                    float angle = (MathF.PI * 2f * step) / radialSteps;
                    float x = baseCenter.X + MathF.Cos(angle) * radius;
                    float z = baseCenter.Z + MathF.Sin(angle) * radius;
                    positions.Add(new float3(x, minY, z));
                    normals.Add(new float3(0f, -1f, 0f));
                    texCoords.Add(new float2((MathF.Cos(angle) + 1f) * 0.5f, (MathF.Sin(angle) + 1f) * 0.5f));
                }

                for (int step = 0; step < radialSteps; step++) {
                    ushort ringA = (ushort)(bottomCenterIndex + 1 + step);
                    ushort ringB = (ushort)(bottomCenterIndex + 1 + ((step + 1) % radialSteps));
                    indices.Add(bottomCenterIndex);
                    indices.Add(ringA);
                    indices.Add(ringB);
                }

                ushort topCenterIndex = (ushort)positions.Count;
                positions.Add(new float3(baseCenter.X, maxY, baseCenter.Z));
                normals.Add(new float3(0f, 1f, 0f));
                texCoords.Add(new float2(0.5f, 0.5f));

                for (int step = 0; step < radialSteps; step++) {
                    float angle = (MathF.PI * 2f * step) / radialSteps;
                    float x = baseCenter.X + MathF.Cos(angle) * radius;
                    float z = baseCenter.Z + MathF.Sin(angle) * radius;
                    positions.Add(new float3(x, maxY, z));
                    normals.Add(new float3(0f, 1f, 0f));
                    texCoords.Add(new float2((MathF.Cos(angle) + 1f) * 0.5f, (MathF.Sin(angle) + 1f) * 0.5f));
                }

                for (int step = 0; step < radialSteps; step++) {
                    ushort ringA = (ushort)(topCenterIndex + 1 + step);
                    ushort ringB = (ushort)(topCenterIndex + 1 + ((step + 1) % radialSteps));
                    indices.Add(topCenterIndex);
                    indices.Add(ringB);
                    indices.Add(ringA);
                }

                ushort sideStartIndex = (ushort)positions.Count;
                for (int step = 0; step < radialSteps; step++) {
                    float angle = (MathF.PI * 2f * step) / radialSteps;
                    float x = baseCenter.X + MathF.Cos(angle) * radius;
                    float z = baseCenter.Z + MathF.Sin(angle) * radius;
                    float3 normal = new float3(MathF.Cos(angle), 0f, MathF.Sin(angle));
                    float u = (float)step / radialSteps;

                    positions.Add(new float3(x, maxY, z));
                    normals.Add(normal);
                    texCoords.Add(new float2(u, 0f));

                    positions.Add(new float3(x, minY, z));
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
                    indices.Add(topB);
                    indices.Add(bottomA);

                    indices.Add(topB);
                    indices.Add(bottomB);
                    indices.Add(bottomA);
                }
            }

            void AppendPennant(float3 poleAttachPoint, float flagLength, float flagHeight, float halfThickness) {
                float3 backTop = new float3(poleAttachPoint.X, poleAttachPoint.Y, -halfThickness);
                float3 backBottom = new float3(poleAttachPoint.X, poleAttachPoint.Y - flagHeight, -halfThickness);
                float3 backTip = new float3(poleAttachPoint.X + flagLength, poleAttachPoint.Y - flagHeight * 0.45f, -halfThickness);
                float3 frontTop = new float3(poleAttachPoint.X, poleAttachPoint.Y, halfThickness);
                float3 frontBottom = new float3(poleAttachPoint.X, poleAttachPoint.Y - flagHeight, halfThickness);
                float3 frontTip = new float3(poleAttachPoint.X + flagLength, poleAttachPoint.Y - flagHeight * 0.45f, halfThickness);

                AddTriangle(backTop, backBottom, backTip, new float3(0f, 0f, -1f));
                AddTriangle(frontTop, frontTip, frontBottom, new float3(0f, 0f, 1f));
                AddQuad(frontTop, frontBottom, backBottom, backTop, new float3(-1f, 0f, 0f));

                float3 topEdgeNormal = NormalizeSafe(new float3(flagHeight * 0.45f, flagLength, 0f));
                AddQuad(backTop, backTip, frontTip, frontTop, topEdgeNormal);

                float3 bottomEdgeNormal = NormalizeSafe(new float3(flagHeight * 0.55f, -flagLength, 0f));
                AddQuad(frontBottom, frontTip, backTip, backBottom, bottomEdgeNormal);
            }

            void AddQuad(float3 a, float3 b, float3 c, float3 d, float3 normal) {
                ushort start = (ushort)positions.Count;
                positions.Add(a);
                positions.Add(b);
                positions.Add(c);
                positions.Add(d);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                texCoords.Add(new float2(0f, 0f));
                texCoords.Add(new float2(0f, 1f));
                texCoords.Add(new float2(1f, 1f));
                texCoords.Add(new float2(1f, 0f));

                indices.Add(start);
                indices.Add((ushort)(start + 2));
                indices.Add((ushort)(start + 1));
                indices.Add(start);
                indices.Add((ushort)(start + 3));
                indices.Add((ushort)(start + 2));
            }

            void AddTriangle(float3 a, float3 b, float3 c, float3 normal) {
                ushort start = (ushort)positions.Count;
                positions.Add(a);
                positions.Add(b);
                positions.Add(c);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                texCoords.Add(new float2(0f, 0f));
                texCoords.Add(new float2(0f, 1f));
                texCoords.Add(new float2(1f, 0.5f));

                indices.Add(start);
                indices.Add((ushort)(start + 2));
                indices.Add((ushort)(start + 1));
            }

            float3 NormalizeSafe(float3 value) {
                float length = value.Length();
                return length <= 0.0001f ? new float3(1f, 0f, 0f) : value / length;
            }
        }
    }
}
