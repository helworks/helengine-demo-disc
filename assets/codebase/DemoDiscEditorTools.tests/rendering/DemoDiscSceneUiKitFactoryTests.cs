using helengine;
using DemoDisc.EditorTools;
using DemoDisc.menu;
using DemoDisc.rendering;

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

        /// <summary>
        /// Ensures every text and FPS component of the standard scene UI draws the shared scene UI font at native pixel size
        /// with a font scale of one, and saves a reference to that font.
        /// </summary>
        [Fact]
        public void CreateStandardSceneUi_DrawsAllTextWithTheSharedSceneUiFontAtNativePixelSize() {
            string projectRoot = Path.Combine(Path.GetTempPath(), "demodisc-scene-ui-font-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRoot);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRoot);
                graph.RendererResources.SetDefaultFontAsset(new FontAsset(new FontInfo("SceneUiTest", 16, 4f), null, new Dictionary<char, FontChar>(), 16f, 1, 1));
                IEditorProjectAuthoringSession session = SoftwarePathTracerSceneFactoryTests.ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(projectRoot));

                Entity uiRoot = new DemoDiscSceneUiKitFactory(session).CreateStandardSceneUi("TestSceneUi", "1. Test Scene");

                Entity[] entities = EnumerateEntities(uiRoot).ToArray();
                FPSComponent fps = Assert.Single(entities.SelectMany(entity => entity.Components.OfType<FPSComponent>()));
                Assert.Equal(TextNativePixelMode.RoundUp, fps.NativeFontPixels);
                Assert.Equal(1f, fps.FontScale);
                AssertSavesSceneUiFont(uiRoot, fps);

                TextComponent[] texts = entities.SelectMany(entity => entity.Components.OfType<TextComponent>()).ToArray();
                Assert.Single(texts);
                foreach (Entity entity in entities) {
                    foreach (TextComponent text in entity.Components.OfType<TextComponent>()) {
                        Assert.Equal(TextNativePixelMode.RoundUp, text.NativeFontPixels);
                        Assert.Equal(1f, text.FontScale);
                        AssertSavesSceneUiFont(entity, text);
                    }
                }
            } finally {
                if (Directory.Exists(projectRoot)) {
                    Directory.Delete(projectRoot, true);
                }
            }
        }

        /// <summary>
        /// Ensures the light indicator is only the swatch, sitting inside the instruction panel on the light row after the label.
        /// </summary>
        [Fact]
        public void CreateStandardSceneUi_PlacesTheLightSwatchOnThePanelLightRow() {
            string projectRoot = Path.Combine(Path.GetTempPath(), "demodisc-light-indicator-order-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRoot);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRoot);
                graph.RendererResources.SetDefaultFontAsset(new FontAsset(new FontInfo("SceneUiTest", 16, 4f), null, new Dictionary<char, FontChar>(), 16f, 1, 1));
                IEditorProjectAuthoringSession session = SoftwarePathTracerSceneFactoryTests.ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(projectRoot));

                Entity uiRoot = new DemoDiscSceneUiKitFactory(session).CreateStandardSceneUi("TestSceneUi", string.Empty);

                Entity[] entities = EnumerateEntities(uiRoot).ToArray();
                Entity viewport = entities.Single(entity => Assert.IsType<EditorEntity>(entity).Name == DemoDiscLightIndicatorOverlayFactory.IndicatorViewportEntityName);
                Entity swatch = Assert.Single(viewport.Children);
                Assert.Equal(DemoDiscLightIndicatorOverlayFactory.IndicatorSwatchEntityName, Assert.IsType<EditorEntity>(swatch).Name);
                Assert.Empty(EnumerateEntities(viewport).SelectMany(entity => entity.Components.OfType<TextComponent>()));

                RoundedRectComponent swatchRect = Assert.Single(swatch.Components.OfType<RoundedRectComponent>());
                Assert.Equal(new int2(DemoSceneInstructionOverlayFactory.LightSwatchSize, DemoSceneInstructionOverlayFactory.LightSwatchSize), swatchRect.Size);
                Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft + DemoSceneInstructionOverlayFactory.LightSwatchPanelLeft, swatch.LocalPosition.X);
                Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop + DemoSceneInstructionOverlayFactory.LightSwatchPanelTop, swatch.LocalPosition.Y);
            } finally {
                if (Directory.Exists(projectRoot)) {
                    Directory.Delete(projectRoot, true);
                }
            }
        }

        /// <summary>
        /// Asserts that the editor save state of one entity stores the shared scene UI font as the component Font reference.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose saved Font reference is verified.</param>
        static void AssertSavesSceneUiFont(Entity entity, Component component) {
            EntitySaveComponent saveComponent = Assert.Single(entity.Components.OfType<EntitySaveComponent>());
            Assert.True(saveComponent.TryGetComponentState(component, out EntityComponentSaveState saveState));
            Assert.True(saveState.TryGetAssetReference("Font", out SceneAssetReference fontReference));
            Assert.Equal(DemoDiscSceneUiFont.RelativePath, fontReference.RelativePath);
        }

        /// <summary>
        /// Enumerates one entity and all of its descendants.
        /// </summary>
        /// <param name="root">Entity whose subtree is enumerated.</param>
        /// <returns>The entity followed by every descendant.</returns>
        static IEnumerable<Entity> EnumerateEntities(Entity root) {
            yield return root;
            for (int index = 0; index < root.Children.Count; index++) {
                foreach (Entity descendant in EnumerateEntities(root.Children[index])) {
                    yield return descendant;
                }
            }
        }
    }
}