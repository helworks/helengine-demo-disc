namespace city.rendering.tools {
    /// <summary>
    /// Centralizes the reusable console camera/light instruction Blueprint path and target platforms.
    /// </summary>
    public static class ConsoleCameraLightInstructionsAssetCatalog {
        /// <summary>
        /// Stable project-relative path for the shared console instruction Blueprint.
        /// </summary>
        public const string ConsoleCameraLightInstructionsBlueprintRelativePath = "blueprints/ui/ConsoleCameraLightInstructions.hblueprint";

        /// <summary>
        /// Console platform ids that receive the shared Blueprint instance.
        /// </summary>
        public static readonly string[] ConsolePlatformIds = ["ps2", "gamecube", "wii", "switch", "wiiu"];
    }
}
