namespace city.game {
    /// <summary>
    /// Drives one freely orbiting third-person camera around the playable Tilt Trial sphere.
    /// </summary>
    public sealed class DemoTiltFollowCameraComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets whether follow-camera updates should be skipped for the current frame.
        /// </summary>
        public bool UpdatesAreSuppressed { get; set; }

        /// <summary>
        /// Normalized analog threshold used to ignore right-stick drift.
        /// </summary>
        const double GamepadDeadzone = 0.18d;

        /// <summary>
        /// Smallest valid orbit radius accepted when deriving the initial camera orbit from the authored camera pose.
        /// </summary>
        const double MinimumOrbitRadius = 0.001d;

        /// <summary>
        /// Gets or sets the serialized scene reference that identifies the followed player entity.
        /// </summary>
        public SceneEntityReference TargetEntityReference { get; set; }

        /// <summary>
        /// Gets or sets the stable authored entity name used to resolve the followed player across Blueprint boundaries.
        /// </summary>
        public string TargetEntityName { get; set; }

        /// <summary>
        /// Gets or sets the serialized gameplay role used to resolve the followed entity at runtime.
        /// </summary>
        public string TargetEntityRole { get; set; }

        /// <summary>
        /// Gets or sets the world-space offset applied on top of the followed player position before orbit math is evaluated.
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
        /// Stores the authored rigid body attached to the tracked ball so the camera can predict the imminent post-physics center.
        /// </summary>
        RigidBody3DComponent TargetRigidBody;

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
        /// Initializes one Tilt Trial follow camera with Super Monkey Ball-style orbit defaults.
        /// </summary>
        public DemoTiltFollowCameraComponent() {
            UpdatesAreSuppressed = false;
            TargetEntityReference = null;
            TargetEntityName = string.Empty;
            TargetEntityRole = string.Empty;
            TargetEntity = null;
            TargetRigidBody = null;
            CurrentYawRadians = 0f;
            CurrentPitchRadians = 0f;
            CurrentOrbitRadius = 0f;
            IsOrbitInitialized = false;
            UpdateOrder = 1;
            ManualYawSpeedRadians = 1.9f;
            ManualPitchSpeedRadians = 1.35f;
            MinimumPitchRadians = -1.2f;
            MaximumPitchRadians = 0.15f;
            TargetOffset = new float3(0f, 0.65f, 0f);
        }

        /// <summary>
        /// Advances the orbit state from keyboard and gamepad camera input before applying the resolved camera pose around the tracked ball.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoTiltFollowCameraComponent requires an attached parent camera entity.");
            } else if (UpdatesAreSuppressed) {
                return;
            }

            ResolveTargetEntityWhenNeeded();
            ResolveTargetRigidBodyWhenNeeded();
            EnsureOrbitInitialized();

            Core core = Core.Instance ?? throw new InvalidOperationException("A core instance must exist before Tilt Trial follow camera updates can run.");
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
        /// Resolves the orbit center from the imminent tracked target position predicted one frame ahead from the current rigid-body velocity.
        /// </summary>
        /// <param name="targetPosition">Current tracked target position before the physics step.</param>
        /// <param name="targetOffset">Camera target offset applied on top of the tracked target.</param>
        /// <param name="targetLinearVelocity">Current authored target linear velocity.</param>
        /// <param name="elapsedSeconds">Current frame delta in seconds.</param>
        /// <returns>Predicted orbit center for the current frame render.</returns>
        public static float3 ResolvePredictedOrbitCenter(float3 targetPosition, float3 targetOffset, float3 targetLinearVelocity, double elapsedSeconds) {
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Tilt Trial follow camera prediction requires a finite non-negative elapsed time.");
            }

            return targetPosition + targetOffset + (targetLinearVelocity * (float)elapsedSeconds);
        }

        /// <summary>
        /// Resolves the tracked runtime entity from the serialized scene-entity id when the target has not been cached yet.
        /// </summary>
        void ResolveTargetEntityWhenNeeded() {
            if (TargetEntity != null) {
                return;
            } else if (TargetEntityReference == null) {
                throw new InvalidOperationException("DemoTiltFollowCameraComponent requires a serialized target entity reference.");
            } else if (TargetEntityReference.EntityId == 0u) {
                throw new InvalidOperationException("DemoTiltFollowCameraComponent requires a non-zero target scene entity id.");
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial follow camera target resolution can run.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidate = entities[entityIndex];
                uint candidateSceneEntityId = FindSceneEntityRuntimeIdOrZero(candidate);
                if (candidateSceneEntityId == TargetEntityReference.EntityId) {
                    TargetEntity = candidate;
                    return;
                }
            }

            throw new InvalidOperationException($"DemoTiltFollowCameraComponent could not resolve target scene entity id {TargetEntityReference.EntityId}.");
        }

        /// <summary>
        /// Finds one runtime entity by its stable authored name.
        /// </summary>
        /// <param name="name">Entity name to resolve.</param>
        /// <returns>Matching runtime entity, or null when it is not loaded yet.</returns>
        Entity FindEntityByName(string name) {
            return FindEntityByRole(name);
        }

        /// <summary>
        /// Finds one runtime entity carrying the requested serialized gameplay role.
        /// </summary>
        /// <param name="role">Gameplay role to resolve.</param>
        /// <returns>Matching runtime entity, or null when it is not loaded yet.</returns>
        Entity FindEntityByRole(string role) {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity match = FindEntityByRoleRecursive(entities[entityIndex], role);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches one entity hierarchy for a serialized gameplay role.
        /// </summary>
        /// <param name="entity">Current hierarchy entity.</param>
        /// <param name="role">Gameplay role to resolve.</param>
        /// <returns>Matching entity, or null when the subtree does not contain it.</returns>
        static Entity FindEntityByRoleRecursive(Entity entity, string role) {
            if (entity == null) {
                return null;
            }
            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is TiltTrialEntityRoleComponent roleComponent
                        && string.Equals(roleComponent.Role, role, StringComparison.Ordinal)) {
                        return entity;
                    }
                }
            }
            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                Entity match = FindEntityByRoleRecursive(entity.Children[childIndex], role);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches one entity hierarchy for a stable authored name.
        /// </summary>
        /// <param name="entity">Current hierarchy entity.</param>
        /// <param name="name">Entity name to resolve.</param>
        /// <returns>Matching entity, or null when the subtree does not contain it.</returns>
        static Entity FindEntityByNameRecursive(Entity entity, string name) {
            return FindEntityByRoleRecursive(entity, name);
        }

        /// <summary>
        /// Resolves the authored rigid body attached to the tracked ball when available.
        /// </summary>
        void ResolveTargetRigidBodyWhenNeeded() {
            if (TargetRigidBody != null || TargetEntity == null || TargetEntity.Components == null) {
                return;
            }

            for (int componentIndex = 0; componentIndex < TargetEntity.Components.Count; componentIndex++) {
                if (TargetEntity.Components[componentIndex] is RigidBody3DComponent rigidBody) {
                    TargetRigidBody = rigidBody;
                    return;
                }
            }
        }

        /// <summary>
        /// Finds the runtime scene-id component attached to one candidate entity when present.
        /// </summary>
        /// <param name="entity">Candidate runtime entity.</param>
        /// <returns>Resolved authored scene entity id when present; otherwise <c>0</c>.</returns>
        uint FindSceneEntityRuntimeIdOrZero(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
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
                throw new InvalidOperationException("DemoTiltFollowCameraComponent requires the authored camera pose to be offset from the tracked target.");
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
        /// Resolves the current world-space orbit center from the followed ball and configured offset.
        /// </summary>
        /// <returns>Current world-space orbit center.</returns>
        float3 ResolveOrbitCenter() {
            if (TargetEntity == null) {
                throw new InvalidOperationException("DemoTiltFollowCameraComponent requires a resolved target entity before orbit evaluation.");
            }

            Core core = Core.Instance;
            double elapsedSeconds = core != null ? core.PredictedPhysicsStepSeconds : 0d;
            float3 targetLinearVelocity = TargetRigidBody != null ? TargetRigidBody.GetLinearVelocity() : float3.Zero;
            return ResolvePredictedOrbitCenter(TargetEntity.Position, TargetOffset, targetLinearVelocity, elapsedSeconds);
        }

        /// <summary>
        /// Resolves normalized camera yaw input from keyboard arrows, shoulder buttons, and the right-stick horizontal axis.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>Normalized yaw input in the range [-1, 1].</returns>
        double ResolveYawInput(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            double keyboardYaw = 0d;
            if (inputSystem.IsKeyDown(Keys.Left)) {
                keyboardYaw -= 1d;
            }
            if (inputSystem.IsKeyDown(Keys.Right)) {
                keyboardYaw += 1d;
            }

            InputGamepadState gamepadState = inputSystem.GetGamepadState(0);
            double gamepadYaw = 0d;
            if (gamepadState.Connected) {
                if (gamepadState.IsButtonDown(InputGamepadButton.LeftShoulder)) {
                    gamepadYaw -= 1d;
                }
                if (gamepadState.IsButtonDown(InputGamepadButton.RightShoulder)) {
                    gamepadYaw += 1d;
                }

                gamepadYaw += NormalizeStickAxis(gamepadState.RightStickX);
            }

            return Math.Clamp(keyboardYaw + gamepadYaw, -1d, 1d);
        }

        /// <summary>
        /// Resolves normalized camera pitch input from keyboard arrows and the right-stick vertical axis.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>Normalized pitch input in the range [-1, 1].</returns>
        double ResolvePitchInput(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            double keyboardPitch = 0d;
            if (inputSystem.IsKeyDown(Keys.Up)) {
                keyboardPitch += 1d;
            }
            if (inputSystem.IsKeyDown(Keys.Down)) {
                keyboardPitch -= 1d;
            }

            InputGamepadState gamepadState = inputSystem.GetGamepadState(0);
            double gamepadPitch = 0d;
            if (gamepadState.Connected) {
                gamepadPitch += -NormalizeStickAxis(gamepadState.RightStickY);
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
        /// Applies the resolved orbit yaw, pitch, and radius around the tracked player.
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
