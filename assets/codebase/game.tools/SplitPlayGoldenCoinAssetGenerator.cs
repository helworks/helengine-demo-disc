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
        const string GoldenCoinBaseColor = "#FFE27AFF";
        const string GoldenCoinEmissiveColor = "#FFD54A33";
        const string WindowsGoldenCoinRoughness = "0.28";
        const string WindowsGoldenCoinMetallic = "0.10";
        const string WindowsGoldenCoinSpecular = "0.65";

        const string WindowsMaterialSchemaId = "standard-shader";
        const string Ps2MaterialSchemaId = "ps2-simple-lit";
        const string GameCubeMaterialSchemaId = "gamecube-standard";
        const string DsMaterialSchemaId = "ds-standard-lit";
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable component key for the blueprint trigger observer so scene entity-reference overrides survive regeneration.
        /// </summary>
        public const string TriggerObserverComponentKey = "1a2b3c4d5e6f70819203a4b5c6d7e8f9";

        /// <summary>
        /// Stable component key for the blueprint kinematic rigid body so scene overrides survive regeneration.
        /// </summary>
        public const string RigidBodyComponentKey = "8f7e6d5c4b3a2918070605040302010f";

        /// <summary>
        /// Stable component key for the blueprint trigger box collider so scene overrides survive regeneration.
        /// </summary>
        public const string BoxColliderComponentKey = "0f1e2d3c4b5a69788796a5b4c3d2e1f0";

        const string UseCustomShaderFieldId = "use-custom-shader";
        const string ShaderAssetIdFieldId = "shader-asset-id";
        const string RoughnessFieldId = "roughness";
        const string MetallicFieldId = "metallic";
        const string SpecularFieldId = "specular";
        const string BaseColorFieldId = "base-color";
        const string EmissiveColorFieldId = "emissive-color";
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
                CreateBlueprintAsset(projectRootPath));
        }

        BlueprintAsset CreateBlueprintAsset(string projectRootPath) {
            SceneAssetReference commonModelReference = global::helengine.editor.EditorAssetReferenceFactory.CreateFileReference(projectRootPath, SplitPlayAssetCatalog.GoldenCoinCommonModelRelativePath, AssetEntryKind.Model);
            SceneAssetReference dsModelReference = global::helengine.editor.EditorAssetReferenceFactory.CreateFileReference(projectRootPath, SplitPlayAssetCatalog.GoldenCoinDsModelRelativePath, AssetEntryKind.Model);
            SceneAssetReference materialReference = global::helengine.editor.EditorAssetReferenceFactory.CreateFileReference(projectRootPath, SplitPlayAssetCatalog.GoldenCoinMaterialRelativePath, AssetEntryKind.Material);

            MeshComponent meshComponent = new MeshComponent();
            meshComponent.Materials = new RuntimeMaterial[] { null };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference("Model", commonModelReference);
            saveState.SetAssetReference("Materials[0]", materialReference);

            EntityComponentPlatformOverrideState dsOverride = saveState.GetOrCreatePlatformOverride("ds");
            dsOverride.Payload = Array.Empty<byte>();
            dsOverride.SetAssetReference("Model", dsModelReference);

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneComponentAssetRecord baseRecord = registry.GetDescriptor(meshComponent).SerializeComponent(meshComponent, 0, saveState);
            SceneComponentAssetRecord meshRecord = new ComponentPlatformOverridePayloadService().Wrap(baseRecord, saveState);
            SceneComponentAssetRecord collectibleRecord = AutomaticDescriptor.SerializeComponent(new city.game.TiltTrialCollectibleCoinComponent(), 1, null);
            SceneComponentAssetRecord idleMotionRecord = AutomaticDescriptor.SerializeComponent(new city.game.SplitPlayIdleMotionComponent(), 2, null);
            SceneComponentAssetRecord observerRecord = SerializeWithStableKey(registry, new global::helengine.SceneEntityTriggerObserverComponent(), 3, TriggerObserverComponentKey);
            SceneComponentAssetRecord rigidBodyRecord = SerializeWithStableKey(registry, new RigidBody3DComponent {
                BodyKind = BodyKind3D.Kinematic,
                UseGravity = false,
                Mass = 1d
            }, 4, RigidBodyComponentKey);
            BoxCollider3DComponent triggerCollider = new BoxCollider3DComponent {
                Size = new float3(3f, 6f, 3f)
            };
            triggerCollider.IsTrigger = true;
            SceneComponentAssetRecord colliderRecord = SerializeWithStableKey(registry, triggerCollider, 5, BoxColliderComponentKey);

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
                    Components = [meshRecord, collectibleRecord, idleMotionRecord, observerRecord, rigidBodyRecord, colliderRecord],
                    Children = Array.Empty<SceneEntityAsset>()
                },
                AssetReferences = [commonModelReference, dsModelReference, materialReference]
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

        CityGeneratedMaterialAssetDefinition CreateMaterialDefinition() {
            CityGeneratedMaterialAssetDefinition definition = new CityGeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = SplitPlayAssetCatalog.GoldenCoinMaterialAssetId,
                RenderState = new MaterialRenderState {
                    CullMode = MaterialCullMode.Back
                },
                CastsShadows = false,
                ReceivesShadows = false
            };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"));
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"));
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"));
            ConfigureGameCubePlatform(definition.GetOrCreatePlatform("gamecube"));
            ConfigureDsPlatform(definition.GetOrCreatePlatform("ds"));
            return definition;
        }

        void ConfigureWindowsPlatform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = WindowsMaterialSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(RoughnessFieldId, WindowsGoldenCoinRoughness);
            platformDefinition.SetFieldValue(MetallicFieldId, WindowsGoldenCoinMetallic);
            platformDefinition.SetFieldValue(SpecularFieldId, WindowsGoldenCoinSpecular);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "false");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "false");
            platformDefinition.SetFieldValue(BaseColorFieldId, GoldenCoinBaseColor);
            platformDefinition.SetFieldValue(EmissiveColorFieldId, GoldenCoinEmissiveColor);
        }

        void ConfigurePs2Platform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = Ps2MaterialSchemaId;
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(Ps2CastShadowsFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, GoldenCoinBaseColor);
        }

        void ConfigureGameCubePlatform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = GameCubeMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, GoldenCoinBaseColor);
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        void ConfigureDsPlatform(CityGeneratedMaterialPlatformDefinition platformDefinition) {
            platformDefinition.SchemaId = DsMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, GoldenCoinBaseColor);
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        ModelAsset CreateCylinderModel(string assetId, int radialSteps) {
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            } else if (radialSteps < 3) {
                throw new ArgumentOutOfRangeException(nameof(radialSteps), "Cylinder generation requires at least three radial steps.");
            }

            const float radius = 0.5f;
            const float rimHalfDepth = 0.04f;

            ModelAsset engineCylinder = TransformGizmoMeshFactory.CreateCylinder(radius, rimHalfDepth * 2f, radialSteps);
            float3[] positions = new float3[engineCylinder.Positions.Length];
            float3[] normals = new float3[engineCylinder.Normals.Length];
            float2[] texCoords = new float2[engineCylinder.TexCoords.Length];
            ushort[] indices = ExtractSingleSidedTriangles(engineCylinder.Indices16, radialSteps);

            for (int index = 0; index < engineCylinder.Positions.Length; index++) {
                float3 centeredPosition = engineCylinder.Positions[index] - new float3(0f, rimHalfDepth, 0f);
                positions[index] = RotateYAxisCylinderToCoinAxis(centeredPosition);
                normals[index] = RotateYAxisCylinderToCoinAxis(engineCylinder.Normals[index]);
                texCoords[index] = engineCylinder.TexCoords[index];
            }

            return new ModelAsset {
                Id = assetId,
                Positions = positions,
                Normals = normals,
                TexCoords = texCoords,
                Indices16 = indices,
                BoundsMin = new float3(-radius, -radius, -rimHalfDepth),
                BoundsMax = new float3(radius, radius, rimHalfDepth),
                Submeshes = [
                    new ModelSubmeshAsset {
                        MaterialSlotName = "DefaultMaterial",
                        IndexStart = 0,
                        IndexCount = indices.Length
                    }
                ]
            };

            static float3 RotateYAxisCylinderToCoinAxis(float3 value) {
                return new float3(value.X, -value.Z, value.Y);
            }

            static ushort[] ExtractSingleSidedTriangles(ushort[] sourceIndices, int radialSteps) {
                if (sourceIndices == null) {
                    throw new InvalidOperationException("Engine cylinder generation must provide triangle indices.");
                } else if (sourceIndices.Length % 6 != 0) {
                    throw new InvalidOperationException("Expected the engine cylinder helper to emit paired double-sided triangles.");
                }

                ushort[] singleSidedIndices = new ushort[sourceIndices.Length / 2];
                int writeIndex = 0;
                for (int sourceIndex = 0; sourceIndex < sourceIndices.Length; sourceIndex += 6) {
                    singleSidedIndices[writeIndex++] = sourceIndices[sourceIndex];
                    singleSidedIndices[writeIndex++] = sourceIndices[sourceIndex + 1];
                    singleSidedIndices[writeIndex++] = sourceIndices[sourceIndex + 2];
                }

                int sideTriangleCount = radialSteps * 2;
                int bottomCapTriangleStart = sideTriangleCount;
                int topCapTriangleStart = sideTriangleCount + radialSteps;
                FlipTriangleRange(singleSidedIndices, bottomCapTriangleStart, radialSteps);
                FlipTriangleRange(singleSidedIndices, topCapTriangleStart, radialSteps);

                return singleSidedIndices;
            }

            static void FlipTriangleRange(ushort[] indices, int triangleStart, int triangleCount) {
                for (int triangleIndex = triangleStart; triangleIndex < triangleStart + triangleCount; triangleIndex++) {
                    int indexOffset = triangleIndex * 3;
                    ushort b = indices[indexOffset + 1];
                    indices[indexOffset + 1] = indices[indexOffset + 2];
                    indices[indexOffset + 2] = b;
                }
            }
        }
    }
}
