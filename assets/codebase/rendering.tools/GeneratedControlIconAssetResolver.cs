using helengine;
using helengine.editor;
using System.IO.Compression;

namespace city.rendering.tools {
    /// <summary>
    /// Resolves generated control icons into both source paths and imported texture asset ids.
    /// </summary>
    public sealed class GeneratedControlIconAssetResolver {
        public ResolvedControlIcon RequireIcon(string projectRootPath, string platformId, string controlId) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string familyId = GeneratedControlIconPlatformMap.ResolveFamilyId(platformId);
            GeneratedControlIconCatalog catalog = GeneratedControlIconCatalog.Load(fullProjectRootPath);
            string relativePath = catalog.RequireControlPath(familyId, controlId);

            string fullAssetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string fullSourcePath = Path.Combine(fullAssetsRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullSourcePath)) {
                throw new InvalidOperationException($"Generated control icon source '{relativePath}' was not found for platform '{platformId}' and control '{controlId}'.");
            }

            AssetImportManager importManager = CreateImportManager(fullProjectRootPath);
            TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(fullSourcePath);
            if (settings == null || settings.Importer == null || string.IsNullOrWhiteSpace(settings.Importer.AssetId)) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' did not produce a persisted imported texture asset id.");
            }

            return new ResolvedControlIcon {
                PlatformId = platformId,
                FamilyId = familyId,
                ControlId = controlId,
                SourcePngRelativePath = relativePath,
                ImportedTextureAssetId = settings.Importer.AssetId,
                SourceRect = LoadTrimmedSourceRect(fullProjectRootPath, relativePath)
            };
        }

        static float4 LoadTrimmedSourceRect(string fullProjectRootPath, string relativePath) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            string fullSourcePath = Path.Combine(
                Path.Combine(fullProjectRootPath, "assets"),
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            using FileStream stream = new FileStream(fullSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            PngAlphaBounds bounds = ReadPngAlphaBounds(stream, relativePath);
            if (bounds.MaxX < bounds.MinX || bounds.MaxY < bounds.MinY) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' did not contain any opaque pixels.");
            }

            return new float4(
                (float)bounds.MinX / bounds.Width,
                (float)bounds.MinY / bounds.Height,
                (float)(bounds.MaxX - bounds.MinX + 1) / bounds.Width,
                (float)(bounds.MaxY - bounds.MinY + 1) / bounds.Height);
        }

        static PngAlphaBounds ReadPngAlphaBounds(Stream stream, string relativePath) {
            ArgumentNullException.ThrowIfNull(stream);
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            byte[] pngSignature = [(byte)0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
            byte[] signature = new byte[pngSignature.Length];
            ReadExactly(stream, signature, 0, signature.Length, relativePath);
            for (int index = 0; index < pngSignature.Length; index++) {
                if (signature[index] != pngSignature[index]) {
                    throw new InvalidOperationException($"Generated control icon '{relativePath}' is not a valid PNG file.");
                }
            }

            int width = 0;
            int height = 0;
            int bitDepth = 0;
            int colorType = -1;
            int interlaceMethod = -1;
            using MemoryStream idatStream = new MemoryStream();
            bool sawHeader = false;
            bool sawEnd = false;
            byte[] chunkHeader = new byte[8];
            while (!sawEnd && TryReadExactly(stream, chunkHeader, 0, chunkHeader.Length)) {
                int chunkLength = ReadBigEndianInt32(chunkHeader, 0, relativePath);
                string chunkType = System.Text.Encoding.ASCII.GetString(chunkHeader, 4, 4);
                if (chunkLength < 0) {
                    throw new InvalidOperationException($"Generated control icon '{relativePath}' contains an invalid PNG chunk length.");
                }

                byte[] chunkData = new byte[chunkLength];
                ReadExactly(stream, chunkData, 0, chunkLength, relativePath);
                SkipBytes(stream, 4, relativePath);

                if (string.Equals(chunkType, "IHDR", StringComparison.Ordinal)) {
                    if (chunkLength != 13) {
                        throw new InvalidOperationException($"Generated control icon '{relativePath}' contains an invalid IHDR chunk.");
                    }

                    width = ReadBigEndianInt32(chunkData, 0, relativePath);
                    height = ReadBigEndianInt32(chunkData, 4, relativePath);
                    bitDepth = chunkData[8];
                    colorType = chunkData[9];
                    interlaceMethod = chunkData[12];
                    sawHeader = true;
                } else if (string.Equals(chunkType, "IDAT", StringComparison.Ordinal)) {
                    idatStream.Write(chunkData, 0, chunkData.Length);
                } else if (string.Equals(chunkType, "IEND", StringComparison.Ordinal)) {
                    sawEnd = true;
                }
            }

            if (!sawHeader || width < 1 || height < 1) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' is missing a valid PNG header.");
            } else if (!sawEnd) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' is missing the PNG end chunk.");
            } else if (bitDepth != 8) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' must use 8-bit PNG channels.");
            } else if (interlaceMethod != 0) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' must not use interlaced PNG encoding.");
            }

            int bytesPerPixel = colorType switch {
                6 => 4,
                4 => 2,
                2 => 3,
                0 => 1,
                _ => throw new InvalidOperationException($"Generated control icon '{relativePath}' uses unsupported PNG color type '{colorType}'.")
            };
            int stride = checked(width * bytesPerPixel);
            byte[] previousRow = new byte[stride];
            byte[] currentRow = new byte[stride];
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            idatStream.Position = 0;
            using ZLibStream zlibStream = new ZLibStream(idatStream, CompressionMode.Decompress, leaveOpen: true);
            for (int y = 0; y < height; y++) {
                int filterType = zlibStream.ReadByte();
                if (filterType < 0) {
                    throw new InvalidOperationException($"Generated control icon '{relativePath}' ended before every PNG scanline could be decoded.");
                }

                ReadExactly(zlibStream, currentRow, 0, stride, relativePath);
                ApplyPngFilter(currentRow, previousRow, bytesPerPixel, filterType, relativePath);

                for (int x = 0; x < width; x++) {
                    if (ResolveAlpha(currentRow, x, bytesPerPixel, colorType) <= 0) {
                        continue;
                    }

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }

                byte[] swap = previousRow;
                previousRow = currentRow;
                currentRow = swap;
            }

            return new PngAlphaBounds(width, height, minX, minY, maxX, maxY);
        }

        static void ApplyPngFilter(byte[] currentRow, byte[] previousRow, int bytesPerPixel, int filterType, string relativePath) {
            ArgumentNullException.ThrowIfNull(currentRow);
            ArgumentNullException.ThrowIfNull(previousRow);
            if (bytesPerPixel < 1) {
                throw new ArgumentOutOfRangeException(nameof(bytesPerPixel));
            }

            switch (filterType) {
                case 0:
                    return;
                case 1:
                    for (int index = 0; index < currentRow.Length; index++) {
                        int left = index >= bytesPerPixel ? currentRow[index - bytesPerPixel] : 0;
                        currentRow[index] = unchecked((byte)(currentRow[index] + left));
                    }
                    return;
                case 2:
                    for (int index = 0; index < currentRow.Length; index++) {
                        currentRow[index] = unchecked((byte)(currentRow[index] + previousRow[index]));
                    }
                    return;
                case 3:
                    for (int index = 0; index < currentRow.Length; index++) {
                        int left = index >= bytesPerPixel ? currentRow[index - bytesPerPixel] : 0;
                        int up = previousRow[index];
                        currentRow[index] = unchecked((byte)(currentRow[index] + ((left + up) >> 1)));
                    }
                    return;
                case 4:
                    for (int index = 0; index < currentRow.Length; index++) {
                        int left = index >= bytesPerPixel ? currentRow[index - bytesPerPixel] : 0;
                        int up = previousRow[index];
                        int upLeft = index >= bytesPerPixel ? previousRow[index - bytesPerPixel] : 0;
                        currentRow[index] = unchecked((byte)(currentRow[index] + PaethPredictor(left, up, upLeft)));
                    }
                    return;
                default:
                    throw new InvalidOperationException($"Generated control icon '{relativePath}' uses unsupported PNG filter type '{filterType}'.");
            }
        }

        static int ResolveAlpha(byte[] currentRow, int x, int bytesPerPixel, int colorType) {
            int offset = x * bytesPerPixel;
            return colorType switch {
                6 => currentRow[offset + 3],
                4 => currentRow[offset + 1],
                2 => 255,
                0 => 255,
                _ => 0
            };
        }

        static int PaethPredictor(int left, int up, int upLeft) {
            int predictor = left + up - upLeft;
            int distanceLeft = Math.Abs(predictor - left);
            int distanceUp = Math.Abs(predictor - up);
            int distanceUpLeft = Math.Abs(predictor - upLeft);
            if (distanceLeft <= distanceUp && distanceLeft <= distanceUpLeft) {
                return left;
            } else if (distanceUp <= distanceUpLeft) {
                return up;
            }

            return upLeft;
        }

        static int ReadBigEndianInt32(byte[] bytes, int offset, string relativePath) {
            ArgumentNullException.ThrowIfNull(bytes);
            if (offset < 0 || offset + 4 > bytes.Length) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' contains an invalid PNG integer payload.");
            }

            return (bytes[offset] << 24) |
                   (bytes[offset + 1] << 16) |
                   (bytes[offset + 2] << 8) |
                   bytes[offset + 3];
        }

        static void SkipBytes(Stream stream, int byteCount, string relativePath) {
            ArgumentNullException.ThrowIfNull(stream);
            if (byteCount < 0) {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            byte[] buffer = new byte[Math.Min(byteCount, 256)];
            int remaining = byteCount;
            while (remaining > 0) {
                int chunkSize = Math.Min(remaining, buffer.Length);
                ReadExactly(stream, buffer, 0, chunkSize, relativePath);
                remaining -= chunkSize;
            }
        }

        static void ReadExactly(Stream stream, byte[] buffer, int offset, int count, string relativePath) {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(buffer);

            if (!TryReadExactly(stream, buffer, offset, count)) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' ended unexpectedly while decoding PNG data.");
            }
        }

        static bool TryReadExactly(Stream stream, byte[] buffer, int offset, int count) {
            int totalRead = 0;
            while (totalRead < count) {
                int bytesRead = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (bytesRead <= 0) {
                    return false;
                }

                totalRead += bytesRead;
            }

            return true;
        }

        readonly record struct PngAlphaBounds(int Width, int Height, int MinX, int MinY, int MaxX, int MaxY);

        static AssetImportManager CreateImportManager(string fullProjectRootPath) {
            try {
                return GeneratedAuthoringSceneWriteService.CreateGeneratedSceneAssetImportManager(fullProjectRootPath);
            } catch (FileNotFoundException) {
                // Tests do not load the editor app assembly, but committed texture sidecars are enough
                // to recover the imported asset ids that scene authoring persists.
                string fullAssetsRootPath = Path.Combine(fullProjectRootPath, "assets");
                ContentManager assetContentManager = new ContentManager(new HostFileSystemContentStreamSource(fullAssetsRootPath));
                EditorContentManagerConfiguration.ConfigureEditorContentManager(assetContentManager);
                return new AssetImportManager(fullProjectRootPath, assetContentManager);
            }
        }
    }
}
