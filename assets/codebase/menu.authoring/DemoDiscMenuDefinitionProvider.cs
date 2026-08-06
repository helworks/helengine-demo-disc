namespace city.menu {
    /// <summary>
    /// Produces the canonical demo-disc menu definition used by the generated city menu scene.
    /// </summary>
    public sealed class DemoDiscMenuDefinitionProvider : IMenuDefinitionProvider {
        /// <summary>
        /// Builds the complete demo-disc menu definition.
        /// </summary>
        /// <returns>Menu definition consumed by the runtime menu host.</returns>
        public MenuDefinition CreateMenuDefinition() {
            DemoDiscMenuTheme theme = new DemoDiscMenuTheme();
            DemoDiscSceneCatalog sceneCatalog = new DemoDiscSceneCatalog();
            MenuItemDefinition[] demoSceneItems = sceneCatalog.CreateDemoSceneItems();
            MenuItemDefinition[] physicsSceneItems = sceneCatalog.CreatePhysicsSceneItems();
            MenuItemDefinition[] gameSceneItems = sceneCatalog.CreateGameSceneItems();
            MenuItemDefinition[] mainMenuItems = [
                new MenuItemDefinition("main-scenes", "Demo Scenes", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "scene-select")),
                new MenuItemDefinition("main-physics", "Physics Scenes", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "physics-select")),
                new MenuItemDefinition("main-games", "Games", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "games-select")),
                new MenuItemDefinition("main-options", "Options", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "options"))
            ];
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
                [
                    new MenuPanelDefinition(
                        "main",
                        "Main Menu",
                        6,
                        mainMenuItems),
                    new MenuPanelDefinition(
                        "scene-select",
                        "Demo Scenes",
                        4,
                        demoSceneItems),
                    new MenuPanelDefinition(
                        "physics-select",
                        "Physics Scenes",
                        4,
                        physicsSceneItems),
                    new MenuPanelDefinition(
                        "games-select",
                        "Games",
                        4,
                        gameSceneItems),
                    new MenuPanelDefinition(
                        "options",
                        "Options",
                        6,
                        [
                            new MenuItemDefinition("options-display", "Display", true, new MenuActionDefinition(MenuActionKind.None, string.Empty)),
                            new MenuItemDefinition("options-audio", "Audio", true, new MenuActionDefinition(MenuActionKind.None, string.Empty)),
                            new MenuItemDefinition("options-controls", "Controls", true, new MenuActionDefinition(MenuActionKind.None, string.Empty)),
                            new MenuItemDefinition("options-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
                        ])
                ],
                new MenuOverlayImageDefinition(
                    theme.LogoTexturePath,
                    theme.LogoWidth,
                    theme.LogoHeight,
                    theme.LogoBottomMargin,
                    theme.LogoRightMargin),
                new MenuPlatformInfoDefinition(
                    theme.PlatformInfoTopMargin,
                    theme.PlatformInfoRightMargin,
                    theme.PlatformInfoLineSpacing));
        }
    }
}
