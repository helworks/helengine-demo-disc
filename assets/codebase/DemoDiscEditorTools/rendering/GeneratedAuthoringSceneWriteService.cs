using helengine.editor;
using DemoDisc.menu;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Persists generated live-authored scenes through the editor scene save pipeline.
    /// </summary>
    public sealed class GeneratedAuthoringSceneWriteService {
        /// <summary>
        /// Shared Nintendo DS scaffold builder used to derive companion scenes from generated showcase roots.
        /// </summary>
        readonly NintendoDsRenderingSceneScaffoldFactory NintendoDsRenderingSceneScaffoldFactoryValue;

        /// <summary>
        /// High-level editor helper used to author platform-exclusive entity subtrees without touching low-level save metadata directly.
        /// </summary>
        readonly PlatformSceneAuthoringHelperService PlatformSceneAuthoringHelperServiceValue;


        /// <summary>
        /// Resolver backed by the currently loaded city gameplay assemblies so temporary clone round-trips can restore project-authored components.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolverValue;

        /// <summary>
        /// Host-owned asset-authoring capability used to resolve file-backed scene references.
        /// </summary>
        readonly IEditorProjectAuthoringSession AuthoringSession;

        /// <summary>
        /// Caller-owned transaction that publishes every generated scene output atomically.
        /// </summary>
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Lazily created scope rewriter backing <see cref="GroupFirstScopeRewriteServiceValue"/>.
        /// </summary>
        GeneratedSceneGroupFirstScopeRewriteService GroupFirstScopeRewriteServiceStorage;

        /// <summary>
        /// Canonical project root supplied by the owning authoring session.
        /// </summary>
        string ProjectRootPath => Path.GetFullPath(AuthoringSession.ProjectRootPath);

        /// <summary>
        /// Gets the scope rewriter used when a scene gains Nintendo dual-screen augmentation, created on first use
        /// so scenes without a handheld companion never read the platform group settings.
        /// </summary>
        GeneratedSceneGroupFirstScopeRewriteService GroupFirstScopeRewriteServiceValue =>
            GroupFirstScopeRewriteServiceStorage ??= new GeneratedSceneGroupFirstScopeRewriteService(ProjectRootPath);

        /// <summary>
        /// Initializes one generated authored-scene writer with a project component resolver and the required host capability.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to restore project-authored components during temporary clone loads.</param>
        /// <param name="assetAuthoringService">Required host-owned asset-authoring capability used to resolve source assets.</param>
        public GeneratedAuthoringSceneWriteService(
            IScriptTypeResolver scriptTypeResolver,
            IEditorProjectAuthoringSession authoringSession,
            EditorAuthoringTransaction transaction) {
            if (authoringSession == null) {
                throw new ArgumentNullException(nameof(authoringSession));
            }

            AuthoringSession = authoringSession;
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            NintendoDsRenderingSceneScaffoldFactoryValue = new NintendoDsRenderingSceneScaffoldFactory(authoringSession);
            PlatformSceneAuthoringHelperServiceValue = new PlatformSceneAuthoringHelperService();
            ScriptTypeResolverValue = scriptTypeResolver;
        }

        /// <summary>
        /// Writes one generated live-authored scene into the supplied city project.
        /// </summary>
        /// <param name="sceneDefinition">Generated scene definition to persist.</param>
        public void WriteScene(GeneratedAuthoringSceneDefinition sceneDefinition) {
            if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (string.IsNullOrWhiteSpace(sceneDefinition.SceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneDefinition));
            } else if (!string.IsNullOrWhiteSpace(sceneDefinition.SceneAssetRelativePath) && Path.IsPathRooted(sceneDefinition.SceneAssetRelativePath)) {
                throw new ArgumentException("Scene asset relative path must be project-relative when provided.", nameof(sceneDefinition));
            } else if (sceneDefinition.RootEntities == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            }

            ComponentPersistenceRegistry persistenceRegistry = GeneratedScenePersistenceRegistryFactory.Create(ScriptTypeResolverValue);
            List<Entity> rootsToDispose = new List<Entity>();

            try {
                Entity[] desktopPresentationRoots = sceneDefinition.DesktopPresentationRootEntities ?? Array.Empty<Entity>();
                AddUniqueRoots(rootsToDispose, sceneDefinition.RootEntities);
                AddUniqueRoots(rootsToDispose, desktopPresentationRoots);
                AdoptGroupFirstLevelOrderForRoots(sceneDefinition.RootEntities);
                AdoptGroupFirstLevelOrderForRoots(desktopPresentationRoots);
                Entity[] rootsToWrite = CombineRootSets(sceneDefinition.RootEntities, desktopPresentationRoots);
                if (sceneDefinition.NintendoDsScene != null) {
                    // The content roots are shared: only the presentation varies by group, so the single-screen
                    // presentation steps aside on the dual-screen rigs and the handheld one takes its place.
                    Entity[] nintendoDsSceneRoots = BuildNintendoHandheldSceneRoots(
                        sceneDefinition);
                    AddUniqueRoots(rootsToDispose, nintendoDsSceneRoots);
                    ExcludeRootsFromNintendoHandheldPlatforms(desktopPresentationRoots);
                    RestrictRootsToNintendoHandheldPlatforms(nintendoDsSceneRoots);
                    // Generators number their own roots, some from a private counter, so the dual-screen
                    // presentation takes ids above everything already in the scene rather than risking a clash.
                    AssignEntityIdsAbove(nintendoDsSceneRoots, FindMaximumEntityId(rootsToWrite));
                    rootsToWrite = CombineRootSets(rootsToWrite, nintendoDsSceneRoots);
                }

                SaveSceneAsset(
                    sceneDefinition.SceneId,
                    sceneDefinition.SceneAssetRelativePath,
                    sceneDefinition.SceneSettings,
                    rootsToWrite,
                    persistenceRegistry,
                    sceneDefinition.AuthoringAssetId);
            } finally {
                DisposeGeneratedRoots(rootsToDispose);
            }
        }

        /// <summary>
        /// Builds the Nintendo handheld root augmentation that should be merged into the canonical scene asset before it is saved.
        /// </summary>
        /// <param name="sceneDefinition">Generated scene definition being persisted.</param>
        /// <returns>Nintendo handheld-only roots that should be appended to the canonical scene.</returns>
        Entity[] BuildNintendoHandheldSceneRoots(
            GeneratedAuthoringSceneDefinition sceneDefinition) {
            if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (sceneDefinition.NintendoDsScene == null) {
                throw new InvalidOperationException("Nintendo handheld scene roots require a Nintendo DS scene definition.");
            }

            Entity[] authoredNintendoHandheldRoots = sceneDefinition.NintendoDsScene.RootEntities;
            if (authoredNintendoHandheldRoots != null && authoredNintendoHandheldRoots.Length > 0) {
                return authoredNintendoHandheldRoots;
            }

            return NintendoDsRenderingSceneScaffoldFactoryValue.CreateBottomScreenRoots(
                sceneDefinition.NintendoDsScene.BottomScreenRootEntities ?? Array.Empty<Entity>());
        }

        /// <summary>
        /// Saves one generated scene asset with the supplied id, settings, and currently live generated roots.
        /// </summary>
        /// <param name="sceneId">Project-relative scene id to persist.</param>
        /// <param name="sceneSettings">Scene-level settings to persist.</param>
        /// <param name="generatedRoots">Currently live generated roots visible to the serializer.</param>
        void SaveSceneAsset(
            string sceneId,
            string sceneAssetRelativePath,
            SceneSettingsAsset sceneSettings,
            Entity[] generatedRoots,
            ComponentPersistenceRegistry persistenceRegistry,
            string authoringAssetId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (!string.IsNullOrWhiteSpace(sceneAssetRelativePath) && Path.IsPathRooted(sceneAssetRelativePath)) {
                throw new ArgumentException("Scene asset relative path must be project-relative when provided.", nameof(sceneAssetRelativePath));
            } else if (generatedRoots == null) {
                throw new ArgumentNullException(nameof(generatedRoots));
            } else if (persistenceRegistry == null) {
                throw new ArgumentNullException(nameof(persistenceRegistry));
            }

            string sceneRelativePathToSave = string.IsNullOrWhiteSpace(sceneAssetRelativePath)
                ? sceneId
                : sceneAssetRelativePath;
            NormalizeGeneratedMenuRootInitialPanels(generatedRoots);
            MarkGeneratedRootsAsSceneOwned(generatedRoots);
            EditorEntitySceneOwnershipSnapshot[] hiddenRootSnapshots = HideNonTargetSceneRoots(generatedRoots);

            try {
                string stableIdentity = string.IsNullOrWhiteSpace(authoringAssetId)
                    ? global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(sceneRelativePathToSave)
                    : authoringAssetId;
                using SceneSaveService sceneSaveService = new SceneSaveService(AuthoringSession, persistenceRegistry);
                sceneSaveService.Save(
                    Path.Combine(ProjectRootPath, "assets", sceneRelativePathToSave.Replace('/', Path.DirectorySeparatorChar)),
                    sceneSettings ?? new SceneSettingsAsset(),
                    generatedRoots,
                    stableIdentity,
                    Transaction);
            } finally {
                RestoreHiddenUserSceneRoots(hiddenRootSnapshots);
            }
        }

        /// <summary>
        /// Excludes the common root set from the Nintendo dual-screen group so only the handheld augmentation remains after platform pruning.
        /// </summary>
        /// <param name="roots">Common scene roots that should not survive on Nintendo dual-screen builds.</param>
        void ExcludeRootsFromNintendoHandheldPlatforms(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                if (roots[index] == null) {
                    continue;
                }

                EditorEntity editorRootEntity = roots[index] as EditorEntity;
                if (editorRootEntity == null) {
                    throw new InvalidOperationException("Generated scene roots must be editor entities before platform-exclusive authoring can be applied.");
                }

                PlatformSceneAuthoringHelperServiceValue.ExcludeEntitySubtreeFromScope(editorRootEntity, DemoDiscOverrideScopes.DualScreen);
            }
        }

        /// <summary>
        /// Moves every canonical scene root onto the group-first level order. Generated scenes author their rules
        /// against the platform groups, and a group scope only matches a build target once the entity's level order
        /// starts with the Group level, so this runs for every written scene, with or without a handheld augmentation.
        /// </summary>
        /// <param name="roots">Canonical scene roots being written.</param>
        void AdoptGroupFirstLevelOrderForRoots(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                if (roots[index] == null) {
                    continue;
                }

                EditorEntity editorRootEntity = roots[index] as EditorEntity;
                if (editorRootEntity == null) {
                    throw new InvalidOperationException("Generated scene roots must be editor entities before the group-first level order can be applied.");
                }

                AdoptGroupFirstLevelOrder(editorRootEntity);
            }
        }

        /// <summary>
        /// Restricts the Nintendo handheld augmentation roots so they only survive beneath the Nintendo dual-screen group.
        /// Every platform outside that group, current or future, resolves the Common value and drops the augmentation.
        /// </summary>
        /// <param name="roots">Nintendo handheld augmentation roots.</param>
        void RestrictRootsToNintendoHandheldPlatforms(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                if (roots[index] == null) {
                    continue;
                }

                EditorEntity editorRootEntity = roots[index] as EditorEntity;
                if (editorRootEntity == null) {
                    throw new InvalidOperationException("Nintendo handheld augmentation roots must be editor entities before platform-exclusive authoring can be applied.");
                }

                AdoptGroupFirstLevelOrder(editorRootEntity);
                PlatformSceneAuthoringHelperServiceValue.RestrictEntitySubtreeToScope(editorRootEntity, DemoDiscOverrideScopes.DualScreen);
            }
        }

        /// <summary>
        /// Moves one generated subtree onto the group-first level order, re-pathing the per-platform scopes the
        /// scene factories authored under the default order so none of them stops matching its build target.
        /// </summary>
        /// <param name="rootEntity">Generated root whose subtree should adopt the group-first level order.</param>
        void AdoptGroupFirstLevelOrder(EditorEntity rootEntity) {
            if (rootEntity == null) {
                throw new ArgumentNullException(nameof(rootEntity));
            }

            GroupFirstScopeRewriteServiceValue.RewriteSubtree(rootEntity);
            SetGroupFirstLevelOrderWhereUnset(rootEntity);
        }

        /// <summary>
        /// Gives every entity in the subtree the group-first level order unless a factory already recorded one, so
        /// debug-only entities authored build-config first keep their order and their <c>release</c> scope.
        /// </summary>
        /// <param name="entity">Subtree root to visit.</param>
        void SetGroupFirstLevelOrderWhereUnset(EditorEntity entity) {
            EntitySaveComponent saveComponent = null;
            if (entity.Components != null) {
                for (int index = 0; index < entity.Components.Count; index++) {
                    if (entity.Components[index] is EntitySaveComponent existingSaveComponent) {
                        saveComponent = existingSaveComponent;
                        break;
                    }
                }
            }

            if (saveComponent == null) {
                saveComponent = new EntitySaveComponent();
                entity.AddComponent(saveComponent);
            }

            if (saveComponent.OverrideLevelOrder == null) {
                saveComponent.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            }

            if (entity.Children == null) {
                return;
            }

            for (int index = 0; index < entity.Children.Count; index++) {
                if (entity.Children[index] is EditorEntity childEntity) {
                    SetGroupFirstLevelOrderWhereUnset(childEntity);
                }
            }
        }

        /// <summary>
        /// Finds the largest authored entity id across one root set.
        /// </summary>
        /// <param name="roots">Roots to scan.</param>
        /// <returns>Largest authored entity id, or zero when none carry one.</returns>
        static uint FindMaximumEntityId(Entity[] roots) {
            uint maximumId = 0u;
            for (int index = 0; index < roots.Length; index++) {
                maximumId = Math.Max(maximumId, FindMaximumEntityId(roots[index]));
            }

            return maximumId;
        }

        /// <summary>
        /// Finds the largest authored entity id in one hierarchy.
        /// </summary>
        /// <param name="entity">Hierarchy root to scan.</param>
        /// <returns>Largest authored entity id in the subtree.</returns>
        static uint FindMaximumEntityId(Entity entity) {
            if (entity == null) {
                return 0u;
            }

            uint maximumId = TryFindEntitySaveComponent(entity, out EntitySaveComponent saveComponent) ? saveComponent.EntityId : 0u;
            if (entity.Children == null) {
                return maximumId;
            }

            for (int index = 0; index < entity.Children.Count; index++) {
                maximumId = Math.Max(maximumId, FindMaximumEntityId(entity.Children[index]));
            }

            return maximumId;
        }

        /// <summary>
        /// Renumbers one root set so every entity sits above the supplied id. The dual-screen presentation
        /// holds no entity references, so renumbering it cannot invalidate an authored binding.
        /// </summary>
        /// <param name="roots">Roots to renumber.</param>
        /// <param name="minimumExclusiveId">Id every renumbered entity must exceed.</param>
        static void AssignEntityIdsAbove(Entity[] roots, uint minimumExclusiveId) {
            uint nextId = minimumExclusiveId + 1u;
            for (int index = 0; index < roots.Length; index++) {
                AssignEntityIdsAbove(roots[index], ref nextId);
            }
        }

        /// <summary>
        /// Renumbers one hierarchy from the supplied running id.
        /// </summary>
        /// <param name="entity">Hierarchy root to renumber.</param>
        /// <param name="nextId">Running id, advanced for every visited entity.</param>
        static void AssignEntityIdsAbove(Entity entity, ref uint nextId) {
            if (entity == null) {
                return;
            }

            if (TryFindEntitySaveComponent(entity, out EntitySaveComponent saveComponent)) {
                saveComponent.EntityId = nextId;
                nextId++;
            }

            if (entity.Children == null) {
                return;
            }

            for (int index = 0; index < entity.Children.Count; index++) {
                AssignEntityIdsAbove(entity.Children[index], ref nextId);
            }
        }

        /// <summary>
        /// Attempts to resolve the hidden save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be resolved.</param>
        /// <param name="saveComponent">Resolved save component when one is attached.</param>
        /// <returns>True when the entity carries a save component.</returns>
        static bool TryFindEntitySaveComponent(Entity entity, out EntitySaveComponent saveComponent) {
            saveComponent = null;
            if (entity == null || entity.Components == null) {
                return false;
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent match) {
                    saveComponent = match;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Combines two root arrays into one deterministic root set while preserving the original order inside each source array.
        /// </summary>
        /// <param name="commonRoots">Common scene roots written for every non-handheld platform.</param>
        /// <param name="nintendoHandheldRoots">Nintendo handheld-only roots appended to the canonical scene.</param>
        /// <returns>Combined root array written to the canonical scene asset.</returns>
        static Entity[] CombineRootSets(Entity[] commonRoots, Entity[] nintendoHandheldRoots) {
            if (commonRoots == null) {
                throw new ArgumentNullException(nameof(commonRoots));
            } else if (nintendoHandheldRoots == null) {
                throw new ArgumentNullException(nameof(nintendoHandheldRoots));
            }

            Entity[] combinedRoots = new Entity[commonRoots.Length + nintendoHandheldRoots.Length];
            Array.Copy(commonRoots, 0, combinedRoots, 0, commonRoots.Length);
            Array.Copy(nintendoHandheldRoots, 0, combinedRoots, commonRoots.Length, nintendoHandheldRoots.Length);
            return combinedRoots;
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
        /// Marks the generated root subtrees as authored scene content before they enter the editor serializer.
        /// </summary>
        /// <param name="generatedRoots">Generated roots that should participate in scene serialization.</param>
        void MarkGeneratedRootsAsSceneOwned(Entity[] generatedRoots) {
            if (generatedRoots == null) {
                throw new ArgumentNullException(nameof(generatedRoots));
            }

            for (int index = 0; index < generatedRoots.Length; index++) {
                if (generatedRoots[index] is not EditorEntity editorEntity) {
                    throw new InvalidOperationException("Generated scene roots must be EditorEntity instances.");
                }

                MarkSceneSubtreeAsOwned(editorEntity);
            }
        }

        /// <summary>
        /// Marks editor entities within one non-internal generated subtree as authored scene content.
        /// </summary>
        /// <param name="entity">Generated editor entity whose subtree should be marked.</param>
        void MarkSceneSubtreeAsOwned(EditorEntity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }
            if (entity.InternalEntity) {
                return;
            }

            entity.IsSceneOwned = true;
            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (entity.Children[childIndex] is not EditorEntity childEntity) {
                    continue;
                }

                MarkSceneSubtreeAsOwned(childEntity);
            }
        }

        /// <summary>
        /// Temporarily removes every non-target authored scene root from serializer ownership so only generated roots are written.
        /// </summary>
        /// <param name="generatedRoots">Generated roots that should remain visible to the serializer.</param>
        /// <returns>Snapshots used to restore the hidden roots.</returns>
        EditorEntitySceneOwnershipSnapshot[] HideNonTargetSceneRoots(Entity[] generatedRoots) {
            if (generatedRoots == null) {
                throw new ArgumentNullException(nameof(generatedRoots));
            }

            HashSet<EditorEntity> generatedRootSet = new HashSet<EditorEntity>();
            for (int index = 0; index < generatedRoots.Length; index++) {
                if (generatedRoots[index] is EditorEntity editorGeneratedRoot) {
                    generatedRootSet.Add(editorGeneratedRoot);
                }
            }

            List<EditorEntitySceneOwnershipSnapshot> snapshots = new List<EditorEntitySceneOwnershipSnapshot>();
            List<Entity> liveEntities = AuthoringSession.OwningCore.ObjectManager.Entities;
            for (int index = 0; index < liveEntities.Count; index++) {
                if (liveEntities[index] is not EditorEntity editorEntity) {
                    continue;
                } else if (generatedRootSet.Contains(editorEntity)) {
                    continue;
                } else if (editorEntity.Parent != null) {
                    continue;
                } else if (editorEntity.InternalEntity) {
                    continue;
                } else if (!editorEntity.IsSceneOwned) {
                    continue;
                }

                snapshots.Add(new EditorEntitySceneOwnershipSnapshot(editorEntity, editorEntity.IsSceneOwned));
                editorEntity.IsSceneOwned = false;
            }

            return snapshots.ToArray();
        }

        /// <summary>
        /// Restores authored-scene ownership for user scene roots temporarily excluded during generated scene save.
        /// </summary>
        /// <param name="snapshots">Root snapshots captured before the save operation.</param>
        void RestoreHiddenUserSceneRoots(EditorEntitySceneOwnershipSnapshot[] snapshots) {
            if (snapshots == null) {
                return;
            }

            for (int index = 0; index < snapshots.Length; index++) {
                snapshots[index].Entity.IsSceneOwned = snapshots[index].IsSceneOwned;
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

    }
}
