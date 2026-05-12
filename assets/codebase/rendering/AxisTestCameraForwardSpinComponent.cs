namespace gameplay.rendering {
    /// <summary>
    /// Rotates the parent entity around one supplied camera-forward axis using deterministic absolute runtime time.
    /// </summary>
    public sealed class AxisTestCameraForwardSpinComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the base angle offset in radians applied before time-based rotation.
        /// </summary>
        public float BaseAngleRadians { get; set; }

        /// <summary>
        /// Gets or sets the angular speed in radians per second.
        /// </summary>
        public float AngularSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the X component of the camera-forward rotation axis.
        /// </summary>
        public float CameraForwardAxisX { get; set; }

        /// <summary>
        /// Gets or sets the Y component of the camera-forward rotation axis.
        /// </summary>
        public float CameraForwardAxisY { get; set; }

        /// <summary>
        /// Gets or sets the Z component of the camera-forward rotation axis.
        /// </summary>
        public float CameraForwardAxisZ { get; set; }

        /// <summary>
        /// Evaluates the current orientation from total elapsed runtime time.
        /// </summary>
        public override void Update() {
            base.Update();

            double angleRadians = BaseAngleRadians + (AngularSpeedRadians * Core.Instance.TotalElapsedSeconds);
            float3 axis = float3.Normalize(new float3(CameraForwardAxisX, CameraForwardAxisY, CameraForwardAxisZ));

            float4 orientation;
            float4.CreateFromAxisAngle(ref axis, (float)angleRadians, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
