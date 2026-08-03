namespace city.tests {
    /// <summary>
    /// Verifies the colored-face clipping probe factories keep their deterministic model, atlas, and textured material source contracts.
    /// </summary>
    public sealed class TiltTrialClippingProbeAssetSourceTests {
        /// <summary>
        /// Ensures the probe model factory declares the canonical asset identity, cube geometry, and six independently addressable face UV arrays.
        /// </summary>
        [Fact]
        public void Clipping_probe_model_source_defines_canonical_cube_and_face_uv_contract() {
            string modelSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeModelFactory.cs";
            string modelSource = File.ReadAllText(modelSourcePath);

            Assert.Contains("public const string ModelAssetId = \"Models.rendering.tilt_trial.ClippingProbeFaceColors\";", modelSource, StringComparison.Ordinal);
            Assert.Contains("public ModelAsset CreateModelAsset()", modelSource, StringComparison.Ordinal);
            Assert.Contains("new float3(-0.5f, -0.5f, -0.5f)", modelSource, StringComparison.Ordinal);
            Assert.Contains("Indices16 =", modelSource, StringComparison.Ordinal);
            Assert.Contains("BackFaceUv", modelSource, StringComparison.Ordinal);
            Assert.Contains("FrontFaceUv", modelSource, StringComparison.Ordinal);
            Assert.Contains("RightFaceUv", modelSource, StringComparison.Ordinal);
            Assert.Contains("LeftFaceUv", modelSource, StringComparison.Ordinal);
            Assert.Contains("TopFaceUv", modelSource, StringComparison.Ordinal);
            Assert.Contains("BottomFaceUv", modelSource, StringComparison.Ordinal);

            global::System.Text.RegularExpressions.Match indicesMatch = global::System.Text.RegularExpressions.Regex.Match(
                modelSource,
                @"Indices16\s*=\s*\[(?<indices>[^\]]+)\]",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.True(indicesMatch.Success, "The probe model must assign its 16-bit triangle indices from one array literal.");
            string[] indices = indicesMatch.Groups["indices"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.Equal(36, indices.Length);
        }

        /// <summary>
        /// Ensures the six face UV arrays use one-texel-inset regions that occupy distinct cells in the 128-by-64 atlas.
        /// </summary>
        [Fact]
        public void Clipping_probe_model_source_maps_each_face_to_one_non_overlapping_padded_atlas_region() {
            string modelSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeModelFactory.cs";
            string modelSource = File.ReadAllText(modelSourcePath);

            AssertFaceUvRegion(modelSource, "BackFaceUv", 9, 38, 5, 26);
            AssertFaceUvRegion(modelSource, "FrontFaceUv", 49, 78, 5, 26);
            AssertFaceUvRegion(modelSource, "RightFaceUv", 89, 118, 5, 26);
            AssertFaceUvRegion(modelSource, "LeftFaceUv", 9, 38, 37, 58);
            AssertFaceUvRegion(modelSource, "TopFaceUv", 49, 78, 37, 58);
            AssertFaceUvRegion(modelSource, "BottomFaceUv", 89, 118, 37, 58);
        }

        /// <summary>
        /// Ensures the probe texture factory preserves the deterministic atlas size and per-cell color resolver.
        /// </summary>
        [Fact]
        public void Clipping_probe_texture_source_defines_deterministic_face_color_atlas() {
            string textureSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeTextureFactory.cs";
            string textureSource = File.ReadAllText(textureSourcePath);

            Assert.Contains("const int TextureWidth = 128;", textureSource, StringComparison.Ordinal);
            Assert.Contains("const int TextureHeight = 64;", textureSource, StringComparison.Ordinal);
            Assert.Contains("ResolveCellColor", textureSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the probe material keeps PS2 on the lit textured shader path with explicit imported texture and culling settings.
        /// </summary>
        [Fact]
        public void Clipping_probe_material_source_defines_lit_ps2_textured_contract() {
            string materialSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeMaterialFactory.cs";
            string materialSource = File.ReadAllText(materialSourcePath);

            Assert.Contains("ps2-simple-lit-textured", materialSource, StringComparison.Ordinal);
            Assert.Contains("texture-relative-path", materialSource, StringComparison.Ordinal);
            Assert.Contains("double-sided", materialSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies one named UV array contains exactly the four inset corners of its assigned atlas cell.
        /// </summary>
        /// <param name="modelSource">Complete model factory source text to inspect.</param>
        /// <param name="faceUvName">Named UV array assigned to one canonical cube face.</param>
        /// <param name="minimumU">Inclusive atlas-space minimum horizontal texel coordinate.</param>
        /// <param name="maximumU">Inclusive atlas-space maximum horizontal texel coordinate.</param>
        /// <param name="minimumV">Inclusive atlas-space minimum vertical texel coordinate.</param>
        /// <param name="maximumV">Inclusive atlas-space maximum vertical texel coordinate.</param>
        static void AssertFaceUvRegion(string modelSource, string faceUvName, int minimumU, int maximumU, int minimumV, int maximumV) {
            string faceUvPattern = $@"{faceUvName}\s*=\s*\[(?<coordinates>[\s\S]*?)\];";
            global::System.Text.RegularExpressions.Match faceUvMatch = global::System.Text.RegularExpressions.Regex.Match(modelSource, faceUvPattern);
            Assert.True(faceUvMatch.Success, $"The probe model must define the {faceUvName} atlas UV array.");
            string coordinates = faceUvMatch.Groups["coordinates"].Value;

            Assert.Contains($"new float2({minimumU}f / TextureWidth, {minimumV}f / TextureHeight)", coordinates, StringComparison.Ordinal);
            Assert.Contains($"new float2({maximumU}f / TextureWidth, {minimumV}f / TextureHeight)", coordinates, StringComparison.Ordinal);
            Assert.Contains($"new float2({maximumU}f / TextureWidth, {maximumV}f / TextureHeight)", coordinates, StringComparison.Ordinal);
            Assert.Contains($"new float2({minimumU}f / TextureWidth, {maximumV}f / TextureHeight)", coordinates, StringComparison.Ordinal);
        }
    }
}
