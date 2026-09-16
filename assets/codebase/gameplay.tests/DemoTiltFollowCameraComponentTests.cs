using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial follow camera maps shoulder buttons to the requested orbit directions.
    /// </summary>
    public sealed class DemoTiltFollowCameraComponentTests {
        /// <summary>
        /// Ensures L1 rotates left and R1 rotates right while preserving the existing PSP-specific reversal.
        /// </summary>
        [Fact]
        public void Tilt_follow_camera_maps_shoulders_to_requested_orbit_directions() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\DemoTiltFollowCameraComponent.cs").Replace("\r\n", "\n");

            Assert.Contains(
                "if (city.menu.DemoDiscGamepadInput.IsButtonDown(inputSystem, InputGamepadButton.LeftShoulder)) {\n                gamepadYaw += invertPspShoulderCamera ? -1d : 1d;\n            }\n            if (city.menu.DemoDiscGamepadInput.IsButtonDown(inputSystem, InputGamepadButton.RightShoulder)) {\n                gamepadYaw += invertPspShoulderCamera ? 1d : -1d;",
                source,
                StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures both right-stick camera axes are inverted at the camera consumer so stick motion matches the requested orbit directions on every platform.
        /// </summary>
        [Fact]
        public void Tilt_follow_camera_inverts_both_right_stick_axes() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\DemoTiltFollowCameraComponent.cs").Replace("\r\n", "\n");

            Assert.Contains(
                "gamepadYaw -= NormalizeStickAxis(city.menu.DemoDiscGamepadInput.GetRightStickX(inputSystem));",
                source,
                StringComparison.Ordinal);
            Assert.Contains(
                "gamepadPitch += NormalizeStickAxis(city.menu.DemoDiscGamepadInput.GetRightStickY(inputSystem));",
                source,
                StringComparison.Ordinal);
        }
    }
}
