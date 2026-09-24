using helengine;
using DemoDisc.EditorTools;
using DemoDisc.rendering;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
    public sealed class FixedSceneReferenceAuthoringTests {
        [Fact]
        public void StandardSceneUi_AuthorsLightAndSwatchEntityReferences() {
            string projectRoot = Path.Combine(Path.GetTempPath(), "demodisc-fixed-scene-reference-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRoot);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRoot);
                graph.RendererResources.SetDefaultFontAsset(new FontAsset(new FontInfo("SceneReferenceTest", 16, 4f), null, new Dictionary<char, FontChar>(), 16f, 1, 1));
                IEditorProjectAuthoringSession session = SoftwarePathTracerSceneFactoryTests.ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(projectRoot));
                Entity light = session.OwningCore.EntityFactory.Create("AuthoredLight");
                light.AddComponent(new DirectionalLightComponent { Intensity = 2f });

                Entity uiRoot = new DemoDiscSceneUiKitFactory(session).CreateStandardSceneUi("TestSceneUi", string.Empty, new[] { light });
                DemoDiscLightToggleComponent toggle = Assert.Single(uiRoot.Components.OfType<DemoDiscLightToggleComponent>());
                SceneEntityReference lightReference = Assert.Single(toggle.LightEntityReferences);
                Entity swatch = FindSwatch(uiRoot);

                Assert.Equal(GetEntityId(light), lightReference.EntityId);
                Assert.NotNull(toggle.IndicatorSwatchEntityReference);
                Assert.Equal(GetEntityId(swatch), toggle.IndicatorSwatchEntityReference.EntityId);
            } finally {
                if (Directory.Exists(projectRoot)) {
                    Directory.Delete(projectRoot, true);
                }
            }
        }

        [Fact]
        public void LoadingScreenFactory_ReferencesItsAuthoredRectangles() {
            string projectRoot = Path.Combine(Path.GetTempPath(), "demodisc-loading-reference-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRoot);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRoot);
                IEditorProjectAuthoringSession session = SoftwarePathTracerSceneFactoryTests.ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(projectRoot));
                GeneratedAuthoringSceneDefinition definition = new SceneLoadingScreenFactory(session).CreateSceneDefinition();
                SceneLoadingScreenComponent component = FindComponent<SceneLoadingScreenComponent>(definition.RootEntities);

                Entity background = FindEntityById(definition.RootEntities, component.BackgroundEntityReference.EntityId);
                Entity track = FindEntityById(definition.RootEntities, component.TrackEntityReference.EntityId);
                Entity fill = FindEntityById(definition.RootEntities, component.FillEntityReference.EntityId);
                Assert.NotNull(Assert.Single(background.Components.OfType<RoundedRectComponent>()));
                Assert.NotNull(Assert.Single(track.Components.OfType<RoundedRectComponent>()));
                Assert.NotNull(Assert.Single(fill.Components.OfType<RoundedRectComponent>()));
            } finally {
                if (Directory.Exists(projectRoot)) Directory.Delete(projectRoot, true);
            }
        }

        [Fact]
        public void SplashFactory_ReferencesItsAuthoredBackgroundAndLogo() {
            string projectRoot = Path.Combine(Path.GetTempPath(), "demodisc-splash-reference-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRoot);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRoot);
                IEditorProjectAuthoringSession session = SoftwarePathTracerSceneFactoryTests.ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(projectRoot));
                GeneratedAuthoringSceneDefinition definition = new HelenOfCodeSplashSceneFactory(session).CreateSceneDefinition();
                HelenOfCodeSplashComponent component = FindComponent<HelenOfCodeSplashComponent>(definition.RootEntities);

                Entity background = FindEntityById(definition.RootEntities, component.BackgroundSpriteEntityReference.EntityId);
                Entity logo = FindEntityById(definition.RootEntities, component.LogoSpriteEntityReference.EntityId);
                Assert.NotNull(Assert.Single(background.Components.OfType<RoundedRectComponent>()));
                Assert.NotNull(Assert.Single(logo.Components.OfType<SpriteComponent>()));
            } finally {
                if (Directory.Exists(projectRoot)) Directory.Delete(projectRoot, true);
            }
        }
        static T FindComponent<T>(Entity[] roots) where T : Component {
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++) {
                T found = FindComponent<T>(roots[rootIndex]);
                if (found != null) return found;
            }
            return null;
        }

        static T FindComponent<T>(Entity entity) where T : Component {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is T component) return component;
            }
            for (int index = 0; index < entity.Children.Count; index++) {
                T found = FindComponent<T>(entity.Children[index]);
                if (found != null) return found;
            }
            return null;
        }

        static Entity FindEntityById(Entity[] roots, uint id) {
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++) {
                Entity found = FindEntityById(roots[rootIndex], id);
                if (found != null) return found;
            }
            return null;
        }

        static Entity FindEntityById(Entity entity, uint id) {
            if (GetEntityId(entity) == id) return entity;
            for (int index = 0; index < entity.Children.Count; index++) {
                Entity found = FindEntityById(entity.Children[index], id);
                if (found != null) return found;
            }
            return null;
        }
        static Entity FindSwatch(Entity parent) {
            for (int index = 0; index < parent.Components.Count; index++) {
                if (parent.Components[index] is RoundedRectComponent roundedRect
                    && roundedRect.RenderOrder2D == 252
                    && roundedRect.Size.X == 32
                    && roundedRect.Size.Y == 32) {
                    return parent;
                }
            }

            for (int index = 0; index < parent.Children.Count; index++) {
                Entity found = FindSwatch(parent.Children[index]);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }

        static uint GetEntityId(Entity entity) {
            return Assert.Single(entity.Components.OfType<EntitySaveComponent>()).EntityId;
        }
    }
}