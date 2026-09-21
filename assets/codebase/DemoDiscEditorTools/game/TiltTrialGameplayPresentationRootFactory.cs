using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Builds the platform-scoped Tilt Trial presentation roots that every generated gameplay level carries.
    /// The roots are authored as live Blueprint instances while the scene is still being generated, so one
    /// command writes a complete level instead of rewriting a level it wrote moments earlier.
    /// </summary>
    public sealed class TiltTrialGameplayPresentationRootFactory {
        /// <summary>
        /// Resolver used to materialize project-authored Blueprint components during reference discovery.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolverValue;

        /// <summary>
        /// Host-owned capability used to reference the published presentation Blueprints.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Core that owns every presentation root entity created here.
        /// </summary>
        readonly Core OwningCore;

        /// <summary>
        /// Initializes one Tilt Trial presentation root factory.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver for project-authored Blueprint component types.</param>
        /// <param name="assetAuthoringService">Host-owned capability used to reference published Blueprints.</param>
        public TiltTrialGameplayPresentationRootFactory(
            IScriptTypeResolver scriptTypeResolver,
            IEditorProjectAuthoringSession assetAuthoringService) {
            ScriptTypeResolverValue = scriptTypeResolver;
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            if (AssetAuthoringService.OwningCore == null) {
                throw new InvalidOperationException("Tilt Trial presentation roots require an active editor core.");
            }

            OwningCore = AssetAuthoringService.OwningCore;
        }

        /// <summary>
        /// Returns the supplied gameplay roots followed by the console and handheld presentation roots.
        /// </summary>
        /// <param name="sceneRoots">Authored gameplay roots of one Tilt Trial level scene.</param>
        /// <returns>Gameplay roots plus both presentation roots.</returns>
        public Entity[] AppendPresentationRoots(Entity[] sceneRoots) {
            if (sceneRoots == null) {
                throw new ArgumentNullException(nameof(sceneRoots));
            }

            uint playerEntityId = FindRequiredPlayerEntityId(sceneRoots);
            Entity consoleRoot = CreatePresentationRoot(
                "TiltTrialConsolePresentation",
                TiltTrialGameplayPresentationBlueprintGenerator.ConsoleBlueprintRelativePath,
                playerEntityId);
            // Absent on the dual-screen rigs, present everywhere else including every release build.
            FindRequiredEntitySaveComponent(consoleRoot)
                .GetOrCreateExistencePlatformOverride(DemoDiscOverrideScopes.DualScreen).Exists = false;

            Entity handheldRoot = CreatePresentationRoot(
                "TiltTrialHandheldPresentation",
                TiltTrialGameplayPresentationBlueprintGenerator.HandheldBlueprintRelativePath,
                playerEntityId);
            // Absent everywhere by default, present beneath the dual-screen group. Every platform outside the
            // group, current or future, resolves the Common value and never receives the root.
            EntitySaveComponent handheldSaveComponent = FindRequiredEntitySaveComponent(handheldRoot);
            handheldSaveComponent.GetOrCreateExistencePlatformOverride(EditorOverrideScope.Common).Exists = false;
            handheldSaveComponent.GetOrCreateExistencePlatformOverride(DemoDiscOverrideScopes.DualScreen).Exists = true;

            Entity[] roots = new Entity[sceneRoots.Length + 2];
            Array.Copy(sceneRoots, 0, roots, 0, sceneRoots.Length);
            roots[sceneRoots.Length] = consoleRoot;
            roots[sceneRoots.Length + 1] = handheldRoot;
            return roots;
        }

        /// <summary>
        /// Creates one presentation Blueprint instance root bound to the authored player sphere.
        /// </summary>
        /// <param name="name">Stable root entity name.</param>
        /// <param name="blueprintRelativePath">Project-relative presentation Blueprint path.</param>
        /// <param name="playerEntityId">Authored scene entity id of the player sphere.</param>
        /// <returns>Generated presentation root entity.</returns>
        Entity CreatePresentationRoot(string name, string blueprintRelativePath, uint playerEntityId) {
            Entity entity = OwningCore.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;

            BlueprintInstanceComponent blueprintInstance = new BlueprintInstanceComponent {
                BlueprintAssetReference = AssetAuthoringService.CreateFileReference(
                    blueprintRelativePath,
                    AssetEntryKind.Blueprint)
            };
            BlueprintAsset blueprintAsset = AssetAuthoringService.LoadNativeAsset<BlueprintAsset>(blueprintRelativePath);
            BlueprintEntityReferenceOverrideService overrideService = new BlueprintEntityReferenceOverrideService(
                GeneratedScenePersistenceRegistryFactory.Create(ScriptTypeResolverValue));
            overrideService.BindAllEntityReferences(blueprintInstance, blueprintAsset, playerEntityId);
            entity.AddComponent(blueprintInstance);
            return entity;
        }

        /// <summary>
        /// Resolves the authored player sphere entity id the presentation bindings target.
        /// </summary>
        /// <param name="sceneRoots">Authored gameplay roots to search.</param>
        /// <returns>Non-zero authored player sphere entity id.</returns>
        uint FindRequiredPlayerEntityId(Entity[] sceneRoots) {
            for (int index = 0; index < sceneRoots.Length; index++) {
                Entity match = FindEntityByName(sceneRoots[index], "PlayerSphere");
                if (match == null) {
                    continue;
                }

                uint entityId = FindRequiredEntitySaveComponent(match).EntityId;
                if (entityId == 0u) {
                    throw new InvalidOperationException("Tilt Trial presentation bindings require a non-zero PlayerSphere entity id.");
                }

                return entityId;
            }

            throw new InvalidOperationException("Tilt Trial gameplay scenes must contain one PlayerSphere entity.");
        }

        /// <summary>
        /// Finds one live entity by its authored name within one hierarchy.
        /// </summary>
        /// <param name="entity">Current entity to inspect.</param>
        /// <param name="name">Authored entity name to locate.</param>
        /// <returns>Matching entity, or null when the subtree does not contain the requested name.</returns>
        static Entity FindEntityByName(Entity entity, string name) {
            if (entity == null) {
                return null;
            } else if (entity is EditorEntity editorEntity && string.Equals(editorEntity.Name, name, StringComparison.Ordinal)) {
                return entity;
            } else if (entity.Children == null) {
                return null;
            }

            for (int index = 0; index < entity.Children.Count; index++) {
                Entity match = FindEntityByName(entity.Children[index], name);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the hidden save component every editor-authored entity carries.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached save component.</returns>
        static EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated presentation entities must carry one EntitySaveComponent.");
        }
    }
}
