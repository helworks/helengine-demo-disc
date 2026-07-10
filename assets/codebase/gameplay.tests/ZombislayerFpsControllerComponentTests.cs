namespace city.tests {
    /// <summary>
    /// Verifies the Zombislayer first-person controller exposes deterministic camera clamp and planar-movement helpers.
    /// </summary>
    public sealed class ZombislayerFpsControllerComponentTests {
        /// <summary>
        /// Ensures the pitch clamp caps positive overflow at the configured upper bound.
        /// </summary>
        [Fact]
        public void Clamp_pitch_degrees_caps_positive_overflow() {
            float clamped = city.game.ZombislayerFpsControllerComponent.ClampPitchDegrees(120f);

            Assert.Equal(80f, clamped);
        }

        /// <summary>
        /// Ensures the pitch clamp caps negative overflow at the configured lower bound.
        /// </summary>
        [Fact]
        public void Clamp_pitch_degrees_caps_negative_overflow() {
            float clamped = city.game.ZombislayerFpsControllerComponent.ClampPitchDegrees(-120f);

            Assert.Equal(-80f, clamped);
        }

        /// <summary>
        /// Ensures planar forward movement follows the engine's -Z forward convention at zero yaw.
        /// </summary>
        [Fact]
        public void Build_planar_move_direction_uses_negative_z_as_forward_at_zero_yaw() {
            float3 direction = city.game.ZombislayerFpsControllerComponent.BuildPlanarMoveDirection(0f, 1f, 0f);

            AssertApproximatelyEqual(0f, direction.X);
            AssertApproximatelyEqual(0f, direction.Y);
            AssertApproximatelyEqual(-1f, direction.Z);
        }

        /// <summary>
        /// Ensures combined forward-right input is normalized before being returned.
        /// </summary>
        [Fact]
        public void Build_planar_move_direction_normalizes_combined_input() {
            float3 direction = city.game.ZombislayerFpsControllerComponent.BuildPlanarMoveDirection(0f, 1f, 1f);

            AssertApproximatelyEqual(0.70710677f, direction.X);
            AssertApproximatelyEqual(0f, direction.Y);
            AssertApproximatelyEqual(-0.70710677f, direction.Z);
        }

        /// <summary>
        /// Compares two scalar values with a narrow gameplay-friendly tolerance.
        /// </summary>
        /// <param name="expected">Expected scalar value.</param>
        /// <param name="actual">Actual scalar value.</param>
        static void AssertApproximatelyEqual(float expected, float actual) {
            Assert.True(Math.Abs(expected - actual) <= 0.0001f, $"Expected {expected} but received {actual}.");
        }
    }
}
