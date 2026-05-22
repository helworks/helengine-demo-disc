using helengine.editor;

namespace city.physics.tools {
    /// <summary>
    /// Stores one temporary editor entity layer-mask value while generated physics scenes are saved in isolation.
    /// </summary>
    public sealed class EditorEntityLayerMaskSnapshot {
        /// <summary>
        /// Initializes one snapshot for a hidden editor entity root.
        /// </summary>
        /// <param name="entity">Editor entity whose layer mask was changed.</param>
        /// <param name="layerMask">Layer mask value to restore after saving.</param>
        public EditorEntityLayerMaskSnapshot(EditorEntity entity, ushort layerMask) {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            LayerMask = layerMask;
        }

        /// <summary>
        /// Gets the editor entity whose layer mask was changed.
        /// </summary>
        public EditorEntity Entity { get; }

        /// <summary>
        /// Gets the layer mask value to restore after saving.
        /// </summary>
        public ushort LayerMask { get; }
    }
}
