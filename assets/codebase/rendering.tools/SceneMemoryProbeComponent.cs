namespace city.rendering.tools {
    /// <summary>
    /// Placeholder authored component that preserves scene-memory probe scene generation after the original runtime probe system was removed.
    /// </summary>
    public sealed class SceneMemoryProbeComponent : Component {
        /// <summary>
        /// Gets or sets the stable probe name written into generated data.
        /// </summary>
        public string ProbeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the probe should begin automatically when the scene loads.
        /// </summary>
        public bool StartAutomatically { get; set; }

        /// <summary>
        /// Gets or sets the startup delay, in seconds, before the probe begins.
        /// </summary>
        public double InitialDelaySeconds { get; set; }

        /// <summary>
        /// Gets or sets whether the probe should loop after the final step.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Gets or sets the authored step sequence consumed by the placeholder probe component.
        /// </summary>
        public SceneMemoryProbeStep[] Steps { get; set; } = Array.Empty<SceneMemoryProbeStep>();
    }
}
