using helengine;
using helengine.editor;

namespace city.tests {
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
            @"assets\scenes\rendering\ground_cube_probe.helen",
            @"assets\scenes\rendering\scaled_cube.helen",
            @"assets\scenes\rendering\scene_memory_probe.helen",
            @"assets\scenes\rendering\spotlight_street_slice.helen",
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

        [Fact]
        public void Rendering_and_physics_generators_do_not_author_shared_music() {
            string renderingSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneGenerator.cs");
            string physicsSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneFactory.cs");
            string physicsNintendoDsSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsNintendoDsSceneGenerator.cs");

            Assert.DoesNotContain("CreateRenderingAndPhysicsMusicEntity", renderingSource, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateRenderingAndPhysicsMusicEntity", physicsSource, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateRenderingAndPhysicsMusicEntity", physicsNintendoDsSource, StringComparison.Ordinal);
            Assert.Contains("LoadSceneAssetWithoutSharedMusic", physicsNintendoDsSource, StringComparison.Ordinal);
            Assert.Contains("StripSharedSceneMusic", physicsNintendoDsSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the handheld physics generator skips the matrix scene because the rendering pipeline authors it, including its handheld augmentation.
        /// </summary>
        [Fact]
        public void Nintendo_handheld_generator_skips_the_rendering_pipeline_matrix_scene() {
            string physicsNintendoDsSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsNintendoDsSceneGenerator.cs");

            Assert.Contains("string.Equals(sceneEntry.SceneId, \"test_scene_matrix_render\", StringComparison.Ordinal)", physicsNintendoDsSource, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateFreshPhysicsSceneAssetWithoutSharedMusic", physicsNintendoDsSource, StringComparison.Ordinal);
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
