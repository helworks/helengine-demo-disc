namespace city.rendering.tools {
    /// <summary>
    /// Enumerates the authored actions supported by the scene-memory probe placeholder component.
    /// </summary>
    public enum SceneMemoryProbeActionKind {
        /// <summary>
        /// Loads one scene as the sole active scene.
        /// </summary>
        LoadSceneSingle,

        /// <summary>
        /// Waits for a fixed duration without changing the active scene.
        /// </summary>
        Wait
    }
}
