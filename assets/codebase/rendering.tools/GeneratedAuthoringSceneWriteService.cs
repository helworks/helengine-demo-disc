using helengine.editor;
using System.Reflection;
using city.menu;

namespace city.rendering.tools {
    /// <summary>
    /// Persists generated live-authored scenes through the editor scene save pipeline.
    /// </summary>
    public sealed class GeneratedAuthoringSceneWriteService {
        /// <summary>
        /// Shared Nintendo DS scaffold builder used to derive companion scenes from generated showcase roots.
        /// </summary>
        readonly NintendoDsRenderingSceneScaffoldFactory NintendoDsRenderingSceneScaffoldFactoryValue;

        /// <summary>
        /// Initializes one generated authored-scene writer.
        /// </summary>
        public GeneratedAuthoringSceneWriteService() {
            NintendoDsRenderingSceneScaffoldFactoryValue = new NintendoDsRenderingSceneScaffoldFactory();
        }

        /// <summary>
        /// Writes one generated live-authored scene into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated scene definition to persist.</param>
        public void WriteScene(string projectRootPath, GeneratedAuthoringSceneDefinition sceneDefinition) {
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
            ComponentPersistenceRegistry persistenceRegistry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneSaveService saveService = new SceneSaveService(fullProjectRootPath, persistenceRegistry);
            List<Entity> rootsToDispose = new List<Entity>();

            try {
                AddUniqueRoots(rootsToDispose, sceneDefinition.RootEntities);
                SaveSceneAsset(fullProjectRootPath, saveService, sceneDefinition.SceneId, sceneDefinition.SceneSettings, sceneDefinition.RootEntities);
                if (sceneDefinition.NintendoDsScene != null) {
                    FontAsset bottomOverlayFont = ResolveRequiredNintendoDsDebugFont();
                    Entity[] nintendoDsSceneRoots = sceneDefinition.NintendoDsScene.RootEntities;
                    if (nintendoDsSceneRoots == null || nintendoDsSceneRoots.Length < 1) {
                        nintendoDsSceneRoots = NintendoDsRenderingSceneScaffoldFactoryValue.CreateSceneRoots(
                            sceneDefinition.RootEntities,
                            sceneDefinition.NintendoDsScene.UseDefaultBottomOverlay,
                            sceneDefinition.NintendoDsScene.BottomScreenRootEntities ?? Array.Empty<Entity>(),
                            bottomOverlayFont);
                    }

                    AddUniqueRoots(rootsToDispose, nintendoDsSceneRoots);
                    SaveSceneAsset(fullProjectRootPath, saveService, sceneDefinition.NintendoDsScene.SceneId, sceneDefinition.SceneSettings, nintendoDsSceneRoots);
                }
            } finally {
                DisposeGeneratedRoots(rootsToDispose);
            }
        }

        /// <summary>
        /// Writes one Nintendo DS companion scene from already-authored top-screen roots through the shared DS scaffold path.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneId">Project-relative Nintendo DS scene id to persist.</param>
        /// <param name="sceneSettings">Scene-level settings copied from the authored source scene.</param>
        /// <param name="topScreenRoots">Live authored roots that should remain on the top screen.</param>
        /// <param name="useDefaultBottomOverlay">True when the standard bottom return overlay should be emitted.</param>
        /// <param name="bottomScreenRootEntities">Optional custom bottom-screen roots that should be attached beneath the bottom viewport root.</param>
        public void WriteNintendoDsCompanionScene(
            string projectRootPath,
            string sceneId,
            SceneSettingsAsset sceneSettings,
            Entity[] topScreenRoots,
            bool useDefaultBottomOverlay,
            Entity[] bottomScreenRootEntities) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            } else if (bottomScreenRootEntities == null) {
                throw new ArgumentNullException(nameof(bottomScreenRootEntities));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            ComponentPersistenceRegistry persistenceRegistry = GeneratedScenePersistenceRegistryFactory.Create();
            SceneSaveService saveService = new SceneSaveService(fullProjectRootPath, persistenceRegistry);
            FontAsset bottomOverlayFont = ResolveRequiredNintendoDsDebugFont();
            Entity[] nintendoDsSceneRoots = NintendoDsRenderingSceneScaffoldFactoryValue.CreateSceneRoots(
                topScreenRoots,
                useDefaultBottomOverlay,
                bottomScreenRootEntities,
                bottomOverlayFont);
            List<Entity> rootsToDispose = new List<Entity>();

            try {
                AddUniqueRootsExcept(rootsToDispose, nintendoDsSceneRoots, topScreenRoots);
                SaveSceneAsset(fullProjectRootPath, saveService, sceneId, sceneSettings, nintendoDsSceneRoots);
            } finally {
                DisposeGeneratedRoots(rootsToDispose);
            }
        }

        /// <summary>
        /// Loads the dedicated project font used by the Nintendo DS bottom overlay.
        /// </summary>
        /// <returns>Generated Nintendo DS debug font asset.</returns>
        static FontAsset ResolveRequiredNintendoDsDebugFont() {
            Assembly appAssembly = Assembly.Load("helengine.editor.app");
            Type debugFontFactoryType = appAssembly.GetType("helengine.editor.app.NintendoDsDebugFontFactory", throwOnError: true);
            MethodInfo createFontMethod = debugFontFactoryType.GetMethod("CreateBottomOverlayFont", BindingFlags.Public | BindingFlags.Static);
            if (createFontMethod == null) {
                throw new InvalidOperationException("NintendoDsDebugFontFactory.CreateBottomOverlayFont was not found.");
            }

            object result = createFontMethod.Invoke(null, Array.Empty<object>());
            if (result is not FontAsset fontAsset) {
                throw new InvalidOperationException("Nintendo DS debug font factory did not return a FontAsset.");
            }

            return fontAsset;
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
            NormalizeGeneratedMenuRootInitialPanels(generatedRoots);
            EditorEntityLayerMaskSnapshot[] hiddenRootSnapshots = HideNonTargetSceneRoots(generatedRoots);

            try {
                saveService.Save(scenePath, sceneSettings ?? new SceneSettingsAsset());
            } finally {
                RestoreHiddenUserSceneRoots(hiddenRootSnapshots);
            }
        }

        /// <summary>
        /// Reapplies the authored initial-panel enabled state to every generated baked menu root before serialization so hidden menu panels never leak into persisted scene output.
        /// </summary>
        /// <param name="generatedRoots">Generated scene roots being serialized.</param>
        void NormalizeGeneratedMenuRootInitialPanels(Entity[] generatedRoots) {
            if (generatedRoots == null) {
                throw new ArgumentNullException(nameof(generatedRoots));
            }

            for (int index = 0; index < generatedRoots.Length; index++) {
                NormalizeGeneratedMenuRootInitialPanels(generatedRoots[index]);
            }
        }

        /// <summary>
        /// Walks one generated entity subtree and reapplies authored initial-panel visibility to any baked menu hierarchy rooted inside it.
        /// </summary>
        /// <param name="entity">Generated entity subtree to inspect.</param>
        void NormalizeGeneratedMenuRootInitialPanels(Entity entity) {
            if (entity == null) {
                return;
            }

            if (TryFindFirstComponent(entity, out MenuComponent menuComponent)) {
                ApplyInitialMenuPanelStates(entity, menuComponent.InitialPanelId);
            }

            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                NormalizeGeneratedMenuRootInitialPanels(entity.Children[childIndex]);
            }
        }

        /// <summary>
        /// Applies the supplied initial panel id to every baked panel entity under one generated menu root before the hierarchy is serialized.
        /// </summary>
        /// <param name="menuRootEntity">Generated menu root whose baked panels should be normalized.</param>
        /// <param name="initialPanelId">Stable panel id that should remain enabled in the persisted scene.</param>
        void ApplyInitialMenuPanelStates(Entity menuRootEntity, string initialPanelId) {
            if (menuRootEntity == null) {
                throw new ArgumentNullException(nameof(menuRootEntity));
            }
            if (string.IsNullOrWhiteSpace(initialPanelId)) {
                throw new InvalidOperationException("Generated menu roots must define one initial panel id before serialization.");
            }

            List<Entity> panelEntities = new List<Entity>();
            CollectEntitiesWithComponent<MenuPanelComponent>(menuRootEntity, panelEntities);
            for (int panelIndex = 0; panelIndex < panelEntities.Count; panelIndex++) {
                Entity panelEntity = panelEntities[panelIndex];
                if (!TryFindFirstComponent(panelEntity, out MenuPanelComponent panelComponent)) {
                    continue;
                }

                panelEntity.Enabled = string.Equals(panelComponent.PanelId, initialPanelId, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Collects every entity in one subtree that owns the requested component type.
        /// </summary>
        /// <typeparam name="TComponent">Component type that should be collected.</typeparam>
        /// <param name="entity">Entity subtree to inspect.</param>
        /// <param name="entities">Destination list that receives matching entities.</param>
        void CollectEntitiesWithComponent<TComponent>(Entity entity, List<Entity> entities) where TComponent : Component {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            }

            if (TryFindFirstComponent(entity, out TComponent component)) {
                entities.Add(entity);
            }

            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                CollectEntitiesWithComponent<TComponent>(entity.Children[childIndex], entities);
            }
        }

        /// <summary>
        /// Resolves the first component of the requested type on one entity.
        /// </summary>
        /// <typeparam name="TComponent">Component type to resolve.</typeparam>
        /// <param name="entity">Entity whose component list should be scanned.</param>
        /// <param name="component">Resolved component when present; otherwise null.</param>
        /// <returns>True when a matching component was found on the entity.</returns>
        bool TryFindFirstComponent<TComponent>(Entity entity, out TComponent component) where TComponent : Component {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            component = null;
            if (entity.Components == null) {
                return false;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is not TComponent typedComponent) {
                    continue;
                }

                component = typedComponent;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Temporarily hides every non-target scene root so the editor serializer only sees the generated roots being written.
        /// </summary>
        /// <param name="generatedRoots">Generated roots that should remain visible to the serializer.</param>
        /// <returns>Snapshots used to restore the hidden roots.</returns>
        EditorEntityLayerMaskSnapshot[] HideNonTargetSceneRoots(Entity[] generatedRoots) {
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
        void DisposeGeneratedRoots(List<Entity> generatedRoots) {
            if (generatedRoots == null) {
                return;
            }

            for (int index = 0; index < generatedRoots.Count; index++) {
                if (generatedRoots[index] != null) {
                    generatedRoots[index].Dispose();
                }
            }
        }

        /// <summary>
        /// Adds one root-entity set to the pending disposal list without duplicating shared root references.
        /// </summary>
        /// <param name="pendingRoots">Accumulated root entities that should be disposed.</param>
        /// <param name="candidateRoots">Root entities produced by the current scene-generation step.</param>
        void AddUniqueRoots(List<Entity> pendingRoots, Entity[] candidateRoots) {
            if (pendingRoots == null) {
                throw new ArgumentNullException(nameof(pendingRoots));
            } else if (candidateRoots == null) {
                return;
            }

            for (int index = 0; index < candidateRoots.Length; index++) {
                Entity rootEntity = candidateRoots[index];
                if (rootEntity == null || pendingRoots.Contains(rootEntity)) {
                    continue;
                }

                pendingRoots.Add(rootEntity);
            }
        }

        /// <summary>
        /// Adds one root-entity set to the pending disposal list while excluding caller-owned shared roots that should remain under external ownership.
        /// </summary>
        /// <param name="pendingRoots">Accumulated root entities that should be disposed.</param>
        /// <param name="candidateRoots">Root entities produced by the current scene-generation step.</param>
        /// <param name="excludedRoots">Caller-owned root entities that must not be disposed by this service.</param>
        void AddUniqueRootsExcept(List<Entity> pendingRoots, Entity[] candidateRoots, Entity[] excludedRoots) {
            if (pendingRoots == null) {
                throw new ArgumentNullException(nameof(pendingRoots));
            } else if (candidateRoots == null) {
                return;
            } else if (excludedRoots == null) {
                throw new ArgumentNullException(nameof(excludedRoots));
            }

            for (int index = 0; index < candidateRoots.Length; index++) {
                Entity rootEntity = candidateRoots[index];
                if (rootEntity == null || pendingRoots.Contains(rootEntity) || Array.IndexOf(excludedRoots, rootEntity) >= 0) {
                    continue;
                }

                pendingRoots.Add(rootEntity);
            }
        }
    }
}
