using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial rotating platform component spins at the requested constant speed tuning.
    /// </summary>
    public sealed class TiltTrialRotatingPlatformComponentTests {
        [Fact]
        public void Rotating_platform_defaults_to_a_slow_spin() {
            city.game.TiltTrialRotatingPlatformComponent component = new city.game.TiltTrialRotatingPlatformComponent();

            AssertApproximatelyEqual(45f, component.RotationSpeedDegreesPerSecond);
        }

        [Fact]
        public void Rotating_platform_spin_angle_grows_linearly_with_time() {
            float angle = city.game.TiltTrialRotatingPlatformComponent.ResolveSpinAngleRadians(
                elapsedSeconds: 2f,
                rotationSpeedDegreesPerSecond: 45f);

            AssertApproximatelyEqual(MathF.PI * 0.5f, angle);
        }

        [Fact]
        public void Rotating_platform_spin_angle_wraps_after_a_full_turn() {
            float angle = city.game.TiltTrialRotatingPlatformComponent.ResolveSpinAngleRadians(
                elapsedSeconds: 9f,
                rotationSpeedDegreesPerSecond: 45f);

            AssertApproximatelyEqual(MathF.PI * 0.25f, angle);
        }

        [Fact]
        public void Rotating_platform_negative_speed_reverses_the_spin() {
            float angle = city.game.TiltTrialRotatingPlatformComponent.ResolveSpinAngleRadians(
                elapsedSeconds: 2f,
                rotationSpeedDegreesPerSecond: -45f);

            AssertApproximatelyEqual(-MathF.PI * 0.5f, angle);
        }

        [Fact]
        public void Rotating_platform_angular_speed_converts_degrees_to_radians() {
            float angularSpeed = city.game.TiltTrialRotatingPlatformComponent.ResolveSpinAngularSpeedRadians(90f);

            AssertApproximatelyEqual(MathF.PI * 0.5f, angularSpeed);
        }

        static void AssertApproximatelyEqual(float expected, float actual) {
            Assert.True(Math.Abs(expected - actual) <= 0.0001f, $"Expected {expected} but received {actual}.");
        }
    }
}
