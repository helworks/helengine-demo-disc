using helengine;
using helengine.editor;
using System.Reflection;

namespace city.rendering.tools {
    /// <summary>
    /// Writes the deterministic six-cell bitmap atlas used by the colored-face clipping probe.
    /// </summary>
    public sealed class TiltTrialClippingProbeTextureFactory {
        /// <summary>
        /// Stable project-relative bitmap path written by the clipping probe authoring workflow.
        /// </summary>
        public const string TextureRelativePath = "textures/rendering/tilt_trial/clipping_probe_face_colors.bmp";

        /// <summary>
        /// Fixed atlas width containing three color cells and horizontal padding.
        /// </summary>
        const int TextureWidth = 128;

        /// <summary>
        /// Fixed atlas height containing two color rows and vertical padding.
        /// </summary>
        const int TextureHeight = 64;

        /// <summary>
        /// Width of each solid face-color cell.
        /// </summary>
        const int CellWidth = 32;

        /// <summary>
        /// Height of each solid face-color cell.
        /// </summary>
        const int CellHeight = 24;

        /// <summary>
        /// Horizontal padding at the atlas edges and between adjacent columns.
        /// </summary>
        const int HorizontalPadding = 8;

        /// <summary>
        /// Vertical padding above and below the cells.
        /// </summary>
        const int VerticalEdgePadding = 4;

        /// <summary>
        /// Vertical padding between the two rows of cells.
        /// </summary>
        const int VerticalRowPadding = 8;

        /// <summary>
        /// RGB colors assigned to back, front, right, left, top, and bottom faces in model face order.
        /// </summary>
        static readonly byte4[] FaceColors = [
            new byte4(255, 0, 0, 255),
            new byte4(0, 255, 0, 255),
            new byte4(0, 0, 255, 255),
            new byte4(255, 255, 0, 255),
            new byte4(255, 0, 255, 255),
            new byte4(0, 255, 255, 255)
        ];

        /// <summary>
        /// Writes the source atlas and returns the persisted imported texture asset id.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative DemoDisc project root path.</param>
        /// <returns>Imported texture asset id created by the editor's registered importers.</returns>
        public string WriteTextureAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string assetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string fullTexturePath = Path.Combine(assetsRootPath, TextureRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string textureDirectoryPath = Path.GetDirectoryName(fullTexturePath);
            if (string.IsNullOrWhiteSpace(textureDirectoryPath)) {
                throw new InvalidOperationException($"Could not resolve a texture directory for '{TextureRelativePath}'.");
            }

            Directory.CreateDirectory(textureDirectoryPath);
            byte[] textureBytes = BuildTextureFileBytes();
            if (!File.Exists(fullTexturePath) || !File.ReadAllBytes(fullTexturePath).AsSpan().SequenceEqual(textureBytes)) {
                File.WriteAllBytes(fullTexturePath, textureBytes);
            }

            AssetImportManager importManager = CreateAssetImportManager(fullProjectRootPath, assetsRootPath);
            TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(fullTexturePath);
            string assetId = settings.Importer.AssetId;
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new InvalidOperationException("The clipping probe atlas requires one persisted imported texture asset id.");
            }

            return assetId;
        }

        /// <summary>
        /// Builds the complete 24-bit BMP file for the six padded face-color cells.
        /// </summary>
        /// <returns>Serialized bitmap bytes in BMP file order.</returns>
        static byte[] BuildTextureFileBytes() {
            int rowStride = ((TextureWidth * 3) + 3) & ~3;
            int pixelDataLength = rowStride * TextureHeight;
            const int FileHeaderLength = 14;
            const int DibHeaderLength = 40;
            int pixelDataOffset = FileHeaderLength + DibHeaderLength;
            byte[] fileBytes = new byte[pixelDataOffset + pixelDataLength];

            fileBytes[0] = (byte)'B';
            fileBytes[1] = (byte)'M';
            WriteInt32(fileBytes, 2, fileBytes.Length);
            WriteInt32(fileBytes, 10, pixelDataOffset);
            WriteInt32(fileBytes, 14, DibHeaderLength);
            WriteInt32(fileBytes, 18, TextureWidth);
            WriteInt32(fileBytes, 22, TextureHeight);
            WriteInt16(fileBytes, 26, 1);
            WriteInt16(fileBytes, 28, 24);
            WriteInt32(fileBytes, 34, pixelDataLength);

            for (int y = 0; y < TextureHeight; y++) {
                int rowOffset = pixelDataOffset + ((TextureHeight - 1 - y) * rowStride);
                for (int x = 0; x < TextureWidth; x++) {
                    byte4 color = ResolveCellColor(x, y);
                    int pixelOffset = rowOffset + (x * 3);
                    fileBytes[pixelOffset] = color.Z;
                    fileBytes[pixelOffset + 1] = color.Y;
                    fileBytes[pixelOffset + 2] = color.X;
                }
            }

            return fileBytes;
        }

        /// <summary>
        /// Resolves one atlas pixel to its face color or the opaque unused border color.
        /// </summary>
        /// <param name="x">Zero-based atlas x coordinate.</param>
        /// <param name="y">Zero-based atlas y coordinate.</param>
        /// <returns>Face cell color when the coordinate is inside a solid cell; otherwise opaque black.</returns>
        static byte4 ResolveCellColor(int x, int y) {
            int firstRowStart = VerticalEdgePadding;
            int secondRowStart = firstRowStart + CellHeight + VerticalRowPadding;
            int rowIndex;
            if (y >= firstRowStart && y < firstRowStart + CellHeight) {
                rowIndex = 0;
            } else if (y >= secondRowStart && y < secondRowStart + CellHeight) {
                rowIndex = 1;
            } else {
                return new byte4(0, 0, 0, 255);
            }

            int columnIndex = (x - HorizontalPadding) / (CellWidth + HorizontalPadding);
            int cellStartX = HorizontalPadding + (columnIndex * (CellWidth + HorizontalPadding));
            if (columnIndex < 0 || columnIndex >= 3 || x < cellStartX || x >= cellStartX + CellWidth) {
                return new byte4(0, 0, 0, 255);
            }

            return FaceColors[(rowIndex * 3) + columnIndex];
        }

        /// <summary>
        /// Creates an asset import manager initialized with the editor host's registered default importers.
        /// </summary>
        /// <param name="projectRootPath">Absolute DemoDisc project root path.</param>
        /// <param name="assetsRootPath">Absolute DemoDisc assets root path.</param>
        /// <returns>Import manager prepared to persist source texture import settings.</returns>
        AssetImportManager CreateAssetImportManager(string projectRootPath, string assetsRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(assetsRootPath)) {
                throw new ArgumentException("Assets root path must be provided.", nameof(assetsRootPath));
            }

            ContentManager contentManager = new ContentManager(new HostFileSystemContentStreamSource(assetsRootPath));
            AssetImportManager importManager = new AssetImportManager(projectRootPath, contentManager);
            IReadOnlyList<IAssetImporterRegistration> importers = CreateDefaultImporters();
            for (int index = 0; index < importers.Count; index++) {
                IAssetImporterRegistration importer = importers[index];
                if (importer == null) {
                    throw new InvalidOperationException("Importer registrations must not contain null entries.");
                }

                importer.Register(importManager);
            }

            importManager.GenerateMissingImportSettings();
            return importManager;
        }

        /// <summary>
        /// Obtains the editor host's importer registrations without constructing ad hoc import behavior.
        /// </summary>
        /// <returns>Default editor-host importer registrations.</returns>
        IReadOnlyList<IAssetImporterRegistration> CreateDefaultImporters() {
            Assembly appAssembly = Assembly.Load("helengine.editor.app");
            Type importerFactoryType = appAssembly.GetType("helengine.editor.app.EditorHostImporterFactory", throwOnError: true);
            MethodInfo createDefaultMethod = importerFactoryType.GetMethod("CreateDefault", BindingFlags.Public | BindingFlags.Static);
            if (createDefaultMethod == null) {
                throw new InvalidOperationException("EditorHostImporterFactory.CreateDefault was not found.");
            }

            object result = createDefaultMethod.Invoke(null, Array.Empty<object>());
            IReadOnlyList<IAssetImporterRegistration> importers = result as IReadOnlyList<IAssetImporterRegistration>;
            if (importers == null) {
                throw new InvalidOperationException("Editor host importer factory did not return importer registrations.");
            }

            return importers;
        }

        /// <summary>
        /// Writes one 16-bit little-endian value into a BMP header buffer.
        /// </summary>
        /// <param name="buffer">Target BMP byte buffer.</param>
        /// <param name="offset">Starting header offset.</param>
        /// <param name="value">Integer value to encode.</param>
        static void WriteInt16(byte[] buffer, int offset, int value) {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        /// <summary>
        /// Writes one 32-bit little-endian value into a BMP header buffer.
        /// </summary>
        /// <param name="buffer">Target BMP byte buffer.</param>
        /// <param name="offset">Starting header offset.</param>
        /// <param name="value">Integer value to encode.</param>
        static void WriteInt32(byte[] buffer, int offset, int value) {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
    }
}
