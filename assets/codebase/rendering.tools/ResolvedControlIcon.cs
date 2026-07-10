namespace city.rendering.tools {
    /// <summary>
    /// Describes one generated control icon resolved from the manifest and editor import pipeline.
    /// </summary>
    public sealed class ResolvedControlIcon {
        public string PlatformId { get; init; } = string.Empty;
        public string FamilyId { get; init; } = string.Empty;
        public string ControlId { get; init; } = string.Empty;
        public string SourcePngRelativePath { get; init; } = string.Empty;
        public string ImportedTextureAssetId { get; init; } = string.Empty;
        public float4 SourceRect { get; init; } = new float4(0f, 0f, 1f, 1f);

        public int2 FitDisplaySizeWithin(int2 maxDisplaySize) {
            if (maxDisplaySize.X < 1 || maxDisplaySize.Y < 1) {
                throw new ArgumentOutOfRangeException(nameof(maxDisplaySize), "Display bounds must be positive.");
            } else if (SourceRect.Z <= 0f || SourceRect.W <= 0f) {
                throw new InvalidOperationException("Resolved control icon source rect must be positive.");
            }

            double sourceAspectRatio = SourceRect.Z / SourceRect.W;
            double constrainedWidth = maxDisplaySize.X;
            double constrainedHeight = Math.Round(constrainedWidth / sourceAspectRatio);
            if (constrainedHeight > maxDisplaySize.Y) {
                constrainedHeight = maxDisplaySize.Y;
                constrainedWidth = Math.Round(constrainedHeight * sourceAspectRatio);
            }

            return new int2(
                Math.Max(1, (int)constrainedWidth),
                Math.Max(1, (int)constrainedHeight));
        }
    }
}
