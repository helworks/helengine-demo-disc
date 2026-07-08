using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Creates the authored shared solid-color material used by the cube-test scene and writes its per-platform material settings.
    /// </summary>
    public sealed class ForwardSolidColorMaterialFactory {
        /// <summary>
        /// Stable project-relative material path used by the cube-test scene.
        /// </summary>
        public const string MaterialRelativePath = "materials/rendering/cube_test/CubeTestSolid.hasset";

        /// <summary>
        /// Stable serialized material asset identifier stored inside the generated material asset.
        /// </summary>
        public const string MaterialAssetId = "Materials.rendering.cube_test.CubeTestSolid";

        /// <summary>
        /// Stable Windows and PS Vita standard-shader schema identifier used by the shared material pipeline.
        /// </summary>
        const string StandardShaderSchemaId = "standard-shader";

        /// <summary>
        /// Stable PlayStation 2 material schema identifier used by the shared PS2 material pipeline.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable built-in forward standard shader asset identifier used by the shared shader pipeline.
        /// </summary>
        const string SolidColorShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable built-in forward standard vertex program entry used by the shared shader pipeline.
        /// </summary>
        const string SolidColorVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Stable built-in forward standard pixel program entry used by the shared shader pipeline.
        /// </summary>
        const string SolidColorPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Stable mesh shader variant used by the cube-test solid-color path.
        /// </summary>
        const string MeshVariantName = "Mesh";

        /// <summary>
        /// Stable authored base color assigned to the rotating cube.
        /// </summary>
        const string CubeBaseColor = "#ffffffff";

        /// <summary>
        /// Stable field identifier used to store the shared shader asset reference in generated material settings.
        /// </summary>
        const string ShaderAssetIdFieldId = "shader-asset-id";

        /// <summary>
        /// Stable field identifier used to preserve the standard-shader settings toggle expected by the shared editor material pipeline.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

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
        /// Stable field identifier used to store the authored shadow-casting flag in generated material settings.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// Stable field identifier used to store the imported texture-relative path required by the PS2 material pipeline.
        /// </summary>
        const string Ps2TextureRelativePathFieldId = "texture-relative-path";

        /// <summary>
        /// Stable field identifier used to store the alpha mode required by the PS2 material pipeline.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Stable field identifier used to store the double-sided flag required by the PS2 material pipeline.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Stable field identifier used to store the vertex-color behavior required by the PS2 material pipeline.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Stable field identifier used to store the authored shadow-receiving flag in generated material settings.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Service used to persist the generated material asset plus its sidecar settings.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes one shared solid-color material factory.
        /// </summary>
        public ForwardSolidColorMaterialFactory() {
            MaterialWriteService = new GeneratedMaterialAssetWriteService();
        }

        /// <summary>
        /// Writes the file-backed cube-test solid-color material asset and its platform settings into the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            MaterialWriteService.WriteMaterial(projectRootPath, MaterialRelativePath, CreateGeneratedMaterialDefinition());
        }

        /// <summary>
        /// Builds the generated material definition that routes cube-test through the shared lit forward shader while preserving the authored flat white base color.
        /// </summary>
        /// <returns>Generated material definition for the cube-test solid-color material.</returns>
        GeneratedMaterialAssetDefinition CreateGeneratedMaterialDefinition() {
            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = MaterialAssetId,
                RenderState = new MaterialRenderState(),
                CastsShadows = false,
                ReceivesShadows = false
            };

            GeneratedMaterialPlatformDefinition windowsSettings = definition.GetOrCreatePlatform("windows");
            windowsSettings.SchemaId = StandardShaderSchemaId;
            windowsSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            windowsSettings.SetFieldValue(ShaderAssetIdFieldId, SolidColorShaderAssetId);
            windowsSettings.SetFieldValue(VertexProgramFieldId, SolidColorVertexProgramName);
            windowsSettings.SetFieldValue(PixelProgramFieldId, SolidColorPixelProgramName);
            windowsSettings.SetFieldValue(VariantFieldId, MeshVariantName);
            windowsSettings.SetFieldValue(BaseColorFieldId, CubeBaseColor);
            windowsSettings.SetFieldValue(CastsShadowFieldId, "false");
            windowsSettings.SetFieldValue(ReceivesShadowFieldId, "false");

        GeneratedMaterialPlatformDefinition psVitaSettings = definition.GetOrCreatePlatform("psvita");
        psVitaSettings.SchemaId = StandardShaderSchemaId;
        psVitaSettings.SetFieldValue(UseCustomShaderFieldId, "false");
        psVitaSettings.SetFieldValue(ShaderAssetIdFieldId, SolidColorShaderAssetId);
        psVitaSettings.SetFieldValue(VertexProgramFieldId, SolidColorVertexProgramName);
            psVitaSettings.SetFieldValue(PixelProgramFieldId, SolidColorPixelProgramName);
            psVitaSettings.SetFieldValue(VariantFieldId, MeshVariantName);
        psVitaSettings.SetFieldValue(BaseColorFieldId, CubeBaseColor);
        psVitaSettings.SetFieldValue(CastsShadowFieldId, "false");
        psVitaSettings.SetFieldValue(ReceivesShadowFieldId, "false");

        GeneratedMaterialPlatformDefinition ps2Settings = definition.GetOrCreatePlatform("ps2");
        ps2Settings.SchemaId = Ps2MaterialSchemaId;
        ps2Settings.SetFieldValue(Ps2TextureRelativePathFieldId, string.Empty);
        ps2Settings.SetFieldValue(AlphaModeFieldId, "opaque");
        ps2Settings.SetFieldValue(DoubleSidedFieldId, "false");
        ps2Settings.SetFieldValue(CastsShadowFieldId, "false");
        ps2Settings.SetFieldValue(VertexColorModeFieldId, "ignore");
        ps2Settings.SetFieldValue(BaseColorFieldId, CubeBaseColor);

        return definition;
    }
}
}
