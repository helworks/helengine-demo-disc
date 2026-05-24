namespace city.rendering {
    /// <summary>
    /// Stores one authored directional light state so the demo-disc toggle can restore it after lights were disabled.
    /// </summary>
    public sealed class DemoDiscDirectionalLightToggleState {
        /// <summary>
        /// Gets or sets the light controlled by the toggle state.
        /// </summary>
        public DirectionalLightComponent Light { get; set; }

        /// <summary>
        /// Gets or sets the authored intensity that should be restored when lighting is enabled again.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets whether the authored light originally had shadows enabled.
        /// </summary>
        public bool ShadowsEnabled { get; set; }
    }
}
