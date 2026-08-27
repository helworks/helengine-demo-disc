using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Prepares the runtime imported models required by the Zombislayer gameplay scene generator.
    /// </summary>
    public sealed class ZombislayerAssetPreparationService {
        /// <summary>
        /// Host-owned asset-authoring capability used to resolve imported model sources.
        /// </summary>
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        /// <summary>
        /// Initializes one Zombislayer asset preparation service.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used for source imports.</param>
        public ZombislayerAssetPreparationService(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }
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
            string fullSourcePath = Path.GetFullPath(Path.Combine(assetsRootPath, relativeSourcePath.Replace('/', Path.DirectorySeparatorChar)));
            return AssetAuthoringService.ResolveRuntimeModel(fullSourcePath);
        }

    }
}
