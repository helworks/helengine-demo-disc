using DemoDisc.menu;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Contributes the demo workflow menu items used by the city project inside the editor menu strip.
    /// Every project-authored editor command appears here, so no generation step is reachable only from the
    /// headless command line.
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
                    "demo.generate-cube-test-scene",
                    "Generate Cube Test Scene",
                    210,
                    "menu.generate-cube-test-scene"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.generate-colored-cube-grid-scene",
                    "Generate Colored Cube Grid Scene",
                    220,
                    "menu.generate-colored-cube-grid-scene"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.regenerate-ray-tracing-showcases",
                    "Regenerate Ray Tracing Showcases",
                    230,
                    "rendering.regenerate-ray-tracing-showcases"),
                new EditorMenuItemDescriptor(
                    "demo",
                    "Demo",
                    100,
                    "demo.generate-console-camera-light-instructions-blueprint",
                    "Generate Console Camera/Light Instructions Blueprint",
                    240,
                    "menu.generate-console-camera-light-instructions-blueprint"),
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
                    "demo.generate-tilt-trial-scene",
                    "Generate Tilt Trial Selector Scenes",
                    295,
                    "menu.generate-tilt-trial-scene"),
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
