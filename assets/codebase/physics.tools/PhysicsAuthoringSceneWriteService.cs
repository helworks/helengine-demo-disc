using helengine.editor;

namespace city.physics.tools {
    /// <summary>
    /// Persists generated physics scenes through the editor scene save pipeline instead of owning component serialization in city code.
    /// </summary>
    public sealed class PhysicsAuthoringSceneWriteService {
        /// <summary>
        /// Writes one generated live-authored physics scene into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated physics scene definition to persist.</param>
        public void WriteScene(string projectRootPath, PhysicsAuthoringSceneDefinition sceneDefinition) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (string.IsNullOrWhiteSpace(sceneDefinition.SceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneDefinition));
            } else if (sceneDefinition.RootEntities == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            ComponentPersistenceRegistry persistenceRegistry = CreatePersistenceRegistry();
            SceneSaveService saveService = new SceneSaveService(fullProjectRootPath, persistenceRegistry);
            EditorEntityLayerMaskSnapshot[] hiddenRootSnapshots = Array.Empty<EditorEntityLayerMaskSnapshot>();

            try {
                hiddenRootSnapshots = HideExistingUserSceneRoots(sceneDefinition.RootEntities);
                SaveSceneAsset(fullProjectRootPath, saveService, sceneDefinition.SceneId, sceneDefinition.SceneSettings, sceneDefinition.RootEntities);
            } finally {
                RestoreHiddenUserSceneRoots(hiddenRootSnapshots);
                DisposeGeneratedRoots(sceneDefinition.RootEntities);
            }
        }

        /// <summary>
        /// Saves one generated scene asset with the supplied id, settings, and currently live generated roots.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute project root path.</param>
        /// <param name="saveService">Scene save service writing the current editor scene.</param>
        /// <param name="sceneId">Project-relative scene id to persist.</param>
        /// <param name="sceneSettings">Scene-level settings to persist.</param>
        /// <param name="generatedRoots">Currently live generated roots visible to the serializer.</param>
        void SaveSceneAsset(
            string fullProjectRootPath,
            SceneSaveService saveService,
            string sceneId,
            SceneSettingsAsset sceneSettings,
            Entity[] generatedRoots) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (saveService == null) {
                throw new ArgumentNullException(nameof(saveService));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (generatedRoots == null) {
                throw new ArgumentNullException(nameof(generatedRoots));
            }

            string scenePath = Path.Combine(fullProjectRootPath, "assets", sceneId.Replace('/', Path.DirectorySeparatorChar));
            saveService.Save(scenePath, sceneSettings ?? new SceneSettingsAsset());
        }

        /// <summary>
        /// Creates the persistence registry used by generated physics scene save operations.
        /// </summary>
        /// <returns>Configured scene persistence registry with editor and physics component descriptors.</returns>
        ComponentPersistenceRegistry CreatePersistenceRegistry() {
            ComponentPersistenceRegistry persistenceRegistry = new ComponentPersistenceRegistry();
            persistenceRegistry.Register(new MeshComponentPersistenceDescriptor());
            persistenceRegistry.Register(new CameraComponentPersistenceDescriptor());
            persistenceRegistry.Register(new DebugComponentPersistenceDescriptor());
            persistenceRegistry.Register(new DirectionalLightComponentPersistenceDescriptor());
            persistenceRegistry.Register(new RigidBody3DComponentPersistenceDescriptor());
            persistenceRegistry.Register(new BoxCollider3DComponentPersistenceDescriptor());
            persistenceRegistry.Register(new CharacterController3DComponentPersistenceDescriptor());
            persistenceRegistry.Register(new KinematicMotion3DComponentPersistenceDescriptor());
            return persistenceRegistry;
        }

        /// <summary>
        /// Temporarily hides pre-existing user scene roots so the editor serializer only sees the generated roots being written.
        /// </summary>
        /// <param name="generatedRoots">Generated roots that should remain visible to the serializer.</param>
        /// <returns>Snapshots used to restore the hidden roots.</returns>
        EditorEntityLayerMaskSnapshot[] HideExistingUserSceneRoots(Entity[] generatedRoots) {
            if (generatedRoots == null) {
                throw new ArgumentNullException(nameof(generatedRoots));
            }

            HashSet<EditorEntity> generatedRootSet = new HashSet<EditorEntity>();
            for (int index = 0; index < generatedRoots.Length; index++) {
                if (generatedRoots[index] is EditorEntity editorGeneratedRoot) {
                    generatedRootSet.Add(editorGeneratedRoot);
                }
            }

            List<EditorEntityLayerMaskSnapshot> snapshots = new List<EditorEntityLayerMaskSnapshot>();
            List<Entity> liveEntities = Core.Instance.ObjectManager.Entities;
            for (int index = 0; index < liveEntities.Count; index++) {
                if (liveEntities[index] is not EditorEntity editorEntity) {
                    continue;
                } else if (generatedRootSet.Contains(editorEntity)) {
                    continue;
                } else if (editorEntity.Parent != null) {
                    continue;
                } else if (editorEntity.InternalEntity) {
                    continue;
                } else if (editorEntity.LayerMask != EditorLayerMasks.SceneObjects) {
                    continue;
                }

                snapshots.Add(new EditorEntityLayerMaskSnapshot(editorEntity, editorEntity.LayerMask));
                editorEntity.LayerMask = 0;
            }

            return snapshots.ToArray();
        }

        /// <summary>
        /// Restores the user scene roots that were temporarily hidden during scene save.
        /// </summary>
        /// <param name="snapshots">Root snapshots captured before the save operation.</param>
        void RestoreHiddenUserSceneRoots(EditorEntityLayerMaskSnapshot[] snapshots) {
            if (snapshots == null) {
                return;
            }

            for (int index = 0; index < snapshots.Length; index++) {
                snapshots[index].Entity.LayerMask = snapshots[index].LayerMask;
            }
        }

        /// <summary>
        /// Disposes every generated root created for the current save operation.
        /// </summary>
        /// <param name="generatedRoots">Generated roots to dispose.</param>
        void DisposeGeneratedRoots(Entity[] generatedRoots) {
            if (generatedRoots == null) {
                return;
            }

            for (int index = 0; index < generatedRoots.Length; index++) {
                if (generatedRoots[index] != null) {
                    generatedRoots[index].Dispose();
                }
            }
        }
    }
}
