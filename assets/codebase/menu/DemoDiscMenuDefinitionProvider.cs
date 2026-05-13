namespace city.menu {
    /// <summary>
    /// Produces the first-pass demo-disc menu definition used by the generated city menu scene.
    /// </summary>
    public sealed class DemoDiscMenuDefinitionProvider : IMenuDefinitionProvider {
        /// <summary>
        /// Builds the complete demo-disc menu definition.
        /// </summary>
        /// <returns>Menu definition consumed by the runtime menu host.</returns>
        public MenuDefinition CreateMenuDefinition() {
            DemoDiscMenuTheme theme = new DemoDiscMenuTheme();
            DemoDiscSceneCatalog sceneCatalog = new DemoDiscSceneCatalog();
            return new MenuDefinition(
                string.Empty,
                string.Empty,
                "main",
                theme.TitleFontPath,
                theme.BodyFontPath,
                theme.BackgroundColor,
                theme.SurfaceColor,
                theme.SurfaceBorderColor,
                theme.AccentColor,
                theme.AccentSecondaryColor,
                theme.TextColor,
                theme.MutedTextColor,
                new[] {
                    new MenuPanelDefinition(
                        "main",
                        "Main Menu",
                        "Pick a destination or peek at the menu shell.",
                        6,
                        new[] {
                            new MenuItemDefinition("main-scenes", "Select Scene", "Browse the curated demo-disc lineup.", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "scene-select")),
                            new MenuItemDefinition("main-options", "Options", "Preview the reusable options shell layout.", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "options"))
                        }),
                    new MenuPanelDefinition(
                        "scene-select",
                        "Select Scene",
                        "Every entry here is explicitly curated and ordered from city-side code.",
                        4,
                        sceneCatalog.CreateSceneItems()),
                    new MenuPanelDefinition(
                        "options",
                        "Options",
                        "Polished shell for future settings categories.",
                        6,
                        new[] {
                            new MenuItemDefinition("options-display", "Display", "Placeholder row for future video settings.", true, new MenuActionDefinition(MenuActionKind.None, string.Empty)),
                            new MenuItemDefinition("options-audio", "Audio", "Placeholder row for future volume settings.", true, new MenuActionDefinition(MenuActionKind.None, string.Empty)),
                            new MenuItemDefinition("options-controls", "Controls", "Placeholder row for future input remapping.", true, new MenuActionDefinition(MenuActionKind.None, string.Empty)),
                            new MenuItemDefinition("options-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
                        })
                });
        }
    }
}
