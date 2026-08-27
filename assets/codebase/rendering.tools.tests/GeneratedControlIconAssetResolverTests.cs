using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Verifies generated control-icon lookup stays manifest-driven and strict.
    /// </summary>
    public sealed class GeneratedControlIconAssetResolverTests {
        [Fact]
        public void Platform_map_defaults_windows_and_win32_to_keyboard() {
            Assert.Equal("keyboard", city.rendering.tools.GeneratedControlIconPlatformMap.ResolveFamilyId("windows"));
            Assert.Equal("keyboard", city.rendering.tools.GeneratedControlIconPlatformMap.ResolveFamilyId("win32"));
        }

        [Fact]
        public void Platform_map_uses_wii_family_for_wiiu_fallback() {
            Assert.Equal("wii", city.rendering.tools.GeneratedControlIconPlatformMap.ResolveFamilyId("wiiu"));
        }

        [Fact]
        public void Catalog_returns_generated_png_path_for_known_family_and_control() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\demodisc");

            string relativePath = catalog.RequireControlPath("keyboard", "wasd");

            Assert.Equal("images/instructions/controls/generated/keyboard/wasd.png", relativePath);
        }

        [Fact]
        public void Catalog_returns_generated_png_paths_for_camera_stick_equivalents() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\demodisc");

            Assert.Equal("images/instructions/controls/generated/3ds/circle_pad.png", catalog.RequireControlPath("3ds", "circle_pad"));
            Assert.Equal("images/instructions/controls/generated/psp/analog.png", catalog.RequireControlPath("psp", "analog"));
            Assert.Equal("images/instructions/controls/generated/gamecube/control_stick.png", catalog.RequireControlPath("gamecube", "control_stick"));
            Assert.Equal("images/instructions/controls/generated/wii/stick.png", catalog.RequireControlPath("wii", "stick"));
            Assert.Equal("images/instructions/controls/generated/n64/control_stick.png", catalog.RequireControlPath("n64", "control_stick"));
        }

        [Fact]
        public void Catalog_throws_for_missing_control() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\demodisc");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => {
                    catalog.RequireControlPath("ps2", "not-a-real-control");
                });

            Assert.Contains("ps2", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("not-a-real-control", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void Resolver_returns_generated_png_path_and_imported_texture_asset_id() {
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();

            city.rendering.tools.ResolvedControlIcon resolved = resolver.RequireIcon(
                @"C:\dev\helprojs\demodisc",
                "ps2",
                "r1",
                new TestAssetAuthoringService());

            Assert.Equal("ps2", resolved.PlatformId);
            Assert.Equal("ps2", resolved.FamilyId);
            Assert.Equal("r1", resolved.ControlId);
            Assert.Equal("images/instructions/controls/generated/ps2/r1.png", resolved.SourcePngRelativePath);
            Assert.False(string.IsNullOrWhiteSpace(resolved.ImportedTextureAssetId));
        }

        [Fact]
        public void Resolver_returns_trimmed_source_rect_and_aspect_fit_size_for_wide_icons() {
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();

            city.rendering.tools.ResolvedControlIcon resolved = resolver.RequireIcon(
                @"C:\dev\helprojs\demodisc",
                "xbox360",
                "rb",
                new TestAssetAuthoringService());

            Assert.Equal(32f / 256f, resolved.SourceRect.X, 3);
            Assert.Equal(82f / 256f, resolved.SourceRect.Y, 3);
            Assert.Equal(193f / 256f, resolved.SourceRect.Z, 3);
            Assert.Equal(93f / 256f, resolved.SourceRect.W, 3);
            Assert.Equal(new int2(78, 38), resolved.FitDisplaySizeWithin(new int2(78, 45)));
        }

        /// <summary>
        /// Ensures the DS Accept icon is authored at native OBJ size with no transparent source padding.
        /// </summary>
        [Fact]
        public void Nintendo_ds_accept_icon_is_32_pixels_and_uses_the_full_texture() {
            const string projectRootPath = @"C:\dev\helprojs\demodisc";
            const string relativePath = "images/instructions/controls/generated/ds/a.png";
            string fullPath = Path.Combine(projectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(fullPath)) {
                stream.ReadExactly(header);
            }

            int width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4));
            int height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4));
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();
            city.rendering.tools.ResolvedControlIcon resolved = resolver.RequireIcon(projectRootPath, "ds", "a", new TestAssetAuthoringService());

            Assert.Equal(32, width);
            Assert.Equal(32, height);
            Assert.Equal(new float4(0f, 0f, 1f, 1f), resolved.SourceRect);
        }

        [Fact]
        public void Resolver_throws_for_unknown_platform() {
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => {
                    resolver.RequireIcon(@"C:\dev\helprojs\demodisc", "saturn", "a", new TestAssetAuthoringService());
                });

            Assert.Contains("saturn", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Supplies the current typed texture settings needed by resolver tests without constructing editor import internals.
        /// </summary>
        sealed class TestAssetAuthoringService : IEditorProjectAssetAuthoringService {
            /// <summary>
            /// Loads deterministic current texture settings for one test source.
            /// </summary>
            public TextureAssetImportSettings LoadOrCreateTextureImportSettings(string sourcePath) {
                return new TextureAssetImportSettings {
                    Importer = new AssetImporterSettings {
                        ImporterId = "gdi",
                        AssetId = "test-imported-texture"
                    }
                };
            }

            /// <summary>
            /// Accepts deterministic test settings without writing editor files.
            /// </summary>
            public void SaveTextureImportSettings(string sourcePath, TextureAssetImportSettings settings) { }

            /// <summary>
            /// Model settings are not used by these tests.
            /// </summary>
            public ModelAssetImportSettings LoadOrCreateModelImportSettings(string sourcePath) => throw new NotSupportedException();

            /// <summary>
            /// Audio settings are not used by these tests.
            /// </summary>
            public AudioAssetImportSettings LoadOrCreateAudioImportSettings(string sourcePath) => throw new NotSupportedException();

            /// <summary>
            /// Sectioned settings are not used by these tests.
            /// </summary>
            public AssetImportSettings LoadOrCreateSectionedImportSettings(string sourcePath) => throw new NotSupportedException();

            /// <summary>
            /// Model settings are not used by these tests.
            /// </summary>
            public void SaveModelImportSettings(string sourcePath, ModelAssetImportSettings settings) => throw new NotSupportedException();

            /// <summary>
            /// Audio settings are not used by these tests.
            /// </summary>
            public void SaveAudioImportSettings(string sourcePath, AudioAssetImportSettings settings) => throw new NotSupportedException();

            /// <summary>
            /// Sectioned settings are not used by these tests.
            /// </summary>
            public void SaveSectionedImportSettings(string sourcePath, AssetImportSettings settings) => throw new NotSupportedException();

            /// <summary>
            /// Runtime model resolution is not used by these tests.
            /// </summary>
            public RuntimeModel ResolveRuntimeModel(string sourcePath) => throw new NotSupportedException();

            /// <summary>
            /// Font resolution is not used by these tests.
            /// </summary>
            public FontAsset ResolveFontAsset(string sourcePath) => throw new NotSupportedException();

            /// <summary>
            /// Texture resolution is not used by these tests.
            /// </summary>
            public TextureAsset ResolveTextureAsset(string sourcePath) => throw new NotSupportedException();

            /// <summary>
            /// Scene reference resolution is not used by these tests.
            /// </summary>
            public EditorSceneAssetReferenceResolver CreateSceneAssetReferenceResolver() => throw new NotSupportedException();

            /// <summary>
            /// Imported texture lookup is not used by these tests.
            /// </summary>
            public bool TryLoadImportedTextureAsset(string assetId, out TextureAsset textureAsset) {
                textureAsset = null;
                return false;
            }
        }
    }
}
