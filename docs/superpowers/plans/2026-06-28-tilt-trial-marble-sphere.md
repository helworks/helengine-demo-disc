# Tilt Trial Marble Sphere Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Give the Tilt Trial player sphere a dedicated project-owned white marble material with gray veining, without changing any other scene materials.

**Architecture:** Keep the change local to the existing generated Tilt Trial scene path. Add one dedicated marble texture and one dedicated marble material asset, then update only `CreatePlayerSphereEntity()` so the player sphere uses that material instead of `GeneratedStandardMaterial`.

**Tech Stack:** C#, HelEngine generated scene factory, project-owned `.bmp` texture source assets, project-owned `.hasset` material assets, xUnit source tests, Windows source-build pipeline

---

### Task 1: Add Source-Level Sphere Material Coverage

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityGameSceneSourceTests.cs`

- [ ] **Step 1: Write the failing source test**

Add one new xUnit test to `CityGameSceneSourceTests.cs` that asserts the Tilt Trial player sphere source now references the dedicated marble material path.

```csharp
    /// <summary>
    /// Ensures the Tilt Trial player sphere source uses the dedicated marble material asset.
    /// </summary>
    [Fact]
    public void City_tilt_trial_player_sphere_source_uses_marble_material() {
        string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("Materials.rendering.tilt_trial.PlayerSphereMarble", source, StringComparison.Ordinal);
    }
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj -v q --filter City_tilt_trial_player_sphere_source_uses_marble_material`

Expected: `FAIL` because `GameSceneFactory.cs` still contains `GeneratedStandardMaterial` for the player sphere.

- [ ] **Step 3: Commit the failing test checkpoint**

```bash
git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityGameSceneSourceTests.cs
git -C C:\dev\helworks\helengine commit -m "test: cover tilt trial marble sphere material"
```

### Task 2: Add The Tilt Trial Marble Assets

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\textures\rendering\tilt_trial\PlayerSphereMarble.bmp`
- Create: `C:\dev\helprojs\demodisc\assets\textures\rendering\tilt_trial\PlayerSphereMarble.bmp.hasset`
- Create: `C:\dev\helprojs\demodisc\assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset`

- [ ] **Step 1: Create the marble texture source**

Create `PlayerSphereMarble.bmp` as a small project-owned white marble texture with soft gray veining sized only for the Tilt Trial sphere.

Required visual direction:

```text
- white to off-white base
- soft medium-gray veining
- no colored swirls
- no heavy contrast that reads like cracked stone
- tileable enough to avoid obvious seams on the sphere
```

- [ ] **Step 2: Create the texture import metadata**

Create `PlayerSphereMarble.bmp.hasset` alongside the texture using the same import-file placement pattern already used by:

```text
C:\dev\helprojs\demodisc\assets\textures\rendering\textured_cube_grid\Cube00.bmp.hasset
```

The implementation should mirror the existing project texture-import pattern rather than inventing a new metadata shape.

- [ ] **Step 3: Create the dedicated marble material asset**

Create `assets/materials/rendering/tilt_trial/PlayerSphereMarble.hasset` using the same `ds-standard-textured` schema already used by:

```text
C:\dev\helprojs\demodisc\assets\materials\rendering\textured_cube_grid\Cube00.hasset
```

Material requirements:

```text
material id: Materials.rendering.tilt_trial.PlayerSphereMarble
shader/schema: ds-standard-textured
texture-relative-path: textures/rendering/tilt_trial/PlayerSphereMarble.bmp
double-sided: false
vertex-color-mode: multiply
base-color: #FFFFFFFF
lighting-mode: lit
```

- [ ] **Step 4: Verify the assets exist in the expected project paths**

Run: `Get-ChildItem C:\dev\helprojs\demodisc\assets\textures\rendering\tilt_trial, C:\dev\helprojs\demodisc\assets\materials\rendering\tilt_trial`

Expected: output includes:

```text
PlayerSphereMarble.bmp
PlayerSphereMarble.bmp.hasset
PlayerSphereMarble.hasset
```

- [ ] **Step 5: Commit the asset checkpoint**

```bash
git -C C:\dev\helprojs\demodisc add assets/textures/rendering/tilt_trial/PlayerSphereMarble.bmp assets/textures/rendering/tilt_trial/PlayerSphereMarble.bmp.hasset assets/materials/rendering/tilt_trial/PlayerSphereMarble.hasset
git -C C:\dev\helprojs\demodisc commit -m "feat: add tilt trial marble sphere assets"
```

### Task 3: Wire The Sphere To The Marble Material

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs`
- Verify against: `C:\dev\helprojs\demodisc\assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset`

- [ ] **Step 1: Replace the player sphere material assignment**

Update `CreatePlayerSphereEntity()` so the `MeshComponent` for `PlayerSphere` uses the dedicated marble material asset instead of the generated shared scene material.

Replace:

```csharp
            entity.AddComponent(new MeshComponent {
                Model = GeneratedSphereModel,
                Materials = new[] { GeneratedStandardMaterial },
                RenderOrder3D = 0
            });
```

With the project-owned marble assignment that follows the existing authored-asset material pattern used elsewhere in the project.

- [ ] **Step 2: Run the source test to verify it passes**

Run: `dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj -v q --filter City_tilt_trial_player_sphere_source_uses_marble_material`

Expected: `PASS`

- [ ] **Step 3: Run the broader Tilt Trial source coverage**

Run: `dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj -v q --filter CityGameSceneSourceTests`

Expected: `PASS`

- [ ] **Step 4: Commit the scene wiring checkpoint**

```bash
git -C C:\dev\helprojs\demodisc add assets/codebase/game.tools/GameSceneFactory.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: wire tilt trial sphere to marble material"
```

### Task 4: Rebuild Windows And Verify Runtime Output

**Files:**
- Verify output: `C:\dev\helprojs\demodisc\windows-build\helengine_windows.exe`
- Verify log: `C:\dev\helprojs\demodisc\windows-build\helengine_windows.startup.log`

- [ ] **Step 1: Rebuild the Windows package**

Run:

```bash
powershell -NoProfile -ExecutionPolicy Bypass -Command "& 'C:\dev\helworks\helengine\artifacts\build-platform.ps1' -Project 'C:\dev\helprojs\demodisc\project.heproj' -Platform 'windows' -Output 'C:\dev\helprojs\demodisc\windows-build'"
```

Expected: command exits successfully and refreshes `C:\dev\helprojs\demodisc\windows-build\helengine_windows.exe`

- [ ] **Step 2: Launch directly into Tilt Trial**

Run:

```bash
Start-Process -FilePath 'C:\dev\helprojs\demodisc\windows-build\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\demodisc\windows-build'
```

Expected: the Windows player opens and loads `tilt_trial`

- [ ] **Step 3: Verify startup succeeded**

Run:

```bash
Start-Sleep -Seconds 5
Get-Content 'C:\dev\helprojs\demodisc\windows-build\helengine_windows.startup.log' -Tail 40
```

Expected: log includes:

```text
[Host] Loading startup scene from runtime scene catalog entry 'tilt_trial'.
[Host] First frame completed EngineCore->Update().
```

Expected: log does not contain `Fatal host/engine exception`

- [ ] **Step 4: Perform manual visual verification**

Manual checklist:

```text
- the playable sphere renders as white marble with gray veining
- the stage and walls still look unchanged
- Tilt Trial still boots and runs
```

- [ ] **Step 5: Commit the final packaged-change checkpoint**

```bash
git -C C:\dev\helprojs\demodisc add docs/superpowers/specs/2026-06-28-tilt-trial-marble-sphere-design.md docs/superpowers/plans/2026-06-28-tilt-trial-marble-sphere.md
git -C C:\dev\helprojs\demodisc commit -m "docs: add tilt trial marble sphere design and plan"
```
