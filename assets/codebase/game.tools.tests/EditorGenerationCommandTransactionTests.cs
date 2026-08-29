using city.game.tools;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Exercises a production editor generation command at its public command
    /// boundary, including disposal rollback when generation fails before the
    /// command reaches commit.
    /// </summary>
    public sealed class EditorGenerationCommandTransactionTests : IDisposable {
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
        public void Command_failure_after_real_generator_stages_nothing_in_the_project() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(ProjectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(ProjectRootPath);
            EditorCommandContext context = new EditorCommandContext(
                ProjectRootPath,
                new ScriptTypeResolver(),
                authoringSession,
                graph.OwnerCore,
                graph.InteractionServices,
                graph.Registry,
                graph.RendererResources);

            Assert.Throws<InvalidOperationException>(() => new FailingAfterPendulumGenerationCommand().Execute(context));

            Assert.All(GetGeneratedPaths(), path => Assert.False(File.Exists(path), path));
            string transactionRoot = Path.Combine(ProjectRootPath, "cache", "editor", "authoring-transactions");
            Assert.True(!Directory.Exists(transactionRoot) || !Directory.EnumerateFileSystemEntries(transactionRoot).Any());
        }

        string[] GetGeneratedPaths() {
            return new[] {
                Path.Combine(ProjectRootPath, "assets", "models", "games", "tilt", "pendulum_hammer.hasset"),
                Path.Combine(ProjectRootPath, "assets", "models", "games", "tilt", "pendulum_hammer_ds.hasset"),
                Path.Combine(ProjectRootPath, "assets", "materials", "games", "tilt", "PendulumHammerHandle.hasset"),
                Path.Combine(ProjectRootPath, "assets", "materials", "games", "tilt", "PendulumHammerHead.hasset"),
                Path.Combine(ProjectRootPath, "assets", "blueprints", "games", "tilt", "PendulumHammer.hblueprint")
            };
        }

        sealed class FailingAfterPendulumGenerationCommand : IEditorCommand {
            public string CommandId => "test.fail-after-pendulum-generation";

            public string DisplayName => "Fail after pendulum generation";

            public void Execute(IEditorCommandContext context) {
                using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
                new TiltTrialPendulumHammerAssetGenerator(context.Authoring, transaction).Generate(context.ProjectRootPath);
                throw new InvalidOperationException("Deterministic command failure after generated outputs were staged.");
            }
        }
    }
}
