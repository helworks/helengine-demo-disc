namespace city.menu {
    /// <summary>
    /// Animates the standard menu's grid and scanline roots with continuous wrapped movement.
    /// </summary>
    public sealed class MenuBackgroundMotionComponent : UpdateComponent {
        /// <summary>
        /// Serialized entity reference identifying the animated grid root.
        /// </summary>
        public SceneEntityReference GridEntityReference { get; set; }

        /// <summary>
        /// Serialized entity reference identifying the animated scanline root.
        /// </summary>
        public SceneEntityReference ScanlineEntityReference { get; set; }

        /// <summary>
        /// Horizontal and vertical wrap period used by the grid in authored pixels.
        /// </summary>
        public float GridPeriod { get; set; }

        /// <summary>
        /// Vertical wrap period used by the scanlines in authored pixels.
        /// </summary>
        public float ScanlinePeriod { get; set; }

        /// <summary>
        /// Constant diagonal grid movement in authored pixels per second.
        /// </summary>
        public float GridPixelsPerSecond { get; set; }

        /// <summary>
        /// Constant vertical scanline movement in authored pixels per second.
        /// </summary>
        public float ScanlinePixelsPerSecond { get; set; }

        /// <summary>
        /// Resolved runtime grid root.
        /// </summary>
        Entity GridEntity;

        /// <summary>
        /// Resolved runtime scanline root.
        /// </summary>
        Entity ScanlineEntity;

        /// <summary>
        /// Advances both decorative background layers after their serialized scene references resolve.
        /// </summary>
        public override void Update() {
            base.Update();
            ResolveEntitiesWhenNeeded();
            if (GridEntity == null || ScanlineEntity == null) {
                return;
            }

            double frameSeconds = Core.Instance.FrameDeltaSeconds;
            MoveGrid((float)((double)GridPixelsPerSecond * frameSeconds));
            MoveScanlines((float)((double)ScanlinePixelsPerSecond * frameSeconds));
        }

        /// <summary>
        /// Resolves both authored layer roots after the scene loader attaches runtime entity identifiers.
        /// </summary>
        void ResolveEntitiesWhenNeeded() {
            if (GridEntity != null && ScanlineEntity != null) {
                return;
            } else if (Core.Instance == null || Core.Instance.ObjectManager == null) {
                throw new InvalidOperationException("Menu background motion requires an initialized object manager.");
            }

            GridEntity = FindRequiredEntity(GridEntityReference, "grid");
            ScanlineEntity = FindRequiredEntity(ScanlineEntityReference, "scanline");
        }

        /// <summary>
        /// Moves the grid diagonally and restarts its tile offset after one full grid cell.
        /// </summary>
        /// <param name="movement">Distance to move during the current frame.</param>
        void MoveGrid(float movement) {
            if (GridPeriod <= 0f) {
                throw new InvalidOperationException("Menu background grid period must be positive.");
            }

            float3 localPosition = GridEntity.LocalPosition;
            float nextX = localPosition.X - movement;
            float nextY = localPosition.Y - movement;
            if (nextX <= -GridPeriod) {
                nextX += GridPeriod;
            }
            if (nextY <= -GridPeriod) {
                nextY += GridPeriod;
            }

            GridEntity.LocalPosition = new float3(nextX, nextY, localPosition.Z);
        }

        /// <summary>
        /// Moves scanlines vertically and restarts their tile offset after one scanline cell.
        /// </summary>
        /// <param name="movement">Distance to move during the current frame.</param>
        void MoveScanlines(float movement) {
            if (ScanlinePeriod <= 0f) {
                throw new InvalidOperationException("Menu background scanline period must be positive.");
            }

            float3 localPosition = ScanlineEntity.LocalPosition;
            float nextY = localPosition.Y - movement;
            if (nextY <= -ScanlinePeriod) {
                nextY += ScanlinePeriod;
            }

            ScanlineEntity.LocalPosition = new float3(localPosition.X, nextY, localPosition.Z);
        }

        /// <summary>
        /// Resolves one serialized scene entity reference to its runtime entity.
        /// </summary>
        /// <param name="entityReference">Serialized reference identifying the required entity.</param>
        /// <param name="description">Human-readable layer description used in failure messages.</param>
        /// <returns>Resolved runtime entity, or <c>null</c> while the scene loader is still creating it.</returns>
        Entity FindRequiredEntity(SceneEntityReference entityReference, string description) {
            if (entityReference == null || entityReference.EntityId == 0u) {
                throw new InvalidOperationException($"Menu background motion requires a serialized {description} entity reference.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidateEntity = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(candidateEntity) == entityReference.EntityId) {
                    return candidateEntity;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the stable serialized scene id attached to one runtime entity.
        /// </summary>
        /// <param name="entity">Runtime entity whose scene id should be inspected.</param>
        /// <returns>Serialized entity id, or zero when the runtime id has not been attached.</returns>
        uint FindSceneEntityRuntimeIdOrZero(Entity entity) {
            if (entity == null || entity.Components == null) {
                return 0u;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is SceneEntityRuntimeIdComponent runtimeIdComponent) {
                    return runtimeIdComponent.SceneEntityId;
                }
            }

            return 0u;
        }
    }
}
