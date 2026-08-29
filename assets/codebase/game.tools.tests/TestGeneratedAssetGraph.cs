using helengine;
using helengine.editor;
using helengine.ui;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

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

            EnsureAssimpNativeLibrary();
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

        static void EnsureAssimpNativeLibrary() {
            if (!OperatingSystem.IsWindows()) {
                return;
            }

            string runtimeIdentifier = ResolveAssimpRuntimeIdentifier();
            string packageRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            List<string> packageRoots = new List<string>();
            if (!string.IsNullOrWhiteSpace(packageRoot)) {
                packageRoots.Add(packageRoot);
            }

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userProfile)) {
                packageRoots.Add(Path.Combine(userProfile, ".nuget", "packages"));
            }

            foreach (string environmentVariable in new[] { "USERPROFILE", "HOME" }) {
                string environmentProfile = Environment.GetEnvironmentVariable(environmentVariable);
                if (!string.IsNullOrWhiteSpace(environmentProfile)) {
                    packageRoots.Add(Path.Combine(environmentProfile, ".nuget", "packages"));
                }
            }

            string nativeSourcePath = null;
            foreach (string root in packageRoots.Distinct(StringComparer.OrdinalIgnoreCase)) {
                string packagePath = Path.Combine(root, "assimpnetter");
                if (!Directory.Exists(packagePath)) {
                    continue;
                }

                nativeSourcePath = Directory.EnumerateFiles(packagePath, "assimp.dll", SearchOption.AllDirectories)
                    .Where(path => path.Replace('\\', '/').Contains("/runtimes/" + runtimeIdentifier + "/native/", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(nativeSourcePath)) {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(nativeSourcePath)) {
                throw new InvalidOperationException(
                    "The deterministic generated-asset test graph could not locate the AssimpNetter native importer payload in: "
                    + string.Join(";", packageRoots.Distinct(StringComparer.OrdinalIgnoreCase)));
            }

            string nativeDestinationPath = Path.Combine(AppContext.BaseDirectory, "assimp.dll");
            PublishAssimpNativeLibraryAtomically(nativeSourcePath, nativeDestinationPath, runtimeIdentifier);
        }

        static string ResolveAssimpRuntimeIdentifier() {
            switch (RuntimeInformation.ProcessArchitecture) {
                case Architecture.X64:
                    return "win-x64";
                case Architecture.X86:
                    return "win-x86";
                case Architecture.Arm64:
                    return "win-arm64";
                default:
                    throw new PlatformNotSupportedException(
                        "The deterministic generated-asset test graph does not have an AssimpNetter native payload mapping for process architecture '"
                        + RuntimeInformation.ProcessArchitecture
                        + "'.");
            }
        }

        static void PublishAssimpNativeLibraryAtomically(
            string nativeSourcePath,
            string nativeDestinationPath,
            string runtimeIdentifier) {
            string destinationDirectory = Path.GetDirectoryName(nativeDestinationPath);
            if (string.IsNullOrWhiteSpace(destinationDirectory)) {
                throw new InvalidOperationException(
                    "The deterministic generated-asset test graph could not determine the Assimp destination directory.");
            }

            Directory.CreateDirectory(destinationDirectory);
            if (File.Exists(nativeDestinationPath)) {
                ValidateAssimpNativeLibrary(nativeSourcePath, nativeDestinationPath, runtimeIdentifier);
                return;
            }

            string temporaryPath = Path.Combine(
                destinationDirectory,
                "." + Path.GetFileName(nativeDestinationPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
            try {
                File.Copy(nativeSourcePath, temporaryPath, false);
                try {
                    File.Move(temporaryPath, nativeDestinationPath);
                } catch (IOException) when (File.Exists(nativeDestinationPath)) {
                    // Another test process won the no-replace move. Its
                    // atomically published payload is validated below.
                }

                ValidateAssimpNativeLibrary(nativeSourcePath, nativeDestinationPath, runtimeIdentifier);
            } finally {
                if (File.Exists(temporaryPath)) {
                    File.Delete(temporaryPath);
                }
            }
        }

        static void ValidateAssimpNativeLibrary(
            string nativeSourcePath,
            string nativeDestinationPath,
            string runtimeIdentifier) {
            FileInfo sourceInfo = new FileInfo(nativeSourcePath);
            FileInfo destinationInfo = new FileInfo(nativeDestinationPath);
            if (!destinationInfo.Exists
                || destinationInfo.Length == 0
                || destinationInfo.Length != sourceInfo.Length) {
                throw new InvalidOperationException(
                    "The AssimpNetter payload published for '"
                    + runtimeIdentifier
                    + "' is incomplete or has an unexpected length.");
            }

            byte[] sourceHash;
            byte[] destinationHash;
            using (FileStream sourceStream = File.OpenRead(nativeSourcePath)) {
                sourceHash = SHA256.HashData(sourceStream);
            }
            using (FileStream destinationStream = File.OpenRead(nativeDestinationPath)) {
                destinationHash = SHA256.HashData(destinationStream);
            }

            if (!CryptographicOperations.FixedTimeEquals(sourceHash, destinationHash)) {
                throw new InvalidOperationException(
                    "The AssimpNetter payload published for '"
                    + runtimeIdentifier
                    + "' does not match the expected native architecture payload.");
            }
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
