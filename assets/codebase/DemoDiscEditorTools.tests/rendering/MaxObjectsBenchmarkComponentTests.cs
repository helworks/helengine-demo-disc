using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the frame-rate threshold logic used by the Max Objects benchmark.
    /// </summary>
    public sealed class MaxObjectsBenchmarkComponentTests {
        [Theory]
        [InlineData(60, 1d, 60d)]
        [InlineData(75, 1.5d, 50d)]
        [InlineData(30, 1d, 30d)]
        public void Frame_rate_sample_uses_frames_divided_by_elapsed_seconds(int frameCount, double elapsedSeconds, double expectedFramesPerSecond) {
            Assert.Equal(expectedFramesPerSecond, MaxObjectsBenchmarkComponent.ResolveFramesPerSecond(frameCount, elapsedSeconds), 10);
        }

        [Theory]
        [InlineData(30d, true)]
        [InlineData(29.5d, true)]
        [InlineData(30.1d, false)]
        [InlineData(60d, false)]
        public void Spawning_stops_when_average_fps_reaches_or_falls_below_target(double measuredFramesPerSecond, bool expectedToStop) {
            Assert.Equal(expectedToStop, MaxObjectsBenchmarkComponent.HasReachedTarget(measuredFramesPerSecond, 30d));
        }
    }
}
