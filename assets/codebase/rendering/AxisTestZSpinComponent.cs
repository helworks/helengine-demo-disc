namespace gameplay.rendering {
    /// <summary>
    /// Rotates the parent entity around its local Z axis using deterministic absolute runtime time.
    /// </summary>
    public sealed class AxisTestZSpinComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the base roll offset in radians applied before time-based rotation.
        /// </summary>
        public float BaseRollRadians { get; set; }

        /// <summary>
        /// Gets or sets the angular speed in radians per second.
        /// </summary>
        public float AngularSpeedRadians { get; set; }

        /// <summary>
        /// Evaluates the current orientation from total elapsed runtime time.
        /// </summary>
        public override void Update() {
            base.Update();

            double rollRadians = BaseRollRadians + (AngularSpeedRadians * Core.Instance.TotalElapsedSeconds);
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, 0f, (float)rollRadians, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
