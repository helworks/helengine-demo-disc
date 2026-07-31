namespace city.menu.tools.tests {
    /// <summary>
    /// Protects the standard menu logo from being submitted beneath its panel surface.
    /// </summary>
    public sealed class DemoDiscMenuOverlayOrderSourceTests {
        /// <summary>
        /// Ensures the decorative logo draws after the panel background that occupies render order thirty.
        /// </summary>
        [Fact]
        public void Standard_menu_logo_draws_after_its_panel_surface() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscStandardMainMenuSceneFactory.cs");
            int overlayMethodStart = source.IndexOf("void CreateOverlayImageEntity(", StringComparison.Ordinal);
            int platformInfoMethodStart = source.IndexOf("void CreatePlatformInfoOverlayEntity(", overlayMethodStart, StringComparison.Ordinal);

            Assert.True(overlayMethodStart >= 0);
            Assert.True(platformInfoMethodStart > overlayMethodStart);
            string overlayMethodSource = source[overlayMethodStart..platformInfoMethodStart];

            Assert.Contains("RenderOrder2D = 31,", overlayMethodSource, StringComparison.Ordinal);
        }
    }
}
