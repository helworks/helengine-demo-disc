namespace city.game {
    /// <summary>
    /// Rotates the authored Tilt Trial course as one kinematic group so the player sphere moves through normal rigid-body contact instead of scripted translation.
    /// </summary>
    public sealed class DemoTiltStageComponent : UpdateComponent {
        /// <summary>
        /// Normalized analog threshold used to ignore left-stick drift.
        /// </summary>
        const double GamepadDeadzone = 0.18d;

        /// <summary>
        /// Gets or sets the maximum absolute stage tilt applied on each driven axis in radians.
        /// </summary>
        public float MaxTiltRadians { get; set; }

        /// <summary>
        /// Gets or sets the angular response speed used to ease the stage toward the requested tilt target.
        /// </summary>
        public float TiltResponseRadiansPerSecond { get; set; }

        /// <summary>
        /// Stores the direct stage child entities that carry kinematic rigid bodies.
        /// </summary>
        List<Entity> StagePieceEntities;

        /// <summary>
        /// Stores the authored local positions for the tracked stage pieces before runtime tilt is applied.
        /// </summary>
        List<float3> RestLocalPositions;

        /// <summary>
        /// Stores the authored local orientations for the tracked stage pieces before runtime tilt is applied.
        /// </summary>
        List<float4> RestLocalOrientations;

        /// <summary>
        /// Stores the current stage pitch in radians.
        /// </summary>
        float CurrentPitchRadians;

        /// <summary>
        /// Stores the current stage roll in radians.
        /// </summary>
        float CurrentRollRadians;

        /// <summary>
        /// Tracks whether the supported kinematic stage pieces have already been gathered from the parent entity.
        /// </summary>
        bool IsStageBound;

        /// <summary>
        /// Initializes one Tilt Trial stage controller with a modest responsive tilt envelope.
        /// </summary>
        public DemoTiltStageComponent() {
            MaxTiltRadians = 0.2617994f;
            TiltResponseRadiansPerSecond = 1.75f;
            StagePieceEntities = new List<Entity>();
            RestLocalPositions = new List<float3>();
            RestLocalOrientations = new List<float4>();
        }

        /// <summary>
        /// Advances the requested stage tilt from player input and pushes the resulting kinematic transforms back into BEPU before the next simulation step.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires an attached stage root entity.");
            }

            EnsureStageBound();

            Core core = Core.Instance ?? throw new InvalidOperationException("A core instance must exist before Tilt Trial stage updates can run.");
            double elapsedSeconds = core.FrameDeltaSeconds;
            double pitchInput = ResolvePitchInput(core.Input);
            double rollInput = ResolveRollInput(core.Input);
            float desiredPitchRadians = (float)(-pitchInput * MaxTiltRadians);
            float desiredRollRadians = (float)(-rollInput * MaxTiltRadians);
            CurrentPitchRadians = MoveToward(CurrentPitchRadians, desiredPitchRadians, TiltResponseRadiansPerSecond * (float)elapsedSeconds);
            CurrentRollRadians = MoveToward(CurrentRollRadians, desiredRollRadians, TiltResponseRadiansPerSecond * (float)elapsedSeconds);
            ApplyTiltToTrackedPieces(ResolveRequiredPhysicsWorld());
        }

        /// <summary>
        /// Gathers the direct child entities that participate in the Tilt Trial course as kinematic support geometry.
        /// </summary>
        void EnsureStageBound() {
            if (IsStageBound) {
                return;
            } else if (Parent.Children == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires an initialized child-entity list on the stage root.");
            }

            StagePieceEntities.Clear();
            RestLocalPositions.Clear();
            RestLocalOrientations.Clear();
            for (int childIndex = 0; childIndex < Parent.Children.Count; childIndex++) {
                Entity child = Parent.Children[childIndex];
                if (!IsKinematicStagePiece(child)) {
                    continue;
                }

                StagePieceEntities.Add(child);
                RestLocalPositions.Add(child.LocalPosition);
                RestLocalOrientations.Add(child.LocalOrientation);
            }

            if (StagePieceEntities.Count < 1) {
                throw new InvalidOperationException("DemoTiltStageComponent requires at least one direct kinematic stage child.");
            }

            IsStageBound = true;
        }

        /// <summary>
        /// Returns whether one direct stage child is driven by the Tilt Trial controller.
        /// </summary>
        /// <param name="entity">Direct stage child under evaluation.</param>
        /// <returns>True when the child carries a rigid body and therefore participates in stage motion.</returns>
        bool IsKinematicStagePiece(Entity entity) {
            if (entity == null || entity.Components == null) {
                return false;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is RigidBody3DComponent) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies the resolved stage tilt to every tracked kinematic piece and synchronizes the resulting transforms into the BEPU simulation.
        /// </summary>
        /// <param name="physicsWorld">Active BEPU world receiving the updated kinematic poses.</param>
        void ApplyTiltToTrackedPieces(IPhysicsBodySynchronizationRuntime3D physicsWorld) {
            if (physicsWorld == null) {
                throw new ArgumentNullException(nameof(physicsWorld));
            }

            float4 stageRotation;
            float4.CreateFromYawPitchRoll(0f, CurrentPitchRadians, CurrentRollRadians, out stageRotation);
            stageRotation.Normalize();

            for (int pieceIndex = 0; pieceIndex < StagePieceEntities.Count; pieceIndex++) {
                Entity stagePiece = StagePieceEntities[pieceIndex];
                stagePiece.LocalPosition = float4.RotateVector(RestLocalPositions[pieceIndex], stageRotation);

                float4 desiredOrientation;
                float4 restLocalOrientation = RestLocalOrientations[pieceIndex];
                float4.Concatenate(ref restLocalOrientation, ref stageRotation, out desiredOrientation);
                desiredOrientation.Normalize();
                stagePiece.LocalOrientation = desiredOrientation;

                FindRequiredRigidBodyComponent(stagePiece);
                physicsWorld.SynchronizeKinematicBody(stagePiece);
            }
        }

        /// <summary>
        /// Resolves normalized requested stage pitch from keyboard and left-stick vertical input.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
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
                gamepadPitch += -NormalizeStickAxis(gamepadState.LeftStickY);
            }

            return Math.Clamp(keyboardPitch + gamepadPitch, -1d, 1d);
        }

        /// <summary>
        /// Resolves normalized requested stage roll from keyboard and left-stick horizontal input.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>Normalized roll input in the range [-1, 1].</returns>
        double ResolveRollInput(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            double keyboardRoll = 0d;
            if (inputSystem.IsKeyDown(Keys.A)) {
                keyboardRoll -= 1d;
            }
            if (inputSystem.IsKeyDown(Keys.D)) {
                keyboardRoll += 1d;
            }

            InputGamepadState gamepadState = inputSystem.GetGamepadState(0);
            double gamepadRoll = 0d;
            if (gamepadState.Connected) {
                gamepadRoll += NormalizeStickAxis(gamepadState.LeftStickX);
            }

            return Math.Clamp(keyboardRoll + gamepadRoll, -1d, 1d);
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
        /// Resolves the authored rigid-body component attached to one tracked stage piece.
        /// </summary>
        /// <param name="entity">Tracked stage piece whose rigid body should be returned.</param>
        /// <returns>Attached rigid-body component.</returns>
        RigidBody3DComponent FindRequiredRigidBodyComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Tilt Trial stage pieces must expose initialized component collections.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is RigidBody3DComponent rigidBody) {
                    return rigidBody;
                }
            }

            throw new InvalidOperationException("Tilt Trial stage pieces must include a RigidBody3DComponent.");
        }

        /// <summary>
        /// Resolves the active BEPU world required to synchronize updated kinematic stage transforms.
        /// </summary>
        /// <returns>Active BEPU-backed physics world.</returns>
        IPhysicsBodySynchronizationRuntime3D ResolveRequiredPhysicsWorld() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial stage updates can run.");
            }

            IPhysicsBodySynchronizationRuntime3D physicsWorld = Core.Instance.PhysicsRuntime as IPhysicsBodySynchronizationRuntime3D;
            if (physicsWorld == null) {
                throw new InvalidOperationException("DemoTiltStageComponent requires a physics runtime that supports kinematic body synchronization.");
            }

            return physicsWorld;
        }

        /// <summary>
        /// Moves one angular value toward a target by the supplied maximum step without overshooting.
        /// </summary>
        /// <param name="currentValue">Current value.</param>
        /// <param name="targetValue">Requested target value.</param>
        /// <param name="maximumStep">Maximum absolute movement allowed this frame.</param>
        /// <returns>New value moved toward the target.</returns>
        float MoveToward(float currentValue, float targetValue, float maximumStep) {
            if (maximumStep <= 0f) {
                return currentValue;
            }

            float delta = targetValue - currentValue;
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
