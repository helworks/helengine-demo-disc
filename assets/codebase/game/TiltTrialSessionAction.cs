namespace city.game {
    /// <summary>
    /// Identifies presentation-independent commands that can be sent to an active Tilt Trial session.
    /// </summary>
    public enum TiltTrialSessionAction {
        /// <summary>
        /// Toggles the active gameplay session between playing and paused states.
        /// </summary>
        TogglePause = 0,

        /// <summary>
        /// Restarts the currently active level.
        /// </summary>
        Retry = 1,

        /// <summary>
        /// Advances to the next level or returns to level select after the final level.
        /// </summary>
        Next = 2,

        /// <summary>
        /// Returns to the Tilt Trial level selector.
        /// </summary>
        LevelSelect = 3,

        /// <summary>
        /// Moves the result or failure selection backward.
        /// </summary>
        NavigatePrevious = 4,

        /// <summary>
        /// Moves the result or failure selection forward.
        /// </summary>
        NavigateNext = 5,

        /// <summary>
        /// Activates the currently selected result or failure option.
        /// </summary>
        Accept = 6
    }
}
