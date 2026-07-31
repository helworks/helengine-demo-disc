namespace city.rendering {
    /// <summary>
    /// Drives one manual orbit camera that follows a serialized target entity by position while ignoring the target's physical rotation.
    /// </summary>
    public sealed class DemoFollowCameraComponent : UpdateComponent {
        /// <summary>
        /// Normalized analog threshold used to ignore small stick noise.
        /// </summary>
        const double GamepadDeadzone = 0.18d;

        /// <summary>
        /// Smallest valid orbit radius used when deriving the initial orbit from the authored camera pose.
        /// </summary>
        const double MinimumOrbitRadius = 0.001d;

        /// <summary>
        /// Gets or sets the serialized scene reference that identifies the followed target entity.
        /// </summary>
        public SceneEntityReference TargetEntityReference { get; set; }

        /// <summary>
        /// Gets or sets the world-space offset applied on top of the followed target position before orbit math is evaluated.
        /// </summary>
        public float3 TargetOffset { get; set; }

        /// <summary>
        /// Gets or sets the manual yaw speed in radians per second while the player is steering the camera.
        /// </summary>
        public float ManualYawSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the manual pitch speed in radians per second while the player is steering the camera.
        /// </summary>
        public float ManualPitchSpeedRadians { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed orbit pitch in radians.
        /// </summary>
        public float MinimumPitchRadians { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed orbit pitch in radians.
        /// </summary>
        public float MaximumPitchRadians { get; set; }

        /// <summary>
        /// Stores the resolved live runtime target entity.
        /// </summary>
        Entity TargetEntity;

        /// <summary>
        /// Stores the current orbit yaw in radians.
        /// </summary>
        float CurrentYawRadians;

        /// <summary>
        /// Stores the current orbit pitch in radians.
        /// </summary>
        float CurrentPitchRadians;

        /// <summary>
        /// Stores the current orbit radius derived from the authored camera pose.
        /// </summary>
        float CurrentOrbitRadius;

        /// <summary>
        /// Tracks whether the initial orbit state has already been derived from the authored camera pose.
        /// </summary>
        bool IsOrbitInitialized;

        /// <summary>
        /// Initializes one follow camera with demo defaults tuned for the static-mesh showcase.
        /// </summary>
        public DemoFollowCameraComponent() {
            ManualYawSpeedRadians = 1.8f;
            ManualPitchSpeedRadians = 1.25f;
            MinimumPitchRadians = -1.2f;
            MaximumPitchRadians = 0.35f;
            TargetOffset = new float3(0f, 1.4f, 0f);
        }

        /// <summary>
        /// Advances the orbit state from current player input and applies the resolved camera pose around the tracked target.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoFollowCameraComponent requires an attached parent camera entity.");
            }

            ResolveTargetEntityWhenNeeded();
            EnsureOrbitInitialized();

            Core core = Core.Instance ?? throw new InvalidOperationException("A core instance must exist before follow camera updates can run.");
            InputSystem inputSystem = core.Input;
            double elapsedSeconds = core.FrameDeltaSeconds;
            double yawInput = ResolveYawInput(inputSystem);
            double pitchInput = ResolvePitchInput(inputSystem);

            CurrentYawRadians += (float)(yawInput * ManualYawSpeedRadians * elapsedSeconds);
            CurrentPitchRadians -= (float)(pitchInput * ManualPitchSpeedRadians * elapsedSeconds);
            CurrentPitchRadians = ClampPitch(CurrentPitchRadians);

            ApplyOrbitPose();
        }

        /// <summary>
        /// Resolves the tracked runtime entity from the serialized scene-entity id when the target has not been cached yet.
        /// </summary>
        void ResolveTargetEntityWhenNeeded() {
            if (TargetEntity != null) {
                return;
            }
            if (TargetEntityReference == null) {
                throw new InvalidOperationException("DemoFollowCameraComponent requires a serialized target entity reference.");
            }
            if (TargetEntityReference.EntityId == 0u) {
                throw new InvalidOperationException("DemoFollowCameraComponent requires a non-zero target scene entity id.");
            }
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before follow camera target resolution can run.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidate = entities[entityIndex];
                uint candidateSceneEntityId = FindSceneEntityRuntimeIdOrZero(candidate);
                if (candidateSceneEntityId == 0u) {
                    continue;
                }
                if (candidateSceneEntityId == TargetEntityReference.EntityId) {
                    TargetEntity = candidate;
                    return;
                }
            }

            throw new InvalidOperationException($"DemoFollowCameraComponent could not resolve target scene entity id {TargetEntityReference.EntityId}.");
        }

        /// <summary>
        /// Finds the runtime scene-id component attached to one candidate entity when present.
        /// </summary>
        /// <param name="entity">Candidate runtime entity.</param>
        /// <returns>Resolved authored scene entity id when present; otherwise <c>0</c>.</returns>
        uint FindSceneEntityRuntimeIdOrZero(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }
            if (entity.Components == null) {
                return 0u;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is SceneEntityRuntimeIdComponent runtimeIdComponent) {
                    return runtimeIdComponent.SceneEntityId;
                }
            }

            return 0u;
        }

        /// <summary>
        /// Derives the initial orbit radius, yaw, and pitch from the authored camera pose the first time the component updates.
        /// </summary>
        void EnsureOrbitInitialized() {
            if (IsOrbitInitialized) {
                return;
            }

            float3 orbitCenter = ResolveOrbitCenter();
            float3 offset = Parent.LocalPosition - orbitCenter;
            double orbitRadius = Math.Sqrt((offset.X * offset.X) + (offset.Y * offset.Y) + (offset.Z * offset.Z));
            if (orbitRadius <= MinimumOrbitRadius) {
                throw new InvalidOperationException("DemoFollowCameraComponent requires the authored camera pose to be offset from the tracked target.");
            }

            double horizontalRadius = Math.Sqrt((offset.X * offset.X) + (offset.Z * offset.Z));
            double yawRadians = Math.Atan2(offset.X, offset.Z);
            double pitchRadians = -Math.Atan2(offset.Y, horizontalRadius);

            CurrentOrbitRadius = (float)orbitRadius;
            CurrentYawRadians = (float)yawRadians;
            CurrentPitchRadians = ClampPitch((float)pitchRadians);
            IsOrbitInitialized = true;
        }

        /// <summary>
        /// Resolves the world-space orbit center from the followed target position and configured offset.
        /// </summary>
        /// <returns>World-space orbit center.</returns>
        float3 ResolveOrbitCenter() {
            if (TargetEntity == null) {
                throw new InvalidOperationException("DemoFollowCameraComponent requires a resolved target entity before orbit evaluation.");
            }

            return TargetEntity.Position + TargetOffset;
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
#if DESKTOP_PLATFORM
            if (inputSystem.IsKeyDown(Keys.A)) {
                keyboardYaw -= 1d;
            }
            if (inputSystem.IsKeyDown(Keys.D)) {
                keyboardYaw += 1d;
            }
#endif

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
#if DESKTOP_PLATFORM
            if (inputSystem.IsKeyDown(Keys.W)) {
                keyboardPitch += 1d;
            }
            if (inputSystem.IsKeyDown(Keys.S)) {
                keyboardPitch -= 1d;
            }
#endif

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
        /// Applies the resolved orbit yaw, pitch, and radius around the current followed target position.
        /// </summary>
        void ApplyOrbitPose() {
            float3 orbitCenter = ResolveOrbitCenter();
            double horizontalRadius = Math.Cos(CurrentPitchRadians) * CurrentOrbitRadius;
            double x = orbitCenter.X + (Math.Sin(CurrentYawRadians) * horizontalRadius);
            double y = orbitCenter.Y - (Math.Sin(CurrentPitchRadians) * CurrentOrbitRadius);
            double z = orbitCenter.Z + (Math.Cos(CurrentYawRadians) * horizontalRadius);
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
    }
}
