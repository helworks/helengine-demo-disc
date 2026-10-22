using DemoDisc.Raytracing;

namespace DemoDisc.Raytracing.tests {
    /// <summary>Preserves the established software image policy when native desktop and Apple targets are selected.</summary>
    public sealed class SoftwareTraceResolutionTests {
        /// <summary>Native target identifiers use the same explicit 320x240 accumulator as Windows.</summary>
        /// <param name="platform">Supported target identifier, including case variants.</param>
        [Theory]
        [InlineData("linux")]
        [InlineData("LINUX")]
        [InlineData("macos")]
        [InlineData("MACOS")]
        [InlineData("ios")]
        [InlineData("IOS")]
        [InlineData("windows")]
        public void NativePlatformsUseExistingSoftwareImagePolicy(string platform) {
            SoftwareTraceResolution resolution=SoftwareTraceResolution.ForPlatform(platform);
            Assert.Equal(320,resolution.Width);
            Assert.Equal(240,resolution.Height);
            Assert.Equal(76800,resolution.PixelCount);
            Assert.Equal(921600L,resolution.AccumulatorBytes);
        }

        /// <summary>Adding native targets does not weaken explicit failure for unknown platform identifiers.</summary>
        [Fact]
        public void UnknownPlatformsRemainRejected() {
            Assert.Throws<ArgumentOutOfRangeException>(()=>SoftwareTraceResolution.ForPlatform("unsupported-native"));
        }
    }
}