using city.menu;
using city.rendering.tools;

namespace city.physics.tools {
    /// <summary>
    /// Augments curated physics showcase scenes with Nintendo handheld-only roots inside their canonical authored scene assets.
    /// </summary>
    public sealed class PhysicsNintendoDsSceneGenerator {
        /// <summary>
        /// Relative assets subfolder that owns the curated authored physics scenes.
        /// </summary>
        const string PhysicsSceneFolderRelativePath = "scenes/physics";

        /// <summary>
        /// Writer used to persist canonical scenes through the shared generated authored-scene pipeline.
        /// </summary>
        readonly GeneratedAuthoringSceneWriteService SceneWriteService;

        /// <summary>
        /// Resolver backed by the currently loaded city gameplay assemblies.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolver;

        /// <summary>
        /// Initializes one Nintendo handheld physics scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to load authored gameplay components from the generated physics scenes.</param>
        public PhysicsNintendoDsSceneGenerator(IScriptTypeResolver scriptTypeResolver) {
            ScriptTypeResolver = scriptTypeResolver ?? throw new ArgumentNullException(nameof(scriptTypeResolver));
            SceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolver);
        }

        /// <summary>
        /// Rewrites every curated physics showcase scene so Nintendo handheld builds consume handheld-only roots from the canonical scene asset.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("Generating Nintendo handheld physics scenes requires an active editor core.");
            } else if (Core.Instance.ContentManager == null) {
                throw new InvalidOperationException("Generating Nintendo handheld physics scenes requires Core.Instance.ContentManager.");
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            DemoDiscSceneCatalog sceneCatalog = new DemoDiscSceneCatalog();
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries = sceneCatalog.CreatePhysicsNintendoHandheldSceneEntries();
            DeleteObsoleteNintendoHandheldCompanionScenes(fullProjectRootPath, physicsSceneEntries);
            for (int index = 0; index < physicsSceneEntries.Count; index++) {
                RewriteSceneWithNintendoHandheldAugmentation(fullProjectRootPath, physicsSceneEntries[index]);
            }
        }

        /// <summary>
        /// Deletes the obsolete Nintendo handheld companion physics scenes so stale generated output does not remain discoverable in the project scene catalog.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute city project root path.</param>
        static void DeleteObsoleteNintendoHandheldCompanionScenes(string fullProjectRootPath, IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (physicsSceneEntries == null) {
                throw new ArgumentNullException(nameof(physicsSceneEntries));
            }

            for (int index = 0; index < physicsSceneEntries.Count; index++) {
                string obsoleteScenePath = Path.Combine(
                    fullProjectRootPath,
                    "assets",
                    "scenes",
                    "physics",
                    physicsSceneEntries[index].SceneId + "_ds.helen");
                if (File.Exists(obsoleteScenePath)) {
                    File.Delete(obsoleteScenePath);
                }
            }

            string obsoleteMatrixProbeScenePath = Path.Combine(fullProjectRootPath, "assets", "scenes", "physics", "test_scene_render_matrix_probe_ds.helen");
            if (File.Exists(obsoleteMatrixProbeScenePath)) {
                File.Delete(obsoleteMatrixProbeScenePath);
            }
        }

        /// <summary>
        /// Loads one authored or generated physics scene and rewrites its canonical asset with Nintendo handheld-only augmentation roots.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute city project root path.</param>
        /// <param name="sceneEntry">Curated physics scene entry being rewritten for handheld augmentation.</param>
        void RewriteSceneWithNintendoHandheldAugmentation(string fullProjectRootPath, DemoDiscPhysicsSceneEntry sceneEntry) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (sceneEntry == null) {
                throw new ArgumentNullException(nameof(sceneEntry));
            }

            if (IsPlayablePhysicsShowcaseScene(sceneEntry)) {
                RewritePlayablePhysicsShowcaseScene(fullProjectRootPath, sceneEntry);
                return;
            }

            ComponentPersistenceRegistry persistenceRegistry = GeneratedScenePersistenceRegistryFactory.Create(ScriptTypeResolver);
            AssetImportManager assetImportManager = GeneratedAuthoringSceneWriteService.CreateGeneratedSceneAssetImportManager(fullProjectRootPath);
            EditorFileSystemModelResolver fileSystemModelResolver = new EditorFileSystemModelResolver(assetImportManager);
            EditorFileSystemFontResolver fileSystemFontResolver = new EditorFileSystemFontResolver(assetImportManager);
            EditorFileSystemTextureResolver fileSystemTextureResolver = new EditorFileSystemTextureResolver(assetImportManager);
            EditorSceneAssetReferenceResolver referenceResolver = new EditorSceneAssetReferenceResolver(
                assetImportManager.ContentManager,
                fullProjectRootPath,
                fileSystemModelResolver,
                fileSystemFontResolver,
                fileSystemTextureResolver);
            SceneFileLoadService sceneLoadService = new SceneFileLoadService(fullProjectRootPath, persistenceRegistry, referenceResolver);
            string authoredScenePath = Path.Combine(
                fullProjectRootPath,
                "assets",
                PhysicsSceneFolderRelativePath.Replace('/', Path.DirectorySeparatorChar),
                sceneEntry.SceneId + ".helen");
            LoadedEditorSceneDocument loadedScene = sceneLoadService.Load(authoredScenePath);

            try {
                SceneWriteService.WriteScene(fullProjectRootPath, new GeneratedAuthoringSceneDefinition {
                    SceneId = BuildPhysicsSceneAssetId(sceneEntry.SceneId),
                    SceneSettings = loadedScene.SceneSettings,
                    RootEntities = loadedScene.RootEntities,
                    NintendoDsScene = new GeneratedDsSceneDefinition {
                        UseDefaultBottomOverlay = false,
                        BottomScreenRootEntities = Array.Empty<Entity>()
                    }
                });
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
                || string.Equals(sceneEntry.SceneId, "test_scene_dynamic_mixed_stack", StringComparison.Ordinal)
                || string.Equals(sceneEntry.SceneId, "test_scene_static_mesh_showcase", StringComparison.Ordinal)
                || string.Equals(sceneEntry.SceneId, "test_scene_static_mesh_minimal", StringComparison.Ordinal);
        }

        /// <summary>
        /// Rewrites one playable physics showcase scene directly from the shared live scene-definition path so the handheld workflow does not depend on reloading an intermediate desktop `.helen` file.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute city project root path.</param>
        /// <param name="sceneEntry">Curated playable physics showcase scene entry being rewritten for handheld augmentation.</param>
        void RewritePlayablePhysicsShowcaseScene(string fullProjectRootPath, DemoDiscPhysicsSceneEntry sceneEntry) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (sceneEntry == null) {
                throw new ArgumentNullException(nameof(sceneEntry));
            }

            PhysicsSceneFactory physicsSceneFactory = new PhysicsSceneFactory();
            GeneratedAuthoringSceneDefinition sceneDefinition = physicsSceneFactory.CreatePlayablePhysicsShowcaseSceneDefinition(
                fullProjectRootPath,
                sceneEntry.SceneId,
                true);
            sceneDefinition.NintendoDsScene = new GeneratedDsSceneDefinition {
                UseDefaultBottomOverlay = false,
                BottomScreenRootEntities = Array.Empty<Entity>()
            };
            try {
                SceneWriteService.WriteScene(fullProjectRootPath, sceneDefinition);
            } finally {
                DisposeRoots(sceneDefinition.RootEntities);
            }
        }

        /// <summary>
        /// Builds the project-relative scene asset id for one authored physics scene.
        /// </summary>
        /// <param name="sceneId">Logical physics scene id.</param>
        /// <returns>Project-relative `.helen` scene id.</returns>
        string BuildPhysicsSceneAssetId(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            return PhysicsSceneFolderRelativePath + "/" + sceneId + ".helen";
        }

        /// <summary>
        /// Disposes the loaded root entities after the companion scene has been written.
        /// </summary>
        /// <param name="roots">Loaded root entities to dispose.</param>
        void DisposeRoots(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                roots[index]?.Dispose();
            }
        }
    }
}
