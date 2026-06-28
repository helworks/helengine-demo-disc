namespace city.menu.tools {
    /// <summary>
    /// Contributes the demo workflow menu items used by the city project inside the editor menu strip.
    /// </summary>
    public sealed class DemoMenuItemProvider : IEditorMenuItemProvider {
        /// <summary>
        /// Returns the contributed demo menu items.
        /// </summary>
        /// <returns>Ordered contributed demo menu items.</returns>
        public IReadOnlyList<EditorMenuItemDescriptor> GetMenuItems() {
            return [
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.regenerate-main-menu",
                    "Regenerate Main Menu...",
                    100,
                    "menu.regenerate-demo-disc-main-menu"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.generate-rendering-scenes",
                    "Generate Rendering Scenes",
                    200,
                    "menu.generate-rendering-scenes"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.migrate-physics-demo-materials",
                    "Migrate Physics Demo Materials",
                    250,
                    "menu.migrate-physics-demo-materials"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.generate-physics-scenes",
                    "Generate Physics Scenes",
                    275,
                    "menu.generate-physics-scenes"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.generate-game-scenes",
                    "Generate Game Scenes",
                    290,
                    "menu.generate-game-scenes"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.generate-physics-ds-scenes",
                    "Generate Physics DS Scenes",
                    300,
                    "menu.generate-physics-nintendo-ds-scenes")
            ];
        }
    }
}
