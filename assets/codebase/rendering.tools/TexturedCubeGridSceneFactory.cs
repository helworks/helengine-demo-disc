using city.menu;
using helengine;

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
        /// Stable GameCube textured material schema identifier used by the GX runtime path.
        /// </summary>
        const string GameCubeMaterialSchemaId = "gamecube-standard-textured";

        /// <summary>
        /// Stable Nintendo DS textured material schema identifier used by the DS runtime path.
        /// </summary>
        const string DsMaterialSchemaId = "ds-standard-textured";

        /// <summary>
        /// Stable standard shader asset identifier used by compatibility material payloads.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable standard shader source file used by generated textured cube-grid runtime materials.
        /// </summary>
        const string StandardShaderSourceFileName = "ForwardStandardShader.hlsl";

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
        /// Stable DS material field identifier used for cooked imported texture paths.
        /// </summary>
        const string DsTextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Stable GameCube material field identifier used for cooked imported texture paths.
        /// </summary>
        const string GameCubeTextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Stable DS material field identifier used to select fixed-pipeline lighting behavior.
        /// </summary>
        const string LightingModeFieldId = "lighting-mode";

        /// <summary>
        /// Stable material field identifier used for compatibility shader asset references.
        /// </summary>
        const string ShaderAssetIdFieldId = "shader-asset-id";

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
        /// Stable minimum angular speed applied to rotating cubes in degrees per second.
        /// </summary>
        const double CubeBaseAngularSpeedDegreesPerSecond = 48.0;

        /// <summary>
        /// Stable per-cube angular speed increase in degrees per second.
        /// </summary>
        const double CubeAngularSpeedDegreesPerIndex = 4.0;

        /// <summary>
        /// Service used to persist generated authored material assets plus their per-platform material settings.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes the textured cube-grid scene factory with the descriptors and services required for authored output.
        /// </summary>
        public TexturedCubeGridSceneFactory() {
            MaterialWriteService = new GeneratedMaterialAssetWriteService();
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
        /// Creates the canonical textured cube-grid live-authored scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to every cube.</param>
        /// <param name="texturedMaterials">Generated runtime materials assigned to the sixteen cubes.</param>
        /// <returns>Live-authored scene definition for the sixteen-cube textured grid.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeMaterial[] texturedMaterials) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (texturedMaterials == null) {
                throw new ArgumentNullException(nameof(texturedMaterials));
            } else if (texturedMaterials.Length != CubeMaterialRelativePaths.Length) {
                throw new ArgumentException("Textured cube-grid generation requires sixteen runtime materials.", nameof(texturedMaterials));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
            Entity[] cubeEntities = CreateCubeEntities(cubeModel, texturedMaterials);
            Entity[] rootEntities = new Entity[cubeEntities.Length + 4];
            Entity cameraEntity = CreateCameraEntity();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(instructionFont);
            rootEntities[0] = cameraEntity;
            rootEntities[1] = instructionOverlayEntity;
            rootEntities[2] = CreateUiEntity();
            rootEntities[3] = CreateDirectionalLightEntity();
            Array.Copy(cubeEntities, 0, rootEntities, 4, cubeEntities.Length);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    SceneId = RenderingSceneGenerator.TexturedCubeGridNintendoDsSceneId,
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = instructionOverlayFactory.CreateNintendoDsBottomInstructionRoots(instructionFont)
                },
                RootEntities = rootEntities
            };
        }

        /// <summary>
        /// Creates the runtime materials used while authoring the textured cube-grid scene.
        /// </summary>
        /// <param name="standardMaterial">Generated standard runtime material used as the parent for the textured material instances.</param>
        /// <returns>Runtime materials ordered to match the generated cube indices.</returns>
        public RuntimeMaterial[] CreateRuntimeMaterials(RuntimeMaterial standardMaterial) {
            if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            }

            RuntimeMaterial[] materials = new RuntimeMaterial[CubeMaterialRelativePaths.Length];
            for (int cubeIndex = 0; cubeIndex < materials.Length; cubeIndex++) {
                materials[cubeIndex] = CreateRuntimeMaterial(cubeIndex, standardMaterial);
            }

            return materials;
        }

        /// <summary>
        /// Creates the authored camera entity for the textured cube-grid scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TexturedCubeGridCamera");
            entity.LocalPosition = new float3(0f, 0f, 18f);
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
                AutoYawSpeedRadians = 0.09f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the textured cube-grid scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TexturedCubeGridUi");
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            entity.AddComponent(new DemoDiscLightToggleComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored directional light entity for the textured cube-grid scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            Entity entity = Core.Instance.EntityFactory.Create("TexturedCubeGridSun");
            entity.LocalPosition = new float3(0f, 6f, 0f);
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1.35f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 40f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored rotating cube entities for the textured cube-grid scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to every cube.</param>
        /// <param name="texturedMaterials">Generated runtime materials assigned to the sixteen cubes.</param>
        /// <returns>Live authored cube entities ordered by row-major grid position.</returns>
        Entity[] CreateCubeEntities(RuntimeModel cubeModel, RuntimeMaterial[] texturedMaterials) {
            Entity[] cubeEntities = new Entity[texturedMaterials.Length];
            for (int row = 0; row < 4; row++) {
                for (int column = 0; column < 4; column++) {
                    int cubeIndex = (row * 4) + column;
                    cubeEntities[cubeIndex] = CreateCubeEntity(
                        cubeIndex,
                        row,
                        column,
                        cubeModel,
                        texturedMaterials[cubeIndex],
                        new float3((column - 1.5f) * 3.0f, (1.5f - row) * 3.0f, 0f));
                }
            }

            return cubeEntities;
        }

        /// <summary>
        /// Creates one rotating cube entity for the textured cube-grid scene.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="cubeModel">Generated cube runtime model.</param>
        /// <param name="material">Generated runtime material assigned to the cube.</param>
        /// <param name="localPosition">Authored local position for the cube.</param>
        /// <returns>Live authored cube entity.</returns>
        Entity CreateCubeEntity(
            int cubeIndex,
            int row,
            int column,
            RuntimeModel cubeModel,
            RuntimeMaterial material,
            float3 localPosition) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(CreateCubeEntityName(cubeIndex));
            entity.LocalPosition = localPosition;
            entity.LocalScale = new float3(1.5f, 1.5f, 1.5f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Material = material,
                RenderOrder3D = 0
            });
            entity.AddComponent(new gameplay.rendering.AxisRotationComponent {
                Axis = new float3(0f, 1f, 0f),
                AngularSpeedRadiansPerSecond = GetCubeAngularSpeedRadiansPerSecond(cubeIndex, row, column)
            });
            return entity;
        }

        /// <summary>
        /// Returns the authored angular speed magnitude for one cube.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Unsigned angular speed in radians per second.</returns>
        float GetCubeAngularSpeedMagnitudeRadiansPerSecond(int cubeIndex) {
            double angularSpeedDegreesPerSecond = CubeBaseAngularSpeedDegreesPerSecond + (CubeAngularSpeedDegreesPerIndex * cubeIndex);
            return (float)(angularSpeedDegreesPerSecond * (Math.PI / 180.0));
        }

        /// <summary>
        /// Returns the authored angular speed for one cube, alternating direction across the grid.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="row">Stable zero-based row index.</param>
        /// <param name="column">Stable zero-based column index.</param>
        /// <returns>Signed angular speed in radians per second.</returns>
        float GetCubeAngularSpeedRadiansPerSecond(int cubeIndex, int row, int column) {
            float angularSpeedRadiansPerSecond = GetCubeAngularSpeedMagnitudeRadiansPerSecond(cubeIndex);
            return ((row + column) & 1) == 0
                ? angularSpeedRadiansPerSecond
                : -angularSpeedRadiansPerSecond;
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
            MaterialWriteService.WriteMaterial(projectRootPath, relativePath, CreateGeneratedMaterialDefinition(cubeIndex));
        }

        /// <summary>
        /// Creates one file-backed material asset for the supplied cube.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>File-backed textured material asset.</returns>
        ShaderMaterialAsset CreateAuthoredMaterialAsset(int cubeIndex) {
            return new ShaderMaterialAsset {
                Id = CreateMaterialAssetId(cubeIndex),
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };
        }

        /// <summary>
        /// Creates one shader-backed preview material asset for the supplied cube.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Shader-backed preview material asset for the supplied cube.</returns>
        ShaderMaterialAsset CreatePreviewMaterialAsset(int cubeIndex) {
            return new ShaderMaterialAsset {
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
        /// Creates one generated authored material definition for the supplied cube material.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Generated authored material definition for the cube material.</returns>
        GeneratedMaterialAssetDefinition CreateGeneratedMaterialDefinition(int cubeIndex) {
            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = CreateAuthoredMaterialAsset(cubeIndex);

            GeneratedMaterialPlatformDefinition windowsSettings = definition.GetOrCreatePlatform("windows");
            windowsSettings.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            windowsSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            windowsSettings.SetFieldValue(TextureIdFieldId, CubeTextureAssetIds[cubeIndex]);
            windowsSettings.SetFieldValue(CastsShadowFieldId, "true");
            windowsSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            windowsSettings.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");

            GeneratedMaterialPlatformDefinition ps2Settings = definition.GetOrCreatePlatform("ps2");
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.SetFieldValue(TextureIdFieldId, CubeTextureAssetIds[cubeIndex]);
            ps2Settings.SetFieldValue(AlphaModeFieldId, "opaque");
            ps2Settings.SetFieldValue(DoubleSidedFieldId, "false");
            ps2Settings.SetFieldValue(Ps2CastShadowsFieldId, "true");
            ps2Settings.SetFieldValue(VertexColorModeFieldId, "ignore");
            ps2Settings.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");

            GeneratedMaterialPlatformDefinition pspSettings = definition.GetOrCreatePlatform("psp");
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            pspSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            pspSettings.SetFieldValue(TextureIdFieldId, CubeTextureAssetIds[cubeIndex]);
            pspSettings.SetFieldValue(CastsShadowFieldId, "true");
            pspSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            pspSettings.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");

            GeneratedMaterialPlatformDefinition gameCubeSettings = definition.GetOrCreatePlatform("gamecube");
            gameCubeSettings.SchemaId = GameCubeMaterialSchemaId;
            gameCubeSettings.SetFieldValue(TextureIdFieldId, CubeTextureAssetIds[cubeIndex]);
            gameCubeSettings.SetFieldValue(GameCubeTextureRelativePathFieldId, "cooked/imported/" + CubeTextureAssetIds[cubeIndex]);
            gameCubeSettings.SetFieldValue(DoubleSidedFieldId, "false");
            gameCubeSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            gameCubeSettings.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
            gameCubeSettings.SetFieldValue(LightingModeFieldId, "lit");

            GeneratedMaterialPlatformDefinition dsSettings = definition.GetOrCreatePlatform("ds");
            dsSettings.SchemaId = DsMaterialSchemaId;
            dsSettings.SetFieldValue(TextureIdFieldId, CubeTextureAssetIds[cubeIndex]);
            dsSettings.SetFieldValue(DsTextureRelativePathFieldId, "cooked/imported/" + CubeTextureAssetIds[cubeIndex]);
            dsSettings.SetFieldValue(DoubleSidedFieldId, "false");
            dsSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            dsSettings.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
            dsSettings.SetFieldValue(LightingModeFieldId, "lit");
            return definition;
        }

        /// <summary>
        /// Creates one runtime material for the supplied cube index by parenting a generated standard material and binding the generated texture.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="standardMaterial">Generated standard runtime material used as the shared parent.</param>
        /// <returns>Runtime material instance for the supplied cube.</returns>
        RuntimeMaterial CreateRuntimeMaterial(int cubeIndex, RuntimeMaterial standardMaterial) {
            if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            }

            ShaderMaterialAsset materialAsset = CreatePreviewMaterialAsset(cubeIndex);
            ShaderAsset shaderAsset = helengine.editor.EditorBuiltInShaderAssetLibrary.LoadShaderAsset(Core.Instance.RenderManager3D, StandardShaderSourceFileName);
            RuntimeMaterial runtimeMaterial = Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
            ShaderRuntimeMaterial shaderRuntimeMaterial = ShaderRuntimeMaterialAccess.Require(runtimeMaterial);

            int diffuseTextureBindingIndex = shaderRuntimeMaterial.Layout.FindTextureBindingIndex(StandardMaterialTextureBindingDefaults.DiffuseTextureBindingName);
            if (diffuseTextureBindingIndex < 0) {
                throw new InvalidOperationException("The generated standard material must expose a diffuse texture binding.");
            }

            RuntimeTexture runtimeTexture = Core.Instance.RenderManager2D.BuildTextureFromRaw(CreateTextureAsset(cubeIndex));
            shaderRuntimeMaterial.Properties.SetTexture(diffuseTextureBindingIndex, runtimeTexture);
            StandardMaterialTextureBindingDefaults.Apply(shaderRuntimeMaterial);
            return runtimeMaterial;
        }

        /// <summary>
        /// Resolves the editor font that should back the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Editor font asset required by the FPS component.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the textured cube-grid scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Builds the stable per-cube material relative paths used by the textured cube-grid scene.
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
        /// Creates one stable cube entity name for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Stable scene entity name.</returns>
        static string CreateCubeEntityName(int cubeIndex) {
            return "TexturedCubeGridCube" + cubeIndex.ToString("00");
        }

    }
}


