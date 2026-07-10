using helengine;

namespace city.game {
    /// <summary>
    /// Marks one Tilt Trial entity as a collectible coin and tracks whether it has already been collected.
    /// </summary>
    public sealed class TiltTrialCollectibleCoinComponent : Component {
        /// <summary>
        /// Gets whether the collectible coin has already been collected during the active scene lifetime.
        /// </summary>
        public bool IsCollected { get; private set; }

        /// <summary>
        /// Marks the owning coin as collected and hides the runtime entity so it no longer renders or triggers.
        /// </summary>
        public void Collect() {
            if (IsCollected) {
                return;
            }

            IsCollected = true;
            if (Parent != null) {
                Parent.Enabled = false;
            }
        }
    }
}
