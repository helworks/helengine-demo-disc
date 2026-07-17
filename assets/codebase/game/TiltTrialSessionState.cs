namespace city.game {
    /// <summary>
    /// Stores the high-level Tilt Trial session states used by gameplay overlays and progression flow.
    /// </summary>
    public enum TiltTrialSessionState {
        Playing = 0,
        Paused = 1,
        Results = 2,
        Failed = 3
    }
}
