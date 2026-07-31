namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies that generated menu button labels use the shared purple outline style.
    /// </summary>
    public sealed class DemoDiscMenuButtonTextStyleSourceTests {
        /// <summary>
    /// Ensures both standard and handheld button label factories assign the darker purple surface-border outline at size two.
        /// </summary>
        [Fact]
        public void Menu_button_labels_use_darker_purple_outline_size_two() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string standardFactorySource = File.ReadAllText(Path.Combine(
                projectRootPath,
                "assets",
                "codebase",
                "menu.tools",
                "DemoDiscStandardMainMenuSceneFactory.cs"));
            string handheldFactorySource = File.ReadAllText(Path.Combine(
                projectRootPath,
                "assets",
                "codebase",
                "menu.tools",
                "DemoDiscHandheldMainMenuSceneFactory.cs"));
            string outlineAssignment = "definition.SurfaceBorderColor,\n                2f";

            Assert.Contains("OutlineColor = outlineColor", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("OutlineScale = outlineScale", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains(outlineAssignment, standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("OutlineColor = outlineColor", handheldFactorySource, StringComparison.Ordinal);
            Assert.Contains("OutlineScale = outlineScale", handheldFactorySource, StringComparison.Ordinal);
            Assert.Contains(outlineAssignment, handheldFactorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the standard selected menu button retains its purple fill while exposing the teal secondary accent at its border.
        /// </summary>
        [Fact]
        public void Standard_menu_selected_button_uses_the_teal_secondary_accent_for_its_border() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string standardFactorySource = File.ReadAllText(Path.Combine(
                projectRootPath,
                "assets",
                "codebase",
                "menu.tools",
                "DemoDiscStandardMainMenuSceneFactory.cs"));

            Assert.Contains("byte4 selectedFillColor = definition.AccentColor;", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("byte4 selectedBorderColor = definition.AccentSecondaryColor;", standardFactorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the standard menu authors Helena's identity line at the bottom-left of the generated viewport.
        /// </summary>
        [Fact]
        public void Standard_menu_authors_the_helen_of_code_footer_identity() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string standardFactorySource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu.tools", "DemoDiscStandardMainMenuSceneFactory.cs"));

            Assert.Contains("MADE BY HELENA / HELEN OF CODE", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("CreateFooterIdentityEntity(generatedRootEntity, definition)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("SetAnchorDistances(left: 0f, bottom: 8f)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("new int2(DemoMenuLayout.CanvasWidth, 42)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("new float3(DemoMenuLayout.CanvasWidth, 2f, 0.2f)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("DemoDiscFooterIdentityTopBorder", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("DemoDiscFooterIdentityBottomBorder", standardFactorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the footer identity begins outside the right edge and receives the component that scrolls it continuously left.
        /// </summary>
        [Fact]
        public void Standard_menu_authors_a_continuous_footer_identity_marquee() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string standardFactorySource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu.tools", "DemoDiscStandardMainMenuSceneFactory.cs"));
            string marqueeComponentSource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu", "FooterIdentityMarqueeComponent.cs"));

            Assert.Contains("new float3(DemoMenuLayout.CanvasWidth, 2f, 0.2f)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("new FooterIdentityMarqueeComponent", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("TextEntityReference = CreateEntityReference(footerTextEntity)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("PixelsPerSecond = 70f", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("ReferenceCanvasFitComponent", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("CalculateScale()", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("CalculatePosition", marqueeComponentSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the marquee scales its reset and exit geometry into the active viewport together with its movement speed.
        /// </summary>
        [Fact]
        public void Footer_marquee_scales_its_runtime_geometry_to_the_viewport() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string marqueeComponentSource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu", "FooterIdentityMarqueeComponent.cs"));

            Assert.Contains("float stripWidth = (float)(StripWidth * viewportScale);", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("float textWidth = (float)(TextWidth * viewportScale);", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("if (nextPositionX + textWidth <= 0f)", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("nextPositionX = stripWidth;", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("new float3(stripWidth, TextEntity.LocalPosition.Y, TextEntity.LocalPosition.Z)", marqueeComponentSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the standard menu authors subtle animated grid and scanline layers behind its interactive visuals.
        /// </summary>
        [Fact]
        public void Standard_menu_authors_animated_grid_and_scanline_background() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string standardFactorySource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu.tools", "DemoDiscStandardMainMenuSceneFactory.cs"));
            string motionComponentSource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu", "MenuBackgroundMotionComponent.cs"));

            Assert.Contains("CreateAnimatedBackgroundEntity(generatedRootEntity, definition)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("DemoDiscAnimatedBackgroundGrid", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("DemoDiscAnimatedBackgroundScanlines", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("new MenuBackgroundMotionComponent", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("GridEntityReference = CreateEntityReference(gridEntity)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("ScanlineEntityReference = CreateEntityReference(scanlineEntity)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("GridPixelsPerSecond", motionComponentSource, StringComparison.Ordinal);
            Assert.Contains("ScanlinePixelsPerSecond", motionComponentSource, StringComparison.Ordinal);
            Assert.Contains("GridPixelsPerSecond = 0.6f", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("ScanlinePixelsPerSecond = 0.2f", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("LogoBottomMargin => 56", File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu", "DemoDiscMenuTheme.cs")), StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the standard menu presents its fixed title at top-right while the footer retains its maker signature and appends live platform metadata.
        /// </summary>
        [Fact]
        public void Standard_menu_authors_title_and_platform_marquee_copy() {
            string projectRootPath = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT") ?? @"C:\dev\helprojs\demodisc";
            string standardFactorySource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu.tools", "DemoDiscStandardMainMenuSceneFactory.cs"));
            string marqueeComponentSource = File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "menu", "FooterIdentityMarqueeComponent.cs"));

            Assert.Contains("\"HELENGINE\"", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("\"DEMO DISC\"", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("new float3(-300f, 0f, 0.1f)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("new float3(-280f, 64f, 0.1f)", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("definition.SurfaceBorderColor, 2f", standardFactorySource, StringComparison.Ordinal);
            Assert.Contains("MADE BY HELENA / HELEN OF CODE /", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("Core.Instance.PlatformInfo.Name", marqueeComponentSource, StringComparison.Ordinal);
            Assert.Contains("Core.Instance.PlatformInfo.Version", marqueeComponentSource, StringComparison.Ordinal);
        }
    }
}
