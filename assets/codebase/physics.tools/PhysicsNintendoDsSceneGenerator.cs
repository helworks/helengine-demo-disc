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
        /// Shared scene-music authoring service used to reattach the looping showcase track after raw scene loads strip the unsupported audio root.
        /// </summary>
        readonly city.scene.tools.GeneratedSceneMusicAuthoringService SceneMusicAuthoringService;

        /// <summary>
        /// Initializes one Nintendo handheld physics scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to load authored gameplay components from the generated physics scenes.</param>
        public PhysicsNintendoDsSceneGenerator(IScriptTypeResolver scriptTypeResolver) {
            ScriptTypeResolver = scriptTypeResolver ?? throw new ArgumentNullException(nameof(scriptTypeResolver));
            SceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolver);
            SceneMusicAuthoringService = new city.scene.tools.GeneratedSceneMusicAuthoringService();
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
            string authoredScenePath = Path.Combine(
                fullProjectRootPath,
                "assets",
                PhysicsSceneFolderRelativePath.Replace('/', Path.DirectorySeparatorChar),
                sceneEntry.SceneId + ".helen");
            SceneAsset authoredSceneAsset = LoadSceneAssetWithoutSharedMusic(authoredScenePath);
            SceneLoadService sceneLoadService = new SceneLoadService(fullProjectRootPath, persistenceRegistry, referenceResolver);
            IReadOnlyList<EditorEntity> loadedRoots = sceneLoadService.Load(authoredSceneAsset);
            Entity[] rootEntities = new Entity[loadedRoots.Count + 1];
            for (int index = 0; index < loadedRoots.Count; index++) {
                rootEntities[index] = loadedRoots[index];
            }
            rootEntities[rootEntities.Length - 1] = SceneMusicAuthoringService.CreateRenderingAndPhysicsMusicEntity();

            try {
                SceneWriteService.WriteScene(fullProjectRootPath, new GeneratedAuthoringSceneDefinition {
                    SceneId = BuildPhysicsSceneAssetId(sceneEntry.SceneId),
                    SceneSettings = authoredSceneAsset.SceneSettings,
                    RootEntities = rootEntities,
                    NintendoDsScene = new GeneratedDsSceneDefinition {
                        UseDefaultBottomOverlay = true,
                        BottomScreenRootEntities = Array.Empty<Entity>()
                    }
                });
            } finally {
                DisposeRoots(rootEntities);
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
        /// Loads one serialized physics scene asset and strips the shared generated music root so the editor scene loader never has to deserialize unsupported audio-backed component members.
        /// </summary>
        /// <param name="authoredScenePath">Absolute path to the serialized physics scene asset.</param>
        /// <returns>Deserialized scene asset with the shared generated music root removed.</returns>
        static SceneAsset LoadSceneAssetWithoutSharedMusic(string authoredScenePath) {
            if (string.IsNullOrWhiteSpace(authoredScenePath)) {
                throw new ArgumentException("Scene path must be provided.", nameof(authoredScenePath));
            }

            using FileStream stream = File.OpenRead(authoredScenePath);
            SceneAsset sceneAsset = global::helengine.editor.AssetSerializer.Deserialize(stream) as SceneAsset
                ?? throw new InvalidOperationException($"Expected '{authoredScenePath}' to contain a SceneAsset payload.");
            StripSharedSceneMusic(sceneAsset);
            return sceneAsset;
        }

        /// <summary>
        /// Removes the shared generated scene-music root and its file-backed audio reference from one serialized scene asset before editor scene load materializes the remaining roots.
        /// </summary>
        /// <param name="sceneAsset">Scene asset whose shared music payload should be removed.</param>
        static void StripSharedSceneMusic(SceneAsset sceneAsset) {
            if (sceneAsset == null) {
                throw new ArgumentNullException(nameof(sceneAsset));
            }

            SceneEntityAsset[] existingRootEntities = sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>();
            List<SceneEntityAsset> filteredRootEntities = new List<SceneEntityAsset>(existingRootEntities.Length);
            for (int index = 0; index < existingRootEntities.Length; index++) {
                SceneEntityAsset rootEntity = existingRootEntities[index];
                if (rootEntity == null) {
                    continue;
                } else if (string.Equals(rootEntity.Name, "SceneMusic", StringComparison.Ordinal)) {
                    continue;
                }

                filteredRootEntities.Add(rootEntity);
            }

            SceneAssetReference[] existingAssetReferences = sceneAsset.AssetReferences ?? Array.Empty<SceneAssetReference>();
            List<SceneAssetReference> filteredAssetReferences = new List<SceneAssetReference>(existingAssetReferences.Length);
            for (int index = 0; index < existingAssetReferences.Length; index++) {
                SceneAssetReference reference = existingAssetReferences[index];
                if (reference == null) {
                    continue;
                } else if (string.Equals(reference.RelativePath, city.scene.tools.GeneratedSceneMusicAuthoringService.RenderingAndPhysicsMusicAudioPath, StringComparison.Ordinal)) {
                    continue;
                }

                filteredAssetReferences.Add(reference);
            }

            sceneAsset.RootEntities = filteredRootEntities.ToArray();
            sceneAsset.AssetReferences = filteredAssetReferences.ToArray();
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
