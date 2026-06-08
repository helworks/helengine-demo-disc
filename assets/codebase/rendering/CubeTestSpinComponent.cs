namespace city.rendering {
    /// <summary>
    /// Rotates the cube-test entity around its local Y axis using deterministic absolute time.
    /// </summary>
    public sealed class CubeTestSpinComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the base yaw offset applied before time-driven rotation.
        /// </summary>
        public float BaseYawRadians { get; set; }

        /// <summary>
        /// Gets or sets the angular speed in radians per second used to rotate the parent entity.
        /// </summary>
        public float AngularSpeedRadians { get; set; }

        /// <summary>
        /// Updates the parent entity orientation from total elapsed runtime time.
        /// </summary>
        public override void Update() {
            double yawRadians = BaseYawRadians + (AngularSpeedRadians * Core.Instance.TotalElapsedSeconds);
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)yawRadians, 0f, 0f, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
