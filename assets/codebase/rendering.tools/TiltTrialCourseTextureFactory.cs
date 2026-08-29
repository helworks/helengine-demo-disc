using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the authored pastel lilac grid texture used by the Tilt Trial course material.
    /// </summary>
    public sealed class TiltTrialCourseTextureFactory {
        /// <summary>
        /// Stable project-relative source texture path used by the Tilt Trial course material.
        /// </summary>
        public const string TextureRelativePath = "textures/rendering/tilt_trial/CourseLilacGrid.bmp";

        /// <summary>
        /// Stable generated texture width.
        /// </summary>
        const int TextureWidth = 256;

        /// <summary>
        /// Stable generated texture height.
        /// </summary>
        const int TextureHeight = 256;

        /// <summary>
        /// Stable grid-cell size; 16 pixels yields a 16x16 cell grid so per-object UV stretching reads far less than the previous 8x8 layout.
        /// </summary>
        const int GridCellSize = 16;

        /// <summary>
        /// Stable grid-line thickness.
        /// </summary>
        const int GridLineThickness = 2;

        /// <summary>
        /// Writes the lilac grid source texture and returns its imported texture asset id.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Imported texture asset id backing the generated source bitmap.</returns>
        public string WriteTextureAsset(
            string projectRootPath,
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            byte[] textureBytes = BuildTextureFileBytes();
            if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            }
            if (transaction == null) {
                throw new ArgumentNullException(nameof(transaction));
            }

            TextureAssetImportSettings settings = assetAuthoringService.LoadOrCreateTextureImportSettings(
                Path.Combine(assetAuthoringService.ProjectRootPath, "assets", TextureRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            string assetId = GeneratedFileTransactionWriter.WriteTexture(
                assetAuthoringService,
                transaction,
                TextureRelativePath,
                textureBytes,
                settings);
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new InvalidOperationException("Tilt Trial lilac grid texture requires one persisted imported texture asset id.");
            }

            return assetId;
        }

        /// <summary>
        /// Builds one bitmap source file for the pastel lilac grid texture.
        /// </summary>
        /// <returns>BMP file bytes for the course texture.</returns>
        static byte[] BuildTextureFileBytes() {
            int rowStride = ((TextureWidth * 3) + 3) & ~3;
            int pixelDataLength = rowStride * TextureHeight;
            const int fileHeaderLength = 14;
            const int dibHeaderLength = 40;
            int pixelDataOffset = fileHeaderLength + dibHeaderLength;
            byte[] fileBytes = new byte[pixelDataOffset + pixelDataLength];

            fileBytes[0] = (byte)'B';
            fileBytes[1] = (byte)'M';
            WriteInt32(fileBytes, 2, fileBytes.Length);
            WriteInt32(fileBytes, 10, pixelDataOffset);
            WriteInt32(fileBytes, 14, dibHeaderLength);
            WriteInt32(fileBytes, 18, TextureWidth);
            WriteInt32(fileBytes, 22, TextureHeight);
            WriteInt16(fileBytes, 26, 1);
            WriteInt16(fileBytes, 28, 24);
            WriteInt32(fileBytes, 34, pixelDataLength);

            for (int y = 0; y < TextureHeight; y++) {
                int rowOffset = pixelDataOffset + ((TextureHeight - 1 - y) * rowStride);
                for (int x = 0; x < TextureWidth; x++) {
                    ResolvePixelColor(x, y, out byte red, out byte green, out byte blue);
                    int pixelOffset = rowOffset + (x * 3);
                    fileBytes[pixelOffset] = blue;
                    fileBytes[pixelOffset + 1] = green;
                    fileBytes[pixelOffset + 2] = red;
                }
            }

            return fileBytes;
        }

        /// <summary>
        /// Resolves one lilac-grid source pixel.
        /// </summary>
        /// <param name="x">Zero-based texture x coordinate.</param>
        /// <param name="y">Zero-based texture y coordinate.</param>
        /// <param name="red">Resolved red component.</param>
        /// <param name="green">Resolved green component.</param>
        /// <param name="blue">Resolved blue component.</param>
        static void ResolvePixelColor(int x, int y, out byte red, out byte green, out byte blue) {
            float verticalBlend = TextureHeight <= 1 ? 0f : (float)y / (TextureHeight - 1);
            int backgroundRed = LerpToByte(244, 232, verticalBlend);
            int backgroundGreen = LerpToByte(236, 224, verticalBlend);
            int backgroundBlue = LerpToByte(252, 246, verticalBlend);
            int tileX = x / GridCellSize;
            int tileY = y / GridCellSize;
            int localX = x % GridCellSize;
            int localY = y % GridCellSize;
            bool isGridLine = localX < GridLineThickness
                || localY < GridLineThickness
                || localX >= GridCellSize - GridLineThickness
                || localY >= GridCellSize - GridLineThickness;

            if (isGridLine) {
                red = 216;
                green = 196;
                blue = 240;
                return;
            }

            int checkerOffset = ((tileX + tileY) & 1) == 0 ? 6 : -4;
            int centerDistance = Math.Abs(localX - (GridCellSize / 2)) + Math.Abs(localY - (GridCellSize / 2));
            int centerLift = Math.Max(0, 6 - (centerDistance / 3));
            red = ClampToByte(backgroundRed + checkerOffset + centerLift);
            green = ClampToByte(backgroundGreen + checkerOffset + centerLift);
            blue = ClampToByte(backgroundBlue + checkerOffset + (centerLift * 2));
        }

        /// <summary>
        /// Writes one 16-bit little-endian integer into the supplied byte buffer.
        /// </summary>
        /// <param name="buffer">Target byte buffer.</param>
        /// <param name="offset">Starting write offset.</param>
        /// <param name="value">Integer value to write.</param>
        static void WriteInt16(byte[] buffer, int offset, int value) {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        /// <summary>
        /// Writes one 32-bit little-endian integer into the supplied byte buffer.
        /// </summary>
        /// <param name="buffer">Target byte buffer.</param>
        /// <param name="offset">Starting write offset.</param>
        /// <param name="value">Integer value to write.</param>
        static void WriteInt32(byte[] buffer, int offset, int value) {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        /// <summary>
        /// Resolves one linear byte interpolation between two endpoints.
        /// </summary>
        /// <param name="start">Inclusive start value.</param>
        /// <param name="end">Inclusive end value.</param>
        /// <param name="t">Normalized interpolation amount.</param>
        /// <returns>Interpolated byte value.</returns>
        static int LerpToByte(int start, int end, float t) {
            return (int)MathF.Round(start + ((end - start) * t));
        }

        /// <summary>
        /// Clamps one integer channel value into byte range.
        /// </summary>
        /// <param name="value">Unclamped channel value.</param>
        /// <returns>Clamped byte value.</returns>
        static byte ClampToByte(int value) {
            return (byte)Math.Clamp(value, 0, 255);
        }
    }
}
