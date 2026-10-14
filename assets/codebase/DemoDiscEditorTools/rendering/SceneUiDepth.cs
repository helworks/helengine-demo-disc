namespace DemoDisc.EditorTools {
    /// <summary>
    /// Authors a UI entity at a world-space depth while retaining its screen-plane position.
    /// </summary>
    public static class SceneUiDepth {
        /// <summary>
        /// Converts the requested world depth into the entity's parent-relative transform.
        /// </summary>
        /// <param name="entity">UI entity whose depth is being authored.</param>
        /// <param name="depth">World Z coordinate; larger values appear in front.</param>
        public static void SetWorldDepth(Entity entity, float depth) {
            ArgumentNullException.ThrowIfNull(entity);
            float3 position = entity.Position;
            position.Z = depth;
            if (entity.Parent == null) {
                entity.LocalPosition = position;
                return;
            }
            float3 scale = entity.Parent.Scale;
            if (scale.X == 0f || scale.Y == 0f || scale.Z == 0f) {
                throw new InvalidOperationException("Cannot author UI depth beneath a zero-scale parent.");
            }
            float3 relative = float4.RotateVector(position - entity.Parent.Position, float4.Inverse(entity.Parent.Orientation));
            entity.LocalPosition = new float3(relative.X / scale.X, relative.Y / scale.Y, relative.Z / scale.Z);
        }
    }
}
