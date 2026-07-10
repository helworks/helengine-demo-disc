using helengine.editor;
using System.Reflection;

namespace city.game.tools {
    /// <summary>
    /// Prepares the runtime imported models required by the Zombislayer gameplay scene generator.
    /// </summary>
    public sealed class ZombislayerAssetPreparationService {
        /// <summary>
        /// Prepares the runtime imported models required by the Zombislayer gameplay scene generator.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Prepared Zombislayer runtime assets.</returns>
        public ZombislayerGenerationAssets Prepare(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            RuntimeModel environmentModel = LoadImportedModelRuntime(projectRootPath, ZombislayerAssetCatalog.EnvironmentModelRelativePath);
            RuntimeModel weaponModel = LoadImportedModelRuntime(projectRootPath, ZombislayerAssetCatalog.WeaponModelRelativePath);
            return new ZombislayerGenerationAssets {
                EnvironmentModel = environmentModel,
                WeaponModel = weaponModel
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
    }
}
