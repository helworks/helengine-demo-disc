namespace city.game {
    /// <summary>
    /// Identifies one semantic action available from the Tilt Play front-door menu.
    /// </summary>
    public enum TiltPlayMenuAction {
        /// <summary>
        /// Opens the existing level selector.
        /// </summary>
        Play = 0,

        /// <summary>
        /// Opens the temporary settings placeholder.
        /// </summary>
        Options = 1,

        /// <summary>
        /// Returns to the Demo Disc main menu.
        /// </summary>
        BackToDemoDisc = 2,

        /// <summary>
        /// Returns from a Tilt Play submenu to the title.
        /// </summary>
        Back = 3
    }
}
