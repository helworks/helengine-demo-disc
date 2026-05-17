namespace gameplay.rendering {
    /// <summary>
    /// Moves the parent entity around one authored world-space orbit center using deterministic absolute time.
    /// </summary>
    public sealed class DirectionalShadowOrbitComponent : UpdateComponent {
        /// <summary>
        /// Stores the accumulated local elapsed time used to evaluate the orbit phase.
        /// </summary>
        double ElapsedSeconds;

        /// <summary>
        /// Gets or sets the world-space orbit center.
        /// </summary>
        public float3 OrbitCenter { get; set; }

        /// <summary>
        /// Gets or sets the orbit radius in world units.
        /// </summary>
        public float OrbitRadius { get; set; }

        /// <summary>
        /// Gets or sets the vertical offset applied relative to the orbit center.
        /// </summary>
        public float OrbitHeight { get; set; }

        /// <summary>
        /// Gets or sets the base orbit angle in radians.
        /// </summary>
        public float BaseAngleRadians { get; set; }

        /// <summary>
        /// Gets or sets the angular speed in radians per second.
        /// </summary>
        public float AngularSpeedRadians { get; set; }

        /// <summary>
        /// Resets the local orbit timer when the component joins a scene entity.
        /// </summary>
        /// <param name="entity">Owning entity receiving the component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            ElapsedSeconds = 0d;
        }

        /// <summary>
        /// Evaluates the current orbit position and facing from accumulated local runtime time.
        /// </summary>
        public override void Update() {
            base.Update();

            ElapsedSeconds += Core.Instance.FrameDeltaSeconds;
            double angleRadians = BaseAngleRadians + (AngularSpeedRadians * ElapsedSeconds);
            double x = OrbitCenter.X + (Math.Sin(angleRadians) * OrbitRadius);
            double z = OrbitCenter.Z + (Math.Cos(angleRadians) * OrbitRadius);
            Parent.LocalPosition = new float3((float)x, OrbitCenter.Y + OrbitHeight, (float)z);

            double inwardYawRadians = Math.Atan2(OrbitCenter.X - x, -(OrbitCenter.Z - z));
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)inwardYawRadians, 0f, 0f, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
