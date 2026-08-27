using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Attaches the shared console camera/light Blueprint and prunes the legacy overlay on console targets.
    /// </summary>
    public sealed class ConsoleCameraLightInstructionsSceneAttachmentService {
        /// <summary>
        /// Creates a Blueprint instance root constrained to the console platform set.
        /// </summary>
        /// <param name="projectRootPath">Project root used to resolve supported platform ids.</param>
        /// <param name="assetAuthoringService">Host-owned public capability used to create the Blueprint reference.</param>
        /// <returns>Live Blueprint instance root.</returns>
        public Entity CreateBlueprintInstanceRoot(string projectRootPath, IEditorProjectAssetAuthoringService assetAuthoringService) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("Console camera/light attachment requires an active editor core.");
            }

            Entity root = Core.Instance.EntityFactory.Create("ConsoleCameraLightInstructions");
            root.LayerMask = EditorLayerMasks.SceneObjects;
            root.AddComponent(new BlueprintInstanceComponent {
                BlueprintAssetReference = assetAuthoringService.CreateFileReference(
                    ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath,
                    AssetEntryKind.Blueprint)
            });

            EntitySaveComponent saveComponent = EnsureEntitySaveComponent(root);
            string[] excludedPlatformIds = ["windows", "psp", "psvita", "ds", "3ds"];
            for (int index = 0; index < excludedPlatformIds.Length; index++) {
                saveComponent.GetOrCreateExistencePlatformOverride(excludedPlatformIds[index]).Exists = false;
            }
            return root;
        }

        /// <summary>
        /// Excludes the legacy duplicated overlay from the console targets while preserving Windows and handheld paths.
        /// </summary>
        /// <param name="projectRootPath">Project root used to resolve supported platform ids.</param>
        /// <param name="legacyOverlayRoot">Existing desktop/handheld overlay root.</param>
        public void ExcludeLegacyOverlayFromConsoles(string projectRootPath, Entity legacyOverlayRoot) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (legacyOverlayRoot == null) {
                throw new ArgumentNullException(nameof(legacyOverlayRoot));
            }

            EntitySaveComponent saveComponent = EnsureEntitySaveComponent(legacyOverlayRoot);
            for (int index = 0; index < ConsoleCameraLightInstructionsAssetCatalog.ConsolePlatformIds.Length; index++) {
                string platformId = ConsoleCameraLightInstructionsAssetCatalog.ConsolePlatformIds[index];
                saveComponent.GetOrCreateExistencePlatformOverride(platformId).Exists = false;
            }
        }

        /// <summary>
        /// Resolves the hidden save component attached to one generated entity, creating it when needed.
        /// </summary>
        /// <param name="entity">Entity whose save metadata should be returned.</param>
        /// <returns>Attached save component.</returns>
        static EntitySaveComponent EnsureEntitySaveComponent(Entity entity) {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            EntitySaveComponent createdSaveComponent = new EntitySaveComponent();
            entity.AddComponent(createdSaveComponent);
            return createdSaveComponent;
        }
    }
}
