using city.menu;
using city.rendering.tools;

namespace city.physics.tools {
    /// <summary>
    /// Generates Nintendo DS companion scenes for the curated authored physics showcase scenes.
    /// </summary>
    public sealed class PhysicsNintendoDsSceneGenerator {
        /// <summary>
        /// Relative assets subfolder that owns the curated authored physics scenes.
        /// </summary>
        const string PhysicsSceneFolderRelativePath = "scenes/physics";

        /// <summary>
        /// Stable desktop showcase UI root name that should not remain in DS direct-start physics scenes.
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

            if (IsPlayablePhysicsShowcaseScene(sceneEntry)) {
                WritePlayablePhysicsShowcaseCompanionScene(fullProjectRootPath, sceneEntry);
                return;
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
        /// Builds one playable physics showcase DS companion scene directly from the shared live scene-definition path so the DS workflow does not depend on the editor-only reloadability of the desktop `.helen` file.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute city project root path.</param>
        /// <param name="sceneEntry">Curated playable physics showcase scene entry being transformed into a DS companion scene.</param>
        void WritePlayablePhysicsShowcaseCompanionScene(string fullProjectRootPath, DemoDiscPhysicsSceneEntry sceneEntry) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (sceneEntry == null) {
                throw new ArgumentNullException(nameof(sceneEntry));
            }

            PhysicsSceneFactory physicsSceneFactory = new PhysicsSceneFactory();
            GeneratedAuthoringSceneDefinition sceneDefinition = physicsSceneFactory.CreatePlayablePhysicsShowcaseSceneDefinition(
                fullProjectRootPath,
                sceneEntry.SceneId,
                false);
            Entity[] topScreenRoots = RemoveDesktopShowcaseUiRoot(sceneDefinition.RootEntities);
            uint nextEntityId = 1u;
            AssignFreshGeneratedEntityIds(topScreenRoots, nextEntityId);
            try {
                SceneWriteService.WriteNintendoDsCompanionScene(
                    fullProjectRootPath,
                    BuildNintendoDsSceneAssetId(sceneEntry.NintendoDsSceneId),
                    sceneDefinition.SceneSettings,
                    topScreenRoots,
                    true,
                    Array.Empty<Entity>());
            } finally {
                DisposeRoots(sceneDefinition.RootEntities);
            }
        }

        /// <summary>
        /// Removes the desktop showcase UI root from one generated playable physics showcase root set before the DS scaffold persists it.
        /// </summary>
        /// <param name="roots">Generated playable physics showcase roots.</param>
        /// <returns>Generated roots without the desktop showcase UI root.</returns>
        static Entity[] RemoveDesktopShowcaseUiRoot(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            List<Entity> filteredRoots = new List<Entity>(roots.Length);
            for (int index = 0; index < roots.Length; index++) {
                Entity root = roots[index];
                if (root == null) {
                    continue;
                } else if (root is EditorEntity editorRoot
                    && string.Equals(editorRoot.Name, DesktopShowcaseUiRootName, StringComparison.Ordinal)) {
                    continue;
                }

                filteredRoots.Add(root);
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
        void DisposeRoots(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                roots[index]?.Dispose();
            }
        }

        /// <summary>
        /// Assigns fresh save-component entity ids across one generated root set before the shared authoring save pipeline persists the DS companion scene.
        /// </summary>
        /// <param name="roots">Generated roots that should receive fresh ids.</param>
        /// <param name="nextEntityId">First id available for assignment.</param>
        /// <returns>Next unassigned id after the supplied root set has been processed.</returns>
        static uint AssignFreshGeneratedEntityIds(Entity[] roots, uint nextEntityId) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            } else if (nextEntityId == 0u) {
                throw new ArgumentOutOfRangeException(nameof(nextEntityId), "Generated entity ids must start at a non-zero value.");
            }

            uint currentEntityId = nextEntityId;
            for (int index = 0; index < roots.Length; index++) {
                if (roots[index] is not EditorEntity editorRootEntity) {
                    throw new InvalidOperationException("Nintendo DS physics scene roots must be editor entities before they can be saved.");
                }

                currentEntityId = AssignFreshGeneratedEntityIds(editorRootEntity, currentEntityId);
            }

            return currentEntityId;
        }

        /// <summary>
        /// Assigns fresh save-component entity ids across one generated editor subtree.
        /// </summary>
        /// <param name="entity">Generated editor subtree root that should receive fresh ids.</param>
        /// <param name="nextEntityId">First id available for assignment.</param>
        /// <returns>Next unassigned id after the subtree has been processed.</returns>
        static uint AssignFreshGeneratedEntityIds(EditorEntity entity, uint nextEntityId) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (nextEntityId == 0u) {
                throw new ArgumentOutOfRangeException(nameof(nextEntityId), "Generated entity ids must start at a non-zero value.");
            }

            EntitySaveComponent saveComponent = EnsureEntitySaveComponent(entity);
            saveComponent.EntityId = nextEntityId;
            uint currentEntityId = nextEntityId + 1u;
            if (entity.Children == null) {
                return currentEntityId;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (entity.Children[childIndex] is not EditorEntity childEntity) {
                    continue;
                }

                currentEntityId = AssignFreshGeneratedEntityIds(childEntity, currentEntityId);
            }

            return currentEntityId;
        }

        /// <summary>
        /// Resolves the hidden save component attached to one live editor entity, creating it when a freshly generated subtree has not received one yet.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached save component.</returns>
        static EntitySaveComponent EnsureEntitySaveComponent(EditorEntity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            EntitySaveComponent createdSaveComponent = new EntitySaveComponent();
            entity.AddComponent(createdSaveComponent);
            return createdSaveComponent;
        }

    }
}
