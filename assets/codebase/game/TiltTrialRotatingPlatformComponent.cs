using helengine;

namespace city.game {
    /// <summary>
    /// Spins a platform around its local Y axis at constant speed and feeds the kinematic platform body its exact pose and angular velocity so surface friction carries the player.
    /// </summary>
    public sealed class TiltTrialRotatingPlatformComponent : UpdateComponent {
        float4 BaseLocalOrientation;
        bool HasCapturedBaseLocalOrientation;
        float ElapsedSeconds;
        RigidBody3DComponent PlatformRigidBody;

        /// <summary>
        /// Gets or sets the spin speed in degrees per second; negative values reverse the spin direction.
        /// </summary>
        public float RotationSpeedDegreesPerSecond { get; set; } = 45f;

        /// <summary>
        /// Evaluates the wrapped platform spin angle for one elapsed time sample.
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed loop time in seconds.</param>
        /// <param name="rotationSpeedDegreesPerSecond">Spin speed in degrees per second.</param>
        /// <returns>Signed spin angle in radians wrapped to one full turn.</returns>
        public static float ResolveSpinAngleRadians(float elapsedSeconds, float rotationSpeedDegreesPerSecond) {
            float angleRadians = elapsedSeconds * rotationSpeedDegreesPerSecond * (float)(Math.PI / 180d);
            return angleRadians % (float)(Math.PI * 2d);
        }

        /// <summary>
        /// Evaluates the constant platform spin angular speed.
        /// </summary>
        /// <param name="rotationSpeedDegreesPerSecond">Spin speed in degrees per second.</param>
        /// <returns>Signed spin angular speed in radians per second around the local Y axis.</returns>
        public static float ResolveSpinAngularSpeedRadians(float rotationSpeedDegreesPerSecond) {
            return rotationSpeedDegreesPerSecond * (float)(Math.PI / 180d);
        }

        /// <summary>
        /// Advances the platform spin by rotating the owning entity around its local Y axis, then synchronizes the bound kinematic body.
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
                throw new ArgumentOutOfRangeException(nameof(Core.Instance.FrameDeltaSeconds), "Rotating platform motion requires a finite non-negative frame delta.");
            } else if (frameDeltaSeconds == 0d) {
                return;
            }

            ElapsedSeconds += (float)frameDeltaSeconds;

            float spinAngleRadians = ResolveSpinAngleRadians(ElapsedSeconds, RotationSpeedDegreesPerSecond);
            float3 axis = new float3(0f, 1f, 0f);
            float4.CreateFromAxisAngle(ref axis, spinAngleRadians, out float4 spinRotation);

            float4 baseOrientation = BaseLocalOrientation;
            float4.Concatenate(ref spinRotation, ref baseOrientation, out float4 orientation);
            orientation.Normalize();
            Parent.LocalOrientation = orientation;

            SynchronizePlatformBody();
        }

        /// <summary>
        /// Pushes the spun platform pose and its analytic angular velocity into the bound kinematic physics body.
        /// </summary>
        void SynchronizePlatformBody() {
            if (PlatformRigidBody == null) {
                PlatformRigidBody = FindRequiredPlatformRigidBody();
            }

            float3 worldSpinAxis = float4.RotateVector(new float3(0f, 1f, 0f), Parent.Orientation);
            PlatformRigidBody.AngularVelocity = worldSpinAxis * ResolveSpinAngularSpeedRadians(RotationSpeedDegreesPerSecond);
            PlatformRigidBody.LinearVelocity = float3.Zero;

            if (Core.Instance.PhysicsSimulationIsPaused) {
                return;
            }
            if (Core.Instance.PhysicsRuntime is IPhysicsBodySynchronizationRuntime3D synchronizationRuntime) {
                synchronizationRuntime.SynchronizeKinematicBody(Parent);
            }
        }

        /// <summary>
        /// Finds the kinematic rigid body attached to the platform entity.
        /// </summary>
        /// <returns>Attached kinematic rigid body.</returns>
        RigidBody3DComponent FindRequiredPlatformRigidBody() {
            if (Parent.Components != null) {
                for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                    if (Parent.Components[componentIndex] is RigidBody3DComponent rigidBody && rigidBody.BodyKind == BodyKind3D.Kinematic) {
                        return rigidBody;
                    }
                }
            }

            throw new InvalidOperationException("TiltTrialRotatingPlatformComponent requires a kinematic RigidBody3DComponent on the platform entity.");
        }
    }
}
