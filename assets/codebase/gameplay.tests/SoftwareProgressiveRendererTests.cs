using System;
using System.Collections.Generic;
using System.Reflection;
using city.rendering;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies fixed-resolution progressive software tracing and CPU presentation.
    /// </summary>
    public sealed class SoftwareProgressiveRendererTests {
        /// <summary>
        /// Requires the stateless default allocator adapter to be absent from the production assembly.
        /// </summary>
        [Fact]
        public void Default_allocator_adapter_is_removed() {
            Assert.Null(typeof(SoftwarePathTracer).Assembly.GetType("city.rendering.DefaultSoftwareTraceBufferAllocator"));
        }

        /// <summary>
        /// Ensures every supported platform selects the exact contract resolution and memory.
        /// </summary>
        [Theory]
        [InlineData("ds", 256, 192, 49152, 589824)]
        [InlineData("3ds", 320, 240, 76800, 921600)]
        [InlineData("gamecube", 320, 240, 76800, 921600)]
        [InlineData("ps2", 320, 240, 76800, 921600)]
        [InlineData("psp", 320, 240, 76800, 921600)]
        [InlineData("psvita", 320, 240, 76800, 921600)]
        [InlineData("switch", 320, 240, 76800, 921600)]
        [InlineData("wii", 320, 240, 76800, 921600)]
        [InlineData("wiiu", 320, 240, 76800, 921600)]
        [InlineData("windows", 320, 240, 76800, 921600)]
        public void Resolution_and_accumulator_bytes_match_contract(string platform, int width, int height, int pixels, long bytes) {
            SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform(platform);

            Assert.Equal(width, resolution.Width);
            Assert.Equal(height, resolution.Height);
            Assert.Equal(pixels, resolution.PixelCount);
            Assert.Equal(bytes, resolution.AccumulatorBytes);
        }

        /// <summary>
        /// Ensures platform identifiers use ordinal case-insensitive matching.
        /// </summary>
        [Fact]
        public void Platform_resolution_is_ordinal_case_insensitive() {
            SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform("DS");

            Assert.Equal(256, resolution.Width);
            Assert.Equal(192, resolution.Height);
        }

        /// <summary>
        /// Ensures custom dimensions use checked positive byte arithmetic.
        /// </summary>
        [Fact]
        public void Resolution_rejects_invalid_or_overflowing_dimensions() {
            SoftwareTraceResolution resolution = new SoftwareTraceResolution(17, 9);

            Assert.Equal(153, resolution.PixelCount);
            Assert.Equal(1836L, resolution.AccumulatorBytes);
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceResolution(0, 9));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceResolution(-1, 9));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceResolution(int.MaxValue, int.MaxValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoftwareTraceResolution.ForPlatform("unknown"));
        }

        /// <summary>
        /// Ensures the runtime has no persistent per-pixel state beyond one float3 accumulator.
        /// </summary>
        [Fact]
        public void Progressive_state_has_no_per_pixel_count_rng_or_display_arrays() {
            FieldInfo[] fields = typeof(SoftwarePathTracer).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields) {
                if (!field.FieldType.IsArray) {
                    continue;
                }

                string name = field.Name.ToLowerInvariant();
                Assert.DoesNotContain("count", name);
                Assert.DoesNotContain("rng", name);
                Assert.DoesNotContain("display", name);
                Assert.DoesNotContain("sample", name);
            }
        }

        /// <summary>
        /// Ensures initialization allocates exactly one accumulator and one 256-byte tile.
        /// </summary>
        [Fact]
        public void Initialization_reports_exact_owned_progressive_memory() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            SoftwareTraceResolution resolution = new SoftwareTraceResolution(17, 9);

            fixture.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);

            Assert.True(fixture.Tracer.IsProgressiveInitialized);
            Assert.Equal(153, fixture.Tracer.Accumulation.Length);
            Assert.Equal(256, fixture.Tracer.TileRgba8.Length);
            Assert.Equal(32, fixture.Tracer.TileRowPitch);
            Assert.Equal(resolution.AccumulatorBytes + 256L, fixture.Tracer.ProgressiveOwnedBytes);
        }

        /// <summary>
        /// Ensures allocator failures roll progressive ownership back to an empty state.
        /// </summary>
        [Fact]
        public void Allocation_failure_rolls_back_all_progressive_buffers() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            ThrowingAllocator accumulatorFailure = new ThrowingAllocator(true, false);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f, accumulatorFailure));

            Assert.Equal("Software progressive buffers could not be allocated.", exception.Message);
            Assert.False(fixture.Tracer.IsProgressiveInitialized);
            Assert.Empty(fixture.Tracer.Accumulation);
            Assert.Empty(fixture.Tracer.TileRgba8);
            Assert.Equal(0L, fixture.Tracer.ProgressiveOwnedBytes);
        }

        /// <summary>
        /// Ensures a tile allocator failure also releases an already allocated accumulator.
        /// </summary>
        [Fact]
        public void Tile_allocation_failure_rolls_back_accumulator() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            ThrowingAllocator tileFailure = new ThrowingAllocator(false, true);

            Assert.Throws<InvalidOperationException>(() => fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f, tileFailure));

            Assert.False(fixture.Tracer.IsProgressiveInitialized);
            Assert.Empty(fixture.Tracer.Accumulation);
            Assert.Empty(fixture.Tracer.TileRgba8);
            Assert.Equal(0L, fixture.Tracer.ProgressiveOwnedBytes);
        }

        /// <summary>
        /// Ensures initialization cannot replace an active progressive state.
        /// </summary>
        [Fact]
        public void Double_initialization_is_rejected() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f);

            Assert.Throws<InvalidOperationException>(() => fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f));
        }

        /// <summary>
        /// Ensures progressive disposal is idempotent and preserves borrowed scene state.
        /// </summary>
        [Fact]
        public void Progressive_disposal_is_idempotent_and_borrowed_state_survives() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            SoftwareTriangle[] triangles = fixture.Triangles;
            SoftwareBvh bvh = fixture.Bvh;
            int[] stack = fixture.Stack;
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f);

            fixture.Tracer.DisposeProgressive();
            fixture.Tracer.DisposeProgressive();

            Assert.False(fixture.Tracer.IsProgressiveInitialized);
            Assert.Empty(fixture.Tracer.Accumulation);
            Assert.Empty(fixture.Tracer.TileRgba8);
            Assert.Same(triangles, fixture.Triangles);
            Assert.Same(bvh, fixture.Bvh);
            Assert.Same(stack, fixture.Stack);
            Assert.Throws<InvalidOperationException>(() => fixture.Tracer.RenderNextTile());
        }

        /// <summary>
        /// Ensures camera validation accepts a canonical basis and rejects malformed bases.
        /// </summary>
        [Fact]
        public void Camera_validation_rejects_nonfinite_nonorthogonal_or_wrong_handed_inputs() {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(new float3(float.NaN, 0f, 0f), new float3(0f, 0f, -1f), new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), 60f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(float3.Zero, new float3(float.NaN, 0f, -1f), new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), 60f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(float3.Zero, new float3(0f, 0f, -2f), new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), 60f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(float3.Zero, new float3(0f, 0f, -1f), new float3(1f, 0f, 0f), new float3(1f, 0f, 0f), 60f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(float3.Zero, new float3(0f, 0f, -1f), new float3(-1f, 0f, 0f), new float3(0f, 1f, 0f), 60f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(float3.Zero, new float3(0f, 0f, -1f), new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SoftwareTraceCamera(float3.Zero, new float3(0f, 0f, -1f), new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), 179f));
        }

        /// <summary>
        /// Ensures invalid exposure values are rejected before progressive allocation.
        /// </summary>
        [Fact]
        public void Progressive_initialization_rejects_invalid_exposure() {
            Assert.Throws<ArgumentOutOfRangeException>(() => InitializeWithExposure(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => InitializeWithExposure(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => InitializeWithExposure(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => InitializeWithExposure(float.PositiveInfinity));
        }

        /// <summary>
        /// Ensures one render call traces one clipped tile and reuses the staging array.
        /// </summary>
        [Fact]
        public void Render_next_tile_processes_one_edge_clipped_tile_and_reuses_staging() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f);
            byte[] staging = fixture.Tracer.TileRgba8;

            SoftwareTraceTile tile = fixture.Tracer.RenderNextTile();

            Assert.InRange(tile.X, 0, 8);
            Assert.InRange(tile.Y, 0, 8);
            Assert.InRange(tile.Width, 1, 8);
            Assert.InRange(tile.Height, 1, 8);
            Assert.Same(staging, fixture.Tracer.TileRgba8);
            Assert.Equal(0, fixture.Tracer.CompletedPasses);
        }

        /// <summary>
        /// Ensures a complete pass covers each pixel once and changes SPP only at its end.
        /// </summary>
        [Fact]
        public void Complete_pass_permutation_covers_each_pixel_once() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            SoftwareTraceResolution resolution = new SoftwareTraceResolution(17, 9);
            fixture.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            bool[] visited = new bool[resolution.PixelCount];
            int tileCount = 0;

            while (fixture.Tracer.CompletedPasses == 0) {
                SoftwareTraceTile tile = fixture.Tracer.RenderNextTile();
                tileCount++;
                for (int y = tile.Y; y < tile.Y + tile.Height; y++) {
                    for (int x = tile.X; x < tile.X + tile.Width; x++) {
                        int index = (y * resolution.Width) + x;
                        Assert.False(visited[index]);
                        visited[index] = true;
                    }
                }
            }

            Assert.Equal(6, tileCount);
            for (int index = 0; index < visited.Length; index++) {
                Assert.True(visited[index]);
            }
            Assert.Equal(1, fixture.Tracer.CompletedPasses);
        }

        /// <summary>
        /// Ensures pass one starts at a different deterministic tile and repeated runs match.
        /// </summary>
        [Fact]
        public void Passes_are_deterministic_and_interleaved() {
            TraceFixture first = CreateFixture(new float3(1f, 1f, 1f));
            TraceFixture second = CreateFixture(new float3(1f, 1f, 1f));
            SoftwareTraceResolution resolution = new SoftwareTraceResolution(17, 9);
            first.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            second.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            List<int> firstTiles = new List<int>();
            List<int> secondTiles = new List<int>();

            for (int index = 0; index < 12; index++) {
                firstTiles.Add(first.Tracer.RenderNextTile().TileIndex);
                secondTiles.Add(second.Tracer.RenderNextTile().TileIndex);
            }

            Assert.Equal(firstTiles, secondTiles);
            Assert.NotEqual(firstTiles[0], firstTiles[6]);
            Assert.NotEqual(firstTiles[0] + 1, firstTiles[1]);
            for (int index = 0; index < first.Tracer.Accumulation.Length; index++) {
                Assert.Equal(first.Tracer.Accumulation[index].X, second.Tracer.Accumulation[index].X);
                Assert.Equal(first.Tracer.Accumulation[index].Y, second.Tracer.Accumulation[index].Y);
                Assert.Equal(first.Tracer.Accumulation[index].Z, second.Tracer.Accumulation[index].Z);
            }
        }

        /// <summary>
        /// Ensures accumulation stores one sample per pixel and display averages remain stable across passes.
        /// </summary>
        [Fact]
        public void Accumulation_and_display_average_are_stable_across_completed_passes() {
            TraceFixture fixture = CreateFixture(new float3(0.5f, 0.5f, 0.5f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(1, 1), CanonicalCamera(), 1f);

            fixture.Tracer.RenderNextTile();
            Assert.Equal(1, fixture.Tracer.CompletedPasses);
            Assert.Equal(0.5f, fixture.Tracer.Accumulation[0].X);
            byte firstDisplay = fixture.Tracer.TileRgba8[0];

            fixture.Tracer.RenderNextTile();
            Assert.Equal(2, fixture.Tracer.CompletedPasses);
            Assert.Equal(1f, fixture.Tracer.Accumulation[0].X);
            Assert.Equal(firstDisplay, fixture.Tracer.TileRgba8[0]);
        }

        /// <summary>
        /// Ensures ACES and sRGB conversion produces the checked midtone bytes and exposure is monotonic.
        /// </summary>
        [Fact]
        public void Tone_mapping_matches_midtone_and_exposure_contract() {
            TraceFixture midtone = CreateFixture(float3.Zero);
            midtone.Tracer.InitializeProgressive(new SoftwareTraceResolution(1, 1), CanonicalCamera(), 1f);
            midtone.Tracer.Accumulation[0] = new float3(0.5f, 0.25f, 0.5f);

            midtone.Tracer.RenderNextTile();

            Assert.Equal(206, midtone.Tracer.TileRgba8[0]);
            Assert.Equal(165, midtone.Tracer.TileRgba8[1]);
            Assert.Equal(206, midtone.Tracer.TileRgba8[2]);
            Assert.Equal(255, midtone.Tracer.TileRgba8[3]);

            TraceFixture dim = CreateFixture(float3.Zero);
            TraceFixture bright = CreateFixture(float3.Zero);
            dim.Tracer.InitializeProgressive(new SoftwareTraceResolution(1, 1), CanonicalCamera(), 1f);
            bright.Tracer.InitializeProgressive(new SoftwareTraceResolution(1, 1), CanonicalCamera(), 2f);
            dim.Tracer.Accumulation[0] = new float3(0.5f, 0.5f, 0.5f);
            bright.Tracer.Accumulation[0] = new float3(0.5f, 0.5f, 0.5f);

            dim.Tracer.RenderNextTile();
            bright.Tracer.RenderNextTile();

            Assert.True(bright.Tracer.TileRgba8[0] > dim.Tracer.TileRgba8[0]);
        }

        /// <summary>
        /// Ensures tone mapping maps negative and NaN channels to black and positive infinity to white.
        /// </summary>
        [Fact]
        public void Tone_mapping_handles_nonfinite_and_negative_accumulation() {
            TraceFixture negative = CreateFixture(float3.Zero);
            TraceFixture nan = CreateFixture(float3.Zero);
            TraceFixture largeFinite = CreateFixture(float3.Zero);
            TraceFixture positiveInfinity = CreateFixture(float3.Zero);
            SoftwareTraceResolution resolution = new SoftwareTraceResolution(1, 1);
            negative.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            nan.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            largeFinite.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            positiveInfinity.Tracer.InitializeProgressive(resolution, CanonicalCamera(), 1f);
            negative.Tracer.Accumulation[0] = new float3(-1f, -1f, -1f);
            nan.Tracer.Accumulation[0] = new float3(float.NaN, float.NaN, float.NaN);
            largeFinite.Tracer.Accumulation[0] = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            positiveInfinity.Tracer.Accumulation[0] = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

            negative.Tracer.RenderNextTile();
            nan.Tracer.RenderNextTile();
            largeFinite.Tracer.RenderNextTile();
            positiveInfinity.Tracer.RenderNextTile();

            Assert.Equal(0, negative.Tracer.TileRgba8[0]);
            Assert.Equal(0, nan.Tracer.TileRgba8[0]);
            Assert.Equal(255, largeFinite.Tracer.TileRgba8[0]);
            Assert.Equal(255, positiveInfinity.Tracer.TileRgba8[0]);
            Assert.Equal(255, positiveInfinity.Tracer.TileRgba8[1]);
            Assert.Equal(255, positiveInfinity.Tracer.TileRgba8[2]);
        }

        /// <summary>
        /// Ensures tile rows use the fixed pitch and edge tiles expose only their clipped rectangle.
        /// </summary>
        [Fact]
        public void Edge_tiles_use_fixed_pitch_and_reusable_capacity() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f);
            bool foundEdge = false;

            for (int index = 0; index < 4; index++) {
                for (int byteIndex = 0; byteIndex < fixture.Tracer.TileRgba8.Length; byteIndex++) {
                    fixture.Tracer.TileRgba8[byteIndex] = 17;
                }
                SoftwareTraceTile tile = fixture.Tracer.RenderNextTile();
                if (tile.Width < SoftwarePathTracer.TileSize || tile.Height < SoftwarePathTracer.TileSize) {
                    foundEdge = true;
                    if (tile.Width < SoftwarePathTracer.TileSize) {
                        Assert.Equal(17, fixture.Tracer.TileRgba8[tile.Width * 4]);
                    }
                    if (tile.Height < SoftwarePathTracer.TileSize) {
                        Assert.Equal(17, fixture.Tracer.TileRgba8[tile.Height * fixture.Tracer.TileRowPitch]);
                    }
                }
                Assert.Equal(32, fixture.Tracer.TileRowPitch);
                Assert.Equal(256, fixture.Tracer.TileRgba8.Length);
                Assert.True((tile.Height - 1) * fixture.Tracer.TileRowPitch + ((tile.Width - 1) * 4) + 4 <= fixture.Tracer.TileRgba8.Length);
            }

            Assert.True(foundEdge);
        }

        /// <summary>
        /// Ensures each scheduled tile is unique and the selected step is coprime with the tile count.
        /// </summary>
        [Fact]
        public void Tile_permutation_step_is_coprime_and_covers_the_grid() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(17, 9), CanonicalCamera(), 1f);
            HashSet<int> tileIndices = new HashSet<int>();

            for (int index = 0; index < 6; index++) {
                tileIndices.Add(fixture.Tracer.RenderNextTile().TileIndex);
            }

            FieldInfo stepField = typeof(SoftwarePathTracer).GetField("permutationStep", BindingFlags.Instance | BindingFlags.NonPublic);
            int step = (int)stepField.GetValue(fixture.Tracer);
            Assert.Equal(6, tileIndices.Count);
            Assert.Equal(1, GreatestCommonDivisor(step, 6));
        }

        /// <summary>
        /// Ensures the primary camera rays are finite, normalized, and pass-dependent.
        /// </summary>
        [Fact]
        public void Camera_rays_are_normalized_and_use_top_left_image_orientation() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f);

            SoftwareRay center = fixture.Tracer.CreateCameraRay(4, 4, 0);
            SoftwareRay top = fixture.Tracer.CreateCameraRay(4, 0, 0);
            SoftwareRay bottom = fixture.Tracer.CreateCameraRay(4, 8, 0);
            SoftwareRay nextPass = fixture.Tracer.CreateCameraRay(4, 4, 1);

            Assert.True(IsFinite(center.Direction));
            Assert.InRange(center.Direction.Length(), 0.9998f, 1.0002f);
            Assert.True(float3.Dot(center.Direction, CanonicalCamera().Forward) > 0.99f);
            Assert.True(top.Direction.Y > bottom.Direction.Y);
            Assert.NotEqual(BitConverter.SingleToInt32Bits(center.Direction.X), BitConverter.SingleToInt32Bits(nextPass.Direction.X));
        }

        /// <summary>
        /// Ensures tone mapping writes opaque RGBA and handles finite and non-finite values defensively.
        /// </summary>
        [Fact]
        public void Tone_mapping_clamps_black_white_and_midtones() {
            TraceFixture fixture = CreateFixture(new float3(0f, 0f, 0f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(1, 1), CanonicalCamera(), 1f);
            SoftwareTraceTile blackTile = fixture.Tracer.RenderNextTile();

            Assert.Equal(0, fixture.Tracer.TileRgba8[0]);
            Assert.Equal(0, fixture.Tracer.TileRgba8[1]);
            Assert.Equal(0, fixture.Tracer.TileRgba8[2]);
            Assert.Equal(255, fixture.Tracer.TileRgba8[3]);
            Assert.Equal(1, blackTile.Width);
            Assert.Equal(1, blackTile.Height);
        }

        /// <summary>
        /// Ensures RenderNextTile does not allocate managed memory after warmup.
        /// </summary>
        [Fact]
        public void Render_next_tile_allocates_no_managed_bytes_after_warmup() {
            TraceFixture fixture = CreateFixture(new float3(1f, 1f, 1f));
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(9, 9), CanonicalCamera(), 1f);
            fixture.Tracer.RenderNextTile();
            fixture.Tracer.RenderNextTile();

            long before = GC.GetAllocatedBytesForCurrentThread();
            fixture.Tracer.RenderNextTile();
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.Equal(before, after);
        }

        /// <summary>
        /// Creates a real BVH-backed primary-emitter fixture.
        /// </summary>
        /// <param name="emission">Emitter radiance used for every primary sample.</param>
        /// <returns>A fixture with borrowed scene state.</returns>
        static TraceFixture CreateFixture(float3 emission) {
            SoftwareTriangle[] triangles = {
                MakeTriangle(new float3(-100f, -100f, -2f), new float3(200f, 0f, 0f), new float3(0f, 200f, 0f), 0),
                MakeTriangle(new float3(100f, 100f, -2f), new float3(-200f, 0f, 0f), new float3(0f, -200f, 0f), 0)
            };
            SoftwareMaterialData[] materials = { new SoftwareMaterialData(float3.One, emission) };
            SoftwareAreaLight light = new SoftwareAreaLight(
                new float3(-100f, -100f, -2f),
                new float3(200f, 0f, 0f),
                new float3(0f, 200f, 0f),
                new float3(0f, 0f, 1f),
                40000f,
                emission,
                0,
                1);
            SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];
            SoftwarePathTracer tracer = new SoftwarePathTracer(triangles, materials, light, bvh, stack);
            return new TraceFixture(tracer, triangles, bvh, stack);
        }

        /// <summary>
        /// Creates the canonical camera used by progressive fixtures.
        /// </summary>
        /// <returns>A camera looking down negative Z.</returns>
        static SoftwareTraceCamera CanonicalCamera() {
            return new SoftwareTraceCamera(
                new float3(0f, 0f, 0f),
                new float3(0f, 0f, -1f),
                new float3(1f, 0f, 0f),
                new float3(0f, 1f, 0f),
                60f);
        }

        /// <summary>
        /// Creates and initializes a temporary tracer for exposure validation.
        /// </summary>
        /// <param name="value">Exposure candidate.</param>
        static void InitializeWithExposure(float value) {
            TraceFixture fixture = CreateFixture(float3.Zero);
            fixture.Tracer.InitializeProgressive(new SoftwareTraceResolution(1, 1), CanonicalCamera(), value);
        }

        /// <summary>
        /// Creates one compact triangle with derived geometric fields.
        /// </summary>
        /// <param name="p0">Triangle origin.</param>
        /// <param name="edge1">First edge.</param>
        /// <param name="edge2">Second edge.</param>
        /// <param name="materialIndex">Material index.</param>
        /// <returns>A compact triangle.</returns>
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
        /// Computes the greatest common divisor for the scheduling assertion.
        /// </summary>
        /// <param name="first">First positive integer.</param>
        /// <param name="second">Second positive integer.</param>
        /// <returns>The greatest common divisor.</returns>
        static int GreatestCommonDivisor(int first, int second) {
            while (second != 0) {
                int remainder = first % second;
                first = second;
                second = remainder;
            }
            return first;
        }

        /// <summary>
        /// Tests all components of one vector for finite values.
        /// </summary>
        /// <param name="value">Vector to inspect.</param>
        /// <returns>True when all components are finite.</returns>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }

        /// <summary>
        /// Stores fixture state needed to verify borrowed ownership.
        /// </summary>
        sealed class TraceFixture {
            /// <summary>Progressive tracer under test.</summary>
            public readonly SoftwarePathTracer Tracer;
            /// <summary>Exact triangles borrowed by the tracer.</summary>
            public readonly SoftwareTriangle[] Triangles;
            /// <summary>Exact BVH borrowed by the tracer.</summary>
            public readonly SoftwareBvh Bvh;
            /// <summary>Exact traversal scratch borrowed by the tracer.</summary>
            public readonly int[] Stack;

            /// <summary>
            /// Initializes one fixture.
            /// </summary>
            /// <param name="tracer">Tracer under test.</param>
            /// <param name="triangles">Borrowed triangle array.</param>
            /// <param name="bvh">Borrowed BVH.</param>
            /// <param name="stack">Borrowed traversal stack.</param>
            public TraceFixture(SoftwarePathTracer tracer, SoftwareTriangle[] triangles, SoftwareBvh bvh, int[] stack) {
                Tracer = tracer;
                Triangles = triangles;
                Bvh = bvh;
                Stack = stack;
            }
        }

        /// <summary>
        /// Injects deterministic allocation failures at one progressive allocation stage.
        /// </summary>
        sealed class ThrowingAllocator : ISoftwareTraceBufferAllocator {
            /// <summary>Whether accumulator allocation should fail.</summary>
            readonly bool failAccumulator;
            /// <summary>Whether tile allocation should fail.</summary>
            readonly bool failTile;

            /// <summary>
            /// Initializes one failure-injecting allocator.
            /// </summary>
            /// <param name="failAccumulator">Whether the accumulator stage fails.</param>
            /// <param name="failTile">Whether the tile stage fails.</param>
            public ThrowingAllocator(bool failAccumulator, bool failTile) {
                this.failAccumulator = failAccumulator;
                this.failTile = failTile;
            }

            /// <summary>
            /// Allocates or fails the requested accumulator.
            /// </summary>
            /// <param name="pixelCount">Required pixel count.</param>
            /// <returns>An accumulator array.</returns>
            public float3[] AllocateAccumulator(int pixelCount) {
                if (failAccumulator) {
                    throw new InvalidOperationException("Injected accumulator failure.");
                }
                return new float3[pixelCount];
            }

            /// <summary>
            /// Allocates or fails the requested tile staging array.
            /// </summary>
            /// <param name="byteCount">Required byte count.</param>
            /// <returns>A tile staging array.</returns>
            public byte[] AllocateTileRgba8(int byteCount) {
                if (failTile) {
                    throw new InvalidOperationException("Injected tile failure.");
                }
                return new byte[byteCount];
            }
        }
    }
}
