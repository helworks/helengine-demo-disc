namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial selector formats its compact handheld timing presentation independently from the full desktop medal summary.
    /// </summary>
    public sealed class TiltTrialLevelSelectPresentationTests {
        /// <summary>
        /// Ensures the selector labels the single allowed level duration as the maximum time.
        /// </summary>
        [Fact]
        public void FormatMaximumTimeLabel_presents_one_maximum_value() {
            System.Reflection.MethodInfo formatter = typeof(city.game.TiltTrialLevelSelectComponent).GetMethod(
                "FormatMaximumTimeLabel",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            Assert.NotNull(formatter);
            Assert.Equal("MAX 99.00", formatter.Invoke(null, new object[] { 99f }));
        }

        /// <summary>
        /// Ensures the handheld details stage hides medal thresholds while the larger shared selector retains them.
        /// </summary>
        [Fact]
        public void FormatTargetTimesText_hides_handheld_medals_and_retains_desktop_summary() {
            System.Reflection.MethodInfo formatter = typeof(city.game.TiltTrialLevelSelectComponent).GetMethod(
                "FormatTargetTimesText",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            Assert.NotNull(formatter);
            Assert.Equal(string.Empty, formatter.Invoke(null, new object[] { true, 18f, 28f, 40f }));
            Assert.Equal(
                "Gold  18.00\nSilver 28.00\nBronze 40.00",
                formatter.Invoke(null, new object[] { false, 18f, 28f, 40f }));
        }
    }
}
