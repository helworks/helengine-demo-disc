namespace city.game {
    /// <summary>
    /// Identifies one presentation entity by a stable serialized HUD role.
    /// </summary>
    public sealed class TiltTrialPresentationRoleComponent : Component {
        /// <summary>
        /// Gets or sets the stable presentation role carried by the entity.
        /// </summary>
        public string Role { get; set; }
    }
}
