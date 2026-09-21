using System.Reflection;
using helengine;

namespace DemoDisc.Raytracing.tests {
    /// <summary>
    /// Verifies the authored-data contract of the software model component.
    /// </summary>
    public sealed class SoftwareModelComponentTests {
        /// <summary>
        /// Ensures the component exposes the exact requested public type name and direct component inheritance.
        /// </summary>
        [Fact]
        public void Software_model_component_uses_the_exact_name_and_component_base_type() {
            Type componentType = typeof(DemoDisc.Raytracing.SoftwareModelComponent);

            Assert.Equal("SoftwareModelComponent", componentType.Name);
            Assert.Equal(typeof(Component), componentType.BaseType);
        }

        /// <summary>
        /// Ensures the component exposes only the authored model reference and software material array with exact types and defaults.
        /// </summary>
        [Fact]
        public void Software_model_component_exposes_authored_reference_and_material_defaults() {
            Type componentType = typeof(DemoDisc.Raytracing.SoftwareModelComponent);
            PropertyInfo modelReferenceProperty = componentType.GetProperty(nameof(DemoDisc.Raytracing.SoftwareModelComponent.ModelReference));
            PropertyInfo materialsProperty = componentType.GetProperty(nameof(DemoDisc.Raytracing.SoftwareModelComponent.Materials));
            DemoDisc.Raytracing.SoftwareModelComponent component = new DemoDisc.Raytracing.SoftwareModelComponent();

            Assert.NotNull(modelReferenceProperty);
            Assert.Equal(typeof(SceneAssetReference), modelReferenceProperty.PropertyType);
            Assert.Null(component.ModelReference);

            Assert.NotNull(materialsProperty);
            Assert.Equal(typeof(DemoDisc.Raytracing.SoftwareMaterial[]), materialsProperty.PropertyType);
            Assert.NotNull(component.Materials);
            Assert.Empty(component.Materials);
        }

        /// <summary>
        /// Ensures a software material starts with white diffuse color, no emission color, and zero emission strength.
        /// </summary>
        [Fact]
        public void Software_material_uses_neutral_default_values() {
            DemoDisc.Raytracing.SoftwareMaterial material = new DemoDisc.Raytracing.SoftwareMaterial();

            Assert.Equal(1f, material.DiffuseColor.X);
            Assert.Equal(1f, material.DiffuseColor.Y);
            Assert.Equal(1f, material.DiffuseColor.Z);
            Assert.Equal(0f, material.EmissionColor.X);
            Assert.Equal(0f, material.EmissionColor.Y);
            Assert.Equal(0f, material.EmissionColor.Z);
            Assert.Equal(0f, material.EmissionStrength);
        }

        /// <summary>
        /// Ensures the model reference property carries the CPU-readable model marker required by packaging.
        /// </summary>
        [Fact]
        public void Software_model_component_marks_model_reference_as_cpu_readable() {
            PropertyInfo modelReferenceProperty = typeof(DemoDisc.Raytracing.SoftwareModelComponent)
                .GetProperty(nameof(DemoDisc.Raytracing.SoftwareModelComponent.ModelReference));

            Assert.NotNull(modelReferenceProperty);
            Assert.NotNull(modelReferenceProperty.GetCustomAttribute<CpuReadableModelReferenceAttribute>());
        }

        /// <summary>
        /// Ensures diffuse and emissive material scalars round-trip through ordinary object state without hidden runtime conversion.
        /// </summary>
        [Fact]
        public void Software_material_scalars_round_trip_through_object_state() {
            DemoDisc.Raytracing.SoftwareMaterial material = new DemoDisc.Raytracing.SoftwareMaterial {
                DiffuseColor = new float3(0.2f, 0.4f, 0.6f),
                EmissionColor = new float3(0.7f, 0.5f, 0.3f),
                EmissionStrength = 3.25f
            };
            DemoDisc.Raytracing.SoftwareModelComponent component = new DemoDisc.Raytracing.SoftwareModelComponent {
                Materials = new[] { material }
            };

            DemoDisc.Raytracing.SoftwareMaterial restoredMaterial = component.Materials[0];

            Assert.Equal(0.2f, restoredMaterial.DiffuseColor.X);
            Assert.Equal(0.4f, restoredMaterial.DiffuseColor.Y);
            Assert.Equal(0.6f, restoredMaterial.DiffuseColor.Z);
            Assert.Equal(0.7f, restoredMaterial.EmissionColor.X);
            Assert.Equal(0.5f, restoredMaterial.EmissionColor.Y);
            Assert.Equal(0.3f, restoredMaterial.EmissionColor.Z);
            Assert.Equal(3.25f, restoredMaterial.EmissionStrength);
        }

        /// <summary>
        /// Ensures the software component does not expose legacy runtime model or GPU mesh surfaces.
        /// </summary>
        [Fact]
        public void Software_model_component_has_no_runtime_model_or_mesh_component_surface() {
            Type componentType = typeof(DemoDisc.Raytracing.SoftwareModelComponent);

            Assert.Null(componentType.GetProperty("Model", BindingFlags.Instance | BindingFlags.Public));
            Assert.Null(componentType.GetProperty("RuntimeModel", BindingFlags.Instance | BindingFlags.Public));
            Assert.False(typeof(MeshComponent).IsAssignableFrom(componentType));
            Assert.DoesNotContain(
                componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public),
                property => property.PropertyType == typeof(RuntimeModel));
        }
    }
}
