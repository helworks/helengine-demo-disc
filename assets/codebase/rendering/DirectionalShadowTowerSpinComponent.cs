namespace gameplay.rendering {
    /// <summary>
    /// Rotates one plaza tower group around the local Y axis using deterministic absolute time.
    /// </summary>
    public sealed class DirectionalShadowTowerSpinComponent : UpdateComponent {
        /// <summary>
        /// Stores the accumulated local elapsed time used to evaluate the tower spin phase.
        /// </summary>
        double ElapsedSeconds;

        /// <summary>
        /// Gets or sets the base yaw offset in radians applied before time-based rotation.
        /// </summary>
        public float BaseYawRadians { get; set; }

        /// <summary>
        /// Gets or sets the angular speed in radians per second.
        /// </summary>
        public float AngularSpeedRadians { get; set; }

        /// <summary>
        /// Resets the local spin timer when the component joins a scene entity.
        /// </summary>
        /// <param name="entity">Owning entity receiving the component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            ElapsedSeconds = 0d;
        }

        /// <summary>
        /// Evaluates the current orientation from accumulated local runtime time.
        /// </summary>
        public override void Update() {
            base.Update();

            ElapsedSeconds += Core.Instance.FrameDeltaSeconds;
            double yawRadians = BaseYawRadians + (AngularSpeedRadians * ElapsedSeconds);
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)yawRadians, 0f, 0f, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
