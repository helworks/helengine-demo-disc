using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>Checks authored UI depth independently of the parent's translation and scale.</summary>
    public sealed class SceneUiDepthTests {
        /// <summary>Changing depth retains world XY and orders nested labels ahead of their panel.</summary>
        [Fact]
        public void WorldDepth_PreservesXYUnderScaledParent() {
            string root = Path.Combine(AppContext.BaseDirectory, "test-assets", "ui-depth", Guid.NewGuid().ToString("N"));
            using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(root);
            IEditorProjectAuthoringSession session = graph.CreateAuthoringSession(root);
            Entity parent = session.OwningCore.EntityFactory.Create("Panel");
            parent.LocalPosition = new float3(10f, 20f, 200f);
            parent.LocalScale = new float3(2f, 3f, 4f);
            Entity label = session.OwningCore.EntityFactory.CreateChild(parent, "Label");
            label.LocalPosition = new float3(5f, 6f, 1f);
            float3 before = label.Position;

            SceneUiDepth.SetWorldDepth(label, 202f);

            Assert.Equal(before.X, label.Position.X);
            Assert.Equal(before.Y, label.Position.Y);
            Assert.Equal(202f, label.Position.Z);
            Assert.True(label.Position.Z > parent.Position.Z);
        }
    }
}
