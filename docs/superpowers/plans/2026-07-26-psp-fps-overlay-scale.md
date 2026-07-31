# PSP FPS Overlay Scale Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Render every generated render and physics FPS overlay at half its usual size on PSP only.

**Architecture:** Preserve the `2f` authored scale in scene factories. Use the existing persisted platform-override mechanism to apply `FPSComponent.FontScale = 1f` for platform id `psp`, so non-PSP generated scene data remains unchanged.

**Tech Stack:** C#, Helengine editor scene generation, xUnit, PSP platform packaging.

---

### Task 1: Add a focused PSP FPS-override source contract

**Files:**
- Modify: `assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs`
- Modify: `assets/codebase/rendering.tools/ConsoleCameraLightInstructionsSceneAttachmentService.cs`
- Test: `assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs`

- [ ] **Step 1: Write the failing source assertion**

Add a fact that requires the shared FPS override authoring path to use platform id `psp`, `nameof(FPSComponent.FontScale)`, and `1f`.

```csharp
Assert.Contains("PspPlatformId", source, StringComparison.Ordinal);
Assert.Contains("nameof(FPSComponent.FontScale)", source, StringComparison.Ordinal);
Assert.Contains("PspFpsFontScale = 1f", source, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the focused test to verify it fails**

Run:

```powershell
rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj --no-restore --filter FullyQualifiedName~FpsFontScaleSourceTests
```

Expected: failure because no PSP FPS scale override is authored.

- [ ] **Step 3: Implement the platform override in the shared FPS authoring path**

Use the existing `PlatformEditingServiceValue.MarkPropertyOverride` and `PersistPlatformOverride` calls, with the PSP platform id and a clone of the common FPS component whose scale is `1f`.

```csharp
FPSComponent overrideComponent = new FPSComponent();
overrideComponent.FontScale = PspFpsFontScale;
PlatformEditingServiceValue.MarkPropertyOverride(commonFpsComponent, saveComponent, PspPlatformId, nameof(FPSComponent.FontScale));
PlatformEditingServiceValue.PersistPlatformOverride(commonFpsComponent, overrideComponent, saveComponent, PspPlatformId);
```

- [ ] **Step 4: Run the focused test to verify it passes**

Run the command from Step 2.

Expected: all `FpsFontScaleSourceTests` pass.

- [ ] **Step 5: Commit the contract and generator implementation**

```powershell
git -C C:\dev\helprojs\demodisc add -- assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs assets/codebase/rendering.tools/ConsoleCameraLightInstructionsSceneAttachmentService.cs
git -C C:\dev\helprojs\demodisc commit -m "Scale PSP FPS overlays to half size"
```

### Task 2: Regenerate and package PSP scene data

**Files:**
- Modify: generated render and physics `.helen` assets only through their registered scene-generation command
- Test: PSP package artifact

- [ ] **Step 1: Regenerate scenes using the registered command**

Run:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\run-editor-command.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -CommandId rendering.generate-scenes
```

Expected: render and physics scene assets are regenerated with PSP FPS overrides.

- [ ] **Step 2: Build PSP through the canonical script**

Run:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform psp -Output C:\dev\helprojs\output\psp-fps-scale
```

Expected: a fresh `PSP\GAME\HELENGINE\EBOOT.PBP` is produced successfully.

- [ ] **Step 3: Commit only regenerated scene assets that contain the FPS overrides**

```powershell
git -C C:\dev\helprojs\demodisc add -- assets/scenes/rendering assets/scenes/physics
git -C C:\dev\helprojs\demodisc commit -m "Persist PSP FPS overlay overrides"
```
