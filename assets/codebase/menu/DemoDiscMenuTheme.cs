namespace city.menu {
    /// <summary>
    /// Stores reusable colors and font paths for the first-pass demo-disc menu.
    /// </summary>
    public sealed class DemoDiscMenuTheme {
        /// <summary>
        /// Gets the packaged title font path.
        /// </summary>
        public string TitleFontPath => "Fonts/DemoDiscTitle.hefont";

        /// <summary>
        /// Gets the packaged body font path.
        /// </summary>
        public string BodyFontPath => "Fonts/DemoDiscBody.hefont";

        /// <summary>
        /// Gets the primary lilac background color.
        /// </summary>
        public byte4 BackgroundColor => new byte4(30, 17, 41, 255);

        /// <summary>
        /// Gets the panel surface color.
        /// </summary>
        public byte4 SurfaceColor => new byte4(60, 41, 76, 232);

        /// <summary>
        /// Gets the panel border color.
        /// </summary>
        public byte4 SurfaceBorderColor => new byte4(135, 94, 163, 255);


        /// <summary>
        /// Gets the primary accent color.
        /// </summary>
        public byte4 AccentColor => new byte4(201, 147, 255, 255);

        /// <summary>
        /// Gets the secondary accent color.
        /// </summary>
        public byte4 AccentSecondaryColor => new byte4(118, 219, 209, 255);

        /// <summary>
        /// Gets the primary text color.
        /// </summary>
        public byte4 TextColor => new byte4(249, 243, 255, 255);

        /// <summary>
        /// Gets the muted text color.
        /// </summary>
        public byte4 MutedTextColor => new byte4(211, 198, 228, 255);
    }
}
