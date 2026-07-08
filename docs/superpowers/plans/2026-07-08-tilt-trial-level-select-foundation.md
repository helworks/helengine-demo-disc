# Tilt Trial Level Select Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn Tilt Trial into a level-based flow with a dedicated selector scene, five scaffolded gameplay levels, reusable per-level settings, and FSM-backed finish/fail session flow.

**Architecture:** Keep the logical `tilt_trial` entry id as the front-door scene and repurpose `assets/scenes/games/tilt_trial.helen` into the new level selector. Add five generated Tilt Trial gameplay scenes under new scene ids, a small ordered level catalog shared by selector and session flow, a reusable `TiltTrialLevelSettingsComponent` on every gameplay level, and a `TiltTrialSessionComponent` that uses `FiniteStateMachine<TState>` for `Playing`, `Results`, and `Failed`.

**Tech Stack:** C#, HelEngine generated scene factories, existing city menu/runtime components, helengine `FiniteStateMachine<TState>`, xUnit, editor command regeneration, Windows build pipeline

---

### Task 1: Add Failing Level Catalog And Settings Tests

**Files:**
- Create: `C:\dev\helprojs\city\tests\gameplay.tests\TiltTrialLevelCatalogTests.cs`
- Create: `C:\dev\helprojs\city\tests\gameplay.tests\TiltTrialLevelSettingsComponentTests.cs`
- Verify against: `C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj`

- [ ] **Step 1: Write the failing level catalog tests**

Create `TiltTrialLevelCatalogTests.cs` with direct behavioral coverage for the ordered five-level catalog that does not exist yet.

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies the shared Tilt Trial level catalog stays complete and deterministic.
    /// </summary>
    public sealed class TiltTrialLevelCatalogTests {
        /// <summary>
        /// Ensures the selector/session catalog exposes exactly five ordered level entries.
        /// </summary>
        [Fact]
        public void Catalog_returns_exactly_five_ordered_levels() {
            IReadOnlyList<city.game.TiltTrialLevelCatalogEntry> entries = city.game.TiltTrialLevelCatalog.CreateEntries();

            Assert.Equal(5, entries.Count);
            Assert.Equal("tilt-trial-01", entries[0].LevelId);
            Assert.Equal("tilt-trial-05", entries[4].LevelId);
        }

        /// <summary>
        /// Ensures every level entry carries the metadata required by the selector and session controller.
        /// </summary>
        [Fact]
        public void Catalog_entries_expose_scene_name_timer_medals_and_optional_preview() {
            foreach (city.game.TiltTrialLevelCatalogEntry entry in city.game.TiltTrialLevelCatalog.CreateEntries()) {
                Assert.False(string.IsNullOrWhiteSpace(entry.LevelId));
                Assert.False(string.IsNullOrWhiteSpace(entry.DisplayName));
                Assert.False(string.IsNullOrWhiteSpace(entry.SceneId));
                Assert.True(entry.StartTimeSeconds > 0f);
                Assert.True(entry.GoldTimeSeconds > 0f);
                Assert.True(entry.SilverTimeSeconds >= entry.GoldTimeSeconds);
                Assert.True(entry.BronzeTimeSeconds >= entry.SilverTimeSeconds);
            }
        }
    }
}
```

- [ ] **Step 2: Write the failing settings-component validation tests**

Create `TiltTrialLevelSettingsComponentTests.cs` so the new per-level data component fails fast on bad configuration.

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies per-level Tilt Trial metadata rejects invalid authoring.
    /// </summary>
    public sealed class TiltTrialLevelSettingsComponentTests {
        [Fact]
        public void Validate_throws_when_scene_id_is_missing() {
            city.game.TiltTrialLevelSettingsComponent component = new city.game.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = string.Empty,
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 20f,
                SilverTimeSeconds = 35f,
                BronzeTimeSeconds = 50f
            };

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => component.Validate());
            Assert.Contains("scene id", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Validate_throws_when_medal_times_are_not_ascending() {
            city.game.TiltTrialLevelSettingsComponent component = new city.game.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = "scenes/games/tilt_trial_level_01.helen",
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 40f,
                SilverTimeSeconds = 30f,
                BronzeTimeSeconds = 20f
            };

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => component.Validate());
            Assert.Contains("medal", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
```

- [ ] **Step 3: Run the new tests to verify they fail**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialLevelCatalogTests|TiltTrialLevelSettingsComponentTests" -v minimal
```

Expected: `FAIL` because `TiltTrialLevelCatalogEntry`, `TiltTrialLevelCatalog`, `TiltTrialLevelSettingsComponent`, and `Validate()` do not exist yet.

- [ ] **Step 4: Commit the failing-test checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add tests/gameplay.tests/TiltTrialLevelCatalogTests.cs tests/gameplay.tests/TiltTrialLevelSettingsComponentTests.cs
rtk git -C C:\dev\helprojs\city commit -m "test: cover tilt trial level metadata foundation"
```

### Task 2: Implement The Shared Level Catalog And Settings Component

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialLevelCatalogEntry.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialLevelCatalog.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialLevelSettingsComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneCatalog.cs`

- [ ] **Step 1: Add the immutable catalog entry type**

Create `TiltTrialLevelCatalogEntry.cs`.

```csharp
namespace city.game {
    /// <summary>
    /// Stores one ordered Tilt Trial level entry shared by selector and gameplay progression.
    /// </summary>
    public sealed class TiltTrialLevelCatalogEntry {
        public TiltTrialLevelCatalogEntry(
            string levelId,
            string displayName,
            string sceneId,
            float startTimeSeconds,
            float goldTimeSeconds,
            float silverTimeSeconds,
            float bronzeTimeSeconds,
            string previewTexturePath) {
            if (string.IsNullOrWhiteSpace(levelId)) {
                throw new ArgumentException("Level id must be provided.", nameof(levelId));
            }
            if (string.IsNullOrWhiteSpace(displayName)) {
                throw new ArgumentException("Display name must be provided.", nameof(displayName));
            }
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            LevelId = levelId;
            DisplayName = displayName;
            SceneId = sceneId;
            StartTimeSeconds = startTimeSeconds;
            GoldTimeSeconds = goldTimeSeconds;
            SilverTimeSeconds = silverTimeSeconds;
            BronzeTimeSeconds = bronzeTimeSeconds;
            PreviewTexturePath = previewTexturePath ?? string.Empty;
        }

        public string LevelId { get; }
        public string DisplayName { get; }
        public string SceneId { get; }
        public float StartTimeSeconds { get; }
        public float GoldTimeSeconds { get; }
        public float SilverTimeSeconds { get; }
        public float BronzeTimeSeconds { get; }
        public string PreviewTexturePath { get; }
    }
}
```

- [ ] **Step 2: Add the five-level ordered catalog**

Create `TiltTrialLevelCatalog.cs` and update `GameSceneCatalog.cs` so the new selector scene keeps the current `tilt_trial` path while levels 1-5 use dedicated scene ids.

```csharp
namespace city.game {
    /// <summary>
    /// Exposes the canonical Tilt Trial level order used by selector and Next progression.
    /// </summary>
    public static class TiltTrialLevelCatalog {
        public static IReadOnlyList<TiltTrialLevelCatalogEntry> CreateEntries() {
            return [
                new TiltTrialLevelCatalogEntry("tilt-trial-01", "Level 1", city.game.tools.GameSceneCatalog.TiltTrialLevel01SceneId, 99f, 18f, 28f, 40f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-02", "Level 2", city.game.tools.GameSceneCatalog.TiltTrialLevel02SceneId, 99f, 20f, 31f, 44f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-03", "Level 3", city.game.tools.GameSceneCatalog.TiltTrialLevel03SceneId, 99f, 23f, 35f, 48f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-04", "Level 4", city.game.tools.GameSceneCatalog.TiltTrialLevel04SceneId, 99f, 25f, 38f, 52f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-05", "Level 5", city.game.tools.GameSceneCatalog.TiltTrialLevel05SceneId, 99f, 27f, 41f, 56f, string.Empty),
            ];
        }
    }
}
```

Update `GameSceneCatalog.cs` to define:

```csharp
public const string TiltTrialSceneId = "scenes/games/tilt_trial.helen";
public const string TiltTrialLevel01SceneId = "scenes/games/tilt_trial_level_01.helen";
public const string TiltTrialLevel02SceneId = "scenes/games/tilt_trial_level_02.helen";
public const string TiltTrialLevel03SceneId = "scenes/games/tilt_trial_level_03.helen";
public const string TiltTrialLevel04SceneId = "scenes/games/tilt_trial_level_04.helen";
public const string TiltTrialLevel05SceneId = "scenes/games/tilt_trial_level_05.helen";
```

and return all six scene ids from `GetSceneIds()`.

- [ ] **Step 3: Add the reusable per-level settings component**

Create `TiltTrialLevelSettingsComponent.cs`.

```csharp
namespace city.game {
    /// <summary>
    /// Stores authored per-level Tilt Trial metadata used by timer, medals, and next-scene flow.
    /// </summary>
    public sealed class TiltTrialLevelSettingsComponent : Component {
        public string LevelId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SceneId { get; set; } = string.Empty;
        public float StartTimeSeconds { get; set; } = 99f;
        public float GoldTimeSeconds { get; set; } = 20f;
        public float SilverTimeSeconds { get; set; } = 35f;
        public float BronzeTimeSeconds { get; set; } = 50f;
        public string PreviewTexturePath { get; set; } = string.Empty;

        public void Validate() {
            if (string.IsNullOrWhiteSpace(LevelId)) {
                throw new InvalidOperationException("Tilt Trial level settings require a level id.");
            }
            if (string.IsNullOrWhiteSpace(DisplayName)) {
                throw new InvalidOperationException("Tilt Trial level settings require a display name.");
            }
            if (string.IsNullOrWhiteSpace(SceneId)) {
                throw new InvalidOperationException("Tilt Trial level settings require a scene id.");
            }
            if (StartTimeSeconds <= 0f) {
                throw new InvalidOperationException("Tilt Trial level settings require a positive start time.");
            }
            if (GoldTimeSeconds <= 0f || SilverTimeSeconds < GoldTimeSeconds || BronzeTimeSeconds < SilverTimeSeconds) {
                throw new InvalidOperationException("Tilt Trial level settings require ascending gold, silver, and bronze medal times.");
            }
        }
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialLevelCatalogTests|TiltTrialLevelSettingsComponentTests" -v minimal
```

Expected: `PASS`

- [ ] **Step 5: Commit the metadata implementation checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/game/TiltTrialLevelCatalogEntry.cs assets/codebase/game/TiltTrialLevelCatalog.cs assets/codebase/game/TiltTrialLevelSettingsComponent.cs assets/codebase/game.tools/GameSceneCatalog.cs
rtk git -C C:\dev\helprojs\city commit -m "feat: add tilt trial level metadata foundation"
```

### Task 3: Add Failing Session-State Tests For Timer, Results, And Next Progression

**Files:**
- Create: `C:\dev\helprojs\city\tests\gameplay.tests\TiltTrialSessionComponentTests.cs`

- [ ] **Step 1: Write the failing FSM/session tests**

Create `TiltTrialSessionComponentTests.cs` to lock down the gameplay-state rules before authoring the runtime controller.

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial session controller drives timeout and completion flow deterministically.
    /// </summary>
    public sealed class TiltTrialSessionComponentTests {
        [Fact]
        public void Resolve_medal_returns_gold_for_fastest_clear() {
            city.game.TiltTrialLevelSettingsComponent settings = new city.game.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = "scenes/games/tilt_trial_level_01.helen",
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 20f,
                SilverTimeSeconds = 35f,
                BronzeTimeSeconds = 50f
            };

            city.game.TiltTrialMedal medal = city.game.TiltTrialSessionComponent.ResolveMedal(settings, 19.5f);
            Assert.Equal(city.game.TiltTrialMedal.Gold, medal);
        }

        [Fact]
        public void Resolve_next_scene_id_returns_level_select_when_current_level_is_last() {
            string nextSceneId = city.game.TiltTrialSessionComponent.ResolveNextSceneId(
                "tilt-trial-05",
                city.game.tools.GameSceneCatalog.TiltTrialSceneId);

            Assert.Equal(city.game.tools.GameSceneCatalog.TiltTrialSceneId, nextSceneId);
        }

        [Fact]
        public void Build_state_machine_transitions_from_playing_to_failed_when_timeout_occurs() {
            helengine.FiniteStateMachine<city.game.TiltTrialSessionState> machine = city.game.TiltTrialSessionComponent.CreateStateMachine();

            machine.Initialize(city.game.TiltTrialSessionState.Playing);
            bool changed = machine.TryChangeState(city.game.TiltTrialSessionState.Failed);

            Assert.True(changed);
            Assert.Equal(city.game.TiltTrialSessionState.Failed, machine.CurrentState);
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialSessionComponentTests" -v minimal
```

Expected: `FAIL` because `TiltTrialSessionComponent`, `TiltTrialSessionState`, `TiltTrialMedal`, and `CreateStateMachine()` do not exist yet.

- [ ] **Step 3: Commit the failing session-test checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add tests/gameplay.tests/TiltTrialSessionComponentTests.cs
rtk git -C C:\dev\helprojs\city commit -m "test: cover tilt trial session state flow"
```

### Task 4: Implement The FSM-Backed Session Controller

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialSessionState.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialMedal.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialSessionComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\game\DemoTiltBallResetComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\game\DemoTiltSpeedTextComponent.cs`

- [ ] **Step 1: Add the gameplay session enums**

Create the two enums used by the session controller.

```csharp
namespace city.game {
    public enum TiltTrialSessionState {
        Playing = 0,
        Results = 1,
        Failed = 2
    }

    public enum TiltTrialMedal {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3
    }
}
```

- [ ] **Step 2: Add the FSM-backed session component**

Create `TiltTrialSessionComponent.cs` with state-machine setup and testable helpers first.

```csharp
namespace city.game {
    /// <summary>
    /// Owns Tilt Trial timer state, finish/fail transitions, and Retry/Next/Level Select scene actions.
    /// </summary>
    public sealed class TiltTrialSessionComponent : UpdateComponent {
        readonly helengine.FiniteStateMachine<TiltTrialSessionState> SessionStateMachine;

        public TiltTrialSessionComponent() {
            SessionStateMachine = CreateStateMachine();
        }

        public static TiltTrialMedal ResolveMedal(TiltTrialLevelSettingsComponent settings, float clearTimeSeconds) {
            settings.Validate();
            if (clearTimeSeconds <= settings.GoldTimeSeconds) {
                return TiltTrialMedal.Gold;
            }
            if (clearTimeSeconds <= settings.SilverTimeSeconds) {
                return TiltTrialMedal.Silver;
            }
            if (clearTimeSeconds <= settings.BronzeTimeSeconds) {
                return TiltTrialMedal.Bronze;
            }

            return TiltTrialMedal.None;
        }

        public static string ResolveNextSceneId(string currentLevelId, string levelSelectSceneId) {
            IReadOnlyList<TiltTrialLevelCatalogEntry> entries = TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < entries.Count; index++) {
                if (!string.Equals(entries[index].LevelId, currentLevelId, StringComparison.Ordinal)) {
                    continue;
                }

                return index == entries.Count - 1 ? levelSelectSceneId : entries[index + 1].SceneId;
            }

            return levelSelectSceneId;
        }

        public static helengine.FiniteStateMachine<TiltTrialSessionState> CreateStateMachine() {
            helengine.FiniteStateMachine<TiltTrialSessionState> machine = new helengine.FiniteStateMachine<TiltTrialSessionState>();
            machine.RegisterState(TiltTrialSessionState.Playing, new helengine.FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Results, new helengine.FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Failed, new helengine.FiniteStateDefinition<TiltTrialSessionState>());
            return machine;
        }
    }
}
```

- [ ] **Step 3: Add small hooks needed by the session controller**

Adjust `DemoTiltBallResetComponent` and `DemoTiltSpeedTextComponent` so the session controller can suppress gameplay-side updates during overlays instead of forking those components later.

Apply this minimal shape:

```csharp
public bool UpdatesAreSuppressed { get; set; }

public override void Update() {
    base.Update();
    if (UpdatesAreSuppressed) {
        return;
    }
    // existing behavior
}
```

- [ ] **Step 4: Run the session tests to verify they pass**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialSessionComponentTests" -v minimal
```

Expected: `PASS`

- [ ] **Step 5: Commit the session-flow implementation checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/game/TiltTrialSessionState.cs assets/codebase/game/TiltTrialMedal.cs assets/codebase/game/TiltTrialSessionComponent.cs assets/codebase/game/DemoTiltBallResetComponent.cs assets/codebase/game/DemoTiltSpeedTextComponent.cs
rtk git -C C:\dev\helprojs\city commit -m "feat: add tilt trial session state flow"
```

### Task 5: Add Failing Source Tests For Selector Scene And Multi-Scene Generation

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityGameSceneSourceTests.cs`
- Create: `C:\dev\helprojs\city\tests\gameplay.tests\TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Extend the existing source audit in helengine**

Add one new test to `CityGameSceneSourceTests.cs` so the cross-repo source audit checks that `tilt_trial` is now the selector and that five level scene ids exist.

```csharp
    /// <summary>
    /// Ensures Tilt Trial now routes through a dedicated selector scene with five generated gameplay levels behind it.
    /// </summary>
    [Fact]
    public void City_tilt_trial_source_exports_selector_scene_and_five_level_scene_ids() {
        string sourcePath = @"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneCatalog.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("public const string TiltTrialSceneId = \"scenes/games/tilt_trial.helen\";", source, StringComparison.Ordinal);
        Assert.Contains("TiltTrialLevel01SceneId", source, StringComparison.Ordinal);
        Assert.Contains("TiltTrialLevel05SceneId", source, StringComparison.Ordinal);
    }
```

- [ ] **Step 2: Add city-local source tests for generator/factory wiring**

Create `TiltTrialSceneGenerationSourceTests.cs`.

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies the generated Tilt Trial scene source now emits selector plus scaffolded gameplay levels.
    /// </summary>
    public sealed class TiltTrialSceneGenerationSourceTests {
        [Fact]
        public void Game_scene_generator_writes_selector_and_all_five_levels() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("CreateTiltTrialLevelSelectScene()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevelScenes()", source, StringComparison.Ordinal);
            Assert.Contains("sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_authors_level_settings_and_session_components() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("new city.game.TiltTrialLevelSettingsComponent", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialSessionComponent", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialLevelSelectComponent", source, StringComparison.Ordinal);
        }
    }
}
```

- [ ] **Step 3: Run the tests to verify they fail**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_tilt_trial_source_exports_selector_scene_and_five_level_scene_ids" -v minimal
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialSceneGenerationSourceTests" -v minimal
```

Expected: `FAIL` because the generator/factory still only emits one gameplay scene and none of the selector/session types exist.

- [ ] **Step 4: Commit the failing source-test checkpoint**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityGameSceneSourceTests.cs
rtk git -C C:\dev\helprojs\city add tests/gameplay.tests/TiltTrialSceneGenerationSourceTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "test: cover tilt trial selector scene ids"
rtk git -C C:\dev\helprojs\city commit -m "test: cover tilt trial scene generation layout"
```

### Task 6: Author The Selector Scene And Five Scaffolded Gameplay Levels

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\game\TiltTrialLevelSelectComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneGenerator.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\menu\DemoDiscSceneCatalog.cs`

- [ ] **Step 1: Add the selector controller component**

Create `TiltTrialLevelSelectComponent.cs` as the dedicated selector-scene runtime that owns current index, preview/details binding, and scene loading.

```csharp
namespace city.game {
    /// <summary>
    /// Drives the dedicated Tilt Trial level-select scene.
    /// </summary>
    public sealed class TiltTrialLevelSelectComponent : UpdateComponent {
        public int SelectedIndex { get; private set; }

        public override void Update() {
            InputSystem inputSystem = Core.Instance.Input;
            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = TiltTrialLevelCatalog.CreateEntries();
            if (inputSystem.WasKeyPressed(Keys.Up)) {
                SelectedIndex = SelectedIndex <= 0 ? levels.Count - 1 : SelectedIndex - 1;
            } else if (inputSystem.WasKeyPressed(Keys.Down)) {
                SelectedIndex = SelectedIndex >= levels.Count - 1 ? 0 : SelectedIndex + 1;
            } else if (Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept)) {
                Core.Instance.SceneManager.LoadScene(levels[SelectedIndex].SceneId, SceneLoadMode.Single);
            }

            ApplySelectionToUi(levels[SelectedIndex]);
        }

        void ApplySelectionToUi(TiltTrialLevelCatalogEntry selectedLevel) {
            if (selectedLevel == null) {
                throw new ArgumentNullException(nameof(selectedLevel));
            }

            ApplyLevelNameText(selectedLevel.DisplayName);
            ApplyLevelTimerText(selectedLevel.StartTimeSeconds);
            ApplyLevelMedalText(selectedLevel.GoldTimeSeconds, selectedLevel.SilverTimeSeconds, selectedLevel.BronzeTimeSeconds);
            ApplyPreviewState(selectedLevel.PreviewTexturePath);
        }
    }
}
```

Keep the first pass narrow:

```text
- Up/Down or D-pad changes selection
- Accept loads the selected level scene
- Escape/Return uses DemoDiscReturnToMenuComponent to go back to the demo-disc menu
- missing preview path keeps a fallback preview panel active
```

- [ ] **Step 2: Refactor the game-scene factory into selector + reusable gameplay-level builders**

Update `GameSceneFactory.cs` to:

```csharp
public GeneratedAuthoringSceneDefinition CreateTiltTrialLevelSelectScene()
public IReadOnlyList<GeneratedAuthoringSceneDefinition> CreateTiltTrialLevelScenes()
GeneratedAuthoringSceneDefinition CreateTiltTrialGameplayScene(city.game.TiltTrialLevelCatalogEntry levelEntry)
```

Implementation requirements:

```text
- keep GameSceneCatalog.TiltTrialSceneId as the selector scene id
- create five gameplay scenes under TiltTrialLevel01..05 scene ids
- attach TiltTrialLevelSettingsComponent to every gameplay scene root
- attach TiltTrialSessionComponent to every gameplay scene UI/session root
- keep current stage/camera/ball generation as the initial scaffold for all five levels
- leave the current main-menu game entry label as "Tilt Trial"
```

- [ ] **Step 3: Update the scene generator to write six scenes instead of one**

Update `GameSceneGenerator.cs` to write the selector scene first and then iterate all five gameplay definitions.

```csharp
GeneratedAuthoringSceneDefinition tiltTrialLevelSelectScene = factory.CreateTiltTrialLevelSelectScene();
sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);

IReadOnlyList<GeneratedAuthoringSceneDefinition> tiltTrialLevelScenes = factory.CreateTiltTrialLevelScenes();
for (int index = 0; index < tiltTrialLevelScenes.Count; index++) {
    sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelScenes[index]);
}
```

- [ ] **Step 4: Keep the menu entry stable**

Only update `DemoDiscSceneCatalog.cs` if needed for clarity, but preserve the logical scene target:

```csharp
new DemoDiscGameSceneEntry(
    "games-tilt-trial",
    "Tilt Trial",
    "tilt_trial")
```

The goal is to avoid touching scene-map routing when the existing logical id can keep working.

- [ ] **Step 5: Run the source tests to verify they pass**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_tilt_trial_source_exports_selector_scene_and_five_level_scene_ids" -v minimal
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialSceneGenerationSourceTests" -v minimal
```

Expected: `PASS`

- [ ] **Step 6: Commit the selector/generation checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/game/TiltTrialLevelSelectComponent.cs assets/codebase/game.tools/GameSceneFactory.cs assets/codebase/game.tools/GameSceneGenerator.cs assets/codebase/menu/DemoDiscSceneCatalog.cs
rtk git -C C:\dev\helprojs\city commit -m "feat: author tilt trial selector and scaffolded levels"
```

### Task 7: Regenerate Scenes And Verify Authored Outputs

**Files:**
- Verify output: `C:\dev\helprojs\city\assets\scenes\games\tilt_trial.helen`
- Verify output: `C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_01.helen`
- Verify output: `C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_05.helen`

- [ ] **Step 1: Regenerate the game scenes through the editor command**

Run:

```bash
rtk proxy dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-game-scenes
```

Expected: command exits successfully and rewrites:

```text
C:\dev\helprojs\city\assets\scenes\games\tilt_trial.helen
C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_01.helen
C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_02.helen
C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_03.helen
C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_04.helen
C:\dev\helprojs\city\assets\scenes\games\tilt_trial_level_05.helen
```

- [ ] **Step 2: Verify the scene files exist**

Run:

```bash
rtk powershell -NoProfile -Command "Get-ChildItem 'C:\dev\helprojs\city\assets\scenes\games' | Where-Object { $_.Name -like 'tilt_trial*' } | Select-Object -ExpandProperty Name"
```

Expected:

```text
tilt_trial.helen
tilt_trial_level_01.helen
tilt_trial_level_02.helen
tilt_trial_level_03.helen
tilt_trial_level_04.helen
tilt_trial_level_05.helen
```

- [ ] **Step 3: Commit the regenerated authored scenes**

```bash
rtk git -C C:\dev\helprojs\city add assets/scenes/games/tilt_trial.helen assets/scenes/games/tilt_trial_level_01.helen assets/scenes/games/tilt_trial_level_02.helen assets/scenes/games/tilt_trial_level_03.helen assets/scenes/games/tilt_trial_level_04.helen assets/scenes/games/tilt_trial_level_05.helen
rtk git -C C:\dev\helprojs\city commit -m "feat: regenerate tilt trial selector and level scenes"
```

### Task 8: Full Verification And Windows Runtime Smoke

**Files:**
- Verify build output: `C:\dev\helprojs\city\windows-build\helengine_windows.exe`
- Verify log: `C:\dev\helprojs\city\windows-build\helengine_windows.startup.log`

- [ ] **Step 1: Run the focused city tests**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "TiltTrial" -v minimal
```

Expected: `PASS`

- [ ] **Step 2: Run the focused helengine source audit**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_tilt_trial" -v minimal
```

Expected: `PASS`

- [ ] **Step 3: Rebuild the Windows package**

Run:

```bash
rtk powershell -NoProfile -ExecutionPolicy Bypass -Command "& 'C:\dev\helworks\helengine\artifacts\build-platform.ps1' -Project 'C:\dev\helprojs\city\project.heproj' -Platform 'windows' -Output 'C:\dev\helprojs\city\windows-build'"
```

Expected: command exits successfully and refreshes `C:\dev\helprojs\city\windows-build\helengine_windows.exe`

- [ ] **Step 4: Launch the Windows build**

Run:

```bash
rtk powershell -NoProfile -Command "Start-Process -FilePath 'C:\dev\helprojs\city\windows-build\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\city\windows-build'"
```

Expected: the Windows player launches into the demo-disc flow and `Tilt Trial` opens the new level selector.

- [ ] **Step 5: Verify the startup log stays clean**

Run:

```bash
rtk powershell -NoProfile -Command "Start-Sleep -Seconds 5; Get-Content 'C:\dev\helprojs\city\windows-build\helengine_windows.startup.log' -Tail 60"
```

Expected:

```text
- no 'Fatal host/engine exception'
- scene loading reaches first-frame completion
```

- [ ] **Step 6: Perform manual flow verification**

Manual checklist:

```text
- main demo-disc menu still shows one Tilt Trial entry
- selecting Tilt Trial opens the dedicated level selector
- the selector shows 5 levels immediately
- missing preview art falls back to the preview-fallback panel instead of blank/error
- selecting a level loads the matching gameplay scene
- timer starts from that level's configured value
- timeout opens fail UI with Retry and Level Select
- finish opens results UI with Retry, Next, and Level Select
- the fifth level's Next action returns to the selector
```

- [ ] **Step 7: Commit the final verification-backed implementation**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/game assets/codebase/game.tools assets/codebase/menu assets/scenes/games tests/gameplay.tests docs/superpowers/plans/2026-07-08-tilt-trial-level-select-foundation.md
rtk git -C C:\dev\helprojs\city commit -m "feat: add tilt trial level select foundation"
```
