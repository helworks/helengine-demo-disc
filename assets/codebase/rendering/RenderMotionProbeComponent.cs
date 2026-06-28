namespace city.rendering {
    /// <summary>
    /// Drives one render-only probe cube through a deterministic transform phase loop for matrix-order debugging.
    /// </summary>
    public sealed class RenderMotionProbeComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the local origin that all probe phases animate around.
        /// </summary>
        public float3 BaseLocalPosition { get; set; }

        /// <summary>
        /// Gets or sets the local offset applied by motion-enabled phases.
        /// </summary>
        public float3 MotionOffset { get; set; }

        /// <summary>
        /// Gets or sets the neutral local scale used by non-scaling phases.
        /// </summary>
        public float3 BaseLocalScale { get; set; }

        /// <summary>
        /// Gets or sets the non-uniform local scale used by scaling phases.
        /// </summary>
        public float3 ScaledLocalScale { get; set; }

        /// <summary>
        /// Gets or sets the quaternion used by rotation-enabled phases.
        /// </summary>
        public float4 RotatedLocalOrientation { get; set; }

        /// <summary>
        /// Gets or sets the duration of each phase in seconds.
        /// </summary>
        public double PhaseDurationSeconds { get; set; }

        /// <summary>
        /// Advances the parent cube through the fixed move/rotate/scale phase sequence.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("RenderMotionProbeComponent requires an attached parent entity.");
            } else if (PhaseDurationSeconds <= 0d) {
                throw new InvalidOperationException("RenderMotionProbeComponent requires a positive phase duration.");
            }

            const int PhaseCount = 7;
            double totalPhaseSeconds = PhaseDurationSeconds * PhaseCount;
            double wrappedSeconds = Core.Instance.TotalElapsedSeconds % totalPhaseSeconds;
            int phaseIndex = (int)(wrappedSeconds / PhaseDurationSeconds);
            double phaseProgress = (wrappedSeconds - (phaseIndex * PhaseDurationSeconds)) / PhaseDurationSeconds;
            float easedProgress = (float)(0.5d - (0.5d * Math.Cos(phaseProgress * Math.PI)));

            float3 position = BaseLocalPosition;
            float3 scale = BaseLocalScale;
            float4 orientation = float4.Identity;

            if (phaseIndex == 0) {
                position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
            } else if (phaseIndex == 1) {
                orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
            } else if (phaseIndex == 2) {
                scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
            } else if (phaseIndex == 3) {
                position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
                orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
            } else if (phaseIndex == 4) {
                position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
                scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
            } else if (phaseIndex == 5) {
                orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
                scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
            } else if (phaseIndex == 6) {
                position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
                orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
                scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
            } else {
                throw new InvalidOperationException($"Unsupported render motion probe phase index '{phaseIndex}'.");
            }

            orientation.Normalize();
            Parent.LocalPosition = position;
            Parent.LocalScale = scale;
            Parent.LocalOrientation = orientation;
        }
    }
}
