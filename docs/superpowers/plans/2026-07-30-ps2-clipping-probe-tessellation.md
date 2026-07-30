# PS2 Clipping-Probe Tessellation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable the existing 1.0-unit cook-time tessellation override on the deterministic clipping-probe cube for PS2 and PSP.

**Architecture:** Keep the generated cube, camera, material, and renderer unchanged. Flip the existing factory argument that controls component-level constrained-platform tessellation, update its source contracts, and let the normal cook regenerate the tessellated PS2 scene.

**Tech Stack:** C#, xUnit source contracts, Helengine editor cook pipeline, C++ PS2 runtime, PS2SDK, PCSX2, HelenUI OCR.

---

### Task 1: Enable Component Tessellation On The Probe Cube

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs:40-47`
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs:156-170`
- Modify: `assets/codebase/game.tools/GameSceneFactory.cs:1518-1529`

- [ ] **Step 1: Change the source contracts to require tessellation**

Update both assertions to require the existing factory argument to be `true`. Rename the focused test and its XML summary to describe one tessellated probe cube:

```csharp
/// <summary>
/// Ensures the render-test scene uses one constrained-platform tessellated cube for near-camera clipping diagnostics.
/// </summary>
[Fact]
public void Game_scene_factory_creates_one_tessellated_clipping_probe_cube() {
    string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

    Assert.Contains("CreateLevel01RenderOnlyCourseBoxEntity(\"ClipProbeCube\", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true)", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Verify the new contract is red against production source**

Run:

```powershell
rtk.exe rg -n -F 'CreateLevel01RenderOnlyCourseBoxEntity("ClipProbeCube", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true)' assets/codebase/game.tools/GameSceneFactory.cs
```

Expected: exit code 1 because the current source passes `false`.

- [ ] **Step 3: Enable the existing cook-time path**

Change the probe attachment and its XML summary to:

```csharp
/// <summary>
/// Creates the clipping probe root with exactly one constrained-platform tessellated 5-by-1-by-5 cube.
/// </summary>
EditorEntity CreateLevel01RenderOnlyStageRootEntity() {
    Entity entity = Core.Instance.EntityFactory.Create("Ps2ClippingProbe");
    entity.LayerMask = EditorLayerMasks.SceneObjects;
    entity.LocalPosition = float3.Zero;
    entity.LocalScale = float3.One;
    entity.LocalOrientation = float4.Identity;
    entity.AddChild(CreateLevel01RenderOnlyCourseBoxEntity("ClipProbeCube", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true));
    return RequireEditorEntity(entity, "single-cube clipping probe");
}
```

- [ ] **Step 4: Verify the focused source contracts are green**

Run:

```powershell
rtk.exe rg -n -F 'CreateLevel01RenderOnlyCourseBoxEntity("ClipProbeCube", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true)' assets/codebase/game.tools/GameSceneFactory.cs assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs
rtk.exe rg -n -F 'const double TiltTrialRenderTestTessellationMaxEdgeLength = 1d;' assets/codebase/game.tools/GameSceneFactory.cs
rtk.exe git diff --check
```

Expected: the enabled call appears in all three files, the 1.0-unit constant remains present, and `git diff --check` reports no errors.

### Task 2: Identify, Build, Launch, And Inspect B297

**Files:**
- Modify: `C:\dev\helworks\helengine-ps2\src\platform\ps2\Ps2BootHost.cpp:194`
- Build output: `C:\dev\helworks\builds\demodisc\ps2\B297-tessellated-clip-probe`

- [ ] **Step 1: Set the visible build identifier**

Change:

```cpp
constexpr const char* FrameTimingOverlayBuildNumber = "B297";
```

- [ ] **Step 2: Run the existing PS2 clipping contracts**

Run:

```powershell
rtk.exe dotnet test C:\dev\helworks\helengine-ps2\builder.tests\helengine.ps2.builder.tests.csproj --filter FullyQualifiedName~Ps2VuNearPlaneClippingSourceTests --no-build --no-restore --nologo
```

Expected: 7 tests pass.

- [ ] **Step 3: Build and deterministically wait for all PS2 artifacts**

Run the workspace build waiter with output `C:\dev\helworks\builds\demodisc\ps2\B297-tessellated-clip-probe`, requiring `game.iso`, `disc/SYSTEM.CNF`, and `disc/HELENGIN.ELF`, around the normal `build-platform.ps1` PS2 command.

Expected: exit code 0 and all three required files have fresh timestamps.

- [ ] **Step 4: Launch the exact B297 ISO**

Run:

```powershell
rtk.exe proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine-ps2\scripts\launch_in_emulator.ps1 -ArtifactPath C:\dev\helworks\builds\demodisc\ps2\B297-tessellated-clip-probe\game.iso
```

Expected: one PCSX2 instance launches the exact B297 ISO.

- [ ] **Step 5: OCR the running game through HelenUI**

Capture the launched PCSX2 window by exact handle with ScreenshotCli and analyze it with RecognitionCli using `C:\dev\helenui\pcsx2.json`.

Expected: HelenUI recognizes `Running Game`, reads `B297`, reports a non-N/A frame, and shows a triangle count greater than the untessellated baseline of 12.

- [ ] **Step 6: Request visual acceptance**

Ask the user to verify that the tessellated cube renders normally and to move the camera through it. Do not commit implementation changes until the visual result is accepted.
