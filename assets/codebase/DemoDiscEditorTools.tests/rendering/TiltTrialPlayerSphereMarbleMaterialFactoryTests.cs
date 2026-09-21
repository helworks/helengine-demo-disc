using System.Reflection;
using DemoDisc.EditorTools;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies platform-specific material settings for the Tilt Trial player sphere.
    /// </summary>
    public sealed class TiltTrialPlayerSphereMarbleMaterialFactoryTests {
        /// <summary>
        /// Ensures Nintendo DS uses the lit solid-color path so the sphere consumes no texture slot.
        /// </summary>
        [Fact]
        public void Nintendo_ds_player_sphere_material_is_untextured() {
            string projectRootPath = Path.Combine(Path.GetTempPath(), "city-marble-material-tests", Guid.NewGuid().ToString("N"));
            using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            using EditorAuthoringTransaction transaction = authoringSession.BeginTransaction();
            DemoDisc.EditorTools.TiltTrialPlayerSphereMarbleMaterialFactory factory = new DemoDisc.EditorTools.TiltTrialPlayerSphereMarbleMaterialFactory(
                authoringSession,
                transaction);
            MethodInfo createDefinitionMethod = typeof(DemoDisc.EditorTools.TiltTrialPlayerSphereMarbleMaterialFactory).GetMethod(
                "CreateDefinition",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

            DemoDisc.EditorTools.GeneratedMaterialAssetDefinition definition = Assert.IsType<DemoDisc.EditorTools.GeneratedMaterialAssetDefinition>(
                createDefinitionMethod.Invoke(factory, new object[] { "diffuse-texture-id", "roughness-texture-id" }));
            DemoDisc.EditorTools.GeneratedMaterialPlatformDefinition dsDefinition = definition.Platforms["ds"];

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
            string projectRootPath = Path.Combine(Path.GetTempPath(), "city-walnut-material-tests", Guid.NewGuid().ToString("N"));
            using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            using EditorAuthoringTransaction transaction = authoringSession.BeginTransaction();
            DemoDisc.EditorTools.TiltTrialPlayerSphereWalnutMaterialFactory factory = new DemoDisc.EditorTools.TiltTrialPlayerSphereWalnutMaterialFactory(
                authoringSession,
                transaction);
            MethodInfo createDefinitionMethod = typeof(DemoDisc.EditorTools.TiltTrialPlayerSphereWalnutMaterialFactory).GetMethod(
                "CreateDefinition",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

            DemoDisc.EditorTools.GeneratedMaterialAssetDefinition first = Assert.IsType<DemoDisc.EditorTools.GeneratedMaterialAssetDefinition>(
                createDefinitionMethod.Invoke(factory, new object[] { "diffuse-texture-id" }));
            DemoDisc.EditorTools.GeneratedMaterialAssetDefinition second = Assert.IsType<DemoDisc.EditorTools.GeneratedMaterialAssetDefinition>(
                createDefinitionMethod.Invoke(factory, new object[] { "diffuse-texture-id" }));

            Assert.False(string.IsNullOrWhiteSpace(first.MaterialAsset.AuthoringAssetId));
            Assert.Equal(first.MaterialAsset.AuthoringAssetId, second.MaterialAsset.AuthoringAssetId);
        }
    }
}
