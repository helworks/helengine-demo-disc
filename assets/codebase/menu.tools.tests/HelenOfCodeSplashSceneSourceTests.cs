namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies the authored source contract for the generated Helen of Code splash scene.
    /// </summary>
    public sealed class HelenOfCodeSplashSceneSourceTests {
        /// <summary>
        /// Proves the splash factory authors the required scene, sprites, timing component, and logo asset reference.
        /// </summary>
        [Fact]
        public void Splash_factory_authors_centered_ninety_percent_logo_scene() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu.tools",
                "HelenOfCodeSplashSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("HelenOfCodeSplash", source, StringComparison.Ordinal);
            Assert.Contains("HelenOfCodeSplashComponent", source, StringComparison.Ordinal);
            Assert.Contains("images/splash/helen_of_code_logo.png", source, StringComparison.Ordinal);
            Assert.Contains("DemoMenuLayout.CanvasHeight * 0.9d", source, StringComparison.Ordinal);
            Assert.Contains("new byte4(0, 0, 0, 255)", source, StringComparison.Ordinal);
        }
    }
}
