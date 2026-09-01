namespace city.tests {
    /// <summary>
    /// Verifies the authored Tilt Trial camera stays on the intended closer orbit without changing its framing.
    /// </summary>
    public sealed class TiltTrialCameraAuthoringTests {
        /// <summary>
        /// Ensures the authored camera start position keeps the existing orbit angle while using the current tighter close-up view.
        /// </summary>
        [Fact]
        public void Tilt_trial_camera_starts_at_the_current_close_view_pose() {
            string gameSceneSourcePath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs");
            string source = File.ReadAllText(gameSceneSourcePath);

            Assert.Contains("entity.LocalPosition = new float3(0f, 2.74425f, -10.92f);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("entity.LocalPosition = new float3(0f, 2.74425f, -3.08f);", source, StringComparison.Ordinal);
            Assert.Contains("float4.CreateFromYawPitchRoll(MathF.PI, -0.42f, 0f, out orientation);", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the serialized first Tilt Trial gameplay scene carries the current tighter camera pose instead of a stale farther view.
        /// </summary>
        [Fact]
        public void Tilt_trial_scene_asset_camera_uses_the_current_close_view_pose() {
            string sceneAssetPath = global::city.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial_level_01.helen");
            string bytes = BitConverter.ToString(File.ReadAllBytes(sceneAssetPath));

            Assert.Contains("54-69-6C-74-54-72-69-61-6C-43-61-6D-65-72-61-00-01-00-40-00-00-00-00-CB-A1-2F-40-52-B8-2E-C1", bytes, StringComparison.Ordinal);
            Assert.DoesNotContain("54-69-6C-74-54-72-69-61-6C-43-61-6D-65-72-61-00-01-00-40-00-00-00-00-CB-A1-2F-40-B8-1E-45-C0", bytes, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the authored Tilt Trial speed HUD uses the dedicated Fredoka source font instead of the shared editor UI font, with a larger layout for the more playful treatment.
        /// </summary>
        [Fact]
        public void Tilt_trial_speed_hud_uses_fredoka_with_a_larger_layout() {
            string gameSceneSourcePath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs");
            string source = File.ReadAllText(gameSceneSourcePath);
            int speedTextComponentStart = source.IndexOf("TextComponent speedTextComponent = new TextComponent {", StringComparison.Ordinal);
            int speedTextComponentEnd = source.IndexOf("speedTextEntity.AddComponent(speedTextComponent);", StringComparison.Ordinal);
            Assert.True(speedTextComponentStart >= 0);
            Assert.True(speedTextComponentEnd > speedTextComponentStart);
            string speedTextComponentBlock = source.Substring(speedTextComponentStart, speedTextComponentEnd - speedTextComponentStart);

            Assert.Contains("const string TiltTrialSpeedHudFontRelativePath = \"Fonts/Fredoka.ttf\";", source, StringComparison.Ordinal);
            Assert.Contains("FixedSize = new int2(1280, 720)", source, StringComparison.Ordinal);
            Assert.Contains("ReferenceWidth = 1280,", source, StringComparison.Ordinal);
            Assert.Contains("ReferenceHeight = 720", source, StringComparison.Ordinal);
            Assert.Contains("speedTextEntity.LocalPosition = new float3(16f, 600f, 0f);", source, StringComparison.Ordinal);
            Assert.Contains("Text = \"0\\nkm/h\",", source, StringComparison.Ordinal);
            Assert.Contains("Size = new int2(320, 224),", source, StringComparison.Ordinal);
            Assert.Contains("FontScale = 2.2f,", source, StringComparison.Ordinal);
            Assert.Contains("Alignment = TextAlignment.Center,", source, StringComparison.Ordinal);
            Assert.Contains("speedTextAnchorComponent.LayoutSpace = LayoutComponent.CameraViewportLayoutSpace;", source, StringComparison.Ordinal);
            Assert.Contains("speedTextAnchorComponent.SetAnchorDistances(left: 16f, bottom: 16f);", source, StringComparison.Ordinal);
            Assert.Contains("ApplyFontReference(speedTextEntity, speedTextComponent, TiltTrialSpeedHudFontRelativePath);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Text = \"0 km/h\",", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Size = new int2(320, 104),", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Alignment = TextAlignment.Left,", speedTextComponentBlock, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the handheld selector title is owned by the top-screen camera and does not enter the bottom-screen queue.
        /// </summary>
        [Fact]
        public void Tilt_trial_handheld_selector_binds_title_to_the_top_camera() {
            string gameSceneSourcePath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs");
            string source = File.ReadAllText(gameSceneSourcePath);

            Assert.Contains("Entity topCameraEntity = CreateTiltTrialHandheldLevelSelectTopCameraEntity();", source, StringComparison.Ordinal);
            Assert.Contains("CreateHandheldLevelSelectTopInfoEntity(topCameraEntity)", source, StringComparison.Ordinal);
            Assert.Contains("Entity CreateHandheldLevelSelectTopInfoEntity(Entity parent)", source, StringComparison.Ordinal);
            Assert.Contains("OwningCore.EntityFactory.CreateChild(parent, \"TiltTrialHandheldLevelSelectTopInfo\")", source, StringComparison.Ordinal);
            Assert.Contains("BindingMode = ViewportComponent.AncestorCameraBindingMode", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures generated Tilt Trial text points at the source font that the platform cook pipeline can atlas.
        /// </summary>
        [Fact]
        public void Tilt_trial_generated_text_uses_the_cookable_fredoka_reference() {
            string gameSceneSourcePath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs");
            string source = File.ReadAllText(gameSceneSourcePath);

            Assert.Contains("ApplyFontReference(entity, textComponent, TiltTrialSpeedHudFontRelativePath);", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the generated handheld selector keeps its authored Fredoka dependency in the serialized scene asset.
        /// </summary>
        [Fact]
        public void Tilt_trial_handheld_selector_scene_asset_keeps_fredoka_dependency() {
            string sceneAssetPath = global::city.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial_ds.helen");
            using FileStream stream = File.OpenRead(sceneAssetPath);
            SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.AssetSerializer.Deserialize(stream));

            Assert.Contains(
                sceneAsset.AssetReferences,
                reference => reference.SourceKind == SceneAssetReferenceSourceKind.FileSystem &&
                    string.Equals(reference.RelativePath, "fonts/Fredoka.ttf", StringComparison.Ordinal));
        }
    }
}
