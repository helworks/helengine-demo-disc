namespace city.rendering {
    /// <summary>
    /// Enables exactly one platform-specific instruction-icon group beneath the owning entity so demo-scene overlays show the best controller family per runtime platform.
    /// </summary>
    public sealed class DemoScenePlatformInstructionIconSetComponent : UpdateComponent {
        /// <summary>
        /// Stable direct-child index used for the Xbox 360 style instruction icon group.
        /// </summary>
        const int Xbox360GroupChildIndex = 0;

        /// <summary>
        /// Stable direct-child index used for the PS2 style instruction icon group.
        /// </summary>
        const int Ps2GroupChildIndex = 1;

        /// <summary>
        /// Stable direct-child index used for the Switch style instruction icon group.
        /// </summary>
        const int SwitchGroupChildIndex = 2;

        /// <summary>
        /// Caches the owning icon-set host entity once the component attaches to a scene hierarchy.
        /// </summary>
        Entity OwnerEntity;

        /// <summary>
        /// Tracks whether the platform-specific child-group enablement has already been applied successfully.
        /// </summary>
        bool IsConfigured;

        /// <summary>
        /// Captures the owning entity and attempts to apply the runtime platform icon selection immediately.
        /// </summary>
        /// <param name="entity">Owning entity that hosts the platform icon groups.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            OwnerEntity = entity;
            IsConfigured = false;
            TryApplyPlatformSelection();
        }

        /// <summary>
        /// Retries platform-group enablement until the overlay hierarchy is fully available.
        /// </summary>
        public override void Update() {
            if (IsConfigured) {
                return;
            }

            TryApplyPlatformSelection();
        }

        /// <summary>
        /// Clears cached runtime state when the component detaches from the scene hierarchy.
        /// </summary>
        /// <param name="entity">Owning entity that is removing this component.</param>
        public override void ComponentRemoved(Entity entity) {
            OwnerEntity = null;
            IsConfigured = false;
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Resolves the best platform icon family for the current runtime and applies it to the direct child groups.
        /// </summary>
        void TryApplyPlatformSelection() {
            if (OwnerEntity == null || OwnerEntity.Children == null || OwnerEntity.Children.Count == 0) {
                return;
            } else if (Core.Instance == null || Core.Instance.PlatformInfo == null || string.IsNullOrWhiteSpace(Core.Instance.PlatformInfo.Name)) {
                return;
            }

            int selectedGroupIndex = ResolveSelectedGroupIndex(Core.Instance.PlatformInfo.Name);
            for (int childIndex = 0; childIndex < OwnerEntity.Children.Count; childIndex++) {
                Entity childEntity = OwnerEntity.Children[childIndex];
                if (childEntity == null) {
                    continue;
                }

                childEntity.Enabled = childIndex == selectedGroupIndex;
            }

            IsConfigured = true;
        }

        /// <summary>
        /// Maps one runtime platform name to the instruction icon family that best matches the platform's controller conventions.
        /// </summary>
        /// <param name="platformName">Runtime platform name exposed by the packaged player.</param>
        /// <returns>Stable direct-child index that should remain enabled.</returns>
        int ResolveSelectedGroupIndex(string platformName) {
            if (string.IsNullOrWhiteSpace(platformName)) {
                throw new ArgumentException("Platform name must be provided.", nameof(platformName));
            }

            string normalizedPlatformName = platformName.Trim().ToLowerInvariant();
            if (normalizedPlatformName.Contains("3ds", StringComparison.Ordinal) || normalizedPlatformName == "ds") {
                return SwitchGroupChildIndex;
            } else if (normalizedPlatformName.Contains("ps2", StringComparison.Ordinal) || normalizedPlatformName.Contains("psp", StringComparison.Ordinal)) {
                return Ps2GroupChildIndex;
            } else if (normalizedPlatformName.Contains("windows", StringComparison.Ordinal)
                || normalizedPlatformName.Contains("win32", StringComparison.Ordinal)
                || normalizedPlatformName.Contains("gamecube", StringComparison.Ordinal)) {
                return Xbox360GroupChildIndex;
            }

            return Xbox360GroupChildIndex;
        }
    }
}
