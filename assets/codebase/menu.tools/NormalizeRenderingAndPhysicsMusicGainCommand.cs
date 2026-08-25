using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Normalizes serialized rendering and physics showcase scene-music gains back to the shared authored value.
    /// </summary>
    public sealed class NormalizeRenderingAndPhysicsMusicGainCommand : IEditorCommand {
        static readonly string[] SceneRelativePaths = {
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
            @"assets\scenes\rendering\textured_cube_grid.helen",
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

        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.normalize-rendering-and-physics-music-gain";

        /// <summary>
        /// Gets the human-readable editor command label.
        /// </summary>
        public string DisplayName => "Normalize Rendering And Physics Music Gain";

        /// <summary>
        /// Rewrites the serialized scene-music audio source gain for the shared rendering and physics showcase scenes.
        /// </summary>
        /// <param name="context">Editor-safe command context supplied by the editor host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            string projectRootPath = Path.GetFullPath(context.ProjectRootPath);
            for (int index = 0; index < SceneRelativePaths.Length; index++) {
                NormalizeScene(projectRootPath, SceneRelativePaths[index]);
            }
        }

        static void NormalizeScene(string projectRootPath, string sceneRelativePath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(sceneRelativePath)) {
                throw new ArgumentException("Scene relative path must be provided.", nameof(sceneRelativePath));
            }

            string fullScenePath = Path.Combine(projectRootPath, sceneRelativePath);
            if (!File.Exists(fullScenePath)) {
                return;
            }

            SceneAsset sceneAsset;
            using (FileStream stream = File.OpenRead(fullScenePath)) {
                sceneAsset = (SceneAsset)global::helengine.editor.AssetSerializer.Deserialize(stream);
            }

            if (!NormalizeEntities(sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>())) {
                return;
            }

            string relativePath = Path.GetRelativePath(Path.Combine(projectRootPath, "assets"), fullScenePath).Replace('\\', '/');
            new global::helengine.editor.GeneratedAssetWriteService().WriteAsset(projectRootPath, relativePath, sceneAsset);
        }

        static bool NormalizeEntities(SceneEntityAsset[] entities) {
            bool modified = false;
            if (entities == null) {
                return false;
            }

            for (int index = 0; index < entities.Length; index++) {
                SceneEntityAsset entity = entities[index];
                if (entity == null) {
                    continue;
                }

                if (string.Equals(entity.Name, "SceneMusic", StringComparison.Ordinal)) {
                    modified |= NormalizeSceneMusicEntity(entity);
                }

                modified |= NormalizeEntities(entity.Children);
            }

            return modified;
        }

        static bool NormalizeSceneMusicEntity(SceneEntityAsset entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            bool modified = false;
            SceneComponentAssetRecord[] components = entity.Components ?? Array.Empty<SceneComponentAssetRecord>();
            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());
            string audioSourceComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(AudioSourceComponent));
            for (int index = 0; index < components.Length; index++) {
                SceneComponentAssetRecord record = components[index];
                if (record == null || !string.Equals(record.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal)) {
                    continue;
                }

                EntitySaveComponent saveComponent = new EntitySaveComponent();
                AudioSourceComponent audioSource = (AudioSourceComponent)descriptor.DeserializeComponent(record, saveComponent, null);
                if (!string.Equals(audioSource.BusId, "music", StringComparison.Ordinal)
                    || Math.Abs(audioSource.Gain - city.scene.tools.GeneratedSceneMusicAuthoringService.RenderingAndPhysicsMusicGain) < 0.001f) {
                    continue;
                }

                audioSource.Gain = city.scene.tools.GeneratedSceneMusicAuthoringService.RenderingAndPhysicsMusicGain;
                SceneComponentAssetRecord updatedRecord = descriptor.SerializeComponent(
                    audioSource,
                    record.ComponentIndex,
                    saveComponent.GetOrCreateComponentState(audioSource));
                updatedRecord.ComponentKey = record.ComponentKey;
                components[index] = updatedRecord;
                modified = true;
            }

            entity.Components = components;
            return modified;
        }
    }
}
