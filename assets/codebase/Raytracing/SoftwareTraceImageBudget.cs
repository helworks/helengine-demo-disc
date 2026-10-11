using System;

namespace DemoDisc.Raytracing {
    /// <summary>Budgets image buffers independently of model, scene and BVH memory; a rejected budget is reported before any presentation or progressive allocation.</summary>
    public static class SoftwareTraceImageBudget {
        /// <summary>Default PS1 image-buffer ceiling, allowing 128x96 accumulation, RGBA presentation, native 16-bit shadow and one tile.</summary>
        public const long DefaultPs1Bytes = 256L * 1024L;

        /// <summary>Computes the exact retained PS1 image bytes. This estimate excludes scene/BVH arrays and transient model loads.</summary>
        /// <param name="resolution">Validated image dimensions.</param>
        /// <returns>Accumulator plus RGBA source, native 16-bit shadow and reusable tile.</returns>
        public static long ComputePs1ImageBytes(SoftwareTraceResolution resolution) {
            long shadowStrideWords = ((long)resolution.Width + 1L) & ~1L;
            long shadowBytes = checked(shadowStrideWords * resolution.Height * 2L);
            long presentationBytes = checked((long)resolution.PixelCount * 4L);
            return checked(resolution.AccumulatorBytes + presentationBytes + shadowBytes + SoftwarePathTracer.TileRgba8Bytes);
        }

        /// <summary>Checks only the PS1 image budget. Other platforms retain their established resolution and allocation behavior.</summary>
        /// <param name="resolution">Requested image dimensions.</param>
        /// <param name="platformName">Current runtime platform name.</param>
        /// <param name="ps1ImageBufferBudgetBytes">Explicit positive PS1 image-buffer ceiling.</param>
        /// <param name="failureMessage">Stable budget diagnostic on rejection, otherwise empty.</param>
        /// <returns>Whether image allocation may proceed.</returns>
        public static bool TryValidate(SoftwareTraceResolution resolution, string platformName, long ps1ImageBufferBudgetBytes, out string failureMessage) {
            failureMessage = string.Empty;
            if (!string.Equals(platformName, "ps1", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            if (ps1ImageBufferBudgetBytes <= 0L) {
                failureMessage = "PS1 image-buffer budget must be positive.";
                return false;
            }
            if (resolution.Width <= 0 || resolution.Height <= 0 || resolution.Width > 256 || resolution.Height > 256) {
                failureMessage = "PS1 software trace dimensions must be between 1 and 256 pixels.";
                return false;
            }
            if (ComputePs1ImageBytes(resolution) > ps1ImageBufferBudgetBytes) {
                failureMessage = "Software path tracing image buffers exceed the configured PS1 budget.";
                return false;
            }
            return true;
        }
    }
}
