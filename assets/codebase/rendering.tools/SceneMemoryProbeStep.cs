namespace city.rendering.tools {
    /// <summary>
    /// Stores one authored step for the scene-memory probe placeholder component.
    /// </summary>
    public sealed class SceneMemoryProbeStep {
        /// <summary>
        /// Gets or sets the action executed by this step.
        /// </summary>
        public SceneMemoryProbeActionKind ActionKind { get; set; }

        /// <summary>
        /// Gets or sets the scene id targeted by the step when the action loads a scene.
        /// </summary>
        public string SceneId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the wait duration, in seconds, for this step.
        /// </summary>
        public double DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the stable diagnostic label emitted for this step.
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }
}
