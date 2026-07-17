namespace city.game {
    /// <summary>
    /// Identifies a gameplay entity by a stable serialized role that survives scene and Blueprint boundaries.
    /// </summary>
    public sealed class TiltTrialEntityRoleComponent : Component {
        /// <summary>
        /// Gets or sets the stable gameplay role carried by the entity.
        /// </summary>
        public string Role { get; set; }
    }
}
