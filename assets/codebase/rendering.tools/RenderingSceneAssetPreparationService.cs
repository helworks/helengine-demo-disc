using helengine;
using helengine.editor;
using System.Reflection;

namespace city.rendering.tools {
    /// <summary>
    /// Prepares the runtime assets required by the city rendering showcase generators.
    /// </summary>
    public sealed class RenderingSceneAssetPreparationService {
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
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Prepared runtime asset bundle.</returns>
        public RenderingSceneGenerationAssets Prepare(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            EditorProjectBootstrapContext bootstrap = EditorProjectBootstrapper.Create(fullProjectRootPath);
            ForwardSolidColorMaterialFactory forwardSolidColorMaterialFactory = new ForwardSolidColorMaterialFactory();
            TiltTrialCourseMaterialFactory tiltTrialCourseMaterialFactory = new TiltTrialCourseMaterialFactory();
            forwardSolidColorMaterialFactory.WriteMaterialAsset(fullProjectRootPath);
            tiltTrialCourseMaterialFactory.WriteMaterialAsset(fullProjectRootPath);
            RuntimeModel generatedCubeModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.CubeAssetId);
            RuntimeModel generatedPlaneModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.PlaneAssetId);
            RuntimeModel generatedSphereModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.SphereAssetId);
            RuntimeModel generatedArrowModel = LoadImportedModelRuntime(projectRootPath, "models/rendering/axis_test/directional_light_arrow.obj");
            RuntimeMaterial generatedStandardMaterial = EngineGeneratedMaterialCache.GetRuntimeMaterial(EngineGeneratedMaterialCache.StandardAssetId);
            RuntimeMaterial tiltTrialPlayerSphereMarbleMaterial = LoadRuntimeMaterial(bootstrap, projectRootPath, "materials/rendering/tilt_trial/PlayerSphereMarble.hasset");
            RuntimeMaterial tiltTrialCourseMaterial = LoadRuntimeMaterial(bootstrap, projectRootPath, TiltTrialCourseMaterialFactory.MaterialRelativePath);
            RuntimeMaterial generatedCubeTestSolidMaterial = LoadRuntimeMaterial(bootstrap, projectRootPath, ForwardSolidColorMaterialFactory.MaterialRelativePath);
            RuntimeMaterial[] axisMaterials = new[] {
                LoadRuntimeMaterial(bootstrap, projectRootPath, "materials/rendering/axis_test/X.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "materials/rendering/axis_test/Y.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "materials/rendering/axis_test/Z.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "materials/rendering/axis_test/Ground.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "materials/rendering/axis_test/Marker.hasset")
            };
            RuntimeMaterial[] racerMaterials = new[] {
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/riemers/racer/x3ds_mat_ruedas.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/riemers/racer/x3ds_mat_Material__0_3.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/riemers/racer/x3ds_mat_Material_1_2.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/riemers/racer/x3ds_mat_Material_2_1.hasset")
            };
            RuntimeModel lamppostModel = LoadImportedModelRuntime(projectRootPath, "models/riemers/lamppost.x");
            RuntimeModel racerModel = LoadImportedModelRuntime(projectRootPath, "models/riemers/racer.x");

            return new RenderingSceneGenerationAssets {
                GeneratedCubeModel = generatedCubeModel,
                GeneratedPlaneModel = generatedPlaneModel,
                GeneratedSphereModel = generatedSphereModel,
                GeneratedStandardMaterial = generatedStandardMaterial,
                TiltTrialPlayerSphereMarbleMaterial = tiltTrialPlayerSphereMarbleMaterial,
                TiltTrialCourseMaterial = tiltTrialCourseMaterial,
                GeneratedCubeTestSolidMaterial = generatedCubeTestSolidMaterial,
                GeneratedArrowModel = generatedArrowModel,
                AxisMaterials = axisMaterials,
                RacerMaterials = racerMaterials,
                LamppostModel = lamppostModel,
                RacerModel = racerModel
            };
        }

        /// <summary>
        /// Loads one imported model runtime asset from the project assets folder.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="relativeSourcePath">Project-relative model source path.</param>
        /// <returns>Runtime model rebuilt from the imported cache.</returns>
        RuntimeModel LoadImportedModelRuntime(string projectRootPath, string relativeSourcePath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativeSourcePath)) {
                throw new ArgumentException("Relative source path must be provided.", nameof(relativeSourcePath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            AssetImportManager importManager = CreateAssetImportManager(fullProjectRootPath, assetsRootPath);
            EditorFileSystemModelResolver modelResolver = new EditorFileSystemModelResolver(importManager);
            string fullSourcePath = Path.GetFullPath(Path.Combine(assetsRootPath, relativeSourcePath.Replace('/', Path.DirectorySeparatorChar)));
            return modelResolver.ResolveRuntimeModel(fullSourcePath);
        }

        /// <summary>
        /// Loads one authored runtime material from a project material settings document.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="relativeMaterialPath">Project-relative material path.</param>
        /// <returns>Runtime material rebuilt from the authored material settings.</returns>
        RuntimeMaterial LoadRuntimeMaterial(EditorProjectBootstrapContext bootstrap, string projectRootPath, string relativeMaterialPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (bootstrap == null) {
                throw new ArgumentNullException(nameof(bootstrap));
            } else if (string.IsNullOrWhiteSpace(relativeMaterialPath)) {
                throw new ArgumentException("Relative material path must be provided.", nameof(relativeMaterialPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string platformId = ResolveMaterialPreviewPlatformId(fullProjectRootPath);
            string fullMaterialPath = Path.GetFullPath(Path.Combine(assetsRootPath, relativeMaterialPath.Replace('/', Path.DirectorySeparatorChar)));
            MaterialAssetSettingsService settingsService = new MaterialAssetSettingsService();
            ShaderMaterialAsset materialAsset;
            try {
                materialAsset = settingsService.LoadMaterialAsset(fullMaterialPath, platformId);
            } catch (InvalidOperationException) {
                materialAsset = MigrateLegacyMaterialAsset(fullMaterialPath, bootstrap, settingsService, platformId);
            }
            MaterialAssetProcessorSettings platformSettings;
            if (!settingsService.TryLoadPlatformSettings(fullMaterialPath, platformId, out platformSettings) || platformSettings == null) {
                throw new InvalidOperationException($"Material settings for platform '{platformId}' could not be loaded from '{relativeMaterialPath}'.");
            }

            if (string.IsNullOrWhiteSpace(materialAsset.ShaderAssetId)) {
                return BuildPreviewRuntimeMaterial(materialAsset, platformSettings);
            }

            ShaderAsset shaderAsset = global::helengine.editor.EditorShaderPackageService.LoadShaderAsset(materialAsset.ShaderAssetId);
            return Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
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

            ShaderAsset shaderAsset = helengine.editor.EditorBuiltInShaderAssetLibrary.LoadShaderAsset(Core.Instance.RenderManager3D, StandardShaderSourceFileName);
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
            RuntimeMaterial runtimeMaterial = Core.Instance.RenderManager3D.BuildMaterialFromRaw(previewMaterialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(runtimeMaterial));
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
        /// Migrates one legacy binary material asset into the current settings-document format.
        /// </summary>
        /// <param name="fullMaterialPath">Absolute path to the material file.</param>
        /// <param name="bootstrap">Project bootstrap context used to resolve supported platforms.</param>
        /// <param name="settingsService">Material settings service used to write the migrated document.</param>
        /// <param name="platformId">Platform whose effective runtime material should be resolved after migration.</param>
        /// <returns>Runtime-facing material asset loaded from the migrated settings document.</returns>
        ShaderMaterialAsset MigrateLegacyMaterialAsset(
            string fullMaterialPath,
            EditorProjectBootstrapContext bootstrap,
            MaterialAssetSettingsService settingsService,
            string platformId) {
            if (string.IsNullOrWhiteSpace(fullMaterialPath)) {
                throw new ArgumentException("Material path must be provided.", nameof(fullMaterialPath));
            } else if (bootstrap == null) {
                throw new ArgumentNullException(nameof(bootstrap));
            } else if (settingsService == null) {
                throw new ArgumentNullException(nameof(settingsService));
            } else if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            Asset loadedAsset;
            using (FileStream stream = new FileStream(fullMaterialPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                loadedAsset = global::helengine.editor.AssetSerializer.Deserialize(stream);
            }

            if (loadedAsset is not MaterialAsset materialAsset) {
                throw new InvalidOperationException($"Material document '{fullMaterialPath}' could not be loaded.");
            }

            settingsService.LoadOrCreate(fullMaterialPath, materialAsset, bootstrap.SupportedPlatforms, bootstrap.ResolveSelectionModel);
            ShaderMaterialAsset migratedMaterialAsset = settingsService.LoadMaterialAsset(fullMaterialPath, platformId);
            return migratedMaterialAsset;
        }

        /// <summary>
        /// Builds one asset import manager initialized with the editor host's default importer registrations.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path.</param>
        /// <param name="assetsRootPath">Absolute project assets root path.</param>
        /// <returns>Configured asset import manager.</returns>
        AssetImportManager CreateAssetImportManager(string projectRootPath, string assetsRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(assetsRootPath)) {
                throw new ArgumentException("Assets root path must be provided.", nameof(assetsRootPath));
            }

            ContentManager contentManager = new ContentManager(new HostFileSystemContentStreamSource(assetsRootPath));
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
        /// Creates the default importer registrations used by the editor host.
        /// </summary>
        /// <returns>Default importer registrations resolved from the editor app assembly.</returns>
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

        /// <summary>
        /// Resolves the editor preview platform that should drive authored material loading during headless rendering-scene generation.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path.</param>
        /// <returns>Preferred preview platform identifier, or the active/first supported platform when the preferred preview platform is unavailable.</returns>
        string ResolveMaterialPreviewPlatformId(string projectRootPath) {
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


