namespace city.rendering {
    /// <summary>
    /// Drives one shared demo-disc orbit camera that can hand control to the player and then smoothly return to auto rotation after inactivity.
    /// </summary>
    public sealed class DemoDiscOrbitCameraComponent : UpdateComponent {
        /// <summary>
        /// Normalized analog threshold used to ignore small stick noise.
        /// </summary>
        const double GamepadDeadzone = 0.18d;

        /// <summary>
        /// Smallest valid orbit radius used when deriving the initial orbit from the authored camera pose.
        /// </summary>
        const double MinimumOrbitRadius = 0.001d;

        /// <summary>
        /// Gets or sets the world-space orbit center that the camera should circle around.
        /// </summary>
        public float3 OrbitCenter { get; set; }

        /// <summary>
        /// Gets or sets the automatic yaw speed in radians per second while the camera is in auto-orbit mode.
        /// </summary>
        public float AutoYawSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the manual yaw speed in radians per second while the player is actively steering the camera.
        /// </summary>
        public float ManualYawSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the manual pitch speed in radians per second while the player is actively steering the camera.
        /// </summary>
        public float ManualPitchSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the delay, in seconds, before auto-orbit begins fading back in after player input stops.
        /// </summary>
        public double IdleReturnDelaySeconds { get; set; }

        /// <summary>
        /// Gets or sets the rate used to fade automatic orbit influence back in after the idle delay elapses.
        /// </summary>
        public double AutoReturnBlendSpeed { get; set; }

        /// <summary>
        /// Gets or sets the speed used to ease the camera pitch back toward its authored auto-orbit pitch.
        /// </summary>
        public float AutoPitchReturnSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed orbit pitch in radians.
        /// </summary>
        public float MinimumPitchRadians { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed orbit pitch in radians.
        /// </summary>
        public float MaximumPitchRadians { get; set; }

        /// <summary>
        /// Stores the current orbit yaw in radians.
        /// </summary>
        float CurrentYawRadians;

        /// <summary>
        /// Stores the current orbit pitch in radians.
        /// </summary>
        float CurrentPitchRadians;

        /// <summary>
        /// Stores the orbit radius derived from the authored camera pose.
        /// </summary>
        float CurrentOrbitRadius;

        /// <summary>
        /// Stores the authored auto-orbit pitch that the controller should ease back toward after inactivity.
        /// </summary>
        float AutoPitchRadians;

        /// <summary>
        /// Stores the number of consecutive idle seconds since the last manual orbit input.
        /// </summary>
        double IdleElapsedSeconds;

        /// <summary>
        /// Stores the current automatic orbit influence in the range [0, 1].
        /// </summary>
        double AutoOrbitBlend;

        /// <summary>
        /// Tracks whether the initial orbit state has already been derived from the authored camera pose.
        /// </summary>
        bool IsOrbitInitialized;

        /// <summary>
        /// Initializes one orbit controller with demo-disc defaults tuned for showcase scenes.
        /// </summary>
        public DemoDiscOrbitCameraComponent() {
            AutoYawSpeedRadians = 0.07f;
            ManualYawSpeedRadians = 1.5f;
            ManualPitchSpeedRadians = 1.2f;
            IdleReturnDelaySeconds = 10d;
            AutoReturnBlendSpeed = 0.35d;
            AutoPitchReturnSpeedRadians = 0.8f;
            MinimumPitchRadians = -1.2f;
            MaximumPitchRadians = 0.2f;
            AutoOrbitBlend = 1d;
            IdleElapsedSeconds = IdleReturnDelaySeconds;
        }

        /// <summary>
        /// Advances the orbit state from current player input and applies the resolved camera pose to the parent entity.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoDiscOrbitCameraComponent requires an attached parent camera entity.");
            }

            EnsureOrbitInitialized();

            Core core = Core.Instance ?? throw new InvalidOperationException("A core instance must exist before orbit camera updates can run.");
            InputSystem inputSystem = core.Input;
            double elapsedSeconds = core.FrameDeltaSeconds;
            double yawInput = ResolveYawInput(inputSystem);
            double pitchInput = ResolvePitchInput(inputSystem);
            bool hasManualInput = Math.Abs(yawInput) > 0.0001d || Math.Abs(pitchInput) > 0.0001d;

            if (hasManualInput) {
                IdleElapsedSeconds = 0d;
                AutoOrbitBlend = 0d;
                CurrentYawRadians += (float)(yawInput * ManualYawSpeedRadians * elapsedSeconds);
                CurrentPitchRadians -= (float)(pitchInput * ManualPitchSpeedRadians * elapsedSeconds);
                CurrentPitchRadians = ClampPitch(CurrentPitchRadians);
            } else {
                IdleElapsedSeconds += elapsedSeconds;
                if (IdleElapsedSeconds >= IdleReturnDelaySeconds) {
                    AutoOrbitBlend = Math.Min(1d, AutoOrbitBlend + (AutoReturnBlendSpeed * elapsedSeconds));
                } else {
                    AutoOrbitBlend = 0d;
                }

                if (AutoOrbitBlend > 0d) {
                    CurrentYawRadians += (float)(AutoYawSpeedRadians * elapsedSeconds * AutoOrbitBlend);
                    CurrentPitchRadians = MoveToward(CurrentPitchRadians, AutoPitchRadians, AutoPitchReturnSpeedRadians * (float)(elapsedSeconds * AutoOrbitBlend));
                }
            }

            ApplyOrbitPose();
        }

        /// <summary>
        /// Derives the initial orbit radius, yaw, and pitch from the authored camera pose the first time the component updates.
        /// </summary>
        void EnsureOrbitInitialized() {
            if (IsOrbitInitialized) {
                return;
            }

            float3 offset = Parent.LocalPosition - OrbitCenter;
            double orbitRadius = Math.Sqrt((offset.X * offset.X) + (offset.Y * offset.Y) + (offset.Z * offset.Z));
            if (orbitRadius <= MinimumOrbitRadius) {
                throw new InvalidOperationException("DemoDiscOrbitCameraComponent requires the authored camera pose to be offset from the orbit center.");
            }

            double horizontalRadius = Math.Sqrt((offset.X * offset.X) + (offset.Z * offset.Z));
            double yawRadians = Math.Atan2(offset.X, offset.Z);
            double pitchRadians = -Math.Atan2(offset.Y, horizontalRadius);

            CurrentOrbitRadius = (float)orbitRadius;
            CurrentYawRadians = (float)yawRadians;
            CurrentPitchRadians = ClampPitch((float)pitchRadians);
            AutoPitchRadians = CurrentPitchRadians;
            IsOrbitInitialized = true;
        }

        /// <summary>
        /// Resolves the normalized yaw input from keyboard, d-pad, and left-stick horizontal input.
        /// </summary>
        /// <param name="inputSystem">Input system supplying current frame input.</param>
        /// <returns>Normalized yaw input in the range [-1, 1].</returns>
        double ResolveYawInput(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            double keyboardYaw = 0d;
            if (inputSystem.IsKeyDown(Keys.A)) {
                keyboardYaw -= 1d;
            }
            if (inputSystem.IsKeyDown(Keys.D)) {
                keyboardYaw += 1d;
            }

            InputGamepadState gamepadState = inputSystem.GetGamepadState(0);
            double gamepadYaw = 0d;
            if (gamepadState.Connected) {
                if (gamepadState.IsButtonDown(InputGamepadButton.DPadLeft)) {
                    gamepadYaw -= 1d;
                }
                if (gamepadState.IsButtonDown(InputGamepadButton.DPadRight)) {
                    gamepadYaw += 1d;
                }

                gamepadYaw += NormalizeStickAxis(gamepadState.LeftStickX);
            }

            return Math.Clamp(keyboardYaw + gamepadYaw, -1d, 1d);
        }

        /// <summary>
        /// Resolves the normalized pitch input from keyboard, d-pad, and left-stick vertical input.
        /// </summary>
        /// <param name="inputSystem">Input system supplying current frame input.</param>
        /// <returns>Normalized pitch input in the range [-1, 1].</returns>
        double ResolvePitchInput(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            double keyboardPitch = 0d;
            if (inputSystem.IsKeyDown(Keys.W)) {
                keyboardPitch += 1d;
            }
            if (inputSystem.IsKeyDown(Keys.S)) {
                keyboardPitch -= 1d;
            }

            InputGamepadState gamepadState = inputSystem.GetGamepadState(0);
            double gamepadPitch = 0d;
            if (gamepadState.Connected) {
                if (gamepadState.IsButtonDown(InputGamepadButton.DPadUp)) {
                    gamepadPitch += 1d;
                }
                if (gamepadState.IsButtonDown(InputGamepadButton.DPadDown)) {
                    gamepadPitch -= 1d;
                }

                gamepadPitch += -NormalizeStickAxis(gamepadState.LeftStickY);
            }

            return Math.Clamp(keyboardPitch + gamepadPitch, -1d, 1d);
        }

        /// <summary>
        /// Converts one signed stick axis into a normalized analog input while applying the configured deadzone.
        /// </summary>
        /// <param name="axisValue">Raw signed stick axis value.</param>
        /// <returns>Normalized analog input in the range [-1, 1].</returns>
        double NormalizeStickAxis(short axisValue) {
            double normalized = axisValue / 32767d;
            if (Math.Abs(normalized) < GamepadDeadzone) {
                return 0d;
            }

            return Math.Clamp(normalized, -1d, 1d);
        }

        /// <summary>
        /// Applies the resolved orbit yaw, pitch, and radius to the parent camera entity.
        /// </summary>
        void ApplyOrbitPose() {
            double horizontalRadius = Math.Cos(CurrentPitchRadians) * CurrentOrbitRadius;
            double x = OrbitCenter.X + (Math.Sin(CurrentYawRadians) * horizontalRadius);
            double y = OrbitCenter.Y - (Math.Sin(CurrentPitchRadians) * CurrentOrbitRadius);
            double z = OrbitCenter.Z + (Math.Cos(CurrentYawRadians) * horizontalRadius);
            Parent.LocalPosition = new float3((float)x, (float)y, (float)z);

            float4 orientation;
            float4.CreateFromYawPitchRoll(CurrentYawRadians, CurrentPitchRadians, 0f, out orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;
        }

        /// <summary>
        /// Clamps one requested orbit pitch into the configured supported range.
        /// </summary>
        /// <param name="pitchRadians">Requested pitch in radians.</param>
        /// <returns>Clamped pitch in radians.</returns>
        float ClampPitch(float pitchRadians) {
            return (float)Math.Clamp(pitchRadians, MinimumPitchRadians, MaximumPitchRadians);
        }

        /// <summary>
        /// Moves one scalar value toward a target without overshooting by the supplied step amount.
        /// </summary>
        /// <param name="currentValue">Current value.</param>
        /// <param name="targetValue">Target value.</param>
        /// <param name="maximumStep">Maximum step to move this frame.</param>
        /// <returns>Updated value after moving toward the target.</returns>
        float MoveToward(float currentValue, float targetValue, float maximumStep) {
            if (maximumStep <= 0f) {
                return currentValue;
            }

            double delta = targetValue - currentValue;
            if (Math.Abs(delta) <= maximumStep) {
                return targetValue;
            }

            if (delta > 0f) {
                return currentValue + maximumStep;
            }

            return currentValue - maximumStep;
        }
    }
}
