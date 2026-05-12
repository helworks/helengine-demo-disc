using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored textured cube-grid scene plus its file-backed textures and materials.
    /// </summary>
    public sealed class TexturedCubeGridSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated textured cube-grid asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.TexturedCubeGridSceneId;

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
        /// Stable texture importer identifier stored on generated texture sidecar settings.
        /// </summary>
        const string TextureImporterId = "gdi";

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
        /// Relative project folder used for the generated textured cube-grid materials.
        /// </summary>
        const string MaterialRootRelativePath = "Materials/rendering/textured_cube_grid";

        /// <summary>
        /// Relative project folder used for the generated textured cube-grid texture sources.
        /// </summary>
        const string TextureRootRelativePath = "Textures/rendering/textured_cube_grid";

        /// <summary>
        /// Enumerates the generated realistic surface families used by the textured cube-grid scene.
        /// </summary>
        enum TextureSurfaceKind {
            /// <summary>
            /// Brick masonry with visible mortar lines.
            /// </summary>
            Brick,

            /// <summary>
            /// Large-cut stone or block masonry.
            /// </summary>
            StoneBlock,

            /// <summary>
            /// Poured or weathered concrete surfaces.
            /// </summary>
            Concrete,

            /// <summary>
            /// Smaller repeating ceramic or plaster tile surfaces.
            /// </summary>
            Tile
        }

        /// <summary>
        /// Describes one generated realistic texture variant for the textured cube-grid scene.
        /// </summary>
        readonly struct RealisticTextureDefinition {
            /// <summary>
            /// Initializes one realistic texture definition.
            /// </summary>
            public RealisticTextureDefinition(
                TextureSurfaceKind surfaceKind,
                string primaryColor,
                string secondaryColor,
                string accentColor,
                int cellWidth,
                int cellHeight,
                int mortarThickness,
                double noiseStrength,
                double wearStrength) {
                SurfaceKind = surfaceKind;
                PrimaryColor = primaryColor;
                SecondaryColor = secondaryColor;
                AccentColor = accentColor;
                CellWidth = cellWidth;
                CellHeight = cellHeight;
                MortarThickness = mortarThickness;
                NoiseStrength = noiseStrength;
                WearStrength = wearStrength;
            }

            /// <summary>
            /// Gets the surface family for the generated texture.
            /// </summary>
            public TextureSurfaceKind SurfaceKind { get; }

            /// <summary>
            /// Gets the primary color used for the generated surface.
            /// </summary>
            public string PrimaryColor { get; }

            /// <summary>
            /// Gets the secondary color used for the generated surface.
            /// </summary>
            public string SecondaryColor { get; }

            /// <summary>
            /// Gets the accent color used for highlights, wear, and mortar variation.
            /// </summary>
            public string AccentColor { get; }

            /// <summary>
            /// Gets the nominal cell width for one repeating masonry or tile unit.
            /// </summary>
            public int CellWidth { get; }

            /// <summary>
            /// Gets the nominal cell height for one repeating masonry or tile unit.
            /// </summary>
            public int CellHeight { get; }

            /// <summary>
            /// Gets the mortar or seam thickness expressed in pixels.
            /// </summary>
            public int MortarThickness { get; }

            /// <summary>
            /// Gets the deterministic noise strength applied to the generated surface.
            /// </summary>
            public double NoiseStrength { get; }

            /// <summary>
            /// Gets the deterministic wear strength applied to the generated surface.
            /// </summary>
            public double WearStrength { get; }
        }

        /// <summary>
        /// Generated texture width used by the diagnostic cube-grid textures.
        /// </summary>
        const int TextureWidth = 64;

        /// <summary>
        /// Generated texture height used by the diagnostic cube-grid textures.
        /// </summary>
        const int TextureHeight = 64;

        /// <summary>
        /// Realistic texture definitions used to generate the sixteen authored textured cube-grid materials.
        /// </summary>
        static readonly RealisticTextureDefinition[] TextureDefinitions = {
            new(TextureSurfaceKind.Brick, "#7A3028FF", "#5A221CFF", "#A45642FF", 14, 8, 2, 0.10, 0.08),
            new(TextureSurfaceKind.Brick, "#8B3A2BFF", "#64281DFF", "#B5694CFF", 12, 7, 2, 0.11, 0.10),
            new(TextureSurfaceKind.Brick, "#6F2F24FF", "#4D2018FF", "#8E5140FF", 16, 8, 2, 0.09, 0.12),
            new(TextureSurfaceKind.Brick, "#9A4C34FF", "#733524FF", "#C07C56FF", 13, 8, 2, 0.10, 0.08),
            new(TextureSurfaceKind.Brick, "#7B4331FF", "#583022FF", "#A46C58FF", 15, 9, 2, 0.08, 0.11),
            new(TextureSurfaceKind.Brick, "#91553CFF", "#6B3E2BFF", "#B88563FF", 14, 8, 2, 0.08, 0.09),
            new(TextureSurfaceKind.StoneBlock, "#8A8177FF", "#6F685FFF", "#AAA196FF", 16, 12, 2, 0.07, 0.06),
            new(TextureSurfaceKind.StoneBlock, "#7E756BFF", "#635C54FF", "#9E948AFF", 18, 12, 2, 0.06, 0.08),
            new(TextureSurfaceKind.StoneBlock, "#958C81FF", "#766F66FF", "#B5ACA0FF", 14, 10, 2, 0.07, 0.07),
            new(TextureSurfaceKind.StoneBlock, "#A0978BFF", "#80786EFF", "#C0B7AAFF", 20, 12, 2, 0.05, 0.07),
            new(TextureSurfaceKind.Concrete, "#8A8A84FF", "#70706BFF", "#A4A49EFF", 32, 32, 0, 0.12, 0.10),
            new(TextureSurfaceKind.Concrete, "#7B7A74FF", "#61605BFF", "#94938DFF", 32, 32, 0, 0.10, 0.13),
            new(TextureSurfaceKind.Concrete, "#9B978FFF", "#7E7A73FF", "#B5B1A8FF", 32, 32, 0, 0.09, 0.12),
            new(TextureSurfaceKind.Tile, "#6C5E4BFF", "#544838FF", "#8B7A63FF", 10, 10, 1, 0.05, 0.05),
            new(TextureSurfaceKind.Tile, "#4F5F67FF", "#3E4B52FF", "#6D8089FF", 8, 8, 1, 0.04, 0.04),
            new(TextureSurfaceKind.Tile, "#837B70FF", "#675F56FF", "#A19A90FF", 12, 12, 1, 0.05, 0.06)
        };

        /// <summary>
        /// Stable authored material paths used by the sixteen cube entities.
        /// </summary>
        static readonly string[] CubeMaterialRelativePaths = BuildMaterialRelativePaths();

        /// <summary>
        /// Stable authored texture source paths used by the sixteen cube materials.
        /// </summary>
        static readonly string[] CubeTextureRelativePaths = BuildTextureRelativePaths();

        /// <summary>
        /// Stable imported texture asset identifiers derived from the generated texture bytes.
        /// </summary>
        static readonly string[] CubeTextureAssetIds = BuildTextureAssetIds();

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
        /// Initializes the textured cube-grid scene factory with the descriptors and services required for authored output.
        /// </summary>
        public TexturedCubeGridSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            MaterialSettingsService = new MaterialAssetSettingsService();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical textured cube-grid scene asset.
        /// </summary>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <returns>Authored scene asset for the sixteen-cube textured grid.</returns>
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
                        CreateTexturedMaterialReference(cubeIndex),
                        new float3((column - 1.5f) * 3.0f, (1.5f - row) * 3.0f, 0f),
                        float4.Identity));
                }
            }

            List<SceneAssetReference> assetReferences = new List<SceneAssetReference> {
                cubeReference,
                DemoDiscSceneComponentRecordFactory.CreateEditorFontReference()
            };
            for (int cubeIndex = 0; cubeIndex < CubeMaterialRelativePaths.Length; cubeIndex++) {
                assetReferences.Add(CreateTexturedMaterialReference(cubeIndex));
            }

            return new SceneAsset {
                Id = SceneId,
                AssetReferences = [.. assetReferences],
                RootEntities = [.. rootEntities]
            };
        }

        /// <summary>
        /// Writes the generated texture sources, texture sidecars, material assets, and material sidecars used by the textured cube-grid scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            for (int cubeIndex = 0; cubeIndex < CubeTextureRelativePaths.Length; cubeIndex++) {
                WriteTextureSource(projectRootPath, cubeIndex);
                WriteMaterialAsset(projectRootPath, cubeIndex);
            }
        }

        /// <summary>
        /// Creates the authored camera entity for the textured cube-grid scene.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            return new SceneEntityAsset {
                Id = "textured-cube-grid-camera",
                Name = "TexturedCubeGridCamera",
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
        /// Creates the authored directional light entity for the textured cube-grid scene.
        /// </summary>
        /// <returns>Serialized directional light entity.</returns>
        SceneEntityAsset CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "textured-cube-grid-sun",
                Name = "TexturedCubeGridSun",
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
        /// Creates one rotating cube entity for the textured cube-grid scene.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="modelReference">Stable generated cube model reference.</param>
        /// <param name="materialReference">Stable file-backed textured material reference.</param>
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
        /// <returns>Scene asset reference targeting one file-backed textured material.</returns>
        SceneAssetReference CreateTexturedMaterialReference(int cubeIndex) {
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
        /// Writes one generated texture source file and its import-settings sidecar.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        void WriteTextureSource(string projectRootPath, int cubeIndex) {
            string relativePath = CubeTextureRelativePaths[cubeIndex];
            string fullPath = Path.Combine(projectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a texture directory for '{relativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);
            byte[] textureBytes = BuildTextureFileBytes(cubeIndex);
            File.WriteAllBytes(fullPath, textureBytes);

            using FileStream stream = File.Create(fullPath + ".hasset");
            AssetImportSettingsBinarySerializer.Serialize(stream, CreateTextureImportSettings(cubeIndex, textureBytes));
            WriteTextureCacheAsset(projectRootPath, cubeIndex);
        }

        /// <summary>
        /// Writes one serialized cached <see cref="TextureAsset"/> for the supplied generated source texture so the build pipeline can package it without requiring a separate import pass.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        void WriteTextureCacheAsset(string projectRootPath, int cubeIndex) {
            string cachePath = Path.Combine(projectRootPath, "cache", CubeTextureAssetIds[cubeIndex]);
            string directoryPath = Path.GetDirectoryName(cachePath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a texture cache directory for '{CubeTextureAssetIds[cubeIndex]}'.");
            }

            Directory.CreateDirectory(directoryPath);
            using FileStream stream = File.Create(cachePath);
            global::helengine.editor.AssetSerializer.Serialize(stream, CreateTextureAsset(cubeIndex));
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
        /// Creates one file-backed material asset for the supplied cube.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>File-backed textured material asset.</returns>
        MaterialAsset CreateMaterialAsset(int cubeIndex) {
            return new MaterialAsset {
                Id = CreateMaterialAssetId(cubeIndex),
                ShaderAssetId = StandardShaderAssetId,
                VertexProgram = StandardVertexProgramName,
                PixelProgram = StandardPixelProgramName,
                Variant = MeshVariantName,
                DiffuseTextureAssetId = CubeTextureAssetIds[cubeIndex],
                RenderState = new MaterialRenderState(),
                ConstantBuffers = Array.Empty<MaterialConstantBufferAsset>(),
                CastsShadows = true,
                ReceivesShadows = true
            };
        }

        /// <summary>
        /// Creates one texture import-settings sidecar for the supplied generated texture source.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="textureBytes">Texture source bytes written to disk.</param>
        /// <returns>Generated import-settings payload for the texture source.</returns>
        AssetImportSettings CreateTextureImportSettings(int cubeIndex, byte[] textureBytes) {
            if (cubeIndex < 0 || cubeIndex >= CubeTextureAssetIds.Length) {
                throw new ArgumentOutOfRangeException(nameof(cubeIndex), "Cube index must address one generated texture.");
            } else if (textureBytes == null) {
                throw new ArgumentNullException(nameof(textureBytes));
            }

            AssetImportSettings settings = new AssetImportSettings();
            settings.Importer.ImporterId = TextureImporterId;
            settings.Importer.SourceChecksum = ComputeSourceChecksum(textureBytes);
            settings.Importer.AssetId = CubeTextureAssetIds[cubeIndex];
            return settings;
        }

        /// <summary>
        /// Creates one runtime <see cref="TextureAsset"/> that matches the generated diagnostic bitmap written for the supplied cube.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Runtime texture asset stored in the project cache.</returns>
        TextureAsset CreateTextureAsset(int cubeIndex) {
            return new TextureAsset {
                Width = TextureWidth,
                Height = TextureHeight,
                Colors = BuildTextureAssetColors(cubeIndex)
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
            windowsSettings.FieldValues[TextureIdFieldId] = CubeTextureAssetIds[cubeIndex];
            windowsSettings.FieldValues[CastsShadowFieldId] = "true";
            windowsSettings.FieldValues[ReceivesShadowFieldId] = "true";
            windowsSettings.FieldValues[BaseColorFieldId] = "#FFFFFFFF";
            settings.Processor.Platforms["windows"] = windowsSettings;

            MaterialAssetProcessorSettings ps2Settings = new MaterialAssetProcessorSettings();
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.FieldValues[TextureIdFieldId] = CubeTextureAssetIds[cubeIndex];
            ps2Settings.FieldValues[AlphaModeFieldId] = "opaque";
            ps2Settings.FieldValues[DoubleSidedFieldId] = "false";
            ps2Settings.FieldValues[Ps2CastShadowsFieldId] = "true";
            ps2Settings.FieldValues[VertexColorModeFieldId] = "ignore";
            ps2Settings.FieldValues[BaseColorFieldId] = "#FFFFFFFF";
            settings.Processor.Platforms["ps2"] = ps2Settings;

            MaterialAssetProcessorSettings pspSettings = new MaterialAssetProcessorSettings();
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.FieldValues[UseCustomShaderFieldId] = "false";
            pspSettings.FieldValues[TextureIdFieldId] = CubeTextureAssetIds[cubeIndex];
            pspSettings.FieldValues[CastsShadowFieldId] = "true";
            pspSettings.FieldValues[ReceivesShadowFieldId] = "true";
            pspSettings.FieldValues[BaseColorFieldId] = "#FFFFFFFF";
            settings.Processor.Platforms["psp"] = pspSettings;
            return settings;
        }

        /// <summary>
        /// Builds the stable per-cube material relative paths used by the textured cube-grid scene.
        /// </summary>
        /// <returns>Stable project-relative material paths.</returns>
        static string[] BuildMaterialRelativePaths() {
            string[] relativePaths = new string[16];
            for (int cubeIndex = 0; cubeIndex < relativePaths.Length; cubeIndex++) {
                relativePaths[cubeIndex] = MaterialRootRelativePath + "/Cube" + cubeIndex.ToString("00") + ".helmat";
            }

            return relativePaths;
        }

        /// <summary>
        /// Builds the stable per-cube texture relative paths used by the textured cube-grid scene.
        /// </summary>
        /// <returns>Stable project-relative texture paths.</returns>
        static string[] BuildTextureRelativePaths() {
            string[] relativePaths = new string[16];
            for (int cubeIndex = 0; cubeIndex < relativePaths.Length; cubeIndex++) {
                relativePaths[cubeIndex] = TextureRootRelativePath + "/Cube" + cubeIndex.ToString("00") + ".bmp";
            }

            return relativePaths;
        }

        /// <summary>
        /// Builds the stable imported texture asset identifiers derived from the generated texture bytes.
        /// </summary>
        /// <returns>Stable imported texture asset ids.</returns>
        static string[] BuildTextureAssetIds() {
            string[] assetIds = new string[16];
            for (int cubeIndex = 0; cubeIndex < assetIds.Length; cubeIndex++) {
                byte[] textureBytes = BuildTextureFileBytes(cubeIndex);
                assetIds[cubeIndex] = BuildImporterQualifiedAssetId(ComputeSourceChecksum(textureBytes), TextureImporterId);
            }

            return assetIds;
        }

        /// <summary>
        /// Builds one realistic bitmap texture for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>24-bit bitmap file bytes.</returns>
        static byte[] BuildTextureFileBytes(int cubeIndex) {
            RealisticTextureDefinition definition = TextureDefinitions[cubeIndex];
            int rowStride = ((TextureWidth * 3) + 3) & ~3;
            int pixelDataLength = rowStride * TextureHeight;
            int pixelDataOffset = 14 + 40;
            int fileLength = pixelDataOffset + pixelDataLength;
            byte[] fileBytes = new byte[fileLength];

            fileBytes[0] = (byte)'B';
            fileBytes[1] = (byte)'M';
            WriteInt32(fileBytes, 2, fileLength);
            WriteInt32(fileBytes, 10, pixelDataOffset);
            WriteInt32(fileBytes, 14, 40);
            WriteInt32(fileBytes, 18, TextureWidth);
            WriteInt32(fileBytes, 22, TextureHeight);
            WriteInt16(fileBytes, 26, 1);
            WriteInt16(fileBytes, 28, 24);
            WriteInt32(fileBytes, 34, pixelDataLength);

            for (int y = 0; y < TextureHeight; y++) {
                int rowOffset = pixelDataOffset + ((TextureHeight - 1 - y) * rowStride);
                for (int x = 0; x < TextureWidth; x++) {
                    byte[] pixelColor = ResolveSurfacePixelColor(cubeIndex, x, y, definition);
                    int pixelOffset = rowOffset + (x * 3);
                    fileBytes[pixelOffset + 0] = pixelColor[2];
                    fileBytes[pixelOffset + 1] = pixelColor[1];
                    fileBytes[pixelOffset + 2] = pixelColor[0];
                }
            }

            return fileBytes;
        }

        /// <summary>
        /// Builds the runtime RGBA pixel payload that matches the generated realistic bitmap for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Top-down row-major RGBA pixel bytes.</returns>
        static byte[] BuildTextureAssetColors(int cubeIndex) {
            RealisticTextureDefinition definition = TextureDefinitions[cubeIndex];
            byte[] colors = new byte[TextureWidth * TextureHeight * 4];

            for (int y = 0; y < TextureHeight; y++) {
                for (int x = 0; x < TextureWidth; x++) {
                    byte[] pixelColor = ResolveSurfacePixelColor(cubeIndex, x, y, definition);
                    int pixelOffset = ((y * TextureWidth) + x) * 4;
                    colors[pixelOffset] = pixelColor[0];
                    colors[pixelOffset + 1] = pixelColor[1];
                    colors[pixelOffset + 2] = pixelColor[2];
                    colors[pixelOffset + 3] = pixelColor[3];
                }
            }

            return colors;
        }

        /// <summary>
        /// Resolves the authored pixel color for one realistic texture coordinate.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate.</param>
        /// <param name="y">Pixel y coordinate.</param>
        /// <param name="definition">Surface definition used for the generated texture.</param>
        /// <returns>Resolved RGBA color bytes.</returns>
        static byte[] ResolveSurfacePixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
            if (definition.SurfaceKind == TextureSurfaceKind.Brick) {
                return ResolveBrickPixelColor(cubeIndex, x, y, definition);
            } else if (definition.SurfaceKind == TextureSurfaceKind.StoneBlock) {
                return ResolveStoneBlockPixelColor(cubeIndex, x, y, definition);
            } else if (definition.SurfaceKind == TextureSurfaceKind.Concrete) {
                return ResolveConcretePixelColor(cubeIndex, x, y, definition);
            }

            return ResolveTilePixelColor(cubeIndex, x, y, definition);
        }

        /// <summary>
        /// Resolves one brick-like surface pixel.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate.</param>
        /// <param name="y">Pixel y coordinate.</param>
        /// <param name="definition">Surface definition used for the generated texture.</param>
        /// <returns>Resolved RGBA color bytes.</returns>
        static byte[] ResolveBrickPixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
            byte[] mortarColor = LerpColor(ParseColor(definition.SecondaryColor), ParseColor(definition.AccentColor), 0.22d);
            byte[] brickBaseColor = ParseColor(definition.PrimaryColor);
            int rowIndex = y / definition.CellHeight;
            int rowOffset = (rowIndex & 1) == 0 ? 0 : definition.CellWidth / 2;
            int localX = (x + rowOffset) % definition.CellWidth;
            int localY = y % definition.CellHeight;
            bool mortarPixel = localX < definition.MortarThickness
                || localY < definition.MortarThickness
                || localX >= definition.CellWidth - definition.MortarThickness
                || localY >= definition.CellHeight - definition.MortarThickness;
            if (mortarPixel) {
                return ApplyWearAndNoise(mortarColor, cubeIndex, x, y, definition);
            }

            double edgeShade = ComputeNormalizedValue(localY, 0, definition.CellHeight - 1) * 0.12d;
            double jointShade = ComputeNormalizedValue(localX, 0, definition.CellWidth - 1) * 0.05d;
            byte[] litColor = LightenColor(brickBaseColor, 0.08d - edgeShade + jointShade);
            return ApplyWearAndNoise(litColor, cubeIndex, x, y, definition);
        }

        /// <summary>
        /// Resolves one stone-block surface pixel.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate.</param>
        /// <param name="y">Pixel y coordinate.</param>
        /// <param name="definition">Surface definition used for the generated texture.</param>
        /// <returns>Resolved RGBA color bytes.</returns>
        static byte[] ResolveStoneBlockPixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
            byte[] seamColor = LerpColor(ParseColor(definition.SecondaryColor), ParseColor(definition.AccentColor), 0.18d);
            byte[] blockColor = ParseColor(definition.PrimaryColor);
            int localX = x % definition.CellWidth;
            int localY = y % definition.CellHeight;
            bool seamPixel = localX < definition.MortarThickness
                || localY < definition.MortarThickness
                || localX >= definition.CellWidth - definition.MortarThickness
                || localY >= definition.CellHeight - definition.MortarThickness;
            if (seamPixel) {
                return ApplyWearAndNoise(seamColor, cubeIndex, x, y, definition);
            }

            double bevelX = Math.Min(localX, definition.CellWidth - 1 - localX) / Math.Max(1d, definition.CellWidth / 2d);
            double bevelY = Math.Min(localY, definition.CellHeight - 1 - localY) / Math.Max(1d, definition.CellHeight / 2d);
            double bevel = Math.Min(bevelX, bevelY);
            byte[] shadedColor = LightenColor(blockColor, (0.18d * bevel) - 0.08d);
            return ApplyWearAndNoise(shadedColor, cubeIndex, x, y, definition);
        }

        /// <summary>
        /// Resolves one concrete or plaster surface pixel.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate.</param>
        /// <param name="y">Pixel y coordinate.</param>
        /// <param name="definition">Surface definition used for the generated texture.</param>
        /// <returns>Resolved RGBA color bytes.</returns>
        static byte[] ResolveConcretePixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
            byte[] baseColor = ParseColor(definition.PrimaryColor);
            double streakNoise = ComputeDeterministicNoise(cubeIndex, x / 2, y, 7);
            double blotchNoise = ComputeDeterministicNoise(cubeIndex, x / 6, y / 6, 17);
            double lighteningAmount = ((streakNoise - 0.5d) * 0.14d) + ((blotchNoise - 0.5d) * 0.18d);
            byte[] shadedColor = LightenColor(baseColor, lighteningAmount);
            return ApplyWearAndNoise(shadedColor, cubeIndex, x, y, definition);
        }

        /// <summary>
        /// Resolves one tile or plaster-panel surface pixel.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate.</param>
        /// <param name="y">Pixel y coordinate.</param>
        /// <param name="definition">Surface definition used for the generated texture.</param>
        /// <returns>Resolved RGBA color bytes.</returns>
        static byte[] ResolveTilePixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
            byte[] seamColor = LerpColor(ParseColor(definition.SecondaryColor), ParseColor(definition.AccentColor), 0.30d);
            byte[] tileColor = ParseColor(definition.PrimaryColor);
            int localX = x % definition.CellWidth;
            int localY = y % definition.CellHeight;
            bool seamPixel = localX < definition.MortarThickness
                || localY < definition.MortarThickness
                || localX >= definition.CellWidth - definition.MortarThickness
                || localY >= definition.CellHeight - definition.MortarThickness;
            if (seamPixel) {
                return ApplyWearAndNoise(seamColor, cubeIndex, x, y, definition);
            }

            double highlight = ((ComputeNormalizedValue(localX, 0, definition.CellWidth - 1)
                + ComputeNormalizedValue(localY, 0, definition.CellHeight - 1)) * 0.08d) - 0.04d;
            byte[] shadedColor = LightenColor(tileColor, highlight);
            return ApplyWearAndNoise(shadedColor, cubeIndex, x, y, definition);
        }

        /// <summary>
        /// Applies deterministic wear and noise to one generated base pixel color.
        /// </summary>
        /// <param name="sourceColor">Base RGBA color bytes.</param>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate.</param>
        /// <param name="y">Pixel y coordinate.</param>
        /// <param name="definition">Surface definition used for the generated texture.</param>
        /// <returns>Final RGBA color bytes.</returns>
        static byte[] ApplyWearAndNoise(byte[] sourceColor, int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
            byte[] accentColor = ParseColor(definition.AccentColor);
            double fineNoise = ComputeDeterministicNoise(cubeIndex, x, y, 29) - 0.5d;
            double broadNoise = ComputeDeterministicNoise(cubeIndex, x / 4, y / 4, 53) - 0.5d;
            double wearMask = ComputeDeterministicNoise(cubeIndex, x / 8, y / 8, 71);
            byte[] mixedColor = LightenColor(sourceColor, (fineNoise * definition.NoiseStrength) + (broadNoise * definition.NoiseStrength * 0.6d));
            if (wearMask > (1d - definition.WearStrength)) {
                double wearAmount = (wearMask - (1d - definition.WearStrength)) / Math.Max(definition.WearStrength, 0.0001d);
                mixedColor = LerpColor(mixedColor, accentColor, wearAmount * 0.35d);
            }

            return mixedColor;
        }

        /// <summary>
        /// Computes one deterministic normalized noise value for the supplied coordinate and salt.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="x">Pixel x coordinate or derived sample coordinate.</param>
        /// <param name="y">Pixel y coordinate or derived sample coordinate.</param>
        /// <param name="salt">Stable salt used to decorrelate different noise layers.</param>
        /// <returns>Deterministic normalized noise value in the <c>[0, 1]</c> interval.</returns>
        static double ComputeDeterministicNoise(int cubeIndex, int x, int y, int salt) {
            int seed = cubeIndex + 1;
            int hashedValue = (x * 73856093) ^ (y * 19349663) ^ (seed * 83492791) ^ (salt * 265443576);
            uint normalizedBits = (uint)hashedValue;
            return (normalizedBits & 0xFFFF) / 65535d;
        }

        /// <summary>
        /// Computes one normalized value between zero and one for the supplied integer range.
        /// </summary>
        /// <param name="value">Source value to normalize.</param>
        /// <param name="min">Inclusive minimum value.</param>
        /// <param name="max">Inclusive maximum value.</param>
        /// <returns>Normalized double value in the <c>[0, 1]</c> interval.</returns>
        static double ComputeNormalizedValue(int value, int min, int max) {
            if (max <= min) {
                return 0d;
            }

            double normalizedValue = (value - min) / (double)(max - min);
            if (normalizedValue < 0d) {
                return 0d;
            } else if (normalizedValue > 1d) {
                return 1d;
            }

            return normalizedValue;
        }

        /// <summary>
        /// Blends two RGBA colors using the supplied interpolation amount.
        /// </summary>
        /// <param name="left">Left RGBA color bytes.</param>
        /// <param name="right">Right RGBA color bytes.</param>
        /// <param name="amount">Interpolation amount in the <c>[0, 1]</c> interval.</param>
        /// <returns>Blended RGBA color bytes.</returns>
        static byte[] LerpColor(byte[] left, byte[] right, double amount) {
            if (left == null) {
                throw new ArgumentNullException(nameof(left));
            } else if (right == null) {
                throw new ArgumentNullException(nameof(right));
            }

            double clampedAmount = amount;
            if (clampedAmount < 0d) {
                clampedAmount = 0d;
            } else if (clampedAmount > 1d) {
                clampedAmount = 1d;
            }

            return [
                (byte)(left[0] + ((right[0] - left[0]) * clampedAmount)),
                (byte)(left[1] + ((right[1] - left[1]) * clampedAmount)),
                (byte)(left[2] + ((right[2] - left[2]) * clampedAmount)),
                (byte)(left[3] + ((right[3] - left[3]) * clampedAmount))
            ];
        }

        /// <summary>
        /// Builds one lighter or darker RGBA color variant from the supplied source color.
        /// </summary>
        /// <param name="sourceColor">Source RGBA color bytes.</param>
        /// <param name="amount">Brightness delta applied to the RGB channels.</param>
        /// <returns>Adjusted RGBA color bytes.</returns>
        static byte[] LightenColor(byte[] sourceColor, double amount) {
            if (sourceColor == null) {
                throw new ArgumentNullException(nameof(sourceColor));
            }

            return [
                AdjustChannel(sourceColor[0], amount),
                AdjustChannel(sourceColor[1], amount),
                AdjustChannel(sourceColor[2], amount),
                sourceColor[3]
            ];
        }

        /// <summary>
        /// Parses one authored hex color string into RGBA byte channels.
        /// </summary>
        /// <param name="colorValue">Authored color string in <c>#RRGGBBAA</c> form.</param>
        /// <returns>RGBA byte channel array.</returns>
        static byte[] ParseColor(string colorValue) {
            if (string.IsNullOrWhiteSpace(colorValue)) {
                throw new ArgumentException("Color value must be provided.", nameof(colorValue));
            } else if (!colorValue.StartsWith('#') || colorValue.Length != 9) {
                throw new InvalidOperationException($"Color value '{colorValue}' must use #RRGGBBAA format.");
            }

            uint rgba = Convert.ToUInt32(colorValue.Substring(1, 8), 16);
            return [
                (byte)((rgba >> 24) & 0xFF),
                (byte)((rgba >> 16) & 0xFF),
                (byte)((rgba >> 8) & 0xFF),
                (byte)(rgba & 0xFF)
            ];
        }

        /// <summary>
        /// Adjusts one 8-bit channel by the supplied normalized brightness delta.
        /// </summary>
        /// <param name="value">Source channel value.</param>
        /// <param name="amount">Normalized brightness delta.</param>
        /// <returns>Adjusted 8-bit channel value.</returns>
        static byte AdjustChannel(byte value, double amount) {
            double adjustedValue = value + (255d * amount);
            if (adjustedValue < 0d) {
                return 0;
            } else if (adjustedValue > 255d) {
                return 255;
            }

            return (byte)adjustedValue;
        }

        /// <summary>
        /// Computes the stable lowercase SHA-256 checksum string stored in one generated texture sidecar.
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
        /// Creates one stable material asset id for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Material asset id stored inside the serialized file-backed asset.</returns>
        static string CreateMaterialAssetId(int cubeIndex) {
            return "Materials.rendering.textured_cube_grid.Cube" + cubeIndex.ToString("00");
        }

        /// <summary>
        /// Creates one stable cube entity id for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Stable scene entity id.</returns>
        static string CreateCubeEntityId(int cubeIndex) {
            return "textured-cube-grid-cube-" + cubeIndex.ToString("00");
        }

        /// <summary>
        /// Creates one stable cube entity name for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Stable scene entity name.</returns>
        static string CreateCubeEntityName(int cubeIndex) {
            return "TexturedCubeGridCube" + cubeIndex.ToString("00");
        }

    }
}
