using helengine;

namespace city.game {
    /// <summary>
    /// Swings a pendulum hammer around its pivot through one arc and back so the kinematic hammer head pushes the player sphere.
    /// </summary>
    public sealed class TiltTrialPendulumHammerComponent : UpdateComponent {
        float4 BaseLocalOrientation;
        bool HasCapturedBaseLocalOrientation;
        float ElapsedSeconds;
        Entity HeadBodyEntity;
        RigidBody3DComponent HeadRigidBody;

        /// <summary>
        /// Gets or sets the full swing arc in degrees; the hammer sweeps half the arc to each side of its authored orientation.
        /// </summary>
        public float SwingArcDegrees { get; set; } = 180f;

        /// <summary>
        /// Gets or sets the duration of one full swing cycle (side to side and back) in seconds.
        /// </summary>
        public float SwingPeriodSeconds { get; set; } = 2.6f;

        /// <summary>
        /// Gets or sets the swing phase offset in radians so multiple hammers can run out of step.
        /// </summary>
        public float SwingPhaseRadians { get; set; }

        /// <summary>
        /// Evaluates the pendulum swing angle for one elapsed time sample.
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed loop time in seconds.</param>
        /// <param name="swingArcDegrees">Full swing arc in degrees.</param>
        /// <param name="swingPeriodSeconds">Duration of one full swing cycle in seconds.</param>
        /// <param name="swingPhaseRadians">Swing phase offset in radians.</param>
        /// <returns>Signed swing angle in radians around the pivot axis.</returns>
        public static float ResolveSwingAngleRadians(
            float elapsedSeconds,
            float swingArcDegrees,
            float swingPeriodSeconds,
            float swingPhaseRadians) {
            if (swingPeriodSeconds <= 0f) {
                return 0f;
            }

            float halfArcRadians = swingArcDegrees * (float)(Math.PI / 180d) * 0.5f;
            float angularSpeed = (float)(Math.PI * 2d) / swingPeriodSeconds;
            return MathF.Cos(swingPhaseRadians + elapsedSeconds * angularSpeed) * halfArcRadians;
        }

        /// <summary>
        /// Evaluates the pendulum swing angular speed for one elapsed time sample.
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed loop time in seconds.</param>
        /// <param name="swingArcDegrees">Full swing arc in degrees.</param>
        /// <param name="swingPeriodSeconds">Duration of one full swing cycle in seconds.</param>
        /// <param name="swingPhaseRadians">Swing phase offset in radians.</param>
        /// <returns>Signed swing angular speed in radians per second around the pivot axis.</returns>
        public static float ResolveSwingAngularSpeedRadians(
            float elapsedSeconds,
            float swingArcDegrees,
            float swingPeriodSeconds,
            float swingPhaseRadians) {
            if (swingPeriodSeconds <= 0f) {
                return 0f;
            }

            float halfArcRadians = swingArcDegrees * (float)(Math.PI / 180d) * 0.5f;
            float angularSpeed = (float)(Math.PI * 2d) / swingPeriodSeconds;
            return -MathF.Sin(swingPhaseRadians + elapsedSeconds * angularSpeed) * halfArcRadians * angularSpeed;
        }

        /// <summary>
        /// Advances the pendulum swing by rotating the owning entity around its local X axis, then feeds the kinematic head body its exact pose and velocities so contact impulses push the player correctly.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                return;
            }

            if (!HasCapturedBaseLocalOrientation) {
                BaseLocalOrientation = Parent.LocalOrientation;
                HasCapturedBaseLocalOrientation = true;
            }

            double frameDeltaSeconds = Core.Instance.FrameDeltaSeconds;
            if (double.IsNaN(frameDeltaSeconds) || double.IsInfinity(frameDeltaSeconds) || frameDeltaSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(Core.Instance.FrameDeltaSeconds), "Pendulum hammer motion requires a finite non-negative frame delta.");
            } else if (frameDeltaSeconds == 0d) {
                return;
            }

            ElapsedSeconds += (float)frameDeltaSeconds;

            float swingAngleRadians = ResolveSwingAngleRadians(ElapsedSeconds, SwingArcDegrees, SwingPeriodSeconds, SwingPhaseRadians);
            float3 axis = new float3(1f, 0f, 0f);
            float4.CreateFromAxisAngle(ref axis, swingAngleRadians, out float4 swingRotation);

            float4 baseOrientation = BaseLocalOrientation;
            float4.Concatenate(ref swingRotation, ref baseOrientation, out float4 orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;

            SynchronizeHeadBody();
        }

        /// <summary>
        /// Pushes the swung head pose and its analytic velocities into the bound kinematic physics body.
        /// </summary>
        void SynchronizeHeadBody() {
            if (HeadRigidBody == null) {
                FindRequiredHeadBody();
            }

            float swingAngularSpeedRadians = ResolveSwingAngularSpeedRadians(ElapsedSeconds, SwingArcDegrees, SwingPeriodSeconds, SwingPhaseRadians);
            float3 worldHingeAxis = float4.RotateVector(new float3(1f, 0f, 0f), Parent.Orientation);
            float3 angularVelocity = worldHingeAxis * swingAngularSpeedRadians;
            HeadRigidBody.AngularVelocity = angularVelocity;
            HeadRigidBody.LinearVelocity = float3.Cross(angularVelocity, HeadBodyEntity.Position - Parent.Position);

            if (Core.Instance.PhysicsSimulationIsPaused) {
                return;
            }
            if (Core.Instance.PhysicsRuntime is IPhysicsBodySynchronizationRuntime3D synchronizationRuntime) {
                synchronizationRuntime.SynchronizeKinematicBody(HeadBodyEntity);
            }
        }

        /// <summary>
        /// Finds and caches the kinematic head body entity beneath the hammer pivot.
        /// </summary>
        void FindRequiredHeadBody() {
            if (Parent.Children != null) {
                for (int childIndex = 0; childIndex < Parent.Children.Count; childIndex++) {
                    Entity child = Parent.Children[childIndex];
                    if (child == null || child.Components == null) {
                        continue;
                    }

                    for (int componentIndex = 0; componentIndex < child.Components.Count; componentIndex++) {
                        if (child.Components[componentIndex] is RigidBody3DComponent rigidBody && rigidBody.BodyKind == BodyKind3D.Kinematic) {
                            HeadBodyEntity = child;
                            HeadRigidBody = rigidBody;
                            return;
                        }
                    }
                }
            }

            throw new InvalidOperationException("TiltTrialPendulumHammerComponent requires a child entity carrying a kinematic RigidBody3DComponent for the hammer head.");
        }
    }
}
