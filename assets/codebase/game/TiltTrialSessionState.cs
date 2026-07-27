namespace city.game {
    /// <summary>
    /// Stores the high-level Tilt Trial session states used by gameplay overlays and progression flow.
    /// </summary>
    public enum TiltTrialSessionState {
        Start = 0,
        Playing = 1,
        Paused = 2,
        Results = 3,
        Failed = 4
    }
}
