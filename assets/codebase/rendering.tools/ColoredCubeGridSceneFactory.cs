using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored colored cube-grid scene and its file-backed material assets.
    /// </summary>
    public sealed class ColoredCubeGridSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated colored cube-grid asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.ColoredCubeGridSceneId;

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
        /// Relative project folder used for the generated colored cube-grid materials.
        /// </summary>
        const string MaterialRootRelativePath = "Materials/rendering/colored_cube_grid";

        /// <summary>
        /// Authored color palette assigned to the sixteen cube materials.
        /// </summary>
        static readonly string[] CubeMaterialColors = {
            "#FF4040FF", "#FF8040FF", "#FFC040FF", "#FFFF40FF",
            "#C0FF40FF", "#80FF40FF", "#40FF40FF", "#40FF80FF",
            "#40FFC0FF", "#40FFFFFF", "#40C0FFFF", "#4080FFFF",
            "#4040FFFF", "#8040FFFF", "#C040FFFF", "#FF40FFFF"
        };

        /// <summary>
        /// Stable authored material paths used by the sixteen cube entities.
        /// </summary>
        static readonly string[] CubeMaterialRelativePaths = BuildMaterialRelativePaths();

        /// <summary>
        /// Stable authored starting orientations used to expose different cube faces before spin updates.
        /// </summary>
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
        /// Initializes the colored cube-grid scene factory with the descriptors and services required for authored output.
        /// </summary>
        public ColoredCubeGridSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            MaterialSettingsService = new MaterialAssetSettingsService();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical colored cube-grid scene asset.
        /// </summary>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <returns>Authored scene asset for the sixteen-cube color grid.</returns>
        public SceneAsset CreateSceneAsset(SceneAssetReference cubeReference) {
            if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            }

            List<SceneEntityAsset> rootEntities = new List<SceneEntityAsset> {
                CreateCameraEntity(),
                CreateDirectionalLightEntity()
            };

            for (int row = 0; row < 4; row++) {
                for (int column = 0; column < 4; column++) {
                    int cubeIndex = (row * 4) + column;
                    rootEntities.Add(CreateCubeEntity(
                        cubeIndex,
                        cubeReference,
                        CreateColoredMaterialReference(cubeIndex),
                        new float3((column - 1.5f) * 3.0f, (1.5f - row) * 3.0f, 0f),
                        float4.Identity));
                }
            }

            List<SceneAssetReference> assetReferences = new List<SceneAssetReference> {
                cubeReference,
                DemoDiscSceneComponentRecordFactory.CreateEditorFontReference()
            };
            for (int cubeIndex = 0; cubeIndex < CubeMaterialRelativePaths.Length; cubeIndex++) {
                assetReferences.Add(CreateColoredMaterialReference(cubeIndex));
            }

            return new SceneAsset {
                Id = SceneId,
                AssetReferences = [.. assetReferences],
                RootEntities = [.. rootEntities]
            };
        }

        /// <summary>
        /// Writes the sixteen file-backed material assets and sidecars used by the colored cube-grid scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            for (int cubeIndex = 0; cubeIndex < CubeMaterialRelativePaths.Length; cubeIndex++) {
                WriteMaterialAsset(projectRootPath, cubeIndex);
            }
        }

        /// <summary>
        /// Creates the authored camera entity for the colored cube-grid scene.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            return new SceneEntityAsset {
                Id = "colored-cube-grid-camera",
                Name = "ColoredCubeGridCamera",
                LocalPosition = new float3(0f, 0f, 18f),
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = [
                    CreateCameraComponentRecord(),
                    DemoDiscSceneComponentRecordFactory.CreateFpsComponentRecord(1),
                    DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(2)
                ],
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored directional light entity for the colored cube-grid scene.
        /// </summary>
        /// <returns>Serialized directional light entity.</returns>
        SceneEntityAsset CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "colored-cube-grid-sun",
                Name = "ColoredCubeGridSun",
                LocalPosition = new float3(0f, 6f, 0f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = [
                    CreateDirectionalLightComponentRecord(1.35f, 40f)
                ],
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one rotating cube entity for the colored cube-grid scene.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="modelReference">Stable generated cube model reference.</param>
        /// <param name="materialReference">Stable file-backed colored material reference.</param>
        /// <param name="localPosition">Authored local position for the cube.</param>
        /// <param name="localOrientation">Authored starting orientation for the cube.</param>
        /// <returns>Serialized cube entity.</returns>
        SceneEntityAsset CreateCubeEntity(
            int cubeIndex,
            SceneAssetReference modelReference,
            SceneAssetReference materialReference,
            float3 localPosition,
            float4 localOrientation) {
            return new SceneEntityAsset {
                Id = CreateCubeEntityId(cubeIndex),
                Name = CreateCubeEntityName(cubeIndex),
                LocalPosition = localPosition,
                LocalScale = new float3(1.5f, 1.5f, 1.5f),
                LocalOrientation = localOrientation,
                Components = [
                    CreateMeshComponentRecord(modelReference, materialReference),
                    RenderingScriptComponentRecordFactory.CreateTowerSpinRecord(1, 0f, (float)(Math.PI / 2.0))
                ],
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
        /// <param name="materialReference">Stable file-backed material reference used by the mesh payload.</param>
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
        /// Writes one serialized camera component payload.
        /// </summary>
        /// <returns>Serialized camera component payload.</returns>
        byte[] WriteCameraPayload() {
            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("CameraDrawOrder", fieldWriter => fieldWriter.WriteByte(0));
            writer.WriteField("LayerMask", fieldWriter => fieldWriter.WriteUInt16(SceneObjectsLayerMask));
            writer.WriteField("Viewport", fieldWriter => fieldWriter.WriteFloat4(new float4(0f, 0f, 1f, 1f)));
            writer.WriteField("NearPlaneDistance", fieldWriter => fieldWriter.WriteSingle(0.1f));
            writer.WriteField("FarPlaneDistance", fieldWriter => fieldWriter.WriteSingle(96f));
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
                        ShadowDistance = 40f,
                        PostProcessTier = PostProcessTier.Disabled
                    }));
            return writer.BuildPayload();
        }

        /// <summary>
        /// Writes one serialized mesh component payload.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh.</param>
        /// <param name="materialReference">Stable material reference used by the mesh.</param>
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
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = intensity,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = shadowDistance
            };
            return DirectionalLightDescriptor.SerializeComponent(lightComponent, 0, null).Payload;
        }

        /// <summary>
        /// Creates one stable file-backed material reference for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Scene asset reference targeting one file-backed colored material.</returns>
        SceneAssetReference CreateColoredMaterialReference(int cubeIndex) {
            if (cubeIndex < 0 || cubeIndex >= CubeMaterialRelativePaths.Length) {
                throw new ArgumentOutOfRangeException(nameof(cubeIndex), "Cube index must address one generated material.");
            }

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = CubeMaterialRelativePaths[cubeIndex],
                ProviderId = string.Empty,
                AssetId = string.Empty
            };
        }

        /// <summary>
        /// Writes one file-backed material asset and its settings sidecar for the supplied cube index.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        void WriteMaterialAsset(string projectRootPath, int cubeIndex) {
            string relativePath = CubeMaterialRelativePaths[cubeIndex];
            string fullPath = Path.Combine(projectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a material directory for '{relativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);

            using (FileStream stream = File.Create(fullPath)) {
                global::helengine.editor.AssetSerializer.Serialize(stream, CreateMaterialAsset(cubeIndex));
            }

            MaterialSettingsService.Save(fullPath, CreateMaterialSettings(cubeIndex));
        }

        /// <summary>
        /// Creates one compatibility material asset that resolves to the shared standard shader.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>File-backed material asset for the supplied cube.</returns>
        MaterialAsset CreateMaterialAsset(int cubeIndex) {
            return new MaterialAsset {
                Id = CreateMaterialAssetId(cubeIndex),
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
        /// Creates one per-platform settings sidecar for the supplied cube material.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Generated import-settings payload for the cube material.</returns>
        MaterialAssetImportSettings CreateMaterialSettings(int cubeIndex) {
            MaterialAssetImportSettings settings = new MaterialAssetImportSettings();
            settings.Importer.ImporterId = MaterialImporterId;
            settings.Importer.SourceChecksum = string.Empty;
            settings.Importer.AssetId = CreateMaterialAssetId(cubeIndex);

            MaterialAssetProcessorSettings windowsSettings = new MaterialAssetProcessorSettings();
            windowsSettings.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.FieldValues[UseCustomShaderFieldId] = "false";
            windowsSettings.FieldValues[TextureIdFieldId] = string.Empty;
            windowsSettings.FieldValues[CastsShadowFieldId] = "true";
            windowsSettings.FieldValues[ReceivesShadowFieldId] = "true";
            windowsSettings.FieldValues[BaseColorFieldId] = CubeMaterialColors[cubeIndex];
            settings.Processor.Platforms["windows"] = windowsSettings;

            MaterialAssetProcessorSettings ps2Settings = new MaterialAssetProcessorSettings();
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.FieldValues[AlphaModeFieldId] = "opaque";
            ps2Settings.FieldValues[DoubleSidedFieldId] = "false";
            ps2Settings.FieldValues[Ps2CastShadowsFieldId] = "true";
            ps2Settings.FieldValues[VertexColorModeFieldId] = "ignore";
            ps2Settings.FieldValues[BaseColorFieldId] = CubeMaterialColors[cubeIndex];
            settings.Processor.Platforms["ps2"] = ps2Settings;

            MaterialAssetProcessorSettings pspSettings = new MaterialAssetProcessorSettings();
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.FieldValues[UseCustomShaderFieldId] = "false";
            pspSettings.FieldValues[TextureIdFieldId] = string.Empty;
            pspSettings.FieldValues[CastsShadowFieldId] = "true";
            pspSettings.FieldValues[ReceivesShadowFieldId] = "true";
            pspSettings.FieldValues[BaseColorFieldId] = CubeMaterialColors[cubeIndex];
            settings.Processor.Platforms["psp"] = pspSettings;
            return settings;
        }

        /// <summary>
        /// Builds the stable per-cube material relative paths used by the colored cube-grid scene.
        /// </summary>
        /// <returns>Stable project-relative material paths.</returns>
        static string[] BuildMaterialRelativePaths() {
            string[] relativePaths = new string[16];
            for (int cubeIndex = 0; cubeIndex < relativePaths.Length; cubeIndex++) {
                relativePaths[cubeIndex] = MaterialRootRelativePath + "/Cube" + cubeIndex.ToString("00") + ".hasset";
            }

            return relativePaths;
        }

        /// <summary>
        /// Creates one stable material asset id for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Material asset id stored inside the serialized file-backed asset.</returns>
        static string CreateMaterialAssetId(int cubeIndex) {
            return "Materials.rendering.colored_cube_grid.Cube" + cubeIndex.ToString("00");
        }

        /// <summary>
        /// Creates one stable cube entity id for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Stable scene entity id.</returns>
        static string CreateCubeEntityId(int cubeIndex) {
            return "colored-cube-grid-cube-" + cubeIndex.ToString("00");
        }

        /// <summary>
        /// Creates one stable cube entity name for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Stable scene entity name.</returns>
        static string CreateCubeEntityName(int cubeIndex) {
            return "ColoredCubeGridCube" + cubeIndex.ToString("00");
        }

    }
}



