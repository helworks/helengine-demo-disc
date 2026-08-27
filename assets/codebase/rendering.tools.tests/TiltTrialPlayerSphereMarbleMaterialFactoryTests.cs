using System.Reflection;

namespace city.tests {
    /// <summary>
    /// Verifies platform-specific material settings for the Tilt Trial player sphere.
    /// </summary>
    public sealed class TiltTrialPlayerSphereMarbleMaterialFactoryTests {
        /// <summary>
        /// Ensures Nintendo DS uses the lit solid-color path so the sphere consumes no texture slot.
        /// </summary>
        [Fact]
        public void Nintendo_ds_player_sphere_material_is_untextured() {
            city.rendering.tools.TiltTrialPlayerSphereMarbleMaterialFactory factory = new city.rendering.tools.TiltTrialPlayerSphereMarbleMaterialFactory();
            MethodInfo createDefinitionMethod = typeof(city.rendering.tools.TiltTrialPlayerSphereMarbleMaterialFactory).GetMethod(
                "CreateDefinition",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

            city.rendering.tools.GeneratedMaterialAssetDefinition definition = Assert.IsType<city.rendering.tools.GeneratedMaterialAssetDefinition>(
                createDefinitionMethod.Invoke(factory, new object[] { "diffuse-texture-id", "roughness-texture-id" }));
            city.rendering.tools.GeneratedMaterialPlatformDefinition dsDefinition = definition.Platforms["ds"];

            Assert.Equal("ds-standard-lit", dsDefinition.SchemaId);
            Assert.False(dsDefinition.FieldValues.ContainsKey("texture-id"));
            Assert.False(dsDefinition.FieldValues.ContainsKey("texture-relative-path"));
            Assert.Equal("#FFFFFFFF", dsDefinition.FieldValues["base-color"]);
            Assert.Equal("lit", dsDefinition.FieldValues["lighting-mode"]);
        }

        /// <summary>
        /// Ensures repeated public generation keeps the native material identity stable.
        /// </summary>
        [Fact]
        public void Walnut_material_definition_uses_a_repeatable_authoring_identity() {
            city.rendering.tools.TiltTrialPlayerSphereWalnutMaterialFactory factory = new city.rendering.tools.TiltTrialPlayerSphereWalnutMaterialFactory();
            MethodInfo createDefinitionMethod = typeof(city.rendering.tools.TiltTrialPlayerSphereWalnutMaterialFactory).GetMethod(
                "CreateDefinition",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

            city.rendering.tools.GeneratedMaterialAssetDefinition first = Assert.IsType<city.rendering.tools.GeneratedMaterialAssetDefinition>(
                createDefinitionMethod.Invoke(factory, new object[] { "diffuse-texture-id" }));
            city.rendering.tools.GeneratedMaterialAssetDefinition second = Assert.IsType<city.rendering.tools.GeneratedMaterialAssetDefinition>(
                createDefinitionMethod.Invoke(factory, new object[] { "diffuse-texture-id" }));

            Assert.False(string.IsNullOrWhiteSpace(first.MaterialAsset.AuthoringAssetId));
            Assert.Equal(first.MaterialAsset.AuthoringAssetId, second.MaterialAsset.AuthoringAssetId);
        }
    }
}
