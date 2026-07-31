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

            Assert.Equal(255, component.ResolveAlphaForElapsedSeconds(3.5d));
            Assert.Equal(0, component.ResolveAlphaForElapsedSeconds(5d));
        }

        /// <summary>
        /// Ensures a synchronous disc read cannot advance the splash timer by multiple seconds in one update.
        /// </summary>
        [Fact]
        public void Splash_animation_caps_disc_load_frame_time() {
            HelenOfCodeSplashComponent component = new HelenOfCodeSplashComponent();

            Assert.Equal(0.1d, component.ResolveAnimationFrameDeltaSeconds(7d), 10);
            Assert.Equal(1d / 30d, component.ResolveAnimationFrameDeltaSeconds(1d / 30d), 10);
        }

        /// <summary>
        /// Verifies the runtime component requests additive menu loading and removes only its splash scene so the persistent loading overlay remains available for later transitions.
        /// </summary>
        [Fact]
        public void Splash_runtime_source_uses_additive_menu_loading_and_preserves_loading_overlay() {
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
            Assert.DoesNotContain("SceneManager.UnloadScene(LoadingScreenSceneId)", source, StringComparison.Ordinal);
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
        /// Ensures the splash keeps menu input blocked until its deferred unload disposes the splash component.
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
            int requestStart = source.IndexOf("void RequestSplashUnload()", StringComparison.Ordinal);
            int disposeStart = source.IndexOf("public override void Dispose()", StringComparison.Ordinal);
            Assert.True(requestStart >= 0);
            Assert.True(disposeStart >= 0);
            string requestSource = source.Substring(requestStart);
            Assert.Contains("SplashUnloadWasRequested = true;", requestSource, StringComparison.Ordinal);
            Assert.DoesNotContain("StartupInputGate.Release();", requestSource, StringComparison.Ordinal);
            Assert.Contains("StartupInputGate.Release();", source.Substring(0, requestStart), StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures a controller state already held when the splash begins cannot be interpreted as a request to skip it.
        /// </summary>
        [Fact]
        public void Splash_runtime_source_ignores_accept_input_on_its_first_update() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu",
                "HelenOfCodeSplashComponent.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("bool HasCompletedFirstUpdate;", source, StringComparison.Ordinal);
            Assert.Contains("if (HasCompletedFirstUpdate && IsAcceptPressed())", source, StringComparison.Ordinal);
            Assert.Contains("HasCompletedFirstUpdate = true;", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures disposal also releases the input gate when an external lifecycle path removes the splash.
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
