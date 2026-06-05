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
        /// Stable desktop instruction overlay viewport root name that should not remain on the DS top screen.
        /// </summary>
        const string DesktopInstructionOverlayRootName = "DemoSceneInstructionViewport";

        /// <summary>
        /// Stable desktop showcase UI root name that should not remain in the DS playable physics companion scenes.
        /// </summary>
        const string DesktopShowcaseUiRootName = "ShowcaseUi";

        /// <summary>
        /// Writer used to persist Nintendo DS companion scenes through the shared generated authored-scene pipeline.
        /// </summary>
        readonly GeneratedAuthoringSceneWriteService SceneWriteService;

        /// <summary>
        /// Resolver backed by the currently loaded city gameplay assemblies.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolver;

        /// <summary>
        /// Initializes one Nintendo DS physics scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to load authored gameplay components from the generated physics scenes.</param>
        public PhysicsNintendoDsSceneGenerator(IScriptTypeResolver scriptTypeResolver) {
            ScriptTypeResolver = scriptTypeResolver ?? throw new ArgumentNullException(nameof(scriptTypeResolver));
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

            ComponentPersistenceRegistry persistenceRegistry = GeneratedScenePersistenceRegistryFactory.Create(ScriptTypeResolver);
            EditorSceneAssetReferenceResolver referenceResolver = new EditorSceneAssetReferenceResolver(Core.Instance.ContentManager, fullProjectRootPath);
            SceneFileLoadService sceneLoadService = new SceneFileLoadService(fullProjectRootPath, persistenceRegistry, referenceResolver);
            string authoredScenePath = Path.Combine(
                fullProjectRootPath,
                "assets",
                PhysicsSceneFolderRelativePath.Replace('/', Path.DirectorySeparatorChar),
                sceneEntry.SceneId + ".helen");
            LoadedEditorSceneDocument loadedScene = sceneLoadService.Load(authoredScenePath);

            try {
                Entity[] bottomScreenRoots = Array.Empty<Entity>();
                Entity[] topScreenRoots = loadedScene.RootEntities;
                if (IsPlayablePhysicsShowcaseScene(sceneEntry)) {
                    topScreenRoots = RemoveDesktopOnlyRoots(loadedScene.RootEntities);
                }

                SceneWriteService.WriteNintendoDsCompanionScene(
                    fullProjectRootPath,
                    BuildNintendoDsSceneAssetId(sceneEntry.NintendoDsSceneId),
                    loadedScene.SceneSettings,
                    topScreenRoots,
                    true,
                    bottomScreenRoots);
            } finally {
                DisposeRoots(loadedScene.RootEntities);
            }
        }

        /// <summary>
        /// Returns whether the supplied curated scene entry belongs to the three playable physics showcase scenes that need instruction overlays.
        /// </summary>
        /// <param name="sceneEntry">Curated physics scene entry being evaluated.</param>
        /// <returns>True when the entry belongs to the playable physics showcase set.</returns>
        bool IsPlayablePhysicsShowcaseScene(DemoDiscPhysicsSceneEntry sceneEntry) {
            if (sceneEntry == null) {
                throw new ArgumentNullException(nameof(sceneEntry));
            }

            return string.Equals(sceneEntry.SceneId, "test_scene_dynamic_stack_boxes", StringComparison.Ordinal)
                || string.Equals(sceneEntry.SceneId, "test_scene_dynamic_sphere_stack", StringComparison.Ordinal)
                || string.Equals(sceneEntry.SceneId, "test_scene_dynamic_mixed_stack", StringComparison.Ordinal);
        }

        /// <summary>
        /// Removes desktop-only overlay and showcase UI roots from one playable showcase scene before the DS scaffold reuses the authored top-screen roots.
        /// </summary>
        /// <param name="rootEntities">Authored top-screen roots loaded from the desktop scene file.</param>
        /// <returns>Top-screen roots without desktop-only UI overlays.</returns>
        static Entity[] RemoveDesktopOnlyRoots(EditorEntity[] rootEntities) {
            if (rootEntities == null) {
                throw new ArgumentNullException(nameof(rootEntities));
            }

            List<Entity> filteredRoots = new List<Entity>(rootEntities.Length);
            for (int index = 0; index < rootEntities.Length; index++) {
                EditorEntity rootEntity = rootEntities[index];
                if (rootEntity == null) {
                    continue;
                } else if (string.Equals(rootEntity.Name, DesktopInstructionOverlayRootName, StringComparison.Ordinal)) {
                    continue;
                } else if (string.Equals(rootEntity.Name, DesktopShowcaseUiRootName, StringComparison.Ordinal)) {
                    continue;
                }

                filteredRoots.Add(rootEntity);
            }

            return filteredRoots.ToArray();
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

        /// <summary>
    }
}
