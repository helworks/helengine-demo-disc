namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies the timing contract used by the initial Helen of Code splash transition.
    /// </summary>
    public sealed class HelenOfCodeSplashComponentTests {
        /// <summary>
        /// Proves the splash reaches full opacity at the end of its fade-in period.
        /// </summary>
        [Fact]
        public void Splash_phase_starts_fully_transparent_and_fades_to_opaque() {
            HelenOfCodeSplashComponent component = new HelenOfCodeSplashComponent();

            Assert.Equal(0, component.ResolveAlphaForElapsedSeconds(0d));
            Assert.Equal(255, component.ResolveAlphaForElapsedSeconds(0.75d));
        }

        /// <summary>
        /// Proves the splash remains opaque during its hold and reaches transparency at the end.
        /// </summary>
        [Fact]
        public void Splash_phase_remains_opaque_during_hold_and_fades_to_transparent() {
            HelenOfCodeSplashComponent component = new HelenOfCodeSplashComponent();

            Assert.Equal(255, component.ResolveAlphaForElapsedSeconds(3.0d));
            Assert.Equal(0, component.ResolveAlphaForElapsedSeconds(4.5d));
        }

        /// <summary>
        /// Verifies the runtime component requests additive menu loading and removes only its splash scene.
        /// </summary>
        [Fact]
        public void Splash_runtime_source_uses_additive_menu_loading_and_self_unload() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu",
                "HelenOfCodeSplashComponent.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("SceneLoadMode.Additive", source, StringComparison.Ordinal);
            Assert.Contains("SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Additive)", source, StringComparison.Ordinal);
            Assert.Contains("SceneManager.UnloadScene(SplashSceneId)", source, StringComparison.Ordinal);
        }
    }
}
