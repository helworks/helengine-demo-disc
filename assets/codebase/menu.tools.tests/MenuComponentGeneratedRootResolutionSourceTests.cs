namespace city.tests {
    /// <summary>
    /// Verifies the runtime menu binder resolves the generated menu subtree by its runtime panel-component subtree instead of assuming the menu root has exactly one child.
    /// </summary>
    public sealed class MenuComponentGeneratedRootResolutionSourceTests {
        const string MenuComponentSourcePath = @"C:\dev\helprojs\city\assets\codebase\menu\MenuComponent.cs";

        /// <summary>
        /// Ensures menu runtime binding remains compatible with additional menu-root helper children such as the shared looping music entity.
        /// </summary>
        [Fact]
        public void Menu_component_resolves_generated_root_by_panel_subtree() {
            string source = File.ReadAllText(MenuComponentSourcePath);

            Assert.Contains("ContainsComponentInSubtree<MenuPanelComponent>", source, StringComparison.Ordinal);
            Assert.DoesNotContain("if (rootEntity.Children.Count == 1) {\r\n                return rootEntity.Children[0];\r\n            }\r\n\r\n            return null;", source, StringComparison.Ordinal);
        }
    }
}
