namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies the authored source contract for the generated Helen of Code splash scene.
    /// </summary>
    public sealed class HelenOfCodeSplashSceneSourceTests {
        /// <summary>
        /// Proves the splash factory authors the required scene, sprites, timing component, and logo asset reference.
        /// </summary>
        [Fact]
        public void Splash_factory_authors_centered_ninety_percent_logo_scene() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu.tools",
                "HelenOfCodeSplashSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("HelenOfCodeSplash", source, StringComparison.Ordinal);
            Assert.Contains("HelenOfCodeSplashComponent", source, StringComparison.Ordinal);
            Assert.Contains("images/splash/helen_of_code_logo.png", source, StringComparison.Ordinal);
            Assert.Contains("DemoMenuLayout.CanvasHeight * 0.9d", source, StringComparison.Ordinal);
            Assert.Contains("new byte4(0, 0, 0, 255)", source, StringComparison.Ordinal);
            Assert.Contains("SceneEntityIdAllocator.Allocate()", source, StringComparison.Ordinal);
            Assert.Contains("BackgroundSpriteEntityReference", source, StringComparison.Ordinal);
            Assert.Contains("LogoSpriteEntityReference", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the opaque splash background uses the menu's solid rounded-rectangle path and does not add an importable texture dependency.
        /// </summary>
        [Fact]
        public void Splash_factory_authors_an_opaque_black_solid_background() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu.tools",
                "HelenOfCodeSplashSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("new RoundedRectComponent {", source, StringComparison.Ordinal);
            Assert.Contains("Radius = 0f", source, StringComparison.Ordinal);
            Assert.Contains("BorderThickness = 0f", source, StringComparison.Ordinal);
            Assert.DoesNotContain("BackgroundTexturePath", source, StringComparison.Ordinal);
            Assert.DoesNotContain("black.ppm", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the splash viewport and sprites are owned by the overlay camera that renders after the menu camera.
        /// </summary>
        [Fact]
        public void Splash_factory_nests_the_sprite_subtree_under_its_overlay_camera() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu.tools",
                "HelenOfCodeSplashSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("Entity cameraEntity = CreateCameraEntity();", source, StringComparison.Ordinal);
            Assert.Contains("CreateSplashRootEntity(cameraEntity, backgroundEntity)", source, StringComparison.Ordinal);
            Assert.Contains("Entity CreateSplashRootEntity(Entity parent, Entity backgroundEntity)", source, StringComparison.Ordinal);
            Assert.Contains("Core.Instance.EntityFactory.CreateChild(parent, SceneId)", source, StringComparison.Ordinal);
            Assert.Contains("BindingMode = ViewportComponent.AncestorCameraBindingMode", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the splash blackout rectangle is owned by the camera and resized from the live viewport rather than letterboxed with the reference-canvas content.
        /// </summary>
        [Fact]
        public void Splash_factory_keeps_its_blackout_background_outside_the_fitted_canvas() {
            string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\HelenOfCodeSplashSceneFactory.cs");
            string componentSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu\HelenOfCodeSplashComponent.cs");

            Assert.Contains("Entity backgroundEntity = CreateBackgroundEntity(cameraEntity);", factorySource, StringComparison.Ordinal);
            Assert.Contains("BackgroundRectangle.Size = Core.Instance.RenderManager3D.MainWindowSize", componentSource, StringComparison.Ordinal);
            Assert.Contains("new CameraClearSettings(\n                    true,", factorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures every splash camera and sprite entity occupies the dedicated runtime layer culled by the overlay camera rather than the menu camera.
        /// </summary>
        [Fact]
        public void Splash_factory_assigns_the_dedicated_runtime_layer_to_every_splash_entity() {
            string sourcePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "codebase",
                "menu.tools",
                "HelenOfCodeSplashSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Equal(4, CountOccurrences(source, "entity.LayerMask = SplashRuntimeLayerMask;"));
        }

        /// <summary>
        /// Ensures the latest packaged Windows splash scene retains the dedicated overlay layer on the camera-owned sprite subtree.
        /// </summary>
        [Fact]
        public void Packaged_windows_splash_scene_preserves_overlay_layer_on_the_sprite_subtree() {
            string authoredScenePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "assets",
                "scenes",
                "HelenOfCodeSplash.helen");
            string packagedScenePath = Path.Combine(
                @"C:\dev\helprojs\demodisc",
                "output",
                "windows",
                "cooked",
                "scenes",
                "helenofcodesplash.hasset");
            using FileStream authoredStream = File.OpenRead(authoredScenePath);
            SceneAsset authoredScene = Assert.IsType<SceneAsset>(global::helengine.AssetSerializer.Deserialize(authoredStream));
            using FileStream stream = File.OpenRead(packagedScenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.AssetSerializer.Deserialize(stream));

            SceneEntityAsset authoredCameraEntity = Assert.Single(authoredScene.RootEntities);
            Assert.Equal(2, authoredCameraEntity.Children.Length);
            SceneEntityAsset authoredBackgroundEntity = Assert.Single(authoredCameraEntity.Children.Where(entity => entity.Components.Any(component => component.ComponentTypeId == "helengine.RoundedRectComponent")));
            SceneEntityAsset authoredSplashRootEntity = Assert.Single(authoredCameraEntity.Children.Where(entity => entity.Children.Length == 1));
            SceneEntityAsset cameraEntity = Assert.Single(scene.RootEntities);
            SceneEntityAsset splashRootEntity = Assert.Single(cameraEntity.Children);
            SceneEntityAsset[] spriteEntities = splashRootEntity.Children;

            Assert.Equal((ushort)2, authoredCameraEntity.LayerMask);
            Assert.Equal((ushort)2, authoredBackgroundEntity.LayerMask);
            Assert.Equal((ushort)2, authoredSplashRootEntity.LayerMask);
            Assert.Equal((ushort)2, cameraEntity.LayerMask);
            Assert.Equal((ushort)2, splashRootEntity.LayerMask);
            Assert.Equal(2, spriteEntities.Length);
            Assert.All(spriteEntities, entity => Assert.Equal((ushort)2, entity.LayerMask));
        }

        /// <summary>
        /// Counts non-overlapping occurrences of one source fragment.
        /// </summary>
        /// <param name="source">Source text to search.</param>
        /// <param name="value">Exact fragment to count.</param>
        /// <returns>Number of non-overlapping matching fragments.</returns>
        int CountOccurrences(string source, string value) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            } else if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException("Source fragment must be provided.", nameof(value));
            }

            int count = 0;
            int startIndex = 0;
            while (true) {
                int matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
                if (matchIndex < 0) {
                    return count;
                }

                count++;
                startIndex = matchIndex + value.Length;
            }
        }
    }
}
