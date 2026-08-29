using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the twenty-five metallic-by-roughness material assets and builds their in-memory runtime materials for the PBR material gallery showcase.
    /// </summary>
    public sealed class PbrMaterialGalleryMaterialFactory {
        /// <summary>
        /// Number of metallic steps swept by the gallery grid, evenly spanning zero to one.
        /// </summary>
        public const int MetallicSteps = 5;

        /// <summary>
        /// Number of roughness steps swept by the gallery grid, evenly spanning zero to one.
        /// </summary>
        public const int RoughnessSteps = 5;

        /// <summary>
        /// Stable Windows and PSP schema identifier used by the standard shader material path.
        /// </summary>
        const string WindowsMaterialSchemaId = "standard-shader";

        /// <summary>
        /// Stable PS2 textured material schema identifier.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable GameCube untextured material schema identifier.
        /// </summary>
        const string GameCubeMaterialSchemaId = "gamecube-standard";

        /// <summary>
        /// Stable Nintendo DS untextured material schema identifier.
        /// </summary>
        const string DsMaterialSchemaId = "ds-standard-lit";

        /// <summary>
        /// Stable built-in forward shader asset id used by the editor preview material path.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable built-in standard shader source file used by generated gallery runtime materials.
        /// </summary>
        const string StandardShaderSourceFileName = "ForwardStandardShader.hlsl";

        /// <summary>
        /// Stable standard shader vertex program used by preview material payloads.
        /// </summary>
        const string StandardVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Stable standard shader pixel program used by preview material payloads.
        /// </summary>
        const string StandardPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Stable mesh variant used by preview material payloads.
        /// </summary>
        const string StandardMeshVariantName = "Mesh";

        const string UseCustomShaderFieldId = "use-custom-shader";
        const string ShaderAssetIdFieldId = "shader-asset-id";
        const string TextureIdFieldId = "texture-id";
        const string RoughnessFieldId = "roughness";
        const string MetallicFieldId = "metallic";
        const string SpecularFieldId = "specular";
        const string BaseColorFieldId = "base-color";
        const string CastsShadowFieldId = "casts-shadow";
        const string Ps2CastShadowsFieldId = "cast-shadows";
        const string ReceivesShadowFieldId = "receives-shadow";
        const string AlphaModeFieldId = "alpha-mode";
        const string DoubleSidedFieldId = "double-sided";
        const string VertexColorModeFieldId = "vertex-color-mode";
        const string LightingModeFieldId = "lighting-mode";

        /// <summary>
        /// Stable base color shared by every gallery sphere on Windows and PSP, where metallic and roughness alone drive the visual difference.
        /// </summary>
        const string SharedBaseColor = "#B0B0B0FF";

        /// <summary>
        /// Relative project folder used for the generated PBR gallery materials.
        /// </summary>
        const string MaterialRootRelativePath = "materials/rendering/pbr_gallery";

        /// <summary>
        /// Service used to persist generated authored material assets plus their per-platform material settings.
        /// </summary>
        readonly IEditorProjectAuthoringSession AuthoringSession;
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes one PBR material gallery material factory.
        /// </summary>
        public PbrMaterialGalleryMaterialFactory(IEditorProjectAuthoringSession authoringSession) {
            AuthoringSession = authoringSession ?? throw new ArgumentNullException(nameof(authoringSession));
            MaterialWriteService = new GeneratedMaterialAssetWriteService(AuthoringSession);
        }

        /// <summary>
        /// Resolves the flat gallery index for a metallic/roughness step pair, in row-major (metallic, then roughness) order.
        /// </summary>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        /// <returns>Flat index into the twenty-five element gallery material array.</returns>
        public static int ResolveIndex(int metallicIndex, int roughnessIndex) {
            if (metallicIndex < 0 || metallicIndex >= MetallicSteps) {
                throw new ArgumentOutOfRangeException(nameof(metallicIndex));
            } else if (roughnessIndex < 0 || roughnessIndex >= RoughnessSteps) {
                throw new ArgumentOutOfRangeException(nameof(roughnessIndex));
            }

            return (metallicIndex * RoughnessSteps) + roughnessIndex;
        }

        /// <summary>
        /// Resolves the authored metallic value for a metallic step index, evenly spanning zero to one.
        /// </summary>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <returns>Authored metallic scalar.</returns>
        public static float ResolveMetallic(int metallicIndex) {
            return metallicIndex / (float)(MetallicSteps - 1);
        }

        /// <summary>
        /// Resolves the authored roughness value for a roughness step index, evenly spanning zero to one.
        /// </summary>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        /// <returns>Authored roughness scalar.</returns>
        public static float ResolveRoughness(int roughnessIndex) {
            return roughnessIndex / (float)(RoughnessSteps - 1);
        }

        /// <summary>
        /// Writes the twenty-five file-backed material assets and settings documents used by the gallery scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            for (int metallicIndex = 0; metallicIndex < MetallicSteps; metallicIndex++) {
                for (int roughnessIndex = 0; roughnessIndex < RoughnessSteps; roughnessIndex++) {
                    WriteMaterialAsset(projectRootPath, metallicIndex, roughnessIndex);
                }
            }
        }

        /// <summary>
        /// Creates the twenty-five runtime materials used while authoring the gallery scene.
        /// </summary>
        /// <returns>Runtime materials ordered by <see cref="ResolveIndex"/>.</returns>
        public RuntimeMaterial[] CreateRuntimeMaterials() {
            RuntimeMaterial[] materials = new RuntimeMaterial[MetallicSteps * RoughnessSteps];
            for (int metallicIndex = 0; metallicIndex < MetallicSteps; metallicIndex++) {
                for (int roughnessIndex = 0; roughnessIndex < RoughnessSteps; roughnessIndex++) {
                    materials[ResolveIndex(metallicIndex, roughnessIndex)] = CreateRuntimeMaterial(metallicIndex, roughnessIndex);
                }
            }

            return materials;
        }

        /// <summary>
        /// Creates one runtime material for the supplied metallic/roughness step pair using the generated standard shader path.
        /// </summary>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        /// <returns>Runtime material instance for the supplied step pair.</returns>
        RuntimeMaterial CreateRuntimeMaterial(int metallicIndex, int roughnessIndex) {
            ShaderMaterialAsset materialAsset = new ShaderMaterialAsset {
                Id = CreateMaterialAssetId(metallicIndex, roughnessIndex),
                ShaderAssetId = StandardShaderAssetId,
                VertexProgram = StandardVertexProgramName,
                PixelProgram = StandardPixelProgramName,
                Variant = StandardMeshVariantName,
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };
            ShaderAsset shaderAsset = AuthoringSession.LoadBuiltInShaderAsset(StandardShaderSourceFileName);
            materialAsset.ConstantBuffers = new[] {
                new MaterialConstantBufferAsset {
                    Name = StandardMaterialBaseColorDefaults.BaseColorBufferName,
                    Data = StandardMaterialBaseColorDefaults.CreateConstantBufferData(ParseColor(SharedBaseColor))
                },
                new MaterialConstantBufferAsset {
                    Name = StandardMaterialMetallicDefaults.MetallicBufferName,
                    Data = StandardMaterialMetallicDefaults.CreateConstantBufferData(ResolveMetallic(metallicIndex))
                },
                new MaterialConstantBufferAsset {
                    Name = StandardMaterialRoughnessDefaults.RoughnessBufferName,
                    Data = StandardMaterialRoughnessDefaults.CreateConstantBufferData(ResolveRoughness(roughnessIndex))
                },
                new MaterialConstantBufferAsset {
                    Name = StandardMaterialSpecularDefaults.SpecularBufferName,
                    Data = StandardMaterialSpecularDefaults.CreateConstantBufferData(0.5f)
                }
            };

            RuntimeMaterial runtimeMaterial = AuthoringSession.OwningCore.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(runtimeMaterial), AuthoringSession.OwningCore.RenderManager2D);
            return runtimeMaterial;
        }

        /// <summary>
        /// Writes one file-backed material asset and its settings document for the supplied step pair.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        void WriteMaterialAsset(string projectRootPath, int metallicIndex, int roughnessIndex) {
            string relativePath = BuildMaterialRelativePath(metallicIndex, roughnessIndex);
            MaterialWriteService.WriteMaterial(relativePath, CreateGeneratedMaterialDefinition(metallicIndex, roughnessIndex));
        }

        /// <summary>
        /// Creates one generated authored material definition for the supplied step pair.
        /// </summary>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        /// <returns>Generated authored material definition populated for every supported platform.</returns>
        GeneratedMaterialAssetDefinition CreateGeneratedMaterialDefinition(int metallicIndex, int roughnessIndex) {
            float metallic = ResolveMetallic(metallicIndex);
            float roughness = ResolveRoughness(roughnessIndex);
            string metallicText = metallic.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string roughnessText = roughness.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string fallbackBaseColor = ResolveFallbackBaseColor(metallic);

            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = CreateMaterialAssetId(metallicIndex, roughnessIndex),
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };

            GeneratedMaterialPlatformDefinition windowsSettings = definition.GetOrCreatePlatform("windows");
            windowsSettings.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            windowsSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            windowsSettings.SetFieldValue(TextureIdFieldId, string.Empty);
            windowsSettings.SetFieldValue(MetallicFieldId, metallicText);
            windowsSettings.SetFieldValue(RoughnessFieldId, roughnessText);
            windowsSettings.SetFieldValue(SpecularFieldId, "0.5");
            windowsSettings.SetFieldValue(AlphaModeFieldId, "opaque");
            windowsSettings.SetFieldValue(DoubleSidedFieldId, "false");
            windowsSettings.SetFieldValue(CastsShadowFieldId, "true");
            windowsSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            windowsSettings.SetFieldValue(BaseColorFieldId, SharedBaseColor);

            GeneratedMaterialPlatformDefinition pspSettings = definition.GetOrCreatePlatform("psp");
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            pspSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            pspSettings.SetFieldValue(TextureIdFieldId, string.Empty);
            pspSettings.SetFieldValue(MetallicFieldId, metallicText);
            pspSettings.SetFieldValue(RoughnessFieldId, roughnessText);
            pspSettings.SetFieldValue(SpecularFieldId, "0.5");
            pspSettings.SetFieldValue(AlphaModeFieldId, "opaque");
            pspSettings.SetFieldValue(DoubleSidedFieldId, "false");
            pspSettings.SetFieldValue(CastsShadowFieldId, "true");
            pspSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            pspSettings.SetFieldValue(BaseColorFieldId, SharedBaseColor);

            GeneratedMaterialPlatformDefinition ps2Settings = definition.GetOrCreatePlatform("ps2");
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.SetFieldValue(AlphaModeFieldId, "opaque");
            ps2Settings.SetFieldValue(DoubleSidedFieldId, "false");
            ps2Settings.SetFieldValue(Ps2CastShadowsFieldId, "true");
            ps2Settings.SetFieldValue(VertexColorModeFieldId, "ignore");
            ps2Settings.SetFieldValue(BaseColorFieldId, fallbackBaseColor);

            GeneratedMaterialPlatformDefinition gameCubeSettings = definition.GetOrCreatePlatform("gamecube");
            gameCubeSettings.SchemaId = GameCubeMaterialSchemaId;
            gameCubeSettings.SetFieldValue(DoubleSidedFieldId, "false");
            gameCubeSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            gameCubeSettings.SetFieldValue(BaseColorFieldId, fallbackBaseColor);
            gameCubeSettings.SetFieldValue(LightingModeFieldId, "lit");

            GeneratedMaterialPlatformDefinition dsSettings = definition.GetOrCreatePlatform("ds");
            dsSettings.SchemaId = DsMaterialSchemaId;
            dsSettings.SetFieldValue(DoubleSidedFieldId, "false");
            dsSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            dsSettings.SetFieldValue(BaseColorFieldId, fallbackBaseColor);
            dsSettings.SetFieldValue(LightingModeFieldId, "lit");

            return definition;
        }

        /// <summary>
        /// Resolves a mid-gray fallback base color whose lightness scales with the authored metallic value, for platforms without metallic shading.
        /// </summary>
        /// <param name="metallic">Authored metallic scalar in the zero-to-one range.</param>
        /// <returns>Fallback base color in <c>#RRGGBBAA</c> form.</returns>
        static string ResolveFallbackBaseColor(float metallic) {
            int channel = 60 + (int)Math.Round(140f * metallic);
            string channelHex = channel.ToString("X2");
            return "#" + channelHex + channelHex + channelHex + "FF";
        }

        /// <summary>
        /// Builds the stable per-material relative path used by the supplied step pair.
        /// </summary>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        /// <returns>Stable project-relative material path.</returns>
        static string BuildMaterialRelativePath(int metallicIndex, int roughnessIndex) {
            return MaterialRootRelativePath + "/M" + metallicIndex + "R" + roughnessIndex + ".hasset";
        }

        /// <summary>
        /// Creates one stable material asset id for the supplied step pair.
        /// </summary>
        /// <param name="metallicIndex">Zero-based metallic step index.</param>
        /// <param name="roughnessIndex">Zero-based roughness step index.</param>
        /// <returns>Material asset id stored inside the serialized file-backed asset.</returns>
        static string CreateMaterialAssetId(int metallicIndex, int roughnessIndex) {
            return "Materials.rendering.pbr_gallery.M" + metallicIndex + "R" + roughnessIndex;
        }

        /// <summary>
        /// Parses one authored hex color string into a normalized float4 color.
        /// </summary>
        /// <param name="colorValue">Authored color string in <c>#RRGGBBAA</c> form.</param>
        /// <returns>Normalized float4 color.</returns>
        static float4 ParseColor(string colorValue) {
            uint rgba = Convert.ToUInt32(colorValue.Substring(1, 8), 16);
            return new float4(
                ((rgba >> 24) & 0xFF) / 255f,
                ((rgba >> 16) & 0xFF) / 255f,
                ((rgba >> 8) & 0xFF) / 255f,
                (rgba & 0xFF) / 255f);
        }
    }
}
