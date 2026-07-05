# DS Bottom Screen Lilac Clear Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the shared DS bottom-screen scaffold clear with the exact demo-disc menu lilac color on DS/3DS bottom screens.

**Architecture:** Keep the change at the shared `NintendoDsRenderingSceneScaffoldFactory` seam that already owns the bottom-screen camera and control strip. Lock the exact RGBA contract with one source-audit test so render and physics DS scenes inherit the same bottom-screen clear automatically.

**Tech Stack:** C#, xUnit source-audit tests, city scene scaffold generation, 3DS build pipeline

---

### Task 1: Lock The Lilac Clear Contract In Source Tests

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityNintendoDsBottomScreenControlsSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Write the failing test**

```csharp
/// <summary>
/// Ensures the shared DS scaffold bottom-screen camera clears with the exact demo-disc lilac background color.
/// </summary>
[Fact]
public void City_ds_scaffold_source_uses_demo_disc_lilac_bottom_screen_clear() {
    string sourcePath = @"C:\dev\helprojs\city\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs";
    Assert.True(File.Exists(sourcePath), $"Expected source file '{sourcePath}' to exist.");

    string source = File.ReadAllText(sourcePath);

    Assert.Contains("new float4(30f / 255f, 17f / 255f, 41f / 255f, 1f)", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityNintendoDsBottomScreenControlsSourceTests.City_ds_scaffold_source_uses_demo_disc_lilac_bottom_screen_clear" 2>&1 | Out-String`

Expected: `FAIL` because `NintendoDsRenderingSceneScaffoldFactory.cs` still contains `new float4(0f, 0f, 0f, 1f)`.

- [ ] **Step 3: Commit**

```bash
git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityNintendoDsBottomScreenControlsSourceTests.cs
git -C C:\dev\helworks\helengine commit -m "Add DS bottom screen lilac clear audit"
```

### Task 2: Apply The Shared Lilac Clear In The DS Scaffold

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`
- Verify: `C:\dev\helworks\helengine\artifacts\build-platform.ps1`
- Launch: `C:\dev\helworks\helengine-3ds\scripts\launch_in_emulator.ps1`

- [ ] **Step 1: Write the minimal implementation**

Replace the current bottom-screen clear color inside `CreateBottomScreenCameraEntity()`:

```csharp
ClearSettings = new CameraClearSettings(
    true,
    new float4(30f / 255f, 17f / 255f, 41f / 255f, 1f),
    true,
    1f,
    false,
    0),
```

- [ ] **Step 2: Run the source-audit test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityNintendoDsBottomScreenControlsSourceTests" 2>&1 | Out-String`

Expected: `PASS` with the DS bottom-screen controls source-audit suite green.

- [ ] **Step 3: Rebuild the 3DS artifact**

Run: `rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\artifacts\build-platform.ps1 -Project C:\dev\helprojs\city\project.heproj -Platform 3ds -Output C:\dev\helprojs\city\3ds-build 2>&1 | Out-String`

Expected: `Build completed for platform '3ds': C:\dev\helprojs\city\3ds-build`

- [ ] **Step 4: Relaunch Azahar on the rebuilt artifact**

Run: `rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine-3ds\scripts\launch_in_emulator.ps1 -ArtifactPath C:\dev\helprojs\city\3ds-build\helengine_3ds.3dsx -EmulatorPath C:\dev\helworks\emus\azahar-windows-msvc-2125.1.1\azahar.exe 2>&1 | Out-String`

Expected: output includes `ARTIFACT=C:\dev\helprojs\city\3ds-build\helengine_3ds.3dsx` and a fresh `PROCESS_ID=...`

- [ ] **Step 5: Commit**

```bash
git -C C:\dev\helprojs\city add assets/codebase/rendering.tools/NintendoDsRenderingSceneScaffoldFactory.cs
git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityNintendoDsBottomScreenControlsSourceTests.cs
git -C C:\dev\helprojs\city commit -m "Set DS bottom screen clear to demo disc lilac"
```
