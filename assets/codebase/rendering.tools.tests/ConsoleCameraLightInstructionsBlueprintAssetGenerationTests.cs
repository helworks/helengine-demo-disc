using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Verifies the committed console camera/light Blueprint has the requested serialized shape.
    /// </summary>
    public sealed class ConsoleCameraLightInstructionsBlueprintAssetGenerationTests {
        const string BlueprintPath = @"C:\dev\helprojs\demodisc\assets\blueprints\ui\ConsoleCameraLightInstructions.hblueprint";

        [Fact]
        public void Committed_console_blueprint_contains_only_the_camera_light_panel() {
            Assert.True(File.Exists(BlueprintPath), $"Expected generated Blueprint at '{BlueprintPath}'.");

            using FileStream stream = File.OpenRead(BlueprintPath);
            BlueprintAsset blueprint = Assert.IsType<BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));

            Assert.Equal("blueprints/ui/ConsoleCameraLightInstructions.hblueprint", blueprint.Id);
            Assert.NotNull(blueprint.RootEntity);
            Assert.NotEmpty(blueprint.AssetReferences);

            SceneEntityAsset root = blueprint.RootEntity;
            Assert.Contains(root.Children, child => child != null && child.Name == "ConsoleCameraLightInstructionsPanel");
            Assert.Contains(EnumerateEntities(root), entity => entity.Name == "CameraIconPrimary");
            Assert.Contains(EnumerateEntities(root), entity => entity.Name == "CameraText");
            Assert.Contains(EnumerateEntities(root), entity => entity.Name == "LightIcon");
            Assert.Contains(EnumerateEntities(root), entity => entity.Name == "LightIconText");
            Assert.DoesNotContain(EnumerateEntities(root), entity => entity.Name.Contains("Secondary", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(EnumerateEntities(root), entity => entity.Name.Contains("Fps", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(EnumerateEntities(root), entity => entity.Name.Contains("Back", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(EnumerateEntities(root), entity => entity.Name.Contains("Swatch", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(EnumerateComponentTypes(root), componentType => componentType.Contains("BlueprintInstanceComponent", StringComparison.Ordinal));
            Assert.DoesNotContain(EnumerateComponentTypes(root), componentType => componentType.Contains("DemoDiscLightToggleComponent", StringComparison.Ordinal));

            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/ps2/dpad.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/ps2/r1.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/gamecube/dpad.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/gamecube/r.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/wii/dpad.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/wii/b.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/switch/dpad.png");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "images/instructions/controls/generated/switch/r.png");
        }

        [Theory]
        [InlineData(@"C:\dev\helprojs\demodisc\assets\scenes\rendering\cube_test.helen")]
        [InlineData(@"C:\dev\helprojs\demodisc\assets\scenes\physics\test_scene_dynamic_stack_boxes.helen")]
        public void Generated_rendering_and_physics_scenes_attach_one_console_blueprint_root(string scenePath) {
            Assert.True(File.Exists(scenePath), $"Expected generated scene at '{scenePath}'.");

            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] blueprintRoots = scene.RootEntities
                .Where(entity => entity != null && entity.Name == "ConsoleCameraLightInstructions")
                .ToArray();
            Assert.True(
                blueprintRoots.Length == 1,
                $"Expected one console Blueprint root, found {blueprintRoots.Length}: {string.Join(", ", scene.RootEntities.Select(entity => entity?.Name ?? "<null>"))}");
            SceneEntityAsset blueprintRoot = blueprintRoots[0];

            Assert.Contains(
                blueprintRoot.Components,
                component => component.ComponentTypeId.Contains("BlueprintInstanceComponent", StringComparison.Ordinal));
            Assert.Contains(
                blueprintRoot.PlatformExistenceOverrides,
                overrideAsset => overrideAsset.PlatformId == "windows" && !overrideAsset.Exists);
            Assert.Contains(
                blueprintRoot.PlatformExistenceOverrides,
                overrideAsset => overrideAsset.PlatformId == "psp" && !overrideAsset.Exists);
            Assert.Contains(
                blueprintRoot.PlatformExistenceOverrides,
                overrideAsset => overrideAsset.PlatformId == "psvita" && !overrideAsset.Exists);
            Assert.Contains(
                blueprintRoot.PlatformExistenceOverrides,
                overrideAsset => overrideAsset.PlatformId == "ds" && !overrideAsset.Exists);
            Assert.Contains(
                blueprintRoot.PlatformExistenceOverrides,
                overrideAsset => overrideAsset.PlatformId == "3ds" && !overrideAsset.Exists);

            SceneEntityAsset legacyOverlayRoot = Assert.Single(
                scene.RootEntities,
                entity => entity != null && entity.Name == "DemoSceneInstructionViewport");
            Assert.Contains(
                legacyOverlayRoot.PlatformExistenceOverrides,
                overrideAsset => overrideAsset.PlatformId == "ps2" && !overrideAsset.Exists);
        }

        static IEnumerable<SceneEntityAsset> EnumerateEntities(SceneEntityAsset root) {
            if (root == null) {
                yield break;
            }

            yield return root;
            foreach (SceneEntityAsset child in root.Children ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneEntityAsset descendant in EnumerateEntities(child)) {
                    yield return descendant;
                }
            }
        }

        static IEnumerable<string> EnumerateComponentTypes(SceneEntityAsset root) {
            foreach (SceneEntityAsset entity in EnumerateEntities(root)) {
                foreach (SceneComponentAssetRecord component in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                    yield return component.ComponentTypeId ?? string.Empty;
                }
            }
        }
    }
}
