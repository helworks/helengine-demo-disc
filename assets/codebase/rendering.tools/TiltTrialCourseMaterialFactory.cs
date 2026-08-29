using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the authored textured material assigned to the Tilt Trial course geometry.
    /// </summary>
    public sealed class TiltTrialCourseMaterialFactory {
        /// <summary>
        /// Stable project-relative material path used by the Tilt Trial course pieces and catch floor.
        /// </summary>
        public const string MaterialRelativePath = "materials/rendering/tilt_trial/Course.hasset";

        /// <summary>
        /// Stable material asset identifier used by the Tilt Trial course material.
        /// </summary>
        public const string MaterialAssetId = "Materials.rendering.tilt_trial.Course";

        /// <summary>
        /// Stable Windows and PSP standard-shader schema identifier used by the shared material pipeline.
        /// </summary>
        const string StandardShaderSchemaId = "standard-shader";

        /// <summary>
        /// Stable PlayStation 2 material schema identifier used by the shared PS2 material pipeline.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable built-in forward standard shader asset identifier used by the shared shader pipeline.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable built-in forward standard vertex program entry used by the shared shader pipeline.
        /// </summary>
        const string StandardVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Stable built-in forward standard pixel program entry used by the shared shader pipeline.
        /// </summary>
        const string StandardPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Stable mesh shader variant used by the Tilt Trial course material.
        /// </summary>
        const string MeshVariantName = "Mesh";

        /// <summary>
        /// Stable authored base color that preserves the generated texture colors without tinting them.
        /// </summary>
        const string CourseBaseColor = "#FFFFFFFF";

        /// <summary>
        /// Stable authored roughness scalar that keeps the stage slightly matte without flattening it.
        /// </summary>
        const string CourseRoughness = "0.65";

        /// <summary>
        /// Stable authored metallic scalar for the non-metal course material.
        /// </summary>
        const string CourseMetallic = "0.0";

        /// <summary>
        /// Stable authored specular scalar that preserves a little highlight on the course.
        /// </summary>
        const string CourseSpecular = "0.35";

        /// <summary>
        /// Stable field identifier used to opt into the built-in standard shader.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

        /// <summary>
        /// Stable field identifier used to store the shared shader asset reference in generated material settings.
        /// </summary>
        const string ShaderAssetIdFieldId = "shader-asset-id";

        /// <summary>
        /// Stable field identifier used to store the resolved vertex program name in generated material settings.
        /// </summary>
        const string VertexProgramFieldId = "vertex-program";

        /// <summary>
        /// Stable field identifier used to store the resolved pixel program name in generated material settings.
        /// </summary>
        const string PixelProgramFieldId = "pixel-program";

        /// <summary>
        /// Stable field identifier used to store the resolved shader variant name in generated material settings.
        /// </summary>
        const string VariantFieldId = "variant";

        /// <summary>
        /// Stable field identifier used to store the authored base color in generated material settings.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Stable field identifier used to store imported texture bindings on textured material paths.
        /// </summary>
        const string TextureIdFieldId = "texture-id";

        /// <summary>
        /// Stable field identifier used to store the authored roughness scalar.
        /// </summary>
        const string RoughnessFieldId = "roughness";

        /// <summary>
        /// Stable field identifier used to store the authored metallic scalar.
        /// </summary>
        const string MetallicFieldId = "metallic";

        /// <summary>
        /// Stable field identifier used to store the authored specular scalar.
        /// </summary>
        const string SpecularFieldId = "specular";

        /// <summary>
        /// Stable field identifier used to store the authored cast-shadows toggle.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// Stable field identifier used to store the authored receive-shadows toggle.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Stable field identifier used to store the authored alpha mode.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Stable field identifier used to store the authored double-sided flag.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Stable field identifier used to store the imported texture-relative path required by the PS2 material pipeline.
        /// </summary>
        const string Ps2TextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Stable field identifier used to store the vertex-color behavior required by fixed-pipeline platforms.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Stable generated material writer used to persist the authored Tilt Trial course material.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Initializes one Tilt Trial course material factory.
        /// </summary>
        public TiltTrialCourseMaterialFactory(IEditorProjectAuthoringSession assetAuthoringService, EditorAuthoringTransaction transaction) {
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            MaterialWriteService = new GeneratedMaterialAssetWriteService(assetAuthoringService, transaction);
        }

        /// <summary>
        /// Writes the Tilt Trial course material asset and its platform settings into the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAsset(string projectRootPath, IEditorProjectAuthoringSession assetAuthoringService) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            TiltTrialCourseTextureFactory textureFactory = new TiltTrialCourseTextureFactory();
            string textureAssetId = textureFactory.WriteTextureAsset(projectRootPath, assetAuthoringService, Transaction);
            MaterialWriteService.WriteMaterial(MaterialRelativePath, CreateGeneratedMaterialDefinition(textureAssetId));
        }

        /// <summary>
        /// Builds the generated material definition that routes the Tilt Trial course through the shared lit forward shader with authored color and PBR overrides.
        /// </summary>
        /// <returns>Generated material definition for the Tilt Trial course material.</returns>
        GeneratedMaterialAssetDefinition CreateGeneratedMaterialDefinition(string textureAssetId) {
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
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), textureAssetId);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"), textureAssetId);
            return definition;
        }

        /// <summary>
        /// Populates the shared Windows and PSP standard-shader settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        void ConfigureWindowsPlatform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

            platformDefinition.SchemaId = StandardShaderSchemaId;
            platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
            platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            platformDefinition.SetFieldValue(VertexProgramFieldId, StandardVertexProgramName);
            platformDefinition.SetFieldValue(PixelProgramFieldId, StandardPixelProgramName);
            platformDefinition.SetFieldValue(VariantFieldId, MeshVariantName);
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(BaseColorFieldId, CourseBaseColor);
            platformDefinition.SetFieldValue(RoughnessFieldId, CourseRoughness);
            platformDefinition.SetFieldValue(MetallicFieldId, CourseMetallic);
            platformDefinition.SetFieldValue(SpecularFieldId, CourseSpecular);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
        }

        /// <summary>
        /// Populates the PS2 fixed-pipeline solid-color material settings.
        /// </summary>
        /// <param name="platformDefinition">Generated platform definition to populate.</param>
        void ConfigurePs2Platform(GeneratedMaterialPlatformDefinition platformDefinition, string textureAssetId) {
            if (platformDefinition == null) {
                throw new ArgumentNullException(nameof(platformDefinition));
            } else if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

            platformDefinition.SchemaId = Ps2MaterialSchemaId;
            platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);
            platformDefinition.SetFieldValue(Ps2TextureRelativePathFieldId, "cooked/imported/" + textureAssetId);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(VertexColorModeFieldId, "ignore");
            platformDefinition.SetFieldValue(BaseColorFieldId, CourseBaseColor);
        }
    }
}
