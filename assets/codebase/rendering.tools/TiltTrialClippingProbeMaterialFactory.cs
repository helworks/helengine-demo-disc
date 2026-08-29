using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the lit textured material that keeps the clipping probe on the same platform runtime path as the PS2 investigation target.
    /// </summary>
    public sealed class TiltTrialClippingProbeMaterialFactory {
        /// <summary>
        /// Stable project-relative material path written by the clipping probe authoring workflow.
        /// </summary>
        public const string MaterialRelativePath = "materials/rendering/tilt_trial/ClippingProbeFaceColors.hasset";

        /// <summary>
        /// Stable material asset identifier used by the colored-face clipping probe.
        /// </summary>
        public const string MaterialAssetId = "Materials.rendering.tilt_trial.ClippingProbeFaceColors";

        /// <summary>
        /// Standard shader schema used by the Windows and PSP preview platforms.
        /// </summary>
        const string StandardShaderSchemaId = "standard-shader";

        /// <summary>
        /// Lit textured schema used by the PlayStation 2 runtime platform.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Built-in forward shader asset used by the standard shader schema.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Built-in forward vertex program used by the standard shader schema.
        /// </summary>
        const string StandardVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Built-in forward pixel program used by the standard shader schema.
        /// </summary>
        const string StandardPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Mesh variant selected by the standard shader schema.
        /// </summary>
        const string MeshVariantName = "Mesh";

        /// <summary>
        /// Untinted opaque base color that leaves each atlas cell's source color unchanged.
        /// </summary>
        const string ProbeBaseColor = "#FFFFFFFF";

        /// <summary>
        /// Matte roughness used by preview platforms for a readable directional-light response.
        /// </summary>
        const string ProbeRoughness = "0.65";

        /// <summary>
        /// Non-metallic scalar used by preview platforms.
        /// </summary>
        const string ProbeMetallic = "0.0";

        /// <summary>
        /// Modest specular scalar used by preview platforms.
        /// </summary>
        const string ProbeSpecular = "0.35";

        /// <summary>
        /// Generated material field that selects use of a built-in shader asset.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

        /// <summary>
        /// Generated material field that references a built-in shader asset.
        /// </summary>
        const string ShaderAssetIdFieldId = "shader-asset-id";

        /// <summary>
        /// Generated material field that selects a vertex program.
        /// </summary>
        const string VertexProgramFieldId = "vertex-program";

        /// <summary>
        /// Generated material field that selects a pixel program.
        /// </summary>
        const string PixelProgramFieldId = "pixel-program";

        /// <summary>
        /// Generated material field that selects a shader variant.
        /// </summary>
        const string VariantFieldId = "variant";

        /// <summary>
        /// Generated material field that binds the imported face-color texture.
        /// </summary>
        const string TextureIdFieldId = "texture-id";

        /// <summary>
        /// Generated material field that stores the preview-platform roughness scalar.
        /// </summary>
        const string RoughnessFieldId = "roughness";

        /// <summary>
        /// Generated material field that stores the preview-platform metallic scalar.
        /// </summary>
        const string MetallicFieldId = "metallic";

        /// <summary>
        /// Generated material field that stores the preview-platform specular scalar.
        /// </summary>
        const string SpecularFieldId = "specular";

        /// <summary>
        /// Generated material field that stores the opaque base color.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Generated material field that stores the alpha rendering mode.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Generated material field that records whether both face windings are rendered.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Generated material field that records whether geometry contributes to shadows.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// Generated material field that records whether geometry receives shadows.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Generated material field that stores the PS2 cooked imported texture location.
        /// </summary>
        const string Ps2TextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Generated material field that disables vertex-color tinting on the PS2 texture path.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Shared writer that persists generated material assets and platform definitions.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes the generated material writer required by the clipping probe material factory.
        /// </summary>
        public TiltTrialClippingProbeMaterialFactory(IEditorProjectAuthoringSession assetAuthoringService, EditorAuthoringTransaction transaction) {
            MaterialWriteService = new GeneratedMaterialAssetWriteService(assetAuthoringService, transaction);
        }

        /// <summary>
        /// Writes the clipping probe material after persisting the imported face-color atlas.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative DemoDisc project root path.</param>
        public void WriteMaterialAsset(string projectRootPath, IEditorProjectAuthoringSession assetAuthoringService) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            TiltTrialClippingProbeTextureFactory textureFactory = new TiltTrialClippingProbeTextureFactory();
            string textureAssetId = textureFactory.WriteTextureAsset(projectRootPath, assetAuthoringService);
            MaterialWriteService.WriteMaterial(MaterialRelativePath, CreateGeneratedMaterialDefinition(textureAssetId));
        }

        /// <summary>
        /// Builds the generated material payload for every supported preview and runtime platform.
        /// </summary>
        /// <param name="textureAssetId">Persisted imported texture asset id for the face-color atlas.</param>
        /// <returns>Generated material definition with explicit culling and texture bindings.</returns>
        GeneratedMaterialAssetDefinition CreateGeneratedMaterialDefinition(string textureAssetId) {
            if (string.IsNullOrWhiteSpace(textureAssetId)) {
                throw new ArgumentException("Texture asset id must be provided.", nameof(textureAssetId));
            }

            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = MaterialAssetId,
                DiffuseTextureAssetId = textureAssetId,
                RenderState = new MaterialRenderState {
                    CullMode = MaterialCullMode.Back
                },
                CastsShadows = true,
                ReceivesShadows = true
            };

            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("windows"), textureAssetId);
            ConfigureWindowsPlatform(definition.GetOrCreatePlatform("psp"), textureAssetId);
            ConfigurePs2Platform(definition.GetOrCreatePlatform("ps2"), textureAssetId);
            return definition;
        }

        /// <summary>
        /// Populates the shared lit standard-shader settings used by Windows and PSP previews.
        /// </summary>
        /// <param name="platformDefinition">Platform material definition to populate.</param>
        /// <param name="textureAssetId">Persisted imported texture asset id for the face-color atlas.</param>
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
            platformDefinition.SetFieldValue(BaseColorFieldId, ProbeBaseColor);
            platformDefinition.SetFieldValue(RoughnessFieldId, ProbeRoughness);
            platformDefinition.SetFieldValue(MetallicFieldId, ProbeMetallic);
            platformDefinition.SetFieldValue(SpecularFieldId, ProbeSpecular);
            platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
            platformDefinition.SetFieldValue(DoubleSidedFieldId, "false");
            platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
            platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
        }

        /// <summary>
        /// Populates the lit textured PlayStation 2 settings required by the clipping probe runtime path.
        /// </summary>
        /// <param name="platformDefinition">Platform material definition to populate.</param>
        /// <param name="textureAssetId">Persisted imported texture asset id for the face-color atlas.</param>
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
            platformDefinition.SetFieldValue(BaseColorFieldId, ProbeBaseColor);
        }
    }
}
