using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies that the software path tracer is exposed by the shared Demo Disc scene catalog and HelenUI profile.
    /// </summary>
    public sealed class DemoDiscSoftwarePathTracerCatalogTests {
        /// <summary>
        /// Ensures the software path tracer is the final rendering scene before the rendering menu back item.
        /// </summary>
        [Fact]
        public void Software_path_tracer_is_catalogued_between_shadow_theater_and_back() {
            MenuItemDefinition[] items = new city.menu.DemoDiscSceneCatalog().CreateDemoSceneItems();
            string[] expectedItemIds = {
                "scene-cube-test",
                "scene-colored-cube-grid",
                "scene-textured-cube-grid",
                "scene-axis-test",
                "scene-axis-test-2",
                "scene-matrix-render",
                "scene-directional-shadow-plaza",
                "scene-pbr-material-gallery",
                "scene-pbr-textured-showcase",
                "scene-pbr-shadow-theater",
                "scene-software-path-tracer",
                "scene-back"
            };

            Assert.Equal(expectedItemIds, items.Select(item => item.ItemId).ToArray());

            MenuItemDefinition softwarePathTracer = Assert.Single(items, item => item.ItemId == "scene-software-path-tracer");
            Assert.Equal("Software Path Tracer", softwarePathTracer.Label);
            Assert.True(softwarePathTracer.Enabled);
            Assert.Equal(MenuActionKind.LoadScene, softwarePathTracer.Action.Kind);
            Assert.Equal("software_path_tracer", softwarePathTracer.Action.TargetId);
        }

        /// <summary>
        /// Ensures the shared HelenUI profile recognizes and presents the software path tracer rendering scene.
        /// </summary>
        [Fact]
        public void Helenui_profile_recognizes_and_presents_the_software_path_tracer() {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(FindRepositoryRoot(), "helenui", "demodisc.json")));
            JsonElement renderingMenu = FindSurface(document, "surface-demodisc-demo-scenes-menu");
            JsonElement renderingRecognition = FindClue(renderingMenu, "demodisc-rendering-catalog-entries");
            JsonElement renderingTexts = renderingRecognition.GetProperty("params").GetProperty("texts");
            Assert.Equal(1, renderingTexts.EnumerateArray().Count(text => text.GetString() == "Software Path Tracer"));

            JsonElement[] renderingNodes = renderingMenu.GetProperty("uiNodes").EnumerateArray().ToArray();
            JsonElement softwarePathTracerNode = Assert.Single(renderingNodes, node => node.GetProperty("id").GetString() == "node-demodisc-rendering-software-path-tracer");
            JsonElement pbrShadowTheaterNode = Assert.Single(renderingNodes, node => node.GetProperty("id").GetString() == "node-demodisc-rendering-pbr-shadow-theater");
            JsonElement backNode = Assert.Single(renderingNodes, node => node.GetProperty("id").GetString() == "node-demodisc-rendering-back");

            Assert.Equal("menu_item", softwarePathTracerNode.GetProperty("type").GetString());
            Assert.Equal("Software Path Tracer", softwarePathTracerNode.GetProperty("name").GetString());
            Assert.Equal("Software Path Tracer", softwarePathTracerNode.GetProperty("text").GetString());
            Assert.Equal(10, softwarePathTracerNode.GetProperty("order").GetInt32());

            JsonElement[] interactions = softwarePathTracerNode.GetProperty("interactions").EnumerateArray().ToArray();
            JsonElement previousInteraction = Assert.Single(interactions, interaction => interaction.GetProperty("kind").GetString() == "move_previous");
            Assert.Equal("node-demodisc-rendering-software-path-tracer-previous", previousInteraction.GetProperty("id").GetString());
            Assert.False(previousInteraction.GetProperty("isDefault").GetBoolean());
            JsonElement nextInteraction = Assert.Single(interactions, interaction => interaction.GetProperty("kind").GetString() == "move_next");
            Assert.Equal("node-demodisc-rendering-software-path-tracer-next", nextInteraction.GetProperty("id").GetString());
            Assert.False(nextInteraction.GetProperty("isDefault").GetBoolean());
            JsonElement activateInteraction = Assert.Single(interactions, interaction => interaction.GetProperty("kind").GetString() == "activate");
            Assert.Equal("node-demodisc-rendering-software-path-tracer-activate-surface-demodisc-showcase-scene", activateInteraction.GetProperty("id").GetString());
            Assert.True(activateInteraction.GetProperty("isDefault").GetBoolean());
            Assert.Equal("surface-demodisc-showcase-scene", activateInteraction.GetProperty("targetSurfaceId").GetString());

            JsonElement softwarePathTracerSelectedState = Assert.Single(softwarePathTracerNode.GetProperty("states").EnumerateArray(), state => state.GetProperty("name").GetString() == "selected");
            JsonElement softwarePathTracerHighlight = Assert.Single(softwarePathTracerSelectedState.GetProperty("recognition").GetProperty("clues").EnumerateArray(), clue => clue.GetProperty("type").GetString() == "highlighted_text");
            JsonElement softwarePathTracerHighlightParams = softwarePathTracerHighlight.GetProperty("params");
            JsonElement softwarePathTracerCandidates = softwarePathTracerHighlightParams.GetProperty("candidates");
            Assert.Equal(new[] { "Software Path Tracer" }, softwarePathTracerCandidates.EnumerateArray().Select(candidate => candidate.GetString()).ToArray());

            JsonElement pbrSelectedState = Assert.Single(pbrShadowTheaterNode.GetProperty("states").EnumerateArray(), state => state.GetProperty("name").GetString() == "selected");
            JsonElement pbrHighlight = Assert.Single(pbrSelectedState.GetProperty("recognition").GetProperty("clues").EnumerateArray(), clue => clue.GetProperty("type").GetString() == "highlighted_text");
            JsonElement pbrHighlightParams = pbrHighlight.GetProperty("params");
            foreach (string propertyName in new[] { "highlightColorHex", "hueTolerance", "minSaturation", "includeArrowRegion", "requireArrowRegion", "arrowOffsetX", "arrowWidth" }) {
                Assert.Equal(pbrHighlightParams.GetProperty(propertyName).GetRawText(), softwarePathTracerHighlightParams.GetProperty(propertyName).GetRawText());
            }

            Assert.Equal(11, backNode.GetProperty("order").GetInt32());
            Assert.Equal(backNode.GetProperty("id").GetString(), renderingNodes[^1].GetProperty("id").GetString());

            JsonElement showcaseScene = FindSurface(document, "surface-demodisc-showcase-scene");
            JsonElement showcaseRecognition = FindClue(showcaseScene, "demodisc-showcase-catalog-label");
            JsonElement showcaseTexts = showcaseRecognition.GetProperty("params").GetProperty("texts");
            Assert.Equal(1, showcaseTexts.EnumerateArray().Count(text => text.GetString() == "Software Path Tracer"));
        }

        static JsonElement FindSurface(JsonDocument document, string surfaceId) {
            return Assert.Single(document.RootElement.GetProperty("surfaces").EnumerateArray(), surface => surface.GetProperty("id").GetString() == surfaceId);
        }

        static JsonElement FindClue(JsonElement surface, string clueId) {
            return Assert.Single(surface.GetProperty("recognition").GetProperty("clues").EnumerateArray(), clue => clue.GetProperty("id").GetString() == clueId);
        }

        static string FindRepositoryRoot([CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (directory != null) {
                if (File.Exists(Path.Combine(directory.FullName, "helenui", "demodisc.json"))) {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate the Demo Disc repository root.");
        }
    }
}
