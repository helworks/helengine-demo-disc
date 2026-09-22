using DemoDisc.EditorTools;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
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

    }
}
