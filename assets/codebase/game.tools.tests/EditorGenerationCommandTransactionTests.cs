using city.game.tools;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Exercises a production editor generation command at its public command
    /// boundary, including rollback after generated destinations have been
    /// published before the native graph rejects the batch.
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
        public void Production_command_failure_after_partial_publication_restores_every_generated_destination() {
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

            Assert.ThrowsAny<Exception>(() => new GenerateTiltTrialPendulumHammerCommand().Execute(context));

            string[] generatedPaths = GetGeneratedPaths();
            Assert.All(generatedPaths, path => Assert.False(File.Exists(path), path));
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

    }
}
