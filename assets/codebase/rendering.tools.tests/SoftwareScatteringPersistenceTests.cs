using city.rendering;
using System.Runtime.CompilerServices;

namespace city.tests {
    /// <summary>Verifies scattering additions preserve existing nested material bytes and persist through editor authoring.</summary>
    public sealed class SoftwareScatteringPersistenceTests {
        /// <summary>The nested material format has no field framing, so its original member set must stay fixed.</summary>
        [Fact]
        public void Existing_nested_material_contract_is_unchanged() {
            Assert.Equal(new[] { "DiffuseColor", "EmissionColor", "EmissionStrength" }, typeof(SoftwareMaterial).GetProperties().Select(property => property.Name).OrderBy(name => name));
            Assert.NotNull(typeof(SoftwareModelComponent).GetProperty("Scattering").GetCustomAttributes(typeof(ScenePersistenceAppendAttribute), false).SingleOrDefault());
        }

        /// <summary>New scattering settings survive the real component descriptor without changing diffuse/emission data.</summary>
        [Fact]
        public void Scattering_settings_round_trip_through_authoring() {
            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());
            SoftwareModelComponent component = new SoftwareModelComponent {
                Materials = new[] { new SoftwareMaterial { DiffuseColor = new float3(0.1f, 0.2f, 0.3f) }, new SoftwareMaterial() },
                Scattering = new[] {
                    new SoftwareScatteringMaterial { Kind = SoftwareMaterialKind.Mirror, ReflectionColor = new float3(0.2f, 0.5f, 1f) },
                    new SoftwareScatteringMaterial { Kind = SoftwareMaterialKind.Glass, IndexOfRefraction = 1.33f }
                }
            };
            SceneComponentAssetRecord record = descriptor.SerializeComponent(component, 0, new EntityComponentSaveState());
            SoftwareModelComponent restored = Assert.IsType<SoftwareModelComponent>(descriptor.DeserializeComponent(record, null, null));
            Assert.Equal(new float3(0.1f, 0.2f, 0.3f), restored.Materials[0].DiffuseColor);
            Assert.Equal(SoftwareMaterialKind.Mirror, restored.Scattering[0].Kind);
            Assert.Equal(new float3(0.2f, 0.5f, 1f), restored.Scattering[0].ReflectionColor);
            Assert.Equal(SoftwareMaterialKind.Glass, restored.Scattering[1].Kind);
            Assert.Equal(1.33f, restored.Scattering[1].IndexOfRefraction);
        }

        /// <summary>Loads the existing Cornell Box payloads directly, with no asset rewrite or migration.</summary>
        [Fact]
        public void Existing_cornell_material_payloads_load_as_diffuse() {
            using FileStream stream = File.OpenRead(Path.Combine(FindProjectRoot(), "assets", "scenes", "rendering", "software_path_tracer.helen"));
            SceneAsset scene = Assert.IsType<SceneAsset>(helengine.editor.AssetSerializer.Deserialize(stream));
            Stack<SceneEntityAsset> pending = new Stack<SceneEntityAsset>(scene.RootEntities);
            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());
            ComponentPlatformOverridePayloadService overrides = new ComponentPlatformOverridePayloadService();
            int models = 0;
            while (pending.Count > 0) {
                SceneEntityAsset entity = pending.Pop();
                foreach (SceneEntityAsset child in entity.Children ?? Array.Empty<SceneEntityAsset>()) pending.Push(child);
                foreach (SceneComponentAssetRecord record in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                    if (!record.ComponentTypeId.StartsWith("city.rendering.SoftwareModelComponent,", StringComparison.Ordinal)) continue;
                    SoftwareModelComponent model = Assert.IsType<SoftwareModelComponent>(descriptor.DeserializeComponent(overrides.UnwrapBaseRecord(record), null, null));
                    Assert.Single(model.Materials);
                    Assert.Empty(model.Scattering);
                    models++;
                }
            }
            Assert.Equal(8, models);
        }

        /// <summary>Saved showcases retain canonical model identities and all twelve gallery scattering overrides.</summary>
        [Theory]
        [InlineData("ray_tracing_teapot", 2)]
        [InlineData("ray_tracing_spheres", 12)]
        [InlineData("ray_tracing_soft_shadows", 4)]
        public void Saved_showcases_have_identity_backed_models(string name, int expectedModels) {
            using FileStream stream = File.OpenRead(Path.Combine(FindProjectRoot(), "assets", "scenes", "rendering", name + ".helen"));
            SceneAsset scene = Assert.IsType<SceneAsset>(helengine.editor.AssetSerializer.Deserialize(stream));
            Stack<SceneEntityAsset> pending = new Stack<SceneEntityAsset>(scene.RootEntities);
            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());
            ComponentPlatformOverridePayloadService overrides = new ComponentPlatformOverridePayloadService();
            int models = 0;
            while (pending.Count > 0) {
                SceneEntityAsset entity = pending.Pop();
                foreach (SceneEntityAsset child in entity.Children ?? Array.Empty<SceneEntityAsset>()) pending.Push(child);
                foreach (SceneComponentAssetRecord record in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                    if (!record.ComponentTypeId.StartsWith("city.rendering.SoftwareModelComponent,", StringComparison.Ordinal)) continue;
                    SoftwareModelComponent model = Assert.IsType<SoftwareModelComponent>(descriptor.DeserializeComponent(overrides.UnwrapBaseRecord(record), null, null));
                    if (model.ModelReference.SourceKind != SceneAssetReferenceSourceKind.FileSystem) continue;
                    Assert.Equal(32, model.ModelReference.AssetId.Length);
                    Assert.StartsWith("sha256:", model.ModelReference.ContentHash);
                    if (name == "ray_tracing_spheres") Assert.Single(model.Scattering);
                    models++;
                }
            }
            Assert.Equal(expectedModels, models);
        }

        /// <summary>All four saved ray tracing scenes expose the shared main-menu return action.</summary>
        [Theory]
        [InlineData("software_path_tracer")]
        [InlineData("ray_tracing_teapot")]
        [InlineData("ray_tracing_spheres")]
        [InlineData("ray_tracing_soft_shadows")]
        public void Saved_ray_tracing_scenes_enable_main_menu_return(string name) {
            using FileStream stream = File.OpenRead(Path.Combine(FindProjectRoot(), "assets", "scenes", "rendering", name + ".helen"));
            SceneAsset scene = Assert.IsType<SceneAsset>(helengine.editor.AssetSerializer.Deserialize(stream));
            Stack<SceneEntityAsset> pending = new Stack<SceneEntityAsset>(scene.RootEntities);
            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());
            ComponentPlatformOverridePayloadService overrides = new ComponentPlatformOverridePayloadService();
            int actions = 0;
            while (pending.Count > 0) {
                SceneEntityAsset entity = pending.Pop();
                foreach (SceneEntityAsset child in entity.Children ?? Array.Empty<SceneEntityAsset>()) pending.Push(child);
                foreach (SceneComponentAssetRecord record in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                    if (!record.ComponentTypeId.StartsWith("city.menu.DemoDiscReturnToMenuComponent,", StringComparison.Ordinal)) continue;
                    city.menu.DemoDiscReturnToMenuComponent action = Assert.IsType<city.menu.DemoDiscReturnToMenuComponent>(descriptor.DeserializeComponent(overrides.UnwrapBaseRecord(record), null, null));
                    Assert.True(action.AllowKeyboardReturn);
                    Assert.True(action.AllowGamepadReturn);
                    Assert.True(action.AllowPointerReturn);
                    actions++;
                }
            }
            Assert.Equal(1, actions);
        }
        /// <summary>Finds the authored project instead of relying on the generated test output path.</summary>
        static string FindProjectRoot([CallerFilePath] string sourcePath = "") {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(sourcePath));
            while (directory != null) {
                if (File.Exists(Path.Combine(directory.FullName, "project.heproj"))) return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Demo Disc project not found.");
        }
    }
}
