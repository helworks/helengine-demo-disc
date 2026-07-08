namespace city.game {
    /// <summary>
    /// Resets the playable Tilt Trial sphere back to its authored spawn pose after it falls below the supported course bounds.
    /// </summary>
    public sealed class DemoTiltBallResetComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the local-space spawn position reapplied when the tracked ball falls out of bounds.
        /// </summary>
        public float3 SpawnPosition { get; set; }

        /// <summary>
        /// Gets or sets the local-space spawn orientation reapplied when the tracked ball falls out of bounds.
        /// </summary>
        public float4 SpawnOrientation { get; set; }

        /// <summary>
        /// Gets or sets the world-space height below which the playable ball is teleported back to its spawn pose.
        /// </summary>
        public float ResetHeight { get; set; }

        /// <summary>
        /// Initializes one Tilt Trial ball-reset controller with a conservative out-of-bounds threshold.
        /// </summary>
        public DemoTiltBallResetComponent() {
            SpawnPosition = new float3(0f, 1.2f, -7f);
            SpawnOrientation = float4.Identity;
            ResetHeight = -12f;
        }

        /// <summary>
        /// Checks whether the owning ball fell below the reset threshold and restores the authored spawn state when needed.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoTiltBallResetComponent requires an attached playable ball entity.");
            }

            if (Parent.Position.Y >= ResetHeight) {
                return;
            }

            ResetTrackedBall();
        }

        /// <summary>
        /// Restores the owning ball to its authored spawn pose and pushes the updated transform back into the live BEPU body.
        /// </summary>
        void ResetTrackedBall() {
            IPhysicsBodySynchronizationRuntime3D physicsWorld = ResolveRequiredPhysicsWorld();
            Parent.LocalPosition = SpawnPosition;
            Parent.LocalOrientation = SpawnOrientation;
            RigidBody3DComponent rigidBody = FindRequiredRigidBodyComponent();
            rigidBody.SetLinearVelocity(float3.Zero);
            rigidBody.SetAngularVelocity(float3.Zero);
            physicsWorld.SynchronizeDynamicBody(Parent);
        }

        /// <summary>
        /// Resolves the authored rigid-body component attached to the playable ball.
        /// </summary>
        /// <returns>Attached rigid-body component.</returns>
        RigidBody3DComponent FindRequiredRigidBodyComponent() {
            if (Parent.Components == null) {
                throw new InvalidOperationException("DemoTiltBallResetComponent requires an initialized component list on the playable ball.");
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is RigidBody3DComponent rigidBody) {
                    return rigidBody;
                }
            }

            throw new InvalidOperationException("DemoTiltBallResetComponent requires a RigidBody3DComponent on the playable ball.");
        }

        /// <summary>
        /// Resolves the active BEPU world required to synchronize teleport updates back into the physics simulation.
        /// </summary>
        /// <returns>Active BEPU-backed physics world.</returns>
        IPhysicsBodySynchronizationRuntime3D ResolveRequiredPhysicsWorld() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial ball resets can run.");
            }

            IPhysicsBodySynchronizationRuntime3D physicsWorld = Core.Instance.PhysicsRuntime as IPhysicsBodySynchronizationRuntime3D;
            if (physicsWorld == null) {
                throw new InvalidOperationException("DemoTiltBallResetComponent requires a physics runtime that supports dynamic body synchronization.");
            }

            return physicsWorld;
        }
    }
}
