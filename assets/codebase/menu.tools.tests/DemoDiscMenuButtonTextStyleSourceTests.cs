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
            string projectRootPath = @"C:\dev\helprojs\demodisc";
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
    }
}
