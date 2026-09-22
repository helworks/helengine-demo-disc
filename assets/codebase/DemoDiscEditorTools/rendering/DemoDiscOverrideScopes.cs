using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// The only override scopes the generators author. Every platform supports every feature, so generated scene
    /// content never names a platform: it varies by platform group (HD, SD, Micro SD, dual-screen) and, for debug-only
    /// content, by build config. A scope applies to a build target when it is a prefix of that target's path under
    /// the entity's level order, so group scopes live on entities with <see cref="GroupFirstLevelOrder"/> and build
    /// config scopes on entities with <see cref="BuildConfigFirstLevelOrder"/>.
    /// </summary>
    public static class DemoDiscOverrideScopes {
        /// <summary>
        /// Group id of the HD devices (desktop and HD consoles) in <c>settings/platform-groups.json</c>.
        /// </summary>
        public const string HdGroupId = "hd";

        /// <summary>
        /// Group id of the SD devices; Micro SD and dual-screen groups nest beneath it.
        /// </summary>
        public const string SdGroupId = "sd";

        /// <summary>
        /// Group id of 240p consoles, nested beneath the SD devices.
        /// </summary>
        public const string MicroSdGroupId = "msd";

        /// <summary>
        /// Group id of the dual-screen handhelds, nested beneath <see cref="SdGroupId"/>.
        /// </summary>
        public const string DualScreenGroupId = "dual-screen";

        /// <summary>
        /// Build config id of debug builds.
        /// </summary>
        public const string DebugBuildConfigId = "debug";

        /// <summary>
        /// Build config id of release builds; debug-only entities are absent at this scope.
        /// </summary>
        public const string ReleaseBuildConfigId = "release";

        /// <summary>
        /// Level order for ordinary generated entities: groups first, then platform, then build config.
        /// </summary>
        public static readonly IReadOnlyList<SceneOverrideScopeStepKind> GroupFirstLevelOrder = new[] {
            SceneOverrideScopeStepKind.Group,
            SceneOverrideScopeStepKind.Platform,
            SceneOverrideScopeStepKind.BuildConfig
        };

        /// <summary>
        /// Level order for debug-only entities: build config first so one <c>release</c> scope covers every
        /// platform, then groups, then platform.
        /// </summary>
        public static readonly IReadOnlyList<SceneOverrideScopeStepKind> BuildConfigFirstLevelOrder = new[] {
            SceneOverrideScopeStepKind.BuildConfig,
            SceneOverrideScopeStepKind.Group,
            SceneOverrideScopeStepKind.Platform
        };

        /// <summary>
        /// Builds a fresh mutable copy of <see cref="GroupFirstLevelOrder"/> for one serialized scene entity.
        /// </summary>
        public static SceneOverrideScopeStepKind[] CreateGroupFirstLevelOrder() {
            return CopyLevelOrder(GroupFirstLevelOrder);
        }

        /// <summary>
        /// Builds a fresh mutable copy of <see cref="BuildConfigFirstLevelOrder"/> for one serialized scene entity.
        /// </summary>
        public static SceneOverrideScopeStepKind[] CreateBuildConfigFirstLevelOrder() {
            return CopyLevelOrder(BuildConfigFirstLevelOrder);
        }

        /// <summary>
        /// Gets <c>hd</c> under the group-first level order.
        /// </summary>
        public static EditorOverrideScope Hd => EditorOverrideScope.Common
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, HdGroupId));

        /// <summary>
        /// Gets <c>sd</c> under the group-first level order.
        /// </summary>
        public static EditorOverrideScope Sd => EditorOverrideScope.Common
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, SdGroupId));

        /// <summary>
        /// Gets sd/msd under the group-first level order for 240p console layout overrides.
        /// </summary>
        public static EditorOverrideScope MicroSd => Sd
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, MicroSdGroupId));

        /// <summary>
        /// Gets <c>sd/dual-screen</c> under the group-first level order: the DS and 3DS rigs.
        /// </summary>
        public static EditorOverrideScope DualScreen => Sd
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, DualScreenGroupId));

        /// <summary>
        /// Gets <c>release</c> under the build-config-first level order: every platform's release build.
        /// </summary>
        public static EditorOverrideScope Release => EditorOverrideScope.Common
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.BuildConfig, ReleaseBuildConfigId));

        /// <summary>
        /// Gets <c>debug/sd/dual-screen</c> under the build-config-first level order.
        /// </summary>
        public static EditorOverrideScope DebugDualScreen => EditorOverrideScope.Common
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.BuildConfig, DebugBuildConfigId))
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, SdGroupId))
            .Append(new EditorOverrideScopeStep(SceneOverrideScopeStepKind.Group, DualScreenGroupId));

        /// <summary>
        /// Builds one resolver over the project's platform group tree. Callers build one per generation pass and
        /// thread it through, so a generator never re-reads the settings files once per authored override.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root whose settings should be read.</param>
        public static EditorOverrideScopeResolver CreateResolver(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            return EditorOverrideScopeResolver.Load(Path.GetFullPath(projectRootPath));
        }

        /// <summary>
        /// Returns true when the platform sits in the dual-screen group of the project's group file.
        /// </summary>
        /// <param name="platformGroupsDocument">Loaded platform group tree.</param>
        /// <param name="platformId">Platform id to classify.</param>
        public static bool IsDualScreenPlatformId(EditorProjectPlatformGroupsDocument platformGroupsDocument, string platformId) {
            if (platformGroupsDocument == null) {
                throw new ArgumentNullException(nameof(platformGroupsDocument));
            }
            if (string.IsNullOrWhiteSpace(platformId)) {
                return false;
            }

            IReadOnlyList<string> groupChain = EditorProjectPlatformGroupsService.FindGroupChain(platformGroupsDocument, platformId);
            for (int index = 0; index < groupChain.Count; index++) {
                if (string.Equals(groupChain[index], DualScreenGroupId, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Builds the one-step path for a single platform. Only the build-config-driven menu exclusions use it;
        /// generated scene content varies by group, never by platform.
        /// </summary>
        /// <param name="platformId">Platform identifier the override should apply to.</param>
        public static EditorOverrideScope Platform(string platformId) {
            if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            return EditorOverrideScope.ForPlatform(platformId);
        }

        /// <summary>
        /// Copies one level order into a detached array for asset fields.
        /// </summary>
        static SceneOverrideScopeStepKind[] CopyLevelOrder(IReadOnlyList<SceneOverrideScopeStepKind> order) {
            SceneOverrideScopeStepKind[] copy = new SceneOverrideScopeStepKind[order.Count];
            for (int index = 0; index < order.Count; index++) {
                copy[index] = order[index];
            }

            return copy;
        }
    }
}
