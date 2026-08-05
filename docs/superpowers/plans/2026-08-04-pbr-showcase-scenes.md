# PBR Showcase Scenes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add three new rendering showcase scenes (PBR material gallery, textured PBR showcase, multi-light shadow theater) to the end of the curated rendering scene list, demonstrating metallic/roughness/specular PBR shading and multi-light cast shadows, using downloaded CC0 textures for the textured showcase.

**Architecture:** Follow the existing `rendering.tools` showcase-scene pattern exactly: one material factory per scene that writes per-platform `.hasset` material documents via `GeneratedMaterialAssetWriteService` (windows/psp get real `standard-shader` metallic/roughness/specular fields, ps2/gamecube/ds get fixed-function fallback fields), one scene factory per scene that builds the live-authored entity tree, and registration of both in `RenderingSceneGenerator`. The PBR material gallery builds its 25 runtime materials in-memory directly (mirroring `ColoredCubeGridSceneFactory`), while the textured showcase's 2 materials go through `RenderingSceneAssetPreparationService`'s file-round-trip loader (mirroring `TiltTrialPlayerSphereMarbleMaterialFactory`), since it needs real imported texture asset ids.

**Tech Stack:** C#/.NET 9, xUnit, Helengine editor scene/material authoring APIs, PowerShell (texture download/extraction), JSON (`build_config.json` curation).

## Global Constraints

- Match the existing `rendering.tools` XML doc-comment convention: every public and private member gets a `/// <summary>` (and `<param>`/`<returns>` where applicable), exactly as in `ColoredCubeGridSceneFactory.cs`/`SpotlightStreetSliceSceneFactory.cs`.
- All argument validation uses the existing house style: `throw new ArgumentException("... must be provided.", nameof(x))` for blank strings, `throw new ArgumentNullException(nameof(x))` for null references.
- New scene ids: `scenes/rendering/pbr_material_gallery.helen` (13), `scenes/rendering/pbr_textured_showcase.helen` (14), `scenes/rendering/pbr_shadow_theater.helen` (15). No Nintendo DS companion scenes are authored for these three (they are windows/psp-focused PBR showcases; ps2/gamecube/ds still get valid fixed-function fallback materials but no dedicated DS scene variant, matching how `SpotlightStreetSliceSceneFactory` has no DS-specific companion either).
- Every new material factory populates all five of: `windows`, `psp`, `ps2`, `gamecube`, `ds` platform blocks (the full set `TiltTrialPlayerSphereMarbleMaterialFactory` populates).
- Downloaded texture license: ambientCG assets are CC0 (public domain, no attribution required) — safe to vendor directly into the repo.
- The engine's `standard-shader` schema has no normal-map or metallic-map field — only `texture-id` (diffuse), `roughness-texture-id`, and scalar `metallic`/`specular`/`roughness`. Do not attempt to wire a normal or metalness texture.
- Run `dotnet test` from the repo root (`C:\dev\helprojs\demodisc`) using the generated project files under `user_settings/generated_code/projects/`.

---

### Task 1: Stage the two PBR showcase textures

**Files:**
- Create: `assets/textures/rendering/pbr_textured_showcase/Metal032Albedo.jpg`
- Create: `assets/textures/rendering/pbr_textured_showcase/Metal032Roughness.jpg`
- Create: `assets/textures/rendering/pbr_textured_showcase/WoodFloor041Albedo.jpg`
- Create: `assets/textures/rendering/pbr_textured_showcase/WoodFloor041Roughness.jpg`
- Create: `assets/codebase/rendering.tools.tests/PbrTexturedShowcaseTextureAssetTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Produces: two texture pairs on disk at the paths above, consumed by `PbrTexturedShowcaseMaterialFactory` in Task 4.

- [ ] **Step 1: Download and inspect the ambientCG zips**

Run (PowerShell), downloading into the scratch temp folder rather than the repo:

    New-Item -ItemType Directory -Force -Path "$env:TEMP\pbr_showcase_download" | Out-Null
    Invoke-WebRequest -Uri "https://ambientcg.com/get?file=Metal032_1K-JPG.zip" -OutFile "$env:TEMP\pbr_showcase_download\Metal032_1K-JPG.zip"
    Invoke-WebRequest -Uri "https://ambientcg.com/get?file=WoodFloor041_1K-JPG.zip" -OutFile "$env:TEMP\pbr_showcase_download\WoodFloor041_1K-JPG.zip"
    Expand-Archive -Path "$env:TEMP\pbr_showcase_download\Metal032_1K-JPG.zip" -DestinationPath "$env:TEMP\pbr_showcase_download\Metal032" -Force
    Expand-Archive -Path "$env:TEMP\pbr_showcase_download\WoodFloor041_1K-JPG.zip" -DestinationPath "$env:TEMP\pbr_showcase_download\WoodFloor041" -Force
    Get-ChildItem "$env:TEMP\pbr_showcase_download\Metal032"
    Get-ChildItem "$env:TEMP\pbr_showcase_download\WoodFloor041"

Expected: each listing shows files following ambientCG's standard naming (`Metal032_1K-JPG_Color.jpg`, `Metal032_1K-JPG_Roughness.jpg`, plus Normal/AmbientOcclusion/etc. that this task does not need — same pattern for `WoodFloor041_1K-JPG_Color.jpg`/`WoodFloor041_1K-JPG_Roughness.jpg`). If either listing uses different suffixes than `_Color`/`_Roughness`, use the actual filenames shown in place of those below.

- [ ] **Step 2: Copy and rename only the Color and Roughness maps into the project**

Run (PowerShell):

    New-Item -ItemType Directory -Force -Path "assets/textures/rendering/pbr_textured_showcase" | Out-Null
    Copy-Item "$env:TEMP\pbr_showcase_download\Metal032\Metal032_1K-JPG_Color.jpg" "assets/textures/rendering/pbr_textured_showcase/Metal032Albedo.jpg"
    Copy-Item "$env:TEMP\pbr_showcase_download\Metal032\Metal032_1K-JPG_Roughness.jpg" "assets/textures/rendering/pbr_textured_showcase/Metal032Roughness.jpg"
    Copy-Item "$env:TEMP\pbr_showcase_download\WoodFloor041\WoodFloor041_1K-JPG_Color.jpg" "assets/textures/rendering/pbr_textured_showcase/WoodFloor041Albedo.jpg"
    Copy-Item "$env:TEMP\pbr_showcase_download\WoodFloor041\WoodFloor041_1K-JPG_Roughness.jpg" "assets/textures/rendering/pbr_textured_showcase/WoodFloor041Roughness.jpg"
    Remove-Item -Recurse -Force "$env:TEMP\pbr_showcase_download"

- [ ] **Step 3: Write the failing texture-presence test**

Create `assets/codebase/rendering.tools.tests/PbrTexturedShowcaseTextureAssetTests.cs`:

    namespace city.tests {
        public sealed class PbrTexturedShowcaseTextureAssetTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Theory]
            [InlineData("Metal032Albedo.jpg")]
            [InlineData("Metal032Roughness.jpg")]
            [InlineData("WoodFloor041Albedo.jpg")]
            [InlineData("WoodFloor041Roughness.jpg")]
            public void Downloaded_pbr_showcase_texture_exists_and_is_a_real_image_file(string fileName) {
                string path = Path.Combine(ProjectRootPath, "assets", "textures", "rendering", "pbr_textured_showcase", fileName);
                Assert.True(File.Exists(path), $"Expected '{path}' to exist.");
                Assert.True(new FileInfo(path).Length > 10_000, $"Expected '{path}' to be a real downloaded JPG, not a placeholder.");
            }
        }
    }

- [ ] **Step 4: Run the test and verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrTexturedShowcaseTextureAssetTests`

Expected: PASS (files were already staged in Steps 1-2; this test guards against them being missing or truncated).

- [ ] **Step 5: Commit**

    git add assets/textures/rendering/pbr_textured_showcase assets/codebase/rendering.tools.tests/PbrTexturedShowcaseTextureAssetTests.cs
    git commit -m "feat: stage CC0 PBR showcase textures from ambientCG"

---

### Task 2: PbrMaterialGalleryMaterialFactory

**Files:**
- Create: `assets/codebase/rendering.tools/PbrMaterialGalleryMaterialFactory.cs`
- Create: `assets/codebase/rendering.tools.tests/PbrMaterialGalleryMaterialFactorySourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Produces: `public const int MetallicSteps = 5`, `public const int RoughnessSteps = 5`, `public static int ResolveIndex(int metallicIndex, int roughnessIndex)`, `public static float ResolveMetallic(int metallicIndex)`, `public static float ResolveRoughness(int roughnessIndex)`, `public void WriteMaterialAssets(string projectRootPath)`, `public RuntimeMaterial[] CreateRuntimeMaterials()` — all consumed by `PbrMaterialGallerySceneFactory` (Task 3), `PbrShadowTheaterSceneFactory` (Task 6), and `RenderingSceneGenerator` (Task 8).

- [ ] **Step 1: Write the failing source test**

Create `assets/codebase/rendering.tools.tests/PbrMaterialGalleryMaterialFactorySourceTests.cs`:

    namespace city.tests {
        public sealed class PbrMaterialGalleryMaterialFactorySourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Gallery_material_factory_sweeps_five_metallic_and_five_roughness_steps() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrMaterialGalleryMaterialFactory.cs");
                Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("public const int MetallicSteps = 5;", source, StringComparison.Ordinal);
                Assert.Contains("public const int RoughnessSteps = 5;", source, StringComparison.Ordinal);
                Assert.Contains("public static int ResolveIndex(int metallicIndex, int roughnessIndex)", source, StringComparison.Ordinal);
                Assert.Contains("public RuntimeMaterial[] CreateRuntimeMaterials()", source, StringComparison.Ordinal);
                Assert.Contains("public void WriteMaterialAssets(string projectRootPath)", source, StringComparison.Ordinal);
                Assert.Contains("StandardMaterialMetallicDefaults.MetallicBufferName", source, StringComparison.Ordinal);
                Assert.Contains("StandardMaterialRoughnessDefaults.RoughnessBufferName", source, StringComparison.Ordinal);
                Assert.Contains("StandardMaterialSpecularDefaults.SpecularBufferName", source, StringComparison.Ordinal);
                Assert.Contains("materials/rendering/pbr_gallery", source, StringComparison.Ordinal);
            }
        }
    }

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrMaterialGalleryMaterialFactorySourceTests`

Expected: FAIL because `PbrMaterialGalleryMaterialFactory.cs` does not exist yet.

- [ ] **Step 3: Implement PbrMaterialGalleryMaterialFactory**

Create `assets/codebase/rendering.tools/PbrMaterialGalleryMaterialFactory.cs`:

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
            readonly GeneratedMaterialAssetWriteService MaterialWriteService;

            /// <summary>
            /// Initializes one PBR material gallery material factory.
            /// </summary>
            public PbrMaterialGalleryMaterialFactory() {
                MaterialWriteService = new GeneratedMaterialAssetWriteService();
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
                ShaderAsset shaderAsset = helengine.editor.EditorBuiltInShaderAssetLibrary.LoadShaderAsset(Core.Instance.RenderManager3D, StandardShaderSourceFileName);
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

                RuntimeMaterial runtimeMaterial = Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
                StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(runtimeMaterial));
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
                MaterialWriteService.WriteMaterial(projectRootPath, relativePath, CreateGeneratedMaterialDefinition(metallicIndex, roughnessIndex));
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

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrMaterialGalleryMaterialFactorySourceTests`

Expected: PASS.

- [ ] **Step 5: Commit**

    git add assets/codebase/rendering.tools/PbrMaterialGalleryMaterialFactory.cs assets/codebase/rendering.tools.tests/PbrMaterialGalleryMaterialFactorySourceTests.cs
    git commit -m "feat: add PBR material gallery material factory"

---

### Task 3: PbrMaterialGallerySceneFactory

**Files:**
- Create: `assets/codebase/rendering.tools/PbrMaterialGallerySceneFactory.cs`
- Create: `assets/codebase/rendering.tools.tests/PbrMaterialGallerySceneFactorySourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Consumes: `PbrMaterialGalleryMaterialFactory.MetallicSteps`, `.RoughnessSteps`, `.ResolveIndex`, `.ResolveMetallic`, `.ResolveRoughness` (Task 2).
- Produces: `public const string SceneId = "scenes/rendering/pbr_material_gallery.helen";` and `public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel planeModel, RuntimeModel sphereModel, RuntimeMaterial groundMaterial, RuntimeMaterial[] galleryMaterials)`, consumed by `RenderingSceneGenerator` (Task 8).

- [ ] **Step 1: Write the failing source test**

Create `assets/codebase/rendering.tools.tests/PbrMaterialGallerySceneFactorySourceTests.cs`:

    namespace city.tests {
        public sealed class PbrMaterialGallerySceneFactorySourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Gallery_scene_factory_declares_its_scene_id_and_five_light_lit_grid() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrMaterialGallerySceneFactory.cs");
                Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("public const string SceneId = \"scenes/rendering/pbr_material_gallery.helen\";", source, StringComparison.Ordinal);
                Assert.Contains("public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel planeModel, RuntimeModel sphereModel, RuntimeMaterial groundMaterial, RuntimeMaterial[] galleryMaterials)", source, StringComparison.Ordinal);
                Assert.Contains("new DirectionalLightComponent", source, StringComparison.Ordinal);
                Assert.Contains("new AmbientLightComponent", source, StringComparison.Ordinal);
                Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
                Assert.Contains("\"13. PBR Gallery\"", source, StringComparison.Ordinal);
                Assert.Contains("PbrMaterialGalleryMaterialFactory.ResolveIndex", source, StringComparison.Ordinal);
            }
        }
    }

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrMaterialGallerySceneFactorySourceTests`

Expected: FAIL because `PbrMaterialGallerySceneFactory.cs` does not exist yet.

- [ ] **Step 3: Implement PbrMaterialGallerySceneFactory**

Create `assets/codebase/rendering.tools/PbrMaterialGallerySceneFactory.cs`:

    using city.menu;
    using helengine;

    namespace city.rendering.tools {
        /// <summary>
        /// Builds the authored PBR material gallery scene: a five by five sphere grid sweeping metallic and roughness under a three-light rig.
        /// </summary>
        public sealed class PbrMaterialGallerySceneFactory {
            /// <summary>
            /// Stable scene id used by the generated PBR material gallery asset.
            /// </summary>
            public const string SceneId = "scenes/rendering/pbr_material_gallery.helen";

            /// <summary>
            /// World-space spacing between adjacent gallery spheres, in both grid axes.
            /// </summary>
            const float SphereSpacing = 2.4f;

            /// <summary>
            /// Local uniform scale applied to every gallery sphere.
            /// </summary>
            const float SphereScale = 1.6f;

            /// <summary>
            /// Local Y position every gallery sphere rests at, equal to its scaled radius.
            /// </summary>
            const float SphereRestY = SphereScale / 2f;

            /// <summary>
            /// Creates the canonical PBR material gallery live-authored scene definition.
            /// </summary>
            /// <param name="planeModel">Generated plane runtime model used by the ground mesh.</param>
            /// <param name="sphereModel">Generated sphere runtime model shared by every gallery sphere.</param>
            /// <param name="groundMaterial">Runtime material used by the ground mesh.</param>
            /// <param name="galleryMaterials">Twenty-five runtime materials ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</param>
            /// <returns>Live-authored scene definition for the PBR material gallery showcase.</returns>
            public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel planeModel, RuntimeModel sphereModel, RuntimeMaterial groundMaterial, RuntimeMaterial[] galleryMaterials) {
                if (planeModel == null) {
                    throw new ArgumentNullException(nameof(planeModel));
                } else if (sphereModel == null) {
                    throw new ArgumentNullException(nameof(sphereModel));
                } else if (groundMaterial == null) {
                    throw new ArgumentNullException(nameof(groundMaterial));
                } else if (galleryMaterials == null) {
                    throw new ArgumentNullException(nameof(galleryMaterials));
                } else if (galleryMaterials.Length != PbrMaterialGalleryMaterialFactory.MetallicSteps * PbrMaterialGalleryMaterialFactory.RoughnessSteps) {
                    throw new ArgumentException("PBR material gallery generation requires twenty-five runtime materials.", nameof(galleryMaterials));
                }

                Entity[] sphereEntities = CreateSphereEntities(sphereModel, galleryMaterials);
                Entity[] rootEntities = new Entity[sphereEntities.Length + 6];
                rootEntities[0] = CreateCameraEntity();
                rootEntities[1] = CreateUiEntity();
                rootEntities[2] = CreateDirectionalLightEntity();
                rootEntities[3] = CreateDirectionalFillLightEntity();
                rootEntities[4] = CreateAmbientLightEntity();
                rootEntities[5] = CreateGroundEntity(planeModel, groundMaterial);
                Array.Copy(sphereEntities, 0, rootEntities, 6, sphereEntities.Length);

                return new GeneratedAuthoringSceneDefinition {
                    SceneId = SceneId,
                    SceneSettings = new SceneSettingsAsset(),
                    NintendoDsScene = new GeneratedDsSceneDefinition {
                        UseDefaultBottomOverlay = true,
                        BottomScreenRootEntities = Array.Empty<Entity>()
                    },
                    RootEntities = rootEntities
                };
            }

            /// <summary>
            /// Creates the authored camera entity for the PBR material gallery scene.
            /// </summary>
            /// <returns>Live authored camera entity.</returns>
            Entity CreateCameraEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(0f, -0.42f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryCamera");
                entity.LocalPosition = new float3(0f, 10f, 16f);
                entity.LocalScale = float3.One;
                entity.LocalOrientation = orientation;
                entity.AddComponent(new CameraComponent {
                    CameraDrawOrder = 0,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    Viewport = new float4(0f, 0f, 1f, 1f),
                    NearPlaneDistance = 0.1f,
                    FarPlaneDistance = 96f,
                    ClearSettings = new CameraClearSettings(
                        true,
                        new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                        true,
                        1f,
                        false,
                        0),
                    RenderSettings = new CameraRenderSettings {
                        DepthPrepassMode = DepthPrepassMode.Auto,
                        ShadowDistance = 40f,
                        PostProcessTier = PostProcessTier.Disabled
                    }
                });
                entity.AddComponent(new DemoDiscReturnToMenuComponent());
                return entity;
            }

            /// <summary>
            /// Creates the authored UI root entity for the PBR material gallery scene.
            /// </summary>
            /// <returns>Live authored UI entity.</returns>
            Entity CreateUiEntity() {
                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryUi");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.AddComponent(new FPSComponent {
                    Font = ResolveRequiredEditorFont(),
                    FontScale = 2f
                });
                PspFpsComponentOverrideService.Apply(entity);
                DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
                sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "13. PBR Gallery");
                return entity;
            }

            /// <summary>
            /// Creates the authored shadow-casting sun for the PBR material gallery scene.
            /// </summary>
            /// <returns>Live authored directional light entity.</returns>
            Entity CreateDirectionalLightEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(-0.6f, -0.95f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGallerySun");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = new float3(0f, 8f, 0f);
                entity.LocalOrientation = orientation;
                entity.AddComponent(new DirectionalLightComponent {
                    Color = new float4(1f, 0.97f, 0.92f, 1f),
                    Intensity = 1.15f,
                    ShadowsEnabled = true,
                    ShadowMapMode = ShadowMapMode.Forced,
                    ShadowStrength = 0.95f,
                    ShadowDistance = 40f
                });
                return entity;
            }

            /// <summary>
            /// Creates one weaker directional fill light that lifts every sphere's unlit hemisphere without adding a second shadow pass.
            /// </summary>
            /// <returns>Live authored fill-light entity.</returns>
            Entity CreateDirectionalFillLightEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(2.45f, -0.32f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryFill");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = new float3(0f, 6f, 0f);
                entity.LocalOrientation = orientation;
                entity.AddComponent(new DirectionalLightComponent {
                    Color = new float4(0.78f, 0.84f, 1f, 1f),
                    Intensity = 0.7f,
                    ShadowsEnabled = false,
                    ShadowMapMode = ShadowMapMode.Disabled,
                    ShadowStrength = 0f,
                    ShadowDistance = 0f
                });
                return entity;
            }

            /// <summary>
            /// Creates one low-intensity ambient light so spheres facing away from the key lights do not collapse to flat black.
            /// </summary>
            /// <returns>Live authored ambient-light entity.</returns>
            Entity CreateAmbientLightEntity() {
                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryAmbient");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = float3.Zero;
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new AmbientLightComponent {
                    Color = new float4(1f, 0.95f, 0.82f, 1f),
                    Intensity = 0.18f,
                    ShadowsEnabled = false,
                    ShadowMapMode = ShadowMapMode.Disabled,
                    ShadowStrength = 0f
                });
                return entity;
            }

            /// <summary>
            /// Creates the authored ground receiver mesh for the PBR material gallery scene.
            /// </summary>
            /// <param name="model">Runtime plane model used by the mesh.</param>
            /// <param name="material">Runtime material used by the mesh.</param>
            /// <returns>Live authored ground entity.</returns>
            Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGalleryGround");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = float3.Zero;
                entity.LocalScale = new float3(14f, 1f, 14f);
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new MeshComponent {
                    Model = model,
                    Materials = new[] { material },
                    RenderOrder3D = 0
                });
                return entity;
            }

            /// <summary>
            /// Creates the twenty-five authored gallery sphere entities.
            /// </summary>
            /// <param name="sphereModel">Generated sphere runtime model shared by every gallery sphere.</param>
            /// <param name="galleryMaterials">Twenty-five runtime materials ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</param>
            /// <returns>Live authored sphere entities ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</returns>
            Entity[] CreateSphereEntities(RuntimeModel sphereModel, RuntimeMaterial[] galleryMaterials) {
                Entity[] sphereEntities = new Entity[galleryMaterials.Length];
                for (int metallicIndex = 0; metallicIndex < PbrMaterialGalleryMaterialFactory.MetallicSteps; metallicIndex++) {
                    for (int roughnessIndex = 0; roughnessIndex < PbrMaterialGalleryMaterialFactory.RoughnessSteps; roughnessIndex++) {
                        int flatIndex = PbrMaterialGalleryMaterialFactory.ResolveIndex(metallicIndex, roughnessIndex);
                        float x = (roughnessIndex - 2) * SphereSpacing;
                        float z = (metallicIndex - 2) * SphereSpacing;
                        sphereEntities[flatIndex] = CreateSphereEntity(flatIndex, sphereModel, galleryMaterials[flatIndex], new float3(x, SphereRestY, z));
                    }
                }

                return sphereEntities;
            }

            /// <summary>
            /// Creates one authored gallery sphere entity.
            /// </summary>
            /// <param name="flatIndex">Stable zero-based gallery index.</param>
            /// <param name="sphereModel">Generated sphere runtime model.</param>
            /// <param name="material">Runtime material assigned to the sphere.</param>
            /// <param name="localPosition">Authored local position for the sphere.</param>
            /// <returns>Live authored sphere entity.</returns>
            Entity CreateSphereEntity(int flatIndex, RuntimeModel sphereModel, RuntimeMaterial material, float3 localPosition) {
                Entity entity = Core.Instance.EntityFactory.Create("PbrMaterialGallerySphere" + flatIndex.ToString("00"));
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = localPosition;
                entity.LocalScale = new float3(SphereScale, SphereScale, SphereScale);
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new MeshComponent {
                    Model = sphereModel,
                    Materials = new[] { material },
                    RenderOrder3D = 0
                });
                return entity;
            }

            /// <summary>
            /// Resolves the editor font that should back the generated FPS overlay during live authoring.
            /// </summary>
            /// <returns>Editor font asset required by the FPS component.</returns>
            FontAsset ResolveRequiredEditorFont() {
                if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                    throw new InvalidOperationException("A default editor font must be loaded before the PBR material gallery scene can be generated.");
                }

                return editorCore.DefaultFontAssetForEditor;
            }
        }
    }

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrMaterialGallerySceneFactorySourceTests`

Expected: PASS.

- [ ] **Step 5: Commit**

    git add assets/codebase/rendering.tools/PbrMaterialGallerySceneFactory.cs assets/codebase/rendering.tools.tests/PbrMaterialGallerySceneFactorySourceTests.cs
    git commit -m "feat: add PBR material gallery scene factory"

---

### Task 4: PbrTexturedShowcaseMaterialFactory

**Files:**
- Create: `assets/codebase/rendering.tools/PbrTexturedShowcaseMaterialFactory.cs`
- Create: `assets/codebase/rendering.tools.tests/PbrTexturedShowcaseMaterialFactorySourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Consumes: the four texture files staged in Task 1.
- Produces: `public const string MetalMaterialRelativePath`, `public const string WoodMaterialRelativePath`, `public void WriteMaterialAssets(string projectRootPath)` — consumed by `RenderingSceneAssetPreparationService` (Task 7).

- [ ] **Step 1: Write the failing source test**

Create `assets/codebase/rendering.tools.tests/PbrTexturedShowcaseMaterialFactorySourceTests.cs`:

    namespace city.tests {
        public sealed class PbrTexturedShowcaseMaterialFactorySourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Textured_showcase_material_factory_references_downloaded_textures_and_metallic_split() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrTexturedShowcaseMaterialFactory.cs");
                Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("textures/rendering/pbr_textured_showcase/Metal032Albedo.jpg", source, StringComparison.Ordinal);
                Assert.Contains("textures/rendering/pbr_textured_showcase/Metal032Roughness.jpg", source, StringComparison.Ordinal);
                Assert.Contains("textures/rendering/pbr_textured_showcase/WoodFloor041Albedo.jpg", source, StringComparison.Ordinal);
                Assert.Contains("textures/rendering/pbr_textured_showcase/WoodFloor041Roughness.jpg", source, StringComparison.Ordinal);
                Assert.Contains("public const string MetalMaterialRelativePath", source, StringComparison.Ordinal);
                Assert.Contains("public const string WoodMaterialRelativePath", source, StringComparison.Ordinal);
                Assert.Contains("public void WriteMaterialAssets(string projectRootPath)", source, StringComparison.Ordinal);
                Assert.Contains("\"1.0\"", source, StringComparison.Ordinal);
                Assert.Contains("\"0.0\"", source, StringComparison.Ordinal);
            }
        }
    }

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrTexturedShowcaseMaterialFactorySourceTests`

Expected: FAIL because `PbrTexturedShowcaseMaterialFactory.cs` does not exist yet.

- [ ] **Step 3: Implement PbrTexturedShowcaseMaterialFactory**

Create `assets/codebase/rendering.tools/PbrTexturedShowcaseMaterialFactory.cs`, mirroring `TiltTrialPlayerSphereMarbleMaterialFactory.cs`'s structure with two materials instead of one:

    using helengine;
    using helengine.editor;
    using System.Reflection;

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
            public PbrTexturedShowcaseMaterialFactory() {
                MaterialWriteService = new GeneratedMaterialAssetWriteService();
            }

            /// <summary>
            /// Writes the authored scuffed-metal and wood-plank material settings required by the textured showcase scene.
            /// </summary>
            /// <param name="projectRootPath">Absolute or relative city project root path.</param>
            public void WriteMaterialAssets(string projectRootPath) {
                if (string.IsNullOrWhiteSpace(projectRootPath)) {
                    throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
                }

                string metalDiffuseTextureAssetId = ResolveTextureAssetId(projectRootPath, MetalDiffuseTextureRelativePath);
                string metalRoughnessTextureAssetId = ResolveTextureAssetId(projectRootPath, MetalRoughnessTextureRelativePath);
                MaterialWriteService.WriteMaterial(
                    projectRootPath,
                    MetalMaterialRelativePath,
                    CreateDefinition(MetalMaterialAssetId, metalDiffuseTextureAssetId, metalRoughnessTextureAssetId, metallic: "1.0"));

                string woodDiffuseTextureAssetId = ResolveTextureAssetId(projectRootPath, WoodDiffuseTextureRelativePath);
                string woodRoughnessTextureAssetId = ResolveTextureAssetId(projectRootPath, WoodRoughnessTextureRelativePath);
                MaterialWriteService.WriteMaterial(
                    projectRootPath,
                    WoodMaterialRelativePath,
                    CreateDefinition(WoodMaterialAssetId, woodDiffuseTextureAssetId, woodRoughnessTextureAssetId, metallic: "0.0"));
            }

            /// <summary>
            /// Resolves one imported texture asset id that should back an authored showcase material.
            /// </summary>
            /// <param name="projectRootPath">Absolute or relative city project root path.</param>
            /// <param name="relativeTexturePath">Project-relative source texture path.</param>
            /// <returns>Imported texture asset id persisted by the shared editor import pipeline.</returns>
            string ResolveTextureAssetId(string projectRootPath, string relativeTexturePath) {
                string fullProjectRootPath = Path.GetFullPath(projectRootPath);
                string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
                AssetImportManager importManager = CreateAssetImportManager(fullProjectRootPath, assetsRootPath);
                string sourceTexturePath = Path.Combine(assetsRootPath, relativeTexturePath.Replace('/', Path.DirectorySeparatorChar));
                TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(sourceTexturePath);
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

            /// <summary>
            /// Builds one asset import manager initialized with the editor host's default importer registrations.
            /// </summary>
            AssetImportManager CreateAssetImportManager(string projectRootPath, string assetsRootPath) {
                ContentManager contentManager = new ContentManager(new HostFileSystemContentStreamSource(assetsRootPath));
                AssetImportManager importManager = new AssetImportManager(projectRootPath, contentManager);
                IReadOnlyList<IAssetImporterRegistration> importers = CreateDefaultImporters();
                for (int index = 0; index < importers.Count; index++) {
                    importers[index].Register(importManager);
                }

                importManager.GenerateMissingImportSettings();
                return importManager;
            }

            /// <summary>
            /// Creates the default importer registrations exposed by the editor host assembly.
            /// </summary>
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

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrTexturedShowcaseMaterialFactorySourceTests`

Expected: PASS.

- [ ] **Step 5: Commit**

    git add assets/codebase/rendering.tools/PbrTexturedShowcaseMaterialFactory.cs assets/codebase/rendering.tools.tests/PbrTexturedShowcaseMaterialFactorySourceTests.cs
    git commit -m "feat: add PBR textured showcase material factory"

---

### Task 5: PbrTexturedShowcaseSceneFactory

**Files:**
- Create: `assets/codebase/rendering.tools/PbrTexturedShowcaseSceneFactory.cs`
- Create: `assets/codebase/rendering.tools.tests/PbrTexturedShowcaseSceneFactorySourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Produces: `public const string SceneId = "scenes/rendering/pbr_textured_showcase.helen";` and `public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel planeModel, RuntimeMaterial groundMaterial, RuntimeMaterial metalMaterial, RuntimeMaterial woodMaterial)`, consumed by `RenderingSceneGenerator` (Task 8).

- [ ] **Step 1: Write the failing source test**

Create `assets/codebase/rendering.tools.tests/PbrTexturedShowcaseSceneFactorySourceTests.cs`:

    namespace city.tests {
        public sealed class PbrTexturedShowcaseSceneFactorySourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Textured_showcase_scene_factory_declares_its_scene_id_and_two_props() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrTexturedShowcaseSceneFactory.cs");
                Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("public const string SceneId = \"scenes/rendering/pbr_textured_showcase.helen\";", source, StringComparison.Ordinal);
                Assert.Contains("public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel planeModel, RuntimeMaterial groundMaterial, RuntimeMaterial metalMaterial, RuntimeMaterial woodMaterial)", source, StringComparison.Ordinal);
                Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
                Assert.Contains("\"14. PBR Textures\"", source, StringComparison.Ordinal);
                Assert.Contains("ShadowsEnabled = true", source, StringComparison.Ordinal);
            }
        }
    }

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrTexturedShowcaseSceneFactorySourceTests`

Expected: FAIL because `PbrTexturedShowcaseSceneFactory.cs` does not exist yet.

- [ ] **Step 3: Implement PbrTexturedShowcaseSceneFactory**

Create `assets/codebase/rendering.tools/PbrTexturedShowcaseSceneFactory.cs`:

    using city.menu;
    using helengine;

    namespace city.rendering.tools {
        /// <summary>
        /// Builds the authored PBR textured showcase scene: a scuffed-metal prop and a wood-plank prop lit by one shadow-casting sun.
        /// </summary>
        public sealed class PbrTexturedShowcaseSceneFactory {
            /// <summary>
            /// Stable scene id used by the generated PBR textured showcase asset.
            /// </summary>
            public const string SceneId = "scenes/rendering/pbr_textured_showcase.helen";

            /// <summary>
            /// Creates the canonical PBR textured showcase live-authored scene definition.
            /// </summary>
            /// <param name="cubeModel">Generated cube runtime model used by both hero props.</param>
            /// <param name="planeModel">Generated plane runtime model used by the ground mesh.</param>
            /// <param name="groundMaterial">Runtime material used by the ground mesh.</param>
            /// <param name="metalMaterial">Runtime scuffed-metal material used by the metal prop.</param>
            /// <param name="woodMaterial">Runtime wood-plank material used by the wood prop.</param>
            /// <returns>Live-authored scene definition for the PBR textured showcase.</returns>
            public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel planeModel, RuntimeMaterial groundMaterial, RuntimeMaterial metalMaterial, RuntimeMaterial woodMaterial) {
                if (cubeModel == null) {
                    throw new ArgumentNullException(nameof(cubeModel));
                } else if (planeModel == null) {
                    throw new ArgumentNullException(nameof(planeModel));
                } else if (groundMaterial == null) {
                    throw new ArgumentNullException(nameof(groundMaterial));
                } else if (metalMaterial == null) {
                    throw new ArgumentNullException(nameof(metalMaterial));
                } else if (woodMaterial == null) {
                    throw new ArgumentNullException(nameof(woodMaterial));
                }

                return new GeneratedAuthoringSceneDefinition {
                    SceneId = SceneId,
                    SceneSettings = new SceneSettingsAsset(),
                    NintendoDsScene = new GeneratedDsSceneDefinition {
                        UseDefaultBottomOverlay = true,
                        BottomScreenRootEntities = Array.Empty<Entity>()
                    },
                    RootEntities = new[] {
                        CreateCameraEntity(),
                        CreateUiEntity(),
                        CreateDirectionalLightEntity(),
                        CreateGroundEntity(planeModel, groundMaterial),
                        CreatePropEntity("PbrTexturedShowcaseMetalProp", new float3(-2.6f, 1.2f, 0f), new float3(2.4f, 2.4f, 2.4f), cubeModel, metalMaterial),
                        CreatePropEntity("PbrTexturedShowcaseWoodProp", new float3(2.6f, 1.2f, 0f), new float3(2.4f, 2.4f, 2.4f), cubeModel, woodMaterial)
                    }
                };
            }

            /// <summary>
            /// Creates the authored camera entity for the PBR textured showcase scene.
            /// </summary>
            /// <returns>Live authored camera entity.</returns>
            Entity CreateCameraEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(0f, -0.3f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseCamera");
                entity.LocalPosition = new float3(0f, 5f, 10f);
                entity.LocalScale = float3.One;
                entity.LocalOrientation = orientation;
                entity.AddComponent(new CameraComponent {
                    CameraDrawOrder = 0,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    Viewport = new float4(0f, 0f, 1f, 1f),
                    NearPlaneDistance = 0.1f,
                    FarPlaneDistance = 96f,
                    ClearSettings = new CameraClearSettings(
                        true,
                        new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                        true,
                        1f,
                        false,
                        0),
                    RenderSettings = new CameraRenderSettings {
                        DepthPrepassMode = DepthPrepassMode.Auto,
                        ShadowDistance = 30f,
                        PostProcessTier = PostProcessTier.Disabled
                    }
                });
                entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                    OrbitCenter = new float3(0f, 1.2f, 0f),
                    AutoYawSpeedRadians = 0.08f
                });
                entity.AddComponent(new DemoDiscReturnToMenuComponent());
                return entity;
            }

            /// <summary>
            /// Creates the authored UI root entity for the PBR textured showcase scene.
            /// </summary>
            /// <returns>Live authored UI entity.</returns>
            Entity CreateUiEntity() {
                Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseUi");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.AddComponent(new FPSComponent {
                    Font = ResolveRequiredEditorFont(),
                    FontScale = 2f
                });
                PspFpsComponentOverrideService.Apply(entity);
                DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
                sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "14. PBR Textures");
                return entity;
            }

            /// <summary>
            /// Creates the authored shadow-casting sun for the PBR textured showcase scene.
            /// </summary>
            /// <returns>Live authored directional light entity.</returns>
            Entity CreateDirectionalLightEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(-0.5f, -0.85f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseSun");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = new float3(0f, 7f, 0f);
                entity.LocalOrientation = orientation;
                entity.AddComponent(new DirectionalLightComponent {
                    Color = new float4(1f, 0.97f, 0.92f, 1f),
                    Intensity = 1.1f,
                    ShadowsEnabled = true,
                    ShadowMapMode = ShadowMapMode.Forced,
                    ShadowStrength = 0.9f,
                    ShadowDistance = 30f
                });
                return entity;
            }

            /// <summary>
            /// Creates the authored ground receiver mesh for the PBR textured showcase scene.
            /// </summary>
            /// <param name="model">Runtime plane model used by the mesh.</param>
            /// <param name="material">Runtime material used by the mesh.</param>
            /// <returns>Live authored ground entity.</returns>
            Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
                Entity entity = Core.Instance.EntityFactory.Create("PbrTexturedShowcaseGround");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = float3.Zero;
                entity.LocalScale = new float3(12f, 1f, 12f);
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new MeshComponent {
                    Model = model,
                    Materials = new[] { material },
                    RenderOrder3D = 0
                });
                return entity;
            }

            /// <summary>
            /// Creates one authored hero prop entity for the PBR textured showcase scene.
            /// </summary>
            /// <param name="name">Stable entity name.</param>
            /// <param name="localPosition">Local position assigned to the entity.</param>
            /// <param name="localScale">Local scale assigned to the entity.</param>
            /// <param name="model">Runtime cube model used by the mesh.</param>
            /// <param name="material">Runtime material used by the mesh.</param>
            /// <returns>Live authored prop entity.</returns>
            Entity CreatePropEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
                Entity entity = Core.Instance.EntityFactory.Create(name);
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = localPosition;
                entity.LocalScale = localScale;
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new MeshComponent {
                    Model = model,
                    Materials = new[] { material },
                    RenderOrder3D = 0
                });
                return entity;
            }

            /// <summary>
            /// Resolves the editor font that should back the generated FPS overlay during live authoring.
            /// </summary>
            /// <returns>Editor font asset required by the FPS component.</returns>
            FontAsset ResolveRequiredEditorFont() {
                if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                    throw new InvalidOperationException("A default editor font must be loaded before the PBR textured showcase scene can be generated.");
                }

                return editorCore.DefaultFontAssetForEditor;
            }
        }
    }

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrTexturedShowcaseSceneFactorySourceTests`

Expected: PASS.

- [ ] **Step 5: Commit**

    git add assets/codebase/rendering.tools/PbrTexturedShowcaseSceneFactory.cs assets/codebase/rendering.tools.tests/PbrTexturedShowcaseSceneFactorySourceTests.cs
    git commit -m "feat: add PBR textured showcase scene factory"

---

### Task 6: PbrShadowTheaterSceneFactory

**Files:**
- Create: `assets/codebase/rendering.tools/PbrShadowTheaterSceneFactory.cs`
- Create: `assets/codebase/rendering.tools.tests/PbrShadowTheaterSceneFactorySourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Consumes: `PbrMaterialGalleryMaterialFactory.ResolveIndex` (Task 2), the 25-element `RuntimeMaterial[]` produced by `PbrMaterialGalleryMaterialFactory.CreateRuntimeMaterials()`.
- Produces: `public const string SceneId = "scenes/rendering/pbr_shadow_theater.helen";` and `public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel sphereModel, RuntimeMaterial pedestalMaterial, RuntimeMaterial[] galleryMaterials)`, consumed by `RenderingSceneGenerator` (Task 8).

- [ ] **Step 1: Write the failing source test**

Create `assets/codebase/rendering.tools.tests/PbrShadowTheaterSceneFactorySourceTests.cs`:

    namespace city.tests {
        public sealed class PbrShadowTheaterSceneFactorySourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Shadow_theater_scene_factory_declares_its_scene_id_and_two_shadow_casting_lights() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrShadowTheaterSceneFactory.cs");
                Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("public const string SceneId = \"scenes/rendering/pbr_shadow_theater.helen\";", source, StringComparison.Ordinal);
                Assert.Contains("public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel sphereModel, RuntimeMaterial pedestalMaterial, RuntimeMaterial[] galleryMaterials)", source, StringComparison.Ordinal);
                Assert.Contains("new DirectionalLightComponent", source, StringComparison.Ordinal);
                Assert.Contains("new SpotLightComponent", source, StringComparison.Ordinal);
                Assert.Contains("PbrMaterialGalleryMaterialFactory.ResolveIndex", source, StringComparison.Ordinal);
                Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
                Assert.Contains("\"15. PBR Shadow Theater\"", source, StringComparison.Ordinal);
                int shadowsEnabledCount = System.Text.RegularExpressions.Regex.Matches(source, "ShadowsEnabled = true").Count;
                Assert.Equal(2, shadowsEnabledCount);
            }
        }
    }

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrShadowTheaterSceneFactorySourceTests`

Expected: FAIL because `PbrShadowTheaterSceneFactory.cs` does not exist yet.

- [ ] **Step 3: Implement PbrShadowTheaterSceneFactory**

Create `assets/codebase/rendering.tools/PbrShadowTheaterSceneFactory.cs`:

    using city.menu;
    using helengine;

    namespace city.rendering.tools {
        /// <summary>
        /// Builds the authored PBR shadow theater scene: a metallic sphere cluster on a pedestal, lit by both a shadow-casting sun and a shadow-casting spotlight.
        /// </summary>
        public sealed class PbrShadowTheaterSceneFactory {
            /// <summary>
            /// Stable scene id used by the generated PBR shadow theater asset.
            /// </summary>
            public const string SceneId = "scenes/rendering/pbr_shadow_theater.helen";

            /// <summary>
            /// Creates the canonical PBR shadow theater live-authored scene definition.
            /// </summary>
            /// <param name="cubeModel">Generated cube runtime model used by the pedestal.</param>
            /// <param name="sphereModel">Generated sphere runtime model shared by every cluster sphere.</param>
            /// <param name="pedestalMaterial">Runtime material used by the pedestal.</param>
            /// <param name="galleryMaterials">Twenty-five gallery runtime materials ordered by <see cref="PbrMaterialGalleryMaterialFactory.ResolveIndex"/>.</param>
            /// <returns>Live-authored scene definition for the PBR shadow theater showcase.</returns>
            public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel sphereModel, RuntimeMaterial pedestalMaterial, RuntimeMaterial[] galleryMaterials) {
                if (cubeModel == null) {
                    throw new ArgumentNullException(nameof(cubeModel));
                } else if (sphereModel == null) {
                    throw new ArgumentNullException(nameof(sphereModel));
                } else if (pedestalMaterial == null) {
                    throw new ArgumentNullException(nameof(pedestalMaterial));
                } else if (galleryMaterials == null) {
                    throw new ArgumentNullException(nameof(galleryMaterials));
                } else if (galleryMaterials.Length != PbrMaterialGalleryMaterialFactory.MetallicSteps * PbrMaterialGalleryMaterialFactory.RoughnessSteps) {
                    throw new ArgumentException("PBR shadow theater generation requires the full twenty-five element gallery material array.", nameof(galleryMaterials));
                }

                RuntimeMaterial lowRoughnessMetal = galleryMaterials[PbrMaterialGalleryMaterialFactory.ResolveIndex(4, 0)];
                RuntimeMaterial highRoughnessMetal = galleryMaterials[PbrMaterialGalleryMaterialFactory.ResolveIndex(4, 4)];
                RuntimeMaterial lowRoughnessDielectric = galleryMaterials[PbrMaterialGalleryMaterialFactory.ResolveIndex(0, 1)];

                return new GeneratedAuthoringSceneDefinition {
                    SceneId = SceneId,
                    SceneSettings = new SceneSettingsAsset(),
                    NintendoDsScene = new GeneratedDsSceneDefinition {
                        UseDefaultBottomOverlay = true,
                        BottomScreenRootEntities = Array.Empty<Entity>()
                    },
                    RootEntities = new[] {
                        CreateCameraEntity(),
                        CreateUiEntity(),
                        CreateDirectionalLightEntity(),
                        CreateSpotLightEntity(),
                        CreatePedestalEntity(cubeModel, pedestalMaterial),
                        CreateClusterSphereEntity("PbrShadowTheaterSphereLowRoughMetal", new float3(-1.3f, 2.1f, 0f), sphereModel, lowRoughnessMetal),
                        CreateClusterSphereEntity("PbrShadowTheaterSphereHighRoughMetal", new float3(1.3f, 2.1f, 0f), sphereModel, highRoughnessMetal),
                        CreateClusterSphereEntity("PbrShadowTheaterSphereDielectric", new float3(0f, 2.1f, 1.6f), sphereModel, lowRoughnessDielectric)
                    }
                };
            }

            /// <summary>
            /// Creates the authored orbit camera entity for the PBR shadow theater scene.
            /// </summary>
            /// <returns>Live authored camera entity.</returns>
            Entity CreateCameraEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(0f, -0.32f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterCamera");
                entity.LocalPosition = new float3(0f, 6f, 11f);
                entity.LocalScale = float3.One;
                entity.LocalOrientation = orientation;
                entity.AddComponent(new CameraComponent {
                    CameraDrawOrder = 0,
                    LayerMask = EditorLayerMasks.SceneObjects,
                    Viewport = new float4(0f, 0f, 1f, 1f),
                    NearPlaneDistance = 0.1f,
                    FarPlaneDistance = 96f,
                    ClearSettings = new CameraClearSettings(
                        true,
                        new float4(0.015f, 0.015f, 0.03f, 1f),
                        true,
                        1f,
                        false,
                        0),
                    RenderSettings = new CameraRenderSettings {
                        DepthPrepassMode = DepthPrepassMode.Auto,
                        ShadowDistance = 30f,
                        PostProcessTier = PostProcessTier.Disabled
                    }
                });
                entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                    OrbitCenter = new float3(0f, 1.6f, 0f),
                    AutoYawSpeedRadians = 0.1f
                });
                entity.AddComponent(new DemoDiscReturnToMenuComponent());
                return entity;
            }

            /// <summary>
            /// Creates the authored UI root entity for the PBR shadow theater scene.
            /// </summary>
            /// <returns>Live authored UI entity.</returns>
            Entity CreateUiEntity() {
                Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterUi");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.AddComponent(new FPSComponent {
                    Font = ResolveRequiredEditorFont(),
                    FontScale = 2f
                });
                PspFpsComponentOverrideService.Apply(entity);
                DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
                sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "15. PBR Shadow Theater");
                return entity;
            }

            /// <summary>
            /// Creates the authored shadow-casting sun for the PBR shadow theater scene.
            /// </summary>
            /// <returns>Live authored directional light entity.</returns>
            Entity CreateDirectionalLightEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(-0.9f, -0.7f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterSun");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = new float3(0f, 7f, 0f);
                entity.LocalOrientation = orientation;
                entity.AddComponent(new DirectionalLightComponent {
                    Color = new float4(1f, 0.95f, 0.9f, 1f),
                    Intensity = 0.85f,
                    ShadowsEnabled = true,
                    ShadowMapMode = ShadowMapMode.Forced,
                    ShadowStrength = 0.9f,
                    ShadowDistance = 30f
                });
                return entity;
            }

            /// <summary>
            /// Creates the authored shadow-casting spotlight for the PBR shadow theater scene.
            /// </summary>
            /// <returns>Live authored spotlight entity.</returns>
            Entity CreateSpotLightEntity() {
                float4 orientation;
                float4.CreateFromYawPitchRoll(1.9f, -0.95f, 0f, out orientation);

                Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterSpotlight");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = new float3(-3.5f, 6f, 3f);
                entity.LocalScale = float3.One;
                entity.LocalOrientation = orientation;
                entity.AddComponent(new SpotLightComponent {
                    Color = new float4(0.7f, 0.85f, 1f, 1f),
                    Range = 20f,
                    InnerConeAngleDegrees = 18f,
                    OuterConeAngleDegrees = 30f,
                    Intensity = 1.4f,
                    ShadowsEnabled = true,
                    ShadowMapMode = ShadowMapMode.Forced,
                    ShadowStrength = 1f
                });
                return entity;
            }

            /// <summary>
            /// Creates the authored pedestal mesh for the PBR shadow theater scene.
            /// </summary>
            /// <param name="model">Runtime cube model used by the mesh.</param>
            /// <param name="material">Runtime material used by the mesh.</param>
            /// <returns>Live authored pedestal entity.</returns>
            Entity CreatePedestalEntity(RuntimeModel model, RuntimeMaterial material) {
                Entity entity = Core.Instance.EntityFactory.Create("PbrShadowTheaterPedestal");
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = new float3(0f, 0.5f, 0f);
                entity.LocalScale = new float3(6f, 1f, 6f);
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new MeshComponent {
                    Model = model,
                    Materials = new[] { material },
                    RenderOrder3D = 0
                });
                return entity;
            }

            /// <summary>
            /// Creates one authored cluster sphere entity for the PBR shadow theater scene.
            /// </summary>
            /// <param name="name">Stable entity name.</param>
            /// <param name="localPosition">Local position assigned to the entity.</param>
            /// <param name="model">Runtime sphere model used by the mesh.</param>
            /// <param name="material">Runtime material used by the mesh.</param>
            /// <returns>Live authored cluster sphere entity.</returns>
            Entity CreateClusterSphereEntity(string name, float3 localPosition, RuntimeModel model, RuntimeMaterial material) {
                Entity entity = Core.Instance.EntityFactory.Create(name);
                entity.LayerMask = EditorLayerMasks.SceneObjects;
                entity.LocalPosition = localPosition;
                entity.LocalScale = new float3(1.6f, 1.6f, 1.6f);
                entity.LocalOrientation = float4.Identity;
                entity.AddComponent(new MeshComponent {
                    Model = model,
                    Materials = new[] { material },
                    RenderOrder3D = 0
                });
                return entity;
            }

            /// <summary>
            /// Resolves the editor font that should back the generated FPS overlay during live authoring.
            /// </summary>
            /// <returns>Editor font asset required by the FPS component.</returns>
            FontAsset ResolveRequiredEditorFont() {
                if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                    throw new InvalidOperationException("A default editor font must be loaded before the PBR shadow theater scene can be generated.");
                }

                return editorCore.DefaultFontAssetForEditor;
            }
        }
    }

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~PbrShadowTheaterSceneFactorySourceTests`

Expected: PASS.

- [ ] **Step 5: Commit**

    git add assets/codebase/rendering.tools/PbrShadowTheaterSceneFactory.cs assets/codebase/rendering.tools.tests/PbrShadowTheaterSceneFactorySourceTests.cs
    git commit -m "feat: add PBR shadow theater scene factory"

---

### Task 7: Wire the textured-showcase materials into asset preparation

**Files:**
- Modify: `assets/codebase/rendering.tools/RenderingSceneGenerationAssets.cs`
- Modify: `assets/codebase/rendering.tools/RenderingSceneAssetPreparationService.cs`
- Create: `assets/codebase/rendering.tools.tests/RenderingSceneAssetPreparationServiceSourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Consumes: `PbrTexturedShowcaseMaterialFactory.MetalMaterialRelativePath`/`.WoodMaterialRelativePath`/`.WriteMaterialAssets` (Task 4).
- Produces: `RenderingSceneGenerationAssets.PbrTexturedShowcaseMetalMaterial`/`.PbrTexturedShowcaseWoodMaterial` (`RuntimeMaterial` properties), consumed by `RenderingSceneGenerator` (Task 8).

- [ ] **Step 1: Write the failing source test**

Create `assets/codebase/rendering.tools.tests/RenderingSceneAssetPreparationServiceSourceTests.cs`:

    namespace city.tests {
        public sealed class RenderingSceneAssetPreparationServiceSourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Asset_bundle_exposes_the_textured_showcase_materials() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerationAssets.cs");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("public RuntimeMaterial PbrTexturedShowcaseMetalMaterial { get; set; }", source, StringComparison.Ordinal);
                Assert.Contains("public RuntimeMaterial PbrTexturedShowcaseWoodMaterial { get; set; }", source, StringComparison.Ordinal);
            }

            [Fact]
            public void Preparation_service_writes_and_loads_the_textured_showcase_materials() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneAssetPreparationService.cs");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("PbrTexturedShowcaseMaterialFactory", source, StringComparison.Ordinal);
                Assert.Contains("PbrTexturedShowcaseMaterialFactory.MetalMaterialRelativePath", source, StringComparison.Ordinal);
                Assert.Contains("PbrTexturedShowcaseMaterialFactory.WoodMaterialRelativePath", source, StringComparison.Ordinal);
                Assert.Contains("PbrTexturedShowcaseMetalMaterial = ", source, StringComparison.Ordinal);
                Assert.Contains("PbrTexturedShowcaseWoodMaterial = ", source, StringComparison.Ordinal);
            }
        }
    }

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~RenderingSceneAssetPreparationServiceSourceTests`

Expected: FAIL, both facts fail because the properties/wiring do not exist yet.

- [ ] **Step 3: Add the two properties to RenderingSceneGenerationAssets**

In `assets/codebase/rendering.tools/RenderingSceneGenerationAssets.cs`, add after the existing `RacerModel` property (before the closing class brace):

        /// <summary>
        /// Gets or sets the authored scuffed-metal runtime material used by the PBR textured showcase scene.
        /// </summary>
        public RuntimeMaterial PbrTexturedShowcaseMetalMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored wood-plank runtime material used by the PBR textured showcase scene.
        /// </summary>
        public RuntimeMaterial PbrTexturedShowcaseWoodMaterial { get; set; }

- [ ] **Step 4: Wire the write + load calls into RenderingSceneAssetPreparationService**

In `assets/codebase/rendering.tools/RenderingSceneAssetPreparationService.cs`, inside `Prepare(...)`, add the factory instantiation and write call next to the other material factories (near `TiltTrialClippingProbeMaterialFactory tiltTrialClippingProbeMaterialFactory = new TiltTrialClippingProbeMaterialFactory();` and `tiltTrialClippingProbeMaterialFactory.WriteMaterialAsset(fullProjectRootPath);`):

        PbrTexturedShowcaseMaterialFactory pbrTexturedShowcaseMaterialFactory = new PbrTexturedShowcaseMaterialFactory();

and

        pbrTexturedShowcaseMaterialFactory.WriteMaterialAssets(fullProjectRootPath);

Then, next to the other `LoadRuntimeMaterial(...)` calls (near `RuntimeMaterial tiltTrialPlayerSphereMarbleMaterial = LoadRuntimeMaterial(...)`), add:

        RuntimeMaterial pbrTexturedShowcaseMetalMaterial = LoadRuntimeMaterial(bootstrap, projectRootPath, PbrTexturedShowcaseMaterialFactory.MetalMaterialRelativePath);
        RuntimeMaterial pbrTexturedShowcaseWoodMaterial = LoadRuntimeMaterial(bootstrap, projectRootPath, PbrTexturedShowcaseMaterialFactory.WoodMaterialRelativePath);

Finally, inside the `return new RenderingSceneGenerationAssets { ... }` object initializer, add two more lines next to `RacerModel = racerModel,`:

        PbrTexturedShowcaseMetalMaterial = pbrTexturedShowcaseMetalMaterial,
        PbrTexturedShowcaseWoodMaterial = pbrTexturedShowcaseWoodMaterial

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~RenderingSceneAssetPreparationServiceSourceTests`

Expected: PASS.

- [ ] **Step 6: Build to confirm the whole rendering.tools project still compiles**

Run: `dotnet build user_settings/generated_code/projects/rendering.tools/rendering.tools.csproj -v quiet`

Expected: `0 errors`.

- [ ] **Step 7: Commit**

    git add assets/codebase/rendering.tools/RenderingSceneGenerationAssets.cs assets/codebase/rendering.tools/RenderingSceneAssetPreparationService.cs assets/codebase/rendering.tools.tests/RenderingSceneAssetPreparationServiceSourceTests.cs
    git commit -m "feat: load PBR textured showcase materials in asset preparation"

---

### Task 8: Register the three scenes in RenderingSceneGenerator and the curated label list

**Files:**
- Modify: `assets/codebase/rendering.tools/RenderingSceneGenerator.cs`
- Modify: `assets/codebase/rendering.tools.tests/DemoDiscSceneLabelOverlaySourceTests.cs`
- Create: `assets/codebase/rendering.tools.tests/RenderingSceneGeneratorPbrRegistrationSourceTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

**Interfaces:**
- Consumes: `PbrMaterialGalleryMaterialFactory` (Task 2), `PbrMaterialGallerySceneFactory` (Task 3), `PbrTexturedShowcaseMaterialFactory` (Task 4), `PbrTexturedShowcaseSceneFactory` (Task 5), `PbrShadowTheaterSceneFactory` (Task 6), `RenderingSceneGenerationAssets.PbrTexturedShowcaseMetalMaterial`/`.PbrTexturedShowcaseWoodMaterial` (Task 7).

- [ ] **Step 1: Write the failing source tests**

Create `assets/codebase/rendering.tools.tests/RenderingSceneGeneratorPbrRegistrationSourceTests.cs`:

    namespace city.tests {
        public sealed class RenderingSceneGeneratorPbrRegistrationSourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Generator_declares_the_three_new_pbr_scene_ids() {
                string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs"));
                Assert.Contains("public const string PbrMaterialGallerySceneId = \"scenes/rendering/pbr_material_gallery.helen\";", source, StringComparison.Ordinal);
                Assert.Contains("public const string PbrTexturedShowcaseSceneId = \"scenes/rendering/pbr_textured_showcase.helen\";", source, StringComparison.Ordinal);
                Assert.Contains("public const string PbrShadowTheaterSceneId = \"scenes/rendering/pbr_shadow_theater.helen\";", source, StringComparison.Ordinal);
            }

            [Fact]
            public void Generator_writes_the_three_new_pbr_scenes_after_the_existing_spotlight_scene() {
                string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs"));
                int spotlightWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, spotlightStreetSliceSceneDefinition);", StringComparison.Ordinal);
                int galleryWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, pbrMaterialGallerySceneDefinition);", StringComparison.Ordinal);
                int texturedWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, pbrTexturedShowcaseSceneDefinition);", StringComparison.Ordinal);
                int theaterWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, pbrShadowTheaterSceneDefinition);", StringComparison.Ordinal);
                Assert.True(spotlightWriteIndex >= 0 && galleryWriteIndex > spotlightWriteIndex && texturedWriteIndex > galleryWriteIndex && theaterWriteIndex > texturedWriteIndex,
                    "Expected the three new PBR scenes to be written, in order, after the existing spotlight street-slice scene.");
                Assert.Contains("PbrMaterialGalleryMaterialFactory.WriteMaterialAssets(projectRootPath);", source, StringComparison.Ordinal);
            }
        }
    }

Then modify `assets/codebase/rendering.tools.tests/DemoDiscSceneLabelOverlaySourceTests.cs`'s `Curated_rendering_factories_contain_the_approved_labels` fact, appending three entries to the `expected` array right after `("DirectionalShadowPlazaSceneFactory.cs", "7. Shadow Plaza")`:

            (string FileName, string Label)[] expected = [
                ("CubeTestSceneFactory.cs", "1. Cube Test"),
                ("ColoredCubeGridSceneFactory.cs", "2. Colored Cubes"),
                ("TexturedCubeGridSceneFactory.cs", "3. Textured Cubes"),
                ("AxisTestSceneFactory.cs", "4. Axis 1"),
                ("AxisTest2SceneFactory.cs", "5. Axis 2"),
                ("DirectionalShadowPlazaSceneFactory.cs", "7. Shadow Plaza"),
                ("PbrMaterialGallerySceneFactory.cs", "13. PBR Gallery"),
                ("PbrTexturedShowcaseSceneFactory.cs", "14. PBR Textures"),
                ("PbrShadowTheaterSceneFactory.cs", "15. PBR Shadow Theater")
            ];

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter "FullyQualifiedName~RenderingSceneGeneratorPbrRegistrationSourceTests|FullyQualifiedName~DemoDiscSceneLabelOverlaySourceTests"`

Expected: `RenderingSceneGeneratorPbrRegistrationSourceTests` FAILs (facts not yet true); `DemoDiscSceneLabelOverlaySourceTests.Curated_rendering_factories_contain_the_approved_labels` FAILs (the three new factory files don't exist as sources to read).

- [ ] **Step 3: Wire RenderingSceneGenerator**

In `assets/codebase/rendering.tools/RenderingSceneGenerator.cs`:

Add three scene-id consts after `SceneMemoryProbeNintendoDsSceneId` (around line 111):

        /// <summary>
        /// Stable scene id used by the PBR material gallery showcase.
        /// </summary>
        public const string PbrMaterialGallerySceneId = "scenes/rendering/pbr_material_gallery.helen";

        /// <summary>
        /// Stable scene id used by the PBR textured showcase.
        /// </summary>
        public const string PbrTexturedShowcaseSceneId = "scenes/rendering/pbr_textured_showcase.helen";

        /// <summary>
        /// Stable scene id used by the PBR shadow theater showcase.
        /// </summary>
        public const string PbrShadowTheaterSceneId = "scenes/rendering/pbr_shadow_theater.helen";

Add three factory fields after `readonly SceneMemoryProbeSceneFactory SceneMemoryProbeFactory;` (around line 171):

        /// <summary>
        /// Factory used to author the PBR material gallery materials.
        /// </summary>
        readonly PbrMaterialGalleryMaterialFactory PbrMaterialGalleryMaterialFactory;

        /// <summary>
        /// Factory used to author the PBR material gallery scene.
        /// </summary>
        readonly PbrMaterialGallerySceneFactory PbrMaterialGallerySceneFactory;

        /// <summary>
        /// Factory used to author the PBR textured showcase materials.
        /// </summary>
        readonly PbrTexturedShowcaseMaterialFactory PbrTexturedShowcaseMaterialFactory;

        /// <summary>
        /// Factory used to author the PBR textured showcase scene.
        /// </summary>
        readonly PbrTexturedShowcaseSceneFactory PbrTexturedShowcaseSceneFactory;

        /// <summary>
        /// Factory used to author the PBR shadow theater scene.
        /// </summary>
        readonly PbrShadowTheaterSceneFactory PbrShadowTheaterSceneFactory;

Instantiate them in the constructor, after `SceneMemoryProbeFactory = new SceneMemoryProbeSceneFactory();` (around line 189):

            PbrMaterialGalleryMaterialFactory = new PbrMaterialGalleryMaterialFactory();
            PbrMaterialGallerySceneFactory = new PbrMaterialGallerySceneFactory();
            PbrTexturedShowcaseMaterialFactory = new PbrTexturedShowcaseMaterialFactory();
            PbrTexturedShowcaseSceneFactory = new PbrTexturedShowcaseSceneFactory();
            PbrShadowTheaterSceneFactory = new PbrShadowTheaterSceneFactory();

In `Generate(...)`, after `GeneratedAuthoringSceneDefinition spotlightStreetSliceSceneDefinition = SpotlightStreetSliceFactory.CreateSceneDefinition(...)` and before `ColoredCubeGridFactory.WriteMaterialAssets(projectRootPath);`, add:

            PbrMaterialGalleryMaterialFactory.WriteMaterialAssets(projectRootPath);
            RuntimeMaterial[] pbrGalleryMaterials = PbrMaterialGalleryMaterialFactory.CreateRuntimeMaterials();
            GeneratedAuthoringSceneDefinition pbrMaterialGallerySceneDefinition = PbrMaterialGallerySceneFactory.CreateSceneDefinition(assets.GeneratedPlaneModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial, pbrGalleryMaterials);
            GeneratedAuthoringSceneDefinition pbrTexturedShowcaseSceneDefinition = PbrTexturedShowcaseSceneFactory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedPlaneModel, assets.GeneratedStandardMaterial, assets.PbrTexturedShowcaseMetalMaterial, assets.PbrTexturedShowcaseWoodMaterial);
            GeneratedAuthoringSceneDefinition pbrShadowTheaterSceneDefinition = PbrShadowTheaterSceneFactory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial, pbrGalleryMaterials);

Do not call `PbrTexturedShowcaseMaterialFactory.WriteMaterialAssets(...)` here — `RenderingSceneAssetPreparationService.Prepare(...)` (Task 7) already wrote those two materials and loaded `assets.PbrTexturedShowcaseMetalMaterial`/`.PbrTexturedShowcaseWoodMaterial` before `Generate(...)` ever runs; a second call here would just rewrite the same files redundantly.

Finally, append the three `WriteScene` calls after the existing `AuthoringSceneWriteService.WriteScene(projectRootPath, spotlightStreetSliceSceneDefinition);` line (the last line inside `Generate(...)` before its closing brace):

            AuthoringSceneWriteService.WriteScene(projectRootPath, pbrMaterialGallerySceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, pbrTexturedShowcaseSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, pbrShadowTheaterSceneDefinition);

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter "FullyQualifiedName~RenderingSceneGeneratorPbrRegistrationSourceTests|FullyQualifiedName~DemoDiscSceneLabelOverlaySourceTests"`

Expected: PASS.

- [ ] **Step 5: Build the full rendering.tools project**

Run: `dotnet build user_settings/generated_code/projects/rendering.tools/rendering.tools.csproj -v quiet`

Expected: `0 errors`. If `pbrGalleryMaterials` reports as unused or any parameter mismatch appears, fix the call site to match the exact method signatures produced in Tasks 2, 3, 5, 6.

- [ ] **Step 6: Commit**

    git add assets/codebase/rendering.tools/RenderingSceneGenerator.cs assets/codebase/rendering.tools.tests/DemoDiscSceneLabelOverlaySourceTests.cs assets/codebase/rendering.tools.tests/RenderingSceneGeneratorPbrRegistrationSourceTests.cs
    git commit -m "feat: register the three PBR showcase scenes in RenderingSceneGenerator"

---

### Task 9: Curate build_config.json and run full verification

**Files:**
- Modify: `user_settings/build_config.json`

- [ ] **Step 1: Add the three scenes to the windows platform curation**

In `user_settings/build_config.json`, the `windows` platform block spans roughly lines 4-137 (the block whose `"platformId": "windows"` appears first, before `"platformId": "ps2"`). Inside that block's `selectedSceneIds` array, add the three new scene ids right after `"tilt_trial_level_05"` (currently the last entry before the closing `]`):

                                                   "tilt_trial_level_05",
                                                   "pbr_material_gallery",
                                                   "pbr_textured_showcase",
                                                   "pbr_shadow_theater"

And inside that same block's `sceneOrders` array, add three more order entries right after the `"tilt_trial_level_05"` / `"orderNumber": 19` entry (before the array's closing `]`), continuing the numbering:

                                              {
                                                  "sceneId":  "tilt_trial_level_05",
                                                  "orderNumber":  19
                                                   },
                                              {
                                                  "sceneId":  "pbr_material_gallery",
                                                  "orderNumber":  20
                                              },
                                              {
                                                  "sceneId":  "pbr_textured_showcase",
                                                  "orderNumber":  21
                                              },
                                              {
                                                  "sceneId":  "pbr_shadow_theater",
                                                  "orderNumber":  22
                                              }

Use the Edit tool with the exact surrounding text (including its existing irregular indentation) as `old_string`/`new_string` context so the diff is minimal and unambiguous — do not reformat the rest of the file.

- [ ] **Step 2: Validate the JSON is still well-formed**

Run (PowerShell):

    Get-Content user_settings/build_config.json -Raw | ConvertFrom-Json | Out-Null

Expected: no error. If `ConvertFrom-Json` throws, the edit introduced a syntax error (missing comma or brace) — fix it before continuing.

- [ ] **Step 3: Run the full affected test suites**

Run:

    dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj -v quiet
    dotnet test user_settings/generated_code/projects/game.tools.tests/game.tools.tests.csproj -v quiet

Expected: all newly added tests pass. Compare the failure list against the pre-existing baseline (12 known pre-existing failures in `game.tools.tests`, confirmed unrelated to rendering work earlier this session via `git stash`/`git stash pop` — do not attempt to fix those) — no *new* failures should appear beyond that baseline. If `rendering.tools.tests` has any pre-existing failures, capture that list first with `git stash` the same way before concluding a failure is new.

- [ ] **Step 4: Build the full solution-adjacent projects touched this session**

Run:

    dotnet build user_settings/generated_code/projects/rendering.tools/rendering.tools.csproj -v quiet
    dotnet build user_settings/generated_code/projects/game.tools/game.tools.csproj -v quiet

Expected: `0 errors` for both.

- [ ] **Step 5: Commit**

    git add user_settings/build_config.json
    git commit -m "feat: curate the three PBR showcase scenes into the windows build"

- [ ] **Step 6: Report completion**

Summarize for the user: three new scenes were added (`pbr_material_gallery`, `pbr_textured_showcase`, `pbr_shadow_theater`), all registered in `RenderingSceneGenerator`, curated into the windows build, and covered by source-level regression tests. Note that actual visual verification (do the spheres/shadows/textures look right) requires running the project's editor scene-generation step, which is outside this plan's automated test scope — recommend the user regenerate and open the three scenes in the editor to confirm the visuals match intent.
