using helengine;

namespace city.game {
    /// <summary>
    /// Applies the reusable Split Play collectible idle loop: a slow vertical bob paired with a slow continuous spin.
    /// </summary>
    public sealed class SplitPlayIdleMotionComponent : UpdateComponent {
        float3 BaseLocalPosition;
        bool HasCapturedBaseLocalPosition;
        float ElapsedSeconds;

        /// <summary>
        /// Gets or sets the peak vertical displacement applied above and below the authored local position.
        /// </summary>
        public float VerticalAmplitude { get; set; } = 0.15f;

        /// <summary>
        /// Gets or sets the angular speed used by the vertical sine-wave bob in radians per second.
        /// </summary>
        public float VerticalBobAngularSpeedRadians { get; set; } = 1.4f;

        /// <summary>
        /// Gets or sets the vertical bob phase offset in radians.
        /// </summary>
        public float BobPhaseRadians { get; set; }

        /// <summary>
        /// Gets or sets the local Y-axis rotation speed in radians per second.
        /// </summary>
        public float RotationAngularSpeedRadians { get; set; } = 0.9f;

        /// <summary>
        /// Evaluates the current bob offset for one elapsed time sample.
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed loop time in seconds.</param>
        /// <param name="verticalAmplitude">Peak bob amplitude.</param>
        /// <param name="verticalBobAngularSpeedRadians">Bob angular speed in radians per second.</param>
        /// <param name="bobPhaseRadians">Bob phase offset in radians.</param>
        /// <returns>Signed vertical offset from the captured base local position.</returns>
        public static float ResolveVerticalOffset(
            float elapsedSeconds,
            float verticalAmplitude,
            float verticalBobAngularSpeedRadians,
            float bobPhaseRadians) {
            return MathF.Sin(bobPhaseRadians + elapsedSeconds * verticalBobAngularSpeedRadians) * verticalAmplitude;
        }

        /// <summary>
        /// Advances the reusable collectible idle loop.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                return;
            }

            if (!HasCapturedBaseLocalPosition) {
                BaseLocalPosition = Parent.LocalPosition;
                HasCapturedBaseLocalPosition = true;
            }

            double frameDeltaSeconds = Core.Instance.FrameDeltaSeconds;
            if (double.IsNaN(frameDeltaSeconds) || double.IsInfinity(frameDeltaSeconds) || frameDeltaSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(Core.Instance.FrameDeltaSeconds), "Split Play idle motion requires a finite non-negative frame delta.");
            } else if (frameDeltaSeconds == 0d) {
                return;
            }

            float elapsedSeconds = (float)frameDeltaSeconds;
            ElapsedSeconds += elapsedSeconds;

            Parent.LocalPosition = new float3(
                BaseLocalPosition.X,
                BaseLocalPosition.Y + ResolveVerticalOffset(ElapsedSeconds, VerticalAmplitude, VerticalBobAngularSpeedRadians, BobPhaseRadians),
                BaseLocalPosition.Z);

            float3 axis = new float3(0f, 1f, 0f);
            float4.CreateFromAxisAngle(ref axis, RotationAngularSpeedRadians * elapsedSeconds, out float4 deltaRotation);

            float4 orientation = Parent.LocalOrientation;
            float4.Concatenate(ref orientation, ref deltaRotation, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }
    }
}
