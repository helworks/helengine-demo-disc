using System.Reflection;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial planar ball-drive steering behavior exposed by the gameplay controller.
    /// </summary>
    public sealed class DemoTiltStageComponentTests {
        /// <summary>
        /// Ensures the Tilt Trial planar steering preserves vertical velocity, normalizes diagonal movement, respects the configured acceleration limit, and brakes back toward rest with no input.
        /// </summary>
        [Fact]
        public void Tilt_trial_planar_drive_preserves_vertical_velocity_normalizes_diagonal_input_limits_acceleration_and_brakes_to_rest() {
            MethodInfo steeringMethod = typeof(city.game.DemoTiltStageComponent).GetMethod(
                "ResolveDrivenLinearVelocity",
                BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(steeringMethod);

            float3 drivenVelocity = InvokeDrivenVelocity(
                steeringMethod,
                new float3(0f, 3f, 0f),
                float4.Identity,
                new float2(1f, 1f),
                10d,
                4d,
                0.5d);

            AssertApproximatelyEqual(1.4142135f, drivenVelocity.X);
            AssertApproximatelyEqual(3f, drivenVelocity.Y);
            AssertApproximatelyEqual(-1.4142135f, drivenVelocity.Z);
            AssertApproximatelyEqual(2f, new float3(drivenVelocity.X, 0f, drivenVelocity.Z).Length());

            float3 brakingVelocity = InvokeDrivenVelocity(
                steeringMethod,
                new float3(2.5f, -6f, 0f),
                float4.Identity,
                new float2(0f, 0f),
                10d,
                4d,
                0.5d);

            AssertApproximatelyEqual(0.5f, brakingVelocity.X);
            AssertApproximatelyEqual(-6f, brakingVelocity.Y);
            AssertApproximatelyEqual(0f, brakingVelocity.Z);
        }

        /// <summary>
        /// Ensures the Tilt Trial steering helper leaves velocity unchanged when the frame delta is zero so startup frames do not crash the scene.
        /// </summary>
        [Fact]
        public void Tilt_trial_planar_drive_keeps_current_velocity_when_elapsed_time_is_zero() {
            MethodInfo steeringMethod = typeof(city.game.DemoTiltStageComponent).GetMethod(
                "ResolveDrivenLinearVelocity",
                BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(steeringMethod);

            float3 currentVelocity = new float3(2f, 5f, -3f);
            float3 drivenVelocity = InvokeDrivenVelocity(
                steeringMethod,
                currentVelocity,
                float4.Identity,
                new float2(1f, 0f),
                10d,
                4d,
                0d);

            AssertApproximatelyEqual(currentVelocity.X, drivenVelocity.X);
            AssertApproximatelyEqual(currentVelocity.Y, drivenVelocity.Y);
            AssertApproximatelyEqual(currentVelocity.Z, drivenVelocity.Z);
        }

        /// <summary>
        /// Ensures explicit rigid-body linear-velocity access returns the authored value used by gameplay steering code.
        /// </summary>
        [Fact]
        public void Rigid_body_linear_velocity_getter_returns_authored_value() {
            RigidBody3DComponent rigidBody = new RigidBody3DComponent();
            float3 expectedVelocity = new float3(4f, -2f, 7f);

            rigidBody.SetLinearVelocity(expectedVelocity);

            float3 actualVelocity = rigidBody.GetLinearVelocity();

            AssertApproximatelyEqual(expectedVelocity.X, actualVelocity.X);
            AssertApproximatelyEqual(expectedVelocity.Y, actualVelocity.Y);
            AssertApproximatelyEqual(expectedVelocity.Z, actualVelocity.Z);
        }

        /// <summary>
        /// Ensures the Tilt Trial controller ships with the requested movement defaults used by the latest tuning pass.
        /// </summary>
        [Fact]
        public void Tilt_trial_controller_defaults_to_requested_planar_speed_and_acceleration() {
            city.game.DemoTiltStageComponent component = new city.game.DemoTiltStageComponent();

            AssertApproximatelyEqual(11.25f, component.MaximumPlanarSpeed);
            AssertApproximatelyEqual(4.25f, component.PlanarAccelerationUnitsPerSecond);
        }

        /// <summary>
        /// Ensures the Tilt Trial follow camera predicts the imminent post-physics orbit center from the tracked ball velocity so rendered presentation does not lag one frame behind physics.
        /// </summary>
        [Fact]
        public void Tilt_trial_follow_camera_predicts_orbit_center_from_target_velocity() {
            float3 orbitCenter = city.game.DemoTiltFollowCameraComponent.ResolvePredictedOrbitCenter(
                new float3(2f, 3f, 4f),
                new float3(0f, 0.65f, 0f),
                new float3(1.5f, -2f, 0.25f),
                0.5d);

            AssertApproximatelyEqual(2.75f, orbitCenter.X);
            AssertApproximatelyEqual(2.65f, orbitCenter.Y);
            AssertApproximatelyEqual(4.125f, orbitCenter.Z);
        }

        /// <summary>
        /// Ensures the Tilt Trial speed HUD formats the current ball speed as a rounded kilometers-per-hour label.
        /// </summary>
        [Fact]
        public void Tilt_trial_speed_hud_formats_current_ball_speed_in_kilometers_per_hour() {
            string speedText = city.game.DemoTiltSpeedTextComponent.FormatSpeedKilometersPerHour(new float3(3f, 4f, 0f));

            Assert.Equal("18\nkm/h", speedText);
        }

        /// <summary>
        /// Invokes the reflected Tilt Trial steering helper and returns the resolved velocity.
        /// </summary>
        /// <param name="steeringMethod">Reflected steering method under test.</param>
        /// <param name="currentVelocity">Current rigid-body velocity before steering.</param>
        /// <param name="cameraOrientation">Current orbit-camera orientation used for camera-relative movement.</param>
        /// <param name="inputAxes">Raw movement axes before diagonal normalization.</param>
        /// <param name="maximumPlanarSpeed">Configured maximum planar speed.</param>
        /// <param name="planarAccelerationUnitsPerSecond">Configured planar acceleration.</param>
        /// <param name="elapsedSeconds">Elapsed frame time.</param>
        /// <returns>Velocity returned by the steering helper.</returns>
        static float3 InvokeDrivenVelocity(MethodInfo steeringMethod, float3 currentVelocity, float4 cameraOrientation, float2 inputAxes, double maximumPlanarSpeed, double planarAccelerationUnitsPerSecond, double elapsedSeconds) {
            object result = steeringMethod.Invoke(
                null,
                new object[] {
                    currentVelocity,
                    cameraOrientation,
                    inputAxes,
                    maximumPlanarSpeed,
                    planarAccelerationUnitsPerSecond,
                    elapsedSeconds
                });

            Assert.IsType<float3>(result);
            return (float3)result;
        }

        /// <summary>
        /// Asserts that two single-precision values are equal within a tight gameplay-safe tolerance.
        /// </summary>
        /// <param name="expected">Expected scalar value.</param>
        /// <param name="actual">Actual scalar value.</param>
        static void AssertApproximatelyEqual(float expected, float actual) {
            Assert.True(Math.Abs(expected - actual) <= 0.0001f, $"Expected {expected} but received {actual}.");
        }
    }
}
