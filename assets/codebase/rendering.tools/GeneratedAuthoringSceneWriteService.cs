using helengine.editor;
using city.menu;

namespace city.rendering.tools {
    /// <summary>
    /// Persists generated live-authored scenes through the editor scene save pipeline.
    /// </summary>
    public sealed class GeneratedAuthoringSceneWriteService {
        /// <summary>
        /// Stable platform identifiers used by the Nintendo handheld scene augmentation path.
        /// </summary>
        static readonly string[] NintendoHandheldPlatformIds = ["ds", "3ds"];

        /// <summary>
        /// Shared Nintendo DS scaffold builder used to derive companion scenes from generated showcase roots.
        /// </summary>
        readonly NintendoDsRenderingSceneScaffoldFactory NintendoDsRenderingSceneScaffoldFactoryValue;

        /// <summary>
        /// High-level editor helper used to author platform-exclusive entity subtrees without touching low-level save metadata directly.
        /// </summary>
        readonly PlatformSceneAuthoringHelperService PlatformSceneAuthoringHelperServiceValue;

        /// <summary>
        /// In-memory generated scene clone service used to duplicate top-screen roots before the handheld scaffold mutates them.
        /// </summary>
        readonly GeneratedSceneEntityCloneService GeneratedSceneEntityCloneServiceValue;

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
        /// Canonical project root supplied by the owning authoring session.
        /// </summary>
        string ProjectRootPath => Path.GetFullPath(AuthoringSession.ProjectRootPath);

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
            GeneratedSceneEntityCloneServiceValue = new GeneratedSceneEntityCloneService(authoringSession);
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
                AddUniqueRoots(rootsToDispose, sceneDefinition.RootEntities);
                Entity[] rootsToWrite = sceneDefinition.RootEntities;
                if (sceneDefinition.NintendoDsScene != null) {
                    Entity[] nintendoDsSceneRoots = BuildNintendoHandheldSceneRoots(
                        sceneDefinition);
                    AddUniqueRoots(rootsToDispose, nintendoDsSceneRoots);
                    ExcludeRootsFromNintendoHandheldPlatforms(sceneDefinition.RootEntities);
                    RestrictRootsToNintendoHandheldPlatforms(nintendoDsSceneRoots);
                    rootsToWrite = CombineRootSets(sceneDefinition.RootEntities, nintendoDsSceneRoots);
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

            FontAsset bottomOverlayFont = ResolveRequiredBottomOverlayFont();
            Entity[] clonedTopScreenRoots = CloneSceneRoots(sceneDefinition.RootEntities);
            return NintendoDsRenderingSceneScaffoldFactoryValue.CreateSceneRoots(
                clonedTopScreenRoots,
                sceneDefinition.NintendoDsScene.UseDefaultBottomOverlay,
                sceneDefinition.NintendoDsScene.MoveTopScreen2DRootsToBottomScreen,
                sceneDefinition.NintendoDsScene.BottomScreenRootEntities ?? Array.Empty<Entity>(),
                bottomOverlayFont);
        }

        /// <summary>
        /// Loads the dedicated project body font used by the Nintendo DS bottom overlay through the normal authored source-font import pipeline.
        /// </summary>
        /// <returns>Imported project body font asset.</returns>
        FontAsset ResolveRequiredBottomOverlayFont() {
            SceneAssetReference fontReference = DemoDiscSceneComponentRecordFactory.CreateEditorFontReference(AuthoringSession);
            if (fontReference == null || fontReference.SourceKind != SceneAssetReferenceSourceKind.FileSystem || string.IsNullOrWhiteSpace(fontReference.RelativePath)) {
                throw new InvalidOperationException("The demo-disc body font reference must resolve to one file-backed source font path.");
            }

            string fullSourcePath = Path.Combine(ProjectRootPath, "assets", fontReference.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            return AuthoringSession.ResolveFontAsset(fullSourcePath);
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
                    ? global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(sceneRelativePathToSave)
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
        /// Clones one generated scene root set through the editor serialization pipeline so Nintendo handheld scaffolding may mutate copies without rewriting the common authored roots.
        /// </summary>
        /// <param name="sourceRoots">Root entities that should be cloned.</param>
        /// <returns>Detached editor roots cloned in memory.</returns>
        EditorEntity[] CloneSceneRoots(Entity[] sourceRoots) {
            if (sourceRoots == null) {
                throw new ArgumentNullException(nameof(sourceRoots));
            }
            EditorEntity[] clonedRoots = GeneratedSceneEntityCloneServiceValue.CloneRoots(sourceRoots);
            AssignFreshGeneratedEntityIds(clonedRoots);
            return clonedRoots;
        }

        /// <summary>
        /// Excludes the common root set from Nintendo handheld builds so only the handheld augmentation remains after platform pruning.
        /// </summary>
        /// <param name="roots">Common scene roots that should not survive on Nintendo handheld builds.</param>
        void ExcludeRootsFromNintendoHandheldPlatforms(Entity[] roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            for (int index = 0; index < roots.Length; index++) {
                if (roots[index] == null) {
                    continue;
                }

                if (IsConsoleCameraLightInstructionsBlueprintRoot(roots[index])) {
                    continue;
                }

                EditorEntity editorRootEntity = roots[index] as EditorEntity;
                if (editorRootEntity == null) {
                    throw new InvalidOperationException("Generated scene roots must be editor entities before platform-exclusive authoring can be applied.");
                }

                PlatformSceneAuthoringHelperServiceValue.ExcludeEntitySubtreeFromPlatforms(
                    ProjectRootPath,
                    editorRootEntity,
                    NintendoHandheldPlatformIds);

            }
        }

        /// <summary>
        /// Identifies the console-only instruction Blueprint root whose explicit platform rules must survive handheld augmentation.
        /// </summary>
        /// <param name="rootEntity">Generated root being considered for handheld exclusion.</param>
        /// <returns>True when the root is the console camera/light Blueprint instance.</returns>
        static bool IsConsoleCameraLightInstructionsBlueprintRoot(Entity rootEntity) {
            if (rootEntity == null || rootEntity.Components == null) {
                return false;
            }

            for (int index = 0; index < rootEntity.Components.Count; index++) {
                if (rootEntity.Components[index] is BlueprintInstanceComponent blueprintInstance
                    && string.Equals(
                        blueprintInstance.BlueprintAssetReference?.RelativePath,
                        ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath,
                        StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Restricts the Nintendo handheld augmentation roots so they only survive on DS and 3DS builds.
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

                PlatformSceneAuthoringHelperServiceValue.RestrictEntitySubtreeToPlatforms(
                    ProjectRootPath,
                    editorRootEntity,
                    NintendoHandheldPlatformIds);
            }
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
        /// Assigns fresh non-zero scene entity ids across one cloned root set so the handheld augmentation can coexist with the common roots inside one canonical scene asset.
        /// </summary>
        /// <param name="roots">Cloned root entities that should receive fresh ids.</param>
        void AssignFreshGeneratedEntityIds(IReadOnlyList<EditorEntity> roots) {
            if (roots == null) {
                throw new ArgumentNullException(nameof(roots));
            }

            EditorSceneEntityIdAllocator entityIdAllocator = ResolveRequiredSceneEntityIdAllocator();
            for (int index = 0; index < roots.Count; index++) {
                if (roots[index] == null) {
                    continue;
                }

                AssignFreshGeneratedEntityIds(roots[index], entityIdAllocator);
            }
        }

        /// <summary>
        /// Assigns fresh non-zero scene entity ids throughout one cloned editor subtree.
        /// </summary>
        /// <param name="entity">Cloned editor subtree root that should receive fresh ids.</param>
        /// <param name="entityIdAllocator">Allocator that owns numeric scene entity ids for the active editor host.</param>
        void AssignFreshGeneratedEntityIds(EditorEntity entity, EditorSceneEntityIdAllocator entityIdAllocator) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entityIdAllocator == null) {
                throw new ArgumentNullException(nameof(entityIdAllocator));
            }

            FindRequiredEntitySaveComponent(entity).EntityId = entityIdAllocator.Allocate();
            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (entity.Children[childIndex] is EditorEntity childEntity) {
                    AssignFreshGeneratedEntityIds(childEntity, entityIdAllocator);
                }
            }
        }

        /// <summary>
        /// Resolves the active editor-owned scene entity id allocator required for cloned handheld augmentation roots.
        /// </summary>
        /// <returns>Active editor-owned scene entity id allocator.</returns>
        EditorSceneEntityIdAllocator ResolveRequiredSceneEntityIdAllocator() {
            if (AuthoringSession.OwningCore is not EditorCore editorCore) {
                throw new InvalidOperationException("Cloning generated handheld scene roots requires an active EditorCore.");
            } else if (editorCore.SceneEntityIdAllocator == null) {
                throw new InvalidOperationException("Cloning generated handheld scene roots requires EditorCore.SceneEntityIdAllocator.");
            }

            return editorCore.SceneEntityIdAllocator;
        }

        /// <summary>
        /// Resolves the hidden save component attached to one cloned editor entity.
        /// </summary>
        /// <param name="entity">Editor entity whose save component should be returned.</param>
        /// <returns>Attached hidden save component.</returns>
        static EntitySaveComponent FindRequiredEntitySaveComponent(EditorEntity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated handheld scene roots must carry one EntitySaveComponent before they can be cloned.");
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
