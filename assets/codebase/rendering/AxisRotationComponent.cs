namespace gameplay.rendering {
    /// <summary>
    /// Rotates the parent entity around one authored local-space axis using frame-rate-independent delta time.
    /// </summary>
    public sealed class AxisRotationComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the local-space axis used by incremental rotation updates.
        /// </summary>
        public float3 Axis { get; set; }

        /// <summary>
        /// Gets or sets the angular speed in radians per second.
        /// </summary>
        public float AngularSpeedRadiansPerSecond { get; set; }

        /// <summary>
        /// Advances the parent local orientation by one delta-time rotation step around the authored local axis.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Axis == float3.Zero) {
                throw new InvalidOperationException("AxisRotationComponent requires a non-zero axis.");
            }

            float3 normalizedAxis = float3.Normalize(Axis);
            float deltaAngleRadians = AngularSpeedRadiansPerSecond * (float)Core.Instance.FrameDeltaSeconds;
            float4 deltaRotation;
            float4.CreateFromAxisAngle(ref normalizedAxis, deltaAngleRadians, out deltaRotation);

            float4 orientation = Parent.LocalOrientation * deltaRotation;
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
