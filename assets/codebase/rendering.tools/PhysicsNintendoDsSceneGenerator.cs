using city.menu;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Generates Nintendo DS companion scenes for the curated authored physics showcase scenes.
    /// </summary>
    public sealed class PhysicsNintendoDsSceneGenerator {
        /// <summary>
        /// Relative assets subfolder that owns the curated authored physics scenes.
        /// </summary>
        const string PhysicsSceneFolderRelativePath = "scenes/physics";

        /// <summary>
        /// Writer used to persist Nintendo DS companion scenes through the shared generated authored-scene pipeline.
        /// </summary>
        readonly GeneratedAuthoringSceneWriteService SceneWriteService;

        /// <summary>
        /// Initializes one Nintendo DS physics scene generator.
        /// </summary>
        public PhysicsNintendoDsSceneGenerator() {
            SceneWriteService = new GeneratedAuthoringSceneWriteService();
        }

        /// <summary>
        /// Generates Nintendo DS companion scenes for every curated physics showcase scene in the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("Generating Nintendo DS physics scenes requires an active editor core.");
            } else if (Core.Instance.ContentManager == null) {
                throw new InvalidOperationException("Generating Nintendo DS physics scenes requires Core.Instance.ContentManager.");
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            DemoDiscSceneCatalog sceneCatalog = new DemoDiscSceneCatalog();
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries = sceneCatalog.CreatePhysicsSceneEntries();
            for (int index = 0; index < physicsSceneEntries.Count; index++) {
                GenerateCompanionScene(fullProjectRootPath, physicsSceneEntries[index]);
            }
        }

        /// <summary>
        /// Loads one authored physics scene and writes its Nintendo DS companion scene.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute city project root path.</param>
        /// <param name="sceneEntry">Curated physics scene entry being transformed into a DS companion scene.</param>
        void GenerateCompanionScene(string fullProjectRootPath, DemoDiscPhysicsSceneEntry sceneEntry) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (sceneEntry == null) {
                throw new ArgumentNullException(nameof(sceneEntry));
            }

            ComponentPersistenceRegistry persistenceRegistry = GeneratedScenePersistenceRegistryFactory.Create();
            EditorSceneAssetReferenceResolver referenceResolver = new EditorSceneAssetReferenceResolver(Core.Instance.ContentManager, fullProjectRootPath);
            SceneFileLoadService sceneLoadService = new SceneFileLoadService(fullProjectRootPath, persistenceRegistry, referenceResolver);
            string authoredScenePath = Path.Combine(
                fullProjectRootPath,
                "assets",
                PhysicsSceneFolderRelativePath.Replace('/', Path.DirectorySeparatorChar),
                sceneEntry.SceneId + ".helen");
            LoadedEditorSceneDocument loadedScene = sceneLoadService.Load(authoredScenePath);

            try {
                SceneWriteService.WriteNintendoDsCompanionScene(
                    fullProjectRootPath,
                    BuildNintendoDsSceneAssetId(sceneEntry.NintendoDsSceneId),
                    loadedScene.SceneSettings,
                    loadedScene.RootEntities,
                    true,
                    Array.Empty<Entity>());
            } finally {
                DisposeRoots(loadedScene.RootEntities);
            }
        }

        /// <summary>
        /// Builds the project-relative scene asset id for one Nintendo DS companion scene.
        /// </summary>
        /// <param name="nintendoDsSceneId">Logical Nintendo DS scene id.</param>
        /// <returns>Project-relative `.helen` scene id.</returns>
        string BuildNintendoDsSceneAssetId(string nintendoDsSceneId) {
            if (string.IsNullOrWhiteSpace(nintendoDsSceneId)) {
                throw new ArgumentException("Nintendo DS scene id must be provided.", nameof(nintendoDsSceneId));
            }

            return PhysicsSceneFolderRelativePath + "/" + nintendoDsSceneId + ".helen";
        }

        /// <summary>
        /// Disposes the loaded root entities after the companion scene has been written.
        /// </summary>
        /// <param name="roots">Loaded root entities to dispose.</param>
        void DisposeRoots(EditorEntity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                roots[index]?.Dispose();
            }
        }
    }
}
