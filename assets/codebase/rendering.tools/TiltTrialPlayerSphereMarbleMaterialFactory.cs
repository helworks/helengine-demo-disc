using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the authored marble material assigned to the Tilt Trial player sphere.
    /// </summary>
    public sealed class TiltTrialPlayerSphereMarbleMaterialFactory {
        /// <summary>
        /// Stable project-relative material path used by the Tilt Trial player sphere.
        /// </summary>
        public const string MaterialRelativePath = "materials/rendering/tilt_trial/PlayerSphereMarble.hasset";

        /// <summary>
        /// Stable material asset identifier used by the Tilt Trial player sphere.
        /// </summary>
        public const string MaterialAssetId = "Materials.rendering.tilt_trial.PlayerSphereMarble";

        /// <summary>
        /// Stable project-relative marble albedo source texture path.
        /// </summary>
        public const string DiffuseTextureRelativePath = "textures/rendering/tilt_trial/PlayerSphereMarble.jpg";

        /// <summary>
        /// Stable project-relative marble roughness source texture path.
        /// </summary>
        public const string RoughnessTextureRelativePath = "textures/rendering/tilt_trial/PlayerSphereMarbleRoughness.jpg";

        /// <summary>
        /// Stable Windows and PSP schema identifier used by the standard shader material path.
        /// </summary>
        const string WindowsMaterialSchemaId = "standard-shader";

        /// <summary>
        /// Stable PS2 textured material schema identifier.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable GameCube textured material schema identifier.
        /// </summary>
        const string GameCubeMaterialSchemaId = "gamecube-standard-textured";

        /// <summary>
        /// Stable Nintendo DS untextured material schema identifier.
        /// </summary>
        const string DsMaterialSchemaId = "ds-standard-lit";

        /// <summary>
        /// Stable built-in forward shader asset id used by the editor preview material path.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable generated material field id used to opt into the standard shader defaults.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

        /// <summary>
        /// Stable generated material field id used to store the shared shader asset id.
        /// </summary>
        const string ShaderAssetIdFieldId = "shader-asset-id";

        /// <summary>
        /// Stable generated material field id used to store imported texture bindings.
        /// </summary>
        const string TextureIdFieldId = "texture-id";

        /// <summary>
        /// Stable generated material field id used to store the authored roughness scalar.
        /// </summary>
        const string RoughnessFieldId = "roughness";

        /// <summary>
        /// Stable generated material field id used to store the authored roughness texture binding.
        /// </summary>
        const string RoughnessTextureIdFieldId = "roughness-texture-id";

        /// <summary>
        /// Stable generated material field id used to store the authored metallic scalar.
        /// </summary>
        const string MetallicFieldId = "metallic";

        /// <summary>
        /// Stable generated material field id used to store the authored specular scalar.
        /// </summary>
        const string SpecularFieldId = "specular";

        /// <summary>
        /// Stable generated material field id used to store cooked imported texture paths.
        /// </summary>
        const string TextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Stable generated material field id used to store the authored base color.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Stable generated material field id used to store the authored cast-shadows toggle.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// Stable PS2 generated material field id used to store cast-shadows participation.
        /// </summary>
        const string Ps2CastShadowsFieldId = "cast-shadows";

        /// <summary>
        /// Stable generated material field id used to store the authored receive-shadows toggle.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Stable generated material field id used to store the authored alpha mode.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Stable generated material field id used to store the authored double-sided toggle.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Stable generated material field id used to store the authored vertex-color mode.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Stable generated material field id used to store the fixed-pipeline lighting mode.
        /// </summary>
        const string LightingModeFieldId = "lighting-mode";

        /// <summary>
        /// Shared generated material writer used to persist the authored marble material settings.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Initializes one marble material factory.
        /// </summary>
        public TiltTrialPlayerSphereMarbleMaterialFactory(IEditorProjectAuthoringSession assetAuthoringService, EditorAuthoringTransaction transaction) {
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            MaterialWriteService = new GeneratedMaterialAssetWriteService(assetAuthoringService, transaction);
        }

        /// <summary>
        /// Writes the authored marble material settings required by the Tilt Trial player sphere.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAsset(string projectRootPath, IEditorProjectAuthoringSession assetAuthoringService) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            (string diffuseTextureAssetId, string roughnessTextureAssetId) = ResolveTextureAssetIds(projectRootPath, assetAuthoringService);
            MaterialWriteService.WriteMaterial(
                MaterialRelativePath,
                CreateDefinition(diffuseTextureAssetId, roughnessTextureAssetId));
        }

        /// <summary>
        /// Resolves the imported texture asset ids that should back the marble material settings.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Imported diffuse and roughness texture asset ids persisted by the shared editor import pipeline.</returns>
        (string DiffuseTextureAssetId, string RoughnessTextureAssetId) ResolveTextureAssetIds(string projectRootPath, IEditorProjectAuthoringSession assetAuthoringService) {
            if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string diffuseTextureAssetId = ResolveTextureAssetId(assetAuthoringService, assetsRootPath, DiffuseTextureRelativePath);
            string roughnessTextureAssetId = ResolveTextureAssetId(assetAuthoringService, assetsRootPath, RoughnessTextureRelativePath);
            return (diffuseTextureAssetId, roughnessTextureAssetId);
        }

        /// <summary>
        /// Resolves one imported texture asset id that should back the authored marble material settings.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned asset-authoring capability.</param>
        /// <param name="assetsRootPath">Absolute assets root path.</param>
        /// <param name="relativeTexturePath">Project-relative source texture path.</param>
        /// <returns>Imported texture asset id persisted by the shared editor import pipeline.</returns>
        string ResolveTextureAssetId(IEditorProjectAuthoringSession assetAuthoringService, string assetsRootPath, string relativeTexturePath) {
            if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            } else if (string.IsNullOrWhiteSpace(assetsRootPath)) {
                throw new ArgumentException("Assets root path must be provided.", nameof(assetsRootPath));
            } else if (string.IsNullOrWhiteSpace(relativeTexturePath)) {
                throw new ArgumentException("Relative texture path must be provided.", nameof(relativeTexturePath));
            }

            string sourceTexturePath = Path.Combine(assetsRootPath, relativeTexturePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(sourceTexturePath)) {
                throw new FileNotFoundException("Tilt Trial marble source texture was not found.", sourceTexturePath);
            }
            TextureAssetImportSettings settingsIntent = new TextureAssetImportSettings();
            settingsIntent.Importer.ImporterId = "gdi";
            TextureAssetImportSettings settings = assetAuthoringService.WriteGeneratedTexture(
                relativeTexturePath,
                File.ReadAllBytes(sourceTexturePath),
                settingsIntent,
                Transaction);
            string assetId = settings.Importer.AssetId;
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new InvalidOperationException($"Tilt Trial marble material requires a persisted imported texture asset id for '{relativeTexturePath}'.");
            }

            return assetId;
        }

        /// <summary>
        /// Creates the generated authored material definition consumed by the shared material-settings writer.
        /// </summary>
        /// <param name="diffuseTextureAssetId">Imported marble albedo texture asset id.</param>
        /// <param name="roughnessTextureAssetId">Imported marble roughness texture asset id.</param>
        /// <returns>Generated material definition populated for every supported preview and runtime platform.</returns>
        GeneratedMaterialAssetDefinition CreateDefinition(string diffuseTextureAssetId, string roughnessTextureAssetId) {
            if (string.IsNullOrWhiteSpace(diffuseTextureAssetId)) {
                throw new ArgumentException("Diffuse texture asset id must be provided.", nameof(diffuseTextureAssetId));
            } else if (string.IsNullOrWhiteSpace(roughnessTextureAssetId)) {
                throw new ArgumentException("Roughness texture asset id must be provided.", nameof(roughnessTextureAssetId));
            }

            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = MaterialAssetId,
                DiffuseTextureAssetId = diffuseTextureAssetId,
                RenderState = new MaterialRenderState {
                    CullMode = MaterialCullMode.None
                },
                CastsShadows = true,
                ReceivesShadows = true
            };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"), diffuseTextureAssetId, roughnessTextureAssetId);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"), diffuseTextureAssetId);
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), diffuseTextureAssetId, roughnessTextureAssetId);
            ConfigureGameCubePlatform(definition.GetOrCreatePlatform("gamecube"), diffuseTextureAssetId);
            ConfigureDsPlatform(definition.GetOrCreatePlatform("ds"));
            return definition;
        }

        /// <summary>
        /// Populates the shared Windows and PSP preview material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="diffuseTextureAssetId">Imported marble albedo texture asset id.</param>
        /// <param name="roughnessTextureAssetId">Imported marble roughness texture asset id.</param>
        void ConfigureWindowsPlatform(
            GeneratedMaterialPlatformDefinition platformDefinition,
            string diffuseTextureAssetId,
            string roughnessTextureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(diffuseTextureAssetId)) {
                throw new ArgumentException("Diffuse texture asset id must be provided.", nameof(diffuseTextureAssetId));
            } else if (string.IsNullOrWhiteSpace(roughnessTextureAssetId)) {
                throw new ArgumentException("Roughness texture asset id must be provided.", nameof(roughnessTextureAssetId));
            }

            platformDefinition.SchemaId = WindowsMaterialSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(TextureIdFieldId, diffuseTextureAssetId);
            platformDefinition.SetFieldValue(RoughnessFieldId, "1.0");
            platformDefinition.SetFieldValue(MetallicFieldId, "0.0");
            platformDefinition.SetFieldValue(SpecularFieldId, "0.5");
            platformDefinition.SetFieldValue(RoughnessTextureIdFieldId, roughnessTextureAssetId);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "true");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
        }

        /// <summary>
        /// Populates the PS2 textured material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="textureAssetId">Imported marble albedo texture asset id.</param>
        void ConfigurePs2Platform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

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
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="textureAssetId">Imported marble albedo texture asset id.</param>
        void ConfigureGameCubePlatform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

            platformDefinition.SchemaId = GameCubeMaterialSchemaId;
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(TextureRelativePathFieldId, "cooked/imported/" + textureAssetId);
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        /// <summary>
        /// Populates the Nintendo DS untextured material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        void ConfigureDsPlatform(GeneratedMaterialPlatformDefinition platformDefinition) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            }

            platformDefinition.SchemaId = DsMaterialSchemaId;
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

    }
}
