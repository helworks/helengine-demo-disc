using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Verifies the generated scene-memory probe asset does not carry the editor-only placeholder component into packaged scene data.
    /// </summary>
    public sealed class SceneMemoryProbeSceneGenerationTests {
        static readonly string ProjectRootPath = ResolveProjectRoot();
        const string SceneRelativePath = @"assets\scenes\rendering\scene_memory_probe.helen";
        const string PlaceholderComponentTypeId = "city.rendering.tools.SceneMemoryProbeComponent, rendering.tools";

        [Fact]
        public void Fresh_scene_memory_probe_factory_output_contains_no_editor_only_placeholder_component() {
            string projectRootPath = Path.Combine(Path.GetTempPath(), "city-scene-memory-probe-tests", Guid.NewGuid().ToString("N"));
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            city.rendering.tools.SceneMemoryProbeSceneFactory factory = new city.rendering.tools.SceneMemoryProbeSceneFactory(authoringSession);

            city.rendering.tools.GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition();

            Assert.DoesNotContain(
                FlattenComponents(definition.RootEntities),
                component => component is city.rendering.tools.SceneMemoryProbeComponent);
            Assert.Equal(city.rendering.tools.SceneMemoryProbeSceneFactory.SceneId, definition.SceneId);
            Assert.True(definition.SceneSettings.DontUnload);
            Assert.Single(definition.RootEntities);
        }

        [Fact]
        public void Checked_in_scene_memory_probe_contains_no_editor_only_placeholder_component() {
            SceneAsset scene = LoadSceneAsset();

            Assert.DoesNotContain(
                FlattenComponents(scene.RootEntities),
                component => string.Equals(component.ComponentTypeId, PlaceholderComponentTypeId, StringComparison.Ordinal));
        }

        [Fact]
        public void Generated_scene_memory_probe_preserves_scene_generation_metadata() {
            SceneAsset scene = LoadSceneAsset();

            Assert.Equal("scenes/rendering/scene_memory_probe.helen", scene.Id);
            Assert.True(scene.SceneSettings.DontUnload);
            Assert.Contains(scene.RootEntities, entity => string.Equals(entity.Name, "SceneMemoryProbeRoot", StringComparison.Ordinal));
        }

        static SceneAsset LoadSceneAsset() {
            string fullPath = Path.Combine(ProjectRootPath, SceneRelativePath);
            Assert.True(File.Exists(fullPath), $"Expected generated scene asset '{fullPath}' to exist.");
            using FileStream stream = File.OpenRead(fullPath);
            return Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
        }

        static IReadOnlyList<SceneComponentAssetRecord> FlattenComponents(SceneEntityAsset[] rootEntities) {
            List<SceneComponentAssetRecord> components = new List<SceneComponentAssetRecord>();
            AppendComponents(rootEntities, components);
            return components;
        }

        static IReadOnlyList<Component> FlattenComponents(Entity[] rootEntities) {
            List<Component> components = new List<Component>();
            AppendComponents(rootEntities, components);
            return components;
        }

        static void AppendComponents(Entity[] entities, List<Component> components) {
            if (entities == null) {
                return;
            }

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                Entity entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }

                if (entity.Components != null) {
                    components.AddRange(entity.Components);
                }

                AppendComponents(entity.Children?.ToArray(), components);
            }
        }

        static void AppendComponents(SceneEntityAsset[] entities, List<SceneComponentAssetRecord> components) {
            if (entities == null) {
                return;
            }

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                SceneEntityAsset entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }

                SceneComponentAssetRecord[] entityComponents = entity.Components ?? Array.Empty<SceneComponentAssetRecord>();
                for (int componentIndex = 0; componentIndex < entityComponents.Length; componentIndex++) {
                    SceneComponentAssetRecord component = entityComponents[componentIndex];
                    if (component != null) {
                        components.Add(component);
                    }
                }

                AppendComponents(entity.Children, components);
            }
        }

        static string ResolveProjectRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo currentDirectory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (currentDirectory != null) {
                string assetsPath = Path.Combine(currentDirectory.FullName, "assets");
                string projectFilePath = Path.Combine(currentDirectory.FullName, "project.heproj");
                if (Directory.Exists(assetsPath) && File.Exists(projectFilePath)) {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }

            throw new InvalidOperationException("Unable to locate the demo-disc checkout root from the test source path.");
        }
    }
}
