using System;
using System.Reflection;
using city.rendering;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies finite deterministic radiance samples through the real compact BVH.
    /// </summary>
    public sealed class SoftwarePathKernelTests {
        /// <summary>
        /// Ensures a primary ray directly seeing an emitter returns its emission.
        /// </summary>
        [Fact]
        public void Primary_emitter_hit_returns_emission() {
            TraceFixture fixture = CreateDiffuseAndLight(new float3(1f, 1f, 1f));
            SoftwareRay ray = new SoftwareRay(new float3(0f, 0f, 3f), new float3(0f, 0f, -1f));

            float3 result = fixture.Tracer.TraceSample(ref ray, 0, 0, 0);

            Assert.Equal(fixture.LightEmission.X, result.X);
            Assert.Equal(fixture.LightEmission.Y, result.Y);
            Assert.Equal(fixture.LightEmission.Z, result.Z);
            Assert.Equal(1L, fixture.Tracer.RayCount);
        }

        /// <summary>
        /// Ensures an unobstructed diffuse hit receives positive finite direct light.
        /// </summary>
        [Fact]
        public void Unobstructed_diffuse_hit_receives_finite_direct_light() {
            TraceFixture fixture = CreateDiffuseAndLight(new float3(1f, 1f, 1f));
            SoftwareRay ray = DiffuseCameraRay();

            float3 result = fixture.Tracer.TraceSample(ref ray, 2, 3, 0);

            Assert.True(IsFinite(result));
            Assert.True(result.X > 0f && result.Y > 0f && result.Z > 0f);
            Assert.True(fixture.Tracer.RayCount >= 2L);
            Assert.Equal(0L, fixture.Tracer.NonFiniteSampleCount);
        }

        /// <summary>
        /// Ensures a blocker covering every light sample removes the direct contribution.
        /// </summary>
        [Fact]
        public void Blocker_between_diffuse_surface_and_light_removes_direct_light() {
            TraceFixture clear = CreateDiffuseAndLight(new float3(1f, 1f, 1f));
            TraceFixture blocked = CreateDiffuseAndLight(new float3(1f, 1f, 1f), true);
            SoftwareRay clearRay = DiffuseCameraRay();
            SoftwareRay blockedRay = DiffuseCameraRay();

            float3 clearResult = clear.Tracer.TraceSample(ref clearRay, 2, 3, 0);
            float3 blockedResult = blocked.Tracer.TraceSample(ref blockedRay, 2, 3, 0);

            Assert.True(clearResult.X > 0f);
            Assert.Equal(0f, blockedResult.X);
            Assert.Equal(0f, blockedResult.Y);
            Assert.Equal(0f, blockedResult.Z);
            Assert.True(blocked.Tracer.RayCount > clear.Tracer.RayCount);
        }

        /// <summary>
        /// Ensures diffuse albedo transfers only its corresponding neutral-light channel.
        /// </summary>
        [Fact]
        public void Diffuse_albedo_transfers_red_and_green_channels() {
            TraceFixture red = CreateDiffuseAndLight(new float3(1f, 0f, 0f));
            TraceFixture green = CreateDiffuseAndLight(new float3(0f, 1f, 0f));
            SoftwareRay redRay = DiffuseCameraRay();
            SoftwareRay greenRay = DiffuseCameraRay();

            float3 redResult = red.Tracer.TraceSample(ref redRay, 2, 3, 0);
            float3 greenResult = green.Tracer.TraceSample(ref greenRay, 2, 3, 0);

            Assert.True(redResult.X > 0f);
            Assert.Equal(0f, redResult.Y);
            Assert.Equal(0f, redResult.Z);
            Assert.True(greenResult.Y > 0f);
            Assert.Equal(0f, greenResult.X);
            Assert.Equal(0f, greenResult.Z);
        }

        /// <summary>
        /// Ensures diffuse-bounce emitter hits do not add emission a second time.
        /// </summary>
        [Fact]
        public void Diffuse_bounce_emitter_hit_does_not_double_count_emission() {
            TraceFixture largeEmitter = CreateBounceFixture(true);
            TraceFixture escapingEmitter = CreateBounceFixture(false);
            SoftwareRay firstRay = DiffuseCameraRay();
            SoftwareRay secondRay = DiffuseCameraRay();

            float3 first = largeEmitter.Tracer.TraceSample(ref firstRay, 2, 3, 0);
            float3 second = escapingEmitter.Tracer.TraceSample(ref secondRay, 2, 3, 0);

            Assert.True(largeEmitter.Tracer.RayCount >= 3L);
            Assert.True(escapingEmitter.Tracer.RayCount >= 3L);
            Assert.Equal(second.X, first.X);
            Assert.Equal(second.Y, first.Y);
            Assert.Equal(second.Z, first.Z);
        }

        /// <summary>
        /// Ensures the scalar loop has a hard four-diffuse-hit ray bound.
        /// </summary>
        [Fact]
        public void Trace_terminates_after_at_most_four_diffuse_bounces() {
            TraceFixture fixture = CreateClosedDiffuseFixture();
            SoftwareRay ray = DiffuseCameraRay();

            float3 result = fixture.Tracer.TraceSample(ref ray, 5, 7, 0);

            Assert.True(IsFinite(result));
            Assert.InRange(fixture.Tracer.RayCount, 1L, 9L);
        }

        /// <summary>
        /// Ensures invalid primary identities and directions are rejected before counters change.
        /// </summary>
        [Fact]
        public void Invalid_primary_inputs_are_rejected_before_tracing() {
            TraceFixture fixture = CreateDiffuseAndLight(new float3(1f, 1f, 1f));
            SoftwareRay nanRay = new SoftwareRay(float3.Zero, new float3(float.NaN, 0f, 1f));
            SoftwareRay zeroRay = new SoftwareRay(float3.Zero, float3.Zero);
            SoftwareRay nonUnitRay = new SoftwareRay(float3.Zero, new float3(0f, 0f, -2f));

            Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Tracer.TraceSample(ref nanRay, 0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Tracer.TraceSample(ref zeroRay, 0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Tracer.TraceSample(ref nonUnitRay, 0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Tracer.TraceSample(ref nanRay, -1, 0, 0));
            Assert.Equal(0L, fixture.Tracer.RayCount);
        }

        /// <summary>
        /// Ensures finite inputs that overflow radiance discard one sample and expose no partial value.
        /// </summary>
        [Fact]
        public void Non_finite_radiance_discards_one_sample() {
            TraceFixture fixture = CreateDiffuseAndLight(new float3(float.MaxValue, float.MaxValue, float.MaxValue), emissionStrength: float.MaxValue);
            SoftwareRay ray = DiffuseCameraRay();

            float3 result = fixture.Tracer.TraceSample(ref ray, 1, 2, 0);

            Assert.Equal(float3.Zero.X, result.X);
            Assert.Equal(float3.Zero.Y, result.Y);
            Assert.Equal(float3.Zero.Z, result.Z);
            Assert.Equal(1L, fixture.Tracer.NonFiniteSampleCount);
        }

        /// <summary>
        /// Ensures consecutive traces allocate no per-sample managed objects after warmup.
        /// </summary>
        [Fact]
        public void Trace_sample_loop_allocates_no_managed_bytes() {
            TraceFixture fixture = CreateDiffuseAndLight(new float3(1f, 1f, 1f));
            SoftwareRay warmupRay = DiffuseCameraRay();
            fixture.Tracer.TraceSample(ref warmupRay, 2, 3, 0);
            fixture.Tracer.TraceSample(ref warmupRay, 2, 3, 1);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 16; index++) {
                SoftwareRay ray = DiffuseCameraRay();
                fixture.Tracer.TraceSample(ref ray, index, 3, 0);
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.Equal(before, after);
        }

        /// <summary>
        /// Ensures the tracer retains and reuses the caller traversal array for deterministic samples.
        /// </summary>
        [Fact]
        public void Consecutive_samples_reuse_traversal_scratch_and_are_deterministic() {
            TraceFixture fixture = CreateDiffuseAndLight(new float3(1f, 1f, 1f));
            SoftwareRay firstRay = DiffuseCameraRay();
            SoftwareRay secondRay = DiffuseCameraRay();

            float3 first = fixture.Tracer.TraceSample(ref firstRay, 2, 3, 4);
            float3 second = fixture.Tracer.TraceSample(ref secondRay, 2, 3, 4);

            Assert.Equal(first.X, second.X);
            Assert.Equal(first.Y, second.Y);
            Assert.Equal(first.Z, second.Z);
            FieldInfo stackField = typeof(SoftwarePathTracer).GetField("traversalStack", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.Same(fixture.Stack, stackField.GetValue(fixture.Tracer));
        }

        /// <summary>
        /// Stores one fully built kernel fixture and its owned traversal scratch.
        /// </summary>
        sealed class TraceFixture {
            /// <summary>Tracer under test.</summary>
            public readonly SoftwarePathTracer Tracer;
            /// <summary>Caller-owned traversal scratch supplied to the tracer.</summary>
            public readonly int[] Stack;
            /// <summary>Emission used by the fixture light.</summary>
            public readonly float3 LightEmission;

            /// <summary>
            /// Initializes one trace fixture.
            /// </summary>
            /// <param name="tracer">Tracer under test.</param>
            /// <param name="stack">Traversal scratch.</param>
            /// <param name="lightEmission">Fixture emission.</param>
            public TraceFixture(SoftwarePathTracer tracer, int[] stack, float3 lightEmission) {
                Tracer = tracer;
                Stack = stack;
                LightEmission = lightEmission;
            }
        }

        /// <summary>
        /// Creates a diffuse floor and rectangular emitter, with an optional full blocker.
        /// </summary>
        /// <param name="diffuse">Diffuse floor color.</param>
        /// <param name="blocked">Whether a slab covers every light sample.</param>
        /// <param name="emissionStrength">Emitter strength per channel.</param>
        /// <returns>A real BVH-backed tracer fixture.</returns>
        static TraceFixture CreateDiffuseAndLight(float3 diffuse, bool blocked = false, float emissionStrength = 1f) {
            float3 emission = new float3(emissionStrength, emissionStrength, emissionStrength);
            SoftwareTriangle[] triangles = new SoftwareTriangle[blocked ? 6 : 4];
            triangles[0] = MakeTriangle(new float3(-3f, -3f, 0f), new float3(6f, 0f, 0f), new float3(0f, 6f, 0f), 0);
            triangles[1] = MakeTriangle(new float3(3f, 3f, 0f), new float3(-6f, 0f, 0f), new float3(0f, -6f, 0f), 0);
            triangles[2] = MakeTriangle(new float3(-3f, -3f, 2f), new float3(6f, 0f, 0f), new float3(0f, 6f, 0f), 1);
            triangles[3] = MakeTriangle(new float3(3f, 3f, 2f), new float3(-6f, 0f, 0f), new float3(0f, -6f, 0f), 1);
            if (blocked) {
                triangles[4] = MakeTriangle(new float3(-3f, -3f, 1f), new float3(6f, 0f, 0f), new float3(0f, 6f, 0f), 2);
                triangles[5] = MakeTriangle(new float3(3f, 3f, 1f), new float3(-6f, 0f, 0f), new float3(0f, -6f, 0f), 2);
            }

            SoftwareMaterialData[] materials = {
                new SoftwareMaterialData(diffuse, float3.Zero),
                new SoftwareMaterialData(float3.One, emission),
                new SoftwareMaterialData(float3.Zero, float3.Zero)
            };
            SoftwareAreaLight light = new SoftwareAreaLight(
                new float3(-3f, -3f, 2f),
                new float3(6f, 0f, 0f),
                new float3(0f, 6f, 0f),
                new float3(0f, 0f, -1f),
                36f,
                emission,
                2,
                3);
            return BuildFixture(triangles, materials, light);
        }

        /// <summary>
        /// Creates matched large-emitter and escaping-emitter geometry for double-count sensitivity.
        /// </summary>
        /// <param name="largeEmitter">Whether the emitter triangles cover all cosine bounce directions.</param>
        /// <returns>A real BVH-backed tracer fixture.</returns>
        static TraceFixture CreateBounceFixture(bool largeEmitter) {
            float3 emission = new float3(2f, 2f, 2f);
            SoftwareTriangle[] triangles = new SoftwareTriangle[4];
            triangles[0] = MakeTriangle(new float3(-1f, -1f, 0f), new float3(2f, 0f, 0f), new float3(0f, 2f, 0f), 0);
            triangles[1] = MakeTriangle(new float3(1f, 1f, 0f), new float3(-2f, 0f, 0f), new float3(0f, -2f, 0f), 0);
            float extent = largeEmitter ? 100f : 0.1f;
            triangles[2] = MakeTriangle(new float3(-extent, -extent, 2f), new float3(2f * extent, 0f, 0f), new float3(0f, 2f * extent, 0f), 1);
            triangles[3] = MakeTriangle(new float3(extent, extent, 2f), new float3(-2f * extent, 0f, 0f), new float3(0f, -2f * extent, 0f), 1);
            SoftwareMaterialData[] materials = {
                new SoftwareMaterialData(float3.One, float3.Zero),
                new SoftwareMaterialData(float3.One, emission)
            };
            SoftwareAreaLight light = new SoftwareAreaLight(
                new float3(-100f, -100f, 2f),
                new float3(200f, 0f, 0f),
                new float3(0f, 200f, 0f),
                new float3(0f, 0f, -1f),
                40000f,
                emission,
                2,
                3);
            return BuildFixture(triangles, materials, light);
        }

        /// <summary>
        /// Creates a finite diffuse fixture whose path still exercises the bounded loop.
        /// </summary>
        /// <returns>A real BVH-backed tracer fixture.</returns>
        static TraceFixture CreateClosedDiffuseFixture() {
            return CreateDiffuseAndLight(float3.One, false);
        }

        /// <summary>
        /// Builds a tracer over the exact triangle array supplied to its real BVH.
        /// </summary>
        /// <param name="triangles">Compact scene triangles.</param>
        /// <param name="materials">Compact material data.</param>
        /// <param name="light">Explicit rectangular light.</param>
        /// <returns>A real BVH-backed tracer fixture.</returns>
        static TraceFixture BuildFixture(SoftwareTriangle[] triangles, SoftwareMaterialData[] materials, SoftwareAreaLight light) {
            SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];
            SoftwarePathTracer tracer = new SoftwarePathTracer(triangles, materials, light, bvh, stack);
            return new TraceFixture(tracer, stack, light.Emission);
        }

        /// <summary>
        /// Creates the camera ray used to hit the diffuse floor from above.
        /// </summary>
        /// <returns>A normalized downward ray.</returns>
        static SoftwareRay DiffuseCameraRay() {
            return new SoftwareRay(new float3(0f, 0f, 0.5f), new float3(0f, 0f, -1f));
        }

        /// <summary>
        /// Creates one compact triangle and computes all derived fields.
        /// </summary>
        /// <param name="p0">First corner.</param>
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
        /// Tests all components of a vector for finite values.
        /// </summary>
        /// <param name="value">Vector to inspect.</param>
        /// <returns>True when every component is finite.</returns>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }
    }
}
