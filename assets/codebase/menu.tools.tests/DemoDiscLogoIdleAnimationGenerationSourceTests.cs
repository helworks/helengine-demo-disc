using System.Runtime.CompilerServices;
using Xunit;

namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies that the demo-disc logo animation is authored through the current public asset writer.
    /// </summary>
    public sealed class DemoDiscLogoIdleAnimationGenerationSourceTests {
        /// <summary>
        /// Ensures main-menu regeneration publishes the generated animation before opening the scene transaction.
        /// </summary>
        [Fact]
        public void Main_menu_generation_authors_logo_animation_through_current_writer() {
            string checkoutRoot = FindCheckoutRoot();
            string commandSource = File.ReadAllText(Path.Combine(checkoutRoot, "assets", "codebase", "menu.tools", "RegenerateDemoDiscMainMenuCommand.cs"));
            string generatorSource = File.ReadAllText(Path.Combine(checkoutRoot, "assets", "codebase", "menu.tools", "DemoDiscLogoIdleAnimationGenerator.cs"));

            Assert.Contains("DemoDiscLogoIdleAnimationGenerator", commandSource, StringComparison.Ordinal);
            Assert.Contains("using (helengine.editor.EditorAuthoringTransaction animationTransaction = context.Authoring.BeginTransaction())", commandSource, StringComparison.Ordinal);
            Assert.Contains("new DemoDiscLogoIdleAnimationGenerator(context.Authoring, animationTransaction).Generate();", commandSource, StringComparison.Ordinal);
            Assert.Contains("animationTransaction.Commit();", commandSource, StringComparison.Ordinal);
            Assert.Contains("using (helengine.editor.EditorAuthoringTransaction sceneTransaction = context.Authoring.BeginTransaction())", commandSource, StringComparison.Ordinal);
            Assert.Contains("new DemoDiscSceneGenerator(context.ScriptTypeResolver, context.Authoring, sceneTransaction)", commandSource, StringComparison.Ordinal);
            Assert.Contains("sceneTransaction.Commit();", commandSource, StringComparison.Ordinal);
            Assert.DoesNotContain("using helengine.editor.EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();", commandSource, StringComparison.Ordinal);

            int animationTransactionIndex = commandSource.IndexOf("animationTransaction", StringComparison.Ordinal);
            int animationGenerationIndex = commandSource.IndexOf("new DemoDiscLogoIdleAnimationGenerator(context.Authoring, animationTransaction).Generate();", StringComparison.Ordinal);
            int animationCommitIndex = commandSource.IndexOf("animationTransaction.Commit();", StringComparison.Ordinal);
            int sceneTransactionIndex = commandSource.IndexOf("sceneTransaction", animationCommitIndex, StringComparison.Ordinal);
            int sceneGenerationIndex = commandSource.IndexOf("new DemoDiscSceneGenerator(context.ScriptTypeResolver, context.Authoring, sceneTransaction)", sceneTransactionIndex, StringComparison.Ordinal);
            int sceneCommitIndex = commandSource.IndexOf("sceneTransaction.Commit();", sceneGenerationIndex, StringComparison.Ordinal);

            Assert.True(animationTransactionIndex >= 0, "The animation transaction scope must be named and present.");
            Assert.True(animationGenerationIndex > animationTransactionIndex, "The animation must be generated inside the animation transaction.");
            Assert.True(animationCommitIndex > animationGenerationIndex, "The animation transaction must commit after generation.");
            Assert.True(sceneTransactionIndex > animationCommitIndex, "The scene transaction must begin after the animation transaction commits.");
            Assert.True(sceneGenerationIndex > sceneTransactionIndex, "The scenes must be generated inside the scene transaction.");
            Assert.True(sceneCommitIndex > sceneGenerationIndex, "The scene transaction must commit after scene generation.");

            Assert.Contains("IEditorProjectAuthoringSession", generatorSource, StringComparison.Ordinal);
            Assert.Contains("WriteAsset", generatorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("GeneratedAssetWriteService", generatorSource, StringComparison.Ordinal);
            Assert.Contains("RotationTracks", generatorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("EditorAssetBinarySerializer", generatorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("AssetSerializer", generatorSource, StringComparison.Ordinal);
        }

        static string FindCheckoutRoot([CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (directory != null) {
                if (Directory.Exists(Path.Combine(directory.FullName, "assets"))
                    && File.Exists(Path.Combine(directory.FullName, "project.heproj"))) {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Unable to locate the active Demo Disc checkout root from the test source path.");
        }
    }
}
