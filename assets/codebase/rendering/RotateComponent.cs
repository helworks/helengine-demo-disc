namespace city.rendering {
    /// <summary>
    /// Applies a continuous authored local Y-axis rotation to the owning city showcase entity.
    /// </summary>
    public sealed class RotateComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the local radians advanced every frame update.
        /// </summary>
        public float RadiansPerFrame { get; set; } = 0.07f;

        /// <summary>
        /// Rotates the owning entity and normalizes the result to avoid drift.
        /// </summary>
        public override void Update() {
            base.Update();

            float4 deltaRotation;
            float3 axis = new float3(0f, 1f, 0f);
            float4.CreateFromAxisAngle(ref axis, RadiansPerFrame, out deltaRotation);

            float4 orientation = Parent.Orientation;
            orientation = deltaRotation * orientation;
            orientation.Normalize();

            Parent.Orientation = orientation;
        }
    }
}
