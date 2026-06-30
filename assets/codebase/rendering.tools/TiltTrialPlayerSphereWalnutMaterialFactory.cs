using helengine;
using helengine.editor;
using System.Reflection;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the authored walnut material assigned to the Tilt Trial player sphere.
    /// </summary>
    public sealed class TiltTrialPlayerSphereWalnutMaterialFactory {
        /// <summary>
        /// Stable project-relative material path used by the Tilt Trial player sphere.
        /// </summary>
        public const string MaterialRelativePath = "materials/rendering/tilt_trial/PlayerSphereWalnut.hasset";

        /// <summary>
        /// Stable material asset identifier used by the Tilt Trial player sphere.
        /// </summary>
        public const string MaterialAssetId = "Materials.rendering.tilt_trial.PlayerSphereWalnut";

        /// <summary>
        /// Stable project-relative walnut source texture path.
        /// </summary>
        public const string TextureRelativePath = "Textures/rendering/tilt_trial/PlayerSphereWalnut.bmp";

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
        /// Stable Nintendo DS textured material schema identifier.
        /// </summary>
        const string DsMaterialSchemaId = "ds-standard-textured";

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
        /// Stable authored base color that keeps the walnut bitmap readable without flattening it.
        /// </summary>
        const string WalnutBaseColor = "#FFF0E2CE";

        /// <summary>
        /// Shared generated material writer used to persist the authored walnut material settings.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes one walnut material factory.
        /// </summary>
        public TiltTrialPlayerSphereWalnutMaterialFactory() {
            MaterialWriteService = new GeneratedMaterialAssetWriteService();
        }

        /// <summary>
        /// Writes the authored walnut material settings required by the Tilt Trial player sphere.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string importedTextureAssetId = ResolveTextureAssetId(projectRootPath);
            MaterialWriteService.WriteMaterial(projectRootPath, MaterialRelativePath, CreateDefinition(importedTextureAssetId));
        }

        /// <summary>
        /// Resolves the imported texture asset id that should back the walnut material settings.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Imported texture asset id persisted by the shared editor import pipeline.</returns>
        string ResolveTextureAssetId(string projectRootPath) {
            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string sourceTexturePath = Path.Combine(assetsRootPath, TextureRelativePath.Replace('/', Path.DirectorySeparatorChar));
            AssetImportManager importManager = CreateAssetImportManager(fullProjectRootPath, assetsRootPath);
            TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(sourceTexturePath);
            string assetId = settings.Importer.AssetId;
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new InvalidOperationException("Tilt Trial walnut material requires a persisted imported texture asset id.");
            }

            return assetId;
        }

        /// <summary>
        /// Creates the generated authored material definition consumed by the shared material-settings writer.
        /// </summary>
        /// <param name="textureAssetId">Imported walnut texture asset id.</param>
        /// <returns>Generated material definition populated for every supported preview and runtime platform.</returns>
        GeneratedMaterialAssetDefinition CreateDefinition(string textureAssetId) {
            if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

         GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
         definition.MaterialAsset = new ShaderMaterialAsset {
             Id = MaterialAssetId,
             DiffuseTextureAssetId = textureAssetId,
             RenderState = new MaterialRenderState(),
             CastsShadows = true,
             ReceivesShadows = true
         };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"), textureAssetId);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"), textureAssetId);
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), textureAssetId);
            ConfigureGameCubePlatform(definition.GetOrCreatePlatform("gamecube"), textureAssetId);
            ConfigureDsPlatform(definition.GetOrCreatePlatform("ds"), textureAssetId);
            return definition;
        }

        /// <summary>
        /// Populates the shared Windows and PSP preview material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="textureAssetId">Imported walnut texture asset id.</param>
        void ConfigureWindowsPlatform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

            platformDefinition.SchemaId = WindowsMaterialSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
            platformDefinition.SetFieldValue(BaseColorFieldId, WalnutBaseColor);
        }

        /// <summary>
        /// Populates the PS2 textured material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="textureAssetId">Imported walnut texture asset id.</param>
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
            platformDefinition.SetFieldValue(BaseColorFieldId, WalnutBaseColor);
        }

        /// <summary>
        /// Populates the GameCube textured material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="textureAssetId">Imported walnut texture asset id.</param>
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
            platformDefinition.SetFieldValue(BaseColorFieldId, WalnutBaseColor);
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        /// <summary>
        /// Populates the Nintendo DS textured material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        /// <param name="textureAssetId">Imported walnut texture asset id.</param>
        void ConfigureDsPlatform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

            platformDefinition.SchemaId = DsMaterialSchemaId;
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(TextureRelativePathFieldId, "cooked/imported/" + textureAssetId);
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, WalnutBaseColor);
            platformDefinition.SetFieldValue(LightingModeFieldId, "lit");
        }

        /// <summary>
        /// Builds one asset import manager initialized with the editor host's default importer registrations.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path.</param>
        /// <param name="assetsRootPath">Absolute assets root path.</param>
        /// <returns>Configured asset import manager.</returns>
        AssetImportManager CreateAssetImportManager(string projectRootPath, string assetsRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(assetsRootPath)) {
                throw new ArgumentException("Assets root path must be provided.", nameof(assetsRootPath));
            }

            ContentManager contentManager = new ContentManager(assetsRootPath);
            AssetImportManager importManager = new AssetImportManager(projectRootPath, contentManager);
            IReadOnlyList<IAssetImporterRegistration> importers = CreateDefaultImporters();
            for (int index = 0; index < importers.Count; index++) {
                IAssetImporterRegistration importer = importers[index];
                if (importer == null) {
                    throw new InvalidOperationException("Importer registrations must not contain null entries.");
                }

                importer.Register(importManager);
            }

            importManager.GenerateMissingImportSettings();
            return importManager;
        }

        /// <summary>
        /// Creates the default importer registrations exposed by the editor host assembly.
        /// </summary>
        /// <returns>Importer registrations that match the editor host defaults.</returns>
        IReadOnlyList<IAssetImporterRegistration> CreateDefaultImporters() {
            Assembly appAssembly = Assembly.Load("helengine.editor.app");
            Type importerFactoryType = appAssembly.GetType("helengine.editor.app.EditorHostImporterFactory", throwOnError: true);
            MethodInfo createDefaultMethod = importerFactoryType.GetMethod("CreateDefault", BindingFlags.Public | BindingFlags.Static);
            if (createDefaultMethod == null) {
                throw new InvalidOperationException("EditorHostImporterFactory.CreateDefault was not found.");
            }

            object result = createDefaultMethod.Invoke(null, Array.Empty<object>());
            if (result is not IReadOnlyList<IAssetImporterRegistration> importers) {
                throw new InvalidOperationException("Editor host importer factory did not return importer registrations.");
            }

            return importers;
        }
    }
}
