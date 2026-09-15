using city.rendering;
using helengine;

namespace city.gameplay.tests {
    /// <summary>Checks ideal reflection and dielectric transport against analytic directions and radiance.</summary>
    public sealed class SoftwareSpecularScatteringTests {
        /// <summary>A mirror preserves tangential direction and reverses the normal component.</summary>
        [Fact]
        public void Mirror_reflects_at_equal_angles() {
            float3 result = SoftwareSpecularScattering.Reflect(new float3(0.6f, -0.8f, 0f), new float3(0f, 1f, 0f));
            AssertNear(new float3(0.6f, 0.8f, 0f), result);
        }

        /// <summary>Air-to-glass transmission bends toward the normal and applies radiance transport scaling.</summary>
        [Fact]
        public void Glass_entry_obeys_snell_and_radiance_scaling() {
            bool transmitted = SoftwareSpecularScattering.SampleDielectric(new float3(0.6f, -0.8f, 0f), new float3(0f, 1f, 0f), 1.5f, 0.99f, out float3 direction, out float weight);
            Assert.True(transmitted);
            AssertNear(new float3(0.4f, -(float)Math.Sqrt(0.84), 0f), direction);
            Assert.InRange(weight, 0.44443f, 0.44446f);
        }

        /// <summary>Leaving a parallel slab restores direction and cancels the two eta-squared factors.</summary>
        [Fact]
        public void Glass_exit_restores_direction_and_radiance() {
            SoftwareSpecularScattering.SampleDielectric(new float3(0.6f, -0.8f, 0f), new float3(0f, 1f, 0f), 1.5f, 0.99f, out float3 inside, out float entryWeight);
            bool transmitted = SoftwareSpecularScattering.SampleDielectric(inside, new float3(0f, -1f, 0f), 1.5f, 0.99f, out float3 outside, out float exitWeight);
            Assert.True(transmitted);
            AssertNear(new float3(0.6f, -0.8f, 0f), outside);
            Assert.InRange(entryWeight * exitWeight, 0.99999f, 1.00001f);
        }

        /// <summary>Beyond the critical angle a ray inside glass reflects regardless of its sample.</summary>
        [Fact]
        public void Glass_total_internal_reflection_is_finite() {
            bool transmitted = SoftwareSpecularScattering.SampleDielectric(new float3(0.8f, 0.6f, 0f), new float3(0f, 1f, 0f), 1.5f, 0.99f, out float3 direction, out float weight);
            Assert.False(transmitted);
            AssertNear(new float3(0.8f, -0.6f, 0f), direction);
            Assert.Equal(1f, weight);
        }

        /// <summary>Normal-incidence air/glass reflection has the analytic four-percent probability.</summary>
        [Theory]
        [InlineData(0.039f, false)]
        [InlineData(0.041f, true)]
        public void Glass_uses_fresnel_branch_probability(float sample, bool expectedTransmission) {
            bool transmitted = SoftwareSpecularScattering.SampleDielectric(new float3(0f, -1f, 0f), new float3(0f, 1f, 0f), 1.5f, sample, out float3 direction, out float weight);
            Assert.Equal(expectedTransmission, transmitted);
            AssertNear(new float3(0f, expectedTransmission ? -1f : 1f, 0f), direction);
        }

        /// <summary>Equal refractive indices never create a false reflection, including grazing incidence.</summary>
        [Fact]
        public void Equal_indices_pass_through_at_grazing_angles() {
            float3 incoming = new float3(1f, 0f, 0f);
            Assert.True(SoftwareSpecularScattering.SampleDielectric(incoming, new float3(0f, 1f, 0f), 1f, 0f, out float3 direction, out float weight));
            AssertNear(incoming, direction);
            Assert.Equal(1f, weight);
        }

        /// <summary>Very high finite indices still produce a finite Fresnel reflection at normal incidence.</summary>
        [Fact]
        public void Extreme_finite_index_reflects_without_overflow() {
            bool transmitted = SoftwareSpecularScattering.SampleDielectric(new float3(0f, 1f, 0f), new float3(0f, 1f, 0f), float.MaxValue, 0.5f, out float3 direction, out float weight);
            Assert.False(transmitted);
            AssertNear(new float3(0f, -1f, 0f), direction);
            Assert.Equal(1f, weight);
        }
        /// <summary>Invalid refractive indices are rejected before a ray is scattered.</summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        public void Invalid_refractive_index_is_rejected(float ior) {
            Assert.Throws<ArgumentOutOfRangeException>(() => SoftwareSpecularScattering.SampleDielectric(new float3(0f, -1f, 0f), new float3(0f, 1f, 0f), ior, 0.5f, out _, out _));
        }

        /// <summary>Emission reached after a perfect mirror contributes with the mirror tint.</summary>
        [Fact]
        public void Tracer_sees_emitter_in_tinted_mirror() {
            SoftwareTriangle[] triangles = new SoftwareTriangle[4];
            AddQuad(triangles, 0, 0f, true, 0);
            AddQuad(triangles, 2, 2f, false, 1);
            SoftwareMaterialData[] materials = {
                new SoftwareMaterialData(float3.Zero, float3.Zero, SoftwareMaterialKind.Mirror, new float3(0.2f, 0.5f, 1f), 1.5f),
                new SoftwareMaterialData(float3.Zero, new float3(10f, 10f, 10f))
            };
            SoftwarePathTracer tracer = CreateTracer(triangles, materials, 2, 2f, false);
            SoftwareRay ray = new SoftwareRay(new float3(0f, 0f, 1f), new float3(0f, 0f, -1f));
            AssertNear(new float3(2f, 5f, 10f), tracer.TraceSample(ref ray, 0, 0, 0));
            Assert.Equal(2, tracer.RayCount);
            Assert.Equal(0, tracer.NonFiniteSampleCount);
        }

        /// <summary>Two glass boundaries transmit the emitter without self-intersections or lost emission.</summary>
        [Fact]
        public void Tracer_sees_emitter_through_glass_slab_deterministically() {
            SoftwareTriangle[] triangles = new SoftwareTriangle[6];
            AddQuad(triangles, 0, 0f, true, 0);
            AddQuad(triangles, 2, -1f, false, 0);
            AddQuad(triangles, 4, -2f, true, 1);
            SoftwareMaterialData[] materials = {
                new SoftwareMaterialData(float3.Zero, float3.Zero, SoftwareMaterialKind.Glass, float3.One, 1.5f),
                new SoftwareMaterialData(float3.Zero, new float3(10f, 10f, 10f))
            };
            SoftwarePathTracer tracer = CreateTracer(triangles, materials, 4, -2f, true);
            SoftwareRay ray = new SoftwareRay(new float3(0f, 0f, 1f), new float3(0f, 0f, -1f));
            int pass = 0;
            while (SoftwarePathSampler.Sample01(0, 0, pass, 0, 4) < 0.04f || SoftwarePathSampler.Sample01(0, 0, pass, 1, 4) < 0.04f) pass++;
            float3 first = tracer.TraceSample(ref ray, 0, 0, pass);
            float3 second = tracer.TraceSample(ref ray, 0, 0, pass);
            AssertNear(new float3(10f, 10f, 10f), first);
            Assert.Equal(first, second);
            Assert.Equal(6, tracer.RayCount);
            Assert.Equal(0, tracer.NonFiniteSampleCount);
        }

        /// <summary>Creates an ordinary BVH-backed tracer with the fixture's emissive rectangle.</summary>
        static SoftwarePathTracer CreateTracer(SoftwareTriangle[] triangles, SoftwareMaterialData[] materials, int emitterIndex, float emitterZ, bool positiveNormal) {
            float3 corner = positiveNormal ? new float3(-2f, -2f, emitterZ) : new float3(-2f, 2f, emitterZ);
            SoftwareAreaLight light = new SoftwareAreaLight(corner, new float3(4f, 0f, 0f), new float3(0f, positiveNormal ? 4f : -4f, 0f), new float3(0f, 0f, positiveNormal ? 1f : -1f), 16f, new float3(10f, 10f, 10f), emitterIndex, emitterIndex + 1);
            return new SoftwarePathTracer(triangles, materials, light, SoftwareBvh.Build(triangles), new int[SoftwareBvh.TraversalStackCapacity]);
        }

        /// <summary>Adds an outward-wound rectangle perpendicular to the primary ray.</summary>
        static void AddQuad(SoftwareTriangle[] triangles, int index, float z, bool positiveNormal, int material) {
            float3 corner = positiveNormal ? new float3(-2f, -2f, z) : new float3(-2f, 2f, z);
            float3 edge1 = new float3(4f, 0f, 0f);
            float3 edge2 = new float3(0f, positiveNormal ? 4f : -4f, 0f);
            triangles[index] = MakeTriangle(corner, edge1, edge2, material);
            triangles[index + 1] = MakeTriangle(corner + edge1 + edge2, -edge1, -edge2, material);
        }

        /// <summary>Builds validated compact triangle bounds and a unit geometric normal.</summary>
        static SoftwareTriangle MakeTriangle(float3 corner, float3 edge1, float3 edge2, int material) {
            float3 p1 = corner + edge1;
            float3 p2 = corner + edge2;
            return new SoftwareTriangle(corner, edge1, edge2, float3.Normalize(float3.Cross(edge1, edge2)), material, (corner + p1 + p2) / 3f, float3.Min(corner, float3.Min(p1, p2)), float3.Max(corner, float3.Max(p1, p2)));
        }

        /// <summary>Compares analytic vectors with a tolerance suitable for native single-precision arithmetic.</summary>
        static void AssertNear(float3 expected, float3 actual) {
            Assert.InRange(Math.Abs(expected.X - actual.X), 0f, 0.00002f);
            Assert.InRange(Math.Abs(expected.Y - actual.Y), 0f, 0.00002f);
            Assert.InRange(Math.Abs(expected.Z - actual.Z), 0f, 0.00002f);
        }
    }
}
