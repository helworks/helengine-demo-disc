using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored axis-test scene and its file-backed lit material assets.
    /// </summary>
    public sealed class AxisTestSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated axis-test asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.AxisTestSceneId;

        /// <summary>
        /// Stable serialized component identifier used by mesh records.
        /// </summary>
        const string MeshComponentTypeId = "helengine.MeshComponent";

        /// <summary>
        /// Stable serialized component identifier used by camera records.
        /// </summary>
        const string CameraComponentTypeId = "helengine.CameraComponent";

        /// <summary>
        /// Stable serialized component identifier used by directional-light records.
        /// </summary>
        const string DirectionalLightComponentTypeId = "helengine.DirectionalLightComponent";

        /// <summary>
        /// Stable material importer identifier stored on generated sidecar settings.
        /// </summary>
        const string MaterialImporterId = "helengine.material";

        /// <summary>
        /// Stable Windows standard-material schema identifier used by the shared editor material pipeline.
        /// </summary>
        const string WindowsMaterialSchemaId = "standard-shader";

        /// <summary>
        /// Stable PS2 lit material schema identifier used by the PS2 runtime path.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable standard shader asset identifier used by compatibility material payloads.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable standard shader vertex program used by compatibility material payloads.
        /// </summary>
        const string StandardVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Stable standard shader pixel program used by compatibility material payloads.
        /// </summary>
        const string StandardPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Stable mesh variant used by compatibility material payloads.
        /// </summary>
        const string MeshVariantName = "Mesh";

        /// <summary>
        /// Stable material field identifier used to opt into standard-shader defaults.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

        /// <summary>
        /// Stable material field identifier used for authored texture bindings.
        /// </summary>
        const string TextureIdFieldId = "texture-id";

        /// <summary>
        /// Stable material field identifier used for shadow-casting participation.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// Stable PS2 material field identifier used for shadow-casting participation.
        /// </summary>
        const string Ps2CastShadowsFieldId = "cast-shadows";

        /// <summary>
        /// Stable material field identifier used for shadow receiving.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Stable material field identifier used for authored base color.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Stable PS2 material field identifier used for alpha mode.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Stable PS2 material field identifier used for double-sided control.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Stable PS2 material field identifier used for vertex-color control.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Layer mask used by authored scene objects in packaged runtime scenes.
        /// </summary>
        const ushort SceneObjectsLayerMask = 0b0100000000000000;

        /// <summary>
        /// Stable save-state slot name used for serialized mesh model references.
        /// </summary>
        const string MeshModelReferenceName = "Model";

        /// <summary>
        /// Stable save-state slot name used for serialized mesh material references.
        /// </summary>
        const string MeshMaterialReferenceName = "Material";

        /// <summary>
        /// Relative project folder used for the generated axis-test materials.
        /// </summary>
        const string MaterialRootRelativePath = "materials/rendering/axis_test";

        /// <summary>
        /// Authored colors used by the axis shafts, neutral ground, and lighting markers.
        /// </summary>
        static readonly string[] AxisMaterialColors = {
            "#FF4040FF",
            "#40FF40FF",
            "#4080FFFF",
            "#B8C2CCFF",
            "#FFFFFFFF"
        };

        /// <summary>
        /// Stable authored material paths used by the axis-test scene.
        /// </summary>
        static readonly string[] AxisMaterialRelativePaths = BuildMaterialRelativePaths();

        /// <summary>
        /// Descriptor used to serialize authored mesh payloads for committed editor scenes.
        /// </summary>
        readonly MeshComponentPersistenceDescriptor MeshDescriptor;

        /// <summary>
        /// Descriptor used to serialize authored directional-light payloads for committed editor scenes.
        /// </summary>
        readonly DirectionalLightComponentPersistenceDescriptor DirectionalLightDescriptor;

        /// <summary>
        /// Service used to write generated material settings sidecars.
        /// </summary>
        readonly MaterialAssetSettingsService MaterialSettingsService;

        /// <summary>
        /// Placeholder runtime model used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly AuthoringPlaceholderRuntimeModel PlaceholderModel;

        /// <summary>
        /// Placeholder runtime material used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly RuntimeMaterial PlaceholderMaterial;

        /// <summary>
        /// Initializes the axis-test scene factory with the descriptors and services required for authored output.
        /// </summary>
        public AxisTestSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            MaterialSettingsService = new MaterialAssetSettingsService();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical axis-test scene asset.
        /// </summary>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <returns>Authored scene asset for the three-axis transform-gizmo test.</returns>
        public SceneAsset CreateSceneAsset(SceneAssetReference cubeReference) {
            if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            }

            return new SceneAsset {
                Id = "axis_test",
                AssetReferences = new[] {
                    cubeReference,
                    CreateAxisMaterialReference(0),
                    CreateAxisMaterialReference(1),
                    CreateAxisMaterialReference(2),
                    CreateAxisMaterialReference(3),
                    CreateAxisMaterialReference(4)
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateFloorEntity(cubeReference, CreateAxisMaterialReference(3)),
                    CreateGroundEntity(cubeReference, CreateAxisMaterialReference(3)),
                    CreateXAxisEntity(cubeReference, CreateAxisMaterialReference(0)),
                    CreateYAxisEntity(cubeReference, CreateAxisMaterialReference(1)),
                    CreateZAxisEntity(cubeReference, CreateAxisMaterialReference(2)),
                    CreateOriginMarkerEntity(cubeReference, CreateAxisMaterialReference(4)),
                    CreateXAxisMarkerEntity(cubeReference, CreateAxisMaterialReference(4)),
                    CreateYAxisMarkerEntity(cubeReference, CreateAxisMaterialReference(4)),
                    CreateZAxisMarkerEntity(cubeReference, CreateAxisMaterialReference(4))
                }
            };
        }

        /// <summary>
        /// Writes the file-backed material assets and sidecars used by the axis-test scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            for (int materialIndex = 0; materialIndex < AxisMaterialRelativePaths.Length; materialIndex++) {
                WriteMaterialAsset(projectRootPath, materialIndex);
            }
        }

        /// <summary>
        /// Creates the authored camera entity for the axis-test scene.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            return new SceneEntityAsset {
                Id = "axis-test-camera",
                Name = "AxisTestCamera",
                LocalPosition = new float3(5f, 6f, 30f),
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(1)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored directional light entity for the axis-test scene.
        /// </summary>
        /// <returns>Serialized directional light entity.</returns>
        SceneEntityAsset CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "axis-test-sun",
                Name = "AxisTestSun",
                LocalPosition = new float3(0f, 8f, 0f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateDirectionalLightComponentRecord(1.2f, 32f)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored X-axis mesh entity.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed red material reference.</param>
        /// <returns>Serialized X-axis entity.</returns>
        SceneEntityAsset CreateXAxisEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-x-axis",
                "AxisTestXAxis",
                new float3(5f, 0f, 0f),
                new float3(10f, 0.5f, 0.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored Y-axis mesh entity.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed green material reference.</param>
        /// <returns>Serialized Y-axis entity.</returns>
        SceneEntityAsset CreateYAxisEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-y-axis",
                "AxisTestYAxis",
                new float3(0f, 5f, 0f),
                new float3(0.5f, 10f, 0.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored Z-axis mesh entity.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed blue material reference.</param>
        /// <returns>Serialized Z-axis entity.</returns>
        SceneEntityAsset CreateZAxisEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-z-axis",
                "AxisTestZAxis",
                new float3(0f, 0f, 5f),
                new float3(0.5f, 0.5f, 10f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored neutral floor entity used to make directional lighting readable.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed ground material reference.</param>
        /// <returns>Serialized floor entity.</returns>
        SceneEntityAsset CreateFloorEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-floor",
                "AxisTestFloor",
                new float3(5f, -5f, 5f),
                new float3(14f, 0.5f, 14f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored neutral wall entity used to read light direction against one large surface.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed ground material reference.</param>
        /// <returns>Serialized wall entity.</returns>
        SceneEntityAsset CreateGroundEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-ground",
                "AxisTestGround",
                new float3(16f, 5f, 5f),
                new float3(0.5f, 12f, 14f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the world origin.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized origin marker entity.</returns>
        SceneEntityAsset CreateOriginMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-origin-marker",
                "AxisTestOriginMarker",
                float3.Zero,
                new float3(1.25f, 1.25f, 1.25f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the positive X-axis endpoint.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized positive X marker entity.</returns>
        SceneEntityAsset CreateXAxisMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-x-marker",
                "AxisTestXMarker",
                new float3(10f, 0f, 0f),
                new float3(1.5f, 1.5f, 1.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the positive Y-axis endpoint.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized positive Y marker entity.</returns>
        SceneEntityAsset CreateYAxisMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-y-marker",
                "AxisTestYMarker",
                new float3(0f, 10f, 0f),
                new float3(1.5f, 1.5f, 1.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the positive Z-axis endpoint.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized positive Z marker entity.</returns>
        SceneEntityAsset CreateZAxisMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-z-marker",
                "AxisTestZMarker",
                new float3(0f, 0f, 10f),
                new float3(1.5f, 1.5f, 1.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates one axis mesh entity with the supplied transform.
        /// </summary>
        /// <param name="id">Stable entity id.</param>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed material reference used by the mesh payload.</param>
        /// <returns>Serialized axis mesh entity.</returns>
        SceneEntityAsset CreateAxisEntity(
            string id,
            string name,
            float3 localPosition,
            float3 localScale,
            SceneAssetReference modelReference,
            SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = id,
                Name = name,
                LocalPosition = localPosition,
                LocalScale = localScale,
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReference)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one serialized camera component record.
        /// </summary>
        /// <returns>Serialized camera component record.</returns>
        SceneComponentAssetRecord CreateCameraComponentRecord() {
            return new SceneComponentAssetRecord {
                ComponentTypeId = CameraComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteCameraPayload()
            };
        }

        /// <summary>
        /// Creates one serialized mesh component record.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized mesh component record.</returns>
        SceneComponentAssetRecord CreateMeshComponentRecord(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = MeshComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteMeshPayload(modelReference, materialReference)
            };
        }

        /// <summary>
        /// Creates one serialized directional-light component record.
        /// </summary>
        /// <param name="intensity">Authored directional-light intensity.</param>
        /// <param name="shadowDistance">Authored directional-light shadow cutoff distance.</param>
        /// <returns>Serialized directional-light component record.</returns>
        SceneComponentAssetRecord CreateDirectionalLightComponentRecord(float intensity, float shadowDistance) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = DirectionalLightComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteDirectionalLightPayload(intensity, shadowDistance)
            };
        }

        /// <summary>
        /// Creates one stable file-backed material reference for the supplied axis-test material index.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>Scene asset reference targeting one file-backed axis-test material.</returns>
        SceneAssetReference CreateAxisMaterialReference(int materialIndex) {
            if (materialIndex < 0 || materialIndex >= AxisMaterialRelativePaths.Length) {
                throw new ArgumentOutOfRangeException(nameof(materialIndex), "Axis-test material index must address one generated material.");
            }

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = AxisMaterialRelativePaths[materialIndex],
                ProviderId = string.Empty,
                AssetId = string.Empty
            };
        }

        /// <summary>
        /// Writes one file-backed axis-test material asset and its settings sidecar.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        void WriteMaterialAsset(string projectRootPath, int materialIndex) {
            string relativePath = AxisMaterialRelativePaths[materialIndex];
            string fullPath = Path.Combine(projectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a material directory for '{relativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);

            using (FileStream stream = File.Create(fullPath)) {
                global::helengine.editor.AssetSerializer.Serialize(stream, CreateMaterialAsset(materialIndex));
            }

            MaterialSettingsService.Save(fullPath, CreateMaterialSettings(materialIndex));
        }

        /// <summary>
        /// Creates one compatibility material asset that resolves to the shared standard shader.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>File-backed material asset for the supplied axis-test material.</returns>
        MaterialAsset CreateMaterialAsset(int materialIndex) {
            return new MaterialAsset {
                Id = CreateMaterialAssetId(materialIndex),
                ShaderAssetId = StandardShaderAssetId,
                VertexProgram = StandardVertexProgramName,
                PixelProgram = StandardPixelProgramName,
                Variant = MeshVariantName,
                RenderState = new MaterialRenderState(),
                ConstantBuffers = Array.Empty<MaterialConstantBufferAsset>(),
                CastsShadows = true,
                ReceivesShadows = true
            };
        }

        /// <summary>
        /// Creates one per-platform settings sidecar for the supplied axis-test material.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>Generated import-settings payload for the axis-test material.</returns>
        AssetImportSettings CreateMaterialSettings(int materialIndex) {
            AssetImportSettings settings = new AssetImportSettings();
            settings.Importer.ImporterId = MaterialImporterId;
            settings.Importer.SourceChecksum = string.Empty;
            settings.Importer.AssetId = CreateMaterialAssetId(materialIndex);

            string baseColor = AxisMaterialColors[materialIndex];

            AssetPlatformProcessorSettings windowsSettings = new AssetPlatformProcessorSettings();
            windowsSettings.Material.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.Material.FieldValues[UseCustomShaderFieldId] = "false";
            windowsSettings.Material.FieldValues[TextureIdFieldId] = string.Empty;
            windowsSettings.Material.FieldValues[CastsShadowFieldId] = "true";
            windowsSettings.Material.FieldValues[ReceivesShadowFieldId] = "true";
            windowsSettings.Material.FieldValues[BaseColorFieldId] = baseColor;
            settings.Processor.Platforms["windows"] = windowsSettings;

            AssetPlatformProcessorSettings ps2Settings = new AssetPlatformProcessorSettings();
            ps2Settings.Material.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.Material.FieldValues[AlphaModeFieldId] = "opaque";
            ps2Settings.Material.FieldValues[DoubleSidedFieldId] = "false";
            ps2Settings.Material.FieldValues[Ps2CastShadowsFieldId] = "true";
            ps2Settings.Material.FieldValues[VertexColorModeFieldId] = "ignore";
            ps2Settings.Material.FieldValues[BaseColorFieldId] = baseColor;
            settings.Processor.Platforms["ps2"] = ps2Settings;
            return settings;
        }

        /// <summary>
        /// Writes one serialized camera component payload.
        /// </summary>
        /// <returns>Serialized camera component payload.</returns>
        byte[] WriteCameraPayload() {
            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("CameraDrawOrder", fieldWriter => fieldWriter.WriteByte(0));
            writer.WriteField("LayerMask", fieldWriter => fieldWriter.WriteUInt16(SceneObjectsLayerMask));
            writer.WriteField("Viewport", fieldWriter => fieldWriter.WriteFloat4(new float4(0f, 0f, 1f, 1f)));
            writer.WriteField("NearPlaneDistance", fieldWriter => fieldWriter.WriteSingle(0.1f));
            writer.WriteField("FarPlaneDistance", fieldWriter => fieldWriter.WriteSingle(64f));
            writer.WriteField(
                "ClearSettings",
                fieldWriter => SceneComponentBinaryFieldEncoding.WriteCameraClearSettings(
                    fieldWriter,
                    new CameraClearSettings(
                        true,
                        new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                        true,
                        1f,
                        false,
                        0)));
            writer.WriteField(
                "RenderSettings",
                fieldWriter => SceneComponentBinaryFieldEncoding.WriteCameraRenderSettings(
                    fieldWriter,
                    new CameraRenderSettings {
                        DepthPrepassMode = DepthPrepassMode.Auto,
                        ShadowDistance = 32f,
                        PostProcessTier = PostProcessTier.Disabled
                    }));
            return writer.BuildPayload();
        }

        /// <summary>
        /// Writes one serialized mesh component payload.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh.</param>
        /// <returns>Serialized mesh component payload.</returns>
        byte[] WriteMeshPayload(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            } else if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            MeshComponent meshComponent = new MeshComponent {
                Model = PlaceholderModel,
                Material = PlaceholderMaterial,
                RenderOrder3D = 0
            };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference(MeshModelReferenceName, modelReference);
            saveState.SetAssetReference(MeshMaterialReferenceName, materialReference);
            return MeshDescriptor.SerializeComponent(meshComponent, 0, saveState).Payload;
        }

        /// <summary>
        /// Writes one serialized directional-light component payload.
        /// </summary>
        /// <param name="intensity">Authored directional-light intensity.</param>
        /// <param name="shadowDistance">Authored directional-light shadow cutoff distance.</param>
        /// <returns>Serialized directional-light component payload.</returns>
        byte[] WriteDirectionalLightPayload(float intensity, float shadowDistance) {
            DirectionalLightComponent lightComponent = new DirectionalLightComponent {
                Color = new float4(1f, 0.95f, 0.9f, 1f),
                Intensity = intensity,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = shadowDistance
            };
            return DirectionalLightDescriptor.SerializeComponent(lightComponent, 0, null).Payload;
        }

        /// <summary>
        /// Builds the stable material relative paths used by the axis-test scene.
        /// </summary>
        /// <returns>Stable project-relative material paths.</returns>
        static string[] BuildMaterialRelativePaths() {
            return new[] {
                MaterialRootRelativePath + "/X.helmat",
                MaterialRootRelativePath + "/Y.helmat",
                MaterialRootRelativePath + "/Z.helmat",
                MaterialRootRelativePath + "/Ground.helmat",
                MaterialRootRelativePath + "/Marker.helmat"
            };
        }

        /// <summary>
        /// Creates one stable material asset id for the supplied axis-test material index.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>Material asset id stored inside the serialized file-backed asset.</returns>
        static string CreateMaterialAssetId(int materialIndex) {
            switch (materialIndex) {
                case 0:
                    return "Materials.rendering.axis_test.X";
                case 1:
                    return "Materials.rendering.axis_test.Y";
                case 2:
                    return "Materials.rendering.axis_test.Z";
                case 3:
                    return "Materials.rendering.axis_test.Ground";
                case 4:
                    return "Materials.rendering.axis_test.Marker";
                default:
                    throw new ArgumentOutOfRangeException(nameof(materialIndex), "Axis-test material index must be between zero and four.");
            }
        }
    }
}
