namespace city.tests {
    /// <summary>
    /// Verifies the colored-face clipping probe factories keep their deterministic model, atlas, and textured material source contracts.
    /// </summary>
    public sealed class TiltTrialClippingProbeAssetSourceTests {
        [Fact]
        /// <summary>
        /// Validates the probe model source isolates the positive-Y face while preserving the fixed asset identity and full-cube bounds used by the clipping experiment.
        /// </summary>
        public void Clipping_probe_model_source_defines_top_face_only_contract() {
            string modelSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeModelFactory.cs";
            string modelSource = File.ReadAllText(modelSourcePath);

            Assert.Contains("public const string ModelAssetId = \"Models.rendering.tilt_trial.ClippingProbeFaceColors\";", modelSource, StringComparison.Ordinal);
            Assert.Contains("public ModelAsset CreateModelAsset()", modelSource, StringComparison.Ordinal);
            Assert.Contains("new float3(-0.5f, -0.5f, -0.5f)", modelSource, StringComparison.Ordinal);
            Assert.Contains("global::helengine.editor.AssetSerializer.Serialize", modelSource, StringComparison.Ordinal);
            Assert.Contains("TopFaceUv", modelSource, StringComparison.Ordinal);
            Assert.Contains("TexCoords = [.. TopFaceUv]", modelSource, StringComparison.Ordinal);
            Assert.Contains("BoundsMin = new float3(-0.5f, -0.5f, -0.5f)", modelSource, StringComparison.Ordinal);
            Assert.Contains("BoundsMax = new float3(0.5f, 0.5f, 0.5f)", modelSource, StringComparison.Ordinal);
            Assert.DoesNotContain("BackFaceUv", modelSource, StringComparison.Ordinal);
            Assert.DoesNotContain("FrontFaceUv", modelSource, StringComparison.Ordinal);
            Assert.DoesNotContain("RightFaceUv", modelSource, StringComparison.Ordinal);
            Assert.DoesNotContain("LeftFaceUv", modelSource, StringComparison.Ordinal);
            Assert.DoesNotContain("BottomFaceUv", modelSource, StringComparison.Ordinal);

            global::System.Text.RegularExpressions.Match positionsMatch = global::System.Text.RegularExpressions.Regex.Match(
                modelSource,
                @"Positions\s*=\s*\[(?<positions>[^\]]+)\]",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.True(positionsMatch.Success, "The probe model must assign its positions from one array literal.");
            string positions = positionsMatch.Groups["positions"].Value;
            Assert.Equal(4, global::System.Text.RegularExpressions.Regex.Matches(positions, @"new float3\([^\)]*0\.5f[^\)]*\)").Count);
            Assert.Equal(4, global::System.Text.RegularExpressions.Regex.Matches(positions, @"new float3\([^,]+, 0\.5f, [^\)]+\)").Count);

            global::System.Text.RegularExpressions.Match normalsMatch = global::System.Text.RegularExpressions.Regex.Match(
                modelSource,
                @"Normals\s*=\s*\[(?<normals>[^\]]+)\]",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.True(normalsMatch.Success, "The probe model must assign its normals from one array literal.");
            string normals = normalsMatch.Groups["normals"].Value;
            Assert.Equal(4, global::System.Text.RegularExpressions.Regex.Matches(normals, @"new float3\(0f, 1f, 0f\)").Count);

            global::System.Text.RegularExpressions.Match indicesMatch = global::System.Text.RegularExpressions.Regex.Match(
                modelSource,
                @"Indices16\s*=\s*\[(?<indices>[^\]]+)\]",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.True(indicesMatch.Success, "The probe model must assign its 16-bit triangle indices from one array literal.");
            string[] indices = indicesMatch.Groups["indices"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.Equal(new[] { "0", "1", "2", "0", "2", "3" }, indices);
            Assert.Contains("IndexCount = 6", modelSource, StringComparison.Ordinal);
        }

        [Fact]
        /// <summary>
        /// Validates the texture source retains the fixed atlas dimensions and cell-color resolver that make individual face colors reproducible across probe runs.
        /// </summary>
        public void Clipping_probe_texture_source_defines_deterministic_face_color_atlas() {
            string textureSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeTextureFactory.cs";
            string textureSource = File.ReadAllText(textureSourcePath);

            Assert.Contains("const int TextureWidth = 128;", textureSource, StringComparison.Ordinal);
            Assert.Contains("const int TextureHeight = 64;", textureSource, StringComparison.Ordinal);
            Assert.Contains("ResolveCellColor", textureSource, StringComparison.Ordinal);
        }

        [Fact]
        /// <summary>
        /// Validates the material source keeps PlayStation 2 on the lit textured path with the imported atlas and explicit culling fields needed by the clipping probe.
        /// </summary>
        public void Clipping_probe_material_source_defines_lit_ps2_textured_contract() {
            string materialSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeMaterialFactory.cs";
            string materialSource = File.ReadAllText(materialSourcePath);

            Assert.Contains("ps2-simple-lit-textured", materialSource, StringComparison.Ordinal);
            Assert.Contains("texture-relative-path", materialSource, StringComparison.Ordinal);
            Assert.Contains("double-sided", materialSource, StringComparison.Ordinal);
        }
    }
}
