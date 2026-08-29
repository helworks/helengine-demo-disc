using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Prepares the runtime assets required by the city rendering showcase generators.
    /// </summary>
    public sealed class RenderingSceneAssetPreparationService {
        /// <summary>
        /// Host-owned asset-authoring capability used to resolve imported source assets.
        /// </summary>
        readonly IEditorProjectAuthoringSession AuthoringSession;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Initializes one rendering asset preparation service.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used for settings and source imports.</param>
        public RenderingSceneAssetPreparationService(
            IEditorProjectAuthoringSession authoringSession,
            EditorAuthoringTransaction transaction) {
            AuthoringSession = authoringSession ?? throw new ArgumentNullException(nameof(authoringSession));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }
        /// <summary>
        /// Preferred editor preview platform used when authored material settings need one shader-backed runtime preview path.
        /// </summary>
        const string PreferredEditorPreviewPlatformId = "windows";

        /// <summary>
        /// Built-in standard shader source file used by synthesized editor preview materials.
        /// </summary>
        const string StandardShaderSourceFileName = "ForwardStandardShader.hlsl";

        /// <summary>
        /// Built-in standard shader asset id used by synthesized editor preview materials.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Built-in standard vertex program used by synthesized editor preview materials.
        /// </summary>
        const string StandardVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Built-in standard pixel program used by synthesized editor preview materials.
        /// </summary>
        const string StandardPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Standard mesh variant used by synthesized editor preview materials.
        /// </summary>
        const string StandardMeshVariantName = "Mesh";

        /// <summary>
        /// Field id that stores fixed-pipeline authored base color in material settings.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Prepares all runtime assets required by the rendering showcase scene generator.
        /// </summary>
        /// <returns>Prepared runtime asset bundle.</returns>
        public RenderingSceneGenerationAssets Prepare() {
            string fullProjectRootPath = Path.GetFullPath(AuthoringSession.ProjectRootPath);
            ForwardSolidColorMaterialFactory forwardSolidColorMaterialFactory = new ForwardSolidColorMaterialFactory(AuthoringSession, Transaction);
            TiltTrialCourseMaterialFactory tiltTrialCourseMaterialFactory = new TiltTrialCourseMaterialFactory(AuthoringSession, Transaction);
            TiltTrialPlayerSphereWalnutMaterialFactory tiltTrialPlayerSphereWalnutMaterialFactory = new TiltTrialPlayerSphereWalnutMaterialFactory(AuthoringSession, Transaction);
            TiltTrialClippingProbeModelFactory tiltTrialClippingProbeModelFactory = new TiltTrialClippingProbeModelFactory();
            TiltTrialClippingProbeMaterialFactory tiltTrialClippingProbeMaterialFactory = new TiltTrialClippingProbeMaterialFactory(AuthoringSession, Transaction);
            DepthClipProbeMaterialFactory depthClipProbeMaterialFactory = new DepthClipProbeMaterialFactory(AuthoringSession, Transaction);
            DepthClipProbeCenterMaterialFactory depthClipProbeCenterMaterialFactory = new DepthClipProbeCenterMaterialFactory(AuthoringSession, Transaction);
            PbrTexturedShowcaseMaterialFactory pbrTexturedShowcaseMaterialFactory = new PbrTexturedShowcaseMaterialFactory(AuthoringSession, Transaction);
            AxisTestMaterialFactory axisTestMaterialFactory = new AxisTestMaterialFactory(AuthoringSession, Transaction);
            forwardSolidColorMaterialFactory.WriteMaterialAsset(fullProjectRootPath);
            tiltTrialCourseMaterialFactory.WriteMaterialAsset(fullProjectRootPath, AuthoringSession);
            tiltTrialPlayerSphereWalnutMaterialFactory.WriteMaterialAsset(fullProjectRootPath, AuthoringSession);
            tiltTrialClippingProbeModelFactory.WriteModelAsset(AuthoringSession, Transaction);
            tiltTrialClippingProbeMaterialFactory.WriteMaterialAsset(fullProjectRootPath, AuthoringSession);
            depthClipProbeMaterialFactory.WriteMaterialAsset(fullProjectRootPath);
            depthClipProbeCenterMaterialFactory.WriteMaterialAsset(fullProjectRootPath);
            pbrTexturedShowcaseMaterialFactory.WriteMaterialAssets(fullProjectRootPath, AuthoringSession);
            axisTestMaterialFactory.WriteMaterialAssets(fullProjectRootPath);
            RuntimeModel generatedCubeModel = AuthoringSession.GeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.CubeAssetId);
            RuntimeModel generatedPlaneModel = AuthoringSession.GeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.PlaneAssetId);
            RuntimeModel generatedSphereModel = AuthoringSession.GeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.SphereAssetId);
            RuntimeModel generatedArrowModel = LoadImportedModelRuntime("models/rendering/axis_test/directional_light_arrow.obj");
            RuntimeMaterial generatedStandardMaterial = AuthoringSession.GeneratedMaterialCache.GetRuntimeMaterial(EngineGeneratedMaterialCache.StandardAssetId);
            RuntimeMaterial tiltTrialPlayerSphereMarbleMaterial = LoadRuntimeMaterial("materials/rendering/tilt_trial/PlayerSphereMarble.hasset");
            RuntimeMaterial tiltTrialCourseMaterial = LoadRuntimeMaterial(TiltTrialCourseMaterialFactory.MaterialRelativePath);
            RuntimeModel tiltTrialClippingProbeModel = LoadImportedModelRuntime(TiltTrialClippingProbeModelFactory.ModelRelativePath);
            RuntimeMaterial tiltTrialClippingProbeMaterial = LoadRuntimeMaterial(TiltTrialClippingProbeMaterialFactory.MaterialRelativePath);
            RuntimeModel goldenCoinModel = LoadImportedModelRuntime("models/games/tilt/golden_coin.hasset");
            RuntimeMaterial goldenCoinMaterial = LoadRuntimeMaterial("materials/games/tilt/GoldenCoin.hasset");
            RuntimeModel goalFlagModel = LoadImportedModelRuntime("models/games/tilt/goal_flag.hasset");
            RuntimeMaterial goalFlagPoleMaterial = LoadRuntimeMaterial("materials/games/tilt/GoalFlagPole.hasset");
            RuntimeMaterial goalFlagBannerMaterial = LoadRuntimeMaterial("materials/games/tilt/GoalFlagBanner.hasset");
            RuntimeMaterial generatedCubeTestSolidMaterial = LoadRuntimeMaterial(ForwardSolidColorMaterialFactory.MaterialRelativePath);
            RuntimeMaterial depthClipProbeMaterial = LoadRuntimeMaterial(DepthClipProbeMaterialFactory.MaterialRelativePath);
            RuntimeMaterial depthClipProbeCenterMaterial = LoadRuntimeMaterial(DepthClipProbeCenterMaterialFactory.MaterialRelativePath);
            RuntimeMaterial[] axisMaterials = new[] {
                LoadRuntimeMaterial("materials/rendering/axis_test/X.hasset"),
                LoadRuntimeMaterial("materials/rendering/axis_test/Y.hasset"),
                LoadRuntimeMaterial("materials/rendering/axis_test/Z.hasset"),
                LoadRuntimeMaterial("materials/rendering/axis_test/Ground.hasset"),
                LoadRuntimeMaterial("materials/rendering/axis_test/Marker.hasset")
            };
            RuntimeModel lamppostModel = LoadImportedModelRuntime("models/riemers/lamppost.x");
            RuntimeModel racerModel = LoadImportedModelRuntime("models/riemers/racer.x");
            RuntimeMaterial[] racerMaterials = new[] {
                LoadRuntimeMaterial("models/riemers/racer/x3ds_mat_ruedas.hasset"),
                LoadRuntimeMaterial("models/riemers/racer/x3ds_mat_Material__0_3.hasset"),
                LoadRuntimeMaterial("models/riemers/racer/x3ds_mat_Material_1_2.hasset"),
                LoadRuntimeMaterial("models/riemers/racer/x3ds_mat_Material_2_1.hasset")
            };
            RuntimeMaterial pbrTexturedShowcaseMetalMaterial = LoadRuntimeMaterial(PbrTexturedShowcaseMaterialFactory.MetalMaterialRelativePath);
            RuntimeMaterial pbrTexturedShowcaseWoodMaterial = LoadRuntimeMaterial(PbrTexturedShowcaseMaterialFactory.WoodMaterialRelativePath);

            return new RenderingSceneGenerationAssets {
                GeneratedCubeModel = generatedCubeModel,
                GeneratedPlaneModel = generatedPlaneModel,
                GeneratedSphereModel = generatedSphereModel,
                GeneratedStandardMaterial = generatedStandardMaterial,
                TiltTrialPlayerSphereMarbleMaterial = tiltTrialPlayerSphereMarbleMaterial,
                TiltTrialCourseMaterial = tiltTrialCourseMaterial,
                TiltTrialClippingProbeModel = tiltTrialClippingProbeModel,
                TiltTrialClippingProbeMaterial = tiltTrialClippingProbeMaterial,
                GoldenCoinModel = goldenCoinModel,
                GoldenCoinMaterial = goldenCoinMaterial,
                GoalFlagModel = goalFlagModel,
                GoalFlagPoleMaterial = goalFlagPoleMaterial,
                GoalFlagBannerMaterial = goalFlagBannerMaterial,
                GeneratedCubeTestSolidMaterial = generatedCubeTestSolidMaterial,
                DepthClipProbeMaterial = depthClipProbeMaterial,
                DepthClipProbeCenterMaterial = depthClipProbeCenterMaterial,
                GeneratedArrowModel = generatedArrowModel,
                AxisMaterials = axisMaterials,
                RacerMaterials = racerMaterials,
                LamppostModel = lamppostModel,
                RacerModel = racerModel,
                PbrTexturedShowcaseMetalMaterial = pbrTexturedShowcaseMetalMaterial,
                PbrTexturedShowcaseWoodMaterial = pbrTexturedShowcaseWoodMaterial
            };
        }

        /// <summary>
        /// Loads one imported model runtime asset from the project assets folder.
        /// </summary>
        /// <param name="relativeSourcePath">Project-relative model source path.</param>
        /// <returns>Runtime model rebuilt from the imported cache.</returns>
        RuntimeModel LoadImportedModelRuntime(string relativeSourcePath) {
            if (string.IsNullOrWhiteSpace(relativeSourcePath)) {
                throw new ArgumentException("Relative source path must be provided.", nameof(relativeSourcePath));
            }

            return AuthoringSession.LoadImportedRuntimeModel(relativeSourcePath);
        }

        /// <summary>
        /// Loads one authored runtime material from a project material settings document.
        /// </summary>
        /// <param name="relativeMaterialPath">Project-relative material path.</param>
        /// <returns>Runtime material rebuilt from the authored material settings.</returns>
        RuntimeMaterial LoadRuntimeMaterial(string relativeMaterialPath) {
            if (string.IsNullOrWhiteSpace(relativeMaterialPath)) {
                throw new ArgumentException("Relative material path must be provided.", nameof(relativeMaterialPath));
            }

            string platformId = ResolveMaterialPreviewPlatformId();
            ShaderMaterialAsset materialAsset = AuthoringSession.LoadMaterialAsset(relativeMaterialPath, platformId, Transaction);
            MaterialAssetProcessorSettings platformSettings = AuthoringSession.LoadMaterialPlatformSettings(relativeMaterialPath, platformId, Transaction);

            if (string.IsNullOrWhiteSpace(materialAsset.ShaderAssetId)) {
                return BuildPreviewRuntimeMaterial(materialAsset, platformSettings);
            }

            ShaderAsset shaderAsset = AuthoringSession.LoadBuiltInShaderAssetById(materialAsset.ShaderAssetId);
            return AuthoringSession.OwningCore.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
        }

        /// <summary>
        /// Builds one shader-backed preview runtime material for authored fixed-pipeline material settings that do not expose one direct shader asset id.
        /// </summary>
        /// <param name="materialAsset">Authored material asset carrying the stable asset id that must survive scene serialization.</param>
        /// <param name="platformSettings">Effective platform settings document used to extract preview-facing values such as base color.</param>
        /// <returns>Shader-backed preview runtime material that preserves the authored material asset id.</returns>
        RuntimeMaterial BuildPreviewRuntimeMaterial(ShaderMaterialAsset materialAsset, MaterialAssetProcessorSettings platformSettings) {
            if (materialAsset == null) {
                throw new ArgumentNullException(nameof(materialAsset));
            } else if (platformSettings == null) {
                throw new ArgumentNullException(nameof(platformSettings));
            }

            ShaderAsset shaderAsset = AuthoringSession.LoadBuiltInShaderAssetById(StandardShaderAssetId);
            ShaderMaterialAsset previewMaterialAsset = new ShaderMaterialAsset {
                Id = materialAsset.Id,
                ShaderAssetId = StandardShaderAssetId,
                VertexProgram = StandardVertexProgramName,
                PixelProgram = StandardPixelProgramName,
                Variant = StandardMeshVariantName,
                ConstantBuffers = new[] {
                    new MaterialConstantBufferAsset {
                        Name = helengine.editor.StandardMaterialBaseColorDefaults.BaseColorBufferName,
                        Data = helengine.editor.StandardMaterialBaseColorDefaults.CreateConstantBufferData(ResolvePreviewBaseColor(platformSettings))
                    }
                },
                CastsShadows = materialAsset.CastsShadows,
                ReceivesShadows = materialAsset.ReceivesShadows
            };
            RuntimeMaterial runtimeMaterial = AuthoringSession.OwningCore.RenderManager3D.BuildMaterialFromRaw(previewMaterialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(runtimeMaterial), AuthoringSession.OwningCore.RenderManager2D);
            return runtimeMaterial;
        }

        /// <summary>
        /// Resolves one preview base color from the effective fixed-pipeline platform settings.
        /// </summary>
        /// <param name="platformSettings">Effective platform settings that may publish one HTML-style base-color field.</param>
        /// <returns>Preview base color, or opaque white when the settings omit or corrupt the field.</returns>
        float4 ResolvePreviewBaseColor(MaterialAssetProcessorSettings platformSettings) {
            if (platformSettings == null) {
                throw new ArgumentNullException(nameof(platformSettings));
            } else if (platformSettings.FieldValues == null) {
                return new float4(1f, 1f, 1f, 1f);
            }

            if (!platformSettings.FieldValues.TryGetValue(BaseColorFieldId, out string colorValue) || string.IsNullOrWhiteSpace(colorValue)) {
                return new float4(1f, 1f, 1f, 1f);
            }

            return ParseHtmlColor(colorValue);
        }

        /// <summary>
        /// Parses one `#RRGGBBAA` HTML color string into normalized float components.
        /// </summary>
        /// <param name="colorValue">HTML-style color string to parse.</param>
        /// <returns>Normalized float color representation.</returns>
        float4 ParseHtmlColor(string colorValue) {
            if (string.IsNullOrWhiteSpace(colorValue) || colorValue.Length != 9 || colorValue[0] != '#') {
                return new float4(1f, 1f, 1f, 1f);
            }

            try {
                byte red = byte.Parse(colorValue.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                byte green = byte.Parse(colorValue.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                byte blue = byte.Parse(colorValue.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                byte alpha = byte.Parse(colorValue.Substring(7, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                return new float4(
                    red / 255f,
                    green / 255f,
                    blue / 255f,
                    alpha / 255f);
            } catch (FormatException) {
                return new float4(1f, 1f, 1f, 1f);
            } catch (OverflowException) {
                return new float4(1f, 1f, 1f, 1f);
            }
        }

        /// <summary>
        /// Resolves the editor preview platform that should drive authored material loading during headless rendering-scene generation.
        /// </summary>
        /// <returns>Preferred preview platform identifier, or the active/first supported platform when the preferred preview platform is unavailable.</returns>
        string ResolveMaterialPreviewPlatformId() {
            string projectRootPath = Path.GetFullPath(AuthoringSession.ProjectRootPath);
            EditorProjectPlatformsDocument platformsDocument = new EditorProjectPlatformsService(projectRootPath).Load();
            IReadOnlyList<string> supportedPlatforms = platformsDocument.SupportedPlatforms;
            if (supportedPlatforms.Count == 0) {
                throw new InvalidOperationException("At least one supported project platform must exist before authored materials can be loaded.");
            }

            for (int index = 0; index < supportedPlatforms.Count; index++) {
                if (string.Equals(supportedPlatforms[index], PreferredEditorPreviewPlatformId, StringComparison.OrdinalIgnoreCase)) {
                    return supportedPlatforms[index];
                }
            }

            string activePlatformId = new EditorProjectLocalSettingsService(projectRootPath, supportedPlatforms).LoadActivePlatform();
            if (!string.IsNullOrWhiteSpace(activePlatformId)) {
                return activePlatformId;
            }

            return supportedPlatforms[0];
        }
    }
}


