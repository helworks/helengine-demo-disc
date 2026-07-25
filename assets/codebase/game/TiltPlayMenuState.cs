namespace city.game {
    /// <summary>
    /// Identifies the visible high-level panel in the Tilt Play front-door menu.
    /// </summary>
    public enum TiltPlayMenuState {
        /// <summary>
        /// Shows the title and its Play, Options, and Demo Disc actions.
        /// </summary>
        Title = 0,

        /// <summary>
        /// Shows the temporary settings placeholder panel.
        /// </summary>
        Options = 1,

        /// <summary>
        /// Shows the existing Tilt Trial level selector.
        /// </summary>
        LevelSelect = 2
    }
}
