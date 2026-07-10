namespace city.game {
    /// <summary>
    /// Enumerates the high-level runtime states used by the Zombislayer gameplay session.
    /// </summary>
    public enum ZombislayerSessionState {
        /// <summary>
        /// Gameplay simulation and first-person input are active.
        /// </summary>
        Playing,

        /// <summary>
        /// Gameplay simulation is paused and the pause overlay is visible.
        /// </summary>
        Paused
    }
}
