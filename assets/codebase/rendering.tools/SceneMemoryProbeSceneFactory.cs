using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds one persistent authored scene that retains metadata for the retired scene-memory probe.
    /// </summary>
    public sealed class SceneMemoryProbeSceneFactory {
        readonly IEditorProjectAuthoringSession AuthoringSession;
        /// <summary>
        /// Stable scene id used by the generated scene-memory probe asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.SceneMemoryProbeSceneId;

        /// <summary>
        /// Stable demo-disc main-menu scene id used by the authored probe loop.
        /// </summary>
        const string DemoDiscMainMenuSceneId = "DemoDiscMainMenu";

        /// <summary>
        /// Stable runtime scene id used by the authored cube-test rendering step.
        /// </summary>
        const string CubeTestRuntimeSceneId = "cube_test";

        /// <summary>
        /// Stable probe name written to emitted runtime checkpoint log lines.
        /// </summary>
        const string ProbeName = "menu-cube-memory-probe";

        /// <summary>
        /// Number of full menu-to-cube-to-menu round trips executed by the authored probe.
        /// </summary>
        const int RoundTripCount = 20;

        /// <summary>
        /// Stable idle duration applied to each probe scene after it is loaded.
        /// </summary>
        const double IdleDurationSeconds = 10.0d;

        /// <summary>
        /// Initializes one scene-memory probe scene factory.
        /// </summary>
        public SceneMemoryProbeSceneFactory(IEditorProjectAuthoringSession authoringSession) {
            AuthoringSession = authoringSession ?? throw new ArgumentNullException(nameof(authoringSession));
        }

        /// <summary>
        /// Creates the authored persistent probe scene definition.
        /// </summary>
        /// <returns>Live-authored scene definition that keeps the probe loaded across single-scene transitions.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset {
                    DontUnload = true
                },
                RootEntities = new[] {
                    CreateProbeRootEntity()
                }
            };
        }

        /// <summary>
        /// Creates the authored root entity retained for scene-generation metadata.
        /// </summary>
        /// <returns>Live-authored root entity retained for scene-generation metadata.</returns>
        Entity CreateProbeRootEntity() {
            Entity entity = AuthoringSession.OwningCore.EntityFactory.Create("SceneMemoryProbeRoot");
            return entity;
        }

        /// <summary>
        /// Builds the fixed-step probe sequence that alternates between the main menu and cube-test scene for a long-running round-trip soak.
        /// </summary>
        /// <returns>Authored step array consumed by the runtime probe component.</returns>
        SceneMemoryProbeStep[] BuildProbeSteps() {
            List<SceneMemoryProbeStep> steps = new List<SceneMemoryProbeStep>();
            for (int roundTripIndex = 1; roundTripIndex <= RoundTripCount; roundTripIndex++) {
                steps.Add(CreateLoadStep(DemoDiscMainMenuSceneId, "menu", roundTripIndex));
                steps.Add(CreateWaitStep("menu", roundTripIndex));
                steps.Add(CreateLoadStep(CubeTestRuntimeSceneId, "cube", roundTripIndex));
                steps.Add(CreateWaitStep("cube", roundTripIndex));
            }

            steps.Add(CreateLoadStep(DemoDiscMainMenuSceneId, "menu", RoundTripCount + 1));
            steps.Add(CreateWaitStep("menu", RoundTripCount + 1));
            return steps.ToArray();
        }

        /// <summary>
        /// Creates one scene-load probe step with a stable numbered label.
        /// </summary>
        /// <param name="sceneId">Runtime scene id to load.</param>
        /// <param name="labelPrefix">Stable label prefix for the step.</param>
        /// <param name="ordinal">One-based ordinal appended to the label.</param>
        /// <returns>Authored scene-load step.</returns>
        SceneMemoryProbeStep CreateLoadStep(string sceneId, string labelPrefix, int ordinal) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (string.IsNullOrWhiteSpace(labelPrefix)) {
                throw new ArgumentException("Label prefix must be provided.", nameof(labelPrefix));
            } else if (ordinal <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ordinal), "Probe step ordinal must be greater than zero.");
            }

            return new SceneMemoryProbeStep {
                ActionKind = SceneMemoryProbeActionKind.LoadSceneSingle,
                SceneId = sceneId,
                DurationSeconds = 0d,
                Label = "load-" + labelPrefix + "-" + ordinal
            };
        }

        /// <summary>
        /// Creates one idle probe step with a stable numbered label.
        /// </summary>
        /// <param name="labelPrefix">Stable scene label prefix for the wait step.</param>
        /// <param name="ordinal">One-based ordinal appended to the label.</param>
        /// <returns>Authored wait step.</returns>
        SceneMemoryProbeStep CreateWaitStep(string labelPrefix, int ordinal) {
            if (string.IsNullOrWhiteSpace(labelPrefix)) {
                throw new ArgumentException("Label prefix must be provided.", nameof(labelPrefix));
            } else if (ordinal <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ordinal), "Probe step ordinal must be greater than zero.");
            }

            return new SceneMemoryProbeStep {
                ActionKind = SceneMemoryProbeActionKind.Wait,
                SceneId = string.Empty,
                DurationSeconds = IdleDurationSeconds,
                Label = "idle-" + labelPrefix + "-" + ordinal
            };
        }
    }
}
