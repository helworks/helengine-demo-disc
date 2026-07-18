using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Generates the shared console camera/light instruction Blueprint.
    /// </summary>
    public sealed class ConsoleCameraLightInstructionsBlueprintGenerator {
        /// <summary>
        /// Generates and serializes the shared console instruction root.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets folder.</param>
        /// <param name="overlayFactory">Factory used to author the temporary Blueprint root.</param>
        /// <param name="font">Font used by the Blueprint's camera and light labels.</param>
        public void Generate(string projectRootPath, DemoSceneInstructionOverlayFactory overlayFactory, FontAsset font) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (overlayFactory == null) {
                throw new ArgumentNullException(nameof(overlayFactory));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("Console camera/light Blueprint generation requires an active editor core.");
            }

            Entity root = overlayFactory.CreateConsoleCameraLightInstructionsRoot(projectRootPath, font);
            try {
                string fullPath = Path.Combine(
                    Path.GetFullPath(projectRootPath),
                    "assets",
                    ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Blueprint directory could not be resolved."));
                BlueprintSaveService saveService = new BlueprintSaveService(
                    projectRootPath,
                    GeneratedScenePersistenceRegistryFactory.Create());
                saveService.Save(fullPath);
            } finally {
                root.Dispose();
            }
        }
    }
}
