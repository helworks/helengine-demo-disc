using System.Reflection;
using city.game.tools;
using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Exercises a production editor generation command at its public
    /// boundary, including a deterministic pre-commit staging failure after
    /// all generated outputs have been prepared and a complete no-op rerun.
    /// </summary>
    public sealed class EditorGenerationCommandTransactionTests : IDisposable {
        const string InjectedFailureMessage =
            "Injected deterministic generation failure after all generated model and material outputs staged.";

        readonly string ProjectRootPath;

        public EditorGenerationCommandTransactionTests() {
            ProjectRootPath = Path.Combine(
                Path.GetTempPath(),
                "city-editor-generation-command-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(ProjectRootPath, "assets"));
        }

        public void Dispose() {
            if (Directory.Exists(ProjectRootPath)) {
                Directory.Delete(ProjectRootPath, true);
            }
        }

        [Fact]
        public void Production_command_failure_after_all_outputs_are_staged_leaves_changed_seeded_publication_unchanged() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(ProjectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(ProjectRootPath);

            ExecuteCommand(graph, authoringSession);

            string[] deliberatelyChangedPaths = GetDeliberatelyChangedPaths();
            MutatePublishedSeedState(ProjectRootPath);
            Dictionary<string, byte[]> seededChangedBytes = deliberatelyChangedPaths.ToDictionary(
                path => path,
                path => File.ReadAllBytes(GetAssetPath(ProjectRootPath, path)),
                StringComparer.Ordinal);

            string[] generatedPaths = GetGeneratedPaths();
            GeneratedAssetPublicationSnapshot beforeFault = GeneratedAssetPublicationSnapshot.Capture(ProjectRootPath);
            beforeFault.AssertExactPaths(generatedPaths);
            GeneratedAssetReferenceSnapshot beforeReferences = GeneratedAssetReferenceSnapshot.Capture(
                authoringSession,
                GetGeneratedAssetKinds());

            IEditorProjectAuthoringSession failingAuthoringSession =
                DispatchProxy.Create<IEditorProjectAuthoringSession, FailureInjectingAuthoringSessionProxy>();
            FailureInjectingAuthoringSessionProxy failureProxy =
                (FailureInjectingAuthoringSessionProxy)(object)failingAuthoringSession;
            failureProxy.Inner = authoringSession;

            InjectedGenerationFailureException exception = Assert.Throws<InjectedGenerationFailureException>(
                () => ExecuteCommand(graph, failingAuthoringSession));

            Assert.Equal(InjectedFailureMessage, exception.Message);
            Assert.True(failureProxy.CheckpointReached);
            Assert.Equal(GetGeneratedPaths().Length - 1, failureProxy.StagedOutputCount);
            Assert.True(
                failureProxy.ChangedStagedOutputCount >= 2,
                "Expected at least two changed staged payloads, observed "
                + failureProxy.ChangedStagedOutputCount
                + " ("
                + string.Join(",", failureProxy.ChangedStagedOutputPaths)
                + ").");
            beforeFault.AssertUnchanged();
            beforeReferences.AssertUnchanged(authoringSession, GetGeneratedAssetKinds());
            AssertTransactionRootRetired(ProjectRootPath);

            ExecuteCommand(graph, authoringSession);
            foreach (string path in deliberatelyChangedPaths) {
                Assert.NotEqual(seededChangedBytes[path], File.ReadAllBytes(GetAssetPath(ProjectRootPath, path)));
            }

            GeneratedAssetPublicationSnapshot repairedSnapshot = GeneratedAssetPublicationSnapshot.Capture(ProjectRootPath);
            repairedSnapshot.AssertExactPaths(generatedPaths);
            GeneratedAssetReferenceSnapshot repairedReferences = GeneratedAssetReferenceSnapshot.Capture(
                authoringSession,
                GetGeneratedAssetKinds());
            ExecuteCommand(graph, authoringSession);
            repairedSnapshot.AssertUnchanged();
            repairedReferences.AssertUnchanged(authoringSession, GetGeneratedAssetKinds());
            AssertTransactionRootRetired(ProjectRootPath);
        }

        [Fact]
        public void Production_command_successful_first_and_identical_second_pass_publish_complete_set_without_churn() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(ProjectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(ProjectRootPath);

            ExecuteCommand(graph, authoringSession);

            string[] generatedPaths = GetGeneratedPaths();
            GeneratedAssetPublicationSnapshot firstSnapshot = GeneratedAssetPublicationSnapshot.Capture(ProjectRootPath);
            firstSnapshot.AssertExactPaths(generatedPaths);
            GeneratedAssetReferenceSnapshot firstReferences = GeneratedAssetReferenceSnapshot.Capture(
                authoringSession,
                GetGeneratedAssetKinds());

            ExecuteCommand(graph, authoringSession);

            firstSnapshot.AssertUnchanged();
            firstReferences.AssertUnchanged(authoringSession, GetGeneratedAssetKinds());
            AssertTransactionRootRetired(ProjectRootPath);
        }

        void ExecuteCommand(TestGeneratedAssetGraph graph, IEditorProjectAuthoringSession authoringSession) {
            EditorCommandContext context = new EditorCommandContext(
                ProjectRootPath,
                new ScriptTypeResolver(),
                authoringSession,
                graph.OwnerCore,
                graph.InteractionServices,
                graph.Registry,
                graph.RendererResources);

            new GenerateTiltTrialPendulumHammerCommand().Execute(context);
        }

        static string[] GetGeneratedPaths() {
            return new[] {
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerCommonModelRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerDsModelRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".windows.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".ps2.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".psp.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".gamecube.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".ds.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".windows.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".ps2.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".psp.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".gamecube.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".ds.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerBlueprintRelativePath
            };
        }

        static string[] GetPreBlueprintGeneratedPaths() {
            return new[] {
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerCommonModelRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerDsModelRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".windows.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".ps2.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".psp.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".gamecube.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath + ".ds.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".windows.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".ps2.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".psp.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".gamecube.hasset",
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath + ".ds.hasset"
            };
        }

        static string[] GetDeliberatelyChangedPaths() {
            return new[] {
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerCommonModelRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerDsModelRelativePath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerBlueprintRelativePath
            };
        }

        static void MutatePublishedSeedState(string projectRootPath) {
            string modelPath = GetAssetPath(
                projectRootPath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerCommonModelRelativePath);
            MutatePublishedAsset<ModelAsset>(modelPath, model => {
                model.Positions[0] += new float3(0.125f, 0.25f, 0.375f);
            });

            string dsModelPath = GetAssetPath(
                projectRootPath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerDsModelRelativePath);
            MutatePublishedAsset<ModelAsset>(dsModelPath, model => {
                model.Positions[0] += new float3(0.375f, 0.25f, 0.125f);
            });

            string blueprintPath = GetAssetPath(
                projectRootPath,
                TiltTrialPendulumHammerAssetGenerator.PendulumHammerBlueprintRelativePath);
            MutatePublishedAsset<BlueprintAsset>(blueprintPath, blueprint => {
                blueprint.RootEntity.Name += " (seeded state)";
            });
        }

        static void MutatePublishedAsset<TAsset>(string fullPath, Action<TAsset> mutation)
            where TAsset : Asset {
            TAsset asset;
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(fullPath))) {
                asset = Assert.IsType<TAsset>(global::helengine.editor.EditorAssetBinarySerializer.Deserialize(stream));
            }
            try {
                mutation(asset);
                using (MemoryStream stream = new MemoryStream()) {
                    global::helengine.editor.EditorAssetBinarySerializer.Serialize(stream, asset);
                    File.WriteAllBytes(fullPath, stream.ToArray());
                }
            } finally {
                if (asset is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        }

        static string GetAssetPath(string projectRootPath, string projectRelativePath) {
            return Path.Combine(
                projectRootPath,
                "assets",
                projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        static IReadOnlyDictionary<string, AssetEntryKind> GetGeneratedAssetKinds() {
            return new Dictionary<string, AssetEntryKind>(StringComparer.Ordinal) {
                [TiltTrialPendulumHammerAssetGenerator.PendulumHammerCommonModelRelativePath] = AssetEntryKind.Model,
                [TiltTrialPendulumHammerAssetGenerator.PendulumHammerDsModelRelativePath] = AssetEntryKind.Model,
                [TiltTrialPendulumHammerAssetGenerator.PendulumHammerHandleMaterialRelativePath] = AssetEntryKind.Material,
                [TiltTrialPendulumHammerAssetGenerator.PendulumHammerHeadMaterialRelativePath] = AssetEntryKind.Material,
                [TiltTrialPendulumHammerAssetGenerator.PendulumHammerBlueprintRelativePath] = AssetEntryKind.Blueprint
            };
        }

        static void AssertTransactionRootRetired(string projectRootPath) {
            string transactionRoot = Path.Combine(projectRootPath, "cache", "editor", "authoring-transactions");
            Assert.True(!Directory.Exists(transactionRoot) || !Directory.EnumerateFileSystemEntries(transactionRoot).Any());
        }

        sealed class InjectedGenerationFailureException : InvalidOperationException {
            public InjectedGenerationFailureException(string message)
                : base(message) { }
        }

        class FailureInjectingAuthoringSessionProxy : DispatchProxy {
            public IEditorProjectAuthoringSession Inner { get; set; }
            public bool CheckpointReached { get; private set; }
            public int StagedOutputCount { get; private set; }
            public int ChangedStagedOutputCount { get; private set; }
            public IReadOnlyList<string> ChangedStagedOutputPaths { get; private set; } = Array.Empty<string>();
            EditorAuthoringTransaction ActiveTransaction;

            protected override object Invoke(MethodInfo targetMethod, object[] args) {
                if (targetMethod.Name == nameof(IEditorProjectAuthoringSession.BeginTransaction)) {
                    EditorAuthoringTransaction transaction = (EditorAuthoringTransaction)InvokeInner(targetMethod, args);
                    ActiveTransaction = transaction;
                    return transaction;
                }

                if (targetMethod.Name == nameof(IEditorProjectAssetAuthoringService.CreateFileReference)
                    || targetMethod.Name == nameof(IEditorProjectAuthoringSession.CreateReference)) {
                    int stagedCount = CountStagedOutputs();
                    if (stagedCount != GetPreBlueprintGeneratedPaths().Length) {
                        throw new InvalidOperationException(
                            "The injected generation checkpoint moved before every model and material output was staged.");
                    }

                    CheckpointReached = true;
                    StagedOutputCount = stagedCount;
                    IReadOnlyList<string> changedPaths = GetChangedStagedOutputPaths();
                    ChangedStagedOutputCount = changedPaths.Count;
                    ChangedStagedOutputPaths = changedPaths;
                    throw new InjectedGenerationFailureException(InjectedFailureMessage);
                }

                return InvokeInner(targetMethod, args);
            }

            int CountStagedOutputs() {
                if (ActiveTransaction == null) {
                    return 0;
                }

                int count = 0;
                foreach (string path in GetPreBlueprintGeneratedPaths()) {
                    if (IsStaged(path) || IsStaged("assets/" + path)) {
                        count++;
                    }
                }
                return count;
            }

            IReadOnlyList<string> GetChangedStagedOutputPaths() {
                List<string> changedPaths = new List<string>();
                foreach (string path in GetPreBlueprintGeneratedPaths()) {
                    string destinationPath = GetAssetPath(Inner.ProjectRootPath, path);
                    byte[] stagedBytes = ReadStaged(path);
                    if (!File.ReadAllBytes(destinationPath).SequenceEqual(stagedBytes)) {
                        changedPaths.Add(path);
                    }
                }
                return changedPaths;
            }

            byte[] ReadStaged(string path) {
                try {
                    return ActiveTransaction.ReadStagedFile(path);
                } catch (InvalidOperationException) {
                    return ActiveTransaction.ReadStagedFile("assets/" + path);
                }
            }

            bool IsStaged(string path) {
                try {
                    ActiveTransaction.ReadStagedFile(path);
                    return true;
                } catch (InvalidOperationException) {
                    return false;
                }
            }

            object InvokeInner(MethodInfo targetMethod, object[] args) {
                try {
                    return targetMethod.Invoke(Inner, args);
                } catch (TargetInvocationException exception) when (exception.InnerException != null) {
                    throw exception.InnerException;
                }
            }
        }

    }
}
