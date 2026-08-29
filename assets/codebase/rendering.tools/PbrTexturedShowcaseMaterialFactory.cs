using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the two authored materials used by the PBR textured showcase: a metallic scuffed-metal prop and a non-metallic wood-plank prop.
    /// </summary>
    public sealed class PbrTexturedShowcaseMaterialFactory {
        /// <summary>
        /// Stable project-relative material path used by the scuffed-metal prop.
        /// </summary>
        public const string MetalMaterialRelativePath = "materials/rendering/pbr_textured_showcase/ScuffedMetal.hasset";

        /// <summary>
        /// Stable material asset identifier used by the scuffed-metal prop.
        /// </summary>
        public const string MetalMaterialAssetId = "Materials.rendering.pbr_textured_showcase.ScuffedMetal";

        /// <summary>
        /// Stable project-relative albedo source texture path used by the scuffed-metal prop.
        /// </summary>
        public const string MetalDiffuseTextureRelativePath = "textures/rendering/pbr_textured_showcase/Metal032Albedo.jpg";

        /// <summary>
        /// Stable project-relative roughness source texture path used by the scuffed-metal prop.
        /// </summary>
        public const string MetalRoughnessTextureRelativePath = "textures/rendering/pbr_textured_showcase/Metal032Roughness.jpg";

        /// <summary>
        /// Stable project-relative material path used by the wood-plank prop.
        /// </summary>
        public const string WoodMaterialRelativePath = "materials/rendering/pbr_textured_showcase/WoodPlanks.hasset";

        /// <summary>
        /// Stable material asset identifier used by the wood-plank prop.
        /// </summary>
        public const string WoodMaterialAssetId = "Materials.rendering.pbr_textured_showcase.WoodPlanks";

        /// <summary>
        /// Stable project-relative albedo source texture path used by the wood-plank prop.
        /// </summary>
        public const string WoodDiffuseTextureRelativePath = "textures/rendering/pbr_textured_showcase/WoodFloor041Albedo.jpg";

        /// <summary>
        /// Stable project-relative roughness source texture path used by the wood-plank prop.
        /// </summary>
        public const string WoodRoughnessTextureRelativePath = "textures/rendering/pbr_textured_showcase/WoodFloor041Roughness.jpg";

        const string WindowsMaterialSchemaId = "standard-shader";
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";
        const string GameCubeMaterialSchemaId = "gamecube-standard-textured";
        const string DsMaterialSchemaId = "ds-standard-textured";
        const string StandardShaderAssetId = "ForwardStandardShader";
        const string UseCustomShaderFieldId = "use-custom-shader";
        const string ShaderAssetIdFieldId = "shader-asset-id";
        const string TextureIdFieldId = "texture-id";
        const string RoughnessFieldId = "roughness";
        const string RoughnessTextureIdFieldId = "roughness-texture-id";
        const string MetallicFieldId = "metallic";
        const string SpecularFieldId = "specular";
        const string TextureRelativePathFieldId = "texture-relative-path";
        const string BaseColorFieldId = "base-color";
        const string CastsShadowFieldId = "casts-shadow";
        const string Ps2CastShadowsFieldId = "cast-shadows";
        const string ReceivesShadowFieldId = "receives-shadow";
        const string AlphaModeFieldId = "alpha-mode";
        const string DoubleSidedFieldId = "double-sided";
        const string VertexColorModeFieldId = "vertex-color-mode";
        const string LightingModeFieldId = "lighting-mode";

        /// <summary>
        /// Shared generated material writer used to persist the authored showcase material settings.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes one PBR textured showcase material factory.
        /// </summary>
        public PbrTexturedShowcaseMaterialFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            MaterialWriteService = new GeneratedMaterialAssetWriteService(assetAuthoringService);
        }

        /// <summary>
        /// Writes the authored scuffed-metal and wood-plank material settings required by the textured showcase scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAssets(string projectRootPath, IEditorProjectAuthoringSession assetAuthoringService) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string metalDiffuseTextureAssetId = ResolveTextureAssetId(projectRootPath, MetalDiffuseTextureRelativePath, assetAuthoringService);
            string metalRoughnessTextureAssetId = ResolveTextureAssetId(projectRootPath, MetalRoughnessTextureRelativePath, assetAuthoringService);
            MaterialWriteService.WriteMaterial(
                MetalMaterialRelativePath,
                CreateDefinition(MetalMaterialAssetId, metalDiffuseTextureAssetId, metalRoughnessTextureAssetId, metallic: "1.0"));

            string woodDiffuseTextureAssetId = ResolveTextureAssetId(projectRootPath, WoodDiffuseTextureRelativePath, assetAuthoringService);
            string woodRoughnessTextureAssetId = ResolveTextureAssetId(projectRootPath, WoodRoughnessTextureRelativePath, assetAuthoringService);
            MaterialWriteService.WriteMaterial(
                WoodMaterialRelativePath,
                CreateDefinition(WoodMaterialAssetId, woodDiffuseTextureAssetId, woodRoughnessTextureAssetId, metallic: "0.0"));
        }

        /// <summary>
        /// Resolves one imported texture asset id that should back an authored showcase material.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="relativeTexturePath">Project-relative source texture path.</param>
        /// <returns>Imported texture asset id persisted by the shared editor import pipeline.</returns>
        string ResolveTextureAssetId(string projectRootPath, string relativeTexturePath, IEditorProjectAuthoringSession assetAuthoringService) {
            if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string sourceTexturePath = Path.Combine(assetsRootPath, relativeTexturePath.Replace('/', Path.DirectorySeparatorChar));
            bool settingsFileExists = File.Exists(sourceTexturePath + ".hasset");
            TextureAssetImportSettings settings = assetAuthoringService.LoadOrCreateTextureImportSettings(sourceTexturePath);
            if (!settingsFileExists) {
                assetAuthoringService.SaveTextureImportSettings(sourceTexturePath, settings);
            }
            string assetId = settings.Importer.AssetId;
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new InvalidOperationException($"PBR textured showcase requires a persisted imported texture asset id for '{relativeTexturePath}'.");
            }

            return assetId;
        }

        /// <summary>
        /// Creates the generated authored material definition consumed by the shared material-settings writer.
        /// </summary>
        /// <param name="materialAssetId">Stable material asset id.</param>
        /// <param name="diffuseTextureAssetId">Imported albedo texture asset id.</param>
        /// <param name="roughnessTextureAssetId">Imported roughness texture asset id.</param>
        /// <param name="metallic">Authored metallic scalar text, either <c>"1.0"</c> or <c>"0.0"</c>.</param>
        /// <returns>Generated material definition populated for every supported platform.</returns>
        GeneratedMaterialAssetDefinition CreateDefinition(string materialAssetId, string diffuseTextureAssetId, string roughnessTextureAssetId, string metallic) {
            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = materialAssetId,
                DiffuseTextureAssetId = diffuseTextureAssetId,
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"), diffuseTextureAssetId, roughnessTextureAssetId, metallic);
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), diffuseTextureAssetId, roughnessTextureAssetId, metallic);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"), diffuseTextureAssetId);
            ConfigureGameCubePlatform(definition.GetOrCreatePlatform("gamecube"), diffuseTextureAssetId);
            ConfigureDsPlatform(definition.GetOrCreatePlatform("ds"), diffuseTextureAssetId);
            return definition;
        }

        /// <summary>
        /// Populates the shared Windows and PSP preview material settings.
        /// </summary>
        void ConfigureWindowsPlatform(GeneratedMaterialPlatformDefinition platformDefinition, string diffuseTextureAssetId, string roughnessTextureAssetId, string metallic) {
            platformDefinition.SchemaId = WindowsMaterialSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(TextureIdFieldId, diffuseTextureAssetId);
            platformDefinition.SetFieldValue(RoughnessFieldId, "1.0");
            platformDefinition.SetFieldValue(MetallicFieldId, metallic);
            platformDefinition.SetFieldValue(SpecularFieldId, "0.5");
            platformDefinition.SetFieldValue(RoughnessTextureIdFieldId, roughnessTextureAssetId);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
        }

        /// <summary>
        /// Populates the PS2 textured material settings.
        /// </summary>
        void ConfigurePs2Platform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            platformDefinition.SchemaId = Ps2MaterialSchemaId;
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(TextureRelativePathFieldId, "cooked/imported/" + textureAssetId);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(Ps2CastShadowsFieldId, "true");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
        }

        /// <summary>
        /// Populates the GameCube textured material settings.
        /// </summary>
        void ConfigureGameCubePlatform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            platformDefinition.SchemaId = GameCubeMaterialSchemaId;
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(TextureRelativePathFieldId, "cooked/imported/" + textureAssetId);
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        /// <summary>
        /// Populates the Nintendo DS textured material settings.
        /// </summary>
        void ConfigureDsPlatform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            platformDefinition.SchemaId = DsMaterialSchemaId;
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(TextureRelativePathFieldId, "cooked/imported/" + textureAssetId);
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

    }
}
