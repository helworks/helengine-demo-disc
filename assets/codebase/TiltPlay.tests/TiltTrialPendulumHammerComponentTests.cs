using helengine;

namespace DemoDisc.TiltPlay.tests {
    /// <summary>
    /// Verifies the Tilt Trial pendulum hammer component sweeps its full arc on the requested cosine swing tuning.
    /// </summary>
    public sealed class TiltTrialPendulumHammerComponentTests {
        [Fact]
        public void Pendulum_hammer_defaults_to_full_half_turn_swing() {
            DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent component = new DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent();

            AssertApproximatelyEqual(180f, component.SwingArcDegrees);
            AssertApproximatelyEqual(2.6f, component.SwingPeriodSeconds);
            AssertApproximatelyEqual(0f, component.SwingPhaseRadians);
        }

        [Fact]
        public void Pendulum_hammer_starts_at_positive_arc_extreme() {
            float angle = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                elapsedSeconds: 0f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 2.6f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(MathF.PI * 0.5f, angle);
        }

        [Fact]
        public void Pendulum_hammer_reaches_opposite_extreme_after_half_period() {
            float angle = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                elapsedSeconds: 1.3f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 2.6f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(-MathF.PI * 0.5f, angle);
        }

        [Fact]
        public void Pendulum_hammer_crosses_center_at_quarter_period() {
            float angle = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                elapsedSeconds: 0.65f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 2.6f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(0f, angle);
        }

        [Fact]
        public void Pendulum_hammer_phase_offsets_the_swing() {
            float angle = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                elapsedSeconds: 0f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 2.6f,
                swingPhaseRadians: MathF.PI);

            AssertApproximatelyEqual(-MathF.PI * 0.5f, angle);
        }

        [Fact]
        public void Pendulum_hammer_is_momentarily_still_at_the_arc_extremes() {
            float angularSpeed = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngularSpeedRadians(
                elapsedSeconds: 0f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 2.6f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(0f, angularSpeed);
        }

        [Fact]
        public void Pendulum_hammer_swings_fastest_through_center() {
            float angularSpeed = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngularSpeedRadians(
                elapsedSeconds: 0.65f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 2.6f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(-(MathF.PI * 0.5f) * ((MathF.PI * 2f) / 2.6f), angularSpeed);
        }

        [Fact]
        public void Pendulum_hammer_angular_speed_is_the_swing_angle_derivative() {
            const float sampleSeconds = 0.4f;
            const float derivativeStepSeconds = 0.0005f;
            float angleBefore = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                sampleSeconds - derivativeStepSeconds, 180f, 2.6f, 0.3f);
            float angleAfter = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                sampleSeconds + derivativeStepSeconds, 180f, 2.6f, 0.3f);
            float numericalDerivative = (angleAfter - angleBefore) / (derivativeStepSeconds * 2f);

            float angularSpeed = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngularSpeedRadians(
                sampleSeconds, 180f, 2.6f, 0.3f);

            Assert.True(Math.Abs(numericalDerivative - angularSpeed) <= 0.005f, $"Expected {numericalDerivative} but received {angularSpeed}.");
        }

        [Fact]
        public void Pendulum_hammer_angular_speed_treats_non_positive_period_as_idle() {
            float angularSpeed = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngularSpeedRadians(
                elapsedSeconds: 1f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 0f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(0f, angularSpeed);
        }

        [Fact]
        public void Pendulum_hammer_treats_non_positive_period_as_idle() {
            float angle = DemoDisc.TiltPlay.TiltTrialPendulumHammerComponent.ResolveSwingAngleRadians(
                elapsedSeconds: 1f,
                swingArcDegrees: 180f,
                swingPeriodSeconds: 0f,
                swingPhaseRadians: 0f);

            AssertApproximatelyEqual(0f, angle);
        }

        static void AssertApproximatelyEqual(float expected, float actual) {
            Assert.True(Math.Abs(expected - actual) <= 0.0001f, $"Expected {expected} but received {actual}.");
        }
    }
}
