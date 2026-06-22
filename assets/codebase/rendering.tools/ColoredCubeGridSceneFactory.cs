using city.menu;
using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored colored cube-grid scene and its generated material assets.
    /// </summary>
    public sealed class ColoredCubeGridSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated colored cube-grid asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.ColoredCubeGridSceneId;

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
        /// Stable standard shader source file used by generated colored cube-grid runtime materials.
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
        /// Relative project folder used for the generated colored cube-grid materials.
        /// </summary>
        const string MaterialRootRelativePath = "Materials/rendering/colored_cube_grid";

        /// <summary>
        /// Stable authored colors assigned to the sixteen cube materials.
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
        /// Initializes the colored cube-grid scene factory with the services required for authored output.
        /// </summary>
        public ColoredCubeGridSceneFactory() {
            MaterialWriteService = new GeneratedMaterialAssetWriteService();
        }

        /// <summary>
        /// Creates the canonical colored cube-grid live-authored scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to every cube.</param>
        /// <param name="coloredMaterials">Generated runtime materials assigned to the sixteen cubes.</param>
        /// <returns>Live-authored scene definition for the sixteen-cube color grid.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeMaterial[] coloredMaterials) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (coloredMaterials == null) {
                throw new ArgumentNullException(nameof(coloredMaterials));
            } else if (coloredMaterials.Length != CubeMaterialRelativePaths.Length) {
                throw new ArgumentException("Colored cube-grid generation requires sixteen runtime materials.", nameof(coloredMaterials));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
            Entity[] cubeEntities = CreateCubeEntities(cubeModel, coloredMaterials);
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
                    SceneId = RenderingSceneGenerator.ColoredCubeGridNintendoDsSceneId,
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = instructionOverlayFactory.CreateNintendoDsBottomInstructionRoots(instructionFont)
                },
                RootEntities = rootEntities
            };
        }

        /// <summary>
        /// Creates the runtime materials used while authoring the colored cube-grid scene.
        /// </summary>
        /// <returns>Runtime materials ordered to match the generated cube indices.</returns>
        public RuntimeMaterial[] CreateRuntimeMaterials() {
            RuntimeMaterial[] materials = new RuntimeMaterial[CubeMaterialRelativePaths.Length];
            for (int cubeIndex = 0; cubeIndex < materials.Length; cubeIndex++) {
                materials[cubeIndex] = CreateRuntimeMaterial(cubeIndex);
            }

            return materials;
        }

        /// <summary>
        /// Writes the sixteen file-backed material assets and settings documents used by the colored cube-grid scene.
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
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("ColoredCubeGridCamera");
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
        /// Creates the authored UI root entity for the colored cube-grid scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("ColoredCubeGridUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            entity.AddComponent(new DemoDiscLightToggleComponent());
            DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory();
            lightIndicatorOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont());
            return entity;
        }

        /// <summary>
        /// Creates the authored directional light entity for the colored cube-grid scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("ColoredCubeGridSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
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
        /// Creates the authored rotating cube entities for the colored cube-grid scene.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to every cube.</param>
        /// <param name="coloredMaterials">Generated runtime materials assigned to the sixteen cubes.</param>
        /// <returns>Live authored cube entities ordered by row-major grid position.</returns>
        Entity[] CreateCubeEntities(RuntimeModel cubeModel, RuntimeMaterial[] coloredMaterials) {
            Entity[] cubeEntities = new Entity[coloredMaterials.Length];
            for (int row = 0; row < 4; row++) {
                for (int column = 0; column < 4; column++) {
                    int cubeIndex = (row * 4) + column;
                    cubeEntities[cubeIndex] = CreateCubeEntity(
                        cubeIndex,
                        row,
                        column,
                        cubeModel,
                        coloredMaterials[cubeIndex],
                        new float3((column - 1.5f) * 3.0f, (1.5f - row) * 3.0f, 0f));
                }
            }

            return cubeEntities;
        }

        /// <summary>
        /// Creates one rotating cube entity for the colored cube-grid scene.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <param name="cubeModel">Generated cube runtime model.</param>
        /// <param name="material">Generated runtime material assigned to the cube.</param>
        /// <param name="localPosition">Authored local position for the cube.</param>
        /// <returns>Live authored cube entity.</returns>
        Entity CreateCubeEntity(int cubeIndex, int row, int column, RuntimeModel cubeModel, RuntimeMaterial material, float3 localPosition) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(CreateCubeEntityName(cubeIndex));
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = localPosition;
            entity.LocalScale = new float3(1.5f, 1.5f, 1.5f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Materials = new[] { material },
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
        /// Creates one runtime material for the supplied cube index using the generated standard shader path.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Runtime material instance for the supplied cube.</returns>
        RuntimeMaterial CreateRuntimeMaterial(int cubeIndex) {
            ShaderMaterialAsset materialAsset = CreatePreviewMaterialAsset(cubeIndex);
            ShaderAsset shaderAsset = helengine.editor.EditorBuiltInShaderAssetLibrary.LoadShaderAsset(Core.Instance.RenderManager3D, StandardShaderSourceFileName);
            materialAsset.ConstantBuffers = new[] {
                new MaterialConstantBufferAsset {
                    Name = helengine.editor.StandardMaterialBaseColorDefaults.BaseColorBufferName,
                    Data = helengine.editor.StandardMaterialBaseColorDefaults.CreateConstantBufferData(ParseColor(CubeMaterialColors[cubeIndex]))
                }
            };

            RuntimeMaterial runtimeMaterial = Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(runtimeMaterial));
            return runtimeMaterial;
        }

        /// <summary>
        /// Writes one file-backed material asset and its settings document for the supplied cube index.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        void WriteMaterialAsset(string projectRootPath, int cubeIndex) {
            string relativePath = CubeMaterialRelativePaths[cubeIndex];
            MaterialWriteService.WriteMaterial(projectRootPath, relativePath, CreateGeneratedMaterialDefinition(cubeIndex));
        }

        /// <summary>
        /// Creates one compatibility material asset that resolves to the shared standard shader.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>File-backed material asset for the supplied cube.</returns>
        ShaderMaterialAsset CreateAuthoredMaterialAsset(int cubeIndex) {
            return new ShaderMaterialAsset {
                Id = CreateMaterialAssetId(cubeIndex),
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };
        }

        /// <summary>
        /// Creates one shader-backed preview material asset that resolves to the shared standard shader.
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
                RenderState = new MaterialRenderState(),
                ConstantBuffers = Array.Empty<MaterialConstantBufferAsset>(),
                CastsShadows = true,
                ReceivesShadows = true
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
            windowsSettings.SetFieldValue(TextureIdFieldId, string.Empty);
            windowsSettings.SetFieldValue(CastsShadowFieldId, "true");
            windowsSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            windowsSettings.SetFieldValue(BaseColorFieldId, CubeMaterialColors[cubeIndex]);

            GeneratedMaterialPlatformDefinition ps2Settings = definition.GetOrCreatePlatform("ps2");
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.SetFieldValue(AlphaModeFieldId, "opaque");
            ps2Settings.SetFieldValue(DoubleSidedFieldId, "false");
            ps2Settings.SetFieldValue(Ps2CastShadowsFieldId, "true");
            ps2Settings.SetFieldValue(VertexColorModeFieldId, "ignore");
            ps2Settings.SetFieldValue(BaseColorFieldId, CubeMaterialColors[cubeIndex]);

            GeneratedMaterialPlatformDefinition pspSettings = definition.GetOrCreatePlatform("psp");
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            pspSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            pspSettings.SetFieldValue(TextureIdFieldId, string.Empty);
            pspSettings.SetFieldValue(CastsShadowFieldId, "true");
            pspSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            pspSettings.SetFieldValue(BaseColorFieldId, CubeMaterialColors[cubeIndex]);
            return definition;
        }

        /// <summary>
        /// Resolves the editor font that should back the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Editor font asset required by the FPS component.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the colored cube-grid scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
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
        /// Creates one stable cube entity name for the supplied cube index.
        /// </summary>
        /// <param name="cubeIndex">Stable zero-based cube index.</param>
        /// <returns>Stable scene entity name.</returns>
        static string CreateCubeEntityName(int cubeIndex) {
            return "ColoredCubeGridCube" + cubeIndex.ToString("00");
        }

        /// <summary>
        /// Parses one authored hex color string into a normalized float4 color.
        /// </summary>
        /// <param name="colorValue">Authored color string in <c>#RRGGBBAA</c> form.</param>
        /// <returns>Normalized float4 color.</returns>
        static float4 ParseColor(string colorValue) {
            if (string.IsNullOrWhiteSpace(colorValue)) {
                throw new ArgumentException("Color value must be provided.", nameof(colorValue));
            } else if (!colorValue.StartsWith('#') || colorValue.Length != 9) {
                throw new InvalidOperationException($"Color value '{colorValue}' must use #RRGGBBAA format.");
            }

            uint rgba = Convert.ToUInt32(colorValue.Substring(1, 8), 16);
            return new float4(
                ((rgba >> 24) & 0xFF) / 255f,
                ((rgba >> 16) & 0xFF) / 255f,
                ((rgba >> 8) & 0xFF) / 255f,
                (rgba & 0xFF) / 255f);
        }
    }
}


