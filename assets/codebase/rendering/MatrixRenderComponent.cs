namespace city.rendering {
    /// <summary>
    /// Drives one render-only showcase cube through a deterministic transform phase loop for matrix-order debugging.
    /// </summary>
    public sealed class MatrixRenderComponent : UpdateComponent {
        /// <summary>
        /// Total number of deterministic animation phases in the showcase loop.
        /// </summary>
        const int PhaseCount = 7;

        /// <summary>
        /// Human-readable operation labels keyed by the fixed phase index.
        /// </summary>
        static readonly string[] OperationLabels = new[] {
            "Translation",
            "Rotation",
            "Scale",
            "Translation + Rotation",
            "Translation + Scale",
            "Rotation + Scale",
            "Translation + Rotation + Scale"
        };

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
        /// Gets the human-readable operation label for the currently active showcase phase.
        /// </summary>
        /// <returns>Current operation label.</returns>
        public string GetCurrentOperationLabel() {
            int phaseIndex = GetCurrentPhaseIndex();
            return OperationLabels[phaseIndex];
        }

        /// <summary>
        /// Advances the parent cube through the fixed move/rotate/scale phase sequence.
        /// </summary>
        public override void Update() {
            base.Update();

            ValidateConfiguration();

            int phaseIndex = GetCurrentPhaseIndex();
            double phaseProgress = GetCurrentPhaseProgress();
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
                throw new InvalidOperationException($"Unsupported matrix render phase index '{phaseIndex}'.");
            }

            orientation.Normalize();
            Parent.LocalPosition = position;
            Parent.LocalScale = scale;
            Parent.LocalOrientation = orientation;
        }

        /// <summary>
        /// Resolves the active phase index from the deterministic loop clock.
        /// </summary>
        /// <returns>Current phase index in the range [0, 6].</returns>
        int GetCurrentPhaseIndex() {
            ValidateConfiguration();

            double totalPhaseSeconds = PhaseDurationSeconds * PhaseCount;
            double wrappedSeconds = GetWrappedSeconds(totalPhaseSeconds);
            return (int)(wrappedSeconds / PhaseDurationSeconds);
        }

        /// <summary>
        /// Resolves the current normalized progress within the active phase.
        /// </summary>
        /// <returns>Phase-local progress in the range [0, 1).</returns>
        double GetCurrentPhaseProgress() {
            ValidateConfiguration();

            double totalPhaseSeconds = PhaseDurationSeconds * PhaseCount;
            double wrappedSeconds = GetWrappedSeconds(totalPhaseSeconds);
            int phaseIndex = (int)(wrappedSeconds / PhaseDurationSeconds);
            return (wrappedSeconds - (phaseIndex * PhaseDurationSeconds)) / PhaseDurationSeconds;
        }

        /// <summary>
        /// Resolves the loop clock wrapped to the total phase span.
        /// </summary>
        /// <param name="totalPhaseSeconds">Total duration of the full phase loop.</param>
        /// <returns>Wrapped loop clock in seconds.</returns>
        static double GetWrappedSeconds(double totalPhaseSeconds) {
            if (Core.Instance == null) {
                throw new InvalidOperationException("MatrixRenderComponent requires an initialized core instance.");
            }

            return Core.Instance.TotalElapsedSeconds % totalPhaseSeconds;
        }

        /// <summary>
        /// Validates the authored runtime configuration before phase evaluation runs.
        /// </summary>
        void ValidateConfiguration() {
            if (Parent == null) {
                throw new InvalidOperationException("MatrixRenderComponent requires an attached parent entity.");
            } else if (PhaseDurationSeconds <= 0d) {
                throw new InvalidOperationException("MatrixRenderComponent requires a positive phase duration.");
            }
        }
    }
}
