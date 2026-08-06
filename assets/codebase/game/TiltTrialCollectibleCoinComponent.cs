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
        /// Resolves the authored collectible root entity that should be hidden when one nested coin visual is collected.
        /// </summary>
        /// <param name="entity">Coin entity or one authored wrapper entity in its ancestor chain.</param>
        /// <returns>Nearest authored wrapper with a trigger observer when present; otherwise the supplied entity.</returns>
        [NativeBorrowedReturn]
        public static Entity ResolveCollectibleRootEntity(Entity entity) {
            Entity current = entity;
            while (current != null) {
                if (TryFindTriggerObserverComponent(current) != null) {
                    return current;
                }

                current = current.Parent;
            }

            return entity;
        }

        /// <summary>
        /// Marks the owning coin as collected so session logic ignores future trigger events.
        /// </summary>
        public void Collect() {
            if (IsCollected) {
                return;
            }

            IsCollected = true;
            if (Parent != null) {
                Entity collectibleRootEntity = ResolveCollectibleRootEntity(Parent);
                if (collectibleRootEntity != null) {
                    collectibleRootEntity.Enabled = false;
                }
            }
        }

        static global::helengine.SceneEntityTriggerObserverComponent TryFindTriggerObserverComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is global::helengine.SceneEntityTriggerObserverComponent component) {
                    return component;
                }
            }

            return null;
        }
    }
}
