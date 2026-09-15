using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace city.menu.tools.tests {
    /// <summary>Verifies scene navigation through the dedicated software ray-tracing category.</summary>
    public sealed class DemoDiscSoftwarePathTracerCatalogTests {
        /// <summary>Ensures the existing Cornell Box scene is reachable only through its dedicated category.</summary>
        [Fact]
        public void Cornell_box_has_a_dedicated_ray_tracing_menu() {
            MenuDefinition definition = new DemoDiscMenuDefinitionProvider().CreateMenuDefinition();
            MenuPanelDefinition main = Assert.Single(definition.Panels, panel => panel.PanelId == "main");
            MenuItemDefinition renderingEntry = Assert.Single(main.Items, item => item.Label == "Rendering Scenes");
            Assert.Equal(MenuActionKind.OpenPanel, renderingEntry.Action.Kind);
            MenuPanelDefinition rendering = Assert.Single(definition.Panels, panel => panel.PanelId == renderingEntry.Action.TargetId);
            Assert.Equal("Rendering Scenes", rendering.Heading);
            Assert.DoesNotContain(main.Items, item => item.Action.TargetId == "ray-tracing-select");
            MenuItemDefinition category = Assert.Single(rendering.Items, item => item.Label == "Ray Tracing");
            Assert.Equal(MenuActionKind.OpenPanel, category.Action.Kind);
            MenuPanelDefinition rayTracing = Assert.Single(definition.Panels, panel => panel.PanelId == category.Action.TargetId);
            Assert.Equal("Ray Tracing", rayTracing.Heading);
            Assert.Equal(new[] { "Cornell Box", "Teapot", "Material Spheres", "Soft Shadows", "Back" }, rayTracing.Items.Select(item => item.Label));
            Assert.True(rayTracing.Items[0].Enabled);
            Assert.Equal(MenuActionKind.LoadScene, rayTracing.Items[0].Action.Kind);
            Assert.Equal("software_path_tracer", rayTracing.Items[0].Action.TargetId);
            Assert.Equal(MenuActionKind.Back, rayTracing.Items[4].Action.Kind);
            Assert.Single(definition.Panels.SelectMany(panel => panel.Items), item => item.Action.TargetId == "software_path_tracer");
            Assert.DoesNotContain(new DemoDiscSceneCatalog().CreateDemoSceneItems(), item => item.Action.TargetId == "software_path_tracer");
        }

        /// <summary>Ensures automated navigation follows the same category, scene, and return routes as the menu.</summary>
        [Fact]
        public void Helenui_profile_routes_cornell_box_through_ray_tracing() {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(FindRepositoryRoot(), "helenui", "demodisc.json")));
            JsonElement main = FindSurface(document, "surface-demodisc-main-menu");
            JsonElement renderingEntry = Assert.Single(main.GetProperty("uiNodes").EnumerateArray(), node => node.GetProperty("text").GetString() == "Rendering Scenes");
            JsonElement renderingActivate = Assert.Single(renderingEntry.GetProperty("interactions").EnumerateArray(), item => item.GetProperty("kind").GetString() == "activate");
            JsonElement renderingMenu = FindSurface(document, renderingActivate.GetProperty("targetSurfaceId").GetString());
            Assert.DoesNotContain(main.GetProperty("uiNodes").EnumerateArray(), node => node.GetProperty("text").GetString() == "Ray Tracing" || node.GetProperty("text").GetString() == "Software Ray Tracing");
            JsonElement category = Assert.Single(renderingMenu.GetProperty("uiNodes").EnumerateArray(), node => node.GetProperty("text").GetString() == "Ray Tracing");
            JsonElement activate = Assert.Single(category.GetProperty("interactions").EnumerateArray(), item => item.GetProperty("kind").GetString() == "activate");
            JsonElement menu = FindSurface(document, activate.GetProperty("targetSurfaceId").GetString());
            JsonElement[] nodes = menu.GetProperty("uiNodes").EnumerateArray().ToArray();
            Assert.Equal(new[] { "Cornell Box", "Teapot", "Material Spheres", "Soft Shadows", "Back" }, nodes.Select(node => node.GetProperty("text").GetString()));
            Assert.Equal("surface-demodisc-showcase-scene", Assert.Single(nodes[0].GetProperty("interactions").EnumerateArray(), item => item.GetProperty("kind").GetString() == "activate").GetProperty("targetSurfaceId").GetString());
            Assert.Equal("surface-demodisc-demo-scenes-menu", Assert.Single(nodes[4].GetProperty("interactions").EnumerateArray(), item => item.GetProperty("kind").GetString() == "activate").GetProperty("targetSurfaceId").GetString());
            JsonElement rendering = FindSurface(document, "surface-demodisc-demo-scenes-menu");
            Assert.DoesNotContain(rendering.GetProperty("uiNodes").EnumerateArray(), node => node.GetProperty("text").GetString() == "Software Path Tracer" || node.GetProperty("text").GetString() == "Cornell Box");
            Assert.Equal(Enumerable.Range(0, nodes.Length), nodes.Select(node => node.GetProperty("order").GetInt32()));
        }

        /// <summary>Finds exactly one named navigation surface.</summary>
        static JsonElement FindSurface(JsonDocument document, string surfaceId) {
            return Assert.Single(document.RootElement.GetProperty("surfaces").EnumerateArray(), surface => surface.GetProperty("id").GetString() == surfaceId);
        }

        /// <summary>Locates the authored profile independently of the generated test project's output directory.</summary>
        static string FindRepositoryRoot([CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (directory != null) {
                if (File.Exists(Path.Combine(directory.FullName, "helenui", "demodisc.json"))) return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate the Demo Disc repository root.");
        }
    }
}
