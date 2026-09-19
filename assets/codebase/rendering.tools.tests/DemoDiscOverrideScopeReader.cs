using helengine.editor;

namespace city.testing {
    /// <summary>
    /// Reads authored override scopes out of generated assets the way the packager does, so tests assert the
    /// effective per-platform outcome instead of the shape of one authored scope path.
    /// </summary>
    public static class DemoDiscOverrideScopeReader {
        /// <summary>
        /// Resolver over the demo-disc project's platform group tree.
        /// </summary>
        static readonly EditorOverrideScopeResolver OverrideScopeResolver = EditorOverrideScopeResolver.Load(DemoDiscTestProject.RootPath);

        /// <summary>
        /// Returns the platform id named by one scope, or an empty string when the scope names no platform.
        /// </summary>
        /// <param name="scope">Authored override scope path.</param>
        /// <returns>Platform id on the path, or an empty string.</returns>
        public static string PlatformIdOf(EditorOverrideScope scope) {
            return scope.TryGetStepId(SceneOverrideScopeStepKind.Platform, out string platformId) ? platformId : string.Empty;
        }

        /// <summary>
        /// Returns the platform id named by one serialized scope, or an empty string when the scope names no platform.
        /// </summary>
        /// <param name="scope">Serialized override scope steps.</param>
        /// <returns>Platform id on the path, or an empty string.</returns>
        public static string PlatformIdOf(SceneOverrideScopeStepAsset[] scope) {
            return PlatformIdOf(EditorOverrideScope.FromSteps(scope));
        }

        /// <summary>
        /// Resolves whether one serialized entity survives a build for the supplied platform, honouring the level
        /// order the entity records and the deepest authored prefix of that platform's target path.
        /// </summary>
        /// <param name="entity">Serialized scene entity under evaluation.</param>
        /// <param name="platformId">Platform whose build is being resolved.</param>
        /// <returns>True when the entity exists on the supplied platform.</returns>
        public static bool ExistsOnPlatform(SceneEntityAsset entity, string platformId) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            IReadOnlyList<SceneOverrideScopeStepKind> levelOrder = OverrideScopeResolver.ResolveLevelOrder(
                entity.HasOverrideLevelOrder ? entity.OverrideLevelOrder : null);
            EditorOverrideScope targetScope = OverrideScopeResolver.BuildTargetPath(levelOrder, platformId, string.Empty);
            SceneEntityPlatformExistenceOverrideAsset[] overrides = entity.PlatformExistenceOverrides ?? Array.Empty<SceneEntityPlatformExistenceOverrideAsset>();
            return !EditorOverrideScopeResolver.TrySelectDeepest(
                overrides,
                overrideAsset => EditorOverrideScope.FromSteps(overrideAsset.Scope),
                targetScope,
                out SceneEntityPlatformExistenceOverrideAsset selectedOverride) || selectedOverride.Exists;
        }
    }
}
