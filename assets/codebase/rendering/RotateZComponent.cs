namespace city.rendering {
    /// <summary>
    /// Applies a continuous authored local Z-axis rotation to the owning entity so 2D sprites can spin through the normal update path.
    /// </summary>
    public sealed class RotateZComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the local radians advanced every frame update.
        /// </summary>
        public float RadiansPerFrame { get; set; } = 0.01f;

        /// <summary>
        /// Rotates the owning entity around its local Z axis and normalizes the result to avoid quaternion drift.
        /// </summary>
        public override void Update() {
            base.Update();

            float4 deltaRotation;
            float3 axis = new float3(0f, 0f, 1f);
            float4.CreateFromAxisAngle(ref axis, RadiansPerFrame, out deltaRotation);

            float4 orientation = Parent.LocalOrientation;
            float4.Concatenate(ref orientation, ref deltaRotation, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
