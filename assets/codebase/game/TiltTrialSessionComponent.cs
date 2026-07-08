using helengine;

namespace city.game {
    /// <summary>
    /// Owns Tilt Trial timer state, finish/fail transitions, and Retry/Next/Level Select scene actions.
    /// </summary>
    public sealed class TiltTrialSessionComponent : UpdateComponent {
        /// <summary>
        /// Backing state machine used by the active gameplay session.
        /// </summary>
        readonly FiniteStateMachine<TiltTrialSessionState> SessionStateMachine;

        /// <summary>
        /// Initializes one Tilt Trial session controller.
        /// </summary>
        public TiltTrialSessionComponent() {
            SessionStateMachine = CreateStateMachine();
        }

        /// <summary>
        /// Resolves the medal tier awarded for one completed level clear.
        /// </summary>
        /// <param name="settings">Validated level settings.</param>
        /// <param name="clearTimeSeconds">Measured clear time in seconds.</param>
        /// <returns>Awarded medal tier.</returns>
        public static TiltTrialMedal ResolveMedal(TiltTrialLevelSettingsComponent settings, float clearTimeSeconds) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.Validate();
            if (clearTimeSeconds <= settings.GoldTimeSeconds) {
                return TiltTrialMedal.Gold;
            } else if (clearTimeSeconds <= settings.SilverTimeSeconds) {
                return TiltTrialMedal.Silver;
            } else if (clearTimeSeconds <= settings.BronzeTimeSeconds) {
                return TiltTrialMedal.Bronze;
            }

            return TiltTrialMedal.None;
        }

        /// <summary>
        /// Resolves the next gameplay scene for the supplied current level id, or the selector scene when progression is complete.
        /// </summary>
        /// <param name="currentLevelId">Stable current logical level id.</param>
        /// <param name="levelSelectSceneId">Stable selector scene id used as the fallback target.</param>
        /// <returns>Next level scene id or the selector scene id when the current level is last or unknown.</returns>
        public static string ResolveNextSceneId(string currentLevelId, string levelSelectSceneId) {
            if (string.IsNullOrWhiteSpace(levelSelectSceneId)) {
                throw new ArgumentException("Level select scene id must be provided.", nameof(levelSelectSceneId));
            }

            IReadOnlyList<TiltTrialLevelCatalogEntry> entries = TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < entries.Count; index++) {
                if (!string.Equals(entries[index].LevelId, currentLevelId, StringComparison.Ordinal)) {
                    continue;
                }

                return index == entries.Count - 1 ? levelSelectSceneId : entries[index + 1].SceneId;
            }

            return levelSelectSceneId;
        }

        /// <summary>
        /// Creates one uninitialized session-state machine used by runtime and unit tests.
        /// </summary>
        /// <returns>Uninitialized Tilt Trial state machine with registered states.</returns>
        public static FiniteStateMachine<TiltTrialSessionState> CreateStateMachine() {
            FiniteStateMachine<TiltTrialSessionState> machine = new FiniteStateMachine<TiltTrialSessionState>();
            machine.RegisterState(TiltTrialSessionState.Playing, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Results, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Failed, new FiniteStateDefinition<TiltTrialSessionState>());
            return machine;
        }
    }
}
