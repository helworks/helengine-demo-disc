using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Prepares the runtime imported models required by the Zombislayer gameplay scene generator.
    /// </summary>
    public sealed class ZombislayerAssetPreparationService {
        /// <summary>
        /// Host-owned asset-authoring capability used to resolve imported model sources.
        /// </summary>
        readonly IEditorProjectAuthoringSession AuthoringSession;

        /// <summary>
        /// Initializes one Zombislayer asset preparation service.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used for source imports.</param>
        public ZombislayerAssetPreparationService(IEditorProjectAuthoringSession authoringSession) {
            AuthoringSession = authoringSession ?? throw new ArgumentNullException(nameof(authoringSession));
        }
        /// <summary>
        /// Prepares the runtime imported models required by the Zombislayer gameplay scene generator.
        /// </summary>
        /// <returns>Prepared Zombislayer runtime assets.</returns>
        public ZombislayerGenerationAssets Prepare() {
            RuntimeModel environmentModel = AuthoringSession.LoadImportedRuntimeModel(ZombislayerAssetCatalog.EnvironmentModelRelativePath);
            RuntimeModel weaponModel = AuthoringSession.LoadImportedRuntimeModel(ZombislayerAssetCatalog.WeaponModelRelativePath);
            return new ZombislayerGenerationAssets {
                EnvironmentModel = environmentModel,
                WeaponModel = weaponModel
            };
        }

    }
}
