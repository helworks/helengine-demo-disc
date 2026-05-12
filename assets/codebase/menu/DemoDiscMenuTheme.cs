namespace city.menu {
    /// <summary>
    /// Stores reusable colors, fonts, and decorative artwork paths for the demo-disc menu.
    /// </summary>
    public sealed class DemoDiscMenuTheme {
        /// <summary>
        /// Gets the authored title font path.
        /// </summary>
        public string TitleFontPath => "Fonts/DemoDiscTitle.ttf";

        /// <summary>
        /// Gets the authored body font path.
        /// </summary>
        public string BodyFontPath => "Fonts/DemoDiscBody.ttf";

        /// <summary>
        /// Gets the decorative logo texture path.
        /// </summary>
        public string LogoTexturePath => "Images/Menu/helengine-logo.png";

        /// <summary>
        /// Gets the decorative logo width in authored canvas pixels.
        /// </summary>
        public int LogoWidth => 220;

        /// <summary>
        /// Gets the decorative logo height in authored canvas pixels.
        /// </summary>
        public int LogoHeight => 220;

        /// <summary>
        /// Gets the decorative logo top margin in authored canvas pixels.
        /// </summary>
        public int LogoTopMargin => 36;

        /// <summary>
        /// Gets the decorative logo right margin in authored canvas pixels.
        /// </summary>
        public int LogoRightMargin => 44;

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
