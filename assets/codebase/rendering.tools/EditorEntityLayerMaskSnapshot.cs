using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Stores the authored layer mask captured for one editor entity before generated-scene serialization temporarily hides it.
    /// </summary>
    public sealed class EditorEntityLayerMaskSnapshot {
        /// <summary>
        /// Initializes one layer-mask snapshot.
        /// </summary>
        /// <param name="entity">Entity whose layer mask was captured.</param>
        /// <param name="layerMask">Captured layer mask value.</param>
        public EditorEntityLayerMaskSnapshot(EditorEntity entity, ushort layerMask) {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            LayerMask = layerMask;
        }

        /// <summary>
        /// Gets the entity whose layer mask should be restored.
        /// </summary>
        public EditorEntity Entity { get; }

        /// <summary>
        /// Gets the captured layer mask value.
        /// </summary>
        public ushort LayerMask { get; }
    }
}
