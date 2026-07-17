namespace city.game {
    /// <summary>
    /// Identifies pointer actions exposed by the handheld Tilt Trial level selector.
    /// </summary>
    public enum TiltTrialLevelSelectAction {
        /// <summary>
        /// Opens the details view for the supplied stage index.
        /// </summary>
        SelectStage = 0,

        /// <summary>
        /// Returns from the stage details view to the stage list.
        /// </summary>
        BackToStages = 1,

        /// <summary>
        /// Starts the currently selected stage.
        /// </summary>
        PlaySelectedStage = 2
    }
}
