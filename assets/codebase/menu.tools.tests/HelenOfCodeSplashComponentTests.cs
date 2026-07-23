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
            string menuSourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu",
                "MenuComponent.cs");
            string menuSource = File.ReadAllText(menuSourcePath);

            Assert.Contains("SceneLoadMode.Additive", source, StringComparison.Ordinal);
            Assert.Contains("SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Additive)", source, StringComparison.Ordinal);
            Assert.Contains("SceneManager.UnloadScene(SplashSceneId)", source, StringComparison.Ordinal);
            Assert.Contains("StartupInputGate.Acquire()", source, StringComparison.Ordinal);
            Assert.Contains("StartupInputGate.Release()", source, StringComparison.Ordinal);
            Assert.Contains("Core.Instance != null && Core.Instance.SceneManager != null", source, StringComparison.Ordinal);
            Assert.Contains("SceneEntityReference BackgroundSpriteEntityReference", source, StringComparison.Ordinal);
            Assert.Contains("SceneEntityReference LogoSpriteEntityReference", source, StringComparison.Ordinal);
            Assert.DoesNotContain("BackgroundChildIndex", source, StringComparison.Ordinal);
            Assert.DoesNotContain("LogoChildIndex", source, StringComparison.Ordinal);
            Assert.Contains("if (StartupInputGate.IsBlocked)", menuSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the splash uses the menu's accept inputs to release the gate and unload immediately.
        /// </summary>
        [Fact]
        public void Splash_runtime_source_skips_on_accept_input() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu",
                "HelenOfCodeSplashComponent.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("inputSystem.WasKeyPressed(Keys.Enter)", source, StringComparison.Ordinal);
            Assert.Contains("inputSystem.WasKeyPressed(Keys.Space)", source, StringComparison.Ordinal);
            Assert.Contains("inputSystem.WasKeyPressed(Keys.J)", source, StringComparison.Ordinal);
            Assert.Contains("Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept)", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South)", source, StringComparison.Ordinal);
            Assert.Contains("StartupInputGate.Release()", source, StringComparison.Ordinal);
            Assert.Contains("Core.Instance.SceneManager.UnloadScene(SplashSceneId)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures splash input ownership remains held until disposal so the accept event cannot reach the menu in the same frame.
        /// </summary>
        [Fact]
        public void Splash_runtime_source_releases_input_gate_during_dispose() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu",
                "HelenOfCodeSplashComponent.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("public override void Dispose()", source, StringComparison.Ordinal);
            Assert.Contains("StartupInputGate.Release();", source, StringComparison.Ordinal);
            Assert.Contains("base.Dispose();", source, StringComparison.Ordinal);
        }
    }
}
