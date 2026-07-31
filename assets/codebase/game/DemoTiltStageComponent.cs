namespace city.game {
    /// <summary>
    /// Drives the playable Tilt Trial sphere with camera-relative planar velocity steering while leaving the authored course fixed in place.
    /// </summary>
    public sealed class DemoTiltStageComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets whether stage movement updates should be skipped for the current frame.
        /// </summary>
        public bool UpdatesAreSuppressed { get; set; }

        /// <summary>
        /// Normalized analog threshold used to ignore left-stick drift.
        /// </summary>
        const double GamepadDeadzone = 0.18d;

        /// <summary>
        /// Smallest planar vector length treated as valid when deriving camera-relative movement directions.
        /// </summary>
        const double MinimumPlanarLengthSquared = 0.0000000001d;

        /// <summary>
        /// Canonical local forward axis used to derive one camera-relative planar forward vector from the orbit-camera orientation.
        /// </summary>
        static readonly float3 CameraForwardAxis = new float3(0f, 0f, -1f);

        /// <summary>
        /// Canonical local right axis used to derive one camera-relative planar right vector from the orbit-camera orientation.
        /// </summary>
        static readonly float3 CameraRightAxis = new float3(1f, 0f, 0f);

        /// <summary>
        /// Stores the resolved runtime playable sphere entity once scene lookup succeeds.
        /// </summary>
        Entity PlayerSphereEntity;

        /// <summary>
        /// Stores the authored rigid-body component that backs the playable sphere.
        /// </summary>
        RigidBody3DComponent PlayerSphereRigidBody;

        /// <summary>
        /// Stores the resolved runtime orbit-camera entity once scene lookup succeeds.
        /// </summary>
        Entity OrbitCameraEntity;

        /// <summary>
        /// Stores the resolved follow-camera component that owns the active Tilt Trial orbit state.
        /// </summary>
        DemoTiltFollowCameraComponent FollowCameraComponent;

        /// <summary>
        /// Gets or sets the maximum planar speed applied to the driven sphere while input is held.
        /// </summary>
        public float MaximumPlanarSpeed { get; set; }

        /// <summary>
        /// Gets or sets the maximum planar acceleration used to approach the requested target velocity.
        /// </summary>
        public float PlanarAccelerationUnitsPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the normalized stick threshold used to ignore left-stick drift during gameplay.
        /// </summary>
        public float GamepadDeadzoneThreshold { get; set; }

        /// <summary>
        /// Initializes one Tilt Trial ball-drive controller with moderated planar movement defaults tuned for the close follow camera.
        /// </summary>
        public DemoTiltStageComponent() {
            UpdatesAreSuppressed = false;
            PlayerSphereEntity = null;
            PlayerSphereRigidBody = null;
            OrbitCameraEntity = null;
            FollowCameraComponent = null;
            MaximumPlanarSpeed = 11.25f;
            PlanarAccelerationUnitsPerSecond = 4.25f;
            GamepadDeadzoneThreshold = (float)GamepadDeadzone;
        }

        /// <summary>
        /// Resolves the active Tilt Trial runtime wiring, reads movement input, and steers the playable sphere without moving stage geometry.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires an attached stage root entity.");
            } else if (UpdatesAreSuppressed) {
                return;
            }

            if (!ResolveRuntimeDependenciesWhenNeeded()) {
                return;
            }

            Core core = Core.Instance ?? throw new InvalidOperationException("A core instance must exist before Tilt Trial updates can run.");
            double elapsedSeconds = core.FrameDeltaSeconds;
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(core.FrameDeltaSeconds), "Tilt Trial updates require a finite non-negative frame delta.");
            } else if (elapsedSeconds == 0d) {
                return;
            }

            float2 inputAxes = ResolveMovementInput(core.Input);
            if (inputAxes.X == 0f && inputAxes.Y == 0f) {
                return;
            }

            PlayerSphereRigidBody.SetLinearVelocity(ResolveDrivenLinearVelocity(
                PlayerSphereRigidBody.GetLinearVelocity(),
                OrbitCameraEntity.Orientation,
                inputAxes,
                MaximumPlanarSpeed,
                PlanarAccelerationUnitsPerSecond,
                elapsedSeconds));
            ResolveRequiredPhysicsWorld().SynchronizeDynamicBodyVelocity(PlayerSphereEntity);
        }

        /// <summary>
        /// Resolves the world-space driven sphere velocity for one frame of camera-relative planar input while preserving the incoming vertical velocity.
        /// </summary>
        /// <param name="currentVelocity">Current rigid-body velocity before steering.</param>
        /// <param name="cameraOrientation">Current orbit-camera orientation that defines movement heading.</param>
        /// <param name="inputAxes">Raw horizontal and forward movement axes before diagonal normalization.</param>
        /// <param name="maximumPlanarSpeed">Maximum requested planar speed in world units per second.</param>
        /// <param name="planarAccelerationUnitsPerSecond">Maximum planar acceleration in world units per second squared.</param>
        /// <param name="elapsedSeconds">Elapsed frame time in seconds.</param>
        /// <returns>Driven world-space velocity after planar steering for the current frame.</returns>
        public static float3 ResolveDrivenLinearVelocity(float3 currentVelocity, float4 cameraOrientation, float2 inputAxes, double maximumPlanarSpeed, double planarAccelerationUnitsPerSecond, double elapsedSeconds) {
            if (double.IsNaN(maximumPlanarSpeed) || double.IsInfinity(maximumPlanarSpeed) || maximumPlanarSpeed < 0d) {
                throw new ArgumentOutOfRangeException(nameof(maximumPlanarSpeed), "Maximum planar speed must be finite and non-negative.");
            } else if (double.IsNaN(planarAccelerationUnitsPerSecond) || double.IsInfinity(planarAccelerationUnitsPerSecond) || planarAccelerationUnitsPerSecond < 0d) {
                throw new ArgumentOutOfRangeException(nameof(planarAccelerationUnitsPerSecond), "Planar acceleration must be finite and non-negative.");
            } else if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Tilt Trial updates require a finite non-negative elapsed time.");
            } else if (elapsedSeconds == 0d) {
                return currentVelocity;
            }

            float3 targetPlanarVelocity = float3.Zero;
            if (!(inputAxes.X == 0f && inputAxes.Y == 0f)) {
                float3 planarForward = ResolveFlattenedPlanarAxis(float4.RotateVector(CameraForwardAxis, cameraOrientation), "forward");
                float3 planarRight = ResolveFlattenedPlanarAxis(float4.RotateVector(CameraRightAxis, cameraOrientation), "right");
                float3 rawMove = (planarRight * inputAxes.X) + (planarForward * inputAxes.Y);
                float3 moveDirection = NormalizePlanarOrThrow(rawMove, "combined movement");
                targetPlanarVelocity = moveDirection * (float)maximumPlanarSpeed;
            }

            float3 currentPlanarVelocity = new float3(currentVelocity.X, 0f, currentVelocity.Z);
            float3 drivenPlanarVelocity = MovePlanarVelocityTowardTarget(
                currentPlanarVelocity,
                targetPlanarVelocity,
                planarAccelerationUnitsPerSecond * elapsedSeconds);
            return new float3(drivenPlanarVelocity.X, currentVelocity.Y, drivenPlanarVelocity.Z);
        }

        /// <summary>
        /// Resolves the cached follow camera, followed sphere, and sphere rigid body required by the Tilt Trial controller.
        /// </summary>
        bool ResolveRuntimeDependenciesWhenNeeded() {
            if (!ResolveFollowCameraWhenNeeded()) {
                return false;
            }
            ResolvePlayerSphereWhenNeeded();

            if (PlayerSphereRigidBody != null) {
                return true;
            }

            PlayerSphereRigidBody = FindRequiredRigidBodyComponent(PlayerSphereEntity);
            return true;
        }

        /// <summary>
        /// Resolves the active Tilt Trial follow-camera component and its owning camera entity from the live scene.
        /// </summary>
        bool ResolveFollowCameraWhenNeeded() {
            if (FollowCameraComponent != null && OrbitCameraEntity != null) {
                return true;
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial camera resolution can run.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity entity = entities[entityIndex];
                DemoTiltFollowCameraComponent component = FindFollowCameraComponentOrNull(entity);
                if (component == null) {
                    continue;
                }

                OrbitCameraEntity = entity;
                FollowCameraComponent = component;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves the playable sphere entity from the serialized target reference owned by the active follow camera.
        /// </summary>
        void ResolvePlayerSphereWhenNeeded() {
            if (PlayerSphereEntity != null) {
                return;
            } else if (FollowCameraComponent == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires a resolved follow camera before player resolution can run.");
            } else if (FollowCameraComponent.TargetEntityReference == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires the Tilt Trial follow camera to expose a serialized player target reference.");
            } else if (FollowCameraComponent.TargetEntityReference.EntityId == 0u) {
                throw new InvalidOperationException("DemoTiltStageComponent requires the Tilt Trial follow camera to reference a non-zero scene entity id.");
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial player resolution can run.");
            }

            uint targetSceneEntityId = FollowCameraComponent.TargetEntityReference.EntityId;
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity entity = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(entity) != targetSceneEntityId) {
                    continue;
                }

                PlayerSphereEntity = entity;
                return;
            }

            throw new InvalidOperationException($"DemoTiltStageComponent could not resolve the Tilt Trial player sphere for scene entity id {targetSceneEntityId}.");
        }

        /// <summary>
        /// Resolves the keyboard and left-stick movement axes for the current frame.
        /// </summary>
        /// <param name="inputSystem">Input system supplying keyboard and gamepad state.</param>
        /// <returns>Combined horizontal and forward movement axes before diagonal normalization.</returns>
        float2 ResolveMovementInput(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            double horizontal = 0d;
#if DESKTOP_PLATFORM
            if (inputSystem.IsKeyDown(Keys.A)) {
                horizontal -= 1d;
            }
            if (inputSystem.IsKeyDown(Keys.D)) {
                horizontal += 1d;
            }
#endif

            double forward = 0d;
#if DESKTOP_PLATFORM
            if (inputSystem.IsKeyDown(Keys.W)) {
                forward += 1d;
            }
            if (inputSystem.IsKeyDown(Keys.S)) {
                forward -= 1d;
            }
#endif

            if (city.menu.DemoDiscGamepadInput.IsButtonDown(inputSystem, InputGamepadButton.DPadLeft)) {
                horizontal -= 1d;
            }
            if (city.menu.DemoDiscGamepadInput.IsButtonDown(inputSystem, InputGamepadButton.DPadRight)) {
                horizontal += 1d;
            }
            if (city.menu.DemoDiscGamepadInput.IsButtonDown(inputSystem, InputGamepadButton.DPadUp)) {
                forward += 1d;
            }
            if (city.menu.DemoDiscGamepadInput.IsButtonDown(inputSystem, InputGamepadButton.DPadDown)) {
                forward -= 1d;
            }

            horizontal += NormalizeStickAxis(city.menu.DemoDiscGamepadInput.GetLeftStickX(inputSystem));
            forward += -NormalizeStickAxis(city.menu.DemoDiscGamepadInput.GetLeftStickY(inputSystem));

            return new float2(
                (float)Math.Clamp(horizontal, -1d, 1d),
                (float)Math.Clamp(forward, -1d, 1d));
        }

        /// <summary>
        /// Converts one signed stick axis into a normalized analog input while applying the configured deadzone.
        /// </summary>
        /// <param name="axisValue">Raw signed stick axis value.</param>
        /// <returns>Normalized analog input in the range [-1, 1].</returns>
        double NormalizeStickAxis(short axisValue) {
            double normalized = axisValue / 32767d;
            if (Math.Abs(normalized) < GamepadDeadzoneThreshold) {
                return 0d;
            }

            return Math.Clamp(normalized, -1d, 1d);
        }

        /// <summary>
        /// Resolves the authored rigid-body component attached to the playable sphere.
        /// </summary>
        /// <param name="entity">Playable sphere entity whose rigid body should be returned.</param>
        /// <returns>Attached rigid-body component.</returns>
        RigidBody3DComponent FindRequiredRigidBodyComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires the playable sphere to expose an initialized component list.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is RigidBody3DComponent rigidBody) {
                    return rigidBody;
                }
            }

            throw new InvalidOperationException("DemoTiltStageComponent requires a RigidBody3DComponent on the playable sphere.");
        }

        /// <summary>
        /// Resolves the follow-camera component attached to one candidate entity when present.
        /// </summary>
        /// <param name="entity">Candidate runtime entity.</param>
        /// <returns>Attached follow-camera component when present; otherwise <c>null</c>.</returns>
        DemoTiltFollowCameraComponent FindFollowCameraComponentOrNull(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is DemoTiltFollowCameraComponent followCameraComponent) {
                    return followCameraComponent;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the runtime authored scene id attached to one candidate entity when present.
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
        /// Resolves the active BEPU runtime required to synchronize updated dynamic-body velocity state.
        /// </summary>
        /// <returns>Active BEPU-backed physics runtime.</returns>
        IPhysicsBodySynchronizationRuntime3D ResolveRequiredPhysicsWorld() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial updates can run.");
            }

            IPhysicsBodySynchronizationRuntime3D physicsWorld = Core.Instance.PhysicsRuntime as IPhysicsBodySynchronizationRuntime3D;
            if (physicsWorld == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires a physics runtime that supports dynamic-body velocity synchronization.");
            }

            return physicsWorld;
        }

        /// <summary>
        /// Removes vertical influence from one camera-derived axis and normalizes the remaining planar direction.
        /// </summary>
        /// <param name="axis">Camera-derived axis to flatten onto the gameplay plane.</param>
        /// <param name="axisName">Axis label used when reporting invalid basis failures.</param>
        /// <returns>Normalized planar axis.</returns>
        static float3 ResolveFlattenedPlanarAxis(float3 axis, string axisName) {
            float3 flattenedAxis = new float3(axis.X, 0f, axis.Z);
            return NormalizePlanarOrThrow(flattenedAxis, axisName);
        }

        /// <summary>
        /// Normalizes one planar vector and throws when the vector is too small to define a stable gameplay direction.
        /// </summary>
        /// <param name="value">Planar vector to normalize.</param>
        /// <param name="vectorName">Vector label used when reporting invalid direction failures.</param>
        /// <returns>Normalized planar vector.</returns>
        static float3 NormalizePlanarOrThrow(float3 value, string vectorName) {
            double lengthSquared = (value.X * value.X) + (value.Z * value.Z);
            if (lengthSquared <= MinimumPlanarLengthSquared) {
                throw new InvalidOperationException($"DemoTiltStageComponent could not derive a valid planar {vectorName} direction from the active camera orientation.");
            }

            double inverseLength = 1d / Math.Sqrt(lengthSquared);
            return new float3((float)(value.X * inverseLength), 0f, (float)(value.Z * inverseLength));
        }

        /// <summary>
        /// Moves the current planar velocity toward the requested target velocity by the supplied acceleration-limited step.
        /// </summary>
        /// <param name="currentPlanarVelocity">Current planar velocity before steering.</param>
        /// <param name="targetPlanarVelocity">Requested planar velocity after steering.</param>
        /// <param name="maximumPlanarStep">Maximum planar speed delta that may be applied this frame.</param>
        /// <returns>Planar velocity moved toward the target without overshooting.</returns>
        static float3 MovePlanarVelocityTowardTarget(float3 currentPlanarVelocity, float3 targetPlanarVelocity, double maximumPlanarStep) {
            if (double.IsNaN(maximumPlanarStep) || double.IsInfinity(maximumPlanarStep) || maximumPlanarStep < 0d) {
                throw new ArgumentOutOfRangeException(nameof(maximumPlanarStep), "Tilt Trial planar steering requires a finite non-negative step.");
            }

            float3 velocityDelta = targetPlanarVelocity - currentPlanarVelocity;
            double deltaLengthSquared = (velocityDelta.X * velocityDelta.X) + (velocityDelta.Z * velocityDelta.Z);
            if (deltaLengthSquared <= MinimumPlanarLengthSquared || maximumPlanarStep == 0d) {
                return currentPlanarVelocity;
            }

            double deltaLength = Math.Sqrt(deltaLengthSquared);
            if (deltaLength <= maximumPlanarStep) {
                return targetPlanarVelocity;
            }

            double scale = maximumPlanarStep / deltaLength;
            return new float3(
                (float)(currentPlanarVelocity.X + (velocityDelta.X * scale)),
                0f,
                (float)(currentPlanarVelocity.Z + (velocityDelta.Z * scale)));
        }
    }
}
