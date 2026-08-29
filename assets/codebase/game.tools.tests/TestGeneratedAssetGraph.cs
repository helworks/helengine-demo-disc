using helengine;
using helengine.editor;
using helengine.ui;

namespace city.tests {
    /// <summary>
    /// Composes one complete public editor authoring graph for generation
    /// behavior tests.  All generators in a test share this graph and the
    /// transaction returned by its authoring session.
    /// </summary>
    public sealed class TestGeneratedAssetGraph : IDisposable {
        readonly EditorCore CoreValue;
        readonly EditorCoreInteractionGraphBinding InteractionBinding;
        readonly List<IEditorProjectAuthoringSession> AuthoringSessions = new List<IEditorProjectAuthoringSession>();

        public EditorSessionInteractionServices InteractionServices { get; }
        public GeneratedAssetProviderRegistry Registry { get; }
        public EngineGeneratedModelCache ModelCache { get; }
        public EngineGeneratedMaterialCache MaterialCache { get; }
        public EditorSessionRendererResources RendererResources { get; }
        public EditorCore OwnerCore => CoreValue;

        public TestGeneratedAssetGraph(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string root = Path.GetFullPath(projectRootPath);
            Directory.CreateDirectory(Path.Combine(root, "assets"));
            CoreValue = new EditorCore(new Project { Name = "demodisc-test", Path = root });
            GeneratedTestRenderManager3D render3D = new GeneratedTestRenderManager3D();
            GeneratedTestRenderManager2D render2D = new GeneratedTestRenderManager2D();
            CoreValue.Initialize(render3D, render2D, null, new PlatformInfo("test", "test-version"));

            InteractionServices = new EditorSessionInteractionServices();
            InteractionBinding = new EditorCoreInteractionGraphBinding(CoreValue, InteractionServices);
            RendererResources = new EditorSessionRendererResources(
                CoreValue.RenderManager3D,
                CoreValue.RenderManager2D,
                CoreValue.ObjectManager,
                CoreValue.EntityFactory,
                CoreValue.SceneEntityIdAllocator,
                CoreValue.Input,
                () => CoreValue.FrameDeltaSeconds,
                null,
                InteractionServices);
            Registry = new GeneratedAssetProviderRegistry();
            ShaderBackendRegistry shaderBackends = new ShaderBackendRegistry();
            EditorBuiltInShaderAssetLibrary shaderLibrary = new EditorBuiltInShaderAssetLibrary(shaderBackends);
            MaterialCache = new EngineGeneratedMaterialCache(CoreValue, shaderLibrary);
            ModelCache = new EngineGeneratedModelCache(CoreValue);
            ShaderLibrary = shaderLibrary;
        }

        public EditorBuiltInShaderAssetLibrary ShaderLibrary { get; }

        public IEditorProjectAuthoringSession CreateAuthoringSession(string projectRootPath) {
            string root = Path.GetFullPath(projectRootPath);
            Directory.CreateDirectory(Path.Combine(root, "assets"));
            ContentManager contentManager = new ContentManager(
                new HostFileSystemContentStreamSource(Path.Combine(root, "assets")));
            try {
                IEditorProjectAuthoringSession session = new EditorProjectAuthoringSession(
                    root,
                    Array.Empty<IAssetImporterRegistration>(),
                    contentManager,
                    Registry,
                    ModelCache,
                    MaterialCache,
                    RendererResources);
                AuthoringSessions.Add(session);
                return session;
            } catch {
                contentManager.Dispose();
                throw;
            }
        }

        public void Dispose() {
            List<Exception> failures = new List<Exception>();
            for (int index = AuthoringSessions.Count - 1; index >= 0; index--) {
                DisposeOne(AuthoringSessions[index], failures);
            }
            AuthoringSessions.Clear();
            DisposeOne(Registry, failures);
            DisposeOne(RendererResources, failures);
            DisposeOne(MaterialCache, failures);
            DisposeOne(ModelCache, failures);
            DisposeOne(ShaderLibrary, failures);
            DisposeOne(InteractionBinding, failures);
            DisposeOne(CoreValue, failures);
            if (failures.Count != 0) {
                throw failures.Count == 1
                    ? failures[0]
                    : new AggregateException("Generated test graph disposal failed.", failures);
            }
        }

        static void DisposeOne(IDisposable disposable, List<Exception> failures) {
            try {
                disposable?.Dispose();
            } catch (Exception exception) {
                failures.Add(exception);
            }
        }
    }

    sealed class GeneratedTestRenderManager3D : RenderManager3D {
        public override RuntimeModel BuildModelFromRaw(ModelAsset data) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            GeneratedTestRuntimeModel model = new GeneratedTestRuntimeModel();
            model.SetBounds(data.BoundsMin, data.BoundsMax);
            return model;
        }
    }

    sealed class GeneratedTestRuntimeModel : RuntimeModel { }

    sealed class GeneratedTestRenderManager2D : RenderManager2D {
        public override RuntimeTexture BuildTextureFromRaw(TextureAsset data) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            return new GeneratedTestRuntimeTexture { Width = data.Width, Height = data.Height };
        }

        public override void DrawSprite(ISpriteDrawable2D sprite) { }
        public override void DrawText(ITextDrawable2D text) { }
        public override void DrawRoundedRect(IRoundedRectDrawable2D shape) { }
    }

    sealed class GeneratedTestRuntimeTexture : RuntimeTexture { }
}
