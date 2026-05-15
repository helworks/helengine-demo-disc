using helengine.editor;
using System.Reflection;

namespace city.rendering.tools {
    /// <summary>
    /// Prepares the runtime assets required by the city rendering showcase generators.
    /// </summary>
    public sealed class RenderingSceneAssetPreparationService {
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
            RuntimeModel generatedCubeModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.CubeAssetId);
            RuntimeModel generatedPlaneModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.PlaneAssetId);
            RuntimeModel generatedSphereModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.SphereAssetId);
            RuntimeMaterial generatedStandardMaterial = EngineGeneratedMaterialCache.GetRuntimeMaterial(EngineGeneratedMaterialCache.StandardAssetId);
            RuntimeMaterial[] racerMaterials = new[] {
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/Riemers/racer/x3ds_mat_ruedas.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/Riemers/racer/x3ds_mat_Material__0_3.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/Riemers/racer/x3ds_mat_Material_1_2.hasset"),
                LoadRuntimeMaterial(bootstrap, projectRootPath, "models/Riemers/racer/x3ds_mat_Material_2_1.hasset")
            };
            RuntimeModel lamppostModel = LoadImportedModelRuntime(projectRootPath, "models/Riemers/lamppost.x");
            RuntimeModel racerModel = LoadImportedModelRuntime(projectRootPath, "models/Riemers/racer.x");

            return new RenderingSceneGenerationAssets {
                GeneratedCubeModel = generatedCubeModel,
                GeneratedPlaneModel = generatedPlaneModel,
                GeneratedSphereModel = generatedSphereModel,
                GeneratedStandardMaterial = generatedStandardMaterial,
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
            string platformId = ResolveActivePlatformId(fullProjectRootPath);
            string fullMaterialPath = Path.GetFullPath(Path.Combine(assetsRootPath, relativeMaterialPath.Replace('/', Path.DirectorySeparatorChar)));
            MaterialAssetSettingsService settingsService = new MaterialAssetSettingsService();
            MaterialAsset materialAsset;
            try {
                materialAsset = settingsService.LoadMaterialAsset(fullMaterialPath, platformId);
            } catch (InvalidOperationException) {
                materialAsset = MigrateLegacyMaterialAsset(fullMaterialPath, bootstrap, settingsService, platformId);
            }
            if (string.IsNullOrWhiteSpace(materialAsset.ShaderAssetId)) {
                throw new InvalidOperationException($"Material '{relativeMaterialPath}' did not resolve a shader asset.");
            }

            ShaderAsset shaderAsset = global::helengine.editor.EditorShaderPackageService.LoadShaderAsset(materialAsset.ShaderAssetId);
            return Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
        }

        /// <summary>
        /// Migrates one legacy binary material asset into the current settings-document format.
        /// </summary>
        /// <param name="fullMaterialPath">Absolute path to the material file.</param>
        /// <param name="bootstrap">Project bootstrap context used to resolve supported platforms.</param>
        /// <param name="settingsService">Material settings service used to write the migrated document.</param>
        /// <param name="platformId">Platform whose effective runtime material should be resolved after migration.</param>
        /// <returns>Runtime-facing material asset loaded from the migrated settings document.</returns>
        MaterialAsset MigrateLegacyMaterialAsset(
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
            MaterialAsset migratedMaterialAsset = settingsService.LoadMaterialAsset(fullMaterialPath, platformId);
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

            ContentManager contentManager = new ContentManager(assetsRootPath);
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
        /// Resolves the active project platform that should drive authored material loading.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path.</param>
        /// <returns>Active platform identifier, or the first supported platform when no explicit active platform is available.</returns>
        string ResolveActivePlatformId(string projectRootPath) {
            EditorProjectPlatformsDocument platformsDocument = new EditorProjectPlatformsService(projectRootPath).Load();
            IReadOnlyList<string> supportedPlatforms = platformsDocument.SupportedPlatforms;
            if (supportedPlatforms.Count == 0) {
                throw new InvalidOperationException("At least one supported project platform must exist before authored materials can be loaded.");
            }

            string activePlatformId = new EditorProjectLocalSettingsService(projectRootPath, supportedPlatforms).LoadActivePlatform();
            if (!string.IsNullOrWhiteSpace(activePlatformId)) {
                return activePlatformId;
            }

            return supportedPlatforms[0];
        }
    }
}


