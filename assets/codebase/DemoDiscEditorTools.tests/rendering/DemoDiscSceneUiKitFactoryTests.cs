using helengine;
using DemoDisc.EditorTools;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
    public sealed class DemoDiscSceneUiKitFactoryTests {
        [Fact]
        public void CreateStandardSceneUi_WhenRootIsNotClickable_DisablesPointerReturn() {
            string projectRoot = Path.Combine(Path.GetTempPath(), "demodisc-scene-ui-kit-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRoot);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRoot);
                graph.RendererResources.SetDefaultFontAsset(new FontAsset(new FontInfo("SceneUiTest", 16, 4f), null, new Dictionary<char, FontChar>(), 16f, 1, 1));
                IEditorProjectAuthoringSession session = SoftwarePathTracerSceneFactoryTests.ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(projectRoot));

                Entity uiRoot = new DemoDiscSceneUiKitFactory(session).CreateStandardSceneUi("TestSceneUi", string.Empty);

                DemoDiscReturnToMenuComponent returnAction =
                    Assert.Single(uiRoot.Components.OfType<DemoDiscReturnToMenuComponent>());
                Assert.Empty(uiRoot.Components.OfType<InteractableComponent>());
                Assert.False(returnAction.AllowPointerReturn);
            } finally {
                if (Directory.Exists(projectRoot)) {
                    Directory.Delete(projectRoot, true);
                }
            }
        }
    }
}