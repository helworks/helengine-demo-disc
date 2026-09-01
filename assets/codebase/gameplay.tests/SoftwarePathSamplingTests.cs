using System;
using System.Reflection;
using city.rendering;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies stateless deterministic software path sampling.
    /// </summary>
    public sealed class SoftwarePathSamplingTests {
        /// <summary>
        /// Ensures identical hash keys produce bit-identical samples.
        /// </summary>
        [Fact]
        public void Sample01_is_bit_identical_for_identical_keys() {
            float first = SoftwarePathSampler.Sample01(13, 7, 11, 2, 3);
            float second = SoftwarePathSampler.Sample01(13, 7, 11, 2, 3);

            Assert.Equal(BitConverter.SingleToInt32Bits(first), BitConverter.SingleToInt32Bits(second));
        }

        /// <summary>
        /// Ensures each sampler key changes a fixed representative sample.
        /// </summary>
        [Fact]
        public void Sample01_changes_when_each_key_changes() {
            float baseline = SoftwarePathSampler.Sample01(13, 7, 11, 2, 3);

            Assert.NotEqual(baseline, SoftwarePathSampler.Sample01(14, 7, 11, 2, 3));
            Assert.NotEqual(baseline, SoftwarePathSampler.Sample01(13, 8, 11, 2, 3));
            Assert.NotEqual(baseline, SoftwarePathSampler.Sample01(13, 7, 12, 2, 3));
            Assert.NotEqual(baseline, SoftwarePathSampler.Sample01(13, 7, 11, 3, 3));
            Assert.NotEqual(baseline, SoftwarePathSampler.Sample01(13, 7, 11, 2, 4));
        }

        /// <summary>
        /// Ensures a deterministic range of signed keys stays finite and half-open.
        /// </summary>
        [Fact]
        public void Sample01_stays_finite_in_half_open_unit_interval() {
            for (int index = 0; index < 4096; index++) {
                float value = SoftwarePathSampler.Sample01(index * 17 - 1000, index * 7 - 300, index, index % 4, index % 5);
                Assert.True(float.IsFinite(value));
                Assert.InRange(value, 0f, BitDecrementOne());
            }
        }

        /// <summary>
        /// Ensures the sampler has no mutable per-pixel or RNG state fields.
        /// </summary>
        [Fact]
        public void Sampler_retains_no_mutable_rng_state() {
            FieldInfo[] fields = typeof(SoftwarePathSampler).GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields) {
                Assert.True(field.IsLiteral, $"Sampler field {field.Name} must be a compile-time hash constant.");
            }
        }

        /// <summary>
        /// Ensures cosine samples are finite, unit length, and above axis-aligned and tilted normals.
        /// </summary>
        [Fact]
        public void Cosine_samples_stay_in_the_supplied_hemisphere() {
            float3[] normals = {
                new float3(0f, 1f, 0f),
                new float3(0f, 0f, 1f),
                float3.Normalize(new float3(0.3f, 0.8f, -0.2f))
            };

            foreach (float3 normal in normals) {
                for (int index = 0; index < 64; index++) {
                    float3 direction = SoftwarePathSampler.SampleCosineHemisphere(normal, Sample(index), Sample(index + 1));
                    Assert.True(IsFinite(direction));
                    Assert.InRange(direction.Length(), 0.9998f, 1.0002f);
                    Assert.True(float3.Dot(direction, normal) >= -0.00001f);
                }
            }
        }

        /// <summary>
        /// Ensures cosine samples cover more than one direction, including central and grazing samples.
        /// </summary>
        [Fact]
        public void Cosine_samples_are_not_constant() {
            float3 normal = new float3(0f, 1f, 0f);
            float3 center = SoftwarePathSampler.SampleCosineHemisphere(normal, 0.0001f, 0.25f);
            float3 horizon = SoftwarePathSampler.SampleCosineHemisphere(normal, 0.9999f, 0.75f);

            Assert.True(float3.Dot(center, normal) > 0.99f);
            Assert.True(float3.Dot(horizon, normal) < 0.05f);
            Assert.NotEqual(center.X, horizon.X);
        }

        /// <summary>
        /// Ensures area-light samples use the exact affine rectangle formula.
        /// </summary>
        [Fact]
        public void Area_light_sample_matches_rectangle_affine_coordinates() {
            SoftwareAreaLight light = new SoftwareAreaLight(
                new float3(1f, 2f, 3f),
                new float3(4f, 0f, 0f),
                new float3(0f, 5f, 0f),
                new float3(0f, 0f, -1f),
                20f,
                new float3(3f, 4f, 5f),
                0,
                1);

            float3 sample = SoftwarePathSampler.SampleAreaLight(ref light, 0.25f, 0.8f);

            Assert.Equal(2f, sample.X);
            Assert.Equal(6f, sample.Y);
            Assert.Equal(3f, sample.Z);
        }

        /// <summary>
        /// Ensures invalid public sampler inputs are rejected at the helper boundary.
        /// </summary>
        [Fact]
        public void Public_sampler_helpers_reject_invalid_inputs() {
            Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePathSampler.SampleCosineHemisphere(float3.Zero, 0.2f, 0.3f));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePathSampler.SampleCosineHemisphere(new float3(0f, 1f, 0f), -0.1f, 0.3f));
            Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePathSampler.SampleCosineHemisphere(new float3(0f, 1f, 0f), 0.2f, 1f));

            SoftwareAreaLight light = new SoftwareAreaLight(
                float3.Zero,
                new float3(1f, 0f, 0f),
                new float3(0f, 1f, 0f),
                new float3(0f, 0f, 1f),
                1f,
                float3.One,
                0,
                1);
            Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePathSampler.SampleAreaLight(ref light, 1f, 0.2f));
        }

        /// <summary>
        /// Computes the largest representable float below one for a range assertion.
        /// </summary>
        /// <returns>The greatest single precision value less than one.</returns>
        static float BitDecrementOne() {
            return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(1f) - 1);
        }

        /// <summary>
        /// Computes one deterministic sample used by the cosine fixture.
        /// </summary>
        /// <param name="index">Fixture sample index.</param>
        /// <returns>A value in the sampler's half-open unit interval.</returns>
        static float Sample(int index) {
            return SoftwarePathSampler.Sample01(index, 19, 3, 0, index + 5);
        }

        /// <summary>
        /// Tests all components of one vector for finite values.
        /// </summary>
        /// <param name="value">Vector to inspect.</param>
        /// <returns>True when every component is finite.</returns>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }
    }
}
