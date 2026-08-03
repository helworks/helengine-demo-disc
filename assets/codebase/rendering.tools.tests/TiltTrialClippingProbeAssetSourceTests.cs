namespace city.tests {
    /// <summary>
    /// Verifies the colored-face clipping probe factories keep their deterministic model, atlas, and textured material source contracts.
    /// </summary>
    public sealed class TiltTrialClippingProbeAssetSourceTests {
        [Fact]
        /// <summary>
        /// Validates the probe model source contains only the exact positive-Y submission required by the isolated fast-path hypothesis test.
        /// </summary>
        public void Clipping_probe_model_source_defines_top_face_only_contract() {
            string modelSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialClippingProbeModelFactory.cs";
            string modelSource = File.ReadAllText(modelSourcePath);

            Assert.Equal("models/rendering/tilt_trial/clipping_probe_face_colors.hasset", GetSingleStringConstantValue(modelSource, "ModelRelativePath"));
            Assert.Equal("Models.rendering.tilt_trial.ClippingProbeFaceColors", GetSingleStringConstantValue(modelSource, "ModelAssetId"));
            Assert.Equal("128", GetSingleIntegerConstantValue(modelSource, "TextureWidth"));
            Assert.Equal("64", GetSingleIntegerConstantValue(modelSource, "TextureHeight"));
            Assert.Contains("public ModelAsset CreateModelAsset()", modelSource, StringComparison.Ordinal);
            Assert.Contains("Id = ModelAssetId,", modelSource, StringComparison.Ordinal);
            Assert.Contains("Path.Combine(Path.GetFullPath(projectRootPath), \"assets\", ModelRelativePath.Replace('/', Path.DirectorySeparatorChar))", modelSource, StringComparison.Ordinal);
            Assert.Contains("global::helengine.editor.AssetSerializer.Serialize(stream, CreateModelAsset())", modelSource, StringComparison.Ordinal);
            Assert.Equal("newfloat3(-0.5f,0.5f,-0.5f),newfloat3(-0.5f,0.5f,0.5f),newfloat3(0.5f,0.5f,0.5f),newfloat3(0.5f,0.5f,-0.5f)", NormalizeSourceFragment(GetSingleArrayContents(modelSource, "Positions")));
            Assert.Equal(4, global::System.Text.RegularExpressions.Regex.Matches(GetSingleArrayContents(modelSource, "Positions"), @"new\s+float3\(").Count);
            Assert.Equal("newfloat3(0f,1f,0f),newfloat3(0f,1f,0f),newfloat3(0f,1f,0f),newfloat3(0f,1f,0f)", NormalizeSourceFragment(GetSingleArrayContents(modelSource, "Normals")));
            Assert.Equal(4, global::System.Text.RegularExpressions.Regex.Matches(GetSingleArrayContents(modelSource, "Normals"), @"new\s+float3\(").Count);
            Assert.Equal("newfloat2(49f/TextureWidth,37f/TextureHeight),newfloat2(78f/TextureWidth,37f/TextureHeight),newfloat2(78f/TextureWidth,58f/TextureHeight),newfloat2(49f/TextureWidth,58f/TextureHeight)", NormalizeSourceFragment(GetSingleArrayContents(modelSource, "TopFaceUv")));
            Assert.Equal(4, global::System.Text.RegularExpressions.Regex.Matches(GetSingleArrayContents(modelSource, "TopFaceUv"), @"new\s+float2\(").Count);
            Assert.Equal("..TopFaceUv", NormalizeSourceFragment(GetSingleArrayContents(modelSource, "TexCoords")));
            Assert.Equal("0,1,2,0,2,3", NormalizeSourceFragment(GetSingleArrayContents(modelSource, "Indices16")));
            string[] indices = GetSingleArrayContents(modelSource, "Indices16").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.Equal(new[] { "0", "1", "2", "0", "2", "3" }, indices);
            string submeshes = GetSingleArrayContents(modelSource, "Submeshes");
            Assert.Single(global::System.Text.RegularExpressions.Regex.Matches(submeshes, @"new\s+ModelSubmeshAsset\s*\{").Cast<global::System.Text.RegularExpressions.Match>());
            Assert.Equal("newModelSubmeshAsset{MaterialSlotName=\"DefaultMaterial\",IndexStart=0,IndexCount=6}", NormalizeSourceFragment(submeshes));
            Assert.Equal("newfloat3(-0.5f,-0.5f,-0.5f)", NormalizeSourceFragment(GetSingleFloat3AssignmentValue(modelSource, "BoundsMin")));
            Assert.Equal("newfloat3(0.5f,0.5f,0.5f)", NormalizeSourceFragment(GetSingleFloat3AssignmentValue(modelSource, "BoundsMax")));
            Assert.DoesNotMatch(@"\b(BackFaceUv|FrontFaceUv|RightFaceUv|LeftFaceUv|BottomFaceUv)\b", modelSource);
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

        /// <summary>
        /// Extracts the contents of one uniquely named C# collection-expression assignment from a model factory source file.
        /// </summary>
        /// <param name="source">Complete model factory source text.</param>
        /// <param name="memberName">Name of the assigned model member or static field.</param>
        /// <returns>Text between the collection-expression brackets.</returns>
        static string GetSingleArrayContents(string source, string memberName) {
            global::System.Text.RegularExpressions.MatchCollection matches = global::System.Text.RegularExpressions.Regex.Matches(
                source,
                $@"\b{memberName}\s*=\s*\[(?<contents>[^\]]*)\]\s*(?:,|;|(?=\}}))",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.Single(matches.Cast<global::System.Text.RegularExpressions.Match>());
            return matches[0].Groups["contents"].Value;
        }

        /// <summary>
        /// Extracts one exact public string constant value from the model factory source.
        /// </summary>
        /// <param name="source">Complete model factory source text.</param>
        /// <param name="constantName">Name of the required public string constant.</param>
        /// <returns>Unquoted constant value.</returns>
        static string GetSingleStringConstantValue(string source, string constantName) {
            global::System.Text.RegularExpressions.MatchCollection matches = global::System.Text.RegularExpressions.Regex.Matches(
                source,
                $@"public\s+const\s+string\s+{constantName}\s*=\s*""(?<value>[^""]*)""\s*;",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.Single(matches.Cast<global::System.Text.RegularExpressions.Match>());
            return matches[0].Groups["value"].Value;
        }

        /// <summary>
        /// Extracts one exact integer constant value from the model factory source.
        /// </summary>
        /// <param name="source">Complete model factory source text.</param>
        /// <param name="constantName">Name of the required integer constant.</param>
        /// <returns>Integer literal without its declaration syntax.</returns>
        static string GetSingleIntegerConstantValue(string source, string constantName) {
            global::System.Text.RegularExpressions.MatchCollection matches = global::System.Text.RegularExpressions.Regex.Matches(
                source,
                $@"const\s+int\s+{constantName}\s*=\s*(?<value>\d+)\s*;",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.Single(matches.Cast<global::System.Text.RegularExpressions.Match>());
            return matches[0].Groups["value"].Value;
        }

        /// <summary>
        /// Extracts one float3 assignment used for the model bounds from the model factory source.
        /// </summary>
        /// <param name="source">Complete model factory source text.</param>
        /// <param name="memberName">Name of the bounds member assignment.</param>
        /// <returns>Assigned float3 construction expression.</returns>
        static string GetSingleFloat3AssignmentValue(string source, string memberName) {
            global::System.Text.RegularExpressions.MatchCollection matches = global::System.Text.RegularExpressions.Regex.Matches(
                source,
                $@"\b{memberName}\s*=\s*(?<value>new\s+float3\([^\)]*\))\s*,",
                global::System.Text.RegularExpressions.RegexOptions.Singleline);
            Assert.Single(matches.Cast<global::System.Text.RegularExpressions.Match>());
            return matches[0].Groups["value"].Value;
        }

        /// <summary>
        /// Removes layout whitespace so source contract assertions compare semantic collection contents rather than formatting.
        /// </summary>
        /// <param name="sourceFragment">Source fragment whose layout whitespace is insignificant.</param>
        /// <returns>Source fragment with all whitespace removed.</returns>
        static string NormalizeSourceFragment(string sourceFragment) {
            return global::System.Text.RegularExpressions.Regex.Replace(sourceFragment, @"\s+", string.Empty);
        }
    }
}
