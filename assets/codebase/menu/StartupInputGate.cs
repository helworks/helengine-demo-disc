namespace city.menu {
    /// <summary>
    /// Controls whether the initial boot sequence currently owns Demo Disc menu input.
    /// </summary>
    public static class StartupInputGate {
        /// <summary>
        /// Gets whether boot-time content currently blocks menu navigation input.
        /// </summary>
        public static bool IsBlocked { get; private set; }

        /// <summary>
        /// Gives the initial boot sequence exclusive ownership of menu input.
        /// </summary>
        public static void Acquire() {
            IsBlocked = true;
        }

        /// <summary>
        /// Returns menu input ownership after the initial boot sequence finishes.
        /// </summary>
        public static void Release() {
            IsBlocked = false;
        }
    }
}
