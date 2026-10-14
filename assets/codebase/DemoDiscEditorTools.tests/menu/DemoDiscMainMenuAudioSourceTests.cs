using helengine;
using helengine.editor;
using DemoDisc.EditorTools;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the generated demo-disc menu scenes remain silent until music is intentionally reintroduced.
    /// </summary>
    public sealed class DemoDiscMainMenuAudioSourceTests {
        static string ProjectRootPath => global::DemoDisc.testing.DemoDiscTestProject.RootPath;
        /// <summary>
        /// Ensures the persisted standard and handheld menu scenes contain no serialized audio source component or menu music reference.
        /// </summary>
        [Fact]
        public void Generated_menu_scenes_are_silent() {
            SceneAsset standardScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneAsset handheldScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenuHandheld.helen");
            string audioSourceComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(AudioSourceComponent));

            Assert.DoesNotContain(standardScene.AssetReferences, reference => reference != null && reference.RelativePath.Contains("audio/", StringComparison.Ordinal));
            Assert.DoesNotContain(handheldScene.AssetReferences, reference => reference != null && reference.RelativePath.Contains("audio/", StringComparison.Ordinal));
            Assert.DoesNotContain(FlattenComponents(standardScene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
            Assert.DoesNotContain(FlattenComponents(handheldScene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
        }

        /// <summary>Ensures generated menu content is identical across platform builds.</summary>
        [Fact]
        public void Generated_menu_items_are_not_filtered_by_platform_scene_packages() {
            SceneAsset standardScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneAsset handheldScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenuHandheld.helen");

            AssertMenuItemsHaveNoPlatformExistenceOverrides(standardScene);
            AssertMenuItemsHaveNoPlatformExistenceOverrides(handheldScene);
        }

        static void AssertMenuItemsHaveNoPlatformExistenceOverrides(SceneAsset scene) {
            SceneEntityAsset[] menuEntities = EnumerateEntities(scene.RootEntities)
                .Where(entity => entity.Name.StartsWith("Item-", StringComparison.Ordinal)
                    || entity.Name.StartsWith("Panel-", StringComparison.Ordinal))
                .ToArray();

            Assert.NotEmpty(menuEntities);
            Assert.DoesNotContain(menuEntities, entity => entity.PlatformExistenceOverrides.Length > 0);
        }

        [Fact]
        public void Standard_menu_uses_viewport_scaling_for_the_shared_720p_layout() {
            SceneAsset scene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneEntityAsset menuRoot = Assert.Single(scene.RootEntities, entity => entity.Name == "DemoDiscMenuRoot");
            string viewportTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(ViewportComponent));
            string canvasFitTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(ReferenceCanvasFitComponent));
            SceneComponentAssetRecord viewportRecord = Assert.Single(
                menuRoot.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == viewportTypeId);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            ViewportComponent viewport = Assert.IsType<ViewportComponent>(
                registry.GetDescriptor(viewportRecord.ComponentTypeId).DeserializeComponent(viewportRecord, new EntitySaveComponent(), null));

            Assert.Equal(ViewportComponent.ScreenBindingMode, viewport.BindingMode);
            Assert.Equal(ViewportComponent.ReferenceCanvasStretchScalingMode, viewport.ScalingMode);
            Assert.Equal(new int2(1280, 720), viewport.FixedSize);
            Assert.Equal(1280, viewport.ReferenceWidth);
            Assert.Equal(720, viewport.ReferenceHeight);
            Assert.DoesNotContain(FlattenComponents(scene.RootEntities), component => component.ComponentTypeId == canvasFitTypeId);
        }

        [Fact]
        public void Standard_menu_restores_navigation_and_uses_square_n64_surfaces() {
            SceneAsset scene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneEntityAsset menuRoot = Assert.Single(scene.RootEntities, entity => entity.Name == "DemoDiscMenuRoot");
            SceneEntityAsset generatedRoot = Assert.Single(menuRoot.Children, entity => entity.Name == DemoMenuLayout.GeneratedRootEntityName);

            string menuComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(MenuComponent));
            Assert.Contains(menuRoot.Components ?? Array.Empty<SceneComponentAssetRecord>(), component => component.ComponentTypeId == menuComponentTypeId);
            Assert.Contains(generatedRoot.Children, entity => entity.Name == "DemoDiscAnimatedBackground");
            Assert.Contains(generatedRoot.Children, entity => entity.Name == "DemoDiscOverlayImage");
            Assert.Contains(generatedRoot.Children, entity => entity.Name == "DemoDiscPlatformInfoOverlay");
            Assert.Contains(generatedRoot.Children, entity => entity.Name == "DemoDiscFooterIdentitySurface");

            SceneEntityAsset mainPanel = Assert.Single(generatedRoot.Children, entity => entity.Name == "Panel-main");
            SceneEntityAsset panelSurface = Assert.Single(mainPanel.Children, entity => entity.Name == "panel-main-surface");
            AssertSquareCornersForN64(panelSurface, 18f);

            SceneEntityAsset mainItem = Assert.Single(EnumerateEntities(mainPanel), entity => entity.Name == "Item-main-scenes");
            AssertSquareCornersForN64(mainItem, 7.2f);
        }

        static void AssertSquareCornersForN64(SceneEntityAsset entity, float commonRadius) {
            string roundedRectTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(RoundedRectComponent));
            SceneComponentAssetRecord persistedComponent = Assert.Single(
                entity.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == roundedRectTypeId);
            ComponentPlatformOverridePayloadService payloadService = new ComponentPlatformOverridePayloadService();
            SceneComponentAssetRecord commonRecord = payloadService.UnwrapBaseRecord(persistedComponent);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            RoundedRectComponent commonComponent = Assert.IsType<RoundedRectComponent>(
                registry.GetDescriptor(roundedRectTypeId).DeserializeComponent(commonRecord, new EntitySaveComponent(), null));
            Assert.Equal(commonRadius, commonComponent.Radius);

            EntityComponentPlatformOverrideState n64Override = Assert.Single(
                payloadService.ReadOverrideStates(persistedComponent),
                overrideState => overrideState.Scope.TryGetStepId(SceneOverrideScopeStepKind.Platform, out string platformId)
                    && platformId == "n64");
            Assert.Contains(nameof(RoundedRectComponent.Radius), n64Override.EnumeratePropertyOverrides());
            EditorTaggedSceneComponentFieldReader fields = new EditorTaggedSceneComponentFieldReader(n64Override.Payload);
            Assert.True(fields.TryGetFieldReader(nameof(RoundedRectComponent.Radius), out EngineBinaryReader radius));
            using (radius) Assert.Equal(0f, radius.ReadSingle());
        }

        static IEnumerable<SceneEntityAsset> EnumerateEntities(SceneEntityAsset root) {
            if (root == null) yield break;
            yield return root;
            foreach (SceneEntityAsset child in root.Children ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneEntityAsset descendant in EnumerateEntities(child)) yield return descendant;
            }
        }
        static IEnumerable<SceneEntityAsset> EnumerateEntities(SceneEntityAsset[] roots) {
            if (roots == null) yield break;
            foreach (SceneEntityAsset root in roots) {
                foreach (SceneEntityAsset entity in EnumerateEntities(root)) yield return entity;
            }
        }
        static SceneAsset LoadSceneAsset(string relativePath) {
            string fullPath = Path.Combine(ProjectRootPath, relativePath);
            using FileStream stream = File.OpenRead(fullPath);
            return Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
        }

        static IReadOnlyList<SceneComponentAssetRecord> FlattenComponents(SceneEntityAsset[] rootEntities) {
            List<SceneComponentAssetRecord> components = new List<SceneComponentAssetRecord>();
            AppendComponents(rootEntities, components);
            return components;
        }

        static void AppendComponents(SceneEntityAsset[] entities, List<SceneComponentAssetRecord> components) {
            if (entities == null) {
                return;
            }

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                SceneEntityAsset entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }

                SceneComponentAssetRecord[] entityComponents = entity.Components ?? Array.Empty<SceneComponentAssetRecord>();
                for (int componentIndex = 0; componentIndex < entityComponents.Length; componentIndex++) {
                    SceneComponentAssetRecord component = entityComponents[componentIndex];
                    if (component != null) {
                        components.Add(component);
                    }
                }

                AppendComponents(entity.Children, components);
            }
        }
    }
}
