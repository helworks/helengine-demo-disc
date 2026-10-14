using helengine;
using helengine.editor;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Lays out the persisted main-menu title through the real runtime viewport stretch, right-anchor and text-alignment
    /// math, and checks that the native-size title glyphs stay inside the canvas at narrow console resolutions.
    /// </summary>
    public sealed class DemoDiscMainMenuTitleLayoutTests {
        /// <summary>
        /// Width of the synthetic font atlas used to express glyph widths as normalized source rectangles.
        /// </summary>
        const int AtlasWidth = 1024;

        /// <summary>
        /// Conservative average glyph advance of the cooked Trebuchet title font, as a fraction of its pixel size.
        /// GDI measures "HELENGINE" at 0.52 em and "DEMO DISC" at 0.50 em, so 0.6 em overestimates both lines.
        /// </summary>
        const double TitleGlyphAdvanceEm = 0.6d;

        static string ProjectRootPath => global::DemoDisc.testing.DemoDiscTestProject.RootPath;

        /// <summary>
        /// Checks every title line's right edge, including its outline, against the live canvas width.
        /// </summary>
        /// <param name="canvasWidth">Live screen width the stretch viewport fills.</param>
        /// <param name="canvasHeight">Live screen height the stretch viewport fills.</param>
        /// <param name="fontPixelSize">Cooked title font pixel size that renders at native size.</param>
        [Theory]
        [InlineData(640, 448, 34)]
        [InlineData(640, 448, 69)]
        [InlineData(1280, 720, 69)]
        public void Title_right_edge_stays_inside_the_canvas(int canvasWidth, int canvasHeight, int fontPixelSize) {
            SceneEntityAsset menuRootAsset;
            SceneEntityAsset overlayAsset;
            LoadTitleAssets(out menuRootAsset, out overlayAsset);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();

            using RenderingTestRenderManager3D meshes = new RenderingTestRenderManager3D();
            using RenderingTestRenderManager2D overlay = new RenderingTestRenderManager2D();
            using Core core = new Core(new CoreInitializationOptions {
                ContentStreamSource = new HostFileSystemContentStreamSource(AppContext.BaseDirectory)
            });
            core.Initialize(meshes, overlay, null, new PlatformInfo("test", "test-version"));
            meshes.AddWindow(IntPtr.Zero, canvasWidth, canvasHeight);
            FontAsset font = CreateTitleFont(fontPixelSize);

            Entity menuRoot = CreateEntity(core, menuRootAsset.LocalPosition);
            Entity generatedRoot = CreateEntity(core, float3.Zero);
            menuRoot.AddChild(generatedRoot);
            Entity overlayEntity = CreateEntity(core, overlayAsset.LocalPosition);
            generatedRoot.AddChild(overlayEntity);

            List<TextComponent> titleTexts = new List<TextComponent>();
            foreach (SceneEntityAsset titleAsset in overlayAsset.Children ?? Array.Empty<SceneEntityAsset>()) {
                Entity titleEntity = CreateEntity(core, titleAsset.LocalPosition);
                overlayEntity.AddChild(titleEntity);
                TextComponent text = DeserializeSingle<TextComponent>(registry, titleAsset, "helengine.TextComponent");
                text.Font = font;
                titleEntity.AddComponent(text);
                titleTexts.Add(text);
            }
            Assert.Equal(2, titleTexts.Count);

            overlayEntity.AddComponent(DeserializeSingle<LayoutComponent>(registry, overlayAsset, "helengine.LayoutComponent"));
            ViewportComponent viewport = DeserializeSingle<ViewportComponent>(registry, menuRootAsset, "helengine.ViewportComponent");
            menuRoot.AddComponent(viewport);
            viewport.Update();

            foreach (TextComponent text in titleTexts) {
                Assert.NotEqual(TextNativePixelMode.Disabled, text.NativeFontPixels);
                Assert.Equal(1f, text.FontScale);
                double visibleWidth = TextLayoutAlignmentUtils.MeasureVisibleLineWidth(text.Text, font, text.FontScale, AtlasWidth);
                double left = text.Parent.Position.X + TextLayoutAlignmentUtils.ResolveHorizontalOffset(text.Alignment, text.Size.X, visibleWidth);
                double outline = Math.Ceiling(text.OutlineScale);
                double right = left + visibleWidth + outline;
                Assert.True(right <= canvasWidth, $"'{text.Text}' ends at x={right:0.0} on a {canvasWidth}px-wide canvas.");
                Assert.True(left - outline >= 0d, $"'{text.Text}' starts at x={left - outline:0.0}.");
            }
        }

        /// <summary>
        /// Loads the persisted menu root and the title overlay subtree from the generated main-menu scene.
        /// </summary>
        /// <param name="menuRootAsset">Persisted menu root carrying the stretch viewport.</param>
        /// <param name="overlayAsset">Persisted right-anchored title overlay.</param>
        static void LoadTitleAssets(out SceneEntityAsset menuRootAsset, out SceneEntityAsset overlayAsset) {
            string scenePath = Path.Combine(ProjectRootPath, @"assets\scenes\DemoDiscMainMenu.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            menuRootAsset = Assert.Single(scene.RootEntities, entity => entity.Name == "DemoDiscMenuRoot");
            SceneEntityAsset generatedRoot = Assert.Single(menuRootAsset.Children, entity => entity.Name == DemoMenuLayout.GeneratedRootEntityName);
            Assert.Equal(float3.Zero, generatedRoot.LocalPosition);
            overlayAsset = Assert.Single(generatedRoot.Children, entity => entity.Name == "DemoDiscPlatformInfoOverlay");
        }

        /// <summary>
        /// Deserializes the single persisted component of one type from a scene entity.
        /// </summary>
        /// <typeparam name="TComponent">Expected live component type.</typeparam>
        /// <param name="registry">Generated scene persistence registry.</param>
        /// <param name="entityAsset">Persisted entity that owns the component.</param>
        /// <param name="componentTypeId">Persisted component type id.</param>
        /// <returns>Live component restored from the persisted record.</returns>
        static TComponent DeserializeSingle<TComponent>(ComponentPersistenceRegistry registry, SceneEntityAsset entityAsset, string componentTypeId) where TComponent : Component {
            SceneComponentAssetRecord record = Assert.Single(
                entityAsset.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == componentTypeId);
            return Assert.IsType<TComponent>(registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(record, new EntitySaveComponent(), null));
        }

        /// <summary>
        /// Creates one initialized runtime entity at the persisted local position.
        /// </summary>
        /// <param name="core">Runtime core that owns the entity.</param>
        /// <param name="localPosition">Persisted local position.</param>
        /// <returns>Initialized entity.</returns>
        static Entity CreateEntity(Core core, float3 localPosition) {
            Entity entity = new Entity(core) { LocalPosition = localPosition };
            entity.InitComponents();
            entity.InitChildren();
            return entity;
        }

        /// <summary>
        /// Builds a font whose title glyphs all advance and cover <see cref="TitleGlyphAdvanceEm"/> of the cooked pixel size.
        /// </summary>
        /// <param name="pixelSize">Cooked font pixel size.</param>
        /// <returns>Font asset with glyphs for the title letters and space.</returns>
        static FontAsset CreateTitleFont(int pixelSize) {
            float advance = (float)(pixelSize * TitleGlyphAdvanceEm);
            Dictionary<char, FontChar> characters = new Dictionary<char, FontChar>();
            foreach (char character in "ABCDEFGHIJKLMNOPQRSTUVWXYZ") {
                characters[character] = new FontChar(new float4(0f, 0f, advance / AtlasWidth, 0.05f), 0f, advance, 0f, 0f);
            }

            return new FontAsset(
                new FontInfo("TitleLayoutProbe", pixelSize, advance),
                new RenderingTestRuntimeTexture { Width = AtlasWidth, Height = AtlasWidth },
                characters,
                pixelSize,
                AtlasWidth,
                AtlasWidth);
        }
    }
}
