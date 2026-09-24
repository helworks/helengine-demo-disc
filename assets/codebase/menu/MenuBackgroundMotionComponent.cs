namespace DemoDisc.menu {
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
        /// Initializes one background motion component with every runtime-resolved reference in a known state, because native builds do not zero-initialize C# instance fields automatically.
        /// </summary>
        public MenuBackgroundMotionComponent() {
            GridEntity = null;
            ScanlineEntity = null;
        }

        /// <summary>
        /// Binds the authored background layers before the first frame update.
        /// </summary>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            GridEntity = GridEntityReference?.ResolvedEntity
                ?? throw new InvalidOperationException("Menu background motion requires a resolved grid entity reference.");
            ScanlineEntity = ScanlineEntityReference?.ResolvedEntity
                ?? throw new InvalidOperationException("Menu background motion requires a resolved scanline entity reference.");
        }

        /// <summary>
        /// Advances both decorative background layers after their serialized scene references resolve.
        /// </summary>
        public override void Update() {
            base.Update();

            if (GridEntity == null || ScanlineEntity == null) {
                return;
            }

            double frameSeconds = Core.Instance.FrameDeltaSeconds;
            MoveGrid((float)((double)GridPixelsPerSecond * frameSeconds));
            MoveScanlines((float)((double)ScanlinePixelsPerSecond * frameSeconds));
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

    }
}
