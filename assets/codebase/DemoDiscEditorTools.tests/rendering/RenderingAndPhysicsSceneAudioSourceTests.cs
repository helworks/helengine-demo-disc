using helengine;
using helengine.editor;
using DemoDisc.EditorTools;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the generated rendering and physics showcase scenes remain silent until music is intentionally reintroduced.
    /// </summary>
    public sealed class RenderingAndPhysicsSceneAudioSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";
        static readonly string[] RenderingSceneRelativePaths = {
            @"assets\scenes\rendering\axis_test.helen",
            @"assets\scenes\rendering\axis_test2.helen",
            @"assets\scenes\rendering\colored_cube_grid.helen",
            @"assets\scenes\rendering\cube_test.helen",
            @"assets\scenes\rendering\directional_shadow_plaza.helen",
            @"assets\scenes\rendering\test_scene_matrix_render.helen",
            @"assets\scenes\rendering\textured_cube_grid.helen"
        };

        static readonly string[] PhysicsSceneRelativePaths = {
            @"assets\scenes\physics\test_scene_character_moving_platform.helen",
            @"assets\scenes\physics\test_scene_character_slope.helen",
            @"assets\scenes\physics\test_scene_character_steps.helen",
            @"assets\scenes\physics\test_scene_dynamic_mixed_stack.helen",
            @"assets\scenes\physics\test_scene_dynamic_sphere_stack.helen",
            @"assets\scenes\physics\test_scene_dynamic_stack_boxes.helen",
            @"assets\scenes\physics\test_scene_kinematic_push.helen",
            @"assets\scenes\physics\test_scene_mesh_ground_stability.helen",
            @"assets\scenes\physics\test_scene_render_only_slope.helen",
            @"assets\scenes\physics\test_scene_single_falling_cube.helen",
            @"assets\scenes\physics\test_scene_strict_rotated_box_compare.helen",
            @"assets\scenes\physics\test_scene_trigger_volume.helen"
        };

        [Fact]
        public void Generated_rendering_and_physics_scenes_are_silent() {
            string audioSourceComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(AudioSourceComponent));

            AssertAllScenesAreSilent(RenderingSceneRelativePaths, audioSourceComponentTypeId);
            AssertAllScenesAreSilent(PhysicsSceneRelativePaths, audioSourceComponentTypeId);
        }

        static void AssertAllScenesAreSilent(IEnumerable<string> relativePaths, string audioSourceComponentTypeId) {
            foreach (string relativePath in relativePaths) {
                SceneAsset scene = LoadSceneAsset(relativePath);

                Assert.DoesNotContain(scene.AssetReferences, reference => reference != null && reference.RelativePath.Contains("audio/", StringComparison.Ordinal));
                Assert.DoesNotContain(FlattenComponents(scene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
            }
        }

        static SceneAsset LoadSceneAsset(string relativePath) {
            string fullPath = Path.Combine(ProjectRootPath, relativePath);
            using FileStream stream = File.OpenRead(fullPath);
            return Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
        }

        static IReadOnlyList<SceneComponentAssetRecord> FlattenComponents(SceneEntityAsset[] rootEntities) {
            List<SceneComponentAssetRecord> components = new List<SceneComponentAssetRecord>();
            AppendComponents(rootEntities, components);
            return components;
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
    }
}
