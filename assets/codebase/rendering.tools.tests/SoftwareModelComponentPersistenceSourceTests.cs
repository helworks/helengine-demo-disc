using helengine;
using helengine.editor;

namespace city.rendering.tools.tests {
    /// <summary>
    /// Verifies the software model component survives automatic authored-component persistence without runtime model resolution.
    /// </summary>
    public sealed class SoftwareModelComponentPersistenceSourceTests {
        /// <summary>
        /// Ensures automatic reflected persistence round-trips one authored model reference and two software material values.
        /// </summary>
        [Fact]
        public void Automatic_persistence_round_trips_model_reference_and_material_values_without_runtime_resolver() {
            SceneAssetReference modelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemReference(
                "1234567890abcdef1234567890abcdef",
                "models/software-path-tracer-probe.hmodel",
                "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef");
            city.rendering.SoftwareModelComponent component = new city.rendering.SoftwareModelComponent {
                ModelReference = modelReference,
                Materials = new[] {
                    new city.rendering.SoftwareMaterial {
                        DiffuseColor = new float3(0.1f, 0.2f, 0.3f),
                        EmissionColor = new float3(0.4f, 0.5f, 0.6f),
                        EmissionStrength = 1.25f
                    },
                    new city.rendering.SoftwareMaterial {
                        DiffuseColor = new float3(0.7f, 0.8f, 0.9f),
                        EmissionColor = new float3(0.9f, 0.8f, 0.7f),
                        EmissionStrength = 4.5f
                    }
                }
            };
            AutomaticScriptComponentPersistenceDescriptor descriptor = new AutomaticScriptComponentPersistenceDescriptor(
                new ScriptComponentReflectionSchemaBuilder());

            SceneComponentAssetRecord record = descriptor.SerializeComponent(
                component,
                componentIndex: 0,
                saveState: new EntityComponentSaveState());
            city.rendering.SoftwareModelComponent restored = Assert.IsType<city.rendering.SoftwareModelComponent>(
                descriptor.DeserializeComponent(record, saveComponent: null, referenceResolver: null));

            Assert.NotNull(restored.ModelReference);
            Assert.Equal(modelReference.SourceKind, restored.ModelReference.SourceKind);
            Assert.Equal(modelReference.ProviderId, restored.ModelReference.ProviderId);
            Assert.Equal(modelReference.AssetId, restored.ModelReference.AssetId);
            Assert.Equal(modelReference.RelativePath, restored.ModelReference.RelativePath);
            Assert.Equal(modelReference.ContentHash, restored.ModelReference.ContentHash);

            Assert.Equal(2, restored.Materials.Length);
            Assert.Equal(0.1f, restored.Materials[0].DiffuseColor.X);
            Assert.Equal(0.2f, restored.Materials[0].DiffuseColor.Y);
            Assert.Equal(0.3f, restored.Materials[0].DiffuseColor.Z);
            Assert.Equal(0.4f, restored.Materials[0].EmissionColor.X);
            Assert.Equal(0.5f, restored.Materials[0].EmissionColor.Y);
            Assert.Equal(0.6f, restored.Materials[0].EmissionColor.Z);
            Assert.Equal(1.25f, restored.Materials[0].EmissionStrength);
            Assert.Equal(0.7f, restored.Materials[1].DiffuseColor.X);
            Assert.Equal(0.8f, restored.Materials[1].DiffuseColor.Y);
            Assert.Equal(0.9f, restored.Materials[1].DiffuseColor.Z);
            Assert.Equal(0.9f, restored.Materials[1].EmissionColor.X);
            Assert.Equal(0.8f, restored.Materials[1].EmissionColor.Y);
            Assert.Equal(0.7f, restored.Materials[1].EmissionColor.Z);
            Assert.Equal(4.5f, restored.Materials[1].EmissionStrength);
        }

        /// <summary>
        /// Ensures the reflected persistence schema exposes no runtime model or resolver-backed member path.
        /// </summary>
        [Fact]
        public void Automatic_persistence_schema_has_no_runtime_model_member_path() {
            ScriptComponentReflectionSchema schema = new ScriptComponentReflectionSchemaBuilder().Build(
                typeof(city.rendering.SoftwareModelComponent));

            Assert.Contains(schema.Members, member => member.Name == nameof(city.rendering.SoftwareModelComponent.ModelReference));
            Assert.Contains(schema.Members, member => member.Name == nameof(city.rendering.SoftwareModelComponent.Materials));
            Assert.DoesNotContain(schema.Members, member => member.Name == "Model");
            Assert.DoesNotContain(schema.Members, member => member.Name == "RuntimeModel");
            Assert.DoesNotContain(schema.Members, member => member.ValueType == typeof(RuntimeModel));
            Assert.DoesNotContain(
                schema.Members,
                member => member.ValueType.FullName != null && member.ValueType.FullName.Contains("RuntimeModel", StringComparison.Ordinal));
        }
    }
}
