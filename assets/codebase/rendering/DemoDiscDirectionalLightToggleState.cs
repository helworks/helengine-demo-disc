namespace city.rendering {
    /// <summary>
    /// Stores one authored directional light state so the demo-disc light cycle can restore the original intensity and shadow behavior for any active color state.
    /// </summary>
    public sealed class DemoDiscDirectionalLightToggleState {
        /// <summary>
        /// Gets or sets the light controlled by the toggle state.
        /// </summary>
        public DirectionalLightComponent Light { get; set; }

        /// <summary>
        /// Gets or sets the authored intensity that should be restored whenever the light cycle is in a non-off state.
        /// </summary>
        public float AuthoredIntensity { get; set; }

        /// <summary>
        /// Gets or sets whether the authored light originally had shadows enabled before the cycle forced the off state.
        /// </summary>
        public bool AuthoredShadowsEnabled { get; set; }
    }
}
