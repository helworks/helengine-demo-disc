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
        public void Catalog_returns_generated_png_path_for_known_family_and_control() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\city");

            string relativePath = catalog.RequireControlPath("keyboard", "wasd");

            Assert.Equal("images/instructions/controls/generated/keyboard/wasd.png", relativePath);
        }

        [Fact]
        public void Catalog_returns_generated_png_paths_for_camera_stick_equivalents() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\city");

            Assert.Equal("images/instructions/controls/generated/3ds/circle_pad.png", catalog.RequireControlPath("3ds", "circle_pad"));
            Assert.Equal("images/instructions/controls/generated/psp/analog.png", catalog.RequireControlPath("psp", "analog"));
            Assert.Equal("images/instructions/controls/generated/gamecube/control_stick.png", catalog.RequireControlPath("gamecube", "control_stick"));
            Assert.Equal("images/instructions/controls/generated/wii/stick.png", catalog.RequireControlPath("wii", "stick"));
            Assert.Equal("images/instructions/controls/generated/n64/control_stick.png", catalog.RequireControlPath("n64", "control_stick"));
        }

        [Fact]
        public void Catalog_throws_for_missing_control() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\city");

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
                @"C:\dev\helprojs\city",
                "ps2",
                "r1");

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
                @"C:\dev\helprojs\city",
                "xbox360",
                "rb");

            Assert.Equal(32f / 256f, resolved.SourceRect.X, 3);
            Assert.Equal(82f / 256f, resolved.SourceRect.Y, 3);
            Assert.Equal(193f / 256f, resolved.SourceRect.Z, 3);
            Assert.Equal(93f / 256f, resolved.SourceRect.W, 3);
            Assert.Equal(new int2(78, 38), resolved.FitDisplaySizeWithin(new int2(78, 45)));
        }

        [Fact]
        public void Resolver_throws_for_unknown_platform() {
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => {
                    resolver.RequireIcon(@"C:\dev\helprojs\city", "saturn", "a");
                });

            Assert.Contains("saturn", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
