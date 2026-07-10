using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies the reusable Split Play idle motion component keeps collectible pickups on the requested slow looping bob and spin tuning.
    /// </summary>
    public sealed class SplitPlayIdleMotionComponentTests {
        [Fact]
        public void Split_play_idle_motion_defaults_to_slow_bob_and_spin() {
            city.game.SplitPlayIdleMotionComponent component = new city.game.SplitPlayIdleMotionComponent();

            AssertApproximatelyEqual(0.15f, component.VerticalAmplitude);
            AssertApproximatelyEqual(1.4f, component.VerticalBobAngularSpeedRadians);
            AssertApproximatelyEqual(0.9f, component.RotationAngularSpeedRadians);
        }

        [Fact]
        public void Split_play_idle_motion_vertical_offset_follows_sine_wave() {
            float offset = city.game.SplitPlayIdleMotionComponent.ResolveVerticalOffset(
                elapsedSeconds: MathF.PI * 0.5f,
                verticalAmplitude: 0.15f,
                verticalBobAngularSpeedRadians: 1f,
                bobPhaseRadians: 0f);

            AssertApproximatelyEqual(0.15f, offset);
        }

        static void AssertApproximatelyEqual(float expected, float actual) {
            Assert.True(Math.Abs(expected - actual) <= 0.0001f, $"Expected {expected} but received {actual}.");
        }
    }
}
