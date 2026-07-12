using helengine;

namespace city.game {
    /// <summary>
    /// Stores one authored box bounds definition consumed only by the Tilt Trial physics debug overlay.
    /// </summary>
    public sealed class TiltTrialPhysicsDebugBoxBoundsComponent : Component {
        /// <summary>
        /// Gets or sets the full local-space box size.
        /// </summary>
        public float3 Size { get; set; } = float3.One;
    }

    /// <summary>
    /// Stores one authored sphere bounds definition consumed only by the Tilt Trial physics debug overlay.
    /// </summary>
    public sealed class TiltTrialPhysicsDebugSphereBoundsComponent : Component {
        /// <summary>
        /// Gets or sets the authored local-space sphere radius.
        /// </summary>
        public float Radius { get; set; } = 0.5f;
    }
}
