using System;
using city.rendering;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies one deterministic 16x16 Cornell enclosure through the complete software tracer.
    /// </summary>
    public sealed class SoftwarePathTracerReferenceTests {
        const int Width = 16;
        const int Height = 16;
        const int SamplesPerPixel = 64;
        const int ExpectedTileCalls = SamplesPerPixel * 4;
        const float Exposure = 1f;
        static readonly SoftwareTraceCamera Camera = new SoftwareTraceCamera(
            new float3(0f, 0f, 3f),
            new float3(0f, 0f, -1f),
            new float3(1f, 0f, 0f),
            new float3(0f, 1f, 0f),
            55f);

        /// <summary>
        /// Exercises the exact reference geometry, progressive scheduler, path kernel, and cleanup.
        /// </summary>
        [Fact]
        public void Cornell_reference_render_is_finite_illuminated_deterministic_and_transfer_sensitive() {
            ReferenceFixture first = CreateFixture();
            ReferenceFixture second = CreateFixture();
            try {
                AssertFixtureGeometry(first);
                Assert.NotSame(first.Triangles, second.Triangles);
                Assert.NotSame(first.Materials, second.Materials);
                Assert.NotSame(first.Bvh, second.Bvh);
                Assert.NotSame(first.Stack, second.Stack);
                Assert.NotSame(first.Tracer, second.Tracer);
                first.Tracer.InitializeProgressive(new SoftwareTraceResolution(Width, Height), Camera, Exposure);
                second.Tracer.InitializeProgressive(new SoftwareTraceResolution(Width, Height), Camera, Exposure);
                Assert.Equal(Width * Height, first.Tracer.Accumulation.Length);
                Assert.Equal(Width * Height, second.Tracer.Accumulation.Length);
                Assert.Equal(SoftwarePathTracer.TileRgba8Bytes, first.Tracer.TileRgba8.Length);
                Assert.Equal(SoftwarePathTracer.TileRgba8Bytes, second.Tracer.TileRgba8.Length);

                AssertPrimaryBackHit(first, 8, 8);
                AssertPrimaryRegionMaterial(first, 4, 11, 2, 2, 0);
                AssertPrimaryRegionMaterial(first, 10, 11, 2, 2, 0);
                AssertPrimaryMaterial(first, 3, 11, 1);
                AssertPrimaryMaterial(first, 12, 11, 2);

                RenderResult firstResult = RenderToCompletion(first);
                RenderResult secondResult = RenderToCompletion(second);

                Assert.Equal(SamplesPerPixel, first.Tracer.CompletedPasses);
                Assert.Equal(SamplesPerPixel, second.Tracer.CompletedPasses);
                Assert.Equal(ExpectedTileCalls, firstResult.TileIndices.Length);
                Assert.Equal(ExpectedTileCalls, secondResult.TileIndices.Length);
                Assert.Equal(first.Tracer.CompletedPasses, second.Tracer.CompletedPasses);
                Assert.Equal(first.Tracer.RayCount, second.Tracer.RayCount);
                Assert.Equal(first.Tracer.NonFiniteSampleCount, second.Tracer.NonFiniteSampleCount);
                Assert.Equal(0L, first.Tracer.NonFiniteSampleCount);
                Assert.Equal(0L, second.Tracer.NonFiniteSampleCount);
                Assert.True(first.Tracer.RayCount > (long)Width * Height * SamplesPerPixel, $"RayCount={first.Tracer.RayCount}");
                Assert.True(firstResult.CentralLuminance > 0.2f, Describe(first, firstResult));
                Assert.True(firstResult.EmitterLuminance > firstResult.CornerLuminance + 0.1f, Describe(first, firstResult));
                Assert.True(firstResult.LeftTransferRedMinusGreen > 0.01f, Describe(first, firstResult));
                Assert.True(firstResult.RightTransferGreenMinusRed > 0.01f, Describe(first, firstResult));

                Assert.Equal(firstResult.TileIndices, secondResult.TileIndices);
                Assert.Equal(firstResult.NonBlackPixels, secondResult.NonBlackPixels);
                Assert.Equal(firstResult.CentralLuminance, secondResult.CentralLuminance);
                Assert.Equal(firstResult.EmitterLuminance, secondResult.EmitterLuminance);
                Assert.Equal(firstResult.CornerLuminance, secondResult.CornerLuminance);
                Assert.Equal(firstResult.LeftTransferRedMinusGreen, secondResult.LeftTransferRedMinusGreen);
                Assert.Equal(firstResult.RightTransferGreenMinusRed, secondResult.RightTransferGreenMinusRed);
                AssertBitwiseEqual(first.Tracer.Accumulation, second.Tracer.Accumulation);

            }
            finally {
                first.Tracer.DisposeProgressive();
                first.Bvh.Dispose();
                second.Tracer.DisposeProgressive();
                second.Bvh.Dispose();
                Assert.Empty(first.Tracer.Accumulation);
                Assert.Empty(first.Tracer.TileRgba8);
                Assert.Empty(first.Bvh.Nodes);
                Assert.Empty(first.Bvh.TriangleOrder);
                Assert.Empty(second.Tracer.Accumulation);
                Assert.Empty(second.Tracer.TileRgba8);
                Assert.Empty(second.Bvh.Nodes);
                Assert.Empty(second.Bvh.TriangleOrder);
            }
        }

        /// <summary>
        /// Validates compact fixture geometry before any image is accumulated.
        /// </summary>
        static void AssertFixtureGeometry(ReferenceFixture fixture) {
            Assert.Equal(12, fixture.Triangles.Length);
            Assert.Equal(4, fixture.Materials.Length);
            Assert.Equal(SoftwareBvh.TraversalStackCapacity, fixture.Stack.Length);
            Assert.NotEmpty(fixture.Bvh.Nodes);
            AssertNear(fixture.Materials[0].DiffuseColor, new float3(0.75f, 0.75f, 0.75f), 0.00001f);
            AssertNear(fixture.Materials[1].DiffuseColor, new float3(0.75f, 0.05f, 0.05f), 0.00001f);
            AssertNear(fixture.Materials[2].DiffuseColor, new float3(0.05f, 0.75f, 0.05f), 0.00001f);
            AssertNear(fixture.Materials[3].DiffuseColor, float3.Zero, 0.00001f);
            Assert.Equal(10, fixture.Light.FirstTriangleIndex);
            Assert.Equal(11, fixture.Light.SecondTriangleIndex);
            Assert.Equal(0.49f, fixture.Light.Area);
            AssertNear(fixture.Light.InwardNormal, new float3(0f, -1f, 0f), 0.00001f);

            float3[] expectedNormals = {
                new float3(0f, 1f, 0f),
                new float3(0f, -1f, 0f),
                new float3(0f, 0f, 1f),
                new float3(1f, 0f, 0f),
                new float3(-1f, 0f, 0f),
                new float3(0f, -1f, 0f)
            };
            for (int quad = 0; quad < 6; quad++) {
                AssertNear(fixture.Triangles[quad * 2].GeometricNormal, expectedNormals[quad], 0.00001f);
                AssertNear(fixture.Triangles[(quad * 2) + 1].GeometricNormal, expectedNormals[quad], 0.00001f);
            }

            SoftwareTriangle emitterFirst = fixture.Triangles[fixture.Light.FirstTriangleIndex];
            SoftwareTriangle emitterSecond = fixture.Triangles[fixture.Light.SecondTriangleIndex];
            Assert.Equal(3, emitterFirst.MaterialIndex);
            Assert.Equal(3, emitterSecond.MaterialIndex);
            AssertNear(emitterFirst.GeometricNormal, fixture.Light.InwardNormal, 0.00001f);
            AssertNear(emitterSecond.GeometricNormal, fixture.Light.InwardNormal, 0.00001f);
            AssertNear(emitterFirst.P0, fixture.Light.Corner, 0.00001f);
            AssertNear(emitterFirst.Edge1, fixture.Light.Edge1, 0.00001f);
            AssertNear(emitterFirst.Edge2, fixture.Light.Edge2, 0.00001f);
            AssertNear(emitterSecond.P0, fixture.Light.Corner + fixture.Light.Edge1 + fixture.Light.Edge2, 0.00001f);
            AssertNear(emitterSecond.Edge1, -fixture.Light.Edge1, 0.00001f);
            AssertNear(emitterSecond.Edge2, -fixture.Light.Edge2, 0.00001f);
            Assert.Equal(10f, fixture.Materials[3].Emission.X);
            Assert.Equal(10f, fixture.Materials[3].Emission.Y);
            Assert.Equal(10f, fixture.Materials[3].Emission.Z);

            float3 cross = float3.Cross(fixture.Light.Edge1, fixture.Light.Edge2);
            float area = (float)Math.Sqrt((cross.X * cross.X) + (cross.Y * cross.Y) + (cross.Z * cross.Z));
            Assert.InRange(area, 0.48999f, 0.49001f);
            Assert.True(float3.Dot(cross, fixture.Light.InwardNormal) > 0f);
        }

        /// <summary>
        /// Renders every scheduled tile until the pass boundary, retaining only tile identity diagnostics.
        /// </summary>
        static RenderResult RenderToCompletion(ReferenceFixture fixture) {
            int[] tileIndices = new int[ExpectedTileCalls];
            int tileCalls = 0;
            while (fixture.Tracer.CompletedPasses < SamplesPerPixel) {
                SoftwareTraceTile tile = fixture.Tracer.RenderNextTile();
                Assert.True(tileCalls < tileIndices.Length, $"Tile calls exceeded expected count: {tileCalls + 1}");
                tileIndices[tileCalls++] = tile.TileIndex;
            }
            Assert.Equal(ExpectedTileCalls, tileCalls);
            Assert.Equal(SamplesPerPixel, fixture.Tracer.CompletedPasses);
            AssertEveryPassCoversAllTiles(tileIndices);
            return Measure(fixture, tileIndices);
        }

        /// <summary>
        /// Measures finite radiance and the fixed multi-pixel Cornell transfer regions.
        /// </summary>
        static RenderResult Measure(ReferenceFixture fixture, int[] tileIndices) {
            float3[] accumulation = fixture.Tracer.Accumulation;
            int nonBlackPixels = 0;
            for (int pixel = 0; pixel < accumulation.Length; pixel++) {
                float3 value = accumulation[pixel];
                Assert.True(IsFinite(value), $"Non-finite accumulator at pixel {pixel}: {value}");
                Assert.True(value.X >= 0f && value.Y >= 0f && value.Z >= 0f, $"Negative accumulator at pixel {pixel}: {value}");
                float3 average = value / SamplesPerPixel;
                Assert.True(IsFinite(average), $"Non-finite averaged radiance at pixel {pixel}: {average}");
                Assert.True(average.X >= 0f && average.Y >= 0f && average.Z >= 0f, $"Negative averaged radiance at pixel {pixel}: {average}");
                if (value.X != 0f || value.Y != 0f || value.Z != 0f) {
                    nonBlackPixels++;
                }
            }
            Assert.True(nonBlackPixels > 0, "The reference accumulator is entirely black.");

            return new RenderResult(
                tileIndices,
                nonBlackPixels,
                AverageLuminance(accumulation, 6, 6, 4, 4),
                AverageLuminance(accumulation, 7, 5, 2, 2),
                AverageCornerLuminance(accumulation),
                AverageChannelDifference(accumulation, 4, 11, 2, 2, true),
                AverageChannelDifference(accumulation, 10, 11, 2, 2, false));
        }

        /// <summary>
        /// Proves the center primary ray lands on neutral back geometry.
        /// </summary>
        static void AssertPrimaryMaterial(ReferenceFixture fixture, int pixelX, int pixelY, int expectedMaterial) {
            SoftwareRay ray = fixture.Tracer.CreateCameraRay(pixelX, pixelY, 0);
            Assert.True(fixture.Bvh.Intersect(fixture.Triangles, ref ray, SoftwarePathTracer.RayEpsilon, float.PositiveInfinity, fixture.Stack, out _, out int triangleIndex));
            Assert.InRange(triangleIndex, 0, fixture.Triangles.Length - 1);
            Assert.Equal(expectedMaterial, fixture.Triangles[triangleIndex].MaterialIndex);
        }

        /// <summary>
        /// Proves the center camera ray lands on one of the two neutral back-wall triangles.
        /// </summary>
        static void AssertPrimaryBackHit(ReferenceFixture fixture, int pixelX, int pixelY) {
            SoftwareRay ray = fixture.Tracer.CreateCameraRay(pixelX, pixelY, 0);
            Assert.True(fixture.Bvh.Intersect(fixture.Triangles, ref ray, SoftwarePathTracer.RayEpsilon, float.PositiveInfinity, fixture.Stack, out _, out int triangleIndex));
            Assert.True(triangleIndex == 4 || triangleIndex == 5, $"Center primary hit triangle {triangleIndex}, expected back wall 4 or 5.");
            Assert.Equal(0, fixture.Triangles[triangleIndex].MaterialIndex);
        }

        /// <summary>
        /// Proves every pixel in one transfer region has a neutral primary hit before color comparison.
        /// </summary>
        static void AssertPrimaryRegionMaterial(ReferenceFixture fixture, int x, int y, int width, int height, int expectedMaterial) {
            for (int row = y; row < y + height; row++) {
                for (int column = x; column < x + width; column++) {
                    AssertPrimaryMaterial(fixture, column, row, expectedMaterial);
                }
            }
        }

        /// <summary>
        /// Proves the scheduler visits each of the four 8x8 tiles exactly once per pass.
        /// </summary>
        static void AssertEveryPassCoversAllTiles(int[] tileIndices) {
            for (int pass = 0; pass < SamplesPerPixel; pass++) {
                bool[] seen = new bool[4];
                for (int tilePosition = 0; tilePosition < 4; tilePosition++) {
                    int tileIndex = tileIndices[(pass * 4) + tilePosition];
                    Assert.InRange(tileIndex, 0, 3);
                    Assert.False(seen[tileIndex], $"Tile {tileIndex} repeated in pass {pass}.");
                    seen[tileIndex] = true;
                }
                for (int tileIndex = 0; tileIndex < seen.Length; tileIndex++) {
                    Assert.True(seen[tileIndex], $"Tile {tileIndex} missing in pass {pass}.");
                }
            }
        }

        /// <summary>
        /// Compares every accumulated channel by exact IEEE-754 bit pattern.
        /// </summary>
        static void AssertBitwiseEqual(float3[] first, float3[] second) {
            Assert.Equal(first.Length, second.Length);
            for (int index = 0; index < first.Length; index++) {
                Assert.Equal(BitConverter.SingleToInt32Bits(first[index].X), BitConverter.SingleToInt32Bits(second[index].X));
                Assert.Equal(BitConverter.SingleToInt32Bits(first[index].Y), BitConverter.SingleToInt32Bits(second[index].Y));
                Assert.Equal(BitConverter.SingleToInt32Bits(first[index].Z), BitConverter.SingleToInt32Bits(second[index].Z));
            }
        }

        /// <summary>
        /// Averages linear luminance over one rectangular image region.
        /// </summary>
        static float AverageLuminance(float3[] accumulation, int x, int y, int width, int height) {
            float sum = 0f;
            for (int row = y; row < y + height; row++) {
                for (int column = x; column < x + width; column++) {
                    sum += Luminance(accumulation[(row * Width) + column] / SamplesPerPixel);
                }
            }
            return sum / (width * height);
        }

        /// <summary>
        /// Averages the four 2x2 image-corner luminance regions.
        /// </summary>
        static float AverageCornerLuminance(float3[] accumulation) {
            float sum = AverageLuminance(accumulation, 0, 0, 2, 2);
            sum += AverageLuminance(accumulation, Width - 2, 0, 2, 2);
            sum += AverageLuminance(accumulation, 0, Height - 2, 2, 2);
            sum += AverageLuminance(accumulation, Width - 2, Height - 2, 2, 2);
            return sum / 4f;
        }

        /// <summary>
        /// Averages red-minus-green or green-minus-red over one transfer region.
        /// </summary>
        static float AverageChannelDifference(float3[] accumulation, int x, int y, int width, int height, bool redFirst) {
            float sum = 0f;
            for (int row = y; row < y + height; row++) {
                for (int column = x; column < x + width; column++) {
                    float3 value = accumulation[(row * Width) + column] / SamplesPerPixel;
                    sum += redFirst ? value.X - value.Y : value.Y - value.X;
                }
            }
            return sum / (width * height);
        }

        /// <summary>
        /// Returns CIE-style luminance for linear RGB values.
        /// </summary>
        static float Luminance(float3 value) {
            return (value.X * 0.2126f) + (value.Y * 0.7152f) + (value.Z * 0.0722f);
        }

        /// <summary>
        /// Builds a detailed invariant message with observed values for failed assertions.
        /// </summary>
        static string Describe(ReferenceFixture fixture, RenderResult result) {
            return "triangles=" + fixture.Triangles.Length +
                ", materials=" + fixture.Materials.Length +
                ", tiles=" + result.TileIndices.Length +
                ", passes=" + fixture.Tracer.CompletedPasses +
                ", rays=" + fixture.Tracer.RayCount +
                ", nonfinite=" + fixture.Tracer.NonFiniteSampleCount +
                ", nonblack=" + result.NonBlackPixels +
                ", central=" + result.CentralLuminance +
                ", emitter=" + result.EmitterLuminance +
                ", corners=" + result.CornerLuminance +
                ", leftRedMinusGreen=" + result.LeftTransferRedMinusGreen +
                ", rightGreenMinusRed=" + result.RightTransferGreenMinusRed;
        }

        /// <summary>
        /// Creates the exact 12-triangle, four-material Cornell enclosure fixture.
        /// </summary>
        static ReferenceFixture CreateFixture() {
            SoftwareTriangle[] triangles = new SoftwareTriangle[12];
            AddQuad(triangles, 0, new float3(-1f, -1f, -1f), new float3(0f, 0f, 2f), new float3(2f, 0f, 0f), 0);
            AddQuad(triangles, 2, new float3(-1f, 1f, -1f), new float3(2f, 0f, 0f), new float3(0f, 0f, 2f), 0);
            AddQuad(triangles, 4, new float3(-1f, -1f, -1f), new float3(2f, 0f, 0f), new float3(0f, 2f, 0f), 0);
            AddQuad(triangles, 6, new float3(-1f, -1f, -1f), new float3(0f, 2f, 0f), new float3(0f, 0f, 2f), 1);
            AddQuad(triangles, 8, new float3(1f, -1f, -1f), new float3(0f, 0f, 2f), new float3(0f, 2f, 0f), 2);
            AddQuad(triangles, 10, new float3(-0.35f, 0.9f, -0.35f), new float3(0.7f, 0f, 0f), new float3(0f, 0f, 0.7f), 3);

            SoftwareMaterialData[] materials = {
                new SoftwareMaterialData(new float3(0.75f, 0.75f, 0.75f), float3.Zero),
                new SoftwareMaterialData(new float3(0.75f, 0.05f, 0.05f), float3.Zero),
                new SoftwareMaterialData(new float3(0.05f, 0.75f, 0.05f), float3.Zero),
                new SoftwareMaterialData(float3.Zero, new float3(10f, 10f, 10f))
            };
            SoftwareAreaLight light = new SoftwareAreaLight(
                new float3(-0.35f, 0.9f, -0.35f),
                new float3(0.7f, 0f, 0f),
                new float3(0f, 0f, 0.7f),
                new float3(0f, -1f, 0f),
                0.49f,
                new float3(10f, 10f, 10f),
                10,
                11);
            SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];
            SoftwarePathTracer tracer = new SoftwarePathTracer(triangles, materials, light, bvh, stack);
            return new ReferenceFixture(triangles, materials, light, bvh, stack, tracer);
        }

        /// <summary>
        /// Adds two consistently wound triangles for one rectangle.
        /// </summary>
        static void AddQuad(SoftwareTriangle[] triangles, int firstIndex, float3 corner, float3 edge1, float3 edge2, int materialIndex) {
            triangles[firstIndex] = MakeTriangle(corner, edge1, edge2, materialIndex);
            triangles[firstIndex + 1] = MakeTriangle(corner + edge1 + edge2, -edge1, -edge2, materialIndex);
        }

        /// <summary>
        /// Computes all compact fields for one triangle.
        /// </summary>
        static SoftwareTriangle MakeTriangle(float3 p0, float3 edge1, float3 edge2, int materialIndex) {
            float3 p1 = p0 + edge1;
            float3 p2 = p0 + edge2;
            float3 normal = float3.Normalize(float3.Cross(edge1, edge2));
            float3 centroid = (p0 + p1 + p2) / 3f;
            float3 minimum = float3.Min(p0, float3.Min(p1, p2));
            float3 maximum = float3.Max(p0, float3.Max(p1, p2));
            return new SoftwareTriangle(p0, edge1, edge2, normal, materialIndex, centroid, minimum, maximum);
        }

        /// <summary>
        /// Compares vectors component-wise within a fixture tolerance.
        /// </summary>
        static void AssertNear(float3 actual, float3 expected, float tolerance) {
            Assert.InRange(actual.X, expected.X - tolerance, expected.X + tolerance);
            Assert.InRange(actual.Y, expected.Y - tolerance, expected.Y + tolerance);
            Assert.InRange(actual.Z, expected.Z - tolerance, expected.Z + tolerance);
        }

        /// <summary>
        /// Tests all color channels for finite values.
        /// </summary>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }

        /// <summary>
        /// Stores borrowed scene state and progressive tracer state for one reference render.
        /// </summary>
        sealed class ReferenceFixture {
            public readonly SoftwareTriangle[] Triangles;
            public readonly SoftwareMaterialData[] Materials;
            public readonly SoftwareAreaLight Light;
            public readonly SoftwareBvh Bvh;
            public readonly int[] Stack;
            public readonly SoftwarePathTracer Tracer;

            public ReferenceFixture(SoftwareTriangle[] triangles, SoftwareMaterialData[] materials, SoftwareAreaLight light, SoftwareBvh bvh, int[] stack, SoftwarePathTracer tracer) {
                Triangles = triangles;
                Materials = materials;
                Light = light;
                Bvh = bvh;
                Stack = stack;
                Tracer = tracer;
            }
        }

        /// <summary>
        /// Stores measured image invariants and the complete tile-order sequence.
        /// </summary>
        sealed class RenderResult {
            public readonly int[] TileIndices;
            public readonly int NonBlackPixels;
            public readonly float CentralLuminance;
            public readonly float EmitterLuminance;
            public readonly float CornerLuminance;
            public readonly float LeftTransferRedMinusGreen;
            public readonly float RightTransferGreenMinusRed;

            public RenderResult(int[] tileIndices, int nonBlackPixels, float centralLuminance, float emitterLuminance, float cornerLuminance, float leftTransferRedMinusGreen, float rightTransferGreenMinusRed) {
                TileIndices = tileIndices;
                NonBlackPixels = nonBlackPixels;
                CentralLuminance = centralLuminance;
                EmitterLuminance = emitterLuminance;
                CornerLuminance = cornerLuminance;
                LeftTransferRedMinusGreen = leftTransferRedMinusGreen;
                RightTransferGreenMinusRed = rightTransferGreenMinusRed;
            }
        }
    }
}
