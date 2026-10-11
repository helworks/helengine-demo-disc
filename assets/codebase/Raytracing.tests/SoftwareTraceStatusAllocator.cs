using DemoDisc.Raytracing;
using helengine;

namespace DemoDisc.Raytracing.tests {
    /// <summary>Returns an explicit null at a selected progressive allocation boundary instead of throwing, exercising the fatal-profile compatible status path.</summary>
    public sealed class SoftwareTraceStatusAllocator : ISoftwareTraceBufferAllocator {
        /// <summary>Selects which allocation returns null.</summary>
        readonly bool failAccumulator;

        /// <summary>Configures the failed allocation stage.</summary>
        /// <param name="failAccumulator">True fails accumulation; false fails the reusable tile.</param>
        public SoftwareTraceStatusAllocator(bool failAccumulator) {
            this.failAccumulator = failAccumulator;
        }

        /// <summary>Returns an owned accumulator or the explicit failed allocation status.</summary>
        /// <param name="pixelCount">Required accumulator elements.</param>
        /// <returns>Fresh accumulator or null.</returns>
        public float3[] AllocateAccumulator(int pixelCount) {
            return failAccumulator ? null : new float3[pixelCount];
        }

        /// <summary>Returns the explicit failed tile allocation status.</summary>
        /// <param name="byteCount">Required reusable tile byte count.</param>
        /// <returns>Null indicating allocation did not complete.</returns>
        public byte[] AllocateTileRgba8(int byteCount) {
            return null;
        }
    }
}
