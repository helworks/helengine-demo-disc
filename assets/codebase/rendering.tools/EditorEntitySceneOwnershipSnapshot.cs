using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Stores the authored-scene ownership state captured before generated-scene serialization temporarily excludes an editor root.
    /// </summary>
    public sealed class EditorEntitySceneOwnershipSnapshot {
        /// <summary>
        /// Initializes one authored-scene ownership snapshot.
        /// </summary>
        /// <param name="entity">Entity whose authored-scene ownership was captured.</param>
        /// <param name="isSceneOwned">Captured authored-scene ownership state.</param>
        public EditorEntitySceneOwnershipSnapshot(EditorEntity entity, bool isSceneOwned) {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            IsSceneOwned = isSceneOwned;
        }

        /// <summary>
        /// Gets the entity whose authored-scene ownership should be restored.
        /// </summary>
        public EditorEntity Entity { get; }

        /// <summary>
        /// Gets the captured authored-scene ownership state.
        /// </summary>
        public bool IsSceneOwned { get; }
    }
}
