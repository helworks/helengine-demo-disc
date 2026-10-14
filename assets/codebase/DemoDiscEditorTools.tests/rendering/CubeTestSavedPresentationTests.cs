using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Guards the saved Cube Test scene and the shared instruction Blueprint on disk, not the live generator output: the
    /// scene once shipped with the editor's preview-scaled transforms, so these tests read the committed assets back.
    /// </summary>
    public sealed class CubeTestSavedPresentationTests {
        /// <summary>
        /// Height of the 1280 by 720 reference canvas the overlays are authored against.
        /// </summary>
        const float ReferenceCanvasHeight = 720f;

        /// <summary>
        /// Reads the saved light swatch and checks it sits on the instruction panel's light row at its authored size.
        /// </summary>
        [Fact]
        public void Saved_cube_test_places_the_light_swatch_on_the_panel_light_row() {
            SceneAsset scene = LoadScene();
            SceneEntityAsset swatch = Find(scene.RootEntities, DemoDiscLightIndicatorOverlayFactory.IndicatorSwatchEntityName);
            RoundedRectComponent rect = ReadBaseComponent<RoundedRectComponent>(swatch, "helengine.RoundedRectComponent");

            Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft + DemoSceneInstructionOverlayFactory.LightSwatchPanelLeft, swatch.LocalPosition.X);
            Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop + DemoSceneInstructionOverlayFactory.LightSwatchPanelTop, swatch.LocalPosition.Y);
            Assert.Equal(new int2(DemoSceneInstructionOverlayFactory.LightSwatchSize, DemoSceneInstructionOverlayFactory.LightSwatchSize), rect.Size);
        }

        /// <summary>
        /// Checks every scene-local viewport root and the legacy panel keep their reference-canvas placement.
        /// </summary>
        [Fact]
        public void Saved_cube_test_keeps_its_viewport_roots_at_the_origin() {
            SceneAsset scene = LoadScene();
            foreach (string name in new[] { "DemoSceneInstructionViewport", DemoDiscLightIndicatorOverlayFactory.IndicatorViewportEntityName, "DemoDiscSceneLabelViewport" }) {
                SceneEntityAsset root = Find(scene.RootEntities, name);
                Assert.True(root.LocalPosition.X == 0f && root.LocalPosition.Y == 0f && root.LocalPosition.Z == 0f, name + " must sit at the canvas origin.");
                Assert.True(root.LocalScale.X == 1f && root.LocalScale.Y == 1f, name + " must keep unit scale.");
            }

            SceneEntityAsset panel = Find(scene.RootEntities, "DemoSceneInstructionPanel");
            Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft, panel.LocalPosition.X);
            Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop, panel.LocalPosition.Y);
        }

        /// <summary>
        /// Checks the saved scene carries the generator-authored Android presentation: Blueprint instructions instead of the
        /// legacy panel, density viewports, scalable glyphs, edge-anchored swatch and label, and pinned entity references.
        /// </summary>
        [Fact]
        public void Saved_cube_test_carries_the_android_presentation() {
            SceneAsset scene = LoadScene();
            SceneEntityAsset legacy = Find(scene.RootEntities, "DemoSceneInstructionViewport");
            SceneEntityPlatformExistenceOverrideAsset legacyExistence = Assert.Single(legacy.PlatformExistenceOverrides, IsAndroid);
            Assert.False(legacyExistence.Exists);
            Assert.DoesNotContain(Find(scene.RootEntities, "ConsoleCameraLightInstructions").PlatformExistenceOverrides, IsAndroid);

            SceneEntityAsset ui = FindRoot(scene, "CubeTestUi");
            FPSComponent fps = ReadAndroidOverride<FPSComponent>(ui, "helengine.FPSComponent", "NativeFontPixels");
            Assert.Equal(TextNativePixelMode.Disabled, fps.NativeFontPixels);
            ViewportComponent fpsViewport = ReadAndroidAddedComponent<ViewportComponent>(ui, AndroidDensityPresentationAuthoringService.FpsDensityViewportComponentKey);
            Assert.Equal(ViewportComponent.DensityScalingMode, fpsViewport.ScalingMode);
            Assert.Equal(AndroidDensityPresentationAuthoringService.AndroidUiScale, fpsViewport.UiScale);

            foreach (string name in new[] { DemoDiscLightIndicatorOverlayFactory.IndicatorViewportEntityName, "DemoDiscSceneLabelViewport" }) {
                AssertDensityViewport(Find(new[] { ui }, name));
            }

            SceneEntityAsset label = Find(new[] { ui }, DemoDiscSceneLabelOverlayFactory.LabelEntityName);
            Assert.Equal(TextNativePixelMode.Disabled, ReadAndroidOverride<TextComponent>(label, "helengine.TextComponent", "NativeFontPixels").NativeFontPixels);
            TextComponent labelText = ReadBaseComponent<TextComponent>(label, "helengine.TextComponent");
            LayoutComponent labelAnchor = ReadAndroidAddedComponent<LayoutComponent>(label, AndroidDensityPresentationAuthoringService.EdgeAnchorLayoutComponentKey);
            Assert.Equal((byte)(LayoutComponent.RightAnchorFlag | LayoutComponent.TopAnchorFlag), labelAnchor.AnchorFlags);
            Assert.Equal(new float4(0f, 1280f - label.LocalPosition.X - labelText.Size.X, label.LocalPosition.Y, 0f), labelAnchor.AnchorDistances);

            SceneEntityAsset swatch = Find(new[] { ui }, DemoDiscLightIndicatorOverlayFactory.IndicatorSwatchEntityName);
            LayoutComponent swatchAnchor = ReadAndroidAddedComponent<LayoutComponent>(swatch, AndroidDensityPresentationAuthoringService.EdgeAnchorLayoutComponentKey);
            float swatchTop = DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop + DemoSceneInstructionOverlayFactory.LightSwatchPanelTop;
            Assert.Equal((byte)(LayoutComponent.LeftAnchorFlag | LayoutComponent.BottomAnchorFlag), swatchAnchor.AnchorFlags);
            Assert.Equal(new float4(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft + DemoSceneInstructionOverlayFactory.LightSwatchPanelLeft, 0f, 0f,
                ReferenceCanvasHeight - swatchTop - DemoSceneInstructionOverlayFactory.LightSwatchSize), swatchAnchor.AnchorDistances);

            DemoDiscLightToggleComponent toggle = ReadAndroidOverride<DemoDiscLightToggleComponent>(ui,
                AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(DemoDiscLightToggleComponent)), nameof(DemoDiscLightToggleComponent.IndicatorSwatchEntityReference));
            Assert.Equal(swatch.Id, toggle.IndicatorSwatchEntityReference?.EntityId);
            DemoDiscDebugSceneLabelComponent sceneLabel = ReadAndroidOverride<DemoDiscDebugSceneLabelComponent>(ui,
                AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(DemoDiscDebugSceneLabelComponent)), nameof(DemoDiscDebugSceneLabelComponent.LabelEntityReference));
            Assert.Equal(label.Id, sceneLabel.LabelEntityReference?.EntityId);
        }

        /// <summary>
        /// Checks the saved shared instruction Blueprint carries its generator-authored Android presentation.
        /// </summary>
        [Fact]
        public void Saved_instruction_blueprint_carries_the_android_presentation() {
            string path = DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "blueprints", "ui", "ConsoleCameraLightInstructions.hblueprint");
            using FileStream stream = File.OpenRead(path);
            BlueprintAsset blueprint = Assert.IsType<BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));

            AssertDensityViewport(blueprint.RootEntity);
            SceneEntityAsset panel = Find(new[] { blueprint.RootEntity }, "ConsoleCameraLightInstructionsPanel");
            RoundedRectComponent rect = ReadBaseComponent<RoundedRectComponent>(panel, "helengine.RoundedRectComponent");
            LayoutComponent anchor = ReadAndroidAddedComponent<LayoutComponent>(panel, AndroidDensityPresentationAuthoringService.EdgeAnchorLayoutComponentKey);
            Assert.Equal((byte)(LayoutComponent.LeftAnchorFlag | LayoutComponent.BottomAnchorFlag), anchor.AnchorFlags);
            Assert.Equal(new float4(panel.LocalPosition.X, 0f, 0f, ReferenceCanvasHeight - panel.LocalPosition.Y - rect.Size.Y), anchor.AnchorDistances);
            foreach (string name in new[] { "CameraText", "LightIconText" }) {
                TextComponent text = ReadAndroidOverride<TextComponent>(Find(new[] { panel }, name), "helengine.TextComponent", "NativeFontPixels");
                Assert.Equal(TextNativePixelMode.Disabled, text.NativeFontPixels);
            }
        }

        /// <summary>
        /// Asserts one entity's viewport switches to density scaling with the approved Android multiplier.
        /// </summary>
        static void AssertDensityViewport(SceneEntityAsset entity) {
            ViewportComponent viewport = ReadAndroidOverride<ViewportComponent>(entity, "helengine.ViewportComponent", "ScalingMode", "PixelsPerCentimeter", "UiScale");
            Assert.Equal(ViewportComponent.DensityScalingMode, viewport.ScalingMode);
            Assert.Equal(AndroidDensityPresentationAuthoringService.AndroidPixelsPerCentimeter, viewport.PixelsPerCentimeter);
            Assert.Equal(AndroidDensityPresentationAuthoringService.AndroidUiScale, viewport.UiScale);
        }

        /// <summary>
        /// Loads the committed Cube Test scene asset.
        /// </summary>
        static SceneAsset LoadScene() {
            string path = DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "rendering", "cube_test.helen");
            using FileStream stream = File.OpenRead(path);
            return (SceneAsset)global::helengine.AssetSerializer.Deserialize(stream);
        }

        /// <summary>
        /// Returns whether a serialized override scope is the one-step Android platform scope.
        /// </summary>
        static bool IsAndroid(SceneEntityPlatformExistenceOverrideAsset item) {
            return EditorOverrideScope.FromSteps(item.Scope).Equals(AndroidDensityPresentationAuthoringService.AndroidScope);
        }

        /// <summary>
        /// Deserializes the common value of one component, unwrapping its platform override container.
        /// </summary>
        static TComponent ReadBaseComponent<TComponent>(SceneEntityAsset entity, string typeId) where TComponent : Component {
            SceneComponentAssetRecord record = Assert.Single(entity.Components, item => item.ComponentTypeId == typeId);
            SceneComponentAssetRecord baseRecord = new ComponentPlatformOverridePayloadService().UnwrapBaseRecord(record);
            return (TComponent)new ComponentPersistenceRegistry().GetDescriptor(typeId).DeserializeComponent(baseRecord, new EntitySaveComponent(), null);
        }

        /// <summary>
        /// Deserializes the Android override payload of one component and checks exactly the expected properties are overridden.
        /// </summary>
        static TComponent ReadAndroidOverride<TComponent>(SceneEntityAsset entity, string typeId, params string[] properties) where TComponent : Component {
            ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
            SceneComponentAssetRecord record = Assert.Single(entity.Components, item => item.ComponentTypeId == typeId);
            EntityComponentPlatformOverrideState android = Assert.Single(payloads.ReadOverrideStates(record),
                state => state.Scope.Equals(AndroidDensityPresentationAuthoringService.AndroidScope));
            Assert.Equal(properties.OrderBy(name => name, StringComparer.Ordinal), android.EnumeratePropertyOverrides().OrderBy(name => name, StringComparer.Ordinal));
            SceneComponentAssetRecord baseRecord = payloads.UnwrapBaseRecord(record);
            SceneComponentAssetRecord overrideRecord = new SceneComponentAssetRecord {
                ComponentTypeId = baseRecord.ComponentTypeId,
                ComponentKey = baseRecord.ComponentKey,
                ComponentIndex = baseRecord.ComponentIndex,
                Payload = android.Payload
            };
            return (TComponent)new ComponentPersistenceRegistry().GetDescriptor(typeId).DeserializeComponent(overrideRecord, new EntitySaveComponent(), null);
        }

        /// <summary>
        /// Deserializes one Android-only component added to an entity under a stable key.
        /// </summary>
        static TComponent ReadAndroidAddedComponent<TComponent>(SceneEntityAsset entity, string componentKey) where TComponent : Component {
            SceneEntityPlatformComponentOverrideAsset android = Assert.Single(entity.PlatformComponentOverrides,
                item => EditorOverrideScope.FromSteps(item.Scope).Equals(AndroidDensityPresentationAuthoringService.AndroidScope));
            SceneComponentAssetRecord record = Assert.Single(android.AddedComponents, item => item.Component.ComponentKey == componentKey).Component;
            return (TComponent)new ComponentPersistenceRegistry().GetDescriptor(record.ComponentTypeId).DeserializeComponent(record, new EntitySaveComponent(), null);
        }

        /// <summary>
        /// Finds one top-level scene root by name.
        /// </summary>
        static SceneEntityAsset FindRoot(SceneAsset scene, string name) {
            return Assert.Single(scene.RootEntities, root => root.Name == name);
        }

        /// <summary>
        /// Finds the first entity with the supplied name beneath the given roots, depth first.
        /// </summary>
        static SceneEntityAsset Find(SceneEntityAsset[] roots, string name) {
            foreach (SceneEntityAsset root in roots ?? Array.Empty<SceneEntityAsset>()) {
                SceneEntityAsset found = Find(root, name);
                if (found != null) {
                    return found;
                }
            }

            throw new InvalidOperationException($"Saved asset is missing '{name}'.");
        }

        /// <summary>
        /// Depth-first search for one named entity.
        /// </summary>
        static SceneEntityAsset Find(SceneEntityAsset entity, string name) {
            if (entity.Name == name) {
                return entity;
            }

            foreach (SceneEntityAsset child in entity.Children ?? Array.Empty<SceneEntityAsset>()) {
                SceneEntityAsset found = Find(child, name);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }
    }
}
