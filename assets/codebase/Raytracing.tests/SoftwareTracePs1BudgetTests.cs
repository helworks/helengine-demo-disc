using DemoDisc.Raytracing;
using DemoDisc.Raytracing.tests;
using helengine;

namespace DemoDisc.Raytracing.tests {
    /// <summary>Verifies the explicit PS1 image allocation policy without substituting native behavior or changing managed exception recovery tests.</summary>
    public sealed class SoftwareTracePs1BudgetTests {
        /// <summary>PS1 resolution and image-memory estimate remain explicit and within the configured default, independently of BVH/model memory.</summary>
        [Fact]
        public void Ps1ResolutionFitsDeclaredImageBudget() {
            SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform("PS1");
            Assert.Equal(128, resolution.Width);
            Assert.Equal(96, resolution.Height);
            Assert.Equal(147456L, resolution.AccumulatorBytes);
            Assert.Equal(221440L, SoftwareTraceImageBudget.ComputePs1ImageBytes(resolution));
            Assert.True(SoftwareTraceImageBudget.TryValidate(resolution, "ps1", SoftwareTraceImageBudget.DefaultPs1Bytes, out string failure));
            Assert.Equal(string.Empty, failure);
            Assert.Equal(SoftwareTraceImageBudget.DefaultPs1Bytes, new SoftwarePathTracerComponent().Ps1ImageBufferBudgetBytes);
        }

        /// <summary>Invalid or insufficient explicit budgets fail before presentation/model allocation and expose a stable recoverable preparation status.</summary>
        /// <param name="budget">An invalid budget or one byte below the exact image requirement.</param>
        [Theory]
        [InlineData(0L)]
        [InlineData(-1L)]
        [InlineData(221439L)]
        public void BudgetFailurePrecedesAllocation(long budget) {
            SoftwarePathTracerTestRenderManager2D renderer = new SoftwarePathTracerTestRenderManager2D();
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(renderer);
            bool ready = session.TryInitialize(new List<Entity>(), null, SoftwareTraceResolution.ForPlatform("ps1"), default, 1f, null, "ps1", budget);
            Assert.False(ready);
            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Contains("budget", session.FailureMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(renderer.BuildCalls);
            Assert.Null(session.PresentationTexture);
            Assert.Null(session.Tracer);
            Assert.Equal(0L, session.SteadyStateOwnedBytes);
            session.Dispose();
            session.Dispose();
            Assert.Equal(SoftwarePathTraceSessionState.Disposed, session.State);
        }

        /// <summary>The PS1 image limit is not applied to the established Windows image policy.</summary>
        [Fact]
        public void WindowsRetainsEstablishedResolutionAndMemoryPolicy() {
            SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform("windows");
            Assert.Equal(320, resolution.Width);
            Assert.Equal(240, resolution.Height);
            Assert.True(SoftwareTraceImageBudget.TryValidate(resolution, "windows", 1L, out string failure));
            Assert.Empty(failure);
        }

        /// <summary>Odd image widths account for the native even-word shadow stride before applying the byte-exact budget boundary.</summary>
        [Fact]
        public void OddWidthIncludesNativeShadowRowPadding() {
            SoftwareTraceResolution resolution = new SoftwareTraceResolution(129, 96);
            long imageBytes = (129L * 96L * 12L) + (129L * 96L * 4L) + (130L * 96L * 2L) + 256L;
            Assert.Equal(imageBytes, SoftwareTraceImageBudget.ComputePs1ImageBytes(resolution));
            Assert.False(SoftwareTraceImageBudget.TryValidate(resolution, "ps1", imageBytes - 1L, out string failure));
            Assert.Contains("budget", failure, StringComparison.OrdinalIgnoreCase);
            Assert.True(SoftwareTraceImageBudget.TryValidate(resolution, "ps1", imageBytes, out failure));
            Assert.Empty(failure);
        }

        /// <summary>An explicit resolution override outside the native mutable-texture limit returns a diagnostic before renderer allocation.</summary>
        /// <param name="width">Requested image width.</param>
        /// <param name="height">Requested image height.</param>
        [Theory]
        [InlineData(257, 96)]
        [InlineData(128, 257)]
        public void HardwareDimensionsRejectedBeforeAllocation(int width, int height) {
            SoftwarePathTracerTestRenderManager2D renderer = new SoftwarePathTracerTestRenderManager2D();
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(renderer);
            Assert.False(session.TryInitialize(new List<Entity>(), null, new SoftwareTraceResolution(width, height), default, 1f, null, "ps1", long.MaxValue));
            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Contains("dimensions", session.FailureMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(renderer.BuildCalls);
        }

        /// <summary>A null allocator result is a recoverable status and releases any earlier owned buffer while preserving borrowed geometry and BVH.</summary>
        /// <param name="failAccumulator">Selects failure before or after the accumulator allocation.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InvalidBufferReturnsStatusWithoutPublishingProgressiveState(bool failAccumulator) {
            SoftwareTriangle[] triangles = {
                MakeTriangle(new float3(-1f, -1f, -2f), new float3(2f, 0f, 0f), new float3(0f, 2f, 0f)),
                MakeTriangle(new float3(1f, 1f, -2f), new float3(-2f, 0f, 0f), new float3(0f, -2f, 0f))
            };
            SoftwareMaterialData[] materials = { new SoftwareMaterialData(float3.One, float3.One) };
            SoftwareAreaLight light = new SoftwareAreaLight(triangles[0].P0, triangles[0].Edge1, triangles[0].Edge2, new float3(0f, 0f, 1f), 4f, float3.One, 0, 1);
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            using SoftwarePathTracer tracer = new SoftwarePathTracer(triangles, materials, light, bvh, new int[SoftwareBvh.TraversalStackCapacity]);
            SoftwareTraceCamera camera = new SoftwareTraceCamera(float3.Zero, new float3(0f, 0f, -1f), new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), 60f);
            Assert.False(tracer.TryInitializeProgressive(SoftwareTraceResolution.ForPlatform("ps1"), camera, 1f, new SoftwareTraceStatusAllocator(failAccumulator)));
            Assert.False(tracer.IsProgressiveInitialized);
            Assert.Empty(tracer.Accumulation);
            Assert.Empty(tracer.TileRgba8);
            Assert.Equal(0L, tracer.ProgressiveOwnedBytes);
            Assert.NotEmpty(bvh.Nodes);
            for (int cycle = 0; cycle < 2; cycle++) {
                Assert.True(tracer.TryInitializeProgressive(SoftwareTraceResolution.ForPlatform("ps1"), camera, 1f));
                Assert.Equal(147712L, tracer.ProgressiveOwnedBytes);
                tracer.Dispose();
                tracer.Dispose();
                Assert.Empty(tracer.Accumulation);
                Assert.Empty(tracer.TileRgba8);
                Assert.NotEmpty(bvh.Nodes);
            }
            Assert.Equal(2, triangles.Length);
        }

        /// <summary>Constructs finite emitter geometry with exact bounds for the borrowed-scene lifetime test.</summary>
        /// <param name="origin">Triangle origin.</param>
        /// <param name="firstEdge">First edge.</param>
        /// <param name="secondEdge">Second edge.</param>
        /// <returns>Validated compact triangle.</returns>
        static SoftwareTriangle MakeTriangle(float3 origin, float3 firstEdge, float3 secondEdge) {
            float3 first = origin + firstEdge;
            float3 second = origin + secondEdge;
            return new SoftwareTriangle(origin, firstEdge, secondEdge, float3.Normalize(float3.Cross(firstEdge, secondEdge)), 0,
                (origin + first + second) / 3f, float3.Min(origin, float3.Min(first, second)), float3.Max(origin, float3.Max(first, second)));
        }
    }
}
