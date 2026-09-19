using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Re-paths the override scopes of one generated entity subtree from the project's default level order onto
    /// <see cref="DemoDiscOverrideScopes.GroupFirstLevelOrder"/>.
    /// <para>
    /// An override only applies to a build target when its scope is a prefix of that target's path. Once a subtree
    /// records the group-first order, a target's path starts with the platform's group chain, so the one-step
    /// <c>platform:ps2</c> scopes the individual scene factories author would stop matching and their overrides
    /// would silently vanish from every build. Rewriting each such scope to <c>group:…/platform:ps2</c> keeps every
    /// authored per-platform rule intact while the subtree gains a Group level for the Nintendo dual-screen rules.
    /// </para>
    /// </summary>
    public sealed class GeneratedSceneGroupFirstScopeRewriteService {
        /// <summary>
        /// Resolver used to expand one platform id into its group chain.
        /// </summary>
        readonly EditorOverrideScopeResolver OverrideScopeResolver;

        /// <summary>
        /// Initializes one rewrite service over the supplied project's platform group tree.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root whose platform groups should be read.</param>
        public GeneratedSceneGroupFirstScopeRewriteService(string projectRootPath) {
            OverrideScopeResolver = DemoDiscOverrideScopes.CreateResolver(projectRootPath);
        }

        /// <summary>
        /// Re-paths every platform-bearing override scope in one entity subtree onto the group-first level order.
        /// Scopes without a platform step, Common included, are already valid under the new order and are left alone.
        /// </summary>
        /// <param name="rootEntity">Root entity whose subtree should be rewritten.</param>
        public void RewriteSubtree(EditorEntity rootEntity) {
            if (rootEntity == null) {
                throw new ArgumentNullException(nameof(rootEntity));
            }

            RewriteEntity(rootEntity);
            if (rootEntity.Children == null) {
                return;
            }

            for (int index = 0; index < rootEntity.Children.Count; index++) {
                if (rootEntity.Children[index] is EditorEntity childEntity) {
                    RewriteSubtree(childEntity);
                }
            }
        }

        /// <summary>
        /// Re-paths the override scopes stored on one entity's save metadata.
        /// </summary>
        /// <param name="entity">Entity whose authored override scopes should be rewritten.</param>
        void RewriteEntity(EditorEntity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            EntitySaveComponent saveComponent = FindEntitySaveComponent(entity);
            if (saveComponent == null) {
                return;
            }

            RewriteExistenceOverrides(saveComponent);
            RewriteTransformOverrides(saveComponent);
            RewriteComponentExistenceOverrides(saveComponent);
            foreach (EntityComponentSaveState componentSaveState in new List<EntityComponentSaveState>(saveComponent.EnumerateComponentStates())) {
                RewriteComponentPropertyOverrides(componentSaveState);
            }
        }

        /// <summary>
        /// Re-paths the entity existence overrides stored on one save component.
        /// </summary>
        /// <param name="saveComponent">Save component whose existence overrides should be rewritten.</param>
        void RewriteExistenceOverrides(EntitySaveComponent saveComponent) {
            List<SceneEntityPlatformExistenceOverrideAsset> overrides = new List<SceneEntityPlatformExistenceOverrideAsset>(saveComponent.EnumerateExistencePlatformOverrides());
            for (int index = 0; index < overrides.Count; index++) {
                SceneEntityPlatformExistenceOverrideAsset overrideAsset = overrides[index];
                if (overrideAsset == null) {
                    continue;
                }

                EditorOverrideScope currentScope = EditorOverrideScope.FromSteps(overrideAsset.Scope);
                if (!TryBuildGroupFirstScope(currentScope, out EditorOverrideScope rewrittenScope)) {
                    continue;
                }

                saveComponent.RemoveExistencePlatformOverride(currentScope);
                saveComponent.SetExistencePlatformOverride(rewrittenScope, overrideAsset);
            }
        }

        /// <summary>
        /// Re-paths the entity transform overrides stored on one save component.
        /// </summary>
        /// <param name="saveComponent">Save component whose transform overrides should be rewritten.</param>
        void RewriteTransformOverrides(EntitySaveComponent saveComponent) {
            List<SceneEntityPlatformTransformOverrideAsset> overrides = new List<SceneEntityPlatformTransformOverrideAsset>(saveComponent.EnumerateTransformPlatformOverrides());
            for (int index = 0; index < overrides.Count; index++) {
                SceneEntityPlatformTransformOverrideAsset overrideAsset = overrides[index];
                if (overrideAsset == null) {
                    continue;
                }

                EditorOverrideScope currentScope = EditorOverrideScope.FromSteps(overrideAsset.Scope);
                if (!TryBuildGroupFirstScope(currentScope, out EditorOverrideScope rewrittenScope)) {
                    continue;
                }

                saveComponent.RemoveTransformPlatformOverride(currentScope);
                saveComponent.SetTransformPlatformOverride(rewrittenScope, overrideAsset);
            }
        }

        /// <summary>
        /// Re-paths the component add and remove sets stored on one save component.
        /// </summary>
        /// <param name="saveComponent">Save component whose component existence overrides should be rewritten.</param>
        void RewriteComponentExistenceOverrides(EntitySaveComponent saveComponent) {
            List<EntityPlatformComponentOverrideState> overrides = new List<EntityPlatformComponentOverrideState>(saveComponent.EnumerateComponentPlatformOverrides());
            for (int index = 0; index < overrides.Count; index++) {
                EntityPlatformComponentOverrideState overrideState = overrides[index];
                if (overrideState == null) {
                    continue;
                }

                EditorOverrideScope currentScope = overrideState.Scope;
                if (!TryBuildGroupFirstScope(currentScope, out EditorOverrideScope rewrittenScope)) {
                    continue;
                }

                saveComponent.RemoveComponentPlatformOverride(currentScope);
                saveComponent.SetComponentPlatformOverride(rewrittenScope, overrideState);
            }
        }

        /// <summary>
        /// Re-paths the per-scope property override payloads stored for one component.
        /// </summary>
        /// <param name="componentSaveState">Component save state whose override payloads should be rewritten.</param>
        void RewriteComponentPropertyOverrides(EntityComponentSaveState componentSaveState) {
            if (componentSaveState == null) {
                return;
            }

            List<EntityComponentPlatformOverrideState> overrides = new List<EntityComponentPlatformOverrideState>(componentSaveState.EnumeratePlatformOverrides());
            for (int index = 0; index < overrides.Count; index++) {
                EntityComponentPlatformOverrideState overrideState = overrides[index];
                if (overrideState == null) {
                    continue;
                }

                EditorOverrideScope currentScope = overrideState.Scope;
                if (!TryBuildGroupFirstScope(currentScope, out EditorOverrideScope rewrittenScope)) {
                    continue;
                }

                componentSaveState.RemoveScopedPlatformOverride(currentScope);
                componentSaveState.SetScopedPlatformOverride(rewrittenScope, overrideState);
            }
        }

        /// <summary>
        /// Builds the group-first path for one authored scope.
        /// </summary>
        /// <param name="currentScope">Scope as authored under the default level order.</param>
        /// <param name="rewrittenScope">Group-first scope when the scope names a platform and the path changes.</param>
        /// <returns>True when the scope must be replaced; false when it is already valid under the group-first order.</returns>
        bool TryBuildGroupFirstScope(EditorOverrideScope currentScope, out EditorOverrideScope rewrittenScope) {
            rewrittenScope = currentScope;
            if (!currentScope.TryGetStepId(SceneOverrideScopeStepKind.Platform, out string platformId)) {
                return false;
            }

            string environmentId = currentScope.TryGetStepId(SceneOverrideScopeStepKind.BuildConfig, out string buildConfigId)
                ? buildConfigId
                : string.Empty;
            rewrittenScope = OverrideScopeResolver.BuildTargetPath(DemoDiscOverrideScopes.GroupFirstLevelOrder, platformId, environmentId);
            return rewrittenScope != currentScope;
        }

        /// <summary>
        /// Resolves the hidden save component attached to one editor entity.
        /// </summary>
        /// <param name="entity">Entity whose hidden save component should be returned.</param>
        /// <returns>Attached hidden save component, or null when the entity carries none.</returns>
        static EntitySaveComponent FindEntitySaveComponent(EditorEntity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            return null;
        }
    }
}
