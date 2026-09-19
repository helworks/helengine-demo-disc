using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Canonical override scope paths and level order that the demo-disc scene generators author against.
    /// A rule such as "only the Nintendo dual-screen rigs" needs the Group level ahead of Platform, which is what
    /// <see cref="GroupFirstLevelOrder"/> records. A scope only applies to a build target when it is a prefix of
    /// that target's path, so a subtree that adopts the group-first order must have its existing one-step platform
    /// scopes re-pathed through the platform's group chain; <see cref="GeneratedSceneGroupFirstScopeRewriteService"/>
    /// does that at save time so individual generators keep authoring plain platform scopes.
    /// </summary>
    public static class DemoDiscOverrideScopes {
        /// <summary>
        /// Group id of the handheld family in <c>settings/platform-groups.json</c>.
        /// </summary>
        public const string HandheldsGroupId = "handhelds";

        /// <summary>
        /// Group id of the Nintendo dual-screen family nested beneath <see cref="HandheldsGroupId"/>.
        /// </summary>
        public const string NintendoDualScreenGroupId = "nintendo-dual-screen";

        /// <summary>
        /// Level order for entities that vary by group before platform.
        /// </summary>
        public static readonly IReadOnlyList<SceneOverrideScopeStepKind> GroupFirstLevelOrder = new[] {
            SceneOverrideScopeStepKind.Group,
            SceneOverrideScopeStepKind.Platform,
            SceneOverrideScopeStepKind.BuildConfig
        };

        /// <summary>
        /// Builds a fresh mutable copy of <see cref="GroupFirstLevelOrder"/> for one serialized scene entity.
        /// </summary>
        /// <returns>Group-first level order as a detached array.</returns>
        public static SceneOverrideScopeStepKind[] CreateGroupFirstLevelOrder() {
            SceneOverrideScopeStepKind[] order = new SceneOverrideScopeStepKind[GroupFirstLevelOrder.Count];
            for (int index = 0; index < GroupFirstLevelOrder.Count; index++) {
                order[index] = GroupFirstLevelOrder[index];
            }

            return order;
        }

        /// <summary>
        /// Gets <c>handhelds/nintendo-dual-screen</c> — the DS and 3DS rigs.
        /// </summary>
        public static EditorOverrideScope NintendoDualScreen => EditorOverrideScope.Common
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, HandheldsGroupId))
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, NintendoDualScreenGroupId));

        /// <summary>
        /// Builds one resolver over the project's platform group tree. Callers build one per generation pass and
        /// thread it through, so a generator never re-reads the settings files once per authored override.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root whose settings should be read.</param>
        /// <returns>Resolver over the project's platform group tree.</returns>
        public static EditorOverrideScopeResolver CreateResolver(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            return EditorOverrideScopeResolver.Load(Path.GetFullPath(projectRootPath));
        }

        /// <summary>
        /// Builds the one-step path for a single platform beneath the default level order.
        /// </summary>
        /// <param name="platformId">Platform identifier the override should apply to.</param>
        /// <returns>One-step platform scope path.</returns>
        public static EditorOverrideScope Platform(string platformId) {
            if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            return EditorOverrideScope.ForPlatform(platformId);
        }
    }
}
