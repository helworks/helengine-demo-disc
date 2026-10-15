using DemoDisc.menu;

namespace DemoDisc.EditorTools {
    /// <summary>Builds the full desktop menu in the authoring module.</summary>
    public sealed class DemoDiscDesktopMenuDefinitionFactory {
        /// <summary>Creates the complete desktop menu independently of the editor host compilation symbols.</summary>
        public MenuDefinition Create() {
            MenuDefinition original = new DemoDiscMenuDefinitionProvider().CreateMenuDefinition();
            DemoDiscSceneCatalog catalog = new DemoDiscSceneCatalog();
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physics = catalog.CreateDesktopPhysicsSceneEntries();
            MenuItemDefinition[] physicsItems = new MenuItemDefinition[physics.Count + 1];
            for (int index = 0; index < physics.Count; index++) {
                DemoDiscPhysicsSceneEntry entry = physics[index];
                physicsItems[index] = new MenuItemDefinition(entry.MenuItemId, entry.DisplayName, true, new MenuActionDefinition(MenuActionKind.LoadScene, entry.SceneId));
            }
            physicsItems[physics.Count] = new MenuItemDefinition("physics-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty));
            MenuItemDefinition[] gameItems = new[] {
                new MenuItemDefinition("games-tilt-trial", "Tilt Trial", true, new MenuActionDefinition(MenuActionKind.LoadScene, "tilt_trial")),
                new MenuItemDefinition("games-zombislayer", "Zombislayer", true, new MenuActionDefinition(MenuActionKind.LoadScene, "zombislayer")),
                new MenuItemDefinition("games-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
            MenuPanelDefinition[] panels = new MenuPanelDefinition[original.Panels.Length];
            for (int index = 0; index < panels.Length; index++) {
                MenuPanelDefinition panel = original.Panels[index];
                panels[index] = panel.PanelId == "physics-select" ? new MenuPanelDefinition(panel.PanelId, panel.Heading, panel.VisibleItemCount, physicsItems)
                    : panel.PanelId == "games-select" ? new MenuPanelDefinition(panel.PanelId, panel.Heading, panel.VisibleItemCount, gameItems) : panel;
            }
            return new MenuDefinition(original.Title, original.Subtitle, original.InitialPanelId, original.TitleFontPath, original.BodyFontPath, original.CarouselFontPath,
                original.BackgroundColor, original.SurfaceColor, original.SurfaceBorderColor, original.AccentColor, original.AccentSecondaryColor, original.TextColor, original.MutedTextColor,
                panels, original.OverlayImage, original.PlatformInfoOverlay);
        }

    }
}
